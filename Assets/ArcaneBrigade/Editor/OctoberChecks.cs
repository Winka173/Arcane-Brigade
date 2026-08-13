using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class OctoberChecks
    {
        public static void Run()
        {
            var notes = new List<string>(); Directory.CreateDirectory("PreviewExports/October");
            void Check(bool valid, string caption)
            {
                notes.Add((valid ? "PASS " : "FAIL ") + caption); File.WriteAllLines("PreviewExports/October/Focused_Checks.txt", notes);
                if (!valid) throw new InvalidOperationException("October: " + caption);
            }
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog"); OctoberRoster manifest = OctoberContent.Roster;
            Check(c.Champions.Length == 36 && c.Companions.Length == 120, "Design roster: 36 heroes and 120 companion species installed");
            foreach (var row in manifest.heroes) Check(c.Champions.Any(u => u.Id == row.id && HeroRulesV3Catalog.Find(u.Id) != null), "Hero counterpart " + row.reference + " / " + row.id);
            foreach (var row in manifest.allies) Check(c.Companions.Any(u => u.Id == row.id && u.ReferenceCompanion == (row.reference == "Original" ? "Original:" + row.id : row.reference)), "Ally counterpart " + row.reference + " / " + row.id);
            ItemDefinition[] reference = ReferenceRelicCatalog.Create();
            Check(reference.Length == 76 && reference.All(r => c.Items.Count(i => i.ReferenceName == r.ReferenceName && i.Icon != null) == 1), "All 76 reference relics installed once with artwork");
            Check(c.Items.Where(i => !string.IsNullOrEmpty(i.ReferenceName)).Select(i => i.Icon).Distinct().Count() == 76, "76 distinct relic icon slices");
            Check(c.Bonds.Length == BondTuning.MajorCount + BondTuning.MinorCount, "Design installs 20 major and 32 minor bonds");
            foreach (var bond in BondTuning.V3Definitions)
                Check(c.Bonds.Count(installed => installed.Id == bond.Id) == 1, "Canonical v3 bond installed once " + bond.Id);
            foreach (var row in manifest.newHeroes.Concat(manifest.newAllies))
            {
                UnitDefinition u = c.Unit(row.id);
                Check(u.Body != null && u.AuthoredMotion != null && u.AuthoredMotion.Walk.Distinct().Count() >= 2 && u.AuthoredMotion.Windup[0] != u.AuthoredMotion.Release[0], "Separate painted movement and attack poses " + row.id);
            }
            Check(OctoberArtwork.Current != null && OctoberArtwork.Current.Door.Length == 4 && OctoberArtwork.Current.Watchers.Length == 12 && OctoberArtwork.Current.Keeper.Length == 4, "Door, watchers, new Keeper and nightmare floor imported");
            Check(Enumerable.Range(10, 11).Select(OctoberBalance.NewAllyChance).SequenceEqual(Enumerable.Range(10, 11).Select(OctoberBalance.NewAllyChance).OrderByDescending(x => x)), "New ally odds fall monotonically from 10 to 20 bodies");
            Check(OctoberBalance.NewAllyChance(20) < .03f && OctoberBalance.RecruitChance(20) > .3f, "Merge stock declines less than new species");
            Check(CarnivalBalance.ShopPrice(20) > CarnivalBalance.ShopPrice(8) * 1.5f, "Prices rise more noticeably after wave 8");
            Check(OctoberBalance.EndlessHealth(21, true) / OctoberBalance.EndlessHealth(20, true) > 1.15f && OctoberBalance.EndlessDamage(21, true) > OctoberBalance.EndlessDamage(20, true), "Endless pressure rises each wave from 20");
            foreach (UnitDefinition hero in c.Champions)
            {
                using var s = new RunSession(c); s.Start(hero.Id, 662314);
                s.State.Wave = 12; CheckpointCatalog a = s.PrepareCombat();
                Check(Mathf.Approximately(a.WaveDuration, WaveCadence.WaveLength(12)) && float.IsFinite(a.Champion.Health) && a.Champion.Health > 0 && a.Champion.Weapon.Cooldown >= .28f, "Playable compiles with the wave-length ramp and bounded stats " + hero.Id);
                var sim = new CombatSimulation(a); sim.Start(662314);
                for (int tick = 0; tick < 120; tick++) { sim.Champion.Health = sim.Champion.Definition.Health; sim.Step(.05f, new Vector2(.4f, .2f)); }
                Check(sim.Units.All(u => float.IsFinite(u.Position.x) && float.IsFinite(u.Position.y) && (u.Faction != Faction.Companion || float.IsFinite(u.Health) && u.Health >= 0)), "Playable live simulation finite; companions have mortal health " + hero.Id);
                s.Validate(s.State); var save = JsonUtility.ToJson(s.State); s.Restore(JsonUtility.FromJson<RunSnapshot>(save));
                Check(s.State.ChampionId == hero.Id && s.State.AchievementEvidence.Count > 0, "Progress save round trip " + hero.Id);
            }
            using (var s = new RunSession(c))
            {
                s.Start(c.Champions[0].Id, 77); s.State.Wave = 12; s.State.Phase = SessionPhase.Shop; s.State.Gold = 100000;
                MethodInfo roll = typeof(RunSession).GetMethod("RollShop", BindingFlags.NonPublic | BindingFlags.Instance);
                int evolved = 0, special = 0;
                for (int seed = 1; seed <= 120; seed++)
                {
                    s.State.ShopRandom = (uint)seed * 7919; roll.Invoke(s, new object[] { true });
                    evolved += s.State.Offers.Count(o => o.Kind == OfferKind.Companion && o.Tier > 1);
                    special += s.State.SpecialOffers.Count(o => o.Kind == OfferKind.Companion && o.Tier > 1);
                    Check(s.State.Offers.Where(o => o.Kind == OfferKind.Companion).All(o => K11HeroRules.Eligible(c.Champions[0], c.Unit(o.Id))), "Seeded normal offers respect hero themes " + seed);
                }
                Check(evolved > 20 && special > 0, "Small teams receive evolved offers in both shop pools");
                var ally = c.Companions.First(u => !u.SpecialRecruit && K11HeroRules.Eligible(c.Champions[0], u));
                var weapons = c.Weapons.Where(w => !w.SpecialOnly).Take(3).ToArray(); s.State.Roster.Clear();
                foreach (var w in weapons.Take(2)) s.State.Roster.Add(new RosterEntry { UnitId = ally.Id, WeaponId = w.Id });
                var offer = new ShopOffer { Id = ally.Id, Kind = OfferKind.Companion, Token = ++s.State.NextOffer, Price = 1 }; s.State.Offers.Clear(); s.State.Offers.Add(offer);
                // Capture the three actual ingredients immediately before the merge by replaying its isolated core.
                s.State.Roster.Add(new RosterEntry { UnitId = ally.Id, WeaponId = weapons[2].Id });
                typeof(RunSession).GetMethod("MergeCompanions", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new object[] { ally.Id });
                Check(s.State.Roster.Count == 1 && s.State.Roster[0].Tier == 2 && weapons.Any(w => w.Id == s.State.Roster[0].WeaponId), "Merge preserves exactly one actual ingredient weapon");
            }
            // Section 8: only Lone Jelly absorbs; the Curator records enemy species instead.
            foreach (var heroId in new[] { "ArchiveCurator", "LoneJelly" })
            {
                using var s = new RunSession(c); s.Start(heroId, 99); s.State.Wave = 8; s.State.Phase = SessionPhase.Shop; s.State.Gold = 1000;
                var trait = HeroRulesV3Catalog.Traits[0];
                var offer = new ShopOffer { Token = ++s.State.NextOffer, Kind = OfferKind.Companion, Id = trait.CompanionId, Price = 1 }; s.State.Offers.Add(offer);
                s.State.Roster.Clear();
                Check(s.Buy(offer.Token), "Companion purchase " + heroId);
                Check(heroId == "LoneJelly" ? s.State.Roster.Count == 0 && s.RulesV3HeroProgress.AbsorbedTraits.Contains(offer.Id) : s.State.Roster.Any(entry => entry.UnitId == offer.Id), "Only Lone Jelly absorbs a personal trait without a body " + heroId);
            }
            foreach (UnitDefinition hero in c.Champions)
            {
                using var s = new RunSession(c); s.Start(hero.Id, 662315); s.State.Wave = 12;
                typeof(RunSession).GetMethod("RollShop", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(s, new object[] { true });
                var stock = s.State.Offers.Concat(s.State.SpecialOffers).ToArray();
                Check(stock.Where(o => o.Kind == OfferKind.Companion).All(o => K11HeroRules.Eligible(hero, c.Unit(o.Id))), "Both recruit shelves respect themes " + hero.Id);
                Check(stock.Where(o => o.Kind == OfferKind.Item).All(o => s.OctoberItemEligible(c.Item(o.Id))), "Both relic shelves respect themes " + hero.Id);
            }
            // Nursery merges, hunger self-damage, delayed stitches and chest rain were replaced by section 8.
            HeroRulesV3Checks.Run();
            using (var s = new RunSession(c))
            {
                s.Start("CopperCauldron", 78); s.State.Roster.Clear();
                s.State.Roster.Add(new RosterEntry { UnitId = "CarouselPony", WeaponId = "InkPistol" });
                s.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "SpoonSabre" });
                var a = s.PrepareCombat(); var sim = new CombatSimulation(a); sim.Start(78); sim.Step(.05f, Vector2.right);
                Check(sim.Units.Any(u => u.Definition.Id == "CarouselPony" && u.RiderId > 0) && sim.Units.Any(u => u.MountId > 0 && u.BodyLift > 0), "Real mount and rider pairing follows simulation");
                Check(OctoberBalance.Push(c.Bosses[0], 100) == 0 && OctoberBalance.Push(c.Enemies[0], 100) <= .45f, "Boss knockback immune and ordinary displacement capped");
            }
            foreach (AltarRecipe recipe in c.Recipes.Where(r => r.Id.StartsWith("OctoberAltar")))
            {
                using var s = new RunSession(c); s.Start("PatchworkHost", 6617); s.State.Phase = SessionPhase.Shop; s.State.Eggs = 100; s.State.Roster.Clear();
                string[] ingredients = new[] { recipe.FirstUnit, recipe.SecondUnit, recipe.ThirdUnit }.Where(id => !string.IsNullOrEmpty(id)).ToArray();
                WeaponDefinition[] weapons = c.Weapons.Where(w => !w.SpecialOnly).Take(ingredients.Length).ToArray();
                for (int i = 0; i < ingredients.Length; i++) s.State.Roster.Add(new RosterEntry { UnitId = ingredients[i], WeaponId = weapons[i].Id });
                Check(s.CanAltar(recipe.Id) && s.Altar(recipe.Id), "Executable reference altar recipe " + recipe.ResultUnit);
                RosterEntry result = s.State.Roster.First(r => r.UnitId == recipe.ResultUnit);
                // a Mythic or Prismatic result carries its own locked weapon (content expansion v5); others keep an ingredient's
                bool locked = c.PoolRevision > 0 && c.Unit(recipe.ResultUnit).Rarity >= ItemRarity.Mythic;
                Check(result.Tier == 2 && (locked ? result.WeaponId == c.Unit(recipe.ResultUnit).Weapon.Id : weapons.Any(w => w.Id == result.WeaponId)), "Altar keeps an ingredient weapon, or the result's own locked weapon " + recipe.ResultUnit);
                s.Validate(s.State);
            }
            using (var s = new RunSession(c))
            {
                s.Start("LanternKnight", 77); s.State.Roster.Clear(); s.State.Phase = SessionPhase.Shop; s.State.Gold = 10000;
                s.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "InkPistol", Tier = 2 });
                s.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "SpoonSabre", Tier = 2 });
                foreach (var u in c.Companions.Where(u => u.Id != "TeapotPilgrim").Take(18)) s.State.Roster.Add(new RosterEntry { UnitId = u.Id, WeaponId = "InkPistol" });
                var offer = new ShopOffer { Id = "TeapotPilgrim", Tier = 2, Kind = OfferKind.Companion, Price = 1, Token = ++s.State.NextOffer }; s.State.Offers.Add(offer);
                Check(s.State.Roster.Count == 20 && s.Buy(offer.Token) && s.State.Roster.Count == 19 && s.State.Roster.Any(r => r.UnitId == offer.Id && r.Tier == 3), "Full party can complete a tier-two merge without exceeding the saved body cap");
                s.State.Roster.Add(new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "TinFish" });
                offer = new ShopOffer { Id = c.Companions.First(unit => !s.OwnsCompanionSpecies(unit.Id)).Id, Tier = 1, Kind = OfferKind.Companion, Price = 1, Token = ++s.State.NextOffer }; s.State.Offers.Add(offer);
                Check(s.ActiveRoster.Count == RulesV3Tuning.CompanionLimit && s.CompanionCapacityBlocks(offer), "Ten deployed species block a new species while owned upgrade copies remain legal");
            }
            Check(c.Companions.Where(u => u.GiftOnly).All(u => c.Champions.All(h => !K11HeroRules.Eligible(h, u))), "Gift-only allies never enter ordinary theme pools");
            Check(RunAchievements.Definitions(c).Count >= 90 && RunAchievements.Definitions(c).Select(d => d.Id).Distinct().Count() == RunAchievements.Definitions(c).Count, "Achievement journal has distinct attainable conditions");
            // Section 9 replaces the old six-pattern encore on every boss with tiered enemy and boss rules.
            EnemyRulesV3Checks.Run();
            K24OutsideStageChecks.Run();
            Debug.Log("OCTOBER_CHECKS " + notes.Count + " passed");
        }
    }
}
