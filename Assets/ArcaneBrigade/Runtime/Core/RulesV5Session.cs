using UnityEngine;
namespace ArcaneBrigade
{
    public static class BondV5EconomyTuning { public const float VersePrice=.5f; public const float ExhibitHealth=.06f; }
    public static class BondV5ShopRules
    {
        public static void Apply(BondRunProgress progress,System.Collections.Generic.IList<ShopOffer> offers,System.Collections.Generic.IEnumerable<ItemDefinition> eligible)
        {
            if(progress==null || offers==null || eligible==null)return;
            for(int i=0;i<offers.Count;i++)
            {
                ShopOffer offer=offers[i];if(offer.Sold || offer.Kind!=OfferKind.Item)continue;
                if(progress.PendingFourLeaves>0)
                {
                    ItemDefinition current=null,upgrade=null;
                    foreach(ItemDefinition item in eligible)if(item.Id==offer.Id){current=item;break;}
                    if(current!=null)foreach(ItemDefinition item in eligible)
                        if((int)item.Rarity==(int)current.Rarity+1 && (upgrade==null || string.CompareOrdinal(item.Id,upgrade.Id)<0))upgrade=item;
                    if(upgrade!=null){offer.Id=upgrade.Id;progress.PendingFourLeaves--;}
                }
                if(progress.PendingRainVerses>0){offer.Price=Mathf.Max(1,Mathf.CeilToInt(offer.Price*BondV5EconomyTuning.VersePrice));progress.PendingRainVerses--;}
                return;
            }
        }
    }
    public sealed partial class RunSession
    {
        private float RulesV5GrowthShare(UnitDefinition unit, bool companion)
        {
            bool member = companion && System.Array.IndexOf(unit.Tags, "orchard") >= 0;
            return member ? 1 : BondCounts.TryGetValue("orchard", out int count) && count >= BondTuning.SecondStep ? FriendDefenseTuning.ItemShare : 0;
        }
        public float RulesV5BondStat(UnitDefinition unit, ItemEffect effect, bool companion)
            => BondV3Catalog.Stat(effect, BondCounts, companion ? unit.Tags : HeroRulesV3Catalog.MinorIds(unit.Id));

        // Only the hero's armour item points are shared; class and bond armor are not shared a second time.
        private float RulesV5ArmorItems()
        {
            float armor = 0;
            bool loom = MechanicStacks(RelicMechanic.DreamLoom) > 0;
            foreach(ItemStack stack in State.Items)
            {
                ItemDefinition item = catalog.Item(stack.Id);
                foreach(StatModifier modifier in item.EffectiveModifiers)
                    if(!SecretItemSuspended(stack.Id) && modifier.Effect == ItemEffect.Armor && RunModifierRules.Applies(modifier.Scope, ModifierScope.Champion))
                        armor += modifier.Value * (loom && item.Rarity == ItemRarity.Common ? 2 : 1) * item.EffectiveStacks(stack.Count)
                            * (modifier.Value > 0 && SecretItemImproved(stack.Id) ? 1.25f : 1) * (modifier.Value > 0 ? KitItemScale(item, stack.Count, ModifierScope.Champion) : 1);
            }
            return armor;
        }
        private void CompileRulesV5Friend(UnitDefinition unit, UnitDefinition source, int stars, bool companion)
        {
            unit.Weapon.Damage *= 1 + RulesV5BondStat(source, ItemEffect.Damage, companion);
            unit.Weapon.Cooldown /= 1 + RulesV5BondStat(source, ItemEffect.AttackSpeed, companion);
            unit.Weapon.Range *= 1 + RulesV5BondStat(source, ItemEffect.AttackRange, companion);
            unit.CriticalChance = Mathf.Clamp01(unit.CriticalChance + RulesV5BondStat(source, ItemEffect.Crit, companion));
            unit.BondLuck = RulesV5BondStat(source, ItemEffect.Luck, companion);
            unit.SkillHaste = RulesV5BondStat(source, ItemEffect.SkillHaste, companion);
            unit.Health *= (1 + RulesV5BondStat(source, ItemEffect.Health, companion) + (!companion && BondCounts.TryGetValue("exhibit-hall",out int exhibits) && exhibits>=BondTuning.FirstStep?BondV5EconomyTuning.ExhibitHealth:0)) * (companion ? FriendDefenseTuning.Health(source.Id, stars) : 1);
            unit.FriendArmorPoints = RulesV5BondStat(source, ItemEffect.Armor, companion);
            if(companion) unit.FriendArmorPoints += FriendDefenseTuning.Armor(source.Id, stars) + FriendDefenseTuning.SharedItemPoints(RulesV5ArmorItems())
                + Bonus(ItemEffect.Armor, ModifierScope.Companions) / RunModifierRules.ArmorPoint;
            unit.Weapon.Windup = Mathf.Min(unit.Weapon.Windup, unit.Weapon.Cooldown);
            unit.Weapon.Recovery = Mathf.Min(unit.Weapon.Recovery, unit.Weapon.Cooldown);
        }
    }
}
