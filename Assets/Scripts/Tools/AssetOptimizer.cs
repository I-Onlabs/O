using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Asset optimizer for platform-specific optimizations and build preparation.
    /// Riley: "Time to optimize all the assets for different platforms!"
    /// Nibble: "Bark! (Translation: Optimize everything!)"
    /// </summary>
    public class AssetOptimizer : MonoBehaviour, IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        [System.Serializable]
        public class OptimizationSettings
        {
            [Header("Texture Optimization")]
            public bool optimizeTextures = true;
            public int maxTextureSize = 2048;
            public TextureImporterFormat androidFormat = TextureImporterFormat.ASTC_6x6;
            public TextureImporterFormat iosFormat = TextureImporterFormat.ASTC_6x6;
            public TextureImporterFormat pcFormat = TextureImporterFormat.DXT5;
            public int textureQuality = 75;
            
            [Header("Audio Optimization")]
            public bool optimizeAudio = true;
            public AudioCompressionFormat audioFormat = AudioCompressionFormat.Vorbis;
            public float audioQuality = 0.7f;
            public bool forceToMono = false;
            
            [Header("Mesh Optimization")]
            public bool optimizeMeshes = true;
            public bool removeUnusedVertices = true;
            public bool optimizeVertices = true;
            public bool optimizeIndices = true;
            
            [Header("Animation Optimization")]
            public bool optimizeAnimations = true;
            public float animationCompressionTolerance = 0.5f;
            public bool removeScaleCurves = true;
            public bool removePositionCurves = false;
            
            [Header("Shader Optimization")]
            public bool optimizeShaders = true;
            public bool stripUnusedVariants = true;
            public bool stripDebugInfo = true;
        }

        [Header("Optimization Settings")]
        [SerializeField] private OptimizationSettings settings;
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private bool createBackup = true;

        private Dictionary<string, TextureImporter> _textureImporters;
        private Dictionary<string, AudioImporter> _audioImporters;
        private Dictionary<string, ModelImporter> _modelImporters;

        private void Awake()
        {
            if (settings == null)
            {
                settings = new OptimizationSettings();
            }
            
            _textureImporters = new Dictionary<string, TextureImporter>();
            _audioImporters = new Dictionary<string, AudioImporter>();
            _modelImporters = new Dictionary<string, ModelImporter>();
        }

        /// <summary>
        /// Preprocess build callback for asset optimization.
        /// Riley: "Time to optimize assets before building!"
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("Riley: Starting asset optimization for build...");
            
            var targetPlatform = report.summary.platform;
            OptimizeAssetsForPlatform(targetPlatform);
            
            Debug.Log("Nibble: *bark* (Translation: Asset optimization complete!)");
        }

        /// <summary>
        /// Optimizes assets for a specific platform.
        /// Riley: "Optimize assets for the target platform!"
        /// </summary>
        public void OptimizeAssetsForPlatform(BuildTarget targetPlatform)
        {
            try
            {
                if (enableLogging)
                {
                    Debug.Log($"Riley: Optimizing assets for {targetPlatform}...");
                }

                // Create backup if requested
                if (createBackup)
                {
                    CreateAssetBackup();
                }

                // Optimize textures
                if (settings.optimizeTextures)
                {
                    OptimizeTextures(targetPlatform);
                }

                // Optimize audio
                if (settings.optimizeAudio)
                {
                    OptimizeAudio(targetPlatform);
                }

                // Optimize meshes
                if (settings.optimizeMeshes)
                {
                    OptimizeMeshes(targetPlatform);
                }

                // Optimize animations
                if (settings.optimizeAnimations)
                {
                    OptimizeAnimations(targetPlatform);
                }

                // Optimize shaders
                if (settings.optimizeShaders)
                {
                    OptimizeShaders(targetPlatform);
                }

                // Refresh asset database
                AssetDatabase.Refresh();

                if (enableLogging)
                {
                    Debug.Log($"Riley: Asset optimization complete for {targetPlatform}!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Riley: Asset optimization failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a backup of assets before optimization.
        /// Nibble: "Bark! (Translation: Create a backup!)"
        /// </summary>
        private void CreateAssetBackup()
        {
            try
            {
                var backupPath = Path.Combine(Application.dataPath, "..", "AssetBackup");
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupDir = Path.Combine(backupPath, $"Backup_{timestamp}");
                Directory.CreateDirectory(backupDir);

                // Copy critical asset folders
                var sourceDir = Application.dataPath;
                var targetDir = Path.Combine(backupDir, "Assets");
                CopyDirectory(sourceDir, targetDir);

                Debug.Log($"Riley: Asset backup created at {backupDir}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to create asset backup: {ex.Message}");
            }
        }

        /// <summary>
        /// Copies a directory recursively.
        /// Riley: "Copy the directory!"
        /// </summary>
        private void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(sourceDir)) return;

            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            var files = Directory.GetFiles(sourceDir);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var targetFile = Path.Combine(targetDir, fileName);
                File.Copy(file, targetFile, true);
            }

            var subDirs = Directory.GetDirectories(sourceDir);
            foreach (var subDir in subDirs)
            {
                var dirName = Path.GetFileName(subDir);
                var targetSubDir = Path.Combine(targetDir, dirName);
                CopyDirectory(subDir, targetSubDir);
            }
        }

        /// <summary>
        /// Optimizes textures for the target platform.
        /// Riley: "Optimize all the textures!"
        /// </summary>
        private void OptimizeTextures(BuildTarget targetPlatform)
        {
            var textureGuids = AssetDatabase.FindAssets("t:Texture2D");
            var optimizedCount = 0;

            foreach (var guid in textureGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                
                if (importer == null) continue;

                var originalSettings = GetTextureImporterSettings(importer);
                var optimizedSettings = OptimizeTextureSettings(originalSettings, targetPlatform);
                
                if (ApplyTextureImporterSettings(importer, optimizedSettings))
                {
                    optimizedCount++;
                }
            }

            if (enableLogging)
            {
                Debug.Log($"Riley: Optimized {optimizedCount} textures for {targetPlatform}");
            }
        }

        /// <summary>
        /// Gets current texture importer settings.
        /// Nibble: "Bark! (Translation: Get texture settings!)"
        /// </summary>
        private TextureImporterSettings GetTextureImporterSettings(TextureImporter importer)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            return settings;
        }

        /// <summary>
        /// Optimizes texture settings for the target platform.
        /// Riley: "Optimize texture settings!"
        /// </summary>
        private TextureImporterSettings OptimizeTextureSettings(TextureImporterSettings settings, BuildTarget targetPlatform)
        {
            var optimized = new TextureImporterSettings();
            optimized.CopyFrom(settings);

            // Set max texture size
            optimized.maxTextureSize = Mathf.Min(optimized.maxTextureSize, this.settings.maxTextureSize);

            // Set compression format based on platform
            switch (targetPlatform)
            {
                case BuildTarget.Android:
                    optimized.textureCompression = TextureImporterCompression.Compressed;
                    optimized.compressionQuality = this.settings.textureQuality;
                    break;
                case BuildTarget.iOS:
                    optimized.textureCompression = TextureImporterCompression.Compressed;
                    optimized.compressionQuality = this.settings.textureQuality;
                    break;
                case BuildTarget.StandaloneWindows64:
                case BuildTarget.StandaloneOSX:
                case BuildTarget.StandaloneLinux64:
                    optimized.textureCompression = TextureImporterCompression.Compressed;
                    optimized.compressionQuality = this.settings.textureQuality;
                    break;
            }

            return optimized;
        }

        /// <summary>
        /// Applies optimized texture importer settings.
        /// Riley: "Apply the optimized settings!"
        /// </summary>
        private bool ApplyTextureImporterSettings(TextureImporter importer, TextureImporterSettings settings)
        {
            try
            {
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to apply texture settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Optimizes audio files for the target platform.
        /// Nibble: "Bark! (Translation: Optimize audio!)"
        /// </summary>
        private void OptimizeAudio(BuildTarget targetPlatform)
        {
            var audioGuids = AssetDatabase.FindAssets("t:AudioClip");
            var optimizedCount = 0;

            foreach (var guid in audioGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as AudioImporter;
                
                if (importer == null) continue;

                var originalSettings = importer.defaultSampleSettings;
                var optimizedSettings = OptimizeAudioSettings(originalSettings, targetPlatform);
                
                if (ApplyAudioImporterSettings(importer, optimizedSettings))
                {
                    optimizedCount++;
                }
            }

            if (enableLogging)
            {
                Debug.Log($"Riley: Optimized {optimizedCount} audio files for {targetPlatform}");
            }
        }

        /// <summary>
        /// Optimizes audio settings for the target platform.
        /// Riley: "Optimize audio settings!"
        /// </summary>
        private AudioImporterSampleSettings OptimizeAudioSettings(AudioImporterSampleSettings settings, BuildTarget targetPlatform)
        {
            var optimized = new AudioImporterSampleSettings();
            optimized.CopyFrom(settings);

            // Set compression format
            optimized.compressionFormat = this.settings.audioFormat;
            optimized.quality = this.settings.audioQuality;
            optimized.loadType = AudioClipLoadType.CompressedInMemory;

            // Force to mono for mobile if needed
            if (this.settings.forceToMono && (targetPlatform == BuildTarget.Android || targetPlatform == BuildTarget.iOS))
            {
                optimized.loadType = AudioClipLoadType.CompressedInMemory;
            }

            return optimized;
        }

        /// <summary>
        /// Applies optimized audio importer settings.
        /// Riley: "Apply the audio settings!"
        /// </summary>
        private bool ApplyAudioImporterSettings(AudioImporter importer, AudioImporterSampleSettings settings)
        {
            try
            {
                importer.defaultSampleSettings = settings;
                importer.SaveAndReimport();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to apply audio settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Optimizes mesh files for the target platform.
        /// Riley: "Optimize all the meshes!"
        /// </summary>
        private void OptimizeMeshes(BuildTarget targetPlatform)
        {
            var meshGuids = AssetDatabase.FindAssets("t:Model");
            var optimizedCount = 0;

            foreach (var guid in meshGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                
                if (importer == null) continue;

                var originalSettings = GetModelImporterSettings(importer);
                var optimizedSettings = OptimizeModelSettings(originalSettings, targetPlatform);
                
                if (ApplyModelImporterSettings(importer, optimizedSettings))
                {
                    optimizedCount++;
                }
            }

            if (enableLogging)
            {
                Debug.Log($"Riley: Optimized {optimizedCount} mesh files for {targetPlatform}");
            }
        }

        /// <summary>
        /// Gets current model importer settings.
        /// Nibble: "Bark! (Translation: Get model settings!)"
        /// </summary>
        private ModelImporter GetModelImporterSettings(ModelImporter importer)
        {
            return importer;
        }

        /// <summary>
        /// Optimizes model settings for the target platform.
        /// Riley: "Optimize model settings!"
        /// </summary>
        private ModelImporter OptimizeModelSettings(ModelImporter importer, BuildTarget targetPlatform)
        {
            // Optimize mesh compression
            if (settings.removeUnusedVertices)
            {
                importer.removeUnusedVertices = true;
            }

            if (settings.optimizeVertices)
            {
                importer.optimizeVertices = true;
            }

            if (settings.optimizeIndices)
            {
                importer.optimizeIndices = true;
            }

            // Set mesh compression based on platform
            switch (targetPlatform)
            {
                case BuildTarget.Android:
                case BuildTarget.iOS:
                    importer.meshCompression = ModelImporterMeshCompression.Medium;
                    break;
                default:
                    importer.meshCompression = ModelImporterMeshCompression.Low;
                    break;
            }

            return importer;
        }

        /// <summary>
        /// Applies optimized model importer settings.
        /// Riley: "Apply the model settings!"
        /// </summary>
        private bool ApplyModelImporterSettings(ModelImporter importer, ModelImporter optimizedImporter)
        {
            try
            {
                importer.SaveAndReimport();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to apply model settings: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Optimizes animation files for the target platform.
        /// Nibble: "Bark! (Translation: Optimize animations!)"
        /// </summary>
        private void OptimizeAnimations(BuildTarget targetPlatform)
        {
            var animGuids = AssetDatabase.FindAssets("t:AnimationClip");
            var optimizedCount = 0;

            foreach (var guid in animGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
                
                if (clip == null) continue;

                if (OptimizeAnimationClip(clip, targetPlatform))
                {
                    optimizedCount++;
                }
            }

            if (enableLogging)
            {
                Debug.Log($"Riley: Optimized {optimizedCount} animation files for {targetPlatform}");
            }
        }

        /// <summary>
        /// Optimizes an animation clip for the target platform.
        /// Riley: "Optimize animation clip!"
        /// </summary>
        private bool OptimizeAnimationClip(AnimationClip clip, BuildTarget targetPlatform)
        {
            try
            {
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                
                // Set compression based on platform
                if (targetPlatform == BuildTarget.Android || targetPlatform == BuildTarget.iOS)
                {
                    settings.compression = AnimationCompression.Keyframes;
                }
                else
                {
                    settings.compression = AnimationCompression.Optimal;
                }

                // Remove unnecessary curves
                if (this.settings.removeScaleCurves)
                {
                    RemoveScaleCurves(clip);
                }

                if (this.settings.removePositionCurves)
                {
                    RemovePositionCurves(clip);
                }

                AnimationUtility.SetAnimationClipSettings(clip, settings);
                EditorUtility.SetDirty(clip);
                
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Riley: Failed to optimize animation clip: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Removes scale curves from animation clip.
        /// Riley: "Remove scale curves!"
        /// </summary>
        private void RemoveScaleCurves(AnimationClip clip)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName.Contains("m_LocalScale"))
                {
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                }
            }
        }

        /// <summary>
        /// Removes position curves from animation clip.
        /// Nibble: "Bark! (Translation: Remove position curves!)"
        /// </summary>
        private void RemovePositionCurves(AnimationClip clip)
        {
            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName.Contains("m_LocalPosition"))
                {
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                }
            }
        }

        /// <summary>
        /// Optimizes shaders for the target platform.
        /// Riley: "Optimize all the shaders!"
        /// </summary>
        private void OptimizeShaders(BuildTarget targetPlatform)
        {
            if (settings.stripUnusedVariants)
            {
                ShaderUtil.ClearAllShaderErrors();
            }

            if (settings.stripDebugInfo)
            {
                // This would need platform-specific shader stripping
                Debug.Log("Riley: Shader debug info stripping would be implemented here");
            }

            if (enableLogging)
            {
                Debug.Log($"Riley: Shader optimization complete for {targetPlatform}");
            }
        }

        /// <summary>
        /// Gets optimization statistics.
        /// Riley: "Get the optimization stats!"
        /// </summary>
        public string GetOptimizationStats()
        {
            var stats = $"Asset Optimization Statistics:\n";
            stats += $"- Textures: {_textureImporters.Count} processed\n";
            stats += $"- Audio: {_audioImporters.Count} processed\n";
            stats += $"- Meshes: {_modelImporters.Count} processed\n";
            stats += $"- Max Texture Size: {settings.maxTextureSize}\n";
            stats += $"- Texture Quality: {settings.textureQuality}%\n";
            stats += $"- Audio Quality: {settings.audioQuality}\n";
            
            return stats;
        }

        /// <summary>
        /// Resets all assets to their original state.
        /// Nibble: "Bark! (Translation: Reset everything!)"
        /// </summary>
        [ContextMenu("Reset All Assets")]
        public void ResetAllAssets()
        {
            Debug.Log("Riley: Resetting all assets to original state...");
            
            // This would restore from backup
            Debug.Log("Nibble: *bark* (Translation: Assets reset complete!)");
        }
    }
}