using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    public sealed class NotificationSettingsUiTests
    {
        private GameObject root;
        private PrototypeHud hud;
        private const string BoxKey = "pockle.notifications.v1.boxes";
        private bool had; private int prior;
        [UnitySetUp] public IEnumerator Setup()
        {
            had = PlayerPrefs.HasKey(BoxKey); prior = PlayerPrefs.GetInt(BoxKey);
            root = new GameObject("Notification settings test");
            var collection = root.AddComponent<CollectionSession>(); collection.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(collection);
            hud.SendMessage("ShowSettings"); yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Object.Destroy(root); yield return null;
            if (had) PlayerPrefs.SetInt(BoxKey, prior); else PlayerPrefs.DeleteKey(BoxKey);
            PlayerPrefs.Save();
        }
        private Button ActionButton => hud.GetComponentsInChildren<Button>(true).Single(b => b.name == "Phone notification permission");
        [Test] public void BindingDoesNotPromptButExplicitOptInDoes()
        {
            int requested = 0; NotificationPrefs.Set(NotificationPrefs.BoxReady, false);
            hud.SetNotificationPermission(BoxNotificationPermission.NotAsked, () => requested++, () => Assert.Fail("Not a settings redirect"));
            Assert.AreEqual(0, requested);
            Assert.IsTrue(ActionButton.interactable);
            ActionButton.onClick.Invoke();
            Assert.AreEqual(1, requested);
            Assert.IsTrue(NotificationPrefs.IsOn(NotificationPrefs.BoxReady));
        }
        [Test] public void DeniedPermissionDirectsToPhoneSettingsAndKeepsChoices()
        {
            int opened = 0; NotificationPrefs.Set(NotificationPrefs.BoxReady, false);
            hud.SetNotificationPermission(BoxNotificationPermission.Denied, () => Assert.Fail("Do not prompt again"), () => opened++);
            Assert.That(ActionButton.GetComponentInChildren<Text>().text, Does.Contain("settings"));
            ActionButton.onClick.Invoke();
            Assert.AreEqual(1, opened);
            Assert.IsFalse(NotificationPrefs.IsOn(NotificationPrefs.BoxReady));
            var note = hud.GetComponentsInChildren<Text>(true).Single(t => t.name == "Notification note");
            Assert.That(note.text, Does.Contain("turned off"));
        }
        [Test] public void PendingAndUnsupportedCannotPrompt()
        {
            foreach (var state in new[]{BoxNotificationPermission.Pending, BoxNotificationPermission.Unsupported})
            {
                hud.SetNotificationPermission(state, () => Assert.Fail("No prompt"), () => Assert.Fail("No redirect"));
                Assert.IsFalse(ActionButton.interactable);
            }
        }
        [Test] public void AllowedPermissionShowsDailyLimitAndSettingsShortcut()
        {
            int opened = 0;
            hud.SetNotificationPermission(BoxNotificationPermission.Granted, () => Assert.Fail("Already allowed"), () => opened++);
            var note = hud.GetComponentsInChildren<Text>(true).Single(t => t.name == "Notification note");
            Assert.That(note.text, Does.Contain("at most two"));
            ActionButton.onClick.Invoke(); Assert.AreEqual(1, opened);
        }
    }
}
