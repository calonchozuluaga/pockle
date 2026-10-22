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

        private static Texture2D ring, swap;
        private static readonly System.Collections.Generic.Dictionary<int, Texture2D> arcs = new System.Collections.Generic.Dictionary<int, Texture2D>();
        private const int RingSize = 256;
        private const float RingOuter = .5f, RingInner = .38f;

        /// <summary>A plain white ring (the track behind a progress arc), tinted by the UI.</summary>
        public static Texture2D Ring => ring != null ? ring : ring = DrawRing("Pockle ring", 1f, false);

        /// <summary>A progress arc from twelve o'clock, clockwise, with round ends, shading jelly pink to gold. Not tinted.</summary>
        public static Texture2D ProgressArc(float fraction)
        {
            int key = Mathf.RoundToInt(Mathf.Clamp01(fraction) * 100);
            if (arcs.TryGetValue(key, out var texture) && texture != null) return texture;
            return arcs[key] = DrawRing("Pockle progress arc " + key, key / 100f, true);
        }

        /// <summary>Two curved arrows chasing each other: the trade/swap glyph.</summary>
        public static Texture2D Swap => swap != null ? swap : swap = Draw("Pockle swap", 128, InsideSwap);

        private static Texture2D DrawRing(string name, float fraction, bool gradient)
        {
            var texture = new Texture2D(RingSize, RingSize, TextureFormat.RGBA32, false)
            { name = name, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[RingSize * RingSize];
            Color pink = new Color(1f, .561f, .69f), gold = new Color(.949f, .702f, .239f);
            float mid = (RingOuter + RingInner) / 2, half = (RingOuter - RingInner) / 2, sweep = fraction * 360f;
            Vector2 Cap(float degrees) { float r = degrees * Mathf.Deg2Rad; return new Vector2(Mathf.Sin(r), Mathf.Cos(r)) * mid; }
            Vector2 start = Cap(0), end = Cap(sweep);
            const int samples = 3;
            for (int y = 0; y < RingSize; y++)
            for (int x = 0; x < RingSize; x++)
            {
                int hits = 0;
                for (int sy = 0; sy < samples; sy++)
                for (int sx = 0; sx < samples; sx++)
                {
                    var p = new Vector2((x + (sx + .5f) / samples) / RingSize - .5f, (y + (sy + .5f) / samples) / RingSize - .5f);
                    float r = p.magnitude;
                    bool band = r <= RingOuter && r >= RingInner;
                    if (fraction < 1f)
                    {
                        // Angle clockwise from twelve o'clock (texture rows run bottom-up, so +y is up).
                        float angle = Mathf.Atan2(p.x, p.y) * Mathf.Rad2Deg; if (angle < 0) angle += 360f;
                        band = band && angle <= sweep || (p - start).magnitude <= half || (p - end).magnitude <= half;
                        if (fraction <= 0f) band = false;
                    }
                    if (band) hits++;
                }
                if (hits == 0) { pixels[y * RingSize + x] = new Color32(255, 255, 255, 0); continue; }
                Color colour = Color.white;
                if (gradient)
                {
                    var q = new Vector2((x + .5f) / RingSize - .5f, (y + .5f) / RingSize - .5f);
                    float angle = Mathf.Atan2(q.x, q.y) * Mathf.Rad2Deg; if (angle < 0) angle += 360f;
                    // Past the sweep: the end cap stays gold, the start cap (just before twelve o'clock) stays pink.
                    float t = angle <= sweep ? angle / Mathf.Max(1f, sweep) : angle > (sweep + 360f) / 2 ? 0f : 1f;
                    colour = Color.Lerp(pink, gold, t);
                }
                colour.a = hits / (float)(samples * samples);
                pixels[y * RingSize + x] = colour;
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return texture;
        }

        private static bool InsideSwap(Vector2 p)
        {
            // Two half-turn arrows; the second is the first turned 180 degrees about the centre.
            return InsideSwapHalf(p) || InsideSwapHalf(new Vector2(24 - p.x, 24 - p.y));
        }

        private static bool InsideSwapHalf(Vector2 p)
        {
            // Path space is 24 units, y down. An arc over the top from 160 to 34 degrees, ending in an arrowhead.
            var d = new Vector2(p.x - 12, 12 - p.y);
            float r = d.magnitude, angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            if (r >= 5.6f && r <= 8.2f && angle >= 34f && angle <= 160f) return true;
            if ((d - Polar(160f, 6.9f)).magnitude <= 1.3f) return true; // round tail
            float end = 34f * Mathf.Deg2Rad;
            Vector2 e = Polar(34f, 6.9f), radial = new Vector2(Mathf.Cos(end), Mathf.Sin(end)), tangent = new Vector2(Mathf.Sin(end), -Mathf.Cos(end));
            return InTriangle(d, e + radial * 3.4f, e - radial * 3.4f, e + tangent * 4.2f);
        }

        private static Vector2 Polar(float degrees, float radius)
        {
            float r = degrees * Mathf.Deg2Rad; return new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * radius;
        }

        private static bool InTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float Cross(Vector2 u, Vector2 v, Vector2 w) => (v.x - u.x) * (w.y - u.y) - (v.y - u.y) * (w.x - u.x);
            float d1 = Cross(a, b, p), d2 = Cross(b, c, p), d3 = Cross(c, a, p);
            bool negative = d1 < 0 || d2 < 0 || d3 < 0, positive = d1 > 0 || d2 > 0 || d3 > 0;
            return !(negative && positive);
        }

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
