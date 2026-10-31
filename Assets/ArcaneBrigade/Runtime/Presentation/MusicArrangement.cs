using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>
    /// Which stems of a layered piece play where (playtest item 2a, music for focus). Stems: 0 base, 1 rhythm, 2 melody, 3 intensity.
    /// Combat (early / mid / late / endless / boss) plays base + rhythm only, so nothing competes with the fight; the melody is
    /// kept for the shop, the menus and the first bars of a boss (its intro). Any new piece with the same four stems follows the same rules.
    /// </summary>
    public static class MusicArrangement
    {
        /// <summary>How long a boss's melody plays at full level before it settles back under base + rhythm.</summary>
        public const float BossIntroSeconds = 8f;

        public static bool IsCombat(string scene) => scene != null && scene.StartsWith("combat_map_", System.StringComparison.Ordinal) || scene == ArcaneCueLibrary.SceneCombatEarly || scene == ArcaneCueLibrary.SceneCombatMid
            || scene == ArcaneCueLibrary.SceneCombatLate || scene == ArcaneCueLibrary.SceneEndless || scene == ArcaneCueLibrary.SceneBoss;

        /// <summary>Stem levels for a scene at an enemy density (0..1); <paramref name="bossIntro"/> lets a boss's melody through.</summary>
        public static void Targets(string scene, float density, bool bossIntro, float[] levels)
        {
            levels[0] = 1f;
            // requirement 27/10/2026: a secret room's piece plays like a shop's, the ending like a menu
            if (scene != null && scene.StartsWith("secret_", System.StringComparison.Ordinal)) { levels[1] = 0.9f; levels[2] = 1f; levels[3] = 0.75f; return; }
            if (scene == ArcaneCueLibrary.SceneEnding) { levels[1] = 0.85f; levels[2] = 1f; levels[3] = 0f; return; }
            switch (scene)
            {
                case ArcaneCueLibrary.SceneMenu:
                case ArcaneCueLibrary.SceneHub: levels[1] = 0.85f; levels[2] = 1f; levels[3] = 0f; break;
                case ArcaneCueLibrary.SceneSelect: levels[1] = 0.7f; levels[2] = 1f; levels[3] = 0f; break;
                case ArcaneCueLibrary.SceneShopGold: levels[1] = 0.9f; levels[2] = 1f; levels[3] = 0.75f; break;
                case ArcaneCueLibrary.SceneShopDream: levels[1] = 0.8f; levels[2] = 1f; levels[3] = 0.7f; break;
                case ArcaneCueLibrary.SceneBoss: levels[1] = 1f; levels[2] = bossIntro ? 1f : .55f; levels[3] = 0f; break; // the melody stays, softer, after the intro (base + rhythm alone read as the music stopping)
                case ArcaneCueLibrary.SceneEndless: levels[1] = Mathf.Lerp(0.75f, 1f, density); levels[2] = 0f; levels[3] = 0f; break;
                default: levels[1] = Mathf.Lerp(0.7f, 1f, density); levels[2] = 0f; levels[3] = 0f; break;
            }
        }
    }
}
