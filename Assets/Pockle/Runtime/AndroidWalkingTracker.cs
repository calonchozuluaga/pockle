using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pockle.Runtime
{
    internal sealed class AndroidWalkingTracker : IDisposable
    {
        public bool Failed { get; private set; }
        /// <summary>Successful sensor registration with permission still held; independent of saved steps/status copy.</summary>
        public bool Active
        {
            get
            {
#if UNITY_ANDROID && !UNITY_EDITOR
                if (!HasPermission()) started = false;
                return started && !Failed;
#elif UNITY_IOS && !UNITY_EDITOR
                return !Failed && PockleStepsState() == 2;
#else
                return false;
#endif
            }
        }
        public string Status { get; private set; } = "Enable walking on a supported phone.";
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string Permission = "android.permission.ACTIVITY_RECOGNITION";
        private AndroidJavaObject counter;
        private bool requested;
        private bool started;
#endif
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void PockleStepsResume(int request);
        [DllImport("__Internal")] private static extern int PockleStepsState();
        [DllImport("__Internal")] private static extern long PockleStepsTotal();
        [DllImport("__Internal")] private static extern double PockleStepsUptime();
        [DllImport("__Internal")] private static extern int PockleStepsBoot();
        [DllImport("__Internal")] private static extern void PockleStepsStop();
        private void RefreshIosStatus()
        {
            switch (PockleStepsState())
            {
                case 1: Status = "Allow Motion & Fitness to count steps."; break;
                case 2: Status = "Your iPhone counts steps with Motion & Fitness. No location access needed."; break;
                case 3: Status = "Allow Motion & Fitness for Pockle in iPhone Settings."; break;
                case 4: Status = "Motion & Fitness access is restricted on this iPhone."; break;
                case 5: Status = "Step tracking isn't available on this iPhone."; break;
                case 6: Status = "Step tracking stopped. Tap Enable walking to retry."; break;
                default: Status = "Enable walking to allow Motion & Fitness access."; break;
            }
        }
#endif
        public void Enable()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!HasPermission())
            {
                started = false;
                if (!requested) { requested = true; UnityEngine.Android.Permission.RequestUserPermission(Permission); }
                Status = "Allow Physical activity in Android settings to count steps.";
                return;
            }
#elif UNITY_IOS && !UNITY_EDITOR
            try { PockleStepsResume(1); RefreshIosStatus(); }
            catch (Exception exception) { ReportFailure(exception, "Step tracking is unavailable on this iPhone."); }
            return;
#endif
            ResumeIfAllowed();
        }

        public void ResumeIfAllowed()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Failed) return;
            if (!HasPermission()) { started = false; Status = "Enable walking to allow Physical activity access."; return; }
            try
            {
                if (counter == null)
                {
                    using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                        counter = new AndroidJavaObject("studio.pockle.walking.StepCounter", activity);
                }
                bool available = counter.Call<bool>("start");
                started = available;
                Status = available ? "Keep Pockle running while you walk. No location access needed." : "This phone has no step-counter sensor.";
            }
            catch (Exception exception)
            { ReportFailure(exception, "Step tracking is unavailable on this device."); }
#elif UNITY_IOS && !UNITY_EDITOR
            if (Failed) return;
            try { PockleStepsResume(0); RefreshIosStatus(); }
            catch (Exception exception) { ReportFailure(exception, "Step tracking is unavailable on this iPhone."); }
#endif
        }

        public bool TryRead(out long total, out double uptime, out int boot)
        {
            total = -1; uptime = 0; boot = -1;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Active || counter == null) return false;
            try
            {
                total = counter.Call<long>("total");
                uptime = counter.Call<long>("uptimeMillis") / 1000d;
                boot = counter.Call<int>("boot");
                return total >= 0;
            }
            catch (Exception exception) { ReportFailure(exception, "Step tracking stopped. Reopen Pockle to retry."); }
#elif UNITY_IOS && !UNITY_EDITOR
            if (!Active) return false;
            total = PockleStepsTotal(); uptime = PockleStepsUptime(); boot = PockleStepsBoot();
            return total >= 0;
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
#if UNITY_ANDROID && !UNITY_EDITOR
            started = false;
#endif
            Status = status;
            if (Debug.isDebugBuild) Status += " (" + exception.GetType().Name + ")";
            Debug.LogWarning("Pockle step counter: " + exception);
        }

        public void Dispose()
        {
#if UNITY_IOS && !UNITY_EDITOR
            PockleStepsStop();
#elif UNITY_ANDROID && !UNITY_EDITOR
            started = false;
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
