using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>A small static studio environment, generated once for candy reflections.</summary>
    internal static class StudioReflection
    {
        public static Cubemap Create()
        {
            const int size = 64;
            TextureFormat format = SystemInfo.SupportsTextureFormat(TextureFormat.RGBAHalf)
                ? TextureFormat.RGBAHalf : TextureFormat.RGBA32;
            var cube = new Cubemap(size, format, true)
            {
                name = "Pockle · warm studio reflection",
                filterMode = FilterMode.Trilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color[size * size];
            for (int face = 0; face < 6; face++)
            {
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float u = 2f * (x + .5f) / size - 1f;
                    float v = 2f * (y + .5f) / size - 1f;
                    Vector3 direction = FaceDirection(face, u, v).normalized;
                    float key = Panel(direction, new Vector3(.5f, .64f, -.57f), .24f, .40f);
                    float strip = Panel(direction, new Vector3(-.72f, .12f, -.68f), .06f, .60f);
                    float back = Panel(direction, new Vector3(.62f, .25f, .74f), .15f, .48f);
                    Color room = new Color(.035f, .028f, .021f, 1f);
                    Color light = new Color(1f, .98f, .92f, 0f) * (key * 1.65f + strip * .85f + back * .55f);
                    pixels[y * size + x] = room + light;
                }
                cube.SetPixels(pixels, (CubemapFace)face);
            }
            cube.Apply(true, true);
            return cube;
        }

        private static Vector3 FaceDirection(int face, float u, float v)
        {
            switch ((CubemapFace)face)
            {
                case CubemapFace.PositiveX: return new Vector3(1f, -v, -u);
                case CubemapFace.NegativeX: return new Vector3(-1f, -v, u);
                case CubemapFace.PositiveY: return new Vector3(u, 1f, v);
                case CubemapFace.NegativeY: return new Vector3(u, -1f, -v);
                case CubemapFace.PositiveZ: return new Vector3(u, -v, 1f);
                default: return new Vector3(-u, -v, -1f);
            }
        }

        private static float Panel(Vector3 direction, Vector3 center, float width, float height)
        {
            center.Normalize();
            if (Vector3.Dot(direction, center) < .5f) return 0f;
            Vector3 across = Vector3.Cross(center, Vector3.up).normalized;
            Vector3 along = Vector3.Cross(across, center);
            float x = Vector3.Dot(direction, across) / width;
            float y = Vector3.Dot(direction, along) / height;
            float edge = Mathf.Clamp01(1f - x * x * x * x - y * y * y * y);
            return Mathf.Pow(edge, .18f);
        }
    }
}
