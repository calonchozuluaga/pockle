#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    /// <summary>One main sound per character, played only from the Play screen's Sound control.</summary>
    public sealed class MainSoundUiTests
    {
        private const string SaveKey = "pockle.collection.v1", VolumeKey = "pockle.prototype.soundVolume";
        private readonly Dictionary<string, string> oldStrings = new Dictionary<string, string>();
        private readonly List<string> stringKeys = new List<string>();
        private bool hadVolume; private float oldVolume;
        private GameObject root;
        private PrototypeHud hud;

        [SetUp]
        public void SetUp()
        {
            stringKeys.Clear(); oldStrings.Clear();
            stringKeys.AddRange(new[] { SaveKey, SaveKey + ".backup", SaveKey + ".before-v2", SaveKey + ".before-v3",
                SaveKey + ".before-v4", SaveKey + ".before-v5", SaveKey + ".before-v6",
                "pockle.profile.local.name", "pockle.profile.local.avatarId", "pockle.profile.local.favoriteId",
                PrototypeHud.MomentsKey, PrototypeHud.LiveShelfKey, PrototypeHud.LiveShelfSeenKey,
                LocalActivityFeed.LogKey, ShelfFriendships.Key, BadgeCabinet.Key });
            stringKeys.AddRange(ToyCatalog.Collections.Select(c => "pockle.stage.environment.v1." + c.Id));
            foreach (string key in stringKeys) { if (PlayerPrefs.HasKey(key)) oldStrings[key] = PlayerPrefs.GetString(key); PlayerPrefs.DeleteKey(key); }
            hadVolume = PlayerPrefs.HasKey(VolumeKey); oldVolume = PlayerPrefs.GetFloat(VolumeKey, 1); PlayerPrefs.SetFloat(VolumeKey, 1);
            ShelfFriendships.Reload();
            var inventory = ToyCatalog.Collections.Where(c => c.Id == c.CharacterId).SelectMany(c => c.Members.Take(1))
                .Append(ToyCatalog.PeachId).Append(ToyCatalog.FoxId).Distinct().Select(id => new OwnedCollectible(id, 1)).ToList();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new CollectionSave { Version = CollectionProgress.SaveVersion,
                Day = CollectionSession.Today, Inventory = inventory }));
        }

        private void BuildHud()
        {
            root = new GameObject("Main sound UI test");
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session); hud.SetSettings(true, false, true);
            Assert.IsTrue(hud.ShowToy(ToyCatalog.PeachId));
            Canvas.ForceUpdateCanvases();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root);
            yield return null;
            foreach (string key in stringKeys) { if (oldStrings.TryGetValue(key, out var value)) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); }
            if (hadVolume) PlayerPrefs.SetFloat(VolumeKey, oldVolume); else PlayerPrefs.DeleteKey(VolumeKey);
            PlayerPrefs.Save(); ShelfFriendships.Reload();
        }

        [Test]
        public void EveryCharacterHasOneShortLocalMainSound()
        {
            var seen = new HashSet<AudioClip>();
            foreach (var character in ToyCatalog.Characters)
            {
                var clip = Resources.Load<AudioClip>(PrototypeHud.MainSoundResource(character.Id));
                Assert.IsNotNull(clip, character.Id + " needs a main sound.");
                Assert.IsTrue(seen.Add(clip), character.Id + " must have its own clip.");
                Assert.AreEqual(1, clip.channels, character.Id);
                Assert.AreEqual(44100, clip.frequency, character.Id);
                Assert.That(clip.length, Is.InRange(.2f, 3f), character.Id);
                var samples = new float[clip.samples]; Assert.IsTrue(clip.GetData(samples, 0));
                Assert.IsTrue(System.Array.Exists(samples, s => System.Math.Abs(s) > .01f), character.Id + " must contain audio.");
                Assert.IsFalse(System.Array.Exists(samples, s => System.Math.Abs(s) >= .999f), character.Id + " must not clip.");
            }
            Assert.AreEqual("", PrototypeHud.MainSoundResource(""));
        }

        [UnityTest]
        public IEnumerator SoundControlPlaysTheStagedCharacterOnlyWhenPressed()
        {
            BuildHud();
            yield return null;
            var started = new List<string>();
            hud.MainSoundPlayed += (id, seconds) => { started.Add(id); Assert.Greater(seconds, 0f); };
            var sound = SoundButton;
            Assert.IsTrue(sound.gameObject.activeInHierarchy, "A toy with a main sound shows the control on the play rail.");
            Assert.IsTrue(sound.interactable);
            Assert.IsNull(hud.CurrentMainSound, "Nothing plays until the player asks.");

            sound.onClick.Invoke();
            var pip = Resources.Load<AudioClip>(PrototypeHud.MainSoundResource("pip"));
            Assert.AreSame(pip, hud.CurrentMainSound);
            sound.onClick.Invoke();
            Assert.AreSame(pip, hud.CurrentMainSound, "Pressing again restarts the same clip.");
            Assert.AreEqual(1, root.GetComponents<AudioSource>().Length, "Presses share one source, so sounds never stack.");
            CollectionAssert.AreEqual(new[] { ToyCatalog.PeachId, ToyCatalog.PeachId }, started);

            // Another character: the old sound stops and the control now plays the new one.
            Assert.IsTrue(hud.ShowToy(ToyCatalog.FoxId));
            yield return null;
            Assert.IsNull(hud.CurrentMainSound, "Changing toy stops the previous sound.");
            SoundButton.onClick.Invoke();
            Assert.AreSame(Resources.Load<AudioClip>(PrototypeHud.MainSoundResource("fox")), hud.CurrentMainSound);

            // The four labelled controls and Sound share one row without overlapping.
            var rail = root.GetComponentsInChildren<RectTransform>().Single(t => t.name == "Stage rail");
            var row = rail.GetComponentsInChildren<Button>().Select(b => b.GetComponent<RectTransform>()).OrderBy(r => r.anchoredPosition.x).ToList();
            Assert.AreEqual(5, row.Count);
            for (int i = 1; i < row.Count; i++)
                Assert.GreaterOrEqual(row[i].anchoredPosition.x, row[i - 1].anchoredPosition.x + row[i - 1].sizeDelta.x - .01f);
            Assert.LessOrEqual(row[4].anchoredPosition.x + row[4].sizeDelta.x, rail.sizeDelta.x + .01f);
            Assert.AreEqual("Stage sound", row[4].name, "Sound sits at the end of the row.");
        }

        [UnityTest]
        public IEnumerator SoundControlHonoursMuteVolumeAndLeavingPlay()
        {
            BuildHud();
            yield return null;
            hud.SetSettings(false, false, true);
            Assert.IsFalse(SoundButton.interactable, "Sound off dims the control.");
            hud.PlayMainSound();
            Assert.IsNull(hud.CurrentMainSound, "Sound off plays nothing.");

            hud.SetSettings(true, false, true);
            PlayerPrefs.SetFloat(VolumeKey, 0);
            hud.PlayMainSound();
            Assert.IsNull(hud.CurrentMainSound, "Zero volume plays nothing.");

            PlayerPrefs.SetFloat(VolumeKey, 1);
            hud.PlayMainSound();
            Assert.IsNotNull(hud.CurrentMainSound);
            hud.SetSettings(false, false, true);
            Assert.IsNull(hud.CurrentMainSound, "Turning Sound off stops it at once.");

            hud.SetSettings(true, false, true);
            hud.PlayMainSound();
            Assert.IsNotNull(hud.CurrentMainSound);
            hud.GoBack();
            yield return null;
            Assert.IsNull(hud.CurrentMainSound, "Leaving play stops the sound.");
            hud.PlayMainSound();
            Assert.IsNull(hud.CurrentMainSound, "The sound belongs to the play screen.");
        }

        [UnityTest]
        public IEnumerator TouchResponsesShowTheirCalloutWithoutACreatureVoice()
        {
            var runtimeRoot = new GameObject("Silent touch response");
            runtimeRoot.AddComponent<TactilePrototype>();
            yield return null; yield return null;
            var runtime = runtimeRoot.GetComponent<TactilePrototype>();
            var voice = runtimeRoot.GetComponent<CollectionVoicePlayer>();
            try
            {
                voice.SetSettings(true, 1);
                int callouts = 0;
                runtime.ToyReactionStarted += (cue, head) => callouts++;
                var present = typeof(TactilePrototype).GetMethod("PresentToyResponse", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                Assert.IsNotNull(present);
                foreach (PipTemperament temperament in System.Enum.GetValues(typeof(PipTemperament)))
                {
                    var response = ToyReactionResponses.Temperament("pip", temperament);
                    Assert.IsNotNull(Resources.Load<AudioClip>(response.AudioResource), "The recording still exists; touch simply does not play it.");
                    present.Invoke(runtime, new object[] { response, ReactionCue.Squish, null });
                    Assert.IsNull(voice.CurrentClip, temperament + " must stay silent on touch.");
                    Assert.IsFalse(voice.IsBusy);
                }
                Assert.Greater(callouts, 0, "The written callout still appears.");
            }
            finally { Object.Destroy(runtimeRoot); }
            yield return null;
        }

        private Button SoundButton => root.GetComponentsInChildren<Button>(true).Single(b => b.name == "Stage sound");
    }
}
#endif
