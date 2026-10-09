#nullable enable
using System;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Owned portrait leases. Unavailable art returns null; release images when leaving the visible set.</summary>
    public sealed class CollectiblePreviewCache : IDisposable
    {
        public sealed class PreviewLease : IDisposable
        {
            private readonly BoundedLeaseCache<RenderTexture>.Lease lease;
            internal PreviewLease(BoundedLeaseCache<RenderTexture>.Lease lease) { this.lease = lease; }
            public string Id => lease.Id;
            public RenderTexture Texture => lease.Value;
            public void Dispose() { lease.Dispose(); }
        }
        private readonly BoundedLeaseCache<RenderTexture> cache;
        public int Capacity => cache.Capacity;
        public int CachedCount => cache.CachedCount;
        public CollectiblePreviewCache(int capacity = 8)
        { cache = new BoundedLeaseCache<RenderTexture>(capacity, Render, Release); }
        public PreviewLease? TryAcquire(string id)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var definition) || !definition.Available) return null;
            var lease = cache.TryAcquire(id); return lease != null ? new PreviewLease(lease) : null;
        }
        private static RenderTexture? Render(string id)
        {
            if (!PipVariants.TryFromCollectibleId(id, out var variant)) return null;
            try { return ToyPortrait.Render(variant); }
            catch (Exception exception)
            { Debug.LogWarning("Pockle portrait unavailable for " + id + ": " + exception.GetType().Name); return null; }
        }
        private static void Release(RenderTexture texture)
        { if (texture != null) { texture.Release(); UnityEngine.Object.Destroy(texture); } }
        public void Dispose() { cache.Dispose(); }
    }
}
