using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    /// <summary>Authoring previews; no expression buttons in the game HUD.</summary>
    public sealed class ExpressionPreviewWindow : EditorWindow
    {
        [MenuItem("Pockle/Expressions")]
        private static void Open() { GetWindow<ExpressionPreviewWindow>("Pip expressions"); }
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Press Play, open an owned toy from Collection, then preview a face. Automatic: lift/stretch → curious; squish/shake/drop → delighted; 12 seconds of rest → sleepy. J simulates a shake. Previews clear when selecting another toy or resetting.", MessageType.Info);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Automatic reactions")) Preview(null);
                if (GUILayout.Button("Happy")) Preview(FaceExpression.Happy);
                if (GUILayout.Button("Curious")) Preview(FaceExpression.Curious);
                if (GUILayout.Button("Sleepy")) Preview(FaceExpression.Sleepy);
                if (GUILayout.Button("Delighted")) Preview(FaceExpression.Delighted);
            }
        }
        private static void Preview(FaceExpression? expression)
        {
            TactilePrototype viewer = Object.FindFirstObjectByType<TactilePrototype>();
            if (viewer == null) Debug.LogWarning("Start the Pockle scene and open an owned toy to preview expressions.");
            else viewer.PreviewExpression(expression);
        }
    }
}
