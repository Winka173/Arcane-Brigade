using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>For diagnostic captures only: how large each painted UI image is drawn compared with its own pixels (the largest
    /// seen). A capture writes KitDrawSizes.txt to its -capturePath on quit, worst first, so art drawn far over its painted size
    /// (it looks soft) can be found and repainted (requirement 4/11/2026).</summary>
    public static class KitDrawAudit
    {
        private static readonly Dictionary<string, (float scale, int w, int h)> seen = new Dictionary<string, (float, int, int)>();
        private static bool hooked;
        /// <summary>The image <paramref name="key"/> (w x h painted pixels) is drawn at <paramref name="guiPerPixel"/> 1080p GUI
        /// units per painted pixel in this pass.</summary>
        public static void Note(string key, float guiPerPixel, int w, int h)
        {
            if (!RunPaths.Diagnostic || Event.current == null || Event.current.type != EventType.Repaint || string.IsNullOrEmpty(key)) return;
            if (!hooked) { hooked = true; Application.quitting += Write; }
            if (!seen.TryGetValue(key, out var known) || guiPerPixel > known.scale) seen[key] = (guiPerPixel, w, h);
        }
        private static void Write()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) return;
            var lines = seen.OrderByDescending(p => p.Value.scale).Select(p => p.Value.scale.ToString("0.00") + "\t" + p.Value.w + "x" + p.Value.h + "\t" + p.Key);
            try { File.WriteAllLines(Path.Combine(args[at + 1], "KitDrawSizes.txt"), lines); } catch (IOException) { }
        }
    }
}
