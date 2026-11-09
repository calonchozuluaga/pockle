#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Pockle.Tests
{
    public sealed class CommunityUiTests
    {
        [UnityTest]
        public IEnumerator UnconnectedSectionsNeverOfferFakeActivityOrTrades()
        {
            var root = new GameObject("Community availability test");
            try
            {
                var session = root.AddComponent<CollectionSession>(); session.Initialize();
                var before = session.OwnedCounts.ToDictionary(p => p.Key, p => p.Value);
                var hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session);
                yield return null;
                typeof(PrototypeHud).GetMethod("Navigate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, new object[] { AppPage.Social });
                foreach (string section in new[] { "Feed", "Friends", "Exchange" })
                {
                    root.GetComponentsInChildren<Button>().Single(b => b.name == "Community section · " + section).onClick.Invoke();
                    yield return null;
                    var text = root.GetComponentsInChildren<Text>().Select(t => t.text).ToArray();
                    CollectionAssert.Contains(text, "COMING SOON");
                    Assert.IsTrue(text.Any(t => t.Contains("coming soon")));
                    Assert.IsFalse(root.GetComponentsInChildren<Button>().Any(b => b.name.Contains("Accept offer") || b.name.Contains("Post offer") || b.name.Contains("Send request")));
                    Assert.AreEqual(AppPage.Social, hud.CurrentPage);
                }
                CollectionAssert.AreEquivalent(before, session.OwnedCounts);
                root.GetComponentsInChildren<Button>().Single(b => b.name == "Make your profile yours").onClick.Invoke();
                Assert.AreEqual(AppPage.Profile, hud.CurrentPage);
                Assert.IsTrue(hud.GoBack()); Assert.AreEqual(AppPage.Social, hud.CurrentPage);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
    }
}
#endif
