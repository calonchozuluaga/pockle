using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class ExpressionTests
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
            root = new GameObject("Face test");
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
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
        public IEnumerator EachFinishKeepsFaceAttachedAndReusesResourcesAcrossReactions()
        {
            JellyToy toy = root.AddComponent<JellyToy>();
            Assert.That(toy.UsesAuthoredMesh, Is.True);
            int resources = toy.VisualRoot.GetComponentsInChildren<Renderer>(true).Length;
            Mesh body = toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
            Transform smile = toy.VisualRoot.Find("Tiny smile");
            Transform left = toy.VisualRoot.Find("Left soft eye");
            Transform glint = toy.VisualRoot.Find("Cream eye glint");
            var lid = toy.VisualRoot.Find("Left closed eyelid").GetComponent<LineRenderer>();
            var interior = toy.VisualRoot.Find("Open smile interior").GetComponent<MeshRenderer>();
            Mesh mouthMesh = interior.GetComponent<MeshFilter>().sharedMesh;
            foreach (PipVariant variant in new[] { PipVariant.PeachJelly, PipVariant.MoonJelly, PipVariant.GoldGlitter, PipVariant.MintSoft })
            {
                toy.SetVariant(variant);
                foreach (FaceExpression expression in new[] { FaceExpression.Happy, FaceExpression.Curious, FaceExpression.Sleepy, FaceExpression.Delighted })
                {
                    toy.SetDeformation(.25f, .15f, .1f, -.1f, .2f, -.2f, .12f, new Vector3(.7f, .7f, 0), .3f, new Vector3(0, .6f, -.3f), .6f);
                    toy.SetFacePose(FacePose.For(expression));
                    CheckMarksInFront(toy, smile.GetComponent<LineRenderer>());
                    if (expression == FaceExpression.Sleepy || expression == FaceExpression.Delighted) CheckMarksInFront(toy, lid);
                    Vector3 first = smile.GetComponent<LineRenderer>().GetPosition(0);
                    Assert.That(float.IsNaN(first.x) || float.IsInfinity(first.z), Is.False);
                    bool closed = expression == FaceExpression.Sleepy || expression == FaceExpression.Delighted;
                    Assert.That(left.gameObject.activeSelf, Is.EqualTo(!closed));
                    Assert.That(glint.gameObject.activeSelf, Is.EqualTo(!closed));
                    Assert.That(lid.enabled, Is.EqualTo(closed));
                    Assert.That(interior.enabled, Is.EqualTo(expression == FaceExpression.Curious || expression == FaceExpression.Delighted));
                    Assert.That(toy.VisualRoot.Find("Little pink tongue").gameObject.activeSelf, Is.EqualTo(expression == FaceExpression.Delighted));
                    toy.transform.localRotation = Quaternion.Euler(0, 95, 0);
                    toy.transform.localPosition = Vector3.up * .4f;
                    // Changing only the face must work even when body deformation is unchanged.
                    toy.SetFacePose(FacePose.For(expression));
                    Assert.That(smile.GetComponent<LineRenderer>().GetPosition(0), Is.EqualTo(first));
                    Assert.That(toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh, Is.SameAs(body));
                    Assert.That(interior.GetComponent<MeshFilter>().sharedMesh, Is.SameAs(mouthMesh));
                    yield return null;
                }
            }
            toy.SetVariant(PipVariant.PeachJelly);
            yield return null; // Deferred destruction must finish before counting resources.
            Assert.That(toy.VisualRoot.GetComponentsInChildren<Renderer>(true).Length, Is.EqualTo(resources));
            toy.SetDeformation(0, 0, 0, 0, 0, 0);
            toy.SetFacePose(FacePose.For(FaceExpression.Happy));
            var asset = Resources.Load<PipCharacterAsset>(PipCharacterAsset.ResourcePath);
            for (int i = 0; i < 13; i++) Assert.That(Vector3.Distance(smile.GetComponent<LineRenderer>().GetPosition(i), asset.Mouth[i]), Is.LessThan(.00001f));
        }

        private static void CheckMarksInFront(JellyToy toy, LineRenderer mark)
        {
            for (int i = 0; i < mark.positionCount; i++)
            {
                Vector3 point = mark.GetPosition(i);
                var ray = new Ray(toy.VisualRoot.TransformPoint(new Vector3(point.x, point.y, -3)), toy.VisualRoot.forward);
                Assert.That(toy.RaycastBody(ray, out Vector3 hit), Is.True, "Facial mark escaped the visible body.");
                float front = toy.VisualRoot.InverseTransformPoint(hit).z;
                Assert.That(front - point.z, Is.GreaterThan(.003f), "Facial mark sank into the deformed shell.");
            }
        }

        [UnityTest]
        public IEnumerator ExpressionsSurviveCharacterRebuildWithoutRetainingOwnedMeshes()
        {
            JellyToy toy = root.AddComponent<JellyToy>();
            toy.SetFacePose(FacePose.For(FaceExpression.Delighted));
            Mesh old = toy.VisualRoot.Find("Open smile interior").GetComponent<MeshFilter>().sharedMesh;
            Assert.That(toy.TrySetCollectible(CharacterArt.NookPlushStudyId), Is.True);
            yield return null;
            Assert.That(old == null, Is.True, "Old expression mesh was retained.");
            Assert.That(toy.ExpressionPose.Delighted, Is.EqualTo(1));
            toy.SetDeformation(.1f, 0, .1f, 0, 0, 0);
            Assert.That(toy.VisualRoot.Find("Left closed eyelid").GetComponent<LineRenderer>().enabled, Is.True);
            Assert.That(toy.TrySetCollectible(ToyCatalog.PeachId), Is.True);
            yield return null;
            toy.SetFacePose(FacePose.For(FaceExpression.Curious));
            Assert.That(toy.VisualRoot.Find("Open smile interior").GetComponent<MeshRenderer>().enabled, Is.True);
        }
    }
}
