using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// UX-002 press feedback: a button squishes when pressed and springs back with a small bounce.
    /// The squish scales each graphic's vertices around the button's centre, so layout, raycast
    /// areas, and anchored positions never change. Calm motion keeps a small dip without bounce.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SquishFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        /// <summary>Mirrors the player's Motion calm preference.</summary>
        public static bool Calm;

        private const float Stiffness = 520f;
        private const float BounceDamping = 17f;
        private const float CalmDamping = 46f;
        private readonly List<SquishMesh> meshes = new List<SquishMesh>();
        private Selectable selectable;
        private bool pressed, moving;
        private float scale = 1f, velocity;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (selectable == null) selectable = GetComponent<Selectable>();
            if (selectable != null && !selectable.IsInteractable()) return;
            pressed = true; moving = true;
        }

        public void OnPointerUp(PointerEventData eventData) { Release(); }
        public void OnPointerExit(PointerEventData eventData) { Release(); }

        private void Release()
        {
            if (!pressed) return;
            pressed = false; moving = true;
        }

        private void OnDisable()
        {
            pressed = false; moving = false; velocity = 0f;
            Apply(1f);
        }

        private void Update()
        {
            if (!moving) return;
            float target = pressed ? (Calm ? PockleTheme.CalmPressScale : PockleTheme.PressScale) : 1f;
            float damping = Calm ? CalmDamping : BounceDamping;
            // Fixed small substeps keep the spring stable on slow frames.
            float remaining = Mathf.Min(Time.unscaledDeltaTime, 1f / 20f);
            while (remaining > 0f)
            {
                float dt = Mathf.Min(remaining, 1f / 120f);
                velocity += (Stiffness * (target - scale) - damping * velocity) * dt;
                scale += velocity * dt;
                remaining -= dt;
            }
            if (!pressed && Mathf.Abs(scale - 1f) < .0005f && Mathf.Abs(velocity) < .01f)
            {
                scale = 1f; velocity = 0f; moving = false;
            }
            Apply(scale);
        }

        private void Apply(float value)
        {
            if (meshes.Count == 0 && Mathf.Approximately(value, 1f)) return;
            if (meshes.Count == 0) Collect();
            var rect = (RectTransform)transform;
            Vector3 worldCentre = rect.TransformPoint(rect.rect.center);
            for (int i = meshes.Count - 1; i >= 0; i--)
            {
                if (meshes[i] == null) { meshes.RemoveAt(i); continue; }
                meshes[i].Set(worldCentre, value);
            }
        }

        private void Collect()
        {
            // Collected on first press so graphics added after the button (portraits, icons) are included.
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                var mesh = graphic.GetComponent<SquishMesh>();
                if (mesh == null) mesh = graphic.gameObject.AddComponent<SquishMesh>();
                meshes.Add(mesh);
            }
        }
    }
}
