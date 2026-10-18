using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    // The axolotl is an art review asset, deliberately outside Resources/catalog loading.
    [InitializeOnLoad]
    public sealed class AxolotlReviewImport : AssetPostprocessor
    {
        private const string Folder = "Assets/Pockle/Art/Models/Axolotl";
        static AxolotlReviewImport() { EditorApplication.delayCall += EnsureReviewPrefab; }

        private void OnPreprocessModel()
        {
            if (assetPath != Folder + "/Axolotl.fbx") return;
            var importer = (ModelImporter)assetImporter;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.isReadable = true;
            importer.importAnimation = false;
        }

        private void OnPreprocessTexture()
        {
            if (assetPath != Folder + "/AxolotlBaseColor.png") return;
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
                if (path == Folder + "/Axolotl.fbx" || path == Folder + "/AxolotlBaseColor.png")
                { EditorApplication.delayCall += EnsureReviewPrefab; break; }
        }

        [MenuItem("Pockle/Character/Select imported axolotl review prefab")]
        private static void EnsureReviewPrefab()
        {
            string prefabPath = Folder + "/AxolotlReview.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return; // Preserve subsequent owner material/prefab edits.
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "/Axolotl.fbx");
            var paint = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "/AxolotlBaseColor.png");
            if (model == null || paint == null) return;
            string materialPath = Folder + "/AxolotlPlush.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Standard");
                if (shader == null) { Debug.LogError("Axolotl review needs Built-in Standard shader."); return; }
                material = new Material(shader) { name = "Axolotl plush", mainTexture = paint };
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Glossiness", .12f);
                AssetDatabase.CreateAsset(material, materialPath);
            }
            var instance = Object.Instantiate(model);
            instance.name = "Axolotl art review";
            try
            {
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                    renderer.sharedMaterial = material;
                var prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                Selection.activeObject = prefab;
                Debug.Log("Axolotl imported for art review. Not yet wired to collectible inventory or tactile play.");
            }
            finally { Object.DestroyImmediate(instance); }
        }
    }
}
