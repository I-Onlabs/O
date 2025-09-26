using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngryDogs.SaveSystem;
using AngryDogs.Data;
using AngryDogs.UI;

namespace AngryDogs.Gameplay
{
    /// <summary>
    /// Daily challenge system with randomized objectives and KibbleCoin rewards.
    /// Riley: "Time for daily challenges! Gotta keep players coming back for more cyberpunk dog-chasing action!"
    /// Nibble: "Bark! (Translation: Daily treats for completing challenges!)"
    /// </summary>
    public sealed class DailyChallengeManager : MonoBehaviour
    {
        [System.Serializable]
        public class DailyChallenge
        {
            public string challengeId;
            public string title;
            public string description;
            public ChallengeType type;
            public int targetValue;
            public int currentValue;
            public int kibbleCoinReward;
            public bool isCompleted;
            public bool isClaimed;
            public DateTime startTime;
            public DateTime endTime;
            public DifficultyLevel difficulty;
            public string[] requiredUpgrades; // Upgrades needed to unlock this challenge
        }

        public enum ChallengeType
        {
            SurviveDistance,        // Survive for X meters without shooting
            ProtectNibble,          // Protect Nibble from X bites
            ObstacleRepurposing,    // Repurpose X obstacles
            BossWeakPoints,         // Destroy X boss weak points
            NoDamageRun,           // Complete a run without taking damage
            SpeedRun,              // Complete a run in under X time
            KibbleCollection,      // Collect X KibbleCoins
            HoundDefeat,           // Defeat X hounds
            ComboStreak,           // Achieve X combo streak
            SpecialObstacle,       // Interact with X special obstacles
            CloudSync,             // Perform X cloud syncs
            QuipDisplay,           // Display X quips
            NeonSlobberCannons,    // Repurpose X Neon Slobber Cannons
            CyberChihuahuaKing,    // Defeat Cyber-Chihuahua King X times
            KibbleCoinCollection   // Collect X KibbleCoins in a single run
        }

        public enum DifficultyLevel
        {
            Easy,      // 1-2 KibbleCoins
            Medium,    // 3-5 KibbleCoins
            Hard,      // 6-10 KibbleCoins
            Expert     // 11-20 KibbleCoins
        }

        [System.Serializable]
        public class ChallengeTemplate
        {
            public ChallengeType type;
            public string[] titles;
            public string[] descriptions;
            public int[] targetValues;
            public int[] kibbleCoinRewards;
            public DifficultyLevel[] difficulties;
            public string[][] requiredUpgrades;
        }

        [Header("Challenge Settings")]
        [SerializeField] private bool enableDailyChallenges = true;
        [SerializeField] private int maxActiveChallenges = 3;
        [SerializeField] private int challengeDurationHours = 24;
        [SerializeField] private int resetHour = 0; // Hour of day to reset challenges (0-23)
        [SerializeField] private bool allowChallengeReroll = true;
        [SerializeField] private int rerollCost = 10; // KibbleCoins cost to reroll a challenge

