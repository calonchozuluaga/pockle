using System;

namespace Pockle.Core
{
    /// <summary>A soft pebble with a slightly fuller lower body and a fixed base.</summary>
    public static class JellyShape
    {
        /// <summary>
        /// Samples the original surface. Latitude runs from 0 at the crown to pi at
        /// the base; longitude turns around the vertical axis. Coordinates are local.
        /// </summary>
        public static Point3 Rest(float latitude, float longitude)
        {
            double angle = Numeric.FiniteOr(latitude, (float)(Math.PI * .5d));
            angle = Math.Max(0d, Math.Min(Math.PI, angle));
            double around = Numeric.FiniteOr(longitude, 0f);
            double height = Math.Cos(angle);
            double radius = Math.Max(0d, Math.Sin(angle));

            return new Point3(
                (float)(.85d * (1d - .10d * height) * radius * Math.Cos(around)),
                (float)height,
                (float)(.72d * (1d - .06d * height) * radius * Math.Sin(around)));
        }

        /// <summary>
        /// Squash is 0..0.42; stretch is 0..0.6. Tilt values are normalized -1..1
        /// lateral lean. Contact coordinates use the same local X/Z units as Rest.
        /// Vertical scale and inverse-square-root radial scale conserve bulk volume;
        /// a small pressure dent provides a localized response without collapsing it.
        /// </summary>
        public static Point3 Deform(Point3 rest, float compression, float stretch,
            float tiltX, float tiltZ, float contactX, float contactZ)
        {
            float originalX = Numeric.Clamp(Numeric.FiniteOr(rest.X, 0f), -1.5f, 1.5f);
            float originalY = Numeric.Clamp(Numeric.FiniteOr(rest.Y, 0f), -1f, 1f);
            float originalZ = Numeric.Clamp(Numeric.FiniteOr(rest.Z, 0f), -1.5f, 1.5f);
            compression = Numeric.Clamp(Numeric.FiniteOr(compression, 0f), 0f, .42f);
            stretch = Numeric.Clamp(Numeric.FiniteOr(stretch, 0f), 0f, .6f);
            tiltX = Numeric.Clamp(Numeric.FiniteOr(tiltX, 0f), -1f, 1f);
            tiltZ = Numeric.Clamp(Numeric.FiniteOr(tiltZ, 0f), -1f, 1f);
            contactX = Numeric.Clamp(Numeric.FiniteOr(contactX, 0f), -1.2f, 1.2f);
            contactZ = Numeric.Clamp(Numeric.FiniteOr(contactZ, 0f), -1.2f, 1.2f);

            // This also keeps a flattened base ring fixed if a mesh uses one.
            if (originalY <= -1f)
                return new Point3(originalX, -1f, originalZ);

            float heightFraction = (originalY + 1f) * .5f;
            float baseWeight = Numeric.SmoothStep(heightFraction / .2f);
            float leanWeight = heightFraction * heightFraction;
            float heightScale = 1f - compression + stretch;
            float radialScale = 1f + ((float)(1d / Math.Sqrt(heightScale)) - 1f) * baseWeight;

            float distanceX = originalX - contactX;
            float distanceZ = originalZ - contactZ;
            float contactInfluence = (float)Math.Exp(-(distanceX * distanceX + distanceZ * distanceZ) / .2025f);
            float pressureWeight = Numeric.SmoothStep((heightFraction - .2f) / .6f);
            float dent = compression * .18f * pressureWeight * contactInfluence;
            float localBulge = 1f + compression * .035f * baseWeight * contactInfluence;

            return new Point3(
                originalX * radialScale * localBulge + tiltX * .42f * leanWeight,
                -1f + (originalY + 1f) * heightScale - dent,
                originalZ * radialScale * localBulge + tiltZ * .34f * leanWeight);
        }
    }
}
