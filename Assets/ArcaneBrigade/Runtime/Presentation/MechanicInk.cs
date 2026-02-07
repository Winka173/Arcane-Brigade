using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum InkToken { Spark, Ember, Star, Leaf, Drop, Die, Note, Shard, Coin, Petal, Puff, Bubble, Spear, Button, Fang, Thorn, Citrus, Seal, Patch, Page, Bolt, Moon, Dumpling, Knot, Sand, Envelope, Glint }

    /// <summary>Colours for one mechanic: dark ink contour, painted body, cream light, soft glow and a secondary accent.</summary>
    public struct InkPalette
    {
        public Color Ink, Body, Light, Glow, Accent;
        public InkPalette(Color ink, Color body, Color light, Color glow, Color accent) { Ink = ink; Body = body; Light = light; Glow = glow; Accent = accent; }
        public int Key => Mathf.RoundToInt(Body.r * 31) | Mathf.RoundToInt(Body.g * 31) << 5 | Mathf.RoundToInt(Body.b * 31) << 10 | Mathf.RoundToInt(Ink.r * 15) << 15 | Mathf.RoundToInt(Accent.r * 15) << 19 | Mathf.RoundToInt(Accent.b * 15) << 23 | Mathf.RoundToInt(Light.b * 15) << 27;
    }

    /// <summary>Procedural storybook ink: brush-stroke and paper-wash textures for mesh ribbons, plus small painted tokens
    /// (sparks, envelopes, leaves, coins...) with dark contours, lit cream edges and shaded undersides. Built once, cached.</summary>
    public static class MechanicInk
    {
        private static Texture2D stroke, wash, glowTexture;
        private static Material strokeMaterial, washMaterial;
        private static Sprite glow;
        private static readonly Dictionary<long, Sprite> tokens = new Dictionary<long, Sprite>();
        private const int TokenSize = 96;

        public static Material StrokeMaterial { get { if (strokeMaterial == null) strokeMaterial = Make(Stroke(), "MechanicInk stroke"); return strokeMaterial; } }
        public static Material WashMaterial { get { if (washMaterial == null) washMaterial = Make(Wash(), "MechanicInk wash"); return washMaterial; } }
        public static Sprite Glow
        {
            get
            {
                if (glow != null) return glow;
                const int n = 64; glowTexture = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "MechanicInk glow", wrapMode = TextureWrapMode.Clamp };
                var px = new Color32[n * n];
                for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
                {
                    float dx = (x + .5f) / n * 2 - 1, dy = (y + .5f) / n * 2 - 1, r = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1 - r); a = a * a * (3 - 2 * a); px[y * n + x] = new Color(1, 1, 1, a * a);
                }
                glowTexture.SetPixels32(px); glowTexture.Apply(false, true);
                glow = Sprite.Create(glowTexture, new Rect(0, 0, n, n), new Vector2(.5f, .5f), n);
                return glow;
            }
        }

        private static Material Make(Texture2D texture, string name)
        {
            var shader = Shader.Find("Sprites/Default");
            return new Material(shader) { name = name, mainTexture = texture };
        }

        /// <summary>GLSL-style smoothstep (Unity's Mathf.SmoothStep interpolates values instead).</summary>
        private static float Step(float edge0, float edge1, float x) { float k = Mathf.Clamp01((x - edge0) / (edge1 - edge0)); return k * k * (3 - 2 * k); }
        // ---------- noise ----------
        private static float Hash(int x, int y) { unchecked { uint h = (uint)(x * 374761393 + y * 668265263); h = (h ^ (h >> 13)) * 1274126177u; return ((h ^ (h >> 16)) & 0xffffff) / 16777215f; } }
        private static float Noise(float x, float y, int period = 0)
        {
            int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y); float fx = x - ix, fy = y - iy;
            fx = fx * fx * (3 - 2 * fx); fy = fy * fy * (3 - 2 * fy);
            int x0 = ix, x1 = ix + 1, y0 = iy, y1 = iy + 1;
            if (period > 0) { x0 = ((x0 % period) + period) % period; x1 = ((x1 % period) + period) % period; y0 = ((y0 % period) + period) % period; y1 = ((y1 % period) + period) % period; }
            return Mathf.Lerp(Mathf.Lerp(Hash(x0, y0), Hash(x1, y0), fx), Mathf.Lerp(Hash(x0, y1), Hash(x1, y1), fx), fy);
        }
        private static float Fbm(float x, float y, int period = 0)
        {
            float sum = 0, amp = .5f, norm = 0;
            for (int o = 0; o < 4; o++) { sum += Noise(x, y, period) * amp; norm += amp; x *= 2; y *= 2; amp *= .5f; if (period > 0) period *= 2; }
            return sum / norm;
        }

        // ---------- ribbon textures ----------
        private static Texture2D Stroke()
        {
            if (stroke != null) return stroke;
            const int w = 256, h = 64; stroke = new Texture2D(w, h, TextureFormat.RGBA32, true) { name = "MechanicInk brush", wrapModeU = TextureWrapMode.Repeat, wrapModeV = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 2 };
            var px = new Color[w * h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++)
            {
                float u = x / (float)w, v = (y + .5f) / h, across = Mathf.Abs(v - .5f) * 2;
                float edge = .8f + (Fbm(u * 8, 1.3f, 8) - .5f) * .26f - (Fbm(u * 8, 7.1f, 8) - .5f) * .1f * Mathf.Sign(v - .5f);
                float a = 1 - Step(edge - .16f, edge, across);
                float streak = .88f + .12f * Noise(v * 26 + Fbm(u * 4, 3, 4) * 3, 2.5f);
                float dry = Step(.6f, .78f, Fbm(u * 16, v * 3.5f, 16)) * Step(.25f, .9f, across);
                a *= 1 - dry * .7f;
                px[y * w + x] = new Color(streak, streak, streak, a);
            }
            stroke.SetPixels(px); stroke.Apply(true, true); return stroke;
        }
        private static Texture2D Wash()
        {
            if (wash != null) return wash;
            const int n = 128; wash = new Texture2D(n, n, TextureFormat.RGBA32, true) { name = "MechanicInk paper wash", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear };
            var px = new Color[n * n];
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                float u = x / (float)n, v = y / (float)n;
                float tone = .82f + .18f * Fbm(u * 4, v * 4, 4), pool = .62f + .38f * Fbm(u * 3 + 9, v * 3 + 2, 3);
                px[y * n + x] = new Color(tone, tone, tone, pool);
            }
            wash.SetPixels(px); wash.Apply(true, true); return wash;
        }

        // ---------- signed distance helpers (shape space is [-1,1], y up, negative inside) ----------
        private static float Circle(Vector2 p, Vector2 c, float r) => (p - c).magnitude - r;
        private static float Box(Vector2 p, Vector2 c, Vector2 half, float round, float angle = 0)
        {
            p -= c; if (angle != 0) p = Rotate(p, -angle);
            Vector2 d = new Vector2(Mathf.Abs(p.x), Mathf.Abs(p.y)) - half + Vector2.one * round;
            return new Vector2(Mathf.Max(d.x, 0), Mathf.Max(d.y, 0)).magnitude + Mathf.Min(Mathf.Max(d.x, d.y), 0) - round;
        }
        private static float Segment(Vector2 p, Vector2 a, Vector2 b, float r)
        {
            Vector2 pa = p - a, ba = b - a; float h = Mathf.Clamp01(Vector2.Dot(pa, ba) / Mathf.Max(1e-5f, Vector2.Dot(ba, ba)));
            return (pa - ba * h).magnitude - r;
        }
        private static float Uneven(Vector2 p, Vector2 bottom, float r1, float r2, float hgt)
        {
            p -= bottom; p.x = Mathf.Abs(p.x);
            float b = (r1 - r2) / hgt, a = Mathf.Sqrt(Mathf.Max(0, 1 - b * b)), k = Vector2.Dot(p, new Vector2(-b, a));
            if (k < 0) return p.magnitude - r1;
            if (k > a * hgt) return (p - new Vector2(0, hgt)).magnitude - r2;
            return Vector2.Dot(p, new Vector2(a, b)) - r1;
        }
        private static float Polygon(Vector2 p, Vector2[] v)
        {
            float d = Vector2.Dot(p - v[0], p - v[0]), s = 1;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                Vector2 e = v[j] - v[i], w = p - v[i], b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s = -s;
            }
            return s * Mathf.Sqrt(d);
        }
        private static Vector2[] StarPoints(int points, float outer, float inner, float rotation)
        {
            var v = new Vector2[points * 2];
            for (int i = 0; i < v.Length; i++) { float a = rotation + i * Mathf.PI / points; float r = i % 2 == 0 ? outer : inner; v[i] = new Vector2(Mathf.Sin(a), Mathf.Cos(a)) * r; }
            return v;
        }
        private static Vector2 Rotate(Vector2 p, float degrees) { float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r); return new Vector2(p.x * c - p.y * s, p.x * s + p.y * c); }
        private static float SmoothMin(float a, float b, float k) { float h = Mathf.Clamp01(.5f + .5f * (b - a) / k); return Mathf.Lerp(b, a, h) - k * h * (1 - h); }
        private static float Ring(Vector2 p, Vector2 c, float r, float w) => Mathf.Abs((p - c).magnitude - r) - w;

        private static readonly Vector2[] ShardPoly = { new Vector2(.02f, .92f), new Vector2(.34f, .18f), new Vector2(.12f, -.88f), new Vector2(-.3f, -.12f) };
        private static readonly Vector2[] SpearPoly = { new Vector2(0, .95f), new Vector2(.3f, .22f), new Vector2(.1f, .12f), new Vector2(.11f, -.9f), new Vector2(-.11f, -.9f), new Vector2(-.1f, .12f), new Vector2(-.3f, .22f) };
        private static readonly Vector2[] BoltPoly = { new Vector2(-.12f, .95f), new Vector2(.5f, .95f), new Vector2(.12f, .22f), new Vector2(.48f, .22f), new Vector2(-.38f, -.95f), new Vector2(-.06f, -.08f), new Vector2(-.42f, -.08f) };
        private static readonly Vector2[] Spark4 = StarPoints(4, .92f, .22f, 0), Spark4b = StarPoints(4, .5f, .14f, Mathf.PI / 4), Star5 = StarPoints(5, .9f, .42f, 0), Glint4 = StarPoints(4, .9f, .16f, 0);
        private static readonly Vector2[] HourTop = { new Vector2(-.56f, .78f), new Vector2(.56f, .78f), new Vector2(.08f, 0), new Vector2(-.08f, 0) }, HourBottom = { new Vector2(-.56f, -.78f), new Vector2(-.08f, 0), new Vector2(.08f, 0), new Vector2(.56f, -.78f) };

        /// <summary>Main silhouette. face: die face for Die.</summary>
        private static float Shape(InkToken kind, Vector2 p, int face)
        {
            switch (kind)
            {
                case InkToken.Spark: return Mathf.Min(Polygon(p, Spark4), Polygon(p, Spark4b)) - .02f;
                case InkToken.Glint: return Polygon(p, Glint4) - .01f;
                case InkToken.Star: return Polygon(p, Star5) - .05f;
                case InkToken.Ember: { Vector2 q = p; q.x += .14f * Mathf.Sin((p.y + .3f) * 2.6f) * Mathf.Clamp01(p.y + .4f); return Uneven(q, new Vector2(0, -.42f), .46f, .04f, 1.28f); }
                case InkToken.Drop: return Uneven(p, new Vector2(0, -.36f), .5f, .05f, 1.2f);
                case InkToken.Leaf: { Vector2 q = Rotate(p, -32); q.x += .1f * q.y * q.y; return Mathf.Min(Mathf.Max(Circle(q, new Vector2(-.42f, 0), .8f), Circle(q, new Vector2(.42f, 0), .8f)), Segment(q, new Vector2(0, -.95f), new Vector2(0, -.5f), .07f)); }
                case InkToken.Petal: { Vector2 q = p; q.x += .08f * Mathf.Sin(p.y * 2); return Uneven(q, new Vector2(0, -.72f), .1f, .46f, 1.12f); }
                case InkToken.Die: return Box(p, Vector2.zero, new Vector2(.7f, .7f), .22f, 10);
                case InkToken.Note: return Mathf.Min(Mathf.Min(Box(p, new Vector2(-.24f, -.5f), new Vector2(.32f, .24f), .22f, 22), Segment(p, new Vector2(.04f, -.42f), new Vector2(.04f, .78f), .08f)), Segment(p, new Vector2(.04f, .78f), new Vector2(.48f, .3f), .09f));
                case InkToken.Shard: return Polygon(p, ShardPoly) - .02f;
                case InkToken.Coin: case InkToken.Button: return Circle(p, Vector2.zero, .8f);
                case InkToken.Citrus: return Circle(p, Vector2.zero, .82f);
                case InkToken.Puff: return SmoothMin(SmoothMin(Circle(p, new Vector2(-.38f, -.12f), .42f), Circle(p, new Vector2(.1f, .18f), .5f), .2f), SmoothMin(Circle(p, new Vector2(.45f, -.16f), .36f), Circle(p, new Vector2(.02f, -.34f), .4f), .2f), .2f);
                case InkToken.Bubble: return Circle(p, Vector2.zero, .82f);
                case InkToken.Spear: return Polygon(p, SpearPoly) - .02f;
                case InkToken.Fang: { Vector2 q = p; q.x -= .3f * (p.y + .9f) * (p.y + .9f) * .25f; return Polygon(q, new[] { new Vector2(-.42f, .85f), new Vector2(.42f, .85f), new Vector2(.05f, -.9f) }) - .04f; }
                case InkToken.Thorn: { Vector2 q = p; q.x += .32f * (p.y + .9f) * (p.y + .9f) * .22f; return Polygon(q, new[] { new Vector2(-.34f, -.85f), new Vector2(.34f, -.85f), new Vector2(.02f, .92f) }) - .03f; }
                case InkToken.Seal: { float a = Mathf.Atan2(p.y, p.x); return p.magnitude - (.74f + .07f * Mathf.Cos(a * 9)); }
                case InkToken.Patch: return Box(p, Vector2.zero, new Vector2(.72f, .66f), .1f, -8);
                case InkToken.Page: return Mathf.Max(Box(p, Vector2.zero, new Vector2(.6f, .8f), .05f, 6), Vector2.Dot(Rotate(p, -6) - new Vector2(.6f, .8f), new Vector2(.7071f, .7071f)) + .2f);
                case InkToken.Bolt: return Polygon(p, BoltPoly) - .02f;
                case InkToken.Moon: return Mathf.Max(Circle(p, Vector2.zero, .82f), -Circle(p, new Vector2(.36f, .2f), .66f));
                case InkToken.Dumpling: return Mathf.Min(Box(p, new Vector2(0, -.12f), new Vector2(.84f, .5f), .45f), Circle(p, new Vector2(0, .3f), .3f));
                case InkToken.Knot: return Mathf.Min(Ring(p, Vector2.zero, .5f, .16f), Mathf.Min(Segment(p, new Vector2(-.9f, -.3f), new Vector2(.9f, .3f), .09f), Segment(p, new Vector2(-.3f, .9f), new Vector2(.3f, -.9f), .09f)));
                case InkToken.Sand: return Mathf.Min(Mathf.Min(Polygon(p, HourTop), Polygon(p, HourBottom)) - .04f, Mathf.Min(Box(p, new Vector2(0, .86f), new Vector2(.7f, .1f), .05f), Box(p, new Vector2(0, -.86f), new Vector2(.7f, .1f), .05f)));
                case InkToken.Envelope: return Box(p, Vector2.zero, new Vector2(.88f, .6f), .08f, -6);
            }
            return Circle(p, Vector2.zero, .8f);
        }
        /// <summary>Ink details drawn inside the silhouette (pips, veins, stitches). Negative inside the detail.</summary>
        private static float Detail(InkToken kind, Vector2 p, int face)
        {
            const float none = 9;
            switch (kind)
            {
                case InkToken.Leaf: { Vector2 q = Rotate(p, -32); q.x += .1f * q.y * q.y; return Segment(q, new Vector2(0, -.6f), new Vector2(0, .62f), .035f); }
                case InkToken.Petal: return Segment(p, new Vector2(0, -.5f), new Vector2(.04f, .3f), .03f);
                case InkToken.Die:
                    {
                        Vector2 q = Rotate(p, -10); float d = none; float o = .38f;
                        bool c = face % 2 == 1, diag = face >= 2, other = face >= 4, mid = face == 6;
                        if (c) d = Mathf.Min(d, Circle(q, Vector2.zero, .13f));
                        if (diag) { d = Mathf.Min(d, Circle(q, new Vector2(-o, o), .12f)); d = Mathf.Min(d, Circle(q, new Vector2(o, -o), .12f)); }
                        if (other) { d = Mathf.Min(d, Circle(q, new Vector2(o, o), .12f)); d = Mathf.Min(d, Circle(q, new Vector2(-o, -o), .12f)); }
                        if (mid) { d = Mathf.Min(d, Circle(q, new Vector2(-o, 0), .12f)); d = Mathf.Min(d, Circle(q, new Vector2(o, 0), .12f)); }
                        return d;
                    }
                case InkToken.Coin: return Mathf.Min(Ring(p, Vector2.zero, .56f, .035f), Polygon(p * 1.6f, Star5) / 1.6f + .02f);
                case InkToken.Button: return Mathf.Min(Ring(p, Vector2.zero, .58f, .04f), Mathf.Min(Mathf.Min(Circle(p, new Vector2(-.2f, .2f), .1f), Circle(p, new Vector2(.2f, .2f), .1f)), Mathf.Min(Circle(p, new Vector2(-.2f, -.2f), .1f), Circle(p, new Vector2(.2f, -.2f), .1f))));
                case InkToken.Citrus:
                    {
                        float d = Ring(p, Vector2.zero, .62f, .03f);
                        for (int i = 0; i < 6; i++) { float a = i * Mathf.PI / 3; d = Mathf.Min(d, Segment(p, Vector2.zero, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .58f, .03f)); }
                        return d;
                    }
                case InkToken.Seal: return Mathf.Min(Ring(p, Vector2.zero, .46f, .04f), Polygon(p * 2.2f, Star5) / 2.2f + .01f);
                case InkToken.Patch:
                    {
                        Vector2 q = Rotate(p, 8); float d = none;
                        for (int i = -3; i <= 3; i++) { d = Mathf.Min(d, Segment(q, new Vector2(i * .18f - .05f, .48f), new Vector2(i * .18f + .05f, .48f), .03f)); d = Mathf.Min(d, Segment(q, new Vector2(i * .18f - .05f, -.48f), new Vector2(i * .18f + .05f, -.48f), .03f)); }
                        return d;
                    }
                case InkToken.Page: { Vector2 q = Rotate(p, -6); float d = none; for (int i = 0; i < 4; i++) d = Mathf.Min(d, Segment(q, new Vector2(-.38f, .4f - i * .26f), new Vector2(i == 3 ? .05f : .3f, .4f - i * .26f), .03f)); return d; }
                case InkToken.Dumpling: { float d = none; for (int i = -2; i <= 2; i++) d = Mathf.Min(d, Segment(p, new Vector2(i * .18f, .28f), new Vector2(i * .28f, -.05f), .028f)); return d; }
                case InkToken.Envelope: { Vector2 q = Rotate(p, 6); return Mathf.Min(Segment(q, new Vector2(-.8f, .52f), new Vector2(0, -.08f), .035f), Segment(q, new Vector2(.8f, .52f), new Vector2(0, -.08f), .035f)); }
                case InkToken.Note: return none;
                case InkToken.Sand: return none;
            }
            return none;
        }
        /// <summary>Secondary painted fill region (wax seal, sand, rind). Negative inside.</summary>
        private static float Accent(InkToken kind, Vector2 p)
        {
            switch (kind)
            {
                case InkToken.Envelope: return Circle(p, new Vector2(.02f, -.1f), .2f);
                case InkToken.Sand: return Mathf.Min(Polygon(p, new[] { new Vector2(-.36f, .52f), new Vector2(.36f, .52f), new Vector2(.06f, .06f), new Vector2(-.06f, .06f) }), Polygon(p, new[] { new Vector2(-.5f, -.74f), new Vector2(0, -.36f), new Vector2(.5f, -.74f) }));
                case InkToken.Citrus: return Ring(p, Vector2.zero, .72f, .08f);
                case InkToken.Knot: return Circle(p, Vector2.zero, .2f);
            }
            return 9;
        }
        private static bool PaperFill(InkToken k) => k == InkToken.Envelope || k == InkToken.Page || k == InkToken.Die || k == InkToken.Puff || k == InkToken.Fang || k == InkToken.Dumpling || k == InkToken.Bubble;

        /// <summary>A painted token in this palette. face selects a die face (1-6).</summary>
        private static MechanicEffectArtwork painted; private static bool paintedLoaded;
        /// <summary>A hand-painted replacement for this token, when one has been imported (never for dice: faces matter).</summary>
        public static Sprite Painted(InkToken kind)
        {
            if (!paintedLoaded) { paintedLoaded = true; painted = MechanicEffectArtwork.Current; }
            return kind == InkToken.Die || painted == null ? null : painted.PaintedToken((int)kind);
        }
        public static Sprite Token(InkToken kind, InkPalette palette, int face = 0)
        {
            Sprite authored = Painted(kind); if (authored != null) return authored;
            long key = ((long)palette.Key << 12) | ((long)kind << 4) | (long)(face & 15);
            if (tokens.TryGetValue(key, out Sprite cached) && cached != null) return cached;
            const int n = TokenSize; var texture = new Texture2D(n, n, TextureFormat.RGBA32, true) { name = "MechanicInk token " + kind, wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear };
            var px = new Color[n * n]; float aa = 2.4f / n, inkWidth = .085f; Vector2 light = new Vector2(-.55f, .83f) * .17f;
            int seed = (int)kind * 17 + face;
            Color fill = PaperFill(kind) ? palette.Light : palette.Body, shade = Color.Lerp(fill, palette.Ink, PaperFill(kind) ? .32f : .42f), lit = Color.Lerp(palette.Light, Color.white, .25f);
            if (kind == InkToken.Bubble) shade = Color.Lerp(palette.Body, palette.Ink, .2f);
            for (int y = 0; y < n; y++) for (int x = 0; x < n; x++)
            {
                Vector2 p = new Vector2((x + .5f) / n * 2 - 1, (y + .5f) / n * 2 - 1) * 1.12f;
                Vector2 wob = p + new Vector2(Noise(p.x * 2.2f + seed, p.y * 2.2f) - .5f, Noise(p.x * 2.2f, p.y * 2.2f + seed + 5) - .5f) * .07f;
                float d = Shape(kind, wob, face);
                float alpha = Mathf.Clamp01(.5f - d / aa);
                if (alpha <= 0) { px[y * n + x] = new Color(palette.Ink.r, palette.Ink.g, palette.Ink.b, 0); continue; }
                float depth = -d;
                float rim = Step(-.03f, .07f, Shape(kind, wob + light, face));
                float under = Step(-.03f, .09f, Shape(kind, wob - light, face));
                Color c = fill;
                float grain = .9f + .16f * Fbm(p.x * 3 + seed, p.y * 3, 0);
                float volume = Mathf.Clamp01(.5f - Vector2.Dot(p, new Vector2(-.55f, .83f)) * .45f);
                c = Color.Lerp(c, shade, volume * .45f);
                c = Color.Lerp(c, shade, under * .85f);
                c = Color.Lerp(c, lit, rim * .8f);
                float ac = Accent(kind, wob);
                if (ac < 0) { Color ap = kind == InkToken.Citrus ? Color.Lerp(palette.Light, palette.Body, .3f) : palette.Accent; c = Color.Lerp(c, Color.Lerp(ap, lit, rim * .5f), Mathf.Clamp01(.5f - ac / aa)); }
                c *= grain;
                if (kind == InkToken.Bubble) { c = Color.Lerp(palette.Body, palette.Glow, Mathf.Clamp01((p.x + p.y) * .5f + .5f)); alpha *= Mathf.Lerp(.16f, 1, Step(.06f, -.03f, d + .16f)); c = Color.Lerp(c, lit, Step(.6f, .9f, Mathf.Clamp01(1 - (p - new Vector2(-.32f, .34f)).magnitude * 3))); }
                float detail = Detail(kind, wob, face);
                c = Color.Lerp(c, palette.Ink, Mathf.Clamp01(.5f - detail / aa) * .9f);
                float ink = 1 - Step(inkWidth - aa, inkWidth + aa, depth);
                c = Color.Lerp(c, palette.Ink, ink);
                c.a = alpha * (kind == InkToken.Bubble ? Mathf.Lerp(1, .95f, ink) : 1);
                px[y * n + x] = c;
            }
            texture.SetPixels(px); texture.Apply(true, true);
            var sprite = Sprite.Create(texture, new Rect(0, 0, n, n), new Vector2(.5f, .5f), n, 0, SpriteMeshType.FullRect);
            sprite.name = texture.name; tokens[key] = sprite; return sprite;
        }
        public static bool IsInk(Sprite sprite) => sprite != null && sprite.texture != null && sprite.texture.name.StartsWith("MechanicInk", StringComparison.Ordinal);
    }
}
