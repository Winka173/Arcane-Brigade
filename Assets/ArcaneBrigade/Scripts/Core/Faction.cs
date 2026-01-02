namespace ArcaneBrigade.Core
{
    /// <summary>Faction tag for allies (and used by Champion/Enemy for synergy filtering later).</summary>
    public enum Faction
    {
        None = 0,
        Vanguard = 1,   // offense
        Bulwark = 2,    // defense
        Mystic = 3,     // support
        Specialist = 4, // hybrid (post-MVP)
    }

    /// <summary>Which side a unit fights for. Drives target acquisition.</summary>
    public enum Allegiance
    {
        Player = 0,  // Champion + Allies
        Enemy = 1,   // Scourge
    }

    /// <summary>High-level weapon category. Determines AI behaviour and visuals.</summary>
    public enum WeaponKind
    {
        // Melee
        Blade = 0,
        Blunt = 1,
        Piercing = 2,
        Claws = 3,
        Shield = 4,
        // Ranged
        Crossbow = 100,
        Catapult = 101,
        Staff = 102,
        Beam = 103,
        Mirror = 104,
    }

    /// <summary>Damage typing for effectiveness matrix (most MVP enemies don't use it yet).</summary>
    public enum DamageType
    {
        Physical = 0,
        Fire = 1,
        Holy = 2,
        Arcane = 3,
    }
}
