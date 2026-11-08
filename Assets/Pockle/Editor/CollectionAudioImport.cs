using System;
using Pockle.Core;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class CollectionAudioImport
    {
        public static void Configure()
        {
            var paths = AssetDatabase.FindAssets("t:AudioClip", new[]{"Assets/Pockle/Resources/Audio/Collections"});
            int count = 0;
            foreach (string guid in paths)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer=AssetImporter.GetAtPath(path) as AudioImporter;
                if(importer==null)throw new InvalidOperationException("Audio importer missing: "+path);
                importer.forceToMono=true;importer.loadInBackground=false;
                var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.preloadAudioData=true;
                settings.compressionFormat=AudioCompressionFormat.PCM;importer.defaultSampleSettings=settings;
                importer.SaveAndReimport();
                if(AssetDatabase.LoadAssetAtPath<AudioClip>(path)==null)throw new InvalidOperationException("Audio clip missing: "+path);
                count++;
            }
            Debug.Log("COLLECTION_AUDIO_IMPORT_OK " + count + " local PCM mono one-shots");
        }
    }
}
