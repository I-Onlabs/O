using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Main game optimizer that coordinates all optimization systems.
    /// Riley: "Time to optimize everything! Can't have lag when hounds are chasing!"
    /// Nibble: "Bark! (Translation: Optimize the game!)"
    /// </summary>
    public class GameOptimizer : MonoBehaviour
    {
        [System.Serializable]
        public class OptimizationProfile
        {
            [Header("Profile Info")]
            public string name = "Default";
            public bool isMobile = false;
            public bool isLowEnd = false;
            public bool isBatteryOptimized = false;
            
            [Header("Performance Targets")]
            public float targetFPS = 60f;
            public float minFPS = 30f;
            public int maxDrawCalls = 100;
            public int maxTriangles = 100000;
            public float maxMemoryUsage = 1024f; // MB
            
            [Header("Quality Settings")]
            public int qualityLevel = 2;
            public bool enableShadows = true;
            public bool enableParticles = true;
            public bool enablePostProcessing = true;
            public bool enableNeonEffects = true;
            public int textureQuality = 0;
            public int antiAliasing = 1;
            
            [Header("Mobile Optimizations")]
            public bool reduceDrawCalls = true;
            public bool optimizeForBattery = true;
            public bool useObjectPooling = true;
            public bool limitParticleCount = true;
            public int maxParticleCount = 100;
            public bool useLODSystem = true;
            public int maxLODLevel = 2;
        }

        [Header("Optimization Profiles")]
        [SerializeField] private OptimizationProfile[] profiles = new OptimizationProfile[4];
        [SerializeField] private int currentProfileIndex = 2;
        [SerializeField] private bool autoOptimize = true;
        [SerializeField] private float optimizationCheckInterval = 10f;

        [Header("System References")]
        [SerializeField] private PerformanceProfiler performanceProfiler;
        [SerializeField] private QualitySettingsManager qualityManager;
        [SerializeField] private PlatformDetector platformDetector;
        [SerializeField] private GameAnalytics gameAnalytics;

        [Header("Optimization Settings")]
        [SerializeField] private bool enableDynamicOptimization = true;
        [SerializeField] private bool enableBatteryOptimization = true;
        [SerializeField] private bool enableMemoryOptimization = true;
        [SerializeField] private bool enableRenderingOptimization = true;

        private OptimizationProfile _currentProfile;
        private float _lastOptimizationCheck;
        private bool _isInitialized;
        private Dictionary<string, object> _optimizationHistory;

        // Events
        public System.Action<OptimizationProfile> OnProfileChanged;
        public System.Action<string> OnOptimizationApplied;
        public System.Action<float> OnPerformanceWarning;

        private void Awake()
        {
            _optimizationHistory = new Dictionary<string, object>();
            InitializeProfiles();
        }

        private void Start()
        {
            InitializeOptimizer();
        }

        private void Update()
        {
            if (!_isInitialized || !autoOptimize) return;

            if (Time.time - _lastOptimizationCheck >= optimizationCheckInterval)
            {
                CheckPerformanceAndOptimize();
                _lastOptimizationCheck = Time.time;
            }
        }

        /// <summary>
        /// Initializes default optimization profiles.
        /// Riley: "Initialize the optimization profiles!"
        /// </summary>
        private void InitializeProfiles()
        {
            // Ultra Profile
            profiles[0] = new OptimizationProfile
            {
                name = "Ultra",
                isMobile = false,
                isLowEnd = false,
                isBatteryOptimized = false,
                targetFPS = 60f,
                minFPS = 45f,
                maxDrawCalls = 200,
                maxTriangles = 500000,
                maxMemoryUsage = 4096f,
                qualityLevel = 4,
                enableShadows = true,
                enableParticles = true,
                enablePostProcessing = true,
                enableNeonEffects = true,
                textureQuality = 0,
                antiAliasing = 3,
                reduceDrawCalls = false,
                optimizeForBattery = false,
                useObjectPooling = false,
                limitParticleCount = false,
                maxParticleCount = 1000,
                useLODSystem = false,
                maxLODLevel = 0
            };

            // High Profile
            profiles[1] = new OptimizationProfile
            {
                name = "High",
                isMobile = false,
                isLowEnd = false,
                isBatteryOptimized = false,
                targetFPS = 60f,
                minFPS = 45f,
                maxDrawCalls = 150,
                maxTriangles = 300000,
                maxMemoryUsage = 2048f,
                qualityLevel = 3,
                enableShadows = true,
                enableParticles = true,
                enablePostProcessing = true,
                enableNeonEffects = true,
                textureQuality = 0,
                antiAliasing = 2,
                reduceDrawCalls = false,
                optimizeForBattery = false,
                useObjectPooling = true,
                limitParticleCount = false,
                maxParticleCount = 500,
                useLODSystem = true,
                maxLODLevel = 1
            };

            // Medium Profile
            profiles[2] = new OptimizationProfile
            {
                name = "Medium",
                isMobile = false,
                isLowEnd = false,
                isBatteryOptimized = false,
                targetFPS = 60f,
                minFPS = 30f,
                maxDrawCalls = 100,
                maxTriangles = 150000,
                maxMemoryUsage = 1024f,
                qualityLevel = 2,
                enableShadows = true,
                enableParticles = true,
                enablePostProcessing = false,
                enableNeonEffects = true,
                textureQuality = 1,
                antiAliasing = 1,
                reduceDrawCalls = true,
                optimizeForBattery = false,
                useObjectPooling = true,
                limitParticleCount = true,
                maxParticleCount = 200,
                useLODSystem = true,
                maxLODLevel = 2
            };

            // Mobile Profile
            profiles[3] = new OptimizationProfile
            {
                name = "Mobile",
                isMobile = true,
                isLowEnd = true,
                isBatteryOptimized = true,
                targetFPS = 30f,
                minFPS = 20f,
                maxDrawCalls = 50,
                maxTriangles = 50000,
                maxMemoryUsage = 512f,
                qualityLevel = 1,
                enableShadows = false,
                enableParticles = false,
                enablePostProcessing = false,
                enableNeonEffects = false,
                textureQuality = 2,
                antiAliasing = 0,
                reduceDrawCalls = true,
                optimizeForBattery = true,
                useObjectPooling = true,
                limitParticleCount = true,
                maxParticleCount = 50,
                useLODSystem = true,
                maxLODLevel = 3
            };
        }

        /// <summary>
        /// Initializes the game optimizer.
        /// Nibble: "Bark! (Translation: Initialize the optimizer!)"
        /// </summary>
        private void InitializeOptimizer()
        {
            try
            {
                // Get system references
                if (performanceProfiler == null)
                    performanceProfiler = FindObjectOfType<PerformanceProfiler>();
                
                if (qualityManager == null)
                    qualityManager = FindObjectOfType<QualitySettingsManager>();
                
                if (platformDetector == null)
                    platformDetector = FindObjectOfType<PlatformDetector>();
                
                if (gameAnalytics == null)
                    gameAnalytics = FindObjectOfType<GameAnalytics>();

                // Detect platform and set initial profile
                if (platformDetector != null)
                {
                    platformDetector.DetectPlatform();
                    SelectOptimalProfile();
                }
                else
                {
                    SetProfile(currentProfileIndex);
                }

                _isInitialized = true;
                
                Debug.Log("Riley: Game optimizer initialized!");
                Debug.Log($"Nibble: *bark* (Translation: Using {_currentProfile.name} profile!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to initialize game optimizer: {ex.Message}");
            }
        }

        /// <summary>
        /// Selects the optimal profile based on platform detection.
        /// Riley: "Select the optimal profile!"
        /// </summary>
        private void SelectOptimalProfile()
        {
            if (platformDetector == null) return;

            var deviceInfo = platformDetector.GetDeviceInfo();
            var performanceTier = platformDetector.GetPerformanceTier();

            int selectedProfile = 2; // Default to Medium

            if (deviceInfo.isMobile)
            {
                selectedProfile = 3; // Mobile
            }
            else
            {
                switch (performanceTier)
                {
                    case PlatformDetector.PerformanceTier.Ultra:
                        selectedProfile = 0; // Ultra
                        break;
                    case PlatformDetector.PerformanceTier.High:
                        selectedProfile = 1; // High
                        break;
                    case PlatformDetector.PerformanceTier.Medium:
                        selectedProfile = 2; // Medium
                        break;
                    case PlatformDetector.PerformanceTier.Low:
                        selectedProfile = 3; // Mobile (lowest)
                        break;
                }
            }

            SetProfile(selectedProfile);
        }

        /// <summary>
        /// Sets the optimization profile.
        /// Riley: "Set the optimization profile!"
        /// </summary>
        public void SetProfile(int profileIndex)
        {
            if (profileIndex < 0 || profileIndex >= profiles.Length)
            {
                Debug.LogWarning($"Riley: Invalid profile index {profileIndex}");
                return;
            }

            currentProfileIndex = profileIndex;
            _currentProfile = profiles[profileIndex];
            
            ApplyProfile(_currentProfile);
            OnProfileChanged?.Invoke(_currentProfile);
            
            Debug.Log($"Riley: Switched to {_currentProfile.name} profile");
        }

        /// <summary>
        /// Applies the optimization profile.
        /// Nibble: "Bark! (Translation: Apply the profile!)"
        /// </summary>
        private void ApplyProfile(OptimizationProfile profile)
        {
            try
            {
                // Apply quality settings
                if (qualityManager != null)
                {
                    qualityManager.SetQualityLevel(profile.qualityLevel);
                }

                // Apply Unity Quality Settings
                ApplyUnityQualitySettings(profile);

                // Apply custom optimizations
                ApplyCustomOptimizations(profile);

                // Apply mobile optimizations
                if (profile.isMobile)
                {
                    ApplyMobileOptimizations(profile);
                }

                // Apply battery optimizations
                if (profile.isBatteryOptimized)
                {
                    ApplyBatteryOptimizations(profile);
                }

                // Apply memory optimizations
                if (enableMemoryOptimization)
                {
                    ApplyMemoryOptimizations(profile);
                }

                // Apply rendering optimizations
                if (enableRenderingOptimization)
                {
                    ApplyRenderingOptimizations(profile);
                }

                OnOptimizationApplied?.Invoke($"Applied {profile.name} profile");
                
                Debug.Log($"Nibble: *bark* (Translation: {profile.name} profile applied!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to apply profile: {ex.Message}");
            }
        }

        /// <summary>
        /// Applies Unity Quality Settings.
        /// Riley: "Apply Unity Quality Settings!"
        /// </summary>
        private void ApplyUnityQualitySettings(OptimizationProfile profile)
        {
            QualitySettings.SetQualityLevel(profile.qualityLevel);
            QualitySettings.masterTextureLimit = profile.textureQuality;
            QualitySettings.antiAliasing = (int)Mathf.Pow(2, profile.antiAliasing);
            QualitySettings.shadowResolution = profile.enableShadows ? ShadowResolution.Medium : ShadowResolution.Low;
            QualitySettings.shadowDistance = profile.enableShadows ? 50f : 0f;
            Application.targetFrameRate = Mathf.RoundToInt(profile.targetFPS);
        }

        /// <summary>
        /// Applies custom optimizations.
        /// Riley: "Apply custom optimizations!"
        /// </summary>
        private void ApplyCustomOptimizations(OptimizationProfile profile)
        {
            // Enable/disable neon effects
            var uiManager = FindObjectOfType<UI.UIManager>();
            if (uiManager != null)
            {
                // This would integrate with your UI system
                Debug.Log($"Riley: Neon effects {(profile.enableNeonEffects ? "enabled" : "disabled")}");
            }

            // Enable/disable particles
            var particleSystems = FindObjectsOfType<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                if (profile.enableParticles)
                {
                    ps.Play();
                }
                else
                {
                    ps.Stop();
                }
            }

            // Limit particle count
            if (profile.limitParticleCount)
            {
                foreach (var ps in particleSystems)
                {
                    var main = ps.main;
                    main.maxParticles = Mathf.Min(main.maxParticles, profile.maxParticleCount);
                }
            }
        }

        /// <summary>
        /// Applies mobile optimizations.
        /// Nibble: "Bark! (Translation: Apply mobile optimizations!)"
        /// </summary>
        private void ApplyMobileOptimizations(OptimizationProfile profile)
        {
            // Disable HDR on mobile
            Camera.main.allowHDR = false;

            // Disable MSAA on mobile
            QualitySettings.antiAliasing = 0;

            // Set target frame rate
            Application.targetFrameRate = Mathf.RoundToInt(profile.targetFPS);

            // Optimize for mobile rendering
            if (profile.reduceDrawCalls)
            {
                // This would integrate with your object pooling system
                Debug.Log("Riley: Draw call reduction enabled for mobile");
            }
        }

        /// <summary>
        /// Applies battery optimizations.
        /// Riley: "Apply battery optimizations!"
        /// </summary>
        private void ApplyBatteryOptimizations(OptimizationProfile profile)
        {
            // Limit frame rate to save battery
            Application.targetFrameRate = 30;

            // Reduce update frequency
            Time.fixedDeltaTime = 0.02f; // 50 FPS physics

            // Disable unnecessary effects
            QualitySettings.softVegetation = false;
            QualitySettings.realtimeReflectionProbes = false;

            Debug.Log("Riley: Battery optimizations applied");
        }

        /// <summary>
        /// Applies memory optimizations.
        /// Riley: "Apply memory optimizations!"
        /// </summary>
        private void ApplyMemoryOptimizations(OptimizationProfile profile)
        {
            // Force garbage collection
            System.GC.Collect();

            // Set texture quality
            QualitySettings.masterTextureLimit = profile.textureQuality;

            // Limit texture sizes
            var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            foreach (var texture in textures)
            {
                if (texture.width > 1024 || texture.height > 1024)
                {
                    // This would resize textures in a real implementation
                    Debug.Log($"Riley: Texture {texture.name} would be resized for memory optimization");
                }
            }

            Debug.Log("Riley: Memory optimizations applied");
        }

        /// <summary>
        /// Applies rendering optimizations.
        /// Nibble: "Bark! (Translation: Apply rendering optimizations!)"
        /// </summary>
        private void ApplyRenderingOptimizations(OptimizationProfile profile)
        {
            // Set LOD bias
            QualitySettings.lodBias = profile.useLODSystem ? 1f : 2f;

            // Set maximum LOD level
            QualitySettings.maximumLODLevel = profile.maxLODLevel;

            // Optimize shadows
            if (!profile.enableShadows)
            {
                QualitySettings.shadowDistance = 0f;
            }

            Debug.Log("Riley: Rendering optimizations applied");
        }

        /// <summary>
        /// Checks performance and optimizes if needed.
        /// Riley: "Check performance and optimize!"
        /// </summary>
        private void CheckPerformanceAndOptimize()
        {
            if (!enableDynamicOptimization || performanceProfiler == null) return;

            var metrics = performanceProfiler.GetCurrentMetrics();
            var currentFPS = metrics.currentFPS;
            var memoryUsage = metrics.usedMemory / 1024f / 1024f; // MB
            var drawCalls = metrics.drawCalls;

            bool needsOptimization = false;
            string reason = "";

            // Check FPS
            if (currentFPS < _currentProfile.minFPS)
            {
                needsOptimization = true;
                reason = $"Low FPS: {currentFPS:F1} < {_currentProfile.minFPS}";
            }

            // Check memory usage
            if (memoryUsage > _currentProfile.maxMemoryUsage)
            {
                needsOptimization = true;
                reason = $"High memory usage: {memoryUsage:F1}MB > {_currentProfile.maxMemoryUsage}MB";
            }

            // Check draw calls
            if (drawCalls > _currentProfile.maxDrawCalls)
            {
                needsOptimization = true;
                reason = $"High draw calls: {drawCalls} > {_currentProfile.maxDrawCalls}";
            }

            if (needsOptimization)
            {
                OptimizeForPerformance(reason);
            }
        }

        /// <summary>
        /// Optimizes for performance.
        /// Riley: "Optimize for performance!"
        /// </summary>
        private void OptimizeForPerformance(string reason)
        {
            Debug.LogWarning($"Riley: Performance optimization needed - {reason}");

            // Try to reduce quality level
            if (currentProfileIndex < profiles.Length - 1)
            {
                SetProfile(currentProfileIndex + 1);
                OnPerformanceWarning?.Invoke($"Performance warning: {reason}. Reduced quality to {_currentProfile.name}");
            }
            else
            {
                // Apply emergency optimizations
                ApplyEmergencyOptimizations();
            }
        }

        /// <summary>
        /// Applies emergency optimizations.
        /// Nibble: "Bark! (Translation: Apply emergency optimizations!)"
        /// </summary>
        private void ApplyEmergencyOptimizations()
        {
            Debug.Log("Riley: Applying emergency optimizations!");

            // Disable all non-essential effects
            QualitySettings.antiAliasing = 0;
            QualitySettings.shadowDistance = 0f;
            QualitySettings.softVegetation = false;
            QualitySettings.realtimeReflectionProbes = false;

            // Reduce frame rate
            Application.targetFrameRate = 30;

            // Force garbage collection
            System.GC.Collect();

            OnOptimizationApplied?.Invoke("Emergency optimizations applied");
        }

        /// <summary>
        /// Gets current optimization profile.
        /// Riley: "Get current profile!"
        /// </summary>
        public OptimizationProfile GetCurrentProfile()
        {
            return _currentProfile;
        }

        /// <summary>
        /// Gets all available profiles.
        /// Nibble: "Bark! (Translation: Get all profiles!)"
        /// </summary>
        public OptimizationProfile[] GetProfiles()
        {
            return profiles;
        }

        /// <summary>
        /// Gets optimization statistics.
        /// Riley: "Get optimization stats!"
        /// </summary>
        public string GetOptimizationStats()
        {
            var stats = $"Game Optimization Statistics:\n";
            stats += $"Current Profile: {_currentProfile.name}\n";
            stats += $"Target FPS: {_currentProfile.targetFPS}\n";
            stats += $"Min FPS: {_currentProfile.minFPS}\n";
            stats += $"Max Draw Calls: {_currentProfile.maxDrawCalls}\n";
            stats += $"Max Memory: {_currentProfile.maxMemoryUsage}MB\n";
            stats += $"Quality Level: {_currentProfile.qualityLevel}\n";
            stats += $"Shadows: {_currentProfile.enableShadows}\n";
            stats += $"Particles: {_currentProfile.enableParticles}\n";
            stats += $"Neon Effects: {_currentProfile.enableNeonEffects}\n";
            stats += $"Mobile Optimized: {_currentProfile.isMobile}\n";
            stats += $"Battery Optimized: {_currentProfile.isBatteryOptimized}\n";
            
            return stats;
        }

        /// <summary>
        /// Forces optimization check.
        /// Riley: "Force optimization check!"
        /// </summary>
        public void ForceOptimizationCheck()
        {
            CheckPerformanceAndOptimize();
        }

        /// <summary>
        /// Enables automatic optimization.
        /// Nibble: "Bark! (Translation: Enable auto optimization!)"
        /// </summary>
        public void EnableAutoOptimization()
        {
            autoOptimize = true;
            Debug.Log("Riley: Automatic optimization enabled");
        }

        /// <summary>
        /// Disables automatic optimization.
        /// Riley: "Disable auto optimization!"
        /// </summary>
        public void DisableAutoOptimization()
        {
            autoOptimize = false;
            Debug.Log("Riley: Automatic optimization disabled");
        }
    }
}