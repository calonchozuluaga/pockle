using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;

namespace Pockle.Tests
{
    public sealed class AuthoredPipTests
    {
        private static PipCharacterAsset Load()
        {
            PipCharacterAsset asset = Resources.Load<PipCharacterAsset>(PipCharacterAsset.ResourcePath);
            Assert.That(asset, Is.Not.Null, "Pip.pocklemesh must import as a native character asset.");
            Assert.That(asset.BodyMesh, Is.Not.Null);
            return asset;
        }

        [Test]
        public void AuthoredBodyIsReadableAndHasMatchingUvsAndWeldGroups()
        {
            PipCharacterAsset asset = Load();
            Assert.That(asset.BodyMesh.isReadable, Is.True);
            Assert.That(asset.BodyMesh.vertexCount, Is.GreaterThan(1000));
            Assert.That(asset.BodyMesh.uv.Length, Is.EqualTo(asset.BodyMesh.vertexCount));
            Assert.That(asset.NormalGroups.Length, Is.EqualTo(asset.BodyMesh.vertexCount));
            foreach (int group in asset.NormalGroups)
                Assert.That(group, Is.InRange(0, asset.BodyMesh.vertexCount - 1));
            Assert.That(asset.BodyMesh.bounds.min.y, Is.EqualTo(-1f).Within(.00001f));
            Assert.That(asset.IntegratedCrown, Is.True);
            Assert.That(asset.BodyMesh.bounds.max.y, Is.InRange(.9f, 1f));
        }

        [Test]
        public void AuthoredAnchorsFitTheFaceAndFullStretchViewer()
        {
            PipCharacterAsset asset = Load();
            Assert.That(asset.Eyes.Length, Is.EqualTo(2));
            Assert.That(asset.EyeGlints.Length, Is.EqualTo(2));
            Assert.That(asset.Cheeks.Length, Is.EqualTo(2));
            Assert.That(asset.Mouth.Length, Is.EqualTo(13));
            Assert.That(asset.Crowns.Length, Is.EqualTo(2));
            Assert.That(asset.Eyes[0].z, Is.LessThan(0f));
            Assert.That(asset.Eyes[1].z, Is.LessThan(0f));
            for (int i = 0; i < asset.Crowns.Length; i++)
            {
                Vector3 rest = asset.Crowns[i];
                Point3 center = JellyShape.Deform(new Point3(rest.x, rest.y, rest.z), 0f, .6f, 0f, 0f, 0f, 0f);
                float worldTop = 1.15f + center.Y + asset.CrownScales[i].y * 1.6f;
                Assert.That(worldTop, Is.LessThan(3.6f), "Crown must fit the prototype's full-stretch framing.");
            }
        }

        [Test]
        public void AuthoredFlatBaseRemainsFixedAtMaximumSquashAndStretch()
        {
            PipCharacterAsset asset = Load();
            int anchors = 0;
            foreach (Vector3 vertex in asset.BodyMesh.vertices)
            {
                if (vertex.y > -.99999f) continue;
                anchors++;
                Point3 deformed = JellyShape.Deform(new Point3(vertex.x, vertex.y, vertex.z), .42f, .6f, .25f, -.25f, .5f, -.5f);
                Assert.That(deformed.X, Is.EqualTo(vertex.x).Within(.00001f));
                Assert.That(deformed.Y, Is.EqualTo(-1f).Within(.00001f));
                Assert.That(deformed.Z, Is.EqualTo(vertex.z).Within(.00001f));
            }
            Assert.That(anchors, Is.GreaterThan(30));
        }
    }
}
