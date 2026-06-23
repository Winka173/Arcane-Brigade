using UnityEngine;

namespace ArcaneBrigade
{
    // The Archive Vault's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere dispatch here).

    public sealed partial class MapEnvironmentVisual
    {
        private void DrawArchiveVault(MapEnvironmentState s, float t)
        {
            // the map's fixed shape: its rows of shelves
            foreach (Vector2 piece in s.WallProps) Put("ArchiveVault/Shelf_Straight", piece, 2.4f);
            bool ringing = t >= s.ChimeAt - MapEnvironmentRules.ChimeWarn && t < s.ChimeAt + .9f;
            Put("ArchiveVault/" + (ringing ? s.Twelfth ? "Clock_Twelfth" : "Clock_Chime" : "Clock_Idle"), s.Clock, 2.6f);
            if (s.SlowUntil > t || s.RushUntil > t)
            {
                bool slow = s.SlowUntil > t;
                int shown = 0;
                foreach (UnitState u in sim.Units)
                {
                    if (shown >= 16) break;
                    if (!u.Alive || u.Faction != Faction.Enemy || sim.Champion == null || (u.Position - sim.Champion.Position).sqrMagnitude > 64) continue;
                    ink.TokenAt(slow ? InkToken.Moon : InkToken.Spark, slow ? ArsenalInk.Midnight : ArsenalInk.Amber, Head(u) + Vector2.up * .15f, .28f, still ? 0 : t * (slow ? 30 : 400), .85f);
                    shown++;
                }
            }
            foreach (MapEnvironmentState.Shelf shelf in s.Shelves)
            {
                bool moving = t >= shelf.MoveAt && t < shelf.MoveEnd, warning = t >= shelf.WarnAt && t < shelf.MoveAt;
                if (warning && (shelf.To - shelf.From).sqrMagnitude > .01f) Lane(shelf.From, shelf.To, .8f, Mathf.Clamp01((t - shelf.WarnAt) / MapEnvironmentRules.ShelfWarn), ArsenalInk.Amber, .95f);
                Put(moving ? "ArchiveVault/Shelf_Turning" : "ArchiveVault/Shelf_Straight", shelf.Position, 2.4f);
            }
            // the ledger plaque and its area, one wine wash (kills inside it pay double; no outline ring)
            Put("ArchiveVault/Ledger", s.Ledger + Vector2.up * .1f, 1.7f);
            float r = MapEnvironmentRules.LedgerRadius;
            Ring(s.Ledger, r, r, .3f, ArsenalInk.Wine, .6f);
        }
        /// <summary>How much the map's own light lifts a standing prop at this spot (0 none, 1 fully lit).</summary>
        private float ArchiveVaultPropLight(MapEnvironmentState s, Vector2 at) => 0;
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>Heavy dusk haze, the clock's slow ticking flicker, ink motes drifting up.</summary>
        private void AirArchiveVault(MapEnvironmentState s, int motes, float clock)
        {
            // the vault clock's slow tick: a faint lift of the dusk light on each second (none with Reduced Flashes or Motion)
            float tick = calm || still || sparse ? 0 : Mathf.Exp(-Mathf.Repeat(clock, 1f) * 5f);
            Grade(new Color(.86f, .58f, .48f, .1f + .035f * tick), new Color(.13f, .05f, .15f, .4f));
            // heavy haze banks
            for (int i = 0; i < 4; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 1) * 30, 0), new Vector2(-.08f, 0)); at.y = centre.y + Mathf.Lerp(-.7f, .7f, i / 3f) * halfH;
                Put(MechanicInk.Glow, at, new Vector2(halfW * 1.3f, 2.2f), 0, new Color(.74f, .6f, .7f, .12f), HazeOrder);
            }
            // ink motes drifting up
            for (int i = 0; i < motes; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 2) * 40, H(i, 3) * 40), new Vector2(.04f * (H(i, 4) - .5f), .16f + .1f * H(i, 5)));
                at.x += still ? 0 : Mathf.Sin(t * .9f + i * 1.7f) * .12f;
                Put(MechanicInk.Glow, at, Vector2.one * (.08f + .06f * H(i, 6)), 0, new Color(.16f, .09f, .26f, .5f), AirOrder);
            }
        }
    }
}
