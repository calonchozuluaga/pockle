using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pockle.Runtime
{
    public sealed partial class TactilePrototype
    {
        private void ApplyLift()
        {
            float requested = revealing ? revealLift : reducedMotion ? lift.Target : lift.Value;
            // Keep the top of a lifted, vertically stretched Pip inside the viewer.
            float scale = Mathf.Max(.01f, toyMount.lossyScale.y);
            float room = Mathf.Max(0f, 3.4f - (toyMount.position.y + toy.DeformedTop * scale)) / scale;
            visibleLift = Mathf.Clamp(requested, 0f, Mathf.Min(revealing ? 1.05f : .75f, room));
            toy.transform.localPosition = toyMount.InverseTransformVector(Vector3.up * visibleLift);
            if (Mathf.Abs(visibleLift - lastShadowLift) < .001f) return;
            lastShadowLift = visibleLift;
            float height = Mathf.Clamp01(visibleLift * scale / .75f);
            float spread = 1f + height * .3f;
            contactShadow.localScale = new Vector3(shadowRestScale.x * spread, shadowRestScale.y, shadowRestScale.z * spread);
            shadowMaterial.color = Color.Lerp(shadowRestColor, new Color(1f, .985f, .957f), height * .72f);
        }

        private void ReadMotion(float deltaTime)
        {
            if (reducedMotion || revealing || hud.StoreVisible) { ClearMotion(); return; }
            if (sensorOrientation != Screen.orientation)
            { sensorOrientation = Screen.orientation; ClearMotion(); }
            Point3 drive = new Point3(0f, 0f, 0f);
            if (Application.isMobilePlatform && SystemInfo.supportsAccelerometer)
            {
                Vector3 acceleration = Input.acceleration;
                drive = motion.Step(new Point3(acceleration.x, acceleration.y, acceleration.z), deltaTime);
            }
            else motion.Reset();
            jiggleX.Target = drive.X;
            jiggleY.Target = drive.Y;
            jiggleZ.Target = drive.Z;
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.J))
            {
                jiggleX.Reset(.30f); jiggleY.Reset(.20f); jiggleZ.Reset(-.16f);
                jiggleX.Target = jiggleY.Target = jiggleZ.Target = 0f;
            }
#endif
        }

        private void ClearMotion()
        {
            motion.Reset();
            jiggleX.Reset(); jiggleY.Reset(); jiggleZ.Reset();
        }
    }
}
