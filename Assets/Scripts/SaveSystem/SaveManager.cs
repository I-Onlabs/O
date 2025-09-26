using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using AngryDogs.Data;

namespace AngryDogs.SaveSystem
{
    /// <summary>
    /// Centralised persistence manager responsible for JSON save/load with mobile safe fallbacks.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SaveManager : MonoBehaviour
    {
        private const string SaveFileName = "angry_dogs_save.json";
        private const string KeyBindingPrefix = "AngryDogs_Key_";

        [Header("Behaviour")]
        [SerializeField, Tooltip("Automatically load progress during Awake.")]
        private bool autoLoadOnAwake = true;
        [SerializeField, Tooltip("Encrypt save payload lightly to deter casual tampering.")]
        private bool obfuscatePayload = true;

        public event Action<PlayerProgressData> SaveLoaded;

        private PlayerProgressData _cachedProgress;
        private readonly byte[] _xorMask = Encoding.UTF8.GetBytes("RILEY_LOVES_NIBBLE");

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public PlayerProgressData Progress
        {
            get
            {
                _cachedProgress ??= PlayerProgressData.CreateDefault();
                return _cachedProgress;
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
            _cachedProgress = ReadFromDisk();
            SaveLoaded?.Invoke(Progress);
        }

        /// <summary>
        /// Persists the current <see cref="PlayerProgressData"/> to storage.
        /// </summary>
        public void Save()
        {
            if (_cachedProgress == null)
            {
                Debug.LogWarning("Save() called without progress data. Creating defaults.");
                _cachedProgress = PlayerProgressData.CreateDefault();
            }

            _cachedProgress.Version++;
            WriteToDisk(_cachedProgress);
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
        /// Simple binding persistence using PlayerPrefs for cross-platform compatibility.
        /// </summary>
        public void StoreKeyBinding(string actionId, KeyCode key)
        {
            if (string.IsNullOrEmpty(actionId))
            {
                return;
            }

            PlayerPrefs.SetInt(KeyBindingPrefix + actionId, (int)key);
        }

        public KeyCode LoadKeyBinding(string actionId, KeyCode fallback)
        {
            if (string.IsNullOrEmpty(actionId))
            {
                return fallback;
            }

            return (KeyCode)PlayerPrefs.GetInt(KeyBindingPrefix + actionId, (int)fallback);
        }

        public void DeleteSave()
        {
            _cachedProgress = PlayerProgressData.CreateDefault();
#if UNITY_WEBGL && !UNITY_EDITOR
            PlayerPrefs.DeleteKey(SaveFileName);
            PlayerPrefs.Save();
#else
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
            }
#endif
            SaveLoaded?.Invoke(_cachedProgress);
        }

        private PlayerProgressData ReadFromDisk()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var payload = PlayerPrefs.GetString(SaveFileName, string.Empty);
            if (string.IsNullOrEmpty(payload))
            {
                return PlayerProgressData.CreateDefault();
            }

            return Deserialize(payload);
#else
            try
            {
                if (!File.Exists(SavePath))
                {
                    return PlayerProgressData.CreateDefault();
                }

                using var stream = new FileStream(SavePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(stream, Encoding.UTF8);
                var payload = reader.ReadToEnd();
                return Deserialize(payload);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load save file at {SavePath}: {ex.Message}");
                return PlayerProgressData.CreateDefault();
            }
#endif
        }

        private void WriteToDisk(PlayerProgressData data)
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

        private string Serialize(PlayerProgressData data)
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

        private PlayerProgressData Deserialize(string payload)
        {
            try
            {
                if (string.IsNullOrEmpty(payload))
                {
                    return PlayerProgressData.CreateDefault();
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

                var data = JsonUtility.FromJson<PlayerProgressData>(payload);
                return data ?? PlayerProgressData.CreateDefault();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to parse save payload, using defaults. {ex.Message}");
                return PlayerProgressData.CreateDefault();
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Open Save Location")]
        private void RevealSaveFolder()
        {
            UnityEditor.EditorUtility.RevealInFinder(SavePath);
        }
#endif
    }
}
