using UnityEngine;

namespace ArcaneBrigade
{
    public enum BondSpawnKind { Phoenix, IceSoldier, Shade, LampWisp, Snack, Clover, Chef, Leviathan, WindBeast, Sprout, Treant, Colossus, Turret, Spirit, Orchestra, Dish, Sapling, GreatTree }
    public readonly struct BondSpawnRequest
    {
        public readonly BondSpawnKind Kind;
        public readonly Vector2 Position;
        public readonly int SourceId, Coins, FreeRerolls;
        public readonly float Lifetime, Damage, HealMaxHealthFraction;
        public BondSpawnRequest(BondSpawnKind kind, Vector2 position, int sourceId = 0, float lifetime = 0,
            float damage = 0, float healMaxHealthFraction = 0, int coins = 0, int freeRerolls = 0)
        {
            Kind = kind; Position = position; SourceId = sourceId; Lifetime = lifetime; Damage = damage;
            HealMaxHealthFraction = healMaxHealthFraction; Coins = coins; FreeRerolls = freeRerolls;
        }
    }
    public readonly struct BondDamageRequest
    {
        public readonly UnitState Target, Attacker;
        public readonly float Damage;
        public readonly string Rule;
        public BondDamageRequest(UnitState target, UnitState attacker, float damage, string rule)
        { Target = target; Attacker = attacker; Damage = damage; Rule = rule; }
    }
    public readonly struct BondRuleEvent
    {
        public readonly string Rule;
        public readonly Vector2 Position;
        public readonly int SubjectId;
        public readonly float Value;
        public BondRuleEvent(string rule, UnitState subject, float value = 0)
        { Rule = rule; Position = subject.Position; SubjectId = subject.Id; Value = value; }
    }
    public sealed class BondEnemyState
    {
        public int Burn, Chill, Poison, Charge, Collisions, DischargeWave;
        public float BurstReadyAt; public UnitState MarkSource;
        public float SoakedUntil,RootedUntil;
        public int ClusterNeighbors;
        public float BurnUntil, PoisonUntil, FrozenUntil, StunnedUntil, BurnWeapon, PoisonWeapon;
        public bool Marked, FrozenHitReady, DeathHandled;
        public UnitState BurnSource, PoisonSource;
    }
}
