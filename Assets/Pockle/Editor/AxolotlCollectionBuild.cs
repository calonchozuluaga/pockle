using System;
using System.Collections.Generic;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    public static class AxolotlCollectionBuild
    {
        public static void Build()
        {
            const string review = "Assets/Pockle/Art/Models/Axolotl/";
            const string folder = "Assets/Pockle/Resources/Characters/Axolotl";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Pockle/Resources/Characters", "Axolotl");
            AssetDatabase.ImportAsset(review + "Axolotl.fbx", ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(review + "AxolotlBaseColor.png", ImportAssetOptions.ForceUpdate);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(review + "Axolotl.fbx");
            var paint = AssetDatabase.LoadAssetAtPath<Texture2D>(review + "AxolotlBaseColor.png");

            if (model == null || paint == null) throw new InvalidOperationException("Axolotl review import is incomplete: model=" + (model != null) + " paint=" + (paint != null));
            var filters = model.GetComponentsInChildren<MeshFilter>();
            if (filters.Length != 1) throw new InvalidOperationException("Expected one axolotl mesh.");
            var filter = filters[0];
            Mesh mesh = UnityEngine.Object.Instantiate(filter.sharedMesh);
            var vertices = mesh.vertices;
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0)) * filter.transform.localToWorldMatrix;
            Bounds bounds = new Bounds(matrix.MultiplyPoint3x4(vertices[0]), Vector3.zero);
            for (int i = 0; i < vertices.Length; i++) { vertices[i] = matrix.MultiplyPoint3x4(vertices[i]); bounds.Encapsulate(vertices[i]); }
            float scale = Mathf.Min(1.94f / bounds.size.y, 2.05f / bounds.size.x, 2.05f / bounds.size.z);
            var groups = new int[vertices.Length]; var positions = new Dictionary<Vector3, int>();
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3((vertices[i].x - bounds.center.x) * scale,
                    (vertices[i].y - bounds.min.y) * scale - 1f, (vertices[i].z - bounds.center.z) * scale);
                if (!positions.TryGetValue(vertices[i], out int group)) { group = i; positions.Add(vertices[i], i); }
                groups[i] = group;
            }
            if (vertices.Length > 65000) throw new InvalidOperationException("Axolotl exceeds the readable toy mesh limit.");
            mesh.name = "Axolotl plush body"; mesh.vertices = vertices; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            // Smooth duplicated UV-seam vertices together, matching runtime normal groups.
            var normals = mesh.normals; var sums = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++) sums[groups[i]] += normals[i];
            for (int i = 0; i < normals.Length; i++) normals[i] = sums[groups[i]].normalized;
            mesh.normals = normals;
            var asset = ScriptableObject.CreateInstance<PipCharacterAsset>();
            asset.name = "Axolotl · Gloss Plush"; asset.CharacterId = "axolotl";
            asset.BodyMesh = mesh; asset.NormalGroups = groups; asset.BakedSurface = true;
            asset.BaseColorTexture = paint; asset.IntegratedCrown = true;
            asset.Eyes = asset.EyeGlints = asset.Cheeks = asset.Mouth = asset.Crowns = asset.CrownScales = asset.Pearls = asset.Flecks = Array.Empty<Vector3>();
            asset.BodyColor = asset.TopColor = asset.BottomColor = Color.white;
            const string path = folder + "/Axolotl.asset";
            var existing = AssetDatabase.LoadAssetAtPath<PipCharacterAsset>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(mesh, existing.BodyMesh);
                existing.NormalGroups = groups;
                EditorUtility.SetDirty(existing.BodyMesh); EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(mesh); UnityEngine.Object.DestroyImmediate(asset);
            }
            else
            {
                AssetDatabase.CreateAsset(asset, path); AssetDatabase.AddObjectToAsset(mesh, asset);
                var plush = new Material(Resources.Load<Shader>("Characters/Axolotl/AxolotlPlush")) { name = "Axolotl plush runtime", mainTexture = paint };
                AssetDatabase.CreateAsset(plush, folder + "/AxolotlPlush.mat");
            }
            AssetDatabase.SaveAssets();
            Debug.Log("AXOLOTL_COLLECTION_BUILD " + vertices.Length + " vertices / " + filter.sharedMesh.triangles.Length / 3 + " triangles.");
        }
    }
}
