using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private sealed class AshRemnant { public Vector2 Position; public float Until; }
        private readonly List<AshRemnant> ashRemnants = new List<AshRemnant>();
        public void ConfigureEnemyVariantCapture()
        {
            if (!RunPaths.K7Capture) throw new System.InvalidOperationException("Enemy variant fixture requires isolated capture mode");
            catalog.EnemyBudget = 24; nextSpawn = float.PositiveInfinity;
            catalog.EnableArenaToys = catalog.EnableSpectacles = catalog.EnableBossArenaPhase = false;
        }
        private EnemyVariantState NewEnemyVariant(UnitState enemy) => new EnemyVariantState { NextSpecial = Elapsed + 2.2f + enemy.Id % 5 * 0.3f, NextAffix = Elapsed + 4 + enemy.Id % 7 * 0.25f, AssignedAt = Elapsed };
        private void ClearEnemyVariants()
        {
            ashRemnants.Clear();
            foreach (UnitState unit in units)
                if (unit.Variant != null) { unit.Variant.ReleaseAt = 0; unit.Variant.RecoveryUntil = 0; unit.Variant.TravelEnd = -100; unit.Variant.DecoyUntil = 0; }
        }
        private void InitializeEnemyVariant(UnitState enemy)
        {
            if (!catalog.ExpandedContent || enemy.Variant == null || enemy.Variant.Affix != EnemyAffix.None || enemy.SwarmMinion || enemy.Variant.Summoned || enemy.Definition.IsBoss || catalog.RunWave < (catalog.MetaPactElites ? 1 : 5)) return;
            int affixed = 0;
            foreach (UnitState other in units) if (other.Alive && other.Variant != null && other.Variant.Affix != EnemyAffix.None) affixed++;
            uint seed = unchecked((uint)runSeed ^ (uint)enemy.Id * 2654435761u ^ (uint)catalog.RunWave * 2246822519u);
            seed ^= seed >> 16;
            int chance = Mathf.Min(24, 8 + catalog.RunWave - 5);
            if (catalog.MetaPactElites) chance = Mathf.Max(chance, 20); // Night Pact: elites every wave
            if (affixed >= 4 || seed % 100 >= chance) return;
            uint kind = (seed / 100) % 100;
            enemy.Variant.Affix = kind < 35 ? EnemyAffix.BlackSmoke : kind < 60 ? EnemyAffix.BlueFlame : kind < 85 ? EnemyAffix.RedFlame : kind < 95 ? EnemyAffix.Blackfire : EnemyAffix.TwinFlame;
            if (catalog.MapThemed >= 0 && (kind * 7 + 3) % 100 < 75) enemy.Variant.Affix = MapIdentity.Affix((MapTheme)catalog.MapThemed); // each map's own elite affix (item 24)
            EmpowerEnemyVariant(enemy);
        }
        private void EmpowerEnemyVariant(UnitState enemy)
        {
            UnitDefinition boosted = System.Array.Find(catalog.AffixedEnemies, unit => unit.Id == enemy.Definition.Id);
            if (boosted == null || boosted == enemy.Definition) return;
            float healthRatio = enemy.Health / Mathf.Max(1, enemy.Definition.Health);
            enemy.Definition = boosted; enemy.Health = boosted.Health * healthRatio;
            maximumRadius = Mathf.Max(maximumRadius, boosted.Radius); maximumSpeed = Mathf.Max(maximumSpeed, boosted.Speed);
        }
        private void RememberVariantDeath(UnitState target)
        {
            if (target.Faction != Faction.Enemy || target.Definition.IsBoss) return;
            if (target.Variant == null || !target.Variant.Summoned)
            {
                if (ashRemnants.Count == 24) ashRemnants.RemoveAt(0);
                ashRemnants.Add(new AshRemnant { Position = target.Position, Until = Elapsed + 7 });
            }
            foreach (UnitState follower in units)
                if (follower.Alive && follower.Variant != null && follower.Variant.LeaderId == target.Id)
                {
                    follower.Variant.LeaderLost = true;
                    follower.Variant.NextSpecial = Elapsed + 3;
                    follower.Variant.RecoveryUntil = Elapsed + 1.1f;
                }
        }
        private int ActiveSpecialists()
        {
            int active = 0;
            foreach (UnitState unit in units) if (unit.Alive && unit.Faction == Faction.Enemy && unit.Definition.SpecialEnemy != SpecialEnemyKind.None && (unit.Variant == null || !unit.Variant.Summoned)) active++;
            return active;
        }
        private bool MoveEnemyVariant(UnitState enemy)
        {
            EnemyVariantState state = enemy.Variant;
            if (state == null || !catalog.ExpandedContent) return false;
            if (state.Summoned && Elapsed - state.AssignedAt > 12)
            {
                enemy.Alive = false;
                events.Add(new CombatEvent(CombatEventKind.Kill, enemy.Position, Faction.Enemy, subjectId: enemy.Id, subject: enemy));
                return true;
            }
            if (state.TravelEnd > state.TravelStart && state.TravelStart >= 0)
            {
                float progress = Mathf.Clamp01((Elapsed - state.TravelStart) / Mathf.Max(0.01f, state.TravelEnd - state.TravelStart));
                enemy.Position = ClampPosition(Vector2.Lerp(state.Origin, state.Target, Mathf.SmoothStep(0, 1, progress)), enemy.Definition.Radius);
                enemy.BodyLift = !state.CastingAffix && enemy.Definition.SpecialEnemy == SpecialEnemyKind.LostLanternSoul ? Mathf.Sin(progress * Mathf.PI) * 0.65f : 0;
                if (progress >= 1) { state.TravelStart = state.TravelEnd = -100; state.RecoveryUntil = Mathf.Max(state.RecoveryUntil, Elapsed + 0.65f); enemy.BodyLift = 0; }
                return true;
            }
            if (state.ReleaseAt > 0)
            {
                if (Elapsed < state.ReleaseAt) return true;
                state.ReleaseAt = 0; enemy.TelegraphUntil = 0; state.RecoveryUntil = Elapsed + 0.6f;
                if (state.CastingAffix) { state.AffixCasts++; ResolveEnemyAffix(enemy); }
                else { state.SpecialCasts++; ResolveSpecialEnemy(enemy); }
                events.Add(new CombatEvent(CombatEventKind.Fire, enemy.Position, Faction.Enemy, subject: enemy, weapon: enemy.Definition.Weapon));
                return true;
            }
            if (state.RecoveryUntil > Elapsed) return true;
            if (state.Summoned && state.LeaderId > 0 && !state.LeaderLost)
            {
                UnitState leader = UnitWithId(state.LeaderId);
                if (leader != null && !leader.Alive) leader = null;
                if (leader != null)
                {
                    bool flanking = Elapsed - state.AssignedAt > 3 && leader.Variant.SpecialCasts % 2 == 0;
                    Vector2 side = new Vector2(-leader.Facing.y, leader.Facing.x) * (enemy.Id % 2 == 0 ? -1 : 1);
                    Vector2 formation = flanking ? Champion.Position + side * 1.6f : leader.Position + side * 0.9f - leader.Facing * 0.5f;
                    float delta = Mathf.Clamp(Elapsed - state.LastMovement, 0, 0.1f); state.LastMovement = Elapsed;
                    enemy.Position = ClampPosition(Vector2.MoveTowards(enemy.Position, formation, enemy.Definition.Speed * delta), enemy.Definition.Radius);
                    enemy.Facing = (Champion.Position - enemy.Position).normalized;
                    return true;
                }
                state.LeaderLost = true;
            }
            if (catalog.RunWave <= 1 || state.Summoned || state.LeaderLost || enemy.PendingAttack || enemy.EvolutionReleaseAt > 0 || enemy.EvolutionTravelUntil > Elapsed || enemy.EvolutionRecoveryUntil > Elapsed || enemy.BehaviorUntil > Elapsed || enemy.TelegraphUntil > Elapsed || enemy.ContactImpactAt > 0) return false;
            if (enemy.Definition.Behavior == EnemyBehavior.Mimic && !enemy.Awakened) return false;
            bool blackfire = state.Affix == EnemyAffix.Blackfire;
            bool affixReady = state.Affix != EnemyAffix.None && Elapsed >= state.NextAffix && (!blackfire || !state.BlackfireSpent && enemy.Health < enemy.Definition.Health * 0.4f);
            bool specialReady = enemy.Definition.SpecialEnemy != SpecialEnemyKind.None && Elapsed >= state.NextSpecial;
            if (!affixReady && !specialReady || (Champion.Position - enemy.Position).sqrMagnitude > 64) return false;
            int casting = 0;
            foreach (UnitState other in units) if (other.Alive && (other.EvolutionReleaseAt > Elapsed || other.Variant != null && other.Variant.ReleaseAt > Elapsed)) casting++;
            if (casting >= 4) return false;
            state.CastingAffix = affixReady;
            state.Heading = (Champion.Position - enemy.Position).sqrMagnitude > 0.001f ? (Champion.Position - enemy.Position).normalized : Vector2.down;
            state.Target = ClampPosition(enemy.Position + Vector2.ClampMagnitude(Champion.Position - enemy.Position, 4.5f), enemy.Definition.Radius);
            state.Origin = enemy.Position;
            state.ReleaseAt = Elapsed + (blackfire && affixReady ? 1.2f : 1);
            if (affixReady) state.NextAffix = Elapsed + 9 + enemy.Id % 3;
            else state.NextSpecial = Elapsed + 6.5f + enemy.Id % 4 * 0.4f;
            enemy.AttackStarted = Elapsed; enemy.AttackWindup = state.ReleaseAt - Elapsed;
            enemy.AttackDirection = state.Heading; enemy.Facing = state.Heading; enemy.TelegraphUntil = state.ReleaseAt;
            enemy.NextAttack = Mathf.Max(enemy.NextAttack, state.ReleaseAt + 1.3f); enemy.NextBehavior = Mathf.Max(enemy.NextBehavior, state.ReleaseAt + 1.6f);
            events.Add(new CombatEvent(CombatEventKind.Windup, enemy.Position, Faction.Enemy, subject: enemy, weapon: enemy.Definition.Weapon));
            return true;
        }
        private void VariantTravel(UnitState enemy, Vector2 destination, float duration)
        {
            EnemyVariantState state = enemy.Variant;
            state.Origin = enemy.Position; state.Target = ClampPosition(destination, enemy.Definition.Radius);
            state.TravelStart = Elapsed; state.TravelEnd = Elapsed + duration;
            enemy.NextAttack = Mathf.Max(enemy.NextAttack, state.TravelEnd + 0.7f);
        }
        private void SmokeFeint(UnitState enemy)
        {
            EnemyVariantState state = enemy.Variant;
            state.DecoyPosition = enemy.Position; state.DecoyUntil = Elapsed + 1.6f;
            Vector2 side = new Vector2(-state.Heading.y, state.Heading.x) * (enemy.Id % 2 == 0 ? 1 : -1);
            VariantTravel(enemy, enemy.Position + side * 1.7f, 0.55f);
        }
        private void VariantVolley(UnitState enemy, int count, bool blue, float power = 1)
        {
            if (catalog.RunWave <= 1) return;
            EnemyVariantState state = enemy.Variant;
            Vector2 origin = enemy.Position;
            if (enemy.Definition.SpecialEnemy == SpecialEnemyKind.LostLanternSoul)
                origin += new Vector2(Mathf.Cos(Elapsed * 1.3f), Mathf.Sin(Elapsed * 1.3f)) * 0.65f;
            CombatCause cause = NewCause(enemy.Id).Child(state.CastingAffix ? "enemy-affix" : "special-enemy");
            for (int index = 0; index < count && projectiles.Count < 4096; index++)
            {
                Vector2 direction = Quaternion.Euler(0, 0, (index - (count - 1) * 0.5f) * 19) * state.Heading;
                Sprite sprite = EnemyAffixLibrary.Effect(blue ? 1 : 2, index * 0.11f);
                projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Enemy, SourceUnitId = enemy.Id, Position = origin, PreviousPosition = origin, Velocity = direction * 1.65f, LaunchVelocity = direction * 1.65f, HostileMotion = blue ? HostileShot.Hook : HostileShot.Accelerate, Damage = enemy.Definition.Weapon.Damage * power, Remaining = 3.2f, Radius = 0.085f, Sprite = sprite, VisualWidth = 0.29f, Color = blue ? new Color(0.45f, 0.9f, 1) : new Color(1, 0.42f, 0.2f), SourceName = enemy.Definition.DisplayName + (state.CastingAffix ? " affix" : " special"), Cause = cause });
            }
        }
        private void RedCharge(UnitState enemy)
        {
            Vector2 target = enemy.Variant.Target;
            VariantTravel(enemy, target, 0.8f);
            Danger(ExpandedFieldKind.Tentacle, enemy, target, 0.23f, 0.25f, 0.45f);
        }
        private void ResolveEnemyAffix(UnitState enemy)
        {
            EnemyVariantState state = enemy.Variant;
            switch (state.Affix)
            {
                case EnemyAffix.BlackSmoke: SmokeFeint(enemy); break;
                case EnemyAffix.BlueFlame: VariantVolley(enemy, 3, true, 0.75f); break;
                case EnemyAffix.RedFlame: RedCharge(enemy); state.RecoveryUntil = Elapsed + 1.4f; break;
                case EnemyAffix.Blackfire:
                    state.BlackfireSpent = true;
                    Danger(ExpandedFieldKind.Shockwave, enemy, enemy.Position, 2.6f, 0.45f, 1.1f);
                    break;
                case EnemyAffix.TwinFlame:
                    if (state.AffixCasts % 2 == 1) VariantVolley(enemy, 3, true, 0.75f); else RedCharge(enemy);
                    break;
            }
        }
        private void ResolveSpecialEnemy(UnitState enemy)
        {
            EnemyVariantState state = enemy.Variant;
            bool evolved = enemy.Definition.EvolutionStage > 0;
            switch (enemy.Definition.SpecialEnemy)
            {
                case SpecialEnemyKind.SmokeWraith:
                    SmokeFeint(enemy);
                    if (evolved) Danger(ExpandedFieldKind.Tentacle, enemy, state.DecoyPosition + state.Heading * 2.2f, 0.18f, 0.85f, 0.3f);
                    break;
                case SpecialEnemyKind.BlueFlameFiend: VariantVolley(enemy, evolved ? 4 : 3, true); break;
                case SpecialEnemyKind.RedFlameBerserker:
                    RedCharge(enemy);
                    if (evolved) Danger(ExpandedFieldKind.ShadowPool, enemy, state.Origin, 0.55f, 0.6f, 1.2f);
                    break;
                case SpecialEnemyKind.TwinFlameFurnace:
                    if (state.SpecialCasts % 2 == 1) VariantVolley(enemy, evolved ? 4 : 3, true); else { RedCharge(enemy); if (evolved) Danger(ExpandedFieldKind.Shockwave, enemy, state.Target, 1.6f, 0.8f, 0.8f); }
                    break;
                case SpecialEnemyKind.AshSwallower:
                    for (int index = ashRemnants.Count - 1; index >= 0 && state.AshMeals < 3; index--)
                    {
                        AshRemnant remnant = ashRemnants[index];
                        if (remnant.Until < Elapsed || (remnant.Position - enemy.Position).sqrMagnitude > 12.25f) continue;
                        ashRemnants.RemoveAt(index); state.AshMeals++;
                    }
                    VariantVolley(enemy, 1 + state.AshMeals, false, 0.65f + state.AshMeals * 0.15f);
                    if (evolved && state.AshMeals > 0) Danger(ExpandedFieldKind.ShadowPool, enemy, state.Target, 0.6f, 0.8f, 1.2f);
                    break;
                case SpecialEnemyKind.LostLanternSoul:
                    if (state.SpecialCasts % 2 == 1) VariantVolley(enemy, evolved ? 3 : 2, true);
                    else { VariantTravel(enemy, state.Target, 0.8f); Danger(ExpandedFieldKind.Shockwave, enemy, state.Target, evolved ? 1.8f : 1.3f, 0.8f, 0.7f); }
                    break;
                case SpecialEnemyKind.SmokingCoffin:
                    if (enemy.Health < enemy.Definition.Health * 0.5f) state.CoffinBroken = true;
                    if (state.CoffinBroken) { RedCharge(enemy); state.NextSpecial = Elapsed + 4.5f; }
                    else
                    {
                        SummonVariantChildren(enemy, evolved ? 3 : 2, false);
                        Danger(ExpandedFieldKind.Tentacle, enemy, enemy.Position + state.Heading * 2.2f, 0.25f, 0.6f, 0.35f);
                    }
                    break;
                case SpecialEnemyKind.EmberProcession:
                    if (!state.ProcessionLeader) { state.ProcessionLeader = true; SummonVariantChildren(enemy, 2, true); }
                    VariantVolley(enemy, evolved ? 3 : 2, false, 0.8f);
                    break;
            }
        }
        private void SummonVariantChildren(UnitState source, int count, bool procession)
        {
            if (catalog.RunWave <= 1) return;
            int summons = 0, enemies = 0;
            foreach (UnitState other in units) if (other.Alive && other.Faction == Faction.Enemy) { enemies++; if (other.Variant != null && other.Variant.Summoned) summons++; }
            for (int index = 0; index < count && summons < 12 && enemies < Mathf.Min(240, catalog.EnemyBudget); index++, summons++, enemies++)
            {
                UnitDefinition definition = procession ? source.Definition : SwarmDefinition();
                Vector2 offset = Quaternion.Euler(0, 0, index * 110 + source.Id) * Vector2.right * 1.1f;
                Vector2 position = ClampPosition(source.Position + offset, definition.Radius);
                if ((position - Champion.Position).sqrMagnitude < 2.25f) continue;
                UnitState child = Spawn(definition, Faction.Enemy, position);
                child.Health *= 0.35f; child.SwarmMinion = true; child.EntranceUntil = Elapsed + 0.55f;
                child.Variant.Summoned = true; child.Variant.LeaderId = procession ? source.Id : 0;
                child.NextAttack = child.NextContact = Elapsed + 1.1f;
            }
        }
    }
}
