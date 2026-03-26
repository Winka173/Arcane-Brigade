using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K24OutsideStageChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool result, string label) { if (!result) throw new InvalidOperationException("K24 " + label); notes.Add("PASS " + label); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            OutsideStageAssets assets = OutsideStageAssets.Current;
            Check(assets != null && assets.BossFrames.Length == 16 && assets.RewardFrames.Length == 16, "Full original boss and reward atlases");
            Check(catalog.Bosses.Length == 10 && !catalog.Bosses.Contains(assets.Boss), "Optional boss never enters ordinary rotation");
            Check(BossAnatomy.For(assets.Boss).Organs.Length == 3, "The transfer hand belongs to the boss anatomy");
            Check((int)ItemRarity.Mythic == 3 && ItemRarity.Crimson > ItemRarity.Mythic && ItemRarity.Rainbow > ItemRarity.Crimson, "Rarity ordinals remain compatible");
            Check(catalog.Items.Count(item => item.Rarity == ItemRarity.Rainbow) >= 4 && catalog.Companions.Count(unit => unit.Rarity == ItemRarity.Rainbow) >= 3, "Four relics and the prismatic allies (two originals and Eclipse Egg, 18/10/2026)");
            Check(catalog.Items.Where(item => item.Rarity == ItemRarity.Rainbow).All(item => item.ShopWeight == 0 && item.UnlockWave == int.MaxValue), "Prismatic gifts cannot reroll into normal shops");
            foreach (string hero in new[] { "LanternKnight", "LoneJelly" })
            {
                using var session = new RunSession(catalog); session.Start(hero, 241077);
                session.State.Wave = 13; session.State.Phase = SessionPhase.Shop; session.State.Endless = true; // Act I: the optional rematch waits for the endless night
                session.State.Offers.Add(new ShopOffer { Id = catalog.Items[0].Id, Kind = OfferKind.Item, Price = 10, Token = ++session.State.NextOffer });
                Check(!session.OutsideChallengeAvailable, "No challenge before three transfers " + hero);
                foreach (int wave in new[] { 5, 9, 13 }) { session.State.Wave = wave; session.RegisterOutsideTransfer(); session.RegisterOutsideTransfer(); }
                Check(session.State.OutsideTransfers == 3 && session.OutsideChallengeAvailable, "Transfer count is idempotent " + hero);
                string stock = JsonUtility.ToJson(session.State.Offers[0]); int wallet = session.State.Gold;
                Check(session.AcceptOutsideStage() && !session.AcceptOutsideStage(), "Optional challenge accepts exactly once " + hero);
                CheckpointCatalog arena = session.PrepareCombat();
                Check(arena.Boss.Id == assets.Boss.Id && arena.EnemyBudget == 0 && !arena.EnableSpectacles, "Isolated super boss arena " + hero);
                var simulation = new CombatSimulation(arena); simulation.Start(241077);
                simulation.Champion.NextAttack = float.PositiveInfinity;
                for (int tick = 0; tick < 240; tick++)
                {
                    simulation.Champion.Health = simulation.Champion.Definition.Health;
                    simulation.Step(0.05f, Vector2.zero);
                }
                Check(simulation.Boss != null && simulation.Boss.BossVolley >= 1 && simulation.ExpandedFields.Count <= 48, "Real hand attacks execute " + hero);
                simulation.DamageFixture(simulation.Boss, simulation.Boss.Health + 100);
                simulation.Step(0.05f, Vector2.zero); session.Settle(simulation);
                Check(session.State.OutsideStage == OutsideStageStatus.Reward && session.State.OutsideRewards.Count == 3, "Victory gives exactly three choices " + hero);
                Check(session.State.Gold == wallet && JsonUtility.ToJson(session.State.Offers[0]) == stock, "No free shop refresh or duplicated regular income " + hero);
                Check(!session.Reroll() && !session.CanBuy(session.State.Offers[0]), "Reward phase cannot buy or reroll ordinary shop " + hero);
                if (hero == "LoneJelly") Check(session.State.OutsideRewards.All(id => catalog.Item(id) != null), "Solo hero never receives unusable ally choices");
                RunSnapshot saved = MergeReveal.Capture(session.State); session.Restore(saved);
                Check(session.ClaimOutsideStageReward(0) && !session.ClaimOutsideStageReward(0), "Save-loaded reward claims once " + hero);
                Check(session.State.Wave == 13 && session.State.OutsideStage == OutsideStageStatus.Completed && !session.OutsideChallengeAvailable, "Return to same wave shop after reward " + hero);
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 241077); session.State.Wave = 13; session.State.Endless = true; session.State.OutsideTransfers = 3; session.State.Phase = SessionPhase.Shop;
                int gold = session.State.Gold;
                Check(session.DeclineOutsideStage() && !session.OutsideChallengeAvailable && session.State.Gold == gold, "Declining has no economic penalty");
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 241077); session.State.Wave = 2;
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0;
                var simulation = new CombatSimulation(arena); simulation.Start(241077);
                UnitDefinition minion = arena.MeleeEnemy;
                for (int index = 0; index < 6; index++)
                {
                    UnitState target = simulation.AddFixture(minion, Faction.Enemy, new Vector2(8, 0)); target.SwarmMinion = true;
                    simulation.DamageFixture(target, target.Health + 100);
                }
                Check(simulation.Pickups.Where(pickup => pickup.Kind == LootKind.Coin).Sum(pickup => pickup.Value) == arena.MinionCoin * 6, "Six extra kills each drop their own small coin");
            }
            foreach (int wave in new[] { 1, 3, 5, 9, 16 })
            {
                Check(WaveCadence.SwarmBatch(wave, 1) >= WaveCadence.SwarmBatch(wave, 10), "Swarm crest and breathing interval " + wave);
                using var session = new RunSession(catalog); session.Start("LanternKnight", 241077); session.State.Wave = wave;
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnableArenaToys = arena.EnableSpectacles = false;
                var simulation = new CombatSimulation(arena); simulation.Start(241077); simulation.Champion.NextAttack = float.PositiveInfinity;
                for (int tick = 0; tick < 40; tick++) simulation.Step(0.05f, Vector2.zero);
                Check(simulation.SwarmSpawnCount >= 1 && simulation.Units.All(unit => !unit.SwarmMinion || unit.Definition.Weapon.Kind != WeaponKind.Projectile), "Swarms consist of small melee bodies " + wave);
                if (wave >= 3) Check(simulation.Units.Any(unit => unit.SwarmMinion && unit.Health < unit.Definition.Health), "Extra bodies are fragile, not extra tanks " + wave);
                UnitState fighter = simulation.Champion;
                // melee reach was shortened on 19/10/2026 (MeleeBalance.Range 1.35 -> 1.1): still above the authored reach early, never far below it late
                Check(MeleeBalance.Range > 1 && fighter.Definition.Weapon.Range >= catalog.Unit("LanternKnight").Weapon.Range * .85f, "Balanced melee weapon keeps its reach " + wave);
            }
            File.WriteAllLines("PreviewExports/K24/Focused_Checks.txt", notes);
            Debug.Log("K24_CHECKS_PASSED " + notes.Count);
        }
    }
}
