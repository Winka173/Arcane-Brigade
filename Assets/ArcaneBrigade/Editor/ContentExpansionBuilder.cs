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

        private static (ItemEffect effect, float value, ModifierScope? scope) M(ItemEffect effect, float value, ModifierScope? scope = null) => (effect, value, scope);
        public static readonly Dictionary<string, (RelicMechanic mechanic, (ItemEffect effect, float value, ModifierScope? scope)[] mods, bool unique)> ItemChanges =
            new Dictionary<string, (RelicMechanic, (ItemEffect, float, ModifierScope?)[], bool)>
        {
            ["key-oil"] = (RelicMechanic.None, new[] { M(ItemEffect.Damage, .12f) }, false),
            ["CurioTruceBottle"] = (RelicMechanic.None, new[] { M(ItemEffect.Damage, .4f), M(ItemEffect.Crit, .2f), M(ItemEffect.AttackRange, .4f), M(ItemEffect.Armor, .6f), M(ItemEffect.MoveSpeed, -.3f, ModifierScope.Champion) }, true),
            ["tarnished-crown"] = (RelicMechanic.RoyalBlow, null, false),
            ["starlight-quilt"] = (RelicMechanic.QuiltGuard, null, false),
            ["K26Defense6"] = (RelicMechanic.ApronShove, null, false),
            ["K26Defense7"] = (RelicMechanic.QuietMending, null, false),
            ["echo-postage"] = (RelicMechanic.EchoForward, null, false),
            ["CurioShieldBlower"] = (RelicMechanic.BlowerReturn, null, false),
            ["CurioEmeraldInvoice"] = (RelicMechanic.InvoiceSettles, null, false),
            ["CrimsonSeal"] = (RelicMechanic.SealBurst, new[] { M(ItemEffect.ChampionDamage, .5f), M(ItemEffect.CollisionDamage, 16) }, false),
            ["CrimsonSpring"] = (RelicMechanic.SpringDouble, new[] { M(ItemEffect.AttackSpeed, .25f), M(ItemEffect.AttackRange, .25f) }, false),
            ["RainbowCrown"] = (RelicMechanic.BorrowedCrown, new[] { M(ItemEffect.Damage, .15f) }, false),
            ["RainbowHeart"] = (RelicMechanic.ClockworkHeart, new[] { M(ItemEffect.AttackSpeed, .1f) }, false),
            ["RainbowPrism"] = (RelicMechanic.ImpossiblePrism, new[] { M(ItemEffect.BulletSplit, 3) }, false),
            ["RainbowThread"] = (RelicMechanic.LivingThread, new[] { M(ItemEffect.MeleeDamage, 4), M(ItemEffect.Knockback, .3f) }, false),
        };

        /// <summary>The thirty new items (Docs/Content_Expansion_v5.md §10): name, mechanic, food. Fish Cracker is the one plain stat.</summary>
        public static readonly Dictionary<string, (string name, RelicMechanic mechanic, bool food)> NewItems = new Dictionary<string, (string, RelicMechanic, bool)>
        {
            ["TallyChalk"] = ("Tally Chalk", RelicMechanic.TallyChalk, false),
            ["CoinSash"] = ("Coin Sash", RelicMechanic.CoinSash, false),
            ["FishCracker"] = ("Fish Cracker", RelicMechanic.None, true),
            ["PostageStamp"] = ("Postage Stamp", RelicMechanic.PostageStamp, false),
            ["FrostSalt"] = ("Frost Salt", RelicMechanic.FrostSalt, false),
            ["RefundSlip"] = ("Refund Slip", RelicMechanic.RefundSlip, false),
            ["WhetstoneWafer"] = ("Whetstone Wafer", RelicMechanic.WhetstoneWafer, true),
            ["LateFee"] = ("Late Fee", RelicMechanic.LateFee, false),
            ["HagglersAbacus"] = ("Haggler's Abacus", RelicMechanic.HagglersAbacus, false),
            ["SlipstreamScarf"] = ("Slipstream Scarf", RelicMechanic.SlipstreamScarf, false),
            ["BrainFreeze"] = ("Brain Freeze", RelicMechanic.BrainFreeze, false),
            ["WindChime"] = ("Wind Chime", RelicMechanic.WindChime, false),
            ["SpecimenPin"] = ("Specimen Pin", RelicMechanic.SpecimenPin, false),
            ["StarPolish"] = ("Star Polish", RelicMechanic.StarPolish, false),
            ["KiteTailRibbon"] = ("Kite Tail Ribbon", RelicMechanic.KiteTailRibbon, false),
            ["MeltingSundae"] = ("Melting Sundae", RelicMechanic.MeltingSundae, false),
            ["CuckooPendulum"] = ("Cuckoo Pendulum", RelicMechanic.CuckooPendulum, false),
            ["DivingBellLantern"] = ("Diving Bell Lantern", RelicMechanic.DivingBellLantern, false),
            ["TwinTicket"] = ("Twin Ticket", RelicMechanic.TwinTicket, false),
            ["TreasureMimic"] = ("Treasure Mimic", RelicMechanic.TreasureMimic, false),
            ["DuelistsRibbon"] = ("Duelist's Ribbon", RelicMechanic.DuelistsRibbon, false),
            ["FrostLedger"] = ("Frost Ledger", RelicMechanic.FrostLedger, false),
            ["UndertowAnchor"] = ("Undertow Anchor", RelicMechanic.UndertowAnchor, false),
            ["FunhouseMirror"] = ("Funhouse Mirror", RelicMechanic.FunhouseMirror, false),
            ["SpliceKit"] = ("Splice Kit", RelicMechanic.SpliceKit, false),
            ["TheEndlessEncore"] = ("The Endless Encore", RelicMechanic.EndlessEncore, false),
            ["MoonClockKey"] = ("Moon Clock Key", RelicMechanic.MoonClockKey, false),
            ["MirrorOfTwins"] = ("Mirror of Twins", RelicMechanic.MirrorOfTwins, false),
            ["StorybookEnding"] = ("Storybook Ending", RelicMechanic.StorybookEnding, false),
            ["DreamLoom"] = ("Dream Loom", RelicMechanic.DreamLoom, false),
        };
        public static string NewItemIconPath(string id) => "Assets/ArcaneBrigade/Art/Expansion/Item_" + id + "_v1.png";
        private static Sprite NewItemIcon(string id)
        {
            string path = NewItemIconPath(id);
            if (!File.Exists(path)) return null;
            if (AssetImporter.GetAtPath(path) == null) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport); // painted after the last refresh
            if (AssetImporter.GetAtPath(path) is TextureImporter importer && (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || importer.maxTextureSize != 512))
            {
                importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false; importer.maxTextureSize = 512; importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        /// <summary>Fills a new item from its plan row (price and unlock by rarity; Prismatics are gifts only, like the old ones).</summary>
        public static void FillNewItem(ItemDefinition item, ItemRow row, Sprite icon, RunCatalog catalog)
        {
            var spec = NewItems[row.id];
            item.Id = row.id; item.Name = spec.name; item.ReferenceName = ""; item.Icon = icon; item.Rarity = Rarity(row.rarity);
            item.Price = item.Rarity == ItemRarity.Common ? 24 : item.Rarity == ItemRarity.Uncommon ? 45 : item.Rarity == ItemRarity.Rare ? 60 : item.Rarity == ItemRarity.Mythic ? 74 : 200;
            item.UnlockWave = item.Rarity == ItemRarity.Common ? 0 : item.Rarity == ItemRarity.Uncommon ? 2 : item.Rarity == ItemRarity.Rare ? 4 : item.Rarity == ItemRarity.Mythic ? 6 : int.MaxValue;
            item.ShopWeight = row.weight; item.ShopThemes = row.themes.ToArray(); item.Pools = ShopPool.Gold;
            item.Mechanic = spec.mechanic; item.Category = spec.food ? RelicCategory.Food : RelicCategory.Curio;
            item.Effect = ItemEffect.Damage; item.Value = 0;
            item.Modifiers = row.id == "FishCracker" ? new[] { new StatModifier { Effect = ItemEffect.Crit, Scope = ModifierScope.Champion, Value = .06f } } : new StatModifier[0];
            item.Stacking = item.Rarity >= ItemRarity.Mythic ? StackPolicy.Unique : StackPolicy.Additive;
            item.MaximumStacks = item.Rarity == ItemRarity.Rare ? 4 : 100000;
            item.Description = RelicDescriptions.Describe(item, 1, catalog).Replace("\r\n", "\n");
        }

        [MenuItem("Arcane Brigade/Content/Apply expansion step 1 (bonds, pools, rarities)")]
        public static void Apply()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Plan plan = Load();
            ExpansionUnits.BuildCompanions(catalog, plan); // the new friends whose sheets are painted
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
            // the thirty new items enter once their icon is painted
            var allItems = catalog.Items.ToList();
            foreach (ItemRow row in plan.itemList.Where(r => r.isNew))
            {
                Sprite icon = NewItemIcon(row.id);
                if (icon == null) continue;
                ItemDefinition item = allItems.Find(i => i.Id == row.id);
                if (item == null) { item = new ItemDefinition(); allItems.Add(item); }
                FillNewItem(item, row, icon, catalog);
            }
            catalog.Items = allItems.ToArray();
            // step 3: stat-only Mythics get a small mechanic, the four old Prismatics are rebuilt around one, Key Oil and Truce
            // Bottle are rebalanced (Docs/Content_Expansion_v5.md §5 and §8)
            foreach (var change in ItemChanges)
            {
                ItemDefinition item = catalog.Item(change.Key) ?? throw new InvalidOperationException("Item missing: " + change.Key);
                var (mechanic, mods, unique) = change.Value;
                if (mechanic != RelicMechanic.None) item.Mechanic = mechanic;
                if (mods != null)
                    item.Modifiers = mods.Select(m => new StatModifier { Effect = m.effect, Value = m.value,
                        Scope = m.scope ?? item.EffectiveModifiers.FirstOrDefault(o => o.Effect == m.effect)?.Scope ?? RunModifierRules.DefaultScope(m.effect) }).ToArray();
                if (unique) item.Stacking = StackPolicy.Unique;
                item.Description = RelicDescriptions.Describe(item, 1, catalog).Replace("\r\n", "\n");
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
