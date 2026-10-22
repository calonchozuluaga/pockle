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
        /// <summary>When set (server walking boxes), the time left comes from here instead of the next UTC midnight.
        /// Returning null shows <see cref="Waiting"/>.</summary>
        public Func<TimeSpan?> Source;
        public string Waiting = "Checking…";
        /// <summary>Words before the time, e.g. "Next box in ".</summary>
        public string Prefix = "";
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
            TimeSpan? given = Source != null ? Source() : UntilNextBox(DateTime.UtcNow);
            int second = given.HasValue ? (int)Math.Max(0, given.Value.TotalSeconds) : -2;
            if (second == lastSecond) return;
            lastSecond = second;
            Target.text = given.HasValue ? Prefix + Format(given.Value) : Waiting;
        }
    }
}
