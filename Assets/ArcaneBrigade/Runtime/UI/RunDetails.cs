using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunDetails
    {
        private RunBuildReadModel model;
        private int tab;
        private GUIStyle tabStyle;
        private GUIStyle handStyle;
        private GUIStyle seatName;
        private GUIStyle sayHand;
        private int selection;
        private Vector2 scroll;
        private Vector2 explanationScroll;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle numberStyle;
        private GUIStyle buttonStyle;
        private float openedAt;
        private float selectedAt;
        private readonly HeroIdentityOptions heroOptions = new HeroIdentityOptions();
        private GUIStyle rarityStyle;
        public bool Open => model != null;
        public int Tab => tab;
        public bool HeroSelected => Open && tab == 0 && selection == 0;
        public const int TabCount = 4;
        public void Show(RunSession session, RunCatalog catalog, float health, CombatSimulation simulation = null) { if (model == null) CheckpointAudio.PlayInterfaceId("ui.open"); model = RunBuildReadModel.Capture(session, catalog, health, simulation); selection = 0; scroll = explanationScroll = Vector2.zero; openedAt = selectedAt = Time.unscaledTime; }
        public void Close() { if (model != null) CheckpointAudio.PlayInterfaceId("ui.close"); model = null; GlossaryUI.Close(); }
        /// <summary>B11: the brigade sits at one long table, a seat per species (back row behind the table, front row before it).
        /// Clicking someone selects them; they lean in and speak their key facts and a line in their own hand.</summary>
        private GUIStyle badgeStyle, tileName;
        private void DrawTable(List<RunBuildReadModel.Row> rows, float rowHeight, RunSnapshot state)
        {
            var seats = new List<int>();
            var counts = new Dictionary<string, int>();
            for (int index = 0; index < rows.Count; index++)
            {
                string key = SeatKey(rows[index]);
                if (counts.ContainsKey(key)) { counts[key]++; continue; }
                counts[key] = 1;
                if (seats.Count < 14) seats.Add(index);
            }
            Rect area = new Rect(70, 290, 1060, 340);
            float breathe = PresentationSettings.ReducedMotion ? 0.6f : 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 1.8f);
            ArcadeSkin.Glow(new Rect(area.x + 180, area.y + 40, area.width - 360, area.height), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.20f + 0.05f * breathe));
            Rect top = new Rect(area.x + 90, area.y + 194, area.width - 180, 74);
            int back = Mathf.CeilToInt(seats.Count / 2f), front = seats.Count - back;
            int selectedSeat = -1;
            string selectedKey = selection >= 0 && selection < rows.Count ? SeatKey(rows[selection]) : null;
            for (int k = 0; k < seats.Count; k++) if (SeatKey(rows[seats[k]]) == selectedKey) selectedSeat = k;
            Rect SeatRect(int k)
            {
                bool backRow = k < back;
                int n = backRow ? back : front, i = backRow ? k : k - back;
                float span = top.width - 80, x = top.x + 40 + (n == 1 ? span * 0.5f : span * i / (n - 1)) - 48;
                float y = backRow ? top.y - 96 : top.y + 30;
                if (k == selectedSeat) y -= PresentationSettings.ReducedMotion ? 0 : 6;
                return new Rect(x, y, 96, 104);
            }
            // back row sits behind the table
            for (int k = 0; k < back; k++) DrawSeat(rows, seats[k], counts, SeatRect(k), k == selectedSeat, rowHeight);
            ArcadeSkin.Slant(new Rect(top.x + 6, top.y + 10, top.width, top.height), new Color(0, 0, 0, 0.5f));
            ArcadeSkin.Slant(top, new Color(0.30f, 0.19f, 0.11f, 1f));
            ArcadeSkin.Slant(new Rect(top.x + 8, top.y + 6, top.width - 16, 10), new Color(0.48f, 0.31f, 0.18f, 1f));
            ArcadeSkin.Divider(new Rect(top.x + 30, top.y + top.height - 16, top.width - 60, 1), new Color(ArcadeSkin.Paper.r, ArcadeSkin.Paper.g, ArcadeSkin.Paper.b, 0.25f));
            // a candle in the middle of the table
            Vector2 candle = new Vector2(top.center.x, top.y + 6);
            ArcadeSkin.Line(new Rect(candle.x - 5, candle.y - 26, 10, 28), new Color(0.94f, 0.90f, 0.80f));
            ArcadeSkin.Glow(new Rect(candle.x - 30, candle.y - 66, 60, 60), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, 0.65f * breathe));
            ArcadeSkin.Dot(new Rect(candle.x - 5, candle.y - 44, 10, 16), new Color(1f, 0.88f, 0.6f));
            for (int k = back; k < seats.Count; k++) DrawSeat(rows, seats[k], counts, SeatRect(k), k == selectedSeat, rowHeight);
            int hidden = counts.Count - seats.Count;
            if (seatName == null) { seatName = ArcadeTypography.Style(22, true); seatName.alignment = TextAnchor.MiddleCenter; seatName.wordWrap = false; }
            if (hidden > 0) { seatName.normal.textColor = ArcadeSkin.Cream; GUI.Label(new Rect(area.xMax - 220, area.yMax - 28, 210, 22), "+" + hidden + " MORE AT THE TABLE", seatName); }
            // the selected guest speaks
            if (selectedSeat >= 0)
            {
                RunBuildReadModel.Row row = rows[seats[selectedSeat]];
                Rect seat = SeatRect(selectedSeat);
                string said = CompanionVoice.Line(row.UnitId, state.Wave, state);
                string facts = row.Name + "  ·  " + (row.Summary ?? "").Split('\n')[0];
                if (sayHand == null) { sayHand = ArcadeTypography.Style(25); sayHand.font = ArcadeTypography.Hand; sayHand.normal.textColor = new Color(0.25f, 0.20f, 0.14f); sayHand.wordWrap = true; }
                seatName.normal.textColor = ArcadeSkin.Ink;
                float width = 640, height = string.IsNullOrEmpty(said) ? 48 : 48 + Mathf.Min(36, sayHand.CalcHeight(new GUIContent(said), width - 36));
                // inside the table's area, under the tabs and above the back row (requirement 22/10/2026: it covered the tabs)
                Rect bubble = new Rect(Mathf.Clamp(seat.center.x - width * 0.5f, area.x, area.xMax - width), area.y + 2, width, height);
                ArcadeSkin.Ticket(bubble, ArcadeSkin.Paper, ArcadeSkin.Accent);
                seatName.alignment = TextAnchor.MiddleLeft;
                GUI.Label(new Rect(bubble.x + 18, bubble.y + 12, width - 36, 24), facts, seatName);
                seatName.alignment = TextAnchor.MiddleCenter;
                if (!string.IsNullOrEmpty(said)) GUI.Label(new Rect(bubble.x + 18, bubble.y + 36, width - 36, height - 48), said, sayHand);
            }
        }

        private static string SeatKey(RunBuildReadModel.Row row) => (row.UnitId ?? "") + "/" + row.Name;

        private void DrawSeat(List<RunBuildReadModel.Row> rows, int index, Dictionary<string, int> counts, Rect seat, bool chosen, float rowHeight)
        {
            RunBuildReadModel.Row row = rows[index];
            if (chosen) ArcadeSkin.Glow(new Rect(seat.x - 24, seat.y - 18, seat.width + 48, seat.height + 36), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, 0.45f));
            // chair back, then the guest
            ArcadeSkin.Slant(new Rect(seat.x + 22, seat.y + 30, seat.width - 44, 64), new Color(0.36f, 0.23f, 0.13f, 0.9f));
            if (row.Icon != null) RunHud.Sprite(new Rect(seat.x + 4, seat.y, seat.width - 8, seat.height - 10), row.Icon);
            int many = counts.TryGetValue(SeatKey(row), out int c) ? c : 1;
            if (many > 1)
            {
                if (seatName == null) { seatName = ArcadeTypography.Style(22, true); seatName.alignment = TextAnchor.MiddleCenter; seatName.wordWrap = false; }
                Rect badge = new Rect(seat.xMax - 34, seat.y + 4, 34, 22);
                ArcadeSkin.Slant(badge, ArcadeSkin.Ink);
                seatName.normal.textColor = ArcadeSkin.Gold;
                GUI.Label(badge, "×" + many, seatName);
            }
            if (TouchTargets.Button(seat, GUIContent.none, GUIStyle.none))
            {
                selection = index; selectedAt = Time.unscaledTime; explanationScroll = Vector2.zero;
                scroll.y = index * rowHeight;
                CheckpointAudio.PlayInterfaceId("ui.select");
            }
        }

        public void SelectTab(int index)
        {
            int next = Mathf.Clamp(index, 0, TabCount - 1);
            if (tab != next) CheckpointAudio.PlayInterfaceId("ui.tab");
            tab = next; selection = 0; scroll = explanationScroll = Vector2.zero; selectedAt = Time.unscaledTime;
        }
        public void MoveSelection(int direction)
        {
            if (model == null) return;
            int count = tab == 0 ? model.Stats.Count : tab == 1 ? model.Companions.Count : tab == 2 ? model.Relics.Count : model.Links.Count;
            int previousSelection = selection;
            selection = Mathf.Clamp(selection + direction, 0, Mathf.Max(0, count - 1));
            if (selection != previousSelection) CheckpointAudio.PlayInterfaceId("ui.page");
            selectedAt = Time.unscaledTime;
            float height = tab == 0 ? 115 : 145;
            scroll.y = Mathf.Clamp(scroll.y, Mathf.Max(0, (selection + 1) * height - 670), selection * height);
            explanationScroll = Vector2.zero;
        }
        public void SelectBond(string name)
        {
            if (model == null) return;
            SelectTab(3);
            selection = Mathf.Max(0, model.Links.FindIndex(row => row.Name == "Bond " + name));
            scroll.y = selection * 145; explanationScroll = Vector2.zero;
        }

        public void Draw(ArenaController owner, Camera camera)
        {
            if (model == null) return;
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            Color previousTint = GUI.color;
            LivingUI.Enter(Time.unscaledTime - openedAt, new Vector2(960, 540));
            GlossaryUI.BlockUnderlying(new Vector2(1920, 1080));
            GUIStyle title = titleStyle ?? (titleStyle = ArcadeTypography.Style(44, true));
            GUIStyle text = bodyStyle ?? (bodyStyle = ArcadeTypography.Style(ArcadeTypography.SizeNote));
            GUIStyle number = numberStyle ?? (numberStyle = ArcadeTypography.Style(27, false, true));
            GUIStyle button = buttonStyle ?? (buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 26 });
            CardPop.Apply("brigade", new Rect(30, 20, 1860, 1040)); // the colour is restored with previousTint at the end
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(new Rect(30, 20, 1860, 1040), KitFrame.Dialogue, .7f); else RunHud.Fill(new Rect(30, 20, 1860, 1040), ArcadeSkin.Deep);
            ArcadeSkin.Header(new Rect(310, 50, 1300, 100), "", "BRIGADE");
            if (!ArcadeSkin.HasKit) ArcadeSkin.Divider(new Rect(75, 167, 1750, 2), ArcadeSkin.Accent);
            if (ArcadeSkin.KitGuiButton(new Rect(1610, 55, 220, 70), "CLOSE", button) && !CardPop.Closing("brigade")) CardPop.CloseThen("brigade", owner.ToggleDetails);
            // requirement 24/10/2026 ("FRIENDS 3, RELICS 0, LINK 55 - nobody understands"): each tab has an icon and a plain name, its
            // count in a little bubble
            string[] names = { "STATS", "FRIENDS", "RELICS", "BONDS" };
            int[] counts = { -1, model.Companions.Count, model.Relics.Count, model.Links.Count };
            if (tabStyle == null) { tabStyle = ArcadeTypography.Style(ArcadeTypography.SizeNote, true); tabStyle.alignment = TextAnchor.MiddleCenter; }
            for (int index = 0; index < names.Length; index++)
            {
                Rect tabRect = new Rect(75 + index * 372, 200, 320, 62); // the painted end caps hang outside the bar: 52 px apart so they never overlap
                bool current = index == tab;
                // painted tabs (UI kit): velvet when current, walnut otherwise, brass under the pointer
                if (ArcadeSkin.Tab(tabRect, "     " + names[index], current, true, tabStyle)) SelectTab(index);
                Rect icon = new Rect(tabRect.x + 34, tabRect.y + 13, 36, 36);
                if (index == 0) { if (!PaintedStatIcons.TryDraw(icon, ItemEffect.Damage)) RunHud.Sprite(icon, MetaIcons.Keepsake); }
                else RunHud.Sprite(icon, index == 1 ? MetaIcons.Station(MetaStation.Nursery, true) : index == 2 ? MetaIcons.Keepsake : owner.Content.Bonds != null && owner.Content.Bonds.Length > 0 ? owner.Content.Bonds[0].Icon : MetaIcons.Keepsake);
                if (counts[index] >= 0)
                {
                    Vector2 bubble = new Vector2(tabRect.xMax - 44, tabRect.center.y);
                    ArcadeSkin.Diamond(bubble, 34, new Color(.12f, .08f, .1f, .95f)); ArcadeSkin.Diamond(bubble, 28, counts[index] > 0 ? ArcadeSkin.Accent : new Color(.4f, .36f, .32f));
                    badgeStyle ??= ArcadeTypography.Style(22, true); badgeStyle.alignment = TextAnchor.MiddleCenter; badgeStyle.normal.textColor = ArcadeSkin.KitInk;
                    GUI.Label(new Rect(bubble.x - 20, bubble.y - 14, 40, 28), counts[index].ToString(), badgeStyle);
                }
            }
            List<RunBuildReadModel.Row> rows = tab == 0 ? model.Stats : tab == 1 ? model.Companions : tab == 2 ? model.Relics : model.Links;
            float rowHeight = tab == 0 ? 115 : 145;
            bool table = tab == 1 && rows.Count > 0;
            if (table) DrawTable(rows, rowHeight, owner.Session.State);
            Rect listArea = table ? new Rect(70, 640, 1060, 325) : new Rect(70, 295, 1060, 670);
            // friends and relics as a grid of picture tiles (requirement 24/10/2026: "the content is still ugly"), stats and bonds as rows
            bool grid = tab == 1 || tab == 2;
            const int cols = 5; const float tileW = 186, tileH = 196, tileGap = 15;
            float contentHeight = grid ? Mathf.CeilToInt(rows.Count / (float)cols) * (tileH + tileGap) : rows.Count * rowHeight;
            scroll = ScrollPanel.Begin(listArea, scroll, new Rect(0, 0, 1005, Mathf.Max(listArea.height - 5, contentHeight)));
            for (int index = 0; index < rows.Count && grid; index++)
            {
                RunBuildReadModel.Row row = rows[index];
                Rect tile = new Rect(5 + index % cols * (tileW + tileGap), 5 + index / cols * (tileH + tileGap), tileW, tileH);
                RunHud.Fill(tile, index == selection ? ArcadeSkin.CardLit : ArcadeSkin.Card);
                // requirement 25/10/2026: every friend and relic says its rarity in words (the border colour alone was hard to read)
                ItemRarity? shownRarity = tab == 2 ? row.Rarity : tab == 1 && !string.IsNullOrEmpty(row.UnitId) ? owner.Content.Unit(row.UnitId)?.Rarity : null;
                if (shownRarity is ItemRarity tileRarity) { RarityDescriptionFrame.Edge(tile, tileRarity, Time.unscaledTime); RarityDescriptionFrame.Backdrop(new Rect(tile.x + 38, tile.y + 16, 110, 100), tileRarity, Time.unscaledTime); }
                else if (index == selection) ArcadeSkin.Line(new Rect(tile.x, tile.yMax - 5, tile.width, 5), ArcadeSkin.Accent);
                if (row.Icon != null) RunHud.Sprite(new Rect(tile.x + 38, tile.y + 16, 110, 100), row.Icon);
                if (row.WeaponIcon != null) RunHud.Sprite(new Rect(tile.xMax - 62, tile.y + 80, 46, 46), row.WeaponIcon);
                if (tab == 1 && row.Summary != null && row.Summary.StartsWith("Stars ") && float.TryParse(row.Summary.Substring(6, 3).Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float starsShown))
                {
                    int whole = Mathf.FloorToInt(starsShown); bool half = starsShown - whole > .25f;
                    for (int st = 0; st < whole + (half ? 1 : 0); st++)
                    {
                        Vector2 sp = new Vector2(tile.x + 22 + st * 20, tile.y + 22); bool halfStar = half && st == whole; // half a star: a smaller, paler diamond
                        ArcadeSkin.Diamond(sp, halfStar ? 14 : 18, new Color(.1f, .06f, .04f, .9f)); ArcadeSkin.Diamond(sp, halfStar ? 8 : 13, halfStar ? new Color(1f, .86f, .5f, .7f) : new Color(1f, .8f, .3f));
                    }
                }
                tileName ??= ArcadeTypography.Style(22, true); tileName.alignment = TextAnchor.MiddleCenter; tileName.wordWrap = true; tileName.normal.textColor = ArcadeSkin.Paper;
                GUI.Label(new Rect(tile.x + 12, tile.y + 128, tile.width - 24, 56), row.Name, tileName);
                if (shownRarity is ItemRarity tagRarity)
                {
                    Rect rarityTag = new Rect(tile.xMax - 128, tile.y + 8, 120, 28);
                    ArcadeSkin.Slant(rarityTag, RunModifierRules.RarityColor(tagRarity));
                    rarityStyle ??= new GUIStyle(ArcadeTypography.Style(20, true)) { alignment = TextAnchor.MiddleCenter, wordWrap = false };
                    rarityStyle.normal.textColor = ArcadeSkin.Ink;
                    GUI.Label(rarityTag, RunModifierRules.RarityName(tagRarity).ToUpperInvariant(), rarityStyle);
                }
                if (TouchTargets.Button(tile, GUIContent.none, GUIStyle.none)) { selection = index; selectedAt = Time.unscaledTime; explanationScroll = Vector2.zero; }
            }
            for (int index = 0; index < rows.Count && !grid; index++)
            {
                RunBuildReadModel.Row row = rows[index];
                Rect card = new Rect(5, index * rowHeight + 5, 990, rowHeight - 12);
                RunHud.Fill(card, index == selection ? ArcadeSkin.CardLit : ArcadeSkin.Card);
                if (index == selection) ArcadeSkin.Line(new Rect(card.x + 2, card.y + 2, 4, card.height - 4), ArcadeSkin.Accent);
                if (tab == 2)
                {
                    ArcadeSkin.Line(new Rect(card.x + 2, card.y + 4, 5, card.height - 8), row.Accent);
                    RunHud.Fill(new Rect(18, card.y + 12, 90, 100), ArcadeSkin.Deep);
                    if (row.Rarity is ItemRarity rarity) // each relic framed by its rarity, like the shop's cards (requirement 23/10/2026)
                    { RarityDescriptionFrame.Edge(card, rarity, Time.unscaledTime); RarityDescriptionFrame.Backdrop(new Rect(18, card.y + 12, 90, 100), rarity, Time.unscaledTime); }
                }
                float contentLeft = row.Icon != null ? 130 : 28;
                if (row.Icon != null) RunHud.Sprite(new Rect(25, card.y + 20, 75, 80), row.Icon);
                if (row.WeaponIcon != null) RunHud.Sprite(new Rect(68, card.y + 62, 42, 48), row.WeaponIcon);
                GlossaryUI.Text(new Rect(contentLeft, card.y + 12, 820, 48), row.Name, text);
                GUI.Label(new Rect(contentLeft, card.y + 58, 820, rowHeight - 65), row.Summary, tab == 0 ? number : text);
                if (TouchTargets.Button(card, GUIContent.none, GUIStyle.none)) { selection = index; selectedAt = Time.unscaledTime; explanationScroll = Vector2.zero; }
            }
            if (rows.Count == 0) GUI.Label(new Rect(25, 35, 900, 120), tab == 1 ? "No friends yet. Recruit from shops at rest." : tab == 2 ? "No relics yet. Buy them from shops at rest." : "No links to inspect yet.", text);
            ScrollPanel.End();
            RunHud.Fill(new Rect(1160, 300, 665, 665), ArcadeSkin.Card);
            ArcadeSkin.Line(new Rect(1160, 300, 5, 665), Color.Lerp(ArcadeSkin.Accent, ArcadeSkin.Teal, LivingUI.Reveal(Time.unscaledTime - selectedAt, PresentationSettings.ReducedMotion)));
            if (tab == 2 && rows.Count > 0)
            {
                var chosen = rows[Mathf.Clamp(selection, 0, rows.Count - 1)];
                RunHud.Sprite(new Rect(1185, 325, 100, 100), chosen.Icon);
                GUI.Label(new Rect(1310, 340, 480, 70), chosen.Name, text);
            }
            string explanation = rows.Count == 0 ? "" : rows[Mathf.Clamp(selection, 0, rows.Count - 1)].Explanation;
            float quoteHeight = 0;
            if (tab == 1 && rows.Count > 0)
            {
                // B3: the companion's own words, in their hand, above the facts
                string said = CompanionVoice.Line(rows[Mathf.Clamp(selection, 0, rows.Count - 1)].UnitId, owner.Session.State.Wave, owner.Session.State);
                if (!string.IsNullOrEmpty(said))
                {
                    if (handStyle == null) { handStyle = ArcadeTypography.Style(30); handStyle.font = ArcadeTypography.Hand; handStyle.normal.textColor = ArcadeSkin.Paper; handStyle.wordWrap = true; } // wraps inside the panel, never runs off screen
                    quoteHeight = Mathf.Min(150, handStyle.CalcHeight(new GUIContent("“" + said + "”"), 590) + 18);
                    GUI.Label(new Rect(1190, 318, 600, quoteHeight), "“" + said + "”", handStyle);
                    ArcadeSkin.Divider(new Rect(1190, 318 + quoteHeight, 590, 1), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.4f));
                    quoteHeight += 12;
                }
            }
            if (tab == 2) explanation += "\nRECENT RECEIPTS\n\n" + (string.IsNullOrEmpty(model.Receipts) ? "No saved transactions yet." : model.Receipts);
            bool heroSelected = tab == 0 && selection == 0 && owner.Session.HeroIdentityDefinition != null;
            if (heroSelected) explanation += "\n\n" + HeroIdentityOptions.Details(owner); // one scroll for the whole column
            float explanationHeight = Mathf.Max(620, GlossaryUI.Height(explanation, text, 570) + 30);
            explanationScroll = ScrollPanel.Begin(new Rect(1180, (tab == 2 ? 445 : 325) + quoteHeight, 625, (heroSelected ? 430 : tab == 2 ? 490 : 610) - quoteHeight), explanationScroll, new Rect(0, 0, 585, explanationHeight));
            GlossaryUI.Text(new Rect(5, 5, 570, explanationHeight), explanation, text);
            ScrollPanel.End();
            if (heroSelected) heroOptions.Draw(owner, new Rect(1185, 770, 610, 177), text);
            GlossaryUI.Draw(new Vector2(1920, 1080));
            GUI.matrix = previous;
            GUI.color = previousTint;
            GUI.skin = previousSkin;
        }
    }
}
