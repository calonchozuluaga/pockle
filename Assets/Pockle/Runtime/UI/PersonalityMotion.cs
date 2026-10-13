using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Living-shelf motion (docs/PERSONALITIES.md): a toy idles in character, and small marks (sparkles, "z", a heart)
    /// come and go. It moves its own RectTransform, which should be a stretched child of the laid-out slot, so layout and
    /// hit areas never change. Motion calm holds the rest pose and leaves the marks visible and still.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PersonalityMotion : MonoBehaviour
    {
        public enum Kind { Toy, Twinkle, FloatUp, Pop }

        private RectTransform rect;
        private CanvasGroup group;
        private Kind kind;
        private PersonalityMotionStyle style;
        private float phase, amplitude = 1f;

        public PersonalityMotionStyle Style => style;

        public void Configure(Kind what, PersonalityMotionStyle motion = PersonalityMotionStyle.Calm, float phaseSeconds = 0f, float scale = 1f)
        {
            rect = (RectTransform)transform;
            kind = what; style = motion; phase = phaseSeconds; amplitude = scale;
            // Marks fade as a group (a bubble and its icon together); toys never fade.
            if (what != Kind.Toy) { group = GetComponent<CanvasGroup>(); if (group == null) group = gameObject.AddComponent<CanvasGroup>(); group.blocksRaycasts = false; }
            Apply(0f);
        }

        public void SetStyle(PersonalityMotionStyle motion) { style = motion; }

        private void Update()
        {
            if (rect == null) return;
            Apply(SquishFeedback.Calm ? -1f : Time.unscaledTime + phase);
        }

        private void OnDisable() { if (rect != null) Apply(-1f); }

        /// <summary>Negative time means rest: no motion, marks fully shown.</summary>
        private void Apply(float time)
        {
            Vector2 offset = Vector2.zero; float roll = 0f; Vector2 scale = Vector2.one; float alpha = 1f;
            if (time >= 0f)
            {
                switch (kind)
                {
                    case Kind.Toy: ToyPose(time, ref offset, ref roll, ref scale); break;
                    case Kind.Twinkle:
                    {
                        float t = Cycle(time, 2.4f);
                        float on = Window(t, .15f, .25f, .40f, .55f);
                        alpha = on; scale = Vector2.one * Mathf.Lerp(.4f, 1f, on); break;
                    }
                    case Kind.FloatUp:
                    {
                        float t = Cycle(time, 3.2f);
                        alpha = t < .3f ? t / .3f : 1f - (t - .3f) / .7f;
                        offset = new Vector2(10f * t, 6f - 28f * t); scale = Vector2.one * Mathf.Lerp(.7f, 1.1f, t); break;
                    }
                    case Kind.Pop:
                    {
                        float t = Cycle(time, 3f);
                        float on = Window(t, .55f, .62f, .90f, .97f);
                        alpha = on; scale = Vector2.one * Mathf.Lerp(.5f, 1f, on); break;
                    }
                }
            }
            rect.anchoredPosition = offset * amplitude;
            rect.localRotation = Quaternion.Euler(0, 0, roll * amplitude);
            rect.localScale = new Vector3(scale.x, scale.y, 1f);
            if (group != null) group.alpha = alpha;
        }

        private void ToyPose(float time, ref Vector2 offset, ref float roll, ref Vector2 scale)
        {
            switch (style)
            {
                case PersonalityMotionStyle.Show:
                {
                    // Hop and pose, land with a squash, then rest for half the cycle.
                    float t = Cycle(time, 2.4f);
                    if (t < .20f) { float k = Ease(t / .20f); offset.y = 10f * k; roll = 6f * k; }
                    else if (t < .35f) { float k = Ease((t - .20f) / .15f); offset.y = 10f * (1 - k); roll = Mathf.Lerp(6f, -4f, k); scale = new Vector2(1 + .04f * k, 1 - .04f * k); }
                    else if (t < .50f) { float k = Ease((t - .35f) / .15f); roll = -4f * (1 - k); scale = new Vector2(1 + .04f * (1 - k), 1 - .04f * (1 - k)); }
                    break;
                }
                case PersonalityMotionStyle.Dream:
                {
                    float s = Mathf.Sin(time * Mathf.PI * 2f / 4.6f);
                    roll = 3f * s; offset.y = 3f * (1f - Mathf.Abs(s)); break;
                }
                case PersonalityMotionStyle.Curious:
                {
                    // Lean toward the neighbour, hold a moment, come back.
                    float t = Cycle(time, 3f);
                    float k = t < .4f ? Ease(t / .4f) : t < .6f ? 1f : 1f - Ease((t - .6f) / .4f);
                    roll = -8f * k; offset.x = 6f * k; break;
                }
                case PersonalityMotionStyle.Calm:
                {
                    float s = .5f - .5f * Mathf.Cos(time * Mathf.PI * 2f / 4f);
                    scale = new Vector2(1 + .03f * s, 1 - .03f * s); break;
                }
                case PersonalityMotionStyle.Sleep:
                {
                    float s = .5f - .5f * Mathf.Cos(time * Mathf.PI * 2f / 5f);
                    roll = Mathf.Lerp(2f, 5f, s); scale = new Vector2(1 + .02f + .02f * s, 1 - .02f - .03f * s); break;
                }
            }
        }

        private static float Cycle(float time, float period) => Mathf.Repeat(time, period) / period;
        private static float Ease(float k) => k * k * (3f - 2f * k);
        /// <summary>0 before <paramref name="rise"/>, 1 between <paramref name="full"/> and <paramref name="hold"/>, 0 after <paramref name="fall"/>.</summary>
        private static float Window(float t, float rise, float full, float hold, float fall)
        {
            if (t <= rise || t >= fall) return 0f;
            if (t < full) return Ease((t - rise) / (full - rise));
            if (t <= hold) return 1f;
            return 1f - Ease((t - hold) / (fall - hold));
        }
    }
}
