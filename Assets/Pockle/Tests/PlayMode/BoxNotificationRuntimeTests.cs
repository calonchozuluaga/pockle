using NUnit.Framework;
using Pockle.Core;

namespace Pockle.Tests
{
    public sealed class BoxNotificationRuntimeTests
    {
        [Test] public void RecoveryAndForegroundAreSilentAndCannotRepeat()
        {
            var p = new BoxNotificationPolicy();
            Assert.IsFalse(p.Observe(20, 1, new[]{"a"}, false));
            Assert.IsFalse(p.Observe(20, 2, new[]{"a","b"}, true));
            Assert.IsFalse(p.Observe(20, 2, new[]{"a","b"}, false));
            Assert.IsTrue(p.Observe(20, 3, new[]{"b","c"}, false));
            Assert.IsFalse(p.Observe(20, 4, new[]{"c","d"}, false));
        }
        [Test] public void OpeningOrReplayingAnOldSnapshotDoesNotEarn()
        {
            var p = new BoxNotificationPolicy();
            p.Observe(20, 2, new[]{"a","b"}, true);
            Assert.IsFalse(p.Observe(20, 2, new[]{"b"}, false));
            Assert.IsFalse(p.Observe(20, 2, new[]{"a","b"}, false));
            Assert.IsFalse(p.Observe(19, 3, new[]{"new"}, false));
        }
        [Test] public void TwoKindsHaveAtMostOneSlotEachPerServerDay()
        {
            var p = new BoxNotificationPolicy();
            p.Observe(20, 0, new string[0], true);
            Assert.IsTrue(p.ReserveEligibility(20));
            Assert.IsFalse(p.ReserveEligibility(20));
            Assert.IsTrue(p.Observe(20, 1, new[]{"a"}, false));
            Assert.IsFalse(p.Observe(20, 2, new[]{"b"}, false));
            Assert.IsTrue(p.ReserveEligibility(21));
            Assert.IsTrue(p.Observe(21, 1, new[]{"c"}, false));
        }
        [Test] public void PersistedHistorySuppressesDuplicatesAfterRestart()
        {
            var p = new BoxNotificationPolicy();
            p.Observe(20, 0, new string[0], true);
            p.ReserveEligibility(20); p.Observe(20, 1, new[]{"a"}, false);
            var restored = UnityEngine.JsonUtility.FromJson<BoxNotificationPolicy>(UnityEngine.JsonUtility.ToJson(p));
            Assert.IsFalse(restored.ReserveEligibility(20));
            Assert.IsFalse(restored.Observe(20, 1, new[]{"a"}, false));
            Assert.IsFalse(restored.Observe(20, 2, new[]{"b"}, false));
        }
    }
}
