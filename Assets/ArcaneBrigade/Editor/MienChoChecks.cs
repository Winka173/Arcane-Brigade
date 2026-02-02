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
            Check(MienChoStory.Chapters.Length == 21 && MienChoStory.Chapters.All(x => x.Pages.Length >= 1 && x.Pages.All(p => p.Length > 80)), "Act I canon: arrival, nineteen shop beats and the final hall");
            Check(MienChoStory.Chapters.Where(x => x.Shop).Select(x => x.Wave).SequenceEqual(Enumerable.Range(1, 19)), "Every shop from wave 1 to 19 has its own story beat");
            for (int i = 1; i < MienChoStory.Chapters.Length; i++) Check(MienChoStory.Chapters[i].Wave >= MienChoStory.Chapters[i - 1].Wave, "Story beats stay in wave order " + i);
            for (int i = 0; i < MienChoStory.Chapters.Length; i++)
                Check(ActOneCanon.ChoiceOptions[i] == null || MienChoStory.Chapters[i].Pages.Length == 4 && ActOneCanon.ChoiceOptions[i].Length == 2, "Choice beat has an opening, two branches and an aftermath " + i);
            Check(MienChoStory.NextPage(8, 0, 0) == -1 && MienChoStory.NextPage(8, 0, 2) == 2 && MienChoStory.NextPage(8, 1, 1) == 3 && MienChoStory.NextPage(8, 2, 2) == 3 && MienChoStory.PreviousPage(8, 3, 1) == 1 && MienChoStory.NextPage(3, 0, 0) == 1, "Choice pages skip the branch not taken");
            Check(ActOneCanon.Broken(2) == 0 && ActOneCanon.Broken(3) == 1 && ActOneCanon.Broken(14) == 6 && ActOneCanon.Broken(20) == 7 && ActOneCanon.Broken(500) == 7, "Seven lanterns break on their canon waves; the eighth survives the act");
            Check(ActOneCanon.LampState(7, 18) == 0 && ActOneCanon.LampState(7, 19) == 1 && ActOneCanon.LampState(0, 2) == 1 && ActOneCanon.LampState(0, 3) == 2 && ActOneCanon.LampLoss(4).Contains("face"), "Lantern states flicker before breaking and name what was lost");
            Check(ActOneCanon.Zone(5) == 0 && ActOneCanon.Zone(6) == 1 && ActOneCanon.Zone(15) == 2 && ActOneCanon.Zone(20) == 3 && ActOneCanon.BossWave(5) && ActOneCanon.BossWave(20) && !ActOneCanon.BossWave(4) && !ActOneCanon.BossWave(12), "Four zones of five waves, each closed by a boss");
            foreach (int wave in new[] { 4, 5, 10, 15 })
            {
                using var s = new RunSession(c); s.Start("LanternKnight", 161040); s.State.Wave = wave;
                var combat = s.PrepareCombat();
                Check(wave == 4 ? combat.Boss == null : combat.Boss != null && combat.Boss.DisplayName == ActOneCanon.BossName(wave), "Act I story boss on wave " + wave);
                if (wave == 5) Check(combat.Boss.Id == "OutsideStageKeeper", "The Keeper guards the gate at wave 5");
            }
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 161041); s.State.Wave = 9; s.State.Phase = SessionPhase.Shop;
                s.State.MienCho = new MienChoMemory { Revision = 1, ReadMask = 7, FinishedMask = 7, Pages = new int[10] };
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.State.MienCho.Revision == MienChoMemory.CurrentRevision && s.State.MienCho.Pages.Length == 21 && MienChoStory.NextUnread(s.State) == MienChoStory.Available(s.State) - 1, "First-canon save migrates and opens only the current beat");
                s.State.MienCho.Choices[3] = 1;
                bool rejected = false; try { s.Restore(s.State); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "A choice cannot be stored on a beat that has none");
                s.State.MienCho.Choices[3] = 0; s.State.MienCho.Choices[14] = 2;
                rejected = false; try { s.Restore(s.State); } catch (InvalidOperationException) { rejected = true; }
                Check(rejected, "A future choice cannot be forged");
                s.State.MienCho.Choices[14] = 0; s.State.MienCho.Choices[8] = 2;
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(MienChoStory.Choice(s.State, 8) == 2, "The telephone choice survives a real save");
            }
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
                Check(unlocked >= 1 && unlocked <= 21, "Bounded chapter availability wave " + wave);
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
                Check(NightmareLighting.AmbientFor(depth, 0) >= .015f && NightmareLighting.AmbientFor(depth, 0) <= 1, "Ambient stays finite while party lights preserve readability " + depth);
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
