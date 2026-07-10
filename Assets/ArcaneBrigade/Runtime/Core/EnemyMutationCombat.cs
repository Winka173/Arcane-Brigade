using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        private bool MoveMutatedEnemy(UnitState enemy)
        {
            if (!catalog.ExpandedContent || enemy.Variant != null && enemy.Variant.Summoned || enemy.Definition.EvolutionStage == 0 || enemy.Definition.Mutation == EnemyMutation.None || catalog.RunWave <= 1) return false;
            if (enemy.EvolutionTravelUntil > enemy.EvolutionTravelStarted && enemy.EvolutionTravelStarted >= 0)
                return AdvanceMutationTravel(enemy);
            if (enemy.EvolutionReleaseAt > 0)
            {
                if (Elapsed < enemy.EvolutionReleaseAt) return true;
                enemy.EvolutionReleaseAt = 0; enemy.TelegraphUntil = 0;
                enemy.EvolutionRecoveryUntil = Elapsed + 0.45f;
                enemy.EvolutionCasts++;
                ResolveMutation(enemy);
                events.Add(new CombatEvent(CombatEventKind.Fire, enemy.Position, Faction.Enemy, subject: enemy, weapon: enemy.Definition.Weapon));
                return true;
            }
            if (enemy.EvolutionRecoveryUntil > Elapsed) return true;
            if (enemy.NextEvolutionAttack < 0) enemy.NextEvolutionAttack = Elapsed + 2.5f + enemy.Id % 7 * 0.35f;
            if (Elapsed < enemy.NextEvolutionAttack || enemy.PendingAttack || enemy.TelegraphUntil > Elapsed || enemy.BehaviorUntil > Elapsed || enemy.ContactImpactAt > 0) return false;
            if (enemy.Definition.Behavior == EnemyBehavior.Mimic && !enemy.Awakened) return false;
            Vector2 offset = Champion.Position - enemy.Position;
            if (offset.sqrMagnitude > 56.25f) return false;
            int charging = 0;
            foreach (UnitState other in StableUnits) if (other.Alive && (other.EvolutionReleaseAt > Elapsed || other.Variant != null && other.Variant.ReleaseAt > Elapsed)) charging++;
            if (charging >= 4) { enemy.NextEvolutionAttack = Elapsed + 0.4f + enemy.Id % 5 * 0.08f; return false; }
            enemy.NextEvolutionAttack = Elapsed + 7.5f + enemy.Id % 5 * 0.35f;
            enemy.EvolutionOrigin = enemy.Position;
            enemy.EvolutionTarget = ClampPosition(enemy.Position + Vector2.ClampMagnitude(offset, 4), enemy.Definition.Radius);
            enemy.AttackDirection = offset.sqrMagnitude > 0.001f ? offset.normalized : Vector2.down;
            enemy.AttackStarted = Elapsed; enemy.AttackWindup = 0.85f;
            enemy.EvolutionReleaseAt = enemy.TelegraphUntil = Elapsed + enemy.AttackWindup;
            enemy.NextAttack = Mathf.Max(enemy.NextAttack, enemy.EvolutionReleaseAt + 0.6f);
            enemy.NextBehavior = Mathf.Max(enemy.NextBehavior, enemy.EvolutionReleaseAt + 1.5f);
            events.Add(new CombatEvent(CombatEventKind.Windup, enemy.Position, Faction.Enemy, subject: enemy, weapon: enemy.Definition.Weapon));
            return true;
        }

        private bool AdvanceMutationTravel(UnitState enemy)
        {
            EnemyMutation kind = enemy.Definition.Mutation;
            float progress = Mathf.Clamp01((Elapsed - enemy.EvolutionTravelStarted) / Mathf.Max(0.01f, enemy.EvolutionTravelUntil - enemy.EvolutionTravelStarted));
            enemy.Position = ClampPosition(Vector2.Lerp(enemy.EvolutionOrigin, enemy.EvolutionTarget, Mathf.SmoothStep(0, 1, progress)), enemy.Definition.Radius);
            bool jumps = kind == EnemyMutation.SootPounce || kind == EnemyMutation.MimicLunge || kind == EnemyMutation.KegRebound;
            enemy.BodyLift = kind == EnemyMutation.BellEruption ? -Mathf.Sin(progress * Mathf.PI) * 0.5f : jumps ? Mathf.Sin(progress * Mathf.PI) * 0.85f : 0;
            if (progress < 1) return true;
            enemy.BodyLift = 0; enemy.EvolutionTravelStarted = enemy.EvolutionTravelUntil = -100;
            if (enemy.EvolutionRepeats > 0)
            {
                enemy.EvolutionRepeats--;
                Vector2 heading = enemy.AttackDirection;
                Vector2 target = kind == EnemyMutation.RamBackstep ? enemy.Position + heading * 4 : enemy.Position + Vector2.ClampMagnitude(Champion.Position - enemy.Position, 2.3f);
                BeginMutationTravel(enemy, target, kind == EnemyMutation.RamBackstep ? 0.65f : 0.8f);
                Danger(kind == EnemyMutation.RamBackstep ? ExpandedFieldKind.Tentacle : ExpandedFieldKind.Shockwave, enemy, enemy.EvolutionTarget, kind == EnemyMutation.RamBackstep ? 0.25f : 1.6f, kind == EnemyMutation.RamBackstep ? 0.15f : 0.8f, 0.6f);
            }
            else enemy.EvolutionRecoveryUntil = Elapsed + 0.45f;
            return true;
        }

        private void BeginMutationTravel(UnitState enemy, Vector2 target, float duration)
        {
            enemy.EvolutionOrigin = enemy.Position; enemy.EvolutionTarget = ClampPosition(target, enemy.Definition.Radius);
            enemy.EvolutionTravelStarted = Elapsed; enemy.EvolutionTravelUntil = Elapsed + duration;
            enemy.NextAttack = enemy.NextBehavior = Mathf.Max(enemy.NextBehavior, enemy.EvolutionTravelUntil + 0.7f);
        }

        private void MutationVolley(UnitState enemy, int count, float spread, HostileShot motion, bool radial = false)
        {
            if (catalog.RunWave <= 1) return;
            float aim = Mathf.Atan2(enemy.AttackDirection.y, enemy.AttackDirection.x);
            CombatCause cause = NewCause(enemy.Id).Child("mutation-volley");
            for (int index = 0; index < count && projectiles.Count < 4096; index++)
            {
                float angle = radial ? index * Mathf.PI * 2 / count + aim : aim + (index - (count - 1) * 0.5f) * spread;
                Vector2 heading = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 origin = ClampPosition(enemy.Position + heading * enemy.Definition.Radius * 0.8f, 0.05f);
                float speed = motion == HostileShot.Accelerate ? 2.2f : 1.8f;
                projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Enemy, SourceUnitId = enemy.Id, Position = origin, PreviousPosition = origin, Velocity = heading * speed, LaunchVelocity = heading * speed, HostileMotion = motion, Damage = enemy.Definition.Weapon.Damage, Remaining = 4.5f, Radius = 0.09f, VisualWidth = enemy.Definition.Mutation == EnemyMutation.FurnaceFan ? 0.38f : 0.27f, Sprite = EnemyEvolutionLibrary.Fragment(enemy.Definition, index), Color = new Color(1, 0.25f, 0.4f), SourceName = enemy.Definition.DisplayName + " mutation", Cause = cause });
            }
        }

        private void ResolveMutation(UnitState enemy)
        {
            Vector2 target = enemy.EvolutionTarget;
            Vector2 heading = enemy.AttackDirection;
            Vector2 side = new Vector2(-heading.y, heading.x);
            switch (enemy.Definition.Mutation)
            {
                case EnemyMutation.SootPounce:
                case EnemyMutation.MimicLunge:
                    BeginMutationTravel(enemy, target, 0.7f);
                    Danger(ExpandedFieldKind.Shockwave, enemy, target, enemy.Definition.Mutation == EnemyMutation.MimicLunge ? 1.9f : 1.4f, 0.7f, 0.8f);
                    break;
                case EnemyMutation.BatCrossfire:
                    BeginMutationTravel(enemy, enemy.Position + side * (enemy.Id % 2 == 0 ? 1.8f : -1.8f), 0.45f);
                    MutationVolley(enemy, 3, 0.28f, HostileShot.Hook);
                    break;
                case EnemyMutation.BristleRush:
                    BeginMutationTravel(enemy, target, 0.6f);
                    Danger(ExpandedFieldKind.Tentacle, enemy, target, 0.24f, 0.25f, 0.35f);
                    break;
                case EnemyMutation.RuneSpiral:
                    MutationVolley(enemy, 6, 0, HostileShot.Hook, true);
                    break;
                case EnemyMutation.MothMolting:
                    for (int index = -1; index <= 1; index += 2) Danger(ExpandedFieldKind.ShadowPool, enemy, target + side * index, 0.65f, 0.6f, 1.4f);
                    break;
                case EnemyMutation.SirenEcho:
                    Danger(ExpandedFieldKind.Shockwave, enemy, enemy.Position, 2.7f, 0.25f, 1.2f);
                    Danger(ExpandedFieldKind.Shockwave, enemy, enemy.Position, 2.2f, 1.05f, 1.2f);
                    break;
                case EnemyMutation.DoorSlam:
                    Danger(ExpandedFieldKind.Tentacle, enemy, enemy.Position + heading * 3, 0.5f, 0.3f, 0.35f);
                    break;
                case EnemyMutation.MagnetSalvo:
                    foreach (PickupState pickup in pickups)
                        if (pickup.Kind == LootKind.Coin && !pickup.Attracting && (pickup.Position - enemy.Position).sqrMagnitude < 9)
                            pickup.Position = Vector2.Lerp(pickup.Position, enemy.Position, 0.45f);
                    MutationVolley(enemy, 4, 0.2f, HostileShot.Weave);
                    break;
                case EnemyMutation.CurtainCollapse:
                    for (int index = 0; index < 3; index++) Danger(ExpandedFieldKind.FallingProp, enemy, target + side * ((index - 1) * 1.3f), 0.65f, 0.6f + index * 0.2f, 0.35f);
                    break;
                case EnemyMutation.FurnaceFan:
                    MutationVolley(enemy, 5, 0.22f, HostileShot.Accelerate);
                    break;
                case EnemyMutation.CabinetScissor:
                    Danger(ExpandedFieldKind.Tentacle, enemy, target + side * 1.2f, 0.32f, 0.35f, 0.35f);
                    Danger(ExpandedFieldKind.Tentacle, enemy, target - side * 1.2f, 0.32f, 0.7f, 0.35f);
                    break;
                case EnemyMutation.KegRebound:
                    enemy.EvolutionRepeats = 1; BeginMutationTravel(enemy, target, 0.8f);
                    Danger(ExpandedFieldKind.Shockwave, enemy, target, 1.6f, 0.8f, 0.8f);
                    break;
                case EnemyMutation.RamBackstep:
                    enemy.EvolutionRepeats = 1; BeginMutationTravel(enemy, enemy.Position + heading * 1.1f, 0.35f);
                    break;
                case EnemyMutation.CoinBarrage:
                    for (int index = pickups.Count - 1; index >= 0; index--)
                    {
                        PickupState coin = pickups[index];
                        if (coin.Kind != LootKind.Coin || coin.Attracting || coin.Value <= 0 || coin.Value > 1000 - enemy.StolenCoins || (coin.Position - enemy.Position).sqrMagnitude > 9) continue;
                        enemy.StolenCoins += coin.Value; pickups.RemoveAt(index);
                    }
                    if (enemy.StolenCoins > 0) MutationVolley(enemy, Mathf.Min(5, 2 + enemy.StolenCoins / 3), 0.23f, HostileShot.Drift);
                    break;
                case EnemyMutation.PufferEcho:
                    Danger(ExpandedFieldKind.Shockwave, enemy, enemy.Position, 3, 0.25f, 1.1f);
                    Danger(ExpandedFieldKind.Shockwave, enemy, enemy.Position, 1.7f, 1.1f, 0.8f);
                    break;
                case EnemyMutation.SporeGarden:
                    for (int index = 0; index < 3; index++)
                    {
                        Vector2 offset = Quaternion.Euler(0, 0, index * 120) * Vector2.right * 1.25f;
                        Danger(ExpandedFieldKind.ShadowPool, enemy, target + offset, 0.65f, 0.65f + index * 0.18f, 1.6f);
                    }
                    break;
                case EnemyMutation.CombRake:
                    for (int index = -1; index <= 1; index++) Danger(ExpandedFieldKind.Tentacle, enemy, target + side * index * 1.1f, 0.2f, 0.3f + (index + 1) * 0.2f, 0.35f);
                    break;
                case EnemyMutation.HydraWeave:
                    MutationVolley(enemy, 5, 0.21f, HostileShot.Weave);
                    break;
                case EnemyMutation.BellEruption:
                    BeginMutationTravel(enemy, target, 0.9f);
                    Danger(ExpandedFieldKind.Shockwave, enemy, target, 2.3f, 0.9f, 1.1f);
                    break;
            }
        }
    }
}
