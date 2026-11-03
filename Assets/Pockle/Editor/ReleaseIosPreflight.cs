using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Pockle.Editor
{
    /// <summary>Diagnostic iOS export only; no signing, store upload, or release identity changes.</summary>
    public static class ReleaseIosPreflight
    {
        public const string OutputPath = "/private/tmp/Pockle-iOS-Preflight";
        public static void Export()
        {
            if (Application.unityVersion != "6000.6.5f1") throw new InvalidOperationException("Pinned Unity version required.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS)) throw new InvalidOperationException("iOS Build Support missing.");
            if (!File.Exists(PockleProjectSetup.ScenePath)) throw new InvalidOperationException("Expected tactile scene missing.");
            if (Resources.Load<TextAsset>("Backend/PockleBackend") == null) throw new InvalidOperationException("Account dependency missing.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { PockleProjectSetup.ScenePath }, locationPathName = OutputPath,
                target = BuildTarget.iOS, options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("iOS preflight export failed: " + report.summary.result);
            Debug.Log("POCKLE_IOS_PREFLIGHT_EXPORT_OK " + report.summary.totalSize);
        }
    }
}
