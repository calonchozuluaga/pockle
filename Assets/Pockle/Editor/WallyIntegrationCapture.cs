using System;
using System.IO;
using System.Reflection;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
namespace Pockle.Editor
{
    public static class WallyIntegrationCapture
    {
        public static void Capture()
        {
            var output = Path.Combine(Directory.GetCurrentDirectory(), "test-results/wally-ui"); Directory.CreateDirectory(output);
            var root = new GameObject("Wally integration review");
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            var hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_=>{}, _=>{}, _=>{}, _=>{}); hud.Bind(session);
            var cameraObject = new GameObject("UI capture camera"); var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.white;
            var target = new RenderTexture(1080, 2340, 24); camera.targetTexture = target;
            foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                var scaler=canvas.GetComponent<CanvasScaler>(); if(scaler!=null) scaler.enabled=false;
                canvas.scaleFactor=1080f/390;
            }
            Canvas.ForceUpdateCanvases();
            var safe=(RectTransform)typeof(PrototypeHud).GetField("safeRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(hud);
            safe.anchorMin=Vector2.zero; safe.anchorMax=Vector2.one;
            typeof(PrototypeHud).GetMethod("AdaptLayout",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,null);
            var states = new[] {
                new WalkingBoxView {BankedCollectionIds=new[]{"wally"},CanOpen=true,StepCredit=400},
                new WalkingBoxView {BankedCollectionIds=new[]{"wally"},CanOpen=true,Connection=WalkingBoxConnection.Offline},
                new WalkingBoxView {BankedCollectionIds=new[]{"wally","fox"},CanOpen=true,EarnedToday=2},
                new WalkingBoxView {SecondsUntilNextBox=7200,StepCredit=600},
                new WalkingBoxView {EarnedToday=4},
                new WalkingBoxView {Connection=WalkingBoxConnection.Reconnecting}
            };
            string[] names={"ready","offline","full","countdown","day-done","reconnecting"};
            var navigate=typeof(PrototypeHud).GetMethod("Navigate",BindingFlags.Instance|BindingFlags.NonPublic);
            for(int i=0;i<states.Length;i++) foreach(var page in new[]{AppPage.Home,AppPage.Boxes})
            {
                hud.SetWalkingBoxes(states[i]); navigate.Invoke(hud,new object[]{page}); typeof(PrototypeHud).GetMethod("ApplyPage",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(hud,new object[]{true});
                Canvas.ForceUpdateCanvases();
                foreach(var clock in root.GetComponentsInChildren<CountdownText>()) if(clock.enabled) typeof(CountdownText).GetMethod("Tick",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(clock,null);
                camera.Render();
                var previous=RenderTexture.active; RenderTexture.active=target;
                var image=new Texture2D(1080,2340,TextureFormat.RGB24,false); image.ReadPixels(new Rect(0,0,1080,2340),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(output,page+"-"+names[i]+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);RenderTexture.active=previous;
            }
            UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(target);
            Debug.Log("WALLY_UI_CAPTURE_OK");
        }
    }
}
