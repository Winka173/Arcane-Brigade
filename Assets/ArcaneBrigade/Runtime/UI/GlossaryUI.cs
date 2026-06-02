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
        private static readonly Regex tokens = new Regex("(\\{mark\\}|\\{gives\\}|\\{costs\\}|\\{bonus:[^}]+\\}|[+−-]\\d+(?:\\.\\d+)?%?|(?<!\\p{L})\\d+(?:\\.\\d+)?(?:%|s)?|(?<!\\p{L})(?:" + string.Join("|", entries.Keys.OrderByDescending(key => key.Length).Select(Regex.Escape)) + ")(?!\\p{L})[.,:;!?)]*|\\r?\\n|[^\\S\\r\\n]+|[^\\s]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
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
            Add(result, "armor", "Armor", ItemEffect.Armor, "Takes a bite out of every hit you take.", "armor");
            Add(result, "damage", "Damage", ItemEffect.Damage, "How hard every hit lands.");
            Add(result, "bond", "Bond", ItemEffect.CompanionDamage, "Friends of the same family together unlock a bonus; the card shows how many you need.");
            Add(result, "merge", "Merge", ItemEffect.CompanionDamage, "Two of the same friend at the same stars become one friend with one more star.");
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
                        Matrix4x4 keep = GUI.matrix; ArcadeSkin.TurnAround(45, m.center);
                        ArcadeSkin.Line(m, ArcadeSkin.Gold); ArcadeSkin.Line(new Rect(m.x + mark * .3f, m.y + mark * .3f, mark * .4f, mark * .4f), ArcadeSkin.Cream);
                        GUI.matrix = keep;
                    }
                    horizontal += mark + 9; continue;
                }
                if (word == "{gives}" || word == "{costs}")
                {
                    // the card's GIVES (green) and COSTS (red) plates (play-test 20/10/2026, item 11)
                    bool gives = word == "{gives}"; string caption = gives ? "GIVES" : "COSTS";
                    FontStyle keptStyle = words.fontStyle; words.fontStyle = FontStyle.Bold;
                    float plate = words.CalcSize(new GUIContent(caption)).x * .82f + 16;
                    if (draw)
                    {
                        Rect r = new Rect(bounds.x + horizontal, bounds.y + vertical + line * .12f, plate, line * .76f);
                        ArcadeSkin.Line(r, gives ? new Color(.14f, .40f, .25f, .95f) : new Color(.50f, .11f, .12f, .95f));
                        int size = words.fontSize; words.fontSize = Mathf.RoundToInt(size * .8f);
                        Color prior = GUI.color; GUI.color *= gives ? new Color(.75f, 1f, .78f) : new Color(1f, .8f, .76f);
                        words.alignment = TextAnchor.MiddleCenter; GUI.Label(r, caption, words); words.alignment = TextAnchor.UpperLeft;
                        GUI.color = prior; words.fontSize = size;
                    }
                    words.fontStyle = keptStyle;
                    horizontal += plate + 8; continue;
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
                        Color prior = GUI.color; GUI.color *= ShopDescriptionCard.BenefitColor(effect, bonus[2].StartsWith("-") || bonus[2].StartsWith("−") || bonus[2].StartsWith("×0") ? -1 : 1);
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
