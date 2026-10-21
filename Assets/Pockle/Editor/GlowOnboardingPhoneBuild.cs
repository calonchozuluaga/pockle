using System;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace Pockle.Editor { public static class GlowOnboardingPhoneBuild {
public static void Build(){
if(PrototypeHud.UiBuild!="v2 screens 31" || ToyCatalog.TryGetCollection("tula",out _))throw new Exception("UI/roster build guard failed");
foreach(var name in new[]{"Play","Fuse","Boxes","Badges","Arrange"})if(Resources.Load<Texture2D>("UI/Features/Feature"+name)==null)throw new Exception("Missing feature painting: "+name);
PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"com.PocklePrototype.Pockle");
EditorUserBuildSettings.buildAppBundle=false;EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{PockleProjectSetup.ScenePath},locationPathName="/Users/calonchozulu/Desktop/pockle/Builds/Pockle-Ui31-Glow-Onboarding.apk",target=BuildTarget.Android,options=BuildOptions.Development});
if(report.summary.result!=BuildResult.Succeeded)throw new Exception("UI31 Android build failed: "+report.summary.result);
Debug.Log("UI31_GLOW_ONBOARDING_ANDROID_OK "+report.summary.totalSize);
}}}
