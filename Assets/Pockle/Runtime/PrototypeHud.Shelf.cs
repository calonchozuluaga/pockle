using System.Globalization;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>UX-004 shelf: a display cabinet of cubbies, one per collectible, with progress and room to grow.</summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform shelfSummary, shelfTrack, shelfComingSoon;
        private Text shelfSummaryTitle;
        private Image shelfFill;
        private readonly RectTransform[] toyMysteries = new RectTransform[4];
        private readonly RectTransform[] toyCountBadges = new RectTransform[4];
        private readonly Text[] toyCountBadgeLabels = new Text[4];
        private readonly RectTransform[] toyFavoriteBadges = new RectTransform[4];
        private readonly Text[] toyNameLabels = new Text[4];

        /// <summary>Display name from the catalog ("Moon Jelly"); falls back to the legacy label in title case.</summary>
        private static string ToyName(PipVariant variant)
        {
            if (ToyCatalog.TryGetCollectible(PipVariants.CollectibleId(variant), out var collectible)) return collectible.FinishDisplayName;
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(PipVariants.Label(variant).ToLowerInvariant());
        }

        private void BuildShelf()
        {
            previews[0] = ToyPortrait.Render(PipVariant.PeachJelly);
            previews[1] = ToyPortrait.Render(PipVariant.MoonJelly);
            previews[2] = ToyPortrait.Render(PipVariant.GoldGlitter);
            previews[3] = ToyPortrait.Render(PipVariant.MintSoft);

            // Collection progress across the top of the cabinet.
            shelfSummary = Rect("Collection progress", shelfRoot); Card(shelfSummary, Paper);
            Label("JELLY GARDEN · PIP", shelfSummary, PockleTheme.EyebrowSize, MutedInk, FontStyle.Bold, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(200, 18));
            shelfSummaryTitle = Label("0 of 4 discovered", shelfSummary, 17, Ink, FontStyle.Bold, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(200, 26));
            shelfTrack = Rect("Discovered progress", shelfSummary); Surface(shelfTrack, Quiet);
            var fill = Rect("Discovered fill", shelfTrack); Stretch(fill);
            shelfFill = fill.gameObject.AddComponent<Image>(); shelfFill.sprite = roundedSprite; shelfFill.color = PockleTheme.PeachDeep;
            shelfFill.type = Image.Type.Filled; shelfFill.fillMethod = Image.FillMethod.Horizontal; shelfFill.raycastTarget = false;

            for (int i = 0; i < toyTiles.Length; i++)
            {
                PipVariant choice = (PipVariant)i;
                toyTiles[i] = Rect(PipVariants.Label(choice) + " · shelf toy", shelfRoot);
                var face = Card(toyTiles[i], PockleTheme.ShelfFace); face.raycastTarget = true;
                toyButtons[i] = toyTiles[i].gameObject.AddComponent<Button>(); toyButtons[i].targetGraphic = face;
                var colours = toyButtons[i].colors;
                colours.pressedColor = new Color(.95f, .93f, .92f); colours.disabledColor = Color.white; colours.fadeDuration = .08f;
                toyButtons[i].colors = colours;
                toyButtons[i].onClick.AddListener(() => ShowToy(choice));
                toyTiles[i].gameObject.AddComponent<SquishFeedback>();

                toyPictures[i] = Rect("Pip portrait", toyTiles[i]);
                var image = toyPictures[i].gameObject.AddComponent<RawImage>(); image.texture = previews[i]; image.raycastTarget = false;
                // An undiscovered toy shows a mystery cubby instead of its portrait.
                toyMysteries[i] = Rect("Yet to discover", toyTiles[i]); Surface(toyMysteries[i], Quiet);
                var mark = Label("?", toyMysteries[i], 64, new Color(.84f, .78f, .74f), FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(mark.rectTransform);

                // A two-tone ceramic shelf the toy stands on.
                toyPlanks[i] = Rect("Ceramic shelf", toyTiles[i]); Surface(toyPlanks[i], PockleTheme.ShelfPlank);
                var edge = Rect("Shelf edge", toyPlanks[i]); Surface(edge, PockleTheme.ShelfEdge);
                edge.anchorMin = new Vector2(0, 0); edge.anchorMax = new Vector2(1, 0); edge.pivot = new Vector2(.5f, 0);
                edge.offsetMin = new Vector2(0, 0); edge.offsetMax = new Vector2(0, 5);

                toyNameLabels[i] = Label(ToyName(choice), toyTiles[i], 15, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(150, 24));
                toyNames[i] = toyNameLabels[i].rectTransform;
                toyCounts[i] = Label("", toyTiles[i], PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(150, 20));

                toyCountBadges[i] = Rect("Count", toyTiles[i]); Surface(toyCountBadges[i], Paper);
                toyCountBadgeLabels[i] = Label("×2", toyCountBadges[i], 13, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(toyCountBadgeLabels[i].rectTransform);
                toyFavoriteBadges[i] = Rect("Favorite", toyTiles[i]); Surface(toyFavoriteBadges[i], Paper);
                var heart = AddIcon(toyFavoriteBadges[i], "Heart", 16, PockleTheme.PeachDeep); heart.anchoredPosition = Vector2.zero;
            }

            // Honest room to grow: more collections are planned but not in this build.
            shelfComingSoon = Rect("More collections", shelfRoot); Surface(shelfComingSoon, Quiet);
            var soonIcon = AddIcon(shelfComingSoon, "Box", 22, MutedInk);
            soonIcon.anchorMin = soonIcon.anchorMax = soonIcon.pivot = new Vector2(0, .5f); soonIcon.anchoredPosition = new Vector2(16, 0);
            var soonText = Label("More little collections are on the way.", shelfComingSoon, 13, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            Stretch(soonText.rectTransform); soonText.rectTransform.offsetMin = new Vector2(48, 4); soonText.rectTransform.offsetMax = new Vector2(-14, -4);
        }

        private float LayoutShelf(float width)
        {
            float y = 0;
            const float summaryHeight = 64;
            TopCentered(shelfSummary, Vector2.zero, new Vector2(width, summaryHeight));
            var summaryLabels = shelfSummary.GetComponentsInChildren<Text>();
            float trackWidth = Mathf.Clamp(width * .32f, 60, 160);
            LeftLabel(summaryLabels[0].rectTransform, new Vector2(16, -12), new Vector2(width - trackWidth - 40, 18));
            LeftLabel(shelfSummaryTitle.rectTransform, new Vector2(16, -30), new Vector2(width - trackWidth - 40, 26));
            shelfTrack.anchorMin = shelfTrack.anchorMax = shelfTrack.pivot = new Vector2(1, .5f);
            shelfTrack.anchoredPosition = new Vector2(-16, -6); shelfTrack.sizeDelta = new Vector2(trackWidth, 8);
            y += summaryHeight + 16;

            int count = toyTiles.Length;
            int columns = width >= 650 ? 4 : width >= 470 ? 3 : width >= 260 ? 2 : 1;
            const float gap = 14;
            float tileWidth = (width - (columns - 1) * gap) / columns;
            float portrait = Mathf.Min(220, tileWidth - 16);
            float tileHeight = portrait + 74;
            int rows = (count + columns - 1) / columns;
            for (int i = 0; i < count; i++)
            {
                // Keep the last row centered when it isn't full.
                int row = i / columns, inRow = Mathf.Min(columns, count - row * columns);
                float rowOffset = (width - (inRow * tileWidth + (inRow - 1) * gap)) / 2;
                LeftLabel(toyTiles[i], new Vector2(rowOffset + (i % columns) * (tileWidth + gap), -y - row * (tileHeight + gap)),
                    new Vector2(tileWidth, tileHeight));
                TopCentered(toyPictures[i], new Vector2(0, -8), new Vector2(portrait, portrait));
                TopCentered(toyMysteries[i], new Vector2(0, -8), new Vector2(portrait, portrait));
                TopCentered(toyPlanks[i], new Vector2(0, -8 - portrait + 6), new Vector2(tileWidth - 8, 14));
                TopCentered(toyNames[i], new Vector2(0, -portrait - 24), new Vector2(tileWidth - 16, 24));
                TopCentered(toyCounts[i].rectTransform, new Vector2(0, -portrait - 48), new Vector2(tileWidth - 16, 20));
                toyCountBadges[i].anchorMin = toyCountBadges[i].anchorMax = toyCountBadges[i].pivot = new Vector2(1, 1);
                toyCountBadges[i].anchoredPosition = new Vector2(-14, -14); toyCountBadges[i].sizeDelta = new Vector2(38, 24);
                LeftLabel(toyFavoriteBadges[i], new Vector2(14, -14), new Vector2(28, 28));
            }
            y += rows * tileHeight + (rows - 1) * gap + 16;
            TopCentered(shelfComingSoon, new Vector2(0, -y), new Vector2(width, 56));
            y += 56;
            TopCentered(shelfRoot, Vector2.zero, new Vector2(width, y));
            RefreshShelf();
            return y;
        }

        private void RefreshShelf()
        {
            if (session == null || shelfSummaryTitle == null) return;
            int discovered = 0;
            for (int i = 0; i < toyTiles.Length; i++)
            {
                int owned = Owned((PipVariant)i);
                bool has = owned > 0;
                if (has) discovered++;
                toyButtons[i].interactable = has;
                toyPictures[i].gameObject.SetActive(has);
                toyMysteries[i].gameObject.SetActive(!has);
                // Blind-box rule: an undiscovered toy keeps its name a surprise.
                toyNameLabels[i].text = has ? ToyName((PipVariant)i) : "???";
                toyNameLabels[i].color = has ? Ink : MutedInk;
                toyCounts[i].text = owned > 1 ? owned + " on your shelf" : has ? "Yours to play with" : "Yet to discover";
                toyCountBadges[i].gameObject.SetActive(owned > 1);
                toyCountBadgeLabels[i].text = "×" + Mathf.Min(owned, 99);
                toyFavoriteBadges[i].gameObject.SetActive(has && profile != null && profile.FavoriteId == PipVariants.CollectibleId((PipVariant)i));
            }
            shelfSummaryTitle.text = discovered + " of " + toyTiles.Length + " discovered";
            shelfFill.fillAmount = toyTiles.Length == 0 ? 0 : discovered / (float)toyTiles.Length;
        }

        private void RefreshCollection()
        {
            if (session == null) return;
            var saved = session.Progress.Save;
            RefreshShelf(); RefreshHome(); RefreshProfile(); RefreshSettings();
            bool currentDay = saved.Day == CollectionSession.Today;
            walkProgress.text = saved.Steps.ToString("N0") + " / 1,000 steps";
            walkFill.fillAmount = saved.Steps / 1000f;
            walkStatus.text = !currentDay ? "Check your device date to continue walking." : saved.Claimed
                ? "Today's box is yours. A new box arrives tomorrow (UTC)." : session.WalkingStatus;
            openDaily.interactable = currentDay && session.Progress.CanClaim;
            openDaily.GetComponentInChildren<Text>().text = saved.Claimed ? "Opened today" : session.Progress.CanClaim ? "Open your box" : "Walk to unlock";
        }
    }
}
