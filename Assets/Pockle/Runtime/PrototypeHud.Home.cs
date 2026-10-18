using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Home (owner-approved canvas row "Home", October 15): greeting; today's box as a big card with the real box
    /// (glowing and shaking when it's ready) and your steps; a peek at your live shelf; an activity rail you can switch
    /// between Nearby, Friends and Everyone; "Keep collecting" cards, closest to complete first; and the newest collections.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private const float HomeMaxWidth = 480f;
        // Today's box.
        private RectTransform homeBox, homeSteps, homeTrack;
        private RawImage homeBoxArt;
        private BoxHype homeBoxHype;
        private Text homeBoxLabel, homeBoxTitle, homeStepCount, homeStepSuffix, homeStatus;
        private Image homeFill, homeBoxFace;
        private Button homeOpen, homeWalk;
        // Live shelf peek.
        private Text homeShelfTitle;
        private Button homeSeeAll, homeShelfPeek;
        private RawImage homePeekBackdrop;
        private readonly RawImage[] homePeekToys = new RawImage[3];
        private RectTransform homePeekChip, homePeekEmpty;
        private Text homePeekChipText;
        // Activity.
        private Text homeActivityTitle;
        private Button homeActivityAll;
        private readonly Button[] homeScopeChips = new Button[3];
        private readonly Image[] homeScopeFaces = new Image[3];
        private RectTransform homeActivityViewport, homeActivityContent, homeActivityEmpty;
        private Text homeActivityEmptyTitle, homeActivityEmptyLine;
        private Button homeActivityEmptyAction;
        private ActivityScope homeScope = ActivityScope.Friends;
        private const string HomeScopeKey = "pockle.home.activityScope.v1";
        private LocalActivityFeed activityFeed;
        private sealed class ActivityCard
        {
            public RectTransform Root, Art, SampleTag; public Image ArtFace, AvatarFace, CheerFace;
            public RawImage Avatar, Picture, Second, Badge; public Text Who, When, Kind, Caption, Plus, CheerText; public Button Cheer;
        }
        private readonly List<ActivityCard> activityCards = new List<ActivityCard>();
        private const float ActivityCardWidth = 214, ActivityCardHeight = 252;
        // Keep collecting.
        private Text homeCollectTitle;
        private Button homeCollectAll;
        private RectTransform homeCollectViewport, homeCollectContent;
        private sealed class CollectCard { public Button Button; public RawImage Cover, Box; public Text Name, Status; public Image Fill; }
        private readonly List<CollectCard> collectCards = new List<CollectCard>();
        private const float CollectWidth = 236, CollectHeight = 156;
        // New this season.
        private Text homeNewTitle;
        private Button homeNewBanner;
        private RawImage homeNewCover;
        private readonly RawImage[] homeNewBoxes = new RawImage[4];
        private Text homeNewName, homeNewLine;
        private RectTransform rewardNote, badgeEntry;

        private void BuildHome()
        {
            activityFeed = new LocalActivityFeed();
            homeScope = (ActivityScope)Mathf.Clamp(PlayerPrefs.GetInt(HomeScopeKey, (int)ActivityScope.Friends), 0, 2);

            // Today's box: label, name, the box itself, then steps or one clear action.
            homeBox = Rect("Today's box", homeRoot); homeBoxFace = Surface(homeBox, PockleTheme.Jelly, 30);
            homeBox.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var glow = Rect("Box glow", homeBox);
            var glowImage = glow.gameObject.AddComponent<RawImage>(); glowImage.texture = UiShapes.Glow; glowImage.color = new Color(1, 1, 1, .8f); glowImage.raycastTarget = false;
            glow.anchorMin = glow.anchorMax = glow.pivot = new Vector2(1, 1); glow.anchoredPosition = new Vector2(40, 10); glow.sizeDelta = new Vector2(240, 240);
            var art = Rect("Box art", homeBox);
            homeBoxArt = art.gameObject.AddComponent<RawImage>(); homeBoxArt.raycastTarget = false;
            homeBoxHype = BoxHype.Add(homeBoxArt, Color.white, .2f);
            homeBoxLabel = Label("TODAY'S BOX", homeBox, PockleTheme.SmallSize - 1, PockleTheme.JellyInk, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 18));
            homeBoxTitle = Label("Pip box", homeBox, 24, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 30));
            homeStatus = Label("", homeBox, PockleTheme.CaptionSize, PockleTheme.JellyInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(190, 40));
            homeSteps = Rect("Today's steps", homeBox);
            homeStepCount = Label("0", homeSteps, 26, Ink, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, new Vector2(140, 32));
            homeStepCount.resizeTextForBestFit = false; homeStepCount.horizontalOverflow = HorizontalWrapMode.Overflow;
            homeStepSuffix = Label("of 1,000 steps", homeSteps, PockleTheme.CaptionSize, PockleTheme.JellyInk, FontStyle.Normal, TextAnchor.LowerLeft, Vector2.zero, new Vector2(160, 22));
            homeTrack = Rect("Today's step progress", homeSteps); Surface(homeTrack, new Color(1, 1, 1, .6f), 5);
            homeFill = ProgressFill(homeTrack, Ink);
            homeOpen = CreateButton("Open your box", homeBox, Vector2.zero, new Vector2(300, 54), PockleTheme.Plum,
                () => { if (session != null && session.Progress.CanClaim) DailyBoxRequested?.Invoke(); }, out _, 18);
            homeWalk = CreateButton("Keep walking", homeBox, Vector2.zero, new Vector2(300, 48), new Color(1, 1, 1, .72f), () => Navigate(AppPage.Rewards), out _, 15);
            homeWalk.name = "Home keep walking";

            // Your live shelf: a strip of the shelf art with the top row of toys and the moment they share.
            homeShelfTitle = Label("Your live shelf", homeRoot, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 30));
            homeSeeAll = HeaderLink("See all", () => Navigate(AppPage.Shelf));
            homeShelfPeek = CreateButton("Live shelf", homeRoot, Vector2.zero, new Vector2(300, 176), SceneDay, () => Navigate(AppPage.Shelf), out var peekFace, 12, false);
            homeShelfPeek.name = "Home live shelf";
            homeShelfPeek.GetComponentInChildren<Text>().enabled = false;
            SetRadius(peekFace, 26);
            homeShelfPeek.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var backdrop = Rect("Shelf art", homeShelfPeek.transform);
            homePeekBackdrop = backdrop.gameObject.AddComponent<RawImage>(); homePeekBackdrop.raycastTarget = false;
            SetPicture(homePeekBackdrop, Resources.Load<Texture2D>(ShelfBackdropPath));
            homePeekBackdrop.enabled = homePeekBackdrop.texture != null;
            for (int i = 0; i < homePeekToys.Length; i++)
            {
                var holder = Rect("Peek toy", homeShelfPeek.transform);
                var motion = Rect("Motion", holder); Stretch(motion); motion.pivot = new Vector2(.5f, SceneBase);
                motion.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Calm, i * .4f);
                var picture = Rect("Toy", motion); Stretch(picture);
                homePeekToys[i] = picture.gameObject.AddComponent<RawImage>(); homePeekToys[i].raycastTarget = false;
            }
            homePeekChip = Rect("Peek moment", homeShelfPeek.transform); Surface(homePeekChip, Color.white, 15);
            homePeekChipText = Label("", homePeekChip, PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(homePeekChipText.rectTransform); homePeekChipText.resizeTextForBestFit = false; homePeekChipText.horizontalOverflow = HorizontalWrapMode.Overflow;
            homePeekEmpty = Rect("Peek empty", homeShelfPeek.transform);
            var emptyText = Label("Your first toys will live here", homePeekEmpty, PockleTheme.CaptionSize, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(emptyText.rectTransform);

            // Activity: title, "See all", three scope chips, then a sideways rail of cards (or a friendly empty state).
            homeActivityTitle = Label("Activity", homeRoot, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 30));
            homeActivityAll = HeaderLink("See all", () => Navigate(AppPage.Social));
            homeActivityAll.name = "Home activity · see all";
            string[] scopes = { "Nearby", "Friends", "Everyone" };
            for (int i = 0; i < 3; i++)
            {
                var scope = (ActivityScope)i;
                homeScopeChips[i] = CreateButton(scopes[i], homeRoot, Vector2.zero, new Vector2(90, 36), Quiet, () => SetHomeScope(scope), out homeScopeFaces[i], 14);
                homeScopeChips[i].name = "Home activity scope · " + scopes[i];
            }
            homeActivityViewport = Rail("Activity rail", out homeActivityContent);
            homeActivityEmpty = Rect("Activity empty", homeRoot); Surface(homeActivityEmpty, Quiet, 24);
            homeActivityEmptyTitle = Label("", homeActivityEmpty, PockleTheme.BodySize, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            homeActivityEmptyLine = Label("", homeActivityEmpty, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            homeActivityEmptyLine.verticalOverflow = VerticalWrapMode.Overflow;
            homeActivityEmptyAction = CreateButton("Find collectors", homeActivityEmpty, Vector2.zero, new Vector2(160, 40), PockleTheme.Plum, () => Navigate(AppPage.Social), out _, 14);
            homeActivityEmptyAction.name = "Home activity · find";

            // Keep collecting.
            homeCollectTitle = Label("Keep collecting", homeRoot, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(240, 30));
            homeCollectAll = HeaderLink("All", () => Navigate(AppPage.Collections));
            homeCollectAll.name = "Home collections · all";
            homeCollectViewport = Rail("Collection rail", out homeCollectContent);

            // New this season: the newest collections on their painting, with their boxes.
            homeNewTitle = Label("New this season", homeRoot, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(240, 30));
            homeNewBanner = CreateButton("New collections", homeRoot, Vector2.zero, new Vector2(300, 196), PockleTheme.Midnight, () => Navigate(AppPage.Collections), out var newFace, 12, false);
            homeNewBanner.name = "Home new collections";
            homeNewBanner.GetComponentInChildren<Text>().enabled = false;
            SetRadius(newFace, 26);
            homeNewBanner.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var cover = Rect("Cover", homeNewBanner.transform); Stretch(cover);
            homeNewCover = cover.gameObject.AddComponent<RawImage>(); homeNewCover.raycastTarget = false;
            var shade = Rect("Shade", homeNewBanner.transform); Stretch(shade); shade.anchorMax = new Vector2(1, .7f);
            var shadeImage = shade.gameObject.AddComponent<RawImage>(); shadeImage.texture = UiShapes.FadeDown; shadeImage.color = new Color(.16f, .08f, .19f, .7f); shadeImage.raycastTarget = false;
            var tag = Rect("New tag", homeNewBanner.transform); Surface(tag, Color.white, 13);
            tag.anchorMin = tag.anchorMax = tag.pivot = new Vector2(0, 1); tag.anchoredPosition = new Vector2(14, -14); tag.sizeDelta = new Vector2(56, 26);
            var tagText = Label("NEW", tag, 12, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(tagText.rectTransform);
            homeNewName = Label("", homeNewBanner.transform, 22, Color.white, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, Vector2.zero);
            homeNewLine = Label("", homeNewBanner.transform, PockleTheme.SmallSize, new Color(1, 1, 1, .9f), FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            homeNewLine.verticalOverflow = VerticalWrapMode.Overflow;
            for (int i = 0; i < homeNewBoxes.Length; i++)
            {
                var box = Rect("New box", homeNewBanner.transform);
                var bob = Rect("Bob", box); Stretch(bob);
                bob.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Calm, i * .35f, .5f);
                homeNewBoxes[i] = bob.gameObject.AddComponent<RawImage>(); homeNewBoxes[i].raycastTarget = false;
            }
        }

        /// <summary>A plum "See all"-style link for a section heading.</summary>
        private Button HeaderLink(string text, System.Action click)
        {
            var link = CreateButton(text, homeRoot, Vector2.zero, new Vector2(100, 44), Color.clear, click, out _, 15);
            var label = link.GetComponentInChildren<Text>(); label.color = PockleTheme.JellyInk; label.alignment = TextAnchor.MiddleRight;
            return link;
        }

        /// <summary>A sideways-scrolling strip that runs edge to edge.</summary>
        private RectTransform Rail(string name, out RectTransform content)
        {
            var viewport = Rect(name, homeRoot);
            viewport.gameObject.AddComponent<RectMask2D>();
            var hit = viewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var scroll = viewport.gameObject.AddComponent<ScrollRect>(); scroll.vertical = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            content = Rect(name + " content", viewport);
            content.anchorMin = content.anchorMax = content.pivot = new Vector2(0, 1);
            scroll.content = content; scroll.viewport = viewport;
            return viewport;
        }

        private void SetHomeScope(ActivityScope scope)
        {
            homeScope = scope; PlayerPrefs.SetInt(HomeScopeKey, (int)scope);
            RefreshHome(); AdaptLayout();
        }

        // ----- Cards -----

        private ActivityCard BuildActivityCard()
        {
            var card = new ActivityCard();
            card.Root = Rect("Activity card", homeActivityContent); Surface(card.Root, Color.white, 24);
            var avatar = Rect("Avatar", card.Root); card.AvatarFace = Surface(avatar, PockleTheme.FieldPeach, 16);
            avatar.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            LeftLabel(avatar, new Vector2(12, -12), new Vector2(32, 32));
            var avatarPicture = Rect("Avatar picture", avatar); Stretch(avatarPicture);
            card.Avatar = avatarPicture.gameObject.AddComponent<RawImage>(); card.Avatar.raycastTarget = false;
            card.Who = Label("", card.Root, 13, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(card.Who.rectTransform, new Vector2(50, -12), new Vector2(80, 16));
            card.When = Label("", card.Root, 11, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(card.When.rectTransform, new Vector2(50, -29), new Vector2(80, 14));
            var kind = Rect("Kind", card.Root); Surface(kind, Quiet, 11);
            kind.anchorMin = kind.anchorMax = kind.pivot = new Vector2(1, 1); kind.anchoredPosition = new Vector2(-12, -16); kind.sizeDelta = new Vector2(76, 22);
            card.Kind = Label("", kind, 10, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(card.Kind.rectTransform);
            card.Art = Rect("Art", card.Root); card.ArtFace = Surface(card.Art, PockleTheme.FieldPeach, 18);
            card.Art.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            LeftLabel(card.Art, new Vector2(12, -54), new Vector2(ActivityCardWidth - 24, 96));
            card.Picture = ArtImage("Picture", card.Art); card.Second = ArtImage("Second", card.Art); card.Badge = ArtImage("Badge", card.Art);
            card.Plus = Label("+", card.Art, 20, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            card.SampleTag = Rect("Sample", card.Art); Surface(card.SampleTag, new Color(1, 1, 1, .9f), 10);
            card.SampleTag.anchorMin = card.SampleTag.anchorMax = card.SampleTag.pivot = new Vector2(0, 1);
            card.SampleTag.anchoredPosition = new Vector2(8, -8); card.SampleTag.sizeDelta = new Vector2(64, 20);
            var sample = Label("SAMPLE", card.SampleTag, 10, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(sample.rectTransform);
            card.Caption = Label("", card.Root, PockleTheme.CaptionSize, Ink, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            card.Caption.supportRichText = true; card.Caption.verticalOverflow = VerticalWrapMode.Overflow;
            LeftLabel(card.Caption.rectTransform, new Vector2(12, -160), new Vector2(ActivityCardWidth - 24, 40));
            card.Cheer = CreateButton("Cheer", card.Root, Vector2.zero, new Vector2(124, 36), Quiet, () => { }, out card.CheerFace, 13);
            card.CheerText = card.Cheer.GetComponentInChildren<Text>();
            LeftLabel(card.Cheer.GetComponent<RectTransform>(), new Vector2(12, -(ActivityCardHeight - 48)), new Vector2(124, 36));
            return card;
        }

        private static RawImage ArtImage(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var image = go.AddComponent<RawImage>(); image.raycastTarget = false;
            return image;
        }

        private void ShowActivityCard(ActivityCard card, ActivityItem item, int index)
        {
            card.Root.gameObject.SetActive(true);
            LeftLabel(card.Root, new Vector2(index * (ActivityCardWidth + 10), 0), new Vector2(ActivityCardWidth, ActivityCardHeight));
            card.Root.name = "Activity · " + item.Kind + " · " + item.Who;
            card.Who.text = item.Who; card.When.text = LocalActivityFeed.Ago(item.WhenUtc);
            SetPicture(card.Avatar, StillPortrait(item.AvatarId)); card.AvatarFace.color = PockleTheme.FieldFor(item.AvatarId);
            string[] kinds = { "NEW TOY", "FUSION", "COMPLETE", "BADGE", "MILESTONE" };
            card.Kind.text = item.Detail == "SECRET" ? "SECRET" : kinds[(int)item.Kind];
            card.Caption.text = item.Caption;
            card.SampleTag.gameObject.SetActive(item.Sample);
            bool fusion = item.Kind == ActivityKind.Fusion, emblem = item.Kind == ActivityKind.Badge || item.Kind == ActivityKind.Milestone;
            card.Plus.gameObject.SetActive(fusion); card.Second.gameObject.SetActive(fusion);
            card.Badge.gameObject.SetActive(emblem); card.Picture.gameObject.SetActive(!emblem);
            switch (item.Kind)
            {
                case ActivityKind.Complete:
                    ToyCatalog.TryGetCollection(item.CollectionId, out var done);
                    card.ArtFace.color = done != null ? CatalogUnityColor(done.FieldColor) : PockleTheme.FieldPeach;
                    SetPicture(card.Picture, done != null ? CollectionLook.Box(done, out _) : null);
                    PlaceCentered(card.Picture.rectTransform, Vector2.zero, new Vector2(84, 84));
                    break;
                case ActivityKind.Fusion:
                    card.ArtFace.color = Color.Lerp(PockleTheme.FieldFor(item.CollectibleId), PockleTheme.FieldFor(item.SecondId), .5f);
                    SetPicture(card.Picture, StillPortrait(item.CollectibleId)); SetPicture(card.Second, StillPortrait(item.SecondId));
                    PlaceCentered(card.Picture.rectTransform, new Vector2(-48, 0), new Vector2(72, 72));
                    PlaceCentered(card.Plus.rectTransform, Vector2.zero, new Vector2(24, 24));
                    PlaceCentered(card.Second.rectTransform, new Vector2(48, 0), new Vector2(72, 72));
                    break;
                case ActivityKind.Badge:
                case ActivityKind.Milestone:
                    bool badge = item.Kind == ActivityKind.Badge;
                    card.ArtFace.color = badge ? PockleTheme.Midnight : PockleTheme.FieldGold;
                    card.Badge.texture = UiShapes.Sparkle; card.Badge.color = badge ? PockleTheme.FieldGold : Ink;
                    PlaceCentered(card.Badge.rectTransform, Vector2.zero, new Vector2(52, 52));
                    break;
                default:
                    card.ArtFace.color = PockleTheme.FieldFor(item.CollectibleId);
                    SetPicture(card.Picture, StillPortrait(item.CollectibleId));
                    PlaceCentered(card.Picture.rectTransform, Vector2.zero, new Vector2(88, 88));
                    break;
            }
            card.Picture.enabled = card.Picture.texture != null; card.Second.enabled = card.Second.texture != null;
            // Your own posts show in "Everyone" but you don't cheer yourself.
            card.Cheer.gameObject.SetActive(!item.Mine);
            card.CheerText.text = "Cheer · " + item.Cheers;
            card.CheerFace.color = item.Cheered ? new Color32(0xFF, 0xE1, 0xEA, 0xFF) : Quiet;
            card.CheerText.color = item.Cheered ? PockleTheme.Heart : Ink;
            card.Cheer.name = "Cheer · " + item.Id;
            card.Cheer.onClick.RemoveAllListeners();
            string id = item.Id;
            card.Cheer.onClick.AddListener(() => { activityFeed.Cheer(id); RefreshHome(); });
        }

        private static void PlaceCentered(RectTransform rect, Vector2 offset, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = offset; rect.sizeDelta = size;
        }

        private CollectCard BuildCollectCard()
        {
            var card = new CollectCard();
            card.Button = CreateButton("", homeCollectContent, Vector2.zero, new Vector2(CollectWidth, CollectHeight), PockleTheme.Mystery, () => { }, out var face, 12, false);
            SetRadius(face, 24);
            card.Button.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var cover = Rect("Cover", card.Button.transform); Stretch(cover);
            card.Cover = cover.gameObject.AddComponent<RawImage>(); card.Cover.raycastTarget = false;
            var shade = Rect("Shade", card.Button.transform); Stretch(shade); shade.anchorMax = new Vector2(1, .7f);
            var shadeImage = shade.gameObject.AddComponent<RawImage>(); shadeImage.texture = UiShapes.FadeDown; shadeImage.color = new Color(.16f, .08f, .19f, .66f); shadeImage.raycastTarget = false;
            var box = Rect("Box", card.Button.transform);
            box.anchorMin = box.anchorMax = box.pivot = new Vector2(1, 1); box.anchoredPosition = new Vector2(-8, -8); box.sizeDelta = new Vector2(62, 62);
            card.Box = box.gameObject.AddComponent<RawImage>(); card.Box.raycastTarget = false;
            card.Name = BottomLabel(card.Button.transform, 18, Color.white, 42, 24);
            card.Status = BottomLabel(card.Button.transform, 12, new Color(1, 1, 1, .92f), 22, 18);
            var track = Rect("Progress", card.Button.transform); Surface(track, new Color(1, 1, 1, .32f), 3);
            track.anchorMin = track.anchorMax = track.pivot = new Vector2(0, 0); track.anchoredPosition = new Vector2(14, 12); track.sizeDelta = new Vector2(CollectWidth - 28, 6);
            card.Fill = ProgressFill(track, Color.white);
            return card;
        }

        private Text BottomLabel(Transform parent, int size, Color colour, float y, float height)
        {
            var text = Label("", parent, size, colour, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, Vector2.zero);
            var rect = text.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 0);
            rect.anchoredPosition = new Vector2(14, y); rect.sizeDelta = new Vector2(CollectWidth - 28, height);
            return text;
        }

        // ----- Layout -----

        private float LayoutHome(float width)
        {
            float w = Mathf.Min(width, HomeMaxWidth);
            float left = (width - w) / 2;
            float y = 4;

            // Today's box.
            const float boxHeight = 256;
            LeftLabel(homeBox, new Vector2(left, -y), new Vector2(w, boxHeight));
            float textWidth = Mathf.Max(150, w - 190);
            LeftLabel(homeBoxLabel.rectTransform, new Vector2(18, -18), new Vector2(textWidth, 18));
            LeftLabel(homeBoxTitle.rectTransform, new Vector2(18, -38), new Vector2(textWidth, 30));
            LeftLabel(homeStatus.rectTransform, new Vector2(18, -70), new Vector2(textWidth, 40));
            LeftLabel(homeSteps, new Vector2(18, -104), new Vector2(textWidth, 60));
            LeftLabel(homeStepCount.rectTransform, Vector2.zero, new Vector2(textWidth, 32));
            PlaceStepSuffix();
            LeftLabel(homeTrack, new Vector2(0, -42), new Vector2(textWidth, 10));
            var art = homeBoxArt.rectTransform;
            art.anchorMin = art.anchorMax = art.pivot = new Vector2(1, 1);
            float boxSize = Mathf.Min(156, w * .42f);
            art.anchoredPosition = new Vector2(2, -28); art.sizeDelta = new Vector2(boxSize, boxSize);
            var open = homeOpen.GetComponent<RectTransform>(); var walk = homeWalk.GetComponent<RectTransform>();
            foreach (var action in new[] { open, walk })
            {
                action.anchorMin = action.anchorMax = action.pivot = new Vector2(.5f, 0);
                action.anchoredPosition = new Vector2(0, 18); action.sizeDelta = new Vector2(w - 36, action == open ? 54 : 48);
            }
            y += boxHeight + 22;

            // Live shelf peek.
            LeftLabel(homeShelfTitle.rectTransform, new Vector2(left, -y), new Vector2(w - 100, 30));
            LayoutHeaderLink(homeSeeAll, left + w, y);
            y += 40;
            const float peekHeight = 176;
            LeftLabel(homeShelfPeek.GetComponent<RectTransform>(), new Vector2(left, -y), new Vector2(w, peekHeight));
            // The shelf art at the card's width, slid up so its top plank sits near the card's bottom.
            var shelfArt = homePeekBackdrop.texture;
            float artHeight = shelfArt != null ? w * shelfArt.height / Mathf.Max(1f, shelfArt.width) : peekHeight;
            float plankInCard = peekHeight - 30;
            var backdropRect = homePeekBackdrop.rectTransform;
            backdropRect.anchorMin = backdropRect.anchorMax = backdropRect.pivot = new Vector2(.5f, 1);
            backdropRect.anchoredPosition = new Vector2(0, BackdropTopPlank * artHeight - plankInCard); backdropRect.sizeDelta = new Vector2(w, artHeight);
            float toySize = Mathf.Min(104, (w - 24) / 3);
            for (int i = 0; i < homePeekToys.Length; i++)
            {
                var holder = (RectTransform)homePeekToys[i].transform.parent.parent;
                float feet = FeetOf(PeekId(i));
                LeftLabel(holder, new Vector2(w * (i + .5f) / 3 - toySize / 2, -(plankInCard - (1 - feet) * toySize)), new Vector2(toySize, toySize));
                ((RectTransform)homePeekToys[i].transform.parent).pivot = new Vector2(.5f, feet);
            }
            homePeekChip.anchorMin = homePeekChip.anchorMax = homePeekChip.pivot = new Vector2(.5f, 1);
            homePeekChip.anchoredPosition = new Vector2(0, -12); homePeekChip.sizeDelta = new Vector2(Mathf.Min(w - 24, homePeekChipText.preferredWidth + 28), 30);
            homePeekEmpty.anchorMin = new Vector2(0, 1); homePeekEmpty.anchorMax = new Vector2(1, 1); homePeekEmpty.pivot = new Vector2(.5f, 1);
            homePeekEmpty.anchoredPosition = new Vector2(0, -16); homePeekEmpty.sizeDelta = new Vector2(0, 28);
            y += peekHeight + 22;

            // Activity.
            LeftLabel(homeActivityTitle.rectTransform, new Vector2(left, -y), new Vector2(w - 100, 30));
            LayoutHeaderLink(homeActivityAll, left + w, y);
            y += 40;
            float chipX = left;
            for (int i = 0; i < 3; i++)
            {
                float chipWidth = homeScopeChips[i].GetComponentInChildren<Text>().preferredWidth + 30;
                LeftLabel(homeScopeChips[i].GetComponent<RectTransform>(), new Vector2(chipX, -y), new Vector2(chipWidth, 36));
                chipX += chipWidth + 8;
            }
            y += 36 + 12;
            if (homeActivityViewport.gameObject.activeSelf)
            {
                LeftLabel(homeActivityViewport, new Vector2(0, -y), new Vector2(width, ActivityCardHeight + 4));
                if (Mathf.Approximately(homeActivityContent.anchoredPosition.x, 0)) homeActivityContent.anchoredPosition = new Vector2(left, 0);
                y += ActivityCardHeight + 4 + 22;
            }
            else
            {
                const float emptyHeight = 132;
                LeftLabel(homeActivityEmpty, new Vector2(left, -y), new Vector2(w, emptyHeight));
                LeftLabel(homeActivityEmptyTitle.rectTransform, new Vector2(16, -16), new Vector2(w - 32, 22));
                LeftLabel(homeActivityEmptyLine.rectTransform, new Vector2(16, -40), new Vector2(w - 32, 40));
                float actionWidth = Mathf.Max(150, homeActivityEmptyAction.GetComponentInChildren<Text>().preferredWidth + 32);
                LeftLabel(homeActivityEmptyAction.GetComponent<RectTransform>(), new Vector2(16, -(emptyHeight - 54)), new Vector2(actionWidth, 40));
                y += emptyHeight + 22;
            }

            // Keep collecting.
            LeftLabel(homeCollectTitle.rectTransform, new Vector2(left, -y), new Vector2(w - 100, 30));
            LayoutHeaderLink(homeCollectAll, left + w, y);
            y += 40;
            LeftLabel(homeCollectViewport, new Vector2(0, -y), new Vector2(width, CollectHeight));
            if (Mathf.Approximately(homeCollectContent.anchoredPosition.x, 0)) homeCollectContent.anchoredPosition = new Vector2(left, 0);
            y += CollectHeight + 22;

            // New this season.
            bool fresh = homeNewBanner.gameObject.activeSelf;
            homeNewTitle.gameObject.SetActive(fresh);
            if (fresh)
            {
                LeftLabel(homeNewTitle.rectTransform, new Vector2(left, -y), new Vector2(w, 30));
                y += 40;
                const float bannerHeight = 196;
                LeftLabel(homeNewBanner.GetComponent<RectTransform>(), new Vector2(left, -y), new Vector2(w, bannerHeight));
                homeNewCover.uvRect = CollectionLook.Cover(homeNewCover.texture, w / bannerHeight, .5f, .45f);
                float textWidth2 = Mathf.Min(170, w * .48f);
                var name = homeNewName.rectTransform; name.anchorMin = name.anchorMax = name.pivot = new Vector2(0, 0);
                name.anchoredPosition = new Vector2(14, 36); name.sizeDelta = new Vector2(textWidth2, 56);
                var line = homeNewLine.rectTransform; line.anchorMin = line.anchorMax = line.pivot = new Vector2(0, 0);
                line.anchoredPosition = new Vector2(14, 12); line.sizeDelta = new Vector2(textWidth2, 22);
                for (int i = 0; i < homeNewBoxes.Length; i++)
                {
                    var box = (RectTransform)homeNewBoxes[i].transform.parent; box.anchorMin = box.anchorMax = box.pivot = new Vector2(1, 0);
                    box.anchoredPosition = new Vector2(-10 - (homeNewBoxes.Length - 1 - i) * 42, 12); box.sizeDelta = new Vector2(44, 44);
                }
                y += bannerHeight;
            }

            RefreshHome();
            TopCentered(homeRoot, Vector2.zero, new Vector2(width, y + 8));
            return y + 8;
        }

        private static void LayoutHeaderLink(Button link, float right, float y)
        {
            var rect = link.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(right, -(y - 7)); rect.sizeDelta = new Vector2(100, 44);
        }

        private void PlaceStepSuffix()
        {
            float x = Mathf.Min(homeStepCount.preferredWidth, homeSteps.sizeDelta.x) + 6;
            LeftLabel(homeStepSuffix.rectTransform, new Vector2(x, -8), new Vector2(Mathf.Max(40, homeSteps.sizeDelta.x - x), 22));
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

        // ----- Refresh -----

        private string PeekId(int i) => i < liveShelf.Count ? liveShelf[i] : null;

        private void RefreshHome()
        {
            if (session == null || homeStepCount == null) return;
            var saved = session.Progress.Save;
            int target = CollectionProgress.DailyTarget;
            bool today = saved.Day == CollectionSession.Today;
            bool ready = today && session.Progress.CanClaim;
            bool walking = today && !ready && !saved.Claimed;

            // Today's box: the daily pool (Jelly Garden) is the Pip collection.
            ToyCatalog.TryGetCollection("pip", out var daily);
            homeBoxArt.texture = daily != null ? CollectionLook.Box(daily, out _) : null;
            homeBoxArt.enabled = homeBoxArt.texture != null;
            homeBoxTitle.text = (daily != null ? daily.DisplayName : "Pip") + " box";
            homeBoxFace.color = ready ? PockleTheme.Jelly : Color.Lerp(PockleTheme.Jelly, Color.white, .3f);
            homeBoxHype.Excited = ready; // the box only glows, shakes and sparkles when it can be opened
            homeOpen.gameObject.SetActive(ready); homeOpen.interactable = ready;
            homeWalk.gameObject.SetActive(walking);
            homeSteps.gameObject.SetActive(walking);
            int remaining = Mathf.Max(0, target - saved.Steps);
            homeWalk.GetComponentInChildren<Text>().text = "Keep walking · " + remaining.ToString("N0") + " to go";
            homeStepCount.text = saved.Steps.ToString("N0");
            homeStepSuffix.text = "of " + target.ToString("N0") + " steps";
            if (homeSteps.sizeDelta.x > 0) PlaceStepSuffix();
            homeFill.fillAmount = Mathf.Clamp01(saved.Steps / (float)target);
            homeStatus.text = !today ? "Check your device date to keep walking." : ready ? "Your box is ready!"
                : walking ? "Opens at " + target.ToString("N0") + " steps." : "Opened today. " + NextBoxIn();

            // Live shelf peek: the first three spots and the moment the first neighbours share.
            for (int i = 0; i < homePeekToys.Length; i++)
            {
                string id = PeekId(i);
                homePeekToys[i].transform.parent.parent.gameObject.SetActive(id != null);
                SetPicture(homePeekToys[i], id != null ? ScenePortrait(id, false) : null);
            }
            ShelfMoment moment = null;
            for (int i = 0; i + 1 < homePeekToys.Length && moment == null; i++)
                if (PeekId(i) != null && PeekId(i + 1) != null) moment = PersonalityBook.FindMoment(PeekId(i), PeekId(i + 1), false, ShelfFriendships.Level(PeekId(i), PeekId(i + 1)));
            homePeekChip.gameObject.SetActive(moment != null);
            if (moment != null) homePeekChipText.text = moment.Caption;
            homePeekEmpty.gameObject.SetActive(liveShelf.Count == 0);

            RefreshHomeActivity();
            RefreshHomeCollections();
            if (page == AppPage.Home && subtitle != null) subtitle.text = HomeLine();
        }

        private void RefreshHomeActivity()
        {
            activityFeed.Observe(session.OwnedCounts);
            for (int i = 0; i < 3; i++)
            {
                bool on = (int)homeScope == i;
                homeScopeFaces[i].color = on ? Primary : Quiet;
                homeScopeChips[i].GetComponentInChildren<Text>().color = on ? PockleTheme.OnPlum : Ink;
            }
            var items = activityFeed.Items(homeScope);
            bool show = items.Count > 0;
            homeActivityViewport.gameObject.SetActive(show);
            homeActivityEmpty.gameObject.SetActive(!show);
            homeActivityTitle.text = homeScope == ActivityScope.Nearby ? "Nearby" : homeScope == ActivityScope.Friends ? "Friends' activity" : "Everyone";
            int shown = Mathf.Min(items.Count, 12);
            while (activityCards.Count < shown) activityCards.Add(BuildActivityCard());
            for (int i = 0; i < activityCards.Count; i++)
            {
                if (i < shown) ShowActivityCard(activityCards[i], items[i], i);
                else activityCards[i].Root.gameObject.SetActive(false);
            }
            homeActivityContent.sizeDelta = new Vector2(shown * (ActivityCardWidth + 10) + 30, ActivityCardHeight);
            switch (homeScope)
            {
                case ActivityScope.Nearby:
                    homeActivityEmptyTitle.text = "Collectors near you";
                    homeActivityEmptyLine.text = "See what people around you find and fuse, once Pockle accounts are ready. It will only use your town, never your exact spot.";
                    homeActivityEmptyAction.gameObject.SetActive(false);
                    break;
                case ActivityScope.Friends:
                    homeActivityEmptyTitle.text = "Follow other collectors";
                    homeActivityEmptyLine.text = "See what they find, fuse and earn.";
                    homeActivityEmptyAction.gameObject.SetActive(true);
                    break;
                default:
                    homeActivityEmptyTitle.text = "Everyone's finds";
                    homeActivityEmptyLine.text = "New toys, fusions and badges from every collector show here. Yours will too, once you open a box.";
                    homeActivityEmptyAction.gameObject.SetActive(false);
                    break;
            }
        }

        private void RefreshHomeCollections()
        {
            // Keep collecting: collections you can collect from, closest to complete first; finished ones last.
            var collections = new List<CollectionDefinition>();
            foreach (var collection in ToyCatalog.Collections)
            {
                bool available = false;
                foreach (string member in collection.Members) if (ToyCatalog.TryGetCollectible(member, out var toy) && toy.Available) available = true;
                if (available) collections.Add(collection);
            }
            collections.Sort((a, b) =>
            {
                int fa = FoundCount(a), fb = FoundCount(b);
                bool da = fa == a.Members.Count, db = fb == b.Members.Count;
                if (da != db) return da ? 1 : -1;
                return fb.CompareTo(fa);
            });
            while (collectCards.Count < collections.Count) collectCards.Add(BuildCollectCard());
            for (int i = 0; i < collectCards.Count; i++)
            {
                var card = collectCards[i];
                bool shown = i < collections.Count;
                card.Button.gameObject.SetActive(shown);
                if (!shown) continue;
                var collection = collections[i];
                int found = FoundCount(collection), total = collection.Members.Count;
                card.Button.name = "Home collection · " + collection.Id;
                LeftLabel(card.Button.GetComponent<RectTransform>(), new Vector2(i * (CollectWidth + 10), 0), new Vector2(CollectWidth, CollectHeight));
                SetPicture(card.Cover, CollectionLook.Banner(collection));
                card.Cover.enabled = card.Cover.texture != null;
                card.Cover.uvRect = CollectionLook.Cover(card.Cover.texture, CollectWidth / CollectHeight, .5f, .45f);
                card.Box.texture = CollectionLook.Box(collection, out _); card.Box.enabled = card.Box.texture != null;
                card.Name.text = collection.DisplayName;
                card.Status.text = found == 0 ? total + " to find" : found == total ? found + " of " + total + " · Complete" : found + " of " + total + " · " + (total - found) + " to go";
                card.Fill.fillAmount = total == 0 ? 0 : found / (float)total;
                card.Button.GetComponent<Image>().color = CatalogUnityColor(collection.FieldColor);
                string id = collection.Id;
                card.Button.onClick.RemoveAllListeners();
                card.Button.onClick.AddListener(() => OpenCollection(id));
            }
            homeCollectContent.sizeDelta = new Vector2(collections.Count * (CollectWidth + 10) + 30, CollectHeight);
            homeCollectTitle.text = FoundTotal() == 0 ? "Collections to discover" : "Keep collecting";

            // New this season: the last few unnumbered collections in the catalog that have toys and a painting.
            var fresh = new List<CollectionDefinition>();
            var all = ToyCatalog.Collections;
            for (int i = all.Count - 1; i >= 0 && fresh.Count < homeNewBoxes.Length; i--)
            {
                var c = all[i];
                if (c.SeriesNumber > 0 || CollectionLook.Banner(c) == null || !collections.Contains(c)) continue;
                fresh.Insert(0, c);
            }
            homeNewBanner.gameObject.SetActive(fresh.Count > 0);
            if (fresh.Count == 0) return;
            SetPicture(homeNewCover, CollectionLook.Banner(fresh[0]));
            homeNewName.text = fresh.Count > 1 ? "New collections" : fresh[0].DisplayName;
            var names = new List<string>(); foreach (var c in fresh) names.Add(c.DisplayName);
            homeNewLine.text = names.Count > 1 ? string.Join(", ", names.GetRange(0, names.Count - 1).ToArray()) + " and " + names[names.Count - 1] : "A new collection";
            for (int i = 0; i < homeNewBoxes.Length; i++)
            {
                bool has = i < fresh.Count;
                homeNewBoxes[i].transform.parent.gameObject.SetActive(has);
                if (has) SetPicture(homeNewBoxes[i], CollectionLook.Box(fresh[i], out _));
            }
        }

        private int FoundTotal()
        {
            int found = 0;
            if (session != null) foreach (var pair in session.OwnedCounts) if (pair.Value > 0) found++;
            return found;
        }

        private static string NextBoxIn()
        {
            var remaining = System.DateTime.UtcNow.Date.AddDays(1) - System.DateTime.UtcNow;
            return "Next box in " + (int)remaining.TotalHours + "h " + remaining.Minutes + "m.";
        }

        /// <summary>
        /// Sets a picture and hides it while there is none: an empty RawImage draws a white square (owner, October 17:
        /// white boxes where art hadn't loaded).
        /// </summary>
        private static void SetPicture(RawImage image, Texture texture)
        {
            if (image == null) return;
            image.texture = texture;
            image.enabled = texture != null;
        }
    }
}
