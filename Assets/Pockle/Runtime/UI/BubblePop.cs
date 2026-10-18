using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// Pops a speech bubble in, holds it, then floats it away (canvas "Play: speech bubble", October 16). Calm motion just
    /// fades. Uses unscaled time so pausing the toy never freezes a bubble on screen.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class BubblePop : MonoBehaviour
    {
        public const float In = .18f, Hold = 1.25f, Out = .3f;
        private CanvasGroup group;
        private RectTransform rect;
        private Vector2 home;
        private float start = -1, delay;
        private bool calm;

        public bool Showing => start >= 0;

        private void Awake() { group = GetComponent<CanvasGroup>(); rect = (RectTransform)transform; group.alpha = 0; group.blocksRaycasts = false; }

        /// <summary>Shows the bubble at its current anchored position after <paramref name="wait"/> seconds.</summary>
        public void Play(float wait, bool calmMotion)
        {
            if (group == null) Awake();
            home = rect.anchoredPosition; delay = Mathf.Max(0, wait); calm = calmMotion;
            start = Time.unscaledTime; group.alpha = 0; rect.localScale = Vector3.one;
            gameObject.SetActive(true);
        }

        public void Hide() { start = -1; if (group != null) group.alpha = 0; gameObject.SetActive(false); }

        private void Update()
        {
            if (start < 0) return;
            float t = Time.unscaledTime - start - delay;
            if (t < 0) { group.alpha = 0; return; }
            if (t > In + Hold + Out) { Hide(); rect.anchoredPosition = home; return; }
            if (t < In)
            {
                float k = t / In;
                group.alpha = k;
                // Overshoot a little, like the toy's own squash.
                rect.localScale = calm ? Vector3.one : Vector3.one * Mathf.LerpUnclamped(.6f, 1f, 1f + 2.7f * Mathf.Pow(k - 1f, 3) + 1.7f * Mathf.Pow(k - 1f, 2));
                rect.anchoredPosition = home;
            }
            else if (t < In + Hold) { group.alpha = 1; rect.localScale = Vector3.one; rect.anchoredPosition = home; }
            else
            {
                float k = (t - In - Hold) / Out;
                group.alpha = 1 - k;
                rect.anchoredPosition = home + new Vector2(0, calm ? 0 : 10 * k);
            }
        }
    }
}
