#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Runtime;
using Pockle.Core;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    public sealed class CollectionUiTests
    {
        private const string SaveKey = "pockle.collection.v1";
        private GameObject root;
        private PrototypeHud hud;
        private CollectionSession session;
        private string oldSave, oldBackup;
        private bool hadSave, hadBackup;
        private readonly string[] profileKeys = { "pockle.profile.local.name", "pockle.profile.local.avatar", "pockle.profile.local.favorite" };
        private readonly string[] previousProfile = new string[3];
        private readonly bool[] hadProfile = new bool[3];
        private bool hadVolume;
        private float previousVolume, observedVolume;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            hadSave = PlayerPrefs.HasKey(SaveKey); oldSave = PlayerPrefs.GetString(SaveKey);
            hadBackup = PlayerPrefs.HasKey(SaveKey + ".backup"); oldBackup = PlayerPrefs.GetString(SaveKey + ".backup");
            PlayerPrefs.DeleteKey(SaveKey);
            for (int i = 0; i < profileKeys.Length; i++)
            {
                hadProfile[i] = PlayerPrefs.HasKey(profileKeys[i]);
                previousProfile[i] = i == 0 ? PlayerPrefs.GetString(profileKeys[i]) : PlayerPrefs.GetInt(profileKeys[i]).ToString();
                PlayerPrefs.DeleteKey(profileKeys[i]);
            }
            hadVolume = PlayerPrefs.HasKey("pockle.prototype.soundVolume"); previousVolume = PlayerPrefs.GetFloat("pockle.prototype.soundVolume");
            PlayerPrefs.DeleteKey("pockle.prototype.soundVolume");
            root = new GameObject("Collection UI test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }, value => observedVolume = value); hud.Bind(session);
            hud.SetSettings(true, false, false);
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root); yield return null; // Session persists before we restore the original save.
            if (hadSave) PlayerPrefs.SetString(SaveKey, oldSave); else PlayerPrefs.DeleteKey(SaveKey);
            if (hadBackup) PlayerPrefs.SetString(SaveKey + ".backup", oldBackup); else PlayerPrefs.DeleteKey(SaveKey + ".backup");
            for (int i = 0; i < profileKeys.Length; i++)
            {
                if (!hadProfile[i]) PlayerPrefs.DeleteKey(profileKeys[i]);
                else if (i == 0) PlayerPrefs.SetString(profileKeys[i], previousProfile[i]);
                else PlayerPrefs.SetInt(profileKeys[i], int.Parse(previousProfile[i]));
            }
            if (hadVolume) PlayerPrefs.SetFloat("pockle.prototype.soundVolume", previousVolume); else PlayerPrefs.DeleteKey("pockle.prototype.soundVolume");
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator ShelfSelectionAndUnavailableCheckoutDoNotGrantToys()
        {
            Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            ButtonWithText("Collection").onClick.Invoke();
            var portraits = root.GetComponentsInChildren<RawImage>().Where(image => image.texture is RenderTexture).ToArray();
            Assert.AreEqual(4, portraits.Length, "Shelf must show four separate rendered toys.");
            Assert.IsTrue(portraits.All(image => ((RenderTexture)image.texture).IsCreated()));
            PipVariant selected = PipVariant.PeachJelly;
            hud.VariantChanged += choice => selected = choice;
            ButtonWithText("Moon Jelly").onClick.Invoke();
            Assert.AreEqual(PipVariant.MoonJelly, selected);
            Assert.IsFalse(hud.StoreVisible, "Selected toy must be available for direct manipulation.");
            hud.GoBack();
            ButtonWithText("Boxes").onClick.Invoke();
            var before = (int[])session.Progress.Save.Counts.Clone();
            ButtonWithText("Buy box").onClick.Invoke();
            yield return null;
            CollectionAssert.AreEqual(before, session.Progress.Save.Counts, "Unavailable checkout must never grant a toy.");
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text.Contains("Nothing will be charged")));
            hud.GoBack();
            Assert.IsTrue(hud.StoreVisible, "Closing purchase dialog must retain box browsing.");
        }

        [UnityTest]
        public IEnumerator DailyBoxUiUnlocksAndOnlyGrantsOnce()
        {
            ButtonWithText("Boxes").onClick.Invoke();
            Assert.IsFalse(ButtonWithText("Walk to unlock").interactable);
            int grants = 0;
            hud.DailyBoxRequested += () => { if (session.ClaimDaily(out _)) grants++; };
            session.SimulateDailyWalk();
            yield return null;
            var open = ButtonWithText("Open your box");
            Assert.IsTrue(open.interactable);
            open.onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, grants);
            Assert.AreEqual(5, session.Progress.Save.Counts.Sum());
            Assert.IsFalse(ButtonWithText("Opened today").interactable);
            open.onClick.Invoke(); // Even stale/replayed callbacks cannot duplicate ownership.
            Assert.AreEqual(1, grants);
        }

        [UnityTest]
        public IEnumerator SettingsReturnsToPlayAndShelfRestoresScroll()
        {
            ButtonWithText("Collection").onClick.Invoke();
            yield return null; Canvas.ForceUpdateCanvases();
            var scroll = root.GetComponentInChildren<ScrollRect>();
            float position = Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height) * .5f;
            scroll.content.anchoredPosition = new Vector2(0, position);
            ButtonWithText("Moon Jelly").onClick.Invoke();
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);
            ButtonWithText("Settings").onClick.Invoke();
            Assert.AreEqual(AppPage.Settings, hud.CurrentPage); Assert.IsTrue(hud.StoreVisible);
            var volume = root.GetComponentInChildren<Slider>(); volume.value = .42f;
            Assert.AreEqual(.42f, observedVolume, .001f); Assert.AreEqual(.42f, PlayerPrefs.GetFloat("pockle.prototype.soundVolume"), .001f);
            ButtonWithText("Sound on").onClick.Invoke();
            Assert.IsFalse(volume.interactable); Assert.AreEqual(.42f, volume.value, .001f, "Muting erased the chosen volume.");
            hud.GoBack(); Assert.AreEqual(AppPage.Play, hud.CurrentPage); Assert.IsFalse(hud.StoreVisible);
            hud.GoBack(); Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            Assert.AreEqual(position, scroll.content.anchoredPosition.y, .1f, "Settings/play round trip lost the shelf position.");
            hud.GoBack(); Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            Assert.IsFalse(hud.GoBack(), "Home Back must hand root exit to the platform.");
        }

        [UnityTest]
        public IEnumerator LocalProfilePersistsWithoutPublishingOrGrantingToys()
        {
            var inventory = (int[])session.Progress.Save.Counts.Clone();
            ButtonWithText("You").onClick.Invoke();
            var field = root.GetComponentInChildren<InputField>();
            field.text = "  Lucía 李  "; field.onEndEdit.Invoke(field.text);
            Assert.AreEqual("Lucía 李", PlayerPrefs.GetString("pockle.profile.local.name"));
            Assert.IsFalse(field.textComponent.supportRichText);
            ButtonWithText("Moon Jelly").onClick.Invoke();
            var avatar = root.GetComponentsInChildren<Button>().Single(button => button.name == "Avatar · MOON JELLY");
            avatar.onClick.Invoke();
            Assert.AreEqual(1, PlayerPrefs.GetInt("pockle.profile.local.avatar"));
            Assert.AreEqual(1, PlayerPrefs.GetInt("pockle.profile.local.favorite"));
            ButtonWithText("Home").onClick.Invoke();
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text == "Hey, Lucía 李"));
            ButtonWithText("Friends").onClick.Invoke();
            Assert.AreEqual(AppPage.Social, hud.CurrentPage);
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text.Contains("No public profiles are connected yet")));
            ButtonWithText("Friends").onClick.Invoke();
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text.Contains("when accounts are ready")));
            CollectionAssert.AreEqual(inventory, session.Progress.Save.Counts);
            Object.Destroy(root); yield return null;
            root = new GameObject("Restarted local profile test");
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { });
            yield return null;
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text == "Hey, Lucía 李"));
            ButtonWithText("You").onClick.Invoke();
            Assert.AreEqual("Lucía 李", root.GetComponentInChildren<InputField>().text);
        }

        [UnityTest]
        public IEnumerator HomeAndShelfReflectProgressAndKeepUndiscoveredToysASurprise()
        {
            var counts = session.Progress.Save.Counts;
            counts[(int)PipVariant.MoonJelly] = 0; counts[(int)PipVariant.MintSoft] = 3;
            session.SimulateDailyWalk(); // Also raises Changed so the HUD refreshes.
            yield return null;
            Assert.IsTrue(ButtonWithText("Open your box").interactable, "Home must offer the ready box as its main action.");
            Assert.IsTrue(VisibleText("Your box is ready!"), "Rewards tile must reflect today's ready box.");
            Assert.IsTrue(VisibleText("3 of 4 finishes · 5 toys on your shelf"));
            ButtonWithText("Collection").onClick.Invoke();
            yield return null; Canvas.ForceUpdateCanvases();
            Assert.IsTrue(VisibleText("3 of 4 discovered"));
            Assert.IsTrue(VisibleText("???"), "An undiscovered toy must keep its name a surprise.");
            Assert.IsFalse(VisibleText("Moon Jelly"), "An undiscovered toy's name must not be shown.");
            var moon = root.GetComponentsInChildren<Button>().Single(button => button.name == "MOON JELLY · shelf toy");
            Assert.IsFalse(moon.interactable, "An undiscovered toy cannot be played.");
            Assert.IsTrue(VisibleText("×3"), "Duplicates show a count badge.");
            var portraits = root.GetComponentsInChildren<RawImage>().Where(image => image.texture is RenderTexture).ToArray();
            Assert.AreEqual(3, portraits.Length, "Only discovered toys show their rendered portrait.");
        }

        private bool VisibleText(string value)
        {
            return root.GetComponentsInChildren<Text>().Any(text => text.text == value && text.enabled);
        }

        private Button ButtonWithText(string value)
        {
            return root.GetComponentsInChildren<Button>().First(button =>
                button.GetComponentsInChildren<Text>().Any(text => text.text == value));
        }
    }
}
#endif
