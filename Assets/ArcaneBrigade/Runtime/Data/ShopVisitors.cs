using System;

namespace ArcaneBrigade
{
    public enum VisitorChoice { Accept, PickItem, PickCompanion, Options }

    /// <summary>A visitor of the dream shop (Docs/Events_Ideas.md, approved 18/10/2026): a character with one offer, taken or
    /// left. Every effect is temporary (this shop or the next wave) and every reward small; nobody heals or trades health.</summary>
    public sealed class VisitorDefinition
    {
        public string Id, Name, Line, Offer;
        public VisitorChoice Choice;
        public string[] Options = Array.Empty<string>();
        public int Cost;
        public VisitorDefinition(string id, string name, string line, string offer, VisitorChoice choice = VisitorChoice.Accept, int cost = 0, params string[] options)
        { Id = id; Name = name; Line = line; Offer = offer; Choice = choice; Cost = cost; Options = options; }
    }

    public static class ShopVisitors
    {
        private const VisitorChoice A = VisitorChoice.Accept, I = VisitorChoice.PickItem, C = VisitorChoice.PickCompanion, O = VisitorChoice.Options;
        private static VisitorDefinition V(string id, string name, string line, string offer, VisitorChoice choice = A, int cost = 0, params string[] options) => new VisitorDefinition(id, name, line, offer, choice, cost, options);

