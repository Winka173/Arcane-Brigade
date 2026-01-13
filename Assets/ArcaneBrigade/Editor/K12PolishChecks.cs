using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K12PolishChecks
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException(caption); notes.Add("PASS " + caption); };
            foreach (int wave in new[] { 4, 8 })
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 12077); session.State.Wave = wave;
                    CheckpointCatalog combat = session.PrepareCombat(); combat.EnemyBudget = 0;
                    var simulation = new CombatSimulation(combat); simulation.Start(12077);
                    Vector2 position = simulation.Boss.Position; float health = simulation.Champion.Health;
                    for (int tick = 0; tick < 65; tick++) simulation.Step(1f / 60, Vector2.zero);
                    require(simulation.Boss.EntranceUntil > simulation.Elapsed && simulation.Boss.Position == position && simulation.Champion.Health == health, "Boss entrance stays stationary and harmless " + wave);
                    for (int tick = 0; tick < 15; tick++) simulation.Step(1f / 60, Vector2.zero);
                    require(simulation.Boss.EntranceUntil == 0, "Boss becomes active after entrance " + wave);
                }
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 12077); session.State.Phase = SessionPhase.Shop; session.State.Gold = 10000;
                var offer = new ShopOffer { Kind = OfferKind.Companion, Id = "CandleSnail", Token = ++session.State.NextOffer, Price = 1 }; session.State.Offers.Add(offer);
                string before = JsonUtility.ToJson(session.State);
                ShortOfferCopy.BondPreview(session, catalog, offer);
                require(JsonUtility.ToJson(session.State) == before, "Synergy preview leaves live save/RNG unchanged");
                foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
                    require(catalog.Items.Any(item => item.Rarity == rarity), "All rarity bands remain purchasable " + rarity);
            }
            PaintedAnimationLibrary art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            require(art.DreamGardenProps.Length == 4 && art.DreamGardenProps.Select(sprite => sprite.name).Distinct().Count() == 4, "Four new original scenery props imported");
            var enemy = new UnitState { Faction = Faction.Enemy, Definition = catalog.Enemies.First(unit => unit.Id == "FurnaceToad"), PendingAttack = true, AttackStarted = 1, AttackWindup = 0.4f, LastHit = -100 };
            require(K8Motion.BodyFrame(enemy, 1.2f, false) == 1, "Spitter inflates cheeks during windup");
            enemy.PendingAttack = false; require(K8Motion.BodyFrame(enemy, 1.45f, false) == 2, "Spitter opens mouth at release");
            enemy.LastHit = 2; require(K8Motion.BodyFrame(enemy, 2.1f, false) == 3, "Spitter uses authored recoil after hit");
            require(K8Motion.BodyFrame(enemy, 2.1f, true) == 0, "Reduced motion uses stable hostile frame");
            CheckKnockback(catalog, require);
            CheckLuck(catalog, require);
            CheckChestAndRelics(catalog, require);
            require(catalog.Companions.Select(unit => new SpeciesMotionProfile(unit).IdleCadence).Distinct().Count() > 30, "Species-specific motion profiles are not one shared cadence");
            File.WriteAllLines("PreviewExports/K12/Polish_Checks.txt", notes); Debug.Log("K12_POLISH_CHECKS_PASSED " + notes.Count);
        }
        private static void CheckKnockback(RunCatalog catalog, Action<bool, string> require)
        {
            float lastDistance = 10;
            foreach (float resistance in new[] { 0f, 0.65f, 0.85f })
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 12077); session.State.Wave = 2; session.State.Roster.Clear();
                    CheckpointCatalog arena = session.PrepareCombat(); arena.EnemyBudget = 0; arena.Boss = null; arena.EnableHeroMechanics = false;
                    arena.EnableArenaToys = false; arena.Champion.Weapon = UnityEngine.Object.Instantiate(catalog.Weapon("SpoonSabre"));
                    WeaponDefinition attack = arena.Champion.Weapon; attack.ConeDot = -1; attack.PushDistance = 0.4f; attack.Damage = 1;
                    try
                    {
                        var simulation = new CombatSimulation(arena); simulation.Start(12077);
                        UnitState target = simulation.AddFixture(arena.MeleeEnemy, Faction.Enemy, Vector2.right);
                        target.Definition.Speed = 0; target.Definition.KnockbackResistance = resistance; target.NextAttack = target.NextContact = 100;
                        target.Health = 100000;
                        for (int tick = 0; tick < 45 && target.LastHit < 0; tick++) simulation.Step(1f / 60, Vector2.zero);
                        float distance = target.Position.x - 1;
                        require(target.LastHit > 0 && distance > 0 && distance < lastDistance, "Actual weapon knockback reduced by resistance " + resistance);
                        lastDistance = distance;
                    }
                    finally { UnityEngine.Object.DestroyImmediate(attack); }
                }
        }
        private static void CheckLuck(RunCatalog catalog, Action<bool, string> require)
        {
            ItemDefinition luck = catalog.Items.First(item => item.EffectiveModifiers.Any(modifier => modifier.Effect == ItemEffect.Luck && modifier.Value > 0));
            int previousRare = -1;
            foreach (int stacks in new[] { 0, 20 })
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 12077); session.State.Wave = 10; session.State.Phase = SessionPhase.Shop;
                    session.State.NewRecruitsThisWave = 3;
                    if (stacks > 0) session.State.Items.Add(new ItemStack { Id = luck.Id, Count = stacks });
                    int rare = 0;
                    for (int roll = 0; roll < 300; roll++)
                    {
                        session.State.Gold = 1000000; session.Reroll();
                        foreach (ShopOffer offer in session.State.Offers.Where(offer => offer.Kind == OfferKind.Item))
                        {
                            ItemDefinition item = catalog.Item(offer.Id);
                            if (item.Rarity >= ItemRarity.Rare) rare++;
                            float discount = session.Bonus(ItemEffect.Discount) + session.Bonus(ItemEffect.RelicDiscount);
                            int expected = Mathf.Max(1, Mathf.CeilToInt(Mathf.CeilToInt(item.Price * (1 + (int)item.Rarity * 0.4f) * session.StagePriceFactor) * (1 - Mathf.Clamp(discount, -1, 0.5f))));
                            require(offer.Price == expected, "Rarity-priced gold item respects scoped discounts " + roll + "/" + item.Id);
                        }
                    }
                    require(previousRare < 0 || rare > previousRare, "Luck improves observed purple/gold frequency: stacks=" + stacks + " count=" + rare);
                    previousRare = rare;
                }
        }
        private static void CheckChestAndRelics(RunCatalog catalog, Action<bool, string> require)
        {
            using var session = new RunSession(catalog); session.Start(catalog.Champions[0].Id, 12077); session.State.Wave = 2;
            session.State.Items.Add(new ItemStack { Id = "long-pocket", Count = 2 });
            session.State.Items.Add(new ItemStack { Id = "velvet-first-aid", Count = 1 });
            CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; arena.PassiveHeal = 0;
            foreach (UnitDefinition unit in arena.StartingCompanions) unit.Passive = PassiveKind.None;
            arena.Champion.Passive = PassiveKind.None;
            var simulation = new CombatSimulation(arena); simulation.Start(12077); simulation.Champion.Health = 30;
            simulation.LootFixture(LootKind.Chest, simulation.Champion.Position + Vector2.right * 1.2f, 1);
            for (int tick = 0; tick < 20; tick++) simulation.Step(1f / 60, Vector2.zero);
            require(simulation.ChestsOpened == 1, "ChestReach opens actual chest beyond original0.85 radius");
            require(simulation.Contributions.Any(entry => entry.EffectId == "velvet-first-aid" && entry.Healing == 4), "New chest relic records actual four-HP healing");
            require((int)ItemEffect.CollisionFrequency == 47 && (int)ItemEffect.ChestReach == 48, "New stat appended without renumbering existing saved effect IDs");
        }
    }
}
