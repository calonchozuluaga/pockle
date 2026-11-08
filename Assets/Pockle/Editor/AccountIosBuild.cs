#if UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
namespace Pockle.Editor
{
    public static class AccountIosBuild
    {
        [PostProcessBuild(50)] public static void Configure(BuildTarget target,string path)
        {
            if(target!=BuildTarget.iOS)return;
            string project=PBXProject.GetPBXProjectPath(path);var pbx=new PBXProject();pbx.ReadFromFile(project);
            pbx.AddFrameworkToProject(pbx.GetUnityFrameworkTargetGuid(),"Security.framework",false);pbx.AddFrameworkToProject(pbx.GetUnityFrameworkTargetGuid(),"CoreMotion.framework",false);pbx.WriteToFile(project);
            string file=Path.Combine(path,"Info.plist");var plist=new PlistDocument();plist.ReadFromFile(file);
            plist.root.SetString("NSMotionUsageDescription","Pockle uses your step count to track walks and earn walking boxes. No location data is needed.");
            var types=plist.root.values.ContainsKey("CFBundleURLTypes")?plist.root["CFBundleURLTypes"].AsArray():plist.root.CreateArray("CFBundleURLTypes");
            var entry=types.AddDict();entry.SetString("CFBundleURLName","Pockle account");entry.CreateArray("CFBundleURLSchemes").AddString("com.playpockle.app");plist.WriteToFile(file);
        }
    }
}
#endif
