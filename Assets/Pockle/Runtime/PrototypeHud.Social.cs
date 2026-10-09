using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private RectTransform socialTabs, socialCard, ownProfile;
        private Image discoverTab, friendsTab;
        private Text socialTitle, socialDescription;
        private void BuildSocial()
        {
            socialTabs = Rect("Discover and friends", socialRoot);
            CreateButton("Discover", socialTabs, Vector2.zero, new Vector2(145, 48), Peach, () => SelectSocial(false), out discoverTab);
            CreateButton("Friends", socialTabs, Vector2.zero, new Vector2(145, 48), Quiet, () => SelectSocial(true), out friendsTab);
            socialCard = Rect("Social empty state", socialRoot); Card(socialCard, Paper);
            socialTitle = Label("", socialCard, 21, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -24), new Vector2(300, 62));
            socialDescription = Label("", socialCard, 14, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter,
                new Vector2(0, -100), new Vector2(290, 142));
            ownProfile = CreateButton("Your profile", socialRoot, Vector2.zero, new Vector2(290, 52), Peach,
                () => Navigate(AppPage.Profile), out _).GetComponent<RectTransform>();
            SelectSocial(false);
        }
        private void SelectSocial(bool friends)
        {
            discoverTab.color = friends ? Quiet : Peach; friendsTab.color = friends ? Peach : Quiet;
            socialTitle.text = friends ? "Your friends will live here" : "A world of little collections";
            socialDescription.text = friends
                ? "Friend requests and visits are coming later, when accounts are ready. For now, enjoy your own shelf and personalize your local profile."
                : "Public profiles and shelf discovery are coming later. You'll be able to browse published shelves without becoming friends first. No public profiles are connected yet.";
        }
        private float LayoutSocial(float width)
        {
            float w = Mathf.Min(width, 480);
            TopCentered(socialRoot, Vector2.zero, new Vector2(width, 428));
            TopCentered(socialTabs, Vector2.zero, new Vector2(w, 48));
            LeftLabel(discoverTab.rectTransform, Vector2.zero, new Vector2((w - 10) / 2, 48));
            LeftLabel(friendsTab.rectTransform, new Vector2((w + 10) / 2, 0), new Vector2((w - 10) / 2, 48));
            TopCentered(socialCard, new Vector2(0, -66), new Vector2(w, 278));
            socialTitle.rectTransform.sizeDelta = new Vector2(w - 32, 62);
            socialDescription.rectTransform.sizeDelta = new Vector2(w - 40, 142);
            TopCentered(ownProfile, new Vector2(0, -364), new Vector2(w, 52));
            return 428;
        }
    }
}
