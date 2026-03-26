using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The enemy roster v2 (Docs/Enemy_Roster_v2.md): the eight new enemies of the Waiting Realm, plus the pieces a
    /// Sewn Letter bursts into. None on an ordinary (older) species.</summary>
    public enum WaitingEnemyKind { None, SpareChair, WakingSitter, InkNotary, SewnLetter, PartyLine, DoorKnocker, LampSnuffer, LedgerHound, LetterHalf, LetterStamp }

    /// <summary>Painted art for one species of the v2 roster: a new enemy, or an older species it re-themes (the older id keeps
    /// its behaviour, stats and save identity; only the look and the shown name change).</summary>
    [Serializable]
    public sealed class WaitingEnemyArt
    {
        public string Id;          // the unit id this art dresses (CoinLeech for the Toll Tick art)
        public string ArtId;       // the art file stem (Art/Expansion/Enemies/<ArtId>_v1.png)
        public string DisplayName; // shown name, empty to keep the unit's own
        public AuthoredMotionSet Normal;
        public AuthoredMotionSet Evolved;
    }

    /// <summary>What the enemy roster builder (Editor/EnemyRosterBuilder) has imported. An enemy whose art is missing is not
    /// listed and stays out of the game; a re-themed species without its new art keeps its old look.</summary>
    public sealed class WaitingEnemyRoster : ScriptableObject
    {
        public UnitDefinition[] Enemies = Array.Empty<UnitDefinition>();
        public WaitingEnemyArt[] Art = Array.Empty<WaitingEnemyArt>();
        public AuthoredMotionSet LetterHalf;   // SewnLetterPieces_v1 row 1 (the letter's own art, smaller, until it exists)
        public AuthoredMotionSet LetterStamp;  // SewnLetterPieces_v1 row 2
        public Sprite IndebtedStamp;           // IndebtedStamp_v1: the receipt-ink mark on an Indebted elite

        public static readonly string[] NewIds = { "SpareChair", "WakingSitter", "InkNotary", "SewnLetter", "PartyLine", "DoorKnocker", "LampSnuffer", "LedgerHound" };
        public static readonly string[] RethemedIds = { "CoinLeech", "SporeBishop", "SmokingCoffin", "HopperKeg", "BristleImp" };
        public static readonly string[] RethemeArt = { "TollTick", "CoatRack", "UnclaimedTrunk", "HatboxHopper", "SplinterImp" };
        public static readonly string[] RethemeNames = { "Toll Tick", "Coat Rack", "Unclaimed Trunk", "Hatbox Hopper", "Splinter Imp" };

        private static WaitingEnemyRoster cached;
        private static bool looked;
        public static WaitingEnemyRoster Current
        {
            get
            {
                if (cached != null || looked) return cached;
                looked = true; cached = Resources.Load<WaitingEnemyRoster>("WaitingEnemyRoster");
                return cached;
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() { cached = null; looked = false; }

        /// <summary>First and last regular wave of each new enemy (its zone: Platform 6-10, House 11-15, Perfect Room 16-20).</summary>
        public static int From(WaitingEnemyKind kind) => kind switch
        {
            WaitingEnemyKind.SpareChair or WaitingEnemyKind.SewnLetter => 6,
            WaitingEnemyKind.PartyLine => 8,
            WaitingEnemyKind.LampSnuffer => 9,
            WaitingEnemyKind.DoorKnocker => 11,
            WaitingEnemyKind.InkNotary or WaitingEnemyKind.LedgerHound => 16,
            _ => int.MaxValue
        };
        public static int Until(WaitingEnemyKind kind) => kind switch
        {
            WaitingEnemyKind.SpareChair => 10,
            WaitingEnemyKind.PartyLine => 14,
            WaitingEnemyKind.SewnLetter or WaitingEnemyKind.DoorKnocker => 15,
            WaitingEnemyKind.LampSnuffer or WaitingEnemyKind.InkNotary or WaitingEnemyKind.LedgerHound => 20,
            _ => -1
        };
        /// <summary>Whether this kind walks into an ordinary wave. The Waking Sitter never does ("they are not monsters"): it only
        /// rises in the hidden SIT CLOSER fight. After Act I (endless) every zone's enemies stay once unlocked.</summary>
        public static bool InRegularWaves(WaitingEnemyKind kind, int wave, bool endless)
        {
            if (kind == WaitingEnemyKind.None || kind == WaitingEnemyKind.WakingSitter || kind == WaitingEnemyKind.LetterHalf || kind == WaitingEnemyKind.LetterStamp) return false;
            if (wave < From(kind)) return false;
            return wave <= Until(kind) || endless || wave > ActOneCanon.Waves;
        }
        /// <summary>The wave from which a new enemy wears its evolved form (rows 3-4 of its atlas).</summary>
        public static int EvolvesAt(WaitingEnemyKind kind) => kind switch
        {
            WaitingEnemyKind.SpareChair => 9,
            WaitingEnemyKind.SewnLetter => 11,
            WaitingEnemyKind.PartyLine => 12,
            WaitingEnemyKind.LampSnuffer => 14,
            WaitingEnemyKind.DoorKnocker => 14,
            WaitingEnemyKind.WakingSitter => 14,
            WaitingEnemyKind.InkNotary or WaitingEnemyKind.LedgerHound => 19,
            _ => int.MaxValue
        };

        public WaitingEnemyArt Find(string id)
        {
            if (Art != null) foreach (WaitingEnemyArt art in Art) if (art != null && art.Id == id) return art;
            return null;
        }

        /// <summary>Dresses a run-compiled enemy (RunSession.BuildEnemy, after the K22/K25 forms are chosen): a new enemy takes
        /// its normal or evolved form by wave; a re-themed species keeps its stage and behaviour and takes the new look and name.</summary>
        public static void ApplyArt(UnitDefinition unit, int wave)
        {
            WaitingEnemyRoster roster = Current;
            if (roster == null || unit == null || unit.IsBoss) return;
            WaitingEnemyArt art = roster.Find(unit.Id);
            if (art == null || art.Normal == null || art.Normal.Idle == null || art.Normal.Idle.Length < 2 || art.Normal.Walk == null || art.Normal.Walk.Length < 2) return;
            bool fresh = unit.WaitingKind != WaitingEnemyKind.None;
            if (fresh) unit.EvolutionStage = wave >= EvolvesAt(unit.WaitingKind) && art.Evolved != null ? 1 : 0;
            AuthoredMotionSet motion = unit.EvolutionStage > 0 && art.Evolved != null ? art.Evolved : art.Normal;
            unit.AuthoredMotion = motion;
            unit.Body = motion.Idle[0];
            unit.AnimationFrames = new[] { motion.Idle[0], motion.Idle[1], motion.Walk[0], motion.Walk[1] };
            if (fresh && unit.EvolutionStage > 0) { unit.BodyHeight *= 1.1f; unit.Radius *= 1.05f; }
            if (!string.IsNullOrEmpty(art.DisplayName)) unit.DisplayName = art.DisplayName;
        }
    }
}
