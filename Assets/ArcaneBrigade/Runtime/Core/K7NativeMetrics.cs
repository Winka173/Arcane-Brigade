using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace ArcaneBrigade
{
    public sealed class K7NativeMetrics : IDisposable
    {
        private struct Sample { public double Frame; public double Cpu; public double Gpu; public long Gc; public long Memory; public int Units; public int Projectiles; }
        private readonly List<Sample> samples = new List<Sample>(4000);
        private readonly FrameTiming[] timings = new FrameTiming[1];
        private ProfilerRecorder gc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
        private ProfilerRecorder cpu = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "CPU Main Thread Frame Time");
        private double started;
        public void Begin() { samples.Clear(); started = Time.realtimeSinceStartupAsDouble; }
        public void Record(CombatSimulation simulation)
        {
            FrameTimingManager.CaptureFrameTimings();
            uint available = FrameTimingManager.GetLatestTimings(1, timings);
            samples.Add(new Sample { Frame = Time.unscaledDeltaTime * 1000, Cpu = cpu.Valid ? cpu.LastValue / 1000000.0 : available > 0 ? timings[0].cpuMainThreadFrameTime : 0, Gpu = available > 0 ? timings[0].gpuFrameTime : 0, Gc = gc.Valid ? gc.LastValue : -1, Memory = Profiler.GetTotalAllocatedMemoryLong(), Units = simulation.Units.Count, Projectiles = simulation.Projectiles.Count });
        }
        public string Write(string directory, string name)
        {
            double elapsed = Time.realtimeSinceStartupAsDouble - started;
            var frames = new List<double>(); var gpuFrames = new List<double>(); var cpuFrames = new List<double>();
            double sum = 0; long peakMemory = 0; long peakGc = -1; long totalGc = 0; int units = 0; int projectiles = 0;
            using (var writer = new StreamWriter(Path.Combine(directory, name + "-frames.csv")))
            {
                writer.WriteLine("frame_ms,cpu_main_ms,gpu_ms,gc_bytes,total_allocated_bytes,units,projectiles");
                foreach (Sample sample in samples)
                {
                    frames.Add(sample.Frame); sum += sample.Frame;
                    if (sample.Gpu > 0) gpuFrames.Add(sample.Gpu); if (sample.Cpu > 0) cpuFrames.Add(sample.Cpu);
                    peakMemory = Math.Max(peakMemory, sample.Memory); peakGc = Math.Max(peakGc, sample.Gc); totalGc += Math.Max(0, sample.Gc);
                    units = Math.Max(units, sample.Units); projectiles = Math.Max(projectiles, sample.Projectiles);
                    writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "{0:0.000},{1:0.000},{2:0.000},{3},{4},{5},{6}", sample.Frame, sample.Cpu, sample.Gpu, sample.Gc, sample.Memory, sample.Units, sample.Projectiles));
                }
            }
            frames.Sort(); gpuFrames.Sort(); cpuFrames.Sort();
            string summary = string.Format(CultureInfo.InvariantCulture, "{0}: frames={1}; wall={2:0.00}s; fps={3:0.0}; frame_p50={4}ms; p95={5}ms; p99={6}ms; cpu_p95={7}ms; gpu_p95={8}ms; gc_valid={9}; gc_mean={10}bytes/frame; gc_peak={11}; memory_peak={12:0.0}MiB; units_peak={13}; projectiles_peak={14}", name, samples.Count, elapsed, samples.Count * 1000 / Math.Max(1, sum), Percentile(frames, 0.5), Percentile(frames, 0.95), Percentile(frames, 0.99), Percentile(cpuFrames, 0.95), Percentile(gpuFrames, 0.95), gc.Valid, gc.Valid ? (totalGc / (double)Math.Max(1, samples.Count)).ToString("0", CultureInfo.InvariantCulture) : "NA", gc.Valid ? peakGc.ToString(CultureInfo.InvariantCulture) : "NA", peakMemory / 1048576.0, units, projectiles);
            File.WriteAllText(Path.Combine(directory, name + "-summary.txt"), summary); return summary;
        }
        private static string Percentile(List<double> values, double percentile) => values.Count == 0 ? "NA" : values[(int)Math.Min(values.Count - 1, Math.Floor(values.Count * percentile))].ToString("0.000", CultureInfo.InvariantCulture);
        public void Dispose() { gc.Dispose(); cpu.Dispose(); }
    }
}
