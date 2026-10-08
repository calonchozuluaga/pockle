using System;
using UnityEngine;

namespace Pockle.Runtime
{
    internal sealed class AndroidWalkingTracker : IDisposable
    {
        public string Status { get; private set; } = "Walking is available on supported Android phones.";
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string Permission = "android.permission.ACTIVITY_RECOGNITION";
        private AndroidJavaObject counter;
        private bool requested;
        private bool failed;
#endif
        public void Enable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!HasPermission())
            {
                if (!requested) { requested = true; UnityEngine.Android.Permission.RequestUserPermission(Permission); }
                Status = "Allow Physical activity in Android settings to count steps.";
                return;
            }
#endif
            ResumeIfAllowed();
        }

        public void ResumeIfAllowed()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (failed) return;
            if (!HasPermission()) { Status = "Enable walking to allow Physical activity access."; return; }
            try
            {
                if (counter == null)
                {
                    using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                        counter = new AndroidJavaObject("studio.pockle.walking.StepCounter", activity);
                }
                bool available = counter.Call<bool>("start");
                Status = available ? "Keep Pockle running while you walk. No location access needed." : "This phone has no step-counter sensor.";
            }
            catch (Exception exception)
            { failed = true; Status = "Step tracking is unavailable on this device."; Debug.LogWarning("Pockle step counter: " + exception.GetType().Name); }
#endif
        }

        public bool TryRead(out long total, out double uptime, out int boot)
        {
            total = -1; uptime = 0; boot = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (counter == null || failed || !HasPermission()) return false;
            try
            {
                total = counter.Call<long>("total");
                uptime = counter.Call<long>("uptimeMillis") / 1000d;
                boot = counter.Call<int>("boot");
                return total >= 0;
            }
            catch (Exception) { failed = true; Status = "Step tracking stopped. Reopen Pockle to retry."; }
#endif
            return false;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static bool HasPermission()
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                return version.GetStatic<int>("SDK_INT") < 29 || UnityEngine.Android.Permission.HasUserAuthorizedPermission(Permission);
        }
#endif
        public void Dispose()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (counter != null) { counter.Call("stop"); counter.Dispose(); counter = null; }
#endif
        }
    }
}
