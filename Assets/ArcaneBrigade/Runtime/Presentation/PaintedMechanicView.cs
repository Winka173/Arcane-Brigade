using UnityEngine;
namespace ArcaneBrigade
{
    // Clean geometry for every build-morph strike and companion-weapon area: a bright edge swelling out to the real
    // radius over a faint wash, an arc for sweeps, a beam for lines, in the mechanic's colour, on the floor under the
    // bodies. Geometry stays in the simulation and what is drawn is exactly what is hit. The painted "wreath" motifs
    // (MechanicEffectArtwork.Forms: crockery, ribbons of notes, gears, leaves) are no longer drawn: at area size they
    // sheeted over the brigade and read as clutter rather than as the hit.
    public sealed class PaintedMechanicView
    {
        private const int Floor = 64; // with the other area washes (fields 61-65), under every body
        private const int CircleSegments = 48, ArcSegments = 24;
        private static Sprite wash;
        private static Material ink;
        private static readonly AnimationCurve Flat = AnimationCurve.Constant(0, 1, 1);
        private static readonly AnimationCurve Taper = new AnimationCurve(new Keyframe(0, .15f), new Keyframe(.5f, 1), new Keyframe(1, .15f));
        private readonly SpriteRenderer fill;
        private readonly LineRenderer edge, shade;
        private readonly SpriteRenderer painted;
        private const int MoteCount = 6;
        private readonly SpriteRenderer[] motes = new SpriteRenderer[MoteCount];
        public int VisibleSprites => (fill.enabled ? 1 : 0) + (edge.enabled ? 1 : 0) + (shade.enabled ? 1 : 0) + (painted.enabled ? 1 : 0);

