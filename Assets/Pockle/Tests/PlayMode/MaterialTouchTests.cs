using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
 public sealed class MaterialTouchTests
 {
  static void Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,a);
  static void Set(object o,string n,object v)=>o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,v);
  [UnityTest] public IEnumerator HeldStrokesRetainOrbitAndPairArbitration()
  {
   PrototypeStage.Create(out var camera,out var mount,out var pivot,out _,out _);var root=camera.transform.parent.gameObject;
   var obj=new GameObject("Material touch test");obj.transform.SetParent(mount,false);var toy=obj.AddComponent<JellyToy>();toy.Initialize();
   var g=root.AddComponent<ToyViewGesture>();g.Initialize(camera,pivot,toy,_=>{},()=>{});int starts=0,strokes=0,cancel=0;
   g.SurfaceStarted=()=>starts++;g.SurfaceStroked=(_,__)=>strokes++;g.SurfaceEnded=c=>{if(!c)cancel++;};
   try {
    foreach(string id in new[]{"pip.gloss-vinyl",ToyCatalog.PeachId,"axolotl.boucle-plush","fox.velvet-flock"}) {
     g.Cancel();g.ResetView();Assert.IsTrue(toy.TrySetCollectible(id));yield return null;
     Vector2 p=camera.WorldToScreenPoint(toy.VisualRoot.TransformPoint(new Vector3(0,-.15f,-.6f)));
     Call(g,"Begin",-1,p);Set(g,"began",Time.unscaledTime-.5f);Call(g,"Move",p);int before=starts;
     Call(g,"Move",p+new Vector2(60,15));Assert.AreEqual(before+1,starts,id);Assert.AreEqual(0,g.Orbit.Yaw);Assert.AreEqual(0,g.Orbit.Pitch);Assert.IsTrue(g.IsHolding);
     before=cancel;Call(g,"BeginPair",p,p+Vector2.right*120,2);Assert.AreEqual(before+1,cancel);g.Cancel();
     g.ResetView();Call(g,"Begin",-1,p);Call(g,"Move",p+Vector2.right*60);Assert.That(Math.Abs(g.Orbit.Yaw),Is.GreaterThan(0));g.Cancel();
    }
    Assert.AreEqual(4,strokes);
   }finally{UnityEngine.Object.Destroy(root);}yield return null;
  }
  [UnityTest] public IEnumerator SurfaceResponsesChangeNativePixels()
  {
   PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);var root=camera.transform.parent.gameObject;
   var obj=new GameObject("Native material touch");obj.transform.SetParent(mount,false);var toy=obj.AddComponent<JellyToy>();toy.Initialize();
   try {
    foreach(string id in new[]{ToyCatalog.MoonId,"axolotl.clear-jelly","firb.clear-jelly","pip.boucle-plush","axolotl.boucle-plush","guin.velvet-flock","fox.velvet-flock"}) {
     Assert.IsTrue(toy.TrySetCollectible(id));toy.SetDeformation(0,0,0,0,0,0);yield return null;
     var m=toy.BodyRenderer.sharedMaterial;Assert.IsTrue(m.shader.isSupported);var original=m.name;
     var p=camera.WorldToScreenPoint(toy.VisualRoot.TransformPoint(new Vector3(0,-.3f,-.6f)));Assert.IsTrue(toy.RaycastBody(camera.ScreenPointToRay(p),out var hit),id);
     Vector3 local=toy.BodyRenderer.transform.InverseTransformPoint(hit),from=local-Vector3.right*.4f,to=local+Vector3.right*.4f;
     toy.SetSurfacePlay(0,from,to,Vector3.right,0);var a=Capture(camera,toy.VisualRoot.position);
     toy.SetSurfacePlay(1.15f,from,to,Vector3.right,1);var b=Capture(camera,toy.VisualRoot.position);
     Assert.AreEqual(original,m.name);Assert.AreEqual((float)MaterialTouch.ForToy(id),m.GetFloat("_SurfaceKind"));
     int changed=0;for(int i=0;i<a.Length;i++)if(Math.Abs(a[i].r-b[i].r)+Math.Abs(a[i].g-b[i].g)+Math.Abs(a[i].b-b[i].b)>8)changed++;
     Assert.That(changed,Is.GreaterThan(25),id+" response must actually render.");Save(b,id.Replace('.','-'));
     toy.SetSurfacePlay(0,Vector3.zero,Vector3.zero,Vector3.up,0);Assert.AreEqual(0,m.GetFloat("_BrushAmount"));
    }
   }finally{UnityEngine.Object.Destroy(root);}yield return null;
  }
  [UnityTest] public IEnumerator RigidRockMovesTheWholeToyWithoutBendingItsMesh()
  {
   var root=new GameObject("Rigid rocking test");var toy=root.AddComponent<JellyToy>();toy.Initialize();
   try {
    toy.TrySetCollectible("pip.gloss-vinyl");toy.SetDeformation(0,0,0,0,0,0);
    var mesh=toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;var original=mesh.vertices;
    toy.SetSurfaceRock(.8f,-.5f);toy.SetDeformation(0,0,0,0,0,0);
    CollectionAssert.AreEqual(original,mesh.vertices,"Wobble rotates the rigid body, not its vertex shape.");
    Assert.That(Quaternion.Angle(Quaternion.identity,toy.VisualRoot.localRotation),Is.GreaterThan(1));
    toy.SetSurfaceRock(0,0);toy.SetDeformation(0,0,0,0,0,0);Assert.That(Quaternion.Angle(Quaternion.identity,toy.VisualRoot.localRotation),Is.LessThan(.01f));
   }finally {UnityEngine.Object.Destroy(root);}yield return null;
  }
  static Color32[] Capture(Camera camera,Vector3 centre)
  {
   var target=new RenderTexture(384,512,24);var old=camera.targetTexture;var active=RenderTexture.active;var image=new Texture2D(384,512,TextureFormat.RGB24,false);
   var oldRect=camera.rect;float oldSize=camera.orthographicSize,oldAspect=camera.aspect;var oldProjection=camera.projectionMatrix;
   var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;var oldBackground=camera.backgroundColor;var oldClear=camera.clearFlags;
   try {target.Create();camera.targetTexture=target;camera.rect=new Rect(0,0,1,1);camera.orthographicSize=1.45f;camera.aspect=.75f;camera.ResetProjectionMatrix();
    camera.transform.position=centre+new Vector3(0,.1f,-5);camera.transform.LookAt(centre);camera.backgroundColor=new Color(.95f,.93f,.91f);camera.clearFlags=CameraClearFlags.SolidColor;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,384,512),0,0);image.Apply();var pixels=image.GetPixels32();Assert.That(pixels.Count(c=>c.r>240&&c.b>240&&c.g<15),Is.LessThan(20),"Shader error magenta");return pixels;}
   finally{camera.targetTexture=old;camera.rect=oldRect;camera.orthographicSize=oldSize;camera.aspect=oldAspect;camera.transform.SetPositionAndRotation(oldPosition,oldRotation);camera.backgroundColor=oldBackground;camera.clearFlags=oldClear;camera.projectionMatrix=oldProjection;RenderTexture.active=active;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);}
  }
  static void Save(Color32[] pixels,string name)
  {
   var image=new Texture2D(384,512,TextureFormat.RGB24,false);image.SetPixels32(pixels);image.Apply();
   string dir=System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath),"test-results","material-touch");System.IO.Directory.CreateDirectory(dir);System.IO.File.WriteAllBytes(System.IO.Path.Combine(dir,name+".png"),image.EncodeToPNG());UnityEngine.Object.Destroy(image);
  }
 }
}