        public static readonly VisitorDefinition[] All =
        {
            V("GateKeeper", "The Gatekeeper's Herald", "The Gatekeeper dares you: a harder hall, and two gilded guards.", "Next wave: +20% enemies and 2 gilded elites. Win it for a Common relic (Uncommon from wave 12)."),
            V("LostPostman", "The Lost Postman", "Would you carry this letter through the next hall? Gently, please.", "Next wave: take no more than 5 hits and earn 15 gold."),
            V("DiceElder", "The Dice Elder", "A small wager on your nerve?", "Finish the next wave above half health and win back 1.5 times your bet.", O, 0, "Bet 5 gold", "Bet 10 gold", "Bet 20 gold"),
            V("Pawnbroker", "The Roadside Pawnbroker", "I'll keep it safe. Mostly.", "Pawn a relic for half its price; redeem it within 2 waves for the same.", I),
            V("ParcelCourier", "The Unmarked Parcel", "(the parcel rustles, hopeful to be opened)", "Open it for 10 gold: 15 gold, an egg, a Common relic - or an empty box and a note.", A, 10),
            V("TroupeLeader", "The Travelling Troupe", "Choose tonight's play, traveller!", "Pick a play for the next wave.", O, 0, "Swift friends: +10% attack speed", "Sleepy audience: enemies 10% slower", "Coin toss: coins +1"),
            V("Lamplighter", "The Lamplighter", "Four lamps, four corners. Shall I light them?", "Next wave: four corner lamps set passing enemies alight.", A, 6),
            V("LostToy", "The Lost Toy", "Could I walk with you for a while? I think home is that way.", "Next wave it wanders beside you: stay within 3 units of it for 20s and it finds its way (an egg)."),
            V("ToyMender", "The Toy Mender", "A stitch or two, and they'll hold their ground.", "Patch one friend: next wave its hits slow enemies 20%.", C, 8),
            V("MusicBoxKeeper", "The Music Box Winder", "Shall I wind it a little tighter?", "Next wave everything is 15% faster, enemies too; coins are worth 1 more."),
            V("Barterer", "The Barterer", "That trinket, for one of mine?", "Swap a relic for a random relic of the same rarity.", I),
            V("JunkCollector", "The Junk Collector", "Old things find new homes.", "Sell a relic for 60% of its price.", I),
            V("Lender", "The Kind Lender", "Borrow it for one hall. Bring it back.", "Borrow a random Rare relic for the next wave."),
            V("Storyteller", "The Storyteller", "Sit, sit. Have you heard about the lamp that waited?", "Hear a page of the Waiting Realm; the first 20s of the next wave drop +1 coin per kill."),
            V("SwapClown", "The Stall-Swapping Clown", "Shuffle, shuffle! Fancier, pricier!", "Every relic on the shelf becomes one rarity higher, prices +30%."),
            V("ClockRewinder", "The Clock Rewinder", "Shall we pretend that refresh never happened?", "Undo your last refresh: its gold back, the shelf as it was.", A, 4),
            V("SleeplessChild", "The Sleepless Child", "Can you make the hall quiet? Then I could sleep.", "Next wave: defeat 80 enemies and earn an egg."),
            V("CandyWitch", "The Candy Witch", "A bag of sweets? They'll want them more than you.", "Next wave: every 10s a sweet drops near you and enemies crowd it for 2s.", A, 6),
            V("BalloonSeller", "The Balloon Seller", "Let one go - it always brings something back. Usually.", "After the next wave the balloon brings back a random Common relic (or nothing).", A, 5),
            V("PromiseSeller", "The Promise Seller", "Promise me something small, and keep it.", "Promise to recruit no friend in the next shop; keep it and the shop after is 15% cheaper."),
            V("Ferryman", "The Ferryman", "One passenger across. Someone else comes back.", "Swap a one-star friend for another one-star friend of the same theme.", C),
            V("MatchGirl", "The Match Girl", "One match? It burns longer than you'd think.", "Next wave the first 3 enemies defeated flare up, burning those beside them.", A, 3),
            V("Collector", "The Odd Collector", "I need ten of a particular kind. You'll know them.", "Next wave: defeat 10 of the species he names and earn 12 gold."),
            V("Engraver", "The Name Engraver", "Whose name shall I carve?", "Engrave one friend: next wave it deals +15% damage.", C, 6),
            V("Waker", "The Waker", "Up early, up ready!", "Next wave your gauge starts a third full.", A, 6),
            V("DreamSeller", "The Dream Seller", "Three dreams, sealed. Which one is yours?", "A small surprise for the next wave.", O, 0, "The silver jar", "The blue jar", "The golden jar"),
            V("Twins", "The Twins", "Buy one, and the other comes along - half price!", "The next friend you buy in this shop is offered again at half price."),
            V("WaterCarrier", "The Well-Water Carrier", "Fresh from the well. It soaks everything.", "Next wave puddles in the four corners soak enemies (frost freezes them).", A, 5),
            V("NightThief", "The Night Thief", "Heavy purse you've got there...", "Leave this shop with more than 60 gold and he takes 10% - unless you chase him off.", O, 0, "Chase him off (5 gold)", "Ignore him"),
            V("DollDoctor", "The Doll Doctor", "A stitch in your cloak, for the bad moment.", "Next wave, the first time you fall below 25% health, enemies around you are thrown back.", A, 10),
            V("KeyKeeper", "The Key Keeper", "This one opens something good.", "Next wave the first chest opened also gives a Common relic.", A, 10),
            V("CupMagician", "The Stage Magician", "Watch closely - and now it's free!", "Make one random offer on the shelf free.", A, 6),
            V("FriendSeeker", "The Friend Seeker", "Could one of yours walk with me for a while?", "Lend a friend for the next wave; it comes back with an egg and 10 gold.", C),
            V("DreamSmith", "The Dream Smith", "I can forge a new dream into that weapon.", "Reforge one friend's weapon: choose one of three.", C, 8),
            V("MapSeller", "The Map Seller", "Every map shows where the chests sleep.", "Next wave chests appear more often.", A, 6),
            V("EggTrader", "The Egg Trader", "Eggs for trinkets, trinkets for eggs.", "Trade 2 eggs for an Uncommon relic, or a relic for 2 eggs.", O, 0, "Give 2 eggs", "Give a relic"),
            V("WeatherTeller", "The Weather Teller", "Rain of teeth tomorrow. Take an umbrella.", "Learn the next wave's enemies; an umbrella brushes aside 1 in 5 hostile shots near you.", A, 6),
            V("Knitter", "The Old Knitter", "Wrap up. Run quicker when you're warm.", "Next wave the hero runs 15% faster.", A, 8),
            V("Painter", "The Portrait Painter", "Hold still, all of you!", "A portrait of your brigade, kept as a keepsake of this run."),
            V("ShadowBorrower", "The Shadow Borrower", "Lend me your shadow. They'll chase it instead.", "Next wave, for the first 5s enemies chase your shadow instead of you.", A, 8),
            V("Sandman", "The Sandman", "A pinch of sand, and they'll come slower.", "For the first 20s of the next wave enemies arrive 15% slower.", A, 8),
            V("ColourStranger", "The Colour Stranger", "Fewer colours, fewer coins.", "Refresh the shelf for free: one rarity lower and 30% cheaper."),
            V("FuturePost", "The Post Office to the Future", "Send it now; it arrives later, a little heavier.", "Post 10 gold: three waves from now, 15 gold arrives.", A, 10),
            V("HourglassFixer", "The Hourglass Turner", "Let it run a little longer this time.", "Next wave lasts 10s longer: more enemies, more coins."),
            V("Busker", "The Wandering Busker", "A song for the friends who almost belong together.", "Next wave the bond closest to its next level counts one more friend."),
        };
        public static VisitorDefinition For(string id) => Array.Find(All, v => v.Id == id);
    }
}
