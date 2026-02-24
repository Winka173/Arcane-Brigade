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
            void Path(params Vector2[] points) { for (int i = 1; i < points.Length; i++) Stroke(points[i - 1].x, points[i - 1].y, points[i].x, points[i].y); }
            void Circle(float x, float y, float radius) { for (int i = 0; i < 20; i++) { float a = i * Mathf.PI / 10, b = (i + 1) * Mathf.PI / 10; Stroke(x + Mathf.Cos(a) * radius, y + Mathf.Sin(a) * radius, x + Mathf.Cos(b) * radius, y + Mathf.Sin(b) * radius); } }
            Color tint = new Color(1, 0.86f, 0.45f);
            if (effect == ItemEffect.Discount || effect == ItemEffect.RecruitDiscount || effect == ItemEffect.RelicDiscount)
            { Path(new Vector2(.16f,.7f),new Vector2(.16f,.37f),new Vector2(.62f,.14f),new Vector2(.87f,.58f),new Vector2(.4f,.86f),new Vector2(.16f,.7f)); Circle(.3f,.62f,.035f); Stroke(.43f,.37f,.68f,.63f); if(effect==ItemEffect.RecruitDiscount) { Circle(.54f,.62f,.07f); } else if(effect==ItemEffect.RelicDiscount) { Stroke(.55f,.3f,.74f,.49f); } }
            else if (effect == ItemEffect.Income || effect == ItemEffect.Interest)
            { Circle(.45f,.5f,.28f); Stroke(.45f,.34f,.45f,.66f); if(effect==ItemEffect.Income) { Stroke(.75f,.4f,.75f,.82f); Stroke(.75f,.82f,.63f,.67f); Stroke(.75f,.82f,.88f,.67f); } else { Stroke(.61f,.13f,.85f,.35f); Stroke(.85f,.35f,.65f,.35f); } }
            else if (effect == ItemEffect.ShopRarity)
            { Path(new Vector2(.18f,.6f),new Vector2(.32f,.82f),new Vector2(.68f,.82f),new Vector2(.82f,.6f),new Vector2(.5f,.16f),new Vector2(.18f,.6f),new Vector2(.82f,.6f)); Stroke(.32f,.82f,.5f,.16f); Stroke(.68f,.82f,.5f,.16f); }
            else if (effect == ItemEffect.ScoreMultiplier)
            { Path(new Vector2(.28f,.82f),new Vector2(.72f,.82f),new Vector2(.68f,.46f),new Vector2(.5f,.36f),new Vector2(.32f,.46f),new Vector2(.28f,.82f)); Stroke(.5f,.36f,.5f,.18f); Stroke(.3f,.16f,.7f,.16f); Path(new Vector2(.28f,.72f),new Vector2(.12f,.72f),new Vector2(.17f,.49f),new Vector2(.33f,.46f)); Path(new Vector2(.72f,.72f),new Vector2(.88f,.72f),new Vector2(.83f,.49f),new Vector2(.67f,.46f)); }
            else if (effect == ItemEffect.CollisionDamage || effect == ItemEffect.Knockback)
            { if(effect==ItemEffect.CollisionDamage) { for(int i=0;i<6;i++) { float a=i*Mathf.PI/3; Stroke(.5f+Mathf.Cos(a)*.17f,.5f+Mathf.Sin(a)*.17f,.5f+Mathf.Cos(a)*.36f,.5f+Mathf.Sin(a)*.36f); } } else { Stroke(.12f,.5f,.68f,.5f); Stroke(.68f,.5f,.5f,.69f); Stroke(.68f,.5f,.5f,.31f); Stroke(.85f,.2f,.85f,.8f); } }
            else if (effect == ItemEffect.CollisionFrequency)
            { tint=ArcadeSkin.Teal; Circle(.5f,.5f,.29f); Stroke(.5f,.5f,.5f,.72f); Stroke(.5f,.5f,.7f,.38f); Stroke(.12f,.78f,.32f,.78f); Stroke(.12f,.78f,.12f,.58f); }
            else if (effect == ItemEffect.BulletSpeed)
            { tint=ArcadeSkin.Teal; Path(new Vector2(.43f,.35f),new Vector2(.72f,.35f),new Vector2(.87f,.5f),new Vector2(.72f,.65f),new Vector2(.43f,.65f),new Vector2(.43f,.35f)); Stroke(.1f,.38f,.28f,.38f); Stroke(.16f,.62f,.31f,.62f); Stroke(.12f,.5f,.36f,.5f); }
            else if (effect == ItemEffect.BodySize || effect == ItemEffect.CompanionSize)
            { tint=ArcadeSkin.Teal; Circle(.5f,.7f,.14f); Path(new Vector2(.22f,.17f),new Vector2(.26f,.43f),new Vector2(.5f,.52f),new Vector2(.74f,.43f),new Vector2(.78f,.17f),new Vector2(.22f,.17f)); if(effect==ItemEffect.CompanionSize) { Stroke(.37f,.75f,.3f,.9f); Stroke(.63f,.75f,.7f,.9f); } }
            else if (effect == ItemEffect.Health || effect == ItemEffect.HealthCorrection || effect == ItemEffect.EnemyHealth)
            { tint = effect == ItemEffect.EnemyHealth ? ArcadeSkin.Worse : new Color(.88f, .66f, .42f); Path(new Vector2(.5f,.16f),new Vector2(.18f,.49f),new Vector2(.16f,.69f),new Vector2(.3f,.82f),new Vector2(.5f,.68f),new Vector2(.7f,.82f),new Vector2(.84f,.69f),new Vector2(.82f,.49f),new Vector2(.5f,.16f)); if(effect==ItemEffect.HealthCorrection) Stroke(.35f,.47f,.65f,.47f); }
            else if (effect == ItemEffect.RangedDamage)
            { tint = new Color(1,.68f,.42f); Path(new Vector2(.28f,.15f),new Vector2(.48f,.3f),new Vector2(.56f,.5f),new Vector2(.48f,.7f),new Vector2(.28f,.85f),new Vector2(.28f,.15f)); Stroke(.15f,.5f,.88f,.5f); Stroke(.88f,.5f,.72f,.66f); Stroke(.88f,.5f,.72f,.34f); }
            else if (effect == ItemEffect.TreasureDetection || effect == ItemEffect.ChestQuality)
            { tint = ArcadeSkin.Gold; Path(new Vector2(.16f,.23f),new Vector2(.84f,.23f),new Vector2(.84f,.64f),new Vector2(.7f,.79f),new Vector2(.3f,.79f),new Vector2(.16f,.64f),new Vector2(.16f,.23f)); Stroke(.16f,.55f,.84f,.55f); Stroke(.5f,.44f,.5f,.62f); if(effect==ItemEffect.ChestQuality) { Stroke(.38f,.89f,.62f,.89f); } }
            else if (effect == ItemEffect.MoveSpeed || effect == ItemEffect.SpeedCorrection || effect == ItemEffect.EnemySpeed)
            { tint = effect == ItemEffect.EnemySpeed ? ArcadeSkin.Worse : ArcadeSkin.Teal; Path(new Vector2(.36f,.85f),new Vector2(.65f,.85f),new Vector2(.6f,.43f),new Vector2(.85f,.32f),new Vector2(.85f,.18f),new Vector2(.26f,.18f),new Vector2(.26f,.36f),new Vector2(.36f,.48f),new Vector2(.36f,.85f)); Stroke(.1f,.55f,.24f,.55f); Stroke(.12f,.72f,.26f,.72f); }
            else if (effect == ItemEffect.Luck)
            { tint = new Color(.64f,.88f,.48f); Circle(.35f,.64f,.17f); Circle(.65f,.64f,.17f); Circle(.35f,.36f,.17f); Circle(.65f,.36f,.17f); Stroke(.5f,.48f,.65f,.12f); }
            else if (effect == ItemEffect.Dodge)
            { tint = ArcadeSkin.Teal; Path(new Vector2(.1f,.5f),new Vector2(.3f,.72f),new Vector2(.7f,.72f),new Vector2(.9f,.5f),new Vector2(.7f,.28f),new Vector2(.3f,.28f),new Vector2(.1f,.5f)); Stroke(.2f,.15f,.8f,.85f); }
            else if (effect == ItemEffect.Roam || effect == ItemEffect.Pickup)
            { tint = ArcadeSkin.Teal; Circle(.5f,.5f,.31f); Circle(.5f,.5f,.07f); Stroke(.1f,.5f,.3f,.5f); Stroke(.7f,.5f,.9f,.5f); if(effect==ItemEffect.Roam) { Stroke(.5f,.1f,.5f,.3f); Stroke(.5f,.7f,.5f,.9f); } }
            else if (effect == ItemEffect.BodySize || effect == ItemEffect.CompanionSize || effect == ItemEffect.BulletSize)
            { tint = ArcadeSkin.Teal; Circle(.5f,.5f,effect==ItemEffect.BulletSize?.16f:.23f); Path(new Vector2(.2f,.65f),new Vector2(.2f,.82f),new Vector2(.37f,.82f)); Path(new Vector2(.8f,.35f),new Vector2(.8f,.18f),new Vector2(.63f,.18f)); }
            else if (effect == ItemEffect.EnemyCount)
            { tint=ArcadeSkin.Worse; Circle(.3f,.65f,.14f); Circle(.7f,.65f,.14f); Path(new Vector2(.1f,.2f),new Vector2(.1f,.4f),new Vector2(.5f,.4f),new Vector2(.5f,.2f)); Path(new Vector2(.52f,.2f),new Vector2(.52f,.4f),new Vector2(.9f,.4f),new Vector2(.9f,.2f)); }
            else if (effect == ItemEffect.ChestReach)
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
            else if (effect == ItemEffect.Damage || effect == ItemEffect.DamageCorrection || effect == ItemEffect.MeleeDamage || effect == ItemEffect.ChampionDamage || effect == ItemEffect.CompanionDamage || effect == ItemEffect.Knockback || effect == ItemEffect.CollisionDamage)
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
            if (effect == ItemEffect.DamageCorrection || effect == ItemEffect.ArmorCorrection || effect == ItemEffect.HealthCorrection || effect == ItemEffect.SpeedCorrection || effect == ItemEffect.HealCorrection)
            { Stroke(.74f,.14f,.9f,.3f); Stroke(.74f,.3f,.9f,.14f); }
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
