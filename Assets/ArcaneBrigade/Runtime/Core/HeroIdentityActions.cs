using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        // Budgets are in W (the hero's prepared weapon damage), tuned by Editor/HeroMechanicBalance so every hero's
        // mechanic adds roughly a third to half of a weapon hit per second in production waves (see Docs/Hero_Mechanics_Review.md).
        private void IdentityRecordHealing(float recovered, CombatCause cause)
        {
            if (HeroIdentityEnabled && !IdentityOwn(cause) && (IdentityId == "ChinaMarionette" || IdentityId == "RainArchivist")) identityRecovered += Mathf.Max(0, recovered);
        }
        private void ObserveHeroIdentityEvents()
        {
            int count = events.Count;
            for (int i = 0; i < count; i++)
            {
                CombatEvent e = events[i];
                if (IdentityOwn(e.Cause)) continue;
                bool personal = e.Cause.SourceUnitId == Champion.Id;
                bool basic = e.Cause.Depth == 0;
                if (e.Kind == CombatEventKind.Fire && e.Faction != Faction.Enemy && basic)
                {
                    UnitState performer = e.Subject ?? UnitWithId(e.Cause.SourceUnitId);
                    if (IdentityId == "LanternKnight" && Elapsed >= identitySecondAt && performer != null && (CompanionCount == 0 ? personal : performer.Faction == Faction.Companion) && (performer.Position - Champion.Position).sqrMagnitude <= 16)
                    { identityCharge = Mathf.Min(8, identityCharge + 1); identitySecondAt = Elapsed + .25f; }
                    if (IdentityId == "VelvetRegent" && e.Faction == Faction.Companion) identityCharge = Mathf.Min(6, identityCharge + 1);
                    if (IdentityId == "PatchworkHost" && performer == identityLead) identityCharge = Mathf.Min(4, identityCharge + 1);
                    if (personal)
                    {
                        identityAttacks++;
                        if (IdentityId == "Postmaster" || IdentityId == "LoneJelly" || IdentityId == "PetalCitadel") identityCharge++;
                        if (IdentityId == "DrowsySeer") identityCharge = Mathf.Min(4, identityCharge + 1);
                        if (IdentityId == "MirrorKeeper" && identityReflections.Count > 0) ReleaseIdentityReflections();
                        if (IdentityId == "ChinaMarionette" && identityCharge >= 1 && Elapsed >= identityAt)
                        {
                            var target = IdentityTarget(Champion.Position); if (target != null)
                            { identityCharge--; identityAt = Elapsed + 1; IdentityPattern(target, IdentityPower * 1.1f); if (IdentityApproach == 2) Heal(1, IdentityCause()); IdentityCast(); }
                        }
                        if (IdentityId == "ButtonCavalier" && identityCharge >= 3 && Elapsed >= identityAt)
                        { var target = IdentityTarget(Champion.Position,5); if (target != null) { IdentityPattern(target,IdentityPower * 2.6f); identityCharge = 0; identityAt = Elapsed + 2; IdentityCast(); } }
                        if (IdentityId == "ThornTreasurer" && identityAttacks % 4 == 0) identityCharge = Mathf.Min(4, identityCharge + 1);
                        if (IdentityId == "IvorySovereign" && identityAttacks % 3 == 0) identityCharge = 3;
                        if (IdentityId == "TinVirtuoso") identityCharge = Mathf.Min(HeroIdentityRequired, identityCharge + (e.Critical ? 2 : 1));
                        if (IdentityId == "LanternWeaver" && identityAttacks % 4 == 0) identityCounter = 1;
                        if (IdentityId == "RainArchivist" && identityAttacks % 3 == 0) IdentityAddPage(0);
                    }
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && personal && basic && e.Subject != null)
                {
                    if (IdentityId == "Postmaster" || IdentityId == "LanternWeaver") IdentityMarkTarget(e.Subject);
                    if (IdentityId == "TreasureAcorn" && e.Subject == identityBounty && Elapsed >= identitySecondAt) { identityCharge = Mathf.Min(3, identityCharge + 1); identitySecondAt = Elapsed + .2f; }
                    if (IdentityId == "ArchiveCurator" && !identitySpecimens.Exists(d => d.Id == e.Subject.Definition.Id))
                    {
                        if (identitySpecimens.Count < 3) identitySpecimens.Add(e.Subject.Definition);
                        else if (IdentityApproach != 1) { identitySpecimens[identityCounter++ % 3] = e.Subject.Definition; }
                        identityCharge = identitySpecimens.Count;
                    }
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && IdentityId == "PatchworkHost" && identityLead != null && e.Cause.SourceUnitId == identityLead.Id && (identityLead.Definition.Weapon.Kind == WeaponKind.Contact || identityLead.Definition.ReferenceCompanion == "Big Cabbage" || identityLead.Definition.ReferenceCompanion == "Little Cabbage" || identityLead.Definition.ReferenceCompanion == "Domestic Spiked Ball") && Elapsed >= identitySecondAt)
                { identityCharge = Mathf.Min(4, identityCharge + 1); identitySecondAt = Elapsed + .4f; }
                if (e.Kind == CombatEventKind.Kill && e.Faction == Faction.Enemy)
                {
                    if (IdentityId == "NightMaw" && !(e.Subject?.Variant?.Summoned ?? false)) identityCharge = Mathf.Min(3, identityCharge + 1);
                    if (IdentityId == "TreasureAcorn" && e.Subject == identityBounty) identityCharge = 3;
                    if (IdentityId == "MoonProwler" && personal && identityRearSpent && e.Subject == identityBounty && identityPayouts < 6) { Gold = RunSession.BoundedCounter((long)Gold + 1); identityPayouts++; identityRearSpent = false; HeroIdentityGold++; }
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Champion && e.Cause.ItemId != "hunger" && e.Cause.ItemId != "overtime-cost" && e.Cause.ItemId != "curio-cost")
                {
                    if (IdentityId == "CopperCauldron") identityCharge = Mathf.Min(6, identityCharge + 2);
                    if (IdentityId == "ThornTreasurer" && identityCharge >= 1)
                    { var target = IdentityTarget(Champion.Position,6); if (target != null) { identityCharge--; IdentityPattern(target, IdentityPower * 1.3f); IdentityCast(); } }
                    if (IdentityId == "IvorySovereign") identityCharge = 3;
                    // a porcelain crack becomes a shard; a cavalier answers a blow with a bow
                    if (IdentityId == "ChinaMarionette") identityCharge = Mathf.Min(3, identityCharge + 1);
                    if (IdentityId == "ButtonCavalier") identityCharge = Mathf.Min(3, identityCharge + 1);
                }
                if (e.Kind == CombatEventKind.Dodge)
                {
                    if (IdentityId == "ButtonCavalier" || IdentityId == "StormFencer") identityCharge = Mathf.Min(3,identityCharge + 2);
                    if (IdentityId == "RainArchivist") IdentityAddPage(1);
                }
                if (e.Kind == CombatEventKind.Pickup && e.Loot == LootKind.Coin)
                {
                    if (IdentityId == "GlassDice") identityCharge = Mathf.Min(6,identityCharge + e.Value);
                    if (IdentityId == "TideSinger") identityCharge = Mathf.Min(3,identityCharge + 1);
                    if (IdentityId == "PennyBroker") identityCharge = Mathf.Min(10,identityCharge + e.Value);
                }
                if (e.Kind == CombatEventKind.ChestOpened && IdentityId == "RainArchivist") IdentityAddPage(3);
            }
        }
        private void ProcessHeroIdentity(float delta, Vector2 movement)
        {
            if (!HeroIdentityEnabled || !Champion.Alive || Phase != RunPhase.Combat) return;
            identityGlyphs.RemoveAll(g => g.Until <= Elapsed);
            identityMarks.RemoveAll(m => !m.Target.Alive || m.Until <= Elapsed);
            ResolveIdentityStrikes();
            if (!Champion.Alive || Phase != RunPhase.Combat) return;
            ObserveHeroIdentityEvents();
            float moved = Mathf.Min(2, Vector2.Distance(Champion.Position,Champion.PreviousPosition));
            bool walking = moved > .001f;
            identityStill = walking ? 0 : identityStill + delta;
            if (walking) identityHeading = (Champion.Position - Champion.PreviousPosition).normalized;
            if (Elapsed >= identityStepAt)
            {
                identityStepAt = Elapsed + .35f;
                if (identityTrail.Count >= 4) identityTrail.RemoveAt(0); identityTrail.Add(Champion.Position);
                foreach (var m in identityMarks) IdentityGlyph(MorphGeometry.Seal, m.Target.Position, m.Target.Position, .23f,.4f, role: HeroGlyphRole.Mark);
                if (identityBounty != null && identityBounty.Alive && IdentityId == "TreasureAcorn") IdentityGlyph(MorphGeometry.Seal,identityBounty.Position,identityBounty.Position,.32f,.4f,role:HeroGlyphRole.Mark);
            }
            UnitState target = IdentityTarget(Champion.Position, 8, IdentityId == "VelvetRegent" || IdentityId == "TreasureAcorn" ? IdentityApproach : 0);
            switch (IdentityId)
            {
                case "LanternKnight":
                    if (identityCharge >= 8 && target != null && Elapsed >= identityAt) { IdentityPerform(IdentityPower * 1.8f, IdentityApproach == 1 ? 4 : 3); if(IdentityApproach==2){IdentityHit(MorphGeometry.Ring,Champion.Position,Champion.Position,1.7f,IdentityPower*.35f);CatchIdentityBullet(Champion.Position,2);}else IdentityPattern(target,IdentityPower * .6f); identityCharge = 0; identityAt = Elapsed + 4; IdentityCast(); }
                    break;
                case "Postmaster":
                    identityCharge = Mathf.Min(3,identityCharge);
                    if (identityCharge >= 3 && target != null && Elapsed >= identityAt)
                    {
                        if (identityMarks.Count == 0) IdentityMarkTarget(target);
                        int n = identityMarks.Count;
                        Vector2 from = Champion.Position;
                        foreach (var m in identityMarks)
                        {
                            if (IdentityApproach == 2) { IdentityHit(MorphGeometry.Line,from,m.Target.Position,0,IdentityPower * 1.6f/n,.25f); from = m.Target.Position; }
                            else IdentityShot(from,m.Target.Position - from,IdentityPower * 1.6f/n,element:IdentityApproach == 1 ? DamageElement.Ember : DamageElement.Physical,homingTarget:m.Target.Id);
                        }
                        identityMarks.Clear(); identityCharge = 0; identityAt = Elapsed + 2.5f; IdentityCast();
                    }
                    break;
                case "DrowsySeer":
                    // The dream that waits: a full sleep gauge leaves a dream wherever you pause (0.5s), or by itself after 3s.
                    if (walking) identityCharge = Mathf.Min(4,identityCharge + delta * .4f);
                    if (identityCharge >= 4 && identityFullAt < 0) identityFullAt = Elapsed;
                    if (identityCharge >= 4 && identityGhostUntil <= Elapsed && target != null && (identityStill >= .5f || Elapsed - identityFullAt >= 3))
                    { identityGhost = Champion.Position; identityGhostUntil = Elapsed + 4; identityGhostAt = Elapsed; identityCharge = 0; identityFullAt = -1; IdentityCast(); }
                    if (identityGhostUntil > Elapsed)
                    {
                        if (Elapsed >= identityGhostAt)
                        {
                            var dreamTarget = IdentityTarget(identityGhost,6);
                            if (dreamTarget != null) { IdentityPattern(dreamTarget,IdentityPower * .8f,origin:identityGhost,element:IdentityApproach == 2 ? DamageElement.Frost : DamageElement.Arcane); identityGhostAt = Elapsed + .7f; }
                        }
                        if (Elapsed >= identityTickAt) { identityTickAt = Elapsed + .3f; var g = IdentityGlyph(MorphGeometry.Ring,identityGhost,identityGhost,.4f,.4f,role:HeroGlyphRole.Ambient); g.Portrait = Champion.Definition.Body; }
                    }
                    break;
                case "RootOracle": ProcessIdentityRoots(target); break;
                case "LoneJelly":
                    identityCharge = Mathf.Min(3,identityCharge);
                    if (identityCharge >= 3 && target != null && Elapsed >= identityAt)
                    {
                        UnitDefinition organ = catalog.HeroOrgans.Length > 0 ? catalog.HeroOrgans[identityCycle++ % catalog.HeroOrgans.Length] : Champion.Definition;
                        if (IdentityApproach == 1 || organ.Weapon.Kind == WeaponKind.Projectile) IdentityShot(Champion.Position,target.Position-Champion.Position,IdentityPower * 1.5f,3,element:organ.Weapon.Element);
                        else IdentityPattern(target,IdentityPower * 1.5f,IdentityApproach == 2 ? 0 : 1,element:organ.Weapon.Element);
                        identityCharge = 0; identityAt = Elapsed + 2.4f; IdentityCast();
                    }
                    break;
                case "GlassDice":
                    if (identityCharge >= 6 && target != null && Elapsed >= identityAt)
                    {
                        switch(identityFace)
                        {
                            case 1: IdentityShot(Champion.Position,target.Position-Champion.Position,IdentityPower * 2f,3); break;
                            case 2: IdentityPattern(target,IdentityPower * 2.4f,2); break;
                            case 3: IdentityHit(MorphGeometry.Disc,target.Position,target.Position,1.5f,IdentityPower * 2f,.25f,element:DamageElement.Frost,slow:1); break;
                            case 4: Gold = RunSession.BoundedCounter((long)Gold+1); HeroIdentityGold++; IdentityPattern(target,IdentityPower * 1.2f); break;
                            case 5: Heal(2,IdentityCause()); IdentityPattern(target,IdentityPower * 1.2f); break;
                            case 6: IdentityPattern(target,IdentityPower * 3.2f); IdentityGlyph(MorphGeometry.Seal,target.Position,target.Position,.6f); break;
                        }
                        identityFace = identityFace % 6 + 1; identityCharge = 0; identityAt = Elapsed + 4; IdentityCast();
                    }
                    break;
                case "TideSinger":
                    if (identityCharge >= 3 && target != null && Elapsed >= identityAt)
                    {
                        int notes = 2 + Mathf.Min(3,catalog.HeroDrinks/2);
                        if (IdentityApproach == 1) { Heal(3,IdentityCause()); IdentityHit(MorphGeometry.Ring,Champion.Position,Champion.Position,1.6f,IdentityPower * 1.6f,slow:.6f,push:.25f); }
                        else if (IdentityApproach == 0) IdentityShot(Champion.Position,target.Position-Champion.Position,IdentityPower * 2.2f,notes,12,DamageElement.Frost);
                        else IdentityHit(MorphGeometry.Crescent,Champion.Position,target.Position,2.2f,IdentityPower * 2.2f,slow:.5f,push:.4f);
                        identityCharge = 0; identityAt = Elapsed + 4; IdentityCast();
                    }
                    break;
                case "CometRunner": case "CitrusMarshal":
                    identityCharge = Mathf.Min(HeroIdentityRequired,identityCharge + moved);
                    if (identityCharge >= HeroIdentityRequired && target != null && Elapsed >= identityAt)
                    {
                        int segments = Mathf.Max(1,identityTrail.Count-1);
                        float total = IdentityPower * (IdentityId == "CometRunner" ? (IdentityApproach == 2 ? 1.6f : 2.3f) : IdentityApproach == 2 ? 2.2f : 2.7f);
                        for (int i=0;i<segments;i++)
                        {
                            Vector2 a=identityTrail[i], b=identityTrail[Mathf.Min(identityTrail.Count-1,i+1)];
                            if (IdentityApproach == 2) IdentityHit(MorphGeometry.Ring,b,b,1.1f,total/segments,.12f+i*.1f);
                            else if(IdentityApproach==1){for(int sign=-1;sign<=1;sign+=2)IdentityHit(MorphGeometry.Line,a+Vector2.right*.4f*sign,b+Vector2.right*.4f*sign,0,total*.5f/segments,.12f+i*.1f,element:IdentityId=="CometRunner"?DamageElement.Ember:DamageElement.Physical,slow:IdentityId=="CitrusMarshal"?.5f:0);}
                            else IdentityHit(MorphGeometry.Line,a,b,0,total/segments,.12f+i*.1f,element:IdentityId=="CometRunner"?DamageElement.Ember:DamageElement.Physical,slow:IdentityId=="CitrusMarshal"?.5f:0);
                        }
                        identityCharge=0; identityAt=Elapsed+3; IdentityCast();
                    }
                    break;
                case "MirrorKeeper":
                    // The mirror catches a hostile shot that comes close (1 unit; 2 once you have stood still 0.5s), one per 2 seconds.
                    if (identityReflections.Count<3 && Elapsed>=identitySecondAt)
                    {
                        float reach = identityStill >= .5f ? 2f : 1f;
                        for(int i=projectiles.Count-1;i>=0;i--)
                        {
                            var p=projectiles[i]; if(p.Faction!=Faction.Enemy || (p.Position-Champion.Position).sqrMagnitude>reach*reach)continue;
                            identityReflections.Add(new IdentityReflection { Direction=-p.Velocity.normalized,Element=p.Element }); HeroIdentityPrevented+=p.Damage; IdentityGlyph(MorphGeometry.Ring,p.Position,Champion.Position,.35f,.35f,role:HeroGlyphRole.Catch); projectiles.RemoveAt(i); identitySecondAt=Elapsed+2; identityBankAt=Elapsed; break;
                        }
                    }
                    identityCharge=identityReflections.Count;
                    if(identityReflections.Count>0 && Elapsed-identityBankAt>=2 && target!=null)ReleaseIdentityReflections();
                    break;
                case "TreasureAcorn":
                    if((identityBounty==null || !identityBounty.Alive) && identityCharge<3)identityBounty=target;
                    if(identityCharge>=3 && target!=null && Elapsed>=identityAt)
                    { if(identityPayouts<8){Gold=RunSession.BoundedCounter((long)Gold+1);identityPayouts++;HeroIdentityGold++;} IdentityPattern(target,IdentityPower*.85f);identityCharge=0;identityBounty=null;identityAt=Elapsed+3;IdentityCast(); }
                    break;
                case "VerdantMatron":
                    identityCharge=Mathf.Min(6,Mathf.Max(0,Elapsed-identityAt));
                    if(identityCharge>=6 && target!=null)
                    {
                        UnitState sprout=units.Find(u=>u.Faction==Faction.Companion && (u.Definition.Id=="GrandSprout" || u.Definition.Id=="LittleSprout"));
                        Vector2 from=sprout==null?Champion.Position:sprout.Position;
                        if(IdentityApproach==0)CatchIdentityBullet(from,2);
                        if(IdentityApproach==2)Heal(sprout==null?2:3,IdentityCause());
                        IdentityHit(MorphGeometry.Disc,from,from,1.6f,IdentityPower*(IdentityApproach==1?3f:IdentityApproach==2?2.2f:1.4f),slow:.6f);
                        identityAt=Elapsed;identityCharge=0;IdentityCast();
                    }
                    break;
                case "CopperCauldron":
                    if(IdentityTarget(Champion.Position,3)!=null)identityCharge=Mathf.Min(6,identityCharge+delta);
                    if(identityCharge>=6 && target!=null && Elapsed>=identityAt)
                    { if(IdentityApproach==0)CatchIdentityBullet(Champion.Position,3); IdentityPattern(target,IdentityPower*(IdentityApproach==0?1.8f:2.5f),IdentityApproach==1?1:0,element:DamageElement.Ember);identityCharge=0;identityAt=Elapsed+3;IdentityCast(); }
                    break;
                case "ChinaMarionette":
                    while(identityRecovered>=6){identityRecovered-=6;identityCharge=Mathf.Min(3,identityCharge+1);}break;
                case "SoapSkater":
                    if(walking && movement.sqrMagnitude>.01f && Elapsed>=identitySecondAt)
                    {
                        Vector2 before=identityGhost; Vector2 heading=movement.normalized;
                        if(before.sqrMagnitude>.1f && Vector2.Dot(before,heading)<.574f)
                        {
                            identityCharge++;identitySecondAt=Elapsed+1.2f;
                            float power=IdentityPower*(identityCharge>=3?1.8f:.7f);float size=identityCharge>=3?1.4f:.9f;
                            if(IdentityApproach==0)IdentityHit(MorphGeometry.Line,Champion.PreviousPosition-heading,Champion.Position+heading,0,power,.65f,slow:.7f,push:.2f);
                            else IdentityHit(IdentityApproach==1?MorphGeometry.Crescent:MorphGeometry.Disc,Champion.Position,Champion.Position+heading,size,power,.65f,slow:.7f,push:.25f);
                            if(identityCharge>=3)identityCharge=0;IdentityCast();
                        }
                        identityGhost=heading;
                    }
                    break;
                case "VelvetRegent": case "TinVirtuoso":
                    if(identityCharge>=HeroIdentityRequired && target!=null && Elapsed>=identityAt)
                    { IdentityPerform(IdentityPower*1.3f,IdentityId=="VelvetRegent"?3:IdentityApproach==0?1:IdentityApproach==1?2:4);identityCharge=0;identityAt=Elapsed+ (IdentityId=="VelvetRegent"?4:1.5f);IdentityCast(); }
                    break;
                case "ButtonCavalier": IdentityNearMiss(); break;
                case "NightMaw":
                    if(identityCharge>=3 && target!=null && Vector2.Distance(target.Position,Champion.Position)<=3.5f && Elapsed>=identityAt)
                    { IdentityPattern(target,IdentityPower*1.2f,IdentityApproach==0?0:1,origin:IdentityApproach==2?Champion.Position-Champion.Facing:Champion.Position);identityCharge=0;identityAt=Elapsed+2.5f;IdentityCast(); }
                    break;
                case "ThornTreasurer":
                    // a full crown overflows: with 4 thorns and an enemy within 2.5 units, one thorn answers by itself (3s)
                    if(identityCharge>=4 && Elapsed>=identityAt)
                    { var near=IdentityTarget(Champion.Position,2.5f); if(near!=null){identityCharge--;identityAt=Elapsed+3;IdentityPattern(near,IdentityPower*1.3f);IdentityCast();} }
                    break;
                case "IvorySovereign":
                    if(identityCharge>=3 && target!=null && Elapsed>=identityAt)
                    { if(IdentityApproach==1)IdentityHit(MorphGeometry.Seal,target.Position,target.Position,1.5f,IdentityPower);else IdentityPattern(target,IdentityPower,IdentityApproach==0?0:2);identityCharge=0;identityAt=Elapsed+1;IdentityCast(); }
                    break;
                case "PetalCitadel":
                    identityCharge=Mathf.Min(4,identityCharge);
                    if(identityCharge>=4 && target!=null && Elapsed>=identityAt){ReleaseIdentityPetals(target);identityCharge=0;identityAt=Elapsed+3;IdentityCast();}break;
                case "PatchworkHost":
                    if(identityLead==null || !identityLead.Alive)identityLead=units.Find(u=>u.Faction==Faction.Companion && u.Alive);
                    if(identityCharge>=4 && target!=null && identityLead!=null && Elapsed>=identityAt)
                    {
                        float budget=Mathf.Min(IdentityPower*2.2f,identityLead.Definition.Weapon.Damage*1.1f);
                        if(IdentityApproach==1){IdentityEcho(identityLead,target,budget*.5f);IdentityEcho(identityLead,target,budget*.5f,true);}
                        else if(IdentityApproach==2)IdentityHit(MorphGeometry.Crescent,Champion.Position,target.Position,2.2f,budget);
                        else IdentityEcho(identityLead,target,budget,true);
                        if(identityLead.Definition.Passive==PassiveKind.HealChampion)Heal(2,IdentityCause());
                        IdentityGlyph(MorphGeometry.Line,identityLead.Position,Champion.Position,0,.6f,role:HeroGlyphRole.Link);identityCharge=0;identityAt=Elapsed+3;IdentityCast();
                    }
                    break;
                case "ArchiveCurator":
                    if(identitySpecimens.Count>0 && target!=null && Elapsed>=identityAt)
                    {
                        float budget=IdentityPower*3f/identitySpecimens.Count;
                        foreach(var specimen in identitySpecimens)
                        {
                            if(specimen.Weapon.Kind==WeaponKind.Projectile)IdentityShot(Champion.Position,target.Position-Champion.Position,budget,3,element:specimen.Weapon.Element);
                            else IdentityPattern(target,budget,specimen.Behavior==EnemyBehavior.Charger || specimen.Behavior==EnemyBehavior.Door?0:1);
                        }
                        if(IdentityApproach==2){identitySpecimens.Clear();identityCharge=0;}
                        identityAt=Elapsed+6;IdentityCast();
                    }
                    break;
                case "PennyBroker":
                    if(identityCharge>=10 && target!=null && Elapsed>=identityAt){IdentityHit(MorphGeometry.Seal,target.Position,target.Position,.8f,IdentityPower*1.45f);identityCharge=0;identityAt=Elapsed+3;IdentityCast();}break;
                case "StormFencer":
                    IdentityNearMiss();if(target!=null && Vector2.Distance(target.Position,Champion.Position)<6)identityCharge=Mathf.Min(3,identityCharge+delta*.4f);
                    if(identityCharge>=3 && target!=null && Elapsed>=identityAt){IdentityPattern(target,IdentityPower*2.8f,element:DamageElement.Arcane);identityCharge=0;identityAt=Elapsed+3;IdentityCast();}break;
                case "MoonProwler":
                    // ready: the shadow waits behind the nearest enemy; the next basic attack (IdentityAttackDamage) releases it
                    identityCharge=Mathf.Min(5,identityCharge+moved);
                    if(identityCharge>=5 && target!=null){identityBounty=target;Vector2 toHero=Champion.Position-target.Position;if(toHero.sqrMagnitude>.0001f)identityGhost=toHero.normalized;identityRearSpent=false;}
                    if(identityCharge>=5 && identityBounty!=null && Elapsed>=identityTickAt){IdentityGlyph(MorphGeometry.Line,identityBounty.Position,identityBounty.Position-identityGhost*1.1f,0,.4f,role:HeroGlyphRole.Mark);identityTickAt=Elapsed+.35f;}
                    break;
                case "LanternWeaver":
                    identityCharge=identityMarks.Count;
                    if(identityCounter>0 && identityMarks.Count>0 && Elapsed>=identityAt)
                    {
                        Vector2 previous=IdentityApproach==1?Champion.Position:identityMarks[identityMarks.Count-1].Target.Position;
                        float total=IdentityPower*(IdentityApproach==2?3.6f:IdentityApproach==1?3.4f:3.8f);
                        foreach(var m in identityMarks){if(IdentityApproach==2)IdentityHit(MorphGeometry.Crescent,m.Target.Position,Champion.Position,1.4f,total/identityMarks.Count,.35f,slow:.3f);else IdentityHit(MorphGeometry.Line,previous,m.Target.Position,0,total/identityMarks.Count,.25f,slow:.3f,push:IdentityApproach==1?-.15f:0);previous=m.Target.Position;}
                        identityMarks.Clear();identityCounter=0;identityAt=Elapsed+2.4f;IdentityCast();
                    }
                    break;
                case "HourglassStag":
                    if(identityBank>0 && identityBankAt>=0 && Elapsed-identityBankAt>=(IdentityApproach==0?4:IdentityApproach==1?2.5f:5) && target!=null)
                    {
                        // the debt is collected from the enemy it was borrowed against, with interest
                        UnitState debtor=identityDebtor!=null && identityDebtor.Alive && CanHit(Faction.Champion,identityDebtor)?identityDebtor:target;
                        if(IdentityApproach==1)IdentityHit(MorphGeometry.Disc,debtor.Position,debtor.Position,1.4f,identityBank*2.6f,.2f);
                        else if(IdentityApproach==2)IdentityShot(Champion.Position,debtor.Position-Champion.Position,identityBank*3.2f,3,element:DamageElement.Frost,homingTarget:debtor.Id);
                        else IdentityHit(MorphGeometry.Disc,debtor.Position,debtor.Position,.6f,identityBank*3.2f,.2f);
                        identityBank=identityCharge=0;identityBankAt=-1;identityDebtor=null;IdentityCast();
                    }
                    break;
                case "RainArchivist":
                    if(identityRecovered>=4){identityRecovered=0;IdentityAddPage(2);}
                    if(identityPages.Count>=3 && target!=null && Elapsed>=identityAt)
                    {
                        int healing=0,chests=0;foreach(int p in identityPages){if(p==2)healing++;if(p==3)chests++;}
                        int shots=Mathf.Min(6,3+chests+(IdentityApproach==1?1:0));
                        IdentityShot(Champion.Position,target.Position-Champion.Position,IdentityPower*4.5f,shots,IdentityApproach==2?24:14,DamageElement.Frost);
                        if(healing>0 || IdentityApproach==2)Heal(Mathf.Min(4,healing+1),IdentityCause());
                        identityPages.Clear();identityCharge=0;identityAt=Elapsed+3;IdentityCast();
                    }
                    break;
            }
            identityCharge=Mathf.Clamp(identityCharge,0,HeroIdentityRequired);
        }
        private void ProcessIdentityRoots(UnitState target)
        {
            identityCharge=Mathf.Min(3,Mathf.Max(0,Elapsed-identityAt));
            if(Elapsed>=identityTickAt)
            {
                identityTickAt=Elapsed+.4f;
                for(int i=0;i<3;i++){Vector2 p=Champion.Position+(Vector2)(Quaternion.Euler(0,0,90+i*120)*Vector2.right)*1.8f;IdentityGlyph(MorphGeometry.Line,Champion.Position,p,0,.45f,role:HeroGlyphRole.Ambient);IdentityGlyph(MorphGeometry.Seal,p,p,.2f,.45f,role:HeroGlyphRole.Ambient);}
            }
            if(identityCharge<3 || target==null)return;
            Vector2 root=Champion.Position+(Vector2)(Quaternion.Euler(0,0,90+(identityCycle++%3)*120)*Vector2.right)*1.8f;
            var threat=IdentityTarget(root,4);if(threat==null)return;
            if(IdentityApproach==0)CatchIdentityBullet(root,2);
            if(IdentityApproach==2)Heal(2,IdentityCause());
            IdentityHit(IdentityApproach==1?MorphGeometry.Line:MorphGeometry.Disc,root,threat.Position,1.3f,IdentityPower*(IdentityApproach==1?1.5f:IdentityApproach==0?.8f:1.3f),slow:.7f);
            identityAt=Elapsed;identityCharge=0;IdentityCast();
        }
        private void CatchIdentityBullet(Vector2 position,float radius)
        {
            for(int i=projectiles.Count-1;i>=0;i--)if(projectiles[i].Faction==Faction.Enemy && (projectiles[i].Position-position).sqrMagnitude<=radius*radius){HeroIdentityPrevented+=projectiles[i].Damage;projectiles.RemoveAt(i);IdentityGlyph(MorphGeometry.Ring,position,position,.6f,.3f,role:HeroGlyphRole.Catch);return;}
        }
        private void ReleaseIdentityReflections()
        {
            var target=IdentityTarget(Champion.Position);if(target==null || identityReflections.Count==0 || Elapsed<identityAt)return;
            int count=identityReflections.Count;
            for(int i=0;i<count;i++)
            {
                Vector2 dir=IdentityApproach==1?identityReflections[i].Direction:IdentityApproach==2?(Vector2)(Quaternion.Euler(0,0,i*360f/count)*Vector2.right):(Vector2)(Quaternion.Euler(0,0,(i-(count-1)*.5f)*15)*(target.Position-Champion.Position).normalized);
                // each borrowed shot is returned with 60% W of its own
                IdentityShot(Champion.Position,dir,IdentityPower*.6f,element:identityReflections[i].Element);
            }
            identityReflections.Clear();identityCharge=0;identityAt=Elapsed+1;identityBankAt=-1;IdentityCast();
        }
        private void ReleaseIdentityPetals(UnitState target)
        {
            int grown=0,total=0;for(int i=0;i<4;i++){if(catalog.HeroPetals[i]>0)grown++;total+=catalog.HeroPetals[i];}
            float budget=IdentityPower*(1.5f+Mathf.Min(.9f,total*.08f));
            Vector2 dir=(target.Position-Champion.Position).normalized,side=new Vector2(-dir.y,dir.x);
            for(int i=0;i<4;i++)
            {
                Vector2 at=target.Position+(i%2==0?side:dir)*(i<2?-.8f:.8f);
                IdentityHit(IdentityApproach==1?MorphGeometry.Line:MorphGeometry.Disc,IdentityApproach==1?Champion.Position:at,at,.65f+Mathf.Min(3,catalog.HeroPetals[i])*.1f,budget/4,.18f+i*.12f,element:i==3?DamageElement.Frost:DamageElement.Physical);
            }
            if(grown==4)IdentityHit(MorphGeometry.Ring,target.Position,target.Position,1.5f,IdentityPower*.5f,.8f);
            if(catalog.HeroPetals[2]>0)Heal(Mathf.Min(3,catalog.HeroPetals[2]),IdentityCause());
            if(IdentityApproach==2 && catalog.HeroPetals[1]>0)CatchIdentityBullet(Champion.Position,2.5f);
        }
    }
}
