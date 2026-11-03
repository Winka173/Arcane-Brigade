using UnityEngine;

namespace ArcaneBrigade
{
    public static partial class ArcadeSkin
    {
        /// <summary>The picked hero card, collection shelf or friend medallion: a small wax seal stamped inside its top-right corner,
        /// still (requirement 4/11/2026: a turning magic circle round it was cut by frames and scroll edges and looked broken).
        /// Only for pieces with no other way to show the pick: rows and cards that change colour or lift do not take it. Call it
        /// after the piece's own art, in the same GUI space.</summary>
        public static void PickedSeal(Rect piece)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || piece.width <= 0 || piece.height <= 0) return;
            float unit = Screen.height / 1080f / Mathf.Max(.01f, GUI.matrix.lossyScale.y);
            float size = Mathf.Clamp(Mathf.Min(piece.width, piece.height) * .3f, 30 * unit, 52 * unit);
            Vector2 at = new Vector2(piece.xMax - size * .5f - 10 * unit, piece.y + size * .5f + 10 * unit); // wholly inside the piece
            Sprite wax = KitSprite("Info/SigilSealWax");
            if (wax != null && !Crafted) GUI.DrawTexture(new Rect(at.x - size * .5f, at.y - size * .5f, size, size), wax.texture, ScaleMode.ScaleToFit);
            else Seal(at, size, "✓");
        }
    }
}
