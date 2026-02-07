using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One brush pass layout: a dark ink underlay, the painted body inside it and a thin cream light stroke.</summary>
    public struct InkStroke
    {
        public float Width, Head, Tail, Taper, Seed, Alpha, LightOffset;
        public Color Ink, Body, Light;
        public bool NoLight, InkOnly;
        public static InkStroke Of(InkPalette palette, float width, float alpha, float seed = 0)
            => new InkStroke { Width = width, Head = 1, Tail = 0, Taper = 1, Seed = seed, Alpha = alpha, Ink = palette.Ink, Body = palette.Body, Light = palette.Light, LightOffset = .12f };
    }

    /// <summary>Pooled painted geometry: watercolour washes (sectors, bands) and tapered brush ribbons, batched into two
    /// meshes per layer. Ribbon passes are bucketed so every ink underlay draws beneath every painted body and light.</summary>
    public sealed class InkCanvas
    {
        private readonly Mesh strokeMesh, washMesh;
        private readonly MeshRenderer strokeRenderer, washRenderer;
        private readonly List<Vector3> sv = new List<Vector3>(), wv = new List<Vector3>();
        private readonly List<Vector2> suv = new List<Vector2>(), wuv = new List<Vector2>();
        private readonly List<Color> sc = new List<Color>(), wc = new List<Color>();
        private readonly List<int>[] st = { new List<int>(), new List<int>(), new List<int>() };
        private readonly List<int> wt = new List<int>(), merged = new List<int>();
        private static readonly List<Vector2> scratch = new List<Vector2>(256);
        private static readonly List<float> lengths = new List<float>(256);
        public int Primitives { get; private set; }

        public InkCanvas(Transform parent, string name, int order)
        {
            MeshRenderer Make(string suffix, Material material, int sort, out Mesh mesh)
            {
                var go = new GameObject(name + suffix); go.transform.SetParent(parent, false);
                mesh = new Mesh { name = name + suffix }; mesh.MarkDynamic();
                go.AddComponent<MeshFilter>().sharedMesh = mesh;
                var r = go.AddComponent<MeshRenderer>(); r.sharedMaterial = material; r.sortingOrder = sort;
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; r.enabled = false;
                return r;
            }
            washRenderer = Make(" / wash", MechanicInk.WashMaterial, order, out washMesh);
            strokeRenderer = Make(" / brush", MechanicInk.StrokeMaterial, order + 1, out strokeMesh);
        }

        public void Clear()
        {
            sv.Clear(); suv.Clear(); sc.Clear(); wv.Clear(); wuv.Clear(); wc.Clear(); wt.Clear();
            foreach (var b in st) b.Clear(); Primitives = 0;
        }
        public void Commit()
        {
            washMesh.Clear(); strokeMesh.Clear();
            washRenderer.enabled = wt.Count > 0; strokeRenderer.enabled = sv.Count > 0;
            if (wt.Count > 0) { washMesh.SetVertices(wv); washMesh.SetUVs(0, wuv); washMesh.SetColors(wc); washMesh.SetTriangles(wt, 0, false); washMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2000); }
            if (sv.Count > 0)
            {
                merged.Clear(); foreach (var b in st) merged.AddRange(b);
                strokeMesh.SetVertices(sv); strokeMesh.SetUVs(0, suv); strokeMesh.SetColors(sc); strokeMesh.SetTriangles(merged, 0, false); strokeMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 2000);
            }
        }
        public void Hide() { Clear(); Commit(); }
        public bool Visible => washRenderer.enabled || strokeRenderer.enabled;

        // ---------------- washes ----------------
        private void WashVertex(Vector2 p, Color c) { wv.Add(p); wuv.Add(p * .55f); wc.Add(c); }
        /// <summary>A watercolour sector (or full disc) whose pigment pools toward the edge, like wet paint drying.</summary>
        public void Sector(Vector2 centre, float radius, float fromDegrees, float toDegrees, Color inner, Color edge, float innerRadius = 0)
        {
            if (radius <= .01f || edge.a <= .003f && inner.a <= .003f) return;
            Primitives++;
            float span = toDegrees - fromDegrees; int seg = Mathf.Clamp(Mathf.CeilToInt(Mathf.Abs(span) / 7.5f * Mathf.Clamp(radius, .6f, 2)), 6, 96);
            Color mid = Color.Lerp(inner, edge, .55f);
            int start = wv.Count;
            for (int i = 0; i <= seg; i++)
            {
                float a = (fromDegrees + span * i / seg) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                WashVertex(centre + d * innerRadius, inner); WashVertex(centre + d * Mathf.Lerp(innerRadius, radius, .82f), mid); WashVertex(centre + d * radius, edge);
            }
            for (int i = 0; i < seg; i++)
            {
                int a = start + i * 3, b = a + 3;
                wt.Add(a); wt.Add(a + 1); wt.Add(b); wt.Add(b); wt.Add(a + 1); wt.Add(b + 1);
                wt.Add(a + 1); wt.Add(a + 2); wt.Add(b + 1); wt.Add(b + 1); wt.Add(a + 2); wt.Add(b + 2);
            }
        }
        /// <summary>A flat wash of exact width along a segment (the true hit band of a line strike).</summary>
        public void Band(Vector2 a, Vector2 b, float halfWidth, Color centre, Color edge)
        {
            Vector2 axis = b - a; if (axis.sqrMagnitude < 1e-4f) return; Primitives++;
            Vector2 n = new Vector2(-axis.y, axis.x).normalized * halfWidth, t = axis.normalized * halfWidth;
            int s = wv.Count;
            // Rounded ends follow the capsule the simulation actually tests.
            WashVertex(a - n, edge); WashVertex(a, centre); WashVertex(a + n, edge);
            WashVertex(b - n, edge); WashVertex(b, centre); WashVertex(b + n, edge);
            int[] q = { 0, 1, 3, 3, 1, 4, 1, 2, 4, 4, 2, 5 }; foreach (int i in q) wt.Add(s + i);
            Sector(a, halfWidth, Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg, Mathf.Atan2(n.y, n.x) * Mathf.Rad2Deg + 180, centre, edge);
            Sector(b, halfWidth, Mathf.Atan2(-n.y, -n.x) * Mathf.Rad2Deg, Mathf.Atan2(-n.y, -n.x) * Mathf.Rad2Deg + 180, centre, edge);
        }
        /// <summary>An annulus wash between two radii (the true hit band of a ring strike).</summary>
        public void Annulus(Vector2 centre, float inner, float outer, Color edge, Color middle)
        {
            if (outer <= inner) return; Primitives++;
            int seg = Mathf.Clamp(Mathf.CeilToInt(outer * 28), 24, 120), s = wv.Count;
            for (int i = 0; i <= seg; i++)
            {
                float a = i * Mathf.PI * 2 / seg; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                WashVertex(centre + d * inner, edge); WashVertex(centre + d * (inner + outer) * .5f, middle); WashVertex(centre + d * outer, edge);
            }
            for (int i = 0; i < seg; i++)
            {
                int a = s + i * 3, b = a + 3;
                wt.Add(a); wt.Add(a + 1); wt.Add(b); wt.Add(b); wt.Add(a + 1); wt.Add(b + 1);
                wt.Add(a + 1); wt.Add(a + 2); wt.Add(b + 1); wt.Add(b + 1); wt.Add(a + 2); wt.Add(b + 2);
            }
        }

        /// <summary>A soft candle-light ribbon under a stroke: bright along the path, fading to nothing at its edges.</summary>
        public void GlowRibbon(List<Vector2> path, float head, float tail, float width, Color color)
        {
            if (path.Count < 2 || color.a <= .003f || head <= tail) return; Primitives++;
            float total = 0; for (int i = 1; i < path.Count; i++) total += Vector2.Distance(path[i - 1], path[i]);
            if (total < 1e-4f) return;
            float h = Mathf.Clamp01(head) * total, tl = Mathf.Clamp01(tail) * total, along = 0; int first = wv.Count, rows = 0;
            Color clear = color; clear.a = 0;
            for (int i = 0; i < path.Count; i++)
            {
                if (i > 0) along += Vector2.Distance(path[i - 1], path[i]);
                if (along < tl - .001f && i < path.Count - 1 && along + Vector2.Distance(path[i], path[i + 1]) < tl) continue;
                if (along > h + .001f) break;
                Vector2 tan = path[Mathf.Min(path.Count - 1, i + 1)] - path[Mathf.Max(0, i - 1)]; Vector2 n = tan.sqrMagnitude > 1e-8f ? new Vector2(-tan.y, tan.x).normalized : Vector2.up;
                float k = Mathf.Clamp01(Mathf.Min(along - tl, h - along) / Mathf.Max(.05f, width)); Color c = color; c.a *= .35f + .65f * k;
                WashVertex(path[i] - n * width * .5f, clear); WashVertex(path[i], c); WashVertex(path[i] + n * width * .5f, clear); rows++;
            }
            for (int i = 0; i < rows - 1; i++)
            {
                int a = first + i * 3, b = a + 3;
                wt.Add(a); wt.Add(a + 1); wt.Add(b); wt.Add(b); wt.Add(a + 1); wt.Add(b + 1);
                wt.Add(a + 1); wt.Add(a + 2); wt.Add(b + 1); wt.Add(b + 1); wt.Add(a + 2); wt.Add(b + 2);
            }
        }
        /// <summary>Short inked ticks just inside a rim, like the chapter marks of an antique brass dial.</summary>
        public void Ticks(Vector2 centre, float radius, int count, float length, float width, float startDegrees, Color ink, float fromDegrees = 0, float toDegrees = 360)
        {
            if (count <= 0 || ink.a <= .003f) return;
            InkStroke s = new InkStroke { Width = width, Head = 1, Tail = 0, Taper = 1.4f, Alpha = 1, Ink = ink, InkOnly = true };
            float span = toDegrees - fromDegrees; bool full = Mathf.Abs(span) >= 359;
            for (int i = 0; i < count; i++)
            {
                float a = (full ? startDegrees + i * 360f / count : fromDegrees + span * (i + .5f) / count) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                float l = i % 3 == 0 ? length : length * .55f;
                scratch.Clear(); scratch.Add(centre + d * (radius - l)); scratch.Add(centre + d * radius); Stroke(scratch, s);
            }
        }
        // ---------------- path helpers ----------------
        public static List<Vector2> Path => scratch;
        public static List<Vector2> Arc(Vector2 centre, float rx, float ry, float fromDegrees, float toDegrees, int segments = 0)
        {
            scratch.Clear(); float span = toDegrees - fromDegrees;
            int seg = segments > 0 ? segments : Mathf.Clamp(Mathf.CeilToInt(Mathf.Abs(span) / 6 * Mathf.Clamp(Mathf.Max(rx, ry), .5f, 2.5f)), 4, 120);
            for (int i = 0; i <= seg; i++) { float a = (fromDegrees + span * i / seg) * Mathf.Deg2Rad; scratch.Add(centre + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry)); }
            return scratch;
        }
        /// <summary>A straight path, optionally rippled (wave), zig-zagged (bolt) by at most `amplitude` either side.</summary>
        public static List<Vector2> Line(Vector2 a, Vector2 b, float amplitude = 0, float frequency = 0, bool zigzag = false, float seed = 0)
        {
            scratch.Clear(); float length = Vector2.Distance(a, b);
            int seg = amplitude > 0 ? Mathf.Clamp(Mathf.CeilToInt(length * (zigzag ? 3 : 10)), 3, 80) : 1;
            Vector2 n = length > 1e-4f ? new Vector2(-(b - a).y, (b - a).x) / length : Vector2.zero;
            for (int i = 0; i <= seg; i++)
            {
                float t = i / (float)seg, envelope = Mathf.Sin(t * Mathf.PI);
                float off = amplitude <= 0 ? 0 : zigzag ? (i == 0 || i == seg ? 0 : ((i + (int)seed) % 2 == 0 ? 1 : -1) * amplitude * (.55f + .45f * Mathf.Abs(Mathf.Sin(seed * 7.3f + i * 2.1f)))) : Mathf.Sin(t * length * frequency + seed) * amplitude * Mathf.Clamp01(envelope * 2.5f);
                scratch.Add(Vector2.Lerp(a, b, t) + n * off);
            }
            return scratch;
        }
        public static List<Vector2> Lob(Vector2 a, Vector2 b, float height, int segments = 18)
        {
            scratch.Clear();
            for (int i = 0; i <= segments; i++) { float t = i / (float)segments; scratch.Add(Vector2.Lerp(a, b, t) + Vector2.up * height * 4 * t * (1 - t)); }
            return scratch;
        }
        public static Vector2 Sample(List<Vector2> path, float fraction)
        {
            if (path.Count == 0) return Vector2.zero; if (path.Count == 1) return path[0];
            float total = 0; for (int i = 1; i < path.Count; i++) total += Vector2.Distance(path[i - 1], path[i]);
            float want = Mathf.Clamp01(fraction) * total;
            for (int i = 1; i < path.Count; i++) { float l = Vector2.Distance(path[i - 1], path[i]); if (want <= l || i == path.Count - 1) return Vector2.Lerp(path[i - 1], path[i], l > 0 ? Mathf.Clamp01(want / l) : 0); want -= l; }
            return path[path.Count - 1];
        }

        // ---------------- brush ribbons ----------------
        /// <summary>Paints the [Tail, Head] fraction of a path as a tapered three-pass brush stroke.</summary>
        public void Stroke(List<Vector2> path, InkStroke s, bool closed = false)
        {
            if (path.Count < 2 || s.Alpha <= .003f || s.Width <= .002f || s.Head <= s.Tail) return;
            Primitives++;
            lengths.Clear(); lengths.Add(0);
            for (int i = 1; i < path.Count; i++) lengths.Add(lengths[i - 1] + Vector2.Distance(path[i - 1], path[i]));
            float total = lengths[lengths.Count - 1]; if (total < 1e-4f) return;
            float headL = Mathf.Clamp01(s.Head) * total, tailL = Mathf.Clamp01(s.Tail) * total;
            bool seamless = closed && s.Head >= 1 && s.Tail <= 0;
            float taper = Mathf.Min((headL - tailL) * .5f, s.Width * 2.2f * Mathf.Max(.05f, s.Taper));
            if (s.InkOnly) { Pass(path, total, headL, tailL, seamless, taper, s.Width, 0, s.Ink, s.Alpha, s.Seed, 0); return; }
            // the contour stays a confident hand-inked line (at most ~0.035 units a side) however broad the stroke is
            float body = Mathf.Max(s.Width - .07f, s.Width * .7f);
            Pass(path, total, headL, tailL, seamless, taper, s.Width, 0, s.Ink, s.Alpha * .9f, s.Seed, 0);
            Pass(path, total, headL, tailL, seamless, taper, body, 0, s.Body, s.Alpha, s.Seed + .37f, 1);
            if (!s.NoLight) Pass(path, total, headL, tailL, seamless, taper * .8f, body * .34f, body * s.LightOffset * 1.4f, s.Light, s.Alpha * .95f, s.Seed + .71f, 2);
        }
        private void Pass(List<Vector2> path, float total, float headL, float tailL, bool seamless, float taper, float width, float offset, Color color, float alpha, float seed, int bucket)
        {
            int first = sv.Count; int emitted = 0;
            void Emit(Vector2 p, Vector2 tangent, float along)
            {
                float k = 1;
                if (!seamless && taper > 1e-4f)
                {
                    float fromTail = (along - tailL) / taper, fromHead = (headL - along) / taper;
                    k = Mathf.Min(Mathf.Clamp01(fromTail), Mathf.Clamp01(fromHead)); k = Mathf.Sqrt(k) * (.25f + .75f * k);
                    k = Mathf.Max(.06f, k);
                }
                Vector2 n = new Vector2(-tangent.y, tangent.x); float h = width * .5f * k;
                Vector2 c = p + n * offset * k;
                float u = along / Mathf.Max(.05f, width * 2.6f) + seed * 3.1f;
                Color col = color; col.a *= alpha;
                sv.Add(c - n * h); suv.Add(new Vector2(u, 0)); sc.Add(col);
                sv.Add(c + n * h); suv.Add(new Vector2(u, 1)); sc.Add(col);
                emitted++;
            }
            Vector2 Tangent(int i)
            {
                Vector2 a = path[Mathf.Max(0, i - 1)], b = path[Mathf.Min(path.Count - 1, i + 1)];
                if (seamless && (i == 0 || i == path.Count - 1)) { a = path[path.Count - 2]; b = path[1]; }
                Vector2 t = b - a; return t.sqrMagnitude > 1e-8f ? t.normalized : Vector2.right;
            }
            for (int i = 0; i < path.Count - 1; i++)
            {
                float l0 = lengths[i], l1 = lengths[i + 1];
                if (l1 < tailL || l0 > headL) continue;
                Vector2 seg = path[i + 1] - path[i]; Vector2 t = seg.sqrMagnitude > 1e-8f ? seg.normalized : Vector2.right;
                if (emitted == 0)
                {
                    float f = l1 > l0 ? Mathf.Clamp01((tailL - l0) / (l1 - l0)) : 0;
                    Emit(Vector2.Lerp(path[i], path[i + 1], f), f <= 0 ? Tangent(i) : t, Mathf.Max(l0, tailL));
                }
                // subdivide long segments so tapers stay smooth on straight strokes
                float a0 = Mathf.Max(l0, tailL), a1 = Mathf.Min(l1, headL);
                int steps = Mathf.Clamp(Mathf.CeilToInt((a1 - a0) / Mathf.Max(.04f, taper * .3f)), 1, 24);
                for (int k = 1; k <= steps; k++)
                {
                    float along = Mathf.Lerp(a0, a1, k / (float)steps), f = l1 > l0 ? (along - l0) / (l1 - l0) : 1;
                    Emit(Vector2.Lerp(path[i], path[i + 1], f), k == steps && along >= l1 - 1e-5f ? Tangent(i + 1) : t, along);
                }
            }
            var tris = st[bucket];
            for (int i = 0; i < emitted - 1; i++) { int a = first + i * 2; tris.Add(a); tris.Add(a + 1); tris.Add(a + 2); tris.Add(a + 2); tris.Add(a + 1); tris.Add(a + 3); }
        }
    }
}
