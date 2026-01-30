using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class MienChoChecks
    {
        private const string Root = "PreviewExports/MienCho";
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool value, string message)
        {
            notes.Add((value ? "PASS " : "FAIL ") + message);
            Directory.CreateDirectory(Root); File.WriteAllLines(Root + "/Checks.txt", notes);
            if (!value) throw new InvalidOperationException(message);
        }
        public static void Import()
        {
            foreach (string name in new[] { "SharedLight", "WaitingGlow", "MirrorWitness" })
            {
                string path = "Assets/ArcaneBrigade/Resources/MienCho/" + name + ".mat";
                string shaderName = name == "SharedLight" ? "SharedLightField" : name;
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                var shader = Shader.Find("ArcaneBrigade/" + shaderName);
                if (shader == null) throw new InvalidOperationException("Missing shader " + shaderName);
                if (material == null) AssetDatabase.CreateAsset(new Material(shader), path);
                else { material.shader = shader; EditorUtility.SetDirty(material); }
            }
            foreach (string name in new[] { "EmptyChair", "WaitingRoom", "WithheldDawn", "OpenDawn" })
            {
                string path = "Assets/ArcaneBrigade/Resources/MienCho/" + name + ".png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = name == "EmptyChair" ? TextureImporterType.Sprite : TextureImporterType.Default;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.alphaIsTransparency = name == "EmptyChair"; importer.mipmapEnabled = false;
                importer.maxTextureSize = 2048; importer.textureCompression = TextureImporterCompression.Uncompressed;
                if (name == "EmptyChair") { importer.spriteImportMode = SpriteImportMode.Single; importer.spritePixelsPerUnit = 256; }
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            foreach (string name in new[] { "ReservedTable", "BreathingPhone", "LateReflection" })
            {
                var path = "Assets/ArcaneBrigade/Resources/MienCho/Encounters/" + name + ".png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 256; importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed; importer.npotScale = TextureImporterNPOTScale.None;
                importer.SaveAndReimport();
            }
        }
        public static void Run()
        {
            notes.Clear(); var c = Resources.Load<RunCatalog>("RunCatalog");
            Check(MienChoStory.Chapters.Length == 10 && MienChoStory.Chapters.Sum(x => x.Pages.Length) == 27, "Ten fixed milestones / twenty-seven authored pages");
            Check(CreatureLives.SpeciesCount == c.Companions.Length && CreatureLives.ArcCount == 6, "All sixty-six origins and six three-part companion arcs");
            foreach (var hero in c.Champions) Check(WaitingArrivals.For(hero.Id).Length > 100, "Individual dream arrival " + hero.Id);
            foreach (var u in c.Companions)
            {
                Check(CreatureLives.Has(u.Id) && CreatureLives.Origin(u.Id).Length > 100, "Individual life and intent " + u.Id);
                Check(CompanionDescriptions.Describe(u).Contains(CreatureLives.Origin(u.Id)), "Origin reaches actual companion description " + u.Id);
            }
            Check(c.Companions.Select(u => CreatureLives.Origin(u.Id)).Distinct().Count() == 66, "No shared fallback biographies");
            for (int wave = 1; wave <= 24; wave++)
            {
                using var s = new RunSession(c); s.Start("LanternKnight", 161026); s.State.Wave = wave; s.State.Phase = SessionPhase.Shop;
                int unlocked = MienChoStory.Available(s.State);
                Check(unlocked >= 1 && unlocked <= 10, "Bounded chapter availability wave " + wave);
                s.State.MienCho.ReadMask = (1 << unlocked) - 1;
                var json = JsonUtility.ToJson(s.State); s.Restore(JsonUtility.FromJson<RunSnapshot>(json));
                Check(s.State.MienCho.ReadMask == (1 << unlocked) - 1 && MienChoStory.NextUnread(s.State) == -1, "Journal read flags survive real save validation wave " + wave);
            }
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 161026); s.State.MienCho = null;
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(MienChoStory.NextUnread(s.State) == 0, "Legacy missing journal migrates without losing run");
                s.State.MienCho = new MienChoMemory { ReadMask = 1 << 6 };
                bool invalid = false; try { s.Restore(s.State); } catch (InvalidOperationException) { invalid = true; }
                Check(invalid, "Cannot forge future evidence in a saved journal");
            }
            for (int depth = 0; depth < 5; depth++)
            {
                Check(NightmareLighting.AmbientFor(depth, 0) >= .08f && NightmareLighting.AmbientFor(depth, 0) <= 1, "Darkness has a finite readable floor " + depth);
                if (depth > 0) Check(NightmareLighting.AmbientFor(depth, 0) < NightmareLighting.AmbientFor(depth - 1, 0), "World gets darker at chapter " + depth);
                Check(NightmareLighting.AmbientFor(depth, 1) > 1, "Real dawn restores light at depth " + depth);
            }
            Check(SharedLightField.Capacity == 16 && SharedLightField.HeroRadius >= 6, "Fixed light budget retains solo visibility and fifteen companions");
            Check(SharedLightField.Influence(Vector2.zero, 6) == 1 && SharedLightField.Influence(new Vector2(6, 0), 6) == 0, "Soft light has bounded support and full centre");
            Check(!NightmarePresence.HasEyes("RuneWisp") && !NightmarePresence.HasEyes("SmokingCoffin") && NightmarePresence.HasEyes("ClockBat"), "Anatomy uses eyes or ember rather than universal pairs");
            Check(new RunPauseGate { Story = true }.Paused, "Reading pauses combat independently from manual pause");
            for (int wave = 1; wave <= 24; wave++)
            {
                var kind = WaitingShopEncounter.ForWave(wave);
                Check(kind == WaitingEncounter.None || MienChoStory.Chapters[WaitingShopEncounter.ChapterFor(kind)].Wave <= wave, "Shop encounter never reveals future chapter wave=" + wave);
            }
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 161027); s.State.Wave = 12; s.State.Phase = SessionPhase.Shop;
                s.State.MienCho.Pages[5] = 1; s.State.MienCho.EncounterMask = 7;
                s.State.MienCho.FinishedMask = 3;
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.State.MienCho.Pages[5] == 1 && s.State.MienCho.EncounterMask == 7 && s.State.MienCho.FinishedMask == 3, "Per-chapter page and completed/encounter flags survive actual save restore");
                s.State.MienCho.Pages[5] = 99;
                bool blocked = false; try { s.Restore(s.State); } catch (InvalidOperationException) { blocked = true; }
                Check(blocked, "Invalid resumed page rejected");
                s.State.MienCho.Pages = null; s.State.MienCho.ReadMask = 1;
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.State.MienCho.Pages == null || s.State.MienCho.Pages.Length == 0, "Old journal missing page array remains loadable");
                s.State.Wave = 1; s.State.MienCho.FinishedMask = 0; s.State.MienCho.EncounterMask = 4;
                blocked = false; try { s.Restore(s.State); } catch (InvalidOperationException) { blocked = true; }
                Check(blocked, "Future mirror visit cannot be injected into an early save");
            }
            foreach (var name in new[] { "SharedLight", "WaitingGlow" }) Check(Resources.Load<Material>("MienCho/" + name)?.shader != null, "Player-included material " + name);
            foreach (var name in new[] { "WaitingRoom", "WithheldDawn", "OpenDawn" }) Check(Resources.Load<Texture2D>("MienCho/" + name) != null, "Drawn narrative panel imported " + name);
            Check(Resources.Load<Sprite>("MienCho/EmptyChair") != null, "Drawn chair imported as runtime sprite");
            foreach (var name in new[] { "ReservedTable", "BreathingPhone", "LateReflection" }) Check(Resources.Load<Sprite>("MienCho/Encounters/" + name) != null, "Runtime encounter art imported " + name);
            Check(File.Exists("Docs/Mien_Cho_Painted_Art_Prompts.json"), "Drawn asset prompt provenance retained");
            Debug.Log("MIEN_CHO_CHECKS " + notes.Count + " PASS");
        }
        public static void VerifyAndBuild()
        {
            Import(); Run(); RosterCapAndBondChecks.Run(); RecruitmentChecks.Run(); HeroIdentityChecks.RefreshDescriptions();
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = new[] { "Assets/ArcaneBrigade/Scenes/RunBoot.unity", "Assets/ArcaneBrigade/Scenes/RunFrontend.unity", "Assets/ArcaneBrigade/Scenes/RunArena.unity", "Assets/ArcaneBrigade/Scenes/WaitingRoom.unity" },
                locationPathName = "Builds/HeroIdentity/ArcaneBrigade.exe", target = BuildTarget.StandaloneWindows64, options = BuildOptions.None });
            File.WriteAllText(Root + "/Build.txt", "Result: " + report.summary.result + "\nErrors: " + report.summary.totalErrors + "\nWarnings: " + report.summary.totalWarnings + "\nBytes: " + report.summary.totalSize + "\nPath: Builds/HeroIdentity/ArcaneBrigade.exe");
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new InvalidOperationException("Waiting Realm build failed");
            GameReviewSnapshot.Export();
        }
    }
}
