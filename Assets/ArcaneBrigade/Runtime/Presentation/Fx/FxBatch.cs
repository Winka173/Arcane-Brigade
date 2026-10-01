using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The shapes the FxProc shader can draw into one quad (see Resources/Shaders/FxProc.shader).</summary>
    public enum FxShape { Glow = 0, Flame = 1, Beam = 2, Vortex = 3, Shock = 4, Sparkle = 5, Smoke = 6, Rays = 7, Cracks = 8, Disc = 9, Shade = 10, Link = 11, Dust = 12 }

    /// <summary>Every effect quad of a frame, rebuilt each frame into two meshes: additive light and alpha-blended smoke. One
    /// draw call each, however many pieces an effect is made of.</summary>
    public sealed class FxBatch
    {
        private sealed class Layer
        {
            public readonly Mesh Mesh = new Mesh { name = "Fx" };
            public readonly MeshRenderer Renderer;
            public readonly List<Vector3> V = new List<Vector3>(2048);
            public readonly List<Color> C = new List<Color>(2048);
            public readonly List<Vector2> U = new List<Vector2>(2048);
            public readonly List<Vector4> P = new List<Vector4>(2048);
            public readonly List<int> T = new List<int>(3072);
            public Layer(Transform parent, string name, int order, bool additive)
            {
                Mesh.MarkDynamic(); Mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                var go = new GameObject(name); go.transform.SetParent(parent, false);
                go.AddComponent<MeshFilter>().sharedMesh = Mesh;
                Renderer = go.AddComponent<MeshRenderer>();
                var material = new Material(Shader.Find("ArcaneBrigade/FxProc")) { name = name };
                material.SetFloat("_Src", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_Dst", (float)(additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
                Renderer.sharedMaterial = material; Renderer.sortingOrder = order;
                Renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; Renderer.receiveShadows = false; Renderer.enabled = false;
            }
            public void Clear() { V.Clear(); C.Clear(); U.Clear(); P.Clear(); T.Clear(); }
            public void Flush()
            {
                Renderer.enabled = V.Count > 0;
                if (V.Count == 0) return;
                Mesh.Clear(); Mesh.SetVertices(V); Mesh.SetColors(C); Mesh.SetUVs(0, U); Mesh.SetUVs(1, P); Mesh.SetTriangles(T, 0, false);
                Mesh.bounds = new Bounds(Vector3.zero, new Vector3(400, 400, 10));
            }
        }

        private readonly Layer light, dust;
        public int Quads { get; private set; }
        public const int MaxQuads = 6000;
        private const float BigSpan = 4.5f, MaxSpan = 9f; // half-extents in world units (the arena is 24 x 13)

        public FxBatch(Transform parent, int lightOrder = 2145, int dustOrder = 2144)
        {
            light = new Layer(parent, "Fx light", lightOrder, true);
            dust = new Layer(parent, "Fx dust", dustOrder, false);
        }

        public void Begin() { light.Clear(); dust.Clear(); Quads = 0; }
        public void End() { light.Flush(); dust.Flush(); }

        /// <summary>A shape centred on a point. <paramref name="size"/> is the half-extent (world units), rotated by degrees.</summary>
        public void Quad(FxShape shape, Vector2 at, Vector2 size, float degrees, Color tint, float progress = 0, int seed = 0, float param = 0)
        {
            if (Quads >= MaxQuads || tint.a <= .003f) return;
            // a shape bigger than a crowd is dimmed (and kept under a size), so no single effect blinds the field
            float span = Mathf.Max(size.x, size.y);
            if (shape != FxShape.Beam && shape != FxShape.Link && span > BigSpan)
            {
                tint.a *= Mathf.Clamp01(BigSpan / span) * (shape == FxShape.Shade || shape == FxShape.Smoke || shape == FxShape.Dust ? 1 : .8f);
                if (span > MaxSpan) size *= MaxSpan / span;
            }
            if (tint.a <= .003f) return;
            Layer l = shape == FxShape.Smoke || shape == FxShape.Shade || shape == FxShape.Dust ? dust : light; Quads++;
            float c = Mathf.Cos(degrees * Mathf.Deg2Rad), s = Mathf.Sin(degrees * Mathf.Deg2Rad);
            Vector2 ax = new Vector2(c, s) * size.x, ay = new Vector2(-s, c) * size.y;
            int n = l.V.Count; var p = new Vector4((float)shape, progress, (seed % 997) * .731f, param);
            for (int k = 0; k < 4; k++)
            {
                float ux = (k == 1 || k == 2) ? 1 : -1, uy = k >= 2 ? 1 : -1;
                Vector2 w = at + ax * ux + ay * uy;
                l.V.Add(new Vector3(w.x, w.y, 0)); l.C.Add(tint); l.U.Add(new Vector2(ux, uy)); l.P.Add(p);
            }
            l.T.Add(n); l.T.Add(n + 2); l.T.Add(n + 1); l.T.Add(n); l.T.Add(n + 3); l.T.Add(n + 2);
        }

        public void Quad(FxShape shape, Vector2 at, float radius, Color tint, float progress = 0, int seed = 0, float param = 0)
            => Quad(shape, at, new Vector2(radius, radius), 0, tint, progress, seed, param);

        public void Quad(FxShape shape, Vector2 at, float radius, float degrees, Color tint, float progress, int seed, float param)
            => Quad(shape, at, new Vector2(radius, radius), degrees, tint, progress, seed, param);

        /// <summary>A shape on the floor: squashed to the arena's slant so it lies flat under the units.</summary>
        public void Floor(FxShape shape, Vector2 at, float radius, Color tint, float progress = 0, int seed = 0, float param = 0)
            => Quad(shape, at, new Vector2(radius, radius * .62f), 0, tint, progress, seed, param);

        /// <summary>A beam from one point to another, <paramref name="width"/> across (half-width in world units).</summary>
        public void Link(Vector2 from, Vector2 to, float width, Color tint, int seed = 0, float thin = 0)
        {
            Vector2 d = to - from; float len = d.magnitude; if (len < .001f) return; Vector2 n = d / len;
            Quad(FxShape.Link, (from + to) * .5f, new Vector2(len * .5f + width * 1.1f, width), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, tint, 0, seed, thin);
        }

        public void Beam(Vector2 from, Vector2 to, float width, Color tint, float progress = 0, int seed = 0, float thin = 0)
        {
            Vector2 d = to - from; float len = d.magnitude; if (len < .001f) return;
            Quad(FxShape.Beam, (from + to) * .5f, new Vector2(len * .5f, width), Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, tint, progress, seed, thin);
        }
    }
}
