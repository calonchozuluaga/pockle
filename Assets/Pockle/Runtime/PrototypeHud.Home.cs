using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>UX-003 Home: greeting, today's box card, and the Collection / Rewards / Friends places.</summary>
    public sealed partial class PrototypeHud
    {
        private const float HomeCardMaxWidth = 480f;
        private RectTransform homeDaily, homeBoxArt, homeDailyText, homeStepIcon, homeTrack, homePlacesLabel;
        private Text homeEyebrow, homeProgress, homeProgressSuffix, homeStatus;
        private Image homeDailySurface, homeFill, homeDailyActionSurface;
        private Button homeDailyAction;
        private readonly RectTransform[] homeTiles = new RectTransform[3];
        private readonly RectTransform[] homeTileIcons = new RectTransform[3];
        private readonly Text[] homeTileDetails = new Text[3];
        private readonly RectTransform[] homeMinis = new RectTransform[4];
        private RectTransform homeNext, homeSoon;
        private Text collectionSummary, rewardsSummary;
        private RectTransform rewardNote, badgeEntry;

        private void BuildHome()
        {
            // Today's box card: the box you're walking toward, your steps, and one clear action.
            homeDaily = Rect("Today's little walk", homeRoot); homeDailySurface = Card(homeDaily, Paper);
            homeBoxArt = Rect("Jelly Garden box", homeDaily);
            var art = Resources.Load<Texture2D>("Store/CollectionBoxes");
            if (art != null)
            {
                var image = homeBoxArt.gameObject.AddComponent<RawImage>(); image.texture = art;
                image.uvRect = new Rect(.04f, .09f, .30f, .74f); image.raycastTarget = false;
            }
            else Surface(homeBoxArt, Quiet);
            homeDailyText = Rect("Today's progress", homeDaily);
            homeEyebrow = Label("TODAY'S JELLY GARDEN BOX", homeDailyText, PockleTheme.EyebrowSize, MutedInk, FontStyle.Bold,
                TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 20));
            homeStepIcon = AddIcon(homeDailyText, "Steps", 22, PockleTheme.PeachDeep);
            homeProgress = Label("0", homeDailyText, 26, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(120, 34));
            homeProgress.resizeTextForBestFit = false; homeProgress.horizontalOverflow = HorizontalWrapMode.Overflow;
            homeProgressSuffix = Label("/ 1,000 steps", homeDailyText, 13, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(120, 22));
            homeTrack = Rect("Today's step progress", homeDailyText); Surface(homeTrack, Quiet);
            var fill = Rect("Walk progress fill", homeTrack); Stretch(fill);
            homeFill = fill.gameObject.AddComponent<Image>(); homeFill.sprite = roundedSprite; homeFill.color = PockleTheme.PeachDeep;
            homeFill.type = Image.Type.Filled; homeFill.fillMethod = Image.FillMethod.Horizontal; homeFill.fillOrigin = 0;
            homeFill.raycastTarget = false; homeFill.fillAmount = 0;
            homeStatus = Label("Your daily walk starts here.", homeDailyText, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal,
                TextAnchor.UpperLeft, Vector2.zero, new Vector2(200, 34));
            homeDailyAction = CreateButton("View rewards", homeDaily, Vector2.zero, new Vector2(250, 48), Quiet,
                () => { if (session != null && session.Progress.CanClaim) DailyBoxRequested?.Invoke(); else Navigate(AppPage.Rewards); },
                out homeDailyActionSurface);

            homePlacesLabel = Label("YOUR PLACES", homeRoot, PockleTheme.EyebrowSize, MutedInk, FontStyle.Bold,
                TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 20)).rectTransform;

            string[] names = { "Collection", "Rewards", "Friends" };
            string[] icons = { "Shelf", "Box", "Friends" };
            string[] descriptions = { "Your own shelf of little wonders.", "A daily box, earned one step at a time.", "Discover shelves" };
            AppPage[] destinations = { AppPage.Shelf, AppPage.Rewards, AppPage.Social };
            Color[] colours = { PockleTheme.TilePeach, PockleTheme.TileLavender, PockleTheme.TileMint };
            for (int i = 0; i < homeTiles.Length; i++)
            {
                AppPage destination = destinations[i];
                var button = CreateButton(names[i], homeRoot, Vector2.zero, new Vector2(340, 132), colours[i], () => Navigate(destination),
                    out _, PockleTheme.HeadingSize, false);
                // Tiles float on a soft shadow rather than a button lip.
                var shadow = button.gameObject.AddComponent<Shadow>();
                shadow.effectColor = PockleTheme.CardShadow; shadow.effectDistance = PockleTheme.CardShadowOffset;
                homeTiles[i] = button.GetComponent<RectTransform>();
                var title = button.GetComponentInChildren<Text>(); title.alignment = TextAnchor.MiddleLeft;
                homeTileDetails[i] = Label(descriptions[i], button.transform, 13, MutedInk, FontStyle.Normal,
                    TextAnchor.UpperLeft, Vector2.zero, new Vector2(290, 36));
                if (i == 0)
                {
                    homeTileIcons[i] = AddIcon(button.transform, icons[i], 22, Ink);
                }
                else
                {
                    // Half tiles carry their icon in a little paper bubble.
                    var bubble = Rect(names[i] + " icon bubble", button.transform); Surface(bubble, Paper);
                    var glyph = AddIcon(bubble, icons[i], 24, Ink); glyph.anchoredPosition = Vector2.zero;
                    homeTileIcons[i] = bubble;
                }
            }
            collectionSummary = homeTileDetails[0]; rewardsSummary = homeTileDetails[1];
            for (int i = 0; i < homeMinis.Length; i++)
            {
                // Small rounded portraits of the toys you own, masked to the shared rounded sprite.
                var frame = Rect("Mini portrait", homeTiles[0]); Surface(frame, PockleTheme.ShelfFace);
                frame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                var picture = Rect("Portrait", frame); Stretch(picture);
                var raw = picture.gameObject.AddComponent<RawImage>(); raw.texture = previews[i]; raw.raycastTarget = false;
                homeMinis[i] = frame;
            }
            homeNext = AddIcon(homeTiles[0], "Next", 20, MutedInk);
            homeSoon = Rect("Coming soon", homeTiles[2]); Surface(homeSoon, Paper);
            var soon = Label("Soon", homeSoon, PockleTheme.EyebrowSize, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(soon.rectTransform);
        }

        private float LayoutHome(float width)
        {
            float w = Mathf.Min(width, HomeCardMaxWidth);
            float y = 0;
            // Today's box card.
            const float dailyHeight = 196;
            TopCentered(homeDaily, new Vector2(0, -y), new Vector2(w, dailyHeight));
            float art = Mathf.Clamp(w * .28f, 72, 116);
            LeftLabel(homeBoxArt, new Vector2(12, -14), new Vector2(art, art * 1.13f));
            float textX = 12 + art + 14, textWidth = Mathf.Max(80, w - textX - 14);
            LeftLabel(homeDailyText, new Vector2(textX, -14), new Vector2(textWidth, 120));
            LeftLabel(homeEyebrow.rectTransform, new Vector2(0, -4), new Vector2(textWidth, 20));
            LeftLabel(homeStepIcon, new Vector2(0, -30), new Vector2(22, 22));
            LeftLabel(homeProgress.rectTransform, new Vector2(28, -24), new Vector2(textWidth - 28, 34));
            PlaceProgressSuffix();
            LeftLabel(homeTrack, new Vector2(0, -68), new Vector2(textWidth, 10));
            LeftLabel(homeStatus.rectTransform, new Vector2(0, -86), new Vector2(textWidth, 34));
            LeftLabel(homeDailyAction.GetComponent<RectTransform>(), new Vector2(14, -(dailyHeight - 62)), new Vector2(w - 28, 48));
            y += dailyHeight + 22;
            TopCentered(homePlacesLabel, new Vector2(0, -y), new Vector2(w - 8, 20));
            y += 26;
            // Collection: full width with your toys peeking out.
            const float tileHeight = 132, gap = 12;
            TopCentered(homeTiles[0], new Vector2(0, -y), new Vector2(w, tileHeight));
            LayoutTileText(0, w, 46, -12);
            LeftLabel(homeTileIcons[0], new Vector2(16, -18), new Vector2(22, 22));
            for (int i = 0; i < homeMinis.Length; i++) LeftLabel(homeMinis[i], new Vector2(16 + i * 52, -74), new Vector2(44, 44));
            homeNext.anchorMin = homeNext.anchorMax = new Vector2(1, .5f); homeNext.pivot = new Vector2(1, .5f);
            homeNext.anchoredPosition = new Vector2(-16, 0);
            y += tileHeight + gap;
            // Rewards and Friends side by side; stacked on very narrow screens.
            bool stack = w < 300;
            float half = stack ? w : (w - gap) / 2;
            for (int i = 1; i < 3; i++)
            {
                float x = stack ? 0 : (i - 1) * (half + gap);
                float top = stack ? y + (i - 1) * (tileHeight + gap) : y;
                homeTiles[i].anchorMin = homeTiles[i].anchorMax = new Vector2(.5f, 1); homeTiles[i].pivot = new Vector2(0, 1);
                homeTiles[i].anchoredPosition = new Vector2(-w / 2 + x, -top); homeTiles[i].sizeDelta = new Vector2(half, tileHeight);
                LeftLabel(homeTileIcons[i], new Vector2(14, -14), new Vector2(44, 44));
                LayoutTileText(i, half, 16, -64);
            }
            homeSoon.anchorMin = homeSoon.anchorMax = homeSoon.pivot = new Vector2(1, 1);
            homeSoon.anchoredPosition = new Vector2(-14, -16); homeSoon.sizeDelta = new Vector2(52, 22);
            y += stack ? 2 * tileHeight + gap : tileHeight;
            RefreshHome(); // Re-packs the mini portraits after layout resets their positions.
            TopCentered(homeRoot, Vector2.zero, new Vector2(width, y + 8));
            return y + 8;
        }

        private void LayoutTileText(int index, float width, float left, float top)
        {
            var title = homeTiles[index].GetComponentInChildren<Text>().rectTransform;
            LeftLabel(title, new Vector2(left, top), new Vector2(width - left - 40, 34));
            float detailLeft = index == 0 ? 16 : left;
            LeftLabel(homeTileDetails[index].rectTransform, new Vector2(detailLeft, top - 34), new Vector2(width - detailLeft - 40, 36));
        }

        private void PlaceProgressSuffix()
        {
            float numberWidth = Mathf.Min(homeProgress.preferredWidth, homeProgress.rectTransform.sizeDelta.x);
            float x = 28 + numberWidth + 6;
            float width = Mathf.Max(40, homeDailyText.sizeDelta.x - x);
            LeftLabel(homeProgressSuffix.rectTransform, new Vector2(x, -32), new Vector2(width, 22));
        }

        private void BuildRewards()
        {
            rewardNote = Label("Daily boxes reset at midnight UTC. Walking is counted while Pockle is open; keep it open during your walk for now.",
                rewardsRoot, PockleTheme.BodySize, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, new Vector2(330, 96)).rectTransform;
            badgeEntry = CreateButton("Badges & milestones", rewardsRoot, Vector2.zero, new Vector2(290, 56), Quiet,
                () => ShowDialog("More little milestones", "Badges and milestone rewards are coming later. Your daily walking box is the reward you can earn today."), out _).GetComponent<RectTransform>();
        }

        private float LayoutRewards(float width)
        {
            LayoutWalkingCard(width);
            TopCentered(rewardsRoot, Vector2.zero, new Vector2(width, 468));
            TopCentered(rewardNote, new Vector2(0, -304), new Vector2(Mathf.Min(width - 24, 440), 96));
            TopCentered(badgeEntry, new Vector2(0, -408), new Vector2(Mathf.Min(width, 360), 56));
            return 480;
        }

        private void RefreshHome()
        {
            if (session == null || homeProgress == null) return;
            var saved = session.Progress.Save;
            int target = CollectionProgress.DailyTarget;
            bool today = saved.Day == CollectionSession.Today;
            bool ready = today && session.Progress.CanClaim;
            homeProgress.text = saved.Steps.ToString("N0");
            homeProgressSuffix.text = "/ " + target.ToString("N0") + " steps";
            if (homeDailyText.sizeDelta.x > 0) PlaceProgressSuffix();
            homeFill.fillAmount = Mathf.Clamp01(saved.Steps / (float)target);
            var remaining = System.DateTime.UtcNow.Date.AddDays(1) - System.DateTime.UtcNow;
            string reset = "Next box in " + (int)remaining.TotalHours + "h " + remaining.Minutes + "m (UTC).";
            homeStatus.text = !today ? "Check your device date to continue." : saved.Claimed ? "Opened today. " + reset
                : ready ? "Your little surprise is ready!" : "Just " + (target - saved.Steps).ToString("N0") + " more steps to your box.";
            homeDailyAction.GetComponentInChildren<Text>().text = ready ? "Open your box" : "View rewards";
            // The card warms up and the action turns primary when the box is ready to open.
            homeDailySurface.color = ready ? PockleTheme.TilePeach : Paper;
            homeDailyActionSurface.color = ready ? Peach : Quiet;
            var lip = homeDailyAction.GetComponent<Shadow>();
            if (lip != null) lip.effectColor = ready ? PockleTheme.PeachDeep : PockleTheme.ButtonLip;
            rewardsSummary.text = !today ? "Check your device date." : saved.Claimed ? "Opened today · back tomorrow"
                : ready ? "Your box is ready!" : "Daily box · " + saved.Steps.ToString("N0") + "/" + target.ToString("N0");
            CollectionTotals(out int distinct, out int total);
            int slot = 0;
            for (int i = 0; i < homeMinis.Length; i++)
            {
                // Only toys you own peek out of the Collection tile, packed to the left.
                bool owned = Owned((PipVariant)i) > 0;
                homeMinis[i].gameObject.SetActive(owned);
                if (owned) homeMinis[i].anchoredPosition = new Vector2(16 + slot++ * 52, -74);
            }
            collectionSummary.text = distinct + " of " + PipVariants.Count + " finishes · " + total + (total == 1 ? " toy" : " toys") + " on your shelf";
        }
    }
}
