using System;
using UnityEngine;
namespace ArcaneBrigade
{
    [Serializable] public sealed class OctoberUnitRow { public string reference, id, name, design, theme, weapon, bond, source, effect; public int trait; public bool @new; }
    [Serializable] public sealed class OctoberRoster { public OctoberUnitRow[] heroes, newHeroes, allies, newAllies; }
    public static class OctoberContent
    {
        public static OctoberRoster Roster => JsonUtility.FromJson<OctoberRoster>(Resources.Load<TextAsset>("October/ReferenceRoster").text);
        public static readonly string[] BondIds = { "hare", "archer", "root", "gauntlet", "regal", "clover", "fruit", "gear", "hat", "horn", "sprout", "knight", "ball", "morning", "rune", "mask", "hide", "resolve", "shoe" };
        public static readonly string[] BondNames = { "Springstep", "Longbow", "Rootkin", "Prizefighters", "Velvet Court", "Lucky Leaves", "Fruit Basket", "Clockwork Union", "Hat Society", "Little Horns", "Jade Nursery", "Crossing Knights", "Lantern Revel", "Morning Bells", "Runic School", "Masked Carnival", "Wild Stitch", "Unbroken", "Quicksoles" };
        public static readonly string[] ReferenceBonds = { "Bouncy Rabbit Ears", "Bow and Arrow", "Carrot", "Crimson Gauntlets", "Empire", "Four-Leaf Clover", "Fruit", "Gear", "Hat", "Horned Head", "Jade Cabbage", "Knight", "Melon-Striped Ball", "Morning Star", "Mysterious Rune", "Ominous Mask", "Primitive Leather Skirt", "Resolve!", "Shoe" };
        public static string HeroCopy(string reference) => reference switch
        {
            "Mr. Work" => "Lantern Stride charges while walking. Start with +5 HP, +5% speed and 0.2 HP regeneration each second.",
            "Derpy Wolf" => "Hero & friends speed +30%. Each 1% of your speed is +1% dodge and crit. When struck, you and Clockwork friends move and attack 10% faster for 3s. Armor -0.15.",
            "Sassy Senior" => "Never attacks. Taking damage makes her explode around herself; each Four-Leaf Clover friend makes it 50% stronger. Half your luck raises hero & friends damage (up to +50%). Hearts add 2% luck.",
            "Baggy" => "Stay rooted in combat and collect loot across the arena. Shop discount +30%. Cannot dodge; treasure detection -100%.",
            "Red Scale" => "Each drink bought grants 1 gold plus 1 per 10 maximum HP (up to 16; grows by wave), +2 max HP and -0.05 armor; the next fight has 10% more monsters per drink.",
            "Gulpy" => "Your hits and archive friends poison: a second later the target loses 10% of its health (bosses: half a hit). Hero & friends moves 30% faster. Each chest: +1 max HP, heal 4.",
            "Frosty" => "Moving: +40% attack speed, shots split 3 ways. Standing still: weaker shots (60%) and heal 15% max HP a second. Each friend strengthens your shots and body, but weakens friends slightly.",
            "Capped Gallant" => "Fight alone. Buying friends absorbs their support traits without adding a body. Your original solo whirl and bounded damage growth remain active.",
            "Steel Shell" => "Every 3 seconds every enemy takes your contact damage. Each Prizefighter activation adds 3 contact damage.",
            "Box Shell" => "Hero & friends flat ranged damage +3, melee damage -2. After 25 seconds of combat, melee attacks deal 40% more damage and ranged attacks deal 25% less.",
            "Jade Cabbage" => "Nursery shops sell Little Sprouts. Buying a Sprout adds 1 projectile (max 6) and costs 2 maximum HP. Two Sprouts grow into a random evolved friend plus a Grand Sprout.",
            "The Big Pot" => "Start with +8 contact damage. Each Morning Bells activation gives +0.15 armor. Taking damage adds 0.15 regeneration for this run. Enemies move 15% faster.",
            "Porcelain Doll" => "Healing is 2.5 times as effective, but armor starts 0.25 lower. Each Wild Stitch activation adds 0.4 regeneration. Taking damage adds 1 contact damage.",
            "Slippy" => "Brigade damage -55%. Knockback +200%. Each Clockwork Union activation adds 5 wave income. Treasure +30%.",
            "Card King" => "Light attacks (60% damage). Your own damage, attack speed, crit and projectile bonuses go to every friend; friends are 20% larger. Shops sell 2-star friends. More enemies appear.",
            "Sir Purrcival" => "+45% dodge, 50% less HP. Dodge cap: 90%. Dodges add treasure detection for this run. Crossing Knights deal double damage and +10 contact damage.",
            "Omnom" => "Lose 2 HP each second in battle. Each enemy defeated heals 2 HP. Regeneration and hearts do not heal. Hero & friends +30% speed, again at waves 3, 6 and 9. Monsters +20%.",
            "Periwinky" => "The dream shop opens after every wave. At wave start, maximum HP follows saved gold (20–400). Buying a spiked relic recruits a Bristle Ball. Brigade damage -50%; contact damage +15.",
            "Clementie" => "Light attacks (60% damage); best when you run into enemies. Move 30% faster, double HP, pickup range -50%. Your speed becomes contact damage for you and every friend. Friends stay close. Hero & friends attack speed -25%.",
            "Card Queen" => "Start each wave with 12 royal health; each injury takes exactly 1. Every injury pays gold and strikes every enemy on the field. Healing cannot refill it. Velvet Court friends +1 projectile.",
            "Bloom Shell" => "Start each wave with a balanced blessing: +2 flat melee and ranged damage, +10 HP, +5% speed and +0.15 armor.",
            "Belle" => "Every friend may appear in your shop. Monsters +30% and +60% health, rising 5% and 10% more each wave. Rerolls cost 3 less gold.",
            "Wise" => "Shops also sell other Dream Seekers. Buy one and its gifts become yours (+50%); each makes enemies +10% health and 1% more. Refresh costs 5 more. Your positive personal bonuses are 25% stronger.",
            "Nicole" => "Relics cost 20% less. Rerolls cost 5 more gold. Each paid refresh adds 1 income and 1% luck for this run.",
            "Anby" => "Light attacks (60%) until transformed, full strength while transformed. Contact damage x3. Every dodge looses a forward lightning barrage. +20% dodge.",
            "Billy" => "Friends never attack on their own: on each of your critical attacks, every ready friend has a 50% chance to use its equipped weapon at 180% damage, with its own windup, cooldown and star effects. Your shots cannot split. +10% crit, 2 projectiles.",
            "Nekomata" => "Move 20% faster, attack 30% faster. Sometimes she breaks into Run N' Stop: 3s of +20% speed and +40% attack speed. Every 3s moving pays 2 gold. Each friend: wave income -1.",
            "Burgerboo" => "Fire up to two delayed mortar shells. Shells cannot pierce. +0.4 armor, +50% movement speed, pickup range -50%. Enemies move 75% faster.",
            "Original:LanternWeaver" => "Every fourth attack stitches three nearby enemies with a delayed lantern thread. The thread shares one damage budget.",
            "Original:HourglassStag" => "Every 6 seconds, briefly slow nearby ordinary enemies and release an expanding clock ring. Bosses keep moving.",
            "Original:RainArchivist" => "Each opened chest heals 4 HP and records a raindrop. Every third record releases a fan of frost shots.",
            "Original:KiteAdmiral" => "A mouse admiral whose kite fleet flies over the brigade's archers.",
            "Original:SundaeDuchess" => "A porcelain duchess crowned in ice cream; what she freezes pays.",
            "Original:BonsaiClocksmith" => "A tortoise clocksmith with a bonsai garden that keeps time.",
            "Original:JugglingJester" => "A felt jester who never lets five balls touch the floor.",
            "Original:BellDiver" => "A frog diver who listens for what moves below.",
            _ => "Keep your original automatic skill and themed recruits. Equipment bonuses apply during this run only."
        };
    }
}
