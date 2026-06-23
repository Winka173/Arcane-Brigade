using UnityEngine;

namespace ArcaneBrigade
{
    // The Forgotten Theatre (waves 15-17; Docs/Map_Mechanics_2026-10-25.md section 5): the three-colour spotlight, stage smoke,
    // the ghost audience, the velvet curtain, the firework machine. Requirement 25/10/2026: the map's state, tunings and rules
    // live in this file only; MapEnvironmentCombat and MapBoosts dispatch to the hooks below. Its mechanics are still to be written.

    public sealed partial class MapEnvironmentState
    {
        // Theatre state goes here
    }

    public static partial class MapEnvironmentRules
    {
        // Theatre tunings go here
    }

    public sealed partial class CombatSimulation
    {
        /// <summary>Sets the map up at the start of an ordinary wave (s.Anchor is the room's centre near the hero).</summary>
        private void InitTheatre(MapEnvironmentState s) { }
        /// <summary>Runs the map each simulation step of the wave.</summary>
        private void StepTheatre(float deltaTime) { }
        /// <summary>A stat the hero gets while standing somewhere on this map (the spotlight; MapBoosts.MapStandingBoost).</summary>
        private float TheatreStandingBoost(MapStat stat) => 0;
        /// <summary>How this map changes one enemy along one axis (1 = unchanged; MapBoosts.MapEnemyFactor).</summary>
        private float TheatreEnemyFactor(UnitState unit, MapEnemyAxis axis) => 1;
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float TheatreAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float TheatreHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map.</summary>
        private int TheatreCoins(UnitState target, int coins) => coins;
        /// <summary>For captures: every element of the map on screen at once.</summary>
        private void TheatreCaptureFixture() { }
    }
}
