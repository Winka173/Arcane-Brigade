using System;
using System.Collections.Generic;

namespace ArcaneBrigade
{
    public enum VisitorChoice { Accept, PickItem, PickCompanion, Options }

    /// <summary>A visitor's offer, the way its card shows it (feedback 19/10/2026: icons and colour, not prose): what you DO (a
    /// short verb), what you PAY (red), what you GET (green), and WHEN or IF it happens (a neutral chip). PAY and GET are rows of
    /// terms "icon:amount", joined by " + " (and) or " / " (one of); "free" means nothing is paid. Icons: gold, coin (each coin
    /// picked up), egg, chest, relic (amount = a rarity or a count), friend, bond, hero, gauge, lamp, lure, umbrella, time, price, rarity,
    /// shelf, weapon, elite, ? (a surprise), or an ItemEffect name drawn with its stat glyph.</summary>
    public sealed class VisitorDeal
    {
        public const string Free = "free";
        public string Do, Pay, Get, When;
        public VisitorDeal(string act, string pay, string get, string when = "") { Do = act; Pay = pay; Get = get; When = when ?? ""; }
        public bool IsFree => Pay == Free;
        public VisitorDeal With(string get) => new VisitorDeal(Do, Pay, get, When);
    }

    /// <summary>One "icon:amount" of a deal row; Or marks a term that is an alternative to the one before it.</summary>
    public readonly struct VisitorTerm
    {
        public readonly string Icon, Amount; public readonly bool Or;
        public VisitorTerm(string icon, string amount, bool or) { Icon = icon; Amount = amount; Or = or; }
    }

    /// <summary>A visitor of the dream shop (Docs/Events_Ideas.md, approved 18/10/2026): a character with one offer, taken or
    /// left. Every effect is temporary (this shop or the next wave) and every reward small; nobody heals or trades health.</summary>
    public sealed class VisitorDefinition
    {
        public string Id, Name, Line, Offer;
        public VisitorChoice Choice;
        public string[] Options = Array.Empty<string>();
        public int Cost;
        /// <summary>The card's summary: one deal per option for an Options visitor, otherwise one deal for the whole offer
        /// (a relic or friend pick shows it once above the pictures).</summary>
        public VisitorDeal[] Deals = Array.Empty<VisitorDeal>();
        public VisitorDefinition(string id, string name, string line, string offer, VisitorChoice choice = VisitorChoice.Accept, int cost = 0, params string[] options)
        { Id = id; Name = name; Line = line; Offer = offer; Choice = choice; Cost = cost; Options = options; }
        public VisitorDefinition With(params VisitorDeal[] deals) { Deals = deals; return this; }
    }

    public static class ShopVisitors
    {
        private const VisitorChoice A = VisitorChoice.Accept, I = VisitorChoice.PickItem, C = VisitorChoice.PickCompanion, O = VisitorChoice.Options;
        private const string F = VisitorDeal.Free, Next = "next wave", Here = "this shop";
        private static VisitorDefinition V(string id, string name, string line, string offer, VisitorChoice choice = A, int cost = 0, params string[] options) => new VisitorDefinition(id, name, line, offer, choice, cost, options);
        private static VisitorDeal D(string act, string pay, string get, string when = "") => new VisitorDeal(act, pay, get, when);

