using System.Collections.Generic;
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
        public static string StartFriend; public static bool AltStarter, SecondHabit, OpenHub, OpenLampTree;
        /// <summary>The Postbox's keepsake relic picked in hero select; the Daily Dream's day (null: an ordinary run).</summary>
        public static string Keepsake, Daily;
    }
    public sealed class RunMenu : MonoBehaviour
    {
        // The page stays centred; the right edge leaves 20 design pixels before the BACK button.
        public static Rect PageHeaderBounds => new Rect(290, 32, 700, 92);
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
        // a short plain note for the player; technical detail goes to the log only (UI standard: no exception text on screen)
        private string errorText;
        private float errorAt;
        private string error { get => errorText; set { errorText = value; errorAt = Time.unscaledTime; } }
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
            selection = index; rosterDescriptionScroll = Vector2.zero; ShowPage(Page.Roster); scroll.y = Mathf.Max(0, RosterCardBounds(RosterSlot(index)).y - 50);
            if (!HeroUnlocks.Open(profile, catalog.Champions[index])) ExplainLocked(catalog.Champions[index], RosterCardBounds(RosterSlot(index))); // as a tap does
        }
        /// <summary>Review capture: hero select with the Night Pact list open (two curses taken, on the isolated save).</summary>
        public void OpenPactsForDiagnostics()
        {
            if (!MenuReviewCapture.Requested) throw new InvalidOperationException("Pact fixture requires the menu review capture");
            MetaProgression.Ensure(profile).Pacts = (1 << MetaProgression.PactSwift) | (1 << MetaProgression.PactElites);
            ShowPage(Page.Roster); pactPickerAt = Time.unscaledTime; // opened after the page change
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
            pactPickerAt = -1; // a page change closes hero select's Night Pact list
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
        /// <summary>A run still in play can be continued; a finished one (lost or won) cannot: its summary was shown and recorded already (play-test 27/10/2026).</summary>
        public static bool CanResumeSnapshot(RunSnapshot snapshot) => snapshot != null && Enum.IsDefined(typeof(SessionPhase), snapshot.Phase) && snapshot.Phase != SessionPhase.Victory && snapshot.Phase != SessionPhase.Defeat;
        private bool CanResume => CanResumeSnapshot(saved);

        private void Start()
        {
            if (diagnosticState) return;
            storage = new RunStorage(RunPaths.Root);
            profile = storage.LoadProfile();
            difficulty = Mathf.Clamp(profile.LastDifficulty, 0, RunMeta.UnlockedDifficulty(profile)); // the difficulty chosen last time
            if (Deluxe.DifficultyLocked(difficulty)) difficulty = Deluxe.FirstDeluxeDifficulty - 1; // Deluxe was refunded or this is another device
            ArcadeSkin.Bundle = profile.Cosmetic;
            if (!string.IsNullOrEmpty(storage.LastError))
            {
                Debug.LogWarning(storage.LastError); storage.ClearError();
                error = "Your old records could not be opened, so a fresh record begins.";
            }
            ReloadSaveSummary();
            PresentationSettings.Apply(profile);
            if (FindAnyObjectByType<AudioListener>() == null) gameObject.AddComponent<AudioListener>();
            music = gameObject.AddComponent<ArcadeMusic>();
            gameObject.AddComponent<CheckpointAudio>().ConfigurePresentation();
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            if (RunLaunch.OpenHub) RunLaunch.OpenHub = false; // back from a finished run: the main menu (requirement 22/10/2026)
            // unless the run's end asked for the Waiting Room: straight to the Lamp Tree, where the haul is spent
            if (RunLaunch.OpenLampTree) { RunLaunch.OpenLampTree = false; ShowPage(Page.Hub); hub.OpenStation(MetaStation.Lantern); }
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
                        try { storage.RecordProgress(profile, saved, catalog); } catch (Exception exception) { Debug.LogWarning(exception); error = "Your progress will be saved next time."; }
                }
            }
            if (!string.IsNullOrEmpty(storage.LastError))
            {
                Debug.LogWarning(storage.LastError); storage.ClearError();
                if (saved == null) SetAsideUnreadableRun(); // a backup that still loaded needs no note
            }
            existingSave = File.Exists(Path.Combine(RunPaths.Root, "run.json")) || File.Exists(Path.Combine(RunPaths.Root, "run.json.bak"));
        }

        /// <summary>A run save this version cannot read is moved into ArchivedRuns (never deleted) so the menu stops tripping over it.</summary>
        private void SetAsideUnreadableRun()
        {
            try
            {
                string archive = Path.Combine(RunPaths.Root, "ArchivedRuns", "Unreadable_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
                foreach (string name in new[] { "run.json", "run.json.bak" })
                {
                    string source = Path.Combine(RunPaths.Root, name);
                    if (!File.Exists(source)) continue;
                    Directory.CreateDirectory(archive);
                    File.Move(source, Path.Combine(archive, name));
                }
                error = "An old journey could not be opened. It was put away, so a new one can begin.";
            }
            catch (Exception exception) { Debug.LogWarning(exception); error = "An old journey could not be opened."; }
        }

        private void Update()
        {
            if (music != null) music.ForceScene(CurrentPage == Page.Roster ? ArcaneCueLibrary.SceneSelect : CurrentPage == Page.Hub ? ArcaneCueLibrary.SceneHub : null);
            RunUIHints.Refresh();
            if (CurrentPage == Page.Hub)
            {
                HubAction walked = hub.Tick(Time.unscaledDeltaTime);
                if (walked == HubAction.NextNight) { fromHub = true; ShowPage(Page.Roster); } // the hub's door: straight on to a new journey
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
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
            {
                // a phone's Back on the front page asks before leaving (release testing 3/11/2026: it did nothing; the phone has
                // no EXIT button); Back again while it asks closes the question
                if (CurrentPage == Page.Landing && RunUIHints.Phone) { quitAskedAt = quitAskedAt < 0 ? Time.unscaledTime : -1; CheckpointAudio.PlayInterfaceId("ui.select"); }
                // an open popup closes first; Back again leaves the page
                else if (pactPickerAt >= 0) LivingUI.CloseThen("pact-picker", () => pactPickerAt = -1);
                else if (difficultyPickerAt >= 0) LivingUI.CloseThen("difficulty-picker", () => difficultyPickerAt = -1);
                else if (heroDetailsAt >= 0) LivingUI.CloseThen("hero-details", () => heroDetailsAt = -1);
                else GoBack();
            }
        }

        private void Launch(bool resume)
        {
            if (resume && !CanResume || !resume && (!HeroUnlocks.Open(profile, catalog.Champions[selection]) || difficulty > RunMeta.UnlockedDifficulty(profile) || Deluxe.DifficultyLocked(difficulty))) return;
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
                catch (Exception exception) { Debug.LogWarning(exception); error = "The last journey could not be set aside. Try again."; CheckpointAudio.PlayInterfaceId("ui.error"); return; }
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
        private void Label(Rect rectangle, string caption) { LocalizedGUI.Label(rectangle, ArcadeTypography.DisplayText(caption), text); }

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
                chip = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote), true);
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
                ArcadeSkin.Header(PageHeaderBounds, PageEyebrow(CurrentPage), PageTitle(CurrentPage));
            }
            if (CurrentPage != Page.Landing && CurrentPage != Page.Hub && Button(new Rect(1010, 40, 200, 50), "BACK"))
            {
                if (CurrentPage == Page.WeaponSandbox) CloseWeaponSandbox();
                else GoBack();
            }
            switch (CurrentPage)
            {
                case Page.Landing: GUI.enabled = quitAskedAt < 0; DrawLanding(); GUI.enabled = true; DrawQuitAsk(); break; // the question takes the taps while it asks
                case Page.Roster: DrawRoster(); DrawKeepsakePicker(); DrawExplain(); break;
                case Page.Hub:
                    HubAction chosen = hub.Draw(catalog, profile, selection, SaveProfile);
                    if (chosen == HubAction.NextNight) { fromHub = true; ShowPage(Page.Roster); } else if (chosen == HubAction.Back) ShowPage(Page.Landing); else if (chosen == HubAction.Daily) StartDaily();
                    else if (chosen == HubAction.Collection) { hub.Close(); fromHub = true; ShowPage(Page.Museum); } // the Museum counts finds for gifts; the Collection shows each one in full
                    break;
                case Page.Resume:
                    Label(new Rect(110, 200, 1060, 230), CanResume ? RunUICopy.UnitName(catalog.Unit(saved.ChampionId)) + " · Wave " + saved.Wave + "\n" + RunUICopy.PhaseName(saved.Phase) + " · Gold " + saved.Gold + " · Dreamsand " + saved.Eggs + "\n" + CompanionDeployment.Select(saved.Roster).Count + " friends · " + saved.Items.Count + " relic types\n" + (saved.Phase == SessionPhase.Victory || saved.Phase == SessionPhase.Defeat ? "Reopens the settled receipt without claiming rewards twice. Victorious runs can still enter endless mode." : "Unfinished combat restarts at wave start; shops resume after the last transaction.") : "No valid saved boundary.");
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
            if (CurrentPage != Page.WeaponSandbox && !string.IsNullOrEmpty(error) && Time.unscaledTime - errorAt < 7f)
            {
                // a short note on a plate over the lower middle that leaves after a few seconds, never a line over the page's own buttons
                var note = new Rect(320, 572, 640, 72);
                ArcadeSkin.Plate(note, ArcadeSkin.Gold);
                LocalizedGUI.Label(new Rect(note.x + 32, note.y, note.width - 64, note.height), error, new GUIStyle(text) { alignment = TextAnchor.MiddleCenter });
            }
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
        // each lamp's light layer on the 1920x1080 lane painting (x, y, width, height) and its lantern head
        private static readonly Vector4[] LaneGlows = { new Vector4(0, 501, 185, 456), new Vector4(21, 319, 277, 692), new Vector4(137, 437, 308, 636), new Vector4(254, 97, 361, 983), new Vector4(389, 299, 421, 781), new Vector4(536, 169, 483, 911), new Vector4(731, 65, 580, 1015), new Vector4(941, 0, 483, 1080) };
        private static readonly Vector2[] LaneHeads = { new Vector2(70, 549), new Vector2(164, 539), new Vector2(289, 516), new Vector2(436, 482), new Vector2(601, 436), new Vector2(777, 371), new Vector2(1011, 284), new Vector2(1275, 195) };
        private void DrawBootLoading()
        {
            if (booted) return;
            if (bootStarted < 0) bootStarted = Time.unscaledTime;
            float age = Time.unscaledTime - bootStarted, length = profile.ReducedMotion ? .4f : 1.8f, fade = Mathf.Clamp01((age - length) / .45f);
            if (fade >= 1) { booted = true; return; }
            Color keep = GUI.color;
            // an opaque night (a Fill panel is see-through and showed the menu's ghost behind the loading screen), past the frame's edges
            GUI.color = new Color(.035f, .04f, .07f, 1 - fade);
            GUI.DrawTexture(new Rect(-2000, -1200, 5280, 3120), Texture2D.whiteTexture);
            GUI.color = new Color(1, 1, 1, 1 - fade);
            float k = Mathf.Clamp01(age / (length * .75f)); // all eight lit before the end, held a moment before the menu
            // requirement 31/10/2026 (review): the night lane to the Waiting Room; the eight lanterns of the story (the HUD's own)
            // stand along its kerb and light one by one as the progress; the title sits where the main menu keeps it
            // the lamps are part of the painting: the dark lane, and each lamp's own light (cut from the same painting lit) fading in
            // over it in turn, then living - its flame flickers and a few sparks rise from it
            Sprite lane = ArcadeSkin.KitSprite("Menu/LaneDark");
            if (lane != null) GUI.DrawTexture(new Rect(0, 0, 1280, 720), lane.texture, ScaleMode.ScaleAndCrop);
            const float px = 1280f / 1920f; // the painting's pixels on this 1280x720 page
            for (int i = 0; i < LaneGlows.Length && lane != null; i++)
            {
                float lit = Mathf.Clamp01(k * 8.4f - i);
                if (lit <= 0) continue;
                Sprite glow = ArcadeSkin.KitSprite("Menu/LaneGlow" + i);
                if (glow == null) continue;
                float flicker = profile.ReducedMotion ? 1 : .9f + .06f * Mathf.Sin(Time.unscaledTime * 9.3f + i * 2.1f) + .04f * Mathf.Sin(Time.unscaledTime * 23.7f + i);
                Color before = GUI.color; GUI.color = new Color(before.r, before.g, before.b, before.a * lit * flicker);
                Vector4 g = LaneGlows[i];
                GUI.DrawTexture(new Rect(g.x * px, g.y * px, g.z * px, g.w * px), glow.texture);
                GUI.color = before;
                Vector2 head = LaneHeads[i] * px;
                ArcadeSkin.Glow(new Rect(head.x - 14, head.y - 14, 28, 28), new Color(1f, .9f, .6f, .35f * lit * flicker * (1 - fade))); // the hot core
                if (!profile.ReducedMotion)
                    for (int spark = 0; spark < 2; spark++)
                    {
                        float r = Mathf.Repeat(Time.unscaledTime * .55f + spark * .5f + i * .17f, 1);
                        ArcadeSkin.Diamond(new Vector2(head.x + Mathf.Sin(r * 6 + i + spark) * 8, head.y - 10 - r * 46), 3 + 2 * Mathf.Sin(r * Mathf.PI), new Color(1f, .8f, .45f, .8f * lit * Mathf.Sin(r * Mathf.PI) * (1 - fade)));
                    }
            }
            GUIStyle logo = logoStyle ?? (logoStyle = ArcadeTypography.Style(64, true));
            if (ArcadeTypography.Display != null) logo.font = ArcadeTypography.Display;
            TextAnchor a = logo.alignment; logo.alignment = TextAnchor.UpperLeft; logo.normal.textColor = ArcadeSkin.Cream;
            if (ArcadeSkin.KitSprite("Menu/GameLogo") is Sprite bootLogo) RunHud.Sprite(new Rect(28, 8, 440, 154), bootLogo);
            else LocalizedGUI.Label(new Rect(40, 54, 760, 90), "Arcane Brigade", logo);
            logo.alignment = a;
            GUI.color = keep;
        }

        // Main menu = the Waiting Room (Resources/MienCho/WaitingRoom.png, painted 16:9, 1280x720 design space):
        // the door begins a journey, the armchair is where you left off, the bookshelf holds everyone you have met,
        // the letters on the side table are the ones you kept, the lamp sets light and sound. How to play lives only
        // in the bottom row; the row mirrors every entry one-to-one in the same order as before.
        // requirement 22/10/2026: NEW JOURNEY goes straight to hero select; the Waiting Room (permanent upgrades) is its own entry
        private static readonly string[] SpotLabels = { "NEW JOURNEY", "CONTINUE", "WAITING ROOM", "COLLECTION", "RECORDS", "SETTINGS", "HOW TO PLAY", "EXIT" };
        private static readonly string[] SpotHints = { "Step through the door", "Sit back down where you left off", "Spend lamp oil on the room's stations", "Everyone and everything you have met", "The letters you kept", "Light, sound and comfort", "How the night works", "Close the game" };
        private static readonly string[] SpotCues = { "menu.spot.door", "menu.spot.hero", "menu.spot.lamp", "menu.spot.jar", "menu.spot.printer", "menu.spot.lamp", "menu.spot.bell", "menu.spot.door" };
        public static readonly Rect[] SpotAreas =
        {
            new Rect(800, 0, 312, 452),    // the carved door
            new Rect(40, 172, 360, 368),   // the armchair
            Rect.zero,                     // the Waiting Room (the bottom row only)
            new Rect(1160, 318, 120, 156), // the bookshelf
            new Rect(400, 280, 160, 86),   // the letters on the side table
            new Rect(340, 118, 190, 152),  // the lamp
            Rect.zero,
            Rect.zero,                     // exit (the bottom row only)
        };
        // where each entry's paper label sits in the room (centre x, top y)
        private static readonly Vector2[] SpotTags = { new Vector2(956, 372), new Vector2(222, 300), new Vector2(0, 0), new Vector2(1180, 280), new Vector2(480, 372), new Vector2(436, 80), new Vector2(0, 0), new Vector2(0, 0) };
        private int hoveredSpot = -1;
        private GUIStyle smallButton;
        private GUIStyle spotHint;
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
        /// <summary>The run is saved at every boundary already, so leaving is just closing the window.</summary>
        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void ActivateSpot(int index)
        {
            if (!SpotEnabled(index)) { CheckpointAudio.PlayInterfaceId("ui.locked"); return; }
            fromHub = false; // opened from the main menu: Back returns there
            switch (index)
            {
                case 0: ShowPage(Page.Roster); break; // the door: straight to hero select
                case 1: ShowPage(Page.Resume); break;
                case 2: ShowPage(Page.Hub); break;    // the Waiting Room: permanent upgrades, Daily Dream
                case 3: ShowPage(Page.Museum); break;
                case 4: ShowPage(Page.Records); break;
                case 5: ShowPage(Page.Settings); break;
                case 7: QuitGame(); break;
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
            LocalizedGUI.Label(new Rect(62, 302, 340, 80), "Would you like me to sit closer?", chairVoice);
            chairVoice.normal.textColor = new Color(ArcadeSkin.Paper.r, ArcadeSkin.Paper.g, ArcadeSkin.Paper.b, alpha);
            LocalizedGUI.Label(new Rect(60, 300, 340, 80), "Would you like me to sit closer?", chairVoice);
        }

        private Texture2D Backdrop()
        {
            // the painted menu background (UI kit, item 13); after Act One the room with the turned chair keeps the story
            if (!ActOneDone && ArcadeSkin.KitSprite("Menu/DoorRoom") is Sprite door) return door.texture; // the door the brigade waits at
            if (ActOneDone)
            {
                if (turnedRoom == null) turnedRoom = Resources.Load<Texture2D>("MienCho/WaitingRoom_ChairTurned");
                if (turnedRoom != null) return turnedRoom;
            }
            if (waitingRoom == null) waitingRoom = Resources.Load<Texture2D>("MienCho/WaitingRoom");
            if (waitingRoom == null && shopIllustration == null) shopIllustration = Resources.Load<Texture2D>("LostShop_v1");
            return waitingRoom != null ? waitingRoom : shopIllustration;
        }

        // requirement 31/10/2026 (review: one still Lantern Knight read as monotonous): the brigade of your last night waits at the
        // door - the hero in front, its friends around it, each in its own idle; the first time, a starting brigade. Different
        // for every player and after every run.
        private static readonly Vector3[] BrigadeSpots = // foot x, foot y, height on the 1280x720 menu (back row first)
        {
            new Vector3(665, 548, 130), new Vector3(880, 548, 130), new Vector3(560, 590, 142), new Vector3(985, 590, 142),
            new Vector3(600, 650, 156), new Vector3(950, 650, 156),
        };
        private List<UnitDefinition> brigade; private int brigadeFor = -1;
        private List<UnitDefinition> MenuBrigade(out UnitDefinition hero)
        {
            int key = profile?.History?.Count ?? 0;
            if (brigade == null || brigadeFor != key)
            {
                brigadeFor = key; brigade = new List<UnitDefinition>();
                RunRecord last = profile?.History != null && profile.History.Count > 0 ? profile.History[profile.History.Count - 1] : null;
                heroCache = last?.Build != null ? catalog.Unit(last.Build.ChampionId) : null;
                if (last?.Build?.Roster != null)
                    foreach (RosterEntry entry in CompanionDeployment.Select(last.Build.Roster))
                        if (brigade.Count < BrigadeSpots.Length && catalog.Unit(entry.UnitId) is UnitDefinition u && u.Body != null && !RunSession.StorySpecies(u.Id) && !brigade.Contains(u)) brigade.Add(u);
                if (brigade.Count == 0)
                    foreach (UnitDefinition u in catalog.Companions)
                        if (brigade.Count < BrigadeSpots.Length && u != null && u.Body != null && !u.SpecialRecruit && !u.GiftOnly && string.IsNullOrEmpty(u.ExclusiveHero) && !RunSession.StorySpecies(u.Id)) brigade.Add(u);
            }
            hero = heroCache ?? catalog.Unit("LanternKnight") ?? catalog.Champions[0];
            return brigade;
        }
        private UnitDefinition heroCache;

        /// <summary>A unit standing on the menu floor: its foot at the point, its idle frame, turned to face <paramref name="faceRight"/>.</summary>
        private static void MenuUnit(Vector2 foot, float height, UnitDefinition unit, int identity, bool faceRight, bool reduced, float hop)
        {
            Sprite pose = unit.AnimationFrames != null && unit.AnimationFrames.Length >= 2 ? unit.AnimationFrames[UnitVisual.Frame(unit, UnitVisual.AnimationFrame(Time.unscaledTime, identity, false, reduced))] : unit.Body;
            if (pose == null) return;
            Rect src = pose.rect; float w = height * src.width / src.height;
            ArcadeSkin.Glow(new Rect(foot.x - w * .45f, foot.y - 12, w * .9f, 24), new Color(0, 0, 0, .55f)); // its shadow on the floor
            Rect at = new Rect(foot.x - w * .5f, foot.y - height - hop, w, height);
            Texture2D t = pose.texture;
            Rect uv = new Rect(src.x / t.width, src.y / t.height, src.width / t.width, src.height / t.height);
            if (!faceRight) uv = new Rect(uv.xMax, uv.y, -uv.width, uv.height); // the art faces right; mirrored to face left
            GUI.DrawTextureWithTexCoords(at, t, uv);
        }

        private void DrawWaitingBrigade(bool reduced, float breathe)
        {
            // the Lantern Knight alone at the door (review 31/10/2026: one emblem, standing on the carpet, not floating): its feet on
            // the floor with a contact shadow, a slow breath that squashes it from the feet, its own acting poses now and then, the
            // flame on its head flickering and sending up embers, and its light pooling on the carpet around it
            UnitDefinition hero = catalog.Unit("LanternKnight") ?? catalog.Champions[0];
            Vector2 foot = ActOneDone ? new Vector2(965, 560) : new Vector2(800, 690);
            float height = ActOneDone ? 300 : 280, now = Time.unscaledTime;
            float flicker = reduced ? .8f : .75f + .15f * Mathf.Sin(now * 9.1f) + .1f * Mathf.Sin(now * 23.3f);
            ArcadeSkin.Glow(new Rect(foot.x - 300, foot.y - 120, 600, 200), new Color(1f, .74f, .38f, .22f * flicker)); // its light on the carpet
            Sprite pose = RosterPortrait(hero, now, 3, true, reduced);
            if (pose == null) return;
            // the drawn picture's own transparent rows under the feet are taken off, so the feet touch the floor
            Rect ink = SpriteInkMetrics.BoundsOf(pose);
            float padBottom = Mathf.Max(0, ink.yMin - pose.bounds.min.y) / Mathf.Max(.001f, pose.bounds.size.y);
            float padTop = Mathf.Max(0, pose.bounds.max.y - ink.yMax) / Mathf.Max(.001f, pose.bounds.size.y);
            float full = height / Mathf.Max(.2f, 1 - padBottom - padTop), w = full * pose.rect.width / pose.rect.height;
            float squash = reduced ? 0 : Mathf.Sin(now * 1.6f) * .018f; // breathing: a little taller, a little narrower
            float hh = full * (1 + squash), ww = w * (1 - squash * .6f);
            Rect at = new Rect(foot.x - ww * .5f, foot.y - hh * (1 - padBottom), ww, hh);
            ArcadeSkin.Glow(new Rect(foot.x - w * .32f, foot.y - 12, w * .64f, 24), new Color(0, 0, 0, .7f)); // the contact shadow
            RunHud.Sprite(at, pose);
            // the flame on its head: a hot core that flickers and embers drifting up from it
            Vector2 flame = new Vector2(foot.x + (ink.center.x - pose.bounds.center.x) / pose.bounds.size.x * ww, at.y + padTop * hh + hh * .06f);
            ArcadeSkin.Glow(new Rect(flame.x - 46, flame.y - 46, 92, 92), new Color(1f, .78f, .36f, .45f * flicker));
            if (!reduced)
                for (int i = 0; i < 7; i++)
                {
                    float r = Mathf.Repeat(now * .45f + i / 7f, 1);
                    Vector2 ember = new Vector2(flame.x + Mathf.Sin(r * 6 + i * 1.7f) * (10 + 26 * r), flame.y - 8 - r * 150);
                    ArcadeSkin.Diamond(ember, 3 + 4 * Mathf.Sin(r * Mathf.PI), new Color(1f, .7f + .2f * r, .35f, .9f * Mathf.Sin(r * Mathf.PI)));
                }
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

            // UI standard: the title alone, no small capital label above it
            GUIStyle logo = logoStyle ?? (logoStyle = ArcadeTypography.Style(64, true));
            if (ArcadeTypography.Display != null) logo.font = ArcadeTypography.Display;
            logo.wordWrap = false;
            if (logoShadow == null) { logoShadow = new GUIStyle(logo); }
            logoShadow.normal.textColor = new Color(0, 0, 0, 0.6f);
            ArcadeSkin.Glow(new Rect(-40, -10, 620, 200), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.10f + 0.04f * breathe));
            // the painted title lettering (review round 3); the typed title stands in without it
            if (ArcadeSkin.KitSprite("Menu/GameLogo") is Sprite gameLogo) RunHud.Sprite(new Rect(28, 8, 440, 154), gameLogo);
            else
            {
                LocalizedGUI.Label(new Rect(44, 58, 620, 90), "Arcane Brigade", logoShadow);
                logo.normal.textColor = ArcadeSkin.Cream;
                LocalizedGUI.Label(new Rect(40, 54, 620, 90), "Arcane Brigade", logo);
            }
            if (spotHint == null) { spotHint = ArcadeTypography.Style(24, false); spotHint.font = ArcadeTypography.Hand; spotHint.alignment = TextAnchor.MiddleCenter; spotHint.normal.textColor = ArcadeSkin.Cream; spotHint.wordWrap = false; }
            spotHint.alignment = TextAnchor.UpperLeft;
            LocalizedGUI.Label(new Rect(46, ArcadeSkin.KitSprite("Menu/GameLogo") != null ? 160 : 140, 760, 32), "someone is waiting for you…", spotHint);
            spotHint.alignment = TextAnchor.MiddleCenter;

            // requirement 24/10/2026: "nobody can tell what reacts to the mouse, let alone on a phone - redo the main menu, with the
            // Lantern Knight as its emblem". Every choice is a plain painted button with its icon, all visible at once (the
            // order mirrors SpotLabels for keys and pads); the Lantern Knight stands on the right holding up its lamp.
            DrawWaitingBrigade(reduced, breathe);
            if (spotPlate == null) { spotPlate = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote), true); }
            Sprite[] icons = { null, null, MetaIcons.Station(MetaStation.Lantern, true), MetaIcons.Station(MetaStation.Museum, true), MetaIcons.Station(MetaStation.Ledger, true), MetaIcons.Station(MetaStation.Clock, true), MetaIcons.Station(MetaStation.Postbox, true), null };
            if (smallButton == null)
            {
                // one size for every button in the grid: the size at which the longest caption fits (EXIT stood out larger,
                // the longer ones having shrunk to fit)
                smallButton = new GUIStyle(button) { fontSize = ArcadeTypography.On720(ArcadeTypography.SizeNote) };
                float room = SmallSpotWidth - 2 * (ArcadeSkin.CompactCap * 60 / 120f + 8) - 14;
                while (smallButton.fontSize > 12 && smallButton.CalcSize(LocalizedGUI.Content("        HOW TO PLAY")).x > room) smallButton.fontSize--;
            }
            for (int index = 0; index < SpotLabels.Length; index++)
            {
                if (index == 7 && (RunUIHints.Phone || Application.platform == RuntimePlatform.WebGLPlayer)) continue; // a phone leaves with its own button
                // requirement 1/11/2026: no "nothing to continue" button when no run waits (a button that does nothing), and EXIT is a
                // small round icon in the top right corner, not a button in the grid
                if (index == 1 && !CanResume) continue;
                if (index == 7)
                {
                    Rect exit = new Rect(1206, 16, 58, 58);
                    if (ArcadeSkin.Medallion(exit, KitMedallion.Close) | navigation.Button(exit, "", GUIStyle.none)) ActivateSpot(7);
                    continue;
                }
                GUI.enabled = SpotEnabled(index) && quitAskedAt < 0; // the leave question takes the taps while it asks
                string caption = index == 1 ? (CanResume ? "CONTINUE · WAVE " + saved.Wave : "NOTHING TO CONTINUE") : SpotLabels[index];
                // the Waiting Room (permanent upgrades) is a big plate of its own, never one small button among the rest
                float lift = CanResume ? 0 : 82; // without Continue the rest move up into its place
                Rect spot = index < 3 ? new Rect(44, 214 + index * 82 - (index >= 2 ? lift : 0), SmallSpotWidth * 2 + 10, 68) : new Rect(44 + (index - 3) % 2 * (SmallSpotWidth + 10), 476 - lift + (index - 3) / 2 * 72, SmallSpotWidth, 60); // a breath under the subtitle
                if (index == 2) caption = "         WAITING ROOM";
                else if (index >= 3 && icons[index] != null) caption = "        " + caption; // room for its icon (EXIT has none: centred)
                // UI standard: one gold button per screen; Continue when a run waits, New Journey otherwise
                bool main = CanResume ? index == 1 : index == 0;
                if (navigation.Button(spot, caption, main ? primary : index < 3 && (index != 1 || CanResume) ? button : smallButton)) ActivateSpot(index);
                if (index >= 2 && icons[index] != null) RunHud.Sprite(navigation.Moved(index == 2 ? new Rect(spot.x + 32, spot.y + 14, 40, 40) : new Rect(spot.x + 30, spot.y + 14, 32, 32)), icons[index]); // rides with the plate, clear of its end cap
                // a small lit badge when the lamp oil can buy something in the Waiting Room
                if (index == 2 && HubAffordable())
                {
                    // the plate itself pulses and a seal says it: there is something to buy
                    ArcadeSkin.Glow(navigation.Moved(new Rect(spot.x - 18, spot.y - 14, spot.width + 36, spot.height + 28)), new Color(1f, .82f, .45f, .22f + .18f * breathe));
                    ArcadeSkin.Glow(navigation.Moved(new Rect(spot.xMax - 50, spot.y + 6, 44, 44)), new Color(1f, .82f, .45f, .6f));
                    ArcadeSkin.Seal(navigation.Moved(new Vector2(spot.xMax - 28, spot.y + 28)), 30, "!");
                }
            }
            // the Deluxe entry on phones, until it is bought (requirement 4/11/2026: one place in the menu, never a pop-up of its own)
            if (Deluxe.Sold && !Deluxe.Owned)
            {
                GUI.enabled = quitAskedAt < 0;
                Rect deluxe = new Rect(44, 476 - (CanResume ? 0 : 82) + 2 * 72, SmallSpotWidth * 2 + 10, 64);
                if (navigation.Button(deluxe, "DELUXE · " + Deluxe.Price, button)) DeluxeOffer.Open();
            }
            GUI.enabled = true;
        }
        private GUIStyle spotPlate;
        /// <summary>The small menu buttons are wide enough for their words at the standard size (at 230 they shrank to 15, unreadable
        /// on a phone; requirement 3/11/2026).</summary>
        private const float SmallSpotWidth = 290;

        private void DrawRoster()
        {
            if (catalog.Champions == null || catalog.Champions.Length == 0) { Label(new Rect(75, 200, 1100, 90), "No hero available."); return; }
            selection = Mathf.Clamp(selection, 0, catalog.Champions.Length - 1);
            RosterOverlays(false); // an open picker or details card takes the taps before the page under it
            GUIStyle caption = portraitCaption ?? (portraitCaption = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote), true));
            caption.alignment = TextAnchor.MiddleCenter;
            GUIStyle note = rosterNote ?? (rosterNote = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote)));
            if (!HeroUnlocks.Listed(catalog.Champions[selection])) selection = Mathf.Max(0, Array.FindIndex(catalog.Champions, HeroUnlocks.Listed));
            int listed = 0; foreach (UnitDefinition u in catalog.Champions) if (HeroUnlocks.Listed(u)) listed++;
            scroll = ScrollPanel.Begin(new Rect(65, 180, 660, 460), scroll, new Rect(0, 0, 620, Mathf.Max(460, Mathf.CeilToInt(listed / 3f) * 232)));
            int slot = -1;
            for (int index = 0; index < catalog.Champions.Length; index++)
            {
                UnitDefinition unit = catalog.Champions[index];
                if (!HeroUnlocks.Listed(unit)) continue; // waits for an update or a later act: not shown at all
                slot++;
                bool unlocked = HeroUnlocks.Open(profile, unit);
                Rect card = RosterCardBounds(slot);
                if (ArcadeSkin.HasKit) ArcadeSkin.Frame(card, KitFrame.Dialogue, .42f); else RunHud.Fill(card, index == selection ? ArcadeSkin.CardLit : ArcadeSkin.Card);
                // the chosen hero's card is stamped (drawn at the end of its card); the others stand a little in shadow, and a
                // locked hero is a dark silhouette with a lock (requirement 4/11/2026: locked heroes looked playable)
                if (navigation.Button(card, "", GUIStyle.none))
                {
                    if (selection != index) rosterDescriptionScroll = Vector2.zero;
                    selection = index;
                    if (!unlocked) ExplainLocked(unit, card);
                }
                Color previousColor = GUI.color;
                if (!unlocked) GUI.color = new Color(0.22f, 0.22f, 0.28f);
                else if (index != selection) GUI.color = new Color(.7f, .7f, .76f); // the others stand in shadow
                const float portraitSize = 160; // the hero fills its card; the chosen one keeps its size (nothing shifts)
                // portrait and name at the same place on every card, locked or not; a locked one's chip sits under its name
                float top = card.y + 6;
                RunHud.Sprite(new Rect(card.center.x - portraitSize * 0.5f, top, portraitSize, 140), RosterPortrait(unit, Time.unscaledTime, index, true, profile.ReducedMotion)); // every hero in the roster acts (requirement 24/10/2026)
                GUI.color = previousColor;
                TextAnchor keepAlign = caption.alignment; caption.alignment = TextAnchor.MiddleCenter;
                LocalizedGUI.Label(new Rect(card.x + 10, top + 142, card.width - 20, 38), RunUICopy.UnitName(unit), caption);
                caption.alignment = keepAlign;
                if (!unlocked) ArcadeSkin.Tag(new Rect(card.x + 12, card.y + 186, card.width - 24, 28), HeroUnlocks.Coming(unit) ? "COMING SOON" : HeroUnlocks.Short(unit), CodeTone.Cream, chip);
                if (!unlocked) ArcadeSkin.Lock(new Vector2(card.x + 34, card.y + 34), 22, ArcadeSkin.KitCream); // the same lock as the collection's
                if (index == selection) ArcadeSkin.PickedSeal(card); // the chosen hero's card is stamped
            }
            ScrollPanel.End();
            UnitDefinition champion = catalog.Champions[selection];
            // requirement 25/10/2026 (the text box was too small): the hero's panel is the whole right column; the difficulty sits
            // under the hero grid and STEP THROUGH under the panel, one bottom row
            Rect heroPanel = new Rect(745, 135, 490, 505);
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(heroPanel, KitFrame.Dialogue, .45f); else RunHud.Fill(heroPanel, ArcadeSkin.Card);
            // the chosen hero stands in a spotlight on a little stage (review round 3)
            if (ArcadeSkin.KitSprite("Menu/HeroStage") is Sprite stage) RunHud.Sprite(new Rect(752, 220, 176, 74), stage);
            else ArcadeSkin.Glow(new Rect(752, 125, 170, 175), new Color(1f, 0.8f, 0.45f, 0.28f));
            RunHud.Sprite(new Rect(775, 134, 132, 132), RosterPortrait(champion, Time.unscaledTime, selection, true, profile.ReducedMotion));
            if (rosterTitle == null) { rosterTitle = ArcadeTypography.Style(28, true); if (ArcadeTypography.Display != null) rosterTitle.font = ArcadeTypography.Display; }
            LocalizedGUI.Label(new Rect(934, 150, 286, 70), RunUICopy.UnitName(champion), rosterTitle); // right of the stage
            ArcadeSkin.Tag(new Rect(934, 222, 230, 30), RunUICopy.WeaponName(champion.Weapon.Id), CodeTone.Walnut, chip);
            string unlockNote = HeroUnlocks.Full(profile, champion, catalog, false); // the count stands beside its bar
            if (!string.IsNullOrEmpty(unlockNote))
            {
                // the locked hero's road, on its own line with a bar (it was one small line under the weapon)
                LocalizedGUI.Label(new Rect(934, 256, 290, 30), HeroUnlocks.Coming(champion) ? "Arrives in a later update" : unlockNote, note);
                float share = HeroUnlocks.Share(profile, champion);
                if (share >= 0)
                {
                    Rect bar = new Rect(934, 292, 160, 14);
                    if (ArcadeSkin.Strip(bar, "Info/RatioTrack", 24, 24) && bar.width * share > bar.height) ArcadeSkin.Strip(new Rect(bar.x + 2, bar.y + 2, (bar.width - 4) * share, bar.height - 4), "Info/RatioFill", 20, 20);
                    var shown = HeroUnlocks.Progress(profile, HeroUnlocks.RuleOf(champion));
                    LocalizedGUI.Label(new Rect(bar.xMax + 10, 284, 120, 30), shown.have.ToString("N0") + " / " + shown.need.ToString("N0"), note);
                }
            }
            else if (catalog.HeroRevision > 0)
            {
                // the friends this hero's shop leans to, as tappable icons under the weapon (review 31/10/2026: they floated over
                // the hero grid with a small label, like a tag over a heading)
                int iconIndex = 0;
                foreach (BondDefinition bond in catalog.Bonds)
                    if (Array.IndexOf(champion.RecruitThemes, bond.Id) >= 0)
                    {
                        Rect icon = new Rect(940 + iconIndex++ * 40, 260, 32, 32); RunHud.Sprite(icon, bond.Icon); // each theme icon is tappable and says what it means (requirement 24/10/2026)
                        if (TouchTargets.Button(new Rect(icon.x - 4, icon.y - 4, 40, 40), GUIContent.none, GUIStyle.none)) Explain(icon, bond.Name + " theme", "Friends of this theme join this hero's shop more often. " + bond.Threshold + " of them together wake a bond bonus.");
                    }
            }
            // requirement 25/10/2026: the panel says who the hero is in two lines; everything else is one tap away (the i beside it)
            string fullDescription = RunUICopy.ChampionDescription(champion);
            string description = ShortHero(champion);
            if (catalog.HeroRevision > 0)
            {
                fullDescription += "\n\nBonds in this hero's shop: ";
                foreach (BondDefinition bond in K11HeroRules.PoolBonds(catalog, champion)) fullDescription += bond.Name + " · ";
            }
            // requirement 25/10/2026: the hero's basic numbers, each with its stat icon, then any bonus it starts with
            WeaponDefinition arm = champion.Weapon;
            description += "\n\nBasics\n" + Basic(ItemEffect.Health, "Health", Mathf.RoundToInt(champion.Health).ToString())
                + Basic(ItemEffect.Damage, "Damage", Mathf.RoundToInt(FriendlyWeaponBalance.BaseDamage(arm)).ToString())
                + Basic(ItemEffect.AttackSpeed, "Attacks a second", (1f / Mathf.Max(.05f, arm.Cooldown)).ToString("0.0"))
                + Basic(ItemEffect.AttackRange, "Range", arm.Range.ToString("0.0"))
                + Basic(ItemEffect.MoveSpeed, "Move speed", champion.Speed.ToString("0.0"));
            if (champion.ChampionModifiers != null && champion.ChampionModifiers.Length > 0)
            {
                description += "\nStarts with\n";
                foreach (StatModifier modifier in champion.ChampionModifiers)
                    description += GlossaryUI.Modifier(modifier.Effect, modifier.Scope, modifier.Value) + "\n";
            }
            float descriptionHeight = Mathf.Max(210, GlossaryUI.Height(description, text, 425) + 8);
            bool loadout = DrawLoadout(champion, new Rect(770, 546, 450, 44), note); // its caption keeps clear of the frame's foot
            rosterDescriptionScroll = ScrollPanel.Begin(new Rect(770, 298, 450, loadout ? 238 : 318), rosterDescriptionScroll, new Rect(0, 0, 430, descriptionHeight));
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
            bool deluxeDifficulty = Deluxe.DifficultyLocked(difficulty); // requirement 4/11/2026: the two hardest difficulties are part of Deluxe on phones
            bool sealedDifficulty = difficulty > RunMeta.UnlockedDifficulty(profile) || deluxeDifficulty;
            // the difficulty's danger colour on its name alone, never on the painted plate (requirement 24/10/2026: no tinted plates or boxes)
            GUIStyle danger = dangerStyle ?? (dangerStyle = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote), true)); danger.alignment = TextAnchor.MiddleLeft; danger.wordWrap = true;
            danger.normal.textColor = RunMeta.DifficultyColor(difficulty);
            // beside it, the Night Pact (release testing 3/11/2026: it lived only in the Waiting Room and players starting a run never
            // found it): its curses are chosen here, where the night is set up; what each difficulty does is in the difficulty list
            string dangerText = deluxeDifficulty ? "Part of Deluxe" : sealedDifficulty ? "Light the whole Lamp Tree" : DifficultyLine(difficulty);
            Rect dangerRect = new Rect(410, 656, 352, 52); // one line beside the button
            if (!sealedDifficulty) DrawPactButton(new Rect(410, 656, 340, 52));
            else
            {
                // the padlock stands just before the words, on their middle line, the pair centred together
                ArcadeSkin.Lock(new Vector2(dangerRect.x + 9, dangerRect.center.y), 16, danger.normal.textColor);
                dangerRect = new Rect(dangerRect.x + 26, dangerRect.y, dangerRect.width - 26, dangerRect.height);
            }
            if (sealedDifficulty) LocalizedGUI.Label(dangerRect, dangerText, danger);
            if (deluxeDifficulty && TouchTargets.Button(new Rect(410, 656, 352, 52), GUIContent.none, GUIStyle.none)) DeluxeOffer.Open("Sleepless and The Last Night are part of Deluxe.");
            GUI.enabled = HeroUnlocks.Open(profile, champion) && !sealedDifficulty;
            if (navigation.Button(new Rect(770, 650, 450, 62), HeroUnlocks.Open(profile, champion) && !sealedDifficulty ? "STEP THROUGH ▶" : HeroUnlocks.Coming(champion) ? "COMING SOON" : "LOCKED", primary)) { if (existingSave) ShowPage(Page.ConfirmOverwrite); else Launch(false); }
            GUI.enabled = true;
            RosterOverlays(true);
        }
        // ---- hero select's two popups: all of a hero's details, and the difficulty list --------------------------------------
        private float heroDetailsAt = -1, difficultyPickerAt = -1, pactPickerAt = -1;
        private const string PactSeen = "seen/night-pact";
        /// <summary>Hero select's Night Pact button: the curses taken and what they pay, a seal until it is first opened.</summary>
        private void DrawPactButton(Rect r)
        {
            var meta = MetaProgression.Ensure(profile);
            int taken = 0; for (int i = 0; i < MetaProgression.Pacts.Length; i++) if (MetaProgression.PactOn(meta.Pacts, i)) taken++;
            string label = taken == 0 ? "NIGHT PACT" : "NIGHT PACT · " + taken + " · ×" + MetaProgression.PactMultiplier(meta.Pacts).ToString("0.##");
            if (Button(r, label)) { pactPickerAt = Time.unscaledTime; if (!meta.Found.Contains(PactSeen)) { meta.Found.Add(PactSeen); SaveProfile(); } }
            if (!meta.Found.Contains(PactSeen))
            {
                float breathe = profile.ReducedMotion ? .8f : .7f + .3f * Mathf.Sin(Time.unscaledTime * 3);
                ArcadeSkin.Glow(navigation.Moved(new Rect(r.xMax - 46, r.y - 10, 44, 44)), new Color(1f, .82f, .45f, .6f * breathe));
                ArcadeSkin.Seal(navigation.Moved(new Vector2(r.xMax - 24, r.y + 12)), 26, "!");
            }
        }
        private string heroDetailsText = "", heroDetailsName = "";
        private Vector2 heroDetailsScroll;
        private GUIStyle overlayTitle, overlayLine;
        /// <summary>The hero in two lines: its own move's name and first sentence, and what it turns into.</summary>
        private static string Basic(ItemEffect icon, string name, string value) => "{bonus:" + icon + "|" + name + "|" + value + "}\n";
        private static string ShortHero(UnitDefinition champion)
        {
            HeroIdentityDefinition identity = HeroIdentities.For(champion.Id);
            if (identity == null) return RunUICopy.ChampionDescription(champion);
            string first = GameLocalization.Text(identity.Description); int stop = first.IndexOf(". ", System.StringComparison.Ordinal); if (stop > 0) first = first.Substring(0, stop + 1);
            HeroKit kit = HeroKits.For(champion.Id);
            return RunUICopy.Title(identity.Name) + "\n" + first + (kit != null ? "\nTransforms into " + RunUICopy.Title(kit.Transformation) + "." : ""); // UI standard: no all-capital lines
        }
        /// <summary>Drawn twice a frame: first (paint false) to take the taps before the page, last (paint true) to draw on top.</summary>
        private void RosterOverlays(bool paint)
        {
            bool details = heroDetailsAt >= 0, picker = difficultyPickerAt >= 0, pacts = pactPickerAt >= 0;
            if (!details && !picker && !pacts) return;
            if (paint != (Event.current.type == EventType.Repaint)) return;
            overlayTitle ??= new GUIStyle(ArcadeTypography.Style(26, true)) { alignment = TextAnchor.MiddleCenter };
            overlayLine ??= new GUIStyle(ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote))) { wordWrap = true };
            RunHud.Fill(new Rect(-1280, -720, 3840, 2160), new Color(0, 0, 0, .5f)); // past the 16:9 frame: the whole screen dims, never a box
            Matrix4x4 m = GUI.matrix; Color c = GUI.color;
            if (details)
            {
                Rect card = new Rect(240, 70, 800, 580);
                if (LivingUI.Auto("hero-details", card.center))
                {
                    ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f);
                    LocalizedGUI.Label(new Rect(card.x, card.y + 22, card.width, 40), heroDetailsName, overlayTitle);
                    float h = Mathf.Max(400, GlossaryUI.Height(heroDetailsText, overlayLine, 700) + 10);
                    heroDetailsScroll = ScrollPanel.Begin(new Rect(card.x + 40, card.y + 76, 720, card.height - 110), heroDetailsScroll, new Rect(0, 0, 700, h));
                    GlossaryUI.Text(new Rect(0, 0, 700, h), heroDetailsText, overlayLine);
                    ScrollPanel.End();
                    if (ArcadeSkin.Medallion(new Rect(card.xMax - 52, card.y - 4, 56, 56), KitMedallion.Close) && !LivingUI.Closing("hero-details")) LivingUI.CloseThen("hero-details", () => heroDetailsAt = -1);
                }
            }
            else if (pacts)
            {
                // the Night Pact: curses for this run, each paying more Lamp Oil and Keepsakes; a tap turns one on or off
                var meta = MetaProgression.Ensure(profile);
                int count = MetaProgression.Pacts.Length;
                Rect card = new Rect(290, 40, 700, 150 + count * 64);
                if (LivingUI.Auto("pact-picker", card.center))
                {
                    ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f);
                    LocalizedGUI.Label(new Rect(card.x, card.y + 20, card.width, 40), "NIGHT PACT", overlayTitle);
                    overlayLine.alignment = TextAnchor.MiddleCenter; overlayLine.normal.textColor = ArcadeSkin.KitCream;
                    LocalizedGUI.Label(new Rect(card.x + 40, card.y + 60, card.width - 80, 34), "Curses for this night. Lamp Oil and Keepsakes ×" + MetaProgression.PactMultiplier(meta.Pacts).ToString("0.##"), overlayLine);
                    for (int i = 0; i < count; i++)
                    {
                        MetaPact pact = MetaProgression.Pacts[i];
                        Rect row = new Rect(card.x + 40, card.y + 104 + i * 64, card.width - 80, 56);
                        bool on = MetaProgression.PactOn(meta.Pacts, i);
                        ArcadeSkin.CodePlate(row, CodeTone.Walnut, on ? ArcadeSkin.Chosen : 0); // the chosen pact: the same plate, lacquered red
                        Rect seal = new Rect(row.x + 10, row.y + 6, 44, 44);
                        if (MetaHub.CurseSeal(i) is Sprite wax) RunHud.Sprite(seal, wax); else RunHud.Sprite(seal, MetaIcons.Station(MetaStation.NightPact, true));
                        overlayTitle.fontSize = 20; overlayTitle.alignment = TextAnchor.MiddleLeft; overlayTitle.normal.textColor = on ? ArcadeSkin.Gold : ArcadeSkin.KitCream;
                        LocalizedGUI.Label(new Rect(row.x + 66, row.y, 220, row.height), pact.Name, overlayTitle);
                        overlayLine.alignment = TextAnchor.MiddleRight; overlayLine.normal.textColor = ArcadeSkin.KitCream;
                        LocalizedGUI.Label(new Rect(row.x + 280, row.y, row.width - 298, row.height), pact.Line + " · ×" + pact.Reward.ToString("0.##"), overlayLine);
                        int pick = i;
                        if (TouchTargets.Button(row, GUIContent.none, GUIStyle.none) && !LivingUI.Closing("pact-picker")) { MetaProgression.TogglePact(profile, pick); SaveProfile(); CheckpointAudio.PlayInterfaceId("ui.select"); }
                    }
                    overlayLine.alignment = TextAnchor.UpperLeft;
                    overlayTitle.fontSize = 26; overlayTitle.alignment = TextAnchor.MiddleCenter; overlayTitle.normal.textColor = ArcadeSkin.KitCream;
                    if (ArcadeSkin.Medallion(new Rect(card.xMax - 52, card.y - 4, 56, 56), KitMedallion.Close) && !LivingUI.Closing("pact-picker")) LivingUI.CloseThen("pact-picker", () => pactPickerAt = -1);
                }
            }
            else
            {
                int count = RunMeta.DifficultyNames.Length, unlocked = RunMeta.UnlockedDifficulty(profile);
                Rect card = new Rect(290, 40, 700, 110 + count * 64);
                if (LivingUI.Auto("difficulty-picker", card.center))
                {
                    ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f);
                    LocalizedGUI.Label(new Rect(card.x, card.y + 20, card.width, 40), "CHOOSE THE NIGHT'S DIFFICULTY", overlayTitle);
                    for (int i = 0; i < count; i++)
                    {
                        Rect row = new Rect(card.x + 40, card.y + 76 + i * 64, card.width - 80, 56);
                        bool deluxeRow = Deluxe.DifficultyLocked(i), sealedRow = i > unlocked || deluxeRow, chosen = i == difficulty;
                        ArcadeSkin.CodePlate(row, CodeTone.Walnut, sealedRow ? 3 : chosen ? ArcadeSkin.Chosen : 0);
                        overlayTitle.fontSize = 20; overlayTitle.alignment = TextAnchor.MiddleLeft; overlayTitle.normal.textColor = RunMeta.DifficultyColor(i);
                        LocalizedGUI.Label(new Rect(row.x + (sealedRow ? 44 : 18), row.y, 240, row.height), RunMeta.DifficultyNames[i], overlayTitle);
                        if (sealedRow) ArcadeSkin.Lock(new Vector2(row.x + 24, row.center.y), 14, ArcadeSkin.KitCream);
                        overlayLine.alignment = TextAnchor.MiddleRight; overlayLine.normal.textColor = ArcadeSkin.KitCream;
                        LocalizedGUI.Label(new Rect(row.x + 250, row.y, row.width - 268, row.height), deluxeRow ? "Part of Deluxe" : sealedRow ? "Light the whole Lamp Tree" : DifficultyLine(i), overlayLine);
                        overlayLine.alignment = TextAnchor.UpperLeft;
                        int pick = i;
                        if (deluxeRow && TouchTargets.Button(row, GUIContent.none, GUIStyle.none)) DeluxeOffer.Open("Sleepless and The Last Night are part of Deluxe.");
                        if (!sealedRow && TouchTargets.Button(row, GUIContent.none, GUIStyle.none) && !LivingUI.Closing("difficulty-picker")) LivingUI.CloseThen("difficulty-picker", () => { difficulty = pick; difficultyPickerAt = -1; });
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
        /// <summary>Hero select or the Collection opened from the Waiting Room goes back to it (the Collection to the Museum it came
        /// from); opened from the main menu, back to the main menu.</summary>
        private bool fromHub;
        private void GoBack()
        {
            if (fromHub && (CurrentPage == Page.Roster || CurrentPage == Page.Museum))
            {
                bool museumOpen = CurrentPage == Page.Museum;
                fromHub = false; ShowPage(Page.Hub);
                if (museumOpen) hub.OpenStation(MetaStation.Museum);
                return;
            }
            ShowPage(BackOf(CurrentPage));
        }
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
            explainTitle ??= ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeBody), true); explainBody ??= ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote)); explainBody.wordWrap = true;
            // the words sit inside the frame's own padded area (release testing 3/11/2026: the title ran under the top rim)
            float w = 360, h = 30 + 30 + explainBody.CalcHeight(LocalizedGUI.Content(explainText), w - 52) + 28;
            Rect card = new Rect(Mathf.Clamp(explainAt.center.x - w * .5f, 20, 1260 - w), explainAt.yMax + 10, w, h); // the menu's 1280x720 space
            if (card.yMax > 704) card.y = explainAt.y - h - 10;
            Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
            LivingUI.Pop(Time.unscaledTime - explainShown, new Vector2(card.center.x, card.y));
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f); else RunHud.Fill(card, ArcadeSkin.Card);
            Rect inner = ArcadeSkin.HasKit ? ArcadeSkin.Content(card, KitFrame.Dialogue, true, .5f) : new Rect(card.x + 18, card.y + 12, w - 36, h - 24);
            LocalizedGUI.Label(new Rect(inner.x, inner.y, inner.width, 28), explainHead, explainTitle);
            LocalizedGUI.Label(new Rect(inner.x, inner.y + 30, inner.width, inner.yMax - inner.y - 30), explainText, explainBody);
            GUI.matrix = keepMatrix; GUI.color = keepColour;
        }
        private float quitAskedAt = -1;
        /// <summary>The phone's "leave the game?" card, opened by Back on the front page: stay, or leave.</summary>
        private void DrawQuitAsk()
        {
            if (quitAskedAt < 0) return;
            Rect card = new Rect(440, 250, 400, 220); // the menu's 1280x720 space
            if (Event.current.type == EventType.MouseDown && !card.Contains(Event.current.mousePosition) && Time.unscaledTime - quitAskedAt > .1f) { quitAskedAt = -1; return; }
            Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
            LivingUI.Pop(Time.unscaledTime - quitAskedAt, card.center);
            if (ArcadeSkin.HasKit) ArcadeSkin.Frame(card, KitFrame.Dialogue, .5f); else RunHud.Fill(card, ArcadeSkin.Card);
            explainTitle ??= ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeBody), true);
            TextAnchor keepAlign = explainTitle.alignment; explainTitle.alignment = TextAnchor.MiddleCenter;
            LocalizedGUI.Label(new Rect(card.x + 24, card.y + 34, card.width - 48, 60), "Leave the Waiting Realm?", explainTitle);
            explainTitle.alignment = keepAlign;
            if (navigation.Button(new Rect(card.x + 30, card.yMax - 92, 160, 56), "STAY", primary)) quitAskedAt = -1;
            if (navigation.Button(new Rect(card.xMax - 190, card.yMax - 92, 160, 56), "LEAVE", button)) QuitGame();
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
            explainTitle ??= ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeBody), true);
            LocalizedGUI.Label(new Rect(card.x + pad, card.y + 10, w - pad * 2, 26), "Bring a keepsake", explainTitle);
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
                if (TouchTargets.Button(slot, GUIContent.none, GUIStyle.none))
                {
                    keepsakePick = id; keepsakeOpen = false; CheckpointAudio.PlayInterfaceId("ui.select");
                    Explain(keepsakeAt, relic != null ? relic.Name : "No keepsake", relic != null ? RelicDescriptions.Describe(relic, 1) + "\nKept from a Postbox letter; it rides along from the first wave." : "This run starts without a keepsake relic.");
                }
            }
            GUI.matrix = keepMatrix; GUI.color = keepColour;
        }
        // what a difficulty does, in numbers (requirement 25/10/2026: the poetic lines said nothing)
        private static string DifficultyLine(int d) => "Foes ×" + JourneyBalance.HealthFactor[Mathf.Clamp(d, 0, JourneyBalance.HealthFactor.Length - 1)].ToString("0.##") + " health · ×" + JourneyBalance.DamageFactor[Mathf.Clamp(d, 0, JourneyBalance.DamageFactor.Length - 1)].ToString("0.##") + " damage";

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
                if (TouchTargets.Button(nursery, GUIContent.none, GUIStyle.none)) Explain(nursery, "Cradle friends", "From the Lamp Tree's Cradle: one of these three friends can start the run at your side. They change every run. Tap one to bring it.");
                foreach (UnitDefinition friend in friends)
                {
                    Rect cell = new Rect(x, row.y, 44, 44); bool on = nurseryPick == friend.Id;
                    if (on) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Brings " + RunUICopy.UnitName(friend); }
                    ArcadeSkin.PortraitSlot(cell, on); RunHud.Sprite(new Rect(cell.x + 6, cell.y + 6, 32, 32), friend.Body);
                    if (TouchTargets.Button(cell, GUIContent.none, GUIStyle.none)) { nurseryPick = on ? null : friend.Id; CheckpointAudio.PlayInterfaceId("ui.select"); Explain(cell, RunUICopy.UnitName(friend), on ? "Left at home." : "Starts the run at your side."); }
                    x += 48;
                }
                x += 12;
            }
            if (coat != null)
            {
                Rect cell = new Rect(x, row.y, 44, 44);
                if (altCoat) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Starts with " + RunUICopy.UnitName(coat); }
                ArcadeSkin.PortraitSlot(cell, altCoat); RunHud.Sprite(new Rect(cell.x + 6, cell.y + 6, 32, 32), coat.Body);
                if (TouchTargets.Button(cell, GUIContent.none, GUIStyle.none)) { altCoat = !altCoat; CheckpointAudio.PlayInterfaceId("ui.select"); Explain(cell, "Second coat", (altCoat ? "Start with " + RunUICopy.UnitName(coat) + " instead of the usual first friend." : "Back to the usual first friend.") + " Earned in the Wardrobe by playing this hero."); }
                x += 56;
            }
            if (canHabit)
            {
                Rect cell = new Rect(x, row.y, 44, 44);
                if (habit) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = MetaProgression.HabitAdded(HeroKits.For(champion.Id)).Text.Replace("Second habit: ", "").TrimEnd('.'); }
                ArcadeSkin.PortraitSlot(cell, habit); RunHud.Sprite(new Rect(cell.x + 4, cell.y + 4, 36, 36), MetaIcons.Station(MetaStation.Wardrobe, true));
                if (TouchTargets.Button(cell, GUIContent.none, GUIStyle.none)) { habit = !habit; CheckpointAudio.PlayInterfaceId("ui.select"); Explain(cell, "Second habit", (habit ? MetaProgression.HabitAdded(HeroKits.For(champion.Id)).Text.Replace("Second habit: ", "Swaps one hero & friends bonus: ") : "The hero keeps its usual hero & friends bonus.") + " Earned in the Wardrobe."); }
                x += 56;
            }
            if (keeps.Count > 0)
            {
                // the Postbox: one keepsake relic to bring; a tap opens the picker with all of them (no cycling through)
                Rect cell = new Rect(x, row.y, 44, 44); bool on = keepsakePick != null;
                ItemDefinition relic = on ? catalog.Item(keepsakePick) : null;
                if (on) { ArcadeSkin.Glow(new Rect(cell.x - 10, cell.y - 10, 64, 64), new Color(1f, .8f, .45f, .6f)); caption = "Brings " + relic?.Name; }
                ArcadeSkin.PortraitSlot(cell, on); RunHud.Sprite(new Rect(cell.x + 5, cell.y + 5, 34, 34), relic?.Icon ?? MetaIcons.Station(MetaStation.Postbox, true));
                if (TouchTargets.Button(cell, GUIContent.none, GUIStyle.none)) { keepsakeOpen = !keepsakeOpen; keepsakeAt = cell; keepsakeOpenedAt = Time.unscaledTime; explainText = null; CheckpointAudio.PlayInterfaceId("ui.open"); }
            }
            if (caption != null) LocalizedGUI.Label(new Rect(row.x, row.yMax + 1, row.width, 22), caption, note);
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
                case Page.Records: return "Records"; // one name per place: the menu says RECORDS
                case Page.Achievements: return "DREAM JOURNAL";
                case Page.Museum: return "The Collection"; // the menu says COLLECTION
                default: return "Arcane Brigade";
            }
        }
        private static string PageEyebrow(Page page) => page == Page.Roster ? "THE WAITING PLACE · NEW JOURNEY" : page == Page.Records ? "THE WAITING PLACE · RECORDS" : page == Page.Museum ? "THE WAITING PLACE · COLLECTION" : "THE WAITING PLACE";
        /// <summary>A locked hero says what opens it and how far along you are, beside its card (release testing 3/11/2026: the
        /// short chip on the card alone was hard to read).</summary>
        private void ExplainLocked(UnitDefinition unit, Rect card) =>
            Explain(new Rect(65 + card.x, 180 + card.y - scroll.y, card.width, card.height), "Locked · " + RunUICopy.UnitName(unit),
                HeroUnlocks.Coming(unit) ? "Arrives in a later update." : HeroUnlocks.Full(profile, unit, catalog, false) + ".\n" + HeroUnlocks.ProgressLine(profile, unit, catalog));
        /// <summary>A hero's place in the grid: its index among the heroes that are shown.</summary>
        private int RosterSlot(int index) { int slot = 0; for (int i = 0; i < index && i < catalog.Champions.Length; i++) if (HeroUnlocks.Listed(catalog.Champions[i])) slot++; return slot; }
        public static Rect RosterCardBounds(int index) => new Rect(index % 3 * 210, index / 3 * 232, 200, 224); // two whole rows fit the 460-tall window (review 3/11/2026: the second row ran under the buttons)
        public static Sprite RosterPortrait(UnitDefinition unit, float time, int identity, bool focused, bool reducedMotion)
        {
            float curiosity = Mathf.Repeat(time + identity * .73f, 6.4f);
            CharacterActingEntry acting = CharacterActingLibrary.Current != null ? CharacterActingLibrary.Current.Find(unit.Id) : null;
            if (!reducedMotion && focused && curiosity < 1.3f && acting != null)
                return CharacterActingEntry.Frame(acting.Curious, curiosity / 1.3f);
            if (unit.AnimationFrames == null || unit.AnimationFrames.Length < 4) return unit.Body;
            bool stepping = focused && !reducedMotion && Mathf.Repeat(time + identity * 0.73f, 7f) > 6.2f;
            return unit.AnimationFrames[UnitVisual.Frame(unit, UnitVisual.AnimationFrame(time, identity, stepping, reducedMotion))] ?? unit.Body;
        }

        private void DrawSettings() => SettingsSheet.Draw(profile, Vector2.zero, Button, Label, SaveProfile, false, r => navigation.Button(r, "", GUIStyle.none));

        private void OnApplicationFocus(bool focus)
        {
            if (RunPaths.Diagnostic) return;
            focusLost = !focus; AudioListener.pause = focusLost || backgrounded;
        }
        private void OnApplicationPause(bool paused) { backgrounded = paused; AudioListener.pause = focusLost || backgrounded; }

        private void DrawRecords()
        {
            if (Button(new Rect(60, 620, 560, 52), "OPEN DREAM JOURNAL")) ShowPage(Page.Achievements);
            RecordTotals(new Rect(60, 150, 560, 92));
            scroll = ScrollPanel.Begin(new Rect(60, 250, 560, 350), scroll, new Rect(0, 0, 520, Mathf.Max(345, profile.History.Count * 106 + profile.Records.Count * 65 + profile.Achievements.FindAll(a => !a.StartsWith("journal-")).Count * 40 + 65)));
            float vertical = 0;
            for (int index = 0; index < profile.History.Count; index++)
            {
                RunRecord record = profile.History[index];
                if (Button(new Rect(14, vertical + 4, 486, 92), record.Nickname + "\n" + "Night " + record.Score + " · " + RunUICopy.UnitName(record.Build.ChampionId) + " · " + (record.Build.Endless ? "Endless" : "Journey"))) historySelection = index;
                vertical += 106; // two lines with room above and below inside the plate
            }
            if (profile.Records.Count > 0) { Label(new Rect(0, vertical, 510, 50), "Best by hero, mode and difficulty"); vertical += 60; }
            foreach (ScoreRecord record in profile.Records) { Label(new Rect(0, vertical, 510, 60), RunUICopy.RecordName(record.Key, catalog) + "\n" + "Night " + record.Score); vertical += 65; }
              foreach (string achievement in profile.Achievements) if (!achievement.StartsWith("journal-")) { Label(new Rect(0, vertical, 510, 40), "✓ " + RunUICopy.AchievementName(achievement)); vertical += 40; }
            ScrollPanel.End();
            if (profile.History.Count > 0)
            {
                RunRecord record = profile.History[Mathf.Clamp(historySelection, 0, profile.History.Count - 1)];
                // the summary is laid out on the 1080 canvas: drawn at two thirds on this 720 page (its title was cut)
                Matrix4x4 keepInvoice = GUI.matrix; GUI.matrix = keepInvoice * Matrix4x4.Scale(new Vector3(2f / 3f, 2f / 3f, 1));
                invoice.Draw(new Rect(990, 240, 818, 780), record.Build, catalog, record.Nickname, record.Score);
                GUI.matrix = keepInvoice;
            }
            else
            {
                // an empty page says why and shows what will fill it (review 31/10/2026): the empty letter rack
                if (ArcadeSkin.KitSprite("Info/EmptyLetters") is Sprite rack) RunHud.Sprite(new Rect(700, 180, 450, 296), rack);
                Label(new Rect(690, 490, 470, 90), "No journeys yet. Finish one and its letter waits here, with your brigade.");
            }
        }

        /// <summary>The totals on a paper card: best night, wins, foes and difficulties open, each a big number over its caption.</summary>
        private void RecordTotals(Rect r)
        {
            ArcadeSkin.Frame(r, KitFrame.Card, .3f);
            Rect c = ArcadeSkin.Content(r, KitFrame.Card, true, .3f);
            if (totalNumber == null)
            {
                totalNumber = ArcadeTypography.Style(28, false, true); totalNumber.alignment = TextAnchor.MiddleCenter;
                totalCaption = ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote)); totalCaption.alignment = TextAnchor.MiddleCenter; totalCaption.wordWrap = false;
            }
            totalNumber.normal.textColor = totalCaption.normal.textColor = ArcadeSkin.FrameText(KitFrame.Card);
            string[] numbers = { profile.BestWave.ToString(), profile.Wins.ToString(), profile.TotalKills.ToString("N0"), (RunMeta.UnlockedDifficulty(profile) + 1) + "/" + RunMeta.DifficultyNames.Length };
            string[] captions = { "Best night", "Wins", "Foes", "Difficulties" }; // sentence case at the standard size (15-unit capitals were unreadable on a phone)
            float w = c.width / numbers.Length;
            for (int i = 0; i < numbers.Length; i++)
            {
                Rect col = new Rect(c.x + i * w, c.y, w, c.height);
                LocalizedGUI.Label(new Rect(col.x, col.y, col.width, col.height * .6f), numbers[i], totalNumber);
                totalCaption.fontSize = ArcadeTypography.On720(ArcadeTypography.SizeNote);
                while (totalCaption.fontSize > 16 && totalCaption.CalcSize(LocalizedGUI.Content(captions[i])).x > col.width - 8) totalCaption.fontSize--;
                LocalizedGUI.Label(new Rect(col.x, col.y + col.height * .58f, col.width, col.height * .4f), captions[i], totalCaption);
                if (i > 0) ArcadeSkin.Diamond(new Vector2(col.x, col.center.y), 5, new Color(.55f, .36f, .18f, .7f)); // a small brass stud between columns, not a ruled line
            }
        }
        private GUIStyle totalNumber, totalCaption;

        private bool SaveProfile()
        {
            try { storage.SaveProfile(profile); error = ""; return true; }
            catch (Exception exception) { Debug.LogWarning(exception); error = "Settings could not be saved. Try again."; CheckpointAudio.PlayInterfaceId("ui.error"); return false; }
        }
    }
}
