#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Pockle.Editor
{
    /// <summary>Keep the phone's time, battery and connection indicators visible in every iOS export.</summary>
    public static class MobileSystemBarsBuild
    {
        [PostProcessBuild(60)]
        public static void Configure(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;
            var file = Path.Combine(path, "Info.plist");
            var plist = new PlistDocument();
            plist.ReadFromFile(file);
            plist.root.SetBoolean("UIStatusBarHidden", false);
            plist.root.SetString("UIStatusBarStyle", "UIStatusBarStyleDefault");
            plist.WriteToFile(file);
        }
    }
}
#endif
