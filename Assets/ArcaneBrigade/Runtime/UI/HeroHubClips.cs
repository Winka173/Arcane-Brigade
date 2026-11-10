using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The main-menu room's performance of a redrawn hero (requirement 10/11/2026: the hero there walked on two frames). The
    /// room frames <c>Resources/HeroHub/&lt;Id&gt;/&lt;Id&gt;_h01..h22</c> are: breathing 1-4, walking 5-10, looking round 11-14, a wave
    /// 15-18 and warming up at the flame 19-22. Standing still it breathes; every few seconds it plays one of the three small
    /// moments (never the same twice in a row); walking cancels it.</summary>
    public sealed class HeroHubClips
    {
        public const int Frames = 22;
        private static readonly Dictionary<string, Sprite[]> cache = new Dictionary<string, Sprite[]>();
        private static readonly int[] Breath = { 0, 1, 2, 3, 2, 1 };
        private static readonly int[][] Moments =
        {
            new[] { 10, 10, 11, 11, 12, 12, 13, 13 },                                 // looks round: left, centre, right, centre
            new[] { 14, 15, 16, 17, 16, 17, 16, 15, 14 },                             // raises a hand and waves
            new[] { 18, 19, 20, 21, 20, 19, 18, 19, 20, 21, 20, 19 }                  // warms up at the flame, nodding
        };
        private static readonly float[] MomentRate = { 2.8f, 6f, 5f };

        private int moment = -1, lastMoment = -1;
        private float momentStart, nextMoment = -1;

        /// <summary>The hero's room frames, or null when it has none (it then walks on its own frames).</summary>
        public static Sprite[] For(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (!cache.TryGetValue(id, out Sprite[] frames))
            {
                Sprite[] all = Resources.LoadAll<Sprite>("HeroHub/" + id);
                frames = all != null && all.Length >= Frames ? all.OrderBy(s => s.name, System.StringComparer.Ordinal).Take(Frames).ToArray() : null;
                cache[id] = frames;
            }
            return frames;
        }

        public Sprite Pose(Sprite[] frames, bool walking, float time, bool reduced)
        {
            if (reduced) return frames[0];
            if (walking) { moment = -1; nextMoment = time + Random.Range(5f, 9f); return frames[4 + Mathf.FloorToInt(time * 9f) % 6]; }
            if (nextMoment < 0) nextMoment = time + Random.Range(3f, 6f);
            if (moment < 0 && time >= nextMoment)
            {
                do moment = Random.Range(0, Moments.Length); while (moment == lastMoment && Moments.Length > 1);
                lastMoment = moment; momentStart = time;
            }
            if (moment >= 0)
            {
                int[] clip = Moments[moment]; int step = Mathf.FloorToInt((time - momentStart) * MomentRate[moment]);
                if (step < clip.Length) return frames[clip[step]];
                moment = -1; nextMoment = time + Random.Range(5f, 9f);
            }
            return frames[Breath[Mathf.FloorToInt(time * 3.5f) % Breath.Length]];
        }
    }
}
