using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pockle.Runtime
{
    /// <summary>Collection entry point and tactile viewer. Rewards are local beta saves.</summary>
    public sealed partial class TactilePrototype : MonoBehaviour
    {
        private const int NoPointer = PointerGesture.NoPointer;
        private readonly PointerGesture gesture = new PointerGesture();
        private const string PrefPrefix = "pockle.prototype.";
        private Spring1D compression = new Spring1D(3.8f, .45f);
        private Spring1D stretch = new Spring1D(3.6f, 0.59f);
        private Spring1D tiltX = new Spring1D(3.3f, 0.63f);
        private Spring1D tiltZ = new Spring1D(3.3f, 0.63f);
        private readonly WeightedLift lift = new WeightedLift();
        private Spring1D sag = new Spring1D(2.9f, .62f);
        private Vector3 gripPoint, gripTarget;
        private Spring1D pinch = new Spring1D(4f, .64f);
        private Spring1D jiggleX = new Spring1D(ToyFeel.ShakeFrequency, ToyFeel.ShakeDamping);
        private Spring1D jiggleY = new Spring1D(ToyFeel.ShakeFrequency, ToyFeel.ShakeDamping);
        private Spring1D jiggleZ = new Spring1D(ToyFeel.ShakeFrequency, ToyFeel.ShakeDamping);
        private readonly MotionJiggle motion = new MotionJiggle();
        private Camera viewCamera;
        private Transform toyMount;
        private Transform turntable;
        private Collider plateCollider;
        private Transform contactShadow;
        private Material shadowMaterial;
        private Color shadowRestColor;
        private Vector3 shadowRestScale;
        private float visibleLift;
        private float lastShadowLift = -1f;
        private float liftStart;
        private float pairLiftStart;
        private float pairStartDistance;
        private float pairStartPinch;
        private Vector2 pairStartCenter;
        private Vector3 pinchAxis = Vector3.up;
        private Vector2 interactionScreenSize;
        private ScreenOrientation sensorOrientation;
        private JellyToy toy;
        private PrototypeHud hud;
        private CollectionSession collection;
        private Transform boxRoot;
        private MysteryBox discoveryBox;
        private float revealLift;
        private AudioSource audioSource;
        private AudioClip pressClip;
        private AudioClip releaseClip;
        private AudioClip landingClip;
        private AudioClip revealClip;
        private Vector2 pointerStart;
        private Vector2 pointerPrevious;
        private float contactX;
        private float contactZ;
        private bool soundEnabled;
        private float soundVolume;
        private bool hapticsEnabled;
        private bool reducedMotion;
        private bool revealing;
        private float revealTime;
        private bool revealChimePlayed;
        private bool dropFeedback;

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            soundEnabled = PlayerPrefs.GetInt(PrefPrefix + "sound", 1) == 1;
            float savedVolume = PlayerPrefs.GetFloat(PrefPrefix + "soundVolume", 1);
            soundVolume = float.IsNaN(savedVolume) || float.IsInfinity(savedVolume) ? 1 : Mathf.Clamp01(savedVolume);
            hapticsEnabled = PlayerPrefs.GetInt(PrefPrefix + "haptics", 0) == 1;
            reducedMotion = PlayerPrefs.GetInt(PrefPrefix + "reducedMotion", 0) == 1;

            PrototypeStage.Create(out viewCamera, out toyMount, out turntable, out plateCollider, out contactShadow);
            shadowMaterial = contactShadow.GetComponent<Renderer>().sharedMaterial;
            shadowRestColor = shadowMaterial.color;
            shadowRestScale = contactShadow.localScale;
            interactionScreenSize = new Vector2(Screen.width, Screen.height);
            sensorOrientation = Screen.orientation;
            GameObject toyObject = new GameObject("Pip - tactile companion");
            toyObject.transform.SetParent(toyMount, false);
            toy = toyObject.AddComponent<JellyToy>();
            toy.Initialize();
            ConfigureHandling();

            collection = gameObject.AddComponent<CollectionSession>();
            collection.Initialize();
            hud = new GameObject("Pockle game UI").AddComponent<PrototypeHud>();
            hud.Initialize(SetSound, SetHaptics, SetReducedMotion, SetVolume);
            hud.SetSoundVolume(soundVolume);
            hud.SetSettings(soundEnabled, hapticsEnabled, reducedMotion);
            hud.SetVariant(toy.Variant);
            hud.VariantChanged += SetVariant;
            hud.StoreVisibilityChanged += StoreVisibilityChanged;
            hud.DailyBoxRequested += OpenDailyBox;
            hud.Bind(collection);

            BuildRevealBox();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.28f * soundVolume;
            pressClip = CreateTone("Soft jelly press", 160f, 0.09f, false);
            releaseClip = CreateTone("Jelly rebound", 390f, 0.18f, false);
            landingClip = CreateTone("Pip soft landing", 95f, .24f, false);
            revealClip = CreateTone("Pip reveal", 620f, 0.45f, true);
            ResetToy();
            StoreVisibilityChanged(true);
            if (collection.Progress.Save.PendingReveal >= 0)
            {
                hud.ShowToy((PipVariant)collection.Progress.Save.PendingReveal);
                Reveal();
            }
            if (Application.isEditor || Debug.isDebugBuild)
                Debug.Log("Pockle controls: lift · two-finger squish/stretch · phone-motion jiggle", this);
        }

        private void Update()
        {
            if (toy == null) return;
            var screenSize = new Vector2(Screen.width, Screen.height);
            if (screenSize != interactionScreenSize && gesture.IsActive) ReleasePointer(false);
            interactionScreenSize = screenSize;
            if (Input.GetKeyDown(KeyCode.Escape) && !hud.GoBack() && Application.platform == RuntimePlatform.Android) Application.Quit();
            if (revealing) UpdateReveal(Time.unscaledDeltaTime);
            else if (!hud.StoreVisible) ReadInput();

            float dt = Time.unscaledDeltaTime;
            ReadMotion(dt);
            compression.Step(dt);
            stretch.Step(dt);
            tiltX.Step(dt);
            tiltZ.Step(dt);
            bool held = gesture.IsActive && gesture.Target == PointerTarget.Toy && !revealing && !hud.StoreVisible;
            lift.Step(dt, held, reducedMotion || revealing || hud.StoreVisible);
            sag.Target = !reducedMotion && held ? ToyFeel.HangingSag * Mathf.Clamp01(lift.Value / .25f) : 0f;
            sag.Step(dt);
            gripPoint = Vector3.Lerp(gripPoint, gripTarget, 1f - Mathf.Exp(-18f * dt));
            if (lift.LandingSpeed > .08f && !revealing && !hud.StoreVisible && !reducedMotion)
            {
                compression.Reset(ToyFeel.LandingCompression(lift.LandingSpeed));
                compression.Target = 0f;
                jiggleY.Reset(ToyFeel.LandingJiggle(lift.LandingSpeed));
                tiltX.Reset(ToyFeel.LandingLean(lift.LandingSpeed, gripPoint.x)); tiltX.Target = 0;
                if (dropFeedback)
                {
                    if (soundEnabled) audioSource.PlayOneShot(landingClip, ToyFeel.LandingVolume(lift.LandingSpeed));
                    Vibrate();
                }
                dropFeedback = false;
            }
            pinch.Step(dt);
            jiggleX.Step(dt); jiggleY.Step(dt); jiggleZ.Step(dt);
            // Reduced motion keeps direct touch feedback and omits the spring rebound.
            float c = reducedMotion ? compression.Target : compression.Value;
            float s = reducedMotion ? stretch.Target : stretch.Value;
            float tx = reducedMotion ? tiltX.Target : tiltX.Value;
            float tz = reducedMotion ? tiltZ.Target : tiltZ.Value;
            Vector3 wobble = toy.transform.InverseTransformDirection(
                viewCamera.transform.right * jiggleX.Value + viewCamera.transform.forward * jiggleZ.Value);
            tx += wobble.x;
            tz += wobble.z;
            // A released squash briefly stretches past rest; a released stretch softly
            // compresses. The shape's safe nonnegative inputs still permit this rebound.
            float squash = Mathf.Max(0f, c) + Mathf.Max(0f, -s) * 0.35f + Mathf.Max(0f, -jiggleY.Value) * .6f;
            float elongation = Mathf.Max(0f, s) + Mathf.Max(0f, -c) * 0.65f + Mathf.Max(0f, jiggleY.Value) * .6f;
            float hanging = reducedMotion || revealing ? 0f : Mathf.Max(0f, sag.Value);
            float clearance = revealing ? 0f : lift.Value;
            float p = reducedMotion ? pinch.Target : pinch.Value;
            toy.SetDeformation(squash, elongation, tx, tz, contactX, contactZ, p, pinchAxis, hanging, gripPoint, clearance);
            ApplyLift();
            if (hanging > 0f && visibleLift < clearance - .0001f)
                toy.SetDeformation(squash, elongation, tx, tz, contactX, contactZ, p, pinchAxis, hanging, gripPoint, visibleLift);
            if (!revealing) lift.ConstrainHeight(visibleLift);
        }

        private void ReadInput()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.R)) ResetToy();
            if (Input.GetKeyDown(KeyCode.Space)) { Reveal(); return; }
