using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class StageTransfer : MonoBehaviour
    {
        private ArenaController owner;
        private Action completed;
        private SpriteRenderer courier;
        private Sprite[] frames;
        private UnitVisual[] passengers;
        private Vector2 origin;
        private float age;
        private bool active;
        private GroundShadow grounded;
        public bool Active => active;
        public Vector2 VisualPlayerPosition => PresentationSettings.ReducedMotion ? origin : PassengerPosition(origin, age);
        public static bool ChangesMap(int wave) => wave > 1 && (wave - 1) % 4 == 0;

        public void Configure(ArenaController controller, Action onComplete)
        {
            owner = controller; completed = onComplete; active = true;
            origin = owner.Simulation.Champion.Position;
            CarnivalStageAssets assets = Resources.Load<CarnivalStageAssets>("CarnivalStageAssets");
            frames = assets != null ? assets.Courier : new Sprite[0];
            passengers = FindObjectsByType<UnitVisual>();
            courier = gameObject.AddComponent<SpriteRenderer>(); courier.sortingOrder = 500;
            grounded = gameObject.AddComponent<GroundShadow>();
            CheckpointAudio.PlayInterfaceId("shop.descend");
        }

        public static Vector2 PassengerPosition(Vector2 start, float seconds)
        {
            Vector2 destination = new Vector2(0, -0.2f);
            Vector2 release = Vector2.Lerp(start, destination, 0.75f) + Vector2.up;
            if (seconds < 0.4f) return start;
            if (seconds < 1.3f)
            {
                float progress = Mathf.SmoothStep(0, 1, (seconds - 0.4f) / 0.9f);
                return Vector2.Lerp(start, release, progress);
            }
            float flight = Mathf.Clamp01((seconds - 1.3f) / 0.9f);
            return Vector2.Lerp(release, destination, flight) + Vector2.up * Mathf.Sin(flight * Mathf.PI) * 1.4f;
        }

        private void Update()
        {
            if (!active || owner == null || owner.PresentationPaused) return;
            age += Time.unscaledDeltaTime;
            bool reduced = PresentationSettings.ReducedMotion;
            if (frames.Length > 0)
            {
                courier.sprite = frames[reduced ? 0 : 2 + Mathf.FloorToInt(age * 7) % 4];
                courier.transform.localScale = Vector3.one * (3.8f / Mathf.Max(0.01f, frames[0].bounds.size.x));
                Vector2 passenger = PassengerPosition(origin, age);
                Vector2 position = age < 0.4f ? Vector2.Lerp(origin + Vector2.left * 8, origin, Mathf.SmoothStep(0, 1, age / 0.4f)) : age < 1.3f ? passenger : Vector2.Lerp(PassengerPosition(origin, 1.3f), new Vector2(9, 4), Mathf.Clamp01((age - 1.3f) / 1.1f));
                transform.position = reduced ? origin + Vector2.up : position + Vector2.down * 0.45f;
                courier.color = new Color(1, 1, 1, Mathf.Clamp01((2.6f - age) * 2));
                grounded.Configure(courier.sprite);
                grounded.Display(courier.sprite, (Vector2)transform.position - Vector2.up * (reduced ? 0 : 1.1f), 3.8f, reduced ? 0 : 1.1f, 0.22f * courier.color.a);
            }
            if (age < (reduced ? 0.6f : 2.6f)) return;
            active = false;
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = new Vector2(0, -0.2f);
            completed?.Invoke(); Destroy(gameObject);
        }

        private void LateUpdate()
        {
            if (!active || PresentationSettings.ReducedMotion) return;
            Vector2 offset = PassengerPosition(origin, age) - origin;
            foreach (UnitVisual passenger in passengers)
                if (passenger != null && passenger.State != null && passenger.State.Alive && passenger.State.Faction != Faction.Enemy)
                {
                    passenger.transform.position = (Vector3)(passenger.State.Position + offset);
                    float lift = age < 0.4f ? 0 : age < 1.3f ? Mathf.SmoothStep(0, 1, (age - 0.4f) / 0.9f) : Mathf.Max(0, 1 - (age - 1.3f) / 0.9f) + Mathf.Sin(Mathf.Clamp01((age - 1.3f) / 0.9f) * Mathf.PI) * 1.4f;
                    passenger.TransportShadow(offset, lift);
                }
        }
    }
}
