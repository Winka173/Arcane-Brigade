using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The run summary at the end of a journey and in the records (play-test 20/10/2026, item 32): written from the
    /// player's side. A headline (wave reached, time, bosses, score), "Your brigade" (one row per hero or ally: portrait, stars,
    /// a share-of-damage bar and its percentage; every member), all bonds, three small trophies, and relic icons.
    /// No internal ids, no raw floats, no zero columns, no seed.</summary>
    public sealed class RunInvoice
    {
        private Vector2 scroll;
        private string identity;
        private GUIStyle headingStyle, bodyStyle, numericStyle, smallStyle;
        private bool scrollToEnd;
        private float revealedAt;
        private RunSnapshot cachedRun;
        private List<Member> cachedMembers;
        private List<BondRow> cachedBonds;
        public void ScrollToEnd() { scrollToEnd = true; }

        /// <summary>One hero or ally across all its measured sources (attack and effects summed).</summary>
        public sealed class Member { public string UnitId; public int Stars; public double Damage, Gold; public int Kills; public float Share; public bool Hero; }
        public sealed class BondRow { public BondDefinition Bond; public int Count; }

        public static List<BondRow> Bonds(RunSnapshot run, RunCatalog catalog)
        {
            // Restore/validation may migrate old saves; only ever give it a detached copy of the record.
            using var session = new RunSession(catalog);
            session.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(run)));
            var rows = new List<BondRow>();
            foreach (BondDefinition bond in catalog.Bonds)
                if (session.BondCounts.TryGetValue(bond.Id, out int count) && count > 0)
                    rows.Add(new BondRow { Bond = bond, Count = count });
            return rows;
        }

        public static List<Member> Brigade(RunSnapshot run)
        {
            var byUnit = new Dictionary<string, Member>();
            double total = 0;
            foreach (ContributionEntry entry in run.Contributions)
            {
                if (string.IsNullOrEmpty(entry.UnitId)) continue;
                if (!byUnit.TryGetValue(entry.UnitId, out Member m)) byUnit[entry.UnitId] = m = new Member { UnitId = entry.UnitId, Hero = entry.UnitId == run.ChampionId };
                m.Damage += Math.Max(0, entry.Damage); m.Kills += Math.Max(0, entry.Kills); m.Gold += Math.Max(0, entry.Coins);
                total += Math.Max(0, entry.Damage);
            }
            foreach (RosterEntry r in CompanionDeployment.Select(run.Roster))
            {
                if (!byUnit.TryGetValue(r.UnitId, out Member m)) byUnit[r.UnitId] = m = new Member { UnitId = r.UnitId };
                m.Stars = Math.Max(m.Stars, r.Tier);
            }
            if (!byUnit.ContainsKey(run.ChampionId)) byUnit[run.ChampionId] = new Member { UnitId = run.ChampionId, Hero = true };
            var list = new List<Member>(byUnit.Values);
            foreach (Member m in list) m.Share = total > 0 ? (float)(m.Damage / total) : 0;
            list.Sort((a, b) => a.Damage != b.Damage ? b.Damage.CompareTo(a.Damage) : a.Hero != b.Hero ? (a.Hero ? -1 : 1) : string.CompareOrdinal(a.UnitId, b.UnitId));
            return list;
        }

        /// <summary>Big numbers the short way: 940, 12.5K, 3.8M.</summary>
        public static string Short(double value) => value >= 1e9 ? (value / 1e9).ToString("0.#") + "B" : value >= 1e6 ? (value / 1e6).ToString("0.#") + "M" : value >= 1e4 ? (value / 1e3).ToString("0.#") + "K" : Math.Round(value).ToString("N0");
        public static string Time(float seconds) { int s = Mathf.Max(0, Mathf.RoundToInt(seconds)); return s / 60 + ":" + (s % 60).ToString("00"); }
        private static string Stars(int tier) => tier > 0 ? " " + new string('★', Mathf.Clamp(tier, 1, 3)) : "";
        private static string Name(string unitId, RunCatalog catalog) => RunUICopy.UnitName(unitId, catalog.Unit(unitId)?.DisplayName);

        /// <summary>The best relic: the relic whose effects did the most damage.</summary>
        private static (ItemDefinition item, double damage) BestRelic(RunSnapshot run, RunCatalog catalog)
        {
            var damage = new Dictionary<string, double>();
            foreach (ContributionEntry entry in run.Contributions)
                if (!string.IsNullOrEmpty(entry.EffectId) && catalog.Item(entry.EffectId) != null && entry.Damage > 0)
                    damage[entry.EffectId] = (damage.TryGetValue(entry.EffectId, out double d) ? d : 0) + entry.Damage;
            string best = null; double most = 0;
            foreach (var pair in damage) if (pair.Value > most) { most = pair.Value; best = pair.Key; }
            return (best == null ? null : catalog.Item(best), most);
        }

        /// <summary>The summary as plain lines (the records list and checks read it): only things a player can act on or enjoy.</summary>
        public static string Describe(RunSnapshot run, RunCatalog catalog)
        {
            var lines = new StringBuilder();
            UnitDefinition champion = catalog.Unit(run.ChampionId);
            lines.AppendLine("HERO  " + (champion == null ? RunUICopy.UnitName(run.ChampionId) : RunUICopy.UnitName(champion)) + " · " + (champion == null ? "" : RunUICopy.WeaponName(champion.Weapon.Id)));
            lines.AppendLine("WAVE " + run.Wave + " · " + Time(run.CombatSeconds) + " · " + Short(run.TotalKills) + " defeated · " + run.BossKills + (run.BossKills == 1 ? " boss" : " bosses"));
            lines.AppendLine("\nYOUR BRIGADE");
            List<Member> members = Brigade(run);
            foreach (Member m in members)
                lines.AppendLine(Name(m.UnitId, catalog) + Stars(m.Stars) + (m.Damage > 0 ? " · " + Mathf.RoundToInt(m.Share * 100) + "% of damage · " + Short(m.Damage) : "") + (m.Kills > 0 ? " · " + Short(m.Kills) + " defeated" : ""));
            foreach (BondRow row in Bonds(run, catalog))
                lines.AppendLine("BOND  " + row.Bond.Name + " · " + row.Count + (row.Count == 1 ? " member · " : " members · ") + (row.Bond.ActiveLevel(row.Count) > 0 ? "level " + row.Bond.ActiveLevel(row.Count) : "not ready"));
            var (relic, relicDamage) = BestRelic(run, catalog);
            if (relic != null) lines.AppendLine("\nBEST RELIC  " + relic.Name + " · " + Short(relicDamage) + " damage");
            if (run.Items.Count > 0)
            {
                lines.AppendLine("\nRELICS");
                foreach (ItemStack stack in run.Items) if (catalog.Item(stack.Id) is ItemDefinition item) lines.AppendLine(item.Name + (stack.Count > 1 ? " ×" + stack.Count : ""));
            }
            if (run.Phase == SessionPhase.Defeat && !string.IsNullOrEmpty(run.LastDamageSource)) lines.AppendLine("\nFELL TO  " + RunUICopy.DamageSource(run.LastDamageSource, catalog));
            if (HasHaul(run))
            {
                lines.AppendLine("\nFOR THE WAITING ROOM  +" + run.Meta.HaulOil + " Lamp Oil · +" + run.Meta.HaulKeepsakes + " Keepsakes");
                foreach (string line in run.Meta.HaulLines) { string[] p = line.Split('|'); if (p.Length == 3) lines.AppendLine(p[1] + " · +" + p[2]); }
            }
            return lines.ToString();
        }

        /// <summary>The Waiting Room's haul of a finished run (play-test item 27): Lamp Oil and Keepsakes, line by line.</summary>
        public static bool HasHaul(RunSnapshot run) => run?.Meta != null && (run.Meta.HaulOil > 0 || run.Meta.HaulKeepsakes > 0) && run.Meta.HaulLines != null;
        private static float HaulHeight(RunSnapshot run) => HasHaul(run) ? 58 + run.Meta.HaulLines.Count * 26 + 16 : 0;
        private void DrawHaul(float y, float inner, RunSnapshot run, GUIStyle body, GUIStyle number, GUIStyle small)
        {
            ArcadeSkin.Line(new Rect(0, y, inner, HaulHeight(run) - 10), new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .08f));
            GUI.Label(new Rect(8, y + 8, inner * .5f, 28), "FOR THE WAITING ROOM", body);
            float x = inner - 8;
            foreach (var (icon, amount) in new[] { (MetaIcons.Keepsake, run.Meta.HaulKeepsakes), (MetaIcons.LampOil, run.Meta.HaulOil) })
            {
                string text = "+" + amount; float w = body.CalcSize(new GUIContent(text)).x;
                body.alignment = TextAnchor.MiddleRight; GUI.Label(new Rect(x - w - 4, y + 8, w + 4, 34), text, body); body.alignment = TextAnchor.UpperLeft;
                RunHud.Sprite(new Rect(x - w - 44, y + 6, 36, 36), icon); x -= w + 60;
            }
            float row = y + 48;
            foreach (string line in run.Meta.HaulLines)
            {
                string[] p = line.Split('|'); if (p.Length != 3) continue;
                RunHud.Sprite(new Rect(10, row, 22, 22), p[0] == "keep" ? MetaIcons.Keepsake : MetaIcons.LampOil);
                GUI.Label(new Rect(40, row, inner - 130, 24), p[1], small);
                small.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(inner - 90, row, 80, 24), "+" + p[2], small); small.alignment = TextAnchor.UpperLeft;
                row += 26;
            }
        }

        public void Draw(Rect rectangle, RunSnapshot run, RunCatalog catalog, string nickname = null, int frozenScore = -1)
        {
            if (identity != run.RunId) { identity = run.RunId; scroll = Vector2.zero; revealedAt = UnityEngine.Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.receipt"); }
            if (cachedRun != run || cachedMembers == null) { cachedRun = run; cachedMembers = Brigade(run); cachedBonds = Bonds(run, catalog); }
            if (ArcadeSkin.HasKit) { ArcadeSkin.Frame(rectangle, KitFrame.Card, .6f); rectangle = ArcadeSkin.Content(rectangle, KitFrame.Card, true, .6f); } // the cream card; the receipt sits inside its rim
            else
            {
                ArcadeSkin.Line(new Rect(rectangle.x + 5, rectangle.y + 7, rectangle.width, rectangle.height), ArcadeSkin.Ink);
                ArcadeSkin.Line(rectangle, ArcadeSkin.Paper);
                for (float horizontal = rectangle.x; horizontal < rectangle.xMax; horizontal += 22)
                    ArcadeSkin.Line(new Rect(horizontal, rectangle.yMax - 6, 10, 6), ArcadeSkin.Ink);
            }
            GUIStyle heading = headingStyle ?? (headingStyle = ArcadeTypography.Style(26, true));
            GUIStyle body = bodyStyle ?? (bodyStyle = ArcadeTypography.Style(19));
            GUIStyle number = numericStyle ?? (numericStyle = ArcadeTypography.Style(22, false, true));
            GUIStyle small = smallStyle ?? (smallStyle = ArcadeTypography.Style(16));
            heading.normal.textColor = body.normal.textColor = number.normal.textColor = ArcadeSkin.Ink;
            small.normal.textColor = new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .72f);
            heading.wordWrap = false; heading.clipping = TextClipping.Clip;

            // headline: who, how far, how long, how many bosses
            float x = rectangle.x + 24, w = rectangle.width - 48;
            GUI.Label(new Rect(x, rectangle.y + 14, w, 40), nickname ?? RunMeta.Nickname(run), heading);
            string journey = (run.Endless ? "Endless" : "Journey") + " · " + RunMeta.DifficultyNames[Mathf.Clamp(run.Difficulty, 0, RunMeta.DifficultyNames.Length - 1)] + " · " + (run.Phase == SessionPhase.Victory ? "Dawn reached" : "Ended at wave " + run.Wave);
            GUI.Label(new Rect(x, rectangle.y + 54, w, 28), journey, body);
            GUI.Label(new Rect(x, rectangle.y + 86, w, 32), "WAVE " + run.Wave + "  ·  " + Time(run.CombatSeconds) + "  ·  " + run.BossKills + (run.BossKills == 1 ? " BOSS" : " BOSSES") + "  ·  SCORE " + (frozenScore < 0 ? RunMeta.Score(run) : frozenScore).ToString("N0"), number);
            ArcadeSkin.Divider(new Rect(x, rectangle.y + 126, w, 2), ArcadeSkin.Ink);

            Rect view = new Rect(rectangle.x + 16, rectangle.y + 136, rectangle.width - 32, rectangle.height - 150);
            float inner = view.width - 14;
            int shown = cachedMembers.Count;
            var (relic, relicDamage) = BestRelic(run, catalog);
            int columns = Mathf.Max(1, Mathf.FloorToInt(inner / 76));
            int relicRows = Mathf.CeilToInt(run.Items.Count / (float)columns);
            float bondHeight = cachedBonds.Count > 0 ? 40 + cachedBonds.Count * 54 : 0;
            float height = 34 + shown * 66 + bondHeight + 20 + 120 + (run.Items.Count > 0 ? 40 + relicRows * 76 : 0) + (run.Phase == SessionPhase.Defeat ? 44 : 0) + 10 + HaulHeight(run);
            if (scrollToEnd) { scroll.y = height; scrollToEnd = false; }
            scroll = ScrollPanel.Begin(view, scroll, new Rect(0, 0, inner, height), new Color(ArcadeSkin.Paper.r * .6f, ArcadeSkin.Paper.g * .6f, ArcadeSkin.Paper.b * .6f, .5f));
            float y = 0;
            if (HasHaul(run)) { DrawHaul(y, inner, run, body, number, small); y += HaulHeight(run); }
            GUI.Label(new Rect(4, y, inner, 30), "YOUR BRIGADE", body); y += 34;
            for (int index = 0; index < shown; index++)
            {
                Member m = cachedMembers[index];
                float reveal = LivingUI.Reveal(UnityEngine.Time.unscaledTime - revealedAt - Mathf.Min(index, 12) * 0.04f, PresentationSettings.ReducedMotion);
                Color keep = GUI.color; GUI.color *= new Color(1, 1, 1, reveal);
                UnitDefinition unit = catalog.Unit(m.UnitId);
                RunHud.Sprite(new Rect(4, y + 4, 54, 54), unit != null ? unit.Body : null);
                float textX = 68, right = 92;
                GUI.Label(new Rect(textX, y, inner - textX - right, 28), (m.Hero ? "YOU · " : "") + Name(m.UnitId, catalog) + Stars(m.Stars), body);
                Rect bar = new Rect(textX, y + 32, inner - textX - right, 12);
                ArcadeSkin.Line(bar, new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .14f));
                ArcadeSkin.Line(new Rect(bar.x, bar.y, bar.width * m.Share * reveal, bar.height), m.Hero ? ArcadeSkin.Accent : ArcadeSkin.Teal * .85f);
                if (m.Damage > 0 || m.Kills > 0)
                    GUI.Label(new Rect(textX, y + 44, inner - textX - right, 20), (m.Damage > 0 ? Short(m.Damage) + " damage" : "") + (m.Damage > 0 && m.Kills > 0 ? " · " : "") + (m.Kills > 0 ? Short(m.Kills) + " defeated" : ""), small);
                number.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(inner - right, y + 8, right - 4, 40), m.Damage > 0 ? Mathf.RoundToInt(m.Share * 100) + "%" : "", number);
                number.alignment = TextAnchor.UpperLeft;
                GUI.color = keep;
                y += 66;
            }
            if (cachedBonds.Count > 0)
            {
                GUI.Label(new Rect(4, y + 6, inner, 30), "BONDS", body); y += 40;
                foreach (BondRow row in cachedBonds)
                {
                    RunHud.Sprite(new Rect(4, y + 4, 40, 40), row.Bond.Icon);
                    GUI.Label(new Rect(54, y, inner - 58, 26), row.Bond.Name, body);
                    int level = row.Bond.ActiveLevel(row.Count);
                    GUI.Label(new Rect(54, y + 26, inner - 58, 24), row.Count + (row.Count == 1 ? " member · " : " members · ") + (level > 0 ? "Level " + level : "Not ready"), small);
                    y += 54;
                }
            }
            y += 20;
            // three small trophies
            float cardW = (inner - 24) / 3f;
            Member richest = null, slayer = null;
            foreach (Member m in cachedMembers) { if (m.Gold > 0 && (richest == null || m.Gold > richest.Gold)) richest = m; if (m.Kills > 0 && (slayer == null || m.Kills > slayer.Kills)) slayer = m; }
            Trophy(new Rect(0, y, cardW, 108), "BEST RELIC", relic?.Icon, relic != null ? relic.Name : "No relic yet", relic != null ? Short(relicDamage) + " damage" : "", body, small);
            Trophy(new Rect(cardW + 12, y, cardW, 108), "MOST GOLD", richest != null ? catalog.Unit(richest.UnitId)?.Body : null, richest != null ? Name(richest.UnitId, catalog) : "None yet", richest != null ? Short(richest.Gold) + " gold" : "", body, small);
            Trophy(new Rect(cardW * 2 + 24, y, cardW, 108), "MOST DEFEATED", slayer != null ? catalog.Unit(slayer.UnitId)?.Body : null, slayer != null ? Name(slayer.UnitId, catalog) : "None yet", slayer != null ? Short(slayer.Kills) : "", body, small);
            y += 120;
            if (run.Items.Count > 0)
            {
                GUI.Label(new Rect(4, y, inner, 30), "RELICS", body); y += 40;
                for (int index = 0; index < run.Items.Count; index++)
                {
                    ItemStack stack = run.Items[index];
                    Rect cell = new Rect(index % columns * 76, y + index / columns * 76, 68, 68);
                    RunHud.Sprite(new Rect(cell.x + 4, cell.y + 2, 56, 56), catalog.Item(stack.Id)?.Icon);
                    if (stack.Count > 1) { number.alignment = TextAnchor.LowerRight; GUI.Label(cell, "×" + stack.Count, number); number.alignment = TextAnchor.UpperLeft; }
                }
                y += relicRows * 76;
            }
            if (run.Phase == SessionPhase.Defeat && !string.IsNullOrEmpty(run.LastDamageSource))
                GUI.Label(new Rect(4, y + 6, inner, 32), "FELL TO  " + RunUICopy.DamageSource(run.LastDamageSource, catalog), body);
            ScrollPanel.End();
        }

        private static void Trophy(Rect r, string title, Sprite icon, string name, string amount, GUIStyle body, GUIStyle small)
        {
            ArcadeSkin.Line(r, new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .08f));
            GUI.Label(new Rect(r.x + 8, r.y + 4, r.width - 16, 22), title, small);
            if (icon != null) RunHud.Sprite(new Rect(r.x + 8, r.y + 32, 60, 60), icon);
            float tx = icon != null ? r.x + 74 : r.x + 8;
            body.wordWrap = true;
            GUI.Label(new Rect(tx, r.y + 28, r.xMax - tx - 6, 48), name, body);
            body.wordWrap = false;
            GUI.Label(new Rect(tx, r.y + 78, r.xMax - tx - 6, 24), amount, small);
        }
    }
}
