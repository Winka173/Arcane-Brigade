using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Plays Cartoon FX Remaster effects on the field: a few pooled copies of each, drawn over the bodies, sized
    /// for the field. The pack's own effect script (camera shake, lights, self-destroy) is stripped: the game shakes the
    /// camera itself (CombatFeel) and keeps the copies for reuse.</summary>
    public sealed class CartoonFx : MonoBehaviour
    {
        private const int Order = 2170, PerKey = 6;
        private sealed class Copy { public GameObject Root; public ParticleSystem[] Systems; public float Born; }
        private readonly Dictionary<string, List<Copy>> pools = new Dictionary<string, List<Copy>>();
        private CartoonFxLibrary library;
        private Transform shelf;

        public int Played { get; private set; }

        private void Awake()
        {
            library = CartoonFxLibrary.Load();
            // copies are made under an inactive shelf so the pack's scripts never wake before they are stripped
            shelf = new GameObject("Cartoon FX shelf").transform; shelf.SetParent(transform, false); shelf.gameObject.SetActive(false);
        }

        public bool Has(string key) => library != null && library.Get(key) != null;

        /// <summary>Play one effect at a point, scaled, turned (degrees) and tinted by its own colours.</summary>
        public void Play(string key, Vector2 at, float scale, float angle = 0)
        {
            if (library == null) return;
            GameObject prefab = library.Get(key);
            if (prefab == null) return;
            if (!pools.TryGetValue(key, out List<Copy> pool)) pools[key] = pool = new List<Copy>();
            Copy copy = null, oldest = null;
            foreach (Copy c in pool)
            {
                if (!c.Root.activeSelf || !Alive(c)) { copy = c; break; }
                if (oldest == null || c.Born < oldest.Born) oldest = c;
            }
            if (copy == null) copy = pool.Count < PerKey ? Make(prefab, pool) : oldest;
            if (copy == null) return;
            Transform t = copy.Root.transform;
            t.SetParent(transform, false);
            t.position = new Vector3(at.x, at.y, 0); t.localScale = Vector3.one * scale; t.rotation = Quaternion.Euler(0, 0, angle);
            copy.Root.SetActive(true);
            foreach (ParticleSystem s in copy.Systems) { s.Clear(false); s.Play(false); }
            copy.Born = Time.time; Played++;
        }

        private static bool Alive(Copy c) { foreach (ParticleSystem s in c.Systems) if (s.IsAlive(false)) return true; return false; }

        private Copy Make(GameObject prefab, List<Copy> pool)
        {
            GameObject root = Instantiate(prefab, shelf);
            foreach (MonoBehaviour script in root.GetComponentsInChildren<MonoBehaviour>(true))
                if (script != null && script.GetType().Name.StartsWith("CFXR_")) DestroyImmediate(script);
            foreach (Light light in root.GetComponentsInChildren<Light>(true)) DestroyImmediate(light);
            // no rings on our side: the pack's shockwave rings and flat ground ripples are cut off
            foreach (Transform part in root.GetComponentsInChildren<Transform>(true))
                if (part != root.transform && part != null && (part.name.Contains("Ring") || part.name.Contains("ipple") || part.name.Contains("collision"))) DestroyImmediate(part.gameObject);
            ParticleSystem[] systems = root.GetComponentsInChildren<ParticleSystem>(true);
            if (systems.Length == 0) { Destroy(root); return null; }
            foreach (ParticleSystem s in systems)
            {
                var main = s.main; main.playOnAwake = false; main.scalingMode = ParticleSystemScalingMode.Hierarchy; main.stopAction = ParticleSystemStopAction.None;
                var r = s.GetComponent<ParticleSystemRenderer>(); if (r != null) r.sortingOrder = Order + r.sortingOrder;
            }
            root.SetActive(false);
            var copy = new Copy { Root = root, Systems = systems };
            pool.Add(copy);
            return copy;
        }
    }
}
