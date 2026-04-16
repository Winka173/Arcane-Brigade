using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Every hero's transformation (18/10/2026; see HeroKits): the hero's one mechanic. Its gauge climbs a tier for every
    /// full charge of the hero's own resource; at the third tier the hero transforms by itself (no button) for about six seconds,
    /// then rests for three. While transformed the kit's stats apply (attack speed, move speed, guard, crit, the team's attack
    /// speed) and the transformation acts every step. The approach shapes it: the first focuses its damage, the second spreads
    /// it over more targets, the third lasts longer and heals the hero. Strikes use the identity strike shapes, so the painted
    /// area is the area that hits; nothing bursts in a ring from the hero.</summary>
    public sealed partial class CombatSimulation
    {
        public const float AwakenSeconds = 6f, AwakenCooldown = 3f, AwakenTierBonus = .12f;
        private float awakenPrimedAt = -1, awakenReceiptAt, awakenMeter, awakenUntil = -1, awakenStarted = -100, awakenCoolUntil = -1, awakenShotAt, awakenRamAt, awakenPressure, awakenEchoAt, awakenHealAt;
        private int awakenRound, awakenFlurry;
        private UnitState awakenFoe;
        private readonly List<Vector2> awakenPath = new List<Vector2>();
        private readonly Vector2[] awakenEchoes = new Vector2[3];
        private Vector2 awakenAim;

        // the five expansion heroes' transformations: the kite fleet's escorted friends, the bonsai's planted spot, falling objects
        public const float GardenReach = 2.6f;
        private readonly List<UnitState> awakenFleet = new List<UnitState>();
        private readonly List<UnitState> awakenPicks = new List<UnitState>();
        private Vector2 awakenPlot;
        /// <summary>Something the transformation drops from above (a giant scoop, a depth charge): where, and when it lands.</summary>
        public struct AwakenDrop { public Vector2 At; public float Born, LandAt; public int Kind; }
        private readonly List<AwakenDrop> awakenDrops = new List<AwakenDrop>();
        /// <summary>KITE FLEET: the ranged friends a kite flies over (their shots split).</summary>
        public IReadOnlyList<UnitState> HeroKiteFleet => awakenFleet;
        /// <summary>TIME GARDEN: where the clockwork bonsai stands.</summary>
        public Vector2 HeroAwakenPlot => awakenPlot;
        /// <summary>Scoops (Kind 0) and depth charges (Kind 1) on their way down; landed ones fall away after 0.4s.</summary>
        public IReadOnlyList<AwakenDrop> HeroAwakenDrops => awakenDrops;

        /// <summary>Every hero with a kit has an awakening.</summary>
        public static bool HasAwakening(string heroId) => HeroKits.For(heroId) != null;
        public bool HeroAwakenEnabled => HeroIdentityEnabled && Kit != null;
        public HeroKit HeroKitInfo => Kit;
        /// <summary>0..1: one third per full charge; full transforms.</summary>
        public float HeroAwakenMeter => awakenMeter;
        public int HeroAwakenTier => Mathf.Clamp(Mathf.FloorToInt(awakenMeter * 3 + .001f), 0, 3);
        public bool HeroAwakening => HeroAwakenEnabled && awakenUntil > Elapsed;
        private float AwakenLength => (Kit?.Seconds ?? AwakenSeconds) + (IdentityApproach == 2 ? 1.5f : 0) + (Kit != null && Kit.Has(KitPassive.DrinkExtendsAwaken) ? Mathf.Min(3, Mathf.Floor(catalog.HeroDrinks / 2f)) : 0);
        /// <summary>1 at the start of a transformation, 0 at its end.</summary>
        public float HeroAwakenRemaining => HeroAwakening ? Mathf.Clamp01((awakenUntil - Elapsed) / Mathf.Max(.1f, AwakenLength)) : 0;
        public float HeroAwakenStarted => awakenStarted;
        public bool HeroAwakenCooling => HeroAwakenEnabled && !HeroAwakening && awakenCoolUntil > Elapsed;
        /// <summary>The steam engine's boiler pressure, 0..1 (1 is the red line).</summary>
        public float HeroAwakenPressure => awakenPressure;
        /// <summary>Where the full-moon dreams (or the split jellies, the showtime copies) stand while transformed.</summary>
        public IReadOnlyList<Vector2> HeroAwakenEchoes => awakenEchoes;
        /// <summary>Where a transformed gun last aimed.</summary>
        public Vector2 HeroAwakenAim => HeroAwakening ? awakenAim : Vector2.zero;
        /// <summary>The transformation's chosen foe (the duel).</summary>
        public UnitState HeroAwakenFoe => HeroAwakening ? awakenFoe : null;
        /// <summary>Transformed weapons that replace the hero's own attacks.</summary>
        private bool HeroAwakenReplacesAttacks => HeroAwakening && Kit.ReplacesAttacks;
        /// <summary>The mechanic's strength before transforming: tiers I and II add 12% each.</summary>
        private float AwakenScale => HeroAwakenEnabled && !HeroAwakening ? 1 + AwakenTierBonus * Mathf.Min(2, HeroAwakenTier) : 1;
        /// <summary>The approach's damage weight: focused, wide or lasting.</summary>
        private float ApproachScale => IdentityApproach == 0 ? 1.15f : IdentityApproach == 2 ? .85f : 1f;
        private int ApproachTargets => IdentityApproach == 1 ? 5 : 3;

        /// <summary>Captures only: fill the gauge so the next moment with an enemy near transforms.</summary>
        public void PrimeAwakeningForCapture() { if (HeroAwakenEnabled) { awakenMeter = 1; awakenCoolUntil = -1; awakenPrimedAt = Elapsed - GaugePrimeSeconds; } }
        /// <summary>The gauge is full and flashing: the hero transforms within a second.</summary>
        public bool HeroAwakenPrimed => HeroAwakenEnabled && !HeroAwakening && awakenMeter >= 1;
        private void ResetAwakening()
        {
            awakenMeter = awakenPressure = 0; awakenUntil = awakenCoolUntil = -1; awakenPrimedAt = -1; awakenStarted = -100; awakenShotAt = awakenRamAt = awakenEchoAt = awakenHealAt = 0; awakenRound = awakenFlurry = 0; awakenFoe = null;
            awakenPath.Clear(); for (int i = 0; i < awakenEchoes.Length; i++) awakenEchoes[i] = Champion != null ? Champion.Position : Vector2.zero;
            awakenFleet.Clear(); awakenDrops.Clear(); awakenPlot = Champion != null ? Champion.Position : Vector2.zero;
            if (Champion != null) Champion.AwakenArt = null;
        }
        /// <summary>A full charge: climb a tier.</summary>
        private void AwakenClimb() { if (HeroAwakenEnabled && !HeroAwakening && awakenCoolUntil <= Elapsed) awakenMeter = Mathf.Min(1, awakenMeter + 1f / 3f); }

        /// <summary>Runs every step; true while transformed.</summary>
        private bool ProcessAwakening(float delta, bool walking, UnitState target)
        {
            if (!HeroAwakenEnabled) return false;
            if (awakenPath.Count == 0 || (Champion.Position - awakenPath[awakenPath.Count - 1]).sqrMagnitude > .0004f)
            { awakenPath.Add(Champion.Position); if (awakenPath.Count > 160) awakenPath.RemoveAt(0); }
            DreamSwap(); // round 6
            if (!HeroAwakening)
            {
                if (awakenUntil > 0 && Elapsed >= awakenUntil) EndAwakening();
                // a full gauge flashes for a second before the hero transforms (HeroAwakenPrimed), so the moment is never a surprise
                if (awakenMeter >= 1 && awakenPrimedAt < 0) awakenPrimedAt = Elapsed;
                if (awakenMeter >= 1 && target != null && Elapsed - awakenPrimedAt >= GaugePrimeSeconds) BeginAwakening(target);
                if (!HeroAwakening) return false;
            }
            HeroKit k = Kit;
            if (IdentityApproach == 2 && Elapsed >= awakenHealAt) { awakenHealAt = Elapsed + 2; Heal(1, IdentityCause()); }
            target ??= IdentityTarget(Champion.Position, 8);
            switch (k.Action)
            {
                case KitAction.Gatling: GunBurst(target, .2f, .22f, true); break;
                case KitAction.AcornGun: GunBurst(identityBounty != null && identityBounty.Alive && CanHit(Faction.Champion, identityBounty) ? identityBounty : target, .22f, .1f, false); break;
                case KitAction.Kitchen: Kitchen(target); break;
                case KitAction.Engine: SteamEngine(delta); break;
                case KitAction.Dreams: Echoes(PathAgo(1), PathAgo(2), PathAgo(3), .22f, .48f, IdentityApproach == 2 ? DamageElement.Frost : DamageElement.Arcane); DreamShots(); break;
                case KitAction.Mitosis: { Vector2 side = Perpendicular() * 1.1f; Echoes(Champion.Position + side, Champion.Position - side, Champion.Position, .3f, .85f, DamageElement.Physical); break; } // round 6: Lone Jelly splits strike every .45 -> .3 s, .7 -> .85 W
                case KitAction.Showtime: Showtime(target); break;
                case KitAction.Lighthouse: Lighthouse(); break;
                case KitAction.Treant: Every(.35f, () => { if (walking) Hit(MorphGeometry.Disc, Champion.Position + Forward() * .4f, Champion.Position + Forward(), .9f, .42f, .1f); }); break;
                case KitAction.Casino: Every(1f, () => Casino(target)); break;
                case KitAction.HighTide: Every(.3f, () => { Hit(MorphGeometry.Crescent, Champion.Position, Champion.Position + Forward(), 1.3f, .34f, .05f, push: .45f); }); HealEvery(.5f); break; // round 3: 1 → 2 HP/s (Tide Singer lost 6/6)
                case KitAction.Meteors: Every(.25f, () => Meteor()); break; // round 6: a meteor every .3 -> .25 s
                case KitAction.Mirrors: Mirrors(); break;
                case KitAction.Spring: Every(.65f, () => Spring(target)); HealEvery(1); break; // round 6: blossom every .8 -> .65 s
                case KitAction.Kintsugi: Every(.4f, () => { if (target != null) Hit(MorphGeometry.Line, target.Position - Forward() * .8f, target.Position + Forward() * .8f, 0, .42f, .3f, element: DamageElement.Ember); }); break;
                case KitAction.Rink: Every(.35f, () => { Vector2 way = target != null ? (target.Position - Champion.Position).normalized : Forward(); Hit(MorphGeometry.Line, Champion.Position, Champion.Position + way * 3.2f, 0, .6f, .1f, slow: .6f); }); break; // round 6: Soap Skater ice line every .5 -> .35 s, .5 -> .6 W
                case KitAction.Parade: Every(1.2f, () => { foreach (UnitState f in units) if (f.Alive && f.Faction == Faction.Companion) IdentityGlyph(MorphGeometry.Seal, f.Position + Vector2.up * .5f, f.Position, .2f, .5f, role: HeroGlyphRole.Emblem); }); break; // Knighting (KitDamageDealt, KitCycle)
                case KitAction.Symphony: Every(1f, () => IdentityPerform(IdentityPower * .9f * ApproachScale, 5)); break;
                case KitAction.Duel: Duel(); break;
                case KitAction.Feast: Every(.4f, () => Feast()); break;
                case KitAction.Throne: Every(.7f, () => { if (target != null) Hit(MorphGeometry.Line, target.Position + Perpendicular() * .9f, target.Position - Perpendicular() * .9f, 0, .4f, .2f); }); break; // a bramble lash; thorns also answer every blow (ObserveInjury), the throne guards (KitDamageTaken)
                case KitAction.Train: Every(.22f, () => { Vector2 back = -Forward(); Hit(MorphGeometry.Line, Champion.Position, Champion.Position + back * 3f, 0, .5f, .05f, push: .35f); }); break; // round 6: Citrus Marshal carriages every .3 -> .22 s, .4 -> .5 W
                case KitAction.Coronation: Every(.8f, () => { if (target != null) Hit(MorphGeometry.Line, Champion.Position, Champion.Position + (target.Position - Champion.Position).normalized * MaxLineLength, 0, .8f, .25f); }); break;
                case KitAction.Citadel: Every(.5f, () => { if (target != null) IdentityShot(Champion.Position, target.Position - Champion.Position, IdentityPower * .65f * ApproachScale, 2, 10); }); break; // round 6: wall shots .55 -> .65 W
                case KitAction.Exhibit: Every(.7f, () => Exhibit(target)); break;
                case KitAction.BullMarket: break; // coins count double and every pickup fires a receipt (ObserveHeroIdentityEvents)
                case KitAction.Thunder: Every(.8f, () => Thunder(target)); break;
                case KitAction.Eclipse: Every(.7f, () => Eclipse(target)); break;
                case KitAction.Tapestry: Every(.5f, () => Tapestry()); break;
                case KitAction.Foreclosure: Foreclosure(target); break;
                case KitAction.Monsoon: Every(.4f, () => Monsoon()); HealEvery(1.5f); break; // round 6: monsoon page every .5 -> .4 s
                case KitAction.KiteFleet: if (Elapsed >= awakenEchoAt) { awakenEchoAt = Elapsed + 1; PickFleet(); } break; // the friends' shots split (KitEmitted)
                case KitAction.SundaeRain: Every(.45f, () => SundaeScoop()); break;
                case KitAction.TimeGarden: Every(.5f, () => GardenTick()); break;
                case KitAction.Circus: break; // the opening balls (BeginAwakening); every throw then tosses two pins (ArsenalCombat)
                case KitAction.DivingBell: DivingBell(); break;
            }
            for (int i = awakenDrops.Count - 1; i >= 0; i--) if (Elapsed > awakenDrops[i].LandAt + .4f) awakenDrops.RemoveAt(i);
            if (HeroAwakening && Elapsed >= awakenUntil - .0001f) EndAwakening();
            return true;
        }

        private void BeginAwakening(UnitState target)
        {
            HeroKit k = Kit;
            awakenStarted = Elapsed; awakenUntil = Elapsed + AwakenLength; awakenMeter = 1; awakenPressure = 0; awakenPrimedAt = -1;
            awakenShotAt = awakenRamAt = awakenEchoAt = awakenHealAt = Elapsed; awakenRound = awakenFlurry = 0; awakenFoe = target;
            Champion.AwakenArt = k.Held ? k.ArtKey : null;
            if (k.ReplacesAttacks) Champion.PendingAttack = false; // a blow already raised with the usual weapon is not thrown
            identityCharge = 0;
            HeroIdentityActivations++; HeroIdentityLastCast = Elapsed; Champion.LastSupport = Elapsed;
            var cause = IdentityCause(); RecordActivation(cause);
            events.Add(new CombatEvent(CombatEventKind.HeroIdentityCast, Champion.Position, Faction.Champion, 1, subject: Champion, cause: cause));
            IdentityGlyph(MorphGeometry.Seal, Champion.Position, Champion.Position, .5f, .9f, role: HeroGlyphRole.Emblem);
            awakenFleet.Clear(); awakenDrops.Clear();
            if (k.Action == KitAction.KiteFleet) { PickFleet(); awakenEchoAt = Elapsed + 1; }
            if (k.Action == KitAction.TimeGarden) awakenPlot = ClampPosition(Champion.Position + Forward() * 1.4f, .3f);
            if (k.Action == KitAction.Circus) CircusBalls();
            if (k.Action == KitAction.DivingBell) awakenEchoAt = Elapsed + .6f;
            if (k.Action == KitAction.Coronation && target != null)
            {
                Vector2 dir = (target.Position - Champion.Position).normalized;
                Hit(MorphGeometry.Line, Champion.Position - dir * 1f, Champion.Position + dir * MaxLineLength, 0, 2.6f, .35f);
                Verdict(Champion.Position - dir * 1f, Champion.Position + dir * MaxLineLength);
            }
        }
        /// <summary>Coronation: ordinary enemies under 25% health on the line are executed (15% before balance round 4).</summary>
        private void Verdict(Vector2 from, Vector2 to)
        {
            foreach (UnitState u in units.ToArray())
                if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && u.Health < u.Definition.Health * .25f && SegmentCircle(from, to, u.Position, .9f + u.Definition.Radius) >= 0) // round 6: line .6 -> .9 wide
                { ApplyDamage(u, u.Health + 1, Faction.Champion, "Verdict", to - from, cause: IdentityCause()); RoyalMend(); }
        }
        private void EndAwakening()
        {
            if (Kit != null && Kit.Action == KitAction.Engine && awakenUntil > 0) Whistle();
            awakenUntil = -1; awakenMeter = 0; awakenCoolUntil = Elapsed + (RelicCount(RelicMechanic.EndlessEncore) > 0 ? 0 : AwakenCooldown); Champion.AwakenArt = null; awakenFoe = null;
            awakenFleet.Clear();
            // Bonsai Clocksmith: half a tier stays wound after the garden
            if (Kit != null && Kit.Has(KitPassive.KeepHalfTier)) identityCharge = Mathf.Max(identityCharge, HeroIdentityRequired * Kit.Value(KitPassive.KeepHalfTier));
        }

        /// <summary>KITE FLEET: a kite over each of the four ranged friends nearest the admiral.</summary>
        private void PickFleet()
        {
            awakenFleet.Clear();
            for (int n = 0; n < 5; n++) // round 6: Kite Admiral flies 4 -> 5 kites
            {
                UnitState best = null; float near = float.PositiveInfinity;
                foreach (UnitState u in units)
                {
                    if (!u.Alive || u.Faction != Faction.Companion || u.Definition.Weapon.Kind != WeaponKind.Projectile || awakenFleet.Contains(u)) continue;
                    float d = (u.Position - Champion.Position).sqrMagnitude; if (d < near) { near = d; best = u; }
                }
                if (best == null) break;
                awakenFleet.Add(best);
            }
        }
        /// <summary>SUNDAE STORM: a giant scoop falls where an unfrozen enemy is heading; what it lands on freezes (ObserveOwnEvent).</summary>
        private void SundaeScoop()
        {
            UnitState pick = null; int seen = 0;
            foreach (UnitState u in units) { if (!CanHit(Faction.Champion, u) || u.FrozenUntil > Elapsed || (u.Position - Champion.Position).sqrMagnitude > 49) continue; if (combatRandom.Next(++seen) == 0) pick = u; }
            pick ??= IdentityTarget(Champion.Position, 7);
            if (pick == null) return;
            const float fall = .5f;
            Vector2 at = ClampPosition(pick.Position + Vector2.ClampMagnitude(pick.Velocity * fall, .8f), .1f);
            var scoop = Hit(MorphGeometry.Disc, at, at, .8f, .45f, fall, DamageElement.Frost, slow: .6f);
            if (scoop != null) scoop.Painted = true; // the falling scoop and its landing are painted by HeroAwakeningVisual
            if (awakenDrops.Count < 12) awakenDrops.Add(new AwakenDrop { At = at, Born = Elapsed, LandAt = Elapsed + fall, Kind = 0 });
        }
        /// <summary>TIME GARDEN: the bonsai's clock hand goes once round over the transformation, striking and slowing what it crosses.</summary>
        private void GardenTick()
        {
            float turn = Mathf.Clamp01((Elapsed - awakenStarted) / Mathf.Max(.1f, AwakenLength));
            Vector2 hand = Quaternion.Euler(0, 0, 90 - turn * 360) * Vector2.right;
            var sweep = Hit(MorphGeometry.Line, awakenPlot + hand * .3f, awakenPlot + hand * GardenReach, 0, .5f, .05f, slow: 1f);
            if (sweep != null) sweep.Painted = true; // the painted clock hand is the strike
        }
        /// <summary>The bonsai's clock hand right now (for the painted hand).</summary>
        public Vector2 HeroGardenHand => Quaternion.Euler(0, 0, 90 - Mathf.Clamp01((Elapsed - awakenStarted) / Mathf.Max(.1f, AwakenLength)) * 360) * Vector2.right;
        /// <summary>GRAND CIRCUS: five balls slam down on five different enemies, one after another.</summary>
        private void CircusBalls()
        {
            awakenPicks.Clear();
            for (int n = 0; n < 5; n++)
            {
                UnitState best = null; float near = 49;
                foreach (UnitState u in units)
                {
                    if (!CanHit(Faction.Champion, u) || awakenPicks.Contains(u)) continue;
                    float d = (u.Position - Champion.Position).sqrMagnitude; if (d < near) { near = d; best = u; }
                }
                if (best == null) break;
                awakenPicks.Add(best);
            }
            for (int i = 0; i < awakenPicks.Count; i++)
            {
                float flight = .45f + i * .12f; Vector2 at = awakenPicks[i].Position;
                IdentityHit(MorphGeometry.Disc, at, at, IdentityApproach == 1 ? .7f : .55f, IdentityPower * .6f * ApproachScale, flight, 1);
                IdentityGlyph(MorphGeometry.Line, Champion.Position + Vector2.up * .5f, at, 0, flight, role: HeroGlyphRole.Lob);
            }
        }
        /// <summary>DIVING BELL: the bell rams what is ahead (and probes it) and drops a depth charge behind every 1.2s.</summary>
        private void DivingBell()
        {
            Every(.35f, () =>
            {
                Vector2 heading = Forward(), front = Champion.Position + heading * (Champion.Definition.Radius + .45f);
                Hit(MorphGeometry.Disc, front, front + heading * .4f, .75f, .3f, .05f, push: .4f);
            });
            if (Elapsed < awakenEchoAt) return;
            awakenEchoAt = Elapsed + 1.2f;
            const float fuse = .7f;
            Vector2 back = ClampPosition(Champion.Position - Forward() * .9f, .1f);
            var charge = Hit(MorphGeometry.Disc, back, back, .9f, .7f, fuse, DamageElement.Frost);
            if (charge != null) charge.Painted = true; // the ticking charge and its burst are painted by HeroAwakeningVisual
            if (awakenDrops.Count < 12) awakenDrops.Add(new AwakenDrop { At = back, Born = Elapsed, LandAt = Elapsed + fuse, Kind = 1 });
        }

        // ------------------------------------------------------------------ helpers
        private Vector2 Forward() => identityHeading.sqrMagnitude > .01f ? identityHeading.normalized : Champion.Facing.sqrMagnitude > .01f ? Champion.Facing.normalized : Vector2.right;
        private Vector2 Perpendicular() { Vector2 f = Forward(); return new Vector2(-f.y, f.x); }
        private void Every(float period, System.Action act) { if (Elapsed < awakenShotAt) return; awakenShotAt = Elapsed + period; act(); }
        private void HealEvery(float period) { if (Elapsed < awakenRamAt) return; awakenRamAt = Elapsed + period; Heal(1, IdentityCause()); }
        /// <summary>A transformation strike: budget in W, scaled by the approach (and shared by more targets on the wide one).</summary>
        private HeroIdentityGlyph Hit(MorphGeometry shape, Vector2 at, Vector2 end, float radius, float budget, float delay = .15f, DamageElement element = DamageElement.Physical, float slow = 0, float push = 0)
            => IdentityHit(shape, at, end, radius, IdentityPower * budget * ApproachScale * (radius > 0 || shape == MorphGeometry.Line ? CrowdScale : 1), delay, ApproachTargets, element, slow, push);
        /// <summary>A transformed gun: one aimed shot per period that flashes at the barrel and sounds like a gun.</summary>
        private void GunBurst(UnitState target, float period, float budget, bool stamps)
        {
            if (Elapsed < awakenShotAt) return;
            awakenShotAt = Elapsed + period;
            UnitState aim = null;
            if (stamps) { int marks = identityMarks.Count; for (int k = 0; k < marks && aim == null; k++) { var m = identityMarks[(awakenRound + k) % marks]; if (m.Target.Alive) aim = m.Target; } }
            awakenRound++;
            aim ??= target ?? IdentityTarget(Champion.Position, 8);
            if (aim == null) return;
            if (stamps) IdentityMarkTarget(aim);
            AimGun(aim);
            IdentityShot(Champion.Position, aim.Position - Champion.Position, IdentityPower * budget * ApproachScale, element: IdentityApproach == 1 ? DamageElement.Ember : DamageElement.Physical, homingTarget: aim.Id);
        }
        private void AimGun(UnitState aim)
        {
            awakenAim = (aim.Position - Champion.Position).sqrMagnitude > .0001f ? (aim.Position - Champion.Position).normalized : Champion.Facing;
            Champion.AttackDirection = awakenAim; Champion.AttackTargetId = aim.Id; Champion.AttackWindup = .04f; Champion.AttackStarted = Elapsed - .04f;
            // a shot like any gun's: the barrel flashes and the gun sounds (an identity cause: it charges nothing)
            events.Add(new CombatEvent(CombatEventKind.Fire, Champion.Position, Champion.Faction, 0f, awakenAim, Champion.Definition.Weapon, subject: Champion, cause: IdentityCause()));
        }
        private void Kitchen(UnitState target)
        {
            if (Elapsed < awakenShotAt || target == null) return;
            awakenShotAt = Elapsed + .25f; AimGun(target); // round 6: a course every .3 -> .25 s
            UnitState dessert = DessertCourse(); if (dessert != null) target = dessert; // round 6: every sixth course lands on the enemy nearest you
            // six barrels serve the menu: each shot a course lobbed where the diner will be
            float flight = .35f; Vector2 at = ClampPosition(target.Position + Vector2.ClampMagnitude(target.Velocity * flight, 1f), .1f);
            Hit(MorphGeometry.Disc, at, at, .9f, .34f, flight, (awakenRound++ % 3) switch { 0 => DamageElement.Ember, 1 => DamageElement.Frost, _ => DamageElement.Physical });
            IdentityGlyph(MorphGeometry.Line, Champion.Position, at, 0, flight, role: HeroGlyphRole.Lob);
            if (dessert != null) ShoveFromHero(dessert, .9f);
        }
        private void Echoes(Vector2 a, Vector2 b, Vector2 c, float period, float budget, DamageElement element)
        {
            awakenEchoes[0] = a; awakenEchoes[1] = b; awakenEchoes[2] = c;
            if (Elapsed < awakenEchoAt) return;
            awakenEchoAt = Elapsed + period;
            Vector2 from = awakenEchoes[awakenRound++ % 3];
            UnitState near = IdentityTarget(from, 5);
            if (near != null) IdentityPattern(near, IdentityPower * budget * ApproachScale, origin: from, element: element);
        }
        private void Showtime(UnitState target)
        {
            UnitState lead = identityLead != null && identityLead.Alive ? identityLead : units.Find(u => u.Alive && u.Faction == Faction.Companion);
            Vector2 at = lead != null ? lead.Position : Champion.Position;
            awakenEchoes[0] = at + Vector2.left * .9f; awakenEchoes[1] = at + Vector2.right * .9f; awakenEchoes[2] = at + Vector2.up * .7f;
            if (Elapsed < awakenEchoAt || target == null) return;
            awakenEchoAt = Elapsed + .4f; // round 6: the troupe performs every .5 -> .4 s
            if (lead != null) IdentityEcho(lead, target, IdentityPower * .7f * ApproachScale);
            else IdentityPattern(target, IdentityPower * .7f * ApproachScale);
        }
        /// <summary>LIGHTHOUSE sweep speed in degrees a second (owner 22/10/2026: 120 -> 75, a slower, grander sweep). Strikes still
        /// fall every 0.25s with the same budget, so the damage per second is unchanged; they now land closer together.</summary>
        public const float LighthouseSweep = 75f;
        private void Lighthouse()
        {
            if (Elapsed < awakenShotAt) return;
            awakenShotAt = Elapsed + .25f;
            // the beam sweeps once round every 4.8s (LighthouseSweep), striking what it crosses
            float angle = (Elapsed - awakenStarted) * LighthouseSweep;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
            var beam = Hit(MorphGeometry.Line, Champion.Position + dir * .5f, Champion.Position + dir * MaxLineLength, 0, .7f, .05f);
            if (beam != null) beam.Painted = true; // the sweeping beam is painted by HeroAwakeningVisual
        }
        private void Casino(UnitState target)
        {
            if (target == null) return;
            int a = combatRandom.Next(1, 7), b = combatRandom.Next(1, 7), c = combatRandom.Next(1, 7);
            identityFace = a;
            if (a == b && b == c) { Hit(MorphGeometry.Disc, target.Position, target.Position, 1.4f, 2.2f, .3f); Gold = RunSession.BoundedCounter((long)Gold + 1); HeroIdentityGold++; IdentityGlyph(MorphGeometry.Seal, target.Position, target.Position, .6f); }
            else if (a == b || b == c || a == c) IdentityPattern(target, IdentityPower * 1.0f * ApproachScale);
            else IdentityShot(Champion.Position, target.Position - Champion.Position, IdentityPower * .45f * ApproachScale, 3);
        }
        private void Meteor()
        {
            if (awakenPath.Count == 0) return;
            Vector2 at = awakenPath[Mathf.Max(0, awakenPath.Count - 1 - (awakenRound++ * 7) % Mathf.Min(awakenPath.Count, 60))];
            Hit(MorphGeometry.Disc, at, at, .9f, .55f, .5f, DamageElement.Ember);
        }
        private void Mirrors()
        {
            // every hostile shot that reaches the circling mirrors goes back at the dream
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                var p = projectiles[i]; if (p.Faction != Faction.Enemy || (p.Position - Champion.Position).sqrMagnitude > 1.7f * 1.7f) continue;
                HeroIdentityPrevented += p.Damage; projectiles.RemoveAt(i);
                var t = IdentityTarget(p.Position, 8);
                IdentityShot(p.Position, t != null ? t.Position - p.Position : -p.Velocity, IdentityPower * .3f * ApproachScale, element: p.Element);
                IdentityGlyph(MorphGeometry.Ring, p.Position, Champion.Position, .35f, .3f, role: HeroGlyphRole.Catch);
            }
            Every(.6f, () => { var t = IdentityTarget(Champion.Position, 7); if (t != null) IdentityShot(Champion.Position, t.Position - Champion.Position, IdentityPower * .2f * ApproachScale, 2, 20); });
        }
        private void Spring(UnitState target)
        {
            int bloomed = 0;
            foreach (var u in units)
            {
                if (!u.Alive || u.Faction != Faction.Companion || u.Definition.Id != "GrandSprout" && u.Definition.Id != "LittleSprout") continue;
                var near = IdentityTarget(u.Position, 3); if (near == null) continue;
                Hit(MorphGeometry.Disc, near.Position, near.Position, 1.1f, .8f, .25f); if (++bloomed >= 3) break;
            }
            if (bloomed == 0 && target != null) Hit(MorphGeometry.Disc, target.Position, target.Position, 1.1f, .9f, .25f);
        }
        private void Duel()
        {
            if (awakenFoe == null || !awakenFoe.Alive || (awakenFoe.Position - Champion.Position).sqrMagnitude > 25) { awakenFoe = IdentityTarget(Champion.Position, 5, 2); awakenFlurry = 0; }
            if (awakenFoe == null || Elapsed < awakenShotAt) return;
            // six thrusts in a flurry, then a breath
            awakenShotAt = Elapsed + (++awakenFlurry % 6 == 0 ? .7f : .16f);
            Vector2 dir = (awakenFoe.Position - Champion.Position).normalized;
            Hit(MorphGeometry.Line, Champion.Position, awakenFoe.Position + dir * .5f, 0, .32f, .04f);
        }
        private void Feast()
        {
            Vector2 f = Forward();
            UnitState meal = null; float best = 2.4f * 2.4f;
            foreach (var u in units)
            {
                if (!CanHit(Faction.Champion, u) || u.Definition.IsBoss || u.Definition.Radius >= .4f) continue;
                Vector2 d = u.Position - Champion.Position; if (d.sqrMagnitude > best || Vector2.Dot(d.normalized, f) < .2f) continue;
                if (u.Health > IdentityPower * 4) continue;
                best = d.sqrMagnitude; meal = u;
            }
            if (meal != null) { ApplyDamage(meal, meal.Health + 1, Faction.Champion, "THE FEAST", f, cause: IdentityCause(), injectedDamage: true); Heal(1, IdentityCause()); IdentityGlyph(MorphGeometry.Crescent, Champion.Position, meal.Position, 1.2f, .3f, role: HeroGlyphRole.Strike); }
            else Hit(MorphGeometry.Crescent, Champion.Position, Champion.Position + f, 1.4f, .3f, .1f);
        }
        private void Exhibit(UnitState target)
        {
            if (target == null) return;
            int puppets = Mathf.Max(1, identitySpecimens.Count);
            for (int i = 0; i < puppets; i++)
            {
                Vector2 from = Champion.Position + (Vector2)(Quaternion.Euler(0, 0, 60 + i * 120) * Vector2.right) * 1.2f;
                var specimen = i < identitySpecimens.Count ? identitySpecimens[i] : null;
                // round 3: puppet strike .34 → .5 W (Archive Curator fights nearly alone once its three absorbs are held)
                if (specimen != null && specimen.Weapon.Kind == WeaponKind.Projectile) IdentityShot(from, target.Position - from, IdentityPower * .6f * ApproachScale, 1, element: specimen.Weapon.Element);
                else IdentityPattern(target, IdentityPower * .6f * ApproachScale, origin: from); // round 6: puppet strike .5 -> .6 W
            }
        }
        private void Thunder(UnitState target)
        {
            if (target == null) return;
            UnitState from = target; Vector2 a = Champion.Position;
            Hit(MorphGeometry.Line, a, target.Position, 0, .2f, .05f, DamageElement.Arcane); target.KitShockedUntil = Elapsed + 3;
            for (int i = 0; i < 3; i++)
            {
                UnitState next = null; float best = 9;
                foreach (var u in units) { if (!CanHit(Faction.Champion, u) || u == from || u.KitShockedUntil > Elapsed + 2.9f) continue; float d = (u.Position - from.Position).sqrMagnitude; if (d < best) { best = d; next = u; } }
                if (next == null) break;
                Hit(MorphGeometry.Line, from.Position, next.Position, 0, .14f, .08f + i * .05f, DamageElement.Arcane); next.KitShockedUntil = Elapsed + 3; from = next;
            }
        }
        private void Eclipse(UnitState target)
        {
            if (target == null) return;
            // slip behind the foe (against the way it faces) and cut through it
            Vector2 facing = target.Facing.sqrMagnitude > .01f ? target.Facing.normalized : (target.Position - Champion.Position).normalized;
            Vector2 behind = ClampPosition(target.Position - facing * (target.Definition.Radius + Champion.Definition.Radius + .3f), Champion.Definition.Radius);
            Champion.PreviousPosition = Champion.Position = behind;
            Hit(MorphGeometry.Line, behind, target.Position + facing * .8f, 0, .45f, .08f);
        }
        private void Tapestry()
        {
            foreach (var u in units) if (CanHit(Faction.Champion, u) && (u.Position - Champion.Position).sqrMagnitude <= 16 && identityMarks.Count < 3) IdentityMarkTarget(u);
            Vector2 previous = Champion.Position;
            foreach (var m in identityMarks) { Hit(MorphGeometry.Line, previous, m.Target.Position, 0, .18f, .1f, slow: .6f); previous = m.Target.Position; }
        }
        private void Foreclosure(UnitState target)
        {
            // time stops for 1.5s (enemies frozen: KitEnemySpeed) and every debt is called in at once
            if (Elapsed - awakenStarted < .05f && target != null)
            {
                float owed = Mathf.Max(identityBank, IdentityPower * .6f);
                UnitState debtor = identityDebtor != null && identityDebtor.Alive && CanHit(Faction.Champion, identityDebtor) ? identityDebtor : target;
                IdentityHit(MorphGeometry.Disc, debtor.Position, debtor.Position, 1.2f, owed * 4f * ApproachScale, .6f, ApproachTargets);
                identityBank = 0; identityBankAt = -1;
                foreach (var u in units) if (u.Faction == Faction.Enemy && u.Alive) { u.NextAttack = Mathf.Max(u.NextAttack, Elapsed + 1.5f); u.NextContact = Mathf.Max(u.NextContact, Elapsed + 1.5f); }
            }
            Every(.6f, () => { if (target != null) Hit(MorphGeometry.Disc, target.Position, target.Position, .7f, .6f, .2f, DamageElement.Frost); });
        }
        private void Monsoon()
        {
            UnitState pick = null; int seen = 0;
            foreach (var u in units) { if (!CanHit(Faction.Champion, u) || (u.Position - Champion.Position).sqrMagnitude > 49) continue; if (combatRandom.Next(++seen) == 0) pick = u; }
            if (pick == null) return;
            pick.KitWetUntil = Elapsed + 3;
            Hit(MorphGeometry.Disc, pick.Position, pick.Position, .7f, .9f, .35f, DamageElement.Frost, slow: .3f);
        }

        private void SteamEngine(float delta)
        {
            awakenPressure = Mathf.Min(1, awakenPressure + delta / (AwakenLength * 1.15f));
            if (Elapsed >= awakenShotAt)
            {
                awakenShotAt = Elapsed + .25f; // round 6: Copper Cauldron steam blast every .3 -> .25 s
                Vector2 heading = Forward(), front = Champion.Position + heading * (Champion.Definition.Radius + .45f);
                Hit(MorphGeometry.Disc, front, front + heading * .4f, .75f, .32f, .05f, DamageElement.Ember, push: .45f);
            }
            if (awakenPressure >= 1) EndAwakening(); // the red line: the whistle blows now
        }
        /// <summary>A blow taken while the engine runs builds pressure faster.</summary>
        private void AwakenOnInjury() { if (HeroAwakening && Kit.Action == KitAction.Engine) awakenPressure = Mathf.Min(1, awakenPressure + .15f); }
        /// <summary>The engine's whistle: one long steam jet ahead, stronger the higher the pressure.</summary>
        private void Whistle()
        {
            Vector2 heading = Forward(), from = Champion.Position + heading * (Champion.Definition.Radius + .2f);
            IdentityHit(MorphGeometry.Line, from, from + heading * MaxLineLength, 0, IdentityPower * (1f + 1f * awakenPressure) * ApproachScale, .25f, 6, DamageElement.Ember, push: .4f);
            awakenPressure = 0;
        }
        /// <summary>Where the hero stood about `seconds` ago, walked back along its path.</summary>
        private Vector2 PathAgo(int seconds)
        {
            if (awakenPath.Count == 0) return Champion.Position;
            float want = seconds * Champion.Definition.Speed * .55f, walked = 0;
            for (int i = awakenPath.Count - 1; i > 0; i--)
            {
                walked += Vector2.Distance(awakenPath[i], awakenPath[i - 1]);
                if (walked >= want) return awakenPath[i - 1];
            }
            return awakenPath[0];
        }
    }
}
