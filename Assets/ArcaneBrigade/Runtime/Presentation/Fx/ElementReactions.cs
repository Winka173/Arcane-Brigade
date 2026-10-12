using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What an element leaves on the enemies it touches, presentation only: an enemy struck by an element carries it for a
    /// while (EnemyElementState), two elements landing on the same enemy within a moment react (ElementCombos), and an enemy killed
    /// by an element dies its way (ElementDeaths). Fed by RulesV3Ink.OnCombat; its own batch, so bond effects keep their budget.</summary>
    public sealed class ElementReactions
    {
        public delegate bool Locate(int unit, out Vector2 centre, out float height);

        private struct Mark { public ElementLook Look; public float Since, Last; public int Seed; }
        private struct Combo { public ElementCombos.Kind Kind; public ElementLook Other; public Vector2 At; public float Born, Scale; public int Seed; }
        private struct Death { public ElementLook Look; public Vector2 At, Dir; public float Born, Height; public int Seed; }

        /// <summary>How long a mark stays after the last elemental hit, and how close a second element must follow to react.</summary>
        public const float MarkLife = 3.2f, MarkFade = 1f, ComboWindow = 2f;
        private const int MaxTracked = 600, MaxCombos = 6, MaxDeaths = 40;

        private readonly FxBatch batch;
        private readonly Func<CombatEvent, ElementLook> hitLook;
        private readonly Dictionary<int, Mark> marks = new Dictionary<int, Mark>(256);
        private readonly List<int> gone = new List<int>(64);
        private readonly List<Combo> combos = new List<Combo>(MaxCombos);
        private readonly List<Death> deaths = new List<Death>(MaxDeaths);
        private CombatSimulation sim;
        private readonly Locate fromSim;
        /// <summary>Most reactions on screen at once.</summary>
        public int ComboCap = MaxCombos;
        private int seed;

        /// <summary>Quads drawn last frame, detailed marks and tint-only marks (read by checks and the preview).</summary>
        public int Quads => batch.Quads;
        public int DetailedMarks { get; private set; }
        public int TintedMarks { get; private set; }
        public int LiveCombos => combos.Count;
        public int LiveDeaths => deaths.Count;

        /// <param name="look">The element a hit was dealt in (RulesV3Ink.HitLook).</param>
        public ElementReactions(Transform parent, Func<CombatEvent, ElementLook> look)
        {
            batch = new FxBatch(parent, 2143, 2142);
            hitLook = look;
            fromSim = LocateInSim;
        }

        private bool LocateInSim(int id, out Vector2 centre, out float height)
        {
            UnitState u = sim?.UnitForPresentation(id);
            if (u == null || !u.Alive || u.Definition == null) { centre = default; height = 0; return false; }
            centre = u.Position; height = Mathf.Clamp(K8Motion.BodyHeight(u), .5f, 3f); return true;
        }

        private static bool Elemental(ElementLook look) => look != ElementLook.None && look != ElementLook.Gold && look != ElementLook.Thread;

        /// <summary>Only element effects mark (bond traits and steps, companion skills); basic attacks draw nothing of their own.</summary>
        private static bool FromEffect(CombatEvent e)
        {
            string a = e.Cause.AttributionId;
            return !string.IsNullOrEmpty(a) && a.StartsWith("bond:", StringComparison.Ordinal) || e.Cause.ItemId == "skill-v3";
        }

        public void Observe(CombatEvent e, float now)
        {
            if (e.Faction != Faction.Enemy || e.Subject == null || e.Subject.Definition == null) return;
            if (e.Kind == CombatEventKind.Hit)
            {
                if (!FromEffect(e)) return;
                ElementLook look = hitLook(e);
                if (Elemental(look)) Struck(e.SubjectId, e.Subject.Position, K8Motion.BodyHeight(e.Subject), look, now);
            }
            else if (e.Kind == CombatEventKind.Kill)
            {
                ElementLook look = FromEffect(e) ? hitLook(e) : ElementLook.None;
                if (e.Subject.Definition.IsBoss || e.Subject.Flung) { marks.Remove(e.SubjectId); return; }
                Killed(e.SubjectId, e.Subject.Position, K8Motion.BodyHeight(e.Subject), Elemental(look) ? look : ElementLook.None, e.Direction, now);
            }
        }

        /// <summary>An element effect struck an enemy: it carries the element now; if it still carried another, they react.</summary>
        public void Struck(int unit, Vector2 centre, float height, ElementLook look, float now)
        {
            if (!Elemental(look)) return;
            if (marks.TryGetValue(unit, out Mark m) && now - m.Last <= MarkLife)
            {
                if (m.Look != look && now - m.Last <= ComboWindow && ElementCombos.For(m.Look, look, out ElementCombos.Kind kind, out ElementLook other))
                    TryCombo(kind, other, centre + Vector2.down * height * .42f, height, now);
                if (m.Look == look) { m.Last = now; marks[unit] = m; return; }
            }
            if (marks.Count >= MaxTracked && !marks.ContainsKey(unit)) return;
            marks[unit] = new Mark { Look = look, Since = now, Last = now, Seed = unit * 7 + (int)look };
        }

        private void TryCombo(ElementCombos.Kind kind, ElementLook other, Vector2 at, float height, float now)
        {
            // one reaction per spot at a time: a crowd struck by two elements reacts once, not once per enemy
            for (int i = 0; i < combos.Count; i++)
            {
                float age = now - combos[i].Born, d = (combos[i].At - at).sqrMagnitude;
                if (d < 5f && (combos[i].Kind == kind ? age < ElementCombos.Life * .7f : age < .5f)) return;
            }
            int cap = PresentationSettings.ReducedEffects ? 2 : ComboCap;
            if (combos.Count >= cap) combos.RemoveAt(0);
            combos.Add(new Combo { Kind = kind, Other = other, At = at, Born = now, Scale = Mathf.Clamp(height / 1.1f, .8f, 1.4f), Seed = ++seed * 13 });
        }

        /// <summary>An enemy died: by the element of the killing effect, else by the element it still carried, else as before.</summary>
        public void Killed(int unit, Vector2 centre, float height, ElementLook look, Vector2 direction, float now)
        {
            if (marks.TryGetValue(unit, out Mark m)) { if (look == ElementLook.None && now - m.Last <= MarkLife) look = m.Look; marks.Remove(unit); }
            if (!Elemental(look)) return;
            int cap = PresentationSettings.ReducedEffects ? 12 : MaxDeaths;
            if (deaths.Count >= cap) deaths.RemoveAt(0);
            deaths.Add(new Death { Look = look, At = centre, Dir = direction.sqrMagnitude > .001f ? direction.normalized : Vector2.right, Born = now, Height = Mathf.Clamp(height, .5f, 2f), Seed = ++seed * 17 });
        }

        public void Draw(CombatSimulation simulation, float now) { sim = simulation; Draw(now, fromSim); }

        public void Draw(float now, Locate locate)
        {
            batch.Begin();
            // deaths and reactions first (they sit under the marks of the living)
            for (int i = deaths.Count - 1; i >= 0; i--)
            {
                Death d = deaths[i]; float t = now - d.Born;
                if (t > ElementDeaths.Life || t < -1) { deaths.RemoveAt(i); continue; }
                if (t >= 0) ElementDeaths.Draw(batch, d.Look, d.At, d.Height, d.Dir, t, now, d.Seed);
            }
            for (int i = combos.Count - 1; i >= 0; i--)
            {
                Combo c = combos[i]; float t = now - c.Born;
                if (t > ElementCombos.Life || t < -1) { combos.RemoveAt(i); continue; }
                if (t >= 0) ElementCombos.Draw(batch, c.Kind, c.Other, c.At, c.Scale, t, now, c.Seed);
            }
            int detail = FxBudget.MarkDetail(PresentationSettings.ReducedEffects ? 16 : 56), drawn = 0, tinted = 0;
            gone.Clear();
            foreach (KeyValuePair<int, Mark> pair in marks)
            {
                Mark m = pair.Value; float idle = now - m.Last;
                if (idle > MarkLife || !locate(pair.Key, out Vector2 centre, out float height)) { gone.Add(pair.Key); continue; }
                float alpha = Mathf.Clamp01((MarkLife - idle) / MarkFade) * Mathf.Clamp01((now - m.Since) / .12f + .3f);
                if (drawn < detail) { EnemyElementState.Draw(batch, m.Look, centre, height, alpha, now - m.Since, now, m.Seed); drawn++; }
                else if (FxBudget.MarkTint(tinted)) { EnemyElementState.Tint(batch, m.Look, centre, height, alpha); tinted++; }
            }
            foreach (int id in gone) marks.Remove(id);
            DetailedMarks = drawn; TintedMarks = tinted;
            batch.End();
        }

        public void Clear()
        {
            marks.Clear(); combos.Clear(); deaths.Clear();
            batch.Begin(); batch.End(); DetailedMarks = TintedMarks = 0;
        }
    }
}
