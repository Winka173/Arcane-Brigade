using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class PaintedEffects : MonoBehaviour
    {
        private sealed class Burst { public SpriteRenderer View; public Sprite[] Frames; public float Age = 10; public float Duration; public float Width; public Color Tint; public bool Important; }
        private readonly List<Burst> pool = new List<Burst>();
        private PaintedAnimationLibrary library;
        private ImpactAnimationLibrary impacts;
        private readonly Dictionary<int, float> nextImpact = new Dictionary<int, float>();
        private float clock;
        public int ActiveCount { get { int count = 0; foreach (Burst burst in pool) if (burst.Age < burst.Duration) count++; return count; } }
        private void Awake() { library = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary"); impacts = Resources.Load<ImpactAnimationLibrary>("ImpactAnimationLibrary"); }
        public bool Play(CombatEvent value)
        {
            if (library == null) return false;
            Sprite[] frames; float width; float duration; Vector2 position = value.Position;
            bool slash = value.Kind == CombatEventKind.Fire && value.Weapon != null && value.Weapon.Kind == WeaponKind.Melee && value.Weapon.Family != ExpandedWeapon.Fish;
            bool magic = value.Kind == CombatEventKind.Fire && value.Weapon != null && value.Weapon.Kind == WeaponKind.Projectile && value.Weapon.Motion != WeaponMotion.Bow;
            bool hit = value.Kind == CombatEventKind.Hit && value.Value > 0 && value.Faction != Faction.Companion;
            // kills are never thinned out by crowd budgets: what landed always shows
            bool important = value.Critical || value.Faction == Faction.Champion || value.Subject != null && value.Subject.Definition.IsBoss || value.Kind == CombatEventKind.Kill && value.Faction == Faction.Enemy;
            // the hostile side's painted fire (the hero's hurt splash, enemy muzzle flames) is retired (owner 22/10/2026): the hurt is
            // drawn in code by CombatFeedback, a hostile shot's launch by its own code-drawn bolt
            if (hit && value.Faction == Faction.Champion || value.Kind == CombatEventKind.Fire && value.Weapon != null && value.Weapon.Kind == WeaponKind.Projectile && value.Faction == Faction.Enemy) return true;
            string combatArt = slash && value.Weapon.Motion == WeaponMotion.Thrust ? "Thrust" : null;
            Sprite[] combatFrames = CombatVfxArtwork.Frames(combatArt);
            bool illustrated = combatFrames != null && combatFrames.Length > 0;
            if (illustrated)
            {
                frames = combatFrames;
                width = combatArt == "Thrust" ? Mathf.Clamp(value.Weapon.Range * 0.85f, 0.6f, 3.4f) : combatArt == "HeroHurt" ? 0.85f : 0.55f;
                duration = combatArt == "HeroHurt" ? 0.28f : 0.22f;
                if (combatArt != "HeroHurt") position += value.Direction * (combatArt == "Thrust" ? value.Weapon.Range * 0.4f : 0.4f);
            }
            else if (hit && impacts != null)
            {
                if (!important && nextImpact.TryGetValue(value.SubjectId, out float until) && clock < until) return true;
                if (nextImpact.Count > 128) nextImpact.Clear(); nextImpact[value.SubjectId] = clock + 0.075f;
                frames = impacts.For(value.Weapon); width = value.Critical ? 1.0f : 0.6f; duration = value.Critical ? 0.26f : 0.19f;
                if (value.Weapon != null && (value.Weapon.Motion == WeaponMotion.Hammer || value.Weapon.Motion == WeaponMotion.Slam || value.Weapon.Mechanic == CarnivalWeapon.SpringHammer)) { width *= 1.45f; duration = 0.28f; }
            }
            else if (value.Kind == CombatEventKind.BossLanded) { frames = library.Dust; width = value.Subject.Definition.BodyHeight * 1.5f; duration = 0.42f; position += Vector2.down * value.Subject.Definition.BodyHeight * 0.5f; }
            else if (slash)
            {
                bool slam = value.Weapon.Motion == WeaponMotion.Slam || value.Weapon.Motion == WeaponMotion.Hammer;
                frames = slam ? library.Dust : library.Slash; width = Mathf.Clamp(value.Weapon.Range * 0.85f, 0.6f, 3.4f); duration = slam ? 0.3f : 0.22f;
                position += value.Direction * value.Weapon.Range * 0.4f;
            }
            else if (magic) { frames = library.Magic; width = 0.48f; duration = 0.18f; position += value.Direction * 0.65f; }
            // ordinary kills are drawn by ExplosionFeedback.Pop (flash, ink burst, chips) around the squashing body; a
            // second puff here would only stack another sprite on the same spot, so the event is consumed
            else if (value.Kind == CombatEventKind.Kill && value.Faction == Faction.Enemy && (value.Subject == null || !value.Subject.Definition.IsBoss)) return true;
            else if (value.Kind == CombatEventKind.Kill && value.Faction == Faction.Enemy) return true; // a boss's end is drawn by BossFinaleVisual; the painted death smoke is retired (owner 22/10/2026)
            else if (value.Kind == CombatEventKind.SignaturePulse || value.Kind == CombatEventKind.SignatureBurst || value.Kind == CombatEventKind.ChestOpened || value.Kind == CombatEventKind.Heal || value.Kind == CombatEventKind.Proc)
            { frames = library.Magic; width = Mathf.Clamp(value.Value * 0.25f, 0.55f, 1.2f); duration = 0.3f; }
            else return false;
            if (frames == null || frames.Length == 0) return false;
            bool atCapacity = ActiveCount >= EffectDensity.Limit(64);
            if (atCapacity && !important) return true;
            Burst burst = atCapacity ? pool.Find(candidate => !candidate.Important && candidate.Age < candidate.Duration) : pool.Find(candidate => candidate.Age >= candidate.Duration);
            if (atCapacity && burst == null) return true;
            if (burst == null && pool.Count >= EffectDensity.Limit(64))
            {
                if (!important) return true;
                burst = pool.Find(candidate => !candidate.Important);
                if (burst == null) return true;
            }
            if (burst == null)
            {
                var child = new GameObject("PaintedBurst"); child.transform.SetParent(transform, false);
                burst = new Burst { View = child.AddComponent<SpriteRenderer>() }; pool.Add(burst);
            }
            burst.Important = important; burst.View.sortingOrder = value.Faction == Faction.Enemy && value.Kind == CombatEventKind.Fire ? 1850 : 1450;
            burst.Age = 0; burst.Duration = duration; burst.Frames = frames; burst.Width = width;
            burst.Tint = !illustrated && (slash || magic || value.Kind == CombatEventKind.SignaturePulse) ? Color.Lerp(Color.white, ArcadeEffects.ElementColor(value.Element), 0.3f) : Color.white;
            if (PresentationSettings.ReducedFlashes) burst.Tint *= new Color(0.8f, 0.8f, 0.8f, 1);
            if (!important) { Color quiet = burst.Tint; quiet.a = EffectDensity.AlliedAlpha; burst.Tint = quiet; }
            burst.View.sprite = frames[0]; burst.View.enabled = true; burst.View.color = Color.white;
            burst.View.transform.position = position;
            burst.View.transform.rotation = Quaternion.Euler(0, 0, slash || combatArt == "HostileMuzzle" ? Mathf.Atan2(value.Direction.y, value.Direction.x) * Mathf.Rad2Deg : 0);
            return true;
        }
        public void Advance(float delta)
        {
            if (delta <= 0 || !float.IsFinite(delta)) return;
            clock += delta;
            foreach (Burst burst in pool)
            {
                if (burst.Age >= burst.Duration) continue;
                burst.Age += delta; float progress = Mathf.Clamp01(burst.Age / burst.Duration);
                burst.View.sprite = burst.Frames[Mathf.Min(burst.Frames.Length - 1, Mathf.FloorToInt(progress * burst.Frames.Length))];
                float scale = burst.Width / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(burst.View.sprite).width);
                burst.View.transform.localScale = Vector3.one * scale * (PresentationSettings.ReducedMotion ? 1 : 0.9f + progress * 0.18f);
                Color tint = burst.Tint; tint.a *= Mathf.Clamp01((1 - progress) * 3); burst.View.color = tint;
                if (progress >= 1) burst.View.enabled = false;
            }
        }
        public void Clear() { nextImpact.Clear(); clock = 0; foreach (Burst burst in pool) { burst.Age = burst.Duration; burst.View.enabled = false; } }
    }
}
