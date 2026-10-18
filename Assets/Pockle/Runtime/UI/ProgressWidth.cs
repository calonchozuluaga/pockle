using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Draws a progress fill as a rounded bar whose width follows <see cref="Image.fillAmount"/>. A Filled image cut the
    /// rounded sprite and squeezed its ends into points on device; this keeps the pill shape at every amount.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class ProgressWidth : MonoBehaviour
    {
        private Image image;
        private RectTransform rect;
        private float shown = -1f, height = -1f;

        private void Awake() { image = GetComponent<Image>(); rect = (RectTransform)transform; }

        private void LateUpdate()
        {
            if (image == null) return;
            // Round ends: corner radius is half the bar's height (the shared sprite has 64 px corners).
            float h = rect.rect.height;
            if (!Mathf.Approximately(h, height) && h > 0) { height = h; image.pixelsPerUnitMultiplier = 64f / Mathf.Max(1f, h / 2f); }
            float amount = Mathf.Clamp01(image.fillAmount);
            if (Mathf.Approximately(amount, shown)) return;
            shown = amount;
            rect.anchorMin = Vector2.zero; rect.anchorMax = new Vector2(amount, 1f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            // A sliver narrower than its own height can't stay round; show nothing until there's real progress.
            image.canvasRenderer.SetAlpha(amount > 0.001f ? 1f : 0f);
        }
    }
}
