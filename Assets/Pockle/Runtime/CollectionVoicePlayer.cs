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
        public bool IsBusy => source != null && Time.unscaledTime < nextVoice;
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
            return Load(path);
        }
        private AudioClip Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!clips.TryGetValue(path, out var clip)) { clip = Resources.Load<AudioClip>(path); clips.Add(path, clip); }
            return clip;
        }
        public AudioClip ClipForAction(string id, System.Enum action)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var toy)) return null;
            return Load(CollectionVoicePolicy.ActionResource(toy.CharacterId, action));
        }
        public AudioClip ClipForTemperament(string id, PipTemperament temperament)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var toy)) return null;
            return Load(CollectionVoicePolicy.TemperamentResource(toy.CharacterId, temperament));
        }
        public bool TryPlayAction(string id, System.Enum action, CollectionVoiceMood fallback)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var toy)) return false;
            string path = CollectionVoicePolicy.ActionResource(toy.CharacterId, action);
            if (path.Length == 0) return false;
            return Play(ClipForAction(id, action) ?? ClipFor(id, fallback));
        }
        public bool TryPlayTemperament(string id, PipTemperament temperament, CollectionVoiceMood fallback)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var toy)) return false;
            string path = CollectionVoicePolicy.TemperamentResource(toy.CharacterId, temperament);
            if (path.Length == 0) return false;
            return Play(ClipForTemperament(id, temperament) ?? ClipFor(id, fallback));
        }
        public bool TryPlay(string id, CollectionVoiceMood mood) => Play(ClipFor(id, mood));
        private bool Play(AudioClip clip)
        {
            if (!soundEnabled || volume <= 0 || !isActiveAndEnabled || source == null || IsBusy || clip == null) return false;
            source.clip = clip; source.Play(); nextVoice = Time.unscaledTime + Mathf.Max(.3f, clip.length); return true;
        }
        public void Stop() { if (source != null) { source.Stop(); source.clip = null; } nextVoice = float.NegativeInfinity; }
        private void OnApplicationPause(bool paused) { if (paused) Stop(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Stop(); }
        private void OnDisable() { Stop(); }
        private void OnDestroy() { if (source != null) Destroy(source); }
    }
}
