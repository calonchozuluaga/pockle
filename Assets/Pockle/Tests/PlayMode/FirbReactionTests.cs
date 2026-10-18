using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class FirbReactionTests
    {
        [UnityTest]
        public IEnumerator AllTenFinishesHaveBranchStarOrnamentAndFacialReactionsAndReturnToRest()
        {
            var obj=new GameObject("Firb reaction test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var driver=obj.AddComponent<FirbReactionDriver>();driver.Initialize(toy);
            var asset=Resources.Load<PipCharacterAsset>("Characters/Firb/Firb");var source=(Vector3[])asset.BodyMesh.vertices.Clone();
            try
            {
                foreach(var item in ToyCatalog.Collectibles.Where(x=>x.CharacterId=="firb"))
                {
                    Assert.That(toy.TrySetCollectible(item.Id),Is.True);
                    var material=toy.BodyRenderer.sharedMaterial;
                    foreach(FirbAction action in System.Enum.GetValues(typeof(FirbAction)))
                    {
                        Assert.That(driver.TryPlay(action),Is.True);var pose=FirbReactions.Sample(action,.4f,false);driver.ApplyPose(pose);
                        Assert.That(toy.VisualRoot.parent.localScale,Is.EqualTo(Vector3.one),"No whole-body squash on vinyl reactions.");
                        Assert.That(material.GetFloat("_FirbBranch"),Is.EqualTo(pose.Branch));Assert.That(material.GetFloat("_FirbStar"),Is.EqualTo(pose.Star));
                        Assert.That(material.GetFloat("_FirbOrnaments"),Is.EqualTo(pose.Ornaments));Assert.That(material.GetFloat("_Happy"),Is.EqualTo(pose.Face.Happy));
                        driver.Cancel();Assert.That(material.GetFloat("_FirbBranch"),Is.Zero);Assert.That(material.GetFloat("_FirbStar"),Is.Zero);Assert.That(material.GetFloat("_FirbOrnaments"),Is.Zero);
                        Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));
                    }
                }
                CollectionAssert.AreEqual(source,asset.BodyMesh.vertices,"Source mesh stays immutable.");
                int previous=-1;
                for(int i=0;i<100;i++){driver.PlayRandom();Assert.That(driver.IsPlaying,Is.True);Assert.That((int)driver.CurrentAction,Is.Not.EqualTo(previous));previous=(int)driver.CurrentAction;driver.Cancel();}
                driver.TryPlay(FirbAction.BranchWave);yield return new WaitForSeconds(2.2f);Assert.That(driver.IsPlaying,Is.False);
                foreach(string other in new[]{ToyCatalog.FoxId,"coco.matte-vinyl","snowy.matte-vinyl","ginger.matte-vinyl"})
                {toy.TrySetCollectible(other);Assert.That(driver.TryPlay(FirbAction.BranchWave),Is.False);}
                toy.TrySetCollectible("firb.matte-vinyl");Assert.That(driver.TryPlay(FirbAction.SleepySway),Is.True);driver.Cancel();
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
    }
}
