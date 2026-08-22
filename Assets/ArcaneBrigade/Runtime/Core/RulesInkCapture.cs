using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native review of the v3/v5 rules ink. Run with -arcane-rulesink-capture -capturePath DIR (isolated diagnostics
    /// save): a wave-12 night with a ten-companion brigade from one bond house plays for real, then every drawn shape is staged
    /// next to the hero (friendly disc, ring, line, cone; enemy circle, line, cone, arena; a fallen friend; allies and pickups)
    /// so each can be judged on a frame. Counts of what was drawn go to RulesInk.txt.</summary>
    public sealed class RulesInkCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-rulesink-capture") >= 0;
        private string root;
        private readonly List<string> log = new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<RulesInkCapture>() != null) return;
            var go = new GameObject("RulesInkCapture"); DontDestroyOnLoad(go); go.AddComponent<RulesInkCapture>();
        }
        private static string Arg(string key) { string[] a = Environment.GetCommandLineArgs(); int i = Array.IndexOf(a, key); return i >= 0 && i + 1 < a.Length ? a[i + 1] : null; }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToJPG(82)); Destroy(image);
        }
        private void Write() => File.WriteAllLines(Path.Combine(root, "RulesInk.txt"), log);
        private void OnLog(string message, string trace, LogType type) { if (type == LogType.Exception) { log.Add("ERROR " + message + " | " + trace.Split('\n')[0]); Write(); } }
        private void Count(string when, RulesV3Ink ink)
        {
            log.Add(when + ": friendly " + ink.FriendAreas + ", danger " + ink.DangerAreas + ", fallen " + ink.Fallen + ", entities " + ink.Entities + ", auras " + ink.Auras + ", chains " + ink.Chains + ", walls " + ink.Walls + ", status " + ink.StatusMarks);
            CombatSimulation sim = FindAnyObjectByType<ArenaController>()?.Simulation;
            if (sim == null) return;
            // every unit on screen must have a painting
            var bare = sim.Units.Where(u => u.Definition.Body == null && u.Definition.AuthoredMotion == null && (u.Definition.AnimationFrames == null || u.Definition.AnimationFrames.Length == 0)).Select(u => u.Faction + ":" + u.Definition.Id).Distinct().ToArray();
            if (bare.Length > 0) log.Add("  unpainted: " + string.Join(", ", bare));
        }

        private IEnumerator Start()
        {
            root = Arg("-capturePath"); if (root == null) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(root); Directory.CreateDirectory(root); Application.runInBackground = true; Application.logMessageReceived += OnLog;
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true });
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.5f);
            string house = Arg("-house") ?? "Ember";
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 280601); session.State.Wave = 12; session.State.Phase = SessionPhase.Combat;
                string bond = BondV3Catalog.BondId(house) ?? house;
                UnitDefinition[] members = catalog.Companions.Where(u => u != null && u.Body != null && !u.SpecialRecruit && (BondV3Catalog.Tags(u.Id).Contains(bond) || BondV3Catalog.Tags(u.Id).Contains(house))).Take(10).ToArray();
                string weapon = catalog.Weapons.Where(w => w != null && w.Kind != WeaponKind.Contact).Select(w => w.Id).FirstOrDefault();
                session.State.Roster.Clear();
                foreach (UnitDefinition member in members) session.State.Roster.Add(new RosterEntry { UnitId = member.Id, WeaponId = weapon, Tier = 2 });
                try { session.Validate(session.State); } catch (Exception problem) { log.Add("INVALID RUN: " + problem.Message); }
                storage.SaveRun(session.State);
                log.Add("house " + house + ": " + string.Join(", ", members.Select(m => m.Id)));
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            ArenaController owner = null;
            for (float t = 0; t < 8 && (owner == null || owner.Simulation == null || owner.Simulation.Phase != RunPhase.Combat); t += .1f) { yield return new WaitForSecondsRealtime(.1f); owner = FindAnyObjectByType<ArenaController>(); }
            if (owner?.Simulation == null || owner.RulesInk == null) { log.Add("NO COMBAT OR NO RULES INK"); Write(); Application.Quit(1); yield break; }
            RulesV3Ink ink = owner.RulesInk;
            // every bond rule key and skill seen in the live fight, to judge each house's look
            var rules = new SortedDictionary<string, int>();
            if (owner.Simulation.Bonds != null) owner.Simulation.Bonds.RuleTriggered += e => rules[e.Rule] = rules.TryGetValue(e.Rule, out int n) ? n + 1 : 1;
            if (owner.Simulation.SkillsV3 != null) owner.Simulation.SkillsV3.CastRequested += c => rules["cast:" + c.SkillId] = rules.TryGetValue("cast:" + c.SkillId, out int n) ? n + 1 : 1;
            // the real fight: bond steps and skills fire on their own
            for (int shot = 0; shot < 4; shot++) { yield return new WaitForSecondsRealtime(3f); Count("live " + shot, ink); yield return Shot(house + "-live-" + shot); }

            log.Add("rules seen: " + string.Join(", ", rules.Select(r => r.Key + " x" + r.Value)));
            if (Arg("-liveonly") != null) { Write(); Application.Quit(0); yield break; }
            // staged shapes around the hero, so each look can be judged
            CombatSimulation sim = owner.Simulation;
            owner.Simulation.Pause(true);
            Vector2 h = sim.Champion.Position;
            int source = sim.Champion.Id;
            UnitState caster = sim.Units.FirstOrDefault(u => u.Faction == Faction.Companion);
            int cast = caster != null ? caster.Id : source;
            ink.OnCast(new SkillCastRequest(cast, "TeapotPilgrim", SkillShape.Disc, h + new Vector2(-3.2f, 1.2f), Vector2.zero, 1.4f, 30, 0, 1));
            ink.OnCast(new SkillCastRequest(cast, "PillowOracle", SkillShape.Ring, h + new Vector2(0, 2.4f), Vector2.zero, 1.6f, 30, 0, 1));
            ink.OnCast(new SkillCastRequest(cast, "RainHeron", SkillShape.Line, h + new Vector2(-3.5f, -1.6f), h + new Vector2(-0.6f, -1.6f), .45f, 30, 0, 1));
            ink.OnCast(new SkillCastRequest(cast, "SnowSpoon", SkillShape.Cone, h + new Vector2(2.2f, 1.6f), h + new Vector2(4.6f, 2.4f), 2.4f, 30, 0, 1));
            ink.OnDanger(new EnemyDangerRequest(1, source, EnemyDangerShape.Circle, EnemyRuleAction.Damage, "slam", h + new Vector2(3.6f, -1.4f), Vector2.zero, 1.3f, 1.5f, 30, 1));
            ink.OnDanger(new EnemyDangerRequest(2, source, EnemyDangerShape.Line, EnemyRuleAction.Damage, "charge", h + new Vector2(0.4f, -2.8f), h + new Vector2(4.4f, -2.8f), .5f, 1.5f, 30, 1));
            ink.OnDanger(new EnemyDangerRequest(3, source, EnemyDangerShape.Cone, EnemyRuleAction.Damage, "cone", h + new Vector2(-1.2f, 3.4f), h + new Vector2(-3.6f, 4.4f), 2.6f, 1.5f, 30, 1));
            UnitState friend = sim.Units.FirstOrDefault(u => u.Faction == Faction.Companion);
            // a real fall: the body must stay on the field in its defeat pose with the countdown
            if (friend != null && sim.Mortality != null) { friend.Health = 0; sim.Mortality.OnLethal(friend, source); }
            ink.OnEntity(new RulesV3EntityRequest(9001, source, 0, RulesV3EntityKind.Ally, RulesV3EntityChange.Created, "phoenix", h + new Vector2(1.6f, -0.4f), Vector2.zero, .8f, 30, 0));
            ink.OnEntity(new RulesV3EntityRequest(9002, source, 0, RulesV3EntityKind.Snack, RulesV3EntityChange.Created, "sugar-rush", h + new Vector2(-1.4f, -0.6f), Vector2.zero, .4f, 30, 0));
            ink.OnEntity(new RulesV3EntityRequest(9003, source, 0, RulesV3EntityKind.Page, RulesV3EntityChange.Created, "page", h + new Vector2(-2.2f, 0.2f), Vector2.zero, .4f, 30, 0));
            ink.OnEntity(new RulesV3EntityRequest(9004, source, 0, RulesV3EntityKind.WindLane, RulesV3EntityChange.Created, "wind-lane", h + new Vector2(-4.5f, 3.2f), h + new Vector2(-1.8f, 4.0f), .5f, 30, 0));
            for (int i = 0; i < 6; i++) ink.OnBondRule(new BondRuleEvent(i % 2 == 0 ? "burn-stack" : "freeze", sim.Champion, 1));
            var friends = sim.Units.Where(u => u.Faction == Faction.Companion && u.Alive).ToList();
            if (friends.Count > 1) ink.OnSupport(new BondSupportRequest(BondSupportKind.Shield, friends[1], .2f, 30, 0, "bulwark"));
            if (friends.Count > 2) ink.OnSupport(new BondSupportRequest(BondSupportKind.DamageBuff, friends[2], .1f, 30, 0, "warmth"));
            ink.OnSupport(new BondSupportRequest(BondSupportKind.Wall, sim.Champion, 0, 30, 1.6f, "bastion"));
            var foes = sim.Units.Where(u => u.Faction == Faction.Enemy && u.Alive).Take(4).ToList();
            if (friends.Count > 0) foreach (UnitState foe in foes) ink.OnBondDamage(new BondDamageRequest(foe, friends[0], 10, "storm-chain"));
            sim.Pause(false);
            for (int shot = 0; shot < 3; shot++) { yield return new WaitForSecondsRealtime(.12f + shot * .45f); Count("staged " + shot, ink); yield return Shot("staged-" + shot); }
            ink.OnBondRule(new BondRuleEvent("blizzard", sim.Champion, 1));
            yield return new WaitForSecondsRealtime(.7f); Count("blizzard", ink); yield return Shot("field-blizzard");
            ink.OnBondRule(new BondRuleEvent("riptide", sim.Champion, 1));
            yield return new WaitForSecondsRealtime(.5f); Count("riptide", ink); yield return Shot("field-riptide");
            ink.OnDanger(new EnemyDangerRequest(4, source, EnemyDangerShape.Arena, EnemyRuleAction.Countdown, "countdown", h, Vector2.zero, 14, 1.5f, 30, 1));
            yield return new WaitForSecondsRealtime(.6f); Count("arena", ink); yield return Shot("staged-arena");
            Write(); Application.Quit(0);
        }
        private void OnDestroy() { Application.logMessageReceived -= OnLog; }
    }
}
