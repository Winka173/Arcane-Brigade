using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Native screenshots of every hero's identity mechanic mid-effect, for reviewing the presentation.
    /// Uses the isolated diagnostics save. Run with -arcane-mechanic-capture -capturePath DIR
    /// [-mechanic-approach 0|1|2] [-mechanic-heroes Id,Id] [-mechanic-reduced]. Never changes gameplay numbers:
    /// it only seeds the trigger state each mechanic already reads, then lets the real simulation cast it.</summary>
    public sealed class HeroMechanicCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-mechanic-capture") >= 0;
        private string root;
        private readonly List<string> notes = new List<string>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<HeroMechanicCapture>() != null) return;
            var go = new GameObject("HeroMechanicCapture"); DontDestroyOnLoad(go); go.AddComponent<HeroMechanicCapture>();
        }

        private static string Arg(string[] args, string key) { int i = Array.IndexOf(args, key); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }

        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG());
            Destroy(image);
        }

        private void Log(string message, string trace, LogType type)
        {
            if (type == LogType.Error || type == LogType.Exception) { notes.Add("ERROR " + message + " " + trace); File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes); }
        }

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            root = Arg(args, "-capturePath"); if (root == null) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(root); Directory.CreateDirectory(root); Application.runInBackground = true;
            Application.logMessageReceived += Log;
            var catalog = Resources.Load<RunCatalog>("RunCatalog"); var storage = new RunStorage(RunPaths.Root);
            if (!RunPaths.Root.Contains("ArcaneBrigadeDiagnostics")) { Application.Quit(1); yield break; }
            bool reduced = Array.IndexOf(args, "-mechanic-reduced") >= 0;
            storage.SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true, ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced });
            int approach = Mathf.Clamp(int.TryParse(Arg(args, "-mechanic-approach"), out int a) ? a : 0, 0, 2);
            string filter = Arg(args, "-mechanic-heroes");
            bool crowd = Array.IndexOf(args, "-mechanic-crowd") >= 0, shop = Array.IndexOf(args, "-mechanic-shop") >= 0;
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
            int index = 0;
            foreach (var hero in HeroIdentities.All)
            {
                index++;
                if (filter != null && Array.IndexOf(filter.Split(','), hero.Id) < 0) continue;
                if (catalog.Unit(hero.Id) == null) { notes.Add("MISSING " + hero.Id); continue; }
                using (var session = new RunSession(catalog))
                {
                    session.Start(hero.Id, 171017 + index); session.State.Wave = int.TryParse(Arg(args, "-mechanic-wave"), out int wave) ? wave : 3;
                    session.State.Roster.Clear();
                    foreach (string id in new[] { "TeapotPilgrim", "HoodWisp", "BrassAcorn", "PillowOracle" })
                        if (catalog.Unit(id) != null) session.State.Roster.Add(new RosterEntry { UnitId = id, WeaponId = catalog.Unit(id).Weapon.Id });
                    session.State.HeroIdentity.Approach = approach;
                    if (shop) { session.State.Phase = SessionPhase.Shop; session.State.Gold = 200; }
                    storage.SaveRun(session.State);
                }
                if (shop)
                {
                    RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2.6f);
                    var shopOwner = FindAnyObjectByType<ArenaController>(); shopOwner.ToggleDetails(); shopOwner.Details.SelectTab(0);
                    yield return new WaitForSecondsRealtime(.5f); yield return Shot(index.ToString("00") + "-" + hero.Id + "-a" + approach + "-shop");
                    shopOwner.ToggleDetails(); notes.Add("OK " + hero.Id + " shop"); continue;
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
                ArenaController owner = null;
                for (float t = 0; t < 6 && (owner == null || owner.Simulation == null || owner.Simulation.Phase != RunPhase.Combat); t += .1f) { yield return new WaitForSecondsRealtime(.1f); owner = FindAnyObjectByType<ArenaController>(); }
                if (owner == null || owner.Simulation == null) { notes.Add("NO COMBAT " + hero.Id); continue; }
                yield return new WaitForSecondsRealtime(.5f);
                if (Array.IndexOf(args, "-mechanic-tokens") >= 0 && index == 1 || filter != null && Array.IndexOf(args, "-mechanic-tokens") >= 0 && notes.Count == 0) yield return TokenSheet();
                var sim = owner.Simulation;
                if (Array.IndexOf(args, "-mechanic-live") >= 0) { yield return LiveSurvey(sim, index.ToString("00") + "-" + hero.Id + "-a" + approach); continue; }
                if (Array.IndexOf(args, "-mechanic-dps") >= 0) { yield return DpsBench(sim, catalog, hero.Id, int.TryParse(Arg(args, "-mechanic-dps-count"), out int n) ? n : 1); continue; }
                sim.PrepareMechanicCapture(catalog, crowd ? 12 : 0);
                Time.timeScale = .35f;
                int casts = sim.HeroIdentityActivations; float started = Time.realtimeSinceStartup;
                // a hero whose whole mechanic is its transformation is brought to a full gauge after 4s of waiting
                // -mechanic-awaken: wait for the transformation itself (not the hero's ordinary mechanic) and review it
                bool awakenOnly = Array.IndexOf(args, "-mechanic-awaken") >= 0;
                while ((awakenOnly || sim.HeroIdentityActivations == casts) && !sim.HeroAwakening && Time.realtimeSinceStartup - started < 16) { sim.NudgeMechanicCapture(); if (awakenOnly || Time.realtimeSinceStartup - started > 4) sim.FillGaugeForCapture(); yield return null; }
                float cast = sim.Elapsed;
                bool fired = sim.HeroIdentityActivations > casts || sim.HeroAwakening;
                string stem = index.ToString("00") + "-" + hero.Id + "-a" + approach;
                if (!fired) { notes.Add("NO CAST " + hero.Id); yield return Shot(stem + "-idle"); Time.timeScale = 1; continue; }
                // time the frames on the first real strike: its anticipation, its action and its follow-through
                float strike = -1; foreach (var g in sim.HeroIdentityGlyphs) if (g.Role == HeroGlyphRole.Strike && g.Created >= cast - .02f && (strike < 0 || g.At < strike)) strike = g.At;
                float[] at = strike < 0 ? new[] { .06f, .22f, .4f } : new[] { Mathf.Max(.03f, (strike - cast) * .6f), strike - cast + .07f, strike - cast + .21f };
                string[] label = { "1-anticipate", "2-action", "3-follow" };
                if (awakenOnly) { at = new[] { .12f, .4f, .9f, 1.6f, 2.8f }; label = new[] { "w1-powerup", "w2-rising", "w3-buffed", "w4-fighting", "w5-later" }; }
                // -mechanic-sequence: ten frames through the strike (its anticipation to its fade) to review motion, not stills
                if (Array.IndexOf(args, "-mechanic-sequence") >= 0)
                {
                    float first = strike < 0 ? .02f : Mathf.Max(.02f, strike - cast - .14f);
                    at = new float[10]; label = new string[10];
                    for (int f = 0; f < 10; f++) { at[f] = first + f * .065f; label[f] = "s" + f.ToString("00"); }
                }
                for (int i = 0; i < at.Length; i++)
                {
                    while (sim.Elapsed - cast < at[i] && Time.realtimeSinceStartup - started < 24) yield return null;
                    yield return Shot(stem + "-" + label[i]);
                }
                // the transformation a moment after its flash, as it reads for most of its six seconds
                while (sim.HeroAwakening && sim.Elapsed - cast < 1.2f && Time.realtimeSinceStartup - started < 26) yield return null;
                if (sim.HeroAwakening) yield return Shot(stem + "-4-held");
                // the finale as the transformation runs out: its arrival, then its burst
                if (sim.HeroAwakening && Array.IndexOf(args, "-mechanic-finale") >= 0)
                {
                    Time.timeScale = 1; float wait = Time.realtimeSinceStartup;
                    while (sim.HeroAwakening && Time.realtimeSinceStartup - wait < 14) yield return null;
                    Time.timeScale = .35f; float end = sim.Elapsed;
                    while (sim.Elapsed - end < .22f && Time.realtimeSinceStartup - wait < 20) yield return null;
                    yield return Shot(stem + "-5-finale-arrive");
                    while (sim.Elapsed - end < .55f && Time.realtimeSinceStartup - wait < 22) yield return null;
                    yield return Shot(stem + "-6-finale-burst");
                }
                notes.Add("OK " + hero.Id + " glyphs=" + sim.HeroIdentityGlyphs.Count);
                File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes);
                Time.timeScale = 1;
            }
            Time.timeScale = 1;
            File.WriteAllLines(Path.Combine(root, "Notes.txt"), notes);
            Application.Quit(0);
        }
        /// <summary>-mechanic-live [-mechanic-wave N]: ordinary play, nothing seeded, for 30 s. Every frame it looks for
        /// anything larger than 1.3 units drawn over the hero apart from the hero, its weapon, companions' own lights
        /// and ground shadows; it logs each one (what it is, its art, tint, size) and saves a frame the first time each
        /// kind appears, to find what paints over the hero during a normal fight.</summary>
        private IEnumerator LiveSurvey(CombatSimulation sim, string stem)
        {
            var tally = new Dictionary<string, int>(); // shots and hits per weapon (a hero that never attacks shows here)
            var lines = new List<string>(); var seen = new HashSet<string>(); float started = Time.realtimeSinceStartup, lastHealth = sim.Champion.Health, lastStep = -1;
            bool shots = Array.IndexOf(Environment.GetCommandLineArgs(), "-mechanic-live-shots") >= 0; float nextShot = 4; int shot = 0;
            while (sim.Phase == RunPhase.Combat && Time.realtimeSinceStartup - started < 30)
            {
                yield return null;
                // -mechanic-live-shots: a frame every four seconds of ordinary play (to see which enemies walk in and how they act)
                if (shots && Time.realtimeSinceStartup - started >= nextShot) { nextShot += 4; yield return Shot(stem + "-live" + (shot++).ToString("00")); }
                Vector2 hero = sim.Champion.Position;
                if (Mathf.Abs(sim.Champion.Health - lastHealth) > .5f) { lines.Add($"t={sim.Elapsed:0.00} hp {lastHealth:0} -> {sim.Champion.Health:0} dreaming={sim.HeroIdentityDreaming} {sim.HeroIdentityStatus}"); lastHealth = sim.Champion.Health; }
                if (sim.Elapsed != lastStep)
                {
                    lastStep = sim.Elapsed;
                    foreach (CombatEvent e in sim.Events)
                        if (e.Kind == CombatEventKind.Fire || e.Kind == CombatEventKind.Hit) { string k = e.Kind + " " + (e.Weapon?.Id ?? "-") + " " + e.Faction; tally[k] = tally.TryGetValue(k, out int c) ? c + 1 : 1; }
                    foreach (CombatEvent e in sim.Events)
                        if (e.Kind != CombatEventKind.Hit && e.Kind != CombatEventKind.Fire && e.Kind != CombatEventKind.Kill && (e.Position - hero).sqrMagnitude < 2.25f)
                            lines.Add($"t={sim.Elapsed:0.00} EVENT {e.Kind} {e.Faction} value {e.Value:0.00} weapon {e.Weapon?.Id ?? "-"} cause {e.Cause.ItemId ?? "-"} subject {e.Subject?.Definition?.Id ?? "-"}");
                }
                string fresh = null;
                foreach (Renderer view in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
                {
                    if (!view.enabled || !view.gameObject.activeInHierarchy) continue;
                    Bounds b = view.bounds; float size = Mathf.Max(b.size.x, b.size.y);
                    if (size < 1.3f || size > 14f) continue;
                    if (Mathf.Abs(b.center.x - hero.x) > b.extents.x || Mathf.Abs(b.center.y - hero.y - .4f) > b.extents.y) continue;
                    string path = view.name; Transform up = view.transform.parent; for (int depth = 0; up != null && depth < 2; depth++, up = up.parent) path = up.name + "/" + path;
                    if (path.Contains("CompanionOwnLight") || path.Contains("BodyVisual") || path.Contains("WeaponVisual") || path.Contains("GroundShadow") || path.Contains("identity portrait")) continue;
                    string art = view is SpriteRenderer s ? (s.sprite != null ? s.sprite.name : "-") + " tint " + ColorUtility.ToHtmlStringRGBA(s.color)
                        : view is LineRenderer l ? "line " + ColorUtility.ToHtmlStringRGBA(l.startColor) : view.sharedMaterial != null ? view.sharedMaterial.name : "-";
                    lines.Add($"t={sim.Elapsed:0.00}   {size:0.0}u order {view.sortingOrder} {view.GetType().Name} {path} :: {art}");
                    string key = path + "|" + (view is SpriteRenderer named && named.sprite != null ? named.sprite.name.Split('_')[0] : "");
                    if (seen.Add(key)) fresh = key;
                }
                if (fresh != null) yield return Shot(stem + "-live-" + seen.Count.ToString("00"));
            }
            foreach (var kv in tally) lines.Add("TALLY " + kv.Key + " x" + kv.Value);
            lines.Add("HERO weapon " + sim.Champion.Definition.Weapon?.Id + " arsenal " + sim.Champion.Definition.Weapon?.Arsenal + " borrowed " + sim.Champion.Borrowed?.Id);
            File.WriteAllLines(Path.Combine(root, stem + "-live.txt"), lines);
        }
        /// <summary>-mechanic-dps [-mechanic-dps-count N]: the hero's own damage a second against N still targets in its reach,
        /// six seconds of ordinary attacks (the gauge held empty), then six seconds transformed. One line a hero in dps.txt.</summary>
        private IEnumerator DpsBench(CombatSimulation sim, RunCatalog catalog, string id, int count)
        {
            sim.PrepareDpsBench(catalog, count);
            Time.timeScale = 1;
            float heroNormal = 0, heroAwake = 0, teamNormal = 0, teamAwake = 0, lastStep = -1;
            float start = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - start < 6) { sim.HoldGaugeForCapture(); Tally(ref heroNormal, ref teamNormal); yield return null; }
            start = Time.realtimeSinceStartup;
            while (!sim.HeroAwakening && Time.realtimeSinceStartup - start < 5) { sim.FillGaugeForCapture(); sim.KeepBenchAlive(); yield return null; }
            bool transformed = sim.HeroAwakening;
            start = Time.realtimeSinceStartup;
            bool shot = false; while (Time.realtimeSinceStartup - start < 6) { Tally(ref heroAwake, ref teamAwake); if (!shot && Time.realtimeSinceStartup - start > 2.5f) { shot = true; yield return Shot(id + "-bench-" + count); } yield return null; }
            File.AppendAllText(Path.Combine(root, "dps.txt"), $"{id}	{count}	hero {heroNormal / 6:0} -> {heroAwake / 6:0}	team {teamNormal / 6:0} -> {teamAwake / 6:0}	teamRatio {(teamNormal > 0 ? teamAwake / teamNormal : 0):0.00}{(transformed ? "" : "	NO-TRANSFORM")}" + Environment.NewLine);
            void Tally(ref float hero, ref float team)
            {
                sim.KeepBenchAlive();
                if (sim.Elapsed == lastStep) return; lastStep = sim.Elapsed;
                foreach (CombatEvent e in sim.Events)
                {
                    if (e.Kind != CombatEventKind.Hit || e.Faction != Faction.Enemy) continue;
                    team += e.Value;
                    if (e.Cause.SourceUnitId == sim.Champion.Id) hero += e.Value;
                }
            }
        }
        /// <summary>Every hero's painted tokens laid out on a dark card for review.</summary>
        private IEnumerator TokenSheet()
        {
            Camera cam = null; foreach (var candidate in Camera.allCameras) if (candidate.orthographic && (cam == null || candidate.orthographicSize < cam.orthographicSize)) cam = candidate;
            if (cam == null) yield break;
            var card = new GameObject("Token review card"); Vector3 c = cam.transform.position; c.z = 0;
            var backing = new GameObject("Token review backing"); backing.transform.SetParent(card.transform, false); var bg = backing.AddComponent<SpriteRenderer>(); var tex = Texture2D.whiteTexture;
            bg.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), tex.width); bg.color = new Color(.16f, .2f, .22f, 1); bg.sortingOrder = 6000;
            float h = cam.orthographicSize * 2, w = h * cam.aspect; bg.transform.position = c; bg.transform.localScale = new Vector3(w, h, 1);
            int i = 0; float cell = h / 9f;
            foreach (var style in HeroMechanicStyle.All)
            {
                int row = i / 4, col = i % 4; i++;
                Vector2 o = (Vector2)c + new Vector2(-w * .5f + cell * .7f + col * w / 4, h * .5f - cell * .6f - row * cell);
                InkToken[] kinds = { style.Token, style.Missile, InkToken.Seal, InkToken.Glint };
                for (int k = 0; k < kinds.Length; k++)
                {
                    var go = new GameObject("token"); go.transform.SetParent(card.transform, false);
                    var r = go.AddComponent<SpriteRenderer>(); r.sprite = MechanicInk.Token(kinds[k], style.Palette, kinds[k] == InkToken.Die ? 1 + k * 2 : 0); r.sortingOrder = 6001;
                    go.transform.position = o + new Vector2(k * cell * .95f, 0); go.transform.localScale = Vector3.one * cell * .85f;
                }
            }
            yield return new WaitForSecondsRealtime(.3f);
            yield return Shot("00-token-sheet");
            Destroy(card);
        }
        private void OnDestroy() => Application.logMessageReceived -= Log;
    }

    public sealed partial class CombatSimulation
    {
        private UnitState captureTarget;
        /// <summary>Capture-only: replaces the wave with fixed harmless dummies and seeds the trigger the hero reads.</summary>
        public void PrepareMechanicCapture(RunCatalog c, int extraFriends = 0)
        {
            if (!HeroMechanicCapture.Requested && !HowToRecorder.Requested && !WebsiteCapture.Requested) throw new InvalidOperationException("Isolated mechanic capture required");
            catalog.EnemyBudget = 0; catalog.Armor = 100; Champion.Health = Champion.Definition.Health;
            var dummy = UnityEngine.Object.Instantiate(c.Enemies[0]); dummy.Speed = 0; dummy.Health = 100000; dummy.ContactDamage = 0; dummy.Behavior = EnemyBehavior.Standard;
            Vector2[] offsets = { new Vector2(2.1f, .2f), new Vector2(2.9f, -.9f), new Vector2(3.3f, .9f), new Vector2(1.5f, -1.3f) };
            captureTarget = null;
            foreach (Vector2 o in offsets)
            {
                var u = Spawn(dummy, Faction.Enemy, Champion.Position + o); u.NextAttack = u.NextContact = float.PositiveInfinity;
                if (captureTarget == null) captureTarget = u;
            }
            Vector2 p = Champion.Position;
            // a crowded brigade (capture only): extra friends standing around the hero, as in a late wave
            for (int i = 0; i < extraFriends && i < c.Companions.Length; i++)
            {
                float a = i * 137.5f * Mathf.Deg2Rad, d = 1.1f + (i % 4) * .45f;
                var friend = Spawn(c.Companions[(i * 5) % c.Companions.Length], Faction.Companion, p + new Vector2(Mathf.Cos(a) * d - .6f, Mathf.Sin(a) * d * .8f));
                friend.Position = ClampPosition(friend.Position, friend.Definition.Radius); spatial.Move(friend);
            }
            switch (IdentityId)
            {
                case "Postmaster": foreach (UnitState u in StableUnits) if (u.Faction == Faction.Enemy && u.Alive) IdentityMarkTarget(u); break;
                case "LanternWeaver": foreach (UnitState u in StableUnits) if (u.Faction == Faction.Enemy && u.Alive) IdentityMarkTarget(u); identityCounter = 1; break;
                case "CometRunner": case "CitrusMarshal":
                    identityTrail.Clear(); identityTrail.Add(p + new Vector2(-1.4f, -1.2f)); identityTrail.Add(p + new Vector2(-.3f, -.2f)); identityTrail.Add(p + new Vector2(1.2f, .5f)); identityTrail.Add(p + new Vector2(2.6f, .1f)); break;
                case "MirrorKeeper":
                    identityReflections.Clear();
                    foreach (Vector2 d in new[] { new Vector2(1, .2f), new Vector2(.8f, -.5f), new Vector2(.7f, .7f) }) identityReflections.Add(new IdentityReflection { Direction = d.normalized, Element = DamageElement.Arcane });
                    break;
                case "PetalCitadel": catalog.HeroPetals = new[] { 2, 1, 1, 1 }; break;
                case "TreasureAcorn": identityBounty = captureTarget; break;
                case "MoonProwler": identityBounty = captureTarget; identityGhost = (captureTarget.Position - p).normalized; break;
                case "RainArchivist": identityPages.Clear(); identityPages.Add(0); identityPages.Add(2); identityPages.Add(3); break;
                case "HourglassStag": identityBank = IdentityPower * 1.6f; identityBankAt = 0; break;
                case "GlassDice": identityFace = 1 + (IdentityApproach * 2 + 2) % 6; break;
            }
        }
        /// <summary>Capture-only damage bench: the wave's own enemies gone, N still targets of huge health inside the hero's weapon
        /// reach (one, or a cluster), the hero untouchable.</summary>
        public void PrepareDpsBench(RunCatalog c, int count)
        {
            if (!HeroMechanicCapture.Requested && !WebsiteCapture.Requested) throw new InvalidOperationException("Isolated mechanic capture required");
            catalog.EnemyBudget = 0; catalog.Armor = 100; Champion.Health = Champion.Definition.Health;
            benchTargets.Clear();
            foreach (UnitState u in StableUnits) if (u.Faction == Faction.Enemy) { u.Health = 0; u.Alive = false; }
            var dummy = UnityEngine.Object.Instantiate(c.Enemies[0]); dummy.Speed = 0; dummy.Health = 1000000; dummy.ContactDamage = 0; dummy.Behavior = EnemyBehavior.Standard;
            float reach = Mathf.Clamp(Champion.Definition.Weapon.Range * .7f, .9f, 2.4f);
            Vector2 centre = Champion.Position + Vector2.right * reach;
            for (int i = 0; i < count; i++)
            {
                Vector2 o = i == 0 ? Vector2.zero : new Vector2(Mathf.Cos(i * 2.4f), Mathf.Sin(i * 2.4f)) * (.45f + .2f * (i % 3));
                var u = Spawn(dummy, Faction.Enemy, ClampPosition(centre + o, .3f)); u.NextAttack = u.NextContact = float.PositiveInfinity;
                u.EntranceUntil = 0; benchTargets[u.Id] = u.Position;
            }
        }
        public void HoldGaugeForCapture() { if (HeroMechanicCapture.Requested && !HeroAwakening) { awakenMeter = 0; identityCharge = 0; } }
        private readonly Dictionary<int, Vector2> benchTargets = new Dictionary<int, Vector2>();
        /// <summary>Bench upkeep: the hero full, the targets full, every enemy that is not a bench target gone (the map, visitors
        /// and waiting enemies keep spawning).</summary>
        public void KeepBenchAlive()
        {
            if (!HeroMechanicCapture.Requested) return;
            Champion.Health = Champion.Definition.Health;
            foreach (UnitState u in StableUnits)
                if (u.Faction == Faction.Enemy && u.Alive)
                {
                    if (!benchTargets.TryGetValue(u.Id, out Vector2 home)) { u.Health = 0; u.Alive = false; }
                    else { u.Health = u.Definition.Health; u.Position = home; spatial.Move(u); } // blows knock targets away: pin them
                }
        }
        /// <summary>Capture-only (How to Play review): the wave ends as a win now.</summary>
        public void ForceVictoryForCapture() { if (HowToRecorder.Requested && Phase == RunPhase.Combat) Phase = RunPhase.Victory; }
        /// <summary>Capture-only: a toy pickup lands right at the hero's feet.</summary>
        public void DropToyForCapture(LootKind kind) { if (HowToRecorder.Requested && Phase == RunPhase.Combat) DropLoot(kind, Champion.Position, 1); }
        /// <summary>Capture-only (feel review): a body of any kind placed in the fight, so redrawn art can be filmed beside the hero.</summary>
        public UnitState AddForFeelCapture(UnitDefinition definition, Faction faction, Vector2 position) => (FeelCapture.Requested || WebsiteCapture.Requested) && definition != null ? Spawn(definition, faction, position) : null;
        /// <summary>Capture-only (death-style review): a friend's blow that takes a share of the body's health without felling it.</summary>
        public void StrikeForCapture(UnitState target, DamageElement element, float share)
        {
            if (!(FeelCapture.Requested || WebsiteCapture.Requested) || target == null || !target.Alive) return;
            ApplyWeaponDamage(target, Mathf.Min(target.Health - 1, target.Definition.Health * share), Faction.Companion, element, "", Vector2.right, null, false);
        }
        /// <summary>Capture-only (death-style review): a friend's blow that fells the body, with overkill and a chosen crit.</summary>
        public void FellForCapture(UnitState target, DamageElement element, bool critical, float overkill)
        {
            if (!(FeelCapture.Requested || WebsiteCapture.Requested) || target == null || !target.Alive) return;
            ApplyWeaponDamage(target, target.Health * (1 + overkill) + 1, Faction.Companion, element, "", Vector2.right, null, critical);
        }
        /// <summary>Capture-only: keeps the hero's trigger topped up until the real mechanic casts.</summary>
        public void FillGaugeForCapture()
        {
            if (!HeroMechanicCapture.Requested && !HowToRecorder.Requested && !WebsiteCapture.Requested || !HeroAwakenEnabled || HeroAwakening) return;
            awakenMeter = 1;
        }
        public void NudgeMechanicCapture()
        {
            if (!HeroMechanicCapture.Requested && !WebsiteCapture.Requested || !HeroIdentityEnabled) return;
            identityAt = Mathf.Min(identityAt, Elapsed); identitySecondAt = Mathf.Min(identitySecondAt, Elapsed);
            switch (IdentityId)
            {
                case "RootOracle": case "VerdantMatron": identityAt = Elapsed - 6; break;
                case "DrowsySeer": identityCharge = 6; identityStill = 2; break;
                case "MirrorKeeper": identityStill = 3.2f; break;
                case "SoapSkater":
                    {
                        // Movement comes from the player's stick; reproduce the cast this mechanic makes on a sharp turn.
                        Vector2 heading = new Vector2(.6f, .8f); identityCharge = 3; float power = IdentityPower * .8f, size = 1.7f;
                        if (IdentityApproach == 0) IdentityHit(MorphGeometry.Line, Champion.Position - heading, Champion.Position + heading, 0, power, .65f, slow: .7f, push: .2f);
                        else if (IdentityApproach == 1) IdentityHit(MorphGeometry.Crescent, Champion.Position, Champion.Position + heading, size, power, .65f, slow: .7f, push: .25f);
                        else IdentityHit(MorphGeometry.Disc, Champion.Position + heading * (size + .6f), Champion.Position + heading * (size + 1.2f), size, power, .65f, slow: .7f, push: .25f);
                        identityCharge = 0; IdentityCast(); break;
                    }
                case "ThornTreasurer":
                    {
                        // Injury-triggered; the capture dummies never hit, so answer one ordinary injury as the mechanic does.
                        var target = IdentityTarget(Champion.Position, 6); if (target != null && identityCharge >= 1) { identityCharge--; IdentityPattern(target, IdentityPower * .8f); IdentityCast(); }
                        else identityCharge = 4; break;
                    }
                case "MoonProwler":
                    identityCharge = 5;
                    if (captureTarget != null) { Champion.Position = Champion.PreviousPosition = captureTarget.Position - identityGhost * 1.2f; spatial.Move(Champion); }
                    break;
                case "HourglassStag": if (identityBank <= 0) { identityBank = IdentityPower * 1.6f; identityBankAt = 0; } break;
                case "ArchiveCurator": if (identitySpecimens.Count == 0 && captureTarget != null) { identitySpecimens.Add(captureTarget.Definition); identityCharge = 1; } break;
                case "PetalCitadel": catalog.HeroPetals = new[] { 2, 1, 1, 1 }; identityCharge = 4; break;
                case "CometRunner": case "CitrusMarshal": identityCharge = HeroIdentityRequired; break;
                case "Postmaster": case "LanternWeaver":
                    if (identityMarks.Count == 0) foreach (UnitState u in StableUnits) if (u.Faction == Faction.Enemy && u.Alive) IdentityMarkTarget(u);
                    if (IdentityId == "LanternWeaver") identityCounter = 1; else identityCharge = 3; break;
                case "TreasureAcorn": if (identityBounty == null) identityBounty = captureTarget; identityCharge = 3; break;
                case "RainArchivist": if (identityPages.Count < 3) { identityPages.Clear(); identityPages.Add(0); identityPages.Add(2); identityPages.Add(3); } identityCharge = 3; break;
                case "SupperMortar": case "ChinaMarionette": case "ButtonCavalier": identityCharge = Mathf.Max(identityCharge, HeroIdentityRequired); break;
                default: identityCharge = HeroIdentityRequired; break;
            }
        }
    }
}
