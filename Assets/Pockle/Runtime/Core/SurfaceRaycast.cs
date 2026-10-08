using System;

namespace Pockle.Core
{
    /// <summary>Front-face ray picking, also used against the current deformed shell.</summary>
    public static class SurfaceRaycast
    {
        public static bool TryTriangle(Point3 origin, Point3 direction,
            Point3 a, Point3 b, Point3 c, out float distance)
        {
            distance = 0f;
            Point3 edge1 = Subtract(b, a), edge2 = Subtract(c, a);
            Point3 p = Cross(direction, edge2);
            double determinant = Dot(edge1, p);
            if (!(determinant > 1e-8)) return false; // Parallel, invalid, or back-facing.
            Point3 offset = Subtract(origin, a);
            double u = Dot(offset, p) / determinant;
            if (!(u >= -1e-6 && u <= 1.000001)) return false;
            Point3 q = Cross(offset, edge1);
            double v = Dot(direction, q) / determinant;
            if (!(v >= -1e-6 && u + v <= 1.000001)) return false;
            double hit = Dot(edge2, q) / determinant;
            if (!(hit >= 0 && hit <= float.MaxValue)) return false;
            distance = (float)hit;
            return true;
        }

        private static Point3 Subtract(Point3 a, Point3 b)
        { return new Point3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
        private static Point3 Cross(Point3 a, Point3 b)
        { return new Point3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X); }
        private static double Dot(Point3 a, Point3 b)
        { return (double)a.X * b.X + (double)a.Y * b.Y + (double)a.Z * b.Z; }
    }
}
