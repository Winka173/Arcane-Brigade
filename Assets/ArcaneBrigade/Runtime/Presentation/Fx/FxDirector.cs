using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One bond moment on screen. <c>Draw</c> is a pure function of time: <c>pre</c> runs 0..1 from the announcement to the
    /// landing, <c>t</c> is seconds since it landed (negative before), so nothing needs per-frame state.</summary>
    public abstract class FxEffect
    {
        public BondMomentRequest Moment; public float Born, Land, End; public int Seed; public Rect Arena;
        public Vector2 At => Moment.To; public Vector2 From => Moment.From; public float Radius => Mathf.Max(.8f, Moment.Radius);
        protected abstract float Life { get; }
        public void Start(BondMomentRequest m, float now, int seed, Rect arena)
        {
            Moment = m; Born = now; Land = now + Mathf.Max(0, m.Delay); Seed = seed; Arena = arena;
            End = Land + Life; if (m.Kind == BondMomentKind.Weather) End = now + Mathf.Min(m.Duration, 600);
        }
        public abstract void Draw(FxBatch b, float now, float pre, float t);
    }

    /// <summary>Plays every bond-step moment (Docs/Bond_Steps_Proposal_2026-10-28.md) with an animation of its own. A rule with no
    /// registered effect draws nothing, and the checks fail it, so nothing is ever silently borrowed from another rule.</summary>
    public sealed class FxDirector
    {
        private static readonly Dictionary<string, Func<FxEffect>> Registry = new Dictionary<string, Func<FxEffect>>();
        private readonly List<FxEffect> live = new List<FxEffect>(64);
        private readonly FxBatch batch;
        private int seed;
        public Rect Arena = new Rect(-12, -6.5f, 24, 13);
        public int Live => live.Count;
        public int Quads => batch.Quads;

        static FxDirector()
        {
            StormFx.Register(Registry);
        }

        public FxDirector(Transform parent) { batch = new FxBatch(parent); }

        public static bool Has(string rule) => rule != null && Registry.ContainsKey(rule);
        public static IEnumerable<string> Rules => Registry.Keys;

        public bool Spawn(BondMomentRequest m, float now)
        {
            if (m.Rule == null || !Registry.TryGetValue(m.Rule, out Func<FxEffect> make)) return false;
            if (m.Kind == BondMomentKind.Weather) for (int i = live.Count - 1; i >= 0; i--) if (live[i].Moment.Rule == m.Rule && live[i].Moment.Kind == m.Kind) live.RemoveAt(i);
            if (live.Count >= 96) live.RemoveAt(0);
            FxEffect e = make(); e.Start(m, now, ++seed, Arena); live.Add(e); return true;
        }

        public void Clear() { live.Clear(); }

        /// <summary>Draws every live effect at <paramref name="now"/> (simulation seconds).</summary>
        public void Draw(float now)
        {
            batch.Begin();
            for (int i = live.Count - 1; i >= 0; i--)
            {
                FxEffect e = live[i];
                if (now > e.End) { live.RemoveAt(i); continue; }
                if (now < e.Born) continue;
                float pre = e.Land > e.Born ? Mathf.Clamp01((now - e.Born) / (e.Land - e.Born)) : 1;
                e.Draw(batch, now, pre, now - e.Land);
            }
            batch.End();
        }
    }
}
