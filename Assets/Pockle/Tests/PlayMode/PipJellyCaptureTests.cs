using System.Collections;
using System.IO;
using NUnit.Framework;
using Pockle.Runtime;
using Pockle.Runtime.UI;
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
                Assert.That(toy.RaycastBody(new Ray(mount.position + new Vector3(.75f, -.95f, -3), Vector3.forward), out _), Is.False,
                    "The old wide foot must not remain pickable.");
                camera.backgroundColor = PockleTheme.FieldFor(PipVariants.CollectibleId((PipVariant)i));
                Capture(camera, Path.Combine(directory, ((PipVariant)i) + ".png"));
            }
            toy.SetVariant(PipVariant.PeachJelly);
            toy.SetDeformation(.18f, .3f, .15f, -.15f, .2f, -.2f, .22f, new Vector3(1, 1, .1f));
            foreach (Transform child in toyObject.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 31;
            yield return null;
            Assert.That(toy.RaycastBody(new Ray(mount.position + new Vector3(0, -.4f, -3), Vector3.forward), out _), Is.True);
            camera.backgroundColor = PockleTheme.FieldFor(PipVariants.CollectibleId(PipVariant.PeachJelly));
            Capture(camera, Path.Combine(directory, "PeachJelly-grip.png"));
            Debug.Log("PIP_JELLY_CAPTURES " + directory + " · " + SystemInfo.graphicsDeviceName);
        }

        private void Capture(Camera camera, string path)
        {
            RenderTexture previous = RenderTexture.active;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                camera.targetTexture = target; camera.rect = new Rect(0, 0, 1, 1);
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.orthographicSize = 2.15f;
                camera.transform.LookAt(new Vector3(0, 1.45f, 0));
                camera.Render();
                RenderTexture.active = target;
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
                // Reject an empty render; aesthetic review is deliberately manual.
                Assert.That(Vector4.Distance(image.GetPixel(360, 450), image.GetPixel(20, 940)), Is.GreaterThan(.05f));
            }
            finally { camera.targetTexture = null; RenderTexture.active = previous; Object.Destroy(image); }
        }
    }
}