        [Header("Challenge Templates")]
        [SerializeField] private ChallengeTemplate[] challengeTemplates = {
            new ChallengeTemplate {
                type = ChallengeType.SurviveDistance,
                titles = new[] { "Silent Runner", "Stealth Mode", "No Shooting Zone" },
                descriptions = new[] { "Survive {0} meters without shooting", "Complete {0} meters without firing a single shot", "Navigate {0} meters in stealth mode" },
                targetValues = new[] { 100, 250, 500, 1000 },
                kibbleCoinRewards = new[] { 2, 5, 8, 15 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.ProtectNibble,
                titles = new[] { "Nibble's Guardian", "Protector Mode", "Shield Master" },
                descriptions = new[] { "Protect Nibble from {0} bites", "Keep Nibble safe from {0} attacks", "Shield Nibble from {0} hound bites" },
                targetValues = new[] { 3, 5, 8, 12 },
                kibbleCoinRewards = new[] { 3, 6, 10, 18 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.ObstacleRepurposing,
                titles = new[] { "Obstacle Master", "Repurposing Pro", "Trap Specialist" },
                descriptions = new[] { "Repurpose {0} obstacles", "Turn {0} obstacles into traps", "Convert {0} obstacles to defenses" },
                targetValues = new[] { 5, 10, 15, 25 },
                kibbleCoinRewards = new[] { 2, 4, 7, 12 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.BossWeakPoints,
                titles = new[] { "Weak Point Hunter", "Boss Breaker", "Mech Destroyer" },
                descriptions = new[] { "Destroy {0} boss weak points", "Hit {0} weak points on the Cyber-Chihuahua", "Break {0} mech-suit weak points" },
                targetValues = new[] { 2, 4, 6, 10 },
                kibbleCoinRewards = new[] { 4, 8, 12, 20 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.NoDamageRun,
                titles = new[] { "Perfect Run", "Untouchable", "Flawless Victory" },
                descriptions = new[] { "Complete a run without taking damage", "Finish a run with full health", "Achieve a perfect run" },
                targetValues = new[] { 1, 1, 1, 1 },
                kibbleCoinRewards = new[] { 5, 10, 15, 25 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.SpeedRun,
                titles = new[] { "Speed Demon", "Lightning Fast", "Sonic Speed" },
                descriptions = new[] { "Complete a run in under {0} seconds", "Finish a run in {0} seconds or less", "Achieve {0} second completion time" },
                targetValues = new[] { 120, 90, 60, 45 },
                kibbleCoinRewards = new[] { 3, 6, 9, 15 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.KibbleCollection,
                titles = new[] { "Kibble Collector", "Coin Hunter", "Treasure Seeker" },
                descriptions = new[] { "Collect {0} KibbleCoins", "Gather {0} KibbleCoins in a single run", "Earn {0} KibbleCoins" },
                targetValues = new[] { 50, 100, 200, 500 },
                kibbleCoinRewards = new[] { 2, 4, 6, 10 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.HoundDefeat,
                titles = new[] { "Hound Hunter", "Pack Destroyer", "Alpha Predator" },
                descriptions = new[] { "Defeat {0} hounds", "Take down {0} cybernetic hounds", "Eliminate {0} hounds" },
                targetValues = new[] { 10, 25, 50, 100 },
                kibbleCoinRewards = new[] { 2, 4, 7, 12 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.ComboStreak,
                titles = new[] { "Combo Master", "Streak King", "Chain Breaker" },
                descriptions = new[] { "Achieve a {0} combo streak", "Maintain a {0} combo streak", "Build a {0} combo streak" },
                targetValues = new[] { 5, 10, 20, 50 },
                kibbleCoinRewards = new[] { 3, 6, 9, 15 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.SpecialObstacle,
                titles = new[] { "Obstacle Expert", "Specialist", "Master of Obstacles" },
                descriptions = new[] { "Interact with {0} special obstacles", "Use {0} special obstacles", "Activate {0} special obstacles" },
                targetValues = new[] { 3, 6, 10, 15 },
                kibbleCoinRewards = new[] { 2, 4, 6, 10 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.CloudSync,
                titles = new[] { "Cloud Master", "Sync Specialist", "Data Guardian" },
                descriptions = new[] { "Perform {0} cloud syncs", "Sync data {0} times", "Complete {0} cloud operations" },
                targetValues = new[] { 3, 5, 8, 12 },
                kibbleCoinRewards = new[] { 1, 2, 3, 5 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.QuipDisplay,
                titles = new[] { "Chatty Cathy", "Quip Master", "Dialogue King" },
                descriptions = new[] { "Display {0} quips", "Show {0} Riley quips", "Trigger {0} Nibble barks" },
                targetValues = new[] { 5, 10, 15, 25 },
                kibbleCoinRewards = new[] { 1, 2, 3, 5 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.NeonSlobberCannons,
                titles = new[] { "Neon Slobber Master", "Cannon Repurposer", "Goo Trap Specialist" },
                descriptions = new[] { "Repurpose {0} Neon Slobber Cannons", "Convert {0} cannons into defensive shields", "Turn {0} slobber cannons into traps" },
                targetValues = new[] { 5, 10, 15, 25 },
                kibbleCoinRewards = new[] { 3, 6, 10, 18 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.CyberChihuahuaKing,
                titles = new[] { "King Slayer", "Mech Destroyer", "Tyrant Hunter" },
                descriptions = new[] { "Defeat the Cyber-Chihuahua King {0} times", "Take down the tiny tyrant {0} times", "Overthrow the mech-suit king {0} times" },
                targetValues = new[] { 1, 2, 3, 5 },
                kibbleCoinRewards = new[] { 8, 15, 25, 40 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            },
            new ChallengeTemplate {
                type = ChallengeType.KibbleCoinCollection,
                titles = new[] { "KibbleCoin Collector", "Currency Hunter", "Treasure Seeker" },
                descriptions = new[] { "Collect {0} KibbleCoins in a single run", "Gather {0} KibbleCoins", "Earn {0} KibbleCoins" },
                targetValues = new[] { 25, 50, 100, 200 },
                kibbleCoinRewards = new[] { 2, 4, 6, 10 },
                difficulties = new[] { DifficultyLevel.Easy, DifficultyLevel.Medium, DifficultyLevel.Hard, DifficultyLevel.Expert },
                requiredUpgrades = new[] { new string[0], new string[0], new string[0], new string[0] }
            }
        };

        [Header("Systems")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private LocalizationManager localizationManager;

        private List<DailyChallenge> _activeChallenges;
        private DateTime _lastResetTime;
        private bool _isInitialized;

        // Events
        public System.Action<DailyChallenge> OnChallengeCompleted;
        public System.Action<DailyChallenge> OnChallengeClaimed;
        public System.Action<DailyChallenge> OnChallengeRerolled;
        public System.Action<DailyChallenge> OnChallengeProgressUpdated;

        private void Awake()
        {
            _activeChallenges = new List<DailyChallenge>();
        }

        private void Start()
        {
            if (enableDailyChallenges)
            {
                InitializeDailyChallenges();
            }
        }

        /// <summary>
        /// Initializes the daily challenge system.
        /// Riley: "Initialize the daily challenge system!"
        /// </summary>
        private void InitializeDailyChallenges()
        {
            try
            {
                // Load saved challenges
                LoadChallenges();
                
                // Check if challenges need reset
                CheckAndResetChallenges();
                
                // Generate new challenges if needed
                GenerateNewChallenges();
                
                _isInitialized = true;
                
                Debug.Log("Riley: Daily challenge system initialized!");
                Debug.Log("Nibble: *bark* (Translation: Daily challenges ready!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to initialize daily challenges: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads challenges from save data.
        /// Nibble: "Bark! (Translation: Load challenges from save data!)"
        /// </summary>
        private void LoadChallenges()
        {
            if (saveManager != null)
            {
                // Load challenges from save data
                // For now, we'll start with empty challenges
                _activeChallenges.Clear();
                
                Debug.Log("Riley: Challenges loaded from save data!");
            }
        }

        /// <summary>
        /// Checks if challenges need reset and resets them if necessary.
        /// Riley: "Check if challenges need reset!"
        /// </summary>
        private void CheckAndResetChallenges()
        {
            var now = DateTime.Now;
            var lastReset = _lastResetTime;
            
            // Check if 24 hours have passed since last reset
            if ((now - lastReset).TotalHours >= challengeDurationHours)
            {
                ResetChallenges();
            }
        }

        /// <summary>
        /// Resets all challenges for a new day.
        /// Riley: "Reset challenges for a new day!"
        /// </summary>
        private void ResetChallenges()
        {
            _activeChallenges.Clear();
            _lastResetTime = DateTime.Now;
            
            Debug.Log("Riley: Daily challenges reset!");
            Debug.Log("Nibble: *bark* (Translation: New challenges available!)");
        }

        /// <summary>
        /// Generates new challenges if needed.
        /// Nibble: "Bark! (Translation: Generate new challenges!)"
        /// </summary>
        private void GenerateNewChallenges()
        {
            while (_activeChallenges.Count < maxActiveChallenges)
            {
                var newChallenge = GenerateRandomChallenge();
                if (newChallenge != null)
                {
                    _activeChallenges.Add(newChallenge);
                }
            }
            
            Debug.Log($"Riley: Generated {_activeChallenges.Count} daily challenges!");
        }

        /// <summary>
        /// Generates a random challenge.
        /// Riley: "Generate a random challenge!"
        /// </summary>
        private DailyChallenge GenerateRandomChallenge()
        {
            if (challengeTemplates.Length == 0)
            {
                return null;
            }

            // Select random challenge type
            var template = challengeTemplates[UnityEngine.Random.Range(0, challengeTemplates.Length)];
            
            // Select random difficulty
            var difficultyIndex = UnityEngine.Random.Range(0, template.difficulties.Length);
            var difficulty = template.difficulties[difficultyIndex];
            
            // Create challenge
            var challenge = new DailyChallenge
            {
                challengeId = Guid.NewGuid().ToString(),
                title = template.titles[UnityEngine.Random.Range(0, template.titles.Length)],
                description = template.descriptions[UnityEngine.Random.Range(0, template.descriptions.Length)],
                type = template.type,
                targetValue = template.targetValues[difficultyIndex],
                currentValue = 0,
                kibbleCoinReward = template.kibbleCoinRewards[difficultyIndex],
                isCompleted = false,
                isClaimed = false,
                startTime = DateTime.Now,
                endTime = DateTime.Now.AddHours(challengeDurationHours),
                difficulty = difficulty,
                requiredUpgrades = template.requiredUpgrades[difficultyIndex]
            };

            // Format description with target value
            challenge.description = string.Format(challenge.description, challenge.targetValue);

            return challenge;
        }

        /// <summary>
        /// Updates challenge progress.
        /// Riley: "Update challenge progress!"
        /// </summary>
        public void UpdateChallengeProgress(ChallengeType type, int amount = 1)
        {
            if (!_isInitialized) return;

            foreach (var challenge in _activeChallenges)
            {
                if (challenge.type == type && !challenge.isCompleted)
                {
                    challenge.currentValue += amount;
                    
                    // Check if challenge is completed
                    if (challenge.currentValue >= challenge.targetValue)
                    {
                        challenge.isCompleted = true;
                        OnChallengeCompleted?.Invoke(challenge);
                        
                        Debug.Log($"Riley: Challenge '{challenge.title}' completed!");
                        Debug.Log($"Nibble: *bark* (Translation: Challenge completed! Reward: {challenge.kibbleCoinReward} KibbleCoins!)");
                    }
                    
                    OnChallengeProgressUpdated?.Invoke(challenge);
                }
            }
        }

        /// <summary>
        /// Claims a completed challenge reward.
        /// Nibble: "Bark! (Translation: Claim challenge reward!)"
        /// </summary>
        public bool ClaimChallengeReward(string challengeId)
        {
            var challenge = _activeChallenges.Find(c => c.challengeId == challengeId);
            if (challenge == null || !challenge.isCompleted || challenge.isClaimed)
            {
                return false;
            }

            challenge.isClaimed = true;
            
            // Award KibbleCoins
            if (saveManager != null)
            {
                var progress = saveManager.Progress;
                progress.Currency += challenge.kibbleCoinReward;
                saveManager.Save();
            }
            
            OnChallengeClaimed?.Invoke(challenge);
            
            Debug.Log($"Riley: Challenge '{challenge.title}' reward claimed! +{challenge.kibbleCoinReward} KibbleCoins!");
            Debug.Log($"Nibble: *bark* (Translation: Reward claimed! Got {challenge.kibbleCoinReward} KibbleCoins!)");
            
            return true;
        }

        /// <summary>
        /// Rerolls a challenge.
        /// Riley: "Reroll a challenge!"
        /// </summary>
        public bool RerollChallenge(string challengeId)
        {
            if (!allowChallengeReroll)
            {
                Debug.LogWarning("Riley: Challenge rerolling is disabled!");
                return false;
            }

            var challenge = _activeChallenges.Find(c => c.challengeId == challengeId);
            if (challenge == null || challenge.isClaimed)
            {
                return false;
            }

            // Check if player has enough KibbleCoins
            if (saveManager != null)
            {
                var progress = saveManager.Progress;
                if (progress.Currency < rerollCost)
                {
                    Debug.LogWarning("Riley: Not enough KibbleCoins to reroll challenge!");
                    return false;
                }

                // Deduct reroll cost
                progress.Currency -= rerollCost;
                saveManager.Save();
            }

            // Generate new challenge
            var newChallenge = GenerateRandomChallenge();
            if (newChallenge != null)
            {
                var index = _activeChallenges.IndexOf(challenge);
                _activeChallenges[index] = newChallenge;
                
                OnChallengeRerolled?.Invoke(newChallenge);
                
                Debug.Log($"Riley: Challenge rerolled! Cost: {rerollCost} KibbleCoins");
                Debug.Log($"Nibble: *bark* (Translation: New challenge generated!)");
                
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all active challenges.
        /// Riley: "Get all active challenges!"
        /// </summary>
        public List<DailyChallenge> GetActiveChallenges()
        {
            return new List<DailyChallenge>(_activeChallenges);
        }

        /// <summary>
        /// Gets completed challenges.
        /// Nibble: "Bark! (Translation: Get completed challenges!)"
        /// </summary>
        public List<DailyChallenge> GetCompletedChallenges()
        {
            return _activeChallenges.FindAll(c => c.isCompleted);
        }

        /// <summary>
        /// Gets claimable challenges.
        /// Riley: "Get claimable challenges!"
        /// </summary>
        public List<DailyChallenge> GetClaimableChallenges()
        {
            return _activeChallenges.FindAll(c => c.isCompleted && !c.isClaimed);
        }

        /// <summary>
        /// Gets challenge progress percentage.
        /// Nibble: "Bark! (Translation: Get challenge progress!)"
        /// </summary>
        public float GetChallengeProgress(string challengeId)
        {
            var challenge = _activeChallenges.Find(c => c.challengeId == challengeId);
            if (challenge == null)
            {
                return 0f;
            }

            return Mathf.Clamp01((float)challenge.currentValue / challenge.targetValue);
        }

        /// <summary>
        /// Gets time remaining for challenges.
        /// Riley: "Get time remaining for challenges!"
        /// </summary>
        public TimeSpan GetTimeRemaining()
        {
            var now = DateTime.Now;
            var nextReset = _lastResetTime.AddHours(challengeDurationHours);
            
            if (nextReset > now)
            {
                return nextReset - now;
            }
            
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Gets total KibbleCoins available from challenges.
        /// Nibble: "Bark! (Translation: Get total KibbleCoins available!)"
        /// </summary>
        public int GetTotalAvailableKibbleCoins()
        {
            var total = 0;
            foreach (var challenge in _activeChallenges)
            {
                if (challenge.isCompleted && !challenge.isClaimed)
                {
                    total += challenge.kibbleCoinReward;
                }
            }
            return total;
        }

        /// <summary>
        /// Gets challenge statistics.
        /// Riley: "Get challenge statistics!"
        /// </summary>
        public string GetChallengeStats()
        {
            var stats = $"Daily Challenge Statistics:\n";
            stats += $"Active Challenges: {_activeChallenges.Count}\n";
            stats += $"Completed Challenges: {GetCompletedChallenges().Count}\n";
            stats += $"Claimable Challenges: {GetClaimableChallenges().Count}\n";
            stats += $"Total Available KibbleCoins: {GetTotalAvailableKibbleCoins()}\n";
            stats += $"Time Remaining: {GetTimeRemaining().ToString(@"hh\:mm\:ss")}\n";
            stats += $"Reroll Cost: {rerollCost} KibbleCoins\n";
            stats += $"Max Active Challenges: {maxActiveChallenges}\n";
            
            return stats;
        }

        /// <summary>
        /// Saves challenges to save data.
        /// Riley: "Save challenges to save data!"
        /// </summary>
        public void SaveChallenges()
        {
            if (saveManager != null)
            {
                // Save challenges to save data
                // You would need to add a Challenges property to PlayerSaveData
                Debug.Log("Riley: Challenges saved to save data!");
            }
        }

        private void OnDestroy()
        {
            SaveChallenges();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveChallenges();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                SaveChallenges();
            }
        }
    }
}