        public static readonly VisitorDefinition[] All =
        {
            V("GateKeeper", "The Gatekeeper's Herald", "The Gatekeeper dares you: a harder hall, and two gilded guards.", "Next wave: +20% enemies and 2 gilded elites. Win it for a Common relic (Uncommon from wave 12).")
                .With(D("Take the dare", "EnemyCount:+20% enemies + elite:2 elites", "relic:Common", Next)),
            V("LostPostman", "The Lost Postman", "Would you carry this letter through the next hall? Gently, please.", "Next wave: take no more than 5 hits and earn 15 gold.")
                .With(D("Carry the letter", F, "gold:15", "next wave · 5 hits max")),
            V("DiceElder", "The Dice Elder", "A small wager on your nerve?", "Finish the next wave above half health and win back 1.5 times your bet.", O, 0, "Bet 5 gold", "Bet 10 gold", "Bet 20 gold")
                .With(D("Bet 5", "gold:5", "gold:8", "next wave · HP 50%+"), D("Bet 10", "gold:10", "gold:15", "next wave · HP 50%+"), D("Bet 20", "gold:20", "gold:30", "next wave · HP 50%+")),
            V("Pawnbroker", "The Roadside Pawnbroker", "I'll keep it safe. Mostly.", "Pawn a relic for half its price; redeem it within 2 waves for the same.", I)
                .With(D("Pawn a relic", "relic:1 relic", "gold:half price", "buy back within 2 waves")),
            V("ParcelCourier", "The Unmarked Parcel", "(the parcel rustles, hopeful to be opened)", "Open it for 10 gold: 15 gold, an egg, a Common relic - or an empty box and a note.", A, 10)
                .With(D("Open it", "gold:10", "gold:15 / egg:1 / relic:Common / ?:empty")),
            V("TroupeLeader", "The Travelling Troupe", "Choose tonight's play, traveller!", "Pick a play for the next wave.", O, 0, "Swift friends: +10% attack speed", "Sleepy audience: enemies 10% slower", "Coin toss: coins +1")
                .With(D("Swift friends", F, "CompanionAttackSpeed:+10%", Next), D("Sleepy audience", F, "EnemySpeed:enemies -10%", Next), D("Coin toss", F, "coin:+1 each", Next)),
            V("Lamplighter", "The Lamplighter", "Four lamps, four corners. Shall I light them?", "Next wave: four corner lamps set passing enemies alight.", A, 6)
                .With(D("Light the lamps", "gold:6", "lamp:4 burning", Next)),
            V("LostToy", "The Lost Toy", "Could I walk with you for a while? I think home is that way.", "Next wave it wanders beside you: stay within 3 units of it for 20s and it finds its way (an egg).")
                .With(D("Walk with it", F, "egg:1", "next wave · near it 20s")),
            V("ToyMender", "The Toy Mender", "A stitch or two, and they'll hold their ground.", "Patch one friend: next wave its hits slow enemies 20%.", C, 8)
                .With(D("Patch a friend", "gold:8", "EnemySpeed:-20% on hit", Next)),
            V("MusicBoxKeeper", "The Music Box Winder", "Shall I wind it a little tighter?", "Next wave everything is 15% faster, enemies too; coins are worth 1 more.")
                .With(D("Wind it up", "EnemySpeed:enemies +15%", "CompanionAttackSpeed:friends +15% + MoveSpeed:you +15% + coin:+1 each", Next)),
            V("Barterer", "The Barterer", "That trinket, for one of mine?", "Swap a relic for a random relic of the same rarity.", I)
                .With(D("Swap a relic", "relic:1 relic", "relic:same rarity")),
            V("JunkCollector", "The Junk Collector", "Old things find new homes.", "Sell a relic for 60% of its price.", I)
                .With(D("Sell a relic", "relic:1 relic", "gold:60% price")),
            V("Lender", "The Kind Lender", "Borrow it for one hall. Bring it back.", "Borrow a random Rare relic for the next wave.")
                .With(D("Borrow a relic", F, "relic:Rare", "next wave only")),
            V("Storyteller", "The Storyteller", "Sit, sit. Have you heard about the lamp that waited?", "Hear a page of the Waiting Realm; the first 20s of the next wave drop +1 coin per kill.")
                .With(D("Listen", F, "coin:+1 each", "next wave · first 20s")),
            V("SwapClown", "The Stall-Swapping Clown", "Shuffle, shuffle! Fancier, pricier!", "Every relic on the shelf becomes one rarity higher, prices +30%.")
                .With(D("Shuffle the shelf", "price:+30%", "rarity:+1", Here)),
            V("ClockRewinder", "The Clock Rewinder", "Shall we pretend that refresh never happened?", "Undo your last refresh: its gold back, the shelf as it was.", A, 4)
                .With(D("Undo the refresh", "gold:4", "shelf:old shelf + gold:refund", Here)),
            V("SleeplessChild", "The Sleepless Child", "Can you make the hall quiet? Then I could sleep.", "Next wave: defeat 80 enemies and earn an egg.")
                .With(D("Hush the hall", F, "egg:1", "next wave · 80 kills")),
            V("CandyWitch", "The Candy Witch", "A bag of sweets? They'll want them more than you.", "Next wave: every 10s a sweet drops near you and enemies crowd it for 2s.", A, 6)
                .With(D("Take the sweets", "gold:6", "lure:sweet every 10s", Next)),
            V("BalloonSeller", "The Balloon Seller", "Let one go - it always brings something back. Usually.", "After the next wave the balloon brings back a random Common relic (or nothing).", A, 5)
                .With(D("Let it go", "gold:5", "relic:Common 70%", "after next wave")),
            V("PromiseSeller", "The Promise Seller", "Promise me something small, and keep it.", "Promise to recruit no friend in the next shop; keep it and the shop after is 15% cheaper.")
                .With(D("Promise", "friend:no recruit next shop", "price:-15%", "the shop after")),
            V("Ferryman", "The Ferryman", "One passenger across. Someone else comes back.", "Swap a one-star friend for another one-star friend of the same theme.", C)
                .With(D("Ferry a friend", "friend:1★", "friend:1★ same theme")),
            V("MatchGirl", "The Match Girl", "One match? It burns longer than you'd think.", "Next wave the first 3 enemies defeated flare up, burning those beside them.", A, 3)
                .With(D("Buy a match", "gold:3", "lamp:3 flares", Next)),
            V("Collector", "The Odd Collector", "I need ten of a particular kind. You'll know them.", "Next wave: defeat 10 of the species he names and earn 12 gold.")
                .With(D("Hunt for him", F, "gold:12", "next wave · 10 of one kind")),
            V("Engraver", "The Name Engraver", "Whose name shall I carve?", "Engrave one friend: next wave it deals +15% damage.", C, 6)
                .With(D("Engrave a friend", "gold:6", "CompanionDamage:+15%", Next)),
            V("Waker", "The Waker", "Up early, up ready!", "Next wave your gauge starts a third full.", A, 6)
                .With(D("Wake early", "gold:6", "gauge:1/3", "next wave start")),
            V("DreamSeller", "The Dream Seller", "Three dreams, sealed. Which one is yours?", "A small surprise for the next wave.", O, 0, "The silver jar", "The blue jar", "The golden jar")
                .With(D("Silver jar", F, "?:surprise", Next), D("Blue jar", F, "?:surprise", Next), D("Golden jar", F, "?:surprise", Next)),
            V("Twins", "The Twins", "Buy one, and the other comes along - half price!", "The next friend you buy in this shop is offered again at half price.")
                .With(D("Meet the twins", F, "friend:twin half price", Here)),
            V("WaterCarrier", "The Well-Water Carrier", "Fresh from the well. It soaks everything.", "Next wave puddles in the four corners soak enemies (frost freezes them).", A, 5)
                .With(D("Pour the water", "gold:5", "EnemySpeed:4 puddles", Next)),
            V("NightThief", "The Night Thief", "Heavy purse you've got there...", "Leave this shop with more than 60 gold and he takes 10% - unless you chase him off.", O, 0, "Chase him off (5 gold)", "Ignore him")
                .With(D("Chase him off", "gold:5", "gold:purse safe", Here), D("Ignore him", "gold:10%", "gold:5 kept", "if over 60 on leaving")),
            V("DollDoctor", "The Doll Doctor", "A stitch in your cloak, for the bad moment.", "Next wave, the first time you fall below 25% health, enemies around you are thrown back.", A, 10)
                .With(D("Stitch the cloak", "gold:10", "Knockback:push back once", "next wave · HP <25%")),
            V("KeyKeeper", "The Key Keeper", "This one opens something good.", "Next wave the first chest opened also gives a Common relic.", A, 10)
                .With(D("Take the key", "gold:10", "relic:Common", "next wave · first chest")),
            V("CupMagician", "The Stage Magician", "Watch closely - and now it's free!", "Make one random offer on the shelf free.", A, 6)
                .With(D("Watch the trick", "gold:6", "price:1 offer free", Here)),
            V("FriendSeeker", "The Friend Seeker", "Could one of yours walk with me for a while?", "Lend a friend for the next wave; it comes back with an egg and 10 gold.", C)
                .With(D("Lend a friend", "friend:1 for a wave", "egg:1 + gold:10", "after next wave")),
            V("DreamSmith", "The Dream Smith", "I can forge a new dream into that weapon.", "Reforge one friend's weapon: choose one of three.", C, 8)
                .With(D("Reforge a weapon", "gold:8", "weapon:pick 1 of 3")),
            V("MapSeller", "The Map Seller", "Every map shows where the chests sleep.", "Next wave chests appear more often.", A, 6)
                .With(D("Buy a map", "gold:6", "chest:more often", Next)),
            V("EggTrader", "The Egg Trader", "Eggs for trinkets, trinkets for eggs.", "Trade 2 eggs for an Uncommon relic, or a relic for 2 eggs.", O, 0, "Give 2 eggs", "Give a relic")
                .With(D("Give 2 eggs", "egg:2", "relic:Uncommon"), D("Give a relic", "relic:1 relic", "egg:2")),
            V("WeatherTeller", "The Weather Teller", "Rain of teeth tomorrow. Take an umbrella.", "Learn the next wave's enemies; an umbrella brushes aside 1 in 5 hostile shots near you.", A, 6)
                .With(D("Take the umbrella", "gold:6", "umbrella:1 in 5 shots", Next)),
            V("Knitter", "The Old Knitter", "Wrap up. Run quicker when you're warm.", "Next wave the hero runs 15% faster.", A, 8)
                .With(D("Wear the scarf", "gold:8", "MoveSpeed:+15%", Next)),
            V("Painter", "The Portrait Painter", "Hold still, all of you!", "A portrait of your brigade, kept as a keepsake of this run.")
                .With(D("Sit for it", F, "hero:a portrait")),
            V("ShadowBorrower", "The Shadow Borrower", "Lend me your shadow. They'll chase it instead.", "Next wave, for the first 5s enemies chase your shadow instead of you.", A, 8)
                .With(D("Lend your shadow", "gold:8", "lure:decoy shadow", "next wave · first 5s")),
            V("Sandman", "The Sandman", "A pinch of sand, and they'll come slower.", "For the first 20s of the next wave enemies arrive 15% slower.", A, 8)
                .With(D("Sprinkle sand", "gold:8", "EnemyCount:spawns 15% slower", "next wave · first 20s")),
            V("ColourStranger", "The Colour Stranger", "Fewer colours, fewer coins.", "Refresh the shelf for free: one rarity lower and 30% cheaper.")
                .With(D("Fade the shelf", "rarity:-1", "price:-30%", Here)),
            V("FuturePost", "The Post Office to the Future", "Send it now; it arrives later, a little heavier.", "Post 10 gold: three waves from now, 15 gold arrives.", A, 10)
                .With(D("Post it", "gold:10", "gold:15", "in 3 waves")),
            V("HourglassFixer", "The Hourglass Turner", "Let it run a little longer this time.", "Next wave lasts 10s longer: more enemies, more coins.")
                .With(D("Turn it", "time:+10s fight", "coin:more coins", Next)),
            V("Busker", "The Wandering Busker", "A song for the friends who almost belong together.", "Next wave the bond closest to its next level counts one more friend.")
                .With(D("Hear the song", F, "bond:+1 friend", Next)),
        };
        public static VisitorDefinition For(string id) => Array.Find(All, v => v.Id == id);

