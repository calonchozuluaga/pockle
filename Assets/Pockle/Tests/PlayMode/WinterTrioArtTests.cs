using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class WinterTrioArtTests
    {
        [UnityTest]
        public IEnumerator FifteenWorldsHaveUniquePaintAndReflectionsAndThreeCoversHaveTheRequiredRatio()
        {
            var ambient=RenderSettings.ambientLight;var mode=RenderSettings.ambientMode;bool fog=RenderSettings.fog;
            PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);var stage=camera.transform.parent.gameObject;
            var obj=new GameObject("Art binding test toy");obj.transform.SetParent(mount,false);var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var used=new System.Collections.Generic.HashSet<Texture>();
            try
            {
                foreach(string id in new[]{"reiny","guin","bundy"})
                {
                    ToyCatalog.TryGetCollection(id,out var collection);Assert.That(collection.Worlds.Count,Is.EqualTo(5));
                    var cover=Resources.Load<Texture2D>(collection.CoverArt);Assert.That(cover,Is.Not.Null);
                    Assert.That((float)cover.width/cover.height,Is.EqualTo(3).Within(.001));
                    Assert.That(toy.TrySetCollectible(id+".gloss-vinyl"),Is.True);
                    foreach(var world in collection.Worlds)
                    {
                        var art=Resources.Load<PaintedWorldAsset>(world.EnvironmentArt);
                        Assert.That(art,Is.Not.Null);Assert.That(art.Background,Is.Not.Null);Assert.That(used.Add(art.Background),Is.True,"Each world needs distinct art.");
                        Assert.That(art.Reflections,Is.Not.Null);Assert.That(art.Reflections.width,Is.EqualTo(128));
                        Assert.That(PrototypeStage.SetEnvironment(camera,world.Id,toy.CollectibleId),Is.True);
                        Assert.That(toy.BodyRenderer.sharedMaterial.GetTexture("_StageEnvironment"),Is.SameAs(art.Reflections));
                        Assert.That(PrototypeStage.SetEnvironment(camera,world.Id,ToyCatalog.PeachId),Is.False);
                        yield return null;
                    }
                    toy.TrySetCollectible(id+".glow-jelly");PrototypeStage.SetEnvironment(camera,collection.Worlds[4].Id,toy.CollectibleId);PrototypeStage.SetLight(camera,StageLight.Night);
                    Assert.That(camera.GetComponent<ToyGlowAura>().Strength,Is.GreaterThan(.4));
                }
                Assert.That(used.Count,Is.EqualTo(15));
            }
            finally{Object.Destroy(stage);RenderSettings.ambientLight=ambient;RenderSettings.ambientMode=mode;RenderSettings.fog=fog;}
            yield return null;
        }
        [UnityTest]
        public IEnumerator RealCartonsHaveOpeningHingesBrightPrintAndTransparentProductRenders()
        {
            foreach(string id in new[]{"reiny","guin","bundy"})
            {
                ToyCatalog.TryGetCollection(id,out var collection);var root=new GameObject("Winter native carton");
                try
                {
                    var product=Resources.Load<Texture2D>(collection.BoxArt);var opened=Resources.Load<Texture2D>(collection.BoxArt+"-open");
                    Assert.That(product,Is.Not.Null);Assert.That(opened,Is.Not.Null);Assert.That(product.width,Is.EqualTo(1024));Assert.That(opened.height,Is.EqualTo(1024));
                    foreach(var image in new[]{product,opened})
                    {
                        var rt=RenderTexture.GetTemporary(16,16,0,RenderTextureFormat.ARGB32);var previous=RenderTexture.active;
                        var pixels=new Texture2D(16,16,TextureFormat.RGBA32,false);
                        try
                        {
                            Graphics.Blit(image,rt);RenderTexture.active=rt;pixels.ReadPixels(new Rect(0,0,16,16),0,0);pixels.Apply();
                            Assert.That(pixels.GetPixel(0,0).a,Is.LessThan(.01f),"Product render corners must be transparent.");
                            Assert.That(pixels.GetPixel(8,8).a,Is.GreaterThan(.8f),"Carton itself must remain opaque.");
                        }
                        finally{RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);Object.Destroy(pixels);}
                    }
                    var box=root.AddComponent<MysteryBox>();box.Initialize(id);var native=root.GetComponentInChildren<NativeCollectionBox>();
                    Assert.That(native,Is.Not.Null);Assert.That(native.OpeningClip.length,Is.GreaterThan(1));
                    var hinge=root.GetComponentsInChildren<Transform>().Single(t=>t.name==HolidayCatalog.Title(id)+"Box_LidHinge");
                    box.ApplyPose(RevealSequence.Sample(0,false),false);var closed=hinge.localRotation;
                    box.ApplyPose(RevealSequence.Sample(2,false),false);Assert.That(Quaternion.Angle(closed,hinge.localRotation),Is.GreaterThan(50));
                    box.ApplyPose(RevealSequence.Sample(2,true),true);Assert.That(Quaternion.Angle(closed,hinge.localRotation),Is.LessThanOrEqualTo(21));
                    Assert.That(root.GetComponentsInChildren<Collider>(),Is.Empty);
                    foreach(var renderer in native.GetComponentsInChildren<Renderer>())
                    {
                        Assert.That(renderer.sharedMaterial.shader.name,Is.EqualTo("Pockle/Reveal Box"));
                        if(renderer.sharedMaterial.mainTexture!=null)
                        {
                            var color=renderer.sharedMaterial.color;
                            Assert.That(color.r,Is.EqualTo(1));Assert.That(color.g,Is.EqualTo(1));Assert.That(color.b,Is.EqualTo(1),"Print RGB remains untinted while reveal controls alpha.");
                        }
                    }
                }
                finally{Object.Destroy(root);}
                yield return null;
            }
        }
    }
}
