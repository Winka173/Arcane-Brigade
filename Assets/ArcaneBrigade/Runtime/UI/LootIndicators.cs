using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class LootIndicators
    {
        public sealed class Marker
        {
            public int Key;
            public LootKind Kind;
            public int Count;
            public Vector2 Position;
            public float Distance;
            public float Alpha;
        }
        private readonly Dictionary<int, Marker> shown = new Dictionary<int, Marker>();
        private int lastFrame = -1;
        public static List<Marker> Layout(IReadOnlyList<PickupState> pickups, Rect worldView, Rect safeArea)
        {
            var groups = new Dictionary<int, Marker>();
            foreach (PickupState loot in pickups)
            {
                if (loot.Kind != LootKind.Chest && loot.Kind != LootKind.Egg || worldView.Contains(loot.Position)) continue;
                Vector2 offset = loot.Position - worldView.center;
                Vector2 direction = new Vector2(offset.x / worldView.width, -offset.y / worldView.height).normalized;
                int sector = Mathf.RoundToInt(Mathf.Atan2(direction.y, direction.x) * 4 / Mathf.PI + 8) % 8;
                int key = (int)loot.Kind * 8 + sector;
                if (!groups.TryGetValue(key, out Marker marker))
                {
                    Vector2 half = safeArea.size * 0.5f;
                    float distance = Mathf.Min(Mathf.Abs(direction.x) < 0.001f ? float.MaxValue : half.x / Mathf.Abs(direction.x), Mathf.Abs(direction.y) < 0.001f ? float.MaxValue : half.y / Mathf.Abs(direction.y));
                    marker = new Marker { Key = key, Kind = loot.Kind, Position = safeArea.center + direction * distance, Distance = offset.magnitude };
                    groups.Add(key, marker);
                }
                marker.Count++; marker.Distance = Mathf.Min(marker.Distance, offset.magnitude);
            }
            var result = new List<Marker>(groups.Values);
            result.Sort((first, second) => first.Kind != second.Kind ? (first.Kind == LootKind.Chest ? -1 : 1) : first.Distance != second.Distance ? first.Distance.CompareTo(second.Distance) : first.Key.CompareTo(second.Key));
            if (result.Count > 6) result.RemoveRange(6, result.Count - 6);
            for (int index = 0; index < result.Count; index++)
                for (int previous = 0; previous < index; previous++)
                    if ((result[index].Position - result[previous].Position).sqrMagnitude < 150 * 150) result[index].Position = Vector2.MoveTowards(result[index].Position, safeArea.center, 170);
            return result;
        }
        public void Draw(CombatSimulation simulation, RunCatalog catalog, Camera camera, GUIStyle text)
        {
            Vector2 half = new Vector2(camera.orthographicSize * camera.aspect, camera.orthographicSize);
            Rect world = new Rect((Vector2)camera.transform.position - half, half * 2);
            List<Marker> targets = Layout(simulation.Pickups, world, new Rect(140, 205, 1640, 650));
            if (lastFrame != Time.frameCount)
            {
                lastFrame = Time.frameCount;
                var active = new HashSet<int>();
                foreach (Marker target in targets)
                {
                    active.Add(target.Key);
                    if (!shown.TryGetValue(target.Key, out Marker marker)) { marker = target; shown.Add(target.Key, marker); }
                    marker.Position = target.Position; marker.Count = target.Count; marker.Distance = target.Distance;
                    if (simulation.Phase != RunPhase.Paused) marker.Alpha = Mathf.MoveTowards(marker.Alpha, 1, Time.deltaTime * 7);
                }
                var remove = new List<int>();
                foreach (Marker marker in shown.Values)
                    if (!active.Contains(marker.Key))
                    {
                        if (simulation.Phase != RunPhase.Paused) marker.Alpha = Mathf.MoveTowards(marker.Alpha, 0, Time.deltaTime * 10);
                        if (marker.Alpha <= 0) remove.Add(marker.Key);
                    }
                foreach (int key in remove) shown.Remove(key);
            }
            foreach (Marker marker in shown.Values)
            {
                Color previous = GUI.color;
                GUI.color = new Color(1, 1, 1, marker.Alpha);
                RunHud.Fill(new Rect(marker.Position.x - 70, marker.Position.y - 33, 140, 66), new Color(0.12f, 0.09f, 0.22f, 0.9f));
                RunHud.Sprite(new Rect(marker.Position.x - 55, marker.Position.y - 24, 42, 42), marker.Kind == LootKind.Chest ? catalog.ChestSprite : catalog.EggSprite);
                GUI.Label(new Rect(marker.Position.x - 3, marker.Position.y - 22, 69, 54), "×" + marker.Count + "\n" + Mathf.CeilToInt(marker.Distance) + "m", text);
                GUI.color = previous;
            }
        }
    }
}
