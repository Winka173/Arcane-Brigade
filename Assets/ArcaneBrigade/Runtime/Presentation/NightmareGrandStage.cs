using UnityEngine;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(30)]
    public sealed class NightmareGrandStage : MonoBehaviour
    {
        private ArenaController owner;
        private Camera viewCamera;
        private readonly SpriteRenderer[] layers = new SpriteRenderer[4];
        private float reveal;
        private float arrival;
        private bool customer;
        public int VisibleCount { get; private set; }
        public float Grandeur { get; private set; }
        public static float StrengthFor(int wave) => Mathf.SmoothStep(0, 1, Mathf.Clamp01((wave - 7) / 13f));
        public void Configure(ArenaController controller)
        {
            owner = controller; viewCamera = Camera.main;
            Sprite[] art = NightmareAssets.Current.GrandStage;
            for (int index = 0; index < layers.Length; index++)
            {
                var child = new GameObject("DreamArchitecture" + index); child.transform.SetParent(transform, false);
                layers[index] = child.AddComponent<SpriteRenderer>(); layers[index].sortingOrder = -42;
                layers[index].sprite = art.Length > index ? art[index] : null;
            }
        }
        private void LateUpdate()
        {
            if (owner == null || owner.Session == null || owner.Simulation == null || viewCamera == null || owner.PresentationPaused) return;
            RunSnapshot state = owner.Session.State;
            bool finale = owner.Simulation.Boss?.Definition.Id == NightmareRules.CustomerId;
            if (finale && !customer) arrival = owner.PresentationClock;
            customer = finale;
            bool dawn = state.Nightmare.CustomerDefeated && !state.Endless;
            float target = owner.Session.SecretRoomOpen || dawn || state.Phase == SessionPhase.Defeat ? 0 : StrengthFor(state.Wave);
            reveal = Mathf.MoveTowards(reveal, target, PresentationSettings.ReducedMotion ? 1 : Time.deltaTime * .65f);
            Grandeur = reveal;
            Vector2 centre = (Vector2)viewCamera.transform.position * .82f;
            float halfHeight = viewCamera.orthographicSize;
            float halfWidth = halfHeight * viewCamera.aspect;
            float age = Mathf.Max(0, owner.PresentationClock - arrival);
            float opening = !finale || PresentationSettings.ReducedMotion ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 2.4f));
            float motion = PresentationSettings.ReducedMotion || NightmareHaunting.Quiet ? 0 : Mathf.Sin(owner.PresentationClock * .37f) * .035f;
            StagePerformance acting = StagePerformance.Active;
            int act = finale && acting != null ? acting.BossAct : 0;
            float impact = acting != null ? acting.Impact : 0;
            float gesture = finale && acting != null ? acting.ActGesture : 0;
            float alpha = reveal > .04f ? 1 : 0;
            VisibleCount = 0;
            for (int index = 0; index < layers.Length; index++)
            {
                SpriteRenderer layer = layers[index]; layer.enabled = layer.sprite != null && alpha > .005f;
                if (!layer.enabled) continue;
                Vector2 size = layer.sprite.bounds.size;
                float width = index == 0 ? halfWidth * 1.45f : index == 3 ? halfWidth * 1.9f : halfWidth * .27f;
                float scale = width / Mathf.Max(.01f, size.x);
                if (index == 0) scale = Mathf.Min(scale, halfHeight * 1.08f / Mathf.Max(.01f, size.y));
                if (index == 1 || index == 2) scale = Mathf.Min(scale, halfHeight * 1.65f / Mathf.Max(.01f, size.y));
                if (index == 3) scale = Mathf.Min(scale, halfHeight * .32f / Mathf.Max(.01f, size.y));
                Vector2 offset = index == 0 ? new Vector2(0, halfHeight * .13f + (1 - opening) * 2) : index == 3 ? new Vector2(0, -halfHeight * 1.03f) : new Vector2((index == 1 ? -1 : 1) * (halfWidth * .9f + (1 - opening) * 2), -halfHeight * .8f);
                layer.transform.position = centre + offset + Vector2.up * motion * (index % 2 == 0 ? 1 : -1);
                if (index == 0) layer.transform.position = centre + new Vector2(0, halfHeight * .62f + (1 - reveal) * 3);
                layer.transform.position += new Vector3((index == 1 ? -1 : index == 2 ? 1 : 0) * (act * .24f + gesture * .16f), impact * .08f, 0);
                layer.transform.localScale = Vector3.one * scale;
                layer.transform.localRotation = Quaternion.Euler(0, 0, motion * (index == 1 ? 6 : index == 2 ? -6 : 0));
                layer.transform.localRotation *= Quaternion.Euler(0, 0, (index == 1 ? -1 : 1) * impact * 2);
                layer.color = Color.Lerp(new Color(.64f, .6f, .74f, alpha), new Color(.9f, .75f, .7f, alpha), finale ? opening : 0);
                VisibleCount++;
            }
        }
    }
}
