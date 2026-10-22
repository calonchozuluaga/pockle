using System;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Pockle.Editor
{
    public static class WallyPilotPhoneBuild
    {
        public static void Build()
        {
            if(PrototypeHud.UiBuild!="v2 screens 33"||Resources.Load<TextAsset>("Backend/PockleBackend")==null)throw new Exception("Backend/UI dependency guard failed");
            if (!Pockle.Core.ToyCatalog.TryGetCollection("wally", out var wally) || wally.Members.Count != 15) throw new Exception("Wally pilot catalog missing");
            EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{PockleProjectSetup.ScenePath},locationPathName="/private/tmp/Pockle-Wally-Pilot.apk",target=BuildTarget.Android,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Wally Android build failed: "+report.summary.result);
            Debug.Log("WALLY_PILOT_ANDROID_OK "+report.summary.totalSize);
        }
    }
}
