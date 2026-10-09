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
        // String keys hold names/IDs; the legacy avatar/favorite keys are ints. All are restored after each test.
        private readonly string[] profileKeys = { "pockle.profile.local.name", "pockle.profile.local.avatar", "pockle.profile.local.favorite",
            "pockle.profile.local.avatarId", "pockle.profile.local.favoriteId" };
        private static bool IsIntKey(int index) => index == 1 || index == 2;
        private readonly string[] previousProfile = new string[5];
        private readonly bool[] hadProfile = new bool[5];
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
                previousProfile[i] = IsIntKey(i) ? PlayerPrefs.GetInt(profileKeys[i]).ToString() : PlayerPrefs.GetString(profileKeys[i]);
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
                else if (IsIntKey(i)) PlayerPrefs.SetInt(profileKeys[i], int.Parse(previousProfile[i]));
                else PlayerPrefs.SetString(profileKeys[i], previousProfile[i]);
            }
            if (hadVolume) PlayerPrefs.SetFloat("pockle.prototype.soundVolume", previousVolume); else PlayerPrefs.DeleteKey("pockle.prototype.soundVolume");
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator ShelfSelectionAndUnavailableCheckoutDoNotGrantToys()
        {
            Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            ButtonWithText("See all").onClick.Invoke();
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
            ButtonNamed("Buy · Jelly Garden").onClick.Invoke();
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
        public IEnumerator PlayRoundTripRestoresShelfScrollAndSettingsKeepVolume()
        {
            ButtonWithText("See all").onClick.Invoke();
            yield return null; Canvas.ForceUpdateCanvases();
            var scroll = root.GetComponentInChildren<ScrollRect>();
            float position = Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height) * .5f;
            scroll.content.anchoredPosition = new Vector2(0, position);
            ButtonWithText("Moon Jelly").onClick.Invoke();
            Assert.AreEqual(AppPage.Play, hud.CurrentPage); Assert.IsFalse(hud.StoreVisible);
            hud.GoBack(); Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            Assert.AreEqual(position, scroll.content.anchoredPosition.y, .1f, "Play round trip lost the shelf position.");
            hud.GoBack(); Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            // Settings opens from the gear on You (v2).
            ButtonWithText("You").onClick.Invoke();
            ButtonWithText("Settings").onClick.Invoke();
            Assert.AreEqual(AppPage.Settings, hud.CurrentPage); Assert.IsTrue(hud.StoreVisible);
            var volume = root.GetComponentInChildren<Slider>(); volume.value = .42f;
            Assert.AreEqual(.42f, observedVolume, .001f); Assert.AreEqual(.42f, PlayerPrefs.GetFloat("pockle.prototype.soundVolume"), .001f);
            ButtonNamed("Sound switch").onClick.Invoke();
            Assert.IsFalse(volume.interactable); Assert.AreEqual(.42f, volume.value, .001f, "Muting erased the chosen volume.");
            Assert.IsTrue(VisibleText("Muted"));
            hud.GoBack(); Assert.AreEqual(AppPage.Profile, hud.CurrentPage);
            ButtonWithText("Home").onClick.Invoke(); Assert.AreEqual(AppPage.Home, hud.CurrentPage);
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
            ButtonNamed("Avatar · MOON JELLY").onClick.Invoke();
            Assert.AreEqual(1, PlayerPrefs.GetInt("pockle.profile.local.avatar"));
            // The favorite is set with the heart while playing a toy (v2).
            Assert.IsTrue(hud.ShowToy(PipVariants.CollectibleId(PipVariant.MoonJelly)));
            ButtonNamed("Favorite").onClick.Invoke();
            Assert.AreEqual(1, PlayerPrefs.GetInt("pockle.profile.local.favorite"));
            hud.GoBack(); hud.GoBack();
            ButtonWithText("Home").onClick.Invoke();
            Assert.IsTrue(Greets("Lucía 李"));
            ButtonWithText("Friends").onClick.Invoke();
            Assert.AreEqual(AppPage.Social, hud.CurrentPage);
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text.Contains("No public profiles are connected yet")));
            ButtonWithText("Your friends").onClick.Invoke();
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text.Contains("when accounts are ready")));
            CollectionAssert.AreEqual(inventory, session.Progress.Save.Counts);
            Object.Destroy(root); yield return null;
            root = new GameObject("Restarted local profile test");
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { });
            yield return null;
            Assert.IsTrue(Greets("Lucía 李"));
            ButtonWithText("You").onClick.Invoke();
            Assert.AreEqual("Lucía 李", root.GetComponentInChildren<InputField>().text);
        }

        [UnityTest]
        public IEnumerator HomeAndShelfReflectProgressAndKeepUndiscoveredToysASurprise()
        {
            // Restart with a saved ID inventory: Moon not yet discovered, three Mint duplicates.
            Object.Destroy(root); yield return null;
            var save = new CollectionSave { Version = CollectionProgress.SaveVersion, Day = CollectionSession.Today,
                Inventory = new System.Collections.Generic.List<OwnedCollectible> {
                    new OwnedCollectible(PipVariants.CollectibleId(PipVariant.PeachJelly), 1),
                    new OwnedCollectible(PipVariants.CollectibleId(PipVariant.GoldGlitter), 1),
                    new OwnedCollectible(PipVariants.CollectibleId(PipVariant.MintSoft), 3) } };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            root = new GameObject("Partial collection UI test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            Assert.AreEqual(0, session.GetOwnedCount(PipVariants.CollectibleId(PipVariant.MoonJelly)));
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session);
            session.SimulateDailyWalk(); // Ready box; also raises Changed so the HUD refreshes.
            yield return null;
            Assert.IsTrue(ButtonWithText("Open your box").interactable, "Home must offer the ready box as its main action.");
            Assert.IsTrue(VisibleText("Your box is ready!"), "Rewards tile must reflect today's ready box.");
            Assert.IsTrue(VisibleText("3 of 4 finishes · 5 toys on your shelf"));
            ButtonWithText("See all").onClick.Invoke();
            yield return null; Canvas.ForceUpdateCanvases();
            Assert.IsTrue(VisibleText("3 of 4 discovered"));
            Assert.IsTrue(VisibleText("???"), "An undiscovered toy must keep its name a surprise.");
            Assert.IsFalse(VisibleText("Moon Jelly"), "An undiscovered toy's name must not be shown.");
            var moon = root.GetComponentsInChildren<Button>().Single(button => button.name == "MOON JELLY · shelf toy");
            Assert.IsFalse(moon.interactable, "An undiscovered toy cannot be played.");
            Assert.IsTrue(VisibleText("×3"), "Duplicates show a count badge.");
            var portraits = root.GetComponentsInChildren<RawImage>().Where(image => image.texture is RenderTexture).ToArray();
            Assert.AreEqual(3, portraits.Length, "Only discovered toys show their rendered portrait.");
            ButtonWithText("Missing").onClick.Invoke();
            Assert.IsTrue(moon.gameObject.activeInHierarchy, "Missing shows the undiscovered toy.");
            Assert.IsFalse(ButtonNamed("PEACH JELLY · shelf toy", true).gameObject.activeInHierarchy, "Missing hides owned toys.");
            ButtonWithText("Duplicates").onClick.Invoke();
            Assert.IsTrue(ButtonNamed("MINT SOFT · shelf toy", true).gameObject.activeInHierarchy, "Duplicates shows the toy you have three of.");
            Assert.IsFalse(moon.gameObject.activeInHierarchy);
            ButtonWithText("Everything").onClick.Invoke();
            Assert.IsTrue(moon.gameObject.activeInHierarchy);
            Assert.IsFalse(hud.ShowToy(PipVariants.CollectibleId(PipVariant.MoonJelly)), "An unowned ID must not open play.");
            Assert.IsFalse(hud.ShowToy("moss.velvet-flock"), "A planned collectible without art must not open play.");
            Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            Assert.IsTrue(hud.ShowToy(PipVariants.CollectibleId(PipVariant.MintSoft)), "An owned, available ID opens play.");
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);
        }

        private bool VisibleText(string value)
        {
            return root.GetComponentsInChildren<Text>().Any(text => text.text == value && text.enabled);
        }

        private Button ButtonNamed(string name, bool includeHidden = false)
        {
            return root.GetComponentsInChildren<Button>(includeHidden).First(button => button.name == name);
        }

        /// <summary>Home greets by time of day ("Morning, Lucía").</summary>
        private bool Greets(string name)
        {
            return root.GetComponentsInChildren<Text>().Any(text => text.enabled && text.text.EndsWith(", " + name)
                && (text.text.StartsWith("Morning") || text.text.StartsWith("Afternoon") || text.text.StartsWith("Evening")));
        }

        private Button ButtonWithText(string value)
        {
            return root.GetComponentsInChildren<Button>().First(button =>
                button.GetComponentsInChildren<Text>().Any(text => text.text == value));
        }
    }
}
#endif
