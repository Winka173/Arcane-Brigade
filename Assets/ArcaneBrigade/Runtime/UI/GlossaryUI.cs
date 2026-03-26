using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class GlossaryUI
    {
        public sealed class Entry { public string Name; public string Description; public ItemEffect Effect; }
        private static readonly Dictionary<string, Entry> entries = Build();
        private static readonly Regex tokens = new Regex("(\\{mark\\}|\\{bonus:[^}]+\\}|[+−-]\\d+(?:\\.\\d+)?%?|(?<!\\p{L})\\d+(?:\\.\\d+)?(?:%|s)?|(?<!\\p{L})(?:" + string.Join("|", entries.Keys.OrderByDescending(key => key.Length).Select(Regex.Escape)) + ")(?!\\p{L})[.,:;!?)]*|\\r?\\n|[^\\S\\r\\n]+|[^\\s]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex signedValue = new Regex("^[+−-]\\d+(?:\\.\\d+)?%?$", RegexOptions.CultureInvariant);
        public static string Modifier(ItemEffect effect, ModifierScope scope, float number) => "{bonus:" + effect + "|" + RunModifierRules.ScopeName(scope) + " / " + RunModifierRules.ModifierName(effect) + "|" + RunModifierRules.ValueText(effect, number) + "}";
        private static ItemEffect NearbyEffect(MatchCollection all, int index)
        {
            int distance = 40; ItemEffect effect = ItemEffect.Damage;
            for (int i = Mathf.Max(0, index - 6); i <= Mathf.Min(all.Count - 1, index + 6); i++)
            {
                if (i == index) continue;
                var entry = Find(all[i].Value);
                if (entry == null || entry.Name == "Effect scope" || entry.Name == "Champion" || entry.Name == "Companions") continue;
                int gap = i < index ? all[index].Index - all[i].Index - all[i].Length : all[i].Index - all[index].Index - all[index].Length;
                if (gap >= distance) continue;
                distance = gap; effect = entry.Effect;
            }
            return effect;
        }
        private static Entry selected;
        private static float openedAt;
        private static readonly Dictionary<GUIStyle, GUIStyle> wordStyles = new Dictionary<GUIStyle, GUIStyle>();
        public static bool Open => selected != null;
        public static void Close() { if (selected != null) CheckpointAudio.PlayInterfaceId("ui.close"); selected = null; }
        public static Entry Find(string term) => entries.TryGetValue(term.Trim().TrimEnd('.', ',', ':', ';', '!', '?', ')'), out Entry entry) ? entry : null;
        public static bool Show(string term)
        {
            var next = Find(term);
            if (next != null && next != selected) CheckpointAudio.PlayInterfaceId("ui.tooltip");
            selected = next; openedAt = Time.unscaledTime; return selected != null;
        }
        public static void BlockUnderlying(Vector2 viewport)
        {
            if (selected == null) return;
            float width = Mathf.Min(620, viewport.x - 60);
            Rect panel = new Rect(viewport.x - width - 35, 150, width, 285);
            Event current = Event.current;
            if (!panel.Contains(current.mousePosition) || current.type != EventType.MouseDown && current.type != EventType.MouseUp && current.type != EventType.ScrollWheel && current.type != EventType.MouseDrag) return;
            if (current.type == EventType.MouseUp)
            {
                if (new Rect(panel.xMax - 55, panel.y + 15, 40, 40).Contains(current.mousePosition)) Close();
                GUIUtility.hotControl = 0;
            }
            current.Use();
        }

        private static Dictionary<string, Entry> Build()
        {
            var result = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
            {
                string name = RunModifierRules.ModifierName(effect);
                if (!result.ContainsKey(name)) result[name] = new Entry { Name = name, Effect = effect, Description = Definition(effect) + "\nScope: " + RunModifierRules.ScopeName(RunModifierRules.DefaultScope(effect)) + ". Lasts for this run; resets in a new run." };
                string shortName = ShortOfferCopy.StatName(effect);
                if (!result.ContainsKey(shortName)) result[shortName] = new Entry { Name = shortName, Effect = effect, Description = Definition(effect) + "\nScope: " + RunModifierRules.ScopeName(RunModifierRules.DefaultScope(effect)) + ". Lasts for this run; resets in a new run." };
            }
            Add(result, "pierce", "Bullet pierce", ItemEffect.BulletPierce, "Extra targets a projectile can pass through. Basic projectiles do not hit the same target twice. Adds weapon, tier and item bonuses, capped at 64 extra hits.", "pierce");
            Add(result, "split", "Bullet split", ItemEffect.BulletSplit, "Creates up to 6 child projectiles after the first impact. Children deal 35% damage, are smaller and cannot split again; lifetime is not reset.", "bullet split");
            Add(result, "bounce", "Bullet bounce", ItemEffect.BulletBounce, "Chance to redirect toward a target not yet hit by this projectile. Up to 8 chance-based bounces. Fixed weapon bounces are separate; lifetime is not extended.");
            Add(result, "critical", "Critical hit", ItemEffect.Crit, "Chance for an attack to critically hit; bonuses add before the 0–100% cap. This is not dodge.", "crit");
            Add(result, "dodge", "Dodge", ItemEffect.Dodge, "Rolled separately for every incoming hit, capped at 60%. Success prevents that hit's damage; there is no shared damage-immunity window after each hit.", "dodge");
            Add(result, "armor", "Armor", ItemEffect.Armor, "Positive armor reduces incoming damage by 1/(1+armor). Negative armor increases damage, up to ×2 before the per-hit cap.", "armor");
            Add(result, "damage", "Damage", ItemEffect.Damage, "Flat melee/ranged damage adds to base damage before percentage multipliers. Critical hits, reactions and secondary effects use separate rules; contributions record actual HP lost.");
            Add(result, "bond", "Bond synergy", ItemEffect.CompanionDamage, "Counts roster bodies with that tag, not stars. Each bond has its own repeating threshold and may grant extra rewards at specific counts. Its card shows the actual next threshold and reward, before final combat limits. Story travellers do not count as recruited bodies.");
            Add(result, "merge", "Tier merge", ItemEffect.CompanionDamage, "Three units of the same species/tier merge into one higher-tier unit. Tier 1/2/3 support ×1/2/4 does not preserve three old sources. Recruit weapons are random; identical species + weapon variants cannot coexist.", "tier");
            Add(result, "altar", "Dream shop", ItemEffect.Luck, "A special shop that uses eggs or HP. An HP price lowers maximum HP for the rest of the run (current HP with it); it must leave at least 1 HP and a quarter of your maximum. The new item's bonuses cannot pay its purchase cost.");
            Add(result, "reroll", "Reroll stock", ItemEffect.Discount, "Pay gold to replace displayed wares. No selling or locking stock; item discounts do not discount rerolls.");
            Add(result, "stack", "Stackable", ItemEffect.Damage, "Multiple copies add bonuses according to effective stacks. Unique items cannot stack twice. Technical caps do not remove owned stacks.", "stackable");
            Add(result, "scope", "Effect scope", ItemEffect.CompanionDamage, "Champion, companions, team or run. Bonuses apply only to their declared scope; champion bonuses do not automatically transfer to companions.", "team", "run");
            Add(result, "correction", "Stat correction", ItemEffect.DamageCorrection, "An extra multiplier applied after the stat's basic bonuses, with its own floor/cap. This is not a permanent profile upgrade.");
            Add(result, "weapon", "Weapons", ItemEffect.MeleeDamage, "Defines range, base damage, cycle, attack type, element and projectile behavior. Attacks are automatic; attached hands and weapons are separate from the body, without arms.");
            Add(result, "champion", "Champion", ItemEffect.Health, "The character you control by moving; attacks automatically. Death ends the run. Immortal companions do not block bullets by default.");
            Add(result, "companions", "Companions", ItemEffect.CompanionDamage, "Units automatically patrol, rest and attack around the champion. Each has its own benefits/tradeoffs, without upkeep or a roster cap. Melee units can enter a faster pursuit mode.");
            Add(result, "passive", "Automatic passive", ItemEffect.Heal, "Triggers automatically on timers/conditions, without skill buttons. Unit healing, scavenging and chill are measured separately, not added to fake damage totals.", "auto-attack");
            Add(result, "boomerang", "Returning weapon", ItemEffect.BulletDistance, "Flies out and returns to its owner. Lifetime is not reset. Split/pierce follow the weapon's projectile rules; not every weapon is a boomerang.");
            Add(result, "health", "Current / maximum health", ItemEffect.Health, "Current HP keeps you alive; maximum HP is the build's health ceiling. HP purchases lower maximum HP for the rest of the run and must leave 1 HP. Maximum changes from buying/merging preserve missing HP. Clearing a wave restores full HP.", "HP");
            Add(result, "gold", "Gold", ItemEffect.Income, "Collected in combat, earned as wave-end income/interest and spent in the regular shop. Combat gold commits to the save at wave end; animations do not create currency.");
            Add(result, "eggs", "Eggs", ItemEffect.Luck, "Separate currency for the dream shop/altar. Some wares cost HP instead of eggs; the two costs are never combined.");
            Add(result, "immortal", "Immortal companions", ItemEffect.Interceptor, "Companions cannot lose HP or die. They do not block hostile bullets by default; interception items enable that mechanic. The champion is not immortal.");
            Add(result, "endless", "Endless journey", ItemEffect.EnemyCount, "Continue after victory, retaining this run's roster/items/stats. Enemies grow stronger with each wave; stats do not carry into a new run's profile.");
            Add(result, "LANTERN STRIDE", "Lantern Stride", ItemEffect.CollisionDamage, K11HeroRules.Describe(HeroMechanic.LanternStride), "Lantern stride");
            Add(result, "PARCEL EXPRESS", "Parcel Express", ItemEffect.Projectiles, K11HeroRules.Describe(HeroMechanic.ParcelExpress), "Parcel express");
            Add(result, "DREAM BREATH", "Dream Breath", ItemEffect.Dodge, K11HeroRules.Describe(HeroMechanic.DreamBreath), "Dream breath");
            Add(result, "RECRUIT THEMES", "Recruit themes", ItemEffect.CompanionDamage, "Only companions matching the champion's selected bonds roll in the regular shop. Weapons remain random. Existing saved units and fusion results are not removed for being off-theme.");
            return result;
        }
        private static void Add(Dictionary<string, Entry> result, string key, string name, ItemEffect effect, string description, params string[] aliases)
        {
            var entry = new Entry { Name = name, Effect = effect, Description = description }; result[key] = entry;
            foreach (string alias in aliases) result[alias] = entry;
        }

        private static string Definition(ItemEffect effect) => effect switch
        {
            ItemEffect.Damage or ItemEffect.ChampionDamage or ItemEffect.CompanionDamage => "Percentage damage, separate from flat melee/ranged bonuses. Scope determines champion, companions or team; buffs do not transfer across scopes.",
            ItemEffect.AttackSpeed or ItemEffect.CompanionAttackSpeed => "Divides attack cycle by 1 + bonus. Higher attack speed shortens time between attacks, not movement or projectile speed.",
            ItemEffect.Health => "Raises maximum HP: (base + HP bonus) × health correction, minimum 1. Maximum changes from buying/merging preserve missing HP.",
            ItemEffect.MoveSpeed => "Multiplies base movement speed. Does not increase attack speed; negative bonuses slow movement with a minimum multiplier.",
            ItemEffect.Pickup => "Distance that triggers automatic loot attraction. Measured in map units; does not enlarge projectiles or the player's hitbox.",
            ItemEffect.Roam => "Companion patrol radius around the champion, separate from attack range. Melee companions add 2 units and can pursue nearby enemies. Companions still rest independently.",
            ItemEffect.Interceptor => "Enables hostile-bullet interception by immortal companions. Disabled by default; does not protect against every collision or hazard.",
            ItemEffect.Heal => "Continuous stat-based HP regeneration per second. Unit passives, hearts and healing procs are separate sources.",
            ItemEffect.Crit => "Critical-hit chance, added before the 0–100% cap. Separate from dodge or flat melee damage.",
            ItemEffect.Projectiles => "Projectiles per volley before splitting. Melee attacks do not become projectiles; technical cap is 96 projectiles per volley.",
            ItemEffect.Interest => "Raises the interest cap at rest. Actual interest also depends on stored gold and the run's divisor; separate from base income.",
            ItemEffect.Armor => "Positive armor divides incoming damage by 1+armor. Negative armor increases damage, capped at ×2 before the per-hit limit.",
            ItemEffect.Discount => "Discounts gold wares, up to 50% combined discount. Does not discount rerolls or HP/egg purchases.",
            ItemEffect.Luck => "Increases loot chance and contributes 25% of its bonus to shop-rarity weights. Does not change critical-hit or combat damage RNG.",
            ItemEffect.TreasureDetection => "Chests appear sooner, at least 4 seconds apart. Adds 25% of the bonus to chest gold quality.",
            ItemEffect.ScoreMultiplier => "Multiplies build score after base score/difficulty. Commits on purchase/wave end; animations do not invent score.",
            ItemEffect.CollisionDamage => "Outgoing damage when the champion/companions touch enemies, with its own cooldown. Separate from damage enemies deal to you.",
            ItemEffect.MeleeDamage => "Flat bonus added to melee base damage before percentage multipliers. Does not increase projectile damage; tradeoffs can reduce it.",
            ItemEffect.RangedDamage => "Flat bonus added to projectile base damage before percentage multipliers. Separate from projectile count, splitting and team percentage damage.",
            ItemEffect.AttackRange => "Increases weapon targeting/attack range and projectile travel distance. Does not increase patrol radius; projectile speed is separate.",
            ItemEffect.Dodge => "Rolled separately for each incoming hit, capped at 60%. Success avoids that hit, without a shared invulnerability window.",
            ItemEffect.Income => "Adds base gold after waves and on entering endless mode. Separate from interest and pickups; negative values reduce income.",
            ItemEffect.BulletPierce => "Extra targets projectiles can pierce. Victim tracking prevents repeated hits; adds weapon/tier/stat bonuses, capped at 64.",
            ItemEffect.BulletSplit => "Creates up to 6 child projectiles at first impact, each smaller and dealing 35% damage. No recursive splitting or lifetime reset.",
            ItemEffect.BulletBounce => "Chance to redirect to an unhit target, 0–100%, up to 8 bounces. Fixed weapon bounces are separate.",
            ItemEffect.BulletSize => "Multiplies both rendered width and projectile collision radius, ×0.25–4. Not merely visual enlargement with fake damage.",
            ItemEffect.BulletSpeed => "Increases flight speed. Lifetime decreases proportionally so speed does not grant extra range; range/distance have separate stats.",
            ItemEffect.BulletDistance => "Increases remaining flight time/distance. No extension on splitting/bouncing; boomerangs retain their own return rules.",
            ItemEffect.Knockback => "Increases the weapon's automatic on-hit push distance. No manual shove button; bosses/special behaviors have separate rules.",
            ItemEffect.BodySize => "Scales champion sprite and hitbox together. Bigger champions are easier to hit; this is not an HP bonus.",
            ItemEffect.CompanionSize => "Scales companion sprite and radius. Does not change unit/bond counts or remove immortality.",
            ItemEffect.EnemyCount => "Changes enemy budget and spawn pacing. Can trade pressure for loot/score opportunities; does not increase your recruitable species.",
            ItemEffect.EnemyHealth => "Multiplies enemy HP alongside wave/difficulty curves. Late-game enemies gain extra scaling, not merely larger artwork.",
            ItemEffect.EnemySpeed => "Multiplies enemy movement speed, not bullet speed. Hostile bullets retain readable speed caps.",
            ItemEffect.HeartHealing => "Adds HP restored by collected hearts. Does not add to continuous regeneration or every healing passive.",
            ItemEffect.ChestQuality => "Multiplies generated chest gold, ×0.2–5. Does not create new item rarities or turn one chest into every loot type.",
            ItemEffect.RecruitDiscount => "Discounts gold-priced recruits, combined with general discount up to 50%. Does not discount every ware.",
            ItemEffect.RelicDiscount => "Discounts gold-priced items, combined with general discount up to 50%. Does not affect rerolls or blood shops.",
            ItemEffect.ShopRarity => "Increases higher-rarity weights for newly generated stock. Does not upgrade owned/displayed items or guarantee rare rolls.",
            ItemEffect.HealthCorrection => "Multiplies maximum HP after base HP bonuses; separate from current HP and blood-shop costs.",
            ItemEffect.DamageCorrection => "Multiplies damage after base + flat melee/ranged bonuses and regular damage multipliers, respecting source scope.",
            ItemEffect.SpeedCorrection => "An extra movement multiplier after MoveSpeed; does not change attack cooldown or projectile speed.",
            ItemEffect.ArmorCorrection => "Multiplies armor bonuses before incoming-damage formulas. Not a directly added damage-immunity percentage.",
            ItemEffect.HealCorrection => "Multiplies continuous regeneration when compiling combat. Does not automatically multiply hearts, all procs or recorded healing.",
            ItemEffect.CollisionFrequency => "Divides outgoing collision-damage cooldown by 1+bonus, bounded to 0.15–4 seconds. Does not shorten enemy contact cooldown.",
            ItemEffect.ChestReach => "Scales the distance needed to open a chest, from 0.5 to 4 times its base radius. Separate from Treasure Detection (frequency), chest quality and loose-loot attraction. Chests stay on the ground rather than flying into you.",
            ItemEffect.ChainChance => "Chance that a friend's hit chains to one more enemy nearby (Static Club).",
            ItemEffect.RecordedDamage => "Friends deal more damage to species already recorded this run (Exhibit Hall).",
            _ => "Inspect the modifier's scope and source in the ledger; explanatory text grants no hidden bonuses."
        };

        public static float Height(string text, GUIStyle style, float width) => Layout(new Rect(0, 0, width, 10000), text, style, false);
        public static void Text(Rect bounds, string text, GUIStyle style) { Layout(bounds, text, style, true); }
        private static float Layout(Rect bounds, string text, GUIStyle style, bool draw)
        {
            if (!wordStyles.TryGetValue(style, out GUIStyle words))
            {
                if (wordStyles.Count > 32) wordStyles.Clear();
                words = new GUIStyle(style) { wordWrap = false, clipping = TextClipping.Overflow, padding = new RectOffset(), margin = new RectOffset() }; wordStyles[style] = words;
            }
            if (draw && words.font != null) words.font.RequestCharactersInTexture(text ?? "", words.fontSize, words.fontStyle);
            float line = Mathf.Max(style.lineHeight, style.fontSize * 1.25f);
            float horizontal = 0, vertical = 0;
            MatchCollection all = tokens.Matches(text ?? "");
            for (int tokenIndex = 0; tokenIndex < all.Count; tokenIndex++)
            {
                Match token = all[tokenIndex];
                string word = token.Value;
                if (word.Contains("\n")) { horizontal = 0; vertical += line; continue; }
                if (word == "{mark}")
                {
                    // a small gold lozenge that opens each effect line of a shop card
                    float mark = line * .38f;
                    if (draw)
                    {
                        Rect m = new Rect(bounds.x + horizontal + 2, bounds.y + vertical + (line - mark) * .5f, mark, mark);
                        Matrix4x4 keep = GUI.matrix; GUIUtility.RotateAroundPivot(45, m.center);
                        ArcadeSkin.Line(m, ArcadeSkin.Gold); ArcadeSkin.Line(new Rect(m.x + mark * .3f, m.y + mark * .3f, mark * .4f, mark * .4f), ArcadeSkin.Cream);
                        GUI.matrix = keep;
                    }
                    horizontal += mark + 9; continue;
                }
                Entry entry = Find(word);
                string[] bonus = word.StartsWith("{bonus:", StringComparison.Ordinal) ? word.Substring(7, word.Length - 8).Split('|') : null;
                bool formatted = bonus != null && bonus.Length == 3 && Enum.TryParse(bonus[0], out ItemEffect _);
                if (formatted)
                {
                    ItemEffect effect = (ItemEffect)Enum.Parse(typeof(ItemEffect), bonus[0]);
                    float glyph = line * .85f;
                    string caption = bonus[1] + " " + bonus[2];
                    float normalValue = words.CalcSize(new GUIContent(bonus[2])).x;
                    FontStyle measuredStyle = words.fontStyle; words.fontStyle = FontStyle.Bold;
                    float boldValue = words.CalcSize(new GUIContent(bonus[2])).x; words.fontStyle = measuredStyle;
                    float size = words.CalcSize(new GUIContent(caption)).x + glyph + 5 + boldValue - normalValue;
                    if (horizontal > 0 && horizontal + size > bounds.width) { horizontal = 0; vertical += line; }
                    if (draw)
                    {
                        Rect target = new Rect(bounds.x + horizontal, bounds.y + vertical, size, line);
                        StatGlyph.Draw(new Rect(target.x, target.y, glyph, line), effect);
                        GUI.Label(new Rect(target.x + glyph + 3, target.y, size - glyph, line), bonus[1], words);
                        float offset = glyph + 3 + words.CalcSize(new GUIContent(bonus[1] + " ")).x;
                        Color prior = GUI.color; GUI.color *= ShopDescriptionCard.BenefitColor(effect, bonus[2].StartsWith("-") ? -1 : 1);
                        FontStyle previousStyle = words.fontStyle; words.fontStyle = FontStyle.Bold;
                        GUI.Label(new Rect(target.x + offset, target.y, size - offset + 8, line), bonus[2], words); words.fontStyle = previousStyle; GUI.color = prior;
                        if (GUI.Button(target, GUIContent.none, GUIStyle.none)) Show(RunModifierRules.ModifierName(effect));
                    }
                    horizontal += size; continue;
                }
                float icon = entry == null || entry.Name == "Effect scope" || entry.Name == "Champion" || entry.Name == "Companions" ? 0 : line * 0.85f;
                bool number = signedValue.IsMatch(word) || char.IsDigit(word[0]);
                FontStyle measuredWordStyle = words.fontStyle; if (number) words.fontStyle = FontStyle.Bold;
                float width = words.CalcSize(new GUIContent(word)).x + icon + (entry == null ? 0 : 2); words.fontStyle = measuredWordStyle;
                if (horizontal > 0 && horizontal + width > bounds.width) { horizontal = 0; vertical += line; }
                if (draw)
                {
                    Rect target = new Rect(bounds.x + horizontal, bounds.y + vertical, width, line);
                    if (entry != null)
                    {
                        StatGlyph.Draw(new Rect(target.x, target.y, icon, line), entry.Effect);
                        Color previous = GUI.color; GUI.color *= ArcadeSkin.Teal;
                        Rect caption = new Rect(target.x + icon, target.y, width - icon, line);
                        GUI.Label(caption, word, words);
                        if (GUI.Button(caption, GUIContent.none, GUIStyle.none)) Show(word);
                        GUI.color = previous;
                        if (target.Contains(Event.current.mousePosition)) ArcadeSkin.Line(new Rect(target.x + icon, target.yMax - 2, width - icon, 2), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.8f));
                    }
                    else
                    {
                        Color previous = GUI.color;
                        if (signedValue.IsMatch(word)) GUI.color *= ShopDescriptionCard.BenefitColor(NearbyEffect(all, tokenIndex), word[0] == '+' ? 1 : -1);
                        else if (char.IsDigit(word[0])) GUI.color *= ArcadeSkin.Gold;
                        FontStyle previousStyle = words.fontStyle;
                        if (signedValue.IsMatch(word) || char.IsDigit(word[0])) words.fontStyle = FontStyle.Bold;
                        GUI.Label(target, word, words); words.fontStyle = previousStyle; GUI.color = previous;
                    }
                }
                horizontal += width;
            }
            return vertical + line;
        }
        public static void Draw(Vector2 viewport)
        {
            if (selected == null) return;
            if (Time.unscaledTime - openedAt > 30) { Close(); return; }
            float width = Mathf.Min(620, viewport.x - 60);
            Rect panel = new Rect(viewport.x - width - 35, 150, width, 285);
            ArcadeSkin.Ticket(panel, ArcadeSkin.Ink, ArcadeSkin.Teal);
            StatGlyph.Draw(new Rect(panel.x + 20, panel.y + 18, 36, 36), selected.Effect);
            GUIStyle heading = ArcadeTypography.Style(24, true); heading.normal.textColor = ArcadeSkin.Cream;
            GUIStyle body = ArcadeTypography.Style(20); body.normal.textColor = ArcadeSkin.Cream;
            GUI.Label(new Rect(panel.x + 68, panel.y + 20, width - 125, 40), selected.Name, heading);
            if (GUI.Button(new Rect(panel.xMax - 55, panel.y + 15, 40, 40), "×")) Close();
            if (selected != null) GUI.Label(new Rect(panel.x + 25, panel.y + 75, width - 50, 180), selected.Description, body);
            if (Time.unscaledTime - openedAt > 30) Close();
        }
    }
}
