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
        public void ReadNavigation(bool enabled) { RunUIHints.Refresh(); if (enabled) navigation.ReadInput(); }
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
            if (walletRun != state.RunId) { walletRun = state.RunId; healthTrail = health; lastGold = gold; lastEggs = eggs; goldChanged = eggsChanged = -100; }
            if (lastGold != gold) { goldChanged = simulation.VisualElapsed; lastGold = gold; }
            if (lastEggs != eggs) { eggsChanged = simulation.VisualElapsed; lastEggs = eggs; }
            bool fighting = state.Phase == SessionPhase.Combat && simulation.Champion.Alive;
            danger = fighting ? Mathf.Clamp01(Mathf.InverseLerp(0.32f, 0.06f, health / maximum)) : 0f;
            ArcadeMusic.Danger = danger;
            LowHealthVignette.Draw(new Vector2(1920, 1080), danger);
            if (state.Phase == SessionPhase.Combat) NightHud.EdgeEyes(simulation, camera, new Vector2(1920, 1080));
            float bossFlash = owner.Feel != null ? owner.Feel.ScreenFlash : 0;
            if (bossFlash > 0.01f) Fill(new Rect(0, 0, 1920, 1080), new Color(1f, 0.96f, 0.86f, bossFlash * 0.55f)); // a boss falls: the night flashes white
            if (Event.current.type == EventType.Repaint) ObserveMoments(owner);
            if (PresentationSettings.ReducedMotion || health > healthTrail) healthTrail = health;
            else if (Event.current.type == EventType.Repaint && simulation.Phase != RunPhase.Paused) healthTrail = Mathf.MoveTowards(healthTrail, health, Mathf.Min(Time.unscaledDeltaTime, 0.05f) * maximum * 0.5f);
            ArcadeSkin.Plate(new Rect(22, 16, 340, 72), ArcadeSkin.Accent);
            Sprite(new Rect(35, 20, 62, 62), owner.Content.Unit(state.ChampionId).Body);
            NightHud.LightRing(new Vector2(66, 51), 40, state.Roster.Count + ActOneCanon.StoryCount(state.Wave, state.Phase == SessionPhase.Shop, state.Endless));
            ArcadeSkin.Line(new Rect(105, 36, 230, 30), new Color(0.02f, 0.02f, 0.05f, 0.95f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(healthTrail / maximum), 24), new Color(0.55f, 0.30f, 0.14f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(health / maximum), 24), new Color(1f, 0.70f, 0.36f)); // your light, not the hostile red
            numeric.normal.textColor = health / maximum > 0.45f ? ArcadeSkin.Ink : ArcadeSkin.Cream; // ink reads on the lantern bar
            GUI.Label(new Rect(108, 36, 224, 30), Mathf.CeilToInt(health) + " / " + Mathf.CeilToInt(maximum), numeric);
            numeric.normal.textColor = ArcadeSkin.Cream;
            if (owner.Content.HeroRevision > 0 && state.Phase == SessionPhase.Combat)
            {
                float required = simulation.HeroIdentityEnabled ? simulation.HeroIdentityRequired : simulation.Champion.Definition.HeroMechanic == HeroMechanic.LanternStride ? 12 : simulation.Champion.Definition.HeroMechanic == HeroMechanic.ParcelExpress ? 6 : 2;
                ArcadeSkin.Line(new Rect(108, 72, 224, 5), new Color(0.06f, 0.07f, 0.11f, 0.85f));
                ArcadeSkin.Line(new Rect(108, 72, 224 * Mathf.Clamp01((simulation.HeroIdentityEnabled ? simulation.HeroIdentityCharge : simulation.HeroCharge) / required), 5), simulation.HeroIdentityEnabled ? HeroIdentities.For(state.ChampionId).Color : ArcadeSkin.Teal);
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
            ArcadeSkin.Plate(new Rect(22, 96, 190, 40), ArcadeSkin.Gold);
            ArcadeSkin.Plate(new Rect(22, 142, 190, 40), ArcadeSkin.Teal);
            Sprite(new Rect(40 - goldPulse, 100 - goldPulse, 32 + goldPulse * 2, 32 + goldPulse * 2), owner.Content.Arena.Coin);
            numericLeft.normal.textColor = Color.Lerp(ArcadeSkin.Cream, ArcadeSkin.Gold, goldPulse / 4f);
            GUI.Label(new Rect(84, 100, 120, 32), gold.ToString("N0"), numericLeft);
            Sprite(new Rect(40 - eggPulse, 146 - eggPulse, 32 + eggPulse * 2, 32 + eggPulse * 2), owner.Content.EggSprite);
            numericLeft.normal.textColor = Color.Lerp(ArcadeSkin.Cream, ArcadeSkin.Teal, eggPulse / 4f);
            GUI.Label(new Rect(84, 146, 120, 32), eggs.ToString("N0"), numericLeft);
            Rect waveRect = state.Phase == SessionPhase.Shop ? new Rect(380, 24, 280, 52) : new Rect(820, 22, 280, 52);
            ArcadeSkin.Plate(waveRect, ArcadeSkin.Accent);
            GUI.Label(new Rect(waveRect.x + 10, waveRect.y + 4, waveRect.width - 10, 44), (state.Endless ? "∞ " : "") + "Wave " + state.Wave, title);
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
                if (simulation.Phase == RunPhase.Paused)
                {
                    Fill(new Rect(640, 300, 640, 500), Panel);
                    ArcadeSkin.Header(new Rect(700, 380, 520, 100), "THE NIGHT WAITS WITH YOU", "Paused");
                    if (navigation.Button(new Rect(750, 515, 420, 65), "RESUME", primary)) owner.TogglePause();
                    if (navigation.Button(new Rect(700, 595, 250, 60), "STATS", button)) { owner.ToggleDetails(); owner.Details.SelectTab(0); }
                    if (navigation.Button(new Rect(970, 595, 250, 60), "RELICS", button)) { owner.ToggleDetails(); owner.Details.SelectTab(2); }
                    if (navigation.Button(new Rect(750, 675, 420, 55), "MAIN MENU", button)) owner.RequestMenuReturn();
                }
            }
            else if (state.Phase == SessionPhase.Shop) { owner.Shop.DrawPriceTags(camera); DrawShop(owner); statFeedback.Draw(owner,camera); }
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
                ArcadeSkin.Header(new Rect(110, 228, 575, 105), "THE NIGHT IS RECORDED · NO STAT REWARDS", state.Phase == SessionPhase.Victory ? "Dawn Has Come" : (state.ActOne?.SitCloser ?? 0) == ActOneProgress.Lost ? "You Sat Down" : "Until You Return");
                bool fallen = state.Phase == SessionPhase.Defeat;
                Sprite(fallen ? new Rect(240, 345, 230, 200) : new Rect(220, 355, 270, 270), owner.Content.Unit(state.ChampionId).Body);
                Sprite(fallen ? new Rect(450, 450, 90, 90) : new Rect(475, 480, 100, 100), owner.Content.Unit(state.ChampionId).Weapon.Sprite);
                if (fallen) DrawFallenWord(owner, state, new Rect(115, 552, 565, 96));
                GUI.Label(new Rect(115, 655, 565, 90), owner.MetaNotice, small);
                if (state.Phase == SessionPhase.Victory && navigation.Button(new Rect(115, 750, 560, 60), "ENDLESS · KEEP BRIGADE AND RELICS", button)) owner.ContinueEndless();
                if (navigation.Button(new Rect(115, 825, 270, 65), "PLAY AGAIN", primary)) owner.RestartRun();
                if (navigation.Button(new Rect(405, 825, 270, 65), "MAIN MENU", button)) owner.ReturnToMenu();
                invoice.Draw(new Rect(770, 200, 1060, 685), state, owner.Content);
                if (navigation.Button(new Rect(1025, 915, 570, 55), "SAVE RECEIPT IMAGE · PNG", button))
                    try { RunInvoice.ExportFrame(state.RunId); } catch (System.Exception exception) { Debug.LogWarning("Receipt export failed: " + exception.Message); }
            }
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(65, 130, 1800, 70), owner.SaveError, text);
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) DrawBonds(owner);
            waveStamp.Observe(state.RunId, state.Wave, state.Phase == SessionPhase.Combat && !owner.TransitionPending);
            if (state.Phase == SessionPhase.Combat && !owner.TransitionPending) waveStamp.Draw((state.Endless ? "∞ " : "") + "WAVE " + state.Wave);
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) coach.Draw(owner, camera);
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
            if (owner.Simulation.Phase == RunPhase.Paused)
            {
                Fill(new Rect(710, 440, 500, 180), Panel);
                if (navigation.Button(new Rect(755, 485, 410, 75), "RESUME", primary)) owner.TogglePause();
                return;
            }
            WorldShop.Station station = shop.Nearby;
            ShownShopCard = Rect.zero;
            if (station != null)
            {
                if (station.Action == "NEXT HALL")
                {
                    GUI.Label(new Rect(730, 930, 460, 32), "↓ Next wave · " + RunUIHints.Interact(RunUIHints.Device), small);
                    if (navigation.Button(new Rect(850, 978, 220, 52), "CONTINUE ↓", primary)) shop.Interact();
                    return;
                }
                if (tooltipStation != station) { tooltipStation = station; shopScroll = Vector2.zero; shopDetails = false; }
                if (station.Offer != null) offerCard.Refresh(owner.Session, owner.Content, station.Offer);
                string description = station.Offer != null ? shopDetails ? offerCard.Details : offerCard.Summary : station.Recipe != null ? owner.Session.RecipePreview(station.Recipe) : station.Description;
                if (shopDetails && station.Offer != null && BuildPreview(owner, station.Offer) > 0)
                    foreach (var row in previewRows) description += "\n" + row.name + " " + row.before + " → " + row.after;
                if (station.Offer?.Currency == ShopCurrency.Health) description += "\nPay current HP; must leave at least 1 HP.";
                float textWidth = 880;
                float contentHeight = GlossaryUI.Height(description, small, textWidth) + 8;
                Rect panel = ShopTooltipBounds(contentHeight);
                float headerHeight = station.Offer != null ? offerCard.HeaderHeight : 0;
                if (station.Offer != null) { panel.height = shopDetails ? Mathf.Max(455, headerHeight + 280) : headerHeight > 0 ? headerHeight + 270 : 276; panel.y = 1030 - panel.height; }
                ShownShopCard = panel;
                bool item = station.Offer != null && station.Offer.Kind == OfferKind.Item;
                ItemRarity rarity = item ? owner.Content.Item(station.Offer.Id).Rarity : station.Offer != null ? owner.Content.Unit(station.Offer.Id).Rarity : ItemRarity.Common;
                Color tag = rarity != ItemRarity.Common ? RunModifierRules.RarityColor(rarity) : station.Offer != null ? ArcadeSkin.Teal : ArcadeSkin.Cream;
                RarityDescriptionFrame.Draw(panel,rarity,owner.PresentationClock);
                ArcadeSkin.Line(new Rect(panel.x + 2, panel.y + 2, 8, panel.height - 4), tag);
                string kind = item ? RunModifierRules.RarityName(rarity) : station.Offer != null && station.Offer.Kind == OfferKind.Companion ? "COMPANION" : null;
                if (kind != null)
                {
                    ArcadeSkin.Slant(new Rect(panel.x + 22, panel.y - 14, 150, 26), tag);
                    GUI.Label(new Rect(panel.x + 22, panel.y - 14, 150, 26), kind, tagStyle);
                }
                if (station.Offer != null) ShopDescriptionCard.DrawSprite(new Rect(panel.x + 24, panel.y + 12, 38, 38), offerCard.Portrait);
                GUI.Label(new Rect(panel.x + (station.Offer != null ? 70 : 24), panel.y + 12, 500, 38), shop.DisplayName(station), text);
                if (station.Offer != null) { small.normal.textColor = tag; GUI.Label(new Rect(panel.x + 70, panel.y + 47, 520, 27), offerCard.Identity, small); small.normal.textColor = ArcadeSkin.Cream; }
                if (station.Offer != null)
                {
                    Rect costIcon = new Rect(panel.xMax - 320, panel.y + 54, 24, 24);
                    if (station.Offer.Currency == ShopCurrency.Health) StatGlyph.Draw(costIcon, ItemEffect.Health);
                    else Sprite(costIcon, station.Offer.Currency == ShopCurrency.Eggs ? owner.Content.EggSprite : owner.Content.Arena.Coin);
                    GUI.Label(new Rect(costIcon.xMax + 6, costIcon.y - 3, 265, 30), station.Offer.Price + " " + (station.Offer.Currency == ShopCurrency.Health ? "HP · leave 1 HP" : station.Offer.Currency == ShopCurrency.Eggs ? "eggs" : "gold"), small);
                }
                if (station.Offer != null && navigation.Button(new Rect(panel.xMax - 340, panel.y + 18, 100, 36), shopDetails ? "BACK" : "INFO", button)) SetShopDetails(!shopDetails);
                if (headerHeight > 0) offerCard.DrawHeader(new Rect(panel.x + 24, panel.y + 80, 902, headerHeight), owner.PresentationClock);
                float bodyY = station.Offer != null ? 84 + headerHeight : 64;
                shopScroll = GUI.BeginScrollView(new Rect(panel.x + 24, panel.y + bodyY, textWidth + 26, panel.height - bodyY - 15), shopScroll, new Rect(0, 0, textWidth + 5, contentHeight));
                GlossaryUI.Text(new Rect(0, 0, textWidth, contentHeight), description, small);
                GUI.EndScrollView();
                if (navigation.Button(new Rect(panel.xMax - 230, panel.y + 14, 205, 46), RunUIHints.Device == RunUIDevice.Controller ? "A · BUY" : RunUIHints.Device == RunUIDevice.Touch ? "BUY" : "Space · BUY", primary)) shop.Interact();
                GlossaryUI.Draw(new Vector2(1920, 1080));
            }
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
            Rect pause = new Rect(35, 245, 65, 52);
            Rect details = new Rect(115, 245, 65, 52);
            if (navigation.Button(pause, "", button)) owner.TogglePause();
            if (navigation.Button(details, "", button)) owner.ToggleDetails();
            ArcadeSkin.Line(new Rect(pause.x + 21, pause.y + 14, 8, 24), ArcadeSkin.Ink);
            ArcadeSkin.Line(new Rect(pause.x + 36, pause.y + 14, 8, 24), ArcadeSkin.Ink);
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
            foreach (RosterEntry entry in state.Roster) if (entry.Tier >= 3) { encores++; encoreUnit = owner.Content.Unit(entry.UnitId); }
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
                previewRows.Add(("Brigade size", state.Roster.Count.ToString(), preview.State.Roster.Count.ToString(), preview.State.Roster.Count >= state.Roster.Count));
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
                previewRows.Add((name, before, after, better));
                if (previewRows.Count >= 3) break;
            }
            return previewRows.Count;
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
            if (bondSummary.Rows.Count == 0) return;
            const float rowHeight = BondHudSummary.RowHeight, width = BondHudSummary.Width;
            Rect card = new Rect(1900 - width, 18, width, bondSummary.Height);
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
            for (int index = 0; index < bondSummary.VisibleCount; index++)
            {
                BondDefinition bond = bondSummary.Rows[index]; int count = owner.Session.BondCounts[bond.Id];
                Rect line = new Rect(card.x + 10, card.y + 32 + index * rowHeight, width - 20, rowHeight - 4);
                int level = bond.ActiveLevel(count);
                int next = Mathf.Max(count, bond.NextThreshold(count));
                bool flashing = bondFeedback.Flashing(bond.Id, owner.Simulation.VisualElapsed);
                if (flashing) ArcadeSkin.Slant(line, new Color(ArcadeSkin.Teal.r, ArcadeSkin.Teal.g, ArcadeSkin.Teal.b, 0.55f));
                else if (index % 2 == 1) ArcadeSkin.Line(line, new Color(0, 0, 0, 0.05f));
                float burst = owner.Profile != null && owner.Profile.ReducedMotion ? 0 : bondFeedback.Burst(bond.Id, owner.Simulation.VisualElapsed);
                Sprite(new Rect(line.x + 1 - burst * 2, line.y + 1 - burst * 2, 26 + burst * 4, 26 + burst * 4), bond.Icon);
                GUI.Label(new Rect(line.x + 34, line.y - 1, 150, 26), bond.Name, programStyle);
                programStyle.fontSize = 12; programStyle.alignment = TextAnchor.MiddleRight;
                GUI.Label(new Rect(line.x + 181, line.y, 54, 24), count + "/" + next, programStyle);
                programStyle.fontSize = 15; programStyle.alignment = TextAnchor.MiddleLeft;
                Rect progress = new Rect(line.x + 35, line.yMax - 3, line.width - 39, 2);
                ArcadeSkin.Line(progress, new Color(0, 0, 0, .12f));
                ArcadeSkin.Line(new Rect(progress.x, progress.y, progress.width * Mathf.Clamp01((float)count / next), progress.height), level > 0 ? ArcadeSkin.Teal : ArcadeSkin.Gold);
                if (level > 0)
                {
                    Rect stamp = new Rect(line.xMax - 36, line.y + 2, 34, 21);
                    ArcadeSkin.Slant(stamp, ArcadeSkin.Accent);
                    programStyle.fontSize = 11; programStyle.alignment = TextAnchor.MiddleCenter; programStyle.normal.textColor = ArcadeSkin.Cream;
                    GUI.Label(stamp, "LV" + level, programStyle);
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
                // the warm chair kept them: Hob speaks for everyone who pulled
                speaker = owner.Content.Unit(ActOneCanon.StoryIds[0]) ?? speaker;
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
            receiptScroll = GUI.BeginScrollView(new Rect(1340, 355, 470, panelHeight - 105), receiptScroll, new Rect(0, 0, 430, height));
            GUI.Label(new Rect(0, 0, 425, height), receipt.Description, small);
            GUI.EndScrollView();
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
