using System;
using System.Collections.Generic;
using System.Linq;
using Pockle.Core;
internal static class RosterRarityChecks
{
    public static void Run()
    {
        var saved = new CollectionSave { Version = 4, Day = 42, Inventory = new List<OwnedCollectible>() };
        foreach (var collection in ToyCatalog.Collections.Where(c => c.Id != PipFusionStudies.CollectionId))
        {
            Check(collection.Members.Count == 15, collection.Id + " must have fifteen finishes.");
            int[] expected = {9,3,2,1};
            for (int tier = 0; tier < 4; tier++)
                Check(collection.Members.Count(id => ToyCatalog.TryGetCollectible(id,out var t) && (int)t.Tier == tier) == expected[tier], collection.Id + " tier count " + tier);
            Check(collection.SecretId == collection.Members[14], "Secret is last in the tier-ordered lineup.");
            foreach (string id in collection.Members)
            {
                ToyCatalog.TryGetCollectible(id, out var toy);
                Check(toy.IsSecret == (toy.Tier == ToyTier.Secret), "Secret metadata disagrees for " + id);
                if (!RosterFinishDesign.IsAddition(id)) saved.Inventory.Add(new OwnedCollectible(id, 3));
                Check(!collection.HasApprovedOdds, "Art refresh must not publish paid draw odds.");
            }
            string glowId = collection.CharacterId + ".glow-jelly";
            Check(ToyCatalog.TryGetCollectible(glowId, out var glow) && glow.Tier == ToyTier.Epic && !glow.IsSecret, "Existing glow survives as Epic.");
        }
        var restored = new CollectionProgress(saved, 42);
        foreach (var row in saved.Inventory) Check(restored.GetOwnedCount(row.Id) == 3, "Existing ownership lost: " + row.Id);
        Check(ToyCatalog.TryGetCollectible(ToyCatalog.FoxAuroraId, out _) && ToyCatalog.TryGetCollectible(ToyCatalog.AxolotlSpecialId, out _), "Legacy glow IDs must remain loadable.");
    }
    static void Check(bool value, string why) { if (!value) throw new InvalidOperationException(why); }
}
