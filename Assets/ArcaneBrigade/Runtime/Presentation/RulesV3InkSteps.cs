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
                    Targeted(look, m.From, m.To, m.Delay * .5f);
                    float length = (m.To - m.From).magnitude; int n = Mathf.Clamp(Mathf.RoundToInt(length / 2.2f), 3, 6);
                    for (int i = 0; i < n; i++) Blast(look, Vector2.Lerp(m.From, m.To, (i + .5f) / n), Mathf.Max(.7f, m.Radius), m.Delay + i * .06f);
                    Sound(look, "burst", (m.From + m.To) * .5f);
                    break;
                }
                case BondMomentKind.Zone:
                    Blast(look, m.To, m.Radius, 0);
                    break;
                default:
                {
                    // a thrown event (a flask, a pie, a shell) flies from the hero; a sky event falls onto the spot
                    if (FallsFromSky(m.Rule)) Targeted(look, m.To + new Vector2(-1.2f, 7.5f), m.To, Mathf.Max(0, m.Delay - .3f));
                    else if ((m.From - m.To).sqrMagnitude > .25f) Targeted(look, m.From, m.To, Mathf.Max(0, m.Delay - .3f));
                    if (m.Step >= 8 && m.Radius >= 3) Finale(look, m.To);
                    Blast(look, m.To, m.Radius, m.Delay);
                    if (popsThisFrame < MaxPopsPerFrame) { popsThisFrame++; Sound(look, "burst", m.To); }
                    break;
                }
            }
        }

        /// <summary>Weather over the arena for the wave, lasting zones bursting where they stand, and the lighthouse beam.</summary>
        private void DrawSteps(CombatSimulation sim, float now)
        {
            Vector2 centre = sim.Champion.Position;
            for (int i = weathers.Count - 1; i >= 0; i--)
            {
                StepWeather w = weathers[i];
                if (now > w.Until || now < w.Born - 1) { weathers.RemoveAt(i); continue; }
                float rise = Mathf.Clamp01((now - w.Born) / .8f), fade = Mathf.Clamp01((w.Until - now) / .8f), a = rise * fade * (PresentationSettings.ReducedEffects ? .5f : 1);
                ink.Vignette(centre, 30, NumberColour(w.Look), .1f * a, .42f * a);
            }
            BondRuntime bonds = sim.Bonds;
            if (bonds == null) return;
            for (int i = 0; i < bonds.StepZoneCount; i++)
            {
                if (!bonds.StepZoneAt(i, out string rule, out Vector2 at, out float radius)) continue;
                int key = rule.GetHashCode() ^ i;
                if (zonePulse.TryGetValue(key, out float last) && now - last < .45f) continue;
                zonePulse[key] = now; Blast(LookFor(rule), at, radius * .8f, 0, .05f);
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
