using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class WallyTests
    {
        [UnityTest]
        public IEnumerator TailFlipperAndTemperamentChangesActuallyRender()
        {
            var root=new GameObject("Native Wally pixels");var toy=root.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(WallyCatalog.ClassicId);
            var view=new GameObject("Wally pixel camera",typeof(Camera));var camera=view.GetComponent<Camera>();camera.enabled=false;
            camera.orthographic=true;camera.orthographicSize=1.5f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.89f,.95f,.97f);
            camera.transform.position=new Vector3(4,1,-5);camera.transform.LookAt(new Vector3(0,-.2f,0));
            try
            {
                var m=toy.BodyRenderer.sharedMaterial;var a=Capture(camera);
                m.SetFloat("_WallyTail",.85f);var b=Capture(camera);Assert.That(Difference(a,b),Is.GreaterThan(40),"Tail articulation must move visible native pixels.");
                m.SetFloat("_WallyTail",0);m.SetFloat("_WallyFlipper",1);b=Capture(camera);Assert.That(Difference(a,b),Is.GreaterThan(30),"The flipper must wave independently of whole-body movement.");
                m.SetFloat("_WallyFlipper",0);camera.transform.position=new Vector3(0,0,-5);camera.transform.LookAt(new Vector3(0,-.2f,0));
                var face=toy.GetComponentInChildren<ToyTemperamentDriver>();face.ApplyFace(PipFaces.Pose(PipTemperament.Sunny,0));a=Capture(camera);
                face.ApplyFace(PipFaces.Pose(PipTemperament.Grumpy,0));b=Capture(camera);Assert.That(Difference(a,b),Is.GreaterThan(80),"Temperament eyes and mouth must visibly change.");
            }
            finally{UnityEngine.Object.Destroy(root);UnityEngine.Object.Destroy(view);}yield return null;
        }
        private static Color32[] Capture(Camera camera)
        {
            var rt=new RenderTexture(384,384,24);rt.Create();var image=new Texture2D(384,384,TextureFormat.RGB24,false);var active=RenderTexture.active;
            try
            {
                camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,384,384),0,0);image.Apply();var pixels=image.GetPixels32();
                Assert.That(pixels.Count(c=>c.r>240&&c.b>240&&c.g<15),Is.LessThan(10),"No shader-error magenta.");return pixels;
            }
            finally{camera.targetTexture=null;RenderTexture.active=active;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(image);}
        }
        private static int Difference(Color32[] a,Color32[] b)
        {
            int count=0;for(int i=0;i<a.Length;i++)if(Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b)>12)count++;return count;
        }
        [UnityTest]
        public IEnumerator TenFinishesEightTemperamentsAndFiveReactionsPreserveTheNativeMesh()
        {
            var root=new GameObject("Wally integration test");var toy=root.AddComponent<JellyToy>();toy.Initialize();
            var driver=root.AddComponent<WallyReactionDriver>();driver.Initialize(toy);
            try
            {
                Assert.That(CharacterArt.TryLoad(WallyCatalog.ClassicId,out var source),Is.True);
                Assert.That(source.BodyMesh.triangles.Length/3,Is.LessThanOrEqualTo(31000));
                Assert.That(source.BodyMesh.vertexCount,Is.LessThan(65000));var original=source.BodyMesh.vertices;
                foreach(var item in ToyCatalog.Collectibles.Where(x=>x.CharacterId=="wally"))
                {
                    Assert.That(toy.TrySetCollectible(item.Id),Is.True);var material=toy.BodyRenderer.sharedMaterial;
                    Assert.That(material.shader.isSupported,Is.True);Assert.That(material.mainTexture,Is.SameAs(source.BaseColorTexture));
                    Assert.That(toy.Handling,Is.SameAs(MaterialHandling.ForFinish(item.FinishId)));
                    var face=toy.GetComponentInChildren<ToyTemperamentDriver>();Assert.That(face,Is.Not.Null);
                    Color pigment=material.GetColor("_BodyColor");float gloss=material.GetFloat("_Gloss");
                    foreach(PipTemperament temperament in Enum.GetValues(typeof(PipTemperament)))
                    {
                        var pose=PipFaces.Pose(temperament,2);face.ApplyFace(pose);
                        Assert.That(material.GetFloat("_TemperFrown"),Is.EqualTo(pose.Frown));
                        Assert.That(material.GetFloat("_TemperLidAngle"),Is.EqualTo(pose.LidAngle));
                        Assert.That(material.GetColor("_BodyColor"),Is.EqualTo(pigment));Assert.That(material.GetFloat("_Gloss"),Is.EqualTo(gloss));
                    }
                    foreach(WallyAction action in Enum.GetValues(typeof(WallyAction)))
                    {
                        Assert.That(driver.TryPlay(action),Is.True);var p=WallyReactions.Sample(action,.35f,false);driver.ApplyPose(p);
                        Assert.That(toy.VisualRoot.parent.localScale,Is.EqualTo(Vector3.one),"Rigid finishes never squash during whale reactions.");
                        Assert.That(material.GetFloat("_WallyTail"),Is.EqualTo(p.Tail));Assert.That(material.GetFloat("_WallyFlipper"),Is.EqualTo(p.Flipper));
                        driver.Cancel();Assert.That(material.GetFloat("_WallyTail"),Is.Zero);Assert.That(material.GetFloat("_WallyFlipper"),Is.Zero);
                        Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));Assert.That(face.IsReacting,Is.False);
                    }
                    CollectionAssert.AreEqual(original,source.BodyMesh.vertices,"Imported source remains immutable.");
                }
                int previous=-1;
                for(int i=0;i<60;i++){driver.PlayRandom();Assert.That(driver.IsPlaying,Is.True);Assert.That((int)driver.CurrentAction,Is.Not.EqualTo(previous));previous=(int)driver.CurrentAction;driver.Cancel();}
                Assert.That(driver.TryPlay((WallyAction)100),Is.False);
                driver.TryPlay(WallyAction.TailSplash);yield return new WaitForSecondsRealtime(WallyReactions.Duration+.2f);Assert.That(driver.IsPlaying,Is.False);
                driver.TryPlay(WallyAction.FlipperWave);toy.TrySetCollectible(ToyCatalog.PeachId);yield return null;
                Assert.That(driver.IsPlaying,Is.False);Assert.That(driver.TryPlay(WallyAction.BreachJump),Is.False);
                toy.TrySetCollectible(WallyCatalog.ClassicId);driver.TryPlay(WallyAction.BreachJump);driver.enabled=false;Assert.That(driver.IsPlaying,Is.False);
            }
            finally{UnityEngine.Object.Destroy(root);}yield return null;
        }
    }
}
