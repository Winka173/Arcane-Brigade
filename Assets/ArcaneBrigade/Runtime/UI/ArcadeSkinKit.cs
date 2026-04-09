using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum KitButton { Primary, Secondary, Danger }
    public enum KitRound { Brass, Walnut, Oxblood, Disabled }
    public enum KitMedallion { Close, Back, Info, Pause }
    public enum KitFrame { Dialogue, Card, Tooltip, Plaque, WaveBanner }
    public enum KitDivider { Lantern, Clock, Key }
    public enum KitCounter { Gold, Egg, Wave }

    /// <summary>The painted UI kit (play-test 20/10/2026, item 13): the image agent's UI_Buttons / UI_Panels / UI_Controls / UI_HUD /
    /// UI_MenuBackdrop sheets, cut into Resources/UI/Kit (scratchpad uikit_cut.py) with their 9-slice borders in the importer.
    /// Every painted button, frame, divider, tab, toggle, slider, scrollbar, medallion and HUD plate is drawn here; when a piece
    /// is missing the old lantern-glass drawing is used instead. Texture pixels are 1080p screen pixels at scale 1.
    /// Ornaments in the middle of an edge (button crests, the tooltip's tail, the divider's lantern) are kept unstretched.</summary>
    public static partial class ArcadeSkin
    {
        private static readonly Dictionary<string, Sprite> kit = new Dictionary<string, Sprite>();
        public static Sprite KitSprite(string path)
        {
            if (!kit.TryGetValue(path, out Sprite sprite)) kit[path] = sprite = Resources.Load<Sprite>("UI/Kit/" + path);
            return sprite;
        }
        public static bool HasKit => KitSprite("Panels/Dialogue") != null;

        // rows above the button's face (the crest and the curls), measured on the art by the cutter
        private static float BodyTop(KitButton kind) => kind == KitButton.Primary ? .336f : kind == KitButton.Secondary ? .30f : .345f;
        private const float ButtonCrest = .40f, TabCrest = .55f, TabTop = .40f, DividerCentre = .46f, TooltipTail = .14f;
        private static readonly string[] States = { "Normal", "Hover", "Pressed", "Disabled" };

        // label colours per plate, checked against the painted faces (WCAG contrast, face sampled at its centre):
        // gold face ~(213,162,54) with ink 7:1; walnut ~(90,45,18) with cream 10:1; oxblood ~(125,12,20) with pale rose 9:1;
        // navy dialogue ~(14,39,58) with cream 12:1; cream card ~(247,226,190) with brown ink 12:1
        public static readonly Color KitInk = new Color(.20f, .10f, .04f), KitCream = new Color(.98f, .93f, .82f), KitRose = new Color(1f, .92f, .86f);
        public static Color KitText(KitButton kind, bool enabled = true) =>
            kind == KitButton.Primary ? (enabled ? KitInk : new Color(.30f, .24f, .18f, .85f))
            : kind == KitButton.Danger ? (enabled ? KitRose : new Color(.88f, .78f, .74f, .8f))
            : enabled ? KitCream : new Color(.78f, .73f, .66f, .8f);

        /// <summary>Nine-slice of a kit sprite, its borders scaled by <paramref name="scale"/> (shrunk further when the rectangle is
        /// too small for them); <paramref name="centre"/> (a fraction of the width) is a middle column drawn unstretched.</summary>
        public static void Sliced(Rect dest, Sprite sprite, float scale, float centre = 0)
        {
            if (sprite == null) return;
            SlicedTexture(dest, sprite.texture, sprite.border, scale, centre);
        }

        /// <summary>Nine-slice of any texture with borders (left, bottom, right, top) in texture pixels.</summary>
        private static void SlicedTexture(Rect dest, Texture2D texture, Vector4 b, float scale, float centre)
        {
            if (texture == null || Event.current.type != EventType.Repaint || dest.width <= 0 || dest.height <= 0) return;
            float W = texture.width, H = texture.height;
            float C = centre * W;
            float k = scale;
            if (b.x + b.z + C > 0) k = Mathf.Min(k, dest.width / (b.x + b.z + C));
            if (b.y + b.w > 0) k = Mathf.Min(k, dest.height / (b.y + b.w));
            // columns: left cap, stretch, [fixed centre, stretch], right cap
            var xs = new List<float>(6); var us = new List<float>(6);
            xs.Add(dest.x); us.Add(0);
            xs.Add(dest.x + b.x * k); us.Add(b.x / W);
            if (C > 0)
            {
                xs.Add(dest.center.x - C * k * .5f); us.Add((W - C) * .5f / W);
                xs.Add(dest.center.x + C * k * .5f); us.Add((W + C) * .5f / W);
            }
            xs.Add(dest.xMax - b.z * k); us.Add((W - b.z) / W);
            xs.Add(dest.xMax); us.Add(1);
            float[] ys = { dest.y, dest.y + b.w * k, dest.yMax - b.y * k, dest.yMax };
            float[] vs = { 1, 1 - b.w / H, b.y / H, 0 };
            for (int i = 0; i + 1 < xs.Count; i++)
                for (int j = 0; j < 3; j++)
                {
                    Rect part = new Rect(xs[i], ys[j], xs[i + 1] - xs[i], ys[j + 1] - ys[j]);
                    if (part.width <= .01f || part.height <= .01f) continue;
                    // overlap the next patch a little: at fractional positions a hairline gap showed the background through
                    if (i + 2 < xs.Count) part.width += .75f;
                    if (j < 2) part.height += .75f;
                    GUI.DrawTextureWithTexCoords(part, texture, new Rect(us[i], vs[j + 1], us[i + 1] - us[i], vs[j] - vs[j + 1]), true);
                }
        }

        /// <summary>A kit sprite fitted whole into the rectangle (aspect kept, centred).</summary>
        public static Rect Fitted(Rect r, Sprite sprite, float grow = 1)
        {
            if (sprite == null) return r;
            float aspect = sprite.rect.width / sprite.rect.height;
            float w = Mathf.Min(r.width, r.height * aspect) * grow, h = w / aspect;
            Rect at = new Rect(r.center.x - w * .5f, r.center.y - h * .5f, w, h);
            if (Event.current.type == EventType.Repaint) GUI.DrawTexture(at, sprite.texture, ScaleMode.StretchToFill, true);
            return at;
        }

        // ---------------------------------------------------------------- buttons
        private static int State(Rect r, bool enabled)
        {
            if (!enabled) return 3;
            Event e = Event.current;
            bool hover = r.Contains(e.mousePosition);
            return hover && GUIUtility.hotControl != 0 ? 2 : hover ? 1 : 0;
        }

        /// <summary>The painted button: the rectangle is its face; the crest and the scroll caps bleed outside it. Below 40 px
        /// tall, or where the painting cannot size cleanly, the code-drawn plate in the same tone is drawn instead.</summary>
        public static void ButtonArt(Rect r, KitButton kind, int state)
        {
            state = Mathf.Clamp(state, 0, 3);
            if (r.height >= 40 && DrawPiece(r, KitPiece.Button, "Buttons/" + kind + States[state])) return;
            CodePlate(r, kind == KitButton.Primary ? CodeTone.Gold : kind == KitButton.Danger ? CodeTone.Oxblood : CodeTone.Walnut, state);
        }

        private static GUIStyle kitLabel, kitShadow;
        private static GUIStyle KitLabel(GUIStyle like)
        {
            if (kitLabel == null)
            {
                kitLabel = ArcadeTypography.Style(24, true); kitLabel.alignment = TextAnchor.MiddleCenter; kitLabel.wordWrap = false; kitLabel.clipping = TextClipping.Overflow;
                kitShadow = new GUIStyle(kitLabel);
            }
            Font font = like != null ? like.font : null;
            kitLabel.font = kitShadow.font = font != null ? font : ArcadeTypography.Body;
            kitLabel.fontSize = kitShadow.fontSize = like != null && like.fontSize > 0 ? like.fontSize : 24;
            kitLabel.wordWrap = kitShadow.wordWrap = like != null && like.wordWrap;
            return kitLabel;
        }

        /// <summary>Text on a painted plate: a soft dark (or, on gold, light) offset under it so it reads on the brushwork.</summary>
        public static void KitCaption(Rect r, string text, Color colour, GUIStyle like = null, bool centred = false)
        {
            GUIStyle label = KitLabel(like);
            if (like != null && !centred) label.alignment = kitShadow.alignment = like.alignment; // a button label is always centred
            bool onGold = colour.grayscale < .35f;
            kitShadow.normal.textColor = onGold ? new Color(1f, .93f, .72f, .45f * colour.a) : new Color(0, 0, 0, .6f * colour.a);
            GUI.Label(new Rect(r.x + (onGold ? 0 : 1), r.y + (onGold ? 1 : 2), r.width, r.height), text, kitShadow);
            label.normal.textColor = colour;
            GUI.Label(r, text, label);
            label.alignment = kitShadow.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>A painted button (primary gold / secondary walnut / danger oxblood) with hover, pressed and disabled art and
        /// its label coloured for that plate. The rectangle is the tap area (keep it at least 64 px tall on phones).</summary>
        public static bool Button(Rect r, string label, KitButton kind = KitButton.Secondary, bool enabled = true, GUIStyle like = null)
        {
            int state = State(r, enabled);
            ButtonArt(r, kind, state);
            Color text = KitText(kind, enabled);
            if (state == 1 && kind == KitButton.Secondary) text = new Color(1f, .88f, .60f);
            Rect inner = Content(r, KitPiece.Button, false); // inside the caps, vertically centred on the face
            if (!string.IsNullOrEmpty(label)) KitCaption(new Rect(inner.x, r.y, inner.width, r.height), label, text, like, true);
            return enabled && GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        /// <summary>A small round painted button (brass / walnut / oxblood medallion base) with a glyph or a picture in it.</summary>
        public static bool IconButton(Rect r, KitRound kind, string glyph = null, Sprite icon = null, bool enabled = true, GUIStyle like = null)
        {
            int state = State(r, enabled);
            if (state == 1) Glow(new Rect(r.x - 10, r.y - 10, r.width + 20, r.height + 20), new Color(Gold.r, Gold.g, Gold.b, .35f));
            Rect face = RoundArt(r, enabled ? kind : KitRound.Disabled, state);
            if (icon != null) RunHud.Sprite(face, icon);
            if (!string.IsNullOrEmpty(glyph))
                KitCaption(face, glyph, kind == KitRound.Brass ? KitInk : KitCream, like, true);
            return enabled && GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        /// <summary>The close / back / info / pause medallions (brass with an ink mark).</summary>
        public static bool Medallion(Rect r, KitMedallion which, bool lit = false)
        {
            MedallionArt(r, which, State(r, true), lit);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        // ---------------------------------------------------------------- frames, plaques, dividers
        /// <summary>A painted frame: the navy dialogue panel, the cream card with lantern corners, the tooltip bubble (tail at the
        /// bottom centre), the velvet title plaque or the wave banner. Text colours: cream on Dialogue / Plaque / WaveBanner, brown
        /// ink on Card / Tooltip.</summary>
        public static void Frame(Rect r, KitFrame frame, float scale = 1)
        {
            KitPiece piece = PieceOf(frame);
            if (DrawPiece(r, piece, KeyOf(piece), frame == KitFrame.Plaque || frame == KitFrame.WaveBanner ? 99 : scale)) return;
            if (frame == KitFrame.Tooltip) Diamond(new Vector2(r.center.x, r.yMax - 2), Mathf.Min(26, r.height * .3f), new Color(.95f, .89f, .76f)); // the tail, under the plate
            CodePlate(r, frame == KitFrame.Card || frame == KitFrame.Tooltip ? CodeTone.Cream : frame == KitFrame.Dialogue ? CodeTone.Navy : CodeTone.Oxblood);
            if (frame == KitFrame.Plaque || frame == KitFrame.WaveBanner)
            {
                float s = Mathf.Min(22, r.height * .42f);
                Diamond(new Vector2(r.x + 1, r.center.y), s, GiltLight); Diamond(new Vector2(r.xMax - 1, r.center.y), s, GiltLight);
            }
        }
        public static KitPiece PieceOf(KitFrame frame) =>
            frame == KitFrame.Card ? KitPiece.Card : frame == KitFrame.Tooltip ? KitPiece.Tooltip : frame == KitFrame.Plaque ? KitPiece.Plaque : frame == KitFrame.WaveBanner ? KitPiece.WaveBanner : KitPiece.Dialogue;
        /// <summary>The usable inner area of a frame drawn with <see cref="Frame"/> at the same rectangle and scale.</summary>
        public static Rect Content(Rect r, KitFrame frame, bool pad = true, float scale = 1) =>
            Content(r, PieceOf(frame), pad, frame == KitFrame.Plaque || frame == KitFrame.WaveBanner ? 99 : scale);

        public static Color FrameText(KitFrame frame) => frame == KitFrame.Card || frame == KitFrame.Tooltip ? new Color(.20f, .13f, .08f) : KitCream;

        /// <summary>A gilt ornamental rule with a small lantern, clock or key in the middle (height sets its size).</summary>
        public static void Ornament(Rect r, KitDivider which = KitDivider.Lantern)
        {
            Sprite sprite = Crafted ? null : KitSprite("Panels/Divider" + which);
            if (sprite == null) { if (HasKit) CraftedDivider(r); else Divider(new Rect(r.x, r.center.y, r.width, 2), Accent); return; }
            float k = r.height / sprite.texture.height;
            float minimum = (sprite.border.x + sprite.border.z + DividerCentre * sprite.texture.width) * k;
            if (r.width < minimum) { k *= r.width / minimum; r = new Rect(r.x, r.center.y - sprite.texture.height * k * .5f, r.width, sprite.texture.height * k); }
            Sliced(r, sprite, k, DividerCentre);
        }

        // ---------------------------------------------------------------- tabs, toggles, slider, scrollbar
        /// <summary>A painted tab (velvet when active, walnut when not, brass under the pointer, grey when disabled).</summary>
        public static bool Tab(Rect r, string label, bool active, bool enabled = true, GUIStyle like = null)
        {
            int state = State(r, enabled);
            string art = !enabled ? "TabDisabled" : active ? "TabActive" : state >= 1 ? "TabHover" : "TabInactive";
            if (!DrawPiece(r, KitPiece.Tab, "Controls/" + art)) CodePlate(r, active ? CodeTone.Oxblood : art == "TabHover" ? CodeTone.Gold : CodeTone.Walnut, enabled ? 0 : 3);
            Color text = !enabled ? new Color(.80f, .76f, .70f, .8f) : art == "TabHover" ? KitInk : KitCream;
            Rect inner = Content(r, KitPiece.Tab, false);
            if (!string.IsNullOrEmpty(label)) KitCaption(new Rect(inner.x, r.y, inner.width, r.height), label, text, like, true);
            return enabled && GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        /// <summary>A painted switch (dark velvet off, lit lantern on) with its label to the right. The whole row is the tap area.</summary>
        public static bool Toggle(Rect r, bool value, string label, GUIStyle like = null)
        {
            Sprite sprite = Crafted ? null : KitSprite("Controls/" + (value ? "ToggleOn" : "ToggleOff"));
            float h = Mathf.Min(r.height, 44), w = h * 1.8f;
            Rect knob = new Rect(r.x, r.center.y - h * .5f, w, h);
            if (sprite != null) Fitted(knob, sprite);
            else
            {
                // crafted switch: a rounded track (lit gold when on) and a round knob that slides
                CodePlate(new Rect(knob.x, knob.y + h * .12f, w, h * .76f), value ? CodeTone.Gold : CodeTone.Walnut);
                CodeRound(new Rect(value ? knob.xMax - h : knob.x, knob.y, h, h), value ? CodeTone.Cream : CodeTone.Navy);
            }
            if (!string.IsNullOrEmpty(label))
            {
                GUIStyle style = KitLabel(like); style.alignment = TextAnchor.MiddleLeft;
                style.normal.textColor = like != null ? like.normal.textColor : Cream;
                GUI.Label(new Rect(knob.xMax + 12, r.y, r.width - knob.width - 12, r.height), label, style);
                style.alignment = TextAnchor.MiddleCenter;
            }
            return GUI.Button(r, GUIContent.none, GUIStyle.none) ? !value : value;
        }

        /// <summary>A painted checkbox (velvet square in a brass frame, an ink tick when checked).</summary>
        public static void CheckBox(Rect r, bool value)
        {
            Sprite sprite = Crafted ? null : KitSprite("Controls/" + (value ? "CheckTicked" : "CheckEmpty"));
            if (sprite != null) { Fitted(r, sprite); return; }
            float size = Mathf.Min(r.width, r.height); Rect box = new Rect(r.center.x - size * .5f, r.center.y - size * .5f, size, size);
            CodePlate(box, CodeTone.Navy);
            if (value) { float w = Mathf.Max(2.5f, size * .12f); Stroke(new Vector2(box.x + size * .26f, box.y + size * .52f), new Vector2(box.x + size * .43f, box.y + size * .7f), w, KitCream); Stroke(new Vector2(box.x + size * .43f, box.y + size * .7f), new Vector2(box.x + size * .76f, box.y + size * .3f), w, KitCream); }
        }

        /// <summary>The scroll thumb of a ScrollPanel: a velvet lozenge in brass along a thin walnut rail.</summary>
        public static void ScrollBar(Rect track, Rect thumb)
        {
            Sprite rail = KitSprite("Controls/ScrollTrack"), grip = KitSprite("Controls/ScrollThumb");
            if (rail == null || grip == null) { Line(new Rect(thumb.center.x - 2.5f, thumb.y, 5, thumb.height), new Color(Gold.r, Gold.g, Gold.b, .75f)); return; }
            if (Crafted) { CodePlate(track, CodeTone.Navy); CodePlate(thumb, CodeTone.Gold); return; }
            Sliced(track, rail, track.width / rail.texture.width);
            Sliced(thumb, grip, thumb.width / grip.texture.width);
        }

        private static GUIStyle KitSliderStyle(GUIStyle source, out GUIStyle thumb)
        {
            thumb = null;
            Sprite rail = KitSprite("Controls/SliderTrack"), knob = KitSprite("Controls/SliderKnob");
            if (rail == null || knob == null) return null;
            if (Crafted)
            {
                var rod = new GUIStyle(source) { border = new RectOffset(13, 13, 13, 13), fixedHeight = 22, overflow = new RectOffset(0, 0, 0, 0) };
                rod.normal.background = rod.hover.background = rod.active.background = rod.focused.background = CodeTexture(CodeTone.Navy, 0, true);
                thumb = new GUIStyle(GUIStyle.none) { fixedWidth = 32, fixedHeight = 32, overflow = new RectOffset(0, 0, 5, 5) };
                thumb.normal.background = thumb.focused.background = CodeRoundTexture(CodeTone.Gold, 0);
                thumb.hover.background = thumb.active.background = CodeRoundTexture(CodeTone.Gold, 1);
                return rod;
            }
            var track = new GUIStyle(source) { border = new RectOffset((int)rail.border.x, (int)rail.border.z, 0, 0), fixedHeight = 30, overflow = new RectOffset(0, 0, 0, 0) };
            track.normal.background = track.hover.background = track.active.background = track.focused.background = rail.texture;
            thumb = new GUIStyle(GUIStyle.none) { fixedWidth = 26, fixedHeight = 42, overflow = new RectOffset(0, 0, 6, 6) };
            thumb.normal.background = thumb.hover.background = thumb.active.background = thumb.focused.background = knob.texture;
            return track;
        }

        private static GUIStyle KitScrollStyle(GUIStyle source, out GUIStyle thumb)
        {
            thumb = null;
            Sprite rail = KitSprite("Controls/ScrollTrack"), grip = KitSprite("Controls/ScrollThumb");
            if (rail == null || grip == null) return null;
            if (Crafted)
            {
                var rod = new GUIStyle(source) { border = new RectOffset(13, 13, 13, 13), fixedWidth = 16 };
                rod.normal.background = CodeTexture(CodeTone.Navy, 0, true);
                thumb = new GUIStyle(source) { border = new RectOffset(13, 13, 13, 13), fixedWidth = 16 };
                thumb.normal.background = thumb.hover.background = thumb.active.background = CodeTexture(CodeTone.Gold, 0, true);
                return rod;
            }
            var track = new GUIStyle(source) { border = new RectOffset(0, 0, (int)rail.border.w, (int)rail.border.y), fixedWidth = 16 };
            track.normal.background = rail.texture;
            thumb = new GUIStyle(source) { border = new RectOffset(0, 0, (int)grip.border.w, (int)grip.border.y), fixedWidth = 16 };
            thumb.normal.background = thumb.hover.background = thumb.active.background = grip.texture;
            return track;
        }

        /// <summary>The skin's own GUI.Button look: the painted plate without its crest (a style's nine-slice would stretch it).</summary>
        private static GUIStyle KitButtonStyle(GUIStyle source, KitButton kind)
        {
            if (KitSprite("Buttons/" + kind + "NormalPlain") != null) return CodeButtonStyle(source, kind); // painted plates come via Fit()
            Sprite normal = KitSprite("Buttons/" + kind + "NormalPlain");
            if (normal == null) return null;
            Sprite hover = KitSprite("Buttons/" + kind + "HoverPlain") ?? normal, pressed = KitSprite("Buttons/" + kind + "PressedPlain") ?? normal;
            Vector4 b = normal.border;
            Rect body = BodyOf("Buttons/" + kind + "NormalPlain", normal.texture);
            int W = normal.texture.width, H = normal.texture.height;
            var style = new GUIStyle(source)
            {
                border = new RectOffset((int)b.x, (int)b.z, (int)b.w, (int)b.y),
                overflow = new RectOffset(Mathf.RoundToInt(body.x), Mathf.RoundToInt(W - body.xMax), Mathf.RoundToInt(body.y), Mathf.RoundToInt(H - body.yMax)),
                padding = new RectOffset(18, 18, 4, 4), clipping = TextClipping.Overflow, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter
            };
            style.normal.background = style.onNormal.background = normal.texture;
            style.hover.background = style.onHover.background = style.focused.background = hover.texture;
            style.active.background = style.onActive.background = pressed.texture;
            Color text = KitText(kind);
            style.normal.textColor = style.onNormal.textColor = style.active.textColor = style.onActive.textColor = text;
            style.hover.textColor = style.onHover.textColor = style.focused.textColor = kind == KitButton.Secondary ? new Color(1f, .88f, .60f) : text;
            return style;
        }

        private static GUIStyle danger;
        /// <summary>The oxblood button style (destructive choices: abandon a run, a blood price). Size/font copied from <paramref name="like"/>.</summary>
        public static GUIStyle DangerStyle(GUIStyle like)
        {
            GUISkin _ = Skin;
            if (danger == null) danger = KitButtonStyle(skin.button, KitButton.Danger) ?? new GUIStyle(skin.button);
            return new GUIStyle(danger) { font = like.font, fontSize = like.fontSize, fontStyle = like.fontStyle };
        }

        // ---------------------------------------------------------------- HUD plates
        /// <summary>The health bar: a brass tube with the heart cap at its left, filled with glossy crimson (the trail behind it
        /// shows recent damage). The heart cap stands left of the rectangle's tube, so pass the whole bar.</summary>
        public static Rect HealthBar(Rect r, float fraction, float trail)
        {
            Sprite frame = KitSprite("HUD/HpFrame"), fill = KitSprite("HUD/HpFill");
            if (frame == null || fill == null) return r;
            if (Crafted) return CraftedHealthBar(r, fraction, trail);
            float k = r.height / frame.texture.height, W = frame.texture.width, H = frame.texture.height;
            // the open tube, measured on the art: x 97..443 of 488 at full length, y 44..76 of 108
            float left = r.x + 97 * k, right = r.xMax - (W - 443) * k;
            Rect tube = new Rect(left - 4 * k, r.y + 42 * k, right - left + 8 * k, 36 * k);
            if (Event.current.type == EventType.Repaint)
            {
                Line(tube, new Color(.10f, .02f, .03f, .92f));
                Color keep = GUI.color;
                if (trail > fraction) { GUI.color = new Color(keep.r * 1f, keep.g * .75f, keep.b * .55f, keep.a * .55f); Sliced(new Rect(tube.x, tube.y, tube.width * Mathf.Clamp01(trail), tube.height), fill, tube.height / fill.texture.height); GUI.color = keep; }
                if (fraction > 0) Sliced(new Rect(tube.x, tube.y, Mathf.Max(tube.height, tube.width * Mathf.Clamp01(fraction)), tube.height), fill, tube.height / fill.texture.height);
            }
            Sliced(r, frame, k);
            return tube;
        }

        /// <summary>A walnut counter plate with a round socket on its left (gold, eggs, wave). Returns the socket for the icon;
        /// <paramref name="text"/> is the rest of the plate (cream reads on the walnut).</summary>
        public static Rect Counter(Rect r, KitCounter which, out Rect text)
        {
            Sprite sprite = KitSprite("HUD/Counter" + which);
            if (sprite != null && Crafted) return CraftedCounter(r, out text);
            if (sprite == null) { Plate(r, which == KitCounter.Gold ? Gold : Teal); text = new Rect(r.x + 60, r.y, r.width - 66, r.height); return new Rect(r.x + 10, r.y + 4, r.height - 8, r.height - 8); }
            float k = r.height / sprite.texture.height;
            Sliced(r, sprite, k);
            // the socket is centred 36 px in on an 80 px plate, inner diameter about 40
            Rect socket = new Rect(r.x + 36 * k - 20 * k, r.center.y - 20 * k, 40 * k, 40 * k);
            text = new Rect(r.x + sprite.border.x * k, r.y, r.width - (sprite.border.x + sprite.border.z * .6f) * k, r.height);
            return socket;
        }

        /// <summary>A round brass portrait slot on velvet (lit: gold glow, the hero you are looking at).</summary>
        public static void PortraitSlot(Rect r, bool lit)
        {
            if (Crafted) { if (lit) Glow(new Rect(r.x - 10, r.y - 10, r.width + 20, r.height + 20), new Color(Gold.r, Gold.g, Gold.b, .45f)); CodeRound(r, CodeTone.Navy, lit ? 1 : 0); return; }
            Sprite s = KitSprite("HUD/" + (lit ? "PortraitLit" : "Portrait")); if (s != null) Fitted(r, s);
        }

        /// <summary>The painted menu background: the waiting room at night, cropped to fill the screen.</summary>
        public static bool MenuBackdrop(Rect r, float alpha = 1)
        {
            Sprite sprite = KitSprite("Menu/Backdrop");
            if (sprite == null || Event.current.type != EventType.Repaint) return sprite != null;
            Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * alpha);
            GUI.DrawTexture(r, sprite.texture, ScaleMode.ScaleAndCrop, false);
            GUI.color = keep;
            return true;
        }

        /// <summary>Hook for the secret-event room's plates (UI_SecretEvent sheet, still to be painted): Resources/UI/Kit/SecretEvent/
        /// Plate, PlateLit and PlateOff when they exist; until then the walnut plate. Returns true when the painted plate was used.</summary>
        public static bool EventPlate(Rect r, bool selected, bool enabled)
        {
            if (!Crafted && KitSprite("SecretEvent/Plate") != null)
            {
                // the painted cream plate with its badge socket (the socket stands above the rect); selected: the gilt-lit plate
                Color keep = GUI.color; if (!enabled) GUI.color = new Color(keep.r * .62f, keep.g * .62f, keep.b * .62f, keep.a);
                bool drawn = DrawPiece(r, KitPiece.EventPlate, selected && enabled ? "SecretEvent/PlateLit" : "SecretEvent/Plate");
                GUI.color = keep;
                if (drawn && selected && enabled) MarkLit(r);
                if (drawn) return true;
            }
            Plate(r, !enabled ? new Color(.45f, .43f, .42f) : selected ? Gold : Accent);
            return false;
        }

        /// <summary>The inner area of an event plate (the painted plate when it lands, else the dialogue frame Plate draws).</summary>
        public static Rect EventContent(Rect r)
        {
            return !Crafted && KitSprite("SecretEvent/Plate") != null ? Content(r, KitPiece.EventPlate) : r.height > 110 && r.width > 160 ? Content(r, KitPiece.Dialogue) : Content(r, KitPiece.Plate);
        }
    }
}
