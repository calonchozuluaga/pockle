namespace Pockle.Runtime
{
    public enum PipVariant { PeachJelly = 0, MoonJelly = 1 }

    public static class PipVariants
    {
        public const string Preference = "pockle.prototype.pipVariant";
        public static PipVariant FromSaved(int value)
        { return value == 1 ? PipVariant.MoonJelly : PipVariant.PeachJelly; }
        public static string Label(PipVariant variant)
        { return variant == PipVariant.MoonJelly ? "MOON JELLY" : "PEACH JELLY"; }
    }
}
