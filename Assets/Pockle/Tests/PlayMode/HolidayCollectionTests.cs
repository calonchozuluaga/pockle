using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class HolidayCollectionTests
    {
        [UnityTest]
        public IEnumerator FortyFinishesUseApprovedReadableShapesAndDistinctRecipes()
        {
            var root=new GameObject("Christmas import regression");
            try
            {
                var toy=root.AddComponent<JellyToy>();
                foreach(string id in new[]{"firb","coco","snowy","ginger"})
                {
                    Assert.That(ToyCatalog.TryGetCollection(id,out var collection),Is.True);
                    Assert.That(CharacterArt.TryLoad(id+".matte-vinyl",out var approved),Is.True);
                    Assert.That(approved.BodyMesh.isReadable,Is.True);
                    Assert.That(approved.BodyMesh.triangles.Length/3,Is.EqualTo(30000));
                    Assert.That(approved.BodyMesh.vertexCount,Is.LessThan(65000));
                    Assert.That(approved.BodyMesh.bounds.min.y,Is.EqualTo(-1).Within(.001));
                    Assert.That(collection.Members.Count,Is.EqualTo(10));
                    Assert.That(collection.Members.Count(member=>ToyCatalog.TryGetCollectible(member,out var item)&&item.IsSecret),Is.EqualTo(1));
                    Material previous=null;
                    foreach(string member in collection.Members)
                    {
                        Assert.That(CharacterArt.TryLoad(member,out var art),Is.True);Assert.That(art,Is.SameAs(approved));
                        Assert.That(toy.TrySetCollectible(member),Is.True);
                        var material=toy.BodyRenderer.sharedMaterial;
                        Assert.That(material,Is.Not.SameAs(previous),"A finish swap must replace the imported recipe.");previous=material;
                        Assert.That(material.mainTexture,Is.SameAs(approved.BaseColorTexture));
                        Assert.That(material.shader.isSupported,Is.True);
                        Assert.That(toy.VisualRoot.childCount,Is.EqualTo(1),"No procedural Pip accents may be attached.");
                        Assert.That(toy.VisualRoot.GetComponent<HolidayExpressionDriver>(),Is.Not.Null);
                        Assert.That(ToyCatalog.IsDailyEligible(member),Is.False);
                        yield return null;
                    }
                    Assert.That(new CollectionProgress(null,CollectionSession.Today).GetOwnedCount(id+".matte-vinyl"),Is.Zero);
                }
            }
            finally{Object.Destroy(root);}
            yield return null;
        }
        [UnityTest]
        public IEnumerator TwentyWorldsCoversAndFourOpeningCartonsAreBound()
        {
            Color ambient=RenderSettings.ambientLight;
            PrototypeStage.Create(out var camera,out _,out _,out _,out _);
            var stage=camera.transform.parent.gameObject;
            try
            {
                foreach(string id in new[]{"firb","coco","snowy","ginger"})
                {
                    ToyCatalog.TryGetCollection(id,out var collection);
                    Assert.That(collection.Worlds.Count,Is.EqualTo(5));
                    foreach(var world in collection.Worlds)
                    {
                        var art=Resources.Load<PaintedWorldAsset>(world.EnvironmentArt);
                        Assert.That(art,Is.Not.Null);Assert.That(art.Background,Is.Not.Null);Assert.That(art.Reflections.width,Is.EqualTo(128));
                        Assert.That(PrototypeStage.SetEnvironment(camera,world.Id,id+".glow-jelly"),Is.True);
                        Assert.That(PrototypeStage.SetEnvironment(camera,world.Id,ToyCatalog.PeachId),Is.False);
                    }
                    var cover=Resources.Load<Texture2D>(collection.CoverArt);
                    Assert.That(cover,Is.Not.Null);Assert.That((float)cover.width/cover.height,Is.EqualTo(3).Within(.001));
                    Assert.That(Resources.Load<Texture2D>(collection.BoxArt),Is.Not.Null);
                    var boxRoot=new GameObject("Christmas native box");
                    try
                    {
                        var box=boxRoot.AddComponent<MysteryBox>();box.Initialize(id);
                        var native=boxRoot.GetComponentInChildren<NativeCollectionBox>();Assert.That(native,Is.Not.Null);Assert.That(native.OpeningClip.length,Is.GreaterThan(1));
                        Assert.That(boxRoot.GetComponentsInChildren<Collider>(),Is.Empty);
                        var hinge=boxRoot.GetComponentsInChildren<Transform>().Single(t=>t.name==HolidayCatalog.Title(id)+"Box_LidHinge");
                        box.ApplyPose(RevealSequence.Sample(0,false),false);var closed=hinge.localRotation;
                        box.ApplyPose(RevealSequence.Sample(2,false),false);Assert.That(Quaternion.Angle(closed,hinge.localRotation),Is.GreaterThan(50));
                        box.ApplyPose(RevealSequence.Sample(2,true),true);Assert.That(Quaternion.Angle(closed,hinge.localRotation),Is.LessThanOrEqualTo(21));
                    }
                    finally{Object.Destroy(boxRoot);}
                    yield return null;
                }
            }
            finally{Object.Destroy(stage);RenderSettings.ambientLight=ambient;}
            yield return null;
        }
        [UnityTest]
        public IEnumerator EventExpressionsReleaseAndCalmNeutralizesEveryCharacter()
        {
            const string key="pockle.prototype.reducedMotion";bool had=PlayerPrefs.HasKey(key);int old=PlayerPrefs.GetInt(key);
            var root=new GameObject("Christmas expression events");
            try
            {
                var toy=root.AddComponent<JellyToy>();
                foreach(string id in new[]{"firb","coco","snowy","ginger"})
                {
                    PlayerPrefs.SetInt(key,0);Assert.That(toy.TrySetCollectible(id+".matte-vinyl"),Is.True);
                    var driver=toy.VisualRoot.GetComponent<HolidayExpressionDriver>();driver.React(new PeachExpressionPose(1,1,1),.1f);
                    yield return null;yield return null;
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Blink"),Is.EqualTo(1));
                    yield return new WaitForSecondsRealtime(.15f);yield return null;
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Happy"),Is.Zero);
                    driver.React(new PeachExpressionPose(1,1,1),1);PlayerPrefs.SetInt(key,1);yield return null;yield return null;
                    foreach(string property in new[]{"_Blink","_Happy","_Surprise","_IdleWave"})Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat(property),Is.Zero);
                }
            }
            finally{Object.Destroy(root);if(had)PlayerPrefs.SetInt(key,old);else PlayerPrefs.DeleteKey(key);}
            yield return null;
        }
    }
}
