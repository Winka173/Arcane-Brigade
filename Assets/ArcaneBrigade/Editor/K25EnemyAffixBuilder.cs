using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K25EnemyAffixBuilder
    {
        public static readonly string[] Ids = { "SmokeWraith", "BlueFlameFiend", "RedFlameBerserker", "TwinFlameFurnace", "AshSwallower", "LostLanternSoul", "SmokingCoffin", "EmberProcession" };
        public static readonly int[] Unlocks = { 6, 7, 8, 10, 9, 10, 11, 12 };
        private static readonly string[] Names = { "Smoke Wraith", "Blue-Flame Fiend", "Red-Flame Berserker", "Twin-Flame Furnace", "Ash Swallower", "Lost Lantern Soul", "Smoking Coffin", "Ember Procession" };
        private static readonly string[] Copy = { "Fades sideways. Its shadow lashes later.", "Spits slow curved ghost flames.", "Charges, then rests. Evolved: leaves hot coals.", "Alternates ghost flames and a furnace rush.", "Consumes fresh remains to strengthen its next spit.", "Soul spits; shell jumps. They share one life.", "Opens its lid to release short-lived ash creatures.", "Leads a trio. Its followers falter when it dies." };
        private static T Resource<T>(string id) where T : ScriptableObject
        {
            string path = "Assets/ArcaneBrigade/Resources/" + id + ".asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        public static void Build()
        {
            K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/K24/OutsideStage_v1.png", "OutsideStage", 4, 4, Enumerable.Range(0, 16).ToArray(), new[] { 0, 296, 561, 740, 1024 }, true, true);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            EnemyAffixLibrary library = Resource<EnemyAffixLibrary>("EnemyAffixLibrary");
            library.Species = new SpecialEnemyArt[8];
            var enemies = catalog.Enemies.Where(enemy => enemy.SpecialEnemy == SpecialEnemyKind.None && Array.IndexOf(Ids, enemy.Id) < 0).ToList();
            for (int index = 0; index < Ids.Length; index++)
            {
                string id = Ids[index]; string path = "Assets/ArcaneBrigade/Art/K25/" + id + "_v1.png";
                Sprite[] frames;
                if (id == "RedFlameBerserker" || id == "EmberProcession") frames = K25AtlasImporter.Import(path, id, 8, 4, Enumerable.Range(0, 8).Concat(Enumerable.Range(16, 8)).ToArray());
                else if (id == "LostLanternSoul") frames = K25AtlasImporter.Import(path, id, 5, 4, new[] { 0, 1, 2, 3, 5, 6, 8, 9, 10, 11, 12, 13, 15, 16, 18, 19 }, new[] { 0, 312, 592, 845, 1024 });
                else frames = K20BossAnatomyBuilder.ImportAtlas(path, id, true);
                AuthoredMotionSet normal = Motion(id + "Motion", frames.Take(8).ToArray()), evolved = Motion(id + "EvolvedMotion", frames.Skip(8).ToArray());
                string assetPath = "Assets/ArcaneBrigade/Data/Run/" + id + ".asset";
                UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(assetPath);
                if (unit == null) { unit = ScriptableObject.CreateInstance<UnitDefinition>(); AssetDatabase.CreateAsset(unit, assetPath); }
                WeaponDefinition weapon = Resource<WeaponDefinition>(id + "BodyAttack");
                weapon.Id = id + "BodyAttack"; weapon.Kind = WeaponKind.Contact; weapon.Damage = 8;
                weapon.Range = 1; weapon.Windup = 1; weapon.Cooldown = 2; weapon.Recovery = 0.6f; weapon.PushDistance = 0;
                weapon.Sprite = null; weapon.ProjectileSprite = null;
                unit.Id = id; unit.DisplayName = Names[index]; unit.SpecialEnemy = (SpecialEnemyKind)(index + 1);
                unit.AuthoredMotion = normal; unit.Body = normal.Idle[0]; unit.AnimationFrames = new[] { frames[0], frames[1], frames[2], frames[3] };
                unit.Weapon = weapon; unit.Health = index == 6 ? 130 : index == 3 ? 95 : 48 + index * 5;
                unit.BodyHeight = index == 6 ? 1.3f : index == 3 ? 1.1f : 0.86f;
                unit.Speed = index == 6 ? 0.65f : index == 4 ? 0.85f : 1.15f;
                unit.Radius = index == 6 ? 0.42f : 0.27f; unit.ContactDamage = 6; unit.ContactInterval = 1.05f;
                unit.UnlockWave = Unlocks[index]; unit.Tint = Color.white; unit.Description = Copy[index];
                unit.Behavior = EnemyBehavior.Standard; unit.Material = index == 6 ? BodyMaterial.Wood : index == 0 || index == 4 ? BodyMaterial.Fabric : BodyMaterial.Metal;
                unit.KnockbackResistance = index == 6 ? 0.55f : 0.2f; unit.Tags = new string[0]; unit.EvolutionStage = 0; unit.Mutation = EnemyMutation.None;
                library.Species[index] = new SpecialEnemyArt { Id = id, Normal = normal, Evolved = evolved };
                enemies.Add(unit); EditorUtility.SetDirty(unit); EditorUtility.SetDirty(weapon);
            }
            library.Effects = K20BossAnatomyBuilder.ImportAtlas("Assets/ArcaneBrigade/Art/K25/EnemyAffixEffects_v1.png", "EnemyAffixEffect");
            catalog.Enemies = enemies.ToArray(); EditorUtility.SetDirty(catalog); EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets(); EnemyAffixLibrary.Invalidate(); Verify();
        }
        private static AuthoredMotionSet Motion(string id, Sprite[] frames)
        {
            AuthoredMotionSet motion = Resource<AuthoredMotionSet>(id);
            motion.Idle = frames.Take(2).ToArray(); motion.Walk = new[] { frames[2], frames[3], frames[2], frames[3] };
            motion.Windup = frames.Skip(4).Take(2).ToArray(); motion.Release = new[] { frames[6], frames[6] }; motion.Recovery = new[] { frames[7], frames[0] };
            EditorUtility.SetDirty(motion); return motion;
        }
        public static void Verify() { Directory.CreateDirectory("PreviewExports/K25"); K25EnemyAffixChecks.Run(); }
        public static void VerifyAndBuild() { Verify(); K24OutsideStageBuilder.Verify(); FullRunBuilder.BuildWindows(); File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K25/Windows_Build.txt", true); }
        public static void Regressions() => K24OutsideStageBuilder.Regressions();
        public static void Campaign() => K17CampaignDiagnostics.RunInto("PreviewExports/K25");
    }
}
