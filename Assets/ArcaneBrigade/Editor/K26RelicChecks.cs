using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K26RelicChecks
    {
        private static object Invoke(object target, string method, params object[] args)
            => target.GetType().GetMethod(method,BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target,args);

        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool valid, string caption) { if (!valid) throw new InvalidOperationException("K26 " + caption); notes.Add("PASS " + caption); }
            RunCatalog source = Resources.Load<RunCatalog>("RunCatalog");
            ItemDefinition[] definitions = ReferenceRelicCatalog.Create();
            Check(definitions.Length == 76 && definitions.Select(item => item.Id).Distinct().Count() == 76 && definitions.Select(item => item.ReferenceName).Distinct().Count() == 76,"76 distinct adapted relic definitions");
            Check(definitions.All(item => item.Pools == ShopPool.Gold || item.Pools == ShopPool.Eggs),"Normal and special reference pools are exclusive");
            Check(definitions.All(item => item.Description.Length < 145 && !item.Description.Contains("<")),"Short plain English descriptions");
            Check(definitions.All(item => item.Modifiers.All(modifier => float.IsFinite(modifier.Value))),"Finite authored modifiers");
            foreach (RelicMechanic mechanic in Enum.GetValues(typeof(RelicMechanic)))
                if (mechanic <= RelicMechanic.SixWeapons) // later mechanics belong to catalog items (content expansion step 3)
                Check(definitions.Any(item => item.Mechanic == mechanic),"Reference definition for mechanic " + mechanic);
            RunCatalog catalog = UnityEngine.Object.Instantiate(source);
            catalog.Items = source.Items.Where(item => string.IsNullOrEmpty(item.ReferenceName)).Concat(definitions).ToArray();
            try
            {
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id,261015);
                    session.State.Wave = 7; session.State.Phase = SessionPhase.Shop; session.State.Gold = 2000;
                    void Own(string id) => session.State.Items.Add(new ItemStack { Id = "Curio" + id,Count = 1 });
                    void Buy(string id)
                    {
                        session.State.Phase = SessionPhase.Shop;
                        var offer = new ShopOffer { Token = ++session.State.NextOffer,Kind = OfferKind.Item,Id = "Curio" + id,Price = 10 };
                        session.State.Offers.Clear(); session.State.Offers.Add(offer);
                        Check(session.Buy(offer.Token),"Purchase " + id);
                    }
                    Own("HungryLedger"); Own("InterestTin");
                    session.State.Phase = SessionPhase.Combat;
                    session.PrepareCombat();
                    int gold = session.State.Gold; float melee = session.Bonus(ItemEffect.MeleeDamage,ModifierScope.Champion);
                    session.PrepareCombat();
                    Check(session.State.Gold == gold && Mathf.Approximately(melee,session.Bonus(ItemEffect.MeleeDamage,ModifierScope.Champion)),"Rebuilding combat cannot duplicate interest or permanent wave growth");
                    using (var restored = new RunSession(catalog))
                    {
                        restored.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State))); restored.PrepareCombat();
                        Check(restored.State.Gold == gold && Mathf.Approximately(restored.Bonus(ItemEffect.MeleeDamage,ModifierScope.Champion),melee),"Save restore keeps run buffs without granting them twice");
                    }
                    Buy("ClearSip"); float health = session.Bonus(ItemEffect.Health);
                    Buy("OrchardFizz");
                    Check(Mathf.Approximately(session.Bonus(ItemEffect.Health),health + 4 + catalog.Item("CurioOrchardFizz").EffectiveModifiers.Where(m => m.Effect == ItemEffect.Health).Sum(m => m.Value)),"Drink purchase grants permanent health plus drink stats"); // Orchard Fizz value from data since the 19/10/2026 balance pass
                    Buy("OddElixir"); float tempo = session.Bonus(ItemEffect.AttackSpeed,ModifierScope.Champion);
                    Buy("MeadowTonic"); Check(session.Bonus(ItemEffect.AttackSpeed,ModifierScope.Champion) >= tempo + .059f,"Drink-triggered attack speed");
                    Buy("NestDumpling"); Check(session.State.Eggs >= 3,"Egg gift applies immediately");
                    Buy("BentTape"); session.State.Wave++; session.State.Phase = SessionPhase.Combat;
                    Check(session.Bonus(ItemEffect.EnemyCount) >= .1f,"Next-wave curse activates");
                    session.PrepareCombat(); session.State.Wave++;
                    Check(session.Bonus(ItemEffect.EnemyCount) < .1f,"Temporary curse expires rather than becoming permanent");
                    session.State.Phase = SessionPhase.Shop; Buy("OathParcel"); float floor = session.Bonus(ItemEffect.Damage,ModifierScope.Champion);
                    Buy("VictorDraught"); Check(session.Bonus(ItemEffect.Damage,ModifierScope.Champion) >= floor,"Stat floor protects acquired team bonuses");
                    Buy("InkFeather"); session.State.RerollsThisShop = 0;
                    Check(session.RerollCost == 1,"Reroll reduction respects one-gold minimum");
                    session.State.Phase = SessionPhase.Combat; session.PrepareCombat();
                    int eggs = session.State.Eggs;
                    Check(session.SpecialStagePriceFactor == 1 && session.State.Eggs == eggs,"Alternative currency does not inflate by wave");
                    Check(session.CombatCatalog.PickupRadius >= 3.2f,"Expanded default gold collection radius");
                    session.State.Phase = SessionPhase.Shop;
                    RunSnapshot invalid = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State));
                    invalid.RelicGains.Add(new StatModifier { Effect = ItemEffect.Health,Value = float.NaN });
                    bool rejected = false; try { session.Validate(invalid); } catch (InvalidOperationException) { rejected = true; }
                    Check(rejected,"Invalid saved relic gains rejected");
                }
                foreach (ItemDefinition definition in definitions)
                {
                    using var session = new RunSession(catalog);
                    session.Start(catalog.Champions[0].Id,261015); session.State.Wave = 8;
                    session.State.Items.Add(new ItemStack { Id = definition.Id,Count = 1 });
                    if (definition.Mechanic == RelicMechanic.SoloOverdrive) session.State.RelicLeases.Add(new RelicLease { Mechanic = definition.Mechanic,Wave = 8,Count = 1 });
                    CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; arena.WaveDuration = 60;
                    var simulation = new CombatSimulation(arena); simulation.Start(arena.Seed);
                    simulation.Champion.Health = Mathf.Max(1,simulation.Champion.Definition.Health - 5);
                    UnitState enemy = simulation.AddFixture(arena.MeleeEnemy, Faction.Enemy, new Vector2(3,0));
                    enemy.NextAttack = enemy.NextContact = 100;
                    for (int tick = 0; tick < 70 && simulation.Phase == RunPhase.Combat; tick++) simulation.Step(.05f,tick < 25 ? Vector2.zero : Vector2.up);
                    Check(float.IsFinite(simulation.Champion.Health) && simulation.Champion.Health >= 0,"Live movement, attacks and timers " + definition.Id);
                }
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id,261015); session.State.Items.Add(new ItemStack { Id = "CurioGlassLotus",Count = 1 });
                    CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0;
                    var simulation = new CombatSimulation(arena); simulation.Start(1);
                    int pierce = simulation.Champion.Definition.Weapon.ProjectilePierce;
                    simulation.DamageFixture(simulation.Champion,1);
                    Check(simulation.Champion.Definition.Weapon.ProjectilePierce == pierce - 3,"Taking damage removes fresh-lotus pierce");
                    simulation.DamageFixture(simulation.Champion,1);
                    Check(simulation.Champion.Definition.Weapon.ProjectilePierce == pierce - 3,"Lotus removes bonus only once");
                }
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id,261015);
                    session.State.Items.Add(new ItemStack { Id = "CurioVenomPass",Count = 1 });
                    CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; arena.WaveDuration = 60;
                    arena.StartingCompanions = Array.Empty<UnitDefinition>();
                    var simulation = new CombatSimulation(arena); simulation.Start(1); simulation.Champion.NextAttack = 100;
                    UnitState enemy = simulation.AddFixture(arena.MeleeEnemy,Faction.Enemy,new Vector2(6,0));
                    enemy.Health = 10000; enemy.NextAttack = enemy.NextContact = 100;
                    for (int tick = 0; tick < 25; tick++)
                    {
                        Invoke(simulation,"RelicHit",enemy,Faction.Champion,new CombatCause(tick + 1,simulation.Champion.Id));
                        simulation.Step(.05f,Vector2.zero);
                    }
                    Check(enemy.Health < 10000,"Repeated hits refresh poison duration without delaying poison ticks forever");
                    float poisoned = enemy.Health;
                    simulation.Pause(true); simulation.Step(.05f,Vector2.zero);
                    Check(enemy.Health == poisoned,"Pause freezes relic poison");
                }
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id,261015); session.State.Phase = SessionPhase.Shop; session.State.Gold = 1000;
                    var offer = new ShopOffer { Token = ++session.State.NextOffer,Id = "CurioSixfoldRoot",Kind = OfferKind.Item,Price = 10 };
                    session.State.Offers.Add(offer); Check(session.Buy(offer.Token),"Acquire sixfold weapon relic");
                    string[] weapons = session.State.RelicWeaponIds.ToArray();
                    Check(weapons.Length == 6 && weapons.All(id => catalog.Weapon(id) != null),"Six actual weapon identities saved");
                    session.State.Phase = SessionPhase.Combat; session.PrepareCombat(); session.State.Wave++; session.PrepareCombat();
                    Check(weapons.SequenceEqual(session.CombatCatalog.RelicWeapons.Select(unit => unit.Weapon.Id)),"Phantom weapons do not reroll for free each wave");
                }
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id,261015); session.State.Wave = 8; session.State.Phase = SessionPhase.Shop; session.State.Gold = 1000;
                    var bought = new ShopOffer { Token = ++session.State.NextOffer,Id = "CurioOilyLozenge",Kind = OfferKind.Item,Price = 100 };
                    var other = new ShopOffer { Token = ++session.State.NextOffer,Id = "CurioOrchardFizz",Kind = OfferKind.Item,Price = 100 };
                    session.State.Offers.Add(bought); session.State.Offers.Add(other);
                    Check(session.Buy(bought.Token) && session.State.Gold == 900 && other.Price == 70,"Turn discount affects remaining offers, never retroactively discounts itself");
                }
                Check(CompanionTierScaling.Body(3) > CompanionTierScaling.Body(2) && CompanionTierScaling.Body(2) > 1.1f && CompanionTierScaling.Body(3) <= 1.3f && CompanionTierScaling.Reach(2) >= 1.35f,"Clearly different but modest promoted body sizes (owner 19/10/2026); reach unchanged");
            }
            finally { UnityEngine.Object.DestroyImmediate(catalog); }
            Directory.CreateDirectory("PreviewExports/K26"); File.WriteAllLines("PreviewExports/K26/Relic_Checks.txt",notes);
            Debug.Log("K26 relic checks PASS " + notes.Count);
        }
    }
}
