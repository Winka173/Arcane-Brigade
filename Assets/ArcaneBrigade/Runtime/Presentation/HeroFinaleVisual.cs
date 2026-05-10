using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Each hero's own finale as its transformation ends (owner 24/10/2026), and each hero's own tell when its gauge
    /// is full. A finale arrives during the strike's anticipation (a giant envelope falling, three dice tumbling, roots
    /// creeping in, a bolt warning) and bursts as it lands. Painted tokens and brushed light only.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private static readonly InkPalette Brassy = new InkPalette(new Color(.24f, .15f, .06f), new Color(.86f, .66f, .3f), new Color(1, .94f, .7f), new Color(1, .85f, .45f), new Color(.6f, .4f, .16f));

        /// <summary>A token flying from <paramref name="from"/> to <paramref name="to"/> over q, arcing by <paramref name="lift"/>.</summary>
        private Vector2 Fly(Vector2 from, Vector2 to, float q, float lift) => Vector2.Lerp(from, to, q) + Vector2.up * Mathf.Sin(q * Mathf.PI) * lift;

        /// <summary>The finale on its way (q: 0..1 of its anticipation).</summary>
        private void FinaleArrive(Vector2 c, float q)
        {
            UnitState hero = sim.Champion; Vector2 h = DrawnAt(hero) + Vector2.up * .3f;
            float e = still ? 1 : OutCubic(q), fall = still ? 1 : InQuad(q), a = Mathf.Clamp01(q * 3);
            Vector2 sky = c + Vector2.up * (2.8f * (1 - fall) + .3f);
            switch (style.Id)
            {
                case "LanternKnight":
                    Light(BeamSprite(), c + Vector2.up * 4.5f, .6f + 1.6f * (1 - e), 4.6f, 180, A(new Color(1, .9f, .6f), .7f * a * strength));
                    break;
                case "Postmaster": Token(InkToken.Envelope, sky, .9f, still ? 0 : q * 200, a, true); break;
                case "DrowsySeer":
                    for (int i = 0; i < 3; i++) { Vector2 from = c + Dir(90 + i * 120) * 2.2f; Token(i == 0 ? InkToken.Moon : InkToken.Star, Vector2.Lerp(from, c, e), .3f, q * 180, a, true); Thread(from, Vector2.Lerp(from, c, e), .4f * a, i); }
                    break;
                case "RootOracle":
                    for (int i = 0; i < 3; i++) { Vector2 from = h + Dir(Mathf.Atan2(c.y - h.y, c.x - h.x) * Mathf.Rad2Deg + (i - 1) * 25) * .4f; Root(InkCanvas.Line(from, Vector2.Lerp(from, c, e), .12f, 6, false, i), .12f, a, TreeFire, i); }
                    break;
                case "LoneJelly":
                    for (int i = 0; i < 2; i++) { Vector2 from = c + new Vector2(i == 0 ? -1.8f : 1.8f, .2f); Token(InkToken.Drop, Fly(from, c, e, 1.2f), .4f, 180, a, true); }
                    break;
                case "GlassDice":
                    for (int i = 0; i < 3; i++) Token(InkToken.Die, c + new Vector2((i - 1) * .45f, .2f + 2.6f * (1 - Mathf.Clamp01(fall * 1.2f - i * .1f))), .4f, still ? 0 : q * 540 + i * 60, a, true, 1 + (i * 2 + Mathf.FloorToInt(t * 12)) % 6);
                    break;
                case "TideSinger":
                {
                    Vector2 d = (c - h).sqrMagnitude > .01f ? (c - h).normalized : Vector2.right; Vector2 crest = Vector2.Lerp(h - d * .5f, c, e);
                    Soft(air, InkCanvas.Arc(crest + Vector2.up * .2f, .9f, .6f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 80, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg + 120), 1, 0, .12f, a);
                    Token(InkToken.Note, crest + Vector2.up * .9f, .3f, 0, a, true);
                    break;
                }
                case "CometRunner": Soft(air, InkCanvas.Line(h, Vector2.Lerp(h, c, e), .05f, 10), 1, Mathf.Max(0, e - .5f), .12f, a); Token(InkToken.Spark, Vector2.Lerp(h, c, e), .45f, q * 720, a, true); break;
                case "MirrorKeeper":
                    for (int i = 0; i < 4; i++) { Vector2 from = c + Dir(45 + i * 90) * 2f; Token(InkToken.Shard, Vector2.Lerp(from, c, e), .35f, 45 + i * 90 + q * 360, a, true); }
                    break;
                case "TreasureAcorn":
                    for (int i = 0; i < 5; i++) { float an = i * 72 + (still ? 0 : q * 300); Token(InkToken.Coin, c + Dir(an) * Mathf.Lerp(1.4f, .1f, e) + Vector2.up * (1 - e) * .6f, .25f, an, a, true); }
                    break;
                case "VerdantMatron": Token(InkToken.Petal, c + Vector2.up * .25f, .2f + .5f * e, 0, a, true); Token(InkToken.Leaf, c + new Vector2(-.25f, .1f), .25f * e, -40, a, true); Token(InkToken.Leaf, c + new Vector2(.25f, .1f), .25f * e, 40, a, true); break;
                case "CopperCauldron": for (int i = 0; i < 3; i++) Token(InkToken.Puff, c + new Vector2((i - 1) * .3f, .1f + .2f * e), .25f + .3f * e, 0, a * .8f, true, 0, ArsenalInk.Cotton); break;
                case "ChinaMarionette": Crackle(c + Vector2.up * .2f, 6, 1.4f * (1 - e) + .2f, .5f, a, 7, GoldPalette, true, .04f); break;
                case "SoapSkater": Token(InkToken.Bubble, c + Vector2.up * .4f, .25f + .75f * e, 0, a, true); break;
                case "VelvetRegent": Token(InkToken.Spear, sky, .95f, -90, a, true, 0, GoldPalette); break;
                case "ButtonCavalier": Soft(air, InkCanvas.Line(h, Vector2.Lerp(h, c, e)), 1, 0, .06f, a); Token(InkToken.Button, Vector2.Lerp(h, c, e), .3f, q * 360, a, true); break;
                case "NightMaw":
                {
                    float open = Mathf.Lerp(.2f, 1.1f, e);
                    Token(InkToken.Fang, c + Vector2.up * (.25f + open), .9f, 180, a, true); Token(InkToken.Fang, c - Vector2.up * (open - .25f), .9f, 0, a, true);
                    break;
                }
                case "ThornTreasurer": for (int i = 0; i < 6; i++) { float an = i * 60; Token(InkToken.Thorn, c + Vector2.up * .2f + Dir(an) * Mathf.Lerp(1.6f, .3f, e), .35f, an + 180, a, true); } break;
                case "CitrusMarshal": for (int i = 0; i < 3; i++) Token(InkToken.Citrus, Vector2.Lerp(h, c, Mathf.Clamp01(e * 1.2f - i * .1f)), .35f, q * 720 + i * 90, a, true); break;
                case "IvorySovereign": Place(MechanicInk.Token(InkToken.Page, ink), c + Vector2.up * 1.3f, new Vector2(.3f + 1f * e, .7f), 0, new Color(1, 1, 1, a * strength), true); break;
                case "PetalCitadel": for (int i = 0; i < 6; i++) { float an = i * 60 + (still ? 0 : (1 - e) * 300); Token(InkToken.Petal, c + Vector2.up * .2f + Dir(an) * Mathf.Lerp(1.5f, .2f, e), .3f, an, a, true); } break;
                case "PatchworkHost": for (int i = 0; i < 4; i++) Token(InkToken.Glint, c + new Vector2((i - 1.5f) * .3f, 2.2f * (1 - e) + .2f), .2f, 0, a, true, 0, GoldPalette); Token(InkToken.Star, c + Vector2.up * .3f, .3f * e, 0, a, true, 0, GoldPalette); break;
                case "ArchiveCurator": for (int i = 0; i < 4; i++) Token(InkToken.Page, Fly(h, c, Mathf.Clamp01(e * 1.3f - i * .1f), .8f + i * .2f), .32f, q * 300 + i * 40, a, true); break;
                case "PennyBroker": Token(InkToken.Page, sky, .7f, Mathf.Sin(q * 12) * 20, a, true); break;
                case "StormFencer": if (still || Mathf.Repeat(t * 10, 1) < .6f) Token(InkToken.Bolt, c + Vector2.up * 1.6f, .5f, 0, a, true); break;
                case "TinVirtuoso": for (int i = 0; i < 5; i++) { float an = i * 72 + 90; Token(InkToken.Note, c + Vector2.up * .3f + Dir(an) * Mathf.Lerp(1.6f, .2f, e), .3f, 0, a, true); } break;
                case "MoonProwler": Token(InkToken.Moon, sky, .7f + .2f * e, -20 + q * 40, a, true); break;
                case "SupperMortar": Token(InkToken.Dumpling, Fly(h, c, e, 2f), .7f, q * 200, a, true); break;
                case "LanternWeaver": for (int i = 0; i < 4; i++) { Vector2 from = c + Dir(i * 90 + 45) * 2.2f; Soft(air, InkCanvas.Line(from, Vector2.Lerp(from, c, e), .04f, 5), 1, 0, .04f, a); } break;
                case "HourglassStag": Token(InkToken.Sand, c + Vector2.up * 1.2f, .7f, still ? 0 : q * 180, a, true); break;
                case "RainArchivist": for (int i = 0; i < 5; i++) { float r = Mathf.Repeat(q * 3 + i / 5f, 1); Token(InkToken.Drop, c + new Vector2((i - 2) * .3f, 2.2f * (1 - r)), .2f, 180, a, true); } break;
                case "KiteAdmiral": for (int i = 0; i < 2; i++) Token(InkToken.Spark, Vector2.Lerp(c + new Vector2(i == 0 ? -2.2f : 2.2f, 2.2f), c, e), .45f, i == 0 ? -45 : 45, a, true); break;
                case "SundaeDuchess": Token(InkToken.Dumpling, sky, .95f, 0, a, true, 0, ArsenalInk.Candy); break;
                case "BonsaiClocksmith": Token(InkToken.Sand, c + Vector2.up * .6f, .6f, still ? 0 : q * 1080, a, true); break;
                case "JugglingJester": for (int i = 0; i < 3; i++) Token(InkToken.Star, c + new Vector2((i - 1) * .5f, .2f + 3f * (1 - Mathf.Clamp01(fall * 1.3f - i * .12f))), .35f, q * 400, a, true, 0, GoldPalette); break;
                case "BellDiver": for (int i = 0; i < 5; i++) { float r = Mathf.Repeat(q * 2 + i / 5f, 1); Token(InkToken.Bubble, c + new Vector2((i - 2) * .15f, r * 1.4f), .12f + .1f * r, 0, a * (1 - r), true, 0, ArsenalInk.Water); } break;
                default: Token(style.Token, sky, .8f, q * 360, a, true); break;
            }
            Gather(c, q);
        }

        /// <summary>The finale bursting where it lands (p: 0..1 after it lands).</summary>
        private void FinaleBurst(HeroIdentityGlyph g, Vector2 at, float p, float fade)
        {
            float s = Seed(g); Vector2 up = at + Vector2.up * .25f;
            sigScale = SignatureSize * 1.6f; sigReach = 1.6f;
            switch (style.Id)
            {
                case "LanternKnight": Fountain(InkToken.Ember, up, 8, 1.6f, 1.6f, .2f, p, fade, s); Spray(InkToken.Spark, up, 6, 1f, .2f, p, fade, s); break;
                case "Postmaster": Stamp(InkToken.Seal, up, .6f, p, fade, SealWax); Spray(InkToken.Envelope, up, 8, 1.2f, .26f, p, fade, s, null, .6f); break;
                case "DrowsySeer": Stamp(InkToken.Moon, up, .5f, p, fade * (1 - p)); Spray(InkToken.Star, up, 9, 1.2f, .2f, p, fade, s, null, .5f); break;
                case "RootOracle":
                    for (int i = 0; i < 7; i++) { float an = i * 360f / 7 + s, q = still ? .7f : Mathf.Sin(Mathf.Min(1, p * 1.5f) * Mathf.PI); SigToken(InkToken.Thorn, at + Dir(an) * .7f + Vector2.up * (.1f + .4f * q), .3f * (.4f + .6f * q), 90, fade, true); }
                    Spray(InkToken.Leaf, up, 6, 1f, .16f, p, fade, s, null, .4f); break;
                case "LoneJelly": Stamp(InkToken.Drop, up, .5f, p, fade * (1 - p), null, 180); Spray(InkToken.Drop, up, 10, 1.1f, .16f, p, fade, s, null, .5f); break;
                case "GlassDice":
                    for (int i = 0; i < 3; i++) SigToken(InkToken.Die, at + new Vector2((i - 1) * .4f, .2f + (still ? 0 : Mathf.Abs(Mathf.Sin(p * Mathf.PI * 2)) * (1 - p) * .5f)), .3f, 0, fade, true, 6);
                    Fountain(InkToken.Coin, up, 7, 1.3f, 1.3f, .2f, p, fade, s); break;
                case "TideSinger": Rise(InkToken.Bubble, at, 6, 1.2f, .16f, p, fade, s, ArsenalInk.Water); Spray(InkToken.Drop, up, 8, 1.1f, .14f, p, fade, s, ArsenalInk.Water, .5f); Rise(InkToken.Note, up, 2, 1.2f, .26f, p, fade, s); break;
                case "CometRunner": Spray(InkToken.Spark, up, 8, 1.3f, .24f, p, fade, s); Spray(InkToken.Ember, up, 6, .9f, .18f, p, fade, s + 30, null, .4f); break;
                case "MirrorKeeper": Spray(InkToken.Shard, up, 10, 1.3f, .22f, p, fade, s); Stamp(InkToken.Glint, up, .6f, p, fade * (1 - p), GlintPalette(ink), 0); break;
                case "TreasureAcorn": Fountain(InkToken.Coin, up, 8, 1.5f, 1.5f, .22f, p, fade, s); break;
                case "VerdantMatron":
                {
                    float open = still ? 1 : OutBack(Mathf.Min(1, p / .35f));
                    for (int i = 0; i < 8; i++) { float an = s + i * 45; SigToken(InkToken.Petal, up + Dir(an) * .55f * open, .3f * open, an - 90, fade * (1 - Smooth(.6f, 1, p)), true); }
                    Spray(InkToken.Leaf, up, 6, 1.1f, .15f, p, fade, s + 20, null, .4f); break;
                }
                case "CopperCauldron": Rise(InkToken.Puff, at, 6, 1.6f, .32f, p, fade, s, ArsenalInk.Cotton); SigToken(InkToken.Seal, up + Vector2.up * (still ? .8f : 2.2f * 4 * p * (1 - p)), .3f, p * 720, fade, true, 0, Brassy); break;
                case "ChinaMarionette": Crackle(up, 7, .9f, p, fade, g.Seed, GoldPalette, true, .04f); Spray(InkToken.Shard, up, 9, 1.2f, .2f, p, fade, s, GoldPalette); break;
                case "SoapSkater": Spray(InkToken.Bubble, up, 10, 1.2f, .15f, p, fade, s); break;
                case "VelvetRegent": Stamp(InkToken.Seal, up, .6f, p, fade, GoldPalette); Spray(InkToken.Glint, up, 8, 1.1f, .16f, p, fade, s, GoldPalette); break;
                case "ButtonCavalier": Crackle(up, 3, .9f, p, fade, g.Seed, GoldPalette, false, .04f); Fountain(InkToken.Button, up, 5, 1.1f, 1.1f, .22f, p, fade, s); break;
                case "NightMaw":
                {
                    float shut = still ? .1f : Mathf.Lerp(.9f, .08f, OutBack(Mathf.Min(1, p / .2f)));
                    SigToken(InkToken.Fang, up + Vector2.up * shut, .55f, 180, fade, true); SigToken(InkToken.Fang, up - Vector2.up * shut, .55f, 0, fade, true);
                    Rise(InkToken.Puff, at, 4, 1.1f, .3f, p, fade, s, ArsenalInk.Violet); break;
                }
                case "ThornTreasurer": Spray(InkToken.Thorn, up, 8, 1.3f, .24f, p, fade, s); Fountain(InkToken.Coin, up, 4, 1.1f, 1f, .18f, p, fade, s); break;
                case "CitrusMarshal": Stamp(InkToken.Citrus, up, .5f, p, fade * (1 - p * .6f), null, s); Fountain(InkToken.Drop, up, 8, 1.2f, 1.2f, .15f, p, fade, s, Juice); break;
                case "IvorySovereign": Stamp(InkToken.Seal, up, .75f, p, fade); Spray(InkToken.Glint, up, 8, 1.2f, .16f, p, fade, s, GlintPalette(ink)); break;
                case "PetalCitadel": Spray(InkToken.Petal, up, 10, 1.4f, .24f, p, fade, s); break;
                case "PatchworkHost": Fountain(InkToken.Patch, up, 6, 1.3f, 1.4f, .22f, p, fade, s); Fountain(InkToken.Star, up, 5, 1.6f, 1.1f, .18f, p, fade, s + 40, GoldPalette); break;
                case "ArchiveCurator": Rise(InkToken.Page, at, 6, 1.5f, .3f, p, fade, s); Spray(InkToken.Page, up, 5, 1f, .2f, p, fade, s, null, .5f); break;
                case "PennyBroker": Stamp(InkToken.Seal, up, .45f, p, fade, SealWax); Fountain(InkToken.Coin, up, 8, 1.4f, 1.4f, .2f, p, fade, s); break;
                case "StormFencer":
                {
                    InkPalette was = ink; float strike = still ? 1 : OutCubic(Mathf.Min(1, p / .15f));
                    Soft(air, InkCanvas.Line(at + Vector2.up * 4.5f, Vector2.Lerp(at + Vector2.up * 4.5f, at, strike), .35f, 0, true, g.Seed + Mathf.FloorToInt(t * 18)), 1, Mathf.Max(0, p * 1.4f - .5f), .09f, fade);
                    ink = was; Crackle(up, 6, 1.1f, p, fade, g.Seed, ink, true, .045f); break;
                }
                case "TinVirtuoso": Rise(InkToken.Note, up, 6, 1.6f, .3f, p, fade, s); Spray(InkToken.Note, up, 5, 1f, .2f, p, fade, s + 30); break;
                case "MoonProwler":
                    for (int k = 0; k < 2; k++) Soft(air, InkCanvas.Arc(up, .7f + k * .25f, .7f + k * .25f, s + k * 90, s + k * 90 + 150), still ? 1 : OutCubic(Mathf.Min(1, p / .3f)), InQuad(Mathf.Max(0, p - .35f) / .65f), .06f, fade);
                    Stamp(InkToken.Moon, up, .45f, p, fade * (1 - p)); Spray(InkToken.Star, up, 6, 1f, .16f, p, fade, s); break;
                case "SupperMortar": Stamp(InkToken.Dumpling, up, .6f, p, fade * (1 - p * .5f)); Rise(InkToken.Puff, at, 4, 1.2f, .26f, p, fade, s, ArsenalInk.Cotton); Spray(InkToken.Drop, up, 7, 1f, .14f, p, fade, s, ArsenalInk.Cream, .4f); break;
                case "LanternWeaver": Stamp(InkToken.Knot, up, .5f, p, fade * (1 - p)); Spray(InkToken.Ember, up, 8, 1.2f, .2f, p, fade, s, null, .5f); break;
                case "HourglassStag": Stamp(InkToken.Sand, up, .55f, p, fade * (1 - p), null, still ? 0 : p * 180); Fountain(InkToken.Glint, up, 10, 1.3f, 1.5f, .13f, p, fade, s, ArsenalInk.Dust); break;
                case "RainArchivist": Spray(InkToken.Drop, at + Vector2.up * .1f, 10, 1.2f, .15f, p, fade, s, null, .3f); Crackle(up + Vector2.up * .4f, 3, .8f, p, fade, g.Seed, ink, true, .03f); break;
                case "KiteAdmiral": Spray(InkToken.Spark, up, 6, 1.2f, .24f, p, fade, s, null, .6f); Rise(InkToken.Page, up, 3, 1.2f, .22f, p, fade, s); break;
                case "SundaeDuchess": Stamp(InkToken.Dumpling, up, .6f, p, fade * (1 - p * .6f), ArsenalInk.Candy, 0); Spray(InkToken.Glint, up, 9, 1.2f, .15f, p, fade, s, ArsenalInk.Cyan); Spray(InkToken.Drop, up, 6, .8f, .14f, p, fade, s + 30, ArsenalInk.Candy, .4f); break;
                case "BonsaiClocksmith": Spray(InkToken.Leaf, up, 10, 1.3f, .2f, p, fade, s, null, .5f); Stamp(InkToken.Sand, up, .4f, p, fade * (1 - p), null, still ? 0 : p * 720); break;
                case "JugglingJester": Fountain(InkToken.Star, up, 7, 1.5f, 1.5f, .22f, p, fade, s, GoldPalette); Spray(InkToken.Patch, up, 6, 1f, .18f, p, fade, s); break;
                case "BellDiver": Fountain(InkToken.Drop, up, 9, 2f, .8f, .16f, p, fade, s, ArsenalInk.Water); Rise(InkToken.Bubble, at, 5, 1.4f, .16f, p, fade, s, ArsenalInk.Water); break;
                default: Spray(style.Token, up, 10, 1.2f, .22f, p, fade, s); break;
            }
        }

        /// <summary>Each hero's tell when its gauge is full (the second before it transforms), beside the circling crown.</summary>
        private void PrimedTell()
        {
            if (!sim.HeroAwakenPrimed || sparse) return;
            UnitState hero = sim.Champion; Vector2 at = DrawnAt(hero), facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;
            float body = hero.Definition.BodyHeight, side = facing.x < -.05f ? -1 : 1, r;
            Vector2 head = at + Vector2.up * (body * .5f + .1f), chest = at + Vector2.up * body * .08f, feet = at - Vector2.up * body * .5f;
            float jitter = still ? 0 : Mathf.Sin(t * 40) * .03f;
            switch (style.Id)
            {
                case "Postmaster": Token(InkToken.Envelope, chest + new Vector2(-side * .45f + jitter, .1f), .24f, Mathf.Sin(t * 30) * 10, .95f, true, 0, ArsenalInk.Leather); break;
                case "LoneJelly": for (int i = 0; i < 4; i++) { r = Mathf.Repeat(t * 2 + i / 4f, 1); Token(InkToken.Drop, chest + Dir(i * 90 + 45) * (.4f + r * .3f), .1f, i * 90, (1 - r) * .9f, true); } break;
                case "CometRunner": for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 3 + i / 3f, 1); Token(InkToken.Ember, feet + new Vector2(-side * (.2f + r * .5f), .1f + r * .2f), .14f, 0, (1 - r) * .9f, true); } break;
                case "MirrorKeeper": for (int i = 0; i < 3; i++) { float tw = Mathf.Abs(Mathf.Sin(t * 9 + i * 2)); Token(InkToken.Glint, chest + Dir(i * 120 + t * 90) * .55f, .1f + .1f * tw, 0, tw, true, 0, GlintPalette(ink)); } break;
                case "TreasureAcorn": Token(InkToken.Coin, head + new Vector2(-side * .4f, .1f), .22f, still ? 0 : t * 720, .95f, true); break;
                case "CopperCauldron": r = still ? .5f : Mathf.Repeat(t * 4, 1); Token(InkToken.Seal, head + Vector2.up * (.05f + 4 * r * (1 - r) * .2f), .22f, Mathf.Sin(t * 30) * 15, .95f, true, 0, Brassy); break;
                case "ChinaMarionette": Crackle(chest, 3, .35f, .4f, .9f, Mathf.FloorToInt(t * 5), GoldPalette, true, .025f); break;
                case "SoapSkater": for (int i = 0; i < 4; i++) { float an = (still ? 0 : t * 120) + i * 90; Token(InkToken.Bubble, feet + new Vector2(Mathf.Cos(an * Mathf.Deg2Rad) * .5f, .1f + Mathf.Sin(an * Mathf.Deg2Rad) * .12f), .14f, 0, .85f, true); } break;
                case "ButtonCavalier": r = still ? .5f : Mathf.Repeat(t * 1.5f, 1); Token(InkToken.Button, head + Vector2.up * (.05f + r * .4f), .2f, r * 360, (1 - r) * .95f, true); break;
                case "ThornTreasurer": for (int i = 0; i < 5; i++) Token(InkToken.Thorn, head + Dir(30 + i * 30) * .35f, .16f, 30 + i * 30, .95f, true); break;
                case "CitrusMarshal": Token(InkToken.Citrus, head + new Vector2(-side * .4f, .1f), .22f, still ? 0 : t * 360, .95f, true); break;
                case "PetalCitadel": for (int i = 0; i < 4; i++) { r = Mathf.Repeat(t * 1.2f + i / 4f, 1); Token(InkToken.Petal, feet + new Vector2((i - 1.5f) * .3f, r * .8f), .16f, r * 200, Mathf.Sin(r * Mathf.PI), true); } break;
                case "PatchworkHost": if (sim.HeroIdentityLead != null && sim.HeroIdentityLead.Alive) Token(InkToken.Star, Head(sim.HeroIdentityLead) + Vector2.up * .2f, .3f + jitter, t * 90, 1, true, 0, GoldPalette); break;
                case "ArchiveCurator": for (int i = 0; i < 3; i++) Token(InkToken.Page, chest + new Vector2(-side * .45f, .1f + i * .05f), .2f, Mathf.Sin(t * 25 + i) * 25, .9f, true); break;
                case "PennyBroker": Fountain(InkToken.Coin, chest + new Vector2(-side * .4f, 0), 2, .5f, .4f, .14f, Mathf.Repeat(t * 1.5f, 1), 1, 0); break;
                case "TinVirtuoso": for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 3 + i / 3f, 1); Token(InkToken.Note, head + new Vector2((i - 1) * .25f, r * .5f), .14f, 0, (1 - r), true); } break;
                case "MoonProwler": Token(InkToken.Moon, head + Vector2.up * .3f, .3f, -20, Mathf.Repeat(t * 3, 1) < .5f ? 1 : .5f, true); break;
                case "SupperMortar": for (int i = 0; i < 4; i++) { r = Mathf.Repeat(t * 2 + i / 4f, 1); Token(InkToken.Puff, head + new Vector2((i - 1.5f) * .15f, r * .6f), .15f + r * .15f, 0, (1 - r) * .8f, true, 0, ArsenalInk.Cotton); } break;
                case "LanternWeaver": { float an = still ? 30 : t * 500; Token(InkToken.Shard, chest + Dir(an) * .6f, .16f, an + 90, .95f, true); break; }
                case "HourglassStag": Token(InkToken.Sand, head + Vector2.up * .35f, .28f, still ? 0 : t * 360, .95f, true); break;
                case "RainArchivist": for (int i = 0; i < 5; i++) { r = Mathf.Repeat(t * 3 + i / 5f, 1); Token(InkToken.Drop, head + new Vector2((i - 2) * .15f, .6f - r * .6f), .09f, 180, Mathf.Sin(r * Mathf.PI), true); } break;
                case "KiteAdmiral": Token(InkToken.Spark, head + new Vector2(-side * .55f, .7f + jitter * 3), .26f, Mathf.Sin(t * 12) * 25, .95f, true); break;
                case "SundaeDuchess": for (int i = 0; i < 3; i++) { r = Mathf.Repeat(t * 2.2f + i / 3f, 1); Token(InkToken.Drop, head + new Vector2((i - 1) * .2f, -r * .5f), .1f, 180, (1 - r), true, 0, ArsenalInk.Candy); } break;
                case "BonsaiClocksmith": Token(InkToken.Sand, chest - new Vector2(side * (body * .35f + .15f), -.2f), .24f, still ? 0 : -t * 720, .95f, true); break;
                case "JugglingJester": for (int i = 0; i < 3; i++) { float q = still ? i / 3f : Mathf.Repeat(t * 2 + i / 3f, 1); Token(InkToken.Star, head + new Vector2(Mathf.Cos(q * Mathf.PI * 2) * .3f, .2f + Mathf.Abs(Mathf.Sin(q * Mathf.PI * 2)) * .6f), .15f, q * 360, .95f, true, 0, GoldPalette); } break;
                case "BellDiver": for (int i = 0; i < 5; i++) { r = Mathf.Repeat(t * 2.5f + i / 5f, 1); Token(InkToken.Bubble, head + new Vector2((i - 2) * .1f, r * .7f), .08f + .06f * r, 0, (1 - r), true, 0, ArsenalInk.Water); } break;
                case "RootOracle": case "LanternKnight": case "DrowsySeer": case "GlassDice": case "TideSinger": case "VerdantMatron":
                case "VelvetRegent": case "NightMaw": case "IvorySovereign": case "StormFencer": break; // their full-gauge tells live in HeroFlair
                default: Token(style.Token, head + Vector2.up * .3f, .26f, t * 90, .95f, true); break;
            }
        }
    }
}
