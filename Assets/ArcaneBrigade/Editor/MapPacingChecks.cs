using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>MapPacing (requirement 1/11/2026): plays every Act I map wave for 45 s and records which side mechanics started.
    /// A map's first fighting wave starts none; every other wave starts only its own one.</summary>
    public static class MapPacingChecks
    {
        public static void RunBatch()
        {
            var lines = new List<string>(); int failed = 0;
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            for (int wave = 2; wave <= 19; wave++)
            {
                if (ActOneCanon.BossWave(wave)) continue;
                var session = new RunSession(catalog); // one run: the same seed on every wave
                try
                {
                    session.Start("LanternKnight", 31002); session.State.Wave = wave; session.State.Roster.Clear(); session.State.Phase = SessionPhase.Combat;
                    CheckpointCatalog arena = session.PrepareCombat();
                    if (arena.MapEnvironment < 0) { lines.Add("SKIP wave " + wave + ": no map mechanics"); continue; }
                    var sim = new CombatSimulation(arena); sim.Start(31002);
                    var seen = new HashSet<MapSide>();
                    for (int i = 0; i < 45 * 20 && sim.Phase == RunPhase.Combat; i++)
                    {
                        sim.Champion.Health = 1e7f;
                        float a = i * .02f;
                        sim.Step(.05f, new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
                        Seen(sim.MapEnvironment, sim.Elapsed, seen);
                    }
                    MapSide side = sim.MapEnvironment.Side;
                    seen.Remove(side);
                    bool ok = seen.Count == 0;
                    if (!ok) failed++;
                    lines.Add((ok ? "PASS" : "FAIL") + " wave " + wave + " " + sim.MapEnvironment.Theme + ": side " + side + (ok ? "" : ", also started " + string.Join(", ", seen)));
                }
                finally { session.Dispose(); }
            }
            Directory.CreateDirectory("PreviewExports/MapPacing");
            File.WriteAllLines("PreviewExports/MapPacing/Checks.txt", lines);
            Debug.Log("MAP_PACING " + (failed == 0 ? "PASS" : "FAIL") + "\n" + string.Join("\n", lines));
            if (Application.isBatchMode) EditorApplication.Exit(failed == 0 ? 0 : 1);
        }

        /// <summary>The side mechanics that have something on the field right now.</summary>
        private static void Seen(MapEnvironmentState s, float t, HashSet<MapSide> seen)
        {
            if (s == null) return;
            if (s.RoomTickets.Count > 0) seen.Add(MapSide.RoomTickets);
            if (s.RoomDust.Count > 0) seen.Add(MapSide.RoomDust);
            foreach (var w in s.RoomWaiters.Values) if (w.AngryUntil > t) { seen.Add(MapSide.RoomWaiters); break; }
            if (s.RoomClockGlowAt > 0) seen.Add(MapSide.RoomClock);
            if (s.GustWarnAt > 0) seen.Add(MapSide.PostWind);
            if (s.Stamped.Count > 0) seen.Add(MapSide.PostStamps);
            if (s.ParcelState > 0) seen.Add(MapSide.PostParcel);
            if (s.PostPlanesWarnAt > 0) seen.Add(MapSide.PostPlanes);
            if (s.GlassCarriers.Count > 0) seen.Add(MapSide.GlassCarriers);
            if (s.GlassPanes.Count > 0) seen.Add(MapSide.GlassPanes);
            foreach (var bud in s.GlassBuds) if (bud.Open(t) || bud.Hold > 0) { seen.Add(MapSide.GlassBuds); break; }
            if (s.LampOils.Count > 0) seen.Add(MapSide.LampOil);
            if (s.LanternBorn > 0) seen.Add(MapSide.LampLantern);
            if (s.TheatreThrows.Count > 0) seen.Add(MapSide.TheatreAudience);
            if (s.TheatreCurtainWarnAt > 0) seen.Add(MapSide.TheatreCurtain);
            if (s.TheatreFuse > 0 || s.TheatreLoad > 0) seen.Add(MapSide.TheatreFirework);
            if (s.VaultPages.Count > 0) seen.Add(MapSide.VaultPages);
            if (s.VaultWorms.Count > 0) seen.Add(MapSide.VaultWorms);
            if (s.VaultPotAt > 0) seen.Add(MapSide.VaultPot);
            if (s.VaultStampAt > 0) seen.Add(MapSide.VaultStamp);
        }
    }
}
