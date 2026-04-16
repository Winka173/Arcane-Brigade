using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws a hostile field at the simulation's real lane/radius, including its warning phase. Effects are drawn in
    /// code as soft light and ink (owner 22/10/2026: no painted fire, cracked rock or smoke): the shock ring, the shadow pool,
    /// the lash and the sewing thread. Painted objects (curtain, cabinet, toy box) and the red telegraph outlines stay.</summary>
    public sealed class CombatFieldPainting
    {
        private readonly SpriteRenderer view, outline;
        private readonly InkCanvas ink;
        private bool inked;
        public CombatFieldPainting(Transform parent, int index)
        {
            var go = new GameObject("Illustrated field " + index); go.transform.SetParent(parent, false);
            view = go.AddComponent<SpriteRenderer>(); view.sortingOrder = 62; // on the floor with every other area, under the bodies
            var danger = new GameObject("Illustrated warning edge " + index); danger.transform.SetParent(parent, false);
            outline = danger.AddComponent<SpriteRenderer>(); outline.sortingOrder = 63;
            ink = new InkCanvas(parent, "Hostile field light " + index, 62);
        }
        public void Hide() { view.enabled = outline.enabled = false; if (inked) { ink.Hide(); inked = false; } }
        public bool Display(ExpandedField field, float time)
        {
            Hide();
            if (time > field.Until) return false;
            bool warning = time < field.At;
            string key = field.Kind switch
            {
                ExpandedFieldKind.Tentacle => "Tentacle", ExpandedFieldKind.Shockwave => "ShockRing",
                ExpandedFieldKind.ShadowPool => "ShadowPool", ExpandedFieldKind.Curtain => "Curtain",
                ExpandedFieldKind.OfficeHazard => "OfficeHazard", ExpandedFieldKind.ToyHazard => "ToyHazard",
                ExpandedFieldKind.SewingLink => "Thread", _ => null
            };
            if (key == null) return false;
            bool lane = field.Kind == ExpandedFieldKind.Tentacle || key == "Thread";
            bool staged = key == "Tentacle" || key == "Curtain" || key == "OfficeHazard" || key == "ToyHazard";
            bool coded = key == "ShockRing" || key == "ShadowPool" || key == "Tentacle" || key == "Thread";
            if (coded && (!warning || staged)) { Code(field, key, time, warning); return true; }
            Sprite[] frames = CombatVfxArtwork.Frames(warning && !staged ? lane ? "TelegraphLane" : "TelegraphCircle" : key);
            if (frames == null || frames.Length == 0) return false;
            int frame = warning || PresentationSettings.ReducedMotion ? 0 : Mathf.FloorToInt((time - field.At) * 10) % frames.Length;
            // These two-frame hazards explicitly author anticipation then impact, rather than looping the warning.
            if (!warning && staged) frame = frames.Length - 1;
            Sprite image = frames[frame]; if (image == null) return false;
            Vector2 position = field.Position;
            float radius = field.Kind == ExpandedFieldKind.Shockwave && !warning ? CarnivalHazards.ShockRadius(field, time) : field.Radius;
            Vector2 size = Vector2.one * Mathf.Max(.05f, radius * 2);
            float angle = 0;
            if (lane)
            {
                Vector2 start = key == "Thread" ? field.Anchor != null ? field.Anchor.Position : field.Source?.Position ?? field.Start : field.Start;
                Vector2 end = key == "Thread" ? field.Target?.Position ?? field.Position : field.Position;
                position = (start + end) * .5f;
                size = new Vector2(Mathf.Max(.1f, Vector2.Distance(start, end) + (key == "Thread" ? 0 : radius * 2)), key == "Thread" ? .12f : radius * 2);
                angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            }
            Rect ink = SpriteInkMetrics.BoundsOf(image);
            // a staged hazard gathers during its warning (75% -> full) while its true edge is shown at full size
            float gather = warning && staged && !PresentationSettings.ReducedMotion ? Mathf.InverseLerp(field.Created, field.At, time) : 1;
            Vector2 shown = size * (warning && staged ? .75f + .25f * gather : 1);
            view.sprite = image; view.enabled = true;
            view.transform.rotation = Quaternion.Euler(0, 0, angle);
            view.transform.localScale = new Vector3(shown.x / Mathf.Max(.01f, ink.width), shown.y / Mathf.Max(.01f, ink.height), 1);
            view.transform.position = (Vector3)position - view.transform.TransformVector(ink.center);
            float fade = warning ? (staged ? .55f + .3f * gather : .85f) : Mathf.Lerp(staged ? .95f : 1, staged ? .2f : .5f, Mathf.Clamp01((time - field.At) / Mathf.Max(.05f, field.Until - field.At)));
            view.color = new Color(1, 1, 1, fade * (key == "Thread" ? EffectDensity.AlliedAlpha : 1));
            if (warning && staged)
            {
                Sprite edge = CombatVfxArtwork.Frame(lane ? "TelegraphLane" : "TelegraphCircle", 0);
                if (edge != null)
                {
                    Rect border = SpriteInkMetrics.BoundsOf(edge); outline.sprite = edge; outline.enabled = true;
                    outline.transform.rotation = view.transform.rotation;
                    outline.transform.localScale = new Vector3(size.x / Mathf.Max(.01f, border.width), size.y / Mathf.Max(.01f, border.height), 1);
                    outline.transform.position = (Vector3)position - outline.transform.TransformVector(border.center);
                    // the edge brightens and beats faster as the blow comes
                    float beat = PresentationSettings.ReducedFlashes || PresentationSettings.ReducedMotion ? 0 : .12f * Mathf.Sin(time * (8 + 22 * gather));
                    outline.color = new Color(1, 1, 1, Mathf.Clamp01(.45f + .4f * gather + beat));
                }
            }
            return true;
        }
        // ------------------------------------------------------------------ code-drawn hostile effects
        private static readonly Color Ember = new Color(1f, .42f, .16f), Hot = new Color(1f, .86f, .55f), Crimson = new Color(.62f, .06f, .12f), Night = new Color(.2f, .05f, .2f);
        private void Code(ExpandedField field, string key, float time, bool warning)
        {
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects, calm = PresentationSettings.ReducedFlashes;
            float dim = sparse ? .75f : 1, life = Mathf.Clamp01((time - field.At) / Mathf.Max(.05f, field.Until - field.At));
            float fade = (warning ? .55f : Mathf.Lerp(1, .25f, life)) * dim;
            float clock = still ? 0 : time;
            int seed = field.GetHashCode() & 1023;
            Vector2 c = field.Position;
            ink.Clear();
            switch (key)
            {
                case "ShockRing":
                {
                    // the expanding shock: a soft ember band at its true edge, a hot thread along it and sparks lifting off
                    float r = Mathf.Max(.1f, CarnivalHazards.ShockRadius(field, time));
                    ink.Annulus(c, Mathf.Max(0, r - .4f), r + .1f, A(Ember, 0), A(Ember, .55f * fade));
                    ink.GlowRibbon(InkCanvas.Arc(c, r, r, 0, 360), 1, 0, .22f, A(Hot, .8f * fade));
                    if (!sparse) for (int i = 0; i < 10; i++)
                    {
                        float a = (i * 36 + seed) * Mathf.Deg2Rad, lift = Mathf.Repeat(clock * 1.4f + i * .37f, 1);
                        Vector2 at = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (r - .05f) + Vector2.up * lift * .35f;
                        ink.Sector(at, .07f + .05f * (1 - lift), 0, 360, A(Hot, .85f * fade * (1 - lift)), A(Ember, 0));
                    }
                    break;
                }
                case "ShadowPool":
                {
                    // the lingering dark: a dusk-violet wash that deepens toward its true edge, a soft crimson rim, ink motes rising
                    float r = Mathf.Max(.1f, field.Radius), breathe = still || calm ? 1 : .85f + .15f * Mathf.Sin(time * 4 + seed);
                    ink.Sector(c, r, 0, 360, A(Night, .12f * fade), A(Night, .42f * fade));
                    ink.GlowRibbon(InkCanvas.Arc(c, r - .04f, r - .04f, 0, 360), 1, 0, .2f, A(Crimson, .7f * fade * breathe));
                    ink.GlowRibbon(InkCanvas.Arc(c, r - .04f, r - .04f, 0, 360), 1, 0, .07f, A(Ember, .55f * fade * breathe));
                    if (!sparse) for (int i = 0; i < 6; i++)
                    {
                        float rise = Mathf.Repeat(clock * .5f + i / 6f, 1), a = (i * 61 + seed) * Mathf.Deg2Rad, d = r * (.25f + .55f * ((i * 37 % 10) / 10f));
                        Vector2 at = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * d + Vector2.up * rise * .6f;
                        ink.Sector(at, .08f + .04f * rise, 0, 360, A(Night, .6f * fade * Mathf.Sin(rise * Mathf.PI)), A(Night, 0));
                    }
                    break;
                }
                case "Tentacle":
                {
                    // the lash: a crimson body of light along the true lane that gathers during its warning, a hot core when it strikes
                    Vector2 a = field.Start, b = field.Position; float half = Mathf.Max(.08f, field.Radius);
                    float gather = warning ? (still ? 1 : Mathf.InverseLerp(field.Created, field.At, time)) : 1;
                    Vector2 tip = Vector2.Lerp(a, b, warning ? .75f + .25f * gather : 1);
                    var lash = InkCanvas.Line(a, tip, half * .3f, 3, false, seed + clock * 2);
                    ink.Band(a, tip, half, A(Crimson, (warning ? .18f + .2f * gather : .4f) * fade), A(Crimson, 0));
                    ink.GlowRibbon(lash, 1, 0, half * 1.4f, A(Crimson, (warning ? .45f : .8f) * fade));
                    if (!warning) ink.GlowRibbon(lash, 1, 0, half * .5f, A(Ember, .85f * fade));
                    else
                    {
                        // the red telegraph outline stays a painted ink edge, brightening and beating faster as the blow comes
                        Sprite edge = CombatVfxArtwork.Frame("TelegraphLane", 0);
                        if (edge != null)
                        {
                            Rect border = SpriteInkMetrics.BoundsOf(edge); outline.sprite = edge; outline.enabled = true;
                            outline.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
                            outline.transform.localScale = new Vector3((Vector2.Distance(a, b) + half * 2) / Mathf.Max(.01f, border.width), half * 2 / Mathf.Max(.01f, border.height), 1);
                            outline.transform.position = (Vector3)((a + b) * .5f) - outline.transform.TransformVector(border.center);
                            float beat = calm || still ? 0 : .12f * Mathf.Sin(time * (8 + 22 * gather));
                            outline.color = new Color(1, 1, 1, Mathf.Clamp01(.45f + .4f * gather + beat));
                        }
                    }
                    break;
                }
                default: // the sewing thread
                {
                    Vector2 a = field.Anchor != null ? field.Anchor.Position : field.Source?.Position ?? field.Start, b = field.Target?.Position ?? field.Position;
                    var path = InkCanvas.Lob(a, b, -.15f, 14);
                    float alpha = fade * EffectDensity.AlliedAlpha;
                    ink.GlowRibbon(path, 1, 0, .16f, A(Crimson, .55f * alpha));
                    ink.GlowRibbon(path, 1, 0, .05f, A(Hot, .7f * alpha));
                    break;
                }
            }
            ink.Commit(); inked = true;
        }
        private static Color A(Color c, float alpha) { c.a = Mathf.Clamp01(alpha); return c; }
    }
}
