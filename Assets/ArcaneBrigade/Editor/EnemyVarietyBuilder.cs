using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Puts the enemy variety art in (Docs/Playtest_Art_Prompts/23_Enemy_Variety.txt, 24/10/2026): the ten repainted
    /// look-alikes get their new bodies and motion (normal and mutated); the eight new enemies are made from a template with
    /// their behaviour; the four map elites go into RunCatalog.MapElites. Sheets are Art/Playtest/MapEnemies/Enemy_&lt;Id&gt;.png,
    /// 4×4 as the map enemies; a missing sheet is skipped, so the set can arrive in parts.</summary>
    public static class EnemyVarietyBuilder
    {
        private const string Art = "Assets/ArcaneBrigade/Art/Playtest/MapEnemies/";
        public static readonly string[] Repaints = { "MoonMagnet", "ClockBat", "RuneWisp", "InkMoth", "SirenSeal", "SleepMimic", "SporeBishop", "BroomSkater", "CurtainWisp", "HushMite" };

        private struct Spec
        {
            public string Id, Name, Template, MutationName, Summary; public float Height, Health, Speed; public int Unlock; public EnemyBehavior Behavior; public EnemyMutation Ability;
        }
        private static Spec S(string id, string name, string template, float height, float health, float speed, int unlock, EnemyBehavior behavior, EnemyMutation ability, string mutation, string summary)
            => new Spec { Id = id, Name = name, Template = template, Height = height, Health = health, Speed = speed, Unlock = unlock, Behavior = behavior, Ability = ability, MutationName = mutation, Summary = summary };

        private static readonly Spec[] NewEnemies =
        {
            S("FireScreenWarden", "Fire Screen Warden", "WardrobeCrab", 1.3f, 160, .85f, 13, EnemyBehavior.Shielder, EnemyMutation.CabinetScissor, "Folding Wall", "Shields the enemies behind it from shots."),
            S("TwinWickLantern", "Twin Wick Lantern", "HushMite", .7f, 60, 1.15f, 14, EnemyBehavior.Splitter, EnemyMutation.MothMolting, "Split Wick", "Breaks into two little lanterns."),
            S("CandleShade", "Candle Shade", "HushMite", .8f, 55, 1.1f, 15, EnemyBehavior.Blinker, EnemyMutation.SootPounce, "Long Shadow", "Sinks into its shadow and rises behind you."),
            S("SpyglassSniper", "Spyglass Sniper", "RuneWisp", 1.0f, 70, .95f, 16, EnemyBehavior.Sniper, EnemyMutation.RuneSpiral, "Twin Lens", "Keeps its distance, aims a line, fires once hard."),
            S("DrawerBroodmother", "Drawer Broodmother", "WardrobeCrab", 1.8f, 300, .7f, 18, EnemyBehavior.Summoner, EnemyMutation.CabinetScissor, "Open Drawers", "Stays back and releases Clip Mites."),
            S("LedgerTortoise", "Ledger Tortoise", "FurnaceToad", 1.5f, 260, .65f, 19, EnemyBehavior.Armored, EnemyMutation.KegRebound, "Iron Binding", "Its ledger shell guards its front; strike from behind."),
            S("BoundBookends", "Bound Bookends", "ChargerRam", 1.0f, 110, 1.0f, 21, EnemyBehavior.Tethered, EnemyMutation.RamBackstep, "Spiked Chain", "Two that must fall together."),
            S("RubberStampTitan", "Rubber Stamp Titan", "SackGolem", 1.9f, 340, .8f, 23, EnemyBehavior.Stomper, EnemyMutation.KegRebound, "Jagged Seal", "Leaps and stamps down where you stand."),
        };
        private static readonly Spec[] Elites =
        {
            S("HeadUsher", "Head Usher", "SofaGlutton", 2.2f, 900, .8f, 99, EnemyBehavior.Standard, EnemyMutation.CabinetScissor, "Opened Seats", "The Waiting Room's elite."),
            S("DeadLetterCourier", "Dead Letter Courier", "SackGolem", 2.1f, 1100, .85f, 99, EnemyBehavior.Hopper, EnemyMutation.KegRebound, "Torn Sack", "The Post Office's elite."),
            S("WickWarden", "Wick Warden", "ChandelierCrawler", 2.2f, 1300, .8f, 99, EnemyBehavior.Shielder, EnemyMutation.BellEruption, "Six Arms", "The Lamp Hall's elite."),
            S("ArchiveSentinel", "Archive Sentinel", "CabinetMimic", 2.3f, 1500, .75f, 99, EnemyBehavior.Armored, EnemyMutation.CabinetScissor, "Drawer Teeth", "The Archive Vault's elite."),
        };

        [MenuItem("Arcane Brigade/Art/Put enemy variety in")]
        public static void Build()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var library = AssetDatabase.LoadAssetAtPath<EnemyEvolutionLibrary>("Assets/ArcaneBrigade/Resources/EnemyEvolutionLibrary.asset");
            var species = new List<EnemyEvolutionEntry>(library.Species ?? new EnemyEvolutionEntry[0]);
            var enemies = new List<UnitDefinition>(catalog.Enemies);
            int repainted = 0, made = 0, elites = 0;

            foreach (string id in Repaints)
            {
                if (!Sheet(id, out Sprite[] frames)) continue;
                UnitDefinition unit = enemies.FirstOrDefault(e => e != null && e.Id == id); if (unit == null) { Debug.Log("ENEMY_VARIETY no unit " + id); continue; }
                AuthoredMotionSet normal = Motion(id, frames.Take(8).ToArray(), false), mutated = Motion(id, frames.Skip(8).ToArray(), true);
                unit.Body = normal.Idle[0]; unit.AuthoredMotion = normal; unit.AnimationFrames = new[] { normal.Idle[0], normal.Idle[1], normal.Walk[0], normal.Walk[1] };
                EditorUtility.SetDirty(unit);
                EnemyEvolutionEntry old = species.FirstOrDefault(e => e != null && e.Id == id);
                species.RemoveAll(e => e != null && e.Id == id);
                species.Add(new EnemyEvolutionEntry { Id = id, MutationName = old?.MutationName ?? "Mutated", Summary = old?.Summary ?? "", Ability = old != null ? old.Ability : EnemyMutation.None, Normal = normal, Mutated = mutated });
                repainted++;
            }
            foreach (Spec s in NewEnemies)
            {
                UnitDefinition unit = Make(s, catalog, species, false); if (unit == null) continue;
                if (!enemies.Contains(unit)) enemies.Add(unit); made++;
            }
            catalog.MapElites ??= new UnitDefinition[4]; if (catalog.MapElites.Length < 4) System.Array.Resize(ref catalog.MapElites, 4);
            for (int m = 0; m < Elites.Length; m++)
            {
                UnitDefinition unit = Make(Elites[m], catalog, species, true); if (unit == null) continue;
                catalog.MapElites[m] = unit; elites++;
            }
            catalog.Enemies = enemies.ToArray(); library.Species = species.ToArray();
            EditorUtility.SetDirty(catalog); EditorUtility.SetDirty(library); EnemyEvolutionLibrary.Invalidate();
            AssetDatabase.SaveAssets();
            Debug.Log("ENEMY_VARIETY repainted " + repainted + ", new " + made + ", elites " + elites);
        }

        private static bool Sheet(string id, out Sprite[] frames)
        {
            frames = null; string path = Art + "Enemy_" + id + ".png";
            if (AssetImporter.GetAtPath(path) == null) return false;
            frames = K20BossAnatomyBuilder.ImportAtlas(path, id + "Evolution", true);
            return frames != null && frames.Length >= 16;
        }

        private static UnitDefinition Make(Spec s, RunCatalog catalog, List<EnemyEvolutionEntry> species, bool elite)
        {
            if (!Sheet(s.Id, out Sprite[] frames)) return null;
            UnitDefinition template = catalog.Enemies.FirstOrDefault(e => e != null && e.Id == s.Template);
            if (template == null) { Debug.Log("ENEMY_VARIETY template " + s.Template + " missing for " + s.Id); return null; }
            AuthoredMotionSet normal = Motion(s.Id, frames.Take(8).ToArray(), false), mutated = Motion(s.Id, frames.Skip(8).ToArray(), true);
            string path = "Assets/ArcaneBrigade/Data/Run/" + s.Id + ".asset";
            UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
            if (unit == null) { unit = ScriptableObject.CreateInstance<UnitDefinition>(); AssetDatabase.CreateAsset(unit, path); }
            EditorUtility.CopySerialized(template, unit);
            unit.name = s.Id; unit.Id = s.Id; unit.DisplayName = s.Name; unit.Description = s.Summary;
            unit.BodyHeight = s.Height; unit.Radius = template.Radius * s.Height / Mathf.Max(.1f, template.BodyHeight); unit.Health = s.Health; unit.Speed = s.Speed; unit.UnlockWave = s.Unlock;
            unit.Behavior = s.Behavior; unit.Body = normal.Idle[0]; unit.AuthoredMotion = normal; unit.AnimationFrames = new[] { normal.Idle[0], normal.Idle[1], normal.Walk[0], normal.Walk[1] };
            unit.EvolutionStage = 0; unit.Mutation = EnemyMutation.None; unit.Tint = Color.white;
            if (s.Behavior == EnemyBehavior.Sniper)
            {
                // a heavy, single shot: the template's ranged weapon with more weight behind it
                WeaponDefinition ranged = template.Weapon != null && template.Weapon.Kind == WeaponKind.Projectile ? template.Weapon : catalog.Enemies.Where(e => e != null && e.Weapon != null && e.Weapon.Kind == WeaponKind.Projectile).Select(e => e.Weapon).FirstOrDefault();
                if (ranged != null)
                {
                    string wpath = "Assets/ArcaneBrigade/Data/Run/" + s.Id + "Shot.asset";
                    WeaponDefinition shot = AssetDatabase.LoadAssetAtPath<WeaponDefinition>(wpath);
                    if (shot == null) { shot = ScriptableObject.CreateInstance<WeaponDefinition>(); AssetDatabase.CreateAsset(shot, wpath); }
                    EditorUtility.CopySerialized(ranged, shot); shot.name = s.Id + "Shot"; shot.Id = s.Id + "Shot"; shot.Damage = ranged.Damage * 2.2f; shot.Range = Mathf.Max(ranged.Range, 7); shot.ProjectileSpeed = ranged.ProjectileSpeed * 1.6f;
                    EditorUtility.SetDirty(shot); unit.Weapon = shot;
                }
            }
            if (elite) { unit.KnockbackResistance = Mathf.Max(unit.KnockbackResistance, .85f); unit.BodyScale = Mathf.Max(1, unit.BodyScale); }
            EditorUtility.SetDirty(unit);
            species.RemoveAll(e => e != null && e.Id == s.Id);
            species.Add(new EnemyEvolutionEntry { Id = s.Id, MutationName = s.MutationName, Summary = s.Summary, Ability = s.Ability, Normal = normal, Mutated = mutated });
            return unit;
        }

        private static AuthoredMotionSet Motion(string id, Sprite[] frames, bool mutated)
        {
            string path = "Assets/ArcaneBrigade/Resources/" + id + (mutated ? "MutatedMotion" : "Motion") + ".asset";
            AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(path);
            if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, path); }
            motion.Idle = frames.Take(2).ToArray(); motion.Walk = new[] { frames[2], frames[3], frames[2], frames[3] };
            motion.Windup = frames.Skip(4).Take(2).ToArray(); motion.Release = new[] { frames[6], frames[6] }; motion.Recovery = new[] { frames[7], frames[0] };
            EditorUtility.SetDirty(motion); return motion;
        }
    }
}
