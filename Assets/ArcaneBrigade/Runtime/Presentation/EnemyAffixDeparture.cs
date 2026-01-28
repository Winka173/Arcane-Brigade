using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class EnemyAffixDeparture : MonoBehaviour
    {
        private static int active;
        private readonly SpriteRenderer[] fragments = new SpriteRenderer[4];
        private ArenaController owner;
        private Vector2 origin;
        private float height;
        private float age;
        private float lastClock;
        private int family;
        private bool registered;
        public static void Play(UnitState unit, SpriteRenderer body)
        {
            int family = EnemyVariantVisual.Family(unit, 0);
            if (family < 0 || active >= 12 || EnemyAffixLibrary.Current == null) return;
            var host = new GameObject("EnemyAffixDeathDissipation");
            EnemyAffixDeparture effect = host.AddComponent<EnemyAffixDeparture>();
            effect.owner = Object.FindAnyObjectByType<ArenaController>();
            if (effect.owner == null) { Object.Destroy(host); return; }
            host.transform.SetParent(effect.owner.transform, false);
            effect.family = family; effect.origin = body.bounds.center; effect.height = unit.Definition.BodyHeight;
            effect.lastClock = effect.owner.PresentationClock;
            effect.registered = true; active++;
            for (int index = 0; index < 4; index++)
            {
                var child = new GameObject("EscapingAffix" + index); child.transform.SetParent(host.transform, false);
                effect.fragments[index] = child.AddComponent<SpriteRenderer>();
                effect.fragments[index].sortingOrder = body.sortingOrder + 3;
            }
            effect.Draw();
        }
        private void Update()
        {
            if (owner == null) { Destroy(gameObject); return; }
            float clock = owner.PresentationClock;
            age += Mathf.Clamp(clock - lastClock, 0, 0.1f); lastClock = clock;
            if (age >= 0.85f) { Destroy(gameObject); return; }
            Draw();
        }
        private void Draw()
        {
            bool reduced = PresentationSettings.ReducedMotion;
            float progress = Mathf.Clamp01(age / 0.85f);
            for (int index = 0; index < fragments.Length; index++)
            {
                SpriteRenderer sprite = fragments[index];
                sprite.enabled = !PresentationSettings.ReducedEffects || index == 0;
                sprite.sprite = EnemyAffixLibrary.Effect(family, age + index * 0.11f, reduced);
                if (sprite.sprite == null) { sprite.enabled = false; continue; }
                float side = (index - 1.5f) * 0.23f;
                sprite.transform.position = origin + (reduced ? Vector2.zero : new Vector2(side * height * progress, height * progress * 0.65f));
                sprite.transform.localScale = Vector3.one * height * 0.45f * (1 - progress * 0.35f) / Mathf.Max(0.01f, sprite.sprite.bounds.size.y);
                sprite.color = new Color(1, 1, 1, (1 - progress) * 0.65f);
            }
        }
        private void OnDestroy() { if (registered) active = Mathf.Max(0, active - 1); }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCount() => active = 0;
    }
}
