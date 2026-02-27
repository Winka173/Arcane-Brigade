using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Lightweight interlude acting. Presentation only: no combat simulation or equipment changes.</summary>
    public static class RoomActorPerformance
    {
        public static void Display(SpriteRenderer view, UnitDefinition unit, Vector2 floor, Vector2 movement, float clock, int index, bool pulling = false)
        {
            var motion = ActOneArtwork.Find(view.gameObject.name)?.Motion ?? unit?.AuthoredMotion;
            bool walking = movement.sqrMagnitude > .002f;
            bool reduced = PresentationSettings.ReducedMotion;
            Sprite[] frames = motion == null ? null : walking && !pulling ? motion.Walk : motion.Idle;
            if (frames != null && frames.Length > 0) view.sprite = frames[reduced ? 0 : Mathf.FloorToInt(clock * (walking && !pulling ? 7 : 2) + index) % frames.Length];
            if (Mathf.Abs(movement.x) > .025f) view.flipX = movement.x < 0;
            float gait = reduced ? 0 : Mathf.Sin(clock * (walking ? 11 : 2.1f) + index * 1.7f);
            float lift = walking && !pulling ? Mathf.Abs(gait) * .045f : 0;
            float lean = reduced ? 0 : pulling ? Mathf.Sin(clock * 3.5f + index) * 2 + (floor.x < 0 ? -9 : 9) : walking ? gait * 2.5f : gait * .65f;
            view.transform.rotation = Quaternion.Euler(0, 0, lean);
            WorldSpriteGrounding.Place(view, floor + Vector2.up * lift);
            view.sortingOrder = WorldSpriteGrounding.SortAt(floor.y);
            var shadow = view.GetComponent<GroundShadow>();
            if (shadow == null) { shadow = view.gameObject.AddComponent<GroundShadow>(); shadow.Configure(view.sprite); }
            shadow.SortingBase = view.sortingOrder - 10;
            shadow.Display(view.sprite, floor, Mathf.Min(1.1f,view.bounds.size.x*.8f),lift,.24f);
        }
    }
}
