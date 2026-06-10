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
        private static Func<bool> save;
        private static Vector2 origin;

        public static readonly int[] ShakeSteps = { 100, 75, 50, 25, 0 };
        public static readonly string[] WindowNames = { "FULLSCREEN", "BORDERLESS", "WINDOWED" };

        private static Vector2 scroll;
        /// <summary>The settings in four groups (requirement 24/10/2026: "settings are too cramped", and the fonts' licence line read as a
        /// mysterious "font" setting - credits belong in the credits): Sound, Screen, Comfort and Play, rows 60 apart with room
        /// between the plates, scrolling when the sheet is taller than its space.</summary>
        public static void Draw(PlayerProfile current, Vector2 at, ButtonFn buttonFn, Action<Rect, string> label, Func<bool> saveFn, bool inRun)
        {
            profile = current; button = buttonFn; save = saveFn;
            const float viewX = 60, viewY = 140, viewW = 1160, viewH = 580, contentH = 860;
            scroll = ScrollPanel.Begin(new Rect(at.x + viewX, at.y + viewY, viewW, viewH), scroll, new Rect(0, 0, viewW - 30, contentH));
            origin = new Vector2(-viewX, -viewY); // the rows below keep their sheet coordinates; inside the scroll they start at its top
            Header(label, 150, "SOUND");
            Volume(label, 194, "Master volume", profile.Volume, value => profile.Volume = value);
            Volume(label, 244, "Music", profile.MusicVolume, value => profile.MusicVolume = value);
            Volume(label, 294, "Sound effects", profile.SfxVolume, value => profile.SfxVolume = value);
            Volume(label, 344, "Voices", profile.VoiceVolume, value => profile.VoiceVolume = value);

            Header(label, 410, "SCREEN");
            if (Application.isMobilePlatform)
            {
                // a phone: no windows, monitor sizes or vsync; its pace and its sharpness instead
                if (Button(new Rect(75, 454, 360, 48), "FRAME RATE: " + (profile.FrameCap == 30 ? "30 · SAVER" : "60")))
                {
                    int before = profile.FrameCap; profile.FrameCap = before == 30 ? 60 : 30;
                    Changed(() => profile.FrameCap = before, true);
                }
                if (Button(new Rect(460, 454, 360, 48), "SHARPNESS: " + (profile.MobileSharpness == 1 ? "BALANCED" : "FULL")))
                {
                    int before = profile.MobileSharpness; profile.MobileSharpness = before == 1 ? 0 : 1;
                    Changed(() => profile.MobileSharpness = before, true);
                }
            }
            else DesktopScreen();

            Header(label, 530, "COMFORT");
            Toggle(new Rect(75, 574, 360, 48), "REDUCED MOTION", profile.ReducedMotion, value => profile.ReducedMotion = value);
            Toggle(new Rect(460, 574, 360, 48), "REDUCED FLASHES", profile.ReducedFlashes, value => profile.ReducedFlashes = value);
            Toggle(new Rect(845, 574, 360, 48), "REDUCED EFFECTS", profile.ReducedEffects, value => profile.ReducedEffects = value);
            // screen shake: a strength, not only on/off (100 -> 75 -> 50 -> 25 -> off)
            int shake = ShakePercent(profile);
            if (Button(new Rect(75, 634, 360, 48), "SCREEN SHAKE: " + (shake == 0 ? "OFF" : shake + "%")))
            {
                float scale = profile.ShakeScale; bool reduced = profile.ReducedShake;
                int next = ShakeSteps[(Array.IndexOf(ShakeSteps, shake) + 1) % ShakeSteps.Length];
                profile.ShakeScale = next / 100f; profile.ReducedShake = next == 0;
                Changed(() => { profile.ShakeScale = scale; profile.ReducedShake = reduced; });
            }
            Toggle(new Rect(460, 634, 360, 48), "QUIETER SOUNDS", profile.ReducedAudio, value => profile.ReducedAudio = value);
            Toggle(new Rect(845, 634, 360, 48), "HIGH-CONTRAST UI", profile.HighReadability, value => profile.HighReadability = value);

            Header(label, 710, "PLAY");
            Toggle(new Rect(75, 754, 360, 48), "DAMAGE NUMBERS", profile.ShowDamageNumbers, value => profile.ShowDamageNumbers = value);
            Toggle(new Rect(460, 754, 360, 48), "LOOT INDICATORS", profile.ShowLootIndicators, value => profile.ShowLootIndicators = value);
            Toggle(new Rect(845, 754, 360, 48), "HIGHLIGHT HERO", profile.HighlightHero, value => profile.HighlightHero = value);
            // story pages: open by themselves after a wave, or only leave a sealed note (open the note when you want to read)
            if (Button(new Rect(75, 814, 745, 48), "STORY PAGES: " + (profile.StoryPagesAutoOpen ? "OPEN BY THEMSELVES" : "NOTE ONLY")))
            {
                bool before = profile.StoryPagesAutoOpen;
                profile.StoryPagesAutoOpen = !before;
                CheckpointAudio.PlayInterfaceId("ui.setting");
                if (!save()) profile.StoryPagesAutoOpen = before;
            }
            // the one-time tips and the first-run coach, shown again from the next moment they apply
            if (Button(new Rect(845, 814, 360, 48), "SHOW TIPS AGAIN"))
            {
                var tips = new List<string>(profile.TipsSeen); bool coach = profile.TutorialSeen;
                profile.TipsSeen.Clear(); profile.TutorialSeen = false;
                Changed(() => { profile.TipsSeen.AddRange(tips); profile.TutorialSeen = coach; });
            }
            // the cosmetic bundle is chosen between runs (the champion is already dressed during one)
            GUI.enabled = !inRun;
            if (Button(new Rect(75, 874, 560, 48), inRun ? "BUNDLE: CHOSEN BETWEEN RUNS" : "BUNDLE: " + RunMeta.CosmeticName(profile.Cosmetic)))
            {
                int before = profile.Cosmetic;
                int candidate = (before + 1) % 3;
                while (!RunMeta.CosmeticUnlocked(profile, candidate)) candidate = (candidate + 1) % 3;
                profile.Cosmetic = candidate;
                if (!save()) profile.Cosmetic = before;
                PresentationSettings.Apply(profile);
            }
            GUI.enabled = true;
            // the UI style: drawn by hand in code, or the painted kit
            if (Button(new Rect(660, 874, 545, 48), "UI STYLE: " + (profile.UiCrafted ? "CRAFTED" : "PAINTED")))
            {
                bool before = profile.UiCrafted; profile.UiCrafted = !before;
                Changed(() => profile.UiCrafted = before);
            }
            ScrollPanel.End();
            origin = at;
        }
        /// <summary>PC: window mode, resolution, and the frame pace (vsync, or a cap of 60 / 120 / none).</summary>
        private static void DesktopScreen()
        {
            int window = DisplayWindow(profile);
            if (Button(new Rect(75, 454, 360, 48), "WINDOW: " + WindowNames[window]))
            {
                int before = profile.WindowMode;
                profile.WindowMode = (window + 1) % WindowNames.Length;
                Changed(() => profile.WindowMode = before, true);
            }
            Vector2Int size = DisplaySize(profile);
            if (Button(new Rect(460, 454, 360, 48), "RESOLUTION: " + size.x + "×" + size.y))
            {
                int width = profile.ScreenWidth, height = profile.ScreenHeight;
                List<Vector2Int> sizes = Sizes();
                int index = sizes.IndexOf(size);
                Vector2Int next = sizes[(index + 1) % sizes.Count];
                profile.ScreenWidth = next.x; profile.ScreenHeight = next.y;
                Changed(() => { profile.ScreenWidth = width; profile.ScreenHeight = height; }, true);
            }
            string pace = profile.VSync ? "VSYNC" : profile.FrameCap == 60 ? "60" : profile.FrameCap == 120 ? "120" : "MAX";
            if (Button(new Rect(845, 454, 360, 48), "FRAMES: " + pace))
            {
                bool vsync = profile.VSync; int cap = profile.FrameCap;
                // VSYNC -> 60 -> 120 -> MAX -> VSYNC
                if (vsync) { profile.VSync = false; profile.FrameCap = 60; }
                else if (cap == 60) profile.FrameCap = 120;
                else if (cap == 120) profile.FrameCap = 0;
                else { profile.VSync = true; profile.FrameCap = 0; }
                Changed(() => { profile.VSync = vsync; profile.FrameCap = cap; }, true);
            }
        }
        private static void Header(Action<Rect, string> label, float top, string caption) => label(Shift(new Rect(75, top, 600, 36)), caption);

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
        private static void Changed(Action undo, bool display = false)
        {
            CheckpointAudio.PlayInterfaceId("ui.setting");
            if (!save()) undo();
            PresentationSettings.Apply(profile);
            if (display) PresentationSettings.ApplyDisplay(profile);
        }
        private static void Toggle(Rect rectangle, string caption, bool value, Action<bool> setter)
        {
            if (!Button(rectangle, caption + (value ? ": ON" : ": OFF"))) return;
            setter(!value);
            Changed(() => setter(value));
        }
        private static void Volume(Action<Rect, string> label, float top, string caption, float value, Action<float> setter)
        {
            label(Shift(new Rect(75, top, 310, 36)), caption + ": " + Mathf.RoundToInt(value * 100) + "%");
            float changed = GUI.HorizontalSlider(Shift(new Rect(390, top + 14, 630, 25)), value, 0, 1);
            if (Button(new Rect(1042, top, 76, 36), "−")) changed = Mathf.Max(0, value - 0.1f);
            if (Button(new Rect(1128, top, 76, 36), "+")) changed = Mathf.Min(1, value + 0.1f);
            if (Mathf.Abs(changed - value) < 0.005f) return;
            setter(changed);
            Changed(() => setter(value));
        }
    }
}
