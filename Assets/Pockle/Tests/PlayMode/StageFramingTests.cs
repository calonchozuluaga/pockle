using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class StageFramingTests
    {
        private Color ambient;
        private UnityEngine.Rendering.AmbientMode ambientMode;
        private bool fog;
        [SetUp]
        public void SaveSettings()
        { ambient = RenderSettings.ambientLight; ambientMode = RenderSettings.ambientMode; fog = RenderSettings.fog; }
        [TearDown]
        public void RestoreSettings()
        { RenderSettings.ambientLight = ambient; RenderSettings.ambientMode = ambientMode; RenderSettings.fog = fog; }
        [UnityTest]
        public IEnumerator FramingMovesAndScalesWithoutCroppingRenderingOrPicking()
        {
            PrototypeStage.Create(out var camera, out _, out _, out _, out _);
            var stage = camera.transform.parent.gameObject;
            var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var material = new Material(Shader.Find("Unlit/Color")) { color = Color.magenta };
            marker.GetComponent<Renderer>().sharedMaterial = material;
            RenderTexture target = null;
            Texture2D pixels = null;
            var oldTarget = RenderTexture.active;
            try
            {
                var area = new Rect(Screen.width * .1f, Screen.height * .35f, Screen.width * .8f, Screen.height * .3f);
                PrototypeStage.SetViewport(camera, area);
                yield return null;
                yield return new WaitForSecondsRealtime(.7f);
                Assert.That(camera.rect, Is.EqualTo(new Rect(0, 0, 1, 1)));
                var centre = new Vector3(0, 1.15f, 0);
                var projected = camera.WorldToScreenPoint(centre);
                Assert.That(projected.x, Is.EqualTo(area.center.x).Within(1));
                Assert.That(projected.y, Is.EqualTo(area.center.y).Within(1));
                float halfHeight = ToyOrbit.FrameSize(area.width / area.height, 1, false);
                var top = camera.WorldToScreenPoint(centre + camera.transform.up * halfHeight);
                Assert.That(top.y, Is.EqualTo(area.yMax).Within(1), "Available HUD area still determines toy scale.");

                // A visible fragment outside the framing area would previously be cut off by camera.rect.
                Vector3 screen = new Vector3(Screen.width * .5f, Screen.height * .8f, 6);
                marker.transform.position = camera.ScreenToWorldPoint(screen);
                marker.transform.rotation = camera.transform.rotation;
                marker.transform.localScale = Vector3.one * .35f;
                Physics.SyncTransforms();
                Ray ray = camera.ScreenPointToRay(screen);
                Assert.That(marker.GetComponent<Collider>().Raycast(ray, out _, 20), Is.True, "Picking follows the shifted full-screen projection.");
                target = new RenderTexture(Screen.width, Screen.height, 24);
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                pixels = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
                pixels.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); pixels.Apply();
                Color rendered = pixels.GetPixel((int)screen.x, (int)screen.y);
                Assert.That(rendered.r, Is.GreaterThan(.9f));
                Assert.That(rendered.b, Is.GreaterThan(.9f));
                Assert.That(rendered.g, Is.LessThan(.1f), "Full-screen rendering must retain fragments above the HUD framing rect.");
                camera.targetTexture = null;

                PrototypeStage.SetZoom(camera, .65f);
                yield return null;
                Assert.That(camera.orthographicSize, Is.EqualTo(halfHeight * .65f / .3f).Within(.01f));
                PrototypeStage.SetViewport(camera, new Rect(float.NaN, 0, 0, 0));
                yield return null;
                Assert.That(camera.rect, Is.EqualTo(new Rect(0, 0, 1, 1)));
                Assert.That(camera.WorldToScreenPoint(centre).y, Is.EqualTo(area.center.y).Within(1));
            }
            finally
            {
                RenderTexture.active = oldTarget;
                camera.targetTexture = null;
                Object.Destroy(stage); Object.Destroy(marker); Object.Destroy(material);
                if (target != null) Object.Destroy(target);
                if (pixels != null) Object.Destroy(pixels);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator HistoricalStepsDoNotClaimAnActiveSensorInTheEditor()
        {
            const string key = "pockle.collection.v1";
            bool hadSave = PlayerPrefs.HasKey(key);
            string previous = PlayerPrefs.GetString(key);
            var root = new GameObject("Walking API test");
            try
            {
                var save = new CollectionProgress(null, CollectionSession.Today).Save;
                save.Steps = 500;
                PlayerPrefs.SetString(key, JsonUtility.ToJson(save));
                var session = root.AddComponent<CollectionSession>(); session.Initialize();
                Assert.That(session.Progress.Save.Steps, Is.EqualTo(500));
                Assert.That(session.WalkingActive, Is.False, "Saved steps and status text are not sensor registration.");
                session.EnableWalking();
                Assert.That(session.WalkingActive, Is.False, "Editor preview never pretends to start an Android sensor.");
            }
            finally
            {
                Object.DestroyImmediate(root);
                if (hadSave) PlayerPrefs.SetString(key, previous); else PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
            yield return null;
        }
    }
}
