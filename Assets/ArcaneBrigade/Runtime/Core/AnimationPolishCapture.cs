using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed partial class AnimationPolishCapture : MonoBehaviour
    {
        private readonly List<string> checks = new List<string>();
        private readonly List<UnitVisual> actors = new List<UnitVisual>();
        private readonly List<UnitDefinition> definitions = new List<UnitDefinition>();
        private readonly List<WeaponDefinition> weapons = new List<WeaponDefinition>();
        private readonly List<Vector2> origins = new List<Vector2>();
        private string root;
        private bool reduced;
        private bool reviewing;
        private int pose;
        private float started;
        private float last;
        private float fixtureClock;
        private bool freeze;
        private GameObject stage;
        private TactileFeedback feedback;
        private DeathFeedback deaths;
        private RunCatalog catalog;
        private string caption;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            if (!RunPaths.K7Capture || Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-animation-capture") < 0 || FindAnyObjectByType<AnimationPolishCapture>() != null) return;
            var runner = new GameObject("AnimationPolishNativeCapture"); DontDestroyOnLoad(runner); runner.AddComponent<AnimationPolishCapture>();
        }
        private void Check(bool valid, string message)
        {
            checks.Add((valid ? "PASS " : "FAIL ") + message);
            if (!valid || checks.Count % 32 == 0 || message.EndsWith("native review complete", StringComparison.Ordinal)) File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), checks);
            if (!valid) { Debug.LogError(message); Application.Quit(1); }
        }
        private void Log(string message, string trace, LogType kind)
        {
            if (kind == LogType.Exception || kind == LogType.Error || kind == LogType.Assert)
            { File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), checks); File.AppendAllText(Path.Combine(root, "Native_Checks.txt"), "FAIL " + message + "\n"); Application.Quit(1); }
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D frame = ScreenCapture.CaptureScreenshotAsTexture();
            Check(frame.width == 1920 && frame.height == 1080, "Native dimensions " + name);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), frame.EncodeToPNG()); Destroy(frame);
            File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), checks);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int path = Array.IndexOf(args, "-capturePath");
            if (path < 0 || path + 1 >= args.Length) yield break;
            root = Path.GetFullPath(args[path + 1]); Directory.CreateDirectory(root);
            Application.logMessageReceived += Log; Application.runInBackground = true;
            yield return new WaitForSecondsRealtime(2.6f);
            reduced = Array.IndexOf(args, "-arcane-animation-reduced") >= 0;
            var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced });
            catalog = Resources.Load<RunCatalog>("RunCatalog");
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 271016); session.State.Wave = 9; session.State.Phase = SessionPhase.Shop; session.State.Gold = 2000;
                session.Reroll(); storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null; yield return new WaitForSecondsRealtime(3);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            Check(owner != null && owner.Session.State.Phase == SessionPhase.Shop, "Actual shop fixture opens");
            MerchantPortraitRig[] merchants = owner.GetComponentsInChildren<MerchantPortraitRig>(true);
            Check(merchants.Length > 0, "Actual merchant uses articulated portrait rig");
            yield return Shot("shop-acting");
            owner.TogglePause(); float clock = owner.PresentationClock;
            yield return new WaitForSecondsRealtime(.2f);
            Check(owner.PresentationClock == clock, "Shop acting obeys pause clock"); owner.TogglePause();
            owner.SetPresentationMoveTarget(owner.Simulation.Champion.Position + Vector2.right * 2);
            yield return new WaitForSecondsRealtime(.25f); yield return Shot("shop-player-start");
            owner.SetPresentationMoveTarget(null);
            yield return new WaitForSecondsRealtime(.3f); yield return Shot("shop-player-stop");
            if (Array.IndexOf(args, "-arcane-ensemble-capture") >= 0)
            {
                WorldShop shop = owner.GetComponentInChildren<WorldShop>();
                Check(shop != null, "Physical shop component available");
                WorldShop.Station offer = null;
                foreach (WorldShop.Station station in shop.Stations)
                    if (station.Offer != null && !station.Offer.Sold && station.Currency == ShopCurrency.Gold)
                    {
                        offer = station;
                        if (station.Offer.Kind == OfferKind.Item) break;
                    }
                Check(offer != null, "Physical shop has an affordable gold offer fixture");
                owner.SetPresentationMoveTarget(offer.Position);
                float deadline = Time.realtimeSinceStartup + 5;
                while (Vector2.Distance(owner.Simulation.Champion.Position, offer.Position) > .45f && Time.realtimeSinceStartup < deadline) yield return null;
                owner.SetPresentationMoveTarget(null); yield return new WaitForSecondsRealtime(.1f);
                Check(shop.Nearby != null && shop.Nearby.Offer != null && shop.Nearby.Offer.Token == offer.Offer.Token, "Player walks into actual offer interaction range");
                Check(shop.Interact(), "Actual physical-shop purchase commits");
                yield return new WaitForSecondsRealtime(.2f); yield return Shot("merchant-purchase-anticipation");
                bool handoffActive = false;
                foreach (ShopDisplayActor actor in owner.GetComponentsInChildren<ShopDisplayActor>()) handoffActive |= actor.PaintedHandoffActive;
                Check(handoffActive, "Actual purchase triggers painted merchant handoff");
                yield return new WaitForSecondsRealtime(.25f); yield return Shot("merchant-purchase-offer");
                yield return new WaitForSecondsRealtime(.7f); yield return Shot("merchant-purchase-settle");
            }
            owner.gameObject.SetActive(false);
            Shader.SetGlobalFloat("_DreamEnabled", 0);
            stage = new GameObject("NativeAnimationReviewStage");
            var cameraObject = new GameObject("NativeAnimationReviewCamera"); cameraObject.transform.SetParent(stage.transform, false);
            Camera camera = cameraObject.AddComponent<Camera>(); camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true; camera.orthographicSize = 5.5f; camera.depth = 99; camera.cullingMask = 1 << 30;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.065f, .06f, .095f);
            feedback = stage.AddComponent<TactileFeedback>(); feedback.Configure();
            deaths = stage.AddComponent<DeathFeedback>(); deaths.Configure(catalog.Arena.Hand);
            if (Array.IndexOf(args, "-arcane-expanded-performance-capture") >= 0)
            {
                reviewing = true; last = Time.unscaledTime;
                yield return ReviewExpandedPerformance();
                Check(true, "Expanded performance native review complete");
                Application.logMessageReceived -= Log; Application.Quit(0); yield break;
            }
            foreach (UnitDefinition hero in catalog.Champions) Add(hero, hero.Weapon, actors.Count);
            reviewing = true; last = Time.unscaledTime;
            string[] phases = { "idle", "walk", "attack", "hurt", "heal", "stop-turn" };
            for (int index = 0; index < phases.Length; index++)
            {
                pose = index; started = fixtureClock; caption = "TEN CHAMPIONS / " + phases[index].ToUpperInvariant();
                yield return new WaitForSecondsRealtime(.55f);
                Check(actors.Count == catalog.Champions.Length, "All champions bound " + phases[index]);
                yield return Shot("champions-" + phases[index]);
                if (index == 1)
                {
                    freeze = true; Vector3 previous = actors[0].BodyRenderer.transform.localPosition;
                    yield return new WaitForSecondsRealtime(.2f);
                    Check(actors[0].BodyRenderer.transform.localPosition == previous, "Same-clock actor rendering freezes"); freeze = false;
                }
            }
            string[] acting = { "curious", "cheer", "flinch", "tired" };
            for (int cue = 0; cue < acting.Length; cue++)
            {
                pose = 10 + cue; started = fixtureClock;
                caption = "TEN CHAMPIONS / PAINTED " + acting[cue].ToUpperInvariant();
                yield return new WaitForSecondsRealtime(.2f);
                foreach (UnitVisual actor in actors)
                    Check(actor.ActingArt != null && (reduced || actor.BodyRenderer.sprite == actor.ActingArt.Preview(cue, Mathf.Clamp01((fixtureClock - started) / .7f))), "Identity-specific painted " + acting[cue] + " " + actor.State.Definition.Id);
                yield return Shot("champions-" + acting[cue]);
                yield return new WaitForSecondsRealtime(.3f);
                yield return Shot("champions-" + acting[cue] + "-settle");
            }
            pose = 0; started = fixtureClock; caption = "TEN CHAMPIONS / AUTHORED DEFEAT / GROUNDED SCALE";
            foreach (UnitVisual actor in actors)
            {
                actor.PreviewActing(-1, 0); actor.State.LastHit = -100; actor.Display(fixtureClock, 1);
                deaths.Play(actor, actor.State); actor.gameObject.SetActive(false);
            }
            Check(deaths.ActiveCount == catalog.Champions.Length, "All champion defeat remnants render");
            foreach (SpriteRenderer part in deaths.GetComponentsInChildren<SpriteRenderer>())
                if (part.name == "Body") Check(part.bounds.size.y > .5f && part.bounds.size.y < 1.8f, "Defeat frame preserves world height " + part.sprite.name);
            yield return new WaitForSecondsRealtime(.15f); yield return Shot("champions-defeat-recoil");
            yield return new WaitForSecondsRealtime(.25f); yield return Shot("champions-defeat-settle");
            foreach (SpriteRenderer part in deaths.GetComponentsInChildren<SpriteRenderer>())
                if (part.name == "Body") Check(part.bounds.size.y > .65f && part.bounds.size.y < 1.8f, "Painted defeat stays readable rather than flattened " + part.sprite.name);
            deaths.Clear(); Check(deaths.ActiveCount == 0, "Defeat pool clears on reuse");
            ClearActors();
            var selected = new HashSet<WeaponMotion>();
            foreach (WeaponDefinition weapon in catalog.Weapons)
                if (weapon.Sprite != null && selected.Add(weapon.Motion)) Add(catalog.Champions[actors.Count % catalog.Champions.Length], weapon, actors.Count);
            pose = 2; started = fixtureClock; caption = "WEAPON WINDUP / RELEASE / SECONDARY SETTLE";
            yield return new WaitForSecondsRealtime(.12f); yield return Shot("weapon-windup");
            yield return new WaitForSecondsRealtime(.18f); yield return Shot("weapon-release");
            yield return new WaitForSecondsRealtime(.2f); yield return Shot("weapon-settle");
            Check(selected.Count >= 8, "Multiple actual weapon motion families rendered");
            int bladeParts = 0;
            foreach (WeaponMechanism mechanism in stage.GetComponentsInChildren<WeaponMechanism>()) bladeParts += mechanism.VisibleParts;
            int paintedMechanisms = 0;
            foreach (WeaponPaintedAction action in stage.GetComponentsInChildren<WeaponPaintedAction>()) if (action.Visible) paintedMechanisms++;
            Check(reduced ? bladeParts == 0 && paintedMechanisms == 0 : bladeParts >= 2 || paintedMechanisms > 0, "Scissor mechanisms use authored action or articulated fallback");
            ClearActors();
            foreach (WeaponDefinition weapon in catalog.Weapons)
                if (WeaponMechanism.Supports(weapon) && actors.Count < 10) Add(catalog.Champions[actors.Count % catalog.Champions.Length], weapon, actors.Count);
            pose = 2; started = fixtureClock; caption = "HINGED BLADES / CAMERA LENS / WINDING KEY / MAIL FLAG";
            yield return new WaitForSecondsRealtime(.1f); yield return Shot("mechanisms-windup");
            yield return new WaitForSecondsRealtime(.2f); yield return Shot("mechanisms-release");
            foreach (UnitVisual actor in actors)
            {
                WeaponMechanism mechanism = actor.GetComponentInChildren<WeaponMechanism>();
                WeaponPaintedAction action = actor.GetComponentInChildren<WeaponPaintedAction>();
                Check(reduced ? (mechanism == null || mechanism.VisibleParts == 0) && (action == null || !action.Visible) : mechanism != null && mechanism.VisibleParts > 0 || action != null && action.Visible, "Source-specific weapon mechanism " + actor.State.Definition.Weapon.Id);
            }
            ClearActors(); pose = 0; started = fixtureClock; caption = "MATERIAL IMPACTS / PROJECTILE EVENTS / CHEST";
            TactileAnimationLibrary art = TactileAnimationLibrary.Current;
            for (int index = 0; index < 4; index++) feedback.Emit(index == 3 ? art.Dust : art.For((BodyMaterial)index), new Vector2(-6 + index * 4, 1.2f), 2, .9f, Color.white);
            feedback.Play(new CombatEvent(CombatEventKind.ChestOpened, Vector2.down * 1.5f, Faction.Champion));
            yield return new WaitForSecondsRealtime(.2f); yield return Shot("material-and-chest");
            Check(reduced ? feedback.ActiveCount == 0 : feedback.ActiveCount > 0, "Tactile feedback respects reduced effects");
            for (int index = 0; index < 256; index++) feedback.Emit(art.Paper, Vector2.zero, .3f, .2f, Color.white);
            Check(feedback.PoolCount <= 48, "Painted feedback pool remains bounded under stress");
            feedback.Clear(); Check(feedback.ActiveCount == 0, "Feedback pool clears on reuse");
            foreach (UnitDefinition boss in catalog.Bosses) if (actors.Count < 10) Add(boss, boss.Weapon, actors.Count);
            if (NightmareAssets.Current?.Customer != null) Add(NightmareAssets.Current.Customer, NightmareAssets.Current.Customer.Weapon, actors.Count);
            if (OutsideStageAssets.Current?.Boss != null) Add(OutsideStageAssets.Current.Boss, OutsideStageAssets.Current.Boss.Weapon, actors.Count);
            for (int act = 0; act < 3; act++)
            {
                pose = 6 + act; started = fixtureClock; caption = "BOSS ACT " + (act + 1) + " / ORGAN ACTING";
                yield return new WaitForSecondsRealtime(.3f); yield return Shot("boss-act-" + act);
            }
            Check(actors.Count > 5, "Boss roster including final Customer rendered");
            ClearActors();
            foreach (UnitDefinition enemy in catalog.Enemies) if (actors.Count < 10 && enemy.Body != null) Add(enemy, enemy.Weapon, actors.Count);
            pose = 2; started = fixtureClock; caption = "ENEMY ATTACK POSES / MATERIAL PERSONALITY";
            yield return new WaitForSecondsRealtime(.14f); yield return Shot("enemy-windup");
            yield return new WaitForSecondsRealtime(.24f); yield return Shot("enemy-release");
            Check(actors.Count > 0, "Enemy roster renders with polish");
            if (Array.IndexOf(args, "-arcane-ensemble-capture") >= 0) yield return ReviewEnsemble();
            ClearActors(); reviewing = false; stage.SetActive(false);
            SceneManager.LoadScene("RunFrontend"); yield return null;
            yield return new WaitForSecondsRealtime(.25f);
            RunMenu menu = FindAnyObjectByType<RunMenu>(); PlayerProfile profile = storage.LoadProfile();
            foreach (UnitDefinition companion in catalog.Companions) profile.SeenUnits.Add(companion.Id);
            string before = JsonUtility.ToJson(profile);
            menu.ConfigureK7Diagnostics(catalog, profile); menu.ShowPage(RunMenu.Page.Museum); menu.Museum.SelectTab(2);
            yield return null;
            int exhibit = Array.FindIndex(catalog.Companions, companion => CharacterActingLibrary.Current.Find(companion.Id) != null);
            Check(exhibit >= 0, "Collection has identity-matched companion acting");
            menu.Museum.SelectExhibitForDiagnostics(exhibit); menu.Museum.SetPreview(3, 0);
            yield return new WaitForSecondsRealtime(.25f); yield return Shot("collection-acting-controls");
            Check(JsonUtility.ToJson(profile) == before, "Collection acting preview does not mutate progress");
            menu.ShowPage(RunMenu.Page.Landing);
            yield return null;
            Check(FindObjectsByType<WeaponMechanism>().Length == 0, "Weapon mechanisms cleaned up on scene change");
            if (Array.IndexOf(args, "-arcane-animation-stress") >= 0) yield return MeasureCrowd(storage);
            Check(true, "Animation polish native review complete");
            Application.logMessageReceived -= Log; Application.Quit(0);
        }
        private void Add(UnitDefinition source, WeaponDefinition sourceWeapon, int index, Faction? factionOverride = null)
        {
            UnitDefinition definition = Instantiate(source); definitions.Add(definition);
            definition.Weapon = Instantiate(sourceWeapon); weapons.Add(definition.Weapon);
            definition.BodyHeight = definition.IsBoss ? 1.45f : 1.1f;
            definition.Weapon.VisualWidth = Mathf.Min(1.4f, definition.Weapon.VisualWidth);
            Vector2 origin = new Vector2(-7.6f + index % 5 * 3.8f, 2.5f - index / 5 * 3.2f);
            var child = new GameObject("NativeReview-" + source.Id); child.transform.SetParent(stage.transform, false);
            UnitVisual visual = child.AddComponent<UnitVisual>(); visual.CreateHierarchy(catalog.Arena.Hand);
            Faction faction = factionOverride ?? (definition.IsBoss || Array.IndexOf(catalog.Enemies, source) >= 0 ? Faction.Enemy : Faction.Champion);
            visual.Bind(new UnitState { Id = index + 100, Definition = definition, Health = definition.Health, Faction = faction, Position = origin, PreviousPosition = origin }, catalog.Arena);
            visual.ReducedMotion = reduced; actors.Add(visual); origins.Add(origin);
            var label = new GameObject("ReviewCaption"); label.transform.SetParent(child.transform, false); label.transform.localPosition = new Vector3(0, -.95f, 0);
            TextMesh text = label.AddComponent<TextMesh>(); text.text = source.Id + "\n" + sourceWeapon.Id; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.anchor = TextAnchor.UpperCenter; text.fontSize = 30; text.characterSize = .06f; text.color = new Color(.87f, .84f, .77f);
            label.GetComponent<MeshRenderer>().sortingOrder = 3000;
        }
        private void Update()
        {
            if (!reviewing) return;
            float delta = Mathf.Clamp(Time.unscaledTime - last, 0, .05f); last = Time.unscaledTime;
            if (freeze) return;
            fixtureClock += delta;
            float age = fixtureClock - started;
            foreach (UnitVisual visual in actors)
            {
                int index = actors.IndexOf(visual); UnitState state = visual.State;
                state.PreviousPosition = state.Position;
                bool walking = pose == 1 || pose == 5 && age < .25f;
                state.Velocity = walking ? Vector2.right * 3 : Vector2.zero;
                state.Position = origins[index] + (walking && !reduced ? Vector2.right * Mathf.Sin(age * 2) * .35f : Vector2.zero);
                state.Facing = pose == 5 && age > .3f ? Vector2.left : Vector2.right;
                state.AttackStarted = pose == 2 ? started : -100;
                state.AttackDirection = Vector2.right; state.PendingAttack = pose == 2 && age < state.Definition.Weapon.Windup;
                state.Health = state.Definition.Health * (pose >= 6 && pose <= 8 ? pose == 6 ? .95f : pose == 7 ? .5f : .2f : pose == 3 ? .7f : 1);
                state.LastHit = pose == 3 ? started + .5f : -100; state.HitDirection = Vector2.right;
                if (state.Definition.IsBoss) { state.ActiveBossOrgan = index % 3; state.TelegraphUntil = pose == 2 ? started + .85f : 0; state.BossOrganReleaseAt = started + .85f; }
                visual.PreviewActing(pose >= 10 && pose <= 13 ? pose - 10 : -1, Mathf.Clamp01(age / .7f));
                visual.Display(fixtureClock, 1);
            }
            feedback.Advance(freeze ? 0 : delta);
            deaths.Advance(delta);
            foreach (Renderer renderer in stage.GetComponentsInChildren<Renderer>(true)) renderer.gameObject.layer = 30;
            shadowRenderers.Clear();
            foreach (GroundShadow shadow in stage.GetComponentsInChildren<GroundShadow>(true)) shadow.AddRenderers(shadowRenderers);
            foreach (Renderer renderer in shadowRenderers) renderer.gameObject.layer = 30;
        }
        private void OnGUI()
        {
            if (!reviewing) return;
            GUIStyle title = new GUIStyle(GUI.skin.label) { fontSize = 24, alignment = TextAnchor.UpperCenter, normal = { textColor = new Color(.94f, .89f, .77f) } };
            LocalizedGUI.Label(new Rect(0, 20, Screen.width, 60), caption + (reduced ? " / REDUCED" : " / STANDARD"), title);
        }
        private void ClearActors()
        {
            foreach (UnitVisual actor in actors) { actor.gameObject.SetActive(false); Destroy(actor.gameObject); }
            actors.Clear(); origins.Clear();
            foreach (UnitDefinition definition in definitions) Destroy(definition); definitions.Clear();
            foreach (WeaponDefinition weapon in weapons) Destroy(weapon); weapons.Clear();
        }
        private void OnDestroy() { Application.logMessageReceived -= Log; }
    }
}
