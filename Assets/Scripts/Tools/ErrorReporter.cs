using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Error reporting system for collecting and reporting crashes and errors.
    /// Riley: "Gotta catch all the errors! Can't have crashes when hounds are chasing!"
    /// Nibble: "Bark! (Translation: Report the errors!)"
    /// </summary>
    public class ErrorReporter : MonoBehaviour
    {
        [System.Serializable]
        public class ErrorReport
        {
            public string errorId;
            public string errorType;
            public string errorMessage;
            public string stackTrace;
            public DateTime timestamp;
            public string sessionId;
            public string deviceInfo;
            public string platform;
            public string version;
            public Dictionary<string, object> context;
        }

        [System.Serializable]
        public class CrashReport
        {
            public string crashId;
            public string crashType;
            public string crashMessage;
            public string stackTrace;
            public DateTime timestamp;
            public string sessionId;
            public string deviceInfo;
            public string platform;
            public string version;
            public float playTime;
            public int levelReached;
            public int score;
            public Dictionary<string, object> context;
        }

        [Header("Error Reporting Settings")]
        [SerializeField] private bool enableErrorReporting = true;
        [SerializeField] private bool enableCrashReporting = true;
        [SerializeField] private bool enableLogFile = true;
        [SerializeField] private string logFilePath = "ErrorLog.txt";
        [SerializeField] private int maxLogEntries = 1000;
        [SerializeField] private bool enableRemoteReporting = false;
        [SerializeField] private string remoteReportUrl = "https://your-error-reporting-service.com/api/report";

        [Header("Error Filtering")]
        [SerializeField] private bool filterDuplicateErrors = true;
        [SerializeField] private float duplicateErrorWindow = 60f; // seconds
        [SerializeField] private string[] ignoredErrorTypes = { "Log", "Warning" };

        private Queue<ErrorReport> _errorQueue;
        private Queue<CrashReport> _crashQueue;
        private Dictionary<string, DateTime> _recentErrors;
        private bool _isInitialized;
        private string _sessionId;
        private string _version;

        // Events
        public System.Action<ErrorReport> OnErrorReported;
        public System.Action<CrashReport> OnCrashReported;
        public System.Action<string> OnErrorLogged;

        private void Awake()
        {
            _errorQueue = new Queue<ErrorReport>();
            _crashQueue = new Queue<CrashReport>();
            _recentErrors = new Dictionary<string, DateTime>();
            _sessionId = Guid.NewGuid().ToString();
            _version = Application.version;
        }

        private void Start()
        {
            if (enableErrorReporting)
            {
                InitializeErrorReporting();
            }
        }

        /// <summary>
        /// Initializes the error reporting system.
        /// Riley: "Initialize the error reporting system!"
        /// </summary>
        private void InitializeErrorReporting()
        {
            try
            {
                // Set up Unity's crash reporting
                if (enableCrashReporting)
                {
                    Application.logMessageReceived += OnLogMessageReceived;
                }

                _isInitialized = true;
                
                Debug.Log("Riley: Error reporting system initialized!");
                Debug.Log("Nibble: *bark* (Translation: Error reporting ready!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to initialize error reporting: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles Unity log messages for error reporting.
        /// Riley: "Handle log messages for error reporting!"
        /// </summary>
        private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
        {
            if (!_isInitialized || !enableErrorReporting) return;

            // Filter out ignored error types
            if (Array.Exists(ignoredErrorTypes, t => t == type.ToString()))
            {
                return;
            }

            // Create error report
            var errorReport = new ErrorReport
            {
                errorId = Guid.NewGuid().ToString(),
                errorType = type.ToString(),
                errorMessage = logString,
                stackTrace = stackTrace,
                timestamp = DateTime.Now,
                sessionId = _sessionId,
                deviceInfo = GetDeviceInfo(),
                platform = Application.platform.ToString(),
                version = _version,
                context = GetCurrentContext()
            };

            // Check for duplicate errors
            if (filterDuplicateErrors && IsDuplicateError(errorReport))
            {
                return;
            }

            // Add to queue
            _errorQueue.Enqueue(errorReport);
            _recentErrors[errorReport.errorMessage] = errorReport.timestamp;

            // Report error
            ReportError(errorReport);

            // Log to file if enabled
            if (enableLogFile)
            {
                LogErrorToFile(errorReport);
            }

            OnErrorReported?.Invoke(errorReport);
            OnErrorLogged?.Invoke($"Error reported: {errorReport.errorType} - {errorReport.errorMessage}");
        }

        /// <summary>
        /// Checks if an error is a duplicate.
        /// Riley: "Check if this is a duplicate error!"
        /// </summary>
        private bool IsDuplicateError(ErrorReport errorReport)
        {
            if (!_recentErrors.ContainsKey(errorReport.errorMessage))
            {
                return false;
            }

            var lastOccurrence = _recentErrors[errorReport.errorMessage];
            var timeDifference = (errorReport.timestamp - lastOccurrence).TotalSeconds;
            
            return timeDifference < duplicateErrorWindow;
        }

        /// <summary>
        /// Reports an error.
        /// Riley: "Report an error!"
        /// </summary>
        public void ReportError(ErrorReport errorReport)
        {
            if (!_isInitialized || !enableErrorReporting) return;

            try
            {
                // Add to queue
                _errorQueue.Enqueue(errorReport);

                // Report remotely if enabled
                if (enableRemoteReporting)
                {
                    StartCoroutine(ReportErrorRemotely(errorReport));
                }

                Debug.Log($"Riley: Error reported - {errorReport.errorType}: {errorReport.errorMessage}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to report error: {ex.Message}");
            }
        }

        /// <summary>
        /// Reports a crash.
        /// Nibble: "Bark! (Translation: Report a crash!)"
        /// </summary>
        public void ReportCrash(CrashReport crashReport)
        {
            if (!_isInitialized || !enableCrashReporting) return;

            try
            {
                // Add to queue
                _crashQueue.Enqueue(crashReport);

                // Report remotely if enabled
                if (enableRemoteReporting)
                {
                    StartCoroutine(ReportCrashRemotely(crashReport));
                }

                Debug.Log($"Riley: Crash reported - {crashReport.crashType}: {crashReport.crashMessage}");
                OnCrashReported?.Invoke(crashReport);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to report crash: {ex.Message}");
            }
        }

        /// <summary>
        /// Reports an error remotely.
        /// Riley: "Report error remotely!"
        /// </summary>
        private IEnumerator ReportErrorRemotely(ErrorReport errorReport)
        {
            var json = JsonUtility.ToJson(errorReport);
            var request = new UnityWebRequest(remoteReportUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Riley: Error reported remotely successfully");
            }
            else
            {
                Debug.LogWarning($"Riley: Failed to report error remotely: {request.error}");
            }
        }

        /// <summary>
        /// Reports a crash remotely.
        /// Nibble: "Bark! (Translation: Report crash remotely!)"
        /// </summary>
        private IEnumerator ReportCrashRemotely(CrashReport crashReport)
        {
            var json = JsonUtility.ToJson(crashReport);
            var request = new UnityWebRequest(remoteReportUrl, "POST");
            request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Riley: Crash reported remotely successfully");
            }
            else
            {
                Debug.LogWarning($"Riley: Failed to report crash remotely: {request.error}");
            }
        }

        /// <summary>
        /// Logs an error to file.
        /// Riley: "Log error to file!"
        /// </summary>
        private void LogErrorToFile(ErrorReport errorReport)
        {
            try
            {
                var logEntry = $"[{errorReport.timestamp:yyyy-MM-dd HH:mm:ss}] {errorReport.errorType}: {errorReport.errorMessage}\n" +
                              $"Stack Trace: {errorReport.stackTrace}\n" +
                              $"Session: {errorReport.sessionId}\n" +
                              $"Device: {errorReport.deviceInfo}\n" +
                              $"Platform: {errorReport.platform}\n" +
                              $"Version: {errorReport.version}\n" +
                              $"---\n";

                var logPath = Path.Combine(Application.persistentDataPath, logFilePath);
                File.AppendAllText(logPath, logEntry);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to log error to file: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets device information for error reports.
        /// Riley: "Get device info for error reports!"
        /// </summary>
        private string GetDeviceInfo()
        {
            return $"{SystemInfo.deviceName} ({SystemInfo.deviceModel}) - {SystemInfo.operatingSystem} - " +
                   $"{SystemInfo.processorType} ({SystemInfo.processorCount} cores) - " +
                   $"{SystemInfo.systemMemorySize}MB RAM - {SystemInfo.graphicsDeviceName}";
        }

        /// <summary>
        /// Gets current context for error reports.
        /// Nibble: "Bark! (Translation: Get current context!)"
        /// </summary>
        private Dictionary<string, object> GetCurrentContext()
        {
            var context = new Dictionary<string, object>
            {
                {"scene_name", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name},
                {"time_scale", Time.timeScale},
                {"target_frame_rate", Application.targetFrameRate},
                {"fps", 1f / Time.unscaledDeltaTime},
                {"memory_usage", UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) / 1024f / 1024f},
                {"screen_resolution", $"{Screen.width}x{Screen.height}"},
                {"fullscreen", Screen.fullScreen}
            };

            // Add game-specific context
            var gameAnalytics = FindObjectOfType<GameAnalytics>();
            if (gameAnalytics != null)
            {
                var session = gameAnalytics.GetCurrentSession();
                if (session != null)
                {
                    context.Add("level_reached", session.levelReached);
                    context.Add("score", session.score);
                    context.Add("deaths", session.deaths);
                    context.Add("play_time", session.totalPlayTime);
                }
            }

            return context;
        }

        /// <summary>
        /// Creates a crash report.
        /// Riley: "Create a crash report!"
        /// </summary>
        public CrashReport CreateCrashReport(string crashType, string crashMessage, string stackTrace)
        {
            var crashReport = new CrashReport
            {
                crashId = Guid.NewGuid().ToString(),
                crashType = crashType,
                crashMessage = crashMessage,
                stackTrace = stackTrace,
                timestamp = DateTime.Now,
                sessionId = _sessionId,
                deviceInfo = GetDeviceInfo(),
                platform = Application.platform.ToString(),
                version = _version,
                context = GetCurrentContext()
            };

            // Add game-specific data
            var gameAnalytics = FindObjectOfType<GameAnalytics>();
            if (gameAnalytics != null)
            {
                var session = gameAnalytics.GetCurrentSession();
                if (session != null)
                {
                    crashReport.playTime = session.totalPlayTime;
                    crashReport.levelReached = session.levelReached;
                    crashReport.score = session.score;
                }
            }

            return crashReport;
        }

        /// <summary>
        /// Handles application crashes.
        /// Riley: "Handle application crashes!"
        /// </summary>
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // App is being paused, check if it's a crash
                var crashReport = CreateCrashReport("Application Pause", "Application was paused unexpectedly", "");
                ReportCrash(crashReport);
            }
        }

        /// <summary>
        /// Handles application focus loss.
        /// Nibble: "Bark! (Translation: Handle focus loss!)"
        /// </summary>
        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                // App lost focus, check if it's a crash
                var crashReport = CreateCrashReport("Application Focus Loss", "Application lost focus unexpectedly", "");
                ReportCrash(crashReport);
            }
        }

        /// <summary>
        /// Gets error statistics.
        /// Riley: "Get error statistics!"
        /// </summary>
        public string GetErrorStats()
        {
            var stats = $"Error Reporting Statistics:\n";
            stats += $"Errors Reported: {_errorQueue.Count}\n";
            stats += $"Crashes Reported: {_crashQueue.Count}\n";
            stats += $"Session ID: {_sessionId}\n";
            stats += $"Version: {_version}\n";
            stats += $"Device: {GetDeviceInfo()}\n";
            stats += $"Platform: {Application.platform}\n";
            
            return stats;
        }

        /// <summary>
        /// Clears error queue.
        /// Riley: "Clear error queue!"
        /// </summary>
        public void ClearErrorQueue()
        {
            _errorQueue.Clear();
            _crashQueue.Clear();
            _recentErrors.Clear();
            Debug.Log("Riley: Error queue cleared!");
        }

        /// <summary>
        /// Exports error logs to file.
        /// Nibble: "Bark! (Translation: Export error logs!)"
        /// </summary>
        public void ExportErrorLogs(string filePath)
        {
            try
            {
                var logContent = $"Error Report Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                logContent += $"Session ID: {_sessionId}\n";
                logContent += $"Version: {_version}\n";
                logContent += $"Device: {GetDeviceInfo()}\n";
                logContent += $"Platform: {Application.platform}\n";
                logContent += $"---\n\n";

                foreach (var error in _errorQueue)
                {
                    logContent += $"[{error.timestamp:yyyy-MM-dd HH:mm:ss}] {error.errorType}: {error.errorMessage}\n";
                    logContent += $"Stack Trace: {error.stackTrace}\n";
                    logContent += $"---\n";
                }

                File.WriteAllText(filePath, logContent);
                Debug.Log($"Riley: Error logs exported to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to export error logs: {ex.Message}");
            }
        }

        /// <summary>
        /// Enables error reporting.
        /// Riley: "Enable error reporting!"
        /// </summary>
        public void EnableErrorReporting()
        {
            enableErrorReporting = true;
            if (!_isInitialized)
            {
                InitializeErrorReporting();
            }
            Debug.Log("Riley: Error reporting enabled!");
        }

        /// <summary>
        /// Disables error reporting.
        /// Nibble: "Bark! (Translation: Disable error reporting!)"
        /// </summary>
        public void DisableErrorReporting()
        {
            enableErrorReporting = false;
            Debug.Log("Riley: Error reporting disabled!");
        }

        private void OnDestroy()
        {
            if (enableCrashReporting)
            {
                Application.logMessageReceived -= OnLogMessageReceived;
            }
        }
    }
}