using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// The ~4.6 s fusion moment before the reveal (owner-approved canvas "The fusion moment", October 16): commit, gather
    /// (the two spares spiral in), squish into one swirling orb, charge (their temperaments fly in), pop, land. Pure
    /// presentation: the fused toy is already saved before the first frame, and a tap skips straight to the reveal.
    /// Timings are fractions of <see cref="Duration"/>, matching the canvas keyframes.
    /// </summary>
    public sealed class FusionMomentPlayer : MonoBehaviour
    {
        public const float Duration = 4.6f;

        public RectTransform ToyA, ToyB, Orb, OrbSwirl, OrbGlow, BadgeA, BadgeB, Say, Secret, Ring, Child, Title, Dim;
        public Graphic Flash;
        public RectTransform[] Bits = new RectTransform[0];
        public Vector2[] BitTargets = new Vector2[0];
        public Action Finished;

        private float start = -1;
        private bool secretRecipe;

        public bool Playing => start >= 0;

        public void Play(bool secret)
        {
            secretRecipe = secret;
            start = Time.unscaledTime;
            gameObject.SetActive(true);
            Apply(0);
        }

        /// <summary>Ends at once (a tap, Back, or leaving the page) and hands over to the reveal.</summary>
        public void Skip()
        {
            if (start < 0) return;
            start = -1;
            gameObject.SetActive(false);
            Finished?.Invoke();
        }

        private void Update()
        {
            if (start < 0) return;
            float p = (Time.unscaledTime - start) / Duration;
            if (p >= 1.08f) { Skip(); return; } // a short hold on the landed toy, then the reveal
            Apply(Mathf.Clamp01(p));
        }

        private static float Seg(float p, float a, float b) => Mathf.Clamp01((p - a) / Mathf.Max(.0001f, b - a));
        private static float EaseOut(float t) => 1 - (1 - t) * (1 - t);

        /// <summary>Piecewise-linear lookup over (time, value) pairs.</summary>
        private static float Keys(float p, params float[] kv)
        {
            if (p <= kv[0]) return kv[1];
            for (int i = 2; i < kv.Length; i += 2)
                if (p <= kv[i]) return Mathf.Lerp(kv[i - 1], kv[i + 1], Seg(p, kv[i - 2], kv[i]));
            return kv[kv.Length - 1];
        }

        private static void Alpha(RectTransform rect, float alpha)
        {
            if (rect == null) return;
            var group = rect.GetComponent<CanvasGroup>();
            if (group != null) group.alpha = alpha;
            rect.gameObject.SetActive(alpha > .001f);
        }

        private void Apply(float p)
        {
            Alpha(Dim, Keys(p, .10f, 0f, .40f, 1f, .72f, 1f, .90f, .35f));

            // Gather: crouch, hop, then spiral in, faster each lap.
            float angle = Keys(p, .14f, 0f, .24f, 110f, .32f, 290f, .40f, 540f) * Mathf.Deg2Rad;
            float radius = Keys(p, .14f, 100f, .24f, 80f, .32f, 50f, .40f, 14f, .46f, 0f);
            float hop = p < .06f ? 0 : p < .11f ? 26 * Seg(p, .06f, .11f) : p < .15f ? 26 * (1 - Seg(p, .11f, .15f)) : 0;
            float squashX = p < .06f ? Mathf.Lerp(1, 1.1f, Seg(p, 0, .06f)) : p < .11f ? Mathf.Lerp(1.1f, .94f, Seg(p, .06f, .11f)) : p < .15f ? Mathf.Lerp(.94f, .9f, Seg(p, .11f, .15f))
                : p < .40f ? Mathf.Lerp(.9f, .8f, Seg(p, .15f, .40f)) : p < .44f ? Mathf.Lerp(.8f, 1.15f, Seg(p, .40f, .44f)) : Mathf.Lerp(1.15f, .2f, Seg(p, .44f, .48f));
            float squashY = p < .06f ? Mathf.Lerp(1, .86f, Seg(p, 0, .06f)) : p < .11f ? Mathf.Lerp(.86f, 1.08f, Seg(p, .06f, .11f)) : p < .15f ? Mathf.Lerp(1.08f, .9f, Seg(p, .11f, .15f))
                : p < .40f ? Mathf.Lerp(.9f, .8f, Seg(p, .15f, .40f)) : p < .44f ? Mathf.Lerp(.8f, .7f, Seg(p, .40f, .44f)) : Mathf.Lerp(.7f, .2f, Seg(p, .44f, .48f));
            float toyAlpha = p < .44f ? 1 : 1 - Seg(p, .44f, .48f);
            for (int side = 0; side < 2; side++)
            {
                var toy = side == 0 ? ToyA : ToyB;
                if (toy == null) continue;
                float r = side == 0 ? -radius : radius;
                toy.anchoredPosition = new Vector2(r * Mathf.Cos(angle), r * Mathf.Sin(angle) + hop);
                toy.localScale = new Vector3(squashX, squashY, 1);
                Alpha(toy, toyAlpha);
            }

            // Squish into the orb, which pulses faster as it charges, then bursts.
            float orbScale = p < .42f ? 0 : Keys(p, .42f, 0f, .46f, 1.1f, .50f, .95f, .54f, 1f, .58f, 1.05f, .61f, 1f, .64f, 1.07f, .66f, 1f, .68f, 1.09f, .70f, 1.02f, .72f, 1.25f, .75f, 0f);
            if (Orb != null) { Orb.localScale = Vector3.one * orbScale; Orb.gameObject.SetActive(orbScale > .01f); }
            if (OrbSwirl != null) OrbSwirl.localRotation = Quaternion.Euler(0, 0, -p * Duration * 450f);
            if (OrbGlow != null) { float g = p < .44f ? 0 : p < .72f ? Mathf.Lerp(.4f, 1.6f, Seg(p, .44f, .72f)) : Mathf.Lerp(1.6f, 0f, Seg(p, .72f, .78f)); OrbGlow.localScale = Vector3.one * g; Alpha(OrbGlow, Mathf.Clamp01(g)); }

            // Charge: each parent's temperament flies into the orb.
            for (int side = 0; side < 2; side++)
            {
                var badge = side == 0 ? BadgeA : BadgeB;
                if (badge == null) continue;
                float appear = Seg(p, .46f, .50f), fly = Seg(p, .54f, .63f);
                float x = (side == 0 ? -1 : 1) * Mathf.Lerp(120, 0, EaseOut(fly));
                badge.anchoredPosition = new Vector2(x, Mathf.Lerp(-10, 0, appear));
                badge.localScale = Vector3.one * Mathf.Lerp(1, .3f, fly);
                Alpha(badge, p < .63f ? appear : 0);
            }
            Alpha(Say, p < .48f ? 0 : p < .52f ? Seg(p, .48f, .52f) : p < .70f ? 1 : 1 - Seg(p, .70f, .74f));
            if (Secret != null)
            {
                float s = secretRecipe ? (p < .52f ? 0 : p < .56f ? Seg(p, .52f, .56f) : p < .70f ? 1 : 1 - Seg(p, .70f, .74f)) : 0;
                Secret.localScale = Vector3.one * (p < .56f ? Mathf.Lerp(.6f, 1.1f, Seg(p, .52f, .56f)) : p < .59f ? Mathf.Lerp(1.1f, 1f, Seg(p, .56f, .59f)) : 1f);
                Alpha(Secret, s);
            }

            // Pop: flash, ring, confetti in both colours.
            if (Flash != null) { var c = Flash.color; c.a = p < .72f ? 0 : p < .75f ? .92f * Seg(p, .72f, .75f) : .92f * (1 - Seg(p, .75f, .84f)); Flash.color = c; Flash.gameObject.SetActive(c.a > .001f); }
            if (Ring != null) { float k = Seg(p, .74f, .88f); Ring.localScale = Vector3.one * Mathf.Lerp(.2f, 3.4f, EaseOut(k)); Alpha(Ring, p < .74f ? 0 : 1 - k); }
            for (int i = 0; i < Bits.Length && i < BitTargets.Length; i++)
            {
                float k = Seg(p, .74f, .96f);
                Bits[i].anchoredPosition = BitTargets[i] * EaseOut(k) + new Vector2(0, p > .96f ? -30 * Seg(p, .96f, 1f) : 0);
                Bits[i].localRotation = Quaternion.Euler(0, 0, i * 67 * k);
                Alpha(Bits[i], p < .74f ? 0 : p < .96f ? 1 : 1 - Seg(p, .96f, 1f));
            }

            // Land: the new toy drops in with a squash-bounce, then its name.
            if (Child != null)
            {
                float y = p < .79f ? 70 : p < .84f ? Mathf.Lerp(70, 0, Seg(p, .79f, .84f) * Seg(p, .79f, .84f)) : p < .88f ? Mathf.Lerp(0, 8, Seg(p, .84f, .88f)) : Mathf.Lerp(8, 0, Seg(p, .88f, .92f));
                float sx = p < .79f ? .6f : p < .84f ? Mathf.Lerp(.6f, 1.16f, Seg(p, .79f, .84f)) : p < .88f ? Mathf.Lerp(1.16f, .95f, Seg(p, .84f, .88f)) : Mathf.Lerp(.95f, 1f, Seg(p, .88f, .92f));
                float sy = p < .79f ? .6f : p < .84f ? Mathf.Lerp(.6f, .84f, Seg(p, .79f, .84f)) : p < .88f ? Mathf.Lerp(.84f, 1.05f, Seg(p, .84f, .88f)) : Mathf.Lerp(1.05f, 1f, Seg(p, .88f, .92f));
                Child.anchoredPosition = new Vector2(0, y); Child.localScale = new Vector3(sx, sy, 1);
                Alpha(Child, Seg(p, .76f, .79f));
            }
            if (Title != null) { float k = Seg(p, .88f, .96f); Title.anchoredPosition = new Vector2(0, Mathf.Lerp(-14, 0, k)); Alpha(Title, k); }
        }
    }
}
