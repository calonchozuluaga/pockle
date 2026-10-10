using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// v2 You (UX-032): avatar and editable name, a privacy line, three stats, avatar choice, your favorite toy,
    /// and quiet rows for badges and cloud save. Settings opens from the header gear.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private InputField profileName;
        private RectTransform identity, avatarCircle, privacyNote, statsRow, avatarHeading, avatarRow, favoriteHeading, profileLinks;
        private RawImage profilePortrait, favoritePortrait;
        private Image avatarCircleFace, favoriteFace;
        private readonly Text[] statValues = new Text[3];
        private readonly RectTransform[] statTiles = new RectTransform[3];
        private readonly Button[] avatarButtons = new Button[4];
        private readonly Image[] avatarColours = new Image[4];
        private Button playFavorite;
        private Text favoriteName, favoriteHint;

        private void BuildProfile()
        {
            identity = Rect("Your local identity", profileRoot);
            avatarCircle = Rect("Your avatar", identity); avatarCircleFace = Surface(avatarCircle, PockleTheme.FieldPeach, 48);
            avatarCircle.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            profilePortrait = Portrait(avatarCircle, 0); Stretch(profilePortrait.rectTransform);
            Label("Your name", identity, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 18));
            var field = Rect("Display name", identity);
            var background = Surface(field, Quiet, 14); background.raycastTarget = true;
            var text = Label("", field, 22, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            text.resizeTextForBestFit = false; // InputField carets misalign with best-fit text.
            Stretch(text.rectTransform); text.rectTransform.offsetMin = new Vector2(14, 2); text.rectTransform.offsetMax = new Vector2(-14, -2);
            profileName = field.gameObject.AddComponent<InputField>(); profileName.targetGraphic = background;
            profileName.textComponent = text; profileName.characterLimit = 60; profileName.lineType = InputField.LineType.SingleLine;
            profileName.text = profile.Name;
            profileName.onEndEdit.AddListener(_ => CommitProfile());
            privacyNote = Label("Your shelf is private. Only you can see it until public shelves arrive.", profileRoot, PockleTheme.CaptionSize,
                MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(350, 40)).rectTransform;

            statsRow = Rect("Your numbers", profileRoot);
            string[] statNames = { "toys", "Pip finishes", "spares" };
            for (int i = 0; i < statTiles.Length; i++)
            {
                statTiles[i] = Rect(statNames[i], statsRow); Surface(statTiles[i], Quiet, PockleTheme.RowRadius);
                statValues[i] = Label("0", statTiles[i], 28, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(100, 34));
                LeftLabel(statValues[i].rectTransform, new Vector2(14, -12), new Vector2(100, 34));
                var name = Label(statNames[i], statTiles[i], PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(100, 20));
                LeftLabel(name.rectTransform, new Vector2(14, -48), new Vector2(100, 20));
            }

            avatarHeading = Heading("Your avatar", profileRoot);
            avatarRow = Rect("Choose your avatar", profileRoot);
            for (int i = 0; i < avatarButtons.Length; i++)
            {
                PipVariant choice = (PipVariant)i;
                avatarButtons[i] = CreateButton(ToyName(choice), avatarRow, Vector2.zero, new Vector2(64, 64),
                    PockleTheme.FieldFor(PipVariants.CollectibleId(choice)), () => { profile.SetAvatar(choice); RefreshProfile(); RefreshAvatarButton(); },
                    out avatarColours[i], 12);
                avatarButtons[i].gameObject.name = "Avatar · " + PipVariants.Label(choice);
                avatarButtons[i].GetComponentInChildren<Text>().enabled = false;
                avatarButtons[i].gameObject.AddComponent<Mask>().showMaskGraphic = true;
                var picture = Portrait(avatarButtons[i].transform, i); Stretch(picture.rectTransform);
            }

            favoriteHeading = Heading("Favorite", profileRoot);
            playFavorite = CreateButton("Play with your favorite", profileRoot, Vector2.zero, new Vector2(350, 96), PockleTheme.FieldPeach,
                () => ShowToy(profile.FavoriteId), out favoriteFace, PockleTheme.BodySize, false);
            SetRadius(favoriteFace, 26);
            playFavorite.GetComponentInChildren<Text>().enabled = false;
            favoritePortrait = Portrait(playFavorite.transform, 0);
            favoriteName = Label("", playFavorite.transform, 19, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 26));
            favoriteHint = Label("Tap to play", playFavorite.transform, PockleTheme.CaptionSize, PockleTheme.PlumSoft, FontStyle.Normal,
                TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 20));
            var next = AddIcon(playFavorite.transform, "Next", 20, Ink);
            next.anchorMin = next.anchorMax = next.pivot = new Vector2(1, .5f); next.anchoredPosition = new Vector2(-18, 0);

            profileLinks = Rect("Your profile links", profileRoot);
            RowButton("Badges", "Coming later", "Heart", profileLinks,
                () => ShowDialog("Your future badge cabinet", "Earned badges will live here once their rules and rewards are ready. No badges have been awarded in this beta."), out _, true);
            RowButton("Cloud save", "Saved on this device", "Shelf", profileLinks,
                () => ShowDialog("Your shelf stays here for now", "Accounts and cloud save are coming later. Your toys and profile are saved on this device. Clearing app data or uninstalling can erase them."), out _, true);
            RefreshProfile();
        }

        private void CommitProfile()
        {
            if (profileName == null || profileName.text == profile.Name) return;
            profile.SetName(profileName.text); profileName.SetTextWithoutNotify(profile.Name);
            if (page == AppPage.Home) heading.text = Greeting() + ", " + profile.Name;
        }

        private void RefreshProfile()
        {
            if (profilePortrait == null) return;
            int avatar = Mathf.Clamp((int)profile.Avatar, 0, previews.Length - 1);
            profilePortrait.texture = previews[avatar];
            avatarCircleFace.color = PockleTheme.FieldFor(profile.AvatarId);
            for (int i = 0; i < avatarButtons.Length; i++)
            {
                // The selected avatar is outlined: a plum ring shows around a slightly inset portrait.
                bool selected = avatar == i;
                avatarColours[i].color = selected ? Ink : PockleTheme.FieldFor(PipVariants.CollectibleId((PipVariant)i));
                var picture = avatarButtons[i].GetComponentInChildren<RawImage>().rectTransform;
                picture.offsetMin = selected ? new Vector2(4, 4) : Vector2.zero; picture.offsetMax = selected ? new Vector2(-4, -4) : Vector2.zero;
            }
            int favorite = Mathf.Clamp((int)profile.Favorite, 0, previews.Length - 1);
            favoritePortrait.texture = previews[favorite];
            favoriteFace.color = PockleTheme.FieldFor(profile.FavoriteId);
            favoriteName.text = ToyName((PipVariant)favorite);
            bool playable = session == null || session.IsOwnedAndAvailable(profile.FavoriteId);
            playFavorite.interactable = playable;
            favoriteHint.text = playable ? "Tap to play" : "Find it again to play";
            if (session != null)
            {
                CollectionTotals(out int distinct, out int total);
                statValues[0].text = total.ToString("N0");
                statValues[1].text = distinct + "/" + PipVariants.Count;
                int spares = 0;
                for (int i = 0; i < PipVariants.Count; i++) spares += Mathf.Max(0, Owned((PipVariant)i) - 1);
                statValues[2].text = spares.ToString("N0");
            }
        }

        private float LayoutProfile(float width)
        {
            float w = Mathf.Min(width, 480);
            float left = (width - w) / 2;
            float y = 4;
            LeftLabel(identity, new Vector2(left, -y), new Vector2(w, 96));
            LeftLabel(avatarCircle, Vector2.zero, new Vector2(96, 96));
            var labels = identity.GetComponentsInChildren<Text>();
            LeftLabel(labels[0].rectTransform, new Vector2(112, -12), new Vector2(w - 112, 18));
            LeftLabel(profileName.GetComponent<RectTransform>(), new Vector2(112, -34), new Vector2(Mathf.Min(220, w - 112), 44));
            y += 96 + 12;
            LeftLabel(privacyNote, new Vector2(left, -y), new Vector2(w, 40));
            y += 40 + 10;
            const float gap = 10;
            float stat = (w - gap * 2) / 3;
            LeftLabel(statsRow, new Vector2(left, -y), new Vector2(w, 80));
            for (int i = 0; i < statTiles.Length; i++) LeftLabel(statTiles[i], new Vector2(i * (stat + gap), 0), new Vector2(stat, 80));
            y += 80 + 24;
            LeftLabel(avatarHeading, new Vector2(left, -y), new Vector2(w, 30));
            y += 40;
            LeftLabel(avatarRow, new Vector2(left, -y), new Vector2(w, 64));
            float circle = Mathf.Min(64, (w - gap * 3) / 4);
            for (int i = 0; i < avatarButtons.Length; i++)
            {
                var rect = avatarButtons[i].GetComponent<RectTransform>();
                LeftLabel(rect, new Vector2(i * (circle + gap), 0), new Vector2(circle, circle));
                SetRadius(avatarColours[i], circle / 2);
            }
            y += circle + 24;
            LeftLabel(favoriteHeading, new Vector2(left, -y), new Vector2(w, 30));
            y += 40;
            var card = playFavorite.GetComponent<RectTransform>();
            LeftLabel(card, new Vector2(left, -y), new Vector2(w, 96));
            LeftLabel(favoritePortrait.rectTransform, new Vector2(12, -10), new Vector2(76, 76));
            LeftLabel(favoriteName.rectTransform, new Vector2(102, -24), new Vector2(w - 150, 26));
            LeftLabel(favoriteHint.rectTransform, new Vector2(102, -52), new Vector2(w - 150, 20));
            y += 96 + 24;
            LeftLabel(profileLinks, new Vector2(left, -y), new Vector2(w, 130));
            for (int i = 0; i < profileLinks.childCount; i++)
                LeftLabel((RectTransform)profileLinks.GetChild(i), new Vector2(0, -i * 70), new Vector2(w, 60));
            y += 130;
            TopCentered(profileRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }
    }
}
