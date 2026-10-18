using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    // The fox is an art review asset, deliberately outside Resources/catalog loading.
    [InitializeOnLoad]
    public sealed class FoxReviewImport : AssetPostprocessor
    {
        private const string Folder = "Assets/Pockle/Art/Models/Fox";
        static FoxReviewImport() { EditorApplication.delayCall += EnsureReviewPrefab; }

        private void OnPreprocessModel()
        {
            if (assetPath != Folder + "/Fox.fbx") return;
            var importer = (ModelImporter)assetImporter;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.isReadable = true;
            importer.importAnimation = false;
        }

        private void OnPreprocessTexture()
        {
            if (assetPath != Folder + "/FoxBaseColor.png") return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.maxTextureSize = 2048;
            importer.mipmapEnabled = true;
        }

        private static void OnPostprocessAllAssets(string[] imported, string[] deleted,
            string[] moved, string[] movedFrom)
        {
            foreach (string path in imported)
                if (path == Folder + "/Fox.fbx" || path == Folder + "/FoxBaseColor.png")
                { EditorApplication.delayCall += EnsureReviewPrefab; break; }
        }

        [MenuItem("Pockle/Character/Select imported fox review prefab")]
        private static void EnsureReviewPrefab()
        {
            string prefabPath = Folder + "/FoxReview.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return; // Preserve subsequent owner material/prefab edits.
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Fox.fbx");
            var paint = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/FoxBaseColor.png");
            if (model == null || paint == null) return;
            string materialPath = Folder + "/FoxVinyl.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Standard");
                if (shader == null) { Debug.LogError("Fox review needs Built-in Standard shader."); return; }
                material = new Material(shader) { name = "Fox vinyl", mainTexture = paint };
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Glossiness", .6f);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            var instance = Object.Instantiate(model);
            instance.name = "Fox art review";
            try
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                    renderer.sharedMaterial = material;
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Selection.activeObject = prefab;
                Debug.Log("Fox imported for art review. Not yet wired to collectible inventory or tactile play.");
            }
            finally { Object.DestroyImmediate(instance); }
        }
    }
}
