using System;
using System.Collections.Generic;
using System.Globalization;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>What the badges are judged on. Everything here is already on the device; nothing is fetched.</summary>
    public sealed class BadgeFacts
    {
        public IReadOnlyDictionary<string, int> Owned = new Dictionary<string, int>();
        /// <summary>Shelf moments found so far (ids such as "a+b@night").</summary>
        public ICollection<string> Moments = new List<string>();
        /// <summary>Fused toys made so far, or -1 while fusion is a preview that saves nothing.</summary>
        public int Fusions = -1;
    }

    /// <summary>How far along a badge is. Untracked badges wait for a feature (steps history, trading) and are never earned.</summary>
    public readonly struct BadgeProgress
    {
        public readonly int Have, Need;
        public readonly bool Tracked;
        public readonly string Text;
        public BadgeProgress(int have, int need, string text) { Have = Math.Max(0, have); Need = Math.Max(1, need); Tracked = true; Text = text; }
        private BadgeProgress(string text) { Have = 0; Need = 1; Tracked = false; Text = text; }
        public static BadgeProgress Later(string text) => new BadgeProgress(text);
        public bool Complete => Tracked && Have >= Need;
        public float Fraction => Tracked ? Mathf.Clamp01((float)Have / Need) : 0;
    }

    public sealed class BadgeDefinition
    {
        public string Id { get; }
        public string Name { get; }
        /// <summary>Resources path of the Meshy art (Resources/UI/Badges).</summary>
        public string Art { get; }
        /// <summary>How to earn it, shown while it's locked.</summary>
        public string HowTo { get; }
        /// <summary>Shown once it's earned.</summary>
        public string Earned { get; }
        internal readonly Func<BadgeFacts, BadgeProgress> Measure;
        internal BadgeDefinition(string id, string name, string art, string howTo, string earned, Func<BadgeFacts, BadgeProgress> measure)
        { Id = id; Name = name; Art = "UI/Badges/" + art; HowTo = howTo; Earned = earned; Measure = measure; }
        public BadgeProgress Progress(BadgeFacts facts) => Measure(facts ?? new BadgeFacts());
    }

    /// <summary>
    /// The fifteen badges and their rules (docs/BADGES.md; canvas board "Badges with Meshy art"). A badge is earned the first
    /// time its rule is met and then stays earned, even if the toys behind it are later fused or traded away.
    /// </summary>
    public static class BadgeBook
    {
        public const int ShelfGoal = 50, NightGoal = 7, FusionGoal = 5, CollectorGoal = 5;
        /// <summary>Distinct toys needed for collector levels 1..5.</summary>
        public static readonly int[] CollectorLevels = { 0, 5, 15, 25, 40 };

        public static readonly IReadOnlyList<BadgeDefinition> All = new[]
        {
            new BadgeDefinition("secret-finder", "Secret finder", "BadgeSecretFinder", "Find a secret toy. Each collection hides one.",
                "You found a secret. They're the rarest toy in each collection.", f => new BadgeProgress(SecretsOwned(f), 1, SecretsOwned(f) + " of 1 secret")),
            new BadgeDefinition("pip-series-1", "Pip Series 1", "BadgePipSeries1", "Collect every Pip finish.",
                "Every Pip finish on your shelf.", f => Collection(f, "pip", "Pip")),
            new BadgeDefinition("fox-collection", "Fox collection", "BadgeFoxCollection", "Collect every fox.",
                "Every fox, Aurora included.", f => Collection(f, "fox", "foxes")),
            new BadgeDefinition("night-walker", "Night walker", "BadgeNightWalker", "Find " + NightGoal + " shelf moments after dark.",
                NightGoal + " night moments on your shelf.", f => Count(NightMoments(f), NightGoal, "night moments")),
            new BadgeDefinition("first-fusion", "First fusion", "BadgeFirstFusion", "Fuse two spares into a one-of-a-kind toy.",
                "Your first one-of-a-kind toy.", f => f.Fusions < 0 ? BadgeProgress.Later("Arrives with saved fusions") : Count(f.Fusions, 1, "fusions")),
            new BadgeDefinition("five-fusions", "5 fusions", "BadgeFiveFusions", "Make " + FusionGoal + " fused toys.",
                FusionGoal + " one-of-a-kind toys.", f => f.Fusions < 0 ? BadgeProgress.Later("Arrives with saved fusions") : Count(f.Fusions, FusionGoal, "fusions")),
            new BadgeDefinition("shelf-of-50", "Shelf of 50", "BadgeShelfOf50", "Own " + ShelfGoal + " different toys.",
                ShelfGoal + " different toys on your shelf.", f => Count(Distinct(f), ShelfGoal, "toys")),
            new BadgeDefinition("all-secrets", "All secrets", "BadgeAllSecrets", "Find the secret in every collection.",
                "Every secret, in every collection.", f => Count(SecretsOwned(f), Math.Max(1, SecretCount), "secrets")),
            new BadgeDefinition("collector-level-5", "Collector lvl 5", "BadgeCollectorLevel5", "Reach collector level 5 (" + CollectorLevels[4] + " different toys).",
                "Collector level 5.", f => new BadgeProgress(CollectorLevel(Distinct(f)), CollectorGoal, "Level " + CollectorLevel(Distinct(f)))),
            new BadgeDefinition("first-10k-day", "First 10k day", "BadgeFirst10kDay", "Walk 10,000 steps in one day.",
                "10,000 steps in one day.", f => BadgeProgress.Later("Arrives with step history")),
            new BadgeDefinition("thirty-day-streak", "30-day streak", "BadgeThirtyDayStreak", "Open a walking box 30 days in a row.",
                "30 days in a row.", f => BadgeProgress.Later("Arrives with step history")),
            new BadgeDefinition("one-million-steps", "1M steps", "BadgeOneMillionSteps", "Walk a million steps with Pockle.",
                "A million steps.", f => BadgeProgress.Later("Arrives with step history")),
            new BadgeDefinition("first-swap", "First swap", "BadgeFirstSwap", "Trade a spare with a friend.",
                "Traded a spare with a friend.", f => BadgeProgress.Later("Arrives with trading")),
            new BadgeDefinition("market-maker", "Market maker", "BadgeMarketMaker", "Make 10 swaps.",
                "10 swaps.", f => BadgeProgress.Later("Arrives with trading")),
            new BadgeDefinition("gift-a-spare", "Gift a spare", "BadgeGiftASpare", "Give a spare to a friend.",
                "Gave a spare to a friend.", f => BadgeProgress.Later("Arrives with gifting")),
        };

        public static bool TryGet(string id, out BadgeDefinition badge)
        {
            foreach (var b in All) if (b.Id == id) { badge = b; return true; }
            badge = null; return false;
        }

        public static bool Owns(BadgeFacts f, string id) => !string.IsNullOrEmpty(id) && f.Owned != null && f.Owned.TryGetValue(id, out int n) && n > 0;

        public static int Distinct(BadgeFacts f)
        {
            int n = 0;
            if (f.Owned != null) foreach (var pair in f.Owned) if (pair.Value > 0 && ToyCatalog.TryGetCollectible(pair.Key, out _)) n++;
            return n;
        }

        /// <summary>Secrets in collections whose secret is in the game today.</summary>
        public static int SecretCount
        {
            get { int n = 0; foreach (var c in ToyCatalog.Collections) if (SecretAvailable(c)) n++; return n; }
        }

        public static int SecretsOwned(BadgeFacts f)
        {
            int n = 0;
            foreach (var c in ToyCatalog.Collections) if (SecretAvailable(c) && Owns(f, c.SecretId)) n++;
            return n;
        }

        private static bool SecretAvailable(CollectionDefinition c) => c.HasSecret && ToyCatalog.TryGetCollectible(c.SecretId, out var toy) && toy.Available;

        public static int NightMoments(BadgeFacts f)
        {
            int n = 0;
            if (f.Moments != null) foreach (string id in f.Moments) if (id != null && id.Contains("@night")) n++;
            return n;
        }

        public static int CollectorLevel(int distinct)
        {
            int level = 1;
            for (int i = 1; i < CollectorLevels.Length; i++) if (distinct >= CollectorLevels[i]) level = i + 1;
            return level;
        }

        private static BadgeProgress Count(int have, int need, string noun) => new BadgeProgress(have, need, Math.Min(have, need) + " of " + need + " " + noun);

        private static BadgeProgress Collection(BadgeFacts f, string collectionId, string noun)
        {
            if (!ToyCatalog.TryGetCollection(collectionId, out var c)) return BadgeProgress.Later("Arrives with the collection");
            int have = 0, need = 0;
            foreach (string id in c.Members)
            {
                if (!ToyCatalog.TryGetCollectible(id, out var toy) || !toy.Available) continue;
                need++; if (Owns(f, id)) have++;
            }
            return need == 0 ? BadgeProgress.Later("Arrives with the collection") : Count(have, need, noun);
        }

        /// <summary>"Oct 13" for a UTC day number (days since 1970-01-01).</summary>
        public static string DateLabel(int day) => new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(day).ToString("MMM d", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// When each badge was earned, on this device (PlayerPrefs key <see cref="Key"/>, "id=day,id=day"). Badges are never
    /// taken back. Until accounts and cloud save arrive this is local only, like friendships; see docs/BADGES.md.
    /// </summary>
    public sealed class BadgeCabinet
    {
        public const string Key = "pockle.badges.earned.v1";
        private readonly Dictionary<string, int> earned = new Dictionary<string, int>();
        private readonly List<string> order = new List<string>();

        public BadgeCabinet() { Reload(); }

        public int Count => earned.Count;
        public bool Has(string id) => earned.ContainsKey(id);
        public bool TryGetDay(string id, out int day) => earned.TryGetValue(id, out day);

        /// <summary>The most recent badge (latest day; on a tie, the later one earned), or null.</summary>
        public string Latest
        {
            get
            {
                string best = null; int bestDay = int.MinValue;
                foreach (string id in order) if (earned[id] >= bestDay) { best = id; bestDay = earned[id]; }
                return best;
            }
        }

        /// <summary>Earned badges, newest first.</summary>
        public List<string> Newest()
        {
            // Later day first; on the same day, the one earned later first (a stable insertion sort).
            var list = new List<string>(order.Count);
            for (int i = order.Count - 1; i >= 0; i--)
            {
                int at = list.Count;
                while (at > 0 && earned[list[at - 1]] < earned[order[i]]) at--;
                list.Insert(at, order[i]);
            }
            return list;
        }

        /// <summary>Awards every badge whose rule is met today. Returns the newly earned ids (already saved).</summary>
        public List<string> Award(BadgeFacts facts, int today)
        {
            var fresh = new List<string>();
            foreach (var badge in BadgeBook.All)
                if (!earned.ContainsKey(badge.Id) && badge.Progress(facts).Complete) { earned[badge.Id] = today; order.Add(badge.Id); fresh.Add(badge.Id); }
            if (fresh.Count > 0) Save();
            return fresh;
        }

        public void Save()
        {
            var parts = new List<string>(order.Count);
            foreach (string id in order) parts.Add(id + "=" + earned[id].ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(Key, string.Join(",", parts));
            PlayerPrefs.Save();
        }

        public void Reload()
        {
            earned.Clear(); order.Clear();
            foreach (string part in (PlayerPrefs.GetString(Key, "") ?? "").Split(','))
            {
                int split = part.IndexOf('=');
                if (split <= 0) continue;
                string id = part.Substring(0, split);
                if (!BadgeBook.TryGet(id, out _) || earned.ContainsKey(id)) continue;
                if (!int.TryParse(part.Substring(split + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out int day)) continue;
                earned[id] = day; order.Add(id);
            }
        }
    }
}
