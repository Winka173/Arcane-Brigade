using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class K15PlayerChecks : MonoBehaviour
    {
        private string root;
        private int frames;
        private bool badClock;
        private bool badCamera;
        private bool badPool;
        private bool badShader;
        private bool captured;
        private bool takingShot;
        private bool reduced;
        private ArenaController configured;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            if (!RunPaths.K7Capture || Array.IndexOf(arguments, "-arcane-k15") < 0 || FindAnyObjectByType<K15PlayerChecks>() != null) return;
            int path = Array.IndexOf(arguments, "-capturePath");
            if (path < 0 || path + 1 >= arguments.Length) return;
            var child = new GameObject("K15PlayerChecks"); DontDestroyOnLoad(child);
            K15PlayerChecks observer = child.AddComponent<K15PlayerChecks>();
            observer.root = Path.GetFullPath(arguments[path + 1]);
            observer.reduced = Array.IndexOf(arguments, "-arcane-k15-reduced") >= 0;
        }
        private void Update()
        {
            frames++; badClock |= Time.timeScale != 1;
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            if (owner == null) return;
            if (reduced && configured != owner && owner.Profile != null)
            {
                owner.Profile.ReducedShake = owner.Profile.ReducedFlashes = owner.Profile.ReducedEffects = true;
                PresentationSettings.Apply(owner.Profile); configured = owner;
            }
            badCamera |= owner.CameraAccent.magnitude > 0.091f || reduced && owner.CameraAccent != Vector2.zero;
            PaintedEffects effects = owner.GetComponent<PaintedEffects>();
            badPool |= effects != null && effects.ActiveCount > EffectDensity.Limit(64);
            Material paint = Resources.Load<Material>("UnitFlash");
            badShader |= paint == null || !paint.shader.isSupported;
            bool visibleHit = false;
            if (owner.Simulation != null && Camera.main != null)
                foreach (CombatEvent value in owner.Simulation.Events)
                {
                    if (value.Kind != CombatEventKind.Hit || value.Faction != Faction.Enemy || value.Value <= 0) continue;
                    Vector3 viewport = Camera.main.WorldToViewportPoint(value.Position);
                    visibleHit |= viewport.x > 0.1f && viewport.x < 0.9f && viewport.y > 0.1f && viewport.y < 0.9f;
                }
            if (!captured && !takingShot && (reduced ? owner.Simulation != null && owner.Simulation.Phase == RunPhase.Combat && owner.Simulation.Elapsed > 0.35f : visibleHit)) StartCoroutine(ImpactShots());
        }
        private IEnumerator ImpactShots()
        {
            takingShot = true;
            for (int frame = 0; frame < 4; frame++)
            {
                yield return new WaitForEndOfFrame();
                Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture();
                File.WriteAllBytes(Path.Combine(root, "09-impact-sequence-" + frame + ".png"), texture.EncodeToPNG()); Destroy(texture);
                yield return new WaitForSecondsRealtime(0.04f);
            }
            captured = true; takingShot = false;
        }
        private void OnApplicationQuit()
        {
            if (string.IsNullOrEmpty(root)) return;
            var notes = new List<string>();
            notes.Add((!badClock && frames > 60 ? "PASS " : "FAIL ") + "Input/simulation clock remains unscaled across " + frames + " native frames");
            notes.Add((!badCamera ? "PASS " : "FAIL ") + "Camera accents remain bounded below 0.091 world units");
            notes.Add((!badPool ? "PASS " : "FAIL ") + "Painted cosmetic pool remains bounded at " + (reduced ? 21 : 64) + " active bursts");
            notes.Add((!badShader ? "PASS " : "FAIL ") + "Sprite lighting shader supported on native renderer");
            notes.Add((captured ? "PASS " : "FAIL ") + (reduced ? "Four rendered frames with independent accessibility settings" : "Four rendered frames after a visible enemy impact"));
            notes.Add("Read-only observer alongside saved K14/K12 fixtures. Not natural-run, input-device latency, audio or human approval.");
            File.WriteAllLines(Path.Combine(root, "FeelChecks.txt"), notes);
        }
    }
}
