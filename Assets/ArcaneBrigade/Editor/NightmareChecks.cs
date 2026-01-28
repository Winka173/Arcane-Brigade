using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class NightmareChecks
    {
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool valid,string caption) { if (!valid) throw new InvalidOperationException("Nightmare " + caption); notes.Add("PASS " + caption); }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); NightmareAssets assets = NightmareAssets.Current;
            Check(assets != null && assets.Scenery.Length == 16 && assets.CustomerFrames.Length == 16 && assets.EndingIllustration != null,"Original scenery, boss and ending illustrations imported");
            Check(!catalog.Bosses.Contains(assets.Customer) && assets.Anatomy.Organs.Length == 3,"Story boss separate from ordinary rotation and optional Keeper");
            Check(NightmareDialogue.AuthoredLineCount >= 180,"Large original English dialogue library");
            Check(ArcadeMusic.DreamStemGain(4,2,false) < ArcadeMusic.DreamStemGain(0,2,false) && ArcadeMusic.DreamStemGain(4,0,false) == 1 && ArcadeMusic.DreamStemGain(4,2,true) == 1,"Deeper melody thins without muting the base; dawn restores clarity");
            using (var legacy = new RunSession(catalog))
            {
                legacy.Start(catalog.Champions[0].Id,271015); legacy.State.Nightmare = null;
                RunSnapshot snapshot = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(legacy.State));
                legacy.Restore(snapshot); legacy.PrepareCombat();
                Check(legacy.State.Nightmare != null && !legacy.EndingPending,"Legacy run gains narrative defaults without inventing an ending");
            }
            foreach (int wave in new[] { 1,4,5,8,9,12,13,16,17,20,500 })
            {
                using var session = new RunSession(catalog); session.Start(catalog.Champions[0].Id,271015); session.State.Wave = wave;
                Check(NightmareRules.Depth(wave) >= 0 && NightmareRules.Depth(wave) <= 4,"Bounded nightmare depth " + wave);
                session.State.CombatSeconds = 100000;
                Check(NightmareRules.Depth(wave) == Mathf.Min(4,(wave - 1) / 4),"Slow play does not worsen depth " + wave);
                foreach (MerchantMoment moment in Enum.GetValues(typeof(MerchantMoment)))
                {
                    string normal = NightmareDialogue.Pick(session.State,false,moment,0),dream = NightmareDialogue.Pick(session.State,true,moment,0);
                    Check(normal.Length > 12 && normal.Length < 150 && dream.Length > 12 && dream.Length < 150,"Depth-aware merchant copy " + wave + "/" + moment);
                }
            }
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id,271015); session.State.Wave = catalog.NormalWaves;
                uint shopRandom = session.State.ShopRandom,weaponRandom = session.State.WeaponRandom;
                CheckpointCatalog arena = session.PrepareCombat();
                Check(arena.Boss.Id == NightmareRules.CustomerId && arena.EnemyBudget == 0,"Final customer available without any secret visits");
                Check(session.State.ShopRandom == shopRandom && session.State.WeaponRandom == weaponRandom,"Narrative does not consume economy RNG");
                arena.StartingCompanions = Array.Empty<UnitDefinition>();
                var simulation = new CombatSimulation(arena); simulation.Start(arena.Seed); simulation.Champion.NextAttack = float.PositiveInfinity;
                var attacks = new HashSet<int>();
                for (int tick = 0; tick < 1000 && simulation.Phase == RunPhase.Combat; tick++)
                {
                    simulation.Champion.Health = simulation.Champion.Definition.Health;
                    simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(9,-7);
                    int before = simulation.Boss.BossVolley; simulation.Step(.05f,Vector2.zero);
                    if (simulation.Boss.BossVolley != before) attacks.Add(before % 6);
                }
                Check(attacks.Count == 6,"Six actual non-projectile customer attack sequences execute");
                Check(simulation.Projectiles.Where(shot => shot.Faction == Faction.Enemy).All(shot => shot.Cause.ItemId != null && shot.Cause.ItemId.StartsWith("boss-encore-")),"Customer retains six signature sequences; only the new telegraphed encore adds projectiles");
                float clock = simulation.Elapsed; simulation.Pause(true); simulation.Step(.05f,Vector2.zero); Check(simulation.Elapsed == clock,"Pause freezes final battle"); simulation.Pause(false);
                simulation.DamageFixture(simulation.Boss,simulation.Boss.Health + 1); simulation.Step(.05f,Vector2.zero); session.Settle(simulation);
                Check(session.State.Phase == SessionPhase.Victory && session.EndingPending && session.State.Nightmare.CustomerDefeated,"Customer victory opens dawn rather than bypassing ending");
                Check(session.State.DefeatedBosses.All(id => id != NightmareRules.CustomerId),"Story identity cannot corrupt normal boss discovery validation");
                session.EnterEndless(); Check(!session.State.Endless,"Endless cannot skip unseen ending");
                Check(session.EndingChoicePending && !session.AdvanceEnding(), "New ending cannot bypass the farewell choice");
                Check(session.ChooseFarewell(NightmareFarewell.CloseShop), "Ordinary ending choice needs no secret events");
                for (int page = 0; page < 4; page++)
                {
                    string text = NightmareDialogue.Ending(session.State,page); Check(text.Length > 50,"Ending page without secrets " + page);
                    using var restored = new RunSession(catalog); restored.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State)));
                    Check(restored.State.Nightmare.EndingPage == page && restored.EndingPending,"Ending progress round-trips " + page);
                    Check(session.AdvanceEnding(),"Advance ending once " + page);
                }
                Check(!session.AdvanceEnding() && session.State.Nightmare.EndingPage == 4,"Completed ending cannot be claimed repeatedly");
                session.EnterEndless(); Check(session.State.Endless,"Explicit endless continuation after ending");
                session.ContinueWave(); Check(session.PrepareCombat().Boss?.Id != NightmareRules.CustomerId,"No mandatory final customer loop in endless");
            }
            foreach (SecretEventKind kind in Enum.GetValues(typeof(SecretEventKind)))
                for (int choice = 0; choice < 3; choice++)
                {
                    using var session = new RunSession(catalog); session.Start(catalog.Champions[0].Id,271015); session.State.Wave = 15;
                    session.State.Nightmare.ChoiceMask = session.State.Nightmare.ResolvedMask = 1 << (int)kind;
                    session.State.Nightmare.Choices[(int)kind] = choice;
                    Check(NightmareDialogue.Memory(session.State,0).Length > 20,"Specific remembered outcome " + kind + "/" + choice);
                    Check(NightmareDialogue.Room(session.State,kind).Length > 20,"Late narrative room connection " + kind + "/" + choice);
                    if (kind == SecretEventKind.Understudy) Check(NightmareRules.LandmarkArt(session.State,2) == 4,"Reflection resolved: familiar window remains clear");
                    if (kind == SecretEventKind.ImpossibleTailor) Check(NightmareRules.LandmarkArt(session.State,3) == 8,"Tailor resolved: repaired shelf retains its familiar form");
                    if (kind == SecretEventKind.EmptyTheatre) Check(NightmareRules.LandmarkArt(session.State,4) == 10,"Theatre resolved: curtain does not become an eye");
                    using var restored = new RunSession(catalog); restored.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(session.State)));
                    Check(restored.State.Nightmare.Choices[(int)kind] == choice,"Choice memory persists " + kind + "/" + choice);
                }
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id,271015);
                session.State.Secrets.RescuedToy = true; session.State.Secrets.LetterSent = true; session.State.OutsideStage = OutsideStageStatus.Completed;
                Check(NightmareDialogue.Ending(session.State,1).Contains("letter") && NightmareDialogue.Ending(session.State,2).Contains("drawer") && NightmareDialogue.Ending(session.State,2).Contains("holds the frame"),"Rescue, letter and optional Keeper each alter the ending");
                session.State.Nightmare.Choices = new[] { 99 };
                bool invalid = false; try { session.Validate(session.State); } catch (InvalidOperationException) { invalid = true; }
                Check(invalid,"Malformed narrative journal rejected");
            }
            Directory.CreateDirectory("PreviewExports/Nightmare"); File.WriteAllLines("PreviewExports/Nightmare/Focused_Checks.txt",notes); Debug.Log("NIGHTMARE_CHECKS_PASSED " + notes.Count);
        }
    }
}
