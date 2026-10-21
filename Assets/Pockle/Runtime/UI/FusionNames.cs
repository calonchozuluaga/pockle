using System;
using Pockle.Core;

namespace Pockle.Runtime
{
    /// <summary>
    /// Short names for fused toys (owner, October 21: "Axolotl · Honey Jelly × Copper" overlapped the toy and broke lines).
    /// A fusion is called by the first word of each parent's finish, "Honey × Copper", or "Double Honey" when both parents are
    /// the same finish. The character shows in the picture and the eyebrow, so it isn't repeated. The full recipe stays in
    /// the toy's details.
    /// </summary>
    public static class FusionNames
    {
        public const int MaxLength = 20; // two 8-letter words and " × "

        public static bool TryShort(string id, out string name)
        {
            name = null;
            if (!FusionCatalog.TryGet(id, out var record)) return false;
            name = Short(record.First, record.Second);
            return name.Length > 0;
        }

        public static string Short(string first, string second)
        {
            string a = Word(first), b = Word(second);
            if (a.Length == 0 || b.Length == 0) return a.Length > 0 ? a : b;
            string name = a == b ? "Double " + a : a + " × " + b;
            return name.Length <= MaxLength ? name : Clip(a, 8) + " × " + Clip(b, 8);
        }

        /// <summary>"Honey Jelly · Ultra rare" → "Honey"; "Rose Quartz" → "Rose".</summary>
        private static string Word(string id)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var toy)) return "";
            string finish = toy.FinishDisplayName ?? "";
            int dot = finish.IndexOf(" · ", StringComparison.Ordinal);
            if (dot > 0) finish = finish.Substring(0, dot);
            int space = finish.IndexOf(' ');
            return (space > 0 ? finish.Substring(0, space) : finish).Trim();
        }

        private static string Clip(string word, int max) => word.Length <= max ? word : word.Substring(0, max - 1) + "…";
    }
}
