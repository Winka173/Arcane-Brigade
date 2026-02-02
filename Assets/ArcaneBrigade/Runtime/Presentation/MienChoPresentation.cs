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
                return owner.Session.State.MienCho;
            }
        }
        public void Configure(ArenaController value)
        {
            owner = value;
            panels[0] = Resources.Load<Texture2D>("MienCho/WithheldDawn");
            panels[1] = Resources.Load<Texture2D>("MienCho/WaitingRoom");
            panels[2] = Resources.Load<Texture2D>("MienCho/OpenDawn");
        }
        private void Update()
        {
            if (owner?.Session == null) return;
            if (open)
            {
                if(InputConsumedFrame==Time.frameCount)return;
                if (!Application.isFocused && !RunPaths.Diagnostic) return;
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0)) Advance();
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
            if (page + 1 >= MienChoStory.For(owner.Session.State)[chapter].Pages.Length)
            {
                int finished = Memory.FinishedMask; Memory.FinishedMask |= 1 << chapter;
                if (!owner.SaveNightmareBoundary()) { Memory.FinishedMask = finished; return; }
                Close(); return;
            }
            SetPage(page + 1);
            scroll = Vector2.zero; CheckpointAudio.PlayInterfaceId("ui.page");
        }
        private void Consume() => InputConsumedFrame = Time.frameCount;
        private void SetPage(int value)
        {
            int previous = Memory.Pages[chapter]; Memory.Pages[chapter] = value;
            if (owner.SaveNightmareBoundary()) page = value;
            else Memory.Pages[chapter] = previous;
        }
        private void Previous() { Consume(); SetPage(Mathf.Max(0, page - 1)); scroll = Vector2.zero; }
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
            var image = panels[c.Image == "WaitingRoom" ? 1 : c.Image == "OpenDawn" ? 2 : 0];
            if (image != null) GUI.DrawTexture(new Rect(60, 150, 720, 540), image, ScaleMode.ScaleToFit);
            GUI.Label(new Rect(70, 55, 1780, 75), "MIEN CHO / " + c.Title, title);
            GUI.Label(new Rect(80, 790, 680, 120), "EVIDENCE\n" + c.Evidence, small);
            if (Press(new Rect(80, 710, 680, 65), "THE PEOPLE TRAVELLING WITH YOU")) ShowLives();
            GUI.Label(new Rect(850, 145, 990, 50), "CHAPTER " + (chapter + 1) + " / PAGE " + (page + 1) + " OF " + c.Pages.Length, small);
            string text = c.Pages[page];
            if (chapter == 0 && page == 0) text += "\n\nYOUR ARRIVAL\n" + WaitingArrivals.For(owner.Session.State.ChampionId);
            float height = Mathf.Max(650, copy.CalcHeight(new GUIContent(text), 940) + 30);
            scroll = GUI.BeginScrollView(new Rect(850, 210, 1000, 660), scroll, new Rect(0, 0, 955, height));
            GUI.Label(new Rect(0, 0, 940, height), text, copy); GUI.EndScrollView();
            for (int i = 0; i < MienChoStory.Available(owner.Session.State); i++)
                if (Press(new Rect(70 + i % 5 * 125, 910 + i / 5 * 65, 110, 50), (i + 1) + ((Memory.FinishedMask & 1 << i) != 0 ? " / READ" : ""))) SelectChapter(i);
            if (Press(new Rect(850, 925, 200, 65), "PREVIOUS")) Previous();
            if (Press(new Rect(1070, 925, 490, 65), page + 1 == c.Pages.Length ? "KEEP THE EVIDENCE / CLOSE" : "CONTINUE  [ENTER]")) Advance();
            if (Press(new Rect(1580, 925, 270, 65), "READ LATER  [ESC]")) Close();
            GUI.Label(new Rect(850, 1000, 1000, 65), "Paused / page saved. Stick or arrows: scroll / LB-RB: pages / X: companions / PgUp-PgDn: chapters.", small);
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
            foreach (var entry in owner.Session.State.Roster) if (!ids.Contains(entry.UnitId)) ids.Add(entry.UnitId);
            foreach (var entry in owner.Session.State.AcquiredVariants) if (!ids.Contains(entry.UnitId)) ids.Add(entry.UnitId);
            GUI.Label(new Rect(70, 55, 1780, 75), "MIEN CHO / LIVES ALONGSIDE YOURS", title);
            if (ids.Count == 0) GUI.Label(new Rect(100, 250, 1500, 200), "Every creature you meet has a life before and after this crossing. Recruit a companion to learn their origin here.", copy);
            else
            {
                creature = Mathf.Clamp(creature, 0, ids.Count - 1);
                var id = ids[creature]; var unit = owner.Content.Unit(id);
                RunHud.Sprite(new Rect(100, 220, 560, 470), unit.Body);
                GUI.Label(new Rect(100, 730, 650, 120), unit.DisplayName + "\n" + (creature + 1) + " / " + ids.Count, title);
                string text = "LIFE IN THE WAITING REALM\n" + CreatureLives.Origin(id) + "\n\nALONG THE CROSSING\n" + CreatureLives.Arc(id, owner.Session.State.Wave) + WaitingCompanionQuests.Consequences(owner.Session.State, id);
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