#endif

            if (Input.touchCount > 0 || gesture.PointerId >= 0)
            {
                ReadTouches();
                return;
            }

            if (Input.GetMouseButtonDown(0)) BeginPointer(-1, Input.mousePosition);
            if (gesture.PointerId == -1)
            {
                if (Input.GetMouseButtonUp(0)) ReleasePointer(true);
                else if (Input.GetMouseButton(0)) MovePointer(Input.mousePosition);
                else ReleasePointer(false);
            }
        }

        private void BeginPointer(int id, Vector2 position)
        {
            if (gesture.PointerId != NoPointer || revealing || hud.StoreVisible) return;
            if (!viewCamera.pixelRect.Contains(position)) return;
            if (EventSystem.current != null)
            {
                bool overUi = id >= 0 ? EventSystem.current.IsPointerOverGameObject(id)
                                      : EventSystem.current.IsPointerOverGameObject();
                if (overUi) return;
            }
            Ray ray = viewCamera.ScreenPointToRay(position);
            Vector3 toyPoint;
            RaycastHit plateHit;
            bool hitToy = toy.RaycastBody(ray, out toyPoint);
            bool hitPlate = plateCollider.Raycast(ray, out plateHit, 30f);
            if (!hitToy && !hitPlate) return;
            var target = hitToy && (!hitPlate || Vector3.Distance(ray.origin, toyPoint) <= plateHit.distance)
                ? PointerTarget.Toy : PointerTarget.Plate;
            if (!gesture.TryBegin(id, target)) return;
            pointerStart = pointerPrevious = position;
            liftStart = visibleLift;
            if (target == PointerTarget.Toy)
            {
                dropFeedback = false;
                lift.Target = liftStart;
                Vector3 localContact = toy.transform.InverseTransformPoint(toyPoint);
                gripTarget = localContact;
                if (visibleLift <= .001f) gripPoint = gripTarget;
                contactX = Mathf.Clamp(localContact.x, -0.9f, 0.9f);
                contactZ = Mathf.Clamp(localContact.z, -0.8f, 0.8f);
                compression.Target = 0.23f;
                Play(pressClip);
                hud.SetStatus("Drag up to lift. Add a finger to stretch.");
            }
            else hud.SetStatus("Drag the plate sideways to turn Pip.");
        }

        private void MovePointer(Vector2 position)
        {
            if (gesture.Target == PointerTarget.Plate)
            {
                float yaw = (position.x - pointerPrevious.x) / Mathf.Max(viewCamera.pixelWidth, 1) * 260f;
                turntable.Rotate(0f, -yaw, 0f, Space.Self);
            }
            else
            {
                float dragX = (position.x - pointerStart.x) / Mathf.Max(viewCamera.pixelHeight, 1) * 2f * viewCamera.orthographicSize;
                float dragY = (position.y - pointerStart.y) / Mathf.Max(viewCamera.pixelHeight, 1) * 2f * viewCamera.orthographicSize;
                float liftDrag = Mathf.Sign(dragY) * Mathf.Max(0f, Mathf.Abs(dragY) - .025f);
                lift.Target = Mathf.Clamp(liftStart + liftDrag, 0f, .75f);
                stretch.Target = 0f;
                compression.Target = Mathf.Clamp(.23f - Mathf.Max(0f, dragY) * .4f + Mathf.Max(0f, -dragY) * .25f, .04f, .42f);
                // Screen-relative leaning stays intuitive after rotating the toy.
                Vector3 worldLean = viewCamera.transform.right * (dragX * 0.0672f)
                    + viewCamera.transform.forward * (-dragY * 0.0085f);
                Vector3 localLean = toy.transform.InverseTransformDirection(worldLean);
                tiltX.Target = Mathf.Clamp(localLean.x / 0.42f, -0.25f, 0.25f);
                tiltZ.Target = Mathf.Clamp(localLean.z / 0.34f, -0.25f, 0.25f);
            }
            pointerPrevious = position;
        }

        private void ReleasePointer(bool feedback)
        {
            bool wasPressed = gesture.Target == PointerTarget.Toy;
            gesture.Cancel();
            compression.Target = stretch.Target = tiltX.Target = tiltZ.Target = 0f;
            lift.Target = pinch.Target = 0f;
            dropFeedback = feedback && wasPressed && !reducedMotion && visibleLift > .03f;
            if (!feedback) { lift.Reset(); sag.Reset(); }
            if (feedback && wasPressed && (reducedMotion || visibleLift <= .03f))
            {
                Play(releaseClip);
                Vibrate();
            }
            if (hud != null && !revealing)
                hud.SetStatus("Lift Pip. Pinch to stretch. Turn the plate.");
        }

        public void ResetToy()
        {
            if (toy == null) return;
            ReleasePointer(false);
            revealing = false;
            boxRoot.gameObject.SetActive(false);
            toyMount.localScale = Vector3.one;
            toy.gameObject.SetActive(true);
            toy.transform.localRotation = Quaternion.identity;
            turntable.localRotation = Quaternion.identity;
            compression.Reset(); stretch.Reset(); tiltX.Reset(); tiltZ.Reset();
            lift.Reset(); pinch.Reset(); ClearMotion();
            sag.Reset(); gripPoint = gripTarget = Vector3.zero;
            revealLift = 0f;
            visibleLift = 0f;
            toy.transform.localPosition = Vector3.zero;
            contactX = contactZ = 0f;
            toy.ResetToy();
            hud.SetRevealAvailable(true);
            hud.SetStatus("Say hello to Pip. Squish or turn.");
        }

        public void Reveal()
        {
            if (toy == null || revealing) return;
            ResetToy();
            revealing = true;
            revealTime = 0f;
            revealChimePlayed = false;
            toy.gameObject.SetActive(false);
            boxRoot.gameObject.SetActive(true);
            boxRoot.localScale = Vector3.one;
            discoveryBox.ApplyPose(RevealSequence.Sample(0f, reducedMotion), reducedMotion);
            hud.SetRevealAvailable(false);
            hud.SetStatus("Jelly Garden… who's inside?");
        }

        private void UpdateReveal(float dt)
        {
            revealTime += Mathf.Min(dt, 0.1f);
            RevealPose pose = RevealSequence.Sample(revealTime, reducedMotion);
            discoveryBox.ApplyPose(pose, reducedMotion);
            boxRoot.gameObject.SetActive(pose.BoxAlpha > .001f);
            revealLift = pose.ToyLift;
            compression.Target = pose.Compression;
            if (pose.ToyVisible)
            {
                toy.gameObject.SetActive(true);
                toyMount.localScale = Vector3.one * pose.ToyScale;
                if (!revealChimePlayed) { Play(revealClip); Vibrate(); revealChimePlayed = true; }
            }
            if (pose.Finished)
            {
                revealing = false;
                revealLift = 0f;
                boxRoot.gameObject.SetActive(false);
                toy.gameObject.SetActive(true);
                toyMount.localScale = Vector3.one;
                compression.Target = 0f;
                hud.SetRevealAvailable(true);
                hud.SetStatus("Lift Pip. Pinch to stretch. Turn the plate.");
                if (collection.Progress.Save.PendingReveal >= 0) collection.FinishReveal();
            }
        }

        private void BuildRevealBox()
        {
            boxRoot = new GameObject("Jelly Garden mystery box").transform;
            boxRoot.SetParent(turntable, false);
            boxRoot.localPosition = new Vector3(0f, .15f, 0f);
            discoveryBox = boxRoot.gameObject.AddComponent<MysteryBox>();
            discoveryBox.Initialize();
            boxRoot.gameObject.SetActive(false);
        }

        private void SetVolume(float value)
        {
            soundVolume = Mathf.Clamp01(value);
            if (audioSource != null) audioSource.volume = .28f * soundVolume;
        }

        private void SetSound(bool enabled)
        {
            soundEnabled = enabled;
            SaveSetting("sound", enabled);
            if (!enabled && audioSource != null) audioSource.Stop();
        }

        private void SetVariant(PipVariant choice)
        {
            ResetToy();
            toy.SetVariant(choice);
            ConfigureHandling();
            PlayerPrefs.SetInt(PipVariants.Preference, (int)toy.Variant);
            PlayerPrefs.Save();
            hud.SetVariant(toy.Variant);
        }

        private void ConfigureHandling()
        {
            MaterialHandling h = toy.Handling;
            bool gel = h == MaterialHandling.Gel;
            compression = new Spring1D(h.RecoveryFrequency, h.RecoveryDamping);
            stretch = new Spring1D(gel ? 3.6f : h.RecoveryFrequency, gel ? .59f : h.RecoveryDamping);
            tiltX = new Spring1D(gel ? 3.3f : h.RecoveryFrequency, gel ? .63f : h.RecoveryDamping);
            tiltZ = new Spring1D(gel ? 3.3f : h.RecoveryFrequency, gel ? .63f : h.RecoveryDamping);
            pinch = new Spring1D(gel ? 4 : h.RecoveryFrequency, gel ? .64f : h.RecoveryDamping);
            sag = new Spring1D(gel ? 2.9f : h.RecoveryFrequency, gel ? .62f : h.RecoveryDamping);
            jiggleX = new Spring1D(h.ShakeFrequency, h.ShakeDamping);
            jiggleY = new Spring1D(h.ShakeFrequency, h.ShakeDamping);
            jiggleZ = new Spring1D(h.ShakeFrequency, h.ShakeDamping);
            motion.Reset();
        }

        /// <summary>Call after the HUD navigates to its viewer. Does not grant ownership or complete a reveal.</summary>
        public bool TryPlayCollectible(string id)
        {
            if (toy == null || collection == null || revealing ||
                !collection.IsOwnedAndAvailable(id) || !toy.TrySetCollectible(id)) return false;
            ResetToy();
            ConfigureHandling();
            return true;
        }

