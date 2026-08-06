using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    /// <summary>One data-driven hero runtime. All outputs describe crowd geometry or friendly/economy actions.</summary>
    public sealed class HeroRulesV3Runtime
    {
        private UnitState hero;
        private IList<UnitState> units;
        private HeroRuleV3Definition definition;
        private HeroRulesV3RunState run;
        private float time, still, distance, next, activeUntil, dangerTime, riposteAt, diveUntil, lastBeat, patchAt, upkeep;
        private int coinCharge, collectedCoins, kills, thorns, sprouts, beatStreak, wave, wavePurchases;
        private bool prowl, riposte, trimmed;
        private string copiedSkill;
        private float copiedValue;
        private readonly HashSet<string> seenWave = new HashSet<string>(StringComparer.Ordinal);
        public event Action<HeroRuleRequest> Requested;
        public HeroRulesV3RunState RunState => run;
        public string HeroId => definition?.Id;
        public float Time => time;
        public bool Untargetable => hero != null && hero.Alive && (diveUntil > time || prowl);
        public bool CanMove => HeroId != "RootOracle";
        public bool WholeArenaPickup => HeroId == "RootOracle";
        public float RecruitDiscount => HeroId == "PennyBroker" ? (Relic(0) ? HeroRulesV3Tuning.HaggleRelic : HeroRulesV3Tuning.Haggle) : 0;
        public float SkillSpeed => HeroId == "BonsaiClocksmith" ? run.WavesSurvived * (Relic(0) ? HeroRulesV3Tuning.PruneRelicSpeed : HeroRulesV3Tuning.PruneSpeed)+run.ShopSkillGrowth : 0;
        public float SpeedBonus => HeroId == "CometRunner" && Relic(1) && time < HeroRulesV3Tuning.InitialSpeedSeconds ? HeroRulesV3Tuning.InitialSpeed : HeroId == "RainArchivist" && time >= (Relic(0) ? 0 : HeroRulesV3Tuning.RainDelay) ? HeroRulesV3Tuning.RainSpeed : 0;
        public float ArmorBonus => HeroId == "LanternKnight" && Relic(2) && distance >= HeroRulesV3Tuning.StrideDistance ? HeroRulesV3Tuning.HeldArmor : HeroId == "CitrusMarshal" && activeUntil > time ? HeroRulesV3Tuning.SignArmor : 0;
        public float CriticalBonus => HeroId == "GlassDice" && Relic(2) ? collectedCoins * HeroRulesV3Tuning.DiceCritPerCoin : HeroId == "MoonProwler" && prowl && Relic(2) ? HeroRulesV3Tuning.ProwlCrit : 0;
        public float MaxHealthBonus => HeroId == "RootOracle" && Relic(2) ? HeroRulesV3Tuning.RootHealth : 0;
        public int ExtraShopSlots => HeroId == "IvorySovereign" && run.Decree == 0 ? 1 : 0;
        public float EnemyCountScale => HeroId == "IvorySovereign" && run.Decree == 2 ? 1-HeroRulesV3Tuning.DecreeEnemyReduction : 1;
        public bool HasRelic(int slot) => slot>=0 && slot<HeroRulesV3Tuning.RelicsPerHero && Relic(slot);
        private bool Relic(int slot) => definition != null && run.Relics.Contains(definition.Relics[slot].Id);
        public void EquipRelic(string id) { if (id != null) run.Relics.Add(id); }
        public void ResetWave(UnitState champion,IList<UnitState> all,int waveNumber,HeroRulesV3RunState persistence=null)
        {
            hero=champion; units=all; definition=HeroRulesV3Catalog.Find(champion?.Definition?.Id); run=persistence ?? new HeroRulesV3RunState(); wave=waveNumber;
            wavePurchases=run.Purchases; run.Purchases=0;
            time=still=distance=dangerTime=activeUntil=upkeep=lastBeat=patchAt=0; riposteAt=diveUntil=-1;
            next=Period(); coinCharge=collectedCoins=kills=thorns=sprouts=beatStreak=0; prowl=riposte=trimmed=false; copiedSkill=null; seenWave.Clear();
            if(HeroId=="IvorySovereign")
            {
                if(run.DecreeUntilWave<wave) run.Decree=-1;
                if(run.Decree<0) Emit(HeroRuleAction.Choice,"decree",count:Relic(0)?4:3);
            }
            if(HeroId=="RootOracle") { Emit(HeroRuleAction.Slow,"ward",Radius(HeroRulesV3Tuning.Radius,Relic(0)?HeroRulesV3Tuning.WardWider:1),HeroRulesV3Tuning.ArenaRadius,HeroRulesV3Tuning.WardSlow); if(Relic(3))Emit(HeroRuleAction.Slow,"sap-patches",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.ArenaRadius,HeroRulesV3Tuning.WardSlow); }
        }
        public bool ChooseDecree(int choice)
        {
            if(HeroId!="IvorySovereign"||choice<0||choice>=(Relic(0)?4:3))return false;
            if(run.Decree>=0) { if(!Relic(3)||run.ClemencyUsed)return false; run.ClemencyUsed=true; }
            run.Decree=choice; run.DecreeUntilWave=wave+(Relic(1)?1:0); Emit(HeroRuleAction.Counter,"decree-chosen",value:choice); if(Relic(2))Emit(HeroRuleAction.Mark,"scribe",HeroRulesV3Tuning.ArenaRadius,HeroRulesV3Tuning.ArenaRadius,position:Cluster(),species:FirstEnemySpecies()); return true;
        }
        public bool Trim(UnitState friend)
        {
            if(HeroId!="BonsaiClocksmith"||trimmed||friend==null||!friend.Alive||friend.Faction!=Faction.Companion)return false;
            trimmed=true; Emit(HeroRuleAction.RefillBar,"pruning",friendId:friend.Id,value:1,count:Relic(1)?2:1);
            if(Relic(2))Emit(HeroRuleAction.DamageBuff,"grafting",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.PruneSeconds,HeroRulesV3Tuning.PruneDamage,friendId:friend.Id);
            return true;
        }
        public void OnPurchase(string companionId=null)
        {
            run.Purchases++;
            if(HeroId!="LoneJelly"||string.IsNullOrEmpty(companionId))return;
            for(int i=0;i<HeroRulesV3Catalog.Traits.Length;i++) if(HeroRulesV3Catalog.Traits[i].CompanionId==companionId)
            {
                run.AbsorbedTraits.Add(companionId); run.AbsorbedCount++;
                Emit(HeroRuleAction.TraitAbsorbed,"absorb",value:Relic(0)?HeroRulesV3Tuning.AbsorbedPower:1,species:companionId,count:Relic(1)&&run.AbsorbedCount==1?2:1); break;
            }
        }
        public void OnWaveSurvived() { run.WavesSurvived++; }
        public void OnSproutRemoved() { sprouts=Mathf.Max(0,sprouts-1); }
        public void Tick(float dt,float movementDistance,float skillFillBonus=0)
        {
            if(hero==null||definition==null||!hero.Alive||!Finite(dt)||dt<=0)return;
            time+=dt;
            // Haste advances cooldown readiness without shortening effect durations or movement thresholds.
            next-=dt*Mathf.Max(0,hero.Definition.SkillHaste+skillFillBonus);
            if(HeroId=="Postmaster"&&coinCharge>=(Relic(0)?HeroRulesV3Tuning.ExpressCoins:HeroRulesV3Tuning.ParcelCoins)&&time>=next)OnCoins(0);
            if(HeroId=="HourglassStag"&&activeUntil>0&&activeUntil<=time) {activeUntil=0;if(Relic(3))Emit(HeroRuleAction.Stun,"antler-bell",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.HourglassStun);}
            distance+=Mathf.Max(0,movementDistance); still=movementDistance>0?0:still+dt;
            if(HeroId=="LanternKnight")
            {
                if(distance>=HeroRulesV3Tuning.StrideDistance && time>=next && EnemyCount(HeroRulesV3Tuning.StrideRadius)>0)
                { distance-=HeroRulesV3Tuning.StrideDistance; next=time+HeroRulesV3Tuning.StrideCooldown; Emit(HeroRuleAction.Damage,"lantern-stride",HeroRulesV3Tuning.StrideRadius*(Relic(0)?HeroRulesV3Tuning.StrideWider:1),value:Weapon()*HeroRulesV3Tuning.StrideDamage); if(Relic(1))Emit(HeroRuleAction.LightLine,"stride-lamp",HeroRulesV3Tuning.StrideRadius,HeroRulesV3Tuning.LampSeconds); if(Relic(3))Emit(HeroRuleAction.DamageBuff,"beacon",HeroRulesV3Tuning.StrideRadius,HeroRulesV3Tuning.LampSeconds,HeroRulesV3Tuning.InitialSpeed); }
            }
            else if(HeroId=="CometRunner" && distance>=HeroRulesV3Tuning.CometDistance*(Relic(3)?HeroRulesV3Tuning.CometChargeRelic:1))
            { distance=0; Emit(HeroRuleAction.Slow,"comet-pulse",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.DefaultDuration,HeroRulesV3Tuning.WardSlow); if(Relic(0))Emit(HeroRuleAction.Damage,"tail-star",HeroRulesV3Tuning.Radius,value:Weapon()); if(Relic(2))Emit(HeroRuleAction.Burn,"cinder-trail",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.CometBurn); }
            else if(HeroId=="DrowsySeer"&&still>=HeroRulesV3Tuning.DreamStill&&time>=next&&EnemyCount(HeroRulesV3Tuning.DreamRadius)>0)
            { next=time+HeroRulesV3Tuning.DreamPeriod; Emit(HeroRuleAction.Slow,"dream-breath",HeroRulesV3Tuning.DreamRadius,HeroRulesV3Tuning.DreamChill+(Relic(0)?HeroRulesV3Tuning.DreamChill:0),HeroRulesV3Tuning.WardSlow); Emit(Relic(3)?HeroRuleAction.ReflectBullets:HeroRuleAction.RemoveBullets,"dream-bullets",HeroRulesV3Tuning.DreamRadius,count:HeroRulesV3Tuning.DreamBullets); if(Relic(2))Emit(HeroRuleAction.Stun,"lullaby",HeroRulesV3Tuning.DreamRadius,HeroRulesV3Tuning.DreamSleep); }
            else if(HeroId=="MirrorKeeper"&&still>=HeroRulesV3Tuning.MirrorStill&&time>=next)
            { next=time+HeroRulesV3Tuning.MirrorStill; Emit(HeroRuleAction.ReflectBullets,"mirror",HeroRulesV3Tuning.DreamRadius,count:Relic(0)?HeroRulesV3Tuning.SilverBullets:HeroRulesV3Tuning.DreamBullets,value:Relic(1)?1:0); }
            else if(HeroId=="CopperCauldron")
            { dangerTime=EnemyCount(HeroRulesV3Tuning.Radius)>=(Relic(0)?HeroRulesV3Tuning.BoilRelicCrowd:HeroRulesV3Tuning.LargeCrowd)?dangerTime+dt:0; if(dangerTime>=HeroRulesV3Tuning.BoilCharge&&activeUntil<=time) { dangerTime=0; activeUntil=time+HeroRulesV3Tuning.BoilDuration; Emit(HeroRuleAction.SpeedBuff,"slow-boil",duration:HeroRulesV3Tuning.BoilDuration,value:HeroRulesV3Tuning.BoilSpeed,friendId:hero.Id); } }
            else if(HeroId=="MoonProwler") prowl=still>=(Relic(0)?HeroRulesV3Tuning.ProwlRelicStill:HeroRulesV3Tuning.ProwlStill);
            else if(HeroId=="PetalCitadel"&&still>=HeroRulesV3Tuning.PetalStill/(Relic(2)?HeroRulesV3Tuning.PetalGrowRelic:1)&&time>=next)
            { next=time+HeroRulesV3Tuning.PetalSeconds; Emit(HeroRuleAction.PetalWall,"bloom-wall",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.PetalSeconds,value:Relic(0)?1:0); if(Relic(1))Emit(HeroRuleAction.Damage,"thorn-trellis",HeroRulesV3Tuning.Radius,value:Weapon()); if(Relic(3))Emit(HeroRuleAction.Shield,"petal-shield",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.PetalSeconds,HeroRulesV3Tuning.ChargeShield); }
            if(riposteAt>0&&time>=riposteAt){riposte=true;riposteAt=-1;}
            if(HeroId=="BellDiver"&&diveUntil>0&&time>=diveUntil)
            { diveUntil=-1; Emit(HeroRuleAction.Mark,"sonar",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.DefaultDuration,Relic(1)?HeroRulesV3Tuning.DiveMarkedDamage:0); if(Relic(2))Emit(HeroRuleAction.Shield,"brass-helmet",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.DefaultDuration,HeroRulesV3Tuning.DiveShield); if(Relic(3))Emit(HeroRuleAction.Push,"sonar-pearl",HeroRulesV3Tuning.Radius,value:HeroRulesV3Tuning.TidePush); }
            if(time>=next && Period()>0){next=time+Period(); Timed();}
            if(time>=upkeep)
            {
                upkeep=time+1;
                if(HeroId=="LoneJelly")Emit(HeroRuleAction.Heal,"jelly-recovery",value:HeroRulesV3Tuning.LoneRecovery*(Relic(2)?HeroRulesV3Tuning.RecoveryRelic:1),friendId:hero.Id);
                if(HeroId=="CopperCauldron"&&activeUntil>time&&Relic(1))Emit(HeroRuleAction.Heal,"ladle",value:HeroRulesV3Tuning.BoilHeal,friendId:hero.Id);
                if(HeroId=="RainArchivist"&&time>=(Relic(0)?0:HeroRulesV3Tuning.RainDelay))
                { Emit(HeroRuleAction.Rain,"downpour",HeroRulesV3Tuning.ArenaRadius,1,HeroRulesV3Tuning.RainSpeed,HeroRuleShape.Arena); if(Relic(1))Emit(HeroRuleAction.Cleanse,"rain-cleanse",HeroRulesV3Tuning.ArenaRadius); if(Relic(3))Emit(HeroRuleAction.Heal,"rain-heal",HeroRulesV3Tuning.ArenaRadius,value:HeroRulesV3Tuning.RainHeal); }
            }
            if(HeroId=="ArchiveCurator")for(int i=0;i<units.Count;i++)if(units[i].Alive&&units[i].Faction==Faction.Enemy)OnSpecies(units[i]);
        }
        private void Timed()
        {
            switch(HeroId)
            {
                case "ChinaMarionette": Emit(HeroRuleAction.TraitSignal,"strings",Relic(0)?HeroRulesV3Tuning.SilkRadius:HeroRulesV3Tuning.StringsRadius,HeroRulesV3Tuning.DefaultDuration,HeroRulesV3Tuning.StringsReduction); break;
                case "ButtonCavalier":
                    Vector2 end=Cluster(); Emit(HeroRuleAction.Damage,"charge",HeroRulesV3Tuning.LineWidth,value:Weapon()*(Relic(0)?2:1),shape:HeroRuleShape.Line,end:end); hero.Position=end;
                    if(Relic(2))Emit(HeroRuleAction.SpeedBuff,"plume",duration:HeroRulesV3Tuning.ChargeSpeedSeconds,value:HeroRulesV3Tuning.ChargeSpeed,friendId:hero.Id);
                    if(Relic(3))Emit(HeroRuleAction.Shield,"horse-blanket",duration:HeroRulesV3Tuning.DefaultDuration,value:HeroRulesV3Tuning.ChargeShield,friendId:hero.Id); break;
                case "CitrusMarshal": activeUntil=time+HeroRulesV3Tuning.SignSeconds+(Relic(0)?HeroRulesV3Tuning.SignExtraSeconds:0); Emit(HeroRuleAction.Signpost,"signpost",HeroRulesV3Tuning.SignRadius,activeUntil-time,Relic(1)?1:0,count:Relic(3)?2:1); if(Relic(2))Emit(HeroRuleAction.Push,"directions",HeroRulesV3Tuning.SignRadius,value:HeroRulesV3Tuning.TidePush); break;
                case "LanternWeaver":
                    UnitState previous=Relic(1)?hero:null;
                    for(int i=0;i<units.Count;i++){UnitState f=units[i];if(!f.Alive||f.Faction!=Faction.Companion)continue; if(previous!=null)Emit(HeroRuleAction.LightLine,"weave",HeroRulesV3Tuning.LineWidth,HeroRulesV3Tuning.WeaveSeconds*(Relic(0)?HeroRulesV3Tuning.WeaveRelicDuration:1),Weapon()*(Relic(3)?HeroRulesV3Tuning.CritMultiplier:1),HeroRuleShape.Line,end:f.Position,position:previous.Position);if(previous!=null&&Relic(2))Emit(HeroRuleAction.Slow,"spool-of-dusk",HeroRulesV3Tuning.LineWidth,HeroRulesV3Tuning.WeaveSeconds,HeroRulesV3Tuning.WeaveSlow,HeroRuleShape.Line,end:f.Position,position:previous.Position);previous=f;} break;
                case "HourglassStag": activeUntil=time+HeroRulesV3Tuning.HourglassSeconds; Emit(HeroRuleAction.Slow,"hourglass",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.HourglassSeconds,Relic(1)?HeroRulesV3Tuning.HourglassRelicSlow:HeroRulesV3Tuning.HourglassSlow); if(Relic(2))Emit(HeroRuleAction.SkillSpeed,"glass-bulb",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.HourglassSeconds,HeroRulesV3Tuning.HourglassSkill);break;
                case "KiteAdmiral": Emit(HeroRuleAction.Fleet,"fleet",HeroRulesV3Tuning.LineWidth,HeroRulesV3Tuning.DefaultDuration,Weapon(),HeroRuleShape.Line,hero.Position+hero.Facing*HeroRulesV3Tuning.LineLength*(Relic(0)?HeroRulesV3Tuning.FleetRelicLength:1)); if(Relic(1))Emit(HeroRuleAction.Damage,"flagship",HeroRulesV3Tuning.Radius,value:Weapon(),position:Cluster()); break;
                case "SundaeDuchess": Vector2 center=Cluster(); float radius=HeroRulesV3Tuning.Radius*(Relic(0)?HeroRulesV3Tuning.SundaeRadiusRelic:1); Emit(HeroRuleAction.Freeze,"sundae",radius,HeroRulesV3Tuning.SundaeFreeze,position:center); Emit(HeroRuleAction.Heal,"sundae-heal",radius,value:Relic(3)?HeroRulesV3Tuning.SundaeRelicHeal:HeroRulesV3Tuning.SundaeHeal,position:center); if(Relic(1))Emit(HeroRuleAction.Damage,"cherry-bomb",radius,value:Weapon(),position:center); break;
                case "JugglingJester": Emit(HeroRuleAction.Juggle,"juggle",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.JugglePeriod,Weapon(),HeroRuleShape.Ring,count:Relic(0)?HeroRulesV3Tuning.JuggleRelicCap:HeroRulesV3Tuning.JuggleCap);break;
                case "BellDiver": diveUntil=time+(Relic(0)?HeroRulesV3Tuning.DiveRelicSeconds:HeroRulesV3Tuning.DiveSeconds); Emit(HeroRuleAction.Untargetable,"dive",duration:diveUntil-time,friendId:hero.Id);break;
                case "TinVirtuoso": lastBeat=time; Emit(HeroRuleAction.Counter,"metronome-beat",duration:HeroRulesV3Tuning.BeatWindow,value:HeroRulesV3Tuning.BeatDamage);break;
            }
        }
        private float Period()
        {
            switch(HeroId)
            {
                case "ButtonCavalier":return Relic(1)?HeroRulesV3Tuning.ChargeRelicPeriod:HeroRulesV3Tuning.ChargePeriod;
                case "CitrusMarshal":return HeroRulesV3Tuning.SignPeriod;
                case "LanternWeaver":return HeroRulesV3Tuning.WeaveSeconds;
                case "HourglassStag":return Relic(0)?HeroRulesV3Tuning.HourglassRelicPeriod:HeroRulesV3Tuning.HourglassPeriod;
                case "KiteAdmiral":return Relic(2)?HeroRulesV3Tuning.FleetRelicPeriod:HeroRulesV3Tuning.FleetPeriod;
                case "SundaeDuchess":return HeroRulesV3Tuning.SundaePeriod;
                case "JugglingJester":return HeroRulesV3Tuning.JugglePeriod;
                case "BellDiver":return HeroRulesV3Tuning.DivePeriod;
                case "TinVirtuoso":return Relic(0)?HeroRulesV3Tuning.BeatRelicPeriod:HeroRulesV3Tuning.BeatPeriod;
                case "ChinaMarionette":return HeroRulesV3Tuning.DefaultDuration;
                default:return 0;
            }
        }
        public void OnCoins(int amount,bool collected=true)
        {
            if(!collected||amount<0||hero==null||!hero.Alive)return;
            coinCharge+=amount; collectedCoins+=amount;
            if(HeroId=="TideSinger"&&Relic(2)&&amount>0) { int pearls=collectedCoins/HeroRulesV3Tuning.PearlCoins-(collectedCoins-amount)/HeroRulesV3Tuning.PearlCoins; if(pearls>0)Emit(HeroRuleAction.Counter,"bonus-pearl",count:pearls); }
            int threshold=0;
            switch(HeroId)
            {
                case "Postmaster":threshold=Relic(0)?HeroRulesV3Tuning.ExpressCoins:HeroRulesV3Tuning.ParcelCoins;break;
                case "GlassDice":threshold=Relic(0)?HeroRulesV3Tuning.LoadedCoins:HeroRulesV3Tuning.DiceCoins;break;
                case "TideSinger":threshold=HeroRulesV3Tuning.TideCoins;break;
                case "TreasureAcorn":threshold=Relic(0)?HeroRulesV3Tuning.StampCoins:HeroRulesV3Tuning.TaxCoins;break;
                case "VerdantMatron":threshold=HeroRulesV3Tuning.SproutCoins;break;
                case "ThornTreasurer":threshold=Relic(0)?HeroRulesV3Tuning.ThornRelicCoins:HeroRulesV3Tuning.ThornCoins;break;
            }
            if(threshold<=0)return;
            while(coinCharge>=threshold)
            {
                if(HeroId=="Postmaster"&&time<next)break;
                coinCharge-=threshold;
                switch(HeroId)
                {
                    case "Postmaster": next=time+HeroRulesV3Tuning.StrideCooldown; Emit(HeroRuleAction.Damage,"parcel-express",HeroRulesV3Tuning.LineWidth,value:Weapon(),shape:HeroRuleShape.Line,end:Cluster()); if(Relic(1))Emit(HeroRuleAction.Mark,"registered-mail",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.DefaultDuration,position:Cluster()); if(Relic(3))Emit(HeroRuleAction.Poison,"dead-letter",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.DefaultDuration,position:Cluster());break;
                    case "GlassDice": if(Relic(1)&&Roll(HeroRulesV3Tuning.DieFaces)==0&&Roll(HeroRulesV3Tuning.DieFaces)==0)coinCharge+=threshold; Emit(HeroRuleAction.Damage,"loaded-dice",HeroRulesV3Tuning.Radius,value:Weapon()*(Relic(3)&&EnemyCount(HeroRulesV3Tuning.Radius)>=HeroRulesV3Tuning.LargeCrowd?HeroRulesV3Tuning.HouseEdge:1));break;
                    case "TideSinger": Emit(HeroRuleAction.Heal,"tide-collection",Relic(1)?HeroRulesV3Tuning.Radius:0,value:HeroRulesV3Tuning.TideHeal/MortalityRuntime.MaxHealth(hero),friendId:Relic(1)?0:hero.Id);Emit(HeroRuleAction.Push,"tide-push",HeroRulesV3Tuning.Radius,value:HeroRulesV3Tuning.TidePush*(Relic(0)?HeroRulesV3Tuning.PushRelic:1));break;
                    case "TreasureAcorn": Emit(HeroRuleAction.Coins,"treasure-tax",count:1);Emit(HeroRuleAction.Heal,"tax-pulse",HeroRulesV3Tuning.Radius,value:HeroRulesV3Tuning.TaxHeal+(Relic(1)?HeroRulesV3Tuning.TaxExtraHeal:0));break;
                    case "VerdantMatron": if(sprouts<(Relic(3)?HeroRulesV3Tuning.SproutRelicCap:HeroRulesV3Tuning.SproutCap)){sprouts++;Emit(HeroRuleAction.SummonSprout,"seed-basket",HeroRulesV3Tuning.Radius,Relic(0)?HeroRulesV3Tuning.ArenaRadius:0,Weapon(),count:1); if(Relic(1))Emit(HeroRuleAction.TraitSignal,"sprout-healing",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.ArenaRadius,HeroRulesV3Tuning.SproutHeal); if(Relic(2))Emit(HeroRuleAction.TraitSignal,"sprout-thorns",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.ArenaRadius,Weapon());}break;
                    case "ThornTreasurer":thorns++;Emit(HeroRuleAction.Damage,"thorn-purse",HeroRulesV3Tuning.Radius,value:Weapon(),shape:HeroRuleShape.Ring);Emit(HeroRuleAction.Poison,"thorn-poison",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.DefaultDuration);if(Relic(2))Emit(HeroRuleAction.Stun,"gilt-prick",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.ThornStun);if(Relic(3)&&thorns%HeroRulesV3Tuning.ThornCoinEvery==0)Emit(HeroRuleAction.Coins,"coin-spike",count:1);break;
                }
            }
        }
        public void OnFood(Vector2 position)
        {
            if(HeroId!="SupperMortar")return;
            Vector2 center=Cluster();float radius=HeroRulesV3Tuning.Radius*(Relic(0)?HeroRulesV3Tuning.SupperRadiusRelic:1);
            Emit(HeroRuleAction.Damage,"supper",radius,value:Weapon()*(Relic(1)?2:1),position:center);
            Emit(HeroRuleAction.Heal,"supper-heal",radius,value:HeroRulesV3Tuning.SupperHeal*(Relic(1)?2:1),position:center);
            Emit(HeroRuleAction.Burn,"supper-burn",radius,HeroRulesV3Tuning.DefaultDuration,position:center);
            if(Relic(3))Emit(HeroRuleAction.Cleanse,"napkin",radius,position:center);
        }
        public void OnPickup()
        { if(HeroId=="RootOracle"&&Relic(1))Emit(HeroRuleAction.Heal,"root-cellar",value:HeroRulesV3Tuning.RootPickupHeal,friendId:hero.Id); }
        public void OnSpecies(UnitState enemy)
        {
            if(HeroId!="ArchiveCurator"||enemy==null||enemy.Definition==null)return;
            string species=enemy.Definition.Id;
            if(!seenWave.Add(species))return;
            if(!run.Species.Add(species))return;
            Emit(HeroRuleAction.Coins,"catalogue-coin",count:Relic(1)?2:1,species:species);
            if(enemy.Definition.IsBoss&&Relic(2)) {run.Species.Add(species+"-boss2");run.Species.Add(species+"-boss3");}
            if(Relic(3))Emit(HeroRuleAction.Stun,"camera-flash",HeroRulesV3Tuning.ArenaRadius,HeroRulesV3Tuning.CatalogueStun,shape:HeroRuleShape.Arena,species:species);
        }
        public void OnKill(UnitState enemy,bool critical=false)
        {
            if(hero==null||!hero.Alive)return;
            kills+=enemy!=null&&enemy.Elite&&HeroId=="NightMaw"&&Relic(3)?HeroRulesV3Tuning.EliteKills:1;
            if(HeroId=="NightMaw")Emit(HeroRuleAction.Heal,"devour",value:Relic(0)?HeroRulesV3Tuning.MawRelicHeal:HeroRulesV3Tuning.MawHeal,friendId:hero.Id);
            if(HeroId=="CopperCauldron"&&Relic(3)&&activeUntil>time)Emit(HeroRuleAction.Coins,"soup-ticket",count:1);
            if(HeroId=="PennyBroker"&&Relic(3)&&Roll(HeroRulesV3Tuning.LooseEvery)==0)Emit(HeroRuleAction.Coins,"loose-change",count:1);
            if(HeroId=="LoneJelly"&&Relic(3))Emit(HeroRuleAction.Counter,"glow-pearl",position:enemy?.Position??hero.Position,count:1);
        }
        public void OnHeroHit()
        { if(HeroId=="StormFencer") {riposteAt=time+(Relic(2)?HeroRulesV3Tuning.RiposteRelicDelay:HeroRulesV3Tuning.RiposteDelay);riposte=false;} }
        public void OnFriendFell(UnitState friend)
        {
            if(friend==null)return;
            if(HeroId=="VelvetRegent")
            { activeUntil=time+HeroRulesV3Tuning.RoyalSeconds+(Relic(1)?HeroRulesV3Tuning.RoyalExtraSeconds:0);Emit(HeroRuleAction.ReviveSooner,"royal-guard",value:HeroRulesV3Tuning.RoyalRevive+(Relic(2)?HeroRulesV3Tuning.RoyalExtraRevive:0),friendId:friend.Id); }
            if(HeroId=="ChinaMarionette"&&Relic(3))Emit(HeroRuleAction.ReviveSooner,"spare-doll",value:HeroRulesV3Tuning.RoyalExtraRevive,friendId:friend.Id);
        }
        public void OnFriendRose(UnitState friend)
        { if(HeroId=="VelvetRegent"&&Relic(3))Emit(HeroRuleAction.Shield,"regent-seal",duration:HeroRulesV3Tuning.DefaultDuration,value:HeroRulesV3Tuning.RoyalShield,friendId:friend.Id); }
        public void OnFriendCast(int friendId,string skillId,float strength=1,bool isCopy=false)
        {
            if(HeroId!="PatchworkHost"||isCopy||time<patchAt)return;
            copiedSkill=skillId;copiedValue=strength;patchAt=time+(Relic(1)?0:HeroRulesV3Tuning.PatchPeriod);
            Emit(HeroRuleAction.CopySkill,"patchwork",value:strength*(Relic(0)?HeroRulesV3Tuning.PatchRelicCopy:HeroRulesV3Tuning.PatchCopy),friendId:friendId,species:skillId);
        }
        public void OnShot()
        {
            if(HeroId=="StormFencer"&&riposte)
            { riposte=false;Emit(HeroRuleAction.Damage,"riposte",HeroRulesV3Tuning.Radius,value:Weapon(),position:Cluster(),count:Relic(0)?HeroRulesV3Tuning.RiposteRelicClusters:HeroRulesV3Tuning.RiposteClusters); if(Relic(1))Emit(HeroRuleAction.BlockBullets,"foil-guard",HeroRulesV3Tuning.Radius,count:1);if(Relic(3))Emit(HeroRuleAction.SpeedBuff,"storm-cloak",duration:HeroRulesV3Tuning.RiposteSpeedSeconds,value:HeroRulesV3Tuning.RiposteSpeed,friendId:hero.Id); }
            if(HeroId=="SoapSkater"&&distance>=HeroRulesV3Tuning.SlideDistance/(Relic(1)?HeroRulesV3Tuning.SlideFillRelic:1))
            {distance=0;Vector2 end=hero.Position+hero.Facing*HeroRulesV3Tuning.LineLength*(Relic(0)?HeroRulesV3Tuning.SlideLengthRelic:1); Emit(HeroRuleAction.Slow,"slide",HeroRulesV3Tuning.LineWidth,HeroRulesV3Tuning.SlideSeconds,HeroRulesV3Tuning.SlideSlow,HeroRuleShape.Line,end);if(Relic(2))Emit(HeroRuleAction.Stun,"foam-blind",HeroRulesV3Tuning.LineWidth,HeroRulesV3Tuning.BlindSeconds,shape:HeroRuleShape.Line,end:end);if(Relic(3))Emit(HeroRuleAction.Cleanse,"rinse",HeroRulesV3Tuning.Radius);}
            if(HeroId=="CopperCauldron"&&activeUntil>time)Emit(HeroRuleAction.Burn,"boiling-shots",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.DefaultDuration,position:Cluster());
            if(HeroId=="TinVirtuoso")
            { bool onBeat=time-lastBeat<=HeroRulesV3Tuning.BeatWindow;beatStreak=onBeat?beatStreak+1:0;if(onBeat&&Relic(3)&&beatStreak%HeroRulesV3Tuning.BeatChainEvery==0)Emit(HeroRuleAction.Damage,"tin-whistle",HeroRulesV3Tuning.Radius,value:Weapon(),position:Cluster()); }
        }
        public bool NextShotCritical => HeroId=="StormFencer"&&riposte;
        public bool NextShotPierces => HeroId=="TinVirtuoso"&&Relic(1)&&time-lastBeat<=HeroRulesV3Tuning.BeatWindow;
        public float ModifyOutgoing(UnitState attacker,UnitState enemy,float damage)
        {
            if(attacker==null||attacker!=hero)return damage;
            float bonus=0;
            switch(HeroId)
            {
                case "VelvetRegent": if(activeUntil>time)bonus+=Relic(0)?HeroRulesV3Tuning.RoyalRelicDamage:HeroRulesV3Tuning.RoyalDamage;break;
                case "NightMaw": bonus+=kills/(Relic(1)?HeroRulesV3Tuning.MawRelicKills:HeroRulesV3Tuning.MawKills)*HeroRulesV3Tuning.MawGrowth;if(Relic(2)&&hero.Health>=MortalityRuntime.MaxHealth(hero)*HeroRulesV3Tuning.MawHighHealth)bonus+=HeroRulesV3Tuning.MawScarf;break;
                case "IvorySovereign":if(run.Decree==1||run.Decree==3)bonus+=HeroRulesV3Tuning.DecreeDamage;break;
                case "ArchiveCurator":bonus+=run.Species.Count*HeroRulesV3Tuning.CatalogueDamage;if(Relic(0)&&enemy!=null&&run.Species.Contains(enemy.Definition.Id))bonus+=HeroRulesV3Tuning.CatalogueMark;break;
                case "RainArchivist": if(Relic(2)&&time>=(Relic(0)?0:HeroRulesV3Tuning.RainDelay))bonus+=HeroRulesV3Tuning.RainDamage;break;
                case "PennyBroker":bonus+=Mathf.Min(wavePurchases*HeroRulesV3Tuning.PurchaseDamage,Relic(1)?HeroRulesV3Tuning.PurchaseCap:HeroRulesV3Tuning.PurchaseDamage);break;
                case "TinVirtuoso":if(time-lastBeat<=HeroRulesV3Tuning.BeatWindow)bonus+=HeroRulesV3Tuning.BeatDamage;break;
                case "MoonProwler":if(prowl){prowl=false;still=0;damage*=HeroRulesV3Tuning.ProwlDamage;if(Relic(1))Emit(HeroRuleAction.Mark,"moon-dust",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.DefaultDuration,position:enemy?.Position);if(Relic(3))Emit(HeroRuleAction.Stun,"silver-claw",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.ProwlStun,position:enemy?.Position);}break;
            }
            return damage*(1+bonus);
        }
        public float ModifyIncoming(UnitState friend,float damage)
        {
            if(friend==hero&&Untargetable)return 0;
            if(HeroId!="ChinaMarionette"||friend==hero||friend==null||!friend.Alive||!hero.Alive)return damage;
            float radius=Relic(0)?HeroRulesV3Tuning.SilkRadius:HeroRulesV3Tuning.StringsRadius;
            if((friend.Position-hero.Position).sqrMagnitude>radius*radius)return damage;
            float shared=Mathf.Min(damage*HeroRulesV3Tuning.StringsReduction,MortalityRuntime.MaxHealth(hero)*HeroRulesV3Tuning.SharedCap);
            Emit(HeroRuleAction.TraitSignal,"strings-shared",value:shared*(Relic(1)?1-HeroRulesV3Tuning.PowderReduction:1),friendId:hero.Id);
            return damage*(1-HeroRulesV3Tuning.StringsReduction);
        }
        public float FriendOutgoingBonus(UnitState friend)
        { return HeroId=="TinVirtuoso"&&Relic(2)&&time-lastBeat<=HeroRulesV3Tuning.BeatWindow ? HeroRulesV3Tuning.FriendBeatDamage : 0; }
        public void OnBulletsHandled(int count)
        { if(count>0&&HeroId=="DrowsySeer"&&Relic(1))Emit(HeroRuleAction.Heal,"night-cap",value:count*HeroRulesV3Tuning.NightCapHeal,friendId:hero.Id);if(count>0&&HeroId=="MirrorKeeper"&&Relic(3))Emit(HeroRuleAction.Heal,"cracked-frame",value:count*HeroRulesV3Tuning.MirrorHeal,friendId:hero.Id); }
        public bool ReturnMissedParcels => HeroId=="Postmaster"&&Relic(2);
        public bool ReflectShotsCritical => HeroId=="MirrorKeeper"&&Relic(1);
        public int ReflectedShotCopies => HeroId=="MirrorKeeper"&&Relic(2)?HeroRulesV3Tuning.ReflectionCopies:1;
        public float SoldFriendRefund => HeroId=="PennyBroker"&&Relic(2)?1:0;
        public int ChestTierBonus => HeroId=="TreasureAcorn"&&Relic(3)?1:0;
        public int SeaShantyMemberDiscount => HeroId=="TideSinger"&&Relic(3)?1:0;
        public float BodySizeBonus => HeroId=="NightMaw"?kills/(Relic(1)?HeroRulesV3Tuning.MawRelicKills:HeroRulesV3Tuning.MawKills)*HeroRulesV3Tuning.MawGrowth:0;
        public int ExtraBurnStacks => HeroId=="CopperCauldron"&&Relic(2)&&activeUntil>time?1:0;
        public float HeldGoldDamage(int gold) => HeroId=="TreasureAcorn"&&Relic(2)?Mathf.Min(Mathf.Max(0,gold)*HeroRulesV3Tuning.StockPerGold,HeroRulesV3Tuning.StockCap):0;
        public float FriendCriticalBonus(UnitState friend) => HeroId=="ChinaMarionette"&&Relic(2)&&friend!=null&&(friend.Position-hero.Position).sqrMagnitude<=(Relic(0)?HeroRulesV3Tuning.SilkRadius:HeroRulesV3Tuning.StringsRadius)*(Relic(0)?HeroRulesV3Tuning.SilkRadius:HeroRulesV3Tuning.StringsRadius)?HeroRulesV3Tuning.StringsCrit:0;
        public void OnShopElapsed(float seconds) {if(HeroId=="BonsaiClocksmith"&&Relic(3)&&seconds>0)run.ShopSkillGrowth+=seconds*HeroRulesV3Tuning.MossSpeed;}
        public void OnJuggleBounce(int bounce)
        {
            if(HeroId!="JugglingJester")return;
            if(Relic(2)&&bounce%HeroRulesV3Tuning.JuggleStunBounce==0)Emit(HeroRuleAction.Stun,"bell-cap",HeroRulesV3Tuning.Radius,HeroRulesV3Tuning.JuggleStun,position:Cluster());
            if(Relic(3))Emit(HeroRuleAction.Heal,"acrobat-mat",value:HeroRulesV3Tuning.JuggleHeal,friendId:hero.Id);
        }
        public float CopiedShieldExtraDuration => HeroId=="PatchworkHost"&&Relic(3)?HeroRulesV3Tuning.PatchShieldExtra:0;
        public bool CopiedHealsGlobal => HeroId=="PatchworkHost"&&Relic(2);
        public bool FleetFollowsWind => HeroId=="KiteAdmiral"&&Relic(3);
        private int Roll(int sides)
        { uint n=run.RandomState; n^=n<<HeroRulesV3Tuning.RandomShiftA; n^=n>>HeroRulesV3Tuning.RandomShiftB; n^=n<<HeroRulesV3Tuning.RandomShiftC;run.RandomState=n;return (int)(n%(uint)sides); }
        private HeroRuleOptions Options(HeroRuleAction action,string rule)
        {
            HeroRuleOptions options=HeroRuleOptions.None;
            if(rule=="parcel-express"&&ReturnMissedParcels)options|=HeroRuleOptions.ReturnIfMissed;
            if(action==HeroRuleAction.ReflectBullets){if(ReflectShotsCritical)options|=HeroRuleOptions.Critical;if(ReflectedShotCopies>1)options|=HeroRuleOptions.SplitReflections;}
            if(action==HeroRuleAction.CopySkill){if(CopiedHealsGlobal)options|=HeroRuleOptions.GlobalCopiedHeal;if(CopiedShieldExtraDuration>0)options|=HeroRuleOptions.ExtendCopiedShield;}
            if(action==HeroRuleAction.Fleet&&FleetFollowsWind)options|=HeroRuleOptions.FollowWind;
            if(action==HeroRuleAction.Burn&&ExtraBurnStacks>0)options|=HeroRuleOptions.BurnExtraStack;
            if(action==HeroRuleAction.Juggle){if(Relic(1))options|=HeroRuleOptions.Critical;if(Relic(2))options|=HeroRuleOptions.JuggleStun;if(Relic(3))options|=HeroRuleOptions.JuggleHeal;}
            if(rule=="foam-blind")options|=HeroRuleOptions.Blind;
            if(rule=="thorn-purse"&&Relic(1)||rule=="sundae"&&Relic(2)||rule=="weave"&&Relic(3))options|=HeroRuleOptions.Critical;
            return options;
        }
        private float Weapon()=>hero?.Definition?.Weapon?.Damage??0;
        private static bool Finite(float n)=>!float.IsNaN(n)&&!float.IsInfinity(n);
        private static float Radius(float radius,float scale)=>radius*scale;
        private string FirstEnemySpecies()
        {for(int i=0;i<units.Count;i++)if(units[i].Alive&&units[i].Faction==Faction.Enemy)return units[i].Definition.Id;return null;}
        private int EnemyCount(float radius)
        { int count=0;float rr=radius*radius;for(int i=0;i<units.Count;i++){var e=units[i];if(e.Alive&&e.Faction==Faction.Enemy&&(e.Position-hero.Position).sqrMagnitude<=rr)count++;}return count; }
        // A fixed spatial bin count chooses a cluster without quadratic enemy-to-enemy searches.
        private readonly int[] bins=new int[HeroRulesV3Tuning.ClusterBins];
        private readonly Vector2[] sums=new Vector2[HeroRulesV3Tuning.ClusterBins];
        private Vector2 Cluster()
        {
            Array.Clear(bins,0,bins.Length);Array.Clear(sums,0,sums.Length);
            int best=0;
            for(int i=0;i<units.Count;i++)
            {var e=units[i];if(!e.Alive||e.Faction!=Faction.Enemy)continue;Vector2 d=e.Position-hero.Position;int x=Mathf.Clamp(Mathf.FloorToInt((d.x+HeroRulesV3Tuning.ClusterExtent)/HeroRulesV3Tuning.ClusterCell),0,HeroRulesV3Tuning.ClusterSide-1);int y=Mathf.Clamp(Mathf.FloorToInt((d.y+HeroRulesV3Tuning.ClusterExtent)/HeroRulesV3Tuning.ClusterCell),0,HeroRulesV3Tuning.ClusterSide-1);int k=x+y*HeroRulesV3Tuning.ClusterSide;bins[k]++;sums[k]+=e.Position;if(bins[k]>bins[best])best=k;}
            return bins[best]>0?sums[best]/bins[best]:hero.Position+hero.Facing*HeroRulesV3Tuning.LineLength;
        }
        private void Emit(HeroRuleAction action,string rule,float radius=0,float duration=0,float value=0,HeroRuleShape shape=HeroRuleShape.Circle,Vector2 end=default,int friendId=0,int count=0,string species=null,Vector2? position=null)
        { Requested?.Invoke(new HeroRuleRequest(action,rule,hero?.Id??0,position??hero?.Position??Vector2.zero,radius,duration,value,shape,end,friendId,count,species,Options(action,rule))); }
    }
}
