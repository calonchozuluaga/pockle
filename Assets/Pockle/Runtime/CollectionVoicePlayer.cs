using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Bounded local one-shots. Never overlaps voices or generates per-touch audio.</summary>
    [DisallowMultipleComponent]
    public sealed class CollectionVoicePlayer : MonoBehaviour
    {
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private AudioSource source;
        private bool soundEnabled = true;
        private float volume = 1, nextVoice;
        public AudioClip CurrentClip => source == null ? null : source.clip;
        private void Awake()
        {
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.spatialBlend = 0; source.loop = false;
            source.volume = .65f; source.Stop(); source.clip = null; nextVoice = float.NegativeInfinity;
        }
        public void SetSettings(bool enabled, float value)
        {
            soundEnabled = enabled;
            volume = float.IsNaN(value) || float.IsInfinity(value) ? 0 : Mathf.Clamp01(value);
            if (source != null) source.volume = .65f * volume;
            if (!soundEnabled || volume <= 0) Stop();
        }
        public AudioClip ClipFor(string collectibleId, CollectionVoiceMood mood)
        {
            if (!ToyCatalog.TryGetCollectible(collectibleId, out var toy)) return null;
            string path = CollectionVoicePolicy.Resource(toy.CharacterId, mood);
            if (path.Length == 0) return null;
            if (!clips.TryGetValue(path, out var clip)) { clip = Resources.Load<AudioClip>(path); clips.Add(path, clip); }
            return clip;
        }
        public bool TryPlay(string id, CollectionVoiceMood mood)
        {
            if (!soundEnabled || volume <= 0 || !isActiveAndEnabled || source == null || Time.unscaledTime < nextVoice) return false;
            var clip = ClipFor(id, mood);
            if (clip == null) return false;
            source.clip = clip; source.Play(); nextVoice = Time.unscaledTime + Mathf.Max(.3f, clip.length); return true;
        }
        public void Stop() { if (source != null) { source.Stop(); source.clip = null; } nextVoice = float.NegativeInfinity; }
        private void OnApplicationPause(bool paused) { if (paused) Stop(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Stop(); }
        private void OnDisable() { Stop(); }
        private void OnDestroy() { if (source != null) Destroy(source); }
    }
}
