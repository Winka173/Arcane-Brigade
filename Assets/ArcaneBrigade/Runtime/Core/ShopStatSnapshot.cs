using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class ShopStatDelta
    {
        public string ActorKey, ActorName, Label, Message;
        public double Before, After;
        public bool LowerIsBetter, Neutral;
        public bool Positive => LowerIsBetter ? After < Before : After > Before;
        public string SignedChange => Math.Abs(After-Before)<.0001 && After!=Before ? (After>Before?"+":"")+(After-Before).ToString("G3") : (After-Before).ToString("+0.####;-0.####;0");
        public string Caption => Message ?? Label + " " + SignedChange + "  (" + Before.ToString("0.####") + " → " + After.ToString("0.####") + ")";
    }
    public sealed class ShopStatSnapshot
    {
        public sealed class Actor
        {
            public string Key, Name;
            public readonly Dictionary<string,double> Values=new Dictionary<string,double>();
            public readonly Dictionary<string,string> Info=new Dictionary<string,string>();
        }
        public readonly Dictionary<string,Actor> Actors=new Dictionary<string,Actor>();
        public static ShopStatSnapshot Capture(RunSession session,RunCatalog c,CheckpointCatalog a)
        {
            var snapshot=new ShopStatSnapshot();
            Actor Add(string key,UnitDefinition u,int tier)
            {
                var x=new Actor{Key=key,Name=u.DisplayName};snapshot.Actors.Add(key,x);var w=u.Weapon;
                x.Values["Damage"]=w.Damage;x.Values["Attacks/s"]=1/Mathf.Max(.001f,w.Cooldown);x.Values["Reach"]=w.Range;x.Values["Speed"]=u.Speed;
                x.Values["Crit %"]=u.CriticalChance*100;x.Values["Shots"]=w.ProjectileCount;x.Values["Pierce"]=w.ProjectilePierce;x.Values["Split"]=w.ProjectileSplit;
                x.Values["Bounce %"]=w.ProjectileBounceChance*100;x.Values["Bullet size"]=w.ProjectileRadius;x.Values["Bullet speed"]=w.ProjectileSpeed;
                x.Values["Travel distance"]=w.ProjectileLifetime*w.ProjectileSpeed;x.Values["Push"]=w.PushDistance;x.Values["Contact damage"]=u.CollisionDamage;
                x.Values["Contacts/s"]=1/Mathf.Max(.001f,u.CollisionFrequency);x.Values["Body size"]=u.BodyHeight;x.Values["Stars"]=tier;
                x.Values["Passive strength"]=u.PassiveStrength;x.Info["Form"]=u.BuildMorph+" / "+u.BuildMorphStage;
                return x;
            }
            Actor hero=Add("hero",a.Champion,1);hero.Values["Max HP"]=a.Champion.Health;hero.Values["Current HP"]=session.State.Health;
            hero.Values["Armor"]=a.Armor;hero.Values["Dodge %"]=a.Dodge*100;hero.Values["Regen HP/s"]=a.PassiveHeal;hero.Values["Heart healing"]=a.HeartHealing;
            hero.Values["Pickup"]=a.PickupRadius;hero.Values["Chest reach"]=a.ChestOpenRadius;
            hero.Values["Gold"]=session.State.Gold;hero.Values["Eggs"]=session.State.Eggs;
            foreach(ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
                if(RunModifierRules.DefaultScope(effect)==ModifierScope.Run)hero.Values["Run / "+RunModifierRules.ModifierName(effect)]=session.Bonus(effect);
            hero.Values["Next refresh cost"]=session.RerollCost;
            var identity=session.HeroIdentityDefinition;
            if(identity!=null)
            {
                var h=session.State.HeroIdentity;hero.Info["Approach"]=identity.Approaches[h.Approach];hero.Info["Featured"]=string.IsNullOrEmpty(h.LeadUnit)?"Automatic":c.Unit(h.LeadUnit).DisplayName+" / "+RunUICopy.WeaponName(h.LeadWeapon);
                hero.Info["Memories"]=string.Join(", ",Array.ConvertAll(h.Organs,id=>string.IsNullOrEmpty(id)?"Auto":c.Unit(id).DisplayName));hero.Info["Order"]=string.IsNullOrEmpty(h.OrderItem)?"":c.Item(h.OrderItem).Name;hero.Values["Next die face"]=h.DiceFace;
                if(session.State.ChampionId=="PetalCitadel")for(int i=0;i<4;i++)hero.Values[new[]{"Attack petal","Guard petal","Healing petal","Projectile petal"}[i]]=a.HeroPetals[i];
            }
            foreach(var stack in session.State.Items)hero.Values["Relic / "+c.Item(stack.Id).Name]=stack.Count;
            for(int i=0;i<session.State.Roster.Count;i++){var r=session.State.Roster[i];Add(r.UnitId+"|"+r.WeaponId,a.StartingCompanions[i],r.Tier);}
            return snapshot;
        }
        public static List<ShopStatDelta> Compare(ShopStatSnapshot before,ShopStatSnapshot after)
        {
            var changes=new List<ShopStatDelta>();
            foreach(var pair in after.Actors)
            {
                Actor next=pair.Value;
                if(!before.Actors.TryGetValue(pair.Key,out Actor previous)){changes.Add(new ShopStatDelta{ActorKey=next.Key,ActorName=next.Name,Label="Roster",Before=0,After=1,Message="JOINED / "+next.Values["Stars"]+" STAR"});continue;}
                foreach(var stat in next.Values)
                {
                    double old; if(!previous.Values.TryGetValue(stat.Key,out old)) { if(!stat.Key.StartsWith("Relic / ",StringComparison.Ordinal)) continue; old=0; }
                    if(!double.IsFinite(stat.Value) || old==stat.Value)continue;
                    changes.Add(new ShopStatDelta{ActorKey=next.Key,ActorName=next.Name,Label=stat.Key,Before=old,After=stat.Value,
                        LowerIsBetter=stat.Key=="Next refresh cost" || stat.Key=="Run / Enemy health" || stat.Key=="Run / Enemy count" || stat.Key=="Run / Enemy speed",
                        Neutral=stat.Key=="Body size" || stat.Key=="Next die face"});
                }
                foreach(var stat in previous.Values)if(stat.Key.StartsWith("Relic / ",StringComparison.Ordinal)&&!next.Values.ContainsKey(stat.Key))changes.Add(new ShopStatDelta{ActorKey=next.Key,ActorName=next.Name,Label=stat.Key,Before=stat.Value,After=0});
                foreach(var info in next.Info)if(previous.Info.TryGetValue(info.Key,out string old) && old!=info.Value)changes.Add(new ShopStatDelta{ActorKey=next.Key,ActorName=next.Name,Label=info.Key,Neutral=true,Message=info.Key+": "+(string.IsNullOrEmpty(info.Value)?"Cleared":info.Value)});
            }
            foreach(var pair in before.Actors)if(!after.Actors.ContainsKey(pair.Key))changes.Add(new ShopStatDelta{ActorKey=pair.Key,ActorName=pair.Value.Name,Label="Roster",Before=1,After=0,Neutral=true,Message="MERGED / LEFT ROSTER"});
            return changes;
        }
    }
    public sealed partial class RunSession
    {
        public int ShopStatRevision { get; private set; }
        public IReadOnlyList<ShopStatDelta> LastShopStatChanges { get; private set; } = Array.Empty<ShopStatDelta>();
        private void ResetShopStatFeedback() { LastShopStatChanges=Array.Empty<ShopStatDelta>();ShopStatRevision++; }
        public void PublishShopStatChanges(ShopStatSnapshot before,ShopStatSnapshot after)
        { LastShopStatChanges=ShopStatSnapshot.Compare(before,after);ShopStatRevision++; }
    }
}