#if UNITY_EDITOR
        /// <summary>Authoring review only: preview studies without modifying player inventory.</summary>
        public bool PreviewCharacterStudy(string id)
        {
            if (toy == null || revealing || (id != CharacterArt.MossStudyId && id != CharacterArt.BopStudyId)) return false;
            int ownedPip = -1;
            for (int i = 0; i < PipVariants.Count; i++)
                if (collection.IsOwnedAndAvailable(ToyCatalog.LegacyCollectibleId(i))) { ownedPip = i; break; }
            if (ownedPip < 0) return false;
            // Existing HUD remains in Claude's lane; enter its viewer using the preserved overload.
            bool hadPreference = PlayerPrefs.HasKey(PipVariants.Preference);
            int savedVariant = PlayerPrefs.GetInt(PipVariants.Preference);
            try { hud.ShowToy((PipVariant)ownedPip); }
            finally
            {
                if (hadPreference) PlayerPrefs.SetInt(PipVariants.Preference, savedVariant);
                else PlayerPrefs.DeleteKey(PipVariants.Preference);
                PlayerPrefs.Save();
            }
            if (!toy.TrySetCollectible(id)) return false;
            ResetToy();
            ConfigureHandling();
            hud.SetStatus("Draft " + (id == CharacterArt.MossStudyId ? "Moss" : "Bop") + " · drag to lift, J to test jiggle");
            return true;
        }
