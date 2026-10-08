using System;
using System.IO;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Pockle.Editor
{
    /// <summary>
    /// Imports Blender's explicit Unity-coordinate mesh and face anchors into native
    /// Unity assets. FBX is supplied for DCC interchange; runtime uses this native mesh.
    /// </summary>
    [ScriptedImporter(2, "pocklemesh")]
    public sealed class PipCharacterImporter : ScriptedImporter
    {
        [Serializable]
        private sealed class Source
        {
            public int schemaVersion;
            public string name;
            public bool integratedCrown;
            public float[] positions;
            public int[] triangles;
            public float[] uv;
            public int[] normalGroups;
            public float[] eyes, eyeGlints, cheeks, mouth, crowns, crownScales, pearls, flecks;
            public float[] bodyColor, topColor, bottomColor;
        }

        public override void OnImportAsset(AssetImportContext context)
        {
            Source source = JsonUtility.FromJson<Source>(File.ReadAllText(context.assetPath));
            if (source == null || source.schemaVersion != 1)
                throw new InvalidDataException("Unsupported Pockle character schema.");
            Vector3[] vertices = Triples(source.positions, -1, "positions");
            if (vertices.Length < 4 || vertices.Length > 65000)
                throw new InvalidDataException("Character mesh must have 4..65000 vertices.");
            foreach (Vector3 vertex in vertices)
                if (vertex.y < -1.00001f || vertex.y > 1f || Mathf.Abs(vertex.x) > 1.5f || Mathf.Abs(vertex.z) > 1.5f)
                    throw new InvalidDataException("Rest mesh exceeds the supported deformation coordinates.");
            if (source.triangles == null || source.triangles.Length == 0 || source.triangles.Length % 3 != 0)
                throw new InvalidDataException("Triangles must contain complete indexed faces.");
            foreach (int index in source.triangles)
                if (index < 0 || index >= vertices.Length) throw new InvalidDataException("Invalid triangle index.");
            if (source.normalGroups == null || source.normalGroups.Length != vertices.Length)
                throw new InvalidDataException("Normal weld groups must match mesh vertices.");
            foreach (int group in source.normalGroups)
                if (group < 0 || group >= vertices.Length) throw new InvalidDataException("Invalid normal group.");
            if (source.uv == null || source.uv.Length != vertices.Length * 2)
                throw new InvalidDataException("UV coordinates must match mesh vertices.");
            Vector2[] uv = new Vector2[vertices.Length];
            for (int i = 0; i < uv.Length; i++)
            {
                RequireFinite(source.uv[i * 2]); RequireFinite(source.uv[i * 2 + 1]);
                uv[i] = new Vector2(source.uv[i * 2], source.uv[i * 2 + 1]);
            }

            PipCharacterAsset asset = ScriptableObject.CreateInstance<PipCharacterAsset>();
            asset.name = source.name;
            asset.IntegratedCrown = source.integratedCrown;
            asset.NormalGroups = source.normalGroups;
            asset.Eyes = Triples(source.eyes, 2, "eyes");
            asset.EyeGlints = Triples(source.eyeGlints, 2, "eye glints");
            asset.Cheeks = Triples(source.cheeks, 2, "cheeks");
            asset.Mouth = Triples(source.mouth, 13, "mouth");
            asset.Crowns = Triples(source.crowns, 2, "crowns");
            asset.CrownScales = Triples(source.crownScales, 2, "crown scales");
            foreach (Vector3 scale in asset.CrownScales)
                if (scale.x <= 0 || scale.y <= 0 || scale.z <= 0)
                    throw new InvalidDataException("Crown scales must be positive.");
            asset.Pearls = Triples(source.pearls, 6, "pearls");
            asset.Flecks = Triples(source.flecks, 6, "flecks");
            asset.BodyColor = ReadColor(source.bodyColor);
            asset.TopColor = ReadColor(source.topColor);
            asset.BottomColor = ReadColor(source.bottomColor);

            Mesh mesh = new Mesh { name = "Pip authored body", vertices = vertices, triangles = source.triangles, uv = uv };
            mesh.RecalculateNormals();
            // Smooth split UV seams using the same logical-vertex groups as runtime.
            Vector3[] normals = mesh.normals;
            Vector3[] sums = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++) sums[source.normalGroups[i]] += normals[i];
            for (int i = 0; i < normals.Length; i++) normals[i] = sums[source.normalGroups[i]].normalized;
            mesh.normals = normals;
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false); // CPU deformation requires readable vertices.
            asset.BodyMesh = mesh;
            context.AddObjectToAsset("body", mesh);
            context.AddObjectToAsset("character", asset);
            context.SetMainObject(asset);
        }

        private static Vector3[] Triples(float[] values, int requiredCount, string label)
        {
            if (values == null || values.Length % 3 != 0 || (requiredCount >= 0 && values.Length != requiredCount * 3))
                throw new InvalidDataException("Invalid character " + label + ".");
            Vector3[] result = new Vector3[values.Length / 3];
            for (int i = 0; i < result.Length; i++)
            {
                RequireFinite(values[i * 3]); RequireFinite(values[i * 3 + 1]); RequireFinite(values[i * 3 + 2]);
                result[i] = new Vector3(values[i * 3], values[i * 3 + 1], values[i * 3 + 2]);
            }
            return result;
        }

        private static Color ReadColor(float[] values)
        {
            if (values == null || values.Length != 4) throw new InvalidDataException("Invalid character color.");
            foreach (float value in values)
            {
                RequireFinite(value);
                if (value < 0 || value > 1) throw new InvalidDataException("Color is outside 0..1.");
            }
            return new Color(values[0], values[1], values[2], values[3]);
        }

        private static void RequireFinite(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value)) throw new InvalidDataException("Nonfinite character value.");
        }

        [MenuItem("Pockle/Character/Use authored Pip")]
        private static void UseAuthored()
        {
            PlayerPrefs.SetInt(PipCharacterAsset.BaselinePreference, 0);
            PlayerPrefs.Save();
            Debug.Log("Authored Pip selected for the next Play session.");
        }

        [MenuItem("Pockle/Character/Use procedural baseline")]
        private static void UseBaseline()
        {
            PlayerPrefs.SetInt(PipCharacterAsset.BaselinePreference, 1);
            PlayerPrefs.Save();
            Debug.Log("Procedural Pip baseline selected for the next Play session.");
        }
    }
}
