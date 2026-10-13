using System.Collections.Generic;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;

namespace Pockle.Tests
{
    public sealed class MeshyPeachTests
    {
        [Test]
        public void UploadedPeachImportsReadableWithinThePhoneBudget()
        {
            var asset = Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath);
            Assert.That(asset, Is.Not.Null);
            Assert.That(asset.CharacterId, Is.EqualTo("pip"));
            Assert.That(asset.BakedSurface, Is.True);
            Assert.That(asset.IntegratedCrown, Is.True);
            Assert.That(asset.BodyMesh.isReadable, Is.True);
            Assert.That(asset.BodyMesh.triangles.Length / 3, Is.InRange(1000, 6000));
            Assert.That(asset.BodyMesh.vertexCount, Is.LessThanOrEqualTo(4000));
            Assert.That(asset.BodyMesh.uv.Length, Is.EqualTo(asset.BodyMesh.vertexCount));
            Assert.That(asset.NormalGroups.Length, Is.EqualTo(asset.BodyMesh.vertexCount));
            Assert.That(asset.BodyMesh.bounds.min.y, Is.EqualTo(-1).Within(.00001f));
            Assert.That(asset.BodyMesh.bounds.max.y, Is.EqualTo(.94).Within(.0001f));
            Assert.That(asset.BaseColorTexture, Is.Not.Null);
            Assert.That(asset.BaseColorTexture.width, Is.InRange(1, 2048));
            Assert.That(asset.BaseColorTexture.height, Is.InRange(1, 2048));
            foreach (int group in asset.NormalGroups)
                Assert.That(group, Is.InRange(0, asset.BodyMesh.vertexCount - 1));
        }

        [Test]
        public void ReducedPeachHasNoOpenOrInconsistentlyWoundTextureSeams()
        {
            var asset = Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath);
            int[] indices = asset.BodyMesh.triangles;
            int[] groups = asset.NormalGroups;
            Vector3[] positions = asset.BodyMesh.vertices;
            var uses = new Dictionary<long, int>();
            var winding = new Dictionary<long, int>();
            for (int i = 0; i < positions.Length; i++)
                Assert.That(positions[i], Is.EqualTo(positions[groups[i]]), "UV splits must share one rest position.");
            for (int i = 0; i < indices.Length; i += 3)
            for (int edge = 0; edge < 3; edge++)
            {
                int a = groups[indices[i + edge]], b = groups[indices[i + (edge + 1) % 3]];
                Assert.That(a, Is.Not.EqualTo(b), "Collapsed logical edge.");
                long key = ((long)Mathf.Min(a, b) << 32) | (uint)Mathf.Max(a, b);
                uses.TryGetValue(key, out int count);
                winding.TryGetValue(key, out int direction);
                uses[key] = count + 1;
                winding[key] = direction + (a < b ? 1 : -1);
            }
            foreach (var edge in uses)
            {
                Assert.That(edge.Value, Is.EqualTo(2), "Open/non-manifold edge " + edge.Key);
                Assert.That(winding[edge.Key], Is.Zero, "Adjacent triangles must face the same side.");
            }
        }

        [Test]
        public void TexturedShaderIsRetainedForThePlayer()
        {
            var material = Resources.Load<Material>("MeshyShaderRetention");
            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader.name, Is.EqualTo("Pockle/Textured Peach Jelly"));
            Assert.That(material.GetFloat("_Transmission"), Is.EqualTo(.90).Within(.00001));
            Assert.That(material.GetFloat("_Roughness"), Is.EqualTo(.20).Within(.00001));
            Assert.That(material.GetFloat("_Subsurface"), Is.EqualTo(.05).Within(.00001));
        }
    }
}
