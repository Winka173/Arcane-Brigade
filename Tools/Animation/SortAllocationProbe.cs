using System;
using System.Collections.Generic;

public static class SortAllocationProbe
{
    private sealed class IdentityComparer : IComparer<int>
    {
        public int Compare(int first, int second) => first.CompareTo(second);
    }
    public static int Main()
    {
        var values = new List<int>(32);
        var comparer = new IdentityComparer();
        for (int index = 31; index >= 0; index--) values.Add(index);
        for (int index = 0; index < 100; index++) values.Sort(comparer);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int index = 0; index < 1000; index++) values.Sort(comparer);
        long comparerBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Comparison<int> comparison = comparer.Compare;
        for (int index = 0; index < 100; index++) values.Sort(comparison);
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int index = 0; index < 1000; index++) values.Sort(comparison);
        long cachedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Console.WriteLine("Installed Mono / 1000 warmed List.Sort calls / 32 integer identities");
        Console.WriteLine("IComparer bytes: " + comparerBytes);
        Console.WriteLine("Cached Comparison bytes: " + cachedBytes);
        Console.WriteLine("This library probe is not an in-game frame-pacing benchmark.");
        return cachedBytes == 0 ? 0 : 1;
    }
}
