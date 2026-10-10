using System.Globalization;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// v2 Shelf (UX-032): Everything / Missing / Duplicates views, then one section per character collection
    /// (newest series first) with its progress and a grid of toy tiles on their colour fields.
    /// Only Pip (Series 1) has playable art today, so there is one section.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private enum ShelfView { Everything, Missing, Duplicates }
        private ShelfView shelfView;
        private Button shelfCollections;
        private RectTransform shelfViews, shelfSection, shelfEmpty;
        private Text shelfSectionName, shelfSectionSeries, shelfSectionProgress, shelfEmptyText;
        private readonly Image[] shelfViewFaces = new Image[3];
        private readonly RectTransform[] toyMysteries = new RectTransform[4];
        private readonly RectTransform[] toyCountBadges = new RectTransform[4];
        private readonly Text[] toyCountBadgeLabels = new Text[4];
        private readonly RectTransform[] toyFavoriteBadges = new RectTransform[4];
        private readonly Text[] toyNameLabels = new Text[4];
        private readonly Image[] toyFaces = new Image[4];

        /// <summary>Display name from the catalog ("Moon Jelly"); falls back to the legacy label in title case.</summary>
        private static string ToyName(PipVariant variant)
        {
            if (ToyCatalog.TryGetCollectible(PipVariants.CollectibleId(variant), out var collectible)) return collectible.FinishDisplayName;
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(PipVariants.Label(variant).ToLowerInvariant());
        }

        /// <summary>"Pip, Series 1": the character collection a toy belongs to, from the catalog.</summary>
        private static string SeriesLine(PipVariant variant)
        {
            if (ToyCatalog.TryGetCollectible(PipVariants.CollectibleId(variant), out var collectible)
                && ToyCatalog.TryGetCollection(collectible.CollectionId, out var collection))
                return collection.SeriesNumber > 0 ? collection.DisplayName + ", Series " + collection.SeriesNumber : collection.DisplayName;
            return "Pip";
        }

        private void BuildShelf()
        {
            shelfCollections = RowButton("Browse collections", "Who's inside each collection?", "Shelf", shelfRoot,
                () => Navigate(AppPage.Collections), out _, true);
            // Each portrait is rendered once, on its toy's colour field, and shared by every screen.
            for (int i = 0; i < previews.Length; i++)
            {
                string id = PipVariants.CollectibleId((PipVariant)i);
                previews[i] = ToyPortrait.Render(id, PockleTheme.FieldFor(id));
            }

            // View switch: Everything / Missing / Duplicates.
            shelfViews = Rect("Shelf views", shelfRoot); Surface(shelfViews, Quiet, 24);
            string[] views = { "Everything", "Missing", "Duplicates" };
            for (int i = 0; i < views.Length; i++)
            {
                var view = (ShelfView)i;
                CreateButton(views[i], shelfViews, Vector2.zero, new Vector2(100, 40), Paper, () => { shelfView = view; RefreshShelf(); AdaptLayout(); },
                    out shelfViewFaces[i], 15);
            }

            // Collection section header: colour dot, name, series, and progress.
            shelfSection = Rect("Pip collection", shelfRoot);
            var dot = Rect("Collection colour", shelfSection); Surface(dot, PockleTheme.FieldPeach, 7);
            LeftLabel(dot, new Vector2(0, -10), new Vector2(14, 14));
            shelfSectionName = Label("Pip", shelfSection, 18, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(120, 28));
            shelfSectionSeries = Label(SeriesNumberLine(), shelfSection, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(120, 24));
            shelfSectionProgress = Label("", shelfSection, PockleTheme.CaptionSize, MutedInk, FontStyle.Bold, TextAnchor.MiddleRight,
                Vector2.zero, new Vector2(150, 24));

            for (int i = 0; i < toyTiles.Length; i++)
            {
                PipVariant choice = (PipVariant)i;
                toyTiles[i] = Rect(PipVariants.Label(choice) + " · shelf toy", shelfRoot);
                toyFaces[i] = Surface(toyTiles[i], PockleTheme.FieldFor(PipVariants.CollectibleId(choice)), PockleTheme.TileRadius);
                toyFaces[i].raycastTarget = true;
                toyButtons[i] = toyTiles[i].gameObject.AddComponent<Button>(); toyButtons[i].targetGraphic = toyFaces[i];
                var colours = toyButtons[i].colors;
                colours.pressedColor = new Color(.92f, .9f, .91f); colours.disabledColor = Color.white; colours.fadeDuration = .08f;
                toyButtons[i].colors = colours;
                toyButtons[i].onClick.AddListener(() => ShowToy(choice));
                toyTiles[i].gameObject.AddComponent<SquishFeedback>();

                toyPictures[i] = Portrait(toyTiles[i], i).rectTransform;
                // An undiscovered toy keeps its look a surprise: a soft "?" instead of its portrait.
                toyMysteries[i] = Rect("Yet to discover", toyTiles[i]);
                var mark = Label("?", toyMysteries[i], 44, PockleTheme.FillDeep, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(mark.rectTransform);

                toyNameLabels[i] = Label(ToyName(choice), toyTiles[i], PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(150, 20));
                toyNames[i] = toyNameLabels[i].rectTransform;
                toyCounts[i] = Label("", toyTiles[i], PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(150, 18));
                toyCounts[i].enabled = false; // Spoken by the tile name for accessibility; the badge carries the count visually.

                toyCountBadges[i] = Rect("Count", toyTiles[i]); Surface(toyCountBadges[i], Ink, 12);
                toyCountBadgeLabels[i] = Label("×2", toyCountBadges[i], PockleTheme.SmallSize, PockleTheme.OnPlum, FontStyle.Bold,
                    TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(toyCountBadgeLabels[i].rectTransform);
                toyFavoriteBadges[i] = Rect("Favorite", toyTiles[i]); Surface(toyFavoriteBadges[i], Paper, 14);
                var heart = AddIcon(toyFavoriteBadges[i], "Heart", 16, PockleTheme.Heart); heart.anchoredPosition = Vector2.zero;
            }

            shelfEmpty = Rect("Nothing in this view", shelfRoot);
            shelfEmptyText = Label("", shelfEmpty, PockleTheme.BodySize, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            Stretch(shelfEmptyText.rectTransform);
        }

        private static string SeriesNumberLine()
        {
            return ToyCatalog.TryGetCollection("pip", out var pip) && pip.SeriesNumber > 0 ? "Series " + pip.SeriesNumber : "";
        }

        private bool ShowOnShelf(int index)
        {
            int owned = Owned((PipVariant)index);
            switch (shelfView)
            {
                case ShelfView.Missing: return owned == 0;
                case ShelfView.Duplicates: return owned > 1;
                default: return true;
            }
        }

        private float LayoutShelf(float width)
        {
            LeftLabel(shelfCollections.GetComponent<RectTransform>(), Vector2.zero, new Vector2(width, 60));
            float y = 74;
            TopCentered(shelfViews, new Vector2(0, -y), new Vector2(width, 48));
            float segment = (width - 8 - 8) / 3;
            for (int i = 0; i < 3; i++)
                LeftLabel(shelfViewFaces[i].rectTransform, new Vector2(4 + i * (segment + 4), -4), new Vector2(segment, 40));
            y += 48 + 20;

            TopCentered(shelfSection, new Vector2(0, -y), new Vector2(width, 34));
            float nameWidth = Mathf.Min(shelfSectionName.preferredWidth + 4, 160);
            LeftLabel(shelfSectionName.rectTransform, new Vector2(22, -3), new Vector2(nameWidth, 28));
            LeftLabel(shelfSectionSeries.rectTransform, new Vector2(22 + nameWidth + 8, -5), new Vector2(110, 24));
            shelfSectionProgress.rectTransform.anchorMin = shelfSectionProgress.rectTransform.anchorMax = shelfSectionProgress.rectTransform.pivot = new Vector2(1, 1);
            shelfSectionProgress.rectTransform.anchoredPosition = new Vector2(0, -5);
            shelfSectionProgress.rectTransform.sizeDelta = new Vector2(Mathf.Max(80, width - 22 - nameWidth - 130), 24);
            y += 34 + 10;

            int columns = width >= 560 ? 4 : width >= 270 ? 3 : 2;
            const float gap = 10;
            float tileWidth = (width - (columns - 1) * gap) / columns;
            float tileHeight = Mathf.Round(tileWidth * 1.2f);
            float portrait = tileWidth * .78f;
            int slot = 0;
            for (int i = 0; i < toyTiles.Length; i++)
            {
                bool shown = ShowOnShelf(i);
                toyTiles[i].gameObject.SetActive(shown);
                if (!shown) continue;
                int row = slot / columns, column = slot % columns; slot++;
                LeftLabel(toyTiles[i], new Vector2(column * (tileWidth + gap), -y - row * (tileHeight + gap)), new Vector2(tileWidth, tileHeight));
                TopCentered(toyPictures[i], new Vector2(0, -6), new Vector2(portrait, portrait));
                TopCentered(toyMysteries[i], new Vector2(0, -6), new Vector2(portrait, portrait));
                toyNames[i].anchorMin = toyNames[i].anchorMax = toyNames[i].pivot = new Vector2(.5f, 0);
                toyNames[i].anchoredPosition = new Vector2(0, 10); toyNames[i].sizeDelta = new Vector2(tileWidth - 12, 20);
                TopCentered(toyCounts[i].rectTransform, new Vector2(0, -tileHeight), new Vector2(tileWidth - 12, 18));
                toyCountBadges[i].anchorMin = toyCountBadges[i].anchorMax = toyCountBadges[i].pivot = new Vector2(1, 1);
                toyCountBadges[i].anchoredPosition = new Vector2(-8, -8); toyCountBadges[i].sizeDelta = new Vector2(34, 24);
                LeftLabel(toyFavoriteBadges[i], new Vector2(8, -8), new Vector2(28, 28));
            }
            int rows = (slot + columns - 1) / columns;
            y += rows > 0 ? rows * tileHeight + (rows - 1) * gap : 0;
            shelfEmpty.gameObject.SetActive(slot == 0);
            if (slot == 0) { TopCentered(shelfEmpty, new Vector2(0, -y - 8), new Vector2(width, 60)); y += 68; }
            TopCentered(shelfRoot, Vector2.zero, new Vector2(width, y + 8));
            RefreshShelf();
            return y + 8;
        }

        /// <summary>The line under "Your shelf": how many toys and how many finishes discovered.</summary>
        private string ShelfLine()
        {
            if (session == null) return "";
            CollectionTotals(out int distinct, out int total);
            return total + (total == 1 ? " toy" : " toys") + " · " + distinct + " of " + PipVariants.Count + " Pip finishes found";
        }

        private void RefreshShelf()
        {
            if (session == null || shelfSectionProgress == null) return;
            int discovered = 0;
            for (int i = 0; i < toyTiles.Length; i++)
            {
                int owned = Owned((PipVariant)i);
                bool has = owned > 0;
                if (has) discovered++;
                toyButtons[i].interactable = has;
                toyPictures[i].gameObject.SetActive(has);
                toyMysteries[i].gameObject.SetActive(!has);
                toyFaces[i].color = has ? PockleTheme.FieldFor(PipVariants.CollectibleId((PipVariant)i)) : PockleTheme.Mystery;
                // Blind-box rule: an undiscovered toy keeps its name a surprise.
                toyNameLabels[i].text = has ? ToyName((PipVariant)i) : "???";
                toyNameLabels[i].color = has ? Ink : MutedInk;
                toyCounts[i].text = owned > 1 ? owned + " on your shelf" : has ? "Yours to play with" : "Yet to discover";
                toyCountBadges[i].gameObject.SetActive(owned > 1);
                toyCountBadgeLabels[i].text = "×" + Mathf.Min(owned, 99);
                toyFavoriteBadges[i].gameObject.SetActive(has && profile != null && profile.FavoriteId == PipVariants.CollectibleId((PipVariant)i));
            }
            shelfSectionProgress.text = discovered + " of " + toyTiles.Length + " discovered";
            for (int i = 0; i < 3; i++)
            {
                bool selected = (int)shelfView == i;
                shelfViewFaces[i].color = selected ? Paper : new Color(1, 1, 1, 0);
                shelfViewFaces[i].GetComponentInChildren<Text>().color = selected ? Ink : MutedInk;
            }
            shelfEmptyText.text = shelfView == ShelfView.Missing ? "You've found every Pip finish. Nice shelf!" : "No duplicates here yet. Spares come from walking boxes.";
            if (page == AppPage.Shelf && subtitle != null) subtitle.text = ShelfLine();
        }

        private void RefreshCollection()
        {
            if (session == null) return;
            RefreshShelf(); RefreshHome(); RefreshProfile(); RefreshSettings(); RefreshWalkingCard();
            RefreshCollections();
            if (page == AppPage.Shelf && lastLayoutSize.x > 0) AdaptLayout(); // Missing/Duplicates views change with ownership.
        }
    }
}
