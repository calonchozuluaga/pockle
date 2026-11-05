#nullable enable
using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    public sealed partial class CollectionProgress
    {
        /// <summary>Materialize an authenticated server delivery in the local cache exactly once.
        /// Callers must persist the candidate before exposing it. This never validates payment.</summary>
        public bool ApplyPaidDelivery(string receipt, string product, IReadOnlyList<string> toys)
        {
            var policy = PaidBoxProducts.Find(product);
            if (!Guid.TryParseExact(receipt, "D", out _) || policy == null || toys == null || toys.Count != policy.Count) return false;
            if (Save.PaidDeliveryIds != null && Save.PaidDeliveryIds.Contains(receipt)) return true;
            var additions = new Dictionary<string,int>(StringComparer.Ordinal);
            foreach (string id in toys)
            {
                if (!ToyCatalog.TryGetCollectible(id, out var toy) || !toy.Available ||
                    (policy.CollectionId != null && toy.CollectionId != policy.CollectionId)) return false;
                additions[id] = additions.TryGetValue(id, out int n) ? n + 1 : 1;
            }
            foreach (var item in additions) if (GetOwnedCount(item.Key) > 9999 - item.Value) return false;
            foreach (var item in additions) counts[item.Key] = GetOwnedCount(item.Key) + item.Value;
            if (Save.PaidDeliveryIds == null) Save.PaidDeliveryIds = new List<string>();
            Save.PaidDeliveryIds.Add(receipt);
            SynchronizeInventory();
            return true;
        }
    }
}
