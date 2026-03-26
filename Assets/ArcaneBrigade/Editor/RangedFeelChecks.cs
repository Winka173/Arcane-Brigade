using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Our ranged weapons' feel (19/10/2026, V1, V10/R16, R1-R8): heft orders recoil, the pointy-shot rule, stuck shots
    /// capped per enemy and cleared with it, material impact cues for all six materials with a swarm budget, pierce links and
    /// rising pitch, bounce chevrons, casings capped per frame, charge-ups, a long-range critical's held beat, a home run's
    /// glint once per body, and the Reduced Motion / Effects / Audio settings honoured.</summary>
    public static class RangedFeelChecks
    {
        [MenuItem("Arcane Brigade/Checks/Ranged feel")]
        public static void Run()
        {
            int passed = 0;
            void Require(bool condition, string caption)
            {
                if (!condition) throw new InvalidOperationException("RangedFeel: " + caption);
                passed++; Debug.Log("PASS RangedFeel: " + caption);
            }
            var temporary = new List<UnityEngine.Object>();
            PresentationSettings.Apply(new PlayerProfile());
            RangedFeel.Reset(); MaterialAudio.Clear();
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Require(catalog != null, "Run catalog available");
            WeaponDefinition Find(string id) => catalog.Weapons.FirstOrDefault(w => w != null && w.Id == id);
            WeaponDefinition Make(string id, WeaponMotion motion, float damage, float cooldown, float radius = .055f)
            {
                var w = ScriptableObject.CreateInstance<WeaponDefinition>(); temporary.Add(w);
                w.Id = id; w.Kind = WeaponKind.Projectile; w.Motion = motion; w.Damage = damage; w.Cooldown = cooldown; w.ProjectileRadius = radius;
                w.Windup = .2f; w.Recovery = .3f; w.RecoilDistance = .09f; w.ProjectileVisualWidth = .32f;
                return w;
            }
            try
            {
                // ---- R1 heft and recoil
                WeaponDefinition gumball = Make("TestGumballGatling", WeaponMotion.Recoil, 8, .32f), cork = Make("TestCorkRifle", WeaponMotion.Recoil, 8.5f, 1.55f);
                WeaponDefinition cannon = Make("TestShellCannon", WeaponMotion.Recoil, 13, .9f, .085f), blunder = Make("TestConfettiBlunderbuss", WeaponMotion.Recoil, 32, 1.25f);
                Require(RangedFeel.Heft(gumball) < RangedFeel.Heft(cork) && RangedFeel.Heft(cork) < RangedFeel.Heft(cannon) && RangedFeel.Heft(cannon) < RangedFeel.Heft(blunder),
                    $"Heft orders a gumball ({RangedFeel.Heft(gumball):0.00}) < cork ({RangedFeel.Heft(cork):0.00}) < cannon ({RangedFeel.Heft(cannon):0.00}) < blunderbuss ({RangedFeel.Heft(blunder):0.00})");
                Require(RangedFeel.RecoilScale(cork) < RangedFeel.RecoilScale(cannon) && RangedFeel.TiltDegrees(cork) < RangedFeel.TiltDegrees(cannon), "Recoil and barrel climb grow with heft");
                float release = .2f + .3f * .18f;
                K8Motion.Pose light = K8Motion.Attack(cork, release, .2f, false), heavy = K8Motion.Attack(cannon, release, .2f, false);
                Require(heavy.Recoil > light.Recoil && heavy.Angle > light.Angle, "A heavier shot kicks the weapon back further and tilts its barrel higher");
                K8Motion.Pose before = K8Motion.Attack(blunder, .19999f, .2f, false), after = K8Motion.Attack(blunder, .20001f, .2f, false);
                Require(Mathf.Abs(before.Recoil - after.Recoil) < .001f && Mathf.Abs(before.Angle - after.Angle) < .1f, "The kick stays continuous at the moment of release");
                K8Motion.Pose still = K8Motion.Attack(blunder, release, .2f, true);
                Require(still.Recoil == 0 && still.Angle == 0, "Reduced motion: no kick, no climb");
                Require(RangedFeel.BodyKick(blunder, .23f, .2f) > RangedFeel.BodyKick(gumball, .23f, .2f) && RangedFeel.BodyKick(blunder, .1f, .2f) == 0 && RangedFeel.BodyKick(blunder, .9f, .2f) == 0, "The holder is shoved back on release only, more by a heavy shot");
                WeaponDefinition sword = Make("TestSword", WeaponMotion.Slash, 30, 1.5f); sword.Kind = WeaponKind.Melee;
                Require(RangedFeel.Heft(sword) == 0 && !RangedFeel.IsPointy(sword) && RangedFeel.FlourishFor(sword) == RangedFeel.Flourish.None, "Melee weapons have no ranged heft, stuck shots or muzzle flourish");

                // ---- R5 pointy rule, R3 guns
                string[] pointy = { "ButtonBow", "CrescentBow", "LeafCrossbow", "KunaiFan", "DartCompass", "NeedleSinger", "SonarHarpoon" };
                string[] blunt = { "InkPistol", "BubbleWand", "ShellCannon", "GumballGatling", "SpinningTop", "CherryFuseBomb" };
                int foundPointy = 0, foundBlunt = 0;
                foreach (string id in pointy) { WeaponDefinition w = Find(id); if (w == null) continue; foundPointy++; Require(RangedFeel.IsPointy(w), id + " shots are pointy and stay stuck"); }
                foreach (string id in blunt) { WeaponDefinition w = Find(id); if (w == null) continue; foundBlunt++; Require(!RangedFeel.IsPointy(w), id + " shots are not pointy"); }
                Require(foundPointy >= 5 && foundBlunt >= 5, "The pointy rule is checked on the real arsenal");
                Require(RangedFeel.IsPointy(Make("TestQuillThrower", WeaponMotion.Recoil, 9, 1)) && !RangedFeel.IsPointy(Make("TestPebbleSling", WeaponMotion.Recoil, 9, 1)), "A quill is pointy; a pebble is not");
                Require(catalog.Weapons.Where(w => w != null).All(w => RangedFeel.IsPointy(w) == ImpactRules.Pointed(w)), "The stuck-shot rule is the simulation's own pointed rule (ImpactRules.Pointed)");
                string[] guns = { "InkPistol", "ShellCannon", "GumballGatling", "CorkRifle", "SaucerBlaster", "CuckooCannon" };
                string[] thrown = { "KunaiFan", "JugglingPins", "BubbleWand", "PaperShuriken", "HearthBrick", "TinFish" };
                foreach (string id in guns) { WeaponDefinition w = Find(id); if (w != null) Require(RangedFeel.IsGunLike(w), id + " is a gun (ejects a casing)"); }
                foreach (string id in thrown) { WeaponDefinition w = Find(id); if (w != null) Require(!RangedFeel.IsGunLike(w), id + " is thrown, not a gun"); }

                // ---- R2 flourishes: every ranged weapon has its own (or an Arsenal moment already)
                var kinds = new HashSet<RangedFeel.Flourish>(); int ranged = 0;
                foreach (WeaponDefinition w in catalog.Weapons)
                {
                    if (w == null || w.Kind != WeaponKind.Projectile) continue;
                    ranged++;
                    RangedFeel.Flourish look = RangedFeel.FlourishFor(w);
                    Require(look != RangedFeel.Flourish.None || RangedFeel.HasArsenalFireMoment(w.Arsenal), w.Id + " has a muzzle flourish (" + look + ")");
                    if (look != RangedFeel.Flourish.None) kinds.Add(look);
                }
                Require(ranged > 20 && kinds.Count >= 9, "Ranged weapons spread over " + kinds.Count + " different flourishes");
                if (Find("ButtonBow") != null) Require(RangedFeel.FlourishFor(Find("ButtonBow")) == RangedFeel.Flourish.Twang, "A bow twangs");
                if (Find("BobaStraw") != null) Require(RangedFeel.FlourishFor(Find("BobaStraw")) == RangedFeel.Flourish.Steam, "The boba straw puffs steam");
                if (Find("InkPistol") != null) Require(RangedFeel.FlourishFor(Find("InkPistol")) == RangedFeel.Flourish.Smoke, "A pistol smokes");
                if (Find("CorkRifle") != null) Require(RangedFeel.FlourishFor(Find("CorkRifle")) == RangedFeel.Flourish.None, "The cork rifle keeps its own Arsenal cork pop (no second flourish)");

                // ---- V10/R16 material cues
                int cues = 0;
                foreach (BodyMaterial m in Enum.GetValues(typeof(BodyMaterial)))
                    foreach (MaterialAudio.Impact k in Enum.GetValues(typeof(MaterialAudio.Impact)))
                        if (ArcaneCueLibrary.Get(MaterialAudio.Cue(m, k)) != null && MaterialAudio.Cue(m, k).Contains(m.ToString().ToLowerInvariant())) cues++;
                Require(cues == 6 * 4, "All six materials have a library cue for hits, splats, bumps and landings (" + cues + "/24)");
                var metal = ScriptableObject.CreateInstance<UnitDefinition>(); metal.Material = BodyMaterial.Metal; metal.Health = 100; temporary.Add(metal);
                var wax = ScriptableObject.CreateInstance<UnitDefinition>(); wax.Material = BodyMaterial.Wax; wax.Health = 100; temporary.Add(wax);
                var metalUnit = new UnitState { Id = 9001, Definition = metal, Faction = Faction.Enemy };
                var waxUnit = new UnitState { Id = 9002, Definition = wax, Faction = Faction.Enemy };
                string splat = MaterialAudio.CueFor(new CombatEvent(CombatEventKind.WallSplat, Vector2.zero, Faction.Companion, 3, Vector2.right, subjectId: metalUnit.Id, subject: metalUnit), out _, out float splatPitch);
                Require(splat == "cmb.hit.metal" && splatPitch < 1, "A shove into the edge sounds the enemy's metal, pitched down");
                MaterialAudio.Observe(new CombatEvent(CombatEventKind.BodyLaunched, Vector2.zero, Faction.Enemy, 1, Vector2.right, subjectId: waxUnit.Id, subject: waxUnit, cause: new CombatCause(501, 3)));
                Require(MaterialAudio.CueFor(new CombatEvent(CombatEventKind.BodyImpact, Vector2.zero, Faction.Companion, 1, Vector2.right, cause: new CombatCause(501, 3, "flung-body", 1)), out _, out _) == "cmb.hit.wax", "A flung wax body bowling into another sounds wax");
                Require(MaterialAudio.CueFor(new CombatEvent(CombatEventKind.Detonation, Vector2.zero, Faction.Companion, 1, Vector2.right, cause: new CombatCause(501, 3, "flung-body", 1)), out _, out _) == "enemy.die.wax", "A flung wax body lands with its material's burst");
                Require(MaterialAudio.CueFor(new CombatEvent(CombatEventKind.BodyImpact, Vector2.zero, Faction.Companion, 1, Vector2.right, cause: new CombatCause(502, 3, "flung-body", 1)), out _, out _) == null, "An unknown body falls back to the usual knock");
                MaterialAudio.Clear();
                Require(MaterialAudio.Allow(10) && MaterialAudio.Allow(10.01f) && MaterialAudio.Allow(10.02f) && !MaterialAudio.Allow(10.03f), "A swarm gets at most three material impacts per tenth of a second");
                Require(MaterialAudio.Allow(10.12f), "The budget refills after the window");
                PresentationSettings.Apply(new PlayerProfile { ReducedAudio = true }); MaterialAudio.Clear();
                Require(MaterialAudio.Allow(20) && !MaterialAudio.Allow(20.02f), "Reduced audio: one material impact per window");
                PresentationSettings.Apply(new PlayerProfile());

                // ---- R6 pierce pitch
                var pierceCue = new CombatEvent(CombatEventKind.ProjectilePierce, Vector2.zero, Faction.Companion, cause: new CombatCause(900, 3));
                float p0 = RangedFeel.PiercePitch(pierceCue, 1), p1 = RangedFeel.PiercePitch(pierceCue, 1.1f), p2 = RangedFeel.PiercePitch(pierceCue, 1.2f);
                Require(Mathf.Approximately(p0, 1) && p1 > p0 && p2 > p1 && RangedFeel.PitchForStep(30) <= 1.9f, "Each pierce of one shot sounds a step higher (capped)");
                Require(Mathf.Approximately(RangedFeel.PiercePitch(pierceCue, 5f), 1), "A new shot starts the scale again");

                // ---- the arena fixtures
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 7707);
                    CheckpointCatalog combat = session.PrepareCombat();
                    combat.EnemyBudget = 0; combat.Boss = null; combat.WaveDuration = 1000; combat.Procs = new ActiveProc[0]; combat.EnableArenaToys = false; combat.ChestInterval = 1000;
                    var sim = new CombatSimulation(combat); sim.Start(7707);
                    UnitDefinition enemyDef = UnityEngine.Object.Instantiate(catalog.Enemies[0]); temporary.Add(enemyDef); enemyDef.Health = 100000;
                    UnitDefinition allyDef = UnityEngine.Object.Instantiate(catalog.Companions[0]); temporary.Add(allyDef);
                    WeaponDefinition bow = Find("ButtonBow") ?? Make("TestButtonBow", WeaponMotion.Bow, 9, .9f);
                    WeaponDefinition slowBow = Make("TestLongBow", WeaponMotion.Bow, 20, 1.5f); slowBow.ProjectileSprite = bow.ProjectileSprite;
                    WeaponDefinition pistol = Find("InkPistol") ?? Make("TestInkPistol", WeaponMotion.Recoil, 13, 1.13f);
                    allyDef.Weapon = slowBow;
                    UnitState ally = sim.AddFixture(allyDef, Faction.Companion, new Vector2(-6, 0));
                    UnitState enemy = sim.AddFixture(enemyDef, Faction.Enemy, new Vector2(1, 0));
                    UnitState second = sim.AddFixture(enemyDef, Faction.Enemy, new Vector2(2, 1));
                    UnitState third = sim.AddFixture(enemyDef, Faction.Enemy, new Vector2(-1, -1));
                    CombatEvent Hit(UnitState target, WeaponDefinition w, int root, bool critical = false, float amount = 4)
                        => new CombatEvent(CombatEventKind.Hit, target.Position, Faction.Enemy, amount, Vector2.right, w, critical, subjectId: target.Id, subject: target, cause: new CombatCause(root, ally.Id));
                    CombatEvent Fire(WeaponDefinition w, Faction side = Faction.Champion) => new CombatEvent(CombatEventKind.Fire, ally.Position, side, 0, Vector2.right, w, subjectId: ally.Id, subject: ally, cause: new CombatCause(4000, ally.Id));
                    var host = new GameObject("RangedFeelFixture"); temporary.Add(host);
                    RangedFeelVisual visual = host.AddComponent<RangedFeelVisual>();
                    visual.Display(sim, 1f);

                    // R5 stuck shots
                    for (int i = 0; i < 8; i++) visual.Observe(Hit(enemy, bow, 100 + i), 1f);
                    Require(visual.StuckOn(enemy.Id) == RangedFeel.StuckPerEnemy, "At most five pointy shots stay in one enemy (" + visual.StuckOn(enemy.Id) + ")");
                    visual.Observe(Hit(enemy, pistol, 120), 1f);
                    Require(visual.StuckOn(enemy.Id) == RangedFeel.StuckPerEnemy, "A blunt shot never sticks");
                    visual.Display(sim, 1.05f);
                    Require(host.GetComponentsInChildren<SpriteRenderer>().Count(r => r.enabled && r.name == "StuckShot") == RangedFeel.StuckPerEnemy, "Stuck shots are drawn with the shot's own picture");
                    visual.Observe(Hit(second, bow, 130), 1.05f);
                    enemy.Alive = false; visual.Display(sim, 1.1f);
                    Require(visual.StuckOn(enemy.Id) == 0 && visual.StuckOn(second.Id) == 1, "Stuck shots fall with their enemy, others stay");
                    Require(host.GetComponentsInChildren<SpriteRenderer>().Count(r => r.enabled && r.name == "StuckShot") == 1, "A fallen enemy's shots are no longer drawn");
                    visual.Display(null, 1.15f);
                    Require(visual.StuckCount == 0 && host.GetComponentsInChildren<SpriteRenderer>().Count(r => r.enabled && r.name == "StuckShot") == 0, "Everything clears when the fight ends (shop)");
                    visual.Display(sim, 1.2f);
                    PresentationSettings.Apply(new PlayerProfile { ReducedEffects = true });
                    for (int i = 0; i < 6; i++) visual.Observe(Hit(third, bow, 140 + i), 1.2f);
                    Require(visual.StuckOn(third.Id) == RangedFeel.ReducedStuckPerEnemy, "Reduced effects keep fewer stuck shots");
                    PresentationSettings.Apply(new PlayerProfile());

                    // R2 flourish, R3 casings, R4 thup
                    int thups = visual.Thups;
                    for (int i = 0; i < 10; i++) visual.Observe(Fire(pistol), 1.3f);
                    Require(visual.Casings == RangedFeelVisual.CasingsPerFrame, "Casings are capped per frame (" + visual.Casings + ")");
                    Require(visual.Flourishes >= 1, "A pistol's shot leaves its own flourish at the muzzle");
                    visual.Display(sim, 1.32f); visual.Display(sim, 1.36f);
                    Require(visual.Casings == RangedFeelVisual.CasingsPerFrame, "Casings drop and stay a moment");
                    visual.Display(sim, 1.3f + RangedFeelVisual.CasingLife + .05f);
                    Require(visual.Casings == 0 && visual.Flourishes == 0, "Casings and flourishes fade (about 0.6 s and 0.3 s)");
                    Require(visual.Thups > thups, "A slow weapon lets go with a soft thup");
                    PresentationSettings.Apply(new PlayerProfile { ReducedMotion = true, ReducedAudio = true });
                    thups = visual.Thups;
                    visual.Observe(Fire(pistol), 2f);
                    Require(visual.Casings == 0, "Reduced motion: no casings");
                    Require(visual.Thups == thups, "Reduced audio: no thup");
                    PresentationSettings.Apply(new PlayerProfile());
                    int flourishes = visual.Flourishes;
                    WeaponDefinition corkRifle = Find("CorkRifle");
                    if (corkRifle != null) { visual.Observe(Fire(corkRifle), 2f); Require(visual.Flourishes == flourishes, "An Arsenal weapon with its own moment gets no second flourish"); }
                    visual.Observe(Fire(slowBow), 2f);
                    Require(visual.Flourishes == flourishes + 1, "A bow's shot twangs");

                    // R4 charge
                    ally.PendingAttack = true; ally.AttackWindup = .3f; ally.AttackStarted = sim.Elapsed - .1f; ally.AttackDirection = Vector2.right;
                    visual.Display(sim, 2.1f);
                    Require(visual.Charges == 1, "A slow weapon gathers light at its muzzle through its wind-up");
                    ally.AttackStarted = sim.Elapsed - .5f; visual.Display(sim, 2.12f);
                    Require(visual.Charges == 0, "The charge ends with the wind-up");
                    allyDef.Weapon = bow; ally.AttackStarted = sim.Elapsed - .05f; ally.AttackWindup = .15f; visual.Display(sim, 2.13f);
                    Require(RangedFeel.IsSlow(bow) || visual.Charges == 0, "A quick weapon does not charge");
                    Require(RangedFeel.BowDraw(slowBow) > 1 && RangedFeel.BowDraw(Make("TestQuickBow", WeaponMotion.Bow, 5, .6f)) == 1, "A slow bow draws further than a quick one");
                    allyDef.Weapon = slowBow; ally.PendingAttack = false;

                    // R6 pierce links
                    for (int i = 0; i < 3; i++) visual.Observe(new CombatEvent(CombatEventKind.ProjectilePierce, new Vector2(i, 0), Faction.Companion, weapon: bow, cause: new CombatCause(777, ally.Id)), 2.2f + i * .05f);
                    Require(visual.Links == 2, "Successive pierces of one shot are joined by inked lines (" + visual.Links + ")");
                    visual.Observe(new CombatEvent(CombatEventKind.ProjectilePierce, new Vector2(0, 3), Faction.Enemy, cause: new CombatCause(778, 1)), 2.3f);
                    Require(visual.Links == 2, "An enemy shot's pierce draws nothing of ours");

                    // R7 bounce chevron
                    ProjectileState shot = sim.ProjectileFixtureShot(new Vector2(3, 3), new Vector2(0, 5)); shot.Cause = new CombatCause(888, ally.Id);
                    visual.Observe(new CombatEvent(CombatEventKind.ProjectileBounce, new Vector2(3, 3), Faction.Companion, cause: new CombatCause(888, ally.Id)), 2.3f);
                    Require(visual.Chevrons == 1, "A bounce leaves a chevron showing the shot's new way");
                    visual.Display(sim, 2.32f);
                    visual.Display(sim, 3f);
                    Require(visual.Links == 0 && visual.Chevrons == 0, "Pierce lines and chevrons fade");

                    // R8 long-range critical
                    CombatEvent far = Hit(second, bow, 300, true, 1);
                    Require(RangedFeel.LongShot(far, ally.Position) && !RangedFeel.LongShot(far, second.Position + Vector2.left) && !RangedFeel.LongShot(Hit(second, bow, 301, false, 1), ally.Position), "Only a critical from far away is a long shot");
                    RangedFeel.Simulation = sim;
                    Require(RangedFeel.LongShot(far), "The shooter is found from the simulation");
                    var feel = new CombatFeel(); feel.Play(new[] { far }, new Dictionary<int, UnitVisual>(), 1, 0);
                    Require(feel.Frozen, "A long-range critical holds the screen for a blink");
                    var near = new CombatFeel(); near.Play(new[] { new CombatEvent(CombatEventKind.Hit, second.Position, Faction.Enemy, 1, Vector2.right, bow, true, subjectId: second.Id, subject: second, cause: new CombatCause(302, second.Id)) }, new Dictionary<int, UnitVisual>(), 1, 0);
                    Require(!near.Frozen, "A close critical does not");
                    PresentationSettings.Apply(new PlayerProfile { ReducedMotion = true });
                    var calm = new CombatFeel(); calm.Play(new[] { far }, new Dictionary<int, UnitVisual>(), 1, 0);
                    Require(!calm.Frozen, "Reduced motion: no hold");
                    Require(RangedFeel.TipBack(.05f) > 0 && RangedFeel.TipBack(.35f) == 0, "The struck target tips back briefly");
                    PresentationSettings.Apply(new PlayerProfile());

                    // V1 home run
                    var cameraHost = new GameObject("RangedFeelCamera"); temporary.Add(cameraHost);
                    Camera camera = cameraHost.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 5; camera.aspect = 16f / 9f;
                    cameraHost.transform.position = new Vector3(0, 0, -10);
                    visual.ViewOverride = camera;
                    var flung = (List<FlungBody>)typeof(CombatSimulation).GetField("flung", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sim);
                    flung.Add(new FlungBody { Definition = enemyDef, UnitId = 4242, Position = new Vector2(30, 0), Velocity = new Vector2(8, 0), Started = sim.Elapsed, Until = sim.Elapsed + 1, Weight = 1 });
                    flung.Add(new FlungBody { Definition = enemyDef, UnitId = 4243, Position = new Vector2(0, -1), Velocity = new Vector2(1, 0), Started = sim.Elapsed, Until = sim.Elapsed + 1, Weight = .5f });
                    int tings = visual.Tings;
                    visual.Display(sim, 4f);
                    Require(visual.Glints == 1 && visual.Tings == tings + 1, "A body flung out of view leaves a star glint at the edge and a ting");
                    visual.Display(sim, 4.1f);
                    Require(visual.Glints == 1 && visual.Tings == tings + 1, "Once per body");
                    visual.Display(sim, 4f + RangedFeelVisual.GlintLife + .05f);
                    Require(visual.Glints == 0, "The glint lasts about half a second");
                    flung.Clear();

                    // the lead's impact physics drawn: a shot-flung body skims low, a pinned one hangs, a pinned enemy strains, STRIKE!
                    var low = new FlungBody { Definition = enemyDef, UnitId = 5001, Position = Vector2.zero, Velocity = Vector2.right, Started = sim.Elapsed - .3f, Until = sim.Elapsed + .3f, Weight = 1, Low = true };
                    var high = new FlungBody { Definition = enemyDef, UnitId = 5002, Position = Vector2.zero, Velocity = Vector2.right, Started = sim.Elapsed - .3f, Until = sim.Elapsed + .3f, Weight = 1 };
                    Require(ImpactVisual.BodyLift(low, sim.Elapsed, false) < ImpactVisual.BodyLift(high, sim.Elapsed, false) * .35f && ImpactVisual.BodyLift(high, sim.Elapsed, true) == 0, "A body flung by a shot skims low and flat");
                    var impactHost = new GameObject("RangedFeelImpactFixture"); temporary.Add(impactHost);
                    ImpactVisual impact = impactHost.AddComponent<ImpactVisual>();
                    flung.Add(new FlungBody { Definition = enemyDef, UnitId = 5003, Position = new Vector2(4, 0), Velocity = Vector2.right * 6, Started = sim.Elapsed, Until = sim.Elapsed + 1, Weight = .3f, Pointed = true, Pinned = true, Low = true, Cause = new CombatCause(6001, ally.Id, "flung-body", 1) });
                    second.PinnedUntil = sim.Elapsed + ImpactRules.PinSeconds; second.HitDirection = Vector2.right;
                    var stepEvents = (List<CombatEvent>)typeof(CombatSimulation).GetField("events", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sim);
                    stepEvents.Add(new CombatEvent(CombatEventKind.Strike, new Vector2(1, 1), Faction.Companion, 3, Vector2.right, cause: new CombatCause(6001, ally.Id, "strike", 2)));
                    impact.Display(sim, 5f);
                    Require(impact.Strikes == 1 && impactHost.GetComponentsInChildren<InkedText>().Any(t => t.gameObject.activeSelf && t.Text == "STRIKE!"), "A chain that fells three stamps a big inked STRIKE!");
                    stepEvents.Clear();
                    impact.Display(sim, 5f + ImpactVisual.StrikeLife + .05f);
                    Require(impact.Strikes == 0 && !impactHost.GetComponentsInChildren<InkedText>().Any(t => t.gameObject.activeSelf), "The STRIKE! fades");
                    Require(impact.PinnedBodies == 1 && impact.Bodies == 1, "A body pinned to the edge hangs there with the shot through it");
                    Require(impact.PinnedEnemies == 1, "A living enemy pinned to the edge strains against it");
                    second.PinnedUntil = 0; flung.Clear();
                    Require(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Strike, Vector2.zero, Faction.Companion, 3)) != null, "A STRIKE! has its coin chime");
                    MaterialAudio.Observe(new CombatEvent(CombatEventKind.BodyLaunched, Vector2.zero, Faction.Enemy, 1, Vector2.right, subjectId: waxUnit.Id, subject: waxUnit, cause: new CombatCause(6002, 3)));
                    Require(MaterialAudio.CueFor(new CombatEvent(CombatEventKind.WallSplat, Vector2.zero, Faction.Companion, 0, Vector2.right, cause: new CombatCause(6002, 3, "flung-body", 1).Child("pinned")), out _, out _) == "cmb.hit.wax", "A body pinned to the edge thuds in its own material");
                    UnityEngine.Object.DestroyImmediate(visual);
                }
            }
            finally
            {
                PresentationSettings.Apply(new PlayerProfile());
                RangedFeel.Reset(); MaterialAudio.Clear();
                foreach (UnityEngine.Object o in temporary) if (o != null) UnityEngine.Object.DestroyImmediate(o);
            }
            Debug.Log("RANGED_FEEL_CHECKS_PASSED " + passed);
        }
    }
}
