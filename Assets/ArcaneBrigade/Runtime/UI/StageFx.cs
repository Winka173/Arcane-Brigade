using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Gentle UI motion locked to the soundtrack's beat (falls back to a 120 BPM wall clock when no piece plays).
    /// Miền Chờ is quiet and watchful, so the pulse is a soft breath rather than a bounce.</summary>
    public static class BeatPulse
    {
        private static double Now => ArcadeMusic.BeatLive ? AudioSettings.dspTime : Time.unscaledTimeAsDouble;
        private static double Origin => ArcadeMusic.BeatLive ? ArcadeMusic.BeatOriginDsp : 0;
        private static double Seconds => ArcadeMusic.BeatLive ? ArcadeMusic.BeatSeconds : 0.5;
        /// <summary>Position inside the current beat, 0..1.</summary>
        public static float Phase => (float)Repeat((Now - Origin) / Seconds);
        public static int Beat => Mathf.FloorToInt((float)((Now - Origin) / Seconds));
        public static bool Downbeat => ((Beat % ArcadeMusic.BeatsPerBar) + ArcadeMusic.BeatsPerBar) % ArcadeMusic.BeatsPerBar == 0;
        /// <summary>Soft swell on each beat, a little stronger on the bar's downbeat. 0 when motion is reduced.</summary>
        public static float Kick => PresentationSettings.ReducedMotion ? 0f : Mathf.Exp(-Phase * 4f) * (Downbeat ? 0.35f : 0.2f);
        private static double Repeat(double value) => value - System.Math.Floor(value);
    }

    /// <summary>"A moment of light" banner for big moments (boss emerging, boss driven back, a companion shining brighter,
    /// a bond growing). It fades up through the dark with a slow light sweep; play never stops.</summary>
    public sealed class StageCutIn
    {
        private sealed class Entry { public string Title, Subtitle, Cue; public Sprite Sprite; public Color Color; public float Started; }
        private readonly Queue<Entry> queue = new Queue<Entry>();
        private Entry live;
        private GUIStyle title, subtitle;
        private const float Duration = 1.6f;
        public bool Busy => live != null || queue.Count > 0;
        /// <summary>A cut-in band is on screen this frame (speech bubbles wait for it to pass).</summary>
        public static bool LiveNow => Time.frameCount - liveFrame <= 2;
        private static int liveFrame = -10;

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
            liveFrame = Time.frameCount;
            if (title == null)
            {
                title = ArcadeTypography.Style(64, true); title.wordWrap = false; title.alignment = TextAnchor.MiddleLeft; if (ArcadeTypography.Display != null) title.font = ArcadeTypography.Display;
                subtitle = ArcadeTypography.Style(22, true); subtitle.wordWrap = false; subtitle.alignment = TextAnchor.MiddleLeft;
            }
            bool reduced = PresentationSettings.ReducedMotion;
            float alpha = Mathf.Min(Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.25f)), 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - (Duration - 0.4f)) / 0.4f)));
            float rise = reduced ? 0 : (1 - Mathf.Clamp01(age / 0.35f)) * 14;
            float bandY = design.y * 0.38f, bandH = design.y * 0.22f;
            Color tint = live.Color;
            // the night presses in a little, then a band of soft dark with a light passing through it
            ArcadeSkin.Line(new Rect(0, 0, design.x, design.y), new Color(0, 0, 0, 0.22f * alpha));
            ArcadeSkin.Line(new Rect(0, bandY + rise, design.x, bandH), new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, 0.86f * alpha));
            ArcadeSkin.Divider(new Rect(0, bandY + rise + 4, design.x, 1), new Color(tint.r, tint.g, tint.b, 0.55f * alpha));
            ArcadeSkin.Divider(new Rect(0, bandY + rise + bandH - 5, design.x, 1), new Color(tint.r, tint.g, tint.b, 0.55f * alpha));
            float sweep = reduced ? 0.3f : Mathf.Clamp01(age / Duration);
            ArcadeSkin.Glow(new Rect(design.x * (sweep * 1.2f - 0.25f), bandY + rise - bandH * 0.3f, design.x * 0.35f, bandH * 1.6f), new Color(tint.r, tint.g, tint.b, 0.28f * alpha));
            if (live.Sprite != null)
            {
                Rect portrait = new Rect(design.x * 0.1f, bandY + rise - bandH * 0.3f, bandH * 1.5f, bandH * 1.5f);
                ArcadeSkin.Glow(new Rect(portrait.x - 50, portrait.y - 30, portrait.width + 100, portrait.height + 60), new Color(tint.r, tint.g, tint.b, 0.5f * alpha));
                Color previous = GUI.color; GUI.color = new Color(1, 1, 1, alpha);
                RunHud.Sprite(portrait, live.Sprite);
                GUI.color = previous;
            }
            float textX = design.x * 0.1f + bandH * 1.7f;
            title.normal.textColor = new Color(tint.r, tint.g, tint.b, alpha);
            GUI.Label(new Rect(textX, bandY + rise + bandH * 0.1f, design.x, bandH * 0.55f), live.Title, title);
            subtitle.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, alpha * 0.9f);
            GUI.Label(new Rect(textX + 2, bandY + rise + bandH * 0.62f, design.x, bandH * 0.3f), live.Subtitle, subtitle);
        }
    }

    /// <summary>A round postmark "WAVE n" pressed onto the night when a wave begins, like a stamp on a letter that is still on its way.</summary>
    public sealed class WaveStamp
    {
        private int shownWave = -1;
        private string run;
        private float started = -10;
        private bool stampDue;
        private GUIStyle number, label;
        private static Texture2D ring;
        public void Observe(string runId, int wave, bool combat)
        {
            if (run != runId) { run = runId; shownWave = combat ? wave : -1; return; }
            // the stamp waits for the arrival rift to stitch shut, so the two never stack over the hero
            if (combat && wave != shownWave) { shownWave = wave; started = Mathf.Max(Time.unscaledTime, ArrivalRift.ClosesAt); stampDue = true; }
        }
        public void Draw(string caption)
        {
            float age = Time.unscaledTime - started;
            if (age < 0 || age > 1.5f) return;
            if (stampDue) { stampDue = false; CheckpointAudio.PlayInterfaceId("ui.wave.stamp"); }
            if (number == null)
            {
                number = ArcadeTypography.Style(78, true); number.alignment = TextAnchor.MiddleCenter; number.wordWrap = false; if (ArcadeTypography.Display != null) number.font = ArcadeTypography.Display;
                label = ArcadeTypography.Style(20, false, true); label.alignment = TextAnchor.MiddleCenter; label.wordWrap = false;
            }
            if (ring == null) ring = MakeRing();
            bool reduced = PresentationSettings.ReducedMotion;
            float press = reduced ? 1 : Mathf.Lerp(1.35f, 1f, Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.14f)));
            float alpha = Mathf.Clamp01((1.5f - age) / 0.45f) * Mathf.Clamp01(age / 0.06f + 0.2f);
            int split = caption.LastIndexOf(' ');
            string word = split > 0 ? caption.Substring(0, split) : "", digits = split > 0 ? caption.Substring(split + 1) : caption;
            Matrix4x4 previous = GUI.matrix;
            Vector2 centre = new Vector2(960, 310); // above the hero (the camera keeps the hero at the middle), below the wave banner
            ArcadeSkin.TurnAround(-9, centre);
            ArcadeSkin.ScaleAround(Vector2.one * press, centre);
            Color ink = new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.9f * alpha);
            ArcadeSkin.Glow(new Rect(centre.x - 170, centre.y - 170, 340, 340), new Color(0, 0, 0, 0.45f * alpha));
            Color old = GUI.color; GUI.color = ink;
            GUI.DrawTexture(new Rect(centre.x - 115, centre.y - 115, 230, 230), ring);
            GUI.color = old;
            for (int line = 0; line < 3; line++) // cancellation waves to the right of the postmark
                for (int k = 0; k < 14; k++)
                {
                    float x = centre.x + 125 + k * 14, y = centre.y - 30 + line * 30 + Mathf.Sin(k * 0.9f) * 5;
                    ArcadeSkin.Line(new Rect(x, y, 10, 3), new Color(ink.r, ink.g, ink.b, ink.a * (1 - k / 16f)));
                }
            label.normal.textColor = ink;
            GUI.Label(new Rect(centre.x - 100, centre.y - 78, 200, 24), word.Length > 0 ? word : "WAVE", label);
            number.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, alpha);
            GUI.Label(new Rect(centre.x - 110, centre.y - 50, 220, 100), digits, number);
            GUI.matrix = previous;
        }

        private static Texture2D MakeRing()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Postmark", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(64, 64));
                    float outer = Mathf.Clamp01(1.6f - Mathf.Abs(d - 60));
                    float inner = Mathf.Clamp01(1.0f - Mathf.Abs(d - 54));
                    // worn ink: a few gaps around the ring
                    float angle = Mathf.Atan2(y - 64, x - 64);
                    float wear = Mathf.Sin(angle * 7f) > 0.92f ? 0.25f : 1f;
                    pixels[y * size + x] = new Color(1, 1, 1, Mathf.Max(outer, inner) * wear);
                }
            texture.SetPixels(pixels); texture.Apply(false, true);
            return texture;
        }
    }

    /// <summary>Low health: the night closes in from the edges, with a faint ember pulse, like a lantern running low.</summary>
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
                        pixels[y * 64 + x] = new Color(1, 1, 1, Mathf.SmoothStep(0, 1, Mathf.InverseLerp(0.5f, 1.05f, d)));
                    }
                edge.SetPixels(pixels); edge.Apply(false, true);
            }
            float breathe = PresentationSettings.ReducedMotion ? 0.8f : 0.7f + 0.3f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 2.6f));
            Color previous = GUI.color;
            GUI.color = new Color(0.02f, 0.02f, 0.05f, Mathf.Clamp01(amount * 0.95f));
            GUI.DrawTexture(new Rect(0, 0, design.x, design.y), edge);
            GUI.color = new Color(0.55f, 0.08f, 0.06f, Mathf.Clamp01(amount * breathe * 0.45f));
            GUI.DrawTexture(new Rect(-40, -40, design.x + 80, design.y + 80), edge);
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
                    message = "Move to lead your brigade. Everyone attacks by themselves.";
                    if (Vector2.Distance(owner.Simulation.Champion.Position, startPosition) > 2.5f && age > 2f || age > 8f) Next(now);
                    break;
                case 1:
                    if (!combat) { Next(now); break; }
                    message = "Your companions fight beside you on their own. Keep moving, and watch for red eyes in the dark.";
                    if (age > 5.5f) Next(now);
                    break;
                case 2:
                    if (!combat) { Next(now); break; }
                    message = "Gather coins and dreamsand. The shop takes them for companions and relics.";
                    if (age > 5f) Next(now);
                    break;
                case 3:
                    if (!shop) break;
                    if (startRoster < 0) startRoster = state.Roster.Count + state.Items.Count;
                    message = "Walk up to a stand, then BUY.";
                    if (state.Roster.Count + state.Items.Count != startRoster || age > 12f) Next(now);
                    break;
                case 4:
                    if (!shop) { Finish(owner); return; }
                    message = "Three of the same companion become one, and shine brighter.";
                    if (age > 6f) Next(now);
                    break;
                case 5:
                    if (!shop) { Finish(owner); return; }
                    message = "When you are ready, take the stairs down. It grows darker the deeper you go; your light goes with you.";
                    if (age > 6.5f) { Finish(owner); return; }
                    break;
            }
            if (message == null) { lastSpokenMessage = null; return; }
            if (message != lastSpokenMessage) { CheckpointAudio.PlayInterfaceId("ui.coach.pop"); lastSpokenMessage = message; }
            Bubble(owner, camera, message, Mathf.Clamp01(age / 0.3f));
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
            if (copy == null) { copy = ArcadeTypography.Style(29); copy.font = ArcadeTypography.Hand; copy.normal.textColor = new Color(0.22f, 0.18f, 0.13f); copy.alignment = TextAnchor.MiddleLeft; }
            Vector3 screen = camera.WorldToScreenPoint(owner.Simulation.Champion.Position + Vector2.up * 1.1f);
            Vector3 point = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y, 0));
            float width = Mathf.Min(560, copy.CalcSize(new GUIContent(message)).x + 60);
            float height = copy.CalcHeight(new GUIContent(message), width - 50) + 30;
            float drift = PresentationSettings.ReducedMotion ? 0 : (1 - reveal) * 8;
            Rect bubble = new Rect(Mathf.Clamp(point.x - width * 0.5f, 20, 1900 - width), Mathf.Clamp(point.y - height - 30 + drift, 120, 900), width, height);
            Color previous = GUI.color; GUI.color = new Color(1, 1, 1, reveal);
            ArcadeSkin.Glow(new Rect(bubble.x - 30, bubble.y - 20, bubble.width + 60, bubble.height + 40), new Color(ArcadeSkin.Accent.r, ArcadeSkin.Accent.g, ArcadeSkin.Accent.b, 0.25f));
            ArcadeSkin.Line(new Rect(Mathf.Clamp(point.x, bubble.x + 20, bubble.xMax - 30) - 1, bubble.yMax, 2, 22), new Color(ArcadeSkin.Paper.r, ArcadeSkin.Paper.g, ArcadeSkin.Paper.b, 0.8f));
            ArcadeSkin.Ticket(bubble, ArcadeSkin.Paper, ArcadeSkin.Accent);
            GUI.Label(new Rect(bubble.x + 24, bubble.y + 12, bubble.width - 38, bubble.height - 24), message, copy);
            GUI.color = previous;
        }
    }
}
