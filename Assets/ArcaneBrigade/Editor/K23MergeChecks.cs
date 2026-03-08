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
            float Constant(string name) => (float)typeof(MergeReveal).GetField(name, flags).GetRawConstantValue();
            float reveal = Constant("RevealAt"), landing = Constant("LandAt"), finish = Constant("FinishAt");
            Require(reveal > 0.46f && landing > reveal && finish - landing >= 0.7f, "Two stars: distinct gather, reveal, landing and readable hold");
            float knot = Constant("GrandKnotAt"), hold = Constant("GrandHold"), grandLanding = Constant("GrandLandAt"), grandFinish = Constant("GrandFinishAt");
            Require(knot > reveal && hold >= 0.08f && grandLanding > knot && grandFinish - grandLanding >= 1.2f && grandFinish > finish + 1, "Three stars: a longer ceremony with a held beat before its reveal");
            string source = File.ReadAllText("Assets/ArcaneBrigade/Runtime/Presentation/MergeReveal.cs");
            Require(!source.Contains("MergeConfirmationRing") && !source.Contains("ring.loop") && !source.Contains("LineRenderer") && !source.Contains("RevealRay"), "No target ring and no rays radiating from the unit");
            Require(!source.Contains("starMesh") && source.Contains("MechanicInk.Drawn(InkToken.Star") && source.Contains("StarUp_GoldStar") && source.Contains("StarUp_Crown"), "Stars are inked tokens (painted art replaces them when it exists), never a plain mesh");
            Require(source.Contains("RunPhase.Paused || owner.Details.Open || owner.MenuConfirmation) { Skip(); return; }") && source.Contains("private void OnDisable() { Skip(); }"), "Pause, modal and disable still discard the presentation");
            Require(source.Contains("stage.SetActive(false); Destroy(stage)") && source.Contains("back?.Destroy(); front?.Destroy();"), "Skip deactivates the entire stage and releases owned resources");
            Require(!source.Contains(".material.color") && source.Contains("SetPropertyBlock(flashBlock)"), "Silhouettes do not instantiate leaking renderer materials");
            string presentation = source.Substring(source.IndexOf("private void PresentStage()", StringComparison.Ordinal));
            Require(!presentation.Contains("owner.Simulation") && !presentation.Contains("owner.Session") && !presentation.Contains("PendingAttack =") && !presentation.Contains("Damage ="), "Payoff has no simulation writes or empowered attack");
            CheckAnimation();
            Debug.Log("K23 merge presentation checks PASS (two- and three-star fixtures, source contracts and K4 receipt checks; visual play-mode review still required)");
        }

        private static void CheckAnimation()
        {
            var host = new GameObject("K23MergeFixture");
            var stage = new GameObject("K23PresentationStage");
            MergeReveal reveal = host.AddComponent<MergeReveal>();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            void Set(string field, object value) { typeof(MergeReveal).GetField(field, flags).SetValue(reveal, value); }
            T Get<T>(string field) => (T)typeof(MergeReveal).GetField(field, flags).GetValue(reveal);
            void Present(float time)
            {
                Set("age", time);
                typeof(MergeReveal).GetMethod("PresentStage", flags).Invoke(reveal, null);
            }
            try
            {
                Set("stage", stage);
                Set("cues", 7);
                var caption = new GameObject("Caption"); caption.transform.SetParent(stage.transform);
                Set("tierCaption", caption.AddComponent<TextMesh>());
                // two stars
                Set("active", new MergeReveal.Beat { Result = new RosterEntry { Tier = 2 }, Materials = new List<RosterEntry>() });
                Present(0.5f);
                Require(Get<int>("starsShown") == 0 && !caption.activeSelf, "Two stars: nothing stamps before the reveal");
                Present(0.8f);
                Require(Get<int>("starsShown") == 1, "Two stars: the stars stamp on one after the other");
                Present(1.2f);
                Require(Get<int>("starsShown") == 2 && caption.activeSelf && !Get<bool>("crownShown") && Get<float>("dimmed") == 0, "Two stars: two stars and the caption, no crown and no dimmed stage");
                // three stars
                Set("active", new MergeReveal.Beat { Result = new RosterEntry { Tier = 3 }, Materials = new List<RosterEntry>() });
                Present(0.6f);
                Require(Get<float>("dimmed") > 0.5f && Get<int>("starsShown") == 0, "Three stars: the stage dims while the braid rises");
                float knot = (float)typeof(MergeReveal).GetField("GrandKnotAt", BindingFlags.NonPublic | BindingFlags.Static).GetRawConstantValue();
                Set("age", knot + 0.05f);
                Require(Mathf.Approximately((float)typeof(MergeReveal).GetProperty("VisualTime", flags).GetValue(reveal), knot), "Three stars: the frame holds when the braid knots");
                Present(2.4f);
                Require(Get<int>("starsShown") == 3 && Get<bool>("crownShown") && caption.activeSelf, "Three stars: three stars, a crown and the caption");
                reveal.ReducedMotion = true;
                Present(0.1f);
                Require(Get<int>("starsShown") == 3 && Get<bool>("crownShown") && caption.activeSelf, "Reduced motion shows the finished three-star at once");
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
