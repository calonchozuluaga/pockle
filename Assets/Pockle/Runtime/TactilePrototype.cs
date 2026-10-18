using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Pockle.Runtime
{
    /// <summary>Collection entry point and tactile viewer. Rewards are local beta saves.</summary>
    public sealed partial class TactilePrototype : MonoBehaviour
    {
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
        private ToyViewGesture toyView;
        private PipReactionDriver pipReactions;
        private FoxReactionDriver foxReactions;
        private AxolotlReactionDriver axolotlReactions;
        private FirbReactionDriver firbReactions;
        private CocoReactionDriver cocoReactions;
        private GingerReactionDriver gingerReactions;
        private SnowyReactionDriver snowyReactions;
        private WinterTrioReactionDriver winterReactions;
        public event System.Action<ReactionCue,Vector2?> ToyReactionStarted;
        private float lastToyActivity;
        private bool leftAloneSaid,pairSquishSaid,pairLiftSaid;
        private float manipulationLiftStart;
        private Transform contactShadow;
        private Material shadowMaterial;
        private Color shadowRestColor;
        private Vector3 shadowRestScale;
        private float visibleLift;
        private float lastShadowLift = -1f;
        private Vector3 pinchAxis = Vector3.up;
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

            PrototypeStage.Create(out viewCamera, out toyMount, out turntable, out _, out contactShadow);
            shadowMaterial = contactShadow.GetComponent<Renderer>().sharedMaterial;
            shadowRestColor = shadowMaterial.color;
            shadowRestScale = contactShadow.localScale;
            sensorOrientation = Screen.orientation;
            GameObject toyObject = new GameObject("Pip - tactile companion");
            toyObject.transform.SetParent(toyMount, false);
            toy = toyObject.AddComponent<JellyToy>();
            toy.Initialize();
            ConfigureHandling();
            pipReactions=toyObject.AddComponent<PipReactionDriver>();
            pipReactions.Initialize(toy);
            foxReactions=toyObject.AddComponent<FoxReactionDriver>();foxReactions.Initialize(toy);
            axolotlReactions=toyObject.AddComponent<AxolotlReactionDriver>();axolotlReactions.Initialize(toy);
            firbReactions=toyObject.AddComponent<FirbReactionDriver>();firbReactions.Initialize(toy);
            cocoReactions=toyObject.AddComponent<CocoReactionDriver>();cocoReactions.Initialize(toy);
            gingerReactions=toyObject.AddComponent<GingerReactionDriver>();gingerReactions.Initialize(toy);
            snowyReactions=toyObject.AddComponent<SnowyReactionDriver>();snowyReactions.Initialize(toy);
            winterReactions=toyObject.AddComponent<WinterTrioReactionDriver>();winterReactions.Initialize(toy);
            toyView=gameObject.AddComponent<ToyViewGesture>();
            toyView.Initialize(viewCamera,turntable,toy,pressed =>
            {
                compression.Target=pressed&&toy.Handling!=MaterialHandling.Firm?.23f:0;
                if(pressed){Play(pressClip);SayToyCue(ReactionCue.Squish);}else Play(releaseClip);
            },ResetToy);
            toyView.Tapped=PlayTapReaction;
            toyView.TurnStarted=()=>SayToyCue(ReactionCue.Turn);
            toyView.GestureStarted=()=>{RecordToyActivity();pipReactions.Cancel();foxReactions.Cancel();axolotlReactions.Cancel();firbReactions.Cancel();cocoReactions.Cancel();gingerReactions.Cancel();snowyReactions.Cancel();winterReactions.Cancel();};
            toyView.SoftManipulationStarted=position =>
            {
                pairSquishSaid=pairLiftSaid=false;
                manipulationLiftStart=visibleLift;lift.Reset(visibleLift);dropFeedback=false;
                if(toy.RaycastBody(viewCamera.ScreenPointToRay(position),out var point))
                { gripTarget=toy.transform.InverseTransformPoint(point);gripPoint=gripTarget;contactX=gripPoint.x;contactZ=gripPoint.z; }
            };
            toyView.SoftManipulated=(pose,axis) =>
            {
                pinch.Target=pose.Pinch;pinchAxis=axis;
                compression.Target=pose.Compression;
                lift.Target=Mathf.Clamp(manipulationLiftStart+pose.LiftDelta,0,WeightedLift.Maximum);
                RecordToyActivity();
                if(!pairLiftSaid&&pose.LiftDelta>.06f){pairLiftSaid=true;SayToyCue(ReactionCue.PickUp);}
                else if(!pairSquishSaid&&(Mathf.Abs(pose.Pinch)>.025f||pose.Compression>.20f)){pairSquishSaid=true;SayToyCue(ReactionCue.Squish);}
            };
            toyView.SoftManipulationEnded=feedback =>
            {
                compression.Target=pinch.Target=lift.Target=0;
                dropFeedback=feedback&&!reducedMotion&&visibleLift>.03f;
                if(!feedback){lift.Reset();sag.Reset();}
            };

            collection = gameObject.AddComponent<CollectionSession>();
            collection.Initialize();
            hud = new GameObject("Pockle game UI").AddComponent<PrototypeHud>();
            hud.Initialize(SetSound, SetHaptics, SetReducedMotion, SetVolume);
            hud.SetSoundVolume(soundVolume);
            hud.SetSettings(soundEnabled, hapticsEnabled, reducedMotion);
            hud.SetVariant(toy.Variant);
            ApplyStage();
            hud.VariantChanged += SetVariant;
            hud.CollectibleChanged += SelectCollectible;
            hud.StoreVisibilityChanged += StoreVisibilityChanged;
            hud.BackdropChanged += SetBackdrop;
            hud.WorldChanged += SetWorld;
            hud.LightChanged += SetLight;
            hud.StageViewportChanged += SetStageViewport;
            SetStageViewport(hud.StageViewport);
            ApplyStage();
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
                Debug.Log("Pockle controls: orbit · soft pinch/squeeze/lift · rigid zoom/pan · random Pip tap · double-tap reset", this);
        }

        private void Update()
        {
            if (toy == null) return;
            if (Input.GetKeyDown(KeyCode.Escape) && !hud.GoBack() && Application.platform == RuntimePlatform.Android) Application.Quit();
            if (revealing) UpdateReveal(Time.unscaledDeltaTime);
            else if (!hud.StoreVisible) ReadInput();

            toyView.Calm=reducedMotion;
            toyView.Suspended=revealing||hud.StoreVisible;
            float dt = Time.unscaledDeltaTime;
            ReadMotion(dt);
            compression.Step(dt);
            stretch.Step(dt);
            tiltX.Step(dt);
            tiltZ.Step(dt);
            bool held = toyView.IsHolding && !revealing && !hud.StoreVisible;
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
            toy.SetExpressionContext(squash, held && !revealing ? lift.Value : 0f);
            toy.SetDeformation(squash, elongation, tx, tz, contactX, contactZ, p, pinchAxis, hanging, gripPoint, clearance);
            ApplyLift();
            if (hanging > 0f && visibleLift < clearance - .0001f)
                toy.SetDeformation(squash, elongation, tx, tz, contactX, contactZ, p, pinchAxis, hanging, gripPoint, visibleLift);
            if (!revealing) lift.ConstrainHeight(visibleLift);
        }

        private void ReadInput()
        {
            if(!leftAloneSaid&&!revealing&&hud!=null&&hud.CurrentPage==AppPage.Play&&!toyView.IsHolding&&Time.unscaledTime-lastToyActivity>18)
            { SayToyCue(ReactionCue.LeftAlone);leftAloneSaid=true; }
#if UNITY_EDITOR
            if(Input.GetKeyDown(KeyCode.R))ResetToy();
            if(Input.GetKeyDown(KeyCode.Space)){Reveal();return;}
#endif
            toyView.ReadInput();
        }
        private void RecordToyActivity(){lastToyActivity=Time.unscaledTime;leftAloneSaid=false;}
        private void SayToyCue(ReactionCue cue)
        {
            if(hud==null||hud.CurrentPage!=AppPage.Play||revealing||toy==null)return;
            Vector2? head=null;
            if(toy.BodyRenderer!=null&&viewCamera!=null)
            {
                var b=toy.BodyRenderer.bounds;
                var screen=viewCamera.WorldToScreenPoint(b.center+viewCamera.transform.up*b.extents.y*.55f);
                if(screen.z>0)head=new Vector2(screen.x,screen.y);
            }
            hud.ShowToyExpression(cue,head);ToyReactionStarted?.Invoke(cue,head);RecordToyActivity();
        }
        private bool AnyReactionPlaying => pipReactions.IsPlaying||foxReactions.IsPlaying||axolotlReactions.IsPlaying||firbReactions.IsPlaying||cocoReactions.IsPlaying||gingerReactions.IsPlaying||snowyReactions.IsPlaying||winterReactions.IsPlaying;
        private void PlayTapReaction()
        {
            if(AnyReactionPlaying)return;
            if(ToyCatalog.IsFoxCollectible(toy.CollectibleId))foxReactions.PlayRandom();else if(ToyCatalog.TryGetCollectible(toy.CollectibleId,out var tapped)&&tapped.CharacterId=="axolotl")axolotlReactions.PlayRandom();else if(ToyCatalog.TryGetCollectible(toy.CollectibleId,out var firb)&&firb.CharacterId=="firb")firbReactions.PlayRandom();else if(ToyCatalog.TryGetCollectible(toy.CollectibleId,out var coco)&&coco.CharacterId=="coco")cocoReactions.PlayRandom();else if(ToyCatalog.TryGetCollectible(toy.CollectibleId,out var ginger)&&ginger.CharacterId=="ginger")gingerReactions.PlayRandom();else if(ToyCatalog.TryGetCollectible(toy.CollectibleId,out var snowy)&&snowy.CharacterId=="snowy")snowyReactions.PlayRandom();else if(ToyCatalog.TryGetCollectible(toy.CollectibleId,out var winter)&&HolidayCatalog.IsWinterTrio(winter.CharacterId))winterReactions.PlayRandom();else pipReactions.PlayRandom();
            if(AnyReactionPlaying)
                SayToyCue(ReactionCue.Poke);
        }
        private string PlayHint
        {
            get
            {
                return toy.Handling==MaterialHandling.Firm
                    ? "Tap · drag to turn · pinch to zoom"
                    : "Tap · drag to turn · pinch to squish";
            }
        }
        private void SetStageViewport(Rect pixels)
        {
            if(toyView!=null)toyView.Cancel();
            PrototypeStage.SetViewport(viewCamera,pixels);
        }

        private void ReleasePointer(bool feedback)
        {
            if(toyView!=null)toyView.Cancel();
            compression.Target = stretch.Target = tiltX.Target = tiltZ.Target = 0f;
            lift.Target = pinch.Target = 0f;
            dropFeedback = false;
            if (!feedback) { lift.Reset(); sag.Reset(); }
            if (hud != null && !revealing)
                hud.SetStatus(PlayHint);
        }

        public void ResetToy()
        {
            if (toy == null) return;
            ReleasePointer(false);
            pipReactions?.Cancel();foxReactions?.Cancel();axolotlReactions?.Cancel();firbReactions?.Cancel();cocoReactions?.Cancel();gingerReactions?.Cancel();snowyReactions?.Cancel();winterReactions?.Cancel();
            revealing = false;
            boxRoot.gameObject.SetActive(false);
            toyMount.localScale = Vector3.one;
            toy.gameObject.SetActive(true);
            toy.transform.localRotation = Quaternion.identity;
            turntable.localRotation = Quaternion.identity;
            toyView?.ResetView();
            compression.Reset(); stretch.Reset(); tiltX.Reset(); tiltZ.Reset();
            lift.Reset(); pinch.Reset(); ClearMotion();
            sag.Reset(); gripPoint = gripTarget = Vector3.zero;
            revealLift = 0f;
            visibleLift = 0f;
            toy.transform.localPosition = Vector3.zero;
            contactX = contactZ = 0f;
            toy.ResetToy();
            hud.SetRevealAvailable(true);
            hud.SetStatus(PlayHint);
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
                hud.SetStatus(PlayHint);
                if (collection.Progress.Save.PendingReveal >= 0) collection.FinishReveal();
            }
        }

        private void BuildRevealBox()
        {
            boxRoot = new GameObject("Jelly Garden mystery box").transform;
            boxRoot.SetParent(turntable, false);
            boxRoot.localPosition = new Vector3(0f, -1f, 0f);
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
            ApplyStage();
        }

        private void SetBackdrop(Color colour)
        {
            PrototypeStage.SetBackdrop(viewCamera, colour);
            ApplyStage();
        }
        private void SetWorld(StageWorld world) => ApplyStage();
        private void SetLight(StageLight light) { ApplyStage();if(light==StageLight.Night)SayToyCue(ReactionCue.Night); }
        private void ApplyStage()
        {
            if (hud == null || toy == null || viewCamera == null) return;
            // A collection world picked by ID (five per collection) goes through SetEnvironment; Studio and legacy worlds keep SetWorld.
            if (string.IsNullOrEmpty(hud.CurrentEnvironmentId) || !PrototypeStage.SetEnvironment(viewCamera, hud.CurrentEnvironmentId, toy.CollectibleId))
                PrototypeStage.SetWorld(viewCamera, hud.CurrentWorld, toy.CollectibleId);
            PrototypeStage.SetLight(viewCamera, hud.CurrentLight);
            viewCamera.GetComponent<ToyStageEnvironment>().Calm = reducedMotion;
            shadowRestColor = new Color(.54f, .36f, .43f, hud.CurrentLight == StageLight.Night ? .12f : hud.CurrentLight == StageLight.Golden ? .27f : .20f);
        }

        private void ConfigureHandling()
        {
            RecordToyActivity();
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
            ApplyStage();
            return true;
        }

