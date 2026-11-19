using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>How a rounded surface catches light (UX-053, "Jelly glass").</summary>
    public enum GlassStyle
    {
        /// <summary>Clear glass when the colour is see-through, a softly lit solid when it is not.</summary>
        Auto,
        /// <summary>Frosted glass behind words: shows the blurred page painting through its tint.</summary>
        Frost,
        /// <summary>A glossy jelly button with a soft shadow.</summary>
        Jelly,
        /// <summary>The plain rounded shape, with no light or shadow (brand sign-in buttons, thin tracks).</summary>
        Flat
    }

    /// <summary>
    /// Turns an Image into one quad that the Pockle/UI/Glass shader draws as a rounded, lit surface. The Image keeps its
    /// colour, raycasts and masking; its corner radius still comes from pixelsPerUnitMultiplier (PrototypeHud.SetRadius).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class GlassSurface : BaseMeshEffect
    {
        /// <summary>Room around the surface for its shadow, in reference pixels.</summary>
        public const float Margin = 14f;
        [SerializeField] private GlassStyle style = GlassStyle.Auto;

        public GlassStyle Style
        {
            get => style;
            set { if (style == value) return; style = value; if (graphic != null) graphic.SetVerticesDirty(); }
        }

        /// <summary>Rim, gloss, shadow and sheen strengths for a style and tint.</summary>
        public static Vector4 Light(GlassStyle style, Color tint)
        {
            switch (style)
            {
                case GlassStyle.Frost: return new Vector4(.8f, .18f, .45f, 0f);
                case GlassStyle.Jelly: return new Vector4(.6f, 1f, .9f, 0f);
                case GlassStyle.Flat: return Vector4.zero;
                default: return tint.a < .995f ? new Vector4(1f, .45f, .22f, 1f) : new Vector4(.4f, .3f, 0f, 0f);
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || graphic == null) return;
            var image = graphic as Image;
            var rect = graphic.rectTransform.rect;
            Vector2 half = rect.size * .5f, centre = rect.center;
            float radius = image != null ? 64f / Mathf.Max(.0001f, image.pixelsPerUnitMultiplier) : 0f;
            Color tint = graphic.color;
            Vector4 light = Light(style, tint);
            // Thin tracks, dots and dividers are too small to carry a rim or a shadow.
            float small = Mathf.Clamp01((Mathf.Min(half.x, half.y) - 4f) / 8f);
            light *= small;
            float margin = light.z > 0f ? Margin : 1f;
            var shape = new Vector4(half.x, half.y, radius, style == GlassStyle.Frost ? 1f : 0f);
            vh.Clear();
            for (int i = 0; i < 4; i++)
            {
                float sx = i == 0 || i == 1 ? -1f : 1f, sy = i == 0 || i == 3 ? -1f : 1f;
                var local = new Vector2(sx * (half.x + margin), sy * (half.y + margin));
                vh.AddVert(new UIVertex
                {
                    position = new Vector3(centre.x + local.x, centre.y + local.y, 0f),
                    color = tint, uv0 = new Vector4(local.x, local.y, 0f, 0f), uv1 = shape, uv2 = light
                });
            }
            vh.AddTriangle(0, 1, 2); vh.AddTriangle(2, 3, 0);
        }
    }
}
