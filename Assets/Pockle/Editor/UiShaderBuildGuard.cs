using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pockle.Editor
{
    /// <summary>
    /// Stops any player build whose Graphics settings leave out the built-in UI shader.
    /// A fresh checkout once generated an empty "Always Included Shaders" list, and every
    /// uGUI Text, Image and RawImage drew in solid magenta on the phone (TestFlight 15).
    /// </summary>
    public sealed class UiShaderBuildGuard : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            string problem = FindProblem();
            if (problem != null) throw new BuildFailedException(problem);
        }

        public static string FindProblem()
        {
            Shader ui = Shader.Find("UI/Default");
            if (ui == null) return "UI/Default shader not found.";
            var settings = new SerializedObject(GraphicsSettings.GetGraphicsSettings());
            SerializedProperty list = settings.FindProperty("m_AlwaysIncludedShaders");
            if (list == null) return "Graphics settings have no Always Included Shaders list.";
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == ui) return null;
            return "UI/Default is missing from Project Settings > Graphics > Always Included Shaders; " +
                   "the build would draw all UI in magenta. Restore ProjectSettings/GraphicsSettings.asset from git.";
        }
    }
}
