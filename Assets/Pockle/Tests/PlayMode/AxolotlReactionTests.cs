using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class AxolotlReactionTests
    {
        [UnityTest]
        public IEnumerator AllTenFinishesHaveArmGillTailAndFacialReactionsAndReturnToRest()
        {
            var obj=new GameObject("Axolotl reaction test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var driver=obj.AddComponent<AxolotlReactionDriver>();driver.Initialize(toy);
            var asset=Resources.Load<PipCharacterAsset>("Characters/Axolotl/Axolotl");var source=(Vector3[])asset.BodyMesh.vertices.Clone();
            try
            {
                foreach(var item in ToyCatalog.Collectibles.Where(x=>x.CharacterId=="axolotl"))
                {
                    Assert.That(toy.TrySetCollectible(item.Id),Is.True);
                    var material=toy.BodyRenderer.sharedMaterial;
                    foreach(AxolotlAction action in System.Enum.GetValues(typeof(AxolotlAction)))
                    {
                        Assert.That(driver.TryPlay(action),Is.True);var pose=AxolotlReactions.Sample(action,.4f,false);driver.ApplyPose(pose);
                        Assert.That(toy.VisualRoot.parent.localScale,Is.EqualTo(Vector3.one),"No whole-body squash on vinyl reactions.");
                        Assert.That(material.GetFloat("_ArmWave"),Is.EqualTo(pose.Arm));Assert.That(material.GetFloat("_GillFlutter"),Is.EqualTo(pose.Gills));
                        Assert.That(material.GetFloat("_TailSwish"),Is.EqualTo(pose.Tail));Assert.That(material.GetFloat("_Happy"),Is.EqualTo(pose.Face.Happy));
                        driver.Cancel();Assert.That(material.GetFloat("_ArmWave"),Is.Zero);Assert.That(material.GetFloat("_GillFlutter"),Is.Zero);Assert.That(material.GetFloat("_TailSwish"),Is.Zero);
                        Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));
                    }
                }
                CollectionAssert.AreEqual(source,asset.BodyMesh.vertices,"Source mesh stays immutable.");
                int previous=-1;
                for(int i=0;i<100;i++){driver.PlayRandom();Assert.That(driver.IsPlaying,Is.True);Assert.That((int)driver.CurrentAction,Is.Not.EqualTo(previous));previous=(int)driver.CurrentAction;driver.Cancel();}
                driver.TryPlay(AxolotlAction.HelloWave);yield return new WaitForSeconds(2.2f);Assert.That(driver.IsPlaying,Is.False);
                toy.TrySetCollectible(ToyCatalog.FoxId);Assert.That(driver.TryPlay(AxolotlAction.HelloWave),Is.False);
                toy.TrySetCollectible(ToyCatalog.AxolotlId);Assert.That(driver.TryPlay(AxolotlAction.SleepySway),Is.True);driver.Cancel();
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
    }
}
