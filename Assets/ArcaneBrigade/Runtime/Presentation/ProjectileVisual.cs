using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ProjectileVisual : MonoBehaviour
    {
        private SpriteRenderer halo;
        private SpriteRenderer core;
        private LineRenderer streak;
        private LineRenderer motif;
        private Material material;
        private static readonly Vector2[] FishMotif = { new Vector2(0.19f, 0), new Vector2(-0.03f, 0.1f), new Vector2(-0.16f, -0.12f), new Vector2(-0.16f, 0.12f), new Vector2(-0.03f, -0.1f) };
        private static readonly Vector2[] NoteMotif = { new Vector2(-0.03f, -0.08f), new Vector2(0.05f, -0.08f), new Vector2(0.05f, 0.22f), new Vector2(0.14f, 0.18f), new Vector2(0.14f, 0.1f) };

        public void Configure(Sprite disc)
        {
            if (halo != null) return;
            halo = Child("Halo", disc);
            core = Child("Core", disc);
            var child = new GameObject("MotionStreak");
            child.transform.SetParent(transform, false);
            streak = child.AddComponent<LineRenderer>();
            streak.positionCount = 2;
            streak.numCapVertices = 6;
            streak.numCornerVertices = 6;
            material = new Material(Shader.Find("Sprites/Default"));
            streak.sharedMaterial = material;
            streak.sortingOrder = 1800;
            var glyph = new GameObject("ProjectileMotif"); glyph.transform.SetParent(transform, false);
            motif = glyph.AddComponent<LineRenderer>(); motif.sharedMaterial = material; motif.sortingOrder = 1804;
            motif.numCapVertices = 4; motif.numCornerVertices = 4; motif.widthMultiplier = 0.035f;
        }

        private SpriteRenderer Child(string name, Sprite sprite)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            SpriteRenderer view = child.AddComponent<SpriteRenderer>();
            view.sprite = sprite;
            view.sortingOrder = name == "Core" ? 1803 : 1801;
            return view;
        }

        public void Display(ProjectileState state, SpriteRenderer body, Vector2 position, float time)
        {
            if (state.Weapon != null && state.Weapon.Family == ExpandedWeapon.Fish)
                body.transform.rotation = Quaternion.Euler(0, 0, time * 650 + state.Id * 31);
            bool arrow = state.Sprite != null && state.Sprite.name == "ArrowBolt";
            bool magic = !arrow && (state.Element == DamageElement.Arcane || state.Faction == Faction.Enemy || state.Element == DamageElement.Ember);
            halo.enabled = core.enabled = magic;
            float width = state.Faction == Faction.Enemy ? 0.2f : state.VisualWidth > 0 ? state.VisualWidth : 0.22f;
            if (magic)
            {
                float pulse = 1f + Mathf.Sin(time * 18f + state.Id) * 0.08f;
                float local = width / body.transform.localScale.x / halo.sprite.bounds.size.x;
                halo.transform.localScale = Vector3.one * local * 1.85f * pulse;
                core.transform.localScale = Vector3.one * local * 0.45f;
                halo.color = new Color(state.Color.r, state.Color.g, state.Color.b, 0.25f);
                core.color = new Color(1f, 0.96f, 0.89f, 0.9f);
            }
            streak.enabled = arrow || magic;
            streak.widthMultiplier = arrow ? 0.024f : 0.065f;
            streak.startColor = new Color(state.Color.r, state.Color.g, state.Color.b, 0.03f);
            streak.endColor = new Color(state.Color.r, state.Color.g, state.Color.b, 0.65f);
            streak.SetPosition(0, position - state.Velocity.normalized * (arrow ? 0.5f : 0.24f));
            streak.SetPosition(1, position);
            ExpandedWeapon family = state.Weapon == null ? ExpandedWeapon.None : state.Weapon.Family;
            motif.enabled = family == ExpandedWeapon.MusicBox || family == ExpandedWeapon.Fish;
            if (motif.enabled)
            {
                Vector2 direction = state.Velocity.normalized; Vector2 normal = new Vector2(-direction.y, direction.x);
                motif.startColor = motif.endColor = family == ExpandedWeapon.Fish ? new Color(0.3f, 1, 0.8f) : new Color(1, 0.86f, 0.45f);
                motif.loop = family == ExpandedWeapon.Fish; motif.positionCount = 5;
                Vector2[] points = family == ExpandedWeapon.Fish ? FishMotif : NoteMotif;
                for (int index = 0; index < points.Length; index++) motif.SetPosition(index, position + direction * points[index].x + normal * points[index].y);
            }
        }

        private void OnDestroy() { if (material != null) Destroy(material); }
        public void SetFade(float alpha)
        {
            if (halo == null) return;
            Color haloColor = halo.color; haloColor.a *= alpha; halo.color = haloColor;
            Color coreColor = core.color; coreColor.a *= alpha; core.color = coreColor;
            Color start = streak.startColor; start.a *= alpha; streak.startColor = start;
            Color end = streak.endColor; end.a *= alpha; streak.endColor = end;
            Color glyph = motif.startColor; glyph.a *= alpha; motif.startColor = motif.endColor = glyph;
        }
        public void Hide() { if (halo != null) { halo.enabled = core.enabled = streak.enabled = motif.enabled = false; } }
    }
}
