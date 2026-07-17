using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum MetaCurrency { LampOil, Keepsakes }
    /// <summary>The ten objects of the Waiting Room, in the art's reading order (Hub_Stations_v1).</summary>
    public enum MetaStation { Lantern, Ledger, Nursery, Wardrobe, Clock, Postbox, Museum, NightPact, Window, Bell }

    [Serializable] public sealed class MetaRank { public string Id; public int Rank; }
    /// <summary>Wardrobe mastery: waves cleared with one hero, and the run-start choices last picked for it.</summary>
    [Serializable] public sealed class HeroMastery { public string Hero; public int Waves; public bool AltStarter; public bool SecondHabit; public int Wins; /* runs won with this hero (HeroUnlocks) */ }

    /// <summary>The meta block of the profile (play-test 20/10/2026, item 27, "The Waiting Room"). Versioned: a profile saved
    /// before it loads with <see cref="Revision"/> 0 and is migrated once (Lamp Oil for its best wave, nothing else).</summary>
    [Serializable]
    public sealed class MetaProgress
    {
        public const int CurrentRevision = 5; // 2: the Postbox, Museum, Night Pact, Window and Bell (additive, no migration); 3: the Lamp Tree (old ranks refunded); 4: its ranks
        public int Revision;
        public int LampOil, Keepsakes, LampOilEarned, KeepsakesEarned;
        public List<MetaRank> Ranks = new List<MetaRank>();
        /// <summary>One-time Keepsake sources already paid: boss/difficulty/id, keeper/difficulty, secret/kind, duet/a+b.</summary>
        public List<string> Found = new List<string>();
        public int DuetsFound;
        public List<HeroMastery> Mastery = new List<HeroMastery>();
        public string Heirloom = "";
        /// <summary>Rolls the Nursery's three friends; it moves on with every run started.</summary>
        public int NurseryRoll;
        /// <summary>What was already paid for the last credited run (a rewound run pays only what it adds).</summary>
        public string PaidRun = "";
        public int PaidOil, PaidWaves;
        /// <summary>The Postbox: letters read (each keeps its relic for the run-start pick) and the keepsake relic last picked.</summary>
        public List<string> Letters = new List<string>();
        public string Keepsake = "";
        /// <summary>The Night Pact: the curses toggled for the next run (one bit each) and each hero's best pact score.</summary>
        public int Pacts;
        public List<MetaScore> PactBest = new List<MetaScore>();
        /// <summary>The Window: the best Daily Dream of each day (Key yyyy-MM-dd, the last 30 days).</summary>
        public List<MetaScore> DailyBest = new List<MetaScore>();
        /// <summary>The Bell: Favour stars (each +10% Lamp Oil gain).</summary>
        public int Favour;
        /// <summary>The Lamp Tree: each lantern's rank (the roots too); Tree is the first tree's lit ids (revision 3, moved into
        /// Lamps on load); the difficulties won (one bit each; each opens a branch).</summary>
        public List<MetaRank> Lamps = new List<MetaRank>();
        public List<string> Tree = new List<string>();
        public int WonMask;
        /// <summary>Every lantern was lit once (it stays true after the Bell resets the oil ones): the last two difficulties open.</summary>
        public bool TreeDone, TreeAllDone;
    }

    /// <summary>The meta loadout frozen into a run at its start (and what the run used of it). Saved with the run; a run saved
    /// before the Waiting Room loads with zeros and plays exactly as before.</summary>
    [Serializable]
    public sealed class MetaRunState
    {
        public int Revision = 1;
        public float Health, MoveSpeed, Pickup, Crit, ChestGold, TwoStarChance, CandleShare, CandleShield, GaugeStart;
        public int StartGold, Interest, ExtraSlots;
        public bool FreeRefresh, RewindOwned, RewindUsed, CandleUsed, SecondHabit;
        public string Heirloom = "", StartFriend = "", AltStarter = "";
        /// <summary>The shop before the last wave, kept for Rewind (empty once used or when Rewind is not owned).</summary>
        public string RewindShop = "";
        public int RewindAt;
        /// <summary>Discoveries made in this run (first friend / relic / boss ever seen), counted as the profile learns them.</summary>
        public int FoundUnits, FoundItems, FoundBosses;
        /// <summary>Endless milestones the profile had reached when the run began: endgame relics join the shops at 1 and 3.</summary>
        public int Endgame;
        /// <summary>The haul, written when the run is credited: Lamp Oil and Keepsakes and one "kind|words|amount" line each.</summary>
        public int HaulOil, HaulKeepsakes;
        public List<string> HaulLines = new List<string>();
        /// <summary>The Museum's gifts (damage within the +25% cap, gold), the keepsake relic brought along, the Night Pact's
        /// curses (one bit each) and the Daily Dream's day (empty for an ordinary run).</summary>
        public float Damage, Gold;
        public int Pacts;
        public string Keepsake = "", Daily = "";
        /// <summary>The Lamp Tree's crowns: the first friend bought arrives one star up; the first shop holds a rare relic;
        /// Treasure nose's better chests.</summary>
        public bool FirstFriendStar, FirstFriendUsed, FirstShopRare, FirstShopDone;
        public float ChestQuality;
        /// <summary>The later branches: stats with their scopes (armour, dodge, the hero's damage and speed, enemies' health and
        /// speed, prices), the Lamp shell (first hit of each wave blocked), the Night hunter (enemies' damage down), the Merchant's
        /// cheaper refreshes, sooner and wider dream market, and its one cheaper offer in every shop.</summary>
        public List<MetaStat> Stats = new List<MetaStat>();
        public bool WaveShield, DreamSooner, RegularDeal;
        public float EnemyWeaken;
        public int RerollDiscount, DreamExtra;
        public bool Any => Health > 0 || MoveSpeed > 0 || Pickup > 0 || Crit > 0 || ChestGold > 0 || TwoStarChance > 0 || CandleShare > 0 || GaugeStart > 0 || StartGold > 0 || Interest > 0 || ExtraSlots > 0 || FreeRefresh || RewindOwned || SecondHabit || Heirloom.Length > 0 || StartFriend.Length > 0 || AltStarter.Length > 0
                           || Damage > 0 || Gold > 0 || Pacts != 0 || FirstFriendStar || FirstShopRare || ChestQuality > 0 || Stats != null && Stats.Count > 0 || WaveShield || EnemyWeaken > 0 || RerollDiscount > 0 || DreamSooner || DreamExtra > 0 || RegularDeal || !string.IsNullOrEmpty(Keepsake) || !string.IsNullOrEmpty(Daily);
    }

    /// <summary>One upgrade line of a station: ranks, their prices, what one rank adds.</summary>
    public sealed class MetaTrack
    {
        public string Id, Name, Line;
        public MetaStation Station;
        public MetaCurrency Currency;
        public int[] Cost;
        /// <summary>Permanent combat power one rank adds, in percent (the Lantern and the Nursery; capped in total at 25).</summary>
        public float Power;
        public Func<int, string> Value;
        public int Max => Cost.Length;
    }

    /// <summary>The Waiting Room's rules: the stations, buying ranks, the haul a run earns, the loadout a run starts with.</summary>
    public static partial class MetaProgression
    {
        public const float PowerCap = 40f; // was 25 (requirement 24/10/2026: more permanent stats)
        public static readonly float[] DifficultyOil = { 1f, 1.2f, 1.6f, 1.8f, 2.8f }; // Peaceful, Wandering, Nightmare, Sleepless, The Last Night
        public const int OilPerWave = 4, OilPerBoss = 12, OilPerBestWave = 8, FindsCap = 60;
        public static readonly int[] MasteryWaves = { 8, 20, 40 };
        /// <summary>The stations in the room: the Lantern is the Lamp Tree now; the Ledger, Nursery and Clock went into its branches (24/10/2026).</summary>
        public static readonly MetaStation[] Working = { MetaStation.Lantern, MetaStation.Wardrobe, MetaStation.Postbox, MetaStation.Museum, MetaStation.NightPact, MetaStation.Window, MetaStation.Bell };
        public static bool Works(MetaStation station) => Array.IndexOf(Working, station) >= 0;

        private static MetaTrack T(string id, MetaStation station, string name, string line, MetaCurrency currency, int[] cost, float power, Func<int, string> value)
            => new MetaTrack { Id = id, Station = station, Name = name, Line = line, Currency = currency, Cost = cost, Power = power, Value = value };
        private static string Pct(float v) => "+" + Mathf.RoundToInt(v) + "%";

        public static readonly MetaTrack[] Tracks = new MetaTrack[0]; // retired 24/10/2026: the Lamp Tree (LampTree.cs)
        private static readonly MetaTrack[] RetiredRankLines =
        {
            T("lantern.wick", MetaStation.Lantern, "Wick", "Max HP", MetaCurrency.LampOil, new[] { 15, 30, 50, 75, 105 }, 1.5f, r => r == 0 ? "—" : Pct(3 * r)),
            T("lantern.stride", MetaStation.Lantern, "Stride", "Move speed", MetaCurrency.LampOil, new[] { 20, 40, 65, 95 }, .5f, r => r == 0 ? "—" : Pct(2 * r)),
            T("lantern.reach", MetaStation.Lantern, "Long reach", "Pickup range", MetaCurrency.LampOil, new[] { 15, 30, 50, 75 }, 0, r => r == 0 ? "—" : Pct(10 * r)),
            T("lantern.spark", MetaStation.Lantern, "Spark", "Crit chance, you and friends", MetaCurrency.LampOil, new[] { 25, 45, 70, 100, 135 }, 1.5f, r => r == 0 ? "—" : Pct(r)),
            T("ledger.purse", MetaStation.Ledger, "Purse", "Starting gold", MetaCurrency.LampOil, new[] { 15, 30, 50, 75 }, 0, r => r == 0 ? "—" : "+" + 6 * r),
            T("ledger.look", MetaStation.Ledger, "Free look", "First refresh in every shop", MetaCurrency.LampOil, new[] { 120 }, 0, r => r == 0 ? "—" : "Free"),
            T("ledger.pockets", MetaStation.Ledger, "Deep pockets", "Interest cap", MetaCurrency.LampOil, new[] { 60, 120 }, 0, r => r == 0 ? "—" : "+" + r),
            T("ledger.chest", MetaStation.Ledger, "Full chests", "Gold in chests", MetaCurrency.LampOil, new[] { 20, 40, 65, 95 }, 0, r => r == 0 ? "—" : Pct(6 * r)),
            T("ledger.shelf", MetaStation.Ledger, "Extra shelf", "One more offer in every shop", MetaCurrency.Keepsakes, new[] { 6 }, 0, r => r == 0 ? "—" : "+1"),
            T("clock.candle", MetaStation.Clock, "Second Candle", "Once a run, get back up", MetaCurrency.Keepsakes, new[] { 3, 5, 7 }, 0, r => r == 0 ? "—" : r == 1 ? "30% HP" : r == 2 ? "40% HP" : "40% HP · 2s safe"),
            T("clock.rewind", MetaStation.Clock, "Rewind", "Once a run, replay a lost wave", MetaCurrency.Keepsakes, new[] { 10 }, 0, r => r == 0 ? "—" : "Ready"),
            T("nursery.cradle", MetaStation.Nursery, "Cradle", "Start with 1 of 3 friends", MetaCurrency.Keepsakes, new[] { 4 }, 4, r => r == 0 ? "—" : "1 of 3"),
            T("nursery.heirloom", MetaStation.Nursery, "Heirloom", "Your family bonds one friend sooner", MetaCurrency.Keepsakes, new[] { 5 }, 2, r => r == 0 ? "—" : "-1 friend"),
            T("nursery.stork", MetaStation.Nursery, "Stork", "New friend arrives at 2★", MetaCurrency.LampOil, new[] { 40, 80, 120 }, 2f / 3f, r => r == 0 ? "—" : Pct(3 * r)),
        };

        public static MetaTrack Track(string id) => Array.Find(Tracks, t => t.Id == id);
        public static IEnumerable<MetaTrack> For(MetaStation station) { foreach (MetaTrack t in Tracks) if (t.Station == station) yield return t; }
        public static string StationName(MetaStation s) => s == MetaStation.NightPact ? "The Night Pact" : s == MetaStation.Lantern ? "The Lamp Tree" : "The " + s;
        public static string StationLine(MetaStation s)
        {
            switch (s)
            {
                case MetaStation.Lantern: return "Upgrades for every run";
                case MetaStation.Ledger: return "Money";
                case MetaStation.Nursery: return "Friends";
                case MetaStation.Wardrobe: return "Hero mastery";
                case MetaStation.Clock: return "Second chances";
                case MetaStation.Postbox: return "Letters";
                case MetaStation.Museum: return "Collection";
                case MetaStation.NightPact: return "Curses for more";
                case MetaStation.Window: return "Daily Dream";
                case MetaStation.Bell: return "Keeper's favour";
                default: return "";
            }
        }

        // ------------------------------------------------------------------ profile
        /// <summary>Makes the block whole (older profiles, hand-edited saves) and runs the one-time migration.</summary>
        /// <summary>The eight old difficulties' win bits on the five now (Restless counts as Wandering, Beyond the Lamp as Sleepless,
        /// Lightless as The Last Night).</summary>
        private static int RemapDifficultyBits(int old)
        {
            int[] to = { 0, 1, 1, 2, 3, 3, 4, 4 };
            int bits = 0;
            for (int i = 0; i < to.Length; i++) if ((old & (1 << i)) != 0) bits |= 1 << to[i];
            return bits;
        }
        public static MetaProgress Ensure(PlayerProfile profile)
        {
            MetaProgress m = profile.Meta ??= new MetaProgress();
            m.Ranks ??= new List<MetaRank>(); m.Found ??= new List<string>(); m.Mastery ??= new List<HeroMastery>();
            m.Heirloom ??= ""; m.PaidRun ??= ""; m.Tree ??= new List<string>(); m.Lamps ??= new List<MetaRank>();
            if (m.Revision < 3) RefundRetired(m); // before the old ranks are dropped below
            if (m.Revision < 5) { m.WonMask = RemapDifficultyBits(m.WonMask); } // the difficulties went from eight to five (27/10/2026)
            MoveLitIntoRanks(m);
            { var lit = new HashSet<string>(); m.Lamps.RemoveAll(r => r == null || Node(r.Id) == null || !lit.Add(r.Id)); }
            foreach (MetaRank r in m.Lamps) r.Rank = Mathf.Clamp(r.Rank, 0, Node(r.Id).Ranks);
            m.Lamps.RemoveAll(r => r.Rank <= 0);
            if (m.WonMask == 0 && profile.Wins > 0) m.WonMask = 1; // a profile from before: its wins count as one difficulty
            m.LampOil = Mathf.Max(0, m.LampOil); m.Keepsakes = Mathf.Max(0, m.Keepsakes);
            m.LampOilEarned = Mathf.Max(0, m.LampOilEarned); m.KeepsakesEarned = Mathf.Max(0, m.KeepsakesEarned);
            m.DuetsFound = Mathf.Max(0, m.DuetsFound); m.PaidOil = Mathf.Max(0, m.PaidOil); m.PaidWaves = Mathf.Max(0, m.PaidWaves);
            m.Ranks.RemoveAll(r => r == null || Track(r.Id) == null);
            var seen = new HashSet<string>();
            m.Ranks.RemoveAll(r => !seen.Add(r.Id));
            foreach (MetaRank r in m.Ranks) r.Rank = Mathf.Clamp(r.Rank, 0, Track(r.Id).Max);
            m.Mastery.RemoveAll(h => h == null || string.IsNullOrEmpty(h.Hero));
            foreach (HeroMastery h in m.Mastery) { h.Waves = Mathf.Max(0, h.Waves); h.Wins = Mathf.Max(0, h.Wins); }
            m.Found.RemoveAll(string.IsNullOrEmpty);
            EnsureStations(m);
            if (m.Revision < 1)
            {
                // an old profile starts at zero with Lamp Oil for the best wave it ever reached
                int grant = Mathf.Clamp(profile.BestWave, 0, 1000) * OilPerBestWave;
                m.LampOil += grant; m.LampOilEarned += grant;
            }
            m.Revision = MetaProgress.CurrentRevision;
            return m;
        }

        public static int Rank(PlayerProfile profile, string id) => profile?.Meta?.Ranks?.Find(r => r.Id == id)?.Rank ?? 0;
        private static void SetRank(MetaProgress m, string id, int rank)
        {
            MetaRank r = m.Ranks.Find(x => x.Id == id);
            if (r == null) m.Ranks.Add(r = new MetaRank { Id = id });
            r.Rank = rank;
        }
        public static int Wallet(PlayerProfile profile, MetaCurrency currency) => currency == MetaCurrency.LampOil ? profile.Meta.LampOil : profile.Meta.Keepsakes;
        public static int StationRanks(PlayerProfile profile, MetaStation station)
        {
            if (station >= MetaStation.Postbox) return StationLevel(profile, station);
            if (station == MetaStation.Lantern) return profile.Meta.Lamps.Count;
            if (station == MetaStation.Wardrobe) { int best = 0; foreach (HeroMastery h in profile.Meta.Mastery) best = Mathf.Max(best, MasteryRank(h.Waves)); return best; }
            int sum = 0; foreach (MetaTrack t in For(station)) sum += Rank(profile, t.Id); return sum;
        }
        public static bool Restored(PlayerProfile profile, MetaStation station) => Works(station) && StationRanks(profile, station) > 0;

        /// <summary>Why a rank cannot be bought now (null: it can).</summary>
        public static string Blocked(PlayerProfile profile, MetaTrack track)
        {
            Ensure(profile);
            int rank = Rank(profile, track.Id);
            if (rank >= track.Max) return "MAX";
            if (track.Id == "ledger.shelf")
                foreach (MetaTrack t in For(MetaStation.Ledger)) if (t != track && Rank(profile, t.Id) < t.Max) return "Fill the Ledger first";
            if (CombatPower(profile) + track.Power > PowerCap + .001f) return "Power cap";
            if (Wallet(profile, track.Currency) < track.Cost[rank]) return track.Currency == MetaCurrency.LampOil ? "Not enough oil" : "Not enough keepsakes";
            return null;
        }
        public static bool Buy(PlayerProfile profile, string id)
        {
            MetaTrack track = Track(id);
            if (track == null || Blocked(profile, track) != null) return false;
            MetaProgress m = profile.Meta;
            int rank = Rank(profile, id), cost = track.Cost[rank];
            if (track.Currency == MetaCurrency.LampOil) m.LampOil -= cost; else m.Keepsakes -= cost;
            SetRank(m, id, rank + 1);
            return true;
        }
        /// <summary>Permanent combat power from the profile, in percent (never above <see cref="PowerCap"/>).</summary>
        public static float CombatPower(PlayerProfile profile)
        {
            float sum = 0; foreach (LampNode n in Tree) sum += n.Power * LampRank(profile, n.Id);
            return sum;
        }
        /// <summary>The most a tree can add (each fork at its stronger side).</summary>
        public static float MaxCombatPower { get { float sum = 0; foreach (LampNode n in Tree) { LampNode f = Fork(n); if (f == null || n.Power * n.Ranks > f.Power * f.Ranks || n.Power * n.Ranks == f.Power * f.Ranks && string.CompareOrdinal(n.Id, f.Id) < 0) sum += n.Power * n.Ranks; } return sum; } }

        // ------------------------------------------------------------------ wardrobe (hero mastery)
        public static HeroMastery Mastery(PlayerProfile profile, string hero, bool create = false)
        {
            MetaProgress m = Ensure(profile);
            HeroMastery h = m.Mastery.Find(x => x.Hero == hero);
            if (h == null && create) m.Mastery.Add(h = new HeroMastery { Hero = hero });
            return h;
        }
        public static int MasteryRank(int waves) { int r = 0; while (r < MasteryWaves.Length && waves >= MasteryWaves[r]) r++; return r; }
        public static int MasteryRank(PlayerProfile profile, string hero) => MasteryRank(Mastery(profile, hero)?.Waves ?? 0);
        public static readonly string[] MasteryNames = { "Spare coat", "Warm gauge", "Second habit" };
        public static readonly string[] MasteryLines = { "Another starting friend", "Gauge starts 25% full", "Swap a hero & friends passive" };

        /// <summary>The hero's other starting friend (Wardrobe rank 1): the first friend of its own themes that is not its usual one.</summary>
        public static UnitDefinition AltStarter(RunCatalog catalog, UnitDefinition champion)
        {
            if (champion == null || champion.HeroMechanic == HeroMechanic.SoloMaw || catalog.HeroRevision == 0) return null;
            RosterEntry usual = K11HeroRules.Starter(catalog, champion);
            foreach (UnitDefinition unit in catalog.Companions)
                if (unit.Id != usual.UnitId && StarterEligible(catalog, champion, unit)) return unit;
            return null;
        }
        private static bool StarterEligible(RunCatalog catalog, UnitDefinition champion, UnitDefinition unit)
            => unit != null && !unit.SpecialRecruit && !RunSession.StorySpecies(unit.Id) && string.IsNullOrEmpty(unit.ExclusiveHero) && unit.UnlockWave <= 1
               && unit.Rarity <= ItemRarity.Uncommon && K11HeroRules.Eligible(champion, unit);

        /// <summary>The Nursery's three friends for the next run with this hero (they change with every run started).</summary>
        public static List<UnitDefinition> NurseryChoices(PlayerProfile profile, RunCatalog catalog, UnitDefinition champion)
        {
            var result = new List<UnitDefinition>();
            if (!Lit(profile, "friends.cradle") || champion == null || champion.HeroMechanic == HeroMechanic.SoloMaw || catalog.HeroRevision == 0) return result;
            RosterEntry usual = K11HeroRules.Starter(catalog, champion);
            var pool = new List<UnitDefinition>();
            foreach (UnitDefinition unit in catalog.Companions) if (unit.Id != usual.UnitId && StarterEligible(catalog, champion, unit)) pool.Add(unit);
            uint state = unchecked((uint)(profile.Meta.NurseryRoll * 2654435761u) ^ (uint)champion.Id.GetHashCode() ^ 0x5EED1u);
            while (pool.Count > 0 && result.Count < 3)
            {
                state ^= state << 13; state ^= state >> 17; state ^= state << 5;
                int pick = (int)(state % (uint)pool.Count);
                result.Add(pool[pick]); pool.RemoveAt(pick);
            }
            return result;
        }

        /// <summary>Second habit: the hero's first team passive is swapped for a plain one of the same size.</summary>
        public static KitTrait? HabitDropped(HeroKit kit) { if (kit != null) foreach (KitTrait t in kit.Traits) if (!t.Hero) return t; return null; }
        public static KitTrait HabitAdded(HeroKit kit)
        {
            KitTrait? dropped = HabitDropped(kit);
            return dropped.HasValue && dropped.Value.Kind == KitPassive.TeamAttackSpeed
                ? new KitTrait(KitPassive.TeamDamage, .05f, "Second habit: friends +5% damage.", false)
                : new KitTrait(KitPassive.TeamAttackSpeed, .05f, "Second habit: friends +5% attack speed.", false);
        }
        private static readonly Dictionary<string, HeroKit> habits = new Dictionary<string, HeroKit>();
        public static HeroKit Habit(HeroKit kit, bool on)
        {
            if (!on || kit == null || !HabitDropped(kit).HasValue) return kit;
            if (habits.TryGetValue(kit.Id, out HeroKit swapped)) return swapped;
            var traits = new List<KitTrait>(kit.Traits);
            traits.Remove(HabitDropped(kit).Value); traits.Add(HabitAdded(kit));
            return habits[kit.Id] = kit.WithTraits(traits.ToArray());
        }

        // ------------------------------------------------------------------ the run's loadout
        /// <summary>Freezes the profile into a run: stats scaled so the permanent combat power never passes the cap, unlocks,
        /// and the run-start choices made in hero select (checked here; an invalid choice is simply left out).</summary>
        public static MetaRunState Loadout(PlayerProfile profile, RunCatalog catalog, string championId, string startFriend = null, bool altStarter = false, bool secondHabit = false, bool advanceRoll = true, string keepsake = null, string daily = null)
        {
            Ensure(profile);
            var s = new MetaRunState();
            s.Endgame = HeroUnlocks.EndgameTier(profile);
            StationLoadout(profile, catalog, s, keepsake, daily);
            MetaRunState tree = TreeLoadout(profile, out _);
            s.Health = tree.Health;
            s.MoveSpeed = tree.MoveSpeed;
            s.Crit = tree.Crit;
            s.TwoStarChance = tree.TwoStarChance;
            s.Damage += tree.Damage;
            s.Gold += tree.Gold;
            s.Stats = tree.Stats;
            s.WaveShield = tree.WaveShield; s.EnemyWeaken = tree.EnemyWeaken; s.RerollDiscount = tree.RerollDiscount;
            s.DreamSooner = tree.DreamSooner; s.DreamExtra = tree.DreamExtra; s.RegularDeal = tree.RegularDeal;
            s.Pickup = tree.Pickup;
            s.StartGold = tree.StartGold;
            s.FreeRefresh = tree.FreeRefresh;
            s.Interest = tree.Interest;
            s.ChestGold = tree.ChestGold;
            s.ChestQuality = tree.ChestQuality;
            s.ExtraSlots = tree.ExtraSlots;
            s.CandleShare = tree.CandleShare;
            s.CandleShield = tree.CandleShare > 0 ? tree.CandleShield : 0;
            s.RewindOwned = tree.RewindOwned;
            s.FirstFriendStar = tree.FirstFriendStar;
            s.FirstShopRare = tree.FirstShopRare;
            if (tree.Heirloom.Length > 0 && catalog != null && Array.Exists(catalog.Bonds, b => b.Id == profile.Meta.Heirloom)) s.Heirloom = profile.Meta.Heirloom;
            UnitDefinition champion = catalog?.Unit(championId);
            if (champion != null)
            {
                if (!string.IsNullOrEmpty(startFriend) && NurseryChoices(profile, catalog, champion).Exists(u => u.Id == startFriend)) s.StartFriend = startFriend;
                int mastery = MasteryRank(profile, championId);
                if (altStarter && mastery >= 1 && AltStarter(catalog, champion) is UnitDefinition alt) s.AltStarter = alt.Id;
                // the Wardrobe's warm gauge or the tree's Ready flame (a third: one tier), plus Quick and Keen dream
                if (HeroKits.For(championId) != null) s.GaugeStart = Mathf.Min(.6f, Mathf.Max(mastery >= 2 ? .25f : 0, Lit(profile, "hero.crown") ? .34f : 0) + tree.GaugeStart);
                s.SecondHabit = secondHabit && mastery >= 3 && HabitDropped(HeroKits.For(championId)).HasValue;
                HeroMastery h = Mastery(profile, championId, true); h.AltStarter = altStarter; h.SecondHabit = secondHabit;
            }
            if (advanceRoll) profile.Meta.NurseryRoll++;
            return s;
        }

        /// <summary>A profile at a share of the meta (0, half, all) for the bot sweep: every track at that share of its ranks
        /// (rounded half up; the Ledger's last rank only when the rest is full), the Wardrobe likewise for the hero.</summary>
        public static PlayerProfile BotProfile(float level, RunCatalog catalog, string hero)
        {
            var p = new PlayerProfile(); MetaProgress m = Ensure(p);
            int tiers = Mathf.FloorToInt(TreeTiers * level + .5f);
            foreach (LampNode n in Tree) if (n.Tier < tiers && (Fork(n) == null || Array.IndexOf(Tree, n) < Array.IndexOf(Tree, Fork(n)))) m.Lamps.Add(new MetaRank { Id = n.Id, Rank = Mathf.Max(1, Mathf.FloorToInt(n.Ranks * level + .5f)) });
            if (level >= 1) m.WonMask = 1;
            UnitDefinition champion = catalog.Unit(hero);
            if (champion != null && champion.RecruitThemes != null && champion.RecruitThemes.Length > 0) m.Heirloom = champion.RecruitThemes[0];
            int ranks = Mathf.FloorToInt(MasteryWaves.Length * level + .5f);
            if (ranks > 0) Mastery(p, hero, true).Waves = MasteryWaves[ranks - 1];
            return p;
        }
        public static MetaRunState BotLoadout(float level, RunCatalog catalog, string hero)
        {
            if (level <= 0) return new MetaRunState();
            PlayerProfile p = BotProfile(level, catalog, hero);
            List<UnitDefinition> choices = NurseryChoices(p, catalog, catalog.Unit(hero));
            return Loadout(p, catalog, hero, choices.Count > 0 ? choices[0].Id : null, false, level >= 1, false);
        }

        // ------------------------------------------------------------------ earning
        public static int WavesCleared(RunSnapshot run)
        {
            int cleared = Mathf.Max(0, run.Wave - Mathf.Max(1, run.StartHall));
            if (run.Phase == SessionPhase.Victory || run.Phase == SessionPhase.Shop) cleared++;
            return cleared;
        }
        public static bool KeeperBeaten(RunSnapshot run) => run.OutsideStage == OutsideStageStatus.Completed || run.OutsideStage == OutsideStageStatus.Reward && run.OutsideBeforeGift < 0;

        /// <summary>What a finished run earns (Lamp Oil always; Keepsakes only for first times not yet in the profile).
        /// Pure: the profile is read, never changed.</summary>
        public static (int oil, int keepsakes, List<string> lines, List<string> found, int duets) Haul(PlayerProfile profile, RunSnapshot run, RunCatalog catalog)
        {
            Ensure(profile);
            var lines = new List<string>(); var found = new List<string>();
            int d = Mathf.Clamp(run.Difficulty, 0, DifficultyOil.Length - 1);
            int waves = WavesCleared(run), bosses = Mathf.Max(0, run.BossKills);
            MetaRunState meta = run.Meta ?? new MetaRunState();
            int finds = Mathf.Min(FindsCap, 3 * meta.FoundUnits + meta.FoundItems + 10 * meta.FoundBosses) * (Lit(profile, "dream.finds") ? 2 : 1);
            int baseOil = waves * OilPerWave + bosses * OilPerBoss;
            int oil = Mathf.RoundToInt(baseOil * DifficultyOil[d]) + finds;
            if (waves > 0) lines.Add("oil|Waves survived ×" + waves + "|" + waves * OilPerWave);
            if (bosses > 0) lines.Add("oil|Bosses beaten ×" + bosses + "|" + bosses * OilPerBoss);
            if (d > 0 && baseOil > 0) lines.Add("oil|" + RunMeta.DifficultyNames[d] + " ×" + DifficultyOil[d].ToString("0.0#") + "|" + (Mathf.RoundToInt(baseOil * DifficultyOil[d]) - baseOil));
            if (finds > 0) lines.Add("oil|New finds|" + finds);
            float bright = TreeOil(profile);
            if (bright > 0 && oil > 0) { int more = Mathf.RoundToInt(oil * bright); lines.Add("oil|Bright oil +" + Mathf.RoundToInt(bright * 100) + "%|" + more); oil += more; }
            float pact = PactMultiplier(meta.Pacts), favour = FavourMultiplier(profile);
            if (pact > 1 && oil > 0) { int more = Mathf.RoundToInt(oil * pact) - oil; lines.Add("oil|Night Pact ×" + pact.ToString("0.0#") + "|" + more); oil += more; }
            if (favour > 1 && oil > 0) { int more = Mathf.RoundToInt(oil * favour) - oil; lines.Add("oil|Favour ★" + profile.Meta.Favour + "|" + more); oil += more; }
            int keepsakes = 0;
            foreach (string boss in run.DefeatedBosses ?? new List<string>())
            {
                string key = "boss/" + d + "/" + boss;
                if (profile.Meta.Found.Contains(key) || found.Contains(key)) continue;
                found.Add(key); keepsakes++;
                string bossName = catalog?.Bosses == null ? null : Array.Find(catalog.Bosses, b => b != null && b.Id == boss)?.DisplayName;
                lines.Add("keep|First " + RunUICopy.UnitName(boss, bossName ?? catalog?.Unit(boss)?.DisplayName) + " · " + RunMeta.DifficultyNames[d] + "|1");
            }
            if (KeeperBeaten(run) && !profile.Meta.Found.Contains("keeper/" + d)) { found.Add("keeper/" + d); keepsakes += 2; lines.Add("keep|The Keeper · " + RunMeta.DifficultyNames[d] + "|2"); }
            int secrets = 0;
            int mask = run.Secrets?.VisitedMask ?? 0;
            for (int k = 0; k < 8; k++)
                if ((mask & (1 << k)) != 0 && !profile.Meta.Found.Contains("secret/" + k)) { found.Add("secret/" + k); secrets++; }
            if (secrets > 0) { keepsakes += secrets; lines.Add("keep|Secret rooms found ×" + secrets + "|" + secrets); }
            // requirement 27/10/2026: Endless nights are milestones, each paid once; the first two open endgame relics in the shops
            if (run.Endless || run.Wave > 20)
                foreach ((int night, int reward) in HeroUnlocks.EndlessMilestones)
                {
                    string key = "endless/" + night;
                    if (run.Wave < night || profile.Meta.Found.Contains(key) || found.Contains(key)) continue;
                    found.Add(key); keepsakes += reward;
                    lines.Add("keep|Endless night " + night + (night == 25 ? " · new relics in the shops" : night == 40 ? " · more relics in the shops" : "") + "|" + reward);
                }
            int duets = 0;
            foreach (Duet duet in Duets.All)
            {
                if (!run.Roster.Exists(r => r.UnitId == duet.A) || !run.Roster.Exists(r => r.UnitId == duet.B)) continue;
                string key = "duet/" + duet.A + "+" + duet.B;
                if (profile.Meta.Found.Contains(key) || found.Contains(key)) continue;
                found.Add(key); duets++;
            }
            int duetKeepsakes = (profile.Meta.DuetsFound + duets) / 3 - profile.Meta.DuetsFound / 3;
            if (duets > 0) lines.Add("keep|Combos found ×" + duets + "|" + duetKeepsakes);
            keepsakes += duetKeepsakes;
            if (!string.IsNullOrEmpty(meta.Daily) && !profile.Meta.Found.Contains("daily/" + meta.Daily)) { found.Add("daily/" + meta.Daily); keepsakes += DailyKeepsakes; lines.Add("keep|Daily Dream|" + DailyKeepsakes); }
            if (waves >= 10 && Lit(profile, "dream.crown")) { keepsakes++; lines.Add("keep|Keepsake dreams|1"); }
            if (pact > 1 && keepsakes > 0) { int more = Mathf.RoundToInt(keepsakes * pact) - keepsakes; if (more > 0) { keepsakes += more; lines.Add("keep|Night Pact ×" + pact.ToString("0.0#") + "|" + more); } }
            return (oil, keepsakes, lines, found, duets);
        }

        /// <summary>Counts the first-ever finds the profile is about to learn from this run (called before RunMeta.Discover).</summary>
        public static void CountFinds(PlayerProfile profile, RunSnapshot run)
        {
            if (run?.Meta == null) return;
            var units = new HashSet<string>(); var items = new HashSet<string>(); var bosses = new HashSet<string>();
            foreach (RosterEntry r in run.Roster) if (!profile.SeenUnits.Contains(r.UnitId)) units.Add(r.UnitId);
            foreach (ItemStack s in run.Items) if (!profile.SeenItems.Contains(s.Id)) items.Add(s.Id);
            foreach (string b in run.DefeatedBosses ?? new List<string>()) if (!profile.SeenBosses.Contains(b)) bosses.Add(b);
            run.Meta.FoundUnits += units.Count; run.Meta.FoundItems += items.Count; run.Meta.FoundBosses += bosses.Count;
        }

        /// <summary>Pays a finished run once: Lamp Oil (only what a rewound run adds), new Keepsakes, Wardrobe waves; the haul is
        /// written into the run for its summary. True when something was credited.</summary>
        public static bool Credit(PlayerProfile profile, RunSnapshot run, RunCatalog catalog)
        {
            if (run == null || run.Phase != SessionPhase.Victory && run.Phase != SessionPhase.Defeat || profile.CompletedRuns.Contains(run.RunId)) return false;
            MetaProgress m = Ensure(profile);
            run.Meta ??= new MetaRunState();
            var (oil, keepsakes, lines, found, duets) = Haul(profile, run, catalog);
            int waves = WavesCleared(run);
            if (m.PaidRun != run.RunId) { m.PaidRun = run.RunId; m.PaidOil = 0; m.PaidWaves = 0; }
            int payOil = Mathf.Max(0, oil - m.PaidOil), payWaves = Mathf.Max(0, waves - m.PaidWaves);
            m.PaidOil = Mathf.Max(m.PaidOil, oil); m.PaidWaves = Mathf.Max(m.PaidWaves, waves);
            m.LampOil = RunSession.BoundedCounter((long)m.LampOil + payOil); m.LampOilEarned = RunSession.BoundedCounter((long)m.LampOilEarned + payOil);
            m.Keepsakes = RunSession.BoundedCounter((long)m.Keepsakes + keepsakes); m.KeepsakesEarned = RunSession.BoundedCounter((long)m.KeepsakesEarned + keepsakes);
            foreach (string key in found) RunMeta.Add(m.Found, key);
            if (run.Phase == SessionPhase.Victory) m.WonMask |= 1 << Mathf.Clamp(run.Difficulty, 0, 30);
            m.DuetsFound += duets;
            if (payWaves > 0) { HeroMastery h = Mastery(profile, run.ChampionId, true); h.Waves = RunSession.BoundedCounter((long)h.Waves + payWaves); }
            if (run.Phase == SessionPhase.Victory && !run.Endless) { HeroMastery won = Mastery(profile, run.ChampionId, true); won.Wins = RunSession.BoundedCounter((long)won.Wins + 1); }
            run.Meta.HaulOil = oil; run.Meta.HaulKeepsakes = keepsakes; run.Meta.HaulLines = lines;
            RecordScores(profile, run);
            return true;
        }

        /// <summary>Rewind: the lost run is open again (its history entry and kills come off; Lamp Oil already paid stays paid,
        /// the run's next ending pays only what it adds).</summary>
        public static void Reopen(PlayerProfile profile, RunSnapshot lost)
        {
            if (lost == null || !profile.CompletedRuns.Remove(lost.RunId)) return;
            profile.History.RemoveAll(r => r.Id == lost.RunId);
            profile.TotalKills = Mathf.Max(0, profile.TotalKills - lost.TotalKills);
        }
    }
}
