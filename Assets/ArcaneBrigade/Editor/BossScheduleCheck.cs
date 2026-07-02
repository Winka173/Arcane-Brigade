using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Which bosses really walk in on each of the 20 nights: the night's boss, the mid-map boss and any other boss
    /// body seen in the first 40 s (batch: -executeMethod ArcaneBrigade.Editor.BossScheduleCheck.Run).</summary>
    public static class BossScheduleCheck
    {
        public static void Run()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var log = new StringBuilder();
            for (int wave = 1; wave <= 20; wave++)
            {
                var session = new RunSession(catalog);
                session.Start("LanternKnight", 5100 + wave); session.State.Wave = wave; session.State.Phase = SessionPhase.Combat;
                CheckpointCatalog arena = session.PrepareCombat();
                var sim = new CombatSimulation(arena); sim.Start(5100 + wave);
                var seen = new HashSet<string>();
                for (int i = 0; i < 40 * 20 && sim.Phase == RunPhase.Combat; i++)
                {
                    sim.Champion.Health = 1e7f;
                    sim.Step(.05f, new Vector2(Mathf.Cos(i * .01f), Mathf.Sin(i * .013f)) * .5f);
                    foreach (UnitState u in sim.Units) if (u.Alive && u.Definition.IsBoss) seen.Add(u.Definition.Id + "@" + Mathf.RoundToInt(sim.Elapsed));
                }
                var first = new Dictionary<string, string>();
                foreach (string s in seen) { string id = s.Split('@')[0]; if (!first.ContainsKey(id)) first[id] = s; }
                log.AppendLine("BOSSCHECK wave " + wave + " | night boss " + (arena.Boss != null ? arena.Boss.Id : "-") + " | mid " + (arena.MidBoss != null ? arena.MidBoss.Id : "-")
                    + " | elite " + (arena.MapElite != null ? arena.MapElite.Id : "-") + " | length " + Mathf.RoundToInt(arena.WaveDuration) + " | bosses seen " + string.Join(", ", first.Values));
                session.Dispose();
            }
            Debug.Log(log.ToString());
        }
    }
}
