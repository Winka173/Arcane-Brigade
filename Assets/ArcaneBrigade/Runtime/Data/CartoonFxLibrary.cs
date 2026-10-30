using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Cartoon FX Remaster (Jean Moreno, Unity Asset Store) effects the game plays, by key: element hits,
    /// element bursts and comic words. Built by CartoonFxLibraryBuilder into Resources/CartoonFxLibrary.asset so only the
    /// effects listed here go into a build.</summary>
    public sealed class CartoonFxLibrary : ScriptableObject
    {
        [System.Serializable] public sealed class Entry { public string Key; public GameObject Prefab; }
        public List<Entry> Entries = new List<Entry>();

        public GameObject Get(string key)
        {
            foreach (Entry e in Entries) if (e.Key == key) return e.Prefab;
            return null;
        }

        public static CartoonFxLibrary Load() => Resources.Load<CartoonFxLibrary>("CartoonFxLibrary");
    }
}
