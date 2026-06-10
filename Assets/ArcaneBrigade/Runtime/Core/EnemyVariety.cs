using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Enemy variety (requirement 24/10/2026, Docs/Playtest_Art_Prompts/23_Enemy_Variety.txt): eight new ways to fight for the
    /// late waves and Endless, and one named elite per map. Each behaviour reads at a glance and is always telegraphed:
    /// Shielder (shots at enemies behind it lose half their force), Splitter (falls into two smaller halves), Blinker (fades,
    /// then reappears behind the hero), Sniper (keeps its distance, aims a warned line, fires one heavy shot), Summoner
    /// (stays back and releases Clip Mites), Armored (a hard front, soft from behind), Tethered (comes as a pair; one left
    /// alone stands its partner back up unless both fall close together) and Stomper (leaps onto the hero's spot).</summary>
    public sealed partial class CombatSimulation
    {
        public const float ShieldReach = 2.2f, ShieldCut = .5f, ArmorFront = .3f, TetherWindow = 2.5f, SummonCap = 6;

        /// <summary>Damage reductions from a Shielder in front of its ward and from an Armored enemy's front.</summary>
        private float GuardFactor(UnitState target, Vector2 direction, WeaponDefinition weapon, Faction source)
        {
            if (target.Faction != Faction.Enemy || source == Faction.Enemy) return 1;
            float factor = 1;
            Vector2 from = direction.sqrMagnitude > .0001f ? -direction.normalized : (Champion.Position - target.Position).normalized;
            if (target.Definition.Behavior == EnemyBehavior.Armored && Vector2.Dot(from, target.Facing.sqrMagnitude > .01f ? target.Facing.normalized : from) > .2f)
                factor *= ArmorFront; // struck in its ledger shell: from behind it takes the full blow
            if (weapon != null && weapon.Kind == WeaponKind.Projectile)
                foreach (UnitState u in units)
                {
                    if (u == target || !u.Alive || u.Faction != Faction.Enemy || u.Definition.Behavior != EnemyBehavior.Shielder) continue;
                    Vector2 toward = Champion.Position - u.Position, ward = target.Position - u.Position;
                    if (ward.sqrMagnitude > ShieldReach * ShieldReach || Vector2.Dot(toward, ward) > 0) continue; // only enemies on its far side
                    factor *= ShieldCut; break;
                }
            return factor;
        }

        /// <summary>The variety behaviours' own step (called from MoveCarnivalEnemy's cycle).</summary>
        private bool VarietyMove(UnitState enemy, Vector2 offset, float delta)
        {
            EnemyBehavior b = enemy.Definition.Behavior;
            switch (b)
            {
                case EnemyBehavior.Blinker:
                    if (enemy.BehaviorUntil > Elapsed)
                    {
                        if (enemy.TelegraphUntil > Elapsed) { enemy.BodyLift = -.5f * (1 - (enemy.TelegraphUntil - Elapsed) / .6f); return true; } // sinks into its shadow
                        enemy.Position = enemy.AbilityTarget; enemy.PreviousPosition = enemy.Position; spatial.Move(enemy); enemy.BodyLift = 0; enemy.BehaviorUntil = 0;
                    }
                    return false;
                case EnemyBehavior.Sniper:
                    // keeps about five units away, sidling; holds still while it aims
                    if (enemy.TelegraphUntil > Elapsed) return true;
                    float d = offset.magnitude;
                    if (d < 4.2f && d > .01f) { enemy.Position = ClampPosition(enemy.Position - offset / d * enemy.Definition.Speed * delta, enemy.Definition.Radius); enemy.Facing = offset / d; return true; }
                    return d < 5.6f;
                case EnemyBehavior.Summoner:
                    return offset.sqrMagnitude < 25; // stays back among the shelves
                case EnemyBehavior.Stomper:
                    if (enemy.BehaviorUntil <= Elapsed) { enemy.BodyLift = 0; return false; }
                    float progress = Mathf.Clamp01((Elapsed - (enemy.BehaviorUntil - 1.5f)) / 1.5f), travel = Mathf.Clamp01((progress - .4f) / .6f);
                    enemy.Position = Vector2.Lerp(enemy.AbilityOrigin, enemy.AbilityTarget, Mathf.SmoothStep(0, 1, travel));
                    enemy.BodyLift = Mathf.Sin(travel * Mathf.PI) * 1.6f;
                    return true;
            }
            return false;
        }

        /// <summary>The variety behaviours' cycle start (an ability every few seconds, always warned first).</summary>
        private void VarietyBegin(UnitState enemy, Vector2 offset)
        {
            switch (enemy.Definition.Behavior)
            {
                case EnemyBehavior.Blinker:
                {
                    Vector2 back = Champion.Facing.sqrMagnitude > .01f ? -Champion.Facing.normalized : -offset.normalized;
                    enemy.AbilityTarget = ClampPosition(Champion.Position + back * 1.6f, enemy.Definition.Radius);
                    enemy.TelegraphUntil = Elapsed + .6f; enemy.BehaviorUntil = Elapsed + .65f;
                    Danger(ExpandedFieldKind.FallingProp, enemy, enemy.AbilityTarget, .55f, .6f, .25f); // where it will rise
                    break;
                }
                case EnemyBehavior.Sniper:
                    enemy.TelegraphUntil = Elapsed + 1f; enemy.AttackWindup = 1f; enemy.AbilityFireAt = Elapsed + 1f;
                    enemy.SniperAim = Champion.Position; // the shot goes where the hero stood when the line was drawn
                    Danger(ExpandedFieldKind.Tentacle, enemy, ClampPosition(enemy.Position + (Champion.Position - enemy.Position).normalized * 7f, .2f), .18f, 1f, .3f);
                    break;
                case EnemyBehavior.Summoner:
                {
                    int alive = 0; foreach (UnitState u in units) if (u.Alive && u.SummonedBy == enemy.Id) alive++;
                    UnitDefinition mite = catalog.EnemyPool != null ? System.Array.Find(catalog.EnemyPool, e => e != null && e.Id == "ClipMite") : null;
                    if (mite == null || alive >= SummonCap) break;
                    enemy.TelegraphUntil = Elapsed + .5f;
                    for (int i = 0; i < 2; i++)
                    {
                        Vector2 at = ClampPosition(enemy.Position + new Vector2(i == 0 ? -.7f : .7f, -.3f), mite.Radius);
                        UnitState child = Spawn(mite, Faction.Enemy, at); child.SummonedBy = enemy.Id;
                    }
                    break;
                }
                case EnemyBehavior.Stomper:
                    enemy.AbilityOrigin = enemy.Position; enemy.AbilityTarget = ClampPosition(Champion.Position, enemy.Definition.Radius);
                    enemy.BehaviorUntil = Elapsed + 1.5f; enemy.NextContact = enemy.NextAttack = enemy.BehaviorUntil;
                    Danger(ExpandedFieldKind.FallingProp, enemy, enemy.AbilityTarget, 1.25f, 1.45f, .35f); // the stamp lands here
                    break;
            }
        }

        /// <summary>Splitters fall into two halves; a Tethered one left alone stands its partner back up.</summary>
        private void VarietyFell(UnitState dead)
        {
            if (dead.Faction != Faction.Enemy) return;
            if (dead.Definition.Behavior == EnemyBehavior.Splitter && !dead.IsHalf)
            {
                UnitDefinition half = Object.Instantiate(dead.Definition);
                half.Health = dead.Definition.Health * .35f; half.BodyHeight *= .65f; half.Radius *= .7f; half.Speed *= 1.25f;
                half.ContactDamage *= .6f; half.Behavior = EnemyBehavior.Splitter;
                for (int i = 0; i < 2; i++)
                {
                    UnitState h = Spawn(half, Faction.Enemy, ClampPosition(dead.Position + new Vector2(i == 0 ? -.35f : .35f, 0), half.Radius));
                    h.IsHalf = true;
                }
            }
            if (dead.Definition.Behavior == EnemyBehavior.Tethered && dead.TetherPartner != 0 && unitLookup.TryGetValue(dead.TetherPartner, out UnitState partner) && partner.Alive)
                partner.TetherReviveAt = Elapsed + TetherWindow; // unless it falls too, it stands the other one back up
        }

        /// <summary>Per step: tethered pairs revive, pairs are made on arrival, and each map's elite walks in on its third wave.</summary>
        private void ProcessVariety()
        {
            for (int i = units.Count - 1; i >= 0; i--)
            {
                UnitState u = units[i];
                if (!u.Alive || u.Faction != Faction.Enemy || u.Definition.Behavior != EnemyBehavior.Tethered) continue;
                if (u.TetherPartner == 0 && !u.IsHalf)
                {
                    // a newcomer brings its partner
                    UnitState pair = Spawn(u.Definition, Faction.Enemy, ClampPosition(u.Position + new Vector2(1.4f, .2f), u.Definition.Radius));
                    pair.IsHalf = true; pair.TetherPartner = u.Id; u.TetherPartner = pair.Id;
                }
                if (u.TetherReviveAt > 0 && Elapsed >= u.TetherReviveAt)
                {
                    u.TetherReviveAt = 0;
                    UnitState back = Spawn(u.Definition, Faction.Enemy, ClampPosition(u.Position + new Vector2(-1.2f, .1f), u.Definition.Radius));
                    back.Health = u.Definition.Health * .6f; back.IsHalf = true; back.TetherPartner = u.Id; u.TetherPartner = back.Id;
                    events.Add(new CombatEvent(CombatEventKind.Windup, back.Position, Faction.Enemy, subject: back, weapon: back.Definition.Weapon));
                }
            }
            ProcessMapElite();
        }

        private bool mapEliteSpawned;
        /// <summary>Each map's named elite walks in once, on the map's third wave, two-fifths of the way through it.</summary>
        private void ProcessMapElite()
        {
            if (mapEliteSpawned || catalog.MapElite == null || Boss != null || Elapsed < catalog.WaveDuration * .4f) return;
            mapEliteSpawned = true;
            float angle = (float)spawnRandom.NextDouble() * Mathf.PI * 2;
            Vector2 at = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 8f, catalog.MapElite.Radius);
            UnitState unit = Spawn(catalog.MapElite, Faction.Enemy, at); unit.Elite = true;
            events.Add(new CombatEvent(CombatEventKind.Windup, unit.Position, Faction.Enemy, subject: unit, weapon: unit.Definition.Weapon));
        }
    }

}
