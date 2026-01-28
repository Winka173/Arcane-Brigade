using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BuildMorphAppearance : MonoBehaviour
    {
        private sealed class Part
        {
            public SpriteRenderer View;
            public Vector2 Offset;
            public float Height;
            public int Layer, Orbit = -1;
            public bool Weapon;
        }
        private readonly List<Part> parts = new List<Part>();
        private SpriteRenderer body, weapon;
        public int PartCount => parts.Count;
        public void Build(UnitState unit, SpriteRenderer bodyView, SpriteRenderer weaponView)
        {
            Clear(); body = bodyView; weapon = weaponView;
            int stage = Mathf.Clamp(unit.Definition.BuildMorphStage, 0, 3);
            if (unit.Faction == Faction.Enemy || stage == 0) return;
            BuildMorphArtwork art = Resources.Load<BuildMorphArtwork>("BuildMorphArtwork");
            if (art == null) return;
            BuildMorphKind kind = unit.Definition.BuildMorph;
            void Add(Sprite sprite, Vector2 offset, float height, int layer, bool onWeapon = false, int orbit = -1)
            {
                if (sprite == null) return;
                var child = new GameObject("Build" + kind + "Part" + parts.Count); child.transform.SetParent(onWeapon ? weapon.transform : body.transform, false);
                var view = child.AddComponent<SpriteRenderer>(); view.sprite = sprite; view.sharedMaterial = body.sharedMaterial;
                parts.Add(new Part { View = view, Offset = offset, Height = height, Layer = layer, Weapon = onWeapon, Orbit = orbit });
            }
            Add(art.Charm(kind), new Vector2(-.08f, -.13f), .25f, 6);
            Add(art.Charm(kind), new Vector2(-.25f, -.2f), .29f, 6, true);
            if (stage >= 2)
            {
                Add(art.Back(kind), new Vector2(-.3f, .32f), stage == 3 ? 1.65f : 1.35f, -3);
                Add(art.Charm(kind), new Vector2(-.35f, .14f), .26f, 5);
                Add(art.Charm(kind), new Vector2(.34f, .14f), .26f, 5);
            }
            if (stage == 3)
            {
                Add(art.Charm(kind), new Vector2(0, .62f), .3f, 6);
                for (int i = 0; i < 3; i++) Add(art.Charm(kind), Vector2.zero, .19f, 3, orbit: i);
            }
        }
        public void Display(UnitState unit, float time, int order, bool reduced)
        {
            if (body == null || body.sprite == null) return;
            Rect bodyInk = SpriteInkMetrics.BoundsOf(body.sprite);
            foreach (Part part in parts)
            {
                part.View.enabled = unit.Alive;
                SpriteRenderer anchor = part.Weapon ? weapon : body;
                if (anchor == null || anchor.sprite == null) { part.View.enabled = false; continue; }
                if (part.Weapon && !anchor.enabled) part.View.enabled = false;
                Rect ink = part.Weapon ? SpriteInkMetrics.BoundsOf(anchor.sprite) : bodyInk;
                Vector2 offset = part.Offset;
                if (!part.Weapon && part.Orbit < 0 && unit.Facing.x < 0) offset.x = -offset.x;
                if (part.Orbit >= 0)
                {
                    float angle = (reduced ? 0 : time * 26) + part.Orbit * 120 + (int)unit.Definition.BuildMorph * 11;
                    float a = angle * Mathf.Deg2Rad;
                    offset = new Vector2(Mathf.Cos(a) * .64f, .21f + Mathf.Sin(a) * .34f);
                }
                part.View.transform.localPosition = (Vector3)(ink.center + offset * ink.height);
                Rect partInk = SpriteInkMetrics.BoundsOf(part.View.sprite);
                float scale = ink.height * part.Height / Mathf.Max(.01f, partInk.height);
                part.View.transform.localScale = Vector3.one * scale;
                // Trimmed atlas pivots are at the bottom: centre the visible ink explicitly.
                part.View.transform.localPosition -= (Vector3)(partInk.center * scale);
                float sway = reduced || part.Orbit >= 0 ? 0 : Mathf.Sin(time * 3 + unit.Id) * (unit.Velocity.sqrMagnitude > .1f ? 4 : 1);
                part.View.transform.localRotation = Quaternion.Euler(0, 0, sway);
                part.View.sortingOrder = (part.Weapon ? weapon.sortingOrder : order) + part.Layer;
                float alpha = part.Orbit >= 0 ? PresentationSettings.ReducedEffects ? .65f : .9f : 1;
                part.View.color = new Color(1, 1, 1, alpha);
            }
        }
        public static Vector2 WeaponShape(UnitDefinition unit)
        {
            float growth = Mathf.Clamp(unit.BuildMorphStage, 0, 3) * .045f;
            switch (unit.BuildMorph)
            {
                case BuildMorphKind.Thread: case BuildMorphKind.Frost: return new Vector2(1, 1 + growth);
                case BuildMorphKind.Clockwork: case BuildMorphKind.Paper: return new Vector2(1 + growth, 1 + growth * .4f);
                case BuildMorphKind.Chorus: case BuildMorphKind.Porcelain: return new Vector2(1 + growth * .3f, 1 + growth);
                default: return Vector2.one * (1 + growth * .5f);
            }
        }
        public void Clear()
        {
            foreach (Part part in parts) if (part.View != null) { part.View.enabled = false; if (Application.isPlaying) Destroy(part.View.gameObject); else DestroyImmediate(part.View.gameObject); }
            parts.Clear();
        }
        public void Fade(float alpha)
        {
            foreach (Part part in parts) { Color color = part.View.color; color.a *= alpha; part.View.color = color; }
        }
        private void OnDestroy() => Clear();
    }
}
