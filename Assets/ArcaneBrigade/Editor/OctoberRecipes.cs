using System;
using System.Collections.Generic;
using System.Linq;
namespace ArcaneBrigade.Editor
{
    public static class OctoberRecipes
    {
        public static string[] ItemThemes(ItemDefinition item)
        {
            // Small shared stat staples keep every roster playable; character shelves use actual benefits.
            if (item.Rarity == ItemRarity.Common && item.EffectiveModifiers.Length == 1 && (item.Effect == ItemEffect.Health || item.Effect == ItemEffect.Damage || item.Effect == ItemEffect.AttackSpeed)) return Array.Empty<string>();
            var tags = new HashSet<string>();
            void Add(params string[] names) { foreach (string name in names) tags.Add(name); }
            foreach (StatModifier m in item.EffectiveModifiers)
            {
                if (m.Value <= 0) continue;
                switch (m.Effect)
                {
                    case ItemEffect.Health: case ItemEffect.Heal: case ItemEffect.HeartHealing: case ItemEffect.HealCorrection: Add("hearth", "orchard", "frost"); break;
                    case ItemEffect.Armor: case ItemEffect.CollisionDamage: case ItemEffect.CollisionFrequency: Add("clock", "hearth", "oddity"); break;
                    case ItemEffect.Crit: case ItemEffect.Projectiles: case ItemEffect.AttackRange: case ItemEffect.BulletPierce: case ItemEffect.BulletSplit: case ItemEffect.BulletSize: case ItemEffect.BulletBounce: Add("astral", "archive", "frost"); break;
                    case ItemEffect.MoveSpeed: case ItemEffect.Dodge: case ItemEffect.SpeedCorrection: Add("tidal", "clock", "oddity"); break;
                    case ItemEffect.Income: case ItemEffect.Discount: case ItemEffect.RecruitDiscount: case ItemEffect.RelicDiscount: case ItemEffect.Luck: case ItemEffect.TreasureDetection: case ItemEffect.ChestQuality: Add("archive", "tidal", "orchard"); break;
                    case ItemEffect.MeleeDamage: Add("clock", "hearth"); break;
                    case ItemEffect.RangedDamage: case ItemEffect.BulletSpeed: case ItemEffect.BulletDistance: Add("archive", "astral", "frost"); break;
                    default: Add("hearth", "clock", "astral"); break;
                }
            }
            if (item.Category == RelicCategory.Drink) Add("tidal", "archive");
            if (item.Category == RelicCategory.Food) Add("hearth", "orchard");
            if (tags.Count == 0) Add("oddity", "archive");
            return tags.OrderBy(t => t).ToArray();
        }
        public static void Install(RunCatalog c)
        {
            var recipes = c.Recipes.Where(r => !r.Id.StartsWith("OctoberAltar")).ToList();
            string Unit(string name) => c.Companions.First(u => u.ReferenceCompanion == name).Id;
            string[] Bond(string tag, int shift = 0) => c.Companions.Where(u => u.ReferenceCompanion != null && Array.IndexOf(u.Tags, tag) >= 0 && !u.SpecialRecruit).Skip(shift).Take(3).Select(u => u.ReferenceCompanion).ToArray();
            void Add(string result, params string[] ingredients)
            {
                if (ingredients.Length < 2 || ingredients.Length > 3) throw new InvalidOperationException("Incomplete reference altar recipe: " + result);
                recipes.Add(new AltarRecipe { Id = "OctoberAltar" + Unit(result), FirstUnit = Unit(ingredients[0]), SecondUnit = Unit(ingredients[1]), ThirdUnit = ingredients.Length == 3 ? Unit(ingredients[2]) : "", ResultUnit = Unit(result), MinimumMaterialTier = 1, ResultTier = 2, EggCost = 4 });
            }
            Add("\"Art of Food\" Roshi", Bond("regal"));
            Add("\"Bullied\" Sophia", "Monkey King", "Pirate", "Masked Wrestler");
            Add("\"Cars ARE My Kids\" Chris", "Intelligent Construct", "Bullhorn Warrior", "Iron Swordsman");
            Add("\"Colorful Fantasy\" Pinky", "Cheer Bunny", "Smiley Fox", "Phantom");
            Add("\"Enduring Suffering\" Dorothy", "Grey Mage", "Little Witch", "Grey Mage");
            Add("\"Expert Nurturer\" Daisy", "Mummy", "Alien", "Hopping Zombie");
            Add("\"Formatting Pixie\" Quentin", Bond("hide"));
            Add("\"GLHF\" Nia", "Pointy Hat Witch", "Smiley Fox", "Floofy");
            Add("\"Leisurely Walk\" Mary", Bond("morning"));
            Add("\"Misfortune\" Emily", Bond("hide", 1));
            Add("\"Naming Demon\" Yan", "Applehead", "Peach", "Applehead");
            Add("\"Overtime King\" Philip", "Pointy Hat Witch", "Pirate", "TV Head");
            Add("\"Pet Master\" Lisa", "Judge", "Camo Shroom", "Young Lady");
            Add("\"Shove Around\" Bella", "Cheer Bunny", "Iron Swordsman", "Cheer Bunny");
            Add("\"Smug and Satisfied\" Jake", Bond("gauntlet"));
            Add("\"The All-Knowing\" Walter", Bond("gauntlet", 1));
            Add("\"The Creator\" Roger", "Intelligent Construct", "TV Head", "Grey Mage");
            Add("\"Triumphant Return\" Daniel", "Intelligent Construct", "TV Head", "TV Head");
            Add("\"Quick Production\" Tao", "Athlete", "Student", "Student");
            Add("\"Unstoppable\" Paul", Bond("mask"));
            Add("Vengeful Spirit", "Prince", "Iron Swordsman");
            c.Recipes = recipes.ToArray();
            foreach (string name in new[] { "Domestic Spiked Ball", "Inflatable Hammer", "Monster Friend" }) c.Companions.First(u => u.ReferenceCompanion == name).GiftOnly = true;
        }
    }
}
