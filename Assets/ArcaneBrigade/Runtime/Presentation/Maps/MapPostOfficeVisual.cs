using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Post Office's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere
    // dispatch here). Props are painted art (Resources/Environment/PostOffice); every effect is ink drawn in code: the chute's
    // lane, each letter's stat badge, the paper wind (a shader, with an ink fallback), the red stamps and their bursts, the
    // parcel's landing, health bar, cracks and burst, the planes' lane and trails. Danger to the hero is red, gifts are gold,
    // amber, cream and teal, enemy states violet. Areas are solid fills of one colour with no rim.

    public sealed partial class MapEnvironmentVisual
    {
        private static readonly InkPalette PostRed = new InkPalette(new Color(.24f, .03f, .05f), new Color(.82f, .14f, .16f), new Color(1, .78f, .72f), new Color(1, .36f, .3f), new Color(.98f, .8f, .45f));
        private static readonly InkPalette PostTeal = new InkPalette(new Color(.05f, .17f, .18f), new Color(.42f, .8f, .76f), new Color(.92f, 1, .97f), new Color(.55f, .95f, .88f), new Color(.98f, .92f, .7f));
        // each letter's badge: the stat it gives (damage, attack speed, armour, gold, speed)
        private static readonly InkToken[] PostStatToken = { InkToken.Fang, InkToken.Bolt, InkToken.Seal, InkToken.Coin, InkToken.Leaf };
        private static readonly InkPalette[] PostStatInk = { ArsenalInk.Wine, ArsenalInk.Gold, ArsenalInk.Silver, ArsenalInk.Gold, ArsenalInk.Cyan };
        private static readonly string[] PostLetterArt = { "PostOffice/Letter_A", "PostOffice/Letter_B", "PostOffice/Letter_C" };
        private static readonly string[] PostScrapArt = { "PostOffice/Scrap_A", "PostOffice/Scrap_B", "PostOffice/Scrap_C", "PostOffice/Scrap_D" };

        private int postParcelHits; private float postSaidBurst = -100, postSaidParcel = -100, postSaidOpened = -100, postSaidHatch = -100, postSaidPlanes = -100;
        private readonly Dictionary<int, float> postStampSaid = new Dictionary<int, float>();
        private MapEnvironmentState postSeen;

        private void DrawPostOffice(MapEnvironmentState s, float t)
        {
            if (s != postSeen) { postSeen = s; postParcelHits = 0; postSaidBurst = postSaidParcel = postSaidOpened = postSaidHatch = postSaidPlanes = -100; postStampSaid.Clear(); } // a new night
            MapWaitingRoomLayer layer = RoomPaperLayer(); layer.Begin();
            bool sparse = PresentationSettings.ReducedEffects;
            PostDrawWind(s, t, layer, sparse);
            PostDrawPlanes(s, t, sparse);
            PostDrawChute(s, t);
            PostDrawStamps(s, t, layer);
            PostDrawParcel(s, t, layer);
            layer.End();
        }
        /// <summary>How much the map's own light lifts a standing prop at this spot (0 none, 1 fully lit).</summary>
        private float PostOfficePropLight(MapEnvironmentState s, Vector2 at) => 0;
        private static float PostSmooth(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));

        // ------------------------------------------------------------------ chute letters
        private void PostDrawChute(MapEnvironmentState s, float t)
        {
            float chuteEnd = s.ChuteAt + MapEnvironmentRules.ChuteWarn + 5 * .45f + 3.6f;
            if (t >= s.ChuteAt && t < chuteEnd)
            {
                float fade = Mathf.Clamp01((t - s.ChuteAt) / .3f) * Mathf.Clamp01((chuteEnd - t) / .4f);
                Put("PostOffice/Chute", new Vector2(s.ChuteX, s.ChuteTop + .35f), 1.25f, Air, fade);
                // the letters' lane: one band of warm gold down the floor while the chute opens, filling as it charges (no rim)
                float charge = Mathf.Clamp01((t - s.ChuteAt) / MapEnvironmentRules.ChuteWarn);
                if (t < s.ChuteAt + MapEnvironmentRules.ChuteWarn + 2.5f)
                    Band(new Vector2(s.ChuteX, s.ChuteTop - .3f), new Vector2(s.ChuteX, s.ChuteBottom), .6f, charge, ArsenalInk.Gold, .8f * fade);
            }
            for (int i = 0; i < s.Letters.Count; i++)
            {
                MapEnvironmentState.Letter letter = s.Letters[i];
                int stat = Mathf.Clamp(letter.Stat, 0, 4);
                string art = letter.Gold ? "PostOffice/Letter_Gold" : PostLetterArt[Mathf.Abs(letter.Style) % 3];
                Vector2 at = letter.Position;
                float alpha = 1;
                if (letter.Grounded)
                {
                    // a stamp's letter lies on the floor for a while: it bobs on a glow of its stat's colour, blinking at the end
                    float left = letter.Until - t, age = t - letter.Born;
                    alpha = left < 2 ? (calm || still ? .7f : Mathf.Repeat(t * 5, 1) < .5f ? .4f : 1) : 1;
                    at += Vector2.up * (still ? .1f : .1f + .06f * Mathf.Sin(t * 2.6f + i)) + Vector2.up * (still ? 0 : .5f * (1 - RoomOutCubic(age / .4f)));
                    ink.Wash(letter.Position, .7f, PostStatInk[stat].Glow, .32f * alpha);
                    Put(art, at, .56f, 70, alpha, angle: still ? 0 : Mathf.Sin(t * 2 + i) * 8);
                }
                else
                {
                    if (letter.Gold) ink.Light(at, 1.1f, new Color(1f, .85f, .45f), .4f);
                    Put(art, at, letter.Gold ? .66f : .56f, Air, 1, angle: still ? 0 : Mathf.Sin(t * 4 + i) * 16);
                }
                // its badge: the stat it gives
                ink.TokenAt(PostStatToken[stat], PostStatInk[stat], at + new Vector2(.2f, .16f), .22f, 0, alpha);
            }
            // a full delivery: the three boosts bound together, confetti round the hero
            float dp = (t - s.PostDeliveredAt) / 1.1f;
            if (dp >= 0 && dp < 1 && sim.Champion != null) ink.Draw(ArsenalInk.Moment.Confetti, Head(sim.Champion) + Vector2.up * .3f, Vector2.up, dp, 1.5f, 11);
        }

        // ------------------------------------------------------------------ paper wind
        private void PostDrawWind(MapEnvironmentState s, float t, MapWaitingRoomLayer layer, bool sparse)
        {
            if (t < s.GustWarnAt || t >= s.GustUntil + .6f) return;
            Camera cam = Camera.main; if (cam == null || !cam.orthographic) return;
            Vector2 centre = cam.transform.position; float halfH = cam.orthographicSize, halfW = halfH * cam.aspect;
            Vector2 wind = s.PostWind, across = new Vector2(-wind.y, wind.x);
            float rise = PostSmooth(s.GustWarnAt, s.GustAt, t), fall = 1 - PostSmooth(s.GustUntil, s.GustUntil + .6f, t);
            float blow = t < s.GustAt ? rise * .35f : fall; // the warning is a stirring, the gust the full draught
            float clock = still ? 0 : t;
            // the draught itself: long streaks racing across the whole room
            Material sheet = sparse ? null : MapWaitingRoomLayer.Shader("MapPostOfficeWind");
            bool shaded = layer.Quad(sheet, centre, new Vector2(halfW * 2.3f, halfH * 2.3f), 0, new Color(1f, .97f, .9f, calm ? .45f : .7f), MapWaitingRoomLayer.WindOrder, clock, 3, new Vector4(wind.x, wind.y, blow, 0));
            int streaks = shaded ? 4 : sparse ? 6 : 12;
            for (int k = 0; k < streaks; k++)
            {
                float lane = (k + .5f) / streaks * 2 - 1;
                float run = Mathf.Repeat(k * .37f + clock * (.55f + .2f * (k % 3)), 1);
                Vector2 at = centre + across * lane * halfH * .9f + wind * ((run * 2 - 1) * halfW * 1.2f);
                ink.Stroke(at - wind * (1.4f + .5f * (k % 2)), at, ArsenalInk.Cream, .05f, 1, 0, .55f * blow * Mathf.Sin(run * Mathf.PI), .06f, k);
            }
            // the warning: big cream chevrons drifting the wind's way
            if (t < s.GustAt)
                for (int k = 0; k < 3; k++)
                {
                    Vector2 tip = centre + wind * ((k - 1) * 1.2f + (still ? 0 : (t - s.GustWarnAt) * 1.5f)) + across * halfH * .15f;
                    float a = rise * .7f;
                    ink.Stroke(tip - wind * .55f + across * .5f, tip, ArsenalInk.Cream, .1f, 1, 0, a);
                    ink.Stroke(tip - wind * .55f - across * .5f, tip, ArsenalInk.Cream, .1f, 1, 0, a);
                }
            // paper scraps riding the gust
            int scraps = sparse ? 3 : 7;
            for (int k = 0; k < scraps; k++)
            {
                float run = Mathf.Repeat(k * .29f + clock * (.35f + .12f * (k % 3)), 1);
                Vector2 at = centre + wind * ((run * 2 - 1) * halfW * 1.15f) + across * (((k * 37) % 11 / 10f - .5f) * halfH * 1.6f + (still ? 0 : Mathf.Sin(t * 2.2f + k) * .4f));
                float a = blow * Mathf.Clamp01(Mathf.Sin(run * Mathf.PI) * 3);
                if (Put(PostScrapArt[k % 4], at, k % 4 == 3 ? .22f : .34f, Air, a, angle: still ? 15 : t * 240 * Mathf.Sign(wind.x + 1e-4f) + k * 50) != null && k % 2 == 0) ink.Streaks(at, wind, .6f * a);
            }
            // enemies walking into it lean on it: the draught breaks on their fronts
            if (t >= s.GustAt && t < s.GustUntil)
            {
                int shown = 0;
                IReadOnlyList<UnitState> roster = sim.Units;
                for (int r = 0; r < roster.Count; r++)
                {
                    UnitState u = roster[r];
                    if (shown >= 24) break;
                    if (!u.Alive || u.Faction != Faction.Enemy || u.Definition.IsBoss) continue;
                    Vector2 step = u.Position - u.PreviousPosition;
                    if (step.sqrMagnitude < 1e-8f || Vector2.Dot(step.normalized, wind) >= -.3f) continue;
                    shown++;
                    ink.Streaks(u.Position + Vector2.up * u.Definition.BodyHeight * .45f, wind, .55f * fall);
                }
            }
        }

        // ------------------------------------------------------------------ stamped enemies
        private void PostDrawStamps(MapEnvironmentState s, float t, MapWaitingRoomLayer layer)
        {
            int stamps = 0;
            foreach (KeyValuePair<int, float> stamp in s.Stamped)
            {
                if (stamp.Value <= t || stamps >= 30) continue;
                UnitState u = sim.UnitForPresentation(stamp.Key); if (u == null || !u.Alive) continue;
                stamps++;
                float since = s.PostStampedAt.TryGetValue(stamp.Key, out float at0) ? t - at0 : 1;
                float left = Mathf.Clamp01((stamp.Value - t) / MapEnvironmentRules.StampSeconds);
                float fade = Mathf.Clamp01((stamp.Value - t) / .5f);
                // a red stamp slammed onto it (red: it is stronger and quicker now), burning down tick by tick
                float slam = still ? 1 : RoomOutCubic(since / .22f);
                Vector2 head = Head(u) + Vector2.up * .2f;
                if (Put("PostOffice/Stamp", head, .38f * (1 + .9f * (1 - slam)), Air - 10, .95f * fade * Mathf.Clamp01(since / .08f), angle: -12) == null)
                    ink.TokenAt(InkToken.Seal, PostRed, head, .34f * (1 + .9f * (1 - slam)), -12, fade);
                int ticks = Mathf.CeilToInt(8 * left);
                if (ticks > 0) layer.Front.Ticks(head, .34f, ticks, .09f, .04f, 90, PostRed.Body, 90 - 45 * ticks, 90);
                Color red = Fill(PostRed, .5f, .75f * fade);
                ink.FloorZone(u.Position, .55f + u.Definition.Radius * .5f, .33f + u.Definition.Radius * .3f, red, red); Marks++;
                if (since < .3f && !still) ink.Draw(ArsenalInk.Moment.Crumbs, head, Vector2.down, since / .3f, .6f, stamp.Key);
                if (!postStampSaid.TryGetValue(stamp.Key, out float said) || said != at0)
                {
                    postStampSaid[stamp.Key] = at0;
                    ImpactWords.Pop("STAMPED", head + Vector2.up * .35f, new Color(1f, .4f, .36f), 1, .75f);
                }
            }
            if (postStampSaid.Count > 200) postStampSaid.Clear();
            // a stamp burning out on a kill: a splash of red ink, scraps of paper and a letter left behind
            foreach (MapEnvironmentState.PostBurst burst in s.PostBursts)
            {
                float p = (t - burst.At) / 1.2f;
                if (p < 0 || p >= 1) continue;
                if (p < .3f) { Color flash = Fill(PostRed, 1, .9f * (1 - p / .3f)); ink.FloorZone(burst.Position, burst.Radius, burst.Radius * .66f, flash, flash); Marks++; }
                ink.Pool(burst.Position, burst.Radius * .55f, 1 - PostSmooth(.5f, 1, p), PostRed, burst.At * 37 % 360, .85f);
                ink.Draw(ArsenalInk.Moment.PaperScraps, burst.Position + Vector2.up * .4f, Vector2.up, p, 1.2f, burst.At * 13 % 360);
                if (postSaidBurst < burst.At) { postSaidBurst = burst.At; ImpactWords.Pop("BURST!", burst.Position + Vector2.up * 1.1f, new Color(1f, .5f, .4f), 2, .95f); }
            }
        }

        // ------------------------------------------------------------------ the giant parcel
        private void PostDrawParcel(MapEnvironmentState s, float t, MapWaitingRoomLayer layer)
        {
            switch (s.ParcelState)
            {
                case 1:
                {
                    // warned 1.3 s before: an amber spot fills where it will land and its shadow grows
                    float p = Mathf.Clamp01((t - s.ParcelAt) / MapEnvironmentRules.ParcelWarn);
                    Ring(s.Parcel, 1.15f, .72f, p, ArsenalInk.Amber, .95f);
                    ink.Wash(s.Parcel, .5f + .6f * p, ArsenalInk.Midnight.Ink, .35f * p);
                    Put("PostOffice/Parcel_Intact", s.Parcel + Vector2.up * (1 - p * p) * 6, 1.5f, alpha: Mathf.Clamp01(p * 3));
                    if (postSaidParcel != s.ParcelAt) { postSaidParcel = s.ParcelAt; ImpactWords.Pop("PARCEL!", s.Parcel + Vector2.up * 2.4f, new Color(1f, .8f, .45f), 2, 1f); postParcelHits = 0; }
                    break;
                }
                case 2:
                {
                    MapEnvironmentState.StruckProp prop = s.ParcelProp;
                    float left = s.HatchAt - t, landed = t - s.ParcelAt;
                    bool stirring = left < 3;
                    float shake = 0;
                    if (!still && stirring) shake = Mathf.Sin(t * 26) * (2 + 3 * (1 - left / 3));
                    if (!still && t - prop.HitAt < .16f) shake += Mathf.Sin(t * 95) * 4 * (1 - (t - prop.HitAt) / .16f);
                    float squash = still ? 0 : Mathf.Max(0, 1 - landed / .25f);
                    // the landing thump: dust rolling out from under it
                    if (landed < .6f) ink.Draw(ArsenalInk.Moment.BrickDust, s.Parcel, Vector2.up, landed / .6f, 1.4f, 5);
                    Put(prop.Share < .6f || stirring ? "PostOffice/Parcel_Cracked" : "PostOffice/Parcel_Intact", s.Parcel, 1.5f * (1 - .12f * squash), angle: shake);
                    if (stirring) ink.Light(prop.Position, 1.8f, ArsenalInk.Violet.Glow, .25f + (calm || still ? 0 : .15f * Mathf.Sin(t * 9)));
                    // breakable from any range: its health, and a violet line for the time before something climbs out
                    float flash = Mathf.Clamp01(1 - (t - prop.HitAt) / .25f);
                    layer.HealthBar(s.Parcel + Vector2.up * 1.9f, 1.4f, prop.Share, flash, new Color(1f, .78f, .36f), 1);
                    layer.TimeLine(s.Parcel + Vector2.up * 1.72f, 1.4f, Mathf.Clamp01(left / MapEnvironmentRules.ParcelHatch), ArsenalInk.Violet.Glow, .9f);
                    int stage = prop.Share < .25f ? 3 : prop.Share < .5f ? 2 : prop.Share < .8f ? 1 : 0;
                    layer.PropOver(s.Parcel);
                    layer.Cracks(prop.Position, new Vector2(.6f, .5f), stage, 77, 1);
                    RoomStrikeInk(prop, t, true);
                    if (prop.Hits > postParcelHits)
                    {
                        postParcelHits = prop.Hits;
                        if (prop.LastDamage > 0) ImpactWords.Pop(Mathf.Max(1, Mathf.RoundToInt(prop.LastDamage)).ToString(), prop.Position + new Vector2(.4f, .5f), ImpactWords.Cream, 1, .8f);
                    }
                    Marks++;
                    break;
                }
                case 3:
                {
                    // broken open: it bursts across 2.8 units (a brief amber flash of the whole reach), paper and gold flying
                    float p = (t - s.ParcelAt) / 1.4f;
                    if (p >= 0 && p < 1)
                    {
                        if (p < .25f) { Color flash = Fill(ArsenalInk.Amber, 1, 1 - p / .25f); ink.FloorZone(s.Parcel, MapEnvironmentRules.ParcelBurstRadius, MapEnvironmentRules.ParcelBurstRadius * .66f, flash, flash); Marks++; }
                        Put("PostOffice/Parcel_Burst", s.Parcel, 1.7f, alpha: 1 - PostSmooth(.5f, 1, p));
                        ink.Light(s.Parcel + Vector2.up * .6f, 4.5f * RoomOutCubic(p / .3f), new Color(1f, .85f, .5f), .55f * (1 - p));
                        ink.Draw(ArsenalInk.Moment.PaperScraps, s.Parcel + Vector2.up * .6f, Vector2.up, p, 2.2f, 21);
                        ink.Draw(ArsenalInk.Moment.Confetti, s.Parcel + Vector2.up * .8f, Vector2.up, p, 1.6f, 4);
                    }
                    if (postSaidOpened != s.ParcelAt && p >= 0) { postSaidOpened = s.ParcelAt; ImpactWords.Pop("SPECIAL DELIVERY!", s.Parcel + Vector2.up * 2.3f, ImpactWords.Gold, 3, 1.1f); }
                    break;
                }
                case 4:
                {
                    // left too long: it splits and an elite climbs out of a violet dark
                    float p = (t - s.ParcelAt) / 1.4f;
                    if (p >= 0 && p < 1)
                    {
                        Put("PostOffice/Parcel_Burst", s.Parcel, 1.7f, alpha: 1 - PostSmooth(.5f, 1, p), tint: new Color(.75f, .55f, .8f));
                        ink.Wash(s.Parcel, 1.8f, ArsenalInk.Violet.Body, .45f * (1 - p));
                        ink.Pool(s.Parcel, .9f, 1 - PostSmooth(.4f, 1, p), ArsenalInk.Violet, 40, .8f);
                    }
                    if (postSaidHatch != s.ParcelAt) { postSaidHatch = s.ParcelAt; ImpactWords.Pop("RETURN TO SENDER", s.Parcel + Vector2.up * 2.3f, new Color(.8f, .58f, 1f), 2, .95f); }
                    break;
                }
            }
        }

        // ------------------------------------------------------------------ paper planes
        private void PostDrawPlanes(MapEnvironmentState s, float t, bool sparse)
        {
            if (s.PostPlaneCount <= 0 || t < s.PostPlanesWarnAt || t > s.PostPlanesEnd + .5f) return;
            Vector2 from = s.PostLaneFrom, to = s.PostLaneTo, dir = s.PostLaneDirection, across = new Vector2(-dir.y, dir.x);
            float length = s.PostLaneLength;
            // the lane: one solid teal band 1.5 units wide, filling through the warning, fading once the last plane is through
            float charge = Mathf.Clamp01((t - s.PostPlanesWarnAt) / MapEnvironmentRules.PostPlaneWarn);
            float fade = 1 - PostSmooth(s.PostPlanesEnd - .2f, s.PostPlanesEnd + .5f, t);
            Band(from, to, MapEnvironmentRules.PostPlaneHalf, charge, PostTeal, .9f * fade);
            // chevrons running along it the planes' way
            float clock = still ? 0 : t;
            for (int k = 0; k < 9; k++)
            {
                float f = Mathf.Repeat(k / 9f + clock * .12f, 1); Vector2 tip = Vector2.Lerp(from, to, f);
                float a = .7f * fade * Mathf.Sin(f * Mathf.PI) * (t < s.PostPlanesAt ? charge : 1);
                ink.FloorStroke(tip - dir * .3f + across * .32f, tip, PostTeal, .07f, 1, 0, a);
                ink.FloorStroke(tip - dir * .3f - across * .32f, tip, PostTeal, .07f, 1, 0, a);
            }
            if (postSaidPlanes != s.PostPlanesWarnAt) { postSaidPlanes = s.PostPlanesWarnAt; ImpactWords.Pop("AIR MAIL!", Vector2.Lerp(from, to, .5f) + Vector2.up * 1.2f, new Color(.7f, 1f, .92f), 2, .95f); }
            // the planes: each with its own place in the lane and a looping ink trail
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            bool left = dir.x < 0;
            for (int i = 0; i < s.PostPlaneCount; i++)
            {
                float along = s.PostPlaneAlong(i, t);
                if (along < 0 || along > length) continue;
                float bob = still ? 0 : Mathf.Sin(t * 7 + i * 1.3f) * .1f;
                Vector2 at = from + dir * along + across * (MapEnvironmentState.PostPlaneSide(i) + bob) + Vector2.up * .5f;
                float edge = Mathf.Clamp01(along / 1.2f) * Mathf.Clamp01((length - along) / 1.2f);
                if (!sparse)
                {
                    List<Vector2> trail = InkCanvas.Line(at - dir * 1.5f, at - dir * .25f, .07f, 3.5f, false, i * 1.7f + (still ? 0 : t * 4));
                    ink.Ribbon(trail, ArsenalInk.Cream, .045f, 1, .1f, .6f * edge);
                }
                ink.Wash(at + Vector2.down * .5f, .3f, ArsenalInk.Midnight.Ink, .25f * edge); // its little shadow on the floor
                if (Put("PostOffice/Paper_Plane", at, .5f, Air + 5, edge, flip: left, angle: left ? angle - 180 : angle) == null)
                {
                    // until its art is in: a folded cream dart in ink
                    Vector2 nose = at + dir * .3f, tail = at - dir * .25f;
                    ink.Stroke(tail + across * .2f, nose, ArsenalInk.Cream, .09f, 1, 0, edge);
                    ink.Stroke(tail - across * .2f, nose, ArsenalInk.Cream, .09f, 1, 0, edge);
                    ink.Stroke(tail, nose, ArsenalInk.Silver, .05f, 1, 0, edge);
                }
            }
            // where a plane strikes: a puff of paper
            foreach (MapEnvironmentState.PostPlaneStrike hit in s.PostPlaneHits)
            {
                float p = (t - hit.At) / .4f;
                if (p >= 0 && p < 1) ink.Draw(ArsenalInk.Moment.PaperScraps, hit.Position + Vector2.up * .4f, dir, p, .45f, hit.At * 31 % 360);
            }
        }
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>Cool blue, window light shafts, paper scraps and ash-like flecks on a draught; while the paper wind blows,
        /// extra flecks race the wind's way.</summary>
        private void AirPostOffice(MapEnvironmentState s, int motes, float clock)
        {
            Grade(new Color(.68f, .8f, 1f, .08f), new Color(.03f, .06f, .16f, .34f));
            // cool window light falling in long soft shafts from the upper left
            for (int i = 0; i < 3; i++)
            {
                float breathe = still || calm ? 1 : .8f + .2f * Mathf.Sin(t * .4f + i * 1.7f);
                Vector2 at = centre + new Vector2((-.55f + i * .5f) * halfW, .1f * halfH);
                Put(MechanicInk.Glow, at, new Vector2(1.5f + .4f * i, halfH * 3f), -28, new Color(.74f, .88f, 1f, .12f * breathe), HazeOrder);
            }
            // paper scraps fluttering down and ash-like flecks on the draught
            for (int i = 0; i < motes; i++)
            {
                bool scrap = i % 3 == 0;
                Vector2 at = Wrap(new Vector2(H(i, 1) * 40, H(i, 2) * 40), scrap ? new Vector2(.25f, -.35f - .15f * H(i, 3)) : new Vector2(.45f, -.22f - .1f * H(i, 3)));
                at.x += still ? 0 : Mathf.Sin(t * 1.6f + i) * (scrap ? .25f : .08f);
                if (scrap) Put(flat, at, new Vector2(.15f, .1f * (still ? 1 : .4f + .6f * Mathf.Abs(Mathf.Cos(t * 2.2f + i)))), still ? 15 : t * 60 + i * 40, new Color(.96f, .94f, .87f, .6f), AirOrder);
                else Put(MechanicInk.Glow, at, Vector2.one * (.06f + .04f * H(i, 4)), 0, new Color(.72f, .74f, .8f, .38f), AirOrder);
            }
            // the paper wind: flecks streaming the wind's way, fading in and out with the gust
            if (s.Theme != MapTheme.PostOffice || clock < s.GustAt - .3f || clock >= s.GustUntil + .5f) return;
            float gust = Mathf.Clamp01((clock - s.GustAt + .3f) / .5f) * Mathf.Clamp01((s.GustUntil + .5f - clock) / .6f);
            Vector2 wind = s.PostWind;
            for (int i = 0; i < motes; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 11) * 40, H(i, 12) * 40), wind * (5 + 3 * H(i, 13)));
                Put(flat, at, new Vector2(.32f + .2f * H(i, 14), .035f), Mathf.Atan2(wind.y, wind.x) * Mathf.Rad2Deg, new Color(1f, .97f, .9f, .5f * gust), AirOrder);
            }
        }
    }
}
