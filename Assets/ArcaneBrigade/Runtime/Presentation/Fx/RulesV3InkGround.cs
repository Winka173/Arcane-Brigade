using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>RulesV3Ink's floor level (features 4, 8, 12 of 29/10/2026): the effect budget is measured here once a frame, step-8
    /// weather is answered on the floor, big moments throw light (FloorAndLamps.cs), and outside combat the bond effects keep
    /// playing to their end on a real-time clock (weather fades out) instead of freezing on screen when a wave is won.</summary>
    public sealed partial class RulesV3Ink
    {
        private FxDirector floorFx;
        private LampTint lampTint;
        private CombatSimulation groundSim;
        private bool groundLive;
        private float outsideFrom, outsideClock, groundNow;
        /// <summary>Read by checks: floor effects and lamps live, enemies tinted last frame.</summary>
        public int FloorEffects => floorFx?.Live ?? 0;
        public int Lamps => lampTint != null ? lampTint.Live : 0;

        /// <summary>Called once a frame from LateUpdate, before anything else draws.</summary>
        private void GroundFrame(CombatSimulation sim, bool live)
        {
            FxBudget.Measure(sim);
            if (floorFx == null) { floorFx = new FxDirector(transform, GroundFx.LightOrder, GroundFx.DustOrder); lampTint = gameObject.AddComponent<LampTint>(); }
            if (sim != groundSim) { groundSim = sim; floorFx.Clear(); lampTint.Clear(); }
            float now;
            if (live) now = sim.Elapsed;
            else
            {
                // the fight stopped (won, lost, between waves): the simulation clock stands still, so effects run on in real time
                // until they end; weather and lasting zones are told to finish within a moment (their own fade-out plays)
                if (groundLive) { outsideFrom = groundNow; outsideClock = 0; fx?.Settle(outsideFrom, 1.6f); floorFx.Settle(outsideFrom, 1.8f); lampTint.Clear(); }
                outsideClock += Mathf.Min(Time.unscaledDeltaTime, .1f); now = outsideFrom + outsideClock;
                fx?.Draw(now);
            }
            floorFx.Draw(now);
            lampTint.Frame(live ? sim : null, now);
            groundLive = live; groundNow = now;
        }

        /// <summary>Every bond moment also reaches the floor: weather spreads its floor, a step 6/8 moment lights it.</summary>
        private void GroundMoment(BondMomentRequest m)
        {
            if (floorFx == null || m.Rule == null) return;
            if (m.Kind == BondMomentKind.Weather)
            {
                string key = m.Duration >= 3 ? GroundFx.FloorKey(m.Rule) : null;
                if (key != null) floorFx.Spawn(new BondMomentRequest(m.Bond, key, 8, BondMomentKind.Weather, m.From, m.To, 0, 0, m.Duration), Now);
                return;
            }
            if (m.Step < 6 || !FxDirector.Has(m.Rule)) return;
            ElementLook look = LookFor(m.Rule); if (look == ElementLook.None) return;
            bool line = m.Kind == BondMomentKind.Line;
            var lamp = new LampTint.Lamp { From = m.From, To = m.To, Line = line, Land = Now + Mathf.Max(0, m.Delay), Reach = LampEffect.Reach(m.Radius), Look = look, Colour = LookColour(look), Seed = ++seed };
            if (lampTint.Add(lamp))
                floorFx.Spawn(new BondMomentRequest(m.Bond, GroundFx.LampKey(look), m.Step, line ? BondMomentKind.Line : BondMomentKind.Strike, m.From, m.To, m.Radius, m.Delay, 0), Now);
        }
    }
}
