using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Content expansion, step 1 (Docs/Content_Expansion_Pools.md): bonds, fixed hero pools, rarities, the retired
    /// Crimson tier, Prismatic gifts drawn from the hero's pool and the Mythic companions' own skills.</summary>
    public static class ContentExpansionChecks
    {
        private static readonly string[] Themes = { "hearth", "archive", "clock", "frost", "orchard", "astral", "tidal", "oddity" };

        [MenuItem("Arcane Brigade/Checks/Content expansion")]
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption) { if (!condition) throw new InvalidOperationException("ContentExpansion " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            string before = JsonUtility.ToJson(catalog);
            ContentExpansionBuilder.Apply();
            Require(before == JsonUtility.ToJson(catalog) && catalog.PoolRevision == 1, "The expansion is applied and applying it again changes nothing");

            // bonds and companions
            Require(catalog.Bonds.Length == 33 && !catalog.Bonds.Any(b => b.Id == "fruit" || b.Id == "ball") && new[] { "paper-wings", "sweet-tooth", "big-top", "static-club", "sea-shanty", "exhibit-hall" }.All(id => catalog.Bonds.Any(b => b.Id == id && b.Icon != null)),
                "33 bonds: Fruit Basket and Lantern Revel retired; Paper Wings, Sweet Tooth, Big Top, Static Club, Sea Shanty and Exhibit Hall in with an emblem");
            Require(catalog.Bonds.Select(b => b.Effect).Distinct().Count() == catalog.Bonds.Length, "Every bond has its own effect");
            var minors = catalog.Bonds.Where(b => Array.IndexOf(Themes, b.Id) < 0).ToList();
            Require(minors.Count == 25 && minors.All(b => b.Threshold == (b.Id == "sprout" ? 3 : 2)), "25 minor bonds, threshold 2 (Jade Nursery 3)");
            foreach (UnitDefinition u in catalog.Companions)
                Require(u.Tags.Length == 2 && Array.IndexOf(Themes, u.Tags[0]) >= 0 && minors.Any(b => b.Id == u.Tags[1]), u.Id + " has one theme and one minor bond (" + string.Join(",", u.Tags) + ")");
            Require(catalog.Companions.All(u => u.Rarity != ItemRarity.Crimson) && catalog.Items.All(i => i.Rarity != ItemRarity.Crimson), "No Crimson left: Prismatic is the highest rarity");
            Require(catalog.Unit("EclipseEgg").Rarity == ItemRarity.Rainbow && catalog.Unit("EclipseEgg").Tags[0] == "astral", "Eclipse Egg is Prismatic and astral");
            Require(catalog.Companions.Where(u => u.Rarity == ItemRarity.Mythic).All(u => u.SpecialRecruit && CombatSimulation.CompanionSkillIds.Contains(u.Id)), "Every Mythic companion has its own skill and comes from the dream shop");
            Require(RunModifierRules.RarityName(ItemRarity.Rainbow) == "PRISMATIC", "The top rarity reads PRISMATIC");
            // item rarity spread (before the 30 new items)
            var rar = catalog.Items.GroupBy(i => i.Rarity).ToDictionary(g => g.Key, g => g.Count());
            Require(rar[ItemRarity.Common] == 40 && rar[ItemRarity.Uncommon] == 61 && rar[ItemRarity.Rare] == 38 && rar[ItemRarity.Mythic] == 27 && rar[ItemRarity.Rainbow] == 4,
                "Item rarities moved: Common 40, Uncommon 61, Rare 38, Mythic 27, Prismatic 4");
            Require(catalog.Items.Where(i => i.Rarity == ItemRarity.Common).All(i => i.ShopThemes.Length == 0) && catalog.Items.Where(i => i.Rarity != ItemRarity.Common).All(i => i.ShopThemes.Count(t => Array.IndexOf(Themes, t) >= 0) == (i.Rarity == ItemRarity.Rainbow ? 2 : 1) && i.ShopThemes.Count(t => t.StartsWith("fam-")) >= 1),
                "Common items are shared; every other item belongs to one theme (Prismatic two) and its item families");

            // locked weapons: every Mythic and Prismatic friend has its own, and no one else can roll it
            var lockedUnits = catalog.Companions.Where(u => u.Rarity >= ItemRarity.Mythic).ToList();
            Require(lockedUnits.Select(u => u.Weapon.Id).Distinct().Count() == lockedUnits.Count && lockedUnits.All(u => HeroLoadout.Heroes.Values.All(e => e.Weapon != u.Weapon.Id && e.FriendWeapon != u.Weapon.Id)),
                "Mythic and Prismatic friends each carry their own weapon, used by no hero or starting friend");
            using (var probe = new RunSession(catalog))
            {
                probe.Start("LanternKnight", 18102);
                foreach (UnitDefinition u in lockedUnits)
                    Require(catalog.Weapons.Count(w => probe.RecruitWeaponEligible(u.Id, w)) == 1 && probe.RecruitWeaponEligible(u.Id, u.Weapon), u.Id + " can only carry " + u.Weapon.Id);
                Require(catalog.Companions.Where(u => u.Rarity < ItemRarity.Mythic).All(u => lockedUnits.All(l => !probe.RecruitWeaponEligible(u.Id, l.Weapon))), "Locked weapons stay out of other friends' rolls");
            }

            // fixed hero pools
            var companionUse = new Dictionary<string, int>(); var minorUse = new Dictionary<string, int>();
            foreach (UnitDefinition hero in catalog.Champions)
            {
                int themes = hero.Id == "PatchworkHost" ? 3 : 2;
                Require(hero.RecruitThemes.Count(t => Array.IndexOf(Themes, t) >= 0) == themes && hero.RecruitThemes.Count(t => minors.Any(b => b.Id == t)) == 1 && hero.RecruitThemes.Count(t => t.StartsWith("fam-")) == 1 && hero.RecruitThemes.Where(t => t.StartsWith("rival-")).All(t => minors.Any(b => "rival-" + b.Id == t)),
                    hero.Id + " has " + themes + " themes, one signature bond and one item family (" + string.Join(",", hero.RecruitThemes) + ")");
                var pool = catalog.Companions.Where(u => u.Body != null && K11HeroRules.Eligible(hero, u)).ToList();
                foreach (var u in pool) companionUse[u.Id] = companionUse.TryGetValue(u.Id, out int n) ? n + 1 : 1;
                var bonds = K11HeroRules.PoolBonds(catalog, hero);
                foreach (var b in bonds) minorUse[b.Id] = minorUse.TryGetValue(b.Id, out int n) ? n + 1 : 1;
                Require(pool.Count >= 12 && pool.Count(u => u.Rarity == ItemRarity.Mythic) >= 2 && bonds.Count >= themes + 2, // +3 once the new companions have art
                    hero.Id + " pool: " + pool.Count + " companions, " + pool.Count(u => u.Rarity == ItemRarity.Mythic) + " Mythic, " + bonds.Count + " bonds");
                int items = catalog.Items.Count(i => i.ShopThemes.Length == 0 || i.ShopThemes.Any(t => hero.RecruitThemes.Contains(t)));
                Require(items >= 60 && items <= 125, hero.Id + " item pool " + items);
                if (HeroLoadout.Heroes.TryGetValue(hero.Id, out var plan) && catalog.Unit(plan.Friend) is UnitDefinition friend)
                    Require(K11HeroRules.Eligible(hero, friend), hero.Id + " starting friend " + friend.Id + " is in its own pool");
            }
            Require(catalog.Companions.Where(u => u.Body != null && !u.GiftOnly && string.IsNullOrEmpty(u.ExclusiveHero)).All(u => companionUse.TryGetValue(u.Id, out int n) && n >= 5),
                "Every ordinary companion is in at least five heroes' pools");

            // Prismatic gifts come from the hero's own pool
            MethodInfo choices = typeof(RunSession).GetMethod("RainbowChoices", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (UnitDefinition hero in catalog.Champions.Take(6))
                using (var run = new RunSession(catalog))
                {
                    run.Start(hero.Id, 18100);
                    var picked = (List<string>)choices.Invoke(run, new object[] { 3 });
                    bool inPool(string id) => catalog.Item(id) is ItemDefinition it ? it.ShopThemes.Any(t => hero.RecruitThemes.Contains(t)) : K11HeroRules.Eligible(hero, catalog.Unit(id));
                    Require(picked != null && picked.Count == 3 && picked.All(id => (catalog.Item(id)?.Rarity ?? catalog.Unit(id).Rarity) == ItemRarity.Rainbow), hero.Id + " Prismatic gift offers three Prismatic choices (" + string.Join(",", picked ?? new List<string>()) + ")");
                    // until the new Prismatic pieces have art some pools hold fewer than two; the gift then tops up from the others
                    int available = catalog.Items.Count(i => i.Rarity == ItemRarity.Rainbow && i.ShopThemes.Any(t => hero.RecruitThemes.Contains(t)))
                                    + catalog.Companions.Count(u => u.Rarity == ItemRarity.Rainbow && u.Body != null && K11HeroRules.Eligible(hero, u));
                    Require(picked.Count(inPool) >= Mathf.Min(2, available), hero.Id + " Prismatic gift draws from its own pool first (" + available + " in pool)");
                }

            // Mythic companion skills fire in a real fight
            foreach (string id in CombatSimulation.CompanionSkillIds)
                using (var run = new RunSession(catalog))
                {
                    run.Start("LanternKnight", 18101); run.State.Wave = 6; run.State.Roster.Clear();
                    UnitDefinition unit = catalog.Unit(id);
                    run.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = unit.Weapon.Id, Tier = 1 });
                    run.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = catalog.Unit("TeapotPilgrim").Weapon.Id, Tier = 1 });
                    var arena = run.PrepareCombat(); arena.Boss = null; arena.EnableHeroIdentities = true;
                    var sim = new CombatSimulation(arena); sim.Start(18101);
                    for (int t = 0; t < 900 && sim.Phase == RunPhase.Combat && sim.CompanionSkillCasts == 0; t++)
                    {
                        sim.Champion.Health = sim.Champion.Definition.Health;
                        if (id == "MirrorNewt" && t % 10 == 0)
                        {
                            var newt = sim.Units.First(u => u.Definition.Id == id);
                            var list = (List<ProjectileState>)typeof(CombatSimulation).GetField("projectiles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sim);
                            list.Add(new ProjectileState { Id = 990000 + t, Faction = Faction.Enemy, Position = newt.Position + Vector2.left * .3f, Velocity = Vector2.right * .1f, Remaining = 1, Radius = .05f, Damage = 4 });
                        }
                        sim.Step(1 / 20f, Vector2.zero);
                    }
                    Require(sim.CompanionSkillCasts > 0, id + " uses its own skill in a fight");
                }

            Directory.CreateDirectory("PreviewExports/ContentExpansion");
            File.WriteAllLines("PreviewExports/ContentExpansion/Checks.txt", notes);
            Debug.Log("CONTENT_EXPANSION_CHECKS_PASSED " + notes.Count);
        }
    }
}
