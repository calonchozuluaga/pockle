using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class WinterTrioFinishTests
    {
        private static readonly string[] Finishes = { "matte-vinyl", "clear-jelly", "pearl-jelly", "confetti-gel", "mochi-foam", "velvet-flock", "boucle-plush", "gloss-vinyl", "coated-metallic", "glow-jelly" };
        [UnityTest]
        public IEnumerator EveryWinterFinishReadsDifferentlyInNativeRendering()
        {
            var root = new GameObject("Winter finish rendering test");
            var cameraObject = new GameObject("Finish camera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>(); camera.enabled=false; camera.orthographic=true; camera.orthographicSize=1.18f;
            camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.93f,.92f,.94f);
            camera.transform.position=new Vector3(0,0,-5); camera.transform.LookAt(Vector3.zero);
            var toy=root.AddComponent<JellyToy>(); toy.Initialize();
            try
            {
                foreach(string character in new[]{"reiny","guin","bundy"})
                {
                    var images=new List<Color32[]>();
                    foreach(string finish in Finishes)
                    {
                        Assert.That(toy.TrySetCollectible(character+"."+finish),Is.True);
                        var material=toy.BodyRenderer.sharedMaterial;
                        Assert.That(material.shader.isSupported,Is.True);
                        toy.GetComponentInChildren<ToyTemperamentDriver>().ApplyFace(Pockle.Core.PipFaces.Pose(Pockle.Core.PipTemperament.Sunny,0));
                        var pixels=Render(camera);
                        for(int i=0;i<images.Count;i++) Assert.That(Difference(pixels,images[i]),Is.GreaterThan(.009f),character+" "+finish+" must visibly differ from "+Finishes[i]);
                        images.Add(pixels);
                        yield return null;
                    }
                    // Isolate texture response from color: these fabrics must differ even in one pigment.
                    var surfaces=new List<Color32[]>();
                    foreach(string finish in new[]{"gloss-vinyl","mochi-foam","velvet-flock","boucle-plush"})
                    {
                        toy.TrySetCollectible(character+"."+finish);
                        toy.BodyRenderer.sharedMaterial.SetColor("_BodyColor",new Color(.4f,.65f,.7f));
                        var pixels=Render(camera);
                        for(int i=0;i<surfaces.Count;i++) Assert.That(Difference(pixels,surfaces[i]),Is.GreaterThan(.004f),character+" "+finish+" must differ independently of pigment from surface "+i);
                        surfaces.Add(pixels);
                    }
                }
                // Shared shader changes are opt-in; older uploaded collections retain their prior palette.
                toy.TrySetCollectible("firb.velvet-flock");Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_WinterPalette"),Is.Zero);
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_SurfaceRelief"),Is.Zero);
            }
            finally { Object.Destroy(root);Object.Destroy(cameraObject); }
            yield return null;
        }
        private static Color32[] Render(Camera camera)
        {
            var target=new RenderTexture(192,192,24);target.Create();var pixels=new Texture2D(192,192,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try { camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,192,192),0,0);pixels.Apply();return pixels.GetPixels32(); }
            finally { camera.targetTexture=null;RenderTexture.active=previous;target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(pixels); }
        }
        private static float Difference(Color32[] a,Color32[] b)
        {
            long total=0;for(int i=0;i<a.Length;i++)total+=System.Math.Abs(a[i].r-b[i].r)+System.Math.Abs(a[i].g-b[i].g)+System.Math.Abs(a[i].b-b[i].b);
            return total/(a.Length*3f*255);
        }
    }
}
