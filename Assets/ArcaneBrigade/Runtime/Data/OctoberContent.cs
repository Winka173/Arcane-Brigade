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
            "Derpy Wolf" => "Keep moving to release clock pulses. Team speed +20%; start with +10% dodge and critical chance, but 0.15 less armor.",
            "Sassy Senior" => "You use your equipped weapon normally. Half your luck bonus also raises team damage, up to +50%. Taking damage releases a short burst. Hearts add 2% luck for this run.",
            "Baggy" => "Stay rooted in combat and collect loot across the arena. Shop discount +30%. Cannot dodge; treasure detection -100%.",
            "Red Scale" => "Each drink bought grants 1 gold plus 1 per 10 maximum HP (up to 16). Each drink adds 2 maximum HP, but lowers armor by 0.02 and adds 2% more enemies (caps: 0.4 armor, 40% enemies).",
            "Gulpy" => "Your hits and archive allies add a delayed poison strike at 25% weapon damage. Everyone moves 15% faster. Each chest grants 1 maximum HP and heals 4 HP.",
            "Frosty" => "Attack while moving or resting; rest to heal 5% maximum HP each second. Each ally strengthens your shots and body, but weakens allies slightly. Moving shots may split.",
            "Capped Gallant" => "Fight alone. Buying companions absorbs their support traits without adding a body. Your original solo whirl and bounded damage growth remain active.",
            "Steel Shell" => "Standing still prepares a reflection. Every 3 seconds release nearby contact damage as a pulse. Each Prizefighter activation adds 3 contact damage.",
            "Box Shell" => "Team flat ranged damage +3, melee damage -2. After 25 seconds of combat, melee attacks deal 40% more damage and ranged attacks deal 25% less.",
            "Jade Cabbage" => "Nursery shops sell Little Sprouts. Buying a Sprout adds 1 projectile (max 6) and costs 2 maximum HP. Three Sprouts grow into a random evolved friend plus a Grand Sprout.",
            "The Big Pot" => "Start with +8 contact damage. Each Morning Bells activation gives +0.15 armor. Taking damage adds 0.15 regeneration for this run. Enemies move 10% faster.",
            "Porcelain Doll" => "Healing is 2.5 times as effective, but armor starts 0.25 lower. Each Wild Stitch activation adds 0.4 regeneration. Taking damage adds 1 contact damage.",
            "Slippy" => "Ordinary attacks deal 35% less damage. Push ordinary enemies harder, within the push cap. Each Clockwork Union activation adds 4 wave income. Treasure +20%.",
            "Card King" => "Your equipped weapon attacks automatically. Your personal damage, attack speed, critical chance and projectile bonuses also help allies. Shops may sell evolved companions. More enemies appear.",
            "Sir Purrcival" => "Start with 35% dodge and 35% less HP. Dodge cap: 75%. Dodges add treasure detection for this run. Crossing Knights deal 30% more damage.",
            "Omnom" => "Lose 1 HP each second in battle. Each enemy defeated heals 1 HP. Regeneration and hearts do not heal. Every fourth wave raises everyone's movement speed.",
            "Periwinky" => "Dream shops always open. At wave start, maximum HP follows saved gold (20–400). Buying a spiked relic recruits a Bristle Ball. Team damage -20%.",
            "Clementie" => "Use your equipped weapon. Move 20% faster and gain 50% HP. Allies stay close; contact damage scales with your movement speed. Team attack speed -25%.",
            "Card Queen" => "Start each wave with 12 royal health. Each injury consumes exactly 1 royal health and releases a short retaliation. Ordinary healing cannot refill it. Velvet Court allies gain a projectile.",
            "Bloom Shell" => "Start each wave with a balanced blessing: +2 flat melee and ranged damage, +10 HP, +5% speed and +0.15 armor.",
            "Belle" => "Three companion themes (Hearth, Oddity, Orchard) instead of two. Enemy health +12% and count +6% through wave 8, rising to +35% / +20% at wave 20. Rerolls cost 1 less gold.",
            "Wise" => "Shops offer memory companions. Buying one absorbs its support trait instead of adding a body. Each memory makes enemies 4% tougher. Your positive personal modifiers are 25% stronger.",
            "Nicole" => "Relics cost 20% less. Rerolls cost 2 more gold. Each paid refresh adds 1 income and 1% luck for this run.",
            "Anby" => "You use your equipped weapon normally. Your contact damage is doubled. Dodging triggers a short forward lightning barrage. Start with +20% dodge.",
            "Billy" => "Your shots cannot split. Allies attack normally and also respond to your critical attacks, with their own weapon damage and a short cooldown. Start with +10% critical chance and 2 projectiles.",
            "Nekomata" => "Move 20% faster. Every 3 seconds spent moving in combat grants 2 gold. Each ally reduces wave income by 1.",
            "Burgerboo" => "Fire up to two delayed mortar shells. Shells cannot pierce. Start with +0.4 armor and +15% movement speed. Enemies move 20% faster.",
            "Original:LanternWeaver" => "Every fourth attack stitches three nearby enemies with a delayed lantern thread. The thread shares one damage budget.",
            "Original:HourglassStag" => "Every 6 seconds, briefly slow nearby ordinary enemies and release an expanding clock ring. Bosses keep moving.",
            "Original:RainArchivist" => "Each opened chest heals 4 HP and records a raindrop. Every third record releases a fan of frost shots.",
            _ => "Keep your original automatic skill and themed recruits. Equipment bonuses apply during this run only."
        };
    }
}
