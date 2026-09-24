using System.Collections.Generic;

namespace ArcaneBrigade
{
    /// <summary>How one bond rule looks when it lands: whose hero signature it borrows, the strike's shape, the Cartoon FX burst
    /// under it, the piece that flies to it, and a cartoon word for the biggest moments. Colour stays the element's.</summary>
    public readonly struct StepLook
    {
        public readonly string Hero, Burst, Word; public readonly MorphGeometry Shape; public readonly InkToken Token;
        public StepLook(string hero, MorphGeometry shape, string burst, InkToken token, string word = null) { Hero = hero; Shape = shape; Burst = burst; Token = token; Word = word; }
        public string Key => Hero + "|" + Shape + "|" + Burst;
    }

    /// <summary>Every bond moment has its own look, never another step's animation again (requirement 28/10/2026): no two
    /// rules share the same signature, shape and burst, and inside one bond every step differs in signature and burst.</summary>
    public static class RulesStepLooks
    {
        private const MorphGeometry D = MorphGeometry.Disc, C = MorphGeometry.Crescent, S = MorphGeometry.Seal;

        /// <summary>Rule -> (bond it belongs to, its look).</summary>
        public static readonly Dictionary<string, (string Bond, StepLook Look)> All = new Dictionary<string, (string, StepLook)>
        {
            // Ember: a burning burst, the inferno chain, meteors, the phoenix's dive
            ["burn-burst"] = ("ember", new StepLook("CopperCauldron", D, "hit_fire", InkToken.Ember)),
            ["inferno-burst"] = ("ember", new StepLook("CitrusMarshal", C, "hit_crit", InkToken.Ember)),
            ["ember-meteor"] = ("ember", new StepLook("CometRunner", D, "burst_fire", InkToken.Ember, "word_boom")),
            ["phoenix-dive"] = ("ember", new StepLook("SupperMortar", C, "burst_boom", InkToken.Ember)),
            // Frost: the freeze, the shatter, ice spikes, blizzard hail
            ["freeze"] = ("frost", new StepLook("MirrorKeeper", S, "hit_ice", InkToken.Shard, "word_frozen")),
            ["frozen-hit"] = ("frost", new StepLook("ChinaMarionette", C, "hit_cross", InkToken.Shard)),
            ["shatter"] = ("frost", new StepLook("ChinaMarionette", D, "hit_misc", InkToken.Shard)),
            ["frost-ice-spikes"] = ("frost", new StepLook("SundaeDuchess", D, "burst_flash", InkToken.Glint)),
            ["frost-blizzard-hail"] = ("frost", new StepLook("DrowsySeer", D, "burst_poof", InkToken.Shard)),
            // Storm: the chain is its own bolt; the discharge, the thunderbolt, the thunderstorm
            ["discharge"] = ("static-club", new StepLook("StormFencer", D, "hit_storm", InkToken.Bolt)),
            ["storm-thunderbolt"] = ("static-club", new StepLook("StormFencer", S, "burst_storm", InkToken.Bolt, "word_wham")),
            ["storm-thunderstorm"] = ("static-club", new StepLook("RainArchivist", D, "burst_flash", InkToken.Bolt)),
            // Tide: the splash, the riptide, the whirlpool and its geyser, the leviathan
            ["tide-splash"] = ("tidal", new StepLook("BellDiver", D, "burst_water", InkToken.Drop)),
            ["riptide"] = ("tidal", new StepLook("TideSinger", C, "hit_misc", InkToken.Bubble)),
            ["tide-whirlpool"] = ("tidal", new StepLook("TideSinger", D, "burst_poof", InkToken.Drop)),
            ["tide-whirlpool-burst"] = ("tidal", new StepLook("BellDiver", S, "burst_flash", InkToken.Bubble)),
            ["tide-leviathan"] = ("tidal", new StepLook("LoneJelly", C, "burst_boom", InkToken.Bubble)),
            // Windborne: the wind blade, the tornado
            ["piercing-volley"] = ("windborne", new StepLook("KiteAdmiral", C, "hit_cross", InkToken.Spear)),
            ["wind-tornado"] = ("windborne", new StepLook("KiteAdmiral", D, "burst_poof", InkToken.Puff)),
            // Garden: vines, the giant treant's slam, the grove's roots
            ["garden-vine"] = ("orchard", new StepLook("VerdantMatron", S, "hit_misc", InkToken.Thorn)),
            ["root-hit"] = ("orchard", new StepLook("VerdantMatron", C, "hit_light", InkToken.Leaf)),
            ["treant-slam"] = ("orchard", new StepLook("RootOracle", D, "burst_boom", InkToken.Leaf, "word_wham")),
            ["root-eruption"] = ("orchard", new StepLook("PetalCitadel", C, "hit_smoke", InkToken.Thorn)),
            // Venom: rot, the flask and its cloud, the plague
            ["rot"] = ("venom", new StepLook("LoneJelly", D, "hit_misc", InkToken.Bubble)),
            ["venom-flask"] = ("venom", new StepLook("PatchworkHost", D, "burst_poison", InkToken.Bubble, "word_poisoned")),
            ["venom-cloud"] = ("venom", new StepLook("SoapSkater", D, "burst_poof", InkToken.Bubble)),
            ["pandemic"] = ("venom", new StepLook("SoapSkater", S, "hit_smoke", InkToken.Bubble)),
            // Starlight: a falling star, the star shower, the starry night's stars, the comet
            ["falling-star"] = ("astral", new StepLook("JugglingJester", S, "hit_light", InkToken.Star)),
            ["star-shower"] = ("astral", new StepLook("DrowsySeer", D, "burst_flash", InkToken.Star)),
            ["star-fall"] = ("astral", new StepLook("TreasureAcorn", S, "burst_poof", InkToken.Star)),
            ["star-comet"] = ("astral", new StepLook("CometRunner", D, "burst_boom", InkToken.Star, "word_boom")),
            // Dusk: the execution, the shadow pit and its fall, the eclipse
            ["execute"] = ("dusk", new StepLook("MoonProwler", C, "hit_crit", InkToken.Fang)),
            ["shade-execute-growth"] = ("dusk", new StepLook("NightMaw", S, "hit_misc", InkToken.Fang)),
            ["dusk-pit"] = ("dusk", new StepLook("NightMaw", D, "burst_souls", InkToken.Moon)),
            ["dusk-pit-burst"] = ("dusk", new StepLook("MoonProwler", D, "burst_flash", InkToken.Fang)),
            ["eclipse"] = ("dusk", new StepLook("DrowsySeer", C, "hit_smoke", InkToken.Moon)),
            // Carnival: confetti shells, the grand finale
            ["carnival-confetti"] = ("oddity", new StepLook("JugglingJester", D, "burst_poof", InkToken.Die, "word_pow")),
            ["grand-finale"] = ("oddity", new StepLook("PatchworkHost", S, "burst_flash", InkToken.Button)),
            // Court: the seal, the decree, the royal seals of the fanfare
            ["court-seal"] = ("regal", new StepLook("IvorySovereign", S, "hit_cross", InkToken.Seal)),
            ["court-decree"] = ("regal", new StepLook("VelvetRegent", D, "burst_flash", InkToken.Seal, "word_wham")),
            ["court-royal-seal"] = ("regal", new StepLook("VelvetRegent", S, "burst_boom", InkToken.Button)),
            // Archive: the rune, pages, the giant book, the flying library
            ["rune"] = ("archive", new StepLook("ArchiveCurator", S, "hit_light", InkToken.Page)),
            ["record-page"] = ("archive", new StepLook("Postmaster", D, "hit_misc", InkToken.Envelope)),
            ["archive-giant-book"] = ("archive", new StepLook("ArchiveCurator", D, "burst_boom", InkToken.Page, "word_wham")),
            ["archive-pages"] = ("archive", new StepLook("Postmaster", C, "burst_poof", InkToken.Page)),
            // Lantern: a wick, the lighthouse's sweep
            ["wick"] = ("lantern-guild", new StepLook("LanternWeaver", D, "hit_light", InkToken.Glint)),
            ["lighthouse"] = ("lantern-guild", new StepLook("LanternKnight", C, "burst_flash", InkToken.Glint)),
            // Stone: the shove, the rock spike, the boulder, the colossus's stomp
            ["stone-shove"] = ("stone", new StepLook("ButtonCavalier", C, "hit_cross", InkToken.Sand)),
            ["stone-spike"] = ("stone", new StepLook("CitrusMarshal", S, "hit_smoke", InkToken.Sand)),
            ["stone-boulder"] = ("stone", new StepLook("CitrusMarshal", D, "burst_boom", InkToken.Sand)),
            ["colossus-stomp"] = ("stone", new StepLook("TreasureAcorn", D, "burst_flash", InkToken.Sand, "word_wham")),
            ["stone-collision"] = ("stone", new StepLook("ButtonCavalier", D, "hit_misc", InkToken.Sand)),
            // Clockwork: gears, the time stop, the pendulum
            ["clock-gears"] = ("clock", new StepLook("BonsaiClocksmith", D, "hit_misc", InkToken.Spark)),
            ["time-stop"] = ("clock", new StepLook("HourglassStag", S, "hit_light", InkToken.Sand)),
            ["clock-pendulum"] = ("clock", new StepLook("HourglassStag", C, "burst_flash", InkToken.Spark)),
            // Hearth: the hot pot, pies, the feast
            ["hearth-hot-pot"] = ("hearth", new StepLook("CopperCauldron", D, "hit_smoke", InkToken.Puff)),
            ["chef-pie"] = ("hearth", new StepLook("SupperMortar", D, "burst_poof", InkToken.Dumpling)),
            ["hearth-feast"] = ("hearth", new StepLook("SundaeDuchess", S, "burst_flash", InkToken.Dumpling)),
            // Bulwark: the shield bolt; a guard's shock wave
            ["bulwark-shield-bolt"] = ("bulwark", new StepLook("PetalCitadel", S, "hit_light", InkToken.Glint)),
            ["guard-shockwave"] = ("bulwark", new StepLook("PetalCitadel", D, "burst_boom", InkToken.Glint)),
            // Spirit: the swarm, the reaper's sweep
            ["spirit-swarm"] = ("spirit", new StepLook("MoonProwler", S, "burst_souls", InkToken.Moon)),
            ["spirit-reaper"] = ("spirit", new StepLook("NightMaw", C, "hit_crit", InkToken.Fang, "word_wham")),
            // Melody: notes, the sound wave, the war drum, the note rain
            ["melody-notes"] = ("melody", new StepLook("TinVirtuoso", D, "hit_misc", InkToken.Note)),
            ["melody-sound-wave"] = ("melody", new StepLook("TinVirtuoso", C, "burst_flash", InkToken.Note)),
            ["melody-war-drum"] = ("melody", new StepLook("ButtonCavalier", S, "burst_boom", InkToken.Note, "word_boom")),
            ["melody-note-rain"] = ("melody", new StepLook("JugglingJester", C, "hit_light", InkToken.Note)),
            // Gravity: the clump, the singularity's collapse, the falling planet
            ["gravity-clump"] = ("gravity", new StepLook("LoneJelly", S, "hit_smoke", InkToken.Moon)),
            ["singularity-collapse"] = ("gravity", new StepLook("DrowsySeer", S, "burst_souls", InkToken.Moon)),
            ["gravity-orb"] = ("gravity", new StepLook("GlassDice", D, "burst_boom", InkToken.Glint, "word_boom")),
            // minor bonds
            ["rain-letters"] = ("rain-script", new StepLook("RainArchivist", S, "hit_misc", InkToken.Page)),
            ["root-still-roots"] = ("root", new StepLook("ThornTreasurer", S, "hit_smoke", InkToken.Thorn)),
            ["cloud-drift"] = ("cloud", new StepLook("SoapSkater", C, "burst_poof", InkToken.Puff)),
            ["stone-monolith-stomp"] = ("monolith", new StepLook("PetalCitadel", C, "burst_boom", InkToken.Sand)),
            ["stone-monolith-pull"] = ("monolith", new StepLook("LoneJelly", C, "hit_smoke", InkToken.Sand)),
            ["bastion-shield-shard"] = ("bastion", new StepLook("MirrorKeeper", C, "hit_cross", InkToken.Shard)),
            ["hat-disc"] = ("hat", new StepLook("VelvetRegent", C, "hit_cross", InkToken.Button)),
            ["lucky-clover"] = ("clover", new StepLook("GlassDice", S, "hit_light", InkToken.Coin)),
            ["exhibit-statue"] = ("exhibit-hall", new StepLook("ChinaMarionette", S, "hit_smoke", InkToken.Shard)),
            ["exhibit-statue-burst"] = ("exhibit-hall", new StepLook("ChinaMarionette", D, "burst_boom", InkToken.Shard)),
            ["vine-mend"] = ("mend", new StepLook("VerdantMatron", D, "hit_light", InkToken.Leaf)),
            ["thorn-spikes"] = ("spiky-shell", new StepLook("ThornTreasurer", C, "hit_cross", InkToken.Thorn)),
            ["thorn-reflect"] = ("spiky-shell", new StepLook("ThornTreasurer", D, "hit_misc", InkToken.Thorn)),
        };

        public static bool TryGet(string rule, out StepLook look)
        {
            if (rule != null && All.TryGetValue(rule, out var entry)) { look = entry.Look; return true; }
            look = default; return false;
        }
    }
}
