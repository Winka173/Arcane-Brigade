using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Where each painted hero gauge's window runs (18/10/2026): measured by ArsenalBuilder from where the empty and the
    /// full paintings differ, as fractions of the vessel's painted height from the bottom. The fill line travels this window.</summary>
    public sealed class HeroGaugeArtwork : ScriptableObject
    {
        [Serializable] public struct Window { public string Hero; public float Low, High; }
        public Window[] Windows = Array.Empty<Window>();
        private static HeroGaugeArtwork current;
        public static HeroGaugeArtwork Current => current != null ? current : current = Resources.Load<HeroGaugeArtwork>("HeroGaugeArtwork");
        public static Vector2 For(string hero)
        {
            if (Current != null) foreach (var w in Current.Windows) if (w.Hero == hero) return new Vector2(w.Low, w.High);
            return new Vector2(.14f, .78f);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { current = null; }
    }
}
