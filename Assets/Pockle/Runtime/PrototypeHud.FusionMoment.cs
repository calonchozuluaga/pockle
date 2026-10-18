using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Builds and starts the fusion moment (see <see cref="FusionMomentPlayer"/>) over the whole screen, then hands over
    /// to the existing reveal. Reduced motion skips straight to the reveal. Never touches inventory: <see cref="Fuse"/>
    /// has already asked the fusion service (which persists first) before this starts.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform momentRoot;
        private FusionMomentPlayer moment;
        private RawImage momentToyA, momentToyB, momentChild, momentChildTop;
        private Text momentBadgeA, momentBadgeB, momentSay, momentEyebrow, momentTitle, momentLine;
        private Image momentOrbBottom;
        private RawImage momentOrbSwirl;
        private Texture2D momentSwirlTexture;
        private readonly Image[] momentBits = new Image[16];

        /// <summary>True while the fusion moment is on screen (tests and Back use it).</summary>
        public bool FusionMomentPlaying => moment != null && moment.Playing;

        private void BuildFusionMoment()
        {
            momentRoot = Rect("Fusion moment", canvas.transform); Stretch(momentRoot);
            var background = momentRoot.gameObject.AddComponent<Image>(); background.color = LabDeep; background.raycastTarget = true;
            var skip = momentRoot.gameObject.AddComponent<Button>(); skip.transition = Selectable.Transition.None;
            skip.onClick.AddListener(() => moment.Skip()); // tap anywhere: straight to the reveal
            var glow = Rect("Moment glow", momentRoot); glow.anchorMin = glow.anchorMax = glow.pivot = new Vector2(.5f, .58f); glow.sizeDelta = new Vector2(900, 900);
            var glowImage = glow.gameObject.AddComponent<RawImage>(); glowImage.texture = UiShapes.Glow; glowImage.color = LabTop; glowImage.raycastTarget = false;
            var dim = Group("Dim", momentRoot); Stretch(dim);
            var dimImage = dim.gameObject.AddComponent<Image>(); dimImage.color = new Color(.04f, .02f, .08f, .5f); dimImage.raycastTarget = false;
            var eyebrow = Label("FUSING", momentRoot, PockleTheme.SmallSize - 1, LabMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            eyebrow.rectTransform.anchorMin = new Vector2(0, 1); eyebrow.rectTransform.anchorMax = new Vector2(1, 1); eyebrow.rectTransform.pivot = new Vector2(.5f, 1);
            eyebrow.rectTransform.anchoredPosition = new Vector2(0, -64); eyebrow.rectTransform.sizeDelta = new Vector2(0, 18);

            // Everything else moves around one point a little above the middle of the screen.
            var stage = Rect("Moment stage", momentRoot);
            stage.anchorMin = stage.anchorMax = new Vector2(.5f, .58f); stage.pivot = new Vector2(.5f, .5f); stage.sizeDelta = Vector2.zero;

            momentToyA = MomentToy(stage, "Parent A"); momentToyB = MomentToy(stage, "Parent B");

            var orbGlow = Group("Orb glow", stage); orbGlow.sizeDelta = new Vector2(300, 300);
            var orbGlowImage = orbGlow.gameObject.AddComponent<RawImage>(); orbGlowImage.texture = UiShapes.Glow; orbGlowImage.color = Color.white; orbGlowImage.raycastTarget = false;
            var orb = Rect("Orb", stage); orb.sizeDelta = new Vector2(150, 150);
            momentOrbBottom = Surface(orb, Color.white, 75);
            orb.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var swirl = Rect("Swirl", orb); swirl.sizeDelta = new Vector2(190, 190);
            momentOrbSwirl = swirl.gameObject.AddComponent<RawImage>(); momentOrbSwirl.raycastTarget = false;
            var shine = Rect("Shine", orb); shine.anchorMin = shine.anchorMax = shine.pivot = new Vector2(.35f, .7f); shine.sizeDelta = new Vector2(90, 70);
            var shineImage = shine.gameObject.AddComponent<RawImage>(); shineImage.texture = UiShapes.Glow; shineImage.color = new Color(1, 1, 1, .8f); shineImage.raycastTarget = false;

            momentBadgeA = MomentPill(stage, "Badge A", out var badgeA); momentBadgeB = MomentPill(stage, "Badge B", out var badgeB);
            var say = Group("Say", stage); say.anchoredPosition = new Vector2(0, -120); say.sizeDelta = new Vector2(320, 26);
            momentSay = Label("", say, PockleTheme.BodySize, LabMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(momentSay.rectTransform);
            var secret = Group("Secret recipe!", stage); secret.anchoredPosition = new Vector2(0, 150); secret.sizeDelta = new Vector2(176, 34);
            Surface(secret, PockleTheme.FieldGold, 17);
            var secretText = Label("Secret recipe!", secret, PockleTheme.BodySize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(secretText.rectTransform);

            var ring = Group("Ring", stage); ring.sizeDelta = new Vector2(110, 110);
            var ringImage = ring.gameObject.AddComponent<RawImage>(); ringImage.texture = UiShapes.Glow; ringImage.color = Color.white; ringImage.raycastTarget = false;
            var bits = new RectTransform[momentBits.Length]; var targets = new Vector2[momentBits.Length];
            for (int i = 0; i < momentBits.Length; i++)
            {
                bits[i] = Group("Confetti", stage); bits[i].sizeDelta = new Vector2(9, 9);
                momentBits[i] = Surface(bits[i], Color.white, 2);
                float a = i * Mathf.PI * 2 / momentBits.Length + (i % 3) * .2f, r = 150 + (i * 37 % 60);
                targets[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }

            var childSpot = Group("Fused toy", stage); childSpot.sizeDelta = new Vector2(200, 200); childSpot.pivot = new Vector2(.5f, .2f);
            var childArt = Rect("Picture", childSpot); Stretch(childArt);
            momentChild = childArt.gameObject.AddComponent<RawImage>(); momentChild.raycastTarget = false;
            var title = Group("Moment title", stage); title.sizeDelta = new Vector2(340, 90);
            var titleHolder = Rect("Title", title); titleHolder.anchorMin = titleHolder.anchorMax = new Vector2(.5f, .5f); titleHolder.anchoredPosition = new Vector2(0, -170); titleHolder.sizeDelta = new Vector2(340, 90);
            momentEyebrow = Label("", titleHolder, PockleTheme.SmallSize, LabMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            LeftLabel(momentEyebrow.rectTransform, Vector2.zero, new Vector2(340, 18));
            momentTitle = Label("", titleHolder, 30, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            LeftLabel(momentTitle.rectTransform, new Vector2(0, -20), new Vector2(340, 40));
            momentLine = Label("", titleHolder, PockleTheme.CaptionSize, LabMuted, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            LeftLabel(momentLine.rectTransform, new Vector2(0, -62), new Vector2(340, 22));

            var flash = Rect("Flash", momentRoot); Stretch(flash);
            var flashImage = flash.gameObject.AddComponent<Image>(); flashImage.color = new Color(1, 1, 1, 0); flashImage.raycastTarget = false;

            moment = momentRoot.gameObject.AddComponent<FusionMomentPlayer>();
            moment.ToyA = (RectTransform)momentToyA.transform.parent; moment.ToyB = (RectTransform)momentToyB.transform.parent;
            moment.Orb = orb; moment.OrbSwirl = swirl; moment.OrbGlow = orbGlow; moment.BadgeA = badgeA; moment.BadgeB = badgeB;
            moment.Say = say; moment.Secret = secret; moment.Ring = ring; moment.Child = childSpot; moment.Title = title; moment.Dim = dim;
            moment.Flash = flashImage; moment.Bits = bits; moment.BitTargets = targets;
            moment.Finished = () => { momentRoot.gameObject.SetActive(false); Navigate(AppPage.FusionReveal); }; // ApplyPage refreshes the reveal
            momentRoot.gameObject.SetActive(false);
        }

        /// <summary>A RectTransform with a CanvasGroup, so the player can fade it.</summary>
        private static RectTransform Group(string name, Transform parent)
        {
            var rect = Rect(name, parent);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.gameObject.AddComponent<CanvasGroup>();
            return rect;
        }

        private RawImage MomentToy(RectTransform stage, string name)
        {
            var holder = Group(name, stage); holder.sizeDelta = new Vector2(110, 110); holder.pivot = new Vector2(.5f, .1f);
            var art = Rect("Picture", holder); Stretch(art);
            var raw = art.gameObject.AddComponent<RawImage>(); raw.raycastTarget = false;
            return raw;
        }

        private Text MomentPill(RectTransform stage, string name, out RectTransform pill)
        {
            pill = Group(name, stage); pill.sizeDelta = new Vector2(96, 30);
            Surface(pill, Color.white, 15);
            var text = Label("", pill, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(text.rectTransform); text.horizontalOverflow = HorizontalWrapMode.Overflow; text.resizeTextForBestFit = false;
            return text;
        }

        /// <summary>Fills the moment for the current pair and plays it (or goes straight to the reveal with calm motion).</summary>
        private void StartFusionMoment()
        {
            if (reducedMotionEnabled || canvas == null) { Navigate(AppPage.FusionReveal); return; }
            if (momentRoot == null) BuildFusionMoment();
            var outcome = PersonalityBook.DescribeFusion(fuseFirst, fuseSecond);
            Color a = ToyColour(fuseFirst), b = ToyColour(fuseSecond);
            momentToyA.texture = CutoutPortrait(fuseFirst); momentToyB.texture = CutoutPortrait(fuseSecond);
            momentOrbBottom.color = Color.Lerp(a, b, .5f);
            bool secret = outcome != null && outcome.Secret;
            if (momentSwirlTexture != null) Destroy(momentSwirlTexture);
            momentSwirlTexture = Swirl(a, b, secret);
            momentOrbSwirl.texture = momentSwirlTexture;
            string ta = PersonalityBook.TemperamentName(PersonalityBook.TemperamentOf(fuseFirst)), tb = PersonalityBook.TemperamentName(PersonalityBook.TemperamentOf(fuseSecond));
            momentBadgeA.text = ta; momentBadgeB.text = tb;
            ((RectTransform)momentBadgeA.transform.parent).sizeDelta = new Vector2(momentBadgeA.preferredWidth + 26, 30);
            ((RectTransform)momentBadgeB.transform.parent).sizeDelta = new Vector2(momentBadgeB.preferredWidth + 26, 30);
            momentSay.text = ta + " + " + tb + "…";
            for (int i = 0; i < momentBits.Length; i++) momentBits[i].color = i % 3 == 0 ? Color.white : i % 3 == 1 ? a : secret ? PockleTheme.FieldGold : b;
            // The fused toy: the stand-in blended picture the reveal also uses until Codex's fused materials exist.
            momentChild.texture = FusedPicture(fuseFirst, fuseSecond);
            momentEyebrow.text = outcome != null ? outcome.Name.ToUpperInvariant() : "";
            string line = outcome != null ? outcome.PersonalityLine ?? "" : "";
            int bang = line.IndexOf("! ", System.StringComparison.Ordinal);
            momentTitle.text = bang > 0 ? line.Substring(0, bang + 1) : line;
            momentLine.text = bang > 0 ? line.Substring(bang + 2) : "";
            momentRoot.SetAsLastSibling();
            moment.Play(secret);
        }

        /// <summary>
        /// The stand-in picture for the fused toy until Codex's fused materials exist: the second parent's cut-out fading
        /// into the first's from the top, on a clear background (owner, October 17: no square behind the revealed toy).
        /// </summary>
        private Texture FusedPicture(string first, string second)
        {
            if (revealTexture != null) { Destroy(revealTexture); revealTexture = null; }
            revealTexture = BlendCutouts(CutoutPortrait(second) as Texture2D, CutoutPortrait(first) as Texture2D);
            return revealTexture != null ? revealTexture : CutoutPortrait(first);
        }

        /// <summary>Blends two same-size cut-outs top to bottom, keeping their transparency. Null when they can't be read.</summary>
        private static Texture2D BlendCutouts(Texture2D top, Texture2D bottom)
        {
            if (top == null || bottom == null || !top.isReadable || !bottom.isReadable || top.width != bottom.width || top.height != bottom.height) return null;
            int width = top.width, height = top.height;
            Color32[] upper = top.GetPixels32(), lower = bottom.GetPixels32(), blend = new Color32[upper.Length];
            for (int y = 0; y < height; y++)
            {
                float k = Mathf.Clamp01((y / (float)height - .38f) / .3f); // texture rows run bottom to top
                k = k * k * (3 - 2 * k);
                for (int x = 0; x < width; x++) { int i = y * width + x; blend[i] = Color32.Lerp(lower[i], upper[i], k); }
            }
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "Pockle fused cut-out", wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(blend); texture.Apply(false, true);
            return texture;
        }

        /// <summary>A soft two-colour swirl for the orb (gold-flecked for a secret recipe). Made once per fusion.</summary>
        private static Texture2D Swirl(Color a, Color b, bool secret)
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Pockle fusion swirl", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            Color gold = PockleTheme.FieldGold;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + .5f) / size - .5f, dy = (y + .5f) / size - .5f;
                    float angle = Mathf.Atan2(dy, dx), r = Mathf.Sqrt(dx * dx + dy * dy) * 2;
                    float k = .5f + .5f * Mathf.Sin(angle * 2 + r * 5); // two arms that curl outwards
                    Color c = Color.Lerp(a, b, k * k * (3 - 2 * k));
                    if (secret) c = Color.Lerp(c, gold, .35f * Mathf.Clamp01(Mathf.Sin(angle * 4 - r * 6)));
                    c = Color.Lerp(c, Color.white, .18f * (1 - r));
                    pixels[y * size + x] = c;
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return texture;
        }
    }
}
