using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class SnowyReactionTests
    {
        [UnityTest]
        public IEnumerator AllTenFinishesHaveMittenPomHeadAndFacialReactionsAndReturnToRest()
        {
            var obj=new GameObject("Snowy reaction test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var driver=obj.AddComponent<SnowyReactionDriver>();driver.Initialize(toy);
            var asset=Resources.Load<PipCharacterAsset>("Characters/Snowy/Snowy");var source=(Vector3[])asset.BodyMesh.vertices.Clone();
            try
            {
                foreach(var item in ToyCatalog.Collectibles.Where(x=>x.CharacterId=="snowy"))
                {
                    Assert.That(toy.TrySetCollectible(item.Id),Is.True);
                    var material=toy.BodyRenderer.sharedMaterial;
                    foreach(SnowyAction action in System.Enum.GetValues(typeof(SnowyAction)))
                    {
                        Assert.That(driver.TryPlay(action),Is.True);var pose=SnowyReactions.Sample(action,.4f,false);driver.ApplyPose(pose);
                        Assert.That(toy.VisualRoot.parent.localScale,Is.EqualTo(Vector3.one),"No whole-body squash on vinyl reactions.");
                        Assert.That(material.GetFloat("_SnowyMitten"),Is.EqualTo(pose.Mitten));Assert.That(material.GetFloat("_SnowyPom"),Is.EqualTo(pose.Pom));
                        Assert.That(material.GetFloat("_SnowyHead"),Is.EqualTo(pose.Head));Assert.That(material.GetFloat("_Happy"),Is.EqualTo(pose.Face.Happy));
                        driver.Cancel();Assert.That(material.GetFloat("_SnowyMitten"),Is.Zero);Assert.That(material.GetFloat("_SnowyPom"),Is.Zero);Assert.That(material.GetFloat("_SnowyHead"),Is.Zero);
                        Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));
                    }
                }
                CollectionAssert.AreEqual(source,asset.BodyMesh.vertices,"Source mesh stays immutable.");
                int previous=-1;
                for(int i=0;i<100;i++){driver.PlayRandom();Assert.That(driver.IsPlaying,Is.True);Assert.That((int)driver.CurrentAction,Is.Not.EqualTo(previous));previous=(int)driver.CurrentAction;driver.Cancel();}
                driver.TryPlay(SnowyAction.MittenWave);yield return new WaitForSeconds(2.2f);Assert.That(driver.IsPlaying,Is.False);
                foreach(string other in new[]{ToyCatalog.FoxId,"coco.matte-vinyl","ginger.matte-vinyl","firb.matte-vinyl"})
                {toy.TrySetCollectible(other);Assert.That(driver.TryPlay(SnowyAction.MittenWave),Is.False);}
                toy.TrySetCollectible("snowy.matte-vinyl");Assert.That(driver.TryPlay(SnowyAction.CozyNod),Is.True);driver.Cancel();
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
    }
}
