using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The bond step ladder on screen (Docs/Bond_Steps_Proposal_2026-10-28.md): step-6 events fall on the crowds
    /// from the sky or sweep across them, lasting zones (whirlpool, tornado, shadow pit, poison cloud, hot pot) keep bursting
    /// where they stand, and step-8 weather tints the whole arena in its element for the wave.</summary>
    public sealed partial class RulesV3Ink
    {
        private struct StepWeather { public ElementLook Look; public string Rule; public float Born, Until; }
        private readonly List<StepWeather> weathers = new List<StepWeather>();
        private readonly Dictionary<int, float> zonePulse = new Dictionary<int, float>();

        /// <summary>Events that come down from the sky: a strike falls onto the spot before the blast.</summary>
        private static bool FallsFromSky(string rule) => rule.Contains("thunder") || rule.Contains("meteor") || rule.Contains("star") || rule.Contains("comet")
            || rule.Contains("hail") || rule.Contains("orb") || rule.Contains("decree") || rule.Contains("book") || rule.Contains("spikes") || rule.Contains("pages");

        public void OnMoment(BondMomentRequest m)
        {
            ElementLook look = LookFor(m.Rule);
            switch (m.Kind)
            {
                case BondMomentKind.Weather:
                    for (int i = weathers.Count - 1; i >= 0; i--) if (weathers[i].Rule == m.Rule) weathers.RemoveAt(i);
                    weathers.Add(new StepWeather { Look = look, Rule = m.Rule, Born = Now, Until = Now + Mathf.Min(m.Duration, 600) });
                    if (m.Duration < 30) Finale(look, m.To);
                    Sound("rules.field", m.To);
                    break;
                case BondMomentKind.Line:
                {
                    // a sweep along its line (a phoenix dive, a boulder, the leviathan, a pendulum): the strike, then blasts along it
                    Targeted(look, m.From, m.To, m.Delay * .5f, m.Rule);
                    float length = (m.To - m.From).magnitude; int n = Mathf.Clamp(Mathf.RoundToInt(length / 2.2f), 3, 6);
                    for (int i = 0; i < n; i++) Blast(look, Vector2.Lerp(m.From, m.To, (i + .5f) / n), Mathf.Max(.7f, m.Radius), m.Delay + i * .06f, rule: m.Rule);
                    Sound(look, "burst", (m.From + m.To) * .5f);
                    break;
                }
                case BondMomentKind.Zone:
                    Blast(look, m.To, m.Radius, 0, rule: m.Rule);
                    break;
                default:
                {
                    // a thrown event (a flask, a pie, a shell) flies from the hero; a sky event falls onto the spot
                    if (FallsFromSky(m.Rule)) Targeted(look, m.To + new Vector2(-1.2f, 7.5f), m.To, Mathf.Max(0, m.Delay - .3f), m.Rule);
                    else if ((m.From - m.To).sqrMagnitude > .25f) Targeted(look, m.From, m.To, Mathf.Max(0, m.Delay - .3f), m.Rule);
                    if (m.Step >= 8 && m.Radius >= 3) Finale(look, m.To);
                    Blast(look, m.To, m.Radius, m.Delay, rule: m.Rule);
                    if (popsThisFrame < MaxPopsPerFrame) { popsThisFrame++; Sound(look, "burst", m.To); }
                    break;
                }
            }
        }

        private readonly Dictionary<int, ElementLook> shotLook = new Dictionary<int, ElementLook>();
        private static Color Faded(Color c, float a) { c.a = a; return c; }

        /// <summary>Our shots glow in the element of the friend that fired them: a soft halo and a bright core on the light layer,
        /// so a plain painted shot reads as fire, frost or lightning in flight.</summary>
        private void DrawShotGlow(CombatSimulation sim, float now)
        {
            int drawn = 0;
            if (shotLook.Count > 512) shotLook.Clear();
            foreach (ProjectileState shot in sim.Projectiles)
            {
                if (shot == null || shot.Faction == Faction.Enemy) continue;
                if (!shotLook.TryGetValue(shot.SourceUnitId, out ElementLook look))
                {
                    UnitState source = sim.UnitForPresentation(shot.SourceUnitId);
                    look = source == null ? ElementLook.None : source.Faction == Faction.Companion ? LookFor(MainBond(source.Id)) : ElementInk.For(source.Definition.Id);
                    shotLook[shot.SourceUnitId] = look;
                }
                if (look == ElementLook.None) continue;
                Color c = NumberColour(look); float flicker = look == ElementLook.Storm ? .7f + .3f * Mathf.PerlinNoise(now * 20, shot.Id) : 1;
                Vector2 at = shot.Position + shot.PatternOffset, back = shot.Velocity.sqrMagnitude > .01f ? -shot.Velocity.normalized : Vector2.zero;
                light.Sector(at + back * .18f, .5f, 0, 360, Faded(c, .38f * flicker), Faded(c, 0));
                light.Sector(at + back * .42f, .32f, 0, 360, Faded(c, .2f * flicker), Faded(c, 0));
                light.Sector(at, .16f, 0, 360, Faded(Color.Lerp(c, Color.white, .6f), .7f * flicker), Faded(c, 0));
                if (++drawn >= 160) break;
            }
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
            for (int i = weathers.Count - 1; i >= 0; i--)
            {
                StepWeather w = weathers[i];
                if (now > w.Until || now < w.Born - 1) { weathers.RemoveAt(i); continue; }
                float rise = Mathf.Clamp01((now - w.Born) / .8f), fade = Mathf.Clamp01((w.Until - now) / .8f), a = rise * fade * (PresentationSettings.ReducedEffects ? .5f : 1);
                float beat = .85f + .15f * Mathf.Sin(now * 2.2f); // the weather breathes slowly so it reads as alive
                ink.Vignette(centre, 30, NumberColour(w.Look), .2f * a * beat, .62f * a * beat);
            }
            BondRuntime bonds = sim.Bonds;
            if (bonds == null) return;
            for (int i = 0; i < bonds.StepZoneCount; i++)
            {
                if (!bonds.StepZoneAt(i, out string rule, out Vector2 at, out float radius)) continue;
                int key = rule.GetHashCode() ^ i;
                if (zonePulse.TryGetValue(key, out float last) && now - last < .45f) continue;
                zonePulse[key] = now; Blast(LookFor(rule), at, radius * .8f, 0, .05f, rule);
            }
            if (zonePulse.Count > 64) zonePulse.Clear();
            if (bonds.Active("lantern-guild", 8))
            {
                float angle = bonds.LighthouseAngle; Vector2 end = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * BondStepTuning.LighthouseReach;
                ink.Vignette(centre, 1.2f, NumberColour(ElementLook.Light), .5f, .1f);
                HeroIdentityVisual.AddGuest(new HeroIdentityGlyph { Role = HeroGlyphRole.Strike, Shape = MorphGeometry.Line, Position = centre, End = end, Direction = (end - centre).normalized,
                    Width = BondStepTuning.LighthouseWidth, Created = now - .2f, At = now - .1f, Until = now + .05f, Seed = 7 }, HeroIdentityVisual.GuestStyle(ElementLook.Light));
            }
        }
    }
}
