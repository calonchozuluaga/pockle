using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class GingerReactionTests
    {
        [UnityTest]
        public IEnumerator AllTenFinishesHavePawEarHeadAndFacialReactionsAndReturnToRest()
        {
            var obj=new GameObject("Ginger reaction test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var driver=obj.AddComponent<GingerReactionDriver>();driver.Initialize(toy);
            var asset=Resources.Load<PipCharacterAsset>("Characters/Ginger/Ginger");var source=(Vector3[])asset.BodyMesh.vertices.Clone();
            try
            {
                foreach(var item in ToyCatalog.Collectibles.Where(x=>x.CharacterId=="ginger"))
                {
                    Assert.That(toy.TrySetCollectible(item.Id),Is.True);
                    var material=toy.BodyRenderer.sharedMaterial;
                    foreach(GingerAction action in System.Enum.GetValues(typeof(GingerAction)))
                    {
                        Assert.That(driver.TryPlay(action),Is.True);var pose=GingerReactions.Sample(action,.4f,false);driver.ApplyPose(pose);
                        Assert.That(toy.VisualRoot.parent.localScale,Is.EqualTo(Vector3.one),"No whole-body squash on vinyl reactions.");
                        Assert.That(material.GetFloat("_GingerPaw"),Is.EqualTo(pose.Paw));Assert.That(material.GetFloat("_GingerEars"),Is.EqualTo(pose.Ears));
                        Assert.That(material.GetFloat("_GingerHead"),Is.EqualTo(pose.Head));Assert.That(material.GetFloat("_Happy"),Is.EqualTo(pose.Face.Happy));
                        driver.Cancel();Assert.That(material.GetFloat("_GingerPaw"),Is.Zero);Assert.That(material.GetFloat("_GingerEars"),Is.Zero);Assert.That(material.GetFloat("_GingerHead"),Is.Zero);
                        Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));
                    }
                }
                CollectionAssert.AreEqual(source,asset.BodyMesh.vertices,"Source mesh stays immutable.");
                int previous=-1;
                for(int i=0;i<100;i++){driver.PlayRandom();Assert.That(driver.IsPlaying,Is.True);Assert.That((int)driver.CurrentAction,Is.Not.EqualTo(previous));previous=(int)driver.CurrentAction;driver.Cancel();}
                driver.TryPlay(GingerAction.CookieWave);yield return new WaitForSeconds(2.2f);Assert.That(driver.IsPlaying,Is.False);
                foreach(string other in new[]{ToyCatalog.FoxId,"coco.matte-vinyl","snowy.matte-vinyl","firb.matte-vinyl"})
                {toy.TrySetCollectible(other);Assert.That(driver.TryPlay(GingerAction.CookieWave),Is.False);}
                toy.TrySetCollectible("ginger.matte-vinyl");Assert.That(driver.TryPlay(GingerAction.SweetBow),Is.True);driver.Cancel();
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
    }
}
