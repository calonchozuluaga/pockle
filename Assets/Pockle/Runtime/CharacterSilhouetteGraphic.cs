using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Face-free character outlines. Pip projects its real body; planned shapes follow ART-001's silhouette study.</summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasRenderer))]
    public sealed class CharacterSilhouetteGraphic : MaskableGraphic
    {
        private Vector2[] points;
        private int[] indices;
        public string CharacterId { get; private set; }
        public bool ConcealsSecret { get; private set; }

        public void Configure(string characterId, bool secret = false)
        {
            CharacterId = characterId; ConcealsSecret = secret; raycastTarget = false;
            if (!secret && characterId == "pip")
            {
                var asset = Resources.Load<PipCharacterAsset>(PipCharacterAsset.ResourcePath);
                if (asset != null && asset.BodyMesh != null && asset.BodyMesh.isReadable)
                {
                    var vertices = asset.BodyMesh.vertices; points = new Vector2[vertices.Length];
                    var bounds = asset.BodyMesh.bounds;
                    float size = Mathf.Max(bounds.size.x, bounds.size.y);
                    for (int i = 0; i < points.Length; i++)
                        points[i] = new Vector2(vertices[i].x - bounds.center.x, vertices[i].y - bounds.center.y) / size;
                    indices = asset.BodyMesh.triangles; SetVerticesDirty(); return;
                }
            }
            // Outline-only studies, not models or available collectibles. Secret identity never enters this switch.
            float[] outline;
            switch (secret ? "mystery" : characterId)
            {
                case "dew": outline = new float[] { -.45f,-.3f,-.5f,-.1f,-.43f,.08f,-.25f,.1f,-.18f,.26f,-.04f,.34f,0,.48f,.1f,.5f,.2f,.4f,.12f,.34f,.06f,.4f,.03f,.27f,.17f,.13f,.35f,.12f,.47f,.03f,.5f,-.12f,.43f,-.3f,.5f,-.4f,.34f,-.5f,-.25f,-.5f,-.48f,-.42f }; break;
                case "tula": outline = new float[] { -.5f,.16f,-.38f,.28f,0,.32f,.38f,.28f,.5f,.16f,.45f,.02f,.44f,-.3f,.33f,-.33f,.28f,-.19f,.15f,-.2f,0,-.32f,-.15f,-.2f,-.28f,-.19f,-.33f,-.33f,-.44f,-.3f,-.45f,.02f }; break;
                case "ripple": outline = new float[] { -.5f,-.02f,-.48f,.12f,-.4f,.1f,-.36f,.02f,-.27f,.04f,-.14f,.12f,0,.15f,.14f,.12f,.27f,.04f,.36f,.02f,.4f,.1f,.48f,.12f,.5f,-.02f,.46f,-.14f,.25f,-.22f,0,-.25f,-.25f,-.22f,-.46f,-.14f }; break;
                case "moss": outline = new float[] { -.3f,-.5f,-.35f,-.39f,-.38f,-.19f,-.35f,-.03f,-.46f,.08f,-.5f,.05f,-.48f,.23f,-.35f,.34f,-.14f,.4f,0,.5f,.14f,.4f,.35f,.34f,.48f,.23f,.5f,.05f,.46f,.08f,.35f,-.03f,.38f,-.19f,.35f,-.39f,.3f,-.5f,.16f,-.5f,.08f,-.44f,-.08f,-.44f,-.16f,-.5f }; break;
                case "nook": outline = new float[] { -.5f,.33f,-.47f,.44f,-.36f,.4f,.36f,.4f,.47f,.44f,.5f,.33f,.43f,.24f,.43f,-.28f,.48f,-.38f,.42f,-.44f,.29f,-.4f,-.29f,-.4f,-.42f,-.44f,-.48f,-.38f,-.43f,-.28f,-.43f,.24f }; break;
                case "wisp": outline = new float[] { -.22f,-.5f,-.27f,-.39f,-.42f,-.14f,-.44f,.09f,-.35f,.28f,-.16f,.34f,.01f,.3f,.17f,.42f,.22f,.48f,.12f,.5f,.05f,.43f,.03f,.56f,.15f,.62f,.28f,.59f,.38f,.39f,.29f,.22f,.35f,.02f,.31f,-.2f,.22f,-.39f,.2f,-.5f }; break;
                case "loop": outline = new float[] { -.13f,-.38f,-.1f,-.26f,-.22f,-.07f,-.36f,-.11f,-.49f,-.02f,-.5f,.16f,-.37f,.25f,-.22f,.19f,-.14f,.1f,0,.21f,.14f,.1f,.22f,.19f,.37f,.25f,.5f,.16f,.49f,-.02f,.36f,-.11f,.22f,-.07f,.1f,-.26f,.13f,-.38f }; break;
                case "bop": outline = new float[] { -.25f,-.5f,-.3f,-.43f,-.23f,-.31f,-.37f,-.16f,-.5f,-.02f,-.46f,.13f,-.28f,.23f,.06f,.24f,.04f,.37f,.13f,.47f,.18f,.5f,.23f,.4f,.23f,.25f,.39f,.18f,.5f,.04f,.43f,-.09f,.26f,-.25f,.2f,-.33f,.29f,-.43f,.24f,-.5f,.13f,-.5f,.08f,-.42f,-.12f,-.42f,-.15f,-.5f }; break;
                case "rolo": outline = new float[] { -.31f,-.5f,-.35f,-.38f,-.4f,-.3f,-.4f,-.04f,-.5f,-.1f,-.54f,-.02f,-.5f,.06f,-.4f,.02f,-.4f,.29f,-.3f,.43f,.3f,.43f,.4f,.29f,.4f,.02f,.5f,.06f,.54f,-.02f,.5f,-.1f,.4f,-.04f,.4f,-.3f,.35f,-.38f,.31f,-.5f,.17f,-.5f,.13f,-.44f,-.13f,-.44f,-.17f,-.5f }; break;
                case "mallow": outline = new float[] { -.34f,-.5f,-.43f,-.39f,-.43f,.15f,-.39f,.28f,-.4f,.43f,-.3f,.55f,-.11f,.6f,-.09f,.55f,-.23f,.49f,-.28f,.39f,-.26f,.28f,0,.31f,.32f,.28f,.43f,.2f,.4f,.1f,.43f,-.39f,.34f,-.5f }; break;
                case "sprig": outline = new float[] { -.15f,-.5f,-.2f,-.4f,-.32f,-.23f,-.35f,-.04f,-.3f,.17f,-.14f,.33f,-.3f,.43f,-.31f,.51f,-.23f,.54f,-.08f,.49f,-.03f,.59f,.03f,.59f,.08f,.49f,.23f,.54f,.31f,.51f,.3f,.43f,.14f,.33f,.3f,.17f,.35f,-.04f,.32f,-.23f,.2f,-.4f,.15f,-.5f,.07f,-.5f,0,-.43f,-.07f,-.5f }; break;
                default: outline = new float[] { -.3f,-.4f,-.42f,-.22f,-.4f,.04f,-.28f,.25f,-.18f,.33f,-.22f,.44f,.22f,.44f,.18f,.33f,.28f,.25f,.4f,.04f,.42f,-.22f,.3f,-.4f }; break;
            }
            var control = new Vector2[outline.Length / 2];
            for (int i = 0; i < control.Length; i++) control[i] = new Vector2(outline[i * 2], outline[i * 2 + 1]) * .8f;
            // Quadratic corner rounding stays inside the hand-authored contour.
            points = new Vector2[control.Length * 4];
            for (int i = 0; i < control.Length; i++)
            {
                Vector2 before = (control[(i + control.Length - 1) % control.Length] + control[i]) * .5f;
                Vector2 after = (control[i] + control[(i + 1) % control.Length]) * .5f;
                for (int sample = 0; sample < 4; sample++)
                { float t = sample * .25f; points[i * 4 + sample] = before * ((1 - t) * (1 - t)) + control[i] * (2 * t * (1 - t)) + after * (t * t); }
            }
            indices = Triangulate(points); SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear(); if (points == null) return;
            Rect rect = GetPixelAdjustedRect(); float size = Mathf.Min(rect.width, rect.height) * .9f;
            foreach (var p in points) vh.AddVert(rect.center + p * size, color, Vector2.zero);
            for (int i = 0; i < indices.Length; i += 3) vh.AddTriangle(indices[i], indices[i + 1], indices[i + 2]);
            if (CharacterId == "loop" && !ConcealsSecret)
            {
                AddHole(vh, rect.center + new Vector2(-.28f, .065f) * size * .8f, size);
                AddHole(vh, rect.center + new Vector2(.28f, .065f) * size * .8f, size);
            }
        }

        private static void AddHole(VertexHelper vh, Vector2 center, float size)
        {
            int first = vh.currentVertCount;
            vh.AddVert(center, PockleTheme.Mystery, Vector2.zero);
            for (int i = 0; i <= 24; i++)
            { float a = i * Mathf.PI * 2 / 24; vh.AddVert(center + new Vector2(Mathf.Cos(a) * .075f, Mathf.Sin(a) * .055f) * size, PockleTheme.Mystery, Vector2.zero); }
            for (int i = 0; i < 24; i++) vh.AddTriangle(first, first + i + 1, first + i + 2);
        }

        private static int[] Triangulate(Vector2[] polygon)
        {
            var remaining = new List<int>(); var triangles = new List<int>();
            float area = 0; for (int i = 0; i < polygon.Length; i++) area += Cross(polygon[i], polygon[(i + 1) % polygon.Length]);
            for (int i = 0; i < polygon.Length; i++) remaining.Add(area > 0 ? i : polygon.Length - 1 - i);
            int guard = polygon.Length * polygon.Length;
            while (remaining.Count > 2 && guard-- > 0)
            {
                bool clipped = false;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int a = remaining[(i + remaining.Count - 1) % remaining.Count], b = remaining[i], c = remaining[(i + 1) % remaining.Count];
                    if (Cross(polygon[b] - polygon[a], polygon[c] - polygon[b]) <= 0) continue;
                    bool inside = false;
                    foreach (int p in remaining)
                        if (p != a && p != b && p != c && Cross(polygon[b] - polygon[a], polygon[p] - polygon[a]) >= 0 &&
                            Cross(polygon[c] - polygon[b], polygon[p] - polygon[b]) >= 0 && Cross(polygon[a] - polygon[c], polygon[p] - polygon[c]) >= 0)
                        { inside = true; break; }
                    if (inside) continue;
                    triangles.Add(a); triangles.Add(b); triangles.Add(c); remaining.RemoveAt(i); clipped = true; break;
                }
                if (!clipped) break;
            }
            return triangles.ToArray();
        }
        private static float Cross(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x;
    }
}
