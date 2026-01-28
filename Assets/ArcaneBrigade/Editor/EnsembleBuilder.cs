using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class EnsembleBuilder
    {
        public static readonly string[] Merchants = { "ReceiptMaw", "EggOracle", "GiantGoldMerchant", "GiantEggMerchant" };
        public static readonly string[] Weapons = { "PaperFan", "MoonFeatherFan", "ParasolNeedle", "InkPistol", "ShellCannon", "PlasmaTrident", "FireExtinguisher" };
        [MenuItem("Arcane Brigade/Animation/Import ensemble performance")]
        public static void Build()
        {
            const string path = "Assets/ArcaneBrigade/Resources/EnsembleArtwork.asset";
            EnsembleArtwork art = AssetDatabase.LoadAssetAtPath<EnsembleArtwork>(path);
            if (art == null) { art = ScriptableObject.CreateInstance<EnsembleArtwork>(); AssetDatabase.CreateAsset(art, path); }
            art.Merchants = Merchants.Select(id => Import(id, "Handoff", false)).ToArray();
            EnsembleClip[] extraWeapons = art.Weapons.Where(clip => !Weapons.Contains(clip.Id)).ToArray();
            art.Weapons = Weapons.Select(id => Import(id, "Action", true)).Concat(extraWeapons).ToArray();
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets(); Check();
        }
        private static EnsembleClip Import(string id, string kind, bool stabilize)
        {
            string path = "Assets/ArcaneBrigade/Art/Ensemble/" + id + "_" + kind + "_v1.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            int rows = texture.width > texture.height * 2.5f ? 1 : 2;
            Sprite[] frames = K25AtlasImporter.Import(path, id + kind, 4, rows, Enumerable.Range(0, rows * 4).ToArray(), rows == 1 ? new[] { 0, 1024 } : new[] { 0, 512, 1024 }, true);
            texture = frames[0].texture;
            float width = frames.Max(frame => frame.bounds.size.x);
            {
                var factories = new SpriteDataProviderFactories(); factories.Init();
                var provider = factories.GetSpriteEditorDataProviderFromObject(AssetImporter.GetAtPath(path)); provider.InitSpriteEditorDataProvider();
                SpriteRect[] rectangles = provider.GetSpriteRects().OrderBy(rect => rect.name).ToArray();
                for (int index = 0; index < rectangles.Length; index++)
                {
                    int left = index % 4 * texture.width / 4, right = (index % 4 + 1) * texture.width / 4;
                    int bottom = texture.height - (index / 4 + 1) * texture.height / rows, top = texture.height - index / 4 * texture.height / rows;
                    if (stabilize) rectangles[index].rect = new Rect(left, bottom, right - left, top - bottom);
                    rectangles[index].pivot = new Vector2(.5f, .5f); rectangles[index].alignment = SpriteAlignment.Custom;
                }
                provider.SetSpriteRects(rectangles); provider.Apply(); ((TextureImporter)AssetImporter.GetAtPath(path)).SaveAndReimport();
                frames = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(frame => frame.name).ToArray();
            }
            return new EnsembleClip { Id = id, Frames = frames, ReferenceWidth = width };
        }
        public static void Check()
        {
            var notes = new List<string>(); var coverage = new List<string> { "# Ensemble runtime coverage", "", "Generated clips and procedural source articulation are recorded separately.", "", "| Actor | Gesture | Existing painted frames | New acting sheet |", "| --- | --- | --- | --- |" };
            void Assert(bool valid, string caption) { if (!valid) throw new InvalidOperationException("Ensemble: " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); EnsembleArtwork art = EnsembleArtwork.Current;
            using (var targetingSession = new RunSession(catalog))
            {
                targetingSession.Start(catalog.Champions[0].Id, 761026);
                var simulation = new CombatSimulation(targetingSession.PrepareCombat()); simulation.Start(761026);
                var method = typeof(CombatSimulation).GetMethod("FindTarget", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var target = (Func<UnitState, float, UnitState>)Delegate.CreateDelegate(typeof(Func<UnitState, float, UnitState>), simulation, method);
                var hostile = new UnitState { Faction = Faction.Enemy };
                foreach (float range in new[] { 0f, .5f, 3f, 10f })
                    foreach (float distance in new[] { 0f, .5f, 3f, 6f })
                    {
                        hostile.Position = simulation.Champion.Position + Vector2.right * distance;
                        UnitState expected = distance * distance < range * range ? simulation.Champion : null;
                        Assert(target(hostile, range) == expected, "Direct enemy target retains strict range boundary " + range + "/" + distance);
                    }
                hostile.Position = simulation.Champion.Position;
                simulation.Champion.Alive = false; Assert(target(hostile, 10) == null, "Enemy cannot select a defeated champion");
                simulation.Champion.Alive = true; simulation.Champion.EntranceUntil = 1;
                Assert(target(hostile, 10) == null, "Enemy cannot select an entering champion");
                simulation.Champion.EntranceUntil = 0;
                Assert(target(hostile, -1) == null, "Invalid enemy target range stays empty");
            }
            var spatial = new SpatialUnitIndex();
            var neighbours = new List<UnitState>(32);
            var spatialUnits = new List<UnitState>();
            for (int index = 31; index >= 0; index--) spatialUnits.Add(new UnitState { Id = index, Alive = true, Position = new Vector2(index % 4, index / 4), Definition = catalog.Enemies[index % catalog.Enemies.Length], Faction = index % 3 == 0 ? Faction.Companion : Faction.Enemy });
            spatial.Rebuild(spatialUnits);
            spatial.Query(Vector2.one * -10, Vector2.one * 10, neighbours);
            Assert(neighbours.Count == 32 && neighbours.Select(unit => unit.Id).SequenceEqual(Enumerable.Range(0, 32)), "Pooled spatial queries retain deterministic identity order");
            for (int index = 0; index < 100; index++) spatial.Query(Vector2.one * -10, Vector2.one * 10, neighbours);
            long queryAllocation = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1000; index++) spatial.Query(Vector2.one * -10, Vector2.one * 10, neighbours);
            long queryBytes = GC.GetAllocatedBytesForCurrentThread() - queryAllocation;
            long counterProbe = GC.GetAllocatedBytesForCurrentThread();
            var allocationProbe = new byte[1024]; GC.KeepAlive(allocationProbe);
            if (GC.GetAllocatedBytesForCurrentThread() > counterProbe) Assert(queryBytes == 0, "Warm spatial query sorting allocates no managed comparer wrappers");
            else notes.Add("INFO Unity managed allocation counter unavailable; use separate installed-Mono probe, not a zero-allocation claim");
            int[] heavyIdentities = neighbours.Where(unit => unit.Faction == Faction.Enemy && (unit.Definition.IsBoss || unit.Definition.BodyHeight >= 1.2f)).Select(unit => unit.Id).ToArray();
            spatial.Query(Vector2.one * -10, Vector2.one * 10, neighbours, true);
            Assert(neighbours.Select(unit => unit.Id).SequenceEqual(heavyIdentities), "Heavy-only spatial query equals full-query filtered membership and order");
            spatialUnits[0].Position = Vector2.one * 40; spatial.Move(spatialUnits[0]);
            spatial.Query(Vector2.one * -10, Vector2.one * 10, neighbours);
            Assert(neighbours.Count == 31 && neighbours.All(unit => unit.Id != 31), "Moving spatial member leaves old query without changing identities");
            spatialUnits[1].Alive = false; spatial.Rebuild(spatialUnits);
            spatial.Query(Vector2.one * -10, Vector2.one * 10, neighbours);
            Assert(neighbours.Count == 30 && neighbours.All(unit => unit.Id != 30), "Spatial rebuild excludes defeated actors");
            foreach (EnsembleClip clip in art.Merchants.Concat(art.Weapons))
            {
                Assert(clip.Frames.Length >= 4 && clip.ReferenceWidth > 0, "Complete painted clip " + clip.Id);
                foreach (Sprite frame in clip.Frames) Assert(frame != null && frame.rect.width > 8 && frame.rect.height > 8, "Nonempty painted pose " + frame.name);
                Assert(clip.At(-1) == clip.Frames[0] && clip.At(2) == clip.Frames[clip.Frames.Length - 1], "Clip endpoints clamp " + clip.Id);
            }
            AnimationDetailBudget.SetLoad(0, 0);
            foreach (UnitDefinition unit in catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).Concat(catalog.Bosses).Concat(new[] { NightmareAssets.Current?.Customer, OutsideStageAssets.Current?.Boss }).Where(unit => unit != null).Distinct())
            {
                string before = JsonUtility.ToJson(unit);
                var state = new UnitState { Id = 71, Definition = unit, Health = unit.Health, AttackStarted = -100, LastHit = -100, Faction = Faction.Companion };
                Vector4 motion = EnsembleMotion.Surface(state, 10, false);
                Assert(float.IsFinite(motion.x) && Mathf.Abs(motion.z) <= .0061f, "Bounded independent surface acting " + unit.Id);
                Assert(EnsembleMotion.Surface(state, 10, true) == Vector4.zero, "Reduced surface stable " + unit.Id);
                Assert(EnsembleMotion.Goodbye(unit, 1.2f, 0, true) == Vector3.zero, "Reduced farewell stable " + unit.Id);
                Assert(EnsembleMotion.Goodbye(unit, 9, 0, false) == Vector3.zero, "Farewell fully settles " + unit.Id);
                state.PendingAttack = true;
                Assert(EnsembleMotion.Surface(state, 10, false) == Vector4.zero, "Real attack takes priority " + unit.Id);
                Assert(JsonUtility.ToJson(unit) == before, "Presentation preserves gameplay source " + unit.Id);
                if (catalog.Enemies.Contains(unit)) Assert(EnemyVariantVisual.Family(state, 10) >= -1, "Intrinsic enemy effect works before variant assignment " + unit.Id);
                coverage.Add("| " + unit.Id + " | " + EnsembleMotion.For(unit) + " | " + (unit.AuthoredMotion != null ? "Authored combat set" : unit.AnimationFrames.Length + " frames") + " | " + (CharacterActingLibrary.Current?.Find(unit.Id) != null ? "Character acting" : "Existing art + procedural surface") + " |");
            }
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                Assert(EnsembleMotion.WeaponSurface(weapon, -1, .2f, false) == Vector4.zero, "No weapon motion before event " + weapon.Id);
                Assert(EnsembleMotion.WeaponSurface(weapon, 10, .2f, false) == Vector4.zero, "Weapon mechanism settles " + weapon.Id);
                Assert(EnsembleMotion.WeaponSurface(weapon, .1f, .2f, true) == Vector4.zero, "Reduced weapon mechanism stable " + weapon.Id);
            }
            foreach (EnsembleGesture kind in Enum.GetValues(typeof(EnsembleGesture)))
            {
                Assert(SpeciesDefeatPerformance.Dissolve(kind, .5f, .3f, 0, true) == Vector4.zero, "Reduced dissolution stable " + kind);
                Assert(SpeciesDefeatPerformance.Dissolve(kind, .5f, .3f, 0, false) != SpeciesDefeatPerformance.Dissolve(kind, .5f, .3f, 1, false), "Evolution changes death response " + kind);
            }
            AnimationDetailBudget.SetLoad(500, 700); Assert(AnimationDetailBudget.Level == 2 && !AnimationDetailBudget.SurfaceDetail, "Crowd budget sheds ornament before gameplay");
            AnimationDetailBudget.SetLoad(0, 0); Assert(AnimationDetailBudget.Level == 0, "Crowd budget resets");
            Directory.CreateDirectory("PreviewExports/Ensemble"); File.WriteAllLines("PreviewExports/Ensemble/Focused_Checks.txt", notes); File.WriteAllLines("PreviewExports/Ensemble/Coverage.md", coverage);
            Debug.Log("ENSEMBLE_CHECKS " + notes.Count + " PASS");
        }
        public static void Gate() { AnimationPolishBuilder.Gate(); }
    }
}
