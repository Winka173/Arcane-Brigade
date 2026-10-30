using UnityEngine;
namespace ArcaneBrigade
{
    // Section 1b: class defenses, stars and the hero armour-item share are points.
    public static class FriendDefenseTuning
    {
        public const float ArmorPerStar = 2, HealthPerStar = .4f, ItemShare = .5f, ArmorBreakPerHit = 1;
        public const float GuardArmor = 12, StrikerArmor = 6, ControllerArmor = 5, SummonerArmor = 4, SupportArmor = 3, ShooterArmor = 2;
        public const float GuardHealth = 1.6f, StrikerHealth = 1.2f, ControllerHealth = 1, SummonerHealth = .9f, SupportHealth = .9f, ShooterHealth = .8f;
        public static void ClassStats(string id, out float armor, out float health)
        {
            if (!CompanionClassTuning.TryGetRole(id, out CompanionRole role))
            { armor = 0; health = 1; return; }
            switch (role)
            {
                case CompanionRole.Guard: armor = GuardArmor; health = GuardHealth; return;
                case CompanionRole.Striker: armor = StrikerArmor; health = StrikerHealth; return;
                case CompanionRole.Controller: armor = ControllerArmor; health = ControllerHealth; return;
                case CompanionRole.Summoner: armor = SummonerArmor; health = SummonerHealth; return;
                case CompanionRole.Support: armor = SupportArmor; health = SupportHealth; return;
                default: armor = ShooterArmor; health = ShooterHealth; return;
            }
        }
        public static float Armor(string id, int stars)
        { ClassStats(id, out float armor, out float health); return armor + Mathf.Max(0, stars - 1) * ArmorPerStar; }
        public static float Health(string id, int stars)
        { ClassStats(id, out float armor, out float health); return health * (1 + Mathf.Max(0, stars - 1) * HealthPerStar); }
        public static float SharedItemPoints(float armorValue)
        { return Mathf.Floor(Mathf.Max(0, armorValue / RunModifierRules.ArmorPoint) * ItemShare); }
    }
}
