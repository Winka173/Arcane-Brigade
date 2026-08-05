using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    [Flags]
    public enum HeroRuleOptions { None=0, Critical=1, ReturnIfMissed=2, SplitReflections=4, GlobalCopiedHeal=8, ExtendCopiedShield=16, FollowWind=32, BurnExtraStack=64, JuggleStun=128, JuggleHeal=256, Blind=512 }
    public enum HeroRuleShape { Circle, Line, Ring, Arena }
    public enum HeroRuleAction
    {
        Damage, Heal, Shield, Slow, Freeze, Stun, Burn, Poison, Mark, Push, Pull,
        BlockBullets, ReflectBullets, RemoveBullets, DamageBuff, SpeedBuff, SkillSpeed,
        SummonSprout, Signpost, PetalWall, LightLine, Rain, Fleet, Juggle, CopySkill,
        ReviveSooner, RefillBar, Cleanse, Coins, Choice, TraitAbsorbed, TraitSignal,
        PersistentCoals, WindLane, SeedPod, CarouselRing, Beam, Counter, Untargetable
    }
    /// <summary>Rules request only: the combat adapter applies it once; presentation draws the same request.</summary>
    public readonly struct HeroRuleRequest
    {
        public readonly HeroRuleAction Action;
        public readonly HeroRuleOptions Options;
        public readonly HeroRuleShape Shape;
        public readonly string Rule, Species;
        public readonly int SourceId, FriendId, Count;
        public readonly Vector2 Position, End;
        public readonly float Radius, Duration, Value;
        public HeroRuleRequest(HeroRuleAction action,string rule,int source,Vector2 position,float radius=0,float duration=0,float value=0,
            HeroRuleShape shape=HeroRuleShape.Circle,Vector2 end=default,int friendId=0,int count=0,string species=null,HeroRuleOptions options=HeroRuleOptions.None)
        { Options=options; Action=action; Rule=rule; SourceId=source; Position=position; Radius=radius; Duration=duration; Value=value; Shape=shape; End=end; FriendId=friendId; Count=count; Species=species; }
    }
    /// <summary>Run persistence is explicitly passed between waves; wave resets never erase catalogue or growth.</summary>
    [Serializable]
    public sealed class HeroRulesV3RunState : ISerializationCallbackReceiver
    {
        public int WavesSurvived, Purchases, Decree=-1, DecreeUntilWave, AbsorbedCount;
        public bool ClemencyUsed;
        public float ShopSkillGrowth;
        public uint RandomState=HeroRulesV3Tuning.InitialRandom;
        [NonSerialized] public readonly HashSet<string> Species = new HashSet<string>(StringComparer.Ordinal);
        [NonSerialized] public readonly HashSet<string> Relics = new HashSet<string>(StringComparer.Ordinal);
        public List<string> AbsorbedTraits = new List<string>();
        public List<TraitCoalPatch> Coals = new List<TraitCoalPatch>();
        [NonSerialized] public readonly Dictionary<int,int> SeedAges = new Dictionary<int,int>();
        [SerializeField] private List<string> savedSpecies=new List<string>(),savedRelics=new List<string>();
        [SerializeField] private List<int> savedSeedIds=new List<int>(),savedSeedAges=new List<int>();
        public void OnBeforeSerialize()
        {
            savedSpecies.Clear();foreach(string value in Species)savedSpecies.Add(value);savedSpecies.Sort(StringComparer.Ordinal);
            savedRelics.Clear();foreach(string value in Relics)savedRelics.Add(value);savedRelics.Sort(StringComparer.Ordinal);
            savedSeedIds.Clear();foreach(var value in SeedAges)savedSeedIds.Add(value.Key);savedSeedIds.Sort();
            savedSeedAges.Clear();foreach(int id in savedSeedIds)savedSeedAges.Add(SeedAges[id]);
        }
        public void OnAfterDeserialize()
        {
            Species.Clear();Relics.Clear();SeedAges.Clear();
            if(savedSpecies!=null)foreach(string value in savedSpecies)Species.Add(value);
            if(savedRelics!=null)foreach(string value in savedRelics)Relics.Add(value);
            if(savedSeedIds!=null && savedSeedAges!=null)for(int i=0;i<Math.Min(savedSeedIds.Count,savedSeedAges.Count);i++)SeedAges[savedSeedIds[i]]=savedSeedAges[i];
        }
        public string MarginaliaSkill;
        public bool EggBright;
    }
    [Serializable]
    public struct TraitCoalPatch { public Vector2 Position; public float Damage; public int SourceId; }
}
