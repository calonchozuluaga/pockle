using System;
namespace Pockle.Core
{
    /// <summary>Stable unordered-pair appearance; independent of personality and result IDs.</summary>
    public static class FusionPattern
    {
        public static int Kind(string first, string second)
        {
            if (string.CompareOrdinal(first, second) > 0) { string swap = first; first = second; second = swap; }
            uint hash = 2166136261;
            unchecked { foreach (char c in first + "|" + second) hash = (hash ^ c) * 16777619; }
            return 1 + (int)(hash % 4); // gradient, swirl, spots, ribbons
        }
    }
}
