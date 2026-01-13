using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class PaintedEffects : MonoBehaviour
    {
        private sealed class Burst { public SpriteRenderer View; public Sprite[] Frames; public float Age = 10; public float Duration; public float Width; public Color Tint; }
        private readonly List<Burst> pool = new List<Burst>();
        private PaintedAnimationLibrary library;
        private void Awake() { library = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary"); }
        public bool Play(CombatEvent value)
        {
            if (library == null) return false;
            Sprite[] frames; float width; float duration; Vector2 position = value.Position;
            bool slash = value.Kind == CombatEventKind.Fire && value.Weapon != null && value.Weapon.Kind == WeaponKind.Melee;
            bool magic = value.Kind == CombatEventKind.Fire && value.Weapon != null && value.Weapon.Kind == WeaponKind.Projectile && value.Weapon.Motion != WeaponMotion.Bow;
            if (value.Kind == CombatEventKind.BossLanded) { frames = library.Dust; width = value.Subject.Definition.BodyHeight * 1.5f; duration = 0.42f; position += Vector2.down * value.Subject.Definition.BodyHeight * 0.5f; }
            else if (slash) { frames = library.Slash; width = Mathf.Clamp(value.Weapon.Range * 0.85f, 0.6f, 3.4f); duration = 0.22f; position += value.Direction * value.Weapon.Range * 0.4f; }
            else if (magic) { frames = library.Magic; width = 0.48f; duration = 0.18f; position += value.Direction * 0.65f; }
            else if (value.Kind == CombatEventKind.Kill && value.Faction == Faction.Enemy) { frames = library.Death; width = value.Subject != null && value.Subject.Definition.IsBoss ? 1.4f : 0.7f; duration = 0.34f; }
            else if (value.Kind == CombatEventKind.SignaturePulse || value.Kind == CombatEventKind.SignatureBurst || value.Kind == CombatEventKind.ChestOpened || value.Kind == CombatEventKind.Heal || value.Kind == CombatEventKind.Proc)
            { frames = library.Magic; width = Mathf.Clamp(value.Value * 0.25f, 0.55f, 1.2f); duration = 0.3f; }
            else return false;
            Burst burst = pool.Find(candidate => candidate.Age >= candidate.Duration);
            if (burst == null)
            {
                if (pool.Count >= 96) return true;
                var child = new GameObject("PaintedBurst"); child.transform.SetParent(transform, false);
                burst = new Burst { View = child.AddComponent<SpriteRenderer>() }; burst.View.sortingOrder = 1850; pool.Add(burst);
            }
            burst.Age = 0; burst.Duration = duration; burst.Frames = frames; burst.Width = width;
            burst.Tint = slash || magic || value.Kind == CombatEventKind.SignaturePulse ? Color.Lerp(Color.white, ArcadeEffects.ElementColor(value.Element), 0.3f) : Color.white;
            burst.View.sprite = frames[0]; burst.View.enabled = true; burst.View.color = Color.white;
            burst.View.transform.position = position;
            burst.View.transform.rotation = Quaternion.Euler(0, 0, slash ? Mathf.Atan2(value.Direction.y, value.Direction.x) * Mathf.Rad2Deg : 0);
            return true;
        }
        public void Advance(float delta)
        {
            foreach (Burst burst in pool)
            {
                if (burst.Age >= burst.Duration) continue;
                burst.Age += delta; float progress = Mathf.Clamp01(burst.Age / burst.Duration);
                burst.View.sprite = burst.Frames[Mathf.Min(3, Mathf.FloorToInt(progress * 4))];
                float scale = burst.Width / burst.View.sprite.bounds.size.x;
                burst.View.transform.localScale = Vector3.one * scale * (PresentationSettings.ReducedMotion ? 1 : 0.9f + progress * 0.18f);
                Color tint = burst.Tint; tint.a = Mathf.Clamp01((1 - progress) * 3); burst.View.color = tint;
                if (progress >= 1) burst.View.enabled = false;
            }
        }
        public void Clear() { foreach (Burst burst in pool) { burst.Age = burst.Duration; burst.View.enabled = false; } }
    }
}
