using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// How many days each pair of toys has spent side by side on the live shelf (docs/PERSONALITY_SYSTEM.md section 4).
    /// A local presentation preference like found moments: never inventory, never decays, and moving toys apart keeps it.
    /// Stored as "a|b=days@lastDay" entries; a day counts once per pair.
    /// </summary>
    public static class ShelfFriendships
    {
        public const string Key = "pockle.shelf.friendship.v1";
        private static Dictionary<string, (int days, int last)> pairs;

        private static string PairKey(string a, string b) => string.CompareOrdinal(a, b) <= 0 ? a + "|" + b : b + "|" + a;

        private static void Load()
        {
            if (pairs != null) return;
            pairs = new Dictionary<string, (int, int)>();
            foreach (string entry in (PlayerPrefs.GetString(Key, "") ?? "").Split(';'))
            {
                int eq = entry.IndexOf('='), at = entry.IndexOf('@');
                if (eq <= 0 || at <= eq) continue;
                if (int.TryParse(entry.Substring(eq + 1, at - eq - 1), out int days) && int.TryParse(entry.Substring(at + 1), out int last))
                    pairs[entry.Substring(0, eq)] = (Math.Max(0, days), last);
            }
        }

        /// <summary>Forgets the cached copy (tests clear the preference between runs).</summary>
        public static void Reload() { pairs = null; }

        public static int Days(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || a == b) return 0;
            Load();
            return pairs.TryGetValue(PairKey(a, b), out var entry) ? entry.days : 0;
        }

        public static int Level(string a, string b) => PersonalityBook.FriendshipLevel(Days(a, b));

        /// <summary>Counts today for a pair seen side by side. Returns false when today was already counted.</summary>
        public static bool RecordTogether(string a, string b, int today)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) || a == b) return false;
            Load();
            string key = PairKey(a, b);
            pairs.TryGetValue(key, out var entry);
            if (entry.days > 0 && entry.last >= today) return false;
            pairs[key] = (entry.days + 1, today);
            return true;
        }

        public static void Save()
        {
            if (pairs == null) return;
            var text = new StringBuilder();
            foreach (var pair in pairs)
            {
                if (text.Length > 0) text.Append(';');
                text.Append(pair.Key).Append('=').Append(pair.Value.days).Append('@').Append(pair.Value.last);
            }
            PlayerPrefs.SetString(Key, text.ToString());
            PlayerPrefs.Save();
        }
    }
}
