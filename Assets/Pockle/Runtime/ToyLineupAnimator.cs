using System;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Opt-in animation for visible lineup meshes only. Never attached to shelf portraits or the play controller.</summary>
    [DisallowMultipleComponent]
    public sealed class ToyLineupAnimator : MonoBehaviour
    {
        private JellyToy toy;
        private LineupMotionBudget budget;
        private IDisposable slot;
        private Vector3 restPosition;
        private Quaternion restRotation;
        private string profileId;
        private double elapsed;
        private float phase;
        private bool visible, calm, eager, posed, paused;
        public bool IsAnimating => enabled && slot != null && toy != null && toy.isActiveAndEnabled && gameObject.activeInHierarchy;

        private void Awake() { enabled = false; }

        public bool Configure(JellyToy target, LineupMotionBudget motionBudget, bool eager = false, float phaseOffset = 0)
        {
            if (target == null || target.gameObject != gameObject || motionBudget == null || float.IsNaN(phaseOffset) || float.IsInfinity(phaseOffset) ||
                !ToyCatalog.TryGetCollectible(target.CollectibleId, out var definition) ||
                !CharacterMotion.TryGetProfile(definition.CharacterId, out _)) return false;
            StopMotion(); enabled = false;
            toy = target; budget = motionBudget; profileId = definition.CharacterId;
            restPosition = target.transform.localPosition; restRotation = target.transform.localRotation;
            this.eager = eager; phase = phaseOffset; elapsed = 0; visible = false;
            return true;
        }

        /// <returns>False if a visible actor must wait for a free animation slot.</returns>
        public bool SetVisible(bool value) { visible = value; return RefreshRunning(); }
        public void SetCalm(bool value) { calm = value; elapsed = 0; RefreshRunning(); }
        public void SetEager(bool value) { eager = value; elapsed = 0; }

        private bool RefreshRunning()
        {
            bool wantsMotion = toy != null && toy.isActiveAndEnabled && budget != null && visible && !calm && !paused && gameObject.activeInHierarchy;
            if (!wantsMotion) { StopMotion(); enabled = false; return true; }
            if (slot == null) slot = budget.TryAcquire();
            enabled = slot != null;
            return enabled;
        }

        private void Update()
        {
            if (!IsAnimating) { StopMotion(); enabled = false; return; }
            float dt = Time.unscaledDeltaTime;
            if (float.IsNaN(dt) || float.IsInfinity(dt) || dt < 0) return;
            elapsed += dt;
            var pose = CharacterMotion.Sample(profileId, elapsed, eager, calm, phase);
            toy.transform.localPosition = restPosition + Vector3.up * pose.Height;
            toy.transform.localRotation = restRotation * Quaternion.Euler(pose.PitchDegrees, pose.YawDegrees, pose.RollDegrees);
            toy.SetDeformation(pose.Compression, pose.Stretch, 0, 0, 0, 0);
            posed = true;
        }

        private void StopMotion()
        {
            if (slot != null) { slot.Dispose(); slot = null; }
            if (!posed || toy == null) return;
            toy.transform.localPosition = restPosition; toy.transform.localRotation = restRotation;
            toy.ResetToy(); posed = false;
        }
        private void OnEnable() { RefreshRunning(); }
        private void OnDisable() { StopMotion(); }
        private void OnDestroy() { StopMotion(); }
        private void OnApplicationPause(bool value) { paused = value; elapsed = 0; RefreshRunning(); }
    }
}
