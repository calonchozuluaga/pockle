using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Makes a closed box look exciting (owner, October 14): a soft coloured glow behind it, sparkles that twinkle around
    /// it, a bright glint now and then, and every few seconds a little "something's in here" shake.
    /// Add it to the box's RawImage. Layout keeps owning that RectTransform; the picture is redrawn by a child that does the
    /// shaking, so positions, sizes and hit areas never change. Calm motion keeps the glow and still sparkles, no shake.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BoxHype : MonoBehaviour
    {
        private const float Cycle = 4.2f, ShakeSeconds = .6f;
        private RawImage source, picture, glow;
        private RectTransform pictureRect;
        private Color glowColour;
        private float phase;
        private readonly System.Collections.Generic.List<GameObject> sparkles = new System.Collections.Generic.List<GameObject>();

        /// <summary>
        /// False keeps the box picture but drops the glow, sparkles and shake (a box that can't be opened yet). Use this
        /// rather than disabling the component: the child draws the picture, so a disabled one showed a blank white square.
        /// </summary>
        public bool Excited { get; set; } = true;

        /// <summary>Adds the effect to <paramref name="box"/>. <paramref name="colour"/> tints the glow (the collection's accent).</summary>
        public static BoxHype Add(RawImage box, Color colour, float phaseSeconds = 0f)
        {
            var hype = box.GetComponent<BoxHype>() ?? box.gameObject.AddComponent<BoxHype>();
            hype.Build(box, colour, phaseSeconds);
            return hype;
        }

        public void SetColour(Color colour) { glowColour = colour; }

        private void Build(RawImage box, Color colour, float phaseSeconds)
        {
            if (source != null) { glowColour = colour; return; }
            source = box; glowColour = colour; phase = phaseSeconds;
            source.color = Color.clear; // the child below draws the picture

            glow = Child("Box glow", new Vector2(-.28f, -.18f), new Vector2(1.28f, 1.22f)).gameObject.AddComponent<RawImage>();
            glow.texture = UiShapes.Glow; glow.raycastTarget = false;

            pictureRect = Child("Box picture", Vector2.zero, Vector2.one);
            pictureRect.pivot = new Vector2(.5f, .08f);
            picture = pictureRect.gameObject.AddComponent<RawImage>(); picture.raycastTarget = false;
            picture.enabled = false; glow.enabled = false; // nothing until the first mirror: an empty RawImage draws white

            // Sparkles around the box (anchor = place on the box, size in px, colour, phase).
            Sparkle(new Vector2(.06f, .86f), 18, Color.white, 0f);
            Sparkle(new Vector2(.94f, 1.0f), 22, new Color(1f, .86f, .42f), .9f);
            Sparkle(new Vector2(1.04f, .42f), 14, Color.white, 1.7f);
            Sparkle(new Vector2(.02f, .30f), 13, new Color(1f, .86f, .42f), 2.4f);
            // The glint: a bigger flash on the top corner, right after each shake.
            var glint = Sparkle(new Vector2(.72f, .9f), 30, Color.white, 0f);
            glint.GetComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Twinkle, phaseSeconds: phase + .3f);
        }

        private RectTransform Child(string name, Vector2 min, Vector2 max)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform; rect.SetParent(transform, false);
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            return rect;
        }

        private RectTransform Sparkle(Vector2 at, float size, Color colour, float sparklePhase)
        {
            var rect = Child("Box sparkle", at, at); rect.sizeDelta = new Vector2(size, size);
            var image = rect.gameObject.AddComponent<RawImage>(); image.texture = UiShapes.Sparkle; image.color = colour; image.raycastTarget = false;
            rect.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Twinkle, phaseSeconds: phase + sparklePhase);
            sparkles.Add(rect.gameObject);
            return rect;
        }

        private void LateUpdate()
        {
            if (source == null) return;
            // Mirror whatever the page set on the box (texture, crop, visible or not).
            picture.texture = source.texture; picture.uvRect = source.uvRect;
            bool shown = source.enabled && source.texture != null;
            picture.enabled = shown; glow.enabled = shown && Excited;
            if (source.color.a > 0) source.color = Color.clear;
            foreach (var sparkle in sparkles) if (sparkle.activeSelf != (shown && Excited)) sparkle.SetActive(shown && Excited);
            if (!Excited) { pictureRect.localRotation = Quaternion.identity; pictureRect.localScale = Vector3.one; return; }

            bool calm = SquishFeedback.Calm;
            float time = Time.unscaledTime + phase;
            float pulse = calm ? .5f : .5f + .5f * Mathf.Sin(time * 2.1f);
            var c = glowColour; c.a = Mathf.Lerp(.38f, .62f, pulse); glow.color = c;

            float roll = 0f, pop = 1f;
            float t = calm ? 99f : Mathf.Repeat(time, Cycle);
            if (t < ShakeSeconds)
            {
                float k = t / ShakeSeconds, fade = 1f - k;
                roll = Mathf.Sin(k * Mathf.PI * 2f * 4f) * 6f * fade;      // four quick wiggles that settle
                pop = 1f + .06f * Mathf.Sin(k * Mathf.PI) ;                // a small swell
            }
            pictureRect.localRotation = Quaternion.Euler(0, 0, roll);
            pictureRect.localScale = new Vector3(pop, pop, 1f);
        }
    }
}
