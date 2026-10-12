using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Four visible actors, one disabled camera, and at most four 192px live targets at 15 fps.</summary>
    [DisallowMultipleComponent]
    public sealed class ShelfPreviewRuntime : MonoBehaviour
    {
        public const int Capacity = 4;
        public const int Resolution = 192;
        public const float FramesPerSecond = 15f;
        public sealed class Binding
        {
            internal RawImage Picture;
            internal CharacterSilhouetteGraphic Silhouette;
            internal Texture Fallback;
            internal string Id, Profile;
            internal bool Found;
            internal float Phase;
        }
        private sealed class Actor
        {
            public Binding Binding;
            public GameObject Root;
            public JellyToy Toy;
            public ToyLineupAnimator Animator;
            public RenderTexture Target;
            public IDisposable Slot;
            public Vector2 RestPosition;
            public Vector3 RestScale;
            public Quaternion RestRotation;
            public double Elapsed;
            public int ReadyFrame;
        }
        private readonly List<Binding> bindings = new List<Binding>();
        private readonly List<Binding> wanted = new List<Binding>(Capacity);
        private readonly List<Binding> visibleFound = new List<Binding>(16), visibleMissing = new List<Binding>(16);
        private readonly Actor[] actors = { new Actor(), new Actor(), new Actor(), new Actor() };
        private readonly LineupMotionBudget budget = new LineupMotionBudget(Capacity);
        private readonly Vector3[] corners = new Vector3[4];
        private RectTransform viewport, occluder;
        private Camera portraitCamera;
        private bool calm, blocked, paused, focusLost;
        private float nextFrame;
        private bool disposed;
        public int ActiveCount => budget.ActiveCount;
        public int ActiveSilhouetteCount { get { int count = 0; foreach (var actor in actors) if (actor.Binding != null && !actor.Binding.Found && actor.Slot != null) count++; return count; } }
        public int AllocatedTargetCount { get { int count = 0; foreach (var actor in actors) if (actor.Target != null) count++; return count; } }
        public int CameraCount => portraitCamera != null ? 1 : 0;
        public int RenderedFrameCount { get; private set; }

        public void Initialize(RectTransform visibleViewport) { viewport = visibleViewport; }
        public void SetOcclusion(RectTransform overlay) { occluder = overlay; InvalidateView(); }
        public Binding Register(RawImage picture, CharacterSilhouetteGraphic silhouette, string id)
        {
            if (picture == null || silhouette == null || !ToyCatalog.TryGetCollectible(id, out var definition))
                throw new ArgumentException("A shelf preview needs a catalog ID and both presentations.");
            silhouette.Configure(definition.CharacterId, definition.IsSecret);
            var binding = new Binding { Picture = picture, Silhouette = silhouette, Id = id,
                Profile = definition.IsSecret ? "pip" : definition.CharacterId, Fallback = picture.texture, Phase = bindings.Count * .173f };
            bindings.Add(binding); return binding;
        }
        public void Unregister(Binding binding)
        {
            foreach (var actor in actors) if (actor.Binding == binding) Release(actor);
            bindings.Remove(binding);
        }
        public void SetFound(Binding binding, bool found)
        {
            found = found && ToyCatalog.TryGetCollectible(binding.Id, out var definition) && definition.Available &&
                PipVariants.TryFromCollectibleId(binding.Id, out _);
            if (binding.Found != found)
            { foreach (var actor in actors) if (actor.Binding == binding) Release(actor); binding.Found = found; }
            binding.Picture.gameObject.SetActive(found);
            binding.Silhouette.gameObject.SetActive(!found);
        }
        public void SetCalm(bool value) { if (calm == value) return; calm = value; InvalidateView(); }
        public void SetBlocked(bool value) { if (blocked == value) return; blocked = value; if (value) InvalidateView(); }
        public void InvalidateView() { foreach (var actor in actors) Release(actor); nextFrame = 0; }

        private bool InsideViewport(Binding binding)
        {
            var rect = binding.Found ? binding.Picture.rectTransform : binding.Silhouette.rectTransform;
            if (rect == null || !rect.gameObject.activeInHierarchy) return false;
            rect.GetWorldCorners(corners);
            Vector3 low = viewport.InverseTransformPoint(corners[0]), high = low;
            for (int i = 1; i < 4; i++)
            { Vector3 p = viewport.InverseTransformPoint(corners[i]); low = Vector3.Min(low, p); high = Vector3.Max(high, p); }
            // Require at least a quarter of a portrait so a clipped sliver doesn't take a motion slot.
            Rect visible = viewport.rect;
            if (occluder != null && occluder.gameObject.activeInHierarchy)
            {
                occluder.GetWorldCorners(corners);
                float overlayTop = viewport.InverseTransformPoint(corners[1]).y;
                visible.yMin = Mathf.Max(visible.yMin, Mathf.Min(visible.yMax, overlayTop));
            }
            float width = Mathf.Max(0, Mathf.Min(high.x, visible.xMax) - Mathf.Max(low.x, visible.xMin));
            float height = Mathf.Max(0, Mathf.Min(high.y, visible.yMax) - Mathf.Max(low.y, visible.yMin));
            return width * height >= (high.x - low.x) * (high.y - low.y) * .25f && width > 0 && height > 0;
        }
        private void LateUpdate()
        {
            if (disposed || viewport == null || calm || blocked || paused || focusLost)
            { InvalidateView(); return; }
            wanted.Clear(); visibleFound.Clear(); visibleMissing.Clear();
            foreach (var binding in bindings)
                if (InsideViewport(binding)) { if (binding.Found) visibleFound.Add(binding); else visibleMissing.Add(binding); }
            // A tall viewport can show both groups. Reserve room for the eager
            // silhouettes rather than letting the first four found toys starve them.
            int foundLimit = visibleMissing.Count > 0 ? Mathf.Min(2, visibleFound.Count) : Capacity;
            for (int i = 0; i < visibleFound.Count && i < foundLimit; i++) wanted.Add(visibleFound[i]);
            for (int i = 0; i < visibleMissing.Count && wanted.Count < Capacity; i++) wanted.Add(visibleMissing[i]);
            for (int i = foundLimit; i < visibleFound.Count && wanted.Count < Capacity; i++) wanted.Add(visibleFound[i]);
            // Release before acquiring; a new viewport never waits behind offscreen actors.
            foreach (var actor in actors) if (actor.Binding != null && !wanted.Contains(actor.Binding)) Release(actor);
            foreach (var binding in wanted)
            {
                bool assigned = false; foreach (var actor in actors) if (actor.Binding == binding) { assigned = true; break; }
                if (assigned) continue;
                foreach (var actor in actors) if (actor.Binding == null) { Assign(actor, binding); break; }
            }
            foreach (var actor in actors)
            {
                if (actor.Binding == null || actor.Binding.Found) continue;
                actor.Elapsed += Time.unscaledDeltaTime;
                var pose = CharacterMotion.Sample(actor.Binding.Profile, actor.Elapsed, eager: true, phaseOffset: Array.IndexOf(actors, actor) * .075f);
                var rect = actor.Binding.Silhouette.rectTransform;
                rect.anchoredPosition = actor.RestPosition + Vector2.up * (pose.Height * rect.rect.height * .45f);
                rect.localScale = Vector3.Scale(actor.RestScale, new Vector3(1 + pose.Compression * .5f,
                    1 - pose.Compression + pose.Stretch, 1));
                rect.localRotation = actor.RestRotation * Quaternion.Euler(0, 0, pose.RollDegrees + pose.YawDegrees * .25f);
            }
            if (Time.unscaledTime < nextFrame) return;
            nextFrame = Time.unscaledTime + 1f / FramesPerSecond;
            foreach (var actor in actors) if (actor.Binding != null && actor.Binding.Found && Time.frameCount >= actor.ReadyFrame) Render(actor);
        }
        private void Assign(Actor actor, Binding binding)
        {
            actor.Binding = binding; actor.Elapsed = 0;
            if (!binding.Found)
            {
                var rect = binding.Silhouette.rectTransform;
                actor.RestPosition = rect.anchoredPosition; actor.RestScale = rect.localScale; actor.RestRotation = rect.localRotation;
                actor.Slot = budget.TryAcquire(); return;
            }
            if (actor.Root == null)
            {
                int index = Array.IndexOf(actors, actor);
                actor.Root = new GameObject("Visible shelf toy " + index);
                actor.Root.transform.SetParent(transform, false);
                actor.Root.transform.localPosition = new Vector3(1000 + index * 10, 0, 0);
                var toyObject = new GameObject("Shelf-only Pip"); toyObject.transform.SetParent(actor.Root.transform, false);
                actor.Toy = toyObject.AddComponent<JellyToy>(); actor.Toy.Initialize();
                actor.Animator = toyObject.AddComponent<ToyLineupAnimator>();
            }
            actor.Root.SetActive(true); actor.Toy.SetVariant(PipVariants.TryFromCollectibleId(binding.Id, out var choice) ? choice : PipVariant.PeachJelly);
            foreach (Transform child in actor.Root.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 30;
            actor.Animator.Configure(actor.Toy, budget, phaseOffset: binding.Phase);
            actor.Animator.SetVisible(true);
            // Finish changes destroy old filling at the end of this frame. Keep the static
            // fallback until it is gone, rather than flashing the previous finish's pearls.
            actor.ReadyFrame = Time.frameCount + 1;
        }
        private void Render(Actor actor)
        {
            if (portraitCamera == null)
            {
                var cameraObject = new GameObject("Shared shelf preview camera", typeof(Camera));
                cameraObject.transform.SetParent(transform, false); portraitCamera = cameraObject.GetComponent<Camera>();
                portraitCamera.enabled = false; portraitCamera.cullingMask = 1 << 30;
                portraitCamera.orthographic = true; portraitCamera.orthographicSize = 1.13f;
                portraitCamera.clearFlags = CameraClearFlags.SolidColor;
                portraitCamera.nearClipPlane = .1f; portraitCamera.farClipPlane = 8;
                portraitCamera.allowHDR = false; portraitCamera.allowMSAA = false;
            }
            if (actor.Target == null)
            {
                actor.Target = new RenderTexture(Resolution, Resolution, 16, RenderTextureFormat.ARGB32)
                    { name = "Live shelf portrait", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                actor.Target.Create();
            }
            portraitCamera.transform.position = actor.Root.transform.position + new Vector3(0, .18f, -5);
            portraitCamera.transform.LookAt(actor.Root.transform.position + new Vector3(0, .05f, 0));
            portraitCamera.backgroundColor = PockleTheme.FieldFor(actor.Binding.Id);
            try { portraitCamera.targetTexture = actor.Target; portraitCamera.Render(); RenderedFrameCount++; }
            finally { portraitCamera.targetTexture = null; }
            actor.Binding.Picture.texture = actor.Target;
        }
        private void Release(Actor actor)
        {
            if (actor.Binding == null) return;
            if (actor.Binding.Found)
            {
                if (actor.Binding.Picture != null) actor.Binding.Picture.texture = actor.Binding.Fallback;
                if (actor.Animator != null) actor.Animator.SetVisible(false);
                if (actor.Root != null) actor.Root.SetActive(false);
            }
            else if (actor.Binding.Silhouette != null)
            {
                var rect = actor.Binding.Silhouette.rectTransform;
                rect.anchoredPosition = actor.RestPosition; rect.localScale = actor.RestScale; rect.localRotation = actor.RestRotation;
            }
            if (actor.Slot != null) { actor.Slot.Dispose(); actor.Slot = null; }
            actor.Binding = null;
        }
        private void OnDisable() { InvalidateView(); }
        private void OnApplicationPause(bool value) { paused = value; if (value) InvalidateView(); }
        private void OnApplicationFocus(bool value) { focusLost = !value; if (!value) InvalidateView(); }
        public void Dispose()
        {
            if (disposed) return; disposed = true; InvalidateView(); bindings.Clear();
            foreach (var actor in actors)
            {
                if (actor.Root != null) Destroy(actor.Root);
                if (actor.Target != null) { actor.Target.Release(); Destroy(actor.Target); actor.Target = null; }
            }
            if (portraitCamera != null) { Destroy(portraitCamera.gameObject); portraitCamera = null; }
        }
        private void OnDestroy() { Dispose(); }
    }
}
