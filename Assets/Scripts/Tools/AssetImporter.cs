#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Automates import settings for neon cyberpunk assets to keep draw calls and memory low.
    /// Enhanced to address level dressing saturation risks and optimize for mobile performance.
    /// Riley: "Gotta keep these assets optimized for mobile. Can't have lag when hounds are chasing!"
    /// </summary>
    public sealed class AssetImporter : AssetPostprocessor
    {
        private static readonly string[] NeonKeywords = { "neon", "holo", "glow", "cyber", "laser" };
        private static readonly string[] ParticleKeywords = { "fx", "particle", "vfx", "explosion", "spark", "smoke" };
        private static readonly string[] LevelDressingKeywords = { "building", "prop", "decoration", "furniture", "street" };
        private static readonly string[] HoundKeywords = { "hound", "dog", "cyber", "augment", "mech" };
        
        // Performance tracking
        private static int _totalAssetsProcessed = 0;
        private static int _levelDressingCount = 0;
        private static int _neonEffectCount = 0;
        private static int _particleEffectCount = 0;
        
        // Level dressing saturation limits
        private const int MAX_LEVEL_DRESSING_PER_SCENE = 50;
        private const int MAX_NEON_EFFECTS_PER_SCENE = 20;
        private const int MAX_PARTICLE_SYSTEMS_PER_SCENE = 15;

        void OnPreprocessModel()
        {
            var importer = (ModelImporter)assetImporter;
            importer.importMaterials = false; // Materials handled by addressable pipeline.
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.isReadable = false;
            importer.animationCompression = ModelImporterAnimationCompression.Optimal;
            importer.animationRotationError = 0.5f;
            importer.animationPositionError = 0.5f;
        }

        void OnPostprocessModel(GameObject gameObject)
        {
            // Auto-add lightweight LODGroups for skyscrapers and chunky props.
            if (!gameObject.TryGetComponent<LODGroup>(out _))
            {
                var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
                var rendererCount = renderers.Length;
                if (rendererCount > 4)
                {
                    var lodGroup = gameObject.AddComponent<LODGroup>();
                    lodGroup.fadeMode = LODFadeMode.CrossFade;
                    lodGroup.animateCrossFading = true;
                    lodGroup.size = 1f;
                    lodGroup.SetLODs(new[]
                    {
                        new LOD(0.6f, renderers),
                        new LOD(0.2f, System.Array.Empty<Renderer>())
                    });
                }
            }
        }

        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            var assetPath = importer.assetPath;
            
            // Track asset processing
            _totalAssetsProcessed++;
            
            // Check for level dressing saturation
            CheckLevelDressingSaturation(assetPath);
            
            // Optimize based on asset type
            if (IsLevelDressingTexture(assetPath))
            {
                OptimizeLevelDressingTexture(importer);
            }
            else if (IsNeonTexture(assetPath))
            {
                OptimizeNeonTexture(importer);
            }
            else if (IsHoundTexture(assetPath))
            {
                OptimizeHoundTexture(importer);
            }
            else
            {
                OptimizeStandardTexture(importer);
            }
        }

        void OnPreprocessAnimation()
        {
            var importer = (ModelImporter)assetImporter;
            importer.resampleCurves = false;
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (!path.StartsWith("Assets/Art/"))
                {
                    continue;
                }

                var fileInfo = new FileInfo(path);
                if (fileInfo.Length > 50 * 1024 * 1024)
                {
                    Debug.LogWarning($"{Path.GetFileName(path)} is thicc (>{fileInfo.Length / (1024 * 1024)}MB). Consider reducing texture size or mesh detail.");
                }
            }
        }

        private static bool IsFxTexture(string assetPath)
        {
            assetPath = assetPath.ToLowerInvariant();
            foreach (var keyword in ParticleKeywords)
            {
                if (assetPath.Contains(keyword))
                {
                    return true;
                }
            }

            foreach (var keyword in NeonKeywords)
            {
                if (assetPath.Contains(keyword))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if a texture is level dressing to track saturation.
        /// Riley: "Gotta keep track of how many props we're adding!"
        /// </summary>
        private static bool IsLevelDressingTexture(string assetPath)
        {
            assetPath = assetPath.ToLowerInvariant();
            foreach (var keyword in LevelDressingKeywords)
            {
                if (assetPath.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a texture is neon/cyberpunk effect.
        /// Nibble: "Bark! (Translation: Is this a neon effect texture?)"
        /// </summary>
        private static bool IsNeonTexture(string assetPath)
        {
            assetPath = assetPath.ToLowerInvariant();
            foreach (var keyword in NeonKeywords)
            {
                if (assetPath.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a texture is hound-related.
        /// Riley: "Is this a hound texture that needs special optimization?"
        /// </summary>
        private static bool IsHoundTexture(string assetPath)
        {
            assetPath = assetPath.ToLowerInvariant();
            foreach (var keyword in HoundKeywords)
            {
                if (assetPath.Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks for level dressing saturation and warns if limits are exceeded.
        /// Riley: "Too many props can slow down the game! Gotta keep track!"
        /// </summary>
        private static void CheckLevelDressingSaturation(string assetPath)
        {
            if (IsLevelDressingTexture(assetPath))
            {
                _levelDressingCount++;
                if (_levelDressingCount > MAX_LEVEL_DRESSING_PER_SCENE)
                {
                    Debug.LogWarning($"Riley: Level dressing saturation warning! {_levelDressingCount} props detected. " +
                                   $"Consider reducing props or using LOD groups. Max recommended: {MAX_LEVEL_DRESSING_PER_SCENE}");
                }
            }
            else if (IsNeonTexture(assetPath))
            {
                _neonEffectCount++;
                if (_neonEffectCount > MAX_NEON_EFFECTS_PER_SCENE)
                {
                    Debug.LogWarning($"Nibble: *bark* (Translation: Too many neon effects! {_neonEffectCount} detected. " +
                                   $"Max recommended: {MAX_NEON_EFFECTS_PER_SCENE})");
                }
            }
            else if (IsFxTexture(assetPath))
            {
                _particleEffectCount++;
                if (_particleEffectCount > MAX_PARTICLE_SYSTEMS_PER_SCENE)
                {
                    Debug.LogWarning($"Riley: Particle effect saturation warning! {_particleEffectCount} effects detected. " +
                                   $"Consider reducing particle systems. Max recommended: {MAX_PARTICLE_SYSTEMS_PER_SCENE}");
                }
            }
        }

        /// <summary>
        /// Optimizes level dressing textures to reduce draw calls and memory usage.
        /// Riley: "Gotta keep the level dressing efficient. Too many props can slow down the game!"
        /// </summary>
        private static void OptimizeLevelDressingTexture(TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.CompressedLQ; // Lower quality for props
            importer.mipmapEnabled = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.anisoLevel = 0; // No aniso for props
            importer.crunchedCompression = true;
            importer.compressionQuality = 50; // Lower quality for props

            // Mobile optimization
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                format = TextureImporterFormat.ASTC_8x8, // Lower quality for props
                maxTextureSize = 512, // Smaller size for props
                compressionQuality = 50
            });

            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                format = TextureImporterFormat.ASTC_8x8,
                maxTextureSize = 512,
                compressionQuality = 50
            });

            Debug.Log($"Nibble: *bark* (Translation: Optimized level dressing texture {importer.assetPath} for performance!)");
        }

        /// <summary>
        /// Optimizes neon effect textures for mobile performance.
        /// Riley: "Neon effects need to look good but not kill performance!"
        /// </summary>
        private static void OptimizeNeonTexture(TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.mipmapEnabled = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.sRGBTexture = false; // Neon effects don't need sRGB
            importer.alphaIsTransparency = true;
            importer.anisoLevel = 1; // Minimal aniso for effects
            importer.crunchedCompression = true;
            importer.compressionQuality = 70; // Balanced quality

            // Mobile optimization
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                format = TextureImporterFormat.ASTC_6x6,
                maxTextureSize = 1024,
                compressionQuality = 70
            });

            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                format = TextureImporterFormat.ASTC_6x6,
                maxTextureSize = 1024,
                compressionQuality = 70
            });

            Debug.Log($"Riley: Optimized neon texture {importer.assetPath} for mobile performance!");
        }

        /// <summary>
        /// Optimizes hound-related textures for mobile performance.
        /// Nibble: "Bark! (Translation: Optimize the hound textures!)"
        /// </summary>
        private static void OptimizeHoundTexture(TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.mipmapEnabled = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.anisoLevel = 2; // Higher aniso for character textures
            importer.crunchedCompression = true;
            importer.compressionQuality = 80;

            // Mobile optimization
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                format = TextureImporterFormat.ASTC_6x6,
                maxTextureSize = 1024,
                compressionQuality = 80
            });

            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                format = TextureImporterFormat.ASTC_6x6,
                maxTextureSize = 1024,
                compressionQuality = 80
            });

            Debug.Log($"Riley: Optimized hound texture {importer.assetPath} for mobile performance!");
        }

        /// <summary>
        /// Optimizes standard textures for mobile performance.
        /// Riley: "Standard textures need to be efficient too!"
        /// </summary>
        private static void OptimizeStandardTexture(TextureImporter importer)
        {
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.mipmapEnabled = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.anisoLevel = 1;
            importer.crunchedCompression = true;
            importer.compressionQuality = 80;

            // Mobile optimization
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                format = TextureImporterFormat.ASTC_6x6,
                maxTextureSize = 1024,
                compressionQuality = 80
            });

            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                format = TextureImporterFormat.ASTC_6x6,
                maxTextureSize = 1024,
                compressionQuality = 80
            });
        }

        /// <summary>
        /// Resets the asset processing counters (call this when starting a new scene).
        /// Riley: "Time to reset the counters for a new scene!"
        /// </summary>
        [MenuItem("Tools/Angry Dogs/Reset Asset Counters")]
        public static void ResetAssetCounters()
        {
            _totalAssetsProcessed = 0;
            _levelDressingCount = 0;
            _neonEffectCount = 0;
            _particleEffectCount = 0;
            
            Debug.Log("Nibble: *bark* (Translation: Asset counters reset! Ready for new scene!)");
        }

        /// <summary>
        /// Shows current asset processing statistics.
        /// Riley: "Let me check the asset processing stats!"
        /// </summary>
        [MenuItem("Tools/Angry Dogs/Show Asset Statistics")]
        public static void ShowAssetStatistics()
        {
            Debug.Log($"Riley: Asset Processing Statistics:");
            Debug.Log($"  Total Assets Processed: {_totalAssetsProcessed}");
            Debug.Log($"  Level Dressing Count: {_levelDressingCount} (Max: {MAX_LEVEL_DRESSING_PER_SCENE})");
            Debug.Log($"  Neon Effects Count: {_neonEffectCount} (Max: {MAX_NEON_EFFECTS_PER_SCENE})");
            Debug.Log($"  Particle Effects Count: {_particleEffectCount} (Max: {MAX_PARTICLE_SYSTEMS_PER_SCENE})");
            
            if (_levelDressingCount > MAX_LEVEL_DRESSING_PER_SCENE)
            {
                Debug.LogWarning("Riley: Level dressing saturation detected! Consider reducing props.");
            }
            
            if (_neonEffectCount > MAX_NEON_EFFECTS_PER_SCENE)
            {
                Debug.LogWarning("Nibble: *bark* (Translation: Too many neon effects! Consider reducing them.)");
            }
            
            if (_particleEffectCount > MAX_PARTICLE_SYSTEMS_PER_SCENE)
            {
                Debug.LogWarning("Riley: Particle effect saturation detected! Consider reducing particle systems.");
            }
        }
    }
}
#endif
