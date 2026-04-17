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
            if (!PlaytestFixCapture.Requested) throw new InvalidOperationException("Hub fixture requires the play-test capture");
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
            ArcadeSkin.Bundle = profile.Cosmetic;
            error = storage.LastError;
            ReloadSaveSummary();
            PresentationSettings.Apply(profile);
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            music = gameObject.AddComponent<ArcadeMusic>();
            gameObject.AddComponent<CheckpointAudio>().ConfigurePresentation();
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            if (RunLaunch.OpenHub) RunLaunch.OpenHub = false; // back from a finished run: the main menu (owner 22/10/2026)
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
            RunLaunch.StartFriend = nurseryPick; RunLaunch.AltStarter = altCoat; RunLaunch.SecondHabit = habit;
            RunLaunch.Keepsake = keepsakePick; RunLaunch.Daily = dailyDay;
            if (dailyDay != null) { RunLaunch.Difficulty = 0; RunLaunch.ChallengeIndex = 0; }
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
            if (CurrentPage != Page.Landing && CurrentPage != Page.Hub)
            {
                Texture2D room = Backdrop();
                if (room != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), room, ScaleMode.ScaleAndCrop);
                ArcadeSkin.Line(new Rect(0, 0, 1280, 720), new Color(0.02f, 0.03f, 0.055f, PresentationSettings.HighReadability ? 0.97f : 0.84f));
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
                case Page.Roster: DrawRoster(); break;
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
                    Label(new Rect(120, 210, 1040, 180), "A new run replaces the current resume boundary.\nThe old run is backed up before combat. Profile and records are not deleted.\nIf backup fails, the new run does not begin.");
                    if (Button(new Rect(180, 490, 400, 70), "KEEP OLD RUN")) ShowPage(Page.Roster);
                    if (navigation.Button(new Rect(700, 490, 400, 70), "BACK UP AND START", primary)) Launch(false);
                    break;
                case Page.Settings: DrawSettings(); break;
                case Page.Help:
                    Label(new Rect(110, 175, 1060, 440), "MOVE  —  WASD / arrow keys / left stick, or drag the lower-left of the screen. Attacks happen automatically while you move.\nYour friends fight beside you on their own; there are no attack or skill buttons.\nTHE SHOP  —  walk up to a stand and press Space / A, or tap USE. Nothing is bought for you.\nCompanions cannot fall and ask nothing in return. They only block shots when a relic says so. Stock cannot be sold or held back.\nTHE PEOPLE WITH YOU  —  notebook icon / I: stats, friends, relics and links. Tab or LB/RB changes tabs; I / Esc / B closes.\nTHE WAITING JOURNAL  —  A sealed note under your lanterns means a new page is waiting; click the note, or approach the archive cabinet and press Space / A to read and reread chapters. Settings can make pages open by themselves. The people tab records the friends you have met. The warm chair is a separate, dangerous invitation.\nPAUSE  —  pause icon / Esc / Start. Leaving the app pauses too.\nClearing a wave restores your full health. The deeper you go, the darker it becomes; your light, and your friends' light, go with you.\nBetween nights, the Waiting Room turns Lamp Oil and Keepsakes into lasting help.");
                    break;
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
            GUI.matrix = previous;
            GUI.color = previousTint;
            GUI.skin = previousSkin;
        }

        // Main menu = the Waiting Room (Resources/MienCho/WaitingRoom.png, painted 16:9, 1280x720 design space):
        // the door begins a journey, the armchair is where you left off, the bookshelf holds everyone you have met,
        // the letters on the side table are the ones you kept, the lamp sets light and sound. How to play lives only
        // in the bottom row; the row mirrors every entry one-to-one in the same order as before.
        // owner 22/10/2026: NEW JOURNEY goes straight to hero select; the Waiting Room (permanent upgrades) is its own entry
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
            Texture2D room = Backdrop();
            if (room != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), room, ScaleMode.ScaleAndCrop);
            if (landingShade == null)
            {
                landingShade = new Texture2D(64, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[64];
                for (int index = 0; index < pixels.Length; index++) pixels[index] = new Color(0.02f, 0.03f, 0.06f, Mathf.Lerp(0.75f, 0, index / 63f));
                landingShade.SetPixels(pixels); landingShade.Apply(false, true);
            }
            GUI.DrawTexture(new Rect(0, 0, 600, 720), landingShade);
            DrawChairVoice();
            ArcadeSkin.Line(new Rect(0, 590, 1280, 130), new Color(0.02f, 0.025f, 0.05f, 0.85f));
            if (ArcadeSkin.HasKit) ArcadeSkin.Ornament(new Rect(36, 580, 1208, 20), KitDivider.Lantern);
            else ArcadeSkin.Divider(new Rect(36, 590, 1208, 1), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.3f));
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

            int mouseSpot = -1;
            for (int index = 0; index < SpotAreas.Length; index++)
                if (SpotAreas[index].width > 0 && SpotEnabled(index) && SpotAreas[index].Contains(Event.current.mousePosition)) mouseSpot = index;
            int focus = mouseSpot >= 0 ? mouseSpot : Mathf.Clamp(navigation.Selected, 0, SpotAreas.Length - 1);
            if (Event.current.type == EventType.Repaint && mouseSpot >= 0 && mouseSpot != hoveredSpot) CheckpointAudio.PlayInterfaceId(SpotCues[mouseSpot]);
            if (Event.current.type == EventType.Repaint) hoveredSpot = mouseSpot;

            // the door always breathes a little; objects you can use glow faintly so they read as interactive
            ArcadeSkin.Glow(new Rect(760, -40, 400, 560), new Color(1f, 0.85f, 0.65f, 0.08f + 0.07f * breathe));
            for (int index = 1; index < SpotAreas.Length; index++)
            {
                Rect area = SpotAreas[index];
                if (area.width <= 0) continue;
                float hint = SpotEnabled(index) ? 0.10f + 0.06f * breathe : 0.03f;
                ArcadeSkin.Glow(new Rect(area.center.x - 70, area.center.y - 60, 140, 120), new Color(1f, 0.80f, 0.50f, hint));
            }
            Rect focused = SpotAreas[focus];
            if (focused.width > 0)
            {
                float pad = 26 + breathe * 6;
                ArcadeSkin.Glow(new Rect(focused.x - pad, focused.y - pad, focused.width + pad * 2, focused.height + pad * 2), new Color(1f, 0.80f, 0.48f, 0.32f + 0.06f * breathe));
            }

            // the hero stands in the room carrying their own light
            UnitDefinition hero = catalog.Champions[selection];
            Sprite pose = hero.AnimationFrames.Length >= 4 ? hero.AnimationFrames[UnitVisual.AnimationFrame(Time.unscaledTime, selection, false, profile.ReducedMotion)] : hero.Body;
            Rect heroRect = new Rect(540, 345 + (reduced ? 0 : Mathf.Sin(Time.unscaledTime * 1.4f) * 4), 210, 240);
            ArcadeSkin.Glow(new Rect(heroRect.x - 120, heroRect.y - 80, heroRect.width + 240, heroRect.height + 200), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.20f + 0.06f * breathe));
            ArcadeSkin.Glow(new Rect(heroRect.x + 20, heroRect.yMax - 30, heroRect.width - 40, 50), new Color(0, 0, 0, 0.55f));
            Matrix4x4 unflipped = GUI.matrix;
            if (focused.width > 0 && focused.center.x < heroRect.center.x) GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), heroRect.center);
            RunHud.Sprite(heroRect, pose);
            GUI.matrix = unflipped;

            // a paper label for the focused entry, placed by its object (or above the bottom row)
            string label = focus == 1 && CanResume ? "CONTINUE · WAVE " + saved.Wave : SpotLabels[focus];
            Vector2 tagSize = new Vector2(Mathf.Max(180, chip.CalcSize(new GUIContent(label)).x + 48), 32);
            Rect tag;
            if (focused.width > 0)
                tag = new Rect(Mathf.Clamp(SpotTags[focus].x - tagSize.x * 0.5f, 10, 1270 - tagSize.x), SpotTags[focus].y, tagSize.x, tagSize.y);
            else tag = new Rect(36 + focus * 204 + 98 - tagSize.x * 0.5f, 612, tagSize.x, tagSize.y);
            ArcadeSkin.Ticket(tag, ArcadeSkin.Paper, ArcadeSkin.Accent);
            chip.normal.textColor = ArcadeSkin.Ink;
            GUI.Label(tag, label, chip);
            ArcadeSkin.Seal(new Vector2(tag.xMax - 4, tag.y + 2), 30, "B");
            GUI.Label(new Rect(tag.center.x - 220, tag.yMax + 4, 440, 30), focus == 1 && !CanResume ? "nothing to continue yet" : SpotHints[focus].ToLowerInvariant(), spotHint);

            for (int index = 0; index < SpotAreas.Length; index++)
                if (SpotAreas[index].width > 0 && GUI.Button(SpotAreas[index], GUIContent.none, GUIStyle.none)) ActivateSpot(index);

            // keyboard / controller / readability row mirrors every entry one-to-one (order is relied on by navigation checks)
            if (smallButton == null) smallButton = new GUIStyle(button) { fontSize = 15 };
            for (int index = 0; index < SpotLabels.Length; index++)
            {
                GUI.enabled = SpotEnabled(index);
                string caption = SpotLabels[index];
                // row one: NEW JOURNEY and CONTINUE, wide and gold; row two: the other five, walnut (owner, 22/10/2026)
                Rect spot;
                if (index < 2) spot = new Rect(36 + index * 330, 606, 316, 48);
                else { float step = (1208f + 12) / (SpotLabels.Length - 2); spot = new Rect(36 + (index - 2) * step, 664, step - 12, 44); }
                if (navigation.Button(spot, caption, index == 0 || index == 1 && CanResume ? primary : smallButton)) ActivateSpot(index);
                // a small lit badge when the lamp oil can buy something in the Waiting Room
                if (index == 2 && HubAffordable()) { ArcadeSkin.Glow(new Rect(spot.xMax - 22, spot.y - 14, 28, 28), new Color(1f, .82f, .45f, .55f)); ArcadeSkin.Seal(new Vector2(spot.xMax - 8, spot.y), 22, "!"); }
            }
            GUI.enabled = true;
        }

        private void DrawRoster()
        {
            if (catalog.Champions == null || catalog.Champions.Length == 0) { Label(new Rect(75, 200, 1100, 90), "No hero available."); return; }
            selection = Mathf.Clamp(selection, 0, catalog.Champions.Length - 1);
            GUIStyle caption = portraitCaption ?? (portraitCaption = ArcadeTypography.Style(18, true));
            caption.alignment = TextAnchor.MiddleCenter;
            GUIStyle note = rosterNote ?? (rosterNote = ArcadeTypography.Style(16));
            Label(new Rect(65, 142, 660, 32), "CHOOSE YOUR HERO");
            scroll = ScrollPanel.Begin(new Rect(65, 180, 660, 470), scroll, new Rect(0, 0, 620, Mathf.Max(465, Mathf.CeilToInt(catalog.Champions.Length / 3f) * 245)));
            for (int index = 0; index < catalog.Champions.Length; index++)
            {
                UnitDefinition unit = catalog.Champions[index];
                bool unlocked = profile.BestWave >= unit.UnlockWave;
                Rect card = RosterCardBounds(index);
                if (ArcadeSkin.HasKit) ArcadeSkin.Frame(card, KitFrame.Dialogue, .42f); else RunHud.Fill(card, index == selection ? ArcadeSkin.CardLit : ArcadeSkin.Card);
                if (index == selection) { ArcadeSkin.Glow(new Rect(card.x + 10, card.y, card.width - 20, 150), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.26f)); ArcadeSkin.Slant(new Rect(card.x + 14, card.yMax - 16, card.width - 28, 8), ArcadeSkin.Teal); }
                if (navigation.Button(card, "", GUIStyle.none)) { if (selection != index) rosterDescriptionScroll = Vector2.zero; selection = index; }
                Color previousColor = GUI.color;
                if (!unlocked) GUI.color = new Color(0.65f, 0.65f, 0.65f);
                float portraitSize = index == selection ? 120 : 110;
                // portrait + name centred in the card (owner, 21/10/2026); a locked hero lifts them to leave room for its chip
                float top = card.y + (unlocked ? 30 : 12);
                RunHud.Sprite(new Rect(card.center.x - portraitSize * 0.5f, top, portraitSize, 115), RosterPortrait(unit, Time.unscaledTime, index, index == selection, profile.ReducedMotion));
                GUI.color = previousColor;
                TextAnchor keepAlign = caption.alignment; caption.alignment = TextAnchor.MiddleCenter;
                GUI.Label(new Rect(card.x + 10, top + 120, card.width - 20, 52), RunUICopy.UnitName(unit), caption);
                caption.alignment = keepAlign;
                if (!unlocked) ArcadeSkin.Tag(new Rect(card.x + 20, card.y + 186, 160, 26), "LOCKED · WAVE " + unit.UnlockWave, CodeTone.Cream, chip);
            }
            ScrollPanel.End();
            UnitDefinition champion = catalog.Champions[selection];
            if (catalog.HeroRevision > 0)
            {
                int iconIndex = 0;
                foreach (BondDefinition bond in catalog.Bonds)
                    if (Array.IndexOf(champion.RecruitThemes, bond.Id) >= 0) RunHud.Sprite(new Rect(690 - iconIndex++ * 34, 145, 28, 28), bond.Icon);
                if (iconIndex > 0) GUI.Label(new Rect(690 - iconIndex * 34 - 130, 145, 125, 28), "RECRUIT THEMES", note);
            }
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(new Rect(760, 135, 460, 445), KitFrame.Dialogue, .45f); else RunHud.Fill(new Rect(760, 135, 460, 445), ArcadeSkin.Card);
            ArcadeSkin.Glow(new Rect(752, 125, 170, 175), new Color(1f, 0.8f, 0.45f, 0.28f));
            RunHud.Sprite(new Rect(775, 145, 132, 142), RosterPortrait(champion, Time.unscaledTime, selection, true, profile.ReducedMotion));
            if (rosterTitle == null) { rosterTitle = ArcadeTypography.Style(28, true); if (ArcadeTypography.Display != null) rosterTitle.font = ArcadeTypography.Display; }
            GUI.Label(new Rect(918, 150, 290, 70), RunUICopy.UnitName(champion), rosterTitle);
            ArcadeSkin.Tag(new Rect(918, 222, 280, 28), RunUICopy.WeaponName(champion.Weapon.Id).ToUpperInvariant(), CodeTone.Walnut, chip);
            GUI.Label(new Rect(918, 256, 290, 34), RunUICopy.UnlockCondition(champion, profile.BestWave), note);
            string description = RunUICopy.ChampionDescription(champion);
            if (catalog.HeroRevision > 0)
            {
                description += "\n\nRECRUIT THEMES: ";
                foreach (BondDefinition bond in catalog.Bonds)
                    if (Array.IndexOf(champion.RecruitThemes, bond.Id) >= 0) description += bond.Name + " · ";
                // the fixed pool: only these bonds can come together in this hero's shop
                description += "\nBONDS IN THIS HERO'S SHOP: ";
                foreach (BondDefinition bond in K11HeroRules.PoolBonds(catalog, champion)) description += bond.Name + " · ";
            }
            description += "\n\nSTARTS WITH\n";
            foreach (StatModifier modifier in champion.ChampionModifiers)
                description += GlossaryUI.Modifier(modifier.Effect, modifier.Scope, modifier.Value) + "\n";
            float descriptionHeight = Mathf.Max(180, GlossaryUI.Height(description, text, 400) + 8);
            bool loadout = DrawLoadout(champion, new Rect(780, 402, 425, 44), note);
            rosterDescriptionScroll = ScrollPanel.Begin(new Rect(780, 300, 425, loadout ? 98 : 185), rosterDescriptionScroll, new Rect(0, 0, 405, descriptionHeight));
            GlossaryUI.Text(new Rect(0, 0, 400, descriptionHeight), description, text);
            ScrollPanel.End();
            // one journey only (play-test 20/10/2026): the Night Shift / First Light / Small Crew challenges are off the menu;
            // their rules stay in RunMeta, dormant, for a later daily run
            challenge = 0;
            if (Button(new Rect(780, 492, 420, 46), "DIFFICULTY: " + RunMeta.DifficultyNames[difficulty])) difficulty = (difficulty + 1) % (RunMeta.UnlockedDifficulty(profile) + 1);
            GUI.Label(new Rect(780, 540, 430, 40), "Win a difficulty to unlock the next.", note);
            GUI.enabled = profile.BestWave >= champion.UnlockWave;
            if (navigation.Button(new Rect(780, 590, 420, 66), profile.BestWave >= champion.UnlockWave ? "STEP THROUGH ▶" : "LOCKED", primary)) { if (existingSave) ShowPage(Page.ConfirmOverwrite); else Launch(false); }
            GUI.enabled = true;
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
        private static Page BackOf(Page page) => page == Page.ConfirmOverwrite ? Page.Roster : page == Page.Roster ? Page.Landing : Page.Landing;

        /// <summary>Hero select's Waiting Room row (only what is unlocked): the Nursery's three friends, the Wardrobe's spare coat
        /// and second habit. A tap picks; the line under the row says what the pick does.</summary>
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
                RunHud.Sprite(new Rect(x, row.y + 2, 38, 40), MetaIcons.Station(MetaStation.Nursery, true)); x += 42;
                foreach (UnitDefinition friend in friends)
                {
                    Rect cell = new Rect(x, row.y, 44, 44); bool on = nurseryPick == friend.Id;
                    if (on) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Brings " + RunUICopy.UnitName(friend); }
                    ArcadeSkin.PortraitSlot(cell, on); RunHud.Sprite(new Rect(cell.x + 6, cell.y + 6, 32, 32), friend.Body);
                    if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { nurseryPick = on ? null : friend.Id; CheckpointAudio.PlayInterfaceId("ui.select"); }
                    x += 48;
                }
                x += 12;
            }
            if (coat != null)
            {
                Rect cell = new Rect(x, row.y, 44, 44);
                if (altCoat) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Starts with " + RunUICopy.UnitName(coat); }
                ArcadeSkin.PortraitSlot(cell, altCoat); RunHud.Sprite(new Rect(cell.x + 6, cell.y + 6, 32, 32), coat.Body);
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { altCoat = !altCoat; CheckpointAudio.PlayInterfaceId("ui.select"); }
                x += 56;
            }
            if (canHabit)
            {
                Rect cell = new Rect(x, row.y, 44, 44);
                if (habit) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = MetaProgression.HabitAdded(HeroKits.For(champion.Id)).Text.Replace("Second habit: ", "").TrimEnd('.'); }
                ArcadeSkin.PortraitSlot(cell, habit); RunHud.Sprite(new Rect(cell.x + 4, cell.y + 4, 36, 36), MetaIcons.Station(MetaStation.Wardrobe, true));
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { habit = !habit; CheckpointAudio.PlayInterfaceId("ui.select"); }
                x += 56;
            }
            if (keeps.Count > 0)
            {
                // the Postbox: one keepsake relic to bring (a tap moves to the next one, then none)
                Rect cell = new Rect(x, row.y, 44, 44); bool on = keepsakePick != null;
                ItemDefinition relic = on ? catalog.Item(keepsakePick) : null;
                if (on) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Brings " + relic?.Name; }
                ArcadeSkin.PortraitSlot(cell, on); RunHud.Sprite(new Rect(cell.x + 5, cell.y + 5, 34, 34), relic?.Icon ?? MetaIcons.Station(MetaStation.Postbox, true));
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) { int at = on ? keeps.IndexOf(keepsakePick) : -1; keepsakePick = at + 1 < keeps.Count ? keeps[at + 1] : null; CheckpointAudio.PlayInterfaceId("ui.select"); }
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
