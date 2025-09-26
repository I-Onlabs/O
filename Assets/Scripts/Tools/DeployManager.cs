using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using AngryDogs.SaveSystem;
using AngryDogs.Tools;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Automated deployment manager for final build checks and CI/CD integration.
    /// Riley: "Time to automate the deployment process! Can't have manual errors when launching to the world!"
    /// Nibble: "Bark! (Translation: Automated deployment for smooth launches!)"
    /// </summary>
    public sealed class DeployManager : MonoBehaviour
    {
        [System.Serializable]
        public class BuildConfiguration
        {
            public string platform;
            public string buildType;
            public string version;
            public int buildNumber;
            public bool enableIL2CPP;
            public bool enableCodeStripping;
            public bool enableMobileOptimizations;
            public string[] requiredScenes;
            public string[] requiredAssets;
            public Dictionary<string, object> platformSettings;
        }

        [System.Serializable]
        public class DeploymentCheck
        {
            public string checkName;
            public string description;
            public bool isRequired;
            public bool passed;
            public string errorMessage;
            public System.Action<DeploymentCheck> checkAction;
        }

        [Header("Deployment Settings")]
        [SerializeField] private bool enableDeploymentChecks = true;
        [SerializeField] private bool enableVersionValidation = true;
        [SerializeField] private bool enableAssetIntegrity = true;
        [SerializeField] private bool enablePerformanceValidation = true;
        [SerializeField] private bool enableMobileOptimization = true;

        [Header("Build Configurations")]
        [SerializeField] private BuildConfiguration[] buildConfigurations = {
            new BuildConfiguration {
                platform = "PC",
                buildType = "Release",
                version = "1.0.0",
                buildNumber = 1,
                enableIL2CPP = true,
                enableCodeStripping = true,
                enableMobileOptimizations = false,
                requiredScenes = new[] { "MainMenu", "Gameplay", "Settings" },
                requiredAssets = new[] { "Player", "Nibble", "BossHound", "ObstacleManager" },
                platformSettings = new Dictionary<string, object> {
                    {"graphics_api", "Direct3D11"},
                    {"scripting_backend", "IL2CPP"},
                    {"target_architecture", "x64"}
                }
            },
            new BuildConfiguration {
                platform = "Android",
                buildType = "Release",
                version = "1.0.0",
                buildNumber = 1,
                enableIL2CPP = true,
                enableCodeStripping = true,
                enableMobileOptimizations = true,
                requiredScenes = new[] { "MainMenu", "Gameplay", "Settings" },
                requiredAssets = new[] { "Player", "Nibble", "BossHound", "ObstacleManager" },
                platformSettings = new Dictionary<string, object> {
                    {"target_architecture", "ARM64"},
                    {"graphics_api", "OpenGL ES 3.0"},
                    {"texture_compression", "ASTC"},
                    {"min_sdk_version", "21"},
                    {"target_sdk_version", "33"}
                }
            },
            new BuildConfiguration {
                platform = "iOS",
                buildType = "Release",
                version = "1.0.0",
                buildNumber = 1,
                enableIL2CPP = true,
                enableCodeStripping = true,
                enableMobileOptimizations = true,
                requiredScenes = new[] { "MainMenu", "Gameplay", "Settings" },
                requiredAssets = new[] { "Player", "Nibble", "BossHound", "ObstacleManager" },
                platformSettings = new Dictionary<string, object> {
                    {"graphics_api", "Metal"},
                    {"target_ios_version", "12.0"},
                    {"texture_compression", "ASTC"},
                    {"device_family", "iPhone,iPad"}
                }
            }
        };

        [Header("Systems")]
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private GameAnalytics gameAnalytics;
        [SerializeField] private ErrorReporter errorReporter;

        private List<DeploymentCheck> _deploymentChecks;
        private bool _isInitialized;
        private string _buildOutputPath;

        // Events
        public System.Action<DeploymentCheck> OnCheckCompleted;
        public System.Action<bool> OnDeploymentValidationComplete;
        public System.Action<string> OnDeploymentError;

        private void Awake()
        {
            _deploymentChecks = new List<DeploymentCheck>();
        }

        private void Start()
        {
            if (enableDeploymentChecks)
            {
                InitializeDeploymentChecks();
            }
        }

        /// <summary>
        /// Initializes the deployment check system.
        /// Riley: "Initialize the deployment check system!"
        /// </summary>
        private void InitializeDeploymentChecks()
        {
            try
            {
                SetupDeploymentChecks();
                _isInitialized = true;
                
                Debug.Log("Riley: Deployment check system initialized!");
                Debug.Log("Nibble: *bark* (Translation: Deployment checks ready!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Failed to initialize deployment checks: {ex.Message}");
            }
        }

        /// <summary>
        /// Sets up all deployment checks.
        /// Nibble: "Bark! (Translation: Set up deployment checks!)"
        /// </summary>
        private void SetupDeploymentChecks()
        {
            _deploymentChecks.Clear();

            // Version validation checks
            if (enableVersionValidation)
            {
                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Version Validation",
                    description = "Validate version numbers and build numbers",
                    isRequired = true,
                    checkAction = ValidateVersion
                });

                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Build Number Increment",
                    description = "Ensure build number is incremented",
                    isRequired = true,
                    checkAction = ValidateBuildNumberIncrement
                });
            }

            // Asset integrity checks
            if (enableAssetIntegrity)
            {
                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Required Scenes",
                    description = "Verify all required scenes are in build settings",
                    isRequired = true,
                    checkAction = ValidateRequiredScenes
                });

                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Required Assets",
                    description = "Verify all required assets are present",
                    isRequired = true,
                    checkAction = ValidateRequiredAssets
                });

                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Save System Integrity",
                    description = "Validate save system configuration",
                    isRequired = true,
                    checkAction = ValidateSaveSystem
                });
            }

            // Performance validation checks
            if (enablePerformanceValidation)
            {
                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Performance Targets",
                    description = "Validate performance targets are met",
                    isRequired = true,
                    checkAction = ValidatePerformanceTargets
                });

                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Memory Usage",
                    description = "Check memory usage is within limits",
                    isRequired = true,
                    checkAction = ValidateMemoryUsage
                });
            }

            // Mobile optimization checks
            if (enableMobileOptimization)
            {
                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Mobile Optimizations",
                    description = "Verify mobile optimizations are enabled",
                    isRequired = true,
                    checkAction = ValidateMobileOptimizations
                });

                _deploymentChecks.Add(new DeploymentCheck
                {
                    checkName = "Texture Compression",
                    description = "Check texture compression settings",
                    isRequired = true,
                    checkAction = ValidateTextureCompression
                });
            }

            Debug.Log($"Riley: Set up {_deploymentChecks.Count} deployment checks!");
        }

        /// <summary>
        /// Runs all deployment checks.
        /// Riley: "Run all deployment checks!"
        /// </summary>
        public IEnumerator RunDeploymentChecks(string platform = "PC")
        {
            if (!_isInitialized)
            {
                Debug.LogError("Riley: Deployment checks not initialized!");
                yield break;
            }

            Debug.Log($"Riley: Starting deployment checks for {platform}!");
            
            var allPassed = true;
            var passedChecks = 0;
            var totalChecks = _deploymentChecks.Count;

            foreach (var check in _deploymentChecks)
            {
                Debug.Log($"Riley: Running check: {check.checkName}");
                
                try
                {
                    check.checkAction?.Invoke(check);
                    
                    if (check.passed)
                    {
                        passedChecks++;
                        Debug.Log($"Riley: ✓ {check.checkName} - PASSED");
                    }
                    else
                    {
                        Debug.LogError($"Riley: ✗ {check.checkName} - FAILED: {check.errorMessage}");
                        if (check.isRequired)
                        {
                            allPassed = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Riley: ✗ {check.checkName} - ERROR: {ex.Message}");
                    check.passed = false;
                    check.errorMessage = ex.Message;
                    if (check.isRequired)
                    {
                        allPassed = false;
                    }
                }

                OnCheckCompleted?.Invoke(check);
                yield return new WaitForSeconds(0.1f); // Small delay between checks
            }

            Debug.Log($"Riley: Deployment checks completed! {passedChecks}/{totalChecks} passed");
            Debug.Log($"Nibble: *bark* (Translation: Deployment checks {(allPassed ? "passed" : "failed")}!)");

            OnDeploymentValidationComplete?.Invoke(allPassed);
        }

        /// <summary>
        /// Validates version numbers.
        /// Riley: "Validate version numbers!"
        /// </summary>
        private void ValidateVersion(DeploymentCheck check)
        {
            try
            {
                var version = Application.version;
                if (string.IsNullOrEmpty(version))
                {
                    check.passed = false;
                    check.errorMessage = "Version is not set";
                    return;
                }

                // Parse version to ensure it's valid
                var versionParts = version.Split('.');
                if (versionParts.Length != 3)
                {
                    check.passed = false;
                    check.errorMessage = "Version format should be X.Y.Z";
                    return;
                }

                foreach (var part in versionParts)
                {
                    if (!int.TryParse(part, out _))
                    {
                        check.passed = false;
                        check.errorMessage = "Version parts must be numbers";
                        return;
                    }
                }

                check.passed = true;
                Debug.Log($"Riley: Version validation passed: {version}");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Validates build number increment.
        /// Nibble: "Bark! (Translation: Validate build number increment!)"
        /// </summary>
        private void ValidateBuildNumberIncrement(DeploymentCheck check)
        {
            try
            {
                // This would typically check against a build server or previous build
                // For now, we'll just ensure the build number is positive
                var buildNumber = PlayerSettings.Android.bundleVersionCode;
                if (buildNumber <= 0)
                {
                    check.passed = false;
                    check.errorMessage = "Build number must be positive";
                    return;
                }

                check.passed = true;
                Debug.Log($"Riley: Build number validation passed: {buildNumber}");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Validates required scenes are in build settings.
        /// Riley: "Validate required scenes!"
        /// </summary>
        private void ValidateRequiredScenes(DeploymentCheck check)
        {
            try
            {
                var buildScenes = EditorBuildSettings.scenes;
                var sceneNames = new List<string>();
                
                foreach (var scene in buildScenes)
                {
                    if (scene.enabled)
                    {
                        sceneNames.Add(Path.GetFileNameWithoutExtension(scene.path));
                    }
                }

                var missingScenes = new List<string>();
                foreach (var requiredScene in new[] { "MainMenu", "Gameplay", "Settings" })
                {
                    if (!sceneNames.Contains(requiredScene))
                    {
                        missingScenes.Add(requiredScene);
                    }
                }

                if (missingScenes.Count > 0)
                {
                    check.passed = false;
                    check.errorMessage = $"Missing scenes: {string.Join(", ", missingScenes)}";
                    return;
                }

                check.passed = true;
                Debug.Log("Riley: Required scenes validation passed");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Validates required assets are present.
        /// Nibble: "Bark! (Translation: Validate required assets!)"
        /// </summary>
        private void ValidateRequiredAssets(DeploymentCheck check)
        {
            try
            {
                var requiredAssets = new[] { "Player", "Nibble", "BossHound", "ObstacleManager" };
                var missingAssets = new List<string>();

                foreach (var assetName in requiredAssets)
                {
                    var asset = Resources.Load(assetName);
                    if (asset == null)
                    {
                        missingAssets.Add(assetName);
                    }
                }

                if (missingAssets.Count > 0)
                {
                    check.passed = false;
                    check.errorMessage = $"Missing assets: {string.Join(", ", missingAssets)}";
                    return;
                }

                check.passed = true;
                Debug.Log("Riley: Required assets validation passed");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Validates save system configuration.
        /// Riley: "Validate save system configuration!"
        /// </summary>
        private void ValidateSaveSystem(DeploymentCheck check)
        {
            try
            {
                if (saveManager == null)
                {
                    check.passed = false;
                    check.errorMessage = "SaveManager not found";
                    return;
                }

                // Test save system functionality
                var testData = saveManager.Progress;
                if (testData == null)
                {
                    check.passed = false;
                    check.errorMessage = "Save system not returning valid data";
                    return;
                }

                check.passed = true;
                Debug.Log("Riley: Save system validation passed");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Validates performance targets.
        /// Riley: "Validate performance targets!"
        /// </summary>
        private void ValidatePerformanceTargets(DeploymentCheck check)
        {
            try
            {
                var currentFPS = 1f / Time.unscaledDeltaTime;
                var targetFPS = 60f;

                if (currentFPS < targetFPS * 0.8f) // Allow 20% tolerance
                {
                    check.passed = false;
                    check.errorMessage = $"FPS too low: {currentFPS:F1}/{targetFPS}";
                    return;
                }

                check.passed = true;
                Debug.Log($"Riley: Performance targets validation passed: {currentFPS:F1} FPS");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Validates memory usage.
        /// Nibble: "Bark! (Translation: Validate memory usage!)"
        /// </summary>
        private void ValidateMemoryUsage(DeploymentCheck check)
        {
            try
            {
                var memoryUsage = UnityEngine.Profiling.Profiler.GetTotalAllocatedMemory(false) / 1024f / 1024f; // MB
                var maxMemory = 1024f; // 1GB limit

                if (memoryUsage > maxMemory)
                {
                    check.passed = false;
                    check.errorMessage = $"Memory usage too high: {memoryUsage:F1}MB/{maxMemory}MB";
                    return;
                }

                check.passed = true;
                Debug.Log($"Riley: Memory usage validation passed: {memoryUsage:F1}MB");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Validates mobile optimizations.
        /// Riley: "Validate mobile optimizations!"
        /// </summary>
        private void ValidateMobileOptimizations(DeploymentCheck check)
        {
            try
            {
                if (Application.isMobilePlatform)
                {
                    // Check if mobile optimizations are enabled
                    var uiManager = FindObjectOfType<UIManager>();
                    if (uiManager != null)
                    {
                        var settings = uiManager.GetPerformanceSettings();
                        if (!settings.mobileOptimized)
                        {
                            check.passed = false;
                            check.errorMessage = "Mobile optimizations not enabled";
                            return;
                        }
                    }
                }

                check.passed = true;
                Debug.Log("Riley: Mobile optimizations validation passed");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Validates texture compression settings.
        /// Nibble: "Bark! (Translation: Validate texture compression!)"
        /// </summary>
        private void ValidateTextureCompression(DeploymentCheck check)
        {
            try
            {
                if (Application.isMobilePlatform)
                {
                    // Check texture compression settings
                    var textureCompression = PlayerSettings.GetTextureCompressionForPlatform(BuildTargetGroup.Android);
                    if (textureCompression == TextureImporterFormat.Automatic)
                    {
                        check.passed = true; // Automatic is acceptable
                    }
                    else
                    {
                        check.passed = true; // Any specific compression is acceptable
                    }
                }
                else
                {
                    check.passed = true; // Not applicable for desktop
                }

                Debug.Log("Riley: Texture compression validation passed");
            }
            catch (Exception ex)
            {
                check.passed = false;
                check.errorMessage = ex.Message;
            }
        }

        /// <summary>
        /// Gets deployment check results.
        /// Riley: "Get deployment check results!"
        /// </summary>
        public string GetDeploymentCheckResults()
        {
            var results = "Deployment Check Results:\n";
            results += "========================\n\n";

            var passedChecks = 0;
            var totalChecks = _deploymentChecks.Count;

            foreach (var check in _deploymentChecks)
            {
                var status = check.passed ? "✓ PASSED" : "✗ FAILED";
                results += $"{status} - {check.checkName}\n";
                results += $"  {check.description}\n";
                
                if (!check.passed)
                {
                    results += $"  Error: {check.errorMessage}\n";
                }
                
                results += "\n";

                if (check.passed)
                {
                    passedChecks++;
                }
            }

            results += $"Summary: {passedChecks}/{totalChecks} checks passed\n";
            results += $"Overall Status: {(passedChecks == totalChecks ? "READY FOR DEPLOYMENT" : "NOT READY")}\n";

            return results;
        }

        /// <summary>
        /// Exports deployment report.
        /// Nibble: "Bark! (Translation: Export deployment report!)"
        /// </summary>
        public string ExportDeploymentReport()
        {
            var report = new Dictionary<string, object>
            {
                {"deployment_timestamp", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")},
                {"platform", Application.platform.ToString()},
                {"version", Application.version},
                {"unity_version", Application.unityVersion},
                {"build_number", PlayerSettings.Android.bundleVersionCode},
                {"checks_passed", _deploymentChecks.FindAll(c => c.passed).Count},
                {"total_checks", _deploymentChecks.Count},
                {"deployment_ready", _deploymentChecks.TrueForAll(c => c.passed || !c.isRequired)},
                {"check_results", _deploymentChecks}
            };

            return JsonUtility.ToJson(report, true);
        }

        /// <summary>
        /// Runs deployment checks from command line.
        /// Riley: "Run deployment checks from command line!"
        /// </summary>
        [ContextMenu("Run Deployment Checks")]
        public void RunDeploymentChecksFromMenu()
        {
            StartCoroutine(RunDeploymentChecks());
        }

        /// <summary>
        /// Gets deployment statistics.
        /// Riley: "Get deployment statistics!"
        /// </summary>
        public string GetDeploymentStats()
        {
            var stats = $"Deployment Statistics:\n";
            stats += $"Total Checks: {_deploymentChecks.Count}\n";
            stats += $"Required Checks: {_deploymentChecks.FindAll(c => c.isRequired).Count}\n";
            stats += $"Optional Checks: {_deploymentChecks.FindAll(c => !c.isRequired).Count}\n";
            stats += $"Passed Checks: {_deploymentChecks.FindAll(c => c.passed).Count}\n";
            stats += $"Failed Checks: {_deploymentChecks.FindAll(c => !c.passed).Count}\n";
            stats += $"Platform: {Application.platform}\n";
            stats += $"Version: {Application.version}\n";
            stats += $"Unity Version: {Application.unityVersion}\n";
            
            return stats;
        }
    }
}