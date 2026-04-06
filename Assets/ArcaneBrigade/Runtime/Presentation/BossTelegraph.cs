using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A boss's attack ranges, painted as red ink OUTLINES only (play-test item 22, decision D10; no fills, no pools):
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
                for (int side = -1; side <= 1; side += 2) DashedLine(ink, origin, origin + Rotate(aim, side * half * Mathf.Rad2Deg) * reach, charge, seed + side, .95f);
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
                    DashedLine(ink, a + n, b + n, charge, seed, 1); DashedLine(ink, a - n, b - n, charge, seed + 1, 1);
                    ink.FloorStroke(a + n, a - n, Danger, .09f, 1, 0, .8f, 0, seed + 2);
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
        /// <summary>A brushed dashed rim (outline only): the dashes grow with the charge and close into one rim as the blow lands;
        /// in the last beat a hot inner line runs round it.</summary>
        public static void Rim(ArsenalInk ink, Vector2 c, float rx, float ry, float charge, float seed, float alpha)
        {
            bool still = PresentationSettings.ReducedMotion;
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            int dashes = Mathf.Clamp(Mathf.RoundToInt((rx + ry) * 4.5f), 10, 30);
            float slot = 360f / dashes, fill = Mathf.Lerp(.42f, 1.04f, s), spin = still ? 0 : seed + (1 - s) * 40;
            float width = Mathf.Clamp((rx + ry) * .035f, .07f, .14f) * Mathf.Lerp(.85f, 1.25f, s);
            float a = alpha * Mathf.Lerp(.6f, 1, s);
            for (int i = 0; i < dashes; i++) { float from = spin + i * slot; ink.FloorArc(c, rx, ry, from, from + slot * Mathf.Min(fill, 1.04f), Danger, width, a); }
            if (s > .8f) ink.FloorArc(c, rx * .985f, ry * .985f, 0, 360, DangerHot, width * .45f, a * (s - .8f) / .2f);
        }
        /// <summary>A dashed lane edge that closes the same way.</summary>
        public static void DashedLine(ArsenalInk ink, Vector2 a, Vector2 b, float charge, float seed, float alpha)
        {
            float length = Vector2.Distance(a, b); if (length < .05f) return;
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            int dashes = Mathf.Clamp(Mathf.RoundToInt(length * 1.7f), 3, 16);
            float fill = Mathf.Lerp(.42f, 1.02f, s), width = Mathf.Lerp(.07f, .11f, s), alpha2 = alpha * Mathf.Lerp(.6f, 1, s);
            for (int i = 0; i < dashes; i++)
            {
                float f0 = i / (float)dashes, f1 = Mathf.Min(1, (i + fill) / dashes);
                ink.FloorStroke(Vector2.Lerp(a, b, f0), Vector2.Lerp(a, b, f1), Danger, width, 1, 0, alpha2, 0, seed + i);
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
