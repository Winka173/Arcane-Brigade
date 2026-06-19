using System;
using System.Collections.Generic;
namespace ArcaneBrigade
{
    public sealed class BondHudSummary
    {
        public const int MaximumRows = 5;
        public const float RowHeight = 38, Width = 345; // large enough to read on a phone
        private readonly List<BondDefinition> rows = new List<BondDefinition>();
        private BondDefinition[] source;
        private int signature;
        public IReadOnlyList<BondDefinition> Rows => rows;
        public int Active { get; private set; }
        public int VisibleCount => Math.Min(MaximumRows, rows.Count);
        public int HiddenCount => Math.Max(0, rows.Count - MaximumRows);
        public float Height => 36 + VisibleCount * RowHeight + 28;
        public void Refresh(BondDefinition[] bonds, IReadOnlyDictionary<string, int> counts)
        {
            int next = 17;
            foreach (BondDefinition b in bonds) { counts.TryGetValue(b.Id, out int n); next = unchecked(next * 31 + n); }
            if (source == bonds && signature == next) return;
            source = bonds; signature = next; rows.Clear(); Active = 0;
            foreach (BondDefinition b in bonds)
                if (counts.TryGetValue(b.Id, out int n) && n > 0) { rows.Add(b); if (b.ActiveLevel(n) > 0) Active++; }
            rows.Sort((a, b) =>
            {
                int ac = counts[a.Id], bc = counts[b.Id], al = a.ActiveLevel(ac), bl = b.ActiveLevel(bc);
                int byLevel = bl.CompareTo(al); if (byLevel != 0) return byLevel;
                double ap = (double)ac / a.NextThreshold(ac), bp = (double)bc / b.NextThreshold(bc);
                int byProgress = bp.CompareTo(ap); return byProgress != 0 ? byProgress : Array.IndexOf(bonds, a).CompareTo(Array.IndexOf(bonds, b));
            });
        }
    }
}
