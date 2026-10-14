using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Moves the approved painted face in the shader; the imported neutral mesh/paint stay unchanged.</summary>
    public sealed class PeachExpressionDriver : MonoBehaviour
    {
        private static readonly int Blink = Shader.PropertyToID("_Blink"), Happy = Shader.PropertyToID("_Happy"), Surprise = Shader.PropertyToID("_Surprise");
        private static readonly int LeftEye = Shader.PropertyToID("_LeftEye"), RightEye = Shader.PropertyToID("_RightEye"), Mouth = Shader.PropertyToID("_Mouth");
        private Material material;
        private double elapsed;
        private float pressure, lift;
        private bool paused;
        public void Initialize(Material target) { material = target; Apply(default); }
        public void SetContext(float compression, float heldHeight) { pressure = compression; lift = heldHeight; }
        public void SetCenters(Vector3 left, Vector3 right, Vector3 mouth)
        {
            if (material == null) return;
            material.SetVector(LeftEye, new Vector4(left.x, left.y, .15f, .17f));
            material.SetVector(RightEye, new Vector4(right.x, right.y, .15f, .17f));
            material.SetVector(Mouth, new Vector4(mouth.x, mouth.y, .12f, .09f));
        }
        public void Apply(PeachExpressionPose pose)
        {
            if (material == null) return;
            material.SetFloat(Blink, pose.Blink); material.SetFloat(Happy, pose.Happy); material.SetFloat(Surprise, pose.Surprise);
        }
        private void LateUpdate()
        {
            if (paused || material == null) return;
            elapsed += Mathf.Max(0, Time.unscaledDeltaTime);
            Apply(PeachExpression.Sample(elapsed, pressure, lift, PlayerPrefs.GetInt("pockle.prototype.reducedMotion", 0) == 1));
        }
        private void OnApplicationPause(bool value) { paused = value; }
    }
}
