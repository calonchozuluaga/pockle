using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Pockle.Editor
{
    // Offline editorial fixtures rendered by the real HUD. Never connects accounts or stores.
    [InitializeOnLoad] public static class GooglePlayCapture
    {
        const string Active="Pockle.GooglePlayCapture";
        static readonly string[] Keys={"pockle.collection.v1","pockle.collection.v1.before-v5","pockle.collection.v1.backup",CollectionSession.BadgePreferenceKey,PrototypeHud.LiveShelfKey,PrototypeHud.LiveShelfSeenKey,PrototypeHud.MomentsKey,"pockle.profile.local.name","pockle.profile.local.avatarId","pockle.profile.local.favoriteId"};
        static readonly string[] Shots={"01-home","02-collections","03-pip-collection","04-live-shelf","05-fusion","06-play-pip"};
        static PrototypeHud hud; static GameObject stage; static Camera toyCamera,background; static GameObject root; static Camera camera; static RenderTexture target;
        static int index,wait,last=-1; static bool initialized,prepared;
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static string Output=>Path.Combine(Directory.GetCurrentDirectory(),"docs/google-play-art-review/01/screenshots");
        static GooglePlayCapture(){if(SessionState.GetBool(Active,false))EditorApplication.update+=Tick;}
        public static void Capture()
        {
            if(Application.unityVersion!="6000.6.5f1")throw new InvalidOperationException("Pinned Unity required");
            foreach(var key in Keys){SessionState.SetBool(Active+key,PlayerPrefs.HasKey(key));SessionState.SetString(Active+key+"value",PlayerPrefs.GetString(key,""));}
            var save=new CollectionSave{Version=CollectionProgress.SaveVersion,Inventory=new List<OwnedCollectible>(),Day=CollectionSession.Today};
            foreach(var id in new[]{ToyCatalog.PeachId,ToyCatalog.MoonId,ToyCatalog.GoldId,ToyCatalog.MintId,"fox.gloss-vinyl","axolotl.clear-jelly","firb.matte-vinyl",WallyCatalog.ClassicId})
                if(ToyCatalog.TryGetCollectible(id,out var toy)&&toy.Available)save.Inventory.Add(new OwnedCollectible(id,3));
            PlayerPrefs.SetString("pockle.collection.v1",JsonUtility.ToJson(save));
            PlayerPrefs.SetString("pockle.profile.local.name","Collector");
            PlayerPrefs.SetString(PrototypeHud.LiveShelfKey,string.Join(",",save.Inventory.ConvertAll(x=>x.Id).GetRange(0,Math.Min(6,save.Inventory.Count))));
            PlayerPrefs.DeleteKey(PrototypeHud.MomentsKey);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            Directory.CreateDirectory(Output);SessionState.SetBool(Active,true);EditorApplication.isPlaying=true;
        }
        static void Call(string name,params object[] args)=>typeof(PrototypeHud).GetMethod(name,Private).Invoke(hud,args);
        static T Field<T>(string name)=>(T)typeof(PrototypeHud).GetField(name,Private).GetValue(hud);
        static void Restore()
        {
            if(stage!=null)UnityEngine.Object.DestroyImmediate(stage);
            if(root!=null)UnityEngine.Object.DestroyImmediate(root);
            foreach(var key in Keys){if(SessionState.GetBool(Active+key,false))PlayerPrefs.SetString(key,SessionState.GetString(Active+key+"value",""));else PlayerPrefs.DeleteKey(key);}
            PlayerPrefs.Save();SessionState.SetBool(Active,false);
            foreach(var key in Keys)if(PlayerPrefs.HasKey(key)!=SessionState.GetBool(Active+key,false)||(PlayerPrefs.HasKey(key)&&PlayerPrefs.GetString(key)!=SessionState.GetString(Active+key+"value","")))throw new Exception("Capture preference restoration failed: "+key);
            Debug.Log("POCKLE_GOOGLE_PLAY_PREFS_RESTORED");
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling||last==Time.frameCount)return;last=Time.frameCount;
            try
            {
                if(!initialized)
                {
                    root=new GameObject("Offline Google Play capture");var session=root.AddComponent<CollectionSession>();session.Initialize();
                    hud=root.AddComponent<PrototypeHud>();hud.Initialize(_=>{},_=>{},_=>{},_=>{});hud.Bind(session);hud.SetFusionService(new LocalFusionService(session));
                    var c=new GameObject("Portrait capture camera");c.transform.SetParent(root.transform);camera=c.AddComponent<Camera>();camera.orthographic=true;camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.985f,.973f,.961f);
                    target=new RenderTexture(1080,1920,24);target.Create();camera.targetTexture=target;
                    foreach(var canvas in root.GetComponentsInChildren<Canvas>(true)){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;var scaler=canvas.GetComponent<CanvasScaler>();if(scaler!=null)scaler.enabled=false;canvas.scaleFactor=1080f/390;}
                    initialized=true;wait=30;return;
                }
                if(wait-->0)return;
                if(index>=Shots.Length){Restore();Debug.Log("POCKLE_GOOGLE_PLAY_CAPTURE_OK:"+Shots.Length);EditorApplication.Exit(0);return;}
                foreach(var name in new[]{"safeRoot","modalSafeRoot"}){var r=Field<RectTransform>(name);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;}
                if(!prepared)
                {
                    if(index==5)hud.ShowToy(ToyCatalog.PeachId);else if(index==2)Call("OpenCollection","pip");else Call("Navigate",index==0?AppPage.Home:index==1?AppPage.Collections:index==3?AppPage.Shelf:AppPage.Fusion);
                    if(index==4){Call("PickSpare",ToyCatalog.PeachId);Call("PickSpare",ToyCatalog.MoonId);}
                    if(index==5)
                    {
                        PrototypeStage.Create(out toyCamera,out var mount,out _,out _,out _);stage=toyCamera.transform.parent.gameObject;
                        var toyObject=new GameObject("Peach Pip viewer");toyObject.transform.SetParent(mount,false);var toy=toyObject.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(ToyCatalog.PeachId);
                        PrototypeStage.SetWorld(toyCamera,StageWorld.Garden,ToyCatalog.PeachId);PrototypeStage.SetLight(toyCamera,StageLight.Day);
                        var framing=toyCamera.GetComponent("StageFraming") as Behaviour;if(framing!=null)framing.enabled=false;
                        toyCamera.targetTexture=target;toyCamera.rect=new Rect(0,0,1,1);toyCamera.aspect=1080f/1920;toyCamera.orthographicSize=2.1f;toyCamera.transform.LookAt(new Vector3(0,1.53f,0));
                        background=stage.transform.Find("Full-screen background").GetComponent<Camera>();background.targetTexture=target;
                        camera.clearFlags=CameraClearFlags.Depth;camera.cullingMask=1<<28;
                        foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=28;
                    }
                    Call("AdaptLayout");prepared=true;wait=40;return;
                }
                Canvas.ForceUpdateCanvases();if(index==5){background.Render();toyCamera.Render();}camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
                var image=new Texture2D(1080,1920,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1080,1920),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(Output,Shots[index]+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);RenderTexture.active=previous;
                index++;prepared=false;wait=4;
            }
            catch(Exception e){Restore();Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
