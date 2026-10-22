using System;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Pockle.Editor
{
    public static class BackendFoundationBuild
    {
        public static void Build()
        {
            if(PrototypeHud.UiBuild!="v2 screens 33"||Resources.Load<TextAsset>("Backend/PockleBackend")==null)throw new Exception("Backend/UI dependency guard failed");
            EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{PockleProjectSetup.ScenePath},locationPathName="/private/tmp/Pockle-Backend-Foundation.apk",target=BuildTarget.Android,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Backend Android build failed: "+report.summary.result);
            Debug.Log("BACKEND_FOUNDATION_ANDROID_OK "+report.summary.totalSize);
        }
    }
}
