using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Waiting Room's last five stations (Postbox, Museum, Night Pact, Window, Bell; 21/10/2026) and the panels'
    /// keys / stick support: the focus moves over every card, crest, letter, shelf and pact (arrows, WASD, stick), Enter /
    /// Space / A confirms, Esc / B closes (RunMenu), Q / R or LB / RB page the Museum's shelves.</summary>
    public sealed partial class MetaHub
    {
        // ---------------------------------------------------------------- panel focus (keys and stick)
        private int navIndex, navCount, navCols = 1, navLastCount;
        private bool navConfirm, navKeys, navMoved, navInScroll;
        private float navRepeat;
        private Rect? navReveal;
        private HubAction panelAction;
        /// <summary>The focused control of the open panel (diagnostics), -1 when the focus is hidden (mouse / touch).</summary>
        public int PanelFocus => navKeys ? navIndex : -1;

        private void PanelInput()
        {
            int dx = 0, dy = 0;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) dx = -1;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) dx = 1;
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) dy = -1;
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) dy = 1;
            Vector2 pad = ArcadeNavigation.Pad;
            if (pad.sqrMagnitude < .16f) navRepeat = 0;
            else if (Time.unscaledTime >= navRepeat && pad.sqrMagnitude > .36f)
            {
                if (Mathf.Abs(pad.x) > Mathf.Abs(pad.y)) dx = pad.x > 0 ? 1 : -1; else dy = pad.y > 0 ? -1 : 1;
                navRepeat = Time.unscaledTime + .2f;
            }
            if (dx != 0 || dy != 0) MoveFocus(dy != 0 ? dy * Mathf.Max(1, navCols) : dx);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0))
            { if (navKeys) navConfirm = true; navKeys = true; }
            if (Open == MetaStation.Museum)
            {
                int page = Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.JoystickButton4) ? -1 : Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton5) ? 1 : 0;
                if (page != 0) ShowShelf((museumShelf + page + ShelfTabs) % ShelfTabs);
            }
        }
        /// <summary>Moves the panel focus (the first move only shows it).</summary>
        public void MoveFocus(int step)
        {
            if (!navKeys) navKeys = true;
            else navIndex = Mathf.Clamp(navIndex + step, 0, Mathf.Max(0, navLastCount - 1));
            navMoved = true; CheckpointAudio.PlayInterfaceId("ui.hover");
        }
        /// <summary>Diagnostics / keys: confirms the focused control on the next frame.</summary>
        public void ConfirmFocus() { navKeys = true; navConfirm = true; }

        private void NavBegin() { navCount = 0; navInScroll = false; }
        private void NavEnd()
        {
            if (Event.current.type != EventType.Repaint) return;
            navLastCount = navCount;
            if (navIndex >= navCount) navIndex = Mathf.Max(0, navCount - 1);
            navConfirm = false; navMoved = false;
        }
        /// <summary>Registers one focusable control; true when keys confirmed it this frame.</summary>
        private bool Nav(Rect r, bool enabled = true)
        {
            int index = navCount++;
            if (!navKeys || index != navIndex) return false;
            ArcadeSkin.Focus(r);
            if (Event.current.type == EventType.Repaint)
            {
                // a clear gold rim round the focused control (the kit's focus mark alone is small on big cards)
                Color rim = new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .9f);
                ArcadeSkin.Line(new Rect(r.x - 3, r.y - 3, r.width + 6, 3), rim); ArcadeSkin.Line(new Rect(r.x - 3, r.yMax, r.width + 6, 3), rim);
                ArcadeSkin.Line(new Rect(r.x - 3, r.y, 3, r.height), rim); ArcadeSkin.Line(new Rect(r.xMax, r.y, 3, r.height), rim);
            }
            if (navMoved && navInScroll) navReveal = r;
            if (navConfirm && enabled && Event.current.type == EventType.Repaint) { navConfirm = false; return true; }
            return false;
        }
        private void ScrollBegin(Rect view, float height)
        {
            scroll = ScrollPanel.Begin(view, scroll, new Rect(0, 0, view.width - 14, Mathf.Max(view.height, height)));
            navInScroll = true;
        }
        private void ScrollEnd(Rect view)
        {
            ScrollPanel.End();
            navInScroll = false;
            if (navReveal.HasValue) { Rect f = navReveal.Value; scroll.y = Mathf.Max(0, Mathf.Clamp(scroll.y, f.yMax - view.height + 10, f.y - 10)); navReveal = null; }
        }

        private GUIStyle hand, star;
        private void StationStyles()
        {
            if (hand != null) return;
            hand = ArcadeTypography.Style(23); if (ArcadeTypography.Hand != null) hand.font = ArcadeTypography.Hand; hand.wordWrap = true;
            star = ArcadeTypography.Style(48, true); star.alignment = TextAnchor.MiddleCenter; star.wordWrap = false;
        }
        private void Flash(Rect r, string id)
        {
            if (bought == id && Time.unscaledTime - boughtAt < .6f)
                ArcadeSkin.Glow(new Rect(r.x - 12, r.y - 12, r.width + 24, r.height + 24), new Color(1f, .85f, .5f, .5f * (1 - (Time.unscaledTime - boughtAt) / .6f)));
        }
        private void Ink(Color ink) { body.normal.textColor = ink; small.normal.textColor = new Color(ink.r, ink.g, ink.b, .85f); number.normal.textColor = ink; }

        /// <summary>The Bell's stars under the room's title plaque.</summary>
        private void FavourStars(PlayerProfile profile, Rect r)
        {
            int favour = profile.Meta.Favour; if (favour <= 0) return;
            chip.normal.textColor = ArcadeSkin.Gold;
            GUI.Label(r, "FAVOUR " + new string('★', favour), chip);
        }

        private void DrawStation(MetaStation station, Rect area, RunCatalog catalog, PlayerProfile profile, int heroIndex, Func<bool> save)
        {
            StationStyles();
            switch (station)
            {
                case MetaStation.Postbox: DrawPostbox(area, catalog, profile, save); break;
                case MetaStation.Museum: DrawMuseum(area, catalog, profile); break;
                case MetaStation.NightPact: DrawPacts(area, catalog, profile, heroIndex, save); break;
                // the two short pages sit in the middle of the panel, not stuck to its top with the lower half empty
                case MetaStation.Window: DrawWindow(new Rect(area.x, area.y + 56, area.width, area.height - 56), catalog, profile); break;
                case MetaStation.Bell: DrawBell(new Rect(area.x, area.y + 30, area.width, area.height - 30), profile, save); break;
            }
        }

        // ---------------------------------------------------------------- the Postbox
        private int letterOpen = -1, letterTab, letterPage;
        private void DrawPostbox(Rect area, RunCatalog catalog, PlayerProfile profile, Func<bool> save)
        {
            navCols = 1;
            MetaLetter[] letters = MetaProgression.Letters;
            if (letterOpen < 0) letterOpen = Array.FindIndex(letters, l => MetaProgression.LetterRead(profile, l));
            const float gap = 16, listW = 320, rowH = 74;
            // review 31/10/2026 (six letters squeezed into the panel with no room): two tabs, the letters you have and the ones
            // still sealed, each list with room between its letters (four at most a page; arrows turn longer lists)
            var have = new List<int>(); var sealedOnes = new List<int>();
            for (int i = 0; i < letters.Length; i++) (MetaProgression.LetterEarned(profile, letters[i]) ? have : sealedOnes).Add(i);
            if (have.Count == 0) letterTab = 1;
            Rect tabMine = new Rect(area.x, area.y, listW * .5f - 4, 44), tabSealed = new Rect(area.x + listW * .5f + 4, area.y, listW * .5f - 4, 44);
            if (ArcadeSkin.Tab(tabMine, "Letters " + have.Count, letterTab == 0, have.Count > 0, small) && have.Count > 0) { letterTab = 0; letterPage = 0; }
            if (ArcadeSkin.Tab(tabSealed, "Sealed " + sealedOnes.Count, letterTab == 1, sealedOnes.Count > 0, small) && sealedOnes.Count > 0) { letterTab = 1; letterPage = 0; }
            List<int> listed = letterTab == 0 ? have : sealedOnes;
            const int perPage = 4;
            int pages = Mathf.Max(1, (listed.Count + perPage - 1) / perPage); letterPage = Mathf.Clamp(letterPage, 0, pages - 1);
            float listTop = area.y + 62;
            if (pages > 1)
            {
                Rect up = new Rect(area.x + listW * .5f - 76, area.yMax - 52, 48, 48), down = new Rect(area.x + listW * .5f + 28, area.yMax - 52, 48, 48);
                if (ArcadeSkin.IconButton(up, KitRound.Walnut, "◀")) letterPage = (letterPage + pages - 1) % pages;
                if (ArcadeSkin.IconButton(down, KitRound.Walnut, "▶")) letterPage = (letterPage + 1) % pages;
            }
            for (int k = letterPage * perPage; k < Mathf.Min(listed.Count, (letterPage + 1) * perPage); k++)
            {
                int i = listed[k];
                MetaLetter l = letters[i];
                bool earned = MetaProgression.LetterEarned(profile, l), read = MetaProgression.LetterRead(profile, l), open = letterOpen == i;
                Rect row = new Rect(area.x, listTop + (k - letterPage * perPage) * (rowH + gap), listW, rowH);
                Flash(row, l.Id);
                // the open letter glows warm behind its paper (never a rectangle drawn over the painted shape)
                if (open) ArcadeSkin.Glow(new Rect(row.x - 16, row.y - 14, row.width + 32, row.height + 28), new Color(1f, .82f, .45f, .4f + .1f * Mathf.Sin(Time.unscaledTime * 2.4f)));
                if (!(earned && ArcadeSkin.PaperArt(row, read ? PaperKind.LetterRead : PaperKind.LetterUnread))) // a painted letter (its wax seal under the icon)
                    ArcadeSkin.CodePlate(row, read ? CodeTone.Cream : earned ? CodeTone.Gold : CodeTone.Walnut, open ? 1 : earned ? 0 : 3);
                ItemDefinition relic = catalog.Item(l.Relic);
                // the relic (or the postbox) sits on the painted wax seal, centred on it, inside the paper
                Vector2 seal = new Vector2(row.x + row.width * .2f, row.y + row.height * .47f);
                if (!earned) ArcadeSkin.Lock(seal, 30, ArcadeSkin.KitCream); // still sealed: a lock, never a stand-in picture
                else RunHud.Sprite(new Rect(seal.x - 24, seal.y - 24, 48, 48), read && relic?.Icon != null ? relic.Icon : MetaIcons.Station(MetaStation.Postbox, true));
                Ink(earned ? ArcadeSkin.KitInk : ArcadeSkin.KitCream);
                // the words start past the wax seal and stop before the folded corner; each line gets its full height
                float past = seal.x - row.x + 52; // clear of the seal's right edge (the painted seal is about 90 px across)
                Rect words = new Rect(row.x + past, row.y + 9, row.width - past - 66, 56); // two lines; the folded corner takes the last 60 px
                Color words2 = earned ? ArcadeSkin.KitInk : ArcadeSkin.KitCream; // cream on the dark walnut plate of a sealed letter
                ArcadeSkin.KitCaption(new Rect(words.x, words.y, words.width, 30), earned ? l.Name : "Sealed", words2, body);
                ArcadeSkin.KitCaption(new Rect(words.x, words.y + 30, words.width, 26), !earned ? l.Milestone : read ? "Keeps " + (relic?.Name ?? "a relic") : "NEW", words2, small);
                if ((GUI.Button(row, GUIContent.none, GUIStyle.none) | Nav(row, earned)) && earned)
                {
                    letterOpen = i;
                    if (!read && MetaProgression.ReadLetter(profile, l.Id))
                    {
                        if (!save()) { profile.Meta.Letters.Remove(l.Id); CheckpointAudio.PlayInterfaceId("ui.error"); }
                        else { bought = l.Id; boughtAt = Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.open"); }
                    }
                    else CheckpointAudio.PlayInterfaceId("ui.select");
                }
            }
            Rect paper = new Rect(area.x + listW + 18, area.y, area.width - listW - 18, area.height - 4);
            ArcadeSkin.Frame(paper, KitFrame.Card, .3f);
            Rect pc = ArcadeSkin.Content(paper, KitFrame.Card, true, .3f);
            Color ink = ArcadeSkin.FrameText(KitFrame.Card);
            Ink(ink); hand.normal.textColor = ink;
            MetaLetter shown = letterOpen >= 0 && letterOpen < letters.Length && MetaProgression.LetterEarned(profile, letters[letterOpen]) ? letters[letterOpen] : null;
            if (shown == null)
            {
                hand.alignment = TextAnchor.MiddleCenter;
                GUI.Label(pc, Array.Exists(letters, l => MetaProgression.LetterEarned(profile, l)) ? "A letter is waiting." : "No letters yet.\nThe first comes after a boss.", hand);
                hand.alignment = TextAnchor.UpperLeft; return;
            }
            body.alignment = TextAnchor.UpperCenter; GUI.Label(new Rect(pc.x, pc.y, pc.width, 28), RunUICopy.Title(shown.Name), body); body.alignment = TextAnchor.UpperLeft; // the letter's title centred over it
            GUI.Label(new Rect(pc.x, pc.y + 38, pc.width, pc.height - 190), shown.Text, hand);
            hand.alignment = TextAnchor.UpperRight; GUI.Label(new Rect(pc.x, pc.yMax - 160, pc.width, 32), "— Thursday", hand); hand.alignment = TextAnchor.UpperLeft;
            ItemDefinition keep = catalog.Item(shown.Relic);
            Rect stand = new Rect(pc.x, pc.yMax - 112, 112, 96); // narrower, so the words beside it keep two full lines
            ArcadeSkin.Pedestal(stand, Time.unscaledTime);
            if (keep?.Icon != null) RunHud.Sprite(ArcadeSkin.PedestalSeat(stand), keep.Icon);
            GUI.Label(new Rect(stand.xMax + 14, stand.y + 14, pc.xMax - stand.xMax - 14, 28), keep?.Name ?? "", body);
            GUI.Label(new Rect(stand.xMax + 14, stand.y + 46, pc.xMax - stand.xMax - 14, 50), "Bring it from hero select", small);
        }

        // ---------------------------------------------------------------- the Museum
        private const int ShelfTabs = 6;
        private int museumShelf, museumPick = -1;
        private void ShowShelf(int shelf)
        {
            if (shelf == museumShelf) return;
            museumShelf = shelf; museumPick = -1; scroll = Vector2.zero; navIndex = shelf; CheckpointAudio.PlayInterfaceId("ui.select");
        }
        private void DrawMuseum(Rect area, RunCatalog catalog, PlayerProfile profile)
        {
            // the painted tab's end caps hang outside its bar: tabs sit 30 apart and in from the edges (requirement 23/10/2026);
            // two rows of three, wide enough for each name at full size (six in one row squeezed them unreadable)
            const float tabGap = 30, tabInset = 16, perRow = 3;
            float tw = (area.width - 2 * tabInset - (perRow - 1) * tabGap) / perRow;
            for (int t = 0; t < ShelfTabs; t++)
            {
                Rect tab = new Rect(area.x + tabInset + t % 3 * (tw + tabGap), area.y + t / 3 * 46, tw, 40);
                if (ArcadeSkin.Tab(tab, t < 5 ? MetaProgression.ShelfNames[t] : "TROPHIES", museumShelf == t, true, chip) | Nav(tab)) ShowShelf(t);
            }
            int found = MetaProgression.Discoveries(profile, catalog);
            var (damage, gold) = MetaProgression.MuseumGifts(profile, catalog);
            Rect head = new Rect(area.x, area.y + 92, area.width, 32);
            Ink(ArcadeSkin.KitCream);
            List<MuseumPiece> pieces = museumShelf < 5 ? MetaProgression.Shelf(profile, catalog, (MuseumShelf)museumShelf) : null;
            string left = pieces != null && museumPick >= 0 && museumPick < pieces.Count
                ? (pieces[museumPick].Seen ? pieces[museumPick].Name : "Not found yet")
                : found + " found · next gift at " + (found / MetaProgression.MuseumStep + 1) * MetaProgression.MuseumStep;
            GUI.Label(new Rect(head.x, head.y + 4, head.width - 230, 26), left, body);
            Rect gift = new Rect(head.xMax - 220, head.y, 28, 28);
            if (!PaintedStatIcons.TryDraw(gift, ItemEffect.Damage)) RunHud.Sprite(gift, MetaIcons.Station(MetaStation.Museum, true));
            GUI.Label(new Rect(gift.xMax + 4, head.y + 2, 70, 28), "+" + damage + "%", number);
            RunHud.Sprite(new Rect(head.xMax - 110, head.y, 28, 28), CurrencyIcons.Coin);
            GUI.Label(new Rect(head.xMax - 78, head.y + 2, 70, 28), "+" + gold + "%", number);
            Rect view = new Rect(area.x, area.y + 130, area.width, area.height - 130 - 64);
            // every find in full (pictures, poses, numbers, trying a weapon) is in the Collection
            Rect browse = new Rect(area.center.x - 190, area.yMax - 56, 380, 52);
            if (ArcadeSkin.Button(browse, "OPEN THE COLLECTION", KitButton.Secondary, true, body) | Nav(browse)) { panelAction = HubAction.Collection; CheckpointAudio.PlayInterfaceId("ui.confirm"); }
            if (pieces != null)
            {
                const float cell = 66, gap = 8;
                int cols = Mathf.Max(1, Mathf.FloorToInt((view.width - 14 + gap) / (cell + gap)));
                navCols = cols;
                ScrollBegin(view, Mathf.CeilToInt(pieces.Count / (float)cols) * (cell + gap));
                for (int i = 0; i < pieces.Count; i++)
                {
                    MuseumPiece piece = pieces[i];
                    Rect c = new Rect(i % cols * (cell + gap), i / cols * (cell + gap), cell, cell);
                    ArcadeSkin.PortraitSlot(c, museumPick == i);
                    Color keep = GUI.color;
                    if (!piece.Seen) GUI.color = new Color(0, 0, 0, keep.a * .82f); // a silhouette until found
                    if (piece.Second != null) { RunHud.Sprite(new Rect(c.x + 4, c.y + 6, 38, 38), piece.Icon); RunHud.Sprite(new Rect(c.x + 24, c.y + 22, 38, 38), piece.Second); }
                    else RunHud.Sprite(new Rect(c.x + 7, c.y + 7, cell - 14, cell - 14), piece.Icon);
                    GUI.color = keep;
                    if (!piece.Seen) ArcadeSkin.Lock(new Vector2(c.center.x, c.center.y + 4), 24, new Color(.86f, .7f, .42f, .9f)); // a lock on what is not found yet
                    if (GUI.Button(c, GUIContent.none, GUIStyle.none) | Nav(c)) { museumPick = i; CheckpointAudio.PlayInterfaceId("ui.select"); }
                }
                ScrollEnd(view);
                return;
            }
            navCols = 1;
            List<(string name, string line, bool won)> trophies = MetaProgression.Trophies(profile, catalog);
            ScrollBegin(view, trophies.Count * 64);
            for (int i = 0; i < trophies.Count; i++)
            {
                var (name, line, won) = trophies[i];
                Rect r = new Rect(0, i * 64, view.width - 20, 56);
                ArcadeSkin.CodePlate(r, won ? CodeTone.Gold : CodeTone.Walnut, won ? 0 : 3);
                Color keep = GUI.color; if (!won) GUI.color = new Color(0, 0, 0, keep.a * .7f);
                RunHud.Sprite(new Rect(r.x + 10, r.y + 6, 44, 44), MetaIcons.Station(MetaStation.Museum, won));
                GUI.color = keep;
                if (!won) ArcadeSkin.Lock(new Vector2(r.x + 32, r.y + 30), 22, new Color(.86f, .7f, .42f, .9f));
                Ink(won ? ArcadeSkin.KitInk : ArcadeSkin.KitCream);
                GUI.Label(new Rect(r.x + 64, r.y + 4, r.width - 74, 26), name, body);
                GUI.Label(new Rect(r.x + 64, r.y + 28, r.width - 74, 24), won ? line : line + " to earn it", small);
                Nav(r, false);
            }
            ScrollEnd(view);
        }

        // ---------------------------------------------------------------- the Night Pact
        private void DrawPacts(Rect area, RunCatalog catalog, PlayerProfile profile, int heroIndex, Func<bool> save)
        {
            navCols = 1;
            int mask = profile.Meta.Pacts;
            Rect sum = new Rect(area.x, area.y, area.width, 44);
            ArcadeSkin.CodePlate(sum, CodeTone.Navy);
            RunHud.Sprite(new Rect(sum.x + 18, sum.y + 6, 32, 32), MetaIcons.LampOil);
            RunHud.Sprite(new Rect(sum.x + 52, sum.y + 6, 32, 32), MetaIcons.Keepsake);
            Ink(ArcadeSkin.KitCream);
            GUI.Label(new Rect(sum.x + 92, sum.y + 8, 120, 28), "×" + MetaProgression.PactMultiplier(mask).ToString("0.00"), number);
            UnitDefinition traveller = catalog.Champions[Mathf.Clamp(heroIndex, 0, catalog.Champions.Length - 1)];
            MetaScore record = MetaProgression.PactRecord(profile, traveller.Id);
            small.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(sum.x + 200, sum.y, sum.width - 216, sum.height), RunUICopy.UnitName(traveller) + " · " + (record != null ? "best night " + record.Score : "no pact run yet"), small);
            small.alignment = TextAnchor.UpperLeft;
            for (int i = 0; i < MetaProgression.Pacts.Length; i++)
            {
                MetaPact pact = MetaProgression.Pacts[i];
                bool on = MetaProgression.PactOn(mask, i);
                // a clear gap between rows so each plate's ornaments never touch the next one, and the words clear of its rim
                Rect r = new Rect(area.x, area.y + 56 + i * 82, area.width, 68);
                Flash(r, pact.Id);
                ArcadeSkin.CodePlate(r, on ? CodeTone.Oxblood : CodeTone.Walnut, on ? 1 : 0);
                // a chosen pact glows round its round painted seal, sparks circling it (requirement 24/10/2026: no rectangle round round art)
                Rect seal = new Rect(r.x + 18, r.y + 9, 50, 50);
                if (on)
                {
                    float tt = PresentationSettings.ReducedMotion ? 0 : Time.unscaledTime;
                    ArcadeSkin.Glow(new Rect(seal.x - 18, seal.y - 18, seal.width + 36, seal.height + 36), new Color(1f, .5f, .38f, .55f + .15f * Mathf.Sin(tt * 3 + i)));
                    for (int k = 0; k < 3; k++) { float an = (tt * 140 + k * 120 + i * 40) * Mathf.Deg2Rad; Color sp = new Color(1f, .72f, .5f, .9f); ArcadeSkin.Diamond(seal.center + new Vector2(Mathf.Cos(an), Mathf.Sin(an)) * 31, 7, sp); }
                }
                PactIcon(seal, i, catalog);
                Ink(ArcadeSkin.KitCream);
                GUI.Label(new Rect(r.x + 86, r.y + 12, r.width - 270, 26), pact.Name, body);
                GUI.Label(new Rect(r.x + 86, r.y + 38, r.width - 270, 24), pact.Line, small);
                GUI.Label(new Rect(r.xMax - 172, r.y + 20, 90, 28), "×" + pact.Reward.ToString("0.00"), number);
                ArcadeSkin.CheckBox(new Rect(r.xMax - 66, r.y + 15, 38, 38), on);
                if (GUI.Button(r, GUIContent.none, GUIStyle.none) | Nav(r))
                {
                    MetaProgression.TogglePact(profile, i);
                    if (!save()) { profile.Meta.Pacts = mask; CheckpointAudio.PlayInterfaceId("ui.error"); }
                    else { bought = pact.Id; boughtAt = Time.unscaledTime; CheckpointAudio.PlayInterfaceId(on ? "ui.back" : "ui.confirm"); }
                    mask = profile.Meta.Pacts;
                }
            }
        }
        private static void PactIcon(Rect r, int pact, RunCatalog catalog)
        {
            switch (pact)
            {
                case MetaProgression.PactSwift: if (PaintedStatIcons.TryDraw(r, ItemEffect.EnemySpeed) || PaintedStatIcons.TryDraw(r, ItemEffect.MoveSpeed)) return; break;
                case MetaProgression.PactBareShelf: RunHud.Sprite(r, MetaIcons.Station(MetaStation.Ledger, false)); return;
                case MetaProgression.PactNoDream: if (CurrencyIcons.Egg != null) { RunHud.Sprite(r, CurrencyIcons.Egg); return; } break;
                case MetaProgression.PactElites: if (PaintedStatIcons.TryDraw(r, ItemEffect.EnemyHealth)) return; break;
                case MetaProgression.PactEnrage: if (catalog.Bosses != null && catalog.Bosses.Length > 0 && catalog.Bosses[0]?.Body != null) { RunHud.Sprite(r, catalog.Bosses[0].Body); return; } break;
            }
            RunHud.Sprite(r, MetaIcons.Station(MetaStation.NightPact, true));
        }

        // ---------------------------------------------------------------- the Window (Daily Dream)
        private void DrawWindow(Rect area, RunCatalog catalog, PlayerProfile profile)
        {
            navCols = 1;
            string day = MetaProgression.Today;
            UnitDefinition hero = MetaProgression.DailyHero(profile, catalog, day);
            int pact = MetaProgression.DailyPact(day);
            MetaScore best = MetaProgression.DailyRecord(profile, day);
            Rect card = new Rect(area.x, area.y, area.width, 252);
            ArcadeSkin.Frame(card, KitFrame.Card, .3f);
            Ink(ArcadeSkin.FrameText(KitFrame.Card));
            body.alignment = TextAnchor.UpperCenter; GUI.Label(new Rect(card.x + 24, card.y + 16, card.width - 48, 28), "Tonight's dream · " + day, body); body.alignment = TextAnchor.UpperLeft; // a card's title sits in its middle
            if (hero != null)
            {
                ArcadeSkin.Glow(new Rect(card.x + 4, card.y + 40, 170, 190), new Color(1f, .8f, .45f, .22f));
                RunHud.Sprite(new Rect(card.x + 24, card.y + 56, 130, 150), hero.Body);
            }
            float x = card.x + 180, w = card.xMax - x - 24;
            GUI.Label(new Rect(x, card.y + 54, w, 28), hero != null ? RunUICopy.UnitName(hero) : "No hero yet", body);
            GUI.Label(new Rect(x, card.y + 82, w, 24), "Peaceful · same night for everyone", small);
            PactIcon(new Rect(x, card.y + 112, 40, 40), pact, catalog);
            GUI.Label(new Rect(x + 50, card.y + 106, w - 50, 32), MetaProgression.Pacts[pact].Name, body); // tall enough for the body face's ascenders
            GUI.Label(new Rect(x + 50, card.y + 138, w - 50, 24), MetaProgression.Pacts[pact].Line, small);
            RunHud.Sprite(new Rect(x + 4, card.y + 168, 32, 32), MetaIcons.Keepsake);
            GUI.Label(new Rect(x + 50, card.y + 172, w - 50, 26), MetaProgression.DailyPaid(profile, day) ? "Paid today" : "+" + MetaProgression.DailyKeepsakes + " the first dream today", number);
            GUI.Label(new Rect(x + 50, card.y + 206, w - 50, 26), best != null ? "Best today: night " + best.Score : "No dream yet today", small);
            Rect go = new Rect(area.center.x - 170, card.yMax + 16, 340, 58);
            bool can = hero != null;
            if ((ArcadeSkin.Button(go, "DREAM TONIGHT ▶", KitButton.Primary, can, body) | Nav(go, can)) && can) { panelAction = HubAction.Daily; CheckpointAudio.PlayInterfaceId("ui.confirm"); }
            Ink(ArcadeSkin.KitCream);
            float y = go.yMax + 16; int shownDays = 0;
            foreach (MetaScore past in profile.Meta.DailyBest)
            {
                if (past.Key == day || shownDays >= 3) continue;
                GUI.Label(new Rect(area.x + 40, y, area.width - 80, 26), past.Key + "   " + "night " + past.Score, small);
                y += 28; shownDays++;
            }
        }

        // ---------------------------------------------------------------- the Bell (Keeper's favour)
        private bool bellArmed;
        private void DrawBell(Rect area, PlayerProfile profile, Func<bool> save)
        {
            navCols = 1;
            int favour = profile.Meta.Favour;
            for (int i = 0; i < MetaProgression.FavourMax; i++)
            {
                Rect s = new Rect(area.center.x - MetaProgression.FavourMax * 36 + i * 72, area.y + 6, 72, 64);
                bool lit = i < favour;
                if (lit) ArcadeSkin.Glow(new Rect(s.x - 10, s.y - 10, s.width + 20, s.height + 20), new Color(1f, .8f, .4f, .4f));
                star.normal.textColor = lit ? ArcadeSkin.Gold : new Color(.5f, .45f, .4f, .55f);
                GUI.Label(s, "★", star);
            }
            Ink(ArcadeSkin.KitCream);
            number.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(area.x, area.y + 78, area.width, 28), "+" + Mathf.RoundToInt(favour * MetaProgression.FavourOil * 100) + "% Lamp Oil, for good", number);
            number.alignment = TextAnchor.UpperLeft;
            small.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(area.x, area.y + 108, area.width, 24), "Each star: +10% Lamp Oil", small);
            small.alignment = TextAnchor.UpperLeft;
            Rect req = new Rect(area.x + 60, area.y + 146, area.width - 120, 150);
            ArcadeSkin.Frame(req, KitFrame.Card, .3f);
            Ink(ArcadeSkin.FrameText(KitFrame.Card));
            var (have, need) = MetaProgression.BellRanks(profile);
            RunHud.Sprite(new Rect(req.x + 22, req.y + 20, 40, 40), MetaIcons.Station(MetaStation.Window, profile.Wins > 0));
            GUI.Label(new Rect(req.x + 74, req.y + 26, req.width - 230, 28), "See the dawn", body);
            GUI.Label(new Rect(req.xMax - 150, req.y + 26, 128, 28), profile.Wins > 0 ? "DONE" : "—", number);
            RunHud.Sprite(new Rect(req.x + 22, req.y + 70, 40, 40), MetaIcons.Station(MetaStation.Lantern, have >= need));
            GUI.Label(new Rect(req.x + 74, req.y + 76, req.width - 230, 28), "First five branches lit", body);
            GUI.Label(new Rect(req.xMax - 150, req.y + 76, 128, 28), have + " / " + need, number);
            small.normal.textColor = new Color(.55f, .18f, .12f);
            GUI.Label(new Rect(req.x + 74, req.y + 112, req.width - 96, 26), "Ringing sets them back to zero", small);
            string blocked = MetaProgression.BellBlocked(profile);
            bool can = blocked == null;
            if (!can) bellArmed = false;
            Rect button = new Rect(area.center.x - 170, req.yMax + 22, 340, 60);
            string caption = blocked == "MAX" ? "MAX" : bellArmed ? "RING · SURE?" : "RING THE BELL";
            if ((ArcadeSkin.Button(button, caption, !can ? KitButton.Secondary : bellArmed ? KitButton.Danger : KitButton.Primary, can, body) | Nav(button, can)) && can)
            {
                if (!bellArmed) { bellArmed = true; CheckpointAudio.PlayInterfaceId("ui.select"); }
                else
                {
                    string before = JsonUtility.ToJson(profile.Meta);
                    bellArmed = false;
                    if (MetaProgression.RingBell(profile))
                    {
                        if (!save()) { JsonUtility.FromJsonOverwrite(before, profile.Meta); CheckpointAudio.PlayInterfaceId("ui.error"); }
                        else CheckpointAudio.PlayInterfaceId("up.star3");
                    }
                }
            }
            if (!can && blocked != "MAX")
            {
                Ink(ArcadeSkin.KitCream); small.alignment = TextAnchor.UpperCenter;
                GUI.Label(new Rect(area.x, button.yMax + 8, area.width, 24), blocked, small);
                small.alignment = TextAnchor.UpperLeft;
            }
        }
    }
}
