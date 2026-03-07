using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Walk poses whose painting disagrees with the rest of their cycle (a hat star or ribbon on the other side, so the
    /// hero seemed to flip every step), found by Editor/HeroPoseRepairBuilder, each mapped to the cycle's first walk pose.
    /// UnitVisual draws the replacement, so the walk reads as one steady figure.</summary>
    public sealed class HeroPoseRepairs : ScriptableObject
    {
        public Sprite[] From = new Sprite[0];
        public Sprite[] To = new Sprite[0];
        private static Dictionary<Sprite, Sprite> map;
        public static Sprite Repair(Sprite sprite)
        {
            if (map == null)
            {
                map = new Dictionary<Sprite, Sprite>();
                HeroPoseRepairs art = Resources.Load<HeroPoseRepairs>("HeroPoseRepairs");
                if (art != null) for (int i = 0; i < Mathf.Min(art.From.Length, art.To.Length); i++) if (art.From[i] != null && art.To[i] != null) map[art.From[i]] = art.To[i];
            }
            return sprite != null && map.Count > 0 && map.TryGetValue(sprite, out Sprite steady) ? steady : sprite;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() { map = null; }
    }
}
