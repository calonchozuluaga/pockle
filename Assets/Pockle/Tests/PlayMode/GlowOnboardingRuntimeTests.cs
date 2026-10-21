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
    public sealed class GlowOnboardingRuntimeTests
    {
        private static object Field(object target,string name) => target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        private static void Set(object target,string name,object value) => target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
        private static void Call(object target,string name,params object[] args) => target.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(target,args);

        [UnityTest]
        public IEnumerator EveryGlowBodyEmitsWithoutTheHaloAndHaloFollowsRotatedBounds()
        {
            var ambient=RenderSettings.ambientLight; var mode=RenderSettings.ambientMode; bool fog=RenderSettings.fog;
            PrototypeStage.Create(out var camera,out var mount,out _,out _,out _); var root=camera.transform.parent.gameObject;
            var obj=new GameObject("Glow body test");obj.transform.SetParent(mount,false);var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            try
            {
                foreach(var character in ToyCatalog.Characters)
                {
                    string id=character.Id+".glow-jelly";
                    Assert.IsTrue(toy.TrySetCollectible(id),id);
                    PrototypeStage.SetWorld(camera,StageWorld.Studio,id);PrototypeStage.SetLight(camera,StageLight.Night);
                    yield return null;
                    var aura=camera.GetComponent<ToyGlowAura>();var halo=mount.GetComponentsInChildren<Transform>().Single(t=>t.name=="Toy emitted halo");
                    Assert.That(aura.Strength,Is.GreaterThan(.4f),id);
                    foreach(float angle in new[]{0f,90f,180f,270f})
                    {
                        toy.transform.localRotation=Quaternion.Euler(0,angle,0);toy.transform.localPosition=new Vector3(.23f,.12f,-.08f);
                        yield return null;
                        var bounds=toy.SurfaceBounds;
                        Assert.That(bounds.size.magnitude,Is.LessThan(toy.BodyRenderer.bounds.size.magnitude*.65f),"effect bounds must not use the inflated culling cube");
                        var offset=halo.position-bounds.center;
                        Assert.That(Vector3.ProjectOnPlane(offset,camera.transform.forward).magnitude,Is.LessThan(.002f),id+" centred on visible body");
                        var f=camera.transform.forward;var e=bounds.extents;
                        float depth=Vector3.Dot(new Vector3(Mathf.Abs(f.x),Mathf.Abs(f.y),Mathf.Abs(f.z)),e);
                        Assert.That(Vector3.Dot(offset,f),Is.GreaterThan(depth),id+" halo behind whole shell");
                        Assert.That(halo.GetComponent<Renderer>().sharedMaterial.color.a,Is.LessThan(aura.Strength*.3f));
                    }
                    toy.transform.localRotation=Quaternion.identity;toy.transform.localPosition=Vector3.zero;
                    yield return null;
                    aura.enabled=false;halo.gameObject.SetActive(false);
                    var pool=root.GetComponentsInChildren<Transform>().Single(t=>t.name=="Toy emitted ground light");pool.gameObject.SetActive(false);
                    var material=toy.BodyRenderer.sharedMaterial;Assert.IsTrue(material.shader.isSupported,id);
                    float emission=material.GetFloat("_StageEmission");Assert.That(emission,Is.GreaterThan(0));
                    material.SetFloat("_StageEmission",0);var dark=Capture(camera);
                    material.SetFloat("_StageEmission",emission);var lit=Capture(camera);
                    int brighter=0;for(int i=0;i<dark.Length;i++)if(lit[i].r+lit[i].g+lit[i].b>dark[i].r+dark[i].g+dark[i].b+12)brighter++;
                    Assert.That(brighter,Is.GreaterThan(40),id+" actual shell emits with all aura cards hidden");
                    aura.enabled=true;
                    toy.TrySetCollectible(character.Id=="pip"?ToyCatalog.PeachId:character.Id+".gloss-vinyl");
                    PrototypeStage.SetWorld(camera,StageWorld.Studio,toy.CollectibleId);PrototypeStage.SetLight(camera,StageLight.Night);
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_StageEmission"),Is.Zero,"ordinary finish must not emit");
                }
            }
            finally { UnityEngine.Object.Destroy(root);RenderSettings.ambientLight=ambient;RenderSettings.ambientMode=mode;RenderSettings.fog=fog; }
            yield return null;
        }

        private static Color32[] Capture(Camera camera)
        {
            var target=new RenderTexture(256,256,24);target.Create();var image=new Texture2D(256,256,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try {camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();return image.GetPixels32();}
            finally {camera.targetTexture=null;RenderTexture.active=previous;target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);}
        }

        [UnityTest]
        public IEnumerator OnboardingCancelsAHeldGestureAndQueuedTapBeforeToyInputAndResumesCleanly()
        {
            string[] keys={"pockle.collection.v1","pockle.onboarding.v1.completed","pockle.onboarding.v1.ageConfirmed","pockle.onboarding.v1.ageLocked"};
            var had=keys.Select(PlayerPrefs.HasKey).ToArray();string prior=PlayerPrefs.GetString(keys[0]);var ints=keys.Skip(1).Select(k=>PlayerPrefs.GetInt(k)).ToArray();
            var ambient=RenderSettings.ambientLight;var mode=RenderSettings.ambientMode;bool fog=RenderSettings.fog;
            int fps=Application.targetFrameRate,vsync=QualitySettings.vSyncCount;
            var root=new GameObject("Runtime onboarding guard");
            try
            {
                PlayerPrefs.DeleteKey(keys[0]);OnboardingState.Complete();
                var runtime=root.AddComponent<TactilePrototype>();yield return null;yield return null;
                var hud=(PrototypeHud)Field(runtime,"hud");Assert.IsTrue(hud.ShowToy(ToyCatalog.PeachId));yield return null;
                var session=root.GetComponent<CollectionSession>();var before=session.OwnedCounts.ToArray();
                var gesture=root.GetComponent<ToyViewGesture>();Call(runtime,"SyncInputSuspension");Assert.IsFalse(runtime.ToyInputBlocked);
                Set(gesture,"holding",true);Set(gesture,"first",-1);Set(gesture,"pendingTap",Time.unscaledTime);
                gesture.Orbit.Drag(20,6,.02f);Assert.IsTrue(gesture.IsHolding);
                Call(hud,"ShowTutorialAgain");Assert.IsTrue(hud.OnboardingVisible);Assert.IsFalse(hud.StoreVisible,"overlay over the Play page isolates this new guard");
                int cues=0;runtime.ToyReactionStarted+=(cue,point)=>cues++;
                Call(runtime,"SyncInputSuspension");Assert.IsTrue(runtime.ToyInputBlocked);Assert.IsTrue(gesture.Suspended);Assert.IsFalse(gesture.IsHolding);
                Assert.That((float)Field(gesture,"pendingTap"),Is.LessThan(0));Assert.That(gesture.Orbit.YawSpeed,Is.Zero);
                var yaw=gesture.Orbit.Yaw;Call(runtime,"ReadInput");Call(runtime,"PlayTapReaction");gesture.ReadInput();
                yield return new WaitForSecondsRealtime(.35f);
                Assert.That(cues,Is.Zero);Assert.That(gesture.Orbit.Yaw,Is.EqualTo(yaw));
                Call(hud,"FinishOnboarding");Call(runtime,"SyncInputSuspension");Assert.IsFalse(gesture.Suspended);
                Call(runtime,"PlayTapReaction");Assert.That(cues,Is.EqualTo(1),"new intentional taps resume after overlay closes");
                CollectionAssert.AreEquivalent(before,session.OwnedCounts.ToArray(),"guard cannot grant or consume toys");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
                if(had[0])PlayerPrefs.SetString(keys[0],prior);else PlayerPrefs.DeleteKey(keys[0]);
                for(int i=1;i<keys.Length;i++){if(had[i])PlayerPrefs.SetInt(keys[i],ints[i-1]);else PlayerPrefs.DeleteKey(keys[i]);}
                PlayerPrefs.Save();RenderSettings.ambientLight=ambient;RenderSettings.ambientMode=mode;RenderSettings.fog=fog;
                Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;
            }
            yield return null;
        }
    }
}
