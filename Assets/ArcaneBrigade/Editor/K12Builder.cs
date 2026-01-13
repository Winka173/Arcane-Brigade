using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K12Builder
    {
        public static void Build()
        {
            Directory.CreateDirectory("PreviewExports/K12");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            art.DreamGardenProps = K11ArtBuilder.Import("DreamGardenProps_v1", "K12"); EditorUtility.SetDirty(art);
            AddBrute(catalog, "FurnaceToad", "Furnace Toad", 4, true);
            AddBrute(catalog, "WardrobeCrab", "Wardrobe Crab", 5, false);
            foreach (UnitDefinition unit in catalog.Companions)
            {
                unit.SpecialRecruit = unit.Id == "EclipseEgg" || unit.Id == "PillowOracle";
                EditorUtility.SetDirty(unit);
            }
            AddRelics(catalog);
            K11EnglishBuilder.Apply(); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            K12Checks.Run(); K12PolishChecks.Run(); MarketPolishChecks.Run(); K11Checks.Run();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K12/Windows_Build.txt", true);
            Debug.Log("K12_CORE_CANDIDATE_BUILT");
        }

        private static void AddBrute(RunCatalog catalog, string id, string name, int wave, bool ranged)
        {
            string path = "Assets/ArcaneBrigade/Data/Run/" + id + ".asset";
            UnitDefinition unit = AssetDatabase.LoadAssetAtPath<UnitDefinition>(path);
            if (unit == null) { unit = ScriptableObject.CreateInstance<UnitDefinition>(); AssetDatabase.CreateAsset(unit, path); }
            UnitDefinition template = catalog.Enemies.First(enemy => (enemy.Weapon.Kind == WeaponKind.Projectile) == ranged);
            unit.Id = id; unit.DisplayName = name; unit.AnimationFrames = K11ArtBuilder.Import(id + "_v1", "K12"); unit.Body = unit.AnimationFrames[0];
            unit.Weapon = template.Weapon; unit.Health = ranged ? 180 : 240; unit.Speed = ranged ? 1.8f : 2.1f;
            unit.BodyHeight = ranged ? 1.65f : 1.9f; unit.Radius = ranged ? 0.48f : 0.55f;
            unit.ContactDamage = 8; unit.ContactInterval = 0.9f; unit.UnlockWave = wave; unit.KnockbackResistance = ranged ? 0.65f : 0.75f;
            unit.Description = ranged ? "Heavy spitter · resists knockback" : "Heavy biter · resists knockback";
            unit.Material = ranged ? BodyMaterial.Wax : BodyMaterial.Wood;
            unit.Tint = Color.white;
            if (!catalog.Enemies.Contains(unit)) catalog.Enemies = catalog.Enemies.Concat(new[] { unit }).ToArray();
            EditorUtility.SetDirty(unit);
        }
        public static void Balance()
        {
            K7BalanceDiagnostics.Run();
            File.Copy("PreviewExports/K7/Movement_Bot_Balance.csv", "PreviewExports/K12/Movement_Bot_Balance.csv", true);
        }
        public static void FinalizeCandidate()
        {
            K12Checks.Run(); K12PolishChecks.Run(); MarketPolishChecks.Run(); K11Checks.Run();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K12/Windows_Build.txt", true);
            Debug.Log("K12_POLISH_FINAL_CANDIDATE_BUILT");
        }
        private static void AddRelics(RunCatalog catalog)
        {
            var items = catalog.Items.ToList();
            void Add(string id, string name, ItemEffect effect, float value, ItemRarity rarity, string description, string icon)
            {
                ItemDefinition item = items.Find(candidate => candidate.Id == id);
                if (item == null) { item = new ItemDefinition { Id = id }; items.Add(item); }
                item.Name = name; item.Description = description; item.Effect = effect; item.Value = value;
                item.Modifiers = new[] { new StatModifier { Effect = effect, Value = value, Scope = RunModifierRules.DefaultScope(effect) }, new StatModifier { Effect = ItemEffect.MoveSpeed, Value = -0.02f, Scope = ModifierScope.Champion } };
                item.Rarity = rarity; item.Price = 24; item.UnlockWave = 3; item.ShopWeight = 45; item.Pools = ShopPool.Both;
                item.AltarPrice = 3; item.MaximumStacks = 100000; item.Icon = catalog.Weapon(icon).Sprite;
            }
            Add("long-pocket", "Long Pocket", ItemEffect.ChestReach, 0.35f, ItemRarity.Uncommon, "Chest reach +35%. Champion speed -2%.", "TapeWhip");
            Add("velvet-first-aid", "Velvet First Aid", ItemEffect.TreasureDetection, 0.1f, ItemRarity.Rare, "Chest: heal4 HP/stack (2s CD). More chests; speed -2%.", "TeabagFlail");
            Add("brass-avalanche", "Brass Avalanche", ItemEffect.Knockback, 0.1f, ItemRarity.Mythic, "Kill: nearby pulse3 DMG/stack (2s CD). Knockback +10%; speed -2%.", "MaracaHammer");
            catalog.Items = items.ToArray();
            var rules = catalog.ProcRules.ToList();
            foreach (string id in new[] { "velvet-first-aid", "brass-avalanche" })
            {
                ProcRule rule = rules.Find(candidate => candidate.ItemId == id);
                if (rule == null) { rule = new ProcRule { Id = id, ItemId = id }; rules.Add(rule); }
                rule.Trigger = id == "velvet-first-aid" ? ProcTrigger.ChestOpened : ProcTrigger.EnemyKilled;
                rule.Action = id == "velvet-first-aid" ? ProcAction.HealChampion : ProcAction.DamagePulse;
                rule.Power = id == "velvet-first-aid" ? 4 : 3; rule.Radius = 1.8f; rule.Cooldown = 2;
            }
            catalog.ProcRules = rules.ToArray();
        }
    }
}
