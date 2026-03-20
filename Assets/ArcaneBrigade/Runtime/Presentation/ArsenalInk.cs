using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Bizarre Arsenal's effects, painted in the same storybook ink as the hero mechanics (InkCanvas washes and
    /// brush ribbons, MechanicInk tokens): bursts, clouds, swipes, stamps, pools, cords, wakes. The objects themselves (bombs,
    /// balls, signs, the held weapons) stay painted art; only the moments around them are drawn here. No moment is a lone
    /// plain shape: each layers a wash, a broken inked rim or brush strokes, and flying tokens, and plays action then settle.</summary>
    public sealed class ArsenalInk
    {
        public enum Moment
        {
            Burst, CottonPop, PopperBurst, CandyFloss, PaperScraps, BalloonPop, Confetti, MuzzleCloud, CorkPop, Droplets, Steam,
            LeekSwipe, PawSwipe, GoldLeaf, Dragon, MetalSwipe, FistJab, SwatPrint, Crumbs, BrickDust, GavelStamp, IngotBlast,
            KnucklePop, MittCatch, Zap, Spark, HomeRun, Glitch, Snip, FirecrackerBlast, LanternBurst, SnowBurst, AcornPop,
            Splat, Feathers, Sprinkle, SnowPuff, PinClack, Scald, Spout, FrostGust
        }

        private static InkPalette P(Color ink, Color body, Color light, Color glow, Color accent) => new InkPalette(ink, body, light, glow, accent);
        public static readonly InkPalette
            Cream = P(new Color(.16f, .12f, .2f), new Color(.97f, .92f, .8f), new Color(1, .98f, .92f), new Color(1, .9f, .65f), new Color(.85f, .68f, .35f)),
            Cotton = P(new Color(.2f, .17f, .24f), new Color(.98f, .97f, .94f), new Color(1, 1, 1), new Color(1, .95f, .8f), new Color(.9f, .75f, .4f)),
            Gold = P(new Color(.24f, .15f, .08f), new Color(.95f, .74f, .3f), new Color(1, .95f, .7f), new Color(1, .85f, .45f), new Color(.75f, .5f, .2f)),
            Amber = P(new Color(.25f, .12f, .1f), new Color(.98f, .66f, .3f), new Color(1, .93f, .7f), new Color(1, .78f, .42f), new Color(.55f, .18f, .26f)),
            Wine = P(new Color(.18f, .06f, .12f), new Color(.55f, .16f, .3f), new Color(.98f, .8f, .82f), new Color(.95f, .6f, .7f), new Color(.95f, .74f, .3f)),
            Water = P(new Color(.07f, .18f, .24f), new Color(.45f, .78f, .85f), new Color(.92f, 1, 1), new Color(.6f, .95f, 1), new Color(.98f, .95f, .75f)),
            Green = P(new Color(.1f, .2f, .1f), new Color(.55f, .82f, .42f), new Color(.94f, 1, .85f), new Color(.75f, 1, .6f), new Color(.98f, .95f, .7f)),
            Violet = P(new Color(.08f, .05f, .14f), new Color(.32f, .2f, .45f), new Color(.75f, .65f, .95f), new Color(.6f, .45f, .9f), new Color(.9f, .6f, .85f)),
            Terracotta = P(new Color(.22f, .11f, .08f), new Color(.78f, .45f, .32f), new Color(.98f, .85f, .7f), new Color(.95f, .7f, .5f), new Color(.55f, .4f, .3f)),
            Leather = P(new Color(.2f, .11f, .06f), new Color(.62f, .4f, .22f), new Color(.96f, .85f, .65f), new Color(.95f, .75f, .5f), new Color(.9f, .9f, .85f)),
            Candy = P(new Color(.3f, .12f, .22f), new Color(.98f, .72f, .85f), new Color(1, .95f, .98f), new Color(1, .8f, .92f), new Color(.7f, .85f, 1)),
            Cyan = P(new Color(.06f, .16f, .26f), new Color(.42f, .86f, .98f), new Color(.95f, 1, 1), new Color(.55f, .95f, 1), new Color(.98f, .88f, .45f)),
            Silver = P(new Color(.14f, .14f, .2f), new Color(.8f, .84f, .92f), new Color(1, 1, 1), new Color(.85f, .9f, 1), new Color(.95f, .8f, .45f)),
            Midnight = P(new Color(.05f, .05f, .1f), new Color(.16f, .18f, .32f), new Color(.7f, .75f, .95f), new Color(.45f, .5f, .85f), new Color(.95f, .8f, .45f)),
            Dust = P(new Color(.24f, .14f, .1f), new Color(.86f, .7f, .56f), new Color(1, .94f, .84f), new Color(.95f, .8f, .62f), new Color(.7f, .45f, .32f)),
            Glass = P(new Color(.1f, .2f, .26f), new Color(.82f, .95f, .98f), new Color(1, 1, 1), new Color(.85f, 1, 1), new Color(.6f, .9f, .7f));
        private static readonly InkPalette[] Party = { Gold, Wine, Water, Candy, Green, Midnight };

        private readonly int GroundToken, AirToken, GlowOrder;
        private readonly Transform root;
        private readonly InkCanvas ground, air;
        private readonly List<SpriteRenderer> tokens = new List<SpriteRenderer>();
        private int used;
        private bool still, sparse, calm;
        public int Primitives => ground.Primitives + air.Primitives + used;

        /// <summary>groundOrder / airOrder: sorting orders of the floor and air layers (tokens sit just above each, glows just
        /// below the air layer). The defaults paint over the battlefield; a staged moment can paint behind or in front of a unit.</summary>
        public ArsenalInk(Transform parent, int groundOrder = 66, int airOrder = 2140)
        {
            root = parent;
            GroundToken = groundOrder + 3; AirToken = airOrder + 6; GlowOrder = airOrder - 2;
            ground = new InkCanvas(parent, "Arsenal ground ink", groundOrder);
            air = new InkCanvas(parent, "Arsenal air ink", airOrder);
        }
        public void Begin()
        {
            still = PresentationSettings.ReducedMotion; sparse = PresentationSettings.ReducedEffects || AnimationDetailBudget.Level >= 2; calm = PresentationSettings.ReducedFlashes;
            ground.Clear(); air.Clear(); used = 0;
        }
        public void End()
        {
            ground.Commit(); air.Commit();
            for (int i = used; i < tokens.Count; i++) tokens[i].enabled = false;
        }
        public void Destroy() { foreach (SpriteRenderer r in tokens) if (r != null) Object.Destroy(r.gameObject); }

        // ------------------------------------------------------------------ helpers
        private static float OutCubic(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static float InQuad(float x) { x = Mathf.Clamp01(x); return x * x; }
        private static float OutBack(float x) { x = Mathf.Clamp01(x); const float c = 1.6f; float y = x - 1; return 1 + (c + 1) * y * y * y + c * y * y; }
        private static float Smooth(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));
        private static Vector2 Dir(float degrees) => new Vector2(Mathf.Cos(degrees * Mathf.Deg2Rad), Mathf.Sin(degrees * Mathf.Deg2Rad));
        private static Color A(Color c, float alpha) { c.a = Mathf.Clamp01(alpha); return c; }
        private static float Angle(Vector2 v) => Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;

        /// <summary>A sprite placed in world space: a token, a glow, or a held weapon reused in a moment.</summary>
        public void Place(Sprite sprite, Vector2 at, float size, float angle, Color tint, bool airborne, float squash = 1)
        {
            if (sprite == null || tint.a <= .004f) return;
            if (used == tokens.Count)
            {
                var go = new GameObject("Arsenal ink token"); go.transform.SetParent(root, false);
                tokens.Add(go.AddComponent<SpriteRenderer>());
            }
            SpriteRenderer r = tokens[used++]; r.enabled = true; r.sprite = sprite; r.color = tint; r.sortingOrder = airborne ? AirToken : GroundToken; r.flipX = false;
            float unit = Mathf.Max(.01f, Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y));
            r.transform.position = at; r.transform.rotation = Quaternion.Euler(0, 0, still ? 0 : angle);
            r.transform.localScale = new Vector3(size / squash / unit, size * squash / unit, 1);
        }
        public void Token(InkToken kind, InkPalette palette, Vector2 at, float size, float angle, float alpha, bool airborne = true)
            => Place(MechanicInk.Token(kind, palette), at, size, angle, new Color(1, 1, 1, Mathf.Clamp01(alpha)), airborne);
        private void Glow(Vector2 at, float diameter, Color color, float alpha)
        {
            if (calm) alpha *= .45f;
            if (alpha <= .004f) return;
            if (used == tokens.Count) { var go = new GameObject("Arsenal ink glow"); go.transform.SetParent(root, false); tokens.Add(go.AddComponent<SpriteRenderer>()); }
            SpriteRenderer r = tokens[used++]; r.enabled = true; r.sprite = MechanicInk.Glow; r.color = A(color, alpha); r.sortingOrder = GlowOrder;
            float unit = Mathf.Max(.01f, r.sprite.bounds.size.x);
            r.transform.position = at; r.transform.rotation = Quaternion.identity; r.transform.localScale = Vector3.one * diameter / unit;
        }
        private static InkStroke Brush(InkPalette pal, float width, float alpha, float head = 1, float tail = 0, float taper = 1.2f, bool noLight = false)
            => new InkStroke { Width = width, Head = head, Tail = tail, Taper = taper, Alpha = Mathf.Clamp01(alpha), Ink = pal.Ink, Body = pal.Body, Light = pal.Light, LightOffset = .13f, NoLight = noLight };

        private readonly Vector2[] lobeAt = new Vector2[8];
        private readonly float[] lobeR = new float[8];
        /// <summary>A painted cloud: lobes merged under one inked silhouette, a shaded underside, a lit top and a soft highlight.
        /// It swells, drifts up and dissolves by shrinking (never a stack of see-through discs).</summary>
        private void Cloud(Vector2 c, float r, float p, InkPalette pal, float seed, float rise = .25f, int lobes = 5, float alpha = 1, bool low = false)
        {
            InkCanvas layer = low ? ground : air;
            if (sparse) lobes = Mathf.Max(3, lobes - 2);
            lobes = Mathf.Min(lobes, lobeAt.Length);
            float swell = still ? .85f : OutCubic(Mathf.Min(1, p / .4f)), shrink = 1 - .45f * Smooth(.45f, 1, p), a = Mathf.Max(alpha, .92f) * (1 - Smooth(.78f, 1, p)); // layered passes must stay nearly opaque or the inked silhouette shows through
            if (a <= .01f) return;
            for (int i = 0; i < lobes; i++)
            {
                float angle = seed + i * 360f / lobes + (i % 2) * 23;
                Vector2 spread = low ? new Vector2(1.3f, .5f) : new Vector2(1, .8f);
                lobeAt[i] = c + Vector2.Scale(Dir(angle) * (r * .45f * swell), spread) + Vector2.up * (still ? 0 : rise * p * (1 + .3f * (i % 3)));
                lobeR[i] = r * (.5f + .12f * ((i * 7 + Mathf.RoundToInt(seed)) % 3)) * (.5f + .5f * swell) * shrink;
            }
            Color shade = Color.Lerp(pal.Body, pal.Ink, .3f), lit = Color.Lerp(pal.Body, pal.Light, .35f);
            float ink = Mathf.Clamp(r * .09f, .035f, .07f);
            for (int i = 0; i < lobes; i++) layer.Sector(lobeAt[i], lobeR[i] + ink, 0, 360, A(pal.Ink, .9f * a), A(pal.Ink, .9f * a));
            for (int i = 0; i < lobes; i++) layer.Sector(lobeAt[i], lobeR[i], 0, 360, A(shade, a), A(shade, a));
            for (int i = 0; i < lobes; i++) layer.Sector(lobeAt[i] + Vector2.up * lobeR[i] * .17f, lobeR[i] * .8f, 0, 360, A(lit, a), A(pal.Body, a));
            for (int i = 0; i < lobes; i += 2) layer.Sector(lobeAt[i] + new Vector2(-.3f, .36f) * lobeR[i], lobeR[i] * .32f, 0, 360, A(pal.Light, .85f * a), A(pal.Light, 0));
        }
        /// <summary>Comic impact strokes: short tapered dashes thrown outward from an impact point.</summary>
        private void Rays(Vector2 c, float r, float p, InkPalette pal, int n, float seed, float width = .05f, float length = .32f, float spread = 360, float facing = 0)
        {
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .25f)), tail = still ? 0 : InQuad(Mathf.Clamp01((p - .2f) / .8f));
            if (sparse) n = Mathf.Max(3, n / 2);
            for (int i = 0; i < n; i++)
            {
                float a = spread >= 360 ? seed + i * 360f / n + (i * 29) % 17 : facing - spread * .5f + spread * (n == 1 ? .5f : i / (float)(n - 1));
                Vector2 d = Dir(a); float reach = r + (still ? 0 : .12f * OutCubic(p));
                air.Stroke(InkCanvas.Line(c + d * reach, c + d * (reach + length * (.7f + .3f * (i % 2)))), Brush(pal, width * 1.5f, 1, head, tail, 1.6f));
            }
        }
        /// <summary>Tokens thrown out from c: they ease out, fall a little and spin; alternate tokens may use a second look.</summary>
        private void Scatter(Vector2 c, float reach, float p, InkToken kind, InkPalette pal, int n, float size, float seed, float gravity = .5f, InkPalette? alt = null, InkToken? altKind = null, float spread = 360, float facing = 0)
        {
            if (sparse) n = Mathf.Max(2, n / 2);
            for (int i = 0; i < n; i++)
            {
                float a = spread >= 360 ? seed + i * 360f / n + (i * 37) % 23 - 11 : facing - spread * .5f + spread * ((i + .5f) / n) + (i * 13) % 9 - 4;
                float d = reach * (.55f + .45f * ((i * 13) % 5) / 4f) * (still ? .7f : OutCubic(p));
                Vector2 at = c + Dir(a) * d + (still ? Vector2.zero : Vector2.down * gravity * p * p);
                bool second = alt.HasValue && (i % 2 == 1);
                Token(second && altKind.HasValue ? altKind.Value : kind, second ? alt.Value : pal, at, size * 1.7f * (1 - .35f * p), a * 2 + p * 260 * ((i % 2) * 2 - 1), 1 - Smooth(.6f, 1, p));
            }
        }
        /// <summary>Brush strokes sweeping an arc in front of a wielder: the head races ahead, the tail follows.</summary>
        private void Sweep(Vector2 c, Vector2 dir, float r, float p, InkPalette pal, float width, float half, float offset = 0, float squash = 1, float delay = 0)
        {
            float q = Mathf.Clamp01((p - delay) / Mathf.Max(.05f, 1 - delay)); if (p < delay) return;
            float a = Angle(dir) + offset;
            float head = still ? 1 : OutCubic(Mathf.Min(1, q / .4f)), tail = still ? 0 : InQuad(Mathf.Clamp01((q - .25f) / .75f));
            var path = InkCanvas.Arc(c, r, r * squash, a + half, a - half);
            air.GlowRibbon(path, head, tail, width * 2.2f, A(pal.Glow, (calm ? .12f : .26f) * (1 - q)));
            air.Stroke(path, Brush(pal, width, 1 - Smooth(.7f, 1, q), head, tail, 1.3f));
        }

        // ------------------------------------------------------------------ moments (bursts, swipes, stamps)
        /// <summary>One timed moment at `at`, facing `dir`, p = 0..1 through its life, `size` about its reach in world units.</summary>
        public void Draw(Moment m, Vector2 at, Vector2 dir, float p, float size, float seed, Sprite held = null)
        {
            dir = dir.sqrMagnitude > .0001f ? dir.normalized : Vector2.right;
            Vector2 side = new Vector2(-dir.y, dir.x);
            float face = Angle(dir), fade = 1 - Smooth(.55f, 1, p);
            switch (m)
            {
                case Moment.CottonPop:
                    Glow(at, size * 2.4f, Cotton.Glow, .32f * (1 - p));
                    Cloud(at, size * .5f, p, Cotton, seed, .3f, 6);
                    Scatter(at, size * 1.15f, p, InkToken.Shard, Gold, 8, .15f, seed, .55f, Midnight, InkToken.Star);
                    break;
                case Moment.PopperBurst:
                    Glow(at, size * 2f, Amber.Glow, .3f * (1 - p));
                    Cloud(at, size * .42f, p, Amber, seed, .2f, 4, .75f);
                    for (int i = 0; i < (sparse ? 2 : 4); i++)
                    {
                        // curling paper streamers thrown out of the pop
                        float a = seed + i * 90 + 25; float head = still ? 1 : OutCubic(Mathf.Min(1, p / .35f)), tail = still ? 0 : InQuad(Mathf.Clamp01((p - .3f) / .7f));
                        var path = InkCanvas.Line(at + Dir(a) * size * .25f, at + Dir(a) * size * 1.15f + Vector2.down * .25f * p, .07f, 3.2f, false, seed + i);
                        air.Stroke(path, Brush(i % 2 == 0 ? Wine : Gold, .06f, 1, head, tail, 1));
                    }
                    Scatter(at, size, p, InkToken.Spark, Gold, 6, .14f, seed + 11, .4f);
                    break;
                case Moment.CandyFloss:
                {
                    // a candy-floss mushroom: a puff on the floor, a cap rising over it, little stars raining out
                    float rise = still ? .6f : OutCubic(Mathf.Min(1, p / .5f));
                    Glow(at + Vector2.up * size * .4f, size * 2.4f, Candy.Glow, .3f * (1 - p));
                    Cloud(at, size * .4f, p, Candy, seed, .05f, 4, .7f, true);
                    Cloud(at + Vector2.up * size * (.25f + .45f * rise), size * .62f, p, Candy, seed + 50, .1f, 6);
                    if (p > .3f) Scatter(at + Vector2.up * size * .7f, size * .9f, (p - .3f) / .7f, InkToken.Star, Water, 7, .13f, seed, 1.1f, Gold);
                    break;
                }
                case Moment.PaperScraps:
                    Glow(at, size * 1.8f, Gold.Glow, .26f * (1 - p));
                    Rays(at, size * .55f, p, Cream, 7, seed, .05f, size * .35f);
                    Scatter(at, size * 1.25f, p, InkToken.Petal, Wine, 10, .16f, seed, .7f, Gold, InkToken.Page);
                    break;
                case Moment.BalloonPop:
                {
                    Glow(at, size * 1.6f, Candy.Glow, .24f * (1 - p));
                    for (int i = 0; i < 2; i++)
                    {
                        // two ribbons twirling down from the pop
                        float x = (i * 2 - 1) * size * .22f; float drop = still ? .3f : .2f + .6f * OutCubic(p);
                        var path = InkCanvas.Line(at + new Vector2(x, size * .1f), at + new Vector2(x * 1.6f, -size * drop), .06f, 4, false, seed + i * 3);
                        air.Stroke(path, Brush(i == 0 ? Candy : Water, .045f, fade, 1, still ? 0 : InQuad(Mathf.Clamp01((p - .4f) / .6f)), 1));
                    }
                    Scatter(at, size * .9f, p, InkToken.Petal, Candy, 5, .14f, seed, .8f, Water);
                    break;
                }
                case Moment.Confetti:
                {
                    Glow(at, size * 1.6f, Gold.Glow, .2f * (1 - p));
                    int n = sparse ? 6 : 12;
                    for (int i = 0; i < n; i++)
                    {
                        float a = face - 55 + 110 * ((i + .5f) / n) + (i * 17) % 11 - 5, d = size * (.5f + .5f * ((i * 7) % 4) / 3f) * (still ? .7f : OutCubic(p));
                        Vector2 o = at + Dir(a) * d + (still ? Vector2.zero : Vector2.down * .5f * p * p);
                        Token(i % 3 == 0 ? InkToken.Star : i % 3 == 1 ? InkToken.Petal : InkToken.Shard, Party[i % Party.Length], o, .22f * (1 - .3f * p), a * 3 + p * 300 * ((i % 2) * 2 - 1), fade);
                    }
                    break;
                }
                case Moment.MuzzleCloud:
                    Cloud(at + dir * size * .3f + dir * size * .25f * p, size * .35f, p, Cream, seed, .12f, 4, .7f);
                    Rays(at + dir * size * .15f, size * .2f, p, Cream, 3, 0, .045f, size * .3f, 40, face);
                    break;
                case Moment.CorkPop:
                    Cloud(at + dir * .15f, size * .28f, p, Cream, seed, .1f, 4, .7f);
                    Rays(at + dir * .1f, .18f, p, Leather, 4, 0, .04f, .22f, 70, face);
                    Token(InkToken.Puff, Cream, at + dir * (.2f + .25f * p), .2f * (1 - .4f * p), seed, fade);
                    break;
                case Moment.Droplets:
                    Scatter(at + dir * .2f, size * .8f, p, InkToken.Drop, Water, 7, .12f, seed, .35f, Glass, InkToken.Star, 60, face);
                    break;
                case Moment.Steam:
                    Cloud(at, size * .24f, p, Cream, seed, .4f, 3, .55f);
                    break;
                case Moment.LeekSwipe:
                    Sweep(at, dir, size * .85f, p, Green, .2f, 72);
                    Sweep(at, dir, size * .62f, p, Green, .09f, 60, 0, 1, .08f);
                    Scatter(at + dir * size * .6f, size * .6f, p, InkToken.Leaf, Green, 3, .16f, seed, .25f, null, null, 90, face);
                    break;
                case Moment.PawSwipe:
                    Glow(at + dir * size * .55f, size * .9f, Silver.Glow, .22f * (1 - p));
                    Token(InkToken.Glint, Cream, at + dir * size * .8f + side * size * .1f, .16f * (1 - .5f * p), p * 90, fade);
                    for (int k = -1; k <= 1; k++)
                    {
                        // three curved claw rakes just in front of the paw, one after another
                        float delay = (k + 1) * .06f; float q = Mathf.Clamp01((p - delay) / (1 - delay)); if (p < delay) continue;
                        Vector2 centre = at - dir * size * .15f + side * (k * .17f);
                        var path = InkCanvas.Arc(centre, size * .72f, size * .72f, face + 48, face - 40);
                        air.GlowRibbon(path, still ? 1 : OutCubic(Mathf.Min(1, q / .3f)), still ? 0 : InQuad(Mathf.Clamp01((q - .35f) / .65f)), .26f, A(Silver.Glow, .22f * (1 - q)));
                        air.Stroke(path, Brush(Cream, .12f, 1 - Smooth(.65f, 1, q), still ? 1 : OutCubic(Mathf.Min(1, q / .3f)), still ? 0 : InQuad(Mathf.Clamp01((q - .35f) / .65f)), 1.8f));
                    }
                    break;
                case Moment.GoldLeaf:
                    Sweep(at, dir, size * .9f, p, Gold, .3f, 80);
                    Sweep(at, dir, size * .72f, p, Cream, .1f, 70, 0, 1, .06f);
                    for (int i = 0; i < (sparse ? 2 : 5); i++)
                    {
                        float a = face + 70 - 140 * (i / 4f), q = Mathf.Clamp01(p * 1.4f - i * .08f);
                        Token(InkToken.Star, Gold, at + Dir(a) * size * (.9f + .2f * q), .14f * (1 - .5f * q), a + q * 200, (1 - q) * fade);
                    }
                    break;
                case Moment.Dragon:
                    Sweep(at, dir, size * .8f, p, Amber, .2f, 62, 0, .9f);
                    Sweep(at, dir, size * .95f, p, Gold, .07f, 50, 6, .9f, .1f);
                    Scatter(at + dir * size * .7f, size * .55f, p, InkToken.Ember, Amber, 4, .14f, seed, -.2f, Gold, null, 100, face);
                    break;
                case Moment.FirecrackerBlast:
                    // a big firecracker bang: an amber cloud, paper casing torn into streamers, sparks flying wide
                    Glow(at, size * 2.4f, Amber.Glow, .34f * (1 - p));
                    Cloud(at, size * .5f, p, Amber, seed, .22f, 6);
                    Rays(at, size * .55f, p, Cream, 8, seed, .05f, size * .4f);
                    Scatter(at, size * 1.2f, p, InkToken.Page, Wine, 8, .15f, seed, .7f, Gold, InkToken.Spark);
                    break;
                case Moment.LanternBurst:
                    // the paper lantern bursts: a warm gold bloom, lantern paper and embers raining down
                    Glow(at, size * 2.8f, Gold.Glow, .38f * (1 - p));
                    Cloud(at + Vector2.up * size * .1f, size * .55f, p, Gold, seed, .3f, 6);
                    Scatter(at, size * 1.3f, p, InkToken.Ember, Amber, 9, .15f, seed, .9f, Wine, InkToken.Page);
                    break;
                case Moment.SnowBurst:
                    // the snow globe shatters: glass shards, a frosty puff and snowflake stars
                    Glow(at, size * 2f, Water.Glow, .3f * (1 - p));
                    Cloud(at, size * .45f, p, Glass, seed, .12f, 5, 1, true);
                    Scatter(at, size * 1.1f, p, InkToken.Shard, Glass, 7, .14f, seed, .35f, Water, InkToken.Star);
                    Rays(at, size * .45f, p, Glass, 6, seed, .04f, size * .3f);
                    break;
                case Moment.Splat:
                    // a scoop lands: a low creamy splash, drips and a bubble or two flung out
                    Glow(at, size * 1.6f, Candy.Glow, .22f * (1 - p));
                    Cloud(at, size * .42f, p, Candy, seed, .05f, 5, 1, true);
                    Scatter(at, size * .95f, p, InkToken.Drop, Candy, 7, .13f, seed, .45f, Cream, InkToken.Bubble);
                    break;
                case Moment.Feathers:
                    // the cuckoo strikes: a puff of dust, comic strike lines and feathers drifting down
                    Cloud(at, size * .32f, p, Dust, seed, .1f, 4, 1, true);
                    Rays(at, size * .4f, p, Leather, 5, seed, .045f, size * .3f);
                    Scatter(at, size * 1.1f, p, InkToken.Leaf, Leather, 6, .15f, seed, .2f, Cream, InkToken.Petal);
                    break;
                case Moment.Sprinkle:
                    // a watering can's arc: a brush sweep of water ahead and drops raining down along it
                    Sweep(at, dir, size * .75f, p, Water, .08f, 38);
                    Scatter(at + dir * size * .45f, size * .8f, p, InkToken.Drop, Water, 8, .11f, seed, .6f, Glass, InkToken.Bubble, 80, face);
                    break;
                case Moment.SnowPuff:
                    // a clump of snow bursts: a soft powdery cloud on the floor and flakes thrown out
                    Glow(at, size * 1.8f, Water.Glow, .2f * (1 - p));
                    Cloud(at, size * .5f, p, Cotton, seed, .12f, 6, 1, true);
                    Scatter(at, size, p, InkToken.Star, Water, 6, .13f, seed, .4f, Cotton, InkToken.Puff);
                    break;
                case Moment.Scald:
                    // the teapot bursts: a billow of steam, hot droplets and strike lines
                    Glow(at, size * 2f, Amber.Glow, .25f * (1 - p));
                    Cloud(at + Vector2.up * size * .15f, size * .55f, p, Cream, seed, .5f, 6);
                    Scatter(at, size * 1.1f, p, InkToken.Drop, Amber, 7, .12f, seed, .3f, Water, InkToken.Puff);
                    Rays(at, size * .45f, p, Amber, 6, seed, .04f, size * .3f);
                    break;
                case Moment.Spout:
                {
                    // a column of water rises on the spot and falls back as drops round it
                    float rise = still ? .7f : OutCubic(Mathf.Min(1, p / .35f));
                    Glow(at + Vector2.up * size * .6f, size * 1.6f, Water.Glow, .25f * (1 - p));
                    air.Stroke(InkCanvas.Line(at, at + Vector2.up * size * 1.4f * rise, .07f, 3, false, seed), Brush(Water, size * .35f, fade, 1, still ? 0 : InQuad(Mathf.Clamp01((p - .45f) / .55f)), .8f));
                    Cloud(at + Vector2.up * size * 1.3f * rise, size * .35f, p, Water, seed, .1f, 4, 1);
                    if (p > .25f) Scatter(at + Vector2.up * size * 1.1f, size * .9f, (p - .25f) / .75f, InkToken.Drop, Water, 8, .12f, seed, 1.4f, Glass, InkToken.Bubble);
                    break;
                }
                case Moment.FrostGust:
                    // a cold whistle ahead: two frosted brush sweeps and snowflakes blown along them
                    Sweep(at, dir, size * .55f, p, Glass, .09f, 28);
                    Sweep(at, dir, size * .85f, p, Water, .06f, 22, 0, 1, .1f);
                    Scatter(at + dir * size * .5f, size * .7f, p, InkToken.Star, Glass, 7, .12f, seed, .1f, Water, InkToken.Shard, 50, face);
                    break;
                case Moment.PinClack:
                    // a juggling pin lands: a clack of strike lines and a few circus stars
                    Rays(at, size * .3f, p, Gold, 5, seed, .04f, size * .28f);
                    Scatter(at, size * .8f, p, InkToken.Star, Gold, 4, .12f, seed, .3f, Wine, InkToken.Spark);
                    break;
                case Moment.AcornPop:
                    // an acorn pops: a little cloud of husk dust, its cap and chips flying
                    Cloud(at, size * .4f, p, Dust, seed, .1f, 4, 1, true);
                    Scatter(at, size * .9f, p, InkToken.Sand, Leather, 5, .12f, seed, .5f, Green, InkToken.Leaf);
                    Rays(at, size * .35f, p, Leather, 4, seed, .04f, size * .25f);
                    break;
                case Moment.Snip:
                {
                    // two blades closing on a line ahead: their strokes swing in to cross, a glint where they meet
                    float close = still ? 1 : OutCubic(Mathf.Min(1, p / .35f)), spread = Mathf.Lerp(24, 3, close);
                    Vector2 tip = at + dir * size;
                    for (int k = -1; k <= 1; k += 2)
                    {
                        Vector2 blade = (Vector2)(Quaternion.Euler(0, 0, k * spread) * dir);
                        air.Stroke(InkCanvas.Line(at + dir * .15f, at + dir * .15f + blade * size), Brush(Silver, .09f, fade, 1, still ? 0 : InQuad(Mathf.Clamp01((p - .4f) / .6f)), 1.4f));
                    }
                    Token(InkToken.Glint, Cream, tip, .26f * (still ? 1 : OutBack(Mathf.Min(1, p / .35f))) * (1 - .4f * p), p * 90, fade);
                    Scatter(tip, size * .45f, p, InkToken.Leaf, Green, 3, .12f, seed, .3f, null, null, 120, face);
                    break;
                }
                case Moment.MetalSwipe:
                    Sweep(at, dir, size * .78f, p, Silver, .15f, 58);
                    Rays(at + dir * size * .78f, .1f, p, Cream, 3, 0, .04f, .2f, 70, face);
                    break;
                case Moment.FistJab:
                {
                    Vector2 hit = at + dir * size;
                    Glow(hit, .9f, Cream.Glow, .25f * (1 - p));
                    Rays(hit, .14f, p, Silver, 6, seed, .05f, .24f);
                    Token(InkToken.Star, Cream, hit, .3f * (still ? 1 : OutBack(Mathf.Min(1, p / .3f))) * (1 - .4f * p), seed, fade);
                    break;
                }
                case Moment.SwatPrint:
                {
                    // a flat star-mesh print pressed into the floor, a little dust puffing out of its edges
                    float press = still ? 1 : OutBack(Mathf.Min(1, p / .2f)), h = size * .42f * press, life = 1 - Smooth(.5f, 1, p);
                    ground.Sector(at, h * 1.15f, 0, 360, A(Wine.Body, .12f * life), A(Wine.Body, .38f * life));
                    for (int k = -2; k <= 2; k++)
                    {
                        float o = k * h * .4f;
                        ground.Stroke(InkCanvas.Line(at + new Vector2(-h, o), at + new Vector2(h, o)), Brush(Wine, .035f, .8f * life, 1, 0, .4f, true));
                        ground.Stroke(InkCanvas.Line(at + new Vector2(o, -h), at + new Vector2(o, h)), Brush(Wine, .035f, .8f * life, 1, 0, .4f, true));
                    }
                    Scatter(at, size * .7f, p, InkToken.Puff, Cream, 4, .12f, seed, -.1f);
                    break;
                }
                case Moment.Crumbs:
                    Scatter(at + dir * size * .7f, size * .6f, p, InkToken.Sand, Gold, 7, .12f, seed, .6f, Amber, null, 110, face);
                    break;
                case Moment.BrickDust:
                    ground.Sector(at, size * .9f, 0, 360, A(Terracotta.Ink, .05f * fade), A(Terracotta.Ink, .22f * fade));
                    Cloud(at, size * .4f, p, Dust, seed, .12f, 5, 1, true);
                    Rays(at, size * .55f, p, Terracotta, 5, seed, .045f, size * .3f);
                    Scatter(at, size * 1.05f, p, InkToken.Shard, Terracotta, 6, .11f, seed, .45f, Dust, InkToken.Sand);
                    break;
                case Moment.GavelStamp:
                {
                    // the wielder's own gavel, grown large, comes down on the spot; the verdict rings out in gold and coins jump
                    float fall = Mathf.Clamp01(p / .22f), after = Mathf.Clamp01((p - .22f) / .78f);
                    if (held != null)
                    {
                        float angle = still ? 0 : p < .22f ? Mathf.Lerp(55, -8, InQuad(fall)) : -8 + 6 * Mathf.Sin(after * Mathf.PI) * (1 - after);
                        float lift = p < .22f ? (1 - InQuad(fall)) * .8f : after > .55f ? (after - .55f) * 1.2f : 0;
                        float squash = p >= .22f && after < .2f && !still ? 1 - .14f * Mathf.Sin(after / .2f * Mathf.PI) : 1;
                        Place(held, at + Vector2.up * (.35f + lift) - dir * .25f, size * 1.7f, angle + (dir.x < 0 ? 180 : 0), new Color(1, 1, 1, 1 - Smooth(.7f, 1, p)), true, squash);
                    }
                    if (p < .22f)
                    {
                        // the gavel's shadow gathers under it as it falls, and the spot it will strike warms
                        ground.Sector(at, size * .7f * fall, 0, 360, A(Gold.Body, .03f), A(Gold.Body, .16f * fall));
                        ground.Sector(at + Vector2.down * .05f, size * (.25f + .2f * fall), 0, 360, A(Midnight.Ink, .1f + .15f * fall), A(Midnight.Ink, .02f));
                        break;
                    }
                    float ring = still ? 1 : OutCubic(after);
                    Glow(at, size * 2.4f, Gold.Glow, .32f * (1 - after));
                    ground.Sector(at, size * (.75f + .35f * ring), 0, 360, A(Gold.Glow, .12f * (1 - after)), A(Gold.Body, .42f * (1 - after)));
                    Rays(at, size * .7f, after, Cream, 8, seed, .05f, size * .35f);
                    Scatter(at, size * 1.1f, after, InkToken.Coin, Gold, 4, .16f, seed, 1.1f);
                    break;
                }
                case Moment.IngotBlast:
                    Glow(at, size * 2.2f, Gold.Glow, .3f * (1 - p));
                    Cloud(at, size * .45f, p, Gold, seed, .18f, 4, .65f);
                    Scatter(at, size * 1.2f, p, InkToken.Coin, Gold, 8, .14f, seed, .6f, Cream, InkToken.Page);
                    break;
                case Moment.KnucklePop:
                    Glow(at, size * 1.8f, Water.Glow, .26f * (1 - p));
                    Scatter(at, size * .95f, p, InkToken.Star, Silver, 5, .13f, seed, .3f);
                    break;
                case Moment.MittCatch:
                    Rays(at, .22f, p, Leather, 4, seed, .045f, .2f);
                    Cloud(at + Vector2.down * .1f, .25f, p, Cream, seed, .05f, 3, .45f, true);
                    break;
                case Moment.Zap:
                    Glow(at, size * 2f, Cyan.Glow, .3f * (1 - p));
                    for (int i = 0; i < (sparse ? 2 : 4); i++)
                    {
                        // little zig-zag bolts jumping off the buzzer, re-drawn as they flicker
                        float a = seed + i * 90 + 20; int flicker = still ? 0 : Mathf.FloorToInt(p * 14);
                        var path = InkCanvas.Line(at + Dir(a) * .12f, at + Dir(a) * size * (.6f + .2f * (i % 2)), .09f, 0, true, seed + i * 7 + flicker);
                        air.Stroke(path, Brush(Cyan, .06f, fade, 1, still ? 0 : InQuad(Mathf.Clamp01((p - .3f) / .7f)), 1));
                    }
                    Scatter(at, size * .8f, p, InkToken.Spark, Cyan, 5, .12f, seed, .2f, Gold);
                    break;
                case Moment.Spark:
                    Rays(at, .1f, p, Gold, 5, seed, .045f, .22f);
                    Token(InkToken.Glint, Cream, at, .26f * (1 - .5f * p), seed, fade);
                    break;
                case Moment.HomeRun:
                    // a comic swing burst where the bat connected, with stars spinning off the way the enemy flew
                    Glow(at, size * 1.6f, Gold.Glow, .26f * (1 - p));
                    Rays(at, size * .25f, p, Cream, 8, seed, .055f, size * .4f);
                    Scatter(at, size * 1.2f, p, InkToken.Star, Gold, 5, .16f, seed, .2f, Cream, null, 90, face);
                    break;
                case Moment.Glitch:
                {
                    // the jack-in-the-box flickers: short bars of colour slide across it
                    int frame = still ? 0 : Mathf.FloorToInt(p * 18);
                    for (int i = 0; i < (sparse ? 3 : 6); i++)
                    {
                        float y = ((i * 37 + frame * 13) % 9 - 4) * .07f, x = ((i * 53 + frame * 29) % 7 - 3) * .06f;
                        air.Stroke(InkCanvas.Line(at + new Vector2(x - .14f, y), at + new Vector2(x + .14f, y)), Brush(Party[(i + frame) % Party.Length], .05f, fade, 1, 0, .3f, true));
                    }
                    break;
                }
                default:
                    // the ordinary burst many weapons share stays light: an opening inked rim, impact dashes and sparks, no cloud
                    Glow(at, size * 1.6f, Cream.Glow, .22f * (1 - p));
                    Rays(at, size * .45f, p, Cream, 5, seed, .04f, size * .25f);
                    Scatter(at, size, p, InkToken.Spark, Gold, 5, .11f, seed, .4f);
                    break;
            }
        }

        // ------------------------------------------------------------------ every weapon's own swing, hit and muzzle (WeaponInkStyle)
        /// <summary>A melee weapon's own swing, in its form, palette and token.</summary>
        public void Strike(WeaponInkStyle.Strike s, Vector2 at, Vector2 dir, float p, float size, float seed)
        {
            dir = dir.sqrMagnitude > .0001f ? dir.normalized : Vector2.right;
            Vector2 side = new Vector2(-dir.y, dir.x); float face = Angle(dir), fade = 1 - Smooth(.55f, 1, p);
            InkPalette pal = s.Main; Vector2 tip = at + dir * size * .85f;
            float head = still ? 1 : OutCubic(Mathf.Min(1, p / .3f)), tail = still ? 0 : InQuad(Mathf.Clamp01((p - .3f) / .7f));
            switch (s.Form)
            {
                case WeaponInkStyle.StrikeForm.Crescent:
                    for (int k = 0; k < Mathf.Max(1, s.Strokes); k++) Sweep(at, dir, size * (.85f - .14f * k), p, pal, k == 0 ? .2f : .1f, 70 - 8 * k, k * 6, 1, k * .06f);
                    Scatter(tip, size * .5f, p, s.Token, pal, 3, .15f, seed, .2f, null, null, 100, face);
                    break;
                case WeaponInkStyle.StrikeForm.Chop:
                {
                    // a heavy downward chop just ahead: the stroke falls, a short floor mark and chips
                    Vector2 from = tip + Vector2.up * size * .55f + side * .1f, to = tip + Vector2.down * size * .25f;
                    air.Stroke(InkCanvas.Line(from, to), Brush(pal, .2f, fade, head, tail, 1.4f));
                    if (p > .25f) ground.Sector(to, size * .3f, 0, 360, A(pal.Body, .35f * fade), A(pal.Body, 0));
                    Rays(to, .1f, Mathf.Clamp01((p - .25f) / .75f), Cream, 4, seed, .045f, .22f, 160, 90);
                    Scatter(to, size * .45f, Mathf.Clamp01((p - .25f) / .75f), s.Token, pal, 3, .14f, seed, .4f, null, null, 160, 90);
                    break;
                }
                case WeaponInkStyle.StrikeForm.Thrust:
                {
                    int n = Mathf.Max(1, s.Strokes);
                    for (int k = 0; k < n; k++)
                    {
                        Vector2 o = side * ((k - (n - 1) * .5f) * .14f);
                        air.Stroke(InkCanvas.Line(at + dir * .2f + o, at + dir * size * 1.05f + o), Brush(pal, n > 1 ? .08f : .12f, fade, head, tail, 2));
                    }
                    Token(s.Token, pal, at + dir * size * 1.05f, .2f * (still ? 1 : OutBack(Mathf.Min(1, p / .3f))) * (1 - .4f * p), face, fade);
                    break;
                }
                case WeaponInkStyle.StrikeForm.Drill:
                    air.Stroke(InkCanvas.Line(at + dir * .2f, at + dir * size * 1.05f, .07f, 5, false, seed), Brush(pal, .09f, fade, head, tail, 1.6f));
                    Token(s.Token, pal, at + dir * size * 1.05f, .18f, face + p * 300, fade);
                    break;
                case WeaponInkStyle.StrikeForm.Whip:
                    air.Stroke(InkCanvas.Line(at + dir * .15f, at + dir * size * 1.1f, .16f, 2.2f, false, seed + p * 3), Brush(pal, .09f, fade, head, tail, 1.2f));
                    Scatter(at + dir * size * 1.1f, size * .35f, p, s.Token, pal, 2, .15f, seed, .3f, null, null, 60, face);
                    break;
                case WeaponInkStyle.StrikeForm.Pound:
                {
                    // a slam into the floor ahead: a soft print fading out to nothing (no ring), a low puff, the token bouncing up
                    Vector2 spot = at + dir * size * .7f;
                    float press = still ? 1 : OutBack(Mathf.Min(1, p / .2f));
                    ground.Sector(spot, size * .4f * press, 0, 360, A(pal.Body, .42f * fade), A(pal.Body, 0));
                    Cloud(spot, size * .26f, p, Dust, seed, .1f, 3, 1, true);
                    Scatter(spot, size * .5f, p, s.Token, pal, 4, .15f, seed, -.25f, Cream, InkToken.Spark, 140, 90);
                    break;
                }
                case WeaponInkStyle.StrikeForm.Snip:
                    Draw(Moment.Snip, at, dir, p, size, seed);
                    Token(s.Token, pal, at + dir * size, .16f, seed, fade);
                    break;
                case WeaponInkStyle.StrikeForm.Twin:
                    Sweep(at, dir, size * .85f, p, pal, .14f, 55, 18);
                    Sweep(at, dir, size * .8f, p, Silver, .1f, 55, -18, 1, .08f);
                    Token(s.Token, pal, tip, .17f, face, fade);
                    break;
                case WeaponInkStyle.StrikeForm.Spin:
                {
                    // a wide whirl in front of the wielder (never round it): a long arc with its token riding the edge
                    Sweep(at, dir, size * .8f, p, pal, .16f, 105);
                    float a = face + 105 - 210 * head;
                    Token(s.Token, pal, at + Dir(a) * size * .8f, .2f, a * 3, fade);
                    break;
                }
                case WeaponInkStyle.StrikeForm.Rake:
                    for (int k = -1; k <= 1; k++)
                    {
                        float delay = (k + 1) * .06f; float q = Mathf.Clamp01((p - delay) / (1 - delay)); if (p < delay) continue;
                        var path = InkCanvas.Arc(at - dir * size * .15f + side * (k * .17f), size * .72f, size * .72f, face + 48, face - 40);
                        air.Stroke(path, Brush(pal, .11f, 1 - Smooth(.65f, 1, q), still ? 1 : OutCubic(Mathf.Min(1, q / .3f)), still ? 0 : InQuad(Mathf.Clamp01((q - .35f) / .65f)), 1.8f));
                    }
                    Token(s.Token, pal, tip, .15f, face, fade);
                    break;
                case WeaponInkStyle.StrikeForm.Jab:
                    Glow(at + dir * size, .8f, pal.Glow, .22f * (1 - p));
                    Rays(at + dir * size, .13f, p, pal, 6, seed, .05f, .22f);
                    Token(s.Token, pal, at + dir * size, .28f * (still ? 1 : OutBack(Mathf.Min(1, p / .3f))) * (1 - .4f * p), seed, fade);
                    break;
                case WeaponInkStyle.StrikeForm.Glaive:
                    Sweep(at, dir, size * .75f, p, pal, .18f, 60);
                    air.Stroke(InkCanvas.Line(at + dir * .2f, at + dir * size * 1.4f), Brush(Gold, .08f, fade, still ? 1 : OutCubic(Mathf.Clamp01((p - .15f) / .3f)), tail, 2));
                    Scatter(at + dir * size * 1.3f, size * .4f, p, s.Token, pal, 3, .14f, seed, -.1f, null, null, 80, face);
                    break;
                case WeaponInkStyle.StrikeForm.Print:
                    Draw(Moment.SwatPrint, at + dir * size * .9f, dir, p, size * .9f, seed);
                    Token(s.Token, pal, at + dir * size * .9f + Vector2.up * .2f, .16f, seed, fade);
                    break;
            }
        }
        /// <summary>A hit: a small flash in the weapon's own palette and two or three of its tokens knocked off.</summary>
        public void Hit(InkPalette pal, InkToken token, Vector2 at, Vector2 dir, float p, bool critical, float seed)
        {
            Glow(at, critical ? .9f : .6f, pal.Glow, (critical ? .3f : .2f) * (1 - p));
            Rays(at, .08f, p, pal, critical ? 6 : 4, seed, .04f, critical ? .26f : .18f);
            Scatter(at, critical ? .55f : .38f, p, token, pal, critical ? 3 : 2, critical ? .15f : .12f, seed, .25f);
        }
        /// <summary>A shot leaving the muzzle: a little puff in the weapon's palette and its token flicked forward.</summary>
        public void Muzzle(InkPalette pal, InkToken token, Vector2 at, Vector2 dir, float p, float seed)
        {
            dir = dir.sqrMagnitude > .0001f ? dir.normalized : Vector2.right;
            Cloud(at + dir * .08f * p, .14f, p, pal, seed, .06f, 3, 1);
            Token(token, pal, at + dir * (.12f + .3f * OutCubic(p)), .14f * (1 - .5f * p), seed, 1 - Smooth(.5f, 1, p));
        }

        // ------------------------------------------------------------------ what the props leave in the room
        /// <summary>A wet splash on the floor: uneven merged lobes under one inked edge, a lighter middle, two gloss strokes
        /// and a glint (an organic shape, never a disc).</summary>
        public void Pool(Vector2 c, float r, float life, InkPalette pal, float seed, float alpha = 1, float squash = .5f)
        {
            float a = alpha * Mathf.Clamp01(life); if (a <= .01f) return;
            const int n = 6;
            for (int i = 0; i < n; i++)
            {
                float angle = seed + i * 60 + ((i * 41) % 25);
                lobeAt[i] = c + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad) * r * .5f, Mathf.Sin(angle * Mathf.Deg2Rad) * r * squash * .45f);
                lobeR[i] = r * squash * (.62f + .16f * ((i * 5 + Mathf.RoundToInt(seed)) % 3));
            }
            Color deep = Color.Lerp(pal.Body, pal.Ink, .25f), middle = Color.Lerp(pal.Body, pal.Light, .22f);
            for (int i = 0; i < n; i++) ground.Sector(lobeAt[i], lobeR[i] + .04f, 0, 360, A(pal.Ink, .8f * a), A(pal.Ink, .8f * a));
            for (int i = 0; i < n; i++) ground.Sector(lobeAt[i], lobeR[i], 0, 360, A(deep, .9f * a), A(deep, .9f * a));
            for (int i = 0; i < n; i++) ground.Sector(Vector2.Lerp(lobeAt[i], c, .35f), lobeR[i] * .7f, 0, 360, A(middle, .9f * a), A(pal.Body, .9f * a));
            // a few drops flicked out past the edge
            for (int i = 0; i < 3; i++)
            {
                float angle = seed * 3 + i * 120 + 30; Vector2 drop = c + new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad) * r * 1.05f, Mathf.Sin(angle * Mathf.Deg2Rad) * r * squash * 1.05f);
                ground.Sector(drop, .07f, 0, 360, A(pal.Ink, .8f * a), A(pal.Ink, .8f * a)); ground.Sector(drop, .045f, 0, 360, A(pal.Body, a), A(pal.Body, a));
            }
            ground.Stroke(InkCanvas.Arc(c + new Vector2(-r * .12f, r * squash * .12f), r * .45f, r * squash * .35f, 110, 160), Brush(Glass, .05f, .9f * a, 1, 0, 1.4f, true));
            ground.Stroke(InkCanvas.Arc(c + new Vector2(-r * .12f, r * squash * .12f), r * .28f, r * squash * .22f, 112, 140), Brush(Glass, .035f, .7f * a, 1, 0, 1.4f, true));
            Token(InkToken.Glint, Glass, c + new Vector2(-r * .38f, r * squash * .3f), .2f, 0, .85f * a, false);
        }
        /// <summary>Bubbles rising and popping over a pool.</summary>
        public void Bubbles(Vector2 c, float r, float now, InkPalette pal, int n, float seed, float alpha = 1)
        {
            if (sparse) n = Mathf.Max(2, n / 2);
            for (int i = 0; i < n; i++)
            {
                float cycle = still ? .5f : Mathf.Repeat(now * .9f + i * .37f + seed, 1);
                Vector2 at = c + new Vector2(((i * 41) % 9 - 4) / 4f * r * .7f, cycle * .35f);
                Token(InkToken.Bubble, pal, at, .17f * (.6f + .4f * cycle), 0, alpha * (1 - cycle * cycle));
            }
        }
        /// <summary>A crack boring outward from the drill: one jagged spiral with short branching cracks and candy shavings.</summary>
        public void Spiral(Vector2 c, float r, float t, float now, float seed)
        {
            float grow = still ? 1 : Mathf.Clamp01(t * 3), turn = still ? 0 : -now * 90, alpha = 1 - t * .5f;
            List<Vector2> path = InkCanvas.Path; path.Clear();
            for (int i = 0; i <= 28; i++)
            {
                float f = i / 28f, a = (turn + seed + f * 540) * Mathf.Deg2Rad, jag = ((i * 37) % 7 - 3) * .012f;
                path.Add(c + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .5f) * (r * f * grow + jag));
            }
            ground.Stroke(path, Brush(Midnight, .06f, alpha, 1, 0, 1.4f, true));
            for (int k = 0; k < 4; k++)
            {
                float a = seed + k * 90 + 20; Vector2 from = c + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad) * .5f) * r * .8f * grow;
                Vector2 to = from + new Vector2(Mathf.Cos((a + 25) * Mathf.Deg2Rad), Mathf.Sin((a + 25) * Mathf.Deg2Rad) * .5f) * r * .45f * grow;
                ground.Stroke(InkCanvas.Line(from, to, .03f, 0, true, seed + k), Brush(Midnight, .04f, alpha, 1, 0, 1.6f, true));
            }
            Scatter(c, r * 1.1f, Mathf.Clamp01(t * 2), InkToken.Sand, Candy, 5, .1f, seed, .1f, Cream);
        }
        /// <summary>A brass coiled spring stretched from a to b.</summary>
        public void Coil(Vector2 a, Vector2 b, float alpha = 1)
        {
            float length = Vector2.Distance(a, b); if (length < .05f) return;
            air.Stroke(InkCanvas.Line(a, b, .09f, Mathf.Max(2, length * 6), false, 0), Brush(Gold, .07f, alpha, 1, 0, .3f));
        }
        /// <summary>A cord from a to b: slack wire on the floor, or a stretched elastic in the air.</summary>
        public void Cord(Vector2 a, Vector2 b, InkPalette pal, float width, bool slack, float alpha = 1)
        {
            if ((b - a).sqrMagnitude < .0025f) return;
            if (slack) ground.Stroke(InkCanvas.Lob(a, b, -.12f), Brush(pal, width, alpha, 1, 0, .25f));
            else air.Stroke(InkCanvas.Line(a, b), Brush(pal, width * (1 - .4f * Mathf.Clamp01(Vector2.Distance(a, b) / 4)), alpha, 1, 0, .4f));
        }
        /// <summary>A painted road strip: a cream band, dashed centre strokes and arrow chevrons along it.</summary>
        public void Stripes(Vector2 a, Vector2 b, float half, float now, float alpha = 1)
        {
            Vector2 axis = (b - a).normalized, normal = new Vector2(-axis.y, axis.x); float length = Vector2.Distance(a, b);
            ground.Band(a, b, half, A(Cream.Body, .3f * alpha), A(Cream.Body, .55f * alpha));
            for (int i = 0; i < 4; i++)
            {
                float f0 = (i + .15f) / 4, f1 = (i + .6f) / 4;
                ground.Stroke(InkCanvas.Line(a + axis * length * f0, a + axis * length * f1), Brush(Gold, .08f, alpha, 1, 0, .2f));
            }
            float run = still ? .5f : Mathf.Repeat(now * .8f, 1);
            Vector2 tip = a + axis * length * Mathf.Lerp(.2f, .9f, run);
            ground.Stroke(InkCanvas.Line(tip - axis * .18f + normal * half * .6f, tip), Brush(Gold, .05f, .8f * alpha * Mathf.Sin(run * Mathf.PI), 1, 0, .6f, true));
            ground.Stroke(InkCanvas.Line(tip - axis * .18f - normal * half * .6f, tip), Brush(Gold, .05f, .8f * alpha * Mathf.Sin(run * Mathf.PI), 1, 0, .6f, true));
        }
        /// <summary>A wake trailing a shot: broken wave arcs behind it and a few bubbles.</summary>
        public void Wake(Vector2 at, Vector2 velocity, InkPalette pal, float now, float seed, bool floor)
        {
            if (velocity.sqrMagnitude < .01f) return;
            Vector2 back = -velocity.normalized; float a = Angle(back); InkCanvas layer = floor ? ground : air;
            for (int i = 1; i <= (sparse ? 2 : 3); i++)
            {
                float r = .14f + .1f * i, alpha = .8f - .22f * i;
                layer.Stroke(InkCanvas.Arc(at + back * (.18f * i), r, r * (floor ? .5f : .8f), a - 70, a + 70), Brush(pal, .07f, alpha, 1, 0, 1.4f));
            }
            if (!sparse) Token(InkToken.Bubble, pal, at + back * .6f + new Vector2(0, still ? 0 : Mathf.Sin(now * 9 + seed) * .06f), .16f, 0, .8f);
        }
        /// <summary>Stars circling over a dizzy enemy's head.</summary>
        public void Orbit(Vector2 c, float r, float now, InkToken kind, InkPalette pal, int n, float alpha = 1)
        {
            for (int i = 0; i < n; i++)
            {
                float a = (still ? 0 : now * 300) + i * 360f / n; Vector2 o = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * r, Mathf.Sin(a * Mathf.Deg2Rad) * r * .4f);
                Token(kind, pal, c + o, .22f * (o.y < 0 ? 1.1f : .85f), a, alpha);
            }
        }
        /// <summary>Speed strokes trailing something flung through the air.</summary>
        public void Streaks(Vector2 at, Vector2 direction, float alpha)
        {
            if (direction.sqrMagnitude < .0001f || alpha <= .01f) return;
            Vector2 back = -direction.normalized, normal = new Vector2(-back.y, back.x);
            for (int i = -1; i <= 1; i++)
            {
                Vector2 from = at + back * (.25f + .06f * Mathf.Abs(i)) + normal * (i * .16f);
                air.Stroke(InkCanvas.Line(from, from + back * (.45f - .1f * Mathf.Abs(i))), Brush(Cream, .05f, alpha, 1, 0, 1.6f, true));
            }
        }
        // ------------------------------------------------------------------ open brushes (star-up ceremony and other staged moments)
        /// <summary>A painted ribbon along a path: the [tail, head] part of it, with a soft glow under it.</summary>
        public void Ribbon(List<Vector2> path, InkPalette pal, float width, float head, float tail, float alpha, bool glow = true)
        {
            if (path == null || path.Count < 2 || head <= tail || alpha <= .01f) return;
            if (glow) air.GlowRibbon(path, head, tail, width * 2.4f, A(pal.Glow, (calm ? .12f : .24f) * alpha));
            air.Stroke(path, Brush(pal, width, alpha, head, tail, 1.2f));
        }
        /// <summary>A soft wash that fades out to nothing at its edge (no rim): behind a figure, or darkening a corner.</summary>
        public void Wash(Vector2 c, float r, Color colour, float alpha, bool airborne = false)
            => (airborne ? air : ground).Sector(c, r, 0, 360, A(colour, alpha), A(colour, 0));
        /// <summary>A dimming wash over the floor layer, lightest at c and darkening outward (r reaches past the screen).</summary>
        public void Vignette(Vector2 c, float r, Color colour, float inner, float outer) => ground.Sector(c, r, 0, 360, A(colour, inner), A(colour, outer));
        /// <summary>A wash over a wedge of floor (an aimed danger zone): soft at the apex, pooling toward its far edge.</summary>
        public void Cone(Vector2 apex, float reach, float fromDegrees, float toDegrees, Color colour, float alpha, float apexAlpha)
            => ground.Sector(apex, reach, fromDegrees, toDegrees, A(colour, apexAlpha), A(colour, alpha));
        /// <summary>A brush stroke laid on the floor layer (under the units).</summary>
        public void FloorStroke(Vector2 a, Vector2 b, InkPalette pal, float width, float head, float tail, float alpha, float wave = 0, float seed = 0)
        {
            if (head <= tail || alpha <= .01f) return;
            ground.Stroke(InkCanvas.Line(a, b, wave, wave > 0 ? 2.5f : 0, false, seed), Brush(pal, width, alpha, head, tail, 1.3f));
        }
        /// <summary>A painted token placed exactly (size, angle, alpha), on the air layer.</summary>
        public void TokenAt(InkToken kind, InkPalette pal, Vector2 at, float size, float angle, float alpha) => Token(kind, pal, at, size, angle, alpha);
        /// <summary>A soft light behind something.</summary>
        public void Light(Vector2 at, float diameter, Color colour, float alpha) => Glow(at, diameter, colour, alpha);
        /// <summary>A short straight brush stroke (a swash, a drip, a laurel stem).</summary>
        public void Stroke(Vector2 a, Vector2 b, InkPalette pal, float width, float head, float tail, float alpha, float wave = 0, float seed = 0)
        {
            if (head <= tail || alpha <= .01f) return;
            air.Stroke(InkCanvas.Line(a, b, wave, wave > 0 ? 2.5f : 0, false, seed), Brush(pal, width, alpha, head, tail, 1.3f));
        }

        /// <summary>Glass shards scattering where the vase broke.</summary>
        public void Shards(Vector2 c, float p, float seed) => Scatter(c, .5f, p, InkToken.Shard, Glass, 6, .11f, seed, .3f);
    }
}
