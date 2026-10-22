using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class WallyPilotTests
    {
        [UnityTest]
        public IEnumerator PilotMaterialsUseDistinctSurfacesAndCalmFreezesTheVeins()
        {
            const string key="pockle.prototype.reducedMotion";bool had=PlayerPrefs.HasKey(key);int previous=PlayerPrefs.GetInt(key);
            var root=new GameObject("Wally pilot surface checks");var toy=root.AddComponent<JellyToy>();toy.Initialize();
            var driver=root.AddComponent<WallyReactionDriver>();driver.Initialize(toy);
            try
            {
                Assert.IsTrue(toy.TrySetCollectible(WallyCatalog.LeafId));Assert.AreEqual(1,toy.BodyRenderer.sharedMaterial.GetFloat("_GoldLeaf"));
                Assert.IsTrue(toy.TrySetCollectible(WallyCatalog.RoseId));Assert.AreEqual(0,toy.BodyRenderer.sharedMaterial.GetFloat("_GoldLeaf"));
                Assert.AreEqual(1,toy.BodyRenderer.sharedMaterial.GetFloat("_Metallic"));
                Assert.IsTrue(toy.TrySetCollectible(WallyCatalog.DiamondId));Assert.AreEqual(1,toy.BodyRenderer.sharedMaterial.GetFloat("_Diamond"));
                Assert.IsTrue(toy.TrySetCollectible(WallyCatalog.SpecialId));Assert.AreEqual(1,toy.BodyRenderer.sharedMaterial.GetFloat("_Molten"));
                Assert.AreSame(MaterialHandling.Firm,toy.Handling,"Obsidian uses rigid handling, independent of tier.");
                PlayerPrefs.SetInt(key,1);yield return null;float first=toy.BodyRenderer.sharedMaterial.GetFloat("_RarePulse");
                yield return null;Assert.AreEqual(1,first);Assert.AreEqual(first,toy.BodyRenderer.sharedMaterial.GetFloat("_RarePulse"));
                Assert.IsTrue(toy.TrySetCollectible(WallyCatalog.GlowId));Assert.AreSame(MaterialHandling.Gel,toy.Handling,"Epic glow remains squishable.");
                Assert.IsTrue(WallyCatalog.EmitsLight(WallyCatalog.SpecialId));Assert.IsTrue(WallyCatalog.EmitsLight(WallyCatalog.GlowId));
                Assert.IsFalse(WallyCatalog.EmitsLight(WallyCatalog.DiamondId));
            }
            finally
            {
                if(had)PlayerPrefs.SetInt(key,previous);else PlayerPrefs.DeleteKey(key);
                Object.Destroy(root);
            }
        }
    }
}
