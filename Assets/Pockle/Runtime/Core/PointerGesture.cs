namespace Pockle.Core
{
    public enum PointerTarget { None, Toy, Plate }

    /// <summary>A gesture belongs to its starting surface and finger until release.</summary>
    public sealed class PointerGesture
    {
        public const int NoPointer = -999;
        public int PointerId { get; private set; } = NoPointer;
        public int SecondPointerId { get; private set; } = NoPointer;
        public bool IsPair { get { return SecondPointerId != NoPointer; } }
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
            if (!IsActive) return false;
            if (IsPair && pointerId == SecondPointerId)
            {
                SecondPointerId = NoPointer;
                return true;
            }
            if (PointerId != pointerId) return false;
            if (IsPair)
            {
                PointerId = SecondPointerId;
                SecondPointerId = NoPointer;
                return true;
            }
            Cancel();
            return true;
        }

        /// <summary>Only an explicitly hit toy touch can join a captured toy gesture.</summary>
        public bool TryJoin(int pointerId, PointerTarget startingSurface)
        {
            if (Target != PointerTarget.Toy || startingSurface != PointerTarget.Toy || IsPair ||
                PointerId < 0 || pointerId < 0 || pointerId == PointerId) return false;
            SecondPointerId = pointerId;
            return true;
        }

        public void Cancel()
        {
            PointerId = NoPointer;
            SecondPointerId = NoPointer;
            Target = PointerTarget.None;
        }
    }
}
