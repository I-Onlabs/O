using System;

namespace AngryDogs.Data
{
    /// <summary>
    /// Wrapper object combining progress and settings for persistence.
    /// </summary>
    [Serializable]
    public sealed class PlayerSaveData
    {
        public PlayerProgressData progress = PlayerProgressData.CreateDefault();
        public PlayerSettingsData settings = PlayerSettingsData.CreateDefault();

        public PlayerProgressData Progress
        {
            get => progress ??= PlayerProgressData.CreateDefault();
            set => progress = value ?? PlayerProgressData.CreateDefault();
        }

        public PlayerSettingsData Settings
        {
            get => settings ??= PlayerSettingsData.CreateDefault();
            set => settings = value ?? PlayerSettingsData.CreateDefault();
        }

        public static PlayerSaveData CreateDefault()
        {
            return new PlayerSaveData
            {
                progress = PlayerProgressData.CreateDefault(),
                settings = PlayerSettingsData.CreateDefault()
            };
        }
    }
}