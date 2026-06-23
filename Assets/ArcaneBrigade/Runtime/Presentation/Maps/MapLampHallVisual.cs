using UnityEngine;

namespace ArcaneBrigade
{
    // The Lamp Hall's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere dispatch here).

    public sealed partial class MapEnvironmentVisual
    {
        private bool heroLit;
        private void DrawLampHall(MapEnvironmentState s, float t)
        {
            // stepping into a lamp's light (or out into the dark) says what it gives over the hero's head (requirement 25/10/2026)
            bool lit = sim.HeroInMapLight;
            if (lit != heroLit && sim.Champion != null)
            {
                Vector2 head = (Vector2)SpeechBubbles.Head(sim.Champion) + Vector2.up * .5f;
                if (lit) ImpactWords.Pop("IN THE LIGHT · +" + Mathf.RoundToInt((MapEnvironmentRules.LightDamage - 1) * 100) + "% DAMAGE · FRIENDS FASTER", head, new Color(1f, .9f, .55f), 4, .85f);
                else ImpactWords.Pop("IN THE DARK · FOES TOUGHER", head, new Color(.7f, .72f, 1f), 4, .8f);
            }
            heroLit = lit;
            float rx = MapEnvironmentRules.LightRadiusX, ry = MapEnvironmentRules.LightRadiusY;
            for (int i = 0; i < s.Lamps.Count; i++)
            {
                MapEnvironmentState.Lamp lamp = s.Lamps[i];
                Put(lamp.Lit ? "LampHall/Lamp_Lit" : "LampHall/Lamp_Snuffed", lamp.Base, 2.7f);
                if (!lamp.Lit) continue;
                // the light on the floor is the painted glow alone (requirement 25/10/2026: no circle drawn round a friendly area)
                Put("LampHall/LightGlow", lamp.Light, 2 * ry * 1.06f, Floor, .62f, tint: new Color(1, .93f, .78f));
            }
            if (s.SnuffTarget >= 0 && t >= s.ReachAt && t < s.SnuffAt + .9f)
            {
                Vector2 head = s.Lamps[s.SnuffTarget].Base + Vector2.up * 2.45f;
                float p = Mathf.Clamp01((t - s.ReachAt) / MapEnvironmentRules.SnuffReach);
                if (p < 1) Put("LampHall/Hand_Reach", head + new Vector2(1.45f, .15f) + new Vector2(2.6f, 1.8f) * (1 - Mathf.SmoothStep(0, 1, p)), 1, Air + 40, Mathf.Clamp01(p * 2.5f));
                else Put("LampHall/Hand_Snuff", head + new Vector2(1.1f, -.15f), 1.5f, Air + 40, Mathf.Clamp01((s.SnuffAt + .9f - t) / .4f));
                // the lamp about to go dark: its light trembles under an amber rim
                if (p < 1) Ring(s.Lamps[s.SnuffTarget].Light, rx * .98f, ry * .98f, p, ArsenalInk.Amber, .7f);
            }
            string[] frames = { "LampHall/Moths_0", "LampHall/Moths_1", "LampHall/Moths_2", "LampHall/Moths_3" };
            for (int i = 0; i < s.Moths.Count; i++)
                if (t >= 4) Put(frames[still ? 0 : (Mathf.FloorToInt(t * 8) + i) % 4], s.Moths[i], 1.05f, Air + 30, .95f);
        }
        /// <summary>A lit lamp's oval lifts a standing prop back to full light (0 none, 1 fully lit).</summary>
        private float LampHallPropLight(MapEnvironmentState s, Vector2 at)
        {
            float lit = 0;
            foreach (MapEnvironmentState.Lamp lamp in s.Lamps)
            {
                if (!lamp.Lit) continue;
                Vector2 d = at - lamp.Light;
                float e = d.x * d.x / (MapEnvironmentRules.LightRadiusX * MapEnvironmentRules.LightRadiusX) + d.y * d.y / (MapEnvironmentRules.LightRadiusY * MapEnvironmentRules.LightRadiusY);
                lit = Mathf.Max(lit, 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.6f, 1.4f, e)));
            }
            return lit;
        }
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>Deep night on the floor that the lamps' own light cuts through, moths' wing glints round the lit lamps.</summary>
        private void AirLampHall(MapEnvironmentState s, int motes, float clock)
        {
            // deep night on the floor; the lamps' painted light (floor order -40) cuts through it
            // the round floor around the lamps: its edges sink deeper into the dark (requirement 23/10/2026; edge was .5)
            Grade(new Color(.04f, .04f, .12f, .36f), new Color(.01f, .01f, .05f, .8f));
            int glints = 0;
            for (int i = 0; i < s.Lamps.Count && glints < Mathf.Max(motes, 4); i++)
            {
                MapEnvironmentState.Lamp lamp = s.Lamps[i];
                if (!lamp.Lit) continue;
                // moths' wing glints circling the light
                for (int k = 0; k < 2 && glints < Mathf.Max(motes, 4); k++, glints++)
                {
                    float a = (still || sparse ? 0 : clock * (1.1f + .3f * k)) + i * 2.3f + k * 3.1f;
                    Vector2 at = lamp.Base + new Vector2(Mathf.Cos(a) * (.5f + .2f * k), 2.3f + Mathf.Sin(a * 1.7f) * .3f);
                    float blink = calm || still || sparse ? .5f : Mathf.Pow(Mathf.Abs(Mathf.Sin(clock * 9 + i * 1.3f + k * 2f)), 4);
                    Put(MechanicInk.Glow, at, Vector2.one * .16f, 0, new Color(1, .96f, .85f, .8f * blink), AirOrder);
                }
            }
        }
    }
}
