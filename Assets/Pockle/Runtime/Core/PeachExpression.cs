using System;

namespace Pockle.Core
{
    public readonly struct PeachExpressionPose
    {
        public float Blink { get; }
        public float Happy { get; }
        public float Surprise { get; }
        public PeachExpressionPose(float blink, float happy, float surprise)
        { Blink = Clean(blink); Happy = Clean(happy); Surprise = Clean(surprise); }
        private static float Clean(float value) => Numeric.Clamp(Numeric.FiniteOr(value, 0), 0, 1);
    }

    /// <summary>Authored, bounded expression timing for the final approved Peach; no random state or allocations.</summary>
    public static class PeachExpression
    {
        public static PeachExpressionPose Sample(double elapsed, float pressure, float liftedHeight, bool calm)
        {
            if (calm || double.IsNaN(elapsed) || double.IsInfinity(elapsed) || elapsed < 0) return default;
            double phase = elapsed % 5.1;
            float blink = 0;
            if (phase > 4.55 && phase < 4.77)
            { double wave = Math.Sin((phase - 4.55) / .22 * Math.PI); blink = (float)(wave * wave); }
            float happy = Smooth(Numeric.Clamp(Numeric.FiniteOr(pressure, 0) / .24f, 0, 1));
            float surprise = Smooth(Numeric.Clamp((Numeric.FiniteOr(liftedHeight, 0) - .1f) / .5f, 0, 1)) * (1 - happy);
            return new PeachExpressionPose(blink, happy, surprise);
        }
        private static float Smooth(float t) => t * t * (3 - 2 * t);
    }
}
