using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunHud
    {
        private GUIStyle text;
        private GUIStyle small;
        private GUIStyle title;
        private GUIStyle button;
        private GUIStyle numeric;
        private GUIStyle numericLeft;
        private GUIStyle caption;
        private GUIStyle primary;
        private GUIStyle bossName;
        private GUIStyle tagStyle;
        private GUIStyle handStyle;
        private GUIStyle bondHit;
        private string selectedBond;
        private float bondTooltipUntil;
        private bool receiptOpen;
        private int receiptId;
        private Vector2 receiptScroll;
        private readonly RunInvoice invoice = new RunInvoice();
        private readonly ArcadeNavigation navigation = new ArcadeNavigation();
        private readonly RunBondFeedback bondFeedback = new RunBondFeedback();
        private readonly BondHudSummary bondSummary = new BondHudSummary();
        private readonly StageCutIn cutIn = new StageCutIn();
        private readonly WaveStamp waveStamp = new WaveStamp();
        private readonly FirstRunCoach coach = new FirstRunCoach();
        private readonly PlayerGuide guide = new PlayerGuide();
        public PlayerGuide Guide => guide;
        private readonly System.Collections.Generic.Dictionary<string, int> bondLevels = new System.Collections.Generic.Dictionary<string, int>();
        private string momentsRun;
        private UnitState seenBoss;
        private bool bossWasAlive;
        private int encoreCount = -1;
        private float danger;
        private string previewKey;
        private System.Collections.Generic.List<(string name, string before, string after, bool better)> previewRows = new System.Collections.Generic.List<(string, string, string, bool)>();
        private GUIStyle compareStyle;
        private GUIStyle programStyle;
        private GUIStyle identityStyle;
        private Vector2 shopScroll;
        private bool shopDetails;
        private readonly ShopStatFeedback statFeedback = new ShopStatFeedback();
        public bool ShopDetailsOpen => shopDetails;
        public void SetShopDetails(bool expanded) { shopDetails = expanded; shopScroll = Vector2.zero; }
        private WorldShop.Station tooltipStation;
        private string walletRun;
        private float healthTrail;
        private int lastGold;
        private int lastEggs;
        private float goldChanged = -100;
        private float eggsChanged = -100;
        // the pause menu's page: 0 the menu, 1 settings, 2 "start over?"
        private int pausePage;
        private GUIStyle dangerButton;
        private float terminalAt = -1, resultsAt = -1, gaugeHelpAt = -1;
        private bool lanternHelp; // the help card explains the lantern row instead of the gauge
        private static readonly Rect LanternRow = new Rect(22, 188, 190, 34);
        /// <summary>What the bar under the health bar is: the hero's own move, what fills it, and what happens when it is full.
        /// Opens from the "i" beside it and closes on a second tap, a tap elsewhere or after eight seconds.</summary>
        private void DrawGaugeHelp(ArenaController owner, RunSnapshot state)
        {
            if (gaugeHelpAt < 0) return;
            if (Time.unscaledTime - gaugeHelpAt > 8) { gaugeHelpAt = -1; return; }
            HeroIdentityDefinition identity = HeroIdentities.For(state.ChampionId);
            string heading = lanternHelp ? "THE LANTERNS" : owner.Simulation.HeroIdentityEnabled ? identity.Name : "TRANSFORMATION";
            string copy = lanternHelp ? "Each lantern keeps a memory of someone who came looking for you. Some go dark as the nights pass; the warm chair can light one again."
                : owner.Simulation.HeroIdentityEnabled ? FirstSentences(identity.Description, 2) : "Fills as you fight. When it is full, you transform for a while and hit much harder.";
            Rect card = lanternHelp ? new Rect(230, 180, 600, 156) : new Rect(230, 100, 600, 156);
            Matrix4x4 m = GUI.matrix; Color c = GUI.color;
            LivingUI.Pop(Time.unscaledTime - gaugeHelpAt, card.center);
            ArcadeSkin.Frame(card, KitFrame.Dialogue, .6f);
            Rect inner = ArcadeSkin.Content(card, KitFrame.Dialogue);
            LocalizedGUI.Label(new Rect(inner.x, inner.y, inner.width, 30), heading, title);
            small.wordWrap = true; LocalizedGUI.Label(new Rect(inner.x, inner.y + 34, inner.width, inner.height - 34), copy, small);
            GUI.matrix = m; GUI.color = c;
            if (Event.current.type == EventType.MouseUp && !card.Contains(Event.current.mousePosition) && !new Rect(100, 70, 60, 50).Contains(Event.current.mousePosition) && !LanternRow.Contains(Event.current.mousePosition)) gaugeHelpAt = -1;
        }
        private static string FirstSentences(string text, int count)
        {
            if (string.IsNullOrEmpty(text)) return "";
            text = GameLocalization.Text(text);
            int at = -1;
            for (int i = 0; i < count; i++) { int next = text.IndexOf(". ", at + 1, System.StringComparison.Ordinal); if (next < 0) return text; at = next + 1; }
            return text.Substring(0, at).Trim();
        }
        private float shownHealth, shownGauge, goldGain, goldGainAt = -10;
        private float shownGold, shownEggs, nextWalletTick, nextHeartbeat, heartbeatAt = -10;
        private int pauseShown = -1, pauseDrawnFrame = -10; private float pauseAt;
        /// <summary>The pause menu and its pages, each opening with the shared pop (requirement 24/10/2026).</summary>
        private void DrawPause(ArenaController owner)
        {
            if (pausePage != pauseShown || Time.frameCount - pauseDrawnFrame > 2) { pauseShown = pausePage; pauseAt = Time.unscaledTime; }
            pauseDrawnFrame = Time.frameCount;
            // the paused world steps back behind the menu (it drew straight over the room, a speech bubble poking out above it)
            Fill(new Rect(-1920, -1080, 5760, 3240), new Color(.02f, .02f, .05f, .62f));
            Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
            LivingUI.Pop(Time.unscaledTime - pauseAt, new Vector2(960, 540));
            try { DrawPausePage(owner); }
            finally { GUI.matrix = keepMatrix; GUI.color = keepColour; }
        }
        private void DrawPausePage(ArenaController owner)
        {
            if (pausePage == 1)
            {
                ArcadeSkin.Frame(new Rect(330, 150, 1260, 800), KitFrame.Dialogue, .6f);
                ArcadeSkin.Header(new Rect(510, 190, 900, 92), "THE NIGHT WAITS WITH YOU", "Light, Sound & Comfort");
                if (navigation.Button(new Rect(1330, 205, 200, 50), "BACK", button)) pausePage = 0;
                SettingsSheet.Draw(owner.Profile ?? new PlayerProfile(), new Vector2(320, 180), (r, c) => navigation.Button(r, c, button), (r, c) => LocalizedGUI.Label(r, ArcadeTypography.DisplayText(c), text), owner.SaveSettings, true, r => navigation.Button(r, "", GUIStyle.none));
                return;
            }
            if (pausePage == 2)
            {
                ArcadeSkin.Frame(new Rect(600, 330, 720, 400), KitFrame.Dialogue, .6f);
                ArcadeSkin.Header(new Rect(660, 380, 600, 100), "THE NIGHT WAITS WITH YOU", "Start over?");
                LocalizedGUI.Label(new Rect(660, 490, 600, 80), "This night ends here and is not recorded.\nA new night begins with the same hero.", text);
                if (navigation.Button(new Rect(660, 600, 280, 65), "KEEP PLAYING", primary)) pausePage = 0;
                if (navigation.Button(new Rect(980, 600, 280, 65), "START OVER", dangerButton ??= ArcadeSkin.DangerStyle(button))) { pausePage = 0; owner.RestartRun(); }
                return;
            }
            if (pausePage == 4)
            {
                ArcadeSkin.Frame(new Rect(240, 120, 1440, 860), KitFrame.Dialogue, .6f);
                ArcadeSkin.Header(new Rect(510, 150, 900, 92), "THE NIGHT WAITS WITH YOU", "Combos");
                if (navigation.Button(new Rect(1420, 165, 200, 64), "BACK", button)) pausePage = 0;
                DrawCombos(owner, new Rect(300, 270, 1320, 680));
                return;
            }
            if (pausePage == 3)
            {
                // How to play: six cards, one line each, any time
                ArcadeSkin.Frame(new Rect(240, 120, 1440, 860), KitFrame.Dialogue, .6f);
                ArcadeSkin.Header(new Rect(510, 150, 900, 92), "THE NIGHT WAITS WITH YOU", "How to Play");
                if (navigation.Button(new Rect(1420, 165, 200, 64), "BACK", button)) pausePage = 0;
                HowToPlayBook.Draw(new Rect(300, 270, 1320, 680), owner.Content, owner.Session?.State.ChampionId, ref guidePage); // the picture book (requirement 24/10/2026)
                return;
            }
            // the pause card at the standard text size with buttons a thumb can hit (requirement 3/11/2026: at 250 wide the
            // longer words shrank to 1.4 mm on a phone)
            pauseButton ??= new GUIStyle(button) { fontSize = ArcadeTypography.SizeNote };
            ArcadeSkin.Frame(new Rect(580, 200, 760, 760), KitFrame.Dialogue, .6f);
            // the lamp turned down low while the night waits (review 31/10/2026)
            if (ArcadeSkin.KitSprite("Info/PauseLamp") is Sprite pauseLamp) Sprite(new Rect(928, 236, 64, 64), pauseLamp); // inside the frame, never over the room's speech
            ArcadeSkin.Header(new Rect(680, 288, 560, 100), "THE NIGHT WAITS WITH YOU", "Paused");
            if (navigation.Button(new Rect(680, 410, 560, 80), "RESUME", primary)) owner.TogglePause();
            if (navigation.Button(new Rect(640, 510, 310, 76), "HOW TO PLAY", pauseButton)) pausePage = 3;
            if (navigation.Button(new Rect(970, 510, 310, 76), "COMBOS", pauseButton)) { pausePage = 4; comboScroll = Vector2.zero; }
            if (navigation.Button(new Rect(640, 600, 310, 76), "STATS", pauseButton)) { owner.ToggleDetails(); owner.Details.SelectTab(0); }
            if (navigation.Button(new Rect(970, 600, 310, 76), "RELICS", pauseButton)) { owner.ToggleDetails(); owner.Details.SelectTab(2); }
            if (navigation.Button(new Rect(640, 690, 310, 76), "SETTINGS", pauseButton)) pausePage = 1;
            if (navigation.Button(new Rect(970, 690, 310, 76), "START OVER", pauseButton)) pausePage = 2;
            if (navigation.Button(new Rect(680, 800, 560, 76), "MAIN MENU", pauseButton)) owner.RequestMenuReturn();
        }
        private GUIStyle pauseButton;

        public void ReadNavigation(bool enabled) { RunUIHints.Refresh(); if (enabled) navigation.ReadInput(); }
        private Vector2 comboScroll; private GUIStyle comboLine; private int guidePage;
        /// <summary>The Combos page (play-test 20/10/2026, item 26; groups of three to five, requirement 25/10/2026): every combo this
        /// hero's shop can offer, the groups first. A combo shows its friends you have met (the rest as unknown badges), its name once
        /// two of them are known, and what it does (lit while all of them are in the brigade).</summary>
        private void DrawCombos(ArenaController owner, Rect area)
        {
            RunSession session = owner.Session; RunCatalog catalog = owner.Content;
            UnitDefinition hero = catalog.Unit(session.State.ChampionId);
            var list = new System.Collections.Generic.List<(string[] members, string name, string line)>();
            bool Offered(string[] members)
            {
                foreach (string m in members)
                {
                    UnitDefinition u = catalog.Unit(m);
                    if (u == null || u.GiftOnly || catalog.HeroRevision > 0 && hero != null && !K11HeroRules.Eligible(hero, u)) return false;
                }
                return true;
            }
            foreach (ComboDefinition c in Combos.All)
                if ((c.Members.Length >= 3 || Duets.All.Length > 0 && System.Array.TrueForAll(Duets.All, d => Combos.PairFor(d.A, d.B) != c)) && Offered(c.Members)) list.Add((c.Members, c.Name, c.Line));
            foreach (Duet d in Duets.All)
            {
                string[] pair = { d.A, d.B };
                if (!Offered(pair)) continue;
                ComboDefinition move = Combos.PairFor(d.A, d.B);
                list.Add((pair, move != null ? move.Name : null, move != null ? move.Line : d.Line));
            }
            list.Sort((x, y) => y.members.Length != x.members.Length ? y.members.Length.CompareTo(x.members.Length) : Known(session, y.members).CompareTo(Known(session, x.members)));
            comboLine ??= new GUIStyle(text) { fontSize = ArcadeTypography.SizeNote, wordWrap = true };
            const float row = 120;
            // review 31/10/2026: the page says how far you are, and an unfound combo names the friend you already met
            int found = 0; foreach (var c in list) if (Known(session, c.members) > 1) found++;
            comboLine.normal.textColor = ArcadeSkin.Cream;
            LocalizedGUI.Label(new Rect(area.x + 8, area.y, area.width - 40, 80), "Found " + found + " of " + list.Count + " combos" + (found == 0 ? " · bring two friends of a combo into the brigade" : ""), comboLine);
            area = new Rect(area.x, area.y + 86, area.width, area.height - 86);
            comboScroll = ScrollPanel.Begin(area, comboScroll, new Rect(0, 0, area.width - 20, Mathf.Max(area.height, list.Count * row)));
            for (int i = 0; i < list.Count; i++)
            {
                var c = list[i]; int known = Known(session, c.members); bool active = System.Array.TrueForAll(c.members, session.OwnsCompanionSpecies);
                Rect r = new Rect(0, i * row, area.width - 24, row - 10);
                // a walnut row plate per combo (review 31/10/2026); an active one glows behind it
                if (active) ArcadeSkin.Glow(new Rect(r.x - 8, r.y - 8, r.width + 16, r.height + 16), new Color(.95f, .76f, .33f, .35f));
                if (!ArcadeSkin.KitSliced(r, "Info/ComboRow", .6f)) ArcadeSkin.Line(r, active ? new Color(.95f, .76f, .33f, .22f) : new Color(1, 1, 1, .05f));
                float size = c.members.Length <= 2 ? 90 : c.members.Length == 3 ? 76 : 62, x = r.x + 44; // clear of the plate's brass end cap
                for (int k = 0; k < c.members.Length; k++)
                {
                    if (k > 0 && c.members.Length == 2) { Sprite(new Rect(x, r.y + 30, 52, 52), CompanionRoles.ComboBadge); x += 60; }
                    Sprite(new Rect(x, r.y + (row - 10 - size) * .5f, size, size), Met(session, c.members[k]) ? catalog.Unit(c.members[k]).Body : CompanionRoles.UnknownBadge);
                    x += size + 6;
                }
                x = Mathf.Max(x + 18, r.x + 316);
                comboLine.normal.textColor = active ? ArcadeSkin.Gold : known > 1 ? ArcadeSkin.Cream : new Color(.7f, .68f, .64f);
                string met = known == 1 ? RunUICopy.UnitName(catalog.Unit(System.Array.Find(c.members, m => Met(session, m)))) : null;
                string title = known > 1 ? (c.name ?? string.Join(" + ", System.Array.ConvertAll(c.members, m => RunUICopy.UnitName(catalog.Unit(m))))) : met != null ? met + " + ?" : "Unknown combo";
                string count = c.members.Length > 2 && known > 1 ? "  ·  " + Combos.OwnedOf(session, c.members) + " of " + c.members.Length + " in party" : "";
                LocalizedGUI.Label(new Rect(x, r.y + 16, r.xMax - x - 48, r.height - 28), title + count + (active ? "  ·  active" : "") + "\n" + (known > 1 ? c.line : c.members.Length == 2 ? "Meet both friends to find this combo." : "Meet two of these " + c.members.Length + " friends to find this combo."), comboLine);
            }
            ScrollPanel.End();
        }
        private static int Known(RunSession session, string[] members) { int n = 0; foreach (string m in members) if (Met(session, m)) n++; return n; }
        private static bool Met(RunSession session, string id) => session.OwnsCompanionSpecies(id) || session.State.AcquiredVariants.Exists(v => v.UnitId == id);
        public void ShowInvoiceEvidenceFixture()
        {
            if (!RunPaths.K6Capture) throw new System.InvalidOperationException("Invoice fixture requires isolated K6 capture mode");
            invoice.ScrollToEnd();
        }
        public void ShowOfferPreviewFixture()
        {
            if (!RunPaths.K3FinalCapture) throw new System.InvalidOperationException("Preview fixture requires isolated capture mode");
        }
        public void ShowReceiptFixture()
        {
            if (!RunPaths.K3Capture) throw new System.InvalidOperationException("Receipt fixture requires isolated capture mode");
            receiptOpen = true;
        }
        private static readonly Color Panel = ArcadeSkin.Deep;

        /// <summary>A large number kept short: 950, 12.4K, 331K, 1.2M.</summary>
        public static string ShortNumber(float value)
        {
            float v = Mathf.Max(0, Mathf.Ceil(value));
            if (v < 10000) return v.ToString("0");
            if (v < 100000) return (v / 1000f).ToString("0.#") + "K";
            if (v < 1000000) return (v / 1000f).ToString("0") + "K";
            return (v / 1000000f).ToString("0.#") + "M";
        }
        public static void Fill(Rect rectangle, Color color)
        {
            // a whole-screen cover reaches past the 16:9 frame, so wider or taller screens show no edge of the world beside it
            if (rectangle.x == 0 && rectangle.y == 0 && rectangle.width == 1920 && rectangle.height == 1080) rectangle = new Rect(-2000, -1200, 5920, 3480);
            if (rectangle.height >= 35f && rectangle.width >= 55f) { ArcadeSkin.Panel(rectangle, color); return; }
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rectangle, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        /// <summary>Draws only a sprite's painted part (its ink bounds), as large as fits the rectangle and centred: a character
        /// fills a medallion instead of its whole cell with its empty margins (review 3/11/2026: friends filled 30% of their frames).</summary>
        public static void SpriteInk(Rect rectangle, Sprite sprite)
        {
            if (sprite == null) return;
            Rect ink = SpriteInkMetrics.BoundsOf(sprite); float ppu = sprite.pixelsPerUnit;
            Rect tr = sprite.textureRect; Vector2 pivot = sprite.pivot;
            float x0 = Mathf.Clamp(pivot.x + ink.xMin * ppu, 0, tr.width), x1 = Mathf.Clamp(pivot.x + ink.xMax * ppu, 0, tr.width);
            float y0 = Mathf.Clamp(pivot.y + ink.yMin * ppu, 0, tr.height), y1 = Mathf.Clamp(pivot.y + ink.yMax * ppu, 0, tr.height);
            float w = x1 - x0, h = y1 - y0; if (w < 2 || h < 2) { Sprite(rectangle, sprite); return; }
            float k = Mathf.Min(rectangle.width / w, rectangle.height / h);
            Rect dest = new Rect(rectangle.center.x - w * k * .5f, rectangle.center.y - h * k * .5f, w * k, h * k);
            Texture t = sprite.texture;
            if (Event.current.type == EventType.Repaint)
                GUI.DrawTextureWithTexCoords(dest, t, new Rect((tr.x + x0) / t.width, (tr.y + y0) / t.height, w / t.width, h / t.height));
        }
        public static void Sprite(Rect rectangle, Sprite sprite)
        {
            if (sprite == null) return;
            Rect source = sprite.rect;
            float ratio = source.width / source.height;
            float width = Mathf.Min(rectangle.width, rectangle.height * ratio);
            KitDrawAudit.Note(sprite.name, width / source.width, (int)source.width, (int)source.height);
            float height = width / ratio;
            rectangle = new Rect(rectangle.center.x - width * 0.5f, rectangle.center.y - height * 0.5f, width, height);
            GUI.DrawTextureWithTexCoords(rectangle, sprite.texture, new Rect(source.x / sprite.texture.width, source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height));
        }
        /// <summary>A sprite filling the rectangle; <paramref name="keepAspect"/> false stretches it (a page or cover turning on
        /// its edge narrows without moving its edge).</summary>
        public static void Sprite(Rect rectangle, Sprite sprite, bool keepAspect)
        {
            if (keepAspect) { Sprite(rectangle, sprite); return; }
            if (sprite == null) return;
            Rect source = sprite.rect;
            GUI.DrawTextureWithTexCoords(rectangle, sprite.texture, new Rect(source.x / sprite.texture.width, source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height));
        }

        public void Draw(ArenaController owner, Camera camera)
        {
            if (owner.NightmareEndingVisible) { ArcadeMusic.Danger = 0; return; }
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            navigation.Begin();
            if (text == null)
            {
                text = ArcadeTypography.Style(ArcadeTypography.SizeNote);
                small = new GUIStyle(text) { fontSize = ArcadeTypography.SizeNote }; // UI standard: nothing under 28
                title = ArcadeTypography.Style(36, true);
                if (ArcadeTypography.Display != null) title.font = ArcadeTypography.Display;
                title.alignment = TextAnchor.MiddleCenter;
                button = new GUIStyle(GUI.skin.button) { fontSize = 22, font = ArcadeTypography.Heading };
                numeric = ArcadeTypography.Style(ArcadeTypography.SizeNote, false, true); // UI standard: nothing under 28
                numeric.alignment = TextAnchor.MiddleCenter;
                numericLeft = new GUIStyle(numeric) { alignment = TextAnchor.MiddleLeft, fontSize = 30 };
                caption = ArcadeTypography.Style(13, false, true);
                caption.alignment = TextAnchor.MiddleCenter;
                caption.normal.textColor = ArcadeSkin.Teal;
                primary = ArcadeSkin.Primary(button);
                bossName = ArcadeTypography.Style(ArcadeTypography.SizeNote, true);
                bossName.alignment = TextAnchor.MiddleLeft;
                bossName.normal.textColor = ArcadeSkin.Ink;
                tagStyle = ArcadeTypography.Style(22, true); tagStyle.font = ArcadeTypography.Punch;
                tagStyle.alignment = TextAnchor.MiddleCenter;
                tagStyle.normal.textColor = ArcadeSkin.Ink;
            }
            RunSession session = owner.Session;
            RunSnapshot state = session.State;
            statFeedback.Observe(session);
            CombatSimulation simulation = owner.Simulation;
            Rect viewport = camera.pixelRect;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(viewport, 1920f, 1080f);
            GlossaryUI.BlockUnderlying(new Vector2(1920, 1080));
            float health = state.Phase == SessionPhase.Combat ? simulation.Champion.Health : state.Health;
            float maximum = Mathf.Max(1f, owner.Content.Unit(state.ChampionId).Health + session.Bonus(ItemEffect.Health));
            if (owner.Content.StatRevision > 0) maximum = K10StatRules.MaximumHealth(session, owner.Content);
            int gold = RunSession.BoundedCounter((long)state.Gold + (state.Phase == SessionPhase.Combat ? simulation.Gold : 0));
            int eggs = RunSession.BoundedCounter((long)state.Eggs + (state.Phase == SessionPhase.Combat ? simulation.Eggs : 0));
            if (walletRun != state.RunId) { walletRun = state.RunId; healthTrail = health; lastGold = gold; lastEggs = eggs; goldChanged = eggsChanged = -100; shownGold = gold; shownEggs = eggs; shownHealth = health; goldGain = 0; }
            // P2: the counters run up to the new total (a soft tick per step while gold comes in); spending runs down silently
            if (Event.current.type == EventType.Repaint)
            {
                float step = Time.unscaledDeltaTime;
                float rise = gold - shownGold;
                shownGold = PresentationSettings.ReducedMotion ? gold : Mathf.MoveTowards(shownGold, gold, Mathf.Max(8 * step, Mathf.Abs(rise) * 10 * step));
                shownEggs = PresentationSettings.ReducedMotion ? eggs : Mathf.MoveTowards(shownEggs, eggs, Mathf.Max(6 * step, Mathf.Abs(eggs - shownEggs) * 10 * step));
                if (rise >= 1 && !PresentationSettings.ReducedAudio && Time.unscaledTime >= nextWalletTick) { nextWalletTick = Time.unscaledTime + 0.07f; CheckpointAudio.PlayInterfaceId("loot.coin.bounce"); }
            }
            if (lastGold != gold)
            {
                // gold coming in gathers into one +N beside the counter, so a stream of coins reads as one sum
                if (gold > lastGold) { if (Time.unscaledTime - goldGainAt > 1.1f) goldGain = 0; goldGain += gold - lastGold; goldGainAt = Time.unscaledTime; }
                goldChanged = simulation.VisualElapsed; lastGold = gold;
            }
            if (lastEggs != eggs) { eggsChanged = simulation.VisualElapsed; lastEggs = eggs; }
            bool fighting = state.Phase == SessionPhase.Combat && simulation.Champion.Alive;
            danger = fighting ? Mathf.Clamp01(Mathf.InverseLerp(0.32f, 0.06f, health / maximum)) : 0f;
            ArcadeMusic.Danger = danger;
            // M1: while the light is low a heartbeat sounds under the thinned music, quicker as it falls, and the dark edge breathes with it
            bool repaint = Event.current.type == EventType.Repaint;
            if (repaint && danger > 0.01f && !PresentationSettings.ReducedAudio && !owner.PresentationPaused && Time.unscaledTime >= nextHeartbeat)
            {
                nextHeartbeat = Time.unscaledTime + Mathf.Lerp(1.15f, 0.75f, danger); heartbeatAt = Time.unscaledTime;
                CheckpointAudio.PlayInterfaceId("plr.lowhp");
            }
            float beat = PresentationSettings.ReducedMotion || PresentationSettings.ReducedFlashes ? 0 : Mathf.Exp(-(Time.unscaledTime - heartbeatAt) * 7f);
            LowHealthVignette.Draw(new Vector2(1920, 1080), Mathf.Clamp01(danger * (1 + 0.35f * beat)));
            if (state.Phase == SessionPhase.Combat) NightHud.EdgeEyes(simulation, camera, new Vector2(1920, 1080));
            float bossFlash = owner.Feel != null ? owner.Feel.ScreenFlash : 0;
            if (bossFlash > 0.01f) Fill(new Rect(-1920, -1080, 5760, 3240), new Color(1f, 0.96f, 0.86f, bossFlash * 0.55f)); // a boss falls: the night flashes white
            if (Event.current.type == EventType.Repaint) ObserveMoments(owner);
            // healing fills the bar up over a moment (the number is already true); a hit drops it at once, the trail follows
            if (PresentationSettings.ReducedMotion || health < shownHealth) shownHealth = health;
            else if (Event.current.type == EventType.Repaint) shownHealth = Mathf.MoveTowards(shownHealth, health, Time.unscaledDeltaTime * maximum * 1.2f);
            if (PresentationSettings.ReducedMotion || health > healthTrail) healthTrail = health;
            else if (Event.current.type == EventType.Repaint && simulation.Phase != RunPhase.Paused) healthTrail = Mathf.MoveTowards(healthTrail, health, Mathf.Min(Time.unscaledDeltaTime, 0.05f) * maximum * 0.5f);
            // the run's HUD stays off the results screen (review 3/11/2026: health, gold and the wave plate showed behind its panels)
            bool endScreen = (state.Phase == SessionPhase.Victory || state.Phase == SessionPhase.Defeat) && owner.ResultsReady;
            Color hudTint = GUI.color; bool hudEnabled = GUI.enabled;
            if (endScreen) { GUI.color = Color.clear; GUI.enabled = false; }
            if (ArcadeSkin.HasKit)
            {
                // the painted HUD (UI kit, item 13): the hero in a brass portrait slot, the health bar with its heart cap and crimson fill
                ArcadeSkin.PortraitSlot(new Rect(14, 8, 92, 92), false);
                Sprite(new Rect(31, 25, 58, 58), owner.Content.Unit(state.ChampionId).Body);
                NightHud.LightRing(new Vector2(60, 55), 40, CompanionDeployment.Select(state.Roster).Count + ActOneCanon.StoryCount(state.Wave, state.Phase == SessionPhase.Shop, state.Endless));
                Rect tube = ArcadeSkin.HealthBar(new Rect(100, 24, 300, 64), Mathf.Clamp01(shownHealth / maximum), Mathf.Clamp01(healthTrail / maximum));
                numeric.normal.textColor = ArcadeSkin.Cream;
                ArcadeTypography.ShadowLabel(new Rect(tube.x, tube.y - 4, tube.width, tube.height + 8), Mathf.CeilToInt(Mathf.Min(health, maximum)) + " / " + Mathf.CeilToInt(maximum), numeric);
            }
            else
            {
            ArcadeSkin.Plate(new Rect(22, 16, 340, 72), ArcadeSkin.Accent);
            Sprite(new Rect(35, 20, 62, 62), owner.Content.Unit(state.ChampionId).Body);
            NightHud.LightRing(new Vector2(66, 51), 40, CompanionDeployment.Select(state.Roster).Count + ActOneCanon.StoryCount(state.Wave, state.Phase == SessionPhase.Shop, state.Endless));
            ArcadeSkin.Line(new Rect(105, 36, 230, 30), new Color(0.02f, 0.02f, 0.05f, 0.95f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(healthTrail / maximum), 24), new Color(0.55f, 0.30f, 0.14f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(shownHealth / maximum), 24), new Color(1f, 0.70f, 0.36f)); // your light, not the hostile red
            numeric.normal.textColor = health / maximum > 0.45f ? ArcadeSkin.Ink : ArcadeSkin.Cream; // ink reads on the lantern bar
            LocalizedGUI.Label(new Rect(108, 34, 224, 34), Mathf.CeilToInt(Mathf.Min(health, maximum)) + " / " + Mathf.CeilToInt(maximum), numeric);
            Sprite(new Rect(92, 31, 38, 38), owner.Content.HeartSprite);
            }
            numeric.normal.textColor = ArcadeSkin.Cream;
            if (owner.Content.HeroRevision > 0 && state.Phase == SessionPhase.Combat)
            {
                float required = simulation.HeroIdentityEnabled ? simulation.HeroIdentityRequired : simulation.Champion.Definition.HeroMechanic == HeroMechanic.LanternStride ? 12 : simulation.Champion.Definition.HeroMechanic == HeroMechanic.ParcelExpress ? 6 : 2;
                Rect charge = ArcadeSkin.HasKit ? new Rect(160, 90, 210, 6) : new Rect(108, 72, 224, 5);
                ArcadeSkin.Line(charge, new Color(0.06f, 0.07f, 0.11f, 0.85f));
                // the gauge glides to its new fill instead of stepping with each hit
                // while the hero is transformed the bar shows the time left, in a hot gold that flickers (requirement 25/10/2026)
                bool live = simulation.HeroAwakening;
                float gauge = live ? simulation.HeroAwakenRemaining : Mathf.Clamp01((simulation.HeroIdentityEnabled ? simulation.HeroIdentityCharge : simulation.HeroCharge) / required);
                if (PresentationSettings.ReducedMotion) shownGauge = gauge;
                else if (Event.current.type == EventType.Repaint) shownGauge = Mathf.MoveTowards(shownGauge, gauge, Time.unscaledDeltaTime * (gauge > shownGauge ? 2.5f : 6f));
                Color fill = simulation.HeroIdentityEnabled ? HeroIdentities.For(state.ChampionId).Color : ArcadeSkin.Teal;
                if (live)
                {
                    float flicker = PresentationSettings.ReducedFlashes ? .5f : .5f + .5f * Mathf.Sin(Time.unscaledTime * 14f);
                    fill = Color.Lerp(new Color(1f, .62f, .2f), new Color(1f, .97f, .8f), flicker);
                    ArcadeSkin.Glow(new Rect(charge.x - 8, charge.y - 8, charge.width * shownGauge + 16, charge.height + 16), new Color(1f, .75f, .35f, .25f + .25f * flicker));
                }
                ArcadeSkin.Line(new Rect(charge.x, charge.y, charge.width * shownGauge, charge.height), fill);
                // the gauge says what it is (requirement 25/10/2026: icons are tappable and explained): a small brass "i" at its start
                Rect help = new Rect(charge.x - 30, charge.y - 11, 26, 26);
                Rect face = ArcadeSkin.RoundArt(help, KitRound.Brass, help.Contains(Event.current.mousePosition) ? 1 : 0);
                ArcadeSkin.KitCaption(face, "i", ArcadeSkin.KitInk, small, true);
                if (TouchTargets.Button(help, GUIContent.none, GUIStyle.none)) { lanternHelp = false; gaugeHelpAt = gaugeHelpAt > 0 ? -1 : Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.tooltip"); }
                if (simulation.HeroIdentityEnabled && !owner.PresentationPaused && !PlayerGuide.ModalOpen(owner)) // a modal page covers it whole, never half
                {
                    // drawn straight over the floor: a thick name and a shadow under both lines so they read on any tile
                    if (identityStyle == null)
                    {
                        identityStyle = ArcadeTypography.Style(ArcadeTypography.SizeNote, true); identityStyle.wordWrap = false;
                        identityLine = ArcadeTypography.Style(ArcadeTypography.SizeNote); identityLine.normal.textColor = ArcadeSkin.Cream;
                    }
                    identityStyle.normal.textColor = Color.Lerp(HeroIdentities.For(state.ChampionId).Color, ArcadeSkin.Cream, 0.55f);
                    // under the counters in the left column, clear of the tip banner at the top middle (UI standard: HUD zones),
                    // on a velvet plaque so the two lines read on any floor
                    // no plaque behind the two lines any more (review 31/10/2026: the red box read as nothing and covered the prism
                    // count); the thick shadowed words read on any floor
                    float drop = RunUIHints.Device == RunUIDevice.Touch ? TouchButtonSize + 16 : 0; // under the phone's pause and brigade buttons
                    Rect plaque = new Rect(12, 236 + drop, 396, 112);
                    // the whole plaque answers a tap with the gauge's help (what fills it, what the third step does)
                    if (TouchTargets.Button(plaque, GUIContent.none, GUIStyle.none)) { lanternHelp = false; gaugeHelpAt = gaugeHelpAt > 0 ? -1 : Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.tooltip"); }
                    string status = simulation.HeroIdentityStatus;
                    Sprite gaugePlate = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite("HUD/LampGaugePlate");
                    if (gaugePlate != null && status.StartsWith("Gauge ", System.StringComparison.Ordinal))
                    {
                        // the three-step gauge as three lamp beads on a small plate by the counters (review 4/11/2026: bare words on the
                        // floor that damage numbers printed over); its name and what the third step does are in its tap help
                        Rect gaugeArea = new Rect(24, 250 + drop, 216, 61);
                        Sprite(gaugeArea, gaugePlate);
                        Sprite lit = ArcadeSkin.KitSprite("HUD/LampPipLit"), dark = ArcadeSkin.KitSprite("HUD/LampPipEmpty");
                        for (int step = 0; step < 3; step++)
                        {
                            float cx = gaugeArea.x + gaugeArea.width * (.18f + step * .32f), size = gaugeArea.height * .58f;
                            Sprite(new Rect(cx - size * .5f, gaugeArea.center.y - size * .5f, size, size), step < simulation.HeroAwakenTier ? lit : dark);
                        }
                    }
                    else
                    {
                        ArcadeTypography.ShadowLabel(new Rect(32, 248 + drop, 360, 40), RunUICopy.Title(HeroIdentities.For(state.ChampionId).Name), identityStyle);
                        ArcadeTypography.ShadowLabel(new Rect(32, 284 + drop, 360, 62), RunUICopy.Sentence(status), identityLine);
                    }
                }
            }
            float goldPulse = LivingUI.Press(simulation.VisualElapsed - goldChanged, PresentationSettings.ReducedMotion) * 4;
            float eggPulse = LivingUI.Press(simulation.VisualElapsed - eggsChanged, PresentationSettings.ReducedMotion) * 4;
            // walnut counter plates with a round socket for the coin and the egg (UI kit)
            Rect goldSocket = ArcadeSkin.Counter(new Rect(16, 100, 200, 44), KitCounter.Gold, out Rect goldText);
            Rect eggSocket = ArcadeSkin.Counter(new Rect(16, 146, 200, 44), KitCounter.Egg, out Rect eggText);
            Sprite(new Rect(goldSocket.x - goldPulse, goldSocket.y - goldPulse, goldSocket.width + goldPulse * 2, goldSocket.height + goldPulse * 2), owner.Content.Arena.Coin); // centred in the socket
            numericLeft.normal.textColor = Color.Lerp(ArcadeSkin.Cream, ArcadeSkin.Gold, goldPulse / 4f);
            LocalizedGUI.Label(goldText, Mathf.RoundToInt(shownGold).ToString("N0"), numericLeft);
            float gainAge = Time.unscaledTime - goldGainAt;
            if (goldGain > 0 && gainAge < 1.1f)
            {
                Color keepGain = GUI.color; GUI.color = new Color(keepGain.r, keepGain.g, keepGain.b, keepGain.a * (1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.7f, 1.1f, gainAge))));
                numericLeft.normal.textColor = ArcadeSkin.Gold;
                float rise = PresentationSettings.ReducedMotion ? 0 : (1 - Mathf.Clamp01(gainAge / .18f)) * 6;
                ArcadeTypography.ShadowLabel(new Rect(goldText.xMax + 10, goldText.y + rise, 110, goldText.height), "+" + goldGain.ToString("N0"), numericLeft);
                GUI.color = keepGain; numericLeft.normal.textColor = ArcadeSkin.Cream;
            }
            Sprite(new Rect(eggSocket.x - eggPulse, eggSocket.y - eggPulse, eggSocket.width + eggPulse * 2, eggSocket.height + eggPulse * 2), owner.Content.EggSprite);
            numericLeft.normal.textColor = Color.Lerp(ArcadeSkin.Cream, ArcadeSkin.Teal, eggPulse / 4f);
            LocalizedGUI.Label(eggText, Mathf.RoundToInt(shownEggs).ToString("N0"), numericLeft);
            // in the shop the banner sits clear of the health bar's end cap (it overlapped it at x 380)
            Rect waveRect = state.Phase == SessionPhase.Shop ? new Rect(452, 26, 280, 52) : new Rect(820, 22, 280, 52);
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(waveRect, KitFrame.WaveBanner); else ArcadeSkin.Plate(waveRect, ArcadeSkin.Accent);
            string waveText = (state.Endless ? "∞ " : "") + "Wave " + state.Wave;
            if (ArcadeSkin.HasKit) ArcadeSkin.KitCaption(ArcadeSkin.Content(waveRect, KitFrame.WaveBanner, false), waveText, title.normal.textColor, title, true); // shrinks to fit inside the banner
            else LocalizedGUI.Label(new Rect(waveRect.x + 10, waveRect.y + 4, waveRect.width - 10, 44), waveText, title);
            if (state.Phase == SessionPhase.Combat)
            {
                bool overtime = simulation.IsBossOvertime;
                float remaining = simulation.WaveDuration - simulation.Elapsed;
                // the brass-and-walnut plate under the wave banner (its clock hand on the left); the number's colour tells the state
                Rect timer = new Rect(866, 78, 188, 52);
                bool plate = ArcadeSkin.KitSliced(timer, "HUD/TimerPlate", timer.height / 64);
                if (!plate) ArcadeSkin.Plate(timer, overtime ? ArcadeSkin.Danger : remaining <= 5 ? ArcadeSkin.Gold : ArcadeSkin.Teal);
                numeric.normal.textColor = overtime ? new Color(1f, .45f, .4f) : remaining <= 5 ? ArcadeSkin.Gold : ArcadeSkin.Cream;
                float hand = plate ? 44 * timer.height / 64 : 10;
                LocalizedGUI.Label(new Rect(timer.x + hand, timer.y + 1, timer.width - hand - 14, timer.height - 2), CombatTimerCaption(overtime, remaining), numeric);
                numeric.normal.textColor = ArcadeSkin.Cream;
            }
            // no lantern row under the wallet (requirement 4/11/2026: it only confused players); the lanterns stay in the journal
            if (state.Phase == SessionPhase.Combat) { NightHud.LampLoss(new Rect(560, 150, 800, 60)); NightHud.Knocking(new Rect(610, 130, 700, 48), simulation.KnockRemaining); }
            DrawControls(owner);
            GUI.color = hudTint; GUI.enabled = hudEnabled;
            if (state.Phase == SessionPhase.Combat)
            {
                UnitState second = simulation.SecretSecondActor; // the duet: one bar for the two of them
                bool secondUp = second != null && second.Alive;
                if (simulation.Boss != null && (simulation.Boss.Alive || secondUp))
                {
                    float bossNow = (simulation.Boss.Alive ? simulation.Boss.Health : 0) + (secondUp ? second.Health : 0);
                    float bossMax = simulation.Boss.Definition.Health + (second != null ? second.Definition.Health : 0);
                    float bossShare = Mathf.Clamp01(bossNow / Mathf.Max(1, bossMax));
                    string bossTitle = RunUICopy.Title(RunUICopy.UnitName(simulation.Boss.Definition)); // UI standard: a name in title case, never all capitals
                    float bossTitleWidth = Mathf.Clamp(bossName.CalcSize(LocalizedGUI.Content(bossTitle)).x + 16, 350, 990);
                    // the health just lost lingers behind the fill, then drains to it
                    if (bossShare >= bossTrail) bossTrail = bossShare; else bossTrail = Mathf.MoveTowards(bossTrail, bossShare, Time.unscaledDeltaTime * .25f);
                    Sprite bossFrame = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite("HUD/BossBarFrame");
                    if (bossFrame != null)
                    {
                        // the painted boss bar (review 4/11/2026: a flat red rectangle, the one HUD piece without a frame): a brass frame
                        // over a groove of crimson, the lost health amber behind it, the boss's medallion at its left end, its name on a ribbon
                        Rect bossBarRect = new Rect(430, 962, 1120, 79), bossGroove = new Rect(bossBarRect.x + bossBarRect.width * .088f, bossBarRect.y + bossBarRect.height * .335f, bossBarRect.width * .824f, bossBarRect.height * .435f);
                        void BossLiquid(string art, float share)
                        {
                            Sprite bossStrip = ArcadeSkin.KitSprite(art); if (bossStrip == null || share <= 0) return;
                            float w = bossGroove.width * share, bossTile = bossGroove.height * bossStrip.texture.width / bossStrip.texture.height;
                            GUI.DrawTextureWithTexCoords(new Rect(bossGroove.x, bossGroove.y, w, bossGroove.height), bossStrip.texture, new Rect(0, 0, w / bossTile, 1));
                        }
                        Sprite(bossBarRect, bossFrame); // the groove is painted solid: the health is laid over it
                        BossLiquid("HUD/BossBarLag", bossTrail);
                        BossLiquid("HUD/BossBarFill", bossShare);
                        Rect bossMedal = new Rect(bossBarRect.x - 92, bossBarRect.center.y - 66, 132, 132);
                        Sprite(new Rect(bossMedal.x + 26, bossMedal.y + 26, 80, 80), simulation.Boss.Definition.Body);
                        if (ArcadeSkin.KitSprite("HUD/BossMedallion") is Sprite bossRing) Sprite(bossMedal, bossRing);
                        Rect bossRibbon = new Rect(bossBarRect.center.x - Mathf.Max(260, bossTitleWidth * .5f + 70), bossBarRect.y - 50, Mathf.Max(520, bossTitleWidth + 140), 58);
                        if (ArcadeSkin.KitSprite("HUD/BossNameRibbon") is Sprite bossBand) Sprite(bossRibbon, bossBand);
                        TextAnchor bossAlign = bossName.alignment; bossName.alignment = TextAnchor.MiddleCenter;
                        Color bossInk = bossName.normal.textColor; bossName.normal.textColor = new Color(1f, .9f, .66f); // cream on the red ribbon
                        LocalizedGUI.Label(new Rect(bossRibbon.x + 50, bossRibbon.y + 2, bossRibbon.width - 100, bossRibbon.height - 8), bossTitle, bossName);
                        bossName.alignment = bossAlign; bossName.normal.textColor = bossInk;
                        // the boss's health in short numbers (330K / 331K), no divider through them and no percentage
                        LocalizedGUI.Label(bossGroove, ShortNumber(bossNow) + " / " + ShortNumber(bossMax), numeric);
                    }
                    else
                    {
                        ArcadeSkin.Slant(new Rect(420, 908, bossTitleWidth + 70, 40), ArcadeSkin.Danger);
                        Sprite(new Rect(432, 906, 42, 42), simulation.Boss.Definition.Body);
                        LocalizedGUI.Label(new Rect(480, 908, bossTitleWidth, 40), bossTitle, bossName);
                        ArcadeSkin.Line(new Rect(424, 954, 1076, 40), new Color(0.01f, 0.01f, 0.02f, 0.75f));
                        ArcadeSkin.Line(new Rect(420, 950, 1080, 38), ArcadeSkin.Ink);
                        ArcadeSkin.Line(new Rect(425, 955, 1070 * bossShare, 28), ArcadeSkin.Danger);
                        ArcadeSkin.Line(new Rect(425, 955, 1070 * bossShare, 6), new Color(1f, 0.55f, 0.42f));
                        LocalizedGUI.Label(new Rect(425, 950, 1070, 38), ShortNumber(bossNow) + " / " + ShortNumber(bossMax), numeric);
                    }
                }
                if (simulation.Phase == RunPhase.Paused) DrawPause(owner);
                else pausePage = 0;
            }
            else if (state.Phase == SessionPhase.Shop)
            {
                // a visitor's card owns the screen: no tickets or station card over it
                if (owner.GetComponent<VisitorCard>()?.Open != true && owner.GetComponent<LucidAltar>()?.Open != true && owner.GetComponent<WaitingRealmStage>()?.Asking != true) { if (!shopDetails) owner.Shop.DrawPriceTags(camera); DrawShop(owner); } // an open details page owns the screen: no price tickets over the HUD
                if (owner.SecretRoom == null || !owner.SecretRoom.InRoom) statFeedback.Draw(owner,camera); // never over the secret room's card
            }
            else
            {
                // the end of the night opens like every popup: the closing line, then the record
                if (terminalAt < 0) terminalAt = Time.unscaledTime;
                if (owner.ResultsReady && resultsAt < 0) resultsAt = Time.unscaledTime;
                Color resultColour = GUI.color;
                if (!owner.ResultsReady)
                {
                    if (owner.Session.EndingChoicePending) { GUI.matrix = previous; GUI.skin = previousSkin; navigation.End(); return; }
                    LivingUI.Pop(Time.unscaledTime - terminalAt, new Vector2(960, 875));
                    ArcadeSkin.Plate(new Rect(560, 840, 800, 70), state.Phase == SessionPhase.Defeat ? ArcadeSkin.Danger : ArcadeSkin.Gold);
                    bool satDown = (state.ActOne?.SitCloser ?? 0) == ActOneProgress.Lost;
                    LocalizedGUI.Label(new Rect(550, 840, 820, 70), state.Phase == SessionPhase.Defeat ? satDown ? "You sat down." : "Your light goes out…" : "Dawn has come…", title);
                    GUI.matrix = previous; GUI.color = resultColour;
                    GUI.skin = previousSkin;
                    navigation.End();
                    return;
                }
                Matrix4x4 resultMatrix = GUI.matrix;
                LivingUI.Pop(Time.unscaledTime - resultsAt, new Vector2(960, 575));
                // the left column on the painted navy frame (review 31/10/2026: a flat code panel beside the parchment sheet)
                if (ArcadeSkin.HasKit) ArcadeSkin.Frame(new Rect(80, 200, 630, 790), KitFrame.Dialogue); else Fill(new Rect(80, 200, 630, 790), Panel);
                ArcadeSkin.Header(new Rect(110, 228, 575, 105), "THE NIGHT IS RECORDED", state.Phase == SessionPhase.Victory ? "Dawn Has Come" : (state.ActOne?.SitCloser ?? 0) == ActOneProgress.Lost ? "You Sat Down" : "Until You Return");
                bool fallen = state.Phase == SessionPhase.Defeat;
                // the hero stands on the night's emblem: a rising sun for the dawn, a candle just blown out for a run that ended
                if (ArcadeSkin.KitSprite(fallen ? "Info/CandleOut" : "Info/DawnEmblem") is Sprite emblem) Sprite(fallen ? new Rect(225, 330, 280, 280) : new Rect(195, 336, 340, 340), emblem);
                Sprite(fallen ? new Rect(265, 372, 200, 190) : new Rect(225, 378, 280, 280), owner.Content.Unit(state.ChampionId).Body);
                Sprite(fallen ? new Rect(420, 520, 70, 70) : new Rect(425, 540, 80, 80), owner.Content.Unit(state.ChampionId).Weapon.Sprite); // held inside the emblem, by the hero (review 3/11/2026: it hung outside the medallion)
                if (fallen) DrawFallenWord(owner, state, new Rect(115, 608, 565, 84));
                TextAnchor keepNotice = small.alignment; small.alignment = TextAnchor.UpperCenter; LocalizedGUI.Label(new Rect(115, 692, 565, 46), owner.MetaNotice, small); small.alignment = keepNotice; // centred under the emblem
                // one column of full-width plates, one shape for all (review 4/11/2026: two wide ornate plates and two square-ended ones)
                if (state.Phase == SessionPhase.Victory)
                {
                    // Endless is part of Deluxe on phones (requirement 4/11/2026): the button stays, with a lock, and opens the offer
                    Rect endless = new Rect(115, 744, 560, 56);
                    bool locked = Deluxe.EndlessLocked;
                    if (navigation.Button(endless, locked ? "ENDLESS · DELUXE" : "ENDLESS · KEEP BRIGADE AND RELICS", button))
                    {
                        if (locked) DeluxeOffer.Open("Endless is part of Deluxe: keep your brigade and relics past dawn.");
                        else owner.ContinueEndless();
                    }
                    if (locked) ArcadeSkin.Lock(new Vector2(endless.center.x - button.CalcSize(LocalizedGUI.Content("ENDLESS · DELUXE")).x * .5f - 20, endless.center.y), 16, ArcadeSkin.KitCream); // just before the words
                }
                // the Clock's Rewind (play-test item 27): once a run, back to the shop before the lost wave
                if (fallen && owner.Session.RewindReady && navigation.Button(new Rect(115, 744, 560, 56), "REWIND · BACK TO WAVE " + owner.Session.RewindWave, primary)) owner.RewindRun();
                if (navigation.Button(new Rect(115, 806, 560, 56), "PLAY AGAIN", primary)) owner.RestartRun();
                // the summary names what was earned for the Waiting Room: one tap to where it is spent
                if (navigation.Button(new Rect(115, 868, 560, 56), "WAITING ROOM · LAMP TREE", button)) owner.ReturnToWaitingRoom();
                if (navigation.Button(new Rect(115, 930, 560, 56), "MAIN MENU", button)) owner.ReturnToMenu();
                invoice.Draw(new Rect(770, 200, 1060, 750), state, owner.Content);
                GUI.matrix = resultMatrix; GUI.color = resultColour;
            }
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) terminalAt = resultsAt = -1;
            if (!string.IsNullOrEmpty(owner.SaveError))
            {
                Rect note = new Rect(560, 236, 800, 64);
                ArcadeSkin.Plate(note, ArcadeSkin.Worse);
                LocalizedGUI.Label(new Rect(note.x + 24, note.y, note.width - 48, note.height), SaveNote(owner), new GUIStyle(text) { alignment = TextAnchor.MiddleCenter });
            }
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) DrawBonds(owner);
            waveStamp.Observe(state.RunId, state.Wave, state.Phase == SessionPhase.Combat && !owner.TransitionPending);
            if (state.Phase == SessionPhase.Combat && !owner.TransitionPending) waveStamp.Draw((state.Endless ? "∞ " : "") + "WAVE " + state.Wave);
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) { coach.Draw(owner, camera); guide.Draw(owner); }
            if (state.Phase == SessionPhase.Combat || lanternHelp && state.Phase == SessionPhase.Shop) DrawGaugeHelp(owner, state); else gaugeHelpAt = -1;
            cutIn.Draw(new Vector2(1920, 1080));
            navigation.End();
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }

        private void DrawShop(ArenaController owner)
        {
            if (owner.HallExitPending) return;
            if (owner.TransitionPending)
            {
                ArcadeSkin.Plate(new Rect(660, 722, 600, 66), ArcadeSkin.Teal);
                LocalizedGUI.Label(new Rect(550, 720, 820, 70), "The dark draws back", title);
                return;
            }
            WorldShop shop = owner.Shop;
            if (shop == null) return;
            shop.DrawMerchantDialogue();
            if (owner.Simulation.Phase == RunPhase.Paused || owner.PresentationPaused) { DrawPause(owner); return; } // also in a secret room, where losing focus pauses too
            pausePage = 0;
            WorldShop.Station station = shop.Nearby;
            ShownShopCard = Rect.zero;
            if (station != null)
            {
                ShopCardStyles();
                if (station.Action == "NEXT HALL")
                {
                    Rect go = new Rect(790, 950, 340, 90);
                    ShownShopCard = go;
                    if (navigation.Button(go, shop.Special ? "TO THE SHOP  →" : "NEXT WAVE  ↓", primary)) shop.Interact(); // the dream shop leads on to the regular shop (requirement)
                    return;
                }
                if (tooltipStation != station)
                {
                    tooltipStation = station; shopScroll = Vector2.zero; shopDetails = false;
                    // a soft heartbeat when an HP-priced offer comes into focus (think twice)
                    if (station.Offer?.Currency == ShopCurrency.Health && !PresentationSettings.ReducedAudio) CheckpointAudio.PlayInterfaceId("loot.heart", .35f);
                }
                bool blood = station.Offer?.Currency == ShopCurrency.Health;
                if (station.Offer != null) offerCard.Refresh(owner.Session, owner.Content, station.Offer);
                bool item = station.Offer != null && station.Offer.Kind == OfferKind.Item;
                ItemRarity rarity = item ? owner.Content.Item(station.Offer.Id).Rarity : station.Offer != null ? owner.Content.Unit(station.Offer.Id).Rarity : ItemRarity.Common;
                Color tag = rarity != ItemRarity.Common ? RunModifierRules.RarityColor(rarity) : station.Offer != null ? ArcadeSkin.Teal : ArcadeSkin.Cream;
                if (shopDetails && station.Offer != null) { DrawShopDetails(owner, shop, station, rarity, tag, blood); return; }
                // the card, low enough to leave the stalls and their tickets in view: a picture, the name, a row of chips
                // (weapon, bonds), a few short lines, and on the right a big BUY with its price over a smaller i
                Rect panel = new Rect(500, 812, 920, 260); // room inside the rarity trim on every side
                ShownShopCard = panel;
                RarityDescriptionFrame.Draw(panel, rarity, owner.PresentationClock); if (blood) BloodPriceFrame.Draw(panel, owner.PresentationClock);
                ArcadeSkin.Line(new Rect(panel.x + 2, panel.y + 2, 8, panel.height - 4), tag);
                Rect picture = new Rect(panel.x + 26, panel.y + 24, 160, 160);
                ArcadeSkin.Glow(new Rect(picture.x + 10, picture.y + 10, picture.width - 20, picture.height - 20), new Color(0, 0, 0, .35f)); // a soft round shadow behind the picture, not a dark square
                if (station.Offer != null) RarityDescriptionFrame.Backdrop(picture, rarity, owner.PresentationClock);
                ShopDescriptionCard.DrawSprite(new Rect(picture.x + 8, picture.y + 8, 144, 144), station.Offer != null ? offerCard.Portrait : station.Icon);
                if (station.Offer != null)
                {
                    // a friend's tag says its star rank in words (review 31/10/2026: "SUPPORT ★" read as nothing; the role was a guess
                    // from its text and could be wrong, so the card shows what the friend itself does instead)
                    int stars = Mathf.Clamp(station.Offer.Tier, 1, 3);
                    string kind = item ? RunModifierRules.RarityName(rarity) : stars + (stars == 1 ? " STAR" : " STARS");
                    ArcadeSkin.Tag(new Rect(picture.x, picture.yMax + 10, 160, 32), kind, CodeTone.Walnut, tagStyle, tag); // code-drawn tag, rarity colour on walnut
                }
                // one fixed size for the price button and the i under it, centred in the card's right column (requirement 21/10/2026)
                const float PriceH = 104, InfoH = 56, ColumnGap = 12;
                float columnH = station.Offer != null ? PriceH + ColumnGap + InfoH : PriceH;
                Rect buy = new Rect(panel.xMax - 214, panel.y + Mathf.Round((panel.height - columnH) * .5f), 192, PriceH);
                float x = panel.x + 210, width = buy.x - 24 - x;
                // a drink or a food says so in a small tag beside its name, never as a row among what it gives and costs
                var relic = item ? owner.Content.Item(station.Offer.Id) : null;
                string kindWord = relic != null && relic.Category != RelicCategory.Curio ? RelicDescriptions.Category(relic).ToUpperInvariant() : null;
                float kindW = kindWord != null ? tagStyle.CalcSize(LocalizedGUI.Content(kindWord)).x + 28 : 0;
                string shownName = shop.DisplayName(station);
                float nameW = Mathf.Min(cardName.CalcSize(LocalizedGUI.Content(shownName)).x, width - (kindWord != null ? kindW + 14 : 0));
                LocalizedGUI.Label(new Rect(x, panel.y + 22, kindWord != null ? nameW : width, 44), shownName, cardName); // clear of the frame's top rim
                if (kindWord != null) ArcadeSkin.Tag(new Rect(x + nameW + 14, panel.y + 30, kindW, 30), kindWord, CodeTone.Walnut, tagStyle, ArcadeSkin.Cream);
                float y = panel.y + 70;
                if (station.Offer != null && offerCard.Attack != null) y = ShopChips(x, y, width);
                string source = station.Offer != null ? offerCard.Summary : station.Recipe != null ? owner.Session.RecipePreview(station.Recipe) : station.Description;
                int lineSize = cardLine.fontSize;
                string body = Fit(source, width, panel.yMax - 24 - y, out int more); // the last line stays clear of the bottom rim (it may shrink the words a little)
                GlossaryUI.Text(new Rect(x, y, width, panel.yMax - 20 - y), body, cardLine);
                cardLine.fontSize = lineSize;
                // short of the price, the button is drawn dimmed so it does not read as pressable
                Color buyTint = GUI.color; if (!Affordable(owner, station)) GUI.color = new Color(buyTint.r * .55f, buyTint.g * .55f, buyTint.b * .55f, buyTint.a);
                if (navigation.Button(buy, "", primary)) shop.Interact();
                GUI.color = buyTint;
                DrawPriceButton(buy, station, owner);
                if (station.Offer != null && navigation.Button(new Rect(buy.x, buy.yMax + ColumnGap, buy.width, InfoH), "DETAILS", button)) SetShopDetails(true);
                GlossaryUI.Draw(new Vector2(1920, 1080));
            }
        }
        // ---- the shop card's pieces
        private float bossTrail = 1; // the boss bar's lingering lost health (0..1)
        private GUIStyle cardName, cardLine, cardChip, cardBig, cardPrice;
        private void ShopCardStyles()
        {
            if (cardName != null) return;
            cardName = ArcadeTypography.Style(ArcadeTypography.SizeBody, true); cardName.wordWrap = false; cardName.clipping = TextClipping.Clip;
            cardLine = new GUIStyle(text) { fontSize = ArcadeTypography.SizeNote }; // UI standard: the card shows fewer lines, never smaller ones
            cardChip = ArcadeTypography.Style(ArcadeTypography.SizeNote, true); cardChip.alignment = TextAnchor.MiddleLeft; cardChip.wordWrap = false;
            cardBig = ArcadeTypography.Style(30, true); cardBig.alignment = TextAnchor.MiddleCenter; cardBig.normal.textColor = ArcadeSkin.Ink;
            cardPrice = ArcadeTypography.Style(ArcadeTypography.SizeNote, false, true); cardPrice.alignment = TextAnchor.MiddleLeft; cardPrice.normal.textColor = ArcadeSkin.Ink;
        }
        /// <summary>As many whole lines as fit the card (measured as drawn, wrapping included); a first line too long on its own
        /// is cut at a word with an ellipsis. The rest is counted (it waits under the i button).</summary>
        private string Fit(string source, float width, float height, out int more)
        {
            more = 0;
            if (string.IsNullOrEmpty(source)) return "";
            // requirement 25/10/2026: the card is fitted once per text and size, not re-measured on every GUI pass
            if (ReferenceEquals(source, fitSource) && width == fitWidth && height == fitHeight && cardLine.fontSize == fitFont) { more = fitMore; cardLine.fontSize = fitDrawn; return fitKept; }
            int entry = cardLine.fontSize;
            string kept = FitLines(source, width, height, out more);
            fitSource = source; fitWidth = width; fitHeight = height; fitFont = entry; fitDrawn = cardLine.fontSize; fitKept = kept; fitMore = more;
            return kept;
        }
        private string fitSource, fitKept;
        private float fitWidth, fitHeight;
        private int fitFont, fitDrawn, fitMore;
        private string FitLines(string source, float width, float height, out int more)
        {
            more = 0;
            string[] all = source.Replace("\r", "").Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            string kept = "";
            int taken = 0;
            for (; taken < all.Length; taken++)
            {
                string next = taken == 0 ? all[0] : kept + "\n" + all[taken];
                if (GlossaryUI.Height(next, cardLine, width) > height) break;
                kept = next;
            }
            // a first line a little too long shrinks (to 78% at most) before it is ever cut (review 31/10/2026: "Its hits pin the
            // enemy in place for..." lost its point)
            if (taken == 0 && all.Length > 0)
            {
                int size = cardLine.fontSize, floor = Mathf.RoundToInt(size * .78f);
                while (cardLine.fontSize > floor && GlossaryUI.Height(all[0], cardLine, width) > height) cardLine.fontSize--;
                if (GlossaryUI.Height(all[0], cardLine, width) <= height) { kept = all[0]; taken = 1; }
                else
                {
                    // then without its bracketed detail ("for 0.3s (once every 2s per enemy)": the detail waits under DETAILS)
                    string plain = System.Text.RegularExpressions.Regex.Replace(all[0], @"\s*\([^)]*\)", "").Trim();
                    if (plain != all[0] && GlossaryUI.Height(plain, cardLine, width) <= height) { kept = plain; taken = 1; }
                    else cardLine.fontSize = size;
                }
            }
            if (taken == 0)
            {
                string[] words = all[0].Split(' ');
                for (int count = words.Length - 1; count > 0 && taken == 0; count--)
                {
                    string cut = string.Join(" ", words, 0, count) + "…";
                    if (GlossaryUI.Height(cut, cardLine, width) <= height) { kept = cut; taken = 1; }
                }
            }
            more = all.Length - taken;
            return kept;
        }
        /// <summary>The first lines of a description, the rest counted (they wait under the i button).</summary>
        private static string Brief(string source, int lines, out int more)
        {
            more = 0;
            if (string.IsNullOrEmpty(source)) return "";
            string[] all = source.Replace("\r", "").Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            more = Mathf.Max(0, all.Length - lines);
            return string.Join("\n", all, 0, Mathf.Min(lines, all.Length));
        }
        /// <summary>A companion's bond chips: each bond it counts toward (now / next). Its weapon is a WEAPON line in the text below.</summary>
        private float ShopChips(float x, float y, float width)
        {
            // chips with real room (requirement 22/10/2026): the icon in the chip's socket, 10 px before the words, 16 after them and
            // 12 between chips; a chip that does not fit first shrinks its words, then starts a second row
            float cx = x, rowY = y; int rows = 1;
            const float H = 50, Lead = 58, Tail = 30, Gap = 14; // the words clear of the icon socket and of the end cap
            void Chip(Sprite icon, string label, bool lit)
            {
                int size = cardChip.fontSize;
                float Need() => Lead + cardChip.CalcSize(LocalizedGUI.Content(label)).x + Tail;
                float room = width - (cx - x);
                while (Need() > room && cardChip.fontSize > 24) cardChip.fontSize--;
                if (Need() > room && cx > x && rows < 2) { rows++; cx = x; rowY += H + 8; room = width; cardChip.fontSize = size; while (Need() > room && cardChip.fontSize > 24) cardChip.fontSize--; }
                float w = Mathf.Min(room, Need());
                if (w < 110) { cardChip.fontSize = size; return; }
                Rect chip = new Rect(cx, rowY, w, H);
                // a bond this friend lights wears the lit chip (its own painted gold rim); nothing circles it
                ArcadeSkin.Chip(chip, lit); // the painted walnut chip with its icon socket
                if (icon != null) ShopDescriptionCard.DrawSprite(ArcadeSkin.ChipIconRect(chip), icon); // on the socket's measured centre
                cardChip.normal.textColor = lit ? new Color(1f, .9f, .55f) : ArcadeSkin.Cream; // lit words stay bright (a blink dimmed them below the unlit ones)
                // the words sit on the chip's own middle line (its icon socket's measured centre), not the picture's box centre
                float middle = ArcadeSkin.ChipIconRect(chip).center.y + H * .03f; // the words centre on the socket's line (they sat too low and too far right)
                LocalizedGUI.Label(new Rect(chip.x + Lead, middle - H * .5f, w - Lead - Tail, H), label, cardChip);
                cardChip.fontSize = size;
                cx += w + Gap;
            }
            // the weapon leads the row on its own steel plate with its own picture (requirement 4/11/2026: not a bond chip, and
            // not a tag among the text lines)
            {
                int size = cardChip.fontSize;
                string label = offerCard.AttackLabel;
                const float PlateH = 64; // its own row, taller than the chips (review 5/11/2026: at the chips' 50 the plate and its weapon were too small)
                float Need() => WeaponPlate.Width(PlateH, cardChip.CalcSize(LocalizedGUI.Content(label)).x);
                while (Need() > width && cardChip.fontSize > 22) cardChip.fontSize--;
                Rect plate = new Rect(cx, rowY, Mathf.Min(width, Need()), PlateH);
                WeaponPlate.Draw(plate, offerCard.Attack.Weapon != null && offerCard.Attack.Weapon.Sprite != null ? offerCard.Attack.Weapon.Sprite : offerCard.AttackIcon, label, cardChip);
                cardChip.fontSize = size;
                rowY += PlateH + 8; cx = x; // the bond chips start on the row below
            }
            // (no COMBO chip: plain counts are required; a combo is listed under the details' COMBOS)
            foreach (var row in offerCard.Bonds) Chip(row.Bond.Icon, row.After + "/" + (row.Activates ? row.Next : Mathf.Max(row.After, row.Bond.NextThreshold(row.After))), row.Activates);
            return rowY + H + 10;
        }
        /// <summary>The big action button: the verb, and the price with its currency.</summary>
        private void DrawPriceButton(Rect button, WorldShop.Station station, ArenaController owner)
        {
            int price = station.Offer != null ? station.Offer.Price : station.Price;
            ShopCurrency currency = station.Offer != null ? station.Offer.Currency : station.Currency;
            bool affordable = Affordable(owner, station);
            // an HP price reads PAY; short of the price, the button says what is missing (play-test 20/10/2026)
            string verb = station.Offer != null ? (currency == ShopCurrency.Health ? "PAY" : "BUY") : station.Action == "VISITOR" ? "MEET" : station.Action;
            if (!affordable) verb = "NOT ENOUGH"; // one line; the red price and its icon say which currency
            bool priced = price > 0 && station.Action != "VISITOR";
            int bigSize = cardBig.fontSize; if (verb.Length > 6) cardBig.fontSize = 24;
            Color bigColor = cardBig.normal.textColor; // PAY is the ordinary gold button: ink on gold, the heart beside the price
            // laid out inside the plate's rim with real margins (requirement 22/10/2026): the verb on the upper half, the price on the lower
            Rect inner = ArcadeSkin.Content(button, KitPiece.Plate);
            Rect verbRect = priced ? new Rect(inner.x, inner.y, inner.width, inner.height * .5f) : inner; // a gap between the verb and the price
            // one line that shrinks to fit, never wrapped into a second line the plate cuts off
            cardBig.wordWrap = false;
            // fits both ways: as wide as the plate allows and no taller than its half (BUY lost its top to the plate's rim)
            while (cardBig.fontSize > 12 && (cardBig.CalcSize(LocalizedGUI.Content(verb)).x > verbRect.width - 6 || cardBig.CalcSize(LocalizedGUI.Content(verb)).y > verbRect.height + 4)) cardBig.fontSize--;
            LocalizedGUI.Label(verbRect, verb, cardBig);
            cardBig.fontSize = bigSize; cardBig.normal.textColor = bigColor;
            if (!priced) return;
            string amount = price.ToString();
            float wide = 30 + 8 + cardPrice.CalcSize(LocalizedGUI.Content(amount)).x, left = inner.center.x - wide * .5f;
            Rect priceRow = new Rect(inner.x, inner.y + inner.height * .57f, inner.width, inner.height * .43f);
            Rect icon = new Rect(left, priceRow.center.y - 14, 28, 28);
            Sprite(icon, currency == ShopCurrency.Health ? CurrencyIcons.Heart : currency == ShopCurrency.Eggs ? owner.Content.EggSprite : owner.Content.Arena.Coin);
            Color priceColor = cardPrice.normal.textColor;
            if (!affordable) cardPrice.normal.textColor = new Color(.85f, .16f, .2f); // short of it: the price in red
            LocalizedGUI.Label(new Rect(icon.xMax + 8, icon.y - 2, wide, 32), amount, cardPrice);
            cardPrice.normal.textColor = priceColor;
        }
        /// <summary>Whether the player has enough of the offer's currency (HP prices must leave you standing).</summary>
        private static bool Affordable(ArenaController owner, WorldShop.Station station)
        {
            RunSnapshot state = owner.Session.State;
            if (station.Offer == null) return station.Action == "VISITOR" || state.Gold >= station.Price; // the reroll and other paid stalls ask gold
            int price = station.Offer.Price;
            return station.Offer.Currency == ShopCurrency.Health ? owner.Session.BloodAffordable(price) : station.Offer.Currency == ShopCurrency.Eggs ? state.Eggs >= price : state.Gold >= price;
        }
        /// <summary>The full rules of an offer (the i button): attack and bonds, then everything else; BACK returns.</summary>
        private void DrawShopDetails(ArenaController owner, WorldShop shop, WorldShop.Station station, ItemRarity rarity, Color tag, bool blood = false)
        {
            // the page is as tall as its words (UI standard: no big frame left empty), up to the screen's height
            string description = RunUICopy.SentenceLines(offerCard.Details); // UI standard: no all-capital lines
            // the header already names the kind, rarity and stacking: the details do not repeat it on their first line
            int firstBreak = description.IndexOf('\n'), firstDot = description.IndexOf(" · ");
            if (firstBreak > 0 && firstDot > 0 && firstDot < firstBreak && !string.IsNullOrEmpty(offerCard.Identity) && offerCard.Identity.StartsWith(description.Substring(0, firstDot), System.StringComparison.OrdinalIgnoreCase))
                description = description.Substring(firstBreak + 1).TrimStart('\n');
            if (BuildPreview(owner, station.Offer) > 0) foreach (var row in previewRows) description += "\n" + row.name + " " + row.before + " → " + row.after;
            float contentHeight = GlossaryUI.Height(description, cardLine, 1120 - 112 - 30) + 8;
            float panelHeight = Mathf.Clamp(34 + 96 + offerCard.HeaderHeight + 16 + contentHeight + 58, 380, 880);
            Rect panel = new Rect(400, 150 + (880 - panelHeight) * .5f, 1120, panelHeight);
            ShownShopCard = panel;
            RarityDescriptionFrame.Draw(panel, rarity, owner.PresentationClock); if (blood) BloodPriceFrame.Draw(panel, owner.PresentationClock);
            ArcadeSkin.Line(new Rect(panel.x + 2, panel.y + 2, 8, panel.height - 4), tag);
            // stacked blocks inside the frame's padding (requirement 22/10/2026): the header row, the tiles row, then the scrolling text
            // starting below the tiles' full height, so nothing sits behind the tiles or on the frame's rim
            Rect inner = new Rect(panel.x + 56, panel.y + 34, panel.width - 112, panel.height - 68);
            RarityDescriptionFrame.Backdrop(new Rect(inner.x, inner.y, 72, 72), rarity, owner.PresentationClock);
            ShopDescriptionCard.DrawSprite(new Rect(inner.x, inner.y, 72, 72), offerCard.Portrait);
            LocalizedGUI.Label(new Rect(inner.x + 88, inner.y - 4, inner.width - 88 - 196, 44), shop.DisplayName(station), cardName);
            small.normal.textColor = tag; LocalizedGUI.Label(new Rect(inner.x + 88, inner.y + 40, inner.width - 88 - 196, 36), RunUICopy.Sentence(offerCard.Identity), small); small.normal.textColor = ArcadeSkin.Cream;
            if (navigation.Button(new Rect(inner.xMax - 176, inner.y, 176, 72), "BACK", button)) { SetShopDetails(false); return; }
            float headerHeight = offerCard.HeaderHeight, tilesTop = inner.y + 96;
            if (headerHeight > 0) offerCard.DrawHeader(new Rect(inner.x, tilesTop, inner.width, headerHeight), owner.PresentationClock);
            float top = tilesTop + headerHeight + 16 - panel.y, textWidth = inner.width - 30;
            shopScroll = ScrollPanel.Begin(new Rect(inner.x, panel.y + top, inner.width, inner.yMax - panel.y - top), shopScroll, new Rect(0, 0, textWidth + 5, contentHeight));
            GlossaryUI.Text(new Rect(0, 0, textWidth, contentHeight), description, cardLine);
            ScrollPanel.End();
            GlossaryUI.Draw(new Vector2(1920, 1080));
        }
        /// <summary>The shop description card on screen this frame (zero when none), so hover marks can stay off it.</summary>
        public static Rect ShownShopCard { get; private set; }
        public static Rect ShopTooltipBounds(float contentHeight)
        {
            float height = Mathf.Clamp(contentHeight + 85, 155, 185);
            return new Rect(485, 1030 - height, 950, height);
        }
        private readonly ShopDescriptionCard offerCard = new ShopDescriptionCard();
        private string loggedSaveError;
        /// <summary>What a failed save tells the player; the technical line goes to the log once.</summary>
        private string SaveNote(ArenaController owner)
        {
            if (loggedSaveError != owner.SaveError) { loggedSaveError = owner.SaveError; Debug.LogWarning(owner.SaveError); }
            return "Your progress could not be saved just now. It will try again at the next step.";
        }
        public static string CombatTimerCaption(bool bossOvertime, float secondsRemaining) => bossOvertime ? "BOSS" : Mathf.Max(0, Mathf.CeilToInt(secondsRemaining)).ToString();

        /// <summary>The phone's pause and brigade buttons: 96 of 1080 is 6 mm on a 6.5" phone, and TouchTargets widens the tap to 8 mm.</summary>
        private const float TouchButtonSize = 96;
        private void DrawControls(ArenaController owner)
        {
            if (RunUIHints.Device != RunUIDevice.Touch) return;
            Rect pause = new Rect(32, 232, TouchButtonSize, TouchButtonSize);
            Rect details = new Rect(32 + TouchButtonSize + 20, 232, TouchButtonSize, TouchButtonSize);
            float k = TouchButtonSize / 64f;
            if (ArcadeSkin.HasKit)
            {
                ArcadeSkin.MedallionArt(pause, KitMedallion.Pause, 0);
                ArcadeSkin.RoundArt(details, KitRound.Brass, 0);
                if (navigation.Button(pause, "", GUIStyle.none)) owner.TogglePause();
                if (navigation.Button(details, "", GUIStyle.none)) owner.ToggleDetails();
            }
            else
            {
                if (navigation.Button(pause, "", button)) owner.TogglePause();
                if (navigation.Button(details, "", button)) owner.ToggleDetails();
                ArcadeSkin.Line(new Rect(pause.x + 21 * k, pause.y + 14 * k, 8 * k, 24 * k), ArcadeSkin.Ink);
                ArcadeSkin.Line(new Rect(pause.x + 36 * k, pause.y + 14 * k, 8 * k, 24 * k), ArcadeSkin.Ink);
            }
            // A painted leather journal matches the brass medallion and the brigade's storybook pages.
            // Keep the existing hit region, placement and action; only the central artwork changes.
            Sprite brigadeBook = ArcadeSkin.KitSprite("HUD/BrigadeBook");
            if (brigadeBook != null)
            {
                Sprite(new Rect(details.x + 9 * k, details.y + 6 * k, 46 * k, 44 * k), brigadeBook);
            }
            else
            {
                ArcadeSkin.Line(new Rect(details.x + 16 * k, details.y + 12 * k, 33 * k, 28 * k), ArcadeSkin.Ink);
                ArcadeSkin.Line(new Rect(details.x + 19 * k, details.y + 15 * k, 27 * k, 22 * k), ArcadeSkin.Paper);
                ArcadeSkin.Line(new Rect(details.x + 31 * k, details.y + 15 * k, 3 * k, 22 * k), ArcadeSkin.Ink);
                for (int line = 0; line < 3; line++)
                {
                    ArcadeSkin.Line(new Rect(details.x + 22 * k, details.y + (18 + line * 6) * k, 6 * k, 2 * k), ArcadeSkin.Ink);
                    ArcadeSkin.Line(new Rect(details.x + 37 * k, details.y + (18 + line * 6) * k, 6 * k, 2 * k), ArcadeSkin.Ink);
                }
            }
            string hint = pause.Contains(Event.current.mousePosition) ? RunUIHints.Pause(RunUIHints.Device) : details.Contains(Event.current.mousePosition) ? RunUIHints.Details(RunUIHints.Device) : null;
            if (hint == null) return;
            Fill(new Rect(1400, 92, 435, 44), Panel);
            LocalizedGUI.Label(new Rect(1413, 99, 410, 40), hint, small);
        }

        private void ObserveMoments(ArenaController owner)
        {
            RunSnapshot state = owner.Session.State;
            CombatSimulation simulation = owner.Simulation;
            bool first = momentsRun != state.RunId;
            if (first) { momentsRun = state.RunId; bondLevels.Clear(); encoreCount = -1; seenBoss = null; bossWasAlive = false; }
            int encores = 0;
            UnitDefinition encoreUnit = null;
            foreach (RosterEntry entry in owner.Session.ActiveRoster) if (entry.Tier >= 3) { encores++; encoreUnit = owner.Content.Unit(entry.UnitId); }
            encoreCount = encores;
            foreach (BondDefinition bond in owner.Content.Bonds)
            {
                owner.Session.BondCounts.TryGetValue(bond.Id, out int count);
                int level = bond.ActiveLevel(count);
                if (bondLevels.TryGetValue(bond.Id, out int before) && level > before && !first)
                    cutIn.Show(bond.Name, "BOND LEVEL " + level + " · THEY STAND CLOSER TOGETHER", bond.Icon, ArcadeSkin.Teal, "ui.cutin.bond");
                bondLevels[bond.Id] = level;
            }
            if (state.Phase != SessionPhase.Combat) { bossWasAlive = false; return; }
            UnitState boss = simulation.Boss;
            if (boss != null && boss.Alive && boss != seenBoss)
            {
                seenBoss = boss;
                cutIn.Show(RunUICopy.UnitName(boss.Definition), "COMES OUT OF THE DARK", boss.Definition.Body, ArcadeSkin.Danger, "ui.cutin.boss");
            }
            bool alive = boss != null && boss.Alive;
            if (bossWasAlive && !alive && simulation.Champion.Alive)
                cutIn.Show("The Night Recedes", "DRIVEN BACK INTO THE DARK", owner.Content.Unit(state.ChampionId).Body, ArcadeSkin.Gold, "ui.cutin.showstopper");
            bossWasAlive = alive;
        }

        /// <summary>Real before/after numbers for the focused offer (cached; recomputed only when the build changes).</summary>
        private int BuildPreview(ArenaController owner, ShopOffer offer)
        {
            RunSnapshot state = owner.Session.State;
            string key = offer.Token + ":" + state.Gold + ":" + state.Eggs + ":" + state.Roster.Count + ":" + state.Items.Count + ":" + Mathf.RoundToInt(state.Health);
            if (key == previewKey) return previewRows.Count;
            previewKey = key; previewRows.Clear();
            if (offer.Kind == OfferKind.Companion)
            {
                using var preview = new RunSession(owner.Content); preview.Restore(state);
                if (!preview.Buy(offer.Token)) return 0;
                previewRows.Add(("Friends", owner.Session.ActiveRoster.Count.ToString(), preview.ActiveRoster.Count.ToString(), preview.ActiveRoster.Count >= owner.Session.ActiveRoster.Count));
                return previewRows.Count;
            }
            string text;
            try { text = RunOfferPreview.Capture(owner.Session, owner.Content, offer); } catch (System.Exception) { return 0; }
            foreach (string line in text.Split('\n'))
            {
                int arrow = line.IndexOf(" → ");
                int colon = arrow > 0 ? line.LastIndexOf(": ", arrow) : -1;
                if (arrow < 0 || colon < 0) continue;
                string name = line.Substring(0, colon), before = line.Substring(colon + 2, arrow - colon - 2), after = line.Substring(arrow + 3);
                if (name == "Companion roaming range")
                {
                    name = "Ranged roam";
                    before = before.Split('/')[0].Replace("Ranged ", "").Trim();
                    after = after.Split('/')[0].Replace("Ranged ", "").Trim();
                }
                bool lowerIsBetter = name.IndexOf("cycle", System.StringComparison.OrdinalIgnoreCase) >= 0 || name.IndexOf("cooldown", System.StringComparison.OrdinalIgnoreCase) >= 0;
                bool better = FirstNumber(after) >= FirstNumber(before) ^ lowerIsBetter;
                previewRows.Add(PlayerRow(name, before, after, better));
                if (previewRows.Count >= 3) break;
            }
            return previewRows.Count;
        }

        /// <summary>A preview row in player words (play-test 20/10/2026): short names, whole numbers where a decimal says nothing
        /// ("Hero health 127.0 / 127.0" reads "HP 127"), one decimal for seconds and small values.</summary>
        private static (string, string, string, bool) PlayerRow(string name, string before, string after, bool better)
        {
            bool health = name == "Hero health";
            string Words(string value)
            {
                if (health) { int slash = value.LastIndexOf(" / ", System.StringComparison.Ordinal); if (slash >= 0) value = value.Substring(slash + 3); }
                value = System.Text.RegularExpressions.Regex.Replace(value, @"-?\d+(\.\d+)?", m =>
                {
                    if (!float.TryParse(m.Value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float n)) return m.Value;
                    return Mathf.Abs(n) >= 10 || health ? Mathf.RoundToInt(n).ToString() : n.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);
                });
                return value.Replace(" / s", "/s").Replace(" s", "s");
            }
            switch (name)
            {
                case "Hero health": name = "HP"; break;
                case "Hero attack cycle": name = "Attacks every"; break;
                case "Hero damage": name = "Damage"; break;
                case "Hero critical chance": name = "Crit chance"; break;
                case "Hero projectile count": name = "Projectiles"; break;
                case "Hero armor": name = "Armor"; break;
                case "Movement speed": name = "Speed"; break;
                case "Passive regeneration": name = "Regen"; break;
                case "Pickup radius": name = "Pickup range"; break;
                default: if (name.StartsWith("Hero ", System.StringComparison.Ordinal)) name = char.ToUpperInvariant(name[5]) + name.Substring(6); break;
            }
            return (name, Words(before), Words(after), better);
        }

        private static float FirstNumber(string value)
        {
            int start = -1, end = -1;
            for (int index = 0; index < value.Length; index++)
            {
                bool digit = char.IsDigit(value[index]) || value[index] == '.' || value[index] == '-' && start < 0;
                if (digit && start < 0) start = index;
                if (!digit && start >= 0) { end = index; break; }
            }
            if (start < 0) return 0;
            float.TryParse(value.Substring(start, (end < 0 ? value.Length : end) - start), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float parsed);
            return parsed;
        }

        private void DrawPreview(Rect area)
        {
            if (compareStyle == null) { compareStyle = ArcadeTypography.Style(17, false, true); compareStyle.wordWrap = false; compareStyle.clipping = TextClipping.Clip; compareStyle.alignment = TextAnchor.MiddleLeft; }
            ArcadeSkin.Line(new Rect(area.x - 14, area.y, 2, area.height), new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, 0.3f));
            caption.alignment = TextAnchor.MiddleLeft;
            LocalizedGUI.Label(new Rect(area.x, area.y - 4, area.width, 20), "IF YOU BUY", caption);
            caption.alignment = TextAnchor.MiddleCenter;
            float row = Mathf.Min(28, (area.height - 18) / Mathf.Max(1, previewRows.Count));
            for (int index = 0; index < previewRows.Count; index++)
            {
                var entry = previewRows[index];
                Rect line = new Rect(area.x, area.y + 18 + index * row, area.width, row);
                compareStyle.normal.textColor = new Color(0.75f, 0.72f, 0.68f);
                string name = entry.name;
                int slash = name.LastIndexOf(" / ", System.StringComparison.Ordinal);
                if (slash >= 0) name = "Ally " + name.Substring(slash + 3);
                LocalizedGUI.Label(new Rect(line.x, line.y, 165, row), name, compareStyle);
                LocalizedGUI.Label(new Rect(line.x + 180, line.y, 70, row), entry.before, compareStyle);
                compareStyle.normal.textColor = entry.better ? ArcadeSkin.Better : ArcadeSkin.Worse;
                LocalizedGUI.Label(new Rect(line.x + 260, line.y, 130, row), (entry.better ? "▲ " : "▼ ") + entry.after, compareStyle);
            }
        }

        /// <summary>A bounded overview; the full bond list stays in the paused build details.</summary>
        private void DrawBonds(ArenaController owner)
        {
            bondFeedback.Observe(owner.Session.State.RunId, owner.Content.Bonds, owner.Session.BondCounts, owner.Simulation.VisualElapsed);
            if (programStyle == null) { programStyle = ArcadeTypography.Style(ArcadeTypography.SizeNote, true); programStyle.normal.textColor = ArcadeSkin.Ink; programStyle.wordWrap = false; programStyle.alignment = TextAnchor.MiddleLeft; }
            if (bondNumber == null) { bondNumber = ArcadeTypography.Style(ArcadeTypography.SizeNote, false, true); bondNumber.wordWrap = false; bondNumber.alignment = TextAnchor.MiddleRight; }
            bondSummary.Refresh(owner.Content.Bonds, owner.Session.BondCounts);
            // what buying the friend you stand at would do to the bonds: those it adds to blink gold, and new ones appear on top
            previewBonds.Clear(); newBonds.Clear();
            WorldShop.Station focus = owner.Session.State.Phase == SessionPhase.Shop ? owner.Shop?.Nearby : null;
            if (focus?.Offer != null && focus.Offer.Kind == OfferKind.Companion && !focus.Offer.Sold)
            {
                offerCard.Refresh(owner.Session, owner.Content, focus.Offer);
                foreach (var row in offerCard.Bonds)
                    if (row.Preview && row.After > row.Count)
                    {
                        previewBonds[row.Bond.Id] = row;
                        if (row.Count == 0 && newBonds.Count < 2) newBonds.Add(row);
                    }
            }
            if (bondSummary.Rows.Count == 0 && newBonds.Count == 0) return;
            if (owner.PresentationPaused) return; // a pause page covers the panel whole, never under its corner
            // review 31/10/2026: like ZZZ, a plain row of square tiles across the top right - each bond's icon in its tile, the count
            // under it; a tap names it and says what it does. Awake bonds glow and breathe, a bond this purchase lights blinks gold,
            // a count that changes pops its tile, and the tile under the finger lifts.
            // requirement 31/10/2026: two rows of at most five tiles (right-aligned), and a bond's level is shown by the light in its
            // tile (bronze, silver, gold), never by a number stamp
            float tile = BondHudSummary.RowHeight, gap = BondHudSummary.Gap, below = 30, rowStep = BondHudSummary.RowHeight + 36;
            const int perRow = BondHudSummary.PerRow;
            int shownRows = Mathf.Min(newBonds.Count + bondSummary.VisibleCount, perRow * 2 - 1);
            int tiles = shownRows + 1, lines = Mathf.CeilToInt(tiles / (float)perRow);
            Rect TileAt(int i)
            {
                int line = i / perRow, inLine = Mathf.Min(perRow, tiles - line * perRow), col = i % perRow;
                return new Rect(1900 - (inLine - col) * (tile + gap) + gap, 22 + line * rowStep, tile, tile);
            }
            float width = Mathf.Min(perRow, tiles) * (tile + gap) - gap;
            Rect card = new Rect(1900 - width, 22, width, (lines - 1) * rowStep + tile + below);
            bool still = owner.Profile != null && owner.Profile.ReducedMotion, calm = PresentationSettings.ReducedFlashes;
            float clock = Time.unscaledTime, blink = still || calm ? .6f : .5f + .5f * Mathf.Sin(clock * 7f);
            string tooltip = null; Rect tipFor = Rect.zero;
            if (bondHit == null) bondHit = new GUIStyle();
            bondNumber.alignment = TextAnchor.MiddleCenter;
            for (int index = 0; index < shownRows; index++)
            {
                bool fresh = index < newBonds.Count;
                BondDefinition bond = fresh ? newBonds[index].Bond : bondSummary.Rows[index - newBonds.Count];
                int count = fresh ? 0 : owner.Session.BondCounts[bond.Id];
                ShopDescriptionCard.BondRow row = default; bool previewing = !fresh && previewBonds.TryGetValue(bond.Id, out row);
                int after = fresh ? newBonds[index].After : previewing ? row.After : count;
                int level = bond.ActiveLevel(count), levelAfter = bond.ActiveLevel(after);
                bool rising = (fresh || previewing) && levelAfter > level, active = level > 0;
                Rect r = TileAt(index); float rowY = r.y;
                bool hover = r.Contains(Event.current.mousePosition);
                float burst = still ? 0 : bondFeedback.Burst(bond.Id, owner.Simulation.VisualElapsed);
                float lift = hover && !still ? 3 : 0, grow = burst * 4;
                r = new Rect(r.x - grow, r.y - grow - lift, r.width + grow * 2, r.height + grow * 2);
                // behind the tile: an awake bond breathes a warm light, a rising one blinks gold, a just-changed one flashes teal
                float breathe = still ? .5f : .5f + .5f * Mathf.Sin(clock * 2.2f + index * .9f);
                if (rising || fresh) ArcadeSkin.Glow(new Rect(r.x - 14, r.y - 14, r.width + 28, r.height + 28), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .35f + .45f * blink));
                else if (bondFeedback.Flashing(bond.Id, owner.Simulation.VisualElapsed)) ArcadeSkin.Glow(new Rect(r.x - 14, r.y - 14, r.width + 28, r.height + 28), new Color(ArcadeSkin.Teal.r, ArcadeSkin.Teal.g, ArcadeSkin.Teal.b, .7f));
                else if (active) ArcadeSkin.Glow(new Rect(r.x - 10, r.y - 10, r.width + 20, r.height + 20), new Color(1f, .74f, .4f, .2f + .2f * breathe));
                if (!ArcadeSkin.KitSliced(r, active || rising ? "Info/NavyTileLit" : "Info/NavyTile", .45f)) ArcadeSkin.PortraitSlot(r, active || rising); // a square tile
                // the level is the light behind the icon: bronze at the first step (2), silver at the second (4), gold at the third (6)
                int shownLevel = rising ? levelAfter : level;
                if (shownLevel > 0)
                {
                    Color tierLight = shownLevel == 1 ? new Color(.95f, .55f, .25f) : shownLevel == 2 ? new Color(.7f, .88f, 1f) : new Color(1f, .86f, .35f);
                    Rect inner = new Rect(r.x + r.width * .1f, r.y + r.height * .1f, r.width * .8f, r.height * .8f);
                    for (int layer = 0; layer < shownLevel; layer++) ArcadeSkin.Glow(inner, new Color(tierLight.r, tierLight.g, tierLight.b, .45f + .25f * breathe));
                }
                Color keep = GUI.color;
                if (!active && !rising) GUI.color = new Color(keep.r * .62f, keep.g * .62f, keep.b * .66f, keep.a); // not awake yet: dim
                ShopDescriptionCard.DrawSprite(new Rect(r.x + r.width * .18f, r.y + r.height * .18f, r.width * .64f, r.height * .64f), bond.Icon);
                GUI.color = keep;
                int next = Mathf.Max(after, bond.NextThreshold(after));
                bondNumber.normal.textColor = rising ? Color.Lerp(ArcadeSkin.Gold, ArcadeSkin.Cream, blink * .3f) : active ? ArcadeSkin.Cream : new Color(.8f, .76f, .7f);
                ArcadeTypography.ShadowLabel(new Rect(r.x - 10, rowY + tile + 2, tile + 20, below - 2), after + "/" + next, bondNumber);
                if (TouchTargets.Button(new Rect(r.x, r.y, r.width, tile + below), GUIContent.none, bondHit)) { selectedBond = bond.Id; bondTooltipUntil = Time.unscaledTime + 3; }
                if (hover || selectedBond == bond.Id && Time.unscaledTime < bondTooltipUntil)
                {
                    tooltip = RunUICopy.BondDescription(bond, count); // its first line names the bond and its level, the second the count
                    tipFor = r;
                }
            }
            // the rest: one tile that opens the full list
            Rect more = TileAt(shownRows);
            // a round brass button, not another square bond tile (review 3/11/2026: it read as a bond, not as a button)
            Sprite roundButton = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite("Buttons/RoundBrass");
            if (roundButton != null) Sprite(new Rect(more.x - 4, more.y - 4, more.width + 8, more.height + 8), roundButton);
            else if (!ArcadeSkin.KitSliced(more, "Info/NavyTile", .45f)) ArcadeSkin.PortraitSlot(more, false);
            programStyle.alignment = TextAnchor.MiddleCenter; programStyle.normal.textColor = roundButton != null ? ArcadeSkin.Ink : ArcadeSkin.Cream; // dark ink on the brass
            int hidden = bondSummary.Rows.Count - (shownRows - newBonds.Count); // the bonds not on the two rows
            LocalizedGUI.Label(more, hidden > 0 ? "+" + hidden : "···", programStyle);
            ArcadeTypography.ShadowLabel(new Rect(more.x - 10, more.y + tile + 2, tile + 20, below - 2), "all", bondNumber);
            if (TouchTargets.Button(new Rect(more.x, more.y, more.width, tile + below), GUIContent.none, bondHit))
            {
                if (!owner.Details.Open) owner.ToggleDetails();
                owner.Details.SelectBond(bondSummary.Rows[0].Name);
            }
            programStyle.normal.textColor = ArcadeSkin.Ink; programStyle.alignment = TextAnchor.MiddleLeft;
            bondNumber.alignment = TextAnchor.MiddleRight;
            small.normal.textColor = ArcadeSkin.Cream; small.alignment = TextAnchor.UpperLeft;
            if (tooltip != null)
            {
                // under the row, never off the screen's right edge
                // wide enough that a stat and its value share a line (at 420 the value wrapped alone)
                const float TipWidth = 560;
                float height = GlossaryUI.Height(tooltip, small, TipWidth - 44) + 32, x = Mathf.Min(tipFor.x, 1900 - TipWidth);
                Rect box = new Rect(x, card.yMax + 10, TipWidth, height);
                // dark ink on the painted cream tooltip (cream words on cream paper could not be read); cream on the code panel
                bool paper = ArcadeSkin.KitSliced(box, "Panels/Tooltip", .4f);
                if (!paper) Fill(box, Panel);
                small.normal.textColor = paper ? ArcadeSkin.KitInk : ArcadeSkin.Cream;
                GlossaryUI.Text(new Rect(box.x + 22, box.y + 16, TipWidth - 44, height - 32), tooltip, small);
                small.normal.textColor = ArcadeSkin.Cream;
                GlossaryUI.Draw(new Vector2(1920, 1080));
            }
        }

        private readonly Dictionary<string, ShopDescriptionCard.BondRow> previewBonds = new Dictionary<string, ShopDescriptionCard.BondRow>();
        private readonly List<ShopDescriptionCard.BondRow> newBonds = new List<ShopDescriptionCard.BondRow>();
        /// <summary>One bond row's icon, name, count and progress: an inactive bond is drawn grey; a previewed one shows count → after.</summary>
        private void BondRowContent(Rect line, BondDefinition bond, int count, int after, bool active, bool previewing, bool activates, float blink, float burst)
        {
            Color previous = GUI.color;
            bool grey = !active && !(previewing && activates);
            if (grey) GUI.color = new Color(.62f, .62f, .66f, previewing ? .65f + .3f * blink : .55f);
            Sprite(new Rect(line.x + 2 - burst * 2, line.y + (line.height - 36) * .5f - burst * 2, 36 + burst * 4, 36 + burst * 4), bond.Icon);
            GUI.color = previous;
            programStyle.normal.textColor = grey ? new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .5f) : ArcadeSkin.Ink;
            float countRight = active || activates ? line.xMax - 78 : line.xMax - 6; // the count ends before the level stamp
            // the words sit in the row's middle with room above and below (they touched the row's top), the bar under them
            Rect words = new Rect(line.x + 48, line.y + 6, countRight - line.x - 48 - 70, line.height - 18);
            LocalizedGUI.Label(words, bond.Name, programStyle);
            int next = Mathf.Max(after, bond.NextThreshold(after));
            bondNumber.normal.textColor = programStyle.normal.textColor;
            LocalizedGUI.Label(new Rect(countRight - 70, words.y, 70, words.height), previewing && after != count ? after + "/" + next : count + "/" + Mathf.Max(count, bond.NextThreshold(count)), bondNumber);
            programStyle.normal.textColor = ArcadeSkin.Ink;
            Rect progress = new Rect(line.x + 48, line.yMax - 9, line.width - 56, 4);
            ArcadeSkin.Line(progress, new Color(0, 0, 0, .12f));
            int shown = Mathf.Max(count, bond.NextThreshold(count));
            if (previewing && after > count) ArcadeSkin.Line(new Rect(progress.x, progress.y, progress.width * Mathf.Clamp01((float)after / next), progress.height), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .4f + .5f * blink));
            ArcadeSkin.Line(new Rect(progress.x, progress.y, progress.width * Mathf.Clamp01((float)count / shown), progress.height), active ? ArcadeSkin.Teal : grey ? new Color(.5f, .5f, .55f) : ArcadeSkin.Gold);
        }

        private GUIStyle fallenStyle, bondNumber, identityLine;

        /// <summary>B9: after the light goes out, someone in the brigade says one short thing, in their own hand.</summary>
        private void DrawFallenWord(ArenaController owner, RunSnapshot state, Rect area)
        {
            UnitDefinition speaker = null;
            if (state.Roster != null && state.Roster.Count > 0)
            {
                int hash = 7;
                foreach (char c in state.RunId ?? "") hash = hash * 31 + c;
                speaker = owner.Content.Unit(state.Roster[(hash & 0x7fffffff) % state.Roster.Count].UnitId);
            }
            string line = speaker != null ? CompanionVoice.Comforting(state.RunId, speaker.Id) : "I'll find the door again. It was there before; it will be there again.";
            if ((state.ActOne?.SitCloser ?? 0) == ActOneProgress.Lost)
            {
                // the warm chair kept them: a friend speaks for everyone who pulled
                line = "We pulled as hard as we could. The Keeper had to tear you out of it. Next time, we pull sooner.";
            }
            Sprite face = speaker != null ? speaker.Body : owner.Content.Unit(state.ChampionId).Body;
            ArcadeSkin.Ticket(area, ArcadeSkin.Paper, ArcadeSkin.Accent);
            Sprite(new Rect(area.x + 12, area.y + 8, 76, area.height - 18), face);
            if (handStyle == null) { handStyle = ArcadeTypography.Style(24); handStyle.font = ArcadeTypography.Hand; handStyle.wordWrap = false; handStyle.clipping = TextClipping.Clip; }
            if (fallenStyle == null) { fallenStyle = new GUIStyle(handStyle) { fontSize = 27, wordWrap = true, alignment = TextAnchor.MiddleLeft }; fallenStyle.normal.textColor = new Color(0.24f, 0.19f, 0.13f); }
            LocalizedGUI.Label(new Rect(area.x + 100, area.y + 6, area.width - 116, area.height - 16), "“" + line + "”", fallenStyle);
        }

        private void DrawReceipt(RunSnapshot state)
        {
            if (state.Receipts.Count == 0) return;
            RunTransactionReceipt receipt = state.Receipts[state.Receipts.Count - 1];
            if (receiptId != receipt.Id) { receiptId = receipt.Id; receiptScroll = Vector2.zero; receiptOpen = false; }
            if (navigation.Button(new Rect(1460, 205, 370, 52), receiptOpen ? "CLOSE RECEIPT" : "RECEIPT SAVED", button))
            {
                receiptOpen = !receiptOpen;
                CheckpointAudio.PlayInterfaceId(receiptOpen ? "ui.receipt" : "ui.close", 0.08f);
            }
            if (!receiptOpen) return;
            float contentHeight = small.CalcHeight(LocalizedGUI.Content(receipt.Description), 425) + 20;
            float panelHeight = Mathf.Clamp(contentHeight + 110, 190, 470);
            Fill(new Rect(1320, 275, 510, panelHeight), Color.Lerp(Panel, ArcadeSkin.Accent, 0.12f));
            ArcadeSkin.Divider(new Rect(1340, 342, 460, 2), ArcadeSkin.Accent);
            LocalizedGUI.Label(new Rect(1340, 295, 465, 45), receipt.Title, text);
            float height = Mathf.Max(panelHeight - 105, contentHeight);
            receiptScroll = ScrollPanel.Begin(new Rect(1340, 355, 470, panelHeight - 105), receiptScroll, new Rect(0, 0, 430, height));
            LocalizedGUI.Label(new Rect(0, 0, 425, height), receipt.Description, small);
            ScrollPanel.End();
        }

        public void DrawMenuConfirmation(ArenaController owner, Camera camera)
        {
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            navigation.Begin();
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            Fill(new Rect(470, 310, 980, 440), Panel);
            LocalizedGUI.Label(new Rect(520, 345, 880, 80), "Return to the Waiting Room?", title);
            LocalizedGUI.Label(new Rect(540, 445, 840, 105), "Save and return keeps this run where it stands. Leave without saving takes it back to its last save: the start of this wave, or your last shop purchase.", text);
            if (!string.IsNullOrEmpty(owner.SaveError)) LocalizedGUI.Label(new Rect(540, 550, 840, 70), SaveNote(owner), small);
            // requirement 24/10/2026: a way out without saving, beside saving and staying
            if (navigation.Button(new Rect(510, 650, 280, 65), "STAY", button)) owner.CancelMenuReturn();
            if (navigation.Button(new Rect(820, 650, 280, 65), "LEAVE, DON'T SAVE", button)) owner.ExitWithoutSaving();
            if (navigation.Button(new Rect(1130, 650, 280, 65), "SAVE AND RETURN", primary ?? button)) owner.ReturnToMenu();
            navigation.End();
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }
    }
}
