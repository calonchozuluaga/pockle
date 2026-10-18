using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class WinterTrioTests
    {
        [UnityTest]
        public IEnumerator ThirtyFinishesUseUploadedArtAndHaveFiveRandomReactionsAndEightFaces()
        {
            var obj=new GameObject("Winter trio integration test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var driver=obj.AddComponent<WinterTrioReactionDriver>();driver.Initialize(toy);
            try
            {
                foreach(string id in new[]{"reiny","guin","bundy"})
                {
                    Assert.That(CharacterArt.TryLoad(id+".matte-vinyl",out var asset),Is.True);
                    var source=asset.BodyMesh.vertices;
                    Assert.That(asset.BodyMesh.triangles.Length/3,Is.LessThanOrEqualTo(30000));
                    Assert.That(asset.BodyMesh.isReadable,Is.True);Assert.That(asset.BodyMesh.bounds.min.y,Is.EqualTo(-1).Within(.001f));
                    Assert.That(asset.BodyMesh.bounds.size.x,Is.LessThanOrEqualTo(2.051f));
                    foreach(var item in ToyCatalog.Collectibles.Where(t=>t.CharacterId==id))
                    {
                        Assert.That(toy.TrySetCollectible(item.Id),Is.True);
                        var material=toy.BodyRenderer.sharedMaterial;
                        Assert.That(material.mainTexture,Is.SameAs(asset.BaseColorTexture));Assert.That(material.shader.isSupported,Is.True);
                        Assert.That(toy.Handling==MaterialHandling.Firm,Is.EqualTo(item.FinishId=="matte-vinyl"||item.FinishId=="gloss-vinyl"||item.FinishId=="coated-metallic"));
                        var face=toy.GetComponentInChildren<ToyTemperamentDriver>();Assert.That(face,Is.Not.Null);
                        foreach(WinterTrioAction action in System.Enum.GetValues(typeof(WinterTrioAction)))
                        {
                            Assert.That(driver.TryPlay(action),Is.True);driver.ApplyPose(WinterTrioReactions.Sample(id,action,.38f,false));
                            Assert.That(toy.VisualRoot.parent.localScale,Is.EqualTo(Vector3.one),"Rigid finishes never get whole-body squashing.");
                            driver.Cancel();Assert.That(driver.IsPlaying,Is.False);
                            Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));Assert.That(material.GetFloat("_WinterFeature"),Is.Zero);
                        }
                        yield return null;
                    }
                    int previous=-1;
                    for(int i=0;i<60;i++){driver.PlayRandom();Assert.That(driver.IsPlaying,Is.True);Assert.That((int)driver.CurrentAction,Is.Not.EqualTo(previous));previous=(int)driver.CurrentAction;driver.Cancel();}
                    CollectionAssert.AreEqual(source,asset.BodyMesh.vertices,"Original native geometry remains immutable.");
                    driver.TryPlay(WinterTrioAction.Jump);yield return new WaitForSecondsRealtime(2.2f);yield return null;Assert.That(driver.IsPlaying,Is.False);
                    driver.TryPlay(WinterTrioAction.FeatureWiggle);obj.SetActive(false);Assert.That(driver.IsPlaying,Is.False);obj.SetActive(true);
                }
                toy.TrySetCollectible(ToyCatalog.PeachId);Assert.That(driver.TryPlay(WinterTrioAction.Jump),Is.False);
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
        [UnityTest]
        public IEnumerator ReviewGrantPersistsBeforeMarkerAndDoesNotDuplicateOnRestart()
        {
            const string saveKey="pockle.collection.v1";
            bool hadSave=PlayerPrefs.HasKey(saveKey),hadMarker=PlayerPrefs.HasKey(CollectionSession.WinterTrioGrantKey);
            string oldSave=PlayerPrefs.GetString(saveKey);int oldMarker=PlayerPrefs.GetInt(CollectionSession.WinterTrioGrantKey);
            var root=new GameObject("Winter review grant");GameObject restarted=null;
            try
            {
                var save=new CollectionProgress(null,CollectionSession.Today).Save;save.Steps=571;save.PendingRevealId=ToyCatalog.MoonId;
                PlayerPrefs.SetString(saveKey,JsonUtility.ToJson(save));PlayerPrefs.DeleteKey(CollectionSession.WinterTrioGrantKey);
                var session=root.AddComponent<CollectionSession>();session.Initialize();
                Assert.That(session.TryApplyOwnerWinterTrioGrant(),Is.True);
                Assert.That(PlayerPrefs.GetInt(CollectionSession.WinterTrioGrantKey),Is.EqualTo(1));
                var persisted=JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(saveKey));
                Assert.That(persisted.Inventory.Count(i=>i.Count==1&&ToyCatalog.TryGetCollectible(i.Id,out var item)&&HolidayCatalog.IsWinterTrio(item.CharacterId)),Is.EqualTo(30));
                Object.DestroyImmediate(root);root=null;
                restarted=new GameObject("Restarted winter grant");session=restarted.AddComponent<CollectionSession>();session.Initialize();
                Assert.That(session.TryApplyOwnerWinterTrioGrant(),Is.False);
                foreach(var item in ToyCatalog.Collectibles.Where(t=>HolidayCatalog.IsWinterTrio(t.CharacterId)))Assert.That(session.GetOwnedCount(item.Id),Is.EqualTo(1));
                Assert.That(session.Progress.Save.Steps,Is.EqualTo(571));Assert.That(session.PendingRevealId,Is.EqualTo(ToyCatalog.MoonId));
            }
            finally
            {
                if(root!=null)Object.DestroyImmediate(root);if(restarted!=null)Object.DestroyImmediate(restarted);
                if(hadSave)PlayerPrefs.SetString(saveKey,oldSave);else PlayerPrefs.DeleteKey(saveKey);
                if(hadMarker)PlayerPrefs.SetInt(CollectionSession.WinterTrioGrantKey,oldMarker);else PlayerPrefs.DeleteKey(CollectionSession.WinterTrioGrantKey);
                PlayerPrefs.Save();
            }
            yield return null;
        }
        [Test]
        public void TrioWorldAndPackagingMetadataNamesItsOwnArt()
        {
            foreach(string id in new[]{"reiny","guin","bundy"})
            {
                ToyCatalog.TryGetCollection(id,out var collection);
                Assert.That(collection.Worlds.Count,Is.EqualTo(5));Assert.That(collection.CoverArt,Is.EqualTo("Collections/"+HolidayCatalog.Title(id)+"/Cover"));Assert.That(collection.BoxModel,Is.EqualTo("Boxes/"+HolidayCatalog.Title(id)+"CollectionBox"));
                Assert.That(collection.Members.Count(m=>ToyCatalog.TryGetCollectible(m,out var toy)&&toy.IsSecret),Is.EqualTo(1));
            }
        }
    }
}
