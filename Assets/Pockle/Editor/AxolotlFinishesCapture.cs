using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    public static class AxolotlFinishesCapture
    {
        private static readonly string Folder = Path.Combine(Path.GetTempPath(), "pockle-axolotl-review");
        public static void After()
        {
            AxolotlCollectionBuild.Build();
            Capture();
        }
        public static void Finishes()
        {
            foreach (var item in ToyCatalog.Collectibles)
                if (item.CharacterId == "axolotl") Capture(item.Id);
        }
        public static void Expressions()
        {
            Capture(ToyCatalog.AxolotlId, "-neutral");
            Capture(ToyCatalog.AxolotlId, "-blink", 1);
            Capture(ToyCatalog.AxolotlId, "-happy", 0, 1);
            Capture(ToyCatalog.AxolotlId, "-surprise", 0, 0, 1);
            Capture(ToyCatalog.AxolotlId, "-wave", 0, 0, 0, 1);
        }
        private static void Capture(string id = ToyCatalog.AxolotlId, string state = "", float blink = 0, float happy = 0, float surprise = 0, float wave = 0)
        {
            Directory.CreateDirectory(Folder);
            Color previousAmbient = RenderSettings.ambientLight;
            var root = new GameObject("Axolotl appearance capture");
            var cameraObject = new GameObject("Axolotl review camera", typeof(Camera));
            RenderTexture target = null; Texture2D pixels = null;
            try
            {
                root.transform.position = new Vector3(1000, 0, 0);
                root.transform.rotation = Quaternion.identity;
                var toy = root.AddComponent<JellyToy>(); toy.Initialize(); toy.TrySetCollectible(id);
                foreach (Transform t in root.GetComponentsInChildren<Transform>()) t.gameObject.layer = 30;
                var material = toy.BodyRenderer.sharedMaterial;
                var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
                camera.cullingMask = 1 << 30; camera.orthographic = true; camera.orthographicSize = 1.16f;
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.973f, .851f, .776f);
                camera.transform.position = root.transform.position + new Vector3(0, .08f, -5);
                camera.transform.LookAt(root.transform.position + Vector3.up * .02f);
                RenderSettings.ambientLight = Color.white;
                var lamp = cameraObject.AddComponent<Light>(); lamp.type = LightType.Directional; lamp.intensity = 1.2f;
                target = new RenderTexture(768, 768, 24, RenderTextureFormat.ARGB32);
                target.Create(); camera.targetTexture = target;
                material.SetFloat("_Blink", blink); material.SetFloat("_Happy", happy); material.SetFloat("_Surprise", surprise); material.SetFloat("_IdleWave", wave); camera.Render();
                var previous = RenderTexture.active;
                try
                { RenderTexture.active = target; pixels = new Texture2D(768, 768, TextureFormat.RGB24, false); pixels.ReadPixels(new Rect(0, 0, 768, 768), 0, 0); pixels.Apply(); }
                finally { RenderTexture.active = previous; }
                File.WriteAllBytes(Path.Combine(Folder, id + state + ".png"), pixels.EncodeToPNG());
                Debug.Log("AXOLOTL_CAPTURE upright using " + material.shader.name);
            }
            finally
            {
                Object.DestroyImmediate(root); Object.DestroyImmediate(cameraObject);
                if (pixels != null) Object.DestroyImmediate(pixels);
                if (target != null) { target.Release(); Object.DestroyImmediate(target); }
                RenderSettings.ambientLight = previousAmbient;
            }
        }
    }
}
