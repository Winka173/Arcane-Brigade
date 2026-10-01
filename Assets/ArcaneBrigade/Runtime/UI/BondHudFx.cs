using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The active bond rows of the run HUD (top right) in their own element (owner 29/10/2026): a wash and progress bar
    /// in the element's colour, a glow behind the icon, and a small animation of the element living in the row (embers rise,
    /// sparks crackle, waves roll, leaves drift...). It grows busier with the bond's level and stays behind the text.</summary>
    public static class BondHudFx
    {
        public static ElementLook Look(string bondId) => RulesV3Ink.LookFor(bondId);

        /// <summary>The element's colour, a little deepened so it reads on the cream ticket.</summary>
        public static Color Colour(string bondId)
        {
            Color c = RulesV3Ink.LookColour(Look(bondId));
            return c == Color.white ? ArcadeSkin.Teal : c;
        }

        private static float H(int a, int b) { float v = Mathf.Sin(a * 12.9898f + b * 78.233f) * 43758.5453f; return v - Mathf.Floor(v); }
        private static Color A(Color c, float a) { c.a = Mathf.Clamp01(a); return c; }

        /// <summary>The living background of an active row. <paramref name="still"/> (Reduced Motion) keeps only the colour.</summary>
        public static void Row(Rect line, string bondId, int level, int index, float clock, bool still, bool calm)
        {
            ElementLook look = Look(bondId); Color c = Colour(bondId);
            float breathe = still ? .5f : .5f + .5f * Mathf.Sin(clock * 2.2f + index);
            ArcadeSkin.Slant(line, A(c, .14f + .08f * breathe + .03f * level));
            ArcadeSkin.Glow(new Rect(line.x - 8, line.y - 8, 44, 44), A(c, calm ? .35f : .3f + .3f * breathe)); // behind the icon
            if (still) return;
            int n = 3 + 2 * Mathf.Clamp(level, 1, 4), seed = bondId.GetHashCode() & 0xffff;
            Rect area = new Rect(line.x + 30, line.y + 2, line.width - 72, line.height - 4);
            switch (look)
            {
                case ElementLook.Fire: // embers rise and flicker out, heat glows along the bottom
                    ArcadeSkin.Line(new Rect(area.x, area.yMax - 3, area.width, 3), A(c, .25f + .15f * Mathf.Sin(clock * 9 + index)));
                    for (int k = 0; k < n; k++) { float u = Mathf.Repeat(clock * (.6f + .4f * H(seed, k)) + H(seed, k + 9), 1); Dot(area.x + area.width * H(seed, k + 3) + Mathf.Sin(clock * 5 + k) * 4, area.yMax - u * area.height, 4 - 2 * u, A(Color.Lerp(new Color(1, .85f, .3f), c, u), 1 - u)); }
                    break;
                case ElementLook.Storm: // a crackle hops across the row, re-rolled a dozen times a second
                {
                    int f = Mathf.FloorToInt(clock * 12); Vector2 p = new Vector2(area.x, area.center.y);
                    for (int k = 1; k <= 6 + level; k++)
                    {
                        Vector2 q = new Vector2(area.x + area.width * k / (6f + level), area.y + area.height * H(f + seed, k));
                        Segment(p, q, A(Color.Lerp(c, Color.white, .5f), H(f, 99) > .35f ? .8f : .2f)); p = q;
                    }
                    break;
                }
                case ElementLook.Water: // a wave of droplets rolls along the bottom
                    for (int k = 0; k < 10 + 2 * level; k++) { float x = k / (10f + 2 * level); Dot(area.x + area.width * x, area.yMax - 5 - 4 * Mathf.Sin(x * 12 - clock * 4), 3, A(c, .7f)); }
                    break;
                case ElementLook.Wind: // fast streaks blow across
                    for (int k = 0; k < n; k++) { float u = Mathf.Repeat(clock * (1.1f + .5f * H(seed, k)) + H(seed, k + 4), 1.3f) - .15f; float y = area.y + area.height * H(seed, k + 7); ArcadeSkin.Line(new Rect(area.x + area.width * u, y, 22 + 10 * H(seed, k), 2), A(c, .7f * Mathf.Sin(Mathf.Clamp01(u) * Mathf.PI))); }
                    break;
                case ElementLook.Ice: case ElementLook.Crystal: // frost glints twinkle in place
                    for (int k = 0; k < n; k++) { float tw = Mathf.Max(0, Mathf.Sin(clock * (2 + H(seed, k) * 2) + k * 1.9f)); Glint(area.x + area.width * H(seed, k + 2), area.y + area.height * H(seed, k + 5), 3 + 5 * tw, A(Color.Lerp(c, Color.white, .5f), tw)); }
                    break;
                case ElementLook.Poison: // bubbles rise, swell and pop
                    for (int k = 0; k < n; k++) { float u = Mathf.Repeat(clock * (.45f + .3f * H(seed, k)) + H(seed, k + 1), 1); Ring(area.x + area.width * H(seed, k + 6), area.yMax - u * area.height, 2 + 4 * u, A(c, u > .9f ? 0 : .8f)); }
                    break;
                case ElementLook.Bloom: // petals drift down and across
                    for (int k = 0; k < n; k++) { float u = Mathf.Repeat(clock * .35f + H(seed, k), 1); Dot(area.x + area.width * Mathf.Repeat(H(seed, k + 3) + u * .3f, 1), area.y + u * area.height + Mathf.Sin(clock * 3 + k) * 3, 4, A(c, Mathf.Sin(u * Mathf.PI))); }
                    break;
                case ElementLook.Void: case ElementLook.Dream: // specks are drawn in to a dark heart
                {
                    Vector2 heart = new Vector2(area.x + area.width * .5f, area.center.y);
                    ArcadeSkin.Glow(new Rect(heart.x - 30, heart.y - 14, 60, 28), A(new Color(.1f, .02f, .2f), .25f + .15f * breathe));
                    for (int k = 0; k < n; k++) { float u = Mathf.Repeat(clock * .5f + H(seed, k), 1); float a = H(seed, k + 2) * 6.28f; Dot(heart.x + Mathf.Cos(a) * area.width * .5f * (1 - u), heart.y + Mathf.Sin(a) * area.height * .5f * (1 - u), 3, A(c, u)); }
                    break;
                }
                case ElementLook.Light: case ElementLook.Gold: // a bright sweep with glints in its wake
                {
                    float s = Mathf.Repeat(clock * .6f + index * .23f, 1.4f);
                    if (s < 1) ArcadeSkin.Glow(new Rect(area.x + s * area.width - 16, area.y - 4, 32, area.height + 8), A(Color.Lerp(c, Color.white, .4f), .55f * Mathf.Sin(s * Mathf.PI)));
                    for (int k = 0; k < n; k++) { float tw = Mathf.Max(0, Mathf.Sin(clock * 3 + k * 2.3f)); Glint(area.x + area.width * H(seed, k), area.y + area.height * H(seed, k + 8), 2 + 4 * tw, A(c, tw)); }
                    break;
                }
                case ElementLook.Earth: case ElementLook.Paper: // flakes and grit settle slowly
                    for (int k = 0; k < n; k++) { float u = Mathf.Repeat(clock * .25f + H(seed, k), 1); ArcadeSkin.Line(new Rect(area.x + area.width * H(seed, k + 4), area.y + u * area.height, 4, 3), A(c, .8f * Mathf.Sin(u * Mathf.PI))); }
                    break;
                case ElementLook.Sound: // a little equaliser bounces to a beat
                    for (int k = 0; k < 6 + level; k++) { float h = .25f + .75f * Mathf.Abs(Mathf.Sin(clock * (3 + k * .7f) + k)); float w = area.width / (6f + level); ArcadeSkin.Line(new Rect(area.x + k * w + w * .3f, area.yMax - h * area.height, w * .4f, h * area.height), A(c, .45f)); }
                    break;
                case ElementLook.Time: // a pendulum ticks across the row
                {
                    float sw = Mathf.Sin(clock * 3.2f); Dot(area.center.x + sw * area.width * .45f, area.yMax - 4 - (1 - sw * sw) * 6, 5, A(c, .85f));
                    for (int k = 0; k <= 8; k++) ArcadeSkin.Line(new Rect(area.x + area.width * k / 8f, area.yMax - 2, 1, 2), A(c, .5f));
                    break;
                }
                default:
                    if (!calm) { float sweep = Mathf.Repeat(clock * .45f + index * .23f, 1.6f); if (sweep < 1) ArcadeSkin.Line(new Rect(line.x + sweep * (line.width - 18), line.y, 18, line.height), new Color(1, 1, 1, .22f * Mathf.Sin(sweep * Mathf.PI))); }
                    break;
            }
        }

        /// <summary>A level reached: the row flares in its colour and a ring of light runs out of the icon.</summary>
        public static void Burst(Rect line, string bondId, float burst)
        {
            if (burst <= 0) return; Color c = Colour(bondId);
            ArcadeSkin.Slant(line, A(Color.Lerp(c, Color.white, .3f), .55f * burst));
            float r = 14 + 40 * (1 - burst);
            ArcadeSkin.Glow(new Rect(line.x + 14 - r, line.y + 14 - r, r * 2, r * 2), A(c, burst));
        }

        private static void Dot(float x, float y, float r, Color c) { if (c.a > .01f) ArcadeSkin.Dot(new Rect(x - r, y - r, r * 2, r * 2), c); }
        private static void Ring(float x, float y, float r, Color c) { Dot(x, y, r, A(c, c.a * .35f)); Dot(x - r * .3f, y - r * .3f, Mathf.Max(1, r * .3f), A(Color.white, c.a)); }
        private static void Glint(float x, float y, float r, Color c)
        {
            if (c.a <= .01f) return;
            ArcadeSkin.Line(new Rect(x - r, y - .75f, r * 2, 1.5f), c); ArcadeSkin.Line(new Rect(x - .75f, y - r, 1.5f, r * 2), c);
        }
        private static void Segment(Vector2 a, Vector2 b, Color c)
        {
            int steps = Mathf.Max(2, Mathf.CeilToInt((b - a).magnitude / 3));
            for (int k = 0; k < steps; k++) { Vector2 p = Vector2.Lerp(a, b, k / (float)steps); ArcadeSkin.Line(new Rect(p.x - 1, p.y - 1, 2.5f, 2.5f), c); }
        }
    }
}
