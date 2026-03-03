using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K3FinalBuilder
    {
        [MenuItem("Arcane Brigade/K3/Apply final loot proc slice and build")]
        public static void ApplyAndBuild() { Apply(); K3FinalChecks.Run(); FullRunBuilder.BuildWindows(); }
        public static void Apply()
        {
            K32AltarBuilder.Apply();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            // only fill missing loot art: later passes replaced some of it with painted sprites (e.g. the living chest)
            if (catalog.EggSprite == null) catalog.EggSprite = K3LootArt.Create(LootKind.Egg);
            if (catalog.HeartSprite == null) catalog.HeartSprite = K3LootArt.Create(LootKind.Heart);
            if (catalog.ChestSprite == null) catalog.ChestSprite = K3LootArt.Create(LootKind.Chest);
            if (catalog.LootRevision > 0)
            {
                foreach (ItemDefinition item in catalog.Items)
                    if (item.Icon == null && (item.Id == "fortune-egg" || item.Id == "coin-salve" || item.Id == "altar-ember" || item.Id == "treasure-whisper" || item.Id == "chest-thunder")) item.Icon = item.Id == "fortune-egg" ? catalog.EggSprite : item.Id == "coin-salve" || item.Id == "altar-ember" ? catalog.HeartSprite : catalog.ChestSprite;
                EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); return;
            }
            var items = new List<ItemDefinition>(catalog.Items)
            {
                Item("fortune-egg", "Fortune Egg", "Luck +0.2: more egg/heart loot; no hidden damage bonus.", catalog.EggSprite, ItemEffect.Luck, 0.2f, 24, 2),
                Item("treasure-whisper", "Treasure Whisper", "Treasure Detection +0.35: chests appear sooner; walk to open.", catalog.ChestSprite, ItemEffect.TreasureDetection, 0.35f, 30, 2),
                Item("coin-salve", "Coin Salve", "Coin collected: heal 0.5 HP per stack, once per root, cooldown 0.2s.", catalog.HeartSprite, ItemEffect.Heal, 0, 28, 2),
                Item("chest-thunder", "Chest Thunder", "Chest opened: damage pulse 14 per stack, radius 3, cooldown 1s. Cannot trigger another proc.", catalog.ChestSprite, ItemEffect.Damage, 0, 42, 4),
                Item("altar-ember", "Altar Ember", "Altar exclusive: +18% Champion damage, -3% team movement speed.", catalog.HeartSprite, ItemEffect.ChampionDamage, 0.18f, 36, 4)
            };
            ItemDefinition exclusive = items[items.Count - 1];
            exclusive.Pools = ShopPool.Eggs; exclusive.AltarCurrency = ShopCurrency.Health; exclusive.AltarPrice = 10;
            exclusive.Modifiers = new[] { new StatModifier { Effect = ItemEffect.ChampionDamage, Scope = ModifierScope.Champion, Value = 0.18f }, new StatModifier { Effect = ItemEffect.MoveSpeed, Scope = ModifierScope.Team, Value = -0.03f } };
            catalog.Items = items.ToArray();
            catalog.ProcRules = new[]
            {
                new ProcRule { Id = "proc-coin-salve", ItemId = "coin-salve", Trigger = ProcTrigger.CoinCollected, Action = ProcAction.HealChampion, Power = 0.5f, Cooldown = 0.2f },
                new ProcRule { Id = "proc-chest-thunder", ItemId = "chest-thunder", Trigger = ProcTrigger.ChestOpened, Action = ProcAction.DamagePulse, Power = 14, Radius = 3, Cooldown = 1 }
            };
            var links = new List<BuildLinkRule>(catalog.BuildLinks) { new BuildLinkRule { Id = "engine-loot", Name = "KHO BÁU THÌ THẦM", ItemIds = new[] { "treasure-whisper", "chest-thunder" }, Description = "Detection sinh rương sớm hơn; chest pulse dùng event mở rương thật. Proc không tự chain." } };
            catalog.BuildLinks = links.ToArray();
            catalog.LootRevision = 1;
            RunModifierRules.Validate(catalog);
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
        }
        private static ItemDefinition Item(string id, string name, string description, Sprite icon, ItemEffect effect, float value, int price, int wave) => new ItemDefinition { Id = id, Name = name, Description = description, Icon = icon, Effect = effect, Value = value, Price = price, UnlockWave = wave, Rarity = ItemRarity.Rare, ShopWeight = 25, Pools = ShopPool.Both, AltarPrice = 2 };
    }
}
