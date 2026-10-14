using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class PeachAppearanceTests
    {
        [UnityTest]
        public IEnumerator ApprovedPeachHasExpressionsAndInternalFillingWithUnchangedSourceMesh()
        {
            bool had = PlayerPrefs.HasKey(PipCharacterAsset.MeshyPreference); int previous = PlayerPrefs.GetInt(PipCharacterAsset.MeshyPreference);
            bool hadCalm = PlayerPrefs.HasKey("pockle.prototype.reducedMotion"); int calm = PlayerPrefs.GetInt("pockle.prototype.reducedMotion");
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, 1);
            var root = new GameObject("Approved Peach expression test");
            try
            {
                var toy = root.AddComponent<JellyToy>(); toy.Initialize(); toy.SetVariant(PipVariant.PeachJelly);
                Assert.That(toy.UsesMeshyPeach, Is.True);
                var source = Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath);
                Vector3[] original = source.BodyMesh.vertices;
                var material = toy.BodyRenderer.sharedMaterial;
                Assert.That(material.shader.isSupported, Is.True);
                Assert.That(material.renderQueue, Is.EqualTo(3000));
                Assert.That(material.GetFloat("_Opacity"), Is.InRange(.25f,.8f));
                Assert.That(material.GetFloat("_ReflectionStrength"), Is.GreaterThan(.6f));
                Assert.That(toy.VisualRoot.Cast<Transform>().Count(t => t.name.StartsWith("Suspended pearl")), Is.EqualTo(6));
                var driver = toy.VisualRoot.GetComponent<PeachExpressionDriver>(); Assert.That(driver, Is.Not.Null);
                driver.Apply(new PeachExpressionPose(1,0,0)); Assert.That(material.GetFloat("_Blink"), Is.EqualTo(1));
                driver.Apply(new PeachExpressionPose(0,1,0)); Assert.That(material.GetFloat("_Happy"), Is.EqualTo(1));
                driver.Apply(new PeachExpressionPose(0,0,1)); Assert.That(material.GetFloat("_Surprise"), Is.EqualTo(1));
                CollectionAssert.AreEqual(original, source.BodyMesh.vertices, "Expressions modified the approved imported model.");
                PlayerPrefs.SetInt("pockle.prototype.reducedMotion", 1); toy.SetExpressionContext(.3f,.8f);
                yield return null; yield return null;
                Assert.That(material.GetFloat("_Happy"), Is.Zero); Assert.That(material.GetFloat("_Surprise"), Is.Zero);
                Assert.That(material.GetFloat("_Blink"), Is.Zero);
                toy.SetVariant(PipVariant.MoonJelly);
                Assert.That(toy.VisualRoot.GetComponent<PeachExpressionDriver>(), Is.Null, "Unapproved Pip finish gained Peach facial animation.");
            }
            finally
            {
                Object.Destroy(root);
                if (had) PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, previous); else PlayerPrefs.DeleteKey(PipCharacterAsset.MeshyPreference);
                if (hadCalm) PlayerPrefs.SetInt("pockle.prototype.reducedMotion", calm); else PlayerPrefs.DeleteKey("pockle.prototype.reducedMotion");
            }
            yield return null;
        }
    }
}
