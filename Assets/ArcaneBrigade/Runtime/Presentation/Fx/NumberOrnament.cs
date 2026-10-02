using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>A small piece of the element around a damage number or a status word (owner 29/10/2026): a flame licking up behind
    /// a fire number, sparks crackling off a lightning one, frost glints, droplets, bubbles, petals, a dark wisp... Small, short
    /// and behind the digits, so the number stays the thing that reads.</summary>
    public static class NumberOrnament
    {
        /// <param name="look">ElementLook index.</param><param name="at">Centre of the text.</param>
        /// <param name="height">Digit height in world units.</param><param name="width">Text width in world units.</param>
        /// <param name="t">0..1 through the number's life.</param>
        public static void Draw(FxBatch b, int look, Vector2 at, float height, float width, float t, float alpha, int seed, Color colour)
        {
            if (alpha <= .02f || look <= 0) return;
            float h = height, w = Mathf.Max(width, h), a = alpha * (1 - t * t);
            Color c = colour;
            switch ((ElementLook)look)
            {
                case ElementLook.Fire:
                    b.Quad(FxShape.Glow, at, new Vector2(w * .8f, h * .9f), 0, A(c, .45f * a), 0, seed);
                    for (int k = 0; k < 3; k++) b.Quad(FxShape.Flame, at + new Vector2((k - 1) * w * .32f, h * .45f), new Vector2(h * .35f, h * .7f), 0, A(c, .9f * a), t * 3 + k * .3f, seed + k, 1);
                    for (int k = 0; k < 4; k++) { float u = Mathf.Repeat(t * 2.2f + H(seed, k), 1); Glint(b, at + new Vector2((H(seed, k + 3) - .5f) * w, h * (.4f + u * 1.2f)), h * .12f, new Color(1, .8f, .3f), a * (1 - u), 0); }
                    break;
                case ElementLook.Storm:
                {
                    int f = Mathf.FloorToInt(t * 30);
                    for (int k = 0; k < 2; k++)
                    {
                        Vector2 side = at + new Vector2((k == 0 ? -1 : 1) * w * .55f, 0), end = side + new Vector2((k == 0 ? -1 : 1) * h * .7f, (H(seed + f, k) - .5f) * h);
                        if (H(seed + f, k + 5) > .3f) Bolt(b, side, end, h * .05f, Cyan, seed + f * 3 + k, h * .3f, a, 4);
                    }
                    b.Quad(FxShape.Glow, at, new Vector2(w * .75f, h * .8f), 0, A(c, .35f * a * (H(seed + f, 9) > .5f ? 1.4f : .8f)), 0, seed);
                    break;
                }
                case ElementLook.Ice: case ElementLook.Crystal:
                    b.Quad(FxShape.Glow, at, new Vector2(w * .7f, h * .75f), 0, A(new Color(.7f, .95f, 1f), .35f * a), 0, seed);
                    for (int k = 0; k < 4; k++) { float tw = Mathf.Max(0, Mathf.Sin(t * 14 + k * 1.7f)); Glint(b, at + new Vector2((H(seed, k) - .5f) * w * 1.2f, (H(seed, k + 4) - .3f) * h * 1.2f), h * (.12f + .18f * tw), White, a * tw, 45); }
                    break;
                case ElementLook.Water:
                    for (int k = 0; k < 5; k++) { float u = Rise(t, 0, .8f); Vector2 d = Dir(seed, k, 5, 1); Vector2 p = at + new Vector2(d.x * w * .6f * (.6f + u), h * (.2f + 1.4f * u * (1 - u)) - h * .3f); b.Quad(FxShape.Glow, p, h * .12f, A(new Color(.5f, .8f, 1f), a * (1 - u)), 0, seed); }
                    break;
                case ElementLook.Poison:
                    for (int k = 0; k < 4; k++) { float u = Mathf.Repeat(t * 1.6f + H(seed, k), 1); Vector2 p = at + new Vector2((H(seed, k + 2) - .5f) * w, -h * .2f + u * h * 1.3f); b.Quad(FxShape.Shock, p, h * (.1f + .12f * u), A(c, a * (1 - u)), .55f, seed + k); }
                    b.Quad(FxShape.Glow, at + Vector2.down * h * .5f, new Vector2(w * .6f, h * .3f), 0, A(c, .35f * a), 0, seed);
                    break;
                case ElementLook.Void: case ElementLook.Dream:
                    b.Quad(FxShape.Shade, at, new Vector2(w * .8f, h * .85f), 0, A(new Color(.08f, .0f, .14f), .5f * a), 0, seed);
                    b.Quad(FxShape.Vortex, at, h * .8f, t * 300, A(c, .55f * a), t, seed, 0);
                    break;
                case ElementLook.Light: case ElementLook.Gold:
                    b.Quad(FxShape.Rays, at, h * 1.1f, A(c, .5f * a), t, seed, 3);
                    Glint(b, at + new Vector2(w * .55f, h * .45f), h * (.25f + .1f * Mathf.Sin(t * 20)), White, a, t * 90);
                    break;
                case ElementLook.Wind:
                    for (int k = 0; k < 3; k++) { float y = (k - 1) * h * .35f, u = Rise(t, k * .08f, .6f + k * .08f); b.Beam(at + new Vector2(-w * .7f - h * u, y), at + new Vector2(-w * .7f + h * (.6f - u), y), h * .05f, A(White, a * (1 - u)), 0, seed + k, .5f); }
                    break;
                case ElementLook.Earth: case ElementLook.Paper:
                    b.Quad(FxShape.Dust, at + Vector2.down * h * .45f, new Vector2(w * .7f, h * .3f), 0, A(c, .55f * a), t, seed);
                    for (int k = 0; k < 4; k++) { float u = Rise(t, 0, .7f); Vector2 p = at + new Vector2((k - 1.5f) * w * .35f * (1 + u), -h * .4f + h * u * (1.4f - 1.6f * u)); b.Quad(FxShape.Disc, p, h * .07f, A(c, a * (1 - u)), 0, seed); }
                    break;
                case ElementLook.Bloom:
                    for (int k = 0; k < 4; k++) { float u = Mathf.Repeat(t * 1.2f + H(seed, k), 1); Vector2 p = at + new Vector2((H(seed, k + 1) - .5f) * w * 1.3f + Mathf.Sin(u * 6 + k) * h * .2f, h * .6f - u * h * 1.3f); b.Quad(FxShape.Glow, p, h * .14f, A(c, a * Mathf.Sin(u * Mathf.PI)), 0, seed); }
                    break;
                case ElementLook.Sound:
                    for (int k = 0; k < 2; k++) { float u = Mathf.Repeat(t * 2.5f + k * .5f, 1); b.Quad(FxShape.Shock, at, new Vector2(w * (.5f + .4f * u), h * (.6f + .4f * u)), 0, A(c, .7f * a * (1 - u)), .5f, seed + k); }
                    break;
                case ElementLook.Time:
                    b.Quad(FxShape.Shock, at, new Vector2(w * .7f, h * .8f), 0, A(c, .5f * a), .5f, seed);
                    b.Beam(at, at + new Vector2(Mathf.Cos(-t * 12) , Mathf.Sin(-t * 12)) * h * .6f, h * .04f, A(White, a), 0, seed, .5f);
                    break;
                default:
                    b.Quad(FxShape.Glow, at, new Vector2(w * .7f, h * .75f), 0, A(c, .3f * a), 0, seed);
                    break;
            }
        }
    }
}
