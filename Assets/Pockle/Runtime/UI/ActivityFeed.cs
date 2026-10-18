using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Who an activity rail shows: collectors near you, people you follow, or everyone (canvas "Home", October 15).</summary>
    public enum ActivityScope { Nearby, Friends, Everyone }

    public enum ActivityKind { NewToy, Fusion, Complete, Badge, Milestone }

    /// <summary>One post in the activity rail. Presentation only; never grants or changes inventory.</summary>
    public sealed class ActivityItem
    {
        public string Id = "", Who = "", AvatarId = "", CollectibleId = "", SecondId = "", CollectionId = "", Caption = "", Detail = "";
        public ActivityKind Kind;
        public DateTime WhenUtc;
        public int Cheers;
        public bool Cheered, Mine, Sample;
    }

    /// <summary>
    /// Where the Home activity rail gets its posts. Today there is no account server, so only two sources exist:
    /// your own finds (real, kept on this device) and, in development builds, clearly labelled sample posts so the design
    /// can be judged on a phone. A server-backed source can replace this later without touching the UI.
    /// </summary>
    public interface IActivitySource
    {
        /// <summary>False when the scope can't show anything yet (no accounts, no location permission).</summary>
        bool Available(ActivityScope scope);
        IReadOnlyList<ActivityItem> Items(ActivityScope scope);
        void Cheer(string id);
    }

    /// <summary>
    /// The local source: records your own new toys and completed collections (from inventory changes), and adds
    /// sample posts only when <see cref="ShowSamples"/> is on. Nothing leaves the device.
    /// </summary>
    public sealed class LocalActivityFeed : IActivitySource
    {
        public const string LogKey = "pockle.activity.mine.v1";
        public const string SamplesKey = "pockle.dev.sampleActivity";
        private const int MaxMine = 20;
        private readonly List<ActivityItem> mine = new List<ActivityItem>();
        private readonly List<ActivityItem> samples = new List<ActivityItem>();
        private readonly HashSet<string> cheered = new HashSet<string>();
        private Dictionary<string, int> lastCounts;

        /// <summary>Sample posts show in the editor and development builds, or when the dev switch is set.</summary>
        public static bool ShowSamples => Application.isEditor || Debug.isDebugBuild || PlayerPrefs.GetInt(SamplesKey, 0) == 1;

        public LocalActivityFeed()
        {
            Load();
            BuildSamples();
        }

        public bool Available(ActivityScope scope) => ShowSamples || scope == ActivityScope.Everyone && mine.Count > 0;

        public IReadOnlyList<ActivityItem> Items(ActivityScope scope)
        {
            var list = new List<ActivityItem>();
            if (scope == ActivityScope.Everyone) list.AddRange(mine);
            if (ShowSamples)
                foreach (var item in samples)
                    if (scope == ActivityScope.Everyone || SampleScope(item) == scope) list.Add(item);
            list.Sort((a, b) => b.WhenUtc.CompareTo(a.WhenUtc));
            foreach (var item in list) item.Cheered = cheered.Contains(item.Id);
            return list;
        }

        public void Cheer(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            foreach (var item in AllItems())
            {
                if (item.Id != id || item.Mine) continue; // you don't cheer your own posts
                if (cheered.Add(id)) item.Cheers++; else { cheered.Remove(id); item.Cheers = Mathf.Max(0, item.Cheers - 1); }
            }
        }

        /// <summary>
        /// Call when the inventory changes. The first call only remembers the current counts; later calls turn a newly
        /// owned toy into "You found …" and a newly completed collection into "You completed …".
        /// </summary>
        public bool Observe(IReadOnlyDictionary<string, int> counts)
        {
            if (counts == null) return false;
            var now = new Dictionary<string, int>();
            foreach (var pair in counts) now[pair.Key] = pair.Value;
            if (lastCounts == null) { lastCounts = now; return false; }
            bool changed = false;
            foreach (var pair in now)
            {
                if (pair.Value <= 0 || lastCounts.TryGetValue(pair.Key, out int before) && before > 0) continue;
                if (!ToyCatalog.TryGetCollectible(pair.Key, out var toy)) continue;
                Add(new ActivityItem { Kind = ActivityKind.NewToy, CollectibleId = toy.Id, CollectionId = toy.CollectionId,
                    Caption = "Found <b>" + PersonalityBook.ShortName(toy.Id) + "</b>", Detail = toy.IsSecret ? "SECRET" : "" });
                changed = true;
                if (ToyCatalog.TryGetCollection(toy.CollectionId, out var collection) && Complete(collection, now) && !Complete(collection, lastCounts))
                    Add(new ActivityItem { Kind = ActivityKind.Complete, CollectionId = collection.Id,
                        Caption = "Completed the <b>" + collection.DisplayName + "</b> collection" });
            }
            lastCounts = now;
            if (changed) Save();
            return changed;
        }

        private static bool Complete(CollectionDefinition collection, Dictionary<string, int> counts)
        {
            foreach (string id in collection.Members) if (!counts.TryGetValue(id, out int n) || n <= 0) return false;
            return collection.Members.Count > 0;
        }

        private void Add(ActivityItem item)
        {
            item.Mine = true; item.Who = "You"; item.WhenUtc = DateTime.UtcNow;
            item.Id = "mine:" + item.Kind + ":" + item.CollectibleId + item.CollectionId + ":" + item.WhenUtc.Ticks;
            item.AvatarId = item.CollectibleId.Length > 0 ? item.CollectibleId : PersonalityBook.Peach;
            mine.Insert(0, item);
            if (mine.Count > MaxMine) mine.RemoveRange(MaxMine, mine.Count - MaxMine);
        }

        [Serializable] private sealed class Saved { public List<SavedItem> Items = new List<SavedItem>(); }
        [Serializable] private sealed class SavedItem { public int Kind; public string CollectibleId, CollectionId, Caption, Detail; public long Ticks; }

        private void Save()
        {
            var saved = new Saved();
            foreach (var item in mine)
                saved.Items.Add(new SavedItem { Kind = (int)item.Kind, CollectibleId = item.CollectibleId, CollectionId = item.CollectionId,
                    Caption = item.Caption, Detail = item.Detail, Ticks = item.WhenUtc.Ticks });
            PlayerPrefs.SetString(LogKey, JsonUtility.ToJson(saved));
            PlayerPrefs.Save();
        }

        private void Load()
        {
            mine.Clear();
            string json = PlayerPrefs.GetString(LogKey, "");
            if (json.Length == 0) return;
            Saved saved = null;
            try { saved = JsonUtility.FromJson<Saved>(json); } catch (Exception) { }
            if (saved?.Items == null) return;
            foreach (var s in saved.Items)
            {
                var item = new ActivityItem { Kind = (ActivityKind)Mathf.Clamp(s.Kind, 0, 4), CollectibleId = s.CollectibleId ?? "", CollectionId = s.CollectionId ?? "",
                    Caption = s.Caption ?? "", Detail = s.Detail ?? "", WhenUtc = new DateTime(Math.Max(0, s.Ticks), DateTimeKind.Utc), Mine = true, Who = "You" };
                item.Id = "mine:" + item.Kind + ":" + item.CollectibleId + item.CollectionId + ":" + s.Ticks;
                item.AvatarId = item.CollectibleId.Length > 0 ? item.CollectibleId : PersonalityBook.Peach;
                mine.Add(item);
            }
        }

        private IEnumerable<ActivityItem> AllItems() { foreach (var i in mine) yield return i; foreach (var i in samples) yield return i; }

        private static ActivityScope SampleScope(ActivityItem item) => item.Id.EndsWith("@near") ? ActivityScope.Nearby : ActivityScope.Friends;

        /// <summary>Design samples (development builds only, tagged SAMPLE on the card). Invented names, real catalog toys.</summary>
        private void BuildSamples()
        {
            var now = DateTime.UtcNow;
            samples.Add(new ActivityItem { Id = "sample:maya@friend", Who = "Maya", AvatarId = ToyCatalog.FoxAuroraId, Kind = ActivityKind.NewToy, CollectibleId = ToyCatalog.FoxAuroraId,
                CollectionId = "fox", Caption = "Found <b>Aurora</b>, the secret Fox", Detail = "SECRET", WhenUtc = now.AddMinutes(-12), Cheers = 24, Sample = true });
            samples.Add(new ActivityItem { Id = "sample:theo@friend", Who = "Theo", AvatarId = ToyCatalog.MoonId, Kind = ActivityKind.Fusion, CollectibleId = ToyCatalog.MoonId, SecondId = ToyCatalog.MintId,
                CollectionId = "pip", Caption = "Fused <b>Moon</b> + <b>Mint</b> into a one-of-a-kind", WhenUtc = now.AddHours(-1), Cheers = 9, Sample = true });
            samples.Add(new ActivityItem { Id = "sample:june@near", Who = "June", AvatarId = ToyCatalog.PeachId, Kind = ActivityKind.Complete, CollectionId = "pip",
                Caption = "Completed the <b>Pip</b> collection", WhenUtc = now.AddHours(-3), Cheers = 31, Sample = true });
            samples.Add(new ActivityItem { Id = "sample:ari@friend", Who = "Ari", AvatarId = ToyCatalog.GoldId, Kind = ActivityKind.Badge, CollectionId = "pip",
                Caption = "Earned <b>Night Owl</b>: 7 night-shelf moments", WhenUtc = now.AddDays(-1), Cheers = 15, Sample = true });
            samples.Add(new ActivityItem { Id = "sample:leo@near", Who = "Leo", AvatarId = ToyCatalog.MintId, Kind = ActivityKind.Milestone, CollectionId = "pip",
                Caption = "Walked 7 days in a row", WhenUtc = now.AddDays(-1).AddHours(-2), Cheers = 6, Sample = true });
        }

        /// <summary>"12 min ago", "3 h ago", "Yesterday", "Oct 3".</summary>
        public static string Ago(DateTime whenUtc)
        {
            var span = DateTime.UtcNow - whenUtc;
            if (span.TotalMinutes < 1) return "Just now";
            if (span.TotalMinutes < 60) return (int)span.TotalMinutes + " min ago";
            if (span.TotalHours < 24) return (int)span.TotalHours + " h ago";
            if (span.TotalHours < 48) return "Yesterday";
            return whenUtc.ToLocalTime().ToString("MMM d");
        }
    }
}
