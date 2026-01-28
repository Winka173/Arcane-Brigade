using UnityEngine;

namespace ArcaneBrigade
{
    public static class WorldSpriteGrounding
    {
        public static Vector3 Foot(Sprite sprite)
        {
            Rect ink = SpriteInkMetrics.BoundsOf(sprite);
            return new Vector3(ink.center.x, ink.yMin, 0);
        }
        public static Vector3 Seat(Sprite sprite)
        {
            Rect ink = SpriteInkMetrics.BoundsOf(sprite);
            float fraction = .72f;
            if (sprite != null && sprite.texture.name == "MarketStands_v1")
            {
                string suffix = sprite.name.Substring(Mathf.Max(0, sprite.name.Length - 2));
                if (int.TryParse(suffix, out int frame)) fraction = frame / 4 == 1 ? .63f : frame / 4 == 2 ? .64f : .66f;
            }
            return new Vector3(ink.center.x, Mathf.Lerp(ink.yMin, ink.yMax, fraction), 0);
        }
        public static void Place(SpriteRenderer renderer, Vector3 contact)
        {
            renderer.transform.position = contact - renderer.transform.TransformVector(Foot(renderer.sprite));
        }
        public static int SortAt(float groundY) => 1000 - Mathf.RoundToInt(groundY * 12);
    }
}
