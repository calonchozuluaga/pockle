using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class FreeToyViewTests
    {
        private static void Call(object obj,string method,params object[] args) => obj.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(obj,args);
        [UnityTest]
        public IEnumerator OrbitTurnsAboutBodyCentreAndHaloStaysBehindIt()
        {
            PrototypeStage.Create(out var camera,out var mount,out var pivot,out var plate,out var shadow);
            var root=camera.transform.parent.gameObject;
            var body=new GameObject("Orbit test body");body.transform.SetParent(mount,false);
            var toy=body.AddComponent<JellyToy>();toy.Initialize();
            var input=root.AddComponent<ToyViewGesture>();int presses=0,resets=0;
            input.Initialize(camera,pivot,toy,p=>presses+=p?1:-1,()=>{resets++;input.ResetView();});
            try
            {
                toy.TrySetCollectible(ToyCatalog.PipSpecialId);
                PrototypeStage.SetWorld(camera,StageWorld.Midnight,toy.CollectibleId);PrototypeStage.SetLight(camera,StageLight.Night);
                Assert.That(plate,Is.Null);var rest=mount.position;var ground=shadow.position;
                foreach(float yaw in new[]{0,90,180,270,360})
                {
                    input.Orbit.Reset();input.Orbit.Drag(yaw,135,.016f);input.Orbit.Stop();
                    yield return null;yield return null;
                    Assert.That(Vector3.Distance(mount.position,rest),Is.LessThan(.001f),"Toy centre must not orbit the floor.");
                    Assert.That(shadow.position,Is.EqualTo(ground));
                    var halo=mount.Find("Toy emitted halo");
                    Vector3 offset=camera.transform.InverseTransformVector(halo.position-mount.position);
                    Assert.That(offset.x,Is.EqualTo(0).Within(.001f));Assert.That(offset.y,Is.EqualTo(0).Within(.001f));
                    Assert.That(offset.z,Is.EqualTo(.85f).Within(.001f));
                    Assert.That(Quaternion.Angle(halo.rotation,camera.transform.rotation),Is.LessThan(.01f));
                }
                input.Calm=true;float before=input.Orbit.Yaw;yield return null;Assert.That(input.Orbit.Yaw,Is.EqualTo(before));
                input.ResetView();yield return null;
                var centre=(Vector2)camera.WorldToScreenPoint(mount.position);
                Call(input,"Begin",-1,centre);
                typeof(ToyViewGesture).GetField("began",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(input,Time.unscaledTime-1);
                Call(input,"Move",centre);Assert.That(presses,Is.EqualTo(1),"Stationary hold must press.");
                input.Cancel();Assert.That(presses,Is.Zero,"Cancellation releases compression.");
                Call(input,"Begin",-1,centre);Call(input,"End",true);Call(input,"Begin",-1,centre);Call(input,"End",true);
                Assert.That(resets,Is.EqualTo(1),"Double tap resets.");
            }
            finally{Object.Destroy(root);}
            yield return null;
        }
        [UnityTest]
        public IEnumerator ViewportEasesAndBackdropIsBoundedAndStopsInCalm()
        {
            PrototypeStage.Create(out var camera,out _,out _,out _,out _);
            var root=camera.transform.parent.gameObject;
            try
            {
                PrototypeStage.SetWorld(camera,StageWorld.Garden,ToyCatalog.PeachId);
                yield return null;
                var env=camera.GetComponent<ToyStageEnvironment>();
                var material=root.transform.Find("Full-screen background/World gradient").GetComponent<Renderer>().sharedMaterial;
                env.SetViewAngles(180,90);yield return new WaitForSeconds(.3f);
                float pan=material.GetFloat("_Pan"),crop=material.GetFloat("_Crop");
                Assert.That(pan,Is.GreaterThan(.5f));Assert.That(pan-crop/2,Is.GreaterThanOrEqualTo(0));Assert.That(pan+crop/2,Is.LessThanOrEqualTo(1));
                env.Calm=true;env.SetViewAngles(-1000,-1000);yield return new WaitForSeconds(.15f);
                Assert.That(material.GetFloat("_Pan"),Is.EqualTo(pan));
                Rect requested=new Rect(Screen.width*.1f,Screen.height*.3f,Screen.width*.8f,Screen.height*.4f);
                Vector3 previous=camera.WorldToScreenPoint(new Vector3(0,1.15f,0));PrototypeStage.SetViewport(camera,requested);yield return null;
                Assert.That(camera.WorldToScreenPoint(new Vector3(0,1.15f,0)),Is.Not.EqualTo(previous));
                Assert.That(camera.rect,Is.EqualTo(new Rect(0,0,1,1)));
                yield return new WaitForSeconds(.5f);
                var centre=camera.WorldToScreenPoint(new Vector3(0,1.15f,0));
                Assert.That(centre.x,Is.EqualTo(requested.center.x).Within(2));Assert.That(centre.y,Is.EqualTo(requested.center.y).Within(2));
                Assert.That(camera.rect,Is.EqualTo(new Rect(0,0,1,1)));
                PrototypeStage.SetZoom(camera,.8f);yield return null;Assert.That(camera.orthographicSize,Is.GreaterThan(0));
            }
            finally{Object.Destroy(root);}
            yield return null;
        }
    }
}
