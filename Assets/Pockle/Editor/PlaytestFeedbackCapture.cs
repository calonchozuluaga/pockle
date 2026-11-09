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
    [InitializeOnLoad] public static class PlaytestFeedbackCapture
    {
        const string Active="Pockle.PlaytestFeedbackCapture";
        static readonly string[] Keys={"pockle.collection.v1","pockle.collection.v1.before-v5","pockle.collection.v1.before-v6","pockle.stage.environment.v1.pip","pockle.stage.environment.v1.fox",LocalActivityFeed.LogKey,ShelfFriendships.Key,BadgeCabinet.Key,"pockle.collection.v1.backup",CollectionSession.BadgePreferenceKey,PrototypeHud.LiveShelfKey,PrototypeHud.LiveShelfSeenKey,PrototypeHud.MomentsKey,"pockle.profile.local.name","pockle.profile.local.avatarId","pockle.profile.local.favoriteId"};
        static readonly string[] IntKeys={StageLook.LightKey,StageLook.WorldKey,PipCharacterAsset.MeshyPreference,"pockle.profile.local.avatar","pockle.profile.local.favorite"};
        static readonly string[] Shots={"01-home","02-shelf","03-feature-picker","04-avatar-border","05-fox-yawn","06-fox-grumpy","07-pip-happy","08-pip-grumpy","09-mix3-waiting","10-mix3-first","11-mix3-second","12-mix3-summary","13-mix5-summary","14-small-shelf","15-shelf-fusion-action","16-bubble-fade"};
        static PrototypeHud hud; static GameObject stage; static Camera toyCamera,background; static GameObject root; static Camera camera; static RenderTexture target;
        static JellyToy previewToy; static ProfileAvatarService avatarService; static string avatarScope; static ToyReactionResponse response;
        static double readyAt; static int index,wait,last=-1; static bool initialized,prepared;
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static string Output=>Path.Combine(Directory.GetCurrentDirectory(),"docs/playtest-feedback-review/01/screenshots");
        static PlaytestFeedbackCapture(){if(SessionState.GetBool(Active,false))EditorApplication.update+=Tick;}
        public static void Capture()
        {
            if(Application.unityVersion!="6000.6.5f1")throw new InvalidOperationException("Pinned Unity required");
            foreach(var key in Keys){SessionState.SetBool(Active+key,PlayerPrefs.HasKey(key));SessionState.SetString(Active+key+"value",PlayerPrefs.GetString(key,""));}
            foreach(var key in IntKeys){SessionState.SetBool(Active+key,PlayerPrefs.HasKey(key));SessionState.SetInt(Active+key+"int",PlayerPrefs.GetInt(key));}
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,1);
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
            if(avatarService!=null && avatarScope!=null && !avatarService.DeleteAccountData(avatarScope))throw new Exception("Fixture avatar cleanup failed");
            if(stage!=null)UnityEngine.Object.DestroyImmediate(stage);
            if(root!=null)UnityEngine.Object.DestroyImmediate(root);
            foreach(var key in Keys){if(SessionState.GetBool(Active+key,false))PlayerPrefs.SetString(key,SessionState.GetString(Active+key+"value",""));else PlayerPrefs.DeleteKey(key);}
            foreach(var key in IntKeys){if(SessionState.GetBool(Active+key,false))PlayerPrefs.SetInt(key,SessionState.GetInt(Active+key+"int",0));else PlayerPrefs.DeleteKey(key);}
            PlayerPrefs.Save();SessionState.SetBool(Active,false);
            foreach(var key in Keys)if(PlayerPrefs.HasKey(key)!=SessionState.GetBool(Active+key,false)||(PlayerPrefs.HasKey(key)&&PlayerPrefs.GetString(key)!=SessionState.GetString(Active+key+"value","")))throw new Exception("Capture preference restoration failed: "+key);
            Debug.Log("POCKLE_FEEDBACK11_PREFS_RESTORED");
        }
        // Same framing calculation as StageFraming, using the capture target rather than editor Screen dimensions.
        static void FrameToy()
        {
            var p=hud.StageViewport;
            var frame=new Rect(p.x/target.width,p.y/target.height,p.width/target.width,p.height/target.height);
            float aspect=(float)target.width/target.height;
            float halfHeight=ToyOrbit.FrameSize(aspect*frame.width/frame.height,1,true)/frame.height;
            float halfWidth=halfHeight*aspect;
            var offset=new Vector2((1-2*frame.center.x)*halfWidth,(1-2*frame.center.y)*halfHeight);
            toyCamera.orthographicSize=halfHeight;toyCamera.transform.LookAt(new Vector3(0,1.15f,0));
            toyCamera.projectionMatrix=Matrix4x4.Ortho(offset.x-halfWidth,offset.x+halfWidth,offset.y-halfHeight,offset.y+halfHeight,toyCamera.nearClipPlane,toyCamera.farClipPlane);
        }
        static void ShowPack(int count)
        {
            var items=new List<BoxRevealItem>();var ids=new[]{ToyCatalog.PeachId,"fox.gloss-vinyl","axolotl.clear-jelly",WallyCatalog.ClassicId,"firb.matte-vinyl"};
            for(int i=0;i<count;i++){ToyCatalog.TryGetCollectible(ids[i],out var toy);items.Add(new BoxRevealItem{CollectibleId=ids[i],CollectionId=toy.CollectionId,Tier=RarityLook.FromTier(toy.Tier),IsNew=i%2==0});}
            if(!hud.ShowBoxReveal(new BoxRevealView{ReceiptId="offline-review-"+count,Source=BoxRevealSource.MysteryMix,Items=items}))throw new Exception("Pack fixture refused");
        }
        static void Tick()
        {
            if(!EditorApplication.isPlaying||EditorApplication.isCompiling||last==Time.frameCount)return;last=Time.frameCount;
            try
            {
                if(!initialized)
                {
                    root=new GameObject("Offline approved UI capture");var session=root.AddComponent<CollectionSession>();session.Initialize();
                    hud=root.AddComponent<PrototypeHud>();hud.Initialize(_=>{},_=>{},_=>{},_=>{});hud.Bind(session);hud.SetFusionService(new LocalFusionService(session));
                    avatarScope=Guid.NewGuid().ToString();avatarService=root.AddComponent<ProfileAvatarService>();hud.SetAvatarService(avatarService,()=>avatarScope);avatarService.SelectPreset("fox");
                    hud.SetWalkingBoxes(new WalkingBoxView{StepCredit=680,EarnedToday=1,SecondsUntilNextBox=1800});
                    var offers=new List<CheckoutOffer>();foreach(var p in PaidBoxProducts.All)offers.Add(new CheckoutOffer{OfferId=p.Id,CollectionId=p.CollectionId,BoxCount=p.Count,Kind=p.CollectionId==null?CheckoutOfferKind.MysteryMix:CheckoutOfferKind.CollectionBox,Available=true,LocalizedPrice=p.Count==1?"$0.99":p.Count==3?"$2.49":"$3.99"});hud.SetCheckoutCatalog(new CheckoutCatalog{Status=CheckoutCatalogStatus.Ready,Offers=offers});
                    var c=new GameObject("Portrait capture camera");c.transform.SetParent(root.transform);camera=c.AddComponent<Camera>();camera.orthographic=true;camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=PockleTheme.Page;
                    target=new RenderTexture(1080,2340,24);target.Create();camera.targetTexture=target;
                    foreach(var canvas in root.GetComponentsInChildren<Canvas>(true)){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;var scaler=canvas.GetComponent<CanvasScaler>();if(scaler!=null)scaler.enabled=false;canvas.scaleFactor=1080f/390;}
                    initialized=true;wait=30;return;
                }
                if(wait-->0||EditorApplication.timeSinceStartup<readyAt)return;
                if(index>=Shots.Length){Restore();Debug.Log("POCKLE_FEEDBACK11_CAPTURE_OK:"+Shots.Length);EditorApplication.Exit(0);return;}
                foreach(var name in new[]{"safeRoot","modalSafeRoot"}){var r=Field<RectTransform>(name);r.anchorMin=new Vector2(0,index==13?0:34f/844);r.anchorMax=new Vector2(1,1-(index==13?20f/568:44f/844));r.offsetMin=r.offsetMax=Vector2.zero;}
                if(!prepared)
                {
                    if(index==0)Call("Navigate",AppPage.Home);
                    if(index==1)Call("Navigate",AppPage.Shelf);
                    if(index==2)Call("BeginHomeFeatureChoice");
                    if(index==3)Call("Navigate",AppPage.Profile);
                    if((index>=4 && index<=7)||index==15)
                    {
                        stage?.SetActive(true);camera.clearFlags=CameraClearFlags.Depth;string id=index<6?"fox.gloss-vinyl":ToyCatalog.PeachId; hud.ShowToy(id);
                        if(stage==null)
                        {
                            PrototypeStage.Create(out toyCamera,out var mount,out _,out _,out _);stage=toyCamera.transform.parent.gameObject;
                            var obj=new GameObject("Offline response toy");obj.transform.SetParent(mount,false);previewToy=obj.AddComponent<JellyToy>();previewToy.Initialize();
                            var framing=toyCamera.GetComponent("StageFraming") as Behaviour;if(framing!=null)framing.enabled=false;
                            toyCamera.clearFlags=CameraClearFlags.Depth;toyCamera.targetTexture=target;toyCamera.rect=new Rect(0,0,1,1);toyCamera.aspect=1080f/2340;
                            background=stage.transform.Find("Full-screen background").GetComponent<Camera>();background.targetTexture=target;
                            camera.clearFlags=CameraClearFlags.Depth;camera.cullingMask=1<<28;
                            foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=28;
                        }
                        previewToy.TrySetCollectible(id);PrototypeStage.SetWorld(toyCamera,StageWorld.Garden,id);PrototypeStage.SetLight(toyCamera,StageLight.Day);
                        string character=index<6?"fox":"pip";
                        response=index==4?ToyReactionResponses.Action("fox",FoxAction.SleepyBlink):ToyReactionResponses.Temperament(character,index==6?PipTemperament.Sunny:PipTemperament.Grumpy);
                        hud.SetStatus("Tap · drag to turn");
                    }
                    if(index==8)
                    {
                        stage.SetActive(false);camera.clearFlags=CameraClearFlags.SolidColor;
                        ShowPack(3);
                    }
                    if(index==9)hud.CompleteBoxRevealStep();
                    if(index==10){Call("UnboxPrimaryTapped");hud.CompleteBoxRevealStep();}
                    if(index==11)hud.SkipBoxReveal();
                    if(index==12){hud.FinishBoxReveal();ShowPack(5);hud.SkipBoxReveal();}
                    if(index==14)
                    {
                        target.Release();UnityEngine.Object.DestroyImmediate(target);target=new RenderTexture(1080,2340,24);target.Create();camera.targetTexture=target;
                        foreach(var canvas in root.GetComponentsInChildren<Canvas>(true))canvas.scaleFactor=1080f/390;
                        Call("Navigate",AppPage.Shelf);
                    }
                    if(index==13)
                    {
                        hud.FinishBoxReveal();Call("Navigate",AppPage.Shelf);
                        target.Release();UnityEngine.Object.DestroyImmediate(target);target=new RenderTexture(960,1704,24);target.Create();camera.targetTexture=target;
                        foreach(var canvas in root.GetComponentsInChildren<Canvas>(true))canvas.scaleFactor=3;
                    }
                    Call("AdaptLayout");
                    if(index==14){Canvas.ForceUpdateCanvases();var content=Field<RectTransform>("content");var viewport=(RectTransform)content.parent;content.anchoredPosition=new Vector2(0,Mathf.Max(0,content.rect.height-viewport.rect.height));}
                    prepared=true;wait=12;readyAt=EditorApplication.timeSinceStartup+.35;return;
                }
                Canvas.ForceUpdateCanvases();
                if(index==14){var content=Field<RectTransform>("content");var viewport=(RectTransform)content.parent;content.anchoredPosition=new Vector2(0,Mathf.Max(0,content.rect.height-viewport.rect.height));Canvas.ForceUpdateCanvases();}
                if((index>=4 && index<=7)||index==15)
                {
                    var native=previewToy.GetComponentInChildren<ToyTemperamentDriver>();var pip=previewToy.GetComponentInChildren<PeachExpressionDriver>();
                    if(native!=null)native.ApplyFace(ToyReactionResponses.FacePose(response,.4f));else pip.ApplyFace(ToyReactionResponses.FacePose(response,.4f));
                    hud.ShowToyResponse(new ToyExpression(response.Text),null,0,3);
                    var bubble=Field<BubblePop>("talkPop");typeof(BubblePop).GetField("start",Private).SetValue(bubble,Time.unscaledTime-(index==15?2.85f:.5f));
                    typeof(BubblePop).GetMethod("Update",Private).Invoke(bubble,null);
                    toyCamera.targetTexture=target;background.targetTexture=target;toyCamera.aspect=(float)target.width/target.height;
                    FrameToy();background.Render();toyCamera.Render();
                }
                camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;
                var image=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,target.width,target.height),0,0);image.Apply();
                File.WriteAllBytes(Path.Combine(Output,Shots[index]+".png"),image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);RenderTexture.active=previous;
                index++;prepared=false;wait=4;
            }
            catch(Exception e){Restore();Debug.LogException(e);EditorApplication.Exit(1);}
        }
    }
}
