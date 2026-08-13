using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class CombatRepairChecks
    {
        private const string Root="PreviewExports/CombatRepair";
        public static void Audit()=>AuditCombat(false);
        public static void Verify(){AuditCombat(true);VerifyInvariants();}
        private static void VerifyInvariants()
        {
            var notes=new List<string>();
            void Check(bool ok,string label){notes.Add((ok?"PASS ":"FAIL ")+label);File.WriteAllLines(Root+"/Checks.txt",notes);if(!ok)throw new InvalidOperationException(label);}
            var c=Resources.Load<RunCatalog>("RunCatalog");var art=MechanicEffectArtwork.Current;
            Check(art!=null && art.Heroes.Length==31 && art.Heroes.Distinct().Count()==31,"Every hero has distinct illustrated mechanic art");
            Check(art.Forms.Length==16 && art.Forms.All(s=>s!=null) && art.Forms.Take(12).Distinct().Count()==12,"All twelve build themes and four supporting brush effects imported");
            var parent=new GameObject("Mechanic art verification");
            try
            {
                var view=new PaintedMechanicView(parent.transform,"Mechanic area",1080);
                foreach(MorphGeometry shape in new[]{MorphGeometry.Disc,MorphGeometry.Ring,MorphGeometry.Crescent,MorphGeometry.Line})
                {
                    view.Display(art.Form(BuildMorphKind.Chorus),shape,Vector2.zero,Vector2.right*2,Vector2.right,1.2f,0,1,.25f,Color.white,false);
                    var drawn=parent.GetComponentsInChildren<Renderer>().Where(r=>r.enabled).ToArray();
                    Check(drawn.Length>0 && drawn.All(r=>r.sortingOrder<100) && drawn.OfType<SpriteRenderer>().All(r=>r.sprite==null || !art.Forms.Contains(r.sprite)),"Mechanic area draws weapon motes or strokes on the floor, never a painted wreath: "+shape);
                }
                view.Hide();Check(parent.GetComponentsInChildren<Renderer>().All(r=>!r.enabled),"Pooled area layers clear completely");
            }
            finally{UnityEngine.Object.DestroyImmediate(parent);}
            var hover=new Vector3(.03f,.4f,0);
            Check(ShopSceneryLanding.LandingOffset(hover,0,false,0).y>1,"Shop scenery begins above the floor");
            Check(ShopSceneryLanding.LandingOffset(hover,.56f,false,0).y==0,"Shop scenery visibly lands on the fixed floor");
            Check(ShopSceneryLanding.LandingOffset(hover,.72f,false,0).y>.1f,"Landing has a small rebound");
            Check(ShopSceneryLanding.LandingOffset(hover,1.45f,false,0)==hover,"Scenery returns gently to its established hover");
            Check(Enumerable.Range(0,30).All(i=>ShopSceneryLanding.LandingOffset(hover,i*.05f,true,0)==hover),"Reduced motion suppresses the drop");
            Check(WaitingSceneryArtwork.Current?.Frames.Length==12,"Four scenery objects have complete intact/cracked/broken art");
            for(int i=0;i<NightmareGrandStage.Capacity;i++)
            {
                Check(NightmareGrandStage.DamageFor(i,1,171026)==0 && NightmareGrandStage.DamageFor(i,20,171026)==2,"Each world prop ages from intact to broken "+i);
                Vector2 p=NightmareGrandStage.AnchorFor(i,c.WorldSize,171026);
                Check(Mathf.Abs(p.x)<c.WorldSize.x*.5f && Mathf.Abs(p.y)<c.WorldSize.y*.5f,"World scenery anchor lies inside the map "+i);
            }
            Check(WorldShop.ExitPosition.x==0 && WorldShop.ExitPosition.y<-4 && WorldShop.ExitInteractionRadius>=1.8f,"Door is at bottom centre with generous reach");
            foreach(string species in c.Companions.Select(u=>u.Id))
            {
                using var run=new RunSession(c);run.Start("LanternKnight",171026);run.State.Wave=4;run.State.Roster.Clear();
                run.State.Roster.Add(new RosterEntry{UnitId=species,WeaponId=c.Unit(species).Weapon.Id});
                var combat=run.PrepareCombat();combat.EnemyBudget=0;combat.Boss=null;combat.WaveDuration=60;combat.EnableHeroMechanics=false;combat.EnableArenaToys=false;combat.EnableSpectacles=false;
                var sim=new CombatSimulation(combat);sim.Start(171026);sim.Champion.NextAttack=float.PositiveInfinity;
                var ally=sim.Units.First(u=>u.Faction==Faction.Companion);
                var dummy=UnityEngine.Object.Instantiate(combat.MeleeEnemy);dummy.Weapon=UnityEngine.Object.Instantiate(dummy.Weapon);dummy.Speed=0;dummy.Health=100000;dummy.ContactDamage=0;dummy.Behavior=EnemyBehavior.Standard;dummy.Mutation=EnemyMutation.None;
                var enemy=sim.AddFixture(dummy,Faction.Enemy,new Vector2(3.5f,0));enemy.Variant=null;enemy.NextAttack=enemy.NextContact=enemy.NextBehavior=enemy.NextEvolutionAttack=float.PositiveInfinity;
                bool hit=false;
                for(int tick=0;tick<300;tick++){sim.Step(.04f,Vector2.zero);hit|=sim.Events.Any(e=>e.Kind==CombatEventKind.Hit && e.Subject==enemy && e.Cause.SourceUnitId==ally.Id && e.Value>0);}
                Check(hit,"Companion pursues and damages a fixed enemy "+species);
                UnityEngine.Object.DestroyImmediate(dummy.Weapon);UnityEngine.Object.DestroyImmediate(dummy);
            }
            using(var run=new RunSession(c))
            {
                run.Start("LanternKnight",171026);run.State.Roster.Clear();var combat=run.PrepareCombat();combat.EnemyBudget=0;combat.Boss=null;combat.EnableHeroMechanics=false;combat.EnableArenaToys=false;combat.EnableSpectacles=false;
                combat.Champion.Weapon.Range=.4f;combat.Champion.Weapon.Cooldown=.6f;combat.Champion.Weapon.Kind=WeaponKind.Melee;
                var sim=new CombatSimulation(combat);sim.Start(171026);
                var dummy=UnityEngine.Object.Instantiate(combat.MeleeEnemy);dummy.Weapon=UnityEngine.Object.Instantiate(dummy.Weapon);dummy.Weapon.Kind=WeaponKind.Contact;dummy.Speed=0;dummy.Radius=.7f;dummy.Health=100000;dummy.ContactDamage=0;dummy.Behavior=EnemyBehavior.Standard;
                var enemy=sim.AddFixture(dummy,Faction.Enemy,new Vector2(1,0));enemy.Variant=null;enemy.NextAttack=enemy.NextContact=enemy.NextBehavior=enemy.NextEvolutionAttack=float.PositiveInfinity;
                bool fired=false;
                for(int tick=0;tick<100;tick++){sim.Step(.03f,Vector2.zero);fired|=sim.Events.Any(e=>e.Kind==CombatEventKind.Fire && e.Subject==sim.Champion);}
                Check(fired && enemy.Health<dummy.Health,"Short melee can acquire and hit a large enemy whose edge is in reach");
                dummy.Weapon.Kind=WeaponKind.Projectile;dummy.Speed=2;dummy.Weapon.Range=5;
                sim.Champion.NextAttack=float.PositiveInfinity;sim.Champion.PendingAttack=false;
                enemy.Position=enemy.PreviousPosition=new Vector2(1.5f,0);
                for(int tick=0;tick<30;tick++)sim.Step(.03f,Vector2.zero);
                Check(Vector2.Distance(enemy.Position,sim.Champion.Position)<=1.501f,"Ranged enemies never retreat when the hero approaches");
                UnityEngine.Object.DestroyImmediate(dummy.Weapon);UnityEngine.Object.DestroyImmediate(dummy);
            }
            Debug.Log("COMBAT_REPAIR_CHECKS "+notes.Count+" PASS");
        }
        private static void AuditCombat(bool enforce)
        {
            Directory.CreateDirectory(Root);
            var catalog=Resources.Load<RunCatalog>("RunCatalog");
            var rows=new List<string>{"Group\tId\tWeapon\tFires\tEffectiveDamage\tResult"};
            int failures=0,cases=0;
            void Exercise(string group,string id,string hero,string companion,string weapon)
            {
                using var run=new RunSession(catalog);run.Start(hero,171026);run.State.Wave=4;
                run.State.Roster.Clear();
                if(companion!=null)run.State.Roster.Add(new RosterEntry{UnitId=companion,WeaponId=weapon??catalog.Unit(companion).Weapon.Id});
                var combat=run.PrepareCombat();combat.Boss=null;combat.EnemyBudget=0;combat.WaveDuration=120;combat.EnableArenaToys=combat.EnableSpectacles=false;combat.PassiveHeal=0;
                var sim=new CombatSimulation(combat);sim.Start(171026);
                var actor=companion==null?sim.Champion:sim.Units.First(u=>u.Faction==Faction.Companion);
                if(companion!=null)sim.Champion.NextAttack=float.PositiveInfinity;
                var dummy=UnityEngine.Object.Instantiate(combat.MeleeEnemy);dummy.Weapon=UnityEngine.Object.Instantiate(dummy.Weapon);
                dummy.Weapon.Kind=WeaponKind.Contact;dummy.Weapon.Damage=0;dummy.Speed=0;dummy.Health=10000000;dummy.ContactDamage=0;dummy.CollisionDamage=0;dummy.Behavior=EnemyBehavior.Standard;dummy.Mutation=EnemyMutation.None;dummy.SpecialEnemy=SpecialEnemyKind.None;
                var enemy=sim.AddFixture(dummy,Faction.Enemy,actor.Position+Vector2.right*.8f);enemy.Variant=null;enemy.NextAttack=enemy.NextContact=enemy.NextBehavior=enemy.NextEvolutionAttack=float.PositiveInfinity;
                int fires=0;float damage=0;
                for(int tick=0;tick<600;tick++)
                {
                    sim.Champion.Health=sim.Champion.Definition.Health;
                    // Keep a genuine target at body-contact distance. No Fire/Hit injection.
                    enemy.Position=actor.Position+Vector2.right*(actor.Definition.Radius+dummy.Radius+.04f);
                    sim.Step(1f/30,Vector2.zero);
                    foreach(var e in sim.Events)
                    {
                        if(e.Kind==CombatEventKind.Fire && e.Subject==actor)fires++;
                        if(e.Kind==CombatEventKind.Hit && e.Subject==enemy && e.Cause.SourceUnitId==actor.Id)damage+=e.Value;
                    }
                }
                bool success=damage>.001f;cases++;if(!success)failures++;
                rows.Add(group+"\t"+id+"\t"+actor.Definition.Weapon.Id+"\t"+fires+"\t"+damage.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)+"\t"+(success?"PASS":"FAIL"));
                UnityEngine.Object.DestroyImmediate(dummy.Weapon);UnityEngine.Object.DestroyImmediate(dummy);
            }
            foreach(var hero in catalog.Champions)Exercise("Hero",hero.Id,hero.Id,null,null);
            foreach(var ally in catalog.Companions)Exercise("Companion",ally.Id,"LanternKnight",ally.Id,null);
            foreach(var weapon in catalog.Weapons)Exercise("Weapon",weapon.Id,"LanternKnight",catalog.Companions[0].Id,weapon.Id);
            File.WriteAllLines(Root+(enforce?"/Attack_Audit.tsv":"/Attack_Audit_Before.tsv"),rows);
            Debug.Log("COMBAT_ATTACK_AUDIT cases="+cases+" failures="+failures);
            if(enforce && failures>0)throw new InvalidOperationException("Actors/weapons cannot damage a reachable target: "+failures+"; inspect Attack_Audit.tsv");
        }
    }
}
