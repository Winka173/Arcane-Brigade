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
        private float shownGold, shownEggs, nextWalletTick, nextHeartbeat, heartbeatAt = -10;
        private void DrawPause(ArenaController owner)
        {
            if (pausePage == 1)
            {
                ArcadeSkin.Frame(new Rect(330, 150, 1260, 800), KitFrame.Dialogue, .6f);
                ArcadeSkin.Header(new Rect(390, 190, 900, 92), "THE NIGHT WAITS WITH YOU", "Light, Sound & Comfort");
                if (navigation.Button(new Rect(1330, 205, 200, 50), "BACK", button)) pausePage = 0;
                SettingsSheet.Draw(owner.Profile ?? new PlayerProfile(), new Vector2(320, 180), (r, c) => navigation.Button(r, c, button), (r, c) => GUI.Label(r, ArcadeTypography.DisplayText(c), text), owner.SaveSettings, true);
                return;
            }
            if (pausePage == 2)
            {
                ArcadeSkin.Frame(new Rect(600, 330, 720, 400), KitFrame.Dialogue, .6f);
                ArcadeSkin.Header(new Rect(660, 380, 600, 100), "THE NIGHT WAITS WITH YOU", "Start over?");
                GUI.Label(new Rect(660, 490, 600, 80), "This night ends here and is not recorded.\nA new night begins with the same hero.", text);
                if (navigation.Button(new Rect(660, 600, 280, 65), "KEEP PLAYING", primary)) pausePage = 0;
                if (navigation.Button(new Rect(980, 600, 280, 65), "START OVER", dangerButton ??= ArcadeSkin.DangerStyle(button))) { pausePage = 0; owner.RestartRun(); }
                return;
            }
            if (pausePage == 4)
            {
                ArcadeSkin.Frame(new Rect(240, 120, 1440, 860), KitFrame.Dialogue, .6f);
                ArcadeSkin.Header(new Rect(300, 150, 900, 92), "THE NIGHT WAITS WITH YOU", "Combos");
                if (navigation.Button(new Rect(1420, 165, 200, 64), "BACK", button)) pausePage = 0;
                DrawCombos(owner, new Rect(300, 270, 1320, 680));
                return;
            }
            if (pausePage == 3)
            {
                // How to play: six cards, one line each, any time
                ArcadeSkin.Frame(new Rect(240, 120, 1440, 860), KitFrame.Dialogue, .6f);
                ArcadeSkin.Header(new Rect(300, 150, 900, 92), "THE NIGHT WAITS WITH YOU", "How to Play");
                if (navigation.Button(new Rect(1420, 165, 200, 64), "BACK", button)) pausePage = 0;
                guide.DrawHowToPlay(new Rect(300, 270, 1320, 680), owner);
                return;
            }
            ArcadeSkin.Frame(new Rect(640, 230, 640, 680), KitFrame.Dialogue, .6f);
            ArcadeSkin.Header(new Rect(700, 290, 520, 100), "THE NIGHT WAITS WITH YOU", "Paused");
            if (navigation.Button(new Rect(750, 410, 420, 65), "RESUME", primary)) owner.TogglePause();
            if (navigation.Button(new Rect(700, 490, 250, 64), "HOW TO PLAY", button)) pausePage = 3;
            if (navigation.Button(new Rect(970, 490, 250, 64), "COMBOS", button)) { pausePage = 4; comboScroll = Vector2.zero; }
            if (navigation.Button(new Rect(700, 570, 250, 64), "STATS", button)) { owner.ToggleDetails(); owner.Details.SelectTab(0); }
            if (navigation.Button(new Rect(970, 570, 250, 64), "RELICS", button)) { owner.ToggleDetails(); owner.Details.SelectTab(2); }
            if (navigation.Button(new Rect(700, 650, 250, 64), "SETTINGS", button)) pausePage = 1;
            if (navigation.Button(new Rect(970, 650, 250, 64), "START OVER", button)) pausePage = 2;
            if (navigation.Button(new Rect(750, 740, 420, 64), "MAIN MENU", button)) owner.RequestMenuReturn();
        }

        public void ReadNavigation(bool enabled) { RunUIHints.Refresh(); if (enabled) navigation.ReadInput(); }
        private Vector2 comboScroll; private GUIStyle comboLine;
        /// <summary>The Combos page (play-test 20/10/2026, item 26): every duet this hero's shop can offer. Pairs you have met
        /// show both friends and what they do together (lit while both are in the brigade); the rest are silhouettes.</summary>
        private void DrawCombos(ArenaController owner, Rect area)
        {
            RunSession session = owner.Session; RunCatalog catalog = owner.Content;
            UnitDefinition hero = catalog.Unit(session.State.ChampionId);
            var list = new System.Collections.Generic.List<Duet>();
            foreach (Duet d in Duets.All)
            {
                UnitDefinition a = catalog.Unit(d.A), b = catalog.Unit(d.B);
                if (a == null || b == null || a.GiftOnly || b.GiftOnly) continue;
                if (catalog.HeroRevision > 0 && hero != null && (!K11HeroRules.Eligible(hero, a) || !K11HeroRules.Eligible(hero, b))) continue;
                list.Add(d);
            }
            list.Sort((x, y) => Known(session, y).CompareTo(Known(session, x)));
            comboLine ??= new GUIStyle(text) { fontSize = 22, wordWrap = true };
            const float row = 120;
            comboScroll = ScrollPanel.Begin(area, comboScroll, new Rect(0, 0, area.width - 20, Mathf.Max(area.height, list.Count * row)));
            for (int i = 0; i < list.Count; i++)
            {
                Duet d = list[i]; int known = Known(session, d); bool active = Duets.Active(session, d);
                Rect r = new Rect(0, i * row, area.width - 24, row - 10);
                ArcadeSkin.Line(r, active ? new Color(.95f, .76f, .33f, .22f) : new Color(1, 1, 1, .05f));
                Sprite(new Rect(r.x + 8, r.y + 8, 94, 94), Met(session, d.A) ? catalog.Unit(d.A).Body : CompanionRoles.UnknownBadge);
                Sprite(new Rect(r.x + 110, r.y + 30, 52, 52), CompanionRoles.ComboBadge);
                Sprite(new Rect(r.x + 170, r.y + 8, 94, 94), Met(session, d.B) ? catalog.Unit(d.B).Body : CompanionRoles.UnknownBadge);
                comboLine.normal.textColor = active ? ArcadeSkin.Gold : known > 1 ? ArcadeSkin.Cream : new Color(.7f, .68f, .64f);
                string names = known > 1 ? RunUICopy.UnitName(catalog.Unit(d.A)) + " + " + RunUICopy.UnitName(catalog.Unit(d.B)) : "? + ?";
                GUI.Label(new Rect(r.x + 284, r.y + 10, r.width - 300, r.height - 20), names + (active ? "  ·  ACTIVE" : "") + "\n" + (known > 1 ? d.Line : "Meet both friends to find this combo."), comboLine);
            }
            ScrollPanel.End();
        }
        /// <summary>How many of a duet's two friends you have had this run (0, 1 or 2).</summary>
        private static int Known(RunSession session, Duet d) => (Met(session, d.A) ? 1 : 0) + (Met(session, d.B) ? 1 : 0);
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

        public static void Fill(Rect rectangle, Color color)
        {
            if (rectangle.height >= 35f && rectangle.width >= 55f) { ArcadeSkin.Panel(rectangle, color); return; }
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rectangle, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        public static void Sprite(Rect rectangle, Sprite sprite)
        {
            if (sprite == null) return;
            Rect source = sprite.rect;
            float ratio = source.width / source.height;
            float width = Mathf.Min(rectangle.width, rectangle.height * ratio);
            float height = width / ratio;
            rectangle = new Rect(rectangle.center.x - width * 0.5f, rectangle.center.y - height * 0.5f, width, height);
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
                text = ArcadeTypography.Style(23);
                small = new GUIStyle(text) { fontSize = 18 };
                title = ArcadeTypography.Style(36, true);
                if (ArcadeTypography.Display != null) title.font = ArcadeTypography.Display;
                title.alignment = TextAnchor.MiddleCenter;
                button = new GUIStyle(GUI.skin.button) { fontSize = 22, font = ArcadeTypography.Heading };
                numeric = ArcadeTypography.Style(20, false, true);
                numeric.alignment = TextAnchor.MiddleCenter;
                numericLeft = new GUIStyle(numeric) { alignment = TextAnchor.MiddleLeft, fontSize = 24 };
                caption = ArcadeTypography.Style(13, false, true);
                caption.alignment = TextAnchor.MiddleCenter;
                caption.normal.textColor = ArcadeSkin.Teal;
                primary = ArcadeSkin.Primary(button);
                bossName = ArcadeTypography.Style(22, true);
                bossName.alignment = TextAnchor.MiddleLeft;
                bossName.normal.textColor = ArcadeSkin.Ink;
                tagStyle = ArcadeTypography.Style(16, true); tagStyle.font = ArcadeTypography.Punch;
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
            if (walletRun != state.RunId) { walletRun = state.RunId; healthTrail = health; lastGold = gold; lastEggs = eggs; goldChanged = eggsChanged = -100; shownGold = gold; shownEggs = eggs; }
            // P2: the counters run up to the new total (a soft tick per step while gold comes in); spending runs down silently
            if (Event.current.type == EventType.Repaint)
            {
                float step = Time.unscaledDeltaTime;
                float rise = gold - shownGold;
                shownGold = PresentationSettings.ReducedMotion ? gold : Mathf.MoveTowards(shownGold, gold, Mathf.Max(8 * step, Mathf.Abs(rise) * 10 * step));
                shownEggs = PresentationSettings.ReducedMotion ? eggs : Mathf.MoveTowards(shownEggs, eggs, Mathf.Max(6 * step, Mathf.Abs(eggs - shownEggs) * 10 * step));
                if (rise >= 1 && !PresentationSettings.ReducedAudio && Time.unscaledTime >= nextWalletTick) { nextWalletTick = Time.unscaledTime + 0.07f; CheckpointAudio.PlayInterfaceId("loot.coin.bounce"); }
            }
            if (lastGold != gold) { goldChanged = simulation.VisualElapsed; lastGold = gold; }
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
            if (bossFlash > 0.01f) Fill(new Rect(0, 0, 1920, 1080), new Color(1f, 0.96f, 0.86f, bossFlash * 0.55f)); // a boss falls: the night flashes white
            if (Event.current.type == EventType.Repaint) ObserveMoments(owner);
            if (PresentationSettings.ReducedMotion || health > healthTrail) healthTrail = health;
            else if (Event.current.type == EventType.Repaint && simulation.Phase != RunPhase.Paused) healthTrail = Mathf.MoveTowards(healthTrail, health, Mathf.Min(Time.unscaledDeltaTime, 0.05f) * maximum * 0.5f);
            if (ArcadeSkin.HasKit)
            {
                // the painted HUD (UI kit, item 13): the hero in a brass portrait slot, the health bar with its heart cap and crimson fill
                ArcadeSkin.PortraitSlot(new Rect(14, 8, 92, 92), false);
                Sprite(new Rect(31, 25, 58, 58), owner.Content.Unit(state.ChampionId).Body);
                NightHud.LightRing(new Vector2(60, 55), 40, CompanionDeployment.Select(state.Roster).Count + ActOneCanon.StoryCount(state.Wave, state.Phase == SessionPhase.Shop, state.Endless));
                Rect tube = ArcadeSkin.HealthBar(new Rect(100, 24, 300, 64), Mathf.Clamp01(health / maximum), Mathf.Clamp01(healthTrail / maximum));
                numeric.normal.textColor = ArcadeSkin.Cream;
                ArcadeTypography.ShadowLabel(new Rect(tube.x, tube.y - 4, tube.width, tube.height + 8), Mathf.CeilToInt(health) + " / " + Mathf.CeilToInt(maximum), numeric);
            }
            else
            {
            ArcadeSkin.Plate(new Rect(22, 16, 340, 72), ArcadeSkin.Accent);
            Sprite(new Rect(35, 20, 62, 62), owner.Content.Unit(state.ChampionId).Body);
            NightHud.LightRing(new Vector2(66, 51), 40, CompanionDeployment.Select(state.Roster).Count + ActOneCanon.StoryCount(state.Wave, state.Phase == SessionPhase.Shop, state.Endless));
            ArcadeSkin.Line(new Rect(105, 36, 230, 30), new Color(0.02f, 0.02f, 0.05f, 0.95f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(healthTrail / maximum), 24), new Color(0.55f, 0.30f, 0.14f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(health / maximum), 24), new Color(1f, 0.70f, 0.36f)); // your light, not the hostile red
            numeric.normal.textColor = health / maximum > 0.45f ? ArcadeSkin.Ink : ArcadeSkin.Cream; // ink reads on the lantern bar
            GUI.Label(new Rect(108, 36, 224, 30), Mathf.CeilToInt(health) + " / " + Mathf.CeilToInt(maximum), numeric);
            Sprite(new Rect(92, 31, 38, 38), owner.Content.HeartSprite);
            }
            numeric.normal.textColor = ArcadeSkin.Cream;
            if (owner.Content.HeroRevision > 0 && state.Phase == SessionPhase.Combat)
            {
                float required = simulation.HeroIdentityEnabled ? simulation.HeroIdentityRequired : simulation.Champion.Definition.HeroMechanic == HeroMechanic.LanternStride ? 12 : simulation.Champion.Definition.HeroMechanic == HeroMechanic.ParcelExpress ? 6 : 2;
                Rect charge = ArcadeSkin.HasKit ? new Rect(160, 90, 210, 6) : new Rect(108, 72, 224, 5);
                ArcadeSkin.Line(charge, new Color(0.06f, 0.07f, 0.11f, 0.85f));
                ArcadeSkin.Line(new Rect(charge.x, charge.y, charge.width * Mathf.Clamp01((simulation.HeroIdentityEnabled ? simulation.HeroIdentityCharge : simulation.HeroCharge) / required), charge.height), simulation.HeroIdentityEnabled ? HeroIdentities.For(state.ChampionId).Color : ArcadeSkin.Teal);
                if (simulation.HeroIdentityEnabled)
                {
                    // drawn straight over the floor: a thick name and a shadow under both lines so they read on any tile
                    if (identityStyle == null) { identityStyle = new GUIStyle(small) { font = ArcadeTypography.Punch, fontSize = 22, wordWrap = false }; }
                    identityStyle.normal.textColor = Color.Lerp(HeroIdentities.For(state.ChampionId).Color, ArcadeSkin.Cream, 0.55f);
                    ArcadeTypography.ShadowLabel(new Rect(380, 108, 440, 28), HeroIdentities.For(state.ChampionId).Name, identityStyle);
                    ArcadeTypography.ShadowLabel(new Rect(380, 140, 440, 42), simulation.HeroIdentityStatus, small);
                }
            }
            float goldPulse = LivingUI.Press(simulation.VisualElapsed - goldChanged, PresentationSettings.ReducedMotion) * 4;
            float eggPulse = LivingUI.Press(simulation.VisualElapsed - eggsChanged, PresentationSettings.ReducedMotion) * 4;
            // walnut counter plates with a round socket for the coin and the egg (UI kit)
            Rect goldSocket = ArcadeSkin.Counter(new Rect(16, 100, 200, 44), KitCounter.Gold, out Rect goldText);
            Rect eggSocket = ArcadeSkin.Counter(new Rect(16, 146, 200, 44), KitCounter.Egg, out Rect eggText);
            Sprite(new Rect(goldSocket.x - goldPulse, goldSocket.y - goldPulse, goldSocket.width + goldPulse * 2, goldSocket.height + goldPulse * 2), owner.Content.Arena.Coin); // centred in the socket
            numericLeft.normal.textColor = Color.Lerp(ArcadeSkin.Cream, ArcadeSkin.Gold, goldPulse / 4f);
            GUI.Label(goldText, Mathf.RoundToInt(shownGold).ToString("N0"), numericLeft);
            Sprite(new Rect(eggSocket.x - eggPulse, eggSocket.y - eggPulse, eggSocket.width + eggPulse * 2, eggSocket.height + eggPulse * 2), owner.Content.EggSprite);
            numericLeft.normal.textColor = Color.Lerp(ArcadeSkin.Cream, ArcadeSkin.Teal, eggPulse / 4f);
            GUI.Label(eggText, Mathf.RoundToInt(shownEggs).ToString("N0"), numericLeft);
            Rect waveRect = state.Phase == SessionPhase.Shop ? new Rect(380, 24, 280, 52) : new Rect(820, 22, 280, 52);
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(waveRect, KitFrame.WaveBanner); else ArcadeSkin.Plate(waveRect, ArcadeSkin.Accent);
            GUI.Label(ArcadeSkin.HasKit ? ArcadeSkin.Content(waveRect, KitFrame.WaveBanner, false) : new Rect(waveRect.x + 10, waveRect.y + 4, waveRect.width - 10, 44), (state.Endless ? "∞ " : "") + "Wave " + state.Wave, title);
            if (state.Phase == SessionPhase.Combat)
            {
                bool overtime = simulation.IsBossOvertime;
                float remaining = simulation.WaveDuration - simulation.Elapsed;
                Rect timer = new Rect(880, 80, 160, 40);
                ArcadeSkin.Plate(timer, overtime ? ArcadeSkin.Danger : remaining <= 5 ? ArcadeSkin.Gold : ArcadeSkin.Teal);
                GUI.Label(new Rect(timer.x + 14, timer.y, 50, timer.height), overtime ? "" : "TIME", caption);
                numeric.normal.textColor = overtime ? ArcadeSkin.Danger : remaining <= 5 ? ArcadeSkin.Gold : ArcadeSkin.Cream;
                GUI.Label(new Rect(timer.x + (overtime ? 10 : 56), timer.y + 2, overtime ? 140 : 90, timer.height - 4), CombatTimerCaption(overtime, remaining), numeric);
                numeric.normal.textColor = ArcadeSkin.Cream;
            }
            // the eight lanterns sit quietly under the wallet; the old wave road is gone
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) NightHud.Lanterns(new Rect(22, 188, 190, 34), state);
            if (state.Phase == SessionPhase.Combat) { NightHud.LampLoss(new Rect(560, 150, 800, 60)); NightHud.Knocking(new Rect(610, 130, 700, 48), simulation.KnockRemaining); }
            DrawControls(owner);
            if (state.Phase == SessionPhase.Combat)
            {
                if (simulation.Boss != null && simulation.Boss.Alive)
                {
                    float bossShare = Mathf.Clamp01(simulation.Boss.Health / simulation.Boss.Definition.Health);
                    string bossTitle = RunUICopy.UnitName(simulation.Boss.Definition).ToUpperInvariant();
                    float bossTitleWidth = Mathf.Clamp(bossName.CalcSize(new GUIContent(bossTitle)).x + 16, 350, 990);
                    ArcadeSkin.Slant(new Rect(420, 908, bossTitleWidth + 70, 40), ArcadeSkin.Danger);
                    Sprite(new Rect(432, 906, 42, 42), simulation.Boss.Definition.Body);
                    GUI.Label(new Rect(480, 908, bossTitleWidth, 40), bossTitle, bossName);
                    ArcadeSkin.Line(new Rect(424, 954, 1076, 40), new Color(0.01f, 0.01f, 0.02f, 0.75f));
                    ArcadeSkin.Line(new Rect(420, 950, 1080, 38), ArcadeSkin.Ink);
                    ArcadeSkin.Line(new Rect(425, 955, 1070 * bossShare, 28), ArcadeSkin.Danger);
                    ArcadeSkin.Line(new Rect(425, 955, 1070 * bossShare, 6), new Color(1f, 0.55f, 0.42f));
                    ArcadeSkin.Line(new Rect(425 + 1070 * 0.5f - 2, 948, 4, 42), ArcadeSkin.Cream);
                    GUI.Label(new Rect(425, 950, 1070, 38), Mathf.CeilToInt(simulation.Boss.Health) + " / " + Mathf.CeilToInt(simulation.Boss.Definition.Health), numeric);
                }
                if (simulation.Phase == RunPhase.Paused) DrawPause(owner);
                else pausePage = 0;
            }
            else if (state.Phase == SessionPhase.Shop)
            {
                // a visitor's card owns the screen: no tickets or station card over it
                if (owner.GetComponent<VisitorCard>()?.Open != true && owner.GetComponent<LucidAltar>()?.Open != true) { owner.Shop.DrawPriceTags(camera); DrawShop(owner); }
                statFeedback.Draw(owner,camera);
            }
            else
            {
                if (!owner.ResultsReady)
                {
                    if (owner.Session.EndingChoicePending) { GUI.matrix = previous; GUI.skin = previousSkin; navigation.End(); return; }
                    ArcadeSkin.Plate(new Rect(560, 840, 800, 70), state.Phase == SessionPhase.Defeat ? ArcadeSkin.Danger : ArcadeSkin.Gold);
                    bool satDown = (state.ActOne?.SitCloser ?? 0) == ActOneProgress.Lost;
                    GUI.Label(new Rect(550, 840, 820, 70), state.Phase == SessionPhase.Defeat ? satDown ? "You sat down." : "Your light goes out…" : "Dawn has come…", title);
                    GUI.matrix = previous;
                    GUI.skin = previousSkin;
                    navigation.End();
                    return;
                }
                Fill(new Rect(80, 200, 630, 750), Panel);
                ArcadeSkin.Header(new Rect(110, 228, 575, 105), "THE NIGHT IS RECORDED", state.Phase == SessionPhase.Victory ? "Dawn Has Come" : (state.ActOne?.SitCloser ?? 0) == ActOneProgress.Lost ? "You Sat Down" : "Until You Return");
                bool fallen = state.Phase == SessionPhase.Defeat;
                Sprite(fallen ? new Rect(240, 345, 230, 200) : new Rect(220, 355, 270, 270), owner.Content.Unit(state.ChampionId).Body);
                Sprite(fallen ? new Rect(450, 450, 90, 90) : new Rect(475, 480, 100, 100), owner.Content.Unit(state.ChampionId).Weapon.Sprite);
                if (fallen) DrawFallenWord(owner, state, new Rect(115, 552, 565, 96));
                GUI.Label(new Rect(115, 655, 565, 90), owner.MetaNotice, small);
                if (state.Phase == SessionPhase.Victory && navigation.Button(new Rect(115, 750, 560, 60), "ENDLESS · KEEP BRIGADE AND RELICS", button)) owner.ContinueEndless();
                if (navigation.Button(new Rect(115, 825, 270, 65), "PLAY AGAIN", primary)) owner.RestartRun();
                // the Clock's Rewind (play-test item 27): once a run, back to the shop before the lost wave
                if (fallen && owner.Session.RewindReady && navigation.Button(new Rect(115, 750, 560, 60), "REWIND · BACK TO WAVE " + owner.Session.RewindWave, primary)) owner.RewindRun();
                if (navigation.Button(new Rect(405, 825, 270, 65), "MAIN MENU", button)) owner.ReturnToMenu();
                invoice.Draw(new Rect(770, 200, 1060, 750), state, owner.Content);
            }
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(65, 130, 1800, 70), owner.SaveError, text);
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) DrawBonds(owner);
            waveStamp.Observe(state.RunId, state.Wave, state.Phase == SessionPhase.Combat && !owner.TransitionPending);
            if (state.Phase == SessionPhase.Combat && !owner.TransitionPending) waveStamp.Draw((state.Endless ? "∞ " : "") + "WAVE " + state.Wave);
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) { coach.Draw(owner, camera); guide.Draw(owner); }
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
                GUI.Label(new Rect(550, 720, 820, 70), "The dark draws back", title);
                return;
            }
            WorldShop shop = owner.Shop;
            if (shop == null) return;
            shop.DrawMerchantDialogue();
            if (owner.Simulation.Phase == RunPhase.Paused) { DrawPause(owner); return; }
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
                    if (navigation.Button(go, shop.Special ? "TO THE SHOP  →" : "NEXT WAVE  ↓", primary)) shop.Interact(); // the dream shop leads on to the regular shop (owner)
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
                Rect panel = new Rect(500, 832, 920, 240);
                ShownShopCard = panel;
                RarityDescriptionFrame.Draw(panel, rarity, owner.PresentationClock); if (blood) BloodPriceFrame.Draw(panel, owner.PresentationClock);
                ArcadeSkin.Line(new Rect(panel.x + 2, panel.y + 2, 8, panel.height - 4), tag);
                Rect picture = new Rect(panel.x + 22, panel.y + 16, 160, 160);
                ArcadeSkin.Line(picture, new Color(0, 0, 0, .22f));
                ShopDescriptionCard.DrawSprite(new Rect(picture.x + 8, picture.y + 8, 144, 144), station.Offer != null ? offerCard.Portrait : station.Icon);
                if (station.Offer != null && offerCard.Role.HasValue) Sprite(new Rect(picture.x - 10, picture.y - 10, 58, 58), CompanionRoles.Badge(offerCard.Role.Value));
                if (station.Offer != null)
                {
                    string kind = item ? RunModifierRules.RarityName(rarity) : (offerCard.Role.HasValue ? CompanionRoles.Word(offerCard.Role.Value) : "FRIEND") + "  " + new string('★', Mathf.Clamp(station.Offer.Tier, 1, 3));
                    ArcadeSkin.Tag(new Rect(picture.x, picture.yMax + 10, 160, 32), kind, CodeTone.Walnut, tagStyle, tag); // code-drawn tag, rarity colour on walnut
                }
                // one fixed size for the price button and the i under it, centred in the card's right column (owner, 21/10/2026)
                const float PriceH = 104, InfoH = 56, ColumnGap = 12;
                float columnH = station.Offer != null ? PriceH + ColumnGap + InfoH : PriceH;
                Rect buy = new Rect(panel.xMax - 214, panel.y + Mathf.Round((panel.height - columnH) * .5f), 192, PriceH);
                float x = panel.x + 202, width = buy.x - 18 - x;
                GUI.Label(new Rect(x, panel.y + 10, width, 44), shop.DisplayName(station), cardName);
                float y = panel.y + 58;
                if (station.Offer != null && offerCard.Attack != null) y = ShopChips(x, y, width);
                string source = station.Offer != null ? offerCard.Summary : station.Recipe != null ? owner.Session.RecipePreview(station.Recipe) : station.Description;
                string body = Fit(source, width, panel.yMax - 12 - y, out int more);
                GlossaryUI.Text(new Rect(x, y, width, panel.yMax - 8 - y), body, cardLine);
                if (navigation.Button(buy, "", primary)) shop.Interact();
                DrawPriceButton(buy, station, owner);
                if (station.Offer != null && navigation.Button(new Rect(buy.x, buy.yMax + ColumnGap, buy.width, InfoH), more > 0 ? "i  +" + more : "i", button)) SetShopDetails(true);
                GlossaryUI.Draw(new Vector2(1920, 1080));
            }
        }
        // ---- the shop card's pieces
        private GUIStyle cardName, cardLine, cardChip, cardBig, cardPrice;
        private void ShopCardStyles()
        {
            if (cardName != null) return;
            cardName = ArcadeTypography.Style(30, true); cardName.wordWrap = false; cardName.clipping = TextClipping.Clip;
            cardLine = new GUIStyle(text) { fontSize = 21 };
            cardChip = ArcadeTypography.Style(19, true); cardChip.alignment = TextAnchor.MiddleLeft; cardChip.wordWrap = false;
            cardBig = ArcadeTypography.Style(30, true); cardBig.alignment = TextAnchor.MiddleCenter; cardBig.normal.textColor = ArcadeSkin.Ink;
            cardPrice = ArcadeTypography.Style(24, false, true); cardPrice.alignment = TextAnchor.MiddleLeft; cardPrice.normal.textColor = ArcadeSkin.Ink;
        }
        /// <summary>As many whole lines as fit the card (measured as drawn, wrapping included); a first line too long on its own
        /// is cut at a word with an ellipsis. The rest is counted (it waits under the i button).</summary>
        private string Fit(string source, float width, float height, out int more)
        {
            more = 0;
            if (string.IsNullOrEmpty(source)) return "";
            string[] all = source.Replace("\r", "").Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
            string kept = "";
            int taken = 0;
            for (; taken < all.Length; taken++)
            {
                string next = taken == 0 ? all[0] : kept + "\n" + all[taken];
                if (GlossaryUI.Height(next, cardLine, width) > height) break;
                kept = next;
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
        /// <summary>A companion's chips: its weapon's attack and each bond it counts toward (now / next).</summary>
        private float ShopChips(float x, float y, float width)
        {
            // chips with real room (owner, 22/10/2026): the icon in the chip's socket, 10 px before the words, 16 after them and
            // 12 between chips; a chip that does not fit first shrinks its words, then starts a second row
            float cx = x, rowY = y; int rows = 1;
            const float H = 50, Lead = 50, Tail = 16, Gap = 12;
            void Chip(Sprite icon, string label, bool lit)
            {
                int size = cardChip.fontSize;
                float Need() => Lead + cardChip.CalcSize(new GUIContent(label)).x + Tail;
                float room = width - (cx - x);
                while (Need() > room && cardChip.fontSize > 15) cardChip.fontSize--;
                if (Need() > room && cx > x && rows < 2) { rows++; cx = x; rowY += H + 8; room = width; cardChip.fontSize = size; while (Need() > room && cardChip.fontSize > 15) cardChip.fontSize--; }
                float w = Mathf.Min(room, Need());
                if (w < 110) { cardChip.fontSize = size; return; }
                Rect chip = new Rect(cx, rowY, w, H);
                if (lit) ArcadeSkin.BlinkStrong(chip, Time.unscaledTime); // a bond this friend lights blinks, strongly
                ArcadeSkin.Chip(chip, lit); // the painted walnut chip with its icon socket
                if (lit) ArcadeSkin.BlinkOver(chip, Time.unscaledTime);
                if (lit) ArcadeSkin.RunningLight(chip, Time.unscaledTime, ArcadeSkin.Gold, .8f); // COMBO, a bond this friend lights
                if (icon != null) ShopDescriptionCard.DrawSprite(ArcadeSkin.ChipIconRect(chip), icon); // on the socket's measured centre
                cardChip.normal.textColor = lit ? new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .55f + .45f * ArcadeSkin.BuyBlink(Time.unscaledTime)) : ArcadeSkin.Cream;
                GUI.Label(new Rect(chip.x + Lead, chip.y, w - Lead - Tail + 6, H), label, cardChip);
                cardChip.fontSize = size;
                cx += w + Gap;
            }
            // (no COMBO chip: the owner wants plain counts; a combo is listed under the details' COMBOS)
            Chip(offerCard.AttackIcon, offerCard.AttackLabel, false); // the weapon this friend comes with
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
            if (station.Offer != null && !affordable) verb = currency == ShopCurrency.Health ? "NOT ENOUGH HP" : currency == ShopCurrency.Eggs ? "NOT ENOUGH EGGS" : "NOT ENOUGH GOLD";
            bool priced = price > 0 && station.Action != "VISITOR";
            int bigSize = cardBig.fontSize; if (verb.Length > 6) cardBig.fontSize = 20;
            Color bigColor = cardBig.normal.textColor; // PAY is the ordinary gold button: ink on gold, the heart beside the price
            cardBig.wordWrap = true;
            // laid out inside the plate's rim with real margins (owner, 22/10/2026): the verb on the upper half, the price on the lower
            Rect inner = ArcadeSkin.Content(button, KitPiece.Plate);
            Rect verbRect = priced ? new Rect(inner.x, inner.y, inner.width, inner.height * .54f) : inner;
            GUI.Label(verbRect, verb, cardBig);
            cardBig.fontSize = bigSize; cardBig.normal.textColor = bigColor;
            if (!priced) return;
            string amount = price.ToString();
            float wide = 30 + 8 + cardPrice.CalcSize(new GUIContent(amount)).x, left = inner.center.x - wide * .5f;
            Rect priceRow = new Rect(inner.x, inner.y + inner.height * .54f, inner.width, inner.height * .46f);
            Rect icon = new Rect(left, priceRow.center.y - 14, 28, 28);
            Sprite(icon, currency == ShopCurrency.Health ? owner.Content.HeartSprite : currency == ShopCurrency.Eggs ? owner.Content.EggSprite : owner.Content.Arena.Coin);
            Color priceColor = cardPrice.normal.textColor;
            GUI.Label(new Rect(icon.xMax + 8, icon.y - 2, wide, 32), amount, cardPrice);
            cardPrice.normal.textColor = priceColor;
        }
        /// <summary>Whether the player has enough of the offer's currency (HP prices must leave you standing).</summary>
        private static bool Affordable(ArenaController owner, WorldShop.Station station)
        {
            if (station.Offer == null) return true;
            RunSnapshot state = owner.Session.State; int price = station.Offer.Price;
            return station.Offer.Currency == ShopCurrency.Health ? owner.Session.BloodAffordable(price) : station.Offer.Currency == ShopCurrency.Eggs ? state.Eggs >= price : state.Gold >= price;
        }
        /// <summary>The full rules of an offer (the i button): attack and bonds, then everything else; BACK returns.</summary>
        private void DrawShopDetails(ArenaController owner, WorldShop shop, WorldShop.Station station, ItemRarity rarity, Color tag, bool blood = false)
        {
            Rect panel = new Rect(400, 150, 1120, 880);
            ShownShopCard = panel;
            RarityDescriptionFrame.Draw(panel, rarity, owner.PresentationClock); if (blood) BloodPriceFrame.Draw(panel, owner.PresentationClock);
            ArcadeSkin.Line(new Rect(panel.x + 2, panel.y + 2, 8, panel.height - 4), tag);
            // stacked blocks inside the frame's padding (owner, 22/10/2026): the header row, the tiles row, then the scrolling text
            // starting below the tiles' full height, so nothing sits behind the tiles or on the frame's rim
            Rect inner = new Rect(panel.x + 56, panel.y + 34, panel.width - 112, panel.height - 68);
            ShopDescriptionCard.DrawSprite(new Rect(inner.x, inner.y, 72, 72), offerCard.Portrait);
            GUI.Label(new Rect(inner.x + 88, inner.y - 4, inner.width - 88 - 196, 44), shop.DisplayName(station), cardName);
            small.normal.textColor = tag; GUI.Label(new Rect(inner.x + 88, inner.y + 40, inner.width - 88 - 196, 28), offerCard.Identity, small); small.normal.textColor = ArcadeSkin.Cream;
            if (navigation.Button(new Rect(inner.xMax - 176, inner.y, 176, 72), "BACK", button)) { SetShopDetails(false); return; }
            float headerHeight = offerCard.HeaderHeight, tilesTop = inner.y + 96;
            if (headerHeight > 0) offerCard.DrawHeader(new Rect(inner.x, tilesTop, inner.width, headerHeight), owner.PresentationClock);
            string description = offerCard.Details;
            if (BuildPreview(owner, station.Offer) > 0) foreach (var row in previewRows) description += "\n" + row.name + " " + row.before + " → " + row.after;
            float top = tilesTop + headerHeight + 16 - panel.y, textWidth = inner.width - 30;
            float contentHeight = GlossaryUI.Height(description, cardLine, textWidth) + 8;
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
        public static string CombatTimerCaption(bool bossOvertime, float secondsRemaining) => bossOvertime ? "BOSSES" : Mathf.Max(0, Mathf.CeilToInt(secondsRemaining)).ToString();

        private void DrawControls(ArenaController owner)
        {
            if (RunUIHints.Device != RunUIDevice.Touch) return;
            Rect pause = new Rect(35, 240, 64, 64);
            Rect details = new Rect(115, 240, 64, 64);
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
                ArcadeSkin.Line(new Rect(pause.x + 21, pause.y + 14, 8, 24), ArcadeSkin.Ink);
                ArcadeSkin.Line(new Rect(pause.x + 36, pause.y + 14, 8, 24), ArcadeSkin.Ink);
            }
            ArcadeSkin.Line(new Rect(details.x + 16, details.y + 12, 33, 28), ArcadeSkin.Ink);
            ArcadeSkin.Line(new Rect(details.x + 19, details.y + 15, 27, 22), ArcadeSkin.Paper);
            ArcadeSkin.Line(new Rect(details.x + 31, details.y + 15, 3, 22), ArcadeSkin.Ink);
            for (int line = 0; line < 3; line++)
            {
                ArcadeSkin.Line(new Rect(details.x + 22, details.y + 18 + line * 6, 6, 2), ArcadeSkin.Ink);
                ArcadeSkin.Line(new Rect(details.x + 37, details.y + 18 + line * 6, 6, 2), ArcadeSkin.Ink);
            }
            string hint = pause.Contains(Event.current.mousePosition) ? RunUIHints.Pause(RunUIHints.Device) : details.Contains(Event.current.mousePosition) ? RunUIHints.Details(RunUIHints.Device) : null;
            if (hint == null) return;
            Fill(new Rect(1400, 92, 435, 44), Panel);
            GUI.Label(new Rect(1413, 99, 410, 32), hint, small);
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
            if (encoreCount >= 0 && encores > encoreCount && encoreUnit != null)
                cutIn.Show(RunUICopy.UnitName(encoreUnit), "★★★  SHINES BRIGHTER", encoreUnit.Body, ArcadeSkin.Gold, "ui.cutin.encore");
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
            GUI.Label(new Rect(area.x, area.y - 4, area.width, 20), "IF YOU BUY", caption);
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
                GUI.Label(new Rect(line.x, line.y, 165, row), name, compareStyle);
                GUI.Label(new Rect(line.x + 180, line.y, 70, row), entry.before, compareStyle);
                compareStyle.normal.textColor = entry.better ? ArcadeSkin.Better : ArcadeSkin.Worse;
                GUI.Label(new Rect(line.x + 260, line.y, 130, row), (entry.better ? "▲ " : "▼ ") + entry.after, compareStyle);
            }
        }

        /// <summary>A bounded overview; the full bond list stays in the paused build details.</summary>
        private void DrawBonds(ArenaController owner)
        {
            bondFeedback.Observe(owner.Session.State.RunId, owner.Content.Bonds, owner.Session.BondCounts, owner.Simulation.VisualElapsed);
            if (programStyle == null) { programStyle = ArcadeTypography.Style(18, true); programStyle.font = ArcadeTypography.Punch; programStyle.normal.textColor = ArcadeSkin.Ink; programStyle.wordWrap = false; programStyle.alignment = TextAnchor.MiddleLeft; }
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
            const float rowHeight = BondHudSummary.RowHeight, width = BondHudSummary.Width;
            Rect card = new Rect(1900 - width, 18, width, bondSummary.Height + newBonds.Count * rowHeight);
            bool still = owner.Profile != null && owner.Profile.ReducedMotion, calm = PresentationSettings.ReducedFlashes;
            float clock = Time.unscaledTime, blink = still || calm ? .6f : .5f + .5f * Mathf.Sin(clock * 7f);
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Accent);
            Color deep = Color.Lerp(ArcadeSkin.Accent, ArcadeSkin.Ink, 0.4f);
            programStyle.fontSize = 15; programStyle.normal.textColor = deep;
            GUI.Label(new Rect(card.x + 12, card.y + 5, 74, 23), "BONDS", programStyle);
            programStyle.fontSize = 13;
            programStyle.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(card.x + 90, card.y + 5, width - 104, 23), bondSummary.Active + " ACTIVE / " + bondSummary.Rows.Count, programStyle);
            programStyle.fontSize = 15; programStyle.alignment = TextAnchor.MiddleLeft; programStyle.normal.textColor = ArcadeSkin.Ink;
            string tooltip = null;
            float tooltipTop = 0;
            // bonds the offer would start (none yet): drawn first, greyed, with a gold blink and a NEW stamp
            for (int index = 0; index < newBonds.Count; index++)
            {
                var row = newBonds[index];
                Rect line = new Rect(card.x + 10, card.y + 32 + index * rowHeight, width - 20, rowHeight - 4);
                ArcadeSkin.Slant(line, new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .18f + .3f * blink));
                BondRowContent(line, row.Bond, 0, row.After, false, true, row.Activates, blink, 0);
                Rect stamp = new Rect(line.xMax - 40, line.y + 2, 38, 21);
                ArcadeSkin.Slant(stamp, Color.Lerp(ArcadeSkin.Gold, ArcadeSkin.Cream, blink * .4f));
                programStyle.fontSize = 11; programStyle.alignment = TextAnchor.MiddleCenter; programStyle.normal.textColor = ArcadeSkin.Ink;
                GUI.Label(stamp, "NEW", programStyle);
                programStyle.fontSize = 15; programStyle.alignment = TextAnchor.MiddleLeft; programStyle.normal.textColor = ArcadeSkin.Ink;
            }
            for (int index = 0; index < bondSummary.VisibleCount; index++)
            {
                BondDefinition bond = bondSummary.Rows[index]; int count = owner.Session.BondCounts[bond.Id];
                Rect line = new Rect(card.x + 10, card.y + 32 + (index + newBonds.Count) * rowHeight, width - 20, rowHeight - 4);
                int level = bond.ActiveLevel(count);
                bool previewing = previewBonds.TryGetValue(bond.Id, out var preview);
                bool flashing = bondFeedback.Flashing(bond.Id, owner.Simulation.VisualElapsed);
                if (flashing) ArcadeSkin.Slant(line, new Color(ArcadeSkin.Teal.r, ArcadeSkin.Teal.g, ArcadeSkin.Teal.b, 0.55f));
                else if (previewing)
                {
                    ArcadeSkin.Slant(line, new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, (preview.Activates ? .25f : .12f) + (preview.Activates ? .35f : .15f) * blink));
                    if (preview.Activates) ArcadeSkin.RunningLight(line, Time.unscaledTime, ArcadeSkin.Gold, .75f); // the bond this purchase lights
                }
                else if (level > 0)
                {
                    // an active bond breathes: a soft teal wash, and now and then a light sweep runs across the row
                    float breathe = still ? .5f : .5f + .5f * Mathf.Sin(clock * 2.2f + index);
                    ArcadeSkin.Slant(line, new Color(ArcadeSkin.Teal.r, ArcadeSkin.Teal.g, ArcadeSkin.Teal.b, .1f + .1f * breathe));
                    if (!still && !calm)
                    {
                        float sweep = Mathf.Repeat(clock * .45f + index * .23f, 1.6f);
                        if (sweep < 1) ArcadeSkin.Line(new Rect(line.x + sweep * (line.width - 18), line.y, 18, line.height), new Color(1, 1, 1, .22f * Mathf.Sin(sweep * Mathf.PI)));
                    }
                }
                float burst = owner.Profile != null && owner.Profile.ReducedMotion ? 0 : bondFeedback.Burst(bond.Id, owner.Simulation.VisualElapsed);
                BondRowContent(line, bond, count, previewing ? preview.After : count, level > 0, previewing, previewing && preview.Activates, blink, burst);
                if (level > 0 || previewing && preview.Activates)
                {
                    Rect stamp = new Rect(line.xMax - 36, line.y + 2, 34, 21);
                    bool rising = previewing && preview.Activates;
                    ArcadeSkin.Slant(stamp, rising ? Color.Lerp(ArcadeSkin.Gold, ArcadeSkin.Cream, blink * .4f) : ArcadeSkin.Accent);
                    programStyle.fontSize = 11; programStyle.alignment = TextAnchor.MiddleCenter; programStyle.normal.textColor = rising ? ArcadeSkin.Ink : ArcadeSkin.Cream;
                    GUI.Label(stamp, rising ? "LV" + bond.ActiveLevel(preview.After) + "▲" : "LV" + level, programStyle);
                    programStyle.fontSize = 15; programStyle.alignment = TextAnchor.MiddleLeft; programStyle.normal.textColor = ArcadeSkin.Ink;
                }
                if (bondHit == null) bondHit = new GUIStyle();
                if (GUI.Button(line, GUIContent.none, bondHit)) { selectedBond = bond.Id; bondTooltipUntil = Time.unscaledTime + 3; }
                if (line.Contains(Event.current.mousePosition) || selectedBond == bond.Id && Time.unscaledTime < bondTooltipUntil)
                {
                    tooltip = count + "/" + bond.NextThreshold(count) + " · " + RunUICopy.BondDescription(bond, count);
                    tooltipTop = line.y;
                }
            }
            programStyle.fontSize = 13; programStyle.alignment = TextAnchor.MiddleCenter; programStyle.normal.textColor = deep;
            Rect footer = new Rect(card.x + 10, card.yMax - 28, width - 20, 24);
            string more = bondSummary.HiddenCount > 0 ? "+" + bondSummary.HiddenCount + " MORE  ·  VIEW ALL" : "VIEW ALL BONDS";
            if (GUI.Button(footer, more, programStyle))
            {
                if (!owner.Details.Open) owner.ToggleDetails();
                owner.Details.SelectBond(bondSummary.Rows[0].Name);
            }
            programStyle.alignment = TextAnchor.MiddleLeft;
            small.normal.textColor = ArcadeSkin.Cream; small.alignment = TextAnchor.UpperLeft;
            if (tooltip != null)
            {
                float height = GlossaryUI.Height(tooltip, small, 385) + 24;
                Fill(new Rect(1140, tooltipTop, 420, height), Panel);
                GlossaryUI.Text(new Rect(1155, tooltipTop + 12, 385, height - 24), tooltip, small);
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
            Sprite(new Rect(line.x + 1 - burst * 2, line.y + 1 - burst * 2, 26 + burst * 4, 26 + burst * 4), bond.Icon);
            GUI.color = previous;
            programStyle.normal.textColor = grey ? new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, .5f) : ArcadeSkin.Ink;
            GUI.Label(new Rect(line.x + 34, line.y - 1, 150, 26), bond.Name, programStyle);
            int next = Mathf.Max(after, bond.NextThreshold(after));
            programStyle.fontSize = 12; programStyle.alignment = TextAnchor.MiddleRight;
            GUI.Label(new Rect(line.x + 160, line.y, 76, 24), previewing && after != count ? count + "→" + after + "/" + next : count + "/" + Mathf.Max(count, bond.NextThreshold(count)), programStyle);
            programStyle.fontSize = 15; programStyle.alignment = TextAnchor.MiddleLeft; programStyle.normal.textColor = ArcadeSkin.Ink;
            Rect progress = new Rect(line.x + 35, line.yMax - 3, line.width - 39, 2);
            ArcadeSkin.Line(progress, new Color(0, 0, 0, .12f));
            int shown = Mathf.Max(count, bond.NextThreshold(count));
            if (previewing && after > count) ArcadeSkin.Line(new Rect(progress.x, progress.y, progress.width * Mathf.Clamp01((float)after / next), progress.height), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .4f + .5f * blink));
            ArcadeSkin.Line(new Rect(progress.x, progress.y, progress.width * Mathf.Clamp01((float)count / shown), progress.height), active ? ArcadeSkin.Teal : grey ? new Color(.5f, .5f, .55f) : ArcadeSkin.Gold);
        }

        private GUIStyle fallenStyle;

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
            GUI.Label(new Rect(area.x + 100, area.y + 6, area.width - 116, area.height - 16), "“" + line + "”", fallenStyle);
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
            float contentHeight = small.CalcHeight(new GUIContent(receipt.Description), 425) + 20;
            float panelHeight = Mathf.Clamp(contentHeight + 110, 190, 470);
            Fill(new Rect(1320, 275, 510, panelHeight), Color.Lerp(Panel, ArcadeSkin.Accent, 0.12f));
            ArcadeSkin.Divider(new Rect(1340, 342, 460, 2), ArcadeSkin.Accent);
            GUI.Label(new Rect(1340, 295, 465, 45), receipt.Title, text);
            float height = Mathf.Max(panelHeight - 105, contentHeight);
            receiptScroll = ScrollPanel.Begin(new Rect(1340, 355, 470, panelHeight - 105), receiptScroll, new Rect(0, 0, 430, height));
            GUI.Label(new Rect(0, 0, 425, height), receipt.Description, small);
            ScrollPanel.End();
        }

        public void DrawMenuConfirmation(ArenaController owner, Camera camera)
        {
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            navigation.Begin();
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            Fill(new Rect(470, 310, 980, 440), Panel);
            GUI.Label(new Rect(520, 345, 880, 80), "Return to the Waiting Room?", title);
            GUI.Label(new Rect(540, 445, 840, 105), "Saves commit at wave start or after shop transactions. Unfinished combat restarts from the saved boundary, not the pause moment.", text);
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(540, 550, 840, 70), owner.SaveError, small);
            if (navigation.Button(new Rect(550, 650, 360, 65), "STAY", button)) owner.CancelMenuReturn();
            if (navigation.Button(new Rect(1010, 650, 360, 65), "SAVE AND RETURN", primary ?? button)) owner.ReturnToMenu();
            navigation.End();
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }
    }
}