        /// <summary>The deal shown for an option (Options visitors) or for the whole offer, in a shop before wave
        /// <paramref name="shopWave"/>+1. The Gatekeeper's prize is Uncommon when the dared wave is 12 or later (RunVisitors).</summary>
        public static VisitorDeal DealFor(VisitorDefinition v, int option, int shopWave)
        {
            if (v == null || v.Deals.Length == 0) return null;
            VisitorDeal deal = v.Choice == VisitorChoice.Options ? v.Deals[Math.Max(0, Math.Min(option, v.Deals.Length - 1))] : v.Deals[0];
            if (v.Id == "GateKeeper" && shopWave + 1 >= 12) deal = deal.With("relic:Uncommon");
            return deal;
        }

        /// <summary>The icons a deal term may name (besides an ItemEffect's stat glyph).</summary>
        public static readonly string[] Icons = { "gold", "coin", "egg", "chest", "relic", "friend", "bond", "hero", "gauge", "lamp", "lure", "umbrella", "time", "price", "rarity", "shelf", "weapon", "elite", "?" };
        public static bool KnownIcon(string icon) => Array.IndexOf(Icons, icon) >= 0 || Enum.TryParse(icon, out ItemEffect _);

        /// <summary>A row of a deal as terms ("free" is no terms).</summary>
        public static List<VisitorTerm> Terms(string row)
        {
            var list = new List<VisitorTerm>();
            if (string.IsNullOrEmpty(row) || row == VisitorDeal.Free) return list;
            bool or = false; int at = 0;
            while (at < row.Length)
            {
                int and = row.IndexOf(" + ", at, StringComparison.Ordinal), alt = row.IndexOf(" / ", at, StringComparison.Ordinal);
                int end = and < 0 ? alt : alt < 0 ? and : Math.Min(and, alt);
                string part = end < 0 ? row.Substring(at) : row.Substring(at, end - at);
                int cut = part.IndexOf(':');
                list.Add(cut < 0 ? new VisitorTerm("", part.Trim(), or) : new VisitorTerm(part.Substring(0, cut).Trim(), part.Substring(cut + 1).Trim(), or));
                if (end < 0) break;
                or = end == alt; at = end + 3;
            }
            return list;
        }

        /// <summary>The whole number a row names for one icon (e.g. "gold:15" → 15), 0 when it names none.</summary>
        public static int Amount(string row, string icon)
        {
            int total = 0;
            foreach (VisitorTerm t in Terms(row))
            {
                if (t.Icon != icon) continue;
                int n = 0, i = 0; string s = t.Amount.TrimStart('+');
                while (i < s.Length && char.IsDigit(s[i])) n = n * 10 + (s[i++] - '0');
                if (i > 0 && (i == s.Length || s[i] == ' ')) total += n;
            }
            return total;
        }
    }
}
