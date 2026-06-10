using UnityEngine;

namespace ArcaneBrigade
{
    public static class CarnivalHazards
    {
        public static bool IsHostile(ExpandedFieldKind kind) => kind == ExpandedFieldKind.OfficeHazard || kind == ExpandedFieldKind.ToyHazard || kind == ExpandedFieldKind.Curtain || kind == ExpandedFieldKind.Tentacle || kind == ExpandedFieldKind.Shockwave || kind == ExpandedFieldKind.FallingProp || kind == ExpandedFieldKind.ShadowPool || kind == ExpandedFieldKind.Roller;

        public static float ShockRadius(ExpandedField field, float time) => field.Radius * Mathf.Clamp01((time - field.At) / Mathf.Max(0.05f, field.Until - field.At));

        public static bool Contains(ExpandedField field, Vector2 position, float bodyRadius, float time)
        {
            if (field.Kind == ExpandedFieldKind.Tentacle)
            {
                Vector2 segment = field.Position - field.Start;
                float fraction = segment.sqrMagnitude < 0.0001f ? 0 : Mathf.Clamp01(Vector2.Dot(position - field.Start, segment) / segment.sqrMagnitude);
                return Vector2.Distance(position, field.Start + segment * fraction) <= field.Radius + bodyRadius;
            }
            if (field.Kind == ExpandedFieldKind.Roller)
            {
                if (field.Damage <= 0 || time < field.At) return false;
                if (field.Motion == TrademarkMotion.Rise && time > field.Until - 0.05f) return false;
                if (field.Length <= 0) return Vector2.Distance(position, field.Position) <= field.Radius + bodyRadius;
                BossTrademarks.Bar(field, out Vector2 a, out Vector2 b);
                Vector2 bar = b - a;
                float along = Mathf.Clamp01(Vector2.Dot(position - a, bar) / Mathf.Max(0.0001f, bar.sqrMagnitude));
                return Vector2.Distance(position, a + bar * along) <= field.Radius + bodyRadius;
            }
            if (field.Kind == ExpandedFieldKind.Shockwave)
                return Mathf.Abs(Vector2.Distance(position, field.Position) - ShockRadius(field, time)) <= 0.22f + bodyRadius;
            return Vector2.Distance(position, field.Position) <= field.Radius + bodyRadius;
        }
    }

    public sealed partial class CombatSimulation
    {
        private void Danger(ExpandedFieldKind kind, UnitState source, Vector2 destination, float radius, float delay, float duration, int organ = -1)
        {
            // no lingering pools on the floor (requirement 19/10/2026): what used to spread there lands once as a dropped blow and is gone
            if (kind == ExpandedFieldKind.ShadowPool) { kind = ExpandedFieldKind.FallingProp; duration = Mathf.Min(duration, .4f); }
            if (!BossPartRules.Allows(source, kind)) return;
            if (source.Definition.IsBoss && (BossPartRules.Has(source, BossOrganRole.Pulse, true) || BossAnatomy.For(source.Definition) == null && source.RightPartBroken) && kind == ExpandedFieldKind.Shockwave) radius *= 0.8f;
            int count = 0;
            foreach (ExpandedField field in fields) if (CarnivalHazards.IsHostile(field.Kind)) count++;
            if (count >= 48) return;
            Field(kind, source, ClampPosition(destination, radius), (destination - source.Position).normalized, radius, source.Definition.Weapon.Damage, delay, duration, NewCause(source.Id));
            if (organ >= 0)
            {
                ExpandedField field = fields[fields.Count - 1]; field.SourceOrgan = organ; field.Start = BossPartRules.Position(source, organ);
                field.Direction = (field.Position - field.Start).normalized;
            }
        }

        private bool ResolveBossBodyAttack(UnitState boss)
        {
            if (BossAnatomy.For(boss.Definition) != null) return ResolveAnatomyAttack(boss);
            if (boss.LeftPartBroken && boss.RightPartBroken)
            {
                Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 4.5f, 0.9f, 1.4f);
                return false;
            }
            int pattern = boss.Definition.BossPattern;
            if (pattern < 5 && boss.BossVolley % 3 != 2) return false;
            Vector2 target = ClampPosition(Champion.Position, 1.2f);
            Vector2 direction = (target - boss.Position).normalized;
            switch (pattern % 5)
            {
                case 0:
                    Danger(ExpandedFieldKind.Tentacle, boss, boss.Position + direction * 5, 0.42f, 0.65f, 0.45f);
                    for (int index = 0; index < 3; index++)
                        Danger(ExpandedFieldKind.FallingProp, boss, target + new Vector2((index - 1) * 1.6f, 0), 0.8f, 0.9f + index * 0.2f, 0.4f);
                    break;
                case 1:
                    for (int index = 0; index < 3; index++)
                    {
                        Vector2 lash = (Vector2)(Quaternion.Euler(0, 0, (index - 1) * 28) * direction);
                        Danger(ExpandedFieldKind.Tentacle, boss, boss.Position + lash * 6, 0.32f, 0.8f + index * 0.16f, 0.5f);
                    }
                    Danger(ExpandedFieldKind.ShadowPool, boss, target, 1.1f, 1.3f, 1.6f);
                    break;
                case 2:
                    foreach (PickupState pickup in pickups)
                        if (pickup.Kind == LootKind.Coin && (pickup.Position - boss.Position).sqrMagnitude < 36)
                            pickup.Position = Vector2.Lerp(pickup.Position, boss.Position, 0.35f);
                    Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 5, 0.8f, 1.4f);
                    break;
                case 3:
                    Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 6, 0.6f, 1.8f);
                    Danger(ExpandedFieldKind.Shockwave, boss, boss.Position, 6, 1.4f, 1.8f);
                    break;
                default:
                    for (int index = 0; index < 5; index++)
                    {
                        Vector2 offset = (Vector2)(Quaternion.Euler(0, 0, index * 72) * Vector2.right) * 2;
                        Danger(ExpandedFieldKind.FallingProp, boss, target + offset, 0.7f, 0.85f + index * 0.15f, 0.45f);
                    }
                    Danger(ExpandedFieldKind.Tentacle, boss, target, 0.4f, 0.7f, 0.4f);
                    break;
            }
            return pattern != 7;
        }
    }
}
