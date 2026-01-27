using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K23MergeChecks
    {
        [MenuItem("Arcane Brigade/K23/Check star-up presentation")]
        public static void Run()
        {
            K4MergeChecks.Run();
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;
            float reveal = (float)typeof(MergeReveal).GetField("RevealAt", flags).GetRawConstantValue();
            float landing = (float)typeof(MergeReveal).GetField("LandAt", flags).GetRawConstantValue();
            float finish = (float)typeof(MergeReveal).GetField("FinishAt", flags).GetRawConstantValue();
            Require(reveal > 0.46f && landing > reveal && finish - landing >= 0.7f, "Distinct compression, reveal, landing and readable hold");
            string source = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/MergeReveal.cs");
            Require(!source.Contains("MergeConfirmationRing") && !source.Contains("ring.loop"), "No target-ring fallback");
            Require(source.Contains("MergePresentationStar") && source.Contains("CommittedTierStar") && source.Contains("starMesh.RecalculateBounds()"), "Code-native stars work without painted magic frames or font glyphs");
            Require(source.Contains("library.Magic.Length > 0") && source.Contains("confirmationFrames.Length - 1"), "Optional painted payoff tolerates absent or short animation arrays");
            Require(source.Contains("!ReducedMotion && age >= RevealAt") && source.Contains("ReducedMotion ? 0 : (1 - arrival)") && source.Contains("ReducedMotion ? 1.35f"), "Reduced motion removes burst travel and result scaling");
            Require(source.Contains("RunPhase.Paused || owner.Details.Open || owner.MenuConfirmation) { Skip(); return; }") && source.Contains("private void OnDisable() { Skip(); }"), "Pause, modal and disable still discard the presentation");
            Require(source.Contains("stage.SetActive(false); Destroy(stage)") && source.Contains("stars.Clear(); rays.Clear(); compression = null") && source.Contains("Destroy(starMesh)"), "Skip deactivates the entire stage and releases owned effect resources");
            Require(!source.Contains(".material.color") && source.Contains("SetPropertyBlock(starTint)"), "Star fades do not instantiate leaking renderer materials");
            string presentation = source.Substring(source.IndexOf("private void PresentStage()", StringComparison.Ordinal));
            Require(!presentation.Contains("owner.Simulation") && !presentation.Contains("owner.Session") && !presentation.Contains("PendingAttack =") && !presentation.Contains("Damage ="), "Payoff has no simulation writes or empowered attack");
            CheckAnimation();
            Debug.Log("K23 merge presentation checks PASS (animation fixtures, source contracts and K4 receipt checks; visual play-mode review still required)");
        }

        private static void CheckAnimation()
        {
            var host = new GameObject("K23MergeFixture");
            var stage = new GameObject("K23PresentationStage");
            MergeReveal reveal = host.AddComponent<MergeReveal>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            void Set(string field, object value) { typeof(MergeReveal).GetField(field, flags).SetValue(reveal, value); }
            void Present(float time)
            {
                Set("age", time);
                typeof(MergeReveal).GetMethod("PresentStage", flags).Invoke(reveal, null);
            }
            try
            {
                Set("stage", stage);
                Set("active", new MergeReveal.Beat { Result = new RosterEntry { Tier = 3 } });
                Set("sounded", true);
                var compression = new GameObject("Compression"); compression.transform.SetParent(stage.transform);
                Set("compression", compression.transform);
                var caption = new GameObject("Caption"); caption.transform.SetParent(stage.transform);
                Set("tierCaption", caption.AddComponent<TextMesh>());
                var stars = (List<Transform>)typeof(MergeReveal).GetField("stars", flags).GetValue(reveal);
                for (int index = 0; index < 3; index++)
                {
                    var star = new GameObject("Star"); star.transform.SetParent(stage.transform);
                    star.AddComponent<MeshRenderer>(); stars.Add(star.transform);
                }
                var rays = (List<LineRenderer>)typeof(MergeReveal).GetField("rays", flags).GetValue(reveal);
                var rayObject = new GameObject("Ray"); rayObject.transform.SetParent(stage.transform);
                LineRenderer ray = rayObject.AddComponent<LineRenderer>(); ray.positionCount = 2; rays.Add(ray);
                Present(0.55f);
                Require(compression.activeSelf && !stars[0].gameObject.activeSelf && !ray.enabled, "Compression precedes result stars and burst");
                Present(0.7f);
                Require(!compression.activeSelf && stars[0].gameObject.activeSelf && !stars[1].gameObject.activeSelf && ray.enabled, "Reveal starts staggered stars and open rays");
                Present(1.3f);
                Require(stars.TrueForAll(star => star.gameObject.activeSelf) && caption.activeSelf && !ray.enabled, "Landing retains all stars and caption after burst ends");
                reveal.ReducedMotion = true;
                Present(0.1f);
                Vector3 position = stars[0].localPosition;
                Vector3 scale = stars[0].localScale;
                Present(1.3f);
                Require(!compression.activeSelf && !ray.enabled && stars.TrueForAll(star => star.gameObject.activeSelf) && caption.activeSelf, "Reduced motion shows stable complete tier without glint or rays");
                Require(stars[0].localPosition == position && stars[0].localScale == scale && stars[0].localRotation == Quaternion.identity, "Reduced-motion stars do not travel, rotate or pop");
                Require(reveal.CompletedCount == 0, "Sampling visual frames cannot complete or commit a beat");
            }
            finally
            {
                Set("stage", null);
                UnityEngine.Object.DestroyImmediate(stage);
                UnityEngine.Object.DestroyImmediate(host);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("K23 merge check failed: " + message);
        }
    }
}
