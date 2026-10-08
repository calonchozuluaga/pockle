namespace Pockle.Core
{
    public enum PointerTarget { None, Toy, Plate }

    /// <summary>A gesture belongs to its starting surface and finger until release.</summary>
    public sealed class PointerGesture
    {
        public const int NoPointer = -999;
        public int PointerId { get; private set; } = NoPointer;
        public PointerTarget Target { get; private set; }
        public bool IsActive { get { return Target != PointerTarget.None; } }

        public bool TryBegin(int pointerId, PointerTarget target)
        {
            if (IsActive || pointerId == NoPointer ||
                (target != PointerTarget.Toy && target != PointerTarget.Plate)) return false;
            PointerId = pointerId;
            Target = target;
            return true;
        }

        public bool End(int pointerId)
        {
            if (!IsActive || PointerId != pointerId) return false;
            Cancel();
            return true;
        }

        public void Cancel()
        {
            PointerId = NoPointer;
            Target = PointerTarget.None;
        }
    }
}
