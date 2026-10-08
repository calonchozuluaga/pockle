using System;

namespace Pockle.Core
{
    internal static class Numeric
    {
        internal static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        internal static float Clamp(float value, float minimum, float maximum)
        {
            return value < minimum ? minimum : value > maximum ? maximum : value;
        }

        internal static float FiniteOr(float value, float fallback)
        {
            return IsFinite(value) ? value : fallback;
        }

        internal static float ToFiniteFloat(double value)
        {
            if (double.IsNaN(value))
                return 0f;
            if (value > float.MaxValue)
                return float.MaxValue;
            if (value < -float.MaxValue)
                return -float.MaxValue;
            return (float)value;
        }

        internal static float SmoothStep(float value)
        {
            value = Clamp(value, 0f, 1f);
            return value * value * (3f - 2f * value);
        }
    }
}
