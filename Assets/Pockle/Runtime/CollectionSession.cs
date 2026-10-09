using System;
using System.Collections.Generic;
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
        public string WalkingStatus => readOnlySave ? "This build cannot update your saved collection. Use a compatible Pockle version before earning more boxes."
            : Progress != null && PendingRevealId.Length > 0 && !IsOwnedAndAvailable(PendingRevealId)
                ? "An unfinished reveal isn't available in this build. Your saved collection is preserved."
                : tracker == null ? "Step tracking unavailable." : tracker.Status;
        public IReadOnlyDictionary<string, int> OwnedCounts => Progress.OwnedCounts;
        public int GetOwnedCount(string id) => Progress.GetOwnedCount(id);
        public string PendingRevealId => Progress.Save.PendingRevealId;
        public bool IsReadOnlySave => readOnlySave;
        private bool readOnlySave;
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
                catch (Exception) { readOnlySave = true; }
                if (saved == null)
                {
                    PlayerPrefs.SetString(SaveKey + ".backup", json); readOnlySave = true;
                    saved = new CollectionSave { Day = Today }; // Existing unreadable data is not a new install.
                }
                if (!readOnlySave && saved.Version == 1 && !PlayerPrefs.HasKey(SaveKey + ".before-v2"))
                    PlayerPrefs.SetString(SaveKey + ".before-v2", json);
                if (saved != null && saved.Version != 1 && saved.Version != CollectionProgress.SaveVersion)
                {
                    // Do not overwrite a newer schema. Show its legacy snapshot without allowing rewards.
                    PlayerPrefs.SetString(SaveKey + ".backup", json); readOnlySave = true;
                    saved = LegacySnapshot(saved);
                }
                else if (saved.Version == CollectionProgress.SaveVersion && saved.Inventory == null)
                {
                    PlayerPrefs.SetString(SaveKey + ".backup", json); readOnlySave = true;
                    saved = LegacySnapshot(saved);
                }
            }
            Progress = new CollectionProgress(saved, Today);
            tracker = new AndroidWalkingTracker();
            if (!readOnlySave) tracker.ResumeIfAllowed();
            Persist();
        }

        private static CollectionSave LegacySnapshot(CollectionSave source) => new CollectionSave {
            Counts = source.Counts, Day = source.Day, Steps = source.Steps, Claimed = source.Claimed,
            SensorTotal = source.SensorTotal, SensorUptime = source.SensorUptime, SensorBoot = source.SensorBoot
        };

        public void EnableWalking() { if (!readOnlySave) tracker.Enable(); Changed?.Invoke(); }

        public bool ClaimDaily(out PipVariant variant)
        {
            variant = PipVariant.PeachJelly;
            if (readOnlySave || !ToyCatalog.TryGetBoxPool(ToyCatalog.DailyPoolId, out var pool) ||
                !pool.TryChoose(UnityEngine.Random.value, out string id) || !PipVariants.TryFromCollectibleId(id, out variant) ||
                !Progress.TryClaimDaily(Today, id)) return false;
            Persist();
            Changed?.Invoke();
            return true;
        }

        public bool ClaimDailyCollectible(out string id)
        {
            bool claimed = ClaimDaily(out PipVariant variant); id = claimed ? PipVariants.CollectibleId(variant) : "";
            return claimed;
        }
        public bool IsOwnedAndAvailable(string id) => GetOwnedCount(id) > 0 &&
            ToyCatalog.TryGetCollectible(id, out var definition) && definition.Available;
        public bool TryGetPlayableVariant(string id, out PipVariant variant)
        {
            variant = PipVariant.PeachJelly;
            return IsOwnedAndAvailable(id) && PipVariants.TryFromCollectibleId(id, out variant);
        }
        public void FinishReveal() { FinishReveal(PendingRevealId); }
        public bool FinishReveal(string expectedId)
        {
            if (readOnlySave || !IsOwnedAndAvailable(expectedId) || !Progress.FinishReveal(expectedId)) return false;
            Persist(); Changed?.Invoke(); return true;
        }

        private void Update()
        {
            if (Progress == null || readOnlySave || Time.unscaledTime < nextPoll) return;
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
        { if (!readOnlySave) PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Progress.Save)); PlayerPrefs.Save(); dirty = false; }

        private void OnApplicationPause(bool paused) { if (paused && Progress != null) Persist(); }
        private void OnApplicationFocus(bool focused) { if (focused && tracker != null && !readOnlySave) tracker.ResumeIfAllowed(); }
        private void OnDestroy() { if (Progress != null) Persist(); tracker?.Dispose(); }

#if UNITY_EDITOR
        public void SimulateDailyWalk()
        {
            if (readOnlySave) return;
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
