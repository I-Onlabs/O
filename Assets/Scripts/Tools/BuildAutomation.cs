using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Build automation script for CI/CD pipelines and automated deployment.
    /// Riley: "Time to automate the entire build process! No more manual errors!"
    /// Nibble: "Bark! (Translation: Automated builds for everyone!)"
    /// </summary>
    public static class BuildAutomation
    {
        private const string BUILD_CONFIG_PATH = "Assets/Scripts/Tools/BuildConfig.json";
        private const string VERSION_FILE_PATH = "Assets/StreamingAssets/version.txt";
        private const string CHANGELOG_PATH = "CHANGELOG.md";

        /// <summary>
        /// Main entry point for automated builds from command line.
        /// Riley: "This is where the magic happens - automated builds!"
        /// </summary>
        public static void BuildFromCommandLine()
        {
            try
            {
                Debug.Log("Riley: Starting automated build process!");
                
                // Parse command line arguments
                var args = Environment.GetCommandLineArgs();
                var buildTarget = ParseBuildTarget(args);
                var buildType = ParseBuildType(args);
                
                // Load build configuration
                var config = LoadBuildConfiguration();
                
                // Update version if needed
                UpdateVersion(config);
                
                // Execute build
                ExecuteAutomatedBuild(buildTarget, buildType, config);
                
                Debug.Log("Nibble: *bark* (Translation: Automated build completed successfully!)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Automated build failed: {ex.Message}");
                EditorApplication.Exit(1);
            }
        }

        /// <summary>
        /// Parses build target from command line arguments.
        /// Riley: "Gotta figure out what platform to build for!"
        /// </summary>
        private static BuildTarget ParseBuildTarget(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-buildTarget" && i + 1 < args.Length)
                {
                    var targetString = args[i + 1].ToLower();
                    switch (targetString)
                    {
                        case "pc":
                        case "windows":
                            return BuildTarget.StandaloneWindows64;
                        case "android":
                            return BuildTarget.Android;
                        case "ios":
                        case "iphone":
                            return BuildTarget.iOS;
                        case "mac":
                        case "osx":
                            return BuildTarget.StandaloneOSX;
                        case "linux":
                            return BuildTarget.StandaloneLinux64;
                    }
                }
            }
            
            // Default to PC if no target specified
            return BuildTarget.StandaloneWindows64;
        }

        /// <summary>
        /// Parses build type from command line arguments.
        /// Nibble: "Bark! (Translation: Figure out the build type!)"
        /// </summary>
        private static string ParseBuildType(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-buildType" && i + 1 < args.Length)
                {
                    return args[i + 1].ToLower();
                }
            }
            
            return "release"; // Default to release build
        }

        /// <summary>
        /// Loads build configuration from JSON file.
        /// Riley: "Time to load the build configuration!"
        /// </summary>
        private static BuildConfiguration LoadBuildConfiguration()
        {
            var configPath = Path.Combine(Application.dataPath, "..", BUILD_CONFIG_PATH);
            
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                return JsonUtility.FromJson<BuildConfiguration>(json);
            }
            
            // Return default configuration if file doesn't exist
            return CreateDefaultBuildConfiguration();
        }

        /// <summary>
        /// Creates a default build configuration.
        /// Riley: "Creating default build configuration!"
        /// </summary>
        private static BuildConfiguration CreateDefaultBuildConfiguration()
        {
            return new BuildConfiguration
            {
                buildName = "AngryDogs",
                version = "1.0.0",
                buildNumber = 1,
                developmentBuild = false,
                allowDebugging = false,
                targetPlatform = BuildTarget.StandaloneWindows64,
                outputPath = "Builds",
                createFolderPerPlatform = true,
                optimizeForMobile = true,
                compressTextures = true,
                stripUnusedCode = true,
                enableIL2CPP = true,
                useASTC = true,
                useETC2 = false,
                textureCompressionQuality = 50,
                optimizeMeshData = true,
                autoIncrementBuildNumber = true,
                updateReadme = true,
                readmePath = "README.md"
            };
        }

        /// <summary>
        /// Updates version information for the build.
        /// Nibble: "Bark! (Translation: Update the version!)"
        /// </summary>
        private static void UpdateVersion(BuildConfiguration config)
        {
            try
            {
                // Update version in PlayerSettings
                PlayerSettings.bundleVersion = config.version;
                PlayerSettings.Android.bundleVersionCode = config.buildNumber;
                PlayerSettings.iOS.buildNumber = config.buildNumber.ToString();

                // Create version file for runtime access
                var versionDir = Path.GetDirectoryName(VERSION_FILE_PATH);
                if (!Directory.Exists(versionDir))
                {
                    Directory.CreateDirectory(versionDir);
                }

                var versionInfo = $"{config.version}.{config.buildNumber}";
                File.WriteAllText(VERSION_FILE_PATH, versionInfo);

                Debug.Log($"Riley: Version updated to {versionInfo}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to update version: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes the automated build process.
        /// Riley: "Time to execute the automated build!"
        /// </summary>
        private static void ExecuteAutomatedBuild(BuildTarget target, string buildType, BuildConfiguration config)
        {
            Debug.Log($"Riley: Building {target} as {buildType} build...");

            // Configure build settings
            ConfigureBuildSettings(target, buildType, config);

            // Get build path
            var buildPath = GetBuildPath(target, config);

            // Get enabled scenes
            var scenes = GetEnabledScenes();

            // Configure build options
            var buildOptions = GetBuildOptions(buildType);

            // Execute build
            var buildReport = BuildPipeline.BuildPlayer(scenes, buildPath, target, buildOptions);

            // Check result
            if (buildReport.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Riley: Build succeeded! Output: {buildPath}");
                Debug.Log($"Riley: Build size: {buildReport.summary.totalSize / 1024 / 1024} MB");
                
                // Update changelog
                UpdateChangelog(config, buildType);
            }
            else
            {
                throw new Exception($"Build failed: {buildReport.summary.result}");
            }
        }

        /// <summary>
        /// Configures build settings for the target platform and build type.
        /// Riley: "Time to configure the build settings!"
        /// </summary>
        private static void ConfigureBuildSettings(BuildTarget target, string buildType, BuildConfiguration config)
        {
            // Set build target
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(target), target);

            // Configure development build
            EditorUserBuildSettings.development = buildType == "development";
            EditorUserBuildSettings.allowDebugging = buildType == "development";

            // Configure scripting backend
            if (config.enableIL2CPP)
            {
                PlayerSettings.SetScriptingBackend(BuildPipeline.GetBuildTargetGroup(target), ScriptingImplementation.IL2CPP);
            }

            // Configure code stripping
            if (config.stripUnusedCode && buildType == "release")
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildPipeline.GetBuildTargetGroup(target), Il2CppCompilerConfiguration.Master);
            }

            // Platform-specific settings
            ConfigurePlatformSpecificSettings(target, config);
        }

        /// <summary>
        /// Configures platform-specific settings.
        /// Nibble: "Bark! (Translation: Configure platform settings!)"
        /// </summary>
        private static void ConfigurePlatformSpecificSettings(BuildTarget target, BuildConfiguration config)
        {
            switch (target)
            {
                case BuildTarget.Android:
                    ConfigureAndroidSettings(config);
                    break;
                case BuildTarget.iOS:
                    ConfigureiOSSettings(config);
                    break;
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux64:
                    ConfigureDesktopSettings(config);
                    break;
            }
        }

        /// <summary>
        /// Configures Android-specific settings.
        /// Riley: "Time to configure Android settings!"
        /// </summary>
        private static void ConfigureAndroidSettings(BuildConfiguration config)
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.preferredInstallLocation = AndroidPreferredInstallLocation.Auto;
            PlayerSettings.Android.useCustomKeystore = false; // Use default for automated builds
            
            // Graphics API
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3, GraphicsDeviceType.Vulkan });
        }

        /// <summary>
        /// Configures iOS-specific settings.
        /// Riley: "Time to configure iOS settings!"
        /// </summary>
        private static void ConfigureiOSSettings(BuildConfiguration config)
        {
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            
            // Graphics API
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
        }

        /// <summary>
        /// Configures desktop-specific settings.
        /// Riley: "Time to configure desktop settings!"
        /// </summary>
        private static void ConfigureDesktopSettings(BuildConfiguration config)
        {
            // Graphics API
            var graphicsAPIs = new[] { GraphicsDeviceType.Direct3D11, GraphicsDeviceType.OpenGLCore, GraphicsDeviceType.Vulkan };
            PlayerSettings.SetGraphicsAPIs(config.targetPlatform, graphicsAPIs);
        }

        /// <summary>
        /// Gets the build path for the target platform.
        /// Riley: "Gotta figure out where to put the build!"
        /// </summary>
        private static string GetBuildPath(BuildTarget target, BuildConfiguration config)
        {
            var outputDir = Path.Combine(Application.dataPath, "..", config.outputPath);
            
            if (config.createFolderPerPlatform)
            {
                outputDir = Path.Combine(outputDir, target.ToString());
            }
            
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            var extension = GetPlatformExtension(target);
            return Path.Combine(outputDir, $"{config.buildName}{extension}");
        }

        /// <summary>
        /// Gets the file extension for the target platform.
        /// Nibble: "Bark! (Translation: Get the file extension!)"
        /// </summary>
        private static string GetPlatformExtension(BuildTarget target)
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
        /// Gets all enabled scenes for the build.
        /// Riley: "Gotta get all the enabled scenes!"
        /// </summary>
        private static string[] GetEnabledScenes()
        {
            var scenes = new System.Collections.Generic.List<string>();
            
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
        /// Gets build options based on build type.
        /// Riley: "Time to set up the build options!"
        /// </summary>
        private static BuildOptions GetBuildOptions(string buildType)
        {
            var options = BuildOptions.None;

            if (buildType == "development")
            {
                options |= BuildOptions.Development;
                options |= BuildOptions.AllowDebugging;
            }

            return options;
        }

        /// <summary>
        /// Updates the changelog with build information.
        /// Riley: "Time to update the changelog!"
        /// </summary>
        private static void UpdateChangelog(BuildConfiguration config, string buildType)
        {
            try
            {
                var changelogPath = Path.Combine(Application.dataPath, "..", CHANGELOG_PATH);
                var changelogEntry = $"\n## Version {config.version}.{config.buildNumber} - {DateTime.Now:yyyy-MM-dd}\n- Build Type: {buildType}\n- Platform: {config.targetPlatform}\n- Automated Build\n\n";
                
                if (File.Exists(changelogPath))
                {
                    var content = File.ReadAllText(changelogPath);
                    if (!content.Contains($"Version {config.version}.{config.buildNumber}"))
                    {
                        content = changelogEntry + content;
                        File.WriteAllText(changelogPath, content);
                        Debug.Log("Nibble: *bark* (Translation: Changelog updated!)");
                    }
                }
                else
                {
                    File.WriteAllText(changelogPath, $"# Changelog\n{changelogEntry}");
                    Debug.Log("Riley: Changelog created!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to update changelog: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a build configuration JSON file.
        /// Riley: "Time to create the build configuration file!"
        /// </summary>
        [MenuItem("Tools/Angry Dogs/Create Build Configuration")]
        public static void CreateBuildConfiguration()
        {
            var config = CreateDefaultBuildConfiguration();
            var json = JsonUtility.ToJson(config, true);
            var configPath = Path.Combine(Application.dataPath, "..", BUILD_CONFIG_PATH);
            
            var configDir = Path.GetDirectoryName(configPath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }
            
            File.WriteAllText(configPath, json);
            Debug.Log("Riley: Build configuration created!");
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Validates the build configuration.
        /// Nibble: "Bark! (Translation: Validate the build config!)"
        /// </summary>
        [MenuItem("Tools/Angry Dogs/Validate Build Configuration")]
        public static void ValidateBuildConfiguration()
        {
            var config = LoadBuildConfiguration();
            var isValid = true;
            var errors = new System.Collections.Generic.List<string>();

            if (string.IsNullOrEmpty(config.buildName))
            {
                errors.Add("Build name is empty");
                isValid = false;
            }

            if (string.IsNullOrEmpty(config.version))
            {
                errors.Add("Version is empty");
                isValid = false;
            }

            if (config.buildNumber < 1)
            {
                errors.Add("Build number must be at least 1");
                isValid = false;
            }

            if (isValid)
            {
                Debug.Log("Riley: Build configuration is valid!");
            }
            else
            {
                Debug.LogError($"Riley: Build configuration has errors:\n{string.Join("\n", errors)}");
            }
        }
    }
}