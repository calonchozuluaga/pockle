using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Pockle.Core;

namespace Pockle.Editor
{
    // Internal purchase integration. Server policy keeps checkout unavailable until configured and verified.
    public sealed class PlayInternalBundleBuild
    {

        public static void Build()
        {
            if (Application.unityVersion != "6000.6.5f1") throw new InvalidOperationException("Pinned Unity version required.");
            string requested = Environment.GetEnvironmentVariable("POCKLE_PLAY_BUILD_NUMBER");
            int code = 4;
            if (!string.IsNullOrEmpty(requested) && (!int.TryParse(requested, out code) || code < 1))
                throw new InvalidOperationException("POCKLE_PLAY_BUILD_NUMBER must be positive.");
            var flags = new Dictionary<string, byte[]>();
            const string channelPath = "Assets/Pockle/Resources/Backend/CheckoutBuildChannel.txt";
            byte[] originalChannel = File.ReadAllBytes(channelPath);
            string community = Environment.GetEnvironmentVariable("POCKLE_PLAY_COMMUNITY_BETA");
            if (!string.IsNullOrEmpty(community) && community != "1" && community != "0")
                throw new InvalidOperationException("POCKLE_PLAY_COMMUNITY_BETA must be 0 or 1.");
            string channel = community == "1" ? CheckoutBuildPolicy.CommunityBeta : CheckoutBuildPolicy.OwnerSandbox;
            string originalVersion = PlayerSettings.bundleVersion;
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
                File.WriteAllText(channelPath, channel + "\n");
                AssetDatabase.ImportAsset(channelPath, ImportAssetOptions.ForceSynchronousImport);
                foreach (string file in Directory.GetFiles("Assets/Pockle/Resources", "*OwnerTest*.txt", SearchOption.TopDirectoryOnly))
                {
                    flags.Add(file, File.ReadAllBytes(file));
                    File.WriteAllText(file, "disabled\n");
                    AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceSynchronousImport);
                }
                PlayerSettings.bundleVersion = "1.0";
                PlayerSettings.SetApplicationIdentifier(platform, "com.playpockle.app");
                PlayerSettings.SetScriptingBackend(platform, ScriptingImplementation.IL2CPP);
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
                PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)36;
                PlayerSettings.Android.bundleVersionCode = code;
                // Owner signs a separate copy with their private upload key afterward.
                PlayerSettings.Android.useCustomKeystore = false;
                EditorUserBuildSettings.buildAppBundle = true;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                string output = Path.GetFullPath("Builds/Pockle-Play-Internal-v" + code + ".debug-signed.aab");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                PockleAppIcon.Apply();
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                    scenes = new[] { PockleProjectSetup.ScenePath },
                    locationPathName = output, target = BuildTarget.Android,
                    options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new Exception("Play bootstrap bundle failed: " + report.summary.result);
                Debug.Log("POCKLE_PLAY_BOOTSTRAP_AAB_OK:" + output + " channel=" + channel);
            }
            finally
            {
                File.WriteAllBytes(channelPath, originalChannel);
                AssetDatabase.ImportAsset(channelPath, ImportAssetOptions.ForceSynchronousImport);
                PlayerSettings.bundleVersion = originalVersion;
                foreach (var flag in flags)
                {
                    File.WriteAllBytes(flag.Key, flag.Value);
                    AssetDatabase.ImportAsset(flag.Key, ImportAssetOptions.ForceSynchronousImport);
                }
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

    }
}
