using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Enemies step out of the night (requirement 24/10/2026): where one appears inside the view, a small violet tear
    /// opens in the air, the enemy steps through and the tear pinches shut (about half a second). Most enemies arrive past the
    /// edge of the screen and need none. One canvas for every tear; packs share one; at most eight at a time.</summary>
    public static class EnemyRifts
    {
        private const float Life = .5f, Open = .12f;
        private struct Tear { public Vector2 At; public float Born, Height; public int Seed; }
        private static readonly List<Tear> tears = new List<Tear>();
        private static readonly List<Vector2> edge = new List<Vector2>();
        private static InkCanvas canvas, light;
        private static GameObject holder;

        public static void Play(UnitState enemy, Camera view)
        {
            if (enemy?.Definition == null || enemy.Definition.IsBoss || view == null || tears.Count >= 8) return;
            Vector3 seen = view.WorldToViewportPoint(enemy.Position);
            if (seen.x < .02f || seen.x > .98f || seen.y < .02f || seen.y > .98f) return; // arriving off screen: nothing to show
            foreach (Tear t in tears) if ((t.At - enemy.Position).sqrMagnitude < .5f) return; // a pack steps through one tear
            tears.Add(new Tear { At = enemy.Position, Born = Time.time, Height = Mathf.Clamp(enemy.Definition.BodyHeight * 1.15f, .6f, 1.6f), Seed = enemy.Id });
        }

        public static void Draw(int order)
        {
            if (tears.Count == 0) { if (holder != null && canvas.Visible) { canvas.Hide(); light.Hide(); } return; }
            if (holder == null) { holder = new GameObject("Enemy rifts"); canvas = new InkCanvas(holder.transform, "Enemy rift ink", order); light = new InkCanvas(holder.transform, "Enemy rift light", order + 2, true); }
            canvas.SetOrder(order); light.SetOrder(order + 2);
            canvas.Clear(); light.Clear();
            float now = Time.time;
            tears.RemoveAll(t => now - t.Born >= Life);
            foreach (Tear t in tears)
            {
                float age = now - t.Born;
                // it tears open fast, holds while the body steps out, then pinches shut from both ends
                float open = age < Open ? Mathf.SmoothStep(0, 1, age / Open) : 1 - Mathf.SmoothStep(0, 1, (age - Open) / (Life - Open));
                float half = t.Height * .5f * Mathf.Lerp(.35f, 1, Mathf.Sqrt(open)), width = .42f * open;
                Vector2 top = t.At + new Vector2(.04f, half), bottom = t.At - new Vector2(.04f, half);
                List<Vector2> slit = InkCanvas.Line(top, bottom, .05f, 0, true, t.Seed);
                light.GlowRibbon(slit, 1, 0, width * 3.2f, new Color(.62f, .36f, .95f, .55f * open));
                canvas.Stroke(slit, InkStroke.Of(ArsenalInk.Midnight, width, .95f, t.Seed));
                // the torn lips catch a violet light
                for (int side = -1; side <= 1; side += 2)
                {
                    edge.Clear();
                    for (int i = 0; i <= 6; i++)
                    {
                        float k = i / 6f, bulge = Mathf.Sin(k * Mathf.PI) * width * .55f;
                        edge.Add(Vector2.Lerp(top, bottom, k) + Vector2.right * side * (bulge + ((i + t.Seed) % 2 == 0 ? .015f : -.01f)));
                    }
                    light.GlowRibbon(edge, 1, 0, .09f, new Color(.85f, .7f, 1f, .9f * open));
                }
            }
            canvas.Commit(); light.Commit();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset() { tears.Clear(); holder = null; canvas = light = null; }
    }
}
