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
        private bool receiptOpen;
        private int receiptId;
        private Vector2 receiptScroll;
        private readonly LootIndicators lootIndicators = new LootIndicators();
        private readonly RunInvoice invoice = new RunInvoice();
        private readonly ArcadeNavigation navigation = new ArcadeNavigation();
        public void ReadNavigation(bool enabled) { if (enabled) navigation.ReadInput(); }
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
            float health = state.Phase == SessionPhase.Combat ? simulation.Champion.Health : state.Health;
            float maximum = Mathf.Max(1f, owner.Content.Unit(state.ChampionId).Health + session.Bonus(ItemEffect.Health));
            Sprite(new Rect(35, 28, 62, 62), owner.Content.Unit(state.ChampionId).Body);
            ArcadeSkin.Line(new Rect(105, 36, 230, 30), new Color(0.06f, 0.07f, 0.12f, 0.85f));
            ArcadeSkin.Line(new Rect(108, 39, 224 * Mathf.Clamp01(health / maximum), 24), new Color(0.94f, 0.24f, 0.36f));
            GUI.Label(new Rect(135, 36, 200, 32), Mathf.CeilToInt(health) + " / " + maximum.ToString("0"), numeric);
            Sprite(new Rect(40, 100, 32, 32), owner.Content.Arena.Coin);
            GUI.Label(new Rect(85, 102, 240, 30), RunSession.BoundedCounter((long)state.Gold + (state.Phase == SessionPhase.Combat ? simulation.Gold : 0)).ToString(), numeric);
            Sprite(new Rect(40, 140, 32, 32), owner.Content.EggSprite);
            GUI.Label(new Rect(85, 142, 240, 30), RunSession.BoundedCounter((long)state.Eggs + (state.Phase == SessionPhase.Combat ? simulation.Eggs : 0)).ToString(), numeric);
            GUI.Label(new Rect(810, 28, 300, 42), (state.Endless ? "∞ " : "") + "WAVE " + state.Wave, title);
            if (state.Phase == SessionPhase.Combat)
                GUI.Label(new Rect(885, 74, 150, 32), Mathf.Max(0, Mathf.CeilToInt(owner.Content.WaveSeconds - simulation.Elapsed)).ToString(), numeric);
            if (navigation.Button(new Rect(1770, 35, 65, 52), "II", button)) owner.TogglePause();
            if (navigation.Button(new Rect(1680, 35, 65, 52), "I", button)) owner.ToggleDetails();
            if (state.Phase == SessionPhase.Combat)
            {
                if (owner.Content.LootRevision > 0 && PresentationSettings.LootIndicators) lootIndicators.Draw(simulation, owner.Content, camera, small);
                if (simulation.Boss != null && simulation.Boss.Alive)
                {
                    Fill(new Rect(420, 950, 1080, 38), Panel);
                    Fill(new Rect(425, 955, 1070 * simulation.Boss.Health / simulation.Boss.Definition.Health, 28), new Color(0.8f, 0.26f, 0.3f));
                }
                if (simulation.Phase == RunPhase.Paused)
                {
                    Fill(new Rect(640, 360, 640, 320), Panel);
                    GUI.Label(new Rect(680, 395, 560, 60), "PAUSED", title);
                    if (navigation.Button(new Rect(750, 515, 420, 65), "RESUME", button)) owner.TogglePause();
                    if (navigation.Button(new Rect(750, 605, 420, 55), "VỀ MENU", button)) owner.RequestMenuReturn();
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
                Fill(new Rect(80, 200, 630, 750), Panel);
                ArcadeSkin.Header(new Rect(110, 228, 575, 105), "HỒ SƠ ĐÃ KHÉP / KHÔNG THƯỞNG CHỈ SỐ", state.Phase == SessionPhase.Victory ? "BÌNH MINH ĐÃ TỚI" : "HẸN Ở GIẤC MƠ SAU");
                Sprite(new Rect(220, 355, 270, 270), owner.Content.Unit(state.ChampionId).Body);
                Sprite(new Rect(475, 480, 100, 100), owner.Content.Unit(state.ChampionId).Weapon.Sprite);
                GUI.Label(new Rect(115, 655, 565, 90), owner.MetaNotice, small);
                if (state.Phase == SessionPhase.Victory && navigation.Button(new Rect(115, 750, 560, 60), "TIẾP TỤC ENDLESS · GIỮ BUILD", button)) owner.ContinueEndless();
                if (navigation.Button(new Rect(115, 825, 270, 65), "CHƠI LẠI", button)) owner.RestartRun();
                if (navigation.Button(new Rect(405, 825, 270, 65), "VỀ MENU", button)) owner.ReturnToMenu();
                invoice.Draw(new Rect(770, 200, 1060, 685), state, owner.Content);
                if (navigation.Button(new Rect(1025, 915, 570, 55), "LƯU HÓA ĐƠN · PNG LOCAL", button))
                    try { RunInvoice.ExportFrame(state.RunId); } catch (System.Exception exception) { Debug.LogWarning("Receipt export failed: " + exception.Message); }
            }
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(65, 130, 1800, 70), owner.SaveError, text);
            navigation.End();
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
            if (owner.Simulation.Phase == RunPhase.Paused)
            {
                Fill(new Rect(710, 440, 500, 180), Panel);
                if (navigation.Button(new Rect(755, 485, 410, 75), "RESUME", button)) owner.TogglePause();
                return;
            }
            WorldShop.Station station = shop.Nearby;
            if (station != null)
            {
                Fill(new Rect(580, 860, 760, 125), Panel);
                string caption = station.Action;
                if (station.Offer != null) caption += " · " + station.Offer.Price;
                GUI.Label(new Rect(610, 878, 520, 32), caption, text);
                string description = station.Offer?.Kind == OfferKind.Item ? owner.Content.Item(station.Offer.Id).Description : station.Offer != null ? owner.Content.Unit(station.Offer.Id).Description : station.Recipe != null ? owner.Session.RecipePreview(station.Recipe) : station.Description;
                GUI.Label(new Rect(610, 922, 505, 54), description, small);
                GUI.enabled = GUI.enabled && (station.Offer == null || owner.Session.CanBuy(station.Offer)) && (station.Recipe == null || owner.Session.CanAltar(station.Recipe));
                if (navigation.Button(new Rect(1150, 889, 160, 65), "SPACE", button)) shop.Interact();
                GUI.enabled = !owner.Details.Open && !owner.MenuConfirmation;
            }
        }

        private void DrawReceipt(RunSnapshot state)
        {
            if (state.Receipts.Count == 0) return;
            RunTransactionReceipt receipt = state.Receipts[state.Receipts.Count - 1];
            if (receiptId != receipt.Id) { receiptId = receipt.Id; receiptScroll = Vector2.zero; receiptOpen = false; }
            if (navigation.Button(new Rect(1460, 205, 370, 52), receiptOpen ? "ĐÓNG BIÊN LAI" : "BIÊN LAI ĐÃ LƯU", button)) receiptOpen = !receiptOpen;
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
            navigation.Begin();
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            Fill(new Rect(470, 310, 980, 440), Panel);
            GUI.Label(new Rect(520, 345, 880, 80), "VỀ MENU?", title);
            GUI.Label(new Rect(540, 445, 840, 105), "Lượt chơi được lưu tại đầu hall hoặc sau giao dịch shop. Combat đang diễn ra sẽ bắt đầu lại từ mốc lưu, không phải thời điểm đang pause.", text);
            if (!string.IsNullOrEmpty(owner.SaveError)) GUI.Label(new Rect(540, 550, 840, 70), owner.SaveError, small);
            if (navigation.Button(new Rect(550, 650, 360, 65), "Ở LẠI / ESC", button)) owner.CancelMenuReturn();
            if (navigation.Button(new Rect(1010, 650, 360, 65), "LƯU VÀ VỀ MENU", button)) owner.ReturnToMenu();
            navigation.End();
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }
    }
}
