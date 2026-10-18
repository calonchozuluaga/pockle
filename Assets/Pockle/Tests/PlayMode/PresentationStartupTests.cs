using System.Collections;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class PresentationStartupTests
    {
        private GameObject stage, carton;
        private Color ambient;
        private UnityEngine.Rendering.AmbientMode ambientMode;
        private bool fog;

        [SetUp]
        public void SaveSceneSettings()
        { ambient = RenderSettings.ambientLight; ambientMode = RenderSettings.ambientMode; fog = RenderSettings.fog; }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            if (stage != null) Object.Destroy(stage);
            if (carton != null) Object.Destroy(carton);
            yield return null;
            yield return null;
            RenderSettings.ambientLight = ambient; RenderSettings.ambientMode = ambientMode; RenderSettings.fog = fog;
        }

        [UnityTest]
        public IEnumerator StartupHasNoPlateAndReleasesOwnedGeometry()
        {
            PrototypeStage.Create(out Camera viewer, out _, out _, out Collider plate, out _);
            stage = viewer.transform.parent.gameObject;
            Assert.That(stage.GetComponentsInChildren<Collider>(), Is.Empty);
            Assert.That(plate, Is.Null);
            var filters = stage.GetComponentsInChildren<MeshFilter>();
            Assert.That(filters.Length, Is.EqualTo(1));
            Mesh disk = filters[0].sharedMesh;

            carton = new GameObject("Startup test carton");
            var box = carton.AddComponent<MysteryBox>();
            box.Initialize("legacy"); box.Initialize("legacy");
            Assert.That(carton.GetComponentsInChildren<Collider>(), Is.Empty);
            Transform floor = carton.transform.Find("Carton floor");
            Assert.That(floor, Is.Not.Null);
            Mesh floorMesh = floor.GetComponent<MeshFilter>().sharedMesh;
            Assert.That(floorMesh.triangles.Length, Is.EqualTo(36));
            Assert.That(floorMesh.bounds.size, Is.EqualTo(Vector3.one));
            Assert.That(floor.GetComponent<MeshRenderer>().sharedMaterial, Is.Not.Null);
            foreach (int index in floorMesh.triangles) Assert.That(index, Is.InRange(0, floorMesh.vertexCount - 1));
            // Every face is nondegenerate and points outwards, including the plate's caps.
            foreach (Mesh mesh in new[] { disk, floorMesh })
            {
                Vector3[] vertices = mesh.vertices;
                int[] triangles = mesh.triangles;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    Vector3 a = vertices[triangles[i]], b = vertices[triangles[i + 1]], c = vertices[triangles[i + 2]];
                    Assert.That(Vector3.Dot(Vector3.Cross(b - a, c - a), (a + b + c) / 3f), Is.GreaterThan(0f));
                }
            }
            Object.Destroy(stage); Object.Destroy(carton);
            yield return null;
            yield return null;
            Assert.That(disk == null, Is.True); Assert.That(floorMesh == null, Is.True);
        }
    }
}
