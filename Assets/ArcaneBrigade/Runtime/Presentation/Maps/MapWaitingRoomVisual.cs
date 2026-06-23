using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Waiting Room's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere dispatch here).

    public sealed partial class MapEnvironmentVisual
    {
        private void DrawWaitingRoom(MapEnvironmentState s, float t)
        {
            // the map's fixed shape: its rows of chairs
            foreach (Vector2 piece in s.WallProps) Put("WaitingRoom/Chair_Stand", piece, 1.35f);
            bool calling = s.CalledDoor >= 0;
            bool lit = calling && t < s.OpenAt + .8f && (calm || still || Mathf.Repeat(t * 3.5f, 1) < .6f);
            Put("WaitingRoom/" + (lit ? "Board_Flash" : "Board_Idle"), s.Board, 2.2f);
            for (int d = 0; d < 4; d++)
            {
                bool called = calling && d == s.CalledDoor;
                string art = !called ? "Door_Closed" : t >= s.OpenAt ? "Door_Open" : t >= s.CallAt + 1.2f ? "Door_Ajar" : "Door_Closed";
                Put("WaitingRoom/" + art, s.Doors[d], 2.3f, flip: s.Doors[d].x > s.Anchor.x);
                // each door's number as brass pips over it; the called number shines on the board too
                Pips(s.Doors[d] + Vector2.up * 2.55f, d + 1, called ? 1 : .45f, called);
            }
            if (calling)
            {
                Pips(s.Board + Vector2.up * 2.45f, s.CalledDoor + 1, 1, true);
                // in front of the called door: an amber rim where the pack pours out, closing as the door opens
                if (t < s.CloseAt) Ring(s.Doors[s.CalledDoor] + Vector2.down * .5f, 1.25f, .7f, Mathf.Clamp01((t - s.CallAt) / MapEnvironmentRules.DoorCall), ArsenalInk.Amber, t < s.OpenAt ? .95f : .5f, 180, 360);
                // the hero shutting it: a gold ring closes round the door as it is held
                if (s.DoorShut > .01f) Ring(s.Doors[s.CalledDoor] + Vector2.up * .2f, 1.45f, 1.1f, s.DoorShut, ArsenalInk.Gold, .95f, 0, 360);
            }
            foreach (MapEnvironmentState.Chair chair in s.Chairs)
            {
                if (t >= chair.WarnAt && t < chair.SlideAt)
                {
                    float charge = Mathf.Clamp01((t - chair.WarnAt) / MapEnvironmentRules.ChairWarn);
                    Lane(chair.From, chair.To, .5f, charge, ArsenalInk.Amber, .95f);
                    Put("WaitingRoom/Chair_Stand", chair.Home, 1.35f, angle: still ? 0 : Mathf.Sin(t * 32) * 5 * charge);
                }
                else if (t >= chair.SlideAt && t < chair.SlideEnd) Put("WaitingRoom/Chair_Slide", chair.Position, 1.35f, flip: chair.To.x < chair.From.x);
                else if (t >= chair.StayUntil && t < chair.ReturnEnd) Put("WaitingRoom/Chair_Slide", chair.Position, 1.35f, flip: chair.Home.x < chair.To.x);
                else Put("WaitingRoom/Chair_Stand", chair.Position, 1.35f);
            }
            int shown = 0;
            foreach (KeyValuePair<int, float> trip in s.Tripped)
            {
                if (trip.Value <= t || shown >= 12) continue;
                UnitState u = sim.UnitForPresentation(trip.Key); if (u == null || !u.Alive) continue;
                ink.Orbit(Head(u), .28f, t, InkToken.Star, ArsenalInk.Gold, 3, Mathf.Clamp01((trip.Value - t) / .3f));
                shown++;
            }
        }
        /// <summary>How much the map's own light lifts a standing prop at this spot (0 none, 1 fully lit).</summary>
        private float WaitingRoomPropLight(MapEnvironmentState s, Vector2 at) => 0;
        /// <summary>Brass pips (a door's number) in a short row.</summary>
        private void Pips(Vector2 at, int count, float alpha, bool hot)
        {
            for (int i = 0; i < count; i++)
                ink.TokenAt(InkToken.Coin, hot ? ArsenalInk.Gold : ArsenalInk.Leather, at + new Vector2((i - (count - 1) * .5f) * .3f, 0), hot ? .3f : .24f, 0, alpha);
        }
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>Warm sepia, low fog drifting along the floor, dust motes in the lamp light.</summary>
        private void AirWaitingRoom(MapEnvironmentState s, int motes, float clock)
        {
            Grade(new Color(.96f, .84f, .66f, .07f), new Color(.2f, .11f, .05f, .32f));
            // low fog banks along the floor, drifting slowly across
            for (int i = 0; i < 5; i++)
            {
                float y = Mathf.Lerp(-.85f, .55f, i / 4f) * halfH;
                Vector2 at = Wrap(new Vector2(H(i, 1) * 30, 0), new Vector2(.18f + .06f * H(i, 2), 0)); at.y = centre.y + y;
                Put(MechanicInk.Glow, at, new Vector2(halfW * 1.1f, 1.3f + H(i, 3) * .6f), 0, new Color(.93f, .9f, .86f, .13f), HazeOrder);
            }
            // dust motes turning in the lamp light
            for (int i = 0; i < motes; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 4) * 40, H(i, 5) * 40), new Vector2(.05f * (H(i, 6) - .5f), .07f + .05f * H(i, 7)));
                at.x += still ? 0 : Mathf.Sin(t * .7f + i) * .15f;
                float twinkle = calm || still ? .7f : .55f + .45f * Mathf.Sin(t * 1.3f + i * 2.1f);
                Put(MechanicInk.Glow, at, Vector2.one * (.07f + .06f * H(i, 8)), 0, new Color(1, .93f, .72f, .42f * twinkle), AirOrder);
            }
        }
    }
}
