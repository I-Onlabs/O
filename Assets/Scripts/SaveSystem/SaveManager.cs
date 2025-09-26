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

        public event Action<PlayerSaveData> SaveLoaded;

        private PlayerSaveData _cachedSave;
        private readonly byte[] _xorMask = Encoding.UTF8.GetBytes("RILEY_LOVES_NIBBLE");

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

#if UNITY_EDITOR
        [ContextMenu("Open Save Location")]
        private void RevealSaveFolder()
        {
            UnityEditor.EditorUtility.RevealInFinder(SavePath);
        }
#endif
    }
}
