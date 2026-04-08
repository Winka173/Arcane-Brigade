using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>How a trademark piece travels once it is released.</summary>
    public enum TrademarkMotion { Straight, Return, Bounce, Fuse, Home, Arc, Orbit, Rise }

    /// <summary>Each of the ten bosses' own move (19/10/2026): every fourth volley (the second of each four) the boss sends
    /// something that belongs to it across the floor instead of a plain ring or fan - letters that come back, sheep that
    /// cross, a wax tide with one gap, a spotlight that follows you, a pendulum, coins that bounce, an organ scale, a luggage
    /// carousel. Each piece is a moving hazard (ExpandedFieldKind.Roller) with its path inked on the floor during the
    /// wind-up, and each leaves a way through. Painted BossProp_* art draws the pieces once it exists (BossTrademarkVisual).</summary>
    public static class BossTrademarks
    {
        public static string Name(int pattern) => pattern switch
        {
            0 => "Return to Sender", 1 => "Redaction", 2 => "Counting Sheep", 3 => "Wax Tide", 4 => "Lit Fuse",
            5 => "Follow Spot", 6 => "Rocking Pendulum", 7 => "Rolling Change", 8 => "Pipe Scale", 9 => "Baggage Carousel", _ => null
        };
        /// <summary>The painted piece for a style (ArsenalArtwork name), null for pieces drawn in ink only (bars, sparks, light).</summary>
        public static string Prop(int style) => style switch
        {
            0 => "BossProp_Letter", 2 => "BossProp_Sheep", 3 => "BossProp_WaxCrest", 6 => "BossProp_Pendulum",
            7 => "BossProp_Coin", 8 => "BossProp_OrganPipe", 9 => "BossProp_Suitcase", _ => null
        };
        /// <summary>The second volley of every four (signatures take the fourth).</summary>
        public static bool Fires(UnitDefinition boss, int volley) => boss.IsBoss && Name(boss.BossPattern) != null && volley % 4 == 1;
        /// <summary>Where a roller is at a time (pure; the simulation integrates Bounce and Home itself).</summary>
        public static Vector2 Along(ExpandedField f, float t)
        {
            t = Mathf.Max(0, t);
            switch (f.Motion)
            {
                case TrademarkMotion.Return: { float d = t < f.Phase ? t : Mathf.Max(0, 2 * f.Phase - t); return f.Start + f.Direction * f.Speed * d; }
                case TrademarkMotion.Fuse: return f.Start + f.Direction * f.Speed * Mathf.Min(t, f.Phase);
                case TrademarkMotion.Arc: { float swing = f.Phase * Mathf.Cos(t * f.Speed); return f.Center + new Vector2(Mathf.Sin(swing), -Mathf.Cos(swing)) * f.Reach; }
                case TrademarkMotion.Orbit: { float angle = f.Phase + t * f.Speed; return f.Center + new Vector2(Mathf.Cos(angle) * f.Reach, Mathf.Sin(angle) * f.Reach * .62f); }
                case TrademarkMotion.Rise: return f.Start;
                default: return f.Start + f.Direction * f.Speed * t;
            }
        }
        /// <summary>A bar piece (Length &gt; 0) lies across its direction of travel.</summary>
        public static void Bar(ExpandedField f, out Vector2 a, out Vector2 b)
        {
            Vector2 across = new Vector2(-f.Direction.y, f.Direction.x) * f.Length * .5f;
            a = f.Position - across; b = f.Position + across;
        }
    }

    public sealed partial class CombatSimulation
    {
        /// <summary>Adds one moving piece; false when the floor already holds too many hostile marks.</summary>
        private bool Roller(UnitState boss, int style, TrademarkMotion motion, Vector2 start, Vector2 direction, float speed, float radius, float delay, float duration, float phase = 0, float length = 0, Vector2 center = default, float reach = 0, float damageScale = 1)
        {
            int count = 0;
            foreach (ExpandedField field in fields) if (CarnivalHazards.IsHostile(field.Kind)) count++;
            if (count >= 48) return false;
            Field(ExpandedFieldKind.Roller, boss, start, direction.sqrMagnitude > .0001f ? direction.normalized : Vector2.down, radius, boss.Definition.Weapon.Damage * damageScale, delay, duration, NewCause(boss.Id));
            ExpandedField f = fields[fields.Count - 1];
            f.Start = start; f.Style = style; f.Motion = motion; f.Speed = speed; f.Phase = phase; f.Length = length; f.Center = center; f.Reach = reach; f.Moved = f.At;
            f.Position = BossTrademarks.Along(f, 0);
            return true;
        }

        /// <summary>Moves a roller for this step (before its damage tick).</summary>
        private void MoveRoller(ExpandedField f)
        {
            if (Elapsed < f.At) { f.Position = BossTrademarks.Along(f, 0); return; }
            float delta = Mathf.Max(0, Elapsed - f.Moved); f.Moved = Elapsed;
            switch (f.Motion)
            {
                case TrademarkMotion.Bounce:
                {
                    Vector2 next = f.Position + f.Direction * f.Speed * delta, kept = ClampPosition(next, f.Radius);
                    if (!Mathf.Approximately(kept.x, next.x)) f.Direction.x = -f.Direction.x;
                    if (!Mathf.Approximately(kept.y, next.y)) f.Direction.y = -f.Direction.y;
                    f.Position = kept; break;
                }
                case TrademarkMotion.Home:
                    if (Elapsed - f.At < f.Phase) { if (Champion != null && Champion.Alive) f.Position = ClampPosition(Vector2.MoveTowards(f.Position, Champion.Position, f.Speed * delta), f.Radius); }
                    else if (!f.Spent) { f.Spent = true; Danger(ExpandedFieldKind.FallingProp, f.Source, f.Position, 1.05f, .4f, .4f); }
                    break;
                case TrademarkMotion.Fuse:
                    f.Position = BossTrademarks.Along(f, Elapsed - f.At);
                    if (Elapsed - f.At >= f.Phase && !f.Spent) { f.Spent = true; Danger(ExpandedFieldKind.FallingProp, f.Source, f.Position, .95f, .05f, .35f); }
                    break;
                default: f.Position = BossTrademarks.Along(f, Elapsed - f.At); break;
            }
        }

        private void ResolveBossTrademark(UnitState boss)
        {
            int pattern = boss.Definition.BossPattern;
            Vector2 target = ClampPosition(Champion.Position, 1.2f);
            Vector2 heading = target - boss.Position; heading = heading.sqrMagnitude > .01f ? heading.normalized : Vector2.down;
            Vector2 across = new Vector2(-heading.y, heading.x);
            bool enraged = BossEnraged(boss);
            float pace = enraged ? 1.15f : 1;
            int cast = boss.BossVolley / 4, side = cast % 2 == 0 ? 1 : -1;
            const float Lead = .75f; // the inked path shows this long before the piece moves
            switch (pattern)
            {
                case 0: // Return to Sender: three letters fly out past you and come back the same way
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 aim = target + across * i * 1.7f - boss.Position;
                        float outward = (aim.magnitude + 3) / (7 * pace);
                        Roller(boss, 0, TrademarkMotion.Return, boss.Position, aim, 7 * pace, .42f, Lead + Mathf.Abs(i) * .18f, outward * 2 + .1f, outward);
                    }
                    break;
                case 1: // Redaction: two long ink bars sweep over the floor, each leaving one side open
                    for (int i = 0; i < 2; i++)
                    {
                        float open = (i == 0 ? side : -side) * 1.9f;
                        Roller(boss, 1, TrademarkMotion.Straight, target - heading * 5.5f + across * open, heading, 3.4f * pace, .26f, Lead + i * 1.15f, 11 / (3.4f * pace), length: 8);
                    }
                    break;
                case 2: // Counting Sheep: a line of sheep crosses your path one after another
                    for (int i = 0; i < (enraged ? 6 : 5); i++)
                        Roller(boss, 2, TrademarkMotion.Straight, target - across * side * 7.5f, across * side, 4.6f * pace, .48f, Lead + i * .9f, 15 / (4.6f * pace));
                    break;
                case 3: // Wax Tide: a wall of melting wax rolls toward you with one gap in it
                {
                    int gap = cast % 5 - 2;
                    for (int slot = -2; slot <= 2; slot++)
                        if (slot != gap) Roller(boss, 3, TrademarkMotion.Straight, target - heading * 6 + across * slot * 2.3f, heading, 2.7f * pace, .34f, Lead, 12 / (2.7f * pace), length: 2.1f);
                    break;
                }
                case 4: // Lit Fuse: sparks run along three fuses and pop where they end
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 end = target + across * i * 2.2f + heading * (i == 0 ? .8f : -.4f), run = end - boss.Position;
                        float travel = run.magnitude / (6 * pace);
                        Roller(boss, 4, TrademarkMotion.Fuse, boss.Position, run, 6 * pace, .28f, Lead + Mathf.Abs(i) * .25f, travel + .5f, travel);
                    }
                    break;
                case 5: // Follow Spot: the stage light follows you, then a sandbag drops where it stops (the light itself is harmless)
                    for (int i = 0; i < (enraged ? 2 : 1); i++)
                        Roller(boss, 5, TrademarkMotion.Home, boss.Position + across * (i == 0 ? 0 : side * 2), heading, 3.1f * pace, .9f, Lead, 3.1f, 2.4f + i * .6f, damageScale: 0);
                    break;
                case 6: // Rocking Pendulum: a shadow weight swings across the floor on a long cord
                {
                    int count = enraged ? 2 : 1;
                    for (int i = 0; i < count; i++)
                    {
                        Vector2 pivot = target + Vector2.up * 4.6f + across * (count == 1 ? 0 : (i - .5f) * 3.2f);
                        Roller(boss, 6, TrademarkMotion.Arc, pivot, Vector2.down, 1.9f * pace, .75f, Lead, 4.2f, (i == 0 ? 1.15f : -1.15f) * side, center: pivot, reach: 4.6f);
                    }
                    break;
                }
                case 7: // Rolling Change: big coins roll out and bounce off the walls
                    for (int i = 0; i < (enraged ? 5 : 4); i++)
                    {
                        Vector2 aim = Quaternion.Euler(0, 0, (i - (enraged ? 2 : 1.5f)) * 24) * heading;
                        Roller(boss, 7, TrademarkMotion.Bounce, boss.Position + aim * 1.2f, aim, 5.4f * pace, .45f, Lead + i * .12f, 4);
                    }
                    break;
                case 8: // Pipe Scale: organ pipes rise in a row, up the scale and back down; the gap walks with the music
                    for (int note = 0; note < 7; note++)
                    {
                        Vector2 at = target + across * (note - 3) * 1.15f;
                        Roller(boss, 8, TrademarkMotion.Rise, at, heading, 0, .5f, Lead + note * .28f, .5f);
                        Roller(boss, 8, TrademarkMotion.Rise, at, heading, 0, .5f, Lead + 2.4f + (6 - note) * .28f, .5f);
                    }
                    break;
                case 9: // Baggage Carousel: suitcases ride an oval belt around you; one place on the belt is empty
                {
                    int bags = enraged ? 6 : 5;
                    for (int i = 0; i < bags; i++)
                        Roller(boss, 9, TrademarkMotion.Orbit, target, heading, 1.05f * pace * side, .5f, Lead, 4.6f, i * Mathf.PI * 2 / 6, center: target, reach: 3);
                    break;
                }
            }
        }
    }
}
