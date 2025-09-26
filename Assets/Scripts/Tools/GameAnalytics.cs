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
        /// Tracks UI interaction.
        /// Nibble: "Bark! (Translation: Track UI interaction!)"
        /// </summary>
        public void TrackUIInteraction(string uiElement, string action, Dictionary<string, object> additionalData = null)
        {
            var parameters = new Dictionary<string, object>
            {
                {"ui_element", uiElement},
                {"action", action}
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    parameters.Add(kvp.Key, kvp.Value);
                }
            }

            TrackEvent("ui_interaction", parameters);
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
        /// Gets analytics statistics.
        /// Riley: "Get analytics stats!"
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