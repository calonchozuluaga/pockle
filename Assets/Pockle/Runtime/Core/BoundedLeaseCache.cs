#nullable enable
using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    /// <summary>Single-threaded LRU cache. Leased resources remain alive until the final lease is released.</summary>
    public sealed class BoundedLeaseCache<T> : IDisposable where T : class
    {
        internal sealed class Entry
        {
            public string Id = "";
            public T Value = null!;
            public int References;
            public long Used;
        }
        public sealed class Lease : IDisposable
        {
            private BoundedLeaseCache<T>? owner;
            private readonly Entry entry;
            internal Lease(BoundedLeaseCache<T> owner, Entry entry) { this.owner = owner; this.entry = entry; }
            public string Id => entry.Id;
            public T Value => owner != null ? entry.Value : throw new ObjectDisposedException(nameof(Lease));
            public void Dispose()
            {
                var previous = owner; owner = null;
                if (previous != null) previous.Release(entry);
            }
        }
        private readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>(StringComparer.Ordinal);
        private readonly Func<string, T?> create;
        private readonly Action<T> destroy;
        private long clock;
        private bool disposed;
        public int Capacity { get; }
        public int CachedCount => entries.Count;
        public BoundedLeaseCache(int capacity, Func<string, T?> create, Action<T> destroy)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            Capacity = capacity; this.create = create ?? throw new ArgumentNullException(nameof(create));
            this.destroy = destroy ?? throw new ArgumentNullException(nameof(destroy));
        }
        public Lease? TryAcquire(string? id)
        {
            if (disposed) throw new ObjectDisposedException(nameof(BoundedLeaseCache<T>));
            if (string.IsNullOrEmpty(id)) return null;
            if (!entries.TryGetValue(id, out var entry))
            {
                if (entries.Count >= Capacity)
                {
                    Entry? oldest = null;
                    foreach (var candidate in entries.Values)
                        if (candidate.References == 0 && (oldest == null || candidate.Used < oldest.Used)) oldest = candidate;
                    if (oldest == null) return null; // All visible resources are pinned; do not exceed the budget.
                    Remove(oldest);
                }
                var value = create(id);
                if (value == null) return null;
                entry = new Entry { Id = id, Value = value }; entries.Add(id, entry);
            }
            entry.References++; entry.Used = ++clock;
            return new Lease(this, entry);
        }
        private void Release(Entry entry)
        {
            entry.References--;
            if (disposed && entry.References == 0) Remove(entry);
        }
        private void Remove(Entry entry) { entries.Remove(entry.Id); destroy(entry.Value); }
        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            // Disposing the cache must not invalidate images still held by its consumers.
            foreach (var entry in new List<Entry>(entries.Values))
                if (entry.References == 0) Remove(entry);
        }
    }
}
