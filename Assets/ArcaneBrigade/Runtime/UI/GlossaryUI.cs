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
        private static readonly Regex tokens = new Regex("((?<!\\p{L})(?:" + string.Join("|", entries.Keys.OrderByDescending(key => key.Length).Select(Regex.Escape)) + ")(?!\\p{L})|\\r?\\n|[^\\S\\r\\n]+|[^\\s]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static Entry selected;
        private static float openedAt;
        private static readonly Dictionary<GUIStyle, GUIStyle> wordStyles = new Dictionary<GUIStyle, GUIStyle>();
        public static bool Open => selected != null;
        public static void Close() { selected = null; }
        public static Entry Find(string term) => entries.TryGetValue(term.Trim().TrimEnd('.', ',', ':', ';'), out Entry entry) ? entry : null;
        public static bool Show(string term) { selected = Find(term); openedAt = Time.unscaledTime; return selected != null; }
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
            }
            Add(result, "pierce", "Bullet pierce", ItemEffect.BulletPierce, "Extra targets a projectile can pass through. Basic projectiles do not hit the same target twice. Adds weapon, tier and item bonuses, capped at 64 extra hits.", "pierce");
            Add(result, "split", "Bullet split", ItemEffect.BulletSplit, "Creates up to 6 child projectiles after the first impact. Children deal 35% damage, are smaller and cannot split again; lifetime is not reset.", "bullet split");
            Add(result, "bounce", "Bullet bounce", ItemEffect.BulletBounce, "Chance to redirect toward a target not yet hit by this projectile. Up to 8 chance-based bounces. Fixed weapon bounces are separate; lifetime is not extended.");
            Add(result, "critical", "Critical hit", ItemEffect.Crit, "Chance for an attack to critically hit; bonuses add before the 0–100% cap. This is not dodge.", "crit");
            Add(result, "dodge", "Dodge", ItemEffect.Dodge, "Rolled separately for every incoming hit, capped at 60%. Success prevents that hit's damage; there is no shared damage-immunity window after each hit.", "dodge");
            Add(result, "armor", "Armor", ItemEffect.Armor, "Positive armor reduces incoming damage by 1/(1+armor). Negative armor increases damage, up to ×2 before the per-hit cap.", "armor");
            Add(result, "damage", "Damage", ItemEffect.Damage, "Flat melee/ranged damage adds to base damage before percentage multipliers. Critical hits, reactions and secondary effects use separate rules; contributions record actual HP lost.");
            Add(result, "bond", "Bond synergy", ItemEffect.CompanionDamage, "Counts roster units with that tag, not stars. Thresholds 2/4/6 grant bonuses. Each bond has 4–6 recruitable species, not an ownership cap.");
            Add(result, "merge", "Tier merge", ItemEffect.CompanionDamage, "Three units of the same species/tier merge into one higher-tier unit. Tier 1/2/3 support ×1/2/4 does not preserve three old sources. Recruit weapons are random; identical species + weapon variants cannot coexist.", "tier");
            Add(result, "altar", "Dream shop", ItemEffect.Luck, "A special shop that uses eggs or current HP. HP purchases must leave at least 1 HP; the new item's bonuses cannot pay its purchase cost.");
            Add(result, "reroll", "Reroll stock", ItemEffect.Discount, "Pay gold to replace displayed wares. No selling or locking stock; item discounts do not discount rerolls.");
            Add(result, "stack", "Stackable", ItemEffect.Damage, "Multiple copies add bonuses according to effective stacks. Unique items cannot stack twice. Technical caps do not remove owned stacks.", "stackable");
            Add(result, "scope", "Effect scope", ItemEffect.CompanionDamage, "Champion, companions, team or run. Bonuses apply only to their declared scope; champion bonuses do not automatically transfer to companions.", "team", "run");
            Add(result, "correction", "Stat correction", ItemEffect.DamageCorrection, "An extra multiplier applied after the stat's basic bonuses, with its own floor/cap. This is not a permanent profile upgrade.");
            Add(result, "weapon", "Weapons", ItemEffect.MeleeDamage, "Defines range, base damage, cycle, attack type, element and projectile behavior. Attacks are automatic; attached hands and weapons are separate from the body, without arms.");
            Add(result, "champion", "Champion", ItemEffect.Health, "The character you control by moving; attacks automatically. Death ends the run. Immortal companions do not block bullets by default.");
            Add(result, "companions", "Companions", ItemEffect.CompanionDamage, "Units automatically patrol, rest and attack around the champion. Each has its own benefits/tradeoffs, without upkeep or a roster cap. Melee units can enter a faster pursuit mode.");
            Add(result, "passive", "Automatic passive", ItemEffect.Heal, "Triggers automatically on timers/conditions, without skill buttons. Unit healing, scavenging and chill are measured separately, not added to fake damage totals.", "auto-attack");
            Add(result, "boomerang", "Returning weapon", ItemEffect.BulletDistance, "Flies out and returns to its owner. Lifetime is not reset. Split/pierce follow the weapon's projectile rules; not every weapon is a boomerang.");
            Add(result, "health", "Current / maximum health", ItemEffect.Health, "Current HP keeps you alive; maximum HP is the build's health ceiling. HP purchases spend current HP and must leave 1 HP. Maximum changes from buying/merging preserve missing HP. Clearing a wave restores full HP.", "HP");
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
            foreach (Match token in tokens.Matches(text ?? ""))
            {
                string word = token.Value;
                if (word.Contains("\n")) { horizontal = 0; vertical += line; continue; }
                Entry entry = Find(word);
                float icon = entry == null ? 0 : line * 0.85f;
                float width = words.CalcSize(new GUIContent(word)).x + icon + (entry == null ? 0 : 2);
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
                        ArcadeSkin.Line(new Rect(target.x + icon, target.yMax - 2, width - icon, 1), new Color(0.3f, 0.85f, 0.75f, 0.35f));
                    }
                    else GUI.Label(target, word, words);
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
