using System;
using System.Collections.Generic;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    public static class WinterTrioCollectionBuild
    {
        public static void Build()
        {
            foreach (string id in new[] { "reiny", "guin", "bundy" }) BuildOne(id);
        }
        private static void BuildOne(string id)
        {
            string title = Pockle.Core.HolidayCatalog.Title(id);
            string review = "Assets/Pockle/Art/Models/" + title + "/";
            string folder = "Assets/Pockle/Resources/Characters/" + title;
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Pockle/Resources/Characters", title);
            AssetDatabase.ImportAsset(review + title + ".fbx", ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(review + title + "BaseColor.png", ImportAssetOptions.ForceUpdate);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(review + title + ".fbx");
            var paint = AssetDatabase.LoadAssetAtPath<Texture2D>(review + title + "BaseColor.png");

            if (model == null || paint == null) throw new InvalidOperationException("Christmas review import is incomplete: model=" + (model != null) + " paint=" + (paint != null));
            var filters = model.GetComponentsInChildren<MeshFilter>();
            if (filters.Length != 1) throw new InvalidOperationException("Expected one winter toy mesh.");
            var filter = filters[0];
            Mesh mesh = new Mesh { name = title + " readable body", vertices = filter.sharedMesh.vertices, triangles = filter.sharedMesh.triangles, uv = filter.sharedMesh.uv };
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
            if (vertices.Length > 65000) throw new InvalidOperationException("Christmas exceeds the readable toy mesh limit.");
            mesh.name = "Winter toy body"; mesh.vertices = vertices; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            // Smooth duplicated UV-seam vertices together, matching runtime normal groups.
            var normals = mesh.normals; var sums = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++) sums[groups[i]] += normals[i];
            for (int i = 0; i < normals.Length; i++) normals[i] = sums[groups[i]].normalized;
            mesh.normals = normals;
            var asset = ScriptableObject.CreateInstance<PipCharacterAsset>();
            asset.name = title + " · Classic"; asset.CharacterId = id;
            asset.BodyMesh = mesh; asset.NormalGroups = groups; asset.BakedSurface = true;
            asset.BaseColorTexture = paint; asset.IntegratedCrown = true;
            asset.Eyes = asset.EyeGlints = asset.Cheeks = asset.Mouth = asset.Crowns = asset.CrownScales = asset.Pearls = asset.Flecks = Array.Empty<Vector3>();
            asset.BodyColor = asset.TopColor = asset.BottomColor = Color.white;
            string path = folder + "/" + title + ".asset";
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

            }
            AssetDatabase.SaveAssets();
            Debug.Log("HOLIDAY_COLLECTION_BUILD " + vertices.Length + " vertices / " + filter.sharedMesh.triangles.Length / 3 + " triangles.");
        }
    }
}
