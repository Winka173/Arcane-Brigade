using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class RecruitmentChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string label)
        {
            notes.Add((ok ? "PASS " : "FAIL ") + label);
            Directory.CreateDirectory("PreviewExports/Recruitment");
            File.WriteAllLines("PreviewExports/Recruitment/Checks.txt", notes);
            if (!ok) throw new InvalidOperationException(label);
        }
        private static RunSession Session(RunCatalog c, int wave)
        {
            var s = new RunSession(c); s.Start("PatchworkHost", 161026);
            s.State.Phase = SessionPhase.Shop; s.State.Wave = wave;
            s.State.Roster.Clear(); s.State.Offers.Clear(); s.State.SpecialOffers.Clear();
            s.State.Gold = 100000000; s.State.Eggs = 100000;
            return s;
        }
        private static ShopOffer Offer(RunSession s, string id, bool special = false)
        {
            var offer = new ShopOffer { Token = ++s.State.NextOffer, Id = id, Kind = OfferKind.Companion, Price = 1, Currency = special ? ShopCurrency.Eggs : ShopCurrency.Gold };
            (special ? s.State.SpecialOffers : s.State.Offers).Add(offer); return offer;
        }
        private static void Roll(RunSession s, bool special)
        {
            typeof(RunSession).GetMethod(special ? "RollSpecialOffers" : "RollShop", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(s, special ? Array.Empty<object>() : new object[] { false });
        }
        public static void Run()
        {
            notes.Clear(); var c = Resources.Load<RunCatalog>("RunCatalog");
            var ordinary = c.Companions.Where(u => !u.SpecialRecruit && !u.GiftOnly && string.IsNullOrEmpty(u.ExclusiveHero) && !RunSession.StorySpecies(u.Id)).Take(5).ToArray();
            var specials = c.Companions.Where(u => u.SpecialRecruit && !u.GiftOnly).Take(3).ToArray();
            Check(ordinary.Length == 5 && specials.Length == 3, "Catalog supplies ordinary and special recruit fixtures");
            using (var known = Session(c, 2))
            {
                // owner 22/10/2026: a friend's weapon is known before buying, and buying gives exactly that weapon
                var offer = Offer(known, ordinary[0].Id); WeaponDefinition shown = known.OfferWeapon(offer);
                Check(shown != null && offer.WeaponId == shown.Id && known.OfferWeapon(offer) == shown, "Offer weapon is rolled once and kept on the offer");
                Check(known.Buy(offer.Token) && known.State.Roster.Any(r => r.UnitId == offer.Id && r.WeaponId == shown.Id), "Buying a friend gives the weapon shown on its offer");
            }
            foreach (int wave in new[] { 1, 2, 3, 4, 8, 20, 30 })
            {
                using var s = Session(c, wave); int limit = 1;
                string owned = ordinary[4].Id;
                s.State.Roster.Add(new RosterEntry { UnitId = owned, WeaponId = "SpoonSabre" });
                s.State.Roster.Add(new RosterEntry { UnitId = owned, WeaponId = "InkPistol" });
                Check(s.NewRecruitLimit == limit, "Round quota " + wave);
                for (int i = 0; i < limit; i++)
                {
                    Check(s.Buy(Offer(s, ordinary[i].Id).Token), "New normal species accepted " + wave + "/" + i);
                    Check(s.State.NewRecruitsThisWave == i + 1 && s.State.NewSpeciesRecruitsThisWave == i + 1, "New normal species increments both quotas " + wave + "/" + i);
                }
                var stale = Offer(s, ordinary[limit].Id); int gold = s.State.Gold, roster = s.State.Roster.Count;
                Check(s.RecruitQuotaBlocks(stale) && !s.CanBuy(stale) && !s.Buy(stale.Token), "Old offer cannot bypass exhausted quota " + wave);
                Check(s.State.Gold == gold && s.State.Roster.Count == roster, "Denied purchase leaves currency and roster unchanged " + wave);
                Check(s.Buy(Offer(s, owned).Token), "Third copy merges after exhausted new-species quota " + wave);
                Check(s.State.Roster.Any(r => r.UnitId == owned && r.Tier == 2) && s.State.NewSpeciesRecruitsThisWave == limit && s.State.NewRecruitsThisWave == limit + 1, "Upgrade consumes old purchase quota but not new-species quota " + wave);
                // since 19/10/2026: one copy of an owned species per wave, so a second copy waits for the next wave
                Check(s.CopyRecruitsRemaining == 0 && !s.Buy(Offer(s, owned).Token), "Second copy in one wave is blocked " + wave);
                Check(s.State.NewRecruitsThisWave == 2 && s.CompanionPurchasesRemaining == 0, "At most one new species and one copy per wave " + wave);
                for(int index=0;index<specials.Length;index++)
                {
                    var unit=specials[index];
                    var offer = Offer(s, unit.Id, true);
                    bool available=wave<10 && index==0;
                    Check(s.Buy(offer.Token)==available,"Special new species respects per-wave and shared cooldown " +wave+"/"+index);
                    Check(s.State.NewRecruitsThisWave == 2 && s.State.NewSpeciesRecruitsThisWave == limit, "Special purchase leaves regular quotas unchanged " + wave + "/" + unit.Id);
                }
                Check(s.Buy(Offer(s,owned,true).Token),"Special upgrade copy ignores regular and new-species quotas "+wave);
                // Sold normal offers are not serialized beyond a shop's slot capacity.
                s.State.Offers.Clear(); s.State.SpecialOffers.Clear();
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.NewRecruitsRemaining == 0 && !s.CanBuy(Offer(s, ordinary[limit].Id)), "Save/load cannot refresh quota " + wave);
                for (int roll = 0; roll < 8; roll++) // each refresh in a shop costs 50% more (play-test 20/10/2026): 8 is plenty
                {
                    Check(s.Reroll(), "Paid refresh allowed at quota " + wave + "/" + roll);
                    Check(s.State.Offers.All(o => o.Kind != OfferKind.Companion), "Normal rolls show items once the new species and the copy are used " + wave + "/" + roll);
                    Check(s.State.NewRecruitsThisWave == 2 && s.State.NewSpeciesRecruitsThisWave == limit, "Reroll never resets either quota " + wave + "/" + roll);
                }
            }
            using (var s = Session(c, 12))
            {
                Check(s.Buy(Offer(s, specials[0].Id, true).Token) && s.State.NewRecruitsThisWave == 0, "Special-first purchase leaves full normal quota");
                // since 19/10/2026 the dream shop no longer closes the regular shop's new species (its own pace is one every two waves)
                Check(s.Buy(Offer(s, ordinary[0].Id).Token) && s.State.NewRecruitsThisWave == 1, "Special-first recruit leaves the regular new species open");
                s.State.NewRecruitsThisWave = 3;
                Roll(s, true);
                Check(s.State.SpecialOffers.Where(o=>o.Kind==OfferKind.Companion).All(o=>s.OwnsCompanionSpecies(o.Id)),"Special rolls keep only upgrade species during cooldown");
                Check(s.State.SpecialOffers.Where(o => o.Kind == OfferKind.Companion).All(s.CanBuy), "Special duplicate generation and purchase agree");
            }
            foreach(bool specialFirst in new[]{false,true})
            using(var s=Session(c,10))
            {
                string first=specialFirst?specials[0].Id:ordinary[0].Id;
                Check(s.Buy(Offer(s,first,specialFirst).Token),"Wave 10 first new species accepted / special="+specialFirst);
                Check(s.State.LastNewShopSpeciesWave==10 && s.SpecialNewRecruitsRemaining==0 && (s.NewRecruitsRemaining==0)!=specialFirst,"The dream shop closes for both; the regular shop closes only its own new species");
                s.State.Offers.Clear();s.State.SpecialOffers.Clear();s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.NewSpeciesCooldown,"Saving cannot clear the shared cooldown");
                s.State.Wave=11;s.State.NewRecruitsThisWave=s.State.NewSpeciesRecruitsThisWave=s.State.SpecialNewSpeciesRecruitsThisWave=0;
                Check(s.Buy(Offer(s,ordinary[1].Id).Token)==specialFirst && !s.Buy(Offer(s,specials[1].Id,true).Token),"Following wave: the dream shop waits; the regular shop waits only after its own new species");
                Check(s.Buy(Offer(s,first,specialFirst).Token),"Duplicate remains buyable in the intervening wave");
                Check(s.Reroll() && s.NewSpeciesCooldown,"Reroll never bypasses two-wave cooldown");
                s.State.Wave=12;s.State.NewRecruitsThisWave=s.State.NewSpeciesRecruitsThisWave=s.State.SpecialNewSpeciesRecruitsThisWave=0;
                Check(s.Buy(Offer(s,specialFirst?ordinary[1].Id:specials[1].Id,!specialFirst).Token),"New species reopens after two waves in either shop");
            }
            using (var s = Session(c, 12))
            {
                s.State.Roster.Add(new RosterEntry { UnitId = ordinary[0].Id, WeaponId = "SpoonSabre" });
                s.State.Roster.Add(new RosterEntry { UnitId = ordinary[0].Id, WeaponId = "InkPistol" });
                s.State.NewRecruitsThisWave = s.State.NewSpeciesRecruitsThisWave = 1;
                bool found = false;
                for (int i = 0; i < 6; i++) { s.Reroll(); found |= s.State.Offers.Any(o => o.Kind == OfferKind.Companion && o.Id == ordinary[0].Id); }
                Check(found, "Merge drought protection still finds third copy after quota");
                var gift = c.Items.First(i => i.Mechanic == RelicMechanic.CompanionGift);
                var offer = new ShopOffer { Token = ++s.State.NextOffer, Id = gift.Id, Kind = OfferKind.Item, Price = 1 }; s.State.Offers.Add(offer);
                Check(s.Buy(offer.Token) && s.State.NewRecruitsThisWave == 2 && s.State.NewSpeciesRecruitsThisWave == 1 && s.State.Roster.All(r => r.UnitId == ordinary[0].Id), "Ordinary gifts use purchase quota but upgrades do not use new-species quota");
            }
            using (var s = Session(c, 8))
            {
                s.Start("LoneJelly", 161026); s.State.Wave = 8; s.State.Phase = SessionPhase.Shop; s.State.Gold = 100000;
                s.State.AbsorbedSkills.Add(ordinary[0].Id); s.State.NewRecruitsThisWave = s.State.NewSpeciesRecruitsThisWave = 1;
                Check(s.Buy(Offer(s, ordinary[0].Id).Token) && s.State.NewRecruitsThisWave == 2 && s.State.NewSpeciesRecruitsThisWave == 1, "Owned absorbed memory uses the copy quota but not new-species quota");
            }
            using (var s = Session(c, 4))
            {
                s.State.NewRecruitsThisWave = 3;
                s.State.Offers.Clear();
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.CopyRecruitsRemaining == 0 && s.CompanionPurchasesRemaining <= 1, "Legacy quota of three loads conservatively without granting free copies");
                s.State.Phase = SessionPhase.Combat;
                var arena = s.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0; arena.WaveDuration = .2f;
                var battle = new CombatSimulation(arena); battle.Start(161026);
                for (int i = 0; i < 60 && battle.Phase == RunPhase.Combat; i++) battle.Step(1f / 60, Vector2.zero);
                s.Settle(battle);
                Check(s.State.Phase == SessionPhase.Shop && s.State.NewRecruitsThisWave == 0 && s.State.NewSpeciesRecruitsThisWave == 0 && s.NewRecruitsRemaining == 1, "Completing next round resets both quotas through actual settlement");
            }
            float previous = 1;
            for (int count = 0; count <= 20; count++)
            {
                float chance = OctoberBalance.NewAllyChance(count, 4);
                Check(chance <= previous && chance >= .0199f, "New-species curve decreases with roster " + count); previous = chance;
                Check(OctoberBalance.NewAllyChance(count, 3) >= .7f, "First three rounds retain high new-species gate " + count);
            }
            Check(OctoberBalance.NewAllyChance(6, 4) < OctoberBalance.NewAllyChance(5, 4), "Reduction starts at six companions");
            using (var s = Session(c, 12))
            {
                var eligible = c.Companions.Where(u => !u.SpecialRecruit && !u.GiftOnly && u.UnlockWave <= 12 && K11HeroRules.Eligible(c.Unit(s.State.ChampionId), u)).ToArray();
                var counts = new List<int>();
                foreach (int size in new[] { 5, 6, 12 })
                {
                    s.State.Roster.Clear(); for (int i = 0; i < size; i++) s.State.Roster.Add(new RosterEntry { UnitId = eligible[i].Id, WeaponId = "SpoonSabre" });
                    s.State.NewRecruitsThisWave = 0; s.State.ShopRandom = 831010;
                    int newcomers = 0;
                    for (int i = 0; i < 400; i++) { Roll(s, false); newcomers += s.State.Offers.Count(o => o.Kind == OfferKind.Companion && !s.OwnsCompanionSpecies(o.Id)); }
                    counts.Add(newcomers);
                    Check(s.CompanionOfferChance == 1, "Crowd never suppresses owned-species offer slot " + size);
                }
                Check(counts[0] > counts[1] && counts[1] > counts[2], "Seeded shop rolls reduce new species at 5/6/12 bodies: " + string.Join("/", counts));
            }
            File.WriteAllText("PreviewExports/Recruitment/Verification.json", "{\"Passed\":true,\"Checks\":" + notes.Count + ",\"EarlyNewSpeciesLimit\":1,\"LaterNewSpeciesLimit\":1,\"TotalOrdinaryPurchaseLimit\":3,\"SpecialShopExempt\":false,\"SpecialNewSpeciesPerWave\":1,\"SharedCooldownStartsAtWave\":10,\"NewSpeciesWaveInterval\":2,\"UpgradeUsesTotalQuota\":true}");
            Debug.Log("RECRUITMENT_PASS " + notes.Count);
        }
        public static void VerifyAndBuild() { Run(); HeroIdentityChecks.VerifyAndBuild(); }
    }
}
