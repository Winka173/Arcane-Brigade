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
        private static readonly Regex tokens = new Regex("(\\{mark\\}|\\{gives\\}|\\{costs\\}|\\{tag:[^}]+\\}|\\{bonus:[^}]+\\}|[+−-]\\d+(?:\\.\\d+)?%?|(?<!\\p{L})\\d+(?:\\.\\d+)?(?:%|s)?|(?<!\\p{L})(?:" + string.Join("|", entries.Keys.OrderByDescending(key => key.Length).Select(Regex.Escape)) + ")(?!\\p{L})[.,:;!?)]*|\\r?\\n|[^\\S\\r\\n]+|[^\\s]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex signedValue = new Regex("^[+−-]\\d+(?:\\.\\d+)?%?$", RegexOptions.CultureInvariant);
        public static string Modifier(ItemEffect effect, ModifierScope scope, float number) => "{bonus:" + effect + "|" + RunModifierRules.WhoFor(effect, scope) + RunModifierRules.ModifierName(effect) + "|" + RunModifierRules.ValueText(effect, number) + "}";
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
                if (!result.ContainsKey(name)) result[name] = new Entry { Name = name, Effect = effect, Description = Definition(effect) + " Lasts until the run ends." };
                string shortName = ShortOfferCopy.StatName(effect);
                if (!result.ContainsKey(shortName)) result[shortName] = new Entry { Name = shortName, Effect = effect, Description = Definition(effect) + " Lasts until the run ends." };
            }
            Add(result, "pierce", "Pierce", ItemEffect.BulletPierce, "Shots go through this many more enemies.", "pierce");
            Add(result, "split", "Split shots", ItemEffect.BulletSplit, "Shots burst into small shots when they hit.");
            Add(result, "bounce", "Bounce chance", ItemEffect.BulletBounce, "Chance for a shot to bounce on to another enemy.");
            Add(result, "critical", "Crit", ItemEffect.Crit, "Chance for a hit to land twice as hard.", "crit");
            Add(result, "dodge", "Dodge", ItemEffect.Dodge, "Chance to take no damage from a hit (max 60%).", "dodge");
            Add(result, "armor", "Armor", ItemEffect.Armor, "Cuts every hit you take: 15 armor halves it, 30 takes two thirds. At least a quarter of a hit always lands, so stacking armor never makes you untouchable.", "armor");
            Add(result, "damage", "Damage", ItemEffect.Damage, "How hard every hit lands.");
            Add(result, "bond", "Bond", ItemEffect.CompanionDamage, "Friends of the same family together unlock a bonus; the card shows how many you need.");
            Add(result, "merge", "Merge", ItemEffect.CompanionDamage, "Three of the same friend at the same stars become one friend with one more star (two of three show half a star more).");
            Add(result, "altar", "Dream shop", ItemEffect.Luck, "A shop that takes eggs or HP instead of gold. HP prices lower your max HP for the rest of the run.");
            Add(result, "reroll", "Reroll", ItemEffect.Discount, "Pay gold for new offers in the shop.");
            Add(result, "stack", "Stackable", ItemEffect.Damage, "More copies add more of the same bonus.", "stackable");
            Add(result, "scope", "Who it helps", ItemEffect.CompanionDamage, "You means your hero, Friends means your brigade, no label means everyone.");
            Add(result, "correction", "Final bonus", ItemEffect.DamageCorrection, "Multiplies a stat after every other bonus to it.");
            Add(result, "weapon", "Weapons", ItemEffect.MeleeDamage, "Every friend fights with its own weapon, by itself.");
            Add(result, "champion", "Hero", ItemEffect.Health, "The one you move. If your hero falls, the run ends.");
            Add(result, "companions", "Friends", ItemEffect.CompanionDamage, "They follow you and fight by themselves. They never fall.");
            Add(result, "passive", "Passive", ItemEffect.Heal, "Happens by itself; nothing to press.");
            Add(result, "boomerang", "Returning weapon", ItemEffect.BulletDistance, "Flies out and comes back to its owner.");
            Add(result, "health", "HP", ItemEffect.Health, "Your hero's health. At 0 the run ends.");
            Add(result, "gold", "Gold", ItemEffect.Income, "Picked up in fights and earned after each wave; spend it in the shop.");
            Add(result, "eggs", "Eggs", ItemEffect.Luck, "Spend them in the dream shop.");
            Add(result, "immortal", "Friends never fall", ItemEffect.Interceptor, "Friends cannot be hurt; only your hero can.");
            Add(result, "endless", "Endless", ItemEffect.EnemyCount, "Keep going after dawn with the same brigade; every wave is harder.");
            Add(result, "LANTERN STRIDE", "Lantern Stride", ItemEffect.CollisionDamage, K11HeroRules.Describe(HeroMechanic.LanternStride), "Lantern stride");
            Add(result, "PARCEL EXPRESS", "Parcel Express", ItemEffect.Projectiles, K11HeroRules.Describe(HeroMechanic.ParcelExpress), "Parcel express");
            Add(result, "DREAM BREATH", "Dream Breath", ItemEffect.Dodge, K11HeroRules.Describe(HeroMechanic.DreamBreath), "Dream breath");
            Add(result, "RECRUIT THEMES", "Families", ItemEffect.CompanionDamage, "This hero's shop offers friends from these families only.");
            return result;
        }
        private static void Add(Dictionary<string, Entry> result, string key, string name, ItemEffect effect, string description, params string[] aliases)
        {
            var entry = new Entry { Name = name, Effect = effect, Description = description }; result[key] = entry;
            foreach (string alias in aliases) result[alias] = entry;
        }

        /// <summary>One sentence per stat, in player words (shown when a stat line is tapped).</summary>
        private static string Definition(ItemEffect effect) => effect switch
        {
            ItemEffect.Damage or ItemEffect.ChampionDamage or ItemEffect.CompanionDamage => "How hard every hit lands, in percent.",
            ItemEffect.AttackSpeed or ItemEffect.CompanionAttackSpeed => "How often attacks come; higher means less waiting between hits.",
            ItemEffect.Health => "Your most HP; buying with HP lowers it.",
            ItemEffect.MoveSpeed => "How fast you walk.",
            ItemEffect.Pickup => "How far away coins, eggs and hearts start flying to you.",
            ItemEffect.Roam => "How far friends wander from you to fight.",
            ItemEffect.Interceptor => "Friends catch enemy shots that would hit you.",
            ItemEffect.Heal => "HP you get back every second.",
            ItemEffect.Crit => "Chance for a hit to land twice as hard.",
            ItemEffect.Projectiles => "More shots in every volley.",
            ItemEffect.Interest => "Gold you keep earns extra gold after each wave; this raises the most it can earn.",
            ItemEffect.Armor => "Takes a bite out of every hit you take.",
            ItemEffect.Discount => "Gold prices in the shop go down (up to half).",
            ItemEffect.Luck => "More loot and rarer shop offers.",
            ItemEffect.TreasureDetection => "Chests appear more often.",
            ItemEffect.ScoreMultiplier => "Your score at the end is multiplied.",
            ItemEffect.CollisionDamage => "Damage dealt just by touching enemies.",
            ItemEffect.MeleeDamage => "Extra damage on every close hit.",
            ItemEffect.RangedDamage => "Extra damage on every shot.",
            ItemEffect.AttackRange => "How far attacks reach.",
            ItemEffect.Dodge => "Chance to take no damage from a hit (max 60%).",
            ItemEffect.Income => "Gold you get after each wave.",
            ItemEffect.BulletPierce => "Shots go through this many more enemies.",
            ItemEffect.BulletSplit => "Shots burst into small shots when they hit.",
            ItemEffect.BulletBounce => "Chance for a shot to bounce on to another enemy.",
            ItemEffect.BulletSize => "Bigger shots that are easier to land.",
            ItemEffect.BulletSpeed => "Shots fly faster.",
            ItemEffect.BulletDistance => "Shots fly farther.",
            ItemEffect.Knockback => "Hits push enemies farther back.",
            ItemEffect.BodySize => "You get bigger, which also makes you easier to hit.",
            ItemEffect.CompanionSize => "Friends get bigger.",
            ItemEffect.EnemyCount => "How many enemies come each wave.",
            ItemEffect.EnemyHealth => "How much HP enemies have.",
            ItemEffect.EnemySpeed => "How fast enemies move.",
            ItemEffect.HeartHealing => "Extra HP from every heart you pick up.",
            ItemEffect.ChestQuality => "More gold in every chest.",
            ItemEffect.RecruitDiscount => "Friends cost less gold.",
            ItemEffect.RelicDiscount => "Relics cost less gold.",
            ItemEffect.ShopRarity => "Rarer friends and relics in the shop.",
            ItemEffect.HealthCorrection => "Multiplies your max HP after every other HP bonus.",
            ItemEffect.DamageCorrection => "Multiplies damage after every other damage bonus.",
            ItemEffect.SpeedCorrection => "Multiplies move speed after every other speed bonus.",
            ItemEffect.ArmorCorrection => "Multiplies armor after every other armor bonus.",
            ItemEffect.HealCorrection => "Multiplies your HP regen.",
            ItemEffect.CollisionFrequency => "Touch damage hits more often.",
            ItemEffect.ChestReach => "How close you must walk to open a chest.",
            ItemEffect.ChainChance => "Chance for a friend's hit to jump to one more enemy.",
            ItemEffect.RecordedDamage => "Friends hit harder against kinds of enemy you have already met this run.",
            _ => "A bonus for this run."
        };

        // requirement 25/10/2026 (pages with long text lagged): the text's words, their widths and its height are worked out once
        // and kept, and inside a scroll only the lines on screen are drawn
        private static readonly Dictionary<(string, GUIStyle, float, int), float> heights = new Dictionary<(string, GUIStyle, float, int), float>();
        private static readonly Dictionary<string, MatchCollection> tokenCache = new Dictionary<string, MatchCollection>();
        private static readonly Dictionary<(string, Font, int, FontStyle), float> widths = new Dictionary<(string, Font, int, FontStyle), float>();
        public static float Height(string text, GUIStyle style, float width)
        {
            var key = (text ?? "", style, width, style.fontSize);
            if (heights.TryGetValue(key, out float known)) return known;
            if (heights.Count > 512) heights.Clear();
            return heights[key] = Layout(new Rect(0, 0, width, 10000), text, style, false);
        }
        private static MatchCollection Tokens(string text)
        {
            text ??= "";
            if (tokenCache.TryGetValue(text, out MatchCollection found)) return found;
            if (tokenCache.Count > 256) tokenCache.Clear();
            found = tokens.Matches(text); _ = found.Count; // evaluated now, kept for later frames
            return tokenCache[text] = found;
        }
        private static float Measure(GUIStyle style, string word)
        {
            var key = (word, style.font, style.fontSize, style.fontStyle);
            if (widths.TryGetValue(key, out float known)) return known;
            if (widths.Count > 4096) widths.Clear();
            return widths[key] = style.CalcSize(new GUIContent(word)).x;
        }
        public static void Text(Rect bounds, string text, GUIStyle style) { Layout(bounds, text, style, true); }
        private static float Layout(Rect bounds, string text, GUIStyle style, bool draw)
        {
            if (!wordStyles.TryGetValue(style, out GUIStyle words))
            {
                if (wordStyles.Count > 32) wordStyles.Clear();
                words = new GUIStyle(style) { wordWrap = false, clipping = TextClipping.Overflow, padding = new RectOffset(), margin = new RectOffset() }; wordStyles[style] = words;
            }
            // the cached copy follows the style's current colour, size and font (a tooltip on cream paper set ink after the copy was made)
            words.normal.textColor = style.normal.textColor; words.fontSize = style.fontSize; words.font = style.font; words.fontStyle = style.fontStyle;
            if (draw && words.font != null && Event.current.type == EventType.Repaint) words.font.RequestCharactersInTexture(text ?? "", words.fontSize, words.fontStyle);
            float line = Mathf.Max(style.lineHeight, style.fontSize * 1.25f);
            float horizontal = 0, vertical = 0, indent = 0; // indent: where a wrapped line starts (under the words after a lozenge)
            MatchCollection all = Tokens(text);
            Rect seen = draw && ScrollPanel.Inside ? ScrollPanel.Visible : new Rect(-1e6f, -1e6f, 2e6f, 2e6f);
            bool Seen(float v) => bounds.y + v + line >= seen.yMin && bounds.y + v <= seen.yMax;
            for (int tokenIndex = 0; tokenIndex < all.Count; tokenIndex++)
            {
                Match token = all[tokenIndex];
                string word = token.Value;
                if (word.Contains("\n")) { horizontal = 0; indent = 0; vertical += line; continue; }
                if (word == "{mark}")
                {
                    // a small gold lozenge that opens each effect line of a shop card, in a slot as wide as a stat icon so the words
                    // after it start where they do after an icon on the line above; a wrapped line starts under those words
                    float mark = line * .38f, slot = line * .85f + 2;
                    if (draw && Seen(vertical))
                    {
                        Rect m = new Rect(bounds.x + horizontal + (slot - mark) * .5f, bounds.y + vertical + (line - mark) * .5f, mark, mark);
                        Matrix4x4 keep = GUI.matrix; ArcadeSkin.TurnAround(45, m.center);
                        ArcadeSkin.Line(m, ArcadeSkin.Gold); ArcadeSkin.Line(new Rect(m.x + mark * .3f, m.y + mark * .3f, mark * .4f, mark * .4f), ArcadeSkin.Cream);
                        GUI.matrix = keep;
                    }
                    bool opens = horizontal == 0; horizontal += slot; if (opens) indent = horizontal; if (tokenIndex + 1 < all.Count && all[tokenIndex + 1].Value.Trim().Length == 0 && !all[tokenIndex + 1].Value.Contains("\n")) tokenIndex++; // the space after it is the gap already left
                    continue;
                }
                if (word.StartsWith("{tag:", StringComparison.Ordinal))
                {
                    // a neutral plate naming what the item is (DRINK, FOOD), the same shape as GIVES and COSTS
                    string caption = word.Substring(5, word.Length - 6);
                    FontStyle keptTag = words.fontStyle; words.fontStyle = FontStyle.Bold;
                    float plate = Measure(words, caption) * .82f + 16;
                    if (draw && Seen(vertical))
                    {
                        Rect r = new Rect(bounds.x + horizontal, bounds.y + vertical + line * .12f, plate, line * .76f);
                        ArcadeSkin.Line(r, new Color(.45f, .31f, .1f, .95f));
                        int size = words.fontSize; words.fontSize = Mathf.RoundToInt(size * .8f);
                        Color prior = GUI.color; GUI.color *= new Color(1f, .93f, .72f);
                        words.alignment = TextAnchor.MiddleCenter; GUI.Label(r, caption, words); words.alignment = TextAnchor.UpperLeft;
                        GUI.color = prior; words.fontSize = size;
                    }
                    words.fontStyle = keptTag;
                    horizontal += plate + 8; continue;
                }
                if (word == "{gives}" || word == "{costs}")
                {
                    // the card's GIVES (green) and COSTS (red) plates (play-test 20/10/2026, item 11)
                    bool gives = word == "{gives}"; string caption = gives ? "GIVES" : "COSTS";
                    FontStyle keptStyle = words.fontStyle; words.fontStyle = FontStyle.Bold;
                    // GIVES and COSTS plates share one width, so the words after them line up from row to row
                    float plate = Mathf.Max(Measure(words, "GIVES"), Measure(words, "COSTS")) * .82f + 16;
                    if (draw && Seen(vertical))
                    {
                        Rect r = new Rect(bounds.x + horizontal, bounds.y + vertical + line * .12f, plate, line * .76f);
                        ArcadeSkin.Line(r, gives ? new Color(.14f, .40f, .25f, .95f) : new Color(.50f, .11f, .12f, .95f));
                        int size = words.fontSize; words.fontSize = Mathf.RoundToInt(size * .8f);
                        Color prior = GUI.color; GUI.color *= gives ? new Color(.75f, 1f, .78f) : new Color(1f, .8f, .76f);
                        words.alignment = TextAnchor.MiddleCenter; GUI.Label(r, caption, words); words.alignment = TextAnchor.UpperLeft;
                        GUI.color = prior; words.fontSize = size;
                    }
                    words.fontStyle = keptStyle;
                    bool opensLine = horizontal == 0; horizontal += plate + 8; if (opensLine) indent = horizontal; if (tokenIndex + 1 < all.Count && all[tokenIndex + 1].Value.Trim().Length == 0 && !all[tokenIndex + 1].Value.Contains("\n")) tokenIndex++; // the space after it is the gap already left
                    continue; // a wrapped line starts under the words
                }
                Entry entry = Find(word);
                string[] bonus = word.StartsWith("{bonus:", StringComparison.Ordinal) ? word.Substring(7, word.Length - 8).Split('|') : null;
                bool formatted = bonus != null && bonus.Length == 3 && Enum.TryParse(bonus[0], out ItemEffect _);
                if (formatted)
                {
                    ItemEffect effect = (ItemEffect)Enum.Parse(typeof(ItemEffect), bonus[0]);
                    float glyph = line * .85f;
                    string caption = bonus[1] + " " + bonus[2];
                    float normalValue = Measure(words, bonus[2]);
                    FontStyle measuredStyle = words.fontStyle; words.fontStyle = FontStyle.Bold;
                    float boldValue = Measure(words, bonus[2]); words.fontStyle = measuredStyle;
                    float size = Measure(words, caption) + glyph + 5 + boldValue - normalValue;
                    if (horizontal > indent && horizontal + size > bounds.width) { horizontal = indent; vertical += line; }
                    // a stat line is one piece: when it is still too long for the rest of the line, its words shrink a little
                    // (to 80% at most) instead of running into the card's buttons
                    int keepSize = words.fontSize;
                    while (horizontal + size > bounds.width && words.fontSize > keepSize * .8f)
                    {
                        words.fontSize--;
                        normalValue = Measure(words, bonus[2]); words.fontStyle = FontStyle.Bold; boldValue = Measure(words, bonus[2]); words.fontStyle = measuredStyle;
                        size = Measure(words, caption) + glyph + 5 + boldValue - normalValue;
                    }
                    if (draw && Seen(vertical))
                    {
                        Rect target = new Rect(bounds.x + horizontal, bounds.y + vertical, size, line);
                        StatGlyph.Draw(new Rect(target.x, target.y, glyph, line), effect);
                        GUI.Label(new Rect(target.x + glyph + 3, target.y, size - glyph, line), bonus[1], words);
                        float offset = glyph + 3 + Measure(words, bonus[1] + " ");
                        Color prior = GUI.color; GUI.color *= ShopDescriptionCard.BenefitColor(effect, bonus[2].StartsWith("-") || bonus[2].StartsWith("−") || bonus[2].StartsWith("×0") ? -1 : 1);
                        FontStyle previousStyle = words.fontStyle; words.fontStyle = FontStyle.Bold;
                        GUI.Label(new Rect(target.x + offset, target.y, size - offset + 8, line), bonus[2], words); words.fontStyle = previousStyle; GUI.color = prior;
                        if (TouchTargets.Button(target, GUIContent.none, GUIStyle.none)) Show(RunModifierRules.ModifierName(effect));
                    }
                    words.fontSize = keepSize;
                    horizontal += size; continue;
                }
                float icon = entry == null || entry.Name == "Effect scope" || entry.Name == "Champion" || entry.Name == "Companions" ? 0 : line * 0.85f;
                bool number = signedValue.IsMatch(word) || char.IsDigit(word[0]);
                FontStyle measuredWordStyle = words.fontStyle; if (number) words.fontStyle = FontStyle.Bold;
                float width = Measure(words, word) + icon + (entry == null ? 0 : 2); words.fontStyle = measuredWordStyle;
                // a gap never opens a wrapped line (it read " a level"): a space that would wrap is the line break itself
                if (word.Trim().Length == 0 && (horizontal == indent || horizontal + width > bounds.width)) { if (horizontal > indent) { horizontal = indent; vertical += line; } continue; }
                if (horizontal > indent && horizontal + width > bounds.width) { horizontal = indent; vertical += line; }
                if (draw && Seen(vertical))
                {
                    Rect target = new Rect(bounds.x + horizontal, bounds.y + vertical, width, line);
                    if (entry != null)
                    {
                        StatGlyph.Draw(new Rect(target.x, target.y, icon, line), entry.Effect);
                        Color previous = GUI.color; GUI.color *= ArcadeSkin.Teal;
                        Rect caption = new Rect(target.x + icon, target.y, width - icon, line);
                        GUI.Label(caption, word, words);
                        if (TouchTargets.Button(caption, GUIContent.none, GUIStyle.none)) Show(word);
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
            GUIStyle heading = ArcadeTypography.Style(ArcadeTypography.SizeBody, true); heading.normal.textColor = ArcadeSkin.Cream;
            GUIStyle body = ArcadeTypography.Style(ArcadeTypography.SizeNote); body.normal.textColor = ArcadeSkin.Cream;
            // below the bond list in the top-right corner (drawn later, it used to cover the card's right half) and as tall as its words
            float words = body.CalcHeight(new GUIContent(selected.Description), width - 50);
            Rect panel = new Rect(viewport.x - width - 35, 340, width, Mathf.Max(160, 95 + words + 24));
            Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
            LivingUI.Pop(Time.unscaledTime - openedAt, panel.center);
            ArcadeSkin.Ticket(panel, ArcadeSkin.Ink, ArcadeSkin.Teal);
            StatGlyph.Draw(new Rect(panel.x + 20, panel.y + 18, 36, 36), selected.Effect);
            GUI.Label(new Rect(panel.x + 68, panel.y + 20, width - 125, 40), selected.Name, heading);
            if (TouchTargets.Button(new Rect(panel.xMax - 55, panel.y + 15, 40, 40), "×")) Close();
            if (selected != null) GUI.Label(new Rect(panel.x + 25, panel.y + 75, width - 50, words + 8), selected.Description, body);
            GUI.matrix = keepMatrix; GUI.color = keepColour;
            if (Time.unscaledTime - openedAt > 30) Close();
        }
    }
}
