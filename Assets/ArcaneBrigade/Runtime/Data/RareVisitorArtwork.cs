using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Commissioned sheets: exactly two equal cells, waiting then offering, with matching foot pivots.</summary>
    public static class RareVisitorArtwork
    {
        private static readonly Dictionary<string, Sprite[]> sheets = new Dictionary<string, Sprite[]>();
        public static Sprite Portrait(string id, int pose)
        {
            if (pose < 0 || pose > 1) return null;
            if (!sheets.TryGetValue(id, out Sprite[] poses))
            {
                Sprite[] imported = Resources.LoadAll<Sprite>("RareVisitors/" + id + "_v1");
                poses = new Sprite[2];
                foreach (Sprite sprite in imported)
                    if (sprite.name == id + "_Waiting") poses[0] = sprite;
                    else if (sprite.name == id + "_Offering") poses[1] = sprite;
                if (poses[0] == null || poses[1] == null) return null;
                sheets.Add(id, poses);
            }
            return poses[pose];
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            sheets.Clear();
        }
    }
}
