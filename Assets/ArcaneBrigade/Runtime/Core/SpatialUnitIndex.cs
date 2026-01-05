using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class SpatialUnitIndex
    {
        private const float CellSize = 2f;
        private readonly Dictionary<Vector2Int, List<UnitState>> cells = new Dictionary<Vector2Int, List<UnitState>>();
        private readonly Stack<List<UnitState>> pool = new Stack<List<UnitState>>();
        private readonly Dictionary<int, Vector2Int> membership = new Dictionary<int, Vector2Int>();

        private static Vector2Int Cell(Vector2 position) => new Vector2Int(Mathf.FloorToInt(position.x / CellSize), Mathf.FloorToInt(position.y / CellSize));

        public void Rebuild(IReadOnlyList<UnitState> units)
        {
            foreach (List<UnitState> bucket in cells.Values) { bucket.Clear(); pool.Push(bucket); }
            cells.Clear();
            membership.Clear();
            foreach (UnitState unit in units) if (unit.Alive) Insert(unit, Cell(unit.Position));
        }

        private void Insert(UnitState unit, Vector2Int cell)
        {
            if (!cells.TryGetValue(cell, out List<UnitState> bucket))
            {
                bucket = pool.Count > 0 ? pool.Pop() : new List<UnitState>();
                cells.Add(cell, bucket);
            }
            bucket.Add(unit);
            membership[unit.Id] = cell;
        }

        public void Move(UnitState unit)
        {
            Vector2Int cell = Cell(unit.Position);
            if (membership.TryGetValue(unit.Id, out Vector2Int old))
            {
                if (cell == old) return;
                cells[old].Remove(unit);
            }
            Insert(unit, cell);
        }

        public void Query(Vector2 minimum, Vector2 maximum, List<UnitState> results)
        {
            results.Clear();
            Vector2Int first = Cell(minimum);
            Vector2Int last = Cell(maximum);
            for (int horizontal = first.x; horizontal <= last.x; horizontal++)
                for (int vertical = first.y; vertical <= last.y; vertical++)
                    if (cells.TryGetValue(new Vector2Int(horizontal, vertical), out List<UnitState> bucket)) results.AddRange(bucket);
            results.Sort(Compare);
        }

        private static int Compare(UnitState first, UnitState second) => first.Id.CompareTo(second.Id);
    }
}
