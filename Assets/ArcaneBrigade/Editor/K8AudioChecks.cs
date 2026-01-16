using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    [InitializeOnLoad]
    public static class K8AudioChecks
    {
        private const string Armed = "K8AudioChecks.Playback";
        private static readonly List<string> notes = new List<string>();
        private static IEnumerator playback;
        private static double readyAt;
        private static int exitCode;
        private static GameObject first;
        private static GameObject second;
        private static PlayerProfile original;
        private static float originalTimeScale;
        private static bool originalPause;

        static K8AudioChecks() { EditorApplication.playModeStateChanged += PlayStateChanged; }

        private static void Require(bool condition, string caption)
        {
            if (!condition) throw new InvalidOperationException("K8 audio: " + caption);
            notes.Add("PASS " + caption);
        }

        private static string Hash(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var algorithm = SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "");
        }

        [MenuItem("Arcane Brigade/K8/Audio source checks")]
        public static void Run()
        {
            notes.Clear();
            string[] hashes =
            {
                "645F7CD6EF7A56B22FE394FF006DAE17348CF70C72AA9E2A9B4D7F70B2804AF4",
                "BB2668C05173600C90B68F6192F7B7316582AF18D9A28ACC3F7FC98A3C8339CE",
                "EA78B533F259EFD58E8BC07B2C987E6AD4EAA5DC7692ED2F843DDF47449E59E3",
                "9A63F74305F5CBCE8374A72A027A645C07C97663255ABA0B817BC1F70C432ED5"
            };
            string credits = File.ReadAllText("Assets/ArcaneBrigade/Resources/Audio/Licensed/K8-Audio-Credits.txt");
            Require(hashes.Distinct().Count() == 4, "Four different soundtrack content hashes");
            for (int index = 0; index < 4; index++)
            {
                AudioClip clip = Resources.Load<AudioClip>(ArcadeMusic.TrackResource(index));
                Require(clip != null && clip.channels == 2 && clip.length > 60, "Track " + index + " imports stereo and lasts over a minute");
                string path = AssetDatabase.GetAssetPath(clip);
                Require(Hash(path) == hashes[index] && credits.Contains(hashes[index]), "Track " + index + " exact original bytes and shipping hash credit");
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                Require(importer.defaultSampleSettings.loadType == AudioClipLoadType.Streaming && !importer.forceToMono, "Track " + index + " streaming stereo import");
            }
            Require(ArcadeMusic.PhaseTrack(SessionPhase.Shop, true) == 1 && ArcadeMusic.PhaseTrack(SessionPhase.Combat, false) == 2 && ArcadeMusic.PhaseTrack(SessionPhase.Combat, true) == 3 && ArcadeMusic.PhaseTrack(SessionPhase.Victory, false) == 0 && ArcadeMusic.PhaseTrack(SessionPhase.Defeat, false) == 0, "Phase mapping including terminal phases");
            Require(ArcadeMusic.BusGain(true, true) < ArcadeMusic.BusGain(true, false) && ArcadeMusic.BusGain(true, false) < 1, "Music reduced and pause gains compose");
            Require(CheckpointAudio.CosmeticInterval(true) > CheckpointAudio.CosmeticInterval(false), "Reduced global density gate");
            Require(CheckpointAudio.CueGain(24, true, true) == CheckpointAudio.CueGain(24, true, false) && CheckpointAudio.CueGain(0, false, true) < CheckpointAudio.CueGain(0, false, false), "Reduced cosmetics never attenuate danger");
            Require(ReservedLanesAreIsolated(), "All 256 occupancy masks preserve reserved lanes, both density modes and cursors");
            var hitSources = new HashSet<AudioClip>();
            foreach (BodyMaterial material in Enum.GetValues(typeof(BodyMaterial)))
            {
                int hit = ArcadeSound.MaterialCue(material, false);
                int death = ArcadeSound.MaterialCue(material, true);
                AudioClip clip = ArcadeSound.AcquireCue(hit, out bool owned);
                Require(clip != null && !owned && EditorUtility.IsPersistent(clip), material + " borrows licensed hit source");
                hitSources.Add(clip);
                var definition = ScriptableObject.CreateInstance<UnitDefinition>();
                try
                {
                    definition.Material = material;
                    var subject = new UnitState { Definition = definition };
                    Require(CheckpointAudio.EventCue(new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, subject: subject)) == hit && CheckpointAudio.EventCue(new CombatEvent(CombatEventKind.Kill, Vector2.zero, Faction.Enemy, subject: subject)) == death, material + " hit/death event palette");
                    definition.IsBoss = true;
                    Require(CheckpointAudio.EventCue(new CombatEvent(CombatEventKind.Kill, Vector2.zero, Faction.Enemy, subject: subject)) == 6, material + " boss death preserves danger cue");
                }
                finally { UnityEngine.Object.DestroyImmediate(definition); }
                CheckFallback(hit);
                CheckFallback(death);
            }
            Require(hitSources.Count == 6, "Six distinct sampled material hit sources");
            for (int cue = 0; cue < 39; cue++)
            {
                AudioClip clip = ArcadeSound.AcquireCue(cue, out bool owned);
                Require(clip != null && !owned && EditorUtility.IsPersistent(clip), "Cue " + cue + " borrowed, not cloned or generated");
            }
            AudioClip legacy = ArcadeSound.Cue(0);
            Require(!EditorUtility.IsPersistent(legacy), "Legacy builder API retains caller-owned copy");
            UnityEngine.Object.DestroyImmediate(legacy);
            Require(CheckpointAudio.EventCue(new CombatEvent(CombatEventKind.ChestOpened, Vector2.zero, Faction.Champion)) == 20, "Chest uses confirmation instead of projectile glass");
            WriteReport("Source");
            Debug.Log("K8_AUDIO_SOURCE_CHECKS_PASSED " + notes.Count);
        }

        private static void CheckFallback(int cue)
        {
            AudioClip clip = NightCabaretSound.Foley(cue);
            try
            {
                var samples = new float[clip.samples * clip.channels];
                Require(clip.GetData(samples, 0), "Fallback " + cue + " readable samples");
                Require(samples.All(sample => !float.IsNaN(sample) && !float.IsInfinity(sample) && Mathf.Abs(sample) <= 1) && samples.Any(sample => Mathf.Abs(sample) > 0.01f), "Fallback " + cue + " finite non-silent and bounded");
                Require(Mathf.Abs(samples[0]) < 0.001f && Mathf.Abs(samples[samples.Length - 1]) < 0.001f, "Fallback " + cue + " tapered boundaries");
            }
            finally { UnityEngine.Object.DestroyImmediate(clip); }
        }

        public static bool ReservedLanesAreIsolated()
        {
            foreach (bool reduced in new[] { false, true })
                for (int occupied = 0; occupied < 256; occupied++)
                    for (int cursor = 0; cursor < 2; cursor++)
                    {
                        if (CheckpointAudio.AllocateVoice(true, false, reduced, cursor, occupied) != cursor) return false;
                        if (CheckpointAudio.AllocateVoice(true, true, reduced, cursor, occupied) != cursor) return false;
                        if (CheckpointAudio.AllocateVoice(false, true, reduced, cursor, occupied) != 2 + cursor) return false;
                        int expected = -1;
                        for (int index = 4; index < (reduced ? 6 : 8); index++)
                            if ((occupied & (1 << index)) == 0) { expected = index; break; }
                        if (CheckpointAudio.AllocateVoice(false, false, reduced, cursor, occupied) != expected) return false;
                    }
            return CheckpointAudio.VoicePriority(true, false, false) == 16
                && CheckpointAudio.VoicePriority(true, true, true) == 16
                && CheckpointAudio.VoicePriority(false, true, false) == 64
                && CheckpointAudio.VoicePriority(false, false, true) == 120
                && CheckpointAudio.VoicePriority(false, false, false) == 180;
        }

        private static void WriteReport(string kind)
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllLines("Logs/K8Audio-" + kind + "-Checks.txt", notes);
        }

        [MenuItem("Arcane Brigade/K8/Audio playback checks")]
        public static void RunPlayback()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run from edit mode.");
            Run();
            SessionState.SetBool(Armed, true);
            EditorApplication.EnterPlaymode();
        }

        private static void PlayStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(Armed, false)) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                notes.Clear();
                playback = Playback();
                readyAt = EditorApplication.timeSinceStartup + 0.5;
                EditorApplication.update += Advance;
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                SessionState.SetBool(Armed, false);
                if (Application.isBatchMode) EditorApplication.Exit(SessionState.GetInt(Armed + ".Exit", 1));
            }
        }

        private static void Advance()
        {
            if (EditorApplication.timeSinceStartup < readyAt) return;
            try
            {
                if (playback.MoveNext())
                {
                    readyAt = EditorApplication.timeSinceStartup + (playback.Current is float delay ? delay : 0);
                    return;
                }
                exitCode = 0;
                Debug.Log("K8_AUDIO_PLAYBACK_CHECKS_PASSED " + notes.Count);
            }
            catch (Exception exception) { exitCode = 1; notes.Add("FAIL " + exception); Debug.LogException(exception); }
            EditorApplication.update -= Advance;
            if (first != null) UnityEngine.Object.Destroy(first);
            if (second != null) UnityEngine.Object.Destroy(second);
            ArcaneCueLibrary.Enabled = true;
            if (original != null) PresentationSettings.Apply(original);
            Time.timeScale = originalTimeScale;
            AudioListener.pause = originalPause;
            WriteReport("Playback");
            SessionState.SetInt(Armed + ".Exit", exitCode);
            EditorApplication.ExitPlaymode();
        }

        private static AudioSource[] Sources(object component, string field) => (AudioSource[])component.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(component);

        private static IEnumerator Playback()
        {
            originalTimeScale = Time.timeScale;
            originalPause = AudioListener.pause;
            original = new PlayerProfile { Volume = AudioListener.volume, MusicVolume = PresentationSettings.MusicVolume, SfxVolume = PresentationSettings.SfxVolume, ReducedAudio = PresentationSettings.ReducedAudio, ReducedMotion = PresentationSettings.ReducedMotion, ShowDamageNumbers = PresentationSettings.DamageNumbers, ShowLootIndicators = PresentationSettings.LootIndicators, HighReadability = PresentationSettings.HighReadability, Cosmetic = ArcadeSkin.Bundle };
            var settings = new PlayerProfile { Volume = 0, MusicVolume = 1, SfxVolume = 1 };
            PresentationSettings.Apply(settings);
            Time.timeScale = 1;
            AudioListener.pause = false;
            ArcaneCueLibrary.Enabled = false; // this harness asserts the legacy K8 stream/voice behaviour
            first = new GameObject("K8Audio-First");
            CheckpointAudio firstAudio = first.AddComponent<CheckpointAudio>();
            ArcadeMusic firstMusic = first.AddComponent<ArcadeMusic>();
            if (UnityEngine.Object.FindAnyObjectByType<AudioListener>() == null) first.AddComponent<AudioListener>();
            AudioSource[] tracks = Sources(firstMusic, "tracks");
            yield return 1f;
            Require(tracks[0].isPlaying && tracks.Skip(1).All(track => !track.isPlaying), "Default menu starts only menu stream");
            firstMusic.SetPhase(SessionPhase.Shop, false, false);
            yield return 1.5f;
            Require(tracks[1].isPlaying && !tracks[0].isPlaying, "Menu to shop crossfade stops old stream");
            firstMusic.SetPhase(SessionPhase.Combat, false, false);
            yield return 1.5f;
            Require(tracks[2].isPlaying && !tracks[1].isPlaying, "Shop to combat uses different stream");
            firstMusic.SetPhase(SessionPhase.Combat, false, true);
            yield return 1.5f;
            Require(tracks[3].isPlaying && tracks.All(track => track.pitch == 1), "Boss changes composition, never pitch");
            firstMusic.SetPhase(SessionPhase.Combat, true, true);
            yield return 0.8f;
            Require(tracks[3].volume <= ArcadeMusic.TrackLevel(3) * 0.28f + 0.001f, "Manual pause music gain");
            firstMusic.SendMessage("OnApplicationFocus", false);
            float position = tracks[3].time;
            yield return 0.3f;
            Require(Mathf.Abs(tracks[3].time - position) < 0.05f, "Focus pause preserves playhead");
            firstMusic.SendMessage("OnApplicationFocus", true);
            yield return 0.3f;
            Require(tracks[3].time > position, "Focus resumes playhead");
            second = new GameObject("K8Audio-Second");
            CheckpointAudio secondAudio = second.AddComponent<CheckpointAudio>();
            second.AddComponent<ArcadeMusic>();
            yield return 0.3f;
            Require(tracks.All(track => !track.isPlaying), "Latest music owner stops prior owner");
            int before = firstAudio.PlayedCues;
            firstAudio.PlayCue(0);
            Require(firstAudio.PlayedCues == before, "Inactive audio owner rejects cues");
            CheckpointAudio.PlayInterface(20);
            Require(secondAudio.PlayedCues == 1, "Interface routes to latest owner");
            UnityEngine.Object.Destroy(second);
            yield return 0.3f;
            before = firstAudio.PlayedCues;
            CheckpointAudio.PlayInterface(21);
            Require(firstAudio.PlayedCues == before + 1, "Destroy restores prior interface owner");
            Require(Resources.Load<AudioClip>("Audio/Licensed/Cue-00") != null, "Destroying owner preserves borrowed Resources clip");
            firstAudio.ConfigurePresentation();
            firstAudio.ConfigurePresentation();
            AudioSource[] voices = Sources(firstAudio, "voices");
            Require(voices.Length == 8 && first.GetComponents<AudioSource>().Length == 12, "Repeated configuration creates no duplicate sources");
            before = firstAudio.PlayedCues;
            for (int cue = 0; cue < 19; cue++) firstAudio.PlayCue(cue);
            Require(firstAudio.PlayedCues <= before + 2 && firstAudio.DroppedCosmeticCues >= 16, "Burst density globally bounded across different cues");
            Require(voices[0].isPlaying && voices[0].priority == 16, "Danger retains reserved voice under cosmetic burst");
            yield return 0.5f;
            before = firstAudio.PlayedCues;
            firstAudio.PlayCue(24, true);
            Require(firstAudio.PlayedCues == before + 1 && PresentationSettings.MusicDuck < 1, "Warning ducks music after cosmetic saturation");
            yield return 0.02f;
            Require(voices.Skip(4).All(voice => voice.volume <= 0.14f * 0.55f + 0.001f), "Danger ducks cosmetic bus, not interface bus");
            AudioListener.pause = true;
            before = firstAudio.PlayedCues;
            firstAudio.PlayCue(25, true);
            CheckpointAudio.PlayInterface(22);
            Require(firstAudio.PlayedCues == before, "Listener pause suppresses danger and interface enqueue");
            AudioListener.pause = false;
            Time.timeScale = 0;
            before = firstAudio.PlayedCues;
            firstAudio.PlayCue(2);
            CheckpointAudio.PlayInterface(22);
            Require(firstAudio.PlayedCues == before + 1, "Manual pause blocks combat while interface remains usable");
            yield return 0.05f;
            Require(voices.Take(2).Concat(voices.Skip(4)).All(voice => !voice.isPlaying), "Manual pause stops existing combat and danger tails");
            Time.timeScale = 1;
            firstAudio.SendMessage("OnApplicationFocus", false);
            before = firstAudio.PlayedCues;
            firstAudio.PlayCue(25, true);
            Require(firstAudio.PlayedCues == before && voices.All(voice => !voice.isPlaying), "Focus loss clears and blocks SFX");
            firstAudio.SendMessage("OnApplicationFocus", true);
            firstAudio.SendMessage("OnApplicationPause", true);
            before = firstAudio.PlayedCues;
            CheckpointAudio.PlayInterface(20);
            Require(firstAudio.PlayedCues == before, "Application background blocks interface cues");
            firstAudio.SendMessage("OnApplicationPause", false);
            settings.ReducedAudio = true;
            PresentationSettings.Apply(settings);
            yield return 0.5f;
            before = firstAudio.PlayedCues;
            for (int cue = 27; cue < 39; cue++) firstAudio.PlayCue(cue);
            Require(firstAudio.PlayedCues == before + 1 && voices.Skip(6).All(voice => !voice.isPlaying), "Reduced palette burst and two cosmetic voices");
            firstAudio.PlayCue(25, true);
            Require(voices.Take(2).Any(voice => voice.isPlaying && Mathf.Abs(voice.volume - 0.25f) < 0.001f), "Reduced audio preserves danger gain and admission");
            first.SetActive(false);
            yield return 0.1f;
            Require(voices.All(voice => !voice.isPlaying) && tracks.All(track => !track.isPlaying), "Disable releases playback ownership and all voices");
            first.SetActive(true);
            yield return 0.3f;
            Require(tracks[3].isPlaying, "Re-enable restores selected phase");
            settings.MusicVolume = 0;
            settings.SfxVolume = 0;
            PresentationSettings.Apply(settings);
            yield return 0.8f;
            Require(tracks.All(track => track.volume == 0) && voices.All(voice => voice.volume == 0), "Music and SFX bus mute applies to existing sources");
        }
    }
}
