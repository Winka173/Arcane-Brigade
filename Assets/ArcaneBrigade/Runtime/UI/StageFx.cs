using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>UI motion locked to the soundtrack's beat (falls back to a 120 BPM wall clock when no piece plays).</summary>
    public static class BeatPulse
    {
        private static double Now => ArcadeMusic.BeatLive ? AudioSettings.dspTime : Time.unscaledTimeAsDouble;
        private static double Origin => ArcadeMusic.BeatLive ? ArcadeMusic.BeatOriginDsp : 0;
        private static double Seconds => ArcadeMusic.BeatLive ? ArcadeMusic.BeatSeconds : 0.5;
        /// <summary>Position inside the current beat, 0..1.</summary>
        public static float Phase => (float)Repeat((Now - Origin) / Seconds);
        public static int Beat => Mathf.FloorToInt((float)((Now - Origin) / Seconds));
        public static bool Downbeat => ((Beat % ArcadeMusic.BeatsPerBar) + ArcadeMusic.BeatsPerBar) % ArcadeMusic.BeatsPerBar == 0;
        /// <summary>Sharp attack, quick decay on every beat; stronger on the bar's downbeat. 0 when motion is reduced.</summary>
        public static float Kick => PresentationSettings.ReducedMotion ? 0f : Mathf.Exp(-Phase * 7f) * (Downbeat ? 1f : 0.6f);
        private static double Repeat(double value) => value - System.Math.Floor(value);
    }

    /// <summary>Full-width "on stage" cut-in banner for big moments (boss entrance/defeat, encore merge, bond level).</summary>
    public sealed class StageCutIn
    {
        private sealed class Entry { public string Title, Subtitle, Cue; public Sprite Sprite; public Color Color; public float Started; }
        private readonly Queue<Entry> queue = new Queue<Entry>();
        private Entry live;
        private GUIStyle title, subtitle;
        private const float Duration = 1.35f;
        public bool Busy => live != null || queue.Count > 0;

        public void Show(string heading, string caption, Sprite sprite, Color color, string cue = "ui.unlock")
        {
            if (queue.Count >= 3) return;
            queue.Enqueue(new Entry { Title = heading, Subtitle = caption, Sprite = sprite, Color = color, Cue = cue });
        }

        public void Draw(Vector2 design)
        {
            float now = Time.unscaledTime;
            if (live == null && queue.Count > 0) { live = queue.Dequeue(); live.Started = now; CheckpointAudio.PlayInterfaceId(live.Cue); }
            if (live == null) return;
            float age = now - live.Started;
            if (age >= Duration) { live = null; return; }
            if (title == null)
            {
                title = ArcadeTypography.Style(72, true); title.wordWrap = false; title.alignment = TextAnchor.MiddleLeft;
                if (ArcadeTypography.Display != null) title.font = ArcadeTypography.Display;
                subtitle = ArcadeTypography.Style(26, true); subtitle.wordWrap = false; subtitle.alignment = TextAnchor.MiddleLeft;
            }
            bool reduced = PresentationSettings.ReducedMotion;
            float enter = reduced ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.18f));
            float exit = reduced ? (age > Duration - 0.2f ? 0 : 1) : 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - (Duration - 0.25f)) / 0.25f));
            float slide = (1 - enter) * -design.x + (1 - exit) * design.x;
            float alpha = Mathf.Min(enter, exit);
            ArcadeSkin.Line(new Rect(0, 0, design.x, design.y), new Color(0, 0, 0, 0.28f * alpha));
            float bandY = design.y * 0.36f, bandH = design.y * 0.26f;
            Color ink = ArcadeSkin.Ink;
            ArcadeSkin.Slant(new Rect(slide * 1.15f - 60, bandY - 22, design.x + 120, 16), new Color(live.Color.r, live.Color.g, live.Color.b, alpha));
            ArcadeSkin.Slant(new Rect(slide - 60, bandY, design.x + 120, bandH), new Color(ink.r, ink.g, ink.b, 0.94f * alpha));
            ArcadeSkin.Slant(new Rect(slide * 0.85f - 60, bandY + bandH + 6, design.x + 120, 12), new Color(live.Color.r, live.Color.g, live.Color.b, alpha));
            float drift = reduced ? 0 : age * 40;
            if (live.Sprite != null)
            {
                Rect portrait = new Rect(design.x * 0.08f + slide * 0.6f + drift, bandY - bandH * 0.35f, bandH * 1.6f, bandH * 1.6f);
                ArcadeSkin.Glow(new Rect(portrait.x - 40, portrait.y - 20, portrait.width + 80, portrait.height + 40), new Color(live.Color.r, live.Color.g, live.Color.b, 0.45f * alpha));
                Color previous = GUI.color; GUI.color = new Color(1, 1, 1, alpha);
                RunHud.Sprite(portrait, live.Sprite);
                GUI.color = previous;
            }
            float textX = design.x * 0.08f + bandH * 1.75f + slide * 1.2f - drift * 0.5f;
            title.normal.textColor = new Color(live.Color.r, live.Color.g, live.Color.b, alpha);
            GUI.Label(new Rect(textX + 5, bandY + bandH * 0.12f + 5, design.x, bandH * 0.55f), live.Title, new GUIStyle(title) { normal = { textColor = new Color(0, 0, 0, 0.6f * alpha) } });
            GUI.Label(new Rect(textX, bandY + bandH * 0.12f, design.x, bandH * 0.55f), live.Title, title);
            subtitle.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, alpha);
            GUI.Label(new Rect(textX + 4, bandY + bandH * 0.62f, design.x, bandH * 0.3f), live.Subtitle, subtitle);
        }
    }

    /// <summary>Big "WAVE n" rubber stamp that slams in when a wave begins.</summary>
    public sealed class WaveStamp
    {
        private int shownWave = -1;
        private string run;
        private float started = -10;
        private GUIStyle style;
        public void Observe(string runId, int wave, bool combat)
        {
            if (run != runId) { run = runId; shownWave = combat ? wave : -1; return; }
            if (combat && wave != shownWave) { shownWave = wave; started = Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.wave.stamp"); }
        }
        public void Draw(string caption)
        {
            float age = Time.unscaledTime - started;
            if (age < 0 || age > 1.2f) return;
            if (style == null)
            {
                style = ArcadeTypography.Style(84, true); style.alignment = TextAnchor.MiddleCenter; style.wordWrap = false;
                if (ArcadeTypography.Display != null) style.font = ArcadeTypography.Display;
            }
            bool reduced = PresentationSettings.ReducedMotion;
            float slam = reduced ? 1 : Mathf.Lerp(2.1f, 1f, Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.16f)));
            float alpha = Mathf.Clamp01((1.2f - age) / 0.3f);
            Matrix4x4 previous = GUI.matrix;
            Vector2 centre = new Vector2(960, 430);
            GUIUtility.RotateAroundPivot(-7, centre);
            GUIUtility.ScaleAroundPivot(Vector2.one * slam, centre);
            Rect plate = new Rect(centre.x - 290, centre.y - 70, 580, 140);
            ArcadeSkin.Slant(new Rect(plate.x + 8, plate.y + 10, plate.width, plate.height), new Color(0, 0, 0, 0.5f * alpha));
            ArcadeSkin.Slant(plate, new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, alpha));
            ArcadeSkin.Slant(new Rect(plate.x + 14, plate.y + 12, plate.width - 28, plate.height - 24), new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, alpha));
            style.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, alpha);
            GUI.Label(plate, caption, style);
            GUI.matrix = previous;
        }
    }

    /// <summary>Low-health edge vignette that breathes like a lantern running out of oil.</summary>
    public static class LowHealthVignette
    {
        private static Texture2D edge;
        public static void Draw(Vector2 design, float amount)
        {
            if (amount <= 0.001f || Event.current.type != EventType.Repaint) return;
            if (edge == null)
            {
                edge = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "LowHealthEdge", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
                var pixels = new Color[64 * 64];
                for (int y = 0; y < 64; y++)
                    for (int x = 0; x < 64; x++)
                    {
                        Vector2 p = new Vector2((x + 0.5f) / 32f - 1, (y + 0.5f) / 32f - 1);
                        float d = Mathf.Max(Mathf.Abs(p.x), Mathf.Abs(p.y)) * 0.6f + p.magnitude * 0.4f;
                        pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.55f, 1.05f, d)));
                    }
                edge.SetPixels(pixels); edge.Apply(false, true);
            }
            float breathe = PresentationSettings.ReducedMotion ? 0.8f : 0.65f + 0.35f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3.4f)) + BeatPulse.Kick * 0.1f;
            Color previous = GUI.color;
            GUI.color = new Color(0.35f, 0.02f, 0.09f, Mathf.Clamp01(amount * breathe));
            GUI.DrawTexture(new Rect(0, 0, design.x, design.y), edge);
            GUI.color = previous;
        }
    }

    /// <summary>One-time, in-world coaching for a profile's first run; replaces the text-wall help. Persists TutorialSeen.</summary>
    public sealed class FirstRunCoach
    {
        private int step;
        private float stepStarted = -1;
        private Vector2 startPosition;
        private int startRoster = -1;
        private GUIStyle copy;
        private bool done;
        private string lastSpokenMessage;
        private static bool Forced => System.Array.IndexOf(System.Environment.GetCommandLineArgs(), "-arcane-coach") >= 0;

        public void Draw(ArenaController owner, Camera camera)
        {
            PlayerProfile profile = owner.Profile;
            if (done || profile == null || profile.TutorialSeen && !Forced || RunPaths.Diagnostic && !Forced) return;
            RunSnapshot state = owner.Session.State;
            float now = Time.unscaledTime;
            if (stepStarted < 0) { stepStarted = now; startPosition = owner.Simulation.Champion.Position; }
            float age = now - stepStarted;
            string message = null;
            bool combat = state.Phase == SessionPhase.Combat, shop = state.Phase == SessionPhase.Shop;
            if (state.Wave >= 3) { Finish(owner); return; }
            switch (step)
            {
                case 0:
                    if (!combat) break;
                    message = RunUIHints.Device == RunUIDevice.Touch ? "Drag the lower-left area to move. That's the only control!" : RunUIHints.Device == RunUIDevice.Controller ? "Move with the left stick. That's the only control!" : "Move with WASD or the arrow keys. That's the only control!";
                    if (Vector2.Distance(owner.Simulation.Champion.Position, startPosition) > 2.5f && age > 2f || age > 8f) Next(now);
                    break;
                case 1:
                    if (!combat) { Next(now); break; }
                    message = "Your brigade attacks on its own. Keep moving, dodge pink shots!";
                    if (age > 5f) Next(now);
                    break;
                case 2:
                    if (!combat) { Next(now); break; }
                    message = "Walk over coins and eggs. They pay for allies and relics.";
                    if (age > 5f) Next(now);
                    break;
                case 3:
                    if (!shop) break;
                    if (startRoster < 0) startRoster = state.Roster.Count + state.Items.Count;
                    message = "Walk up to a stand and " + (RunUIHints.Device == RunUIDevice.Controller ? "press A" : RunUIHints.Device == RunUIDevice.Touch ? "tap USE" : "press Space") + " to buy.";
                    if (state.Roster.Count + state.Items.Count != startRoster || age > 12f) Next(now);
                    break;
                case 4:
                    if (!shop) { Finish(owner); return; }
                    message = "Three matching allies merge into a stronger one!";
                    if (age > 6f) Next(now);
                    break;
                case 5:
                    if (!shop) { Finish(owner); return; }
                    message = "Take the stairs down when you're ready for the next wave.";
                    if (age > 6f) { Finish(owner); return; }
                    break;
            }
            if (message == null) { lastSpokenMessage = null; return; }
            if (message != lastSpokenMessage) { CheckpointAudio.PlayInterfaceId("ui.coach.pop"); lastSpokenMessage = message; }
            Bubble(owner, camera, message, Mathf.Clamp01(age / 0.2f));
        }

        private void Next(float now) { step++; stepStarted = now; }

        private void Finish(ArenaController owner)
        {
            done = true;
            if (owner.Profile == null || owner.Profile.TutorialSeen) return;
            owner.Profile.TutorialSeen = true;
            if (RunPaths.Diagnostic) return;
            try { new RunStorage(RunPaths.Root).SaveProfile(owner.Profile); } catch (System.Exception exception) { Debug.LogWarning("Tutorial flag not saved: " + exception.Message); }
        }

        private void Bubble(ArenaController owner, Camera camera, string message, float reveal)
        {
            if (copy == null) { copy = ArcadeTypography.Style(24, true); copy.normal.textColor = ArcadeSkin.Ink; copy.alignment = TextAnchor.MiddleLeft; }
            Vector3 screen = camera.WorldToScreenPoint(owner.Simulation.Champion.Position + Vector2.up * 1.1f);
            Vector3 point = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y, 0));
            float width = Mathf.Min(560, copy.CalcSize(new GUIContent(message)).x + 60);
            float height = copy.CalcHeight(new GUIContent(message), width - 50) + 30;
            float bob = PresentationSettings.ReducedMotion ? 0 : BeatPulse.Kick * -5;
            Rect bubble = new Rect(Mathf.Clamp(point.x - width * 0.5f, 20, 1900 - width), Mathf.Clamp(point.y - height - 30 + bob, 120, 900), width, height);
            Color previous = GUI.color; GUI.color = new Color(1, 1, 1, reveal);
            ArcadeSkin.Line(new Rect(Mathf.Clamp(point.x, bubble.x + 20, bubble.xMax - 30) - 6, bubble.yMax - 2, 12, 22), ArcadeSkin.Cream);
            ArcadeSkin.Ticket(bubble, ArcadeSkin.Cream, ArcadeSkin.Teal);
            GUI.Label(new Rect(bubble.x + 26, bubble.y + 12, bubble.width - 40, bubble.height - 24), message, copy);
            GUI.color = previous;
        }
    }
}
