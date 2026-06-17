using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A boss's attack ranges as SOLID painted areas (requirement 24/10/2026: a deep red ground over the whole area, filled
    /// brighter from the centre as the blow charges, closed by an inked rim, with red cracks over it; a faint outline over an empty
    /// floor did not read):
    /// <list type="bullet">
    /// <item>A circle (a slam, a falling piece, a shockwave) is a brushed dashed rim at the exact radius that hurts; the dashes
    /// lengthen as the blow nears and the rim closes when it lands.</item>
    /// <item>A line or a charge (a lash, an aimed fan) is two inked lane edges with an arrowhead, dashed the same way.</item>
    /// <item>During the boss's own wind-up: an aimed volley shows its fan's edges, a ring volley a rim round the painted body,
    /// a signature or a Keeper/Customer blow a rim where it will land.</item>
    /// </list>
    /// The warning lasts exactly as long as the move's own wind-up, so a quick blow flashes and a big one builds. Dark ink under a
    /// bright red body reads on every floor. Reduced motion keeps the marks and drops the pulse.</summary>
    public sealed class BossTelegraph : MonoBehaviour
    {
        private ArsenalInk ink;
        private bool drawn;
        public static readonly InkPalette Danger = new InkPalette(new Color(.16f, .02f, .04f), new Color(.93f, .13f, .14f), new Color(1, .74f, .64f), new Color(1, .32f, .26f), new Color(1, .82f, .42f));
        public static readonly InkPalette DangerHot = new InkPalette(new Color(.2f, .03f, .03f), new Color(1, .42f, .26f), new Color(1, .93f, .78f), new Color(1, .6f, .35f), new Color(1, .9f, .5f));
        /// <summary>Captures and checks: also draw each boss's contact ellipse (the painted body it hurts from).</summary>
        public static bool ShowBodyShapes;
        // what the last frame showed (read by checks)
        public int Windups { get; private set; }
        public int Rims { get; private set; }
        public int Lanes { get; private set; }
        public int Marks { get; private set; }
        public int Bodies { get; private set; }

        public void Display(CombatSimulation simulation)
        {
            Windups = Rims = Lanes = Marks = Bodies = 0;
            bool any = false;
            bool live = simulation != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (live)
            {
                float now = simulation.Elapsed;
                foreach (UnitState unit in simulation.Units)
                    if (unit.Definition.IsBoss && unit.Alive && (unit.TelegraphUntil > now || ShowBodyShapes)) { any = true; break; }
                if (!any) foreach (ExpandedField field in simulation.ExpandedFields) if (Warns(field, now)) { any = true; break; }
            }
            if (!any) { if (drawn) { ink.Begin(); ink.End(); drawn = false; } return; }
            if (ink == null) ink = new ArsenalInk(transform);
            ink.Begin(); drawn = true;
            float time = simulation.Elapsed;
            foreach (UnitState unit in simulation.Units)
            {
                if (!unit.Definition.IsBoss || !unit.Alive) continue;
                if (unit.TelegraphUntil > time) DrawWindup(unit, time);
                if (ShowBodyShapes) DrawBody(unit);
            }
            foreach (ExpandedField field in simulation.ExpandedFields) if (Warns(field, time)) DrawField(field, time);
            ink.End();
        }

        /// <summary>A boss's hostile area still in its warning (the trademark pieces are drawn by BossTrademarkVisual).</summary>
        private static bool Warns(ExpandedField field, float now) =>
            field.Source != null && field.Source.Definition.IsBoss && field.Kind != ExpandedFieldKind.Roller && CarnivalHazards.IsHostile(field.Kind) && now < field.At;

        private static float Charge(float start, float end, float now) => Mathf.Clamp01((now - start) / Mathf.Max(.05f, end - start));

        private void DrawWindup(UnitState boss, float now)
        {
            // the Last Customer and the Keeper wind up their own blows (each lands as areas at the target)
            bool special = boss.Definition.Id == NightmareRules.CustomerId || boss.Definition.Id == "OutsideStageKeeper";
            float windup = special ? Mathf.Max(.1f, boss.AttackWindup) : Mathf.Max(.1f, BossEncounterRules.Windup(boss));
            float charge = Mathf.Clamp01(1 - (boss.TelegraphUntil - now) / windup);
            float seed = boss.Id * 37 % 360;
            Windups++;
            if (boss.ActiveTrademark) return; // its path is inked by BossTrademarkVisual
            if (special || boss.ActiveBossSignature >= 0)
            {
                // where the blow will land: a rim gathering at the target
                Rim(ink, boss.AbilityTarget, 1.1f, 1.1f, charge, seed + 90, .9f);
                Marks++;
                return;
            }
            if (Aimed(boss.Definition.BossPattern) && boss.TelegraphDirection.sqrMagnitude > .01f)
            {
                // an aimed fan: its two edges and an arrowhead along the aim
                BossBodyShape.Shape body = BossBodyShape.Of(boss.Definition);
                Vector2 aim = boss.TelegraphDirection.normalized, origin = boss.Position + aim * Mathf.Max(.5f, body.Half.x * .9f);
                float half = HalfSpread(boss) + .06f, reach = 5.6f;
                FanGround(ink, origin, aim, half, reach, charge, .95f);
                Arrow(ink, origin + aim * (reach * .92f), aim, .55f + .25f * charge, charge, .95f);
                Lanes++;
                return;
            }
            // a ring volley bursts out of the body: a rim round the painted body
            BossBodyShape.Shape shape = BossBodyShape.Of(boss.Definition);
            Vector2 c = BossBodyShape.Centre(boss, shape);
            Rim(ink, c, shape.Half.x / BossBodyShape.Share + .35f, shape.Half.y / BossBodyShape.Share + .35f, charge, seed, .9f);
            Rims++;
        }

        private void DrawField(ExpandedField field, float now)
        {
            float charge = Charge(field.Created, field.At, now), seed = field.Position.x * 13 + field.Position.y * 7;
            switch (field.Kind)
            {
                case ExpandedFieldKind.Tentacle:
                {
                    Vector2 a = field.Start, b = field.Position, axis = b - a;
                    if (axis.sqrMagnitude < .0025f) { Rim(ink, b, field.Radius + .15f, field.Radius + .15f, charge, seed, 1); Rims++; break; }
                    Vector2 dir = axis.normalized, n = new Vector2(-dir.y, dir.x) * (field.Radius + .12f);
                    LaneGround(ink, a, b, field.Radius + .12f, charge, 1);
                    Arrow(ink, b, dir, field.Radius + .35f, charge, 1);
                    Lanes++;
                    break;
                }
                case ExpandedFieldKind.Shockwave:
                    // the ring that will run out to this radius: its outer rim, and a faint inner one where it starts
                    Rim(ink, field.Position, field.Radius, field.Radius, charge, seed, 1);
                    Rim(ink, field.Position, field.Radius * .45f, field.Radius * .45f, charge, seed + 30, .45f);
                    Rims++;
                    break;
                default:
                    Rim(ink, field.Position, field.Radius + .08f, field.Radius + .08f, charge, seed, 1);
                    Rims++;
                    break;
            }
        }

        /// <summary>The painted body a boss hurts from (captures): its contact ellipse as a thin dashed amber line.</summary>
        private void DrawBody(UnitState boss)
        {
            BossBodyShape.Shape shape = BossBodyShape.Of(boss.Definition);
            if (!shape.Valid) return;
            Vector2 c = BossBodyShape.Centre(boss, shape);
            for (int i = 0; i < 24; i++) ink.FloorArc(c, shape.Half.x, shape.Half.y, i * 15, i * 15 + 9, DangerHot, .09f, 1, true);
            Bodies++;
        }

        // ------------------------------------------------------------------ the shared marks (BossTrademarkVisual uses them too)
        // requirement 24/10/2026: a zone is a solid painted area, never a faint outline over an empty floor - a deep red ground
        // fills the whole area at once, a brighter red fills it from the centre as the blow charges, and a solid inked rim closes it
        private static readonly Color ZoneGround = new Color(.42f, .04f, .07f), ZoneCharge = new Color(.95f, .2f, .14f);
        private static void Ground(ArsenalInk ink, Vector2 c, float rx, float ry, float charge, float alpha)
        {
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            ink.FloorZone(c, rx, ry, new Color(ZoneGround.r, ZoneGround.g, ZoneGround.b, .42f * alpha), new Color(ZoneGround.r, ZoneGround.g, ZoneGround.b, .62f * alpha));
            if (s > .02f) ink.FloorZone(c, rx * s, ry * s, new Color(ZoneCharge.r, ZoneCharge.g, ZoneCharge.b, .22f * alpha), new Color(ZoneCharge.r, ZoneCharge.g, ZoneCharge.b, .45f * alpha));
        }
        /// <summary>A lane's solid ground: the deep red band, filled bright from its start as the blow charges.</summary>
        public static void LaneGround(ArsenalInk ink, Vector2 a, Vector2 b, float halfWidth, float charge, float alpha)
        {
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            ink.FloorLane(a, b, halfWidth, new Color(ZoneGround.r, ZoneGround.g, ZoneGround.b, .5f * alpha), new Color(ZoneGround.r, ZoneGround.g, ZoneGround.b, .62f * alpha));
            if (s > .02f) ink.FloorLane(a, Vector2.Lerp(a, b, s), halfWidth * .92f, new Color(ZoneCharge.r, ZoneCharge.g, ZoneCharge.b, .4f * alpha), new Color(ZoneCharge.r, ZoneCharge.g, ZoneCharge.b, .25f * alpha));
        }
        /// <summary>An aimed fan's solid ground: the wedge it will sweep, filled bright from the apex as it charges.</summary>
        public static void FanGround(ArsenalInk ink, Vector2 apex, Vector2 aim, float half, float reach, float charge, float alpha)
        {
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge)), mid = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg, spread = half * Mathf.Rad2Deg;
            ink.Cone(apex, reach, mid - spread, mid + spread, ZoneGround, .62f * alpha, .4f * alpha);
            if (s > .02f) ink.Cone(apex, reach * s, mid - spread, mid + spread, ZoneCharge, .4f * alpha, .2f * alpha);
        }
        /// <summary>Where a blow will land: the solid ground, and cracks running out from the centre to the area's true edge as the
        /// blow charges, a hot spark at each tip in the last beat.</summary>
        public static void Rim(ArsenalInk ink, Vector2 c, float rx, float ry, float charge, float seed, float alpha)
        {
            Ground(ink, c, rx, ry, charge, alpha);
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            int cracks = Mathf.Clamp(Mathf.RoundToInt((rx + ry) * 3), 6, 14);
            float width = Mathf.Clamp((rx + ry) * .03f, .06f, .12f) * Mathf.Lerp(.85f, 1.2f, s), a = alpha * Mathf.Lerp(.6f, 1, s);
            for (int i = 0; i < cracks; i++)
            {
                float ang = (seed + i * 360f / cracks + Mathf.Sin(i * 2.3f) * 10) * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)), perp = new Vector2(-dir.y, dir.x), end = c + new Vector2(dir.x * rx, dir.y * ry) * Mathf.Lerp(.3f, 1, s);
                Vector2 prev = c + new Vector2(dir.x * rx, dir.y * ry) * .12f;
                for (int k = 1; k <= 3; k++)
                {
                    Vector2 p = Vector2.Lerp(c, end, k / 3f) + perp * (((i * 7 + k * 3) % 5 - 2) * .025f * (rx + ry)) * (k < 3 ? 1 : 0);
                    ink.FloorStroke(prev, p, Danger, width * (1.15f - k * .15f), 1, 0, a, 0, seed + i * 3 + k); prev = p;
                }
                if (s > .7f) ink.FloorStroke(end - perp * .1f, end + perp * .1f, DangerHot, width * .7f, 1, 0, a * (s - .7f) / .3f);
            }
        }
        /// <summary>A lane's edge as a jagged crack that runs its whole length as the blow charges (no dashes).</summary>
        public static void DashedLine(ArsenalInk ink, Vector2 a, Vector2 b, float charge, float seed, float alpha)
        {
            float length = Vector2.Distance(a, b); if (length < .05f) return;
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            Vector2 dir = (b - a) / length, perp = new Vector2(-dir.y, dir.x), reach = a + (b - a) * Mathf.Lerp(.3f, 1, s);
            int pieces = Mathf.Clamp(Mathf.RoundToInt(length * 1.6f), 3, 12);
            float width = Mathf.Lerp(.07f, .11f, s), alpha2 = alpha * Mathf.Lerp(.6f, 1, s);
            Vector2 prev = a;
            for (int i = 1; i <= pieces; i++)
            {
                Vector2 p = Vector2.Lerp(a, reach, i / (float)pieces) + perp * ((((int)seed + i * 7) % 5 - 2) * .04f) * (i < pieces ? 1 : 0);
                ink.FloorStroke(prev, p, Danger, width, 1, 0, alpha2, 0, seed + i); prev = p;
            }
        }
        /// <summary>An inked arrowhead at the far end of a lane.</summary>
        public static void Arrow(ArsenalInk ink, Vector2 tip, Vector2 direction, float size, float charge, float alpha)
        {
            Vector2 n = new Vector2(-direction.y, direction.x);
            float a = alpha * Mathf.Lerp(.6f, 1, Mathf.Clamp01(charge));
            InkPalette pal = charge > .8f ? DangerHot : Danger;
            ink.FloorStroke(tip - direction * size + n * size * .8f, tip, pal, .11f, 1, 0, a);
            ink.FloorStroke(tip - direction * size - n * size * .8f, tip, pal, .11f, 1, 0, a);
        }

        private static bool Aimed(int pattern) => pattern == 1 || pattern == 3 || pattern == 5 || pattern == 7 || pattern == 9;
        /// <summary>Half the angle the fan will cover (radians), from the same rules that fire it.</summary>
        private static float HalfSpread(UnitState boss)
        {
            int count = BossPartRules.BulletCount(boss);
            switch (boss.Definition.BossPattern)
            {
                case 5: return (count - 1) * 0.5f * 0.095f;
                case 7: return 2 * 0.17f + (count - 1) / 5 * 0.08f;
                case 9: return 0.8f;
                default: return count / 2 * 0.16f;
            }
        }
        private static Vector2 Rotate(Vector2 v, float degrees) => Quaternion.Euler(0, 0, degrees) * v;

        private void OnDestroy() { ink?.Destroy(); }
    }
}
