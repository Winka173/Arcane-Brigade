using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class RunLaunch { public static string ChampionId; public static bool Resume; public static int Difficulty; public static int ChallengeIndex; }
    public sealed class RunMenu : MonoBehaviour
    {
        public enum Page { Landing, Roster, Resume, Settings, Help, Records, ConfirmOverwrite, WeaponSandbox, Museum }
        [SerializeField] private RunCatalog catalog;
        private RunStorage storage;
        private PlayerProfile profile;
        private RunSnapshot saved;
        private GUIStyle heading;
        private GUIStyle text;
        private GUIStyle button;
        private int selection;
        private Vector2 scroll;
        private string error;
        private bool existingSave;
        private WeaponSandbox sandbox;
        private Page sandboxReturnPage;
        private Vector2 sandboxReturnScroll;
        private bool diagnosticState;
        private int difficulty;
        private int challenge;
        private int historySelection;
        private readonly RunMuseum museum = new RunMuseum();
        private readonly RunInvoice invoice = new RunInvoice();
        private Texture2D shopIllustration;
        private Texture2D landingShade;
        private GUIStyle logoStyle;
        private readonly ArcadeNavigation navigation = new ArcadeNavigation();
        private bool focusLost;
        private bool backgrounded;
        private float pageChangedAt;
        public RunMuseum Museum => museum;
        public void NavigateForDiagnostics(int direction, bool submit)
        {
            if (!RunPaths.K7Capture) throw new InvalidOperationException("Navigation fixture requires isolated mode");
            navigation.Move(direction); if (submit) navigation.QueueSubmit();
        }
        public void ConfigureK7Diagnostics(RunCatalog definition, PlayerProfile readOnlyProfile)
        {
            if (!RunPaths.K7Capture) throw new InvalidOperationException("K7 fixture requires isolated mode");
            catalog = definition; profile = readOnlyProfile; PresentationSettings.Apply(profile);
        }
        public void SelectHistoryForDiagnostics(int index, bool evidence)
        {
            if (!RunPaths.K6Capture) throw new InvalidOperationException("History fixture requires isolated K6 capture mode");
            historySelection = Mathf.Clamp(index, 0, Mathf.Max(0, profile.History.Count - 1));
            if (evidence) invoice.ScrollToEnd();
        }
        public void ConfigureMetaDiagnostics(RunCatalog definition, PlayerProfile readOnlyProfile, RunSnapshot readOnlySaved)
        {
            if (!RunPaths.K6Capture) throw new InvalidOperationException("K6 fixture requires isolated capture mode");
            diagnosticState = true; catalog = definition; profile = readOnlyProfile; saved = readOnlySaved;
            ArcadeSkin.Bundle = profile.Cosmetic;
        }
        public WeaponSandbox Sandbox => sandbox;
        public WeaponSandbox GetSandbox() => sandbox;
        public Page CurrentPage { get; private set; }
        public void Configure(RunCatalog definition) { catalog = definition; }
        public void ShowPage(Page page)
        {
            if (page == Page.WeaponSandbox) { OpenWeaponSandbox(); return; }
            if (CurrentPage == Page.WeaponSandbox && sandbox != null) sandbox.Dispose();
            CurrentPage = page;
            navigation.Reset(); pageChangedAt = Time.unscaledTime;
            scroll = Vector2.zero;
        }
        public void OpenWeaponSandbox()
        {
            if (profile == null || catalog == null) return;
            EnterWeaponSandbox(catalog, profile, saved, CurrentPage == Page.Museum ? museum.SelectedWeapon : null, 1, false);
        }
        public void OpenWeaponSandboxForDiagnostics(RunCatalog definition, PlayerProfile readOnlyProfile,
            RunSnapshot readOnlySaved = null, string weaponId = null, int tier = 1, bool moving = false)
        {
            diagnosticState = true;
            catalog = definition;
            profile = readOnlyProfile;
            saved = readOnlySaved;
            EnterWeaponSandbox(definition, readOnlyProfile, readOnlySaved, weaponId, tier, moving);
        }
        private void EnterWeaponSandbox(RunCatalog definition, PlayerProfile readOnlyProfile,
            RunSnapshot readOnlySaved, string weaponId, int tier, bool moving)
        {
            if (CurrentPage != Page.WeaponSandbox) { sandboxReturnPage = CurrentPage; sandboxReturnScroll = scroll; }
            if (sandbox == null) sandbox = gameObject.AddComponent<WeaponSandbox>();
            sandbox.Open(definition, readOnlyProfile, readOnlySaved, weaponId, tier, moving);
            CurrentPage = Page.WeaponSandbox;
        }
        public void CloseWeaponSandbox()
        {
            if (CurrentPage != Page.WeaponSandbox) return;
            sandbox.Dispose();
            CurrentPage = sandboxReturnPage;
            scroll = sandboxReturnScroll;
        }
        private void OnDisable() { CloseWeaponSandbox(); }
        public static bool CanResumeSnapshot(RunSnapshot snapshot) => snapshot != null && Enum.IsDefined(typeof(SessionPhase), snapshot.Phase);
        private bool CanResume => CanResumeSnapshot(saved);

        private void Start()
        {
            if (diagnosticState) return;
            storage = new RunStorage(RunPaths.Root);
            profile = storage.LoadProfile();
            ArcadeSkin.Bundle = profile.Cosmetic;
            error = storage.LastError;
            ReloadSaveSummary();
            PresentationSettings.Apply(profile);
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            gameObject.AddComponent<ArcadeMusic>();
            gameObject.AddComponent<CheckpointAudio>().ConfigurePresentation();
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        public void ReloadSaveSummary()
        {
            using (var session = new RunSession(catalog))
            {
                saved = storage.LoadRun(session);
                if (saved != null)
                {
                    session.Restore(saved); saved = session.State;
                    if ((saved.Phase == SessionPhase.Victory || saved.Phase == SessionPhase.Defeat) && !profile.CompletedRuns.Contains(saved.RunId))
                        try { storage.RecordProgress(profile, saved, catalog); } catch (Exception exception) { error = "Hồ sơ chờ lưu lại: " + exception.Message; }
                }
            }
            if (!string.IsNullOrEmpty(storage.LastError)) error = storage.LastError;
            existingSave = File.Exists(Path.Combine(RunPaths.Root, "run.json")) || File.Exists(Path.Combine(RunPaths.Root, "run.json.bak"));
        }

        private void Update()
        {
            if (CurrentPage != Page.Museum && CurrentPage != Page.Records && CurrentPage != Page.WeaponSandbox) navigation.ReadInput();
            if (CurrentPage == Page.Records && profile != null && profile.History.Count > 0)
            {
                int direction = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.JoystickButton3) ? 1 : Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.JoystickButton2) ? -1 : 0;
                historySelection = Mathf.Clamp(historySelection + direction, 0, profile.History.Count - 1);
                if (direction != 0) scroll.y = Mathf.Max(0, historySelection * 95 - 250);
            }
            if (CurrentPage == Page.Museum)
            {
                museum.HandleInput();
                if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0)) && museum.SelectedWeapon != null) OpenWeaponSandbox();
            }
            if (CurrentPage == Page.WeaponSandbox)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) CloseWeaponSandbox();
                else sandbox.HandleInput();
                return;
            }
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) ShowPage(CurrentPage == Page.ConfirmOverwrite ? Page.Roster : Page.Landing);
        }

        private void Launch(bool resume)
        {
            if (resume && !CanResume || !resume && profile.BestWave < catalog.Champions[selection].UnlockWave) return;
            if (!resume && existingSave)
            {
                try
                {
                    string archive = Path.Combine(RunPaths.Root, "ArchivedRuns", DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(archive);
                    foreach (string name in new[] { "run.json", "run.json.bak" })
                    {
                        string source = Path.Combine(RunPaths.Root, name);
                        if (File.Exists(source)) File.Copy(source, Path.Combine(archive, name));
                    }
                }
                catch (Exception exception) { error = "Không thể backup lượt cũ: " + exception.Message; return; }
            }
            RunLaunch.Resume = resume;
            RunLaunch.ChampionId = catalog.Champions[selection].Id;
            RunLaunch.Difficulty = challenge > 0 ? 0 : difficulty;
            RunLaunch.ChallengeIndex = challenge;
            SceneManager.LoadScene("RunArena");
        }

        private bool Button(Rect rectangle, string caption) => navigation.Button(rectangle, caption, button);
        private void Label(Rect rectangle, string caption) { GUI.Label(rectangle, ArcadeTypography.DisplayText(caption), text); }

        private void OnGUI()
        {
            if (profile == null) return;
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            navigation.Begin();
            if (text == null)
            {
                heading = ArcadeTypography.Style(38, true);
                text = ArcadeTypography.Style(19);
                button = new GUIStyle(GUI.skin.button) { fontSize = 19, font = ArcadeTypography.Heading };
            }
            Matrix4x4 previous = ArcadeTypography.BeginViewport(new Rect(0, 0, Screen.width, Screen.height), 1280, 720);
            ArcadeSkin.Line(new Rect(0, 0, 1280, 720), ArcadeSkin.Ink);
            if (CurrentPage != Page.Landing)
            {
                RunHud.Fill(new Rect(20, 15, 1240, 690), new Color(0.12f, 0.10f, 0.20f));
                ArcadeSkin.Header(new Rect(60, 32, 940, 92), "", "ARCANE BRIGADE");
            }
            if (CurrentPage != Page.Landing && Button(new Rect(1010, 40, 200, 50), "QUAY LẠI"))
            {
                if (CurrentPage == Page.WeaponSandbox) CloseWeaponSandbox();
                else ShowPage(CurrentPage == Page.ConfirmOverwrite ? Page.Roster : Page.Landing);
            }
            switch (CurrentPage)
            {
                case Page.Landing: DrawLanding(); break;
                case Page.Roster: DrawRoster(); break;
                case Page.Resume:
                    Label(new Rect(110, 200, 1060, 230), CanResume ? catalog.Unit(saved.ChampionId).DisplayName + " · Hall " + saved.Wave + "\n" + saved.Phase + " · Vàng " + saved.Gold + " · Trứng " + saved.Eggs + "\n" + saved.Roster.Count + " đồng minh · " + saved.Items.Count + " loại cổ vật\n" + (saved.Phase == SessionPhase.Victory || saved.Phase == SessionPhase.Defeat ? "Mở lại hóa đơn đã chốt, không nhận thưởng hai lần. Victory vẫn cho tiếp tục Endless." : "Combat chưa xong sẽ chơi lại từ đầu hall; shop tiếp tục ở giao dịch cuối cùng.") : "Không có mốc lưu hợp lệ.");
                    GUI.enabled = CanResume;
                    if (Button(new Rect(420, 490, 440, 70), "TIẾP TỤC")) Launch(true);
                    GUI.enabled = true;
                    break;
                case Page.ConfirmOverwrite:
                    Label(new Rect(120, 210, 1040, 180), "Lượt mới thay mốc tiếp tục hiện tại.\nLượt cũ được backup trong ArchivedRuns trước khi chuyển cảnh. Không xóa hồ sơ hoặc kỷ lục.\nNếu backup thất bại, lượt mới không được mở.");
                    if (Button(new Rect(180, 490, 400, 70), "GIỮ LƯỢT CŨ")) ShowPage(Page.Roster);
                    if (Button(new Rect(700, 490, 400, 70), "BACKUP VÀ TẠO LƯỢT MỚI")) Launch(false);
                    break;
                case Page.Settings: DrawSettings(); break;
                case Page.Help:
                    Label(new Rect(110, 185, 1060, 410), "DI CHUYỂN: WASD / mũi tên / kéo ngón tay vùng dưới bên trái.\nNhân vật và đồng minh tự đánh; không có nút kỹ năng.\nSHOP: đi tới quầy, SPACE / USE để mua; không mua tự động. Nâng cấp cũng tốn tiền.\nĐồng minh bất tử, không chặn đạn mặc định. Không phí nuôi, không bán hoặc khóa shop.\nI / ĐỒ: Stats, Companions, Relics. TAB đổi tab; I / ESC đóng.\nESC pause hoặc đóng lớp đang mở. Mất focus / vào nền giữ pause riêng.\nHồ sơ chỉ lưu unlock, cosmetic và kỷ lục; không tăng stat vĩnh viễn.");
                    break;
                case Page.Records: DrawRecords(); break;
                case Page.Museum:
                    museum.Draw(catalog, profile);
                    GUI.enabled = museum.SelectedWeapon != null;
                    if (Button(new Rect(850, 625, 355, 45), "THỬ MÓN ĐÃ KHÁM PHÁ")) OpenWeaponSandbox();
                    GUI.enabled = true;
                    break;
                case Page.WeaponSandbox: sandbox.DrawPanel(text, button); break;
            }
            if (CurrentPage != Page.WeaponSandbox && !string.IsNullOrEmpty(error)) Label(new Rect(60, 675, 1150, 28), error);
            if (!profile.ReducedMotion)
            {
                float alpha = Mathf.Clamp01(1 - (Time.unscaledTime - pageChangedAt) / 0.16f);
                if (alpha > 0) ArcadeSkin.Line(new Rect(25, 145, 1230, 540), new Color(0.10f, 0.08f, 0.17f, alpha * 0.3f));
            }
            navigation.End();
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }

        private void DrawLanding()
        {
            if (shopIllustration == null) shopIllustration = Resources.Load<Texture2D>("LostShop_v1");
            if (shopIllustration != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), shopIllustration, ScaleMode.ScaleAndCrop);
            if (landingShade == null)
            {
                landingShade = new Texture2D(64, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[64];
                for (int index = 0; index < pixels.Length; index++) pixels[index] = new Color(0.035f, 0.05f, 0.08f, Mathf.Lerp(0.97f, 0, index / 63f));
                landingShade.SetPixels(pixels); landingShade.Apply(false, true);
            }
            GUI.DrawTexture(new Rect(0, 0, 864, 720), landingShade);
            GUIStyle logo = logoStyle ?? (logoStyle = ArcadeTypography.Style(60, true));
            if (ArcadeTypography.Display != null) logo.font = ArcadeTypography.Display;
            GUI.Label(new Rect(75, 85, 655, 165), "ARCANE\nBRIGADE", logo);
            UnitDefinition hero = catalog.Champions[selection];
            Sprite pose = hero.AnimationFrames.Length >= 4 ? hero.AnimationFrames[UnitVisual.AnimationFrame(Time.unscaledTime, selection, false, profile.ReducedMotion)] : hero.Body;
            RunHud.Sprite(new Rect(850, 340 + (profile.ReducedMotion ? 0 : Mathf.Sin(Time.unscaledTime * 1.7f) * 5), 240, 260), pose);
            if (Button(new Rect(75, 285, 390, 68), "CHƠI MỚI")) ShowPage(Page.Roster);
            GUI.enabled = CanResume;
            if (Button(new Rect(75, 365, 390, 52), "TIẾP TỤC")) ShowPage(Page.Resume);
            GUI.enabled = true;
            if (Button(new Rect(75, 429, 390, 52), "BỘ SƯU TẬP")) ShowPage(Page.Museum);
            if (Button(new Rect(75, 493, 390, 52), "KỶ LỤC")) ShowPage(Page.Records);
            if (Button(new Rect(75, 567, 185, 45), "CÀI ĐẶT")) ShowPage(Page.Settings);
            if (Button(new Rect(280, 567, 185, 45), "CÁCH CHƠI")) ShowPage(Page.Help);
            Label(new Rect(75, 660, 560, 28), "Chỉ cần di chuyển. Đồ vật sẽ lo phần còn lại.");
        }

        private void DrawRoster()
        {
            scroll = GUI.BeginScrollView(new Rect(65, 180, 660, 395), scroll, new Rect(0, 0, 620, Mathf.Max(380, catalog.Champions.Length * 130)));
            for (int index = 0; index < catalog.Champions.Length; index++)
            {
                UnitDefinition unit = catalog.Champions[index];
                Rect row = new Rect(0, index * 130, 610, 120);
                if (navigation.Button(row, "", GUIStyle.none)) selection = index;
                RunHud.Fill(row, index == selection ? new Color(0.27f, 0.20f, 0.38f) : new Color(0.17f, 0.13f, 0.25f));
                RunHud.Sprite(new Rect(15, row.y + 10, 90, 95), unit.Body);
                Label(new Rect(125, row.y + 15, 460, 90), unit.DisplayName + "\n" + (profile.BestWave >= unit.UnlockWave ? "Sẵn sàng" : "Đạt hall " + unit.UnlockWave + " để mở khóa"));
            }
            GUI.EndScrollView();
            UnitDefinition champion = catalog.Champions[selection];
            RunHud.Sprite(new Rect(910, 170, 110, 120), champion.Body);
            Label(new Rect(770, 295, 440, 90), champion.DisplayName + "\n" + champion.Description);
            if (Button(new Rect(65, 595, 660, 55), "THỬ THÁCH: " + RunMeta.ChallengeNames[challenge])) challenge = (challenge + 1) % RunMeta.ChallengeIds.Length;
            GUI.enabled = challenge == 0;
            if (Button(new Rect(775, 405, 420, 55), "ĐỘ KHÓ: " + RunMeta.DifficultyNames[difficulty])) difficulty = (difficulty + 1) % (RunMeta.UnlockedDifficulty(profile) + 1);
            GUI.enabled = true;
            GUIStyle modeNote = ArcadeTypography.Style(16);
            GUI.Label(new Rect(775, 470, 430, 44), challenge > 0 ? "Seed cố định · bậc 1 · điểm riêng theo Champion" : "Thắng bậc trước để mở bậc tiếp. Không buff vĩnh viễn.", modeNote);
            GUI.enabled = profile.BestWave >= champion.UnlockWave;
            if (Button(new Rect(775, 520, 420, 65), "BẮT ĐẦU")) { if (existingSave) ShowPage(Page.ConfirmOverwrite); else Launch(false); }
            GUI.enabled = true;
        }

        private void DrawSettings()
        {
            DrawVolume(175, "Âm lượng tổng", profile.Volume, value => profile.Volume = value);
            DrawVolume(230, "Nhạc", profile.MusicVolume, value => profile.MusicVolume = value);
            DrawVolume(285, "Hiệu ứng", profile.SfxVolume, value => profile.SfxVolume = value);
            if (Button(new Rect(75, 350, 540, 55), "BỘ: " + RunMeta.CosmeticName(profile.Cosmetic)))
            {
                int before = profile.Cosmetic;
                int candidate = (before + 1) % 3;
                while (!RunMeta.CosmeticUnlocked(profile, candidate)) candidate = (candidate + 1) % 3;
                profile.Cosmetic = candidate;
                if (!SaveProfile()) profile.Cosmetic = before;
                PresentationSettings.Apply(profile);
            }
            ToggleOption(new Rect(645, 350, 560, 55), "GIẢM CHUYỂN ĐỘNG", profile.ReducedMotion, value => profile.ReducedMotion = value);
            ToggleOption(new Rect(75, 425, 540, 55), "GIẢM DỒN ÂM THANH", profile.ReducedAudio, value => profile.ReducedAudio = value);
            ToggleOption(new Rect(645, 425, 560, 55), "TƯƠNG PHẢN UI CAO", profile.HighReadability, value => profile.HighReadability = value);
            ToggleOption(new Rect(75, 500, 540, 55), "SỐ DAMAGE", profile.ShowDamageNumbers, value => profile.ShowDamageNumbers = value);
            ToggleOption(new Rect(645, 500, 560, 55), "CHỈ BÁO LOOT", profile.ShowLootIndicators, value => profile.ShowLootIndicators = value);
            Label(new Rect(75, 580, 1130, 85), "Tab / stick / mũi tên: chọn · Enter / A: xác nhận · Escape / B: quay lại\nVăn phòng đêm: hall 5; Sân khấu giấy: 3 boss khác nhau. Chỉ presentation, không đổi gameplay.\nBe Vietnam Pro + Noto Sans Mono · SIL OFL 1.1.");
        }

        private void DrawVolume(float top, string caption, float value, Action<float> setter)
        {
            Label(new Rect(75, top, 310, 40), caption + ": " + Mathf.RoundToInt(value * 100) + "%");
            float changed = GUI.HorizontalSlider(new Rect(390, top + 16, 630, 25), value, 0, 1);
            if (Button(new Rect(1050, top, 65, 42), "−")) changed = Mathf.Max(0, value - 0.1f);
            if (Button(new Rect(1135, top, 65, 42), "+")) changed = Mathf.Min(1, value + 0.1f);
            if (Mathf.Abs(changed - value) < 0.005f) return;
            setter(changed);
            if (!SaveProfile()) setter(value);
            PresentationSettings.Apply(profile);
        }

        private void ToggleOption(Rect rectangle, string caption, bool value, Action<bool> setter)
        {
            if (!Button(rectangle, caption + (value ? ": BẬT" : ": TẮT"))) return;
            setter(!value);
            if (!SaveProfile()) setter(value);
            PresentationSettings.Apply(profile);
        }

        private void OnApplicationFocus(bool focus)
        {
            if (RunPaths.Diagnostic) return;
            focusLost = !focus; AudioListener.pause = focusLost || backgrounded;
        }
        private void OnApplicationPause(bool paused) { backgrounded = paused; AudioListener.pause = focusLost || backgrounded; }

        private void DrawRecords()
        {
            Label(new Rect(60, 165, 575, 80), "Hall " + profile.BestWave + " / Thắng " + profile.Wins + " / Kills " + profile.TotalKills + "\nBậc đã mở: " + (RunMeta.UnlockedDifficulty(profile) + 1) + " / 5");
            scroll = GUI.BeginScrollView(new Rect(60, 250, 560, 350), scroll, new Rect(0, 0, 520, Mathf.Max(345, profile.History.Count * 95 + profile.Records.Count * 65 + profile.Achievements.Count * 40 + 65)));
            float vertical = 0;
            for (int index = 0; index < profile.History.Count; index++)
            {
                RunRecord record = profile.History[index];
                if (Button(new Rect(0, vertical, 515, 85), record.Nickname + "\n" + record.Score.ToString("N0") + " điểm · " + record.Build.ChampionId + " · " + (record.Build.Endless ? "Endless" : "Normal"))) historySelection = index;
                vertical += 95;
            }
            Label(new Rect(0, vertical, 510, 50), "KỶ LỤC THEO CHAMPION / MODE / BẬC"); vertical += 60;
            foreach (ScoreRecord record in profile.Records) { Label(new Rect(0, vertical, 510, 60), record.Key + "\n" + record.Score.ToString("N0") + " điểm · hall " + record.Hall); vertical += 65; }
            foreach (string achievement in profile.Achievements) { Label(new Rect(0, vertical, 510, 40), "✓ " + achievement); vertical += 40; }
            GUI.EndScrollView();
            if (profile.History.Count > 0)
            {
                RunRecord record = profile.History[Mathf.Clamp(historySelection, 0, profile.History.Count - 1)];
                invoice.Draw(new Rect(660, 160, 545, 465), record.Build, catalog, record.Nickname, record.Score);
                if (Button(new Rect(660, 640, 545, 42), "LƯU ẢNH HÓA ĐƠN · LOCAL PNG"))
                    try { error = "Ảnh: " + RunInvoice.ExportFrame(record.Id); } catch (Exception exception) { error = exception.Message; }
            }
            else Label(new Rect(690, 240, 470, 200), "Chưa có hóa đơn mới. Kết thúc một lượt để lưu đội/vũ khí/đóng góp thật.\nSave cũ giữ kỷ lục, không dựng lại lịch sử chưa đo.");
        }

        private bool SaveProfile()
        {
            try { storage.SaveProfile(profile); error = ""; return true; }
            catch (Exception exception) { error = "Lưu cài đặt thất bại: " + exception.Message; return false; }
        }
    }
}
