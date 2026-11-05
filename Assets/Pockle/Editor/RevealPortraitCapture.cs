using System.IO;
using System.Reflection;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class RevealPortraitCapture
    {
        public static void Capture()
        {
            string folder = System.Environment.GetEnvironmentVariable("POCKLE_PORTRAIT_OUTPUT") ?? "/private/tmp/pockle-reveal-portraits";
            Directory.CreateDirectory(folder);
            var type = typeof(JellyToy).Assembly.GetType("Pockle.Runtime.PortraitCutout");
            foreach (string id in new[] { "fox.mochi-foam", "fox.gloss-vinyl", "pip.peach-jelly" })
            {
                var args = new object[] { id, 0f };
                var texture = (Texture2D)type.GetMethod("Render").Invoke(null, args);
                File.WriteAllBytes(Path.Combine(folder, id + ".png"), texture.EncodeToPNG());
                Debug.Log("PORTRAIT_CAPTURE " + id + " " + texture.width + "x" + texture.height);
                Object.DestroyImmediate(texture);
            }
        }
    }
}
