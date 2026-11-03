using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Act I as the player meets it (story review 3/11/2026): for every wave, the map, the zone the story is in, the
    /// boss (shown name and body) or mid-map boss, the enemies of its pool and the story beats of that wave. Two seeds, since
    /// the last boss and the pools depend on the seed. Writes PreviewExports/ActOneContent/Waves.md.</summary>
    public static class ActOneContentDump
    {
        public static void RunBatch()
        {
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var lines = new List<string> { "# Act I content by wave", "" };
            foreach (int seed in new[] { 31002, 77138 })
            {
                lines.Add("## Seed " + seed);
                lines.Add("");
                lines.Add("| Wave | Map | Story zone | Boss / mid boss | Enemies | Story beats |");
                lines.Add("| --- | --- | --- | --- | --- | --- |");
                for (int wave = 1; wave <= ActOneCanon.Waves; wave++)
                {
                    var session = new RunSession(catalog);
                    try
                    {
                        session.Start("LanternKnight", seed); session.State.Wave = wave; session.State.Roster.Clear(); session.State.Phase = SessionPhase.Combat;
                        CheckpointCatalog arena = session.PrepareCombat();
                        string map = MapIdentity.Name(MapIdentity.For(wave));
                        string boss = arena.Boss != null ? "**" + arena.Boss.DisplayName + "** (" + arena.Boss.Id + ")" : arena.MidBoss != null ? "mid: " + arena.MidBoss.DisplayName + " (" + arena.MidBoss.Id + ")" : arena.MapElite != null ? "elite: " + arena.MapElite.DisplayName : "";
                        var pool = (arena.EnemyPool ?? new UnitDefinition[0]).Where(u => u != null).Select(u => u.DisplayName).Distinct().ToList();
                        string beats = string.Join(" · ", MienChoStory.Chapters.Where(c => c.Wave == wave).Select(c => (c.Shop ? "shop: " : "fight: ") + c.Title));
                        lines.Add("| " + wave + " | " + map + " | " + ActOneCanon.PlaceNames[ActOneCanon.Place(wave)] + " | " + boss + " | " + string.Join(", ", pool) + " | " + beats + " |");
                    }
                    finally { session.Dispose(); }
                }
                lines.Add("");
            }
            Directory.CreateDirectory("PreviewExports/ActOneContent");
            File.WriteAllLines("PreviewExports/ActOneContent/Waves.md", lines);
            Debug.Log("ACT_ONE_CONTENT written");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
    }
}
