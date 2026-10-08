using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pockle.Editor
{
    /// <summary>Editor setup and build guardrails for the deliberately small Built-in prototype.</summary>
    public sealed class PockleProjectSetup : IPreprocessBuildWithReport
    {
        public const string ScenePath = "Assets/Pockle/Scenes/PockleTactile.unity";
        public int callbackOrder { get { return 0; } }

        [MenuItem("Pockle/Open tactile prototype")]
        public static void OpenPrototype()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("Pockle/Prepare prototype settings")]
        public static void Prepare()
        {
            if (GraphicsSettings.currentRenderPipeline != null)
                throw new InvalidOperationException("Pockle uses the Built-in Render Pipeline. Remove the render pipeline asset in Graphics settings and Quality settings before running.");

            PlayerSettings.companyName = "Pockle Prototype";
            PlayerSettings.productName = "Pockle";
            PlayerSettings.defaultScreenWidth = 430;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            QualitySettings.antiAliasing = 2;
            QualitySettings.shadows = ShadowQuality.Disable;
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            Debug.Log("Pockle prototype prepared. Open the scene, press Play, and run EditMode tests. Choose phone/tablet Game view sizes to inspect layout.");
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            if (GraphicsSettings.currentRenderPipeline != null)
                throw new BuildFailedException("Pockle's prototype shaders require the Built-in Render Pipeline.");
            bool hasScene = false;
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
                if (scene.enabled && scene.path == ScenePath) hasScene = true;
            if (!hasScene)
                throw new BuildFailedException("Run Pockle > Prepare prototype settings to include the tactile scene.");
            foreach (string name in new[] { "Pockle/Jelly Candy", "Pockle/Soft Accent", "Pockle/Reveal Box" })
                if (Shader.Find(name) == null)
                    throw new BuildFailedException("Required shader is missing: " + name);
        }
    }
}
