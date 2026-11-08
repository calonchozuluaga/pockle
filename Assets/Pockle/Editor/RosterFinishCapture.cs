using System;
using System.IO;
using System.Collections.Generic;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Pockle.Editor
{
    public static class RosterFinishCapture
    {
        [Serializable] public class Entry
        {
            public string id, collection, collectionName, name, finish, tier;
        }
        [Serializable] public class Manifest { public List<Entry> entries = new List<Entry>(); }
        static Manifest manifest;
        static string folder;
        static int next;
        static double last;

        public static void Capture()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            folder = Environment.GetEnvironmentVariable("POCKLE_ROSTER_OUTPUT") ?? Path.GetFullPath("docs/roster-finish-review/01");
            Directory.CreateDirectory(Path.Combine(folder, "renders"));
            manifest = new Manifest();
            foreach (var collection in ToyCatalog.Collections)
            {
                if (collection.Id == PipFusionStudies.CollectionId) continue;
                string filter = Environment.GetEnvironmentVariable("POCKLE_ROSTER_COLLECTION");
                if (!string.IsNullOrEmpty(filter) && collection.Id != filter) continue;
                foreach (string id in collection.Members)
                {
                    if (!ToyCatalog.TryGetCollectible(id, out var toy)) throw new InvalidOperationException(id);
                    manifest.entries.Add(new Entry { id = id, collection = collection.Id,
                        collectionName = collection.DisplayName, name = toy.DisplayName,
                        finish = toy.FinishDisplayName, tier = toy.Tier.ToString() });
                }
            }
            next = 0;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            if (EditorApplication.timeSinceStartup - last < .1) return;
            last = EditorApplication.timeSinceStartup;
            try
            {
                if (next == manifest.entries.Count)
                {
                    File.WriteAllText(Path.Combine(folder, "catalog.json"), JsonUtility.ToJson(manifest, true));
                    Debug.Log("ROSTER_COMPLETE " + next);
                    EditorApplication.update -= Tick;
                    EditorApplication.Exit(0);
                    return;
                }
                var entry = manifest.entries[next++];
                var type = typeof(JellyToy).Assembly.GetType("Pockle.Runtime.PortraitCutout");
                var args = new object[] { entry.id, 0f };
                var texture = (Texture2D)type.GetMethod("Render").Invoke(null, args);
                File.WriteAllBytes(Path.Combine(folder, "renders", entry.id + ".png"), texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                Debug.Log("ROSTER_CAPTURE " + entry.id);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorApplication.update -= Tick;
                EditorApplication.Exit(1);
            }
        }
    }
}
