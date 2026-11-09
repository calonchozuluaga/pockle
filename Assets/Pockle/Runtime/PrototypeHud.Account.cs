using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// UX-051 account and notification settings (owner, October 21): the parts playpockle.com promises. "Your account"
    /// (who you're signed in as, change collector name, sign out, delete account) and "Notifications" (which kinds of
    /// notification Pockle may send). The account actions go through <see cref="IAccountService"/> (Codex's runtime);
    /// notification choices are saved here and read by the runtime through <see cref="NotificationPrefs"/>.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform accountHeading, accountCard, notifyHeading, notifyCard;
        private Text accountWho, accountWhoDetail;
        private readonly Button[] notifySwitches = new Button[NotificationPrefs.Kinds.Length];
        private readonly Image[] notifyTracks = new Image[NotificationPrefs.Kinds.Length];
        private readonly RectTransform[] notifyKnobs = new RectTransform[NotificationPrefs.Kinds.Length];
        private RectTransform confirm, confirmCard;
        private Text confirmTitle, confirmText;
        private Button confirmYes, confirmNo;
        private Action confirmAction;
        private bool obRenaming;

        // ----- Build -----

        private void BuildAccountSettings()
        {
            accountHeading = GroupLabel("Your account", settingsRoot);
            accountCard = Rect("Your account", settingsRoot); Surface(accountCard, Quiet, 24);
            var who = Rect("Signed in as", accountCard);
            accountWho = Label("", who, 17, Ink, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, Vector2.zero);
            Place(accountWho.rectTransform, 0, .5f, 1, 1, 0, 0, 0, 8);
            accountWhoDetail = Label("", who, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            Place(accountWhoDetail.rectTransform, 0, 0, 1, .5f, 0, 8, 0, 2);
            Divider(who);
            LinkRow("Change collector name", accountCard, ShowRename);
            LinkRow("Sign out", accountCard, () => ShowConfirm("Sign out?",
                "Your shelf is saved to your account. Sign in again with the same Apple or Google account to get it back.",
                "Sign out", false, SignOut));
            LinkRow("Delete account", accountCard, () => ShowConfirm("Delete your account?",
                "This permanently deletes your toys, spares, fusions, badges and friends within 30 days. It can't be undone, and purchases aren't refunded.",
                "Delete my account", true, DeleteAccount));
            var delete = accountCard.Find("Delete account");
            if (delete != null) delete.GetComponentInChildren<Text>().color = PockleTheme.Heart;
        }

        private void BuildNotificationSettings()
        {
            notifyHeading = GroupLabel("Notifications", settingsRoot);
            notifyCard = Rect("Notification choices", settingsRoot); Surface(notifyCard, Quiet, 24);
            for (int i = 0; i < NotificationPrefs.Kinds.Length; i++)
            {
                int kind = i;
                SwitchRow(NotificationPrefs.Titles[i], notifyCard, () => { NotificationPrefs.Set(NotificationPrefs.Kinds[kind], !NotificationPrefs.IsOn(NotificationPrefs.Kinds[kind])); RefreshAccountSettings(); },
                    out notifySwitches[i], out notifyTracks[i], out notifyKnobs[i]);
            }
            var note = Label("Pockle asks your phone for permission the first time you turn on walking. You can also turn notifications off in your phone's settings.",
                notifyCard, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            note.gameObject.name = "Notification note";
        }

        // ----- Layout and refresh -----

        /// <summary>Lays out "Your account" from <paramref name="y"/> and returns the new y.</summary>
        private float LayoutAccountSettings(float left, float w, float inner, float y)
        {
            LeftLabel(accountHeading, new Vector2(left + 4, -y), new Vector2(w, 22)); y += 30;
            float rowY = 0;
            for (int i = 0; i < accountCard.childCount; i++)
            {
                float h = i == 0 ? 64 : 56;
                LeftLabel((RectTransform)accountCard.GetChild(i), new Vector2(16, -rowY), new Vector2(inner, h)); rowY += h;
            }
            LeftLabel(accountCard, new Vector2(left, -y), new Vector2(w, rowY));
            return y + rowY + 22;
        }

        private float LayoutNotificationSettings(float left, float w, float inner, float y)
        {
            LeftLabel(notifyHeading, new Vector2(left + 4, -y), new Vector2(w, 22)); y += 30;
            float rowY = 0;
            for (int i = 0; i < notifyCard.childCount; i++)
            {
                var child = (RectTransform)notifyCard.GetChild(i);
                float h = child.name == "Notification note" ? 62 : 60;
                LeftLabel(child, new Vector2(16, -(rowY + (h == 62 ? 10 : 0))), new Vector2(inner, h == 62 ? 48 : h)); rowY += h;
            }
            LeftLabel(notifyCard, new Vector2(left, -y), new Vector2(w, rowY));
            return y + rowY + 22;
        }

        private void RefreshAccountSettings()
        {
            if (accountWho == null) return;
            bool signedIn = account != null && account.IsSignedIn;
            accountWho.text = "@" + (signedIn && !string.IsNullOrEmpty(account.Username) ? account.Username : profile.Name);
            accountWhoDetail.text = signedIn ? "Signed in · your shelf is saved to your account"
                : TestBuildSignIn ? "Test build · not signed in, saved on this phone only" : "Not signed in";
            for (int i = 0; i < NotificationPrefs.Kinds.Length; i++)
                StyleSwitch(notifyTracks[i], notifyKnobs[i], NotificationPrefs.IsOn(NotificationPrefs.Kinds[i]));
        }

        // ----- Actions -----

        /// <summary>Reuses the onboarding name step, then comes back to Settings instead of going on to the tutorial.</summary>
        private void ShowRename()
        {
            obRenaming = true; obTutorialOnly = false;
            obName.text = profile.Name;
            ShowOnboardingStep(OnboardingStep.Username);
        }

        private void SignOut()
        {
            if (account == null) { ShowDialog("Test build", "There's no account to sign out of in this build yet."); return; }
            HideAccountAvatar();
            account.SignOut(() =>
            {
                HideAccountAvatar();
                // Signed out: the sign-in step comes back; the age check already passed on this phone.
                obTutorialOnly = false; obRenaming = false;
                ShowOnboardingStep(OnboardingStep.SignIn);
            });
        }

        private void DeleteAccount()
        {
            if (account == null) { ShowDialog("Test build", "Account deletion needs a signed-in account, which this build doesn't have yet. You can also ask at playpockle.com/delete-account."); return; }
            SyncAvatarAccount();
            string deletingAvatarOwner = avatarService != null ? avatarService.AccountId : null;
            account.RequestDeletion(ok =>
            {
                if (!ok) { ShowDialog("Couldn't delete right now", "Check your connection and try again, or email privacy@playpockle.com."); return; }
                bool avatarRemoved = string.IsNullOrEmpty(deletingAvatarOwner) || DeleteAccountAvatar(deletingAvatarOwner);
                HideAccountAvatar();
                ShowDialog("Your account is being deleted", "It will be gone within 30 days. You've been signed out." +
                    (avatarRemoved ? "" : " Your device could not remove its private avatar file; email privacy@playpockle.com for help."));
                obTutorialOnly = false; obRenaming = false;
                ShowOnboardingStep(OnboardingStep.SignIn);
                modal.SetAsLastSibling();
            });
        }

        // ----- Confirm dialog (two choices) -----

        private void BuildConfirm()
        {
            confirm = Rect("Confirm backdrop", canvas.transform); Stretch(confirm);
            var backdrop = confirm.gameObject.AddComponent<Image>(); backdrop.color = PockleTheme.Backdrop;
            confirmCard = Rect("Confirm", confirm); confirmCard.anchorMin = confirmCard.anchorMax = confirmCard.pivot = new Vector2(.5f, .5f);
            confirmCard.sizeDelta = new Vector2(330, 300); Card(confirmCard, Paper);
            confirmTitle = Label("", confirmCard, 24, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, -24), new Vector2(290, 36));
            confirmText = Label("", confirmCard, PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, new Vector2(0, -70), new Vector2(286, 100));
            confirmYes = CreateButton("Confirm", confirmCard, Vector2.zero, new Vector2(286, 52), Primary, () => { var act = confirmAction; CloseConfirm(); act?.Invoke(); }, out _);
            var yes = confirmYes.GetComponent<RectTransform>(); yes.anchorMin = yes.anchorMax = yes.pivot = new Vector2(.5f, 0); yes.anchoredPosition = new Vector2(0, 80);
            confirmNo = CreateButton("Cancel", confirmCard, Vector2.zero, new Vector2(286, 52), Quiet, CloseConfirm, out _);
            var no = confirmNo.GetComponent<RectTransform>(); no.anchorMin = no.anchorMax = no.pivot = new Vector2(.5f, 0); no.anchoredPosition = new Vector2(0, 20);
            confirm.gameObject.SetActive(false);
        }

        private bool ConfirmVisible => confirm != null && confirm.gameObject.activeSelf;

        private void ShowConfirm(string title, string message, string action, bool destructive, Action onConfirm)
        {
            confirmTitle.text = title; confirmText.text = message; confirmAction = onConfirm;
            confirmYes.GetComponentInChildren<Text>().text = action;
            confirmYes.targetGraphic.color = destructive ? PockleTheme.Heart : Primary;
            confirmYes.GetComponentInChildren<Text>().color = PockleTheme.OnPlum;
            confirm.gameObject.SetActive(true); confirm.SetAsLastSibling();
        }

        private void CloseConfirm() { confirmAction = null; if (confirm != null) confirm.gameObject.SetActive(false); }
    }

    /// <summary>Which notifications the player allows. Codex's runtime checks these before scheduling or showing one.</summary>
    public static class NotificationPrefs
    {
        public const string BoxReady = "boxes", Trades = "trades", Friends = "friends", NewCollections = "drops";
        public static readonly string[] Kinds = { BoxReady, Trades, Friends, NewCollections };
        public static readonly string[] Titles = { "Box ready", "Trade offers", "Friend requests", "New collections" };
        private const string Prefix = "pockle.notifications.v1.";
        public static event Action<string, bool> Changed;
        public static bool IsOn(string kind) => PlayerPrefs.GetInt(Prefix + kind, 1) == 1;
        public static void Set(string kind, bool on) { PlayerPrefs.SetInt(Prefix + kind, on ? 1 : 0); PlayerPrefs.Save(); Changed?.Invoke(kind, on); }
    }
}
