using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The six maps of the Waiting Realm (requirement 25/10/2026, Docs/Map_Mechanics_2026-10-25.md). Act I: Waiting Room
    /// w1-4, Post Office w5-8, Glasshouse w9-11, Lamp Hall w12-14, Theatre w15-17, Archive Vault w18-20; endless repeats this
    /// 20-wave cycle (StageTransfer.ChangesMap follows it). New maps are appended so the first four keep their ints
    /// (RunCatalog.MapElites and the families are indexed by them); the order on the road is MapIdentity.Order.</summary>
    public enum MapTheme { WaitingRoom, PostOffice, LampHall, ArchiveVault, Glasshouse, Theatre }

    /// <summary>Each map's identity: its family of enemies (mostly they walk in, plus a few carry-overs), its own elite affix,
    /// its living elements (MapEnvironmentCombat) and one short guide line per element.</summary>
    public static class MapIdentity
    {
        /// <summary>Balance diagnosis only (bot sweeps): "-phased-off=pool,env,contact" switches parts of phase D off.</summary>
        public static bool Off(string part)
        {
            if (offParts == null)
            {
                offParts = "";
                foreach (string arg in Environment.GetCommandLineArgs()) if (arg.StartsWith("-phased-off=", StringComparison.Ordinal)) offParts = "," + arg.Substring(12) + ",";
            }
            return offParts.Length > 0 && offParts.Contains("," + part + ",");
        }
        private static string offParts;
        // requirement 25/10/2026: six maps of uneven length on the road; the story bosses (5/10/15/20) are not map boundaries
        /// <summary>The maps in the order they are met.</summary>
        public static readonly MapTheme[] Order = { MapTheme.WaitingRoom, MapTheme.PostOffice, MapTheme.Glasshouse, MapTheme.LampHall, MapTheme.Theatre, MapTheme.ArchiveVault };
        /// <summary>The first wave of each map in Order, inside one Cycle.</summary>
        private static readonly int[] Starts = { 1, 5, 9, 12, 15, 18 };
        /// <summary>Waves before the maps repeat (Act I's length; endless cycles again).</summary>
        public const int Cycle = 20;
        private static int InCycle(int wave) => (Mathf.Max(1, wave) - 1) % Cycle + 1;
        /// <summary>The wave's place in Order (0 = the Waiting Room).</summary>
        public static int Leg(int wave)
        {
            int w = InCycle(wave), leg = Starts.Length - 1;
            while (leg > 0 && Starts[leg] > w) leg--;
            return leg;
        }
        public static MapTheme For(int wave) => Order[Leg(wave)];
        /// <summary>The first wave of the map this wave belongs to.</summary>
        public static int FirstWave(int wave) => Mathf.Max(1, wave) - (InCycle(wave) - Starts[Leg(wave)]);
        /// <summary>The last wave of the map this wave belongs to.</summary>
        public static int LastWave(int wave)
        {
            int leg = Leg(wave);
            return FirstWave(wave) + (leg + 1 < Starts.Length ? Starts[leg + 1] : Cycle + 1) - Starts[leg] - 1;
        }
        /// <summary>The map met just before this one (the Archive Vault before the Waiting Room).</summary>
        public static MapTheme Previous(MapTheme map) => Order[(Array.IndexOf(Order, map) + Order.Length - 1) % Order.Length];
        /// <summary>The painted map whose art (portal, music, air) stands in until a new map's own exists.</summary>
        public static MapTheme StandIn(MapTheme map) => map switch
        {
            MapTheme.Glasshouse => MapTheme.PostOffice, // window light and a draught
            MapTheme.Theatre => MapTheme.LampHall, // a dark hall cut by light
            _ => map
        };
        /// <summary>The map's named elite comes on its third wave, else its second, else its fourth: never on a story boss's
        /// night (multiples of five) nor on the night of the map's mid-map boss (requirement 25/10/2026: one big foe a night).</summary>
        public static bool EliteWave(int wave)
        {
            int first = FirstWave(wave), last = LastWave(wave);
            foreach (int elite in new[] { first + 2, first + 1, first + 3, first })
            {
                if (elite > last || elite % 5 == 0 || MidMapBossRules.For(elite) != null) continue;
                return wave == elite;
            }
            return false;
        }
        public static string Name(MapTheme map) => map switch
        {
            MapTheme.WaitingRoom => "The Waiting Room",
            MapTheme.PostOffice => "The Post Office",
            MapTheme.Glasshouse => "The Glasshouse",
            MapTheme.LampHall => "The Lamp Hall",
            MapTheme.Theatre => "The Forgotten Theatre",
            _ => "The Archive Vault"
        };
        /// <summary>Folder of the map's painted props under Resources/Environment.</summary>
        public static string Folder(MapTheme map) => map.ToString();

        // the families, from the Waiting Realm roster (re-themed species keep their unit id: CoinLeech is the Toll Tick, SporeBishop
        // the Coat Rack, HopperKeg the Hatbox Hopper, BristleImp the Splinter Imp, SmokingCoffin the Unclaimed Trunk)
        private static readonly string[][] Families =
        {
            new[] { "SpareChair", "DoorKnocker", "SporeBishop", "CoinLeech", "HopperKeg", "BristleImp", "DoorBearer", "SleepMimic", "HushMite", "TicketMoth", "UmbrellaStalker", "SofaGlutton" },
            new[] { "SewnLetter", "PartyLine", "RibbonHydra", "MoonMagnet", "BroomSkater", "ChargerRam", "BlastPuffer", "StampTick", "SealHound", "SackGolem" },
            new[] { "LampSnuffer", "InkMoth", "CurtainWisp", "RuneWisp", "LostLanternSoul", "SmokeWraith", "BlueFlameFiend", "TwinFlameFurnace", "EmberProcession", "AshSwallower", "WickImp", "LampshadeJelly", "ChandelierCrawler", "FireScreenWarden", "TwinWickLantern", "CandleShade" },
            new[] { "LedgerHound", "SmokingCoffin", "ClockBat", "BurrowBell", "RedFlameBerserker", "SirenSeal", "InkNotary", "ClipMite", "InkToad", "CabinetMimic", "SpyglassSniper", "DrawerBroodmother", "LedgerTortoise" }, // + the playtest map enemies (requirement 23/10/2026)
            // requirement 25/10/2026: the Glasshouse's garden (spores, puffers, rain, toads, burrowers, splinters, ribbon vines)
            new[] { "SporeBishop", "BlastPuffer", "UmbrellaStalker", "InkToad", "BurrowBell", "BristleImp", "RibbonHydra", "HopperKeg", "BroomSkater" },
            // requirement 25/10/2026: the Theatre's stage (curtains, stage smoke, footlights, the chandelier, mimics, the singer, tickets)
            new[] { "CurtainWisp", "SmokeWraith", "CandleShade", "ChandelierCrawler", "SleepMimic", "SirenSeal", "TicketMoth", "MoonMagnet", "FireScreenWarden", "CabinetMimic", "EmberProcession" },
        };
        public static IReadOnlyList<string> Family(MapTheme map) => Families[(int)map];
        public static bool InFamily(MapTheme map, string id) => Array.IndexOf(Families[(int)map], id) >= 0;

        /// <summary>A roster v2 enemy of this map's family walks into the map's own waves (the Waiting Room's chairs and knockers
        /// from its third and fourth wave), even before its old zone.</summary>
        public static bool Joins(WaitingEnemyKind kind, string id, int wave)
        {
            if (kind == WaitingEnemyKind.None || kind == WaitingEnemyKind.WakingSitter || kind == WaitingEnemyKind.LetterHalf || kind == WaitingEnemyKind.LetterStamp) return false;
            MapTheme map = For(wave);
            if (!InFamily(map, id)) return false;
            int into = wave - FirstWave(wave);
            // a family's new enemy walks in from the map's second wave (the Waiting Room's chairs and knockers from its third
            // and fourth), never sooner than one wave ahead of its own zone (phase D sweep: Party Line cords at wave 6 were too much)
            if (map == MapTheme.WaitingRoom) return into >= (kind == WaitingEnemyKind.DoorKnocker ? 3 : 2);
            return into >= 1 && wave >= WaitingEnemyRoster.From(kind) - 1 || WaitingEnemyRoster.InRegularWaves(kind, wave, false);
        }

        /// <summary>The map's own elite affix: most affixed enemies of a map wear it.</summary>
        public static EnemyAffix Affix(MapTheme map) => map switch
        {
            MapTheme.WaitingRoom => EnemyAffix.BlackSmoke,
            MapTheme.PostOffice => EnemyAffix.RedFlame,
            MapTheme.LampHall => EnemyAffix.BlueFlame,
            MapTheme.Glasshouse => EnemyAffix.TwinFlame,
            MapTheme.Theatre => EnemyAffix.BlackSmoke, // stage smoke (five affixes for six maps)
            _ => EnemyAffix.Blackfire
        };

        /// <summary>Themes a wave's enemy pool: the map's family is weighted twice, and only a few other species stay as
        /// carry-overs (the last map's family first). A pool with fewer than two family species is left as it is.</summary>
        public static List<UnitDefinition> Theme(List<UnitDefinition> pool, int wave, int seed)
        {
            if (pool == null || pool.Count == 0) return pool;
            MapTheme map = For(wave), previous = Previous(map);
            var familyIds = new HashSet<string>(); var others = new List<string>();
            foreach (UnitDefinition unit in pool)
            {
                if (unit == null) continue;
                if (InFamily(map, unit.Id)) familyIds.Add(unit.Id);
                else if (!others.Contains(unit.Id)) others.Add(unit.Id);
            }
            if (familyIds.Count < 2) return pool;
            int carry = Mathf.Clamp(familyIds.Count - 1, 2, 4);
            others.Sort((a, b) =>
            {
                int pa = InFamily(previous, a) ? 0 : 1, pb = InFamily(previous, b) ? 0 : 1;
                return pa != pb ? pa - pb : Hash(seed, wave / 5, a).CompareTo(Hash(seed, wave / 5, b));
            });
            var kept = new HashSet<string>(); for (int i = 0; i < others.Count && kept.Count < carry; i++) kept.Add(others[i]);
            // the theme changes who walks in, not how hard the wave is: the brutes (big, fast, hard-hitting bodies) keep exactly the
            // share they had in the unthemed pool, whatever the map (phase D sweep: an Archive Vault of Wardrobe Crabs halved the
            // bots' win rate)
            var themed = new List<UnitDefinition>(); var brutes = new List<UnitDefinition>();
            int counted = 0;
            foreach (UnitDefinition unit in pool)
            {
                if (unit == null) continue;
                counted++;
                if (Brute(unit)) { brutes.Add(unit); continue; }
                if (familyIds.Contains(unit.Id)) { themed.Add(unit); themed.Add(unit); }
                else if (kept.Remove(unit.Id)) themed.Add(unit); // each carry-over once
            }
            if (themed.Count == 0) return pool;
            float share = brutes.Count / (float)Mathf.Max(1, counted);
            int wanted = Mathf.RoundToInt(share * themed.Count / Mathf.Max(.05f, 1 - share));
            for (int i = 0; i < wanted && brutes.Count > 0; i++) themed.Add(brutes[i % brutes.Count]);
            return themed;
        }
        /// <summary>A brute: a big ordinary body (Wardrobe Crab, Furnace Toad) whose share of a wave sets its difficulty.</summary>
        public static bool Brute(UnitDefinition unit) => unit.BodyHeight > 1.5f && unit.SpecialEnemy == SpecialEnemyKind.None && unit.WaitingKind == WaitingEnemyKind.None;
        private static uint Hash(int seed, int map, string id)
        {
            uint h = unchecked((uint)seed * 2654435761u ^ (uint)map * 2246822519u);
            foreach (char c in id) h = unchecked((h ^ c) * 16777619u);
            return h;
        }

        /// <summary>The living elements (D11 v2), one guide line each in player words, shown the first time it appears.</summary>
        /// <summary>The living elements of the six maps (Docs/Map_Mechanics_2026-10-25.md), one guide line each in player words.</summary>
        public enum Element
        {
            Board, Chute, Gust, Parcel, Light, Snuffer, Blooms, Spotlight,
            Tickets, Dust, Waiters, WallClock, Stamps, Planes, Weather, Pollen, Roof, Turret, Oil, Lantern,
            Smoke, Audience, Curtain, Fireworks, Pages, InkFog, Bookworms, Inkpot, SkyStamp
        }
        /// <summary>Each map's elements in the order their guide lines are shown on its first night.</summary>
        public static Element[] Elements(MapTheme map) => map switch
        {
            MapTheme.WaitingRoom => new[] { Element.Tickets, Element.Board, Element.Dust, Element.Waiters, Element.WallClock },
            MapTheme.PostOffice => new[] { Element.Chute, Element.Planes, Element.Gust, Element.Stamps, Element.Parcel },
            MapTheme.Glasshouse => new[] { Element.Blooms, Element.Weather, Element.Pollen, Element.Turret, Element.Roof },
            MapTheme.LampHall => new[] { Element.Light, Element.Oil, Element.Snuffer, Element.Lantern },
            MapTheme.Theatre => new[] { Element.Spotlight, Element.Smoke, Element.Audience, Element.Curtain, Element.Fireworks },
            _ => new[] { Element.Pages, Element.InkFog, Element.SkyStamp, Element.Bookworms, Element.Inkpot }
        };
        public static string TipId(Element element) => "env." + element.ToString().ToLowerInvariant();
        public static string TipLine(Element element) => element switch
        {
            Element.Tickets => "Walk over a queue ticket for its colour's boost. A ticket with the board's number gives double.",
            Element.Board => "A number is called: its door shakes, 3-2-1. Break it in time for gold and a ticket, or a pack pours out.",
            Element.Dust => "Ceiling dust: enemies in it move and attack slower.",
            Element.Waiters => "An enemy left waiting gets angry: faster and stronger, but it takes more damage and drops double gold.",
            Element.WallClock => "At twelve the wall clock glows. Stand by it: every enemy freezes for 5 s.",
            Element.Chute => "Catch falling letters for a boost. Three from one drop: Full Delivery!",
            Element.Gust => "Paper wind: enemies walking into it slow down, and their shots drift and slow.",
            Element.Stamps => "A stamped enemy is faster and hits harder. Kill it before the stamp burns out: it bursts and drops a letter.",
            Element.Parcel => "A giant parcel lands! Break it: it bursts and spills gold. Leave it and an elite climbs out.",
            Element.Planes => "Paper planes sweep the lit lane and strike every enemy in it.",
            Element.Blooms => "Pots bloom, faster in the sun. Walk through one for its colour's boost.",
            Element.Weather => "Sun and rain take turns. Sun: foes attack slower. Rain: foes are slowed and knocked further.",
            Element.Pollen => "A foe with a gold aura speeds up those near it. Kill it for a flower bed: more damage and attack speed inside.",
            Element.Roof => "Red floor: a roof pane is about to fall. Step out and let foes stand in it.",
            Element.Turret => "Stand by a big bud to open it: it shoots nearby foes for 15 s.",
            Element.Light => "Lamp light: +15% damage and +2 armour, and friends attack faster. In the dark foes are quick but drowsy; stepping into light dazzles them.",
            Element.Oil => "Lit lamps drop oil vials. Walk through one for a short boost.",
            Element.Snuffer => "A hand snuffs a lamp. Relight it within 3 s and it flares, stunning foes in its light.",
            Element.Lantern => "Stand by a lantern to carry it: you become a light and its flame burns foes close by.",
            Element.Spotlight => "Stand in the spotlight: white damage, yellow attack speed, pink armour. Stay 3 s for an ENCORE.",
            Element.Smoke => "Stage smoke slows enemies and shortens their shots. The drum clears it.",
            Element.Audience => "Outside the light the ghost audience throws at you. In it, they throw coins.",
            Element.Curtain => "The curtain falls where the floor glows: it crushes enemies, but your shots can't pass it.",
            Element.Fireworks => "Kills in the spotlight load the firework machine. Then stand by it to hit and freeze every enemy.",
            Element.Pages => "Catch flying pages for a boost. Three in 30 s is a CHAPTER: all three for longer.",
            Element.InkFog => "Ink fog slows enemies and weakens their hits.",
            Element.Bookworms => "Bookworms eat pages to grow. Kill one to get its pages back, with gold.",
            Element.Inkpot => "Spilled ink makes enemies faster and stronger, but they burst when they die in it.",
            _ => "The stamp falls on the biggest crowd. Survivors take more damage."
        };
        public static string Prop(Element element) => element switch
        {
            Element.Tickets => "WaitingRoom/Ticket_Red", Element.Board => "WaitingRoom/Board_Flash", Element.Dust => "WaitingRoom/Board_Idle",
            Element.Waiters => "WaitingRoom/Door_Ajar", Element.WallClock => "WaitingRoom/Clock_Wall",
            Element.Chute => "PostOffice/Letter_Gold", Element.Gust => "PostOffice/Scrap_A", Element.Stamps => "PostOffice/Stamp",
            Element.Parcel => "PostOffice/Parcel_Intact", Element.Planes => "PostOffice/Paper_Plane",
            Element.Blooms => "Glasshouse/Bloom_Red", Element.Weather => "Glasshouse/Bloom_Blue", Element.Pollen => "Glasshouse/Bloom_Yellow",
            Element.Roof => "Glasshouse/Pot_Bud", Element.Turret => "Glasshouse/Flower_Turret",
            Element.Light => "LampHall/Lamp_Lit", Element.Oil => "LampHall/Oil_Vial", Element.Snuffer => "LampHall/Hand_Snuff", Element.Lantern => "LampHall/Lantern_Carry",
            Element.Spotlight => "Theatre/Curtain_Velvet", Element.Smoke => "Theatre/Seats_Row", Element.Audience => "Theatre/Seats_Row",
            Element.Curtain => "Theatre/Curtain_Velvet", Element.Fireworks => "Theatre/Firework_Machine",
            Element.Pages => "ArchiveVault/Page_A", Element.InkFog => "ArchiveVault/Inkpot_Up", Element.Bookworms => "ArchiveVault/Page_B",
            Element.Inkpot => "ArchiveVault/Inkpot_Tipped", _ => "ArchiveVault/Stamp_Giant"
        };
        private static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public static Sprite Sprite(string path)
        {
            if (sprites.TryGetValue(path, out Sprite found) && found != null) return found;
            Sprite loaded = Resources.Load<Sprite>("Environment/" + path);
            sprites[path] = loaded; return loaded;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() => sprites.Clear();
    }
}
