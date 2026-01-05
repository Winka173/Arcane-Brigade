using UnityEngine;

namespace ArcaneBrigade
{
    [RequireComponent(typeof(Camera))]
    public sealed class ArenaCamera : MonoBehaviour
    {
        private Camera arenaCamera;
        private ArenaController owner;
        private Vector3 velocity;
        private void Awake() { arenaCamera = GetComponent<Camera>(); Refresh(); }
        private void Update() { Refresh(); }
        public void Follow(ArenaController controller) { owner = controller; Snap(); }
        public void Snap()
        {
            if (owner == null || owner.Simulation == null) return;
            Refresh();
            arenaCamera.orthographicSize = owner.Session.State.Phase == SessionPhase.Shop ? owner.Content.RestViewHeight : owner.Content.CombatViewHeight;
            Vector2 target = owner.Session.State.Phase == SessionPhase.Shop ? Vector2.zero : owner.Simulation.Champion.Position;
            Vector2 position = ClampCenter(target, owner.WorldBounds, arenaCamera.orthographicSize, 16f / 9f);
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            velocity = Vector3.zero;
        }
        private void LateUpdate()
        {
            if (owner == null || owner.Simulation == null) return;
            float height = owner.Session.State.Phase == SessionPhase.Shop ? owner.Content.RestViewHeight : owner.Content.CombatViewHeight;
            arenaCamera.orthographicSize = height;
            if (owner.Simulation.Phase == RunPhase.Paused) return;
            Vector2 target = owner.Session.State.Phase == SessionPhase.Shop ? Vector2.zero : owner.VisualChampionPosition;
            Vector2 center = ClampCenter(target, owner.WorldBounds, height, 16f / 9f);
            Vector3 next = Vector3.SmoothDamp(transform.position, new Vector3(center.x, center.y, transform.position.z), ref velocity, 0.16f, Mathf.Infinity, Time.deltaTime);
            Vector2 clamped = ClampCenter(next, owner.WorldBounds, height, 16f / 9f);
            transform.position = new Vector3(clamped.x, clamped.y, next.z);
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
