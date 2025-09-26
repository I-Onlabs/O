using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Performance profiler for monitoring game performance across different platforms.
    /// Riley: "Gotta keep track of performance! Can't have lag when the hounds are chasing!"
    /// Nibble: "Bark! (Translation: Monitor the performance!)"
    /// </summary>
    public class PerformanceProfiler : MonoBehaviour
    {
        [System.Serializable]
        public class PerformanceMetrics
        {
            [Header("Frame Rate")]
            public float currentFPS;
            public float averageFPS;
            public float minFPS;
            public float maxFPS;
            
            [Header("Memory Usage")]
            public long totalMemory;
            public long usedMemory;
            public long freeMemory;
            public int gcCollections;
            
            [Header("Rendering")]
            public int drawCalls;
            public int triangles;
            public int vertices;
            public int batches;
            
            [Header("Physics")]
            public int activeRigidbodies;
            public int activeColliders;
            public int physicsContacts;
            
            [Header("Audio")]
            public int activeAudioSources;
            public float audioMemoryUsage;
            
            [Header("Mobile Specific")]
            public float batteryLevel;
            public float temperature;
            public bool isCharging;
        }

        [Header("Profiling Settings")]
        [SerializeField] private bool enableProfiling = true;
        [SerializeField] private float updateInterval = 0.5f;
        [SerializeField] private bool logToFile = true;
        [SerializeField] private string logFilePath = "PerformanceLog.txt";
        [SerializeField] private bool showOnScreen = true;
        [SerializeField] private int maxLogEntries = 1000;

        [Header("Performance Thresholds")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float warningFPS = 45f;
        [SerializeField] private float criticalFPS = 30f;
        [SerializeField] private long maxMemoryMB = 1024;
        [SerializeField] private int maxDrawCalls = 100;

        private PerformanceMetrics _currentMetrics;
        private Queue<PerformanceMetrics> _metricsHistory;
        private float _lastUpdateTime;
        private bool _isProfiling;
        private GUIStyle _guiStyle;

        // Events
        public System.Action<PerformanceMetrics> OnPerformanceUpdate;
        public System.Action<string> OnPerformanceWarning;
        public System.Action<string> OnPerformanceCritical;

        private void Awake()
        {
            _currentMetrics = new PerformanceMetrics();
            _metricsHistory = new Queue<PerformanceMetrics>();
            _isProfiling = enableProfiling;
            
            if (showOnScreen)
            {
                InitializeGUI();
            }
        }

        private void Start()
        {
            if (_isProfiling)
            {
                StartCoroutine(ProfilingLoop());
            }
        }

        /// <summary>
        /// Initializes GUI style for on-screen display.
        /// Riley: "Time to set up the performance display!"
        /// </summary>
        private void InitializeGUI()
        {
            _guiStyle = new GUIStyle();
            _guiStyle.fontSize = 16;
            _guiStyle.normal.textColor = Color.white;
            _guiStyle.alignment = TextAnchor.UpperLeft;
        }

        /// <summary>
        /// Main profiling loop that collects performance metrics.
        /// Nibble: "Bark! (Translation: Start profiling the performance!)"
        /// </summary>
        private IEnumerator ProfilingLoop()
        {
            while (_isProfiling)
            {
                CollectPerformanceMetrics();
                CheckPerformanceThresholds();
                UpdateMetricsHistory();
                
                OnPerformanceUpdate?.Invoke(_currentMetrics);
                
                if (logToFile)
                {
                    LogMetricsToFile();
                }
                
                yield return new WaitForSeconds(updateInterval);
            }
        }

        /// <summary>
        /// Collects current performance metrics.
        /// Riley: "Time to gather all the performance data!"
        /// </summary>
        private void CollectPerformanceMetrics()
        {
            // Frame rate metrics
            _currentMetrics.currentFPS = 1f / Time.unscaledDeltaTime;
            _currentMetrics.averageFPS = CalculateAverageFPS();
            _currentMetrics.minFPS = CalculateMinFPS();
            _currentMetrics.maxFPS = CalculateMaxFPS();

            // Memory metrics
            _currentMetrics.totalMemory = System.GC.GetTotalMemory(false);
            _currentMetrics.usedMemory = Profiler.GetTotalAllocatedMemory(Profiler.Area.All);
            _currentMetrics.freeMemory = _currentMetrics.totalMemory - _currentMetrics.usedMemory;
            _currentMetrics.gcCollections = System.GC.CollectionCount(0);

            // Rendering metrics
            _currentMetrics.drawCalls = GetDrawCalls();
            _currentMetrics.triangles = GetTriangleCount();
            _currentMetrics.vertices = GetVertexCount();
            _currentMetrics.batches = GetBatchCount();

            // Physics metrics
            _currentMetrics.activeRigidbodies = GetActiveRigidbodyCount();
            _currentMetrics.activeColliders = GetActiveColliderCount();
            _currentMetrics.physicsContacts = GetPhysicsContactCount();

            // Audio metrics
            _currentMetrics.activeAudioSources = GetActiveAudioSourceCount();
            _currentMetrics.audioMemoryUsage = GetAudioMemoryUsage();

            // Mobile-specific metrics
            if (Application.isMobilePlatform)
            {
                _currentMetrics.batteryLevel = GetBatteryLevel();
                _currentMetrics.temperature = GetDeviceTemperature();
                _currentMetrics.isCharging = IsDeviceCharging();
            }
        }

        /// <summary>
        /// Calculates average FPS from recent measurements.
        /// Riley: "Gotta calculate that average FPS!"
        /// </summary>
        private float CalculateAverageFPS()
        {
            if (_metricsHistory.Count == 0) return _currentMetrics.currentFPS;
            
            float totalFPS = 0f;
            foreach (var metrics in _metricsHistory)
            {
                totalFPS += metrics.currentFPS;
            }
            
            return totalFPS / _metricsHistory.Count;
        }

        /// <summary>
        /// Calculates minimum FPS from recent measurements.
        /// Nibble: "Bark! (Translation: Find the lowest FPS!)"
        /// </summary>
        private float CalculateMinFPS()
        {
            if (_metricsHistory.Count == 0) return _currentMetrics.currentFPS;
            
            float minFPS = float.MaxValue;
            foreach (var metrics in _metricsHistory)
            {
                if (metrics.currentFPS < minFPS)
                    minFPS = metrics.currentFPS;
            }
            
            return minFPS;
        }

        /// <summary>
        /// Calculates maximum FPS from recent measurements.
        /// Riley: "Find the highest FPS!"
        /// </summary>
        private float CalculateMaxFPS()
        {
            if (_metricsHistory.Count == 0) return _currentMetrics.currentFPS;
            
            float maxFPS = 0f;
            foreach (var metrics in _metricsHistory)
            {
                if (metrics.currentFPS > maxFPS)
                    maxFPS = metrics.currentFPS;
            }
            
            return maxFPS;
        }

        /// <summary>
        /// Gets current draw call count.
        /// Riley: "Count those draw calls!"
        /// </summary>
        private int GetDrawCalls()
        {
            return Profiler.GetCounterValue(Profiler.Area.Rendering, "Draw Calls Count");
        }

        /// <summary>
        /// Gets current triangle count.
        /// Nibble: "Bark! (Translation: Count the triangles!)"
        /// </summary>
        private int GetTriangleCount()
        {
            return Profiler.GetCounterValue(Profiler.Area.Rendering, "Triangles Count");
        }

        /// <summary>
        /// Gets current vertex count.
        /// Riley: "Count those vertices!"
        /// </summary>
        private int GetVertexCount()
        {
            return Profiler.GetCounterValue(Profiler.Area.Rendering, "Vertices Count");
        }

        /// <summary>
        /// Gets current batch count.
        /// Riley: "Count the batches!"
        /// </summary>
        private int GetBatchCount()
        {
            return Profiler.GetCounterValue(Profiler.Area.Rendering, "Batches Count");
        }

        /// <summary>
        /// Gets active rigidbody count.
        /// Nibble: "Bark! (Translation: Count the rigidbodies!)"
        /// </summary>
        private int GetActiveRigidbodyCount()
        {
            return FindObjectsOfType<Rigidbody>().Length;
        }

        /// <summary>
        /// Gets active collider count.
        /// Riley: "Count the colliders!"
        /// </summary>
        private int GetActiveColliderCount()
        {
            return FindObjectsOfType<Collider>().Length;
        }

        /// <summary>
        /// Gets physics contact count.
        /// Riley: "Count the physics contacts!"
        /// </summary>
        private int GetPhysicsContactCount()
        {
            // This would need to be implemented with a custom physics contact tracker
            return 0;
        }

        /// <summary>
        /// Gets active audio source count.
        /// Nibble: "Bark! (Translation: Count the audio sources!)"
        /// </summary>
        private int GetActiveAudioSourceCount()
        {
            return FindObjectsOfType<AudioSource>().Length;
        }

        /// <summary>
        /// Gets audio memory usage.
        /// Riley: "Calculate audio memory usage!"
        /// </summary>
        private float GetAudioMemoryUsage()
        {
            return Profiler.GetTotalAllocatedMemory(Profiler.Area.Audio) / 1024f / 1024f; // MB
        }

        /// <summary>
        /// Gets battery level on mobile devices.
        /// Riley: "Check the battery level!"
        /// </summary>
        private float GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }

        /// <summary>
        /// Gets device temperature (simulated).
        /// Nibble: "Bark! (Translation: Check the temperature!)"
        /// </summary>
        private float GetDeviceTemperature()
        {
            // This would need platform-specific implementation
            return 25f + UnityEngine.Random.Range(-5f, 5f);
        }

        /// <summary>
        /// Checks if device is charging.
        /// Riley: "Check if the device is charging!"
        /// </summary>
        private bool IsDeviceCharging()
        {
            return SystemInfo.batteryStatus == BatteryStatus.Charging;
        }

        /// <summary>
        /// Checks performance against thresholds and triggers warnings.
        /// Riley: "Check if performance is within acceptable limits!"
        /// </summary>
        private void CheckPerformanceThresholds()
        {
            // FPS checks
            if (_currentMetrics.currentFPS < criticalFPS)
            {
                var message = $"Critical FPS: {_currentMetrics.currentFPS:F1} (Target: {targetFPS})";
                OnPerformanceCritical?.Invoke(message);
                Debug.LogError($"Riley: {message}");
            }
            else if (_currentMetrics.currentFPS < warningFPS)
            {
                var message = $"Warning FPS: {_currentMetrics.currentFPS:F1} (Target: {targetFPS})";
                OnPerformanceWarning?.Invoke(message);
                Debug.LogWarning($"Riley: {message}");
            }

            // Memory checks
            var memoryMB = _currentMetrics.usedMemory / 1024f / 1024f;
            if (memoryMB > maxMemoryMB)
            {
                var message = $"High memory usage: {memoryMB:F1}MB (Max: {maxMemoryMB}MB)";
                OnPerformanceWarning?.Invoke(message);
                Debug.LogWarning($"Riley: {message}");
            }

            // Draw call checks
            if (_currentMetrics.drawCalls > maxDrawCalls)
            {
                var message = $"High draw calls: {_currentMetrics.drawCalls} (Max: {maxDrawCalls})";
                OnPerformanceWarning?.Invoke(message);
                Debug.LogWarning($"Riley: {message}");
            }
        }

        /// <summary>
        /// Updates metrics history with current metrics.
        /// Nibble: "Bark! (Translation: Update the history!)"
        /// </summary>
        private void UpdateMetricsHistory()
        {
            _metricsHistory.Enqueue(_currentMetrics);
            
            if (_metricsHistory.Count > maxLogEntries)
            {
                _metricsHistory.Dequeue();
            }
        }

        /// <summary>
        /// Logs metrics to file.
        /// Riley: "Log the performance data to file!"
        /// </summary>
        private void LogMetricsToFile()
        {
            try
            {
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}," +
                              $"FPS:{_currentMetrics.currentFPS:F1}," +
                              $"Memory:{_currentMetrics.usedMemory / 1024f / 1024f:F1}MB," +
                              $"DrawCalls:{_currentMetrics.drawCalls}," +
                              $"Triangles:{_currentMetrics.triangles}\n";
                
                var logPath = System.IO.Path.Combine(Application.persistentDataPath, logFilePath);
                System.IO.File.AppendAllText(logPath, logEntry);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to log metrics: {ex.Message}");
            }
        }

        /// <summary>
        /// OnGUI for on-screen performance display.
        /// Riley: "Display performance on screen!"
        /// </summary>
        private void OnGUI()
        {
            if (!showOnScreen || _guiStyle == null) return;

            var rect = new Rect(10, 10, 300, 200);
            var content = $"FPS: {_currentMetrics.currentFPS:F1}\n" +
                         $"Avg FPS: {_currentMetrics.averageFPS:F1}\n" +
                         $"Min FPS: {_currentMetrics.minFPS:F1}\n" +
                         $"Max FPS: {_currentMetrics.maxFPS:F1}\n" +
                         $"Memory: {_currentMetrics.usedMemory / 1024f / 1024f:F1}MB\n" +
                         $"Draw Calls: {_currentMetrics.drawCalls}\n" +
                         $"Triangles: {_currentMetrics.triangles}\n" +
                         $"Vertices: {_currentMetrics.vertices}\n" +
                         $"Batches: {_currentMetrics.batches}\n" +
                         $"Rigidbodies: {_currentMetrics.activeRigidbodies}\n" +
                         $"Colliders: {_currentMetrics.activeColliders}\n" +
                         $"Audio Sources: {_currentMetrics.activeAudioSources}";

            if (Application.isMobilePlatform)
            {
                content += $"\nBattery: {_currentMetrics.batteryLevel * 100f:F0}%\n" +
                          $"Charging: {_currentMetrics.isCharging}\n" +
                          $"Temperature: {_currentMetrics.temperature:F1}°C";
            }

            GUI.Label(rect, content, _guiStyle);
        }

        /// <summary>
        /// Starts performance profiling.
        /// Riley: "Start profiling the performance!"
        /// </summary>
        public void StartProfiling()
        {
            _isProfiling = true;
            if (!IsInvoking(nameof(ProfilingLoop)))
            {
                StartCoroutine(ProfilingLoop());
            }
        }

        /// <summary>
        /// Stops performance profiling.
        /// Nibble: "Bark! (Translation: Stop profiling!)"
        /// </summary>
        public void StopProfiling()
        {
            _isProfiling = false;
        }

        /// <summary>
        /// Gets current performance metrics.
        /// Riley: "Get the current performance data!"
        /// </summary>
        public PerformanceMetrics GetCurrentMetrics()
        {
            return _currentMetrics;
        }

        /// <summary>
        /// Gets performance metrics history.
        /// Riley: "Get the performance history!"
        /// </summary>
        public Queue<PerformanceMetrics> GetMetricsHistory()
        {
            return _metricsHistory;
        }

        /// <summary>
        /// Exports performance data to CSV.
        /// Riley: "Export the performance data!"
        /// </summary>
        public void ExportToCSV(string filePath)
        {
            try
            {
                var csv = "Timestamp,FPS,AvgFPS,MinFPS,MaxFPS,MemoryMB,DrawCalls,Triangles,Vertices,Batches,Rigidbodies,Colliders,AudioSources\n";
                
                foreach (var metrics in _metricsHistory)
                {
                    csv += $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}," +
                          $"{metrics.currentFPS:F1}," +
                          $"{metrics.averageFPS:F1}," +
                          $"{metrics.minFPS:F1}," +
                          $"{metrics.maxFPS:F1}," +
                          $"{metrics.usedMemory / 1024f / 1024f:F1}," +
                          $"{metrics.drawCalls}," +
                          $"{metrics.triangles}," +
                          $"{metrics.vertices}," +
                          $"{metrics.batches}," +
                          $"{metrics.activeRigidbodies}," +
                          $"{metrics.activeColliders}," +
                          $"{metrics.activeAudioSources}\n";
                }
                
                System.IO.File.WriteAllText(filePath, csv);
                Debug.Log($"Riley: Performance data exported to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to export performance data: {ex.Message}");
            }
        }

        private void OnDestroy()
        {
            StopProfiling();
        }
    }
}