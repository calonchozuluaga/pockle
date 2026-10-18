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
        /// <summary>Plugin startup/read failed; false for disabled permission, missing sensor or editor preview.</summary>
        public bool WalkingFailed => tracker != null && tracker.Failed;
        /// <summary>Sensor started successfully and permission is held; historical steps do not imply active tracking.</summary>
        public bool WalkingActive => !readOnlySave && tracker != null && tracker.Active;
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
            if (!Application.isBatchMode) { TryApplyOwnerFoxTestGrant(); TryApplyOwnerFoxFinishesGrant(); TryApplyOwnerPipFinishesGrant(); TryApplyOwnerAxolotlFinishesGrant(); TryApplyOwnerPipFusionStudiesGrant(); TryApplyOwnerWinterTrioGrant(); }
        }

        public const string WinterTrioGrantKey="pockle.owner-test.winter-trio.v1";
        public bool TryApplyOwnerWinterTrioGrant()
        {
            if(!(Application.isEditor||Debug.isDebugBuild)||Progress==null||readOnlySave||PlayerPrefs.GetInt(WinterTrioGrantKey,0)==1)return false;
            var flag=Resources.Load<TextAsset>("WinterTrioOwnerTestGrant");
            if(flag==null||flag.text.Trim()!="enabled")return false;
            bool granted=Progress.EnsureWinterTrioTestCopies();
            Persist();PlayerPrefs.SetInt(WinterTrioGrantKey,1);PlayerPrefs.Save();
            Debug.Log("Pockle owner test: Reiny, Guin and Bundy finishes saved (30/30).");Changed?.Invoke();return granted;
        }
        public const string PipFusionStudiesGrantKey="pockle.owner-test.pip-fusion-studies.v2";
        public bool TryApplyOwnerPipFusionStudiesGrant()
        {
            if(Progress==null||readOnlySave||PlayerPrefs.GetInt(PipFusionStudiesGrantKey,0)==1)return false;
            var config=Resources.Load<TextAsset>("PipFusionOwnerTestGrant");
            if(config==null||config.text.Trim()!="enabled")return false;
            bool granted=Progress.EnsurePipFusionStudyCopies();
            Persist(); // Save inventory before the marker and any visible grant.
            PlayerPrefs.SetInt(PipFusionStudiesGrantKey,1);PlayerPrefs.Save();
            Debug.Log("Pockle owner test: Dusk, Sorbet, Honeydew and Eclipse saved for material review.");Changed?.Invoke();return granted;
        }
        public const string FoxTestGrantKey = "pockle.owner-test.fox-grant.v1";
        /// <summary>Temporary owner test configuration; remove FoxOwnerTestGrant.txt before public distribution.</summary>
        public bool TryApplyOwnerFoxTestGrant()
        {
            if (Progress == null || readOnlySave || PlayerPrefs.GetInt(FoxTestGrantKey, 0) == 1) return false;
            var configuration = Resources.Load<TextAsset>("FoxOwnerTestGrant");
            if (configuration == null || configuration.text.Trim() != "enabled") return false;
            bool granted = Progress.EnsureFoxTestCopy();
            Persist(); // Durable inventory first, then the one-time marker, then any UI refresh.
            PlayerPrefs.SetInt(FoxTestGrantKey, 1); PlayerPrefs.Save();
            Changed?.Invoke();
            return granted;
        }
        public const string FoxFinishesGrantKey = "pockle.owner-test.fox-finishes.v1";
        public bool TryApplyOwnerFoxFinishesGrant()
        {
            if (Progress == null || readOnlySave || PlayerPrefs.GetInt(FoxFinishesGrantKey, 0) == 1) return false;
            var configuration = Resources.Load<TextAsset>("FoxOwnerTestGrant");
            if (configuration == null || configuration.text.Trim() != "enabled") return false;
            bool granted = Progress.EnsureFoxFinishTestCopies();
            Persist(); // Inventory precedes grant marker and HUD refresh, including after interrupted startup.
            PlayerPrefs.SetInt(FoxFinishesGrantKey, 1); PlayerPrefs.Save();
            Changed?.Invoke();
            return granted;
        }
        public const string PipFinishesGrantKey = "pockle.owner-test.pip-finishes.v1";
        /// <summary>Temporary owner-only beta grant; disable PipOwnerTestGrant before public distribution.</summary>
        public bool TryApplyOwnerPipFinishesGrant()
        {
            if (Progress == null || readOnlySave || PlayerPrefs.GetInt(PipFinishesGrantKey, 0) == 1) return false;
            var configuration = Resources.Load<TextAsset>("PipOwnerTestGrant");
            if (configuration == null || configuration.text.Trim() != "enabled") return false;
            bool granted = Progress.EnsurePipFinishTestCopies();
            Persist(); // Inventory must be durable before the marker and any collection refresh.
            PlayerPrefs.SetInt(PipFinishesGrantKey, 1); PlayerPrefs.Save();
            int found = 0;
            foreach (var toy in ToyCatalog.Collectibles)
                if (toy.CollectionId == "pip" && Progress.GetOwnedCount(toy.Id) > 0) found++;
            Debug.Log("Pockle owner test: Pip finishes saved (" + found + "/10).");
            Changed?.Invoke();
            return granted;
        }
        public const string AxolotlFinishesGrantKey = "pockle.owner-test.axolotl-finishes.v1";
        /// <summary>Temporary owner-only beta grant; disable AxolotlOwnerTestGrant before public distribution.</summary>
        public bool TryApplyOwnerAxolotlFinishesGrant()
        {
            if (Progress == null || readOnlySave || PlayerPrefs.GetInt(AxolotlFinishesGrantKey, 0) == 1) return false;
            var configuration = Resources.Load<TextAsset>("AxolotlOwnerTestGrant");
            if (configuration == null || configuration.text.Trim() != "enabled") return false;
            bool granted = Progress.EnsureAxolotlFinishTestCopies();
            Persist(); // Inventory must be durable before the marker and any collection refresh.
            PlayerPrefs.SetInt(AxolotlFinishesGrantKey, 1); PlayerPrefs.Save();
            int found = 0;
            foreach (var toy in ToyCatalog.Collectibles)
                if (toy.CharacterId == "axolotl" && Progress.GetOwnedCount(toy.Id) > 0) found++;
            Debug.Log("Pockle owner test: Axolotl finishes saved (" + found + "/10).");
            Changed?.Invoke();
            return granted;
        }
        public bool IsOwnedAndPlayable(string id) => IsOwnedAndAvailable(id) && CharacterArt.TryLoad(id, out _);

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
