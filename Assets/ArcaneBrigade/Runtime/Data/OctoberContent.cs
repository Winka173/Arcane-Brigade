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
            "Derpy Wolf" => "Team speed +20%. Speed also becomes dodge and critical chance (+10%, up to +40%). Armor -0.15.",
            "Sassy Senior" => "Never attacks. Taking damage makes her explode around herself; each Four-Leaf Clover friend makes it 50% stronger. Half your luck raises team damage (up to +50%). Hearts add 2% luck.",
            "Baggy" => "Stay rooted in combat and collect loot across the arena. Shop discount +30%. Cannot dodge; treasure detection -100%.",
            "Red Scale" => "Each drink bought grants 1 gold plus 1 per 10 maximum HP (up to 16; grows by wave). Each drink adds 2 maximum HP, but lowers armor by 0.02 and adds 2% more enemies (caps: 0.4 armor, 40% enemies).",
            "Gulpy" => "Your hits and archive friends poison: a second later the target loses 6% of its health (bosses: half a hit). Everyone moves 15% faster. Each chest: +1 max HP, heal 4.",
            "Frosty" => "Attack while moving or resting; rest to heal 5% maximum HP each second. Each friend strengthens your shots and body, but weakens friends slightly. Moving shots may split.",
            "Capped Gallant" => "Fight alone. Buying friends absorbs their support traits without adding a body. Your original solo whirl and bounded damage growth remain active.",
            "Steel Shell" => "Every 3 seconds every enemy takes your contact damage. Each Prizefighter activation adds 3 contact damage.",
            "Box Shell" => "Team flat ranged damage +3, melee damage -2. After 25 seconds of combat, melee attacks deal 40% more damage and ranged attacks deal 25% less.",
            "Jade Cabbage" => "Nursery shops sell Little Sprouts. Buying a Sprout adds 1 projectile (max 6) and costs 2 maximum HP. Two Sprouts grow into a random evolved friend plus a Grand Sprout.",
            "The Big Pot" => "Start with +8 contact damage. Each Morning Bells activation gives +0.15 armor. Taking damage adds 0.15 regeneration for this run. Enemies move 10% faster.",
            "Porcelain Doll" => "Healing is 2.5 times as effective, but armor starts 0.25 lower. Each Wild Stitch activation adds 0.4 regeneration. Taking damage adds 1 contact damage.",
            "Slippy" => "Ordinary attacks deal 35% less damage. Push ordinary enemies harder, within the push cap. Each Clockwork Union activation adds 4 wave income. Treasure +20%.",
            "Card King" => "Never attacks. Your own damage, attack speed, crit and projectile bonuses go to every friend; friends are 20% larger. Shops sell 2-star friends. More enemies appear.",
            "Sir Purrcival" => "Start with 35% dodge and 35% less HP. Dodge cap: 75%. Dodges add treasure detection for this run. Crossing Knights deal 30% more damage.",
            "Omnom" => "Lose 1 HP each second in battle. Each enemy defeated heals 1 HP. Regeneration and hearts do not heal. Every fourth wave raises everyone's movement speed.",
            "Periwinky" => "Dream shops follow the shared 3-4-wave schedule. At wave start, maximum HP follows saved gold (20–400). Buying a spiked relic recruits a Bristle Ball. Team damage -20%.",
            "Clementie" => "Never attacks: run into enemies. Move 20% faster, +50% HP. Your speed becomes contact damage for you and every friend. Friends stay close. Team attack speed -25%.",
            "Card Queen" => "Start each wave with 18 royal health; each injury takes exactly 1. Every injury pays gold and strikes every enemy on the field. Healing cannot refill it. Velvet Court friends +1 projectile.",
            "Bloom Shell" => "Start each wave with a balanced blessing: +2 flat melee and ranged damage, +10 HP, +5% speed and +0.15 armor.",
            "Belle" => "Three friend themes (Hearth, Oddity, Orchard) instead of two. Enemy health +12% and count +6% through wave 8, rising to +35% / +20% at wave 20. Rerolls cost 1 less gold.",
            "Wise" => "Shops offer memory friends. Buying one absorbs its support trait instead of adding a body; your last two memories fight beside you as paper exhibits. Each memory makes enemies 2% tougher (max 12%). Your positive personal modifiers are 25% stronger.",
            "Nicole" => "Relics cost 20% less. Rerolls cost 2 more gold. Each paid refresh adds 1 income and 1% luck for this run.",
            "Anby" => "Only attacks while transformed. Contact damage x3. Every dodge looses a forward lightning barrage. +20% dodge.",
            "Billy" => "Friends never attack on their own: on each of your critical hits, every friend has a 60% chance to fire its weapon. Your shots cannot split. +10% crit, 2 projectiles.",
            "Nekomata" => "Move 20% faster, attack 30% faster. Every 3 seconds spent moving in combat grants 2 gold. Each friend reduces wave income by 1.",
            "Burgerboo" => "Fire up to two delayed mortar shells. Shells cannot pierce. Start with +0.4 armor and +15% movement speed. Enemies move 20% faster.",
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
