using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// A toy portrait with a transparent background, for pictures that tilt and bob (live shelf, personality page).
    /// A portrait on a solid colour shows its square corners as soon as it rotates, so the toy is rendered once on
    /// black and once on white: where the two agree the pixel is toy, where they differ by the full range it is
    /// background, and partly see-through finishes land in between. One-time cost per toy, no idle cameras.
    /// </summary>
    internal static class PortraitCutout
    {
        /// <summary>Renders <paramref name="id"/> cut out. <paramref name="feet"/> is the lowest solid row as a fraction
        /// from the bottom, or -1 if none was found.</summary>
        public static Texture2D Render(string id, out float feet)
        {
            feet = -1f;
            RenderTexture onBlack = null, onWhite = null;
            try
            {
                onBlack = ToyPortrait.Render(id, Color.black);
                onWhite = ToyPortrait.Render(id, Color.white);
                int width = onBlack.width, height = onBlack.height;
                var dark = Read(onBlack); var light = Read(onWhite);
                var pixels = new Color32[dark.Length];
                for (int i = 0; i < pixels.Length; i++)
                {
                    Color32 b = dark[i], w = light[i];
                    float spread = ((w.r - b.r) + (w.g - b.g) + (w.b - b.b)) / (3f * 255f);
                    float alpha = Mathf.Clamp01(1f - spread);
                    if (alpha < .02f) { pixels[i] = new Color32(0, 0, 0, 0); continue; }
                    pixels[i] = new Color32(Unmix(b.r, alpha), Unmix(b.g, alpha), Unmix(b.b, alpha), (byte)Mathf.RoundToInt(alpha * 255f));
                }
                for (int y = 0; y < height && feet < 0; y++)
                {
                    int solid = 0;
                    for (int x = 0; x < width; x++)
                        if (pixels[y * width + x].a > 128 && ++solid >= 3) { feet = Mathf.Clamp(y / (float)height, 0f, .45f); break; }
                }
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                { name = "Cutout · " + id, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                return texture;
            }
            finally
            {
                if (onBlack != null) { onBlack.Release(); Object.Destroy(onBlack); }
                if (onWhite != null) { onWhite.Release(); Object.Destroy(onWhite); }
            }
        }

        // The render on black is colour × alpha; dividing by alpha gives back the straight colour the UI shader expects.
        private static byte Unmix(byte premultiplied, float alpha) => (byte)Mathf.Clamp(Mathf.RoundToInt(premultiplied / alpha), 0, 255);

        private static Color32[] Read(RenderTexture source)
        {
            var previous = RenderTexture.active;
            var pixels = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = source;
                pixels.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                return pixels.GetPixels32();
            }
            finally { RenderTexture.active = previous; Object.Destroy(pixels); }
        }
    }
}
