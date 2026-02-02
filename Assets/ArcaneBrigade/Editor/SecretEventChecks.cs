using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class SecretEventChecks
    {
        private static readonly List<string> Notes = new List<string>();
        private static void Check(bool ok, string text) { if (!ok) throw new InvalidOperationException("Secret events: " + text); Notes.Add("PASS " + text); }
        private static RunCatalog Catalog => Resources.Load<RunCatalog>("RunCatalog");
        private static RunSession Room(SecretEventKind kind, bool solo = false)
        {
            RunCatalog c = Catalog;
            for (int seed = 0; seed < 500; seed++)
            {
                var s = new RunSession(c); s.Start((solo ? c.Champions.First(u => u.HeroMechanic == HeroMechanic.SoloMaw) : c.Champions.First(u => u.HeroMechanic != HeroMechanic.SoloMaw)).Id, seed);
                s.State.Wave = 7; s.State.Phase = SessionPhase.Shop; s.State.Gold = 200;
                s.State.Items.Add(new ItemStack { Id = c.Items.First(i => !i.Id.StartsWith("Secret") && i.Rarity < ItemRarity.Crimson && i.EffectiveModifiers.Any(m => m.Value > 0)).Id, Count = 1 });
                if (Array.Exists(s.SecretPortals(), k => k == kind)) { Check(s.EnterSecretEvent(kind), "Natural seeded entry " + kind); return s; }
                s.Dispose();
            }
            throw new InvalidOperationException("Portal cannot be discovered: " + kind);
        }
        private static void Roundtrip(RunSession s, string caption)
        {
            RunSnapshot saved = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State));
            s.Validate(saved); s.Restore(saved); Check(JsonUtility.ToJson(saved) == JsonUtility.ToJson(s.State), "Save round-trip " + caption);
        }
        private static CombatSimulation Win(RunSession s)
        {
            CheckpointCatalog c = s.PrepareCombat();
            c.Champion.Health = 100000; c.PassiveHeal = 10000;
            c.Champion.Weapon.Damage = 1000000; c.Champion.Weapon.Range = 30; c.Champion.Weapon.Cooldown = .2f;
            if (c.Boss != null) { c.Boss.Speed = 0; c.Boss.ContactDamage = 0; }
            var sim = new CombatSimulation(c); sim.Start(377); sim.Champion.Health = 100000;
            for (int i = 0; i < 4000 && sim.Phase == RunPhase.Combat; i++) sim.Step(.05f, Vector2.zero);
            Check(sim.Phase == RunPhase.Victory, "Actual combat can win " + s.State.Secrets.Kind + "/" + s.State.Secrets.Option);
            return sim;
        }
        private static void NormalVictory(RunSession s)
        {
            s.State.Wave++; s.State.Phase = SessionPhase.Combat;
            CheckpointCatalog c = s.PrepareCombat(); c.Boss = null; c.EnemyBudget = 0; c.WaveDuration = .1f;
            var simulation = new CombatSimulation(c); simulation.Start(7);
            for (int i = 0; i < 4; i++) simulation.Step(.05f, Vector2.zero);
            s.Settle(simulation);
        }
        public static void Run()
        {
            Notes.Clear(); RunCatalog c = Catalog; SecretEventAssets a = SecretEventAssets.Current;
            Check(a != null && a.Portals.Length == 16 && a.Portals.All(p => p != null), "8 portals / 16 imported states");
            Check(a.Relics.Length == 16 && a.Relics.All(p => p != null), "16 original relic illustrations import");
            Check(c.Items.Count(i => i.Id.StartsWith("Secret")) == 8, "8 unique reward relics");
            Check(c.Items.Where(i => i.Id.StartsWith("Secret")).All(i => i.Rarity != ItemRarity.Rainbow && i.ShopWeight == 0), "Secret rewards do not enter normal shop or replace Keeper prismatic tier");
            for (int k = 0; k < 8; k++)
            for (int option = 0; option < 3; option++)
            using (RunSession s = Room((SecretEventKind)k))
            {
                if (k == 5 && option == 1) s.State.Secrets.Contracts.Add(new SecretContract { Kind = SecretContractKind.IncomeDebt, Remaining = 2 });
                Roundtrip(s, "choosing " + k + "/" + option);
                int gold = s.State.Gold, wave = s.State.Wave; uint random = s.State.ShopRandom;
                var offers = JsonUtility.ToJson(new Offers { Rows = s.State.Offers });
                Check(s.SecretChoices().Length == 3, "Three authored choices " + k);
                // Target capability can exclude one stitch from a particular build.
                if (!s.SecretChoices()[option].Enabled && k == 1)
                {
                    RosterEntry r = s.State.Roster[0]; r.WeaponId = c.Weapons.First(w => w.Kind == (option == 0 ? WeaponKind.Projectile : WeaponKind.Melee)).Id;
                }
                Check(s.SecretChoices()[option].Enabled, "Choice available with valid prerequisites " + k + "/" + option);
                Check(s.ChooseSecretEvent(option), "Commit choice " + k + "/" + option);
                Check(s.State.Nightmare.Choices[k] == option && (s.State.Nightmare.ChoiceMask & (1 << k)) != 0, "Accepted choice enters narrative memory " + k + "/" + option);
                Check(!s.ChooseSecretEvent(option), "Choice cannot double commit " + k + "/" + option);
                Roundtrip(s, "committed " + k + "/" + option);
                if (s.SecretFighting)
                {
                    Check((s.State.Nightmare.ResolvedMask & (1 << k)) == 0, "Unwon optional battle does not fabricate a story outcome " + k + "/" + option);
                    Check(!s.LeaveSecretEvent(), "Cannot escape committed battle");
                    CombatSimulation sim = Win(s); s.Settle(sim);
                    Check(s.State.Phase == SessionPhase.Shop && s.State.Secrets.Stage == SecretEventStage.Reward, "Battle returns to reward " + k + "/" + option);
                    Check(s.State.Gold == gold, "No optional battle income / kill-loot farm " + k + "/" + option);
                    Roundtrip(s, "unclaimed reward");
                    Check(!s.LeaveSecretEvent(), "Cannot discard the claim state by accident");
                    Check(s.ClaimSecretBattleReward() && !s.ClaimSecretBattleReward(), "Battle reward claims once " + k + "/" + option);
                }
                else Check(!s.ClaimSecretBattleReward(), "Immediate deal cannot be claimed again " + k + "/" + option);
                Check((s.State.Nightmare.ResolvedMask & (1 << k)) != 0 && NightmareDialogue.Memory(s.State,k).Length > 20, "Successful outcome becomes contextual dialogue " + k + "/" + option);
                Check(s.State.Wave == wave && s.State.ShopRandom == random && JsonUtility.ToJson(new Offers { Rows = s.State.Offers }) == offers, "Wave, shop RNG and offers preserved " + k + "/" + option);
                Roundtrip(s, "resolved " + k + "/" + option);
                Check(s.LeaveSecretEvent(), "Return exit works " + k + "/" + option);
                Check(s.SecretPortals().Length == 0, "No second room in same shop");
            }
            using (RunSession s = Room(SecretEventKind.ThirteenthHour))
            {
                Check(s.ChooseSecretEvent(1), "Bank loan committed"); s.LeaveSecretEvent();
                int remaining = s.State.Secrets.Contracts[0].Remaining;
                s.State.Wave++;
                CheckpointCatalog arena = s.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; arena.WaveDuration = .1f;
                var sim = new CombatSimulation(arena); sim.Start(1); for (int i=0;i<4;i++) sim.Step(.05f,Vector2.zero);
                s.State.Phase = SessionPhase.Combat; s.Settle(sim);
                Check(s.State.Secrets.Contracts[0].Remaining == remaining-1, "Only normal victory advances loan");
                int gold = s.State.Gold; s.Settle(sim); Check(s.State.Gold == gold, "Normal settlement cannot charge twice");
                Roundtrip(s, "income debt");
            }
            using (RunSession s = Room(SecretEventKind.ImpossibleTailor))
            {
                string id = s.State.Secrets.TargetItem; Check(s.ChooseSecretEvent(2), "Ward stitch commits");
                Check(s.SecretItemSuspended(id) && s.PrepareCombat().SecretWard, "Stitch replaces base modifiers and compiles damage ward");
                Check(s.SecretTargets.All(t => t != id), "Relic cannot be stitched twice");
                CheckpointCatalog ward = s.PrepareCombat(); ward.Armor=ward.Dodge=0; ward.EnemyBudget=0;
                var sim = new CombatSimulation(ward); sim.Start(9); float hp=sim.Champion.Health;
                sim.WeaponDamageFixture(sim.Champion,100000,DamageElement.Physical);
                Check(Mathf.Abs(hp-sim.Champion.Health-hp*.22f*.85f)<.01f,"Shelter reduces capped incoming hits by a real 15 percent");
            }
            using (RunSession s = Room(SecretEventKind.ThirteenthHour))
            {
                string id = s.State.Secrets.TargetItem; float original = s.Bonus(ItemEffect.Damage);
                Check(s.ChooseSecretEvent(2), "Deposit original relic");
                Check(s.SecretItemSuspended(id), "Deposited relic effects sleep");
                s.LeaveSecretEvent(); NormalVictory(s); Roundtrip(s,"parcel half term");
                Check(s.SecretItemSuspended(id) && s.State.Secrets.Contracts[0].Remaining == 1, "Parcel remains unavailable until full term");
                NormalVictory(s);
                Check(!s.SecretItemSuspended(id) && s.State.Secrets.ImprovedItems.Contains(id), "Parcel returns improved after two normal victories");
                Check(s.State.Items.Count(i=>i.Id==id)==1, "Returned parcel never duplicates item stack");
            }
            using (RunSession s = Room(SecretEventKind.ThirteenthHour))
            {
                Check(s.ChooseSecretEvent(0), "Borrow relic for three normal victories"); s.LeaveSecretEvent();
                NormalVictory(s); NormalVictory(s); Check(s.StackCount("SecretHour") == 1, "Borrowed relic remains for two completed waves");
                NormalVictory(s); Check(s.StackCount("SecretHour") == 0 && s.State.Secrets.Contracts.Count == 0, "Borrowed relic expires exactly once at third victory");
                Roundtrip(s, "expired loan");
            }
            using (RunSession s = Room(SecretEventKind.Understudy))
            {
                int count=s.State.Roster.Count;
                Check(s.ChooseSecretEvent(1),"Lend relic for temporary shadow");
                Check(s.PrepareCombat().StartingCompanions.Length==count+1,"Shadow exists in actual compiled roster");
                s.LeaveSecretEvent(); NormalVictory(s); NormalVictory(s);
                Check(s.PrepareCombat().StartingCompanions.Length==count && s.State.Secrets.Contracts.Count==0,"Shadow leaves and relic returns at term");
            }
            using (RunSession s = Room(SecretEventKind.LostProperty))
            {
                s.State.OutsideTransfers=3;s.State.Endless=true;
                Check(!s.OutsideChallengeAvailable && !s.AcceptOutsideStage(),"Keeper invitation cannot overlap active secret room");
                s.LeaveSecretEvent(); Check(s.OutsideChallengeAvailable && s.AcceptOutsideStage(),"Existing Keeper challenge still accepts after secret exit");
                CheckpointCatalog arena=s.PrepareCombat();Check(arena.Boss.Id=="OutsideStageKeeper" && arena.SecretEncounter==0,"Existing Keeper combat stays separate");
                var sim=new CombatSimulation(arena);sim.Start(29);sim.Champion.Definition.Health=sim.Champion.Health=100000;
                sim.Champion.NextAttack=float.PositiveInfinity;
                for(int i=0;i<80;i++)sim.Step(.05f,Vector2.zero);
                Check(sim.Boss.BossVolley>0,"Keeper still executes real attack pattern");
                sim.DamageFixture(sim.Boss,sim.Boss.Health+1);sim.Step(.05f,Vector2.zero);s.Settle(sim);
                Check(s.State.OutsideRewards.Count==3 && s.State.OutsideStage==OutsideStageStatus.Reward,"Keeper preserves three Prismatic gifts");
                Check(s.SecretPortals().Length==0,"Secret doors stay absent during Keeper reward selection");
                Roundtrip(s,"Keeper rewards alongside secret history");
                Check(s.ClaimOutsideStageReward(0) && !s.ClaimOutsideStageReward(0),"Keeper reward still claims once");
            }
            using (RunSession s = Room(SecretEventKind.LostProperty))
            {
                s.ChooseSecretEvent(0);var sim=new CombatSimulation(s.PrepareCombat());sim.Start(5);
                sim.DamageFixture(sim.Champion,sim.Champion.Health+1);s.Settle(sim);
                Check(s.State.Phase==SessionPhase.Defeat && !s.ClaimSecretBattleReward() && !s.LeaveSecretEvent(),"Optional battle defeat ends run without a reward escape");
                Roundtrip(s,"secret defeat");
            }
            foreach (SecretEventKind kind in Enum.GetValues(typeof(SecretEventKind)))
            using (RunSession s = Room(kind, true))
            {
                Check(!s.SecretChoices().Any(o => o.Enabled && o.RewardId != null && o.RewardId.StartsWith("Secret") && c.Unit(o.RewardId) != null), "Solo receives no permanent ally " + kind);
                Roundtrip(s, "solo " + kind);
            }
            using (RunSession s = Room(SecretEventKind.DeadLetters))
            {
                Check(s.ChooseSecretEvent(2), "Post outside-stage letter"); s.LeaveSecretEvent();
                s.State.Wave++; s.State.Secrets.Visits = 2;
                Check(s.SecretPortals().SequenceEqual(new[] { SecretEventKind.EmptyTheatre }), "Letter unlocks saved follow-up past two-visit cap");
                Roundtrip(s, "letter chain");
                Check(s.EnterSecretEvent(SecretEventKind.EmptyTheatre) && s.State.Secrets.Visits == 2, "Follow-up does not overrun visit validator");
            }
            using (RunSession s = Room(SecretEventKind.LostProperty))
            {
                int gold = s.State.Gold; Check(s.LeaveSecretEvent() && s.State.Gold == gold, "Decline has no currency penalty");
                s.State.Secrets = null; Roundtrip(s, "legacy save without secret fields");
            }
            string output = "PreviewExports/Secrets"; Directory.CreateDirectory(output);
            File.WriteAllLines(output + "/Focused_Checks.txt", Notes);
            Debug.Log("SECRET_EVENT_CHECKS_PASSED " + Notes.Count);
        }
        [Serializable] private sealed class Offers { public List<ShopOffer> Rows; }
    }
}
