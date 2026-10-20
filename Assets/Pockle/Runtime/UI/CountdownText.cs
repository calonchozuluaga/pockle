using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Counts down to the next daily box (the next UTC midnight, when the daily reset happens) and rewrites its text once a
    /// second, only while it's on screen. "7:42:10".
    /// </summary>
    public sealed class CountdownText : MonoBehaviour
    {
        public Text Target;
        private int lastSecond = -1;

        public static TimeSpan UntilNextBox(DateTime utcNow) => utcNow.Date.AddDays(1) - utcNow;

        public static string Format(TimeSpan left)
        {
            if (left < TimeSpan.Zero) left = TimeSpan.Zero;
            return (int)left.TotalHours + ":" + left.Minutes.ToString("00") + ":" + left.Seconds.ToString("00");
        }

        private void OnEnable() { lastSecond = -1; Tick(); }
        private void Update() => Tick();

        private void Tick()
        {
            if (Target == null) return;
            var left = UntilNextBox(DateTime.UtcNow);
            int second = (int)left.TotalSeconds;
            if (second == lastSecond) return;
            lastSecond = second;
            Target.text = Format(left);
        }
    }
}
