using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The thirty new items of the content expansion, combat side (Docs/Content_Expansion_v5.md §10).</summary>
    public sealed partial class CombatSimulation
    {
        private float relicCuckooAt, relicUndertowAt, relicFunhouseAt, relicSundaeAt, relicShardAt, relicMimicUntil = -1, relicMimicBiteAt;
        private Vector2 relicMimicAt;
        private bool relicMimicUsed;
        private int relicChimeCrits, relicFrozenKills;

        private void ResetNewRelics()
        {
            relicCuckooAt = 12; relicUndertowAt = 8; relicFunhouseAt = 10; relicSundaeAt = relicShardAt = 0; relicMimicUntil = -1; relicMimicUsed = false;
            relicChimeCrits = relicFrozenKills = 0; KilledSpecies.Clear(); newSkillsStarted = false;
            lastLightUsed = false; lastLightUntil = -1;
            int polish = RelicCount(RelicMechanic.StarPolish);
            UnitState strongest = null;
            foreach (UnitState u in StableUnits)
            {
                if (u.Faction != Faction.Companion) continue;
                if (polish > 0 && u.Definition.CombatTier >= 3 && u.Definition.Weapon.Kind == WeaponKind.Projectile) u.Definition.Weapon.ProjectileCount += polish;
                if (strongest == null || u.Definition.CombatTier > strongest.Definition.CombatTier
                    || u.Definition.CombatTier == strongest.Definition.CombatTier && u.Definition.Weapon.Damage > strongest.Definition.Weapon.Damage) strongest = u;
            }
            if (strongest != null && RelicCount(RelicMechanic.MirrorOfTwins) > 0)
            {
                strongest.Definition.Weapon.Damage *= 1.6f;
                if (strongest.Definition.Weapon.Kind == WeaponKind.Projectile) strongest.Definition.Weapon.ProjectileCount++;
            }
        }

        /// <summary>Frozen: stopped like a sleeper (Sleep) and counted as frozen by the freeze items; Frost Salt adds 0.3s.</summary>
        private void Freeze(UnitState enemy, float seconds)
        {
            seconds += .3f * RelicCount(RelicMechanic.FrostSalt);
            Sleep(enemy, seconds);
            enemy.FrozenUntil = Mathf.Max(enemy.FrozenUntil, enemy.SleepUntil);
        }
        private bool RelicMarked(UnitState u) => Marked(u) || u.ExposedUntil > Elapsed || u.AnchorUntil > Elapsed || u.ProbedUntil > Elapsed;
        private float RelicLateFee => 1 + .25f * RelicCount(RelicMechanic.LateFee);
        private float RelicCycle(UnitState unit) => (unit.FunhouseUntil > Elapsed ? 2 : 1) * VisitorCycle(unit) / (LastLightOn(unit) ? 1.6f : 1) * MapCycle(unit) * OctoberCycle2(unit);
        // Last Light: once a wave, when the hero falls under 30% health, six seconds of +60% damage and attack speed
        private bool lastLightUsed; private float lastLightUntil = -1;
        private bool LastLightOn(UnitState unit)
        {
            if (unit == null || unit.Faction != Faction.Champion || RelicCount(RelicMechanic.LastLight) == 0) return false;
            if (!lastLightUsed && unit.Health < unit.Definition.Health * .3f) { lastLightUsed = true; lastLightUntil = Elapsed + 6; }
            return Elapsed < lastLightUntil;
        }
        /// <summary>Night Alone: with no friend on the field the gauge fills 30% faster and transformations last 3s longer.</summary>
        private bool NightAlone => RelicCount(RelicMechanic.NightAlone) > 0 && !units.Exists(u => u.Alive && u.Faction == Faction.Companion);
        private float RelicPickupRadius => catalog.PickupRadius + (relicStanding ? 0 : .5f * RelicCount(RelicMechanic.CoinSash));
        private float RelicCrit(UnitState unit)
        {
            if (!unit.CuckooReady || unit.Faction != Faction.Companion) return 0;
            unit.CuckooReady = false; return 1;
        }
        /// <summary>Tally Chalk, Frost Ledger and Funhouse Mirror on every friendly attack.</summary>
        private float RelicAttackScale(UnitState unit)
        {
            float scale = 1 + RelicCount(RelicMechanic.TallyChalk) * Mathf.Min(.08f, Kills / 25 * .01f);
            scale *= 1 + Mathf.Min(.2f, relicFrozenKills * .02f * RelicCount(RelicMechanic.FrostLedger));
            if (unit.FunhouseUntil > Elapsed) scale *= 2;
            if (LastLightOn(unit)) scale *= 1.6f;
            return scale;
        }
        /// <summary>Shots just fired by a unit: Wind Chime pierce, Kite Tail Ribbon bounce, Funhouse Mirror size.</summary>
        private void RelicEmitted(UnitState unit, int first)
        {
            if (unit.Faction != Faction.Companion || first >= projectiles.Count) return;
            if (unit.HeraldShot)
            {
                // Court Herald's fanfare: one more shot beside the first of the wave
                unit.HeraldShot = false; ProjectileState a = projectiles[first];
                var b = new ProjectileState { Id = ++nextId, Faction = a.Faction, Position = a.Position, PreviousPosition = a.PreviousPosition, Velocity = (Vector2)(Quaternion.Euler(0, 0, 10) * a.Velocity),
                    LaunchVelocity = (Vector2)(Quaternion.Euler(0, 0, 10) * a.LaunchVelocity), Damage = a.Damage, Remaining = a.Remaining, Color = a.Color, Element = a.Element, SourceName = a.SourceName,
                    LaunchSegment = a.LaunchSegment, Sprite = a.Sprite, VisualWidth = a.VisualWidth, Radius = a.Radius, Critical = a.Critical, SourceUnitId = a.SourceUnitId,
                    Cause = a.Cause.Child("herald-fanfare"), Weapon = a.Weapon, SplitCount = a.SplitCount, SplitOnImpact = a.SplitOnImpact, Pattern = a.Pattern };
                if (projectiles.Count < 4096) projectiles.Add(b);
            }
            int kite = RelicCount(RelicMechanic.KiteTailRibbon);
            for (int i = first; i < projectiles.Count; i++)
            {
                ProjectileState p = projectiles[i];
                if (unit.ChimeShots > 0) p.ExtraPierce++;
                if (p.Critical && kite > 0) p.RelicBounces += kite;
                if (unit.FunhouseUntil > Elapsed) { p.Radius *= 2; p.VisualWidth *= 2; }
            }
            if (unit.ChimeShots > 0) unit.ChimeShots--;
        }
        /// <summary>Melting Sundae: the hero is hit, the enemies around freeze.</summary>
        private void RelicSundae()
        {
            if (RelicCount(RelicMechanic.MeltingSundae) == 0 || Elapsed < relicSundaeAt) return;
            relicSundaeAt = Elapsed + 4;
            foreach (UnitState enemy in StableUnits)
                if (enemy.Alive && enemy.Faction == Faction.Enemy && (enemy.Position - Champion.Position).sqrMagnitude <= 4) Freeze(enemy, .5f);
            events.Add(new CombatEvent(CombatEventKind.Proc, Champion.Position, Faction.Champion, 0, subject: Champion, cause: NewCause(Champion.Id).Child("curio-sundae")));
        }

        private void StepNewRelics()
        {
            if (Elapsed >= relicCuckooAt && RelicCount(RelicMechanic.CuckooPendulum) > 0)
            {
                relicCuckooAt = Elapsed + 12;
                foreach (UnitState u in StableUnits) if (u.Alive && u.Faction == Faction.Companion) u.CuckooReady = true;
                events.Add(new CombatEvent(CombatEventKind.Proc, Champion.Position, Faction.Champion, 0, subject: Champion, cause: NewCause(Champion.Id).Child("curio-cuckoo")));
            }
            if (Elapsed >= relicUndertowAt && RelicCount(RelicMechanic.UndertowAnchor) > 0)
            {
                relicUndertowAt = Elapsed + 8;
                var near = new List<UnitState>();
                foreach (UnitState u in StableUnits) if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && u.EntranceUntil <= Elapsed) near.Add(u);
                near.Sort((a, b) => (a.Position - Champion.Position).sqrMagnitude.CompareTo((b.Position - Champion.Position).sqrMagnitude));
                CombatCause cause = NewCause(Champion.Id).Child("curio-undertow");
                for (int i = 0; i < Mathf.Min(5, near.Count); i++)
                {
                    UnitState u = near[i]; Vector2 away = u.Position - Champion.Position; away = away.sqrMagnitude > .0001f ? away.normalized : Vector2.right;
                    u.Position = ClampPosition(u.Position + away * 2, u.Definition.Radius); spatial.Move(u); u.KitWetUntil = Elapsed + 3;
                    events.Add(new CombatEvent(CombatEventKind.Knockback, u.Position, Faction.Champion, direction: away, cause: cause));
                }
                if (near.Count > 0) events.Add(new CombatEvent(CombatEventKind.Proc, Champion.Position, Faction.Champion, 0, subject: Champion, cause: cause));
            }
            if (Elapsed >= relicFunhouseAt && RelicCount(RelicMechanic.FunhouseMirror) > 0)
            {
                relicFunhouseAt = Elapsed + 10;
                var friends = units.FindAll(u => u.Alive && u.Faction == Faction.Companion);
                if (friends.Count > 0)
                {
                    UnitState warped = friends[combatRandom.Next(friends.Count)]; warped.FunhouseUntil = Elapsed + 3;
                    events.Add(new CombatEvent(CombatEventKind.Proc, warped.Position, Faction.Companion, 0, subject: warped, cause: NewCause(warped.Id).Child("curio-funhouse")));
                }
            }
            if (relicMimicUntil > Elapsed && Elapsed >= relicMimicBiteAt)
            {
                relicMimicBiteAt = Elapsed + .5f;
                UnitState prey = NearestEnemy(relicMimicAt, 2.5f);
                if (prey != null)
                {
                    CombatCause cause = NewCause(Champion.Id).Child("curio-mimic");
                    ApplyDamage(prey, Champion.Definition.Weapon.Damage, Faction.Champion, "Treasure mimic", prey.Position - relicMimicAt, cause: cause);
                    events.Add(new CombatEvent(CombatEventKind.SignaturePulse, prey.Position, Faction.Champion, 0, cause: cause));
                }
            }
        }

        /// <summary>Event-driven new items: species, frozen kills, Treasure Mimic, Whetstone Wafer, Duelist's Ribbon, Brain Freeze, Wind Chime.</summary>
        private void RelicNewItemEvent(CombatEvent e)
        {
            if (e.Kind == CombatEventKind.Kill && e.Faction == Faction.Enemy && e.Subject != null)
            {
                KilledSpecies.Add(e.Subject.Definition.Id);
                if (e.Subject.FrozenUntil > Elapsed - .05f) relicFrozenKills++;
            }
            if (e.Kind == CombatEventKind.ChestOpened && !relicMimicUsed && RelicCount(RelicMechanic.TreasureMimic) > 0)
            {
                relicMimicUsed = true; relicMimicUntil = Elapsed + 10; relicMimicAt = e.Position; relicMimicBiteAt = Elapsed;
            }
            if (e.Kind != CombatEventKind.Hit || e.Faction != Faction.Enemy || e.Subject == null || e.Cause.Depth != 0) return;
            UnitState performer = UnitWithId(e.Cause.SourceUnitId);
            if (performer == null || performer.Faction == Faction.Enemy) return;
            UnitState target = e.Subject;
            bool melee = performer.Definition.Weapon.Kind == WeaponKind.Melee;
            if (melee && RelicCount(RelicMechanic.WhetstoneWafer) > 0)
            {
                UnitState beside = null; float best = 1.44f;
                foreach (UnitState u in StableUnits)
                    if (u != target && u.Alive && u.Faction == Faction.Enemy) { float d = (u.Position - target.Position).sqrMagnitude; if (d < best) { best = d; beside = u; } }
                if (beside != null) ApplyDamage(beside, e.Value * .4f, performer.Faction, "Whetstone wafer", beside.Position - performer.Position, cause: e.Cause.Child("curio-whetstone"));
            }
            if (melee && performer.Faction == Faction.Companion && RelicCount(RelicMechanic.DuelistsRibbon) > 0 && ++performer.DuelHits % 4 == 0 && target.Alive)
                ApplyDamage(target, e.Value, performer.Faction, "Duelist's ribbon", target.Position - performer.Position, cause: e.Cause.Child("curio-duel"));
            if (!e.Critical) return;
            if (RelicCount(RelicMechanic.BrainFreeze) > 0 && target.FrozenUntil > Elapsed && Elapsed >= relicShardAt)
            {
                relicShardAt = Elapsed + .2f; CombatCause cause = e.Cause.Child("curio-shard");
                for (int i = 0; i < 3 && projectiles.Count < 4096; i++)
                {
                    float angle = i * Mathf.PI * 2 / 3 + combatRandom.Next(100) * .01f; Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    var shard = new ProjectileState { Id = ++nextId, Faction = Faction.Companion, Position = target.Position, PreviousPosition = target.Position, Velocity = dir * 5,
                        Damage = e.Value * .3f, Remaining = .6f, Color = new Color(.6f, .9f, 1f), Element = DamageElement.Frost, Sprite = catalog.Projectile, VisualWidth = .22f,
                        Radius = .1f, SourceUnitId = performer.Id, SourceName = "Brain freeze", Cause = cause };
                    shard.Victims.Add(target.Id); projectiles.Add(shard);
                }
                events.Add(new CombatEvent(CombatEventKind.Proc, target.Position, Faction.Companion, 0, cause: cause));
            }
            if (RelicCount(RelicMechanic.WindChime) > 0 && ++relicChimeCrits >= 10)
            {
                relicChimeCrits = 0;
                foreach (UnitState u in StableUnits) if (u.Alive && u.Faction == Faction.Companion) u.ChimeShots = 3;
                events.Add(new CombatEvent(CombatEventKind.Proc, Champion.Position, Faction.Champion, 0, subject: Champion, cause: NewCause(Champion.Id).Child("curio-chime")));
            }
        }
    }
}
