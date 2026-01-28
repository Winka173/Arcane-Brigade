using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class DreamLevitationChecks
    {
        [MenuItem("Arcane Brigade/Presentation/Check dream levitation")]
        public static void Run()
        {
            var notes = new List<string>();
            void Require(bool value, string label) { if (!value) throw new InvalidOperationException(label); notes.Add("PASS " + label); }
            for (int identity = 0; identity < 32; identity++)
                foreach (float depth in new[] { 0f, 2f, 4f })
                {
                    Vector3 still = DreamLevitation.Offset(0, identity, depth, true);
                    Require(still == DreamLevitation.Offset(150, identity, depth, true), "Reduced motion stays suspended without drifting " + identity + "/" + depth);
                    Require(DreamLevitation.Tilt(87, identity, true) == 0, "Reduced motion removes tilt " + identity + "/" + depth);
                    for (int sample = 0; sample < 120; sample++)
                    {
                        Vector3 offset = DreamLevitation.Offset(sample * .71f, identity, depth, false);
                        Require(offset.y > .24f && offset.y < .6f && Mathf.Abs(offset.x) <= .0751f, "Bounded visible air gap " + identity + "/" + depth + "/" + sample);
                        Require(Mathf.Abs(DreamLevitation.Tilt(sample, identity, false)) <= 4.51f, "Tilt does not overturn props " + identity + "/" + sample);
                    }
                }
            Require(DreamLevitation.Offset(3, 1, 0, false) != DreamLevitation.Offset(3, 2, 0, false), "Props do not bob in synchrony");
            Directory.CreateDirectory("PreviewExports/Levitation");
            File.WriteAllLines("PreviewExports/Levitation/Focused_Checks.txt", notes);
            Debug.Log("DREAM_LEVITATION_PASS " + notes.Count);
        }
    }
}
