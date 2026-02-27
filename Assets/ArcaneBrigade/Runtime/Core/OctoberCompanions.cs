using System;
using UnityEngine;
namespace ArcaneBrigade
{
    public static class OctoberCompanions
    {
        public static string Copy(string name) => name switch
        {
            "Alien" => "Each drink bought gives this ally +4% damage (max +40%).",
            "Applehead" => "Fruit Basket allies attack 8% faster.", "Athlete" => "For 4 seconds after you take damage, the team attacks 10% faster.",
            "Bullhorn Warrior" => "Clockwork Union allies gain 8% movement and attack speed.", "Camo Shroom" => "Basic hits slow ordinary enemies for 0.4 seconds.",
            "Cheer Bunny" => "Team movement speed +5%.", "Chieftain" => "Team damage +5%.", "Clown" => "After you take damage, heal 1 HP. Shared 1-second cooldown.",
            "Fierce Fighter" => "Each attack adds 1% attack speed, up to 20%, this wave. Every third hit makes a small blast.",
            "Floofy" => "Maximum HP +5. Armor +0.08.", "Grey Mage" => "When you attack, fire an extra bolt. 0.7-second cooldown; cannot trigger another response.",
            "Hopping Zombie" => "Maximum HP +4. Each 8 kills grants this ally 1% attack speed, up to 15% per wave.",
            "Intelligent Construct" => "Clockwork Union allies attack 4% faster.", "Iron Swordsman" => "Each ally of this same species grants it 10% attack speed, up to 30%.",
            "Judge" => "Each Lucky Leaves activation adds 4 wave income.", "Jungle Folk" => "Each Wild Stitch or Springstep activation grants this ally 10% damage and 5% reach.",
            "Little Witch" => "Runic School allies fire 30% larger shots with 2 extra pierce, but attack 15% slower.",
            "Masked Wrestler" => "Each injury this wave grants Prizefighter allies 1% attack speed, up to 15%.",
            "Matchstick Head" => "Each Morning Bells activation grants you 1 projectile, up to 3 extra.", "Monkey King" => "Quicksole allies move and attack 10% faster.",
            "Mummy" => "Basic hits poison the target for a delayed 25% weapon-damage strike. No poison chain.",
            "Peach" => "Basic kills have a 30% chance to heal you 1 HP.", "Phantom" => "Armor -0.08. Each heart collected adds 2% reach to this ally, up to 20%.",
            "Pirate" => "Each opened chest adds 2% attack speed to this ally, up to 20%.", "Pointy Hat Witch" => "Each opened chest adds 2% damage to this ally, up to 30%.",
            "Prince" => "Each Princess counterpart grants this ally 1 projectile, up to 3.", "Princess" => "Each Prince counterpart grants this ally 8% reach, up to 24%.",
            "Smiley Fox" => "Each paid refresh grants 2 gold and this ally +3% damage, up to 30%.", "Student" => "Fire 2 extra projectiles. Each deals 35% less damage.",
            "TV Head" => "Critical basic hits have a 25% chance to grant 1 gold. Shared 0.8-second cooldown.", "Young Lady" => "Drop a 1-HP heart every 5 seconds in combat.",
            "\"Art of Food\" Roshi" => "Each food bought adds 1 flat team damage. Food also adds a small next-wave health tradeoff.",
            "\"Bullied\" Sophia" => "Each attack grants this ally 1% attack speed, up to 20% this wave.",
            "\"Cars ARE My Kids\" Chris" => "Each Carousel Pony grants this ally 30% damage and 5 contact damage, up to 3 ponies.",
            "\"Colorful Fantasy\" Pinky" => "Reduce the first hit you take every 6 seconds by 25%.",
            "\"Enduring Suffering\" Dorothy" => "Once per wave, taking damage lowers your maximum HP by 2 and heals you fully. Does not refill royal health.",
            "\"Expert Nurturer\" Daisy" => "On purchase, gain 6 eggs once.", "\"Formatting Pixie\" Quentin" => "Allies are 12% smaller. Their hitboxes shrink equally.",
            "\"GLHF\" Nia" => "On purchase, gain one seeded random run bonus: damage, speed, luck or armor.",
            "\"Leisurely Walk\" Mary" => "Paid refreshes cost 2 less gold, minimum 1.", "\"Misfortune\" Emily" => "Taking damage grants 3 gold. Shared 2-second cooldown.",
            "\"Naming Demon\" Yan" => "Every third attack fires an unpredictable coloured echo at 25% weapon damage.",
            "\"Overtime King\" Philip" => "Wave income +15. Lose 0.5 HP every 5 seconds of combat.",
            "\"Pet Master\" Lisa" => "On purchase, recruit up to 4 friendly marbles.",
            "\"Shove Around\" Bella" => "On purchase, recruit up to 2 Balloon Mallets.",
            "\"Smug and Satisfied\" Jake" => "On purchase, recruit two junior helpers.",
            "\"The All-Knowing\" Walter" => "Every bond gains one supporting count. This effect does not stack with another Walter counterpart.",
            "\"The Creator\" Roger" => "On purchase, gain 100 gold once.", "\"Triumphant Return\" Daniel" => "Attacks from the beginning. After 30 seconds, deal triple weapon damage for the rest of the wave.",
            "\"Quick Production\" Tao" => "Team damage +20% and speed +10%. Enemies move 10% faster.",
            "\"Unstoppable\" Paul" => "Contact damage +40. Briefly distract nearby ordinary enemies for 2 seconds every 6 seconds. Bosses ignore the decoy.",
            "Vengeful Spirit" => "This ally has +15% critical chance.", "Attractive Witch" => "Basic hits gently pull ordinary enemies inward. Bosses cannot be pulled.",
            "Big Cabbage" => "Uses its equipped weapon and has 24 base contact damage before brigade bonuses. Can carry nursery bonuses.", "Domestic Spiked Ball" => "Uses its equipped weapon and has 18 base contact damage before brigade bonuses; gain 1 more per injury this wave, up to 20.",
            "Evil" => "Cannot merge. Each 8 personal kills grants you 2% attack speed, up to 20% this wave.",
            "Inflatable Hammer" => "Push ordinary enemies within the push cap. Basic damage is reduced by 70%.",
            "Little Cabbage" => "Nursery-exclusive. Uses its weapon and has 12 base contact damage before brigade bonuses. Three sprouts grow into a random two-star friend and a Grand Sprout. Each keeps an ingredient weapon.",
            "Monster Friend" => "An immortal friendly marble. Uses its own randomly assigned weapon.",
            "Pony" => "Carries one nearby ally while both continue attacking. Rider and mount remain immortal. Mount pairing is automatic.",
            "Original:TeacupCricket" => "Every fifth basic attack releases a short echo at 40% weapon damage.",
            "Original:RibbonOtter" => "Collecting a heart grants 1 extra HP of healing. Its Rain Script bond supports recovery.",
            "Original:SpindleWren" => "Lantern Guild allies gain 8% reach. Its threadwork belongs to both clockwork and lantern themes.",
            _ => "A resident of the Waiting Realm with a life beyond this crossing. Its listed support traits apply during this run."
        };
    }
}
