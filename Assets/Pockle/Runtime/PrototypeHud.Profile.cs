using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private InputField profileName;
        private Text profileSummary;
        private RectTransform identityCard, avatarRow, favoriteCard, profileLinks;
        private RawImage profilePortrait;
        private readonly Button[] avatarButtons = new Button[4];
        private readonly Image[] avatarColours = new Image[4];
        private readonly Button[] favoriteButtons = new Button[4];
        private readonly Image[] favoriteColours = new Image[4];
        private Button playFavorite;

        private void BuildProfile()
        {
            identityCard = Rect("Your local identity", profileRoot); Surface(identityCard, Paper);
            var portrait = Rect("Your avatar", identityCard); TopCentered(portrait, new Vector2(0, -8), new Vector2(110, 110));
            profilePortrait = portrait.gameObject.AddComponent<RawImage>(); profilePortrait.raycastTarget = false;
            Label("DISPLAY NAME", identityCard, 10, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -122), new Vector2(270, 20));
            var field = Rect("Display name", identityCard); TopCentered(field, new Vector2(0, -148), new Vector2(270, 48));
            var background = Surface(field, Quiet); background.raycastTarget = true;
            var text = Label("", field, 18, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            Stretch(text.rectTransform); text.rectTransform.offsetMin = new Vector2(14, 4); text.rectTransform.offsetMax = new Vector2(-14, -4);
            profileName = field.gameObject.AddComponent<InputField>(); profileName.targetGraphic = background;
            profileName.textComponent = text; profileName.characterLimit = 60; profileName.lineType = InputField.LineType.SingleLine;
            profileName.text = profile.Name;
            profileName.onEndEdit.AddListener(_ => CommitProfile());
            profileSummary = Label("", identityCard, 12, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Vector2(0, -206), new Vector2(300, 32));
            Label("Saved on this device. Your profile isn't public yet.", identityCard, 11, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0, -244), new Vector2(280, 38));
            avatarRow = Rect("Choose your avatar", profileRoot);
            Label("CHOOSE YOUR PIP AVATAR", avatarRow, 10, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, 0), new Vector2(300, 24));
            for (int i = 0; i < 4; i++)
            {
                PipVariant choice = (PipVariant)i;
                avatarButtons[i] = CreateButton("", avatarRow, Vector2.zero, new Vector2(70, 78), Quiet,
                    () => { profile.SetAvatar(choice); RefreshProfile(); }, out avatarColours[i]);
                avatarButtons[i].gameObject.name = "Avatar · " + PipVariants.Label(choice);
                var image = Rect("Avatar preview", avatarButtons[i].transform); Stretch(image);
                image.offsetMin = new Vector2(2, 2); image.offsetMax = new Vector2(-2, -2);
                var raw = image.gameObject.AddComponent<RawImage>(); raw.texture = previews[i]; raw.raycastTarget = false;
            }
            favoriteCard = Rect("Favorite toy", profileRoot); Surface(favoriteCard, Paper);
            Label("YOUR FAVORITE TOY", favoriteCard, 11, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -12), new Vector2(270, 24));
            for (int i = 0; i < 4; i++)
            {
                PipVariant choice = (PipVariant)i;
                favoriteButtons[i] = CreateButton(PipVariants.Label(choice), favoriteCard, Vector2.zero, new Vector2(135, 48), Quiet,
                    () => { profile.SetFavorite(choice); RefreshProfile(); }, out favoriteColours[i], 11);
            }
            playFavorite = CreateButton("Play with your favorite", favoriteCard, Vector2.zero, new Vector2(270, 48), Peach,
                () => ShowToy(profile.Favorite), out _);
            profileLinks = Rect("Your profile links", profileRoot);
            CreateButton("Settings", profileLinks, Vector2.zero, new Vector2(290, 48), Quiet, ShowSettings, out _);
            CreateButton("Badge cabinet", profileLinks, Vector2.zero, new Vector2(290, 48), Quiet,
                () => ShowDialog("Your future badge cabinet", "Earned badges will live here once their rules and rewards are ready. No badges have been awarded in this beta."), out _);
            CreateButton("Cloud save", profileLinks, Vector2.zero, new Vector2(290, 48), Quiet,
                () => ShowDialog("Your shelf stays here for now", "Accounts and cloud save are coming later. Your toys and profile are saved on this device. Clearing app data or uninstalling can erase them."), out _);
            RefreshProfile();
        }
        private void CommitProfile()
        {
            if (profileName == null || profileName.text == profile.Name) return;
            profile.SetName(profileName.text); profileName.SetTextWithoutNotify(profile.Name);
            if (page == AppPage.Home) heading.text = "Hey, " + profile.Name;
        }
        private void RefreshProfile()
        {
            if (profilePortrait == null) return;
            profilePortrait.texture = previews[(int)profile.Avatar];
            for (int i = 0; i < 4; i++)
            {
                avatarColours[i].color = (int)profile.Avatar == i ? Peach : Quiet;
                // The rendered portrait is opaque, so outline the selected avatar with inset margins.
                var picture = (RectTransform)avatarButtons[i].transform.GetChild(1);
                picture.offsetMin = new Vector2(6, 6); picture.offsetMax = new Vector2(-6, -6);
                favoriteColours[i].color = (int)profile.Favorite == i ? Peach : Quiet;
                favoriteButtons[i].interactable = session == null || session.Progress.Save.Counts[i] > 0;
            }
            playFavorite.interactable = session == null || session.Progress.Save.Counts[(int)profile.Favorite] > 0;
            if (session != null)
            {
                int distinct = 0, total = 0;
                foreach (int count in session.Progress.Save.Counts) { if (count > 0) distinct++; total += count; }
                profileSummary.text = distinct + " collected finishes · " + total + " toys";
            }
        }
        private float LayoutProfile(float width)
        {
            float w = Mathf.Min(width, 480);
            TopCentered(profileRoot, Vector2.zero, new Vector2(width, 868));
            TopCentered(identityCard, Vector2.zero, new Vector2(w, 296));
            TopCentered(profileName.GetComponent<RectTransform>(), new Vector2(0, -148), new Vector2(Mathf.Min(270, w - 32), 48));
            TopCentered(avatarRow, new Vector2(0, -312), new Vector2(w, 110));
            float avatarWidth = (w - 24) / 4;
            for (int i = 0; i < 4; i++)
                LeftLabel(avatarButtons[i].GetComponent<RectTransform>(), new Vector2(i * (avatarWidth + 8), -30), new Vector2(avatarWidth, 78));
            TopCentered(favoriteCard, new Vector2(0, -438), new Vector2(w, 218));
            float buttonWidth = (w - 42) / 2;
            for (int i = 0; i < 4; i++)
                LeftLabel(favoriteButtons[i].GetComponent<RectTransform>(), new Vector2(16 + i % 2 * (buttonWidth + 10), -44 - i / 2 * 56), new Vector2(buttonWidth, 48));
            TopCentered(playFavorite.GetComponent<RectTransform>(), new Vector2(0, -160), new Vector2(w - 32, 48));
            TopCentered(profileLinks, new Vector2(0, -674), new Vector2(w, 168));
            for (int i = 0; i < profileLinks.childCount; i++)
                LeftLabel((RectTransform)profileLinks.GetChild(i), new Vector2(0, -i * 58), new Vector2(w, 48));
            return 852;
        }
    }
}
