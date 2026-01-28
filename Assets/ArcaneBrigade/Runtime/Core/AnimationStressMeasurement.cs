using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class AnimationStressMeasurement
    {
        public string Name;
        public string Device;
        public string Cpu;
        public int Samples;
        public int PeakUnits;
        public int PeakShots;
        public double MedianMilliseconds;
        public double P95Milliseconds;
        public double P99Milliseconds;
        public long ManagedHeapStart;
        public long ManagedHeapEnd;
        public long UnityAllocatedEnd;
        public int Collections;
        public bool P95Within60Fps;
        public double SimulationP95Milliseconds;
        public double RenderP95Milliseconds;
        public long ManagedAllocatedBytes;
        public bool ManagedAllocationCounterAvailable;
        internal static bool Recording;
        internal static double SimulationMilliseconds;
        internal static double RenderMilliseconds;
        private readonly List<double> simulationFrames = new List<double>(8192);
        private readonly List<double> renderFrames = new List<double>(8192);
        private long allocatedAtStart;
        private readonly List<double> frames = new List<double>(8192);
        private int collectionsAtStart;
        public void Begin(string name)
        {
            Name = name; Device = SystemInfo.graphicsDeviceName; Cpu = SystemInfo.processorType;
            ManagedHeapStart = Profiler.GetMonoUsedSizeLong(); collectionsAtStart = GC.CollectionCount(0); frames.Clear();
            simulationFrames.Clear(); renderFrames.Clear(); SimulationMilliseconds = RenderMilliseconds = 0;
            long probeBefore = GC.GetAllocatedBytesForCurrentThread();
            var allocationProbe = new byte[1024]; GC.KeepAlive(allocationProbe);
            ManagedAllocationCounterAvailable = GC.GetAllocatedBytesForCurrentThread() > probeBefore;
            allocatedAtStart = GC.GetAllocatedBytesForCurrentThread(); Recording = true;
        }
        public void Sample(CombatSimulation simulation)
        {
            frames.Add(Time.unscaledDeltaTime * 1000d); PeakUnits = Mathf.Max(PeakUnits, simulation.Units.Count); PeakShots = Mathf.Max(PeakShots, simulation.Projectiles.Count);
            simulationFrames.Add(SimulationMilliseconds); renderFrames.Add(RenderMilliseconds); SimulationMilliseconds = RenderMilliseconds = 0;
        }
        public void Finish()
        {
            Recording = false; ManagedAllocatedBytes = ManagedAllocationCounterAvailable ? GC.GetAllocatedBytesForCurrentThread() - allocatedAtStart : -1;
            simulationFrames.Sort(); renderFrames.Sort();
            if (simulationFrames.Count > 0) { int percentile = Mathf.Clamp((int)Math.Ceiling(simulationFrames.Count * .95) - 1, 0, simulationFrames.Count - 1); SimulationP95Milliseconds = simulationFrames[percentile]; RenderP95Milliseconds = renderFrames[percentile]; }
            frames.Sort(); Samples = frames.Count;
            if (Samples > 0) { MedianMilliseconds = Percentile(.5); P95Milliseconds = Percentile(.95); P99Milliseconds = Percentile(.99); }
            ManagedHeapEnd = Profiler.GetMonoUsedSizeLong(); UnityAllocatedEnd = Profiler.GetTotalAllocatedMemoryLong(); Collections = GC.CollectionCount(0) - collectionsAtStart;
            P95Within60Fps = Samples > 0 && P95Milliseconds <= 16.667;
        }
        private double Percentile(double fraction) => frames[Mathf.Clamp((int)Math.Ceiling(frames.Count * fraction) - 1, 0, frames.Count - 1)];
    }
}
