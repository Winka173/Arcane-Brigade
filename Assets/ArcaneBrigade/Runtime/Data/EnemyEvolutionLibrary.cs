using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum EnemyMutation { None, SootPounce, BatCrossfire, BristleRush, RuneSpiral, MothMolting, SirenEcho, DoorSlam, MagnetSalvo, MimicLunge, CurtainCollapse, FurnaceFan, CabinetScissor, KegRebound, RamBackstep, CoinBarrage, PufferEcho, SporeGarden, CombRake, HydraWeave, BellEruption }

    [Serializable]
    public sealed class EnemyEvolutionEntry
    {
        public string Id;
        public string MutationName;
        public string Summary;
        public EnemyMutation Ability;
        public AuthoredMotionSet Normal;
        public AuthoredMotionSet Mutated;
    }

    public sealed class EnemyEvolutionLibrary : ScriptableObject
    {
        public EnemyEvolutionEntry[] Species = Array.Empty<EnemyEvolutionEntry>();
        public Sprite[] BodyFragments = Array.Empty<Sprite>();
        private static EnemyEvolutionLibrary cached;
        public static EnemyEvolutionLibrary Current => cached != null ? cached : cached = Resources.Load<EnemyEvolutionLibrary>("EnemyEvolutionLibrary");
        public static EnemyEvolutionEntry For(string id)
        {
            EnemyEvolutionLibrary library = Current;
            if (library != null) foreach (EnemyEvolutionEntry entry in library.Species) if (entry.Id == id) return entry;
            return null;
        }
        public static int MutationWave(string id)
        {
            uint hash = 2166136261;
            foreach (char letter in id ?? "") hash = unchecked((hash ^ letter) * 16777619);
            return 6 + (int)(hash % 7);
        }
        public static void Apply(UnitDefinition unit, int wave)
        {
            if (unit.IsBoss) return;
            EnemyEvolutionEntry entry = For(unit.Id);
            if (entry == null) return;
            bool mutated = wave >= MutationWave(unit.Id);
            unit.EvolutionStage = mutated ? 1 : 0; unit.Mutation = mutated ? entry.Ability : EnemyMutation.None;
            unit.AuthoredMotion = mutated ? entry.Mutated : entry.Normal;
            unit.Body = unit.AuthoredMotion.Idle[0];
            unit.AnimationFrames = new[] { unit.Body, unit.AuthoredMotion.Idle[1], unit.AuthoredMotion.Walk[0], unit.AuthoredMotion.Walk[1] };
            if (!mutated) return;
            unit.BodyHeight *= 1.12f; unit.Radius *= 1.08f;
            unit.Description = entry.Summary;
        }
        public static Sprite Fragment(UnitDefinition unit, int variant = 0)
        {
            EnemyEvolutionLibrary library = Current;
            if (library == null || library.BodyFragments.Length != 16) return unit.Weapon.ProjectileSprite;
            int family = unit.Mutation switch
            {
                EnemyMutation.SootPounce or EnemyMutation.RuneSpiral => 0,
                EnemyMutation.FurnaceFan or EnemyMutation.KegRebound => 1,
                EnemyMutation.MothMolting or EnemyMutation.CurtainCollapse or EnemyMutation.HydraWeave => 2,
                EnemyMutation.SporeGarden or EnemyMutation.PufferEcho => 3,
                EnemyMutation.DoorSlam or EnemyMutation.CabinetScissor or EnemyMutation.CombRake => 4,
                EnemyMutation.MagnetSalvo or EnemyMutation.CoinBarrage => 5,
                EnemyMutation.BellEruption or EnemyMutation.SirenEcho => 6,
                _ => 7
            };
            if (unit.Mutation == EnemyMutation.None)
                family = unit.Behavior == EnemyBehavior.Sweeper ? 4 : unit.Behavior == EnemyBehavior.Spore ? 3 : unit.Behavior == EnemyBehavior.Hopper ? 1 : unit.Behavior == EnemyBehavior.Burrower ? 6 : 0;
            return library.BodyFragments[family * 2 + Mathf.Abs(variant % 2)];
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Invalidate() => cached = null;
    }
}
