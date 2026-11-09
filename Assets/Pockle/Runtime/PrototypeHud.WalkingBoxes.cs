using System;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// UX-052 server walking boxes on Home and Boxes (owner, November 8): 1,000 steps earns a box, one per hour,
    /// up to four a day, and up to two wait unopened. Shows the banked boxes with their server-saved collection (no picking,
    /// no reroll), today's count, step progress, a server-based countdown, and offline/reconnecting states in which saved
    /// boxes are shown but can't be opened. With no server snapshot (signed out, test builds) the local beta card is
    /// unchanged. See <see cref="WalkingBoxView"/>.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private WalkingBoxView boxView;
        private float boxViewAt;
        private RectTransform homeBankRow, walkRules;
        private readonly RawImage[] homeBankSlots = new RawImage[WalkingBoxView.MaxBankedDefault];
        private readonly Image[] homeDayDots = new Image[WalkingBoxView.MaxPerDayDefault];
        private Text homeBankCaption, walkRulesText, walkMystery;
        private CountdownText homeStatusClock, walkStatusClock;
        private Button walkOdds;
        private Texture localWalkTexture;
        private Rect localWalkUv;

        /// <summary>Puts the local beta box picture back after server mode ends.</summary>
        private void RestoreLocalWalkArt()
        {
            if (walkStatusClock != null) walkStatusClock.enabled = false;
            if (walkMystery != null) walkMystery.gameObject.SetActive(false);
            if (rewardNote != null && homeBankRow != null)
                rewardNote.GetComponent<Text>().text = "Daily boxes reset at midnight UTC. Steps count while Pockle is open; keep it open on your walk for now.";
            if (localWalkTexture == null || walkArt == null) return;
            var art = walkArt.GetComponent<RawImage>(); art.texture = localWalkTexture; art.uvRect = localWalkUv; art.color = Color.white; art.enabled = true;
        }

        /// <summary>True when a server snapshot drives the box cards.</summary>
        public bool ServerBoxes => boxView != null;

        /// <summary>Codex: push the latest server state (and again on every change). The HUD keeps no rules of its own.</summary>
        public void SetWalkingBoxes(WalkingBoxView view)
        {
            bool modeChanged = (boxView == null) != (view == null);
            boxView = view; boxViewAt = Time.realtimeSinceStartup;
            EnsureServerBoxParts();
            RefreshHome(); RefreshWalkingCard();
            if (modeChanged) AdaptLayout();
        }

        /// <summary>Back to the local beta card (signed out, test builds).</summary>
        public void ClearWalkingBoxes() => SetWalkingBoxes(null);

        /// <summary>Server seconds left, ticked down locally since the snapshot arrived; null once it reaches zero (waiting
        /// for the server to confirm) or when nothing is counting down.</summary>
        private TimeSpan? ServerTimeLeft()
        {
            if (boxView?.SecondsUntilNextBox == null) return null;
            double left = boxView.SecondsUntilNextBox.Value - (Time.realtimeSinceStartup - boxViewAt);
            return left > 0 ? TimeSpan.FromSeconds(left) : (TimeSpan?)null;
        }

        private static string BoxName(string collectionId)
        {
            return string.IsNullOrEmpty(collectionId) ? "Mystery box"
                : (ToyCatalog.TryGetCollection(collectionId, out var c) ? c.DisplayName : collectionId) + " box";
        }

        private static Texture BoxPicture(string collectionId) => string.IsNullOrEmpty(collectionId) ? null : CollectionLook.Box(collectionId, out _);

        private static string WalkingRuleFrequency(WalkingBoxView view)
        {
            int hours = Mathf.Max(1, view?.CooldownHours ?? WalkingBoxView.CooldownHoursDefault);
            return hours == 1 ? "one every hour" : "one every " + hours + " hours";
        }

        private static string WalkingRuleSummary(WalkingBoxView view) => "Every "
            + (view?.StepsPerBox ?? WalkingBoxView.StepsPerBoxDefault).ToString("N0") + " steps earns a box, "
            + WalkingRuleFrequency(view) + ", up to " + (view?.MaxPerDay ?? WalkingBoxView.MaxPerDayDefault)
            + " a day. Up to " + (view?.MaxBanked ?? WalkingBoxView.MaxBankedDefault) + " can wait for you";

        /// <summary>The one status line both cards share, highest priority first.</summary>
        private string ServerBoxStatus(out bool counting, bool compact = false)
        {
            var v = boxView; counting = false;
            if (v.Connection == WalkingBoxConnection.Offline)
                return compact ? "Offline · boxes open when you're back online" : v.Banked > 0 ? "Offline. Your boxes are saved and open when you're back online." : "Offline. Steps keep counting on your phone.";
            if (v.Connection == WalkingBoxConnection.Reconnecting) return "Reconnecting…";
            if (!string.IsNullOrEmpty(v.Message)) return v.Message;
            if (v.Busy) return "Opening your box…";
            if (v.BankFull) return compact ? "Bank full · open a box to keep earning" : "Your bank is full. Open a box to keep earning.";
            if (v.DayDone) return compact ? "All " + v.MaxPerDay + " earned today · more tomorrow" : "That's all " + v.MaxPerDay + " boxes for today. More tomorrow!";
            if (v.SecondsUntilNextBox.HasValue) { counting = true; return "Next box in"; }
            int left = Mathf.Max(0, v.StepsPerBox - v.StepCredit);
            return left > 0 ? left.ToString("N0") + " more steps to earn a box." : "Box earned! Saving it…";
        }

        private void OpenWalkingBox()
        {
            if (ServerBoxes) { if (boxView.OpenEnabled) DailyBoxRequested?.Invoke(); return; }
            if (session != null && session.Progress.CanClaim) DailyBoxRequested?.Invoke();
        }

        private void ShowBoxRules()
        {
            var v = boxView;
            string odds = v?.Odds == null || v.Odds.Count == 0 ? "The odds for each box are shown here before you open it."
                : "Odds for this box: " + string.Join(", ", System.Linq.Enumerable.Select(v.Odds, o => RarityLook.Words(o.Tier) is var w && w.Length > 0 ? w + " " + TierOdds.Format(o.Chance) : "Common " + TierOdds.Format(o.Chance))) + ".";
            ShowDialog("Walking boxes", WalkingRuleSummary(v)
                + ", so you don't have to open them right away. Each box's collection is picked when you earn it.\n\n" + odds);
        }

        // ----- Build (once, on the first snapshot) -----

        private void EnsureServerBoxParts()
        {
            if (homeBankRow != null || homeBox == null) return;
            homeBankRow = Rect("Banked boxes", homeBox);
            for (int i = 0; i < homeBankSlots.Length; i++)
            {
                var slot = Rect("Bank slot " + (i + 1), homeBankRow); slot.anchorMin = slot.anchorMax = slot.pivot = new Vector2(0, .5f);
                slot.anchoredPosition = new Vector2(i * 34, 0); slot.sizeDelta = new Vector2(30, 30);
                Surface(slot, new Color(1, 1, 1, .55f), 9);
                var art = Rect("Box", slot); Stretch(art); art.offsetMin = new Vector2(2, 2); art.offsetMax = new Vector2(-2, -2);
                homeBankSlots[i] = art.gameObject.AddComponent<RawImage>(); homeBankSlots[i].raycastTarget = false;
            }
            for (int i = 0; i < homeDayDots.Length; i++)
            {
                var dot = Rect("Today " + (i + 1), homeBankRow); dot.anchorMin = dot.anchorMax = dot.pivot = new Vector2(0, .5f);
                dot.anchoredPosition = new Vector2(124 + i * 14, 0); dot.sizeDelta = new Vector2(9, 9); homeDayDots[i] = Surface(dot, Ink, 4.5f);
            }
            // The header already says how many boxes are ready, so the row only names the dots.
            homeBankCaption = Label("Today", homeBankRow, PockleTheme.SmallSize, PockleTheme.JellyInk, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            homeBankCaption.rectTransform.anchorMin = homeBankCaption.rectTransform.anchorMax = homeBankCaption.rectTransform.pivot = new Vector2(0, .5f);
            homeBankCaption.rectTransform.anchoredPosition = new Vector2(78, 0); homeBankCaption.rectTransform.sizeDelta = new Vector2(44, 20);
            homeStatusClock = homeStatus.gameObject.AddComponent<CountdownText>(); homeStatusClock.Target = homeStatus; homeStatusClock.Source = ServerTimeLeft;
            homeStatusClock.Prefix = "Next box in "; homeStatusClock.enabled = false;

            walkStatusClock = walkStatus.gameObject.AddComponent<CountdownText>(); walkStatusClock.Target = walkStatus; walkStatusClock.Source = ServerTimeLeft;
            walkStatusClock.Prefix = "Next box in "; walkStatusClock.enabled = false;
            walkRules = Rect("Walking box rules", boxesRoot);
            walkRulesText = Label("", walkRules, PockleTheme.SmallSize, MutedInk,
                FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            Place(walkRulesText.rectTransform, 0, 0, 1, 1, 0, 0, 96, 0);
            walkOdds = CreateButton("Rules & odds", walkRules, Vector2.zero, new Vector2(92, 32), Quiet, ShowBoxRules, out _, PockleTheme.SmallSize);
            var odds = walkOdds.GetComponent<RectTransform>(); odds.anchorMin = odds.anchorMax = odds.pivot = new Vector2(1, .5f); odds.anchoredPosition = Vector2.zero;
            // The Home and Boxes open buttons both go through OpenWalkingBox (server rules when a snapshot exists).
            homeOpen.onClick.RemoveAllListeners(); homeOpen.onClick.AddListener(OpenWalkingBox);
            homeWalk.onClick.RemoveAllListeners();
            homeWalk.onClick.AddListener(() => { if (ServerBoxes && (boxView.BankFull || boxView.DayDone)) ShowBoxRules(); else Navigate(AppPage.Rewards); });
            var localArt = walkArt.GetComponent<RawImage>(); localWalkTexture = localArt.texture; localWalkUv = localArt.uvRect;
            walkMystery = Label("?", walkArt, 44, PockleTheme.JellyInk, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            walkMystery.gameObject.name = "Unknown walking collection";
            Stretch(walkMystery.rectTransform); walkMystery.raycastTarget = false;
            openDaily.onClick.RemoveAllListeners(); openDaily.onClick.AddListener(OpenWalkingBox);
        }

        // ----- Refresh -----

        /// <summary>Home's top card in server mode.</summary>
        private void RefreshHomeServerBox()
        {
            var v = boxView;
            string first = v.Banked > 0 ? v.BankedCollectionIds[0] : null;
            string shown = first ?? v.NextCollectionId;
            bool online = v.Online;
            homeBoxArt.texture = BoxPicture(shown); homeBoxArt.enabled = homeBoxArt.texture != null;
            homeBoxArt.color = first != null ? Color.white : new Color(1, 1, 1, .45f); // a box still to earn reads as a promise
            homeMystery.gameObject.SetActive(first == null && v.NextCollectionId == null);
            homeMystery.color = new Color(PockleTheme.JellyInk.r, PockleTheme.JellyInk.g, PockleTheme.JellyInk.b, .5f); // readable on pink
            homeCountdown.gameObject.SetActive(false);
            homeBoxLabel.text = v.Banked == 0 ? "WALKING BOX" : v.Banked == 1 ? "1 BOX READY" : v.Banked + " BOXES READY";
            homeBoxTitle.text = first != null ? BoxName(first) : v.NextCollectionId != null ? "Next: " + BoxName(v.NextCollectionId) : "Your next box";
            homeBoxFace.color = !online ? Color.Lerp(PockleTheme.Fill, PockleTheme.Jelly, .25f) : v.OpenEnabled ? PockleTheme.Jelly : Color.Lerp(PockleTheme.Jelly, Color.white, .3f);
            homeBoxLabel.color = homeStatus.color = PockleTheme.JellyInk; homeBoxTitle.color = Ink;
            homeBoxHype.Tint = Color.white; homeBoxHype.Excited = v.OpenEnabled;

            for (int i = 0; i < homeBankSlots.Length; i++)
            {
                string id = i < v.Banked ? v.BankedCollectionIds[i] : null;
                homeBankSlots[i].texture = BoxPicture(id); homeBankSlots[i].enabled = id != null;
                homeBankSlots[i].transform.parent.gameObject.SetActive(i < v.MaxBanked);
            }
            for (int i = 0; i < homeDayDots.Length; i++)
            {
                homeDayDots[i].gameObject.SetActive(i < v.MaxPerDay);
                homeDayDots[i].color = i < v.EarnedToday ? Ink : new Color(Ink.r, Ink.g, Ink.b, .2f);
            }

            homeStatus.text = ServerBoxStatus(out bool counting);
            homeStatusClock.enabled = counting && online;
            bool earning = !v.BankFull && !v.DayDone;
            homeSteps.gameObject.SetActive(earning);
            homeStepCount.text = Mathf.Clamp(v.StepCredit, 0, v.StepsPerBox).ToString("N0");
            homeStepSuffix.text = "of " + v.StepsPerBox.ToString("N0") + " steps";
            if (homeSteps.sizeDelta.x > 0) PlaceStepSuffix();
            homeFill.fillAmount = Mathf.Clamp01(v.StepCredit / (float)Mathf.Max(1, v.StepsPerBox));
            homeFill.GetComponent<ProgressWidth>()?.Apply();

            homeOpen.gameObject.SetActive(v.Banked > 0); homeOpen.interactable = v.OpenEnabled;
            homeOpen.GetComponentInChildren<Text>().text = v.Busy ? "Opening…" : !online ? "Opens when you're online" : "Open " + BoxName(first);
            homeWalk.gameObject.SetActive(v.Banked == 0);
            int left = Mathf.Max(0, v.StepsPerBox - v.StepCredit);
            homeWalk.GetComponentInChildren<Text>().text = earning && left > 0 ? "Keep walking · " + left.ToString("N0") + " to go" : "How walking boxes work";
        }

        /// <summary>The Boxes page card in server mode.</summary>
        private void RefreshServerWalkingCard()
        {
            var v = boxView;
            string first = v.Banked > 0 ? v.BankedCollectionIds[0] : null;
            var art = walkArt.GetComponent<RawImage>();
            var picture = BoxPicture(first ?? v.NextCollectionId);
            art.texture = picture; art.uvRect = new Rect(0, 0, 1, 1); art.enabled = picture != null;
            walkMystery.gameObject.SetActive(picture == null);
            art.color = first != null ? Color.white : new Color(1, 1, 1, .45f);
            walkTitle.text = v.Banked == 0 ? "Walking boxes" : v.Banked == 1 ? BoxName(first) + " is ready" : v.Banked + " boxes ready · " + BoxName(first) + " first";
            walkProgress.text = Mathf.Clamp(v.StepCredit, 0, v.StepsPerBox).ToString("N0") + " of " + v.StepsPerBox.ToString("N0") + " steps · " + v.EarnedToday + " of " + v.MaxPerDay + " today";
            walkFill.fillAmount = v.BankFull || v.DayDone ? 1 : Mathf.Clamp01(v.StepCredit / (float)Mathf.Max(1, v.StepsPerBox));
            walkFill.GetComponent<ProgressWidth>()?.Apply();
            walkStatus.text = ServerBoxStatus(out bool counting, true);
            walkStatusClock.enabled = counting && v.Online;
            bool walking = WalkingOn;
            if (enableWalking.gameObject.activeSelf == walking) { enableWalking.gameObject.SetActive(!walking); AdaptLayout(); }
            walkRulesText.text = v.StepsPerBox.ToString("N0") + " steps earns a box · " + WalkingRuleFrequency(v)
                + " · up to " + v.MaxPerDay + " a day · " + v.MaxBanked + " can wait";
            if (rewardNote != null) rewardNote.GetComponent<Text>().text = WalkingRuleSummary(v) + ". Steps count while Pockle is open.";
            openDaily.interactable = v.OpenEnabled;
            openDaily.GetComponentInChildren<Text>().text = v.Banked == 0 ? "Walk to earn" : v.Busy ? "Opening…" : !v.Online ? "Opens when online" : "Open " + BoxName(first);
        }

        /// <summary>Home's top card is taller in server mode to fit the bank row.</summary>
        private float HomeBoxHeight => ServerBoxes ? 292 : 256;

        /// <summary>Positions the server-only parts of Home's card (called from LayoutHome).</summary>
        private void LayoutHomeServerBox(float textWidth)
        {
            bool server = ServerBoxes;
            if (homeBankRow != null) homeBankRow.gameObject.SetActive(server);
            if (!server) return;
            LeftLabel(homeBankRow, new Vector2(18, -72), new Vector2(textWidth + 40, 30));
            LeftLabel(homeStatus.rectTransform, new Vector2(18, -108), new Vector2(textWidth, 40));
            LeftLabel(homeSteps, new Vector2(18, -146), new Vector2(textWidth, 60));
        }

        /// <summary>The rules line and odds button under the Boxes card in server mode; returns the extra height.</summary>
        private float LayoutWalkRules(float width, float y)
        {
            if (walkRules == null) return 0;
            walkRules.gameObject.SetActive(ServerBoxes);
            if (!ServerBoxes) return 0;
            float w = Mathf.Min(width, 480);
            TopCentered(walkRules, new Vector2(0, -y), new Vector2(w - 8, 36));
            return 44;
        }
    }
}
