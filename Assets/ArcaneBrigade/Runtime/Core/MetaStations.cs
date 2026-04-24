using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A best score kept by the Waiting Room (a hero's pact record, a day's Daily Dream).</summary>
    [Serializable] public sealed class MetaScore { public string Key = ""; public int Score, Wave, Heat; }

    /// <summary>A letter from the Thursday visitor (the Postbox): a milestone opens it; reading it keeps its relic for the
    /// run-start pick.</summary>
    public sealed class MetaLetter { public string Id, Name, Milestone, Text, Relic; public Func<PlayerProfile, bool> Earned; }

    /// <summary>A Night Pact curse: what it does, how much more Lamp Oil and Keepsakes it pays, its heat.</summary>
    public sealed class MetaPact { public string Id, Name, Line; public float Reward; public int Heat; }

    public enum MuseumShelf { Friends, Relics, Enemies, Bosses, Combos }
    public struct MuseumPiece { public string Id, Name; public Sprite Icon, Second; public bool Seen; }

    /// <summary>The Waiting Room's last five stations (play-test item 27, built 21/10/2026): the Postbox's letters and keepsake
    /// relics, the Museum's shelves and gifts, the Night Pact's curses, the Window's Daily Dream and the Bell's Favour.</summary>
    public static partial class MetaProgression
    {
        private static bool FoundAny(PlayerProfile p, string prefix) => p.Meta.Found.Exists(k => k.StartsWith(prefix, StringComparison.Ordinal));

        private static void EnsureStations(MetaProgress m)
        {
            m.Letters ??= new List<string>(); m.PactBest ??= new List<MetaScore>(); m.DailyBest ??= new List<MetaScore>();
            m.Keepsake ??= "";
            m.Letters.RemoveAll(id => Array.Find(Letters, l => l.Id == id) == null);
            var seen = new HashSet<string>(); m.Letters.RemoveAll(id => !seen.Add(id));
            m.PactBest.RemoveAll(s => s == null || string.IsNullOrEmpty(s.Key));
            m.DailyBest.RemoveAll(s => s == null || string.IsNullOrEmpty(s.Key));
            m.Pacts &= PactMask;
            m.Favour = Mathf.Clamp(m.Favour, 0, FavourMax);
        }

        /// <summary>How far a station of the last five has come (its art is restored above zero).</summary>
        private static int StationLevel(PlayerProfile p, MetaStation station)
        {
            MetaProgress m = p.Meta;
            switch (station)
            {
                case MetaStation.Postbox: return m.Letters?.Count ?? 0;
                case MetaStation.Museum: return (p.SeenUnits.Count + p.SeenItems.Count + p.SeenBosses.Count + m.DuetsFound) / MuseumStep;
                case MetaStation.NightPact: return (m.PactBest?.Count ?? 0) + (m.Pacts != 0 ? 1 : 0);
                case MetaStation.Window: return m.DailyBest?.Count ?? 0;
                case MetaStation.Bell: return m.Favour;
                default: return 0;
            }
        }

        /// <summary>The five stations' part of a run's loadout: the Museum's gifts, the keepsake relic, the pacts or the day.</summary>
        private static void StationLoadout(PlayerProfile profile, RunCatalog catalog, MetaRunState s, string keepsake, string daily)
        {
            var (damage, gold) = MuseumGifts(profile, catalog);
            s.Damage = damage / 100f; s.Gold = gold / 100f;
            s.Daily = daily ?? "";
            s.Pacts = string.IsNullOrEmpty(daily) ? profile.Meta.Pacts & PactMask : 1 << DailyPact(daily);
            if (!string.IsNullOrEmpty(keepsake) && Keepsakes(profile, catalog).Contains(keepsake)) s.Keepsake = keepsake;
            profile.Meta.Keepsake = s.Keepsake;
        }

        /// <summary>Best scores: a hero's pact record (score × the pacts' reward) and the day's Daily Dream.</summary>
        private static void RecordScores(PlayerProfile profile, RunSnapshot run)
        {
            MetaRunState meta = run.Meta; if (meta == null) return;
            int score = RunMeta.Score(run);
            if ((meta.Pacts & PactMask) != 0)
            {
                int pactScore = RunSession.BoundedCounter((long)(score * (double)PactMultiplier(meta.Pacts)));
                Best(profile.Meta.PactBest, run.ChampionId, pactScore, run.Wave, PactHeat(meta.Pacts));
            }
            if (!string.IsNullOrEmpty(meta.Daily))
            {
                Best(profile.Meta.DailyBest, meta.Daily, score, run.Wave, PactHeat(meta.Pacts));
                profile.Meta.DailyBest.Sort((a, b) => string.CompareOrdinal(b.Key, a.Key));
                if (profile.Meta.DailyBest.Count > 30) profile.Meta.DailyBest.RemoveRange(30, profile.Meta.DailyBest.Count - 30);
            }
        }
        private static void Best(List<MetaScore> list, string key, int score, int wave, int heat)
        {
            MetaScore best = list.Find(s => s.Key == key);
            if (best == null) list.Add(best = new MetaScore { Key = key, Score = -1 });
            if (score > best.Score) { best.Score = score; best.Wave = wave; best.Heat = heat; }
        }

        // ------------------------------------------------------------------ the Postbox
        public static readonly MetaLetter[] Letters =
        {
            new MetaLetter { Id = "letter.boss", Name = "A door, far away", Milestone = "Beat a boss", Relic = "CurioBreathJar", Earned = p => p.SeenBosses.Count > 0 || FoundAny(p, "boss/"),
                Text = "I heard a door slam tonight, far away. Was that you, not hiding? Good.\nI am still coming on Thursday. You do not need to bring anything." },
            new MetaLetter { Id = "letter.ten", Name = "Ten rooms deep", Milestone = "Reach wave 10", Relic = "CurioNoonSpark", Earned = p => p.BestWave >= 10,
                Text = "Ten rooms deep. The lamps there are old, but they remember being lit.\nKeep one near you. I will look for it." },
            new MetaLetter { Id = "letter.combo", Name = "Two who fit", Milestone = "Find a combo", Relic = "CurioRicochetSeltzer", Earned = p => p.Meta.DuetsFound > 0,
                Text = "You found two friends who fit together.\nThat is how it works. Nobody has to be the whole light." },
            new MetaLetter { Id = "letter.secret", Name = "Not on any map", Milestone = "Open a secret room", Relic = "CurioFangPin", Earned = p => FoundAny(p, "secret/"),
                Text = "Some doors are not on any map. You opened one.\nI have knocked on a few of those myself." },
            new MetaLetter { Id = "letter.dawn", Name = "The curtains moved", Milestone = "See the dawn", Relic = "CurioBentTape", Earned = p => p.Wins > 0,
                Text = "Morning. I saw the curtains move.\nI knocked eight times over the years. This time someone answered." },
            new MetaLetter { Id = "letter.keeper", Name = "The hidden one", Milestone = "Beat the Keeper", Relic = "CurioInkFeather", Earned = p => FoundAny(p, "keeper/"),
                Text = "The hand that kept my letters is open now. Here is one it hid.\nI came on Thursday. I always came." },
        };
        public static MetaLetter Letter(string id) => Array.Find(Letters, l => l.Id == id);
        public static bool LetterRead(PlayerProfile p, MetaLetter letter) => Ensure(p).Letters.Contains(letter.Id);
        public static bool LetterEarned(PlayerProfile p, MetaLetter letter) { Ensure(p); return letter.Earned(p); }
        /// <summary>Opens a letter whose milestone is met (its relic joins the keepsakes). True when it was opened now.</summary>
        public static bool ReadLetter(PlayerProfile p, string id)
        {
            MetaLetter letter = Letter(id);
            if (letter == null || !LetterEarned(p, letter) || LetterRead(p, letter)) return false;
            p.Meta.Letters.Add(id); return true;
        }
        /// <summary>The keepsake relics of the letters read (only relics the catalog has, when one is given).</summary>
        public static List<string> Keepsakes(PlayerProfile p, RunCatalog catalog)
        {
            Ensure(p); var list = new List<string>();
            foreach (MetaLetter l in Letters) if (p.Meta.Letters.Contains(l.Id) && (catalog == null || catalog.Item(l.Relic) != null)) list.Add(l.Relic);
            return list;
        }

        // ------------------------------------------------------------------ the Museum
        public const int MuseumStep = 10;
        public static readonly string[] ShelfNames = { "FRIENDS", "RELICS", "ENEMIES", "BOSSES", "COMBOS" };
        /// <summary>One shelf: everything there is, seen or still a silhouette. Enemies count as met once their first wave is reached.</summary>
        public static List<MuseumPiece> Shelf(PlayerProfile p, RunCatalog c, MuseumShelf shelf)
        {
            Ensure(p); var list = new List<MuseumPiece>();
            if (c == null) return list;
            switch (shelf)
            {
                case MuseumShelf.Friends:
                    foreach (UnitDefinition u in c.Companions) if (u != null && u.Body != null && !RunSession.StorySpecies(u.Id)) list.Add(new MuseumPiece { Id = u.Id, Name = RunUICopy.UnitName(u), Icon = u.Body, Seen = p.SeenUnits.Contains(u.Id) });
                    break;
                case MuseumShelf.Relics:
                    foreach (ItemDefinition i in c.Items) if (i != null && i.Icon != null) list.Add(new MuseumPiece { Id = i.Id, Name = i.Name, Icon = i.Icon, Seen = p.SeenItems.Contains(i.Id) });
                    break;
                case MuseumShelf.Enemies:
                    foreach (UnitDefinition u in c.Enemies) if (u != null && u.Body != null) list.Add(new MuseumPiece { Id = u.Id, Name = RunUICopy.UnitName(u), Icon = u.Body, Seen = p.BestWave >= Mathf.Max(1, u.UnlockWave) });
                    break;
                case MuseumShelf.Bosses:
                    foreach (UnitDefinition u in c.Bosses) if (u != null && u.Body != null) list.Add(new MuseumPiece { Id = u.Id, Name = RunUICopy.UnitName(u), Icon = u.Body, Seen = p.SeenBosses.Contains(u.Id) });
                    break;
                case MuseumShelf.Combos:
                    foreach (Duet d in Duets.All)
                    {
                        UnitDefinition a = c.Unit(d.A), b = c.Unit(d.B);
                        if (a == null || b == null) continue;
                        list.Add(new MuseumPiece { Id = d.A + "+" + d.B, Name = RunUICopy.UnitName(a) + " + " + RunUICopy.UnitName(b), Icon = a.Body, Second = b.Body, Seen = p.Meta.Found.Contains("duet/" + d.A + "+" + d.B) });
                    }
                    break;
            }
            return list;
        }
        public static int Discoveries(PlayerProfile p, RunCatalog c)
        {
            int n = 0;
            foreach (MuseumShelf shelf in Enum.GetValues(typeof(MuseumShelf))) foreach (MuseumPiece piece in Shelf(p, c, shelf)) if (piece.Seen) n++;
            return n;
        }
        /// <summary>The Museum's gifts in percent: every ten finds +1% damage or +1% gold, in turn (damage first). Damage only
        /// fills what the Lantern and Nursery leave of the +25% cap; gold stops at 25.</summary>
        public static (int damage, int gold) MuseumGifts(PlayerProfile p, RunCatalog c)
        {
            int gifts = Discoveries(p, c) / MuseumStep;
            int damage = (gifts + 1) / 2, gold = gifts / 2;
            int room = Mathf.Max(0, Mathf.FloorToInt(PowerCap - CombatPower(p) + .001f));
            return (Mathf.Min(damage, room), Mathf.Min(gold, Mathf.RoundToInt(PowerCap)));
        }
        /// <summary>Named trophies for full sets: each shelf complete, and every combo of one family.</summary>
        public static List<(string name, string line, bool won)> Trophies(PlayerProfile p, RunCatalog c)
        {
            var list = new List<(string, string, bool)>();
            string[] names = { "Old Friends", "Lost and Found", "Night Watch", "Giant Slayer", "The Duet Book" };
            string[] lines = { "Met every friend", "Held every relic", "Met every face in the dark", "Beat every boss", "Found every combo" };
            foreach (MuseumShelf shelf in Enum.GetValues(typeof(MuseumShelf)))
            {
                List<MuseumPiece> pieces = Shelf(p, c, shelf);
                list.Add((names[(int)shelf], lines[(int)shelf], pieces.Count > 0 && pieces.TrueForAll(x => x.Seen)));
            }
            var families = new List<string>();
            foreach (Duet d in Duets.All) if (!string.IsNullOrEmpty(d.Family) && !families.Contains(d.Family)) families.Add(d.Family);
            foreach (string family in families)
            {
                bool all = true;
                foreach (Duet d in Duets.All) if (d.Family == family && !p.Meta.Found.Contains("duet/" + d.A + "+" + d.B)) all = false;
                string title = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(family);
                list.Add((title + " Harmony", "Every " + title + " combo", all));
            }
            return list;
        }

        // ------------------------------------------------------------------ the Night Pact
        public const int PactSwift = 0, PactBareShelf = 1, PactNoDream = 2, PactElites = 3, PactEnrage = 4;
        public static readonly MetaPact[] Pacts =
        {
            new MetaPact { Id = "pact.swift", Name = "Swift night", Line = "Enemies +8% speed", Reward = 1.2f, Heat = 2 },
            new MetaPact { Id = "pact.shelf", Name = "Bare shelf", Line = "One fewer shop offer", Reward = 1.15f, Heat = 1 },
            new MetaPact { Id = "pact.market", Name = "Closed market", Line = "No dream shop", Reward = 1.15f, Heat = 1 },
            new MetaPact { Id = "pact.elites", Name = "Crowned crowd", Line = "Elites every wave", Reward = 1.25f, Heat = 3 },
            new MetaPact { Id = "pact.enrage", Name = "Short temper", Line = "Bosses start enraged", Reward = 1.25f, Heat = 2 },
        };
        public static int PactMask => (1 << Pacts.Length) - 1;
        public static bool PactOn(int mask, int pact) => (mask & (1 << pact)) != 0;
        /// <summary>How much more Lamp Oil and Keepsakes the pacts pay (the product of each pact's reward).</summary>
        public static float PactMultiplier(int mask) { float m = 1; for (int i = 0; i < Pacts.Length; i++) if (PactOn(mask, i)) m *= Pacts[i].Reward; return m; }
        public static int PactHeat(int mask) { int h = 0; for (int i = 0; i < Pacts.Length; i++) if (PactOn(mask, i)) h += Pacts[i].Heat; return h; }
        public static void TogglePact(PlayerProfile p, int pact) { Ensure(p); if (pact >= 0 && pact < Pacts.Length) p.Meta.Pacts ^= 1 << pact; }
        public static MetaScore PactRecord(PlayerProfile p, string hero) => Ensure(p).PactBest.Find(s => s.Key == hero);

        // ------------------------------------------------------------------ the Window (Daily Dream)
        public const int DailyKeepsakes = 2;
        public static string Today => DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        /// <summary>The day's seed (FNV-1a of the date): the same run for everyone on that day.</summary>
        public static int DailySeed(string day)
        {
            uint h = 2166136261u;
            foreach (char ch in day ?? "") { h ^= ch; h *= 16777619u; }
            return (int)(h & 0x7FFFFFFF) | 1;
        }
        public static int DailyPact(string day) => (int)((uint)DailySeed(day) / 7u % (uint)Pacts.Length);
        /// <summary>The day's hero: one of the heroes this profile can play, picked by the day's seed.</summary>
        public static UnitDefinition DailyHero(PlayerProfile p, RunCatalog c, string day)
        {
            var open = new List<UnitDefinition>();
            foreach (UnitDefinition u in c.Champions) if (u != null && p.BestWave >= u.UnlockWave) open.Add(u);
            return open.Count == 0 ? null : open[(int)((uint)DailySeed(day) / 13u % (uint)open.Count)];
        }
        public static MetaScore DailyRecord(PlayerProfile p, string day) => Ensure(p).DailyBest.Find(s => s.Key == day);
        public static bool DailyPaid(PlayerProfile p, string day) => Ensure(p).Found.Contains("daily/" + day);

        // ------------------------------------------------------------------ the Bell (Keeper's favour)
        public const int FavourMax = 5;
        public const float FavourOil = .1f;
        public static float FavourMultiplier(PlayerProfile p) => 1 + FavourOil * Mathf.Clamp(p?.Meta?.Favour ?? 0, 0, FavourMax);
        /// <summary>What the Bell resets: every Lamp Oil track of the Lantern and the Ledger (the Ledger's Keepsake shelf stays).</summary>
        public static IEnumerable<MetaTrack> BellTracks()
        {
            foreach (MetaTrack t in Tracks) if ((t.Station == MetaStation.Lantern || t.Station == MetaStation.Ledger) && t.Currency == MetaCurrency.LampOil) yield return t;
        }
        public static (int have, int need) BellRanks(PlayerProfile p)
        {
            int have = 0, need = 0; foreach (MetaTrack t in BellTracks()) { have += Rank(p, t.Id); need += t.Max; }
            return (have, need);
        }
        /// <summary>Why the Bell cannot ring now (null: it can): after the first full clear, with the Lantern and Ledger full.</summary>
        public static string BellBlocked(PlayerProfile p)
        {
            Ensure(p);
            if (p.Wins < 1) return "See the dawn first";
            if (p.Meta.Favour >= FavourMax) return "MAX";
            var (have, need) = BellRanks(p);
            return have < need ? "Fill the Lantern and Ledger" : null;
        }
        /// <summary>Rings the Bell: the Lantern and Ledger ranks reset for one Favour star.</summary>
        public static bool RingBell(PlayerProfile p)
        {
            if (BellBlocked(p) != null) return false;
            foreach (MetaTrack t in BellTracks()) SetRank(p.Meta, t.Id, 0);
            p.Meta.Favour++;
            return true;
        }
    }
}
