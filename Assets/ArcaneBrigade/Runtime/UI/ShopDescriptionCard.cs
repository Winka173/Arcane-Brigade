using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A data-driven purchase card. Bonds count bodies; support counts stars; attack ranges never promise a rolled weapon.</summary>
    public sealed class ShopDescriptionCard
    {
        public sealed class BondRow { public BondDefinition Bond; public int Count, Next; }
        public readonly List<BondRow> Bonds = new List<BondRow>();
        public string Summary, Details, Identity;
        public ItemRarity Rarity;
        public Sprite Portrait;
        public RecruitAttackReadout Attack;
        private string cacheKey;
        private int refreshedFrame = -1;
        private RunSession refreshedSession;
        private ShopOffer refreshedOffer;
        private GUIStyle label, value, muted;

        public void Refresh(RunSession session, RunCatalog catalog, ShopOffer offer)
        {
            if (Application.isPlaying && refreshedFrame == Time.frameCount && ReferenceEquals(session, refreshedSession) && ReferenceEquals(offer, refreshedOffer)) return;
            refreshedFrame = Time.frameCount; refreshedSession = session; refreshedOffer = offer;
            string key = JsonUtility.ToJson(session.State) + ":" + JsonUtility.ToJson(offer);
            if (key == cacheKey) return;
            cacheKey = key; Bonds.Clear(); Attack = null;
            if (offer.Kind == OfferKind.Item)
            {
                var item = catalog.Item(offer.Id); Rarity = item.Rarity; Portrait = item.Icon;
                Identity = RelicDescriptions.Category(item) + " · " + RunModifierRules.RarityName(Rarity) + (item.Stacking == StackPolicy.Unique ? " · UNIQUE" : " · STACKABLE");
                Details = RelicDescriptions.Describe(item, session.StackCount(item.Id) + 1, catalog) + RelicDescriptions.PurchaseReactions(session, catalog, item);
                var copy = new StringBuilder();
                copy.AppendLine("WHILE OWNED · per copy");
                foreach (var m in item.EffectiveModifiers)
                    if (m.Value != 0) copy.AppendLine(GlossaryUI.Modifier(m.Effect, m.Scope, m.Value));
                // Every condition, proc, purchase reaction and cap remains available in the main scroll as well as INFO.
                int effects = Details.IndexOf("\nSPECIAL EFFECT", StringComparison.Ordinal);
                int synergy = Details.IndexOf("\nWEAPON SYNERGY", StringComparison.Ordinal);
                int trigger = Details.IndexOf("\nTRIGGER:", StringComparison.Ordinal);
                int owned = Details.IndexOf("\nOwned effective copies:", StringComparison.Ordinal);
                int start = new[] { effects, synergy, trigger, owned }.Where(n => n >= 0).DefaultIfEmpty(0).Min();
                if (item.Category != RelicCategory.Curio) copy.AppendLine("ON PURCHASE · counts as one " + RelicDescriptions.Category(item).ToLowerInvariant() + ". No separate use action.");
                copy.Append(Details.Substring(start)); Summary = copy.ToString().Trim();
                return;
            }
            var unit = catalog.Unit(offer.Id); Rarity = unit.Rarity; Portrait = unit.Body;
            Identity = "COMPANION · " + RunModifierRules.RarityName(Rarity) + " · " + offer.Tier + " STAR";
            Attack = session.InspectRecruitAttack(unit.Id, offer.Tier);
            foreach (string tag in unit.Tags ?? Array.Empty<string>())
            {
                var bond = Array.Find(catalog.Bonds, b => b.Id == tag); if (bond == null) continue;
                session.BondCounts.TryGetValue(tag, out int count);
                Bonds.Add(new BondRow { Bond = bond, Count = count, Next = bond.NextThreshold(count) });
            }
            var b = new StringBuilder();
            b.AppendLine("SUPPORT WHILE OWNED · offered " + offer.Tier + "-star body");
            if (!(unit.CompanionModifiers ?? Array.Empty<StatModifier>()).Any(m => m.Value != 0)) b.AppendLine("No direct stat support; talent and personal passive are listed below.");
            foreach (var m in unit.CompanionModifiers ?? Array.Empty<StatModifier>())
                if (m.Value != 0) b.AppendLine(GlossaryUI.Modifier(m.Effect, m.Scope, m.Value * UnitDefinition.ModifierScale(offer.Tier)));
            b.AppendLine("TALENT · " + CompanionDescriptions.Talent(unit));
            if (unit.Passive != PassiveKind.None) b.AppendLine("PASSIVE · " + CompanionDescriptions.Passive(unit, offer.Tier, catalog.TierSupportMultiplier));
            int copies = session.State.Roster.Count(r => r.UnitId == unit.Id && r.Tier == offer.Tier);
            b.AppendLine("Immortal · " + copies + " matching " + offer.Tier + "-star bodies owned. " + (unit.ReferenceCompanion == "Evil" ? "Cannot merge." : offer.Tier >= catalog.MaximumTier ? "Maximum star level; no further merge." : "Three matching bodies merge; one of their weapons is kept."));
            b.AppendLine("Shown support is per body, not the purchase delta; merging replaces its ingredients.");
            b.AppendLine("Random weapon: displayed attack ranges use your current build before this purchase. " + (Attack.Melee && Attack.Ranged ? "Melee and ranged weapons are eligible." : Attack.Ranged ? "Ranged weapons are eligible." : "Close-range weapons are eligible."));
            string voice = CompanionVoice.Line(unit.Id, session.State.Wave);
            if (!string.IsNullOrEmpty(voice)) b.AppendLine("“" + voice + "”");
            Summary = b.ToString().Trim();
            Details = Summary + "\n\nLIFE IN THE WAITING REALM\n" + CreatureLives.Origin(unit.Id)
                + "\n\nSOURCE\n" + (unit.GiftOnly ? "Gift / nursery / special reward; not ordinary shop stock." : !string.IsNullOrEmpty(unit.ExclusiveHero) ? "Recruitment exclusive: " + unit.ExclusiveHero : unit.SpecialRecruit ? "Dream shop / special reward; hero theme and unlock wave apply." : "Regular shop; hero theme and unlock wave apply.")
                + "\nNamed talents count bodies and absorbed memories; star level only scales the separately listed support unless stated."
                + "\n\nWEAPON PREVIEW / CURRENT BUILD\n" + (Attack.Choices == 0 ? "No eligible unused weapon variant." : Attack.Choices + " eligible random weapons. Damage " + RecruitAttackReadout.Range(Attack.MinDamage, Attack.MaxDamage) + "; cycle " + RecruitAttackReadout.Range(Attack.MinCycle, Attack.MaxCycle) + "s; reach " + RecruitAttackReadout.Range(Attack.MinRange, Attack.MaxRange) + ". Bounds use the current build before recruiting. Different weapons produce different attacks; these ranges are not guaranteed together. Purchase talents and merges can change the final build.");
            foreach (var row in Bonds)
            {
                var bond = row.Bond;
                Details += "\n\n" + bond.Name + " / " + RunModifierRules.ScopeName(RunModifierRules.DefaultScope(bond.Effect)) + ": " + GlossaryUI.Modifier(bond.Effect, RunModifierRules.DefaultScope(bond.Effect), bond.BalanceReward(bond.Value)) + " every " + bond.Threshold + " bodies. Card shows cumulative reward at the next threshold.";
                foreach (var extra in bond.ExtraBonuses ?? Array.Empty<BondThresholdBonus>()) Details += " Extra at " + extra.Threshold + " bodies: " + GlossaryUI.Modifier(bond.Effect, RunModifierRules.DefaultScope(bond.Effect), bond.BalanceReward(extra.Value)) + ".";
            }
        }

        public float HeaderHeight => Attack == null ? 0 : Mathf.Max(125, Mathf.CeilToInt(Bonds.Count / 3f) * 115 + 8);
        public void DrawHeader(Rect bounds)
        {
            if (Attack == null) return;
            label ??= ArcadeTypography.Style(17, true); value ??= ArcadeTypography.Style(20, true); muted ??= ArcadeTypography.Style(15);
            Rect attack = new Rect(bounds.x, bounds.y, 220, 116); ArcadeSkin.Panel(attack, ArcadeSkin.Surface);
            DrawSprite(new Rect(attack.x + 8, attack.y + 10, 40, 40), Attack.Icon);
            label.normal.textColor = ArcadeSkin.Gold; label.fontSize = 16; GUI.Label(new Rect(attack.x + 52, attack.y + 7, 165, 25), "RANDOM · " + Attack.Choices, label);
            value.normal.textColor = ArcadeSkin.Cream;
            FitLabel(new Rect(attack.x + 12, attack.y + 34, 202, 26), Attack.Choices == 0 ? "No available variant" : "ATK " + RecruitAttackReadout.Range(Attack.MinDamage, Attack.MaxDamage), value, 20);
            muted.normal.textColor = ArcadeSkin.Teal;
            GUI.Label(new Rect(attack.x + 12, attack.y + 62, 202, 24), "Cycle " + RecruitAttackReadout.Range(Attack.MinCycle, Attack.MaxCycle) + "s", muted);
            GUI.Label(new Rect(attack.x + 12, attack.y + 85, 202, 24), "Reach " + RecruitAttackReadout.Range(Attack.MinRange, Attack.MaxRange), muted);
            int columns = Mathf.Clamp(Bonds.Count, 1, 3);
            float width = (bounds.width - 230 - (columns - 1) * 8) / columns;
            for (int i = 0; i < Bonds.Count; i++)
            {
                var row = Bonds[i]; var bond = row.Bond;
                Rect box = new Rect(bounds.x + 230 + i % columns * (width + 8), bounds.y + i / columns * 115, width, 108);
                ArcadeSkin.Panel(box, ArcadeSkin.Surface); DrawSprite(new Rect(box.x + 8, box.y + 10, 36, 36), bond.Icon);
                label.normal.textColor = ArcadeSkin.Cream; FitLabel(new Rect(box.x + 50, box.y + 5, width - 55, 23), bond.Name, label, 17);
                muted.normal.textColor = ArcadeSkin.Teal; GUI.Label(new Rect(box.x + 50, box.y + 28, width - 55, 24), row.Count + "/" + row.Next + " · " + RunModifierRules.ScopeName(RunModifierRules.DefaultScope(bond.Effect)), muted);
                StatGlyph.Draw(new Rect(box.x + 9, box.y + 55, 23, 23), bond.Effect);
                value.normal.textColor = BenefitColor(bond.Effect, bond.Reward(row.Next));
                FitLabel(new Rect(box.x + 39, box.y + 52, width - 44, 27), RunModifierRules.ValueText(bond.Effect, bond.Reward(row.Next)) + " " + ShortOfferCopy.StatName(bond.Effect), value, 20);
                muted.normal.textColor = ArcadeSkin.Teal;
                GUI.Label(new Rect(box.x + 10, box.y + 80, width - 15, 23), "Now " + RunModifierRules.ValueText(bond.Effect, bond.Reward(row.Count)) + " · next at " + row.Next, muted);
            }
        }
        private static void FitLabel(Rect rect, string text, GUIStyle style, int size)
        {
            style.fontSize = size; style.wordWrap = false; style.clipping = TextClipping.Clip;
            while (style.fontSize > 13 && style.CalcSize(new GUIContent(text)).x > rect.width) style.fontSize--;
            GUI.Label(rect, text, style);
        }
        public static Color BenefitColor(ItemEffect effect, float number)
        {
            bool enemy = effect == ItemEffect.EnemyCount || effect == ItemEffect.EnemyHealth || effect == ItemEffect.EnemySpeed;
            return number == 0 ? ArcadeSkin.Cream : (number > 0 != enemy ? ArcadeSkin.Better : ArcadeSkin.Worse);
        }
        public static void DrawSprite(Rect bounds, Sprite sprite)
        {
            if (sprite == null) return;
            Rect r = sprite.textureRect; float aspect = r.width / r.height;
            if (aspect > bounds.width / bounds.height) { float h = bounds.width / aspect; bounds.y += (bounds.height - h) * .5f; bounds.height = h; }
            else { float w = bounds.height * aspect; bounds.x += (bounds.width - w) * .5f; bounds.width = w; }
            GUI.DrawTextureWithTexCoords(bounds, sprite.texture, new Rect(r.x / sprite.texture.width, r.y / sprite.texture.height, r.width / sprite.texture.width, r.height / sprite.texture.height), true);
        }
    }
}
