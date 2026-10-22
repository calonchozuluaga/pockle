using UnityEngine;
using System;
using System.Runtime.InteropServices;
namespace Pockle.Runtime
{
    internal static class AccountTokenStore
    {
        // Editor/unsupported platforms use memory only, never plaintext session persistence.
        private static string memory="";
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern bool PockleSecureSet(string value);
        [DllImport("__Internal")] private static extern IntPtr PockleSecureGet();
        [DllImport("__Internal")] private static extern void PockleSecureFree(IntPtr value);
#endif
        internal static string Read()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
            using(var secure=new AndroidJavaClass("studio.pockle.account.SecureSession"))return secure.CallStatic<string>("get",activity,"pockle-session-v1");
#elif UNITY_IOS && !UNITY_EDITOR
            var value=PockleSecureGet();try{return Marshal.PtrToStringAnsi(value)??"";}finally{PockleSecureFree(value);}
#else
            return memory;
#endif
        }
        internal static void Write(string value)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using(var player=new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using(var activity=player.GetStatic<AndroidJavaObject>("currentActivity"))
            using(var secure=new AndroidJavaClass("studio.pockle.account.SecureSession"))secure.CallStatic("put",activity,"pockle-session-v1",value);
#elif UNITY_IOS && !UNITY_EDITOR
            if(!PockleSecureSet(value??""))throw new InvalidOperationException("Secure session write failed.");
#else
            memory=value??"";
#endif
        }
    }
}
