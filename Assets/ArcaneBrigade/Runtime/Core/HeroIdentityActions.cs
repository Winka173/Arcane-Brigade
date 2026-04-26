using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class CombatSimulation
    {
        // Hero kits (18/10/2026): each hero has ONE mechanic. Its own actions fill a charge (identityCharge); every full charge
        // (HeroIdentityRequired) climbs the gauge a tier, and at the third tier the hero transforms by itself (HeroAwakening).
        // There are no small casts any more. Around that, the kit's passives act all the time: the hero's own (stamps, the lid,
        // the mirror, roots, thorns, debt...) and the team's (HeroKitCombat). Budgets are in W (the hero's weapon damage).
        private void IdentityRecordHealing(float recovered, CombatCause cause)
        {
            if (HeroIdentityEnabled && !IdentityOwn(cause) && (IdentityId == "ChinaMarionette" || IdentityId == "RainArchivist")) identityRecovered += Mathf.Max(0, recovered);
        }
        /// <summary>Owner (22/10/2026): the transformation gauge fills 20% slower.</summary>
        public const float AwakenChargeRate = .8f;
        private void Charge(float amount) { if (amount > 0) identityCharge += amount * (HeroAwakenEnabled ? AwakenChargeRate : 1) * (NightAlone ? 1.3f : 1); }
        private string gaugeLastPerformer; private float kitBountyCoinAt;

        private void ObserveHeroIdentityEvents()
        {
            HeroKit k = Kit;
            int count = events.Count;
            for (int i = 0; i < count; i++)
            {
                CombatEvent e = events[i];
                if (IdentityOwn(e.Cause)) { ObserveOwnEvent(e); continue; }
                bool personal = e.Cause.SourceUnitId == Champion.Id;
                bool basic = e.Cause.Depth == 0;
                if (e.Kind == CombatEventKind.Fire && e.Faction != Faction.Enemy && basic)
                {
                    UnitState performer = e.Subject ?? UnitWithId(e.Cause.SourceUnitId);
                    if (performer != null && performer.Faction == Faction.Companion)
                    {
                        // friends' attacks (the gauge recipe decides what they are worth to this hero)
                        Fill(GaugeSource.FriendHit);
                        if ((performer.Position - Champion.Position).sqrMagnitude <= 16) Fill(GaugeSource.FriendNear);
                        if (performer.Definition.Id != gaugeLastPerformer) { gaugeLastPerformer = performer.Definition.Id; Fill(GaugeSource.FriendDistinct); }
                        if (catalog.HeroBondLevels > 0) Fill(GaugeSource.FriendBond);
                    }
                    if (IdentityId == "PatchworkHost" && performer != null && IsLead(performer) && k != null && k.Has(KitPassive.HealOnLeadAct) && Elapsed >= kitLeadHealAt)
                    { kitLeadHealAt = Elapsed + k.Value(KitPassive.HealOnLeadAct); Heal(1, IdentityCause()); }
                    if (personal)
                    {
                        identityAttacks++;
                        if (IdentityId == "MirrorKeeper" && identityReflections.Count > 0) ReleaseIdentityReflections();
                    }
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Subject != null)
                {
                    UnitState target = e.Subject;
                    if (personal && basic)
                    {
                        ObservePersonalHit(target, e);
                        Fill(GaugeSource.Hit);
                        if (e.Critical) Fill(GaugeSource.Crit);
                    }
                    // Kite Admiral: any of the hero's own hits beyond 4 units (the kite's string and dive too), never the transformation's
                    if (personal && (target.Position - Champion.Position).sqrMagnitude > 16) Fill(GaugeSource.FarHit);
                    else if (e.Critical && UnitWithId(e.Cause.SourceUnitId)?.Faction == Faction.Companion) Fill(GaugeSource.FriendCrit);
                    // Sundae Duchess: a crit (ours or a friend's) cracks a frozen enemy for 5% of its maximum health
                    if (e.Critical && k != null && k.Has(KitPassive.FrozenCritShatter) && target.Alive && !target.Definition.IsBoss && target.FrozenUntil > Elapsed)
                        ApplyDamage(target, target.Definition.Health * k.Value(KitPassive.FrozenCritShatter), Faction.Champion, "Shatter", e.Direction, cause: IdentityCause().Child("frozen-shatter"));
                    if (!personal && k != null && k.Has(KitPassive.SlowToFreeze) && target.Alive && (target.ChillUntil > Elapsed || target.KitSlowUntil > Elapsed) && target.FreezeReadyAt <= Elapsed
                        && UnitWithId(e.Cause.SourceUnitId)?.Faction == Faction.Companion)
                    { target.FreezeReadyAt = Elapsed + k.Value(KitPassive.SlowToFreeze); Freeze(target, .3f); }
                    if (!personal && k != null && Elapsed >= identityTickAt && k.Has(KitPassive.TeamChainChance) && UnitWithId(e.Cause.SourceUnitId)?.Faction == Faction.Companion
                        && combatRandom.NextDouble() < k.Value(KitPassive.TeamChainChance)) KitChain(target, IdentityPower * .2f);
                }
                if (e.Kind == CombatEventKind.Kill && e.Faction == Faction.Enemy) ObserveKill(e, personal);
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Champion && e.Cause.ItemId != "hunger" && e.Cause.ItemId != "overtime-cost" && e.Cause.ItemId != "curio-cost") ObserveInjury(e);
                if (e.Kind == CombatEventKind.Dodge)
                {
                    Fill(GaugeSource.NearMiss);
                    kitDodgeUntil = Elapsed + 2;
                }
                if (e.Kind == CombatEventKind.Pickup && e.Loot == LootKind.Coin)
                {
                    Fill(GaugeSource.Coin, Mathf.Max(1, e.Value));
                    if (HeroAwakening && k != null && k.Action == KitAction.BullMarket && Elapsed >= awakenReceiptAt) { awakenReceiptAt = Elapsed + .3f; /* round 6: at most one receipt every 0.3s (was 0.4s) */ var t = IdentityTarget(Champion.Position, 7); if (t != null) IdentityShot(Champion.Position, t.Position - Champion.Position, IdentityPower * .45f * ApproachScale, homingTarget: t.Id); }
                }
                if (e.Kind == CombatEventKind.ChestOpened) Fill(GaugeSource.Chest);
            }
        }
        /// <summary>Our own identity strikes still feed a few things (the thunder god marks what it shocks).</summary>
        private void ObserveOwnEvent(CombatEvent e)
        {
            if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Subject != null && IdentityId == "StormFencer") e.Subject.KitShockedUntil = Elapsed + 3;
            // the transformations that leave a state on what they strike: the giant scoops freeze, the bell's ram probes
            if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Subject != null && e.Subject.Alive && HeroAwakening && Kit != null)
            {
                if (Kit.Action == KitAction.SundaeRain) Freeze(e.Subject, .8f);
                if (Kit.Action == KitAction.DivingBell) e.Subject.ProbedUntil = Mathf.Max(e.Subject.ProbedUntil, Elapsed + 3);
            }
            if (e.Kind == CombatEventKind.Hit && e.Subject != null && e.Subject == identityBounty && HeroAwakening && Kit?.Action == KitAction.AcornGun && Elapsed >= kitBountyCoinAt)
            { kitBountyCoinAt = Elapsed + .5f; KitDropCoin(e.Subject.Position); }
            if (e.Kind == CombatEventKind.Kill && e.Faction == Faction.Enemy) ObserveKill(e, true);
        }
        private void ObservePersonalHit(UnitState target, CombatEvent e)
        {
            HeroKit k = Kit; if (k == null) return;
            if (IdentityId == "Postmaster") IdentityMarkTarget(target);
            if (IdentityId == "LanternWeaver")
            {
                if (Marked(target)) Fill(GaugeSource.Pinned); // hitting an enemy already pinned fills the frame faster
                IdentityMarkTarget(target);
            }
            if (IdentityId == "ArchiveCurator" && !identitySpecimens.Exists(d => d.Id == target.Definition.Id))
            {
                if (identitySpecimens.Count < 3) identitySpecimens.Add(target.Definition);
                else if (IdentityApproach != 1) identitySpecimens[identityCounter++ % 3] = target.Definition;
                if (kitSpecies.Add(target.Definition.Id))
                {
                    Fill(GaugeSource.NewSpecies); kitRecorded++;
                    int every = 5; if (k.Has(KitPassive.SpeciesGold) && kitRecorded % every == 0) { KitDropCoin(Champion.Position, Mathf.RoundToInt(k.Value(KitPassive.SpeciesGold))); }
                }
            }
            if (k.Has(KitPassive.EnemySlowOnHeroHit)) KitSlowEnemy(target, k.Value(KitPassive.EnemySlowOnHeroHit), 1);
            if (k.Has(KitPassive.TeamDamageVsWet)) target.KitWetUntil = Elapsed + 3;
            if (k.Has(KitPassive.ChainOnHit) && combatRandom.NextDouble() < k.Value(KitPassive.ChainOnHit)) KitChain(target, IdentityPower * .5f);
            if (Behind(Champion, target))
            {
                Fill(GaugeSource.Rear);
                if (k.Has(KitPassive.EnemySlowOnRear)) KitSlowEnemy(target, k.Value(KitPassive.EnemySlowOnRear), 1.5f);
                if (k.Has(KitPassive.CoinOnRearHit) && lootRandom.NextDouble() < k.Value(KitPassive.CoinOnRearHit)) KitDropCoin(target.Position);
            }
            if (k.Has(KitPassive.HeroHealPerHits) && ++kitHitsForHeal >= k.Value(KitPassive.HeroHealPerHits)) { kitHitsForHeal = 0; Heal(1, IdentityCause()); }
            // Bell Diver: probed enemies fill the depth gauge faster and may shake loose a coin
            if (target.ProbedUntil > Elapsed)
            {
                Fill(GaugeSource.Probed);
                if (k.Has(KitPassive.ProbedCoin) && kitProbedCoins < 5 && lootRandom.NextDouble() < k.Value(KitPassive.ProbedCoin)) { kitProbedCoins++; KitDropCoin(target.Position); }
            }
            // Juggling Jester: a crit tosses a ball that drops on a random enemy for half the blow
            if (e.Critical && k.Has(KitPassive.CritBall) && Elapsed >= kitBallAt)
            {
                UnitState pick = null; int seen = 0;
                foreach (var u in units) { if (!CanHit(Faction.Champion, u) || (u.Position - Champion.Position).sqrMagnitude > 49) continue; if (combatRandom.Next(++seen) == 0) pick = u; }
                if (pick != null)
                {
                    kitBallAt = Elapsed + .3f; const float flight = .6f;
                    IdentityHit(MorphGeometry.Disc, pick.Position, pick.Position, .45f, e.Value * k.Value(KitPassive.CritBall) * ApproachScale, flight, 1);
                    IdentityGlyph(MorphGeometry.Line, Champion.Position + Vector2.up * .4f, pick.Position, 0, flight, role: HeroGlyphRole.Lob);
                }
            }
        }
        private int kitProbedCoins, kitFrozenGold; private float kitBallAt;
        /// <summary>A chain of lightning from one enemy to the next nearest.</summary>
        private void KitChain(UnitState from, float budget)
        {
            UnitState next = null; float best = 9;
            foreach (var u in units) { if (u == from || !CanHit(Faction.Champion, u)) continue; float d = (u.Position - from.Position).sqrMagnitude; if (d < best) { best = d; next = u; } }
            if (next == null) return;
            identityTickAt = Elapsed + .15f;
            IdentityHit(MorphGeometry.Line, from.Position, next.Position, 0, budget, .05f, 1, DamageElement.Arcane);
            next.KitShockedUntil = from.KitShockedUntil = Elapsed + 3;
        }
        private void ObserveKill(CombatEvent e, bool personal)
        {
            HeroKit k = Kit; if (k == null) return;
            UnitState dead = e.Subject;
            kitKills++;
            if (!(dead?.Variant?.Summoned ?? false)) Fill(GaugeSource.Kill, personal ? 1 : .3f);
            if (dead == null) return;
            if (dead == identityBounty && IdentityId == "TreasureAcorn")
            {
                if (identityPayouts < k.Value(KitPassive.BountyGold)) { Gold = RunSession.BoundedCounter((long)Gold + 1); identityPayouts++; HeroIdentityGold++; }
                identityBounty = null;
            }
            if (HeroAwakening && k.Action == KitAction.AcornGun) KitDropCoin(dead.Position);
            if (Marked(dead) && k.Has(KitPassive.CoinOnMarkedKill) && lootRandom.NextDouble() < k.Value(KitPassive.CoinOnMarkedKill)) KitDropCoin(dead.Position);
            if (k.Has(KitPassive.KillCoinsNearHero) && (dead.Position - Champion.Position).sqrMagnitude <= 2.5f * 2.5f && lootRandom.NextDouble() < k.Value(KitPassive.KillCoinsNearHero)) KitDropCoin(dead.Position);
            if (dead == identityDebtor && k.Has(KitPassive.DebtKillGold)) KitDropCoin(dead.Position, Mathf.RoundToInt(k.Value(KitPassive.DebtKillGold)));
            if (dead.FrozenUntil > 0 && dead.FrozenUntil > Elapsed - .05f) // frozen when it fell (0 = never frozen)
            {
                Fill(GaugeSource.FrozenKill);
                if (k.Has(KitPassive.FrozenKillGold) && kitFrozenGold < k.Value(KitPassive.FrozenKillGold)) { kitFrozenGold++; KitDropCoin(dead.Position); }
            }
            if (k.Has(KitPassive.EliteEgg) && !dead.Definition.IsBoss && (dead.Definition.Radius >= .4f || dead.Variant != null && dead.Variant.ProcessionLeader)) DropLoot(LootKind.Egg, dead.Position, 1);
        }
        private void ObserveInjury(CombatEvent e)
        {
            HeroKit k = Kit; if (k == null) return;
            kitHurtUntil = Elapsed + (k.Has(KitPassive.TeamDamageOnHeroHurt) ? 3 : 2); kitInjuries++;
            Fill(GaugeSource.Hurt);
            if (k.Has(KitPassive.CrackDamage)) kitCracks = Mathf.Min(3, kitCracks + 1);
            if (k.Has(KitPassive.GoldOnHeroHurt) && kitInjuryGold < k.Value(KitPassive.GoldOnHeroHurt)) { kitInjuryGold++; Gold = RunSession.BoundedCounter((long)Gold + 1); HeroIdentityGold++; }
            UnitState attacker = UnitWithId(e.Cause.SourceUnitId);
            if (attacker != null && attacker.Faction == Faction.Enemy && attacker.Alive)
            {
                if (k.Has(KitPassive.EnemyCrackOnHit)) attacker.KitCrackedUntil = Elapsed + 4;
                bool throne = HeroAwakening && k.Action == KitAction.Throne;
                if (k.Has(KitPassive.Thorns) && (throne || Elapsed >= kitThornAt))
                {
                    kitThornAt = Elapsed + 1; IdentityHit(MorphGeometry.Line, Champion.Position, attacker.Position, 0, IdentityPower * (throne ? .9f : .7f) * ApproachScale, .05f, 1);
                    if (k.Has(KitPassive.ThornCoin) && lootRandom.NextDouble() < k.Value(KitPassive.ThornCoin)) KitDropCoin(attacker.Position);
                }
            }
            AwakenOnInjury();
        }

        private void ProcessHeroIdentity(float delta, Vector2 movement)
        {
            if (!HeroIdentityEnabled || !Champion.Alive || Phase != RunPhase.Combat) return;
            identityGlyphs.RemoveAll(g => g.Until <= Elapsed);
            liveEnemyCount = 0; foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Enemy) liveEnemyCount++;
            identityMarks.RemoveAll(m => !m.Target.Alive || m.Until <= Elapsed);
            ResolveIdentityStrikes();
            if (!Champion.Alive || Phase != RunPhase.Combat) return;
            ObserveHeroIdentityEvents();
            HeroKit k = Kit;
            float moved = Mathf.Min(2, Vector2.Distance(Champion.Position, Champion.PreviousPosition));
            bool walking = moved > .001f;
            identityStill = walking ? 0 : identityStill + delta;
            identityRun = walking ? identityRun + delta : 0;
            if (walking) identityHeading = (Champion.Position - Champion.PreviousPosition).normalized;
            if (Elapsed >= identityStepAt)
            {
                identityStepAt = Elapsed + .35f;
                if (identityTrail.Count >= 4) identityTrail.RemoveAt(0); identityTrail.Add(Champion.Position);
                foreach (var m in identityMarks) IdentityGlyph(MorphGeometry.Seal, m.Target.Position, m.Target.Position, .23f, .4f, role: HeroGlyphRole.Mark);
                if (identityBounty != null && identityBounty.Alive && IdentityId == "TreasureAcorn") IdentityGlyph(MorphGeometry.Seal, identityBounty.Position, identityBounty.Position, .32f, .4f, role: HeroGlyphRole.Mark);
            }
            UnitState target = IdentityTarget(Champion.Position, 8, IdentityId == "VelvetRegent" || IdentityId == "TreasureAcorn" ? IdentityApproach : 0);
            ProcessKitPassives(delta, movement, walking, moved, target);
            bool transformed = ProcessAwakening(delta, walking, target);
            if (!transformed) ProcessKitFill(delta, movement, walking, moved, target);
            SettleGauge(delta);
            // full charges climb the gauge (one tier each); while transformed or cooling the charge waits
            float required = Mathf.Max(.01f, HeroIdentityRequired);
            while (identityCharge >= required && !HeroAwakening && !HeroAwakenCooling && awakenMeter < 1) { identityCharge -= required; AwakenClimb(); }
            identityCharge = Mathf.Clamp(identityCharge, 0, required);
        }

        /// <summary>What fills each hero's gauge, besides the events above.</summary>
        private void ProcessKitFill(float delta, Vector2 movement, bool walking, float moved, UnitState target)
        {
            Fill(GaugeSource.Distance, moved);
            Fill(GaugeSource.Time, delta);
            if (GaugeUses(GaugeSource.NearEnemies) && IdentityTarget(Champion.Position, 6) != null) Fill(GaugeSource.NearEnemies, delta);
            if (GaugeUses(GaugeSource.Sprouts) && units.Exists(u => u.Alive && u.Faction == Faction.Companion && (u.Definition.Id == "GrandSprout" || u.Definition.Id == "LittleSprout"))) Fill(GaugeSource.Sprouts, delta);
            if (GaugeUses(GaugeSource.NearMiss)) IdentityNearMiss();
            switch (IdentityId)
            {
                case "SoapSkater":
                    if (walking && movement.sqrMagnitude > .01f && Elapsed >= identitySecondAt)
                    {
                        Vector2 heading = movement.normalized;
                        if (identityGhost.sqrMagnitude > .1f && Vector2.Dot(identityGhost, heading) < .574f)
                        {
                            Fill(GaugeSource.Turn); identitySecondAt = Elapsed + 1.2f;
                            // the bubble left on the turn slows and gently pushes (a passive: it barely hurts)
                            IdentityHit(MorphGeometry.Disc, Champion.Position, Champion.Position + heading, .9f, IdentityPower * .15f, .65f, 3, slow: .7f, push: .2f);
                        }
                        identityGhost = heading;
                    }
                    break;
            }
        }

        /// <summary>The hero passives that act by themselves.</summary>
        private void ProcessKitPassives(float delta, Vector2 movement, bool walking, float moved, UnitState target)
        {
            HeroKit k = Kit; if (k == null) return;
            if (k.Has(KitPassive.StillNextAttack)) { if (identityStill >= .5f) kitStillArmed = 1; }
            if (k.Has(KitPassive.LidCatch) && identityStill >= .5f && Elapsed >= kitLidAt && CatchIdentityBullet(Champion.Position, 1.6f)) kitLidAt = Elapsed + k.Value(KitPassive.LidCatch);
            if (k.Has(KitPassive.RootCatch))
            {
                if (Elapsed >= kitRootAt && CatchIdentityBullet(Champion.Position, 2f)) kitRootAt = Elapsed + k.Value(KitPassive.RootCatch);
                if (Elapsed >= identityTickAt)
                {
                    identityTickAt = Elapsed + .4f;
                    for (int i = 0; i < 3; i++) { Vector2 p = Champion.Position + (Vector2)(Quaternion.Euler(0, 0, 90 + i * 120) * Vector2.right) * 1.8f; IdentityGlyph(MorphGeometry.Seal, p, p, .2f, .45f, role: HeroGlyphRole.Ambient); }
                }
            }
            if (k.Has(KitPassive.HealWhileStill) && identityStill >= .5f && Elapsed >= kitHealAt) { kitHealAt = Elapsed + k.Value(KitPassive.HealWhileStill); Heal(1, IdentityCause()); }
            if (k.Has(KitPassive.MirrorCatch) && identityReflections.Count < 1 && Elapsed >= identitySecondAt)
            {
                for (int i = projectiles.Count - 1; i >= 0; i--)
                {
                    var p = projectiles[i]; if (p.Faction != Faction.Enemy || (p.Position - Champion.Position).sqrMagnitude > 1.4f * 1.4f) continue;
                    identityReflections.Add(new IdentityReflection { Direction = -p.Velocity.normalized, Element = p.Element }); HeroIdentityPrevented += p.Damage;
                    IdentityGlyph(MorphGeometry.Ring, p.Position, Champion.Position, .35f, .35f, role: HeroGlyphRole.Catch); projectiles.RemoveAt(i);
                    identitySecondAt = Elapsed + k.Value(KitPassive.MirrorCatch); identityBankAt = Elapsed; Fill(GaugeSource.Block); break;
                }
                if (identityReflections.Count > 0 && Elapsed - identityBankAt >= 2 && target != null) ReleaseIdentityReflections();
            }
            if (k.Has(KitPassive.BountyGold) && (identityBounty == null || !identityBounty.Alive)) identityBounty = target;
            if (k.Has(KitPassive.RootGrip) && Elapsed >= kitRootGripAt && IdentityTarget(Champion.Position, 4) is UnitState gripped)
            {
                kitRootGripAt = Elapsed + k.Value(KitPassive.RootGrip);
                KitSlowEnemy(gripped, 1, 1); IdentityGlyph(MorphGeometry.Seal, gripped.Position, gripped.Position, .35f, 1, role: HeroGlyphRole.Mark);
            }
            if (k.Has(KitPassive.TimeDebt)) CollectDebt(target);
        }

        /// <summary>HourglassStag's time debt: the deferred damage is collected from its debtor with interest.</summary>
        private void CollectDebt(UnitState target)
        {
            if (identityBank <= 0 || identityBankAt < 0 || target == null) return;
            float wait = IdentityApproach == 1 ? 2.5f : IdentityApproach == 2 ? 5 : 4;
            if (Elapsed - identityBankAt < wait) return;
            UnitState debtor = identityDebtor != null && identityDebtor.Alive && CanHit(Faction.Champion, identityDebtor) ? identityDebtor : target;
            float interest = IdentityApproach == 1 ? 2.2f : 2.6f;
            if (IdentityApproach == 2) IdentityShot(Champion.Position, debtor.Position - Champion.Position, identityBank * interest, 3, element: DamageElement.Frost, homingTarget: debtor.Id);
            else IdentityHit(MorphGeometry.Disc, debtor.Position, debtor.Position, IdentityApproach == 1 ? 1.4f : .6f, identityBank * interest, .2f);
            identityBank = 0; identityBankAt = -1;
        }

        private bool CatchIdentityBullet(Vector2 position, float radius)
        {
            for (int i = projectiles.Count - 1; i >= 0; i--)
                if (projectiles[i].Faction == Faction.Enemy && (projectiles[i].Position - position).sqrMagnitude <= radius * radius)
                { HeroIdentityPrevented += projectiles[i].Damage; projectiles.RemoveAt(i); IdentityGlyph(MorphGeometry.Ring, position, position, .6f, .3f, role: HeroGlyphRole.Catch); Fill(GaugeSource.Block); return true; }
            return false;
        }
        private void ReleaseIdentityReflections()
        {
            var target = IdentityTarget(Champion.Position); if (target == null || identityReflections.Count == 0 || Elapsed < identityAt) return;
            int count = identityReflections.Count;
            for (int i = 0; i < count; i++)
            {
                Vector2 dir = IdentityApproach == 1 ? identityReflections[i].Direction : (Vector2)(Quaternion.Euler(0, 0, (i - (count - 1) * .5f) * 15) * (target.Position - Champion.Position).normalized);
                IdentityShot(Champion.Position, dir, IdentityPower * .4f, element: identityReflections[i].Element);
            }
            identityReflections.Clear(); identityAt = Elapsed + 1; identityBankAt = -1;
        }
    }
}
