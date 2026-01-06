using System;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class RunPaths
    {
        private static string diagnosticRoot;
        public static bool K7Capture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k7-capture") >= 0;
        public static bool K6Capture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k6-capture") >= 0;
        public static bool K5Capture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k5-capture") >= 0;
        public static bool K4Capture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k4-capture") >= 0;
        public static bool K4FinalCapture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k4-final-capture") >= 0;
        public static bool QualityCapture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-quality-capture") >= 0;
        public static bool K3FinalCapture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k3-final-capture") >= 0;
        public static bool K2Capture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k2-capture") >= 0;
        public static bool AltarCapture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-altar-capture") >= 0;
        public static bool K32Capture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k32-capture") >= 0 || AltarCapture;
        public static bool K3Capture => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k3-capture") >= 0 || K32Capture;
        public static bool Diagnostic => Application.isBatchMode || FullRunPlayerCapture.Requested || Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-k1-capture") >= 0 || K2Capture || K3Capture || K3FinalCapture || QualityCapture || K4Capture || K4FinalCapture || K5Capture || K6Capture || K7Capture;
        public static string Root
        {
            get
            {
                if (!Diagnostic) return Application.persistentDataPath;
                if (diagnosticRoot == null) diagnosticRoot = Path.Combine(Application.temporaryCachePath, "ArcaneBrigadeDiagnostics", Guid.NewGuid().ToString("N"));
                return diagnosticRoot;
            }
        }
    }
}
