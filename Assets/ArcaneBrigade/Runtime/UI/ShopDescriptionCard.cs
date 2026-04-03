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
        public sealed class BondRow
        {
            public BondDefinition Bond; public int Count, Next, After;
            public bool Preview;
            public float BeforeReward => Bond.Reward(Count);
            public float AfterReward => Bond.Reward(After);
            public bool Activates => Preview && After > Count && Mathf.Abs(AfterReward - BeforeReward) > .00001f;
        }
        public readonly List<BondRow> Bonds = new List<BondRow>();
        public string Summary, Details, Identity;
        /// <summary>A friend's role (null for relics) and whether one of its duet partners is already in the brigade.</summary>
        public CompanionRole? Role; public bool Combo; public string ComboLines = "";
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
                var item = catalog.Item(offer.Id); Rarity = item.Rarity; Portrait = item.Icon; Role = null; Combo = false;
                Identity = RelicDescriptions.Category(item) + " · " + RunModifierRules.RarityName(Rarity) + (item.Stacking == StackPolicy.Unique ? " · UNIQUE" : " · STACKABLE");
                Details = RelicDescriptions.Describe(item, session.StackCount(item.Id) + 1, catalog) + RelicDescriptions.PurchaseReactions(session, catalog, item);
                // the card: one line per stat (painted stat icon, exact value), one short line per special effect; every rule,
                // cap and condition stays under INFO (Details)
                var copy = new StringBuilder();
                // GIVES (green) first, then COSTS (red): a trade-off is never mixed into the bonuses (play-test 20/10/2026)
                AppendSplit(copy, item.EffectiveModifiers, 1, item.Mechanic != RelicMechanic.None || item.SignatureRole != SignatureRelicKind.None);
                if (item.Mechanic != RelicMechanic.None) copy.AppendLine("{mark} " + RelicDescriptions.ShortMechanic(item.Mechanic));
                if (item.SignatureRole != SignatureRelicKind.None) copy.AppendLine("{mark} " + RelicDescriptions.ShortSignature(item.SignatureRole));
                foreach (var proc in catalog.ProcRules ?? Array.Empty<ProcRule>())
                    if (proc.ItemId == item.Id) copy.AppendLine("{mark} " + ProcLine(proc));
                if (item.Category != RelicCategory.Curio) copy.AppendLine("{mark} Counts as a " + RelicDescriptions.Category(item).ToLowerInvariant() + ".");
                AppendCosts(copy, item.EffectiveModifiers, 1);
                int held = session.StackCount(item.Id);
                if (held > 0 || item.StackLimit < 100000) copy.AppendLine("Owned " + held + (item.StackLimit < 100000 ? " / " + item.StackLimit : "") + (item.Stacking == StackPolicy.Unique ? " · unique" : ""));
                Summary = copy.ToString().Trim();
                return;
            }
            var unit = catalog.Unit(offer.Id); Rarity = unit.Rarity; Portrait = unit.Body;
            Role = CompanionRoles.For(unit);
            Identity = CompanionRoles.Word(Role.Value) + " · " + RunModifierRules.RarityName(Rarity) + " · " + offer.Tier + " STAR";
            Attack = session.InspectRecruitAttack(unit.Id, offer.Tier);
            // Run the real recruitment/merge/absorption rules on an isolated copy, never on the live roster or RNG.
            using var preview = new RunSession(catalog);
            preview.Restore(MergeReveal.Capture(session.State));
            bool purchased = preview.Buy(offer.Token);
            foreach (string tag in unit.Tags ?? Array.Empty<string>())
            {
                var bond = Array.Find(catalog.Bonds, b => b.Id == tag); if (bond == null) continue;
                session.BondCounts.TryGetValue(tag, out int count);
                preview.BondCounts.TryGetValue(tag, out int after);
                Bonds.Add(new BondRow { Bond = bond, Count = count, Next = bond.NextThreshold(count), After = purchased ? after : count, Preview = purchased });
            }
            // Nursery transformations and purchase talents can affect bonds beyond the offered species' tags.
            if (purchased) foreach (var bond in catalog.Bonds)
            {
                session.BondCounts.TryGetValue(bond.Id, out int count); preview.BondCounts.TryGetValue(bond.Id, out int after);
                if (after != count && !Bonds.Any(row => row.Bond.Id == bond.Id))
                    Bonds.Add(new BondRow { Bond = bond, Count = count, Next = bond.NextThreshold(count), After = after, Preview = true });
            }
            // the card: support stats with their painted icons, then one short line each for the own skill, the talent, the
            // passive and where a merge stands; the full rules, the lore and the weapon preview stay under INFO (Details)
            var b = new StringBuilder();
            b.AppendLine(CompanionRoles.Tagline(Role.Value)); // what it does, first (play-test 20/10/2026, item 26)
            AppendSplit(b, unit.CompanionModifiers ?? Array.Empty<StatModifier>(), UnitDefinition.ModifierScale(offer.Tier), true);
            AppendCosts(b, unit.CompanionModifiers ?? Array.Empty<StatModifier>(), UnitDefinition.ModifierScale(offer.Tier));
            if (CombatSimulation.SkillCopy.TryGetValue(unit.Id, out string skill)) b.AppendLine("{mark} " + skill);
            string talent = CompanionDescriptions.ShortTalent(unit);
            if (!string.IsNullOrEmpty(talent) && talent != skill) b.AppendLine("{mark} " + talent);
            string passive = CompanionDescriptions.ShortPassive(unit, offer.Tier, catalog.TierSupportMultiplier);
            if (passive.Length > 0) b.AppendLine("{mark} " + passive);
            // its duets: who it pairs with and what they do together (lit when the partner is already in the brigade)
            Combo = false; ComboLines = "";
            foreach (Duet duet in Duets.For(unit.Id))
            {
                UnitDefinition partner = catalog.Unit(Duets.Partner(duet, unit.Id)); if (partner == null) continue;
                bool ready = session.OwnsCompanionSpecies(partner.Id);
                ComboLines += "\nCombo with " + RunUICopy.UnitName(partner) + ": " + duet.Line + (ready ? " ✓" : "");
                if (ready && !Combo) b.AppendLine("{mark} Combo with " + RunUICopy.UnitName(partner) + ": " + duet.Line + " ✓");
                Combo |= ready;
            }
            int copies = session.State.Roster.Count(r => r.UnitId == unit.Id && r.Tier == offer.Tier);
            b.AppendLine(unit.ReferenceCompanion == "Evil" ? "Owned " + copies + " · never merges"
                : offer.Tier >= catalog.MaximumTier ? "Owned " + copies + " at " + offer.Tier + "-star · max stars"
                : "Owned " + copies + "/3 at " + offer.Tier + "-star" + (copies == 2 ? " · buying merges them into " + (offer.Tier + 1) + "-star" : ""));
            Summary = b.ToString().Trim();
            string voice = CompanionVoice.Line(unit.Id, session.State.Wave, session.State);
            Details = Summary + (ComboLines.Length > 0 ? "\n\nCOMBOS" + ComboLines : "") + "\n\nTALENT\n" + CompanionDescriptions.Talent(unit)
                + (unit.Passive != PassiveKind.None ? "\nPASSIVE · " + CompanionDescriptions.Passive(unit, offer.Tier, catalog.TierSupportMultiplier) : "")
                + "\nImmortal. Shown support is per body; three matching bodies merge and keep one of their weapons."
                + "\nRandom weapon: the attack ranges use your current build. " + (Attack.Melee && Attack.Ranged ? "Melee and ranged weapons are eligible." : Attack.Ranged ? "Ranged weapons are eligible." : "Close-range weapons are eligible.")
                + (string.IsNullOrEmpty(voice) ? "" : "\n“" + voice + "”")
                + "\n\nLIFE IN THE WAITING REALM\n" + CreatureLives.Origin(unit.Id)
                + "\n\nSOURCE\n" + (unit.GiftOnly ? "Gift / nursery / special reward; not ordinary shop stock." : !string.IsNullOrEmpty(unit.ExclusiveHero) ? "Recruitment exclusive: " + unit.ExclusiveHero : unit.SpecialRecruit ? "Dream shop / special reward; hero theme and unlock wave apply." : "Regular shop; hero theme and unlock wave apply.")
                + "\nNamed talents count bodies and absorbed memories; star level only scales the separately listed support unless stated."
                + "\n\nWEAPON PREVIEW / CURRENT BUILD\n" + (Attack.Choices == 0 ? "No eligible unused weapon variant." : Attack.Choices + " eligible random weapons. Damage " + RecruitAttackReadout.Range(Attack.MinDamage, Attack.MaxDamage) + "; cycle " + RecruitAttackReadout.Range(Attack.MinCycle, Attack.MaxCycle) + "s; reach " + RecruitAttackReadout.Range(Attack.MinRange, Attack.MaxRange) + ". Bounds use the current build before recruiting. Different weapons produce different attacks; these ranges are not guaranteed together. Purchase talents and merges can change the final build.");
            foreach (var row in Bonds)
            {
                var bond = row.Bond;
                Details += "\n\n" + bond.Name + ": " + GlossaryUI.Modifier(bond.Effect, RunModifierRules.DefaultScope(bond.Effect), bond.BalanceReward(bond.Value)) + " every " + bond.Threshold + " bodies. " + (row.Preview ? "After buying: " + row.Count + " → " + row.After + " bodies; total reward " + RunModifierRules.ValueText(bond.Effect, row.AfterReward) + "." : "Purchase unavailable; no after-purchase result shown.");
                foreach (var extra in bond.ExtraBonuses ?? Array.Empty<BondThresholdBonus>()) Details += " Extra at " + extra.Threshold + " bodies: " + GlossaryUI.Modifier(bond.Effect, RunModifierRules.DefaultScope(bond.Effect), bond.BalanceReward(extra.Value)) + ".";
            }
        }

        /// <summary>The helpful stat lines, the first opened by a green GIVES plate when the card also has costs.</summary>
        private static void AppendSplit(StringBuilder copy, StatModifier[] modifiers, float scale, bool more)
        {
            bool costs = modifiers.Any(m => m.Value != 0 && !RunModifierRules.Helps(m.Effect, m.Value));
            bool first = true;
            foreach (var m in modifiers)
            {
                if (m.Value == 0 || !RunModifierRules.Helps(m.Effect, m.Value)) continue;
                copy.AppendLine((first && costs ? "{gives} " : "") + GlossaryUI.Modifier(m.Effect, m.Scope, m.Value * scale)); first = false;
            }
            if (first && costs && more) copy.Append("{gives} ");
        }
        /// <summary>The harmful stat lines, the first opened by a red COSTS plate.</summary>
        private static void AppendCosts(StringBuilder copy, StatModifier[] modifiers, float scale)
        {
            bool first = true;
            foreach (var m in modifiers)
            {
                if (m.Value == 0 || RunModifierRules.Helps(m.Effect, m.Value)) continue;
                copy.AppendLine((first ? "{costs} " : "") + GlossaryUI.Modifier(m.Effect, m.Scope, m.Value * scale)); first = false;
            }
        }

        private static string ProcLine(ProcRule p)
            => (p.Trigger == ProcTrigger.CoinCollected ? "Coins" : p.Trigger == ProcTrigger.ChestOpened ? "Chests" : "Kills") + ": " + (p.Chance < 1 ? (p.Chance * 100).ToString("0.#") + "% chance to " : "") + (p.Action == ProcAction.HealChampion ? "heal the hero " + p.Power.ToString("0.#") + " HP" : "blast nearby enemies for " + p.Power.ToString("0.#")) + ".";
        public float HeaderHeight => Attack == null ? 0 : Mathf.Max(125, Mathf.CeilToInt(Bonds.Count / 3f) * 115 + 8);
        public void DrawHeader(Rect bounds, float clock = 0)
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
                bool activates = row.Activates;
                Color accent = BenefitColor(bond.Effect, row.AfterReward - row.BeforeReward);
                ArcadeSkin.Panel(box, activates ? Color.Lerp(ArcadeSkin.Surface, accent, .16f) : ArcadeSkin.Surface);
                bool animate = activates && !PresentationSettings.ReducedMotion && !PresentationSettings.ReducedEffects && !PresentationSettings.ReducedFlashes;
                if (activates)
                {
                    ArcadeSkin.Line(new Rect(box.x, box.y, box.width, 3), accent);
                    ArcadeSkin.Line(new Rect(box.x, box.yMax - 3, box.width, 3), accent);
                    ArcadeSkin.Line(new Rect(box.x, box.y, 3, box.height), accent);
                    ArcadeSkin.Line(new Rect(box.xMax - 3, box.y, 3, box.height), accent);
                    if (animate)
                    {
                        float travel = Mathf.Repeat(clock * .28f, 1), shine = Mathf.Min(38, box.width * .2f);
                        ArcadeSkin.Line(new Rect(box.x + (box.width - shine) * travel, box.y, shine, 3), Color.Lerp(accent, Color.white, .65f));
                    }
                }
                float grow = animate ? 1 + Mathf.Sin(clock * 2.4f) * .045f : 1;
                DrawSprite(new Rect(box.x + 26 - 18 * grow, box.y + 28 - 18 * grow, 36 * grow, 36 * grow), bond.Icon);
                label.normal.textColor = ArcadeSkin.Cream; FitLabel(new Rect(box.x + 50, box.y + 5, width - 55, 23), bond.Name, label, 17);
                int target = activates ? row.Next : bond.NextThreshold(row.After);
                muted.normal.textColor = activates ? accent : ArcadeSkin.Teal;
                FitLabel(new Rect(box.x + 50, box.y + 28, width - 55, 24), (row.Preview ? row.Count + " → " + row.After + "/" + target : "Unavailable · " + row.Count + "/" + row.Next), muted, 15);
                StatGlyph.Draw(new Rect(box.x + 9, box.y + 55, 23, 23), bond.Effect);
                value.normal.textColor = BenefitColor(bond.Effect, row.AfterReward);
                FitLabel(new Rect(box.x + 39, box.y + 52, width - 44, 27), RunModifierRules.ValueText(bond.Effect, row.AfterReward) + " " + ShortOfferCopy.StatName(bond.Effect), value, 20);
                muted.normal.textColor = ArcadeSkin.Teal;
                muted.normal.textColor = activates ? accent : ArcadeSkin.Teal;
                string status = !row.Preview ? "Cannot buy this offer" : activates ? (row.BeforeReward == 0 ? "ACTIVATES ON BUY" : "UPGRADES ON BUY") : row.After < row.Count ? "MERGE · FEWER BOND BODIES" : row.AfterReward == 0 ? "NOT ACTIVE · next at " + target : "AFTER BUY · next at " + target;
                FitLabel(new Rect(box.x + 10, box.y + 80, width - 15, 23), status, muted, 15);
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
