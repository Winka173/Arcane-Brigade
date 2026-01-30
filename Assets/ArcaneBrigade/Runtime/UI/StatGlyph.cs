using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class StatGlyph
    {
        private static readonly Dictionary<ItemEffect, Texture2D> cache = new Dictionary<ItemEffect, Texture2D>();
        public static void Draw(Rect bounds, ItemEffect effect)
        {
            if (!cache.TryGetValue(effect, out Texture2D texture)) { texture = Create(effect); cache[effect] = texture; }
            GUI.DrawTexture(bounds, texture, ScaleMode.ScaleToFit, true);
        }
        private static Texture2D Create(ItemEffect effect)
        {
            var segments = new List<Vector4>();
            void Stroke(float startX, float startY, float endX, float endY) { segments.Add(new Vector4(startX, startY, endX, endY)); }
            Color tint = new Color(1, 0.86f, 0.45f);
            if (effect == ItemEffect.ChestReach)
            { Stroke(0.2f, 0.25f, 0.8f, 0.25f); Stroke(0.2f, 0.25f, 0.2f, 0.7f); Stroke(0.8f, 0.25f, 0.8f, 0.7f); Stroke(0.2f, 0.7f, 0.8f, 0.7f); Stroke(0.1f, 0.5f, 0.32f, 0.5f); Stroke(0.68f, 0.5f, 0.9f, 0.5f); Stroke(0.5f, 0.44f, 0.5f, 0.6f); }
            else if (effect == ItemEffect.Health || effect == ItemEffect.Heal || effect == ItemEffect.HeartHealing || effect == ItemEffect.HealthCorrection || effect == ItemEffect.HealCorrection)
            { tint = new Color(1f, 0.62f, 0.45f); Stroke(0.18f, 0.5f, 0.82f, 0.5f); Stroke(0.5f, 0.18f, 0.5f, 0.82f); }
            else if (effect == ItemEffect.BulletSplit || effect == ItemEffect.Projectiles)
            { Stroke(0.18f, 0.5f, 0.5f, 0.5f); for (int branch = -1; branch <= 1; branch++) Stroke(0.5f, 0.5f, 0.82f, 0.5f + branch * 0.26f); }
            else if (effect == ItemEffect.BulletPierce || effect == ItemEffect.AttackRange || effect == ItemEffect.BulletDistance)
            { Stroke(0.18f, 0.5f, 0.82f, 0.5f); Stroke(0.82f, 0.5f, 0.66f, 0.34f); Stroke(0.82f, 0.5f, 0.66f, 0.66f); Stroke(0.5f, 0.18f, 0.5f, 0.82f); }
            else if (effect == ItemEffect.BulletBounce)
            { Stroke(0.18f, 0.25f, 0.5f, 0.75f); Stroke(0.5f, 0.75f, 0.82f, 0.25f); Stroke(0.18f, 0.84f, 0.82f, 0.84f); }
            else if (effect == ItemEffect.MoveSpeed || effect == ItemEffect.BulletSpeed || effect == ItemEffect.AttackSpeed || effect == ItemEffect.SpeedCorrection || effect == ItemEffect.CompanionAttackSpeed || effect == ItemEffect.CollisionFrequency)
            { tint = ArcadeSkin.Teal; for (int chevron = 0; chevron < 2; chevron++) { float shift = chevron * 0.24f; Stroke(0.2f + shift, 0.25f, 0.43f + shift, 0.5f); Stroke(0.43f + shift, 0.5f, 0.2f + shift, 0.75f); } }
            else if (effect == ItemEffect.Armor || effect == ItemEffect.ArmorCorrection || effect == ItemEffect.Interceptor)
            { Vector2[] points = { new Vector2(0.2f, 0.8f), new Vector2(0.8f, 0.8f), new Vector2(0.72f, 0.4f), new Vector2(0.5f, 0.18f), new Vector2(0.28f, 0.4f) }; for (int index = 0; index < points.Length; index++) { Vector2 start = points[index], end = points[(index + 1) % points.Length]; Stroke(start.x, start.y, end.x, end.y); } }
            else if (effect == ItemEffect.Damage || effect == ItemEffect.MeleeDamage || effect == ItemEffect.ChampionDamage || effect == ItemEffect.CompanionDamage || effect == ItemEffect.Knockback || effect == ItemEffect.CollisionDamage)
            { Stroke(0.2f, 0.2f, 0.8f, 0.8f); Stroke(0.25f, 0.48f, 0.48f, 0.25f); }
            else
            {
                int points = effect == ItemEffect.Crit || effect == ItemEffect.ScoreMultiplier || effect == ItemEffect.Luck ? 8 : 16;
                for (int index = 0; index < points; index++)
                {
                    float angle = index * Mathf.PI * 2 / points, next = (index + 1) * Mathf.PI * 2 / points;
                    float radius = points == 8 && index % 2 == 1 ? 0.14f : 0.32f, nextRadius = points == 8 && (index + 1) % 2 == 1 ? 0.14f : 0.32f;
                    Stroke(0.5f + Mathf.Cos(angle) * radius, 0.5f + Mathf.Sin(angle) * radius, 0.5f + Mathf.Cos(next) * nextRadius, 0.5f + Mathf.Sin(next) * nextRadius);
                }
                if (effect == ItemEffect.Income || effect == ItemEffect.Discount || effect == ItemEffect.Interest) Stroke(0.5f, 0.32f, 0.5f, 0.68f);
                else Stroke(0.37f, 0.5f, 0.63f, 0.5f);
            }
            const int size = 96;
            var pixels = new Color[size * size];
            for (int vertical = 0; vertical < size; vertical++)
                for (int horizontal = 0; horizontal < size; horizontal++)
                {
                    Vector2 point = new Vector2(horizontal + 0.5f, vertical + 0.5f) / size;
                    float distance = 1;
                    foreach (Vector4 segment in segments)
                    {
                        Vector2 start = new Vector2(segment.x, segment.y), direction = new Vector2(segment.z - segment.x, segment.w - segment.y);
                        Vector2 closest = start + direction * Mathf.Clamp01(Vector2.Dot(point - start, direction) / Mathf.Max(0.00001f, direction.sqrMagnitude));
                        distance = Mathf.Min(distance, Vector2.Distance(point, closest));
                    }
                    Color pixel = tint; pixel.a = Mathf.Clamp01((0.045f - distance) * size); pixels[vertical * size + horizontal] = pixel;
                }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "VectorStatGlyph_" + effect, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixels(pixels); texture.Apply(false, true); return texture;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { foreach (Texture2D texture in cache.Values) Object.Destroy(texture); cache.Clear(); }
    }
}
