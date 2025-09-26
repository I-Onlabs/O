using System.IO;
using UnityEngine;
using AngryDogs.Data;

namespace AngryDogs.Systems
{
    /// <summary>
    /// Lightweight JSON save system compatible with mobile and PC platforms.
    /// </summary>
    public sealed class SaveSystem : MonoBehaviour
    {
        private const string SaveFileName = "angry_dogs_save.json";

        private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        public PlayerProgressData Load()
        {
            if (!File.Exists(SavePath))
            {
                return PlayerProgressData.CreateDefault();
            }

            try
            {
                var json = File.ReadAllText(SavePath);
                var data = JsonUtility.FromJson<PlayerProgressData>(json);
                return data ?? PlayerProgressData.CreateDefault();
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to load save file: {ex.Message}");
                return PlayerProgressData.CreateDefault();
            }
        }

        public void Save(PlayerProgressData data)
        {
            if (data == null)
            {
                Debug.LogWarning("Attempted to save null PlayerProgressData.");
                return;
            }

            data.Version++;

            var json = JsonUtility.ToJson(data, prettyPrint: true);
            try
            {
                File.WriteAllText(SavePath, json);
            }
            catch (IOException ex)
            {
                Debug.LogError($"Failed to save file: {ex.Message}");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Open Save Folder")]
        private void OpenSaveFolder()
        {
            UnityEditor.EditorUtility.RevealInFinder(SavePath);
        }
#endif
    }
}
