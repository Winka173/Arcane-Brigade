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
                // requirement 22/10/2026: a friend's weapon is known before buying, and buying gives exactly that weapon
                var offer = Offer(known, ordinary[0].Id); WeaponDefinition shown = known.OfferWeapon(offer);
                Check(shown != null && offer.WeaponId == shown.Id && known.OfferWeapon(offer) == shown, "Offer weapon is rolled once and kept on the offer");
                Check(known.Buy(offer.Token) && known.State.Roster.Any(r => r.UnitId == offer.Id && r.WeaponId == shown.Id), "Buying a friend gives the weapon shown on its offer");
            }
            // wave 19 is the last shop open to new friends (requirement 31/10/2026); the closing is checked in ReleaseFiveDifficultyChecks
            foreach (int wave in new[] { 1, 2, 3, 4, 8, 19 })
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
                // since 31/10/2026: copies have no per-wave cap (they only show up less often as more are bought)
                Check(s.CopyRecruitsRemaining > 0 && s.Buy(Offer(s, owned).Token), "A second copy in one wave can be bought " + wave);
                Check(s.CopyOfferChance == (wave >= 10 ? 25 : 10), "Each copy bought makes the next rarer (two bought) " + wave);
                for(int index=0;index<specials.Length;index++)
                {
                    var unit=specials[index];
                    var offer = Offer(s, unit.Id, true);
                    bool available=index==0;
                    Check(s.Buy(offer.Token)==available,"Special new species respects its independent visit quota " +wave+"/"+index);
                    Check(s.State.NewRecruitsThisWave == limit + 2 && s.State.NewSpeciesRecruitsThisWave == limit, "Special purchase leaves regular quotas unchanged " + wave + "/" + unit.Id);
                }
                Check(s.Buy(Offer(s,owned,true).Token),"Special upgrade copy ignores regular and new-species quotas "+wave);
                // Sold normal offers are not serialized beyond a shop's slot capacity.
                s.State.Offers.Clear(); s.State.SpecialOffers.Clear();
                s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.NewRecruitsRemaining == 0 && !s.CanBuy(Offer(s, ordinary[limit].Id)), "Save/load cannot refresh quota " + wave);
                for (int roll = 0; roll < 8; roll++) // each refresh in a shop costs 50% more (play-test 20/10/2026): 8 is plenty
                {
                    Check(s.Reroll(), "Paid refresh allowed at quota " + wave + "/" + roll);
                    Check(s.State.Offers.All(o => o.Kind != OfferKind.Companion || s.OwnsCompanionSpecies(o.Id)), "Normal rolls show no new species once the allowance is used (copies may still come) " + wave + "/" + roll);
                    Check(s.State.NewRecruitsThisWave == limit + 2 && s.State.NewSpeciesRecruitsThisWave == limit, "Reroll never resets either quota " + wave + "/" + roll);
                }
            }
            using (var s = Session(c, 12))
            {
                Check(s.Buy(Offer(s, specials[0].Id, true).Token) && s.State.NewRecruitsThisWave == 0, "Special-first purchase leaves full normal quota");
                // Each shop spends only its own allowance.
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
                Check(s.SpecialNewRecruitsRemaining==(specialFirst ? 0 : 1) && s.NewRecruitsRemaining==(specialFirst ? 1 : 0),"Each shop closes only its own new-species allowance");
                s.State.Offers.Clear();s.State.SpecialOffers.Clear();s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(!s.NewSpeciesCooldown && s.SpecialNewRecruitsRemaining==(specialFirst ? 0 : 1), "Reload preserves separate shop quotas without a shared cooldown");
                s.State.Wave=11;s.State.NewRecruitsThisWave=s.State.NewSpeciesRecruitsThisWave=s.State.SpecialNewSpeciesRecruitsThisWave=0;
                Check(s.Buy(Offer(s,ordinary[1].Id).Token) && s.Buy(Offer(s,specials[1].Id,true).Token),"Following wave: regular catch-up and a fresh dream visit are independent");
                Check(s.Buy(Offer(s,first,specialFirst).Token),"Duplicate remains buyable in the intervening wave");
                Check(s.Reroll() && s.NewRecruitsRemaining == 0 && s.SpecialNewRecruitsRemaining == 0, "Reroll never resets either per-visit or per-wave allowance");
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
                Check(s.Buy(offer.Token) && s.State.NewRecruitsThisWave == 3 && s.State.NewSpeciesRecruitsThisWave == 1 && s.State.Roster.All(r => r.UnitId == ordinary[0].Id), "Ordinary gifts (two copies, no copy cap) use purchase quota but upgrades do not use new-species quota");
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
                Check(s.NewRecruitsRemaining <= 1, "Legacy quota of three loads without granting extra new species");
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
                Check(counts.All(n => n >= 400), "Banked allowance guarantees a new-species offer even with larger deployed teams: " + string.Join("/", counts));
            }
            // requirement 22/10/2026: the shop never sells what can't be bought. A new species left on the shelf when the new-species
            // quota closes is replaced in its place (a copy of an owned species within the copy quota, or a relic)
            foreach (int wave in new[] { 2, 8, 18 }) // 18, then 19: the last shop open to new friends
            {
                using var s = Session(c, wave);
                var first = Offer(s, ordinary[0].Id); var second = Offer(s, ordinary[1].Id); int shelf = s.State.Offers.Count;
                Check(s.Buy(first.Token) && s.NewRecruitsRemaining == 0, "New species bought closes the quota " + wave);
                Check(!s.State.Offers.Contains(second) && s.State.Offers.Count == shelf, "The other new species leaves the shelf, its slot refilled " + wave);
                Check(s.State.Offers.Where(o => !o.Sold).All(s.CanBuy), "Every unsold regular offer can be bought after the quota closes " + wave);
                Check(s.State.Offers.Count(o => !o.Sold && o.Kind == OfferKind.Companion) <= s.CopyRecruitsRemaining, "Stand-in copies respect the copy quota " + wave);
                for (int roll = 0; roll < 6; roll++) { Check(s.Reroll(), "Refresh after quota " + wave); Check(s.State.Offers.All(o => o.Kind != OfferKind.Companion || s.OwnsCompanionSpecies(o.Id)), "Refresh rolls no new species while the quota is closed " + wave + "/" + roll); }
                s.State.Wave = wave + 1; s.State.NewRecruitsThisWave = s.State.NewSpeciesRecruitsThisWave = 0;
                for (int roll = 0; roll < 6; roll++)
                {
                    s.Reroll();
                    Check(s.NewRecruitsRemaining == 1 && s.State.Offers.Any(o => o.Kind == OfferKind.Companion && !s.OwnsCompanionSpecies(o.Id)), "Next wave rolls a new species from unspent allowance " + wave + "/" + roll);
                }
            }
            // Kettle Bat (Scavenge 0.5) once found "0 gold": at least 1, growing with stars and with the wave, and the text says so
            {
                var bat = c.Unit("KettleBat");
                using var early = Session(c, 1); using var late = Session(c, 20);
                int fewer = 0;
                for (int tier = 1; tier <= 3; tier++)
                {
                    int now = early.ScavengeGold(bat, tier), later = late.ScavengeGold(bat, tier);
                    Check(now >= 1 && now > fewer && later > now, "Kettle Bat scavenges " + now + " gold at wave 1, " + later + " at wave 20, " + tier + " stars");
                    Check(CompanionDescriptions.ShortPassive(bat, tier, c.TierSupportMultiplier, late.GoldScale).Contains(" " + later + " gold"), "Kettle Bat card shows the gold it finds " + tier);
                    fewer = now;
                }
                late.State.Roster.Add(new RosterEntry { UnitId = bat.Id, WeaponId = "SpoonSabre", Tier = 1 }); late.State.Phase = SessionPhase.Combat;
                var arena = late.PrepareCombat(); var fielded = arena.StartingCompanions.FirstOrDefault(u => u != null && u.Id == bat.Id);
                Check(fielded != null && Mathf.RoundToInt(fielded.PassiveStrength) == late.ScavengeGold(bat, 1), "Kettle Bat in combat finds the gold its card shows");
            }
            File.WriteAllText("PreviewExports/Recruitment/Verification.json", "{\"Passed\":true,\"Checks\":" + notes.Count + ",\"RegularNewSpeciesPerWave\":1,\"RegularCopyPurchasesPerWave\":1,\"UnusedAllowanceCarriesOver\":true,\"NewSpeciesWaveInterval\":2,\"DreamShopIndependent\":true,\"SpecialNewSpeciesPerVisit\":1}");
            Debug.Log("RECRUITMENT_PASS " + notes.Count);
        }
        public static void VerifyAndBuild() { Run(); HeroIdentityChecks.VerifyAndBuild(); }
        /// <summary>The shop economy pair in one launch: recruitment quotas and the visitors.</summary>
        public static void RunWithVisitors() { Run(); VisitorChecks.Run(); }
    }
}
