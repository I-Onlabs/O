using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using AngryDogs.Data;

namespace AngryDogs.SaveSystem
{
    /// <summary>
    /// Centralised persistence manager responsible for JSON save/load with mobile safe fallbacks.
    /// Now includes cloud syncing for cross-device progress and offline fallback support.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SaveManager : MonoBehaviour
    {
        private const string SaveFileName = "angry_dogs_save.json";
        private const string KeyBindingPrefix = "AngryDogs_Key_";
        private const string CloudSaveKey = "angry_dogs_cloud_save";
        private const int MaxRetryAttempts = 3;
        private const float CloudSyncTimeout = 10f;

        [Header("Behaviour")]
        [SerializeField, Tooltip("Automatically load progress during Awake.")]
        private bool autoLoadOnAwake = true;
        [SerializeField, Tooltip("Encrypt save payload lightly to deter casual tampering.")]
        private bool obfuscatePayload = true;
        [SerializeField, Tooltip("Enable cloud syncing for cross-device progress.")]
        private bool enableCloudSync = true;
        [SerializeField, Tooltip("Sync interval in seconds (0 = manual only).")]
        private float cloudSyncInterval = 300f; // 5 minutes

        [Header("Cloud Settings")]
        [SerializeField, Tooltip("Cloud save service URL (use Unity Cloud Save or custom endpoint).")]
        private string cloudSaveUrl = "https://your-cloud-save-service.com/api/save";
        [SerializeField, Tooltip("API key for cloud save authentication.")]
        private string cloudApiKey = "";
        [SerializeField, Tooltip("Enable offline mode when cloud is unavailable.")]
        private bool enableOfflineMode = true;
        [SerializeField, Tooltip("Maximum retry attempts for cloud sync.")]
        private int maxCloudRetries = 3;
        [SerializeField, Tooltip("Delay between cloud sync retries in seconds.")]
        private float cloudRetryDelay = 2f;

        public event Action<PlayerSaveData> SaveLoaded;
        public event Action<bool> CloudSyncStatusChanged;
        public event Action<string> CloudSyncError;

        private PlayerSaveData _cachedSave;
        private readonly byte[] _xorMask = Encoding.UTF8.GetBytes("RILEY_LOVES_NIBBLE");
        
        // Cloud sync state
        private bool _isCloudSyncing;
        private bool _isCloudAvailable = true;
        private Coroutine _cloudSyncCoroutine;
        private DateTime _lastCloudSync;
        private int _syncRetryCount;
        private bool _isOfflineMode = false;
        private Queue<PlayerSaveData> _offlineSaveQueue = new Queue<PlayerSaveData>();
        private const int MaxOfflineSaves = 10;

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public PlayerProgressData Progress
        {
            get
            {
                return Save.Progress;
            }
        }

        public PlayerSettingsData Settings => Save.Settings;

        private PlayerSaveData Save
        {
            get
            {
                _cachedSave ??= PlayerSaveData.CreateDefault();
                return _cachedSave;
            }
        }

        private void Awake()
        {
            if (autoLoadOnAwake)
            {
                Load();
            }
        }

        /// <summary>
        /// Load persisted data, falling back to defaults when missing or corrupted.
        /// </summary>
        public void Load()
        {
            _cachedSave = ReadFromDisk();
            SaveLoaded?.Invoke(Save);
        }

        /// <summary>
        /// Persists the current <see cref="PlayerProgressData"/> to storage.
        /// Riley: "Time to save our progress! Can't let the hounds mess with our data!"
        /// </summary>
        public void Save()
        {
            if (_cachedSave == null)
            {
                Debug.LogWarning("Save() called without cached data. Creating defaults.");
                _cachedSave = PlayerSaveData.CreateDefault();
            }

            // Validate save data before writing
            if (!ValidateSaveData(Save))
            {
                Debug.LogError("Riley: Save data validation failed! Using default data instead.");
                _cachedSave = PlayerSaveData.CreateDefault();
            }

            Save.Progress.Version++;
            
            try
            {
                WriteToDisk(Save);
                Debug.Log("Nibble: *happy bark* (Translation: Progress saved successfully!)");
            }
            catch (Exception ex)
            {
                HandleSaveError("Save", ex);
                return;
            }
            
            // Use optimized save for mobile, regular save for desktop
            if (Application.isMobilePlatform)
            {
                OptimizedSave();
            }
            else
            {
                // Trigger cloud sync if enabled
                if (enableCloudSync && _isCloudAvailable)
                {
                    SyncToCloud();
                }
            }
        }

        /// <summary>
        /// Store a new high score and optional currency payout before saving.
        /// </summary>
        public void SaveRunResults(int score, int currencyReward, IReadOnlyCollection<string> unlockedDuringRun)
        {
            var progress = Progress;
            progress.HighScore = score;
            progress.Currency = progress.Currency + Mathf.Max(0, currencyReward);

            if (unlockedDuringRun != null)
            {
                foreach (var upgradeId in unlockedDuringRun)
                {
                    progress.UnlockUpgrade(upgradeId);
                }
            }

            Save();
        }

        /// <summary>
        /// Unlocks a single upgrade and immediately saves when successful.
        /// </summary>
        public bool UnlockUpgrade(string upgradeId)
        {
            if (!Progress.UnlockUpgrade(upgradeId))
            {
                return false;
            }

            Save();
            return true;
        }

        /// <summary>
        /// Persist non-binding settings like audio sliders or UI layout tweaks.
        /// </summary>
        public void StoreSettings(float musicVolume, float sfxVolume, bool hapticsEnabled, bool leftHandedUi)
        {
            var settings = Settings;
            settings.MusicVolume = musicVolume;
            settings.SfxVolume = sfxVolume;
            settings.HapticsEnabled = hapticsEnabled;
            settings.LeftHandedUi = leftHandedUi;
            Save();
        }

        /// <summary>
        /// Simple binding persistence using PlayerPrefs for cross-platform compatibility.
        /// </summary>
        public void StoreKeyBinding(string actionId, KeyCode key)
        {
            if (string.IsNullOrEmpty(actionId))
            {
                return;
            }

            Settings.SetBinding(actionId, key);
            PlayerPrefs.SetInt(KeyBindingPrefix + actionId, (int)key);
            PlayerPrefs.Save();
            Save();
        }

        public KeyCode LoadKeyBinding(string actionId, KeyCode fallback)
        {
            if (string.IsNullOrEmpty(actionId))
            {
                return fallback;
            }

            if (PlayerPrefs.HasKey(KeyBindingPrefix + actionId))
            {
                return (KeyCode)PlayerPrefs.GetInt(KeyBindingPrefix + actionId, (int)fallback);
            }

            return Settings.GetBinding(actionId, fallback);
        }

        public void DeleteSave()
        {
            _cachedSave = PlayerSaveData.CreateDefault();
#if UNITY_WEBGL && !UNITY_EDITOR
            PlayerPrefs.DeleteKey(SaveFileName);
            PlayerPrefs.Save();
#else
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
#endif
            foreach (var binding in Settings.Bindings)
            {
                if (!string.IsNullOrEmpty(binding.actionId))
                {
                    PlayerPrefs.DeleteKey(KeyBindingPrefix + binding.actionId);
                }
            }
            PlayerPrefs.Save();
            SaveLoaded?.Invoke(Save);
        }

        private PlayerSaveData ReadFromDisk()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var payload = PlayerPrefs.GetString(SaveFileName, string.Empty);
            if (string.IsNullOrEmpty(payload))
            {
                return PlayerSaveData.CreateDefault();
            }

            return Deserialize(payload);
#else
            try
            {
                if (!File.Exists(SavePath))
                {
                    return PlayerSaveData.CreateDefault();
                }

                using var stream = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var payload = reader.ReadToEnd();
                return Deserialize(payload);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save file at {SavePath}: {ex.Message}");
                return PlayerSaveData.CreateDefault();
            }
#endif
        }

