using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8AudioBuilder
    {
        [MenuItem("Arcane Brigade/K8/Apply licensed audio")]
        public static void Apply()
        {
            for (int index = 0; index < 4; index++)
            {
                AudioClip clip = Resources.Load<AudioClip>(ArcadeMusic.TrackResource(index));
                if (clip == null) throw new InvalidOperationException("Missing K8 licensed track " + index);
                var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                if (settings.loadType == AudioClipLoadType.Streaming && settings.compressionFormat == AudioCompressionFormat.Vorbis && Mathf.Approximately(settings.quality, 0.9f) && !importer.forceToMono && importer.loadInBackground) continue;
                settings.loadType = AudioClipLoadType.Streaming;
                settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.9f;
                importer.defaultSampleSettings = settings;
                importer.forceToMono = false;
                importer.loadInBackground = true;
                importer.SaveAndReimport();
            }
            K8AudioChecks.Run();
            Debug.Log("K8_AUDIO_APPLIED: four CC0 tracks; existing FullRunBuilder copies K8-Audio-Credits.txt into ThirdPartyNotices.");
        }

        public static void CopyCredits(string buildDirectory)
        {
            string notices = Path.Combine(buildDirectory, "ThirdPartyNotices");
            Directory.CreateDirectory(notices);
            File.Copy("Assets/ArcaneBrigade/Resources/Audio/Licensed/K8-Audio-Credits.txt", Path.Combine(notices, "K8-Audio-Credits.txt"), true);
        }
    }
}
