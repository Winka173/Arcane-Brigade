using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Bizarre Arsenal: weapons found in Bizarre Brigade research (Docs/Bizarre_Arsenal.md §1). Each value is a
    /// mechanic resolved by CombatSimulation.ResolveArsenalAttack; nothing radiates from the wielder's own body.</summary>
    public enum ArsenalWeapon
    {
        None, CherryFuseBomb, PopperSticks, DreamRacket, BambooVase, RoseBouquet, HomeRunBat, CandyDrill, InkyOctopus, GrabberClaw,
        StarforgeClaymore, DragonLanternGlaive, TwinMoonStaff, PlushShark, DrumstickClub, PaperShuriken, SquirtTrio, ConfettiBlunderbuss,
        PlungerPopper, SignpostSlammer, DetonatorBox, FirecrackerPinwheel, GumballGatling, BalloonBuddy, GlitchToybox,
        // research pass 3 (the Naming Demon's full cycle and the Potato relic)
        SaucerBlaster, FanfareHorn, WindupWrench, CorkRifle, CatcherMitt, FizzBottle, LeekBaton, FlySwatter,
        // our own takes on the three items too small to identify in the footage
        DowsingRod, StarKnuckle, BuzzerBlock,
        // research pass 4 (forty more videos and a ranking of every companion weapon)
        PawMitten, HearthBrick, VerdictGavel, TalismanIngot, KunaiFan, BareKnuckles,
        // our own takes on pass 4's two unconfirmed items (the Little Witch's crystal ball, the Camo Shroom's shears)
        FortuneGlobe, TopiaryShears
    }

    public sealed partial class WeaponDefinition { public ArsenalWeapon Arsenal; }

    /// <summary>Something an Arsenal weapon leaves in the room: a lobbed bomb, a firecracker mine, a bamboo shoot, a petal
    /// ribbon, a clinging octopus, a planted sign, a pinwheel trap, a grinding drill, a batted or grabbed enemy in flight.</summary>
    public enum ArsenalPropKind { Bomb, Bomblet, Stick, Vase, Shoot, Petals, Octopus, Sign, ToyBomb, Pinwheel, Drill, Bump, Batted, Grab, Plunge, Wound, Fizz, Catch, Buzzer, Brick, Kunai, Snip }

    public sealed class ArsenalProp
    {
        public ArsenalPropKind Kind;
        public UnitState Source;
        public WeaponDefinition Weapon;
        public Vector2 Start, Position, Direction;
        public float Born, LandAt, FuseAt, At, Until, NextTick;
        public float Radius, Damage;
        public bool Critical, Resolved;
        public int Stage, TargetId, Order;
        public CombatCause Cause;
        public readonly System.Collections.Generic.HashSet<int> Victims = new System.Collections.Generic.HashSet<int>();
    }
}
