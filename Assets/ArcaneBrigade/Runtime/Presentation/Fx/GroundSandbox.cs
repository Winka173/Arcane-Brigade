using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Effects sandbox entries for the floor weather, the lamps and the effect budget (FxSandbox.Register).</summary>
    public static class GroundSandbox
    {
        private static FxDirector floor;
        private static Transform floorRoot;
        private static readonly string[] Weathers = { "tide-rain", "storm-thunderstorm", "frost-blizzard", "plague-mist", "starry-night", "eclipse", "lighthouse", "gravity-heavy", "wind-gale", "hearth-feast", "bulwark-fortress" };
        private static readonly ElementLook[] LampLooks = { ElementLook.Fire, ElementLook.Storm, ElementLook.Ice, ElementLook.Void, ElementLook.Light, ElementLook.Poison };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            foreach (string w in Weathers)
            {
                string rule = w;
                FxSandbox.Register("Floor weather", rule, at =>
                {
                    FxSandbox.Spawn(rule, 8, BondMomentKind.Weather, FxSandbox.Hero, FxSandbox.Hero, 0, 0, 10);
                    Floor()?.Spawn(new BondMomentRequest("sandbox", GroundFx.FloorKey(rule), 8, BondMomentKind.Weather, FxSandbox.Hero, FxSandbox.Hero, 0, 0, 10), FxSandbox.Now);
                    Keep(10);
                });
            }
            foreach (ElementLook look in LampLooks)
            {
                ElementLook l = look;
                FxSandbox.Register("Lamp light", l.ToString(), at =>
                {
                    Floor()?.Spawn(new BondMomentRequest("sandbox", GroundFx.LampKey(l), 6, BondMomentKind.Strike, at, at, 2, .3f, 0), FxSandbox.Now);
                    Keep(1.4f);
                    float land = FxSandbox.Now + .3f; int seed = Random.Range(1, 999); Color c = RulesV3Ink.LookColour(l);
                    FxSandbox.Draw(1.4f, (b, t, now) => TintEnemies(at, LampEffect.Reach(2), c, LampEffect.Intensity(l, now - land, seed)));
                });
            }
            FxSandbox.Register("Effect budget", "Crowded field (toggle)", at =>
            {
                bool on = FxBudget.ForcedTier < 0; FxBudget.ForcedTier = on ? 3 : -1; FxBudget.ForcedBySandbox = on;
                FxSandbox.Word(at + Vector2.up * 2.2f, on ? "Crowded field: lean budget" : "Budget measured again", on ? new Color(1, .6f, .3f) : new Color(.6f, 1, .7f));
            });
            FxSandbox.Register("Effect budget", "Trait storm (12 repeats)", at =>
            {
                // twelve repeats of one trait and twelve fire numbers in a breath: under the lean budget they merge to one or two
                for (int i = 0; i < 12; i++)
                {
                    Vector2 p = at + new Vector2(Mathf.Cos(i * 2.4f), Mathf.Sin(i * 2.4f) * .62f) * (1 + (i % 3) * .6f); float delay = i * .06f;
                    FxSandbox.Later(delay, () =>
                    {
                        if (FxBudget.Trait("piercing-volley", 2, FxSandbox.Now)) FxSandbox.Spawn("piercing-volley", 2, BondMomentKind.Strike, p, p, 1.2f, 0);
                        FxSandbox.Number(p + Vector2.up * .8f, 40 + i * 7, ElementLook.Fire);
                    });
                }
            });
        }

        private static FxDirector Floor()
        {
            if (FxSandbox.Current == null) return null;
            if (floor == null || floorRoot == null)
            {
                Transform field = FxSandbox.Current.transform.Find("FxSandboxWorld/Field"); if (field == null) return null;
                floorRoot = new GameObject("Floor fx").transform; floorRoot.SetParent(field, false);
                floor = new FxDirector(floorRoot, GroundFx.LightOrder, GroundFx.DustOrder);
            }
            return floor;
        }

        /// <summary>The sandbox draws only what it is told: keep the floor director drawn for a while.</summary>
        private static void Keep(float seconds) { FxDirector f = floor; FxSandbox.Draw(seconds, (b, t, now) => { if (floorRoot != null) f.Draw(now); }); }

        private static void TintEnemies(Vector2 at, float reach, Color c, float k)
        {
            if (FxSandbox.Current == null) return;
            Transform field = FxSandbox.Current.transform.Find("FxSandboxWorld/Field"); if (field == null) return;
            foreach (SpriteRenderer r in field.GetComponentsInChildren<SpriteRenderer>())
            {
                if (r.name != "enemy") continue;
                Vector2 d = (Vector2)r.transform.localPosition - at; d.y /= .62f;
                float fall = 1 - Mathf.Clamp01(d.magnitude / reach); fall *= fall * (3 - 2 * fall);
                r.color = Color.Lerp(Color.white, LampTint.Lit(Color.white, c), .5f * k * fall);
            }
        }
    }
}
