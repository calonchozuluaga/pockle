using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// v2 Friends (UX-032): a Discover / Your friends switch, a shelf illustration with your own toys, an honest
    /// empty state, and a way to set up your profile. No online services are connected.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform socialTabs, socialScene, ownProfile;
        private Image discoverTab, friendsTab;
        private Text socialTitle, socialDescription;
        private readonly RawImage[] socialToys = new RawImage[3];

        private void BuildSocial()
        {
            socialTabs = Rect("Discover and friends", socialRoot); Surface(socialTabs, Quiet, 24);
            CreateButton("Discover", socialTabs, Vector2.zero, new Vector2(145, 44), Paper, () => SelectSocial(false), out discoverTab, PockleTheme.BodySize);
            CreateButton("Your friends", socialTabs, Vector2.zero, new Vector2(145, 44), Paper, () => SelectSocial(true), out friendsTab, PockleTheme.BodySize);

            // A little shelf scene: three of your toys on a lavender field.
            socialScene = Rect("Shelf scene", socialRoot); Surface(socialScene, PockleTheme.FieldMoon, 32);
            socialScene.gameObject.AddComponent<RectMask2D>();
            var plank = Rect("Shelf", socialScene); Surface(plank, new Color(.725f, .753f, .949f), 7);
            plank.anchorMin = new Vector2(0, 0); plank.anchorMax = new Vector2(1, 0); plank.pivot = new Vector2(.5f, 0);
            plank.offsetMin = new Vector2(24, 36); plank.offsetMax = new Vector2(-24, 50);
            int[] toys = { 0, 1, 3 };
            for (int i = 0; i < socialToys.Length; i++)
            {
                var frame = Rect("Toy", socialScene); Surface(frame, PockleTheme.FieldFor(PipVariants.CollectibleId((PipVariant)toys[i])), PockleTheme.TileRadius);
                frame.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                socialToys[i] = Portrait(frame, toys[i]); Stretch(socialToys[i].rectTransform);
            }

            socialTitle = Label("", socialRoot, 24, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, new Vector2(330, 60));
            socialDescription = Label("", socialRoot, PockleTheme.BodySize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(330, 96));
            ownProfile = CreateButton("Set up your profile", socialRoot, Vector2.zero, new Vector2(330, 56), PockleTheme.Plum,
                () => SelectTab(AppPage.Profile), out _, 18).GetComponent<RectTransform>();
            SelectSocial(false);
        }

        private void SelectSocial(bool friends)
        {
            discoverTab.color = friends ? new Color(1, 1, 1, 0) : Paper; friendsTab.color = friends ? Paper : new Color(1, 1, 1, 0);
            discoverTab.GetComponentInChildren<Text>().color = friends ? MutedInk : Ink;
            friendsTab.GetComponentInChildren<Text>().color = friends ? Ink : MutedInk;
            socialTitle.text = friends ? "Your friends will live here" : "A world of little shelves";
            socialDescription.text = friends
                ? "Friend requests and shelf visits are coming later, when accounts are ready. For now, make your own shelf yours."
                : "Public shelves are coming later. You'll be able to browse them without becoming friends first. No public profiles are connected yet.";
        }

        private float LayoutSocial(float width)
        {
            float w = Mathf.Min(width, 480);
            float left = (width - w) / 2, y = 0;
            LeftLabel(socialTabs, new Vector2(left, -y), new Vector2(w, 52));
            float half = (w - 12) / 2;
            LeftLabel(discoverTab.rectTransform, new Vector2(4, -4), new Vector2(half, 44));
            LeftLabel(friendsTab.rectTransform, new Vector2(8 + half, -4), new Vector2(half, 44));
            y += 52 + 20;
            float scene = Mathf.Clamp(w * .64f, 180, 260);
            LeftLabel(socialScene, new Vector2(left, -y), new Vector2(w, scene));
            float toy = Mathf.Min(96, (w - 80) / 3);
            // Only toys you've found appear, so the scene never spoils an undiscovered finish.
            int[] toys = { 0, 1, 3 };
            int shown = 0;
            for (int i = 0; i < socialToys.Length; i++) shown += Owned((PipVariant)toys[i]) > 0 || session == null ? 1 : 0;
            int slot = 0;
            for (int i = 0; i < socialToys.Length; i++)
            {
                var frame = (RectTransform)socialToys[i].transform.parent;
                bool owned = session == null || Owned((PipVariant)toys[i]) > 0;
                frame.gameObject.SetActive(owned);
                if (!owned) continue;
                frame.anchorMin = frame.anchorMax = frame.pivot = new Vector2(.5f, 0);
                frame.anchoredPosition = new Vector2((slot++ - (shown - 1) / 2f) * (toy + 16), 46); frame.sizeDelta = new Vector2(toy, toy * 1.1f);
            }
            y += scene + 22;
            LeftLabel(socialTitle.rectTransform, new Vector2(left + 4, -y), new Vector2(w - 8, 32)); y += 40;
            LeftLabel(socialDescription.rectTransform, new Vector2(left + 4, -y), new Vector2(w - 8, 96)); y += 96 + 16;
            LeftLabel(ownProfile, new Vector2(left, -y), new Vector2(w, 56)); y += 56;
            TopCentered(socialRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }
    }
}
