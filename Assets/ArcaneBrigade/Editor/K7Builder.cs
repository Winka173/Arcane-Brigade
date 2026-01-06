using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K7Builder
    {
        [MenuItem("Arcane Brigade/K7/Prepare audio and check")]
        public static void Check() { Prepare(); K7Checks.Run(); }
        [MenuItem("Arcane Brigade/K7/Regression and Windows candidate")]
        public static void GateAndBuild() { Prepare(); K7Checks.Run(); K7BalanceDiagnostics.Run(); K6Builder.GateAndBuild(); }
        public static void RecheckCandidate() { K7Checks.Run(); K7QualityChecks.Run(); K6Checks.Run(); K5Checks.Run(); FullRunBuilder.BuildWindows(); }
        public static void BuildDiagnostics()
        {
            string directory = "Builds/K7Diagnostics"; Directory.CreateDirectory(directory);
            BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/ArcaneBrigade/Scenes/RunBoot.unity", "Assets/ArcaneBrigade/Scenes/RunFrontend.unity", "Assets/ArcaneBrigade/Scenes/RunArena.unity" }, locationPathName = directory + "/ArcaneBrigade.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("K7 profiler diagnostic build failed");
        }
        public static void Prepare()
        {
            string directory = "Assets/ArcaneBrigade/Resources/Audio";
            Directory.CreateDirectory(directory);
            string[] names = { "NightShift_Base", "NightShift_Ensemble", "NightShift_Boss" };
            for (int stem = 0; stem < names.Length; stem++) WriteWave(directory + "/" + names[stem] + ".wav", ArcadeSound.NightShiftStem(stem));
            WriteWave(directory + "/ShopAfterHours.wav", ArcadeSound.MusicStudy(true));
            Directory.CreateDirectory("PreviewExports/K7/Audition");
            for (int cue = 0; cue < 33; cue++)
            {
                AudioClip clip = ArcadeSound.Cue(cue); var samples = new float[clip.samples]; clip.GetData(samples, 0);
                WriteWave("PreviewExports/K7/Audition/Cue-" + cue.ToString("00") + ".wav", samples); UnityEngine.Object.DestroyImmediate(clip);
            }
            foreach (string name in names) File.Copy(directory + "/" + name + ".wav", "PreviewExports/K7/Audition/" + name + ".wav", true);
            File.Copy(directory + "/ShopAfterHours.wav", "PreviewExports/K7/Audition/ShopAfterHours.wav", true);
            if (File.Exists(directory + "/Licensed/FusionJazz.ogg")) File.Copy(directory + "/Licensed/FusionJazz.ogg", "PreviewExports/K7/Audition/FusionJazz.ogg", true);
            AssetDatabase.Refresh();
            foreach (string path in Directory.GetFiles(directory + "/Licensed", "*.*").Where(path => path.EndsWith(".wav") || path.EndsWith(".ogg")))
            {
                var importer = (AudioImporter)AssetImporter.GetAtPath(path.Replace('\\', '/'));
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = path.EndsWith(".ogg") ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
                settings.compressionFormat = AudioCompressionFormat.Vorbis; settings.quality = 0.9f;
                settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                importer.defaultSampleSettings = settings; importer.SaveAndReimport();
            }
            foreach (string name in new[] { "NightShift_Base", "NightShift_Ensemble", "NightShift_Boss", "ShopAfterHours" })
            {
                var importer = (AudioImporter)AssetImporter.GetAtPath(directory + "/" + name + ".wav");
                AudioImporterSampleSettings settings = importer.defaultSampleSettings;
                settings.loadType = AudioClipLoadType.Streaming; settings.compressionFormat = AudioCompressionFormat.Vorbis;
                settings.quality = 0.9f; settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
                importer.defaultSampleSettings = settings; importer.forceToMono = true; importer.SaveAndReimport();
            }
            PlayerSettings.enableFrameTimingStats = true;
        }
        public static void WriteWave(string path, float[] samples)
        {
            float peak = 0;
            foreach (float sample in samples) peak = Mathf.Max(peak, Mathf.Abs(sample));
            float gain = peak > 0.92f ? 0.92f / peak : 1;
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + samples.Length * 2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
                writer.Write(ArcadeSound.SampleRate); writer.Write(ArcadeSound.SampleRate * 2); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(samples.Length * 2);
                foreach (float sample in samples) writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample * gain, -1, 1) * 32767));
            }
        }
    }
}
