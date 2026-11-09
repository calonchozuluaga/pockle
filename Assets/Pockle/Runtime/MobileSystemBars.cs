using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace Pockle.Runtime
{
    /// <summary>Restore Android's status bar after launch and native account/photo-picker overlays.</summary>
    public sealed class MobileSystemBars : MonoBehaviour
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var host = new GameObject("Pockle system status bar");
            DontDestroyOnLoad(host);
            host.AddComponent<MobileSystemBars>();
        }

        private void Start() { ShowStatusBar(); }
        private void OnApplicationFocus(bool focused) { if (focused) ShowStatusBar(); }
        private void OnApplicationPause(bool paused) { if (!paused) ShowStatusBar(); }

        private static void ShowStatusBar()
        {
            var insets = AndroidApplication.currentWindowInsets;
            if (insets != null) insets.Show(AndroidWindowInsets.Type.StatusBars);
        }
#endif
    }
}
