using UnityEngine;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(-60)]
    public sealed class MienChoPresentation : MonoBehaviour
    {
        private ArenaController owner;
        private int chapter, page;
        private bool open;
        private bool lives;
        private int creature;
        private GUIStyle title, copy, button, small;
        private Vector2 scroll;
        private readonly Texture2D[] panels = new Texture2D[3];
        private readonly Texture2D[] zonePanels = new Texture2D[4];
        public bool Open => open;
        public int Chapter => chapter;
        public int Page => page;
        public int InputConsumedFrame { get; private set; } = -1;
        private float nextNavigation;
        // Story pages no longer take over the screen after every wave: a sealed note says a page is waiting, and the
        // traveller opens the note or the journal cabinet when they want to. Individual pages retain their reading position.
        private int viewedMask;
        private bool arrivalOpen;
        private int noticedUnread = -1;
        private float noticeAt = -100;
        private GUIStyle noteHand, noteKey;
        /// <summary>Profile setting: story pages open by themselves (old behaviour) or only announce themselves (default).</summary>
        public bool AutoOpen => owner?.Profile != null && owner.Profile.StoryPagesAutoOpen;
        /// <summary>Chapters the traveller can read now and has not opened yet.</summary>
        public int Unread
        {
            get
            {
                if (owner?.Session == null || owner.Session.State.Endless) return 0;
                int read = owner.Session.State.MienCho?.ReadMask ?? 0, count = 0;
                for (int i = 0; i < MienChoStory.Available(owner.Session.State); i++) if ((read & (1 << i)) == 0) count++;
                return count;
            }
        }
        /// <summary>Where the "new page" note sits on the 1920x1080 HUD (under the wallet and the lantern row).</summary>
        public static readonly Rect NoticeRect = new Rect(22, 232, 248, 56);
        private bool NoticeAllowed => !open && owner?.Session != null && owner.Simulation != null && Unread > 0 && !owner.PresentationPaused && !owner.TransitionPending && !owner.HallExitPending && !owner.Session.SecretRoomOpen && !owner.NightmareEndingVisible && !owner.Details.Open && !owner.MenuConfirmation
            && (owner.Session.State.Phase == SessionPhase.Shop || owner.Session.State.Phase == SessionPhase.Combat);
        public bool NoticeVisible => NoticeAllowed;
        private MienChoMemory Memory
        {
            get
            {
                owner.Session.State.MienCho ??= new MienChoMemory();
                if (owner.Session.State.MienCho.Pages == null || owner.Session.State.MienCho.Pages.Length == 0) owner.Session.State.MienCho.Pages = new int[MienChoStory.For(owner.Session.State).Length];
                if (owner.Session.State.MienCho.Choices == null || owner.Session.State.MienCho.Choices.Length == 0) owner.Session.State.MienCho.Choices = new int[MienChoStory.For(owner.Session.State).Length];
                return owner.Session.State.MienCho;
            }
        }
        public void Configure(ArenaController value)
        {
            owner = value;
            panels[0] = Resources.Load<Texture2D>("MienCho/WithheldDawn");
            panels[1] = Resources.Load<Texture2D>("MienCho/WaitingRoom");
            panels[2] = Resources.Load<Texture2D>("MienCho/OpenDawn");
            panels[2] = Resources.Load<Texture2D>("MienCho/ActOne/Panel_Dawn") ?? panels[2];
            for (int i = 0; i < zonePanels.Length; i++) zonePanels[i] = Resources.Load<Texture2D>("MienCho/ActOne/" + ActOneCanon.ZonePanel[i]);
        }
        private void Update()
        {
            if (owner?.Session == null) return;
            if (open)
            {
                if(InputConsumedFrame==Time.frameCount)return;
                if (!Application.isFocused && !RunPaths.Diagnostic) return;
                // a choice is picked first and confirmed (requirement 22/10/2026)
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) storyPick = 0;
                else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.JoystickButton3)) storyPick = 1;
                else if ((Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.Return)) && storyPick >= 0 && ActOneCanon.ChoiceOptions[chapter] != null && page == 0 && Memory.Choices[chapter] == 0) { Choose(storyPick); storyPick = -1; }
                else if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) && HobWaits) EnterHobRoom();
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) Advance(); // a waiting choice is never taken by Enter
                else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.JoystickButton7)) { if (lives) { lives = false; scroll = Vector2.zero; Consume(); } else Close(); }
                else if (Input.GetKeyDown(KeyCode.JoystickButton2)) { lives = !lives; scroll = Vector2.zero; Consume(); }
                else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton4)) { if (lives) { creature = Mathf.Max(0, creature - 1); scroll = Vector2.zero; Consume(); } else Previous(); }
                else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton5)) { if (lives) { creature++; scroll = Vector2.zero; Consume(); } else Advance(); }
                Vector2 pad = ArcadeNavigation.Pad;
                float vertical = Mathf.Abs(pad.y) > .2f ? pad.y : 0;
                if (Input.GetKey(KeyCode.DownArrow)) vertical = -1;
                if (Input.GetKey(KeyCode.UpArrow)) vertical = 1;
                scroll.y = Mathf.Max(0, scroll.y - vertical * Time.unscaledDeltaTime * 440);
                if (Time.unscaledTime >= nextNavigation && (Input.GetKeyDown(KeyCode.PageDown) || Input.GetKeyDown(KeyCode.PageUp)))
                { SelectChapter(chapter + (Input.GetKeyDown(KeyCode.PageDown) ? 1 : -1)); nextNavigation = Time.unscaledTime + .2f; }
                return;
            }
            if (owner.PresentationPaused || owner.TransitionPending || owner.Session.SecretRoomOpen || owner.NightmareEndingVisible) return;
            if (owner.Session.State.Phase == SessionPhase.Victory || owner.Session.State.Phase == SessionPhase.Defeat) return;
            int unread = MienChoStory.NextUnread(owner.Session.State);
            int waiting = Unread;
            if (waiting > noticedUnread && noticedUnread >= 0 && !AutoOpen) { noticeAt = Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.page"); }
            noticedUnread = waiting;
            // The arrival may open once, at the very first run start, and closes on the first Esc; everything else waits
            // for the traveller unless they asked for pages to open automatically.
            bool arrival = unread == 0 && owner.Session.State.Wave == 1 && owner.Profile != null && !owner.Profile.StoryArrivalShown;
            if (unread >= 0 && (AutoOpen || arrival) && (!RunPaths.Diagnostic || MienChoCapture.AllowAutomaticScenes))
            {
                Show(unread);
                if (open && arrival) { arrivalOpen = true; RememberArrival(); }
                return;
            }
        }
        private void RememberArrival()
        {
            if (owner.Profile == null || owner.Profile.StoryArrivalShown) return;
            owner.Profile.StoryArrivalShown = true;
            if (RunPaths.Diagnostic) return;
            try { new RunStorage(RunPaths.Root).SaveProfile(owner.Profile); } catch (System.Exception exception) { Debug.LogWarning("Arrival flag not saved: " + exception.Message); }
        }
        /// <summary>Opens the oldest unread page, or the newest chapter when everything has been read. Returns true if open.</summary>
        public bool OpenNext()
        {
            if (owner?.Session == null || open) return open;
            if (owner.Details.Open || owner.MenuConfirmation || owner.NightmareEndingVisible) return false;
            if (owner.Session.State.Phase != SessionPhase.Shop && owner.Session.State.Phase != SessionPhase.Combat) return false;
            int unread = MienChoStory.NextUnread(owner.Session.State);
            Show(unread >= 0 ? unread : Mathf.Max(0, MienChoStory.Available(owner.Session.State) - 1));
            return open;
        }
        public void Show(int index)
        {
            if (owner?.Simulation == null || owner.Session.SecretRoomOpen || owner.TransitionPending || owner.HallExitPending || owner.PresentationPaused && !open || index < 0 || index >= MienChoStory.Available(owner.Session.State)) return;
            chapter = index; page = Memory.Pages[index]; lives = false; scroll = Vector2.zero; open = true; Consume();
            viewedMask = 1 << index; arrivalOpen = false;
            owner.SetStoryOpen(true);
            CheckpointAudio.PlayInterfaceId("ui.page");
        }
        public void Advance()
        {
            if (!open) return;
            Consume();
            if (lives) { lives = false; scroll = Vector2.zero; return; }
            int next = MienChoStory.NextPage(chapter, page, Memory.Choices[chapter]);
            if (next < 0) return; // a choice is waiting on this page
            if (HobWaits) return;  // the story goes on once everyone has pulled Hob up
            if (next >= MienChoStory.For(owner.Session.State)[chapter].Pages.Length)
            {
                int finished = Memory.FinishedMask; Memory.FinishedMask |= 1 << chapter;
                if (!owner.SaveNightmareBoundary()) { Memory.FinishedMask = finished; return; }
                Close(); return;
            }
            SetPage(next);
            scroll = Vector2.zero; CheckpointAudio.PlayInterfaceId("ui.page");
        }
        private void Consume() => InputConsumedFrame = Time.frameCount;
        private bool HobWaits => chapter == ActOneCanon.HobChapter && page == 0 && owner.Session.HobRescueNeeded;
        /// <summary>Walks into the perfect room, where Hob is sitting (the walkable room scene, rescue mode).</summary>
        public bool EnterHobRoom()
        {
            var state = owner.Session.State;
            if (!owner.Session.HobRescueNeeded || owner.TransitionPending) return false;
            WaitingCampaign.Ensure(state); state.WaitingCampaign.Room = 2;
            if (!owner.SaveNightmareBoundary()) { state.WaitingCampaign.Room = 0; return false; }
            open = false; owner.SetStoryOpen(false);
            SceneDissolve.Load("WaitingRoom"); return true;
        }
        private void SetPage(int value)
        {
            int previous = Memory.Pages[chapter]; Memory.Pages[chapter] = value;
            if (owner.SaveNightmareBoundary()) page = value;
            else Memory.Pages[chapter] = previous;
        }
        private void Previous() { Consume(); SetPage(MienChoStory.PreviousPage(chapter, page, Memory.Choices[chapter])); scroll = Vector2.zero; }
        /// <summary>Commits a story choice once; the branch page follows and the choice is saved with the journal.</summary>
        private int storyPick = -1;
        public void Choose(int option)
        {
            if (!open || ActOneCanon.ChoiceOptions[chapter] == null || Memory.Choices[chapter] != 0 || page != 0) return;
            Consume();
            Memory.Choices[chapter] = option + 1;
            int previous = Memory.Pages[chapter]; Memory.Pages[chapter] = option + 1;
            if (!owner.SaveNightmareBoundary()) { Memory.Choices[chapter] = 0; Memory.Pages[chapter] = previous; return; }
            page = option + 1; scroll = Vector2.zero; CheckpointAudio.PlayInterfaceId("ui.confirm");
        }
        public void SelectChapter(int index)
        {
            if (!open) return;
            chapter = Mathf.Clamp(index, 0, MienChoStory.Available(owner.Session.State) - 1);
            page = Memory.Pages[chapter]; lives = false; scroll = Vector2.zero; Consume();
            viewedMask |= 1 << chapter;
        }
        public void Close()
        {
            if (!open) return;
            Consume();
            var state = owner.Session.State;
            state.MienCho ??= new MienChoMemory();
            int before = state.MienCho.ReadMask;
            int available = MienChoStory.Available(state);
            state.MienCho.ReadMask |= (viewedMask | 1 << chapter) & (available >= 31 ? int.MaxValue : (1 << available) - 1);
            if (!owner.SaveNightmareBoundary()) { state.MienCho.ReadMask = before; return; }
            open = false; arrivalOpen = false; viewedMask = 0; noticedUnread = Unread; owner.SetStoryOpen(false);
        }
        public void Draw(Camera camera)
        {
            if (owner?.Session == null) return;
            Matrix4x4 old = ArcadeTypography.BeginViewport(camera.pixelRect, 1920, 1080);
            if (title == null)
            {
                title = ArcadeTypography.Style(36, true); copy = ArcadeTypography.Style(27); copy.wordWrap = true;
                button = ArcadeTypography.Style(22, true); button.alignment = TextAnchor.MiddleCenter;
                small = ArcadeTypography.Style(20); small.wordWrap = true;
            }
            if (!open)
            {
                if (NoticeAllowed) DrawNotice();
                GUI.matrix = old; return;
            }
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(.015f, .023f, .043f, 1));
            if (lives) { DrawLives(); GUI.matrix = old; return; }
            var c = MienChoStory.For(owner.Session.State)[chapter];
            int zone = System.Array.IndexOf(ActOneCanon.ZonePanel, c.Image);
            var image = zone >= 0 && zonePanels[zone] != null ? zonePanels[zone] : panels[zone == 2 || zone == 3 ? 1 : c.Image == "OpenDawn" ? 2 : 0];
            if (image != null) GUI.DrawTexture(new Rect(60, 150, 720, 540), image, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(70, 55, 1780, 75), ActOneCanon.ZoneNames[ActOneCanon.Zone(c.Wave)] + " / " + c.Title, title);
            // the lanterns still burning are shown as the HUD's own lantern row, not counted out in words
            string evidenceArt = chapter switch { 1 or 18 or 19 => "Item_Receipt", 12 => "Item_SewnLetter", 15 => "Item_Umbrella", 16 => "Item_ExitKey", _ => null };
            if (chapter == ActOneCanon.HobChapter && page == 0) evidenceArt = null;
            Sprite evidence = ActOneArtwork.Picture(evidenceArt);
            if (evidence != null) RunHud.Sprite(new Rect(75, 785, 125, 105), evidence);
            GUI.Label(new Rect(evidence != null ? 220 : 80, 785, evidence != null ? 550 : 680, 80), "EVIDENCE\n" + c.Evidence, small);
            NightHud.Lanterns(new Rect(evidence != null ? 220 : 80, 862, 230, 30), owner.Session.State);
            if (Press(new Rect(80, 710, 680, 65), "THE PEOPLE TRAVELLING WITH YOU")) ShowLives();
            GUI.Label(new Rect(850, 145, 990, 50), "CHAPTER " + (chapter + 1) + " / PAGE " + (page + 1) + " OF " + c.Pages.Length, small);
            string text = c.Pages[page] + ActOneCanon.PageExtra(owner.Session.State, chapter, page);
            if (chapter == 0 && page == 0) text += "\n\nYOUR ARRIVAL\n" + WaitingArrivals.For(owner.Session.State.ChampionId);
            float height = Mathf.Max(650, copy.CalcHeight(new GUIContent(text), 940) + 30);
            string[] options = ActOneCanon.ChoiceOptions[chapter];
            bool choosing = options != null && page == 0 && Memory.Choices[chapter] == 0;
            scroll = ScrollPanel.Begin(new Rect(850, 210, 1000, choosing ? 400 : 660), scroll, new Rect(0, 0, 955, height));
            GUI.Label(new Rect(0, 0, 940, height), text, copy); ScrollPanel.End();
            for (int i = 0; i < MienChoStory.Available(owner.Session.State); i++)
            {
                Rect entry = new Rect(70 + i % 7 * 97, 900 + i / 7 * 56, 88, 46);
                if (Press(entry, (i + 1) + ((Memory.FinishedMask & 1 << i) != 0 ? " ✓" : ""))) SelectChapter(i);
                // a page not opened yet still carries its wax seal
                if (((owner.Session.State.MienCho?.ReadMask ?? 0) & (1 << i)) == 0 && (viewedMask & (1 << i)) == 0) ArcadeSkin.Seal(new Vector2(entry.xMax - 8, entry.y + 6), 18, null);
            }
            if (Press(new Rect(850, 925, 200, 65), "PREVIOUS")) Previous();
            if (choosing)
            {
                // the story waits for the traveller: two choices as cards, picked then confirmed (requirement 22/10/2026)
                const string meaning = "The next page follows this choice. It shapes the story only; your brigade is not changed.";
                int taken = ChoiceCards.Draw(new Rect(850, 625, 1000, 285), options, new[] { meaning, meaning }, ref storyPick);
                if (taken >= 0) { Choose(taken); storyPick = -1; }
            }
            else if (HobWaits)
            {
                if (Press(new Rect(1070, 925, 490, 65), "GO TO HOB")) EnterHobRoom();
            }
            else
            {
                int next = MienChoStory.NextPage(chapter, page, Memory.Choices[chapter]);
                if (Press(new Rect(1070, 925, 490, 65), next >= c.Pages.Length ? "KEEP THE EVIDENCE / CLOSE" : "CONTINUE")) Advance();
            }
            if (Press(new Rect(1580, 925, 270, 65), arrivalOpen ? "SKIP" : "READ LATER")) Close();
            GUI.Label(new Rect(850, 1000, 1000, 65), arrivalOpen ? "The journal keeps this page: open the sealed note any time." : "Paused. This page is saved.", small);
            GUI.matrix = old;
        }
        /// <summary>The non-blocking "a new page in the journal" note: a handwritten ticket with a wax seal holding the
        /// number of unread pages. Clicking it (or J) opens the oldest unread page; play is never interrupted.</summary>
        private void DrawNotice()
        {
            int count = Unread;
            if (noteHand == null)
            {
                noteHand = ArcadeTypography.Style(27); noteHand.font = ArcadeTypography.Hand; noteHand.alignment = TextAnchor.MiddleLeft; noteHand.wordWrap = false;
                noteKey = ArcadeTypography.Style(20, true); noteKey.alignment = TextAnchor.MiddleCenter; noteKey.wordWrap = false;
            }
            float age = Time.unscaledTime - noticeAt;
            bool still = PresentationSettings.ReducedMotion;
            // a fresh page slides in from the left and settles; afterwards the note keeps very still
            float slide = still ? 0 : Mathf.Pow(1 - Mathf.Clamp01(age / 0.45f), 3) * -80;
            float sway = still || age > 2.4f ? 0 : Mathf.Sin(age * 7f) * Mathf.Clamp01(1 - age / 2.4f) * 2.2f;
            Rect note = new Rect(NoticeRect.x + slide, NoticeRect.y, NoticeRect.width, NoticeRect.height);
            Matrix4x4 before = GUI.matrix;
            if (sway != 0) ArcadeSkin.TurnAround(sway, new Vector2(note.x + 12, note.y + 6));
            ArcadeSkin.Ticket(note, ArcadeSkin.Paper, ArcadeSkin.Wax);
            noteHand.normal.textColor = new Color(.25f, .19f, .14f);
            GUI.Label(new Rect(note.x + 16, note.y + 2, note.width - 64, 34), count > 1 ? "new pages" : "a new page", noteHand);
            noteKey.normal.textColor = new Color(.25f, .19f, .14f, .8f);
            noteKey.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(note.x + 18, note.y + 32, note.width - 64, 18), "TAP TO READ", noteKey);
            ArcadeSkin.Seal(new Vector2(note.xMax - 26, note.y + 26), 38, count > 99 ? "99+" : count.ToString());
            GUI.matrix = before;
            if (GUI.Button(note, GUIContent.none, GUIStyle.none)) OpenNext();
        }
        private bool Press(Rect rectangle, string text)
        {
            return ArcadeSkin.Button(rectangle, text, KitButton.Secondary, GUI.enabled, button); // painted, like every button
        }
        public void ShowLives() { if (!open) return; lives = true; scroll = Vector2.zero; Consume(); }
        public void DrawDawn(Rect rectangle)
        {
            if (panels[2] != null) GUI.DrawTexture(rectangle, panels[2], ScaleMode.ScaleToFit);
            var cast = new System.Collections.Generic.List<UnitDefinition> { owner.Content.Unit(owner.Session.State.ChampionId) };
            cast.AddRange(EndingTravellers.Story(owner));
            foreach (var ally in owner.Session.ActiveRoster) cast.Add(owner.Content.Unit(ally.UnitId));
            GUI.BeginGroup(rectangle);
            int columns = Mathf.Min(8, cast.Count);
            float width = Mathf.Min(112, (rectangle.width - 40) / Mathf.Max(1, columns));
            int rows = Mathf.Max(1, Mathf.CeilToInt(cast.Count / (float)columns));
            float rowHeight = Mathf.Min(108, Mathf.Max(20, rectangle.height - 55) / rows);
            float actorSize = Mathf.Min(width,Mathf.Min(105,rowHeight));
            for (int i = 0; i < cast.Count; i++)
            {
                var unit = cast[i];
                if (unit == null) continue;
                Sprite frame = EndingTravellers.Frame(unit, owner.PresentationClock, i, false);
                float x = (rectangle.width - columns * width) * .5f + i % columns * width;
                float y = rectangle.height - 20 - actorSize - i / columns * rowHeight;
                RunHud.Sprite(new Rect(x + (width-actorSize)*.5f, y, actorSize, actorSize), frame);
            }
            GUI.EndGroup();
        }
        private void DrawLives()
        {
            var ids = new System.Collections.Generic.List<string>();
            var state = owner.Session.State;
            for (int i = 0; i < ActOneCanon.StoryIds.Length; i++) if (ActOneCanon.StoryPresent(i, state.Wave, state.Phase == SessionPhase.Shop, state.Endless)) ids.Add("story:" + i);
            foreach (var entry in owner.Session.State.Roster) if (!ids.Contains(entry.UnitId)) ids.Add(entry.UnitId);
            foreach (var entry in owner.Session.State.AcquiredVariants) if (!ids.Contains(entry.UnitId)) ids.Add(entry.UnitId);
            GUI.Label(new Rect(70, 55, 1780, 75), "MIEN CHO / LIVES ALONGSIDE YOURS", title);
            if (ids.Count == 0) GUI.Label(new Rect(100, 250, 1500, 200), "Every creature you meet has a life before and after this crossing. Recruit a companion to learn their origin here.", copy);
            else
            {
                creature = Mathf.Clamp(creature, 0, ids.Count - 1);
                var id = ids[creature];
                int story = id.StartsWith("story:") ? int.Parse(id.Substring(6)) : -1;
                if (story >= 0) id = ActOneCanon.StoryIds[story];
                var unit = owner.Content.Unit(id);
                bool stranger = story < 0 && owner.Session.Stranger(id);
                RunHud.Sprite(new Rect(100, 220, 560, 470), story >= 0 ? ActOneArtwork.Picture(ActOneCanon.StoryNames[story]) ?? unit.Body : unit.Body);
                GUI.Label(new Rect(100, 730, 650, 120), (story >= 0 ? ActOneCanon.StoryNames[story] : stranger ? "???" : unit.DisplayName) + "\n" + (creature + 1) + " / " + ids.Count, title);
                string text = stranger ? "A STRANGER\nYou sold your memory of them to the Last Customer. They still travel with you. You do not know who they are.\n\nThey introduce themselves again at every rest. They are kind about it."
                    : story >= 0 ? "TRAVELLING WITH YOU\n" + CreatureLives.Arc(id, state)
                    : "LIFE IN THE WAITING REALM\n" + CreatureLives.Origin(id) + "\n\nALONG THE CROSSING\n" + CreatureLives.Arc(id, owner.Session.State);
                float height = Mathf.Max(650, copy.CalcHeight(new GUIContent(text), 940) + 30);
                scroll = ScrollPanel.Begin(new Rect(850, 180, 1000, 690), scroll, new Rect(0, 0, 955, height));
                GUI.Label(new Rect(0, 0, 940, height), text, copy); ScrollPanel.End();
                if (Press(new Rect(100, 900, 260, 65), "PREVIOUS FRIEND")) { creature = (creature + ids.Count - 1) % ids.Count; scroll = Vector2.zero; }
                if (Press(new Rect(390, 900, 260, 65), "NEXT FRIEND")) { creature = (creature + 1) % ids.Count; scroll = Vector2.zero; }
            }
            if (Press(new Rect(900, 925, 450, 65), "RETURN TO EVIDENCE")) { lives = false; scroll = Vector2.zero; }
            if (Press(new Rect(1400, 925, 450, 65), "CLOSE JOURNAL")) Close();
        }
    }
    public sealed partial class ArenaController
    {
        public bool StoryOpen => GetComponent<MienChoPresentation>()?.Open == true || GetComponent<WaitingJourney>()?.Open == true;
        public bool StoryConsumesInput => StoryOpen || GetComponent<MienChoPresentation>()?.InputConsumedFrame == Time.frameCount || GetComponent<WaitingJourney>()?.InputConsumedFrame == Time.frameCount;
        public void SetStoryOpen(bool value) { pauseGate.Story = value; ApplyPause(); }
    }
}
