using System.Collections;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class PipVariantTests
    {
        private int savedBaseline;
        private int savedVariant;
        private bool hadBaseline;
        private bool hadVariant;
        private bool hadMeshy;
        private int meshy;
        private GameObject root;

        [SetUp]
        public void SavePreferences()
        {
            hadMeshy = PlayerPrefs.HasKey(PipCharacterAsset.MeshyPreference);
            meshy = PlayerPrefs.GetInt(PipCharacterAsset.MeshyPreference);
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, 0);
            hadBaseline = PlayerPrefs.HasKey(PipCharacterAsset.BaselinePreference);
            hadVariant = PlayerPrefs.HasKey(PipVariants.Preference);
            savedBaseline = PlayerPrefs.GetInt(PipCharacterAsset.BaselinePreference, 0);
            savedVariant = PlayerPrefs.GetInt(PipVariants.Preference, 0);
            PlayerPrefs.SetInt(PipCharacterAsset.BaselinePreference, 0);
            PlayerPrefs.SetInt(PipVariants.Preference, 0);
        }

        [UnityTearDown]
        public IEnumerator RestorePreferences()
        {
            if (root != null) Object.Destroy(root);
            if (hadMeshy) PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, meshy);
            else PlayerPrefs.DeleteKey(PipCharacterAsset.MeshyPreference);
            if (hadBaseline) PlayerPrefs.SetInt(PipCharacterAsset.BaselinePreference, savedBaseline);
            else PlayerPrefs.DeleteKey(PipCharacterAsset.BaselinePreference);
            if (hadVariant) PlayerPrefs.SetInt(PipVariants.Preference, savedVariant);
            else PlayerPrefs.DeleteKey(PipVariants.Preference);
            PlayerPrefs.Save();
            yield return null;
        }

        [UnityTest]
        public IEnumerator RepeatedSwitchesKeepPoseMeshAndMaterialWithoutOldFilling()
        {
            root = new GameObject("Variant test Pip");
            JellyToy toy = root.AddComponent<JellyToy>();
            Assert.That(toy.UsesAuthoredMesh, Is.True);
            toy.SetDeformation(.18f, .24f, .1f, -.1f, .1f, -.1f, .25f, new Vector3(1f, 1f, .1f),
                .18f, new Vector3(.2f, .3f, -.5f), .4f);
            Mesh mesh = toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
            Material material = toy.BodyRenderer.sharedMaterial;
            var ray = new Ray(new Vector3(0f, -.4f, -3f), Vector3.forward);
            Assert.That(toy.RaycastBody(ray, out Vector3 before), Is.True);
            for (int i = 0; i < 16; i++)
            {
                PipVariant choice = (PipVariant)((i + 1) % PipVariants.Count);
                bool moon = choice == PipVariant.MoonJelly;
                bool peach = choice == PipVariant.PeachJelly;
                bool soft = choice == PipVariant.MintSoft;
                toy.SetVariant(choice);
                yield return null; // Old pieces are destroyed at the frame boundary.
                Assert.That(toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh, Is.SameAs(mesh));
                Assert.That(toy.BodyRenderer.sharedMaterial, Is.SameAs(material));
                Assert.That(toy.RaycastBody(ray, out Vector3 after), Is.True);
                Assert.That(Vector3.Distance(before, after), Is.LessThan(.00001f), "Variant selection must preserve deformation.");
                int stars = 0, peachPearls = 0, moonPearls = 0;
                foreach (Transform part in root.GetComponentsInChildren<Transform>(true))
                {
                    if (part.name.StartsWith("Suspended silver star")) stars++;
                    if (part.name.StartsWith("Suspended pearl")) peachPearls++;
                    if (part.name.StartsWith("Moon pearl")) moonPearls++;
                }
                Assert.That(stars, Is.EqualTo(moon ? 8 : 0));
                Assert.That(moonPearls, Is.EqualTo(moon ? 3 : 0));
                Assert.That(peachPearls, Is.EqualTo(peach ? 6 : 0));
                Assert.That(material.GetFloat("_GlitterStrength") > 0f, Is.EqualTo(choice == PipVariant.GoldGlitter));
                Assert.That(material.GetFloat("_Softness"), Is.EqualTo(soft ? 1f : 0f));
                Assert.That(material.GetFloat("_ZWrite"), Is.EqualTo(soft ? 1f : 0f));
                Assert.That(material.renderQueue, Is.EqualTo(soft ? 2001 : 3000));
                Assert.That(material.GetFloat("_StudioStrength") > 0f, Is.EqualTo(!soft));
                Assert.That(material.GetFloat("_Transmission"), Is.EqualTo(soft ? .04f : .14f).Within(.00001f));
                Assert.That(material.GetFloat("_EdgeLight"), Is.EqualTo(soft ? .12f : .15f).Within(.00001f));
                Assert.That(material.color.a, Is.EqualTo(soft ? 1f : peach ? .44f : moon ? .54f : .58f).Within(.00001f));
            }
            Object.Destroy(root);
            yield return null;
            yield return null;
            Assert.That(mesh == null, Is.True, "Owned mesh must be released with Pip.");
            Assert.That(material == null, Is.True, "Owned material must be released with Pip.");
        }

        [UnityTest]
        public IEnumerator MeshyPeachDefaultsOnAndSwitchesBackWithoutLosingTheActivePose()
        {
            PlayerPrefs.DeleteKey(PipCharacterAsset.MeshyPreference);
            root = new GameObject("Meshy comparison test Pip");
            JellyToy toy = root.AddComponent<JellyToy>();
            Assert.That(toy.UsesMeshyPeach, Is.True, "Fresh Android preferences must reach Meshy through normal Peach play.");
            Assert.That(toy.BodyRenderer.sharedMaterial.mainTexture, Is.Not.Null);
            Assert.That(toy.Handling, Is.SameAs(Pockle.Core.MaterialHandling.Gel));
            Assert.That(toy.GetComponentsInChildren<MeshRenderer>().Length, Is.EqualTo(1), "No duplicate eyes, crown or pearls over the painted mesh.");
            toy.SetDeformation(.18f, .24f, .1f, -.1f, .1f, -.1f, .25f, new Vector3(1,1,.1f),
                .18f, new Vector3(.2f,.3f,-.5f), .4f);
            Mesh originalMesh = toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
            Material originalMaterial = toy.BodyRenderer.sharedMaterial;
            Vector3[] expected = originalMesh.vertices;
            var ray = new Ray(new Vector3(0,-.4f,-3), Vector3.forward);
            Assert.That(toy.RaycastBody(ray, out _), Is.True);
            for (int i = 0; i < 3; i++)
            {
                toy.SetVariant(PipVariant.MoonJelly);
                Assert.That(toy.UsesMeshyPeach, Is.False);
                Assert.That(toy.BodyRenderer.sharedMaterial.shader.name, Is.EqualTo("Pockle/Jelly Candy"));
                toy.SetVariant(PipVariant.PeachJelly);
                Assert.That(toy.UsesMeshyPeach, Is.True);
                Assert.That(toy.RaycastBody(ray, out _), Is.True);
                Vector3[] actual = toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh.vertices;
                Assert.That(actual.Length, Is.EqualTo(expected.Length));
                for (int v = 0; v < actual.Length; v++)
                    Assert.That(Vector3.Distance(actual[v], expected[v]), Is.LessThan(.00001f), "Art switching must replay the raw grip pose.");
                yield return null;
                Assert.That(toy.GetComponentsInChildren<MeshRenderer>().Length, Is.EqualTo(1));
            }
            Assert.That(originalMesh == null, Is.True);
            Assert.That(originalMaterial == null, Is.True);
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, 0);
            toy.SetVariant(PipVariant.PeachJelly);
            Assert.That(toy.UsesMeshyPeach, Is.False, "The prior authored comparison remains selectable.");
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, 1);
            toy.SetVariant(PipVariant.PeachJelly);
            Mesh finalMesh = toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
            Material finalMaterial = toy.BodyRenderer.sharedMaterial;
            toy.ResetToy();
            Vector3[] rest = Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath).BodyMesh.vertices;
            Vector3[] reset = finalMesh.vertices;
            for (int v = 0; v < rest.Length; v++) Assert.That(Vector3.Distance(rest[v],reset[v]), Is.LessThan(.00001f));
            Object.Destroy(root);
            yield return null;
            yield return null;
            Assert.That(finalMesh == null && finalMaterial == null, Is.True);
        }

        [UnityTest]
        public IEnumerator StartupUsesTheSavedVariant()
        {
            for (int i = 0; i <= PipVariants.Count; i++)
            {
                int saved = i == PipVariants.Count ? 99 : i;
                PlayerPrefs.SetInt(PipVariants.Preference, saved);
                root = new GameObject("Saved variant test Pip");
                JellyToy toy = root.AddComponent<JellyToy>();
                Assert.That(toy.Variant, Is.EqualTo(saved == 99 ? PipVariant.PeachJelly : (PipVariant)saved));
                Object.Destroy(root);
                yield return null;
                yield return null;
            }
        }
    }
}
