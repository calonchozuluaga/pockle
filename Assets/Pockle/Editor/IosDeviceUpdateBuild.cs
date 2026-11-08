using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Pockle.Editor
{
    public static class IosDeviceUpdateBuild
    {
        public static void Export()
        {
            if (Application.unityVersion != "6000.6.5f1") throw new InvalidOperationException("Pinned Unity version required.");
            var platform = NamedBuildTarget.iOS;
            string id = PlayerSettings.GetApplicationIdentifier(platform), number = PlayerSettings.iOS.buildNumber;
            try
            {
                // Update the already installed development app; do not create a second store-ID app with an empty save.
                PlayerSettings.SetApplicationIdentifier(platform, "studio.pockle.prototype");
                PlayerSettings.iOS.buildNumber = "5";
                PockleAppIcon.Apply();
                string path = Path.GetFullPath("Builds/iOS-Device-Update");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { PockleProjectSetup.ScenePath },
                    locationPathName = path, target = BuildTarget.iOS, options = BuildOptions.Development });
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("iOS export failed: " + report.summary.result);
                Debug.Log("POCKLE_IOS_DEVICE_UPDATE_EXPORT_OK " + path);
            }
            finally { PlayerSettings.SetApplicationIdentifier(platform, id); PlayerSettings.iOS.buildNumber = number; }
        }
    }
}
