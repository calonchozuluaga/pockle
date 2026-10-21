using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Home's big picture sections (owner-approved canvas "Home for little hands", October 19): Play, Fuse, Boxes and Badges
    /// as complete illustrated scenes with a one-word label, so a child who can't read yet can find them.
    /// Each tile uses its own painting from Resources/UI/Features (Codex) and falls back to a collection cover until then.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private sealed class FeatureTile
        {
            public Button Button; public RawImage Art; public RectTransform Label; public string ArtName, FallbackCover;
        }
        private FeatureTile featurePlay, featureFuse, featureBoxes, featureBadges;
        private void BuildHomeFeatures()
        {
            featurePlay = BuildFeature("Play", "Home feature · Play", "FeaturePlay", "pip", "Shelf", PockleTheme.FieldPeach, () => SelectTab(AppPage.Shelf));
            featureFuse = BuildFeature("Fuse", "Home feature · Fuse", "FeatureFuse", "snowy", "Fuse", PockleTheme.Midnight, () => SelectTab(AppPage.Fusion));
            featureBoxes = BuildFeature("Boxes", "Home feature · Boxes", "FeatureBoxes", "fox", "Box", PockleTheme.FieldMint, () => SelectTab(AppPage.Boxes));
            featureBadges = BuildFeature("Badges", "Home feature · Badges", "FeatureBadges", "firb", "Heart", PockleTheme.FieldGold, OpenBadges);
        }

        private FeatureTile BuildFeature(string word, string name, string artName, string fallbackCover, string icon, Color field, System.Action click, Transform parent = null)
        {
            var tile = new FeatureTile { ArtName = artName, FallbackCover = fallbackCover };
            tile.Button = CreateButton(word, parent != null ? parent : homeRoot, Vector2.zero, new Vector2(170, 150), field, click, out var face, 18, false);
            SetRadius(face, 28);
            tile.Button.name = name;
            tile.Button.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var art = Rect("Painting", tile.Button.transform); Stretch(art); art.SetAsFirstSibling();
            tile.Art = art.gameObject.AddComponent<RawImage>(); tile.Art.raycastTarget = false; tile.Art.color = Color.white; tile.Art.enabled = false;
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
            tile.Label = pill;
            return tile;
        }

        private void RefreshHomeFeatures()
        {
            if (featurePlay == null || session == null || page != AppPage.Home) return;
            foreach (var tile in new[] { featurePlay, featureFuse, featureBoxes, featureBadges })
                if (tile.Art.texture == null)
                    SetPicture(tile.Art, Resources.Load<Texture2D>("UI/Features/" + tile.ArtName) ?? CollectionLook.Banner(tile.FallbackCover));
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
            }
            return y + tileHeight * 2 + gap;
        }
    }
}
