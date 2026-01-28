using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K24OutsideStageBuilder
    {
        private const string Root = "Assets/ArcaneBrigade/Resources/";
        private static T Asset<T>(string name) where T : ScriptableObject
        {
            string path = Root + name + ".asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset, path); return asset;
        }
        public static void Build()
        {
            OutsideStageAssets assets = Asset<OutsideStageAssets>("OutsideStageAssets");
            assets.BossFrames = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/K24/OutsideStage_v1.png", "OutsideStage", 4, 4, Enumerable.Range(0, 16).ToArray(), new[] { 0, 296, 561, 740, 1024 }, true, true);
            assets.RewardFrames = K20BossAnatomyBuilder.ImportAtlas("Assets/ArcaneBrigade/Art/K24/OutsideRewards_v1.png", "OutsideReward", true);
            Sprite[] frames = assets.BossFrames;
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            UnitDefinition boss = Asset<UnitDefinition>("OutsideStageKeeper");
            EditorUtility.CopySerialized(catalog.Bosses[0], boss);
            boss.Id = "OutsideStageKeeper"; boss.DisplayName = "The Outside Keeper";
            boss.Body = frames[0]; boss.AnimationFrames = frames.Take(4).ToArray();
            boss.AuthoredMotion = Motion("OutsideStageMotion", frames.Take(4).ToArray());
            boss.Health = 16000; boss.BodyHeight = 2.15f; boss.Radius = 0.85f; boss.Speed = 0.38f;
            boss.ContactDamage = 12; boss.ContactInterval = 1.2f; boss.BossPattern = 10; boss.IsBoss = true;
            boss.KnockbackResistance = 0.95f; boss.Description = "The hands that carried you belong to this thing.";
            WeaponDefinition weapon = Asset<WeaponDefinition>("OutsideStageWeapon"); EditorUtility.CopySerialized(boss.Weapon, weapon);
            weapon.Id = "OutsideStageWeapon"; weapon.Damage = 14; weapon.Kind = WeaponKind.Contact; boss.Weapon = weapon;
            assets.Boss = boss;
            assets.Anatomy = new BossAnatomyEntry { BossId = boss.Id, Organs = new[] {
                Organ("carrying-hand", "The Carrying Hand", BossOrganRole.Lash | BossOrganRole.Props, new Vector2(-0.55f, -0.12f), 0.58f, frames, 4),
                Organ("living-tongue", "The Living Carpet", BossOrganRole.Lash | BossOrganRole.Vacuum, new Vector2(0.48f, -0.22f), 0.52f, frames, 8),
                Organ("keyhole-eye", "The Keyhole Eye", BossOrganRole.Pool | BossOrganRole.Tempo, new Vector2(-0.08f, 0.26f), 0.35f, frames, 12) } };
            var items = catalog.Items.Where(item => !item.Id.StartsWith("Rainbow") && item.Id != "CrimsonSeal" && item.Id != "CrimsonSpring").ToList();
            items.Add(Relic("RainbowCrown", "Borrowed Crown", "Team damage +35%. One extra projectile.", 0, ItemRarity.Rainbow, ItemEffect.Damage, 0.35f, ItemEffect.Projectiles, 1, assets));
            items.Add(Relic("RainbowHeart", "Clockwork Heart", "Max HP +60. Heal 1 HP per second.", 1, ItemRarity.Rainbow, ItemEffect.Health, 60, ItemEffect.Heal, 1, assets));
            items.Add(Relic("RainbowThread", "Living Thread", "Melee damage +60%. Knockback +35%.", 2, ItemRarity.Rainbow, ItemEffect.MeleeDamage, 0.6f, ItemEffect.Knockback, 0.35f, assets));
            items.Add(Relic("RainbowPrism", "Impossible Prism", "Shots split twice. Ranged damage +25%.", 3, ItemRarity.Rainbow, ItemEffect.BulletSplit, 2, ItemEffect.RangedDamage, 0.25f, assets));
            items.Add(Relic("CrimsonSeal", "Crimson Seal", "Personal damage +45%. Collision damage +12.", 4, ItemRarity.Crimson, ItemEffect.ChampionDamage, 0.45f, ItemEffect.CollisionDamage, 12, assets));
            items.Add(Relic("CrimsonSpring", "Crimson Spring", "Attack speed +25%. Melee reach +25%.", 5, ItemRarity.Crimson, ItemEffect.AttackSpeed, 0.25f, ItemEffect.AttackRange, 0.25f, assets));
            catalog.Items = items.ToArray();
            var allies = catalog.Companions.Where(unit => unit.Id != "RainbowTailor" && unit.Id != "RainbowBell").ToList();
            allies.Add(Ally(catalog, assets, "RainbowTailor", "The Impossible Tailor", 8, "MoonseedOrb"));
            allies.Add(Ally(catalog, assets, "RainbowBell", "The Last Bell", 12, "SpringKnocker"));
            catalog.Companions = allies.ToArray();
            UnitDefinition crimson = catalog.Unit("EclipseEgg");
            crimson.Rarity = ItemRarity.Crimson; crimson.Price = Mathf.Max(80, crimson.Price); crimson.UnlockWave = Mathf.Max(10, crimson.UnlockWave);
            EditorUtility.SetDirty(crimson);
            foreach (UnityEngine.Object asset in new UnityEngine.Object[] { assets, boss, weapon, catalog }) EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets(); BossAnatomy.Invalidate(); Verify();
        }
        private static AuthoredMotionSet Motion(string id, Sprite[] frames)
        {
            AuthoredMotionSet motion = Asset<AuthoredMotionSet>(id);
            motion.Idle = new[] { frames[0], frames[1] }; motion.Walk = new[] { frames[0], frames[2], frames[0], frames[2] };
            motion.Windup = new[] { frames[1], frames[2] }; motion.Release = new[] { frames[3], frames[3] }; motion.Recovery = new[] { frames[2], frames[0] };
            EditorUtility.SetDirty(motion); return motion;
        }
        private static BossOrganDefinition Organ(string id, string name, BossOrganRole role, Vector2 offset, float height, Sprite[] frames, int start)
            => new BossOrganDefinition { Id = id, Name = name, Role = role, Offset = offset, Height = height, Durability = 0.14f, Rest = frames[start], Charged = frames[start + 1], Release = frames[start + 2], Scar = frames[start + 3], BreakSummary = "Disables this organ's attack patterns." };
        private static ItemDefinition Relic(string id, string name, string description, int frame, ItemRarity rarity, ItemEffect first, float firstValue, ItemEffect second, float secondValue, OutsideStageAssets assets)
            => new ItemDefinition { Id = id, Name = name, Description = description, Icon = assets.RewardFrames[frame], Price = rarity == ItemRarity.Crimson ? 75 : 200, AltarPrice = 15, Rarity = rarity, Stacking = StackPolicy.Unique, MaximumStacks = 1, ShopWeight = rarity == ItemRarity.Rainbow ? 0 : 1, UnlockWave = rarity == ItemRarity.Rainbow ? int.MaxValue : 10, Modifiers = new[] { new StatModifier { Effect = first, Scope = RunModifierRules.DefaultScope(first), Value = firstValue }, new StatModifier { Effect = second, Scope = RunModifierRules.DefaultScope(second), Value = secondValue } } };
        private static UnitDefinition Ally(RunCatalog catalog, OutsideStageAssets assets, string id, string name, int start, string weaponId)
        {
            UnitDefinition ally = Asset<UnitDefinition>(id); EditorUtility.CopySerialized(catalog.Companions[0], ally);
            ally.Id = id; ally.DisplayName = name; ally.Rarity = ItemRarity.Rainbow; ally.SpecialRecruit = true; ally.UnlockWave = int.MaxValue;
            ally.Body = assets.RewardFrames[start]; ally.AnimationFrames = assets.RewardFrames.Skip(start).Take(4).ToArray(); ally.AuthoredMotion = Motion(id + "Motion", ally.AnimationFrames);
            ally.Weapon = catalog.Weapon(weaponId); ally.BodyHeight = 0.9f;
            ally.Passive = PassiveKind.None; ally.PassiveStrength = 0; ally.Tint = Color.white;
            ally.Material = start == 8 ? BodyMaterial.Fabric : BodyMaterial.Metal;
            BondDefinition bond = catalog.Bonds.OrderBy(candidate => catalog.Companions.Count(unit => unit.Rarity != ItemRarity.Rainbow && Array.IndexOf(unit.Tags, candidate.Id) >= 0)).ElementAt(start == 8 ? 0 : 1);
            if (catalog.Companions.Count(unit => unit.Rarity != ItemRarity.Rainbow && Array.IndexOf(unit.Tags, bond.Id) >= 0) >= 6) throw new InvalidOperationException("Prismatic ally needs an uncrowded bond");
            ally.Tags = new[] { bond.Id };
            ally.Description = start == 8 ? "Splitting dream orbs. Team ranged damage +15%." : "Heavy spring swings. Team melee damage +15%.";
            ally.ChampionModifiers = new[] { new StatModifier { Effect = start == 8 ? ItemEffect.RangedDamage : ItemEffect.MeleeDamage, Scope = ModifierScope.Team, Value = 0.15f } };
            EditorUtility.SetDirty(ally); return ally;
        }
        public static void Verify()
        {
            Directory.CreateDirectory("PreviewExports/K24");
            K23MergeChecks.Run(); K23CooperationChecks.Run(); K24OutsideStageChecks.Run();
        }
        public static void VerifyAndBuild() { Verify(); FullRunBuilder.BuildWindows(); File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K24/Windows_Build.txt", true); }
        public static void Campaign() => K17CampaignDiagnostics.RunInto("PreviewExports/K24");
        public static void Regressions() { K22EnemyEvolutionBuilder.Verify(); K21BossEncounterBuilder.Verify(); K21BossEncounterBuilder.Regressions(); }
    }
}
