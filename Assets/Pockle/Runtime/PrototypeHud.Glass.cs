using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Jelly glass (UX-053, owner's pick "A2 · Jelly glass", November 18): browsing pages sit on a calm painting, and
    /// frosted surfaces show a blurred copy of it. See PockleGlass, GlassSurface and Resources/UI/PockleGlass.shader.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RawImage groundPainting;
        private RenderTexture groundBlur;
        private bool glassBackdropOn;

        private void BuildGlassGround()
        {
            // Glass surfaces carry their size, radius and light in two extra texture coordinates.
            canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            var painting = Resources.Load<Texture2D>(PockleGlass.PagePainting);
            var material = PockleGlass.Ground;
            if (painting == null || material == null || PockleGlass.Surface == null) return; // the flat peach ground remains
            var rect = Rect("Page painting", pageGround); Stretch(rect);
            groundPainting = rect.gameObject.AddComponent<RawImage>();
            groundPainting.texture = painting; groundPainting.material = material; groundPainting.raycastTarget = false;
            groundBlur = PockleGlass.Blur(painting);
        }

        /// <summary>True on pages that stand on the painting rather than a colour of their own.</summary>
        private bool PaintedGround => groundPainting != null && page != AppPage.Play && page != AppPage.Personality && page != AppPage.FusionReveal;

        /// <summary>Keeps the painting's crop and the frosted surfaces' view of it in step with the screen and the page.</summary>
        private void RefreshGlass()
        {
            if (canvas == null) return;
            bool painted = PaintedGround;
            if (groundPainting != null && groundPainting.gameObject.activeSelf != painted) groundPainting.gameObject.SetActive(painted);
            var area = ((RectTransform)canvas.transform).rect;
            var crop = new Rect(0, 0, 1, 1);
            if (painted)
            {
                crop = CollectionLook.Cover(groundPainting.texture, area.width / Mathf.Max(1f, area.height), .5f, .5f);
                if (groundPainting.uvRect != crop) groundPainting.uvRect = crop;
            }
            // Onboarding and a box opening cover the page with scenes of their own; frost over them turns solid instead.
            glassBackdropOn = painted && !OnboardingVisible && !BoxRevealVisible;
            PockleGlass.SetBackdrop(groundBlur, area, crop, glassBackdropOn);
        }

        /// <summary>
        /// Puts a small jelly picture above a control's word (Resources/UI/Icons/Jelly). The word stays: the owner's
        /// October 19 and November 8 decisions keep words with icons. Returns null, changing nothing, if the picture is missing.
        /// </summary>
        private RectTransform JellyIcon(Button button, string name, float size = 32)
        {
            var texture = Resources.Load<Texture2D>("UI/Icons/Jelly/" + name);
            if (texture == null) return null;
            var rect = Rect(name + " jelly icon", button.transform);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -6); rect.sizeDelta = new Vector2(size, size);
            var image = rect.gameObject.AddComponent<RawImage>(); image.texture = texture; image.raycastTarget = false;
            var label = button.GetComponentInChildren<Text>(true);
            label.alignment = TextAnchor.LowerCenter; label.fontSize = 12; label.resizeTextMaxSize = 12; label.resizeTextMinSize = 10;
            label.rectTransform.offsetMin = new Vector2(2, 6); label.rectTransform.offsetMax = new Vector2(-2, -(size + 6));
            return rect;
        }

        private void ReleaseGlass()
        {
            PockleGlass.SetBackdrop(null, new Rect(0, 0, 1, 1), new Rect(0, 0, 1, 1), false);
            if (groundBlur != null) { groundBlur.Release(); Destroy(groundBlur); groundBlur = null; }
        }
    }
}
