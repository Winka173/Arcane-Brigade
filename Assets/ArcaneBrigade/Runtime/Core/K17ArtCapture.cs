using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class K17ArtCapture : MonoBehaviour
    {
        private string root;
        private ArenaController owner;
        private readonly List<string> notes = new List<string>();
        private bool failed;
        private bool posing;
        private int pose;
        private UnitVisual[] actors;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.IndexOf(args, "-arcane-k17-presentation") >= 0) return;
            if (!RunPaths.K7Capture || Array.IndexOf(args, "-arcane-k17-art") < 0 || FindAnyObjectByType<K17ArtCapture>() != null) return;
            int path = Array.IndexOf(args, "-capturePath"); if (path < 0 || path + 1 >= args.Length) return;
            var child = new GameObject("K17ArtCapture"); DontDestroyOnLoad(child);
            child.AddComponent<K17ArtCapture>().root = Path.GetFullPath(args[path + 1]);
        }
        private void Check(bool condition, string caption) { failed |= !condition; notes.Add((condition ? "PASS " : "FAIL ") + caption); }
        private void Log(string message, string trace, LogType kind) { if (kind == LogType.Error || kind == LogType.Exception || kind == LogType.Assert) { failed = true; notes.Add("FAIL " + message); } }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
        private IEnumerator Start()
        {
            Application.logMessageReceived += Log; Application.runInBackground = true; Directory.CreateDirectory(root);
            yield return K7PlayerCapture.ResizeDiagnostic(3840, 2160, Check);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171077); session.State.Wave = 2; session.State.Roster.Clear();
                storage.SaveRun(session.State); storage.SaveProfile(new PlayerProfile());
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null; yield return new WaitForSecondsRealtime(0.2f);
            owner = FindAnyObjectByType<ArenaController>(); owner.Session.CombatCatalog.EnemyBudget = 0;
            owner.Simulation.EnterRest();
            int index = 0;
            foreach (UnitDefinition definition in catalog.Bosses.Where(unit => unit.BossPattern >= 5))
            {
                UnitState unit = owner.Simulation.AddExpandedPresentationFixture(definition, new Vector2(-8 + index++ * 4, 2), 10000); unit.EntranceUntil = 0;
            }
            index = 0;
            foreach (UnitDefinition definition in catalog.Enemies.Where(unit => unit.Behavior >= EnemyBehavior.Hopper))
                owner.Simulation.AddExpandedPresentationFixture(definition, new Vector2(-8.75f + index++ * 2.5f, -2.3f), 10000);
            owner.Simulation.Pause(true); yield return null;
            owner.enabled = false;
            ArenaCamera follow = FindAnyObjectByType<ArenaCamera>(); if (follow != null) follow.enabled = false;
            Camera.main.orthographicSize = 6.2f; Camera.main.transform.position = new Vector3(0, 0, -10);
            actors = FindObjectsByType<UnitVisual>().Where(view => view.State != null && view.State.Alive && view.State.Definition.AuthoredMotion != null).ToArray();
            Check(actors.Length == 14, "Champion, five bosses and eight new enemy renderers");
            foreach (UnitVisual actor in actors)
            {
                if (actor.State.Faction == Faction.Champion) actor.State.Position = actor.State.PreviousPosition = new Vector2(-10, -4.8f);
                actor.State.PreviousPosition = actor.State.Position; actor.State.Velocity = Vector2.zero; actor.State.LastHit = -100;
            }
            posing = true;
            for (pose = 0; pose < 12; pose++)
            {
                yield return null; yield return new WaitForEndOfFrame();
                foreach (UnitVisual actor in actors)
                {
                    AuthoredMotionSet set = actor.State.Definition.AuthoredMotion;
                    Sprite expected = pose < 2 ? set.Idle[pose] : pose < 6 ? set.Walk[pose - 2] : pose < 8 ? set.Windup[pose - 6] : pose < 10 ? set.Release[pose - 8] : set.Recovery[pose - 10];
                    Check(actor.DisplayedBody == expected, actor.State.Definition.Id + " authored rendered pose " + pose);
                }
                Texture2D texture = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(root, "pose-" + pose.ToString("D2") + ".png"), texture.EncodeToPNG()); Destroy(texture);
            }
            posing = false;
            notes.Add("Isolated paused native Unity pose gallery, explicit state clock/positions and boss fixture. Not natural combat, fun/latency, audio or mobile approval.");
            Check(!failed, "No runtime errors or failed authored pose checks"); File.WriteAllLines(Path.Combine(root, "CaptureNotes.txt"), notes); Application.Quit(failed ? 1 : 0);
        }
        private void LateUpdate()
        {
            if (!posing || actors == null || pose >= 12) return;
            foreach (UnitVisual actor in actors)
            {
                UnitState unit = actor.State;
                unit.PendingAttack = pose == 6 || pose == 7; unit.AttackStarted = -100; unit.TelegraphUntil = unit.BossRecoveryUntil = 0;
                unit.PreviousPosition = unit.Position; unit.Facing = Vector2.right; unit.AttackDirection = Vector2.right; unit.TravelMode = CompanionTravelMode.Patrol;
                float time = 0.5f;
                if (pose == 1) time = 4.7f - Mathf.Repeat(unit.Id * 0.73f, 4.7f) + 0.04f;
                if (pose >= 2 && pose < 6) { unit.PreviousPosition = unit.Position + Vector2.left * 0.01f; time = (Mathf.Repeat(pose - 2 - unit.Id * 0.37f, 4) + 0.1f) / 8; }
                if (pose >= 6)
                {
                    unit.AttackStarted = 0; unit.AttackWindup = Mathf.Max(0.01f, unit.Definition.Weapon.Windup);
                    float recovery = Mathf.Max(0.06f, unit.Definition.Weapon.Recovery);
                    time = pose == 6 ? unit.AttackWindup * 0.25f : pose == 7 ? unit.AttackWindup * 0.75f : unit.AttackWindup + recovery * (pose == 8 ? 0.1f : pose == 9 ? 0.4f : pose == 10 ? 0.7f : 0.9f);
                }
                actor.ReducedMotion = false; actor.Display(time, 1);
            }
        }
    }
}
