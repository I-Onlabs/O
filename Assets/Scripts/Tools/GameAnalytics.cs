using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Game analytics system for tracking player behavior and performance metrics.
    /// Riley: "Gotta track what players are doing! Can't improve without data!"
    /// Nibble: "Bark! (Translation: Track the analytics!)"
    /// </summary>
    public class GameAnalytics : MonoBehaviour
    {
        [System.Serializable]
        public class AnalyticsEvent
        {
            public string eventName;
            public Dictionary<string, object> parameters;
            public DateTime timestamp;
            public string sessionId;
        }

        [System.Serializable]
        public class PlayerSession
        {
            public string sessionId;
            public DateTime startTime;
            public DateTime endTime;
            public float totalPlayTime;
            public int levelReached;
            public int score;
            public int deaths;
            public int powerUpsUsed;
            public string deviceInfo;
            public string platform;
        }

        [Header("Analytics Settings")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enablePerformanceTracking = true;
        [SerializeField] private bool enablePlayerBehaviorTracking = true;
        [SerializeField] private float performanceTrackingInterval = 5f;
        [SerializeField] private int maxEventsPerSession = 1000;

        [Header("Performance Tracking")]
        [SerializeField] private bool trackFPS = true;
        [SerializeField] private bool trackMemory = true;
        [SerializeField] private bool trackDrawCalls = true;
        [SerializeField] private bool trackBatteryUsage = true;

        [Header("Player Behavior Tracking")]
        [SerializeField] private bool trackLevelProgress = true;
        [SerializeField] private bool trackDeaths = true;
        [SerializeField] private bool trackPowerUps = true;
        [SerializeField] private bool trackBossEncounters = true;
        [SerializeField] private bool trackObstacleInteractions = true;

        private PlayerSession _currentSession;
        private Queue<AnalyticsEvent> _eventQueue;
        private bool _isInitialized;
        private float _lastPerformanceTracking;
        private int _eventCount;

        // Events
        public System.Action<AnalyticsEvent> OnEventTracked;
        public System.Action<PlayerSession> OnSessionStarted;
        public System.Action<PlayerSession> OnSessionEnded;

        private void Awake()
        {
            _eventQueue = new Queue<AnalyticsEvent>();
            _currentSession = new PlayerSession();
        }

        private void Start()
        {
            if (enableAnalytics)
            {
                InitializeAnalytics();
            }
        }

        private void Update()
        {
            if (!_isInitialized || !enableAnalytics) return;

            if (enablePerformanceTracking && Time.time - _lastPerformanceTracking >= performanceTrackingInterval)
            {
                TrackPerformanceMetrics();
                _lastPerformanceTracking = Time.time;
            }

            // Process event queue
            ProcessEventQueue();
        }

        /// <summary>
        /// Initializes the analytics system.
        /// Riley: "Initialize the analytics system!"
        /// </summary>
        private void InitializeAnalytics()
        {
            try
            {
                // Initialize Unity Analytics
                Analytics.enabled = enableAnalytics;
                
                // Start new session
                StartNewSession();
                
                _isInitialized = true;
                
                Debug.Log("Riley: Analytics system initialized!");
                Debug.Log("Nibble: *bark* (Translation: Analytics ready!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to initialize analytics: {ex.Message}");
            }
        }

        /// <summary>
        /// Starts a new player session.
        /// Riley: "Start a new session!"
        /// </summary>
        private void StartNewSession()
        {
            _currentSession = new PlayerSession
            {
                sessionId = Guid.NewGuid().ToString(),
                startTime = DateTime.Now,
                deviceInfo = SystemInfo.deviceName,
                platform = Application.platform.ToString()
            };

            OnSessionStarted?.Invoke(_currentSession);
            
            // Track session start
            TrackEvent("session_started", new Dictionary<string, object>
            {
                {"session_id", _currentSession.sessionId},
                {"device_name", _currentSession.deviceInfo},
                {"platform", _currentSession.platform},
                {"timestamp", _currentSession.startTime.ToString()}
            });
        }

        /// <summary>
        /// Ends the current player session.
        /// Nibble: "Bark! (Translation: End the session!)"
        /// </summary>
        public void EndSession()
        {
            if (_currentSession == null) return;

            _currentSession.endTime = DateTime.Now;
            _currentSession.totalPlayTime = (float)(_currentSession.endTime - _currentSession.startTime).TotalSeconds;

            OnSessionEnded?.Invoke(_currentSession);

            // Track session end
            TrackEvent("session_ended", new Dictionary<string, object>
            {
                {"session_id", _currentSession.sessionId},
                {"total_play_time", _currentSession.totalPlayTime},
                {"level_reached", _currentSession.levelReached},
                {"final_score", _currentSession.score},
                {"deaths", _currentSession.deaths},
                {"power_ups_used", _currentSession.powerUpsUsed}
            });

            Debug.Log($"Riley: Session ended - Play time: {_currentSession.totalPlayTime:F1}s, Level: {_currentSession.levelReached}, Score: {_currentSession.score}");
        }

        /// <summary>
        /// Tracks a custom event.
        /// Riley: "Track a custom event!"
        /// </summary>
        public void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (!_isInitialized || !enableAnalytics) return;

            try
            {
                var analyticsEvent = new AnalyticsEvent
                {
                    eventName = eventName,
                    parameters = parameters ?? new Dictionary<string, object>(),
                    timestamp = DateTime.Now,
                    sessionId = _currentSession?.sessionId ?? "unknown"
                };

                // Add to queue
                _eventQueue.Enqueue(analyticsEvent);
                _eventCount++;

                // Track with Unity Analytics
                if (parameters != null)
                {
                    Analytics.CustomEvent(eventName, parameters);
                }
                else
                {
                    Analytics.CustomEvent(eventName);
                }

                OnEventTracked?.Invoke(analyticsEvent);

                // Limit events per session
                if (_eventCount > maxEventsPerSession)
                {
                    Debug.LogWarning("Riley: Maximum events per session reached, some events may be dropped");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to track event {eventName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Tracks level progress.
        /// Riley: "Track level progress!"
        /// </summary>
        public void TrackLevelProgress(int level, float progress, int score)
        {
            if (!trackLevelProgress) return;

            _currentSession.levelReached = Mathf.Max(_currentSession.levelReached, level);
            _currentSession.score = Mathf.Max(_currentSession.score, score);

            TrackEvent("level_progress", new Dictionary<string, object>
            {
                {"level", level},
                {"progress", progress},
                {"score", score},
                {"session_score", _currentSession.score}
            });
        }

        /// <summary>
        /// Tracks player death.
        /// Nibble: "Bark! (Translation: Track death!)"
        /// </summary>
        public void TrackDeath(string cause, Vector3 position, int score)
        {
            if (!trackDeaths) return;

            _currentSession.deaths++;

            TrackEvent("player_death", new Dictionary<string, object>
            {
                {"cause", cause},
                {"position_x", position.x},
                {"position_y", position.y},
                {"position_z", position.z},
                {"score_at_death", score},
                {"total_deaths", _currentSession.deaths}
            });
        }

        /// <summary>
        /// Tracks power-up usage.
        /// Riley: "Track power-up usage!"
        /// </summary>
        public void TrackPowerUpUsed(string powerUpType, Vector3 position, int score)
        {
            if (!trackPowerUps) return;

            _currentSession.powerUpsUsed++;

            TrackEvent("power_up_used", new Dictionary<string, object>
            {
                {"power_up_type", powerUpType},
                {"position_x", position.x},
                {"position_y", position.y},
                {"position_z", position.z},
                {"score", score},
                {"total_power_ups", _currentSession.powerUpsUsed}
            });
        }

        /// <summary>
        /// Tracks boss encounter.
        /// Riley: "Track boss encounter!"
        /// </summary>
        public void TrackBossEncounter(string bossType, string phase, float healthPercentage, int score)
        {
            if (!trackBossEncounters) return;

            TrackEvent("boss_encounter", new Dictionary<string, object>
            {
                {"boss_type", bossType},
                {"phase", phase},
                {"health_percentage", healthPercentage},
                {"score", score}
            });
        }

        /// <summary>
        /// Tracks obstacle interaction.
        /// Nibble: "Bark! (Translation: Track obstacle interaction!)"
        /// </summary>
        public void TrackObstacleInteraction(string obstacleType, string interactionType, Vector3 position, bool successful)
        {
            if (!trackObstacleInteractions) return;

            TrackEvent("obstacle_interaction", new Dictionary<string, object>
            {
                {"obstacle_type", obstacleType},
                {"interaction_type", interactionType},
                {"position_x", position.x},
                {"position_y", position.y},
                {"position_z", position.z},
                {"successful", successful}
            });
        }

        /// <summary>
        /// Tracks performance metrics.
        /// Riley: "Track performance metrics!"
        /// </summary>
        private void TrackPerformanceMetrics()
        {
            var parameters = new Dictionary<string, object>();

            if (trackFPS)
            {
                var fps = 1f / Time.unscaledDeltaTime;
                parameters.Add("fps", fps);
            }

            if (trackMemory)
            {
                var memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) / 1024f / 1024f; // MB
                parameters.Add("memory_usage_mb", memoryUsage);
            }

            if (trackDrawCalls)
            {
                var drawCalls = UnityEngine.Profiling.Profiler.GetCounterValue(UnityEngine.Profiling.Profiler.Area.Rendering, "Draw Calls Count");
                parameters.Add("draw_calls", drawCalls);
            }

            if (trackBatteryUsage && Application.isMobilePlatform)
            {
                var batteryLevel = SystemInfo.batteryLevel;
                var isCharging = SystemInfo.batteryStatus == BatteryStatus.Charging;
                parameters.Add("battery_level", batteryLevel);
                parameters.Add("is_charging", isCharging);
            }

            if (parameters.Count > 0)
            {
                TrackEvent("performance_metrics", parameters);
            }
        }

        /// <summary>
        /// Processes the event queue.
        /// Riley: "Process the event queue!"
        /// </summary>
        private void ProcessEventQueue()
        {
            while (_eventQueue.Count > 0)
            {
                var analyticsEvent = _eventQueue.Dequeue();
                
                // Here you would typically send events to your analytics service
                // For now, we'll just log them
                if (enableAnalytics)
                {
                    Debug.Log($"Riley: Event tracked - {analyticsEvent.eventName} at {analyticsEvent.timestamp}");
                }
            }
        }

        /// <summary>
        /// Tracks UI interaction with enhanced bug logging.
        /// Nibble: "Bark! (Translation: Track UI interaction with bug logging!)"
        /// </summary>
        public void TrackUIInteraction(string uiElement, string action, Dictionary<string, object> additionalData = null)
        {
            var parameters = new Dictionary<string, object>
            {
                {"ui_element", uiElement},
                {"action", action},
                {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                {"session_id", _currentSession?.sessionId ?? "unknown"}
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    parameters.Add(kvp.Key, kvp.Value);
                }
            }

            // Track UI interaction
            TrackEvent("ui_interaction", parameters);
            
            // Log potential UI bugs
            TrackUIBugDetection(uiElement, action, parameters);
        }

        /// <summary>
        /// Tracks UI bug detection for quip toggle failures and other UI issues.
        /// Riley: "Track UI bugs to improve the user experience!"
        /// </summary>
        private void TrackUIBugDetection(string uiElement, string action, Dictionary<string, object> parameters)
        {
            // Check for common UI bugs
            if (uiElement.Contains("quip") && action == "toggle")
            {
                // Track quip toggle interactions
                TrackEvent("ui_quip_toggle", new Dictionary<string, object>
                {
                    {"ui_element", uiElement},
                    {"action", action},
                    {"success", true}, // Assume success unless proven otherwise
                    {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
                });
            }
            
            if (uiElement.Contains("settings") && action == "save")
            {
                // Track settings save interactions
                TrackEvent("ui_settings_save", new Dictionary<string, object>
                {
                    {"ui_element", uiElement},
                    {"action", action},
                    {"success", true},
                    {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
                });
            }
            
            if (uiElement.Contains("language") && action == "change")
            {
                // Track language change interactions
                TrackEvent("ui_language_change", new Dictionary<string, object>
                {
                    {"ui_element", uiElement},
                    {"action", action},
                    {"new_language", parameters.GetValueOrDefault("new_language", "unknown")},
                    {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
                });
            }
        }

        /// <summary>
        /// Tracks save/load operations.
        /// Riley: "Track save/load operations!"
        /// </summary>
        public void TrackSaveLoadOperation(string operation, bool successful, float duration, int dataSize)
        {
            TrackEvent("save_load_operation", new Dictionary<string, object>
            {
                {"operation", operation},
                {"successful", successful},
                {"duration_ms", duration * 1000f},
                {"data_size_bytes", dataSize}
            });
        }

        /// <summary>
        /// Tracks cloud sync operations.
        /// Riley: "Track cloud sync operations!"
        /// </summary>
        public void TrackCloudSyncOperation(string operation, bool successful, float duration, int retryCount)
        {
            TrackEvent("cloud_sync_operation", new Dictionary<string, object>
            {
                {"operation", operation},
                {"successful", successful},
                {"duration_ms", duration * 1000f},
                {"retry_count", retryCount}
            });
        }

        /// <summary>
        /// Tracks build information.
        /// Nibble: "Bark! (Translation: Track build info!)"
        /// </summary>
        public void TrackBuildInfo(string platform, string version, string buildType)
        {
            TrackEvent("build_info", new Dictionary<string, object>
            {
                {"platform", platform},
                {"version", version},
                {"build_type", buildType},
                {"unity_version", Application.unityVersion}
            });
        }

        /// <summary>
        /// Gets current session information.
        /// Riley: "Get current session info!"
        /// </summary>
        public PlayerSession GetCurrentSession()
        {
            return _currentSession;
        }

        /// <summary>
        /// Tracks player retention metrics.
        /// Riley: "Track player retention to see who keeps coming back!"
        /// </summary>
        public void TrackPlayerRetention(string retentionType, Dictionary<string, object> additionalData = null)
        {
            var parameters = new Dictionary<string, object>
            {
                {"retention_type", retentionType},
                {"session_id", _currentSession?.sessionId ?? "unknown"},
                {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                {"play_time", _currentSession?.totalPlayTime ?? 0f},
                {"level_reached", _currentSession?.levelReached ?? 0},
                {"score", _currentSession?.score ?? 0}
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    parameters.Add(kvp.Key, kvp.Value);
                }
            }

            TrackEvent("player_retention", parameters);
        }

        /// <summary>
        /// Tracks daily challenge completion for retention analysis.
        /// Nibble: "Bark! (Translation: Track daily challenge completion!)"
        /// </summary>
        public void TrackDailyChallengeCompletion(string challengeId, string challengeType, int kibbleCoinReward, float completionTime)
        {
            TrackEvent("daily_challenge_completion", new Dictionary<string, object>
            {
                {"challenge_id", challengeId},
                {"challenge_type", challengeType},
                {"kibble_coin_reward", kibbleCoinReward},
                {"completion_time_seconds", completionTime},
                {"session_id", _currentSession?.sessionId ?? "unknown"},
                {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
            });
        }

        /// <summary>
        /// Tracks session length for retention analysis.
        /// Riley: "Track session length to understand player engagement!"
        /// </summary>
        public void TrackSessionLength(float sessionLength, string sessionEndReason)
        {
            TrackEvent("session_length", new Dictionary<string, object>
            {
                {"session_length_seconds", sessionLength},
                {"session_end_reason", sessionEndReason},
                {"session_id", _currentSession?.sessionId ?? "unknown"},
                {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
            });
        }

        /// <summary>
        /// Tracks battery usage for mobile optimization.
        /// Nibble: "Bark! (Translation: Track battery usage!)"
        /// </summary>
        public void TrackBatteryUsage(float batteryLevel, bool isCharging, float batteryDrainRate)
        {
            if (Application.isMobilePlatform)
            {
                TrackEvent("battery_usage", new Dictionary<string, object>
                {
                    {"battery_level", batteryLevel},
                    {"is_charging", isCharging},
                    {"battery_drain_rate", batteryDrainRate},
                    {"session_id", _currentSession?.sessionId ?? "unknown"},
                    {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
                });
            }
        }

        /// <summary>
        /// Tracks localization usage for language preference analysis.
        /// Riley: "Track localization usage to see which languages are popular!"
        /// </summary>
        public void TrackLocalizationUsage(string languageCode, string uiElement, bool isNeonThemed)
        {
            TrackEvent("localization_usage", new Dictionary<string, object>
            {
                {"language_code", languageCode},
                {"ui_element", uiElement},
                {"is_neon_themed", isNeonThemed},
                {"session_id", _currentSession?.sessionId ?? "unknown"},
                {"timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}
            });
        }

        /// <summary>
        /// Exports analytics data for post-launch analysis.
        /// Riley: "Export analytics data for analysis!"
        /// </summary>
        public string ExportAnalyticsData()
        {
            var exportData = new Dictionary<string, object>
            {
                {"session_info", _currentSession},
                {"events_tracked", _eventCount},
                {"events_in_queue", _eventQueue.Count},
                {"export_timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                {"platform", Application.platform.ToString()},
                {"version", Application.version},
                {"device_info", SystemInfo.deviceName}
            };

            return JsonUtility.ToJson(exportData, true);
        }

        /// <summary>
        /// Gets analytics statistics with enhanced retention metrics.
        /// Riley: "Get enhanced analytics stats!"
        /// </summary>
        public string GetAnalyticsStats()
        {
            var stats = $"Analytics Statistics:\n";
            stats += $"Events Tracked: {_eventCount}\n";
            stats += $"Events in Queue: {_eventQueue.Count}\n";
            stats += $"Session Duration: {_currentSession?.totalPlayTime:F1}s\n";
            stats += $"Level Reached: {_currentSession?.levelReached}\n";
            stats += $"Score: {_currentSession?.score}\n";
            stats += $"Deaths: {_currentSession?.deaths}\n";
            stats += $"Power-ups Used: {_currentSession?.powerUpsUsed}\n";
            stats += $"Platform: {Application.platform}\n";
            stats += $"Version: {Application.version}\n";
            stats += $"Device: {SystemInfo.deviceName}\n";
            
            return stats;
        }

        /// <summary>
        /// Enables analytics tracking.
        /// Riley: "Enable analytics!"
        /// </summary>
        public void EnableAnalytics()
        {
            enableAnalytics = true;
            if (!_isInitialized)
            {
                InitializeAnalytics();
            }
            Debug.Log("Riley: Analytics enabled!");
        }

        /// <summary>
        /// Disables analytics tracking.
        /// Nibble: "Bark! (Translation: Disable analytics!)"
        /// </summary>
        public void DisableAnalytics()
        {
            enableAnalytics = false;
            Debug.Log("Riley: Analytics disabled!");
        }

        private void OnDestroy()
        {
            if (_currentSession != null)
            {
                EndSession();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App is being paused, end session
                EndSession();
            }
            else
            {
                // App is being resumed, start new session
                StartNewSession();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // App lost focus, end session
                EndSession();
            }
            else
            {
                // App gained focus, start new session
                StartNewSession();
            }
        }
    }
}