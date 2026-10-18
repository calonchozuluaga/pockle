using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;

namespace Pockle.Tests
{
    /// <summary>The rarity colour index: each tier has its own colour and icon, distinct from the personality chips.</summary>
    public sealed class RarityLookTests
    {
        [Test]
        public void TiersReadFromLabelsWithTheirOwnColourAndIcon()
        {
            Assert.AreEqual(RarityTier.UltraRare, RarityLook.TierOf("Ultra rare"));
            Assert.AreEqual(RarityTier.SuperRare, RarityLook.TierOf("Super rare"));
            Assert.AreEqual(RarityTier.Rare, RarityLook.TierOf("Rare"));
            Assert.AreEqual(RarityTier.Common, RarityLook.TierOf(""));

            Assert.IsTrue(ToyCatalog.TryGetCollectible(ToyCatalog.PipSpecialId, out var starlight));
            var ultra = RarityLook.Of(starlight);
            Assert.AreEqual(RarityTier.UltraRare, ultra.Tier);
            Assert.AreEqual("Ultra rare · Secret", ultra.Text);
            Assert.IsTrue(ToyCatalog.TryGetCollectible(ToyCatalog.FoxAuroraId, out var aurora));
            Assert.AreEqual(RarityTier.SuperRare, RarityLook.Of(aurora).Tier);
            Assert.IsTrue(ToyCatalog.TryGetCollectible(ToyCatalog.PeachId, out var peach));
            Assert.IsFalse(RarityLook.Of(peach).Shown, "Common toys show no rarity badge.");

            var tiers = new[] { RarityTier.Rare, RarityTier.SuperRare, RarityTier.UltraRare, RarityTier.Fusion }.Select(t => RarityLook.For(t, false, "x")).ToArray();
            Assert.AreEqual(tiers.Length, tiers.Select(t => t.Fill).Distinct().Count(), "Every tier has its own colour.");
            Assert.AreEqual(tiers.Length, tiers.Select(t => t.Icon).Distinct().Count(), "Every tier has its own icon.");
            foreach (var t in tiers)
            {
                Assert.AreEqual(1f, t.Fill.a, "Rarity badges are solid, unlike the see-through trait chips.");
                Assert.IsNotNull(PockleTheme.Icon(t.Icon), t.Icon + " is in Resources/UI/Icons.");
            }
            // Every secret in the catalog gets a badge.
            foreach (var toy in ToyCatalog.Collectibles.Where(c => c.Available && c.IsSecret))
                Assert.IsTrue(RarityLook.Of(toy).Shown && RarityLook.Of(toy).Secret, toy.Id);
        }
    }
}
