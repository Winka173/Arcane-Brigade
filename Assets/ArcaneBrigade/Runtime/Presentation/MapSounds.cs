using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Every map's mechanics are heard (requirement 1/11/2026): it watches the map's own clocks in MapEnvironmentState and
    /// plays a map.* cue the moment one of them moves (a door called, a gust, the paper planes, a curtain falling, the vault's
    /// stamp) or a time is passed (the door opens, the gust starts, the ink spills). Presentation only: nothing in the simulation
    /// changes. Warnings are danger cues (heard wherever the hero is), the rest play where they happen.</summary>
    public sealed class MapSounds : MonoBehaviour
    {
        /// <summary>For checks: map cues played since start, and the last one.</summary>
        public static int Played; public static string Last;
        private readonly Dictionary<string, float> seen = new Dictionary<string, float>();
        private float lastT = -1;
        private int lastTheme = -1;

        public void Listen(CombatSimulation simulation)
        {
            MapEnvironmentState s = simulation?.MapEnvironment;
            if (s == null || simulation.Champion == null) { lastT = -1; return; }
            float t = simulation.Elapsed;
            // a new night (the clock ran back) or a new map: forget what was heard, without replaying the map's old stamps
            bool fresh = t < lastT || (int)s.Theme != lastTheme || lastT < 0;
            if (fresh) { seen.Clear(); lastTheme = (int)s.Theme; }
            Vector2 hero = simulation.Champion.Position;
            switch (s.Theme)
            {
                case MapTheme.WaitingRoom:
                    Stamp("call", s.CallAt, t, "map.room.call", s.CalledDoor >= 0 ? s.Doors[s.CalledDoor] : hero, fresh);
                    Cross("open", s.OpenAt, t, "map.room.door", s.CalledDoor >= 0 ? s.Doors[s.CalledDoor] : hero);
                    Stamp("ticket", s.RoomTicketTakenAt, t, "map.room.ticket", s.RoomTicketTakenWhere, fresh);
                    Stamp("glow", s.RoomClockGlowAt, t, "map.room.clockglow", s.RoomClock, fresh);
                    Stamp("strike", s.RoomClockStruckAt, t, "map.room.clockstrike", s.RoomClock, fresh);
                    break;
                case MapTheme.PostOffice:
                    Stamp("chute", s.ChuteAt, t, "map.post.chute", new Vector2(s.ChuteX, s.ChuteTop), fresh);
                    Count("caught", s.LettersCaught, "map.post.catch", hero, fresh);
                    Stamp("delivered", s.PostDeliveredAt, t, "map.post.delivered", hero, fresh);
                    Stamp("gustwarn", s.GustWarnAt, t, "map.post.gustwarn", hero, fresh);
                    Cross("gust", s.GustAt, t, "map.post.gust", hero);
                    Stamp("planes", s.PostPlanesWarnAt, t, "map.post.planes", s.PostLaneFrom, fresh);
                    if (s.PostPlaneHits.Count > 0) { var last = s.PostPlaneHits[s.PostPlaneHits.Count - 1]; Stamp("planehit", last.At, t, "map.post.planehit", last.Position, fresh); }
                    Count("parcel", s.ParcelState, "map.post.parcel", s.Parcel, fresh, 2);
                    break;
                case MapTheme.Glasshouse:
                    Count("weather", s.GlassSunny(t) ? 1 : 2, null, hero, fresh, 0, w => w == 1 ? "map.glass.sun" : "map.glass.rain");
                    for (int i = 0; i < s.GlassPanes.Count; i++)
                    {
                        var pane = s.GlassPanes[i];
                        Stamp("pane" + i, pane.WarnAt, t, "map.glass.crack", pane.Position, fresh);
                        Cross("break" + i, pane.BreakAt, t, "map.glass.shatter", pane.Position);
                    }
                    for (int i = 0; i < s.GlassBuds.Count; i++) Stamp("bud" + i, s.GlassBuds[i].LastShot, t, "map.glass.seed", s.GlassBuds[i].Position, fresh);
                    Caught(s, t, "map.glass.bloom", fresh);
                    break;
                case MapTheme.LampHall:
                    Cross("snuff", s.SnuffAt, t, "map.lamp.snuff", s.SnuffTarget >= 0 && s.SnuffTarget < s.Lamps.Count ? s.Lamps[s.SnuffTarget].Light : hero);
                    for (int i = 0; i < s.Lamps.Count; i++) Stamp("flare" + i, s.Lamps[i].FlareAt, t, "map.lamp.flare", s.Lamps[i].Light, fresh);
                    Stamp("lantern", s.LanternUntil - MapEnvironmentRules.LanternSeconds, t, "map.lamp.lantern", hero, fresh);
                    Caught(s, t, "map.lamp.oil", fresh);
                    break;
                case MapTheme.Theatre:
                    Stamp("colour", s.TheatreColourAt, t, "map.theatre.spot", s.TheatreSpot, fresh);
                    Stamp("encore", s.TheatreEncoreAt, t, "map.theatre.encore", hero, fresh);
                    Stamp("drum", s.TheatreDrumAt, t, "map.theatre.drum", hero, fresh);
                    Stamp("curtainwarn", s.TheatreCurtainWarnAt, t, "map.theatre.curtainwarn", hero, fresh);
                    Cross("curtain", s.TheatreCurtainDropAt, t, "map.theatre.curtain", hero);
                    Stamp("cannon", s.TheatreFiredAt, t, "map.theatre.cannon", s.TheatreMachine, fresh);
                    Caught(s, t, "map.theatre.coin", fresh);
                    break;
                case MapTheme.ArchiveVault:
                    Stamp("page", s.VaultCaughtAt, t, "map.vault.page", s.VaultCaughtWhere, fresh);
                    Stamp("chapter", s.VaultChapterAt, t, "map.vault.chapter", hero, fresh);
                    Stamp("fog", s.VaultFogMovedAt, t, "map.vault.fog", hero, fresh);
                    Stamp("pot", s.VaultPotAt, t, "map.vault.potwarn", s.VaultPot, fresh);
                    Cross("spill", s.VaultSpillAt, t, "map.vault.spill", s.VaultPool);
                    Stamp("stampwarn", s.VaultStampAt, t, "map.vault.stampwarn", s.VaultStamp, fresh);
                    Cross("stamp", s.VaultStampHitAt, t, "map.vault.stamp", s.VaultStamp);
                    Count("spits", s.VaultSpits.Count, "map.vault.spit", hero, fresh);
                    break;
            }
            lastT = t;
        }

        /// <summary>A time the map writes when something happens: heard when it changes to a fresh moment.</summary>
        private void Stamp(string key, float at, float t, string cue, Vector2 where, bool fresh)
        {
            bool known = seen.TryGetValue(key, out float before);
            seen[key] = at;
            if (fresh || !known || Mathf.Approximately(before, at) || at < -50 || t - at > .5f || at - t > .5f) return;
            Play(cue, where);
        }
        /// <summary>A time still to come: heard once when the clock passes it.</summary>
        private void Cross(string key, float at, float t, string cue, Vector2 where)
        {
            if (lastT < 0 || at < -50 || !(lastT < at && t >= at)) return;
            if (seen.TryGetValue(key, out float done) && Mathf.Approximately(done, at)) return;
            seen[key] = at;
            Play(cue, where);
        }
        /// <summary>A count or a state: heard when it rises (or, with <paramref name="only"/>, when it becomes that value).</summary>
        private void Count(string key, int value, string cue, Vector2 where, bool fresh, int only = 0, System.Func<int, string> pick = null)
        {
            bool known = seen.TryGetValue(key, out float before);
            seen[key] = value;
            if (fresh || !known || value == (int)before) return;
            if (only != 0 && value != only) return;
            if (pick == null && value < before) return;
            Play(pick != null ? pick(value) : cue, where);
        }
        private static void Play(string cue, Vector2 where) { Played++; Last = cue; CheckpointAudio.PlayWorldId(cue, where); }
        /// <summary>A map boost picked up (its catch spark): the map's own pickup sound.</summary>
        private void Caught(MapEnvironmentState s, float t, string cue, bool fresh)
        {
            if (s.Catches.Count == 0) return;
            MapCatch last = s.Catches[s.Catches.Count - 1];
            Stamp("catch", last.Time, t, cue, last.At, fresh);
        }
    }
}
