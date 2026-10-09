using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class CharacterStudyTests
    {
        private GameObject root;
        private bool hadBaseline, hadVariant;
        private int baseline, variant;

        [SetUp]
        public void Setup()
        {
            hadBaseline = PlayerPrefs.HasKey(PipCharacterAsset.BaselinePreference);
            hadVariant = PlayerPrefs.HasKey(PipVariants.Preference);
            baseline = PlayerPrefs.GetInt(PipCharacterAsset.BaselinePreference);
            variant = PlayerPrefs.GetInt(PipVariants.Preference);
            PlayerPrefs.SetInt(PipCharacterAsset.BaselinePreference, 0);
            PlayerPrefs.SetInt(PipVariants.Preference, 0);
            root = new GameObject("Study fixture");
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            Object.Destroy(root);
            if (hadBaseline) PlayerPrefs.SetInt(PipCharacterAsset.BaselinePreference, baseline);
            else PlayerPrefs.DeleteKey(PipCharacterAsset.BaselinePreference);
            if (hadVariant) PlayerPrefs.SetInt(PipVariants.Preference, variant);
            else PlayerPrefs.DeleteKey(PipVariants.Preference);
            PlayerPrefs.Save();
            yield return null;
        }

        [UnityTest]
        public IEnumerator StudySwitchingReleasesOldAssetsAndRejectsMissingArt()
        {
            var toy = root.AddComponent<JellyToy>();
            foreach (string id in new[] { CharacterArt.MossStudyId, CharacterArt.BopStudyId, ToyCatalog.PeachId, CharacterArt.MossStudyId })
            {
                Mesh previous = toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
                Material oldMaterial = toy.BodyRenderer.sharedMaterial;
                Assert.That(toy.TrySetCollectible(id), Is.True);
                yield return null;
                Assert.That(previous == null && oldMaterial == null, Is.True, "Rebuilding retained owned native assets.");
                Assert.That(toy.CollectibleId, Is.EqualTo(id));
                var renderer = toy.BodyRenderer;
                Mesh mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(mesh.vertexCount, Is.LessThanOrEqualTo(4000));
                Assert.That(mesh.triangles.Length / 3, Is.LessThanOrEqualTo(6000));
                Assert.That(toy.TrySetCollectible("dew.clear-jelly"), Is.False);
                Assert.That(toy.BodyRenderer, Is.SameAs(renderer));
                Assert.That(toy.CollectibleId, Is.EqualTo(id));
                Assert.That(toy.RaycastBody(new Ray(new Vector3(0, 0, -3), Vector3.forward), out _), Is.True);
                int activeRoots = 0;
                foreach (Transform child in root.transform) if (child.gameObject.activeSelf) activeRoots++;
                Assert.That(activeRoots, Is.EqualTo(1));
            }
            toy.SetVariant(PipVariant.MoonJelly);
            yield return null;
            Assert.That(toy.CollectibleId, Is.EqualTo(ToyCatalog.MoonId));
            Assert.That(toy.Handling, Is.SameAs(MaterialHandling.Gel));
        }

        [UnityTest]
        public IEnumerator VinylKeepsItsShapeWhileFlockGivesAndStudiesStayOutOfRewards()
        {
            var toy = root.AddComponent<JellyToy>();
            float[] shifts = new float[2];
            string[] ids = { CharacterArt.MossStudyId, CharacterArt.BopStudyId };
            for (int i = 0; i < ids.Length; i++)
            {
                Assert.That(toy.TrySetCollectible(ids[i]), Is.True);
                Mesh mesh = toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
                Vector3[] rest = mesh.vertices;
                toy.SetDeformation(.4f, .55f, 0, 0, 0, 0, .45f, Vector3.up, .45f, new Vector3(0, .5f, -.5f), .7f);
                Vector3[] posed = mesh.vertices;
                for (int v = 0; v < posed.Length; v++) shifts[i] = Mathf.Max(shifts[i], Vector3.Distance(rest[v], posed[v]));
                Assert.That(toy.BodyRenderer.sharedMaterial.shader.name, Is.EqualTo("Pockle/Solid Toy"));
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Flock"), Is.EqualTo(i == 0 ? 1 : 0));
                toy.ResetToy();
                Vector3[] reset = mesh.vertices;
                for (int v = 0; v < reset.Length; v++)
                    Assert.That(Vector3.Distance(rest[v], reset[v]), Is.LessThan(.000002f));
                Assert.That(ToyCatalog.TryGetCollectible(ids[i], out var definition), Is.True);
                Assert.That(definition.Available, Is.False, "Draft study became production content.");
                Assert.That(ToyCatalog.IsDailyEligible(ids[i]), Is.False);
            }
            Assert.That(shifts[0], Is.GreaterThan(shifts[1] * 5), "Vinyl should feel much firmer than flock.");
            yield return null;
        }
    }
}
