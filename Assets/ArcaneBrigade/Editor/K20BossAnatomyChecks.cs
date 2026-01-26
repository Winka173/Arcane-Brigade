using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K20BossAnatomyChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption) { if (!condition) throw new InvalidOperationException("K20 " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            BossAnatomy anatomy = Resources.Load<BossAnatomy>("BossAnatomy");
            Require(anatomy != null && anatomy.Bosses.Length == 10, "All ten bosses have authored anatomy");
            var parts = anatomy.Bosses.SelectMany(entry => entry.Organs).ToArray();
            Require(parts.Length == 30 && parts.Select(part => part.Id).Distinct().Count() == 30, "Thirty individually identified organs");
            Require(parts.Select(part => part.Rest).Distinct().Count() == 30 && parts.Select(part => part.Charged).Distinct().Count() == 30, "Every boss part has its own rest and charged artwork");
            Require(parts.Select(part => part.Rest).Intersect(catalog.Items.Select(item => item.Icon)).Count() == 0, "Boss anatomy never reuses relic illustrations");
            Require(parts.All(part => part.Rest != null && part.Charged != null && part.Scar != null && part.Rest != part.Charged && !string.IsNullOrWhiteSpace(part.BreakSummary)), "Every organ has poses, scar and consequence copy");
            Require(BossPartRules.HitZone(Vector2.right) == 0 && BossPartRules.HitZone(Vector2.left) == 1 && BossPartRules.HitZone(Vector2.up) == 2 && BossPartRules.HitZone(Vector2.down) == 2, "Directional hits independently address all three organs");
            Require(catalog.Bosses.All(source => BossPartRules.BulletCount(new UnitState { Definition = source, LeftPartBrokenAt = 0, RightPartBrokenAt = 0 }) < CombatSimulation.BossBulletCount(source.BossPattern)), "Every exposed core reduces its volley, including bosses without a projectile organ");
            MethodInfo damage = typeof(CombatSimulation).GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo attack = typeof(CombatSimulation).GetMethod("ResolveAnatomyAttack", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (UnitDefinition source in catalog.Bosses)
            {
                BossAnatomyEntry entry = BossAnatomy.For(source);
                Require(entry != null && entry.Organs.Length == 3, "Mapped anatomy " + source.Id);
                Require(BossPartRules.Describe(source).Contains(entry.Organs[2].Name), "Collection explains third organ " + source.Id);
                for (int part = 0; part < 3; part++)
                {
                    using var session = new RunSession(catalog); session.Start("LanternKnight", 20077); session.State.Wave = 4;
                    CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = source; arena.EnemyBudget = 0;
                    var simulation = new CombatSimulation(arena); simulation.Start(20077);
                    UnitState boss = simulation.Boss; boss.EntranceUntil = 0; boss.ActiveBossOrgan = part; boss.TelegraphDirection = Vector2.down;
                    bool suppressVolley = (bool)attack.Invoke(simulation, new object[] { boss });
                    BossOrganRole role = entry.Organs[part].Role;
                    Require(suppressVolley == ((role & BossOrganRole.Volley) == 0), "Volley tied to correct organ " + entry.Organs[part].Id);
                    foreach (ExpandedField field in simulation.ExpandedFields)
                    {
                        Require(field.At > simulation.Elapsed && field.SourceOrgan == part && field.Source == boss, "Part attack has source attribution and safe warning " + entry.Organs[part].Id);
                        Require(Vector2.Distance(field.Start, BossPartRules.Position(boss, part)) < 0.001f, "Attack starts at visible attachment " + entry.Organs[part].Id);
                    }
                    Require((role & BossOrganRole.Volley) != 0 || simulation.ExpandedFields.Count > 0, "Non-volley organ creates real telegraphed attack " + entry.Organs[part].Id);
                    Vector2 direction = part == 0 ? Vector2.right : part == 1 ? Vector2.left : Vector2.up;
                    float hit = BossPartRules.Durability(boss, part);
                    void Hit(float value, Faction faction) => damage.Invoke(simulation, new object[] { boss, value, faction, "K20 fixture", direction, null, false, DamageElement.Physical, default(CombatCause), false });
                    Hit(hit * 0.49f, Faction.Companion);
                    Require(!BossPartRules.Broken(boss, part), "Partial damage cannot detach " + entry.Organs[part].Id);
                    Hit(hit * 0.52f, Faction.Companion);
                    Require(BossPartRules.Broken(boss, part) && boss.Alive, "Selected organ detaches without killing boss " + entry.Organs[part].Id);
                    Require(Enumerable.Range(0, 3).Where(index => index != part).All(index => !BossPartRules.Broken(boss, index)), "Other two organs survive " + entry.Organs[part].Id);
                    Require(BossPartRules.NextOrgan(boss) != part, "Broken organ cannot be selected " + entry.Organs[part].Id);
                    Require(simulation.Events.Count(value => value.Kind == CombatEventKind.BossPhase && value.Value == part + 1) == 1, "One break feedback event " + entry.Organs[part].Id);
                    Hit(1, Faction.Companion);
                    Require(simulation.Events.Count(value => value.Kind == CombatEventKind.BossPhase && value.Value == part + 1) == 1, "Repeated hit cannot replay break or rewards " + entry.Organs[part].Id);
                    foreach (var mapping in new[] { (BossOrganRole.Lash, ExpandedFieldKind.Tentacle), (BossOrganRole.Props, ExpandedFieldKind.FallingProp), (BossOrganRole.Pool, ExpandedFieldKind.ShadowPool) })
                        if ((role & mapping.Item1) != 0) Require(!BossPartRules.Allows(boss, mapping.Item2), "Corresponding attack disabled " + entry.Organs[part].Id);
                    if ((role & BossOrganRole.Volley) != 0) Require(BossPartRules.BulletCount(boss) < CombatSimulation.BossBulletCount(source.BossPattern), "Projectile budget reduced " + entry.Organs[part].Id);
                    if ((role & BossOrganRole.Locomotion) != 0) Require(BossPartRules.MovementMultiplier(boss) == 0.6f, "Locomotion break changes actual speed " + entry.Organs[part].Id);
                    if ((role & BossOrganRole.Tempo) != 0) Require(BossPartRules.CycleAdjustment(boss) > 0, "Tempo break extends recovery " + entry.Organs[part].Id);
                    if ((role & BossOrganRole.Rupture) != 0) Require(BossPartRules.CycleAdjustment(boss) < 0, "Rupture produces a faster exposed phase " + entry.Organs[part].Id);
                    simulation.EnterRest(); Require(simulation.ExpandedFields.Count == 0, "Shop clears anatomy hazards " + entry.Organs[part].Id);
                }
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 20077); session.State.Wave = 4;
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = catalog.Bosses.First(unit => unit.Id == "CoinWidow"); arena.EnemyBudget = 0;
                var simulation = new CombatSimulation(arena); simulation.Start(20077);
                UnitState boss = simulation.Boss; boss.EntranceUntil = 0; boss.ActiveBossOrgan = 2; boss.TelegraphUntil = 1;
                Vector2 vault = BossPartRules.Position(boss, 2);
                simulation.LootFixture(LootKind.Coin, vault, 7);
                MethodInfo vacuum = typeof(CombatSimulation).GetMethod("ProcessBossVacuum", BindingFlags.Instance | BindingFlags.NonPublic);
                vacuum.Invoke(simulation, new object[] { boss });
                Require(boss.StolenCoins == 7, "Vault actually captures loose coins during its windup");
                damage.Invoke(simulation, new object[] { boss, BossPartRules.Durability(boss, 2) * 1.01f, Faction.Companion, "Vault fixture", Vector2.up, null, false, DamageElement.Physical, default(CombatCause), false });
                Require(boss.StolenCoins == 0 && simulation.Pickups.Where(pickup => pickup.Kind == LootKind.Coin).Sum(pickup => pickup.Value) == 7, "Breaking vault returns exactly stolen gold, without minting money");
                vacuum.Invoke(simulation, new object[] { boss });
                Require(boss.StolenCoins == 0, "Destroyed vault cannot recapture its returned coins");
                boss.ActiveBossOrgan = 0; attack.Invoke(simulation, new object[] { boss });
                float elapsed = simulation.Elapsed; int fields = simulation.ExpandedFields.Count;
                simulation.Pause(true); simulation.Step(0.5f, Vector2.zero);
                Require(simulation.Elapsed == elapsed && simulation.ExpandedFields.Count == fields, "Pause freezes organ hazards and simulation clock");
                simulation.Pause(false); simulation.EnterRest();
                Require(simulation.ExpandedFields.Count == 0, "Rest removes anatomy hazards and cannot leak attacks into shopping");
            }
            Directory.CreateDirectory("PreviewExports/K20"); File.WriteAllLines("PreviewExports/K20/Focused_Checks.txt", notes);
            Debug.Log("K20_ANATOMY_CHECKS_PASSED " + notes.Count);
        }
    }
}
