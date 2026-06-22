using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The change between a night and the shop on the same floor (requirement 24/10/2026: the room's pieces swapped in one
    /// frame): every piece of scenery that goes away leaves a copy that fades out, and every piece that comes in fades up, over
    /// .4 s, while the shopkeeper still drops in and the stock still lands. Units, coins and shots are never touched.</summary>
    [DefaultExecutionOrder(1100)] // after the scenery has set its own colours this frame
    public sealed class SceneryCrossfade : MonoBehaviour
    {
        private const float Seconds = .4f, Watch = .2f;
        private sealed class Ghost { public SpriteRenderer Original, Copy; public Vector3 At; public float Alpha, Gone = -1; }
        private sealed class Rise { public float Found, Written = -1, Base = 1; }
        private readonly List<Ghost> ghosts = new List<Ghost>();
        private readonly HashSet<SpriteRenderer> before = new HashSet<SpriteRenderer>();
        private readonly Dictionary<SpriteRenderer, Rise> rising = new Dictionary<SpriteRenderer, Rise>();
        private readonly List<SpriteRenderer> done = new List<SpriteRenderer>();
        private float started = -1;

        /// <summary>Keeps the scenery as it is, runs the change, then crossfades what went and what came.</summary>
        public static void Around(System.Action change, HashSet<Transform> exclude = null)
        {
            SceneryCrossfade fade = Hold(exclude);
            change();
            fade?.Release();
        }
        /// <summary>Keeps the scenery as it is now (null under reduced motion); Release once the change is made, or Cancel.</summary>
        /// <param name="exclude">The moving actors' views (units, shots, coins): never scenery, and on a busy night there are
        /// hundreds of them (requirement 25/10/2026: the change stuttered on crowded nights).</param>
        public static SceneryCrossfade Hold(HashSet<Transform> exclude = null)
        {
            if (PresentationSettings.ReducedMotion) return null;
            var fade = new GameObject("Scenery crossfade").AddComponent<SceneryCrossfade>();
            fade.exclude = exclude;
            fade.Keep();
            return fade;
        }
        public void Release() { started = Time.unscaledTime; }
        public void Cancel() { Destroy(gameObject); }
        private HashSet<Transform> exclude;
        private float nextScan;
        // requirement 25/10/2026: a renderer found to belong to an actor (or to this fade) stays one; the looks for newcomers
        // skip it instead of walking its parents again on every scan
        private readonly HashSet<SpriteRenderer> actorParts = new HashSet<SpriteRenderer>();
        private bool Scenery(SpriteRenderer r)
        {
            if (r == null || !r.enabled || r.sprite == null || r.color.a <= .02f || !r.gameObject.activeInHierarchy) return false;
            if (actorParts.Contains(r)) return false;
            Vector3 size = r.bounds.size; if (size.x < .3f && size.y < .3f) return false; // sparks and chips are effects, not the room
            for (Transform t = r.transform; t != null; t = t.parent)
            {
                if (t == transform || t.name == "DeathRemnant" || exclude != null && exclude.Contains(t)) { actorParts.Add(r); return false; }
                if (t.GetComponent<UnitVisual>() != null) { actorParts.Add(r); return false; } // a pooled view the caller did not list
            }
            return true;
        }
        private void Keep()
        {
            foreach (SpriteRenderer r in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
            {
                if (!Scenery(r)) continue;
                before.Add(r);
                if (ghosts.Count >= 150) continue;
                var copy = new GameObject("ghost").AddComponent<SpriteRenderer>(); copy.transform.SetParent(transform, false);
                copy.sprite = r.sprite; copy.color = r.color; copy.flipX = r.flipX; copy.flipY = r.flipY; copy.sharedMaterial = r.sharedMaterial;
                copy.sortingLayerID = r.sortingLayerID; copy.sortingOrder = r.sortingOrder; copy.drawMode = r.drawMode; if (r.drawMode != SpriteDrawMode.Simple) copy.size = r.size;
                copy.transform.SetPositionAndRotation(r.transform.position, r.transform.rotation); copy.transform.localScale = r.transform.lossyScale;
                copy.enabled = false;
                ghosts.Add(new Ghost { Original = r, Copy = copy, At = r.transform.position, Alpha = r.color.a });
            }
        }
        private void LateUpdate()
        {
            if (started < 0) return;
            float age = Time.unscaledTime - started;
            // what went (or jumped) while the change settles fades out from where it stood; what stayed needs no copy
            for (int i = ghosts.Count - 1; i >= 0; i--)
            {
                Ghost g = ghosts[i];
                if (g.Gone >= 0) continue;
                bool stayed = g.Original != null && g.Original.enabled && g.Original.gameObject.activeInHierarchy && g.Original.sprite == g.Copy.sprite && (g.Original.transform.position - g.At).sqrMagnitude < .0004f;
                if (!stayed) { g.Gone = Time.unscaledTime; g.Copy.enabled = true; }
                else if (age >= Watch) { Destroy(g.Copy.gameObject); ghosts.RemoveAt(i); }
            }
            if (age < Watch && Time.unscaledTime >= nextScan)
            {
                nextScan = Time.unscaledTime + .07f; // three looks for newcomers are enough
                foreach (SpriteRenderer r in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
                    if (!before.Contains(r) && !rising.ContainsKey(r) && Scenery(r)) rising[r] = new Rise { Found = Time.unscaledTime };
            }
            bool fading = false;
            foreach (Ghost g in ghosts)
            {
                if (g.Gone < 0) continue;
                float k = Mathf.SmoothStep(0, 1, Mathf.Clamp01((Time.unscaledTime - g.Gone) / Seconds)); fading |= k < 1;
                Color c = g.Copy.color; c.a = g.Alpha * (1 - k); g.Copy.color = c;
            }
            done.Clear();
            foreach (var pair in rising)
            {
                SpriteRenderer r = pair.Key; Rise f = pair.Value;
                if (r == null) { done.Add(r); continue; }
                Color c = r.color;
                if (f.Written < 0 || Mathf.Abs(c.a - f.Written) > .002f) f.Base = c.a; // its own script set a new colour: that is the base
                float up = Mathf.SmoothStep(0, 1, Mathf.Clamp01((Time.unscaledTime - f.Found) / Seconds));
                if (up >= 1) { c.a = f.Base; r.color = c; done.Add(r); continue; }
                c.a = f.Base * up; r.color = c; f.Written = c.a;
            }
            foreach (SpriteRenderer r in done) rising.Remove(r);
            if (age >= Watch && !fading && rising.Count == 0) Destroy(gameObject);
        }
    }
}
