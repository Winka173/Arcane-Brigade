using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class EvolutionAdornment : MonoBehaviour
    {
        private readonly List<SpriteRenderer> pieces = new List<SpriteRenderer>();
        private readonly List<Vector3> origins = new List<Vector3>();
        private readonly List<int> layers = new List<int>();
        private bool boss;
        private Transform anchor;
        public int PieceCount => pieces.Count;
        public void Build(UnitState unit, SpriteRenderer body, SpriteRenderer weapon)
        {
            boss = unit.Definition.IsBoss;
            int tier = Mathf.Clamp(unit.Definition.CombatTier, 1, 3);
            if (!boss && (unit.Faction != Faction.Companion || tier == 1)) return;
            EvolutionArtwork art = Resources.Load<EvolutionArtwork>("EvolutionArtwork");
            if (art == null || art.Parts.Length < 12) return;
            anchor = body.transform;
            int material = (int)unit.Definition.Material;
            if (boss)
            {
                Rect ink = SpriteInkMetrics.BoundsOf(body.sprite);
                float edge = ink.width / Mathf.Max(0.01f, ink.height) * 0.43f;
                for (int side = -1; side <= 1; side += 2)
                {
                    Sprite organ = art.Parts[(unit.Definition.BossPattern + (side > 0 ? 1 : 0)) % 4];
                    Vector2 position = new Vector2(side * edge, -0.04f);
                    Add("OrganContactShadow", body.transform, organ, position + new Vector2(0.012f, -0.018f), 0.245f, 4, new Color(0.12f, 0.06f, 0.17f, 0.7f));
                    Add("BreakableOrgan", body.transform, organ, position, 0.23f, 5, Color.white);
                }
                return;
            }
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
                piece.color = boss && index % 2 == 0 ? new Color(0.12f, 0.06f, 0.17f, 0.7f) : Color.white;
                if (!boss) continue;
                bool left = index < 2;
                bool broken = left ? unit.LeftPartBroken : unit.RightPartBroken;
                float progress = Mathf.Clamp01((left ? unit.LeftPartDamage : unit.RightPartDamage) / BossPartRules.Durability(unit));
                if (index % 2 == 1) piece.color = Color.Lerp(Color.white, new Color(1, 0.6f, 0.4f), progress);
                float age = time - (left ? unit.LeftPartBrokenAt : unit.RightPartBrokenAt);
                if (!broken) continue;
                piece.enabled = unit.Alive && !reduced && age < 0.55f;
                piece.transform.localPosition += new Vector3((left ? -1 : 1) * age * 0.6f, -age * age * 2, 0) / Mathf.Max(0.01f, anchor.localScale.y);
                piece.transform.localRotation = Quaternion.Euler(0, 0, (left ? -1 : 1) * age * 180);
                Color fade = piece.color; fade.a *= 1 - Mathf.Clamp01(age / 0.55f); piece.color = fade;
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
