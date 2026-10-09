using Pockle.Core;

namespace Pockle.Runtime
{
    public enum PipVariant { PeachJelly = 0, MoonJelly = 1, GoldGlitter = 2, MintSoft = 3 }

    public static class PipVariants
    {
        public const string Preference = "pockle.prototype.pipVariant";
        public const int Count = 4;
        public static PipVariant FromSaved(int value)
        { return value >= 0 && value < Count ? (PipVariant)value : PipVariant.PeachJelly; }
        public static string CollectibleId(PipVariant variant) => ToyCatalog.LegacyCollectibleId((int)FromSaved((int)variant));
        public static bool TryFromCollectibleId(string id, out PipVariant variant)
        {
            bool known = ToyCatalog.TryGetLegacyIndex(id, out int index);
            variant = known ? (PipVariant)index : PipVariant.PeachJelly;
            return known;
        }
        public static string Label(PipVariant variant)
        {
            switch (variant)
            {
                case PipVariant.MoonJelly: return "MOON JELLY";
                case PipVariant.GoldGlitter: return "GOLD GLITTER";
                case PipVariant.MintSoft: return "MINT SOFT";
                default: return "PEACH JELLY";
            }
        }
        public static string ButtonLabel(PipVariant variant)
        {
            switch (variant)
            {
                case PipVariant.MoonJelly: return "Moon";
                case PipVariant.GoldGlitter: return "Glitter";
                case PipVariant.MintSoft: return "Soft";
                default: return "Peach";
            }
        }
    }
}
