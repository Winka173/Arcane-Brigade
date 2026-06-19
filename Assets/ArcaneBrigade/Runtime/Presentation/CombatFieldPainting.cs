using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws a hostile field at the simulation's real lane/radius, including its warning phase. Effects are drawn in
    /// code as soft light and ink (requirement 22/10/2026: no painted fire, cracked rock or smoke): the shock ring, the shadow pool,
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
            // requirement 24/10/2026: no painted warning rings and no outlined target zones - the ground cracks toward the zone's true
            // reach instead, in hostile violet and crimson, sparks gathering at the crack tips as the blow comes
            if (warning) { Warn(field, lane, time, staged); if (!staged) return true; }
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
            return true;
        }
        // ------------------------------------------------------------------ code-drawn hostile effects
        /// <summary>A hostile blow gathering: cracks run out from the centre (or along a lane) to the zone's true reach, a dark ink
        /// seeping along them, sparks flickering at their tips faster as the moment nears. No ring, no outline, no filled disc.</summary>
        private void Warn(ExpandedField field, bool lane, float time, bool keepPainted)
        {
            bool still = PresentationSettings.ReducedMotion, sparse = PresentationSettings.ReducedEffects;
            float gather = still ? 1 : Mathf.Clamp01(Mathf.InverseLerp(field.Created, field.At, time));
            int seed = field.GetHashCode() & 1023;
            if (!keepPainted) ink.Clear();
            Vector2 c = field.Position; float r = Mathf.Max(.2f, field.Radius);
            float flicker = still ? 1 : .7f + .3f * Mathf.Sin(time * (10 + 25 * gather) + seed);
            if (lane)
            {
                Vector2 start = field.Start, end = field.Position, along = (end - start).sqrMagnitude > .001f ? (end - start).normalized : Vector2.right, side = Vector2.Perpendicular(along);
                float reach = Vector2.Distance(start, end) * (.25f + .75f * gather);
                var spine = InkCanvas.Line(start, start + along * reach, r * .25f, 0, true, seed);
                ink.GlowRibbon(spine, 1, 0, .16f, A(Night, .75f));
                ink.GlowRibbon(InkCanvas.Line(start, start + along * reach, r * .25f, 0, true, seed), 1, 0, .05f, A(Crimson, .9f * flicker));
                if (!sparse) for (int i = 1; i < 6; i++)
                {
                    float f = i / 6f; if (f * Vector2.Distance(start, end) > reach) break;
                    Vector2 root = start + along * f * Vector2.Distance(start, end), tip = root + side * ((i % 2) * 2 - 1) * r * .9f;
                    ink.GlowRibbon(InkCanvas.Line(root, tip, .06f, 0, true, seed + i), 1, 0, .045f, A(Crimson, .8f * flicker));
                    ink.Sector(tip, .07f, 0, 360, A(Ember, .85f * flicker * gather), A(Ember, 0));
                }
                ink.Commit(); inked = true;
                return;
            }
            int cracks = sparse ? 5 : 8;
            for (int i = 0; i < cracks; i++)
            {
                float a = (i * 360f / cracks + seed * 7 + Mathf.Sin(i * 3.1f) * 14) * Mathf.Deg2Rad, len = r * (.35f + .65f * gather) * (.8f + .2f * ((i * 37) % 5) / 4f);
                Vector2 tip = c + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * len;
                ink.GlowRibbon(InkCanvas.Line(c, tip, len * .12f, 0, true, seed + i), 1, 0, .1f, A(Night, .7f));
                ink.GlowRibbon(InkCanvas.Line(c, tip, len * .12f, 0, true, seed + i), 1, 0, .04f, A(Crimson, .9f * flicker));
                ink.Sector(tip, .06f + .05f * gather, 0, 360, A(Ember, .9f * flicker * gather), A(Ember, 0));
            }
            ink.Commit(); inked = true;
        }
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
                    // the expanding shock: one ember wash spreading out to its true reach, the same colour to its edge
                    // (requirement 25/10/2026: no ring, no thread or sparks along the edge)
                    float r = Mathf.Max(.1f, CarnivalHazards.ShockRadius(field, time));
                    ink.Sector(c, r + .1f, 0, 360, A(Ember, .4f * fade), A(Ember, .4f * fade));
                    break;
                }
                case "ShadowPool":
                {
                    // the lingering dark: a dusk-violet wash that deepens toward its true edge, a soft crimson rim, ink motes rising
                    float r = Mathf.Max(.1f, field.Radius), breathe = still || calm ? 1 : .85f + .15f * Mathf.Sin(time * 4 + seed);
                    ink.Sector(c, r, 0, 360, A(Night, .32f * fade), A(Night, .32f * fade)); // one colour to its edge
                    // (its glowing rim is gone: requirement 24/10/2026, no outlined zones)
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
                    // (no painted outline round the lane any more: requirement 24/10/2026)
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
