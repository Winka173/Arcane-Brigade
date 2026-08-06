using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        public MortalityRuntime Mortality { get; private set; }
        public CompanionSkillRuntime SkillsV3 { get; private set; }
        private sealed class V3Status
        {
            public float Damage, DamageUntil, AttackSpeed, AttackUntil, Crit, CritUntil;
            public float Reflect, ReflectUntil, UntouchableUntil, Armor, ArmorUntil;
            public float Weaken, WeakenUntil, Mark, MarkUntil, Slow, SlowUntil;
            public float Dot, DotUntil, DotAt, ProjectileSpeed, ProjectileUntil;
            public float Miss, MissUntil, CharmUntil, FearUntil, SwallowUntil;
            public float MoveSpeed, MoveUntil, ContactAt, ContactDamage;
            public Vector2 FearOrigin;
            public int ReflectHits, ShareSource, FeatherSource, SkillHitCount;
            public float Share, ShareUntil;
            public bool FeatherUsed;
            public UnitState DotSource;
        }
        private struct V3Construct
        {
            public bool Active, Bullets, Bodies;
            public Vector2 Start, End;
            public float Radius, Until;
            public int Source;
        }
        private struct V3Copy { public int Id; public float Strength, ReadyAt; public string Skill; public Vector2? Position; public bool Page; }
        private readonly Dictionary<int,V3Status> v3Statuses = new Dictionary<int,V3Status>();
        private readonly V3Construct[] v3Constructs = new V3Construct[RulesV3Tuning.ConstructCapacity];
        private readonly bool[] v3WallChosen=new bool[MortalityTuning.TeamCapacity];
        private readonly Queue<V3Copy> v3Copies = new Queue<V3Copy>(RulesV3Tuning.CopyCapacity);
        private bool v3FreeCast, v3Sharing;
        private float v3BastionWallUntil;
        private readonly HashSet<int> v3WallShieldRefills=new HashSet<int>(MortalityTuning.TeamCapacity);
        private bool v3PiercingVolley;
        private readonly HashSet<int> v3CrownedIds=new HashSet<int>(MortalityTuning.TeamCapacity);
        private float v3CopyHealAt=-1;
        private string v3CopyHealSkill;
        private int v3TeamReflectHits;
        private float v3TeamReflectUntil;
        public int RulesV3Rerolls { get; private set; }
        public int RulesV3RerollDiscount { get; private set; }
        public event Action<BondSupportRequest> RulesV3SupportApplied;
        private V3Status V3State(UnitState unit) => unit != null && v3Statuses.TryGetValue(unit.Id,out V3Status value) ? value : null;

        private void StartRulesV3()
        {
            v3Statuses.Clear(); v3Copies.Clear(); Array.Clear(v3Constructs,0,v3Constructs.Length);
            RulesV3Rerolls = RulesV3RerollDiscount = 0; v3FreeCast = v3Sharing = v3PiercingVolley = false;v3CrownedIds.Clear();
            v3TeamReflectHits=0;v3TeamReflectUntil=0;v3BastionWallUntil=0;v3WallShieldRefills.Clear();
            Mortality = new MortalityRuntime(); Mortality.ResetWave(units);
            Mortality.ShieldAbsorbed += (friend,amount)=>Bonds.OnFriendShieldAbsorbed(amount);
            Mortality.ShieldBroken += friend=>
            {
                Bonds.OnShieldBroken(friend);
                if(v3BastionWallUntil>Elapsed && v3WallShieldRefills.Add(friend.Id))
                    Mortality.Shield(friend,Mortality.ShieldCapacity(friend)/MortalityRuntime.MaxHealth(friend),v3BastionWallUntil-Elapsed);
            };
            Mortality.WaveLost += value => { Phase = RunPhase.Defeat; };
            Mortality.Fell += value =>
            {
                UnitState friend = UnitWithId(value.FriendId);
                Bonds.OnFriendFell(friend);
            };
            SkillsV3 = new CompanionSkillRuntime();
            SkillsV3.BondFillBonus = Bonds.SkillFillBonusFor;
            SkillsV3.HoldCasts = () => Bonds.TimeStopRemaining>0;
            SkillsV3.ResetWave(units,Champion,catalog.RunWave,unchecked((uint)runSeed));
            SkillsV3.DamageRequested += request =>
            {
                if (!request.Target.Alive) return;
                ApplyDamage(request.Target,request.Damage*Bonds.SkillDamageScale(request.Attacker,request.Target)*(request.PageCopy?Bonds.PageDamageScale(request.Target):1),Faction.Companion,cause: NewCause(request.Attacker.Id).Child("skill-v3"));
                if(!request.Target.Alive)SkillsV3.OnKill(request.Attacker,request.Target,request.SkillId);
                V3Status castState=V3State(request.Attacker);
                Bonds.OnSkillDamage(request.Attacker,request.Target,castState==null?0:castState.SkillHitCount++);
            };
            SkillsV3.CastRequested += request =>
            {
                CompanionSkillCasts++;
                V3Status castState=V3State(UnitWithId(request.SourceId));if(castState!=null)castState.SkillHitCount=0;
                if (!v3FreeCast) Bonds.OnCast(UnitWithId(request.SourceId));
                if (!v3FreeCast && v3CrownedIds.Contains(request.SourceId))QueueV3Copy(UnitWithId(request.SourceId),request.Strength);
            };
            SkillsV3.FriendRequested += ApplyV3Friend;
            SkillsV3.EnemyEffectRequested += ApplyV3EnemyEffect;
            SkillsV3.ConstructRequested += ApplyV3Construct;
            foreach (UnitState unit in units) v3Statuses[unit.Id] = new V3Status();
            Bonds.SupportRequested += ApplyV3BondSupport;
            for (int i=0;i<Bonds.SupportRequests.Count;i++) ApplyV3BondSupport(Bonds.SupportRequests[i]);
            Bonds.ClearSupportRequests();
        }

        private void TickRulesV3(float dt)
        {
            Mortality.Tick(dt);
            SkillsV3.Tick(dt);
            int copies = 0;
            v3FreeCast = true;
            int pendingCopies=v3Copies.Count;
            while (v3Copies.Count>0 && copies++<pendingCopies)
            { V3Copy copy=v3Copies.Dequeue(); if(copy.ReadyAt>Elapsed){v3Copies.Enqueue(copy);continue;} SkillsV3.PageCopyContext=copy.Page;if(copy.Position.HasValue){if(copy.Skill==null)SkillsV3.CastFreeAt(copy.Id,copy.Strength,copy.Position.Value);else SkillsV3.CastCopyAt(UnitWithId(copy.Id),copy.Skill,copy.Strength,copy.Position.Value);}
              else if(copy.Skill==null)SkillsV3.CastFree(copy.Id,copy.Strength);else SkillsV3.CastCopy(UnitWithId(copy.Id),copy.Skill,copy.Strength);SkillsV3.PageCopyContext=false; }
            v3FreeCast = false;
            foreach (UnitState unit in StableUnits)
            {
                V3Status status=V3State(unit);
                if (status==null || !unit.Alive || status.DotUntil<=Elapsed || status.DotAt>Elapsed) continue;
                status.DotAt=Elapsed+RulesV3Tuning.ConstructTick;
                ApplyDamage(unit,status.Dot,unit.Faction==Faction.Enemy?Faction.Companion:Faction.Enemy,cause:NewCause(status.DotSource==null?0:status.DotSource.Id).Child("skill-v3-dot"));
            }
            Bonds.ClearSupportRequests();
        }

        private void QueueV3Copy(UnitState unit,float strength,float delay=0,Vector2? position=null,bool page=false)
        { if(unit!=null && v3Copies.Count<RulesV3Tuning.CopyCapacity) v3Copies.Enqueue(new V3Copy { Id=unit.Id,Strength=strength,ReadyAt=Elapsed+delay,Position=position,Page=page }); }

        private void ApplyV3Friend(SkillFriendRequest request)
        {
            UnitState unit=request.Target; if(unit==null) return;
            V3Status status=V3State(unit); if(status==null) return;
            if(request.Source==Champion && HeroRulesV3!=null && HeroRulesV3.CopiedHealsGlobal && (request.Kind==SkillFriendEffect.HealFraction || request.Kind==SkillFriendEffect.HealFlat))
            {
                if(v3CopyHealAt==Elapsed && v3CopyHealSkill==request.SkillId)return;
                v3CopyHealAt=Elapsed;v3CopyHealSkill=request.SkillId;
                for(int i=0;i<Mortality.FriendCount;i++)
                { UnitState friend=Mortality.FriendAt(i);V3Heal(friend,request.Kind==SkillFriendEffect.HealFraction?MortalityRuntime.MaxHealth(friend)*request.Value:request.Value); }
                return;
            }
            switch(request.Kind)
            {
                case SkillFriendEffect.HealFraction: V3Heal(unit,MortalityRuntime.MaxHealth(unit)*request.Value); break;
                case SkillFriendEffect.HealFlat: V3Heal(unit,request.Value); break;
                case SkillFriendEffect.ShieldFraction: Mortality.Shield(unit,request.Value,request.Duration+(request.Source==Champion?HeroRulesV3?.CopiedShieldExtraDuration??0:0)); break;
                case SkillFriendEffect.DamageReduction: Mortality.ReduceDamage(unit,request.Value,request.Duration); break;
                case SkillFriendEffect.ReviveSooner: Mortality.ReviveSooner(unit,request.Value); break;
                case SkillFriendEffect.ReviveNow: Mortality.ReviveNow(unit,request.Value); break;
                case SkillFriendEffect.Cleanse: V3CleanseFriend(unit); break;
                case SkillFriendEffect.Damage: case SkillFriendEffect.OnBeatDamage:
                    status.Damage=request.Value; status.DamageUntil=Elapsed+request.Duration; break;
                case SkillFriendEffect.AttackSpeed: status.AttackSpeed=request.Value; status.AttackUntil=Elapsed+request.Duration; break;
                case SkillFriendEffect.SkillSpeed: SkillsV3.Accelerate(unit.Id,request.Value,request.Duration); break;
                case SkillFriendEffect.Crit: status.Crit=request.Value; status.CritUntil=Elapsed+request.Duration; break;
                case SkillFriendEffect.Reflect: case SkillFriendEffect.Retaliate:
                    status.Reflect=request.Value; status.ReflectUntil=Elapsed+request.Duration; break;
                case SkillFriendEffect.ReflectHits: case SkillFriendEffect.AbsorbProjectiles:
                    if(request.SkillId=="MirrorNewt"){v3TeamReflectHits=Mathf.Max(v3TeamReflectHits,Mathf.RoundToInt(request.Value));v3TeamReflectUntil=Elapsed+request.Duration;break;}
                    status.ReflectHits=Mathf.RoundToInt(request.Value); status.ReflectUntil=Elapsed+request.Duration; break;
                case SkillFriendEffect.ShareDamage:
                    status.ShareSource=request.SourceId; status.Share=request.Value; status.ShareUntil=Elapsed+request.Duration; break;
                case SkillFriendEffect.RebirthFeather: status.FeatherSource=request.SourceId; break;
                case SkillFriendEffect.PullToSafety: unit.Position=ClampPosition(request.Source.Position,unit.Definition.Radius); break;
                case SkillFriendEffect.Swap:
                    // The skill runtime swaps both bodies before emitting this notification.
                    break;
                case SkillFriendEffect.RerollDiscount: RulesV3RerollDiscount+=Mathf.RoundToInt(request.Value); break;
                case SkillFriendEffect.BarRefund: SkillsV3.Refund(unit.Id,request.Value); break;
                case SkillFriendEffect.ProjectileSpeed:
                    status.ProjectileSpeed=request.Value; status.ProjectileUntil=Elapsed+request.Duration; break;
            }
        }

        private void ApplyV3EnemyEffect(SkillEnemyEffectRequest request)
        {
            UnitState target=request.Target; V3Status status=V3State(target); if(status==null || !target.Alive) return;
            SkillEnemyEffect flags=request.Effects;
            if(EnemyRulesV3?.ImmuneToControl(target)??false)flags &= ~(SkillEnemyEffect.Freeze|SkillEnemyEffect.Stun|SkillEnemyEffect.Sleep|SkillEnemyEffect.Root|SkillEnemyEffect.Slow|SkillEnemyEffect.Chill|SkillEnemyEffect.AttackSlow);
            if((flags&(SkillEnemyEffect.Freeze|SkillEnemyEffect.Stun|SkillEnemyEffect.Sleep|SkillEnemyEffect.Root))!=0)
                Sleep(target,request.Duration);
            if((flags&(SkillEnemyEffect.Slow|SkillEnemyEffect.Chill|SkillEnemyEffect.AttackSlow))!=0)
            { status.Slow=Mathf.Max(status.Slow,request.Amount); status.SlowUntil=Elapsed+request.Duration; }
            if((flags&(SkillEnemyEffect.Mark|SkillEnemyEffect.Lit|SkillEnemyEffect.Soak|SkillEnemyEffect.DuskMark))!=0)
            { status.Mark=request.Amount; status.MarkUntil=Elapsed+request.Duration; if((flags&SkillEnemyEffect.Soak)!=0) Bonds.OnSoak(); }
            if((flags&(SkillEnemyEffect.Weaken|SkillEnemyEffect.Blind))!=0)
            { status.Weaken=request.Amount; status.WeakenUntil=Elapsed+request.Duration; }
            if((flags&(SkillEnemyEffect.Burn|SkillEnemyEffect.Poison))!=0)
            { status.Dot=request.Source.Definition.Weapon.Damage*RulesV3Tuning.DotDamage; status.DotSource=request.Source; status.DotUntil=Elapsed+request.Duration; }
            if((flags&SkillEnemyEffect.Taunt)!=0)
            { target.TauntBy=request.Source.Id; target.TauntUntil=Elapsed+request.Duration; }
            if((flags&(SkillEnemyEffect.Pull|SkillEnemyEffect.Push|SkillEnemyEffect.Fear))!=0)
            {
                Vector2 direction=(flags&SkillEnemyEffect.Pull)!=0 ? -request.Direction : request.Direction;
                target.Position=ClampPosition(target.Position+direction*RulesV3Tuning.PushDistance,target.Definition.Radius);
            }
            if((flags&SkillEnemyEffect.Silence)!=0) target.NextAttack=Mathf.Max(target.NextAttack,Elapsed+request.Duration);
        }

        private void ApplyV3Construct(SkillConstructRequest request)
        {
            if(request.Kind=="coins") { Gold=RunSession.BoundedCounter((long)Gold+Mathf.RoundToInt(request.Value)); return; }
            if(request.Kind=="cherry") {AddV3Entity(RulesV3EntityKind.Snack,request.Kind,request.Position,request.SourceId,heal:request.Value);TraitsV3.OnCherry(UnitWithId(request.SourceId),request.Position);}
            else if(request.Kind=="rebirth-feather")AddV3Entity(RulesV3EntityKind.Feather,request.Kind,request.Position,request.SourceId,request.TargetId,value:request.Value);
            else if(request.Kind=="snack" || request.Kind=="healing-pumpkin")AddV3Entity(request.Kind=="snack"?RulesV3EntityKind.Snack:RulesV3EntityKind.Pumpkin,request.Kind,request.Position,request.SourceId,heal:request.Value);
            else if(request.Kind=="taunt-skull")AddV3Entity(RulesV3EntityKind.Skull,request.Kind,request.Position,request.SourceId,duration:request.Value);
            // Skills resolve persistent summon/transform damage in their pooled area scheduler.
            if(request.BlocksBullets || request.BlocksBodies)
            {
                if(request.Count<=1)AddV3Wall(request.Position,request.End,RulesV3Tuning.ContactPadding,request.Duration,request.SourceId,request.BlocksBullets,request.BlocksBodies || request.SkillId=="CrayonGremlin");
                else
                {
                    Array.Clear(v3WallChosen,0,v3WallChosen.Length);Vector2 danger=V3CrowdCenter(request.Position);
                    for(int wall=0;wall<request.Count;wall++)
                    {
                        int chosen=-1;float distance=float.PositiveInfinity;
                        for(int f=0;f<Mortality.FriendCount && f<v3WallChosen.Length;f++)
                        {UnitState friend=Mortality.FriendAt(f);float square=(friend.Position-danger).sqrMagnitude;if(friend.Alive && !v3WallChosen[f] && square<distance){chosen=f;distance=square;}}
                        if(chosen<0)break;v3WallChosen[chosen]=true;Vector2 center=Mortality.FriendAt(chosen).Position;
                        AddV3Wall(center-Vector2.right*request.Radius,center+Vector2.right*request.Radius,RulesV3Tuning.ContactPadding,request.Duration,request.SourceId,request.BlocksBullets,request.BlocksBodies);
                    }
                }
            }
        }
        private void AddV3Wall(Vector2 start,Vector2 end,float radius,float duration,int source,bool bullets=true,bool bodies=true)
        {
            for(int i=0;i<v3Constructs.Length;i++) if(!v3Constructs[i].Active || v3Constructs[i].Until<=Elapsed)
            { v3Constructs[i]=new V3Construct { Active=true,Start=start,End=end,Radius=radius,Until=Elapsed+duration,Source=source,Bullets=bullets,Bodies=bodies }; return; }
        }
        private bool V3StopsProjectile(ProjectileState shot,float dt)
        {
            if(shot.Faction!=Faction.Enemy) return false;
            for(int i=0;i<v3Constructs.Length;i++)
            {
                V3Construct wall=v3Constructs[i];
                if(wall.Active && wall.Bullets && wall.Until>Elapsed && (V3SegmentsCross(shot.Position,shot.Position+shot.Velocity*dt,wall.Start,wall.End) || CompanionSkillRuntime.Contains(SkillShape.Line,wall.Start,wall.End,wall.Radius+shot.Radius,shot.Position+shot.Velocity*dt))){Bonds.OnWallAbsorbed(shot.Damage);return true;}
            }
            return false;
        }

        private void ApplyV3BondSupport(BondSupportRequest request)
        {
            RulesV3SupportApplied?.Invoke(request);
            UnitState unit=request.Subject; V3Status status=V3State(unit);
            switch(request.Kind)
            {
                case BondSupportKind.Heal: V3Heal(unit,MortalityRuntime.MaxHealth(unit)*request.Amount); break;
                case BondSupportKind.Shield: Mortality.Shield(unit,request.Amount,request.Duration>0?request.Duration:RulesV3Tuning.WaveSeconds); break;
                case BondSupportKind.Revive: Mortality.ReviveNow(unit,request.Amount); break;
                case BondSupportKind.DamageShared:
                    bool sharing=v3Sharing; v3Sharing=true;
                    ApplyDamage(unit,request.Amount,Faction.Enemy,cause:NewCause(0).Child("bond-share")); v3Sharing=sharing; break;
                case BondSupportKind.Untouchable: if(status!=null) status.UntouchableUntil=Elapsed+request.Duration; break;
                case BondSupportKind.Armor: if(status!=null) { status.Armor=request.Amount; status.ArmorUntil=Elapsed+request.Duration; } break;
                case BondSupportKind.Cleanse: V3CleanseFriend(unit); break;
                case BondSupportKind.Page: AddV3Entity(RulesV3EntityKind.Page,request.Rule,request.Position,value:request.Amount);break;
                case BondSupportKind.Speed: if(status!=null){status.AttackSpeed=request.Amount;status.AttackUntil=Elapsed+request.Duration;}break;
                case BondSupportKind.Transform: if(status!=null){status.Damage=request.Amount;status.DamageUntil=Elapsed+request.Duration;}break;
                case BondSupportKind.Crown: if(unit!=null)v3CrownedIds.Add(unit.Id);break;
                case BondSupportKind.Dash: if(unit!=null)unit.Position=ClampPosition(Vector2.MoveTowards(unit.Position,V3CrowdCenter(unit.Position),request.Amount),unit.Definition.Radius);break;
                case BondSupportKind.PiercingVolley: v3PiercingVolley=true;break;
                case BondSupportKind.SkillCopy: case BondSupportKind.ForceCast: QueueV3Copy(unit,request.Amount,request.Duration,request.HasCopyPosition?(Vector2?)request.CopyPosition:null); break;
                case BondSupportKind.SkillFill: if(unit!=null){if(request.Rule=="quicksoles-refund-seconds")SkillsV3.RefundSeconds(unit.Id,request.Amount);else SkillsV3.Refund(unit.Id,request.Amount);}break;
                case BondSupportKind.DamageBuff: if(status!=null){status.Damage=request.Amount;status.DamageUntil=Elapsed+request.Duration;}break;
                case BondSupportKind.CritBuff: if(status!=null){status.Crit=request.Amount;status.CritUntil=Elapsed+request.Duration;}break;
                case BondSupportKind.ReviveDelay: Mortality.ReviveSooner(unit,Mathf.Max(0,MortalityTuning.RiseSeconds-request.Amount));break;
                case BondSupportKind.ArmorBreak: if(unit!=null && unit.Faction==Faction.Enemy)EnemyRulesV3?.BreakArmor(unit,request.Amount);break;
                case BondSupportKind.Coin: Gold=RunSession.BoundedCounter((long)Gold+Mathf.RoundToInt(request.Amount)); break;
                case BondSupportKind.Reroll: RulesV3Rerolls+=Mathf.RoundToInt(request.Amount); break;
                case BondSupportKind.Wall:
                    if(request.Rule=="bastion-wall"){v3BastionWallUntil=Elapsed+request.Duration;v3WallShieldRefills.Clear();}
                    AddV3Wall(request.Position-Vector2.right*request.Radius,request.Position+Vector2.right*request.Radius,RulesV3Tuning.ContactPadding,request.Duration*(TraitsV3?.BastionDurationScale??1),unit==null?0:unit.Id); break;
                case BondSupportKind.Pull:
                    foreach(UnitState enemy in StableUnits) if(enemy.Alive && enemy.Faction==Faction.Enemy && (enemy.Position-request.Position).sqrMagnitude<=request.Radius*request.Radius)
                        enemy.Position=Vector2.MoveTowards(enemy.Position,request.Position,request.Amount);
                    break;
                case BondSupportKind.TimeStop:
                    foreach(UnitState enemy in StableUnits) if(enemy.Alive && enemy.Faction==Faction.Enemy) Sleep(enemy,request.Duration);
                    break;
            }
        }

        private float V3HitDamage(UnitState target,float damage,Faction source,CombatCause cause,WeaponDefinition weapon,Vector2 direction)
        {
            V3Status status=V3State(target), attacker=V3State(UnitWithId(cause.SourceUnitId));
            if(target.Faction==Faction.Enemy && Mortality.ShieldAmount(UnitWithId(cause.SourceUnitId))>0)damage*=Bonds.ShieldedDamageScale;
            if(attacker!=null)
            {
                if(attacker.DamageUntil>Elapsed) damage*=1+attacker.Damage;
                if(attacker.WeakenUntil>Elapsed) damage*=1-attacker.Weaken;
            }
            if(source==Faction.Enemy && Bonds.EnemyMissChance(UnitWithId(cause.SourceUnitId))>0 && combatRandom.NextDouble()<Bonds.EnemyMissChance(UnitWithId(cause.SourceUnitId)))return 0;
            if(source==Faction.Enemy && EnemyRulesV3!=null) damage=EnemyRulesV3.ModifyOutgoing(UnitWithId(cause.SourceUnitId),target,damage);
            if(target.Faction==Faction.Enemy)
            { if(status!=null && status.MarkUntil>Elapsed) damage*=1+status.Mark;
                return EnemyRulesV3==null?damage:EnemyRulesV3.ModifyIncoming(target,damage,UnitWithId(cause.SourceUnitId),weapon,direction,Bonds.ArmorIgnoredFor(UnitWithId(cause.SourceUnitId))); }
            if(Bonds.DodgeChance>0 && combatRandom.NextDouble()<Bonds.DodgeChance){Bonds.OnDodge(target);return 0;}
            if(v3TeamReflectHits>0 && v3TeamReflectUntil>Elapsed){v3TeamReflectHits--;V3ReflectArea(target,UnitWithId(cause.SourceUnitId),damage);return 0;}
            if(status!=null)
            {
                if(status.UntouchableUntil>Elapsed) return 0;
                if(status.ReflectHits>0 && status.ReflectUntil>Elapsed) { status.ReflectHits--;V3ReflectArea(target,UnitWithId(cause.SourceUnitId),damage);return 0; }

                if(!v3Sharing && status.ShareUntil>Elapsed)
                {
                    UnitState receiver=UnitWithId(status.ShareSource);
                    if(receiver!=null && receiver!=target && receiver.Alive)
                    {
                        float shared=damage*status.Share; damage-=shared; v3Sharing=true;
                        ApplyDamage(receiver,shared,Faction.Enemy,cause:cause.Child("skill-share")); v3Sharing=false;
                    }
                }
            }
            if(!v3Sharing) damage=Bonds.AdjustFriendDamage(target,damage);
            damage=RunModifierRules.AfterArmor(damage, FriendArmorPoints(target)*RunModifierRules.ArmorPoint);
            return Mortality.ModifyIncomingDamage(target,damage);
        }
        public float FriendArmorPoints(UnitState unit)
        {
            if(unit == null || unit.Faction == Faction.Enemy) return 0;
            V3Status status=V3State(unit);
            float extra=Bonds.ArmorBonusFor(unit)+Bonds.MonolithArmor(unit)+Bonds.EdictArmorFor(unit);
            if(status!=null && status.ArmorUntil>Elapsed)extra+=status.Armor;
            if(unit==Champion)extra+=catalog.Armor/RunModifierRules.ArmorPoint+MapBoostValue(MapStat.Armor)+(HeroRulesV3?.ArmorBonus??0);
            return MortalityRuntime.ArmorPoints(unit,extra);
        }
        private float V3CriticalBonus(UnitState unit)
        { V3Status status=V3State(unit);return (status!=null && status.CritUntil>Elapsed?status.Crit:0)+(unit==Champion?HeroRulesV3?.CriticalBonus??0:HeroRulesV3?.FriendCriticalBonus(unit)??0); }
        private float V3AttackSpeed(UnitState unit)
        { V3Status status=V3State(unit); return status!=null && status.AttackUntil>Elapsed ? status.AttackSpeed : 0; }
        private float V3MoveScale(UnitState unit)
        { V3Status status=V3State(unit); return status==null?1:(status.SlowUntil>Elapsed?Mathf.Clamp01(1-status.Slow):1)*(1+(status.MoveUntil>Elapsed?status.MoveSpeed:0)); }
        private bool V3Lethal(UnitState unit,int sourceId)
        {
            if(unit.Faction==Faction.Enemy) return false;
            if(Bonds.TryPreventFall(unit)) return true;
            V3Status state=V3State(unit);
            if(state!=null && state.FeatherSource!=0 && !state.FeatherUsed)
            { state.FeatherUsed=true; unit.Health=MortalityRuntime.MaxHealth(unit)*RulesV3Tuning.FeatherHealth; return true; }
            return Mortality.OnLethal(unit,sourceId);
        }
        private UnitState V3NearestFriend(UnitState source,float range)
        {
            UnitState best=null; float square=range*range;
            if(Mortality==null) return Champion!=null && Champion.Alive ? Champion : null;
            for(int i=0;i<Mortality.FriendCount;i++)
            { UnitState friend=Mortality.FriendAt(i); float distance=(friend.Position-source.Position).sqrMagnitude;
                if(friend.Alive && V3Targetable(friend) && distance<=square && (best==null || distance<square || friend.Id<best.Id)) { best=friend; square=distance; } }
            return best;
        }
        private void V3ReflectArea(UnitState friend,UnitState attacker,float damage)
        {
            if(attacker==null)return;
            foreach(UnitState enemy in StableUnits)if(enemy.Alive && enemy.Faction==Faction.Enemy && (enemy.Position-attacker.Position).sqrMagnitude<=BondTuning.BurstRadius*BondTuning.BurstRadius)
                ApplyDamage(enemy,damage,Faction.Companion,cause:NewCause(friend.Id).Child("v3-reflect"));
        }
        private void V3CleanseFriend(UnitState unit)
        {
            Mortality.Cleanse(unit);EnemyRulesV3?.CleanseFriend(unit);V3Status state=V3State(unit);
            if(state!=null)state.DotUntil=state.SlowUntil=state.MissUntil=state.CharmUntil=state.FearUntil=state.SwallowUntil=0;
        }
    }
}
