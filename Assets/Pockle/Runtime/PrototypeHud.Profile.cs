using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>The approved private profile: account name, twelve original faces and a device-local photo.</summary>
    public sealed partial class PrototypeHud
    {
        private Text profileName;
        private RectTransform profileIdentity, profileAvatarCircle, profileAvatarHeading, profileAvatarGrid, profileActions;
        private RawImage profilePortrait;
        private Text profileScope, profileAvatarStatus;
        private Button profileRename, profilePhoto;
        private Button[] profileAvatarChoices;

        private void BuildProfile()
        {
            profileIdentity = Rect("Your identity", profileRoot);
            profileAvatarCircle = Rect("Your avatar", profileIdentity);
            Surface(profileAvatarCircle, PockleTheme.FieldPeach, 56);
            profileAvatarCircle.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            profilePortrait = AvatarImage(profileAvatarCircle, ProfileAvatarTexture);
            profileRename = CreateButton("", profileIdentity, Vector2.zero, new Vector2(280, 44), Color.clear,
                () => { if (account != null && account.IsSignedIn) ShowRename(); }, out _, 24);
            profileRename.gameObject.name = "Change collector name";
            // Identity is a label, not a local text field. The button opens the verified username claim.
            profileName = profileRename.GetComponentInChildren<Text>(); profileName.resizeTextForBestFit = false;
            profileScope = Label("Saved on this device", profileIdentity, 14, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            profileAvatarHeading = Heading("Choose your little face", profileRoot);
            profileAvatarGrid = Rect("Twelve Pockle avatars", profileRoot);
            profileAvatarChoices = BuildAvatarChoices(profileAvatarGrid);
            profilePhoto = CreateButton("Choose a photo", profileRoot, Vector2.zero, new Vector2(350, 52), Quiet, BeginAvatarPhoto, out _, 17);
            profilePhoto.gameObject.name = "Choose a profile photo";
            profileAvatarStatus = Label("Your avatar is private and saved on this device.", profileRoot, 14, MutedInk,
                FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            profileActions = Rect("Profile actions", profileRoot);
            RowButton("Settings", "Account, sound, walking and notifications", "Settings", profileActions,
                () => Navigate(AppPage.Settings), out _, true);
            RowButton("Privacy & sharing", "Your photo stays on this device", "Heart", profileActions,
                () => ShowDialog("Your private little face", "Your avatar is saved separately for your signed-in account on this device. Photos are not uploaded or shared publicly. You can change it any time; signing out hides it."), out _, true);
            RowButton("Badges", "0 of " + BadgeBook.All.Count + " earned", "Heart", profileActions, OpenBadges, out badgesRowDetail, true);
            RefreshProfile();
        }

        // Display-name edits use ClaimUsername through ShowRename/AcceptName. Never persist text on focus loss.
        private void CommitProfile() { }

        private void RefreshProfile()
        {
            if (profilePortrait == null) return;
            profilePortrait.texture = ProfileAvatarTexture;
            string name = account != null && account.IsSignedIn && !string.IsNullOrEmpty(account.Username) ? account.Username : profile.Name;
            profileName.text = name;
            profileRename.interactable = account != null && account.IsSignedIn;
            bool editable = avatarService != null && avatarService.CanEdit;
            profileScope.text = editable ? "Saved on this device" : "Sign in to choose your avatar";
            profilePhoto.interactable = editable && avatarService.Stage == AvatarPhotoStage.Idle;
            profileAvatarStatus.text = avatarService != null && !string.IsNullOrEmpty(avatarService.Message) ? avatarService.Message :
                "Adjust your photo before saving.\nYour avatar is private and saved on this device.";
            RefreshAvatarChoices(profileAvatarChoices);
        }

        private float LayoutProfile(float width)
        {
            float w = Mathf.Min(width, 480), left = (width - w) / 2, y = 4;
            LeftLabel(profileIdentity, new Vector2(left, -y), new Vector2(w, 202));
            LeftLabel(profileAvatarCircle, new Vector2((w - 112) / 2, 0), new Vector2(112, 112));
            LeftLabel(profileRename.GetComponent<RectTransform>(), new Vector2(0, -122), new Vector2(w, 44));
            LeftLabel(profileScope.rectTransform, new Vector2(0, -168), new Vector2(w, 24));
            y += 214;
            LeftLabel(profileAvatarHeading, new Vector2(left, -y), new Vector2(w, 32)); y += 40;
            float gridHeight = LayoutAvatarChoices(profileAvatarChoices, w);
            LeftLabel(profileAvatarGrid, new Vector2(left, -y), new Vector2(w, gridHeight)); y += gridHeight + 18;
            LeftLabel(profilePhoto.GetComponent<RectTransform>(), new Vector2(left, -y), new Vector2(w, 52)); y += 64;
            LeftLabel(profileAvatarStatus.rectTransform, new Vector2(left, -y), new Vector2(w, 58)); y += 72;
            LeftLabel(profileActions, new Vector2(left, -y), new Vector2(w, 210));
            for (int i = 0; i < profileActions.childCount; i++)
                LeftLabel((RectTransform)profileActions.GetChild(i), new Vector2(0, -i * 72), new Vector2(w, 64));
            y += 210; TopCentered(profileRoot, Vector2.zero, new Vector2(width, y)); return y;
        }
    }
}
