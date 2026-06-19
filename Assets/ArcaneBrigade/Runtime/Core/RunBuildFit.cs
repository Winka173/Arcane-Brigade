using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Build-fit badge for a shop offer (18/10/2026): does it move the build forward? A friend that activates or raises
    /// one of its bonds reads BOND (with that bond's icon); a relic of the hero's own item family reads BUILD. Merges keep their
    /// MERGE badge (ShopMergePreview).</summary>
    public sealed partial class RunSession
    {
        public string BuildFit(ShopOffer offer, out Sprite icon)
        {
            icon = null;
            if (offer == null || offer.Sold) return null;
            UnitDefinition hero = catalog.Unit(State.ChampionId);
            if (offer.Kind == OfferKind.Companion)
            {
                UnitDefinition unit = catalog.Unit(offer.Id);
                if (unit == null || OwnsCompanionSpecies(offer.Id) && Stranger(offer.Id)) return null;
                foreach (string tag in unit.Tags ?? Array.Empty<string>())
                {
                    BondDefinition bond = Array.Find(catalog.Bonds, b => b.Id == tag); if (bond == null) continue;
                    BondCounts.TryGetValue(tag, out int count);
                    if (count + 1 >= bond.NextThreshold(count)) { icon = bond.Icon; return "BOND"; }
                }
                return null;
            }
            ItemDefinition item = catalog.Item(offer.Id);
            if (item == null || hero == null) return null;
            foreach (string theme in item.ShopThemes ?? Array.Empty<string>())
                if (theme.StartsWith("fam-", StringComparison.Ordinal) && Array.IndexOf(hero.RecruitThemes, theme) >= 0) { icon = item.Icon; return "FOR HERO"; } // a relic of the hero's own theme
            return null;
        }
    }
}
