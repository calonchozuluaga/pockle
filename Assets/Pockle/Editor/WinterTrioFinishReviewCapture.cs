using System;
using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class WinterTrioFinishReviewCapture
    {
        public static void Capture()
        {
            var args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-finishReview");
            if (index < 0) throw new ArgumentException("Supply -finishReview folder");
            string output = args[index + 1]; Directory.CreateDirectory(output);
            var root = new GameObject("Winter finish review");
            var cameraObject = new GameObject("Review camera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>();
            camera.enabled = false; camera.orthographic = true; camera.orthographicSize = 1.18f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.93f,.92f,.94f);
            camera.transform.position = new Vector3(0,0,-5); camera.transform.LookAt(Vector3.zero);
            var toy = root.AddComponent<JellyToy>(); toy.Initialize();
            try
            {
                foreach (var item in ToyCatalog.Collectibles)
                {
                    if (!item.Available || !HolidayCatalog.IsWinterTrio(item.CharacterId)) continue;
                    if (!toy.TrySetCollectible(item.Id)) throw new InvalidOperationException(item.Id);
                    var face = toy.GetComponentInChildren<ToyTemperamentDriver>();
                    face.ApplyFace(PipFaces.Pose(PipTemperament.Sunny,0));
                    if (ShaderUtil.ShaderHasError(toy.BodyRenderer.sharedMaterial.shader)) throw new InvalidOperationException("Winter shader error");
                    foreach (int angle in new[] { 0, 35 })
                    {
                        root.transform.rotation = Quaternion.Euler(0,angle,0);
                        Save(camera,Path.Combine(output,item.Id+(angle==0?"-front.png":"-side.png")));
                    }
                }
                Debug.Log("WINTER_FINISH_REVIEW_OK 60 native views");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(cameraObject); }
        }
        private static void Save(Camera camera,string path)
        {
            var target = new RenderTexture(512,512,24); target.Create();
            var pixels = new Texture2D(512,512,TextureFormat.RGB24,false); var old = RenderTexture.active;
            try { camera.targetTexture=target; camera.Render(); RenderTexture.active=target; pixels.ReadPixels(new Rect(0,0,512,512),0,0); pixels.Apply(); File.WriteAllBytes(path,pixels.EncodeToPNG()); }
            finally { camera.targetTexture=null; RenderTexture.active=old; target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(pixels); }
        }
    }
}
