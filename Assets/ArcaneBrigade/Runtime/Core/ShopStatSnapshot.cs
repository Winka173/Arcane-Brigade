using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class ShopStatDelta
    {
        public string ActorKey, ActorName, Label, Message;
        public double Before, After;
        public bool LowerIsBetter, Neutral;
        /// <summary>The change caused by this one transaction, in the stat's own units ("+12%", "+4 flat", "+0.15"). The popup shows
        /// this, like Bizarre Brigade's "+5% damage", instead of the difference of a compiled total that already carries every
        /// other bonus, hero power and star multiplier (19/10/2026, Docs/Balance_Pass_2026-10-19.md).</summary>
        public string Display;
        /// <summary>Running totals of a bonus row, for the receipt ("+24% → +36%").</summary>
        public string BeforeText, AfterText;
        public bool Positive => LowerIsBetter ? After < Before : After > Before;
        public string SignedChange => Math.Abs(After-Before)<.0001 && After!=Before ? (After>Before?"+":"")+(After-Before).ToString("G3") : (After-Before).ToString("+0.####;-0.####;0");
        public string Caption => Message ?? Label + " " + (Display ?? SignedChange) + "  (" + (BeforeText ?? Before.ToString("0.####")) + " → " + (AfterText ?? After.ToString("0.####")) + ")";
    }
    public sealed class ShopStatSnapshot
    {
        public sealed class Actor
        {
            public string Key, Name;
            public readonly Dictionary<string,double> Values=new Dictionary<string,double>();
            public readonly Dictionary<string,string> Info=new Dictionary<string,string>();
            /// <summary>The bonus sources behind the compiled values (session.Bonus for this actor's scope).</summary>
            public readonly Dictionary<ItemEffect,double> Bonus=new Dictionary<ItemEffect,double>();
        }
        public readonly Dictionary<string,Actor> Actors=new Dictionary<string,Actor>();

        /// <summary>Which bonuses explain a compiled value.</summary>
        private static readonly Dictionary<string,ItemEffect[]> Explained=new Dictionary<string,ItemEffect[]>
        {
            ["Damage"]=new[]{ItemEffect.Damage,ItemEffect.ChampionDamage,ItemEffect.CompanionDamage,ItemEffect.MeleeDamage,ItemEffect.RangedDamage,ItemEffect.DamageCorrection},
            ["Attacks/s"]=new[]{ItemEffect.AttackSpeed,ItemEffect.CompanionAttackSpeed},
            ["Reach"]=new[]{ItemEffect.AttackRange},
            ["Speed"]=new[]{ItemEffect.MoveSpeed,ItemEffect.SpeedCorrection},
            ["Contact damage"]=new[]{ItemEffect.CollisionDamage},
            ["Contacts/s"]=new[]{ItemEffect.CollisionFrequency},
            ["Bullet size"]=new[]{ItemEffect.BulletSize},
            ["Bullet speed"]=new[]{ItemEffect.BulletSpeed},
            ["Travel distance"]=new[]{ItemEffect.BulletDistance},
            ["Push"]=new[]{ItemEffect.Knockback},
            ["Body size"]=new[]{ItemEffect.BodySize,ItemEffect.CompanionSize},
        };
        /// <summary>Compiled values that multiply: without a bonus change (a merge, a passive) they read as a relative change.</summary>
        private static readonly HashSet<string> Relative=new HashSet<string>{"Damage","Attacks/s","Reach","Speed","Contacts/s","Bullet size","Bullet speed","Travel distance","Push","Body size"};
        private static Dictionary<string,ItemEffect> runEffects;
        private static Dictionary<string,ItemEffect> RunEffects
        {
            get
            {
                if(runEffects!=null)return runEffects;
                runEffects=new Dictionary<string,ItemEffect>();
                foreach(ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
                    if(RunModifierRules.DefaultScope(effect)==ModifierScope.Run)runEffects["Run / "+RunModifierRules.ModifierName(effect)]=effect;
                return runEffects;
            }
        }

        public static ShopStatSnapshot Capture(RunSession session,RunCatalog c,CheckpointCatalog a)
        {
            var snapshot=new ShopStatSnapshot();
            Actor Add(string key,UnitDefinition u,int tier,bool companion)
            {
                var x=new Actor{Key=key,Name=u.DisplayName};snapshot.Actors.Add(key,x);var w=u.Weapon;
                x.Values["Damage"]=w.Damage;x.Values["Attacks/s"]=1/Mathf.Max(.001f,w.Cooldown);x.Values["Reach"]=w.Range;x.Values["Speed"]=u.Speed;
                x.Values["Crit %"]=u.CriticalChance*100;x.Values["Shots"]=w.ProjectileCount;x.Values["Pierce"]=w.ProjectilePierce;x.Values["Split"]=w.ProjectileSplit;
                x.Values["Bounce %"]=w.ProjectileBounceChance*100;x.Values["Bullet size"]=w.ProjectileRadius;x.Values["Bullet speed"]=w.ProjectileSpeed;
                x.Values["Travel distance"]=w.ProjectileLifetime*w.ProjectileSpeed;x.Values["Push"]=w.PushDistance;x.Values["Contact damage"]=u.CollisionDamage;
                x.Values["Contacts/s"]=1/Mathf.Max(.001f,u.CollisionFrequency);x.Values["Body size"]=u.BodyHeight;x.Values["Stars"]=tier;
                x.Values["Passive strength"]=u.PassiveStrength;x.Info["Form"]=u.BuildMorph+" / "+u.BuildMorphStage;
                ModifierScope scope=companion?ModifierScope.Companions:ModifierScope.Champion;
                ItemEffect flat=w.Kind==WeaponKind.Projectile?ItemEffect.RangedDamage:ItemEffect.MeleeDamage;
                foreach(var pair in Explained)
                    foreach(ItemEffect effect in pair.Value)
                    {
                        if((effect==ItemEffect.MeleeDamage || effect==ItemEffect.RangedDamage) && effect!=flat)continue;
                        if(companion?(effect==ItemEffect.ChampionDamage || effect==ItemEffect.BodySize):(effect==ItemEffect.CompanionDamage || effect==ItemEffect.CompanionAttackSpeed || effect==ItemEffect.CompanionSize))continue;
                        x.Bonus[effect]=session.Bonus(effect,scope);
                    }
                return x;
            }
            Actor hero=Add("hero",a.Champion,1,false);hero.Values["Max HP"]=a.Champion.Health;hero.Values["Current HP"]=session.State.Health;
            hero.Values["Armor"]=Mathf.RoundToInt(a.Armor/RunModifierRules.ArmorPoint);hero.Values["Dodge %"]=a.Dodge*100;hero.Values["Regen HP/s"]=a.PassiveHeal;hero.Values["Heart healing"]=a.HeartHealing;
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
            var deployed=session.ActiveRoster;
            for(int i=0;i<deployed.Count;i++){var r=deployed[i];Add(r.UnitId+"|"+r.WeaponId,a.StartingCompanions[i],r.Tier,true);}
            return snapshot;
        }

        /// <summary>A signed figure in the stat's own unit; small values keep their decimals instead of collapsing to "+&lt;1".</summary>
        public static string Signed(double value)
        {
            double size=Math.Abs(value);
            string format=size>=10?"+0;-0;0":size>=1?"+0.#;-0.#;0":"+0.##;-0.##;0";
            string text=value.ToString(format,CultureInfo.InvariantCulture);
            return text=="0"||text=="+0"||text=="-0"?(value>0?"+<0.01":"-<0.01"):text;
        }
        private static string BonusText(ItemEffect effect,double value)
        {
            string text=RunModifierRules.ValueText(effect,(float)value);
            return effect==ItemEffect.MeleeDamage || effect==ItemEffect.RangedDamage?text+" flat":text;
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
                    bool lower=stat.Key=="Next refresh cost" || stat.Key=="Run / Enemy health" || stat.Key=="Run / Enemy count" || stat.Key=="Run / Enemy speed";
                    bool neutral=stat.Key=="Body size" || stat.Key=="Next die face";
                    // the purchase's own share: the bonuses that moved, in their own units (a capped stat that did not move stays silent)
                    bool explained=false;
                    if(Explained.TryGetValue(stat.Key,out ItemEffect[] sources))
                        foreach(ItemEffect effect in sources)
                        {
                            if(!next.Bonus.TryGetValue(effect,out double now) || !previous.Bonus.TryGetValue(effect,out double was) || Math.Abs(now-was)<1e-6)continue;
                            explained=true;
                            bool percent=RunModifierRules.ValueText(effect,1).EndsWith("%",StringComparison.Ordinal);
                            changes.Add(new ShopStatDelta{ActorKey=next.Key,ActorName=next.Name,Label=stat.Key,Before=percent?was*100:was,After=percent?now*100:now,Neutral=neutral,
                                Display=BonusText(effect,now-was),BeforeText=BonusText(effect,was),AfterText=BonusText(effect,now)});
                        }
                    if(explained)continue;
                    string display;
                    if(stat.Key.StartsWith("Relic / ",StringComparison.Ordinal) || stat.Key=="Gold" || stat.Key=="Eggs")display=Signed(stat.Value-old);
                    else if(RunEffects.TryGetValue(stat.Key,out ItemEffect runEffect))display=RunModifierRules.ValueText(runEffect,(float)(stat.Value-old));
                    else if(Relative.Contains(stat.Key) && Math.Abs(old)>1e-6)display=Signed((stat.Value/old-1)*100)+"%";
                    else display=Signed(stat.Value-old);
                    changes.Add(new ShopStatDelta{ActorKey=next.Key,ActorName=next.Name,Label=stat.Key,Before=old,After=stat.Value,LowerIsBetter=lower,Neutral=neutral,Display=display});
                }
                foreach(var stat in previous.Values)if(stat.Key.StartsWith("Relic / ",StringComparison.Ordinal)&&!next.Values.ContainsKey(stat.Key))changes.Add(new ShopStatDelta{ActorKey=next.Key,ActorName=next.Name,Label=stat.Key,Before=stat.Value,After=0,Display=Signed(-stat.Value)});
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
