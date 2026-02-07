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
            for (int i = 0; i < zonePanels.Length; i++) zonePanels[i] = Resources.Load<Texture2D>("MienCho/ActOne/" + ActOneCanon.ZonePanel[i]);
        }
        private void Update()
        {
            if (owner?.Session == null) return;
            if (open)
            {
                if(InputConsumedFrame==Time.frameCount)return;
                if (!Application.isFocused && !RunPaths.Diagnostic) return;
                if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Choose(0);
                else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.JoystickButton3)) Choose(1);
                else if (Input.GetKeyDown(KeyCode.JoystickButton0) && ActOneCanon.ChoiceOptions[chapter] != null && page == 0 && Memory.Choices[chapter] == 0) Choose(0);
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
            if (unread >= 0 && (!RunPaths.Diagnostic || MienChoCapture.AllowAutomaticScenes)) Show(unread);
        }
        public void Show(int index)
        {
            if (owner?.Simulation == null || owner.Session.SecretRoomOpen || owner.TransitionPending || owner.HallExitPending || owner.PresentationPaused && !open || index < 0 || index >= MienChoStory.Available(owner.Session.State)) return;
            chapter = index; page = Memory.Pages[index]; lives = false; scroll = Vector2.zero; open = true; Consume();
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
            UnityEngine.SceneManagement.SceneManager.LoadScene("WaitingRoom"); return true;
        }
        private void SetPage(int value)
        {
            int previous = Memory.Pages[chapter]; Memory.Pages[chapter] = value;
            if (owner.SaveNightmareBoundary()) page = value;
            else Memory.Pages[chapter] = previous;
        }
        private void Previous() { Consume(); SetPage(MienChoStory.PreviousPage(chapter, page, Memory.Choices[chapter])); scroll = Vector2.zero; }
        /// <summary>Commits a story choice once; the branch page follows and the choice is saved with the journal.</summary>
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
        }
        public void Close()
        {
            if (!open) return;
            Consume();
            var state = owner.Session.State;
            state.MienCho ??= new MienChoMemory();
            int before = state.MienCho.ReadMask;
            state.MienCho.ReadMask |= 1 << chapter;
            if (!owner.SaveNightmareBoundary()) { state.MienCho.ReadMask = before; return; }
            open = false; owner.SetStoryOpen(false);
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
                GUI.matrix = old; return;
            }
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(.015f, .023f, .043f, 1));
            if (lives) { DrawLives(); GUI.matrix = old; return; }
            var c = MienChoStory.For(owner.Session.State)[chapter];
            int zone = System.Array.IndexOf(ActOneCanon.ZonePanel, c.Image);
            var image = zone >= 0 && zonePanels[zone] != null ? zonePanels[zone] : panels[zone == 2 || zone == 3 ? 1 : c.Image == "OpenDawn" ? 2 : 0];
            if (image != null) GUI.DrawTexture(new Rect(60, 150, 720, 540), image, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(70, 55, 1780, 75), ActOneCanon.ZoneNames[ActOneCanon.Zone(c.Wave)] + " / " + c.Title, title);
            int burning = ActOneCanon.BreakWave.Length - ActOneCanon.Broken(owner.Session.State.Wave, owner.Session.State.ActOne?.RelitMask ?? 0);
            GUI.Label(new Rect(80, 785, 680, 120), "EVIDENCE\n" + c.Evidence + "\n" + burning + " OF 8 LANTERNS STILL BURNING", small);
            if (Press(new Rect(80, 710, 680, 65), "THE PEOPLE TRAVELLING WITH YOU")) ShowLives();
            GUI.Label(new Rect(850, 145, 990, 50), "CHAPTER " + (chapter + 1) + " / PAGE " + (page + 1) + " OF " + c.Pages.Length, small);
            string text = c.Pages[page] + ActOneCanon.PageExtra(owner.Session.State, chapter, page);
            if (chapter == 0 && page == 0) text += "\n\nYOUR ARRIVAL\n" + WaitingArrivals.For(owner.Session.State.ChampionId);
            float height = Mathf.Max(650, copy.CalcHeight(new GUIContent(text), 940) + 30);
            scroll = GUI.BeginScrollView(new Rect(850, 210, 1000, 660), scroll, new Rect(0, 0, 955, height));
            GUI.Label(new Rect(0, 0, 940, height), text, copy); GUI.EndScrollView();
            for (int i = 0; i < MienChoStory.Available(owner.Session.State); i++)
                if (Press(new Rect(70 + i % 7 * 97, 900 + i / 7 * 56, 88, 46), (i + 1) + ((Memory.FinishedMask & 1 << i) != 0 ? " ✓" : ""))) SelectChapter(i);
            if (Press(new Rect(850, 925, 200, 65), "PREVIOUS")) Previous();
            string[] options = ActOneCanon.ChoiceOptions[chapter];
            if (options != null && page == 0 && Memory.Choices[chapter] == 0)
            {
                // the story waits for the traveller: two choices instead of CONTINUE
                if (Press(new Rect(1070, 925, 240, 65), options[0])) Choose(0);
                if (Press(new Rect(1320, 925, 240, 65), options[1])) Choose(1);
            }
            else if (HobWaits)
            {
                if (Press(new Rect(1070, 925, 490, 65), "GO TO HOB  [ENTER]")) EnterHobRoom();
            }
            else
            {
                int next = MienChoStory.NextPage(chapter, page, Memory.Choices[chapter]);
                if (Press(new Rect(1070, 925, 490, 65), next >= c.Pages.Length ? "KEEP THE EVIDENCE / CLOSE" : "CONTINUE  [ENTER]")) Advance();
            }
            if (Press(new Rect(1580, 925, 270, 65), "READ LATER  [ESC]")) Close();
            GUI.Label(new Rect(850, 1000, 1000, 65), "Paused / page saved. Arrows: scroll / LB-RB: pages / X: companions / PgUp-PgDn: entries / 1-2 or A-Y: choose.", small);
            GUI.matrix = old;
        }
        private bool Press(Rect rectangle, string text)
        {
            ArcadeSkin.Plate(rectangle, ArcadeSkin.Teal);
            return GUI.Button(rectangle, text, button);
        }
        public void ShowLives() { if (!open) return; lives = true; scroll = Vector2.zero; Consume(); }
        public void DrawDawn(Rect rectangle)
        {
            if (panels[2] != null) GUI.DrawTexture(rectangle, panels[2], ScaleMode.ScaleToFit);
            var ids = new System.Collections.Generic.List<string> { owner.Session.State.ChampionId };
            foreach (var ally in owner.Session.State.Roster) ids.Add(ally.UnitId);
            GUI.BeginGroup(rectangle);
            int columns = Mathf.Min(8, ids.Count);
            float width = Mathf.Min(112, (rectangle.width - 40) / Mathf.Max(1, columns));
            for (int i = 0; i < ids.Count; i++)
            {
                var unit = owner.Content.Unit(ids[i]);
                if (unit == null) continue;
                Sprite frame = unit.AnimationFrames.Length == 0 ? unit.Body : unit.AnimationFrames[PresentationSettings.ReducedMotion ? 0 : Mathf.FloorToInt(owner.PresentationClock * 1.8f + i) % unit.AnimationFrames.Length];
                float x = (rectangle.width - columns * width) * .5f + i % columns * width;
                float y = rectangle.height - 125 - i / columns * 108;
                RunHud.Sprite(new Rect(x, y, width, 105), frame);
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
                RunHud.Sprite(new Rect(100, 220, 560, 470), unit.Body);
                GUI.Label(new Rect(100, 730, 650, 120), (story >= 0 ? ActOneCanon.StoryNames[story] : stranger ? "???" : unit.DisplayName) + "\n" + (creature + 1) + " / " + ids.Count, title);
                string text = stranger ? "A STRANGER\nYou sold your memory of them to the Last Customer. They still travel with you. You do not know who they are.\n\nThey introduce themselves again at every rest. They are kind about it."
                    : story >= 0 ? "TRAVELLING WITH YOU\n" + CreatureLives.Arc(id, state.Wave)
                    : "LIFE IN THE WAITING REALM\n" + CreatureLives.Origin(id) + "\n\nALONG THE CROSSING\n" + CreatureLives.Arc(id, owner.Session.State.Wave);
                float height = Mathf.Max(650, copy.CalcHeight(new GUIContent(text), 940) + 30);
                scroll = GUI.BeginScrollView(new Rect(850, 180, 1000, 690), scroll, new Rect(0, 0, 955, height));
                GUI.Label(new Rect(0, 0, 940, height), text, copy); GUI.EndScrollView();
                if (Press(new Rect(100, 900, 260, 65), "PREVIOUS FRIEND")) { creature = (creature + ids.Count - 1) % ids.Count; scroll = Vector2.zero; }
                if (Press(new Rect(390, 900, 260, 65), "NEXT FRIEND")) { creature = (creature + 1) % ids.Count; scroll = Vector2.zero; }
            }
            if (Press(new Rect(900, 925, 450, 65), "RETURN TO EVIDENCE")) { lives = false; scroll = Vector2.zero; }
            if (Press(new Rect(1400, 925, 450, 65), "CLOSE JOURNAL  [ESC]")) Close();
        }
    }
    public sealed partial class ArenaController
    {
        public bool StoryOpen => GetComponent<MienChoPresentation>()?.Open == true || GetComponent<WaitingJourney>()?.Open == true;
        public bool StoryConsumesInput => StoryOpen || GetComponent<MienChoPresentation>()?.InputConsumedFrame == Time.frameCount || GetComponent<WaitingJourney>()?.InputConsumedFrame == Time.frameCount;
        public void SetStoryOpen(bool value) { pauseGate.Story = value; ApplyPause(); }
    }
}
