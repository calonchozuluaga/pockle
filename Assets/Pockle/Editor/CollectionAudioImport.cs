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
            foreach(string character in new[]{"pip","fox","wally"})
            foreach(CollectionVoiceMood mood in Enum.GetValues(typeof(CollectionVoiceMood)))
            {
                string path="Assets/Pockle/Resources/"+CollectionVoicePolicy.Resource(character,mood)+".mp3";
                var importer=AssetImporter.GetAtPath(path) as AudioImporter;
                if(importer==null)throw new InvalidOperationException("Audio importer missing: "+path);
                importer.forceToMono=true;importer.loadInBackground=false;
                var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.preloadAudioData=true;
                settings.compressionFormat=AudioCompressionFormat.PCM;importer.defaultSampleSettings=settings;
                importer.SaveAndReimport();
                if(AssetDatabase.LoadAssetAtPath<AudioClip>(path)==null)throw new InvalidOperationException("Audio clip missing: "+path);
            }
            Debug.Log("COLLECTION_AUDIO_IMPORT_OK 9 local PCM mono one-shots");
        }
    }
}
