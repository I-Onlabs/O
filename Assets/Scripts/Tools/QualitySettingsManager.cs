using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Quality settings manager for dynamic quality adjustment based on device performance.
    /// Riley: "Gotta adjust quality based on device performance! Can't have lag when hounds are chasing!"
    /// Nibble: "Bark! (Translation: Adjust quality for smooth gameplay!)"
    /// </summary>
    public class QualitySettingsManager : MonoBehaviour
    {
        [System.Serializable]
        public class QualityLevel
        {
            [Header("Quality Level Info")]
            public string name = "Custom";
            public int level = 0;
            public bool isMobile = false;
            
            [Header("Rendering Settings")]
            public int pixelLightCount = 4;
            public int textureQuality = 0; // 0 = Full Res, 1 = Half Res, 2 = Quarter Res
            public int antiAliasing = 0; // 0 = Disabled, 1 = 2x, 2 = 4x, 3 = 8x
            public int anisotropicFiltering = 1; // 0 = Disabled, 1 = Per Texture, 2 = Forced On
            public int shadowResolution = 2; // 0 = Low, 1 = Medium, 2 = High, 3 = Very High
            public int shadowCascades = 2; // 0 = No Cascades, 1 = Two Cascades, 2 = Four Cascades
            public float shadowDistance = 50f;
            public int shadowProjection = 0; // 0 = Close Fit, 1 = Stable Fit
            public int vSyncCount = 0; // 0 = Don't Sync, 1 = Every V Blank, 2 = Every Second V Blank
            public int lodBias = 1;
            public int maximumLODLevel = 0;
            public int particleRaycastBudget = 256;
            public int softVegetation = 0; // 0 = Disabled, 1 = Enabled
            public int realtimeReflectionProbes = 0; // 0 = Disabled, 1 = Enabled
            public int billboardsFaceCameraPosition = 0; // 0 = Disabled, 1 = Enabled
            
            [Header("Mobile Specific")]
            public bool useHDR = false;
            public bool useMSAA = false;
            public int maxTextureSize = 1024;
            public bool reduceDrawCalls = true;
            public bool optimizeForBattery = true;
        }

        [Header("Quality Levels")]
        [SerializeField] private QualityLevel[] qualityLevels = new QualityLevel[5];
        [SerializeField] private int currentQualityLevel = 2;
        [SerializeField] private bool autoAdjustQuality = true;
        [SerializeField] private float performanceCheckInterval = 5f;
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float minFPS = 45f;

        [Header("Performance Monitoring")]
        [SerializeField] private bool enablePerformanceMonitoring = true;
        [SerializeField] private int performanceHistorySize = 10;
        [SerializeField] private float performanceThreshold = 0.8f;

        private float[] _fpsHistory;
        private int _fpsHistoryIndex;
        private float _lastPerformanceCheck;
        private bool _isInitialized;
        private PerformanceProfiler _performanceProfiler;

        // Events
        public System.Action<int> OnQualityLevelChanged;
        public System.Action<string> OnQualityAdjustment;
        public System.Action<float> OnPerformanceWarning;

        private void Awake()
        {
            InitializeQualityLevels();
            _fpsHistory = new float[performanceHistorySize];
            _fpsHistoryIndex = 0;
        }

        private void Start()
        {
            _performanceProfiler = FindObjectOfType<PerformanceProfiler>();
            if (_performanceProfiler == null)
            {
                _performanceProfiler = gameObject.AddComponent<PerformanceProfiler>();
            }

            ApplyQualityLevel(currentQualityLevel);
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized || !autoAdjustQuality) return;

            if (Time.time - _lastPerformanceCheck >= performanceCheckInterval)
            {
                CheckPerformanceAndAdjustQuality();
                _lastPerformanceCheck = Time.time;
            }
        }

        /// <summary>
        /// Initializes default quality levels.
        /// Riley: "Time to set up the quality levels!"
        /// </summary>
        private void InitializeQualityLevels()
        {
            // Ultra Quality
            qualityLevels[0] = new QualityLevel
            {
                name = "Ultra",
                level = 0,
                isMobile = false,
                pixelLightCount = 8,
                textureQuality = 0,
                antiAliasing = 3,
                anisotropicFiltering = 2,
                shadowResolution = 3,
                shadowCascades = 2,
                shadowDistance = 100f,
                shadowProjection = 0,
                vSyncCount = 0,
                lodBias = 2,
                maximumLODLevel = 0,
                particleRaycastBudget = 1024,
                softVegetation = 1,
                realtimeReflectionProbes = 1,
                billboardsFaceCameraPosition = 1,
                useHDR = true,
                useMSAA = true,
                maxTextureSize = 4096,
                reduceDrawCalls = false,
                optimizeForBattery = false
            };

            // High Quality
            qualityLevels[1] = new QualityLevel
            {
                name = "High",
                level = 1,
                isMobile = false,
                pixelLightCount = 6,
                textureQuality = 0,
                antiAliasing = 2,
                anisotropicFiltering = 2,
                shadowResolution = 2,
                shadowCascades = 2,
                shadowDistance = 75f,
                shadowProjection = 0,
                vSyncCount = 0,
                lodBias = 1,
                maximumLODLevel = 0,
                particleRaycastBudget = 512,
                softVegetation = 1,
                realtimeReflectionProbes = 1,
                billboardsFaceCameraPosition = 1,
                useHDR = true,
                useMSAA = false,
                maxTextureSize = 2048,
                reduceDrawCalls = false,
                optimizeForBattery = false
            };

            // Medium Quality
            qualityLevels[2] = new QualityLevel
            {
                name = "Medium",
                level = 2,
                isMobile = false,
                pixelLightCount = 4,
                textureQuality = 0,
                antiAliasing = 1,
                anisotropicFiltering = 1,
                shadowResolution = 1,
                shadowCascades = 1,
                shadowDistance = 50f,
                shadowProjection = 1,
                vSyncCount = 0,
                lodBias = 1,
                maximumLODLevel = 0,
                particleRaycastBudget = 256,
                softVegetation = 0,
                realtimeReflectionProbes = 0,
                billboardsFaceCameraPosition = 0,
                useHDR = false,
                useMSAA = false,
                maxTextureSize = 1024,
                reduceDrawCalls = true,
                optimizeForBattery = false
            };

            // Low Quality
            qualityLevels[3] = new QualityLevel
            {
                name = "Low",
                level = 3,
                isMobile = true,
                pixelLightCount = 2,
                textureQuality = 1,
                antiAliasing = 0,
                anisotropicFiltering = 0,
                shadowResolution = 0,
                shadowCascades = 0,
                shadowDistance = 25f,
                shadowProjection = 1,
                vSyncCount = 0,
                lodBias = 0,
                maximumLODLevel = 1,
                particleRaycastBudget = 128,
                softVegetation = 0,
                realtimeReflectionProbes = 0,
                billboardsFaceCameraPosition = 0,
                useHDR = false,
                useMSAA = false,
                maxTextureSize = 512,
                reduceDrawCalls = true,
                optimizeForBattery = true
            };

            // Mobile Quality
            qualityLevels[4] = new QualityLevel
            {
                name = "Mobile",
                level = 4,
                isMobile = true,
                pixelLightCount = 1,
                textureQuality = 2,
                antiAliasing = 0,
                anisotropicFiltering = 0,
                shadowResolution = 0,
                shadowCascades = 0,
                shadowDistance = 15f,
                shadowProjection = 1,
                vSyncCount = 0,
                lodBias = 0,
                maximumLODLevel = 2,
                particleRaycastBudget = 64,
                softVegetation = 0,
                realtimeReflectionProbes = 0,
                billboardsFaceCameraPosition = 0,
                useHDR = false,
                useMSAA = false,
                maxTextureSize = 256,
                reduceDrawCalls = true,
                optimizeForBattery = true
            };
        }

        /// <summary>
        /// Checks performance and adjusts quality if needed.
        /// Riley: "Check performance and adjust quality!"
        /// </summary>
        private void CheckPerformanceAndAdjustQuality()
        {
            if (!enablePerformanceMonitoring) return;

            var currentFPS = 1f / Time.unscaledDeltaTime;
            _fpsHistory[_fpsHistoryIndex] = currentFPS;
            _fpsHistoryIndex = (_fpsHistoryIndex + 1) % performanceHistorySize;

            var averageFPS = CalculateAverageFPS();
            
            if (averageFPS < minFPS && currentQualityLevel < qualityLevels.Length - 1)
            {
                // Performance is poor, reduce quality
                SetQualityLevel(currentQualityLevel + 1);
                OnPerformanceWarning?.Invoke($"Performance warning: FPS {averageFPS:F1}, reducing quality to {qualityLevels[currentQualityLevel].name}");
                Debug.LogWarning($"Riley: Performance warning! FPS: {averageFPS:F1}, reducing quality to {qualityLevels[currentQualityLevel].name}");
            }
            else if (averageFPS > targetFPS && currentQualityLevel > 0)
            {
                // Performance is good, try to increase quality
                var newLevel = currentQualityLevel - 1;
                if (CanIncreaseQuality(newLevel))
                {
                    SetQualityLevel(newLevel);
                    OnQualityAdjustment?.Invoke($"Performance good: FPS {averageFPS:F1}, increasing quality to {qualityLevels[currentQualityLevel].name}");
                    Debug.Log($"Riley: Performance good! FPS: {averageFPS:F1}, increasing quality to {qualityLevels[currentQualityLevel].name}");
                }
            }
        }

        /// <summary>
        /// Calculates average FPS from history.
        /// Nibble: "Bark! (Translation: Calculate average FPS!)"
        /// </summary>
        private float CalculateAverageFPS()
        {
            float total = 0f;
            int count = 0;
            
            for (int i = 0; i < performanceHistorySize; i++)
            {
                if (_fpsHistory[i] > 0f)
                {
                    total += _fpsHistory[i];
                    count++;
                }
            }
            
            return count > 0 ? total / count : 0f;
        }

        /// <summary>
        /// Checks if quality can be increased.
        /// Riley: "Can we increase quality?"
        /// </summary>
        private bool CanIncreaseQuality(int newLevel)
        {
            if (newLevel < 0 || newLevel >= qualityLevels.Length) return false;
            
            var newQuality = qualityLevels[newLevel];
            
            // Don't increase quality if it would be too high for mobile
            if (Application.isMobilePlatform && !newQuality.isMobile)
            {
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Sets the quality level.
        /// Riley: "Set the quality level!"
        /// </summary>
        public void SetQualityLevel(int level)
        {
            if (level < 0 || level >= qualityLevels.Length)
            {
                Debug.LogWarning($"Riley: Invalid quality level {level}");
                return;
            }

            currentQualityLevel = level;
            ApplyQualityLevel(level);
            OnQualityLevelChanged?.Invoke(level);
            
            Debug.Log($"Nibble: *bark* (Translation: Quality set to {qualityLevels[level].name}!)");
        }

        /// <summary>
        /// Applies quality level settings.
        /// Riley: "Apply the quality settings!"
        /// </summary>
        private void ApplyQualityLevel(int level)
        {
            var quality = qualityLevels[level];
            
            // Apply Unity Quality Settings
            QualitySettings.pixelLightCount = quality.pixelLightCount;
            QualitySettings.masterTextureLimit = quality.textureQuality;
            QualitySettings.antiAliasing = (int)Mathf.Pow(2, quality.antiAliasing);
            QualitySettings.anisotropicFiltering = (AnisotropicFiltering)quality.anisotropicFiltering;
            QualitySettings.shadowResolution = (ShadowResolution)quality.shadowResolution;
            QualitySettings.shadowCascades = quality.shadowCascades;
            QualitySettings.shadowDistance = quality.shadowDistance;
            QualitySettings.shadowProjection = (ShadowProjection)quality.shadowProjection;
            QualitySettings.vSyncCount = quality.vSyncCount;
            QualitySettings.lodBias = quality.lodBias;
            QualitySettings.maximumLODLevel = quality.maximumLODLevel;
            QualitySettings.particleRaycastBudget = quality.particleRaycastBudget;
            QualitySettings.softVegetation = quality.softVegetation == 1;
            QualitySettings.realtimeReflectionProbes = quality.realtimeReflectionProbes == 1;
            QualitySettings.billboardsFaceCameraPosition = quality.billboardsFaceCameraPosition == 1;

            // Apply mobile-specific settings
            if (Application.isMobilePlatform)
            {
                ApplyMobileSettings(quality);
            }

            // Apply custom optimizations
            ApplyCustomOptimizations(quality);
        }

        /// <summary>
        /// Applies mobile-specific settings.
        /// Nibble: "Bark! (Translation: Apply mobile settings!)"
        /// </summary>
        private void ApplyMobileSettings(QualityLevel quality)
        {
            // Disable HDR on mobile if not supported
            if (!quality.useHDR)
            {
                Camera.main.allowHDR = false;
            }

            // Disable MSAA on mobile if not supported
            if (!quality.useMSAA)
            {
                QualitySettings.antiAliasing = 0;
            }

            // Optimize for battery life
            if (quality.optimizeForBattery)
            {
                Application.targetFrameRate = 30; // Limit to 30 FPS to save battery
            }
            else
            {
                Application.targetFrameRate = -1; // No limit
            }
        }

        /// <summary>
        /// Applies custom optimizations.
        /// Riley: "Apply custom optimizations!"
        /// </summary>
        private void ApplyCustomOptimizations(QualityLevel quality)
        {
            // Reduce draw calls if needed
            if (quality.reduceDrawCalls)
            {
                // This would integrate with your object pooling system
                Debug.Log("Riley: Draw call reduction enabled");
            }

            // Limit texture sizes
            if (quality.maxTextureSize < 2048)
            {
                // This would integrate with your texture optimization system
                Debug.Log($"Riley: Texture size limited to {quality.maxTextureSize}px");
            }
        }

        /// <summary>
        /// Gets current quality level name.
        /// Riley: "Get current quality level name!"
        /// </summary>
        public string GetCurrentQualityName()
        {
            return qualityLevels[currentQualityLevel].name;
        }

        /// <summary>
        /// Gets current quality level.
        /// Nibble: "Bark! (Translation: Get current quality level!)"
        /// </summary>
        public int GetCurrentQualityLevel()
        {
            return currentQualityLevel;
        }

        /// <summary>
        /// Gets all available quality levels.
        /// Riley: "Get all quality levels!"
        /// </summary>
        public QualityLevel[] GetQualityLevels()
        {
            return qualityLevels;
        }

        /// <summary>
        /// Forces a specific quality level without auto-adjustment.
        /// Riley: "Force a specific quality level!"
        /// </summary>
        public void ForceQualityLevel(int level)
        {
            autoAdjustQuality = false;
            SetQualityLevel(level);
        }

        /// <summary>
        /// Enables automatic quality adjustment.
        /// Nibble: "Bark! (Translation: Enable auto adjustment!)"
        /// </summary>
        public void EnableAutoAdjustment()
        {
            autoAdjustQuality = true;
            Debug.Log("Riley: Automatic quality adjustment enabled");
        }

        /// <summary>
        /// Disables automatic quality adjustment.
        /// Riley: "Disable auto adjustment!"
        /// </summary>
        public void DisableAutoAdjustment()
        {
            autoAdjustQuality = false;
            Debug.Log("Riley: Automatic quality adjustment disabled");
        }

        /// <summary>
        /// Gets performance statistics.
        /// Riley: "Get performance stats!"
        /// </summary>
        public string GetPerformanceStats()
        {
            var averageFPS = CalculateAverageFPS();
            var currentFPS = 1f / Time.unscaledDeltaTime;
            
            return $"Performance Stats:\n" +
                   $"Current FPS: {currentFPS:F1}\n" +
                   $"Average FPS: {averageFPS:F1}\n" +
                   $"Quality Level: {GetCurrentQualityName()}\n" +
                   $"Auto Adjust: {autoAdjustQuality}\n" +
                   $"Target FPS: {targetFPS}\n" +
                   $"Min FPS: {minFPS}";
        }

        /// <summary>
        /// Resets quality to default level.
        /// Riley: "Reset quality to default!"
        /// </summary>
        public void ResetToDefault()
        {
            SetQualityLevel(2); // Medium quality
            EnableAutoAdjustment();
            Debug.Log("Nibble: *bark* (Translation: Quality reset to default!)");
        }
    }
}