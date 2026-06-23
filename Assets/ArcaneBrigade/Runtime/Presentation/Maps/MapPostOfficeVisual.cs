using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Post Office's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere dispatch here).

    public sealed partial class MapEnvironmentVisual
    {
        private int lettersSeen;
        private void DrawPostOffice(MapEnvironmentState s, float t)
        {
            // each letter caught pops over the hero: its count in this drop and the boost it gives; a full delivery says so
            if (s.LettersCaught < lettersSeen) lettersSeen = s.LettersCaught;
            if (s.LettersCaught > lettersSeen && sim.Champion != null)
            {
                lettersSeen = s.LettersCaught;
                Vector2 head = (Vector2)SpeechBubbles.Head(sim.Champion) + Vector2.up * .5f;
                // the stat it gives and its size, with no seconds (requirement 25/10/2026)
                string boost = s.BoostKind == 0 ? "+" + Mathf.RoundToInt((MapEnvironmentRules.BoostSpeed - 1) * 100) + "% SPEED" : s.BoostKind == 1 ? "+" + Mathf.RoundToInt((MapEnvironmentRules.BoostDamage - 1) * 100) + "% DAMAGE" : "+1 GOLD A KILL";
                if (s.LetterStreak == MapEnvironmentRules.FullDelivery) ImpactWords.Pop("FULL DELIVERY! " + boost, head, new Color(1f, .82f, .35f), 5, 1.15f);
                else ImpactWords.Pop("LETTER ×" + s.LetterStreak + " · " + boost, head, new Color(1f, .92f, .7f), 4, .9f);
            }
            float chuteEnd = s.ChuteAt + MapEnvironmentRules.ChuteWarn + 5 * .45f + 3.6f;
            if (t >= s.ChuteAt && t < chuteEnd)
            {
                float fade = Mathf.Clamp01((t - s.ChuteAt) / .3f) * Mathf.Clamp01((chuteEnd - t) / .4f);
                Put("PostOffice/Chute", new Vector2(s.ChuteX, s.ChuteTop + .35f), 1.25f, Air, fade);
                // the letters' lane: a band of warm gold down the floor while the chute opens, filling as it charges (no outline)
                float charge = Mathf.Clamp01((t - s.ChuteAt) / MapEnvironmentRules.ChuteWarn);
                if (t < s.ChuteAt + MapEnvironmentRules.ChuteWarn + 2.5f)
                    Band(new Vector2(s.ChuteX, s.ChuteTop - .3f), new Vector2(s.ChuteX, s.ChuteBottom), .6f, charge, ArsenalInk.Gold, .8f * fade);
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
                    // stand here: one gold area that brightens as the parcel breaks (no rim, no progress ring)
                    Ring(s.Parcel + Vector2.up * .2f, MapEnvironmentRules.ParcelReach, MapEnvironmentRules.ParcelReach * .62f, Mathf.Max(.3f, s.ParcelBreak), ArsenalInk.Gold, .55f);
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
        /// <summary>How much the map's own light lifts a standing prop at this spot (0 none, 1 fully lit).</summary>
        private float PostOfficePropLight(MapEnvironmentState s, Vector2 at) => 0;
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>Cool blue, window light shafts, paper scraps and ash-like flecks on a draught.</summary>
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
        }
    }
}
