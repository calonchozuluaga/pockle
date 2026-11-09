#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
namespace Pockle.Editor
{
    public static class AvatarIosBuild
    {
        [PostProcessBuild(55)] public static void Configure(BuildTarget target, string path)
        {
            if (target != BuildTarget.iOS) return;
            string project = PBXProject.GetPBXProjectPath(path); var pbx = new PBXProject(); pbx.ReadFromFile(project);
            pbx.AddFrameworkToProject(pbx.GetUnityFrameworkTargetGuid(), "PhotosUI.framework", true);
            pbx.AddFrameworkToProject(pbx.GetUnityFrameworkTargetGuid(), "UniformTypeIdentifiers.framework", true);
            pbx.AddFrameworkToProject(pbx.GetUnityFrameworkTargetGuid(), "ImageIO.framework", false);
            // PHPicker shares only the selected image. It does not need full photo-library permission.
            pbx.WriteToFile(project);
        }
    }
}
#endif
