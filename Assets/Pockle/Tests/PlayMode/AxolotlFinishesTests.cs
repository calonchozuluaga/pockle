using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class AxolotlFinishesTests
    {
        [UnityTest]
        public IEnumerator ReactionsHoldThenReleaseAndCalmSuppressesMotion()
        {
            const string key = "pockle.prototype.reducedMotion";
            bool had = PlayerPrefs.HasKey(key); int old = PlayerPrefs.GetInt(key);
            var root = new GameObject("Axolotl event reaction regression");
            try
            {
                PlayerPrefs.SetInt(key, 0);
                var toy = root.AddComponent<JellyToy>(); toy.TrySetCollectible(ToyCatalog.AxolotlId);
                var driver = root.GetComponentInChildren<AxolotlExpressionDriver>();
                driver.React(new PeachExpressionPose(0, 1, 0), .1f);
                yield return null; yield return null;
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Happy"), Is.EqualTo(1));
                yield return new WaitForSecondsRealtime(.15f); yield return null;
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Happy"), Is.Zero);
                driver.React(new PeachExpressionPose(1, 1, 1), 1);
                PlayerPrefs.SetInt(key, 1); yield return null; yield return null;
                foreach (string property in new[] { "_Happy", "_Blink", "_Surprise", "_IdleWave" })
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat(property), Is.Zero);
            }
            finally
            {
                Object.Destroy(root);
                if (had) PlayerPrefs.SetInt(key, old); else PlayerPrefs.DeleteKey(key);
            }
            yield return null;
        }
        [UnityTest]
        public IEnumerator AllTenUseApprovedArtAndKeepFacesAndOwnership()
        {
            Assert.That(CharacterArt.TryLoad(ToyCatalog.AxolotlId, out var approved), Is.True);
            var root = new GameObject("Axolotl finish regression");
            try
            {
                var toy = root.AddComponent<JellyToy>();
                var finishes = ToyCatalog.Collectibles.Where(c => c.CharacterId == "axolotl").ToArray();
                Assert.That(finishes.Length, Is.EqualTo(10));
                Assert.That(finishes.Count(c => c.IsSecret), Is.EqualTo(1));
                foreach (var finish in finishes)
                {
                    Assert.That(finish.Available, Is.True);
                    Assert.That(CharacterArt.TryLoad(finish.Id, out var asset), Is.True);
                    Assert.That(asset, Is.SameAs(approved));
                    Assert.That(toy.TrySetCollectible(finish.Id), Is.True);
                    Assert.That(toy.BodyRenderer.sharedMaterial.mainTexture, Is.SameAs(approved.BaseColorTexture));
                    Assert.That(toy.BodyRenderer.sharedMaterial.shader.isSupported, Is.True);
                    Assert.That(root.GetComponentsInChildren<AxolotlExpressionDriver>().Length, Is.EqualTo(1));
                    Assert.That(ToyCatalog.IsDailyEligible(finish.Id), Is.False);
                    yield return null;
                }
                Assert.That(new CollectionProgress(null, CollectionSession.Today).GetOwnedCount(ToyCatalog.AxolotlSpecialId), Is.Zero);
                var driver = root.GetComponentInChildren<AxolotlExpressionDriver>();
                driver.Apply(new PeachExpressionPose(1, 0, 0), .5f);
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Blink"), Is.EqualTo(1));
                driver.Apply(default, 0);
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_IdleWave"), Is.Zero);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
    }
}
