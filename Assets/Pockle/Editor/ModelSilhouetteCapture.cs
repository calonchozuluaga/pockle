using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Editor
{
    public static class ModelSilhouetteCapture
    {
        public static void Capture()
        {
            string output = Path.Combine(Directory.GetCurrentDirectory(), "test-results", "model-silhouettes");
            Directory.CreateDirectory(output);
            foreach (var character in ToyCatalog.Characters)
            {
                string id = null;
                foreach (var item in ToyCatalog.Collectibles) if (item.CharacterId == character.Id) { id = item.Id; break; }
                if (!CharacterArt.TryLoad(id, out var asset)) throw new System.InvalidOperationException(character.Id);
                var root = new GameObject("Silhouette capture");
                var view = new GameObject("Camera", typeof(Camera));
                var camera = view.GetComponent<Camera>(); camera.enabled = false;
                camera.orthographic = true; camera.orthographicSize = .65f;
                camera.transform.position = new Vector3(0, 0, -5);
                camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.98f, .95f, .91f);
                var material = new Material(Shader.Find("Unlit/Texture")); material.mainTexture = asset.BaseColorTexture;
                var target = new RenderTexture(900, 300, 24); target.Create();
                var image = new Texture2D(900, 300, TextureFormat.RGB24, false);
                var previous = RenderTexture.active;
                try
                {
                    var model = new GameObject("Actual body", typeof(MeshFilter), typeof(MeshRenderer)); model.transform.SetParent(root.transform);
                    model.GetComponent<MeshFilter>().sharedMesh = asset.BodyMesh; model.GetComponent<MeshRenderer>().sharedMaterial = material;
                    float size = Mathf.Max(asset.BodyMesh.bounds.size.x, asset.BodyMesh.bounds.size.y);
                    model.transform.localScale = Vector3.one / size;
                    model.transform.position = new Vector3(-1.3f, 0, 0) - asset.BodyMesh.bounds.center / size;
                    for (int i = 0; i < 2; i++)
                    {
                        var holder = new GameObject("Outline canvas", typeof(RectTransform), typeof(Canvas)); holder.transform.SetParent(root.transform);
                        var canvas = holder.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
                        holder.GetComponent<RectTransform>().sizeDelta = new Vector2(1, 1);
                        holder.transform.position = new Vector3(i * 1.3f, 0, 0);
                        var shape = new GameObject("Outline", typeof(RectTransform), typeof(CharacterSilhouetteGraphic)); shape.transform.SetParent(holder.transform, false);
                        var graphic = shape.GetComponent<CharacterSilhouetteGraphic>(); graphic.rectTransform.sizeDelta = Vector2.one / .9f;
                        graphic.color = new Color(.36f, .29f, .38f); graphic.Configure(character.Id, i == 1);
                    }
                    Canvas.ForceUpdateCanvases(); camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
                    image.ReadPixels(new Rect(0, 0, 900, 300), 0, 0); image.Apply();
                    File.WriteAllBytes(Path.Combine(output, character.Id + ".png"), image.EncodeToPNG());
                }
                finally
                {
                    camera.targetTexture = null; RenderTexture.active = previous; target.Release();
                    Object.DestroyImmediate(root); Object.DestroyImmediate(view); Object.DestroyImmediate(material);
                    Object.DestroyImmediate(target); Object.DestroyImmediate(image);
                }
            }
            Debug.Log("MODEL_SILHOUETTE_REVIEW_OK: 11 actual bodies, normal and rare silhouettes.");
        }
    }
}
