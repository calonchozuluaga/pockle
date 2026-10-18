using System;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// The play screen's personality (owner-approved canvas boards "Play: speech bubble + personality", October 16):
    /// under the name, a temperament badge and two traits plus a quiet "handle with care" chip (tap for the next line);
    /// above the toy, a speech bubble with a sound or a sticker, never a sentence. Words come from
    /// <see cref="ToyExpressions"/>; nothing here touches inventory or saves.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform talkTraits, talkWarning, talkBubble, talkBubbleTail;
        private Image talkTemperamentFace, talkWarningFace;
        private Text talkTemperamentText, talkWarningText, talkBubbleText;
        private readonly Image[] talkTraitFaces = new Image[2];
        private readonly Text[] talkTraitTexts = new Text[2];
        private RawImage talkWarningIcon, talkBubbleSticker;
        private BubblePop talkPop;
        private readonly ExpressionPicker expressionPicker = new ExpressionPicker();
        private string talkToyId;
        private int talkWarningIndex;
        private bool talkHelloPending;
        private const float TalkChipHeight = 28, TalkWarningHeight = 30, TalkGap = 8;

        /// <summary>The bubble's current sound ("" when quiet), for tests and the review capture.</summary>
        public string ToyBubbleText => talkBubble != null && talkBubble.gameObject.activeSelf ? talkBubbleText.text : "";

        private void BuildStageTalk()
        {
            talkTraits = Rect("Personality traits", safeRoot);
            var temperament = Rect("Temperament", talkTraits);
            talkTemperamentFace = Surface(temperament, TemperamentColours[0], TalkChipHeight / 2);
            var dot = Rect("Temperament dot", temperament); Surface(dot, Color.white, 9);
            LeftLabel(dot, new Vector2(5, -5), new Vector2(18, 18));
            var star = Rect("Temperament sparkle", dot); Stretch(star); star.offsetMin = new Vector2(4, 4); star.offsetMax = new Vector2(-4, -4);
            var starImage = star.gameObject.AddComponent<RawImage>(); starImage.texture = UiShapes.Sparkle; starImage.color = Ink; starImage.raycastTarget = false;
            talkTemperamentText = Label("", temperament, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            talkTemperamentText.horizontalOverflow = HorizontalWrapMode.Overflow; talkTemperamentText.resizeTextForBestFit = false;
            for (int i = 0; i < talkTraitFaces.Length; i++)
            {
                var chip = Rect("Trait", talkTraits);
                talkTraitFaces[i] = Surface(chip, new Color(1, 1, 1, .72f), TalkChipHeight / 2);
                talkTraitTexts[i] = Label("", chip, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                talkTraitTexts[i].horizontalOverflow = HorizontalWrapMode.Overflow; talkTraitTexts[i].resizeTextForBestFit = false;
                Stretch(talkTraitTexts[i].rectTransform);
            }

            // The warning: a small see-through chip (owner: "a little more subtle"). Tapping shows the next line.
            var warning = CreateButton("Handle with care", safeRoot, Vector2.zero, new Vector2(260, TalkWarningHeight), new Color(.16f, .08f, .19f, .34f),
                () => { talkWarningIndex++; RefreshTalkWarning(); AdaptLayout(); }, out talkWarningFace, PockleTheme.SmallSize, false);
            SetRadius(talkWarningFace, 13);
            talkWarning = warning.GetComponent<RectTransform>();
            UnityEngine.Object.Destroy(warning.GetComponentInChildren<Text>().gameObject); // replaced by the icon + line below
            var icon = Rect("Warning icon", talkWarning); LeftLabel(icon, new Vector2(9, -7), new Vector2(16, 16));
            talkWarningIcon = icon.gameObject.AddComponent<RawImage>(); talkWarningIcon.texture = PockleTheme.Icon("Warning");
            talkWarningIcon.color = new Color32(0xFF, 0xD3, 0x6B, 0xFF); talkWarningIcon.raycastTarget = false;
            talkWarningText = Label("", talkWarning, PockleTheme.SmallSize, PockleTheme.OnPlum, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            talkWarningText.horizontalOverflow = HorizontalWrapMode.Overflow; talkWarningText.resizeTextForBestFit = false;

            // The bubble: white, rounded, with a little tail pointing at the toy.
            talkBubble = Rect("Toy bubble", safeRoot);
            talkBubble.anchorMin = talkBubble.anchorMax = new Vector2(.5f, .5f); talkBubble.pivot = new Vector2(.5f, 0);
            talkBubbleTail = Rect("Tail", talkBubble); Surface(talkBubbleTail, Color.white, 3);
            talkBubbleTail.anchorMin = talkBubbleTail.anchorMax = new Vector2(.5f, 0); talkBubbleTail.pivot = new Vector2(.5f, .5f);
            talkBubbleTail.sizeDelta = new Vector2(16, 16); talkBubbleTail.anchoredPosition = new Vector2(0, 2); talkBubbleTail.localRotation = Quaternion.Euler(0, 0, 45);
            var face = Rect("Bubble face", talkBubble); Stretch(face); Surface(face, Color.white, 20);
            var shadow = face.gameObject.AddComponent<Shadow>(); shadow.effectColor = new Color(.16f, .08f, .19f, .16f); shadow.effectDistance = new Vector2(0, -3);
            talkBubbleText = Label("", face, 20, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            talkBubbleText.horizontalOverflow = HorizontalWrapMode.Overflow; talkBubbleText.resizeTextForBestFit = false;
            var sticker = Rect("Bubble sticker", face);
            talkBubbleSticker = sticker.gameObject.AddComponent<RawImage>(); talkBubbleSticker.raycastTarget = false;
            talkBubble.gameObject.AddComponent<CanvasGroup>();
            talkPop = talkBubble.gameObject.AddComponent<BubblePop>();
            talkBubble.gameObject.SetActive(false);
        }

        /// <summary>Fills the badge row and warning for the toy on stage; says hello when a new toy arrives.</summary>
        private void RefreshStageTalk(bool show, string toyId)
        {
            if (talkTraits == null) return;
            talkTraits.gameObject.SetActive(show);
            talkWarning.gameObject.SetActive(show);
            if (!show) { if (talkPop != null && talkPop.Showing) talkPop.Hide(); talkToyId = null; talkHelloPending = false; return; }
            if (!PersonalityBook.TryGet(toyId, out var personality)) { talkTraits.gameObject.SetActive(false); talkWarning.gameObject.SetActive(false); return; }

            var temperament = PersonalityBook.TemperamentOf(toyId);
            talkTemperamentText.text = PersonalityBook.TemperamentName(temperament);
            talkTemperamentFace.color = TemperamentColours[(int)temperament];
            talkTemperamentFace.gameObject.name = "Temperament · " + talkTemperamentText.text;
            // The finish's own trait first (it's what makes this toy different from its siblings), then the character's.
            string first = personality.TouchTrait ?? "", second = personality.Traits.Count > 0 ? personality.Traits[0] : "";
            if (string.Equals(first, second, StringComparison.OrdinalIgnoreCase) || string.Equals(first, talkTemperamentText.text, StringComparison.OrdinalIgnoreCase)) first = "";
            if (string.Equals(second, talkTemperamentText.text, StringComparison.OrdinalIgnoreCase)) second = personality.Traits.Count > 1 ? personality.Traits[1] : "";
            string[] traits = first.Length > 0 ? new[] { first, second } : new[] { second, personality.Traits.Count > 1 ? personality.Traits[1] : "" };
            for (int i = 0; i < talkTraitTexts.Length; i++)
            {
                talkTraitTexts[i].text = i < traits.Length ? traits[i] : "";
                talkTraitFaces[i].gameObject.SetActive(talkTraitTexts[i].text.Length > 0);
            }

            if (toyId != talkToyId)
            {
                talkToyId = toyId; talkWarningIndex = 0;
                expressionPicker.Reset(toyId);
                talkHelloPending = true; // a little hello when the toy arrives, once its area is laid out
            }
            RefreshTalkWarning();
        }

        private void RefreshTalkWarning()
        {
            if (talkToyId == null || talkWarningText == null) return;
            var warnings = ToyExpressions.Warnings(talkToyId);
            talkWarningText.text = warnings.Count > 0 ? warnings[talkWarningIndex % warnings.Count] : "";
            talkWarning.gameObject.name = "Handle with care · " + (talkWarningIndex % Mathf.Max(1, warnings.Count));
        }

        private void SayPendingHello()
        {
            if (!talkHelloPending || StageViewport.width < 2) return;
            talkHelloPending = false;
            ShowToyExpression(ReactionCue.Hello, null, .5f);
        }

        /// <summary>Over a painted or dark world the warning chip stays glassy; over the light studio colour it gets darker.</summary>
        private void StyleStageTalk(bool painted)
        {
            if (talkWarningFace == null) return;
            talkWarningFace.color = new Color(.16f, .08f, .19f, painted ? .34f : .62f);
        }

        /// <summary>Places the badge row and warning under the title; returns the new bottom of the title block.</summary>
        private float LayoutStageTalk(float left, float width, float titleBottom)
        {
            if (talkTraits == null || !talkTraits.gameObject.activeSelf) return titleBottom;
            float y = titleBottom - 4;
            // Chips flow left to right and wrap onto a second line instead of overlapping or vanishing; a chip wider than
            // the screen is capped and its words shrink to fit. Only a third line is dropped.
            float x = 0, row = 0;
            float tempWidth = Mathf.Min(width, 5 + 18 + 6 + talkTemperamentText.preferredWidth + 12);
            var temperament = talkTemperamentFace.rectTransform;
            LeftLabel(temperament, new Vector2(0, 0), new Vector2(tempWidth, TalkChipHeight));
            LeftLabel(talkTemperamentText.rectTransform, new Vector2(29, 0), new Vector2(tempWidth - 29 - 8, TalkChipHeight));
            FitChipText(talkTemperamentText, tempWidth - 29 - 8);
            x = tempWidth + 6;
            for (int i = 0; i < talkTraitFaces.Length; i++)
            {
                if (talkTraitTexts[i].text.Length == 0) { talkTraitFaces[i].gameObject.SetActive(false); continue; }
                float chipWidth = Mathf.Min(width, talkTraitTexts[i].preferredWidth + 22);
                if (x > 0 && x + chipWidth > width) { x = 0; row++; }
                bool fits = row < 2;
                talkTraitFaces[i].gameObject.SetActive(fits);
                if (!fits) continue;
                LeftLabel(talkTraitFaces[i].rectTransform, new Vector2(x, -row * (TalkChipHeight + 6)), new Vector2(chipWidth, TalkChipHeight));
                FitChipText(talkTraitTexts[i], chipWidth - 16);
                x += chipWidth + 6;
            }
            float rowsHeight = TalkChipHeight + Mathf.Min(row, 1) * (TalkChipHeight + 6);
            LeftLabel(talkTraits, new Vector2(left, -y), new Vector2(width, rowsHeight));
            y += rowsHeight + TalkGap;

            // The warning stays one line when it fits, otherwise wraps onto two inside a taller chip.
            float single = 9 + 16 + 7 + talkWarningText.preferredWidth + 12;
            bool wrap = single > width;
            float warningWidth = Mathf.Min(width, single);
            talkWarningText.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            talkWarningText.alignment = TextAnchor.MiddleLeft;
            float warningHeight = wrap ? 46 : TalkWarningHeight;
            LeftLabel(talkWarning, new Vector2(left, -y), new Vector2(warningWidth, warningHeight));
            LeftLabel(talkWarningText.rectTransform, new Vector2(32, 0), new Vector2(warningWidth - 32 - 10, warningHeight));
            talkWarningIcon.rectTransform.anchoredPosition = new Vector2(9, -(warningHeight - 16) / 2);
            return y + warningHeight + TalkGap;
        }

        /// <summary>Shrinks a chip's words (down to 11) when even a full-width chip can't hold them.</summary>
        private static void FitChipText(Text text, float room)
        {
            text.fontSize = PockleTheme.CaptionSize;
            while (text.fontSize > PockleTheme.MinimumTextSize && text.preferredWidth > room) text.fontSize--;
        }

        /// <summary>
        /// Shows the toy's bubble for a reaction. <paramref name="headScreen"/> is the top of the toy's head in screen pixels;
        /// without it the bubble sits over the upper part of the toy's area. Returns the expression (empty when the toy
        /// stays quiet this time). Codex: call this from the reaction runtime when a tap reaction starts.
        /// </summary>
        public ToyExpression ShowToyExpression(ReactionCue cue, Vector2? headScreen = null, float delay = 0f)
        {
            if (talkBubble == null || page != AppPage.Play || string.IsNullOrEmpty(talkToyId)) return default;
            var expression = expressionPicker.Next(talkToyId, cue, StageLook.IsDark(stageWorld, stageLight));
            if (expression.IsEmpty) return expression;

            talkBubbleText.text = expression.Sound;
            var sticker = StickerTexture(expression.Sticker);
            talkBubbleSticker.texture = sticker;
            talkBubbleSticker.gameObject.SetActive(sticker != null);
            float textWidth = expression.Sound.Length > 0 ? talkBubbleText.preferredWidth : 0;
            float stickerSize = sticker != null ? 26 : 0, gap = textWidth > 0 && sticker != null ? 6 : 0;
            float bubbleWidth = Mathf.Max(48, 16 + textWidth + gap + stickerSize + 16);
            talkBubble.sizeDelta = new Vector2(bubbleWidth, 42);
            var textRect = talkBubbleText.rectTransform;
            textRect.anchorMin = textRect.anchorMax = new Vector2(0, .5f); textRect.pivot = new Vector2(0, .5f);
            textRect.anchoredPosition = new Vector2(16, 0); textRect.sizeDelta = new Vector2(textWidth + 4, 36);
            var stickerRect = talkBubbleSticker.rectTransform;
            stickerRect.anchorMin = stickerRect.anchorMax = new Vector2(0, .5f); stickerRect.pivot = new Vector2(0, .5f);
            stickerRect.anchoredPosition = new Vector2(16 + textWidth + gap, 0); stickerRect.sizeDelta = new Vector2(stickerSize, stickerSize);

            // Keep callouts closer to the toy; arrivals and reaction anchors both sit lower.
            // Clamp the entire bubble inside the free stage area, including when a tray is open.
            Vector2 screen = headScreen ?? new Vector2(StageViewport.center.x, StageViewport.yMax - StageViewport.height * .22f);
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(safeRoot, screen, cam, out var local))
            {
                float half = safeRoot.rect.width / 2 - bubbleWidth / 2 - 12;
                float y = local.y - 10;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(safeRoot, new Vector2(screen.x, StageViewport.yMin), cam, out var bottom)
                    && RectTransformUtility.ScreenPointToLocalPointInRectangle(safeRoot, new Vector2(screen.x, StageViewport.yMax), cam, out var top))
                {
                    float minY = bottom.y + 10;
                    float maxY = top.y - talkBubble.sizeDelta.y - 10;
                    // On very short layouts, keep the header and tray usable instead of overlapping them.
                    if (maxY < minY) { talkPop.Hide(); return expression; }
                    y = Mathf.Clamp(y, minY, maxY);
                }
                talkBubble.anchoredPosition = new Vector2(Mathf.Clamp(local.x, -half, half), y);
            }
            talkBubble.SetAsLastSibling();
            talkPop.Play(delay, reducedMotionEnabled);
            return expression;
        }

        private static Texture2D StickerTexture(ExpressionSticker sticker) =>
            sticker == ExpressionSticker.None ? null : PockleTheme.Icon("Sticker" + sticker);
    }
}
