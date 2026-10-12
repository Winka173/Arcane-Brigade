using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>F1 (2), owner list 29/10/2026: about 0.3 s before a friend uses its skill, its weapon gathers its bond's element:
    /// motes spiral IN to the weapon tip (never a ring or a burst on our side) while the tip swells with light and an element accent
    /// (a flame licking up, crackles, a swelling drop, a frost star, a little swirl...). At the cast the light leaves as one short
    /// streak toward the enemies and the tip goes dark. Presentation only, a few quads per friend.</summary>
    public static class CastTells
    {
        public const float Lead = .3f, ReleaseTime = .25f;
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        /// <param name="tip">The weapon tip (or the hand) as drawn.</param><param name="charge">0..1 over the last 0.3 s before the cast.</param>
        public static void Charge(FxBatch b, ElementLook look, Vector2 tip, float charge, float now, int seed)
        {
            float c = EaseOut(charge); if (c <= .01f) return;
            Color col = RulesV3Ink.LookColour(look), hot = Color.Lerp(col, Color.white, .6f);
            float pulse = 1 + .12f * Mathf.Sin(now * 38 + seed);
            b.Quad(FxShape.Glow, tip, (.14f + .3f * c) * pulse, A(col, .3f + .45f * c), 0, seed);
            b.Quad(FxShape.Glow, tip, .05f + .07f * c, A(Color.white, .3f + .4f * c), 0, seed + 1);
            // motes drawn in along a spiral, each at its own radius so they never line up into a ring
            for (int k = 0; k < 6; k++)
            {
                float u = Mathf.Repeat(now * 2.8f + k / 6f + H(seed, k) * .3f, 1), r = .75f * (1 - EaseOut(u)) * (.75f + .35f * H(seed, k + 7));
                float a = k * 1.047f + seed + u * 2.2f;
                Vector2 p = tip + V(Mathf.Cos(a), Mathf.Sin(a) * .8f) * r, to = Vector2.Lerp(p, tip, .4f);
                float alpha = c * Mathf.Sin(u * Mathf.PI);
                Mote(b, look, p, to, hot, alpha, now, seed + k);
            }
            switch (look)
            {
                case ElementLook.Fire:
                    b.Quad(FxShape.Flame, tip + V(0, .16f * c), V(.13f, .32f) * c, Mathf.Sin(now * 9) * 8, A(new Color(1f, .45f, .08f), .95f), now * 1.8f, seed, 3);
                    b.Quad(FxShape.Flame, tip + V(0, .08f * c), V(.06f, .17f) * c, 0, A(new Color(1f, .88f, .4f), .95f), now * 2.2f, seed + 2, 3);
                    break;
                case ElementLook.Storm:
                {
                    int roll = Mathf.FloorToInt(now * 20) + seed;
                    for (int j = 0; j < 2; j++)
                    {
                        float a = H(roll, j) * 6.28f; Vector2 z = tip + V(Mathf.Cos(a), Mathf.Sin(a) * .8f) * (.25f + .3f * c);
                        Bolt(b, tip, z, .022f, new Color(.55f, .8f, 1f), roll + j, .08f, .9f * c, 3);
                    }
                    break;
                }
                case ElementLook.Ice: case ElementLook.Crystal:
                    Glint(b, tip, .16f + .24f * c, look == ElementLook.Crystal ? new Color(.95f, .7f, 1f) : new Color(.55f, .88f, 1f), .85f * c, 45 + now * 60);
                    break;
                case ElementLook.Water: // a drop swelling at the tip, about to fall
                    b.Quad(FxShape.Glow, tip + V(0, -.09f * c), V(.07f, .11f) * (.4f + c), 0, A(new Color(.4f, .7f, 1f), .9f * c), 0, seed + 3);
                    b.Quad(FxShape.Glow, tip + V(-.02f, -.06f * c), .03f * c, A(Color.white, c), 0, seed + 4);
                    break;
                case ElementLook.Wind: case ElementLook.Void:
                    if (look == ElementLook.Void) b.Quad(FxShape.Shade, tip, .3f * c, A(new Color(.06f, .02f, .1f), .5f * c), 0, seed);
                    b.Quad(FxShape.Vortex, tip, .26f + .16f * c, now * -260, A(look == ElementLook.Void ? new Color(.7f, .35f, 1f) : new Color(.75f, 1f, .9f), .7f * c), now * .8f, seed, 1);
                    break;
                case ElementLook.Poison:
                    b.Quad(FxShape.Smoke, tip + V(0, .1f), .16f + .1f * c, A(new Color(.35f, .6f, .12f), .45f * c), now * .3f, seed);
                    break;
                case ElementLook.Light: case ElementLook.Dream:
                    b.Quad(FxShape.Rays, tip, .3f + .25f * c, A(look == ElementLook.Dream ? new Color(.8f, .6f, 1f) : new Color(1f, .95f, .7f), .7f * c), now * .5f, seed, 2);
                    break;
                case ElementLook.Gold:
                    Glint(b, tip + V(.08f, .1f), .12f + .2f * Mathf.Abs(Mathf.Sin(now * 9)) * c, new Color(1f, .9f, .5f), c, 0);
                    break;
                case ElementLook.Earth:
                    b.Quad(FxShape.Dust, tip + V(0, -.05f), V(.28f, .16f) * (.6f + .4f * c), 0, A(new Color(.6f, .45f, .3f), .55f * c), now * .6f, seed);
                    break;
                case ElementLook.Sound: // the glow beats three times as it fills
                    b.Quad(FxShape.Glow, tip, .2f + .2f * c * Mathf.Abs(Mathf.Sin(charge * Mathf.PI * 3)), A(new Color(1f, .6f, 1f), .5f * c), 0, seed + 5);
                    break;
                case ElementLook.Time: // a little clock hand sweeping round the tip
                {
                    float a = now * 720 + seed * 30; Vector2 d = V(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad) * .8f);
                    b.Beam(tip, tip + d * (.18f + .14f * c), .035f, A(new Color(1f, .9f, .55f), .9f * c), 0, seed, .8f);
                    break;
                }
            }
        }

        /// <summary>The cast: the gathered light leaves the tip as one short streak toward the enemies and the tip dims.</summary>
        public static void Release(FxBatch b, ElementLook look, Vector2 tip, Vector2 aim, float age, int seed)
        {
            float k = age / ReleaseTime; if (k < 0 || k >= 1) return;
            Color col = RulesV3Ink.LookColour(look);
            Vector2 d = aim.sqrMagnitude > .0001f ? aim.normalized : Vector2.right;
            float go = EaseOut3(k), fade = 1 - k;
            b.Quad(FxShape.Glow, tip, .42f * (1 - go), A(col, .8f * fade), 0, seed);
            b.Beam(tip + d * (.1f + .7f * go), tip + d * (.45f + 1.5f * go), .11f, A(Color.Lerp(col, Color.white, .35f), fade), 0, seed, .5f);
            Glint(b, tip + d * (.45f + 1.5f * go), .16f, Color.Lerp(col, Color.white, .5f), fade, go * 90);

        }

        private static void Mote(FxBatch b, ElementLook look, Vector2 p, Vector2 to, Color hot, float alpha, float now, int seed)
        {
            if (alpha <= .01f) return;
            switch (look)
            {
                case ElementLook.Ice: case ElementLook.Crystal: case ElementLook.Gold: case ElementLook.Dream: case ElementLook.Light:
                    Glint(b, p, .09f, hot, alpha, now * 200 + seed * 40); break;
                case ElementLook.Paper: case ElementLook.Earth:
                    b.Quad(FxShape.Disc, p, look == ElementLook.Paper ? V(.05f, .03f) : V(.032f, .028f), now * 300 + seed * 50, A(look == ElementLook.Paper ? new Color(1f, .96f, .86f) : new Color(.85f, .62f, .38f), alpha), 0, seed); break;
                case ElementLook.Bloom:
                    b.Quad(FxShape.Glow, p, V(.09f, .04f), now * 380 + seed * 60, A(seed % 2 == 0 ? new Color(.5f, .95f, .35f) : new Color(1f, .55f, .78f), alpha), 0, seed); break;
                case ElementLook.Thread:
                    b.Link(p, to, .025f, A(new Color(1f, .45f, .5f), alpha), seed, .9f); break;
                default:
                    b.Link(p, to, .03f, A(hot, alpha * .6f), seed, .8f);
                    b.Quad(FxShape.Glow, p, .055f, A(hot, alpha), 0, seed); break;
            }
        }
    }
}
