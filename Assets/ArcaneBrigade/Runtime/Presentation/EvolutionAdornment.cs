using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class EvolutionAdornment : MonoBehaviour
    {
        private readonly List<SpriteRenderer> pieces = new List<SpriteRenderer>();
        private readonly List<Vector3> origins = new List<Vector3>();
        private readonly List<int> layers = new List<int>();
        public int PieceCount => pieces.Count;
        public void Build(UnitState unit, SpriteRenderer body, SpriteRenderer weapon)
        {
            int tier = Mathf.Clamp(unit.Definition.CombatTier, 1, 3);
            if (unit.Faction != Faction.Companion || tier == 1) return;
            EvolutionArtwork art = Resources.Load<EvolutionArtwork>("EvolutionArtwork");
            if (art == null || art.Parts.Length < 12) return;
            int material = (int)unit.Definition.Material;
            int hat = material == (int)BodyMaterial.Wood ? 7 : material == (int)BodyMaterial.Metal || material == (int)BodyMaterial.Glass ? 5 : material == (int)BodyMaterial.Fabric ? 6 : 4;
            Add("EvolvedMantle", body.transform, art.Parts[tier == 3 ? 9 : 8], new Vector2(0, -0.12f), tier == 3 ? 0.75f : 0.57f, -2, Color.white);
            Add("AscensionHeaddress", body.transform, art.Parts[hat], new Vector2(0, 0.42f), tier == 3 ? 0.32f : 0.25f, 5, Color.white);
            Add("AscensionClasp", body.transform, art.Parts[10], new Vector2(0, -0.14f), tier == 3 ? 0.16f : 0.12f, 5, Color.white);
            if (tier == 3)
            {
                Add("MasterPennant", body.transform, art.Parts[11], new Vector2(-0.2f, -0.32f), 0.35f, -1, Color.white);
                Add("WeaponAscensionCharm", weapon.transform, art.Parts[11], new Vector2(-0.1f, -0.14f), 0.4f, 6, Color.white);
            }
        }
        private void Add(string label, Transform parent, Sprite sprite, Vector2 offset, float relativeHeight, int layer, Color color)
        {
            var child = new GameObject(label, typeof(SpriteRenderer)); child.transform.SetParent(parent, false);
            Rect ink = SpriteInkMetrics.BoundsOf(parent.GetComponent<SpriteRenderer>().sprite);
            child.transform.localPosition = (Vector3)(ink.center + offset * ink.height);
            child.transform.localScale = Vector3.one * ink.height * relativeHeight / Mathf.Max(0.01f, sprite.bounds.size.y);
            SpriteRenderer renderer = child.GetComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.color = color;
            pieces.Add(renderer); origins.Add(child.transform.localPosition); layers.Add(layer);
        }
        public void Display(UnitState unit, float time, int order, bool reduced)
        {
            for (int index = 0; index < pieces.Count; index++)
            {
                SpriteRenderer piece = pieces[index]; piece.enabled = unit.Alive;
                piece.sortingOrder = order + layers[index];
                float sway = reduced ? 0 : Mathf.Sin(time * 3 + index * 0.6f) * (unit.Velocity.sqrMagnitude > 0.1f ? 7 : 2);
                piece.transform.localRotation = Quaternion.Euler(0, 0, sway);
                piece.transform.localPosition = origins[index];
                piece.color = Color.white;
            }
        }
        public void Fade(float alpha)
        {
            foreach (SpriteRenderer piece in pieces) { Color color = piece.color; color.a *= alpha; piece.color = color; }
        }
        public void Clear()
        {
            foreach (SpriteRenderer piece in pieces) if (piece != null) { piece.enabled = false; if (Application.isPlaying) Destroy(piece.gameObject); else DestroyImmediate(piece.gameObject); }
            pieces.Clear(); origins.Clear(); layers.Clear();
        }
        private void OnDestroy() => Clear();
    }
}
