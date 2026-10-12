using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The bond step ladder on screen (Docs/Bond_Steps_Proposal_2026-10-28.md): step-6 events fall on the crowds
    /// from the sky or sweep across them, lasting zones (whirlpool, tornado, shadow pit, poison cloud, hot pot) keep bursting
    /// where they stand, and step-8 weather tints the whole arena in its element for the wave.</summary>
    public sealed partial class RulesV3Ink
    {
        private FxDirector fx;

        /// <summary>Events that come down from the sky: a strike falls onto the spot before the blast.</summary>
        private static bool FallsFromSky(string rule) => rule.Contains("thunder") || rule.Contains("meteor") || rule.Contains("star") || rule.Contains("comet")
            || rule.Contains("hail") || rule.Contains("orb") || rule.Contains("decree") || rule.Contains("book") || rule.Contains("spikes") || rule.Contains("pages");

        /// <summary>Every bond-step moment is drawn by the effect director (Fx/), one animation per rule; a rule it does not know draws nothing.</summary>
        public void OnMoment(BondMomentRequest m)
        {
            if (fx != null && fx.Spawn(m, Now) && popsThisFrame < MaxPopsPerFrame) { popsThisFrame++; Sound(LookFor(m.Rule), "burst", m.To); }
            Forewarning.Spawn(fx, m, Now); // the ground takes the element half a second before a step 6/8 moment lands (Fx/Forewarning.cs)
        }

        private readonly Dictionary<int, ElementLook> shotLook = new Dictionary<int, ElementLook>();
        private static Color Faded(Color c, float a) { c.a = a; return c; }

        /// <summary>Our shots glow in the element of the friend that fired them: a soft halo and a bright core on the light layer,
        /// so a plain painted shot reads as fire, frost or lightning in flight.</summary>
        private void DrawShotGlow(CombatSimulation sim, float now)
        {
            int drawn = 0;
            if (shotLook.Count > 512) shotLook.Clear();
            friendFx ??= FriendFxLayer.Create(transform); friendFx.Begin(); // element trails and cast tells (Fx/FriendFxLayer.cs)
            foreach (ProjectileState shot in sim.Projectiles)
            {
                if (shot == null || shot.Faction == Faction.Enemy) continue;
                ElementLook look = ShotLookOf(sim, shot.SourceUnitId);
                if (look == ElementLook.None) continue;
                friendFx.Shot(look, shot.Position + shot.PatternOffset, shot.Velocity, shot.Id, now);
                if (++drawn >= ShotTrails.MaxShots) break;
            }
            friendFx.DrawTells(sim, boundSkills, now, id => ShotLookOf(sim, id)); friendFx.End();
        }
        private FriendFxLayer friendFx;
        private ElementLook ShotLookOf(CombatSimulation sim, int unitId)
        {
            if (shotLook.TryGetValue(unitId, out ElementLook look)) return look;
            UnitState source = sim.UnitForPresentation(unitId);
            look = source == null ? ElementLook.None : source.Faction == Faction.Companion ? LookFor(MainBond(source.Id)) : ElementInk.For(source.Definition.Id);
            return shotLook[unitId] = look;
        }

        private struct Flight { public Vector2 From, To; public float Born, Time; public ElementLook Look; public int Seed; public InkToken Token; }
        private readonly List<Flight> flights = new List<Flight>();

        /// <summary>A targeted strike of any element but lightning: the element's piece flies from its source to the target on
        /// a short arc, spinning, with a soft glow trail, landing just as its blast goes off.</summary>
        private void Fly(ElementLook look, Vector2 from, Vector2 to, float delay, InkToken? token = null)
        {
            if (flights.Count >= 64) flights.RemoveAt(0);
            float distance = (to - from).magnitude;
            flights.Add(new Flight { From = from, To = to, Born = Now + delay, Time = Mathf.Clamp(distance / 14f, .14f, .5f), Look = look, Seed = ++seed, Token = token ?? TokenFor(look) });
        }

        private void DrawFlights(float now)
        {
            for (int i = flights.Count - 1; i >= 0; i--)
            {
                Flight f = flights[i]; float t = (now - f.Born) / f.Time;
                if (t > 1) { flights.RemoveAt(i); continue; }
                if (t < 0) continue;
                float lift = Mathf.Min(2.2f, (f.To - f.From).magnitude * .22f);
                Vector2 Point(float k) => Vector2.Lerp(f.From, f.To, k) + Vector2.up * (4 * k * (1 - k) * lift + .3f);
                Vector2 at = Point(t);
                Color c = NumberColour(f.Look);
                for (int k = 1; k <= 4; k++) { float back = Mathf.Max(0, t - k * .07f); light.Sector(Point(back), .28f - k * .04f, 0, 360, Faded(c, .32f - k * .06f), Faded(c, 0)); }
                light.Sector(at, .42f, 0, 360, Faded(c, .45f), Faded(c, 0));
                ink.Token(f.Token, PaletteFor(f.Look), at, .5f, now * 540 + f.Seed * 37, 1);
            }
        }

        private struct Slash { public Vector2 At; public float Angle, Reach, Born; public ElementLook Look; }
        private readonly List<Slash> slashes = new List<Slash>();

        /// <summary>A melee hit by our side leaves a glowing swing trail in the striker's element across the enemy it struck.</summary>
        private void OnMeleeHit(CombatEvent e)
        {
            if (e.Kind != CombatEventKind.Hit || e.Weapon == null || e.Weapon.Kind != WeaponKind.Melee || slashes.Count >= 40 || boundSimulation == null) return;
            UnitState source = boundSimulation.UnitForPresentation(e.Cause.SourceUnitId);
            if (source == null || source.Faction == Faction.Enemy) return;
            ElementLook look = source.Faction == Faction.Companion ? LookFor(MainBond(source.Id)) : ElementInk.For(source.Definition.Id);
            if (look == ElementLook.None) look = ElementLook.Gold;
            Vector2 dir = e.Position - source.Position; if (dir.sqrMagnitude < .0001f) dir = e.Direction.sqrMagnitude > 0 ? e.Direction : Vector2.right;
            slashes.Add(new Slash { At = source.Position + Vector2.up * .3f, Angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg, Reach = Mathf.Clamp(dir.magnitude + .35f, .8f, 2.4f), Born = Now, Look = look });
        }

        /// <summary>Each swing trail sweeps through its arc in a tenth of a second and fades: a bright edge in the element's colour.</summary>
        private void DrawSlashes(float now)
        {
            for (int i = slashes.Count - 1; i >= 0; i--)
            {
                Slash s = slashes[i]; float age = now - s.Born;
                if (age > .28f || age < -.1f) { slashes.RemoveAt(i); continue; }
                float sweep = Mathf.Clamp01(age / .1f), fade = 1 - Mathf.Clamp01((age - .1f) / .18f);
                Color c = NumberColour(s.Look);
                float from = s.Angle + 55, to = s.Angle + 55 - 110 * sweep;
                light.Sector(s.At, s.Reach, Mathf.Min(from, to), Mathf.Max(from, to), Faded(c, 0), Faded(c, .55f * fade), s.Reach * .62f);
                light.Sector(s.At, s.Reach * .97f, Mathf.Min(from, to), Mathf.Max(from, to), Faded(Color.white, 0), Faded(Color.Lerp(c, Color.white, .7f), .6f * fade), s.Reach * .86f);
            }
        }

        /// <summary>Weather over the arena for the wave, lasting zones bursting where they stand, and the lighthouse beam.</summary>
        private void DrawSteps(CombatSimulation sim, float now)
        {
            Vector2 centre = sim.Champion.Position;
            DrawShotGlow(sim, now);
            DrawSlashes(now);
            DrawFlights(now);
            fx?.Draw(now);
            BondRuntime bonds = sim.Bonds;
            if (bonds == null) return;
            // the lighthouse beam (LanternFx) follows the angle the rules really sweep, so what is lit is what is hit
            if (bonds.Active("lantern-guild", 8)) { LanternFx.BeamLive = true; LanternFx.BeamCentre = centre; LanternFx.BeamAngle = bonds.LighthouseAngle; }
        }
    }
}
