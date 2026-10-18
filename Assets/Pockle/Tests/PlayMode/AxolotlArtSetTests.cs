using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class AxolotlArtSetTests
    {
        [UnityTest]
        public IEnumerator AllFiveAxolotlWorldsBindTheirOwnReflectionMaps()
        {
            Color ambient=RenderSettings.ambientLight;
            PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);
            var root=camera.transform.parent.gameObject;var obj=new GameObject("Axolotl collection environment test");obj.transform.SetParent(mount,false);
            var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(ToyCatalog.AxolotlSpecialId);
            try
            {
                ToyCatalog.TryGetCollection("axolotl",out var collection);
                Assert.That(collection.Worlds.Count,Is.EqualTo(5));
                foreach(var world in collection.Worlds)
                {
                    var art=Resources.Load<PaintedWorldAsset>(world.EnvironmentArt);
                    Assert.That(art,Is.Not.Null);Assert.That(art.Background,Is.Not.Null);Assert.That(art.Reflections.width,Is.EqualTo(128));
                    Assert.That(PrototypeStage.SetEnvironment(camera,world.Id,toy.CollectibleId),Is.True);
                    Assert.That(camera.GetComponent<ToyStageEnvironment>().EnvironmentId,Is.EqualTo(world.Id));
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetTexture("_StageEnvironment"),Is.SameAs(art.Reflections));
                    yield return null;
                }
                Assert.That(PrototypeStage.SetEnvironment(camera,collection.Worlds[0].Id,ToyCatalog.FoxId),Is.False,"Worlds belong to their own collection.");
                var cover=Resources.Load<Texture2D>(collection.CoverArt);Assert.That(cover,Is.Not.Null);
                Assert.That((float)cover.width/cover.height,Is.EqualTo(3).Within(.01));
                Assert.That(Resources.Load<Texture2D>(collection.BoxArt),Is.Not.Null);
            }
            finally {Object.Destroy(root);RenderSettings.ambientLight=ambient;}
            yield return null;
        }
        [UnityTest]
        public IEnumerator NativeAxolotlCartonOpensRealHingesAndSupportsCalmMotion()
        {
            var root=new GameObject("Axolotl opening-ready carton");
            try
            {
                var box=root.AddComponent<MysteryBox>();box.Initialize("axolotl");
                var native=root.GetComponentInChildren<NativeCollectionBox>();Assert.That(native,Is.Not.Null);
                Assert.That(native.OpeningClip.length,Is.GreaterThan(1));
                Assert.That(root.GetComponentsInChildren<Collider>(),Is.Empty);
                var hinge=root.GetComponentsInChildren<Transform>().Single(t=>t.name=="AxolotlBox_LidHinge");
                box.ApplyPose(RevealSequence.Sample(0,false),false);var closed=hinge.localRotation;
                box.ApplyPose(RevealSequence.Sample(2,false),false);Assert.That(Quaternion.Angle(closed,hinge.localRotation),Is.GreaterThan(50));
                box.ApplyPose(RevealSequence.Sample(2,true),true);Assert.That(Quaternion.Angle(closed,hinge.localRotation),Is.LessThanOrEqualTo(21));
                Assert.That(root.GetComponentsInChildren<Renderer>().All(r=>r.sharedMaterials.All(m=>m!=null)),Is.True);
            }
            finally {Object.Destroy(root);}
            yield return null;
        }
    }
}
