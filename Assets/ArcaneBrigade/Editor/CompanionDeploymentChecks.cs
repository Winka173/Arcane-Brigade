using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class CompanionDeploymentChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string name)
        {
            if (!ok) throw new InvalidOperationException("Deployment: " + name);
            notes.Add("PASS " + name);
        }
        private static RunSnapshot Copy(RunSnapshot state) => JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(state));
        private static void Shop(RunSession run, int wave)
        {
            run.State.Phase = SessionPhase.Shop; run.State.Wave = wave;
            run.State.NewRecruitsThisWave = run.State.NewSpeciesRecruitsThisWave = run.State.SpecialNewSpeciesRecruitsThisWave = 0;
            run.State.Gold = 1000000; run.State.Eggs = 100000;
            run.State.Offers.Clear(); run.State.SpecialOffers.Clear();
        }
        private static ShopOffer Offer(RunSession run, UnitDefinition unit, bool special = false, int tier = 1, string weapon = null)
        {
            var o = new ShopOffer { Token = ++run.State.NextOffer, Id = unit.Id, Kind = OfferKind.Companion, Tier = tier, Price = 1, WeaponId = weapon ?? unit.Weapon.Id };
            (special ? run.State.SpecialOffers : run.State.Offers).Add(o); return o;
        }
        private static UnitDefinition Friend(RunSession run, RunCatalog c, IEnumerable<string> exclude = null)
            => c.Companions.First(u => u.ReferenceCompanion != "Evil" && u.ReferenceCompanion != "Little Cabbage"
                && !u.GiftOnly && !RunSession.StorySpecies(u.Id) && !(exclude?.Contains(u.Id) ?? false)
                && c.Weapons.Any(w => run.RecruitWeaponEligible(u.Id, w)));
        private static string Bonds(RunSession run) => string.Join(";", run.BondCounts.OrderBy(p => p.Key).Select(p => p.Key + "=" + p.Value));

        public static void Run()
        {
            notes.Clear();
            var c = Resources.Load<RunCatalog>("RunCatalog");
            using (var run = new RunSession(c))
            {
                run.Start("LanternKnight", 231026);
                foreach (var milestone in new[] { (wave: 9, earned: 5), (wave: 10, earned: 5), (wave: 11, earned: 5), (wave: 12, earned: 6), (wave: 15, earned: 7), (wave: 18, earned: 8), (wave: 20, earned: 8) })
                {
                    Shop(run, milestone.wave);
                    Check(run.EarnedNewSpeciesAllowance == milestone.earned, "Late allowance calendar at wave " + milestone.wave);
                    Check(run.CopyRecruitsRemaining == (milestone.wave >= 10 ? 2 : 1), "Copy quota at wave " + milestone.wave);
                }
                RunSnapshot old = Copy(run.State); old.SchemaRevision = 4;
                run.Restore(old);
                Check(run.EarnedNewSpeciesAllowance == 10 && run.State.RegularAllowanceCarryover == 2, "Legacy wave-20 allowance is retained without taking back banked recruitment");
                Check(run.State.NextSpecialShopWave >= 23 && run.State.NextSpecialShopWave <= 24, "Legacy dream-shop schedule starts three or four waves after migration");
            }
            Check(RunMeta.UnlockedDifficulty(new PlayerProfile()) == 3 && RunMeta.UnlockedDifficulty(null) == 3, "The first four difficulties are unlocked even for a fresh profile");
            using (var run = new RunSession(c))
            {
                run.Start("LanternKnight", 231026); run.State.Roster.Clear();
                UnitDefinition u = Friend(run, c);
                string fieldWeapon = null, bondBefore = null;
                float bonusBefore = 0, attackBefore = 0;
                var storage = new RunStorage(Path.Combine(Application.temporaryCachePath, "DeploymentChecks", Guid.NewGuid().ToString("N")));
                for (int n = 1; n <= 4; n++)
                {
                    Shop(run, n);
                    var o = Offer(run, u);
                    var card = new ShopDescriptionCard();
                    string original = JsonUtility.ToJson(run.State);
                    card.Refresh(run, c, o);
                    Check(JsonUtility.ToJson(run.State) == original, "Preview keeps live state and RNG unchanged, copy " + n);
                    Check(run.CanBuy(o), "Copy " + n + " is purchasable, even with a repeated weapon");
                    Check(RunTransaction.TryCommit(run, c, "Buy upgrade material", () => run.Buy(o.Token), () => true, out string error), "Transaction copy " + n + " succeeds: " + error);
                    run.Validate(run.State);
                    var arena = run.PrepareCombat();
                    Check(arena.StartingCompanions.Count(x => x.Id == u.Id) == 1 && run.ActiveRoster.Count == 1, "Exactly one body after copy " + n);
                    int tier = n < 2 ? 1 : n < 4 ? 2 : 3;
                    Check(run.ActiveRoster[0].Tier == tier && CompanionDeployment.OwnedValue(run.State.Roster, u.Id) == n, "Copies preserve material value and correct stars " + n);
                    var model = RunBuildReadModel.Capture(run, c, run.State.Health);
                    Check(model.Companions.Count(row => row.UnitId == u.Id) == 1, "Build sheet contains one row, copy " + n);
                    Check(model.ShopStats.Actors.Count == 2, "Stat snapshot has hero and one fighter, copy " + n);
                    if (n == 2)
                    {
                        fieldWeapon = run.ActiveRoster[0].WeaponId; bondBefore = Bonds(run);
                        bonusBefore = run.Bonus(ItemEffect.Damage, ModifierScope.Companions);
                        attackBefore = arena.StartingCompanions[0].Weapon.Damage;
                    }
                    if (n == 3)
                    {
                        Check(run.ActiveRoster[0].WeaponId == fieldWeapon, "Stored 1-star material does not change the deployed 2-star weapon");
                        Check(Bonds(run) == bondBefore && run.Bonus(ItemEffect.Damage, ModifierScope.Companions) == bonusBefore && Mathf.Approximately(arena.StartingCompanions[0].Weapon.Damage, attackBefore), "Inactive copy adds no bonds, passive bonus or attack damage");
                        Check(card.Summary.Contains("Stored for merging") && card.Bonds.All(b => b.After == b.Count), "Shop marks material inactive and does not promise extra bond members");
                    }
                    storage.SaveRun(run.State);
                    using var loaded = new RunSession(c); loaded.Restore(storage.LoadRun(loaded));
                    Check(JsonUtility.ToJson(loaded.State) == JsonUtility.ToJson(run.State) && loaded.ActiveRoster.Count == 1, "Real save/load preserves all material weapons and RNG, copy " + n);
                }
                Shop(run, 10); Check(!run.CanBuy(Offer(run, u)), "Maximum-star species cannot waste money on extra shop materials");
            }
            using (var run = new RunSession(c))
            {
                run.Start("LanternKnight", 231026); Shop(run, 6);
                Check(run.EarnedNewSpeciesAllowance == 3 && run.BankedNewSpeciesAllowance == 3, "Skipping six waves banks three regular species purchases");
                var a = Friend(run, c, run.ActiveRoster.Select(r => r.UnitId));
                Check(run.Buy(Offer(run, a).Token), "Catch-up buys one new species at wave 6");
                var b = Friend(run, c, run.ActiveRoster.Select(r => r.UnitId));
                Check(!run.CanBuy(Offer(run, b)), "Catch-up never buys two new regular species in one wave");
                int banked = run.BankedNewSpeciesAllowance, used = run.State.RegularNewSpeciesPurchased;
                Check(run.Buy(Offer(run, b, true).Token), "Dream shop can still buy its own new species in the same wave");
                Check(run.BankedNewSpeciesAllowance == banked && run.State.RegularNewSpeciesPurchased == used, "Dream-shop purchase does not spend regular bank");
                var d = Friend(run, c, run.ActiveRoster.Select(r => r.UnitId));
                Check(!run.CanBuy(Offer(run, d, true)), "Dream shop refuses a second new species in the same visit");
                run.Restore(Copy(run.State));
                Check(run.SpecialNewRecruitsRemaining == 0 && run.NewRecruitsRemaining == 0 && run.BankedNewSpeciesAllowance == banked, "Reload does not reset either shop quota");
                Shop(run, 7); Check(run.NewRecruitsRemaining == 1 && run.SpecialNewRecruitsRemaining == 1, "Next wave permits catch-up and a fresh dream visit");
                Check(run.Buy(Offer(run, d, true).Token), "Next dream visit permits another new species independently");
                var e = Friend(run, c, run.ActiveRoster.Select(r => r.UnitId));
                Check(run.Buy(Offer(run, e).Token), "Regular catch-up remains available after dream-shop purchase");
                Check(run.BankedNewSpeciesAllowance == run.EarnedNewSpeciesAllowance - run.State.RegularNewSpeciesPurchased, "Allowance is earned minus spent without loss");
            }
            using (var run = new RunSession(c))
            {
                run.Start("LanternKnight", 231026); Shop(run, 6);
                typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, new object[] { false });
                Check(run.State.Offers.Any(o => o.Kind == OfferKind.Companion && !run.OwnsCompanionSpecies(o.Id)), "Unspent allowance guarantees a new-species offer when eligible pool exists");
                var unit = c.Companions.First(u => u.ReferenceCompanion != "Little Cabbage" && u.ReferenceCompanion != "Evil" && c.Weapons.Count(w => run.RecruitWeaponEligible(u.Id, w)) >= 2);
                var weapons = c.Weapons.Where(w => run.RecruitWeaponEligible(unit.Id, w)).Take(2).ToArray();
                run.State.Roster.Clear();
                run.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = weapons[0].Id });
                Shop(run, 7);
                Check(run.Buy(Offer(run, unit, false, 1, weapons[1].Id).Token), "Mixed-weapon material purchase completes merge");
                Check(run.ActiveRoster[0].Tier == 2 && weapons.Any(w => w.Id == run.ActiveRoster[0].WeaponId), "Merged weapon is from the two ingredients");
                RunSnapshot legacy = Copy(run.State); legacy.SchemaRevision = 3;
                legacy.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = weapons.First(w => w.Id != legacy.Roster[0].WeaponId).Id, Tier = 1 });
                string before = JsonUtility.ToJson(legacy);
                run.Restore(legacy);
                Check(run.State.SchemaRevision == RunSnapshot.CurrentSchemaRevision && run.ActiveRoster.Count == 1 && CompanionDeployment.OwnedValue(run.State.Roster, unit.Id) == 3, "Legacy save retains every owned material and deploys the strongest one");
                Check(JsonUtility.ToJson(legacy) == before, "Restore does not mutate original legacy snapshot");
                Check(run.State.RegularNewSpeciesPurchased == 0, "Conservative legacy quota migration excludes the first owned species");
            }
            using (var run = new RunSession(c))
            {
                run.Start("LanternKnight", 231026); Shop(run, 16); run.State.Roster.Clear();
                var unit = c.Companions.First(u => !u.SpecialRecruit && !u.GiftOnly && !RunSession.StorySpecies(u.Id)
                    && u.ReferenceCompanion != "Evil" && u.ReferenceCompanion != "Little Cabbage" && u.UnlockWave <= 16
                    && K11HeroRules.Eligible(c.Unit(run.State.ChampionId), u) && c.Weapons.Any(w => run.RecruitWeaponEligible(u.Id, w)));
                string weapon = c.Weapons.First(w => run.RecruitWeaponEligible(unit.Id, w)).Id;
                run.State.Roster.Add(new RosterEntry { UnitId = unit.Id, WeaponId = weapon, Tier = 2 });
                run.State.RegularNewSpeciesPurchased = run.EarnedNewSpeciesAllowance;
                typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(run, new object[] { false });
                var finalMaterial = run.State.Offers.FirstOrDefault(o => o.Kind == OfferKind.Companion && o.Id == unit.Id);
                Check(finalMaterial != null && finalMaterial.Tier == 2, "Final 2-star material is offered even after the regular new-species bank is exhausted");
                Check(run.Buy(finalMaterial.Token) && run.ActiveRoster.Count == 1 && run.ActiveRoster[0].Tier == 3, "Buying the final 2-star material merges into one 3-star fighter");
            }
            foreach (var hero in c.Champions)
            using (var run = new RunSession(c))
            {
                run.Start(hero.Id, 231026);
                if (run.State.Roster.Count == 0) continue;
                var r = run.State.Roster[0];
                run.State.Roster.Add(new RosterEntry { UnitId = r.UnitId, WeaponId = r.WeaponId, Tier = r.Tier });
                var arena = run.PrepareCombat();
                Check(arena.StartingCompanions.Select(x => x.Id).Distinct().Count() == arena.StartingCompanions.Length, hero.Id + " cannot deploy duplicate species");
                Check(RunBuildReadModel.Capture(run, c, run.State.Health).Companions.Count >= arena.StartingCompanions.Length, hero.Id + " full read-model handles reserve inventory");
            }
            Directory.CreateDirectory("PreviewExports/CompanionDeployment-20261022");
            File.WriteAllLines("PreviewExports/CompanionDeployment-20261022/Focused_Checks.txt", notes);
            Debug.Log("COMPANION_DEPLOYMENT_CHECKS_PASSED " + notes.Count);
            K6Checks.Run();
            K4SandboxChecks.Run();
        }
    }
}
