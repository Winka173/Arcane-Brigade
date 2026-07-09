using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K6Checks
    {
        private static float k6PeacefulHealth, k6PeacefulHero; private static int k6PeacefulBudget;
        public static void Run()
        {
            var results = new List<string>();
            Action<bool, string> require = (condition, name) => { if (!condition) throw new InvalidOperationException("K6: " + name); results.Add("PASS " + name); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            string root = Path.Combine(Application.temporaryCachePath, "ArcaneBrigadeK6Checks", Guid.NewGuid().ToString("N"));
            var storage = new RunStorage(root);
            var profile = new PlayerProfile();
            require(RunPaths.Diagnostic && root != Application.persistentDataPath, "Tests do not touch player profile");
            require(new[] { SessionPhase.Combat, SessionPhase.Shop, SessionPhase.Victory, SessionPhase.Defeat }.All(phase => RunMenu.CanResumeSnapshot(new RunSnapshot { Phase = phase })) && !RunMenu.CanResumeSnapshot(null) && !RunMenu.CanResumeSnapshot(new RunSnapshot { Phase = (SessionPhase)999 }), "All validated run phases including terminal results can resume without silently creating new run");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 610026);
                RunSnapshot state = session.State;
                require(state.SchemaRevision == RunSnapshot.CurrentSchemaRevision && state.StartHall == 1 && state.Difficulty == 0, "New default mode uses current schema");
                string initial = JsonUtility.ToJson(state);
                string first = RunInvoice.Describe(state, catalog); string second = RunInvoice.Describe(state, catalog);
                require(first == second && initial == JsonUtility.ToJson(state), "Invoice read model cannot mutate build or RNG");
                RosterEntry actualEntry = state.Roster[0];
                // play-test 20/10/2026: the summary is player-facing (no run id, seed, tiers or measured-source rows)
                string actualRosterLine = RunUICopy.UnitName(actualEntry.UnitId, catalog.Unit(actualEntry.UnitId)?.DisplayName) + " " + new string('★', Mathf.Clamp(actualEntry.Tier, 1, 3));
                require(first.Contains(actualRosterLine) && first.Contains(RunUICopy.UnitName(catalog.Unit(state.ChampionId))) && !first.Contains(state.RunId) && !first.Contains("tier") && !first.Contains("Seed"), "Run summary names the brigade with stars and the hero, without internal ids");
                state.Phase = SessionPhase.Victory; state.Wave = 20; state.TotalKills = 7; state.BossKills = 2;
                state.Contributions.Add(new ContributionEntry { UnitId = state.ChampionId, WeaponId = "SleepCamera", EffectId = "primary", Tier = 1, Damage = 11 });
                require(RunMeta.Score(state) == 26070, "Score uses20clears+7kills+2bosses+victory exactly");
                storage.SaveRun(state); storage.RecordProgress(profile, state, catalog);
                string recorded = JsonUtility.ToJson(profile);
                storage.RecordProgress(profile, state, catalog);
                require(recorded == JsonUtility.ToJson(profile) && profile.Wins == 1 && profile.TotalKills == 7 && profile.History.Count == 1, "Repeated results credit exactly once");
                require(profile.History[0].Nickname == "Midnight photo booth" && profile.History[0].Score == 26070, "Measured dominant weapon yields stable stored nickname");
                state.Contributions[0].Damage = 200;
                require(profile.History[0].Build.Contributions[0].Damage == 11, "History is a deep frozen build not live reference");
                require(RunMeta.UnlockedDifficulty(profile) == RunMeta.DifficultyNames.Length - 2, "All difficulties but the last are available without permanent stats");
                PlayerProfile loaded = storage.LoadProfile();
                require(loaded.History[0].Nickname == profile.History[0].Nickname && loaded.Records.Count == 1 && loaded.Wins == 1, "History records and dedupe persist across reload");
                string normalId = state.RunId; string roster = JsonUtility.ToJson(new RosterHolder { Entries = state.Roster });
                session.EnterEndless();
                require(state.ParentRunId == normalId && state.RunId != normalId && state.StartHall == 21 && state.Contributions.Count == 0 && state.TotalKills == 0 && state.BossKills == 0 && state.CombatSeconds == 0, "Endless resets only measured segment and retains parent identity");
                require(roster == JsonUtility.ToJson(new RosterHolder { Entries = state.Roster }) && state.Items.Count == profile.History[0].Build.Items.Count, "Endless keeps actual squad and relics");
                require(RunMeta.Score(state) == 0, "Endless entry rest does not score an uncleared hall");
                session.ContinueWave(); state.Phase = SessionPhase.Defeat; state.TotalKills = 3;
                storage.RecordProgress(profile, state, catalog);
                require(profile.TotalKills == 10 && profile.Wins == 1 && profile.Records.Count == 2 && profile.History[0].Build.Endless, "Endless history separate from normal score and wins");
                storage.RecordProgress(profile, state, catalog);
                require(profile.TotalKills == 10 && profile.History.Count == 2, "Endless death cannot duplicate normal or segment credit");
                RunSnapshot legacy = Copy(state); legacy.SchemaRevision = 2; legacy.Contributions.Add(new ContributionEntry { UnitId = "ARENA", WeaponId = "", EffectId = "old-mixed", Tier = 0, Damage = 99 });
                session.Restore(legacy);
                require(session.State.Contributions.Count == 0 && session.State.StartHall == 21 && session.State.Roster.Count == legacy.Roster.Count, "Legacy Endless drops inseparable old ledger not invents segment damage");
                legacy.Endless = false; legacy.SchemaRevision = 1; legacy.Receipts = null;
                session.Restore(legacy);
                require(session.State.SchemaRevision == RunSnapshot.CurrentSchemaRevision && session.State.StartHall == 1 && session.State.Contributions.Count == legacy.Contributions.Count && session.State.Receipts.Count == 0, "Legacy normal schema1 preserves measured ledger and migrates receipts");
            }
            using (var session = new RunSession(catalog))
            {
                for (int level = 0; level < 5; level++)
                {
                    session.Start(catalog.Champions[0].Id, 61, level);
                    CheckpointCatalog combat = session.PrepareCombat();
                    // current rules (JourneyBalance table, phase C budget x WaveCadence.LengthScale): HP scales by the difficulty's
                    // health factor over Peaceful; the wave-1 budget follows RunSession's density formula when no relic or modifier bends it
                    float enemyHealthTrait = catalog.StatRevision > 0 ? K10StatRules.Factor(session.Bonus(ItemEffect.EnemyHealth), 0.2f, 10) : 1;
                    if (level == 0) { k6PeacefulHealth = combat.MeleeEnemy.Health / enemyHealthTrait; k6PeacefulBudget = combat.EnemyBudget; }
                    float healthRatio = JourneyBalance.EnemyHealth(level, 1) / JourneyBalance.EnemyHealth(0, 1);
                    int budget = Mathf.CeilToInt((30 + 3) * CarnivalBalance.EnemyDensity(1) * JourneyBalance.EnemyDensity(level, 1));
                    budget = Mathf.Min(Mathf.CeilToInt(690 * CarnivalBalance.EnemyCountScale), Mathf.CeilToInt(Mathf.Min(Mathf.CeilToInt(600 * CarnivalBalance.EnemyCountScale), Mathf.CeilToInt(budget * 1.4f)) * WaveCadence.LengthScale));
                    bool plainBudget = Mathf.Approximately(session.Bonus(ItemEffect.EnemyCount), 0) && session.ActiveModifier == null;
                    require(Mathf.Abs(combat.MeleeEnemy.Health / enemyHealthTrait - k6PeacefulHealth * healthRatio) <= k6PeacefulHealth * healthRatio * .002f && (!plainBudget || combat.EnemyBudget == budget) && combat.EnemyBudget >= k6PeacefulBudget, "Difficulty " + level + " explicit HP, authored trait and budget (" + combat.EnemyBudget + " vs " + budget + ")");
                    // caps of RunSession.BuildEnemy: 14 before the difficulty/wave pressure, contact x1.08 x1.6 on top; the hero is the same on every difficulty
                    float pressure = JourneyBalance.EnemyDamage(level, 1) * CarnivalBalance.EnemyDamage(1) * 1.08f * OctoberBalance.EndlessDamage(1, false) * 1.08f;
                    if (level == 0) k6PeacefulHero = combat.Champion.Health;
                    require(combat.MeleeEnemy.ContactDamage <= 14 * pressure * 1.6f + .001f && combat.MeleeEnemy.Weapon.Damage <= 14 * pressure + .001f && Mathf.Approximately(combat.Champion.Health, k6PeacefulHero), "Difficulty " + level + " damage caps and base Champion unchanged (" + combat.MeleeEnemy.ContactDamage + ", " + combat.Champion.Health + ")");
                    storage.SaveRun(session.State); session.Restore(storage.LoadRun(session));
                    require(session.State.Difficulty == level && session.State.Seed == 61, "Difficulty " + level + " resume preserves identity");
                }
                for (int index = 1; index < RunMeta.ChallengeIds.Length; index++)
                {
                    session.Start(catalog.Champions[0].Id, RunMeta.ChallengeSeed(index), 0, RunMeta.ChallengeIds[index]);
                    string before = JsonUtility.ToJson(session.State); session.Restore(Copy(session.State));
                    require(before == JsonUtility.ToJson(session.State), "Challenge " + index + " seed and streams stable on resume");
                    session.State.Phase = SessionPhase.Victory; session.State.Wave = 20;
                    var challengeProfile = new PlayerProfile(); storage.RecordProgress(challengeProfile, session.State, catalog);
                    require(challengeProfile.Achievements.Contains("challenge-" + session.State.ChallengeId) && RunMeta.UnlockedDifficulty(challengeProfile) == RunMeta.DifficultyNames.Length - 2, "Challenge " + index + " keeps its own reward and all difficulties remain available");
                }
                RunSnapshot invalid = Copy(session.State); invalid.Difficulty = 5;
                require(Reject(() => session.Validate(invalid)), "Reject invalid difficulty");
                invalid = Copy(session.State); invalid.ChallengeId = "unknown";
                require(Reject(() => session.Validate(invalid)), "Reject unknown challenge identity");
                invalid = Copy(session.State); invalid.StartHall = 100;
                require(Reject(() => session.Validate(invalid)), "Reject impossible segment start");
                session.Start(catalog.Champions[0].Id, 61); session.State.Roster.Clear();
                CheckpointCatalog combatFixture = session.PrepareCombat(); combatFixture.EnemyBudget = 0; combatFixture.Boss = null; combatFixture.WaveDuration = 1000; combatFixture.EnableArenaToys = false;
                combatFixture.Champion.Weapon = UnityEngine.Object.Instantiate(catalog.Weapon("KeyCleaver"));
                combatFixture.Champion.CriticalChance = 0;
                var simulation = new CombatSimulation(combatFixture); simulation.Start(61);
                UnitDefinition enemyDefinition = UnityEngine.Object.Instantiate(combatFixture.MeleeEnemy); enemyDefinition.Health = 1; enemyDefinition.Speed = 0; enemyDefinition.ContactDamage = 0;
                UnitState enemy = simulation.AddFixture(enemyDefinition, Faction.Enemy, Vector2.right); enemy.NextAttack = enemy.NextContact = float.PositiveInfinity;
                for (int tick = 0; tick < 40; tick++) simulation.Step(0.05f, Vector2.zero);
                require(simulation.Kills == 1 && simulation.Contributions.Sum(entry => entry.Kills) == 1 && Mathf.Approximately(simulation.Contributions.Sum(entry => entry.Damage), 1), "Real overkill records only1HP lost with1kill credited to one source");
                simulation.Champion.Health = 1;
                simulation.DamageFixture(simulation.Champion, 0.25f);
                simulation.DamageFixture(simulation.Champion, 2);
                require(simulation.Phase == RunPhase.Defeat && simulation.LastDamageSource == "Enemy damage" && simulation.LastDamageAmount == 2 && simulation.LastDamageKind.Contains("Physical"), "Ordered real lethal hits retain final source kind and applied amount");
                UnityEngine.Object.DestroyImmediate(enemyDefinition); UnityEngine.Object.DestroyImmediate(combatFixture.Champion.Weapon);
                session.Start(catalog.Champions[0].Id, 61); session.State.Wave = 20;
                CheckpointCatalog finish = session.PrepareCombat(); finish.EnemyBudget = 0; finish.Boss = null; finish.WaveDuration = 0.05f;
                simulation = new CombatSimulation(finish); simulation.Start(61); simulation.Step(0.1f, Vector2.zero);
                session.Settle(simulation); string settled = JsonUtility.ToJson(session.State); session.Settle(simulation);
                require(session.State.Phase == SessionPhase.Victory && settled == JsonUtility.ToJson(session.State), "Real wave settlement once even if called twice");
            }
            var oldProfile = new PlayerProfile { Version = 1, Wins = 7, SeenWeapons = null, SeenTiers = null, SeenChains = null, SeenBosses = null, History = null, Records = null };
            storage.SaveProfile(oldProfile); PlayerProfile migrated = storage.LoadProfile();
            require(migrated.Version == PlayerProfile.CurrentVersion && migrated.Wins == 7 && migrated.History.Count == 0 && migrated.SeenTiers.Count == 0 && migrated.SeenWeapons.Count == 0, "Profile1 migration preserves counters without fabricating discovery/history");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 61); session.State.Phase = SessionPhase.Defeat;
                string blocker = root + "-blocked"; File.WriteAllText(blocker, "fixture");
                var failedProfile = new PlayerProfile(); string before = JsonUtility.ToJson(failedProfile);
                require(Reject(() => new RunStorage(blocker).RecordProgress(failedProfile, session.State, catalog)) && before == JsonUtility.ToJson(failedProfile), "Failed profile disk write rolls back all in-memory credit and discovery");
                storage.RecordProgress(failedProfile, session.State, catalog);
                require(failedProfile.History.Count == 1 && failedProfile.CompletedRuns.Count == 1, "Retry after disk failure credits once");
                var corrupt = JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(failedProfile));
                storage.SaveProfile(failedProfile);
                corrupt.History[0].Build.Wave = -1;
                storage.SaveProfile(corrupt);
                require(storage.LoadProfile().History[0].Build.Wave == 1 && storage.LastError.Contains("history"), "Malformed history falls back to durable profile backup");
                for (int index = 0; index < 55; index++) { session.State.RunId = "history-" + index; RunMeta.Credit(failedProfile, session.State); }
                require(failedProfile.History.Count == 50 && failedProfile.CompletedRuns.Count == 56 && failedProfile.Records.Count == 1, "Bounded detailed history retains lifetime dedupe and grouped bests");
                RunMeta.Credit(failedProfile, session.State);
                require(failedProfile.History.Count == 50 && failedProfile.CompletedRuns.Count == 56, "Eviction does not recredit an old identity");
                var museumProfile = new PlayerProfile(); session.State.Phase = SessionPhase.Combat;
                session.State.Items.Add(new ItemStack { Id = "harvest-receipt", Count = 1 });
                RunMeta.Discover(museumProfile, session.State, catalog);
                require(museumProfile.SeenChains.Count == 0 && museumProfile.SeenBosses.Count == 0, "Owned relic/build alone cannot claim chain or boss clear");
                session.State.Contributions.Add(new ContributionEntry { UnitId = session.State.ChampionId, WeaponId = "BellFork", Tier = 1, EffectId = "harvest-receipt", Activations = 1 });
                RunMeta.Discover(museumProfile, session.State, catalog);
                require(museumProfile.SeenChains.SequenceEqual(new[] { "signature-harvest" }), "Chain discovery requires measured gameplay evidence");
                session.State.DefeatedBosses.Add(catalog.Bosses[0].Id); session.State.Roster[0].Tier = 2;
                RunMeta.Discover(museumProfile, session.State, catalog);
                require(museumProfile.SeenBosses.Contains(catalog.Bosses[0].Id) && museumProfile.SeenTiers.Contains(session.State.Roster[0].WeaponId + "/2"), "Actual boss and owned merged tier IDs discovered");
                string runBefore = JsonUtility.ToJson(session.State);
                string catalogBefore = JsonUtility.ToJson(catalog);
                for (int bundle = 0; bundle < 3; bundle++) { museumProfile.Cosmetic = bundle; ArcadeSkin.Bundle = bundle; RunInvoice.Describe(session.State, catalog); }
                require(runBefore == JsonUtility.ToJson(session.State) && catalogBefore == JsonUtility.ToJson(catalog), "All cosmetic bundles change no run/RNG/threat/catalog values");
                require(RunMeta.CosmeticUnlocked(new PlayerProfile(), 0) && !RunMeta.CosmeticUnlocked(new PlayerProfile(), 1) && !RunMeta.CosmeticUnlocked(new PlayerProfile(), 2), "Cosmetic unlock gates default accessible no hidden stat rewards");
                session.Start(catalog.Champions[0].Id, 62); session.State.Phase = SessionPhase.Shop; session.State.Gold = 100000;
                string recruit = session.State.Roster[0].UnitId; // the hero's own starting friend (HeroLoadout), not always the first catalog companion
                for (int purchase = 0; purchase < 2; purchase++)
                {
                    session.State.Offers.Clear(); session.State.NewRecruitsThisWave = session.State.NewSpeciesRecruitsThisWave = 0; // one copy per wave (RecruitmentRules): each on its own night
                    var offer = new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = recruit, Price = 1 };
                    session.State.Offers.Add(offer);
                    require(session.Buy(offer.Token), "Committed recruitment " + purchase);
                }
                require(session.State.Roster.Count(entry => entry.UnitId == recruit) == 1 && session.State.Roster.Find(entry => entry.UnitId == recruit).Tier == 2 && session.State.AcquiredVariants.Count(entry => entry.UnitId == recruit && entry.Tier == 1) == 3 && session.State.AcquiredVariants.Any(entry => entry.UnitId == recruit && entry.Tier == 2), "Discovery journal retains consumed rolled variants through automatic3merge (" + session.State.Roster.Count + " in roster)");
                var mergeProfile = new PlayerProfile(); RunMeta.Discover(mergeProfile, session.State, catalog);
                require(session.State.AcquiredVariants.All(entry => mergeProfile.SeenTiers.Contains(entry.WeaponId + "/" + entry.Tier)), "Museum retains all actually received tiers even when bodies consumed");
                session.State.Offers.Clear();
                session.State.Offers.Add(new ShopOffer { Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Id = recruit, Price = 1 });
                string journalBefore = JsonUtility.ToJson(session.State);
                require(!RunTransaction.TryCommit(session, catalog, "refused", () => session.Buy(session.State.NextOffer), () => false, out _) && journalBefore == JsonUtility.ToJson(session.State), "Rejected recruit rolls back variant journal with RNG and receipt");
                session.State.Contributions.Clear();
                session.State.Contributions.Add(new ContributionEntry { UnitId = "unit", WeaponId = "BellFork", Tier = 1, EffectId = "primary", Damage = 10 });
                session.State.Contributions.Add(new ContributionEntry { UnitId = "unit", WeaponId = "TeapotPopgun", Tier = 1, EffectId = "primary", Damage = 10 });
                string nickname = RunMeta.Nickname(session.State); session.State.Contributions.Reverse();
                require(nickname == RunMeta.Nickname(session.State), "Equal contribution nickname tie-break independent of iteration order");
            }
            foreach (Font font in new[] { ArcadeTypography.Body, ArcadeTypography.Heading, ArcadeTypography.Numbers })
            {
                require(font != null, "Font imported for role");
                string glyphs = "Đội quân thất lạc — Ống kính ngái ngủ | 1.234 vàng / 00:59 | Đã mua";
                font.RequestCharactersInTexture(glyphs, 24);
                require(glyphs.All(character => character == ' ' || font.HasCharacter(character)), "Vietnamese glyph sample " + font.name);
            }
            string skin = File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/ArcadeSkin.cs");
            require(skin.Contains("Divider") && skin.Contains("Stamp") && skin.Contains("Header"), "UI has shared ticket/stamp/document motifs not palette-only");
            require(File.Exists("Assets/ArcaneBrigade/Resources/Fonts/BeVietnamPro-OFL.txt"), "Font license distributed");
            require(RunMeta.ChainEvidence("reflective-ink") == "signature-office" && catalog.BuildLinks.Any(link => link.Id == RunMeta.ChainEvidence("reflective-ink")), "Office discovery maps to existing stable graph ID");
            Texture2D illustration = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ArcaneBrigade/Resources/LostShop_v1.png");
            require(illustration != null && illustration.width == 1536 && illustration.height == 1024, "Original menu illustration full native resolution");
            ArcadeSkin.Bundle = 0;
            Directory.CreateDirectory("PreviewExports/K6"); File.WriteAllLines("PreviewExports/K6/Checks.txt", results);
            Debug.Log("K6_CHECKS_PASSED " + results.Count);
        }
        private static bool Reject(Action operation) { try { operation(); return false; } catch { return true; } }
        private static RunSnapshot Copy(RunSnapshot run) => JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run));
        [Serializable] private sealed class RosterHolder { public List<RosterEntry> Entries; }
    }
}
