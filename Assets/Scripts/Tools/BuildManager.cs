using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Automated build pipeline for iOS/Android/PC deployment with platform-specific optimizations.
    /// Riley: "Time to automate these builds! Can't have manual errors when deploying to multiple platforms!"
    /// Nibble: "Bark! (Translation: Automated builds for all platforms!)"
    /// </summary>
    public class BuildManager : MonoBehaviour
    {
        [System.Serializable]
        public class BuildConfiguration
        {
            [Header("Build Settings")]
            public string buildName = "AngryDogs";
            public string version = "1.0.0";
            public int buildNumber = 1;
            public bool developmentBuild = false;
            public bool allowDebugging = false;
            
            [Header("Platform Settings")]
            public BuildTarget targetPlatform = BuildTarget.StandaloneWindows64;
            public string outputPath = "Builds";
            public bool createFolderPerPlatform = true;
            
            [Header("Optimization Settings")]
            public bool optimizeForMobile = true;
            public bool compressTextures = true;
            public bool stripUnusedCode = true;
            public bool enableIL2CPP = true;
            
            [Header("Mobile Specific")]
            public bool useASTC = true;
            public bool useETC2 = false;
            public int textureCompressionQuality = 50;
            public bool optimizeMeshData = true;
            
            [Header("Version Control")]
            public bool autoIncrementBuildNumber = true;
            public bool updateReadme = true;
            public string readmePath = "README.md";
        }

        [Header("Build Configurations")]
        [SerializeField] private BuildConfiguration pcConfig;
        [SerializeField] private BuildConfiguration androidConfig;
        [SerializeField] private BuildConfiguration iosConfig;

        [Header("Build Status")]
        [SerializeField] private bool isBuilding = false;
        [SerializeField] private string currentBuildStatus = "Ready";
        [SerializeField] private float buildProgress = 0f;

        // Events
        public System.Action<BuildConfiguration> OnBuildStarted;
        public System.Action<BuildConfiguration, bool> OnBuildCompleted;
        public System.Action<string> OnBuildStatusChanged;
        public System.Action<float> OnBuildProgressChanged;

        private void Awake()
        {
            // Initialize default configurations if not set
            InitializeDefaultConfigurations();
        }

        /// <summary>
        /// Initializes default build configurations for all platforms.
        /// Riley: "Time to set up the default build configurations!"
        /// </summary>
        private void InitializeDefaultConfigurations()
        {
            if (pcConfig == null)
            {
                pcConfig = new BuildConfiguration
                {
                    buildName = "AngryDogs_PC",
                    targetPlatform = BuildTarget.StandaloneWindows64,
                    optimizeForMobile = false,
                    enableIL2CPP = true,
                    stripUnusedCode = true
                };
            }

            if (androidConfig == null)
            {
                androidConfig = new BuildConfiguration
                {
                    buildName = "AngryDogs_Android",
                    targetPlatform = BuildTarget.Android,
                    optimizeForMobile = true,
                    useASTC = true,
                    useETC2 = true,
                    textureCompressionQuality = 50,
                    optimizeMeshData = true
                };
            }

            if (iosConfig == null)
            {
                iosConfig = new BuildConfiguration
                {
                    buildName = "AngryDogs_iOS",
                    targetPlatform = BuildTarget.iOS,
                    optimizeForMobile = true,
                    useASTC = true,
                    useETC2 = false,
                    textureCompressionQuality = 75,
                    optimizeMeshData = true
                };
            }
        }

        /// <summary>
        /// Builds for PC platform with optimizations.
        /// Riley: "Time to build for PC! Full power mode!"
        /// </summary>
        [ContextMenu("Build PC")]
        public void BuildPC()
        {
            StartCoroutine(BuildForPlatform(pcConfig));
        }

        /// <summary>
        /// Builds for Android platform with mobile optimizations.
        /// Nibble: "Bark! (Translation: Build for Android!)"
        /// </summary>
        [ContextMenu("Build Android")]
        public void BuildAndroid()
        {
            StartCoroutine(BuildForPlatform(androidConfig));
        }

        /// <summary>
        /// Builds for iOS platform with mobile optimizations.
        /// Riley: "Time to build for iOS! Gotta make sure it runs smooth on iPhones!"
        /// </summary>
        [ContextMenu("Build iOS")]
        public void BuildiOS()
        {
            StartCoroutine(BuildForPlatform(iosConfig));
        }

        /// <summary>
        /// Builds for all platforms sequentially.
        /// Riley: "Time to build for all platforms! This is going to take a while!"
        /// </summary>
        [ContextMenu("Build All Platforms")]
        public void BuildAllPlatforms()
        {
            StartCoroutine(BuildAllPlatformsCoroutine());
        }

        /// <summary>
        /// Builds all platforms in sequence.
        /// Nibble: "Bark! (Translation: Build everything!)"
        /// </summary>
        private IEnumerator BuildAllPlatformsCoroutine()
        {
            Debug.Log("Riley: Starting build for all platforms!");
            
            yield return StartCoroutine(BuildForPlatform(pcConfig));
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(BuildForPlatform(androidConfig));
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(BuildForPlatform(iosConfig));
            
            Debug.Log("Nibble: *bark* (Translation: All platforms built successfully!)");
        }

        /// <summary>
        /// Main build coroutine for a specific platform.
        /// Riley: "Time to build for a specific platform!"
        /// </summary>
        private IEnumerator BuildForPlatform(BuildConfiguration config)
        {
            if (isBuilding)
            {
                Debug.LogWarning("Riley: Build already in progress! Wait for it to complete.");
                yield break;
            }

            isBuilding = true;
            buildProgress = 0f;
            currentBuildStatus = $"Building {config.buildName}...";
            
            OnBuildStarted?.Invoke(config);
            OnBuildStatusChanged?.Invoke(currentBuildStatus);

            try
            {
                // Pre-build setup
                yield return StartCoroutine(PreBuildSetup(config));
                buildProgress = 0.2f;
                OnBuildProgressChanged?.Invoke(buildProgress);

                // Configure platform-specific settings
                yield return StartCoroutine(ConfigurePlatformSettings(config));
                buildProgress = 0.4f;
                OnBuildProgressChanged?.Invoke(buildProgress);

                // Build the project
                yield return StartCoroutine(ExecuteBuild(config));
                buildProgress = 0.8f;
                OnBuildProgressChanged?.Invoke(buildProgress);

                // Post-build cleanup
                yield return StartCoroutine(PostBuildCleanup(config));
                buildProgress = 1f;
                OnBuildProgressChanged?.Invoke(buildProgress);

                currentBuildStatus = $"Build completed successfully: {config.buildName}";
                OnBuildStatusChanged?.Invoke(currentBuildStatus);
                OnBuildCompleted?.Invoke(config, true);

                Debug.Log($"Riley: {config.buildName} build completed successfully!");
            }
            catch (Exception ex)
            {
                currentBuildStatus = $"Build failed: {ex.Message}";
                OnBuildStatusChanged?.Invoke(currentBuildStatus);
                OnBuildCompleted?.Invoke(config, false);

                Debug.LogError($"Riley: Build failed for {config.buildName}: {ex.Message}");
            }
            finally
            {
                isBuilding = false;
            }
        }

        /// <summary>
        /// Pre-build setup and validation.
        /// Riley: "Gotta set up everything before building!"
        /// </summary>
        private IEnumerator PreBuildSetup(BuildConfiguration config)
        {
            Debug.Log($"Riley: Pre-build setup for {config.buildName}...");
            
            // Validate configuration
            if (!ValidateBuildConfiguration(config))
            {
                throw new Exception("Invalid build configuration");
            }

            // Create output directory
            var outputDir = GetOutputDirectory(config);
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Update version if auto-increment is enabled
            if (config.autoIncrementBuildNumber)
            {
                config.buildNumber++;
                Debug.Log($"Nibble: *bark* (Translation: Build number incremented to {config.buildNumber})");
            }

            yield return null;
        }

        /// <summary>
        /// Configures platform-specific settings.
        /// Nibble: "Bark! (Translation: Configure platform settings!)"
        /// </summary>
        private IEnumerator ConfigurePlatformSettings(BuildConfiguration config)
        {
            Debug.Log($"Riley: Configuring platform settings for {config.targetPlatform}...");

            // Set build target
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(config.targetPlatform), config.targetPlatform);

            // Configure player settings
            ConfigurePlayerSettings(config);

            // Configure platform-specific optimizations
            ConfigurePlatformOptimizations(config);

            yield return null;
        }

        /// <summary>
        /// Configures Unity Player Settings for the build.
        /// Riley: "Time to configure the player settings!"
        /// </summary>
        private void ConfigurePlayerSettings(BuildConfiguration config)
        {
            PlayerSettings.productName = config.buildName;
            PlayerSettings.bundleVersion = config.version;
            PlayerSettings.Android.bundleVersionCode = config.buildNumber;
            PlayerSettings.iOS.buildNumber = config.buildNumber.ToString();

            // Configure development build settings
            EditorUserBuildSettings.development = config.developmentBuild;
            EditorUserBuildSettings.allowDebugging = config.allowDebugging;

            // Configure scripting backend
            if (config.enableIL2CPP)
            {
                PlayerSettings.SetScriptingBackend(BuildPipeline.GetBuildTargetGroup(config.targetPlatform), ScriptingImplementation.IL2CPP);
            }

            // Configure code stripping
            if (config.stripUnusedCode)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildPipeline.GetBuildTargetGroup(config.targetPlatform), Il2CppCompilerConfiguration.Master);
            }
        }

        /// <summary>
        /// Configures platform-specific optimizations.
        /// Riley: "Time to optimize for the target platform!"
        /// </summary>
        private void ConfigurePlatformOptimizations(BuildConfiguration config)
        {
            switch (config.targetPlatform)
            {
                case BuildTarget.Android:
                    ConfigureAndroidOptimizations(config);
                    break;
                case BuildTarget.iOS:
                    ConfigureiOSOptimizations(config);
                    break;
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux64:
                    ConfigureDesktopOptimizations(config);
                    break;
            }
        }

        /// <summary>
        /// Configures Android-specific optimizations.
        /// Nibble: "Bark! (Translation: Optimize for Android!)"
        /// </summary>
        private void ConfigureAndroidOptimizations(BuildConfiguration config)
        {
            Debug.Log("Riley: Configuring Android optimizations...");

            // Texture compression
            if (config.useASTC)
            {
                PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
                EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            }

            // Graphics API
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.Vulkan });

            // Optimization settings
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.useCustomKeystore = false; // Use default keystore for testing
        }

        /// <summary>
        /// Configures iOS-specific optimizations.
        /// Riley: "Time to optimize for iOS!"
        /// </summary>
        private void ConfigureiOSOptimizations(BuildConfiguration config)
        {
            Debug.Log("Riley: Configuring iOS optimizations...");

            // Graphics API
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });

            // Optimization settings
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        }

        /// <summary>
        /// Configures desktop-specific optimizations.
        /// Riley: "Desktop optimization - full power mode!"
        /// </summary>
        private void ConfigureDesktopOptimizations(BuildConfiguration config)
        {
            Debug.Log("Riley: Configuring desktop optimizations...");

            // Graphics API
            var graphicsAPIs = new List<GraphicsDeviceType>();
            graphicsAPIs.Add(GraphicsDeviceType.Direct3D11);
            graphicsAPIs.Add(GraphicsDeviceType.OpenGLCore);
            graphicsAPIs.Add(GraphicsDeviceType.Vulkan);

            PlayerSettings.SetGraphicsAPIs(config.targetPlatform, graphicsAPIs.ToArray());
        }

        /// <summary>
        /// Executes the actual build process.
        /// Riley: "Time to execute the build!"
        /// </summary>
        private IEnumerator ExecuteBuild(BuildConfiguration config)
        {
            Debug.Log($"Riley: Executing build for {config.buildName}...");

            var buildPath = GetBuildPath(config);
            var buildOptions = GetBuildOptions(config);

            // Get all scenes
            var scenes = GetEnabledScenes();

            // Execute build
            var buildReport = BuildPipeline.BuildPlayer(scenes, buildPath, config.targetPlatform, buildOptions);

            // Check build result
            if (buildReport.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Riley: Build succeeded! Size: {buildReport.summary.totalSize / 1024 / 1024} MB");
            }
            else
            {
                throw new Exception($"Build failed: {buildReport.summary.result}");
            }

            yield return null;
        }

        /// <summary>
        /// Post-build cleanup and finalization.
        /// Nibble: "Bark! (Translation: Clean up after build!)"
        /// </summary>
        private IEnumerator PostBuildCleanup(BuildConfiguration config)
        {
            Debug.Log($"Riley: Post-build cleanup for {config.buildName}...");

            // Update README if requested
            if (config.updateReadme)
            {
                UpdateReadme(config);
            }

            // Log build information
            LogBuildInformation(config);

            yield return null;
        }

        /// <summary>
        /// Validates the build configuration.
        /// Riley: "Gotta validate the build configuration!"
        /// </summary>
        private bool ValidateBuildConfiguration(BuildConfiguration config)
        {
            if (config == null)
            {
                Debug.LogError("Riley: Build configuration is null!");
                return false;
            }

            if (string.IsNullOrEmpty(config.buildName))
            {
                Debug.LogError("Riley: Build name is empty!");
                return false;
            }

            if (string.IsNullOrEmpty(config.version))
            {
                Debug.LogError("Riley: Version is empty!");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the output directory for the build.
        /// Riley: "Gotta figure out where to put the build!"
        /// </summary>
        private string GetOutputDirectory(BuildConfiguration config)
        {
            var basePath = Path.Combine(Application.dataPath, "..", config.outputPath);
            
            if (config.createFolderPerPlatform)
            {
                return Path.Combine(basePath, config.targetPlatform.ToString());
            }
            
            return basePath;
        }

        /// <summary>
        /// Gets the full build path for the executable.
        /// Nibble: "Bark! (Translation: Get the build path!)"
        /// </summary>
        private string GetBuildPath(BuildConfiguration config)
        {
            var outputDir = GetOutputDirectory(config);
            var extension = GetPlatformExtension(config.targetPlatform);
            return Path.Combine(outputDir, $"{config.buildName}{extension}");
        }

        /// <summary>
        /// Gets the file extension for the target platform.
        /// Riley: "Gotta get the right file extension!"
        /// </summary>
        private string GetPlatformExtension(BuildTarget target)
        {
            switch (target)
            {
                case BuildTarget.StandaloneWindows64:
                    return ".exe";
                case BuildTarget.StandaloneOSX:
                    return ".app";
                case BuildTarget.Android:
                    return ".apk";
                case BuildTarget.iOS:
                    return ""; // iOS builds create a folder
                default:
                    return "";
            }
        }

        /// <summary>
        /// Gets build options based on configuration.
        /// Riley: "Time to set up the build options!"
        /// </summary>
        private BuildOptions GetBuildOptions(BuildConfiguration config)
        {
            var options = BuildOptions.None;

            if (config.developmentBuild)
            {
                options |= BuildOptions.Development;
            }

            if (config.allowDebugging)
            {
                options |= BuildOptions.AllowDebugging;
            }

            return options;
        }

        /// <summary>
        /// Gets all enabled scenes for the build.
        /// Nibble: "Bark! (Translation: Get all the scenes!)"
        /// </summary>
        private string[] GetEnabledScenes()
        {
            var scenes = new List<string>();
            
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorBuildSettings.scenes[i];
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }

            return scenes.ToArray();
        }

        /// <summary>
        /// Updates the README file with build information.
        /// Riley: "Time to update the README with build info!"
        /// </summary>
        private void UpdateReadme(BuildConfiguration config)
        {
            try
            {
                var readmePath = Path.Combine(Application.dataPath, "..", config.readmePath);
                if (File.Exists(readmePath))
                {
                    var content = File.ReadAllText(readmePath);
                    var buildInfo = $"\n\n## Build Information\n- Version: {config.version}\n- Build Number: {config.buildNumber}\n- Platform: {config.targetPlatform}\n- Build Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
                    
                    if (!content.Contains("## Build Information"))
                    {
                        content += buildInfo;
                        File.WriteAllText(readmePath, content);
                        Debug.Log("Nibble: *bark* (Translation: README updated with build info!)");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to update README: {ex.Message}");
            }
        }

        /// <summary>
        /// Logs build information for debugging.
        /// Riley: "Time to log the build information!"
        /// </summary>
        private void LogBuildInformation(BuildConfiguration config)
        {
            Debug.Log($"Riley: Build Information for {config.buildName}:");
            Debug.Log($"- Version: {config.version}");
            Debug.Log($"- Build Number: {config.buildNumber}");
            Debug.Log($"- Platform: {config.targetPlatform}");
            Debug.Log($"- Development Build: {config.developmentBuild}");
            Debug.Log($"- Allow Debugging: {config.allowDebugging}");
            Debug.Log($"- Optimize for Mobile: {config.optimizeForMobile}");
            Debug.Log($"- IL2CPP Enabled: {config.enableIL2CPP}");
        }

        /// <summary>
        /// Gets the current build status.
        /// Riley: "What's the current build status?"
        /// </summary>
        public string GetBuildStatus()
        {
            return currentBuildStatus;
        }

        /// <summary>
        /// Gets the current build progress.
        /// Nibble: "Bark! (Translation: How much progress?)"
        /// </summary>
        public float GetBuildProgress()
        {
            return buildProgress;
        }

        /// <summary>
        /// Checks if a build is currently in progress.
        /// Riley: "Is a build currently running?"
        /// </summary>
        public bool IsBuilding()
        {
            return isBuilding;
        }

        /// <summary>
        /// Gets the build configuration for a specific platform.
        /// Riley: "Get the build config for a platform!"
        /// </summary>
        public BuildConfiguration GetBuildConfiguration(BuildTarget platform)
        {
            switch (platform)
            {
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux64:
                    return pcConfig;
                case BuildTarget.Android:
                    return androidConfig;
                case BuildTarget.iOS:
                    return iosConfig;
                default:
                    return pcConfig;
            }
        }
    }
}