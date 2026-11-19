using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// Shared pieces of the "Jelly glass" look (UX-053): the glass material, the toned page painting and its blurred copy.
    /// The blurred copy is made once at run time from a painting the app already ships, so the look adds no pictures
    /// to the download.
    /// </summary>
    public static class PockleGlass
    {
        private static readonly int BackdropId = Shader.PropertyToID("_PockleBackdrop");
        private static readonly int GroundId = Shader.PropertyToID("_PockleGround");
        private static readonly int GroundUvId = Shader.PropertyToID("_PockleGroundUV");
        private static readonly int BackdropOnId = Shader.PropertyToID("_PockleBackdropOn");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        private static readonly int LiftId = Shader.PropertyToID("_Lift");
        private static readonly int SpreadId = Shader.PropertyToID("_Spread");
        private static Material surface, ground;

        /// <summary>The painting behind every browsing page. A calm room the app already carries for onboarding.</summary>
        public const string PagePainting = "UI/Onboarding/OnboardingTutorial2";
        public const float Saturation = .66f, Lift = .34f;

        /// <summary>One shared material for every glass surface, so they batch. Null if the shader is missing.</summary>
        public static Material Surface
        {
            get
            {
                if (surface != null) return surface;
                var shader = Resources.Load<Shader>("UI/PockleGlass");
                if (shader == null || !shader.isSupported) return null;
                return surface = new Material(shader) { name = "Pockle · jelly glass", hideFlags = HideFlags.HideAndDontSave };
            }
        }

        /// <summary>Draws the sharp painting on the page ground with the same toning as its blurred copy.</summary>
        public static Material Ground
        {
            get
            {
                if (ground != null) return ground;
                var shader = Resources.Load<Shader>("UI/PockleGround");
                if (shader == null || !shader.isSupported) return null;
                ground = new Material(shader) { name = "Pockle · page painting", hideFlags = HideFlags.HideAndDontSave };
                ground.SetFloat(SaturationId, Saturation); ground.SetFloat(LiftId, Lift); ground.SetFloat(SpreadId, 0);
                return ground;
            }
        }

        /// <summary>A small, toned, blurred copy of a painting for frosted surfaces. The caller releases it.</summary>
        public static RenderTexture Blur(Texture painting)
        {
            var shader = Resources.Load<Shader>("UI/PockleGround");
            if (painting == null || shader == null || !shader.isSupported) return null;
            int width = 96, height = Mathf.Clamp(Mathf.RoundToInt(width * (float)painting.height / Mathf.Max(1, painting.width)), 32, 256);
            var pass = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            var previous = RenderTexture.active;
            // Halve in steps so every source texel counts, toning once on the way down.
            var a = RenderTexture.GetTemporary(width * 4, height * 4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var b = RenderTexture.GetTemporary(width * 2, height * 2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            a.filterMode = b.filterMode = FilterMode.Bilinear;
            pass.SetFloat(SaturationId, Saturation); pass.SetFloat(LiftId, Lift); pass.SetFloat(SpreadId, 1f);
            Graphics.Blit(painting, a, pass);
            pass.SetFloat(SaturationId, 1f); pass.SetFloat(LiftId, 0f);
            Graphics.Blit(a, b, pass);
            var result = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB)
            { name = "Pockle · blurred page painting", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, useMipMap = false };
            var c = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            c.filterMode = FilterMode.Bilinear;
            Graphics.Blit(b, result, pass);
            foreach (float spread in new[] { 1f, 1.6f, 2.2f, 1.4f })
            {
                pass.SetFloat(SpreadId, spread);
                Graphics.Blit(result, c, pass); Graphics.Blit(c, result, pass);
            }
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(a); RenderTexture.ReleaseTemporary(b); RenderTexture.ReleaseTemporary(c);
            Object.Destroy(pass);
            return result;
        }

        /// <summary>Tells frosted surfaces which blurred painting lies behind the page, where it is on the canvas and how it is cropped.</summary>
        public static void SetBackdrop(Texture blurred, Rect canvasRect, Rect crop, bool on)
        {
            Shader.SetGlobalTexture(BackdropId, blurred != null ? blurred : Texture2D.whiteTexture);
            Shader.SetGlobalVector(GroundId, new Vector4(canvasRect.xMin, canvasRect.yMin,
                1f / Mathf.Max(1f, canvasRect.width), 1f / Mathf.Max(1f, canvasRect.height)));
            Shader.SetGlobalVector(GroundUvId, new Vector4(crop.x, crop.y, crop.width, crop.height));
            Shader.SetGlobalFloat(BackdropOnId, on && blurred != null ? 1f : 0f);
        }
    }
}
