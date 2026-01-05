using UnityEngine;

namespace ArcaneBrigade.StylePreview
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PreviewSpriteActor : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameDuration = 0.18f;
        [SerializeField] private float phaseOffset;
        private SpriteRenderer spriteRenderer;

        public void Configure(Sprite[] animationFrames, float offset)
        {
            frames = animationFrames;
            phaseOffset = offset;
            ShowFrame(0);
        }

        public void ShowFrame(int frameIndex)
        {
            if (frames == null || frames.Length == 0)
                return;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            spriteRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        }

        private void Update()
        {
            if (frames == null || frames.Length < 4)
                return;

            float cycleTime = Mathf.Repeat(Time.time + phaseOffset, 3f);
            if (cycleTime < 0.9f)
                ShowFrame(0);
            else if (cycleTime < 2.2f)
                ShowFrame(1 + Mathf.FloorToInt(cycleTime / frameDuration) % 2);
            else
                ShowFrame(3);
        }
    }
}
