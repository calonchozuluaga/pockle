using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Small white glyphs drawn once at runtime and tinted by the UI: a four-point sparkle and a crescent moon.</summary>
    public static class UiShapes
    {
        private static Texture2D sparkle, moon, glow;

        /// <summary>The sparkle from the v2 canvas (a four-point star, 24-unit path).</summary>
        public static Texture2D Sparkle => sparkle != null ? sparkle : sparkle = Draw("Pockle sparkle", 64, InsideSparkle);
        public static Texture2D Moon => moon != null ? moon : moon = Draw("Pockle crescent", 64, InsideMoon);
        private static Texture2D fadeDown, fadeRight;
        /// <summary>White, clear at the top and opaque at the bottom: tinted, it darkens a banner behind white text.</summary>
        public static Texture2D FadeDown => fadeDown != null ? fadeDown : fadeDown = Ramp("Pockle fade down", false);
        /// <summary>White, opaque at the left and clear at the right: tinted milk, it lets text sit on a painting.</summary>
        /// <summary>A soft round glow, white in the middle and fading to clear at the edge, tinted by the UI.</summary>
        public static Texture2D Glow
        {
            get
            {
                if (glow != null) return glow;
                const int size = 64;
                glow = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Pockle glow", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                var pixels = new Color32[size * size];
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float dx = (x + .5f) / size * 2f - 1f, dy = (y + .5f) / size * 2f - 1f;
                        float a = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy)); a = a * a * (3f - 2f * a);
                        pixels[y * size + x] = new Color32(255, 255, 255, (byte)(255 * a * a));
                    }
                glow.SetPixels32(pixels); glow.Apply(false, true);
                return glow;
            }
        }

        public static Texture2D FadeRight => fadeRight != null ? fadeRight : fadeRight = Ramp("Pockle fade right", true);

        private static Texture2D Ramp(string name, bool horizontal)
        {
            const int size = 64;
            var texture = new Texture2D(horizontal ? size : 1, horizontal ? 1 : size, TextureFormat.RGBA32, false)
            { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size];
            for (int i = 0; i < size; i++)
            {
                float t = i / (size - 1f);
                // Horizontal: solid for the first half, then eases out. Vertical (rows bottom-up): solid at the bottom, clear by 60% height.
                float a = horizontal ? 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - .45f) / .45f)) : 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .6f));
                pixels[i] = new Color32(255, 255, 255, (byte)(255 * a));
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return texture;
        }

        private static readonly Vector2[] star =
        {
            new Vector2(12, 1), new Vector2(14.6f, 9.4f), new Vector2(23, 12), new Vector2(14.6f, 14.6f),
            new Vector2(12, 23), new Vector2(9.4f, 14.6f), new Vector2(1, 12), new Vector2(9.4f, 9.4f)
        };

        private static bool InsideSparkle(Vector2 p)
        {
            bool inside = false;
            for (int i = 0, j = star.Length - 1; i < star.Length; j = i++)
                if ((star[i].y > p.y) != (star[j].y > p.y) &&
                    p.x < (star[j].x - star[i].x) * (p.y - star[i].y) / (star[j].y - star[i].y) + star[i].x)
                    inside = !inside;
            return inside;
        }

        private static bool InsideMoon(Vector2 p)
        {
            // A disc with a bite taken from its upper right.
            return (p - new Vector2(12, 12)).sqrMagnitude <= 9.5f * 9.5f && (p - new Vector2(16.5f, 8.5f)).sqrMagnitude > 8f * 8f;
        }

        private static Texture2D Draw(string name, int size, System.Func<Vector2, bool> inside)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[size * size];
            const int samples = 4;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                int hits = 0;
                for (int sy = 0; sy < samples; sy++)
                for (int sx = 0; sx < samples; sx++)
                {
                    // Path space is 24 units, y down; texture rows run bottom-up.
                    float px = (x + (sx + .5f) / samples) / size * 24f;
                    float py = 24f - (y + (sy + .5f) / samples) / size * 24f;
                    if (inside(new Vector2(px, py))) hits++;
                }
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(255 * hits / (samples * samples)));
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return texture;
        }
    }
}
