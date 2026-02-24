using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class PaintedStatIcon
    {
        public ItemEffect Effect;
        public Sprite Picture, Badge;
        public Rect Ink, BadgeInk;
        public float Rotation;
        public string Source;
    }

    /// <summary>Painted game art, with authored combinations for scope, enemies and corrections. No per-frame texture creation.</summary>
    public sealed class PaintedStatIcons : ScriptableObject
    {
        public PaintedStatIcon[] Entries = Array.Empty<PaintedStatIcon>();
        private static PaintedStatIcons current;
        private Dictionary<ItemEffect, PaintedStatIcon> lookup;
        public static PaintedStatIcons Current => current != null ? current : current = Resources.Load<PaintedStatIcons>("PaintedStatIcons");
        public PaintedStatIcon Find(ItemEffect effect)
        {
            if (lookup == null) { lookup = new Dictionary<ItemEffect, PaintedStatIcon>(); foreach (var e in Entries) lookup[e.Effect] = e; }
            return lookup.TryGetValue(effect, out var icon) ? icon : null;
        }
        public static bool TryDraw(Rect bounds, ItemEffect effect)
        {
            var icon = Current?.Find(effect); if (icon?.Picture == null) return false;
            Rect main = bounds;
            if (icon.Badge != null) { main.width *= .84f; main.height *= .84f; }
            Draw(main, icon.Picture, icon.Ink, icon.Rotation);
            if (icon.Badge != null) Draw(new Rect(bounds.x + bounds.width * .48f, bounds.y + bounds.height * .47f, bounds.width * .52f, bounds.height * .53f), icon.Badge, icon.BadgeInk);
            return true;
        }
        private static void Draw(Rect bounds, Sprite sprite, Rect ink, float rotation = 0)
        {
            Rect source = ink.width > 0 && ink.height > 0 ? ink : new Rect(0, 0, sprite.rect.width, sprite.rect.height);
            source.x += sprite.textureRect.x; source.y += sprite.textureRect.y;
            Vector2 centre = bounds.center;
            float angle = rotation * Mathf.Deg2Rad, c = Mathf.Abs(Mathf.Cos(angle)), s = Mathf.Abs(Mathf.Sin(angle));
            float scale = Mathf.Min(bounds.width / (source.width * c + source.height * s), bounds.height / (source.width * s + source.height * c));
            bounds = new Rect(centre.x - source.width * scale * .5f, centre.y - source.height * scale * .5f, source.width * scale, source.height * scale);
            var matrix = GUI.matrix;
            if (rotation != 0) GUIUtility.RotateAroundPivot(rotation, centre);
            GUI.DrawTextureWithTexCoords(bounds, sprite.texture, new Rect(source.x / sprite.texture.width, source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height), true);
            GUI.matrix = matrix;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { current = null; }
    }
}
