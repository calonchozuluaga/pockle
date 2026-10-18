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

        // The fusion card (owner-approved canvas "Fuse spares, redesigned", October 16): a dark lab with both picks standing
        // on soft pedestals, a blurred "?" toy in the parents' colours, and three hint chips. Who it is stays a surprise.
        private RectTransform fusionCard, fusionMystery, fusionSecret, fusionSparesHeading, fusionSparesNote, fusionEmpty, fusionNoSpareRow;
        private Button fusionSlotA, fusionSlotB, fusionGo;
        private RawImage fusionSlotPictureA, fusionSlotPictureB, fusionMysteryTop;
        private Image fusionMysteryBottom;
        private RectTransform fusionSlotEmptyA, fusionSlotEmptyB;
        private Text fusionSlotNameA, fusionSlotNameB, fusionSlotTempA, fusionSlotTempB, fusionMysteryLine, fusionGoNote, fusionNoSpareText;
        private RectTransform fusionSlotTempPillA, fusionSlotTempPillB;
        private readonly Text[] fusionChipValues = new Text[3];
        private Image fusionColourSwatchBottom;
        private RawImage fusionColourSwatchTop;
        private PersonalityMotion fusionMysteryMotion;
        private readonly Button[] spareTiles = new Button[4];
        private readonly Image[] spareRings = new Image[4], spareFaces = new Image[4];
        private readonly RawImage[] sparePictures = new RawImage[4];
        private readonly Text[] spareNames = new Text[4], spareLabels = new Text[4], spareBadges = new Text[4];
        private readonly RectTransform[] spareBadgeRects = new RectTransform[4];
        private static readonly Color LabTop = new Color32(0x54, 0x47, 0x9A, 0xFF), LabDeep = new Color32(0x2A, 0x14, 0x30, 0xFF);
        private static readonly Color LabGlass = new Color(1, 1, 1, .12f), LabMuted = new Color32(0xD9, 0xCB, 0xE0, 0xFF);
        private const float FusionCardHeight = 300, SlotWidth = 128, SpareTileHeight = 136;

        // Reveal.
        private RectTransform revealChip, revealDisc, revealToy, revealSheet, revealParents, revealTraits;
        private readonly List<RectTransform> revealSparkles = new List<RectTransform>();
        private RawImage revealPicture, revealParentA, revealParentB;
        private Image revealParentFaceA, revealParentFaceB;
        private Text revealName, revealPersonalityLine, revealParentsLine, revealFootnote, revealPreviewNote;
        private readonly Text[] revealTraitValues = new Text[4];
        private Button revealAdd, revealAgain;
        private PersonalityMotion revealMotion;
        private Texture2D revealTexture;

        private void BuildFusion()
        {
            fusionCard = Rect("Fusion", fusionRoot); Surface(fusionCard, LabDeep, 30);
            fusionCard.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var glow = Rect("Lab glow", fusionCard);
            var glowImage = glow.gameObject.AddComponent<RawImage>(); glowImage.texture = UiShapes.Glow; glowImage.color = LabTop; glowImage.raycastTarget = false;
            glow.anchorMin = glow.anchorMax = glow.pivot = new Vector2(.5f, .5f); glow.anchoredPosition = new Vector2(0, 40); glow.sizeDelta = new Vector2(620, 520);
            foreach (var (x, y, size, colour) in new[] { (26f, -22f, 10f, PockleTheme.FieldGold), (-34f, -30f, 12f, Color.white), (-18f, -150f, 9f, PockleTheme.FieldGold), (20f, -170f, 8f, Color.white) })
            {
                var star = Rect("Lab sparkle", fusionCard);
                var image = star.gameObject.AddComponent<RawImage>(); image.texture = UiShapes.Sparkle; image.color = colour; image.raycastTarget = false;
                star.anchorMin = star.anchorMax = star.pivot = new Vector2(x > 0 ? 0 : 1, 1); star.anchoredPosition = new Vector2(x, y); star.sizeDelta = new Vector2(size, size);
            }
            var eyebrow = Label("YOUR FUSION", fusionCard, PockleTheme.SmallSize - 1, LabMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            eyebrow.rectTransform.anchorMin = new Vector2(0, 1); eyebrow.rectTransform.anchorMax = new Vector2(1, 1); eyebrow.rectTransform.pivot = new Vector2(.5f, 1);
            eyebrow.rectTransform.anchoredPosition = new Vector2(0, -14); eyebrow.rectTransform.sizeDelta = new Vector2(0, 18);

            fusionSlotA = FusionSlot("Spare 1", out fusionSlotPictureA, out fusionSlotEmptyA, out fusionSlotNameA, out fusionSlotTempA, out fusionSlotTempPillA, () => { fuseFirst = null; RefreshFusion(); });
            fusionSlotB = FusionSlot("Spare 2", out fusionSlotPictureB, out fusionSlotEmptyB, out fusionSlotNameB, out fusionSlotTempB, out fusionSlotTempPillB, () => { fuseSecond = null; RefreshFusion(); });

            // The mystery toy: the first spare's colour below, the second's fading in from the top, with a "?".
            fusionMystery = Rect("Mystery", fusionCard);
            var bob = Rect("Mystery motion", fusionMystery); Stretch(bob);
            fusionMysteryMotion = bob.gameObject.AddComponent<PersonalityMotion>();
            fusionMysteryMotion.Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Calm);
            var disc = Rect("Mystery disc", bob); disc.anchorMin = disc.anchorMax = disc.pivot = new Vector2(.5f, 1); disc.anchoredPosition = Vector2.zero; disc.sizeDelta = new Vector2(84, 84);
            fusionMysteryBottom = Surface(disc, PockleTheme.FillDeep, 42);
            disc.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var top = Rect("Mystery top colour", disc); Stretch(top);
            fusionMysteryTop = top.gameObject.AddComponent<RawImage>(); fusionMysteryTop.texture = UiShapes.FadeDown; fusionMysteryTop.raycastTarget = false;
            fusionMysteryTop.rectTransform.localScale = new Vector3(1, -1, 1); // opaque at the top, clear at the bottom
            var shine = Rect("Mystery shine", disc); shine.anchorMin = shine.anchorMax = shine.pivot = new Vector2(.32f, .7f); shine.sizeDelta = new Vector2(46, 34);
            var shineImage = shine.gameObject.AddComponent<RawImage>(); shineImage.texture = UiShapes.Glow; shineImage.color = new Color(1, 1, 1, .55f); shineImage.raycastTarget = false;
            var question = Label("?", disc, 36, Color.white, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(question.rectTransform);
            fusionMysteryLine = Label("", fusionMystery, PockleTheme.SmallSize, LabMuted, FontStyle.Bold, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            fusionMysteryLine.rectTransform.anchorMin = new Vector2(0, 1); fusionMysteryLine.rectTransform.anchorMax = new Vector2(1, 1); fusionMysteryLine.rectTransform.pivot = new Vector2(.5f, 1);
            fusionMysteryLine.rectTransform.anchoredPosition = new Vector2(0, -90); fusionMysteryLine.rectTransform.sizeDelta = new Vector2(30, 32);
            fusionMysteryLine.horizontalOverflow = HorizontalWrapMode.Overflow;

            fusionSecret = Rect("Secret recipe", fusionCard); Surface(fusionSecret, PockleTheme.FieldGold, 13);
            var secretStar = Rect("Star", fusionSecret);
            var secretStarImage = secretStar.gameObject.AddComponent<RawImage>(); secretStarImage.texture = UiShapes.Sparkle; secretStarImage.color = Ink; secretStarImage.raycastTarget = false;
            secretStar.anchorMin = secretStar.anchorMax = secretStar.pivot = new Vector2(0, .5f); secretStar.anchoredPosition = new Vector2(10, 0); secretStar.sizeDelta = new Vector2(13, 13);
            var secret = Label("This pair hides a secret recipe", fusionSecret, PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            secret.horizontalOverflow = HorizontalWrapMode.Overflow; secret.resizeTextForBestFit = false;
            Stretch(secret.rectTransform); secret.rectTransform.offsetMin = new Vector2(28, 0);

            // Hint chips: colour, finish, personality.
            string[] chipNames = { "COLOUR", "FINISH", "PERSONALITY" };
            for (int i = 0; i < 3; i++)
            {
                var chip = Rect("Hint · " + chipNames[i], fusionCard); Surface(chip, LabGlass, 16);
                var name = Label(chipNames[i], chip, 10, LabMuted, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
                LeftLabel(name.rectTransform, new Vector2(9, -6), new Vector2(120, 14));
                fusionChipValues[i] = Label("", chip, PockleTheme.SmallSize, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
                if (i == 0)
                {
                    var swatch = Rect("Swatch", chip); LeftLabel(swatch, new Vector2(9, -26), new Vector2(14, 14));
                    fusionColourSwatchBottom = Surface(swatch, PockleTheme.FillDeep, 7);
                    swatch.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                    var swatchTop = Rect("Swatch top", swatch); Stretch(swatchTop);
                    fusionColourSwatchTop = swatchTop.gameObject.AddComponent<RawImage>(); fusionColourSwatchTop.texture = UiShapes.FadeDown; fusionColourSwatchTop.raycastTarget = false;
                    fusionColourSwatchTop.rectTransform.localScale = new Vector3(1, -1, 1);
                }
            }

            fusionSparesHeading = Label("Your spares", fusionRoot, 20, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 28)).rectTransform;
            fusionSparesNote = Label("Tap two · same one twice works too", fusionRoot, PockleTheme.SmallSize, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleRight, Vector2.zero, new Vector2(200, 28)).rectTransform;
            for (int i = 0; i < spareTiles.Length; i++)
            {
                string id = FusionOrder[i];
                var tile = Rect("Spare · " + id, fusionRoot);
                spareRings[i] = Surface(tile, Ink, 22); spareRings[i].raycastTarget = true;
                spareTiles[i] = tile.gameObject.AddComponent<Button>(); spareTiles[i].targetGraphic = spareRings[i];
                spareTiles[i].onClick.AddListener(() => PickSpare(id));
                tile.gameObject.AddComponent<SquishFeedback>();
                var face = Rect("Face", tile); Stretch(face); spareFaces[i] = Surface(face, PockleTheme.FieldFor(id), 20);
                var art = Rect("Toy", face); art.anchorMin = art.anchorMax = art.pivot = new Vector2(.5f, 1); art.anchoredPosition = new Vector2(0, -8); art.sizeDelta = new Vector2(74, 74);
                sparePictures[i] = art.gameObject.AddComponent<RawImage>(); sparePictures[i].raycastTarget = false;
                spareNames[i] = Label(PersonalityBook.ShortName(id), face, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                spareNames[i].rectTransform.anchorMin = new Vector2(0, 0); spareNames[i].rectTransform.anchorMax = new Vector2(1, 0); spareNames[i].rectTransform.pivot = new Vector2(.5f, 0);
                spareNames[i].rectTransform.anchoredPosition = new Vector2(0, 26); spareNames[i].rectTransform.sizeDelta = new Vector2(-8, 18);
                spareLabels[i] = Label("", face, PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                spareLabels[i].rectTransform.anchorMin = new Vector2(0, 0); spareLabels[i].rectTransform.anchorMax = new Vector2(1, 0); spareLabels[i].rectTransform.pivot = new Vector2(.5f, 0);
                spareLabels[i].rectTransform.anchoredPosition = new Vector2(0, 9); spareLabels[i].rectTransform.sizeDelta = new Vector2(-8, 16);
                spareBadgeRects[i] = Rect("Order", face); Surface(spareBadgeRects[i], Ink, 13);
                spareBadgeRects[i].anchorMin = spareBadgeRects[i].anchorMax = spareBadgeRects[i].pivot = new Vector2(0, 1);
                spareBadgeRects[i].anchoredPosition = new Vector2(8, -8);
                spareBadges[i] = Label("", spareBadgeRects[i], PockleTheme.CaptionSize, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(spareBadges[i].rectTransform);
            }
            fusionNoSpareRow = Rect("No spare yet", fusionRoot); Surface(fusionNoSpareRow, Quiet, 20);
            fusionNoSpareText = Label("", fusionNoSpareRow, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            Stretch(fusionNoSpareText.rectTransform); fusionNoSpareText.rectTransform.offsetMin = new Vector2(14, 6); fusionNoSpareText.rectTransform.offsetMax = new Vector2(-14, -6);
            fusionEmpty = Label("No spares yet. Spares come from walking boxes and the daily check-in.", fusionRoot, PockleTheme.BodySize, MutedInk,
                FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, new Vector2(300, 60)).rectTransform;
            fusionGoNote = Label("", fusionRoot, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(300, 20));
            fusionGo = CreateButton("Pick two spares", fusionRoot, Vector2.zero, new Vector2(300, 56), PockleTheme.Plum, Fuse, out _);
            fusionGo.name = "Fuse";
            BuildFusionReveal();
        }

        private Button FusionSlot(string title, out RawImage picture, out RectTransform empty, out Text name, out Text temperament, out RectTransform temperamentPill, System.Action clear)
        {
            var slot = CreateButton("", fusionCard, Vector2.zero, new Vector2(SlotWidth, 190), new Color(1, 1, 1, 0), clear, out _, 13);
            slot.name = "Fusion slot · " + title;
            var rect = slot.GetComponent<RectTransform>();
            var pedestal = Rect("Pedestal", rect);
            var pedestalImage = pedestal.gameObject.AddComponent<RawImage>(); pedestalImage.texture = UiShapes.Glow; pedestalImage.color = new Color32(0xFF, 0xD6, 0xF0, 0x90); pedestalImage.raycastTarget = false;
            pedestal.anchorMin = pedestal.anchorMax = pedestal.pivot = new Vector2(.5f, 1); pedestal.anchoredPosition = new Vector2(0, -112); pedestal.sizeDelta = new Vector2(104, 26);
            var art = Rect("Spare", rect); art.anchorMin = art.anchorMax = art.pivot = new Vector2(.5f, 1); art.anchoredPosition = new Vector2(0, -16); art.sizeDelta = new Vector2(104, 104);
            picture = art.gameObject.AddComponent<RawImage>(); picture.raycastTarget = false;
            art.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Calm);
            empty = Rect("Empty", rect); empty.anchorMin = empty.anchorMax = empty.pivot = new Vector2(.5f, 1); empty.anchoredPosition = new Vector2(0, -26); empty.sizeDelta = new Vector2(84, 84);
            Surface(empty, new Color(1, 1, 1, .1f), 42);
            var plus = Label("+", empty, 26, LabMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(plus.rectTransform); plus.rectTransform.offsetMin = new Vector2(0, 18);
            var pick = Label("Pick a spare", empty, PockleTheme.SmallSize - 1, LabMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(pick.rectTransform); pick.rectTransform.offsetMax = new Vector2(0, -24);
            name = Label("", rect, PockleTheme.CaptionSize + 1, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            name.rectTransform.anchorMin = name.rectTransform.anchorMax = name.rectTransform.pivot = new Vector2(.5f, 1);
            name.rectTransform.anchoredPosition = new Vector2(0, -126); name.rectTransform.sizeDelta = new Vector2(SlotWidth, 20);
            temperamentPill = Rect("Temperament", rect); Surface(temperamentPill, new Color(1, 1, 1, .14f), 11);
            temperamentPill.anchorMin = temperamentPill.anchorMax = temperamentPill.pivot = new Vector2(.5f, 1); temperamentPill.anchoredPosition = new Vector2(0, -150);
            temperament = Label("", temperamentPill, PockleTheme.SmallSize - 1, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(temperament.rectTransform); temperament.horizontalOverflow = HorizontalWrapMode.Overflow; temperament.resizeTextForBestFit = false;
            return slot;
        }

        /// <summary>Spares are the extra copies: the last one of a toy is never offered.</summary>
        private int SpareCount(string id) => session != null ? Mathf.Max(0, session.GetOwnedCount(id) - 1) : 0;

        /// <summary>
        /// Tap logic from the canvas: a toy already picked as often as it has spares (or with both slots full) is put back;
        /// otherwise it fills the next empty slot, or replaces the second pick.
        /// </summary>
        private void PickSpare(string id)
        {
            int spares = SpareCount(id);
            if (spares <= 0) return;
            int picked = (fuseFirst == id ? 1 : 0) + (fuseSecond == id ? 1 : 0);
            bool full = fuseFirst != null && fuseSecond != null;
            if (picked > 0 && (picked >= spares || full))
            {
                if (fuseSecond == id) fuseSecond = null;
                if (fuseFirst == id) fuseFirst = null;
            }
            else if (fuseFirst == null) fuseFirst = id;
            else if (fuseSecond == null) fuseSecond = id;
            else fuseSecond = id;
            RefreshFusion();
        }

        private void Fuse()
        {
            if (fuseFirst == null || fuseSecond == null) return;
            // A real service persists the fused toy before anything animates; the preview uses nothing.
            lastFusionWasPreview = !(fusion.Available && fusion.TryFuse(fuseFirst, fuseSecond));
            StartFusionMoment(); // the 4-second moment, then the reveal (PrototypeHud.FusionMoment.cs)
        }

        private void RefreshFusion()
        {
            if (fusionGo == null) return;
            // Drop picks that are no longer spares (for example after a trade elsewhere).
            int Picked(string id) => (fuseFirst == id ? 1 : 0) + (fuseSecond == id ? 1 : 0);
            if (fuseSecond != null && Picked(fuseSecond) > SpareCount(fuseSecond)) fuseSecond = null;
            if (fuseFirst != null && Picked(fuseFirst) > SpareCount(fuseFirst)) fuseFirst = null;
            ShowSlot(fusionSlotA, fusionSlotPictureA, fusionSlotEmptyA, fusionSlotNameA, fusionSlotTempA, fusionSlotTempPillA, fuseFirst);
            ShowSlot(fusionSlotB, fusionSlotPictureB, fusionSlotEmptyB, fusionSlotNameB, fusionSlotTempB, fusionSlotTempPillB, fuseSecond);
            bool ready = fuseFirst != null && fuseSecond != null;
            var outcome = ready ? PersonalityBook.DescribeFusion(fuseFirst, fuseSecond) : null;

            // The mystery toy and the three hints. Its name and personality stay hidden until the reveal.
            Color bottom = fuseFirst != null ? ToyColour(fuseFirst) : (Color)new Color32(0x8C, 0x7F, 0xB0, 0xFF);
            Color top = ready ? ToyColour(fuseSecond) : bottom;
            fusionMysteryBottom.color = bottom; fusionMysteryTop.color = top;
            fusionColourSwatchBottom.color = bottom; fusionColourSwatchTop.color = top;
            fusionMysteryMotion.SetStyle(ready ? PersonalityMotionStyle.Dream : PersonalityMotionStyle.Calm);
            bool hybrid = ready && PersonalityBook.HybridOf(PersonalityBook.TemperamentOf(fuseFirst), PersonalityBook.TemperamentOf(fuseSecond)) != null;
            fusionMysteryLine.text = !ready ? "Pick two spares" : outcome != null && outcome.Secret ? "Secret recipe!" : hybrid ? "A named hybrid?" : "Who will it be?";
            fusionChipValues[0].text = outcome != null ? outcome.Colour : "?";
            fusionChipValues[1].text = outcome != null ? outcome.Material : "?";
            fusionChipValues[2].text = !ready ? "?" : fuseFirst == fuseSecond ? "Extra " + PersonalityBook.TemperamentName(PersonalityBook.TemperamentOf(fuseFirst)).ToLowerInvariant()
                : PersonalityBook.TemperamentName(PersonalityBook.TemperamentOf(fuseFirst)) + " + " + PersonalityBook.TemperamentName(PersonalityBook.TemperamentOf(fuseSecond));
            fusionSecret.gameObject.SetActive(outcome != null && outcome.Secret);

            int shown = 0;
            var noSpare = new List<string>();
            // Which spare tiles (and the rows under them) were visible before this refresh. If that changes, the grid is
            // laid out again so the remaining tiles close up from the top-left instead of keeping their old slots.
            string visibleBefore = SpareVisibilityMask();
            for (int i = 0; i < spareTiles.Length; i++)
            {
                string id = FusionOrder[i];
                int owned = session != null ? session.GetOwnedCount(id) : 0;
                bool playable = session != null && session.TryGetPlayableVariant(id, out _);
                bool has = owned > 1 && playable;
                if (owned == 1 && playable) noSpare.Add(PersonalityBook.ShortName(id));
                spareTiles[i].gameObject.SetActive(has);
                if (!has) continue;
                shown++;
                int spares = SpareCount(id);
                spareLabels[i].text = spares + (spares == 1 ? " spare" : " spares");
                if (sparePictures[i].texture == null) sparePictures[i].texture = CutoutPortrait(id);
                bool first = fuseFirst == id, second = fuseSecond == id;
                spareBadges[i].text = first && second ? "1 · 2" : first ? "1" : second ? "2" : "";
                spareBadgeRects[i].gameObject.SetActive(first || second);
                spareBadgeRects[i].sizeDelta = new Vector2(first && second ? 44 : 26, 26);
                spareRings[i].color = first || second ? Ink : PockleTheme.FieldFor(id);
                var face = spareFaces[i].rectTransform; face.offsetMin = first || second ? new Vector2(3, 3) : Vector2.zero; face.offsetMax = first || second ? new Vector2(-3, -3) : Vector2.zero;
                spareTiles[i].interactable = true;
            }
            fusionEmpty.gameObject.SetActive(shown == 0);
            fusionNoSpareRow.gameObject.SetActive(noSpare.Count > 0 && shown > 0);
            fusionNoSpareText.text = noSpare.Count == 0 ? "" :
                (noSpare.Count == 1 ? noSpare[0] + " has no spare yet." : string.Join(" and ", noSpare.GetRange(0, Mathf.Min(2, noSpare.Count))) + (noSpare.Count > 2 ? " and more have" : " have") + " no spares yet.")
                + " A spare is an extra copy; your last one always stays on your shelf.";
            if (SpareVisibilityMask() != visibleBefore && page == AppPage.Fusion && lastLayoutSize.x > 0) AdaptLayout();

            fusionGo.interactable = ready;
            fusionGo.GetComponentInChildren<Text>().text = ready ? "Fuse " + FirstName(fuseFirst) + " + " + FirstName(fuseSecond) : "Pick two spares";
            fusionGo.GetComponent<Image>().color = ready ? PockleTheme.Plum : PockleTheme.FillDeep;
            fusionGo.GetComponentInChildren<Text>().color = ready ? PockleTheme.OnPlum : MutedInk;
            fusionGoNote.text = ready ? "Uses one spare of each. It can't be undone or traded." : "Your last copy of a toy is never used.";
        }

        /// <summary>Which spare tiles, the empty message and the no-spare row are showing, so a change can trigger a relayout.
        /// A string rather than a bit mask, so it keeps working however many spare tiles a collection adds.</summary>
        private string SpareVisibilityMask()
        {
            var shown = new System.Text.StringBuilder(spareTiles.Length + 2);
            foreach (var tile in spareTiles) shown.Append(tile != null && tile.gameObject.activeSelf ? '1' : '0');
            shown.Append(fusionEmpty != null && fusionEmpty.gameObject.activeSelf ? '1' : '0');
            shown.Append(fusionNoSpareRow != null && fusionNoSpareRow.gameObject.activeSelf ? '1' : '0');
            return shown.ToString();
        }

        /// <summary>A toy's own colour (its finish swatch), for the mystery toy and the fusion moment.</summary>
        private static Color ToyColour(string id) => ToyCatalog.TryGetCollectible(id, out var toy) ? CatalogUnityColor(toy.SwatchColor) : PockleTheme.FillDeep;

        private static string FirstName(string id) => PersonalityBook.ShortName(id).Split(' ')[0];

        private void ShowSlot(Button slot, RawImage picture, RectTransform empty, Text name, Text temperament, RectTransform pill, string id)
        {
            bool has = !string.IsNullOrEmpty(id);
            picture.texture = has ? CutoutPortrait(id) : null;
            picture.gameObject.SetActive(has && picture.texture != null);
            empty.gameObject.SetActive(!has);
            name.text = has ? PersonalityBook.ShortName(id) : "Empty";
            temperament.text = has ? PersonalityBook.TemperamentName(PersonalityBook.TemperamentOf(id)) : "Pick one below";
            pill.sizeDelta = new Vector2(temperament.preferredWidth + 20, 22);
            slot.interactable = has;
        }

        private float LayoutFusion(float width)
        {
            float y = 4;
            TopCentered(fusionCard, new Vector2(0, -y), new Vector2(width, FusionCardHeight));
            LeftLabel(fusionSlotA.GetComponent<RectTransform>(), new Vector2(8, -40), new Vector2(SlotWidth, 180));
            LeftLabel(fusionSlotB.GetComponent<RectTransform>(), new Vector2(width - 8 - SlotWidth, -40), new Vector2(SlotWidth, 180));
            LeftLabel(fusionMystery, new Vector2(width / 2 - 60, -62), new Vector2(120, 124));
            float secretWidth = fusionSecret.GetComponentInChildren<Text>().preferredWidth + 40;
            LeftLabel(fusionSecret, new Vector2(width / 2 - secretWidth / 2, -190), new Vector2(secretWidth, 26));
            // Chips along the bottom of the card.
            float chipsWidth = width - 24, gap = 6;
            float[] shares = { 1.3f, .85f, 1.4f };
            float total = shares[0] + shares[1] + shares[2], x = 12;
            for (int i = 0; i < 3; i++)
            {
                float w = (chipsWidth - gap * 2) * shares[i] / total;
                var chip = fusionChipValues[i].transform.parent as RectTransform;
                LeftLabel(chip, new Vector2(x, -(FusionCardHeight - 12 - 52)), new Vector2(w, 52));
                LeftLabel(fusionChipValues[i].rectTransform, new Vector2(i == 0 ? 28 : 9, -22), new Vector2(w - (i == 0 ? 32 : 14), 22));
                x += w + gap;
            }
            y += FusionCardHeight + 20;
            LeftLabel(fusionSparesHeading, new Vector2(0, -y), new Vector2(width * .45f, 28));
            LeftLabel(fusionSparesNote, new Vector2(width * .4f, -y), new Vector2(width * .6f, 28));
            y += 38;
            const float tileGap = 8;
            const int columns = 3;
            float tile = (width - tileGap * (columns - 1)) / columns;
            int slot = 0;
            for (int i = 0; i < spareTiles.Length; i++)
            {
                if (!spareTiles[i].gameObject.activeSelf) continue;
                LeftLabel(spareTiles[i].GetComponent<RectTransform>(), new Vector2(slot % columns * (tile + tileGap), -(y + slot / columns * (SpareTileHeight + tileGap))), new Vector2(tile, SpareTileHeight));
                slot++;
            }
            if (slot == 0) { TopCentered(fusionEmpty, new Vector2(0, -y - 8), new Vector2(width, 60)); y += 68; }
            else y += ((slot + columns - 1) / columns) * (SpareTileHeight + tileGap) - tileGap;
            if (fusionNoSpareRow.gameObject.activeSelf) { y += 12; LeftLabel(fusionNoSpareRow, new Vector2(0, -y), new Vector2(width, 56)); y += 56; }
            y += 22;
            LeftLabel(fusionGoNote.rectTransform, new Vector2(0, -y), new Vector2(width, 20)); y += 28;
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
            revealPersonalityLine = Label("", revealSheet, PockleTheme.BodySize, PockleTheme.JellyInk, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, new Vector2(300, 44));
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
            revealPersonalityLine.text = outcome.PersonalityLine;
            revealParentA.texture = previews[(int)a]; revealParentFaceA.color = PockleTheme.FieldFor(fuseFirst);
            revealParentB.texture = previews[(int)b]; revealParentFaceB.color = PockleTheme.FieldFor(fuseSecond);
            revealParentsLine.text = "Fused from " + ToyName(a) + " and " + ToyName(b);
            revealTraitValues[0].text = outcome.Body; revealTraitValues[1].text = outcome.Colour;
            revealTraitValues[2].text = outcome.Material; revealTraitValues[3].text = outcome.Personality;
            revealPreviewNote.gameObject.SetActive(lastFusionWasPreview);
            revealAdd.interactable = !lastFusionWasPreview;
            if (PersonalityBook.TryGet(fuseFirst, out var personality)) revealMotion.SetStyle(personality.Motion == PersonalityMotionStyle.Show
                ? PersonalityMotionStyle.Show : PersonalityMotionStyle.Calm);
            revealPicture.texture = FusedPicture(fuseFirst, fuseSecond); // clear background, no square behind the toy
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
            LeftLabel(revealName.rectTransform, new Vector2(left, -y), new Vector2(inner, 38)); y += 42;
            // "It's… Secretly Sweet! Grumpy on the outside, soft on the inside." Up to two lines.
            LeftLabel(revealPersonalityLine.rectTransform, new Vector2(left, -y), new Vector2(inner, 44)); y += 44 + 8;
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
