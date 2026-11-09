using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Community destinations with honest availability until the online services are bound.</summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform socialTabs, socialScene, communityState, ownProfile, communityShelf;
        private readonly Image[] communityTabImages = new Image[3];
        private Text socialTitle, socialDescription, communityStatus, communityHeroTitle;
        private RawImage communityArt;
        private int communitySection;

        private void BuildSocial()
        {
            socialTabs = Rect("Community sections", socialRoot); Surface(socialTabs, PockleTheme.FillDeep, 16);
            string[] sections = { "Feed", "Friends", "Exchange" };
            for (int i = 0; i < sections.Length; i++)
            {
                int section = i;
                var button = CreateButton(sections[i], socialTabs, Vector2.zero, new Vector2(100, 44), Paper,
                    () => SelectCommunitySection(section), out communityTabImages[i], 14);
                button.name = "Community section · " + sections[i];
            }
            socialScene = Rect("Community collection scene", socialRoot); Surface(socialScene, PockleTheme.FieldMoon, 24);
            socialScene.gameObject.AddComponent<RectMask2D>();
            var picture = Rect("Community artwork", socialScene); Stretch(picture);
            communityArt = picture.gameObject.AddComponent<RawImage>(); communityArt.raycastTarget = false;
            communityArt.texture = CollectionLook.Banner("fox"); communityArt.enabled = communityArt.texture != null;
            var shade = Rect("Community scene shade", socialScene); Stretch(shade);
            Surface(shade, new Color(.18f, .08f, .17f, .45f), 0).raycastTarget = false;
            communityHeroTitle = Label("Little toys.\nGood company.", socialScene, 29, Color.white, FontStyle.Bold,
                TextAnchor.LowerLeft, Vector2.zero, Vector2.zero);
            communityState = Rect("Community availability", socialRoot); Surface(communityState, Paper, 22);
            communityStatus = Label("COMING SOON", communityState, 11, MutedInk, FontStyle.Bold,
                TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            socialTitle = Label("", communityState, 25, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            socialDescription = Label("", communityState, 15, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            ownProfile = CreateButton("Make your profile yours", socialRoot, Vector2.zero, new Vector2(330, 52), Primary,
                () => Navigate(AppPage.Profile), out _, 16).GetComponent<RectTransform>();
            communityShelf = CreateButton("Back to your shelf", socialRoot, Vector2.zero, new Vector2(330, 48), Quiet,
                () => SelectTab(AppPage.Shelf), out _, 15).GetComponent<RectTransform>();
            SelectCommunitySection(0);
        }

        private void SelectCommunitySection(int section)
        {
            communitySection = Mathf.Clamp(section, 0, 2);
            for (int i = 0; i < communityTabImages.Length; i++)
            {
                communityTabImages[i].color = i == communitySection ? Paper : Color.clear;
                communityTabImages[i].GetComponentInChildren<Text>().color = i == communitySection ? Ink : MutedInk;
            }
            switch (communitySection)
            {
                case 1:
                    socialTitle.text = "Your collecting circle";
                    socialDescription.text = "Friends and shelf visits are coming soon. You can choose your profile picture now; it stays private on this device.";
                    break;
                case 2:
                    socialTitle.text = "Give your extras a new home";
                    socialDescription.text = "Public trade offers are coming soon. You’ll be able to offer a spare and choose the finish you want in return. Trading is not available in this build.";
                    break;
                default:
                    socialTitle.text = "A place for little discoveries";
                    socialDescription.text = "Your community feed is coming soon. This is where discoveries and shared shelves will live. No public profiles are connected yet.";
                    break;
            }
            if (lastLayoutSize.x > 0 && page == AppPage.Social) AdaptLayout();
        }

        private float LayoutSocial(float width)
        {
            float w = Mathf.Min(width, 480), left = (width - w) / 2, y = 0;
            LeftLabel(socialTabs, new Vector2(left, -y), new Vector2(w, 52));
            float tabWidth = (w - 8) / 3;
            for (int i = 0; i < communityTabImages.Length; i++)
                LeftLabel(communityTabImages[i].rectTransform, new Vector2(4 + i * tabWidth, -4), new Vector2(tabWidth, 44));
            y += 68;
            float sceneHeight = Mathf.Clamp(w * .56f, 156, 232);
            LeftLabel(socialScene, new Vector2(left, -y), new Vector2(w, sceneHeight));
            communityArt.uvRect = CollectionLook.Cover(communityArt.texture, w / sceneHeight, .5f, .5f);
            LeftLabel(communityHeroTitle.rectTransform, new Vector2(20, -(sceneHeight - 90)), new Vector2(w - 40, 70));
            y += sceneHeight + 18;
            float inner = Mathf.Max(1, w - 36);
            LeftLabel(communityStatus.rectTransform, new Vector2(18, -20), new Vector2(inner, 20));
            LeftLabel(socialTitle.rectTransform, new Vector2(18, -50), new Vector2(inner, 68));
            LeftLabel(socialDescription.rectTransform, new Vector2(18, -126), new Vector2(inner, 136));
            float descriptionHeight = Mathf.Max(86, socialDescription.preferredHeight);
            socialDescription.rectTransform.sizeDelta = new Vector2(inner, descriptionHeight);
            float cardHeight = 126 + descriptionHeight + 20;
            LeftLabel(communityState, new Vector2(left, -y), new Vector2(w, cardHeight));
            y += cardHeight + 18;
            LeftLabel(ownProfile, new Vector2(left, -y), new Vector2(w, 52)); y += 62;
            LeftLabel(communityShelf, new Vector2(left, -y), new Vector2(w, 48)); y += 48;
            TopCentered(socialRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }
    }
}
