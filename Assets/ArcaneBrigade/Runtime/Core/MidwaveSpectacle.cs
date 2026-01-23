using UnityEngine;

namespace ArcaneBrigade
{
    public enum SpectacleKind { PaperParade, GildedTrap, WanderingEclipse }
    public sealed class MidwaveSpectacle
    {
        public SpectacleKind Kind;
        public Vector2 Origin;
        public Vector2 Position;
        public float Started;
        public float NextReward;
        public float NextThreat;
        public bool Claimed;
        public bool Dangerous;
        public int RewardDrops;
        public const float Duration = 10;
        public float Age(float time) => Mathf.Max(0, time - Started);
        public bool Active(float time) => time >= Started && time < Started + Duration;
        public static bool Eligible(int wave, bool boss) => wave >= 5 && wave % 2 == 1 && !boss;
        public static SpectacleKind Select(int wave, int seed) => (SpectacleKind)((wave / 2 + (seed % 3 + 3) % 3) % 3);
        public bool Contains(Vector2 point)
        {
            Vector2 offset = point - Position;
            if (Kind == SpectacleKind.PaperParade)
            {
                for (int index = 0; index < 3; index++) if ((offset + Vector2.right * index * 1.2f).sqrMagnitude <= 0.75f * 0.75f) return true;
                return false;
            }
            return Kind == SpectacleKind.GildedTrap ? Mathf.Abs(offset.x) <= 1.15f && Mathf.Abs(offset.y) <= 0.8f : offset.sqrMagnitude <= (Kind == SpectacleKind.PaperParade ? 0.75f * 0.75f : 1.15f * 1.15f);
        }
    }
    public sealed partial class CombatSimulation
    {
        public MidwaveSpectacle Spectacle { get; private set; }
        private bool spectacleCompleted;
        private void ClearSpectacle() { Spectacle = null; spectacleCompleted = false; }
        private void ProcessSpectacle()
        {
            if (!catalog.EnableSpectacles || !MidwaveSpectacle.Eligible(catalog.RunWave, catalog.Boss != null) || spectacleCompleted) return;
            if (Spectacle == null)
            {
                if (Elapsed < catalog.WaveDuration * 0.4f) return;
                float side = (runSeed & 1) == 0 ? -1 : 1;
                Vector2 point = ClampPosition(Champion.Position + new Vector2(side * 3.8f, side * 1.8f), 2);
                Spectacle = new MidwaveSpectacle { Kind = MidwaveSpectacle.Select(catalog.RunWave, runSeed), Origin = point, Position = point, Started = Elapsed, NextReward = Elapsed + 2, NextThreat = Elapsed + 2 };
            }
            MidwaveSpectacle show = Spectacle;
            float age = show.Age(Elapsed);
            if (age >= MidwaveSpectacle.Duration) { spectacleCompleted = true; show.Dangerous = false; return; }
            float travel = Mathf.Clamp01((age - 2) / 8);
            if (show.Kind == SpectacleKind.PaperParade) show.Position = ClampPosition(show.Origin + Vector2.right * Mathf.Lerp(-6, 6, travel), 0.8f);
            else if (show.Kind == SpectacleKind.WanderingEclipse) show.Position = ClampPosition(show.Origin + new Vector2(Mathf.Lerp(-3, 3, travel), Mathf.Sin(travel * Mathf.PI * 2) * 1.1f), 1.2f);
            show.Dangerous = age >= 2 && (show.Kind != SpectacleKind.GildedTrap || Mathf.Repeat(age - 2, 2.4f) < 0.65f);
            bool inside = show.Contains(Champion.Position);
            if (show.Kind == SpectacleKind.GildedTrap && age >= 2 && inside && !show.Claimed)
            {
                show.Claimed = true; DropLoot(LootKind.Chest, show.Position, 10); show.RewardDrops++;
            }
            if (show.Kind == SpectacleKind.PaperParade && Elapsed >= show.NextReward && show.RewardDrops < 12)
            {
                show.NextReward = Elapsed + 0.65f; show.RewardDrops++; DropLoot(LootKind.Coin, show.Position - Vector2.right * 0.9f, 1);
            }
            if (!inside || !show.Dangerous || Elapsed < show.NextThreat || !Champion.Alive) return;
            show.NextThreat = Elapsed + 1;
            ApplyDamage(Champion, Mathf.Min(show.Kind == SpectacleKind.PaperParade ? 5 : show.Kind == SpectacleKind.GildedTrap ? 4 : 2, Champion.Definition.Health * 0.05f), Faction.Enemy, "Midwave " + show.Kind, cause: NewCause(0).Child("spectacle"));
            if (show.Kind == SpectacleKind.WanderingEclipse && show.RewardDrops < 8)
            {
                show.RewardDrops++; DropLoot(LootKind.Coin, show.Position, 2);
            }
        }
    }
}
