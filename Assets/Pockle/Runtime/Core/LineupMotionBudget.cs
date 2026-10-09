#nullable enable
using System;

namespace Pockle.Core
{
    /// <summary>Share one budget across a lineup. Offscreen, calm, and disabled actors release their slots.</summary>
    public sealed class LineupMotionBudget
    {
        public const int MaximumCapacity = 6;
        public int Capacity { get; }
        public int ActiveCount { get; private set; }
        public LineupMotionBudget(int capacity = 4)
        {
            if (capacity < 1 || capacity > MaximumCapacity) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity;
        }
        public IDisposable? TryAcquire()
        {
            if (ActiveCount >= Capacity) return null;
            ActiveCount++; return new Slot(this);
        }
        private sealed class Slot : IDisposable
        {
            private LineupMotionBudget? owner;
            public Slot(LineupMotionBudget owner) { this.owner = owner; }
            public void Dispose() { if (owner == null) return; owner.ActiveCount--; owner = null; }
        }
    }
}
