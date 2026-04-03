using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace ArcaneBrigade.Editor
{
    /// <summary>
    /// Playtest item 2a checks: mixer buses and routing, the per-clip voice limit and retrigger, variation ranges, the loudness
    /// table, voice priority, the music duck curve and the stem arrangement. Structural only: ears decide how it sounds.
    /// Batch (all audio suites in one launch): -executeMethod ArcaneBrigade.Editor.AudioMixChecks.RunAll
    /// </summary>
    public static class AudioMixChecks
    {
        private static readonly List<string> notes = new List<string>();
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

        private static void Require(bool condition, string caption)
        {
            if (!condition) throw new InvalidOperationException("Audio mix: " + caption);
            notes.Add("PASS " + caption);
        }

        /// <summary>The existing audio suites plus this one, in one editor launch.</summary>
        public static void RunAll()
        {
            K8AudioChecks.Run();
            ArcaneAudioChecks.Run();
            Run();
            Debug.Log("AUDIO_SUITES_PASSED K8AudioChecks ArcaneAudioChecks AudioMixChecks");
        }

        [MenuItem("Arcane Brigade/Audio/Audio mix checks")]
        public static void Run()
        {
            notes.Clear();
            ArcaneCueLibrary.Enabled = true;
            Routing();
            VoiceLimit();
            Variation();
            Loudness();
            Priority();
            Ducking();
            Stems();
            Directory.CreateDirectory("Logs");
            File.WriteAllLines("Logs/AudioMixChecks.txt", notes);
            Debug.Log("AUDIO_MIX_CHECKS_PASSED " + notes.Count + " assertions");
        }

        private static void Routing()
        {
            AudioMixer mixer = AudioMix.Mixer;
            Require(mixer != null && AudioMix.Live, "mixer asset loads from Resources/" + AudioMix.MixerResource + " with all buses");
            Require(AudioMixBuilder.MixerComplete(mixer), "Master -> Music, SFX, UI, Voice, Boss with every volume exposed");
            foreach (MixBus bus in Enum.GetValues(typeof(MixBus)))
            {
                AudioMixerGroup group = AudioMix.Group(bus);
                Require(group != null && group.name == AudioMix.GroupNames[(int)bus], "bus " + bus + " resolves to its group");
                var probe = new GameObject("MixProbe").AddComponent<AudioSource>();
                Require(AudioMix.Route(probe, bus).outputAudioMixerGroup == group, "Route sends a source to " + bus);
                UnityEngine.Object.DestroyImmediate(probe.gameObject);
            }

            // every source the core audio owners create is routed (Awake by hand: edit mode does not call it)
            var host = new GameObject("MixRoutingProbe");
            var sfx = host.AddComponent<CheckpointAudio>();
            typeof(CheckpointAudio).GetMethod("Awake", Private).Invoke(sfx, null);
            var music = host.AddComponent<ArcadeMusic>();
            typeof(ArcadeMusic).GetMethod("Awake", Private).Invoke(music, null);
            AudioSource[] sources = host.GetComponents<AudioSource>();
            Require(sources.Length >= 16 + 9 && sources.All(source => source.outputAudioMixerGroup != null), "all " + sources.Length + " CheckpointAudio/ArcadeMusic sources are routed");
            var voices = (AudioSource[])typeof(CheckpointAudio).GetField("voices", Private).GetValue(sfx);
            Require(voices.Take(2).Concat(voices.Skip(4)).All(v => v.outputAudioMixerGroup == AudioMix.Group(MixBus.Sfx)) && voices.Skip(2).Take(2).All(v => v.outputAudioMixerGroup == AudioMix.Group(MixBus.UI)), "SFX lanes start on SFX, interface lanes on UI");
            var ambience = (AudioSource)typeof(ArcadeMusic).GetField("ambience", Private).GetValue(music);
            Require(ambience != null && ambience.outputAudioMixerGroup == AudioMix.Group(MixBus.Sfx), "ambience bed on the SFX bus");
            Require(sources.Count(s => s.outputAudioMixerGroup == AudioMix.Group(MixBus.Music)) >= 8, "music stems on the Music bus");
            // old cues retired from playback where the library / stems cover the moment (the files stay)
            var legacyClips = (AudioClip[])typeof(CheckpointAudio).GetField("clips", Private).GetValue(sfx);
            Require(ArcaneCueLibrary.Available && legacyClips.All(clip => clip == null), "licensed Cue-* clips are not even loaded while the cue library is live");
            var legacyTracks = (AudioSource[])typeof(ArcadeMusic).GetField("tracks", Private).GetValue(music);
            Require(ArcaneCueLibrary.MusicAvailable && legacyTracks.All(track => track == null), "licensed K8 tracks are not loaded while the stems are available");
            UnityEngine.Object.DestroyImmediate(host);

            // static guard: any script that creates an AudioSource routes it (new sounds land in the same buses)
            foreach (string file in Directory.GetFiles("Assets/ArcaneBrigade/Runtime", "*.cs", SearchOption.AllDirectories))
            {
                string text = File.ReadAllText(file);
                if (text.Contains("AddComponent<AudioSource>")) Require(text.Contains("AudioMix.Route("), Path.GetFileName(file) + " routes the AudioSources it creates");
                Require(!text.Contains("NightShift_") && !text.Contains("ShopAfterHours") && !text.Contains("\"Audio/ShopStudy") && !text.Contains("\"Audio/CombatStudy"), Path.GetFileName(file) + " never plays the retired WAV cues");
            }

            // cue buses: boss cues on Boss, UI on UI, voices on Voice, the rest on SFX; every cue reaches a real bus
            Require(SfxGovernor.BusOf("boss.enter", AudioBus.Combat) == MixBus.Boss && SfxGovernor.BusOf("ui.cutin.boss", AudioBus.UI) == MixBus.Boss
                && SfxGovernor.BusOf("ui.select", AudioBus.UI) == MixBus.UI && SfxGovernor.BusOf("bark.x", AudioBus.Voice) == MixBus.Voice
                && SfxGovernor.BusOf("cmb.hit.wood", AudioBus.Combat) == MixBus.Sfx && SfxGovernor.BusOf("loot.coin.drop", AudioBus.Foley) == MixBus.Sfx, "cue -> bus mapping");
            Require(ArcaneCueLibrary.All.All(def => AudioMix.Group(SfxGovernor.BusOf(def.id, def.busId)) != null), "every library cue (" + ArcaneCueLibrary.CueCount + ") lands on a mixer bus");

            // the sliders drive the buses
            Require(Mathf.Approximately(AudioMix.BusDb(MixBus.Music, 1f, 0f), AudioMix.MusicTrimDb) && AudioMix.MusicTrimDb <= -2.5f, "music sits a step (" + AudioMix.MusicTrimDb + " dB) lower at full slider");
            Require(AudioMix.BusDb(MixBus.Sfx, 0f, 0f) <= -80f && AudioMix.BusDb(MixBus.Music, 0f, -3f) <= -80f, "a muted slider silences its bus");
            Require(Mathf.Abs(AudioMix.BusDb(MixBus.Sfx, 0.5f, -5f) - (-6.02f)) < 0.05f && Mathf.Abs(AudioMix.BusDb(MixBus.Music, 0.5f, -5f) - (-6.02f - 3f - 5f)) < 0.05f, "slider in dB; only the music bus takes the duck");
            Require(AudioMix.SourceGain(MixBus.Sfx) == (PresentationSettings.SfxVolume > 0.0001f ? 1f : 0f), "routed sources leave the slider to the bus");
            float musicSlider = PresentationSettings.MusicVolume, effects = PresentationSettings.SfxVolume, voice = PresentationSettings.VoiceVolume;
            var probeProfile = new PlayerProfile { Volume = AudioListener.volume, MusicVolume = 0.5f, SfxVolume = 0.25f, VoiceVolume = 1f, ReducedAudio = PresentationSettings.ReducedAudio, ReducedMotion = PresentationSettings.ReducedMotion, ShowDamageNumbers = PresentationSettings.DamageNumbers, ShowLootIndicators = PresentationSettings.LootIndicators, HighReadability = PresentationSettings.HighReadability, Cosmetic = ArcadeSkin.Bundle };
            PresentationSettings.Apply(probeProfile);
            float sfxDb = 0f, uiDb = 0f;
            bool read = mixer.GetFloat(AudioMix.VolumeParams[(int)MixBus.Sfx], out sfxDb) && mixer.GetFloat(AudioMix.VolumeParams[(int)MixBus.UI], out uiDb);
            if (read && Mathf.Abs(sfxDb - AudioMix.ToDb(0.25f)) < 0.1f) Require(Mathf.Abs(uiDb - AudioMix.ToDb(0.25f)) < 0.1f, "settings Apply sets the SFX and UI buses from the slider");
            else notes.Add("INFO edit-mode mixer did not report the pushed value (" + sfxDb + " dB); bus math is checked above");
            probeProfile.MusicVolume = musicSlider; probeProfile.SfxVolume = effects; probeProfile.VoiceVolume = voice;
            PresentationSettings.Apply(probeProfile);
        }

        private static void VoiceLimit()
        {
            SfxGovernor.ResetVoices();
            const int key = 424242;
            Require(SfxGovernor.Admit(key, 1f, 0f, SfxRole.Swarm) && SfxGovernor.Admit(key, 1f, 0.05f, SfxRole.Swarm) && SfxGovernor.Admit(key, 1f, 0.10f, SfxRole.Swarm) && SfxGovernor.Admit(key, 1f, 0.15f, SfxRole.Swarm), "four copies of one clip may ring together");
            Require(!SfxGovernor.Admit(key, 1f, 0.20f, SfxRole.Swarm) && !SfxGovernor.Admit(key, 1f, 0.25f, SfxRole.Normal), "a fifth copy is refused");
            Require(SfxGovernor.Admit(key, 1f, 0.30f, SfxRole.Hero), "a hero sound takes the oldest copy's place instead of being refused");
            Require(!SfxGovernor.Admit(key, 1f, 0.33f, SfxRole.Boss), "no restart of the same clip within 40 ms, whoever asks");
            Require(SfxGovernor.Admit(key, 1f, 1.40f, SfxRole.Swarm), "copies free their place when they end");
            Require(SfxGovernor.Admit(key + 1, 0.2f, 5f, SfxRole.Swarm) && !SfxGovernor.Admit(key + 1, 0.2f, 5.039f, SfxRole.Swarm) && SfxGovernor.Admit(key + 1, 0.2f, 5.041f, SfxRole.Swarm), "40 ms minimum retrigger boundary");
            Require(SfxGovernor.Admit(key + 2, 0.2f, 5.0f, SfxRole.Swarm), "other clips are not limited by this one");
            Require(SfxGovernor.MaxSameClip == 4 && Mathf.Approximately(SfxGovernor.MinRetrigger, 0.04f), "limits: 4 voices, 40 ms");
            SfxGovernor.ResetVoices();
        }

        private static void Variation()
        {
            UnityEngine.Random.InitState(2026);
            float low = 9f, high = 0f, gainLow = 9f, gainHigh = 0f;
            for (int sample = 0; sample < 400; sample++)
            {
                float pitch = SfxGovernor.VaryPitch(1f);
                low = Mathf.Min(low, pitch); high = Mathf.Max(high, pitch);
                float gain = SfxGovernor.VaryGain();
                gainLow = Mathf.Min(gainLow, gain); gainHigh = Mathf.Max(gainHigh, gain);
            }
            Require(low >= 0.96f - 1e-4f && high <= 1.04f + 1e-4f && high - low > 0.06f, "pitch varies within ±4% (" + low.ToString("0.000") + ".." + high.ToString("0.000") + ")");
            Require(AudioMix.ToDb(gainLow) >= -2.001f && AudioMix.ToDb(gainHigh) <= 2.001f && AudioMix.ToDb(gainHigh) - AudioMix.ToDb(gainLow) > 3f, "volume varies within ±2 dB");
            float weapon = SfxGovernor.VaryPitch(1.08f, 0.02f);
            Require(weapon >= 1.08f * 0.98f - 1e-4f && weapon <= 1.08f * 1.02f + 1e-4f, "a weapon's own pitch keeps a narrower spread");
        }

        private static void Loudness()
        {
            var text = Resources.Load<TextAsset>(SfxGovernor.LoudnessResource);
            Require(text != null, "loudness table exists (run AudioMixBuilder.Apply after new files)");
            var table = JsonUtility.FromJson<SfxGovernor.LoudnessTable>(text.text);
            Require(table.clips.Length >= 300 && SfxGovernor.LoudnessEntries == table.clips.Length, "loudness table covers the library (" + table.clips.Length + " clips)");
            Require(table.clips.All(row => Mathf.Abs(row.gainDb) <= AudioMixBuilder.MaxCorrectionDb + 0.01f), "corrections stay within ±" + AudioMixBuilder.MaxCorrectionDb + " dB");
            Require(table.clips.Where(row => Mathf.Abs(row.gainDb) < AudioMixBuilder.MaxCorrectionDb - 0.01f).All(row => Mathf.Abs(row.rmsDb + row.gainDb - table.targetDb) < 0.05f), "every correctable clip lands on one target (" + table.targetDb + " dBFS)");
            Require(table.clips.Count(row => Mathf.Abs(row.gainDb) > 1f) > 10, "the table actually levels clips");
            ArcaneCueDef hit = ArcaneCueLibrary.Get("cmb.hit.wood");
            AudioClip clip = ArcaneCueLibrary.Pick(hit);
            SfxGovernor.LoudnessRow row0 = table.clips.FirstOrDefault(row => row.clip == clip.name);
            Require(row0 != null && Mathf.Abs(SfxGovernor.Loudness(clip) - AudioMix.FromDb(row0.gainDb)) < 1e-4f, "runtime gain for " + clip.name + " comes from the table");
            Require(AudioMixBuilder.MeasureDb(AssetDatabase.GetAssetPath(clip)) is float db && Mathf.Abs(db - row0.rmsDb) < 0.05f, "measurement is reproducible");
        }

        private static void Priority()
        {
            Require(SfxGovernor.RoleOf("cmb.hit.wood", AudioBus.Combat, false) == SfxRole.Swarm && SfxGovernor.RoleOf("enemy.die.paper", AudioBus.Combat, false) == SfxRole.Swarm, "enemy hits and deaths are the swarm");
            Require(SfxGovernor.RoleOf("cmb.swing.blade", AudioBus.Combat, true) == SfxRole.Hero && SfxGovernor.RoleOf("plr.hurt", AudioBus.Combat, false) == SfxRole.Hero && SfxGovernor.RoleOf("hero.lantern.stride", AudioBus.Combat, false) == SfxRole.Hero, "hero sounds");
            Require(SfxGovernor.RoleOf("boss.enter", AudioBus.Combat, false) == SfxRole.Boss && SfxGovernor.RoleOf("ui.select", AudioBus.UI, false) == SfxRole.UI, "boss and UI sounds");
            int worstProtected = new[] { SfxRole.Hero, SfxRole.Boss, SfxRole.UI }.Max(role => SfxGovernor.Priority(role, 255));
            int bestSwarm = Enumerable.Range(0, 256).Min(cue => SfxGovernor.Priority(SfxRole.Swarm, cue));
            Require(worstProtected < bestSwarm && bestSwarm > 120 && worstProtected <= 120, "hero/boss/UI priority " + worstProtected + " beats any swarm priority " + bestSwarm + "; the swarm never steals a lane");
            Require(Enumerable.Range(0, 256).All(cue => SfxGovernor.Priority(SfxRole.Normal, cue) > worstProtected), "ordinary cues cannot steal a hero/boss/UI lane either");
        }

        private static void Ducking()
        {
            Require(AudioMix.DuckDepthDb <= -5f && AudioMix.DuckDepthDb >= -6f, "duck depth 5-6 dB");
            Require(AudioMix.DuckTarget(0f, false) == 0f && AudioMix.DuckTarget(AudioMix.DuckThreshold, false) == 0f && Mathf.Approximately(AudioMix.DuckTarget(AudioMix.DuckFull, false), AudioMix.DuckDepthDb), "duck follows SFX density: none when calm, full when dense");
            float mid = AudioMix.DuckTarget((AudioMix.DuckThreshold + AudioMix.DuckFull) * 0.5f, false);
            Require(mid < 0f && mid > AudioMix.DuckDepthDb, "partial duck in between");
            Require(Mathf.Approximately(AudioMix.DuckTarget(0f, true), AudioMix.DuckDepthDb), "a boss cue ducks the music fully");
            float duck = 0f, t = 0f;
            while (duck > AudioMix.DuckDepthDb + 0.001f && t < 2f) { duck = AudioMix.StepDuck(duck, AudioMix.DuckDepthDb, 1f / 60f); t += 1f / 60f; }
            Require(t <= AudioMix.DuckAttack + 1f / 60f + 1e-4f, "duck goes down within " + AudioMix.DuckAttack + " s");
            t = 0f;
            while (duck < -0.001f && t < 3f) { duck = AudioMix.StepDuck(duck, 0f, 1f / 60f); t += 1f / 60f; }
            Require(t >= 0.55f && t <= 0.65f + 1e-4f, "music recovers over ~0.6 s (" + t.ToString("0.00") + " s)");
            AudioMix.ResetDuck();
            for (int index = 0; index < 3; index++) AudioMix.ReportSfx(1f, MixBus.Sfx);
            AudioMix.ReportSfx(5f, MixBus.UI);
            Require(Mathf.Approximately(AudioMix.Energy, 3f) && Mathf.Approximately(AudioMix.DuckTarget(AudioMix.Energy, false), AudioMix.DuckDepthDb), "combat SFX build the sidechain, UI clicks do not");
            AudioMix.ResetDuck();
        }

        private static void Stems()
        {
            var levels = new float[4];
            foreach (string scene in new[] { ArcaneCueLibrary.SceneCombatEarly, ArcaneCueLibrary.SceneCombatMid, ArcaneCueLibrary.SceneCombatLate, ArcaneCueLibrary.SceneEndless, ArcaneCueLibrary.SceneBoss })
                foreach (float density in new[] { 0f, 0.5f, 1f })
                {
                    MusicArrangement.Targets(scene, density, false, levels);
                    Require(levels[0] == 1f && levels[1] > 0.5f && levels[2] == 0f && levels[3] == 0f, scene + " at density " + density + " plays base + rhythm only");
                }
            MusicArrangement.Targets(ArcaneCueLibrary.SceneBoss, 1f, true, levels);
            Require(levels[2] == 1f && levels[3] == 0f, "a boss's melody plays in its intro");
            foreach (string scene in new[] { ArcaneCueLibrary.SceneMenu, ArcaneCueLibrary.SceneSelect, ArcaneCueLibrary.SceneShopGold, ArcaneCueLibrary.SceneShopDream })
            {
                MusicArrangement.Targets(scene, 0f, false, levels);
                Require(levels[0] == 1f && levels[2] == 1f, scene + " keeps its melody");
            }
            var viaMusic = new float[4];
            ArcadeMusic.StemTargets(ArcaneCueLibrary.SceneCombatMid, 0.7f, viaMusic);
            MusicArrangement.Targets(ArcaneCueLibrary.SceneCombatMid, 0.7f, false, levels);
            Require(viaMusic.SequenceEqual(levels), "ArcadeMusic uses the one arrangement table");
            Require(MusicArrangement.BossIntroSeconds >= 4f && MusicArrangement.BossIntroSeconds <= 12f, "boss intro lasts a few bars");
        }
    }
}
