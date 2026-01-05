using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class RunLaunch { public static string ChampionId; public static bool Resume; }
    public sealed class RunMenu : MonoBehaviour
    {
        public enum Page { Landing, Roster, Resume, Settings, Help, Records, ConfirmOverwrite }
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
        public Page CurrentPage { get; private set; }
        public void Configure(RunCatalog definition) { catalog = definition; }
        public void ShowPage(Page page) { CurrentPage = page; scroll = Vector2.zero; }
        private bool CanResume => saved != null && (saved.Phase == SessionPhase.Combat || saved.Phase == SessionPhase.Shop);

        private void Start()
        {
            storage = new RunStorage(RunPaths.Root);
            profile = storage.LoadProfile();
            error = storage.LastError;
            ReloadSaveSummary();
            AudioListener.volume = profile.Volume;
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            gameObject.AddComponent<ArcadeMusic>();
            Screen.orientation = ScreenOrientation.LandscapeLeft;
        }

        public void ReloadSaveSummary()
        {
            using (var session = new RunSession(catalog)) saved = storage.LoadRun(session);
            if (!string.IsNullOrEmpty(storage.LastError)) error = storage.LastError;
            existingSave = File.Exists(Path.Combine(RunPaths.Root, "run.json")) || File.Exists(Path.Combine(RunPaths.Root, "run.json.bak"));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) ShowPage(CurrentPage == Page.ConfirmOverwrite ? Page.Roster : Page.Landing);
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
            SceneManager.LoadScene("RunArena");
        }

        private bool Button(Rect rectangle, string caption) => GUI.Button(rectangle, caption, button);
        private void Label(Rect rectangle, string caption) { GUI.Label(rectangle, caption, text); }

        private void OnGUI()
        {
            if (profile == null) return;
            GUISkin previousSkin = GUI.skin;
            GUI.skin = ArcadeSkin.Skin;
            if (text == null)
            {
                heading = ArcadeTypography.Style(38, true);
                text = ArcadeTypography.Style(21);
                button = new GUIStyle(GUI.skin.button) { fontSize = 22, font = ArcadeTypography.Heading };
            }
            Matrix4x4 previous = ArcadeTypography.BeginViewport(new Rect(0, 0, Screen.width, Screen.height), 1280, 720);
            RunHud.Fill(new Rect(20, 15, 1240, 690), new Color(0.10f, 0.08f, 0.19f));
            GUI.Label(new Rect(60, 35, 940, 65), "ARCANE BRIGADE", heading);
            Label(new Rect(60, 105, 1120, 48), "Đội quân kỳ quặc · Chỉ di chuyển · Mọi đòn đánh tự động");
            if (CurrentPage != Page.Landing && Button(new Rect(1010, 40, 200, 50), "QUAY LẠI")) ShowPage(CurrentPage == Page.ConfirmOverwrite ? Page.Roster : Page.Landing);
            switch (CurrentPage)
            {
                case Page.Landing: DrawLanding(); break;
                case Page.Roster: DrawRoster(); break;
                case Page.Resume:
                    Label(new Rect(110, 200, 1060, 230), CanResume ? catalog.Unit(saved.ChampionId).DisplayName + " · Hall " + saved.Wave + "\n" + saved.Phase + " · Vàng " + saved.Gold + " · Trứng " + saved.Eggs + "\n" + saved.Roster.Count + " đồng minh · " + saved.Items.Count + " loại cổ vật\nCombat chưa xong sẽ chơi lại từ đầu hall; shop tiếp tục ở giao dịch cuối cùng." : "Không có mốc lưu hợp lệ.");
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
            }
            if (!string.IsNullOrEmpty(error)) Label(new Rect(60, 630, 1150, 65), error);
            GUI.matrix = previous;
            GUI.skin = previousSkin;
        }

        private void DrawLanding()
        {
            RunHud.Fill(new Rect(70, 180, 560, 410), new Color(0.17f, 0.13f, 0.25f));
            RunHud.Sprite(new Rect(245, 200, 220, 240), catalog.Champions[selection].Body);
            Label(new Rect(105, 470, 490, 95), "20 hall tới bình minh.\nĐồng minh, cổ vật và bond tạo build trong từng lượt.");
            if (Button(new Rect(710, 185, 470, 65), "LƯỢT CHƠI MỚI")) ShowPage(Page.Roster);
            GUI.enabled = CanResume;
            if (Button(new Rect(710, 267, 470, 65), "TIẾP TỤC LƯỢT ĐÃ LƯU")) ShowPage(Page.Resume);
            GUI.enabled = true;
            if (Button(new Rect(710, 349, 470, 65), "HỒ SƠ / BỘ SƯU TẬP")) ShowPage(Page.Records);
            if (Button(new Rect(710, 431, 470, 65), "CÀI ĐẶT")) ShowPage(Page.Settings);
            if (Button(new Rect(710, 513, 470, 65), "CÁCH CHƠI")) ShowPage(Page.Help);
        }

        private void DrawRoster()
        {
            scroll = GUI.BeginScrollView(new Rect(65, 180, 660, 395), scroll, new Rect(0, 0, 620, Mathf.Max(380, catalog.Champions.Length * 130)));
            for (int index = 0; index < catalog.Champions.Length; index++)
            {
                UnitDefinition unit = catalog.Champions[index];
                Rect row = new Rect(0, index * 130, 610, 120);
                if (GUI.Button(row, "", GUIStyle.none)) selection = index;
                RunHud.Fill(row, index == selection ? new Color(0.27f, 0.20f, 0.38f) : new Color(0.17f, 0.13f, 0.25f));
                RunHud.Sprite(new Rect(15, row.y + 10, 90, 95), unit.Body);
                Label(new Rect(125, row.y + 15, 460, 90), unit.DisplayName + "\n" + (profile.BestWave >= unit.UnlockWave ? "Sẵn sàng" : "Đạt hall " + unit.UnlockWave + " để mở khóa"));
            }
            GUI.EndScrollView();
            UnitDefinition champion = catalog.Champions[selection];
            RunHud.Sprite(new Rect(870, 175, 170, 175), champion.Body);
            Label(new Rect(770, 370, 440, 130), champion.DisplayName + "\n" + champion.Description);
            GUI.enabled = profile.BestWave >= champion.UnlockWave;
            if (Button(new Rect(775, 520, 420, 65), "BẮT ĐẦU")) { if (existingSave) ShowPage(Page.ConfirmOverwrite); else Launch(false); }
            GUI.enabled = true;
        }

        private void DrawSettings()
        {
            Label(new Rect(130, 195, 980, 60), "Âm lượng tổng: " + Mathf.RoundToInt(profile.Volume * 100) + "%");
            float volume = GUI.HorizontalSlider(new Rect(130, 285, 980, 35), profile.Volume, 0, 1);
            if (Mathf.Abs(volume - profile.Volume) > 0.005f)
            {
                float before = profile.Volume;
                profile.Volume = volume;
                if (!SaveProfile()) profile.Volume = before;
                AudioListener.volume = profile.Volume;
            }
            GUI.enabled = profile.BestWave >= 5;
            if (Button(new Rect(130, 365, 650, 65), profile.Cosmetic == 0 ? "MARKER: VÀNG" : "MARKER: NGỌC LAM"))
            {
                int before = profile.Cosmetic;
                profile.Cosmetic = 1 - before;
                if (!SaveProfile()) profile.Cosmetic = before;
            }
            GUI.enabled = true;
            Label(new Rect(130, 455, 980, 130), "Cosmetic mở ở hall 5, không tăng sức mạnh.\nNoto Sans / Black / Mono — SIL OFL 1.1.\nBộ chữ nền có tiếng Việt; lettering độc quyền sẽ polish sau.");
        }

        private void DrawRecords()
        {
            float height = 130 + profile.Achievements.Count * 55 + catalog.Companions.Length * 80 + catalog.Items.Length * 80;
            scroll = GUI.BeginScrollView(new Rect(90, 180, 1100, 415), scroll, new Rect(0, 0, 1050, height));
            Label(new Rect(15, 10, 1010, 95), "Hall cao nhất: " + profile.BestWave + " · Thắng: " + profile.Wins + " · Kills: " + profile.TotalKills + "\nBộ sưu tập chỉ hiện nội dung đã gặp thật.");
            float vertical = 130;
            foreach (string achievement in profile.Achievements) { Label(new Rect(15, vertical, 1010, 50), achievement); vertical += 55; }
            foreach (UnitDefinition unit in catalog.Companions) { Label(new Rect(15, vertical, 1010, 75), profile.SeenUnits.Contains(unit.Id) ? unit.DisplayName + " — " + unit.Description : "??? Đồng minh chưa gặp"); vertical += 80; }
            foreach (ItemDefinition item in catalog.Items) { Label(new Rect(15, vertical, 1010, 75), profile.SeenItems.Contains(item.Id) ? item.Name + " — " + item.Description : "??? Cổ vật chưa gặp"); vertical += 80; }
            GUI.EndScrollView();
        }

        private bool SaveProfile()
        {
            try { storage.SaveProfile(profile); error = ""; return true; }
            catch (Exception exception) { error = "Lưu cài đặt thất bại: " + exception.Message; return false; }
        }
    }
}
