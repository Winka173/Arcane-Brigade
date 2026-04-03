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
    /// Playtest item 2a, the asset half of the mix: builds Resources/Audio/ArcaneMix.mixer (Master -> Music, SFX, UI, Voice, Boss,
    /// each volume exposed under the names in <see cref="AudioMix"/>) and measures every SFX clip once into the loudness table
    /// (Resources/Audio/Arcane/loudness.json) that brings all effects to one target. Run it again after new sound files arrive.
    /// Batch: -executeMethod ArcaneBrigade.Editor.AudioMixBuilder.Apply
    /// </summary>
    public static class AudioMixBuilder
    {
        public const string MixerPath = "Assets/ArcaneBrigade/Resources/Audio/ArcaneMix.mixer";
        public const string LoudnessPath = "Assets/ArcaneBrigade/Resources/Audio/Arcane/loudness.json";
        /// <summary>Largest correction the table applies either way: a clip far off the target is a content problem, not a gain one.</summary>
        public const float MaxCorrectionDb = 6f;
        private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        [MenuItem("Arcane Brigade/Audio/Build mixer and loudness table")]
        public static void Apply()
        {
            BuildMixer();
            int rows = BuildLoudness();
            AssetDatabase.SaveAssets();
            Debug.Log("AUDIO_MIX_BUILT mixer=" + MixerPath + " loudnessRows=" + rows);
        }

        public static bool MixerComplete(AudioMixer mixer)
        {
            if (mixer == null) return false;
            foreach (string name in AudioMix.GroupNames)
                if (!mixer.FindMatchingGroups("Master/" + name).Any(group => group.name == name)) return false;
            foreach (string param in AudioMix.VolumeParams.Concat(new[] { AudioMix.MasterParam }))
                if (!mixer.GetFloat(param, out _)) return false;
            return true;
        }

        private static Type EditorType(string name) => typeof(EditorWindow).Assembly.GetType(name, true);

        public static void BuildMixer()
        {
            var existing = AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath);
            if (MixerComplete(existing)) { Debug.Log("AudioMixBuilder: mixer already complete"); return; }
            if (existing != null) AssetDatabase.DeleteAsset(MixerPath);

            // AudioMixerController has no public API: the same internal calls the Audio Mixer window makes
            Type controllerType = EditorType("UnityEditor.Audio.AudioMixerController");
            Type groupType = EditorType("UnityEditor.Audio.AudioMixerGroupController");
            Type pathType = EditorType("UnityEditor.Audio.AudioGroupParameterPath");
            object controller = controllerType.GetMethod("CreateMixerControllerAtPath", All).Invoke(null, new object[] { MixerPath });
            object master = controllerType.GetProperty("masterGroup", All).GetValue(controller);
            MethodInfo createGroup = controllerType.GetMethods(All).First(m => m.Name == "CreateNewGroup");
            MethodInfo addChild = controllerType.GetMethod("AddChildToParent", All);
            MethodInfo addExposed = controllerType.GetMethod("AddExposedParameter", All);
            MethodInfo volumeGuid = groupType.GetMethod("GetGUIDForVolume", All);
            var names = new Dictionary<GUID, string>();
            void Expose(object group, string param)
            {
                var guid = (GUID)volumeGuid.Invoke(group, null);
                object path = Activator.CreateInstance(pathType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new[] { group, (object)guid }, null);
                addExposed.Invoke(controller, new[] { path });
                names[guid] = param;
            }
            Expose(master, AudioMix.MasterParam);
            for (int bus = 0; bus < AudioMix.GroupNames.Length; bus++)
            {
                object[] args = createGroup.GetParameters().Length == 2 ? new object[] { AudioMix.GroupNames[bus], false } : new object[] { AudioMix.GroupNames[bus] };
                object group = createGroup.Invoke(controller, args);
                addChild.Invoke(controller, new[] { group, master });
                Expose(group, AudioMix.VolumeParams[bus]);
            }
            PropertyInfo exposedProperty = controllerType.GetProperty("exposedParameters", All);
            var exposed = (Array)exposedProperty.GetValue(controller);
            Type exposedType = exposed.GetType().GetElementType();
            FieldInfo guidField = exposedType.GetField("guid", All), nameField = exposedType.GetField("name", All);
            for (int index = 0; index < exposed.Length; index++)
            {
                object parameter = exposed.GetValue(index);
                if (!names.TryGetValue((GUID)guidField.GetValue(parameter), out string name)) continue;
                nameField.SetValue(parameter, name);
                exposed.SetValue(parameter, index);
            }
            exposedProperty.SetValue(controller, exposed);
            controllerType.GetMethod("OnChangedExposedParameter", All)?.Invoke(controller, null);
            EditorUtility.SetDirty((UnityEngine.Object)controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(MixerPath, ImportAssetOptions.ForceUpdate);
            if (!MixerComplete(AssetDatabase.LoadAssetAtPath<AudioMixer>(MixerPath))) throw new InvalidOperationException("AudioMixBuilder: mixer built but buses/parameters are incomplete");
        }

        /// <summary>Measures each SFX/UI/voice clip of the cue library (ambience beds excepted) and writes the per-clip gain table.</summary>
        public static int BuildLoudness()
        {
            ArcaneCueLibrary.EnsureLoaded();
            var rows = new List<SfxGovernor.LoudnessRow>();
            var seen = new HashSet<string>();
            foreach (ArcaneCueDef def in ArcaneCueLibrary.All)
            {
                if (def.busId == AudioBus.Ambience || def.clips == null) continue;
                foreach (string clipPath in def.clips)
                {
                    AudioClip clip = Resources.Load<AudioClip>("Audio/Arcane/" + clipPath);
                    if (clip == null || !seen.Add(clip.name)) continue;
                    float db = MeasureDb(AssetDatabase.GetAssetPath(clip));
                    if (float.IsNaN(db) || db <= -70f) continue;
                    rows.Add(new SfxGovernor.LoudnessRow { clip = clip.name, rmsDb = (float)Math.Round(db, 2) });
                }
            }
            if (rows.Count == 0) throw new InvalidOperationException("AudioMixBuilder: no SFX clip could be measured");
            List<float> sorted = rows.Select(row => row.rmsDb).OrderBy(value => value).ToList();
            float target = (float)Math.Round(sorted[sorted.Count / 2], 2); // the median: the mix keeps its overall level, the outliers come in
            foreach (SfxGovernor.LoudnessRow row in rows) row.gainDb = (float)Math.Round(Mathf.Clamp(target - row.rmsDb, -MaxCorrectionDb, MaxCorrectionDb), 2);
            rows.Sort((a, b) => string.CompareOrdinal(a.clip, b.clip));
            File.WriteAllText(LoudnessPath, JsonUtility.ToJson(new SfxGovernor.LoudnessTable { targetDb = target, clips = rows.ToArray() }, true));
            AssetDatabase.ImportAsset(LoudnessPath, ImportAssetOptions.ForceUpdate);
            return rows.Count;
        }

        /// <summary>Short-term loudness of a WAV file: the loudest 100 ms RMS window (25 ms hop), in dBFS. NaN when unreadable.</summary>
        public static float MeasureDb(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) || !File.Exists(assetPath)) return float.NaN;
            byte[] bytes = File.ReadAllBytes(assetPath);
            if (bytes.Length < 44 || bytes[0] != 'R' || bytes[8] != 'W') return float.NaN;
            int format = 0, channels = 0, rate = 0, bits = 0, dataAt = -1, dataLength = 0;
            for (int at = 12; at + 8 <= bytes.Length;)
            {
                string id = System.Text.Encoding.ASCII.GetString(bytes, at, 4);
                int size = BitConverter.ToInt32(bytes, at + 4);
                if (id == "fmt ") { format = BitConverter.ToInt16(bytes, at + 8); channels = BitConverter.ToInt16(bytes, at + 10); rate = BitConverter.ToInt32(bytes, at + 12); bits = BitConverter.ToInt16(bytes, at + 22); }
                else if (id == "data") { dataAt = at + 8; dataLength = Math.Min(size, bytes.Length - dataAt); break; }
                at += 8 + size + (size & 1);
            }
            if (dataAt < 0 || channels <= 0 || rate <= 0) return float.NaN;
            int stride = bits / 8 * channels;
            if (stride <= 0) return float.NaN;
            int frames = dataLength / stride;
            var mono = new float[frames];
            for (int frame = 0; frame < frames; frame++)
            {
                float sum = 0f;
                for (int channel = 0; channel < channels; channel++)
                {
                    int at = dataAt + frame * stride + channel * bits / 8;
                    if (format == 1 && bits == 16) sum += BitConverter.ToInt16(bytes, at) / 32768f;
                    else if (format == 1 && bits == 24) sum += ((bytes[at] | bytes[at + 1] << 8 | (sbyte)bytes[at + 2] << 16)) / 8388608f;
                    else if (format == 3 && bits == 32) sum += BitConverter.ToSingle(bytes, at);
                    else return float.NaN;
                }
                mono[frame] = sum / channels;
            }
            int window = Mathf.Max(1, rate / 10), hop = Mathf.Max(1, rate / 40);
            double best = 0;
            for (int start = 0; start == 0 || start + window <= frames; start += hop)
            {
                int end = Math.Min(frames, start + window);
                double energy = 0;
                for (int index = start; index < end; index++) energy += mono[index] * mono[index];
                best = Math.Max(best, energy / Math.Max(1, end - start));
                if (end >= frames) break;
            }
            return best <= 1e-12 ? -120f : (float)(10.0 * Math.Log10(best));
        }
    }
}
