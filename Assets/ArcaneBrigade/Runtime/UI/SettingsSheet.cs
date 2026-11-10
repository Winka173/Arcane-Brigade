using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The settings page (Light, Sound &amp; Comfort), drawn by the main menu and by the pause menu during a run.
    /// Laid out on the menu's 1280x720 canvas; <paramref name="origin"/> moves it (the pause menu centres it on 1920x1080).
    /// Every change is saved at once; a failed save puts the old value back.</summary>
    public static class SettingsSheet
    {
        public delegate bool ButtonFn(Rect rectangle, string caption);
        private static PlayerProfile profile;
        private static ButtonFn button;
        private static Func<Rect, bool> hit;
        private static GUIStyle segmentStyle;
        private static Func<bool> save;
        private static Vector2 origin;

        public static readonly int[] ShakeSteps = { 100, 75, 50, 25, 0 };
        public static readonly string[] WindowNames = { "Fullscreen", "Borderless", "Windowed" };

        private static Vector2 scroll;
        /// <summary>Review captures: show the foot of the sheet (the legal row), or its top again.</summary>
        public static void ScrollToEnd() => scroll.y = 100000;
        public static void ScrollToTop() => scroll.y = 0;
        private static float contentHeight = 1500;
        /// <summary>The settings in four groups (Sound, Screen, Comfort, Play). UI standard (Docs/UI_Standard.md): every row is its label on
        /// the left and its choices on the right as a segmented row, the chosen one lit gold; nothing is changed by tapping the same
        /// button round and round. The sheet scrolls when it is taller than its space.</summary>
        public static void Draw(PlayerProfile current, Vector2 at, ButtonFn buttonFn, Action<Rect, string> label, Func<bool> saveFn, bool inRun, Func<Rect, bool> hitFn = null)
        {
            profile = current; button = buttonFn; save = saveFn; hit = hitFn;
            const float viewX = 60, viewY = 140, viewW = 1160, viewH = 580;
            scroll = ScrollPanel.Begin(new Rect(at.x + viewX, at.y + viewY, viewW, viewH), scroll, new Rect(0, 0, viewW - 30, contentHeight));
            origin = new Vector2(-viewX, -viewY); // the rows below keep their sheet coordinates; inside the scroll they start at its top
            float y = 150;
            Header(label, ref y, "Language");
            int language = Choice(label, ref y, "Language", new[] { "English", "Tiếng Việt" }, profile.Language == GameLocalization.Vietnamese ? 1 : 0);
            if (language >= 0)
            {
                CheckpointAudio.PlayInterfaceId("ui.setting");
                TrySetLanguage(profile, language == 1 ? GameLocalization.Vietnamese : GameLocalization.English, save);
            }
            Header(label, ref y, "Sound");
            Volume(label, ref y, "Master volume", profile.Volume, value => profile.Volume = value);
            Volume(label, ref y, "Music", profile.MusicVolume, value => profile.MusicVolume = value);
            Volume(label, ref y, "Sound effects", profile.SfxVolume, value => profile.SfxVolume = value);
            Volume(label, ref y, "Voices", profile.VoiceVolume, value => profile.VoiceVolume = value);

            Header(label, ref y, "Screen");
            if (RunUIHints.Phone)
            {
                // a phone: no windows, monitor sizes or vsync; its pace and its sharpness instead
                int rate = Choice(label, ref y, "Frame rate", new[] { "30 · saver", "60" }, profile.FrameCap == 30 ? 0 : 1);
                if (rate >= 0) { int before = profile.FrameCap; profile.FrameCap = rate == 0 ? 30 : 60; Changed(() => profile.FrameCap = before, true); }
                int sharp = Choice(label, ref y, "Sharpness", new[] { "Balanced", "Full" }, profile.MobileSharpness == 1 ? 0 : 1);
                if (sharp >= 0) { int before = profile.MobileSharpness; profile.MobileSharpness = sharp == 0 ? 1 : 0; Changed(() => profile.MobileSharpness = before, true); }
            }
            else DesktopScreen(label, ref y);

            Header(label, ref y, "Comfort");
            OnOff(label, ref y, "Reduced motion", profile.ReducedMotion, value => profile.ReducedMotion = value);
            OnOff(label, ref y, "Reduced flashes", profile.ReducedFlashes, value => profile.ReducedFlashes = value);
            OnOff(label, ref y, "Reduced effects", profile.ReducedEffects, value => profile.ReducedEffects = value);
            // screen shake: a strength, not only on/off
            int shake = ShakePercent(profile);
            int[] shakes = { 0, 25, 50, 75, 100 };
            int pickedShake = Choice(label, ref y, "Screen shake", new[] { "Off", "25%", "50%", "75%", "100%" }, Array.IndexOf(shakes, shake));
            if (pickedShake >= 0)
            {
                float scale = profile.ShakeScale; bool reduced = profile.ReducedShake;
                profile.ShakeScale = shakes[pickedShake] / 100f; profile.ReducedShake = shakes[pickedShake] == 0;
                Changed(() => { profile.ShakeScale = scale; profile.ReducedShake = reduced; });
            }
            OnOff(label, ref y, "Quieter sounds", profile.ReducedAudio, value => profile.ReducedAudio = value);
            OnOff(label, ref y, "High-contrast UI", profile.HighReadability, value => profile.HighReadability = value);

            Header(label, ref y, "Play");
            OnOff(label, ref y, "Damage numbers", profile.ShowDamageNumbers, value => profile.ShowDamageNumbers = value);
            OnOff(label, ref y, "Loot indicators", profile.ShowLootIndicators, value => profile.ShowLootIndicators = value);
            OnOff(label, ref y, "Highlight hero", profile.HighlightHero, value => profile.HighlightHero = value);
            // story pages: open by themselves after a wave, or only leave a sealed note (open the note when you want to read)
            int story = Choice(label, ref y, "Story pages", new[] { "Open by themselves", "Note only" }, profile.StoryPagesAutoOpen ? 0 : 1);
            if (story >= 0)
            {
                bool before = profile.StoryPagesAutoOpen;
                profile.StoryPagesAutoOpen = story == 0;
                CheckpointAudio.PlayInterfaceId("ui.setting");
                if (!save()) profile.StoryPagesAutoOpen = before;
            }
            // the cosmetic bundle is chosen between runs (the champion is already dressed during one); a locked bundle cannot be chosen
            int bundle = Choice(label, ref y, inRun ? "Bundle (between runs)" : "Bundle", new[] { RunMeta.CosmeticName(0), RunMeta.CosmeticName(1), RunMeta.CosmeticName(2) },
                profile.Cosmetic, option => !inRun && RunMeta.CosmeticUnlocked(profile, option));
            if (bundle >= 0)
            {
                int before = profile.Cosmetic;
                profile.Cosmetic = bundle;
                if (!save()) profile.Cosmetic = before;
                PresentationSettings.Apply(profile);
            }
            // cloud save (phones, between runs): the save in the player's own Google Drive; nothing changes until they connect
            if (!inRun && CloudSync.Offered) CloudRows(label, ref y);
            // the one-time tips and the first-run coach, shown again from the next moment they apply
            label(Shift(new Rect(75, y + 8, 330, 36)), "Tips");
            if (Button(new Rect(420, y, 380, RowControl), "Show tips again"))
            {
                var tips = new List<string>(profile.TipsSeen); bool coach = profile.TutorialSeen;
                profile.TipsSeen.Clear(); profile.TutorialSeen = false;
                Changed(() => { profile.TipsSeen.AddRange(tips); profile.TutorialSeen = coach; });
            }
            y += RowStep;
            // the published privacy policy, terms and data deletion steps open from inside the game (Google Play user data policy)
            Header(label, ref y, "Legal");
            if (Button(new Rect(75, y, 360, RowControl), "Privacy Policy")) OpenLegal("privacy");
            if (Button(new Rect(460, y, 360, RowControl), "Terms of Service")) OpenLegal("terms");
            if (Button(new Rect(845, y, 360, RowControl), "Data deletion")) OpenLegal("data-deletion");
            y += RowStep;
            contentHeight = y - 150 + 40;
            ScrollPanel.End();
            origin = at;
        }
        private const float RowStep = 66, RowControl = 50;
        /// <summary>The game's legal page on the studio site, in the language the game is shown in (English otherwise).</summary>
        public static string LegalUrl(string page, string language) =>
            "https://winka.top/" + (language == GameLocalization.Vietnamese ? "vi" : "en") + "/games/arcane-brigade/" + page + "/";
        private static void OpenLegal(string page)
        {
            CheckpointAudio.PlayInterfaceId("ui.setting");
            Application.OpenURL(LegalUrl(page, profile.Language));
        }
        /// <summary>Cloud save: connect or disconnect Google, automatic sync, the last sync with Sync now, and the restore points.</summary>
        private static void CloudRows(Action<Rect, string> label, ref float y)
        {
            Header(label, ref y, "Cloud save");
            bool connected = CloudSync.Connected, busy = CloudSync.Busy;
            bool wasEnabled = GUI.enabled;
            label(Shift(new Rect(75, y + 8, 330, 36)), "Google Drive");
            CloudStatus status = CloudSync.Status;
            label(Shift(new Rect(420, y + 8, 395, 36)), connected && (status == CloudStatus.Idle || status == CloudStatus.Working) ? "Connected" : CloudSync.StatusLine());
            GUI.enabled = wasEnabled && !busy;
            if (Button(new Rect(825, y, 380, RowControl), connected ? "Disconnect" : "Connect Google"))
            {
                CheckpointAudio.PlayInterfaceId("ui.setting");
                if (connected) CloudSync.Disconnect(); else CloudSync.Connect();
            }
            GUI.enabled = wasEnabled;
            y += RowStep;
            if (connected)
            {
                int auto = Choice(label, ref y, "Auto sync", new[] { "Off", "On" }, CloudSync.Auto ? 1 : 0);
                if (auto >= 0) { CheckpointAudio.PlayInterfaceId("ui.setting"); CloudSync.SetAuto(auto == 1); }
                label(Shift(new Rect(75, y + 8, 330, 36)), "Last synced");
                label(Shift(new Rect(420, y + 8, 395, 36)), status == CloudStatus.Working ? "Syncing…" : CloudSync.LastSynced());
                string action = status == CloudStatus.Conflict ? "Choose a save" : status == CloudStatus.AuthNeeded ? "Allow access" : "Sync now";
                GUI.enabled = wasEnabled && !busy;
                if (Button(new Rect(825, y, 380, RowControl), action))
                {
                    CheckpointAudio.PlayInterfaceId("ui.setting");
                    if (status == CloudStatus.Conflict) CloudSaveCard.OpenConflict(); else CloudSync.SyncNow();
                }
                GUI.enabled = wasEnabled;
                y += RowStep;
            }
            int points = CloudSync.BackupCount;
            if (connected || points > 0)
            {
                label(Shift(new Rect(75, y + 8, 330, 36)), "Restore");
                label(Shift(new Rect(420, y + 8, 395, 36)), points == 0 ? "No restore points yet" : points == 1 ? "1 restore point" : points + " restore points");
                GUI.enabled = wasEnabled && points > 0 && !busy;
                if (Button(new Rect(825, y, 380, RowControl), "Restore…")) { CheckpointAudio.PlayInterfaceId("ui.setting"); CloudSaveCard.OpenRestore(); }
                GUI.enabled = wasEnabled;
                y += RowStep;
            }
        }
        /// <summary>PC: window mode, resolution (many sizes: a stepper with its arrows), and the frame pace.</summary>
        private static void DesktopScreen(Action<Rect, string> label, ref float y)
        {
            int window = DisplayWindow(profile);
            int pickedWindow = Choice(label, ref y, "Window", new[] { "Fullscreen", "Borderless", "Windowed" }, window);
            if (pickedWindow >= 0) { int before = profile.WindowMode; profile.WindowMode = pickedWindow; Changed(() => profile.WindowMode = before, true); }
            Vector2Int size = DisplaySize(profile);
            List<Vector2Int> sizes = Sizes();
            int index = sizes.IndexOf(size);
            label(Shift(new Rect(75, y + 8, 330, 36)), "Resolution");
            GUI.enabled = index > 0;
            bool smaller = Button(new Rect(420, y, 220, RowControl), "Smaller");
            GUI.enabled = true;
            label(Shift(new Rect(665, y + 8, 220, 36)), size.x + " × " + size.y);
            GUI.enabled = index < sizes.Count - 1;
            bool larger = Button(new Rect(880, y, 220, RowControl), "Larger");
            GUI.enabled = true;
            if (smaller || larger)
            {
                int width = profile.ScreenWidth, height = profile.ScreenHeight;
                Vector2Int next = sizes[Mathf.Clamp(index + (larger ? 1 : -1), 0, sizes.Count - 1)];
                profile.ScreenWidth = next.x; profile.ScreenHeight = next.y;
                Changed(() => { profile.ScreenWidth = width; profile.ScreenHeight = height; }, true);
            }
            y += RowStep;
            int pace = profile.VSync ? 0 : profile.FrameCap == 60 ? 1 : profile.FrameCap == 120 ? 2 : 3;
            int pickedPace = Choice(label, ref y, "Frames", new[] { "Vsync", "60", "120", "Max" }, pace);
            if (pickedPace >= 0)
            {
                bool vsync = profile.VSync; int cap = profile.FrameCap;
                profile.VSync = pickedPace == 0; profile.FrameCap = pickedPace == 1 ? 60 : pickedPace == 2 ? 120 : 0;
                Changed(() => { profile.VSync = vsync; profile.FrameCap = cap; }, true);
            }
        }
        /// <summary>One settings row: the label, then its choices side by side; the chosen one glows gold. Returns the newly picked index, or -1.</summary>
        private static int Choice(Action<Rect, string> label, ref float y, string caption, string[] options, int selected, Func<int, bool> allowed = null)
        {
            label(Shift(new Rect(75, y + 8, 330, 36)), caption);
            // the painted segmented row (joined pieces, the chosen one gold); without the art, separate buttons with a gold bar
            bool painted = !ArcadeSkin.Crafted && ArcadeSkin.KitSprite("Controls/SegmentMiddleNormal") != null;
            const float left = 420, span = 785;
            float gap = painted ? 0 : 10, width = (span - (options.Length - 1) * gap) / options.Length;
            int picked = -1;
            bool enabled = GUI.enabled;
            segmentStyle ??= ArcadeTypography.Style(ArcadeTypography.On720(ArcadeTypography.SizeNote), true);
            for (int i = 0; i < options.Length; i++)
            {
                Rect r = new Rect(left + i * (width + gap), y, width, RowControl);
                bool can = enabled && (allowed == null || allowed(i) || i == selected);
                GUI.enabled = can;
                if (painted)
                {
                    string piece = options.Length == 1 ? "Middle" : i == 0 ? "Left" : i == options.Length - 1 ? "Right" : "Middle";
                    Rect s = Shift(r);
                    Color keep = GUI.color; if (!can) GUI.color = new Color(keep.r * .6f, keep.g * .6f, keep.b * .6f, keep.a);
                    ArcadeSkin.Strip(s, "Controls/Segment" + piece + (i == selected ? ChosenPiece : "Normal"), piece == "Left" ? 24 : 0, piece == "Right" ? 24 : 0);
                    GUI.color = keep;
                    ArcadeSkin.KitCaption(new Rect(s.x + 14, s.y, s.width - 28, s.height), options[i], i == selected ? (ChosenPiece == "Chosen" ? new Color(1f, .9f, .62f) : ArcadeSkin.KitInk) : can ? ArcadeSkin.KitCream : new Color(.7f, .66f, .6f), segmentStyle, true);
                    if (Hit(r) && i != selected) picked = i;
                }
                else
                {
                    if (i == selected) ArcadeSkin.Glow(Shift(new Rect(r.x - 10, r.y - 10, r.width + 20, r.height + 20)), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .6f));
                    if (Button(r, options[i]) && i != selected) picked = i;
                    if (i == selected) ArcadeSkin.Line(Shift(new Rect(r.x + 14, r.yMax + 3, r.width - 28, 6)), ArcadeSkin.Gold); // the chosen one: a thick gold bar under it
                }
            }
            GUI.enabled = enabled;
            y += RowStep;
            return picked;
        }
        private static void OnOff(Action<Rect, string> label, ref float y, string caption, bool value, Action<bool> setter)
        {
            int picked = Choice(label, ref y, caption, new[] { "Off", "On" }, value ? 1 : 0);
            if (picked < 0) return;
            setter(picked == 1);
            Changed(() => setter(value));
        }
        private static void Header(Action<Rect, string> label, ref float y, string caption)
        {
            y += 10;
            // each section opens with its painted icon (review 31/10/2026: a thin gold rule under a small word)
            Sprite icon = ArcadeSkin.KitSprite("Info/Icon" + caption);
            if (icon != null) { RunHud.Sprite(Shift(new Rect(75, y - 6, 48, 48)), icon); label(Shift(new Rect(133, y, 600, 40)), caption); }
            else { label(Shift(new Rect(75, y, 600, 40)), caption); ArcadeSkin.Divider(Shift(new Rect(75, y + 40, 1130, 2)), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .5f)); }
            y += 54;
        }

        public static int ShakePercent(PlayerProfile p) => p.ReducedShake ? 0 : ShakeSteps.OrderBy(step => Mathf.Abs(step - p.ShakeScale * 100)).First();
        public static int DisplayWindow(PlayerProfile p)
        {
            if (p.WindowMode >= 0) return Mathf.Clamp(p.WindowMode, 0, WindowNames.Length - 1);
            return Screen.fullScreenMode == FullScreenMode.Windowed ? 2 : Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen ? 0 : 1;
        }
        public static Vector2Int DisplaySize(PlayerProfile p) => p.ScreenWidth > 0 && p.ScreenHeight > 0 ? new Vector2Int(p.ScreenWidth, p.ScreenHeight) : new Vector2Int(Screen.width, Screen.height);
        /// <summary>The resolutions offered: the monitor's own sizes from 1280x720 up (refresh rates folded), always including the current one.</summary>
        public static List<Vector2Int> Sizes()
        {
            var sizes = new List<Vector2Int>();
            foreach (Resolution r in Screen.resolutions) { var s = new Vector2Int(r.width, r.height); if (s.x >= 1280 && s.y >= 720 && !sizes.Contains(s)) sizes.Add(s); }
            if (sizes.Count == 0) sizes.AddRange(new[] { new Vector2Int(1280, 720), new Vector2Int(1600, 900), new Vector2Int(1920, 1080) });
            var current = DisplaySize(profile ?? new PlayerProfile());
            if (!sizes.Contains(current)) sizes.Add(current);
            sizes.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            return sizes;
        }

        private static Rect Shift(Rect r) => new Rect(r.x + origin.x, r.y + origin.y, r.width, r.height);
        private static bool Button(Rect r, string caption) => button(Shift(r), caption);
        /// <summary>A tap on a painted piece, through the menu's navigation when it gives one (keys and pads reach it too).</summary>
        private static bool Hit(Rect r) => hit != null ? hit(Shift(r)) : TouchTargets.Button(Shift(r), GUIContent.none, GUIStyle.none);
        private static void Changed(Action undo, bool display = false)
        {
            CheckpointAudio.PlayInterfaceId("ui.setting");
            if (!save()) undo();
            PresentationSettings.Apply(profile);
            if (display) PresentationSettings.ApplyDisplay(profile);
        }
        /// <summary>Use the same immediate-save and rollback contract as the other settings.</summary>
        public static bool TrySetLanguage(PlayerProfile current, string language, Func<bool> saveFn)
        {
            string before = current.Language;
            current.Language = GameLocalization.NormalizeLanguage(language);
            bool saved = saveFn();
            if (!saved) current.Language = before;
            PresentationSettings.Apply(current);
            return saved;
        }
        /// <summary>The chosen segment is the same piece lacquered red (gold read as the screen's main button).</summary>
        private static string ChosenPiece => ArcadeSkin.KitSprite("Controls/SegmentMiddleChosen") != null ? "Chosen" : "Selected";
        private static GUIStyle stepStyle;
        /// <summary>A round walnut step button with its sign centred on the disc.</summary>
        private static bool Step(Rect r, string sign)
        {
            Rect at = Shift(r);
            bool over = !RunUIHints.Phone && GUI.enabled && at.Contains(Event.current.mousePosition);
            ArcadeSkin.RoundArt(at, KitRound.Walnut, !GUI.enabled ? 3 : over ? (GUIUtility.hotControl != 0 ? 2 : 1) : 0);
            stepStyle ??= new GUIStyle(ArcadeTypography.Style(32, true)) { alignment = TextAnchor.MiddleCenter };
            stepStyle.normal.textColor = ArcadeSkin.KitCream;
            LocalizedGUI.Label(ArcadeSkin.RoundFace(at, KitRound.Walnut), sign, stepStyle);
            return Hit(r);
        }
        private static void Volume(Action<Rect, string> label, ref float y, string caption, float value, Action<float> setter)
        {
            label(Shift(new Rect(75, y + 8, 330, 36)), caption + ": " + Mathf.RoundToInt(value * 100) + "%");
            // the slider is the control; a small round step button sits at each end of it (keys and pads step by 10%), never the
            // wide flat-ended plates that crowded the row (review 4/11/2026)
            float changed = GUI.HorizontalSlider(Shift(new Rect(500, y + 7, 625, 36)), value, 0, 1);
            TouchTargets.DrawHover(Shift(new Rect(500, y + 7, 625, 36)));
            if (Step(new Rect(430, y - 3, 56, 56), "−")) changed = Mathf.Max(0, value - 0.1f);
            if (Step(new Rect(1139, y - 3, 56, 56), "+")) changed = Mathf.Min(1, value + 0.1f);
            y += RowStep;
            if (Mathf.Abs(changed - value) < 0.005f) return;
            setter(changed);
            Changed(() => setter(value));
        }
    }
}
