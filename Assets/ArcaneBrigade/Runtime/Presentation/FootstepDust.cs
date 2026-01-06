using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class FootstepDust : MonoBehaviour
    {
        private sealed class Puff { public SpriteRenderer View; public Vector2 Origin; public float Age = 1; }
        private readonly List<Puff> pool = new List<Puff>();
        private readonly Dictionary<int, float> nextStep = new Dictionary<int, float>();
        private Sprite sprite;
        private Sprite[] frames;
        public void Configure(Sprite source)
        {
            PaintedAnimationLibrary library = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            frames = library != null ? library.Dust : null; sprite = frames != null ? frames[0] : source;
        }
        public void Display(CombatSimulation simulation, float delta)
        {
            if (sprite == null) return;
            foreach (Puff puff in pool)
            {
                if (puff.Age >= 0.4f) continue;
                puff.Age += delta; float progress = Mathf.Clamp01(puff.Age / 0.4f);
                if (frames != null) puff.View.sprite = frames[Mathf.Min(3, Mathf.FloorToInt(progress * 4))];
                puff.View.transform.position = puff.Origin + Vector2.up * progress * 0.12f;
                puff.View.transform.localScale = new Vector3(0.14f + progress * 0.22f, 0.08f + progress * 0.09f, 1) / sprite.bounds.size.x;
                puff.View.color = new Color(0.78f, 0.75f, 0.68f, (1 - progress) * 0.22f);
                if (progress >= 1) puff.View.enabled = false;
            }
            if (sprite == null || PresentationSettings.ReducedMotion || simulation.Phase == RunPhase.Paused) return;
            foreach (UnitState unit in simulation.Units)
            {
                if (!unit.Alive || unit.Faction == Faction.Enemy || (unit.Position - unit.PreviousPosition).sqrMagnitude < 0.00002f) continue;
                if (nextStep.TryGetValue(unit.Id, out float at) && at > Time.time) continue;
                nextStep[unit.Id] = Time.time + 0.22f;
                Puff puff = pool.Find(candidate => candidate.Age >= 0.4f);
                if (puff == null)
                {
                    if (pool.Count >= 96) continue;
                    var child = new GameObject("FootDust"); child.transform.SetParent(transform, false);
                    puff = new Puff { View = child.AddComponent<SpriteRenderer>() }; puff.View.sprite = sprite; puff.View.sortingOrder = 480; pool.Add(puff);
                }
                puff.Age = 0; puff.Origin = unit.Position - Vector2.up * unit.Definition.BodyHeight * 0.5f;
                puff.View.enabled = true;
            }
        }
        public void Clear() { nextStep.Clear(); foreach (Puff puff in pool) { puff.Age = 1; puff.View.enabled = false; } }
    }
}
