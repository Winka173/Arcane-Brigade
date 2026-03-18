using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The hero's foot crest (18/10/2026; replaces the squashed teal star): a painted brass compass ring lying on the
    /// floor in perspective, with a dark contact pool so it reads on any tile, slow-turning chapter ticks, four brass studs and
    /// a teal arrowhead that points where the hero faces. A faint dashed front arc is also inked above the crowd, so the hero's
    /// feet stay findable when enemies stand in front of them.</summary>
    public sealed class HeroCrestVisual : MonoBehaviour
    {
        public const int OverCrowdOrder = 1460;
        private static readonly Color Ink = new Color(.11f, .08f, .06f), Brass = new Color(.86f, .68f, .36f), Cream = new Color(1f, .95f, .8f),
            Teal = new Color(.38f, .88f, .8f), Pool = new Color(.03f, .05f, .08f);
        private Transform floor;
        private InkCanvas ground, over;
        private static readonly List<Vector2> path = new List<Vector2>(64);

        private void Ensure(int order)
        {
            if (floor != null) return;
            floor = new GameObject("Hero crest").transform; floor.SetParent(transform, false);
            ground = new InkCanvas(floor, "Hero crest ground", order);
            over = new InkCanvas(floor, "Hero crest over crowd", OverCrowdOrder);
        }

        /// <param name="feet">local position of the feet under the body</param>
        public void Display(UnitState state, Vector2 feet, float height, int order, float time, bool reducedMotion)
        {
            Ensure(order - 3);
            bool show = state.Alive && state.Faction == Faction.Champion;
            if (!show) { ground.Hide(); over.Hide(); return; }
            floor.localPosition = new Vector3(feet.x, feet.y, 0);
            floor.localScale = new Vector3(1, .42f, 1); // the floor seen from the game's elevated view
            float r = Mathf.Clamp(height * .62f, .42f, .78f);
            float spin = reducedMotion ? 0 : time * 14f, breathe = reducedMotion ? 0 : Mathf.Sin(time * 2.2f) * .5f + .5f;
            Vector2 facing = state.Facing.sqrMagnitude > .01f ? state.Facing.normalized : Vector2.right;
            // facing in floor space: the crest is squashed vertically, so the arrow's direction is stretched back up
            Vector2 dir = new Vector2(facing.x, facing.y / .42f).normalized;

            ground.Clear();
            ground.Sector(Vector2.zero, r * 1.18f, 0, 360, new Color(Pool.r, Pool.g, Pool.b, .62f), new Color(Pool.r, Pool.g, Pool.b, 0));
            ground.Annulus(Vector2.zero, r * .74f, r * 1.04f, new Color(Teal.r, Teal.g, Teal.b, 0), new Color(Teal.r, Teal.g, Teal.b, .3f + .1f * breathe));
            var ring = new InkStroke { Width = .095f, Head = 1, Tail = 0, Taper = 1, Alpha = 1, Ink = Ink, Body = Teal, Light = Cream, LightOffset = .18f };
            ground.Stroke(InkCanvas.Arc(Vector2.zero, r * .92f, r * .92f, 0, 360, 56), ring, true);
            var inner = new InkStroke { Width = .04f, Head = 1, Tail = 0, Taper = 1, Alpha = .9f, Ink = Ink, Body = Brass, Light = Cream, NoLight = true };
            ground.Stroke(InkCanvas.Arc(Vector2.zero, r * .7f, r * .7f, 0, 360, 44), inner, true);
            ground.Ticks(Vector2.zero, r * .84f, 16, r * .14f, .03f, spin, new Color(Cream.r, Cream.g, Cream.b, .85f));
            // four brass studs on the diagonals, each a small lit lozenge
            for (int i = 0; i < 4; i++)
            {
                float a = (45 + i * 90 + spin * .25f) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                path.Clear(); path.Add(d * r * .82f); path.Add(d * r * 1.02f);
                ground.Stroke(path, new InkStroke { Width = .1f, Head = 1, Tail = 0, Taper = 1.6f, Alpha = 1, Ink = Ink, Body = Brass, Light = Cream, LightOffset = .2f });
            }
            // the teal arrowhead ahead of the feet: where the hero faces
            Vector2 side = new Vector2(-dir.y, dir.x);
            float lift = reducedMotion ? 0 : breathe * r * .05f;
            Vector2 tip = dir * (r * 1.42f + lift), baseL = dir * (r * 1.06f + lift) + side * r * .26f, baseR = dir * (r * 1.06f + lift) - side * r * .26f;
            var arrow = new InkStroke { Width = .1f, Head = 1, Tail = 0, Taper = 1.2f, Alpha = 1, Ink = Ink, Body = Teal, Light = Cream, LightOffset = .2f };
            path.Clear(); path.Add(baseL); path.Add(tip); path.Add(baseR); ground.Stroke(path, arrow);
            ground.Commit();

            // over the crowd: a faint dashed front arc and the arrowhead's ink, so the crest reads when enemies stand in front
            over.Clear();
            var dash = new InkStroke { Width = .03f, Head = 1, Tail = 0, Taper = 1, Alpha = .5f, Ink = Cream, InkOnly = true };
            for (int i = 0; i < 7; i++)
            {
                float from = 196 + i * 21 + spin * .5f;
                over.Stroke(InkCanvas.Arc(Vector2.zero, r * .92f, r * .92f, from, from + 11, 4), dash);
            }
            path.Clear(); path.Add(baseL); path.Add(tip); path.Add(baseR);
            over.Stroke(path, new InkStroke { Width = .028f, Head = 1, Tail = 0, Taper = 1, Alpha = .5f, Ink = Teal, InkOnly = true });
            over.Commit();
        }

        private void OnDisable() { if (ground != null) { ground.Hide(); over.Hide(); } }
    }
}
