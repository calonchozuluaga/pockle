#if UNITY_ANDROID
using System.IO;
using System.Xml;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEngine;

namespace Pockle.Editor
{
    // RevenueCat requires standard/singleTop so switching to a bank app does not cancel checkout.
    public sealed class PaidBoxAndroidBuild : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 20000;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            string manifest = Path.Combine(path, "src/main/AndroidManifest.xml");
            var xml = new XmlDocument(); xml.Load(manifest);
            const string android = "http://schemas.android.com/apk/res/android";
            bool found = false;
            foreach (XmlElement activity in xml.GetElementsByTagName("activity"))
            {
                if (activity.GetAttribute("name", android) != "com.unity3d.player.UnityPlayerActivity") continue;
                activity.SetAttribute("launchMode", android, "singleTop"); found = true;
            }
            if (!found) throw new BuildFailedException("Cannot configure the checkout activity launch mode.");
            xml.Save(manifest);
            // Unity 6 can still emit its logo in the fallback PNG even when the
            // adaptive layers are assigned. Keep that store/launcher fallback approved too.
            string fallback = Path.Combine(Directory.GetParent(path).FullName, "launcher/src/main/res/mipmap-mdpi/app_icon.png");
            if (!File.Exists(fallback)) throw new BuildFailedException("Cannot find the Android icon fallback.");
            var source = new Texture2D(2, 2, TextureFormat.RGB24, false);
            var resized = new Texture2D(96, 96, TextureFormat.RGB24, false);
            try
            {
                if (!ImageConversion.LoadImage(source, File.ReadAllBytes(PockleAppIcon.AssetPath)))
                    throw new BuildFailedException("Cannot decode the approved icon fallback.");
                for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++)
                    resized.SetPixel(x, y, source.GetPixelBilinear((x + .5f) / 96, (y + .5f) / 96));
                resized.Apply(); File.WriteAllBytes(fallback, ImageConversion.EncodeToPNG(resized));
            }
            finally { Object.DestroyImmediate(source); Object.DestroyImmediate(resized); }
        }
    }
}
#endif
