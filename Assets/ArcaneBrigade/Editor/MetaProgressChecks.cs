using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Play-test item 27, "The Waiting Room": the profile's meta block (save, load, migration of old profiles), the
    /// +25% cap on permanent combat power, every working station's effect in a run, Second Candle and Rewind once per run,
    /// the haul a run earns, old run saves, and the hub's art. Also the phase E batch (the suites its changes touch).</summary>
    public static class MetaProgressChecks
    {
        [Serializable] private sealed class Envelope { public string Payload; public string Hash; }
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string label)
        {
            notes.Add((ok ? "PASS " : "FAIL ") + label);
            Directory.CreateDirectory("PreviewExports/MetaProgress");
            File.WriteAllLines("PreviewExports/MetaProgress/Checks.txt", notes);
            if (!ok) throw new InvalidOperationException(label);
        }
        private static RunSession Start(RunCatalog c, string hero, int difficulty = 0, int seed = 161026)
        {
            var s = new RunSession(c); s.Start(hero, seed, difficulty); return s;
        }
        private static void Shop(RunSession s)
        {
            s.State.Phase = SessionPhase.Shop;
            typeof(RunSession).GetMethod("RollShop", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(s, new object[] { true });
        }
        private static string Without(string json, string key)
        {
            int at = json.IndexOf("\"" + key + "\":{", StringComparison.Ordinal);
            if (at < 0) return json;
            int i = json.IndexOf('{', at), depth = 0;
            for (; i < json.Length; i++) { if (json[i] == '{') depth++; else if (json[i] == '}' && --depth == 0) break; }
            int start = at, end = i + 1;
            if (start > 0 && json[start - 1] == ',') start--; else if (end < json.Length && json[end] == ',') end++;
            return json.Substring(0, start) + json.Substring(end);
        }
        private static void WriteEnvelope(string path, string payload)
        {
            string hash; using (SHA256 sha = SHA256.Create()) hash = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)));
            File.WriteAllText(path, JsonUtility.ToJson(new Envelope { Payload = payload, Hash = hash }), Encoding.UTF8);
        }

        public static void Run()
        {
            notes.Clear();
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            const string hero = "LanternKnight";
            UnitDefinition knight = c.Unit(hero);

            // the hub's art ships (cut into Resources, build-size policy applies)
            Check(Resources.Load<Sprite>("Hub/WaitingRoom") != null, "Hub room backdrop loads");
            foreach (MetaStation station in Enum.GetValues(typeof(MetaStation)))
                Check(Resources.Load<Sprite>("Hub/Stations/" + station + "_Dusty") != null && Resources.Load<Sprite>("Hub/Stations/" + station + "_Lit") != null, "Station art dusty + restored: " + station);
            Check(MetaIcons.LampOil != null && MetaIcons.Keepsake != null, "Lamp Oil and Keepsake icons load");
            Check(MetaProgression.Working.Length == 10 && MetaProgression.Tracks.All(t => MetaProgression.Works(t.Station)), "All ten stations work");
            Check(!File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/RunMenu.cs").Contains("permanent stat growth") && !File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/RunHud.cs").Contains("NO STAT REWARDS"), "The old 'no permanent stat growth' lines are gone");

            // the profile block: save, load, migration
            string dir = Path.Combine(Path.GetTempPath(), "ABMetaChecks_" + Guid.NewGuid().ToString("N"));
            try
            {
                var storage = new RunStorage(dir);
                var p = new PlayerProfile(); MetaProgression.Ensure(p);
                p.Meta.LampOil = 500; p.Meta.Keepsakes = 30; p.Meta.Heirloom = c.Bonds[0].Id;
                Check(MetaProgression.Buy(p, "lantern.wick") && MetaProgression.Buy(p, "clock.candle") && p.Meta.LampOil == 485 && p.Meta.Keepsakes == 27, "Buying ranks takes the right currency and price");
                MetaProgression.Mastery(p, hero, true).Waves = 9;
                storage.SaveProfile(p);
                PlayerProfile q = storage.LoadProfile();
                Check(string.IsNullOrEmpty(storage.LastError) && q.Version == PlayerProfile.CurrentVersion && q.Meta.LampOil == 485 && q.Meta.Keepsakes == 27 && MetaProgression.Rank(q, "lantern.wick") == 1
                    && MetaProgression.Rank(q, "clock.candle") == 1 && q.Meta.Heirloom == c.Bonds[0].Id && MetaProgression.Mastery(q, hero).Waves == 9, "Meta block saves and loads");
                var old = new PlayerProfile { BestWave = 12 };
                string payload = Without(JsonUtility.ToJson(old), "Meta").Replace("\"Version\":" + PlayerProfile.CurrentVersion, "\"Version\":3");
                Check(!payload.Contains("\"Meta\"") && payload.Contains("\"Version\":3"), "Fixture: a version-3 profile without the meta block");
                WriteEnvelope(Path.Combine(dir, "profile.json"), payload);
                File.Delete(Path.Combine(dir, "profile.json.bak"));
                PlayerProfile migrated = storage.LoadProfile();
                Check(string.IsNullOrEmpty(storage.LastError) && migrated.BestWave == 12 && migrated.Meta.Revision == MetaProgress.CurrentRevision && migrated.Meta.LampOil == 12 * MetaProgression.OilPerBestWave
                    && migrated.Meta.Ranks.Count == 0 && migrated.Meta.Keepsakes == 0, "Old profile starts at zero with Lamp Oil for its best wave");
                storage.SaveProfile(migrated);
                Check(storage.LoadProfile().Meta.LampOil == 12 * MetaProgression.OilPerBestWave, "The migration grant is paid once");
            }
            finally { try { Directory.Delete(dir, true); } catch { } }

            // the cap on permanent combat power
            Check(Mathf.Abs(MetaProgression.MaxCombatPower - MetaProgression.PowerCap) < .01f, "All Lantern + Nursery ranks add up to exactly the +25% cap (" + MetaProgression.MaxCombatPower.ToString("0.00") + ")");
            PlayerProfile full = MetaProgression.BotProfile(1, c, hero);
            Check(MetaProgression.CombatPower(full) <= MetaProgression.PowerCap + .001f, "A full profile stays within the cap");
            MetaRunState fullLoad = MetaProgression.Loadout(full, c, hero, null, false, false, false);
            Check(Mathf.Approximately(fullLoad.Health, .15f) && Mathf.Approximately(fullLoad.Crit, .05f) && Mathf.Approximately(fullLoad.MoveSpeed, .08f), "At the cap the stats are unscaled (+15% HP, +8% speed, +5% crit)");
            Check(MetaProgression.Blocked(full, MetaProgression.Track("lantern.wick")) == "MAX", "A full track says MAX");
            var poor = new PlayerProfile(); MetaProgression.Ensure(poor);
            Check(MetaProgression.Blocked(poor, MetaProgression.Track("lantern.wick")) == "Not enough oil" && !MetaProgression.Buy(poor, "lantern.wick"), "No oil, no rank");
            poor.Meta.Keepsakes = 99;
            Check(MetaProgression.Blocked(poor, MetaProgression.Track("ledger.shelf")) != null, "The extra shelf waits for a full Ledger");

            // the Lantern, the Ledger, the Clock: one run with and one without
            using (RunSession plain = Start(c, hero), meta = Start(c, hero))
            {
                meta.ApplyMeta(MetaProgression.BotLoadout(1, c, hero));
                Check(meta.State.Gold == plain.State.Gold + 24, "Ledger purse: +24 starting gold");
                Check(meta.State.Health > plain.State.Health * 1.1f, "Lantern wick: more max HP (" + plain.State.Health.ToString("0") + " -> " + meta.State.Health.ToString("0") + ")");
                Check(Mathf.Approximately(meta.Bonus(ItemEffect.Interest), plain.Bonus(ItemEffect.Interest) + 2), "Ledger deep pockets: interest cap +2");
                CheckpointCatalog cp = plain.PrepareCombat(), cm = meta.PrepareCombat();
                Check(cm.Champion.Speed > cp.Champion.Speed * 1.03f, "Lantern stride: faster");
                Check(cm.Champion.CriticalChance > cp.Champion.CriticalChance + .02f, "Lantern spark: more crit for the hero");
                Check(cm.StartingCompanions.Length > 0 && cp.StartingCompanions.Length > 0, "Fixture: friends in both brigades");
                Check(cm.PickupRadius > cp.PickupRadius + 1.3f, "Lantern reach: longer pickup range");
                Check(Mathf.Abs(cm.ChestGold - cp.ChestGold * 1.24f) < .5f, "Ledger chests: +24% chest gold");
                Check(Mathf.Approximately(cm.MetaCandleShare, .4f) && Mathf.Approximately(cm.MetaCandleShield, 2) && Mathf.Approximately(cm.MetaGaugeStart, .25f) && cp.MetaCandleShare == 0, "Clock and Wardrobe reach the battle");

                // Second Candle: back up once at 40%, two seconds untouchable, then the candle is spent for the run
                var sim = new CombatSimulation(cm); sim.Start(7);
                float max = sim.Champion.Definition.Health;
                sim.DamageFixture(sim.Champion, max * 10);
                Check(sim.Phase == RunPhase.Combat && sim.CandleBurned && Mathf.Abs(sim.Champion.Health - max * .4f) < 1 && sim.CandleSafe, "Second Candle: back up at 40% HP");
                float standing = sim.Champion.Health; sim.DamageFixture(sim.Champion, 5);
                Check(Mathf.Approximately(sim.Champion.Health, standing), "Second Candle: untouchable for a moment");
                for (int i = 0; i < 50 && sim.Phase == RunPhase.Combat; i++) sim.Step(.05f, Vector2.zero);
                if (sim.Phase == RunPhase.Combat) sim.DamageFixture(sim.Champion, max * 10);
                Check(sim.Phase == RunPhase.Defeat, "Second Candle burns once a battle");
                meta.Settle(sim);
                Check(meta.State.Meta.CandleUsed && meta.State.Phase == SessionPhase.Defeat, "A burnt candle is recorded in the run");
                meta.State.Phase = SessionPhase.Combat;
                Check(meta.PrepareCombat().MetaCandleShare == 0, "Second Candle: once per run");

                // shop: the free first refresh and the extra shelf
                Shop(plain); Shop(meta);
                Check(meta.State.Offers.Count == plain.State.Offers.Count + 1, "Ledger shelf: one more offer (" + plain.State.Offers.Count + " -> " + meta.State.Offers.Count + ")");
                meta.State.Gold = plain.State.Gold = 100000;
                Check(meta.RerollCost == 0 && plain.RerollCost > 0, "Ledger free look: the first refresh is free");
                Check(meta.Reroll() && meta.RerollCost > 0 && meta.State.Gold == 100000, "Ledger free look: only the first");
            }

            // Rewind: a lost wave replays from its shop, once per run
            using (RunSession r = Start(c, hero))
            {
                r.ApplyMeta(MetaProgression.BotLoadout(1, c, hero));
                Shop(r); r.ContinueWave();
                Check(r.State.Wave == 2 && !string.IsNullOrEmpty(r.State.Meta.RewindShop) && r.State.Meta.RewindAt == 2, "Rewind keeps the shop the traveller left");
                r.State.Phase = SessionPhase.Defeat;
                Check(r.RewindReady && r.RewindWave == 2, "Rewind offered after a loss");
                Check(r.Rewind() && r.State.Phase == SessionPhase.Shop && r.State.Wave == 1 && r.State.Meta.RewindUsed && string.IsNullOrEmpty(r.State.Meta.RewindShop), "Rewind: back to the shop before the lost wave");
                r.ContinueWave(); r.State.Phase = SessionPhase.Defeat;
                Check(!r.RewindReady && !r.Rewind(), "Rewind: once per run");
            }
            using (RunSession none = Start(c, hero))
            {
                Shop(none); none.ContinueWave(); none.State.Phase = SessionPhase.Defeat;
                Check(!none.RewindReady && string.IsNullOrEmpty(none.State.Meta.RewindShop), "No Rewind without the Clock");
            }

            // the Nursery: one of three friends, the heirloom family, the stork
            PlayerProfile nursery = MetaProgression.BotProfile(1, c, hero);
            List<UnitDefinition> choices = MetaProgression.NurseryChoices(nursery, c, knight);
            Check(choices.Count == 3 && choices.Select(u => u.Id).Distinct().Count() == 3, "Nursery: three different friends to choose from");
            using (RunSession plain = Start(c, hero), cradle = Start(c, hero))
            {
                cradle.ApplyMeta(MetaProgression.Loadout(nursery, c, hero, choices[1].Id, false, false, false));
                Check(cradle.State.Roster.Count == plain.State.Roster.Count + 1 && cradle.State.Roster.Exists(e => e.UnitId == choices[1].Id && e.Tier == 1), "Nursery cradle: the chosen friend starts with you");
                Check(MetaProgression.Loadout(nursery, c, hero, "NotAFriend", false, false, false).StartFriend == "", "Nursery: a choice not offered is ignored");
            }
            BondDefinition family = null; string familyHero = null;
            foreach (UnitDefinition champion in c.Champions)
            {
                using (RunSession probe = Start(c, champion.Id))
                    foreach (BondDefinition bond in c.Bonds)
                        if (bond.Threshold >= 2 && probe.BondCounts.TryGetValue(bond.Id, out int n) && n == bond.Threshold - 1) { family = bond; familyHero = champion.Id; break; }
                if (family != null) break;
            }
            Check(family != null, "Fixture: a brigade one friend short of a bond");
            using (RunSession heir = Start(c, familyHero))
            {
                heir.ApplyMeta(new MetaRunState { Heirloom = family.Id });
                Check(heir.BondCounts[family.Id] == family.Threshold && family.ActiveLevel(heir.BondCounts[family.Id]) == 1, "Nursery heirloom: the family's first bond needs one friend fewer");
            }
            using (RunSession stork = Start(c, hero))
            {
                stork.ApplyMeta(new MetaRunState { TwoStarChance = 1 });
                stork.State.Phase = SessionPhase.Shop; stork.State.Gold = 100000;
                string starter = stork.State.Roster[0].UnitId;
                var offer = new ShopOffer { Token = ++stork.State.NextOffer, Kind = OfferKind.Companion, Id = starter, Price = 1, Currency = ShopCurrency.Gold, Tier = 1 };
                stork.State.Offers.Add(offer);
                Check(stork.Buy(offer.Token) && stork.State.Roster.Exists(e => e.UnitId == starter && e.Tier == 2), "Nursery stork: a one-star friend arrives at two stars");
            }

            // the Wardrobe: mastery from playing; another starting friend, a warm gauge, a second habit
            var dressed = new PlayerProfile(); MetaProgression.Ensure(dressed);
            MetaProgression.Mastery(dressed, hero, true).Waves = 40;
            UnitDefinition coat = MetaProgression.AltStarter(c, knight);
            HeroKit kit = HeroKits.For(hero);
            MetaRunState worn = MetaProgression.Loadout(dressed, c, hero, null, true, true, false);
            Check(coat != null && worn.AltStarter == coat.Id && Mathf.Approximately(worn.GaugeStart, .25f) && worn.SecondHabit == MetaProgression.HabitDropped(kit).HasValue, "Wardrobe: all three perks at 40 waves");
            Check(MetaProgression.Loadout(new PlayerProfile(), c, hero, null, true, true, false).AltStarter == "", "Wardrobe: nothing before the waves are played");
            using (RunSession w = Start(c, hero))
            {
                w.ApplyMeta(worn);
                Check(w.State.Roster[0].UnitId == coat.Id, "Wardrobe spare coat: the other starting friend");
                if (worn.SecondHabit)
                {
                    KitTrait added = MetaProgression.HabitAdded(kit);
                    Check(w.Kit != kit && w.Kit.Traits.Length == kit.Traits.Length && w.Kit.Traits.Contains(added) && !w.Kit.Traits.Contains(MetaProgression.HabitDropped(kit).Value), "Wardrobe second habit: one team passive swapped");
                }
                CheckpointCatalog wc = w.PrepareCombat();
                var wsim = new CombatSimulation(wc); wsim.Start(3);
                Check(!wsim.HeroIdentityEnabled || wsim.HeroIdentityCharge > 0, "Wardrobe warm gauge: the gauge starts part full");
            }

            // earning: Lamp Oil every run, Keepsakes for first times, paid once, a rewound run pays only what it adds
            var earner = new PlayerProfile(); MetaProgression.Ensure(earner);
            using (RunSession e = Start(c, hero, 2))
            {
                e.State.Wave = 8; e.State.BossKills = 1; e.State.DefeatedBosses.Add(c.Bosses[0].Id); e.State.Phase = SessionPhase.Defeat;
                int expected = Mathf.RoundToInt((7 * MetaProgression.OilPerWave + MetaProgression.OilPerBoss) * MetaProgression.DifficultyOil[2]);
                Check(MetaProgression.Credit(earner, e.State, c) && earner.Meta.LampOil == expected && earner.Meta.Keepsakes == 1, "Lamp Oil per wave and boss x difficulty, a Keepsake for the first boss (" + earner.Meta.LampOil + " oil)");
                Check(e.State.Meta.HaulOil == expected && e.State.Meta.HaulLines.Count >= 3 && RunInvoice.Describe(e.State, c).Contains("FOR THE WAITING ROOM"), "The haul is on the run summary in player words");
                Check(MetaProgression.Mastery(earner, hero).Waves == 7, "Wardrobe mastery grows with waves played");
                RunMeta.Credit(earner, e.State);
                Check(!MetaProgression.Credit(earner, e.State, c) && earner.Meta.LampOil == expected, "A run is paid once");
                MetaProgression.Reopen(earner, e.State);
                Check(!earner.CompletedRuns.Contains(e.State.RunId) && !earner.History.Exists(h => h.Id == e.State.RunId), "Rewind reopens the lost run");
                e.State.Wave = 10;
                int later = Mathf.RoundToInt((9 * MetaProgression.OilPerWave + MetaProgression.OilPerBoss) * MetaProgression.DifficultyOil[2]);
                Check(MetaProgression.Credit(earner, e.State, c) && earner.Meta.LampOil == later && earner.Meta.Keepsakes == 1 && MetaProgression.Mastery(earner, hero).Waves == 9, "A rewound run pays only what it adds");
            }

            // old run saves load without the meta block and play as before
            using (RunSession plain = Start(c, hero))
            {
                Shop(plain);
                string json = Without(JsonUtility.ToJson(plain.State), "Meta");
                Check(!json.Contains("\"Meta\""), "Fixture: a run save from before the Waiting Room");
                using (var restored = new RunSession(c))
                {
                    restored.Restore(JsonUtility.FromJson<RunSnapshot>(json));
                    Check(restored.State.Meta != null && !restored.State.Meta.Any && restored.State.Offers.Count == plain.State.Offers.Count, "Old run saves load with an empty loadout");
                }
            }
            Stations(c, hero);
            Debug.Log("META_PROGRESS_CHECKS_PASSED " + notes.Count);
        }

        /// <summary>The last five stations (21/10/2026): Postbox letters and keepsake relics, the Museum's shelves and gifts within
        /// the cap, the Night Pact's curses in a run and their pay, the Daily Dream's seed and record, the Bell's Favour; and the
        /// code-drawn secret-event pieces with the cut vignettes.</summary>
        private static void Stations(RunCatalog c, string hero)
        {
            foreach (MetaStation station in Enum.GetValues(typeof(MetaStation)))
                Check(MetaProgression.Works(station) && MetaProgression.StationLine(station).Length > 0, "Station works with a line: " + station);

            // the Postbox
            var p = new PlayerProfile { BestWave = 3 }; MetaProgression.Ensure(p);
            Check(MetaProgression.Letters.Length == 6 && MetaProgression.Letters.All(l => c.Item(l.Relic) != null && l.Text.Length < 160 && !l.Text.Contains("Hob")), "Six short letters, each with a catalog relic");
            Check(!MetaProgression.Letters.Any(l => MetaProgression.LetterEarned(p, l)) && !MetaProgression.ReadLetter(p, "letter.boss"), "Postbox: no letter before its milestone");
            p.SeenBosses.Add(c.Bosses[0].Id);
            Check(MetaProgression.ReadLetter(p, "letter.boss") && !MetaProgression.ReadLetter(p, "letter.boss") && MetaProgression.Keepsakes(p, c).SequenceEqual(new[] { "CurioBreathJar" }), "Postbox: the first boss opens a letter; reading it keeps its relic");
            MetaRunState kept = MetaProgression.Loadout(p, c, hero, null, false, false, false, "CurioBreathJar");
            Check(kept.Keepsake == "CurioBreathJar" && MetaProgression.Loadout(p, c, hero, null, false, false, false, "CurioInkFeather").Keepsake == "", "Postbox: only a kept relic can be brought");
            using (RunSession s = Start(c, hero)) { s.ApplyMeta(kept); Check(s.StackCount("CurioBreathJar") == 1, "Postbox: the keepsake relic starts the run"); }

            // the Museum
            var m = new PlayerProfile(); MetaProgression.Ensure(m);
            Check(MetaProgression.Discoveries(m, c) == 0 && MetaProgression.MuseumGifts(m, c) == (0, 0), "Museum: nothing found, no gift");
            foreach (UnitDefinition u in c.Companions.Where(u => u.Body != null && !RunSession.StorySpecies(u.Id)).Take(20)) m.SeenUnits.Add(u.Id);
            Check(MetaProgression.Discoveries(m, c) == 20 && MetaProgression.MuseumGifts(m, c) == (1, 1), "Museum: every ten finds a gift, damage then gold");
            Check(MetaProgression.Shelf(m, c, MuseumShelf.Friends).Any(x => !x.Seen) && MetaProgression.Shelf(m, c, MuseumShelf.Combos).Count == Duets.All.Length, "Museum: unseen pieces stay silhouettes; every combo shelved");
            Check(MetaProgression.Trophies(m, c).Count >= 5 && !MetaProgression.Trophies(m, c).Any(t => t.won), "Museum: named trophies for full sets, none yet");
            PlayerProfile capped = MetaProgression.BotProfile(1, c, hero); capped.SeenUnits.AddRange(m.SeenUnits);
            Check(MetaProgression.MuseumGifts(capped, c).damage == 0 && MetaProgression.CombatPower(capped) <= MetaProgression.PowerCap + .001f, "Museum: damage stays within the +25% cap");
            MetaRunState museum = MetaProgression.Loadout(m, c, hero, null, false, false, false);
            using (RunSession plain = Start(c, hero), gifted = Start(c, hero))
            {
                gifted.ApplyMeta(museum);
                float gain = gifted.Bonus(ItemEffect.Damage, ModifierScope.Champion) - plain.Bonus(ItemEffect.Damage, ModifierScope.Champion);
                Check(gain > .005f && gain < .02f && Mathf.Approximately(museum.Gold, .01f), "Museum: +1% damage and +1% gold reach the run (" + gain.ToString("0.000") + ")");
            }

            // the Night Pact
            Check(Mathf.Approximately(MetaProgression.PactMultiplier(0), 1) && MetaProgression.PactMultiplier(MetaProgression.PactMask) > 2, "Night Pact: rewards multiply");
            var pacted = new PlayerProfile(); MetaProgression.Ensure(pacted);
            for (int i = 0; i < MetaProgression.Pacts.Length; i++) MetaProgression.TogglePact(pacted, i);
            MetaRunState curse = MetaProgression.Loadout(pacted, c, hero, null, false, false, false);
            Check(curse.Pacts == MetaProgression.PactMask, "Night Pact: toggles carry into the run");
            using (RunSession plain = Start(c, hero), cursed = Start(c, hero))
            {
                cursed.ApplyMeta(curse);
                CheckpointCatalog cp = plain.PrepareCombat(), cc = cursed.PrepareCombat();
                Check(cc.MetaPactElites && cc.MetaPactEnrage && !cp.MetaPactElites && !cp.MetaPactEnrage, "Night Pact: elites every wave and boss enrage reach the battle");
                if (cp.EnemyPool != null && cp.EnemyPool.Length > 0 && cc.EnemyPool != null && cc.EnemyPool.Length > 0)
                    Check(Mathf.Abs(cc.EnemyPool[0].Speed / cp.EnemyPool[0].Speed - 1.15f) < .01f, "Night Pact: enemies +15% speed");
                plain.State.Wave = cursed.State.Wave = 3; Shop(plain); Shop(cursed);
                Check(cursed.State.Offers.Count == plain.State.Offers.Count - 1, "Night Pact: one fewer shop offer (" + plain.State.Offers.Count + " -> " + cursed.State.Offers.Count + ")");
                Check(cursed.State.SpecialOffers.Count == 0, "Night Pact: no dream shop");
                cursed.State.Wave = 6; cursed.State.Phase = SessionPhase.Defeat;
                var earner = new PlayerProfile(); MetaProgression.Ensure(earner); var bare = new PlayerProfile(); MetaProgression.Ensure(bare);
                int oilBare; using (RunSession b = Start(c, hero)) { b.State.Wave = 6; b.State.Phase = SessionPhase.Defeat; oilBare = MetaProgression.Haul(bare, b.State, c).oil; }
                Check(MetaProgression.Credit(earner, cursed.State, c) && earner.Meta.LampOil == Mathf.RoundToInt(oilBare * MetaProgression.PactMultiplier(MetaProgression.PactMask)), "Night Pact: Lamp Oil multiplied");
                Check(MetaProgression.PactRecord(earner, hero) != null && MetaProgression.PactRecord(earner, hero).Heat == MetaProgression.PactHeat(MetaProgression.PactMask), "Night Pact: the hero's pact record");
            }

            // the Window (Daily Dream)
            Check(MetaProgression.DailySeed("2026-10-21") == MetaProgression.DailySeed("2026-10-21") && MetaProgression.DailySeed("2026-10-21") != MetaProgression.DailySeed("2026-10-22"), "Daily Dream: one seed a day");
            var d = new PlayerProfile { BestWave = 40 }; MetaProgression.Ensure(d);
            UnitDefinition dailyHero = MetaProgression.DailyHero(d, c, "2026-10-21");
            MetaRunState daily = MetaProgression.Loadout(d, c, dailyHero.Id, null, false, false, false, null, "2026-10-21");
            Check(daily.Daily == "2026-10-21" && daily.Pacts == 1 << MetaProgression.DailyPact("2026-10-21"), "Daily Dream: fixed hero and one pact");
            using (RunSession s = new RunSession(c))
            {
                s.Start(dailyHero.Id, MetaProgression.DailySeed("2026-10-21")); s.ApplyMeta(daily);
                s.State.Wave = 4; s.State.Phase = SessionPhase.Defeat;
                Check(MetaProgression.Credit(d, s.State, c) && d.Meta.Keepsakes >= MetaProgression.DailyKeepsakes && MetaProgression.DailyRecord(d, "2026-10-21")?.Wave == 4 && MetaProgression.DailyPaid(d, "2026-10-21"), "Daily Dream: bonus Keepsakes once a day and the day's best");
            }

            // the Bell
            var bell = new PlayerProfile(); MetaProgression.Ensure(bell);
            Check(MetaProgression.BellBlocked(bell) == "See the dawn first" && !MetaProgression.RingBell(bell), "Bell: only after the first full clear");
            bell.Wins = 1;
            Check(MetaProgression.BellBlocked(bell) == "Fill the Lantern and Ledger", "Bell: the Lantern and Ledger must be full");
            bell.Meta.LampOil = 100000; bell.Meta.Keepsakes = 100;
            foreach (MetaTrack t in MetaProgression.BellTracks()) while (MetaProgression.Rank(bell, t.Id) < t.Max && MetaProgression.Buy(bell, t.Id)) { }
            Check(MetaProgression.RingBell(bell) && bell.Meta.Favour == 1 && MetaProgression.BellTracks().All(t => MetaProgression.Rank(bell, t.Id) == 0) && Mathf.Approximately(MetaProgression.FavourMultiplier(bell), 1.1f), "Bell: ranks reset for a Favour star (+10% Lamp Oil)");
            using (RunSession b = Start(c, hero))
            {
                b.State.Wave = 6; b.State.Phase = SessionPhase.Defeat;
                var plainHaul = new PlayerProfile(); MetaProgression.Ensure(plainHaul);
                Check(MetaProgression.Haul(bell, b.State, c).oil == Mathf.RoundToInt(MetaProgression.Haul(plainHaul, b.State, c).oil * 1.1f), "Bell: Favour raises every Lamp Oil gain");
            }
            MetaProgress roundTrip = JsonUtility.FromJson<MetaProgress>(JsonUtility.ToJson(bell.Meta));
            Check(roundTrip.Favour == 1 && roundTrip.Letters != null && roundTrip.PactBest != null, "New station fields save and load");

            // secret events and Keeper gifts drawn in code; the header vignettes cut into Resources
            Texture2D stand = ArcadeSkin.PedestalTexture();
            Check(stand != null && stand.width == 256 && stand.height == 200, "Relic pedestal drawn in code");
            Check(SecretEventVignettes.Names.All(n => Resources.Load<Sprite>("UI/SecretEvent/Vignettes/" + n) != null) && SecretEventVignettes.For(SecretEventKind.ImpossibleTailor) != null, "Secret-room vignettes cut and loading");
            string room = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/SecretEventRoom.cs"), gifts = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/OutsideStageChoice.cs");
            Check(room.Contains("CodeTone.Oxblood") && room.Contains("CodeTone.Green") && room.Contains("RunningLight") && room.Contains("Pedestal") && room.Contains("EventPlate(") && gifts.Contains("RarityDescriptionFrame.Draw") && gifts.Contains("\"TAKE\""), "DO / LOSE / GET plates painted (UI_SecretEvent, code fallback); Keeper gifts as shop cards");
        }

        /// <summary>Play-test review phase E (the Waiting Room, one shop per night): the suites its changes touch, in one launch.</summary>
        public static void RunPhaseE()
        {
            var suites = new (string name, Action run)[]
            {
                ("MetaProgress", Run),
                ("BuildSizePolicy", BuildSizeChecks.Policy),
                ("K11Merchant", K11Checks.Run),
                ("K6", K6Checks.Run),
                ("ShopFlow", ShopFlowChecks.Run),
                ("ShopDescription", ShopDescriptionChecks.Run),
                ("CopyAudit", CopyAuditChecks.Run),
                ("Recruitment", RecruitmentChecks.Run),
                ("RosterCapAndBond", RosterCapAndBondChecks.Run),
                ("HeroIdentity", HeroIdentityChecks.Run),
                ("HeroAwakening", HeroAwakeningChecks.Run),
                ("K10", K10Checks.Run),
                ("Pacing", PacingChecks.Run),
                ("October", OctoberChecks.Run),
            };
            var lines = new List<string>(); int failed = 0;
            foreach (var (name, run) in suites)
            {
                try { run(); lines.Add("PASS " + name); }
                catch (Exception error)
                {
                    failed++; Exception root = error.InnerException ?? error;
                    string[] stack = (root.StackTrace ?? "").Split('\n');
                    lines.Add("FAIL " + name + ": " + root.Message + (root is InvalidOperationException ? "" : " @ " + string.Join(" | ", stack, 0, Math.Min(3, stack.Length)).Trim()));
                }
                Debug.Log("SUITE_RESULT " + lines[lines.Count - 1]);
            }
            Directory.CreateDirectory("PreviewExports/PhaseE");
            File.WriteAllLines("PreviewExports/PhaseE/Checks.txt", lines);
            Debug.Log("PHASEE_CHECKS " + (lines.Count - failed) + "/" + lines.Count + " PASS\n" + string.Join("\n", lines));
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }
    }
}
