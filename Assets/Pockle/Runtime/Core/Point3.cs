namespace Pockle.Core
{
    /// <summary>A small, engine-independent point used by the tactile model.</summary>
    public struct Point3
    {
        public readonly float X;
        public readonly float Y;
        public readonly float Z;

        public Point3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
