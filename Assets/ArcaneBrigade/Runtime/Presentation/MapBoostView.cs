using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a map lends the hero, said plainly (requirement 25/10/2026: every boost shows its stat, amount and time).
    /// Lines rise over the hero's head as they are gained, stacked and never dropped; the boosts still running sit in a row of
    /// small plates under the lantern row with their seconds left.</summary>
    public sealed class MapBoostView : MonoBehaviour
    {
        private sealed class Line { public string Text; public Color Colour; public float Born; }
        private const float Life = 1.9f, Rise = 46, Gap = 34;
        private const int MaxShown = 5;
        private ArenaController owner;
        private readonly List<Line> lines = new List<Line>();
        private readonly Queue<Line> waiting = new Queue<Line>();
        private GUIStyle word, chip;

        public void Configure(ArenaController value) => owner = value;

        private void Update()
        {
            MapEnvironmentState s = owner?.Simulation?.MapEnvironment;
            if (s == null) { lines.Clear(); waiting.Clear(); return; }
            foreach (MapPop pop in s.Pops) waiting.Enqueue(new Line { Text = pop.Text, Colour = pop.Colour });
            s.Pops.Clear();
            float now = Time.unscaledTime;
            lines.RemoveAll(l => now - l.Born > Life);
            // a new line waits its turn rather than being dropped, and lines land a beat apart so each can be read
            while (waiting.Count > 0 && lines.Count < MaxShown && (lines.Count == 0 || now - lines[lines.Count - 1].Born > .22f))
            {
                Line next = waiting.Dequeue(); next.Born = now; lines.Add(next);
            }
        }

        private void OnGUI()
        {
            if (owner?.Session == null || owner.Simulation == null || owner.PresentationPaused || Camera.main == null) return;
            if (owner.Session.State.Phase != SessionPhase.Combat) return;
            if (Event.current.type != EventType.Repaint) return;
            word ??= new GUIStyle(ArcadeTypography.Style(26, true)) { alignment = TextAnchor.MiddleCenter, wordWrap = false };
            chip ??= new GUIStyle(ArcadeTypography.Style(15, true)) { alignment = TextAnchor.MiddleCenter, wordWrap = false };
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            UnitState hero = owner.Simulation.Champion;
            if (hero != null && lines.Count > 0)
            {
                Vector3 screen = Camera.main.WorldToScreenPoint(SpeechBubbles.Head(hero) + Vector3.up * .7f);
                Vector2 head = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y, 0));
                float now = Time.unscaledTime;
                bool still = PresentationSettings.ReducedMotion;
                for (int i = 0; i < lines.Count; i++)
                {
                    Line l = lines[i]; float age = now - l.Born;
                    float rise = still ? 0 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / Life)) * Rise;
                    float alpha = Mathf.Clamp01(age / .12f) * Mathf.Clamp01((Life - age) / .45f);
                    float pop = still ? 1 : 1 + .25f * Mathf.Clamp01(1 - age / .18f);
                    float y = head.y - (lines.Count - 1 - i) * Gap - rise;
                    Rect r = new Rect(head.x - 400, y - 18, 800, 36);
                    Matrix4x4 m = GUI.matrix;
                    if (pop > 1.001f) GUIUtility.ScaleAroundPivot(Vector2.one * pop, r.center);
                    word.normal.textColor = new Color(0, 0, 0, .75f * alpha); // a dark backing so the words read on any floor
                    GUI.Label(new Rect(r.x + 2, r.y + 2, r.width, r.height), l.Text, word);
                    word.normal.textColor = new Color(l.Colour.r, l.Colour.g, l.Colour.b, alpha);
                    GUI.Label(r, l.Text, word);
                    GUI.matrix = m;
                }
            }
            // the boosts still running, under the lantern row
            float x = 22, top = 232;
            foreach (MapBoost b in owner.Simulation.MapBoosts)
            {
                float left = b.Until - owner.Simulation.Elapsed; if (left <= 0) continue;
                string text = MapBoostText.Label(b.Stat, b.Value);
                float w = chip.CalcSize(new GUIContent(text)).x + 26;
                if (x + w > 420) { x = 22; top += 34; }
                Rect plate = new Rect(x, top, w, 30);
                ArcadeSkin.Plate(plate, new Color(ArcadeSkin.Card.r, ArcadeSkin.Card.g, ArcadeSkin.Card.b, .9f));
                chip.normal.textColor = MapBoostText.Colour(b.Stat);
                GUI.Label(plate, text, chip);
                x += w + 6;
            }
            GUI.matrix = previous;
        }
    }
}
