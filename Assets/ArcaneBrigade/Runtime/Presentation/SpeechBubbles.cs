using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum SpeechTone { Shopkeeper, Dream, Companion, Story, Hero, Boss, Offscreen, Whisper }
    public enum BubbleSide { Above, Right, Left }

    /// <summary>One spoken line, pinned to the speaker's head in the world. The anchor returns null while the speaker is
    /// not on screen (the bubble then waits, hidden, until it expires).</summary>
    public sealed class SpeechBubble
    {
        public string Speaker;
        public string Name;
        public string Text;
        public Func<Vector3?> Anchor;
        public SpeechTone Tone;
        public BubbleSide Prefer;
        public int Priority;
        public float Began;
        public float Expires;
        public Rect Placed { get; internal set; }
        public bool Drawn { get; internal set; }
    }

    /// <summary>World-anchored speech boxes for everyone who talks (shopkeeper, bosses, companions, the hero) and a quiet
    /// caption for voices without a body (the whisper from the left, narration). Drawn in the 1920x1080 HUD space,
    /// clamped to the screen, stacked so boxes never cover each other, short-lived, and calm under Reduced Motion.</summary>
    public static class SpeechBubbles
    {
        public const int MaxBubbles = 5;
        private const float RevealRate = 58f;
        private static readonly List<SpeechBubble> bubbles = new List<SpeechBubble>();
        private static readonly List<Rect> placed = new List<Rect>();
        private static readonly List<SpeechBubble> order = new List<SpeechBubble>();
        private static GUIStyle copy, plainCopy, nameStyle, whisperStyle;
        private static Texture2D tail;
        private static int drawnFrame = -1;
        public static IReadOnlyList<SpeechBubble> Active => bubbles;
        public static int DrawnCount { get; private set; }
        /// <summary>When a companion last spoke (any reason): chatter keeps a polite gap from it.</summary>
        public static float LastCompanionAt { get; private set; } = -100;
        public static Rect Bounds = new Rect(16, 108, 1888, 952);

        public static float Lifetime(string text) => Mathf.Clamp(1.6f + (text?.Length ?? 0) * 0.055f, 2.4f, 6.5f);

        public static SpeechBubble Show(string speaker, string name, string text, Func<Vector3?> anchor, SpeechTone tone, float seconds = 0, int priority = 1, BubbleSide prefer = BubbleSide.Above)
        {
            if (string.IsNullOrEmpty(text)) return null;
            float now = Time.unscaledTime;
            bubbles.RemoveAll(bubble => bubble.Speaker == speaker || now >= bubble.Expires);
            while (bubbles.Count >= MaxBubbles)
            {
                // the crowd keeps the most important and the newest voices
                int drop = 0;
                for (int index = 1; index < bubbles.Count; index++)
                    if (bubbles[index].Priority < bubbles[drop].Priority || bubbles[index].Priority == bubbles[drop].Priority && bubbles[index].Began < bubbles[drop].Began) drop = index;
                if (bubbles[drop].Priority > priority) return null;
                bubbles.RemoveAt(drop);
            }
            var created = new SpeechBubble
            {
                Speaker = speaker, Name = name, Text = ArcadeTypography.DisplayText(text), Anchor = anchor, Tone = tone, Prefer = prefer, Priority = priority,
                Began = now, Expires = now + (seconds > 0 ? seconds : Lifetime(text))
            };
            bubbles.Add(created);
            if (tone == SpeechTone.Companion || tone == SpeechTone.Story) LastCompanionAt = now;
            return created;
        }

        public static bool Showing(string speaker) { float now = Time.unscaledTime; foreach (SpeechBubble bubble in bubbles) if (bubble.Speaker == speaker && now < bubble.Expires) return true; return false; }
        public static SpeechBubble Find(string speaker) { foreach (SpeechBubble bubble in bubbles) if (bubble.Speaker == speaker) return bubble; return null; }
        public static void Clear(string speaker) => bubbles.RemoveAll(bubble => bubble.Speaker == speaker);
        public static void ClearAll() => bubbles.Clear();
        public static int CountTone(SpeechTone tone) { int count = 0; float now = Time.unscaledTime; foreach (SpeechBubble bubble in bubbles) if (bubble.Tone == tone && now < bubble.Expires) count++; return count; }

        /// <summary>Shop stat text uses the same 1920x1080 canvas and leaves live dialogue, including its tail, clear.</summary>
        public static bool OverlapsDialogue(Rect bounds)
        {
            float now = Time.unscaledTime;
            foreach (var bubble in bubbles)
            {
                if (!bubble.Drawn || now >= bubble.Expires) continue;
                Rect area = bubble.Placed;
                area.xMin -= 8; area.xMax += 8; area.yMin -= 8; area.yMax += 30;
                if (area.Overlaps(bounds)) return true;
            }
            return false;
        }

        /// <summary>Head-top anchor of a simulated unit (positions are body centres).</summary>
        public static Vector3 Head(UnitState unit) => (Vector3)(unit.Position + Vector2.up * (Mathf.Min(K8Motion.BodyHeight(unit) * 0.5f, 2.1f) + 0.12f));

        private static void Styles()
        {
            if (copy != null) return;
            copy = ArcadeTypography.Style(31); copy.font = ArcadeTypography.Hand; copy.richText = true; copy.wordWrap = true; copy.normal.textColor = ArcadeSkin.Ink;
            plainCopy = ArcadeTypography.Style(21); plainCopy.richText = true; plainCopy.wordWrap = true; plainCopy.normal.textColor = ArcadeSkin.Ink;
            nameStyle = ArcadeTypography.Style(13, true); nameStyle.wordWrap = false; nameStyle.clipping = TextClipping.Clip;
            whisperStyle = ArcadeTypography.Style(29); whisperStyle.font = ArcadeTypography.Hand; whisperStyle.richText = true; whisperStyle.wordWrap = true;
            tail = new Texture2D(32, 32, TextureFormat.RGBA32, false) { name = "SpeechTail", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[32 * 32];
            for (int y = 0; y < 32; y++)
                for (int x = 0; x < 32; x++)
                {
                    // texture row 31 is drawn at the top of the rect: wide there, a soft point at the bottom
                    float half = 15.5f * (y / 31f);
                    pixels[y * 32 + x] = new Color(1, 1, 1, Mathf.Clamp01(half - Mathf.Abs(x + 0.5f - 16) + 0.5f));
                }
            tail.SetPixels(pixels); tail.Apply(false, true);
        }

        private static void Colours(SpeechTone tone, out Color paper, out Color stamp, out Color ink, out Color label)
        {
            switch (tone)
            {
                case SpeechTone.Boss: paper = new Color(0.13f, 0.1f, 0.13f, 0.97f); stamp = ArcadeSkin.Danger; ink = ArcadeSkin.Cream; label = new Color(1f, 0.55f, 0.45f); return;
                case SpeechTone.Dream: paper = new Color(0.9f, 0.86f, 0.95f); stamp = new Color(0.7f, 0.45f, 0.85f); ink = ArcadeSkin.Ink; label = new Color(0.42f, 0.22f, 0.55f); return;
                case SpeechTone.Companion: paper = ArcadeSkin.Cream; stamp = ArcadeSkin.Teal; ink = ArcadeSkin.Ink; label = new Color(0.16f, 0.38f, 0.45f); return;
                case SpeechTone.Story: paper = new Color(0.97f, 0.9f, 0.76f); stamp = ArcadeSkin.Gold; ink = ArcadeSkin.Ink; label = new Color(0.5f, 0.3f, 0.08f); return;
                case SpeechTone.Hero: paper = ArcadeSkin.Cream; stamp = ArcadeSkin.Gold; ink = ArcadeSkin.Ink; label = new Color(0.45f, 0.3f, 0.1f); return;
                case SpeechTone.Offscreen: paper = new Color(0.94f, 0.89f, 0.79f, 0.94f); stamp = ArcadeSkin.Accent; ink = ArcadeSkin.Ink; label = new Color(0.45f, 0.3f, 0.15f); return;
                default: paper = ArcadeSkin.Cream; stamp = ArcadeSkin.Accent; ink = ArcadeSkin.Ink; label = new Color(0.45f, 0.26f, 0.08f); return;
            }
        }

        // HUD furniture boxes stay clear of: wallet and lamps (top left), bonds (top right); in a fight also the wave plate,
        // timer and lamp line (top centre) and the boss bar (bottom centre)
        private static readonly Rect[] AlwaysReserved = { new Rect(0, 0, 400, 236), new Rect(1580, 0, 340, 214) };
        private static readonly Rect[] CombatReserved = { new Rect(560, 0, 800, 214), new Rect(410, 896, 1100, 110) };

        /// <summary>Draws every live bubble once per frame (the first caller wins: the shop draws them under its tooltips).</summary>
        public static void Draw(Camera camera, bool combat = false)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || camera == null) return;
            if (drawnFrame == Time.frameCount) return;
            drawnFrame = Time.frameCount;
            float now = Time.unscaledTime;
            bubbles.RemoveAll(bubble => now >= bubble.Expires);
            DrawnCount = 0;
            if (bubbles.Count == 0) return;
            Styles();
            bool reduced = PresentationSettings.ReducedMotion;
            bool readable = PresentationSettings.HighReadability;
            Matrix4x4 inverse = GUI.matrix.inverse;
            placed.Clear(); order.Clear(); order.AddRange(bubbles);
            placed.AddRange(AlwaysReserved);
            if (combat) placed.AddRange(CombatReserved);
            // important voices claim their place first; among equals, the older box keeps its spot
            order.Sort((a, b) => a.Priority != b.Priority ? b.Priority.CompareTo(a.Priority) : a.Began.CompareTo(b.Began));
            float offscreenY = Bounds.y + 6;
            foreach (SpeechBubble bubble in order)
            {
                bubble.Drawn = false;
                if (bubble.Tone == SpeechTone.Whisper) { DrawWhisper(bubble, now, reduced); continue; }
                Vector2 anchor;
                bool tailed = bubble.Tone != SpeechTone.Offscreen;
                if (tailed)
                {
                    Vector3? world = bubble.Anchor?.Invoke();
                    if (world == null) continue;
                    Vector3 screen = camera.WorldToScreenPoint(world.Value);
                    if (screen.z < 0) continue;
                    Vector3 point = inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y, 0));
                    anchor = new Vector2(point.x, point.y);
                    // a speaker still off screen (a boss stepping in from the edge) speaks from the edge it is behind
                    anchor =new Vector2(Mathf.Clamp(anchor.x, 40, 1880), Mathf.Clamp(anchor.y, 40, 1040));
                }
                else anchor = new Vector2(960, offscreenY);

                GUIStyle style = readable ? plainCopy : copy;
                float maxWidth = bubble.Tone == SpeechTone.Shopkeeper || bubble.Tone == SpeechTone.Dream ? 420 : bubble.Tone == SpeechTone.Offscreen ? 620 : 350;
                var content = new GUIContent(bubble.Text);
                float textWidth = Mathf.Min(maxWidth - 44, style.CalcSize(content).x + 4);
                textWidth = Mathf.Max(textWidth, 120);
                float textHeight = style.CalcHeight(content, textWidth);
                bool named = !string.IsNullOrEmpty(bubble.Name);
                float top = named ? 30 : 12;
                var size = new Vector2(textWidth + 44, textHeight + top + 18);

                Rect rect = tailed ? Place(anchor, size, bubble.Prefer) : Stack(new Rect(anchor.x - size.x * 0.5f, anchor.y, size.x, size.y));
                if (!tailed) offscreenY = rect.yMax + 8;
                placed.Add(new Rect(rect.x - 6, rect.y - 6, rect.width + 12, rect.height + 12));
                bubble.Placed = rect; bubble.Drawn = true; DrawnCount++;

                float age = now - bubble.Began;
                float alpha = Mathf.Clamp01(age / 0.12f) * Mathf.Clamp01((bubble.Expires - now) / 0.3f);
                float pop = reduced ? 1 : Mathf.Lerp(0.86f, 1f, EaseOutBack(Mathf.Clamp01(age / 0.18f)));
                Colours(bubble.Tone, out Color paper, out Color stamp, out Color ink, out Color label);
                Matrix4x4 restore = GUI.matrix;
                Color previous = GUI.color;
                GUI.color = new Color(1, 1, 1, alpha);
                Vector2 tailBase = tailed ? ClosestEdge(rect, anchor) : rect.center;
                if (pop < 0.999f) GUIUtility.ScaleAroundPivot(Vector2.one * pop, tailed ? anchor : rect.center);
                if (tailed) DrawTail(tailBase + new Vector2(3, 5), anchor + new Vector2(3, 5), new Color(0, 0, 0, 0.4f));
                ArcadeSkin.Ticket(rect, paper, stamp);
                if (tailed) DrawTail(tailBase, anchor, paper);
                if (named)
                {
                    nameStyle.normal.textColor = label;
                    GUI.Label(new Rect(rect.x + 22, rect.y + 9, rect.width - 34, 20), bubble.Name.ToUpperInvariant(), nameStyle);
                }
                style.normal.textColor = ink;
                int glyphs = reduced ? bubble.Text.Length : Mathf.Clamp(Mathf.FloorToInt(age * RevealRate), 0, bubble.Text.Length);
                // the hidden tail of the sentence keeps its space so words never jump between lines while revealing
                string shown = glyphs >= bubble.Text.Length ? bubble.Text : bubble.Text.Substring(0, glyphs) + "<color=#00000000>" + bubble.Text.Substring(glyphs) + "</color>";
                GUI.Label(new Rect(rect.x + 22, rect.y + top - (readable ? 0 : 4), textWidth + 4, textHeight + 8), shown, style);
                GUI.color = previous;
                GUI.matrix = restore;
            }
        }

        private static float EaseOutBack(float t) { const float c = 1.6f; float u = t - 1; return 1 + (c + 1) * u * u * u + c * u * u; }

        private static Rect Place(Vector2 anchor, Vector2 size, BubbleSide prefer)
        {
            const float gap = 34;
            Vector2[] origins =
            {
                new Vector2(anchor.x - size.x * 0.5f, anchor.y - gap - size.y),              // above
                new Vector2(anchor.x + gap, anchor.y - size.y * 0.75f),                      // right
                new Vector2(anchor.x - gap - size.x, anchor.y - size.y * 0.75f),             // left
                new Vector2(anchor.x - size.x * 0.5f, anchor.y + gap + 30),                  // below (last resort)
            };
            int[] preference = prefer == BubbleSide.Right ? new[] { 1, 2, 0, 3 } : prefer == BubbleSide.Left ? new[] { 2, 1, 0, 3 } : new[] { 0, 1, 2, 3 };
            Rect best = default; float bestScore = float.PositiveInfinity;
            for (int rank = 0; rank < preference.Length; rank++)
            {
                Rect candidate = Clamp(new Rect(origins[preference[rank]], size));
                Vector2 wanted = origins[preference[rank]];
                float moved = Vector2.Distance(wanted, candidate.position);
                float score = rank * 140 + moved * 2.5f + Overlap(candidate) * 0.08f + (candidate.Contains(anchor) ? 4000 : 0);
                if (Overlap(candidate) > 0) score += 600;
                if (score < bestScore) { bestScore = score; best = candidate; }
            }
            return Stack(best);
        }

        private static Rect Stack(Rect rect)
        {
            if (Overlap(rect) <= 0) return rect;
            for (int step = 1; step <= 14; step++)
                foreach (float direction in new[] { -1f, 1f })
                {
                    Rect moved = Clamp(new Rect(rect.x, rect.y + direction * step * 22, rect.width, rect.height));
                    if (Overlap(moved) <= 0) return moved;
                }
            return rect;
        }

        private static Rect Clamp(Rect rect)
        {
            rect.x = Mathf.Clamp(rect.x, Bounds.xMin, Mathf.Max(Bounds.xMin, Bounds.xMax - rect.width));
            rect.y = Mathf.Clamp(rect.y, Bounds.yMin, Mathf.Max(Bounds.yMin, Bounds.yMax - rect.height));
            return rect;
        }

        private static float Overlap(Rect rect)
        {
            float area = 0;
            foreach (Rect other in placed)
            {
                float width = Mathf.Min(rect.xMax, other.xMax) - Mathf.Max(rect.xMin, other.xMin);
                float height = Mathf.Min(rect.yMax, other.yMax) - Mathf.Max(rect.yMin, other.yMin);
                if (width > 0 && height > 0) area += width * height;
            }
            return area;
        }

        private static Vector2 ClosestEdge(Rect rect, Vector2 point)
        {
            Rect inner = new Rect(rect.x + 26, rect.y + 14, Mathf.Max(1, rect.width - 52), Mathf.Max(1, rect.height - 30));
            Vector2 clamped = new Vector2(Mathf.Clamp(point.x, inner.xMin, inner.xMax), Mathf.Clamp(point.y, inner.yMin, inner.yMax));
            // push the base out to the paper's edge on the side facing the speaker
            float left = clamped.x - rect.xMin, right = rect.xMax - clamped.x, up = clamped.y - rect.yMin, down = rect.yMax - 4 - clamped.y;
            if (point.y >= rect.yMax - 4) return new Vector2(clamped.x, rect.yMax - 6);
            if (point.y <= rect.yMin) return new Vector2(clamped.x, rect.yMin + 2);
            if (point.x <= rect.xMin) return new Vector2(rect.xMin + 2, clamped.y);
            if (point.x >= rect.xMax) return new Vector2(rect.xMax - 2, clamped.y);
            float nearest = Mathf.Min(Mathf.Min(left, right), Mathf.Min(up, down));
            return nearest == down ? new Vector2(clamped.x, rect.yMax - 6) : nearest == up ? new Vector2(clamped.x, rect.yMin + 2) : nearest == left ? new Vector2(rect.xMin + 2, clamped.y) : new Vector2(rect.xMax - 2, clamped.y);
        }

        private static void DrawTail(Vector2 from, Vector2 to, Color color)
        {
            Vector2 direction = to - from;
            float distance = direction.magnitude;
            if (distance < 8) return;
            direction /= distance;
            float length = Mathf.Min(distance - 3, 40);
            float angle = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
            Matrix4x4 restore = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, from);
            Color previous = GUI.color;
            GUI.color = new Color(color.r, color.g, color.b, color.a * previous.a);
            GUI.DrawTexture(new Rect(from.x - 17, from.y - 10, 34, length + 10), tail);
            GUI.color = previous;
            GUI.matrix = restore;
        }

        /// <summary>A voice with no body here: a quiet line of handwriting at the left edge, as the canon wants it (always from the left).</summary>
        private static void DrawWhisper(SpeechBubble bubble, float now, bool reduced)
        {
            float age = now - bubble.Began;
            float alpha = Mathf.Clamp01(age / 0.6f) * Mathf.Clamp01((bubble.Expires - now) / 0.9f) * 0.9f;
            float drift = reduced ? 0 : (1 - Mathf.Clamp01(age / 0.8f)) * -14;
            var content = new GUIContent(bubble.Text);
            float height = whisperStyle.CalcHeight(content, 470);
            Rect rect = new Rect(46 + drift, 470 - height * 0.5f, 470, height);
            Color previous = GUI.color;
            GUI.color = new Color(1, 1, 1, alpha);
            ArcadeSkin.Glow(new Rect(rect.x - 70, rect.y - 50, rect.width + 140, rect.height + 100), new Color(0.02f, 0.02f, 0.04f, 0.85f));
            whisperStyle.normal.textColor = new Color(0.86f, 0.82f, 0.9f, 1);
            GUI.Label(rect, bubble.Text, whisperStyle);
            GUI.color = previous;
            bubble.Placed = rect; bubble.Drawn = true; DrawnCount++;
        }
    }
}
