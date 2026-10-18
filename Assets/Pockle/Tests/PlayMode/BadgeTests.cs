using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;

namespace Pockle.Tests
{
    /// <summary>Badge rules and the local cabinet (docs/BADGES.md).</summary>
    public sealed class BadgeTests
    {
        private bool hadCabinet;
        private string previousCabinet;

        [SetUp]
        public void SetUp()
        {
            hadCabinet = PlayerPrefs.HasKey(BadgeCabinet.Key); previousCabinet = PlayerPrefs.GetString(BadgeCabinet.Key);
            PlayerPrefs.DeleteKey(BadgeCabinet.Key);
        }

        [TearDown]
        public void TearDown()
        {
            if (hadCabinet) PlayerPrefs.SetString(BadgeCabinet.Key, previousCabinet); else PlayerPrefs.DeleteKey(BadgeCabinet.Key);
            PlayerPrefs.Save();
        }

        private static BadgeFacts Owning(params string[] ids) => new BadgeFacts { Owned = ids.ToDictionary(id => id, id => 1) };

        [Test]
        public void FifteenBadgesEachWithArtAndUniqueIds()
        {
            Assert.AreEqual(15, BadgeBook.All.Count);
            Assert.AreEqual(15, BadgeBook.All.Select(b => b.Id).Distinct().Count());
            foreach (var badge in BadgeBook.All)
            {
                Assert.IsNotNull(Resources.Load<Texture2D>(badge.Art), badge.Id + " has Meshy art in Resources/UI/Badges.");
                Assert.IsFalse(string.IsNullOrEmpty(badge.HowTo) || string.IsNullOrEmpty(badge.Earned), badge.Id);
            }
            Assert.IsNotNull(Resources.Load<Texture2D>("UI/Icons/Lock"), "Locked badges wear a lock.");
        }

        [Test]
        public void CollectionAndSecretRulesFollowTheCatalog()
        {
            Assert.IsFalse(Progress("secret-finder", Owning(ToyCatalog.PeachId)).Complete);
            Assert.IsTrue(Progress("secret-finder", Owning(ToyCatalog.FoxAuroraId)).Complete, "Any secret counts.");
            Assert.AreEqual("1 of 1 secret", Progress("secret-finder", Owning(ToyCatalog.FoxAuroraId)).Text);

            var foxes = ToyCatalog.Collectibles.Where(c => c.CollectionId == "fox" && c.Available).Select(c => c.Id).ToArray();
            Assert.Greater(foxes.Length, 1);
            Assert.AreEqual("1 of " + foxes.Length + " foxes", Progress("fox-collection", Owning(foxes[0])).Text);
            Assert.IsFalse(Progress("fox-collection", Owning(foxes.Skip(1).ToArray())).Complete);
            Assert.IsTrue(Progress("fox-collection", Owning(foxes)).Complete);

            var pips = ToyCatalog.Collectibles.Where(c => c.CollectionId == "pip" && c.Available).Select(c => c.Id).ToArray();
            Assert.IsTrue(Progress("pip-series-1", Owning(pips)).Complete, "Pip Series 1 is every Pip finish; the fusion-lab studies aren't part of it.");

            var secrets = ToyCatalog.Collections.Where(c => c.HasSecret && ToyCatalog.TryGetCollectible(c.SecretId, out var s) && s.Available).Select(c => c.SecretId).ToArray();
            Assert.AreEqual(secrets.Length, BadgeBook.SecretCount);
            Assert.Greater(secrets.Length, 1);
            Assert.IsFalse(Progress("all-secrets", Owning(ToyCatalog.FoxAuroraId)).Complete);
            Assert.IsTrue(Progress("all-secrets", Owning(secrets)).Complete);

            // Zero copies (a spare fused away, say) doesn't count; unknown ids never count.
            var none = new BadgeFacts { Owned = new Dictionary<string, int> { { ToyCatalog.FoxAuroraId, 0 }, { "not.a-toy", 3 } } };
            Assert.IsFalse(Progress("secret-finder", none).Complete);
            Assert.AreEqual(0, BadgeBook.Distinct(none));
        }

