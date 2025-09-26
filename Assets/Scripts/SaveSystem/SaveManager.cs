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
        /// </summary>
        public void Save()
        {
            if (_cachedSave == null)
            {
                Debug.LogWarning("Save() called without cached data. Creating defaults.");
                _cachedSave = PlayerSaveData.CreateDefault();
            }

            Save.Progress.Version++;
            WriteToDisk(Save);
            
            // Trigger cloud sync if enabled
            if (enableCloudSync && _isCloudAvailable)
            {
                SyncToCloud();
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
            var payload = Serialize(data);
#if UNITY_WEBGL && !UNITY_EDITOR
            PlayerPrefs.SetString(SaveFileName, payload);
            PlayerPrefs.Save();
#else
            try
            {
                var directory = Path.GetDirectoryName(SavePath);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var stream = new FileStream(SavePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var writer = new StreamWriter(stream, Encoding.UTF8);
                writer.Write(payload);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write save file at {SavePath}: {ex.Message}");
            }
#endif
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
                var testData = JsonUtility.FromJson<PlayerSaveData>(jsonData);
                return testData != null && testData.Progress != null && testData.Settings != null;
            }
            catch
            {
                return false;
            }
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
