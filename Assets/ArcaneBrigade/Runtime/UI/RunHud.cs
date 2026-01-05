using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class RunHud
    {
        private GUIStyle text;
        private GUIStyle small;
        private GUIStyle title;
        private GUIStyle button;
        private bool receiptOpen;
        private int receiptId;
        private Vector2 receiptScroll;
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
            if (text == null)
            {
                text = ArcadeTypography.Style(23);
                small = new GUIStyle(text) { fontSize = 18 };
                title = ArcadeTypography.Style(34, true);
                title.alignment = TextAnchor.MiddleCenter;
                button = new GUIStyle(GUI.skin.button) { fontSize = 22, font = ArcadeTypography.Heading };
            }
            RunSession session = owner.Session;
            RunSnapshot state = session.State;
            CombatSimulation simulation = owner.Simulation;
            Rect viewport = camera.pixelRect;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(viewport, 1920f, 1080f);
            Fill(new Rect(40, 25, 525, 108), Color.Lerp(Panel, new Color(0.36f, 0.23f, 0.10f, 0.97f), owner.WalletPulse * 0.65f));
            Fill(new Rect(670, 25, 575, 78), Panel);
            Sprite(new Rect(48, 40, 68, 68), owner.Content.Unit(state.ChampionId).Body);
            float health = state.Phase == SessionPhase.Combat ? simulation.Champion.Health : state.Health;
            float maximum = Mathf.Max(1f, owner.Content.Unit(state.ChampionId).Health + session.Bonus(ItemEffect.Health));
            Fill(new Rect(125, 87, 390, 18), new Color(0.22f, 0.12f, 0.25f));
            Fill(new Rect(125, 87, 390 * Mathf.Clamp01(health / maximum), 18), new Color(0.95f, 0.2f, 0.37f));
            GUI.Label(new Rect(125, 38, 440, 45), Mathf.CeilToInt(health) + " / " + maximum.ToString("0") + "   GOLD " + RunSession.BoundedCounter((long)state.Gold + (state.Phase == SessionPhase.Combat ? simulation.Gold : 0)) + "   EGGS " + state.Eggs, text);
            GUI.Label(new Rect(695, 42, 550, 55), (state.Endless ? "ENDLESS " : "HALL ") + state.Wave + "   ALLIES " + state.Roster.Count + "   " + (state.Phase == SessionPhase.Combat ? Mathf.Max(0, Mathf.CeilToInt(owner.Content.WaveSeconds - simulation.Elapsed)) + "s" : state.Phase.ToString()), text);
            if (GUI.Button(new Rect(1650, 43, 190, 55), "MENU", button)) owner.RequestMenuReturn();
            if (GUI.Button(new Rect(1260, 43, 175, 55), "ĐỒ / I", button)) owner.ToggleDetails();
            if (state.Phase == SessionPhase.Shop && GUI.Button(new Rect(1450, 43, 175, 55), "PAUSE", button)) owner.TogglePause();
            if (state.Phase == SessionPhase.Combat)
            {
                if (GUI.Button(new Rect(1450, 43, 175, 55), "PAUSE", button)) owner.TogglePause();
                if (simulation.Boss != null && simulation.Boss.Alive)
                {
                    Fill(new Rect(420, 950, 1080, 38), Panel);
                    Fill(new Rect(425, 955, 1070 * simulation.Boss.Health / simulation.Boss.Definition.Health, 28), new Color(0.8f, 0.26f, 0.3f));
                    GUI.Label(new Rect(700, 900, 700, 45), simulation.Boss.Definition.DisplayName + " — DEFEAT THE BOSS", text);
                }
                GUI.Label(new Rect(75, 1010, 1680, 45), "MOVE: WASD / arrows / touch drag     F3: radius guide     " + BondSummary(session, owner.Content), small);
                if (session.ActiveModifier != null) GUI.Label(new Rect(75, 142, 1150, 50), session.ActiveModifier.Name, text);
                if (simulation.Phase == RunPhase.Paused)
                {
                    Fill(new Rect(640, 360, 640, 320), Panel);
                    GUI.Label(new Rect(680, 395, 560, 60), "PAUSED", title);
                    if (GUI.Button(new Rect(750, 515, 420, 65), "RESUME", button)) owner.TogglePause();
                    GUI.Label(new Rect(680, 610, 560, 45), "Combat resume uses the last wave boundary save.", small);
                }
            }
            else if (state.Phase == SessionPhase.Shop) DrawShop(owner);
            else
            {
                if (!owner.ResultsReady)
                {
                    GUI.Label(new Rect(550, 840, 820, 70), state.Phase == SessionPhase.Defeat ? "ĐỘI QUÂN GỤC NGÃ…" : "BÌNH MINH ĐÃ TỚI…", title);
                    GUI.matrix = previous;
                    GUI.skin = previousSkin;
                    return;
                }
                Fill(new Rect(430, 280, 1060, 580), Panel);
                GUI.Label(new Rect(480, 325, 960, 70), state.Phase == SessionPhase.Victory ? "DAWN REACHED" : "BRIGADE FALLEN", title);
                GUI.Label(new Rect(560, 430, 850, 130), "Hall " + state.Wave + "   Kills " + state.TotalKills + "   Bosses " + state.BossKills + "\nCombat time " + (state.CombatSeconds / 60f).ToString("0.0") + " min\nUnlocks, codex and records are saved; no permanent stat grind.", text);
                if (state.Phase == SessionPhase.Defeat) GUI.Label(new Rect(560, 570, 850, 70), "Last hit: " + state.LastDamageSource + " / " + state.LastDamageAmount.ToString("0.0") + " damage", small);
                if (state.Phase == SessionPhase.Victory && GUI.Button(new Rect(675, 600, 570, 65), "CONTINUE INTO ENDLESS", button)) owner.ContinueEndless();
                if (GUI.Button(new Rect(675, 700, 570, 65), "NEW RUN", button)) owner.RestartRun();
            }
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(65, 130, 1800, 70), owner.SaveError, text);
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }

        private void DrawShop(ArenaController owner)
        {
            if (owner.TransitionPending)
            {
                GUI.Label(new Rect(550, 840, 820, 70), "HALL CLEARED", title);
                return;
            }
            WorldShop shop = owner.Shop;
            if (shop == null) return;
            GUI.Label(new Rect(75, 145, 1450, 55), (shop.Special ? "DREAM ALTAR · EGGS / HP" : "CURIOSITY EXCHANGE") + "   Income +" + owner.Session.State.LastIncome + "   Interest +" + owner.Session.State.LastInterest, text);
            GUI.Label(new Rect(75, 1010, 1600, 45), "MOVE: WASD / arrows / touch drag. Approach a stand, then SPACE / USE. No automatic purchase.", small);
            if (owner.Simulation.Phase == RunPhase.Paused)
            {
                Fill(new Rect(710, 440, 500, 180), Panel);
                if (GUI.Button(new Rect(755, 485, 410, 75), "RESUME", button)) owner.TogglePause();
                return;
            }
            WorldShop.Station station = shop.Nearby;
            if (station != null)
            {
                Fill(new Rect(440, 750, 1040, 245), Panel);
                string caption = station.Action;
                if (station.Action == "REROLL") caption += "   " + owner.Session.RerollCost + " GOLD";
                if (station.Offer != null) caption += "   " + station.Offer.Price + " " + (station.Offer.Currency == ShopCurrency.Health ? "HP HIỆN TẠI" : station.Offer.Currency.ToString().ToUpperInvariant());
                GUI.Label(new Rect(465, 765, 730, 45), caption, text);
                string description = station.Description;
                if (station.Offer != null && station.Offer.Kind == OfferKind.Item)
                {
                    ItemDefinition item = owner.Content.Item(station.Offer.Id);
                    description += "Đang có ×" + owner.Session.StackCount(item.Id) + (owner.Session.StackCount(item.Id) >= item.StackLimit ? " · ĐÃ ĐẠT GIỚI HẠN" : "");
                }
                GUI.Label(new Rect(465, 815, 730, 175), description, small);
                if (station.Offer != null && !station.Offer.Sold && station.Offer.Currency == ShopCurrency.Health) GUI.Label(new Rect(1215, 810, 235, 55), "HP sau trả: " + (owner.Session.State.Health - station.Offer.Price).ToString("0.#"), small);
                GUI.enabled = GUI.enabled && (station.Offer == null || owner.Session.CanBuy(station.Offer));
                if (GUI.Button(new Rect(1215, 870, 235, 75), "SPACE / USE", button)) shop.Interact();
                GUI.enabled = !owner.Details.Open && !owner.MenuConfirmation;
            }
            GUI.Label(new Rect(75, 205, 1600, 50), owner.Session.Notice, small);
            DrawReceipt(owner.Session.State);
        }

        private void DrawReceipt(RunSnapshot state)
        {
            if (state.Receipts.Count == 0) return;
            RunTransactionReceipt receipt = state.Receipts[state.Receipts.Count - 1];
            if (receiptId != receipt.Id) { receiptId = receipt.Id; receiptScroll = Vector2.zero; receiptOpen = false; }
            if (GUI.Button(new Rect(1460, 205, 370, 52), receiptOpen ? "ĐÓNG BIÊN LAI" : "BIÊN LAI ĐÃ LƯU", button)) receiptOpen = !receiptOpen;
            if (!receiptOpen) return;
            float contentHeight = small.CalcHeight(new GUIContent(receipt.Description), 425) + 20;
            float panelHeight = Mathf.Clamp(contentHeight + 110, 190, 470);
            Fill(new Rect(1320, 275, 510, panelHeight), Panel);
            GUI.Label(new Rect(1340, 295, 465, 45), receipt.Title, text);
            float height = Mathf.Max(panelHeight - 105, contentHeight);
            receiptScroll = GUI.BeginScrollView(new Rect(1340, 355, 470, panelHeight - 105), receiptScroll, new Rect(0, 0, 430, height));
            GUI.Label(new Rect(0, 0, 425, height), receipt.Description, small);
            GUI.EndScrollView();
        }

        private static string BondSummary(RunSession session, RunCatalog catalog)
        {
            string summary = "BONDS: ";
            foreach (BondDefinition bond in catalog.Bonds)
                if (session.BondCounts.TryGetValue(bond.Id, out int count)) summary += bond.Name + " " + count + "/" + bond.Threshold + "  ";
            return summary;
        }

        public void DrawMenuConfirmation(ArenaController owner, Camera camera)
        {
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            Fill(new Rect(470, 310, 980, 440), Panel);
            GUI.Label(new Rect(520, 345, 880, 80), "VỀ MENU?", title);
            GUI.Label(new Rect(540, 445, 840, 105), "Lượt chơi được lưu tại đầu hall hoặc sau giao dịch shop. Combat đang diễn ra sẽ bắt đầu lại từ mốc lưu, không phải thời điểm đang pause.", text);
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(540, 550, 840, 70), owner.SaveError, small);
            if (GUI.Button(new Rect(550, 650, 360, 65), "Ở LẠI / ESC", button)) owner.CancelMenuReturn();
            if (GUI.Button(new Rect(1010, 650, 360, 65), "LƯU VÀ VỀ MENU", button)) owner.ReturnToMenu();
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }
    }
}
