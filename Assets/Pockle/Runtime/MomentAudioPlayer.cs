using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Approved event sounds, independent of creature voices. One full tail at a time; no persistent audio state.</summary>
    [DisallowMultipleComponent]
    public sealed class MomentAudioPlayer : MonoBehaviour
    {
        private readonly Dictionary<MomentCue, AudioClip> clips = new Dictionary<MomentCue, AudioClip>();
        private readonly HashSet<string> fusionIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly MomentCueQueue queue = new MomentCueQueue();
        private readonly WalkingMomentGate walking = new WalkingMomentGate();
        private AudioSource source;
        private bool soundEnabled = true, paused, focused = true;
        private float volume = 1, playingUntil = float.NegativeInfinity;
        private MomentCue playing;
        public Func<bool> VoiceBusy, PassiveAllowed;
        public bool IsBusy => queue.HasPending || Time.unscaledTime < playingUntil;
        public AudioClip CurrentClip => source == null ? null : source.clip;
        public MomentCue CurrentCue => playing;
        private bool Foreground => !paused && focused;

        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0; source.loop = false;
            source.volume = .65f; source.Stop(); source.clip = null;
        }
        public AudioClip ClipFor(MomentCue cue)
        {
            string path = MomentAudioPolicy.Resource(cue);
            if (path.Length == 0) return null;
            if (!clips.TryGetValue(cue, out var clip)) { clip = Resources.Load<AudioClip>(path); clips.Add(cue, clip); }
            return clip;
        }
        public void SetSettings(bool enabled, float value)
        {
            soundEnabled = enabled;
            volume = float.IsNaN(value) || float.IsInfinity(value) ? 0 : Mathf.Clamp01(value);
            if (source != null) source.volume = volume * MomentAudioPolicy.Volume(playing);
            if (!soundEnabled || volume <= 0) Stop();
        }
        public bool Request(MomentCue cue)
        {
            if (!soundEnabled || volume <= 0 || !Foreground || !isActiveAndEnabled || ClipFor(cue) == null) return false;
            return queue.Offer(cue, Time.unscaledTime);
        }
        public void FusionArrived(string savedFusionId)
        {
            if (string.IsNullOrEmpty(savedFusionId) || !fusionIds.Add(savedFusionId)) return;
            Request(MomentCue.FusionComplete);
        }
        public void ObserveWalking(WalkingMomentSnapshot snapshot)
        {
            bool allow = Foreground && soundEnabled && volume > 0 && (PassiveAllowed == null || PassiveAllowed());
            Request(walking.Observe(snapshot, allow));
        }
        private void LateUpdate()
        {
            if (!Foreground || !soundEnabled || volume <= 0 || source == null) { queue.Clear(); return; }
            bool busy = Time.unscaledTime < playingUntil || (VoiceBusy != null && VoiceBusy());
            if (!queue.Take(Time.unscaledTime, busy, PassiveAllowed == null || PassiveAllowed(), out var cue)) return;
            var clip = ClipFor(cue); if (clip == null) return;
            playing = cue; source.volume = volume * MomentAudioPolicy.Volume(cue);
            source.clip = clip; source.Play(); playingUntil = Time.unscaledTime + clip.length;
        }
        public void Stop()
        {
            queue.Clear(); playing = MomentCue.None; playingUntil = float.NegativeInfinity;
            if (source != null) { source.Stop(); source.clip = null; }
        }
        private void OnApplicationPause(bool value) { paused = value; if (value) { Stop(); walking.Rebaseline(); } }
        private void OnApplicationFocus(bool value) { focused = value; if (!value) { Stop(); walking.Rebaseline(); } }
        private void OnDisable() { Stop(); walking.Rebaseline(); }
        private void OnDestroy() { if (source != null) Destroy(source); }
    }
}
