using System.Collections.Generic;

namespace ArcaneBrigade
{
    public enum ComboId
    {
        ChargedBubbles, BeaconLane, SlowMotion, StitchedLetter, CometStrike, Lullaby, StaticNotes, IntoTheLight, BrainFreeze, ScaldingSteam,
        ThornDust, ScribbleBox, Airmail, Crumbs, SproutingVines, MirrorSpice,
        TeaParty, Constellation, ExpressDelivery, ClockworkBand, FrostSundae, ThornGarden, Carousel,
        GhostFleet, MidnightLibrary, StormFront, BakeryRush,
        GrandParade, MidnightClocktower, WinterFeast
    }

    public sealed class ComboDefinition { public ComboId Id; public string Name, Line; public string[] Members; }

    /// <summary>Friend combos with a move of their own (requirement 25/10/2026): named friends fielded together play a joint move,
    /// two to five of them; the bigger the group, the bigger the move. Offence and utility only (friends are immortal). The twelve
    /// pairs that were already duets keep their duet slot and trade its stat bonus for the move (DuetEffect.Combo).</summary>
    public static class Combos
    {
        private static ComboDefinition C(ComboId id, string name, string line, params string[] members) => new ComboDefinition { Id = id, Name = name, Line = line, Members = members };
        public static readonly ComboDefinition[] All =
        {
            C(ComboId.ChargedBubbles, "Charged Bubbles", "Koi's bubbles burst into lightning when Eel strikes them (3 targets)", "BubbleKoi", "VoltEel"),
            C(ComboId.BeaconLane, "Beacon Lane", "every 7s the Whale lights a lane; the Dragon dives down it", "LighthouseWhale", "KiteDragon"),
            C(ComboId.SlowMotion, "Slow Motion", "Lens marks a target; Reel's hits slow it 30% and hit 15% harder", "LensOwl", "ReelOwl"),
            C(ComboId.StitchedLetter, "Stitched Letter", "Imp's letters pin Moth's targets in place", "ThimbleMoth", "MailboxImp"),
            C(ComboId.CometStrike, "Comet Strike", "lightning falls along the Comet's trail", "CometKite", "StormKitten"),
            C(ComboId.Lullaby, "Lullaby", "an enemy struck by both falls asleep", "MoonJelly", "PillowOracle"),
            C(ComboId.StaticNotes, "Static Notes", "every 4th note arcs to 2 more enemies", "ClockworkPiper", "StaticFerret"),
            C(ComboId.IntoTheLight, "Into the Light", "enemies the Duck bounces take 30% more from the Goat", "SpringDuck", "LampGoat"),
            C(ComboId.BrainFreeze, "Brain Freeze", "chilled enemies shatter into shards on the Owl's hit", "SnowSpoon", "IceCreamOwl"),
            C(ComboId.ScaldingSteam, "Scalding Steam", "Bat leaves steam; shots through it scald (+35%)", "KettleBat", "SaffronMechanist"),
            C(ComboId.ThornDust, "Thorn Dust", "Moth's dust sprouts thorns when the Hedgehog strikes in it", "PearMoth", "SeedHedgehog"),
            C(ComboId.ScribbleBox, "Scribble Box", "a box springs out wherever the Gremlin scribbles", "CrayonGremlin", "JackInTheBox"),
            C(ComboId.Airmail, "Airmail", "parcels ride the wind and burst in an ink splash", "ShellPostman", "SailSquid"),
            C(ComboId.Crumbs, "Crumbs", "loaves the Mouse baked crumble and slow a crowd", "BakerMouse", "BreadCrab"),
            C(ComboId.SproutingVines, "Sprouting Vines", "watered seeds grow vines that hold enemies", "WateringSnail", "FirstSeed"),
            C(ComboId.MirrorSpice, "Mirror Spice", "the mirror throws a second spicy strike", "MaskPepper", "MirrorNewt"),
            C(ComboId.TeaParty, "Tea Party", "every 10s a breath of tea steam: steeped enemies take 20% more", "TeapotPilgrim", "TeaDragon", "TeacupCricket"),
            C(ComboId.Constellation, "Constellation", "three starred enemies call down a falling star", "MoonJelly", "StarSock", "OrbitMoth"),
            C(ComboId.ExpressDelivery, "Express Delivery", "every 8s a letter flies round all three, cutting through", "MailboxImp", "ShellPostman", "PaperPlanePigeon"),
            C(ComboId.ClockworkBand, "Clockwork Band", "they play in time: hits on the beat are 25% stronger", "ClockworkPiper", "RainbowBell", "TinTick"),
            C(ComboId.FrostSundae, "Frost Sundae", "three boba pearls freeze an enemy", "SnowSpoon", "IceCreamOwl", "BobaTadpole"),
            C(ComboId.ThornGarden, "Thorn Garden", "every 5s a thorn bed grows under the crowd", "FirstSeed", "WateringSnail", "SeedHedgehog"),
            C(ComboId.Carousel, "Carousel", "every 12s a carousel whirls the crowd in and flings it", "CarouselPony", "LostCarousel", "JackInTheBox"),
            C(ComboId.GhostFleet, "Ghost Fleet", "every 15s a ghost ship sails through, cannons firing", "LighthouseWhale", "AnchorSnail", "SailSquid", "ReturnCaptain"),
            C(ComboId.MidnightLibrary, "Midnight Library", "every 20 defeats a giant book slams shut on the crowd", "LensOwl", "ReelOwl", "EndlessIndex", "BookmarkGolem"),
            C(ComboId.StormFront, "Storm Front", "every 8s a storm cloud drops four bolts", "StormKitten", "VoltEel", "StaticFerret", "CometKite"),
            C(ComboId.BakeryRush, "Bakery Rush", "every 12s an oven fires a row of hot loaves", "BakerMouse", "BreadCrab", "DoughForeman", "SaffronMechanist"),
            C(ComboId.GrandParade, "Grand Parade", "every 20s a parade marches through, stunning all it meets", "CarouselPony", "LostCarousel", "JackInTheBox", "RibbonSpider", "DiceCrab"),
            C(ComboId.MidnightClocktower, "Midnight Clocktower", "every 24s the clock strikes twelve: time stops for enemies", "ClockworkPiper", "TinTick", "TopBeetle", "GrandSprout", "RainbowBell"),
            C(ComboId.WinterFeast, "Winter Feast", "every 18s an avalanche of ice cream buries a column", "IceCreamOwl", "SnowSpoon", "SundaeGolem", "TeacupCricket", "FridgeWisp"),
        };
        public static ComboDefinition Of(ComboId id) => All[(int)id];
        /// <summary>A duet's line when the pair is a combo: the move's name and what it does.</summary>
        public static string PairLine(string a, string b) { ComboDefinition c = PairFor(a, b); return c == null ? "" : c.Name + ": " + c.Line; }
        public static IEnumerable<ComboDefinition> For(string unitId) { foreach (ComboDefinition c in All) if (System.Array.IndexOf(c.Members, unitId) >= 0) yield return c; }
        public static int OwnedOf(RunSession session, string[] members) { int n = 0; foreach (string m in members) if (session.OwnsCompanionSpecies(m)) n++; return n; }
        /// <summary>How many of a combo's friends the brigade has now.</summary>
        public static int Owned(RunSession session, ComboDefinition c) { int n = 0; foreach (string m in c.Members) if (session.OwnsCompanionSpecies(m)) n++; return n; }
        /// <summary>The duet pair that is this combo, if any (its old stat bonus gives way to the move).</summary>
        public static ComboDefinition PairFor(string a, string b)
        {
            foreach (ComboDefinition c in All)
                if (c.Members.Length == 2 && (c.Members[0] == a && c.Members[1] == b || c.Members[0] == b && c.Members[1] == a)) return c;
            return null;
        }
    }
}
