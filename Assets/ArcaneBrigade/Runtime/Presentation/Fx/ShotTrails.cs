using UnityEngine;
using static ArcaneBrigade.FxKit;

namespace ArcaneBrigade
{
    /// <summary>F1 (3), owner list 29/10/2026: a friend's ordinary shot trails its main bond's element (embers and a flame tongue,
    /// a crackling lightning tail, falling droplets, frost glints, tumbling leaves, a ribbon of notes...). Stateless and cheap for
    /// hundreds of shots: a handful of quads each on one shared FxBatch. Trail pieces sit still on the floor while the shot flies on
    /// (each piece's distance behind the shot grows with the shot's speed and wraps), so the trail reads as left behind.</summary>
    public static class ShotTrails
    {
        /// <summary>Shots drawn per frame; past it the rest fly as their painted token alone.</summary>
        public const int MaxShots = 200;
        private static Vector2 V(float x, float y) => new Vector2(x, y);

        /// <param name="at">The drawn shot (its token's centre).</param><param name="velocity">World units per second.</param>
        public static void Draw(FxBatch b, ElementLook look, Vector2 at, Vector2 velocity, int id, float now)
        {
            float speed = velocity.magnitude;
            Vector2 dir = speed > .01f ? velocity / speed : Vector2.right, n = new Vector2(-dir.y, dir.x);
            float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Color c = RulesV3Ink.LookColour(look), hot = Color.Lerp(c, Color.white, .55f);
            int seed = id * 7 + 3;
            // pieces k = 0..count-1 at distance d behind, growing at the shot's speed (capped so a fast shot does not strobe)
            float Piece(int k, int count, float length, out float age, out float fade)
            {
                float s = length / count, rate = Mathf.Clamp(speed, 1.5f, s * 24);
                float d = s * (k + Mathf.Repeat(now * rate / s + id * .37f, 1));
                age = d / rate; float f = d / length;
                fade = (1 - f) * Mathf.Clamp01(f * 6 + .2f);
                return d;
            }
            // every shot but the self-lit ones carries a soft head of its element's light
            if (look != ElementLook.Fire && look != ElementLook.Storm && look != ElementLook.Light && look != ElementLook.Void)
                b.Quad(FxShape.Glow, at - dir * .08f, V(.4f, .3f), ang, A(c, .42f), 0, seed + 50);
            switch (look)
            {
                case ElementLook.Fire: // a licking flame tongue behind the ember, sparks rising off it, a thread of smoke
                {
                    b.Quad(FxShape.Glow, at - dir * .1f, V(.34f, .26f), ang, A(c, .4f), 0, seed);
                    float lick = 1 + .18f * Mathf.Sin(now * 31 + id);
                    b.Quad(FxShape.Flame, at - dir * .36f, V(.2f, .46f * lick), ang + 90, A(new Color(1f, .4f, .06f), .9f), now * 1.6f, seed, 3);
                    b.Quad(FxShape.Flame, at - dir * .24f, V(.1f, .26f * lick), ang + 90, A(new Color(1f, .86f, .35f), .9f), now * 2f, seed + 1, 3);
                    for (int k = 0; k < 4; k++)
                    {
                        float d = Piece(k, 4, 1.1f, out float age, out float fade);
                        Vector2 p = at - dir * (d + .2f) + n * (H(seed, k) - .5f) * .3f + Vector2.up * age * .9f;
                        Glint(b, p, .09f, new Color(1f, .62f, .15f), fade * 1.1f, age * 300);
                    }
                    {
                        float d = Piece(0, 1, 1.2f, out float age, out float fade);
                        b.Quad(FxShape.Smoke, at - dir * (d + .45f) + Vector2.up * age * .5f, .14f + age * .5f, A(new Color(.2f, .14f, .12f), .35f * fade), age, seed + 9);
                    }
                    break;
                }
                case ElementLook.Storm: // only lightning is a bolt: a jagged tail that re-rolls every few hundredths of a second
                {
                    int roll = Mathf.FloorToInt(now * 22) + id * 13; Vector2 prev = at;
                    for (int j = 1; j <= 3; j++)
                    {
                        Vector2 p = at - dir * (j * .36f) + n * (H(roll, j) - .5f) * .26f * j;
                        b.Link(prev, p, .3f - j * .05f, A(new Color(.3f, .55f, 1f), .3f), roll + j, 1);
                        b.Link(prev, p, .15f - j * .025f, A(Color.Lerp(c, Color.white, .45f), 1 - j * .15f), roll + j, .5f);
                        prev = p;
                    }
                    b.Quad(FxShape.Glow, at, .42f, A(new Color(.55f, .8f, 1f), .6f), 0, seed);
                    if (H(roll, 9) > .4f) { Vector2 sp = at - dir * (.3f + H(roll, 7) * .7f) + n * (H(roll, 8) - .5f) * .6f; Glint(b, sp, .2f, hot, 1, roll * 40); }
                    break;
                }
                case ElementLook.Water: // a wet streak shedding droplets that fall away under it
                {
                    b.Beam(at, at - dir * 1.1f, .13f, A(new Color(.3f, .6f, 1f), .75f), now * 2, seed, .5f);
                    for (int k = 0; k < 4; k++)
                    {
                        float d = Piece(k, 4, 1.2f, out float age, out float fade);
                        Vector2 p = at - dir * (d + .12f) + n * (H(seed, k) - .5f) * .22f + Vector2.down * (age * .5f + age * age * 5);
                        b.Quad(FxShape.Glow, p, V(.1f, .15f), 0, A(new Color(.45f, .75f, 1f), fade), 0, seed + k);
                        b.Quad(FxShape.Glow, p + V(-.02f, .03f), .04f, A(Color.white, .9f * fade), 0, seed + k);
                    }
                    break;
                }
                case ElementLook.Ice: case ElementLook.Crystal: // a cold streak and still frost glints hanging in the air
                {
                    bool prism = look == ElementLook.Crystal;
                    b.Beam(at, at - dir * 1.2f, .11f, A(prism ? new Color(.85f, .75f, 1f) : new Color(.6f, .9f, 1f), .8f), 0, seed, .6f);
                    for (int k = 0; k < 3; k++)
                    {
                        float d = Piece(k, 3, 1.3f, out float age, out float fade);
                        Vector2 p = at - dir * (d + .15f) + n * (H(seed, k) - .5f) * .4f;
                        Color g = prism ? Color.HSVToRGB(Mathf.Repeat(.5f + k * .17f + now * .4f, 1), .35f, 1) : new Color(.8f, .97f, 1f);
                        Glint(b, p, .26f * (.6f + .4f * Mathf.Sin(age * 18 + k)), g, fade * 1.2f, 45 + age * 120);
                    }
                    break;
                }
                case ElementLook.Wind: // two curling strands of air twisting round the line of flight
                {
                    for (int strand = 0; strand < 2; strand++)
                    {
                        Vector2 prev = at; float sign = strand == 0 ? 1 : -1;
                        for (int j = 1; j <= 6; j++)
                        {
                            float d = j * .21f, w = Mathf.Sin(d * 3.6f - now * 12 + strand * 2.4f) * .2f * sign * Mathf.Sqrt(d / 1.26f);
                            Vector2 p = at - dir * d + n * w;
                            b.Link(prev, p, .15f, A(new Color(.6f, 1f, .8f), .65f - j * .09f), seed + j, 1); prev = p;

                        }
                    }
                    b.Quad(FxShape.Vortex, at - dir * 1.3f, .26f, now * 400, A(new Color(.8f, 1f, .92f), .55f), now, seed, 0);
                    break;
                }
                case ElementLook.Bloom: // leaves and petals shaken loose, tumbling and swaying as they settle
                {
                    for (int k = 0; k < 4; k++)
                    {
                        float d = Piece(k, 4, 1.3f, out float age, out float fade);
                        Vector2 p = at - dir * (d + .15f) + n * Mathf.Sin(age * 9 + k * 2) * .12f + Vector2.down * age * .35f;
                        Color leaf = k % 2 == 0 ? new Color(.5f, .95f, .35f) : new Color(1f, .55f, .78f);
                        b.Quad(FxShape.Glow, p, V(.24f, .1f), age * 420 + k * 70, A(leaf, fade * 1.4f), 0, seed + k);
                    }
                    break;
                }
                case ElementLook.Poison: // wobbling bubbles and a sickly wisp
                {
                    for (int k = 0; k < 3; k++)
                    {
                        float d = Piece(k, 3, 1.2f, out float age, out float fade);
                        float size = .11f + .07f * H(seed, k) + age * .08f;
                        Vector2 p = at - dir * (d + .15f) + n * Mathf.Sin(age * 12 + k) * .08f + Vector2.up * age * .3f;
                        b.Quad(FxShape.Glow, p, size, A(new Color(.6f, 1f, .25f), fade), 0, seed + k);
                        b.Quad(FxShape.Glow, p + V(-size * .35f, size * .35f), size * .3f, A(Color.white, .7f * fade), 0, seed + k);
                    }
                    {
                        float d = Piece(0, 1, 1f, out float age, out float fade);
                        b.Quad(FxShape.Smoke, at - dir * (d + .3f), .18f + age * .5f, A(new Color(.3f, .55f, .12f), .45f * fade), age, seed + 9);
                    }
                    break;
                }
                case ElementLook.Void: case ElementLook.Dream: // a dark wake with specks spiralling round it
                {
                    bool dream = look == ElementLook.Dream;
                    if (!dream) b.Quad(FxShape.Shade, at - dir * .55f, V(.75f, .22f), ang, A(new Color(.08f, .02f, .14f), .7f), 0, seed);
                    else b.Quad(FxShape.Glow, at - dir * .5f, V(.7f, .18f), ang, A(new Color(.6f, .45f, 1f), .45f), 0, seed);
                    for (int k = 0; k < 4; k++)
                    {
                        float d = Piece(k, 4, 1.3f, out float age, out float fade);
                        Vector2 p = at - dir * (d + .1f) + n * Mathf.Sin(d * 8 - now * 9 + k * 1.6f) * .2f;
                        Glint(b, p, dream ? .2f : .16f, dream ? new Color(.9f, .8f, 1f) : new Color(.75f, .4f, 1f), fade, age * 200);
                    }
                    break;
                }
                case ElementLook.Light: // a white-gold streak of light and a pair of glints
                {
                    b.Beam(at, at - dir * 1.35f, .15f, A(new Color(1f, .9f, .55f), .8f), 0, seed, .3f);
                    b.Quad(FxShape.Glow, at, .38f, A(new Color(1f, .97f, .8f), .55f), 0, seed);
                    for (int k = 0; k < 2; k++)
                    {
                        float d = Piece(k, 2, .9f, out float age, out float fade);
                        Glint(b, at - dir * (d + .2f) + n * (H(seed, k) - .5f) * .3f, .22f, new Color(1f, .95f, .75f), fade, 0);
                    }
                    break;
                }
                case ElementLook.Gold: // coins of light that wink as they turn
                {
                    for (int k = 0; k < 3; k++)
                    {
                        float d = Piece(k, 3, 1.2f, out float age, out float fade);
                        float wink = Mathf.Abs(Mathf.Sin(age * 14 + k));
                        Vector2 p = at - dir * (d + .15f) + n * (H(seed, k) - .5f) * .26f + Vector2.down * age * .2f;
                        b.Quad(FxShape.Glow, p, V(.15f * wink + .03f, .15f), 0, A(new Color(1f, .8f, .3f), fade * 1.4f), 0, seed + k);
                        if (wink > .75f) Glint(b, p, .2f, new Color(1f, .95f, .7f), fade, 0);
                    }
                    break;
                }
                case ElementLook.Earth: case ElementLook.Paper: // grit and dust (earth) or paper scraps fluttering down (paper)
                {
                    bool paper = look == ElementLook.Paper;
                    if (!paper) b.Quad(FxShape.Dust, at - dir * .6f, V(.8f, .26f), ang, A(new Color(.66f, .5f, .34f), .9f), now, seed);
                    for (int k = 0; k < 3; k++)
                    {
                        float d = Piece(k, 3, 1.2f, out float age, out float fade);
                        Vector2 p = at - dir * (d + .15f) + n * (H(seed, k) - .5f) * .28f + Vector2.down * (paper ? age * .3f + Mathf.Sin(age * 8 + k) * .05f : age * age * 4);
                        Color chip = paper ? new Color(1f, .96f, .86f) : new Color(.85f, .62f, .38f);
                        b.Quad(FxShape.Disc, p, paper ? V(.12f, .075f) : V(.07f, .06f), age * (paper ? 260 : 600) + k * 40, A(chip, .9f * fade), 0, seed + k);
                    }
                    break;
                }
                case ElementLook.Sound: // a ribbon of notes: dots riding a wave that travels along the trail
                {
                    for (int k = 0; k < 5; k++)
                    {
                        float d = .16f + k * .24f, fade = 1 - k / 5f;
                        Vector2 p = at - dir * d + n * Mathf.Sin(d * 6.5f - now * 16) * .19f;
                        b.Quad(FxShape.Glow, p, k % 2 == 0 ? .12f : .085f, A(k % 2 == 0 ? new Color(1f, .6f, 1f) : new Color(.7f, .85f, 1f), .95f * fade), 0, seed + k);
                    }
                    break;
                }
                case ElementLook.Time: // grains of sand winding round the shot like a helix
                {
                    b.Beam(at, at - dir * 1f, .08f, A(new Color(.95f, .85f, .55f), .6f), 0, seed, .7f);
                    for (int k = 0; k < 5; k++)
                    {
                        float d = .12f + k * .2f, turn = d * 9 - now * 12;
                        Vector2 p = at - dir * d + n * Mathf.Sin(turn) * .17f;
                        b.Quad(FxShape.Glow, p, Mathf.Cos(turn) > 0 ? .085f : .05f, A(new Color(1f, .88f, .5f), 1 - k / 5f), 0, seed + k);
                    }
                    break;
                }
                case ElementLook.Thread: // a pair of soft ribbons unspooling behind the shot in a slow smooth wave
                {
                    for (int strand = 0; strand < 2; strand++)
                    {
                        Vector2 prev = at;
                        for (int j = 1; j <= 6; j++)
                        {
                            float d = j * .22f; Vector2 p = at - dir * d + n * Mathf.Sin(d * 3.2f - now * 9 + strand * 1.4f) * .1f * Mathf.Sqrt(j / 6f);
                            b.Link(prev, p, strand == 0 ? .08f : .05f, A(strand == 0 ? new Color(1f, .45f, .5f) : new Color(1f, .85f, .7f), .9f - j * .13f), seed + j, 1); prev = p;
                        }
                    }
                    break;
                }
                default:
                    b.Quad(FxShape.Glow, at - dir * .15f, V(.4f, .2f), ang, A(c, .35f), 0, seed);
                    break;
            }
        }
    }
}
