using Pockle.Runtime;
using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    /// <summary>Separate authoring controls; does not add prototype controls to the game UI.</summary>
    public sealed class CharacterStudyWindow : EditorWindow
    {
        [MenuItem("Pockle/Character studies")]
        private static void Open() { GetWindow<CharacterStudyWindow>("Character studies"); }
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Press Play in the Pockle scene, then choose a current model. Drag to turn, or tap for its signature reactions. Review previews do not grant toys or write inventory.", MessageType.Info);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Preview Fox · Gloss Vinyl")) Preview(Pockle.Core.ToyCatalog.FoxId);
                if (GUILayout.Button("Preview Axolotl · Plush")) Preview(Pockle.Core.ToyCatalog.AxolotlId);
            }
        }
        private static void Preview(string id)
        {
            TactilePrototype viewer = Object.FindFirstObjectByType<TactilePrototype>();
            if (viewer == null || !viewer.PreviewCharacterStudy(id))
                Debug.LogWarning("Start the Pockle scene with an owned Pip, finish any box reveal, and check character import errors before previewing.");
        }
    }
}
