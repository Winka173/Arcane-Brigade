using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Every hero's own signature (owner 23/10/2026: "make them all impressive and satisfying, and easy to tell apart").
    /// Each skill strike ends in that hero's own impact - a stamped seal, a bouncing die, a fountain of coins, petals opening,
    /// gold seams, a lightning crackle - and every transformation carries its own ambient life around the hero. A full gauge
    /// is announced by the hero's tokens circling over its head. Painted tokens and brushed light only: no pools, no plain
    /// shapes, no ring opening round the wielder.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private static readonly InkPalette SealWax = new InkPalette(new Color(.24f, .04f, .06f), new Color(.8f, .16f, .2f), new Color(1, .88f, .8f), new Color(1, .55f, .5f), new Color(.98f, .82f, .4f));
        private static readonly InkPalette Juice = new InkPalette(new Color(.3f, .13f, .02f), new Color(1f, .62f, .16f), new Color(1, .95f, .7f), new Color(1, .75f, .3f), new Color(.5f, .75f, .25f));

        // ------------------------------------------------------------------ motions the signatures are made of
        /// <summary>Signatures are drawn 35% larger than the mechanic's own tokens so they read on a phone (owner 23/10/2026).</summary>
        private const float SignatureSize = 1.35f;
        private float sigScale = 1, sigReach = 1;
        private void SigToken(InkToken kind, Vector2 at, float size, float angle, float alpha, bool airborne = true, int face = 0, InkPalette? palette = null, float squash = 1)
            => Token(kind, at, size * sigScale, angle, alpha, airborne, face, palette, squash);
        /// <summary>Pieces thrown out from an impact point (never from the hero), spinning and fading.</summary>
        private void Spray(InkToken kind, Vector2 at, int n, float reach, float size, float p, float fade, float seed, InkPalette? pal = null, float lift = 0)
        {
            if (sparse) n = Mathf.Max(2, n / 2);
            float e = still ? .6f : OutCubic(p);
            for (int i = 0; i < n; i++)
            {
                float a = seed + i * 360f / n + (i % 2) * 14;
                Vector2 pos = at + Dir(a) * reach * sigReach * e * (i % 2 == 0 ? 1 : .75f) + Vector2.up * lift * Mathf.Sin(e * Mathf.PI);
                SigToken(kind, pos, size * (1 - .45f * p), a + p * 220, fade * (1 - InQuad(p)), true, 0, pal);
            }
        }
        /// <summary>Pieces popping up from the impact and falling back in arcs (coins, dice, juice).</summary>
        private void Fountain(InkToken kind, Vector2 at, int n, float height, float spread, float size, float p, float fade, float seed, InkPalette? pal = null, int face = 0)
        {
            if (sparse) n = Mathf.Max(1, n / 2);
            float q = still ? .5f : p;
            for (int i = 0; i < n; i++)
            {
                float side = n == 1 ? 0 : (i / (float)(n - 1) - .5f) * 2;
                Vector2 pos = at + new Vector2(side * spread * sigReach * q, height * sigReach * 4 * q * (1 - q) * (1 - .2f * Mathf.Abs(side)));
                SigToken(kind, pos, size, seed + i * 70 + q * 360 * (side >= 0 ? 1 : -1), fade * (1 - Smooth(.8f, 1, p)), true, face == 0 ? 0 : 1 + (face + i) % 6, pal);
            }
        }
        /// <summary>Pieces drifting up from the impact, swaying (notes, stars, steam, bubbles).</summary>
        private void Rise(InkToken kind, Vector2 at, int n, float height, float size, float p, float fade, float seed, InkPalette? pal = null)
        {
            if (sparse) n = Mathf.Max(1, n / 2);
            for (int i = 0; i < n; i++)
            {
                float q = still ? .5f : Mathf.Clamp01(p * 1.25f - i * .1f);
                Vector2 pos = at + new Vector2((i - (n - 1) * .5f) * .2f + Mathf.Sin(q * 6 + i * 2 + seed) * .1f, .1f + q * height);
                SigToken(kind, pos, size * (1 - .3f * q), Mathf.Sin(q * 5 + i) * 22, fade * Mathf.Sin(Mathf.Min(1, q * 1.4f + .15f) * Mathf.PI), true, 0, pal);
            }
        }
        /// <summary>A token slammed onto the impact: it lands big, squashes and settles (a seal, a stamp, a dumpling).</summary>
        private void Stamp(InkToken kind, Vector2 at, float size, float p, float fade, InkPalette? pal = null, float angle = -6, int face = 0)
        {
            float s = still ? 1 : OutBack(Mathf.Min(1, p / .25f)), squash = still ? 1 : 1 - .2f * Mathf.Sin(Mathf.Clamp01(p / .35f) * Mathf.PI);
            SigToken(kind, at, size * Mathf.Lerp(1.7f, 1, s), angle, Mathf.Clamp01(p / .08f) * fade, true, face, pal, squash);
        }
        /// <summary>Short crackling strokes out of the impact (lightning, gold seams, snapped threads).</summary>
        private void Crackle(Vector2 at, int n, float reach, float p, float fade, int seed, InkPalette pal, bool zigzag = true, float width = .035f)
        {
            if (sparse) n = Mathf.Max(2, n / 2);
            InkPalette was = ink; ink = pal;
            float grow = still ? 1 : OutCubic(Mathf.Min(1, p / .3f)); reach *= sigReach;
            for (int i = 0; i < n; i++)
            {
                Vector2 end = at + Dir(seed * 37 % 360 + i * 360f / n + (i % 2) * 20) * reach * (i % 2 == 0 ? 1 : .7f);
                Soft(air, InkCanvas.Line(at, Vector2.Lerp(at, end, grow), reach * .09f, zigzag ? 0 : 4, zigzag, seed + i + (still ? 0 : Mathf.FloorToInt(t * 18))), 1, Mathf.Max(0, p * 1.3f - .45f), width, fade);
            }
            ink = was;
        }

        // ------------------------------------------------------------------ impact signatures
        /// <summary>The hero's own end to a skill strike, at the point it lands.</summary>
        private void Signature(HeroIdentityGlyph g, Vector2 at, Vector2 dir, float p, float fade)
        {
            if (p <= 0 || p >= 1 || fade <= .02f) return;
            float s = Seed(g); int seed = g.Seed;
            sigScale = SignatureSize * (g.Finale ? 2f : g.Big ? 1.5f : 1); sigReach = g.Finale ? 1.7f : g.Big ? 1.35f : 1;
            if (g.Finale) Spray(style.Token, at + Vector2.up * .2f, 10, 1.1f, .2f, p, fade, s); // the finale bursts into the hero's signs
            if (g.Big && !sparse) Spray(InkToken.Glint, at + Vector2.up * .2f, 6, .95f, .17f, p, fade, s + 30, GlintPalette(ink)); // a felling blow sparkles
            Vector2 up = at + Vector2.up * .2f;
            switch (style.Id)
            {
                case "LanternKnight": Rise(InkToken.Ember, at, 5, 1.1f, .2f, p, fade, s); Stamp(InkToken.Ember, up, .42f, p, fade * (1 - p), null, 0); break;
                case "Postmaster": Stamp(InkToken.Seal, up, .5f, p, fade, SealWax); Spray(InkToken.Envelope, up, 3, .75f, .24f, p, fade, s, null, .35f); break;
                case "DrowsySeer": Rise(InkToken.Star, at, 3, 1f, .2f, p, fade, s); Stamp(InkToken.Moon, up, .4f, p, fade * (1 - p), null, -20); break;
                case "RootOracle":
                    for (int i = 0; i < 3; i++)
                    {
                        float q = still ? .7f : Mathf.Sin(Mathf.Min(1, p * 1.6f) * Mathf.PI) , x = (i - 1) * .22f;
                        SigToken(InkToken.Thorn, at + new Vector2(x, .1f + .35f * q * (i == 1 ? 1.3f : 1)), .3f * (.4f + .6f * q) * (i == 1 ? 1.25f : 1), 90 - x * 60, fade, true);
                    }
                    Spray(InkToken.Leaf, up, 4, .5f, .15f, p, fade, s, null, .3f); break;
                case "LoneJelly": Stamp(InkToken.Drop, up, .36f, p, fade * (1 - p), null, 180); Spray(InkToken.Drop, up, 5, .55f, .13f, p, fade, s, null, .3f); break;
                case "GlassDice":
                {
                    // the die bounces twice and settles on its face
                    float q = still ? 1 : p, hop = still ? 0 : Mathf.Abs(Mathf.Sin(q * Mathf.PI * 2)) * (1 - q) * .7f;
                    SigToken(InkToken.Die, at + new Vector2(.15f * (1 - q), .2f + hop), .4f, still ? 0 : (1 - q) * 540, fade, true, 1 + seed % 6);
                    if (seed % 6 == 5) Fountain(InkToken.Coin, up, 5, 1f, .7f, .2f, p, fade, s); // a six pays out
                    else Spray(InkToken.Glint, up, 4, .45f, .14f, p, fade, s, GlintPalette(ink));
                    break;
                }
                case "TideSinger":
                {
                    Vector2 d = dir.sqrMagnitude > .01f ? dir.normalized : Vector2.right;
                    Soft(air, InkCanvas.Arc(at + Vector2.up * .1f, .45f, .3f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 40, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + 160), still ? 1 : OutCubic(p / .5f), InQuad(Mathf.Max(0, p - .4f) / .6f), .07f, fade);
                    Rise(InkToken.Bubble, at, 3, .8f, .14f, p, fade, s, ArsenalInk.Water); Rise(InkToken.Note, up, 1, 1f, .26f, p, fade, s);
                    break;
                }
                case "CometRunner": Spray(InkToken.Spark, up, 5, .7f, .22f, p, fade, s - 90); Rise(InkToken.Ember, at, 3, .7f, .16f, p, fade, s); break;
                case "MirrorKeeper": Spray(InkToken.Shard, up, 6, .7f, .2f, p, fade, s); Stamp(InkToken.Glint, up, .4f, p, fade * (1 - p), GlintPalette(ink), 0); break;
                case "TreasureAcorn": Fountain(InkToken.Coin, up, 5, 1.1f, .8f, .22f, p, fade, s); break;
                case "VerdantMatron":
                {
                    int n = sparse ? 3 : 5; float open = still ? 1 : OutBack(Mathf.Min(1, p / .4f));
                    for (int i = 0; i < n; i++) { float a = s + i * 360f / n; SigToken(InkToken.Petal, up + Dir(a) * .28f * open, .26f * open, a - 90, fade * (1 - Smooth(.6f, 1, p)), true); }
                    Rise(InkToken.Glint, up, 3, .8f, .1f, p, fade, s, GlintPalette(ink));
                    break;
                }
                case "CopperCauldron": Rise(InkToken.Puff, at, 4, 1f, .3f, p, fade, s, ArsenalInk.Cotton); break;
                case "ChinaMarionette": Crackle(up, 5, .55f, p, fade, seed, GoldPalette, true, .03f); Spray(InkToken.Shard, up, 3, .4f, .16f, p, fade, s); break;
                case "SoapSkater":
                    if (p < .35f) SigToken(InkToken.Bubble, up, .2f + .14f * (p / .35f), 0, fade * .85f, true);
                    else Spray(InkToken.Bubble, up, 5, .5f, .1f, (p - .35f) / .65f, fade * .85f, s);
                    break;
                case "VelvetRegent":
                {
                    float fall = still ? 1 : Mathf.Min(1, p / .22f);
                    SigToken(InkToken.Spear, at + Vector2.up * (.25f + 1.4f * (1 - fall)), .5f, -90, fade * (1 - Smooth(.7f, 1, p)), true);
                    if (fall >= 1) Spray(InkToken.Glint, at + Vector2.up * .15f, 4, .45f, .14f, (p - .22f) / .78f, fade, s, GoldPalette);
                    break;
                }
                case "ButtonCavalier": Crackle(up, 2, .5f, p, fade, seed, GoldPalette, false, .03f); Fountain(InkToken.Button, up, 3, .7f, .6f, .2f, p, fade, s); break;
                case "NightMaw":
                {
                    float shut = still ? .08f : Mathf.Lerp(.45f, .06f, OutBack(Mathf.Min(1, p / .25f)));
                    SigToken(InkToken.Fang, up + Vector2.up * shut, .38f, 180, fade, true); SigToken(InkToken.Fang, up - Vector2.up * shut, .38f, 0, fade, true);
                    Rise(InkToken.Puff, at, 2, .6f, .24f, p, fade, s, ArsenalInk.Violet);
                    break;
                }
                case "ThornTreasurer":
                    for (int i = 0; i < 3; i++) { float a = s + i * 120; float k = still ? 1 : OutCubic(Mathf.Min(1, p / .2f)); SigToken(InkToken.Thorn, up + Dir(a) * Mathf.Lerp(.7f, .18f, k), .26f, a + 180, fade, true); }
                    Fountain(InkToken.Coin, up, 1, .8f, 0, .2f, p, fade, s);
                    break;
                case "CitrusMarshal": Stamp(InkToken.Citrus, up, .36f, p, fade * (1 - p * .7f), null, s); Fountain(InkToken.Drop, up, 4, .7f, .6f, .13f, p, fade, s, Juice); break;
                case "IvorySovereign": Stamp(InkToken.Seal, up, .55f, p, fade); Spray(InkToken.Glint, up, 4, .5f, .13f, p, fade, s, GlintPalette(ink)); break;
                case "PetalCitadel": Spray(InkToken.Petal, up, 5, .75f, .2f, p, fade, s); break;
                case "PatchworkHost": Fountain(InkToken.Patch, up, 3, .8f, .7f, .2f, p, fade, s); Rise(InkToken.Star, up, 2, .7f, .16f, p, fade, s, GoldPalette); break;
                case "ArchiveCurator": Rise(InkToken.Page, at, 3, 1.1f, .26f, p, fade, s); break;
                case "PennyBroker": Stamp(InkToken.Page, up, .42f, p, fade * (1 - p * .5f), null, 8); Stamp(InkToken.Seal, up + new Vector2(.12f, -.08f), .22f, Mathf.Max(0, p - .15f) / .85f, fade, SealWax); Fountain(InkToken.Coin, up, 2, .7f, .5f, .17f, p, fade, s); break;
                case "StormFencer": Crackle(up, 4, .7f, p, fade, seed, ink, true, .04f); Stamp(InkToken.Bolt, up, .36f, p, fade * (1 - p), null, 0); break;
                case "TinVirtuoso": Rise(InkToken.Note, up, 3, 1.1f, .26f, p, fade, s); break;
                case "MoonProwler":
                {
                    Vector2 d = dir.sqrMagnitude > .01f ? dir.normalized : Vector2.right; float h = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                    Soft(air, InkCanvas.Arc(up, .42f, .42f, h - 120, h + 40), still ? 1 : OutCubic(Mathf.Min(1, p / .25f)), InQuad(Mathf.Max(0, p - .3f) / .7f), .05f, fade);
                    Stamp(InkToken.Moon, up + Vector2.up * .1f, .3f, p, fade * (1 - p), null, h);
                    break;
                }
                case "SupperMortar": Stamp(InkToken.Dumpling, up, .4f, p, fade * (1 - p * .6f), null, 0); Rise(InkToken.Puff, at, 2, .8f, .2f, p, fade, s, ArsenalInk.Cotton); break;
                case "LanternWeaver": Stamp(InkToken.Knot, up, .34f, p, fade * (1 - p), null, s); Crackle(up, 2, .45f, p, fade, seed, ink, false, .025f); break;
                case "HourglassStag": Stamp(InkToken.Sand, up, .4f, p, fade * (1 - p), null, still ? 0 : p * 180); Fountain(InkToken.Glint, up, 5, .7f, .8f, .1f, p, fade, s, ArsenalInk.Dust); break;
                case "RainArchivist":
                    for (int i = 0; i < 3; i++) { float q = still ? 1 : Mathf.Clamp01(p * 2.2f - i * .25f); if (q < 1) SigToken(InkToken.Drop, at + new Vector2((i - 1) * .25f, .2f + 1.2f * (1 - q)), .18f, 180, fade, true); }
                    Spray(InkToken.Drop, at + Vector2.up * .1f, 4, .4f, .1f, p, fade, s, null, .2f); break;
                case "KiteAdmiral": Spray(InkToken.Spark, up, 3, .6f, .2f, p, fade, s, null, .4f); Rise(InkToken.Page, up, 1, .8f, .2f, p, fade, s); break;
                case "SundaeDuchess": Stamp(InkToken.Dumpling, up, .42f, p, fade * (1 - p * .6f), null, 0); Spray(InkToken.Glint, up, 5, .55f, .12f, p, fade, s, ArsenalInk.Cyan); break;
                case "BonsaiClocksmith": Spray(InkToken.Leaf, up, 4, .6f, .17f, p, fade, s, null, .3f); Stamp(InkToken.Sand, up, .26f, p, fade * (1 - p), null, still ? 0 : p * 360); break;
                case "JugglingJester": Fountain(InkToken.Star, up, 4, .9f, .8f, .18f, p, fade, s, GoldPalette); Spray(InkToken.Patch, up, 3, .5f, .14f, p, fade, s); break;
                case "BellDiver": Rise(InkToken.Bubble, at, 4, 1f, .15f, p, fade, s, ArsenalInk.Water); Spray(InkToken.Drop, up, 3, .4f, .12f, p, fade, s, ArsenalInk.Water, .3f); break;
                default: Spray(style.Token, up, 4, .5f, .18f, p, fade, s); break;
            }
        }

        // ------------------------------------------------------------------ the full gauge, readable at a glance
        /// <summary>A full gauge (the second before the transformation): the hero's tokens circle over its head.</summary>
        private void PrimedCrown()
        {
            UnitState hero = sim.Champion; Vector2 over = Head(hero) + Vector2.up * .25f;
            int n = sparse ? 3 : 4;
            for (int i = 0; i < n; i++)
            {
                float a = (still ? 0 : t * 220) + i * 360f / n;
                Vector2 at = over + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .42f, Mathf.Sin(a * Mathf.Deg2Rad) * .14f);
                Token(style.Token, at, .22f + (still ? 0 : .04f * Mathf.Sin(t * 14 + i)), a, .95f, true);
            }
        }

        // ------------------------------------------------------------------ transformations' own life around the hero
        /// <summary>Ambient life for the transformations that had none of their own: each reads as that hero at a glance.</summary>
        private void AwakenAura(KitAction action, UnitState hero, Vector2 at, float body, float side)
        {
            if (sparse && action != KitAction.Casino && action != KitAction.Coronation && action != KitAction.Foreclosure) return;
            Vector2 head = at + Vector2.up * (body * .5f + .3f), feet = at - Vector2.up * body * .5f, chest = at + Vector2.up * body * .1f;
            float r;
            switch (action)
            {
                case KitAction.Treant:
                    for (int i = 0; i < 4; i++) { float a = (still ? 0 : t * 60) + i * 90; Token(i % 2 == 0 ? InkToken.Leaf : InkToken.Petal, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * (body * .55f + .25f), Mathf.Sin(a * Mathf.Deg2Rad) * .3f + .2f), .2f, a, .9f, true, 0, TreeFire); }
                    break;
                case KitAction.Casino:
                    // three dice juggled over the head, each landing on its face as the second turns
                    for (int i = 0; i < 3; i++)
                    {
                        float q = still ? .5f : Mathf.Repeat(t * 1f + i / 3f, 1);
                        Vector2 p = head + new Vector2((i - 1) * .3f, .15f + Mathf.Sin(q * Mathf.PI) * .45f);
                        Token(InkToken.Die, p, .24f, still ? 0 : q * 360, .95f, true, 1 + (Mathf.FloorToInt(t) + i * 2) % 6);
                    }
                    break;
                case KitAction.HighTide:
                    for (int i = 0; i < 4; i++) { r = Mathf.Repeat(t * 1.4f + i / 4f, 1); Token(i % 2 == 0 ? InkToken.Bubble : InkToken.Glint, feet + new Vector2(-side * (.3f + r * .9f), .15f + r * .35f), .14f, r * 90, (1 - r) * .9f, true, 0, ArsenalInk.Water); }
                    break;
                case KitAction.Meteors:
                    for (int i = 0; i < 4; i++) { r = Mathf.Repeat(t * 2.2f + i / 4f, 1); Token(i % 2 == 0 ? InkToken.Ember : InkToken.Spark, chest + new Vector2(-side * (.2f + r * 1f), .2f - r * .3f + ((i * 13) % 5 - 2) * .06f), .2f * (1 - .5f * r), r * 200, (1 - r) * .95f, true); }
                    break;
                case KitAction.Spring:
                    for (int i = 0; i < 4; i++) { r = Mathf.Repeat(t * .7f + i / 4f, 1); Token(InkToken.Petal, at + new Vector2(((i * 37) % 9 - 4) * .2f + Mathf.Sin(r * 6 + i) * .15f, 1.4f - r * 1.6f), .18f, r * 240, Mathf.Sin(r * Mathf.PI) * .95f, true); }
                    break;
                case KitAction.Kintsugi:
                    for (int i = 0; i < 4; i++) { float tw = still ? .8f : Mathf.Abs(Mathf.Sin(t * 6 + i * 1.7f)); Token(InkToken.Glint, chest + new Vector2(((i * 29) % 7 - 3) * .1f, ((i * 17) % 7 - 3) * .14f), .12f + .08f * tw, i * 45, tw, true, 0, GoldPalette); }
                    for (int i = 0; i < 3; i++) { float a = (still ? 0 : t * 80) + i * 120; Token(InkToken.Shard, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .6f, Mathf.Sin(a * Mathf.Deg2Rad) * .22f + .1f), .18f, a, .9f, true, 0, GoldPalette); }
                    break;
                case KitAction.Parade:
                    foreach (UnitState f in sim.Units)
                    {
                        if (!f.Alive || f.Faction != Faction.Companion) continue;
                        Vector2 fh = DrawnAt(f) + Vector2.up * (f.Definition.BodyHeight * .5f + .22f + Bob(f.Id) * 2);
                        Token(InkToken.Seal, fh, .2f, still ? 0 : Mathf.Sin(t * 2 + f.Id) * 8, .9f, true, 0, GoldPalette); // a little crown on every knighted friend
                    }
                    break;
                case KitAction.Duel:
                    for (int i = 0; i < 3; i++) { float a = (still ? 0 : t * 140) + i * 120; Token(InkToken.Button, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .55f, Mathf.Sin(a * Mathf.Deg2Rad) * .2f), .18f, a, .95f, true); }
                    break;
                case KitAction.Feast:
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 1.1f + i / 3f, 1); Token(InkToken.Drop, at + new Vector2(side * (body * .7f) + (i - 1) * .15f, -.1f - r * .4f), .12f, 180, (1 - r) * .85f, true, 0, ArsenalInk.Wine); }
                    break;
                case KitAction.Throne:
                    for (int i = 0; i < 4; i++) { r = Mathf.Repeat(t * .8f + i / 4f, 1); Token(InkToken.Thorn, feet + new Vector2((i - 1.5f) * .3f, .1f + r * .5f), .2f * (1 - .4f * r), 90, Mathf.Sin(r * Mathf.PI) * .9f, true); }
                    break;
                case KitAction.Train:
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 1.8f + i / 3f, 1); Token(InkToken.Puff, head + new Vector2(side * (body * .4f) - side * r * .5f, .1f + r * .6f), .2f + r * .2f, 0, (1 - r) * .85f, true, 0, Juice); }
                    break;
                case KitAction.Coronation:
                    Token(InkToken.Seal, head + Vector2.up * (.12f + Bob(2) * 2), .34f, still ? 0 : Mathf.Sin(t * 2.2f) * 6, .95f, true); // the crown it wears
                    if (!sparse) for (int i = 0; i < 2; i++) { float tw = still ? .8f : Mathf.Abs(Mathf.Sin(t * 5 + i * 2)); Token(InkToken.Glint, head + new Vector2((i * 2 - 1) * .2f, .25f), .1f + .06f * tw, 0, tw, true, 0, GlintPalette(ink)); }
                    break;
                case KitAction.Citadel:
                    for (int i = 0; i < 5; i++) { float a = (still ? 0 : t * 50) + i * 72; Token(InkToken.Petal, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * (body * .6f + .3f), Mathf.Sin(a * Mathf.Deg2Rad) * .28f), .22f, a + 90, .9f, true); }
                    break;
                case KitAction.Exhibit:
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * .6f + i / 3f, 1); Token(InkToken.Page, chest + new Vector2(Mathf.Cos((r + i / 3f) * Mathf.PI * 2) * .7f, .2f + r * .7f), .2f, Mathf.Sin(r * 9) * 25, Mathf.Sin(r * Mathf.PI) * .9f, true); }
                    break;
                case KitAction.BullMarket:
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 1.2f + i / 3f, 1); Token(InkToken.Coin, chest + new Vector2((i - 1) * .3f, .2f + r * .9f), .2f, still ? 0 : r * 360, Mathf.Sin(r * Mathf.PI) * .95f, true); }
                    break;
                case KitAction.Thunder:
                    if (!still && Mathf.Repeat(t * 3, 1) < .5f) Crackle(chest + Vector2.up * .1f, 3, body * .5f + .3f, .35f, .8f, Mathf.FloorToInt(t * 3), ink, true, .03f);
                    for (int i = 0; i < 2; i++) { float a = (still ? 0 : t * 200) + i * 180; Token(InkToken.Bolt, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .5f, Mathf.Sin(a * Mathf.Deg2Rad) * .2f + .15f), .2f, a, .9f, true); }
                    break;
                case KitAction.Symphony:
                    foreach (UnitState f in sim.Units)
                    {
                        if (!f.Alive || f.Faction != Faction.Companion) continue;
                        float beat = still ? .5f : Mathf.Repeat(t, 1); // in time with the brigade's beat (every second)
                        Token(InkToken.Note, DrawnAt(f) + Vector2.up * (f.Definition.BodyHeight * .5f + .2f + beat * .45f), .2f, Mathf.Sin(beat * 6) * 15, (1 - beat) * .95f, true);
                    }
                    break;
                case KitAction.Eclipse:
                    Token(InkToken.Moon, head + Vector2.up * (.2f + Bob(1) * 2), .32f, -20, .95f, true);
                    for (int i = 0; i < 3; i++) { float tw = still ? .7f : Mathf.Abs(Mathf.Sin(t * 4 + i * 2.1f)); Token(InkToken.Star, head + new Vector2((i - 1) * .35f, .45f + (i % 2) * .15f), .1f + .05f * tw, 0, tw * .9f, true); }
                    break;
                case KitAction.Kitchen:
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 1.3f + i / 3f, 1); Token(InkToken.Puff, head + new Vector2((i - 1) * .2f + Mathf.Sin(r * 5 + i) * .1f, r * .6f), .18f + r * .15f, 0, (1 - r) * .8f, true, 0, ArsenalInk.Cotton); }
                    break;
                case KitAction.Tapestry:
                    for (int i = 0; i < 3; i++) { float a = (still ? 0 : t * 90) + i * 120; Token(InkToken.Knot, chest + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .6f, Mathf.Sin(a * Mathf.Deg2Rad) * .22f), .2f, a, .9f, true); }
                    break;
                case KitAction.Foreclosure:
                    Token(InkToken.Sand, head + Vector2.up * (.25f + Bob(1) * 2), .42f, still ? 0 : Mathf.Min(1, (t - sim.HeroAwakenStarted) / 1.5f) * 180, .95f, true); // the great hourglass turning
                    if (!sparse) for (int i = 0; i < 4; i++) { r = Mathf.Repeat(t * 1.5f + i / 4f, 1); Token(InkToken.Glint, head + new Vector2(((i * 7) % 5 - 2) * .08f, .1f - r * .6f), .08f, 0, (1 - r) * .8f, true, 0, ArsenalInk.Dust); }
                    break;
                case KitAction.SundaeRain:
                    for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * .9f + i / 3f, 1); Token(InkToken.Glint, at + new Vector2((i - 1) * .4f + Mathf.Sin(r * 5) * .1f, 1.2f - r * 1.4f), .12f, r * 120, Mathf.Sin(r * Mathf.PI) * .9f, true, 0, ArsenalInk.Cyan); }
                    break;
            }
        }
    }
}
