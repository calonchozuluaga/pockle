using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class FoxReactionTests
    {
        [UnityTest]
        public IEnumerator AllTenFinishesHaveTailFaceAndRigidBodyReactionsWithExactReset()
        {
            var obj=new GameObject("Fox reactions test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var driver=obj.AddComponent<FoxReactionDriver>();driver.Initialize(toy);
            var asset=Resources.Load<PipCharacterAsset>("Characters/Fox/Fox");var source=(Vector3[])asset.BodyMesh.vertices.Clone();
            try
            {
                foreach(var item in ToyCatalog.Collectibles.Where(x=>x.CharacterId=="fox"))
                {
                    Assert.That(toy.TrySetCollectible(item.Id),Is.True);
                    var mesh=toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;var rest=mesh.vertices;
                    foreach(FoxAction action in System.Enum.GetValues(typeof(FoxAction)))
                    {
                        Assert.That(driver.TryPlay(action),Is.True);driver.ApplyPose(FoxReactions.Sample(action,.4f,false));
                        Assert.That(toy.VisualRoot.parent.localScale,Is.EqualTo(Vector3.one),"Articulation never squashes the Fox body.");
                        var changed=mesh.vertices;
                        if(action==FoxAction.TailWag||action==FoxAction.GreetingBow)
                        {
                            Assert.That(rest.Where((v,i)=>v.x<-.45f&&v.z>.4f&&(v-changed[i]).magnitude>.02f).Count(),Is.GreaterThan(100),"Tail moves visibly.");
                            foreach(int i in Enumerable.Range(0,rest.Length).Where(i=>rest[i].y<-.95f&&rest[i].z<0))Assert.That(changed[i],Is.EqualTo(rest[i]),"Paws remain rigid.");
                        }
                        if(action==FoxAction.SleepyBlink)
                            Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_TemperBlink"),Is.GreaterThan(.7f),"The authored face closes without folding the eye geometry.");
                        driver.Cancel();CollectionAssert.AreEqual(rest,mesh.vertices,"No accumulated vertex drift after a reaction.");
                    }
                }
                CollectionAssert.AreEqual(source,asset.BodyMesh.vertices,"Source mesh remains immutable.");
                int previous=-1;
                for(int i=0;i<100;i++){driver.PlayRandom();Assert.That(driver.IsPlaying,Is.True);Assert.That((int)driver.CurrentAction,Is.Not.EqualTo(previous));previous=(int)driver.CurrentAction;driver.Cancel();}
                driver.TryPlay(FoxAction.Jump);yield return new WaitForSeconds(2.2f);Assert.That(driver.IsPlaying,Is.False);
                toy.TrySetCollectible(ToyCatalog.PeachId);Assert.That(driver.TryPlay(FoxAction.TailWag),Is.False);
                toy.TrySetCollectible(ToyCatalog.FoxId);Assert.That(driver.TryPlay(FoxAction.TailWag),Is.True);
                driver.Cancel();Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
    }
}
