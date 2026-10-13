using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Fusion (owner-approved canvases "Fuse spares" and "Fusion reveal", October 12; docs/FUSION.md). Pick two spares and
    /// see what they'd make. The HUD never changes inventory: it calls <see cref="IFusionService"/>. Until Codex's runtime
    /// exists the service is preview-only, so the reveal says so and nothing is used.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private static readonly string[] FusionOrder = { PersonalityBook.Peach, PersonalityBook.Moon, PersonalityBook.Gold, PersonalityBook.Mint };
        private IFusionService fusion = new PreviewOnlyFusion();
        /// <summary>Runtime hook for Codex's fusion service; the HUD keeps a preview-only default.</summary>
        public void SetFusionService(IFusionService service) { fusion = service ?? new PreviewOnlyFusion(); RefreshFusion(); }

        private string fuseFirst, fuseSecond;
        private bool lastFusionWasPreview = true;
        private RectTransform fusionCard, fusionPlus, fusionHintRow, fusionSecret, fusionSparesHeading, fusionSparesNote, fusionEmpty;
        private Button fusionSlotA, fusionSlotB, fusionGo;
        private RawImage fusionSlotPictureA, fusionSlotPictureB;
        private Text fusionSlotEmptyA, fusionSlotEmptyB, fusionHint;
        private Image fusionHintBubble;
        private PersonalityMotion fusionHintMotion;
        private readonly Button[] spareTiles = new Button[4];
        private readonly Image[] spareRings = new Image[4], spareFaces = new Image[4];
        private readonly Text[] spareLabels = new Text[4];

        // Reveal.
        private RectTransform revealChip, revealDisc, revealToy, revealSheet, revealParents, revealTraits;
        private readonly List<RectTransform> revealSparkles = new List<RectTransform>();
        private RawImage revealPicture, revealParentA, revealParentB;
        private Image revealParentFaceA, revealParentFaceB;
        private Text revealName, revealParentsLine, revealFootnote, revealPreviewNote;
        private readonly Text[] revealTraitValues = new Text[4];
        private Button revealAdd, revealAgain;
        private PersonalityMotion revealMotion;
        private Texture2D revealTexture;

        private void BuildFusion()
        {
            fusionCard = Rect("Fusion", fusionRoot); Surface(fusionCard, PockleTheme.Confetti, 28);
            fusionSlotA = FusionSlot("Spare 1", out fusionSlotPictureA, out fusionSlotEmptyA, () => { fuseFirst = null; RefreshFusion(); });
            fusionSlotB = FusionSlot("Spare 2", out fusionSlotPictureB, out fusionSlotEmptyB, () => { fuseSecond = null; RefreshFusion(); });
            fusionPlus = Rect("Plus", fusionCard); Surface(fusionPlus, Color.white, 16);
            var plus = Label("+", fusionPlus, 22, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(plus.rectTransform);
            fusionHintRow = Rect("What it could make", fusionCard); Surface(fusionHintRow, new Color(1, 1, 1, .7f), 18);
            var bubbleHolder = Rect("Hint mark", fusionHintRow);
            LeftLabel(bubbleHolder, new Vector2(12, -8), new Vector2(36, 36));
            var bubble = Rect("Bubble", bubbleHolder); Stretch(bubble); bubble.pivot = new Vector2(.5f, .5f);
            fusionHintBubble = Surface(bubble, PockleTheme.FillDeep, 18);
            var question = Label("?", bubble, 18, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(question.rectTransform);
            fusionHintMotion = bubble.gameObject.AddComponent<PersonalityMotion>();
            fusionHintMotion.Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Calm);
            fusionHint = Label("", fusionHintRow, PockleTheme.CaptionSize, Ink, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);

            fusionSecret = Rect("Secret recipe", fusionRoot); Surface(fusionSecret, PockleTheme.Plum, 20);
            var star = Rect("Star", fusionSecret);
            var starImage = star.gameObject.AddComponent<RawImage>(); starImage.texture = UiShapes.Sparkle; starImage.color = PockleTheme.FieldGold; starImage.raycastTarget = false;
            var secret = Label("This pair hides a secret recipe", fusionSecret, PockleTheme.CaptionSize, PockleTheme.FieldGold, FontStyle.Bold,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(secret.rectTransform); secret.rectTransform.offsetMin = new Vector2(24, 0);
            star.anchorMin = star.anchorMax = star.pivot = new Vector2(.5f, .5f); star.sizeDelta = new Vector2(16, 16);

            fusionSparesHeading = Label("Your spares", fusionRoot, 19, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 28)).rectTransform;
            fusionSparesNote = Label("Your last copy is never used", fusionRoot, PockleTheme.SmallSize, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleRight, Vector2.zero, new Vector2(200, 28)).rectTransform;
            for (int i = 0; i < spareTiles.Length; i++)
            {
                string id = FusionOrder[i];
                var tile = Rect("Spare", fusionRoot);
                spareRings[i] = Surface(tile, Ink, 20); spareRings[i].raycastTarget = true;
                spareTiles[i] = tile.gameObject.AddComponent<Button>(); spareTiles[i].targetGraphic = spareRings[i];
                spareTiles[i].onClick.AddListener(() => PickSpare(id));
                tile.gameObject.AddComponent<SquishFeedback>();
                tile.name = "Spare · " + id;
                var face = Rect("Face", tile); Stretch(face); spareFaces[i] = Surface(face, PockleTheme.FieldFor(id), 18);
                PipVariants.TryFromCollectibleId(id, out var variant);
                var art = Portrait(face, (int)variant); art.rectTransform.anchorMin = art.rectTransform.anchorMax = new Vector2(.5f, 1);
                art.rectTransform.pivot = new Vector2(.5f, 1); art.rectTransform.anchoredPosition = new Vector2(0, -4); art.rectTransform.sizeDelta = new Vector2(62, 62);
                spareLabels[i] = Label("", face, 12, Ink, FontStyle.Bold, TextAnchor.LowerCenter, Vector2.zero, Vector2.zero);
                Stretch(spareLabels[i].rectTransform); spareLabels[i].rectTransform.offsetMin = new Vector2(2, 8);
            }
            fusionEmpty = Label("No spares yet. Spares come from walking boxes and the daily check-in.", fusionRoot, PockleTheme.BodySize, MutedInk,
                FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, new Vector2(300, 60)).rectTransform;
            fusionGo = CreateButton("Pick two spares", fusionRoot, Vector2.zero, new Vector2(300, 56), PockleTheme.Plum, Fuse, out _);
            fusionGo.name = "Fuse";
            BuildFusionReveal();
        }

        private Button FusionSlot(string title, out RawImage picture, out Text empty, System.Action clear)
        {
            var slot = CreateButton(title, fusionCard, Vector2.zero, new Vector2(96, 96), Color.white, clear, out _, 13);
            slot.name = "Fusion slot · " + title;
            empty = slot.GetComponentInChildren<Text>(); empty.color = MutedInk;
            slot.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var art = Rect("Spare", slot.transform); Stretch(art); art.offsetMin = new Vector2(10, -6); art.offsetMax = new Vector2(-10, -4);
            picture = art.gameObject.AddComponent<RawImage>(); picture.raycastTarget = false;
            return slot;
        }

        private int SparesLeft(string id)
        {
            int spares = session != null ? Mathf.Max(0, session.GetOwnedCount(id) - 1) : 0;
            if (fuseFirst == id) spares--;
            if (fuseSecond == id) spares--;
            return spares;
        }

        private void PickSpare(string id)
        {
            if (SparesLeft(id) <= 0) return;
            if (fuseFirst == null) fuseFirst = id; else fuseSecond = id;
            RefreshFusion();
        }

        private void Fuse()
        {
            if (fuseFirst == null || fuseSecond == null) return;
            // A real service persists the fused toy before the reveal; the preview uses nothing.
            lastFusionWasPreview = !(fusion.Available && fusion.TryFuse(fuseFirst, fuseSecond));
            Navigate(AppPage.FusionReveal); // ApplyPage refreshes the reveal.
        }

        private void RefreshFusion()
        {
            if (fusionGo == null) return;
            // Drop picks that are no longer spares (for example after a trade elsewhere).
            if (fuseSecond != null && SparesLeft(fuseSecond) < 0) fuseSecond = null;
            if (fuseFirst != null && SparesLeft(fuseFirst) < 0) fuseFirst = null;
            ShowSlot(fusionSlotA, fusionSlotPictureA, fusionSlotEmptyA, fuseFirst);
            ShowSlot(fusionSlotB, fusionSlotPictureB, fusionSlotEmptyB, fuseSecond);
            bool ready = fuseFirst != null && fuseSecond != null;
            var outcome = ready ? PersonalityBook.DescribeFusion(fuseFirst, fuseSecond) : null;
            fusionHint.text = outcome != null ? outcome.Hint : "Pick two spares to see what they could make.";
            fusionHintBubble.color = ready ? PockleTheme.Plum : PockleTheme.FillDeep;
            fusionHintMotion.SetStyle(ready ? PersonalityMotionStyle.Dream : PersonalityMotionStyle.Calm);
            fusionSecret.gameObject.SetActive(outcome != null && outcome.Secret);
            int shown = 0;
            for (int i = 0; i < spareTiles.Length; i++)
            {
                string id = FusionOrder[i];
                int owned = session != null ? session.GetOwnedCount(id) : 0;
                bool has = owned > 1 && session.TryGetPlayableVariant(id, out _);
                spareTiles[i].gameObject.SetActive(has);
                if (!has) continue;
                shown++;
                int left = SparesLeft(id);
                bool picked = fuseFirst == id || fuseSecond == id;
                spareLabels[i].text = left + (left == 1 ? " spare" : " spares");
                spareRings[i].color = picked ? Ink : PockleTheme.FieldFor(id);
                var face = spareFaces[i].rectTransform; face.offsetMin = picked ? new Vector2(3, 3) : Vector2.zero; face.offsetMax = picked ? new Vector2(-3, -3) : Vector2.zero;
                spareTiles[i].interactable = left > 0;
                var colours = spareTiles[i].colors; colours.disabledColor = new Color(1, 1, 1, .45f); spareTiles[i].colors = colours;
            }
            fusionEmpty.gameObject.SetActive(shown == 0);
            fusionGo.interactable = ready;
            fusionGo.GetComponentInChildren<Text>().text = ready ? "Fuse, using both spares" : "Pick two spares";
            fusionGo.GetComponent<Image>().color = ready ? PockleTheme.Plum : PockleTheme.FillDeep;
            fusionGo.GetComponentInChildren<Text>().color = ready ? PockleTheme.OnPlum : MutedInk;
        }

        private void ShowSlot(Button slot, RawImage picture, Text empty, string id)
        {
            bool has = id != null && PipVariants.TryFromCollectibleId(id, out _);
            PipVariants.TryFromCollectibleId(id ?? "", out var variant);
            picture.texture = has ? previews[(int)variant] : null;
            picture.gameObject.SetActive(has);
            empty.enabled = !has;
            slot.GetComponent<Image>().color = has ? PockleTheme.FieldFor(id) : Color.white;
            slot.interactable = has;
        }

        private float LayoutFusion(float width)
        {
            float y = 4;
            TopCentered(fusionCard, new Vector2(0, -y), new Vector2(width, 200));
            LeftLabel(fusionSlotA.GetComponent<RectTransform>(), new Vector2(18, -24), new Vector2(96, 96));
            LeftLabel(fusionSlotB.GetComponent<RectTransform>(), new Vector2(width - 18 - 96, -24), new Vector2(96, 96));
            LeftLabel(fusionPlus, new Vector2(width / 2 - 16, -56), new Vector2(32, 32));
            LeftLabel(fusionHintRow, new Vector2(18, -132), new Vector2(width - 36, 52));
            LeftLabel(fusionHint.rectTransform, new Vector2(60, 0), new Vector2(width - 36 - 72, 52));
            y += 200;
            if (fusionSecret.gameObject.activeSelf) { y += 10; TopCentered(fusionSecret, new Vector2(0, -y), new Vector2(width, 40)); y += 40; }
            var star = fusionSecret.GetChild(0) as RectTransform;
            star.anchoredPosition = new Vector2(-fusionSecret.GetComponentInChildren<Text>().preferredWidth / 2 - 4, 0);
            y += 18;
            LeftLabel(fusionSparesHeading, new Vector2(0, -y), new Vector2(width / 2, 28));
            LeftLabel(fusionSparesNote, new Vector2(width / 2, -y), new Vector2(width / 2, 28));
            y += 38;
            const float gap = 8;
            float tile = (width - gap * 3) / 4;
            int slot = 0;
            for (int i = 0; i < spareTiles.Length; i++)
            {
                if (!spareTiles[i].gameObject.activeSelf) continue;
                LeftLabel(spareTiles[i].GetComponent<RectTransform>(), new Vector2(slot * (tile + gap), -y), new Vector2(tile, 96));
                slot++;
            }
            if (slot == 0) { TopCentered(fusionEmpty, new Vector2(0, -y - 8), new Vector2(width, 60)); y += 68; }
            else y += 96;
            y += 28;
            LeftLabel(fusionGo.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 56));
            y += 56;
            TopCentered(fusionRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        // ---------- Reveal ----------

        private void BuildFusionReveal()
        {
            revealChip = Rect("One of a kind", fusionRevealRoot); Surface(revealChip, PockleTheme.FieldGold, 15);
            var chipStar = Rect("Star", revealChip);
            var chipStarImage = chipStar.gameObject.AddComponent<RawImage>(); chipStarImage.texture = UiShapes.Sparkle; chipStarImage.color = Ink; chipStarImage.raycastTarget = false;
            LeftLabel(chipStar, new Vector2(14, -8), new Vector2(14, 14));
            var chipText = Label("One of a kind", revealChip, PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            Stretch(chipText.rectTransform); chipText.rectTransform.offsetMin = new Vector2(34, 0);
            revealDisc = Rect("Glow", fusionRevealRoot); Surface(revealDisc, new Color32(0x3C, 0x37, 0x72, 0xFF), 150);
            revealSparkles.Add(Sparkle(fusionRevealRoot, PockleTheme.FieldGold, 0f));
            revealSparkles.Add(Sparkle(fusionRevealRoot, PockleTheme.Jelly, .5f));
            revealSparkles.Add(Sparkle(fusionRevealRoot, PockleTheme.FieldMoon, 1f));
            revealToy = Rect("Fused toy", fusionRevealRoot);
            var motion = Rect("Motion", revealToy); Stretch(motion); motion.pivot = new Vector2(.5f, SceneBase);
            revealMotion = motion.gameObject.AddComponent<PersonalityMotion>();
            revealMotion.Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Calm);
            var frame = Rect("Frame", motion); Stretch(frame); Surface(frame, Color.white, 44);
            frame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var picture = Rect("Blend", frame); Stretch(picture);
            revealPicture = picture.gameObject.AddComponent<RawImage>(); revealPicture.raycastTarget = false;

            revealSheet = Rect("Fusion sheet", fusionRevealRoot); Surface(revealSheet, Paper, 32);
            revealName = Label("", revealSheet, PockleTheme.TitleSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 38));
            revealParents = Rect("Parents", revealSheet);
            revealParentA = ParentBubble(revealParents, 0, out revealParentFaceA);
            var plus = Label("+", revealParents, PockleTheme.CaptionSize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            LeftLabel(plus.rectTransform, new Vector2(30, 0), new Vector2(18, 30));
            revealParentB = ParentBubble(revealParents, 48, out revealParentFaceB);
            revealParentsLine = Label("", revealParents, PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            revealTraits = Rect("Traits", revealSheet);
            string[] names = { "Body", "Colour", "Material", "Personality" };
            for (int i = 0; i < revealTraitValues.Length; i++)
            {
                var tile = Rect(names[i], revealTraits); Surface(tile, Quiet, 16);
                var name = Label(names[i], tile, 12, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
                LeftLabel(name.rectTransform, new Vector2(12, -10), new Vector2(140, 16));
                revealTraitValues[i] = Label("", tile, PockleTheme.BodySize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            }
            revealFootnote = Label("Lives in Fusions on your shelf. Yours to keep: fused toys can't be traded or sold.", revealSheet, PockleTheme.SmallSize,
                MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(300, 36));
            revealPreviewNote = Label("Preview: fusion isn't switched on yet, so your spares weren't used.", revealSheet, PockleTheme.SmallSize,
                PockleTheme.JellyInk, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, new Vector2(300, 36));
            revealAdd = CreateButton("Add to shelf", revealSheet, Vector2.zero, new Vector2(200, 54), PockleTheme.Plum, () => SelectTab(AppPage.Shelf), out _);
            revealAgain = CreateButton("Fuse again", revealSheet, Vector2.zero, new Vector2(130, 54), Quiet, () => GoBack(), out _, PockleTheme.BodySize);
        }

        private RawImage ParentBubble(RectTransform parent, float x, out Image face)
        {
            var bubble = Rect("Parent", parent); face = Surface(bubble, Quiet, 15);
            bubble.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            LeftLabel(bubble, new Vector2(x, 0), new Vector2(30, 30));
            var art = Rect("Toy", bubble); Stretch(art);
            var raw = art.gameObject.AddComponent<RawImage>(); raw.raycastTarget = false;
            return raw;
        }

        private void RefreshFusionReveal()
        {
            if (revealName == null || fuseFirst == null || fuseSecond == null) return;
            var outcome = PersonalityBook.DescribeFusion(fuseFirst, fuseSecond);
            if (outcome == null) return;
            PipVariants.TryFromCollectibleId(fuseFirst, out var a);
            PipVariants.TryFromCollectibleId(fuseSecond, out var b);
            revealName.text = outcome.Name;
            revealParentA.texture = previews[(int)a]; revealParentFaceA.color = PockleTheme.FieldFor(fuseFirst);
            revealParentB.texture = previews[(int)b]; revealParentFaceB.color = PockleTheme.FieldFor(fuseSecond);
            revealParentsLine.text = "Fused from " + ToyName(a) + " and " + ToyName(b);
            revealTraitValues[0].text = outcome.Body; revealTraitValues[1].text = outcome.Colour;
            revealTraitValues[2].text = outcome.Material; revealTraitValues[3].text = outcome.Personality;
            revealPreviewNote.gameObject.SetActive(lastFusionWasPreview);
            revealAdd.interactable = !lastFusionWasPreview;
            if (PersonalityBook.TryGet(fuseFirst, out var personality)) revealMotion.SetStyle(personality.Motion == PersonalityMotionStyle.Show
                ? PersonalityMotionStyle.Show : PersonalityMotionStyle.Calm);
            if (revealTexture != null) Destroy(revealTexture);
            revealTexture = BlendPortraits(previews[(int)b], previews[(int)a]);
            revealPicture.texture = revealTexture;
        }

        /// <summary>
        /// Stand-in fused art until Codex's blended materials exist: the second spare's portrait on top, fading into the first
        /// below (the canvas's "moon to peach"). Read back once per reveal; never per frame.
        /// </summary>
        private static Texture2D BlendPortraits(RenderTexture top, RenderTexture bottom)
        {
            int size = Mathf.Min(top.width, bottom.width);
            var upper = ReadBack(top, size); var lower = ReadBack(bottom, size);
            var blend = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                // Rows run bottom-up. Fully top colour above 62%, fully bottom colour below 28%.
                float v = y / (float)(size - 1);
                float weight = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.28f, .62f, v));
                for (int x = 0; x < size; x++)
                    blend[y * size + x] = Color32.Lerp(lower[y * size + x], upper[y * size + x], weight);
            }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Fused toy preview", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            texture.SetPixels32(blend); texture.Apply(false, false);
            return texture;
        }

        private static Color32[] ReadBack(RenderTexture source, int size)
        {
            var previous = RenderTexture.active;
            var copy = new Texture2D(size, size, TextureFormat.RGBA32, false);
            try { RenderTexture.active = source; copy.ReadPixels(new Rect(0, 0, size, size), 0, 0); copy.Apply(false, false); return copy.GetPixels32(); }
            finally { RenderTexture.active = previous; Destroy(copy); }
        }

        private float LayoutFusionReveal(float width)
        {
            const float gutter = 22f;
            float inner = Mathf.Min(width - gutter * 2, 440f), left = (width - inner) / 2;
            float chipWidth = 148;
            LeftLabel(revealChip, new Vector2(width / 2 - chipWidth / 2, -40), new Vector2(chipWidth, 30));
            LeftLabel(revealDisc, new Vector2(width / 2 - 150, -100), new Vector2(300, 300));
            LeftLabel(revealSparkles[0], new Vector2(width / 2 - 139, -132), new Vector2(24, 24));
            LeftLabel(revealSparkles[1], new Vector2(width / 2 + 107, -170), new Vector2(16, 16));
            LeftLabel(revealSparkles[2], new Vector2(width / 2 + 87, -330), new Vector2(12, 12));
            LeftLabel(revealToy, new Vector2(width / 2 - 110, -150), new Vector2(220, 220));
            const float sheetTop = 420f;
            float y = 24;
            LeftLabel(revealName.rectTransform, new Vector2(left, -y), new Vector2(inner, 38)); y += 46;
            LeftLabel(revealParents, new Vector2(left, -y), new Vector2(inner, 30));
            LeftLabel(revealParentsLine.rectTransform, new Vector2(88, 0), new Vector2(inner - 88, 30));
            y += 30 + 16;
            LeftLabel(revealTraits, new Vector2(left, -y), new Vector2(inner, 130));
            float half = (inner - 8) / 2;
            for (int i = 0; i < revealTraitValues.Length; i++)
            {
                var tile = (RectTransform)revealTraits.GetChild(i);
                LeftLabel(tile, new Vector2(i % 2 * (half + 8), -(i / 2) * 66), new Vector2(half, 58));
                LeftLabel(revealTraitValues[i].rectTransform, new Vector2(12, -26), new Vector2(half - 20, 24));
            }
            y += 132 + 10;
            LeftLabel(revealFootnote.rectTransform, new Vector2(left, -y), new Vector2(inner, 36)); y += 36;
            if (revealPreviewNote.gameObject.activeSelf) { LeftLabel(revealPreviewNote.rectTransform, new Vector2(left, -y), new Vector2(inner, 36)); y += 36; }
            y += 8;
            float again = 130;
            LeftLabel(revealAdd.GetComponent<RectTransform>(), new Vector2(left, -y), new Vector2(inner - again - 10, 54));
            LeftLabel(revealAgain.GetComponent<RectTransform>(), new Vector2(left + inner - again, -y), new Vector2(again, 54));
            y += 54 + 28;
            LeftLabel(revealSheet, new Vector2(0, -sheetTop), new Vector2(width, y + 400));
            float height = sheetTop + y;
            TopCentered(fusionRevealRoot, Vector2.zero, new Vector2(width, height));
            return height;
        }

        private void ReleaseFusion()
        {
            if (revealTexture != null) { Destroy(revealTexture); revealTexture = null; }
            foreach (var clip in voiceClips.Values) if (clip != null) Destroy(clip);
            voiceClips.Clear();
        }
    }
}
