using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The weight of our blows (ImpactRules, owner's B1-B5, 19/10/2026): weighted shove, stagger, wall splat,
    /// flung bodies that bowl into others and burst where they land, and no fling from a light blow.</summary>
    public static class ImpactChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Require(bool ok, string caption) { if (!ok) throw new InvalidOperationException("Impact: " + caption); notes.Add("PASS " + caption); }

        public static void Run()
        {
            notes.Clear();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var assets = new List<UnityEngine.Object>();
            MethodInfo apply = typeof(CombatSimulation).GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.NonPublic);
            using var session = new RunSession(catalog); session.Start(catalog.Champions[0].Id, 19109); session.State.Roster.Clear();
            try
            {
                CheckpointCatalog combat = session.PrepareCombat(); combat.EnemyBudget = 0; combat.Boss = null; combat.WaveDuration = 1000;
                combat.EnableArenaToys = combat.EnableBossArenaPhase = combat.EnableSpectacles = false; combat.EnableHeroIdentities = false;
                combat.Champion.Speed = 0;
                var sim = new CombatSimulation(combat); sim.Start(19109);
                sim.Champion.NextAttack = float.PositiveInfinity;
                UnitState Enemy(Vector2 at, float health)
                {
                    UnitDefinition d = UnityEngine.Object.Instantiate(combat.MeleeEnemy); assets.Add(d);
                    d.IsBoss = false; d.Health = health; d.Speed = 1; d.ContactDamage = 0; d.KnockbackResistance = 0;
                    UnitState e = sim.AddFixture(d, Faction.Enemy, at); e.EntranceUntil = 0; e.NextAttack = e.NextContact = float.PositiveInfinity; return e;
                }
                WeaponDefinition Weapon(WeaponMotion motion, WeaponKind kind)
                {
                    var w = ScriptableObject.CreateInstance<WeaponDefinition>(); assets.Add(w);
                    w.Motion = motion; w.Kind = kind; w.PushDistance = .35f; w.Damage = 10; return w;
                }
                void Hit(UnitState target, float damage, Vector2 direction, WeaponDefinition w, bool critical = false)
                    => apply.Invoke(sim, new object[] { target, damage, Faction.Companion, "Fixture", direction, w, critical, DamageElement.Physical, default(CombatCause), false, false });
                WeaponDefinition heavy = Weapon(WeaponMotion.Slam, WeaponKind.Melee), light = Weapon(WeaponMotion.Bow, WeaponKind.Projectile);
                Require(ImpactRules.Weight(heavy) == 1 && ImpactRules.Weight(light) < .3f, "Slams are heavy, bows are light");

                // B4 + B5: a heavy shove goes further than a light one, and staggers
                UnitState a = Enemy(new Vector2(-2, 0), 1000), b = Enemy(new Vector2(-2, 3), 1000);
                Hit(a, 5, Vector2.right, heavy); Hit(b, 5, Vector2.right, light);
                float heavyPush = a.Position.x + 2, lightPush = b.Position.x + 2;
                Require(heavyPush > lightPush * 1.8f && lightPush > 0, "Heavy blows shove further (" + heavyPush.ToString("0.00") + " vs " + lightPush.ToString("0.00") + ")");
                Require(a.StaggerUntil > sim.Elapsed && b.StaggerUntil <= sim.Elapsed && a.HitWeight == 1, "Heavy blows stagger, light ones do not");

                // B3: a shove into the arena's edge splats for extra damage, once per half second
                Vector2 edge = sim.ClampPosition(new Vector2(1000, -3), a.Definition.Radius);
                UnitState wall = Enemy(edge, 1000);
                float before = wall.Health; Hit(wall, 20, Vector2.right, heavy);
                bool splat = sim.Events.Any(e => e.Kind == CombatEventKind.WallSplat && e.Subject == wall);
                Require(splat && before - wall.Health > 20.5f, "A shove into the edge splats for extra damage (" + (before - wall.Health).ToString("0.0") + ")");

                // B1 + B2: a heavy killing blow flings the body; it bowls into an enemy on its way and bursts where it lands
                foreach (UnitState u in new[] { a, b, wall }) u.Position = u.PreviousPosition = new Vector2(-6, -6);
                UnitState victim = Enemy(new Vector2(-1, 1.5f), 10), bystander = Enemy(new Vector2(.6f, 1.5f), 1000);
                Hit(victim, 30, Vector2.right, heavy);
                Require(!victim.Alive && victim.Flung && sim.Flung.Count == 1, "A heavy killing blow flings the body");
                float struck = bystander.Health; bool impact = false, landed = false;
                for (int i = 0; i < 40 && sim.Flung.Count > 0; i++)
                {
                    sim.Step(.05f, Vector2.zero);
                    impact |= sim.Events.Any(e => e.Kind == CombatEventKind.BodyImpact);
                    landed |= sim.Events.Any(e => e.Kind == CombatEventKind.Detonation && e.Cause.ItemId == "flung-body");
                }
                Require(impact && bystander.Health < struck, "The flung body bowls into the enemy in its way (" + (struck - bystander.Health).ToString("0.0") + " damage)");
                Require(landed && sim.Flung.Count == 0, "It bursts where it lands and is gone");

                // a heavy blow that only just kills pops the body where it stood (flights need over twice its health)
                UnitState barely = Enemy(new Vector2(-1, -4), 10);
                Hit(barely, 15, Vector2.right, heavy);
                Require(!barely.Alive && !barely.Flung, "A heavy blow under twice the health does not fling");
                // a light killing blow pops the body where it stood
                UnitState popped = Enemy(new Vector2(-1, -2), 5);
                Hit(popped, 30, Vector2.right, light);
                Require(!popped.Alive && !popped.Flung && sim.Flung.Count == 0, "A light killing blow does not fling");
                // R14: a pointed shot driving an enemy into the edge pins it there for a moment
                WeaponDefinition arrow = Weapon(WeaponMotion.Bow, WeaponKind.Projectile); arrow.PushDistance = .45f;
                UnitState pinned = Enemy(sim.ClampPosition(new Vector2(-1000, 2), .4f), 1000);
                Hit(pinned, 5, Vector2.left, arrow);
                Require(ImpactRules.Pointed(arrow) && pinned.PinnedUntil > sim.Elapsed, "A pointed shot pins an enemy against the edge");
                // R15: inside a blast even a light hit throws the enemy hard
                FieldInfo floor = typeof(CombatSimulation).GetField("impactWeightFloor", BindingFlags.Instance | BindingFlags.NonPublic);
                UnitState thrown = Enemy(new Vector2(-4, -4), 1000), nudged = Enemy(new Vector2(-4, -1), 1000);
                floor.SetValue(sim, ImpactRules.BlastWeight); Hit(thrown, 5, Vector2.right, light); floor.SetValue(sim, -1f);
                Hit(nudged, 5, Vector2.right, light);
                Require(thrown.Position.x + 4 > (nudged.Position.x + 4) * 1.5f, "A blast throws enemies away harder than a light hit");
                // V6: an enemy hauled into another knocks into it
                MethodInfo bump = typeof(CombatSimulation).GetMethod("PullBump", BindingFlags.Instance | BindingFlags.NonPublic);
                UnitState hauled = Enemy(new Vector2(4, -4), 1000), standing = Enemy(new Vector2(4.3f, -4), 1000);
                bump.Invoke(sim, new object[] { hauled, 20f, Faction.Companion, default(CombatCause) });
                Require(hauled.Health < 1000 && standing.Health < 1000 && sim.Events.Any(e => e.Kind == CombatEventKind.BodyImpact), "Pulled enemies knock into each other");
                bump.Invoke(sim, new object[] { hauled, 20f, Faction.Companion, default(CombatCause) });
                Require(standing.Health > 1000 - 9, "At most once per 0.4 s");
                // V2 + V9: a flung body fells a row of weak enemies, the felled fly on, and three felled is a STRIKE
                foreach (UnitState u in sim.Units) if (u.Alive && u.Faction == Faction.Enemy) u.Position = u.PreviousPosition = new Vector2(-12, 8);
                UnitState lead = Enemy(new Vector2(-3, 0), 10);
                for (int k = 0; k < 4; k++) Enemy(new Vector2(-2.35f + k * .6f, 0), 2);
                int strikes = sim.Strikes;
                Hit(lead, 40, Vector2.right, heavy);
                bool chained = false;
                for (int i = 0; i < 40 && sim.Flung.Count > 0; i++) { sim.Step(.05f, Vector2.zero); chained |= sim.Flung.Any(f => f.Depth > 0); }
                Require(chained, "A body it knocks down flies on");
                Require(sim.Strikes > strikes, "Three felled by one flung body is a STRIKE");
                // R10 + R11: sweet ranges and ranged target tastes
                WeaponDefinition shortGun = Weapon(WeaponMotion.Recoil, WeaponKind.Projectile); shortGun.Range = 3.5f;
                WeaponDefinition longGun = Weapon(WeaponMotion.Recoil, WeaponKind.Projectile); longGun.Range = 9;
                Require(ImpactRules.RangeFactor(shortGun, 1) > 1.2f && ImpactRules.RangeFactor(shortGun, 3.2f) < 1 && ImpactRules.RangeFactor(longGun, 1) < 1 && ImpactRules.RangeFactor(longGun, 7) > 1.15f, "Short guns like it close, long guns like it far");
                WeaponDefinition spread = Weapon(WeaponMotion.Recoil, WeaponKind.Projectile); spread.ProjectileCount = 4;
                UnitState tough = Enemy(new Vector2(6, 6), 900), frail = Enemy(new Vector2(6, 7), 10);
                Require(ImpactRules.TargetBias(longGun, tough, Vector2.zero, 0) < ImpactRules.TargetBias(longGun, frail, Vector2.zero, 0), "Long single shots prefer the toughest");
                Require(ImpactRules.TargetBias(spread, frail, Vector2.zero, 4) < ImpactRules.TargetBias(spread, frail, Vector2.zero, 0), "Spreads prefer the crowd");
                Require(ImpactRules.TargetBias(arrow, frail, new Vector2(6, 7), 0) < ImpactRules.TargetBias(arrow, frail, Vector2.zero, 0), "Bows prefer what is closest to the hero");
                // N2: a heavy slam knocks a small enemy up; it crashes down and knocks its neighbours
                foreach (UnitState u in sim.Units) if (u.Alive && u.Faction == Faction.Enemy) u.Position = u.PreviousPosition = new Vector2(12, -8);
                UnitState lifted = Enemy(new Vector2(-5, 5), 1000), neighbour = Enemy(new Vector2(-4.6f, 5), 1000);
                lifted.Definition.BodyHeight = .8f; lifted.Definition.KnockbackResistance = 0;
                Hit(lifted, 20, Vector2.right, heavy);
                Require(lifted.AirborneDamage > 0, "A heavy slam knocks a small enemy up");
                float peak = 0; for (int i = 0; i < 6; i++) { sim.Step(.05f, Vector2.zero); peak = Mathf.Max(peak, lifted.BodyLift); }
                Require(peak > .5f, "It rises visibly (" + peak.ToString("0.00") + ")");
                float before2 = neighbour.Health; for (int i = 0; i < 6; i++) sim.Step(.05f, Vector2.zero);
                Require(lifted.AirborneDamage == 0 && lifted.BodyLift == 0 && neighbour.Health < before2, "It crashes down and the landing knocks its neighbour");
                // N15: a puffer's burst blows up the enemies around it; a puffer felled by it bursts in turn (CHAIN)
                UnitState first = Enemy(new Vector2(4, 5), 20), second = Enemy(new Vector2(5, 5), 5), bystander2 = Enemy(new Vector2(4.5f, 6), 1000);
                first.Definition.Behavior = EnemyBehavior.Puffer; second.Definition.Behavior = EnemyBehavior.Puffer;
                int chains = sim.ChainBlasts; float bystanderBefore = bystander2.Health;
                sim.DamageFixture(first, first.Health + 1);
                for (int i = 0; i < 40 && sim.ChainBlasts == chains; i++) { sim.Step(.05f, Vector2.zero); sim.Champion.Health = sim.Champion.Definition.Health; }
                Require(!second.Alive && bystander2.Health < bystanderBefore, "A puffer's burst blows up the enemies around it");
                Require(sim.ChainBlasts > chains, "A puffer felled by a burst bursts in turn: a CHAIN");
                sim.EnterRest(); Require(sim.Flung.Count == 0, "The shop clears flung bodies");
            }
            finally { foreach (UnityEngine.Object asset in assets) UnityEngine.Object.DestroyImmediate(asset); }
            Directory.CreateDirectory("PreviewExports/Impact");
            File.WriteAllLines("PreviewExports/Impact/Checks.txt", notes);
            Debug.Log("IMPACT_CHECKS_PASSED " + notes.Count);
        }
    }
}
