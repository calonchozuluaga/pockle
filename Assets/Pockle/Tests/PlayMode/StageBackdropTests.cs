using System.Collections;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class StageBackdropTests
    {
        private GameObject stage;
        private GameObject otherCamera;
        private Color ambientLight;
        private UnityEngine.Rendering.AmbientMode ambientMode;
        private bool fog;

        [SetUp]
        public void SaveSceneSettings()
        {
            ambientLight = RenderSettings.ambientLight;
            ambientMode = RenderSettings.ambientMode;
            fog = RenderSettings.fog;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (stage != null) Object.Destroy(stage);
            if (otherCamera != null) Object.Destroy(otherCamera);
            yield return null;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.fog = fog;
        }

        [UnityTest]
        public IEnumerator ColourFieldSurvivesFramingAndResetWithoutChangingOtherCameras()
        {
            PrototypeStage.Create(out Camera viewer, out _, out _, out _, out _);
            stage = viewer.transform.parent.gameObject;
            otherCamera = new GameObject("Unrelated portrait camera", typeof(Camera));
            var portrait = otherCamera.GetComponent<Camera>();
            portrait.enabled = false;
            portrait.backgroundColor = Color.blue;
            Camera clearCamera = null;
            foreach (var candidate in stage.GetComponentsInChildren<Camera>())
                if (candidate != viewer) clearCamera = candidate;
            Assert.That(clearCamera, Is.Not.Null);

            var field = new Color(.82f, .55f, .71f, .2f);
            Assert.That(PrototypeStage.SetBackdrop(viewer, field), Is.True);
            yield return null; // StageFraming changes the viewport/clear flags here.
            var expected = new Color(field.r, field.g, field.b, 1f);
            Assert.That(viewer.backgroundColor, Is.EqualTo(expected));
            Assert.That(clearCamera.backgroundColor, Is.EqualTo(expected));
            Assert.That(clearCamera.enabled, Is.True);
            Assert.That(portrait.backgroundColor, Is.EqualTo(Color.blue));
            Assert.That(PrototypeStage.SetBackdrop(portrait, Color.red), Is.False);
            Assert.That(PrototypeStage.SetBackdrop(viewer, new Color(float.NaN, 0, 0)), Is.False);
            Assert.That(viewer.backgroundColor, Is.EqualTo(expected));
            Assert.That(clearCamera.backgroundColor, Is.EqualTo(expected));

            Assert.That(PrototypeStage.SetBackdrop(viewer, new Color(2f, -1f, .5f)), Is.True);
            Assert.That(clearCamera.backgroundColor, Is.EqualTo(new Color(1f, 0f, .5f, 1f)));
            Assert.That(PrototypeStage.SetBackdrop(viewer, PrototypeStage.Background), Is.True);
            Assert.That(clearCamera.backgroundColor, Is.EqualTo(PrototypeStage.Background));
            Assert.That(viewer.backgroundColor, Is.EqualTo(PrototypeStage.Background));
            Object.Destroy(stage);
            yield return null;
            Assert.That(PrototypeStage.SetBackdrop(viewer, Color.red), Is.False);
        }
    }
}
