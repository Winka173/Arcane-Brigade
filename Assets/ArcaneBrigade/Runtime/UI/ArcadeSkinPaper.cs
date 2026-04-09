using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Cream paper pieces from the image agent's UI_PaperTags sheet (Resources/UI/Kit/Paper).</summary>
    public enum PaperKind { PriceTag, PriceTagRefused, LabelTag, Receipt, LetterUnread, LetterRead }

    /// <summary>The painted pieces cut from UI_SecretEvent, UI_PaperTags and UI_InfoTiles (play-test 21/10/2026): paper tags,
    /// receipts, letters and reward tiles, navy info tiles, LOSE / GET rows, walnut chips, the secret room's banner, plates,
    /// pedestal, swap arrows, wax-seal confirm ribbon and door-tag leave button. Each returns false (drawing nothing) in the
    /// crafted style or where the painting cannot size cleanly, so the caller keeps its code-drawn fallback.</summary>
    public static partial class ArcadeSkin
    {
        /// <summary>A strip of cream paper (it keeps its aspect: scaled to the height, stretched only sideways).</summary>
        public static bool PaperArt(Rect r, PaperKind kind) => !Crafted && DrawPiece(r, KitPiece.Plate, "Paper/" + kind);

        /// <summary>A square reward tile (cream card in a brass frame; lit: warm gold rim), fitted whole.</summary>
        public static bool RewardTile(Rect r, bool lit)
        {
            Sprite s = Crafted ? null : KitSprite("Paper/RewardTile" + (lit ? "Lit" : ""));
            if (s == null) return false;
            Fitted(r, s); return true;
        }

        /// <summary>A night-navy lacquer info tile (bond / attack tiles, wide bars); lit: the keyline glows gold.</summary>
        public static bool InfoTile(Rect r, bool lit)
        {
            bool drawn = !Crafted && DrawPiece(r, KitPiece.Tile, lit ? "Info/NavyTileLit" : r.width / Mathf.Max(1, r.height) < 2.1f ? "Info/NavyTileSmall" : "Info/NavyTile");
            if (drawn && lit) MarkLit(r);
            return drawn;
        }

        // a painted lit piece already shows its selection: the straight RunningLight rim is skipped on it (code-drawn pieces keep it)
        private static Rect litPainted; private static int litFrame = -1;
        private static void MarkLit(Rect r) { litPainted = r; litFrame = Time.frameCount; }
        private static bool OnLitPainted(Rect r) => litFrame == Time.frameCount && Mathf.Abs(r.center.x - litPainted.center.x) < 10 && Mathf.Abs(r.center.y - litPainted.center.y) < 10 && Mathf.Abs(r.width - litPainted.width) < 20;

        /// <summary>Where a reward tile's number sits: centred in the painted tile's inner area, above its bottom rim.</summary>
        public static Rect RewardNumberRect(Rect r)
        {
            float aspect = 104f / 96f, w = Mathf.Min(r.width, r.height * aspect), h = w / aspect;
            Rect art = new Rect(r.center.x - w * .5f, r.center.y - h * .5f, w, h);
            Rect inner = new Rect(art.x + w * .08f, art.y + h * .09f, w * .84f, h * .82f);
            return new Rect(inner.x, inner.yMax - 26, inner.width, 24);
        }

        /// <summary>A walnut chip with a round icon socket at its left (shop chips); lit: gold rim.</summary>
        public static void Chip(Rect r, bool lit)
        {
            if (!Crafted && DrawPiece(r, KitPiece.Plate, "Info/Chip" + (lit ? "Lit" : ""))) { if (lit) MarkLit(r); return; }
            CodePlate(r, CodeTone.Walnut, lit ? 1 : 0);
        }

        /// <summary>A DO / LOSE / GET row band: cream receipt paper, oxblood with a thorn line, green-gilt with a laurel line.</summary>
        public static bool EventBand(Rect r, CodeTone tone) =>
            !Crafted && DrawPiece(r, KitPiece.Plate, tone == CodeTone.Oxblood ? "Info/LoseRow" : tone == CodeTone.Green ? "Info/GetRow" : "Paper/Receipt");

        /// <summary>The velvet title banner held by two brass hands (the hands stand outside the rect).</summary>
        public static bool BannerArt(Rect r) => !Crafted && DrawPiece(r, KitPiece.Plate, "SecretEvent/Banner");

        /// <summary>The painted relic pedestal (bell jar on a brass stand) fitted into r; false when not painted.</summary>
        public static bool PedestalArt(Rect r)
        {
            Sprite s = Crafted ? null : KitSprite("SecretEvent/Pedestal");
            if (s == null) return false;
            Fitted(r, s); return true;
        }
        /// <summary>Where the relic sits inside the painted bell jar (on its cushion).</summary>
        private static Rect PaintedSeat(Rect r)
        {
            float a = Mathf.Min(r.width, r.height), size = a * .42f;
            Rect art = new Rect(r.center.x - a * .5f, r.center.y - a * .5f, a, a);
            return new Rect(art.center.x - size * .5f, art.y + art.height * .64f - size, size, size);
        }

        /// <summary>A brass swap arrow (left or right); returns true when tapped. The rect is the tap area.</summary>
        public static bool SwapArrow(Rect r, bool left)
        {
            int state = State(r, true);
            Sprite s = Crafted ? null : KitSprite("SecretEvent/Arrow" + (left ? "Left" : "Right"));
            if (state == 1) Glow(new Rect(r.x - 8, r.y - 8, r.width + 16, r.height + 16), new Color(Gold.r, Gold.g, Gold.b, .35f));
            if (s != null) Fitted(r, s, state == 2 ? .92f : state == 1 ? 1.06f : 1);
            else { Rect face = RoundArt(r, KitRound.Brass, state); KitCaption(face, left ? "◀" : "▶", KitInk, null, true); }
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        /// <summary>The wax-sealed contract ribbon (the secret room's confirm button): the seal at its left, the words on the
        /// ribbon in cream. Disabled: the ribbon dimmed. Falls back to the gold button.</summary>
        public static bool RibbonButton(Rect r, string label, bool enabled, GUIStyle like = null)
        {
            int state = State(r, enabled);
            Color keep = GUI.color;
            if (!enabled) GUI.color = new Color(keep.r * .6f, keep.g * .6f, keep.b * .6f, keep.a);
            bool painted = !Crafted && DrawPiece(r, KitPiece.Plate, "SecretEvent/Ribbon" + (state == 2 ? "Pressed" : state == 1 ? "Hover" : "Normal"));
            GUI.color = keep;
            if (!painted) return Button(r, label, KitButton.Primary, enabled, like);
            Rect words = new Rect(r.x + r.height * 1.05f, r.y, r.width - r.height * 1.25f, r.height); // right of the seal
            KitCaption(words, label, enabled ? KitRose : new Color(.85f, .75f, .72f, .8f), like, true);
            return enabled && GUI.Button(r, GUIContent.none, GUIStyle.none);
        }

        /// <summary>The leave button: a walnut plate with a brass door tag hanging at its left (lantern on it).</summary>
        public static bool DoorTagButton(Rect r, string label, GUIStyle like = null)
        {
            int state = State(r, true);
            ButtonArt(r, KitButton.Secondary, state);
            Sprite tag = Crafted ? null : KitSprite("SecretEvent/DoorTag" + (state >= 1 ? "Hover" : "Normal"));
            float tagW = 0;
            if (tag != null) { float h = r.height * 1.15f, w = h * tag.rect.width / tag.rect.height; Fitted(new Rect(r.x + 8, r.y - r.height * .12f, w, h), tag); tagW = w + 6; }
            Rect inner = Content(r, KitPiece.Button, false);
            KitCaption(new Rect(Mathf.Max(inner.x, r.x + 8 + tagW), r.y, inner.xMax - Mathf.Max(inner.x, r.x + 8 + tagW), r.height), label, state == 1 ? new Color(1f, .88f, .60f) : KitCream, like, true);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }
    }
}
