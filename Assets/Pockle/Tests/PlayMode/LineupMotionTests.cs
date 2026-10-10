using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class LineupMotionTests
    {
        private GameObject root;
        [SetUp] public void Setup() { root = new GameObject("Lineup motion fixture"); }
        [UnityTearDown] public IEnumerator Teardown() { Object.Destroy(root); yield return null; }

        [Test]
        public void BetaOffersStayPipSubPoolsWithTheirExistingOdds()
        {
            Assert.That(BoxCatalog.Offers.Length, Is.EqualTo(3));
            foreach (var offer in BoxCatalog.Offers)
            {
                Assert.That(offer.CollectionId, Is.EqualTo("pip"));
                Assert.That(offer.Collection.CharacterId, Is.EqualTo("pip"));
                Assert.That(offer.Collection.SeriesNumber, Is.EqualTo(1));
                Assert.That(offer.Pool.Id, Is.EqualTo(offer.PoolId));
            }
            Assert.That(BoxCatalog.Offers[0].Pool.Entries.Count, Is.EqualTo(2));
            Assert.That(BoxCatalog.Offers[0].Pool.Entries[0].Weight, Is.EqualTo(50));
            Assert.That(BoxCatalog.Offers[1].Pool.Entries[0].CollectibleId, Is.EqualTo(ToyCatalog.MoonId));
            Assert.That(BoxCatalog.Offers[2].Pool.Entries[0].CollectibleId, Is.EqualTo(ToyCatalog.GoldId));
        }

        [UnityTest]
        public IEnumerator VisibleBudgetCalmAndDisableRestoreTheToy()
        {
            var budget = new LineupMotionBudget(1);
            var firstObject = new GameObject("First actor"); firstObject.transform.SetParent(root.transform, false);
            var secondObject = new GameObject("Second actor"); secondObject.transform.SetParent(root.transform, false);
            firstObject.transform.localPosition = new Vector3(3, 2, 1);
            firstObject.transform.localRotation = Quaternion.Euler(0, 20, 0);
            var restPosition = firstObject.transform.localPosition; var restRotation = firstObject.transform.localRotation;
            var firstToy = firstObject.AddComponent<JellyToy>(); var secondToy = secondObject.AddComponent<JellyToy>();
            var first = firstObject.AddComponent<ToyLineupAnimator>(); var second = secondObject.AddComponent<ToyLineupAnimator>();
            Assert.That(first.Configure(firstToy, budget, eager: true), Is.True);
            Assert.That(second.Configure(secondToy, budget), Is.True);
            Assert.That(first.enabled, Is.False, "Hidden toys must not tick.");
            Assert.That(first.SetVisible(true), Is.True);
            Assert.That(second.SetVisible(true), Is.False);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(first.IsAnimating, Is.True); Assert.That(second.enabled, Is.False);
            Assert.That(budget.ActiveCount, Is.EqualTo(1));
            Assert.That(firstToy.transform.localPosition.y, Is.GreaterThan(restPosition.y));
            first.SetCalm(true);
            Assert.That(first.enabled, Is.False); Assert.That(budget.ActiveCount, Is.Zero);
            Assert.That(Vector3.Distance(firstToy.transform.localPosition, restPosition), Is.LessThan(.00001f));
            Assert.That(Quaternion.Angle(firstToy.transform.localRotation, restRotation), Is.LessThan(.001f));
            Assert.That(second.SetVisible(true), Is.True);
            secondObject.SetActive(false);
            Assert.That(budget.ActiveCount, Is.Zero, "Inactive objects leaked their slots.");
            first.SetCalm(false);
            Assert.That(first.IsAnimating, Is.True);
            Assert.That(first.SetVisible(false), Is.True);
            Assert.That(first.enabled, Is.False); Assert.That(budget.ActiveCount, Is.Zero);
            Assert.That(Vector3.Distance(firstToy.transform.localPosition, restPosition), Is.LessThan(.00001f));
        }

        [UnityTest]
        public IEnumerator ReconfigurationAndDestructionReleaseSlots()
        {
            var budget = new LineupMotionBudget(1);
            var toy = root.AddComponent<JellyToy>(); var animator = root.AddComponent<ToyLineupAnimator>();
            Assert.That(animator.Configure(toy, budget, phaseOffset: .25f), Is.True);
            Assert.That(animator.SetVisible(true), Is.True);
            yield return null;
            Assert.That(animator.Configure(toy, budget), Is.True);
            Assert.That(budget.ActiveCount, Is.Zero);
            Assert.That(animator.IsAnimating, Is.False);
            Assert.That(animator.Configure(null, budget), Is.False);
            Assert.That(animator.Configure(toy, budget, phaseOffset: float.NaN), Is.False);
            Assert.That(budget.ActiveCount, Is.Zero);
            Assert.That(animator.SetVisible(true), Is.True);
            Object.Destroy(animator);
            yield return null;
            Assert.That(budget.ActiveCount, Is.Zero);
        }

        [UnityTest]
        public IEnumerator ADisabledToyReleasesItsAnimationSlot()
        {
            var budget = new LineupMotionBudget(1);
            var toy = root.AddComponent<JellyToy>(); var animator = root.AddComponent<ToyLineupAnimator>();
            Assert.That(animator.Configure(toy, budget), Is.True);
            Assert.That(animator.SetVisible(true), Is.True);
            toy.enabled = false;
            yield return null;
            Assert.That(budget.ActiveCount, Is.Zero); Assert.That(animator.IsAnimating, Is.False);
            toy.enabled = true;
            Assert.That(animator.SetVisible(true), Is.True);
            Assert.That(budget.ActiveCount, Is.EqualTo(1));
        }
    }
}
