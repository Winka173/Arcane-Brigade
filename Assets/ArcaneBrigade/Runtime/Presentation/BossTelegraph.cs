using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A boss's wind-up, painted in ink on the floor (18/10/2026), so the player reads when and where the blow comes:
    /// <list type="bullet">
    /// <item>Wine-dark ink wells up under the boss and spreads as the wind-up fills; near the end it boils and droplets jump
    /// up out of it (the "now" beat).</item>
    /// <item>An aimed volley (a fan at the hero) also sends a tongue of ink creeping over the floor along the aim, as wide as
    /// the fan, its edges brushed in.</item>
    /// <item>A signature move marks where it lands: a blot of ink seeps into the floor at its target.</item>
    /// </list>
    /// No danger lanes radiating from the boss and no rings. Reduced motion keeps the marks and drops the boiling.</summary>
    public sealed class BossTelegraph : MonoBehaviour
    {
        private ArsenalInk ink;
        private bool drawn;
        private readonly List<UnitState> bosses = new List<UnitState>();
        private static readonly InkPalette Bile = new InkPalette(new Color(.2f, .04f, .1f), new Color(.62f, .14f, .32f), new Color(.98f, .68f, .76f), new Color(.95f, .35f, .45f), new Color(.98f, .72f, .35f));
        // what the last frame showed (read by checks)
        public int Pools { get; private set; }
        public int Tongues { get; private set; }
        public int Blots { get; private set; }

        public void Display(CombatSimulation simulation)
        {
            Pools = Tongues = Blots = 0;
            bosses.Clear();
            if (simulation != null)
                foreach (UnitState unit in simulation.Units)
                    if (unit.Definition.IsBoss && unit.Alive && unit.TelegraphUntil > simulation.Elapsed) bosses.Add(unit);
            if (bosses.Count == 0) { if (drawn) { ink.Begin(); ink.End(); drawn = false; } return; }
            if (ink == null) ink = new ArsenalInk(transform);
            ink.Begin(); drawn = true;
            foreach (UnitState boss in bosses) Draw(boss, simulation.Elapsed);
            ink.End();
        }

        private void Draw(UnitState boss, float now)
        {
            bool still = PresentationSettings.ReducedMotion, calm = PresentationSettings.ReducedFlashes;
            float windup = Mathf.Max(0.1f, BossEncounterRules.Windup(boss));
            float charge = Mathf.Clamp01(1 - (boss.TelegraphUntil - now) / windup);
            float rise = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge / 0.35f)), last = Mathf.Clamp01((charge - 0.72f) / 0.28f);
            float seed = boss.Id * 37 % 360;
            bool special = boss.Definition.Id == NightmareRules.CustomerId || boss.Definition.Id == "OutsideStageKeeper";
            bool signature = boss.ActiveBossSignature >= 0;

            // the pool under the boss: it spreads with the wind-up, and boils in the last beat
            float body = Mathf.Max(boss.Definition.Radius, boss.Definition.BodyHeight * 0.42f);
            float radius = body * (signature ? 1.15f : 0.95f) + (signature ? 1.1f : 0.7f) * charge;
            float throb = still || calm ? 0 : Mathf.Sin(now * Mathf.Lerp(6, 22, charge)) * 0.05f * charge;
            ink.Pool(boss.Position + Vector2.down * 0.05f, radius * (1 + throb), rise, Bile, seed, 0.95f, 0.36f);
            Pools++;
            if (!still && last > 0)
            {
                ink.Bubbles(boss.Position, radius * 0.8f, now * 2.2f, Bile, 5, seed, last);
                // droplets jump straight up out of the ink just before the blow
                for (int i = 0; i < 3; i++)
                {
                    float hop = Mathf.Repeat(now * 3.1f + i * 0.27f, 1);
                    float x = ((i * 53) % 9 - 4) / 4f * radius * 0.75f;
                    ink.TokenAt(InkToken.Drop, Bile, boss.Position + new Vector2(x, -radius * 0.25f + Mathf.Sin(hop * Mathf.PI) * 0.6f), 0.42f, 0, last * (1 - hop * 0.6f));
                }
            }

            if (special) return;
            if (boss.ActiveTrademark) return; // its path is inked by BossTrademarkVisual once the pieces are out
            if (signature)
            {
                // a signature move lands near its target: ink seeps into the floor there
                ink.Pool(boss.AbilityTarget, 0.7f + 1.1f * charge, rise, Bile, seed + 90, 0.85f, 0.42f);
                if (!still && last > 0) ink.Bubbles(boss.AbilityTarget, 0.6f, now * 2.6f, Bile, 3, seed + 3, last);
                Blots++;
                return;
            }
            if (!Aimed(boss.Definition.BossPattern) || boss.TelegraphDirection.sqrMagnitude < 0.01f) return;
            // an aimed fan: a tongue of ink creeps along the aim, as wide as the fan will be
            Vector2 aim = boss.TelegraphDirection.normalized, origin = boss.Position + aim * body * 0.6f;
            float half = HalfSpread(boss) * Mathf.Rad2Deg + 4, reach = Mathf.Lerp(1.4f, 5.4f, Mathf.SmoothStep(0, 1, charge));
            float angle = Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg;
            float alpha = 0.62f + 0.25f * last + (still || calm ? 0 : 0.08f * Mathf.Sin(now * 14) * last);
            ink.Cone(origin, reach, angle - half, angle + half, Bile.Body, alpha * rise, 0.34f * rise);
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 edge = new Vector2(Mathf.Cos((angle + side * half) * Mathf.Deg2Rad), Mathf.Sin((angle + side * half) * Mathf.Deg2Rad));
                ink.FloorStroke(origin + edge * 0.3f, origin + edge * reach, Bile, 0.13f, 1, 0, rise, 0.03f, seed + side);
            }
            // the leading edge of the tongue is wet and bright as the blow comes
            ink.FloorStroke(origin + Rotate(aim, -half * 0.85f) * reach, origin + Rotate(aim, half * 0.85f) * reach, last > 0 ? Ember : Bile, 0.16f + 0.08f * last, 1, 0, rise, 0.05f, seed);
            Tongues++;
        }

        /// <summary>The leading edge in the last beat: the ink catches a hot amber light (enemy colours, never our lantern).</summary>
        private static readonly InkPalette Ember = new InkPalette(new Color(.2f, .04f, .06f), new Color(.85f, .28f, .22f), new Color(1, .78f, .5f), new Color(1, .5f, .3f), new Color(.98f, .72f, .35f));
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
