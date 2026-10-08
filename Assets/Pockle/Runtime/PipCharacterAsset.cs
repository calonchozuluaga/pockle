using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Readable mesh and rest anchors imported from the Blender authoring source.</summary>
    public sealed class PipCharacterAsset : ScriptableObject
    {
        public const string ResourcePath = "Pip/Pip";
        public const string BaselinePreference = "pockle.prototype.proceduralPip";
        public Mesh BodyMesh;
        public int[] NormalGroups;
        public Vector3[] Eyes;
        public Vector3[] EyeGlints;
        public Vector3[] Cheeks;
        public Vector3[] Mouth;
        public Vector3[] Crowns;
        public Vector3[] CrownScales;
        public Vector3[] Pearls;
        public Vector3[] Flecks;
        public Color BodyColor;
        public Color TopColor;
        public Color BottomColor;
    }
}
