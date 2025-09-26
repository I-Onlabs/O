#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AngryDogs.Tools
{
    /// <summary>
    /// Automates import settings for neon cyberpunk assets to keep draw calls and memory low.
    /// Drop into an Editor folder or wrap with UNITY_EDITOR as done here.
    /// </summary>
    public sealed class AssetImporter : AssetPostprocessor
    {
        private static readonly string[] NeonKeywords = { "neon", "holo", "glow" };
        private static readonly string[] ParticleKeywords = { "fx", "particle", "vfx", "explosion" };

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
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.mipmapEnabled = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.sRGBTexture = !IsFxTexture(importer.assetPath);

            if (IsFxTexture(importer.assetPath))
            {
                importer.alphaIsTransparency = true;
                importer.anisoLevel = 2;
            }
            else
            {
                importer.alphaIsTransparency = false;
                importer.anisoLevel = 1;
            }

            importer.crunchedCompression = true;
            importer.compressionQuality = 80;

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
    }
}
#endif
