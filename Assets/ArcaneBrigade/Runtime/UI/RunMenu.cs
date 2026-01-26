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
        private GUIStyle primary;
        private GUIStyle logoShadow;
        private GUIStyle chip;
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
        private GUIStyle portraitCaption;
        private GUIStyle rosterNote;
        private GUIStyle rosterTitle;
        private Vector2 rosterDescriptionScroll;
        private readonly ArcadeNavigation navigation = new ArcadeNavigation();
        private bool focusLost;
        private bool backgrounded;
        private float pageChangedAt;
        private ArcadeMusic music;
        public RunMuseum Museum => museum;
        public void SelectChampionForDiagnostics(int index)
        {
            if (!RunPaths.K7Capture || index < 0 || index >= catalog.Champions.Length) throw new InvalidOperationException("Champion fixture requires isolated capture mode");
            selection = index; rosterDescriptionScroll = Vector2.zero; ShowPage(Page.Roster);
        }
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
            if (page != Page.Museum) museum.Close();
            if (page == Page.WeaponSandbox) { OpenWeaponSandbox(); return; }
            if (CurrentPage == Page.WeaponSandbox && sandbox != null) sandbox.Dispose();
            if (CurrentPage != page) CheckpointAudio.PlayInterfaceId(page == Page.Landing ? "ui.back" : "ui.open");
            CurrentPage = page;
            GlossaryUI.Close();
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
            museum.Close();
            if (CurrentPage != Page.WeaponSandbox) { sandboxReturnPage = CurrentPage; sandboxReturnScroll = scroll; }
            if (sandbox == null) sandbox = gameObject.AddComponent<WeaponSandbox>();
            sandbox.Open(definition, readOnlyProfile, readOnlySaved, weaponId, tier, moving);
            CurrentPage = Page.WeaponSandbox;
            pageChangedAt = Time.unscaledTime;
        }
        public void CloseWeaponSandbox()
        {
            if (CurrentPage != Page.WeaponSandbox) return;
            sandbox.Dispose();
            CurrentPage = sandboxReturnPage;
            pageChangedAt = Time.unscaledTime;
            scroll = sandboxReturnScroll;
        }
        private void OnDisable() { CloseWeaponSandbox(); museum.Close(); }
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
            music = gameObject.AddComponent<ArcadeMusic>();
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
                        try { storage.RecordProgress(profile, saved, catalog); } catch (Exception exception) { error = "Profile awaiting retry: " + exception.Message; }
                }
            }
            if (!string.IsNullOrEmpty(storage.LastError)) error = storage.LastError;
            existingSave = File.Exists(Path.Combine(RunPaths.Root, "run.json")) || File.Exists(Path.Combine(RunPaths.Root, "run.json.bak"));
        }

        private void Update()
        {
            if (music != null) music.ForceScene(CurrentPage == Page.Roster ? ArcaneCueLibrary.SceneSelect : null);
            RunUIHints.Refresh();
            if (CurrentPage != Page.Museum && CurrentPage != Page.Records && CurrentPage != Page.WeaponSandbox)
            {
                int before = navigation.Selected;
                navigation.ReadInput();
                int focused = navigation.Selected - 1;
                if (CurrentPage == Page.Roster && before != navigation.Selected && focused >= 0 && focused < catalog.Champions.Length)
                {
                    Rect card = RosterCardBounds(focused);
                    scroll.y = Mathf.Clamp(scroll.y, Mathf.Max(0, card.yMax - 390), card.y);
                }
            }
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
            CheckpointAudio.PlayInterfaceId("ui.confirm");
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
                catch (Exception exception) { error = "Cannot back up old run: " + exception.Message; CheckpointAudio.PlayInterfaceId("ui.error"); return; }
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
                primary = ArcadeSkin.Primary(button);
                chip = ArcadeTypography.Style(14, true);
                chip.alignment = TextAnchor.MiddleCenter;
                chip.normal.textColor = ArcadeSkin.Ink;
            }
            Matrix4x4 previous = ArcadeTypography.BeginViewport(new Rect(0, 0, Screen.width, Screen.height), 1280, 720);
            Color previousTint = GUI.color;
            LivingUI.Enter(Time.unscaledTime - pageChangedAt, new Vector2(640, 360));
            GlossaryUI.BlockUnderlying(new Vector2(1280, 720));
            ArcadeSkin.Line(new Rect(0, 0, 1280, 720), ArcadeSkin.Ink);
            if (CurrentPage != Page.Landing)
            {
                if (shopIllustration == null) shopIllustration = Resources.Load<Texture2D>("LostShop_v1");
                if (shopIllustration != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), shopIllustration, ScaleMode.ScaleAndCrop);
                ArcadeSkin.Line(new Rect(0, 0, 1280, 720), new Color(0.05f, 0.04f, 0.09f, PresentationSettings.HighReadability ? 0.97f : 0.86f));
                ArcadeSkin.Header(new Rect(60, 32, 940, 92), PageEyebrow(CurrentPage), PageTitle(CurrentPage));
            }
            if (CurrentPage != Page.Landing && Button(new Rect(1010, 40, 200, 50), "BACK"))
            {
                if (CurrentPage == Page.WeaponSandbox) CloseWeaponSandbox();
                else ShowPage(CurrentPage == Page.ConfirmOverwrite ? Page.Roster : Page.Landing);
            }
            switch (CurrentPage)
            {
                case Page.Landing: DrawLanding(); break;
                case Page.Roster: DrawRoster(); break;
                case Page.Resume:
                    Label(new Rect(110, 200, 1060, 230), CanResume ? RunUICopy.UnitName(catalog.Unit(saved.ChampionId)) + " · Wave " + saved.Wave + "\n" + RunUICopy.PhaseName(saved.Phase) + " · Gold " + saved.Gold + " · Eggs " + saved.Eggs + "\n" + saved.Roster.Count + " companions · " + saved.Items.Count + " relic types\n" + (saved.Phase == SessionPhase.Victory || saved.Phase == SessionPhase.Defeat ? "Reopens the settled receipt without claiming rewards twice. Victorious runs can still enter endless mode." : "Unfinished combat restarts at wave start; shops resume after the last transaction.") : "No valid saved boundary.");
                    GUI.enabled = CanResume;
                    if (navigation.Button(new Rect(420, 490, 440, 70), "RESUME", primary)) Launch(true);
                    GUI.enabled = true;
                    break;
                case Page.ConfirmOverwrite:
                    Label(new Rect(120, 210, 1040, 180), "A new run replaces the current resume boundary.\nThe old run is backed up before combat. Profile and records are not deleted.\nIf backup fails, the new run does not begin.");
                    if (Button(new Rect(180, 490, 400, 70), "KEEP OLD RUN")) ShowPage(Page.Roster);
                    if (navigation.Button(new Rect(700, 490, 400, 70), "BACK UP AND START", primary)) Launch(false);
                    break;
                case Page.Settings: DrawSettings(); break;
                case Page.Help:
                    Label(new Rect(110, 185, 1060, 410), "MOVE: WASD / arrows / stick / drag the lower-left area.\nChampion and companions attack automatically; no skill buttons.\nSHOP: approach a stand, press Space / A or tap USE. Purchases are not automatic.\nCompanions are immortal, do not block bullets by default, and require no upkeep. Stock cannot be sold or locked.\nBRIGADE: notebook icon / I / Y opens stats, companions, relics and links.\nTab or LB/RB changes tabs. I / Y / Esc / B closes.\nPAUSE: pause icon / Esc / Start or B. Leaving the app also pauses.\nClearing each wave restores full champion HP. Melee companions patrol and pursue with wider roaming and visible foot dust.\nProfile stores unlocks, cosmetics and records only, without permanent stat growth.");
                    break;
                case Page.Records: DrawRecords(); break;
                case Page.Museum:
                    museum.Draw(catalog, profile);
                    GUI.enabled = museum.SelectedWeapon != null;
                    if (Button(new Rect(850, 625, 355, 45), "TRY DISCOVERED GEAR")) OpenWeaponSandbox();
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
            GlossaryUI.Draw(new Vector2(1280, 720));
            GUI.matrix = previous;
            GUI.color = previousTint;
            GUI.skin = previousSkin;
        }

        // Diegetic main menu: every entry is an object on the shop counter painted in LostShop_v1 (1280x720 design space).
        private static readonly string[] SpotLabels = { "NEW RUN", "RESUME", "COLLECTION", "RECORDS", "SETTINGS", "HOW TO PLAY" };
        private static readonly string[] SpotHints = { "Step onto the stage", "Back into tonight's show", "The display case", "The receipt printer", "Adjust the lights", "Ring for help" };
        private static readonly string[] SpotCues = { "menu.spot.hero", "menu.spot.door", "menu.spot.jar", "menu.spot.printer", "menu.spot.lamp", "menu.spot.bell" };
        public static readonly Rect[] SpotAreas =
        {
            new Rect(505, 330, 230, 280),  // the champion on the lit counter
            new Rect(752, 150, 135, 240),  // arched doorway into the night
            new Rect(1105, 0, 150, 140),   // glass display jar
            new Rect(1030, 380, 200, 190), // receipt printer
            new Rect(548, 0, 200, 96),     // moon lamp
            new Rect(292, 172, 86, 96),    // shop bell
        };
        private int hoveredSpot = -1;
        private GUIStyle smallButton;
        private GUIStyle spotHint;

        private bool SpotEnabled(int index) => index != 1 || CanResume;
        private void ActivateSpot(int index)
        {
            if (!SpotEnabled(index)) { CheckpointAudio.PlayInterfaceId("ui.locked"); return; }
            switch (index)
            {
                case 0: ShowPage(Page.Roster); break;
                case 1: ShowPage(Page.Resume); break;
                case 2: ShowPage(Page.Museum); break;
                case 3: ShowPage(Page.Records); break;
                case 4: ShowPage(Page.Settings); break;
                default: ShowPage(Page.Help); break;
            }
        }

        private void DrawLanding()
        {
            if (shopIllustration == null) shopIllustration = Resources.Load<Texture2D>("LostShop_v1");
            if (shopIllustration != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), shopIllustration, ScaleMode.ScaleAndCrop);
            if (landingShade == null)
            {
                landingShade = new Texture2D(64, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[64];
                for (int index = 0; index < pixels.Length; index++) pixels[index] = new Color(0.035f, 0.05f, 0.08f, Mathf.Lerp(0.85f, 0, index / 63f));
                landingShade.SetPixels(pixels); landingShade.Apply(false, true);
            }
            GUI.DrawTexture(new Rect(0, 0, 520, 200), landingShade);
            ArcadeSkin.Line(new Rect(0, 650, 1280, 70), new Color(0.03f, 0.03f, 0.06f, 0.82f));
            float kick = BeatPulse.Kick;
            GUIStyle logo = logoStyle ?? (logoStyle = ArcadeTypography.Style(56, true));
            if (ArcadeTypography.Display != null) logo.font = ArcadeTypography.Display;
            if (logoShadow == null) { logoShadow = new GUIStyle(logo); logoShadow.normal.textColor = ArcadeSkin.Accent; }
            GUI.Label(new Rect(45, 25 - kick * 3, 600, 150), "ARCANE\nBRIGADE", logoShadow);
            logo.normal.textColor = ArcadeSkin.Cream;
            GUI.Label(new Rect(40, 20 - kick * 3, 600, 150), "ARCANE\nBRIGADE", logo);

            int mouseSpot = -1;
            for (int index = 0; index < SpotAreas.Length; index++)
                if (SpotEnabled(index) && SpotAreas[index].Contains(Event.current.mousePosition)) mouseSpot = index;
            int focus = mouseSpot >= 0 ? mouseSpot : Mathf.Clamp(navigation.Selected, 0, SpotAreas.Length - 1);
            if (Event.current.type == EventType.Repaint && mouseSpot >= 0 && mouseSpot != hoveredSpot) CheckpointAudio.PlayInterfaceId(SpotCues[mouseSpot]);
            if (Event.current.type == EventType.Repaint) hoveredSpot = mouseSpot;

            // idle twinkles walk across the objects on the beat so every object reads as interactive
            for (int index = 1; index < SpotAreas.Length; index++)
            {
                Rect area = SpotAreas[index];
                float twinkle = ((BeatPulse.Beat + index) % 5 == 0 ? kick : 0) + 0.15f;
                if (!SpotEnabled(index)) twinkle = 0.05f;
                ArcadeSkin.Glow(new Rect(area.center.x - 40, area.center.y - 40, 80, 80), new Color(1f, 0.85f, 0.5f, 0.35f * twinkle));
            }
            Rect focused = SpotAreas[focus];
            float glowPad = 30 + kick * 8;
            ArcadeSkin.Glow(new Rect(focused.x - glowPad, focused.y - glowPad, focused.width + glowPad * 2, focused.height + glowPad * 2), new Color(1f, 0.82f, 0.45f, 0.45f + kick * 0.15f));

            UnitDefinition hero = catalog.Champions[selection];
            Sprite pose = hero.AnimationFrames.Length >= 4 ? hero.AnimationFrames[UnitVisual.AnimationFrame(Time.unscaledTime, selection, focus == 0, profile.ReducedMotion)] : hero.Body;
            Rect heroRect = new Rect(SpotAreas[0].x, SpotAreas[0].y + (profile.ReducedMotion ? 0 : Mathf.Sin(Time.unscaledTime * 1.7f) * 5) - kick * 4, SpotAreas[0].width, SpotAreas[0].height - 20);
            ArcadeSkin.Glow(new Rect(heroRect.x + 20, heroRect.yMax - 40, heroRect.width - 40, 60), new Color(0, 0, 0, 0.5f));
            Matrix4x4 unflipped = GUI.matrix;
            if (focus != 0 && SpotAreas[focus].center.x < heroRect.center.x) GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), heroRect.center);
            RunHud.Sprite(heroRect, pose);
            GUI.matrix = unflipped;

            if (spotHint == null) { spotHint = ArcadeTypography.Style(14, false); spotHint.alignment = TextAnchor.MiddleCenter; spotHint.normal.textColor = ArcadeSkin.Cream; }
            Vector2 tagSize = new Vector2(Mathf.Max(170, chip.CalcSize(new GUIContent(SpotLabels[focus])).x + 44), 30);
            bool below = focused.yMax < 300;
            float tagY = below ? focused.yMax + 6 : focused.y - 58;
            Rect tag = new Rect(Mathf.Clamp(focused.center.x - tagSize.x * 0.5f, 10, 1270 - tagSize.x), tagY - kick * 3, tagSize.x, tagSize.y);
            ArcadeSkin.Slant(new Rect(tag.x + 3, tag.y + 4, tag.width, tag.height), new Color(0, 0, 0, 0.5f));
            ArcadeSkin.Slant(tag, focus == 0 ? ArcadeSkin.Accent : ArcadeSkin.Cream);
            chip.normal.textColor = focus == 0 ? ArcadeSkin.Cream : ArcadeSkin.Ink;
            GUI.Label(tag, focus == 1 && CanResume ? "RESUME · WAVE " + saved.Wave : SpotLabels[focus], chip);
            chip.normal.textColor = ArcadeSkin.Ink;
            GUI.Label(new Rect(tag.x - 40, tag.yMax + 1, tag.width + 80, 20), SpotHints[focus], spotHint);

            for (int index = 0; index < SpotAreas.Length; index++)
                if (GUI.Button(SpotAreas[index], GUIContent.none, GUIStyle.none)) ActivateSpot(index);

            // keyboard / controller / readability row mirrors the objects one-to-one
            if (smallButton == null) smallButton = new GUIStyle(button) { fontSize = 15 };
            for (int index = 0; index < SpotLabels.Length; index++)
            {
                GUI.enabled = SpotEnabled(index);
                string caption = index == 1 && !CanResume ? "NO SAVED RUN" : SpotLabels[index];
                if (navigation.Button(new Rect(36 + index * 204, 664, 196, 42), caption, index == 0 ? primary : smallButton)) ActivateSpot(index);
            }
            GUI.enabled = true;
        }

        private void DrawRoster()
        {
            if (catalog.Champions == null || catalog.Champions.Length == 0) { Label(new Rect(75, 200, 1100, 90), "No champion available."); return; }
            selection = Mathf.Clamp(selection, 0, catalog.Champions.Length - 1);
            GUIStyle caption = portraitCaption ?? (portraitCaption = ArcadeTypography.Style(18, true));
            caption.alignment = TextAnchor.MiddleCenter;
            GUIStyle note = rosterNote ?? (rosterNote = ArcadeTypography.Style(16));
            Label(new Rect(65, 142, 660, 32), "CHOOSE YOUR CHAMPION");
            scroll = GUI.BeginScrollView(new Rect(65, 180, 660, 395), scroll, new Rect(0, 0, 620, Mathf.Max(380, Mathf.CeilToInt(catalog.Champions.Length / 3f) * 245)));
            for (int index = 0; index < catalog.Champions.Length; index++)
            {
                UnitDefinition unit = catalog.Champions[index];
                bool unlocked = profile.BestWave >= unit.UnlockWave;
                Rect card = RosterCardBounds(index);
                RunHud.Fill(card, index == selection ? new Color(0.27f, 0.20f, 0.38f) : new Color(0.17f, 0.13f, 0.25f));
                if (index == selection) { ArcadeSkin.Glow(new Rect(card.x + 10, card.y, card.width - 20, 150), new Color(0.28f, 0.94f, 0.86f, 0.22f)); ArcadeSkin.Slant(new Rect(card.x + 14, card.yMax - 16, card.width - 28, 8), ArcadeSkin.Teal); }
                if (navigation.Button(card, "", GUIStyle.none)) { if (selection != index) rosterDescriptionScroll = Vector2.zero; selection = index; }
                Color previousColor = GUI.color;
                if (!unlocked) GUI.color = new Color(0.65f, 0.65f, 0.65f);
                float portraitSize = index == selection ? 120 : 110;
                RunHud.Sprite(new Rect(card.center.x - portraitSize * 0.5f, card.y + 12, portraitSize, 115), RosterPortrait(unit, Time.unscaledTime, index, index == selection, profile.ReducedMotion));
                GUI.color = previousColor;
                GUI.Label(new Rect(card.x + 10, card.y + 132, 180, 52), RunUICopy.UnitName(unit), caption);
                if (!unlocked) { ArcadeSkin.Slant(new Rect(card.x + 20, card.y + 186, 160, 26), ArcadeSkin.Cream); GUI.Label(new Rect(card.x + 20, card.y + 186, 160, 26), "LOCKED · WAVE " + unit.UnlockWave, chip); }
            }
            GUI.EndScrollView();
            UnitDefinition champion = catalog.Champions[selection];
            if (catalog.HeroRevision > 0)
            {
                int iconIndex = 0;
                foreach (BondDefinition bond in catalog.Bonds)
                    if (Array.IndexOf(champion.RecruitThemes, bond.Id) >= 0) RunHud.Sprite(new Rect(690 - iconIndex++ * 34, 145, 28, 28), bond.Icon);
                if (iconIndex > 0) GUI.Label(new Rect(690 - iconIndex * 34 - 130, 145, 125, 28), "RECRUIT THEMES", note);
            }
            RunHud.Fill(new Rect(760, 135, 460, 445), new Color(0.12f, 0.10f, 0.20f));
            ArcadeSkin.Glow(new Rect(752, 125, 170, 175), new Color(1f, 0.8f, 0.45f, 0.28f));
            RunHud.Sprite(new Rect(775, 145, 132, 142), RosterPortrait(champion, Time.unscaledTime, selection, true, profile.ReducedMotion));
            if (rosterTitle == null) { rosterTitle = ArcadeTypography.Style(28, true); if (ArcadeTypography.Display != null) rosterTitle.font = ArcadeTypography.Display; }
            GUI.Label(new Rect(918, 150, 290, 70), RunUICopy.UnitName(champion), rosterTitle);
            ArcadeSkin.Slant(new Rect(918, 222, 280, 28), ArcadeSkin.Teal);
            GUI.Label(new Rect(918, 222, 280, 28), RunUICopy.WeaponName(champion.Weapon.Id).ToUpperInvariant(), chip);
            GUI.Label(new Rect(918, 256, 290, 34), RunUICopy.UnlockCondition(champion, profile.BestWave), note);
            string description = RunUICopy.ChampionDescription(champion);
            if (catalog.HeroRevision > 0)
            {
                description += "\n\nRECRUIT THEMES: ";
                foreach (BondDefinition bond in catalog.Bonds)
                    if (Array.IndexOf(champion.RecruitThemes, bond.Id) >= 0) description += bond.Name + " · ";
            }
            description += "\n\nSTARTING TRAITS\n";
            foreach (StatModifier modifier in champion.ChampionModifiers)
                description += RunModifierRules.ScopeName(modifier.Scope) + " / " + RunModifierRules.ModifierName(modifier.Effect) + ": " + RunModifierRules.ValueText(modifier.Effect, modifier.Value) + "\n";
            float descriptionHeight = Mathf.Max(180, GlossaryUI.Height(description, text, 400) + 8);
            rosterDescriptionScroll = GUI.BeginScrollView(new Rect(780, 300, 425, 185), rosterDescriptionScroll, new Rect(0, 0, 405, descriptionHeight));
            GlossaryUI.Text(new Rect(0, 0, 400, descriptionHeight), description, text);
            GUI.EndScrollView();
            if (Button(new Rect(65, 595, 660, 55), "CHALLENGE: " + RunMeta.ChallengeNames[challenge].Replace("clear hall", "cleared waves"))) challenge = (challenge + 1) % RunMeta.ChallengeIds.Length;
            GUI.enabled = challenge == 0;
            if (Button(new Rect(780, 492, 420, 46), "DIFFICULTY: " + RunMeta.DifficultyNames[difficulty])) difficulty = (difficulty + 1) % (RunMeta.UnlockedDifficulty(profile) + 1);
            GUI.enabled = true;
            GUI.Label(new Rect(780, 540, 430, 40), challenge > 0 ? "Same seeded conditions · tier 1 · records per champion" : "Win the previous difficulty to unlock the next. No permanent stat growth.", note);
            GUI.enabled = profile.BestWave >= champion.UnlockWave;
            if (navigation.Button(new Rect(780, 590, 420, 66), profile.BestWave >= champion.UnlockWave ? "START ▶" : "LOCKED", primary)) { if (existingSave) ShowPage(Page.ConfirmOverwrite); else Launch(false); }
            GUI.enabled = true;
            GUI.Label(new Rect(65, 660, 1130, 30), RunUIHints.Menu, note);
        }
        private static string PageTitle(Page page)
        {
            switch (page)
            {
                case Page.Roster: return "PICK YOUR CREW";
                case Page.Resume: return "RESUME RUN";
                case Page.ConfirmOverwrite: return "START OVER?";
                case Page.Settings: return "SETTINGS";
                case Page.Help: return "HOW TO PLAY";
                case Page.Records: return "RECORDS";
                case Page.Museum: return "COLLECTION";
                default: return "ARCANE BRIGADE";
            }
        }
        private static string PageEyebrow(Page page) => page == Page.Roster ? "ARCANE BRIGADE / NEW RUN" : "ARCANE BRIGADE";
        public static Rect RosterCardBounds(int index) => new Rect(index % 3 * 210, index / 3 * 245, 200, 235);
        public static Sprite RosterPortrait(UnitDefinition unit, float time, int identity, bool focused, bool reducedMotion)
        {
            if (unit.AnimationFrames == null || unit.AnimationFrames.Length < 4) return unit.Body;
            bool stepping = focused && !reducedMotion && Mathf.Repeat(time + identity * 0.73f, 7f) > 6.2f;
            return unit.AnimationFrames[UnitVisual.AnimationFrame(time, identity, stepping, reducedMotion)] ?? unit.Body;
        }

        private void DrawSettings()
        {
            DrawVolume(175, "Master volume", profile.Volume, value => profile.Volume = value);
            DrawVolume(230, "Music", profile.MusicVolume, value => profile.MusicVolume = value);
            DrawVolume(285, "Sound effects", profile.SfxVolume, value => profile.SfxVolume = value);
            if (Button(new Rect(75, 350, 540, 55), "BUNDLE: " + RunMeta.CosmeticName(profile.Cosmetic)))
            {
                int before = profile.Cosmetic;
                int candidate = (before + 1) % 3;
                while (!RunMeta.CosmeticUnlocked(profile, candidate)) candidate = (candidate + 1) % 3;
                profile.Cosmetic = candidate;
                if (!SaveProfile()) profile.Cosmetic = before;
                PresentationSettings.Apply(profile);
            }
            ToggleOption(new Rect(645, 350, 560, 55), "REDUCED MOTION", profile.ReducedMotion, value => profile.ReducedMotion = value);
            ToggleOption(new Rect(75, 425, 540, 55), "REDUCED AUDIO CLUTTER", profile.ReducedAudio, value => profile.ReducedAudio = value);
            ToggleOption(new Rect(645, 425, 560, 55), "HIGH-CONTRAST UI", profile.HighReadability, value => profile.HighReadability = value);
            ToggleOption(new Rect(75, 500, 540, 55), "DAMAGE NUMBERS", profile.ShowDamageNumbers, value => profile.ShowDamageNumbers = value);
            ToggleOption(new Rect(645, 500, 560, 55), "LOOT INDICATORS", profile.ShowLootIndicators, value => profile.ShowLootIndicators = value);
            ToggleOption(new Rect(75, 565, 365, 44), "REDUCED SHAKE", profile.ReducedShake, value => profile.ReducedShake = value);
            ToggleOption(new Rect(460, 565, 365, 44), "REDUCED FLASHES", profile.ReducedFlashes, value => profile.ReducedFlashes = value);
            ToggleOption(new Rect(845, 565, 360, 44), "REDUCED EFFECTS", profile.ReducedEffects, value => profile.ReducedEffects = value);
            Label(new Rect(75, 620, 1130, 65), RunUIHints.Menu + "\nCosmetics: Office wave 5; Stage 3 bosses. Fonts: Be Vietnam Pro, Noto Sans Mono (OFL 1.1).");
        }

        private void DrawVolume(float top, string caption, float value, Action<float> setter)
        {
            Label(new Rect(75, top, 310, 40), caption + ": " + Mathf.RoundToInt(value * 100) + "%");
            float changed = GUI.HorizontalSlider(new Rect(390, top + 16, 630, 25), value, 0, 1);
            if (Button(new Rect(1050, top, 65, 42), "−")) changed = Mathf.Max(0, value - 0.1f);
            if (Button(new Rect(1135, top, 65, 42), "+")) changed = Mathf.Min(1, value + 0.1f);
            if (Mathf.Abs(changed - value) < 0.005f) return;
            setter(changed);
            CheckpointAudio.PlayInterfaceId("ui.setting");
            if (!SaveProfile()) setter(value);
            PresentationSettings.Apply(profile);
        }

        private void ToggleOption(Rect rectangle, string caption, bool value, Action<bool> setter)
        {
            if (!Button(rectangle, caption + (value ? ": ON" : ": OFF"))) return;
            setter(!value);
            CheckpointAudio.PlayInterfaceId("ui.setting");
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
            Label(new Rect(60, 165, 575, 80), "Wave " + profile.BestWave + " / Wins " + profile.Wins + " / Kills " + profile.TotalKills + "\nUnlocked difficulty: " + (RunMeta.UnlockedDifficulty(profile) + 1) + " / 5");
            scroll = GUI.BeginScrollView(new Rect(60, 250, 560, 350), scroll, new Rect(0, 0, 520, Mathf.Max(345, profile.History.Count * 95 + profile.Records.Count * 65 + profile.Achievements.Count * 40 + 65)));
            float vertical = 0;
            for (int index = 0; index < profile.History.Count; index++)
            {
                RunRecord record = profile.History[index];
                if (Button(new Rect(0, vertical, 515, 85), record.Nickname + "\n" + record.Score.ToString("N0") + " points · " + RunUICopy.UnitName(record.Build.ChampionId) + " · " + (record.Build.Endless ? "Endless" : "Journey"))) historySelection = index;
                vertical += 95;
            }
            Label(new Rect(0, vertical, 510, 50), "RECORDS BY CHAMPION / MODE / DIFFICULTY"); vertical += 60;
            foreach (ScoreRecord record in profile.Records) { Label(new Rect(0, vertical, 510, 60), RunUICopy.RecordName(record.Key, catalog) + "\n" + record.Score.ToString("N0") + " points · wave " + record.Hall); vertical += 65; }
            foreach (string achievement in profile.Achievements) { Label(new Rect(0, vertical, 510, 40), "✓ " + RunUICopy.AchievementName(achievement)); vertical += 40; }
            GUI.EndScrollView();
            if (profile.History.Count > 0)
            {
                RunRecord record = profile.History[Mathf.Clamp(historySelection, 0, profile.History.Count - 1)];
                invoice.Draw(new Rect(660, 160, 545, 465), record.Build, catalog, record.Nickname, record.Score);
                if (Button(new Rect(660, 640, 545, 42), "SAVE RECEIPT IMAGE · PNG"))
                    try { error = "Image: " + RunInvoice.ExportFrame(record.Id); } catch (Exception exception) { error = exception.Message; }
            }
            else Label(new Rect(690, 240, 470, 200), "No new receipt yet. Finish a run to save the brigade, weapons and measured contributions.\nOlder saves retain records without reconstructing unmeasured history.");
        }

        private bool SaveProfile()
        {
            try { storage.SaveProfile(profile); error = ""; return true; }
            catch (Exception exception) { error = "Failed to save settings: " + exception.Message; CheckpointAudio.PlayInterfaceId("ui.error"); return false; }
        }
    }
}
