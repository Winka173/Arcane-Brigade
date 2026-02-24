using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class PaintedStatIconBuilder
    {
        public static void Import()
        {
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            Sprite Item(string id) => catalog.Item(id)?.Icon ?? throw new InvalidOperationException("Missing painted item " + id);
            Sprite Weapon(string id) => catalog.Weapon(id)?.Sprite ?? throw new InvalidOperationException("Missing painted weapon " + id);
            Sprite Bond(string id) => catalog.Bonds.First(b => b.Id == id).Icon;
            Sprite Unit(string id) => catalog.Unit(id)?.Body ?? throw new InvalidOperationException("Missing painted companion " + id);
            var rows = new List<PaintedStatIcon>();
            void Add(ItemEffect e, Sprite picture, string source, Sprite badge = null)
            {
                Rect ink = Ink(picture);
                rows.Add(new PaintedStatIcon { Effect = e, Picture = picture, Badge = badge, Ink = ink, Rotation = ink.width > ink.height * 2 ? 45 : 0, BadgeInk = badge != null ? Ink(badge) : Rect.zero, Source = source });
            }
            Add(ItemEffect.Damage, Weapon("CorkscrewRapier"), "CorkscrewRapier");
            Add(ItemEffect.AttackSpeed, Item("clock-spring"), "clock-spring");
            Add(ItemEffect.Health, Item("honey-pot"), "honey-pot / vitality");
            Add(ItemEffect.MoveSpeed, Item("felt-boots"), "felt-boots");
            Add(ItemEffect.Pickup, Item("coin-compass"), "coin-compass");
            Add(ItemEffect.Roam, Item("wandering-map"), "wandering-map");
            Add(ItemEffect.Interceptor, Item("guardian-canopy"), "guardian-canopy");
            Add(ItemEffect.Heal, Item("warm-tea"), "warm-tea / recovery");
            Add(ItemEffect.Crit, Item("oracle-lens"), "oracle-lens");
            Add(ItemEffect.Projectiles, Item("twin-seal"), "twin-seal");
            Add(ItemEffect.Interest, Item("piggy-mail"), "piggy-mail");
            Add(ItemEffect.Armor, Item("pillow-mail"), "pillow-mail");
            Add(ItemEffect.ChampionDamage, Weapon("CorkscrewRapier"), "rapier + royal-key / hero", Item("royal-key"));
            Add(ItemEffect.CompanionDamage, Weapon("CorkscrewRapier"), "rapier + brigade-banner / companions", Item("brigade-banner"));
            Add(ItemEffect.CompanionAttackSpeed, Item("clock-spring"), "spring + brigade-banner / companions", Item("brigade-banner"));
            Add(ItemEffect.Discount, Item("coupon-book"), "coupon-book");
            Add(ItemEffect.Luck, Bond("clover"), "Lucky Leaves bond");
            Add(ItemEffect.TreasureDetection, catalog.ChestSprite, "chest + oracle-lens / detection", Item("oracle-lens"));
            Add(ItemEffect.ScoreMultiplier, Item("tarnished-crown"), "tarnished-crown / score");
            Add(ItemEffect.CollisionDamage, Unit("BristleBall"), "BristleBall / contact");
            Add(ItemEffect.MeleeDamage, Weapon("CorkscrewRapier"), "rapier + gauntlet / flat melee", Bond("gauntlet"));
            Add(ItemEffect.RangedDamage, Weapon("CrescentBow"), "CrescentBow");
            Add(ItemEffect.AttackRange, Item("long-atlas"), "long-atlas / reach");
            Add(ItemEffect.Dodge, Bond("hare"), "Springstep bond");
            Add(ItemEffect.Income, catalog.Arena.Coin, "painted coin");
            Add(ItemEffect.BulletPierce, Weapon("LeafCrossbow"), "leaf crossbow + arrow / pierce", Weapon("DartCompass"));
            Add(ItemEffect.BulletSplit, Item("twin-seal"), "twin seal + arrow / split", Weapon("DartCompass"));
            Add(ItemEffect.BulletBounce, Weapon("CrescentBoomerang"), "CrescentBoomerang / return");
            Add(ItemEffect.BulletSize, Weapon("SnowGlobeOrb"), "SnowGlobeOrb / size");
            Add(ItemEffect.BulletSpeed, Weapon("DartCompass"), "dart + spring / speed", Item("clock-spring"));
            Add(ItemEffect.BulletDistance, Weapon("DartCompass"), "dart + atlas / distance", Item("wandering-map"));
            Add(ItemEffect.Knockback, Weapon("BookPaddle"), "BookPaddle / push");
            Add(ItemEffect.BodySize, Item("starlight-quilt"), "quilt + royal-key / hero size", Item("royal-key"));
            Add(ItemEffect.CompanionSize, Item("starlight-quilt"), "quilt + brigade-banner / companion size", Item("brigade-banner"));
            var enemy = catalog.Enemies.First(u => u.Behavior == EnemyBehavior.Standard).Body;
            Add(ItemEffect.EnemyCount, enemy, "enemy + twin-seal / count", Item("twin-seal"));
            Add(ItemEffect.EnemyHealth, enemy, "enemy + honey-pot / vitality", Item("honey-pot"));
            Add(ItemEffect.EnemySpeed, enemy, "enemy + boots / speed", Item("felt-boots"));
            Add(ItemEffect.HeartHealing, catalog.HeartSprite, "painted heart pickup");
            Add(ItemEffect.ChestQuality, catalog.ChestSprite, "chest + crown / quality", Item("tarnished-crown"));
            Add(ItemEffect.RecruitDiscount, Item("coupon-book"), "coupon + companion flag", Item("brigade-banner"));
            Add(ItemEffect.RelicDiscount, Item("coupon-book"), "coupon + oracle-lens / relic", Item("oracle-lens"));
            Add(ItemEffect.ShopRarity, Item("fortune-egg"), "fortune-egg / rarity");
            Sprite adjust = Weapon("CloudScissors");
            foreach (var pair in new[] { (ItemEffect.HealthCorrection, ItemEffect.Health), (ItemEffect.DamageCorrection, ItemEffect.Damage), (ItemEffect.SpeedCorrection, ItemEffect.MoveSpeed), (ItemEffect.ArmorCorrection, ItemEffect.Armor), (ItemEffect.HealCorrection, ItemEffect.Heal) })
                Add(pair.Item1, rows.First(r => r.Effect == pair.Item2).Picture, pair.Item2 + " + CloudScissors / correction", adjust);
            Add(ItemEffect.CollisionFrequency, Unit("BristleBall"), "contact + spring / frequency", Item("clock-spring"));
            Add(ItemEffect.ChestReach, catalog.ChestSprite, "chest + compass / collection reach", Item("coin-compass"));
            const string path = "Assets/ArcaneBrigade/Resources/PaintedStatIcons.asset";
            var asset = AssetDatabase.LoadAssetAtPath<PaintedStatIcons>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<PaintedStatIcons>(); AssetDatabase.CreateAsset(asset, path); }
            asset.Entries = rows.OrderBy(r => r.Effect).ToArray(); EditorUtility.SetDirty(asset); AssetDatabase.SaveAssets();
            Directory.CreateDirectory("PreviewExports/CompanionReactions");
            File.WriteAllLines("PreviewExports/CompanionReactions/Icon_Sources.tsv", new[] { "Stat\tSource\tAsset\tBadge" }.Concat(asset.Entries.Select(e => e.Effect + "\t" + e.Source + "\t" + AssetDatabase.GetAssetPath(e.Picture) + "#" + e.Picture.name + "\t" + (e.Badge != null ? AssetDatabase.GetAssetPath(e.Badge) + "#" + e.Badge.name : "-"))));
            Debug.Log("PAINTED_STAT_ICONS " + asset.Entries.Length);
        }
        private static Rect Ink(Sprite sprite)
        {
            // Read imported pixels once in the editor, including older sprites without SpriteInkMetrics entries.
            var texture = sprite.texture; var target = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active; Graphics.Blit(texture, target); RenderTexture.active = target;
            var readable = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false); readable.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0); readable.Apply();
            Rect rect = sprite.textureRect; int left = (int)rect.width, bottom = (int)rect.height, right = -1, top = -1;
            var pixels = readable.GetPixels32();
            for (int y = 0; y < (int)rect.height; y++) for (int x = 0; x < (int)rect.width; x++)
                if (pixels[((int)rect.y + y) * texture.width + (int)rect.x + x].a >= 40) { left = Mathf.Min(left, x); bottom = Mathf.Min(bottom, y); right = Mathf.Max(right, x); top = Mathf.Max(top, y); }
            RenderTexture.active = previous; RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(readable);
            if (right < left) throw new InvalidOperationException("Empty painted sprite " + sprite.name);
            return Rect.MinMaxRect(Mathf.Max(0, left - 2), Mathf.Max(0, bottom - 2), Mathf.Min(rect.width, right + 3), Mathf.Min(rect.height, top + 3));
        }
    }
}
