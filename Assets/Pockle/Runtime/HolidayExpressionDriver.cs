using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Bounded local painted-face and gill/arm motion; no extra face geometry.</summary>
    public sealed class HolidayExpressionDriver : MonoBehaviour
    {
        private Material material;
        private double elapsed;
        private float pressure, lift;
        private bool paused;
        private PeachExpressionPose reaction;
        private float reactionRemaining;
        /// <summary>Animation/audio events can hold a local expression for up to three seconds.</summary>
        public void React(PeachExpressionPose pose, float seconds)
        { reaction = pose; reactionRemaining = float.IsNaN(seconds) || float.IsInfinity(seconds) ? 0 : Mathf.Clamp(seconds, 0, 3); }
        public void Initialize(Material target) { material = target; Apply(default, 0); }
        public void SetContext(float compression, float heldHeight) { pressure = compression; lift = heldHeight; }
        public void Apply(PeachExpressionPose expression, float wave)
        {
            if (material == null) return;
            material.SetFloat("_Blink", expression.Blink);
            material.SetFloat("_Happy", expression.Happy);
            material.SetFloat("_Surprise", expression.Surprise);
            material.SetFloat("_IdleWave", float.IsNaN(wave) || float.IsInfinity(wave) ? 0 : Mathf.Clamp(wave, -1, 1));
        }
        private void LateUpdate()
        {
            if (paused || material == null) return;
            bool calm = PlayerPrefs.GetInt("pockle.prototype.reducedMotion", 0) == 1;
            elapsed += Mathf.Max(0, Time.unscaledDeltaTime);
            var pose = PeachExpression.Sample(elapsed, pressure, lift, calm);
            if (!calm && reactionRemaining > 0) pose = reaction;
            reactionRemaining = Mathf.Max(0, reactionRemaining - Time.unscaledDeltaTime);
            Apply(pose, calm ? 0 : Mathf.Sin((float)(elapsed % 20) * 1.7f));
        }
        private void OnApplicationPause(bool value) { paused = value; }
    }
}
