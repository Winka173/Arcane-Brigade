using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Content expansion, step 1 (Docs/Content_Expansion_Proposal.md version 3, Docs/Content_Expansion_Pools.md): applies
    /// Tools/Content/expansion_lists.json (written by Tools/Content/expansion_design.py) to the run catalog. Every companion keeps
    /// one theme bond and one minor bond and gets its new rarity; every hero gets exactly two themes (its fixed pool); minor bonds
    /// get threshold 2 (Jade Nursery 3) with their per-member value kept; Fruit Basket and Lantern Revel go; Paper Wings, Sweet
    /// Tooth and Big Top arrive; items get their new rarity, price, shop weight and one shop theme (Common items are shared).
    /// Crimson is gone from the data. Re-runnable: applying twice changes nothing. Companions and heroes that wait for art are
    /// not in the catalog yet and are skipped.</summary>
    public static class ContentExpansionBuilder
    {
        public const string Source = "Tools/Content/expansion_lists.json";
        [Serializable] public sealed class CompanionRow { public string id, theme, minor, rarity; public bool isNew; }
        [Serializable] public sealed class HeroRow { public string id, starter; public string[] themes; public bool isNew; }
        [Serializable] public sealed class BondEffectRow { public string id, effect; public float value; }
        [Serializable] public sealed class LockRow { public string id, weapon; }
        [Serializable] public sealed class ItemRow { public string id, rarity; public int price, weight; public string[] themes; public bool isNew; }
        [Serializable] public sealed class ThresholdRow { public string id; public int threshold; }
        [Serializable] public sealed class BondRow { public string id, name; }
        [Serializable] public sealed class Plan
        {
            public CompanionRow[] companionList; public HeroRow[] heroList; public ItemRow[] itemList; public ThresholdRow[] minorThresholds;
            public string[] removedBonds; public BondRow[] newBondList; public BondEffectRow[] bondEffects; public LockRow[] lockedWeapons;
        }
        public static Plan Load() => JsonUtility.FromJson<Plan>(File.ReadAllText(Source));
        public static readonly string[] Themes = { "hearth", "archive", "clock", "frost", "orchard", "astral", "tidal", "oddity" };
        public static ItemRarity Rarity(string name) => (ItemRarity)Enum.Parse(typeof(ItemRarity), name);

        /// <summary>A new bond's interim emblem until its own painted icon (Art/Expansion/Bond_*) arrives: a spare painted emblem
        /// from the October bond sheet (teacup, bow, eight-point star).</summary>
        private static readonly Dictionary<string, (string art, string spare, ItemEffect effect, float value)> NewBond = new Dictionary<string, (string, string, ItemEffect, float)>
        {
            ["paper-wings"] = ("Bond_PaperWings", "OctoberBonds2_09", ItemEffect.AttackRange, .06f),
            ["sweet-tooth"] = ("Bond_SweetTooth", "OctoberBonds2_12", ItemEffect.CompanionAttackSpeed, .07f),
            ["big-top"] = ("Bond_BigTop", "OctoberBonds2_13", ItemEffect.Crit, .05f),
            ["static-club"] = ("Bond_StaticClub", "OctoberBonds2_08", ItemEffect.ChainChance, .04f),
            ["sea-shanty"] = ("Bond_SeaShanty", "OctoberBonds2_05", ItemEffect.RelicDiscount, .06f),
            ["exhibit-hall"] = ("Bond_ExhibitHall", "OctoberBonds2_10", ItemEffect.RecordedDamage, .1f),
        };

        [MenuItem("Arcane Brigade/Content/Apply expansion step 1 (bonds, pools, rarities)")]
        public static void Apply()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Plan plan = Load();
            // companions: one theme + one minor bond, new rarity; Mythic companions come from the dream shop like the others
            foreach (CompanionRow row in plan.companionList)
            {
                UnitDefinition unit = catalog.Companions.FirstOrDefault(u => u.Id == row.id);
                if (unit == null) { if (!row.isNew) throw new InvalidOperationException("Companion missing: " + row.id); continue; }
                unit.Tags = new[] { row.theme, row.minor };
                unit.Rarity = Rarity(row.rarity);
                if (unit.Rarity == ItemRarity.Mythic) unit.SpecialRecruit = true;
                EditorUtility.SetDirty(unit);
            }
            // Mythic and Prismatic friends carry their own locked weapon (RunSession keeps it out of everyone else's roll)
            foreach (LockRow row in plan.lockedWeapons)
            {
                UnitDefinition unit = catalog.Companions.FirstOrDefault(u => u.Id == row.id);
                WeaponDefinition weapon = catalog.Weapon(row.weapon);
                if (unit == null) continue; // a new friend waiting for art
                if (weapon == null) throw new InvalidOperationException("Locked weapon missing: " + row.weapon);
                unit.Weapon = weapon; EditorUtility.SetDirty(unit);
            }
            // heroes (version 4): RecruitThemes = the two themes (Patchwork Host three), the signature minor bond and the item
            // family tag "fam-*"; K11HeroRules.Eligible and the item filter read them, so the whole fixed pool follows from it
            foreach (HeroRow row in plan.heroList)
            {
                UnitDefinition hero = catalog.Champions.FirstOrDefault(u => u.Id == row.id);
                if (hero == null) { if (!row.isNew) throw new InvalidOperationException("Hero missing: " + row.id); continue; }
                hero.RecruitThemes = row.themes.ToArray(); hero.Tags = row.themes.Where(t => Array.IndexOf(Themes, t) >= 0).ToArray();
                EditorUtility.SetDirty(hero);
            }
            // bonds
            var bonds = catalog.Bonds.Where(b => Array.IndexOf(plan.removedBonds, b.Id) < 0).ToList();
            foreach (ThresholdRow row in plan.minorThresholds)
            {
                BondDefinition bond = bonds.FirstOrDefault(b => b.Id == row.id);
                if (bond == null) continue;
                if (bond.Threshold != row.threshold)
                {
                    // the same reward per member: a level now needs fewer friends, so each level is worth proportionally less
                    bond.Value *= row.threshold / (float)bond.Threshold;
                    if (bond.ExtraBonuses != null) foreach (var extra in bond.ExtraBonuses) extra.Value *= row.threshold / (float)bond.Threshold;
                    bond.Threshold = row.threshold;
                }
            }
            Sprite[] spares = AssetDatabase.LoadAllAssetsAtPath("Assets/ArcaneBrigade/Art/October/Bonds2.png").OfType<Sprite>().ToArray();
            foreach (BondRow row in plan.newBondList)
            {
                var spec = NewBond[row.id];
                BondDefinition bond = bonds.FirstOrDefault(b => b.Id == row.id);
                if (bond == null) { bond = new BondDefinition { Id = row.id, Name = row.name, Threshold = 2, Effect = spec.effect, Value = spec.value }; bonds.Add(bond); }
                string art = "Assets/ArcaneBrigade/Art/Expansion/" + spec.art + "_v1.png";
                Sprite own = File.Exists(art) ? AssetDatabase.LoadAssetAtPath<Sprite>(art) : null;
                bond.Icon = own != null ? own : spares.First(s => s.name == spec.spare);
            }
            // one effect per bond (version 4 §3)
            foreach (BondEffectRow row in plan.bondEffects)
            {
                BondDefinition bond = bonds.First(b => b.Id == row.id);
                bond.Effect = (ItemEffect)Enum.Parse(typeof(ItemEffect), row.effect); bond.Value = row.value;
                if (bond.ExtraBonuses != null) foreach (var extra in bond.ExtraBonuses) extra.Value = row.value * (extra.Threshold >= 6 ? 2 : 1);
            }
            catalog.Bonds = bonds.ToArray();
            // items
            foreach (ItemRow row in plan.itemList)
            {
                ItemDefinition item = catalog.Item(row.id);
                if (item == null) { if (!row.isNew) throw new InvalidOperationException("Item missing: " + row.id); continue; } // new items wait for their icons
                item.Rarity = Rarity(row.rarity); item.Price = row.price; item.ShopWeight = row.weight; item.ShopThemes = row.themes.ToArray();
            }
            if (catalog.Companions.Any(u => u.Rarity == ItemRarity.Crimson) || catalog.Items.Any(i => i.Rarity == ItemRarity.Crimson))
                throw new InvalidOperationException("Crimson is retired: Prismatic is the highest rarity");
            catalog.PoolRevision = 1;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("CONTENT_EXPANSION_STEP1_APPLIED companions=" + plan.companionList.Count(r => !r.isNew) + " bonds=" + catalog.Bonds.Length + " items=" + plan.itemList.Length);
        }
    }
}