#if UNITY_EDITOR
        /// <summary>Authoring review only: preview studies without modifying player inventory.</summary>
        public bool PreviewCharacterStudy(string id)
        {
            if (toy == null || revealing || !CharacterArt.IsStudy(id)) return false;
            int ownedPip = -1;
            for (int i = 0; i < PipVariants.Count; i++)
                if (collection.IsOwnedAndAvailable(ToyCatalog.LegacyCollectibleId(i))) { ownedPip = i; break; }
            if (ownedPip < 0) return false;
            // Existing HUD remains in Claude's lane; enter its viewer using the preserved overload.
            bool hadPreference = PlayerPrefs.HasKey(PipVariants.Preference);
            int savedVariant = PlayerPrefs.GetInt(PipVariants.Preference);
            try
            {
                if (hud.CurrentPage != AppPage.Play) hud.ShowToy((PipVariant)ownedPip);
            }
            finally
            {
                if (hadPreference) PlayerPrefs.SetInt(PipVariants.Preference, savedVariant);
                else PlayerPrefs.DeleteKey(PipVariants.Preference);
                PlayerPrefs.Save();
            }
            if (!toy.TrySetCollectible(id)) return false;
            ResetToy();
            ConfigureHandling();
            if (ToyCatalog.TryGetCollectible(id, out var definition))
                hud.SetStatus("Draft " + definition.DisplayName + " · " + PlayHint);
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
            ApplyStage();
            if (enabled) { ClearMotion(); toyView?.Orbit.Stop(); }
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
        private void SelectCollectible(string id) { TryPlayCollectible(id); }

        private void OnDestroy()
        {
            if (hud != null) hud.VariantChanged -= SetVariant;
            if (hud != null) hud.CollectibleChanged -= SelectCollectible;
            if (hud != null) { hud.StoreVisibilityChanged -= StoreVisibilityChanged; hud.StageViewportChanged -= SetStageViewport; }
            if (hud != null) hud.BackdropChanged -= SetBackdrop;
            if (hud != null) hud.WorldChanged -= SetWorld;
            if (hud != null) hud.LightChanged -= SetLight;
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
