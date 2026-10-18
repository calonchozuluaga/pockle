using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// The badge cabinet (canvas board "Badges with Meshy art", reached from the You page): a featured badge on a night-sky
    /// card, then Earned in colour and Still to earn greyed with a lock and progress. Tap any badge to feature it.
    /// Rules live in <see cref="BadgeBook"/>; earned dates in <see cref="BadgeCabinet"/>.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private static readonly Color BadgeNight = new Color32(0x3A, 0x2B, 0x6A, 0xFF), BadgeNightGlow = new Color32(0x6A, 0x5C, 0xB8, 0xFF);
        private static readonly Color LockedTint = new Color(.66f, .62f, .7f, .42f);

        private RectTransform badgesRoot, badgeHero, badgeHeroBar, earnedGrid, lockedGrid, earnedHeading, lockedHeading;
        private RawImage badgeHeroArt, badgeHeroGlow;
        private Image badgeHeroFill;
        private Text badgeHeroEyebrow, badgeHeroName, badgeHeroLine, badgeHeroProgress, earnedEmpty, badgesRowDetail, rewardsBadgeDetail;
        private readonly RectTransform[] badgeSparkles = new RectTransform[4];
        private sealed class BadgeTile { public Button Button; public Image Face; public RawImage Art; public Text Name, Detail; public RectTransform Lock; }
        private readonly Dictionary<string, BadgeTile> badgeTiles = new Dictionary<string, BadgeTile>();
        private readonly Dictionary<string, Texture2D> badgeArt = new Dictionary<string, Texture2D>();
        private readonly List<string> earnedOrder = new List<string>(), lockedOrder = new List<string>();
        private BadgeCabinet badgeCabinet;
        private string featuredBadge;
        private bool badgePicked; // "NEXT UP" names the closest badge only until the player taps one to look at

        /// <summary>For tests and the You page: how many badges are earned on this device.</summary>
        public int EarnedBadgeCount => badgeCabinet != null ? badgeCabinet.Count : 0;
        public string FeaturedBadge => featuredBadge;

        private BadgeFacts BadgeFactsNow() => new BadgeFacts
        {
            Owned = session != null ? session.OwnedCounts : new Dictionary<string, int>(),
            Moments = foundMoments,
            Fusions = -1, // fusion is a preview that saves nothing yet (Codex: persistent fusion will report a count)
        };

        private void BuildBadges()
        {
            badgeCabinet = new BadgeCabinet();
            badgeHero = Rect("Featured badge", badgesRoot); Surface(badgeHero, BadgeNight, 30);
            badgeHero.gameObject.AddComponent<RectMask2D>();
            var glow = Rect("Glow", badgeHero); glow.anchorMin = glow.anchorMax = glow.pivot = new Vector2(.5f, 1);
            badgeHeroGlow = glow.gameObject.AddComponent<RawImage>(); badgeHeroGlow.texture = UiShapes.Glow; badgeHeroGlow.color = BadgeNightGlow; badgeHeroGlow.raycastTarget = false;
            for (int i = 0; i < badgeSparkles.Length; i++)
            {
                badgeSparkles[i] = Rect("Twinkle", badgeHero); badgeSparkles[i].anchorMin = badgeSparkles[i].anchorMax = badgeSparkles[i].pivot = new Vector2(.5f, .5f);
                var star = badgeSparkles[i].gameObject.AddComponent<RawImage>(); star.texture = UiShapes.Sparkle; star.raycastTarget = false;
                star.color = i % 2 == 0 ? PockleTheme.FieldGold : new Color(1, 1, 1, .85f);
            }
            badgeHeroEyebrow = Label("", badgeHero, 11, PockleTheme.OnPlumMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            var art = Rect("Badge art", badgeHero); art.anchorMin = art.anchorMax = art.pivot = new Vector2(.5f, 1);
            badgeHeroArt = art.gameObject.AddComponent<RawImage>(); badgeHeroArt.raycastTarget = false;
            badgeHeroName = Label("", badgeHero, 24, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            badgeHeroLine = Label("", badgeHero, PockleTheme.SmallSize, PockleTheme.OnPlumMuted, FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            badgeHeroBar = Rect("Badge progress", badgeHero); Surface(badgeHeroBar, new Color(1, 1, 1, .18f), 4);
            badgeHeroFill = ProgressFill(badgeHeroBar, PockleTheme.FieldGold);
            badgeHeroProgress = Label("", badgeHero, 12, PockleTheme.FieldGold, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);

            earnedHeading = Heading("Earned", badgesRoot);
            earnedGrid = Rect("Earned badges", badgesRoot);
            earnedEmpty = Label("None yet. The one above is closest.", badgesRoot, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            lockedHeading = Heading("Still to earn", badgesRoot);
            lockedGrid = Rect("Badges still to earn", badgesRoot);
            foreach (var badge in BadgeBook.All) badgeTiles[badge.Id] = BuildBadgeTile(badge);
            AwardBadges();
        }

        private BadgeTile BuildBadgeTile(BadgeDefinition badge)
        {
            var tile = new BadgeTile();
            string id = badge.Id;
            tile.Button = CreateButton("", earnedGrid, Vector2.zero, new Vector2(100, 116), new Color(1, 1, 1, 0), () => FeatureBadge(id), out tile.Face, 12, false);
            SetRadius(tile.Face, 18);
            tile.Button.name = "Badge · " + badge.Name;
            tile.Button.GetComponentInChildren<Text>().enabled = false;
            var art = Rect("Art", tile.Button.transform); art.anchorMin = art.anchorMax = art.pivot = new Vector2(.5f, 1);
            art.anchoredPosition = new Vector2(0, -6); art.sizeDelta = new Vector2(76, 76);
            tile.Art = art.gameObject.AddComponent<RawImage>(); tile.Art.raycastTarget = false; tile.Art.enabled = false;
            tile.Lock = Rect("Locked", art); Surface(tile.Lock, Paper, 12);
            tile.Lock.anchorMin = tile.Lock.anchorMax = tile.Lock.pivot = new Vector2(1, 0); tile.Lock.anchoredPosition = new Vector2(-1, 1); tile.Lock.sizeDelta = new Vector2(24, 24);
            AddIcon(tile.Lock, "Lock", 13, MutedInk);
            tile.Name = Label(badge.Name, tile.Button.transform, 12, Ink, FontStyle.Bold, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            tile.Detail = Label("", tile.Button.transform, 11, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            foreach (var text in new[] { tile.Name, tile.Detail })
            {
                var r = text.rectTransform; r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(1, 1); r.pivot = new Vector2(.5f, 1);
            }
            tile.Name.rectTransform.offsetMin = new Vector2(2, -102); tile.Name.rectTransform.offsetMax = new Vector2(-2, -86);
            tile.Detail.rectTransform.offsetMin = new Vector2(2, -118); tile.Detail.rectTransform.offsetMax = new Vector2(-2, -103);
            return tile;
        }

        /// <summary>Awards whatever the local facts now earn. Cheap: fifteen rules over the owned toys.</summary>
        private void AwardBadges()
        {
            if (badgeCabinet == null || session == null || session.IsReadOnlySave) return;
            var fresh = badgeCabinet.Award(BadgeFactsNow(), CollectionSession.Today);
            if (fresh.Count > 0) featuredBadge = null; // the newest badge leads the cabinet next time it opens
        }

        private void FeatureBadge(string id)
        {
            featuredBadge = id; badgePicked = true;
            RefreshBadges(); AdaptLayout();
        }

        private Texture2D BadgeArt(BadgeDefinition badge)
        {
            if (!badgeArt.TryGetValue(badge.Id, out var texture)) badgeArt[badge.Id] = texture = Resources.Load<Texture2D>(badge.Art);
            return texture;
        }

        private void RefreshBadges()
        {
            if (badgesRoot == null || badgeCabinet == null) return;
            AwardBadges();
            int total = BadgeBook.All.Count;
            string count = badgeCabinet.Count + " of " + total + " earned";
            if (badgesRowDetail != null) badgesRowDetail.text = count;
            if (rewardsBadgeDetail != null) rewardsBadgeDetail.text = count;
            if (page == AppPage.Badges) subtitle.text = count;
            if (page != AppPage.Badges) return; // art loads only while the cabinet is open

            var facts = BadgeFactsNow();
            earnedOrder.Clear(); earnedOrder.AddRange(badgeCabinet.Newest());
            lockedOrder.Clear();
            string closest = null; float closestFraction = -1;
            foreach (var badge in BadgeBook.All)
            {
                if (badgeCabinet.Has(badge.Id)) continue;
                lockedOrder.Add(badge.Id);
                var progress = badge.Progress(facts);
                if (progress.Tracked && progress.Fraction > closestFraction) { closest = badge.Id; closestFraction = progress.Fraction; }
            }
            if (featuredBadge == null || !BadgeBook.TryGet(featuredBadge, out _))
                featuredBadge = badgeCabinet.Latest ?? closest ?? (lockedOrder.Count > 0 ? lockedOrder[0] : null);

            foreach (var badge in BadgeBook.All)
            {
                var tile = badgeTiles[badge.Id];
                bool earned = badgeCabinet.TryGetDay(badge.Id, out int day);
                tile.Button.transform.SetParent(earned ? earnedGrid : lockedGrid, false);
                tile.Art.texture = BadgeArt(badge); tile.Art.enabled = tile.Art.texture != null;
                tile.Art.color = earned ? Color.white : LockedTint;
                tile.Lock.gameObject.SetActive(!earned);
                tile.Name.color = earned ? Ink : MutedInk;
                tile.Detail.text = earned ? BadgeBook.DateLabel(day) : badge.Progress(facts).Text;
                tile.Detail.color = earned ? MutedInk : new Color(MutedInk.r, MutedInk.g, MutedInk.b, .8f);
                tile.Face.color = badge.Id == featuredBadge ? Quiet : new Color(1, 1, 1, 0);
                tile.Button.name = "Badge · " + badge.Name + (earned ? " · earned" : " · locked");
            }
            earnedEmpty.gameObject.SetActive(earnedOrder.Count == 0);
            lockedHeading.gameObject.SetActive(lockedOrder.Count > 0);

            // The featured card.
            if (featuredBadge != null && BadgeBook.TryGet(featuredBadge, out var featured))
            {
                bool earned = badgeCabinet.TryGetDay(featured.Id, out int day);
                var progress = featured.Progress(facts);
                badgeHeroArt.texture = BadgeArt(featured); badgeHeroArt.enabled = badgeHeroArt.texture != null;
                badgeHeroArt.color = earned ? Color.white : new Color(.75f, .72f, .8f, .55f);
                badgeHeroEyebrow.text = earned
                    ? (featured.Id == badgeCabinet.Latest ? "LATEST" : "EARNED") + " · " + BadgeBook.DateLabel(day).ToUpperInvariant()
                    : badgePicked ? "STILL TO EARN" : "NEXT UP";
                badgeHeroName.text = featured.Name;
                badgeHeroLine.text = earned ? featured.Earned : featured.HowTo;
                bool bar = !earned && progress.Tracked;
                badgeHeroBar.gameObject.SetActive(bar);
                badgeHeroFill.fillAmount = progress.Fraction;
                badgeHeroProgress.gameObject.SetActive(!earned);
                badgeHeroProgress.text = progress.Text;
                foreach (var star in badgeSparkles) star.gameObject.SetActive(earned);
            }
        }

        /// <summary>Opens the cabinet on the newest badge (or, with none yet, the closest one).</summary>
        private void OpenBadges()
        {
            featuredBadge = null; badgePicked = false;
            Navigate(AppPage.Badges);
        }

        private float LayoutBadges(float width)
        {
            float w = Mathf.Min(width, 480), left = (width - w) / 2, y = 0;
            const float hero = 304;
            LeftLabel(badgeHero, new Vector2(left, -y), new Vector2(w, hero));
            badgeHeroGlow.rectTransform.anchoredPosition = new Vector2(0, 40); badgeHeroGlow.rectTransform.sizeDelta = new Vector2(w * 1.1f, 300);
            var spots = new[] { new Vector2(-w / 2 + 34, -34), new Vector2(w / 2 - 40, -48), new Vector2(w / 2 - 58, -156), new Vector2(-w / 2 + 46, -166) };
            for (int i = 0; i < badgeSparkles.Length; i++)
            {
                badgeSparkles[i].anchorMin = badgeSparkles[i].anchorMax = new Vector2(.5f, 1);
                badgeSparkles[i].anchoredPosition = spots[i]; float s = 14 - i * 1.5f; badgeSparkles[i].sizeDelta = new Vector2(s, s);
            }
            LeftLabel(badgeHeroEyebrow.rectTransform, new Vector2(16, -18), new Vector2(w - 32, 16));
            badgeHeroArt.rectTransform.anchoredPosition = new Vector2(0, -40); badgeHeroArt.rectTransform.sizeDelta = new Vector2(150, 150);
            LeftLabel(badgeHeroName.rectTransform, new Vector2(16, -194), new Vector2(w - 32, 30));
            LeftLabel(badgeHeroLine.rectTransform, new Vector2(24, -226), new Vector2(w - 48, 38));
            float barWidth = Mathf.Min(200, w - 80);
            LeftLabel(badgeHeroBar, new Vector2((w - barWidth) / 2, -268), new Vector2(barWidth, 8));
            LeftLabel(badgeHeroProgress.rectTransform, new Vector2(16, badgeHeroBar.gameObject.activeSelf ? -278 : -266), new Vector2(w - 32, 18));
            y += hero + 22;

            LeftLabel(earnedHeading, new Vector2(left, -y), new Vector2(w, 30)); y += 40;
            if (earnedOrder.Count == 0) { LeftLabel(earnedEmpty.rectTransform, new Vector2(left, -y), new Vector2(w, 22)); y += 30; }
            y = LayoutBadgeGrid(earnedGrid, earnedOrder, left, y, w);
            if (lockedOrder.Count > 0)
            {
                y += 10;
                LeftLabel(lockedHeading, new Vector2(left, -y), new Vector2(w, 30)); y += 40;
                y = LayoutBadgeGrid(lockedGrid, lockedOrder, left, y, w);
            }
            TopCentered(badgesRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        private float LayoutBadgeGrid(RectTransform grid, List<string> ids, float left, float y, float w)
        {
            const int columns = 3; const float gap = 6, tileHeight = 120;
            float tile = (w - gap * (columns - 1)) / columns;
            LeftLabel(grid, new Vector2(left, -y), new Vector2(w, 0));
            for (int i = 0; i < ids.Count; i++)
                LeftLabel(badgeTiles[ids[i]].Button.GetComponent<RectTransform>(), new Vector2((i % columns) * (tile + gap), -(i / columns) * (tileHeight + 8)), new Vector2(tile, tileHeight));
            int rows = (ids.Count + columns - 1) / columns;
            return y + rows * (tileHeight + 8) + 4;
        }
    }
}
