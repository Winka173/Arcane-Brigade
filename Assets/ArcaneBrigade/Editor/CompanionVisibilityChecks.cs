using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class CompanionVisibilityChecks
    {
        public static void Run()
        {
            var notes = new List<string>(); const string path = "PreviewExports/CompanionVisibility/Focused_Checks.txt";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog");
            var block = new MaterialPropertyBlock(); int splitSamples = 0;
            foreach (UnitDefinition source in c.Companions.Concat(c.Champions))
            {
                bool companion = c.Companions.Contains(source);
                for (int tier = 1; tier <= (companion ? 3 : 1); tier++)
                {
                    UnitDefinition definition = UnityEngine.Object.Instantiate(source); definition.CombatTier = tier;
                    // This edit-mode probe isolates body rendering from weapon MonoBehaviour initialization.
                    WeaponDefinition probe = UnityEngine.Object.Instantiate(source.Weapon); probe.Id = "BodyVisibilityProbe";
                    probe.Motion = WeaponMotion.Recoil; definition.Weapon = probe;
                    var state = new UnitState { Id = 9, Definition = definition, Faction = companion ? Faction.Companion : Faction.Champion,
                        Alive = true, Health = companion ? float.PositiveInfinity : definition.Health, Facing = Vector2.right, AttackDirection = Vector2.right,
                        LastHit = -100, AttackStarted = -100, LastSupport = -100 };
                    UnitVisual v = UnityEngine.Object.Instantiate(c.Arena.UnitPrefab).GetComponent<UnitVisual>();
                    try
                    {
                        v.Bind(state, c.Arena, true);
                        for (int phase = 0; phase < 12; phase++)
                        {
                            float time = 10 + phase * .137f;
                            state.Position = Vector2.zero; state.PreviousPosition = phase == 0 ? Vector2.zero : Vector2.left * .03f;
                            state.Velocity = phase == 0 ? Vector2.zero : Vector2.right * 2;
                            state.TravelMode = phase >= 6 ? CompanionTravelMode.Pursuit : CompanionTravelMode.Patrol;
                            state.AttackStarted = phase % 3 == 0 ? -100 : time - (phase % 3 == 1 ? definition.Weapon.Windup * .5f : definition.Weapon.Windup + .03f);
                            state.PendingAttack = phase % 3 == 1; state.AttackWindup = definition.Weapon.Windup;
                            state.LastHit = phase >= 9 ? time - .04f : -100;
                            v.Display(time, 1);
                            bool valid = true; string detail = "";
                            foreach (SpriteRenderer r in v.BodyRenderer.GetComponentsInChildren<SpriteRenderer>())
                            {
                                if (!r.enabled || r.sprite == null) continue;
                                r.GetPropertyBlock(block); Vector4 rect = block.GetVector("_UvRect");
                                if (r != v.BodyRenderer) splitSamples++;
                                bool contains = r.sprite.uv.All(uv => uv.x >= rect.x - .00001f && uv.x <= rect.x + rect.z + .00001f && uv.y >= rect.y - .00001f && uv.y <= rect.y + rect.w + .00001f);
                                if (!contains || block.GetFloat("_Flash") != 0 || r.color.a < .99f) { valid = false; detail += " " + r.name + " shader UV clips its own sprite=" + !contains; }
                            }
                            string line = (valid ? "PASS " : "FAIL ") + source.Id + " tier=" + tier + " phase=" + phase + detail;
                            notes.Add(line);
                            if (!valid) { File.WriteAllLines(path, notes); throw new InvalidOperationException(line); }
                        }
                    }
                    finally { UnityEngine.Object.DestroyImmediate(v.gameObject); UnityEngine.Object.DestroyImmediate(probe); UnityEngine.Object.DestroyImmediate(definition); }
                }
            }
            if (splitSamples == 0) throw new InvalidOperationException("Split-body path must be exercised");
            var summary = new BondHudSummary(); var counts = c.Bonds.ToDictionary(b => b.Id, b => 1);
            BondDefinition priority = c.Bonds[c.Bonds.Length - 1]; counts[priority.Id] = priority.Threshold * 3;
            summary.Refresh(c.Bonds, counts);
            if (summary.Rows[0] != priority || summary.VisibleCount != 5 || summary.HiddenCount != c.Bonds.Length - 5 || summary.Height > 224 || summary.Active != 1)
                throw new InvalidOperationException("Crowded bond HUD must prioritize active bonds and stay bounded");
            notes.Add("PASS Full 29-bond HUD prioritizes active bonds, shows five rows and preserves overflow count within 224px");
            counts.Clear(); counts[c.Bonds[0].Id] = 1; summary.Refresh(c.Bonds, counts);
            if (summary.VisibleCount != 1 || summary.HiddenCount != 0 || summary.Rows.Count != 1) throw new InvalidOperationException("Bond HUD must update after removing companions");
            notes.Add("PASS Small teams shrink the bond HUD and removing companions invalidates its cached ordering");
            File.WriteAllLines(path, notes); Debug.Log("COMPANION_VISIBILITY_CHECKS " + notes.Count + " passed / " + splitSamples + " split-body samples");
        }
        public static void BuildWindows() { Run(); FullRunBuilder.BuildWindows(); }
    }
}
