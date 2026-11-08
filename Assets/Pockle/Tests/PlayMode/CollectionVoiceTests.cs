using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class CollectionVoiceTests
    {
        [Test]
        public void AllApprovedClipsDecodeAndStayCollectionSpecific()
        {
            var root=new GameObject("Approved local voices");var player=root.AddComponent<CollectionVoicePlayer>();
            try
            {
                var seen=new System.Collections.Generic.HashSet<AudioClip>();
                foreach(string character in new[]{"pip","fox","wally"})
                foreach(CollectionVoiceMood mood in System.Enum.GetValues(typeof(CollectionVoiceMood)))
                {
                    string id=character=="pip"?ToyCatalog.PeachId:character=="fox"?ToyCatalog.FoxId:WallyCatalog.ClassicId;
                    var clip=player.ClipFor(id,mood);Assert.IsNotNull(clip,id+" "+mood);Assert.IsTrue(seen.Add(clip));
                    Assert.That(clip.length,Is.InRange(.1f,1.3f));
                    var samples=new float[clip.samples*clip.channels];Assert.IsTrue(clip.GetData(samples,0));
                    Assert.IsTrue(System.Array.Exists(samples,s=>System.Math.Abs(s)>.001f),"Decoded clip must contain audio.");
                }
                Assert.IsNull(player.ClipFor(ToyCatalog.AxolotlId,CollectionVoiceMood.Happy));
                Assert.IsNull(player.ClipFor("unknown",CollectionVoiceMood.Happy));
            }
            finally { Object.DestroyImmediate(root); }
        }
        [UnityTest]
        public IEnumerator SoundControlsStopVoicesAndRapidInputCannotStackThem()
        {
            var root=new GameObject("Voice gate");var player=root.AddComponent<CollectionVoicePlayer>();
            try
            {
                player.SetSettings(true,1);
                Assert.IsTrue(player.TryPlay(ToyCatalog.PeachId,CollectionVoiceMood.Playful), "Enabled voice must start with an imported local clip.");
                var first=player.CurrentClip;Assert.IsFalse(player.TryPlay(ToyCatalog.FoxId,CollectionVoiceMood.Happy));Assert.AreSame(first,player.CurrentClip);
                player.SetSettings(false,1);Assert.IsNull(player.CurrentClip);Assert.IsFalse(player.TryPlay(ToyCatalog.PeachId,CollectionVoiceMood.Happy));
                player.SetSettings(true,0);Assert.IsFalse(player.TryPlay(ToyCatalog.PeachId,CollectionVoiceMood.Happy));
                player.SetSettings(true,1);Assert.IsTrue(player.TryPlay(ToyCatalog.FoxId,CollectionVoiceMood.Curious));
                player.enabled=false;Assert.IsNull(player.CurrentClip);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [Test]
        public void IceVinylUsesItsOwnBluePigmentAndHasNoPlushPile()
        {
            var root=new GameObject("Axolotl finish separation");var toy=root.AddComponent<JellyToy>();toy.Initialize();
            try
            {
                Assert.IsTrue(toy.TrySetCollectible(ToyCatalog.AxolotlId));var plush=toy.BodyRenderer.sharedMaterial;
                Assert.AreEqual(0,plush.GetFloat("_TintStrength"));Assert.That(plush.GetFloat("_Fuzz"),Is.GreaterThan(.5));
                Assert.IsTrue(toy.TrySetCollectible("axolotl.matte-vinyl"));var vinyl=toy.BodyRenderer.sharedMaterial;
                Assert.AreEqual(1,vinyl.GetFloat("_TintStrength"));Assert.AreEqual(0,vinyl.GetFloat("_Fuzz"));
                Color c=vinyl.GetColor("_BodyColor");Assert.That(c.b,Is.GreaterThan(c.r));Assert.That(vinyl.GetFloat("_Gloss"),Is.GreaterThan(.3));
                Assert.IsTrue(toy.TrySetCollectible(ToyCatalog.AxolotlId));Assert.AreEqual(0,toy.BodyRenderer.sharedMaterial.GetFloat("_TintStrength"));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
