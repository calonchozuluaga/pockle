#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Runtime;
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

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            hadSave = PlayerPrefs.HasKey(SaveKey); oldSave = PlayerPrefs.GetString(SaveKey);
            hadBackup = PlayerPrefs.HasKey(SaveKey + ".backup"); oldBackup = PlayerPrefs.GetString(SaveKey + ".backup");
            PlayerPrefs.DeleteKey(SaveKey);
            root = new GameObject("Collection UI test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session);
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root); yield return null; // Session persists before we restore the original save.
            if (hadSave) PlayerPrefs.SetString(SaveKey, oldSave); else PlayerPrefs.DeleteKey(SaveKey);
            if (hadBackup) PlayerPrefs.SetString(SaveKey + ".backup", oldBackup); else PlayerPrefs.DeleteKey(SaveKey + ".backup");
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator ShelfSelectionAndUnavailableCheckoutDoNotGrantToys()
        {
            var portraits = root.GetComponentsInChildren<RawImage>().Where(image => image.texture is RenderTexture).ToArray();
            Assert.AreEqual(4, portraits.Length, "Shelf must show four separate rendered toys.");
            Assert.IsTrue(portraits.All(image => ((RenderTexture)image.texture).IsCreated()));
            PipVariant selected = PipVariant.PeachJelly;
            hud.VariantChanged += choice => selected = choice;
            ButtonWithText("MOON JELLY").onClick.Invoke();
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

        private Button ButtonWithText(string value)
        {
            return root.GetComponentsInChildren<Button>().First(button =>
                button.GetComponentsInChildren<Text>().Any(text => text.text == value));
        }
    }
}
#endif