        private void WriteToDisk(PlayerSaveData data)
        {
            try
            {
                var payload = Serialize(data);
                
                // Validate JSON integrity before writing
                if (!ValidateSaveIntegrity(payload))
                {
                    Debug.LogError("Riley: Generated save data failed integrity check! Aborting save.");
                    return;
                }

#if UNITY_WEBGL && !UNITY_EDITOR
                PlayerPrefs.SetString(SaveFileName, payload);
                PlayerPrefs.Save();
                Debug.Log("Nibble: *bark* (Translation: Save written to PlayerPrefs!)");
#else
                var directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write to temporary file first, then move to final location (atomic operation)
                var tempPath = SavePath + ".tmp";
                using var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                writer.Write(payload);
                writer.Flush();
                stream.Flush();

                // Atomic move to final location
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
                File.Move(tempPath, SavePath);
                
                Debug.Log("Riley: Save file written successfully with atomic operation!");
#endif
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.LogError($"Riley: Permission denied writing save file at {SavePath}: {ex.Message}");
                HandleSaveError("WriteToDisk_Unauthorized", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                Debug.LogError($"Riley: Save directory not found: {ex.Message}");
                HandleSaveError("WriteToDisk_DirectoryNotFound", ex);
            }
            catch (IOException ex)
            {
                Debug.LogError($"Riley: I/O error writing save file: {ex.Message}");
                HandleSaveError("WriteToDisk_IOError", ex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Unexpected error writing save file at {SavePath}: {ex.Message}");
                HandleSaveError("WriteToDisk_Unexpected", ex);
            }
        }

        private string Serialize(PlayerSaveData data)
        {
            if (!obfuscatePayload)
            {
                return JsonUtility.ToJson(data, prettyPrint: true);
            }

            var json = JsonUtility.ToJson(data, prettyPrint: false);
            var bytes = Encoding.UTF8.GetBytes(json);
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= _xorMask[i % _xorMask.Length];
            }

            return Convert.ToBase64String(bytes);
        }

        private PlayerSaveData Deserialize(string payload)
        {
            try
            {
                if (string.IsNullOrEmpty(payload))
                {
                    return PlayerSaveData.CreateDefault();
                }

                if (obfuscatePayload)
                {
                    var bytes = Convert.FromBase64String(payload);
                    for (var i = 0; i < bytes.Length; i++)
                    {
                        bytes[i] ^= _xorMask[i % _xorMask.Length];
                    }

                    payload = Encoding.UTF8.GetString(bytes);
                }

                var data = JsonUtility.FromJson<PlayerSaveData>(payload);
                return data ?? PlayerSaveData.CreateDefault();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse save payload, using defaults. {ex.Message}");
                return PlayerSaveData.CreateDefault();
            }
        }

        /// <summary>
        /// Syncs save data to cloud storage with retry logic and offline fallback.
        /// Riley: "Even my save files need to be backed up in the cloud. Can't trust these hounds with my progress!"
        /// </summary>
        public void SyncToCloud()
        {
            if (!enableCloudSync || _isCloudSyncing || string.IsNullOrEmpty(cloudSaveUrl))
            {
                return;
            }

            if (_cloudSyncCoroutine != null)
            {
                StopCoroutine(_cloudSyncCoroutine);
            }

            _cloudSyncCoroutine = StartCoroutine(CloudSyncCoroutine());
        }

        /// <summary>
        /// Loads save data from cloud storage, merging with local data if conflicts exist.
        /// Nibble: "Bark! (Translation: Fetch my treats from the sky!)"
        /// </summary>
        public void LoadFromCloud()
        {
            if (!enableCloudSync || _isCloudSyncing || string.IsNullOrEmpty(cloudSaveUrl))
            {
                return;
            }

            if (_cloudSyncCoroutine != null)
            {
                StopCoroutine(_cloudSyncCoroutine);
            }

            _cloudSyncCoroutine = StartCoroutine(CloudLoadCoroutine());
        }

        private IEnumerator CloudSyncCoroutine()
        {
            _isCloudSyncing = true;
            CloudSyncStatusChanged?.Invoke(true);

            var payload = Serialize(Save);
            var request = CreateCloudRequest(cloudSaveUrl, "POST", payload);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                _lastCloudSync = DateTime.Now;
                _syncRetryCount = 0;
                _isCloudAvailable = true;
                Debug.Log("Riley: Cloud save successful! Even the hounds can't delete this progress.");
            }
            else
            {
                HandleCloudSyncError($"Cloud sync failed: {request.error}");
            }

            _isCloudSyncing = false;
            CloudSyncStatusChanged?.Invoke(false);
        }

