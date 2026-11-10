using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ExpandedPerformanceBuilder
    {
        private const string Root = "PreviewExports/PerformanceExpansion";
        [MenuItem("Arcane Brigade/Animation/Import expanded performance")]
        public static void Build()
        {
            var manifest = JsonUtility.FromJson<PerformanceExpansionBuilder.Manifest>(File.ReadAllText(Root + "/Manifest.json"));
            const string path = "Assets/ArcaneBrigade/Resources/ExpandedPerformanceLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<ExpandedPerformanceLibrary>(path);
            if (library == null) { library = ScriptableObject.CreateInstance<ExpandedPerformanceLibrary>(); AssetDatabase.CreateAsset(library, path); }
            var entries = new List<ExpandedPerformanceEntry>();
            CharacterActingLibrary acting = CharacterActingLibrary.Current;
            EnsembleArtwork ensemble = EnsembleArtwork.Current;
            var companions = acting.Entries.ToList();
            var weapons = ensemble.Weapons.ToList();
            var coverage = new List<string> { "Job\tKind\tStatus\tActor\tEvolution\tFrames" };
            int completed = 0;
            foreach (PerformanceExpansionBuilder.ArtJob job in manifest.Jobs)
            {
                if (!File.Exists(job.Output)) { coverage.Add(job.Id + "\t" + job.Kind + "\tMISSING\t\t\t0"); continue; }
                int count = job.Actors.Length * job.FramesPerActor;
                Sprite[] sprites = K25AtlasImporter.Import(job.Output, job.Id.Replace("-", "") + "Performance", job.Columns, job.Rows, Enumerable.Range(0, count).ToArray(), Enumerable.Range(0, job.Rows + 1).Select(row => row * 1024 / job.Rows).ToArray(), true, true);
                PerformanceSpriteOutline.Apply(job.Output);
                sprites = AssetDatabase.LoadAllAssetsAtPath(job.Output).OfType<Sprite>().OrderBy(sprite => sprite.name).ToArray();
                for (int index = 0; index < job.Actors.Length; index++)
                {
                    PerformanceExpansionBuilder.ActorSource actor = job.Actors[index];
                    Sprite[] frames = sprites.Skip(index * job.FramesPerActor).Take(job.FramesPerActor).ToArray();
                    float reference = frames[0].bounds.size.y;
                    var entry = new ExpandedPerformanceEntry { Id = actor.Id, Kind = job.Kind, Evolution = actor.Evolution, Frames = frames, ReferenceHeight = reference };
                    entries.Add(entry);
                    if (job.Kind == "companions")
                    {
                        companions.RemoveAll(previous => previous.Id == actor.Id);
                        companions.Add(new CharacterActingEntry { Id = actor.Id, Curious = new[] { frames[0] }, Cheer = new[] { frames[1] }, Hurt = new[] { frames[2] }, Defeat = new[] { frames[3] }, ReferenceHeight = reference });
                    }
                    if (job.Kind == "weapons")
                    {
                        weapons.RemoveAll(previous => previous.Id == actor.Id);
                        weapons.Add(new EnsembleClip { Id = actor.Id, Frames = frames, ReferenceWidth = frames[0].bounds.size.x });
                    }
                    coverage.Add(job.Id + "\t" + job.Kind + "\tIMPORTED\t" + actor.Id + "\t" + actor.Evolution + "\t" + frames.Length);
                }
                completed++;
            }
            library.Entries = entries.ToArray(); acting.Entries = companions.ToArray(); ensemble.Weapons = weapons.ToArray();
            EditorUtility.SetDirty(library); EditorUtility.SetDirty(acting); EditorUtility.SetDirty(ensemble);
            AssetDatabase.SaveAssets(); ExpandedPerformanceLibrary.Invalidate(); SpriteInkMetricsBuilder.Build();
            File.WriteAllLines(Root + "/Imported_Coverage.tsv", coverage);
            File.WriteAllText(Root + "/Progress.txt", completed + " / " + manifest.Jobs.Length + " sheets imported; " + entries.Count + " actor clips. Missing sheets remain explicit; existing art is preserved.\n");
            Check();
        }
        [MenuItem("Arcane Brigade/Animation/Check expanded performance")]
        public static void Check()
        {
            ExpandedPerformanceLibrary library = ExpandedPerformanceLibrary.Current;
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var notes = new List<string>();
            void Require(bool value, string caption) { if (!value) throw new InvalidOperationException("Expanded performance: " + caption); notes.Add("PASS " + caption); }
            Require(library != null && library.Entries.Length > 0, "Real painted clips imported");
            var identities = new HashSet<string>();
            foreach (ExpandedPerformanceEntry entry in library.Entries)
            {
                Require(identities.Add(entry.Kind + ":" + entry.Id + ":" + entry.Evolution), "No duplicate clip " + entry.Kind + "/" + entry.Id + "/" + entry.Evolution);
                Require(entry.Frames.Length == (entry.Kind == "locomotion" ? 8 : 4), "Complete pose count " + entry.Id);
                Require(entry.ReferenceHeight > 0, "Positive stable pose scale " + entry.Id);
                Require(library.Find(entry.Kind, entry.Id, entry.Evolution) == entry, "Runtime lookup resolves actual clip " + entry.Id + "/" + entry.Evolution);
                foreach (Sprite frame in entry.Frames)
                {
                    Require(frame != null && frame.rect.width > 16 && frame.rect.height > 16, "Nonempty imported pose " + entry.Id);
                    Require(frame.pivot.y == 0, "Ground-contact pivot " + frame.name);
                    Require(frame.vertices.Length >= 3 && frame.triangles.Length >= 3, "Imported silhouette has a real render mesh " + frame.name);
                }
                if (entry.Kind == "weapons")
                {
                    Require(EnsembleArtwork.Current.Weapon(entry.Id)?.Frames.SequenceEqual(entry.Frames) == true, "Weapon mechanism uses new art " + entry.Id);
                    continue;
                }
                if (entry.Kind == "companions")
                {
                    Require(CharacterActingLibrary.Current.Find(entry.Id)?.Hurt[0] == entry.Frames[2], "Companion reaction reaches acting library " + entry.Id);
                    continue;
                }
                UnitDefinition definition = catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).Concat(catalog.Bosses).Concat(new[] { NightmareAssets.Current.Customer, OutsideStageAssets.Current.Boss }).First(unit => unit.Id == entry.Id);
                var state = new UnitState { Definition = definition, Alive = true, AttackStarted = -100, LastHit = -100, Position = Vector2.zero, PreviousPosition = Vector2.zero };
                Require(entry.Select(state, 5, true) == null, "Reduced motion retains stable existing animation " + entry.Id);
                state.PendingAttack = true;
                if (entry.HurtOnly) Require(entry.Select(state, 5, false) == null, "Redrawn body keeps its own windup " + entry.Id);
                else Require(entry.Select(state, 5, false) == entry.Frames[entry.Kind == "locomotion" ? 5 : 0], "Actual windup selects painted anticipation " + entry.Id);
                state.PendingAttack = false;
                if (entry.Kind != "locomotion")
                {
                    state.LastHit = 4.95f;
                    Require(entry.Select(state, 5, false) == entry.Frames[2], "Actual damage selects painted recoil " + entry.Id);
                    state.LastHit = -100;
                    Require(entry.Select(state, 5, false) == null, "Existing idle remains fallback " + entry.Id);
                }
                else
                {
                    state.Position = Vector2.right;
                    Require(entry.Select(state, 5, false) == entry.Frames[2 + Mathf.FloorToInt(5 * 8) % 3], "Actual movement selects stride pose " + entry.Id);
                }
            }
            Require(library.Find("bosses", "not-a-real-boss") == null, "Missing identity never borrows another actor");
            File.WriteAllLines(Root + "/Focused_Checks.txt", notes);
            Debug.Log("EXPANDED_PERFORMANCE_CHECKS " + notes.Count + " PASS");
        }
        public static void Gate()
        {
            Build(); ShopPresentationBuilder.Gate();
        }
        public static void RequireComplete()
        {
            Build();
            var manifest = JsonUtility.FromJson<PerformanceExpansionBuilder.Manifest>(File.ReadAllText(Root + "/Manifest.json"));
            string[] missing = manifest.Jobs.Where(job => !File.Exists(job.Output)).Select(job => job.Id).ToArray();
            if (missing.Length > 0) throw new InvalidOperationException("Unfinished painted sheets: " + string.Join(", ", missing));
            RequireCatalogCoverage(manifest);
        }
        public static void CompleteGate()
        {
            RequireComplete(); HeroIdentityChecks.Run(); DreamLevitationChecks.BuildAndCheck(); ShopPresentationBuilder.Gate();
        }
        private static void RequireCatalogCoverage(PerformanceExpansionBuilder.Manifest manifest)
        {
            ExpandedPerformanceLibrary library = ExpandedPerformanceLibrary.Current;
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            if (library.Entries.Length != manifest.Jobs.Sum(job => job.Actors.Length)) throw new InvalidOperationException("Expanded clip count does not match authoring manifest");
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions))
            {
                if (CharacterActingLibrary.Current.Find(unit.Id) == null) throw new InvalidOperationException("Missing catalog acting " + unit.Id);
                if (library.Find("locomotion", unit.Id) != null) continue;
                AuthoredMotionSet motion = unit.AuthoredMotion;
                Sprite[] frames = motion == null ? unit.AnimationFrames : motion.Idle.Concat(motion.Walk).Concat(motion.Windup).Concat(motion.Release).Concat(motion.Recovery).ToArray();
                if (frames.Where(frame => frame != null).Distinct().Count() < 8) throw new InvalidOperationException("Missing eight-pose catalog locomotion " + unit.Id);
            }
            foreach (UnitDefinition enemy in catalog.Enemies)
                foreach (int stage in new[] { 0, 1 })
                    if (library.Find("enemies", enemy.Id, stage) == null) throw new InvalidOperationException("Missing catalog enemy reaction " + enemy.Id + "/" + stage);
            foreach (WeaponDefinition weapon in catalog.Weapons.Where(weapon => weapon.Sprite != null))
                if (EnsembleArtwork.Current.Weapon(weapon.Id) == null) throw new InvalidOperationException("Missing catalog weapon action " + weapon.Id);
            Debug.Log("EXPANDED_CATALOG_COVERAGE_PASS " + library.Entries.Length + " clips; all catalog acting, locomotion, reactions and weapons covered");
        }
    }
}
