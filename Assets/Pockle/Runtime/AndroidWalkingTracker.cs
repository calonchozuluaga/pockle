using System;
using UnityEngine;

namespace Pockle.Runtime
{
    internal sealed class AndroidWalkingTracker : IDisposable
    {
        public bool Failed { get; private set; }
        public string Status { get; private set; } = "Walking is available on supported Android phones.";
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string Permission = "android.permission.ACTIVITY_RECOGNITION";
        private AndroidJavaObject counter;
        private bool requested;
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
            if (Failed) return;
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
            { ReportFailure(exception, "Step tracking is unavailable on this device."); }
#endif
        }

        public bool TryRead(out long total, out double uptime, out int boot)
        {
            total = -1; uptime = 0; boot = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (counter == null || Failed || !HasPermission()) return false;
            try
            {
                total = counter.Call<long>("total");
                uptime = counter.Call<long>("uptimeMillis") / 1000d;
                boot = counter.Call<int>("boot");
                return total >= 0;
            }
            catch (Exception exception) { ReportFailure(exception, "Step tracking stopped. Reopen Pockle to retry."); }
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
        private void ReportFailure(Exception exception, string status)
        {
            Failed = true;
            Status = status;
            if (Debug.isDebugBuild) Status += " (" + exception.GetType().Name + ")";
            Debug.LogWarning("Pockle step counter: " + exception);
        }

        public void Dispose()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (counter != null)
            {
                try { counter.Call("stop"); }
                catch (Exception exception) { Debug.LogWarning("Pockle step counter cleanup: " + exception); }
                finally { counter.Dispose(); counter = null; }
            }
#endif
        }
    }
}