        /// <param name="order">Kept for callers; every area now draws on the floor layer.</param>
        public PaintedMechanicView(Transform parent, string name, int order)
        {
            if (ink == null) ink = new Material(Shader.Find("Sprites/Default")) { name = "MechanicAreaInk", hideFlags = HideFlags.DontSave };
            if (wash == null) wash = SoftDisc();
            var go = new GameObject(name + " / wash"); go.transform.SetParent(parent, false);
            fill = go.AddComponent<SpriteRenderer>(); fill.sprite = wash; fill.sortingOrder = Floor; fill.enabled = false;
            shade = Line(parent, name + " / ink edge", Floor + 1);
            edge = Line(parent, name + " / light edge", Floor + 2);
            var ring = new GameObject(name + " / painted ring"); ring.transform.SetParent(parent, false);
            painted = ring.AddComponent<SpriteRenderer>(); painted.sortingOrder = Floor + 3; painted.enabled = false;
            for (int i = 0; i < MoteCount; i++) { var mote = new GameObject(name + " / mote " + i); mote.transform.SetParent(parent, false); motes[i] = mote.AddComponent<SpriteRenderer>(); motes[i].sortingOrder = Floor + 4; motes[i].enabled = false; }
        }
        private static LineRenderer Line(Transform parent, string name, int order)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false);
            var line = go.AddComponent<LineRenderer>();
            line.sharedMaterial = ink; line.useWorldSpace = true; line.sortingOrder = order; line.numCapVertices = 4; line.numCornerVertices = 2;
            line.enabled = false;
            return line;
        }
        /// <summary>A one-unit disc: a faint flat wash that brightens toward a crisp rim.</summary>
        private static Sprite SoftDisc()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "MechanicAreaWash", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.DontSave };
            var pixels = new Color32[size * size]; float half = (size - 1) * .5f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - half) / half, dy = (y - half) / half, d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Lerp(.45f, 1f, d * d * d) * Mathf.Clamp01((1 - d) * 24);
                    pixels[y * size + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(a) * 255));
                }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size, 0, SpriteMeshType.FullRect);
        }

        /// <summary>Draws one area. <paramref name="motif"/> is ignored (kept so callers need not change). <paramref name="mote"/>
        /// names the CombatVFX art its motes are made of (the weapon's own projectile art: notes, leaves, bubbles, sparks...).</summary>
        public void Display(Sprite motif, MorphGeometry geometry, Vector2 position, Vector2 end, Vector2 direction, float radius, float at, float until, float time, Color tint, bool reduced, string mote = null)
        {
            Hide(); if (time >= until) return;
            bool preparing = time < at;
            float phase = Mathf.Clamp01((time - at) / Mathf.Max(.08f, until - at));
            float fade = preparing ? .35f : Mathf.Min(1, (until - time) / .2f);
            // the wider an area, the quieter it is drawn: a 1.4 radius stays as it is, twice that is half as strong
            float quiet = Mathf.Clamp(1.4f / Mathf.Max(.1f, radius), .4f, 1f);
            float alpha = fade * EffectDensity.AreaAlpha * (PresentationSettings.HighReadability ? 1f : .9f) * quiet;
            float rise = Mathf.Clamp01(phase / .4f);
            float swell = reduced || preparing ? 1f : Mathf.Lerp(.78f, 1f, 1 - Mathf.Pow(1 - rise, 3));
            float edgeAlpha = alpha * (preparing ? .5f : Mathf.Lerp(1f, .55f, phase));
            Color light = Color.Lerp(tint, Color.white, .25f); light.a = edgeAlpha;
            Color dark = new Color(.08f, .06f, .1f, edgeAlpha * .35f);
            Vector2 facing = direction.sqrMagnitude > .0001f ? direction.normalized : Vector2.right;
            float r = Mathf.Max(.05f, radius) * swell;
            switch (geometry)
            {
                case MorphGeometry.Line:
                    Stroke(edge, Taper, preparing ? .08f : .2f, light, position, end);
                    if (!PresentationSettings.ReducedEffects) Stroke(shade, Taper, preparing ? .12f : .26f, dark, position, end);
                    if (!preparing)
                    {
                        // a soft band of light along the beam, so a line reads as a beam and not a pencil stroke
                        Vector2 axis = end - position;
                        fill.enabled = true; fill.transform.position = (position + end) * .5f;
                        fill.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg);
                        fill.transform.localScale = new Vector3(axis.magnitude + .3f, reduced ? .45f : .45f + Mathf.Sin(time * 7) * .05f, 1);
                        Color band = tint; band.a = alpha * .32f * (1 - phase * .5f); fill.color = band;
                    }
                    break;
                case MorphGeometry.Crescent:
                    Arc(edge, Taper, preparing ? .08f : .22f, light, position, r, facing);
                    if (!PresentationSettings.ReducedEffects) Arc(shade, Taper, preparing ? .12f : .28f, dark, position, r, facing);
                    break;
                default:
                    float width = geometry == MorphGeometry.Ring ? Mathf.Clamp(radius * .09f, .07f, .14f) : Mathf.Clamp(radius * .07f, .05f, .11f);
                    Circle(edge, width, light, position, r);
                    if (!PresentationSettings.ReducedEffects) Circle(shade, width + .06f, dark, position, r);
                    if (geometry == MorphGeometry.Disc)
                    {
                        fill.enabled = true; fill.transform.position = position; fill.transform.localScale = Vector3.one * (2 * r);
                        Color inside = tint; inside.a = alpha * (preparing ? .1f : .2f * (1 - phase * .5f)); fill.color = inside;
                    }
                    // the painted ring of light over the edge, turning slowly
                    Sprite ringArt = CombatVfxArtwork.Frame("BlastRing", 0);
                    if (ringArt != null && !preparing)
                    {
                        painted.enabled = true; painted.sprite = ringArt;
                        painted.transform.position = position; painted.transform.rotation = Quaternion.Euler(0, 0, reduced ? 0 : time * 40);
                        painted.transform.localScale = Vector3.one * (2 * r * 1.04f / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(ringArt).width));
                        painted.color = new Color(light.r, light.g, light.b, edgeAlpha * .7f);
                    }
                    break;
            }
            Motes(geometry, position, end, facing, r, phase, preparing, reduced, time, mote, alpha);
        }
        // the area's life: the weapon's own art moving through it. Discs swirl, rings run round, sweeps follow the cut,
        // beams carry sparks from source to end
        private void Motes(MorphGeometry geometry, Vector2 position, Vector2 end, Vector2 facing, float r, float phase, bool preparing, bool reduced, float time, string key, float alpha)
        {
            Sprite art = CombatVfxArtwork.Frame(key ?? "AllySpark", 0) ?? CombatVfxArtwork.Frame("AllySpark", 0);
            int shown = art == null || preparing ? 0 : PresentationSettings.ReducedEffects || r > 2.2f ? 2 : r > 1.6f ? 4 : MoteCount; // big areas carry few motes
            float size = Mathf.Clamp(.12f + r * .05f, .12f, .26f);
            for (int i = 0; i < MoteCount; i++)
            {
                SpriteRenderer view = motes[i]; view.enabled = i < shown; if (!view.enabled) continue;
                float lane = i / (float)MoteCount, travel = reduced ? lane : Mathf.Repeat(lane + phase * 1.3f, 1f);
                Vector2 at;
                switch (geometry)
                {
                    case MorphGeometry.Line: at = Vector2.Lerp(position, end, travel) + new Vector2(-(end - position).y, (end - position).x).normalized * Mathf.Sin((travel + i) * 9f) * .08f; break;
                    case MorphGeometry.Crescent:
                    {
                        float middle = Mathf.Atan2(facing.y, facing.x), sweep = middle + (travel * 2 - 1) * 100 * Mathf.Deg2Rad;
                        at = position + new Vector2(Mathf.Cos(sweep), Mathf.Sin(sweep)) * r * (.9f + .1f * Mathf.Sin(i * 2.1f)); break;
                    }
                    case MorphGeometry.Ring: { float a = (lane + (reduced ? 0 : time * .35f)) * Mathf.PI * 2; at = position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r; break; }
                    default: { float a = (lane + (reduced ? 0 : time * .5f)) * Mathf.PI * 2, reach = r * (.35f + .55f * Mathf.Abs(Mathf.Sin(travel * Mathf.PI))); at = position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * reach; break; }
                }
                view.sprite = art; view.transform.position = at;
                view.transform.rotation = Quaternion.Euler(0, 0, reduced ? 0 : (i % 2 == 0 ? 1 : -1) * time * 140 + i * 40);
                view.transform.localScale = Vector3.one * (size / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(art).width));
                view.color = new Color(1, 1, 1, alpha * Mathf.Clamp01(Mathf.Sin(travel * Mathf.PI) * 2.2f));
            }
        }
        private static void Stroke(LineRenderer line, AnimationCurve curve, float width, Color color, Vector2 from, Vector2 to)
        {
            line.enabled = color.a > .005f; if (!line.enabled) return;
            line.loop = false; line.widthCurve = curve; line.widthMultiplier = width; line.startColor = line.endColor = color;
            line.positionCount = 2; line.SetPosition(0, from); line.SetPosition(1, to);
        }
        private static void Circle(LineRenderer line, float width, Color color, Vector2 centre, float radius)
        {
            line.enabled = color.a > .005f; if (!line.enabled) return;
            line.loop = true; line.widthCurve = Flat; line.widthMultiplier = width; line.startColor = line.endColor = color;
            line.positionCount = CircleSegments;
            for (int i = 0; i < CircleSegments; i++) { float a = i * Mathf.PI * 2 / CircleSegments; line.SetPosition(i, centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius); }
        }
        // a sweep: about a hundred degrees either side of the strike's direction (the default cone, Dot >= -0.2)
        private static void Arc(LineRenderer line, AnimationCurve curve, float width, Color color, Vector2 centre, float radius, Vector2 facing)
        {
            line.enabled = color.a > .005f; if (!line.enabled) return;
            line.loop = false; line.widthCurve = curve; line.widthMultiplier = width; line.startColor = line.endColor = color;
            line.positionCount = ArcSegments; float middle = Mathf.Atan2(facing.y, facing.x), spread = 100 * Mathf.Deg2Rad;
            for (int i = 0; i < ArcSegments; i++) { float a = middle - spread + 2 * spread * i / (ArcSegments - 1); line.SetPosition(i, centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius); }
        }
        public void Hide() { fill.enabled = edge.enabled = shade.enabled = painted.enabled = false; foreach (SpriteRenderer mote in motes) mote.enabled = false; }
    }
}
