using System;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Pockle.Editor
{
    // First Play upload establishes the package. Checkout remains unavailable.
    public sealed class PlayInternalBundleBuild : IPostGenerateGradleAndroidProject
    {
        private static bool building;
        public int callbackOrder => 100;

        public static void Build()
        {
            var platform = NamedBuildTarget.Android;
            string originalId = PlayerSettings.GetApplicationIdentifier(platform);
            var originalBackend = PlayerSettings.GetScriptingBackend(platform);
            var originalArchitectures = PlayerSettings.Android.targetArchitectures;
            var originalMin = PlayerSettings.Android.minSdkVersion;
            var originalTarget = PlayerSettings.Android.targetSdkVersion;
            bool originalBundle = EditorUserBuildSettings.buildAppBundle;
            bool originalExport = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            bool originalKey = PlayerSettings.Android.useCustomKeystore;
            int originalCode = PlayerSettings.Android.bundleVersionCode;
            try
            {
                building = true;
                PlayerSettings.SetApplicationIdentifier(platform, "com.playpockle.app");
                PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
                PlayerSettings.Android.bundleVersionCode = 1;
                // Owner signs a separate copy with their private upload key afterward.
                PlayerSettings.Android.useCustomKeystore = false;
                EditorUserBuildSettings.buildAppBundle = true;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                string output = Path.GetFullPath("Builds/Pockle-Play-Internal.debug-signed.aab");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[] { PockleProjectSetup.ScenePath },
                    locationPathName = output, target = BuildTarget.Android,
                    options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new Exception("Play bootstrap bundle failed: " + report.summary.result);
                Debug.Log("POCKLE_PLAY_BOOTSTRAP_AAB_OK:" + output);
            }
            finally
            {
                building = false;
                PlayerSettings.SetApplicationIdentifier(platform, originalId);
                PlayerSettings.SetScriptingBackend(platform, originalBackend);
                PlayerSettings.Android.targetArchitectures = originalArchitectures;
                PlayerSettings.Android.minSdkVersion = originalMin;
                PlayerSettings.Android.targetSdkVersion = originalTarget;
                PlayerSettings.Android.bundleVersionCode = originalCode;
                PlayerSettings.Android.useCustomKeystore = originalKey;
                EditorUserBuildSettings.buildAppBundle = originalBundle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = originalExport;
            }
        }

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if (!building) return;
            // Declare the planned billing capability for Play catalog setup only.
            // This does not install a billing SDK, enable checkout, or grant purchases.
            string manifest = Path.Combine(path, "src/main/AndroidManifest.xml");
            var xml = new XmlDocument(); xml.Load(manifest);
            const string ns = "http://schemas.android.com/apk/res/android";
            var permission = xml.CreateElement("uses-permission");
            permission.SetAttribute("name", ns, "com.android.vending.BILLING");
            xml.DocumentElement.AppendChild(permission); xml.Save(manifest);
        }
    }
}
