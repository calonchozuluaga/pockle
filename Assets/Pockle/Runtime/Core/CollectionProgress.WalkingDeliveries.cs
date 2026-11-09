#nullable enable
using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    public sealed partial class CollectionProgress
    {
        /// <summary>Apply a saved, authenticated walking-box result to this local cache once.
        /// The server alone earns boxes and selects their contents; persist the candidate before revealing it.</summary>
        public bool ApplyWalkingDelivery(string receipt, string collectionId, string toyId, int earnedDay)
        {
            if (!Guid.TryParseExact(receipt, "D", out var parsed) ||
                !ToyCatalog.TryGetCollectible(toyId, out var toy) || !toy.Available ||
                toy.CollectionId != collectionId || earnedDay < 0 || earnedDay > 2932896) return false;
            string canonical = parsed.ToString("D");
            if (Save.WalkingDeliveryIds != null && Save.WalkingDeliveryIds.Contains(canonical)) return true;
            if (GetOwnedCount(toyId) >= 9999) return false;
            counts[toyId] = GetOwnedCount(toyId) + 1;
            MarkEarned(toyId);
            if (Save.WalkingDeliveryIds == null) Save.WalkingDeliveryIds = new List<string>();
            Save.WalkingDeliveryIds.Add(canonical);
            if (!Save.BadgeState!.WalkingBoxDays.Contains(earnedDay))
            {
                Save.BadgeState.WalkingBoxDays.Add(earnedDay);
                Save.BadgeState.WalkingBoxDays.Sort();
            }
            SynchronizeInventory();
            return true;
        }
    }
}
