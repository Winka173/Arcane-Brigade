using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ProjectileVisual : MonoBehaviour
    {
        private SpriteRenderer halo;
        private SpriteRenderer core;
        private LineRenderer streak;
        private Material material;

        public void Configure(Sprite disc)
        {
            if (halo != null) return;
            halo = Child("Halo", disc);
            core = Child("Core", disc);
            var child = new GameObject("MotionStreak");
            child.transform.SetParent(transform, false);
            streak = child.AddComponent<LineRenderer>();
            streak.positionCount = 2;
            material = new Material(Shader.Find("Sprites/Default"));
            streak.sharedMaterial = material;
            streak.sortingOrder = 1800;
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
            bool arrow = state.Sprite != null && state.Sprite.name == "ArrowBolt";
            bool magic = !arrow && (state.Element == DamageElement.Arcane || state.Faction == Faction.Enemy || state.Element == DamageElement.Ember);
            halo.enabled = core.enabled = magic;
            float width = state.Faction == Faction.Enemy ? 0.2f : state.VisualWidth > 0 ? state.VisualWidth : 0.22f;
            if (magic)
            {
                float pulse = 1f + Mathf.Sin(time * 18f + state.Id) * 0.08f;
                float local = width / body.transform.localScale.x / halo.sprite.bounds.size.x;
                halo.transform.localScale = Vector3.one * local * 2.1f * pulse;
                core.transform.localScale = Vector3.one * local * 0.45f;
                halo.color = new Color(state.Color.r, state.Color.g, state.Color.b, 0.18f);
                core.color = new Color(1f, 0.96f, 0.89f, 0.9f);
            }
            streak.enabled = arrow || magic;
            streak.widthMultiplier = arrow ? 0.024f : 0.065f;
            streak.startColor = new Color(state.Color.r, state.Color.g, state.Color.b, 0.03f);
            streak.endColor = new Color(state.Color.r, state.Color.g, state.Color.b, 0.65f);
            streak.SetPosition(0, position - state.Velocity.normalized * (arrow ? 0.5f : 0.24f));
            streak.SetPosition(1, position);
        }

        private void OnDestroy() { if (material != null) Destroy(material); }
        public void SetFade(float alpha)
        {
            if (halo == null) return;
            Color haloColor = halo.color; haloColor.a *= alpha; halo.color = haloColor;
            Color coreColor = core.color; coreColor.a *= alpha; core.color = coreColor;
            Color start = streak.startColor; start.a *= alpha; streak.startColor = start;
            Color end = streak.endColor; end.a *= alpha; streak.endColor = end;
        }
        public void Hide() { if (halo != null) { halo.enabled = core.enabled = streak.enabled = false; } }
    }
}
