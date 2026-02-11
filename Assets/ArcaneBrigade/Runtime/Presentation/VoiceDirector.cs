using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Short voiced barks for the shopkeeper, the story bosses and the travelling party (Docs/Audio/Voice_System.md).
    /// Selection works without any audio: every bank is a shuffle bag (no immediate repeats), and a missing clip simply
    /// means the line is shown as a subtitle only. Clips load lazily from Resources/Audio/Voice/&lt;Speaker&gt;/&lt;trigger&gt;_NN.</summary>
    public sealed class VoiceDirector : MonoBehaviour
    {
        public static VoiceDirector Active { get; private set; }

        // Barks below this priority are dropped when "reduced audio clutter" is on; the subtitle still appears.
        public const int Chatter = 0, Normal = 1, Important = 2, Story = 3;

        private static readonly Dictionary<string, List<VoiceLine>> bags = new Dictionary<string, List<VoiceLine>>();
        private static readonly Dictionary<string, VoiceLine> lastPicked = new Dictionary<string, VoiceLine>();
        private static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private static readonly HashSet<string> missing = new HashSet<string>();
        private static readonly Dictionary<string, float> cooldownUntil = new Dictionary<string, float>();
        private static System.Random random = new System.Random(20261017);

        private sealed class Channel { public AudioSource Source; public string Speaker; public int Priority; public float Until; public VoiceLine Line; }
        private readonly Dictionary<string, Channel> channels = new Dictionary<string, Channel>();
        private bool focused = true;
        private bool paused;
        public int PlayedClips { get; private set; }
        public int SilentLines { get; private set; }
        public VoiceLine LastLine { get; private set; }

        private void Awake() { Active = this; }
        private void OnDestroy() { if (Active == this) Active = null; }

        /// <summary>Per-trigger rest between two lines of the same speaker and trigger (seconds, unscaled).</summary>
        public static float Cooldown(string speaker, string trigger) => trigger switch
        {
            "attack" => 7.5f,
            "hurt" => 9f,
            "idle" => 22f,
            "chair" => 14f,
            "purchase" => 1.2f,
            "reroll" => 1.6f,
            "cant_afford" => 1.4f,
            _ => 0f
        };

        /// <summary>Draws the next line from a speaker's bank without repeating the previous one. Null if the bank is empty.</summary>
        public static VoiceLine Pick(string speaker, string trigger)
        {
            VoiceLine[] bank = VoiceLines.Bank(speaker, trigger);
            if (bank.Length == 0) return null;
            string key = speaker + "/" + trigger;
            if (!bags.TryGetValue(key, out List<VoiceLine> bag)) bags[key] = bag = new List<VoiceLine>();
            if (bag.Count == 0)
            {
                bag.AddRange(bank);
                for (int index = bag.Count - 1; index > 0; index--) { int other = random.Next(index + 1); (bag[index], bag[other]) = (bag[other], bag[index]); }
                // a fresh shuffle never starts with the line that just played
                if (bag.Count > 1 && lastPicked.TryGetValue(key, out VoiceLine previous) && bag[bag.Count - 1] == previous) (bag[0], bag[bag.Count - 1]) = (bag[bag.Count - 1], bag[0]);
            }
            VoiceLine line = bag[bag.Count - 1]; bag.RemoveAt(bag.Count - 1);
            lastPicked[key] = line;
            return line;
        }

        /// <summary>A specific numbered line (story beats). Null when out of range.</summary>
        public static VoiceLine Exact(string speaker, string trigger, int index)
        {
            VoiceLine[] bank = VoiceLines.Bank(speaker, trigger);
            return index >= 0 && index < bank.Length ? bank[index] : null;
        }

        public static bool Ready(string speaker, string trigger) => !cooldownUntil.TryGetValue(speaker + "/" + trigger, out float until) || Time.unscaledTime >= until;

        /// <summary>Pick (respecting the trigger's cooldown) and voice a line. Returns the line for the subtitle, or null when
        /// the trigger is resting. Audio is optional: without a clip the line is still returned.</summary>
        public static VoiceLine Say(string speaker, string trigger, int priority = Normal, bool ignoreCooldown = false)
        {
            if (!ignoreCooldown && !Ready(speaker, trigger)) return null;
            VoiceLine line = Pick(speaker, trigger);
            if (line == null) return null;
            cooldownUntil[speaker + "/" + trigger] = Time.unscaledTime + Cooldown(speaker, trigger);
            Active?.Play(line, priority);
            return line;
        }

        public static AudioClip Clip(VoiceLine line)
        {
            if (line == null) return null;
            string path = line.Path;
            if (clips.TryGetValue(path, out AudioClip clip) && clip != null) return clip;
            if (missing.Contains(path)) return null;
            clip = Resources.Load<AudioClip>(path);
            if (clip == null) { missing.Add(path); return null; }
            clips[path] = clip; return clip;
        }

        /// <summary>True while this speaker's clip is audible (used to silence the murmur syllables and keep mouths moving).</summary>
        public static bool Speaking(string speaker) => Active != null && Active.channels.TryGetValue(speaker, out Channel channel) && channel.Source != null && channel.Source.isPlaying;
        public static float Remaining(string speaker) => Active != null && Active.channels.TryGetValue(speaker, out Channel channel) && channel.Source != null && channel.Source.isPlaying ? Mathf.Max(0, channel.Until - Time.unscaledTime) : 0;

        public static float Volume => PresentationSettings.VoiceVolume * CheckpointAudio.BusTrim(AudioBus.Voice) * (PresentationSettings.ReducedAudio ? 0.85f : 1f);

        public bool Play(VoiceLine line, int priority)
        {
            LastLine = line;
            AudioClip clip = Clip(line);
            if (clip == null) { SilentLines++; return false; }
            if (!focused || paused || AudioListener.pause || PresentationSettings.VoiceVolume <= 0.001f) return false;
            if (PresentationSettings.ReducedAudio && priority < Important) return false;
            // bosses and the shop share the airwaves: a lower-priority bark never talks over a higher one
            foreach (Channel other in channels.Values)
                if (other.Speaker != line.Speaker && other.Source != null && other.Source.isPlaying && other.Priority > priority) return false;
            if (!channels.TryGetValue(line.Speaker, out Channel channel))
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false; source.spatialBlend = 0; source.priority = 24; source.loop = false;
                channels[line.Speaker] = channel = new Channel { Source = source, Speaker = line.Speaker };
            }
            if (channel.Source.isPlaying && channel.Priority > priority && Time.unscaledTime < channel.Until - 0.25f) return false;
            channel.Source.Stop();
            channel.Source.clip = clip;
            channel.Source.volume = Mathf.Clamp01(Volume);
            channel.Source.pitch = 1f;
            channel.Source.Play();
            channel.Priority = priority; channel.Line = line; channel.Until = Time.unscaledTime + clip.length;
            PresentationSettings.VoiceDuck(clip.length + 0.25f);
            PlayedClips++;
            return true;
        }

        public void Stop(string speaker) { if (channels.TryGetValue(speaker, out Channel channel) && channel.Source != null) channel.Source.Stop(); }
        public void StopAll() { foreach (Channel channel in channels.Values) if (channel.Source != null) channel.Source.Stop(); }

        private void Update()
        {
            bool hold = AudioListener.pause || CheckpointAudio.SimulationPaused;
            if (hold != paused)
            {
                paused = hold;
                foreach (Channel channel in channels.Values) if (channel.Source != null) { if (hold) channel.Source.Pause(); else channel.Source.UnPause(); }
            }
            float volume = Mathf.Clamp01(Volume);
            foreach (Channel channel in channels.Values) if (channel.Source != null) channel.Source.volume = volume;
        }
        private void OnApplicationFocus(bool value) { focused = value; if (!value) StopAll(); }
        private void OnApplicationPause(bool value) { if (value) StopAll(); }
        private void OnDisable() { StopAll(); }

        /// <summary>Which voice bank a boss speaks from, by its story name; null for bosses without a voice.</summary>
        public static string BossSpeaker(UnitDefinition boss)
        {
            if (boss == null) return null;
            if (boss.Id == NightmareRules.CustomerId) return VoiceLines.LastCustomer;
            return boss.DisplayName switch
            {
                "THE KEEPER" => VoiceLines.Keeper,
                "THE CHAIR PORTER" => VoiceLines.ChairPorter,
                "THE DOOR THAT STAYED SHUT" => VoiceLines.Door,
                "THE BEDSIDE" => VoiceLines.Bedside,
                _ => null
            };
        }
        public static string StorySpeaker(string displayName) => displayName switch { "Hob" => VoiceLines.Hob, "Latch" => VoiceLines.Latch, "Postscript" => VoiceLines.Postscript, "Tally" => VoiceLines.Tally, _ => null };
    }
}
