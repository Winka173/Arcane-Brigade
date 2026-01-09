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
        private GUIStyle bondHit;
        private string selectedBond;
        private float bondTooltipUntil;
        private bool receiptOpen;
        private int receiptId;
        private Vector2 receiptScroll;
        private readonly LootIndicators lootIndicators = new LootIndicators();
        private readonly RunInvoice invoice = new RunInvoice();
        private readonly ArcadeNavigation navigation = new ArcadeNavigation();
        private readonly RunBondFeedback bondFeedback = new RunBondFeedback();
        private Vector2 shopScroll;
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
        private static readonly Color Panel = new Color(0.10f, 0.08f, 0.19f, 0.97f);

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
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            navigation.Begin();
            if (text == null)
            {
                text = ArcadeTypography.Style(23);
                small = new GUIStyle(text) { fontSize = 18 };
                title = ArcadeTypography.Style(34, true);
                title.alignment = TextAnchor.MiddleCenter;
                button = new GUIStyle(GUI.skin.button) { fontSize = 22, font = ArcadeTypography.Heading };
                numeric = ArcadeTypography.Style(20, false, true);
            }
            RunSession session = owner.Session;
            RunSnapshot state = session.State;
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
            if (PresentationSettings.ReducedMotion || health > healthTrail) healthTrail = health;
            else if (Event.current.type == EventType.Repaint && simulation.Phase != RunPhase.Paused) healthTrail = Mathf.MoveTowards(healthTrail, health, Mathf.Min(Time.unscaledDeltaTime, 0.05f) * maximum * 0.5f);
            Sprite(new Rect(35, 28, 62, 62), owner.Content.Unit(state.ChampionId).Body);
            ArcadeSkin.Line(new Rect(105, 36, 230, 30), new Color(0.06f, 0.07f, 0.12f, 0.85f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(healthTrail / maximum), 24), new Color(1f, 0.68f, 0.28f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(health / maximum), 24), new Color(0.94f, 0.24f, 0.36f));
            GUI.Label(new Rect(135, 36, 200, 32), Mathf.CeilToInt(health) + " / " + maximum.ToString("0"), numeric);
            if (owner.Content.HeroRevision > 0 && state.Phase == SessionPhase.Combat)
            {
                float required = simulation.Champion.Definition.HeroMechanic == HeroMechanic.LanternStride ? 12 : simulation.Champion.Definition.HeroMechanic == HeroMechanic.ParcelExpress ? 6 : 2;
                ArcadeSkin.Line(new Rect(108, 72, 224, 5), new Color(0.08f, 0.13f, 0.2f, 0.8f));
                ArcadeSkin.Line(new Rect(108, 72, 224 * Mathf.Clamp01(simulation.HeroCharge / required), 5), simulation.HeroCooldownRemaining > 0 ? Color.Lerp(ArcadeSkin.Teal, ArcadeSkin.Ink, 0.6f) : ArcadeSkin.Teal);
            }
            float goldPulse = LivingUI.Press(simulation.VisualElapsed - goldChanged, PresentationSettings.ReducedMotion) * 4;
            float eggPulse = LivingUI.Press(simulation.VisualElapsed - eggsChanged, PresentationSettings.ReducedMotion) * 4;
            Sprite(new Rect(40 - goldPulse, 100 - goldPulse, 32 + goldPulse * 2, 32 + goldPulse * 2), owner.Content.Arena.Coin);
            GUI.Label(new Rect(85, 102, 240, 30), gold.ToString(), numeric);
            Sprite(new Rect(40 - eggPulse, 140 - eggPulse, 32 + eggPulse * 2, 32 + eggPulse * 2), owner.Content.EggSprite);
            GUI.Label(new Rect(85, 142, 240, 30), eggs.ToString(), numeric);
            GUI.Label(state.Phase == SessionPhase.Shop ? new Rect(360, 28, 300, 42) : new Rect(810, 28, 300, 42), (state.Endless ? "∞ " : "") + "WAVE " + state.Wave, title);
            if (state.Phase == SessionPhase.Combat)
                GUI.Label(new Rect(870, 74, 210, 32), CombatTimerCaption(simulation.IsBossOvertime, simulation.WaveDuration - simulation.Elapsed), numeric);
            DrawControls(owner);
            if (state.Phase == SessionPhase.Combat)
            {
                if (owner.Content.LootRevision > 0 && PresentationSettings.LootIndicators) lootIndicators.Draw(simulation, owner.Content, camera, small);
                if (simulation.Boss != null && simulation.Boss.Alive)
                {
                    Sprite(new Rect(360, 941, 48, 52), simulation.Boss.Definition.Body);
                    Fill(new Rect(420, 950, 1080, 38), Panel);
                    Fill(new Rect(425, 955, 1070 * simulation.Boss.Health / simulation.Boss.Definition.Health, 28), new Color(0.8f, 0.26f, 0.3f));
                    GUI.Label(new Rect(810, 950, 440, 35), Mathf.CeilToInt(simulation.Boss.Health) + " / " + simulation.Boss.Definition.Health.ToString("0"), numeric);
                }
                if (simulation.Phase == RunPhase.Paused)
                {
                    Fill(new Rect(640, 360, 640, 320), Panel);
                    GUI.Label(new Rect(680, 395, 560, 60), "PAUSED", title);
                    if (navigation.Button(new Rect(750, 515, 420, 65), "RESUME", button)) owner.TogglePause();
                    if (navigation.Button(new Rect(750, 605, 420, 55), "MAIN MENU", button)) owner.RequestMenuReturn();
                }
            }
            else if (state.Phase == SessionPhase.Shop) { owner.Shop.DrawPriceTags(camera); DrawShop(owner); }
            else
            {
                if (!owner.ResultsReady)
                {
                    GUI.Label(new Rect(550, 840, 820, 70), state.Phase == SessionPhase.Defeat ? "THE BRIGADE HAS FALLEN…" : "DAWN HAS ARRIVED…", title);
                    GUI.matrix = previous;
                    GUI.skin = previousSkin;
                    navigation.End();
                    return;
                }
                Fill(new Rect(80, 200, 630, 750), Panel);
                ArcadeSkin.Header(new Rect(110, 228, 575, 105), "RECORD CLOSED / NO STAT REWARDS", state.Phase == SessionPhase.Victory ? "DAWN HAS ARRIVED" : "UNTIL THE NEXT DREAM");
                Sprite(new Rect(220, 355, 270, 270), owner.Content.Unit(state.ChampionId).Body);
                Sprite(new Rect(475, 480, 100, 100), owner.Content.Unit(state.ChampionId).Weapon.Sprite);
                GUI.Label(new Rect(115, 655, 565, 90), owner.MetaNotice, small);
                if (state.Phase == SessionPhase.Victory && navigation.Button(new Rect(115, 750, 560, 60), "ENDLESS · KEEP BRIGADE AND RELICS", button)) owner.ContinueEndless();
                if (navigation.Button(new Rect(115, 825, 270, 65), "PLAY AGAIN", button)) owner.RestartRun();
                if (navigation.Button(new Rect(405, 825, 270, 65), "MAIN MENU", button)) owner.ReturnToMenu();
                invoice.Draw(new Rect(770, 200, 1060, 685), state, owner.Content);
                if (navigation.Button(new Rect(1025, 915, 570, 55), "SAVE RECEIPT IMAGE · PNG", button))
                    try { RunInvoice.ExportFrame(state.RunId); } catch (System.Exception exception) { Debug.LogWarning("Receipt export failed: " + exception.Message); }
            }
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(65, 130, 1800, 70), owner.SaveError, text);
            if (state.Phase == SessionPhase.Combat || state.Phase == SessionPhase.Shop) DrawBonds(owner);
            navigation.End();
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }

        private void DrawShop(ArenaController owner)
        {
            if (owner.HallExitPending) return;
            if (owner.TransitionPending)
            {
                GUI.Label(new Rect(550, 720, 820, 70), "WAVE CLEARED", title);
                return;
            }
            WorldShop shop = owner.Shop;
            if (shop == null) return;
            shop.DrawMerchantDialogue();
            if (owner.Simulation.Phase == RunPhase.Paused)
            {
                Fill(new Rect(710, 440, 500, 180), Panel);
                if (navigation.Button(new Rect(755, 485, 410, 75), "RESUME", button)) owner.TogglePause();
                return;
            }
            WorldShop.Station station = shop.Nearby;
            if (station != null)
            {
                if (station.Action == "NEXT HALL")
                {
                    GUI.Label(new Rect(730, 738, 460, 32), "↓ Next wave · " + RunUIHints.Interact(RunUIHints.Device), small);
                    if (navigation.Button(new Rect(860, 778, 200, 48), "CONTINUE ↓", button)) shop.Interact();
                    return;
                }
                if (tooltipStation != station) { tooltipStation = station; shopScroll = Vector2.zero; }
                string description = station.Offer != null ? shop.DescribeOffer(station.Offer) : station.Recipe != null ? owner.Session.RecipePreview(station.Recipe) : station.Description;
                float contentHeight = GlossaryUI.Height(description, small, 670) + 8;
                Rect panel = ShopTooltipBounds(contentHeight);
                Fill(panel, Panel);
                GUI.Label(new Rect(panel.x + 24, panel.y + 16, 690, 38), shop.DisplayName(station), text);
                shopScroll = GUI.BeginScrollView(new Rect(panel.x + 24, panel.y + 64, 710, panel.height - 85), shopScroll, new Rect(0, 0, 675, contentHeight));
                GlossaryUI.Text(new Rect(0, 0, 670, contentHeight), description, small);
                GUI.EndScrollView();
                if (navigation.Button(new Rect(panel.xMax - 230, panel.y + 14, 205, 46), RunUIHints.Device == RunUIDevice.Controller ? "A · MUA" : RunUIHints.Device == RunUIDevice.Touch ? "MUA" : "Space · MUA", button)) shop.Interact();
                GlossaryUI.Draw(new Vector2(1920, 1080));
            }
        }
        public static Rect ShopTooltipBounds(float contentHeight)
        {
            float height = Mathf.Clamp(contentHeight + 85, 155, 185);
            return new Rect(485, 870 - height, 950, height);
        }
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

        private void DrawBonds(ArenaController owner)
        {
            bondFeedback.Observe(owner.Session.State.RunId, owner.Content.Bonds, owner.Session.BondCounts, owner.Simulation.VisualElapsed);
            int index = 0;
            string tooltip = null;
            foreach (BondDefinition bond in owner.Content.Bonds)
            {
                if (!owner.Session.BondCounts.TryGetValue(bond.Id, out int count) || count <= 0) continue;
                Rect badge = new Rect(1550 + index % 4 * 74, 38 + index / 4 * 88, 64, 76);
                int level = bond.ActiveLevel(count);
                bool active = level > 0;
                bool flashing = bondFeedback.Flashing(bond.Id, owner.Simulation.VisualElapsed);
                if (flashing) ArcadeSkin.Panel(badge, ArcadeSkin.Accent);
                if (active) ArcadeSkin.Line(new Rect(badge.x + 7, badge.y + 8, 50, 43), new Color(ArcadeSkin.Teal.r, ArcadeSkin.Teal.g, ArcadeSkin.Teal.b, 0.16f));
                float burst = owner.Profile != null && owner.Profile.ReducedMotion ? 0 : bondFeedback.Burst(bond.Id, owner.Simulation.VisualElapsed);
                Rect icon = new Rect(badge.x + 4 - burst * 5, badge.y - burst * 5, 56 + burst * 10, 52 + burst * 10);
                Sprite(icon, bond.Icon);
                small.normal.textColor = active ? ArcadeSkin.Teal : ArcadeSkin.Cream;
                small.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(badge.x, badge.y + 52, 64, 24), count + "/" + bond.NextThreshold(count), small);
                if (bondHit == null) bondHit = new GUIStyle();
                if (GUI.Button(badge, GUIContent.none, bondHit)) { selectedBond = bond.Id; bondTooltipUntil = Time.unscaledTime + 3; }
                if (badge.Contains(Event.current.mousePosition) || selectedBond == bond.Id && Time.unscaledTime < bondTooltipUntil)
                {
                    tooltip = RunUICopy.BondDescription(bond, count);
                }
                index++;
            }
            small.normal.textColor = ArcadeSkin.Cream; small.alignment = TextAnchor.UpperLeft;
            if (tooltip != null)
            {
                float top = 50 + Mathf.CeilToInt(index / 4f) * 88;
                float height = GlossaryUI.Height(tooltip, small, 385) + 24;
                Fill(new Rect(1430, top, 420, height), Panel);
                GlossaryUI.Text(new Rect(1445, top + 12, 385, height - 24), tooltip, small);
                GlossaryUI.Draw(new Vector2(1920, 1080));
            }
        }

        private void DrawReceipt(RunSnapshot state)
        {
            if (state.Receipts.Count == 0) return;
            RunTransactionReceipt receipt = state.Receipts[state.Receipts.Count - 1];
            if (receiptId != receipt.Id) { receiptId = receipt.Id; receiptScroll = Vector2.zero; receiptOpen = false; }
            if (navigation.Button(new Rect(1460, 205, 370, 52), receiptOpen ? "CLOSE RECEIPT" : "RECEIPT SAVED", button)) receiptOpen = !receiptOpen;
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
            GUI.Label(new Rect(520, 345, 880, 80), "RETURN TO MAIN MENU?", title);
            GUI.Label(new Rect(540, 445, 840, 105), "Saves commit at wave start or after shop transactions. Unfinished combat restarts from the saved boundary, not the pause moment.", text);
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(540, 550, 840, 70), owner.SaveError, small);
            if (navigation.Button(new Rect(550, 650, 360, 65), "STAY", button)) owner.CancelMenuReturn();
            if (navigation.Button(new Rect(1010, 650, 360, 65), "SAVE AND RETURN", button)) owner.ReturnToMenu();
            navigation.End();
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }
    }
}
