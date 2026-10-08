using System;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Single persisted local collection; no payment can grant through this service.</summary>
    public sealed class CollectionSession : MonoBehaviour
    {
        private const string SaveKey = "pockle.collection.v1";
        public CollectionProgress Progress { get; private set; }
        public event Action Changed;
        public string WalkingStatus => tracker == null ? "Step tracking unavailable." : tracker.Status;
        private AndroidWalkingTracker tracker;
        private float nextPoll;
        private float nextSave;
        private bool dirty;
        public static int Today => CollectionProgress.UtcDay(DateTime.UtcNow);

        public void Initialize()
        {
            if (Progress != null) return;
            CollectionSave saved = null;
            if (PlayerPrefs.HasKey(SaveKey))
            {
                // Preserve a damaged/unknown save instead of silently overwriting it.
                string json = PlayerPrefs.GetString(SaveKey);
                try { saved = JsonUtility.FromJson<CollectionSave>(json); }
                catch (Exception) { PlayerPrefs.SetString(SaveKey + ".backup", json); }
                if (saved != null && saved.Version != 1)
                { PlayerPrefs.SetString(SaveKey + ".backup", json); saved = null; }
            }
            Progress = new CollectionProgress(saved, Today);
            tracker = new AndroidWalkingTracker();
            tracker.ResumeIfAllowed();
            Persist();
        }

        public void EnableWalking() { tracker.Enable(); Changed?.Invoke(); }

        public bool ClaimDaily(out PipVariant variant)
        {
            variant = UnityEngine.Random.value < .5f ? PipVariant.PeachJelly : PipVariant.MintSoft;
            if (!Progress.TryClaimDaily(Today, (int)variant)) return false;
            Persist();
            Changed?.Invoke();
            return true;
        }

        public void FinishReveal() { Progress.FinishReveal(); Persist(); Changed?.Invoke(); }

        private void Update()
        {
            if (Progress == null || Time.unscaledTime < nextPoll) return;
            nextPoll = Time.unscaledTime + 1f;
            bool changed = Progress.AdvanceDay(Today);
            tracker.ResumeIfAllowed();
            if (tracker.TryRead(out long total, out double uptime, out int boot))
                changed |= Progress.ObserveSteps(Today, total, uptime, boot);
            if (changed) dirty = true;
            Changed?.Invoke(); // Permission and availability can change independently of steps.
            if (dirty && Time.unscaledTime >= nextSave) { Persist(); nextSave = Time.unscaledTime + 10f; }
        }

        private void Persist()
        { PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Progress.Save)); PlayerPrefs.Save(); dirty = false; }

        private void OnApplicationPause(bool paused) { if (paused && Progress != null) Persist(); }
        private void OnApplicationFocus(bool focused) { if (focused && tracker != null) tracker.ResumeIfAllowed(); }
        private void OnDestroy() { if (Progress != null) Persist(); tracker?.Dispose(); }

#if UNITY_EDITOR
        public void SimulateDailyWalk()
        {
            var save = Progress.Save;
            Progress.ObserveSteps(Today, 0, 0, int.MaxValue);
            Progress.ObserveSteps(Today, CollectionProgress.DailyTarget, 600, int.MaxValue);
            // Reset the simulated baseline before the next real sensor sample.
            save.SensorTotal = -1;
            Persist(); Changed?.Invoke();
        }
#endif
    }
}
