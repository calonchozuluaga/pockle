using System.Collections;
using NUnit.Framework;
using Pockle.Runtime;
using Pockle.Core;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class RosterFinishTests
    {
        [UnityTest]
        public IEnumerator SecretLayerDoesNotLeakWhenPipSwitchesToAnOrdinaryToyOrFusion()
        {
            var root = new GameObject("Edition material reset"); var toy = root.AddComponent<JellyToy>(); toy.Initialize();
            try
            {
                Assert.IsTrue(toy.TrySetCollectible("pip.secret-edition"));
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_EditionEffect"), Is.GreaterThan(3));
                Assert.IsTrue(toy.TrySetCollectible(ToyCatalog.PeachId));
                Assert.AreEqual(0, toy.BodyRenderer.sharedMaterial.GetFloat("_EditionEffect"));
                Assert.IsTrue(toy.TrySetCollectible("pip.secret-edition"));
                Assert.IsTrue(toy.TrySetCollectible(PipFusionStudies.DuskId));
                Assert.AreEqual(0, toy.BodyRenderer.sharedMaterial.GetFloat("_EditionEffect"));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [UnityTest]
        public IEnumerator SecretMotionStopsWhenCalmIsEnabled()
        {
            const string key = "pockle.prototype.reducedMotion";
            bool had = PlayerPrefs.HasKey(key); int previous = PlayerPrefs.GetInt(key);
            var root = new GameObject("Edition calm"); var toy = root.AddComponent<JellyToy>(); toy.Initialize();
            try
            {
                toy.TrySetCollectible("firb.secret-edition"); var material = toy.BodyRenderer.sharedMaterial;
                PlayerPrefs.SetInt(key, 0); yield return null; yield return null;
                float moving = material.GetFloat("_EditionClock"); Assert.That(moving, Is.GreaterThan(0));
                PlayerPrefs.SetInt(key, 1); yield return null;
                float frozen = material.GetFloat("_EditionClock"); yield return new WaitForSecondsRealtime(.08f);
                Assert.AreEqual(frozen, material.GetFloat("_EditionClock"));
            }
            finally { Object.Destroy(root); if(had) PlayerPrefs.SetInt(key,previous); else PlayerPrefs.DeleteKey(key); }
        }
    }
}
