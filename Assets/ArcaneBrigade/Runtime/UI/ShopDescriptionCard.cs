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
                if (item.Mechanic == RelicMechanic.ChaosIncome) copy.AppendLine("{mark} Each wave start: " + session.ScaledGold(25) + "-" + session.ScaledGold(75) + " random gold."); // on the gold curve
                else if (item.Mechanic != RelicMechanic.None) copy.AppendLine("{mark} " + RelicDescriptions.ShortMechanic(item.Mechanic));
                if (item.SignatureRole != SignatureRelicKind.None) copy.AppendLine("{mark} " + RelicDescriptions.ShortSignature(item.SignatureRole));
                foreach (var proc in catalog.ProcRules ?? Array.Empty<ProcRule>())
                    if (proc.ItemId == item.Id) copy.AppendLine("{mark} " + ProcLine(proc));
                if (item.Category != RelicCategory.Curio) copy.AppendLine("{mark} Counts as a " + RelicDescriptions.Category(item).ToLowerInvariant() + ".");
                AppendCosts(copy, item.EffectiveModifiers, 1);
                int held = session.StackCount(item.Id);
                if (held > 0 || item.StackLimit < 100000) copy.AppendLine("Owned " + held + (item.StackLimit < 100000 ? " / " + item.StackLimit : "") + (item.Stacking == StackPolicy.Unique ? " · unique" : ""));
                Summary = WithBloodCost(copy.ToString().Trim(), offer); Details = WithBloodCost(Details, offer);
                return;
            }
            var unit = catalog.Unit(offer.Id); Rarity = unit.Rarity; Portrait = unit.Body;
            Role = CompanionRoles.For(unit);
            Identity = CompanionRoles.Word(Role.Value) + " · " + RunModifierRules.RarityName(Rarity) + " · " + offer.Tier + " STAR";
            Attack = session.InspectOfferAttack(offer); // exactly the weapon this offer comes with (rolled before the preview buys it)
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
            // the skill, talent and passive belong to GIVES (before the COSTS plate), never as stray lines under it
            if (CombatSimulation.SkillCopy.TryGetValue(unit.Id, out string skill)) b.AppendLine("{mark} " + skill);
            string talent = CompanionDescriptions.ShortTalent(unit);
            if (unit.ReferenceCompanion is "\"The Creator\" Roger" or "Smiley Fox") talent = ShopVisitors.ScaleText(talent, session.GoldScale); // their gold is on the curve
            if (!string.IsNullOrEmpty(talent) && talent != skill) b.AppendLine("{mark} " + talent);
            string passive = CompanionDescriptions.ShortPassive(unit, offer.Tier, catalog.TierSupportMultiplier, session.GoldScale);
            if (passive.Length > 0) b.AppendLine("{mark} " + passive);
            AppendCosts(b, unit.CompanionModifiers ?? Array.Empty<StatModifier>(), UnitDefinition.ModifierScale(offer.Tier));
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
            b.AppendLine(session.UpgradeProgress(offer));
            b.AppendLine(unit.ReferenceCompanion == "Evil" ? "Owned " + copies + " · never merges"
                : offer.Tier >= catalog.MaximumTier ? "Owned " + copies + " at " + offer.Tier + "-star · max stars"
                : "Owned " + copies + "/3 at " + offer.Tier + "-star" + (copies == 2 ? " · buying merges them into " + (offer.Tier + 1) + "-star" : ""));
            Summary = WithBloodCost(b.ToString().Trim(), offer);
            RosterEntry fieldedBefore = session.ActiveRoster.Find(r => r.UnitId == unit.Id);
            RosterEntry fieldedAfter = purchased ? preview.ActiveRoster.Find(r => r.UnitId == unit.Id) : null;
            if (fieldedBefore != null && fieldedAfter != null && fieldedAfter.Tier == fieldedBefore.Tier && !session.AbsorbsRecruitCopies)
                Summary = WithBloodCost(session.UpgradeProgress(offer) + "\nStored for merging. No extra fighter, bond member or passive stats now.\nPurchase-trigger talents still apply.", offer);
            else if (fieldedBefore != null && fieldedAfter != null && fieldedAfter.Tier > fieldedBefore.Tier)
                Summary = "Upgrade to " + fieldedAfter.Tier + " stars · still one fighter.\n" + Summary;
            string voice = CompanionVoice.Line(unit.Id, session.State.Wave, session.State);
            Details = Summary + (ComboLines.Length > 0 ? "\n\nCOMBOS" + ComboLines : "") + "\n\nTALENT\n" + CompanionDescriptions.Talent(unit)
                + (unit.Passive != PassiveKind.None ? "\nPASSIVE · " + CompanionDescriptions.Passive(unit, offer.Tier, catalog.TierSupportMultiplier) : "")
                + "\nOne fighter per species. Extra copies are saved upgrade materials: no attacks, bond counts or passive stats until merged. Three at the same star merge, keeping a weapon from the ingredients."
                + (Attack.Weapon != null ? "\nComes with its " + RunUICopy.WeaponName(Attack.Weapon.Id) + "." : "\nGets a random " + (Attack.Melee && Attack.Ranged ? "weapon" : Attack.Ranged ? "ranged weapon" : "close-range weapon") + " when bought.")
                + (string.IsNullOrEmpty(voice) ? "" : "\n“" + voice + "”")
                + "\n\nLIFE IN THE WAITING REALM\n" + CreatureLives.Origin(unit.Id)
                + "\n\nWHERE TO FIND\n" + (unit.GiftOnly ? "Only as a gift or a special reward." : !string.IsNullOrEmpty(unit.ExclusiveHero) ? "Only with " + ExclusiveHeroName(catalog, unit.ExclusiveHero) + "." : unit.SpecialRecruit ? "The dream shop and special rewards." : "The regular shop.")
                + (Attack.Choices == 0 ? "" : Attack.Weapon != null
                    ? "\n\nWeapon: " + RunUICopy.WeaponName(Attack.Weapon.Id) + " — " + RecruitAttackReadout.Damage(Attack.MaxDamage) + " damage, attacks every " + RecruitAttackReadout.Seconds(Attack.MaxCycle) + "s, reach " + RecruitAttackReadout.Reach(Attack.MaxRange)
                    : "\n\nWEAPON\nDamage " + RecruitAttackReadout.DamageRange(Attack.MinDamage, Attack.MaxDamage) + ", attacks every " + RecruitAttackReadout.Seconds(Attack.MinCycle) + "–" + RecruitAttackReadout.Seconds(Attack.MaxCycle) + "s.");
            foreach (var row in Bonds)
            {
                var bond = row.Bond;
                int nextAt = bond.NextThreshold(row.After);
                string stat = LowerFirst(ShortOfferCopy.StatName(bond.Effect));
                Details += "\n\n" + bond.Name + " " + row.After + "/" + nextAt + " · " + (row.AfterReward == 0 ? "nothing yet" : RunModifierRules.ValueText(bond.Effect, row.AfterReward) + " " + stat)
                    + (nextAt > row.After ? " · next at " + nextAt + ": " + RunModifierRules.ValueText(bond.Effect, bond.BalanceReward(bond.Value)) + " more" : "") + (row.Preview ? "" : " · cannot be bought now");
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
        public float HeaderHeight => Attack == null ? 0 : Mathf.Max(140, Mathf.CeilToInt(Bonds.Count / 3f) * 140 + 8);
        public void DrawHeader(Rect bounds, float clock = 0)
        {
            if (Attack == null) return;
            label ??= ArcadeTypography.Style(17, true); value ??= ArcadeTypography.Style(20, true); muted ??= ArcadeTypography.Style(15);
            Rect attack = new Rect(bounds.x, bounds.y, 220, 132); ArcadeSkin.CodePlate(attack, CodeTone.Navy);
            Rect ai = ArcadeSkin.Content(attack, KitPiece.Tile); // inside the tile's rim with the standard padding
            DrawSprite(new Rect(ai.x, ai.y, 24, 24), AttackIcon);
            // the weapon this friend comes with: its name, whole damage, its pace and reach (player numbers)
            label.normal.textColor = ArcadeSkin.Gold; label.fontSize = 16; GUI.Label(new Rect(ai.x + 30, ai.y, ai.width - 30, 24), "WEAPON", label);
            value.normal.textColor = ArcadeSkin.Cream;
            FitLabel(new Rect(ai.x, ai.y + 22, ai.width, 27), Attack.Choices == 0 ? "None available" : Attack.Weapon != null ? RunUICopy.WeaponName(Attack.Weapon.Id) : "Random weapon", value, 19);
            muted.normal.textColor = ArcadeSkin.Teal;
            FitLabel(new Rect(ai.x, ai.y + 51, ai.width, 19), RecruitAttackReadout.DamageRange(Attack.MinDamage, Attack.MaxDamage) + " damage · every " + RecruitAttackReadout.Seconds(Attack.MaxCycle) + "s", muted, 15);
            GUI.Label(new Rect(ai.x, ai.y + 69, ai.width, 19), "Reach " + RecruitAttackReadout.Reach(Attack.MaxRange), muted);
            int columns = Mathf.Clamp(Bonds.Count, 1, 3);
            float width = (bounds.width - 230 - (columns - 1) * 8) / columns;
            for (int i = 0; i < Bonds.Count; i++)
            {
                var row = Bonds[i]; var bond = row.Bond;
                Rect box = new Rect(bounds.x + 230 + i % columns * (width + 8), bounds.y + i / columns * 140, width, 132);
                bool activates = row.Activates;
                Color accent = BenefitColor(bond.Effect, row.AfterReward - row.BeforeReward);
                // code-drawn tile (owner, 21/10/2026): navy plate, lit and washed in the bond's colour when this purchase lights it
                if (activates) ArcadeSkin.BlinkStrong(box, clock); // the tile this purchase lights blinks strongly (owner, 22/10/2026)
                ArcadeSkin.CodePlate(box, CodeTone.Navy, activates ? 1 : 0);
                if (activates) { ArcadeSkin.BlinkOver(box, clock); ArcadeSkin.BlinkStrong(box, clock, true); }
                Rect bi = ArcadeSkin.Content(box, KitPiece.Tile); // inside the tile's rim with the standard padding
                bool animate = activates && !PresentationSettings.ReducedMotion && !PresentationSettings.ReducedEffects && !PresentationSettings.ReducedFlashes;
                if (activates) ArcadeSkin.RunningLight(box, animate ? clock : 0, accent); // two sparks chase round the tile
                float grow = animate ? 1 + Mathf.Sin(clock * 2.4f) * .045f : 1;
                DrawSprite(new Rect(bi.x + 13 - 13 * grow, bi.y + 13 - 13 * grow, 26 * grow, 26 * grow), bond.Icon);
                int target = activates ? row.Next : bond.NextThreshold(row.After);
                // the plain count after buying ("2/2", "4/6"), no ACTIVATES / MERGE wording
                string count = row.Preview ? row.After + "/" + target : row.Count + "/" + row.Next;
                muted.normal.textColor = activates ? new Color(1f, .95f, .75f) : ArcadeSkin.Teal; muted.alignment = TextAnchor.MiddleRight;
                FitLabel(new Rect(bi.xMax - 54, bi.y, 54, 26), count, muted, 17); muted.alignment = TextAnchor.MiddleLeft;
                label.normal.textColor = ArcadeSkin.Cream; FitLabel(new Rect(bi.x + 34, bi.y, bi.width - 34 - 58, 26), bond.Name, label, 17);
                StatGlyph.Draw(new Rect(bi.x, bi.y + 34, 23, 23), bond.Effect);
                value.normal.textColor = BenefitColor(bond.Effect, row.AfterReward);
                // nothing yet: say the next step in player words ("−7% enemy speed at 2"), never "0%"
                float stepReward = bond.BalanceReward(bond.Value);
                if (row.AfterReward == 0) value.normal.textColor = Color.Lerp(BenefitColor(bond.Effect, stepReward), ArcadeSkin.Cream, .35f);
                FitLabel(new Rect(bi.x + 30, bi.y + 32, bi.width - 30, 27), row.AfterReward == 0
                    ? RunModifierRules.ValueText(bond.Effect, stepReward) + " " + LowerFirst(ShortOfferCopy.StatName(bond.Effect)) + " at " + target
                    : RunModifierRules.ValueText(bond.Effect, row.AfterReward) + " " + ShortOfferCopy.StatName(bond.Effect), value, 19);
                if (!row.Preview) { muted.normal.textColor = ArcadeSkin.Teal; FitLabel(new Rect(bi.x, bi.y + 62, bi.width, 20), "Cannot be bought now", muted, 15); }
            }
        }
        /// <summary>The card's weapon chip: the weapon's short name and its whole damage ("Moon Scythe · 34 dmg").</summary>
        /// <summary>A generic icon by weapon type, never the weapon's own picture (owner, 22/10/2026): crossed swords for close-range
        /// weapons (the Striker badge), the bow for ranged ones (the ranged-damage stat icon).</summary>
        public Sprite AttackIcon => Attack == null ? null : (Attack.Weapon != null ? Attack.Weapon.Kind != WeaponKind.Projectile : !Attack.Ranged)
            ? CompanionRoles.Badge(CompanionRole.Striker) : rangedIcon != null ? rangedIcon : rangedIcon = Resources.Load<Sprite>("MienCho/ActOne/Stat_RangedDamage");
        private static Sprite rangedIcon;
        public string AttackLabel => Attack == null ? "" : Attack.Weapon != null ? RunUICopy.WeaponName(Attack.Weapon.Id) + " · " + RecruitAttackReadout.Damage(Attack.MaxDamage) + " dmg"
            : RecruitAttackReadout.DamageRange(Attack.MinDamage, Attack.MaxDamage) + " dmg";
        /// <summary>An HP-priced offer opens its COSTS with what it really costs (owner, 22/10/2026): "−N max HP for this run",
        /// before any other cost line (which then joins the same red plate).</summary>
        private static string WithBloodCost(string text, ShopOffer offer)
        {
            if (offer == null || offer.Currency != ShopCurrency.Health || string.IsNullOrEmpty(text)) return text;
            string line = "{costs} −" + offer.Price + " max HP for this run";
            int at = text.IndexOf("{costs} ", StringComparison.Ordinal);
            if (at >= 0) return text.Substring(0, at) + line + "\n" + text.Substring(at + 8);
            int owned = text.IndexOf("\nOwned ", StringComparison.Ordinal); // keep "Owned ..." the last line
            return owned >= 0 ? text.Substring(0, owned) + "\n" + line + text.Substring(owned) : text + "\n" + line;
        }
        private static string LowerFirst(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s.Substring(1); // "Max HP" -> "max HP"
        private static string ExclusiveHeroName(RunCatalog catalog, string id)
        {
            foreach (UnitDefinition hero in catalog.Champions) if (hero.Id == id) return RunUICopy.UnitName(hero);
            return "one hero";
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
