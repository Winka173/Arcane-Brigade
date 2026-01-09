using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class FootstepDust : MonoBehaviour
    {
        private sealed class Puff { public SpriteRenderer View; public LineRenderer Streak; public Vector2 Origin; public Vector2 Direction; public bool Pursuit; public float Age = 1; }
        private readonly List<Puff> pool = new List<Puff>();
        private readonly Dictionary<int, float> nextStep = new Dictionary<int, float>();
        private Sprite sprite;
        private Sprite[] frames;
        private Material streakMaterial;
        public int ActivePursuitPuffs => pool.FindAll(puff => puff.Age < 0.4f && puff.Pursuit).Count;
        public int ActivePuffs => pool.FindAll(puff => puff.Age < 0.4f).Count;
        public void Configure(Sprite source)
        {
            PaintedAnimationLibrary library = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            frames = library != null && library.Dust != null && library.Dust.Length > 0 ? library.Dust : null; sprite = frames != null ? frames[0] : source;
            if (streakMaterial == null) streakMaterial = new Material(Shader.Find("Sprites/Default"));
        }
        public void Display(CombatSimulation simulation, float delta)
        {
            if (sprite == null || simulation == null || simulation.Phase == RunPhase.Paused) return;
            if (PresentationSettings.ReducedMotion) { Clear(); return; }
            delta = Mathf.Max(0, delta);
            foreach (Puff puff in pool)
            {
                if (puff.Age >= 0.4f) continue;
                puff.Age += delta; float progress = Mathf.Clamp01(puff.Age / 0.4f);
                if (frames != null) puff.View.sprite = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(progress * frames.Length))];
                puff.View.transform.position = puff.Origin + Vector2.up * progress * 0.12f - puff.Direction * progress * (puff.Pursuit ? 0.25f : 0.06f);
                puff.View.transform.localScale = new Vector3(0.14f + progress * 0.22f, 0.08f + progress * 0.09f, 1) * (puff.Pursuit ? 1.65f : 1) / sprite.bounds.size.x;
                puff.View.color = new Color(0.78f, 0.75f, 0.68f, (1 - progress) * (puff.Pursuit ? 0.42f : 0.22f));
                if (puff.Streak.enabled)
                {
                    Vector3 head = (Vector3)puff.Origin + Vector3.up * 0.13f;
                    puff.Streak.SetPosition(0, head); puff.Streak.SetPosition(1, head - (Vector3)puff.Direction * (0.3f + progress * 0.2f));
                    puff.Streak.startColor = new Color(0.9f, 0.87f, 0.74f, (1 - progress) * 0.3f);
                    puff.Streak.endColor = new Color(0.9f, 0.87f, 0.74f, 0);
                }
                if (progress >= 1) { puff.View.enabled = false; puff.Streak.enabled = false; }
            }
            if (delta <= 0 || (simulation.Phase != RunPhase.Combat && simulation.Phase != RunPhase.Rest)) return;
            foreach (UnitState unit in simulation.Units)
            {
                if (!unit.Alive || unit.Faction == Faction.Enemy || (unit.Position - unit.PreviousPosition).sqrMagnitude < 0.00002f) continue;
                if (nextStep.TryGetValue(unit.Id, out float at) && at > simulation.VisualElapsed) continue;
                bool pursuit = unit.Faction == Faction.Companion && unit.TravelMode == CompanionTravelMode.Pursuit;
                nextStep[unit.Id] = simulation.VisualElapsed + (pursuit ? 0.12f : unit.Faction == Faction.Companion ? 0.3f : 0.22f);
                Puff puff = pool.Find(candidate => candidate.Age >= 0.4f);
                if (puff == null)
                {
                    if (pool.Count >= 96) continue;
                    var child = new GameObject("FootDust"); child.transform.SetParent(transform, false);
                    puff = new Puff { View = child.AddComponent<SpriteRenderer>(), Streak = child.AddComponent<LineRenderer>() }; pool.Add(puff);
                    puff.Streak.sharedMaterial = streakMaterial; puff.Streak.positionCount = 2; puff.Streak.useWorldSpace = true;
                    puff.Streak.startWidth = 0.025f; puff.Streak.endWidth = 0.005f;
                }
                puff.Age = 0; puff.Origin = unit.Position - Vector2.up * K8Motion.BodyHeight(unit) * 0.5f;
                puff.Pursuit = pursuit; puff.Direction = (unit.Position - unit.PreviousPosition).normalized;
                puff.Streak.enabled = pursuit; puff.Streak.sortingOrder = 989 - Mathf.RoundToInt(unit.Position.y * 12);
                Vector2 head = puff.Origin + Vector2.up * 0.13f;
                puff.Streak.SetPosition(0, head); puff.Streak.SetPosition(1, head - puff.Direction * 0.3f);
                puff.Streak.startColor = new Color(0.9f, 0.87f, 0.74f, 0.3f); puff.Streak.endColor = new Color(0.9f, 0.87f, 0.74f, 0);
                puff.View.sprite = sprite;
                puff.View.sortingOrder = 990 - Mathf.RoundToInt(unit.Position.y * 12);
                puff.View.transform.position = puff.Origin;
                puff.View.transform.localScale = new Vector3(0.14f, 0.08f, 1) / Mathf.Max(0.01f, sprite.bounds.size.x);
                puff.View.color = new Color(0.78f, 0.75f, 0.68f, 0.22f);
                puff.View.enabled = true;
            }
        }
        public void Clear() { nextStep.Clear(); foreach (Puff puff in pool) { puff.Age = 1; puff.View.enabled = false; puff.Streak.enabled = false; } }
        private void OnDestroy() { if (streakMaterial != null) Destroy(streakMaterial); }
    }
}
