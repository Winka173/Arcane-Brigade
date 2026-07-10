using System.Collections.Generic;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        // Damage/death callbacks may append splitters or summons. Removals happen only during
        // phase cleanup, outside damage traversal. Capture the initial count without allocating
        // an array so a new body cannot be hit again by the area that just created it.
        private UnitIteration StableUnits => new UnitIteration(units);

        private readonly struct UnitIteration
        {
            private readonly List<UnitState> source;
            public UnitIteration(List<UnitState> source) { this.source = source; }
            public Enumerator GetEnumerator() => new Enumerator(source);

            public struct Enumerator
            {
                private readonly List<UnitState> source;
                private readonly int count;
                private int index;
                public Enumerator(List<UnitState> source) { this.source = source; count = source.Count; index = -1; }
                public UnitState Current => source[index];
                public bool MoveNext() => ++index < count;
            }
        }
    }
}
