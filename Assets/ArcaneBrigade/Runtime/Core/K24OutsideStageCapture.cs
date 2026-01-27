using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class K24OutsideStageCapture
    {
        public static IEnumerator Run(Action<bool, string> check, Func<string, IEnumerator> capture)
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 241077); session.State.Roster.Clear(); session.State.Wave = 13;
                session.State.Phase = SessionPhase.Shop; session.State.OutsideTransfers = 3; session.State.LastOutsideTransferWave = 13;
                string recruit = catalog.Companions[0].Id;
                WeaponDefinition[] materialWeapons = catalog.Weapons.Where(weapon => !weapon.SpecialOnly).Take(2).ToArray();
                foreach (WeaponDefinition weapon in materialWeapons) session.State.Roster.Add(new RosterEntry { UnitId = recruit, WeaponId = weapon.Id });
                for (int index = 0; index < 3; index++) session.State.Offers.Add(new ShopOffer { Id = catalog.Items[index].Id, Kind = OfferKind.Item, Price = 15 + index * 10, Token = ++session.State.NextOffer });
                session.State.Offers[0].Id = recruit; session.State.Offers[0].Kind = OfferKind.Companion; session.State.Offers[0].Price = 1;
                storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
            ArenaController owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            yield return new WaitForSecondsRealtime(2.8f);
            check(owner.Session.OutsideChallengeAvailable, "Native optional challenge waits in shop");
            yield return capture("01-hand-invitation");
            WorldShop.Station merge = owner.Shop.Stations.First(station => station.Offer != null && station.Offer.Kind == OfferKind.Companion);
            owner.SetPresentationMoveTarget(merge.Position);
            float arrive = Time.realtimeSinceStartup + 5;
            while (Vector2.Distance(owner.Simulation.Champion.Position, merge.Position) > 0.2f && Time.realtimeSinceStartup < arrive) yield return null;
            owner.SetPresentationMoveTarget(null);
            check(owner.Shop.Interact(), "Native committed purchase triggers real star merge");
            yield return new WaitForSecondsRealtime(0.9f);
            check(owner.Shop.Reveal.PendingCount > 0 && owner.Session.State.Roster.Any(entry => entry.Tier == 2), "Native oversized reveal shows committed tier two");
            yield return capture("01b-large-star-reveal");
            yield return new WaitForSecondsRealtime(0.4f);
            yield return capture("01c-star-landing");
            yield return new WaitForSecondsRealtime(1);
            check(owner.AcceptOutsideChallenge(), "Native challenge accepts explicitly");
            check(owner.TransitionPending, "Native challenge entry uses the carrying hand, not a teleport");
            yield return new WaitForSecondsRealtime(2.9f);
            check(owner.Session.State.Wave == 13 && owner.Simulation.Boss.Definition.Id == "OutsideStageKeeper", "Native challenge does not consume next normal wave");
            UnitState champion = owner.Simulation.Champion;
            champion.NextAttack = float.PositiveInfinity;
            UnitState boss = owner.Simulation.Boss;
            yield return new WaitForSecondsRealtime(1.4f);
            check(UnityEngine.Object.FindObjectsByType<BossAnatomyVisual>().Any(view => view.PartCount == 3), "Native hand, carpet tongue and keyhole eye are rendered");
            yield return capture("02-keeper-arrival");
            for (int phase = 0; phase < 3; phase++)
            {
                boss.Health = boss.Definition.Health * (phase == 0 ? 1 : phase == 1 ? 0.65f : 0.3f);
                boss.BossVolley = phase * 2; boss.TelegraphUntil = 0; boss.NextBossPattern = 0;
                float deadline = Time.realtimeSinceStartup + 2;
                while (boss.TelegraphUntil <= owner.Simulation.Elapsed && Time.realtimeSinceStartup < deadline) yield return null;
                check(boss.TelegraphUntil > owner.Simulation.Elapsed, "Native phase body windup " + phase);
                yield return capture("03-phase-" + phase + "-windup");
                yield return new WaitForSecondsRealtime(1.6f);
                check(owner.Simulation.ExpandedFields.Any(field => field.Source == boss), "Native phase produces actual attacks " + phase);
                yield return capture("04-phase-" + phase + "-attack");
            }
            champion.Definition.Weapon.Damage = boss.Health * 4;
            champion.Definition.Weapon.Range = 15;
            champion.Position = champion.PreviousPosition = boss.Position + Vector2.down * 2;
            champion.AttackDirection = Vector2.up; champion.AttackTargetId = boss.Id;
            champion.PendingAttack = true; champion.ImpactTime = 0;
            yield return new WaitForSecondsRealtime(0.1f);
            check(owner.Session.State.OutsideStage == OutsideStageStatus.Reward && owner.TransitionPending, "Real player attack enters boss finale before reward");
            yield return capture("05-keeper-collapse");
            yield return new WaitForSecondsRealtime(2.7f);
            check(owner.Session.State.OutsideRewards.Count == 3 && !owner.TransitionPending, "Native three gifts appear after collapse");
            yield return capture("06-prismatic-choice");
            check(owner.ClaimOutsideReward(0) && !owner.ClaimOutsideReward(0), "Native reward claims once");
            check(owner.Session.State.Phase == SessionPhase.Shop && owner.Session.State.Wave == 13, "Native returns to unchanged shop wave");
            yield return new WaitForSecondsRealtime(2.5f);
            yield return capture("07-return-to-shop");
            owner.NextWave(); yield return new WaitForSecondsRealtime(1);
            check(owner.Session.State.Wave == 14 && owner.Simulation.Phase == RunPhase.Combat, "Normal run continues after optional challenge");
            yield return capture("08-swarm-and-large-weapons");
            yield return new WaitForSecondsRealtime(1.2f);
            check(owner.Simulation.SwarmSpawnCount >= 4 && owner.Simulation.Units.Any(unit => unit.SwarmMinion), "Native pack spawning produces extra small bodies");
            yield return capture("08b-swarm-crest");
            owner.Session.State.Phase = SessionPhase.Shop; owner.Session.State.Wave = 16; owner.EnterShopPresentation();
            owner.NextWave(); yield return new WaitForSecondsRealtime(0.7f);
            check(PresentationSettings.ReducedMotion ? UnityEngine.Object.FindAnyObjectByType<StageTransfer>() == null && owner.Simulation.Phase == RunPhase.Combat : UnityEngine.Object.FindAnyObjectByType<StageTransfer>() != null, PresentationSettings.ReducedMotion ? "Reduced hand transfer completes without extended flight" : "Native hand transfer replaces the old courier");
            yield return capture("09-hand-carries-party");
            yield return new WaitForSecondsRealtime(0.85f);
            yield return capture("10-hand-toss");
            yield return new WaitForSecondsRealtime(1.6f);
            check(owner.Simulation.Phase == RunPhase.Combat && owner.Session.State.Wave == 17 && owner.Session.State.OutsideTransfers == 4, "Native transfer lands once and resumes combat");
            yield return capture("11-party-arrival");
        }
    }
}
