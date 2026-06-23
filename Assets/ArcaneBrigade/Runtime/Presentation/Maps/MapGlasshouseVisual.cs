using UnityEngine;

namespace ArcaneBrigade
{
    // The Glasshouse's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere
    // dispatch here). Props come from Resources/Environment/Glasshouse (MapIdentity.Folder); effects are drawn in code.

    public sealed partial class MapEnvironmentVisual
    {
        /// <summary>Draws the map's living elements from its MapEnvironmentState fields (Put, Ring, Band, Lane, ink).</summary>
        private void DrawGlasshouse(MapEnvironmentState s, float t) { }
        /// <summary>How much the map's own light lifts a standing prop at this spot (0 none, 1 fully lit).</summary>
        private float GlasshousePropLight(MapEnvironmentState s, Vector2 at) => 0;
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>The Glasshouse's air: until its own is written, its stand-in's (the Post Office's window light and draught).</summary>
        private void AirGlasshouse(MapEnvironmentState s, int motes, float clock) => AirPostOffice(s, motes, clock);
    }
}
