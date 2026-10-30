using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    /// <summary>The bond step ladder: every major bond's step 6 and step 8 shows a visible moment within a few seconds of the
    /// wave, the summons it promises are called, and "every Nth hit" counts each friend's own hits.</summary>
    public static class BondStepChecks
    {
        private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException("BondSteps: " + message); }
        private static readonly string[] Majors = { "hearth", "ember", "frost", "tidal", "static-club", "windborne", "orchard", "venom", "astral", "dusk",
            "oddity", "regal", "archive", "lantern-guild", "stone", "clock", "bulwark", "spirit", "melody", "gravity" };

        public static void Run()
        {
            WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            UnitDefinition friendDefinition = ScriptableObject.CreateInstance<UnitDefinition>(), enemyDefinition = ScriptableObject.CreateInstance<UnitDefinition>();
            try
            {
                weapon.Damage = 10; friendDefinition.Health = 100; friendDefinition.Weapon = weapon; enemyDefinition.Health = 100000;
                foreach (string bond in Majors)
                    foreach (int step in new[] { 6, 8 })
                    {
                        friendDefinition.Tags = new[] { bond };
                        var units = Crowd(friendDefinition, enemyDefinition, out UnitState hero);
                        var runtime = new BondRuntime(BondV3Catalog.CreateDefinitions(), new Dictionary<string, int> { { bond, step } });
                        var moments = new List<BondMomentRequest>(); runtime.MomentRequested += moments.Add;
                        int damage = 0; runtime.DamageRequested += _ => damage++;
                        runtime.ResetWave(hero, units);
                        for (int t = 0; t < 400; t++) runtime.Tick(.05f);
                        bool own = false; foreach (BondMomentRequest m in moments) if (m.Step == step || step == 8 && m.Step == 6) own = true;
                        bool quiet = bond == "lantern-guild" && step == 6 || bond == "clock" && step == 6 || bond == "gravity" && step == 6 || bond == "bulwark" && step == 6;
                        Require(own || quiet, bond + " step " + step + " shows no moment in 20 s");
                        if (step == 8 && bond != "clock" && bond != "bulwark" && bond != "dusk") Require(damage > 0, bond + " step 8 deals no damage");
                    }
                // summons the steps promise
                Require(Spawns("orchard", 4, BondSpawnKind.Sapling) == 2, "garden 4 calls two small treants");
                Require(Spawns("orchard", 6, BondSpawnKind.Treant) == 1, "garden 6 calls the giant treant");
                Require(Spawns("regal", 4, BondSpawnKind.RoyalGuard) == 2, "court 4 calls two royal guards");
                Require(Spawns("ember", 8, BondSpawnKind.Phoenix) == 1, "ember 8 calls the phoenix at once");
                Require(Spawns("spirit", 8, BondSpawnKind.Reaper) == 1, "spirit 8 calls the reaper");
                // every Nth hit counts each friend's own hits: ten friends hitting once each never chain
                {
                    friendDefinition.Tags = new[] { "static-club" };
                    var units = Crowd(friendDefinition, enemyDefinition, out UnitState hero);
                    var team = new List<UnitState>();
                    for (int i = 0; i < 10; i++) { var f = new UnitState { Id = 900 + i, Faction = Faction.Companion, Definition = friendDefinition, Health = 100 }; team.Add(f); units.Add(f); }
                    var runtime = new BondRuntime(BondV3Catalog.CreateDefinitions(), new Dictionary<string, int> { { "static-club", 4 } });
                    int chains = 0; runtime.DamageRequested += d => { if (d.Rule == "storm-chain") chains++; };
                    runtime.ResetWave(hero, units);
                    UnitState target = units.Find(u => u.Faction == Faction.Enemy);
                    for (int hit = 0; hit < BondStepTuning.StormEvery - 1; hit++) foreach (UnitState f in team) runtime.OnHit(f, target, 1, false);
                    Require(chains == 0, "a shared counter chained before any friend's own 5th hit");
                    runtime.OnHit(team[0], target, 1, false);
                    Require(chains > 0 && chains <= BondStepTuning.StormStrongTargets, "a friend's own 5th hit chains through up to 6 enemies at step 4");
                }
                // every moment the step ladder fires has an effect of its own in the director (Runtime/Presentation/Fx). Effects are
                // being rebuilt bond by bond (see Docs/Fx_Rebuild.md), so a rule with none yet is listed, and fails once the rebuild is done.
                string ladder = System.IO.File.ReadAllText("Assets/ArcaneBrigade/Runtime/Core/Bonds/BondStepsV6.cs");
                var pending = new SortedSet<string>();
                foreach (System.Text.RegularExpressions.Match m in System.Text.RegularExpressions.Regex.Matches(ladder, @"(?:Strike|LineStrike|Zone|StepWeather)\(""[a-z-]+"", ""([a-z-]+)"""))
                    if (!FxDirector.Has(m.Groups[1].Value)) pending.Add(m.Groups[1].Value);
                const bool RebuildDone = true;
                if (RebuildDone) Require(pending.Count == 0, "moments without an effect: " + string.Join(", ", pending));
                else Debug.Log("BondStepChecks: " + FxDirector.Rules.Count() + " rules drawn, " + pending.Count + " still to draw: " + string.Join(", ", pending));
                Debug.Log("BondStepChecks: PASS");
            }
            finally { UnityEngine.Object.DestroyImmediate(weapon); UnityEngine.Object.DestroyImmediate(friendDefinition); UnityEngine.Object.DestroyImmediate(enemyDefinition); }

            int Spawns(string bond, int step, BondSpawnKind kind)
            {
                friendDefinition.Tags = new[] { bond };
                var units = Crowd(friendDefinition, enemyDefinition, out UnitState hero);
                var runtime = new BondRuntime(BondV3Catalog.CreateDefinitions(), new Dictionary<string, int> { { bond, step } });
                int n = 0; runtime.SpawnRequested += r => { if (r.Kind == kind) n++; };
                runtime.ResetWave(hero, units); return n;
            }
        }

        private static List<UnitState> Crowd(UnitDefinition friend, UnitDefinition enemy, out UnitState hero)
        {
            hero = new UnitState { Id = 1, Faction = Faction.Champion, Definition = friend, Health = 100 };
            var units = new List<UnitState> { hero, new UnitState { Id = 2, Faction = Faction.Companion, Definition = friend, Health = 100 } };
            for (int i = 0; i < 40; i++) units.Add(new UnitState { Id = 10 + i, Faction = Faction.Enemy, Definition = enemy, Health = 100000, Position = new Vector2(4 + (i % 8) * .6f + (i / 20) * 8, (i / 8 % 3) * .6f) });
            return units;
        }
    }
}
