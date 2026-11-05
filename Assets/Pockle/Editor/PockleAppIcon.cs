using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.IO;
using System.Linq;
using Pockle.Core;
using UnityEditor.Build.Reporting;
#if UNITY_ANDROID
using UnityEditor.Android;
#elif UNITY_IOS
using UnityEditor.iOS;
#endif

namespace Pockle.Editor
{
    public static class PockleAppIcon
    {
        public const string AssetPath = "Assets/Pockle/Art/AppIcon/PockleIcon.png";

        [MenuItem("Pockle/Apply approved app icon")]
        public static void Apply()
        {
            var importer = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Approved icon is missing.");
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetPath);
            if (texture == null || texture.width != 1024 || texture.height != 1024)
                throw new InvalidOperationException("Approved icon did not import as a 1024px Texture2D.");
            // Keep the approved artwork intact. Android uses it as the background
            // image with an empty foreground, never as an opaque foreground tile.
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { texture }, IconKind.Any);
#if UNITY_ANDROID
            const string emptyPath = "Assets/Pockle/Art/AppIcon/TransparentForeground.asset";
            var empty = AssetDatabase.LoadAssetAtPath<Texture2D>(emptyPath);
            if (empty == null)
            {
                empty = new Texture2D(4, 4, TextureFormat.RGBA32, false) { name = "Transparent adaptive foreground" };
                empty.SetPixels(new Color[16]); empty.Apply();
                AssetDatabase.CreateAsset(empty, emptyPath);
            }
            var background = AdaptiveBackground();
            var icons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive);
            foreach (var icon in icons) { icon.SetTexture(background, 0); icon.SetTexture(empty, 1); }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, AndroidPlatformIconKind.Adaptive, icons);
#elif UNITY_IOS
            var appIcons = PlayerSettings.GetPlatformIcons(NamedBuildTarget.iOS, iOSPlatformIconKind.Application);
            foreach (var icon in appIcons) icon.SetTexture(texture);
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.iOS, iOSPlatformIconKind.Application, appIcons);
#endif
            AssetDatabase.SaveAssets();
            if (PlayerSettings.GetIcons(NamedBuildTarget.Unknown, IconKind.Any)[0] != texture)
                throw new InvalidOperationException("App icon was not saved.");
            Debug.Log("POCKLE_APPROVED_ICON_OK:" + AssetPath);
        }

#if UNITY_ANDROID
        private static Texture2D AdaptiveBackground()
        {
            const string path = "Assets/Pockle/Art/AppIcon/AdaptiveBackground.asset";
            var background = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            bool create = background == null;
            if (create) background = new Texture2D(1024, 1024, TextureFormat.RGB24, false) { name = "Approved icon with adaptive safe area" };
            var source = new Texture2D(2, 2, TextureFormat.RGB24, false);
            try
            {
                if (!ImageConversion.LoadImage(source, File.ReadAllBytes(AssetPath)))
                    throw new BuildFailedException("Cannot decode the adaptive icon source.");
                source.wrapMode = TextureWrapMode.Clamp;
                // Android's 108dp layer has a 72dp viewport. Fit the entire approved
                // master in that viewport, extending its edge colours into the inset.
                for (int y = 0; y < 1024; y++) for (int x = 0; x < 1024; x++)
                    background.SetPixel(x, y, source.GetPixelBilinear(((x + .5f) / 1024 - .5f) * 1.5f + .5f,
                        ((y + .5f) / 1024 - .5f) * 1.5f + .5f));
                background.Apply();
                if (create) AssetDatabase.CreateAsset(background, path);
                EditorUtility.SetDirty(background);
                return background;
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
        }
#endif

        public static void PrepareCheckout()
        {
            Apply();
            var rows = ToyCatalog.Collectibles.Where(t => t.Available && PaidBoxProducts.All.Any(p => p.CollectionId == t.CollectionId));
            File.WriteAllLines("/private/tmp/pockle-paid-catalog.tsv", rows.Select(t => t.Id + "\t" + t.CollectionId + "\t" + (int)t.Tier));
            Debug.Log("POCKLE_PAID_CATALOG_EXPORTED");
        }
    }

    public sealed class PockleAppIconBuild : IPreprocessBuildWithReport
    {
        public int callbackOrder => -50;
        public void OnPreprocessBuild(BuildReport report)
        {
            try { PockleAppIcon.Apply(); }
            catch (Exception e) { throw new BuildFailedException("Approved Pockle icon failed: " + e.Message); }
        }
    }
}
