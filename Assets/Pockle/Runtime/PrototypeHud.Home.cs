using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// v2 Home (UX-032): greeting, today's box as a large jelly-pink card, a strip of your toys, and
    /// Rewards / Friends rows. Matches "Main" in the owner-approved "Pockle screens v2" canvas.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private const float HomeMaxWidth = 480f;
        private RectTransform homeBox, homeBoxArt, homeShelfHeader, homeSteps, homeTrack, homeRows;
        private Text homeBoxLabel, homeStepCount, homeStepSuffix, homeStatus, homeShelfTitle;
        private Image homeFill;
        private Button homeOpen, homeSeeAll;
        private readonly RectTransform[] homeToys = new RectTransform[4];
        private RectTransform homeEmptyShelf;
        private readonly Button[] homeRowButtons = new Button[2];
        private Text rewardsSummary, friendsSummary;
        private RectTransform rewardNote, badgeEntry;

        private void BuildHome()
        {
            // Today's box: the box you're walking toward, and either your steps or one clear action.
            homeBox = Rect("Today's box", homeRoot); Surface(homeBox, PockleTheme.Jelly, 32);
            homeBoxLabel = Label("Today's Jelly Garden box", homeBox, PockleTheme.CaptionSize, PockleTheme.JellyInk, FontStyle.Bold,
                TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 22));
            homeBoxArt = BoxArt(homeBox, 0);
            homeBoxArt.localEulerAngles = new Vector3(0, 0, 5);
            homeOpen = CreateButton("Open your box", homeBox, Vector2.zero, new Vector2(300, 56), PockleTheme.Plum,
                () => { if (session != null && session.Progress.CanClaim) DailyBoxRequested?.Invoke(); }, out _, 18);
            homeSteps = Rect("Today's steps", homeBox);
            homeStepCount = Label("0", homeSteps, 30, Ink, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, new Vector2(140, 38));
            homeStepCount.resizeTextForBestFit = false; homeStepCount.horizontalOverflow = HorizontalWrapMode.Overflow;
            homeStepSuffix = Label("of 1,000 steps", homeSteps, PockleTheme.CaptionSize + 1, PockleTheme.JellyInk, FontStyle.Normal,
                TextAnchor.LowerLeft, Vector2.zero, new Vector2(160, 24));
            homeTrack = Rect("Today's step progress", homeSteps); Surface(homeTrack, new Color(1, 1, 1, .6f), 5);
            homeFill = ProgressFill(homeTrack, Ink);
            homeStatus = Label("", homeBox, PockleTheme.BodySize, PockleTheme.JellyInk, FontStyle.Normal, TextAnchor.LowerLeft,
                Vector2.zero, new Vector2(300, 44));

            // Your shelf: a heading, "See all", and the toys you own.
            homeShelfHeader = Rect("Your shelf heading", homeRoot);
            homeShelfTitle = Label("Your shelf", homeShelfHeader, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(200, 30));
            homeSeeAll = CreateButton("See all", homeShelfHeader, Vector2.zero, new Vector2(90, 44), Color.clear, () => Navigate(AppPage.Shelf), out _, 15);
            var seeAll = homeSeeAll.GetComponentInChildren<Text>(); seeAll.color = MutedInk; seeAll.alignment = TextAnchor.MiddleRight;
            for (int i = 0; i < homeToys.Length; i++)
            {
                PipVariant choice = (PipVariant)i;
                var button = CreateButton(ToyName(choice), homeRoot, Vector2.zero, new Vector2(104, 120),
                    PockleTheme.FieldFor(PipVariants.CollectibleId(choice)), () => ShowToy(choice), out _, 13, false);
                button.gameObject.name = PipVariants.Label(choice) + " · home toy";
                button.GetComponentInChildren<Text>().enabled = false; // The portrait is the label; the name stays for accessibility.
                SetRadius(button.GetComponent<Image>(), PockleTheme.TileRadius);
                var picture = Portrait(button.transform, i); picture.rectTransform.anchorMin = picture.rectTransform.anchorMax = new Vector2(.5f, .5f);
                homeToys[i] = button.GetComponent<RectTransform>();
            }
            homeEmptyShelf = Rect("Empty shelf", homeRoot); Surface(homeEmptyShelf, Quiet, PockleTheme.TileRadius);
            var empty = Label("Your first toy will sit here. Walk or open a box to find one.", homeEmptyShelf, PockleTheme.CaptionSize, MutedInk,
                FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(empty.rectTransform); empty.rectTransform.offsetMin = new Vector2(16, 6); empty.rectTransform.offsetMax = new Vector2(-16, -6);

            // Two quiet rows: Rewards and Friends.
            homeRows = Rect("Places", homeRoot);
            homeRowButtons[0] = RowButton("Rewards", "Badges soon", "Box", homeRows, () => Navigate(AppPage.Rewards), out rewardsSummary, false);
            homeRowButtons[1] = RowButton("Friends", "Visit shelves", "Friends", homeRows, () => Navigate(AppPage.Social), out friendsSummary, false);
        }

        private float LayoutHome(float width)
        {
            float w = Mathf.Min(width, HomeMaxWidth);
            float left = (width - w) / 2;
            float y = 4;
            // Today's box card: label top-left, box centred, action or steps along the bottom.
            float boxHeight = Mathf.Clamp(w * .78f, 250, 300);
            LeftLabel(homeBox, new Vector2(left, -y), new Vector2(w, boxHeight));
            LeftLabel(homeBoxLabel.rectTransform, new Vector2(22, -18), new Vector2(w - 44, 22));
            float art = Mathf.Min(188, boxHeight - 120);
            homeBoxArt.anchorMin = homeBoxArt.anchorMax = homeBoxArt.pivot = new Vector2(.5f, 1);
            homeBoxArt.anchoredPosition = new Vector2(0, -42); homeBoxArt.sizeDelta = new Vector2(art * .85f, art);
            var open = homeOpen.GetComponent<RectTransform>();
            open.anchorMin = open.anchorMax = open.pivot = new Vector2(.5f, 0);
            open.anchoredPosition = new Vector2(0, 20); open.sizeDelta = new Vector2(w - 44, 56);
            homeSteps.anchorMin = homeSteps.anchorMax = homeSteps.pivot = new Vector2(.5f, 0);
            homeSteps.anchoredPosition = new Vector2(0, 20); homeSteps.sizeDelta = new Vector2(w - 44, 60);
            LeftLabel(homeStepCount.rectTransform, Vector2.zero, new Vector2(w - 44, 38));
            PlaceStepSuffix();
            LeftLabel(homeTrack, new Vector2(0, -50), new Vector2(w - 44, 10));
            homeStatus.rectTransform.anchorMin = homeStatus.rectTransform.anchorMax = homeStatus.rectTransform.pivot = new Vector2(0, 0);
            homeStatus.rectTransform.anchoredPosition = new Vector2(22, 20); homeStatus.rectTransform.sizeDelta = new Vector2(w - 44, 44);
            y += boxHeight + 18;

            // Your shelf.
            LeftLabel(homeShelfHeader, new Vector2(left, -y), new Vector2(w, 44));
            LeftLabel(homeShelfTitle.rectTransform, new Vector2(0, -7), new Vector2(w - 100, 30));
            var see = homeSeeAll.GetComponent<RectTransform>(); see.anchorMin = see.anchorMax = see.pivot = new Vector2(1, 1);
            see.anchoredPosition = Vector2.zero; see.sizeDelta = new Vector2(100, 44);
            y += 50;
            const float gap = 10;
            float tile = Mathf.Clamp((w - gap * 3) / 4, 72, 120), tileHeight = Mathf.Round(tile * 1.15f);
            for (int i = 0; i < homeToys.Length; i++)
            {
                LeftLabel(homeToys[i], new Vector2(left + i * (tile + gap), -y), new Vector2(tile, tileHeight));
                var picture = homeToys[i].GetComponentInChildren<RawImage>().rectTransform;
                picture.anchoredPosition = Vector2.zero; picture.sizeDelta = new Vector2(tile * .8f, tile * .8f);
            }
            LeftLabel(homeEmptyShelf, new Vector2(left, -y), new Vector2(w, tileHeight));
            y += tileHeight + 14;

            // Rewards and Friends, side by side (stacked when narrow).
            bool stack = w < 300;
            float half = stack ? w : (w - gap) / 2;
            LeftLabel(homeRows, new Vector2(left, -y), new Vector2(w, stack ? 130 : 60));
            for (int i = 0; i < 2; i++)
                LeftLabel(homeRowButtons[i].GetComponent<RectTransform>(), stack ? new Vector2(0, -i * 70) : new Vector2(i * (half + gap), 0), new Vector2(half, 60));
            y += stack ? 130 : 60;
            RefreshHome(); // Re-packs the shelf strip after layout resets positions.
            TopCentered(homeRoot, Vector2.zero, new Vector2(width, y + 8));
            return y + 8;
        }

        private void PlaceStepSuffix()
        {
            float x = Mathf.Min(homeStepCount.preferredWidth, homeSteps.sizeDelta.x) + 6;
            LeftLabel(homeStepSuffix.rectTransform, new Vector2(x, -12), new Vector2(Mathf.Max(40, homeSteps.sizeDelta.x - x), 24));
        }

        /// <summary>The line under the Home greeting: your collection at a glance.</summary>
        private string HomeLine()
        {
            if (session == null) return "Your little shelf is waiting.";
            CollectionTotals(out int distinct, out int total);
            return distinct + " of " + PipVariants.Count + " finishes · " + total + (total == 1 ? " toy" : " toys") + " on your shelf";
        }

        private void BuildRewards()
        {
            rewardNote = Label("Daily boxes reset at midnight UTC. Steps count while Pockle is open; keep it open on your walk for now.",
                rewardsRoot, PockleTheme.BodySize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(330, 72)).rectTransform;
            badgeEntry = RowButton("Badges and milestones", "Coming later", "Heart", rewardsRoot,
                () => ShowDialog("More little milestones", "Badges and milestone rewards are coming later. Your daily walking box is the reward you can earn today."),
                out _, true).GetComponent<RectTransform>();
        }

        private float LayoutRewards(float width)
        {
            float w = Mathf.Min(width, HomeMaxWidth);
            float y = LayoutWalkingCard(width);
            TopCentered(rewardNote, new Vector2(0, -y - 16), new Vector2(w - 8, 72));
            TopCentered(badgeEntry, new Vector2(0, -y - 96), new Vector2(w, 60));
            float height = y + 96 + 60;
            TopCentered(rewardsRoot, Vector2.zero, new Vector2(width, height));
            return height;
        }

        private void RefreshHome()
        {
            if (session == null || homeStepCount == null) return;
            var saved = session.Progress.Save;
            int target = CollectionProgress.DailyTarget;
            bool today = saved.Day == CollectionSession.Today;
            bool ready = today && session.Progress.CanClaim;
            bool walking = today && !ready && !saved.Claimed;
            homeOpen.gameObject.SetActive(ready);
            homeOpen.interactable = ready;
            homeSteps.gameObject.SetActive(walking);
            homeStatus.gameObject.SetActive(!ready && !walking);
            homeStepCount.text = saved.Steps.ToString("N0");
            homeStepSuffix.text = "of " + target.ToString("N0") + " steps";
            if (homeSteps.sizeDelta.x > 0) PlaceStepSuffix();
            homeFill.fillAmount = Mathf.Clamp01(saved.Steps / (float)target);
            homeStatus.text = !today ? "Check your device date to keep walking." : "Opened today. " + NextBoxIn();
            rewardsSummary.text = !today ? "Check your device date" : saved.Claimed ? "Back tomorrow"
                : ready ? "Your box is ready!" : saved.Steps.ToString("N0") + " of " + target.ToString("N0") + " steps";

            // Only toys you own sit on the Home strip, packed to the left.
            int slot = 0;
            float step = homeToys[0].sizeDelta.x + 10, x0 = homeEmptyShelf.anchoredPosition.x;
            for (int i = 0; i < homeToys.Length; i++)
            {
                bool owned = Owned((PipVariant)i) > 0;
                homeToys[i].gameObject.SetActive(owned);
                if (owned) homeToys[i].anchoredPosition = new Vector2(x0 + slot++ * step, homeToys[i].anchoredPosition.y);
            }
            homeEmptyShelf.gameObject.SetActive(slot == 0);
            homeSeeAll.gameObject.SetActive(slot > 0);
            if (page == AppPage.Home && subtitle != null) subtitle.text = HomeLine();
        }

        private static string NextBoxIn()
        {
            var remaining = System.DateTime.UtcNow.Date.AddDays(1) - System.DateTime.UtcNow;
            return "Next box in " + (int)remaining.TotalHours + "h " + remaining.Minutes + "m.";
        }
    }
}
