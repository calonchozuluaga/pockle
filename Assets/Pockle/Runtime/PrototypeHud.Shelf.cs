using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private void BuildShelf()
        {
            previews[0] = ToyPortrait.Render(PipVariant.PeachJelly);
            previews[1] = ToyPortrait.Render(PipVariant.MoonJelly);
            previews[2] = ToyPortrait.Render(PipVariant.GoldGlitter);
            previews[3] = ToyPortrait.Render(PipVariant.MintSoft);
            for (int i = 0; i < 4; i++)
            {
                PipVariant choice = (PipVariant)i;
                toyTiles[i] = Rect(PipVariants.Label(choice) + " · shelf toy", shelfRoot);
                var face = Card(toyTiles[i], PockleTheme.ShelfFace); face.raycastTarget = true;
                toyButtons[i] = toyTiles[i].gameObject.AddComponent<Button>(); toyButtons[i].targetGraphic = face;
                toyButtons[i].onClick.AddListener(() => ShowToy(choice));
                toyTiles[i].gameObject.AddComponent<SquishFeedback>();
                toyPlanks[i] = Rect("Ceramic shelf", toyTiles[i]); Surface(toyPlanks[i], PockleTheme.ShelfPlank);
                toyPictures[i] = Rect("Pip portrait", toyTiles[i]);
                var image = toyPictures[i].gameObject.AddComponent<RawImage>(); image.texture = previews[i]; image.raycastTarget = false;
                toyNames[i] = Label(PipVariants.Label(choice), toyTiles[i], 13, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(150, 22)).rectTransform;
                toyCounts[i] = Label("", toyTiles[i], PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(150, 20));
            }
        }

        private float LayoutShelf(float width)
        {
            int columns = width >= 650 ? 4 : width >= 270 ? 2 : 1;
            const float gap = 16;
            float tileWidth = (width - (columns - 1) * gap) / columns;
            float portrait = Mathf.Min(200, tileWidth - 12);
            float tileHeight = portrait + 64;
            int rows = (4 + columns - 1) / columns;
            float height = rows * (tileHeight + gap);
            TopCentered(shelfRoot, Vector2.zero, new Vector2(width, height));
            for (int i = 0; i < 4; i++)
            {
                LeftLabel(toyTiles[i], new Vector2((i % columns) * (tileWidth + gap), -(i / columns) * (tileHeight + gap)), new Vector2(tileWidth, tileHeight));
                TopCentered(toyPictures[i], new Vector2(0, -8), new Vector2(portrait, portrait));
                TopCentered(toyPlanks[i], new Vector2(0, -8 - portrait * .965f), new Vector2(tileWidth - 12, 12));
                toyPictures[i].SetAsLastSibling(); toyPlanks[i].SetAsLastSibling();
                toyNames[i].SetAsLastSibling(); toyCounts[i].transform.SetAsLastSibling();
                TopCentered(toyNames[i], new Vector2(0, -portrait - 14), new Vector2(tileWidth - 12, 22));
                TopCentered(toyCounts[i].rectTransform, new Vector2(0, -portrait - 38), new Vector2(tileWidth - 12, 20));
            }
            return height;
        }

        private void RefreshCollection()
        {
            if (session == null) return;
            var saved = session.Progress.Save;
            for (int i = 0; i < 4; i++)
            {
                toyButtons[i].interactable = saved.Counts[i] > 0;
                toyCounts[i].text = saved.Counts[i] > 1 ? saved.Counts[i] + " in your collection" : saved.Counts[i] == 1 ? "Yours to play with" : "Yet to discover";
            }
            RefreshHome(); RefreshProfile(); RefreshSettings();
            bool currentDay = saved.Day == CollectionSession.Today;
            walkProgress.text = saved.Steps.ToString("N0") + " / 1,000 steps";
            walkFill.fillAmount = saved.Steps / 1000f;
            walkStatus.text = !currentDay ? "Check your device date to continue walking." : saved.Claimed
                ? "Today's box is yours. A new box arrives tomorrow (UTC)." : session.WalkingStatus;
            openDaily.interactable = currentDay && session.Progress.CanClaim;
            openDaily.GetComponentInChildren<Text>().text = saved.Claimed ? "Opened today" : session.Progress.CanClaim ? "Open your box" : "Walk to unlock";
        }
    }
}
