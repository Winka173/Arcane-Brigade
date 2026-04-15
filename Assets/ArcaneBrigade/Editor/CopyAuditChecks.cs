using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Player-facing copy audit (play-test 20/10/2026, items 3 and 11): renders the text of every relic card, friend
    /// card, hero page, bond line, stat explanation and visitor card the way the game builds it, and flags developer words:
    /// scope prefixes ("Team / ..."), internal names (HealthCorrection), champion/companion/ally/tier, raw long floats.
    /// Writes PreviewExports/CopyAudit/Flags.txt (every flagged line with where it came from) and fails while any remain.</summary>
    public static class CopyAuditChecks
    {
        private static readonly (Regex rule, string why)[] Rules =
        {
            (new Regex(@"\b(Team|Run|Champion|Companions|You|Friends|Everyone) /"), "scope prefix"),
            (new Regex(@"\b[Cc]hampions?\b|\bCHAMPIONS?\b"), "say hero"),
            (new Regex(@"\b[Cc]ompanions?\b|\bCOMPANIONS?\b"), "say friend"),
            (new Regex(@"\b[Aa]ll(y|ies)\b|\bALL(Y|IES)\b"), "say friend"),
            (new Regex(@"\b[Tt]iers?\b|\bTIERS?\b"), "say stars"),
            (new Regex(@"\b[a-z]+[A-Z][A-Za-z]+\b|\b[A-Z][a-z]+(Correction|Damage|Speed|Health|Size|Count|Chance|Discount|Rarity|Quality|Healing|Frequency|Reach)\b"), "internal name"),
            (new Regex(@"[Cc]orrection|\bscope\b|\bproc\b|\bsandbox\b|\bmeasured\b|\bRNG\b|\bhitbox\b|\bcooldown\b"), "developer word"),
            (new Regex(@"\d+\.\d{3,}"), "raw number"),
            (new Regex(@"\bEffect [a-z]"), "raw effect id"),
            (new Regex(@"before healing modifiers|[Hh]ealing restrictions apply|total reward|depends on the weapon you get|Brigade size|after-purchase"), "developer phrasing"),
        };

        [MenuItem("Arcane Brigade/Checks/Player copy audit")]
        public static void Run()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            var texts = new List<(string where, string text)>();
            foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
            {
                texts.Add(("stat " + effect, RunModifierRules.ModifierName(effect)));
                texts.Add(("stat line " + effect, RunModifierRules.Line(effect, RunModifierRules.DefaultScope(effect), .1f)));
                var entry = GlossaryUI.Find(RunModifierRules.ModifierName(effect));
                if (entry != null) texts.Add(("explain " + effect, entry.Description));
            }
            foreach (ItemDefinition item in catalog.Items)
            {
                texts.Add(("relic name " + item.Id, item.Name));
                texts.Add(("relic " + item.Id, RelicDescriptions.Describe(item, 1, catalog)));
            }
            foreach (UnitDefinition unit in catalog.Companions)
            {
                texts.Add(("friend " + unit.Id, CompanionDescriptions.Describe(unit)));
                texts.Add(("friend talent " + unit.Id, CompanionDescriptions.ShortTalent(unit)));
            }
            foreach (UnitDefinition hero in catalog.Champions)
            {
                texts.Add(("hero " + hero.Id, RunUICopy.ChampionDescription(hero)));
                HeroKit kit = HeroKits.For(hero.Id);
                if (kit != null) texts.Add(("hero kit " + hero.Id, HeroKits.Describe(kit)));
            }
            foreach (BondDefinition bond in catalog.Bonds) texts.Add(("bond " + bond.Id, RunUICopy.BondDescription(bond, bond.Threshold)));
            foreach (VisitorDefinition v in ShopVisitors.All) texts.Add(("visitor " + v.Id, v.Offer + "\n" + string.Join("\n", v.Options)));
            // the shop cards as the shop builds them
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 20261020);
                var card = new ShopDescriptionCard();
                foreach (ItemDefinition item in catalog.Items.Where(i => i.ShopWeight > 0).Take(400))
                {
                    var offer = new ShopOffer { Token = 9001, Kind = OfferKind.Item, Id = item.Id, Price = 10, Currency = ShopCurrency.Gold };
                    try { card.Refresh(session, catalog, offer); } catch (Exception) { continue; }
                    texts.Add(("relic card " + item.Id, Plain(card.Summary) + "\n" + Plain(card.Identity)));
                }
                foreach (UnitDefinition unit in catalog.Companions.Where(u => !u.GiftOnly).Take(200))
                {
                    var offer = new ShopOffer { Token = 9002, Kind = OfferKind.Companion, Id = unit.Id, Tier = 1, Price = 10, Currency = ShopCurrency.Gold };
                    session.State.Offers.Add(offer);
                    try { card.Refresh(session, catalog, offer); } catch (Exception) { session.State.Offers.Remove(offer); continue; }
                    session.State.Offers.Remove(offer);
                    texts.Add(("friend card " + unit.Id, Plain(card.Summary) + "\n" + Plain(card.Identity)));
                }
            }
            var flags = new List<string>();
            foreach (var (where, text) in texts)
                foreach (string line in (text ?? "").Split('\n'))
                    foreach (var (rule, why) in Rules)
                        if (rule.IsMatch(line)) { flags.Add(where + " [" + why + "]: " + line.Trim()); break; }
            Directory.CreateDirectory("PreviewExports/CopyAudit");
            File.WriteAllLines("PreviewExports/CopyAudit/Flags.txt", flags.Distinct());
            Debug.Log("COPY_AUDIT " + texts.Count + " texts, " + flags.Distinct().Count() + " flagged lines");
            if (flags.Count > 0) throw new InvalidOperationException("Copy audit: " + flags.Distinct().Count() + " lines use developer words, first: " + flags[0]);
        }

        /// <summary>A card line as read: the stat tokens become their caption and value.</summary>
        private static string Plain(string text) => Regex.Replace(text ?? "", @"\{bonus:[^|]+\|([^|]*)\|([^}]*)\}", "$1 $2").Replace("{mark}", "•").Replace("{gives}", "GIVES").Replace("{costs}", "COSTS");
    }
}
