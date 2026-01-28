using UnityEngine;

namespace ArcaneBrigade
{
    public static class DreamLevitation
    {
        public static Vector3 Offset(float time, float identity, float depth, bool reduced, float strength = 1)
        {
            float phase = identity * 2.399963f;
            float height = (.34f + Mathf.Clamp01(depth / 4) * .16f) * strength;
            if (reduced) return Vector3.up * height;
            return new Vector3(Mathf.Sin(time * .43f + phase) * .075f * strength,
                height + (Mathf.Sin(time * .91f + phase) * .07f + Mathf.Sin(time * .37f + phase * 1.7f) * .025f) * strength, 0);
        }
        public static float Tilt(float time, float identity, bool reduced, float strength = 1)
            => reduced ? 0 : (Mathf.Sin(time * .57f + identity * 2.399963f) * 3.4f + Mathf.Sin(time * .23f + identity) * 1.1f) * strength;
    }
    public sealed class DreamSuspension : MonoBehaviour
    {
        private readonly LineRenderer[] threads = new LineRenderer[3];
        private Material material;
        private SpriteRenderer mist;
        public void Display(Vector3 floor, Vector3 foot, float width, float time, float identity, int order, float opacity, bool warm = false)
        {
            if (material == null)
            {
                material = new Material(Shader.Find("Sprites/Default"));
                for (int index = 0; index < threads.Length; index++)
                {
                    var child = new GameObject("SuspendedDreamThread" + index);
                    child.transform.SetParent(transform, false);
                    LineRenderer thread = child.AddComponent<LineRenderer>();
                    thread.sharedMaterial = material; thread.positionCount = 8; thread.useWorldSpace = true;
                    thread.numCapVertices = 3; thread.startWidth = .012f; thread.endWidth = .035f;
                    threads[index] = thread;
                }
            }
            Color tint = warm ? new Color(.95f, .72f, .39f) : new Color(.55f, .72f, .96f);
            DreamSuspensionArtwork artwork = DreamSuspensionArtwork.Current;
            if (artwork != null && artwork.Frames != null && artwork.Frames.Length == 8)
            {
                if (mist == null)
                {
                    var child = new GameObject("PaintedSuspensionMist"); child.transform.SetParent(transform, false);
                    mist = child.AddComponent<SpriteRenderer>();
                }
                mist.enabled = opacity > .005f && !PresentationSettings.ReducedEffects;
                mist.sprite = artwork.Frames[PresentationSettings.ReducedMotion ? 0 : Mathf.FloorToInt(time * 5 + identity * 1.7f) % 8];
                mist.sortingOrder = order;
                mist.transform.rotation = Quaternion.identity;
                Vector3 parentScale = transform.lossyScale;
                mist.transform.localScale = new Vector3(Mathf.Min(width * .7f, 1.5f) / Mathf.Max(.001f, mist.sprite.bounds.size.x * Mathf.Abs(parentScale.x)), Mathf.Max(.1f, foot.y - floor.y + .12f) / Mathf.Max(.001f, mist.sprite.bounds.size.y * Mathf.Abs(parentScale.y)), 1);
                WorldSpriteGrounding.Place(mist, floor);
                mist.color = new Color(warm ? 1 : .8f, warm ? .88f : .9f, warm ? .68f : 1, opacity * .34f);
            }
            for (int index = 0; index < threads.Length; index++)
            {
                LineRenderer thread = threads[index];
                thread.enabled = opacity > .005f && (!PresentationSettings.ReducedEffects || index == 1);
                if (!thread.enabled) continue;
                thread.sortingOrder = order;
                float phase = identity * 2.4f + index * 2.1f;
                float clock = PresentationSettings.ReducedMotion ? 0 : time;
                float side = (index - 1) * Mathf.Min(width * .21f, .55f);
                for (int vertex = 0; vertex < 8; vertex++)
                {
                    float blend = vertex / 7f;
                    Vector3 position = Vector3.Lerp(floor, foot, blend);
                    position.x += side + Mathf.Sin(blend * 5 + clock * .8f + phase) * .07f * Mathf.Sin(blend * Mathf.PI);
                    thread.SetPosition(vertex, position);
                }
                thread.startColor = new Color(tint.r, tint.g, tint.b, 0);
                thread.endColor = new Color(tint.r, tint.g, tint.b, opacity * .22f);
            }
        }
        public void Hide() { foreach (LineRenderer thread in threads) if (thread != null) thread.enabled = false; if (mist != null) mist.enabled = false; }
        private void OnDisable() => Hide();
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
