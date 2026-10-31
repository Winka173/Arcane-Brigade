using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The hero's foot crest: a dark contact pool so the feet read on any tile and a soft teal light in it, breathing
    /// slowly. No ring, ticks, studs, arrow or dashed arc (requirement, repeated 31/10/2026: no rings on our side).</summary>
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
            float breathe = reducedMotion ? 0 : Mathf.Sin(time * 2.2f) * .5f + .5f;

            // a dark contact pool and a soft teal light under the feet: no rings, ticks, studs, arrow or dashes on our side
            // (requirement, repeated 31/10/2026)
            ground.Clear();
            ground.Sector(Vector2.zero, r * 1.18f, 0, 360, new Color(Pool.r, Pool.g, Pool.b, .55f), new Color(Pool.r, Pool.g, Pool.b, 0));
            ground.Sector(Vector2.zero, r * 1.0f, 0, 360, new Color(Teal.r, Teal.g, Teal.b, .32f + .1f * breathe), new Color(Teal.r, Teal.g, Teal.b, 0));
            ground.Commit();
            over.Clear(); over.Commit();
        }

        private void OnDisable() { if (ground != null) { ground.Hide(); over.Hide(); } }
    }
}
