using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Opens one box on the reveal screen: the closed box bobs and waits for a tap, then shakes (longer for rarer tiers),
    /// pops in a flash of its tier colour with a ring and sparkles, and the toy lands with a squash-bounce before its card
    /// fades in. Calm motion is a short crossfade from box to toy with no shake, flash or confetti. Pure presentation: the
    /// result was saved before the first frame, and finishing early (a tap, Skip or Back) never changes it.
    /// </summary>
    public sealed class BoxRevealPlayer : MonoBehaviour
    {
        public const float FullDuration = 2.2f, CalmDuration = .35f;

        public RectTransform Box, Toy, Card, Ring, Glow, Dim, TapHint;
        public Graphic Flash, GlowGraphic, RingGraphic;
        public RectTransform[] Bits = new RectTransform[0];
        public Vector2[] BitTargets = new Vector2[0];
        /// <summary>The moment the box pops (sound/haptics).</summary>
        public Action Opened;
        /// <summary>The toy has landed and its card is fully shown.</summary>
        public Action Landed;

        public enum State { Idle, Waiting, Opening, Shown }
        public State Phase { get; private set; } = State.Idle;
        public bool Calm { get; private set; }

        private float start = -1, duration = FullDuration, shakeEnd = .32f;
        private bool openedFired;

        /// <summary>Shows the closed box, waiting for a tap.</summary>
        public void Present(Color glow, bool calm)
        {
            Calm = calm; Phase = State.Waiting; start = Time.unscaledTime; openedFired = false;
            if (GlowGraphic != null) GlowGraphic.color = new Color(glow.r, glow.g, glow.b, .55f);
            if (RingGraphic != null) RingGraphic.color = glow;
            if (Flash != null) Flash.color = new Color(Mathf.Lerp(glow.r, 1, .55f), Mathf.Lerp(glow.g, 1, .55f), Mathf.Lerp(glow.b, 1, .55f), 0);
            gameObject.SetActive(true);
            Apply(-1);
            ApplyWaiting(0);
        }

        /// <summary>Starts the opening. Rarer tiers shake a little longer before they pop (Common .32, Rare .38, Epic .44, Secret .52).</summary>
        public void Open(RarityTier tier)
        {
            if (Phase != State.Waiting) return;
            Phase = State.Opening; start = Time.unscaledTime; openedFired = false;
            duration = Calm ? CalmDuration : FullDuration;
            shakeEnd = tier == RarityTier.Secret ? .52f : tier == RarityTier.Epic || tier == RarityTier.UltraRare ? .44f
                : tier == RarityTier.Rare || tier == RarityTier.SuperRare ? .38f : .32f;
            Apply(0);
        }

        /// <summary>Jumps to the landed toy and its card (tap during the opening, Skip, Back, tests).</summary>
        public void Complete()
        {
            if (Phase == State.Shown) return;
            if (!openedFired) { openedFired = true; Opened?.Invoke(); }
            Phase = State.Shown; start = -1;
            Apply(1);
            Landed?.Invoke();
        }

        /// <summary>Hides everything (between toys and when the reveal closes).</summary>
        public void Clear() { Phase = State.Idle; start = -1; Apply(-1); if (TapHint != null) TapHint.gameObject.SetActive(false); }

        private void Update()
        {
            if (Phase == State.Waiting) { ApplyWaiting(Time.unscaledTime - start); return; }
            if (Phase != State.Opening) return;
            float p = (Time.unscaledTime - start) / duration;
            if (!openedFired && p >= (Calm ? 0 : shakeEnd + .04f)) { openedFired = true; Opened?.Invoke(); }
            if (p >= 1) { Complete(); return; }
            Apply(p);
        }

        private static float Seg(float p, float a, float b) => Mathf.Clamp01((p - a) / Mathf.Max(.0001f, b - a));
        private static float EaseOut(float t) => 1 - (1 - t) * (1 - t);

        private static void Alpha(RectTransform rect, float alpha)
        {
            if (rect == null) return;
            var group = rect.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = alpha;
            rect.gameObject.SetActive(alpha > .001f);
        }

        /// <summary>The closed box gently bobs (still with calm motion) and the hint breathes.</summary>
        private void ApplyWaiting(float t)
        {
            if (Box != null)
            {
                Alpha(Box, 1);
                Box.anchoredPosition = new Vector2(0, Calm ? 0 : Mathf.Sin(t * 2.4f) * 6);
                Box.localRotation = Quaternion.Euler(0, 0, Calm ? 0 : Mathf.Sin(t * 1.7f) * 2.5f);
                Box.localScale = Vector3.one;
            }
            if (TapHint != null) { TapHint.gameObject.SetActive(true); Alpha(TapHint, Calm ? 1 : .75f + .25f * Mathf.Sin(t * 3)); }
            if (Glow != null) { Alpha(Glow, .5f); Glow.localScale = Vector3.one * (Calm ? 1 : 1 + .04f * Mathf.Sin(t * 2)); }
            Alpha(Dim, .3f);
        }

        /// <summary>p &lt; 0 hides everything; 0..1 is the opening; 1 is the landed toy with its card.</summary>
        private void Apply(float p)
        {
            if (p < 0)
            {
                Alpha(Toy, 0); Alpha(Card, 0); Alpha(Ring, 0); Alpha(Box, 0);
                if (Flash != null) Flash.gameObject.SetActive(false);
                foreach (var bit in Bits) Alpha(bit, 0);
                return;
            }
            if (TapHint != null) TapHint.gameObject.SetActive(false);
            if (Calm) { ApplyCalm(p); return; }

            // Shake: small at first, building up to the pop.
            float shake = Seg(p, 0, shakeEnd), pop = Seg(p, shakeEnd, shakeEnd + .1f);
            if (Box != null)
            {
                float amp = Mathf.Lerp(2, 11, shake * shake);
                Box.localRotation = Quaternion.Euler(0, 0, p < shakeEnd ? Mathf.Sin(p * 95) * amp : 0);
                Box.anchoredPosition = new Vector2(p < shakeEnd ? Mathf.Sin(p * 140) * amp * .5f : 0, 0);
                float s = p < shakeEnd ? Mathf.Lerp(1, .92f, shake) : Mathf.Lerp(.92f, 1.35f, EaseOut(pop));
                Box.localScale = new Vector3(s * (p < shakeEnd ? 1 + .05f * shake : 1), s, 1);
                Alpha(Box, 1 - pop);
            }
            if (Glow != null) { Glow.localScale = Vector3.one * Mathf.Lerp(1, 1.6f, pop); Alpha(Glow, Mathf.Lerp(.5f, 1, pop) * (p > .8f ? 1 - Seg(p, .8f, 1) * .4f : 1)); }
            if (Dim != null) Alpha(Dim, Mathf.Lerp(.3f, 1, shake));

            // Pop: flash, ring, sparkles in the tier's colour.
            float after = shakeEnd + .04f;
            if (Flash != null) { var c = Flash.color; c.a = p < after ? 0 : .85f * (1 - Seg(p, after, after + .14f)); Flash.color = c; Flash.gameObject.SetActive(c.a > .001f); }
            if (Ring != null) { float k = Seg(p, after, after + .22f); Ring.localScale = Vector3.one * Mathf.Lerp(.3f, 3.2f, EaseOut(k)); Alpha(Ring, p < after ? 0 : 1 - k); }
            for (int i = 0; i < Bits.Length && i < BitTargets.Length; i++)
            {
                float k = Seg(p, after, after + .3f);
                Bits[i].anchoredPosition = BitTargets[i] * EaseOut(k) + new Vector2(0, -24 * Seg(p, after + .25f, after + .4f));
                Bits[i].localRotation = Quaternion.Euler(0, 0, i * 53 * k);
                Alpha(Bits[i], p < after ? 0 : 1 - Seg(p, after + .25f, after + .4f));
            }

            // Land: the toy rises out of the glow, drops onto its spot with a squash, settles.
            float land = Seg(p, after + .02f, after + .22f), settle = Seg(p, after + .22f, after + .36f);
            if (Toy != null)
            {
                float y = p < after ? 0 : land < 1 ? Mathf.Lerp(-30, 26, EaseOut(land)) : Mathf.Lerp(26, 0, settle * settle);
                float sx = land < 1 ? Mathf.Lerp(.4f, 1, EaseOut(land)) : settle < .5f ? Mathf.Lerp(1, 1.12f, settle * 2) : Mathf.Lerp(1.12f, 1, (settle - .5f) * 2);
                float sy = land < 1 ? Mathf.Lerp(.4f, 1.06f, EaseOut(land)) : settle < .5f ? Mathf.Lerp(1.06f, .9f, settle * 2) : Mathf.Lerp(.9f, 1, (settle - .5f) * 2);
                Toy.anchoredPosition = new Vector2(0, y); Toy.localScale = new Vector3(sx, sy, 1);
                Alpha(Toy, Seg(p, after, after + .05f));
            }
            if (Card != null) { float k = Seg(p, .82f, 1); Card.anchoredPosition = new Vector2(Card.anchoredPosition.x, Mathf.Lerp(-16, 0, EaseOut(k)) + CardRest); Alpha(Card, k); }
        }

        /// <summary>Where the card rests; set by the HUD's layout.</summary>
        public float CardRest;

        private void ApplyCalm(float p)
        {
            if (Box != null) { Box.localRotation = Quaternion.identity; Box.anchoredPosition = Vector2.zero; Box.localScale = Vector3.one; Alpha(Box, 1 - p); }
            if (Dim != null) Alpha(Dim, 1);
            if (Glow != null) { Glow.localScale = Vector3.one; Alpha(Glow, .6f); }
            if (Flash != null) Flash.gameObject.SetActive(false);
            Alpha(Ring, 0);
            foreach (var bit in Bits) Alpha(bit, 0);
            if (Toy != null) { Toy.anchoredPosition = Vector2.zero; Toy.localScale = Vector3.one; Alpha(Toy, p); }
            if (Card != null) { Card.anchoredPosition = new Vector2(Card.anchoredPosition.x, CardRest); Alpha(Card, p); }
        }
    }
}
