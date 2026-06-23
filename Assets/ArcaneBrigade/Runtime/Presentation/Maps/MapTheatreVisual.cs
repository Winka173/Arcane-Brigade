using UnityEngine;

namespace ArcaneBrigade
{
    // The Forgotten Theatre's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere
    // dispatch here). Props come from Resources/Environment/Theatre (MapIdentity.Folder); effects are drawn in code.

    public sealed partial class MapEnvironmentVisual
    {
        /// <summary>Draws the map's living elements from its MapEnvironmentState fields (Put, Ring, Band, Lane, ink).</summary>
        private void DrawTheatre(MapEnvironmentState s, float t) { }
        /// <summary>How much the map's own light lifts a standing prop at this spot (0 none, 1 fully lit; the spotlight).</summary>
        private float TheatrePropLight(MapEnvironmentState s, Vector2 at) => 0;
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>The Theatre's air: until its own is written, its stand-in's (the Lamp Hall's dark hall; no lamps, so only its grade).</summary>
        private void AirTheatre(MapEnvironmentState s, int motes, float clock) => AirLampHall(s, motes, clock);
    }
}
