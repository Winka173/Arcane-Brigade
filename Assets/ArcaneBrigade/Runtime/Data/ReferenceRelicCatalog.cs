using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class ReferenceRelicCatalog
    {
        private static StatModifier Stat(ItemEffect effect, float value, ModifierScope? scope = null)
            => new StatModifier { Effect = effect, Value = value, Scope = scope ?? RunModifierRules.DefaultScope(effect) };

        public static ItemDefinition[] Create()
        {
            var result = new List<ItemDefinition>();
            void Add(string id, string reference, int rarity, bool special, RelicCategory category, RelicMechanic mechanic, string text, params StatModifier[] modifiers)
            {
                result.Add(new ItemDefinition
                {
                    Id = "Curio" + id, Name = System.Text.RegularExpressions.Regex.Replace(id, "([a-z])([A-Z])", "$1 $2"),
                    ReferenceName = reference, Rarity = (ItemRarity)(rarity - 1), Category = category, Mechanic = mechanic,
                    Description = text, Modifiers = modifiers, Price = 18 + rarity * 14, UnlockWave = rarity > 2 ? 3 : 1,
                    Pools = special ? ShopPool.Eggs : ShopPool.Gold, AltarCurrency = special && rarity == 4 ? ShopCurrency.Health : ShopCurrency.Eggs,
                    AltarPrice = special && rarity == 4 ? 12 : rarity, ShopWeight = rarity == 4 ? 28 : rarity == 3 ? 55 : 100,
                    MaximumStacks = mechanic == RelicMechanic.StatFloor || mechanic == RelicMechanic.SixWeapons || mechanic == RelicMechanic.CollisionOnly || mechanic == RelicMechanic.StaticFollowers ? 1 : 8
                });
            }
            Add("OrchardFizz", "Fruit Crush", 1, false, RelicCategory.Drink, RelicMechanic.None, "+20 health. -2 income.", Stat(ItemEffect.Health,20), Stat(ItemEffect.Income,-2));
            Add("GraveChime", "Midnight Ballad", 1, false, RelicCategory.Drink, RelicMechanic.None, "+10 contact damage. -5% speed.", Stat(ItemEffect.CollisionDamage,10,ModifierScope.Champion),Stat(ItemEffect.MoveSpeed,-.05f,ModifierScope.Champion));
            Add("BreathJar", "Oxygen Bubble", 1, false, RelicCategory.Drink, RelicMechanic.None, "+20% hero & friends damage. -4% attack speed.",Stat(ItemEffect.Damage,.2f),Stat(ItemEffect.AttackSpeed,-.04f));
            Add("CoralBroth", "Peach Sea", 1, false, RelicCategory.Drink, RelicMechanic.None, "+1 regeneration. -4 health.",Stat(ItemEffect.Heal,1),Stat(ItemEffect.Health,-4));
            Add("FangPin", "Shark Button", 1, false, RelicCategory.Curio, RelicMechanic.None, "Player: double knockback. +20% attack speed.",Stat(ItemEffect.Knockback,1,ModifierScope.Champion),Stat(ItemEffect.AttackSpeed,.2f,ModifierScope.Champion));
            Add("RicochetSeltzer", "Soda Tea", 1, false, RelicCategory.Drink, RelicMechanic.None, "+6 ranged damage. +25% bounce. -5% damage.",Stat(ItemEffect.RangedDamage,6),Stat(ItemEffect.BulletBounce,.25f),Stat(ItemEffect.Damage,-.05f));
            Add("NoonSpark", "Startled Awake by Afternoon", 1, false, RelicCategory.Drink, RelicMechanic.None, "+5 melee damage. +6% critical chance.",Stat(ItemEffect.MeleeDamage,5),Stat(ItemEffect.Crit,.06f));
            Add("QuietHour", "Time of Leisure", 1, false, RelicCategory.Drink, RelicMechanic.None, "+0.12 armor. +15% treasure detection.",Stat(ItemEffect.Armor,.12f),Stat(ItemEffect.TreasureDetection,.15f));
            Add("BentTape", "Warped Cassette", 1, false, RelicCategory.Curio, RelicMechanic.CrowdedTape, "+18% attack speed. Next wave: 10% more enemies.",Stat(ItemEffect.AttackSpeed,.18f));
            Add("MeadowTonic", "Weird Grassland", 1, false, RelicCategory.Drink, RelicMechanic.None, "+8% dodge. +6% luck.",Stat(ItemEffect.Dodge,.08f),Stat(ItemEffect.Luck,.06f));
            Add("KindlingGloves", "Wool Mittens", 1, false, RelicCategory.Curio, RelicMechanic.CoinMending, "Coins: 15% chance to heal 2. -0.4 regeneration.",Stat(ItemEffect.Heal,-.4f));
            Add("InterestTin", "Bento Box", 2, false, RelicCategory.Food, RelicMechanic.CompoundInterest, "+60% pickup radius. Wave start: gain 20% of wallet.",Stat(ItemEffect.Pickup,.6f));
            Add("StormDryer", "Cheap Hair Dryer", 2, false, RelicCategory.Curio, RelicMechanic.None, "+24% damage. +20% knockback.",Stat(ItemEffect.Damage,.24f),Stat(ItemEffect.Knockback,.2f));
            Add("RetortCup", "Commemorative Mug", 2, false, RelicCategory.Curio, RelicMechanic.RetortVolley, "+0.4 armor. -8% luck. Hurt: 30% chance of 8 shots.",Stat(ItemEffect.Armor,.4f),Stat(ItemEffect.Luck,-.08f));
            Add("ShatteredPager", "Cracked Phone", 2, false, RelicCategory.Curio, RelicMechanic.None, "+25 income. Player bullets split twice.",Stat(ItemEffect.Income,25),Stat(ItemEffect.BulletSplit,2,ModifierScope.Champion));
            Add("WatchfulTea", "Gaze", 2, false, RelicCategory.Curio, RelicMechanic.ExposedRecovery, "+2 regeneration. Next wave: -0.8 armor.",Stat(ItemEffect.Heal,2));
            Add("SunfireFlask", "Golden Fuel", 2, false, RelicCategory.Drink, RelicMechanic.None, "+40% range. Player: +1 shot, -25% attack speed.",Stat(ItemEffect.AttackRange,.4f),Stat(ItemEffect.Projectiles,1,ModifierScope.Champion),Stat(ItemEffect.AttackSpeed,-.25f,ModifierScope.Champion));
            Add("GiltOrbit", "Gold-Plated Ring", 2, false, RelicCategory.Curio, RelicMechanic.TreasureLease, "+5 ranged damage. Next wave: +100% treasure detection.",Stat(ItemEffect.RangedDamage,5));
            Add("EmberEspresso", "Hot Coffee", 2, false, RelicCategory.Drink, RelicMechanic.None, "Player: +24% attack speed, -1 contact damage.",Stat(ItemEffect.AttackSpeed,.24f,ModifierScope.Champion),Stat(ItemEffect.CollisionDamage,-1,ModifierScope.Champion));
            Add("PrismRecord", "Iridescent Disc", 2, false, RelicCategory.Curio, RelicMechanic.PrismEcho, "Player attacks echo ranged damage at a random foe. -10% range.",Stat(ItemEffect.AttackRange,-.1f));
            Add("CitrusStorm", "Lemon Spring Water", 2, false, RelicCategory.Drink, RelicMechanic.None, "+6 ranged damage. +2% enemies.",Stat(ItemEffect.RangedDamage,6),Stat(ItemEffect.EnemyCount,.02f));
            Add("MoonMilk", "Midnight", 2, false, RelicCategory.Drink, RelicMechanic.None, "Hearts heal 4 more. Player: -5% speed.",Stat(ItemEffect.HeartHealing,4),Stat(ItemEffect.MoveSpeed,-.05f,ModifierScope.Champion));
            Add("PocketSun", "Orange Joy", 2, false, RelicCategory.Drink, RelicMechanic.None, "Player: 10% smaller. +30% luck.",Stat(ItemEffect.BodySize,-.1f),Stat(ItemEffect.Luck,.3f));
            Add("InkFeather", "Quill", 2, false, RelicCategory.Curio, RelicMechanic.RerollFeather, "Rerolls cost 2 less. Minimum cost: 1.");
            Add("MulberryRiot", "Rock 'n' Roll Mulberry", 2, false, RelicCategory.Drink, RelicMechanic.None, "+30 income. +60% treasure detection. -0.3 armor.",Stat(ItemEffect.Income,30),Stat(ItemEffect.TreasureDetection,.6f),Stat(ItemEffect.Armor,-.3f));
            Add("DreamBlindfold", "Sleep Mask", 2, false, RelicCategory.Curio, RelicMechanic.ChestLightning, "Chests strike up to 3 foes. +10% luck, +25% treasure.",Stat(ItemEffect.Luck,.1f),Stat(ItemEffect.TreasureDetection,.25f));
            Add("SleepyMist", "Sleep Well", 2, false, RelicCategory.Drink, RelicMechanic.None, "+35% damage. Hero & friends moves 20% slower.",Stat(ItemEffect.Damage,.35f),Stat(ItemEffect.MoveSpeed,-.2f),Stat(ItemEffect.EnemySpeed,-.2f));
            Add("ClearSip", "Spring Water", 2, false, RelicCategory.Drink, RelicMechanic.DrinkVitality, "Buying drinks grants +4 health. -1 contact damage.",Stat(ItemEffect.CollisionDamage,-1,ModifierScope.Champion));
            Add("OddElixir", "Strangely Favored Drink", 2, false, RelicCategory.Curio, RelicMechanic.DrinkTempo, "Buying drinks grants +6% attack speed. More drinks appear.");
            Add("DaylightBonnet", "Sun Hat", 2, false, RelicCategory.Curio, RelicMechanic.PanicMagnet, "+40 health. Hurt: 5% chance to attract all loot.",Stat(ItemEffect.Health,40));
            Add("NeedleTarget", "Target Board", 2, false, RelicCategory.Curio, RelicMechanic.None, "+1 pierce, +15% critical chance. -3 melee damage.",Stat(ItemEffect.BulletPierce,1),Stat(ItemEffect.Crit,.15f),Stat(ItemEffect.MeleeDamage,-3));
            Add("ManyDoorKey", "Universal Key", 2, false, RelicCategory.Curio, RelicMechanic.ChestLuck, "Chests grant +6% luck. +25% treasure. -5% damage.",Stat(ItemEffect.TreasureDetection,.25f),Stat(ItemEffect.Damage,-.05f));
            Add("CourageJuice", "Vitamin C Rage", 2, false, RelicCategory.Drink, RelicMechanic.None, "+20% speed. +5 melee damage. -10 health.",Stat(ItemEffect.MoveSpeed,.2f),Stat(ItemEffect.MeleeDamage,5),Stat(ItemEffect.Health,-10));
            Add("CrookedEight", "Weird Pool Ball", 2, false, RelicCategory.Curio, RelicMechanic.None, "+40% luck. +20% bounce. -6% damage.",Stat(ItemEffect.Luck,.4f),Stat(ItemEffect.BulletBounce,.2f),Stat(ItemEffect.Damage,-.06f));
            Add("TornAlmanac", "Chaos Calendar", 3, false, RelicCategory.Curio, RelicMechanic.ChaosIncome, "Wave income randomly gains 25-75. -5% damage.",Stat(ItemEffect.Damage,-.05f));
            Add("HoardersLocket", "Cowardice", 3, true, RelicCategory.Curio, RelicMechanic.HoardPower, "Every 5 gold held grants +1% hero & friends damage.");
            Add("SplitKernel", "Cracked Seed", 3, true, RelicCategory.Food, RelicMechanic.HalfHealthSeed, "+3 regeneration. Start battles at half health.",Stat(ItemEffect.Heal,3));
            Add("WildPigment", "Crazy Paint", 3, false, RelicCategory.Drink, RelicMechanic.None, "+15% speed. +7 melee damage. -3 ranged damage.",Stat(ItemEffect.MoveSpeed,.15f),Stat(ItemEffect.MeleeDamage,7),Stat(ItemEffect.RangedDamage,-3));
            Add("StillwaterUrn", "Despair", 3, true, RelicCategory.Curio, RelicMechanic.StillRecovery, "Standing still: +3 regeneration, +20 contact damage. -5% speed.",Stat(ItemEffect.MoveSpeed,-.05f,ModifierScope.Champion));
            Add("PanicBell", "Fear", 3, true, RelicCategory.Curio, RelicMechanic.CoinLightning, "Picking up gold strikes a random foe. Scales with luck.");
            Add("OilyLozenge", "Fish Oil in a Box", 3, false, RelicCategory.Food, RelicMechanic.TurnDiscount, "+20 health. This shop: 30% cheaper items, more drinks.",Stat(ItemEffect.Health,20));
            Add("LodestoneLabel", "Fridge Sticker", 3, false, RelicCategory.Curio, RelicMechanic.CloseFollowers, "+8 ranged damage. +90% pickup radius. Friends stay closer.",Stat(ItemEffect.RangedDamage,8),Stat(ItemEffect.Pickup,.9f));
            Add("HungryLedger", "Greed", 3, true, RelicCategory.Curio, RelicMechanic.WaveGrowth, "Each wave: permanently gain +4 melee/ranged and +10% damage.");
            Add("StoneCrust", "Hard Bread", 3, false, RelicCategory.Food, RelicMechanic.MeleeToCollision, "Half of melee damage also becomes contact damage. -10% player attack speed.",Stat(ItemEffect.AttackSpeed,-.1f,ModifierScope.Champion));
            Add("BitterHoney", "Matcha Honey", 3, false, RelicCategory.Drink, RelicMechanic.None, "+2.5 regeneration. +60% treasure. Enemies move 7% faster.",Stat(ItemEffect.Heal,2.5f),Stat(ItemEffect.TreasureDetection,.6f),Stat(ItemEffect.EnemySpeed,.07f));
            Add("BurdenCollar", "Neck Brace", 3, false, RelicCategory.Curio, RelicMechanic.SlowToArmor, "+20 contact damage. Slower movement grants armor. -20% speed.",Stat(ItemEffect.CollisionDamage,20,ModifierScope.Champion),Stat(ItemEffect.MoveSpeed,-.2f));
            Add("HushEarmuffs", "Noise-Cancelling Headphones", 3, false, RelicCategory.Curio, RelicMechanic.DodgeMending, "+10% dodge and crit. Dodging: 30% chance to heal 10.",Stat(ItemEffect.Dodge,.1f),Stat(ItemEffect.Crit,.1f,ModifierScope.Champion));
            Add("ProfitSlide", "Projector Slide", 3, false, RelicCategory.Curio, RelicMechanic.DamageToIncome, "Every 4% damage grants +1 income. -10 income.",Stat(ItemEffect.Income,-10));
            Add("AngryReliquary", "Rage", 3, true, RelicCategory.Curio, RelicMechanic.ChestOnlyRecovery, "Chests heal 30. Regeneration and hearts no longer heal.");
            Add("PetalTonic", "Rose Water", 3, false, RelicCategory.Drink, RelicMechanic.None, "+90 health. -2 regeneration.",Stat(ItemEffect.Health,90),Stat(ItemEffect.Heal,-2));
            Add("SorrowLantern", "Sadness", 3, true, RelicCategory.Curio, RelicMechanic.SorrowPulse, "Every 3 seconds, strike all foes for 20% max health. -5% dodge.",Stat(ItemEffect.Dodge,-.05f));
            Add("MuteClock", "Silent Alarm", 3, false, RelicCategory.Curio, RelicMechanic.BleedingAlarm, "+45% attack speed, +40% speed. Lose 1 health per second.",Stat(ItemEffect.AttackSpeed,.45f),Stat(ItemEffect.MoveSpeed,.4f));
            Add("UrchinBall", "Spiky Yoga Ball", 3, false, RelicCategory.Curio, RelicMechanic.CollisionOnly, "Player stops attacking. +0.6 armor, +10% speed. Armor boosts contact.",Stat(ItemEffect.Armor,.6f),Stat(ItemEffect.MoveSpeed,.1f,ModifierScope.Champion));
            Add("SourMandarin", "Tangy Tangerine", 3, false, RelicCategory.Food, RelicMechanic.SoloOverdrive, "Next wave: friends cannot attack, player attacks rapidly. 10 kills heal 3.");
            Add("WakefulCordial", "Wake Up!", 3, false, RelicCategory.Drink, RelicMechanic.None, "+25 contact damage, +30% knockback. -10% crit.",Stat(ItemEffect.CollisionDamage,25,ModifierScope.Champion),Stat(ItemEffect.Knockback,.3f,ModifierScope.Champion),Stat(ItemEffect.Crit,-.1f,ModifierScope.Champion));
            Add("GlassLotus", "Water Lily", 3, false, RelicCategory.Curio, RelicMechanic.FreshLotus, "+3 pierce and +30% critical chance until hurt. Resets each wave.");
            Add("NestDumpling", "Weird Rice Ball", 3, false, RelicCategory.Food, RelicMechanic.EggGift, "Gain 3 dreamsand now. -0.2 armor.",Stat(ItemEffect.Armor,-.2f));
            Add("VictorDraught", "Winningway", 3, false, RelicCategory.Drink, RelicMechanic.None, "+20% crit, +1 pierce. -20% damage.",Stat(ItemEffect.Crit,.2f),Stat(ItemEffect.BulletPierce,1),Stat(ItemEffect.Damage,-.2f));
            Add("WishWick", "Birthday Candle", 4, false, RelicCategory.Curio, RelicMechanic.CompanionGift, "Gain 2 random friends. -5 health, -5 income. Counts toward recruit limit.",Stat(ItemEffect.Health,-5),Stat(ItemEffect.Income,-5));
            Add("ShieldBlower", "Cooling Fan", 4, false, RelicCategory.Curio, RelicMechanic.None, "+25 contact damage. Friends block hostile bullets.",Stat(ItemEffect.CollisionDamage,25),Stat(ItemEffect.Interceptor,1));
            Add("LiarMask", "Deception", 4, true, RelicCategory.Curio, RelicMechanic.FragilePower, "+100% damage. Each hit permanently removes 5% damage.",Stat(ItemEffect.Damage,1));
            Add("OrbitPastry", "Donut", 4, false, RelicCategory.Food, RelicMechanic.None, "Player: larger, slower bullets with +100% damage.",Stat(ItemEffect.BodySize,.25f),Stat(ItemEffect.BulletSize,.75f,ModifierScope.Champion),Stat(ItemEffect.BulletSpeed,-.4f,ModifierScope.Champion),Stat(ItemEffect.ChampionDamage,1));
            Add("OathParcel", "Dumplings", 4, false, RelicCategory.Food, RelicMechanic.StatFloor, "Lock current stat bonuses as minimums for the rest of this run.");
            Add("FortunePear", "Expensive Fruit", 4, false, RelicCategory.Food, RelicMechanic.LuckToHealth, "+16% luck. Luck also grants max health. -15 income.",Stat(ItemEffect.Luck,.16f),Stat(ItemEffect.Income,-15));
            Add("EmeraldInvoice", "Green Form", 4, false, RelicCategory.Curio, RelicMechanic.None, "Items cost 25% less. -20% damage.",Stat(ItemEffect.RelicDiscount,.25f),Stat(ItemEffect.Damage,-.2f));
            Add("BatteringHelm", "Hard Hat", 4, false, RelicCategory.Curio, RelicMechanic.None, "+40 contact damage, +70% contact frequency. -10 ranged damage.",Stat(ItemEffect.CollisionDamage,40,ModifierScope.Champion),Stat(ItemEffect.CollisionFrequency,.7f,ModifierScope.Champion),Stat(ItemEffect.RangedDamage,-10));
            Add("GambleSandwich", "Healthy Sandwich", 4, false, RelicCategory.Food, RelicMechanic.GambleRecovery, "Every 3 seconds: chance to heal 20 and/or lose 8. -0.2 armor.",Stat(ItemEffect.Armor,-.2f));
            Add("FeastBucket", "Juicy Fried Chicken", 4, false, RelicCategory.Food, RelicMechanic.FoodGrowth, "Food grants health, size and regeneration. -30% dodge.",Stat(ItemEffect.Health,40),Stat(ItemEffect.Heal,1),Stat(ItemEffect.BodySize,.1f),Stat(ItemEffect.Dodge,-.3f));
            Add("WanderLetter", "Message From A Friend", 4, false, RelicCategory.Curio, RelicMechanic.FreeFollowers, "Friends roam freely. +40% critical chance. -0.7 armor.",Stat(ItemEffect.Crit,.4f),Stat(ItemEffect.Armor,-.7f));
            Add("VenomPass", "Mysterious ID", 4, false, RelicCategory.Curio, RelicMechanic.IncomePoison, "Player hits poison enemies for your income each second. Does not stack.");
            Add("TruceBottle", "Peace Water", 4, false, RelicCategory.Drink, RelicMechanic.None, "+40% damage, +20% crit, +40% range, +0.6 armor.",Stat(ItemEffect.Damage,.4f),Stat(ItemEffect.Crit,.2f),Stat(ItemEffect.AttackRange,.4f),Stat(ItemEffect.Armor,.6f));
            Add("ThornPerfume", "Perfume", 4, false, RelicCategory.Curio, RelicMechanic.HurtGrowth, "Taking damage permanently improves a random stat. Enemies move 30% faster.",Stat(ItemEffect.EnemySpeed,.3f));
            Add("LastHopeMotor", "Self-Driving Force", 4, false, RelicCategory.Curio, RelicMechanic.StillPower, "+16 melee, ranged and contact damage until you move. Resets each wave.");
            Add("AnchorTack", "Thumbtack", 4, false, RelicCategory.Curio, RelicMechanic.StaticFollowers, "Friends become stationary turrets: double range and +3 pierce.");
            Add("VoidDoubloon", "Universe Coin", 4, false, RelicCategory.Curio, RelicMechanic.VoidPulse, "Every 3 seconds, all foes lose 15% current health. -30% damage.",Stat(ItemEffect.Damage,-.3f));
            Add("SixfoldRoot", "Potato", 4, true, RelicCategory.Food, RelicMechanic.SixWeapons, "Six phantom weapons fight for you. Player and other friends stop attacking.");
            return result.ToArray();
        }
    }
}
