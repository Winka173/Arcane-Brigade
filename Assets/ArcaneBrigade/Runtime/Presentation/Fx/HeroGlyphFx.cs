using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Plays the light effect a hero's strike asks for (HeroIdentityGlyph.Fx, e.g. the Comet Runner's falling stars): each
    /// new strike starts its effect so that it lands when the strike lands, on the simulation clock (a pause holds it).</summary>
    public sealed class HeroGlyphFx : MonoBehaviour
    {
        /// <summary>How long before the hit an effect starts its fall (the falling star's streak takes .32 s to arrive).</summary>
        private const float Lead = .32f;
        private ArenaController owner;
        private FxDirector director;
        private readonly HashSet<HeroIdentityGlyph> started = new HashSet<HeroIdentityGlyph>();
        private float lastNow = -1;
        /// <summary>At most one effect of a rule every half second (a meteor falls every .25 s: drawn each time they piled into one
        /// white blaze); the strikes in between still hit, unpainted by this layer.</summary>
        private const float Gap = .5f;
        private readonly Dictionary<string, float> lastStart = new Dictionary<string, float>();

        public void Configure(ArenaController value) => owner = value;

        private void LateUpdate()
        {
            CombatSimulation sim = owner != null ? owner.Simulation : null;
            if (sim == null) return;
            director ??= new FxDirector(transform);
            float now = sim.Elapsed;
            if (now < lastNow) { director.Clear(); started.Clear(); lastStart.Clear(); } // a new night
            lastNow = now;
            foreach (HeroIdentityGlyph glyph in sim.HeroIdentityGlyphs)
            {
                if (string.IsNullOrEmpty(glyph.Fx) || !started.Add(glyph)) continue;
                if (lastStart.TryGetValue(glyph.Fx, out float last) && now - last < Gap) { glyph.Painted = false; continue; } // the ordinary ink mark draws it
                lastStart[glyph.Fx] = now;
                director.Spawn(new FxMoment(glyph.Fx, glyph.Position, Mathf.Max(.8f, glyph.Radius), Mathf.Max(0, glyph.At - Lead - now)), now);
            }
            if (started.Count > 256) started.RemoveWhere(g => g.Until < now);
            director.Draw(now);
        }
    }
}
