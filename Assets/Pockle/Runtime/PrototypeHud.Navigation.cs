using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        // The safe-area inset is added below this content, never taken from its tap targets.
        private const float NavigationContentHeight = 72f;
        private RectTransform navigationItems;
        private NavigationItem[] navigationItemsByPage;

        private sealed class NavigationItem
        {
            internal AppPage Page;
            internal RectTransform Rect;
            internal Image Selection;
            internal RawImage Glyph;
            internal Text Label;
            internal Texture2D Regular, Filled;
        }

        private void BuildNavigation()
        {
            // A canvas sibling spans the screen, including the bottom/side unsafe areas.
            // Its buttons are independently constrained to the safe horizontal bounds.
            navigation = Rect("Navigation", canvas.transform);
            var background = navigation.gameObject.AddComponent<Image>();
            background.color = Primary;
            navigationItems = Rect("Navigation safe targets", navigation);
            navigationItemsByPage = new[]
            {
                BuildNavigationItem("Home", "house", AppPage.Home),
                BuildNavigationItem("Shelf", "squares-four", AppPage.Shelf),
                BuildNavigationItem("Walk", "sneaker-move", AppPage.Rewards),
                BuildNavigationItem("Community", "users-three", AppPage.Social),
                BuildNavigationItem("Shop", "storefront", AppPage.Boxes)
            };
        }

        private NavigationItem BuildNavigationItem(string title, string resource, AppPage destination)
        {
            var button = CreateButton(title, navigationItems, Vector2.zero, new Vector2(78, 56),
                Color.clear, () => SelectTab(destination), out var background, 11);
            // The whole equal-sized target remains tappable; only the icon gets a selected capsule.
            background.sprite = null;
            background.type = Image.Type.Simple;
            var selectionRect = Rect("Selected icon", button.transform);
            TopCentered(selectionRect, new Vector2(0, -1), new Vector2(48, 32));
            var selection = Surface(selectionRect, Color.clear, 14);
            var iconRect = Rect("Navigation icon", selectionRect);
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(.5f, .5f);
            iconRect.sizeDelta = new Vector2(27, 27);
            var glyph = iconRect.gameObject.AddComponent<RawImage>();
            glyph.raycastTarget = false;
            var label = button.GetComponentInChildren<Text>();
            var labelRect = label.rectTransform;
            labelRect.anchorMin = new Vector2(0, 0); labelRect.anchorMax = new Vector2(1, 0);
            labelRect.pivot = new Vector2(.5f, 0);
            labelRect.offsetMin = new Vector2(1, 2); labelRect.offsetMax = new Vector2(-1, 19);
            label.resizeTextMinSize = 10; label.resizeTextMaxSize = 11;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            return new NavigationItem
            {
                Page = destination, Rect = button.GetComponent<RectTransform>(), Selection = selection,
                Glyph = glyph, Label = label,
                Regular = Resources.Load<Texture2D>("UI/Navigation/" + resource),
                Filled = Resources.Load<Texture2D>("UI/Navigation/" + resource + "-fill")
            };
        }

        private void LayoutNavigation()
        {
            if (navigation == null || navigationItemsByPage == null) return;
            var canvasRect = canvas.transform as RectTransform;
            float canvasHeight = canvasRect.rect.height;
            float bottomInset = Mathf.Max(0, safeRoot.anchorMin.y * canvasHeight + safeRoot.offsetMin.y);
            float leftInset = safeRoot.anchorMin.x * canvasRect.rect.width + safeRoot.offsetMin.x;
            navigation.anchorMin = new Vector2(0, 0); navigation.anchorMax = new Vector2(1, 0);
            navigation.pivot = new Vector2(.5f, 0);
            navigation.anchoredPosition = Vector2.zero;
            navigation.sizeDelta = new Vector2(0, NavigationContentHeight + bottomInset);
            LeftLabel(navigationItems, new Vector2(leftInset, 0), new Vector2(safeRoot.rect.width, NavigationContentHeight));
            float width = safeRoot.rect.width / navigationItemsByPage.Length;
            for (int i = 0; i < navigationItemsByPage.Length; i++)
                LeftLabel(navigationItemsByPage[i].Rect, new Vector2(i * width, -8), new Vector2(width, 56));
        }

        private void RefreshNavigation()
        {
            if (navigationItemsByPage == null) return;
            AppPage? current = NavigationPageFor(page);
            foreach (var item in navigationItemsByPage)
            {
                bool selected = current == item.Page;
                item.Selection.color = selected ? PockleTheme.Jelly : Color.clear;
                item.Glyph.color = selected ? Ink : PockleTheme.OnPlumMuted;
                item.Glyph.texture = selected ? item.Filled : item.Regular;
                item.Label.color = selected ? PockleTheme.Jelly : PockleTheme.OnPlumMuted;
            }
        }

        private static AppPage? NavigationPageFor(AppPage value)
        {
            if (MenuNavigation.IsMainTab(value)) return value;
            switch (value)
            {
                case AppPage.Collections: case AppPage.Collection: case AppPage.ShelfPick:
                case AppPage.Personality: case AppPage.Fusion: case AppPage.FusionReveal:
                    return AppPage.Shelf;
                default: return null; // Profile/settings and focused play do not select an unrelated tab.
            }
        }
    }
}
