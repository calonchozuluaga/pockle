using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class WalkingFailureTests
    {
        [Test]
        public void SessionExposesPluginFailureWithoutParsingStatusText()
        {
            Type trackerType = typeof(CollectionSession).Assembly.GetType("Pockle.Runtime.AndroidWalkingTracker", true);
            object tracker = Activator.CreateInstance(trackerType, true);
            var root = new GameObject("Walking failure contract test");
            try
            {
                var session = root.AddComponent<CollectionSession>();
                Assert.That(session.WalkingFailed, Is.False);
                typeof(CollectionSession).GetField("tracker", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(session, tracker);
                Assert.That(session.WalkingFailed, Is.False, "An idle tracker is not a plugin failure.");
                LogAssert.Expect(LogType.Warning, new Regex("Pockle step counter: System.InvalidOperationException: walking test"));
                trackerType.GetMethod("ReportFailure", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(tracker, new object[] { new InvalidOperationException("walking test"), "Readable status" });
                Assert.That(session.WalkingFailed, Is.True);
                string status = (string)trackerType.GetProperty("Status").GetValue(tracker);
                Assert.That(status, Does.StartWith("Readable status"));
                if (Debug.isDebugBuild) Assert.That(status, Does.Contain("InvalidOperationException"));
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
