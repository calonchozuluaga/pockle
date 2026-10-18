using System;
using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class PipTemperamentFaceCapture
    {
        [Serializable] public sealed class Manifest { public Style[] styles; }
        [Serializable] public sealed class Style { public string id,name;public Pose rest,poke,signature; }
        [Serializable] public sealed class Pose
        {
            public float blink,happy,surprise,lids,frown,glanceX,glanceY,blush,wink,lidAngle,sleepCurve,mouthOpen,mouthTilt,highlightStyle,mouthScale=1;
            public static Pose Blend(Pose a,Pose b,float t)
            {
                t=Mathf.SmoothStep(0,1,t);return new Pose{blink=Mathf.Lerp(a.blink,b.blink,t),happy=Mathf.Lerp(a.happy,b.happy,t),surprise=Mathf.Lerp(a.surprise,b.surprise,t),lids=Mathf.Lerp(a.lids,b.lids,t),frown=Mathf.Lerp(a.frown,b.frown,t),glanceX=Mathf.Lerp(a.glanceX,b.glanceX,t),glanceY=Mathf.Lerp(a.glanceY,b.glanceY,t),blush=Mathf.Lerp(a.blush,b.blush,t),wink=Mathf.Lerp(a.wink,b.wink,t),lidAngle=Mathf.Lerp(a.lidAngle,b.lidAngle,t),sleepCurve=Mathf.Lerp(a.sleepCurve,b.sleepCurve,t),mouthOpen=Mathf.Lerp(a.mouthOpen,b.mouthOpen,t),mouthTilt=Mathf.Lerp(a.mouthTilt,b.mouthTilt,t),highlightStyle=Mathf.Lerp(a.highlightStyle,b.highlightStyle,t),mouthScale=Mathf.Lerp(a.mouthScale,b.mouthScale,t)};
            }
        }
        private static void Apply(Material m,Pose p)
        {
            m.SetFloat("_Blink",p.blink);m.SetFloat("_Happy",p.happy);m.SetFloat("_Surprise",p.surprise);
            m.SetFloat("_Lids",p.lids);m.SetFloat("_Frown",p.frown);m.SetFloat("_GlanceX",p.glanceX);m.SetFloat("_GlanceY",p.glanceY);
            m.SetFloat("_LidAngle",p.lidAngle);m.SetFloat("_SleepCurve",p.sleepCurve);m.SetFloat("_MouthOpen",p.mouthOpen);m.SetFloat("_MouthTilt",p.mouthTilt);m.SetFloat("_HighlightStyle",p.highlightStyle);
            m.SetFloat("_Blush",p.blush);m.SetFloat("_Wink",p.wink);m.SetFloat("_MouthScale",p.mouthScale);
        }
        private static void Render(Camera camera,int size,string file)
        {
            var target=new RenderTexture(size,size,24);target.Create();var pixels=new Texture2D(size,size,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,size,size),0,0);pixels.Apply();File.WriteAllBytes(file,pixels.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
        }
        private static Texture2D BakeFace(JellyToy toy,Camera camera)
        {
            var original=toy.BodyRenderer.sharedMaterial;
            var unlit=new Material(Shader.Find("Unlit/Texture")){mainTexture=original.mainTexture};
            toy.BodyRenderer.gameObject.layer=30;toy.BodyRenderer.sharedMaterial=unlit;
            var target=new RenderTexture(2048,2048,24);target.Create();var texture=new Texture2D(2048,2048,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try
            {
                camera.orthographic=true;camera.orthographicSize=1.2f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                camera.transform.position=new Vector3(0,0,-5);camera.transform.LookAt(Vector3.zero);camera.cullingMask=1<<30;camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                texture.ReadPixels(new Rect(0,0,2048,2048),0,0);texture.Apply();texture.wrapMode=TextureWrapMode.Clamp;return texture;
            }
            finally{toy.BodyRenderer.sharedMaterial=original;camera.cullingMask=~0;camera.targetTexture=null;RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(unlit);}
        }
        public static void Capture()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-facePreview");if(i<0||i+1>=args.Length)throw new ArgumentException("Supply -facePreview <folder>.");
            string output=args[i+1];var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(output,"poses.json")));
            if(manifest.styles==null||manifest.styles.Length!=8)throw new InvalidOperationException("Expected eight temperaments.");
            Directory.CreateDirectory(Path.Combine(output,"renders"));Directory.CreateDirectory(Path.Combine(output,"frames"));
            var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Pockle/Editor/PipTemperamentPreview.shader");if(shader==null||!shader.isSupported)throw new InvalidOperationException("Face preview shader unavailable.");
            foreach(var style in manifest.styles)
            {
                var obj=new GameObject("Pip face preview "+style.id);var camObj=new GameObject("Face camera",typeof(Camera));Material preview=null;Texture2D bakedFace=null;
                try
                {
                    var toy=obj.AddComponent<JellyToy>();toy.Initialize();if(!toy.TrySetCollectible(ToyCatalog.PeachId))throw new InvalidOperationException("Pip unavailable.");
                    // Force the approved baked asset even if an owner preference currently selects the old mesh.
                    if(!toy.UsesMeshyPeach){toy.TrySetCollectible(PipFusionStudies.DuskId);toy.TrySetCollectible(ToyCatalog.PeachId);}
                    if(!toy.UsesMeshyPeach)throw new InvalidOperationException("Approved baked Pip required.");
                    var camera=camObj.GetComponent<Camera>();camera.enabled=false;bakedFace=BakeFace(toy,camera);
                    preview=new Material(shader);preview.CopyPropertiesFromMaterial(toy.BodyRenderer.sharedMaterial);preview.SetTexture("_PreviewFace",bakedFace);toy.BodyRenderer.sharedMaterial=preview;
                    camera.orthographic=true;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.973f,.851f,.776f);RenderSettings.ambientLight=Color.white;
                    string[] states={"rest","poke","signature"};Pose[] poses={style.rest,style.poke,style.signature};
                    for(int state=0;state<3;state++)
                    {
                        Apply(preview,poses[state]);
                        camera.orthographicSize=1.2f;camera.transform.position=new Vector3(0,.1f,-5);camera.transform.LookAt(Vector3.zero);
                        Render(camera,512,Path.Combine(output,"renders",style.id+"-"+states[state]+"-body.png"));
                        camera.orthographicSize=.60f;camera.transform.position=new Vector3(.02f,-.12f,-5);camera.transform.LookAt(new Vector3(.02f,-.12f,0));
                        Render(camera,640,Path.Combine(output,"renders",style.id+"-"+states[state]+"-face.png"));
                    }
                    string frames=Path.Combine(output,"frames",style.id);Directory.CreateDirectory(frames);
                    for(int frame=0;frame<64;frame++)
                    {
                        Pose p=frame<13?Pose.Blend(style.rest,style.rest,0):frame<25?Pose.Blend(style.rest,style.poke,(frame-13)/11f):frame<33?Pose.Blend(style.poke,style.poke,0):frame<44?Pose.Blend(style.poke,style.signature,(frame-33)/10f):frame<51?Pose.Blend(style.signature,style.signature,0):Pose.Blend(style.signature,style.rest,(frame-51)/12f);
                        if(frame<13&&(style.id=="sunny"||style.id=="calm"||style.id=="dreamy"))p.blink=Mathf.Max(p.blink,Mathf.Exp(-Mathf.Pow((frame-7)/(style.id=="dreamy"?2.5f:1.6f),2)));
                        Apply(preview,p);Render(camera,384,Path.Combine(frames,frame.ToString("D3")+".png"));
                    }
                }
                finally{UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(camObj);if(preview!=null)UnityEngine.Object.DestroyImmediate(preview);if(bakedFace!=null)UnityEngine.Object.DestroyImmediate(bakedFace);}
            }
            if(ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Face preview shader failed compilation; discard this capture.");
            Debug.Log("PIP_TEMPERAMENT_FACES_OK eight styles / 48 stills / 512 loop frames; editor-only.");
        }
    }
}
