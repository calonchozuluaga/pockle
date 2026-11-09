using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Pockle.Runtime
{
    internal static class NativeAvatarPicker
    {
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern bool PockleAvatarPick(string request, string destination);
        [DllImport("__Internal")] private static extern IntPtr PockleAvatarResult();
        [DllImport("__Internal")] private static extern void PockleAvatarFree(IntPtr value);
        [DllImport("__Internal")] private static extern void PockleAvatarCancel(string request);
#endif
        public static bool Begin(string request, string destination)
        {
            try
            {
#if UNITY_IOS && !UNITY_EDITOR
            return PockleAvatarPick(request, destination);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var picker = new AndroidJavaClass("studio.pockle.avatar.AvatarPickerActivity"))
                return picker.CallStatic<bool>("pick", activity, request, destination);
#else
            return false;
#endif
            }
            catch (Exception) { return false; } // Missing native plugin must leave the saved avatar unchanged.
        }
        public static string TakeResult(string request)
        {
            try
            {
#if UNITY_IOS && !UNITY_EDITOR
            var value = PockleAvatarResult(); try { return Marshal.PtrToStringAnsi(value) ?? ""; } finally { PockleAvatarFree(value); }
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (var picker = new AndroidJavaClass("studio.pockle.avatar.AvatarPickerActivity")) return picker.CallStatic<string>("takeResult");
#else
            return "";
#endif
            }
            catch (Exception) { return "{\"request\":\"" + request + "\",\"status\":\"error\"}"; }
        }
        public static void Cancel(string request)
        {
            try
            {
#if UNITY_IOS && !UNITY_EDITOR
            PockleAvatarCancel(request);
#elif UNITY_ANDROID && !UNITY_EDITOR
            using (var picker = new AndroidJavaClass("studio.pockle.avatar.AvatarPickerActivity")) picker.CallStatic("cancel", request);
#endif
            }
            catch (Exception) { } // Always allow managed cleanup/account switching even if the OS bridge is unavailable.
        }
    }
}
