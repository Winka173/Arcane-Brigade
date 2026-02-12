using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>When an enemy carrying an affix pops, its affix flame bursts out of it in a few quick tongues and is gone
    /// (part of the death explosion, not a lingering ghost). Pooled: no objects are created once a dozen exist.</summary>
    public sealed class EnemyAffixDeparture : MonoBehaviour
    {
        public const float Duration = 0.42f;
        private const int Capacity = 12;
        private static readonly List<EnemyAffixDeparture> pool = new List<EnemyAffixDeparture>();
        private readonly SpriteRenderer[] fragments = new SpriteRenderer[4];
        private ArenaController owner;
        private Vector2 origin;
        private float height;
        private float age;
        private float lastClock;
        private float spin;
        private int family;
        private bool playing;
        public static int ActiveCount { get { int count = 0; foreach (EnemyAffixDeparture effect in pool) if (effect != null && effect.playing) count++; return count; } }
        public static void Play(UnitState unit, SpriteRenderer body)
        {
            int family = EnemyVariantVisual.Family(unit, 0);
            if (family < 0 || EnemyAffixLibrary.Current == null || body == null) return;
            pool.RemoveAll(candidate => candidate == null);
            EnemyAffixDeparture effect = pool.Find(candidate => !candidate.playing);
            if (effect == null)
            {
                if (pool.Count >= Capacity) return;
                ArenaController arena = Object.FindAnyObjectByType<ArenaController>();
                if (arena == null) return;
                var host = new GameObject("EnemyAffixDeathBurst");
                host.transform.SetParent(arena.transform, false);
                effect = host.AddComponent<EnemyAffixDeparture>();
                effect.owner = arena;
                for (int index = 0; index < effect.fragments.Length; index++)
                {
                    var child = new GameObject("EscapingAffix" + index); child.transform.SetParent(host.transform, false);
                    effect.fragments[index] = child.AddComponent<SpriteRenderer>();
                }
                pool.Add(effect);
            }
            effect.family = family; effect.origin = body.bounds.center; effect.height = Mathf.Max(0.3f, unit.Definition.BodyHeight);
            effect.lastClock = effect.owner.PresentationClock; effect.age = 0; effect.playing = true;
            effect.spin = unit.Id * 0.77f;
            foreach (SpriteRenderer fragment in effect.fragments) fragment.sortingOrder = body.sortingOrder + 3;
            effect.gameObject.SetActive(true);
            effect.Draw();
        }
        private void Update()
        {
            if (!playing) return;
            if (owner == null) { Stop(); return; }
            float clock = owner.PresentationClock;
            age += Mathf.Clamp(clock - lastClock, 0, 0.1f); lastClock = clock;
            if (age >= Duration) { Stop(); return; }
            Draw();
        }
        private void Stop()
        {
            playing = false;
            foreach (SpriteRenderer fragment in fragments) if (fragment != null) fragment.enabled = false;
            gameObject.SetActive(false);
        }
        private void Draw()
        {
            bool reduced = PresentationSettings.ReducedMotion;
            float progress = Mathf.Clamp01(age / Duration);
            float out_ = 1 - Mathf.Pow(1 - progress, 3);
            for (int index = 0; index < fragments.Length; index++)
            {
                SpriteRenderer sprite = fragments[index];
                sprite.enabled = !PresentationSettings.ReducedEffects || index == 0;
                if (!sprite.enabled) continue;
                sprite.sprite = EnemyAffixLibrary.Effect(family, age + index * 0.11f, reduced);
                if (sprite.sprite == null) { sprite.enabled = false; continue; }
                // four tongues of the affix flame thrown outward from the burst (one straight up, the rest fanned)
                float angle = spin + index * Mathf.PI * 0.5f + 0.4f;
                Vector2 away = index == 0 ? Vector2.up : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.7f + 0.3f);
                sprite.transform.position = origin + (reduced ? Vector2.zero : away * height * 0.55f * out_);
                sprite.transform.rotation = Quaternion.Euler(0, 0, reduced ? 0 : Mathf.Atan2(away.y, away.x) * Mathf.Rad2Deg - 90f);
                float swell = reduced ? 1f : Mathf.Lerp(0.55f, 1.05f, out_);
                sprite.transform.localScale = Vector3.one * height * 0.42f * swell / Mathf.Max(0.01f, sprite.sprite.bounds.size.y);
                sprite.color = new Color(1, 1, 1, (1 - progress * progress) * 0.8f);
            }
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetPool() => pool.Clear();
    }
}
