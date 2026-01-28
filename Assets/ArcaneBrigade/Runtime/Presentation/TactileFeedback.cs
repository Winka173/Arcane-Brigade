using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class TactileFeedback : MonoBehaviour
    {
        private sealed class Accent
        {
            public SpriteRenderer View;
            public Sprite[] Frames;
            public Vector2 Position;
            public Vector2 Drift;
            public float Age = 10;
            public float Life;
            public float Width;
            public float ReferenceWidth;
            public float Rotation;
            public Color Color;
        }
        private readonly List<Accent> pool = new List<Accent>();
        private readonly Dictionary<int, float> nextHit = new Dictionary<int, float>();
        private TactileAnimationLibrary art;
        private float clock;
        private float nextCosmetic;
        private ChestOpeningPerformance chests;
        public int ActiveCount { get; private set; }
        public int PoolCount => pool.Count;
        public void Configure()
        {
            art = TactileAnimationLibrary.Current;
            if (chests == null) chests = gameObject.AddComponent<ChestOpeningPerformance>();
        }
        public void Emit(Sprite[] frames, Vector2 position, float width, float life, Color color, Vector2 drift = default, float rotation = 0, int order = 1510)
        {
            if (frames == null || frames.Length == 0 || PresentationSettings.ReducedMotion || PresentationSettings.ReducedEffects || PresentationSettings.HighReadability) return;
            Accent accent = pool.Find(candidate => candidate.Age >= candidate.Life);
            if (accent == null)
            {
                if (pool.Count >= EffectDensity.Limit(48)) return;
                var child = new GameObject("TactilePaintedAccent"); child.transform.SetParent(transform, false);
                accent = new Accent { View = child.AddComponent<SpriteRenderer>() }; pool.Add(accent);
            }
            accent.ReferenceWidth = .01f;
            foreach (Sprite frame in frames) if (frame != null) accent.ReferenceWidth = Mathf.Max(accent.ReferenceWidth, frame.bounds.size.x);
            accent.Frames = frames; accent.Position = position; accent.Drift = drift;
            accent.Age = 0; accent.Life = Mathf.Max(.05f, life); accent.Width = Mathf.Clamp(width, .08f, 2.5f);
            accent.Color = color; accent.Rotation = rotation; accent.View.sortingOrder = order;
            Draw(accent);
        }
        public void Play(CombatEvent value)
        {
            if (art == null) return;
            bool heavy = value.Weapon != null && (value.Weapon.Motion == WeaponMotion.Hammer || value.Weapon.Motion == WeaponMotion.Slam || value.Weapon.Motion == WeaponMotion.Stamp);
            if (value.Kind == CombatEventKind.Hit && value.Value > 0 && value.Subject != null)
            {
                if (nextHit.TryGetValue(value.SubjectId, out float next) && next > clock || clock < nextCosmetic) return;
                nextHit[value.SubjectId] = clock + .17f; nextCosmetic = clock + .018f;
                BodyMaterial material = value.Subject.Definition.Material;
                Color tint = material == BodyMaterial.Glass ? new Color(.65f, .92f, 1, .75f) : Color.white;
                Emit(art.For(material), value.Position, heavy ? .72f : value.Critical ? .58f : .38f, .32f, tint, value.Direction.normalized * .16f);
                if (heavy) Emit(art.Dust, value.Position - Vector2.up * .25f, 1, .5f, new Color(.6f, .55f, .5f, .35f), Vector2.zero, 0, 495);
            }
            else if (value.Kind == CombatEventKind.ChestOpened)
            {
                if (chests != null) chests.Play(value.Position);
                Emit(art.Paper, value.Position, 1.2f, .65f, new Color(1, .85f, .48f, .8f), Vector2.up * .2f);
                Emit(art.Dust, value.Position, 1, .5f, new Color(.7f, .61f, .49f, .45f), Vector2.zero, 0, 495);
            }
            else if (value.Kind == CombatEventKind.Heal && value.Value > 0)
                Emit(art.Paper, value.Position, .5f, .6f, new Color(.45f, 1, .76f, .55f), Vector2.up * .45f);
            else if (value.Kind == CombatEventKind.ProjectilePierce || value.Kind == CombatEventKind.ProjectileBounce || value.Kind == CombatEventKind.ProjectileReturn)
            {
                if (clock < nextCosmetic) return; nextCosmetic = clock + .04f;
                Emit(art.Metal, value.Position, .3f, .22f, new Color(.75f, .88f, 1, .6f), value.Direction.normalized * .1f);
            }
            else if (value.Kind == CombatEventKind.ProjectileSplit)
            {
                if (clock < nextCosmetic) return; nextCosmetic = clock + .04f;
                Emit(art.Paper, value.Position, .38f, .25f, new Color(.78f, .65f, 1, .55f));
            }
            else if (value.Kind == CombatEventKind.BossLanded || value.Kind == CombatEventKind.BossPhase)
                Emit(art.Dust, value.Position, 2.4f, .8f, new Color(.7f, .6f, .53f, .45f), Vector2.up * .12f, 0, 495);
            if (nextHit.Count > 512) nextHit.Clear();
        }
        private static void Draw(Accent accent)
        {
            float progress = Mathf.Clamp01(accent.Age / accent.Life);
            Sprite frame = accent.Frames[Mathf.Min(accent.Frames.Length - 1, Mathf.FloorToInt(progress * accent.Frames.Length))];
            if (frame == null) { accent.View.enabled = false; return; }
            accent.View.sprite = frame;
            float scale = accent.Width / accent.ReferenceWidth;
            accent.View.transform.localScale = Vector3.one * scale;
            Quaternion turn = Quaternion.Euler(0, 0, accent.Rotation);
            accent.View.transform.rotation = turn;
            accent.View.transform.position = (Vector3)(accent.Position + accent.Drift * progress) - turn * (frame.bounds.center * scale);
            Color tint = accent.Color; tint.a *= Mathf.Clamp01((1 - progress) * 3) * Mathf.Lerp(1, .5f, EffectDensity.CrowdPressure);
            accent.View.color = tint; accent.View.enabled = progress < 1;
        }
        public void Advance(float delta)
        {
            if (delta <= 0 || !float.IsFinite(delta)) return;
            clock += delta; ActiveCount = 0;
            if (chests != null) chests.Advance(delta);
            foreach (Accent accent in pool)
            {
                if (accent.Age >= accent.Life) continue;
                if (PresentationSettings.ReducedMotion || PresentationSettings.ReducedEffects || PresentationSettings.HighReadability) accent.Age = accent.Life;
                else accent.Age += delta;
                Draw(accent); if (accent.Age < accent.Life) ActiveCount++;
            }
        }
        public void Clear()
        {
            nextHit.Clear(); clock = nextCosmetic = 0; ActiveCount = 0;
            if (chests != null) chests.Clear();
            foreach (Accent accent in pool) { accent.Age = accent.Life; accent.View.enabled = false; }
        }
    }
}
