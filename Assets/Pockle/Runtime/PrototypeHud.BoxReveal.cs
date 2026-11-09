using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// The box reveal (owner, November 3: paid boxes and Mystery Mix packs are in the first release). A full-screen moment
    /// above every page: the collection's closed box waits for a tap, opens with its tier's treatment (see
    /// <see cref="BoxRevealPlayer"/>), and the toy lands with its card: rarity, New or Extra, name, personality, the real
    /// serial when the server sent one, and Play / Keep. Packs open one box after another with a Skip that goes straight to
    /// a summary of everything inside. Never touches inventory: the runtime saved the delivery before calling
    /// <see cref="ShowBoxReveal"/> (contract in <see cref="BoxRevealView"/>).
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private static readonly Color UnboxNight = new Color32(0x2A, 0x14, 0x30, 0xFF), UnboxObsidian = new Color32(0x0E, 0x14, 0x18, 0xFF);
        private static readonly Color UnboxMuted = new Color32(0xD9, 0xCB, 0xE0, 0xFF), UnboxGlass = new Color(1, 1, 1, .1f);
        private const int UnboxBits = 18;

        private RectTransform unboxRoot, unboxSafe, unboxStage, unboxCard, unboxSummary, unboxSummaryGrid, unboxDots, unboxRarityPill, unboxNewPill;
        private Image unboxBackground, unboxRarityFill, unboxNewFill;
        private RawImage unboxBox, unboxToy, unboxRarityIcon;
        private Text unboxEyebrow, unboxProgress, unboxTapHint, unboxRarityText, unboxNewText, unboxName, unboxLine, unboxSerial, unboxSummaryTitle;
        private Button unboxSkip, unboxSecondary, unboxPrimary;
        private Text unboxSecondaryText, unboxPrimaryText;
        private readonly RawImage[] unboxBits = new RawImage[UnboxBits];
        private readonly List<Image> unboxDotImages = new List<Image>();
        private BoxRevealPlayer unboxPlayer;
        private BoxRevealView unboxView;
        private int unboxIndex, unboxRevealed;
        private bool unboxFinished, unboxQuiet;

        /// <summary>Codex: fires with (receipt, how many toys have now been revealed) so a restart can resume at that count.</summary>
        public event Action<string, int> BoxRevealProgress;
        /// <summary>Fires once when the player leaves the reveal.</summary>
        public event Action<string> BoxRevealFinished;
        /// <summary>The moment a box pops open, with its tier (sound and haptics; the runtime respects the settings).</summary>
        public event Action<RarityTier> BoxRevealOpened;
        /// <summary>The player asked to play this toy straight from the reveal.</summary>
        public event Action<string> BoxRevealPlayRequested;

        public bool BoxRevealVisible => unboxRoot != null && unboxRoot.gameObject.activeSelf;
        /// <summary>Which toy is on screen (0-based), or the item count while the summary shows.</summary>
        public int BoxRevealIndex => unboxIndex;
        public bool BoxRevealSummaryVisible => unboxSummary != null && unboxSummary.gameObject.activeSelf;
        /// <summary>True while a box waits for its tap.</summary>
        public bool BoxRevealWaitingForTap => BoxRevealVisible && unboxPlayer != null && unboxPlayer.Phase == BoxRevealPlayer.State.Waiting;

        /// <summary>
        /// Codex: show an already-saved delivery. Returns false (and shows nothing) for an empty view, a missing receipt, or
        /// an unknown toy, and while a different receipt is still on screen. Showing the same receipt again is a no-op.
        /// </summary>
        public bool ShowBoxReveal(BoxRevealView view)
        {
            if (view == null || string.IsNullOrEmpty(view.ReceiptId) || view.Count == 0) return false;
            foreach (var item in view.Items)
                if (item == null || !ToyCatalog.TryGetCollectible(item.CollectibleId, out _))
                { Debug.LogWarning("Pockle box reveal: unknown toy " + (item?.CollectibleId ?? "(null)")); return false; }
            if (BoxRevealVisible) return unboxView != null && unboxView.ReceiptId == view.ReceiptId;
            if (canvas == null) return false;
            if (unboxRoot == null) BuildBoxReveal();
            unboxView = view; unboxFinished = false;
            unboxRevealed = Mathf.Clamp(view.StartAt, 0, view.Count);
            unboxSafe.anchorMin = safeRoot.anchorMin; unboxSafe.anchorMax = safeRoot.anchorMax; unboxSafe.offsetMin = unboxSafe.offsetMax = Vector2.zero;
            unboxRoot.gameObject.SetActive(true); unboxRoot.SetAsLastSibling();
            if (modal != null && modal.gameObject.activeSelf) modal.SetAsLastSibling(); // a dialog stays on top
            BuildUnboxDots();
            if (unboxRevealed >= view.Count) { if (view.IsPack) ShowUnboxSummary(); else { unboxIndex = 0; ShowUnboxItem(true); } }
            else { unboxIndex = unboxRevealed; ShowUnboxItem(false); }
            return true;
        }

        private void BuildBoxReveal()
        {
            unboxRoot = Rect("Box reveal", canvas.transform); Stretch(unboxRoot);
            unboxBackground = unboxRoot.gameObject.AddComponent<Image>(); unboxBackground.color = UnboxNight; unboxBackground.raycastTarget = true;
            var tap = unboxRoot.gameObject.AddComponent<Button>(); tap.transition = Selectable.Transition.None;
            tap.onClick.AddListener(TapBoxReveal); // tap the box to open it; tap during the opening to land it at once
            unboxSafe = Rect("Box reveal safe area", unboxRoot); Stretch(unboxSafe);

            // Top: what is opening, the pack progress and Skip.
            unboxEyebrow = Label("", unboxSafe, PockleTheme.SmallSize - 1, UnboxMuted, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, -22), new Vector2(260, 18));
            unboxProgress = Label("", unboxSafe, PockleTheme.CaptionSize, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, -42), new Vector2(200, 20));
            unboxDots = Rect("Pack progress", unboxSafe); TopCentered(unboxDots, new Vector2(0, -68), new Vector2(120, 8));
            unboxSkip = CreateButton("Reveal all", unboxSafe, Vector2.zero, new Vector2(96, 44), UnboxGlass, SkipBoxReveal, out _, PockleTheme.CaptionSize);
            var skipRect = (RectTransform)unboxSkip.transform;
            skipRect.anchorMin = skipRect.anchorMax = skipRect.pivot = new Vector2(1, 1); skipRect.anchoredPosition = new Vector2(-16, -14);
            unboxSkip.GetComponentInChildren<Text>().color = PockleTheme.OnPlum;

            // Stage: glow, dim, box, ring, sparkles and the toy, around one point above the middle.
            unboxStage = Rect("Reveal stage", unboxSafe);
            unboxStage.anchorMin = unboxStage.anchorMax = new Vector2(.5f, .6f); unboxStage.pivot = new Vector2(.5f, .5f); unboxStage.sizeDelta = Vector2.zero;
            var glow = Group("Tier glow", unboxStage); glow.sizeDelta = new Vector2(520, 520);
            var glowImage = glow.gameObject.AddComponent<RawImage>(); glowImage.texture = UiShapes.Glow; glowImage.raycastTarget = false;
            var dim = Group("Dim", unboxRoot); Stretch(dim); dim.SetSiblingIndex(0);
            var dimImage = dim.gameObject.AddComponent<Image>(); dimImage.color = new Color(.02f, .01f, .04f, .35f); dimImage.raycastTarget = false;
            var box = Group("Closed box", unboxStage); box.sizeDelta = new Vector2(220, 220); box.pivot = new Vector2(.5f, .35f);
            unboxBox = box.gameObject.AddComponent<RawImage>(); unboxBox.raycastTarget = false;
            var ring = Group("Ring", unboxStage); ring.sizeDelta = new Vector2(120, 120);
            var ringImage = ring.gameObject.AddComponent<RawImage>(); ringImage.texture = UiShapes.Glow; ringImage.raycastTarget = false;
            var bits = new RectTransform[UnboxBits]; var targets = new Vector2[UnboxBits];
            for (int i = 0; i < UnboxBits; i++)
            {
                bits[i] = Group("Sparkle", unboxStage); bits[i].sizeDelta = Vector2.one * (14 + i % 4 * 5);
                unboxBits[i] = bits[i].gameObject.AddComponent<RawImage>(); unboxBits[i].texture = UiShapes.Sparkle; unboxBits[i].raycastTarget = false;
                float a = i * Mathf.PI * 2 / UnboxBits + (i % 3) * .17f, r = 140 + (i * 41 % 70);
                targets[i] = new Vector2(Mathf.Cos(a) * r, Mathf.Sin(a) * r);
            }
            var toy = Group("Revealed toy", unboxStage); toy.sizeDelta = new Vector2(250, 250); toy.pivot = new Vector2(.5f, .3f);
            unboxToy = toy.gameObject.AddComponent<RawImage>(); unboxToy.raycastTarget = false;
            var hint = Group("Tap to open", unboxStage); hint.sizeDelta = new Vector2(220, 24); hint.anchoredPosition = new Vector2(0, -130);
            unboxTapHint = Label("Tap to open", hint, PockleTheme.BodySize, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(unboxTapHint.rectTransform);
            var flash = Rect("Flash", unboxSafe); Stretch(flash); flash.offsetMin = new Vector2(-400, -400); flash.offsetMax = new Vector2(400, 400); // above the stage, below the card
            var flashImage = flash.gameObject.AddComponent<Image>(); flashImage.color = new Color(1, 1, 1, 0); flashImage.raycastTarget = false;

            BuildUnboxCard();
            BuildUnboxSummary();

            unboxPlayer = unboxRoot.gameObject.AddComponent<BoxRevealPlayer>();
            unboxPlayer.Box = box; unboxPlayer.Toy = toy; unboxPlayer.Card = unboxCard; unboxPlayer.Ring = ring; unboxPlayer.Glow = glow; unboxPlayer.Dim = dim;
            unboxPlayer.TapHint = hint; unboxPlayer.Flash = flashImage; unboxPlayer.GlowGraphic = glowImage; unboxPlayer.RingGraphic = ringImage;
            unboxPlayer.Bits = bits; unboxPlayer.BitTargets = targets; unboxPlayer.CardRest = 28;
            unboxPlayer.Opened = () => { if (!unboxQuiet) BoxRevealOpened?.Invoke(CurrentUnboxLook().Tier); };
            unboxPlayer.Landed = UnboxLanded;
            unboxRoot.gameObject.SetActive(false);
        }

        private void BuildUnboxCard()
        {
            const float w = 350;
            unboxCard = Group("Reveal card", unboxSafe);
            unboxCard.anchorMin = unboxCard.anchorMax = unboxCard.pivot = new Vector2(.5f, 0); unboxCard.sizeDelta = new Vector2(w, 250);
            Surface(unboxCard, UnboxGlass, PockleTheme.CardRadius).raycastTarget = true; // taps on the card don't fall through to the stage

            unboxRarityPill = Rect("Rarity", unboxCard); LeftLabel(unboxRarityPill, new Vector2(20, -18), new Vector2(110, 30));
            unboxRarityFill = Surface(unboxRarityPill, Color.white, 15);
            var icon = Rect("Rarity icon", unboxRarityPill); icon.anchorMin = icon.anchorMax = icon.pivot = new Vector2(0, .5f);
            icon.anchoredPosition = new Vector2(10, 0); icon.sizeDelta = new Vector2(16, 16);
            unboxRarityIcon = icon.gameObject.AddComponent<RawImage>(); unboxRarityIcon.raycastTarget = false;
            unboxRarityText = Label("", unboxRarityPill, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            Stretch(unboxRarityText.rectTransform); unboxRarityText.rectTransform.offsetMin = new Vector2(32, 0); unboxRarityText.rectTransform.offsetMax = new Vector2(-10, 0);
            unboxRarityText.horizontalOverflow = HorizontalWrapMode.Overflow; unboxRarityText.resizeTextForBestFit = false;

            unboxNewPill = Rect("New or extra", unboxCard); LeftLabel(unboxNewPill, new Vector2(140, -18), new Vector2(80, 30));
            unboxNewFill = Surface(unboxNewPill, PockleTheme.FieldGold, 15);
            unboxNewText = Label("", unboxNewPill, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(unboxNewText.rectTransform); unboxNewText.horizontalOverflow = HorizontalWrapMode.Overflow; unboxNewText.resizeTextForBestFit = false;

            unboxName = Label("", unboxCard, 30, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(unboxName.rectTransform, new Vector2(20, -56), new Vector2(w - 40, 40));
            unboxLine = Label("", unboxCard, PockleTheme.CaptionSize + 1, UnboxMuted, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            LeftLabel(unboxLine.rectTransform, new Vector2(20, -98), new Vector2(w - 40, 40));
            unboxSerial = Label("", unboxCard, PockleTheme.CaptionSize, PockleTheme.FieldGold, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(unboxSerial.rectTransform, new Vector2(20, -142), new Vector2(w - 40, 20));

            float half = (w - 50) / 2;
            unboxSecondary = CreateButton("Keep", unboxCard, new Vector2(20, 178), new Vector2(half, 52), UnboxGlass, UnboxSecondaryTapped, out _);
            unboxSecondaryText = unboxSecondary.GetComponentInChildren<Text>(); unboxSecondaryText.color = PockleTheme.OnPlum;
            unboxPrimary = CreateButton("Play", unboxCard, new Vector2(30 + half, 178), new Vector2(half, 52), PockleTheme.Milk, UnboxPrimaryTapped, out _);
            unboxPrimaryText = unboxPrimary.GetComponentInChildren<Text>();
        }

        private void BuildUnboxSummary()
        {
            unboxSummary = Rect("Reveal summary", unboxSafe); Stretch(unboxSummary);
            unboxSummaryTitle = Label("", unboxSummary, PockleTheme.TitleSize - 4, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, -96), new Vector2(340, 40));
            unboxSummaryGrid = Rect("Summary toys", unboxSummary); TopCentered(unboxSummaryGrid, new Vector2(0, -150), new Vector2(350, 340));
            var done = CreateButton("Done", unboxSummary, Vector2.zero, new Vector2(310, 54), PockleTheme.Milk, FinishBoxReveal, out _);
            var doneRect = (RectTransform)done.transform;
            doneRect.anchorMin = doneRect.anchorMax = doneRect.pivot = new Vector2(.5f, 0); doneRect.anchoredPosition = new Vector2(0, 28);
            unboxSummary.gameObject.SetActive(false);
        }

        private void BuildUnboxDots()
        {
            foreach (var dot in unboxDotImages) if (dot != null) Destroy(dot.gameObject);
            unboxDotImages.Clear();
            int n = unboxView.IsPack ? unboxView.Count : 0;
            unboxDots.gameObject.SetActive(n > 0);
            for (int i = 0; i < n; i++)
            {
                var dot = Rect("Pack box " + (i + 1), unboxDots);
                dot.anchorMin = dot.anchorMax = dot.pivot = new Vector2(.5f, .5f);
                dot.sizeDelta = new Vector2(8, 8); dot.anchoredPosition = new Vector2((i - (n - 1) / 2f) * 16, 0);
                unboxDotImages.Add(Surface(dot, Color.white, 4));
            }
        }

        private void RefreshUnboxDots()
        {
            for (int i = 0; i < unboxDotImages.Count; i++)
                unboxDotImages[i].color = i < unboxRevealed ? PockleTheme.OnPlum : i == unboxIndex ? new Color(1, 1, 1, .7f) : new Color(1, 1, 1, .22f);
        }

        private BoxRevealItem CurrentUnboxItem => unboxView != null && unboxIndex >= 0 && unboxIndex < unboxView.Count ? unboxView.Items[unboxIndex] : null;

        private RarityLook CurrentUnboxLook() => UnboxLook(CurrentUnboxItem);

        /// <summary>The server's tier when it sent one, else the catalog's.</summary>
        private static RarityLook UnboxLook(BoxRevealItem item)
        {
            if (item == null || !ToyCatalog.TryGetCollectible(item.CollectibleId, out var toy)) return RarityLook.For(RarityTier.Common, false, "");
            if (item.Tier.HasValue) { var t = item.Tier.Value; return RarityLook.For(t, t == RarityTier.Secret || toy.IsSecret, RarityLook.Words(t)); }
            return RarityLook.Of(toy);
        }

        /// <summary>Puts toy <see cref="unboxIndex"/> on screen: its closed box waiting for a tap, or (already revealed) its card.</summary>
        private void ShowUnboxItem(bool alreadyRevealed)
        {
            var item = CurrentUnboxItem; if (item == null) return;
            ToyCatalog.TryGetCollectible(item.CollectibleId, out var toy);
            var look = UnboxLook(item);
            string collectionId = !string.IsNullOrEmpty(item.CollectionId) ? item.CollectionId : toy.CollectionId;

            unboxSummary.gameObject.SetActive(false);
            unboxStage.gameObject.SetActive(true);
            unboxBackground.color = look.Tier == RarityTier.Secret ? UnboxObsidian : UnboxNight;
            unboxEyebrow.text = UnboxSourceLine(collectionId);
            if(unboxView.IsPack)
            {
                LeftLabel(unboxEyebrow.rectTransform,new Vector2(16,-22),new Vector2(160,20));
                unboxEyebrow.alignment=TextAnchor.MiddleLeft;
            }
            else { TopCentered(unboxEyebrow.rectTransform,new Vector2(0,-22),new Vector2(260,18));unboxEyebrow.alignment=TextAnchor.MiddleCenter; }
            unboxProgress.text = unboxView.IsPack ? "Box " + (unboxIndex + 1) + " of " + unboxView.Count : "";
            unboxSkip.gameObject.SetActive(unboxView.IsPack);
            RefreshUnboxDots();

            float aspect = 1;
            Texture2D boxArt = ToyCatalog.TryGetCollection(collectionId, out var collection) ? CollectionLook.Box(collection, out aspect) : CollectionLook.Box(collectionId, out aspect);
            if (boxArt == null) boxArt = Resources.Load<Texture2D>("UI/Boxes/JellyGarden");
            unboxBox.texture = boxArt; unboxBox.color = boxArt != null ? Color.white : Color.clear;
            float bh = 220, bw = Mathf.Min(300, bh * Mathf.Max(.4f, aspect));
            ((RectTransform)unboxBox.transform).sizeDelta = new Vector2(bw, bh);
            unboxToy.texture = CutoutPortrait(item.CollectibleId);
            for (int i = 0; i < UnboxBits; i++)
                unboxBits[i].color = i % 3 == 0 ? Color.white : look.Tier == RarityTier.Common ? PockleTheme.FieldFor(item.CollectibleId) : look.RevealGlow;

            FillUnboxCard(item, toy, look);
            unboxPlayer.Present(look.RevealGlow, reducedMotionEnabled);
            if (alreadyRevealed) { unboxQuiet = true; unboxPlayer.Complete(); unboxQuiet = false; } // seen before an interruption: no second cue
        }

        private string UnboxSourceLine(string collectionId)
        {
            string name = ToyCatalog.TryGetCollection(collectionId, out var c) ? c.DisplayName : "Pockle";
            switch (unboxView.Source)
            {
                case BoxRevealSource.MysteryMix: return "MYSTERY MIX · " + name.ToUpperInvariant();
                case BoxRevealSource.WalkingBox: return "WALKING BOX · " + name.ToUpperInvariant();
                default: return (name + " box").ToUpperInvariant();
            }
        }

        private void FillUnboxCard(BoxRevealItem item, CollectibleDefinition toy, RarityLook look)
        {
            unboxRarityPill.gameObject.SetActive(look.Shown);
            float x = 20;
            if (look.Shown)
            {
                unboxRarityFill.color = look.Fill; unboxRarityText.color = look.Ink; unboxRarityText.text = look.Text;
                var iconTexture = PockleTheme.Icon(look.Icon);
                unboxRarityIcon.texture = iconTexture; unboxRarityIcon.color = iconTexture != null ? look.Ink : Color.clear;
                float pw = unboxRarityText.preferredWidth + 44;
                unboxRarityPill.sizeDelta = new Vector2(pw, 30); x += pw + 8;
            }
            unboxNewText.text = item.IsNew ? "New!" : "Extra";
            unboxNewFill.color = item.IsNew ? PockleTheme.FieldGold : UnboxGlass;
            unboxNewText.color = item.IsNew ? Ink : PockleTheme.OnPlum;
            unboxNewPill.anchoredPosition = new Vector2(x, -18); unboxNewPill.sizeDelta = new Vector2(unboxNewText.preferredWidth + 28, 30);

            unboxName.text = PersonalityBook.ShortName(item.CollectibleId);
            string character = toy.DisplayName; int dot = character.IndexOf(" · ", StringComparison.Ordinal);
            if (dot > 0) character = character.Substring(0, dot);
            string about = PersonalityBook.TryGet(item.CollectibleId, out var personality) && !string.IsNullOrEmpty(personality.Summary) ? personality.Summary : "";
            unboxLine.text = item.IsNew
                ? character + (about.Length > 0 ? " · " + about : "")
                : (about.Length > 0 ? character + " · " + about + "\n" : "") + "A spare for fusing or trading.";
            string serial = item.SerialText; // only a real, server-sent number; never a placeholder
            unboxSerial.text = serial ?? ""; unboxSerial.gameObject.SetActive(serial != null);

            bool last = unboxIndex >= unboxView.Count - 1;
            if (!unboxView.IsPack) { unboxSecondaryText.text = "Keep"; unboxPrimaryText.text = "Play"; }
            else { unboxSecondaryText.text = "Play"; unboxPrimaryText.text = last ? "See all " + unboxView.Count : "Next box"; }
            unboxSecondary.name = unboxSecondaryText.text; unboxPrimary.name = unboxPrimaryText.text; // captures and tests find them by what they say
        }

        private void TapBoxReveal()
        {
            if (!BoxRevealVisible || unboxPlayer == null || BoxRevealSummaryVisible) return;
            if (unboxPlayer.Phase == BoxRevealPlayer.State.Waiting) unboxPlayer.Open(CurrentUnboxLook().Tier);
            else if (unboxPlayer.Phase == BoxRevealPlayer.State.Opening) unboxPlayer.Complete();
        }

        /// <summary>Tests and the editor: open the waiting box and land it at once.</summary>
        public void CompleteBoxRevealStep()
        {
            if (!BoxRevealVisible || unboxPlayer == null || BoxRevealSummaryVisible) return;
            if (unboxPlayer.Phase == BoxRevealPlayer.State.Waiting) unboxPlayer.Open(CurrentUnboxLook().Tier);
            unboxPlayer.Complete();
        }

        private void UnboxLanded()
        {
            if (unboxView == null) return;
            if (unboxIndex + 1 > unboxRevealed)
            {
                unboxRevealed = unboxIndex + 1;
                BoxRevealProgress?.Invoke(unboxView.ReceiptId, unboxRevealed);
            }
            RefreshUnboxDots();
        }

        private void UnboxSecondaryTapped()
        {
            if (unboxView == null || unboxPlayer.Phase != BoxRevealPlayer.State.Shown) return;
            if (unboxView.IsPack) PlayFromBoxReveal(CurrentUnboxItem?.CollectibleId); else FinishBoxReveal(); // single: Keep
        }

        private void UnboxPrimaryTapped()
        {
            if (unboxView == null || unboxPlayer.Phase != BoxRevealPlayer.State.Shown) return;
            if (!unboxView.IsPack) { PlayFromBoxReveal(CurrentUnboxItem?.CollectibleId); return; }
            if (unboxIndex >= unboxView.Count - 1) { ShowUnboxSummary(); return; }
            unboxIndex++;
            ShowUnboxItem(unboxIndex < unboxRevealed);
        }

        /// <summary>Packs: everything left is revealed at once and the summary shows. A single box just lands.</summary>
        public void SkipBoxReveal()
        {
            if (!BoxRevealVisible || unboxView == null) return;
            if (!unboxView.IsPack) { CompleteBoxRevealStep(); return; }
            if (unboxRevealed < unboxView.Count)
            {
                if (unboxPlayer.Phase == BoxRevealPlayer.State.Opening || unboxPlayer.Phase == BoxRevealPlayer.State.Waiting)
                    BoxRevealOpened?.Invoke(BestUnboxTier(unboxIndex)); // one cue for everything skipped
                unboxRevealed = unboxView.Count;
                BoxRevealProgress?.Invoke(unboxView.ReceiptId, unboxRevealed);
            }
            ShowUnboxSummary();
        }

        private RarityTier BestUnboxTier(int from)
        {
            var best = RarityTier.Common; int rank = 0;
            for (int i = Mathf.Max(0, from); i < unboxView.Count; i++)
            {
                var t = UnboxLook(unboxView.Items[i]).Tier;
                int r = t == RarityTier.Secret ? 4 : t == RarityTier.Epic || t == RarityTier.UltraRare ? 3 : t == RarityTier.Rare || t == RarityTier.SuperRare ? 2 : 1;
                if (r > rank) { rank = r; best = t; }
            }
            return best;
        }

        private void ShowUnboxSummary()
        {
            unboxPlayer.Clear();
            unboxIndex = unboxView.Count;
            unboxStage.gameObject.SetActive(false);
            unboxSkip.gameObject.SetActive(false);
            bool secret = false;
            foreach (var item in unboxView.Items) if (UnboxLook(item).Tier == RarityTier.Secret) secret = true;
            unboxBackground.color = secret ? UnboxObsidian : UnboxNight;
            TopCentered(unboxEyebrow.rectTransform,new Vector2(0,-22),new Vector2(260,18));unboxEyebrow.alignment=TextAnchor.MiddleCenter;
            unboxEyebrow.text = unboxView.Source == BoxRevealSource.MysteryMix ? "MYSTERY MIX" : "YOUR BOXES";
            unboxProgress.text = "";
            RefreshUnboxDots();
            int fresh = 0; foreach (var item in unboxView.Items) if (item.IsNew) fresh++;
            unboxSummaryTitle.text = fresh == unboxView.Count ? unboxView.Count + " new toys!" : fresh > 0 ? fresh + " new, " + (unboxView.Count - fresh) + " extra" : unboxView.Count + " extras for fusing";

            for (int i = unboxSummaryGrid.childCount - 1; i >= 0; i--) Destroy(unboxSummaryGrid.GetChild(i).gameObject);
            const float cols = 3, gap = 10, tileH = 160;
            float tileW = (350 - gap * (cols - 1)) / cols;
            int n = unboxView.Count, rows = Mathf.CeilToInt(n / cols);
            for (int i = 0; i < n; i++)
            {
                int row = i / (int)cols, inRow = Mathf.Min((int)cols, n - row * (int)cols), col = i % (int)cols;
                float rowWidth = inRow * tileW + (inRow - 1) * gap;
                var position = new Vector2((350 - rowWidth) / 2 + col * (tileW + gap), row * (tileH + gap));
                BuildUnboxSummaryTile(unboxView.Items[i], i, position, new Vector2(tileW, tileH));
            }
            unboxSummaryGrid.sizeDelta = new Vector2(350, rows * (tileH + gap));
            unboxSummary.gameObject.SetActive(true);
        }

        private void BuildUnboxSummaryTile(BoxRevealItem item, int index, Vector2 position, Vector2 size)
        {
            var look = UnboxLook(item);
            string id = item.CollectibleId;
            var tile = CreateButton("Summary toy " + (index + 1), unboxSummaryGrid, position, size, UnboxGlass, () => PlayFromBoxReveal(id), out var fill, PockleTheme.SmallSize, false);
            if (look.Shown) fill.color = new Color(look.Ring.r, look.Ring.g, look.Ring.b, .22f);
            var title = tile.GetComponentInChildren<Text>(); title.text = PersonalityBook.ShortName(id); title.color = PockleTheme.OnPlum;
            title.alignment = TextAnchor.LowerCenter; title.rectTransform.offsetMin = new Vector2(6, 26); title.rectTransform.offsetMax = new Vector2(-6, 0);
            var picture = Rect("Picture", tile.transform); picture.anchorMin = picture.anchorMax = picture.pivot = new Vector2(.5f, 1);
            picture.anchoredPosition = new Vector2(0, -8); picture.sizeDelta = new Vector2(size.x - 10, size.x - 10);
            var raw = picture.gameObject.AddComponent<RawImage>(); raw.texture = CutoutPortrait(id); raw.raycastTarget = false;
            string under = item.SerialText ?? (look.Shown ? look.Text : "");
            var note = Label(under, tile.transform, PockleTheme.SmallSize - 1, look.Shown ? look.Ring : UnboxMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            note.rectTransform.anchorMin = new Vector2(0, 0); note.rectTransform.anchorMax = new Vector2(1, 0); note.rectTransform.pivot = new Vector2(.5f, 0);
            note.rectTransform.anchoredPosition = new Vector2(0, 8); note.rectTransform.sizeDelta = new Vector2(-8, 16);
            if (item.IsNew)
            {
                var badge = Rect("New", tile.transform); badge.anchorMin = badge.anchorMax = badge.pivot = new Vector2(1, 1);
                badge.anchoredPosition = new Vector2(-6, -6); badge.sizeDelta = new Vector2(42, 20);
                Surface(badge, PockleTheme.FieldGold, 10);
                var text = Label("New", badge, PockleTheme.SmallSize - 2, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(text.rectTransform);
            }
        }

        private void PlayFromBoxReveal(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            FinishBoxReveal();
            BoxRevealPlayRequested?.Invoke(id);
            ShowToy(id); // plays it when the saved copy is in the session; otherwise the player stays where they were
        }

        /// <summary>Leaves the reveal (Keep, Done, Play). Anything not yet seen counts as revealed first.</summary>
        public void FinishBoxReveal()
        {
            if (unboxView == null || unboxFinished) return;
            unboxFinished = true;
            if (unboxRevealed < unboxView.Count) { unboxRevealed = unboxView.Count; BoxRevealProgress?.Invoke(unboxView.ReceiptId, unboxRevealed); }
            string receipt = unboxView.ReceiptId;
            unboxPlayer.Clear();
            unboxRoot.gameObject.SetActive(false);
            unboxView = null; unboxIndex = 0;
            BoxRevealFinished?.Invoke(receipt);
        }

        /// <summary>The phone's Back: lands an opening box, jumps a pack to its summary, or closes the card/summary.</summary>
        private void BoxRevealBack()
        {
            if (unboxView == null) return;
            if (BoxRevealSummaryVisible) { FinishBoxReveal(); return; }
            if (unboxPlayer.Phase == BoxRevealPlayer.State.Opening) { unboxPlayer.Complete(); return; }
            if (unboxView.IsPack) { SkipBoxReveal(); return; }
            if (unboxPlayer.Phase == BoxRevealPlayer.State.Waiting) { CompleteBoxRevealStep(); return; }
            FinishBoxReveal();
        }
    }
}
