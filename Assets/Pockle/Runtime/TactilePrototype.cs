using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pockle.Runtime
{
    /// <summary>Local tactile sandbox. No rewards, backend calls, or collectible ownership are simulated.</summary>
    public sealed class TactilePrototype : MonoBehaviour
    {
        private const int NoPointer = -999;
        private const string PrefPrefix = "pockle.prototype.";
        private readonly Spring1D compression = new Spring1D(4.2f, 0.57f);
        private readonly Spring1D stretch = new Spring1D(3.6f, 0.59f);
        private readonly Spring1D tiltX = new Spring1D(3.3f, 0.63f);
        private readonly Spring1D tiltZ = new Spring1D(3.3f, 0.63f);
        private Camera viewCamera;
        private Transform toyMount;
        private JellyToy toy;
        private PrototypeHud hud;
        private Transform boxRoot;
        private Transform boxLid;
        private Material boxMaterial;
        private Material lidMaterial;
        private AudioSource audioSource;
        private AudioClip pressClip;
        private AudioClip releaseClip;
        private AudioClip revealClip;
        private int pointerId = NoPointer;
        private Vector2 pointerStart;
        private Vector2 pointerPrevious;
        private float contactX;
        private float contactZ;
        private bool rotateMode;
        private bool soundEnabled;
        private bool hapticsEnabled;
        private bool reducedMotion;
        private bool revealing;
        private float revealTime;
        private bool revealChimePlayed;

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            soundEnabled = PlayerPrefs.GetInt(PrefPrefix + "sound", 1) == 1;
            hapticsEnabled = PlayerPrefs.GetInt(PrefPrefix + "haptics", 0) == 1;
            reducedMotion = PlayerPrefs.GetInt(PrefPrefix + "reducedMotion", 0) == 1;

            PrototypeStage.Create(out viewCamera, out toyMount);
            GameObject toyObject = new GameObject("Pip - tactile companion");
            toyObject.transform.SetParent(toyMount, false);
            toy = toyObject.AddComponent<JellyToy>();
            toy.Initialize();

            hud = new GameObject("Pockle prototype HUD").AddComponent<PrototypeHud>();
            hud.Initialize(Reveal, ResetToy, SetSound, SetHaptics, SetReducedMotion);
            hud.SetSettings(soundEnabled, hapticsEnabled, reducedMotion);
            hud.RotationChanged += SetRotateMode;
            hud.SetMode(false);

            BuildRevealBox();
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.28f;
            pressClip = CreateTone("Soft jelly press", 160f, 0.09f, false);
            releaseClip = CreateTone("Jelly rebound", 390f, 0.18f, false);
            revealClip = CreateTone("Pip reveal", 620f, 0.45f, true);
            Reveal();
        }

        private void Update()
        {
            if (toy == null) return;
            if (revealing) UpdateReveal(Time.unscaledDeltaTime);
            else ReadInput();

            float dt = Time.unscaledDeltaTime;
            compression.Step(dt);
            stretch.Step(dt);
            tiltX.Step(dt);
            tiltZ.Step(dt);
            // Reduced motion keeps direct touch feedback and omits the spring rebound.
            float c = reducedMotion ? compression.Target : compression.Value;
            float s = reducedMotion ? stretch.Target : stretch.Value;
            float tx = reducedMotion ? tiltX.Target : tiltX.Value;
            float tz = reducedMotion ? tiltZ.Target : tiltZ.Value;
            // A released squash briefly stretches past rest; a released stretch softly
            // compresses. The shape's safe nonnegative inputs still permit this rebound.
            float squash = Mathf.Max(0f, c) + Mathf.Max(0f, -s) * 0.35f;
            float elongation = Mathf.Max(0f, s) + Mathf.Max(0f, -c) * 0.65f;
            toy.SetDeformation(squash, elongation, tx, tz, contactX, contactZ);
            hud.UpdateFeedback(Mathf.Clamp01(c / 0.42f), Mathf.Clamp01(s / 0.6f));
        }

        private void ReadInput()
        {
            if (Input.GetKeyDown(KeyCode.R)) ResetToy();
            if (Input.GetKeyDown(KeyCode.Space)) { Reveal(); return; }

            // Track one finger by ID so a second finger never takes over an active gesture.
            if (Input.touchCount > 0 || pointerId >= 0)
            {
                bool found = false;
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    if (pointerId == NoPointer && touch.phase == TouchPhase.Began)
                        BeginPointer(touch.fingerId, touch.position);
                    if (touch.fingerId != pointerId) continue;
                    found = true;
                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        ReleasePointer(touch.phase == TouchPhase.Ended);
                    else MovePointer(touch.position);
                    break;
                }
                if (pointerId >= 0 && !found) ReleasePointer(false);
                return;
            }

            if (Input.GetMouseButtonDown(0)) BeginPointer(-1, Input.mousePosition);
            if (pointerId == -1)
            {
                if (Input.GetMouseButtonUp(0)) ReleasePointer(true);
                else if (Input.GetMouseButton(0)) MovePointer(Input.mousePosition);
                else ReleasePointer(false);
            }
        }

        private void BeginPointer(int id, Vector2 position)
        {
            if (pointerId != NoPointer || revealing) return;
            if (!viewCamera.pixelRect.Contains(position)) return;
            if (EventSystem.current != null)
            {
                bool overUi = id >= 0 ? EventSystem.current.IsPointerOverGameObject(id)
                                      : EventSystem.current.IsPointerOverGameObject();
                if (overUi) return;
            }
            RaycastHit hit;
            Ray ray = viewCamera.ScreenPointToRay(position);
            if (!Physics.Raycast(ray, out hit, 30f) || hit.collider.GetComponentInParent<JellyToy>() != toy)
                return;

            pointerId = id;
            pointerStart = pointerPrevious = position;
            Vector3 localContact = toy.transform.InverseTransformPoint(hit.point);
            contactX = Mathf.Clamp(localContact.x, -0.9f, 0.9f);
            contactZ = Mathf.Clamp(localContact.z, -0.8f, 0.8f);
            if (!rotateMode)
            {
                compression.Target = 0.23f;
                Play(pressClip);
                hud.SetStatus("Hold to squish. Drag up to stretch.");
            }
            else hud.SetStatus("Drag sideways to turn Pip around.");
        }

        private void MovePointer(Vector2 position)
        {
            if (rotateMode)
            {
                float yaw = (position.x - pointerPrevious.x) / Mathf.Max(viewCamera.pixelWidth, 1) * 260f;
                toy.transform.Rotate(0f, -yaw, 0f, Space.Self);
            }
            else
            {
                float dragX = (position.x - pointerStart.x) / Mathf.Max(viewCamera.pixelHeight, 1) * 2f * viewCamera.orthographicSize;
                float dragY = (position.y - pointerStart.y) / Mathf.Max(viewCamera.pixelHeight, 1) * 2f * viewCamera.orthographicSize;
                stretch.Target = Mathf.Clamp(dragY * 0.58f, 0f, 0.6f);
                compression.Target = Mathf.Clamp(0.23f - dragY * 0.38f, 0.04f, 0.42f);
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
            bool wasPressed = pointerId != NoPointer && !rotateMode;
            pointerId = NoPointer;
            compression.Target = stretch.Target = tiltX.Target = tiltZ.Target = 0f;
            if (feedback && wasPressed)
            {
                Play(releaseClip);
                Vibrate();
            }
            if (hud != null && !revealing)
                hud.SetStatus(rotateMode ? "Rotate mode · drag Pip sideways." : "Press Pip, drag up, then let go.");
        }

        private void SetRotateMode(bool rotate)
        {
            ReleasePointer(false);
            rotateMode = rotate;
            hud.SetMode(rotate);
            hud.SetStatus(rotate ? "Rotate mode · drag Pip sideways." : "Press Pip, drag up, then let go.");
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
            rotateMode = false;
            hud.SetMode(false);
            compression.Reset(); stretch.Reset(); tiltX.Reset(); tiltZ.Reset();
            contactX = contactZ = 0f;
            toy.ResetToy();
            hud.SetRevealAvailable(true);
            hud.SetStatus("Fresh as a peach. Say hello to Pip.");
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
            boxLid.localPosition = new Vector3(0f, 0.6f, 0f);
            boxLid.localRotation = Quaternion.identity;
            SetBoxAlpha(1f);
            hud.SetRevealAvailable(false);
            hud.SetStatus("A little wonder is waking up…");
        }

        private void UpdateReveal(float dt)
        {
            revealTime += Mathf.Min(dt, 0.1f);
            float duration = reducedMotion ? 0.55f : 1.65f;
            float progress = Mathf.Clamp01(revealTime / duration);
            float opening = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.12f, 0.62f, progress));
            boxLid.localPosition = new Vector3(0f, 0.6f + (reducedMotion ? 0.1f : opening * 1.1f), 0f);
            boxLid.localRotation = reducedMotion ? Quaternion.identity : Quaternion.Euler(-opening * 18f, 0f, opening * -9f);
            if (!reducedMotion)
                boxRoot.localScale = Vector3.one * (1f + Mathf.Sin(progress * 22f) * 0.018f * (1f - opening));
            float emergence = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.38f, 0.9f, progress));
            if (progress >= 0.38f)
            {
                toy.gameObject.SetActive(true);
                float scale = reducedMotion ? 1f : Mathf.Lerp(0.16f, 1f, emergence);
                toyMount.localScale = Vector3.one * scale;
                if (!reducedMotion) compression.Target = (1f - emergence) * 0.18f;
                if (!revealChimePlayed) { Play(revealClip); Vibrate(); revealChimePlayed = true; }
            }
            SetBoxAlpha(1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.43f, 0.78f, progress)));
            if (progress >= 1f)
            {
                revealing = false;
                boxRoot.gameObject.SetActive(false);
                toy.gameObject.SetActive(true);
                toyMount.localScale = Vector3.one;
                compression.Target = 0f;
                hud.SetRevealAvailable(true);
                hud.SetStatus("Meet Pip. Press, stretch, and let go.");
            }
        }

        private void BuildRevealBox()
        {
            boxRoot = new GameObject("Simple discovery reveal placeholder").transform;
            boxRoot.position = new Vector3(0f, 0.65f, 0f);
            boxMaterial = CreateBoxMaterial(new Color(0.68f, 0.40f, 0.40f, 1f));
            lidMaterial = CreateBoxMaterial(new Color(0.9f, 0.68f, 0.54f, 1f));
            AddBoxPart("Box", boxRoot, Vector3.zero, new Vector3(1.9f, 1f, 1.65f), boxMaterial);
            boxLid = AddBoxPart("Lid", boxRoot, new Vector3(0f, 0.6f, 0f), new Vector3(2.02f, 0.18f, 1.77f), lidMaterial);
            AddBoxPart("Ribbon", boxRoot, new Vector3(0f, 0f, -0.832f), new Vector3(0.13f, 0.94f, 0.008f), lidMaterial);
            boxRoot.gameObject.SetActive(false);
        }

        private static Transform AddBoxPart(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = part.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
            return part.transform;
        }

        private static Material CreateBoxMaterial(Color color)
        {
            Shader shader = Shader.Find("Pockle/Reveal Box");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Pockle/Soft Accent");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private void SetBoxAlpha(float alpha)
        {
            Color body = boxMaterial.color; body.a = alpha; boxMaterial.color = body;
            Color lid = lidMaterial.color; lid.a = alpha; lidMaterial.color = lid;
        }

        private void SetSound(bool enabled)
        {
            soundEnabled = enabled;
            SaveSetting("sound", enabled);
            if (!enabled && audioSource != null) audioSource.Stop();
        }

        private void SetHaptics(bool enabled) { hapticsEnabled = enabled; SaveSetting("haptics", enabled); }
        private void SetReducedMotion(bool enabled) { reducedMotion = enabled; SaveSetting("reducedMotion", enabled); }
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

        private void OnApplicationPause(bool paused) { if (paused) ReleasePointer(false); }
        private void OnApplicationFocus(bool focused) { if (!focused) ReleasePointer(false); }
        private void OnDisable() { ReleasePointer(false); }
        private void OnDestroy()
        {
            if (hud != null) hud.RotationChanged -= SetRotateMode;
            if (pressClip != null) Destroy(pressClip);
            if (releaseClip != null) Destroy(releaseClip);
            if (revealClip != null) Destroy(revealClip);
            if (boxMaterial != null) Destroy(boxMaterial);
            if (lidMaterial != null) Destroy(lidMaterial);
        }
    }
}
