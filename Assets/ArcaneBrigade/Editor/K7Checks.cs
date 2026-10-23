using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K7Checks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException("K7: " + name); notes.Add("PASS " + name); };
            string root = Path.Combine(Application.temporaryCachePath, "ArcaneK7", Guid.NewGuid().ToString("N"));
            var storage = new RunStorage(root); var old = new PlayerProfile { Version = 2, BestWave = 14, Wins = 3, Cosmetic = 1 };
            storage.SaveProfile(old); PlayerProfile profile = storage.LoadProfile();
            require(profile.Version == 3 && profile.Wins == 3 && profile.BestWave == 14 && profile.MusicVolume == 0.8f && profile.SfxVolume == 0.85f && profile.ShowDamageNumbers && profile.ShowLootIndicators, "Profile2 upgrades audio/readability defaults without touching progression");
            profile.MusicVolume = 0; profile.SfxVolume = 0.25f; profile.ReducedAudio = true; profile.ReducedMotion = true; profile.ShowLootIndicators = false; profile.ShowDamageNumbers = false; profile.HighReadability = true;
            storage.SaveProfile(profile); PlayerProfile loaded = storage.LoadProfile();
            require(loaded.MusicVolume == 0 && loaded.SfxVolume == 0.25f && loaded.ReducedAudio && loaded.ReducedMotion && !loaded.ShowLootIndicators && !loaded.ShowDamageNumbers && loaded.HighReadability, "Every audio/accessibility option survives durable roundtrip");
            old = new PlayerProfile { Version = 1, Wins = 4 }; storage.SaveProfile(old); require(storage.LoadProfile().Version == 3 && storage.LoadProfile().Wins == 4, "Profile1 direct migration keeps old wins");
            require(MovementInput.FilterPad(new Vector2(0.17f, 0)) == Vector2.zero && MovementInput.FilterPad(new Vector2(float.NaN, 1)) == Vector2.zero, "Gamepad drift/nonfinite input ignored");
            require(MovementInput.FilterPad(Vector2.one).magnitude <= 1.001f && MovementInput.FilterPad(Vector2.right).x == 1, "Analog diagonal has normalized speed and full-stick reach");
            foreach (Vector2 size in new[] { new Vector2(1920, 1080), new Vector2(3840, 2160), new Vector2(2560, 1080), new Vector2(1600, 1000), new Vector2(1334, 750) })
            {
                Rect safe = new Rect(35, 24, size.x - 70, size.y - 48);
                Matrix4x4 matrix = ArcadeTypography.FitViewport(new Rect(0, 0, size.x, size.y), safe, size.y, 1280, 720);
                Vector3 start = matrix.MultiplyPoint3x4(Vector3.zero); Vector3 end = matrix.MultiplyPoint3x4(new Vector3(1280, 720, 0));
                require(start.x >= safe.xMin - 0.1f && end.x <= safe.xMax + 0.1f && start.y >= size.y - safe.yMax - 0.1f && end.y <= size.y - safe.yMin + 0.1f, "Safe-area design canvas fits " + size);
            }
            foreach (Font font in new[] { ArcadeTypography.Body, ArcadeTypography.Heading, ArcadeTypography.Numbers })
            {
                string sample = "Đội quân thất lạc — Ống kính ngái ngủ | 1.234 vàng / 00:59 | Đã mua";
                font.RequestCharactersInTexture(sample, 28);
                require(sample.All(glyph => char.IsWhiteSpace(glyph) || font.HasCharacter(glyph)), "Vietnamese readability glyphs " + font.name);
                foreach (int size in new[] { 16, 22, 32, 48 })
                {
                    font.RequestCharactersInTexture("0123456789Độấỵ", size);
                    require(font.GetCharacterInfo('Đ', out CharacterInfo info, size) && info.glyphHeight > 0, "Glyph has actual ink at size " + size + " " + font.name);
                }
            }
            foreach (string name in new[] { "NightShift_Base", "NightShift_Ensemble", "NightShift_Boss", "ShopAfterHours" })
            {
                string path = "Assets/ArcaneBrigade/Resources/Audio/" + name + ".wav";
                byte[] bytes = File.ReadAllBytes(path);
                require(BitConverter.ToInt32(bytes, 24) == 44100 && BitConverter.ToInt16(bytes, 34) == 16 && bytes.Length > 44100 * 10, name + " authored44.1kHzPCM source not empty placeholder");
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                require(importer.defaultSampleSettings.loadType == AudioClipLoadType.Streaming, name + " streams in player rather than decompress entire song");
            }
            for (int cue = 0; cue < 33; cue++)
            {
                AudioClip clip = ArcadeSound.Cue(cue); var data = new float[clip.samples]; clip.GetData(data, 0);
                require(data.All(float.IsFinite) && data.Max(sample => Mathf.Abs(sample)) < 1 && data.Sum(sample => sample * sample) > 0.001, "Cue " + cue + " finite unclipped audible-energy signal only technical check");
                UnityEngine.Object.DestroyImmediate(clip);
            }
            require(CheckpointAudio.IsDangerCue(24) && CheckpointAudio.IsDangerCue(25) && !CheckpointAudio.IsDangerCue(19) && CheckpointAudio.GroupDelay(10, true) > CheckpointAudio.GroupDelay(10, false), "Reserved danger lane and cosmetic density grouping defined");
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 7707); string before = JsonUtility.ToJson(session.State);
                foreach (bool mode in new[] { false, true }) { profile.ReducedAudio = profile.ReducedMotion = profile.HighReadability = mode; profile.ShowLootIndicators = profile.ShowDamageNumbers = !mode; PresentationSettings.Apply(profile); }
                require(before == JsonUtility.ToJson(session.State), "Settings cannot alter RNG/hitboxes/timing/currency/damage");
                var pause = new RunPauseGate { Manual = true, Backgrounded = true, Details = true };
                pause.Backgrounded = false; require(pause.Paused, "Foregrounding cannot release manual/details pause ownership");
            }
            require(K8AudioChecks.ReservedLanesAreIsolated(), "Cosmetic voices cannot steal reserved priority critical lanes");
            string sandbox = File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/WeaponSandbox.cs");
            require(sandbox.Contains("GetComponentsInChildren<Renderer>(true, renderers)"), "Sandbox avoids renderer array allocation every frame");
            require(PlayerSettings.enableFrameTimingStats, "Frame timing enabled for nativeGPU measurement attempt");
            require(ArcadeTypography.DisplayText("Ống kính ngủ") == "Ống kính ngủ", "Combining Vietnamese marks normalize to stable display graphemes");
            PresentationSettings.Apply(new PlayerProfile());
            Directory.CreateDirectory("PreviewExports/K7"); File.WriteAllLines("PreviewExports/K7/Checks.txt", notes);
            Debug.Log("K7_CHECKS_PASSED " + notes.Count);
        }
    }
}
