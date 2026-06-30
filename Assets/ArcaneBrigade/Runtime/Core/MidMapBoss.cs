using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One boss in the middle of each map (Docs/Map_Mechanics_2026-10-25.md): it comes 15 s into its night with about
    /// 40% of the stage's closing-boss health. Beating it keeps that night's memory lamp and gives a prism shard; the wave runs
    /// on to its time. If the night ends first, the boss walks off with the memory.</summary>
    public static class MidMapBossRules
    {
        public const float ArriveAt = 15f, HealthShare = .4f;
        /// <summary>The night is lengthened so the boss can be fought without racing the clock.</summary>
        public const float ExtraSeconds = 15f;
        public static string For(int wave) => wave switch
        {
            3 => "SleepCollector", 8 => "ChoirEngine", 11 => "WaxAdmiral", 13 => "LastLantern", 17 => "MarqueeMaw", 19 => "ScarletIndex", _ => null
        };
        /// <summary>The museum's line for a mid-map boss: where it waits and what it carries off.</summary>
        public static string Journal(string id) => id switch
        {
            "SleepCollector" => "Waiting Room, night 3. It sings their voice to sleep and takes it.",
            "ChoirEngine" => "Post Office, night 8. It howls over every letter so no knock is heard.",
            "WaxAdmiral" => "Glasshouse, night 11. It melts the memories down to candle wax.",
            "LastLantern" => "Lamp Hall, night 13. The last flame, and it wants their face.",
            "MarqueeMaw" => "Theatre, night 17. It swallows their handwriting for its marquee.",
            "ScarletIndex" => "Archive Vault, night 19. It files every name away to be forgotten.",
            _ => null
        };
        public static bool Is(string id) => id is "SleepCollector" or "ChoirEngine" or "WaxAdmiral" or "LastLantern" or "MarqueeMaw" or "ScarletIndex";
        /// <summary>The story boss that closes the stage the wave belongs to (5, 10, 15, 20).</summary>
        public static int ClosingWave(int wave) => (wave + 4) / 5 * 5;
    }

    public sealed partial class CheckpointCatalog
    {
        /// <summary>Filled by RunSession.PrepareCombat on a mid-map boss night; null on every other fight.</summary>
        [NonSerialized] public UnitDefinition MidBoss;
    }

    public sealed partial class CombatSimulation
    {
        private UnitState midBoss;
        public bool MidBossDefeated { get; private set; }

        private void ResetMidBoss() { midBoss = null; MidBossDefeated = false; }

        private void ProcessMidBoss()
        {
            if (catalog.MidBoss == null || catalog.Boss != null || catalog.SecretEncounter > 0 || catalog.WholeKeeper || catalog.KnockDeadline > 0) return;
            if (midBoss == null)
            {
                if (Elapsed < MidMapBossRules.ArriveAt) return;
                midBoss = Boss = Spawn(catalog.MidBoss, Faction.Enemy, ClampPosition(Champion.Position + Vector2.up * 3.4f, catalog.MidBoss.Radius));
                midBoss.EntranceUntil = Elapsed + 1.2f;
                midBoss.NextContact = midBoss.NextAttack = Elapsed + 1.45f;
                midBoss.NextBossPattern = Elapsed + 2.2f;
                return;
            }
            if (MidBossDefeated || midBoss.Alive || !BossDefeated) return;
            MidBossDefeated = true;
            PrismShards++;
            MapSay("MEMORY KEPT", new Color(1f, .84f, .45f));
            MapSay("+1 PRISM SHARD", new Color(.72f, .9f, 1f));
        }

        /// <summary>Capture: the night jumps to just before the boss comes; with defeat, the boss falls at once.</summary>
        public void MidBossCaptureFixture(bool defeat)
        {
            if (!defeat) { Elapsed = Mathf.Max(Elapsed, MidMapBossRules.ArriveAt - .1f); return; }
            if (midBoss != null && midBoss.Alive) ApplyDamage(midBoss, midBoss.Health + 1, Faction.Champion, "capture");
        }
    }

    public sealed partial class RunSession
    {
        private UnitDefinition MidMapBoss(float scaling)
        {
            string id = MidMapBossRules.For(State.Wave);
            UnitDefinition body = id == null ? null : Array.Find(catalog.Bosses, boss => boss != null && boss.Id == id);
            if (body == null) return null;
            UnitDefinition boss = BuildEnemy(body, scaling);
            boss.Health *= MidMapBossRules.HealthShare * WaveCadence.StoryBossBulk(MidMapBossRules.ClosingWave(State.Wave));
            return boss;
        }

        /// <summary>A beaten mid-map boss gives back the lamp that broke this night, or the latest dark one on a night
        /// without a breaking lamp.</summary>
        private void SettleMidMapBoss(CombatSimulation simulation)
        {
            if (simulation == null || !simulation.MidBossDefeated || State.ActOne == null || State.Endless) return;
            int lamp = Array.IndexOf(ActOneCanon.BreakWave, State.Wave);
            if (lamp < 0 || ActOneCanon.LampState(lamp, State.Wave, State.ActOne.RelitMask) != 2) lamp = ActOneCanon.RelightTarget(State.Wave, State.ActOne.RelitMask);
            if (lamp >= 0 && lamp < 7) State.ActOne.RelitMask |= 1 << lamp;
        }
    }
}
