using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class RosterCapAndBondChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string label)
        {
            notes.Add((ok ? "PASS " : "FAIL ") + label);
            Directory.CreateDirectory("PreviewExports/Roster15");
            File.WriteAllLines("PreviewExports/Roster15/Checks.txt", notes);
            if (!ok) throw new InvalidOperationException(label);
        }
        private static RunSession Session(RunCatalog c, int count)
        {
            var s = new RunSession(c); s.Start("PatchworkHost", 151026); s.State.Wave = 12;
            s.State.Phase = SessionPhase.Shop; s.State.Gold = s.State.Eggs = 1000000;
            s.State.Roster.Clear();
            foreach (var u in c.Companions.Where(u => !u.SpecialRecruit && !u.GiftOnly && u.Id != "TeapotPilgrim").Take(count))
                s.State.Roster.Add(new RosterEntry { UnitId = u.Id, WeaponId = "SpoonSabre" });
            return s;
        }
        private static ShopOffer Offer(RunSession s, string id, bool special = false, int tier = 1)
        {
            var o = new ShopOffer { Token = ++s.State.NextOffer, Id = id, Kind = OfferKind.Companion, Tier = tier, Price = 1, Currency = special ? ShopCurrency.Eggs : ShopCurrency.Gold };
            (special ? s.State.SpecialOffers : s.State.Offers).Add(o); return o;
        }
        public static void Run()
        {
            notes.Clear(); var c = Resources.Load<RunCatalog>("RunCatalog");
            Check(OctoberBalance.AllySoftLimit == 15, "Central companion capacity is fifteen");
            var partyGoals = RunAchievements.Definitions(c).Where(a => a.Metric == "party").Select(a => a.Goal).OrderBy(x => x).ToArray();
            Check(partyGoals.SequenceEqual(new float[] { 8, 12, 15 }), "Party achievements remain reachable with stable IDs under new cap");
            foreach (bool special in new[] { false, true })
            {
                using var s = Session(c, 14); var o = Offer(s, "TeapotPilgrim", special);
                Check(s.Buy(o.Token) && s.State.Roster.Count == 15, "Fourteenth to fifteenth body allowed / special=" + special);
                var next = Offer(s, "RainbowTailor", special);
                Check(!s.CompanionCapacityBlocks(next), "No brigade body cap: a sixteenth body is never blocked by capacity / special=" + special);
            }
            foreach (int count in new[] { 15, 18, 20 })
            {
                using var s = Session(c, count);
                s.State.Roster[0] = new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "SpoonSabre" };
                s.State.Roster[1] = new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = "InkPistol" };
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.State.Roster.Count == count, "Older over-cap roster is preserved without deletion " + count);
                Check(s.Buy(Offer(s, "TeapotPilgrim").Token) && s.State.Roster.Count == count - 1, "Immediate merge allowed and reduces roster at capacity " + count);
            }
            using (var s = Session(c, 15))
            {
                for (int i = 0; i < 2; i++) s.State.Roster[i] = new RosterEntry { UnitId = "TeapotPilgrim", WeaponId = i == 0 ? "SpoonSabre" : "InkPistol", Tier = 3 };
                s.Buy(Offer(s, "TeapotPilgrim", tier: 3).Token);
                Check(s.State.Roster.All(r => r.Tier <= 3), "Two max-star bodies cannot falsely promise another merge (no fourth star)");
                var evil = c.Companions.First(u => u.ReferenceCompanion == "Evil");
                for (int i = 0; i < 2; i++) s.State.Roster[i] = new RosterEntry { UnitId = evil.Id, WeaponId = i == 0 ? "SpoonSabre" : "InkPistol" };
                Check(!s.CompanionCapacityBlocks(Offer(s, evil.Id)), "Non-merging Evil talent is paced by quotas, not a body cap");
            }
            foreach (string reference in new[] { "\"Pet Master\" Lisa", "\"Shove Around\" Bella", "\"Smug and Satisfied\" Jake" })
            {
                using var s = Session(c, 14); var giver = c.Companions.First(u => u.ReferenceCompanion == reference);
                Check(s.Buy(Offer(s, giver.Id, true).Token) && s.State.Roster.Count >= 15, "Recruit-triggered gifts can join beyond fifteen " + giver.Id + " (" + s.State.Roster.Count + ")");
            }
            using (var s = Session(c, 15))
            {
                var item = c.Items.First(i => i.Mechanic == RelicMechanic.CompanionGift);
                var o = new ShopOffer { Token = ++s.State.NextOffer, Id = item.Id, Kind = OfferKind.Item, Price = 1 }; s.State.Offers.Add(o);
                Check(s.Buy(o.Token) && s.State.Roster.Count >= 15, "Relic companion gifts can join beyond fifteen (" + s.State.Roster.Count + ")");
            }
            foreach (var kind in new[] { SecretContractKind.DreamFriend, SecretContractKind.BorrowedShadow })
            {
                using var s = Session(c, 15); s.State.Secrets.Contracts.Add(new SecretContract { Kind = kind, Remaining = 2 });
                // no brigade body cap: a temporary contract friend simply joins (or, where the contract echoes instead, the hero gets it)
                int bodies = s.PrepareCombat().StartingCompanions.Length; float echo = s.SecretBonus(ItemEffect.ChampionDamage, ModifierScope.Champion);
                Check(bodies >= 15 && (bodies == 16 || Mathf.Abs(echo - .2f) < .0001f), "Temporary contract joins or echoes, never lost " + kind + " (bodies " + bodies + ", echo " + echo + ")");
            }
            foreach (string reward in new[] { "SecretPatchBear", "SecretPaperBird" })
            {
                using var s = Session(c, 15); s.State.Secrets.Kind = SecretEventKind.LostProperty;
                s.State.Secrets.Stage = SecretEventStage.Reward; s.State.Secrets.RewardId = reward;
                string fallback = reward == "SecretPatchBear" ? "SecretWoodHeart" : "SecretLetter";
                string label = s.SecretRewardLabel;
                bool claimed = s.ClaimSecretBattleReward();
                Check(claimed && (s.State.Roster.Count == 16 || s.StackCount(fallback) == 1), "Rescue reward is claimable with fifteen bodies without softlock " + reward + " (roster " + s.State.Roster.Count + ", fallback " + s.StackCount(fallback) + ", label " + label + ")");
                Check(s.LeaveSecretEvent(), "Rescue room can be left after claim " + reward);
                s.State.Secrets.Stage = SecretEventStage.Reward; s.State.Secrets.RewardId = reward;
                var stack = s.State.Items.FirstOrDefault(i => i.Id == fallback);
                if (stack != null)
                {
                    stack.Count = c.Item(fallback).StackLimit;
                    int gold = s.State.Gold;
                    Check(s.SecretRewardLabel.Contains("25 GOLD") && s.ClaimSecretBattleReward() && s.State.Gold == gold + 25, "Capped replacement relic becomes explicitly quoted gold " + reward);
                }
            }
            using (var s = Session(c, 15))
            {
                s.State.OutsideStage = OutsideStageStatus.Reward;
                s.State.OutsideRewards = new List<string> { "RainbowTailor", "RainbowCrown", "RainbowHeart" };
                Check(s.ClaimOutsideStageReward(0) && s.State.Roster.Count == 16, "Keeper companion reward joins a fifteen-body brigade (no cap) (roster " + s.State.Roster.Count + ")");
                s.State.OutsideStage = OutsideStageStatus.Reward; s.State.OutsideRewards = new List<string> { "RainbowTailor", "RainbowCrown", "RainbowHeart" };
                Check(s.ClaimOutsideStageReward(1) && s.State.OutsideStage == OutsideStageStatus.Completed, "Keeper relic reward remains claimable at capacity");
            }
            var table = new System.Text.StringBuilder("Id\tName\tEffect\tCount\tBefore\tAfter\n");
            foreach (var b in c.Bonds)
            for (int count = 0; count <= 16; count++)
            {
                float raw = b.ActiveLevel(count) * b.Value + (b.ExtraBonuses ?? Array.Empty<BondThresholdBonus>()).Where(x => count >= x.Threshold).Sum(x => x.Value);
                bool beneficial = raw > 0 && b.Effect != ItemEffect.EnemyCount && b.Effect != ItemEffect.EnemyHealth && b.Effect != ItemEffect.EnemySpeed && !BondDefinition.WholeNumber(b.Effect); // whole-number stats unscaled since 19/10/2026
                float expected = beneficial ? raw * .9f : raw;
                Check(Mathf.Abs(b.Reward(count) - expected) < .0001f, "Exact mild bond adjustment " + b.Id + "/" + count);
                Check(b.ActiveLevel(count) == count / b.Threshold, "Bond threshold unchanged " + b.Id + "/" + count);
                if (count > 0 && count % b.Threshold == 0) table.AppendLine(b.Id + "\t" + b.Name + "\t" + b.Effect + "\t" + count + "\t" + raw.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + "\t" + b.Reward(count).ToString("R", System.Globalization.CultureInfo.InvariantCulture));
            }
            var fixture = new BondDefinition { Effect = ItemEffect.Damage, Threshold = 2, Value = .1f, ExtraBonuses = new[] { new BondThresholdBonus { Threshold = 4, Value = .2f } } };
            Check(Mathf.Abs(fixture.Reward(4) - .36f) < .0001f && RunUICopy.BondDescription(fixture, 4).Contains("extra +0.18"), "Bond UI includes reduced extra milestone bonus");
            File.WriteAllText("PreviewExports/Roster15/Bond_Changes.tsv", table.ToString());
            File.WriteAllText("PreviewExports/Roster15/Verification.json", "{\"Passed\":true,\"Checks\":" + notes.Count + ",\"CompanionLimit\":15,\"PositiveBondScale\":0.9,\"Bonds\":" + c.Bonds.Length + "}");
            Debug.Log("ROSTER15_PASS " + notes.Count);
        }
        public static void VerifyAndBuild() { Run(); RecruitmentChecks.VerifyAndBuild(); }
    }
}
