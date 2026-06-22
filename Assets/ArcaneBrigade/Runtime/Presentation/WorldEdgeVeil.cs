using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Past the edge of a fight's floor the night goes on (requirement 25/10/2026: the map's ragged rims looked bad):
    /// from the floor's true edge a dark wash deepens over a short band into endless black, on all four sides. Drawn only in
    /// fights; the shop's room keeps its own walls.</summary>
    public sealed class WorldEdgeVeil : MonoBehaviour
    {
        private const float Band = 1.6f, Reach = 80f;
        private static readonly Color Night = new Color(.025f, .02f, .04f, 1);
        private ArenaController owner;
        private readonly SpriteRenderer[] sides = new SpriteRenderer[4];
        private static Sprite ramp;

        public void Configure(ArenaController value) => owner = value;

        private void LateUpdate()
        {
            bool show = owner != null && owner.Session != null && owner.Session.State.Phase == SessionPhase.Combat;
            if (sides[0] == null)
            {
                if (!show) return;
                for (int i = 0; i < 4; i++)
                {
                    var side = new GameObject("WorldEdgeVeil" + i);
                    side.transform.SetParent(transform, false);
                    sides[i] = side.AddComponent<SpriteRenderer>(); sides[i].sprite = Ramp(); sides[i].color = Night;
                    sides[i].sortingOrder = 4000; // over the scenery that stands past the edge, under the HUD (drawn in OnGUI)
                }
            }
            for (int i = 0; i < 4; i++) sides[i].enabled = show;
            if (!show) return;
            Vector2 half = owner.WorldBounds * .5f;
            // each side: the ramp's clear end on the floor's edge, its dark end Band further out, then solid night to Reach
            Place(sides[0], new Vector2(-half.x, 0), 180, half.y * 2 + Reach * 2); // left
            Place(sides[1], new Vector2(half.x, 0), 0, half.y * 2 + Reach * 2);    // right
            Place(sides[2], new Vector2(0, half.y), 90, half.x * 2 + Reach * 2);   // top
            Place(sides[3], new Vector2(0, -half.y), 270, half.x * 2 + Reach * 2); // bottom
        }

        /// <summary>A side starts at the edge and runs outward along its facing angle; its length along the edge covers the corners.</summary>
        private static void Place(SpriteRenderer side, Vector2 edge, float facing, float along)
        {
            float length = Band + Reach;
            Vector2 outward = new Vector2(Mathf.Cos(facing * Mathf.Deg2Rad), Mathf.Sin(facing * Mathf.Deg2Rad));
            side.transform.position = (Vector3)(edge + outward * (length * .5f));
            side.transform.rotation = Quaternion.Euler(0, 0, facing);
            side.transform.localScale = new Vector3(length, along, 1); // the ramp sprite is 1 x 1 world unit
        }

        /// <summary>Clear at its start, dark by Band, then solid: one texture stretched along the side.</summary>
        private static Sprite Ramp()
        {
            if (ramp != null) return ramp;
            const int width = 2048; // fine enough that the short fade band is smooth
            var texture = new Texture2D(width, 1, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.HideAndDontSave };
            float fadeShare = Band / (Band + Reach);
            for (int x = 0; x < width; x++)
            {
                float u = (x + .5f) / width, a = Mathf.SmoothStep(0, 1, Mathf.Clamp01(u / fadeShare));
                texture.SetPixel(x, 0, new Color(1, 1, 1, a));
            }
            texture.Apply(false, true);
            ramp = Sprite.Create(texture, new Rect(0, 0, width, 1), new Vector2(.5f, .5f), width);
            ramp.hideFlags = HideFlags.HideAndDontSave;
            return ramp;
        }
    }
}
