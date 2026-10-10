using System.Collections;
using System.IO;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    /// <summary>Real Unity render evidence, kept in the existing CI test-results artifact.</summary>
    public sealed class PipJellyCaptureTests
    {
        private GameObject stage, toyObject;
        private RenderTexture target;
        private Color ambient;
        private UnityEngine.Rendering.AmbientMode ambientMode;
        private bool fog;
        private bool hadBaseline, hadVariant;
        private int baseline, variant;

        [SetUp]
        public void SaveSettings()
        {
            ambient = RenderSettings.ambientLight; ambientMode = RenderSettings.ambientMode; fog = RenderSettings.fog;
            hadBaseline = PlayerPrefs.HasKey(PipCharacterAsset.BaselinePreference);
            hadVariant = PlayerPrefs.HasKey(PipVariants.Preference);
            baseline = PlayerPrefs.GetInt(PipCharacterAsset.BaselinePreference);
            variant = PlayerPrefs.GetInt(PipVariants.Preference);
            PlayerPrefs.SetInt(PipCharacterAsset.BaselinePreference, 0);
            PlayerPrefs.SetInt(PipVariants.Preference, 0);
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (stage != null) Object.Destroy(stage);
            if (toyObject != null) Object.Destroy(toyObject);
            if (target != null) { target.Release(); Object.Destroy(target); }
            if (hadBaseline) PlayerPrefs.SetInt(PipCharacterAsset.BaselinePreference, baseline);
            else PlayerPrefs.DeleteKey(PipCharacterAsset.BaselinePreference);
            if (hadVariant) PlayerPrefs.SetInt(PipVariants.Preference, variant);
            else PlayerPrefs.DeleteKey(PipVariants.Preference);
            yield return null;
            yield return null;
            RenderSettings.ambientLight = ambient; RenderSettings.ambientMode = ambientMode; RenderSettings.fog = fog;
        }

        [UnityTest]
        public IEnumerator FourFinishesRenderOnTheirColourFieldsAndKeepGripPicking()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("A graphics device is required for Unity review captures.");
            PrototypeStage.Create(out Camera camera, out Transform mount, out _, out _, out _);
            stage = camera.transform.parent.gameObject;
            toyObject = new GameObject("Captured jelly Pip");
            toyObject.transform.SetParent(mount, false);
            var toy = toyObject.AddComponent<JellyToy>();
            Assert.That(toy.UsesAuthoredMesh, Is.True);
            Assert.That(toy.BodyRenderer.sharedMaterial.shader.name, Is.EqualTo("Pockle/Jelly Candy"));
            Assert.That(toy.BodyRenderer.sharedMaterial.shader.isSupported, Is.True);
            // Isolate captures from the Test Runner's scene and other cameras.
            foreach (Transform child in stage.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
            camera.cullingMask = 1 << 31;
            camera.enabled = false;
            target = new RenderTexture(720, 960, 24, RenderTextureFormat.ARGB32);
            target.Create();
            string directory = Path.Combine(Path.GetDirectoryName(Application.dataPath), "test-results", "pip-jelly");
            Directory.CreateDirectory(directory);
            for (int i = 0; i < PipVariants.Count; i++)
            {
                toy.SetVariant((PipVariant)i);
                toy.ResetToy();
                foreach (Transform child in toyObject.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
                yield return null; // Destroy old filling and let stage framing initialize.
                Assert.That(toy.RaycastBody(new Ray(mount.position + new Vector3(0, -.4f, -3), Vector3.forward), out _), Is.True);
                Assert.That(toy.RaycastBody(new Ray(mount.position + new Vector3(.6f, -.9f, -3), Vector3.forward), out _), Is.True,
                    "The settled lower belly must remain pickable beside its contact area.");
                Assert.That(toy.RaycastBody(new Ray(mount.position + new Vector3(.86f, -.95f, -3), Vector3.forward), out _), Is.False,
                    "Picking must follow the rounded corner rather than an invisible wide foot.");
                camera.backgroundColor = PockleTheme.FieldFor(PipVariants.CollectibleId((PipVariant)i));
                Capture(camera, Path.Combine(directory, ((PipVariant)i) + ".png"));
            }
            toy.SetVariant(PipVariant.PeachJelly);
            toy.ResetToy();
            foreach (Transform child in toyObject.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
            yield return null;
            camera.backgroundColor = PrototypeStage.Background;
            Capture(camera, Path.Combine(directory, "PeachJelly-front-reference.png"));
            toy.SetDeformation(.18f, .3f, .15f, -.15f, .2f, -.2f, .22f, new Vector3(1, 1, .1f));
            foreach (Transform child in toyObject.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
            yield return null;
            Assert.That(toy.RaycastBody(new Ray(mount.position + new Vector3(0, -.4f, -3), Vector3.forward), out _), Is.True);
            camera.backgroundColor = PockleTheme.FieldFor(PipVariants.CollectibleId(PipVariant.PeachJelly));
            Capture(camera, Path.Combine(directory, "PeachJelly-grip.png"), 2.15f);
            Debug.Log("PIP_JELLY_CAPTURES " + directory + " · " + SystemInfo.graphicsDeviceName);
        }

        private void Capture(Camera camera, string path, float framing = 1.6f)
        {
            RenderTexture previous = RenderTexture.active;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target; camera.rect = new Rect(0, 0, 1, 1);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.orthographicSize = framing;
                // Front reference: near-level view with a small downward angle,
                // neutral cream field and enough margin to compare the whole toy.
                camera.transform.position = new Vector3(0, 1.65f, -6f);
                camera.transform.LookAt(new Vector3(0, 1.12f, 0));
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
                // A compact review preview also travels in the CI log when a connector
                // can return an artifact reference but cannot open the ZIP locally.
                // RGB rows are top-down; full-resolution PNGs stay in the artifact.
                const int previewWidth = 240, previewHeight = 320;
                Color32[] pixels = image.GetPixels32();
                var rgb = new byte[previewWidth * previewHeight * 3];
                for (int y = 0; y < previewHeight; y++)
                for (int x = 0; x < previewWidth; x++)
                {
                    Color32 pixel = pixels[(target.height - 1 - y * 3) * target.width + x * 3];
                    int offset = (y * previewWidth + x) * 3;
                    rgb[offset] = pixel.r; rgb[offset + 1] = pixel.g; rgb[offset + 2] = pixel.b;
                }
                using (var compressed = new MemoryStream())
                {
                    using (var gzip = new System.IO.Compression.GZipStream(compressed, System.IO.Compression.CompressionLevel.Fastest, true)) gzip.Write(rgb, 0, rgb.Length);
                    Debug.Log("PIP_JELLY_PREVIEW " + Path.GetFileNameWithoutExtension(path) + " " + previewWidth + " " + previewHeight + " " + System.Convert.ToBase64String(compressed.ToArray()));
                }
                // Reject an empty render; aesthetic review is deliberately manual.
                Assert.That(Vector4.Distance(image.GetPixel(360, 450), image.GetPixel(20, 940)), Is.GreaterThan(.05f));
            }
            finally { camera.targetTexture = null; RenderTexture.active = previous; Object.Destroy(image); }
        }
    }
}
