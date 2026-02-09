using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class HeroIdentityChecks
    {
        [Serializable] private sealed class Report { public int Checks, Heroes, Approaches, Companions, Items; public bool Passed; public string[] Results; }
        private static readonly List<string> notes=new List<string>();
        private static string Root=>"PreviewExports/HeroIdentity";
        private static void Check(bool valid,string label)
        { notes.Add((valid?"PASS ":"FAIL ")+label);Directory.CreateDirectory(Root);File.WriteAllLines(Root+"/Checks.txt",notes);if(!valid)throw new InvalidOperationException("Hero identity: "+label); }
        public static void Run()
        {
            notes.Clear();RunCatalog c=Resources.Load<RunCatalog>("RunCatalog");
            Check(HeroIdentities.All.Length==31 && c.Champions.All(u=>HeroIdentities.For(u.Id)!=null),"Every one of 31 playable IDs has a signature");
            Check(HeroIdentities.All.Select(d=>d.Name).Distinct().Count()==31 && HeroIdentities.All.All(d=>d.Approaches.Length==3),"31 distinct mechanic names and 93 approaches");
            foreach(UnitDefinition hero in c.Champions)
            for(int approach=0;approach<3;approach++)Exercise(c,hero,approach);
            CheckDescriptions(c);
            CheckPurchaseCoverage(c);
            CheckTransactions(c);
            var report=new Report{Checks=notes.Count,Heroes=c.Champions.Length,Approaches=93,Companions=c.Companions.Length,Items=c.Items.Length,Passed=true,Results=notes.ToArray()};
            File.WriteAllText(Root+"/Verification.json",JsonUtility.ToJson(report,true));
            Debug.Log("HERO_IDENTITY_PASS "+notes.Count);
        }
        private static void Exercise(RunCatalog c,UnitDefinition hero,int approach)
        {
            using var s=new RunSession(c);s.Start(hero.Id,641031);s.State.Wave=12;s.State.Gold=300;
            s.State.HeroIdentity.Approach=approach;
            if(hero.Id=="LoneJelly")s.State.AbsorbedSkills.AddRange(new[]{"TeapotPilgrim","HoodWisp","BrassAcorn"});
            var a=s.PrepareCombat();a.EnemyBudget=0;a.WaveDuration=90;a.Boss=null;a.EnableArenaToys=a.EnableSpectacles=false;
            var model=RunBuildReadModel.Capture(s,c,s.State.Health);
            Check(model.Stats[0].Name.StartsWith("Hero / ") && model.Stats[0].Summary.Contains(HeroIdentities.For(hero.Id).Name),"Identity row does not overwrite stat summaries "+hero.Id+" / "+approach);
            Check(model.Stats.Find(r=>r.Name=="Champion damage").Summary==a.Champion.Weapon.Damage.ToString("0.0"),"Damage row keeps correct compiled value "+hero.Id+" / "+approach);
            var sim=new CombatSimulation(a);sim.Start(641031);
            var projectiles=(List<ProjectileState>)typeof(CombatSimulation).GetField("projectiles",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(sim);
            var heal=typeof(CombatSimulation).GetMethod("Heal",BindingFlags.Instance|BindingFlags.NonPublic);
            UnitDefinition dummy=UnityEngine.Object.Instantiate(a.MeleeEnemy),weak=UnityEngine.Object.Instantiate(a.MeleeEnemy);
            dummy.Weapon=UnityEngine.Object.Instantiate(dummy.Weapon);dummy.Speed=0;dummy.Health=1000000;dummy.ContactDamage=0;dummy.CollisionDamage=0;dummy.Behavior=EnemyBehavior.Standard;dummy.SpecialEnemy=SpecialEnemyKind.None;
            weak.Weapon=UnityEngine.Object.Instantiate(weak.Weapon);weak.Speed=0;weak.Health=1;weak.ContactDamage=0;weak.CollisionDamage=0;weak.Behavior=EnemyBehavior.Standard;weak.SpecialEnemy=SpecialEnemyKind.None;
            var foes=new List<UnitState>();
            for(int i=0;i<3;i++){var u=sim.AddFixture(dummy,Faction.Enemy,new Vector2(1+i*.45f,(i-1)*.5f));u.NextAttack=u.NextContact=float.PositiveInfinity;foes.Add(u);}
            float maxGlyphs=0;bool finite=true;bool injurySeen=false;bool capped=true;bool blastsFit=true;int blasts=0;float lastBlast=-1;bool blastSpaced=true;string oversize="";
            var stage=new GameObject("Hero mechanic visual check");var visual=stage.AddComponent<HeroIdentityVisual>();int painted=0;
            for(int tick=0;tick<720;tick++)
            {
                sim.Champion.Health=sim.Champion.Definition.Health;
                Vector2 movement=hero.Id=="MirrorKeeper"?Vector2.zero:hero.Id=="DrowsySeer" ? tick%120<55?Vector2.right:Vector2.zero : (Vector2)(Quaternion.Euler(0,0,(tick/30%4)*90)*Vector2.right);
                if(hero.Id!="MoonProwler")for(int i=0;i<foes.Count;i++)foes[i].Position=sim.Champion.Position+new Vector2(1.1f+i*.4f,(i-1)*.6f);
                if(tick%20==0)
                {
                    sim.LootFixture(LootKind.Coin,sim.Champion.Position,6);
                    sim.LootFixture(LootKind.Chest,sim.Champion.Position,2);
                    var meal=sim.AddFixture(weak,Faction.Enemy,sim.Champion.Position+Vector2.down*.5f);meal.NextAttack=meal.NextContact=float.PositiveInfinity;
                    if(hero.Id=="ChinaMarionette"){sim.Champion.Health-=8;heal.Invoke(sim,new object[]{8f,new CombatCause(100000+tick,sim.Champion.Id,"fixture-recovery")});}
                    if(hero.Id=="MirrorKeeper")projectiles.Add(new ProjectileState{Id=900000+tick,Faction=Faction.Enemy,Position=sim.Champion.Position+Vector2.left*2,Velocity=Vector2.right*.2f,Remaining=3,Radius=.05f,Damage=1});
                    else if(hero.Id=="ButtonCavalier"||hero.Id=="StormFencer")projectiles.Add(new ProjectileState{Id=900000+tick,Faction=Faction.Enemy,Position=sim.Champion.Position+Vector2.right*.7f,Velocity=Vector2.right,Remaining=1,Radius=.05f,Damage=1});
                    else projectiles.Add(new ProjectileState{Id=900000+tick,Faction=Faction.Enemy,Position=sim.Champion.Position+Vector2.up*.8f,Velocity=Vector2.down*3,Remaining=1.2f,Radius=.06f,Damage=1});
                }
                sim.Step(.05f,movement);
                injurySeen|=sim.OctoberInjuries>0;maxGlyphs=Mathf.Max(maxGlyphs,sim.HeroIdentityGlyphs.Count);
                foreach(var g in sim.HeroIdentityGlyphs)
                {
                    if(g.Role!=HeroGlyphRole.Strike)continue;
                    float limit=g.Shape==MorphGeometry.Crescent?CombatSimulation.MaxCrescentRadius:g.Shape==MorphGeometry.Ring?CombatSimulation.MaxRingRadius:CombatSimulation.MaxDiscRadius;
                    bool ok=g.Shape==MorphGeometry.Line?Vector2.Distance(g.Position,g.End)<=CombatSimulation.MaxLineLength+.001f:g.Radius<=limit+.001f;
                    if(!ok){capped=false;oversize=g.Shape+" r="+g.Radius.ToString("0.00")+" len="+Vector2.Distance(g.Position,g.End).ToString("0.00");}
                }
                foreach(var ev in sim.Events)
                {
                    if(ev.Kind!=CombatEventKind.Detonation || ev.Cause.ItemId==null || !ev.Cause.ItemId.StartsWith("hero-identity-",StringComparison.Ordinal))continue;
                    blasts++;blastsFit&=ev.Value*1.4f<=CombatSimulation.MaxDiscRadius+.001f;
                    if(lastBlast>=0 && sim.Elapsed-lastBlast<.29f)blastSpaced=false;lastBlast=sim.Elapsed;
                }
                if(tick%9==0){visual.Display(sim);painted=Mathf.Max(painted,visual.VisibleCount);}
                foreach(var shot in sim.Projectiles)if(HeroIdentityVisual.Missile(shot,a.Projectile,sim.Elapsed,out _,out _)!=null)painted=Mathf.Max(painted,1);
                if(sim.Units.Any(u=>!float.IsFinite(u.Position.x)||!float.IsFinite(u.Position.y)) || !float.IsFinite(sim.HeroIdentityCharge))finite=false;
            }
            string id=hero.Id+" / "+approach;
            Check(finite && sim.Champion.Alive && sim.Units.Where(u=>u.Faction==Faction.Companion).All(u=>u.Alive&&float.IsPositiveInfinity(u.Health)),"Finite and immortal companion behavior "+id);
            Check(sim.HeroIdentityActivations>0,"Actual signature activated "+id+" casts="+sim.HeroIdentityActivations+" injuries="+injurySeen);
            Check(maxGlyphs<=40 && sim.Projectiles.Count<1100,"Bounded visual/projectile queues "+id);
            Check(capped,"Identity strike areas within caps (disc 1.6, ring 1.8, crescent 2.4, line 4.5) "+id+" "+oversize);
            Check(blastsFit && blastSpaced,"Identity detonations sized inside their disc and at most one per 0.3s "+id+" blasts="+blasts);
            float elapsed=sim.Elapsed,charge=sim.HeroIdentityCharge;int casts=sim.HeroIdentityActivations;
            sim.Pause(true);sim.Step(.05f,Vector2.one);
            Check(sim.Elapsed==elapsed && sim.HeroIdentityCharge==charge && sim.HeroIdentityActivations==casts,"Pause cannot farm mechanic "+id);
            sim.Pause(false);sim.EnterRest();Check(sim.HeroIdentityGlyphs.Count==0,"Combat glyphs cleared in rest "+id);
            Check(painted>0 && stage.GetComponentsInChildren<LineRenderer>(true).Length==0,"Painted ink mechanic (strike, live state or painted bolt) drawn without primitive outlines "+id);
            visual.Display(sim);Check(visual.VisibleCount==0 && stage.GetComponentsInChildren<Renderer>().All(r=>!r.enabled),"Mechanic ink clears completely outside combat "+id);
            UnityEngine.Object.DestroyImmediate(stage);
            s.Validate(s.State);string saved=JsonUtility.ToJson(s.State);s.Restore(JsonUtility.FromJson<RunSnapshot>(saved));
            Check(s.State.HeroIdentity.Approach==approach,"Approach save round trip "+id);
            UnityEngine.Object.DestroyImmediate(dummy.Weapon);UnityEngine.Object.DestroyImmediate(dummy);UnityEngine.Object.DestroyImmediate(weak.Weapon);UnityEngine.Object.DestroyImmediate(weak);
        }
        public static void CheckDescriptions(RunCatalog c)
        {
            var report=new System.Text.StringBuilder("# Companion and item description audit\n\nGenerated from the current runtime. Category, talents, direct support, timing, caps and tradeoffs are kept together.\n\n");
            var heroes=new System.Text.StringBuilder("# Hero mechanic and approach descriptions\n\nGenerated from the current runtime: 31 heroes, 93 approaches.\n\n");
            foreach(var u in c.Champions)
            {
                var identity=HeroIdentities.For(u.Id);string copy=RunUICopy.ChampionDescription(u);
                Check(identity.ApproachDescriptions.Length==3,"Three detailed approach descriptions "+u.Id);
                for(int i=0;i<3;i++)Check(identity.ApproachDescriptions[i].Length>60 && copy.Contains(identity.Approaches[i]) && copy.Contains(identity.ApproachDescriptions[i]),"Full mechanic UI includes approach description "+u.Id+" / "+i);
                Check(copy.Contains(HeroApproachCopy.DamageRules)&&copy.Contains("INNATE TRAITS"),"Hero copy includes damage-budget rules and innate traits "+u.Id);
                heroes.AppendLine("## "+u.DisplayName+" / "+u.Id+"\n\n"+copy+"\n");
            }
            File.WriteAllText(Root+"/Hero_Description_Audit.md",heroes.ToString());
            foreach(var u in c.Companions)
            {
                string copy=CompanionDescriptions.Describe(u,1,c.TierSupportMultiplier);
                Check(copy.Contains("SUPPORT WHILE OWNED")&&copy.Contains("PERSONAL PASSIVE")&&!copy.Contains("An original toy companion. Its listed support"),"Complete talent/support/passive copy "+u.Id);
                foreach(var m in u.CompanionModifiers??Array.Empty<StatModifier>())Check(copy.Contains(RunModifierRules.ValueText(m.Effect,m.Value)),"Signed support value present "+u.Id+" / "+m.Effect);
                for(int tier=1;tier<=3;tier++)Check(!string.IsNullOrWhiteSpace(CompanionDescriptions.Describe(u,tier,c.TierSupportMultiplier)),"Star description "+u.Id+" / "+tier);
                report.AppendLine("## "+u.DisplayName+" / "+u.Id+"\n\n"+copy+"\n");
            }
            foreach(var item in c.Items)
            {
                string copy=RelicDescriptions.Describe(item,1,c);
                Check(copy.StartsWith(RelicDescriptions.Category(item)),"Category visible "+item.Id);
                foreach(var m in item.EffectiveModifiers)if(m.Value!=0)Check(copy.Contains(RunModifierRules.ValueText(m.Effect,m.Value)),"Effective item value present "+item.Id+" / "+m.Effect);
                if(item.Mechanic!=RelicMechanic.None)Check(copy.Contains(RelicDescriptions.Mechanic(item.Mechanic)),"Special item mechanic present "+item.Id);
                if(item.SignatureRole!=SignatureRelicKind.None)Check(copy.Contains(RelicDescriptions.Signature(item.SignatureRole)),"Weapon synergy present "+item.Id);
                if(item.Category!=RelicCategory.Curio)Check(copy.Contains("ON PURCHASE: counts as 1"),"Drink/food acquisition trigger visible "+item.Id);
                foreach(var proc in c.ProcRules.Where(p=>p.ItemId==item.Id))Check(copy.Contains(proc.Trigger.ToString())&&copy.Contains(proc.Power.ToString("0.###")),"Authored proc visible "+item.Id);
                report.AppendLine("## "+item.Name+" / "+item.Id+"\n\n"+copy+"\n");
            }
            using(var s=new RunSession(c))
            {
                s.Start("TideSinger",1);s.State.Roster.Add(new RosterEntry{UnitId="TeapotPilgrim",WeaponId="InkPistol"});
                var drink=c.Items.First(i=>i.Category==RelicCategory.Drink);string reaction=RelicDescriptions.PurchaseReactions(s,c,drink);
                Check(reaction.Contains("+4%")&&reaction.Contains("Tide Singer")&&reaction.Contains("next battle"),"Drink preview names actual companion and hero reactions");
            }
            File.WriteAllText(Root+"/Description_Audit.md",report.ToString());
        }
        private static void CheckTransactions(RunCatalog c)
        {
            using var s=new RunSession(c);s.Start("LanternKnight",12);s.State.Wave=8;s.State.Phase=SessionPhase.Shop;s.State.Gold=1000;
            var offer=new ShopOffer{Token=++s.State.NextOffer,Id="key-oil",Kind=OfferKind.Item,Price=1};s.State.Offers.Add(offer);
            Check(RunTransaction.TryCommit(s,c,"test damage",()=>s.Buy(offer.Token),()=>true,out _),"Damage purchase committed");
            Check(s.LastShopStatChanges.Any(d=>d.ActorKey=="hero"&&d.Label=="Damage"&&d.After>d.Before) && s.LastShopStatChanges.Any(d=>d.ActorKey!="hero"&&d.Label=="Damage"&&d.After>d.Before),"Damage changes attributed to hero and affected allies");
            var feedback=new ShopStatFeedback();feedback.Observe(s);Check(feedback.PendingActors>0,"Shop feedback queued");
            s.State.Phase=SessionPhase.Combat;feedback.Observe(s);Check(feedback.PendingActors==0,"All stat feedback removed before combat");
            s.State.Phase=SessionPhase.Shop;int revision=s.ShopStatRevision,gold=s.State.Gold;
            Check(!RunTransaction.TryCommit(s,c,"rollback",()=>{s.State.Gold-=20;return true;},()=>false,out _)&&s.State.Gold==gold&&s.ShopStatRevision==revision,"Failed save rolls back without false stat feedback");
            var negative=c.Items.First(i=>i.EffectiveModifiers.Any(m=>m.Value<0));var original=s.State.Items.ToArray();
            var before=RunBuildReadModel.Capture(s,c,s.State.Health).ShopStats;s.State.Items.Add(new ItemStack{Id=negative.Id,Count=1});
            var after=RunBuildReadModel.Capture(s,c,s.State.Health).ShopStats;
            Check(ShopStatSnapshot.Compare(before,after).Any(d=>d.After<d.Before),"Negative changes retained in feedback model");
            using var broker=new RunSession(c);broker.Start("PennyBroker",13);broker.State.Phase=SessionPhase.Shop;broker.State.Gold=1000;
            Check(broker.PlaceHeroOrder(),"Forward order paid once");string item=broker.State.HeroIdentity.OrderItem;int price=broker.State.HeroIdentity.OrderPrice;
            Check(!broker.PlaceHeroOrder(),"Cannot duplicate order in same wave");broker.State.Wave++;
            var roll=typeof(RunSession).GetMethod("RollShop",BindingFlags.Instance|BindingFlags.NonPublic);roll.Invoke(broker,new object[]{true});
            Check(broker.State.Offers.Any(o=>o.Id==item&&o.Price==price),"Named delivery arrives at quoted price");
            broker.Reroll();Check(broker.State.Offers.Any(o=>o.Id==item&&o.Price==price),"Reroll preserves unclaimed delivery");
            broker.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(broker.State)));Check(broker.State.HeroIdentity.OrderItem==item,"Pending order survives save");
            var legacy=JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(broker.State));legacy.HeroIdentity=null;broker.Restore(legacy);Check(broker.State.HeroIdentity!=null,"Old save receives safe identity defaults");
        }
        private static void CheckPurchaseCoverage(RunCatalog c)
        {
            foreach(var item in c.Items)
            {
                using var s=new RunSession(c);s.Start("LanternKnight",553);s.State.Phase=SessionPhase.Shop;s.State.Wave=12;s.State.Gold=1000000;
                var offer=new ShopOffer{Id=item.Id,Token=++s.State.NextOffer,Kind=OfferKind.Item,Price=1};s.State.Offers.Add(offer);
                int drinks=s.State.OctoberDrinks,food=s.State.OctoberFood;
                Check(RunTransaction.TryCommit(s,c,"Coverage / "+item.Id,()=>s.Buy(offer.Token),()=>true,out string error),"Every item can produce a valid committed description/diff "+item.Id+" "+error);
                Check(s.State.OctoberDrinks==drinks+(item.Category==RelicCategory.Drink?1:0) && s.State.OctoberFood==food+(item.Category==RelicCategory.Food?1:0),"Drink/food category matches actual purchase counter "+item.Id);
                Check(s.LastShopStatChanges.Any(d=>d.Label=="Gold"&&d.After<d.Before),"Currency loss retained "+item.Id);
            }
            using(var s=new RunSession(c))
            {
                s.Start("LanternKnight",553);s.State.Phase=SessionPhase.Shop;s.State.Wave=12;s.State.Gold=1000;
                var drink=c.Items.First(i=>i.Category==RelicCategory.Drink && i.Mechanic==RelicMechanic.None && i.EffectiveModifiers.All(m=>m.Effect!=ItemEffect.Damage&&m.Effect!=ItemEffect.CompanionDamage&&m.Effect!=ItemEffect.RangedDamage&&m.Effect!=ItemEffect.MeleeDamage&&m.Effect!=ItemEffect.DamageCorrection));
                s.State.Roster.Clear();s.State.Roster.Add(new RosterEntry{UnitId="TeapotPilgrim",WeaponId="InkPistol"});s.State.Roster.Add(new RosterEntry{UnitId="HoodWisp",WeaponId="SpoonSabre"});
                var before=s.PrepareCombat();float alien=before.StartingCompanions[0].Weapon.Damage,other=before.StartingCompanions[1].Weapon.Damage;
                var offer=new ShopOffer{Id=drink.Id,Token=++s.State.NextOffer,Kind=OfferKind.Item,Price=1};s.State.Offers.Add(offer);Check(s.Buy(offer.Token),"Real drink acquisition for alien test");
                var after=s.PrepareCombat();Check(Mathf.Abs(after.StartingCompanions[0].Weapon.Damage/alien-1.04f)<.0001f && Mathf.Abs(after.StartingCompanions[1].Weapon.Damage-other)<.0001f,"Drink raises only the reactive companion by its documented 4%");
                s.State.OctoberDrinks=100;after=s.PrepareCombat();Check(Mathf.Abs(after.StartingCompanions[0].Weapon.Damage/alien-1.4f)<.0001f,"Drink companion growth respects documented 40% cap");
            }
        }
        public static void RefreshDescriptions()
        {
            RunCatalog c=Resources.Load<RunCatalog>("RunCatalog");
            foreach(var u in c.Companions){u.Description=CompanionDescriptions.Describe(u,1,c.TierSupportMultiplier);EditorUtility.SetDirty(u);}
            foreach(var item in c.Items)item.Description=RelicDescriptions.Describe(item,1,c);
            EditorUtility.SetDirty(c);AssetDatabase.SaveAssets();
            Run();
        }
        public static void VerifyAndBuild()
        {
            RefreshDescriptions();
            Directory.CreateDirectory("Builds/HeroIdentity");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ArcaneBrigade/Scenes/RunBoot.unity","Assets/ArcaneBrigade/Scenes/RunFrontend.unity","Assets/ArcaneBrigade/Scenes/RunArena.unity","Assets/ArcaneBrigade/Scenes/WaitingRoom.unity"},locationPathName="Builds/HeroIdentity/ArcaneBrigade.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText(Root+"/Build.txt","Result: "+report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nBytes: "+report.summary.totalSize);
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Identity player build failed");
            GameReviewSnapshot.Export();
        }
    }
}
