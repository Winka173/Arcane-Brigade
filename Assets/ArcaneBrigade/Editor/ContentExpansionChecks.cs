using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Current rules roster and skills, plus the retained content-expansion item mechanics.</summary>
    public static class ContentExpansionChecks
    {
        [MenuItem("Arcane Brigade/Checks/Content expansion")]
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption) { if (!condition) throw new InvalidOperationException("ContentExpansion " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            // The section 2/3/6/8 roster replaces the old eight themes, fixed pools and Mythic-only skills.
            RulesV3CatalogAdapter.Configure(catalog);
            string before = JsonUtility.ToJson(catalog);
            RulesV3CatalogAdapter.Configure(catalog);
            Require(before == JsonUtility.ToJson(catalog), "Rules catalog adapter is idempotent");
            Require(catalog.Companions.Length == 120 && catalog.Champions.Length == 36, "120 companions and 36 heroes");
            Require(catalog.Bonds.Length == 52 && catalog.Bonds.Select(b => b.Id).Distinct().Count() == 52, "20 majors and 32 minors with unique ids");
            Require(catalog.Companions.All(u => u.Rarity != ItemRarity.Crimson) && catalog.Items.All(i => i.Rarity != ItemRarity.Crimson), "No Crimson remains in companion or item rarities");
            Require(RunModifierRules.RarityName(ItemRarity.Rainbow) == "PRISMATIC", "The top rarity reads PRISMATIC");
            foreach (var friend in catalog.Companions)
            {
                Require(!friend.GiftOnly && string.IsNullOrEmpty(friend.ExclusiveHero), friend.Id + " uses the section 6 roster rather than retired hero gifts or exclusives");
                Require(friend.Tags.Length >= 2 && friend.Tags.Length <= 3 && friend.Tags.All(tag => catalog.Bonds.Any(b => b.Id == tag)), friend.Id + " has two or three valid bonds");
                Require(CompanionSkillTable.TryGet(friend.Id, out var skill) && !string.IsNullOrEmpty(skill.Name), friend.Id + " has its section 6 skill");
            }
            foreach (var hero in HeroRulesV3Catalog.Definitions)
            {
                Require(catalog.Unit(hero.Id) != null && hero.Relics.All(r => catalog.Item(r.Id) != null), hero.Id + " and four hero relics installed");
                Require(catalog.Companions.All(friend => K11HeroRules.Eligible(catalog.Unit(hero.Id), friend)), hero.Id + " can recruit across the design houses");
            }
            CompanionRulesV3Checks.Run();

            // step 3: items (Docs/Content_Expansion_v5.md §5)
            foreach (var change in ContentExpansionBuilder.ItemChanges)
            {
                ItemDefinition item = catalog.Item(change.Key);
                Require(item != null && (change.Value.mechanic == RelicMechanic.None || item.Mechanic == change.Value.mechanic) && !string.IsNullOrEmpty(RelicDescriptions.Describe(item, 1, catalog)),
                    change.Key + " carries its step 3 mechanic and copy");
            }
            Require(Mathf.Approximately(catalog.Item("key-oil").EffectiveModifiers[0].Value, .12f), "Key Oil gives +12% damage");
            Require(catalog.Item("CurioTruceBottle").StackLimit == 1 && catalog.Item("CurioTruceBottle").Modifiers.Any(m => m.Effect == ItemEffect.MoveSpeed && m.Value < 0), "Truce Bottle is unique and slows the hero");
            foreach (var (itemId, causeId) in new[] { ("tarnished-crown", "curio-royal"), ("starlight-quilt", "curio-quilt"), ("K26Defense6", "curio-apron"), ("echo-postage", "curio-echo"),
                ("CurioShieldBlower", "curio-blower"), ("CrimsonSeal", "curio-seal"), ("RainbowCrown", "curio-crown"), ("RainbowThread", "curio-thread") })
                using (var run = new RunSession(catalog))
                {
                    run.Start("LanternKnight", 18102); run.State.Wave = 6; run.State.Roster.Clear();
                    WeaponDefinition blade = catalog.Weapons.First(w => w.Kind == WeaponKind.Melee);
                    foreach (string friend in new[] { "TeapotPilgrim", "ButtonBeetle" })
                        run.State.Roster.Add(new RosterEntry { UnitId = friend, WeaponId = itemId == "RainbowThread" ? blade.Id : catalog.Unit(friend).Weapon.Id, Tier = 1 });
                    run.State.Items.Add(new ItemStack { Id = itemId, Count = 1 });
                    var arena = run.PrepareCombat(); arena.Boss = null;
                    var sim = new CombatSimulation(arena); sim.Start(18102);
                    bool seen = false;
                    for (int t = 0; t < 1200 && sim.Phase == RunPhase.Combat && !seen; t++)
                    {
                        sim.Champion.Health = itemId == "starlight-quilt" ? 1 : sim.Champion.Definition.Health;
                        if (itemId == "CurioShieldBlower" && t % 10 == 0)
                        {
                            var friend = sim.Units.First(u => u.Faction == Faction.Companion);
                            var list = (List<ProjectileState>)typeof(CombatSimulation).GetField("projectiles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sim);
                            list.Add(new ProjectileState { Id = 991000 + t, Faction = Faction.Enemy, Position = friend.Position + Vector2.left * .3f, Velocity = Vector2.right * .1f, Remaining = 1, Radius = .05f, Damage = 4 });
                        }
                        sim.Step(1 / 20f, itemId == "CrimsonSeal" || itemId == "K26Defense6" ? Vector2.right * (t / 40 % 2 == 0 ? 1 : -1) : Vector2.zero);
                        seen = sim.Events.Any(e => e.Cause.ItemId == causeId);
                    }
                    Require(seen, itemId + " shows its mechanic in a fight (" + causeId + ")");
                }
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 18103);
                float speed = run.Bonus(ItemEffect.AttackSpeed, ModifierScope.Companions);
                run.State.Items.Add(new ItemStack { Id = "RainbowHeart", Count = 1 }); run.State.HeartWinds = 4;
                Require(run.Bonus(ItemEffect.AttackSpeed, ModifierScope.Companions) >= speed + .1f + .12f - .001f, "Clockwork Heart: each merge winds +3% team attack speed");
                run.State.Items.Clear(); run.State.Items.Add(new ItemStack { Id = "CurioEmeraldInvoice", Count = 1 });
                float one = run.Bonus(ItemEffect.Damage, ModifierScope.Companions);
                run.State.Items.Add(new ItemStack { Id = "honey-pot", Count = 9 });
                Require(run.Bonus(ItemEffect.Damage, ModifierScope.Companions) > one + .15f, "Emerald Invoice: owned relics settle the damage penalty");
            }

            // the thirty new items: in the game exactly when their icon exists, each with its own mechanic and copy
            var expansionPlan = ContentExpansionBuilder.Load();
            foreach (var row in expansionPlan.itemList.Where(r => r.isNew))
            {
                ItemDefinition live = catalog.Item(row.id);
                Require((live != null) == File.Exists(ContentExpansionBuilder.NewItemIconPath(row.id)) && (live == null || live.Icon != null && (live.Mechanic != RelicMechanic.None || row.id == "FishCracker")),
                    row.id + (live != null ? " is in the shop with its icon and mechanic" : " waits for its icon"));
            }
            RunCatalog trial = UnityEngine.Object.Instantiate(catalog);
            var trialItems = trial.Items.ToList();
            foreach (var row in expansionPlan.itemList.Where(r => r.isNew))
                if (trial.Item(row.id) == null) { var item = new ItemDefinition(); ContentExpansionBuilder.FillNewItem(item, row, null, trial); trialItems.Add(item); }
            trial.Items = trialItems.ToArray();
            Require(trial.Items.Length == 200 + HeroRulesV3Tuning.HeroCount * HeroRulesV3Tuning.RelicsPerHero && trial.Items.Select(i => i.Id).Distinct().Count() == trial.Items.Length, "With every legacy icon painted the shop holds 200 items plus 144 section 8 hero relics");
            foreach (var row in expansionPlan.itemList.Where(r => r.isNew))
                Require(!string.IsNullOrEmpty(RelicDescriptions.Describe(trial.Item(row.id), 1, trial)), row.id + " has its copy");
            foreach (var (items, causeId, melee, crit) in new[] { (new[] { "WhetstoneWafer" }, "curio-whetstone", true, false), (new[] { "DuelistsRibbon" }, "curio-duel", true, false),
                (new[] { "UndertowAnchor" }, "curio-undertow", false, false), (new[] { "FunhouseMirror" }, "curio-funhouse", false, false), (new[] { "CuckooPendulum" }, "curio-cuckoo", false, false),
                (new[] { "TreasureMimic" }, "curio-mimic", false, false), (new[] { "MeltingSundae" }, "curio-sundae", false, false), (new[] { "WindChime" }, "curio-chime", false, true),
                (new[] { "MeltingSundae", "BrainFreeze" }, "curio-shard", false, true) })
                using (var run = new RunSession(trial))
                {
                    run.Start("LanternKnight", 18104); run.State.Wave = 8; run.State.Roster.Clear();
                    WeaponDefinition blade = trial.Weapons.First(w => w.Kind == WeaponKind.Melee);
                    foreach (string friend in new[] { "TeapotPilgrim", "ButtonBeetle" })
                        run.State.Roster.Add(new RosterEntry { UnitId = friend, WeaponId = melee ? blade.Id : trial.Unit(friend).Weapon.Id, Tier = 1 });
                    foreach (string id in items) run.State.Items.Add(new ItemStack { Id = id, Count = 1 });
                    var arena = run.PrepareCombat(); arena.Boss = null;
                    var sim = new CombatSimulation(arena); sim.Start(18104);
                    if (crit) foreach (UnitState u in sim.Units) if (u.Faction != Faction.Enemy) u.Definition.CriticalChance = 1;
                    if (causeId == "curio-mimic") sim.LootFixture(LootKind.Chest, sim.Champion.Position, 2);
                    bool seen = false;
                    for (int t = 0; t < 1600 && sim.Phase == RunPhase.Combat && !seen; t++)
                    {
                        sim.Champion.Health = sim.Champion.Definition.Health;
                        sim.Step(1 / 20f, Vector2.zero);
                        seen = sim.Events.Any(e => e.Cause.ItemId == causeId);
                    }
                    Require(seen, string.Join("+", items) + " shows its mechanic in a fight (" + causeId + ")");
                }
            using (var run = new RunSession(trial))
            {
                run.Start("LanternKnight", 18105);
                run.State.Items.Add(new ItemStack { Id = "key-oil", Count = 1 }); float oil = run.Bonus(ItemEffect.Damage, ModifierScope.Companions);
                run.State.Items.Add(new ItemStack { Id = "DreamLoom", Count = 1 });
                Require(run.Bonus(ItemEffect.Damage, ModifierScope.Companions) >= oil + .12f - .001f, "Dream Loom doubles Common items");
                run.State.Items.Clear(); run.State.KilledSpecies = Enumerable.Range(0, 7).Select(i => "species" + i).ToList();
                float crit = run.Bonus(ItemEffect.Crit, ModifierScope.Companions); run.State.Items.Add(new ItemStack { Id = "SpecimenPin", Count = 1 });
                Require(run.Bonus(ItemEffect.Crit, ModifierScope.Companions) >= crit + .07f - .001f, "Specimen Pin: +1% crit per species defeated");
                run.State.Items.Clear(); run.State.Gold = 500; run.State.Phase = SessionPhase.Shop; run.State.Items.Add(new ItemStack { Id = "RefundSlip", Count = 1 });
                var offer = new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Item, Id = "key-oil", Price = 20, Currency = ShopCurrency.Gold };
                run.State.Offers.Clear(); run.State.Offers.Add(offer);
                Require(run.Buy(offer.Token), "Buy a relic to refund");
                int gold = run.State.Gold;
                Require(run.RefundAvailable && run.RefundLastRelic() && run.State.Gold == gold + offer.Price && run.StackCount("key-oil") == 0 && !run.RefundAvailable, "Refund Slip returns the last relic for its price, once per shop");
            }
            using (var run = new RunSession(trial))
            {
                run.Start("LanternKnight", 18106); run.State.Roster.Clear();
                var picks = trial.Companions.Where(u => !u.SpecialRecruit && u.Rarity < ItemRarity.Mythic && K11HeroRules.Eligible(trial.Unit("LanternKnight"), u) && !new[] { "TeapotPilgrim", "HoodWisp", "ShellPostman", "PaperDevil" }.Contains(u.Id)).Take(3).ToList();
                foreach (var u in picks) run.State.Roster.Add(new RosterEntry { UnitId = u.Id, WeaponId = u.Weapon.Id, Tier = 1 });
                run.State.Items.Add(new ItemStack { Id = "SpliceKit", Count = 1 }); run.State.Wave = 9; run.State.Phase = SessionPhase.Combat;
                run.PrepareCombat();
                Require(run.State.Roster.Count == 1 && run.State.Roster[0].Tier == 2, "Splice Kit turns three different one-star friends into one two-star friend");
            }

            Directory.CreateDirectory("PreviewExports/ContentExpansion");
            File.WriteAllLines("PreviewExports/ContentExpansion/Checks.txt", notes);
            Debug.Log("CONTENT_EXPANSION_CHECKS_PASSED " + notes.Count);
        }
    }
}
