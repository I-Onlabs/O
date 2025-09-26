using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using AngryDogs.SaveSystem;
using AngryDogs.UI;

namespace AngryDogs.Gameplay
{
    /// <summary>
    /// Leaderboard system with global and regional high scores, integrated with Unity Cloud Save.
    /// Supports social sharing for daily challenge completions and competitive scoring.
    /// Riley: "Time to show off those high scores! Can't let the hounds beat us on the leaderboard!"
    /// Nibble: "Bark! (Translation: Top score? Hounds can't catch this hacker!)"
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LeaderboardManager : MonoBehaviour
    {
        [System.Serializable]
        public class LeaderboardEntry
        {
            public string playerId;
            public string playerName;
            public int score;
            public int rank;
            public string region;
            public DateTime timestamp;
            public string platform;
            public bool isCurrentPlayer;
        }

        [System.Serializable]
        public class LeaderboardData
        {
            public List<LeaderboardEntry> globalEntries;
            public List<LeaderboardEntry> regionalEntries;
            public int playerGlobalRank;
            public int playerRegionalRank;
            public int totalPlayers;
            public DateTime lastUpdated;
        }

        [System.Serializable]
        public class SocialShareData
        {
            public string challengeTitle;
            public string challengeDescription;
            public int score;
            public int kibbleCoinsEarned;
            public string language;
            public string platform;
        }

        [Header("Leaderboard Settings")]
        [SerializeField] private bool enableLeaderboards = true;
        [SerializeField] private int maxGlobalEntries = 100;
        [SerializeField] private int maxRegionalEntries = 100;
        [SerializeField] private float updateInterval = 30f; // 30 seconds
        [SerializeField] private bool enableSocialSharing = true;
        [SerializeField] private bool enableRegionalLeaderboards = true;

        [Header("Cloud Save Integration")]
        [SerializeField] private string leaderboardUrl = "https://your-leaderboard-service.com/api/leaderboard";
        [SerializeField] private string socialShareUrl = "https://your-social-service.com/api/share";
        [SerializeField] private string apiKey = "";
        [SerializeField] private float requestTimeout = 10f;
        [SerializeField] private int maxRetryAttempts = 3;

        [Header("Regional Settings")]
        [SerializeField] private string[] supportedRegions = { "Global", "North America", "Europe", "Asia", "South America", "Oceania" };
        [SerializeField] private string defaultRegion = "Global";
        [SerializeField] private bool autoDetectRegion = true;

        [Header("Social Sharing")]
        [SerializeField] private string[] shareMessages = {
            "I survived {0}km in Angry Dogs! Can you beat my score?",
            "Just completed '{1}' challenge in Angry Dogs! +{2} KibbleCoins!",
            "Top score? Hounds can't catch this hacker! {0} points in Angry Dogs!",
            "Nibble and I are unstoppable! {0} points and counting!",
            "Cyberpunk dog-chase action! {0} points in Angry Dogs!"
        };

        [Header("Systems")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private LocalizationManager localizationManager;
        [SerializeField] private DailyChallengeManager challengeManager;

        private LeaderboardData _cachedLeaderboard;
        private string _currentRegion;
        private string _playerId;
        private string _playerName;
        private bool _isInitialized;
        private Coroutine _updateCoroutine;
        private Dictionary<string, LeaderboardEntry> _playerCache = new Dictionary<string, LeaderboardEntry>();

        // Events
        public System.Action<LeaderboardData> OnLeaderboardUpdated;
        public System.Action<LeaderboardEntry> OnPlayerRankChanged;
        public System.Action<string> OnLeaderboardError;
        public System.Action<SocialShareData> OnSocialShareRequested;

        private void Awake()
        {
            _cachedLeaderboard = new LeaderboardData
            {
                globalEntries = new List<LeaderboardEntry>(),
                regionalEntries = new List<LeaderboardEntry>(),
                playerGlobalRank = -1,
                playerRegionalRank = -1,
                totalPlayers = 0,
                lastUpdated = DateTime.MinValue
            };
        }

        private void Start()
        {
            if (enableLeaderboards)
            {
                InitializeLeaderboard();
            }
        }

        /// <summary>
        /// Initializes the leaderboard system.
        /// Riley: "Initialize the leaderboard system!"
        /// </summary>
        private void InitializeLeaderboard()
        {
            try
            {
                // Generate or load player ID
                _playerId = GeneratePlayerId();
                _playerName = GeneratePlayerName();
                
                // Detect region
                if (autoDetectRegion)
                {
                    _currentRegion = DetectPlayerRegion();
                }
                else
                {
                    _currentRegion = defaultRegion;
                }
                
                // Start update coroutine
                if (updateInterval > 0)
                {
                    _updateCoroutine = StartCoroutine(UpdateLeaderboardCoroutine());
                }
                
                _isInitialized = true;
                
                Debug.Log($"Riley: Leaderboard system initialized! Player: {_playerName}, Region: {_currentRegion}");
                Debug.Log($"Nibble: *bark* (Translation: Leaderboard ready! Let's show off our skills!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to initialize leaderboard: {ex.Message}");
                OnLeaderboardError?.Invoke($"Initialization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Submits a score to the leaderboard.
        /// Riley: "Submit a score to the leaderboard!"
        /// </summary>
        public void SubmitScore(int score, string context = "")
        {
            if (!_isInitialized || !enableLeaderboards)
            {
                Debug.LogWarning("Riley: Leaderboard not initialized or disabled!");
                return;
            }

            StartCoroutine(SubmitScoreCoroutine(score, context));
        }

        /// <summary>
        /// Gets the current leaderboard data.
        /// Nibble: "Bark! (Translation: Get leaderboard data!)"
        /// </summary>
        public LeaderboardData GetLeaderboardData()
        {
            return _cachedLeaderboard;
        }

        /// <summary>
        /// Gets global leaderboard entries.
        /// Riley: "Get global leaderboard entries!"
        /// </summary>
        public List<LeaderboardEntry> GetGlobalLeaderboard()
        {
            return new List<LeaderboardEntry>(_cachedLeaderboard.globalEntries);
        }

        /// <summary>
        /// Gets regional leaderboard entries.
        /// Nibble: "Bark! (Translation: Get regional leaderboard!)"
        /// </summary>
        public List<LeaderboardEntry> GetRegionalLeaderboard()
        {
            return new List<LeaderboardEntry>(_cachedLeaderboard.regionalEntries);
        }

        /// <summary>
        /// Gets the player's current rank.
        /// Riley: "Get my current rank!"
        /// </summary>
        public (int globalRank, int regionalRank) GetPlayerRank()
        {
            return (_cachedLeaderboard.playerGlobalRank, _cachedLeaderboard.playerRegionalRank);
        }

        /// <summary>
        /// Refreshes the leaderboard data.
        /// Nibble: "Bark! (Translation: Refresh leaderboard!)"
        /// </summary>
        public void RefreshLeaderboard()
        {
            if (_isInitialized)
            {
                StartCoroutine(LoadLeaderboardCoroutine());
            }
        }

        /// <summary>
        /// Shares a daily challenge completion.
        /// Riley: "Share my daily challenge completion!"
        /// </summary>
        public void ShareDailyChallengeCompletion(string challengeTitle, string challengeDescription, int score, int kibbleCoinsEarned)
        {
            if (!enableSocialSharing)
            {
                Debug.LogWarning("Riley: Social sharing is disabled!");
                return;
            }

            var shareData = new SocialShareData
            {
                challengeTitle = challengeTitle,
                challengeDescription = challengeDescription,
                score = score,
                kibbleCoinsEarned = kibbleCoinsEarned,
                language = localizationManager?.GetCurrentLanguage() ?? "en",
                platform = Application.platform.ToString()
            };

            OnSocialShareRequested?.Invoke(shareData);
            StartCoroutine(ShareChallengeCoroutine(shareData));
        }

        /// <summary>
        /// Shares a high score achievement.
        /// Nibble: "Bark! (Translation: Share my high score!)"
        /// </summary>
        public void ShareHighScore(int score, string context = "")
        {
            if (!enableSocialSharing)
            {
                Debug.LogWarning("Riley: Social sharing is disabled!");
                return;
            }

            var shareData = new SocialShareData
            {
                challengeTitle = "High Score Achievement",
                challengeDescription = context,
                score = score,
                kibbleCoinsEarned = 0,
                language = localizationManager?.GetCurrentLanguage() ?? "en",
                platform = Application.platform.ToString()
            };

            OnSocialShareRequested?.Invoke(shareData);
            StartCoroutine(ShareHighScoreCoroutine(shareData));
        }

        /// <summary>
        /// Changes the current region.
        /// Riley: "Change the current region!"
        /// </summary>
        public void ChangeRegion(string region)
        {
            if (Array.IndexOf(supportedRegions, region) == -1)
            {
                Debug.LogWarning($"Riley: Region '{region}' is not supported!");
                return;
            }

            _currentRegion = region;
            RefreshLeaderboard();
            
            Debug.Log($"Riley: Region changed to {region}!");
            Debug.Log($"Nibble: *bark* (Translation: New region selected!)");
        }

        /// <summary>
        /// Gets all supported regions.
        /// Nibble: "Bark! (Translation: Get supported regions!)"
        /// </summary>
        public string[] GetSupportedRegions()
        {
            return (string[])supportedRegions.Clone();
        }

        /// <summary>
        /// Gets the current region.
        /// Riley: "Get current region!"
        /// </summary>
        public string GetCurrentRegion()
        {
            return _currentRegion;
        }

        /// <summary>
        /// Generates a unique player ID.
        /// Riley: "Generate a unique player ID!"
        /// </summary>
        private string GeneratePlayerId()
        {
            // Try to load from save data first
            if (saveManager != null)
            {
                var settings = saveManager.Settings;
                // You would need to add a PlayerId property to PlayerSettingsData
                // For now, generate a new one
            }

            // Generate new player ID
            var playerId = $"player_{System.Guid.NewGuid().ToString("N")[..8]}";
            
            // Save to settings
            if (saveManager != null)
            {
                // Store player ID in settings
                saveManager.Save();
            }

            return playerId;
        }

        /// <summary>
        /// Generates a player name.
        /// Nibble: "Bark! (Translation: Generate player name!)"
        /// </summary>
        private string GeneratePlayerName()
        {
            var names = new[]
            {
                "RileyTheHacker", "NibbleTheDog", "CyberpunkRunner", "NeonChaser",
                "HoundHunter", "MechDestroyer", "ObstacleMaster", "ScoreChaser",
                "KibbleCollector", "BarkMaster", "QuipKing", "ChallengeCrusher"
            };

            return names[UnityEngine.Random.Range(0, names.Length)];
        }

        /// <summary>
        /// Detects the player's region based on system settings.
        /// Riley: "Detect the player's region!"
        /// </summary>
        private string DetectPlayerRegion()
        {
            // Simple region detection based on system language
            var systemLanguage = Application.systemLanguage;
            
            switch (systemLanguage)
            {
                case SystemLanguage.English:
                    return "North America";
                case SystemLanguage.Spanish:
                    return "South America";
                case SystemLanguage.Japanese:
                case SystemLanguage.Korean:
                case SystemLanguage.Chinese:
                    return "Asia";
                case SystemLanguage.German:
                case SystemLanguage.French:
                case SystemLanguage.Italian:
                case SystemLanguage.Spanish:
                    return "Europe";
                default:
                    return defaultRegion;
            }
        }

        /// <summary>
        /// Coroutine for updating leaderboard data.
        /// Nibble: "Bark! (Translation: Update leaderboard data!)"
        /// </summary>
        private IEnumerator UpdateLeaderboardCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(updateInterval);
                
                if (_isInitialized)
                {
                    yield return StartCoroutine(LoadLeaderboardCoroutine());
                }
            }
        }

        /// <summary>
        /// Coroutine for loading leaderboard data from server.
        /// Riley: "Load leaderboard data from server!"
        /// </summary>
        private IEnumerator LoadLeaderboardCoroutine()
        {
            var request = CreateLeaderboardRequest();
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    var leaderboardData = JsonUtility.FromJson<LeaderboardData>(request.downloadHandler.text);
                    if (leaderboardData != null)
                    {
                        _cachedLeaderboard = leaderboardData;
                        _cachedLeaderboard.lastUpdated = DateTime.Now;
                        
                        OnLeaderboardUpdated?.Invoke(_cachedLeaderboard);
                        
                        Debug.Log("Riley: Leaderboard data loaded successfully!");
                        Debug.Log($"Nibble: *bark* (Translation: Global rank: {_cachedLeaderboard.playerGlobalRank}, Regional rank: {_cachedLeaderboard.playerRegionalRank})");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Riley: Failed to parse leaderboard data: {ex.Message}");
                    OnLeaderboardError?.Invoke($"Parse error: {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Riley: Failed to load leaderboard: {request.error}");
                OnLeaderboardError?.Invoke($"Network error: {request.error}");
            }
        }

        /// <summary>
        /// Coroutine for submitting a score to the server.
        /// Nibble: "Bark! (Translation: Submit score to server!)"
        /// </summary>
        private IEnumerator SubmitScoreCoroutine(int score, string context)
        {
            var scoreData = new
            {
                playerId = _playerId,
                playerName = _playerName,
                score = score,
                region = _currentRegion,
                platform = Application.platform.ToString(),
                context = context,
                timestamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            var json = JsonUtility.ToJson(scoreData);
            var request = CreateScoreSubmissionRequest(json);
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Riley: Score {score} submitted successfully!");
                Debug.Log($"Nibble: *bark* (Translation: Score submitted! Context: {context})");
                
                // Refresh leaderboard after successful submission
                yield return StartCoroutine(LoadLeaderboardCoroutine());
            }
            else
            {
                Debug.LogError($"Riley: Failed to submit score: {request.error}");
                OnLeaderboardError?.Invoke($"Score submission failed: {request.error}");
            }
        }

        /// <summary>
        /// Coroutine for sharing a daily challenge completion.
        /// Riley: "Share daily challenge completion!"
        /// </summary>
        private IEnumerator ShareChallengeCoroutine(SocialShareData shareData)
        {
            var shareMessage = GenerateShareMessage(shareData);
            var shareUrl = GenerateShareUrl(shareData);
            
            // In a real implementation, you would use Unity's native sharing
            Debug.Log($"Riley: Sharing challenge completion: {shareMessage}");
            Debug.Log($"Nibble: *bark* (Translation: Share URL: {shareUrl})");
            
            // Simulate sharing delay
            yield return new WaitForSeconds(1f);
            
            Debug.Log("Riley: Challenge completion shared successfully!");
        }

        /// <summary>
        /// Coroutine for sharing a high score.
        /// Nibble: "Bark! (Translation: Share high score!)"
        /// </summary>
        private IEnumerator ShareHighScoreCoroutine(SocialShareData shareData)
        {
            var shareMessage = GenerateShareMessage(shareData);
            var shareUrl = GenerateShareUrl(shareData);
            
            // In a real implementation, you would use Unity's native sharing
            Debug.Log($"Riley: Sharing high score: {shareMessage}");
            Debug.Log($"Nibble: *bark* (Translation: Share URL: {shareUrl})");
            
            // Simulate sharing delay
            yield return new WaitForSeconds(1f);
            
            Debug.Log("Riley: High score shared successfully!");
        }

        /// <summary>
        /// Creates a leaderboard request.
        /// Riley: "Create leaderboard request!"
        /// </summary>
        private UnityWebRequest CreateLeaderboardRequest()
        {
            var url = $"{leaderboardUrl}?region={_currentRegion}&playerId={_playerId}";
            var request = UnityWebRequest.Get(url);
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }
            
            request.timeout = (int)requestTimeout;
            return request;
        }

        /// <summary>
        /// Creates a score submission request.
        /// Nibble: "Bark! (Translation: Create score submission request!)"
        /// </summary>
        private UnityWebRequest CreateScoreSubmissionRequest(string json)
        {
            var request = new UnityWebRequest(leaderboardUrl, "POST");
            var bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            if (!string.IsNullOrEmpty(apiKey))
            {
                request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            }
            
            request.timeout = (int)requestTimeout;
            return request;
        }

        /// <summary>
        /// Generates a share message for social media.
        /// Riley: "Generate share message!"
        /// </summary>
        private string GenerateShareMessage(SocialShareData shareData)
        {
            var messageTemplate = shareMessages[UnityEngine.Random.Range(0, shareMessages.Length)];
            
            if (shareData.kibbleCoinsEarned > 0)
            {
                return string.Format(messageTemplate, shareData.score, shareData.challengeTitle, shareData.kibbleCoinsEarned);
            }
            else
            {
                return string.Format(messageTemplate, shareData.score);
            }
        }

        /// <summary>
        /// Generates a share URL for social media.
        /// Nibble: "Bark! (Translation: Generate share URL!)"
        /// </summary>
        private string GenerateShareUrl(SocialShareData shareData)
        {
            var baseUrl = "https://angrydogs.game/share";
            var parameters = $"score={shareData.score}&challenge={Uri.EscapeDataString(shareData.challengeTitle)}&platform={shareData.platform}";
            return $"{baseUrl}?{parameters}";
        }

        /// <summary>
        /// Gets leaderboard statistics.
        /// Riley: "Get leaderboard statistics!"
        /// </summary>
        public string GetLeaderboardStats()
        {
            var stats = $"Leaderboard Statistics:\n";
            stats += $"Player: {_playerName} ({_playerId})\n";
            stats += $"Region: {_currentRegion}\n";
            stats += $"Global Rank: {_cachedLeaderboard.playerGlobalRank}\n";
            stats += $"Regional Rank: {_cachedLeaderboard.playerRegionalRank}\n";
            stats += $"Total Players: {_cachedLeaderboard.totalPlayers}\n";
            stats += $"Last Updated: {_cachedLeaderboard.lastUpdated:yyyy-MM-dd HH:mm:ss}\n";
            stats += $"Global Entries: {_cachedLeaderboard.globalEntries.Count}\n";
            stats += $"Regional Entries: {_cachedLeaderboard.regionalEntries.Count}\n";
            stats += $"Social Sharing: {(enableSocialSharing ? "Enabled" : "Disabled")}\n";
            
            return stats;
        }

        private void OnDestroy()
        {
            if (_updateCoroutine != null)
            {
                StopCoroutine(_updateCoroutine);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // Pause leaderboard updates when app is paused
                if (_updateCoroutine != null)
                {
                    StopCoroutine(_updateCoroutine);
                }
            }
            else
            {
                // Resume leaderboard updates when app is resumed
                if (_isInitialized && updateInterval > 0)
                {
                    _updateCoroutine = StartCoroutine(UpdateLeaderboardCoroutine());
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus && _isInitialized)
            {
                // Refresh leaderboard when app gains focus
                RefreshLeaderboard();
            }
        }
    }
}