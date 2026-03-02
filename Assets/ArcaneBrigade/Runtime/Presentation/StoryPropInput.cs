using UnityEngine;

namespace ArcaneBrigade
{
    public static class StoryPropInput
    {
        /// <summary>Follow the painted, rotated sprite; use the same 1920x1080 viewport coordinates as story UI.</summary>
        public static Rect ClickRect(SpriteRenderer view,Camera camera)
        {
            Rect ink=ShopInspect.Ink(view);
            Vector3 a=camera.WorldToViewportPoint(new Vector3(ink.xMin,ink.yMin)),b=camera.WorldToViewportPoint(new Vector3(ink.xMax,ink.yMax));
            return Rect.MinMaxRect(a.x*1920-16,(1-b.y)*1080-16,b.x*1920+16,(1-a.y)*1080+16);
        }
    }
}