#endif

        private void StoreVisibilityChanged(bool visible)
        {
            if (visible) { ReleasePointer(false); ClearMotion(); }
            // Browsing uses cached portraits; no toy cameras render behind the collection UI.
            viewCamera.enabled = !visible;
            turntable.gameObject.SetActive(!visible);
        }

        private void OpenDailyBox()
        {
            if (!collection.ClaimDaily(out PipVariant choice)) return;
            hud.ShowToy(choice);
            Reveal();
        }

        private void SetHaptics(bool enabled) { hapticsEnabled = enabled; SaveSetting("haptics", enabled); }
        private void SetReducedMotion(bool enabled)
        {
            reducedMotion = enabled;
            if (enabled) ClearMotion();
            SaveSetting("reducedMotion", enabled);
        }
        private static void SaveSetting(string key, bool enabled)
        {
            PlayerPrefs.SetInt(PrefPrefix + key, enabled ? 1 : 0);
            PlayerPrefs.Save();
        }
        private void Play(AudioClip clip) { if (soundEnabled && audioSource != null) audioSource.PlayOneShot(clip); }
        private void Vibrate()
        {
            if (hapticsEnabled && (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer))
                Handheld.Vibrate();
        }

        private static AudioClip CreateTone(string name, float frequency, float duration, bool chime)
        {
            const int sampleRate = 22050;
            float[] samples = new float[Mathf.CeilToInt(sampleRate * duration)];
            for (int i = 0; i < samples.Length; i++)
            {
                float time = (float)i / sampleRate;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(time / duration)) * Mathf.Exp(-time * (chime ? 6f : 12f));
                float phase = 2f * Mathf.PI * frequency * (time + 0.07f * time * time);
                samples[i] = envelope * (Mathf.Sin(phase) * 0.45f + Mathf.Sin(phase * (chime ? 1.5f : 2f)) * 0.16f);
            }
            AudioClip clip = AudioClip.Create(name, samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private void OnApplicationPause(bool paused) { if (paused) { ReleasePointer(false); ClearMotion(); } }
        private void OnApplicationFocus(bool focused) { if (!focused) { ReleasePointer(false); ClearMotion(); } }
        private void OnDisable() { ReleasePointer(false); ClearMotion(); }
        private void OnDestroy()
        {
            if (hud != null) hud.VariantChanged -= SetVariant;
            if (hud != null) hud.StoreVisibilityChanged -= StoreVisibilityChanged;
            if (hud != null) hud.DailyBoxRequested -= OpenDailyBox;
            if (hud != null) Destroy(hud.gameObject);
            if (pressClip != null) Destroy(pressClip);
            if (releaseClip != null) Destroy(releaseClip);
            if (landingClip != null) Destroy(landingClip);
            if (revealClip != null) Destroy(revealClip);
            if (boxRoot != null) Destroy(boxRoot.gameObject);
            if (viewCamera != null) Destroy(viewCamera.transform.parent.gameObject);
        }
    }
}
