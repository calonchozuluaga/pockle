using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pockle.Runtime
{
    public sealed partial class TactilePrototype
    {
        private void ReadTouches()
        {
            if (!gesture.IsActive)
            {
                for (int i = 0; i < Input.touchCount && !gesture.IsActive; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (touch.phase == TouchPhase.Began) BeginPointer(touch.fingerId, touch.position);
                }
            }
            if (!gesture.IsActive) return;
            Touch first, second;
            bool foundFirst = FindTouch(gesture.PointerId, out first);
            bool liveFirst = foundFirst && IsLive(first);
            if (gesture.IsPair)
            {
                bool foundSecond = FindTouch(gesture.SecondPointerId, out second);
                bool liveSecond = foundSecond && IsLive(second);
                if (liveFirst && liveSecond) { MovePair(first.position, second.position); return; }
                if (!liveFirst && !liveSecond)
                {
                    ReleasePointer(foundFirst && first.phase == TouchPhase.Ended &&
                        foundSecond && second.phase == TouchPhase.Ended);
                    return;
                }
                int ended = liveFirst ? gesture.SecondPointerId : gesture.PointerId;
                gesture.End(ended); // Promote the survivor if the first finger left.
                Touch survivor = liveFirst ? first : second;
                pointerStart = pointerPrevious = survivor.position;
                liftStart = visibleLift;
                lift.Target = liftStart;
                pinch.Target = 0f; // Ease out rather than snapping the mesh.
                hud.SetStatus("Drag up to lift. Add a finger to stretch.");
                return;
            }
            if (!liveFirst) { ReleasePointer(foundFirst && first.phase == TouchPhase.Ended); return; }
            if (gesture.Target == PointerTarget.Toy)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch candidate = Input.GetTouch(i);
                    if (candidate.phase != TouchPhase.Began || candidate.fingerId == gesture.PointerId) continue;
                    if (!CanJoinToy(candidate) || Vector2.Distance(first.position, candidate.position) < 8f) continue;
                    if (!gesture.TryJoin(candidate.fingerId, PointerTarget.Toy)) continue;
                    pairStartDistance = Vector2.Distance(first.position, candidate.position);
                    pairStartCenter = (first.position + candidate.position) * .5f;
                    pairLiftStart = visibleLift;
                    pairStartPinch = pinch.Target;
                    lift.Target = pairLiftStart;
                    hud.SetStatus("Pull apart to stretch. Pinch to squish.");
                    MovePair(first.position, candidate.position);
                    return;
                }
            }
            MovePointer(first.position);
        }

        private bool CanJoinToy(Touch touch)
        {
            if (!viewCamera.pixelRect.Contains(touch.position)) return false;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return false;
            Vector3 hit;
            return toy.RaycastBody(viewCamera.ScreenPointToRay(touch.position), out hit);
        }

        private void MovePair(Vector2 first, Vector2 second)
        {
            Vector2 separation = second - first;
            float distance = separation.magnitude;
            pinch.Target = Mathf.Clamp(pairStartPinch + (distance / pairStartDistance - 1f) * .75f, -.35f, .55f);
            if (distance >= 2f)
            {
                Vector3 axis = toy.transform.InverseTransformDirection(
                    viewCamera.transform.right * separation.x + viewCamera.transform.up * separation.y).normalized;
                if (Vector3.Dot(axis, pinchAxis) < 0f) axis = -axis;
                pinchAxis = Vector3.Slerp(pinchAxis, axis, Mathf.Clamp01(Time.unscaledDeltaTime * 18f)).normalized;
            }
            float midpointDelta = ((first + second) * .5f - pairStartCenter).y;
            float worldDelta = midpointDelta / Mathf.Max(1f, viewCamera.pixelHeight) * viewCamera.orthographicSize * 2f;
            lift.Target = Mathf.Clamp(pairLiftStart + worldDelta, 0f, .75f);
        }

        private static bool IsLive(Touch touch)
        { return touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled; }

        private static bool FindTouch(int id, out Touch touch)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch candidate = Input.GetTouch(i);
                if (candidate.fingerId == id) { touch = candidate; return true; }
            }
            touch = default(Touch);
            return false;
        }

        private void ApplyLift()
        {
            float previous = visibleLift;
            float requested = reducedMotion ? lift.Target : lift.Value;
            // Keep the top of a lifted, vertically stretched Pip inside the viewer.
            float room = Mathf.Max(0f, 3.4f - (toyMount.position.y + toy.DeformedTop));
            visibleLift = Mathf.Clamp(requested, 0f, Mathf.Min(.75f, room));
            toy.transform.localPosition = Vector3.up * visibleLift;
            if (!reducedMotion && !gesture.IsActive && previous > .001f && visibleLift <= .001f && lift.Velocity < -.08f)
            {
                compression.Reset(Mathf.Clamp(-lift.Velocity * .06f, .025f, .12f));
                compression.Target = 0f;
            }
            if (Mathf.Abs(visibleLift - lastShadowLift) < .001f) return;
            lastShadowLift = visibleLift;
            float height = visibleLift / .75f;
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
                jiggleX.Reset(.18f); jiggleY.Reset(.12f); jiggleZ.Reset(-.1f);
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
