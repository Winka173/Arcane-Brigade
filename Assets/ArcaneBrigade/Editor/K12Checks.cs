using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K12Checks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException(caption); notes.Add("PASS " + caption); };
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 12077);
                float previousInterval = 100; int previousBudget = 0;
                foreach (int wave in new[] { 1, 3, 4, 5, 10, 20 })
                {
                    session.State.Wave = wave; CheckpointCatalog combat = session.PrepareCombat();
                    // the last night (wave 20) is the Last Customer duel (NightmareRun): no swarm budget, a steady stream instead
                    if (combat.FinalDuel) { require(combat.Boss != null && combat.EnemyBudget == 0 && combat.BossSwarmBudget > 0, "Final hall is a duel with a steady stream " + wave); continue; }
                    require(combat.SpawnInterval < previousInterval && combat.EnemyBudget > previousBudget, "Increasing wave density " + wave);
                    previousInterval = combat.SpawnInterval; previousBudget = combat.EnemyBudget;
                    require(combat.EnemyPool.All(enemy => wave > 1 || enemy.Weapon.Kind == WeaponKind.Contact), "No first-wave hostile shooters");
                    if (combat.Boss != null) require(combat.Boss.Health > catalog.Bosses[0].Health * 2, "Boss substantially enlarged health pool");
                }
                int Population(int wave)
                {
                    session.State.Wave = wave; var arena = session.PrepareCombat(); arena.Boss = null; arena.WaveDuration = 60;
                    var simulation = new CombatSimulation(arena); simulation.Start(12077);
                    simulation.Champion.Definition.Health = simulation.Champion.Health = 1000000;
                    foreach (UnitState unit in simulation.Units) if (unit.Faction != Faction.Enemy) unit.NextAttack = float.PositiveInfinity;
                    for (int tick = 0; tick < 600; tick++) simulation.Step(1f / 60, Vector2.right);
                    return simulation.Units.Count(unit => unit.Faction == Faction.Enemy);
                }
                int early = Population(3), between = Population(4), middle = Population(5), late = Population(19); // wave 20 is the Last Customer duel, 19 the last swarm night
                // swarm pressure ramps from wave 3 (WaveCadence.SwarmPressure), so 3 -> 5 roughly doubles; gradual means no single-night cliff
                require(between > early && middle > between && between <= early * 1.6f && middle <= between * 1.6f, "Wave3-to-5 population increases gradually, without batch cliff " + early + "/" + between + "/" + middle);
                require(late >= middle * 2, "Late waves retain substantially denser hostile population: " + early + "/" + middle + "/" + late);
                session.State.Wave = 5;
                session.State.Phase = SessionPhase.Shop; session.State.Gold = 100000; session.State.NewRecruitsThisWave = 3; session.State.NewSpeciesRecruitsThisWave = 1; // this wave's new species and copy are both used (RecruitmentRules)
                UnitDefinition unseen = catalog.Companions.First(unit => session.State.Roster.All(entry => entry.UnitId != unit.Id));
                var blocked = new ShopOffer { Kind = OfferKind.Companion, Id = unseen.Id, Price = 1 };
                require(!session.CanBuy(blocked), "New-species quota enforced even on old offers");
                require(!session.CanBuy(new ShopOffer { Kind = OfferKind.Companion, Id = session.State.Roster[0].UnitId, Price = 1 }), "Three-purchase limit also applies to owned species");
                for (int roll = 0; roll < 30; roll++)
                {
                    session.State.Gold = 100000; session.State.RerollsThisShop = 0; // the fee grows each refresh (balance pass); only the stock is under test
                    require(session.Reroll(), "Quota reroll " + roll);
                    require(session.State.Offers.All(offer => offer.Kind == OfferKind.Item), "Three-purchase quota rerolls produce items only");
                    require(session.State.Offers.All(offer => offer.Kind != OfferKind.Companion || !catalog.Unit(offer.Id).SpecialRecruit), "Special recruits excluded from gold shop");
                }
                require(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State)).NewRecruitsThisWave == 3, "Recruit quota persists in save");
                float lastChance = 1;
                foreach (int count in new[] { 9, 10, 12, 14, 16, 20, 24 })
                {
                    session.State.Roster.Clear(); for (int index = 0; index < count; index++) session.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions[index % catalog.Companions.Length].Id, WeaponId = catalog.Weapons[index % catalog.Weapons.Length].Id });
                    require(session.NewRecruitChance <= lastChance && session.NewRecruitChance > 0, "Soft cap chance remains nonzero " + count); lastChance = session.NewRecruitChance;
                }
            }
            foreach (string id in new[] { "FurnaceToad", "WardrobeCrab" })
            {
                UnitDefinition unit = catalog.Enemies.First(enemy => enemy.Id == id);
                require(unit.AnimationFrames.Length == 4 && unit.BodyHeight > 1.5f && unit.KnockbackResistance >= 0.6f, "Large hostile with original four-state art " + id);
                // the art is imported without CPU read-back (build size), so the PNG on disk is read instead
                var copy = new Texture2D(2, 2); copy.LoadImage(File.ReadAllBytes(UnityEditor.AssetDatabase.GetAssetPath(unit.Body.texture)));
                bool clear = copy.GetPixel(0, 0).a < 0.01f; UnityEngine.Object.DestroyImmediate(copy);
                require(clear, "True alpha enemy art " + id);
            }
            require(catalog.Companions.Count(unit => unit.SpecialRecruit) >= 2, "Exclusive dream recruits remain available as roster expands");
            require(catalog.Items.All(item => ShortOfferCopy.Item(item).Length < 400), "Compact shop stat descriptions: " + string.Join(", ", catalog.Items.Where(item => ShortOfferCopy.Item(item).Length >= 400).Select(item => item.Id + " " + ShortOfferCopy.Item(item).Length)));
            File.WriteAllLines("PreviewExports/K12/Core_Checks.txt", notes); Debug.Log("K12_CHECKS_PASSED " + notes.Count);
        }
    }
}
