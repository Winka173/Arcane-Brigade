using UnityEngine;

namespace ArcaneBrigade
{
    [RequireComponent(typeof(Camera))]
    public sealed class ArenaCamera : MonoBehaviour
    {
        private Camera arenaCamera;
        private ArenaController owner;
        private Vector3 velocity;
        private float heightVelocity;
        private Vector3 trackedPosition;
        private float DesiredHeight => InShop ? owner.Content.RestViewHeight : owner.Simulation.Boss?.Definition.Id == NightmareRules.CustomerId ? Mathf.Max(6.5f,owner.Content.CombatViewHeight) : owner.Session.State.OutsideStage == OutsideStageStatus.Fighting ? Mathf.Max(7f, owner.Content.CombatViewHeight) : owner.Content.CombatViewHeight;
        private bool InShop => owner.Session.State.Phase == SessionPhase.Shop && !owner.FightStillShown;
        private void Awake() { arenaCamera = GetComponent<Camera>(); Refresh(); }
        private void Update() { Refresh(); }
        public void Follow(ArenaController controller, bool snap = true) { owner = controller; if (snap) Snap(); }
        public void Snap()
        {
            if (owner == null || owner.Simulation == null) return;
            Refresh();
            arenaCamera.orthographicSize = DesiredHeight;
            Vector2 target = InShop ? Vector2.zero : owner.Simulation.Champion.Position;
            Vector2 position = ClampCenter(target, owner.WorldBounds, arenaCamera.orthographicSize, 16f / 9f);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            trackedPosition = transform.position;
            velocity = Vector3.zero;
            heightVelocity = 0;
        }
        private void LateUpdate()
        {
            if (owner == null || owner.Simulation == null) return;
            float height = DesiredHeight;
            if (owner.Simulation.Phase == RunPhase.Paused) return;
            arenaCamera.orthographicSize = PresentationSettings.ReducedMotion ? height : Mathf.SmoothDamp(arenaCamera.orthographicSize, height, ref heightVelocity, 0.3f, Mathf.Infinity, Time.deltaTime);
            height = arenaCamera.orthographicSize;
            Vector2 target = InShop ? Vector2.zero : owner.VisualChampionPosition + BossLean();
            Vector2 center = ClampCenter(target, owner.WorldBounds, height, 16f / 9f);
            Vector3 next = Vector3.SmoothDamp(trackedPosition, new Vector3(center.x, center.y, transform.position.z), ref velocity, 0.16f, Mathf.Infinity, Time.deltaTime);
            Vector2 clamped = ClampCenter(next, owner.WorldBounds, height, 16f / 9f);
            trackedPosition = new Vector3(clamped.x, clamped.y, next.z);
            Vector2 accented = ClampCenter(clamped + owner.CameraAccent, owner.WorldBounds + Vector2.one * (CombatFeel.MaxShake * 2f), height, 16f / 9f);
            transform.position = new Vector3(accented.x, accented.y, next.z);
        }
        /// <summary>While a boss makes its entrance the view leans toward it (and back over half a second after it lands), so
        /// its arrival is seen even when it comes in at the edge of the screen.</summary>
        private Vector2 BossLean()
        {
            if (owner.BossFallPoint is Vector2 fall && !PresentationSettings.ReducedMotion) return Vector2.ClampMagnitude((fall - owner.Simulation.Champion.Position) * .6f, 3f); // watch it fall
            UnitState boss = owner.Simulation.Boss;
            if (PresentationSettings.ReducedMotion || boss == null || !boss.Alive || owner.Session.State.Phase != SessionPhase.Combat) return Vector2.zero;
            float after = owner.Simulation.Elapsed - boss.EntranceUntil;
            float weight = after < 0 ? 1 : 1 - Mathf.Clamp01(after / .5f);
            return weight <= 0 ? Vector2.zero : Vector2.ClampMagnitude((boss.Position - owner.Simulation.Champion.Position) * .45f, 2.5f) * weight;
        }
        public static Vector2 ClampCenter(Vector2 target, Vector2 bounds, float halfHeight, float aspect)
        {
            Vector2 limit = new Vector2(Mathf.Max(0f, bounds.x * 0.5f - halfHeight * aspect), Mathf.Max(0f, bounds.y * 0.5f - halfHeight));
            return new Vector2(Mathf.Clamp(target.x, -limit.x, limit.x), Mathf.Clamp(target.y, -limit.y, limit.y));
        }
        public void Refresh()
        {
            if (arenaCamera == null) arenaCamera = GetComponent<Camera>();
            float ratio = 16f / 9f;
            float actual = Screen.width / (float)Mathf.Max(1, Screen.height);
            arenaCamera.rect = actual > ratio
                ? new Rect((1f - ratio / actual) * 0.5f, 0f, ratio / actual, 1f)
                : new Rect(0f, (1f - actual / ratio) * 0.5f, 1f, actual / ratio);
            arenaCamera.aspect = ratio;
        }
    }
}
