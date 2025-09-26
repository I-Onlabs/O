using System;
using System.Collections.Generic;
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
        [SerializeField] private List<string> unlockedUpgrades = new();

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

        public IReadOnlyList<string> UnlockedUpgrades
        {
            get => unlockedUpgrades;
            set
            {
                unlockedUpgrades = value != null ? new List<string>(value) : new List<string>();
            }
        }

        public static PlayerProgressData CreateDefault()
        {
            return new PlayerProgressData
            {
                version = 1,
                highScore = 0,
                currency = 0,
                unlockedUpgrades = new List<string>()
            };
        }

        public bool HasUpgrade(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId))
            {
                return false;
            }

            return unlockedUpgrades != null && unlockedUpgrades.Contains(upgradeId);
        }

        public bool UnlockUpgrade(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId))
            {
                return false;
            }

            unlockedUpgrades ??= new List<string>();
            if (unlockedUpgrades.Contains(upgradeId))
            {
                return false;
            }

            unlockedUpgrades.Add(upgradeId);
            return true;
        }

        public bool RemoveUpgrade(string upgradeId)
        {
            return unlockedUpgrades != null && unlockedUpgrades.Remove(upgradeId);
        }
    }
}
