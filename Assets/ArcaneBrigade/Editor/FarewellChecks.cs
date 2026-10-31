using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Changing the brigade (requirement 31/10/2026): seeing a friend home, the ferry, the Last Customer's trade and the
    /// dream shop's Starlight Thread.</summary>
    public static class FarewellChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Check(bool ok, string label)
        {
            notes.Add((ok ? "PASS " : "FAIL ") + label);
            Directory.CreateDirectory("PreviewExports/Recruitment");
            File.WriteAllLines("PreviewExports/Recruitment/FarewellChecks.txt", notes);
            if (!ok) throw new InvalidOperationException(label);
        }
        private static RunSession Session(RunCatalog c, int wave, params (string id, int tier)[] roster)
        {
            var s = new RunSession(c); s.Start("PatchworkHost", 161026);
            s.State.Phase = SessionPhase.Shop; s.State.Wave = wave;
            s.State.Roster.Clear(); s.State.Offers.Clear(); s.State.SpecialOffers.Clear();
            s.State.Gold = 1000; s.State.Eggs = 1000;
            string[] weapons = { "SpoonSabre", "InkPistol" };
            int w = 0;
            foreach (var (id, tier) in roster) s.State.Roster.Add(new RosterEntry { UnitId = id, Tier = tier, WeaponId = weapons[w++ % weapons.Length] });
            s.Rescan();
            return s;
        }

        public static void Run()
        {
            notes.Clear(); var c = Resources.Load<RunCatalog>("RunCatalog");
            var ordinary = c.Companions.Where(u => !u.SpecialRecruit && !u.GiftOnly && string.IsNullOrEmpty(u.ExclusiveHero) && !RunSession.StorySpecies(u.Id)).Take(3).Select(u => u.Id).ToArray();
            string a = ordinary[0], b = ordinary[1];

            using (var s = Session(c, 5, (a, 2), (b, 1)))
            {
                int gold = s.State.Gold, refund = s.SeeHomeRefund(a), banked = s.BankedNewSpeciesAllowance;
                Check(refund > 0 && s.SeeHomeReturnsPlace(a), "Seeing a friend home pays back and returns a place before wave 20");
                Check(s.SeeHome(a) && !s.State.Roster.Exists(r => r.UnitId == a), "A friend seen home leaves the brigade");
                Check(s.State.Gold == gold + refund && s.State.Letters == 1 && s.BankedNewSpeciesAllowance == banked + 1, "Half its cost back, a letter, a place for a new friend");
                s.State.Roster.Add(new RosterEntry { UnitId = ordinary[2], Tier = 1, WeaponId = "SpoonSabre" });
                Check(!s.SeeHomeOpen && !s.SeeHome(b), "Once a shop");
            }
            using (var s = Session(c, 5, (a, 1)))
                Check(s.PartingSpecies().Count == 0 && !s.SeeHome(a), "The last friend never leaves");
            using (var s = Session(c, 21, (a, 1), (b, 1)))
            {
                int carry = s.State.RegularAllowanceCarryover;
                Check(!s.SeeHomeReturnsPlace(a) && s.SeeHome(a) && s.State.RegularAllowanceCarryover == carry, "After wave 20 no place comes back");
            }

            using (var s = Session(c, 6, (a, 3), (b, 1)))
            {
                List<string> choices = s.FerryChoices(a);
                Check(choices.Count == 3 && choices.SequenceEqual(s.FerryChoices(a)) && choices.All(id => !s.OwnsCompanionSpecies(id)), "Three new friends wait on the far bank, the same each time");
                int eggs = s.State.Eggs, cost = s.FerryEggs;
                Check(s.Ferry(a, choices[0]) && !s.State.Roster.Exists(r => r.UnitId == a) && s.HighestOwnedTier(choices[0]) == 3, "Rowing across keeps the stars");
                Check(s.State.Eggs == eggs - cost && !s.FerryOpen && !s.Ferry(b, s.FerryChoices(b).FirstOrDefault()), "The ferry costs eggs and crosses once a shop");
            }
            using (var s = Session(c, 6, (a, 1), (b, 1)))
            {
                s.State.Eggs = s.FerryEggs - 1;
                Check(!s.Ferry(a, s.FerryChoices(a)[0]), "No crossing without the eggs");
            }

            int opened = 0, tried = 0;
            for (int wave = 1; wave <= 120; wave++)
            {
                using var s = Session(c, wave, (a, 2), (b, 1));
                if (wave < RunSession.MemorySaleFrom) { Check(!s.MemorySaleOpen, "The Last Customer's trade waits until shop 9 (" + wave + ")"); continue; }
                tried++;
                if (!s.MemorySaleOpen) continue;
                opened++;
                int gold = s.State.Gold, price = s.MemorySalePrice(a), carry = s.State.RegularAllowanceCarryover;
                Check(price == s.SeeHomeRefund(a) * 2 || price == s.SeeHomeRefund(a) * 2 - 1, "He pays all it cost " + wave);
                Check(s.SellToCustomer(a) && s.State.Gold == gold + price && s.State.Letters == 0 && s.State.RegularAllowanceCarryover == carry && !s.MemorySaleOpen, "Gold only: no letter, no place " + wave);
            }
            Check(opened > tried / 10 && opened < tried * 2 / 5, "The Last Customer comes now and then (" + opened + " of " + tried + ")");

            int threads = 0;
            for (int wave = 10; wave <= 200; wave++)
            {
                using var s = Session(c, wave, (a, 4), (b, 1));
                typeof(RunSession).GetMethod("RollSpecialOffers", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(s, Array.Empty<object>());
                ShopOffer thread = s.State.SpecialOffers.Find(o => o.Id == StarlightThreadItem.Id);
                if (wave < RunSession.ThreadFromWave) { Check(thread == null, "No thread before wave 15 (" + wave + ")"); continue; }
                if (thread == null) continue;
                threads++;
                Check(thread.Currency == ShopCurrency.Eggs && thread.Price == s.ThreadEggs && c.Item(thread.Id) != null, "The thread is sold for eggs " + wave);
                Check(s.Buy(thread.Token) && s.HighestOwnedTier(a) == 5 && s.StackCount(thread.Id) == 0, "Buying a thread makes the 4-star friend 5 stars " + wave);
            }
            Check(threads > 3 && threads < 50, "The dream shop sells a thread now and then (" + threads + " of 186)");

            using (var s = Session(c, 5, (a, 1), (b, 1)))
            {
                s.State.Letters = 5;
                var haul = MetaProgression.Haul(new PlayerProfile(), s.State, c);
                Check(haul.lines.Contains("oil|Letters from home ×3|" + 3 * RunSession.OilPerLetter), "Letters bring Lamp Oil at the end, three at most");
            }
            Debug.Log("FarewellChecks: " + notes.Count + " passed");
        }
    }
}
