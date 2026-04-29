using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Wise's shop (the original Dream Seeker; owner 23/10/2026): the other Dream Seekers are sold as their memory.
    /// Bought, a hero's gifts become Wise's - its stat modifiers and its innate bonuses, positive ones +50% - while enemies
    /// grow tougher. Each is a code-made unique relic ("WiseHero_&lt;hero id&gt;") so the shop, the card, the stash and the stat
    /// sums treat it like any relic; RunCatalog.Item finds it here.</summary>
    public static class WiseHeroItems
    {
        public const string Prefix = "WiseHero_";
        public const float GiftBoost = 1.5f;
        /// <summary>Heroes whose whole point is a rule Wise cannot borrow (not attacking, royal health, rooting, solo, gold as
        /// health...) are never offered.</summary>
        private static readonly HashSet<string> Excluded = new HashSet<string>
            { "Wise", "Card King", "Card Queen", "Capped Gallant", "Omnom", "Periwinky", "Baggy", "Clementie", "Sassy Senior", "Anby", "Billy" };
        private static readonly Dictionary<string, ItemDefinition> cache = new Dictionary<string, ItemDefinition>();

        public static bool Offerable(UnitDefinition hero) => hero != null && !string.IsNullOrEmpty(hero.ReferenceHero) && !Excluded.Contains(hero.ReferenceHero);
        public static string IdFor(UnitDefinition hero) => Prefix + hero.Id;
        public static bool Is(string id) => id != null && id.StartsWith(Prefix, System.StringComparison.Ordinal);

        public static ItemDefinition For(RunCatalog catalog, string id)
        {
            if (!Is(id) || catalog == null) return null;
            if (cache.TryGetValue(id, out ItemDefinition found) && found != null) return found;
            string heroId = id.Substring(Prefix.Length);
            UnitDefinition hero = null;
            foreach (UnitDefinition unit in catalog.Champions ?? new UnitDefinition[0]) if (unit != null && unit.Id == heroId) hero = unit;
            if (hero == null) return null;
            var modifiers = new List<StatModifier>();
            foreach (StatModifier m in hero.ChampionModifiers ?? new StatModifier[0])
                modifiers.Add(new StatModifier { Effect = m.Effect, Scope = m.Scope, Value = m.Value > 0 ? m.Value * GiftBoost : m.Value });
            var item = new ItemDefinition
            {
                Id = id, Name = RunUICopy.UnitName(hero), Icon = hero.Body, Rarity = ItemRarity.Mythic, Price = 60, Stacking = StackPolicy.Unique,
                MaximumStacks = 1, Pools = ShopPool.Gold, Category = RelicCategory.Curio, Effect = ItemEffect.ChampionDamage, Value = 0,
                Modifiers = modifiers.ToArray(), Description = "Their gifts become yours (+50%): " + OctoberContent.HeroCopy(hero.ReferenceHero) + " Enemies +10% health, +1% more.",
                ShopThemes = new string[0], UnlockWave = 1
            };
            cache[id] = item; return item;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() => cache.Clear();
    }
}
