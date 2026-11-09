using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Pockle.Tests
{
    public sealed class MomentAudioTests
    {
        [Test]
        public void AllSevenApprovedMomentsDecodeWithFullTailsAndHeadroom()
        {
            var root = new GameObject("Approved moment library");
            var player = root.AddComponent<MomentAudioPlayer>();
            var seen = new HashSet<AudioClip>();
            try
            {
                foreach (MomentCue cue in Enum.GetValues(typeof(MomentCue)))
                {
                    if (cue == MomentCue.None) continue;
                    var clip = player.ClipFor(cue);
                    Assert.IsNotNull(clip, cue.ToString()); Assert.IsTrue(seen.Add(clip));
                    Assert.AreEqual(1, clip.channels); Assert.AreEqual(44100, clip.frequency);
                    Assert.That(clip.length, Is.InRange(2.999f, 3.001f));
                    var samples = new float[clip.samples]; Assert.IsTrue(clip.GetData(samples, 0));
                    float peak = 0; double tail = 0;
                    foreach (float sample in samples) peak = Mathf.Max(peak, Mathf.Abs(sample));
                    for (int i = samples.Length - 882; i < samples.Length; i++) tail += samples[i] * samples[i];
                    Assert.That(peak, Is.InRange(.01f, .751f), cue + " needs signal and clipping headroom.");
                    Assert.Less(Math.Sqrt(tail / 882), .001, cue + " must keep its quiet original tail.");
                }
                Assert.AreEqual(7, seen.Count);
                Assert.IsNull(player.CurrentClip, "Loading the library must not autoplay.");
                Assert.IsNull(player.ClipFor(MomentCue.None));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [UnityTest]
        public IEnumerator QueuedMomentsRespectVoicesFullTailsAndSoundControls()
        {
            var root = new GameObject("Moment playback lifecycle");
            var player = root.AddComponent<MomentAudioPlayer>();
            bool voiceBusy = true;
            player.VoiceBusy = () => voiceBusy;
            try
            {
                player.SetSettings(true, 1);
                Assert.IsTrue(player.Request(MomentCue.BoxReveal));
                Assert.IsTrue(player.Request(MomentCue.SpecialReveal));
                yield return null; yield return null;
                Assert.IsNull(player.CurrentClip, "Existing character voice must finish first.");
                voiceBusy = false;
                yield return null; yield return null;
                Assert.AreEqual(MomentCue.SpecialReveal, player.CurrentCue);
                var fullTail = player.CurrentClip;
                Assert.IsTrue(player.Request(MomentCue.FusionComplete));
                yield return null; yield return null;
                Assert.AreSame(fullTail, player.CurrentClip, "New events cannot truncate the current three-second tail.");
                player.SetSettings(false, 1);
                Assert.IsFalse(player.IsBusy); Assert.IsNull(player.CurrentClip);
                Assert.IsFalse(player.Request(MomentCue.FusionComplete));
                player.SetSettings(true, 0); Assert.IsFalse(player.Request(MomentCue.BoxReveal));
                player.SetSettings(true, float.NaN); Assert.IsFalse(player.Request(MomentCue.BoxReveal));
                player.SetSettings(true, 1); Assert.IsTrue(player.Request(MomentCue.BoxReveal));
                player.enabled = false;
                Assert.IsFalse(player.IsBusy); Assert.IsNull(player.CurrentClip);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator WalkingSyncIsSilentOnStartupAndRepeatedRefresh()
        {
            var root = new GameObject("Walking moment edges"); var player = root.AddComponent<MomentAudioPlayer>();
            try
            {
                player.ObserveWalking(new WalkingMomentSnapshot("owner", 20, 0, 900, Array.Empty<string>()));
                yield return null; yield return null;
                Assert.IsFalse(player.IsBusy);
                player.ObserveWalking(new WalkingMomentSnapshot("owner", 20, 1, 0, new[] { "box" }));
                yield return null; yield return null;
                Assert.AreEqual(MomentCue.WalkingBoxReady, player.CurrentCue);
                player.Stop();
                player.ObserveWalking(new WalkingMomentSnapshot("owner", 20, 1, 0, new[] { "box" }));
                yield return null; yield return null;
                Assert.IsFalse(player.IsBusy);
                player.PassiveAllowed = () => false;
                player.ObserveWalking(new WalkingMomentSnapshot("owner", 20, 1, 1000, new[] { "box" }));
                player.PassiveAllowed = () => true;
                player.ObserveWalking(new WalkingMomentSnapshot("owner", 20, 1, 1000, new[] { "box" }));
                yield return null; yield return null;
                Assert.IsFalse(player.IsBusy, "Closing a modal must not replay a suppressed progress cue.");
            }
            finally { Object.Destroy(root); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator FusionArrivalFiresAtChildAppearanceOrSkipExactlyOnce()
        {
            var root = new GameObject("Fusion sound arrival"); var player = root.AddComponent<FusionMomentPlayer>();
            int arrived = 0, finished = 0; player.Arrived = () => arrived++; player.Finished = () => finished++;
            try
            {
                player.Play(false);
                yield return new WaitForSecondsRealtime(FusionMomentPlayer.Duration * .78f);
                Assert.AreEqual(1, arrived); Assert.AreEqual(0, finished);
                player.Skip(); player.Skip();
                Assert.AreEqual(1, arrived); Assert.AreEqual(1, finished);
                player.Play(false); player.Skip();
                Assert.AreEqual(2, arrived, "Early skip still presents one saved result cue.");
                Assert.AreEqual(2, finished);
            }
            finally { Object.DestroyImmediate(root); }
        }

        [UnityTest]
        public IEnumerator RepeatedSavedFusionArrivalDoesNotRequeueTheSound()
        {
            var root = new GameObject("Fusion sound identity"); var player = root.AddComponent<MomentAudioPlayer>();
            try
            {
                player.FusionArrived("fusion.saved-one");
                yield return null; yield return null;
                Assert.AreEqual(MomentCue.FusionComplete, player.CurrentCue);
                player.Stop(); player.FusionArrived("fusion.saved-one");
                Assert.IsFalse(player.IsBusy);
                player.FusionArrived(""); Assert.IsFalse(player.IsBusy);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
    }
}
