using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ChestOpeningPerformance : MonoBehaviour
    {
        private Sprite[] frames;
        private readonly SpriteRenderer[] views = new SpriteRenderer[4];
        private readonly Vector2[] locations = new Vector2[4];
        private readonly float[] ages = { 10, 10, 10, 10 };
        private float scale;
        public void Play(Vector2 position)
        {
            if (PresentationSettings.ReducedMotion || PresentationSettings.ReducedEffects) return;
            if (frames == null)
            {
                frames = TactileAnimationLibrary.Current?.Chests;
                if (frames == null || frames.Length == 0) return;
                scale = .78f / Mathf.Max(.01f, frames[0].bounds.size.x);
            }
            int slot = -1;
            for (int index = 0; index < ages.Length; index++) if (ages[index] >= .7f) { slot = index; break; }
            if (slot < 0) return;
            if (views[slot] == null)
            {
                var child = new GameObject("PaintedChestOpening" + slot); child.transform.SetParent(transform, false);
                views[slot] = child.AddComponent<SpriteRenderer>(); views[slot].sortingOrder = 1710;
            }
            locations[slot] = position - Vector2.up * .25f; ages[slot] = 0; Draw(slot);
        }
        private void Draw(int index)
        {
            float progress = Mathf.Clamp01(ages[index] / .7f);
            float open = Mathf.Clamp01(progress / .65f);
            SpriteRenderer view = views[index]; view.sprite = frames[Mathf.Min(frames.Length - 1, Mathf.FloorToInt(open * frames.Length))];
            float settle = AnimationTiming.Settle(ages[index] - .3f, .3f);
            view.transform.position = locations[index];
            view.transform.localScale = new Vector3(1 + settle * .025f, 1 - settle * .025f, 1) * scale;
            view.color = new Color(1, 1, 1, Mathf.Clamp01((1 - progress) * 3)); view.enabled = progress < 1;
        }
        public void Advance(float delta)
        {
            if (delta <= 0 || !float.IsFinite(delta)) return;
            for (int index = 0; index < ages.Length; index++)
                if (ages[index] < .7f)
                {
                    ages[index] += delta;
                    if (PresentationSettings.ReducedMotion || PresentationSettings.ReducedEffects) ages[index] = .7f;
                    Draw(index);
                }
        }
        public void Clear()
        {
            for (int index = 0; index < ages.Length; index++) { ages[index] = 10; if (views[index] != null) views[index].enabled = false; }
        }
    }
}
