using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public static class BondV5Tuning
    {
        public const int HighBurnStacks=5,EarlyMarks=5,PoisonDeathBonus=1,BlizzardChill=3,Double=2,StarSplit=3,MulticastEvery=10,BeatStackCap=3,MassPerSecond=10,SoulsPerSecond=5;
        public const int WarmthCap=10,HighWarmthCap=20,DishWarmth=3,FeastWarmth=1,PhoenixHeat=50,PandemicToxin=200,ShadeCap=5,ChapterPages=3,SugarCap=5,CogCap=10,VerseCap=5;
        public const float QuicksolesMove=.06f,LaneRadius=.5f,LaneLength=6;
        public const float CloudSeconds=2,CloudSlow=.3f,SkillStunBonus=.15f,UnstoppableDamage=.05f,UnstoppableSeconds=4,SniperCarryDistance=8,ReturnStrength=.6f,BounceGrowth=.15f;
        public const int UnstoppableStacks=3;
        public const float RodPeriod=15,RodSeconds=6,SteamMiss=.3f,AfterHoleRadiusScale=.5f,PlantLinkRadius=4,PlantRootSeconds=1,ShadeExecuteDamage=.1f;
        public const int WavesPerPlantStage=2,WispCount=3;
        public const float WispOrbitRadius=2,FullCircle=6.2831853f;
        public static readonly int[] DieRank={4,3,2,1,5,0,6};
        public const float BaseCritMultiplier=1.5f;
        public const float EdictRegeneration=.01f,PageRecordedBonus=.1f;
        public const float OrchestraDamage=1,OrchestraShield=.08f,HeatFadeSeconds=5;
        public const float BurstCooldown=2,PoolSeconds=4,LaneSeconds=3,StardustSeconds=4,StarChance=.15f,PoisonWeakness=.1f,SpringSeconds=.1f,MinHoleCooldown=1,SpiritRiseSeconds=11;
        public const float WarmthStat=.02f,FeastFill=.1f,DishPeriod=8,BulwarkMemberShield=.25f,BulwarkTeamShield=.1f,BeatSpeed=.15f,WindSpeed=.15f,GuardThreshold=100;
        public const float IceSeconds=3,WickRadius=.5f,WickCap=3,CrackBonus=.15f,MaskShardDamage=.4f,GlyphBonus=.1f,GlyphSeconds=4,HaymakerDamage=1.5f;
        public const int HaymakerEvery=4;
        public const float SugarSpeed=.02f,SugarFill=.3f,SugarSeconds=6,CogFill=.2f,ChorusDamage=.15f,ChorusSeconds=6,JackpotDamage=.15f,FinaleSeconds=5,ShardCrit=.01f,ShardCritCap=.1f;
        public const float ShieldDamage=.1f,FrostMemberDamage=.3f,RootSeconds=.5f,CrushDamage=.6f,TrailDamage=.15f,TrailSeconds=3,BladeSeconds=2,DisplaySeconds=5,ReviveBase=15;
        public const int CollisionCrack=3,MaskShardCount=3,RunRerolls=10,HatTransform=3,CloversBloom=4;
        public const float OverhealShield=.1f,StardustCrit=.15f,CelestialCrit=.25f,CelestialCritDamage=.5f;
    }
    public readonly struct BondZoneRequest
    {
        public readonly string Rule; public readonly Vector2 Position,End; public readonly float Radius,Duration; public readonly int SourceId;
        public BondZoneRequest(string rule,Vector2 position,Vector2 end,float radius,float duration,int sourceId) {Rule=rule;Position=position;End=end;Radius=radius;Duration=duration;SourceId=sourceId;}
    }
    public sealed partial class BondRuntime
    {
        private struct RapidState{public int Stacks;public float At;}
        private readonly Dictionary<int,RapidState> rapidByFriend=new Dictionary<int,RapidState>(BondTuning.TeamCapacity);
        private struct RuleZone {public string Rule;public Vector2 Position,End;public float Until;public UnitState Source;}
        private readonly List<RuleZone> zones=new List<RuleZone>(BondTuning.RequestCapacity);
        private readonly List<UnitState> infernoNeighbors=new List<UnitState>(BondTuning.EnemyCapacity), stormChainTargets=new List<UnitState>(BondTuning.EnemyCapacity);
        private readonly Queue<UnitState> dischargeQueue=new Queue<UnitState>(BondTuning.EnemyCapacity);
        private int dischargeWave;
        private readonly Queue<UnitState> burstQueue=new Queue<UnitState>(BondTuning.EnemyCapacity);
        private readonly HashSet<string> pageSpecies=new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<int,int> tideHitCounts=new Dictionary<int,int>(BondTuning.TeamCapacity),stormHitCounts=new Dictionary<int,int>(BondTuning.TeamCapacity),windVolleyCounts=new Dictionary<int,int>(BondTuning.TeamCapacity);
        private readonly Dictionary<int,float> monolithArmor=new Dictionary<int,float>(BondTuning.TeamCapacity);
        private readonly Dictionary<int,float> jackpotSkillTimes=new Dictionary<int,float>(BondTuning.TeamCapacity);
        private readonly Dictionary<int,float> stolenByFriend=new Dictionary<int,float>(BondTuning.TeamCapacity);
        private readonly Dictionary<int,int> chargeStacks=new Dictionary<int,int>(BondTuning.TeamCapacity);
        private readonly Dictionary<int,float> chargeUntil=new Dictionary<int,float>(BondTuning.TeamCapacity);
        private bool drainingBursts,phoenixUsed,pandemicUsed,displayUsed,leviathanUsed;private int shades,springs,beatStacks,pages,sugar,cogs,verses;
        private int heldClovers {get=>runProgress.HeldClovers;set=>runProgress.HeldClovers=value;}
        public int HeldClovers=>heldClovers;
        private float shadeDamage,celestialUntil;
        public int PlantStage=>growthWaves/BondV5Tuning.WavesPerPlantStage+(Active("orchard",8)?1:0);
        private float rodAt=BondV5Tuning.RodPeriod;
        private float dishReadyAt,wickRadius,zonePulseAt,finaleUntil,sugarUntil,chorusUntil,maskCrit; private UnitState lastCastFriend;
        public int Sugar=>sugar;public int Cogs=>cogs;public int Pages=>pages;public int Springs=>springs;
        public float LightRadius=>BondTuning.LanternRadius+wickRadius;
        public event Action<BondZoneRequest> ZoneRequested;
        private void ResetV5(){zones.Clear();rapidByFriend.Clear();dischargeQueue.Clear();dischargeWave=0;tideHitCounts.Clear();stormHitCounts.Clear();windVolleyCounts.Clear();jackpotSkillTimes.Clear();stolenByFriend.Clear();chargeStacks.Clear();chargeUntil.Clear();burstQueue.Clear();pageSpecies.Clear();drainingBursts=phoenixUsed=pandemicUsed=displayUsed=leviathanUsed=false;shadeDamage=celestialUntil=0;shades=springs=beatStacks=pages=sugar=cogs=verses=0;dishReadyAt=wickRadius=finaleUntil=sugarUntil=chorusUntil=maskCrit=0;zonePulseAt=BondTuning.PlaguePeriod;lastCastFriend=null;rodAt=BondV5Tuning.RodPeriod;RefreshMonolithArmor();}
        private void AddZone(string rule,UnitState source,float duration,UnitState attacker=null)
        { if(source==null)return; if(zones.Count>=BondTuning.RequestCapacity)zones.RemoveAt(0);zones.Add(new RuleZone{Rule=rule,Position=source.Position,End=source.Position+source.Facing*BondV5Tuning.LaneLength,Until=time+duration,Source=attacker??source});ZoneRequested?.Invoke(new BondZoneRequest(rule,source.Position,source.Position+source.Facing*BondV5Tuning.LaneLength,ZoneRadius(rule),duration,source.Id)); }
        private static bool IsLane(string rule)=>rule=="wind-lane"||rule=="quicksoles-trail"||rule=="lane-cyclone";
        private float ZoneRadius(string rule)=>IsLane(rule)?BondV5Tuning.LaneRadius:BondTuning.BurstRadius*(rule=="ice-patch"&&Active("frost",8)?BondV5Tuning.Double:rule=="after-hole"?BondV5Tuning.AfterHoleRadiusScale:1);
        private bool ZoneContains(RuleZone zone,Vector2 position)=>IsLane(zone.Rule)?CompanionSkillRuntime.Contains(SkillShape.Line,zone.Position,zone.End,ZoneRadius(zone.Rule),position):(zone.Position-position).sqrMagnitude<=ZoneRadius(zone.Rule)*ZoneRadius(zone.Rule);
        private bool InZone(string rule,Vector2 position){foreach(RuleZone zone in zones)if(zone.Rule==rule&&zone.Until>time&&ZoneContains(zone,position))return true;return false;}
        private float WarmthBonus=>Active("hearth",4)?Warmth*BondV5Tuning.WarmthStat:0;
        private float MealBonusFor(UnitState unit)=>RuleApplies(unit,"hearth")?BondV5Tuning.BeatSpeed*Mathf.Clamp01(1-time/BondTuning.MealFade):0;
        public float StolenDamageFor(UnitState unit)=>unit!=null&&stolenByFriend.TryGetValue(unit.Id,out float amount)?amount:0;
        public float HealthGrowthFor(UnitState unit)=>growthWaves*BondTuning.GrowthPerWave*(Member(unit,"orchard")?1:Active("orchard",4)?BondTuning.CopyStrength:0);
        public float RangeBonusFor(UnitState unit)=>ConstellationRange+(RuleApplies(unit,"lantern-guild")&&still>=BondTuning.StillSeconds?BondTuning.LanternBonus:0);
        public float AttackSpeedBonusFor(UnitState unit)=>MealBonusFor(unit)+WarmthBonus+(RuleApplies(unit,"windborne")?momentum:0)+(RuleApplies(unit,"melody")&&time<beatUntil?beatStacks*BondV5Tuning.BeatSpeed:0)+(time<shantyUntil?verses*BondTuning.ShantySpeed:0)+sugar*BondV5Tuning.SugarSpeed+(Member(unit,"rapidfire")?RapidCount(unit)*(Active("rapidfire",4)?BondTuning.RapidHighSpeed:BondTuning.RapidSpeed):0);
        public float SkillFillBonusFor(UnitState unit)=>(RuleApplies(unit,"regal")?EdictSkillFill:0)+(RuleApplies(unit,"clock")?BondV5Tuning.BulwarkMemberShield:0)+(Active("clock",8)&&time>=stopUntil&&time<overclockUntil?1:0)+(time<sugarUntil?BondV5Tuning.SugarFill:0);
        public float ProjectileDistanceBonusFor(UnitState unit)=>Member(unit,"paper-wings")?ProjectileDistanceBonus:0;
        public int BallBouncesFor(UnitState unit)=>Member(unit,"ball")?BallBounces:0;
        public int BoomerangReturnsFor(UnitState unit)=>Member(unit,"paper-wings")?BoomerangReturns:0;
        public float ArmorIgnoredFor(UnitState unit)=>Member(unit,"bludgeoner")?ArmorIgnored:0;
        public float BounceDamageScale(UnitState unit,int bounce)=>Member(unit,"ball")?BondTuning.BallRetention+(Active("ball",4)?Mathf.Max(0,bounce-1)*BondV5Tuning.BounceGrowth:0):1;
        public float ReturningDamageScale(UnitState unit)=>Member(unit,"paper-wings")?BondV5Tuning.ReturnStrength:1;
        public int ProjectilePierceFor(UnitState unit)=>(Member(unit,"rapidfire")&&Active("rapidfire",4)&&RapidCount(unit)>=BondTuning.RapidStackCap?1:0);
        public int ExtraProjectilesFor(UnitState unit)=>Member(unit,"windborne")&&Active("windborne",6)&&momentum>=BondV5Tuning.WindSpeed?1:0;
        public float EdictArmorFor(UnitState unit)=>RuleApplies(unit,"regal")?EdictArmor:0;
        public float ArmorBonusFor(UnitState unit)=>ArmorBonus;
        public float MoveSpeedBonusFor(UnitState unit)=>FriendMoveSpeedBonus+(Member(unit,"shoe")?BondV5Tuning.QuicksolesMove:0);
        public float CriticalBonusFor(UnitState unit)=>(time<celestialUntil?BondV5Tuning.CelestialCrit:0)+(Member(unit,"mask")?maskCrit:0)+(InZone("stardust",unit.Position)?BondV5Tuning.StardustCrit:0);
        public float ZoneSpeedScale(UnitState enemy)=>(InZone("cloud-puff",enemy.Position)||InZone("carnival-slow",enemy.Position))?1-BondV5Tuning.CloudSlow:1;
        public float ChargeDamageFor(UnitState unit)=>unit!=null&&chargeUntil.TryGetValue(unit.Id,out float until)&&until>time&&chargeStacks.TryGetValue(unit.Id,out int stacks)?stacks*BondV5Tuning.UnstoppableDamage:0;
        public void OnCharge(UnitState friend){chargeStacks.TryGetValue(friend.Id,out int count);chargeStacks[friend.Id]=Mathf.Min(BondV5Tuning.UnstoppableStacks,count+1);chargeUntil[friend.Id]=time+BondV5Tuning.UnstoppableSeconds;}
        public bool IsEnemyMarked(UnitState enemy)=>State(enemy)?.Marked==true||Active("dusk",8)&&time>=BondTuning.EclipseStart;
        public Vector2 BondAllyPosition(string kind,int sourceId,Vector2 previous)
        {
            if(hero==null)return previous;
            if(kind=="LampWisp"){float angle=sourceId*BondV5Tuning.FullCircle/BondV5Tuning.WispCount+(still>=BondTuning.StillSeconds?0:time);float radius=still>=BondTuning.StillSeconds?LightRadius:BondV5Tuning.WispOrbitRadius;return hero.Position+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;}
            if(kind=="WindBeast"){foreach(RuleZone zone in zones)if(zone.Rule=="wind-lane"&&zone.Until>time)return Vector2.Lerp(zone.Position,zone.End,Mathf.PingPong(time/BondV5Tuning.LaneLength,1));}
            return previous;
        }
        private bool LampLights(Vector2 position){if(!Active("lantern-guild",6)||still<BondTuning.StillSeconds)return false;for(int lamp=0;lamp<BondV5Tuning.WispCount;lamp++)if((position-BondAllyPosition("LampWisp",lamp,hero.Position)).sqrMagnitude<=LightRadius*LightRadius)return true;return false;}
        public float EnemyMissChance(UnitState enemy)=>enemy!=null&&InZone("steam",enemy.Position)?BondV5Tuning.SteamMiss:0;
        public float BondAllyDamageScale(string kind,int sourceId)=>kind=="Orchestra"?0:kind=="Shade"?1+shadeDamage:kind=="LampWisp"?1+wickRadius:kind=="Sprout"||kind=="Sapling"||kind=="Treant"||kind=="GreatTree"?1+PlantStage*BondTuning.ChoiceBonus:1;
        public void OnAllyKill(string kind,UnitState enemy){if(kind=="Chef"&&Active("hearth",8))Spawn(new BondSpawnRequest(BondSpawnKind.Dish,enemy.Position));if(kind=="Spirit"&&Active("spirit",4))souls++;if(kind=="Shade"&&Active("dusk",6)){foreach(UnitState next in Nearby(enemy,BondTuning.BurstRadius))if(!State(next).Marked){Mark(next,bestFriend);break;}}}
        private void Riptide(){foreach(var pair in enemies)if(pair.Key.Alive){Vector2 destination=pair.Key.Position;float nearest=float.PositiveInfinity;foreach(RuleZone zone in zones)if(zone.Rule=="tide-pool"&&zone.Until>time){float square=(zone.Position-pair.Key.Position).sqrMagnitude;if(square<nearest){nearest=square;destination=zone.Position;}}if(nearest<float.PositiveInfinity)pair.Key.Position=Vector2.MoveTowards(pair.Key.Position,destination,BondTuning.RiptidePush);}Emit("riptide",hero);}
        private void Earthquake(){StunArea(hero,BondTuning.EarthquakeStun,"earthquake",false);enemyTickBuffer.Clear();foreach(var pair in enemies)if(pair.Key.Alive&&pair.Value.Collisions>=BondV5Tuning.CollisionCrack)enemyTickBuffer.Add(pair.Key);foreach(UnitState cracked in enemyTickBuffer){UnitState next=null;foreach(UnitState near in Nearby(cracked,BondTuning.BurstRadius))if(near!=cracked){next=near;break;}if(next!=null){cracked.Position=next.Position;OnCollision(cracked,next);}}}
        private void RuleTeamRequest(string id,BondSupportKind kind,float value,float duration,string rule){if(hero!=null&&RuleApplies(hero,id))Request(kind,hero,value,duration,rule);foreach(UnitState friend in friends)if(friend.Alive&&RuleApplies(friend,id))Request(kind,friend,value,duration,rule);}
        private bool EveryUnit(string kind,UnitState friend,int interval){if(friend==null)return false;Dictionary<int,int> counters=kind=="tide"?tideHitCounts:kind=="storm"?stormHitCounts:windVolleyCounts;counters.TryGetValue(friend.Id,out int count);counters[friend.Id]=++count;return count%interval==0;}
        private Vector2 AlternateClusterPosition(){UnitState primary=DensestCluster(),best=null;int most=-1;if(primary==null)return hero.Position;foreach(var pair in enemies)if(pair.Key.Alive&&(pair.Key.Position-primary.Position).sqrMagnitude>BondTuning.BurstRadius*BondTuning.BurstRadius){int count=0;foreach(var other in enemies)if(other.Key.Alive&&(pair.Key.Position-other.Key.Position).sqrMagnitude<=BondTuning.BurstRadius*BondTuning.BurstRadius)count++;if(count>most){best=pair.Key;most=count;}}return (best??primary).Position;}
        private void GaleLanes(){for(int i=0;i<zones.Count;i++){RuleZone zone=zones[i];if(zone.Rule!="wind-lane"||zone.Until<=time)continue;zone.Rule="lane-cyclone";zone.Until=time+BondTuning.GaleSeconds;zones[i]=zone;ZoneRequested?.Invoke(new BondZoneRequest("lane-cyclone",zone.Position,zone.End,ZoneRadius(zone.Rule),BondTuning.GaleSeconds,zone.Source.Id));}}
        private int RapidCount(UnitState friend)=>friend!=null&&rapidByFriend.TryGetValue(friend.Id,out RapidState state)&&time-state.At<BondTuning.RapidIdle?state.Stacks:0;
        private void OnRapidHit(UnitState friend){int stacks=RapidCount(friend);rapidByFriend[friend.Id]=new RapidState{Stacks=Mathf.Min(BondTuning.RapidStackCap,stacks+1),At=time};}
        private void AddWarmth(int amount){Warmth=Mathf.Min(Active("hearth",8)?BondV5Tuning.HighWarmthCap:BondV5Tuning.WarmthCap,Warmth+amount);Emit("warmth",hero,Warmth);}
        public void OnPickup(BondSpawnKind kind)
        {
            if(kind==BondSpawnKind.Dish&&Active("hearth",4))AddWarmth(BondV5Tuning.DishWarmth);
            if(kind==BondSpawnKind.Snack&&Active("sweet-tooth")){sugar=Mathf.Min(BondV5Tuning.SugarCap,sugar+1);if(Active("sweet-tooth",4)&&sugar==BondV5Tuning.SugarCap){sugar=0;sugarUntil=time+BondV5Tuning.SugarSeconds;Emit("sugar-rush",hero,BondV5Tuning.SugarSeconds);if(Active("sweet-tooth",6))foreach(UnitState friend in friends)if(friend.Alive&&Has(friend,"sweet-tooth")){UnitState cluster=DensestCluster();if(cluster!=null)Area(cluster,friend,Weapon(friend),"candy-volley");Request(BondSupportKind.Coin,friend,1,0,"sugar-coin");}}}
            if(kind==BondSpawnKind.Clover&&Active("clover",4)&&++heldClovers>=BondV5Tuning.CloversBloom){heldClovers=0;runProgress.PendingFourLeaves++;Request(BondSupportKind.Choice,hero,1,0,"four-leaf-rarity");}
        }
        public void OnOverheal(UnitState friend,float amount){if(amount<=0||friend==null)return;if(Active("hearth",6)&&Warmth>=BondV5Tuning.WarmthCap)TeamRequest(BondSupportKind.Shield,amount/Mathf.Max(1,friend.Definition.Health),0,"warmth-overflow");if(Active("mend",4))Request(BondSupportKind.Shield,friend,Mathf.Min(BondV5Tuning.OverhealShield,amount/Mathf.Max(1,friend.Definition.Health)),0,"patch-shield");}
        private void RuneStrike(UnitState enemy,UnitState source,float weapon){bool detonate=Active("rune",6)&&InZone("glyph",enemy.Position);Area(enemy,source,weapon*BondTuning.RuneDamage*(detonate?BondV5Tuning.StarSplit:1),"rune",true);if(Active("rune",4)){if(detonate){for(int i=zones.Count-1;i>=0;i--)if(zones[i].Rule=="glyph"&&(zones[i].Position-enemy.Position).sqrMagnitude<=BondTuning.BurstRadius*BondTuning.BurstRadius)zones.RemoveAt(i);}else AddZone("glyph",enemy,BondV5Tuning.GlyphSeconds,source);}}
        private void MaskShards(UnitState enemy,UnitState source){if(!Active("mask",4))return;List<UnitState> targets=Nearby(enemy,BondTuning.BurstRadius);for(int i=0;i<Mathf.Min(BondV5Tuning.MaskShardCount,targets.Count);i++){Damage(targets[i],source,Weapon(source)*BondV5Tuning.MaskShardDamage,"mask-shard");if(Active("mask",6))maskCrit=Mathf.Min(BondV5Tuning.ShardCritCap,maskCrit+BondV5Tuning.ShardCrit);}}
        private void QueueBurst(UnitState enemy){BondEnemyState state=State(enemy);if(!Active("ember",4)||state.Burn<(Active("ember",6)?BondV5Tuning.HighBurnStacks:BondTuning.BurnStacks)||state.BurstReadyAt>time)return;state.BurstReadyAt=time+BondV5Tuning.BurstCooldown;burstQueue.Enqueue(enemy);}
        private void DrainBursts()
        {
            if(drainingBursts)return;drainingBursts=true;
            while(burstQueue.Count>0){UnitState enemy=burstQueue.Dequeue();BondEnemyState state=State(enemy);bool fuel=Active("ember",6)&&state.Burn>=BondV5Tuning.HighBurnStacks;state.Burn=0;Heat++;Area(enemy,state.BurnSource,state.BurnWeapon*BondTuning.BurstDamage*(fuel?BondV5Tuning.Double:1),"burn-burst");if(time<infernoUntil){infernoNeighbors.Clear();foreach(UnitState nearby in Nearby(enemy,BondTuning.BurstRadius))infernoNeighbors.Add(nearby);foreach(UnitState nearby in infernoNeighbors)Area(nearby,state.BurnSource,state.BurnWeapon*BondTuning.BurstDamage,"inferno-burst");}if(fuel){patches.Add(new EmberPatch{Position=enemy.Position,Until=time+BondTuning.EmberSeconds,Source=state.BurnSource,Weapon=state.BurnWeapon});EmbersCreated?.Invoke(enemy.Position,BondTuning.EmberSeconds);}foreach(UnitState next in Nearby(enemy,BondTuning.BurstRadius)){BondEnemyState other=State(next);other.Burn=Mathf.Min(Active("ember",6)?BondV5Tuning.HighBurnStacks:BondTuning.BurnStacks,other.Burn+1);other.BurnUntil=time+BondTuning.BurnSeconds;other.BurnSource=state.BurnSource;other.BurnWeapon=state.BurnWeapon;QueueBurst(next);}}
            drainingBursts=false;
        }
        private void Shatter(UnitState enemy,UnitState source){if(!Active("frost",4))return;Area(enemy,source,Weapon(source)*BondTuning.ShatterDamage,"shatter");foreach(UnitState next in Nearby(enemy,BondTuning.BurstRadius)){BondEnemyState state=State(next);state.Chill=Mathf.Min(BondTuning.ChillStacks,state.Chill+BondTuning.WinterStacks);}if(Active("frost",6))AddZone("ice-patch",enemy,BondV5Tuning.IceSeconds);}
        private void Mark(UnitState enemy,UnitState source){BondEnemyState state=State(enemy);if(state==null||state.Marked)return;state.Marked=true;state.MarkSource=source;marked++;if(source!=null){stolenByFriend.TryGetValue(source.Id,out float stolen);stolenByFriend[source.Id]=stolen+Mathf.Max(Weapon(enemy),enemy.Definition.ContactDamage)*BondTuning.StrengthSteal;}StrengthStolen?.Invoke(enemy,source,BondTuning.StrengthSteal);Emit("dusk-mark",enemy,BondTuning.StrengthSteal);}
        private void PassMark(UnitState enemy){BondEnemyState state=State(enemy);if(!IsEnemyMarked(enemy)||!Active("dusk",4))return;if(state.Marked)marked--;foreach(UnitState next in Nearby(enemy,float.PositiveInfinity))if(!State(next).Marked){Mark(next,state.MarkSource);break;}if(shades<BondV5Tuning.ShadeCap){shades++;Spawn(new BondSpawnRequest(BondSpawnKind.Shade,enemy.Position,enemy.Id,BondTuning.ShadeSeconds,Weapon(state.MarkSource)));}}
        private void StormChain(UnitState enemy,UnitState source,float weapon)
        {
            bool rod=Active("static-club",6)&&InZone("lightning-rod",enemy.Position);
            dischargeWave++;stormChainTargets.Clear();
            // a chain leaps: from the struck enemy to the nearest enemy it has not struck yet, then on from that one, each leap at
            // most ChainJump long, so it visibly runs through a crowd instead of striking scattered enemies at once
            int targets=BondTuning.ChainTargets*(rod?BondV5Tuning.Double:1);UnitState from=enemy;
            for(int i=0;i<targets;i++)
            {
                UnitState next=null;
                foreach(UnitState candidate in Nearby(from,BondTuning.ChainJump))if(candidate!=enemy&&!stormChainTargets.Contains(candidate)){next=candidate;break;}
                if(next==null)break;
                stormChainTargets.Add(next);Damage(next,source,weapon*BondTuning.ChainDamage,"storm-chain");
                if(Active("static-club",4))AddCharge(next,rod?BondV5Tuning.Double:1);
                from=next;
            }
            // A discharge is a chain hit; a wave of discharges visits each enemy at most once.
            while(dischargeQueue.Count>0)
            {
                UnitState next=dischargeQueue.Dequeue();State(next).Charge-=BondTuning.DischargeStacks;
                Area(next,source,weapon*BondTuning.DischargeDamage,"discharge");
                foreach(UnitState neighbor in Nearby(next,BondTuning.BurstRadius))AddCharge(neighbor,1);
                if(Active("static-club",8))AddZone("lightning-rod",next,BondV5Tuning.LaneSeconds);
            }
            Emit("storm-chain",enemy,weapon*BondTuning.ChainDamage);
        }
        private void AddCharge(UnitState enemy,int amount)
        {
            BondEnemyState state=State(enemy);state.Charge+=amount;Charge+=amount;
            if(state.Charge<BondTuning.DischargeStacks||state.DischargeWave==dischargeWave)return;
            state.DischargeWave=dischargeWave;dischargeQueue.Enqueue(enemy);
        }
        public void OnAllyHit(string kind,UnitState enemy){if(kind=="Phoenix"&&Active("ember",8))for(int i=0;i<patches.Count;i++){EmberPatch patch=patches[i];if((patch.Position-enemy.Position).sqrMagnitude<=BondTuning.BurstRadius*BondTuning.BurstRadius){patch.Until=time+BondTuning.EmberSeconds;patches[i]=patch;EmbersCreated?.Invoke(patch.Position,BondTuning.EmberSeconds);}}if(kind=="Colossus"&&Active("stone",8))Earthquake();if(kind=="Shade"&&Active("dusk",6)&&State(enemy)!=null&&!State(enemy).Marked)Mark(enemy,bestFriend);if(kind=="Sapling"||kind=="Treant"||kind=="GreatTree"){BondEnemyState state=State(enemy);if(state!=null)state.RootedUntil=Mathf.Max(state.RootedUntil,time+BondV5Tuning.PlantRootSeconds);}}
        private void OnBeat(UnitState friend){if(!Active("melody",8))return;UnitState cluster=DensestCluster();if(cluster!=null)Area(cluster,friend,Weapon(friend)*BondV5Tuning.OrchestraDamage,"orchestra-damage-note");TeamRequest(BondSupportKind.Shield,BondV5Tuning.OrchestraShield,0,"orchestra-shield-note");TeamRequest(BondSupportKind.Speed,BondV5Tuning.BeatSpeed,BondTuning.BeatSeconds,"orchestra-speed-note");}
        private void AddJackpot(UnitState source){if(++jackpot<BondTuning.JackpotCasts)return;jackpot=0;Request(BondSupportKind.Coin,hero,BondTuning.JackpotCoins,0,"jackpot");TeamRequest(BondSupportKind.DamageBuff,BondV5Tuning.JackpotDamage,BondTuning.JackpotSeconds,"jackpot-buff");if(Active("oddity",8)){finaleUntil=time+BondV5Tuning.FinaleSeconds;foreach(UnitState friend in friends)if(friend.Alive)Request(BondSupportKind.ForceCast,friend,1,0,"grand-finale");}}
        public float ProjectileZoneDamage(Vector2 position)=>InZone("quicksoles-trail",position)?1+BondV5Tuning.TrailDamage:1;
        public int ProjectileZonePierce(Vector2 position)=>InZone("wind-lane",position)?1:0;
        public float ProjectileZoneSpeed(Vector2 position)=>InZone("wind-lane",position)?1+BondTuning.GaleSpeed:1;
        public float HatDamageFor(UnitState friend)=>HatCount(friend)*BondTuning.ChoiceBonus;
        private void HitV5(UnitState attacker,UnitState enemy,float damage,bool critical)
        {
            BondEnemyState state=State(enemy);
            if(Active("root",4)&&Member(attacker,"root")&&still>=BondTuning.StillSeconds){state.RootedUntil=Mathf.Max(state.RootedUntil,time+BondV5Tuning.RootSeconds);Emit("root-hit",enemy,BondV5Tuning.RootSeconds);}
            if(Member(attacker,"gauntlet")&&Active("gauntlet",4)&&grit>=BondTuning.GritHighCap&&Every("haymaker",Active("gauntlet",6)?BondV5Tuning.HaymakerEvery:1)){Area(enemy,attacker,Weapon(attacker)*BondV5Tuning.HaymakerDamage,"haymaker");if(!Active("gauntlet",6))grit=0;}
            if(Active("horn",6)&&Member(attacker,"horn")&&state.StunnedUntil>time)Emit("horns-vulnerability",enemy,BondTuning.ChoiceBonus*BondV5Tuning.Double);
            if(Active("bludgeoner",4)&&Member(attacker,"bludgeoner"))Request(BondSupportKind.ArmorBreak,enemy,1,0,"bludgeoner-break");
            if(Member(attacker,"bladenado")&&Active("bladenado",4))AddZone("spinning-blade",enemy,BondV5Tuning.BladeSeconds,attacker);
            if(Active("tidal",6)&&state.SoakedUntil>time){if(state.Burn>0)AddZone("steam",enemy,BondV5Tuning.PoolSeconds);if(state.Chill>0){state.FrozenUntil=time+BondTuning.FreezeSeconds;state.FrozenHitReady=true;}if(state.Charge>0)Area(enemy,attacker,Weapon(attacker)*BondTuning.ChainDamage,"pool-shock");}
            if(Active("sea-shanty",4)&&time<chorusUntil&&Member(attacker,"sea-shanty"))Area(enemy,attacker,damage*BondTuning.CopyStrength,"shanty-splash");
            if(Active("gravity",4)&&state.ClusterNeighbors>=BondTuning.ClusterMinimum&&RuleApplies(attacker,"gravity")){Mass++;Area(enemy,attacker,Weapon(attacker)*BondTuning.CopyStrength,"gravity-clump");}
        }
        private int HatCount(UnitState friend){if(friend?.Definition?.Id==null)return 0;foreach(BondHatProgress hat in runProgress.Hats)if(hat.Species==friend.Definition.Id)return hat.Count;return 0;}
        public bool IsCrowned(UnitState friend){if(friend==null||!Active("regal",4))return false;UnitState first=HighestTierFriend();if(friend==first)return true;if(!Active("regal",8))return false;UnitState second=null;foreach(UnitState other in friends)if(other!=first&&(second==null||other.Definition.CombatTier>second.Definition.CombatTier||other.Definition.CombatTier==second.Definition.CombatTier&&other.Id<second.Id))second=other;return friend==second;}
        public void OnRecruit(UnitState friend){OnRecruit(friend?.Definition?.Id);}
        public void OnRecruit(string species){if(species==null||!Active("hat",4))return;foreach(BondHatProgress hat in runProgress.Hats)if(hat.Species==species){hat.Count++;return;}runProgress.Hats.Add(new BondHatProgress{Species=species,Count=1});}
        public void OnFriendShieldAbsorbed(float damage){if(Active("bulwark",4))FillGuard(damage);}
        public void OnWallAbsorbed(float damage){if(Active("bulwark",6))FillGuard(damage*BondV5Tuning.Double);}
        private void FillGuard(float amount){Guard+=Mathf.Max(0,amount);if(Guard<BondV5Tuning.GuardThreshold)return;Guard-=BondV5Tuning.GuardThreshold;UnitState cluster=DensestCluster();if(cluster!=null){Area(cluster,bestFriend,Weapon(bestFriend),"guard-shockwave");StunArea(cluster,BondTuning.HornStun,"guard-line",true);}}
        private void KillV5(UnitState enemy)
        {
            if(Active("venom",8)&&State(enemy).Poison>=BondTuning.RotStacks)AddZone("rot-cloud",enemy,BondTuning.PoisonSeconds,State(enemy).PoisonSource);
            if(Active("hearth",4)&&time>=dishReadyAt){dishReadyAt=time+BondV5Tuning.DishPeriod;Spawn(new BondSpawnRequest(BondSpawnKind.Dish,enemy.Position));}
            if(Active("lantern-guild",4)&&LitBonus(enemy)>0){wickRadius=Mathf.Min(BondV5Tuning.WickCap,wickRadius+BondV5Tuning.WickRadius);Emit("wick",enemy,wickRadius);}
            if(Active("archive",4)&&pageSpecies.Add(enemy.Definition.Id??"")){int drops=Active("archive",8)?BondV5Tuning.Double:1;if(Active("archive",6)){pages+=drops;if(pages>=BondV5Tuning.ChapterPages){pages-=BondV5Tuning.ChapterPages;for(int page=0;page<BondV5Tuning.ChapterPages;page++)Request(BondSupportKind.Page,enemy,BondTuning.CopyStrength,0,"chapter");}}else for(int page=0;page<drops;page++)Request(BondSupportKind.Page,enemy,BondTuning.CopyStrength,0,"record-page");}
        }
        public void OnBonusShotHit(UnitState friend){if(!Active("gear",4))return;if(++cogs>=BondV5Tuning.CogCap){cogs=0;TeamRequest(BondSupportKind.SkillFill,BondV5Tuning.CogFill,0,"cog-fill");}}
        public void OnDodge(UnitState friend){if(Active("cloud",4))AddZone("cloud-puff",friend,BondV5Tuning.CloudSeconds);}
        public void OnShieldBroken(UnitState friend){if(!Active("shield",4))return;StunArea(friend,BondTuning.HornStun,"shield-burst",true);if(Active("shield",6))Area(friend,friend,Weapon(friend),"shield-burst-damage");}
        public void OnReturningShotsCross(UnitState friend,Vector2 position){if(!Member(friend,"paper-wings")||!Active("paper-wings",6))return;UnitState cluster=DensestCluster();if(cluster!=null)Area(cluster,friend,Weapon(friend),"paper-crane");}
        public float TimeStopRemaining=>Mathf.Max(0,stopUntil-time);
        public float PageDamageScale(UnitState enemy)=>Active("archive",6)&&recordedSpecies.Contains(enemy.Definition.Id??"")?1+BondV5Tuning.PageRecordedBonus:1;
        public void OnCloverSpent(){heldClovers=Mathf.Max(0,heldClovers-1);}
        public float SkillDamageScale(UnitState friend,UnitState enemy)=>Member(friend,"demolitionist")&&Active("demolitionist",4)&&State(enemy)?.StunnedUntil>time?1+BondV5Tuning.SkillStunBonus:1;
        public float ShieldedDamageScale=>Active("bastion",4)?1+BondV5Tuning.ShieldDamage:1;
        private void DesperadoVolley(UnitState attacker)
        {
            List<UnitState> targets=Nearby(attacker,float.PositiveInfinity);
            for(int i=0;i<Mathf.Min(BondTuning.DesperadoShots,targets.Count);i++)Damage(targets[i],attacker,Weapon(attacker)*BondTuning.DesperadoDamage,"desperado-volley");
        }
        public void OnGeneratedKill(string rule,UnitState attacker,UnitState victim,float damage=0)
        {
            if(attacker==null||victim==null)return;
            if(rule=="desperado-volley"&&Active("desperado",4))
                foreach(UnitState next in Nearby(victim,float.PositiveInfinity))if(next.Alive){Damage(next,attacker,Weapon(attacker)*BondTuning.DesperadoDamage,"desperado-volley");break;}
            if(rule=="sniper-carry"||!Member(attacker,"sniper")||!Active("sniper",4)||(attacker.Position-victim.Position).sqrMagnitude<BondV5Tuning.SniperCarryDistance*BondV5Tuning.SniperCarryDistance)return;
            Vector2 direction=(victim.Position-attacker.Position).normalized,end=victim.Position+direction*BondV5Tuning.SniperCarryDistance;
            float amount=damage>0?damage:Weapon(attacker);
            foreach(var pair in enemies)if(pair.Key.Alive&&CompanionSkillRuntime.Contains(SkillShape.Line,victim.Position,end,BondV5Tuning.LaneRadius,pair.Key.Position))Damage(pair.Key,attacker,amount,"sniper-carry");
            ZoneRequested?.Invoke(new BondZoneRequest("sniper-carry",victim.Position,end,BondV5Tuning.LaneRadius,0,attacker.Id));
        }
        private void CastV5(UnitState friend)
        {
            lastCastFriend=friend;if(Active("hat",6)&&HatCount(friend)>=BondV5Tuning.HatTransform)Request(BondSupportKind.SkillCopy,friend,1,0,"top-hat");
            if(Active("regal",4)&&IsCrowned(friend))foreach(UnitState other in friends)if(other.Alive&&Has(other,"regal"))Request(BondSupportKind.SkillFill,other,BondV5Tuning.BeatSpeed,0,"crown-fill");
            if(Active("big-top",4)&&combo*BondTuning.ComboPerLink>=BondTuning.ComboHighCap){combo=0;Request(BondSupportKind.SkillCopy,friend,1,0,"showtime",copyPosition:AlternateClusterPosition());}

        }
        private void RefreshMonolithArmor(){monolithArmor.Clear();if(!Active("monolith"))return;if(hero!=null)CacheMonolith(hero);foreach(UnitState friend in friends)CacheMonolith(friend);}
        private void CacheMonolith(UnitState friend){if(!Member(friend,"monolith"))return;int count=0;foreach(var pair in enemies)if(pair.Key.Alive&&(pair.Key.Position-friend.Position).sqrMagnitude<=BondTuning.MonolithRadius*BondTuning.MonolithRadius)count++;int cap=Active("monolith",4)?BondTuning.MonolithHighCap:BondTuning.MonolithCap;monolithArmor[friend.Id]=Mathf.Min(count,cap);if(Active("monolith",4)&&count>=cap)foreach(var pair in enemies)if(pair.Key.Alive&&(pair.Key.Position-friend.Position).sqrMagnitude<=BondTuning.MonolithRadius*BondTuning.MonolithRadius){pair.Key.TauntBy=friend.Id;pair.Key.TauntUntil=time+BondTuning.PlaguePeriod;}}
        private void TickV5(float dt)
        {
            if(Edict==BondEdict.Regeneration&&Active("regal")){if(RuleApplies(hero,"regal"))Request(BondSupportKind.Heal,hero,BondV5Tuning.EdictRegeneration*dt,0,"court-regeneration");foreach(UnitState friend in friends)if(friend.Alive&&RuleApplies(friend,"regal"))Request(BondSupportKind.Heal,friend,BondV5Tuning.EdictRegeneration*dt,0,"court-regeneration");}
            if(Active("tidal",8)&&!leviathanUsed){int largest=-1;RuleZone chosen=default;foreach(RuleZone zone in zones)if(zone.Rule=="tide-pool"&&zone.Until>time){int count=0;foreach(var pair in enemies)if(pair.Key.Alive&&(pair.Key.Position-zone.Position).sqrMagnitude<=BondTuning.BurstRadius*BondTuning.BurstRadius)count++;if(count>largest){largest=count;chosen=zone;}}if(largest>=0){leviathanUsed=true;Spawn(new BondSpawnRequest(BondSpawnKind.Leviathan,chosen.Position,damage:Weapon(bestFriend)));foreach(var pair in enemies)if(pair.Key.Alive)pair.Value.SoakedUntil=time+BondTuning.SoakSeconds;WholeArena("leviathan",Weapon(bestFriend));}}
            if(Active("gravity",8)&&hero!=null)foreach(var pair in enemies)if(pair.Key.Alive&&(pair.Key.Position-hero.Position).sqrMagnitude<=BondTuning.GravityRadius*BondTuning.GravityRadius)pair.Key.Position=Vector2.MoveTowards(pair.Key.Position,hero.Position,BondTuning.GravityPull*dt);
            if(Active("orchard",6)&&hero!=null)foreach(var pair in enemies)if(pair.Key.Alive&&CompanionSkillRuntime.Contains(SkillShape.Line,hero.Position,hero.Position+Vector2.right*(BondTuning.SproutLimit-1),BondTuning.RuneInnerRadius,pair.Key.Position))pair.Value.RootedUntil=Mathf.Max(pair.Value.RootedUntil,time+BondV5Tuning.PlantRootSeconds);
            if(Active("ember",8)&&!phoenixUsed&&Heat>=BondV5Tuning.PhoenixHeat){phoenixUsed=true;Spawn(new BondSpawnRequest(BondSpawnKind.Phoenix,hero.Position,lifetime:BondTuning.PhoenixSeconds,damage:Weapon(bestFriend)));for(int i=0;i<patches.Count;i++){EmberPatch patch=patches[i];patch.Until=time+BondTuning.EmberSeconds;patches[i]=patch;}}
            if(Active("venom",8)&&!pandemicUsed&&Toxin>=BondV5Tuning.PandemicToxin){pandemicUsed=true;foreach(var pair in enemies)if(pair.Key.Alive)AddPoison(pair.Key,bestFriend,Weapon(bestFriend),BondTuning.BurnStacks,true);Emit("pandemic",hero);}
            if(Active("static-club",6)&&time>=rodAt){rodAt=time+BondV5Tuning.RodPeriod;UnitState cluster=DensestCluster();if(cluster!=null)AddZone("lightning-rod",cluster,BondV5Tuning.RodSeconds);}


            if(time>=zonePulseAt){zonePulseAt=time+BondTuning.PlaguePeriod;RefreshMonolithArmor();for(int z=zones.Count-1;z>=0;z--){RuleZone zone=zones[z];if(zone.Until<=time){zones.RemoveAt(z);continue;}foreach(var pair in enemies)if(pair.Key.Alive&&ZoneContains(zone,pair.Key.Position)){BondEnemyState state=pair.Value;if(zone.Rule=="ice-patch"){state.Chill=Mathf.Min(BondTuning.ChillStacks,state.Chill+1);if(state.Chill>=BondTuning.ChillStacks&&state.FrozenUntil<=time){state.FrozenUntil=time+BondTuning.FreezeSeconds;state.FrozenHitReady=true;}}if(zone.Rule=="tide-pool"){state.SoakedUntil=time+BondTuning.SoakSeconds;pair.Key.Position=Vector2.MoveTowards(pair.Key.Position,zone.Position,BondTuning.GravityPull);}if(zone.Rule=="rot-cloud")AddPoison(pair.Key,zone.Source,Weapon(zone.Source),1,true);if(zone.Rule=="lane-cyclone"){pair.Key.Position=Vector2.MoveTowards(pair.Key.Position,zone.End,BondTuning.GravityPull);}if(zone.Rule=="after-hole"){pair.Key.Position=Vector2.MoveTowards(pair.Key.Position,zone.Position,BondTuning.GravityPull);Mass++;Damage(pair.Key,zone.Source,Weapon(zone.Source)*BondTuning.SingularityDamage,"after-hole");}if(zone.Rule=="spinning-blade")Damage(pair.Key,zone.Source,Weapon(zone.Source)*BondTuning.BladeSplash,"spinning-blade");}}}
        }
    }
}
