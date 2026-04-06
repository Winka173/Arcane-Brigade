using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws the map's living elements (play-test item 24, D11 v2) from MapEnvironmentState: the owner's painted props
    /// (Resources/Environment, cut from Art/Playtest/Environment) and their warnings in brushed ink. Warnings are outlines,
    /// arrows and dashed edges (amber for danger, gold for gifts); the lamp light is light on air, never a pool on the floor.</summary>
    public sealed class MapEnvironmentVisual : MonoBehaviour
    {
        private ArsenalInk ink;
        private bool drawn;
        private readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        private int used;
        private CombatSimulation sim;
        private bool still, calm;
        // what the last frame showed (read by checks and captures)
        public int Props { get; private set; }
        public int Marks { get; private set; }
        private const int Air = 2150, Floor = -40;

        public void Display(CombatSimulation simulation)
        {
            Props = Marks = 0; used = 0; sim = simulation;
            MapEnvironmentState s = simulation?.MapEnvironment;
            bool live = s != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (!live)
            {
                for (int i = 0; i < pool.Count; i++) pool[i].enabled = false;
                if (drawn) { ink.Begin(); ink.End(); drawn = false; }
                return;
            }
            if (ink == null) ink = new ArsenalInk(transform);
            still = PresentationSettings.ReducedMotion; calm = PresentationSettings.ReducedFlashes;
            ink.Begin(); drawn = true;
            switch (s.Theme)
            {
                case MapTheme.WaitingRoom: WaitingRoom(s, simulation.Elapsed); break;
                case MapTheme.PostOffice: PostOffice(s, simulation.Elapsed); break;
                case MapTheme.LampHall: LampHall(s, simulation.Elapsed); break;
                default: ArchiveVault(s, simulation.Elapsed); break;
            }
            ink.End();
            for (int i = used; i < pool.Count; i++) pool[i].enabled = false;
        }

        // ------------------------------------------------------------------ Waiting Room
        private void WaitingRoom(MapEnvironmentState s, float t)
        {
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

        // ------------------------------------------------------------------ Post Office
        private void PostOffice(MapEnvironmentState s, float t)
        {
            float chuteEnd = s.ChuteAt + MapEnvironmentRules.ChuteWarn + 5 * .45f + 3.6f;
            if (t >= s.ChuteAt && t < chuteEnd)
            {
                float fade = Mathf.Clamp01((t - s.ChuteAt) / .3f) * Mathf.Clamp01((chuteEnd - t) / .4f);
                Put("PostOffice/Chute", new Vector2(s.ChuteX, s.ChuteTop + .35f), 1.25f, Air, fade);
                // the letters' lane: two gold dashed edges down the floor while the chute opens
                float charge = Mathf.Clamp01((t - s.ChuteAt) / MapEnvironmentRules.ChuteWarn);
                if (t < s.ChuteAt + MapEnvironmentRules.ChuteWarn + 2.5f)
                    for (int side = -1; side <= 1; side += 2)
                        Dash(new Vector2(s.ChuteX + side * .6f, s.ChuteTop - .3f), new Vector2(s.ChuteX + side * .6f, s.ChuteBottom), charge, ArsenalInk.Gold, .8f * fade);
            }
            string[] styles = { "PostOffice/Letter_A", "PostOffice/Letter_B", "PostOffice/Letter_C" };
            for (int i = 0; i < s.Letters.Count; i++)
            {
                MapEnvironmentState.Letter letter = s.Letters[i];
                Put(letter.Gold ? "PostOffice/Letter_Gold" : styles[letter.Style % 3], letter.Position, letter.Gold ? .66f : .56f, Air, 1, angle: still ? 0 : Mathf.Sin(t * 4 + i) * 16);
            }
            int stamps = 0;
            foreach (KeyValuePair<int, float> stamp in s.Stamped)
            {
                if (stamp.Value <= t || stamps >= 30) continue;
                UnitState u = sim.UnitForPresentation(stamp.Key); if (u == null || !u.Alive) continue;
                Put("PostOffice/Stamp", Head(u) + Vector2.up * .1f, .36f, Air - 10, .9f * Mathf.Clamp01((stamp.Value - t) / .5f), angle: -12);
                stamps++;
            }
            if (s.BoostUntil > t && sim.Champion != null)
            {
                Put("PostOffice/Letter_Gold", Head(sim.Champion) + Vector2.up * .45f, .42f, Air + 20, Mathf.Clamp01((s.BoostUntil - t) / .6f), angle: still ? 0 : Mathf.Sin(t * 3) * 8);
                InkToken token = s.BoostKind == 0 ? InkToken.Spark : s.BoostKind == 1 ? InkToken.Star : InkToken.Coin;
                ink.Orbit(Head(sim.Champion) + Vector2.up * .45f, .34f, t * .6f, token, ArsenalInk.Gold, 2, .9f);
            }
            if (t >= s.GustWarnAt && t < s.GustUntil)
            {
                float rise = Mathf.Clamp01((t - s.GustWarnAt) / MapEnvironmentRules.GustWarning), fade = rise * Mathf.Clamp01((s.GustUntil - t) / .5f);
                float x0 = (sim.Champion != null ? sim.Champion.Position.x : 0) - 11, dir = s.GustDirection;
                // the band: two broken cream streak lines, drifting with the wind
                for (int side = -1; side <= 1; side += 2)
                    for (int k = 0; k < 9; k++)
                    {
                        float x = x0 + Mathf.Repeat(k * 2.5f + (still ? 0 : t * 3.2f * dir), 22.5f);
                        Vector2 a = new Vector2(x, s.GustY + side * MapEnvironmentRules.GustHalf), b = a + new Vector2(dir * 1.3f, 0);
                        ink.FloorStroke(a, b, ArsenalInk.Cream, .06f, 1, 0, .75f * fade, .03f, k + side);
                    }
                string[] scraps = { "PostOffice/Scrap_A", "PostOffice/Scrap_B", "PostOffice/Scrap_C", "PostOffice/Scrap_D" };
                for (int k = 0; k < 7; k++)
                {
                    float x = x0 + Mathf.Repeat(k * 3.3f + (still ? 0 : t * 4.6f * dir), 22);
                    Vector2 at = new Vector2(x, s.GustY + Mathf.Sin(k * 1.7f + (still ? 0 : t * 2.2f)) * MapEnvironmentRules.GustHalf * .7f);
                    Put(scraps[k % 4], at, k % 4 == 3 ? .22f : .34f, Air, fade, angle: still ? 0 : t * 220 * dir + k * 50);
                    if (k % 2 == 0) ink.Streaks(at, new Vector2(dir, 0), .7f * fade);
                }
            }
            switch (s.ParcelState)
            {
                case 1:
                {
                    float p = Mathf.Clamp01((t - s.ParcelAt) / MapEnvironmentRules.ParcelWarn);
                    Ring(s.Parcel, 1.1f, 1.1f, p, ArsenalInk.Amber, .95f);
                    Put("PostOffice/Parcel_Intact", s.Parcel + Vector2.up * (1 - p * p) * 6, 1.5f, alpha: Mathf.Clamp01(p * 3));
                    break;
                }
                case 2:
                {
                    float left = s.HatchAt - t;
                    bool shaking = left < 3;
                    Put(s.ParcelBreak > .35f || shaking ? "PostOffice/Parcel_Cracked" : "PostOffice/Parcel_Intact", s.Parcel, 1.5f, angle: shaking && !still ? Mathf.Sin(t * 26) * 4 : 0);
                    // stand here: a gold dashed rim at the reach, and the break filling as a solid gold arc
                    Ring(s.Parcel + Vector2.up * .2f, MapEnvironmentRules.ParcelReach, MapEnvironmentRules.ParcelReach * .62f, .3f, ArsenalInk.Gold, .55f);
                    if (s.ParcelBreak > 0) ink.FloorArc(s.Parcel + Vector2.up * .2f, MapEnvironmentRules.ParcelReach, MapEnvironmentRules.ParcelReach * .62f, 90, 90 + 360 * s.ParcelBreak, ArsenalInk.Gold, .13f, 1);
                    Marks++;
                    break;
                }
                case 3: case 4:
                {
                    float fade = Mathf.Clamp01(1 - (t - s.ParcelAt) / 1.6f);
                    if (fade > 0) Put("PostOffice/Parcel_Burst", s.Parcel, 1.7f, alpha: fade, tint: s.ParcelState == 4 ? new Color(.75f, .55f, .6f) : Color.white);
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ Lamp Hall
        private void LampHall(MapEnvironmentState s, float t)
        {
            float rx = MapEnvironmentRules.LightRadiusX, ry = MapEnvironmentRules.LightRadiusY;
            for (int i = 0; i < s.Lamps.Count; i++)
            {
                MapEnvironmentState.Lamp lamp = s.Lamps[i];
                Put(lamp.Lit ? "LampHall/Lamp_Lit" : "LampHall/Lamp_Snuffed", lamp.Base, 2.7f);
                if (!lamp.Lit) continue;
                // the light on air (painted glow), and its edge in a faint warm dashed line so the bonus reads
                Put("LampHall/LightGlow", lamp.Light, 2 * ry * 1.06f, Floor, .62f, tint: new Color(1, .93f, .78f));
                Ring(lamp.Light, rx, ry, 1, ArsenalInk.Gold, .32f);
            }
            if (s.SnuffTarget >= 0 && t >= s.ReachAt && t < s.SnuffAt + .9f)
            {
                Vector2 head = s.Lamps[s.SnuffTarget].Base + Vector2.up * 2.45f;
                float p = Mathf.Clamp01((t - s.ReachAt) / MapEnvironmentRules.SnuffReach);
                if (p < 1) Put("LampHall/Hand_Reach", head + new Vector2(1.45f, .15f) + new Vector2(2.6f, 1.8f) * (1 - Mathf.SmoothStep(0, 1, p)), 1, Air + 40, Mathf.Clamp01(p * 2.5f));
                else Put("LampHall/Hand_Snuff", head + new Vector2(1.1f, -.15f), 1.5f, Air + 40, Mathf.Clamp01((s.SnuffAt + .9f - t) / .4f));
                // the lamp about to go dark: its light trembles under an amber rim
                if (p < 1) Ring(s.Lamps[s.SnuffTarget].Light, rx * .98f, ry * .98f, p, ArsenalInk.Amber, .7f);
            }
            string[] frames = { "LampHall/Moths_0", "LampHall/Moths_1", "LampHall/Moths_2", "LampHall/Moths_3" };
            for (int i = 0; i < s.Moths.Count; i++)
                if (t >= 4) Put(frames[still ? 0 : (Mathf.FloorToInt(t * 8) + i) % 4], s.Moths[i], 1.05f, Air + 30, .95f);
        }

        // ------------------------------------------------------------------ Archive Vault
        private void ArchiveVault(MapEnvironmentState s, float t)
        {
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
            // the ledger plaque and its ink ring (an outline on the floor, slowly turning: kills inside it pay double)
            Put("ArchiveVault/Ledger", s.Ledger + Vector2.up * .1f, 1.7f);
            float r = MapEnvironmentRules.LedgerRadius, spin = still ? 0 : t * 10;
            for (int i = 0; i < 18; i++) ink.FloorArc(s.Ledger, r, r, spin + i * 20, spin + i * 20 + 13, ArsenalInk.Wine, .1f, .8f);
            Marks++;
        }

        // ------------------------------------------------------------------ helpers
        private static Vector2 Head(UnitState u) => u.Position + Vector2.up * (u.Definition.BodyHeight * .62f + .12f);

        private SpriteRenderer Put(string path, Vector2 at, float height, int order = int.MinValue, float alpha = 1, bool flip = false, float angle = 0, Color? tint = null)
        {
            Sprite sprite = MapIdentity.Sprite(path);
            if (sprite == null || alpha <= .01f) return null;
            if (used == pool.Count)
            {
                var go = new GameObject("Map element"); go.transform.SetParent(transform, false);
                pool.Add(go.AddComponent<SpriteRenderer>());
            }
            SpriteRenderer r = pool[used++]; r.enabled = true; r.sprite = sprite; r.flipX = flip;
            Color c = tint ?? Color.white; c.a *= Mathf.Clamp01(alpha); r.color = c;
            float scale = height / Mathf.Max(.01f, sprite.bounds.size.y);
            r.transform.position = at; r.transform.rotation = Quaternion.Euler(0, 0, angle); r.transform.localScale = new Vector3(scale, scale, 1);
            r.sortingOrder = order != int.MinValue ? order : 1000 - Mathf.RoundToInt((at.y + .35f) * 12);
            Props++;
            return r;
        }

        /// <summary>Brass pips (a door's number) in a short row.</summary>
        private void Pips(Vector2 at, int count, float alpha, bool hot)
        {
            for (int i = 0; i < count; i++)
                ink.TokenAt(InkToken.Coin, hot ? ArsenalInk.Gold : ArsenalInk.Leather, at + new Vector2((i - (count - 1) * .5f) * .3f, 0), hot ? .3f : .24f, 0, alpha);
        }
        /// <summary>A dashed brushed rim (outline only) that closes with the charge.</summary>
        private void Ring(Vector2 c, float rx, float ry, float charge, InkPalette pal, float alpha, float from = 0, float to = 360)
        {
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            int dashes = Mathf.Clamp(Mathf.RoundToInt((rx + ry) * 4 * (to - from) / 360), 5, 28);
            float slot = (to - from) / dashes, fill = Mathf.Lerp(.42f, 1.03f, s), spin = still ? 0 : (1 - s) * 30;
            for (int i = 0; i < dashes; i++) { float a = from + spin + i * slot; ink.FloorArc(c, rx, ry, a, a + slot * fill, pal, .09f, alpha); }
            Marks++;
        }
        private void Dash(Vector2 a, Vector2 b, float charge, InkPalette pal, float alpha)
        {
            float length = Vector2.Distance(a, b); if (length < .05f) return;
            float s = Mathf.SmoothStep(0, 1, Mathf.Clamp01(charge));
            int dashes = Mathf.Clamp(Mathf.RoundToInt(length * 1.6f), 3, 18); float fill = Mathf.Lerp(.42f, 1.02f, s);
            for (int i = 0; i < dashes; i++) ink.FloorStroke(Vector2.Lerp(a, b, i / (float)dashes), Vector2.Lerp(a, b, Mathf.Min(1, (i + fill) / dashes)), pal, .08f, 1, 0, alpha, 0, i);
            Marks++;
        }
        /// <summary>Where something will slide: two dashed edges and an arrowhead.</summary>
        private void Lane(Vector2 from, Vector2 to, float half, float charge, InkPalette pal, float alpha)
        {
            Vector2 axis = to - from; if (axis.sqrMagnitude < .01f) return;
            Vector2 dir = axis.normalized, n = new Vector2(-dir.y, dir.x) * half;
            Dash(from + n, to + n, charge, pal, alpha); Dash(from - n, to - n, charge, pal, alpha);
            ink.FloorStroke(to + n * 1.5f - dir * .6f, to + dir * .25f, pal, .1f, 1, 0, alpha);
            ink.FloorStroke(to - n * 1.5f - dir * .6f, to + dir * .25f, pal, .1f, 1, 0, alpha);
        }

        private void OnDestroy() { ink?.Destroy(); }
    }
}
