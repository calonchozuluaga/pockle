using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    public static class PeachReviewCapture
    {
        private static readonly string Folder = Path.Combine(Path.GetTempPath(), "pockle-peach-review");
        public static void Before() { Capture("before", 0, 0, 0, 0); }
        public static void After()
        {
            Capture("neutral", 0, 0, 0, 0);
            Capture("blink", 1, 0, 0, 0);
            Capture("happy", 0, 1, 0, 0);
            Capture("surprised", 0, 0, 1, 0);
            Capture("turned", 0, 0, 0, 35);
        }
        public static void Collection()
        {
            foreach (var item in ToyCatalog.Collectibles)
                if (item.CharacterId == "pip") Capture(item.Id,0,0,0,0,item.Id);
        }
        private static void Capture(string name, float blink, float happy, float surprise, float yaw, string id = ToyCatalog.PeachId)
        {
            Directory.CreateDirectory(Folder);
            bool hadPreference = PlayerPrefs.HasKey(PipCharacterAsset.MeshyPreference);
            int preference = PlayerPrefs.GetInt(PipCharacterAsset.MeshyPreference);
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, 1);
            var root = new GameObject("Peach appearance capture");
            var cameraObject = new GameObject("Peach review camera", typeof(Camera));
            RenderTexture target = null; Texture2D pixels = null;
            try
            {
                root.transform.position = new Vector3(1000, 0, 0);
                root.transform.rotation = Quaternion.Euler(0, yaw, 0);
                var toy = root.AddComponent<JellyToy>(); toy.Initialize(); toy.TrySetCollectible(id);
                foreach (Transform t in root.GetComponentsInChildren<Transform>()) t.gameObject.layer = 30;
                var material = toy.BodyRenderer.sharedMaterial;
                if (material.HasProperty("_Blink"))
                { material.SetFloat("_Blink", blink); material.SetFloat("_Happy", happy); material.SetFloat("_Surprise", surprise); }
                var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
                camera.cullingMask = 1 << 30; camera.orthographic = true; camera.orthographicSize = 1.16f;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.973f, .851f, .776f);
                camera.transform.position = root.transform.position + new Vector3(0, .08f, -5);
                camera.transform.LookAt(root.transform.position + Vector3.up * .02f);
                target = new RenderTexture(768, 768, 24, RenderTextureFormat.ARGB32);
                target.Create(); camera.targetTexture = target; camera.Render();
                var previous = RenderTexture.active;
                try
                { RenderTexture.active = target; pixels = new Texture2D(768, 768, TextureFormat.RGB24, false); pixels.ReadPixels(new Rect(0, 0, 768, 768), 0, 0); pixels.Apply(); }
                finally { RenderTexture.active = previous; }
                File.WriteAllBytes(Path.Combine(Folder, name + ".png"), pixels.EncodeToPNG());
                Debug.Log("PEACH_CAPTURE " + name + " using " + material.shader.name);
            }
            finally
            {
                Object.DestroyImmediate(root); Object.DestroyImmediate(cameraObject);
                if (pixels != null) Object.DestroyImmediate(pixels);
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
                if (hadPreference) PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, preference); else PlayerPrefs.DeleteKey(PipCharacterAsset.MeshyPreference);
            }
        }
    }
}
