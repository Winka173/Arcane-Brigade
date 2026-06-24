using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Waiting Room's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere
    // dispatch here). Props are painted art (Resources/Environment/WaitingRoom); every effect is ink drawn in code: the ceiling
    // dust's shafts (a shader, with an ink fallback), the door's red glow, countdown, health bar and cracks, the tickets' glow,
    // the clock's hand and chimes, the waiters' temper. Danger to the hero is red, gifts are brass, amber and cream, enemy
    // states violet. Areas are solid fills of one colour with no rim.

    public sealed partial class MapEnvironmentVisual
    {
        private MapWaitingRoomLayer roomPaper;
        private MapWaitingRoomLayer RoomPaperLayer() { if (roomPaper == null) roomPaper = MapWaitingRoomLayer.Under(transform); return roomPaper; }

        private static readonly InkPalette RoomRed = new InkPalette(new Color(.24f, .04f, .06f), new Color(.88f, .2f, .18f), new Color(1, .8f, .72f), new Color(1, .4f, .3f), new Color(.98f, .78f, .4f));
        // the queue tickets' colours: red, yellow, grey, white, blue
        private static readonly InkPalette[] RoomTicketInk = { RoomRed, ArsenalInk.Gold, ArsenalInk.Silver, ArsenalInk.Cotton, ArsenalInk.Cyan };
        private static readonly string[] RoomTicketArt = { "WaitingRoom/Ticket_Red", "WaitingRoom/Ticket_Yellow", "WaitingRoom/Ticket_Grey", "WaitingRoom/Ticket_White", "WaitingRoom/Ticket_Blue" };
        private const float RoomClockHeight = 1.8f, RoomClockFace = .62f, RoomClockDial = .5f;

        // what was already said (each word once), the door's blows seen, the waiters whose temper was announced
        private float roomSaidCall = -100, roomSaidBroken = -100, roomSaidOpen = -100; private int roomSaidBeat = -1, roomDoorHits;
        private readonly Dictionary<int, float> roomAngrySaid = new Dictionary<int, float>();
        // footprints left by enemies that walked through the dust
        private sealed class RoomFoot { public Vector2 Last; public bool Left; public float DustyUntil; }
        private struct RoomPrint { public Vector2 At, Dir; public float Born; }
        private readonly Dictionary<int, RoomFoot> roomFeet = new Dictionary<int, RoomFoot>();
        private readonly List<RoomPrint> roomPrints = new List<RoomPrint>();
        private MapEnvironmentState roomSeen;

        private void DrawWaitingRoom(MapEnvironmentState s, float t)
        {
            if (s != roomSeen) { roomSeen = s; roomSaidCall = roomSaidBroken = roomSaidOpen = -100; roomSaidBeat = -1; roomDoorHits = 0; roomAngrySaid.Clear(); roomFeet.Clear(); roomPrints.Clear(); } // a new night
            MapWaitingRoomLayer layer = RoomPaperLayer(); layer.Begin();
            bool sparse = PresentationSettings.ReducedEffects;
            // the map's fixed shape: its rows of chairs (when a layout lays them)
            foreach (Vector2 piece in s.WallProps) Put("WaitingRoom/Chair_Stand", piece, 1.35f);
            RoomDrawDust(s, t, layer, sparse);
            RoomDrawBoard(s, t, layer);
            RoomDrawTickets(s, t);
            RoomDrawClock(s, t, layer);
            RoomDrawWaiters(s, t);
            layer.End();
        }
        /// <summary>How much the map's own light lifts a standing prop at this spot (0 none, 1 fully lit): the glowing clock.</summary>
        private float WaitingRoomPropLight(MapEnvironmentState s, Vector2 at)
        {
            float t = sim != null ? sim.Elapsed : 0;
            return s.RoomClockGlowing(t) && (at - s.RoomClock).sqrMagnitude < .25f ? .6f : 0;
        }
        /// <summary>Brass pips (a door's number) in a short row.</summary>
        private void Pips(Vector2 at, int count, float alpha, bool hot)
        {
            for (int i = 0; i < count; i++)
                ink.TokenAt(InkToken.Coin, hot ? ArsenalInk.Gold : ArsenalInk.Leather, at + new Vector2((i - (count - 1) * .5f) * .3f, 0), hot ? .3f : .24f, 0, alpha);
        }
        private static float RoomOutBack(float x) { x = Mathf.Clamp01(x); const float c = 1.6f; float y = x - 1; return 1 + (c + 1) * y * y * y + c * y * y; }
        private static float RoomOutCubic(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }

        // ------------------------------------------------------------------ ceiling dust
        private void RoomDrawDust(MapEnvironmentState s, float t, MapWaitingRoomLayer layer, bool sparse)
        {
            Material shaft = sparse ? null : MapWaitingRoomLayer.Shader("MapWaitingRoomDust");
            float clock = still ? 0 : t, rx = MapEnvironmentRules.RoomDustRadiusX, ry = MapEnvironmentRules.RoomDustRadiusY;
            for (int i = 0; i < s.RoomDust.Count; i++)
            {
                MapEnvironmentState.RoomDustPatch patch = s.RoomDust[i];
                Vector2 c = patch.At(t);
                bool gliding = t >= patch.MovedAt && t < patch.MovedAt + MapEnvironmentRules.RoomDustGlide;
                // the patch: one dusty colour on the floor, no rim
                Color fill = new Color(.84f, .72f, .56f, .3f);
                ink.FloorZone(c, rx, ry, fill, fill); Marks++;
                // a shaft of dusty light sifting from the ceiling onto it
                Color beam = new Color(1f, .9f, .72f, calm ? .4f : .62f);
                if (!layer.Quad(shaft, c + Vector2.up * 2.1f, new Vector2(rx * 2.05f, 4.2f), 0, beam, MapWaitingRoomLayer.ShaftOrder, clock, patch.Seed))
                    for (int k = 0; k < 3; k++) ink.Light(c + Vector2.up * (.4f + k * 1.15f), rx * (1.9f - k * .3f), new Color(1f, .9f, .7f), .13f);
                // dust falling into it, thicker while the ceiling shifts; puffs settling on the floor
                int motes = sparse ? 2 : gliding ? 9 : 5;
                for (int k = 0; k < motes; k++)
                {
                    float cycle = still ? .3f + .4f * (k % 2) : Mathf.Repeat(t * (.32f + .09f * (k % 3)) + k * .37f + patch.Seed * .013f, 1);
                    Vector2 at = c + new Vector2(((k * 37 + patch.Seed) % 11 / 10f - .5f) * rx * 1.4f + (still ? 0 : Mathf.Sin(t * 1.3f + k) * .08f), 2.7f * (1 - cycle) - .1f);
                    ink.TokenAt(InkToken.Sand, ArsenalInk.Dust, at, .08f + .04f * (k % 2), cycle * 220, .8f * Mathf.Sin(cycle * Mathf.PI));
                }
                if (!sparse)
                    for (int k = 0; k < 2; k++)
                    {
                        float cycle = still ? .5f : Mathf.Repeat(t * .45f + k * .5f + patch.Seed * .07f, 1);
                        Vector2 at = c + new Vector2((k == 0 ? -.45f : .5f) * rx * (.6f + .4f * cycle), (k == 0 ? .1f : -.15f) * ry);
                        ink.Token(InkToken.Puff, ArsenalInk.Dust, at, .28f + .22f * cycle, 0, .5f * (1 - cycle), false);
                    }
            }
            RoomDrawFootprints(s, t);
        }
        /// <summary>Enemies that walked through the dust leave dusty prints behind them, each fading in 3 s.</summary>
        private void RoomDrawFootprints(MapEnvironmentState s, float t)
        {
            IReadOnlyList<UnitState> roster = sim.Units;
            for (int r = 0; r < roster.Count; r++)
            {
                UnitState u = roster[r];
                if (!u.Alive || u.Faction != Faction.Enemy || u.Definition.IsBoss) continue;
                bool inDust = s.RoomInDust(u.Position, t);
                if (!roomFeet.TryGetValue(u.Id, out RoomFoot foot)) { if (!inDust) continue; roomFeet[u.Id] = foot = new RoomFoot { Last = u.Position }; }
                if (inDust) foot.DustyUntil = t + 2;
                if (foot.DustyUntil <= t) continue;
                Vector2 step = u.Position - foot.Last;
                if (step.sqrMagnitude < .09f) continue;
                if (step.sqrMagnitude > 4) { foot.Last = u.Position; continue; } // a fling or a new wave
                Vector2 dir = step.normalized; foot.Left = !foot.Left; foot.Last = u.Position;
                roomPrints.Add(new RoomPrint { At = u.Position + new Vector2(-dir.y, dir.x) * (foot.Left ? .08f : -.08f), Dir = dir, Born = t });
                if (roomPrints.Count > 160) roomPrints.RemoveAt(0);
            }
            if (roomFeet.Count > 400) roomFeet.Clear();
            for (int i = roomPrints.Count - 1; i >= 0; i--)
            {
                RoomPrint p = roomPrints[i]; float age = t - p.Born;
                if (age < 0 || age > 3) { roomPrints.RemoveAt(i); continue; }
                ink.FloorStroke(p.At - p.Dir * .08f, p.At + p.Dir * .08f, ArsenalInk.Dust, .11f, 1, 0, .6f * (1 - age / 3));
            }
        }

        // ------------------------------------------------------------------ the number board and its doors
        private void RoomDrawBoard(MapEnvironmentState s, float t, MapWaitingRoomLayer layer)
        {
            bool calling = s.CalledDoor >= 0;
            float count = calling ? Mathf.Clamp01((t - s.CallAt) / MapEnvironmentRules.DoorCall) : 0;
            bool counting = calling && !s.RoomDoorBroken && t < s.OpenAt;
            bool lit = calling && t < s.OpenAt + .8f && (calm || still || Mathf.Repeat(t * 3.5f, 1) < .6f);
            Put("WaitingRoom/" + (lit ? "Board_Flash" : "Board_Idle"), s.Board, 2.2f);
            // the number on the board in brass: a ticket with this number gives double
            ink.Light(s.Board + Vector2.up * 2.45f, 1.2f, new Color(1f, .85f, .5f), calling ? .45f : .22f);
            Pips(s.Board + Vector2.up * 2.45f, s.BoardNumber, 1, true);
            for (int d = 0; d < 4; d++)
            {
                bool called = calling && d == s.CalledDoor;
                Vector2 at = s.Doors[d];
                string art = called && (s.RoomDoorBroken || t >= s.OpenAt) ? "Door_Open" : "Door_Closed";
                float shake = 0;
                if (called && counting && !still) shake = Mathf.Sin(t * 38 + d) * (1.2f + 3.8f * count);
                if (called && !still && t - s.RoomDoorProp.HitAt < .16f) shake += Mathf.Sin(t * 95) * 4 * (1 - (t - s.RoomDoorProp.HitAt) / .16f);
                Put("WaitingRoom/" + art, at, 2.3f, flip: at.x > s.Anchor.x, angle: shake);
                Pips(at + Vector2.up * 2.55f, d + 1, called ? 1 : .45f, called);
            }
            if (!calling) return;
            Vector2 door = s.Doors[s.CalledDoor];
            if (s.CallAt != roomSaidCall)
            {
                roomSaidCall = s.CallAt; roomSaidBeat = -1; roomDoorHits = s.RoomDoorProp.Hits;
                ImpactWords.Pop("No. " + s.BoardNumber, s.Board + Vector2.up * 3.1f, new Color(1f, .85f, .45f), 2, 1f, 0);
            }
            if (counting)
            {
                // the countdown: a solid red glow under the shaking door, beating each second; 3-2-1 over it
                int beat = Mathf.Clamp(Mathf.FloorToInt(t - s.CallAt), 0, 2);
                float pulse = calm || still ? 0 : Mathf.Pow(1 - Mathf.Repeat(t - s.CallAt, 1), 3);
                Color red = Fill(RoomRed, count, .9f + .2f * pulse);
                ink.FloorZone(door + Vector2.down * .3f, 1.4f, .8f, red, red); Marks++;
                ink.Light(door + Vector2.up * .3f, 2.6f, RoomRed.Glow, .16f + .22f * pulse);
                if (beat != roomSaidBeat) { roomSaidBeat = beat; ImpactWords.Pop((3 - beat).ToString(), door + Vector2.up * 3.4f, new Color(1f, .42f, .36f), 3, 1.5f - .1f * beat, 0); }
                // its health (about three ordinary enemies) and the time left; cracks spread as it gives
                float flash = Mathf.Clamp01(1 - (t - s.RoomDoorProp.HitAt) / .25f);
                layer.HealthBar(door + Vector2.up * 2.98f, 1.3f, s.RoomDoorProp.Share, flash, new Color(.98f, .7f, .32f), 1);
                layer.TimeLine(door + Vector2.up * 2.8f, 1.3f, 1 - count, RoomRed.Body, .9f);
                int stage = s.RoomDoorProp.Share < .25f ? 3 : s.RoomDoorProp.Share < .5f ? 2 : s.RoomDoorProp.Share < .78f ? 1 : 0;
                layer.PropOver(door);
                layer.Cracks(door + Vector2.up * 1.15f, new Vector2(.5f, .85f), stage, s.CalledDoor * 31 + Mathf.RoundToInt(s.CallAt), 1);
                RoomStrikeInk(s.RoomDoorProp, t, false);
            }
            if (s.RoomDoorProp.Hits > roomDoorHits)
            {
                roomDoorHits = s.RoomDoorProp.Hits;
                if (s.RoomDoorProp.LastDamage > 0) ImpactWords.Pop(Mathf.Max(1, Mathf.RoundToInt(s.RoomDoorProp.LastDamage)).ToString(), s.RoomDoorProp.Position + new Vector2(.35f, .5f), ImpactWords.Cream, 1, .8f);
            }
            if (s.RoomDoorBroken)
            {
                // broken in time: splinters and a brass light through the empty frame; nothing will come out
                float p = (t - s.RoomDoorBrokenAt) / .9f;
                if (p >= 0 && p < 1)
                {
                    ink.Draw(ArsenalInk.Moment.BrickDust, door + Vector2.up * 1f, Vector2.up, p, 1.3f, s.CalledDoor * 40);
                    ink.Draw(ArsenalInk.Moment.Confetti, door + Vector2.up * 1.5f, Vector2.up, p, 1.1f, 7);
                }
                ink.Light(door + Vector2.up * 1.1f, 2.2f, new Color(1f, .82f, .5f), .32f * (1 - Mathf.Clamp01((t - s.RoomDoorBrokenAt) / 2.2f)));
                if (roomSaidBroken != s.RoomDoorBrokenAt) { roomSaidBroken = s.RoomDoorBrokenAt; ImpactWords.Pop("BROKEN!", door + Vector2.up * 3.3f, ImpactWords.Gold, 3, 1.15f); }
            }
            else if (t >= s.OpenAt && t < s.CloseAt)
            {
                // not in time: the pack pours out of a violet dark as the red glow ebbs
                float k = Mathf.Clamp01((t - s.OpenAt) / MapEnvironmentRules.DoorOpen);
                Color red = Fill(RoomRed, 1, .8f * (1 - k));
                ink.FloorZone(door + Vector2.down * .3f, 1.4f, .8f, red, red); Marks++;
                ink.Wash(door + Vector2.down * .2f, 1.7f, ArsenalInk.Violet.Body, .4f * (1 - k));
                if (roomSaidOpen != s.CallAt) { roomSaidOpen = s.CallAt; ImpactWords.Pop("NEXT!", door + Vector2.up * 3.3f, new Color(1f, .42f, .36f), 3, 1.2f); }
            }
        }
        /// <summary>A blow landing on a breakable prop: a quick ink dart for a shot from afar, a slash for a blade, chips flying.</summary>
        private void RoomStrikeInk(MapEnvironmentState.StruckProp prop, float t, bool paperChips)
        {
            float age = t - prop.HitAt; if (age < 0 || age > .45f) return;
            float p = age / .45f;
            Vector2 to = prop.Position, d = to - prop.HitFrom;
            if (d.sqrMagnitude > 3.2f)
            {
                Vector2 dir = d.normalized, head = Vector2.Lerp(to - dir * Mathf.Min(2.2f, d.magnitude), to, still ? 1 : Mathf.Clamp01(age / .1f));
                ink.Stroke(head - dir * .9f, head, ArsenalInk.Cream, .07f, 1, still ? 0 : Mathf.Clamp01((age - .08f) / .2f), 1 - p);
            }
            else
            {
                Vector2 n = d.sqrMagnitude > 1e-4f ? new Vector2(-d.y, d.x).normalized : Vector2.right;
                ink.Stroke(to - n * .55f + Vector2.up * .2f, to + n * .55f - Vector2.up * .2f, ArsenalInk.Cream, .09f, still ? 1 : Mathf.Clamp01(age / .08f), still ? 0 : Mathf.Clamp01((age - .12f) / .25f), 1);
            }
            if (paperChips) ink.Draw(ArsenalInk.Moment.PaperScraps, to, d, p, .55f, prop.Hits * 13);
            else ink.Draw(ArsenalInk.Moment.Crumbs, to, d, p, .7f, prop.Hits * 13);
        }

        // ------------------------------------------------------------------ queue tickets
        private void RoomDrawTickets(MapEnvironmentState s, float t)
        {
            foreach (MapEnvironmentState.RoomTicket ticket in s.RoomTickets)
            {
                float age = t - ticket.Born, left = ticket.Until - t;
                float pop = still ? 1 : RoomOutBack(age / .35f);
                float blink = left < 2 ? (calm || still ? .7f : Mathf.Repeat(t * 5, 1) < .5f ? .4f : 1) : 1;
                bool match = ticket.Number == s.BoardNumber;
                InkPalette pal = RoomTicketInk[Mathf.Clamp(ticket.Colour, 0, 4)];
                Vector2 at = ticket.Position + Vector2.up * (still ? .06f : .06f + .05f * Mathf.Sin(t * 2.4f + ticket.Number));
                // a glow of its colour on the floor and a soft light over it, so it is found at a glance
                ink.Wash(ticket.Position, .8f, pal.Glow, .34f * blink);
                ink.Light(at + Vector2.up * .45f, 1f, pal.Glow, (match ? .45f : .26f) * blink);
                float sway = still ? -8 : -8 + Mathf.Sin(t * 3.1f + ticket.Colour) * 7;
                if (Put(RoomTicketArt[Mathf.Clamp(ticket.Colour, 0, 4)], at, .52f * pop, 70, blink, angle: sway) == null)
                {
                    // until its art is in: a paper slip of its colour with a torn perforation
                    Vector2 axis = new Vector2(Mathf.Cos(sway * Mathf.Deg2Rad), Mathf.Sin(sway * Mathf.Deg2Rad)) * .26f * pop, across = new Vector2(-axis.y, axis.x) * .45f;
                    ink.Stroke(at - axis, at + axis, pal, .3f * pop, 1, 0, blink);
                    ink.Stroke(at + axis * .55f + across, at + axis * .55f - across, ArsenalInk.Cream, .03f, 1, 0, .8f * blink);
                }
                // its number in pips; a number matching the board shines gold, stars turning round it
                for (int k = 0; k < ticket.Number; k++)
                    ink.TokenAt(InkToken.Coin, match ? ArsenalInk.Gold : ArsenalInk.Leather, at + new Vector2((k - (ticket.Number - 1) * .5f) * .15f, .36f), match ? .17f : .14f, 0, blink);
                if (match) ink.Orbit(at + Vector2.up * .05f, .42f, t * .5f, InkToken.Star, ArsenalInk.Gold, 2, .9f * blink);
            }
            float tp = (t - s.RoomTicketTakenAt) / .7f;
            if (tp >= 0 && tp < 1)
                ink.Draw(s.RoomTicketTakenMatched ? ArsenalInk.Moment.Confetti : ArsenalInk.Moment.PaperScraps, s.RoomTicketTakenWhere + Vector2.up * .3f, Vector2.up, tp, s.RoomTicketTakenMatched ? 1.3f : .9f, s.RoomTicketTakenColour * 50);
        }

        // ------------------------------------------------------------------ the wall clock
        private void RoomDrawClock(MapEnvironmentState s, float t, MapWaitingRoomLayer layer)
        {
            Vector2 at = s.RoomClock, face = at + Vector2.up * RoomClockHeight * RoomClockFace;
            bool glowing = s.RoomClockGlowing(t);
            float pulse = calm || still ? .5f : .5f + .5f * Mathf.Sin(t * 5);
            if (glowing)
            {
                // where to stand: one warm brass oval on the floor, the dial blazing, chimes ringing out either side
                Color zone = Fill(ArsenalInk.Gold, .6f + .4f * pulse, .95f);
                ink.FloorZone(at, MapEnvironmentRules.RoomClockReachX, MapEnvironmentRules.RoomClockReachY, zone, zone); Marks++;
                ink.Light(face, 3.2f, new Color(1f, .84f, .45f), .35f + .25f * pulse);
                for (int k = 0; k < 2; k++)
                {
                    float cycle = still ? .5f : Mathf.Repeat(t * 1.2f + k * .5f, 1), r = .7f + .9f * cycle;
                    ink.FloorArc(face, r, r * .9f, -35, 35, ArsenalInk.Gold, .06f, (1 - cycle) * .9f, true);
                    ink.FloorArc(face, r, r * .9f, 145, 215, ArsenalInk.Gold, .06f, (1 - cycle) * .9f, true);
                }
                // how long it still glows: brass ticks round the dial going out one by one
                float glowLeft = 1 - (t - s.RoomClockGlowAt) / MapEnvironmentRules.RoomClockGlow;
                int ticks = Mathf.CeilToInt(12 * Mathf.Clamp01(glowLeft));
                if (ticks > 0) layer.Front.Ticks(face, RoomClockDial + .2f, ticks, .12f, .05f, 90, new Color(1f, .82f, .4f), 90 - 30 * ticks, 90);
            }
            if (Put("WaitingRoom/Clock_Wall", at, RoomClockHeight) == null)
            {
                // until the clock art is in: a cream dial in a brass rim
                Color cream = new Color(.97f, .92f, .8f), brass = new Color(.78f, .58f, .28f);
                layer.Front.Sector(face, RoomClockDial + .06f, 0, 360, brass, brass);
                layer.Front.Sector(face, RoomClockDial - .02f, 0, 360, cream, Color.Lerp(cream, brass, .25f));
                layer.Front.Ticks(face, RoomClockDial - .04f, 12, .1f, .035f, 90, new Color(.2f, .14f, .1f));
            }
            // the hand going round: when it reaches twelve the clock glows
            float hand = s.RoomClockHand(t), a = (90 - 360 * hand) * Mathf.Deg2Rad;
            var brush = new InkStroke { Width = .08f, Head = 1, Tail = 0, Taper = .6f, Alpha = 1, Ink = new Color(.12f, .08f, .06f), Body = new Color(.8f, .6f, .28f), Light = new Color(1f, .93f, .72f), LightOffset = .2f };
            layer.Front.Stroke(InkCanvas.Line(face - new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .08f, face + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * RoomClockDial * .9f), brush);
            layer.Front.Stroke(InkCanvas.Line(face, face + Vector2.up * RoomClockDial * .55f), brush);
            layer.Front.Sector(face, .05f, 0, 360, new Color(.95f, .78f, .4f), new Color(.6f, .42f, .2f));
            float near = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.8f, 1, hand));
            ink.TokenAt(InkToken.Glint, ArsenalInk.Gold, face + Vector2.up * (RoomClockDial + .12f), .2f + .12f * (glowing ? pulse : near), 0, glowing ? 1 : .35f + .6f * near);
            // struck: the dial flares and its light rays out; time stands still for every enemy
            float sp = (t - s.RoomClockStruckAt) / 1.2f;
            if (sp >= 0 && sp < 1)
            {
                ink.Light(face, 7f * RoomOutCubic(sp / .4f), new Color(1f, .92f, .7f), .6f * (1 - sp));
                for (int k = 0; k < 12; k++)
                {
                    Vector2 d = new Vector2(Mathf.Cos(k * 30 * Mathf.Deg2Rad), Mathf.Sin(k * 30 * Mathf.Deg2Rad));
                    float reach = still ? .5f : RoomOutCubic(sp);
                    ink.Stroke(face + d * (.6f + 1.4f * reach), face + d * (.95f + 2.6f * reach), ArsenalInk.Gold, .07f, 1, 0, 1 - sp);
                }
            }
            if (t < s.RoomClockStunUntil)
            {
                int n = 0;
                IReadOnlyList<UnitState> roster = sim.Units;
                for (int r = 0; r < roster.Count; r++)
                {
                    UnitState u = roster[r];
                    if (n >= 40) break;
                    if (!u.Alive || u.Faction != Faction.Enemy || u.SleepUntil <= t) continue;
                    n++;
                    Vector2 h = Head(u) + Vector2.up * .28f; float fade = Mathf.Clamp01((u.SleepUntil - t) / .4f) * Mathf.Clamp01((t - s.RoomClockStruckAt) / .25f);
                    ink.Stroke(h + new Vector2(-.07f, -.1f), h + new Vector2(-.07f, .1f), ArsenalInk.Cream, .065f, 1, 0, fade);
                    ink.Stroke(h + new Vector2(.07f, -.1f), h + new Vector2(.07f, .1f), ArsenalInk.Cream, .065f, 1, 0, fade);
                }
            }
        }

        // ------------------------------------------------------------------ impatient waiters
        private void RoomDrawWaiters(MapEnvironmentState s, float t)
        {
            int shown = 0;
            foreach (KeyValuePair<int, MapEnvironmentState.RoomWaiter> pair in s.RoomWaiters)
            {
                MapEnvironmentState.RoomWaiter w = pair.Value;
                if (w.AngryUntil <= t || t < w.AngryAt) continue;
                if (shown >= 30) break;
                UnitState u = sim.UnitForPresentation(pair.Key); if (u == null || !u.Alive) continue;
                shown++;
                Vector2 head = Head(u) + Vector2.up * .1f;
                float fade = Mathf.Clamp01((w.AngryUntil - t) / .5f) * Mathf.Clamp01((t - w.AngryAt) / .15f);
                // a throbbing temper vein in wine and steam puffing off its head; a violet hush under it (it is stronger now)
                float throb = still ? 1 : 1 + .18f * Mathf.Sin(t * 14 + pair.Key);
                Vector2 vein = head + new Vector2(.22f, .14f);
                for (int k = 0; k < 4; k++)
                {
                    Vector2 d = new Vector2(Mathf.Cos((45 + 90 * k) * Mathf.Deg2Rad), Mathf.Sin((45 + 90 * k) * Mathf.Deg2Rad));
                    ink.Stroke(vein + d * .04f * throb, vein + d * .15f * throb, ArsenalInk.Wine, .06f, 1, 0, fade, .02f, k);
                }
                for (int k = 0; k < 2; k++)
                {
                    float cycle = still ? .5f : Mathf.Repeat(t * 1.6f + k * .5f + pair.Key * .13f, 1);
                    ink.TokenAt(InkToken.Puff, ArsenalInk.Violet, head + new Vector2((k * 2 - 1) * (.12f + .1f * cycle), .1f + .45f * cycle), .2f + .15f * cycle, 0, fade * (1 - cycle) * .85f);
                }
                ink.Wash(u.Position, .65f, ArsenalInk.Violet.Glow, .26f * fade);
                // and the reward: two coins at its feet (it drops double gold now)
                ink.Token(InkToken.Coin, ArsenalInk.Gold, u.Position + new Vector2(-.26f, .05f), .15f, 0, .75f * fade, false);
                ink.Token(InkToken.Coin, ArsenalInk.Gold, u.Position + new Vector2(-.14f, -.02f), .15f, 0, .75f * fade, false);
                if (!roomAngrySaid.TryGetValue(pair.Key, out float said) || said != w.AngryAt)
                {
                    roomAngrySaid[pair.Key] = w.AngryAt;
                    ImpactWords.Pop("IMPATIENT!", head + Vector2.up * .4f, new Color(.8f, .58f, 1f), 1, .72f);
                }
            }
            if (roomAngrySaid.Count > 300) roomAngrySaid.Clear();
        }
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>Warm sepia, low fog drifting along the floor, dust motes in the lamp light; while the clock holds time still,
        /// the room goes a held-breath gold.</summary>
        private void AirWaitingRoom(MapEnvironmentState s, int motes, float clock)
        {
            float hush = clock < s.RoomClockStunUntil ? Mathf.Clamp01((s.RoomClockStunUntil - clock) / .8f) * Mathf.Clamp01((clock - s.RoomClockStruckAt) / .3f) : 0;
            Grade(Color.Lerp(new Color(.96f, .84f, .66f, .07f), new Color(1f, .88f, .62f, .15f), hush), Color.Lerp(new Color(.2f, .11f, .05f, .32f), new Color(.26f, .15f, .05f, .42f), hush));
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
