using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Home's big picture sections (owner-approved canvas "Home for little hands", October 19): Play, Fuse, Boxes and Badges
    /// as painted tiles with the toys or boxes on top and a one-word label, so a child who can't read yet can find them.
    /// Each tile uses its own painting from Resources/UI/Features (Codex) and falls back to a collection cover until then.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private sealed class FeatureTile
        {
            public Button Button; public RawImage Art; public RectTransform Content; public string ArtName, FallbackCover;
        }
        private FeatureTile featurePlay, featureFuse, featureBoxes, featureBadges;
        private readonly RawImage[] featurePlayToys = new RawImage[3];
        private readonly RawImage[] featureBoxArt = new RawImage[2];
        private RawImage featureFuseA, featureFuseB, featureBadgeArt;
        private Text featureFuseCount;
        private RectTransform featureFuseChip, featureFusePlus, featureFuseEquals, featureFuseOrb;

        private void BuildHomeFeatures()
        {
            featurePlay = BuildFeature("Play", "Home feature · Play", "FeaturePlay", "pip", "Shelf", PockleTheme.FieldPeach, () => SelectTab(AppPage.Shelf), false);
            for (int i = 0; i < featurePlayToys.Length; i++) featurePlayToys[i] = FeaturePicture(featurePlay.Content, "Toy");

            featureFuse = BuildFeature("Fuse", "Home feature · Fuse", "FeatureFuse", "snowy", "Fuse", PockleTheme.Midnight, () => SelectTab(AppPage.Fusion), true);
            featureFuseA = FeaturePicture(featureFuse.Content, "First spare");
            var plus = featureFusePlus = Rect("Plus", featureFuse.Content); Surface(plus, PockleTheme.Milk, 10);
            var plusText = Label("+", plus, 16, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(plusText.rectTransform);
            featureFuseB = FeaturePicture(featureFuse.Content, "Second spare");
            var equals = Label("=", featureFuse.Content, 20, PockleTheme.FieldGold, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            equals.name = "Equals"; featureFuseEquals = equals.rectTransform;
            var orb = featureFuseOrb = Rect("Mystery", featureFuse.Content); Surface(orb, PockleTheme.FieldGold, 21);
            var orbGlow = Rect("Glow", orb); orbGlow.SetAsFirstSibling(); orbGlow.anchorMin = orbGlow.anchorMax = new Vector2(.5f, .5f); orbGlow.sizeDelta = new Vector2(80, 80);
            var glowImage = orbGlow.gameObject.AddComponent<RawImage>(); glowImage.texture = UiShapes.Glow; glowImage.color = new Color(1, .89f, .64f, .7f); glowImage.raycastTarget = false;
            var mark = Label("?", orb, 22, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(mark.rectTransform);
            featureFuseChip = Rect("Spares", featureFuse.Button.transform); Surface(featureFuseChip, PockleTheme.FieldGold, 13);
            featureFuseCount = Label("", featureFuseChip, 13, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(featureFuseCount.rectTransform); featureFuseCount.horizontalOverflow = HorizontalWrapMode.Overflow; featureFuseCount.resizeTextForBestFit = false;

            featureBoxes = BuildFeature("Boxes", "Home feature · Boxes", "FeatureBoxes", "fox", "Box", PockleTheme.FieldMint, () => SelectTab(AppPage.Boxes), false);
            for (int i = 0; i < featureBoxArt.Length; i++) featureBoxArt[i] = FeaturePicture(featureBoxes.Content, "Box");

            featureBadges = BuildFeature("Badges", "Home feature · Badges", "FeatureBadges", "firb", "Heart", PockleTheme.FieldGold, OpenBadges, false);
            featureBadgeArt = FeaturePicture(featureBadges.Content, "Badge");
        }

        private FeatureTile BuildFeature(string word, string name, string artName, string fallbackCover, string icon, Color field, System.Action click, bool night, Transform parent = null)
        {
            var tile = new FeatureTile { ArtName = artName, FallbackCover = fallbackCover };
            tile.Button = CreateButton(word, parent != null ? parent : homeRoot, Vector2.zero, new Vector2(170, 150), field, click, out var face, 18, false);
            SetRadius(face, 28);
            tile.Button.name = name;
            tile.Button.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var art = Rect("Painting", tile.Button.transform); Stretch(art); art.SetAsFirstSibling();
            tile.Art = art.gameObject.AddComponent<RawImage>(); tile.Art.raycastTarget = false; tile.Art.enabled = false;
            // A soft shade keeps the toys and the label readable over any painting; the night tile is darker for its white glow.
            var shade = Rect("Shade", tile.Button.transform); Stretch(shade); shade.SetSiblingIndex(1);
            var shadeImage = shade.gameObject.AddComponent<RawImage>(); shadeImage.raycastTarget = false;
            if (night) shadeImage.color = new Color(.18f, .16f, .36f, .72f);
            else { shadeImage.texture = UiShapes.FadeDown; shadeImage.color = new Color(.16f, .08f, .19f, .28f); shade.localScale = new Vector3(1, -1, 1); }
            tile.Content = Rect("Pictures", tile.Button.transform);
            // The word sits on a white pill with its icon, bottom left, so it reads on every painting.
            var label = tile.Button.GetComponentInChildren<Text>();
            var pill = Rect("Label", tile.Button.transform); Surface(pill, new Color(.984f, .973f, .961f, .95f), 19);
            pill.anchorMin = pill.anchorMax = pill.pivot = new Vector2(0, 0); pill.anchoredPosition = new Vector2(10, 10);
            label.transform.SetParent(pill, false);
            label.color = Ink; label.alignment = TextAnchor.MiddleLeft; label.horizontalOverflow = HorizontalWrapMode.Overflow; label.resizeTextForBestFit = false;
            var lr = label.rectTransform; lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = new Vector2(36, 0); lr.offsetMax = new Vector2(-12, 0);
            var glyph = AddIcon(pill, icon, 20, Ink);
            glyph.anchorMin = glyph.anchorMax = glyph.pivot = new Vector2(0, .5f); glyph.anchoredPosition = new Vector2(10, 0);
            pill.sizeDelta = new Vector2(36 + label.preferredWidth + 14, 38);
            return tile;
        }

        private RawImage FeaturePicture(RectTransform parent, string name)
        {
            var rect = Rect(name, parent); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            var raw = rect.gameObject.AddComponent<RawImage>(); raw.raycastTarget = false; raw.enabled = false;
            return raw;
        }

        private void RefreshHomeFeatures()
        {
            if (featurePlay == null || session == null) return;
            bool visible = page == AppPage.Home; // pictures load only while Home is on screen
            foreach (var tile in new[] { featurePlay, featureFuse, featureBoxes, featureBadges })
                if (visible && tile.Art.texture == null)
                {
                    var painting = Resources.Load<Texture2D>("UI/Features/" + tile.ArtName);
                    SetPicture(tile.Art, painting != null ? painting : CollectionLook.Banner(tile.FallbackCover));
                }

            // Play: the first toys on your live shelf.
            for (int i = 0; i < featurePlayToys.Length; i++)
                SetPicture(featurePlayToys[i], visible && i < liveShelf.Count ? CutoutPortrait(liveShelf[i]) : null);

            // Fuse: two toys you have spares of (the same character), or any two you own, and how many spares you have.
            string a = null, b = null; int spares = 0;
            foreach (string id in FusionOrder)
            {
                int extra = SpareCount(id);
                if (extra <= 0 || !session.IsOwnedAndPlayable(id)) continue;
                spares += extra;
                if (a == null) a = id;
                else if (b == null && ToyCatalog.TryGetCollectible(a, out var first) && ToyCatalog.TryGetCollectible(id, out var next) && first.CharacterId == next.CharacterId) b = id;
            }
            if (b == null && a != null && SpareCount(a) > 1) b = a;
            if (a == null) foreach (string id in liveShelf) { if (a == null) a = id; else if (b == null) b = id; }
            SetPicture(featureFuseA, visible && a != null ? CutoutPortrait(a) : null);
            SetPicture(featureFuseB, visible && b != null ? CutoutPortrait(b) : null);
            featureFuseChip.gameObject.SetActive(spares > 0);
            featureFuseCount.text = spares + (spares == 1 ? " spare" : " spares");

            // Boxes: the boxes of the collections you're closest to finishing (unfinished first).
            var boxes = new List<CollectionDefinition>();
            foreach (var c in ToyCatalog.Collections)
                if (FoundCount(c) < c.Members.Count && CollectionLook.Banner(c) != null) boxes.Add(c);
            boxes.Sort((x, y) => FoundCount(y).CompareTo(FoundCount(x)));
            for (int i = 0; i < featureBoxArt.Length; i++)
                SetPicture(featureBoxArt[i], visible && i < boxes.Count ? CollectionLook.Box(boxes[i], out _) : null);

            // Badges: your newest badge in colour, or the first one to earn, faded.
            string badgeId = badgeCabinet != null ? badgeCabinet.Latest : null;
            bool earned = badgeId != null;
            if (badgeId == null) badgeId = "secret-finder";
            SetPicture(featureBadgeArt, visible && BadgeBook.TryGet(badgeId, out var badge) ? BadgeArt(badge) : null);
            featureBadgeArt.color = earned ? Color.white : new Color(1, 1, 1, .55f);
        }

        /// <summary>Lays out the 2×2 sections from <paramref name="y"/>; returns the new y.</summary>
        private float LayoutHomeFeatures(float left, float w, float y)
        {
            const float gap = 12;
            float tileWidth = (w - gap) / 2, tileHeight = Mathf.Clamp(Mathf.Round(tileWidth * .86f), 120, 180);
            var tiles = new[] { featurePlay, featureFuse, featureBoxes, featureBadges };
            for (int i = 0; i < tiles.Length; i++)
            {
                var rect = tiles[i].Button.GetComponent<RectTransform>();
                LeftLabel(rect, new Vector2(left + (i % 2) * (tileWidth + gap), -(y + (i / 2) * (tileHeight + gap))), new Vector2(tileWidth, tileHeight));
                tiles[i].Art.uvRect = CollectionLook.Cover(tiles[i].Art.texture, tileWidth / tileHeight, .5f, .5f);
                var content = tiles[i].Content; content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(.5f, 1);
                content.anchoredPosition = new Vector2(0, -8); content.sizeDelta = new Vector2(0, tileHeight - 56);
            }
            float area = tileHeight - 56; // the pictures sit above the label pill

            // Play: three toys, the middle one a little bigger.
            float toy = Mathf.Min(area, tileWidth / 2.6f);
            for (int i = 0; i < featurePlayToys.Length; i++)
            {
                float size = i == 1 ? toy * 1.15f : toy * .9f;
                var r = featurePlayToys[i].rectTransform; r.sizeDelta = new Vector2(size, size);
                r.anchoredPosition = new Vector2((i - 1) * toy * .78f, -(area - size) / 2);
            }
            featurePlayToys[1].transform.SetAsLastSibling();

            // Fuse: toy + toy = ?
            float spare = Mathf.Min(area * .8f, tileWidth / 4.4f), unit = tileWidth / 5f;
            featureFuseA.rectTransform.sizeDelta = featureFuseB.rectTransform.sizeDelta = new Vector2(spare, spare);
            featureFuseA.rectTransform.anchoredPosition = new Vector2(-2 * unit, 0);
            var plus = featureFusePlus; plus.anchorMin = plus.anchorMax = plus.pivot = new Vector2(.5f, .5f);
            plus.sizeDelta = new Vector2(20, 20); plus.anchoredPosition = new Vector2(-1.1f * unit, 0);
            featureFuseB.rectTransform.anchoredPosition = new Vector2(-.2f * unit, 0);
            var equals = featureFuseEquals; equals.anchorMin = equals.anchorMax = equals.pivot = new Vector2(.5f, .5f);
            equals.sizeDelta = new Vector2(20, 24); equals.anchoredPosition = new Vector2(.75f * unit, 0);
            var orb = featureFuseOrb; orb.anchorMin = orb.anchorMax = orb.pivot = new Vector2(.5f, .5f);
            orb.sizeDelta = new Vector2(42, 42); orb.anchoredPosition = new Vector2(1.75f * unit, 0);
            foreach (var r in new[] { featureFuseA.rectTransform, featureFuseB.rectTransform, plus, equals, orb })
                r.anchoredPosition = new Vector2(r.anchoredPosition.x, 4); // pictures are centred in the area above the label
            featureFuseChip.anchorMin = featureFuseChip.anchorMax = featureFuseChip.pivot = new Vector2(1, 1);
            featureFuseChip.anchoredPosition = new Vector2(-10, -10); featureFuseChip.sizeDelta = new Vector2(featureFuseCount.preferredWidth + 18, 26);

            // Boxes: two boxes side by side.
            float box = Mathf.Min(area, tileWidth / 2.2f);
            for (int i = 0; i < featureBoxArt.Length; i++)
            {
                var r = featureBoxArt[i].rectTransform; r.sizeDelta = new Vector2(box, box);
                r.anchoredPosition = new Vector2((i - .5f) * box * .95f, -(area - box) / 2);
            }

            // Badges: one big badge.
            float badgeSize = Mathf.Min(area + 8, tileWidth * .62f);
            featureBadgeArt.rectTransform.sizeDelta = new Vector2(badgeSize, badgeSize);
            featureBadgeArt.rectTransform.anchoredPosition = new Vector2(0, 2);
            return y + tileHeight * 2 + gap;
        }
    }
}
