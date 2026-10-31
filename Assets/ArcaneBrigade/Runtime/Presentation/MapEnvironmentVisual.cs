using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws the map's living elements (play-test item 24, D11 v2) from MapEnvironmentState: the painted props
    /// (Resources/Environment, cut from Art/Playtest/Environment) and their warnings in brushed ink. Warnings are outlines,
    /// arrows and dashed edges (amber for danger, gold for gifts); the lamp light is light on air, never a pool on the floor.
    /// Requirement 25/10/2026: each map draws in its own file under Presentation/Maps (Draw{Theme}, {Theme}PropLight; its air in
    /// MapAtmosphere's Air{Theme}); this file only dispatches and keeps the shared helpers.</summary>
    public sealed partial class MapEnvironmentVisual : MonoBehaviour
    {
        private ArsenalInk ink;
        private bool drawn;
        private readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        // standing map props float like the rest of the dream (DreamLevitation) and cast a ground shadow (requirement 22/10/2026)
        private readonly List<GroundShadow> shadows = new List<GroundShadow>();
        private int used;
        private CombatSimulation sim;
        private bool still, calm;
        private MapAtmosphere atmosphere;
        // what the last frame showed (read by checks and captures)
        public int Props { get; private set; }
        public int Marks { get; private set; }
        private const int Air = 2150, Floor = -40;

        public void Display(CombatSimulation simulation)
        {
            Props = Marks = 0; used = 0; sim = simulation;
            // each map's own air and colour grade (MapAtmosphere), drawn under the floor marks and among the units
            if (atmosphere == null) { atmosphere = GetComponent<MapAtmosphere>(); if (atmosphere == null) atmosphere = gameObject.AddComponent<MapAtmosphere>(); }
            atmosphere.Display(simulation);
            MapEnvironmentState s = simulation?.MapEnvironment;
            bool live = s != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (!live)
            {
                for (int i = 0; i < pool.Count; i++) { pool[i].enabled = false; shadows[i].Hide(); }
                if (drawn) { ink.Begin(); ink.End(); drawn = false; }
                return;
            }
            if (ink == null) ink = new ArsenalInk(transform);
            still = PresentationSettings.ReducedMotion; calm = PresentationSettings.ReducedFlashes;
            ink.Begin(); drawn = true;
            switch (s.Theme)
            {
                case MapTheme.WaitingRoom: DrawWaitingRoom(s, simulation.Elapsed); break;
                case MapTheme.PostOffice: DrawPostOffice(s, simulation.Elapsed); break;
                case MapTheme.Glasshouse: DrawGlasshouse(s, simulation.Elapsed); break;
                case MapTheme.LampHall: DrawLampHall(s, simulation.Elapsed); break;
                case MapTheme.Theatre: DrawTheatre(s, simulation.Elapsed); break;
                case MapTheme.ArchiveVault: DrawArchiveVault(s, simulation.Elapsed); break;
            }
            DrawCatches(s, simulation.Elapsed);
            ink.End();
            for (int i = used; i < pool.Count; i++) { pool[i].enabled = false; shadows[i].Hide(); }
        }

        /// <summary>A pickup flashes where it lay and its light flies into the hero in a few sparks.</summary>
        private void DrawCatches(MapEnvironmentState s, float t)
        {
            if (sim.Champion == null) return;
            Vector2 hero = Head(sim.Champion) - Vector2.up * .3f;
            foreach (MapCatch c in s.Catches)
            {
                float age = t - c.Time; if (age < 0 || age > .75f) continue;
                float k = age / .75f;
                ink.Light(c.At, 1.4f + 1.2f * k, c.Colour, .65f * (1 - k));
                for (int i = 0; i < 6; i++)
                {
                    float d = still ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - i * .035f) / .45f));
                    Vector2 side = new Vector2(Mathf.Cos(i * 60 * Mathf.Deg2Rad), Mathf.Sin(i * 60 * Mathf.Deg2Rad)) * .55f * Mathf.Sin(d * Mathf.PI);
                    ink.TokenAt(InkToken.Glint, ArsenalInk.Gold, Vector2.Lerp(c.At, hero, d) + side, .22f, i * 40 + age * 300, 1 - Mathf.Clamp01((age - .5f) / .25f));
                }
            }
        }
        /// <summary>How much a map's own light lifts a standing prop at this spot back to full (0 none, 1 fully lit).</summary>
        private float MapPropLight(MapEnvironmentState s, Vector2 at) => s.Theme switch
        {
            MapTheme.WaitingRoom => WaitingRoomPropLight(s, at),
            MapTheme.PostOffice => PostOfficePropLight(s, at),
            MapTheme.Glasshouse => GlasshousePropLight(s, at),
            MapTheme.LampHall => LampHallPropLight(s, at),
            MapTheme.Theatre => TheatrePropLight(s, at),
            MapTheme.ArchiveVault => ArchiveVaultPropLight(s, at),
            _ => 0
        };

        // ------------------------------------------------------------------ helpers
        private static Vector2 Head(UnitState u) => u.Position + Vector2.up * (u.Definition.BodyHeight * .62f + .12f);

        private SpriteRenderer Put(string path, Vector2 at, float height, int order = int.MinValue, float alpha = 1, bool flip = false, float angle = 0, Color? tint = null)
        {
            Sprite sprite = MapIdentity.Sprite(path);
            if (sprite == null || alpha <= .01f) return null;
            if (used == pool.Count)
            {
                var go = new GameObject("Map element"); go.transform.SetParent(transform, false);
                pool.Add(go.AddComponent<SpriteRenderer>());
                var shade = new GameObject("Map element shadow"); shade.transform.SetParent(transform, false);
                shadows.Add(shade.AddComponent<GroundShadow>());
            }
            SpriteRenderer r = pool[used++]; r.enabled = true; r.sprite = sprite; r.flipX = flip;
            float scale = height / Mathf.Max(.01f, sprite.bounds.size.y);
            bool standing = order == int.MinValue; // floor glows and things in the air (letters, moths, hands) keep their own motion
            Color c = tint ?? Color.white;
            if (standing) { Color shade = Shade(at); c.r *= shade.r; c.g *= shade.g; c.b *= shade.b; } // lit like the floor under it
            c.a *= Mathf.Clamp01(alpha); r.color = c;
            int slot = used - 1;
            Vector3 hover = standing ? DreamLevitation.Offset(sim != null ? sim.Elapsed : Time.time, 40 + slot * 1.37f, 1, still, .8f) : Vector3.zero;
            r.transform.position = (Vector3)at + hover; r.transform.rotation = Quaternion.Euler(0, 0, angle + (standing ? DreamLevitation.Tilt(sim != null ? sim.Elapsed : Time.time, 40 + slot * 1.37f, still, .5f) : 0)); r.transform.localScale = new Vector3(scale, scale, 1);
            r.sortingOrder = order != int.MinValue ? order : 1000 - Mathf.RoundToInt((at.y + .35f) * 12);
            if (standing) shadows[slot].Display(sprite, at, sprite.bounds.size.x * scale * .8f, hover.y, .32f * Mathf.Clamp01(alpha));
            else shadows[slot].Hide();
            Props++;
            return r;
        }

        /// <summary>How a standing prop is lit (requirement 22/10/2026: some props stayed bright on a dark floor): the map's grade and
        /// edges (MapAtmosphere.PropShade) and the night's light field like every other prop; a map's own light (a lit lamp's oval)
        /// lifts it back.</summary>
        private Color Shade(Vector2 at)
        {
            Color shade = MapAtmosphere.PropShade(at);
            float field = SharedLightField.Active?.SurfaceVisibility(at) ?? 1;
            shade.r *= field; shade.g *= field; shade.b *= field;
            MapEnvironmentState s = sim?.MapEnvironment;
            if (s != null)
            {
                float lit = MapPropLight(s, at);
                if (lit > 0) shade = Color.Lerp(shade, Color.white, lit);
            }
            return shade;
        }
        /// <summary>A dashed brushed rim (outline only) that closes with the charge.</summary>
        /// <summary>An area of floor in its colour: the whole area softly, filling brighter from the centre as it charges.</summary>
        private void Ring(Vector2 c, float rx, float ry, float charge, InkPalette pal, float alpha, float from = 0, float to = 360)
        {
            // one colour from the centre to the edge (requirement 25/10/2026): the whole area brightens as it charges
            Color fill = Fill(pal, charge, alpha);
            if (to - from >= 359) ink.FloorZone(c, rx, ry, fill, fill);
            else ink.Cone(c, Mathf.Max(rx, ry), from, to, fill, fill.a, fill.a);
            Marks++;
        }
        private static Color Fill(InkPalette pal, float charge, float alpha)
        {
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            Color c = Color.Lerp(pal.Body, pal.Glow, s * .6f); c.a = (.34f + .14f * s) * alpha;
            return c;
        }
        /// <summary>A band of floor in its colour, filling from its start as it charges.</summary>
        private void Band(Vector2 a, Vector2 b, float half, float charge, InkPalette pal, float alpha)
        {
            Color fill = Fill(pal, charge, alpha);
            ink.FloorLane(a, b, half, fill, fill);
            Marks++;
        }
        private void Dash(Vector2 a, Vector2 b, float charge, InkPalette pal, float alpha)
        {
            float length = Vector2.Distance(a, b); if (length < .05f) return;
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            // a solid band growing along it (no dashes)
            Color fill = Fill(pal, s, alpha);
            ink.FloorLane(a, Vector2.Lerp(a, b, Mathf.Max(.05f, s)), .1f, fill, fill);
            Marks++;
        }
        /// <summary>Where something will slide: its band filled, and an arrowhead.</summary>
        private void Lane(Vector2 from, Vector2 to, float half, float charge, InkPalette pal, float alpha)
        {
            Vector2 axis = to - from; if (axis.sqrMagnitude < .01f) return;
            Vector2 dir = axis.normalized, n = new Vector2(-dir.y, dir.x) * half;
            Band(from, to, half, charge, pal, alpha); // the slide's whole band, and an arrowhead for its way
            // no arrowhead lines over the band
        }

        private void OnDestroy() { ink?.Destroy(); }
    }
}
