#if UNITY_IOS
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;
using UnityEngine;
namespace Pockle.Editor
{
    /// <summary>Production-identity beta export; never alters the installed owner app or uploads to Apple.</summary>
    public static class TestFlightIosBuild
    {
        public const string BundleId = "com.playpockle.app";
        public static void Export()
        {
            if (Application.unityVersion != "6000.6.5f1") throw new InvalidOperationException("Pinned Unity version required.");
            string number = Environment.GetEnvironmentVariable("POCKLE_TESTFLIGHT_BUILD_NUMBER");
            if (!int.TryParse(number, out int parsed) || parsed < 1) throw new InvalidOperationException("Set a positive POCKLE_TESTFLIGHT_BUILD_NUMBER.");
            var platform = NamedBuildTarget.iOS;
            string identifier = PlayerSettings.GetApplicationIdentifier(platform), oldNumber = PlayerSettings.iOS.buildNumber, oldVersion = PlayerSettings.bundleVersion;
            var savedFlags = new Dictionary<string, byte[]>();
            try
            {
                // Some legacy grants are flag-driven even outside development builds. Disable every owner flag in the exported player.
                foreach (string file in Directory.GetFiles("Assets/Pockle/Resources", "*OwnerTest*.txt", SearchOption.TopDirectoryOnly))
                {
                    savedFlags.Add(file, File.ReadAllBytes(file));
                    File.WriteAllText(file, "disabled\n");
                    AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceSynchronousImport);
                }
                PlayerSettings.SetApplicationIdentifier(platform, BundleId);
                PlayerSettings.iOS.buildNumber = number;
                PlayerSettings.bundleVersion = "1.0";
                PockleAppIcon.Apply();
                string output = Path.GetFullPath("Builds/iOS-TestFlight");
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[]{PockleProjectSetup.ScenePath}, locationPathName = output,
                    target = BuildTarget.iOS, options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("TestFlight export failed: " + report.summary.result);
                var pbx = new PBXProject(); string project = PBXProject.GetPBXProjectPath(output); pbx.ReadFromFile(project);
                pbx.AddCapability(pbx.GetUnityMainTargetGuid(), PBXCapabilityType.InAppPurchase);
                pbx.WriteToFile(project);
                Debug.Log("POCKLE_TESTFLIGHT_EXPORT_OK " + output + " bundle=" + BundleId + " build=" + number + " disabledOwnerFlags=" + savedFlags.Count);
            }
            finally
            {
                PlayerSettings.SetApplicationIdentifier(platform, identifier); PlayerSettings.iOS.buildNumber = oldNumber; PlayerSettings.bundleVersion = oldVersion;
                foreach (var flag in savedFlags)
                {
                    File.WriteAllBytes(flag.Key, flag.Value);
                    AssetDatabase.ImportAsset(flag.Key, ImportAssetOptions.ForceSynchronousImport);
                }
            }
        }
    }
}
#endif
