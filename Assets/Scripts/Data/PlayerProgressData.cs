using System;
using UnityEngine;

namespace AngryDogs.Data
{
    /// <summary>
    /// Serializable data container for player persistence.
    /// </summary>
    [Serializable]
    public sealed class PlayerProgressData
    {
        [SerializeField] private int version;
        [SerializeField] private int highScore;
        [SerializeField] private int currency;
        [SerializeField] private string[] unlockedUpgrades;

        public int Version
        {
            get => version;
            set => version = value;
        }

        public int HighScore
        {
            get => highScore;
            set => highScore = Mathf.Max(highScore, value);
        }

        public int Currency
        {
            get => currency;
            set => currency = Mathf.Max(0, value);
        }

        public string[] UnlockedUpgrades
        {
            get => unlockedUpgrades;
            set => unlockedUpgrades = value;
        }

        public static PlayerProgressData CreateDefault()
        {
            return new PlayerProgressData
            {
                version = 1,
                highScore = 0,
                currency = 0,
                unlockedUpgrades = Array.Empty<string>()
            };
        }
    }
}
