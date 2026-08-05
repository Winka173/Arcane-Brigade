using UnityEngine;
namespace ArcaneBrigade
{
    public enum BondSupportKind { Shield, Heal, DamageShared, Untouchable, Revive, Armor, Wall, SkillCopy, ForceCast, Coin, Reroll, Choice, Dash, PiercingVolley, Slow, Speed, Transform, Page, Crown, Law, Pull, TimeStop, SpiritRealm, Cleanse }
    /// <summary>Rules requests; consumers apply support once. Duration, radius and position also describe the future arena presentation.</summary>
    public readonly struct BondSupportRequest
    {
        public readonly BondSupportKind Kind;
        public readonly UnitState Subject;
        public readonly Vector2 Position;
        public readonly float Amount, Duration, Radius;
        public readonly string Rule;
        public BondSupportRequest(BondSupportKind kind, UnitState subject, float amount, float duration, float radius, string rule)
        { Kind = kind; Subject = subject; Position = subject == null ? Vector2.zero : subject.Position; Amount = amount; Duration = duration; Radius = radius; Rule = rule; }
    }
    public enum BondEdict { Damage, Armor, Haste }
    public enum BondRecordedBonus { Damage, Coin, PageCopy }
    public enum BondLawCause { None, Critical, Kill, Cast }
    public enum BondLawEffect { None, Heal, Shield, Burst }
    [System.Serializable] public struct BondLaw { public BondLawCause Cause; public BondLawEffect Effect; }
    [System.Serializable]
    public sealed class BondRunProgress : ISerializationCallbackReceiver
    {
        public float Damage; public int Waves, GrowthWaves; public bool ExhibitUsed, HatUsed, DoubledDown;
        [System.NonSerialized] public readonly System.Collections.Generic.HashSet<string> Species = new System.Collections.Generic.HashSet<string>(BondTuning.EnemyCapacity, System.StringComparer.Ordinal);
        [SerializeField] private System.Collections.Generic.List<string> savedSpecies = new System.Collections.Generic.List<string>();
        public void OnBeforeSerialize() { savedSpecies.Clear();foreach(string value in Species)savedSpecies.Add(value);savedSpecies.Sort(System.StringComparer.Ordinal); }
        public void OnAfterDeserialize() { Species.Clear();if(savedSpecies!=null)foreach(string value in savedSpecies)Species.Add(value); }
        public BondConstellation Constellation; public BondLaw FirstLaw, SecondLaw; public BondRecordedBonus RecordedBonus;
    }
    public enum BondWeather { Rain, Sun, Wind, Fog }
    public enum BondConstellation { Archer, Bear, Twins }
    public enum BondTreeFruit { Gold, Heal, Damage }
}
