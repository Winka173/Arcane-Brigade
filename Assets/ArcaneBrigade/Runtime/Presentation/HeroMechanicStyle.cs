using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum InkBrush { Brush, Flame, Wave, Bolt, Thread, Silk, Ink }
    public enum HeroGauge { Pips, Arc, Hidden }

    /// <summary>The painted identity of one hero's mechanic: palette, signature token, brush character and gauge.</summary>
    public sealed class HeroMechanicStyle
    {
        public readonly string Id; public readonly InkToken Token; public readonly InkBrush Brush; public readonly HeroGauge Gauge;
        public readonly InkPalette Palette;
        public HeroMechanicStyle(string id, InkToken token, InkBrush brush, HeroGauge gauge, InkPalette palette) { Id = id; Token = token; Brush = brush; Gauge = gauge; Palette = palette; }

        private static Color C(float r, float g, float b) => new Color(r, g, b, 1);
        private static readonly Color Cream = C(1f, .94f, .8f), Brass = C(.9f, .68f, .3f), Wax = C(.7f, .14f, .2f);
        private static InkPalette P(Color ink, Color body, Color light, Color glow, Color accent) => new InkPalette(ink, body, light, glow, accent);

        private static readonly Dictionary<string, HeroMechanicStyle> all = new Dictionary<string, HeroMechanicStyle>();
        private static void Add(string id, InkToken token, InkBrush brush, HeroGauge gauge, InkPalette palette) => all[id] = new HeroMechanicStyle(id, token, brush, gauge, palette);
        static HeroMechanicStyle()
        {
            Add("LanternKnight", InkToken.Ember, InkBrush.Flame, HeroGauge.Pips, P(C(.24f, .09f, .05f), C(1f, .68f, .28f), C(1f, .93f, .68f), C(1f, .74f, .36f), Brass));
            Add("Postmaster", InkToken.Envelope, InkBrush.Brush, HeroGauge.Pips, P(C(.22f, .06f, .08f), C(.8f, .2f, .22f), C(.98f, .9f, .74f), C(1f, .72f, .5f), Wax));
            Add("DrowsySeer", InkToken.Star, InkBrush.Wave, HeroGauge.Arc, P(C(.09f, .09f, .25f), C(.62f, .72f, 1f), C(1f, .96f, .82f), C(.72f, .66f, 1f), C(1f, .86f, .45f)));
            Add("RootOracle", InkToken.Leaf, InkBrush.Brush, HeroGauge.Arc, P(C(.06f, .15f, .1f), C(.38f, .7f, .4f), C(.93f, .97f, .72f), C(.5f, .9f, .66f), C(.55f, .36f, .2f)));
            Add("LoneJelly", InkToken.Drop, InkBrush.Wave, HeroGauge.Pips, P(C(.04f, .19f, .19f), C(.38f, .86f, .72f), C(.9f, 1f, .92f), C(.45f, .95f, .8f), C(.95f, .55f, .62f)));
            Add("GlassDice", InkToken.Die, InkBrush.Brush, HeroGauge.Pips, P(C(.26f, .05f, .1f), C(.88f, .28f, .36f), C(.99f, .93f, .84f), C(1f, .72f, .62f), C(.86f, .2f, .3f)));
            Add("TideSinger", InkToken.Note, InkBrush.Wave, HeroGauge.Pips, P(C(.04f, .1f, .25f), C(.3f, .58f, .92f), C(.88f, .97f, 1f), C(.4f, .8f, .95f), C(.95f, .88f, .7f)));
            Add("CometRunner", InkToken.Spark, InkBrush.Flame, HeroGauge.Arc, P(C(.23f, .08f, .04f), C(1f, .6f, .2f), C(1f, .95f, .78f), C(1f, .7f, .32f), C(.85f, .3f, .2f)));
            Add("MirrorKeeper", InkToken.Shard, InkBrush.Brush, HeroGauge.Pips, P(C(.1f, .12f, .24f), C(.7f, .82f, .98f), C(1f, .99f, .94f), C(.7f, .86f, 1f), Brass));
            Add("TreasureAcorn", InkToken.Coin, InkBrush.Brush, HeroGauge.Pips, P(C(.24f, .13f, .03f), C(.96f, .72f, .26f), C(1f, .95f, .74f), C(1f, .8f, .4f), C(.6f, .38f, .16f)));
            Add("VerdantMatron", InkToken.Leaf, InkBrush.Brush, HeroGauge.Arc, P(C(.05f, .17f, .08f), C(.5f, .8f, .36f), C(.96f, 1f, .76f), C(.66f, .95f, .55f), C(.98f, .82f, .5f)));
            Add("CopperCauldron", InkToken.Puff, InkBrush.Silk, HeroGauge.Arc, P(C(.42f, .2f, .1f), C(.97f, .9f, .8f), C(1f, 1f, .96f), C(1f, .8f, .55f), C(.86f, .5f, .26f)));
            Add("ChinaMarionette", InkToken.Shard, InkBrush.Thread, HeroGauge.Pips, P(C(.26f, .08f, .15f), C(.96f, .62f, .72f), C(1f, .97f, .95f), C(1f, .76f, .84f), C(.36f, .5f, .86f)));
            Add("SoapSkater", InkToken.Bubble, InkBrush.Silk, HeroGauge.Pips, P(C(.09f, .17f, .32f), C(.62f, .82f, 1f), C(1f, 1f, 1f), C(1f, .78f, .92f), C(.85f, .7f, 1f)));
            Add("VelvetRegent", InkToken.Spear, InkBrush.Silk, HeroGauge.Pips, P(C(.13f, .04f, .17f), C(.6f, .3f, .78f), C(1f, .9f, .62f), C(.74f, .5f, .95f), Brass));
            Add("ButtonCavalier", InkToken.Button, InkBrush.Brush, HeroGauge.Pips, P(C(.21f, .11f, .04f), C(.92f, .7f, .32f), C(1f, .95f, .78f), C(1f, .82f, .48f), C(.6f, .16f, .22f)));
            Add("NightMaw", InkToken.Fang, InkBrush.Brush, HeroGauge.Pips, P(C(.18f, .02f, .08f), C(.82f, .18f, .34f), C(1f, .94f, .86f), C(.95f, .32f, .46f), C(.4f, .05f, .14f)));
            Add("ThornTreasurer", InkToken.Thorn, InkBrush.Brush, HeroGauge.Pips, P(C(.04f, .14f, .08f), C(.26f, .62f, .42f), C(.86f, 1f, .82f), C(.42f, .85f, .6f), Brass));
            Add("CitrusMarshal", InkToken.Citrus, InkBrush.Brush, HeroGauge.Arc, P(C(.27f, .12f, .02f), C(1f, .64f, .18f), C(1f, .96f, .72f), C(1f, .78f, .3f), C(.46f, .7f, .26f)));
            Add("IvorySovereign", InkToken.Seal, InkBrush.Ink, HeroGauge.Pips, P(C(.13f, .04f, .17f), C(.55f, .24f, .64f), C(.99f, .94f, .82f), C(.8f, .58f, .95f), Wax));
            Add("PetalCitadel", InkToken.Petal, InkBrush.Brush, HeroGauge.Pips, P(C(.05f, .17f, .12f), C(.5f, .86f, .66f), C(1f, .96f, .86f), C(.62f, .95f, .78f), C(.98f, .62f, .7f)));
            Add("PatchworkHost", InkToken.Patch, InkBrush.Silk, HeroGauge.Pips, P(C(.23f, .04f, .11f), C(.9f, .36f, .46f), C(1f, .93f, .82f), C(1f, .6f, .66f), C(.96f, .82f, .5f)));
            Add("ArchiveCurator", InkToken.Page, InkBrush.Ink, HeroGauge.Pips, P(C(.06f, .08f, .23f), C(.34f, .48f, .88f), C(.97f, .93f, .8f), C(.5f, .66f, 1f), C(.55f, .7f, 1f)));
            Add("PennyBroker", InkToken.Coin, InkBrush.Brush, HeroGauge.Arc, P(C(.23f, .1f, .04f), C(.88f, .56f, .28f), C(1f, .95f, .8f), C(1f, .74f, .45f), C(.98f, .9f, .74f)));
            Add("StormFencer", InkToken.Bolt, InkBrush.Bolt, HeroGauge.Arc, P(C(.04f, .06f, .21f), C(.44f, .68f, 1f), C(.94f, .98f, 1f), C(.6f, .82f, 1f), C(1f, .9f, .5f)));
            Add("TinVirtuoso", InkToken.Note, InkBrush.Wave, HeroGauge.Pips, P(C(.21f, .12f, .04f), C(.95f, .74f, .3f), C(1f, .96f, .8f), C(1f, .84f, .46f), C(.4f, .62f, .6f)));
            Add("MoonProwler", InkToken.Moon, InkBrush.Ink, HeroGauge.Arc, P(C(.04f, .05f, .17f), C(.52f, .63f, .96f), C(.96f, .96f, 1f), C(.6f, .7f, 1f), C(.98f, .9f, .62f)));
            Add("SupperMortar", InkToken.Dumpling, InkBrush.Brush, HeroGauge.Pips, P(C(.25f, .1f, .04f), C(.98f, .62f, .3f), C(1f, .95f, .84f), C(1f, .74f, .4f), C(.9f, .3f, .16f)));
            Add("LanternWeaver", InkToken.Knot, InkBrush.Thread, HeroGauge.Pips, P(C(.04f, .15f, .15f), C(.36f, .84f, .72f), C(1f, .92f, .64f), C(.5f, .95f, .8f), C(1f, .8f, .4f)));
            Add("HourglassStag", InkToken.Sand, InkBrush.Ink, HeroGauge.Arc, P(C(.06f, .08f, .23f), C(.38f, .56f, .95f), C(.98f, .93f, .78f), C(.56f, .72f, 1f), C(.96f, .78f, .42f)));
            Add("RainArchivist", InkToken.Drop, InkBrush.Ink, HeroGauge.Pips, P(C(.04f, .1f, .23f), C(.44f, .66f, .95f), C(.94f, .98f, 1f), C(.55f, .8f, 1f), C(.97f, .93f, .8f)));
            // the five expansion heroes: teal sailcloth, strawberry cream, brass clockwork, circus wine, deep-sea brass
            Add("KiteAdmiral", InkToken.Spark, InkBrush.Silk, HeroGauge.Arc, P(C(.04f, .12f, .2f), C(.3f, .66f, .7f), C(.98f, .95f, .84f), C(.5f, .85f, .85f), Brass));
            Add("SundaeDuchess", InkToken.Dumpling, InkBrush.Silk, HeroGauge.Pips, P(C(.27f, .09f, .14f), C(.98f, .7f, .78f), C(1f, .97f, .9f), C(.75f, .9f, 1f), C(.92f, .78f, .5f)));
            Add("BonsaiClocksmith", InkToken.Leaf, InkBrush.Brush, HeroGauge.Arc, P(C(.12f, .1f, .04f), C(.84f, .64f, .3f), C(1f, .95f, .78f), C(.6f, .86f, .5f), C(.4f, .62f, .32f)));
            Add("JugglingJester", InkToken.Star, InkBrush.Silk, HeroGauge.Pips, P(C(.2f, .04f, .1f), C(.72f, .2f, .32f), C(1f, .94f, .82f), C(1f, .78f, .5f), C(.98f, .84f, .4f)));
            Add("BellDiver", InkToken.Bubble, InkBrush.Wave, HeroGauge.Arc, P(C(.03f, .1f, .16f), C(.2f, .52f, .6f), C(.9f, .98f, 1f), C(.45f, .82f, .9f), C(.86f, .64f, .3f)));
        }
        public static HeroMechanicStyle For(string id)
        {
            if (id != null && all.TryGetValue(id, out var style)) return style;
            var d = HeroIdentities.For(id); Color body = d != null ? d.Color : Brass;
            return new HeroMechanicStyle(id ?? "", InkToken.Spark, InkBrush.Brush, HeroGauge.Pips, P(C(.1f, .08f, .14f), body, Cream, body, Brass));
        }
        public static IEnumerable<HeroMechanicStyle> All => all.Values;

        /// <summary>Supper Mortar's recipe colours the shells: dumplings warm, pepper ember red, sorbet ice.</summary>
        public InkPalette PaletteFor(int approach)
        {
            if (Id != "SupperMortar" || approach == 0) return Palette;
            var p = Palette;
            if (approach == 1) { p.Body = C(.95f, .32f, .18f); p.Glow = C(1f, .5f, .26f); }
            else { p.Body = C(.62f, .84f, 1f); p.Glow = C(.72f, .9f, 1f); p.Ink = C(.05f, .12f, .26f); }
            return p;
        }
        /// <summary>The thing an identity bolt looks like in flight (envelopes, notes, shards...), or Glint.</summary>
        public InkToken Missile => Id switch
        {
            "Postmaster" => InkToken.Envelope, "TideSinger" => InkToken.Note, "MirrorKeeper" => InkToken.Shard, "GlassDice" => InkToken.Die,
            "LoneJelly" => InkToken.Drop, "RainArchivist" => InkToken.Drop, "HourglassStag" => InkToken.Sand, "ArchiveCurator" => InkToken.Page,
            _ => Token
        };
    }
}
