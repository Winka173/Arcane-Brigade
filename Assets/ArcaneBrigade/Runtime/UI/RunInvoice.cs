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
        private static string Stars(int tier) => tier > 0 ? " " + new string('★', Mathf.Clamp(tier, 1, 5)) + (tier >= 5 ? "  ONE OF A KIND" : "") : "";
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
        private static float HaulHeight(RunSnapshot run) => HasHaul(run) ? 60 + run.Meta.HaulLines.Count * 38 + 16 + 44 : 0;
        private void DrawHaul(float y, float inner, RunSnapshot run, GUIStyle body, GUIStyle number, GUIStyle small)
        {
            ArcadeSkin.Line(new Rect(0, y, inner, HaulHeight(run) - 10), new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .08f));
            GUI.Label(new Rect(8, y + 8, inner * .5f, 36), "For the waiting room", body);
            float x = inner - 8;
            foreach (var (icon, amount) in new[] { (MetaIcons.Keepsake, run.Meta.HaulKeepsakes), (MetaIcons.LampOil, run.Meta.HaulOil) })
            {
                string text = "+" + amount; float w = body.CalcSize(new GUIContent(text)).x;
                body.alignment = TextAnchor.MiddleRight; GUI.Label(new Rect(x - w - 4, y + 8, w + 4, 38), text, body); body.alignment = TextAnchor.UpperLeft;
                RunHud.Sprite(new Rect(x - w - 48, y + 6, 40, 40), icon); x -= w + 64;
            }
            float row = y + 56;
            foreach (string line in run.Meta.HaulLines)
            {
                string[] p = line.Split('|'); if (p.Length != 3) continue;
                RunHud.Sprite(new Rect(10, row + 2, 30, 30), p[0] == "keep" ? MetaIcons.Keepsake : MetaIcons.LampOil);
                GUI.Label(new Rect(50, row, inner - 150, 36), p[1], small);
                small.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(inner - 100, row, 90, 36), "+" + p[2], small); small.alignment = TextAnchor.UpperLeft;
                row += 38;
            }
            // where to spend it (players did not know the permanent upgrades were there)
            GUI.Label(new Rect(10, row + 4, inner - 20, 38), "Spend them in the Waiting Room, at the Lamp Tree.", small);
        }

        public void Draw(Rect rectangle, RunSnapshot run, RunCatalog catalog, string nickname = null, int frozenScore = -1)
        {
            if (identity != run.RunId) { identity = run.RunId; scroll = Vector2.zero; revealedAt = UnityEngine.Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.receipt"); }
            if (cachedRun != run || cachedMembers == null) { cachedRun = run; cachedMembers = Brigade(run); cachedBonds = Bonds(run, catalog); }
            // the painted summary sheet: the run's name sits in its header strip, everything else on the paper below
            if (ArcadeSkin.KitSliced(rectangle, "Panels/SummarySheet", .7f)) rectangle = new Rect(rectangle.x + 40, rectangle.y + 4, rectangle.width - 80, rectangle.height - 40); // the title is centred in the painted header strip (77 px at this scale)
            else if (ArcadeSkin.HasKit) { ArcadeSkin.Frame(rectangle, KitFrame.Card, .6f); rectangle = ArcadeSkin.Content(rectangle, KitFrame.Card, true, .6f); } // the cream card; the receipt sits inside its rim
            else
            {
                ArcadeSkin.Line(new Rect(rectangle.x + 5, rectangle.y + 7, rectangle.width, rectangle.height), ArcadeSkin.Ink);
                ArcadeSkin.Line(rectangle, ArcadeSkin.Paper);
                for (float horizontal = rectangle.x; horizontal < rectangle.xMax; horizontal += 22)
                    ArcadeSkin.Line(new Rect(horizontal, rectangle.yMax - 6, 10, 6), ArcadeSkin.Ink);
            }
            GUIStyle heading = headingStyle ?? (headingStyle = ArcadeTypography.Style(ArcadeTypography.SizeItem, true));
            GUIStyle body = bodyStyle ?? (bodyStyle = ArcadeTypography.Style(ArcadeTypography.SizeNote));
            GUIStyle number = numericStyle ?? (numericStyle = ArcadeTypography.Style(ArcadeTypography.SizeNote, false, true));
            GUIStyle small = smallStyle ?? (smallStyle = ArcadeTypography.Style(ArcadeTypography.SizeNote)); // UI standard: nothing under 28
            heading.normal.textColor = body.normal.textColor = number.normal.textColor = ArcadeSkin.Ink;
            small.normal.textColor = new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .72f);
            heading.wordWrap = false; heading.clipping = TextClipping.Clip;

            // headline: who, how far, how long, how many bosses
            float x = rectangle.x + 24, w = rectangle.width - 48;
            heading.alignment = TextAnchor.MiddleLeft; GUI.Label(new Rect(x, rectangle.y + 10, w, 60), nickname ?? RunMeta.Nickname(run), heading); heading.alignment = TextAnchor.UpperLeft;
            string journey = (run.Endless ? "Endless" : "Journey") + " · " + RunMeta.DifficultyNames[Mathf.Clamp(run.Difficulty, 0, RunMeta.DifficultyNames.Length - 1)] + " · " + (run.Phase == SessionPhase.Victory ? "Dawn reached" : "Ended at wave " + run.Wave);
            GUI.Label(new Rect(x, rectangle.y + 84, w, 36), journey, body); // under the strip, with a breath
            GUI.Label(new Rect(x, rectangle.y + 118, w, 36), "Wave " + run.Wave + "  ·  " + Time(run.CombatSeconds) + "  ·  " + run.BossKills + (run.BossKills == 1 ? " boss" : " bosses"), number);
            ArcadeSkin.Divider(new Rect(x, rectangle.y + 160, w, 2), ArcadeSkin.Ink);

            Rect view = new Rect(rectangle.x + 16, rectangle.y + 172, rectangle.width - 32, rectangle.height - 186);
            float inner = view.width - 14;
            int shown = cachedMembers.Count;
            var (relic, relicDamage) = BestRelic(run, catalog);
            int columns = Mathf.Max(1, Mathf.FloorToInt(inner / 76));
            int relicRows = Mathf.CeilToInt(run.Items.Count / (float)columns);
            float bondHeight = cachedBonds.Count > 0 ? 50 + cachedBonds.Count * 76 : 0;
            float height = 44 + shown * 96 + bondHeight + 20 + 166 + (run.Items.Count > 0 ? 50 + relicRows * 76 : 0) + (run.Phase == SessionPhase.Defeat ? 50 : 0) + 10 + HaulHeight(run);
            if (scrollToEnd) { scroll.y = height; scrollToEnd = false; }
            scroll = ScrollPanel.Begin(view, scroll, new Rect(0, 0, inner, height), new Color(ArcadeSkin.Paper.r * .6f, ArcadeSkin.Paper.g * .6f, ArcadeSkin.Paper.b * .6f, .5f));
            float y = 0;
            if (HasHaul(run)) { DrawHaul(y, inner, run, body, number, small); y += HaulHeight(run); }
            GUI.Label(new Rect(4, y, inner, 38), "Your brigade", body); y += 44;
            for (int index = 0; index < shown; index++)
            {
                Member m = cachedMembers[index];
                float reveal = LivingUI.Reveal(UnityEngine.Time.unscaledTime - revealedAt - Mathf.Min(index, 12) * 0.04f, PresentationSettings.ReducedMotion);
                Color keep = GUI.color; GUI.color *= new Color(1, 1, 1, reveal);
                UnitDefinition unit = catalog.Unit(m.UnitId);
                RunHud.Sprite(new Rect(4, y + 8, 70, 70), unit != null ? unit.Body : null);
                float textX = 86, right = 100;
                GUI.Label(new Rect(textX, y, inner - textX - right, 36), (m.Hero ? "You · " : "") + Name(m.UnitId, catalog) + Stars(m.Stars), body);
                Rect bar = new Rect(textX, y + 38, inner - textX - right, 18);
                // the share of damage on a painted walnut groove with a lamp-gold fill
                if (ArcadeSkin.Strip(bar, "Info/RatioTrack", 24, 24))
                {
                    Rect fill = new Rect(bar.x + 3, bar.y + 3, (bar.width - 6) * m.Share * reveal, bar.height - 6);
                    if (fill.width >= fill.height) ArcadeSkin.Strip(fill, "Info/RatioFill", 20, 20);
                }
                else
                {
                    ArcadeSkin.Line(bar, new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .14f));
                    ArcadeSkin.Line(new Rect(bar.x, bar.y, bar.width * m.Share * reveal, bar.height), m.Hero ? ArcadeSkin.Accent : ArcadeSkin.Teal * .85f);
                }
                if (m.Damage > 0 || m.Kills > 0)
                    GUI.Label(new Rect(textX, y + 58, inner - textX - right, 36), (m.Damage > 0 ? Short(m.Damage) + " damage" : "") + (m.Damage > 0 && m.Kills > 0 ? " · " : "") + (m.Kills > 0 ? Short(m.Kills) + " defeated" : ""), small);
                number.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(inner - right, y + 20, right - 4, 40), m.Damage > 0 ? Mathf.RoundToInt(m.Share * 100) + "%" : "", number);
                number.alignment = TextAnchor.UpperLeft;
                GUI.color = keep;
                y += 96;
            }
            if (cachedBonds.Count > 0)
            {
                GUI.Label(new Rect(4, y + 6, inner, 38), "Bonds", body); y += 50;
                foreach (BondRow row in cachedBonds)
                {
                    RunHud.Sprite(new Rect(4, y + 8, 52, 52), row.Bond.Icon);
                    GUI.Label(new Rect(68, y, inner - 72, 36), row.Bond.Name, body);
                    int level = row.Bond.ActiveLevel(row.Count);
                    GUI.Label(new Rect(68, y + 36, inner - 72, 36), row.Count + (row.Count == 1 ? " member · " : " members · ") + (level > 0 ? "Level " + level : "Not ready"), small);
                    y += 76;
                }
            }
            y += 20;
            // three small trophies
            float cardW = (inner - 24) / 3f;
            Member richest = null, slayer = null;
            foreach (Member m in cachedMembers) { if (m.Gold > 0 && (richest == null || m.Gold > richest.Gold)) richest = m; if (m.Kills > 0 && (slayer == null || m.Kills > slayer.Kills)) slayer = m; }
            Trophy(new Rect(0, y, cardW, 154), "Best relic", relic?.Icon, relic != null ? relic.Name : "No relic yet", relic != null ? Short(relicDamage) + " damage" : "", body, small);
            Trophy(new Rect(cardW + 12, y, cardW, 154), "Most gold", richest != null ? catalog.Unit(richest.UnitId)?.Body : null, richest != null ? Name(richest.UnitId, catalog) : "None yet", richest != null ? Short(richest.Gold) + " gold" : "", body, small);
            Trophy(new Rect(cardW * 2 + 24, y, cardW, 154), "Most defeated", slayer != null ? catalog.Unit(slayer.UnitId)?.Body : null, slayer != null ? Name(slayer.UnitId, catalog) : "None yet", slayer != null ? Short(slayer.Kills) : "", body, small);
            y += 166;
            if (run.Items.Count > 0)
            {
                GUI.Label(new Rect(4, y, inner, 38), "Relics", body); y += 50;
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
                GUI.Label(new Rect(4, y + 6, inner, 38), "Fell to " + RunUICopy.DamageSource(run.LastDamageSource, catalog), body);
            ScrollPanel.End();
        }

        private static void Trophy(Rect r, string title, Sprite icon, string name, string amount, GUIStyle body, GUIStyle small)
        {
            ArcadeSkin.Line(r, new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .08f));
            GUI.Label(new Rect(r.x + 8, r.y + 4, r.width - 16, 36), title, small);
            // each trophy in a brass medal socket (an empty socket when nobody earned it)
            Sprite socket = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite("Info/MedalSocket");
            if (socket != null) GUI.DrawTexture(new Rect(r.x + 4, r.y + 40, 80, 80), socket.texture, ScaleMode.ScaleToFit);
            if (icon != null) RunHud.Sprite(socket != null ? new Rect(r.x + 19, r.y + 50, 50, 50) : new Rect(r.x + 8, r.y + 46, 66, 66), icon);
            float tx = icon != null || socket != null ? r.x + 92 : r.x + 8;
            body.wordWrap = true;
            GUI.Label(new Rect(tx, r.y + 42, r.xMax - tx - 6, 70), name, body);
            body.wordWrap = false;
            GUI.Label(new Rect(tx, r.y + 112, r.xMax - tx - 6, 36), amount, small);
        }
    }
}