        private IEnumerator CloudLoadCoroutine()
        {
            _isCloudSyncing = true;
            CloudSyncStatusChanged?.Invoke(true);

            var request = CreateCloudRequest(cloudSaveUrl, "GET", null);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var cloudData = Deserialize(request.downloadHandler.text);
                    var localData = _cachedSave;

                    // Merge cloud and local data, preferring higher scores and newer unlocks
                    var mergedData = MergeSaveData(localData, cloudData);
                    _cachedSave = mergedData;
                    WriteToDisk(mergedData);
                    SaveLoaded?.Invoke(mergedData);

                    Debug.Log("Nibble: *happy bark* (Translation: Got my treats from the cloud!)");
                }
                catch (Exception ex)
                {
                    HandleCloudSyncError($"Failed to parse cloud save: {ex.Message}");
                }
            }
            else
            {
                HandleCloudSyncError($"Cloud load failed: {request.error}");
            }

            _isCloudSyncing = false;
            CloudSyncStatusChanged?.Invoke(false);
        }

        private UnityWebRequest CreateCloudRequest(string url, string method, string payload)
        {
            var request = new UnityWebRequest(url, method);
            
            if (!string.IsNullOrEmpty(payload))
            {
                var bodyRaw = Encoding.UTF8.GetBytes(payload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(cloudApiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {cloudApiKey}");
            }

            request.timeout = (int)CloudSyncTimeout;
            return request;
        }

        private PlayerSaveData MergeSaveData(PlayerSaveData local, PlayerSaveData cloud)
        {
            // Riley: "Gotta be smart about merging saves. Can't let the hounds mess with my progress!"
            var merged = new PlayerSaveData
            {
                progress = new PlayerProgressData
                {
                    HighScore = Mathf.Max(local.Progress.HighScore, cloud.Progress.HighScore),
                    Currency = Mathf.Max(local.Progress.Currency, cloud.Progress.Currency),
                    Version = Mathf.Max(local.Progress.Version, cloud.Progress.Version),
                    UnlockedUpgrades = MergeUpgradeLists(local.Progress.UnlockedUpgrades, cloud.Progress.UnlockedUpgrades)
                },
                settings = local.Settings // Prefer local settings (key bindings, audio levels)
            };

            return merged;
        }

        private List<string> MergeUpgradeLists(List<string> local, List<string> cloud)
        {
            var merged = new List<string>(local);
            foreach (var upgrade in cloud)
            {
                if (!merged.Contains(upgrade))
                {
                    merged.Add(upgrade);
                }
            }
            return merged;
        }

        private void HandleCloudSyncError(string error)
        {
            _syncRetryCount++;
            _isCloudAvailable = _syncRetryCount < MaxRetryAttempts;
            
            CloudSyncError?.Invoke(error);
            Debug.LogWarning($"Riley: Cloud sync issue - {error}. Retry count: {_syncRetryCount}");

            if (!_isCloudAvailable)
            {
                Debug.LogWarning("Nibble: *sad whine* (Translation: Cloud is down, but we'll keep playing offline!)");
            }
        }

        /// <summary>
        /// Validates JSON integrity to prevent corruption.
        /// Riley: "Can't have corrupted save files. The hounds might have tampered with them!"
        /// </summary>
        private bool ValidateSaveIntegrity(string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData))
                {
                    return false;
                }

                var testData = JsonUtility.FromJson<PlayerSaveData>(jsonData);
                if (testData == null)
                {
                    return false;
                }

                // Validate data structure
                if (testData.Progress == null || testData.Settings == null)
                {
                    return false;
                }

                // Validate reasonable values
                if (testData.Progress.HighScore < 0 || testData.Progress.Currency < 0 || testData.Progress.Version < 1)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Save integrity validation failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Enhanced error handling for save operations with detailed logging.
        /// Nibble: "Bark! (Translation: Better error handling for save operations!)"
        /// </summary>
        private void HandleSaveError(string operation, Exception ex)
        {
            var errorMessage = $"Save operation '{operation}' failed: {ex.Message}";
            Debug.LogError($"Riley: {errorMessage}");
            
            // Notify UI about the error
            CloudSyncError?.Invoke(errorMessage);
            
            // In production, you might want to send this to analytics
            LogErrorToAnalytics(operation, ex);
        }

        /// <summary>
        /// Logs errors to analytics for monitoring save system health.
        /// Riley: "Gotta track these errors to improve the save system!"
        /// </summary>
        private void LogErrorToAnalytics(string operation, Exception ex)
        {
            // In a real game, you'd send this to your analytics service
            Debug.Log($"Analytics: SaveError - Operation: {operation}, Error: {ex.GetType().Name}");
        }

        /// <summary>
        /// Mobile-optimized save operation that reduces battery drain.
        /// Riley: "Gotta be efficient on mobile devices!"
        /// </summary>
        private void OptimizedSave()
        {
            if (_cachedSave == null)
            {
                Debug.LogWarning("Save() called without cached data. Creating defaults.");
                _cachedSave = PlayerSaveData.CreateDefault();
            }

            try
            {
                Save.Progress.Version++;
                WriteToDisk(Save);
                
                // Queue for cloud sync instead of immediate sync on mobile
                if (enableCloudSync && _isCloudAvailable)
                {
                    if (Application.isMobilePlatform)
                    {
                        QueueForCloudSync(Save);
                    }
                    else
                    {
                        SyncToCloud();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleSaveError("OptimizedSave", ex);
            }
        }

        /// <summary>
        /// Queues save data for cloud sync to reduce battery drain on mobile.
        /// Nibble: "Bark! (Translation: Queue saves for better mobile performance!)"
        /// </summary>
        private void QueueForCloudSync(PlayerSaveData saveData)
        {
            if (_offlineSaveQueue.Count >= MaxOfflineSaves)
            {
                _offlineSaveQueue.Dequeue(); // Remove oldest save
            }

            _offlineSaveQueue.Enqueue(saveData);
            
            // Process queue if not already syncing
            if (!_isCloudSyncing)
            {
                StartCoroutine(ProcessOfflineSaveQueue());
            }
        }

        /// <summary>
        /// Processes the offline save queue with mobile-optimized intervals.
        /// Riley: "Process those queued saves efficiently!"
        /// </summary>
        private IEnumerator ProcessOfflineSaveQueue()
        {
            while (_offlineSaveQueue.Count > 0 && _isCloudAvailable)
            {
                var saveData = _offlineSaveQueue.Dequeue();
                
                // Use the most recent save data
                _cachedSave = saveData;
                yield return StartCoroutine(CloudSyncCoroutine());
                
                // Wait between syncs to reduce battery drain
                if (Application.isMobilePlatform)
                {
                    yield return new WaitForSeconds(cloudRetryDelay);
                }
            }
        }

        /// <summary>
        /// Validates save data before writing to prevent corruption.
        /// Riley: "Gotta validate before saving to prevent corrupted data!"
        /// </summary>
        private bool ValidateSaveData(PlayerSaveData data)
        {
            if (data == null)
            {
                Debug.LogError("Riley: Save data is null!");
                return false;
            }

            if (data.Progress == null || data.Settings == null)
            {
                Debug.LogError("Riley: Save data has null progress or settings!");
                return false;
            }

            // Validate reasonable ranges
            if (data.Progress.HighScore < 0 || data.Progress.HighScore > 999999999)
            {
                Debug.LogWarning("Riley: High score out of reasonable range, clamping to valid range");
                data.Progress.HighScore = Mathf.Clamp(data.Progress.HighScore, 0, 999999999);
            }

            if (data.Progress.Currency < 0 || data.Progress.Currency > 999999999)
            {
                Debug.LogWarning("Riley: Currency out of reasonable range, clamping to valid range");
                data.Progress.Currency = Mathf.Clamp(data.Progress.Currency, 0, 999999999);
            }

            if (data.Progress.Version < 1)
            {
                Debug.LogWarning("Riley: Version number invalid, setting to 1");
                data.Progress.Version = 1;
            }

            return true;
        }

        private void Update()
        {
            // Auto-sync if interval has passed and cloud is available
            if (enableCloudSync && _isCloudAvailable && cloudSyncInterval > 0)
            {
                if ((DateTime.Now - _lastCloudSync).TotalSeconds >= cloudSyncInterval)
                {
                    SyncToCloud();
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Open Save Location")]
        private void RevealSaveFolder()
        {
            UnityEditor.EditorUtility.RevealInFinder(SavePath);
        }

        [ContextMenu("Test Cloud Sync")]
        private void TestCloudSync()
        {
            SyncToCloud();
        }
#endif
    }
}
