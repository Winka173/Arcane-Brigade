using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

namespace ArcaneBrigade
{
    public sealed class K7FramebufferRecorder : MonoBehaviour
    {
        private struct Packet { public byte[] Pixels; public int Frame; }
        private readonly object sync = new object();
        private readonly Queue<byte[]> free = new Queue<byte[]>();
        private readonly Queue<Packet> packets = new Queue<Packet>();
        private Process encoder;
        private Thread writer;
        private RenderTexture surface;
        private bool finishing;
        private volatile bool failed;
        private volatile bool writerDone;
        private int pending;
        private int captured;
        private int encoded;
        private int goal;
        private double started;
        private string metadata;
        public bool Complete { get; private set; }
        public bool Passed => Complete && !failed && encoded == goal;
        public void Begin(float seconds)
        {
            if (!RunPaths.K7Capture && !RulesInkCapture.Requested) throw new InvalidOperationException("Framebuffer recorder requires isolated diagnostic mode");
            string[] args = Environment.GetCommandLineArgs();
            int executable = Array.IndexOf(args, "-ffmpegPath"); int output = Array.IndexOf(args, "-videoPath");
            if (executable < 0 || output < 0 || executable + 1 >= args.Length || output + 1 >= args.Length) throw new ArgumentException("Video requires -ffmpegPath and -videoPath");
            if (Screen.width != 1920 || Screen.height != 1080 || Time.timeScale != 1) throw new InvalidOperationException("Realtime recording requires actual1920x1080 and timeScale1");
            string destination = Path.GetFullPath(args[output + 1]); Directory.CreateDirectory(Path.GetDirectoryName(destination));
            metadata = destination + ".txt"; goal = Mathf.RoundToInt(seconds * 60);
            var settings = new ProcessStartInfo(args[executable + 1], "-y -hide_banner -loglevel error -f rawvideo -pixel_format rgba -video_size 1920x1080 -framerate 60 -i pipe:0 -an -c:v libx264 -preset ultrafast -crf 20 -pix_fmt yuv420p \"" + destination + "\"") { UseShellExecute = false, CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden, RedirectStandardInput = true, RedirectStandardError = true };
            encoder = Process.Start(settings); encoder.ErrorDataReceived += (sender, message) => { if (!string.IsNullOrEmpty(message.Data)) failed = true; }; encoder.BeginErrorReadLine();
            for (int slot = 0; slot < 6; slot++) free.Enqueue(new byte[1920 * 1080 * 4]);
            surface = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32); surface.Create();
            writer = new Thread(WriteFrames) { IsBackground = true }; writer.Start(); started = Time.realtimeSinceStartupAsDouble;
            StartCoroutine(Capture(seconds));
        }
        private IEnumerator Capture(float seconds)
        {
            var end = new WaitForEndOfFrame();
            while (Time.realtimeSinceStartupAsDouble - started < seconds && !failed)
            {
                yield return end;
                byte[] buffer = null; lock (sync) { if (free.Count > 0) buffer = free.Dequeue(); }
                if (buffer == null) continue;
                int frame = Mathf.Min(goal - 1, (int)Math.Floor((Time.realtimeSinceStartupAsDouble - started) * 60));
                ScreenCapture.CaptureScreenshotIntoRenderTexture(surface); pending++;
                AsyncGPUReadback.Request(surface, 0, TextureFormat.RGBA32, request =>
                {
                    pending--;
                    if (request.hasError) { failed = true; lock (sync) free.Enqueue(buffer); return; }
                    request.GetData<byte>().CopyTo(buffer); captured++;
                    lock (sync) { packets.Enqueue(new Packet { Pixels = buffer, Frame = frame }); Monitor.Pulse(sync); }
                });
            }
            while (pending > 0) yield return null;
            lock (sync) { finishing = true; Monitor.Pulse(sync); }
            while (!writerDone) yield return null;
            encoder.WaitForExit(); failed |= encoder.ExitCode != 0;
            File.WriteAllText(metadata, (failed ? "FAIL" : "PASS") + "; Unity framebuffer only, no OS window/desktop/microphone capture; wall_seconds=" + seconds + "; encoded_frames=" + encoded + "; unique_readbacks=" + captured + "; CFR=60; missing wall-clock slots repeat captured frames without accelerating gameplay; movieFPS is not performance evidence; timeScale=" + Time.timeScale + "; raw_surface1920x1080; encoder outside performance measurements.");
            surface.Release(); Destroy(surface); surface = null; Complete = true;
        }
        private void WriteFrames()
        {
            byte[] previous = null;
            try
            {
                Stream stream = encoder.StandardInput.BaseStream;
                while (true)
                {
                    Packet packet;
                    lock (sync)
                    {
                        while (packets.Count == 0 && !finishing) Monitor.Wait(sync);
                        if (packets.Count == 0 && finishing) break;
                        packet = packets.Dequeue();
                    }
                    while (encoded <= packet.Frame && encoded < goal) { stream.Write(packet.Pixels, 0, packet.Pixels.Length); encoded++; }
                    lock (sync) { if (previous != null) free.Enqueue(previous); previous = packet.Pixels; }
                }
                while (previous != null && encoded < goal) { stream.Write(previous, 0, previous.Length); encoded++; }
                encoder.StandardInput.Close();
            }
            catch { failed = true; }
            finally { writerDone = true; }
        }
        private void OnDestroy()
        {
            lock (sync) { finishing = true; Monitor.PulseAll(sync); }
            if (encoder != null && !encoder.HasExited) encoder.Kill();
            if (surface != null) { surface.Release(); Destroy(surface); }
        }
    }
}
