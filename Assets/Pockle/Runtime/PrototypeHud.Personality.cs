using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Toy details page (owner-approved canvas board "Pockle details, more visual", October 15): the toy poses on its
    /// colour field; the sheet shows its name, trait badges, three personality sliders, its nature plus its finish's touch,
    /// how it reacts as icon tiles, shelf-friend cards, and the family as a swipeable strip. Full-bleed: the page draws its
    /// own gutters. All words are authored (<see cref="PersonalityBook"/>); nothing is generated at runtime.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private const float PersonalitySheetTop = 330f;
        private string personalityId = PersonalityBook.Peach;
        private RectTransform personalityToy, personalityShadow, personalitySheet, personalityChips, personalityFriendsRow;
        private RawImage personalityPicture;
        private PersonalityMotion personalityMotion;
        private Button personalityVoice, personalityPlay;
        private Text personalityEyebrow, personalityName, personalityLine, personalityVoiceLabel;
        private RectTransform personalityReactsHeading, personalityFriendsHeading;
        private Text personalityFriendsNote;
        private readonly List<RectTransform> personalitySparkles = new List<RectTransform>();
        private AudioSource voiceSource;

        // Trait badges: a white circle with a sparkle, then the word, on a soft colour.
        private readonly List<Text> chipTexts = new List<Text>();
        private readonly List<Image> chipFaces = new List<Image>();
        private readonly List<RectTransform> chipDots = new List<RectTransform>();

        // "Its personality": the temperament badge, its one-line signature, then four read-only bars.
        private RectTransform meterCard, temperamentBadge;
        private Text meterHeading, meterNote, temperamentText;
        // Read-only bars (owner, October 15: a round handle made them look draggable).
        private sealed class Meter { public Text Left, Name, Right; public RectTransform Track; public Image Fill; }
        private readonly Meter[] meters = new Meter[4];
        // The four dials in docs/PERSONALITY_SYSTEM.md order: energy, mood ("Grumpy ↔ Sweet", owner default), boldness, bedtime.
        private static readonly string[,] MeterWords = { { "Sleepy", "Energy", "Bouncy" }, { "Grumpy", "Mood", "Sweet" }, { "Shy", "With friends", "Chatty" }, { "Early bird", "Bedtime", "Night owl" } };
        private static readonly Color[] MeterColours = { new Color32(0xF2, 0xA0, 0x7B, 0xFF), new Color32(0xF0, 0xB8, 0x3C, 0xFF), new Color32(0xEC, 0x7F, 0xA6, 0xFF), new Color32(0x9C, 0x8C, 0xE0, 0xFF) };
        /// <summary>Badge colours in <see cref="Temperament"/> order (Sunny, Grumpy, Shy, Show-off, Dreamy, Sleepy, Mischief, Calm).</summary>
        private static readonly Color[] TemperamentColours =
        {
            new Color32(0xFF, 0xD3, 0x6B, 0xFF), new Color32(0xB9, 0xC3, 0xD6, 0xFF), new Color32(0xFF, 0xC4, 0xD6, 0xFF), new Color32(0xFF, 0xA9, 0x8A, 0xFF),
            new Color32(0xC9, 0xBE, 0xF5, 0xFF), new Color32(0xB8, 0xD8, 0xF2, 0xFF), new Color32(0xA9, 0xE0, 0xB4, 0xFF), new Color32(0xBF, 0xE8, 0xDA, 0xFF)
        };

        // Nature + touch.
        private RectTransform natureCard, touchCard, plusBadge;
        private Text natureTitle, natureLine, natureDetail, touchTitle, touchLine, touchDetail;

        // How it reacts: up to four icon tiles.
        private readonly RectTransform[] reactionTiles = new RectTransform[4];
        private readonly Image[] reactionFaces = new Image[4];
        private readonly RawImage[] reactionIcons = new RawImage[4];
        private readonly Text[] reactionWhen = new Text[4], reactionDoes = new Text[4];
        private static readonly Color[] ReactionColours = { new Color32(0xFF, 0xE7, 0xCF, 0xFF), new Color32(0xFD, 0xE0, 0xE8, 0xFF), new Color32(0xE4, 0xE8, 0xFF, 0xFF), new Color32(0xDD, 0xF2, 0xE6, 0xFF) };

        // Shelf friends: three cards.
        private readonly Image[] friendFaces = new Image[3];
        private readonly RawImage[] friendPictures = new RawImage[3];
        private readonly CharacterSilhouetteGraphic[] friendOutlines = new CharacterSilhouetteGraphic[3];
        private readonly Text[] friendNames = new Text[3], friendStatus = new Text[3], friendLabels = new Text[3];
        private readonly RectTransform[] friendBadges = new RectTransform[3];
        private readonly Image[] friendBadgeFaces = new Image[3];
        private readonly RawImage[] friendBadgeStars = new RawImage[3];
        private readonly Text[] friendBadgeMarks = new Text[3];

        // The family: a sideways strip of small pictures.
        private RectTransform familyHeading, familyViewport, familyContent;
        private Text familyNote;
        private sealed class FamilyTile { public Button Button; public Image Face, Ring; public RawImage Picture; public CharacterSilhouetteGraphic Outline; public Text Name, Trait; }
        private readonly List<FamilyTile> familyTiles = new List<FamilyTile>();
        private const float FamilySize = 58, FamilyStep = 74;

        private void BuildPersonality()
        {
            personalityVoice = CreateButton("Voice", personalityRoot, Vector2.zero, new Vector2(140, 44), new Color(1, 1, 1, .6f), PlayVoice, out _, 14);
            personalityVoice.name = "Personality voice";
            personalityVoiceLabel = personalityVoice.GetComponentInChildren<Text>();

            personalityShadow = Rect("Contact shadow", personalityRoot); Surface(personalityShadow, new Color(.59f, .39f, .08f, .18f), 15);
            personalityToy = Rect("Personality toy", personalityRoot);
            var motion = Rect("Personality motion", personalityToy); Stretch(motion); motion.pivot = new Vector2(.5f, SceneBase);
            personalityMotion = motion.gameObject.AddComponent<PersonalityMotion>();
            personalityMotion.Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Show);
            var picture = Rect("Portrait", motion); Stretch(picture);
            personalityPicture = picture.gameObject.AddComponent<RawImage>(); personalityPicture.raycastTarget = false;
            personalitySparkles.Add(Sparkle(personalityRoot, Color.white, 0f));
            personalitySparkles.Add(Sparkle(personalityRoot, new Color32(0xE0, 0xA9, 0x2E, 0xFF), .4f));

            personalitySheet = Rect("Personality sheet", personalityRoot); Surface(personalitySheet, Paper, 32);
            personalityEyebrow = Label("", personalitySheet, PockleTheme.SmallSize - 1, PockleTheme.JellyInk, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 18));
            personalityName = Label("", personalitySheet, 32, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 38));
            personalityLine = Label("", personalitySheet, PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(300, 40));

            personalityChips = Rect("Traits", personalitySheet);
            for (int i = 0; i < 3; i++)
            {
                var chip = Rect("Trait", personalityChips); chipFaces.Add(Surface(chip, Quiet, 20));
                var dot = Rect("Trait dot", chip); Surface(dot, Color.white, 15); chipDots.Add(dot);
                var star = Rect("Trait sparkle", dot); Stretch(star); star.offsetMin = new Vector2(8, 8); star.offsetMax = new Vector2(-8, -8);
                var starImage = star.gameObject.AddComponent<RawImage>(); starImage.texture = UiShapes.Sparkle; starImage.color = Ink; starImage.raycastTarget = false;
                var text = Label("", chip, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
                text.resizeTextForBestFit = false; text.horizontalOverflow = HorizontalWrapMode.Overflow;
                chipTexts.Add(text);
            }

            meterCard = Rect("Its personality", personalitySheet); Surface(meterCard, Color.white, 24);
            meterHeading = Label("Its personality", meterCard, 17, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            meterNote = Label("", meterCard, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            temperamentBadge = Rect("Temperament", meterCard); Surface(temperamentBadge, TemperamentColours[0], 15);
            var badgeStar = Rect("Temperament sparkle", temperamentBadge);
            LeftLabel(badgeStar, new Vector2(10, -8), new Vector2(14, 14));
            var badgeStarImage = badgeStar.gameObject.AddComponent<RawImage>(); badgeStarImage.texture = UiShapes.Sparkle; badgeStarImage.color = Ink; badgeStarImage.raycastTarget = false;
            temperamentText = Label("", temperamentBadge, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            temperamentText.horizontalOverflow = HorizontalWrapMode.Overflow;
            for (int i = 0; i < meters.Length; i++)
            {
                var m = new Meter();
                m.Left = Label(MeterWords[i, 0], meterCard, PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
                m.Name = Label(MeterWords[i, 1], meterCard, PockleTheme.SmallSize - 1, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                m.Right = Label(MeterWords[i, 2], meterCard, PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleRight, Vector2.zero, Vector2.zero);
                m.Track = Rect("Meter · " + MeterWords[i, 1], meterCard); Surface(m.Track, Quiet, 7);
                m.Fill = ProgressFill(m.Track, MeterColours[i]);
                meters[i] = m;
            }

            natureCard = Rect("Nature card", personalitySheet); Surface(natureCard, Quiet, 22);
            natureTitle = Label("", natureCard, 11, PockleTheme.JellyInk, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            natureLine = Label("", natureCard, 16, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            natureDetail = Label("", natureCard, 12, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            touchCard = Rect("Finish card", personalitySheet); Surface(touchCard, PockleTheme.Confetti, 22);
            touchTitle = Label("", touchCard, 11, PockleTheme.JellyInk, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            touchLine = Label("", touchCard, 16, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            touchDetail = Label("", touchCard, 12, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            foreach (var text in new[] { natureLine, touchLine, natureDetail, touchDetail }) { text.verticalOverflow = VerticalWrapMode.Overflow; text.resizeTextMinSize = 10; }
            // The "+" between the cards: this toy is its character's nature plus its finish's touch.
            plusBadge = Rect("Nature plus touch", personalitySheet); Surface(plusBadge, Ink, 13);
            var plus = Label("+", plusBadge, 18, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(plus.rectTransform);

            personalityReactsHeading = Label("How it reacts", personalitySheet, 18, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 26)).rectTransform;
            for (int i = 0; i < reactionTiles.Length; i++)
            {
                reactionTiles[i] = Rect("Reaction", personalitySheet); reactionFaces[i] = Surface(reactionTiles[i], ReactionColours[i], 20);
                var circle = Rect("Reaction icon", reactionTiles[i]); Surface(circle, new Color(1, 1, 1, .78f), 12);
                LeftLabel(circle, new Vector2(12, -12), new Vector2(36, 36));
                var icon = Rect("Icon", circle); Stretch(icon); icon.offsetMin = new Vector2(8, 8); icon.offsetMax = new Vector2(-8, -8);
                reactionIcons[i] = icon.gameObject.AddComponent<RawImage>(); reactionIcons[i].color = Ink; reactionIcons[i].raycastTarget = false;
                reactionWhen[i] = Label("", reactionTiles[i], PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
                reactionDoes[i] = Label("", reactionTiles[i], PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
                reactionDoes[i].verticalOverflow = VerticalWrapMode.Overflow;
            }

            personalityFriendsHeading = Label("Shelf friends", personalitySheet, 18, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 26)).rectTransform;
            personalityFriendsNote = Label("", personalitySheet, PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleRight, Vector2.zero, new Vector2(200, 26));
            personalityFriendsRow = Rect("Shelf friends", personalitySheet);
            for (int i = 0; i < friendFaces.Length; i++)
            {
                var tile = Rect("Shelf friend", personalityFriendsRow); friendFaces[i] = Surface(tile, PockleTheme.Mystery, 20);
                var art = Rect("Friend", tile);
                friendPictures[i] = art.gameObject.AddComponent<RawImage>(); friendPictures[i].raycastTarget = false;
                var outline = Rect("Friend outline", tile);
                friendOutlines[i] = outline.gameObject.AddComponent<CharacterSilhouetteGraphic>(); friendOutlines[i].color = PockleTheme.PlumSoft;
                friendOutlines[i].Configure("pip");
                friendNames[i] = Label("", tile, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                friendStatus[i] = Label("", tile, 11, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                friendLabels[i] = Label("", tile, 12, Ink, FontStyle.Bold, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
                friendLabels[i].verticalOverflow = VerticalWrapMode.Overflow;
                friendBadges[i] = Rect("Moment badge", tile); friendBadgeFaces[i] = Surface(friendBadges[i], Ink, 11);
                var star = Rect("Badge star", friendBadges[i]); Stretch(star); star.offsetMin = new Vector2(5, 5); star.offsetMax = new Vector2(-5, -5);
                friendBadgeStars[i] = star.gameObject.AddComponent<RawImage>(); friendBadgeStars[i].texture = UiShapes.Sparkle; friendBadgeStars[i].color = PockleTheme.FieldGold; friendBadgeStars[i].raycastTarget = false;
                friendBadgeMarks[i] = Label("?", friendBadges[i], 12, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(friendBadgeMarks[i].rectTransform);
            }

            familyHeading = Label("The family", personalitySheet, 18, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 26)).rectTransform;
            familyNote = Label("", personalitySheet, PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleRight, Vector2.zero, new Vector2(120, 26));
            familyViewport = Rect("Family strip", personalitySheet);
            familyViewport.gameObject.AddComponent<RectMask2D>();
            var hit = familyViewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var scroll = familyViewport.gameObject.AddComponent<ScrollRect>(); scroll.vertical = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            familyContent = Rect("Family tiles", familyViewport);
            familyContent.anchorMin = familyContent.anchorMax = familyContent.pivot = new Vector2(0, 1);
            scroll.content = familyContent; scroll.viewport = familyViewport;

            personalityPlay = CreateButton("Play", personalitySheet, Vector2.zero, new Vector2(300, 52), PockleTheme.Plum, () => ShowToy(personalityId), out _);
        }

        private FamilyTile BuildFamilyTile(int index)
        {
            var tile = new FamilyTile();
            float x = 20 + index * FamilyStep; // starts at the page gutter; the strip itself runs edge to edge
            var ring = Rect("Family ring", familyContent); tile.Ring = Surface(ring, Ink, 21);
            LeftLabel(ring, new Vector2(x - 3, 0), new Vector2(FamilySize + 6, FamilySize + 6));
            tile.Button = CreateButton("", familyContent, Vector2.zero, new Vector2(FamilySize, FamilySize), PockleTheme.Mystery, () =>
            {
                string id = familyTiles[index].Button.name.Substring("Family · ".Length);
                if (id != personalityId) OpenPersonality(id);
            }, out tile.Face, PockleTheme.SmallSize, false);
            SetRadius(tile.Face, 18);
            tile.Button.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var colours = tile.Button.colors; colours.disabledColor = Color.white; tile.Button.colors = colours;
            LeftLabel(tile.Button.GetComponent<RectTransform>(), new Vector2(x, -3), new Vector2(FamilySize, FamilySize));
            var outline = Rect("Outline", tile.Button.transform);
            outline.anchorMin = new Vector2(.14f, .1f); outline.anchorMax = new Vector2(.86f, .9f); outline.offsetMin = outline.offsetMax = Vector2.zero;
            tile.Outline = outline.gameObject.AddComponent<CharacterSilhouetteGraphic>();
            tile.Outline.color = new Color(PockleTheme.PlumSoft.r, PockleTheme.PlumSoft.g, PockleTheme.PlumSoft.b, .45f);
            var picture = Rect("Portrait", tile.Button.transform); Stretch(picture);
            tile.Picture = picture.gameObject.AddComponent<RawImage>(); tile.Picture.raycastTarget = false;
            TintMirror.Follow(tile.Button, tile.Picture);
            tile.Name = Label("", familyContent, 11, Ink, FontStyle.Bold, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            LeftLabel(tile.Name.rectTransform, new Vector2(x - 8, -(FamilySize + 10)), new Vector2(FamilySize + 16, 14));
            tile.Trait = Label("", familyContent, 10, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            LeftLabel(tile.Trait.rectTransform, new Vector2(x - 8, -(FamilySize + 25)), new Vector2(FamilySize + 16, 14));
            return tile;
        }

        private void OpenPersonality(string id)
        {
            if (!PersonalityBook.TryGet(id, out _) || session == null || session.GetOwnedCount(id) <= 0) return;
            personalityId = id;
            if (page == AppPage.Personality) { RefreshPersonality(); AdaptLayout(); return; } // a family tile swaps the toy in place
            Navigate(AppPage.Personality);
            RefreshPersonality();
        }

        /// <summary>The reaction's icon, from what triggers it.</summary>
        private static string ReactionIcon(string when)
        {
            string w = (when ?? "").ToLowerInvariant();
            if (w.Contains("night")) return "ReactNight";
            if (w.Contains("squish") || w.Contains("squeeze") || w.Contains("press")) return "ReactSquish";
            if (w.Contains("pick") || w.Contains("lift")) return "ReactLift";
            if (w.Contains("left") || w.Contains("while") || w.Contains("wait")) return "ReactWait";
            if (w.Contains("turn") || w.Contains("spin")) return "ReactTurn";
            return "ReactPoke";
        }

        private void RefreshPersonality()
        {
            if (personalityName == null || !PersonalityBook.TryGet(personalityId, out var personality)) return;
            string name = PersonalityBook.ShortName(personalityId);
            personalityName.text = name;
            bool known = ToyCatalog.TryGetCollectible(personalityId, out var toy) && ToyCatalog.TryGetCollection(toy.CollectionId, out var family);
            ToyCatalog.TryGetCollection(known ? toy.CollectionId : "pip", out family);
            Color field = PockleTheme.FieldFor(personalityId);
            Color deep = family != null ? CollectionLook.Ink(family) : PockleTheme.JellyInk;
            int number = 0;
            if (family != null) for (int i = 0; i < family.Members.Count; i++) if (family.Members[i] == personalityId) number = i + 1;
            string series = family != null && family.SeriesNumber > 0 ? " · Series " + family.SeriesNumber : "";
            personalityEyebrow.text = ((family != null ? family.DisplayName : "Pip") + series + (number > 0 ? " · No. " + number : "")).ToUpperInvariant();
            personalityEyebrow.color = deep;
            personalityLine.text = personality.Summary + ".";
            personalityVoiceLabel.text = personality.Voice;
            personalityVoice.name = "Voice · " + personality.Voice;
            personalityPicture.texture = CutoutPortrait(personalityId); // tilts with its personality, so no square behind it
            personalityMotion.SetStyle(personality.Motion);
            foreach (var sparkle in personalitySparkles) sparkle.gameObject.SetActive(personality.Motion == PersonalityMotionStyle.Show);
            personalityPlay.GetComponentInChildren<Text>().text = "Play with " + name;
            personalityPlay.interactable = session != null && session.IsOwnedAndAvailable(personalityId);

            // Badges: the character's two traits, then the finish's touch.
            Color[] badgeColours = { field, PockleTheme.FieldGold, PockleTheme.Confetti };
            for (int i = 0; i < chipTexts.Count; i++)
            {
                string text = i < 2 ? (i < personality.Traits.Count ? personality.Traits[i] : "") : personality.TouchTrait;
                if (i == 2 && personality.Traits.Count > 0 && text == personality.Traits[0]) text = "";
                chipTexts[i].text = text;
                chipFaces[i].color = badgeColours[i];
                chipFaces[i].gameObject.SetActive(!string.IsNullOrEmpty(text));
            }

            // The four dials and the temperament they add up to.
            var dials = PersonalityBook.Dials(personalityId);
            var temperament = PersonalityBook.TemperamentOf(personalityId);
            for (int i = 0; i < meters.Length; i++)
            {
                meters[i].Fill.fillAmount = Mathf.Clamp(dials[i], 6, 94) / 100f; // never a bare or overflowing track
                meters[i].Track.name = "Meter · " + MeterWords[i, 1] + " · " + dials[i];
            }
            temperamentText.text = PersonalityBook.TemperamentName(temperament);
            temperamentBadge.GetComponent<Image>().color = TemperamentColours[(int)temperament];
            temperamentBadge.name = "Temperament · " + temperamentText.text;
            meterNote.text = PersonalityBook.TemperamentLine(temperament);

            natureTitle.text = ((family != null ? family.DisplayName : "Pip") + " by nature").ToUpperInvariant();
            natureLine.text = personality.Nature;
            natureDetail.text = personality.NatureDetail;
            touchTitle.text = personality.TouchName.ToUpperInvariant();
            touchLine.text = personality.TouchTrait;
            touchDetail.text = personality.TouchDetail;
            natureCard.GetComponent<Image>().color = field;
            touchCard.GetComponent<Image>().color = Color.Lerp(field, Color.white, .5f);
            natureTitle.color = deep; touchTitle.color = deep;

            for (int i = 0; i < reactionTiles.Length; i++)
            {
                bool has = i < personality.Reactions.Count;
                reactionTiles[i].gameObject.SetActive(has);
                if (!has) continue;
                reactionWhen[i].text = personality.Reactions[i].Key; reactionDoes[i].text = personality.Reactions[i].Value;
                reactionIcons[i].texture = PockleTheme.Icon(ReactionIcon(personality.Reactions[i].Key));
                reactionIcons[i].enabled = reactionIcons[i].texture != null;
            }

            // Shelf friends: the toys it shares the live shelf with. Everyone there is owned, so the friend always shows;
            // "found" is about the pair's moment (they have to sit side by side on the shelf).
            int slot = 0, foundCount = 0;
            foreach (string partner in liveShelf)
            {
                if (partner == personalityId || slot >= friendFaces.Length) continue;
                int level = ShelfFriendships.Level(personalityId, partner);
                var moment = PersonalityBook.FindMoment(personalityId, partner, false, level);
                if (moment == null) continue;
                bool found = foundMoments.Contains(moment.Id);
                if (found) foundCount++;
                var picture = StillPortrait(partner);
                friendFaces[slot].color = PockleTheme.FieldFor(partner);
                friendPictures[slot].texture = picture;
                friendPictures[slot].gameObject.SetActive(picture != null);
                friendOutlines[slot].gameObject.SetActive(picture == null);
                if (ToyCatalog.TryGetCollectible(partner, out var partnerToy)) friendOutlines[slot].Configure(partnerToy.CharacterId);
                friendNames[slot].text = PersonalityBook.ShortName(partner).Split(' ')[0];
                // Once they've met, the card names how they get on and how deep the friendship is (1..3).
                friendStatus[slot].text = level > 0 && found ? PersonalityBook.RelationshipName(PersonalityBook.RelationshipOf(personalityId, partner, false)) + " · Lv " + level
                    : found ? "Moment found" : "Moment to find";
                friendLabels[slot].text = found ? moment.LabelFor(personalityId) : "Seat them side by side";
                friendBadgeFaces[slot].color = found ? Ink : new Color(1, 1, 1, .88f);
                friendBadgeStars[slot].enabled = found; friendBadgeMarks[slot].enabled = !found;
                friendFaces[slot].gameObject.name = "Shelf friend · " + partner;
                slot++;
            }
            for (int i = slot; i < friendFaces.Length; i++) friendFaces[i].gameObject.SetActive(false);
            for (int i = 0; i < slot; i++) friendFaces[i].gameObject.SetActive(true);
            personalityFriendsHeading.gameObject.SetActive(slot > 0);
            personalityFriendsNote.gameObject.SetActive(slot > 0);
            personalityFriendsNote.text = foundCount + " of " + slot + (slot == 1 ? " moment found" : " moments found");

            // The family strip: every finish in the collection; ones you haven't found stay outlines.
            int members = family != null ? family.Members.Count : 0, owned = 0;
            while (familyTiles.Count < members) familyTiles.Add(BuildFamilyTile(familyTiles.Count));
            for (int i = 0; i < familyTiles.Count; i++)
            {
                var tile = familyTiles[i];
                bool shown = i < members;
                tile.Button.gameObject.SetActive(shown); tile.Ring.gameObject.SetActive(false);
                tile.Name.gameObject.SetActive(shown); tile.Trait.gameObject.SetActive(shown);
                if (!shown) continue;
                string member = family.Members[i];
                bool has = session != null && session.GetOwnedCount(member) > 0;
                if (has) owned++;
                bool secret = family.SecretId == member;
                tile.Button.name = "Family · " + member;
                tile.Button.interactable = has;
                tile.Face.color = has ? PockleTheme.FieldFor(member) : PockleTheme.Mystery;
                var picture = has ? StillPortrait(member) : null;
                tile.Picture.texture = picture; tile.Picture.enabled = picture != null;
                tile.Outline.enabled = !has;
                if (!has) tile.Outline.Configure(family.CharacterId, secret);
                tile.Ring.gameObject.SetActive(member == personalityId);
                tile.Name.text = has ? PersonalityBook.ShortName(member).Split(' ')[0] : "?";
                tile.Name.color = has ? Ink : MutedInk;
                tile.Trait.text = has && PersonalityBook.TryGet(member, out var p) ? p.TouchTrait : "";
            }
            familyContent.sizeDelta = new Vector2(members * FamilyStep + 40, FamilySize + 44);
            familyNote.text = owned + " of " + members;
            familyNote.gameObject.SetActive(members > 0);
        }

        /// <summary>Full-bleed layout: the toy on its field, then the sheet. Returns the content height.</summary>
        private float LayoutPersonality(float width)
        {
            const float gutter = 20f;
            float inner = Mathf.Min(width - gutter * 2, 440f), left = (width - inner) / 2;
            var voice = personalityVoice.GetComponent<RectTransform>();
            float voiceWidth = Mathf.Max(120, personalityVoiceLabel.preferredWidth + 32);
            LeftLabel(voice, new Vector2(width - 20 - voiceWidth, -16), new Vector2(voiceWidth, 44));

            const float toy = 220f, baseY = 300f;
            LeftLabel(personalityToy, new Vector2(width / 2 - toy / 2, -(baseY + SceneBase * toy - toy)), new Vector2(toy, toy));
            LeftLabel(personalityShadow, new Vector2(width / 2 - 85, -(baseY - 12)), new Vector2(170, 30));
            LeftLabel(personalitySparkles[0], new Vector2(width / 2 - 98, -108), new Vector2(26, 26));
            LeftLabel(personalitySparkles[1], new Vector2(width / 2 + 78, -140), new Vector2(18, 18));

            float y = 22;
            LeftLabel(personalityEyebrow.rectTransform, new Vector2(left, -y), new Vector2(inner, 18)); y += 20;
            LeftLabel(personalityName.rectTransform, new Vector2(left, -y), new Vector2(inner, 38)); y += 42;
            LeftLabel(personalityLine.rectTransform, new Vector2(left, -y), new Vector2(inner, 40)); y += 46;

            // Badges.
            LeftLabel(personalityChips, new Vector2(left, -y), new Vector2(inner, 40));
            float x = 0;
            for (int i = 0; i < chipTexts.Count; i++)
            {
                if (!chipFaces[i].gameObject.activeSelf) continue;
                float chipWidth = Mathf.Min(inner - x, 6 + 30 + 8 + chipTexts[i].preferredWidth + 14);
                LeftLabel(chipFaces[i].rectTransform, new Vector2(x, 0), new Vector2(chipWidth, 40));
                LeftLabel(chipDots[i], new Vector2(5, -5), new Vector2(30, 30));
                LeftLabel(chipTexts[i].rectTransform, new Vector2(42, 0), new Vector2(chipWidth - 48, 40));
                x += chipWidth + 8;
            }
            y += 40 + 16;

            // Its personality.
            float meterInner = inner - 32;
            const float meterRow = 46;
            float meterHeight = 16 + 26 + 26 + meters.Length * meterRow + 10;
            LeftLabel(meterCard, new Vector2(left, -y), new Vector2(inner, meterHeight));
            LeftLabel(meterHeading.rectTransform, new Vector2(16, -14), new Vector2(meterInner * .5f, 26));
            float badgeWidth = 10 + 14 + 6 + temperamentText.preferredWidth + 14;
            LeftLabel(temperamentBadge, new Vector2(16 + meterInner - badgeWidth, -12), new Vector2(badgeWidth, 30));
            LeftLabel(temperamentText.rectTransform, new Vector2(30, 0), new Vector2(badgeWidth - 36, 30));
            LeftLabel(meterNote.rectTransform, new Vector2(16, -46), new Vector2(meterInner, 18));
            for (int i = 0; i < meters.Length; i++)
            {
                var m = meters[i];
                float top = 16 + 26 + 8 + 26 + i * meterRow;
                LeftLabel(m.Left.rectTransform, new Vector2(16, -top), new Vector2(meterInner / 3, 16));
                LeftLabel(m.Name.rectTransform, new Vector2(16 + meterInner / 3, -top), new Vector2(meterInner / 3, 16));
                LeftLabel(m.Right.rectTransform, new Vector2(16 + meterInner * 2 / 3, -top), new Vector2(meterInner / 3, 16));
                LeftLabel(m.Track, new Vector2(16, -(top + 21)), new Vector2(meterInner, 14));
            }
            y += meterHeight + 12;

            // Nature + touch.
            float cardWidth = (inner - 10) / 2;
            natureLine.rectTransform.sizeDelta = new Vector2(cardWidth - 28, 0);
            float cardHeight = 122;
            LeftLabel(natureCard, new Vector2(left, -y), new Vector2(cardWidth, cardHeight));
            LeftLabel(touchCard, new Vector2(left + cardWidth + 10, -y), new Vector2(cardWidth, cardHeight));
            foreach (var card in new[] { (natureTitle, natureLine, natureDetail), (touchTitle, touchLine, touchDetail) })
            {
                LeftLabel(card.Item1.rectTransform, new Vector2(14, -12), new Vector2(cardWidth - 28, 16));
                LeftLabel(card.Item2.rectTransform, new Vector2(14, -30), new Vector2(cardWidth - 28, 40));
                LeftLabel(card.Item3.rectTransform, new Vector2(14, -72), new Vector2(cardWidth - 28, cardHeight - 80));
            }
            LeftLabel(plusBadge, new Vector2(left + cardWidth + 5 - 13, -(y + cardHeight / 2 - 13)), new Vector2(26, 26));
            plusBadge.SetAsLastSibling();
            y += cardHeight + 22;

            // How it reacts: a two-column grid.
            LeftLabel(personalityReactsHeading, new Vector2(left, -y), new Vector2(inner, 26)); y += 34;
            int tiles = 0; foreach (var tile in reactionTiles) if (tile.gameObject.activeSelf) tiles++;
            float tileWidth = (inner - 8) / 2;
            const float tileHeight = 104;
            for (int i = 0; i < reactionTiles.Length; i++)
            {
                LeftLabel(reactionTiles[i], new Vector2(left + (i % 2) * (tileWidth + 8), -(y + (i / 2) * (tileHeight + 8))), new Vector2(tileWidth, tileHeight));
                LeftLabel(reactionWhen[i].rectTransform, new Vector2(12, -54), new Vector2(tileWidth - 24, 16));
                LeftLabel(reactionDoes[i].rectTransform, new Vector2(12, -70), new Vector2(tileWidth - 24, 30));
            }
            int tileRows = (tiles + 1) / 2;
            y += tileRows * (tileHeight + 8) + 14;

            // Shelf friends.
            bool friends = personalityFriendsHeading.gameObject.activeSelf;
            if (friends)
            {
                LeftLabel(personalityFriendsHeading, new Vector2(left, -y), new Vector2(inner * .5f, 26));
                LeftLabel(personalityFriendsNote.rectTransform, new Vector2(left + inner * .4f, -y), new Vector2(inner * .6f, 26)); y += 34;
                const float friendHeight = 156;
                LeftLabel(personalityFriendsRow, new Vector2(left, -y), new Vector2(inner, friendHeight));
                float tile = (inner - 16) / 3;
                for (int i = 0; i < friendFaces.Length; i++)
                {
                    LeftLabel(friendFaces[i].rectTransform, new Vector2(i * (tile + 8), 0), new Vector2(tile, friendHeight));
                    TopCentered(friendPictures[i].rectTransform, new Vector2(0, -12), new Vector2(54, 54));
                    TopCentered(friendOutlines[i].rectTransform, new Vector2(0, -12), new Vector2(54, 54));
                    TopCentered(friendNames[i].rectTransform, new Vector2(0, -72), new Vector2(tile - 8, 18));
                    TopCentered(friendStatus[i].rectTransform, new Vector2(0, -92), new Vector2(tile - 8, 14));
                    TopCentered(friendLabels[i].rectTransform, new Vector2(0, -108), new Vector2(tile - 12, 40));
                    friendBadges[i].anchorMin = friendBadges[i].anchorMax = friendBadges[i].pivot = new Vector2(1, 1);
                    friendBadges[i].anchoredPosition = new Vector2(-8, -8); friendBadges[i].sizeDelta = new Vector2(22, 22);
                }
                y += friendHeight + 22;
            }

            // The family strip runs edge to edge so it can be swiped.
            bool hasFamily = familyNote.gameObject.activeSelf;
            familyHeading.gameObject.SetActive(hasFamily); familyViewport.gameObject.SetActive(hasFamily);
            if (hasFamily)
            {
                LeftLabel(familyHeading, new Vector2(left, -y), new Vector2(inner * .6f, 26));
                LeftLabel(familyNote.rectTransform, new Vector2(left + inner * .6f, -y), new Vector2(inner * .4f, 26)); y += 34;
                LeftLabel(familyViewport, new Vector2(0, -y), new Vector2(width, FamilySize + 50));
                y += FamilySize + 50 + 16;
            }
            LeftLabel(personalityPlay.GetComponent<RectTransform>(), new Vector2(left, -y), new Vector2(inner, 54)); y += 54 + 28;

            LeftLabel(personalitySheet, new Vector2(0, -PersonalitySheetTop), new Vector2(width, y + 400));
            float height = PersonalitySheetTop + y;
            TopCentered(personalityRoot, Vector2.zero, new Vector2(width, height));
            return height;
        }

        private void PlayVoice()
        {
            if (!soundEnabled || !PersonalityBook.TryGet(personalityId, out var personality)) return;
            if (voiceSource == null) { voiceSource = gameObject.AddComponent<AudioSource>(); voiceSource.playOnAwake = false; }
            voiceSource.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("pockle.prototype.soundVolume", 1));
            voiceSource.PlayOneShot(VoiceClip(personality.VoicePitch));
        }

        private readonly Dictionary<float, AudioClip> voiceClips = new Dictionary<float, AudioClip>();

        /// <summary>A tiny synthesized two-note chirp, pitched per personality. Local and placeholder, like the toy sounds.</summary>
        private AudioClip VoiceClip(float pitch)
        {
            if (voiceClips.TryGetValue(pitch, out var clip) && clip != null) return clip;
            const int rate = 22050; int length = (int)(rate * .26f);
            var samples = new float[length];
            double phase = 0;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)rate;
                float note = t < .12f ? 660f : 880f;
                phase += 2 * Mathf.PI * note * pitch / rate;
                float local = t < .12f ? t / .12f : (t - .12f) / .14f;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(local)) * .35f;
                samples[i] = (float)System.Math.Sin(phase) * envelope;
            }
            clip = AudioClip.Create("Pockle voice " + pitch, length, 1, rate, false);
            clip.SetData(samples, 0);
            voiceClips[pitch] = clip;
            return clip;
        }
    }
}
