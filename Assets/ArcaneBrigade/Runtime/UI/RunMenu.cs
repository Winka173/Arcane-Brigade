using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class RunLaunch
    {
        public static string ChampionId; public static bool Resume; public static int Difficulty; public static int ChallengeIndex;
        /// <summary>The Waiting Room (play-test item 27): the Nursery friend and Wardrobe choices picked in hero select, and
        /// whether the menu opens in the hub (back from a finished run).</summary>
        public static string StartFriend; public static bool AltStarter, SecondHabit, OpenHub;
        /// <summary>The Postbox's keepsake relic picked in hero select; the Daily Dream's day (null: an ordinary run).</summary>
        public static string Keepsake, Daily;
    }
    public sealed class RunMenu : MonoBehaviour
    {
        public enum Page { Landing, Roster, Resume, Settings, Help, Records, ConfirmOverwrite, WeaponSandbox, Museum, Achievements, Hub }
        [SerializeField] private RunCatalog catalog;
        private RunStorage storage;
        private PlayerProfile profile;
        private RunSnapshot saved;
        private GUIStyle heading;
        private int guidePage;
        private GUIStyle dangerStyle, explainTitle, explainBody;
        private string explainHead, explainText; private Rect explainAt; private float explainShown = -10;
        private bool keepsakeOpen; private Rect keepsakeAt; private float keepsakeOpenedAt = -10;
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
        private readonly AchievementJournal achievementBook = new AchievementJournal();
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
        // the Waiting Room between runs (play-test item 27) and the run-start choices it unlocks in hero select
        private readonly MetaHub hub = new MetaHub();
        public MetaHub Hub => hub;
        private string nurseryPick, keepsakePick, dailyDay; private bool altCoat, habit; private int loadoutHero = -1;
        public void SelectChampionForDiagnostics(int index)
        {
            if (!RunPaths.K7Capture || index < 0 || index >= catalog.Champions.Length) throw new InvalidOperationException("Hero fixture requires isolated capture mode");
            selection = index; rosterDescriptionScroll = Vector2.zero; ShowPage(Page.Roster); scroll.y = Mathf.Max(0, RosterCardBounds(index).y - 50);
        }
        public void NavigateForDiagnostics(int direction, bool submit)
        {
            if (!RunPaths.K7Capture) throw new InvalidOperationException("Navigation fixture requires isolated mode");
            navigation.Move(direction); if (submit) navigation.QueueSubmit();
        }
        public void ConfigureK7Diagnostics(RunCatalog definition, PlayerProfile readOnlyProfile)
        {
            if (!RunPaths.K7Capture) throw new InvalidOperationException("K7 fixture requires isolated mode");
            catalog = definition; profile = readOnlyProfile; PresentationSettings.Apply(profile); PresentationSettings.ApplyDisplay(profile);
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
        /// <summary>Play-test capture: the hub and hero select drawn from a prepared profile (never saved).</summary>
        public void ConfigureHubDiagnostics(PlayerProfile readOnlyProfile)
        {
            if (!PlaytestFixCapture.Requested && !MenuReviewCapture.Requested) throw new InvalidOperationException("Hub fixture requires the play-test capture");
            profile = readOnlyProfile; loadoutHero = -1;
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
            if (page == Page.Hub && CurrentPage != Page.Hub) hub.Enter();
            if (page == Page.Roster) loadoutHero = -1;
            if (page != Page.ConfirmOverwrite) dailyDay = null; // the Daily Dream goes straight from the Window (or through the overwrite check)
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
            difficulty = Mathf.Clamp(profile.LastDifficulty, 0, RunMeta.UnlockedDifficulty(profile)); // the difficulty chosen last time
            ArcadeSkin.Bundle = profile.Cosmetic;
            error = storage.LastError;
            ReloadSaveSummary();
            PresentationSettings.Apply(profile);
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            music = gameObject.AddComponent<ArcadeMusic>();
            gameObject.AddComponent<CheckpointAudio>().ConfigurePresentation();
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            if (RunLaunch.OpenHub) RunLaunch.OpenHub = false; // back from a finished run: the main menu (requirement 22/10/2026)
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
            if (music != null) music.ForceScene(CurrentPage == Page.Roster ? ArcaneCueLibrary.SceneSelect : CurrentPage == Page.Hub ? ArcaneCueLibrary.SceneHub : null);
            RunUIHints.Refresh();
            if (CurrentPage == Page.Hub)
            {
                HubAction walked = hub.Tick(Time.unscaledDeltaTime);
                if (walked == HubAction.NextNight) ShowPage(Page.Landing); // the hub's door leads back to the main menu
                else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) { if (hub.PanelOpen) hub.Close(); else ShowPage(Page.Landing); }
                return;
            }
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
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) ShowPage(BackOf(CurrentPage));
        }

        private void Launch(bool resume)
        {
            if (resume && !CanResume || !resume && (!HeroUnlocks.Open(profile, catalog.Champions[selection]) || difficulty > RunMeta.UnlockedDifficulty(profile))) return;
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
            if (!resume && dailyDay == null && profile.LastDifficulty != difficulty) { profile.LastDifficulty = difficulty; SaveProfile(); } // kept for the next new run
            RunLaunch.ChallengeIndex = challenge;
            RunLaunch.StartFriend = nurseryPick; RunLaunch.AltStarter = altCoat; RunLaunch.SecondHabit = habit;
            RunLaunch.Keepsake = keepsakePick; RunLaunch.Daily = dailyDay;
            if (dailyDay != null) { RunLaunch.Difficulty = 0; RunLaunch.ChallengeIndex = 0; }
            SceneDissolve.Load("RunArena"); // hero select melts into the first night
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
            // the menu's room fills the whole screen at any shape (no black bars on wide phones); the page keeps its 16:9 frame
            if (Event.current.type == EventType.Repaint)
            {
                Rect whole = new Rect(0, 0, Screen.width, Screen.height);
                Texture2D wide = CurrentPage == Page.Hub ? null : Backdrop();
                GUI.DrawTexture(whole, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, ArcadeSkin.Ink, 0, 0);
                if (wide != null) GUI.DrawTexture(whole, wide, ScaleMode.ScaleAndCrop);
                if (wide != null && CurrentPage != Page.Landing) GUI.DrawTexture(whole, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 0, new Color(0.02f, 0.03f, 0.055f, PresentationSettings.HighReadability ? 0.97f : 0.84f), 0, 0);
            }
            Matrix4x4 previous = ArcadeTypography.BeginViewport(new Rect(0, 0, Screen.width, Screen.height), 1280, 720);
            Color previousTint = GUI.color;
            LivingUI.Enter(Time.unscaledTime - pageChangedAt, new Vector2(640, 360));
            GlossaryUI.BlockUnderlying(new Vector2(1280, 720));
            if (CurrentPage != Page.Landing && CurrentPage != Page.Hub)
            {
                // (the room and its shade are drawn across the whole screen above)
                ArcadeSkin.Header(new Rect(60, 32, 940, 92), PageEyebrow(CurrentPage), PageTitle(CurrentPage));
            }
            if (CurrentPage != Page.Landing && CurrentPage != Page.Hub && Button(new Rect(1010, 40, 200, 50), "BACK"))
            {
                if (CurrentPage == Page.WeaponSandbox) CloseWeaponSandbox();
                else ShowPage(BackOf(CurrentPage));
            }
            switch (CurrentPage)
            {
                case Page.Landing: DrawLanding(); break;
                case Page.Roster: DrawRoster(); DrawKeepsakePicker(); DrawExplain(); break;
                case Page.Hub:
                    HubAction chosen = hub.Draw(catalog, profile, selection, SaveProfile);
                    if (chosen == HubAction.NextNight) ShowPage(Page.Landing); else if (chosen == HubAction.Back) ShowPage(Page.Landing); else if (chosen == HubAction.Daily) StartDaily();
                    break;
                case Page.Resume:
                    Label(new Rect(110, 200, 1060, 230), CanResume ? RunUICopy.UnitName(catalog.Unit(saved.ChampionId)) + " · Wave " + saved.Wave + "\n" + RunUICopy.PhaseName(saved.Phase) + " · Gold " + saved.Gold + " · Eggs " + saved.Eggs + "\n" + CompanionDeployment.Select(saved.Roster).Count + " friends · " + saved.Items.Count + " relic types\n" + (saved.Phase == SessionPhase.Victory || saved.Phase == SessionPhase.Defeat ? "Reopens the settled receipt without claiming rewards twice. Victorious runs can still enter endless mode." : "Unfinished combat restarts at wave start; shops resume after the last transaction.") : "No valid saved boundary.");
                    GUI.enabled = CanResume;
                    if (navigation.Button(new Rect(420, 490, 440, 70), "RESUME", primary)) Launch(true);
                    GUI.enabled = true;
                    break;
                case Page.ConfirmOverwrite:
                    Label(new Rect(120, 210, 1040, 180), "You have a journey in progress.\nStarting a new one puts it safely aside first; your records and your Waiting Room stay as they are.");
                    if (Button(new Rect(180, 490, 400, 70), "KEEP OLD RUN")) ShowPage(BackOf(Page.ConfirmOverwrite));
                    if (navigation.Button(new Rect(700, 490, 400, 70), "BACK UP AND START", primary)) Launch(false);
                    break;
                case Page.Settings: DrawSettings(); break;
                case Page.Help: HowToPlayBook.Draw(new Rect(90, 160, 1100, 520), catalog, catalog.Champions != null && catalog.Champions.Length > 0 ? catalog.Champions[Mathf.Clamp(selection, 0, catalog.Champions.Length - 1)].Id : null, ref guidePage); break; // the picture book (requirement 24/10/2026)
                case Page.Records: DrawRecords(); break;
                case Page.Achievements: achievementBook.Draw(catalog, profile); break;
                case Page.Museum:
                    museum.Draw(catalog, profile);
                    if (museum.Tab == 0)
                    {
                        GUI.enabled = museum.SelectedWeapon != null;
                        if (Button(new Rect(850, 625, 355, 45), "TRY DISCOVERED GEAR")) OpenWeaponSandbox();
                        GUI.enabled = true;
                    }
                    break;
                case Page.WeaponSandbox: sandbox.DrawPanel(text, button); break;
            }
            if (CurrentPage != Page.WeaponSandbox && !string.IsNullOrEmpty(error)) Label(new Rect(60, 675, 1150, 28), error);
            if (!profile.ReducedMotion)
            {
                float alpha = Mathf.Clamp01(1 - (Time.unscaledTime - pageChangedAt) / 0.16f);
                if (alpha > 0) ArcadeSkin.Line(new Rect(25, 145, 1230, 540), new Color(0.03f, 0.04f, 0.07f, alpha * 0.3f));
            }
            navigation.End();
            GlossaryUI.Draw(new Vector2(1280, 720));
            DrawBootLoading();
            GUI.matrix = previous;
            GUI.color = previousTint;
            GUI.skin = previousSkin;
        }
        // requirement 24/10/2026: the game opens on a loading screen - the night, the lamp lighting a row of lanterns one by one under
        // the name - then fades into the menu (once per launch; Reduced Motion shortens it to a moment)
        private static float bootStarted = -1; private static bool booted;
        private void DrawBootLoading()
        {
            if (booted) return;
            if (bootStarted < 0) bootStarted = Time.unscaledTime;
            float age = Time.unscaledTime - bootStarted, length = profile.ReducedMotion ? .4f : 1.8f, fade = Mathf.Clamp01((age - length) / .45f);
            if (fade >= 1) { booted = true; return; }
            Color keep = GUI.color; GUI.color = new Color(1, 1, 1, 1 - fade);
            RunHud.Fill(new Rect(0, 0, 1280, 720), new Color(.035f, .04f, .07f, 1));
            float k = Mathf.Clamp01(age / length);
            ArcadeSkin.Glow(new Rect(440, 120, 400, 400), new Color(1f, .78f, .42f, .25f + .25f * k));
            UnitDefinition emblem = catalog.Unit("LanternKnight");
            if (emblem != null) RunHud.Sprite(new Rect(555, 170, 170, 200), emblem.Body);
            GUIStyle logo = logoStyle ?? (logoStyle = ArcadeTypography.Style(64, true));
            if (ArcadeTypography.Display != null) logo.font = ArcadeTypography.Display;
            TextAnchor a = logo.alignment; logo.alignment = TextAnchor.MiddleCenter; logo.normal.textColor = ArcadeSkin.Cream;
            GUI.Label(new Rect(240, 390, 800, 90), "Arcane Brigade", logo); logo.alignment = a;
            for (int i = 0; i < 9; i++)
            {
                Vector2 p = new Vector2(640 + (i - 4) * 44, 520); bool lit = k * 9 > i;
                ArcadeSkin.Diamond(p, 26, new Color(.1f, .08f, .06f, .9f)); ArcadeSkin.Diamond(p, lit ? 20 : 12, lit ? new Color(1f, .8f, .4f) : new Color(.3f, .27f, .24f));
            }
            GUI.color = keep;
        }

        // Main menu = the Waiting Room (Resources/MienCho/WaitingRoom.png, painted 16:9, 1280x720 design space):
        // the door begins a journey, the armchair is where you left off, the bookshelf holds everyone you have met,
        // the letters on the side table are the ones you kept, the lamp sets light and sound. How to play lives only
        // in the bottom row; the row mirrors every entry one-to-one in the same order as before.
        // requirement 22/10/2026: NEW JOURNEY goes straight to hero select; the Waiting Room (permanent upgrades) is its own entry
        private static readonly string[] SpotLabels = { "NEW JOURNEY", "CONTINUE", "WAITING ROOM", "COLLECTION", "RECORDS", "SETTINGS", "HOW TO PLAY" };
        private static readonly string[] SpotHints = { "Step through the door", "Sit back down where you left off", "Spend lamp oil on the room's stations", "Everyone and everything you have met", "The letters you kept", "Light, sound and comfort", "How the night works" };
        private static readonly string[] SpotCues = { "menu.spot.door", "menu.spot.hero", "menu.spot.lamp", "menu.spot.jar", "menu.spot.printer", "menu.spot.lamp", "menu.spot.bell" };
        public static readonly Rect[] SpotAreas =
        {
            new Rect(800, 0, 312, 452),    // the carved door
            new Rect(40, 172, 360, 368),   // the armchair
            Rect.zero,                     // the Waiting Room (the bottom row only)
            new Rect(1160, 318, 120, 156), // the bookshelf
            new Rect(400, 280, 160, 86),   // the letters on the side table
            new Rect(340, 118, 190, 152),  // the lamp
            Rect.zero,
        };
        // where each entry's paper label sits in the room (centre x, top y)
        private static readonly Vector2[] SpotTags = { new Vector2(956, 372), new Vector2(222, 300), new Vector2(0, 0), new Vector2(1180, 280), new Vector2(480, 372), new Vector2(436, 80), new Vector2(0, 0) };
        private int hoveredSpot = -1;
        private GUIStyle smallButton;
        private GUIStyle spotHint;
        private GUIStyle eyebrowStyle;
        private Texture2D waitingRoom;

        private bool SpotEnabled(int index) => index != 1 || CanResume;
        private bool HubAffordable()
        {
            if (profile == null) return false;
            if (MetaProgression.AnyLightable(profile)) return true;
            foreach (MetaStation station in MetaProgression.Working)
                foreach (MetaTrack track in MetaProgression.For(station))
                    if (MetaProgression.Blocked(profile, track) == null) return true;
            return false;
        }
        private void ActivateSpot(int index)
        {
            if (!SpotEnabled(index)) { CheckpointAudio.PlayInterfaceId("ui.locked"); return; }
            switch (index)
            {
                case 0: ShowPage(Page.Roster); break; // the door: straight to hero select
                case 1: ShowPage(Page.Resume); break;
                case 2: ShowPage(Page.Hub); break;    // the Waiting Room: permanent upgrades, Daily Dream
                case 3: ShowPage(Page.Museum); break;
                case 4: ShowPage(Page.Records); break;
                case 5: ShowPage(Page.Settings); break;
                default: ShowPage(Page.Help); break;
            }
        }

        // After Act I's dawn the Waiting Room's armchair has turned toward the traveller (Docs/Mien_Cho_Act_One_Story.md, after the credits).
        private bool ActOneDone => profile != null && (profile.NightmareLampPromise || profile.NightmareShopClosed);
        private Texture2D turnedRoom;
        private float chairSpokeAt = -1;
        private GUIStyle chairVoice;
        /// <summary>Once, the first time the menu opens after dawn: the armchair speaks the line it has been saving all act.</summary>
        private void DrawChairVoice()
        {
            if (!ActOneDone || profile.ActOneChairSpoke && chairSpokeAt < 0) return;
            if (chairSpokeAt < 0) chairSpokeAt = Time.unscaledTime;
            float age = Time.unscaledTime - chairSpokeAt;
            if (age > 8f)
            {
                if (!profile.ActOneChairSpoke && !diagnosticState) { profile.ActOneChairSpoke = true; SaveProfile(); }
                return;
            }
            if (chairVoice == null) { chairVoice = ArcadeTypography.Style(30); chairVoice.font = ArcadeTypography.Hand; chairVoice.alignment = TextAnchor.MiddleCenter; chairVoice.wordWrap = true; }
            float alpha = Mathf.Clamp01((age - 1.2f) / 1.4f) * Mathf.Clamp01((8f - age) / 1.5f);
            ArcadeSkin.Glow(new Rect(40, 150, 360, 380), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.22f * alpha));
            chairVoice.normal.textColor = new Color(0, 0, 0, 0.75f * alpha);
            GUI.Label(new Rect(62, 302, 340, 80), "Would you like me to sit closer?", chairVoice);
            chairVoice.normal.textColor = new Color(ArcadeSkin.Paper.r, ArcadeSkin.Paper.g, ArcadeSkin.Paper.b, alpha);
            GUI.Label(new Rect(60, 300, 340, 80), "Would you like me to sit closer?", chairVoice);
        }

        private Texture2D Backdrop()
        {
            // the painted menu background (UI kit, item 13); after Act One the room with the turned chair keeps the story
            if (!ActOneDone && ArcadeSkin.KitSprite("Menu/Backdrop") is Sprite menu) return menu.texture;
            if (ActOneDone)
            {
                if (turnedRoom == null) turnedRoom = Resources.Load<Texture2D>("MienCho/WaitingRoom_ChairTurned");
                if (turnedRoom != null) return turnedRoom;
            }
            if (waitingRoom == null) waitingRoom = Resources.Load<Texture2D>("MienCho/WaitingRoom");
            if (waitingRoom == null && shopIllustration == null) shopIllustration = Resources.Load<Texture2D>("LostShop_v1");
            return waitingRoom != null ? waitingRoom : shopIllustration;
        }

        private void DrawLanding()
        {
            if (landingShade == null)
            {
                landingShade = new Texture2D(64, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[64];
                for (int index = 0; index < pixels.Length; index++) pixels[index] = new Color(0.02f, 0.03f, 0.06f, Mathf.Lerp(0.75f, 0, index / 63f));
                landingShade.SetPixels(pixels); landingShade.Apply(false, true);
            }
            // the shade runs on past the frame on wider or taller screens, so no edge shows
            ArcadeSkin.Line(new Rect(-1200, -600, 1200, 1920), new Color(0.02f, 0.03f, 0.06f, 0.75f));
            GUI.DrawTexture(new Rect(0, -600, 600, 1920), landingShade);
            DrawChairVoice();
            bool reduced = profile.ReducedMotion;
            float breathe = reduced ? 0.5f : 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 1.1f);

            if (eyebrowStyle == null) { eyebrowStyle = ArcadeTypography.Style(14, false, true); eyebrowStyle.normal.textColor = ArcadeSkin.Accent; }
            GUI.Label(new Rect(44, 34, 500, 22), "THE WAITING PLACE", eyebrowStyle);
            GUIStyle logo = logoStyle ?? (logoStyle = ArcadeTypography.Style(64, true));
            if (ArcadeTypography.Display != null) logo.font = ArcadeTypography.Display;
            logo.wordWrap = false;
            if (logoShadow == null) { logoShadow = new GUIStyle(logo); }
            logoShadow.normal.textColor = new Color(0, 0, 0, 0.6f);
            ArcadeSkin.Glow(new Rect(-40, -10, 620, 200), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.10f + 0.04f * breathe));
            GUI.Label(new Rect(44, 58, 620, 90), "Arcane Brigade", logoShadow);
            logo.normal.textColor = ArcadeSkin.Cream;
            GUI.Label(new Rect(40, 54, 620, 90), "Arcane Brigade", logo);
            if (spotHint == null) { spotHint = ArcadeTypography.Style(24, false); spotHint.font = ArcadeTypography.Hand; spotHint.alignment = TextAnchor.MiddleCenter; spotHint.normal.textColor = ArcadeSkin.Cream; spotHint.wordWrap = false; }
            spotHint.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(46, 140, 760, 32), "someone is waiting for you…", spotHint);
            spotHint.alignment = TextAnchor.MiddleCenter;

            // requirement 24/10/2026: "nobody can tell what reacts to the mouse, let alone on a phone - redo the main menu, with the
            // Lantern Knight as its emblem". Every choice is a plain painted button with its icon, all visible at once (the
            // order mirrors SpotLabels for keys and pads); the Lantern Knight stands on the right holding up its lamp.
            UnitDefinition emblem = catalog.Unit("LanternKnight") ?? catalog.Champions[0];
            Sprite pose = emblem.AnimationFrames != null && emblem.AnimationFrames.Length >= 4 ? emblem.AnimationFrames[UnitVisual.AnimationFrame(Time.unscaledTime, 3, false, reduced)] : emblem.Body;
            Rect knight = new Rect(800, 150 + (reduced ? 0 : Mathf.Sin(Time.unscaledTime * 1.3f) * 5), 330, 380);
            ArcadeSkin.Glow(new Rect(knight.x - 160, knight.y - 120, knight.width + 320, knight.height + 220), new Color(1f, .78f, .42f, .22f + .1f * breathe));
            ArcadeSkin.Glow(new Rect(knight.x + 40, knight.yMax - 34, knight.width - 80, 56), new Color(0, 0, 0, .6f));
            RunHud.Sprite(knight, pose);
            if (!reduced) for (int i = 0; i < 6; i++) { float r = Mathf.Repeat(Time.unscaledTime * .25f + i / 6f, 1); ArcadeSkin.Diamond(new Vector2(knight.center.x + Mathf.Sin(r * 7 + i * 2) * 120, knight.yMax - r * 420), 6 + 6 * Mathf.Sin(r * Mathf.PI), new Color(1f, .82f, .45f, .8f * Mathf.Sin(r * Mathf.PI))); }
            if (spotPlate == null) { spotPlate = ArcadeTypography.Style(15, true); }
            Sprite[] icons = { null, null, MetaIcons.Station(MetaStation.Lantern, true), MetaIcons.Station(MetaStation.Museum, true), MetaIcons.Station(MetaStation.Ledger, true), MetaIcons.Station(MetaStation.Clock, true), MetaIcons.Station(MetaStation.Postbox, true) };
            if (smallButton == null) smallButton = new GUIStyle(button) { fontSize = 15 }; // one size that fits the longest label, so the grid reads even
            for (int index = 0; index < SpotLabels.Length; index++)
            {
                GUI.enabled = SpotEnabled(index);
                string caption = index == 1 ? (CanResume ? "CONTINUE · WAVE " + saved.Wave : "NOTHING TO CONTINUE") : SpotLabels[index];
                Rect spot = index < 2 ? new Rect(44, 200 + index * 82, 470, 68) : new Rect(44 + (index - 2) % 2 * 240, 380 + (index - 2) / 2 * 72, 230, 60);
                if (index >= 2) caption = "      " + caption; // room for its icon
                if (navigation.Button(spot, caption, index == 0 || index == 1 && CanResume ? primary : smallButton)) ActivateSpot(index);
                if (index >= 2 && icons[index] != null) RunHud.Sprite(navigation.Moved(new Rect(spot.x + 16, spot.y + 9, 42, 42)), icons[index]); // rides with the plate
                // a small lit badge when the lamp oil can buy something in the Waiting Room
                if (index == 2 && HubAffordable()) { ArcadeSkin.Glow(navigation.Moved(new Rect(spot.xMax - 36, spot.y, 28, 28)), new Color(1f, .82f, .45f, .55f)); ArcadeSkin.Seal(navigation.Moved(new Vector2(spot.xMax - 22, spot.y + 14)), 22, "!"); } // inside the plate's corner
            }
            GUI.enabled = true;
        }
        private GUIStyle spotPlate;

        private void DrawRoster()
        {
            if (catalog.Champions == null || catalog.Champions.Length == 0) { Label(new Rect(75, 200, 1100, 90), "No hero available."); return; }
            selection = Mathf.Clamp(selection, 0, catalog.Champions.Length - 1);
            RosterOverlays(false); // an open picker or details card takes the taps before the page under it
            GUIStyle caption = portraitCaption ?? (portraitCaption = ArcadeTypography.Style(18, true));
            caption.alignment = TextAnchor.MiddleCenter;
            GUIStyle note = rosterNote ?? (rosterNote = ArcadeTypography.Style(16));
            Label(new Rect(65, 142, 660, 32), "CHOOSE YOUR HERO");
            scroll = ScrollPanel.Begin(new Rect(65, 180, 660, 460), scroll, new Rect(0, 0, 620, Mathf.Max(465, Mathf.CeilToInt(catalog.Champions.Length / 3f) * 245)));
            for (int index = 0; index < catalog.Champions.Length; index++)
            {
                UnitDefinition unit = catalog.Champions[index];
                bool unlocked = HeroUnlocks.Open(profile, unit);
                Rect card = RosterCardBounds(index);
                if (ArcadeSkin.HasKit) ArcadeSkin.Frame(card, KitFrame.Dialogue, .42f); else RunHud.Fill(card, index == selection ? ArcadeSkin.CardLit : ArcadeSkin.Card);
                if (index == selection) { ArcadeSkin.Glow(new Rect(card.x + 10, card.y, card.width - 20, 150), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.26f)); ArcadeSkin.Slant(new Rect(card.x + 14, card.yMax - 16, card.width - 28, 8), ArcadeSkin.Teal); }
                if (navigation.Button(card, "", GUIStyle.none)) { if (selection != index) rosterDescriptionScroll = Vector2.zero; selection = index; }
                Color previousColor = GUI.color;
                if (!unlocked) GUI.color = new Color(0.65f, 0.65f, 0.65f);
                float portraitSize = index == selection ? 120 : 110;
                // portrait + name centred in the card (requirement 21/10/2026); a locked hero lifts them to leave room for its chip
                float top = card.y + (unlocked ? 30 : 12);
                RunHud.Sprite(new Rect(card.center.x - portraitSize * 0.5f, top, portraitSize, 115), RosterPortrait(unit, Time.unscaledTime, index, true, profile.ReducedMotion)); // every hero in the roster acts (requirement 24/10/2026)
                GUI.color = previousColor;
                TextAnchor keepAlign = caption.alignment; caption.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(card.x + 10, top + 120, card.width - 20, 52), RunUICopy.UnitName(unit), caption);
                caption.alignment = keepAlign;
                if (!unlocked) ArcadeSkin.Tag(new Rect(card.x + 12, card.y + 186, card.width - 24, 26), HeroUnlocks.Coming(unit) ? "COMING SOON" : HeroUnlocks.Short(unit), CodeTone.Cream, chip);
            }
            ScrollPanel.End();
            UnitDefinition champion = catalog.Champions[selection];
            if (catalog.HeroRevision > 0)
            {
                int iconIndex = 0;
                foreach (BondDefinition bond in catalog.Bonds)
                    if (Array.IndexOf(champion.RecruitThemes, bond.Id) >= 0)
                    {
                        // each theme icon is tappable and says what it means (requirement 24/10/2026)
                        Rect icon = new Rect(690 - iconIndex++ * 34, 145, 28, 28); RunHud.Sprite(icon, bond.Icon);
                        if (GUI.Button(new Rect(icon.x - 4, icon.y - 4, 36, 36), GUIContent.none, GUIStyle.none)) Explain(icon, bond.Name + " theme", "Friends of this theme join this hero's shop more often. " + bond.Threshold + " of them together wake a bond bonus.");
                    }
                if (iconIndex > 0) GUI.Label(new Rect(690 - iconIndex * 34 - 130, 145, 125, 28), "RECRUIT THEMES", note);
            }
            // requirement 25/10/2026 (the text box was too small): the hero's panel is the whole right column; the difficulty sits
            // under the hero grid and STEP THROUGH under the panel, one bottom row
            Rect heroPanel = new Rect(745, 135, 490, 505);
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(heroPanel, KitFrame.Dialogue, .45f); else RunHud.Fill(heroPanel, ArcadeSkin.Card);
            ArcadeSkin.Glow(new Rect(752, 125, 170, 175), new Color(1f, 0.8f, 0.45f, 0.28f));
            RunHud.Sprite(new Rect(775, 145, 132, 142), RosterPortrait(champion, Time.unscaledTime, selection, true, profile.ReducedMotion));
            if (rosterTitle == null) { rosterTitle = ArcadeTypography.Style(28, true); if (ArcadeTypography.Display != null) rosterTitle.font = ArcadeTypography.Display; }
            GUI.Label(new Rect(918, 150, 300, 70), RunUICopy.UnitName(champion), rosterTitle);
            ArcadeSkin.Tag(new Rect(918, 222, 280, 28), RunUICopy.WeaponName(champion.Weapon.Id).ToUpperInvariant(), CodeTone.Walnut, chip);
            GUI.Label(new Rect(918, 256, 250, 34), HeroUnlocks.Full(profile, champion, catalog), note);
            // requirement 25/10/2026: the panel says who the hero is in two lines; everything else is one tap away (the i beside it)
            string fullDescription = RunUICopy.ChampionDescription(champion);
            string description = ShortHero(champion);
            if (catalog.HeroRevision > 0)
            {
                fullDescription += "\n\nBONDS IN THIS HERO'S SHOP: ";
                foreach (BondDefinition bond in K11HeroRules.PoolBonds(catalog, champion)) fullDescription += bond.Name + " · ";
            }
            // requirement 25/10/2026: the hero's basic numbers, each with its stat icon, then any bonus it starts with
            WeaponDefinition arm = champion.Weapon;
            description += "\n\nBASICS\n" + Basic(ItemEffect.Health, "Health", Mathf.RoundToInt(champion.Health).ToString())
                + Basic(ItemEffect.Damage, "Damage", Mathf.RoundToInt(FriendlyWeaponBalance.BaseDamage(arm)).ToString())
                + Basic(ItemEffect.AttackSpeed, "Attacks a second", (1f / Mathf.Max(.05f, arm.Cooldown)).ToString("0.0"))
                + Basic(ItemEffect.AttackRange, "Range", arm.Range.ToString("0.0"))
                + Basic(ItemEffect.MoveSpeed, "Move speed", champion.Speed.ToString("0.0"));
            if (champion.ChampionModifiers != null && champion.ChampionModifiers.Length > 0)
            {
                description += "\nSTARTS WITH\n";
                foreach (StatModifier modifier in champion.ChampionModifiers)
                    description += GlossaryUI.Modifier(modifier.Effect, modifier.Scope, modifier.Value) + "\n";
            }
            float descriptionHeight = Mathf.Max(210, GlossaryUI.Height(description, text, 425) + 8);
            bool loadout = DrawLoadout(champion, new Rect(770, 560, 450, 44), note);
            rosterDescriptionScroll = ScrollPanel.Begin(new Rect(770, 298, 450, loadout ? 254 : 322), rosterDescriptionScroll, new Rect(0, 0, 430, descriptionHeight));
            GlossaryUI.Text(new Rect(0, 0, 425, descriptionHeight), description, text);
            ScrollPanel.End();
            heroDetailsText = fullDescription; heroDetailsName = RunUICopy.UnitName(champion);
            if (ArcadeSkin.Medallion(new Rect(1176, 254, 40, 40), KitMedallion.Info)) { heroDetailsAt = Time.unscaledTime; heroDetailsScroll = Vector2.zero; }
            // one journey only (play-test 20/10/2026): the Night Shift / First Light / Small Crew challenges are off the menu;
            // their rules stay in RunMeta, dormant, for a later daily run
            challenge = 0;
            // every difficulty is shown in turn; the two after the whole Lamp Tree show a lock until it is lit
            // requirement 25/10/2026 (no tap-to-cycle pickers): the difficulty is chosen from a list of all of them
            if (Button(new Rect(65, 656, 330, 52), "DIFFICULTY: " + RunMeta.DifficultyNames[difficulty])) difficultyPickerAt = Time.unscaledTime;
            bool sealedDifficulty = difficulty > RunMeta.UnlockedDifficulty(profile);
            // the difficulty's danger colour on its name alone, never on the painted plate (requirement 24/10/2026: no tinted plates or boxes)
            GUIStyle danger = dangerStyle ?? (dangerStyle = ArcadeTypography.Style(15, true)); danger.alignment = TextAnchor.MiddleLeft; danger.wordWrap = true;
            danger.normal.textColor = RunMeta.DifficultyColor(difficulty);
            // beside the button, what the difficulty does (its name is already on the button)
            string dangerText = sealedDifficulty ? "Light the whole Lamp Tree" : DifficultyLine(difficulty);
            Rect dangerRect = new Rect(410, 656, 315, 52);
            if (sealedDifficulty)
            {
                // the padlock stands just before the words, on their middle line, the pair centred together
                ArcadeSkin.Lock(new Vector2(dangerRect.x + 9, dangerRect.center.y), 16, danger.normal.textColor);
                dangerRect = new Rect(dangerRect.x + 26, dangerRect.y, dangerRect.width - 26, dangerRect.height);
            }
            GUI.Label(dangerRect, dangerText, danger);
            GUI.enabled = HeroUnlocks.Open(profile, champion) && !sealedDifficulty;
            if (navigation.Button(new Rect(770, 650, 450, 62), HeroUnlocks.Open(profile, champion) && !sealedDifficulty ? "STEP THROUGH ▶" : HeroUnlocks.Coming(champion) ? "COMING SOON" : "LOCKED", primary)) { if (existingSave) ShowPage(Page.ConfirmOverwrite); else Launch(false); }
            GUI.enabled = true;
            RosterOverlays(true);
        }
        // ---- hero select's two popups: all of a hero's details, and the difficulty list --------------------------------------
        private float heroDetailsAt = -1, difficultyPickerAt = -1;
        private string heroDetailsText = "", heroDetailsName = "";
        private Vector2 heroDetailsScroll;
        private GUIStyle overlayTitle, overlayLine;
        /// <summary>The hero in two lines: its own move's name and first sentence, and what it turns into.</summary>
        private static string Basic(ItemEffect icon, string name, string value) => "{bonus:" + icon + "|" + name + "|" + value + "}\n";
        private static string ShortHero(UnitDefinition champion)
        {
            HeroIdentityDefinition identity = HeroIdentities.For(champion.Id);
            if (identity == null) return RunUICopy.ChampionDescription(champion);
            string first = identity.Description; int stop = first.IndexOf(". ", System.StringComparison.Ordinal); if (stop > 0) first = first.Substring(0, stop + 1);
            HeroKit kit = HeroKits.For(champion.Id);
            return identity.Name + "\n" + first + (kit != null ? "\nTransforms into " + kit.Transformation + "." : "");
        }
        /// <summary>Drawn twice a frame: first (paint false) to take the taps before the page, last (paint true) to draw on top.</summary>
        private void RosterOverlays(bool paint)
        {
            bool details = heroDetailsAt >= 0, picker = difficultyPickerAt >= 0;
            if (!details && !picker) return;
            if (paint != (Event.current.type == EventType.Repaint)) return;
            overlayTitle ??= new GUIStyle(ArcadeTypography.Style(26, true)) { alignment = TextAnchor.MiddleCenter };
            overlayLine ??= new GUIStyle(ArcadeTypography.Style(16)) { wordWrap = true };
            RunHud.Fill(new Rect(0, 0, 1280, 720), new Color(0, 0, 0, .5f));
            Matrix4x4 m = GUI.matrix; Color c = GUI.color;
            if (details)
            {
                Rect card = new Rect(240, 70, 800, 580);
                if (LivingUI.Auto("hero-details", card.center))
                {
                    ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f);
                    GUI.Label(new Rect(card.x, card.y + 22, card.width, 40), heroDetailsName, overlayTitle);
                    float h = Mathf.Max(400, GlossaryUI.Height(heroDetailsText, overlayLine, 700) + 10);
                    heroDetailsScroll = ScrollPanel.Begin(new Rect(card.x + 40, card.y + 76, 720, card.height - 110), heroDetailsScroll, new Rect(0, 0, 700, h));
                    GlossaryUI.Text(new Rect(0, 0, 700, h), heroDetailsText, overlayLine);
                    ScrollPanel.End();
                    if (ArcadeSkin.Medallion(new Rect(card.xMax - 52, card.y - 4, 56, 56), KitMedallion.Close) && !LivingUI.Closing("hero-details")) LivingUI.CloseThen("hero-details", () => heroDetailsAt = -1);
                }
            }
            else
            {
                int count = RunMeta.DifficultyNames.Length, unlocked = RunMeta.UnlockedDifficulty(profile);
                Rect card = new Rect(290, 40, 700, 110 + count * 64);
                if (LivingUI.Auto("difficulty-picker", card.center))
                {
                    ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f);
                    GUI.Label(new Rect(card.x, card.y + 20, card.width, 40), "CHOOSE THE NIGHT'S DIFFICULTY", overlayTitle);
                    for (int i = 0; i < count; i++)
                    {
                        Rect row = new Rect(card.x + 40, card.y + 76 + i * 64, card.width - 80, 56);
                        bool sealedRow = i > unlocked, chosen = i == difficulty;
                        ArcadeSkin.CodePlate(row, CodeTone.Walnut, sealedRow ? 3 : chosen ? 1 : 0);
                        if (chosen) ArcadeSkin.RunningLight(row, Time.unscaledTime, ArcadeSkin.Gold, .7f);
                        overlayTitle.fontSize = 20; overlayTitle.alignment = TextAnchor.MiddleLeft; overlayTitle.normal.textColor = RunMeta.DifficultyColor(i);
                        GUI.Label(new Rect(row.x + (sealedRow ? 44 : 18), row.y, 240, row.height), RunMeta.DifficultyNames[i], overlayTitle);
                        if (sealedRow) ArcadeSkin.Lock(new Vector2(row.x + 24, row.center.y), 14, ArcadeSkin.KitCream);
                        overlayLine.alignment = TextAnchor.MiddleRight; overlayLine.normal.textColor = ArcadeSkin.KitCream;
                        GUI.Label(new Rect(row.x + 250, row.y, row.width - 268, row.height), sealedRow ? "Light the whole Lamp Tree" : DifficultyLine(i), overlayLine);
                        overlayLine.alignment = TextAnchor.UpperLeft;
                        int pick = i;
                        if (!sealedRow && GUI.Button(row, GUIContent.none, GUIStyle.none) && !LivingUI.Closing("difficulty-picker")) LivingUI.CloseThen("difficulty-picker", () => { difficulty = pick; difficultyPickerAt = -1; });
                    }
                    overlayTitle.fontSize = 26; overlayTitle.alignment = TextAnchor.MiddleCenter; overlayTitle.normal.textColor = ArcadeSkin.KitCream;
                    if (ArcadeSkin.Medallion(new Rect(card.xMax - 52, card.y - 4, 56, 56), KitMedallion.Close) && !LivingUI.Closing("difficulty-picker")) LivingUI.CloseThen("difficulty-picker", () => difficultyPickerAt = -1);
                }
            }
            GUI.matrix = m; GUI.color = c;
            if (!paint && Event.current.isMouse) Event.current.Use(); // nothing under the popup takes the tap
        }
        /// <summary>The Window's Daily Dream: today's hero and pact, Peaceful, the day's seed; straight into the night.</summary>
        private void StartDaily()
        {
            string day = MetaProgression.Today;
            UnitDefinition hero = MetaProgression.DailyHero(profile, catalog, day);
            if (hero == null || diagnosticState) return;
            selection = Array.IndexOf(catalog.Champions, hero); difficulty = 0; challenge = 0;
            nurseryPick = null; altCoat = false; habit = false; loadoutHero = -1;
            if (!MetaProgression.Keepsakes(profile, catalog).Contains(keepsakePick ?? "")) keepsakePick = MetaProgression.Keepsakes(profile, catalog).Contains(profile.Meta.Keepsake) ? profile.Meta.Keepsake : null;
            if (existingSave) { ShowPage(Page.ConfirmOverwrite); dailyDay = day; } else { dailyDay = day; Launch(false); }
        }
        // from the overwrite check, back goes where the run was started from: the Waiting Room for tonight's dream (requirement 23/10/2026)
        private Page BackOf(Page page) => page == Page.ConfirmOverwrite ? (dailyDay != null ? Page.Hub : Page.Roster) : page == Page.Roster ? Page.Landing : Page.Landing;

        /// <summary>Hero select's Waiting Room row (only what is unlocked): the Nursery's three friends, the Wardrobe's spare coat
        /// and second habit. A tap picks; the line under the row says what the pick does.</summary>
        /// <summary>A tapped icon explains itself in a small card beside it (requirement 24/10/2026: every icon on hero select must be
        /// tappable and say what it means); a tap anywhere else closes it.</summary>
        private void Explain(Rect icon, string head, string body) { explainAt = icon; explainHead = head; explainText = body; explainShown = Time.unscaledTime; }
        private void DrawExplain()
        {
            if (explainText == null) return;
            if (Event.current.type == EventType.MouseDown && Time.unscaledTime - explainShown > .1f) { explainText = null; return; }
            explainTitle ??= ArcadeTypography.Style(18, true); explainBody ??= ArcadeTypography.Style(16); explainBody.wordWrap = true;
            float w = 330, h = 36 + explainBody.CalcHeight(new GUIContent(explainText), w - 36) + 18;
            Rect card = new Rect(Mathf.Clamp(explainAt.center.x - w * .5f, 20, 1260 - w), explainAt.yMax + 10, w, h); // the menu's 1280x720 space
            if (card.yMax > 704) card.y = explainAt.y - h - 10;
            Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
            LivingUI.Pop(Time.unscaledTime - explainShown, new Vector2(card.center.x, card.y));
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f); else RunHud.Fill(card, ArcadeSkin.Card);
            GUI.Label(new Rect(card.x + 18, card.y + 12, w - 36, 26), explainHead, explainTitle);
            GUI.Label(new Rect(card.x + 18, card.y + 38, w - 36, h - 50), explainText, explainBody);
            GUI.matrix = keepMatrix; GUI.color = keepColour;
        }
        /// <summary>The keepsake picker: every relic kept from a Postbox letter, and "none", in one small card over the slot
        /// (it pops open; a pick closes it and says what the relic does; a tap elsewhere closes it).</summary>
        private void DrawKeepsakePicker()
        {
            if (!keepsakeOpen) return;
            var keeps = MetaProgression.Keepsakes(profile, catalog);
            const float cell = 56, gap = 10, pad = 18;
            int count = keeps.Count + 1, perRow = Mathf.Min(count, 6), rows = Mathf.CeilToInt(count / (float)perRow);
            float w = pad * 2 + perRow * cell + (perRow - 1) * gap, h = 46 + rows * (cell + gap) + pad;
            Rect card = new Rect(Mathf.Clamp(keepsakeAt.center.x - w * .5f, 20, 1260 - w), keepsakeAt.y - h - 10, w, h);
            if (Event.current.type == EventType.MouseDown && !card.Contains(Event.current.mousePosition) && Time.unscaledTime - keepsakeOpenedAt > .1f) { keepsakeOpen = false; return; }
            Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
            LivingUI.Pop(Time.unscaledTime - keepsakeOpenedAt, new Vector2(card.center.x, card.yMax));
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f); else RunHud.Fill(card, ArcadeSkin.Card);
            explainTitle ??= ArcadeTypography.Style(18, true);
            GUI.Label(new Rect(card.x + pad, card.y + 10, w - pad * 2, 26), "Bring a keepsake", explainTitle);
            for (int i = 0; i < count; i++)
            {
                Rect slot = new Rect(card.x + pad + (i % perRow) * (cell + gap), card.y + 42 + (i / perRow) * (cell + gap), cell, cell);
                string id = i == 0 ? null : keeps[i - 1];
                bool on = keepsakePick == id;
                if (on) ArcadeSkin.Glow(new Rect(slot.x - 10, slot.y - 10, slot.width + 20, slot.height + 20), new Color(1f, .8f, .45f, .6f));
                ArcadeSkin.PortraitSlot(slot, on);
                ItemDefinition relic = id == null ? null : catalog.Item(id);
                if (relic?.Icon != null) RunHud.Sprite(new Rect(slot.x + 7, slot.y + 7, cell - 14, cell - 14), relic.Icon);
                else if (id == null) ArcadeSkin.Stroke(new Vector2(slot.x + 18, slot.y + 18), new Vector2(slot.xMax - 18, slot.yMax - 18), 4, ArcadeSkin.KitCream); // none
                if (GUI.Button(slot, GUIContent.none, GUIStyle.none))
                {
                    keepsakePick = id; keepsakeOpen = false; CheckpointAudio.PlayInterfaceId("ui.select");
                    Explain(keepsakeAt, relic != null ? relic.Name : "No keepsake", relic != null ? RelicDescriptions.Describe(relic, 1) + "\nKept from a Postbox letter; it rides along from the first wave." : "This run starts without a keepsake relic.");
                }
            }
            GUI.matrix = keepMatrix; GUI.color = keepColour;
        }
        // what a difficulty does, in numbers (requirement 25/10/2026: the poetic lines said nothing)
        private static string DifficultyLine(int d) => "enemies ×" + JourneyBalance.HealthFactor[Mathf.Clamp(d, 0, JourneyBalance.HealthFactor.Length - 1)].ToString("0.##") + " health, ×" + JourneyBalance.DamageFactor[Mathf.Clamp(d, 0, JourneyBalance.DamageFactor.Length - 1)].ToString("0.##") + " damage";

        private bool DrawLoadout(UnitDefinition champion, Rect row, GUIStyle note)
        {
            MetaProgression.Ensure(profile);
            var friends = MetaProgression.NurseryChoices(profile, catalog, champion);
            int mastery = MetaProgression.MasteryRank(profile, champion.Id);
            UnitDefinition coat = mastery >= 1 ? MetaProgression.AltStarter(catalog, champion) : null;
            bool canHabit = mastery >= 3 && MetaProgression.HabitDropped(HeroKits.For(champion.Id)).HasValue;
            if (loadoutHero != selection)
            {
                loadoutHero = selection;
                HeroMastery remembered = MetaProgression.Mastery(profile, champion.Id);
                altCoat = remembered?.AltStarter == true; habit = remembered?.SecondHabit == true;
                nurseryPick = friends.Count > 0 ? friends[0].Id : null;
                keepsakePick = MetaProgression.Keepsakes(profile, catalog).Contains(profile.Meta.Keepsake) ? profile.Meta.Keepsake : null;
            }
            var keeps = MetaProgression.Keepsakes(profile, catalog);
            if (keepsakePick != null && !keeps.Contains(keepsakePick)) keepsakePick = null;
            if (coat == null) altCoat = false;
            if (!canHabit) habit = false;
            if (nurseryPick != null && !friends.Exists(f => f.Id == nurseryPick)) nurseryPick = null;
            if (friends.Count == 0 && coat == null && !canHabit && keeps.Count == 0) return false;
            float x = row.x; string caption = null;
            if (friends.Count > 0)
            {
                Rect nursery = new Rect(x, row.y + 2, 38, 40); RunHud.Sprite(nursery, MetaIcons.Station(MetaStation.Nursery, true)); x += 42;
                if (GUI.Button(nursery, GUIContent.none, GUIStyle.none)) Explain(nursery, "Cradle friends", "From the Lamp Tree's Cradle: one of these three friends can start the run at your side. They change every run. Tap one to bring it.");
                foreach (UnitDefinition friend in friends)
                {
                    Rect cell = new Rect(x, row.y, 44, 44); bool on = nurseryPick == friend.Id;
                    if (on) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Brings " + RunUICopy.UnitName(friend); }
                    ArcadeSkin.PortraitSlot(cell, on); RunHud.Sprite(new Rect(cell.x + 6, cell.y + 6, 32, 32), friend.Body);
                    if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { nurseryPick = on ? null : friend.Id; CheckpointAudio.PlayInterfaceId("ui.select"); Explain(cell, RunUICopy.UnitName(friend), on ? "Left at home." : "Starts the run at your side."); }
                    x += 48;
                }
                x += 12;
            }
            if (coat != null)
            {
                Rect cell = new Rect(x, row.y, 44, 44);
                if (altCoat) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Starts with " + RunUICopy.UnitName(coat); }
                ArcadeSkin.PortraitSlot(cell, altCoat); RunHud.Sprite(new Rect(cell.x + 6, cell.y + 6, 32, 32), coat.Body);
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { altCoat = !altCoat; CheckpointAudio.PlayInterfaceId("ui.select"); Explain(cell, "Second coat", (altCoat ? "Start with " + RunUICopy.UnitName(coat) + " instead of the usual first friend." : "Back to the usual first friend.") + " Earned in the Wardrobe by playing this hero."); }
                x += 56;
            }
            if (canHabit)
            {
                Rect cell = new Rect(x, row.y, 44, 44);
                if (habit) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = MetaProgression.HabitAdded(HeroKits.For(champion.Id)).Text.Replace("Second habit: ", "").TrimEnd('.'); }
                ArcadeSkin.PortraitSlot(cell, habit); RunHud.Sprite(new Rect(cell.x + 4, cell.y + 4, 36, 36), MetaIcons.Station(MetaStation.Wardrobe, true));
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { habit = !habit; CheckpointAudio.PlayInterfaceId("ui.select"); Explain(cell, "Second habit", (habit ? MetaProgression.HabitAdded(HeroKits.For(champion.Id)).Text.Replace("Second habit: ", "Swaps one team bonus: ") : "The hero keeps its usual team bonus.") + " Earned in the Wardrobe."); }
                x += 56;
            }
            if (keeps.Count > 0)
            {
                // the Postbox: one keepsake relic to bring; a tap opens the picker with all of them (no cycling through)
                Rect cell = new Rect(x, row.y, 44, 44); bool on = keepsakePick != null;
                ItemDefinition relic = on ? catalog.Item(keepsakePick) : null;
                if (on) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Brings " + relic?.Name; }
                ArcadeSkin.PortraitSlot(cell, on); RunHud.Sprite(new Rect(cell.x + 5, cell.y + 5, 34, 34), relic?.Icon ?? MetaIcons.Station(MetaStation.Postbox, true));
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { keepsakeOpen = !keepsakeOpen; keepsakeAt = cell; keepsakeOpenedAt = Time.unscaledTime; explainText = null; CheckpointAudio.PlayInterfaceId("ui.open"); }
            }
            if (caption != null) GUI.Label(new Rect(row.x, row.yMax + 1, row.width, 22), caption, note);
            return true;
        }

        private static string PageTitle(Page page)
        {
            switch (page)
            {
                case Page.Roster: return "Who Steps Through Tonight?";
                case Page.Resume: return "Where You Left Off";
                case Page.ConfirmOverwrite: return "Begin Again?";
                case Page.Settings: return "Light, Sound & Comfort";
                case Page.Help: return "How the Night Works";
                case Page.Records: return "Letters You Kept";
                case Page.Achievements: return "DREAM JOURNAL";
                case Page.Museum: return "Everyone You Have Met";
                default: return "Arcane Brigade";
            }
        }
        private static string PageEyebrow(Page page) => page == Page.Roster ? "THE WAITING PLACE · NEW JOURNEY" : page == Page.Records ? "THE WAITING PLACE · RECORDS" : page == Page.Museum ? "THE WAITING PLACE · COLLECTION" : "THE WAITING PLACE";
        public static Rect RosterCardBounds(int index) => new Rect(index % 3 * 210, index / 3 * 245, 200, 235);
        public static Sprite RosterPortrait(UnitDefinition unit, float time, int identity, bool focused, bool reducedMotion)
        {
            float curiosity = Mathf.Repeat(time + identity * .73f, 6.4f);
            CharacterActingEntry acting = CharacterActingLibrary.Current != null ? CharacterActingLibrary.Current.Find(unit.Id) : null;
            if (!reducedMotion && focused && curiosity < 1.3f && acting != null)
                return CharacterActingEntry.Frame(acting.Curious, curiosity / 1.3f);
            if (unit.AnimationFrames == null || unit.AnimationFrames.Length < 4) return unit.Body;
            bool stepping = focused && !reducedMotion && Mathf.Repeat(time + identity * 0.73f, 7f) > 6.2f;
            return unit.AnimationFrames[UnitVisual.AnimationFrame(time, identity, stepping, reducedMotion)] ?? unit.Body;
        }

        private void DrawSettings() => SettingsSheet.Draw(profile, Vector2.zero, Button, Label, SaveProfile, false);

        private void OnApplicationFocus(bool focus)
        {
            if (RunPaths.Diagnostic) return;
            focusLost = !focus; AudioListener.pause = focusLost || backgrounded;
        }
        private void OnApplicationPause(bool paused) { backgrounded = paused; AudioListener.pause = focusLost || backgrounded; }

        private void DrawRecords()
        {
            if (Button(new Rect(60, 620, 560, 52), "OPEN ACHIEVEMENT JOURNAL")) ShowPage(Page.Achievements);
            Label(new Rect(60, 165, 575, 80), "Wave " + profile.BestWave + " / Wins " + profile.Wins + " / Kills " + profile.TotalKills + "\nUnlocked difficulty: " + (RunMeta.UnlockedDifficulty(profile) + 1) + " / " + RunMeta.DifficultyNames.Length);
            scroll = ScrollPanel.Begin(new Rect(60, 250, 560, 350), scroll, new Rect(0, 0, 520, Mathf.Max(345, profile.History.Count * 95 + profile.Records.Count * 65 + profile.Achievements.FindAll(a => !a.StartsWith("journal-")).Count * 40 + 65)));
            float vertical = 0;
            for (int index = 0; index < profile.History.Count; index++)
            {
                RunRecord record = profile.History[index];
                if (Button(new Rect(0, vertical, 515, 85), record.Nickname + "\n" + record.Score.ToString("N0") + " points · " + RunUICopy.UnitName(record.Build.ChampionId) + " · " + (record.Build.Endless ? "Endless" : "Journey"))) historySelection = index;
                vertical += 95;
            }
            Label(new Rect(0, vertical, 510, 50), "RECORDS BY HERO / MODE / DIFFICULTY"); vertical += 60;
            foreach (ScoreRecord record in profile.Records) { Label(new Rect(0, vertical, 510, 60), RunUICopy.RecordName(record.Key, catalog) + "\n" + record.Score.ToString("N0") + " points · wave " + record.Hall); vertical += 65; }
              foreach (string achievement in profile.Achievements) if (!achievement.StartsWith("journal-")) { Label(new Rect(0, vertical, 510, 40), "✓ " + RunUICopy.AchievementName(achievement)); vertical += 40; }
            ScrollPanel.End();
            if (profile.History.Count > 0)
            {
                RunRecord record = profile.History[Mathf.Clamp(historySelection, 0, profile.History.Count - 1)];
                invoice.Draw(new Rect(660, 160, 545, 520), record.Build, catalog, record.Nickname, record.Score);
            }
            else Label(new Rect(690, 240, 470, 200), "No journeys yet. Finish one to see your brigade here.");
        }

        private bool SaveProfile()
        {
            try { storage.SaveProfile(profile); error = ""; return true; }
            catch (Exception exception) { error = "Failed to save settings: " + exception.Message; CheckpointAudio.PlayInterfaceId("ui.error"); return false; }
        }
    }
}
