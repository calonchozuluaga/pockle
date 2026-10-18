using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Full-bleed pages (a collection, a toy's details) start with a big picture and its title. Once that scrolls away,
    /// the title used to slide under the back button (owner, October 15). A small bar in the page's colour now fades in
    /// behind the back button with the name centred, so nothing ever sits under it.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform compactBar;
        private Image compactFace;
        private Text compactTitle;
        private CanvasGroup compactGroup;
        private const float CompactBarHeight = 76;

        private void BuildCompactHeader(ScrollRect scroll)
        {
            compactBar = Rect("Compact header", safeRoot);
            compactFace = Surface(compactBar, PockleTheme.Milk, 0);
            compactGroup = compactBar.gameObject.AddComponent<CanvasGroup>(); compactGroup.alpha = 0; compactGroup.blocksRaycasts = false;
            compactTitle = Label("", compactBar, PockleTheme.BodySize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            compactBar.gameObject.SetActive(false);
            scroll.onValueChanged.AddListener(_ => RefreshCompactHeader());
        }

        /// <summary>How far the page scrolls before its own title would reach the back button.</summary>
        private float CompactHeaderAfter => page == AppPage.Collection ? 330 - 120 : page == AppPage.Personality ? PersonalitySheetTop - 40 : float.MaxValue;

        private void LayoutCompactHeader(float left)
        {
            if (compactBar == null) return;
            // Runs up past the safe area so the status bar area takes the same colour.
            compactBar.anchorMin = new Vector2(0, 1); compactBar.anchorMax = new Vector2(1, 1); compactBar.pivot = new Vector2(.5f, 1);
            compactBar.offsetMin = new Vector2(-60, -CompactBarHeight); compactBar.offsetMax = new Vector2(60, 80);
            var title = compactTitle.rectTransform;
            title.anchorMin = new Vector2(0, 0); title.anchorMax = new Vector2(1, 0); title.pivot = new Vector2(.5f, 0);
            title.offsetMin = new Vector2(60 + left + 56, 16); title.offsetMax = new Vector2(-(60 + left + 56), 60);
            // Behind the back button and the favourite, above the page.
            compactBar.SetSiblingIndex(Mathf.Max(0, backButton.transform.GetSiblingIndex()));
            RefreshCompactHeader();
        }

        private void RefreshCompactHeader()
        {
            if (compactBar == null) return;
            bool page1 = page == AppPage.Collection || page == AppPage.Personality;
            float y = content != null ? content.anchoredPosition.y : 0;
            float after = CompactHeaderAfter;
            float alpha = page1 ? Mathf.Clamp01((y - after) / 40f) : 0;
            compactGroup.alpha = alpha;
            compactBar.gameObject.SetActive(alpha > .01f);
            if (alpha <= .01f) return;
            if (page == AppPage.Collection)
            {
                var selected = SelectedCollection;
                compactTitle.text = selected != null ? selected.DisplayName : "";
                compactFace.color = selected != null ? CatalogUnityColor(selected.FieldColor) : PockleTheme.Milk;
            }
            else
            {
                compactTitle.text = PersonalityBook.ShortName(personalityId);
                compactFace.color = PockleTheme.FieldFor(personalityId);
            }
        }
    }
}
