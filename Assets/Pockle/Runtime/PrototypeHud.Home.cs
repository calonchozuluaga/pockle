using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private RectTransform homeDaily;
        private Text homeProgress, homeStatus;
        private Image homeFill;
        private Button homeDailyAction;
        private readonly RectTransform[] homeTiles = new RectTransform[3];
        private Text collectionSummary;
        private RectTransform rewardNote, badgeEntry;

        private void BuildHome()
        {
            homeDaily = Rect("Today's little walk", homeRoot); Card(homeDaily, Paper);
            Label("TODAY'S JELLY GARDEN BOX", homeDaily, PockleTheme.EyebrowSize, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -14), new Vector2(290, 22));
            homeProgress = Label("0 / 1,000 steps", homeDaily, 23, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -42), new Vector2(290, 30));
            var track = Rect("Today's step progress", homeDaily); TopCentered(track, new Vector2(0, -82), new Vector2(250, 8)); Surface(track, Quiet);
            var fill = Rect("Walk progress fill", track); Stretch(fill);
            homeFill = fill.gameObject.AddComponent<Image>(); homeFill.sprite = roundedSprite; homeFill.color = Peach;
            homeFill.type = Image.Type.Filled; homeFill.fillMethod = Image.FillMethod.Horizontal; homeFill.fillOrigin = 0;
            homeFill.raycastTarget = false; homeFill.fillAmount = 0;
            homeStatus = Label("Your daily walk starts here.", homeDaily, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0, -98), new Vector2(290, 34));
            homeDailyAction = CreateButton("View rewards", homeDaily, Vector2.zero, new Vector2(250, 48), Peach,
                () => { if (session != null && session.Progress.CanClaim) DailyBoxRequested?.Invoke(); else Navigate(AppPage.Rewards); }, out _);
            TopCentered(homeDailyAction.GetComponent<RectTransform>(), new Vector2(0, -140), new Vector2(250, 48));
            string[] names = { "Collection", "Rewards", "Friends" };
            string[] descriptions = { "Your own shelf of little wonders.", "A daily box, earned one step at a time.", "Discover shelves and meet collectors. Coming soon." };
            AppPage[] destinations = { AppPage.Shelf, AppPage.Rewards, AppPage.Social };
            Color[] colours = { PockleTheme.TilePeach, PockleTheme.TileLavender, PockleTheme.TileMint };
            for (int i = 0; i < homeTiles.Length; i++)
            {
                AppPage destination = destinations[i];
                var button = CreateButton(names[i], homeRoot, Vector2.zero, new Vector2(340, 102), colours[i], () => Navigate(destination), out _, 19);
                homeTiles[i] = button.GetComponent<RectTransform>();
                var title = button.GetComponentInChildren<Text>();
                LeftLabel(title.rectTransform, new Vector2(20, -10), new Vector2(290, 34)); title.alignment = TextAnchor.MiddleLeft;
                var detail = Label(descriptions[i], button.transform, 13, MutedInk, FontStyle.Normal,
                    TextAnchor.UpperLeft, new Vector2(0, -50), new Vector2(290, 42));
                if (i == 0) collectionSummary = detail;
            }
        }
        private float LayoutHome(float width)
        {
            float cardWidth = Mathf.Min(width, 480);
            TopCentered(homeRoot, Vector2.zero, new Vector2(width, 570));
            TopCentered(homeDaily, Vector2.zero, new Vector2(cardWidth, 206));
            for (int i = 0; i < homeTiles.Length; i++)
            {
                TopCentered(homeTiles[i], new Vector2(0, -224 - i * 116), new Vector2(cardWidth, 102));
                var labels = homeTiles[i].GetComponentsInChildren<Text>();
                LeftLabel(labels[0].rectTransform, new Vector2(20, -10), new Vector2(cardWidth - 40, 34));
                TopCentered(labels[1].rectTransform, new Vector2(0, -50), new Vector2(cardWidth - 40, 42));
            }
            return 570;
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
            if (session == null) return;
            var saved = session.Progress.Save;
            homeProgress.text = saved.Steps.ToString("N0") + " / 1,000 steps";
            homeFill.fillAmount = saved.Steps / 1000f;
            var remaining = System.DateTime.UtcNow.Date.AddDays(1) - System.DateTime.UtcNow;
            string reset = "Next box in " + (int)remaining.TotalHours + "h " + remaining.Minutes + "m (UTC).";
            homeStatus.text = saved.Day != CollectionSession.Today ? "Check your device date to continue." : saved.Claimed ? reset
                : session.Progress.CanClaim ? "Your little surprise is ready to open!" : "Just " + (1000 - saved.Steps).ToString("N0") + " more steps to your box.";
            homeDailyAction.GetComponentInChildren<Text>().text = session.Progress.CanClaim ? "Open your box" : "View rewards";
            int distinct = 0, total = 0;
            foreach (int count in saved.Counts) { if (count > 0) distinct++; total += count; }
            collectionSummary.text = distinct + " finishes · " + total + " toys on your shelf.";
        }
    }
}