        [Test]
        public void CollectorLevelsShelfAndNightMoments()
        {
            Assert.AreEqual(1, BadgeBook.CollectorLevel(0));
            Assert.AreEqual(2, BadgeBook.CollectorLevel(5));
            Assert.AreEqual(4, BadgeBook.CollectorLevel(39));
            Assert.AreEqual(5, BadgeBook.CollectorLevel(40));
            var many = Owning(ToyCatalog.Collectibles.Where(c => c.Available).Take(12).Select(c => c.Id).ToArray());
            Assert.AreEqual("Level 2", Progress("collector-level-5", many).Text);
            Assert.AreEqual("12 of 50 toys", Progress("shelf-of-50", many).Text);

            var night = new BadgeFacts { Moments = new[] { "a+b@night", "a+b@day", "c+d@night#2", "e+f@night", "g+h@night", "i+j@night", "k+l@night" } };
            Assert.AreEqual("6 of 7 night moments", Progress("night-walker", night).Text);
            night.Moments = night.Moments.Concat(new[] { "m+n@night" }).ToArray();
            Assert.IsTrue(Progress("night-walker", night).Complete);
        }

        [Test]
        public void BadgesWaitingOnLaterFeaturesAreNeverEarned()
        {
            var everything = Owning(ToyCatalog.Collectibles.Select(c => c.Id).ToArray());
            foreach (string id in new[] { "first-10k-day", "thirty-day-streak", "one-million-steps", "first-swap", "market-maker", "gift-a-spare", "first-fusion", "five-fusions" })
            {
                var progress = Progress(id, everything);
                Assert.IsFalse(progress.Tracked || progress.Complete, id + " waits for its feature.");
                StringAssert.StartsWith("Arrives with", progress.Text);
            }
            everything.Fusions = 1;
            Assert.IsTrue(Progress("first-fusion", everything).Complete, "Once fusions are saved, the count earns them.");
            Assert.IsFalse(Progress("five-fusions", everything).Complete);
        }

        [Test]
        public void TheCabinetKeepsEarnedDatesAndNeverTakesBadgesBack()
        {
            var cabinet = new BadgeCabinet();
            Assert.AreEqual(0, cabinet.Count);
            CollectionAssert.AreEqual(new[] { "secret-finder" }, cabinet.Award(Owning(ToyCatalog.FoxAuroraId), 20370));
            Assert.IsEmpty(cabinet.Award(Owning(ToyCatalog.FoxAuroraId), 20371), "A badge is earned once.");
            Assert.IsEmpty(cabinet.Award(Owning(), 20372), "Losing the toy doesn't take the badge back.");

            var night = new BadgeFacts { Moments = Enumerable.Range(0, 7).Select(i => "t" + i + "+u@night").ToList() };
            CollectionAssert.AreEqual(new[] { "night-walker" }, cabinet.Award(night, 20372));

            var reloaded = new BadgeCabinet();
            Assert.AreEqual(2, reloaded.Count);
            Assert.IsTrue(reloaded.TryGetDay("secret-finder", out int day) && day == 20370);
            Assert.AreEqual("night-walker", reloaded.Latest);
            CollectionAssert.AreEqual(new[] { "night-walker", "secret-finder" }, reloaded.Newest());
            Assert.AreEqual("Oct 13", BadgeBook.DateLabel(20374)); // 2025-10-13

            PlayerPrefs.SetString(BadgeCabinet.Key, "secret-finder=20370,bogus=1,night-walker=oops,,pip-series-1=20371");
            var tolerant = new BadgeCabinet();
            CollectionAssert.AreEquivalent(new[] { "secret-finder", "pip-series-1" }, tolerant.Newest(), "Unknown or broken entries are skipped.");
        }

        private static BadgeProgress Progress(string id, BadgeFacts facts)
        {
            Assert.IsTrue(BadgeBook.TryGet(id, out var badge), id);
            return badge.Progress(facts);
        }
    }
}
