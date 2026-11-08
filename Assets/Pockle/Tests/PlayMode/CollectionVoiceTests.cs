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
                    Assert.That(clip.length,Is.InRange(1.9f,2.1f));
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
                Assert.IsTrue(player.IsBusy);var first=player.CurrentClip;Assert.IsFalse(player.TryPlay(ToyCatalog.FoxId,CollectionVoiceMood.Happy));Assert.AreSame(first,player.CurrentClip);
                player.SetSettings(false,1);Assert.IsFalse(player.IsBusy);Assert.IsNull(player.CurrentClip);Assert.IsFalse(player.TryPlay(ToyCatalog.PeachId,CollectionVoiceMood.Happy));
                player.SetSettings(true,0);Assert.IsFalse(player.TryPlay(ToyCatalog.PeachId,CollectionVoiceMood.Happy));
                player.SetSettings(true,1);Assert.IsTrue(player.TryPlay(ToyCatalog.FoxId,CollectionVoiceMood.Curious));
                player.enabled=false;Assert.IsNull(player.CurrentClip);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [UnityTest]
        public IEnumerator PoseResetPreservesTailButLeavingPlayStopsVoice()
        {
            var root = new GameObject("Voice reset lifecycle");
            root.AddComponent<TactilePrototype>();
            yield return null; yield return null;
            var runtime = root.GetComponent<TactilePrototype>();
            var voice = root.GetComponent<CollectionVoicePlayer>();
            try
            {
                voice.SetSettings(true, 1);
                Assert.IsTrue(voice.TryPlay(ToyCatalog.PeachId, CollectionVoiceMood.Happy));
                var clip = voice.CurrentClip;
                runtime.ResetToy();
                Assert.AreSame(clip, voice.CurrentClip, "Double-tap pose reset must preserve the complete sound tail.");
                Assert.IsTrue(voice.IsBusy);
                typeof(TactilePrototype).GetMethod("StoreVisibilityChanged", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(runtime, new object[]{true});
                Assert.IsNull(voice.CurrentClip, "Leaving play must stop the creature voice.");
                Assert.IsFalse(voice.IsBusy);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [Test]
        public void All143ApprovedActionAndTemperamentClipsDecodeAndRouteIndependently()
        {
            var root = new GameObject("Approved complete voice library"); var voice = root.AddComponent<CollectionVoicePlayer>();
            var seen = new System.Collections.Generic.HashSet<AudioClip>();
            try
            {
                foreach (string character in new[]{"pip","fox","axolotl","firb","coco","snowy","ginger","reiny","guin","bundy","wally"})
                {
                    string id = null;
                    foreach (var entry in ToyCatalog.Collectibles) if (entry.CharacterId == character) { id = entry.Id; break; }
                    Assert.IsNotNull(id, character);
                    System.Type actionType = character == "pip" ? typeof(PipAction) : character == "fox" ? typeof(FoxAction)
                        : character == "axolotl" ? typeof(AxolotlAction) : character == "firb" ? typeof(FirbAction)
                        : character == "coco" ? typeof(CocoAction) : character == "snowy" ? typeof(SnowyAction)
                        : character == "ginger" ? typeof(GingerAction) : character == "wally" ? typeof(WallyAction) : typeof(WinterTrioAction);
                    foreach (System.Enum action in System.Enum.GetValues(actionType))
                    {
                        var clip = voice.ClipForAction(id, action); Validate(clip, seen, character + " " + action);
                        Assert.IsTrue(voice.TryPlayAction(id, action, CollectionVoiceMood.Happy));
                        Assert.AreSame(clip, voice.CurrentClip); voice.Stop();
                    }
                    foreach (PipTemperament face in System.Enum.GetValues(typeof(PipTemperament)))
                    {
                        var clip = voice.ClipForTemperament(id, face); Validate(clip, seen, character + " " + face);
                        Assert.IsTrue(voice.TryPlayTemperament(id, face, CollectionVoiceMood.Happy));
                        Assert.AreSame(clip, voice.CurrentClip); voice.Stop();
                    }
                }
                Assert.AreEqual(143, seen.Count);
                Assert.IsNull(voice.ClipForAction(ToyCatalog.FoxId, PipAction.Hop));
                Assert.IsFalse(voice.TryPlayAction(ToyCatalog.FoxId, PipAction.Hop, CollectionVoiceMood.Happy));
                Assert.IsNull(voice.ClipForTemperament("unknown", PipTemperament.Calm));
            }
            finally { Object.DestroyImmediate(root); }
        }
        private static void Validate(AudioClip clip, System.Collections.Generic.HashSet<AudioClip> seen, string label)
        {
            Assert.IsNotNull(clip, label); Assert.IsTrue(seen.Add(clip), "Distinct cue required: " + label);
            Assert.That(clip.length, Is.InRange(1.9f,3.1f)); Assert.AreEqual(1,clip.channels);
            var samples = new float[clip.samples * clip.channels]; Assert.IsTrue(clip.GetData(samples,0),label);
            Assert.IsTrue(System.Array.Exists(samples, x => System.Math.Abs(x) > .001f),label);
            int count = System.Math.Min(samples.Length, (int)(clip.frequency*.02f)); double sum = 0;
            for (int i=samples.Length-count; i<samples.Length; i++) sum += samples[i]*samples[i];
            Assert.Less(System.Math.Sqrt(sum/count),.005,"Quiet natural ending required: "+label);
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
