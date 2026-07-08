using System.Text;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Which piece the game plays on each of the 20 nights, the shops, the menus and endless (batch:
    /// -executeMethod ArcaneBrigade.Editor.MusicScheduleCheck.Run). Mirrors ArcadeMusic's own choice.</summary>
    public static class MusicScheduleCheck
    {
        public static void Run()
        {
            var log = new StringBuilder();
            for (int wave = 1; wave <= 20; wave++)
            {
                bool boss = ActOneCanon.BossWave(wave);
                string scene = ArcaneCueLibrary.SceneFor(SessionPhase.Combat, boss, wave, 20, false, false);
                // the same choice as ArcadeMusic.PieceVariant, for three seeds
                var seen = new System.Collections.Generic.List<string>();
                foreach (int seed in new[] { 0, 1, 2 })
                {
                    int ordinal = 0; for (int n = MapIdentity.FirstWave(wave); n < wave; n++) if (!ActOneCanon.BossWave(n)) ordinal++;
                    ArcanePieceDef p = ArcaneCueLibrary.PieceForScene(scene, (boss ? wave / 4 : ordinal) + seed);
                    seen.Add(p != null ? p.id : "NONE");
                }
                ArcanePieceDef piece = null;
                log.AppendLine("MUSICCHECK night " + wave + (boss ? " (boss)" : "") + " | " + scene + " | " + string.Join(" / ", seen));
            }
            foreach (string scene in new[] { "menu", "select", "hub", "shop_gold", "shop_dream", "endless" })
            {
                var all = new System.Collections.Generic.List<string>();
                for (int v = 0; v < 8; v++) { ArcanePieceDef p = ArcaneCueLibrary.PieceForScene(scene, v); if (p != null && !all.Contains(p.id)) all.Add(p.id); }
                log.AppendLine("MUSICCHECK scene " + scene + " | " + string.Join(", ", all));
            }
            Debug.Log(log.ToString());
        }
    }
}
