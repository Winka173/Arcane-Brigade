using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8MotionChecks
    {
        private static int passed;
        private static void Require(bool condition, string caption)
        {
            if (!condition) throw new InvalidOperationException("K8 motion: " + caption);
            passed++;
            Debug.Log("PASS K8 motion: " + caption);
        }

        [MenuItem("Arcane Brigade/K8/Motion focused checks")]
        public static void Run()
        {
            passed = 0;
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Require(catalog != null, "Run catalog available");
            string original = JsonUtility.ToJson(catalog);
            var temporary = new List<UnityEngine.Object>();
            var originalSettings = new PlayerProfile
            {
                ReducedMotion = PresentationSettings.ReducedMotion, ShowDamageNumbers = PresentationSettings.DamageNumbers,
                MusicVolume = PresentationSettings.MusicVolume, SfxVolume = PresentationSettings.SfxVolume,
                ReducedAudio = PresentationSettings.ReducedAudio, ShowLootIndicators = PresentationSettings.LootIndicators,
                HighReadability = PresentationSettings.HighReadability, Cosmetic = ArcadeSkin.Bundle, Volume = AudioListener.volume
            };
            PlayerProfile settings = new PlayerProfile();
            try
            {
                using (var session = new RunSession(catalog))
                {
                    session.Start(catalog.Champions[0].Id, 8808);
                    CheckpointCatalog combat = session.PrepareCombat();
                    combat.EnemyBudget = 0; combat.Boss = null; combat.WaveDuration = 1000;
                    combat.Procs = new ActiveProc[0]; combat.EnableArenaToys = false; combat.ChestInterval = 1000;
                    var simulation = new CombatSimulation(combat); simulation.Start(8808);
                    UnitDefinition definition = UnityEngine.Object.Instantiate(catalog.Companions[0]); temporary.Add(definition);
                    WeaponDefinition weapon = UnityEngine.Object.Instantiate(definition.Weapon); temporary.Add(weapon); definition.Weapon = weapon;
                    definition.BodyHeight = 1; definition.CombatTier = 3;
                    UnitState companion = simulation.AddFixture(definition, Faction.Companion, Vector2.zero);
                    Require(K8Motion.BodyHeight(companion) <= 0.88f * CompanionTierScaling.Body(3) && K8Motion.BodyHeight(companion) >= Mathf.Min(companion.Definition.BodyHeight * 0.95f, 0.88f) && K8Motion.BodyHeight(simulation.Champion) == simulation.Champion.Definition.BodyHeight, "Allies readable with star-scaled cap without scaling Champion");
                    Require(K8Motion.WeaponWidth(companion) <= K8Motion.BodyHeight(companion) * (companion.Definition.Weapon.Kind == WeaponKind.Melee ? 1.9f : 1.05f) + 0.0001f, "Companion weapons bounded by class-specific readable body size");
                    int before = simulation.CompanionCount;
                    for (int index = 0; index < 128; index++) simulation.AddCompanion(definition);
                    Require(simulation.CompanionCount == before + 128, "No companion population cap");
                    simulation.DamageFixture(companion, 100000);
                    Require(companion.Alive && float.IsPositiveInfinity(companion.Health), "Companions remain immortal");

                    weapon.Kind = WeaponKind.Melee; weapon.Motion = WeaponMotion.Slash; weapon.Family = ExpandedWeapon.None;
                    weapon.Windup = 0.2f; weapon.Recovery = 0.3f; weapon.SwingAngle = 65; weapon.BodyAttackTilt = 10;
                    K8Motion.Pose anticipation = K8Motion.Attack(weapon, 0.19f, 0.2f, false);
                    K8Motion.Pose impact = K8Motion.Attack(weapon, 0.2f, 0.2f, false);
                    K8Motion.Pose strike = K8Motion.Attack(weapon, 0.26f, 0.2f, false);
                    K8Motion.Pose settled = K8Motion.Attack(weapon, 0.5f, 0.2f, false);
                    Require(anticipation.Angle < 0 && anticipation.Lean < 0 && strike.Angle > 0 && strike.Lean > 0, "Anticipation precedes actual impact then follows through");
                    Require(Mathf.Abs(anticipation.Angle - impact.Angle) < 1, "Melee weapon continuous at impact boundary");
                    Require(settled.Angle == 0 && settled.Recoil == 0, "Recovery returns to rest");
                    K8Motion.Pose reduced = K8Motion.Attack(weapon, 0.26f, 0.2f, true);
                    Require(reduced.Angle == 0 && reduced.Lean == 0 && reduced.Recoil == 0 && reduced.Lift == 0, "Reduced-motion attack has no transform oscillation");
                    var distinct = new HashSet<Vector2>();
                    foreach (WeaponMotion motion in new[] { WeaponMotion.Thrust, WeaponMotion.Hammer, WeaponMotion.Cast, WeaponMotion.Bow, WeaponMotion.Bell, WeaponMotion.Scissors, WeaponMotion.Camera, WeaponMotion.Iron, WeaponMotion.Music })
                    {
                        weapon.Motion = motion;
                        K8Motion.Pose pose = K8Motion.Attack(weapon, 0.12f, 0.2f, false);
                        distinct.Add(new Vector2(pose.Angle, pose.Recoil));
                        K8Motion.Pose beforeImpact = K8Motion.Attack(weapon, 0.19999f, 0.2f, false);
                        K8Motion.Pose afterImpact = K8Motion.Attack(weapon, 0.20001f, 0.2f, false);
                        Require(Mathf.Abs(beforeImpact.Angle - afterImpact.Angle) < 0.1f && Mathf.Abs(beforeImpact.Recoil - afterImpact.Recoil) < 0.001f && Mathf.Abs(beforeImpact.Lean - afterImpact.Lean) < 0.1f, motion + " has continuous impact pose");
                    }
                    Require(distinct.Count >= 7, "Weapon families have distinct anticipation profiles");
                    weapon.Motion = WeaponMotion.Slash;
                    weapon.Family = ExpandedWeapon.Tea;
                    float tea = K8Motion.Attack(weapon, 0.1f, 0.2f, false).Angle;
                    weapon.Family = ExpandedWeapon.Fish;
                    Require(tea != K8Motion.Attack(weapon, 0.1f, 0.2f, false).Angle, "Tea pour and fish throw differ");
                    weapon.Family = ExpandedWeapon.None;
                    definition.IsBoss = true;
                    companion.PreviousPosition = Vector2.left; companion.Position = Vector2.zero;
                    Require(K8Motion.BodyFrame(companion, 1, false) < 2, "Moving boss does not reuse charge/recoil as walking");
                    companion.TelegraphUntil = 3;
                    Require(K8Motion.BodyFrame(companion, 2, false) == 2 && K8Motion.BodyFrame(companion, 2, true) == 2, "Actual boss telegraph selects charge frame, including reduced motion");
                    companion.TelegraphUntil = 0; companion.AttackStarted = 2; companion.AttackWindup = 0.2f; companion.PendingAttack = true;
                    Require(K8Motion.BodyFrame(companion, 2.1f, false) == 2, "Pending boss attack uses actual anticipation frame");
                    companion.PendingAttack = false;
                    Require(K8Motion.BodyFrame(companion, 2.25f, false) == 3, "Boss recovery uses recoil frame");
                    companion.AttackStarted = -100; companion.LastHit = 4;
                    Require(K8Motion.BodyFrame(companion, 4.1f, false) == 3, "Boss hit uses recoil frame");
                    definition.IsBoss = false; companion.LastHit = -100;
                    Require(K8Motion.BodyFrame(companion, 1, false) >= 2 && K8Motion.BodyFrame(companion, 1, true) == 0, "Hero identity retains alternating walking frames and stable reduced pose");

                    var root = new GameObject("K8MotionTestUnit"); temporary.Add(root);
                    UnitVisual view = root.AddComponent<UnitVisual>(); view.CreateHierarchy(combat.ImpactSprite); view.Bind(companion, combat);
                    Require(root.GetComponentsInChildren<Transform>(true).All(part => !part.name.Contains("Arm")), "Only attached hands, no arm hierarchy");
                    Require(root.GetComponentsInChildren<SpriteRenderer>().Count(part => part.name.StartsWith("K8MotionTierStar") && part.enabled) == 3, "Three-star identity visibly represented");
                    SpriteRenderer held = root.transform.Find("GripRoot/WeaponVisual").GetComponent<SpriteRenderer>();
                    Vector3 heldSize = Vector3.Scale(held.sprite.bounds.size, held.transform.localScale);
                    Require(Mathf.Max(heldSize.x, heldSize.y) <= K8Motion.BodyHeight(companion) * (companion.Definition.Weapon.Kind == WeaponKind.Melee ? 1.75f : 0.95f) + 0.0001f, "Both weapon axes bounded by class, including tall bows and tier-three shapes");
                    SpriteRenderer reference = root.transform.Find("BodyVisual").GetComponent<SpriteRenderer>();
                    SpriteRenderer tierStar = root.GetComponentsInChildren<SpriteRenderer>().First(part => part.name.StartsWith("K8MotionTierStar"));
                    Require(tierStar.transform.localPosition.y > reference.transform.localPosition.y + reference.sprite.bounds.max.y * reference.transform.localScale.y, "Tier stars clear actual sprite pivot and silhouette");
                    companion.AttackStarted = 10; companion.AttackWindup = 0.2f; companion.AttackDirection = Vector2.left; companion.Facing = Vector2.right;
                    view.Display(10.1f, 1);
                    Transform grip = root.transform.Find("GripRoot");
                    Require(grip.localPosition.x < 0, "Actual attack direction locks independently from idle facing");
                    Vector3 gripPosition = grip.localPosition; Quaternion gripRotation = grip.localRotation;
                    view.Display(10.1f, 1);
                    Require(gripPosition == grip.localPosition && gripRotation == grip.localRotation, "Same simulation timestamp freezes aim and attack pose");
                    view.ReducedMotion = true; view.Display(10.26f, 1);
                    Require(Mathf.Abs(Mathf.DeltaAngle(grip.localEulerAngles.z, 180)) < 0.01f, "Reduced motion keeps locked attack direction without swing");
                    view.ReducedMotion = false;
                    view.BeginArrival(new Vector2(4, 3)); view.Display(10.27f, 1);
                    Vector3 arrival = root.transform.position;
                    view.Display(10.27f, 1);
                    Require(root.transform.position == arrival, "Arrival freezes when presentation clock is paused");
                    Require(UnitVisual.IdleBeat(companion, 2, true, false) == 0 && UnitVisual.IdleBeat(companion, 2, false, true) == 0, "Idle personality suppressed during attacks and reduced motion");

                    settings.ReducedMotion = false; settings.ShowDamageNumbers = true; PresentationSettings.Apply(settings);
                    var dustRoot = new GameObject("K8MotionTestDust"); temporary.Add(dustRoot);
                    FootstepDust dust = dustRoot.AddComponent<FootstepDust>(); dust.Configure(combat.ImpactSprite);
                    companion.PreviousPosition = Vector2.left; companion.Position = Vector2.zero;
                    dust.Display(simulation, 0.02f);
                    SpriteRenderer puff = dustRoot.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(part => part.enabled);
                    Require(puff != null && puff.color.a > 0, "Fresh dust initializes visibly without pooled stale transforms");
                    Vector3 puffPosition = puff.transform.position; Color puffColor = puff.color;
                    simulation.Pause(true); dust.Display(simulation, 1);
                    Require(puff.transform.position == puffPosition && puff.color == puffColor, "Paused dust does not advance even with nonzero caller delta");
                    simulation.Pause(false);
                    settings.ReducedMotion = true; PresentationSettings.Apply(settings); dust.Display(simulation, 0.02f);
                    Require(dustRoot.GetComponentsInChildren<SpriteRenderer>().All(part => !part.enabled), "Reduced motion suppresses existing and new dust");

                    var feedbackRoot = new GameObject("K8MotionTestFeedback"); temporary.Add(feedbackRoot);
                    CombatFeedback feedback = feedbackRoot.AddComponent<CombatFeedback>(); feedback.Configure(combat.ImpactSprite);
                    feedback.Play(new CombatEvent(CombatEventKind.Hit, Vector2.one, Faction.Enemy, 12));
                    TextMesh number = feedbackRoot.GetComponentsInChildren<TextMesh>().First(part => part.name == "DamageNumber");
                    Vector3 numberPosition = number.transform.position;
                    feedback.Advance(0.1f);
                    Require(number.transform.position == numberPosition && number.transform.localScale == Vector3.one, "Reduced-motion damage stays stationary with fade only");
                    Color numberColor = number.color; feedback.Advance(0);
                    Require(number.color == numberColor, "Zero-delta feedback is frozen");
                    Require(feedbackRoot.GetComponentsInChildren<SpriteRenderer>().Length == 0, "Reduced motion emits no impact particles");
                    settings.ReducedMotion = false; PresentationSettings.Apply(settings);
                    feedback.Play(new CombatEvent(CombatEventKind.Hit, new Vector2(5, 4), Faction.Enemy, 12, Vector2.right, weapon));
                    SpriteRenderer spark = feedbackRoot.GetComponentsInChildren<SpriteRenderer>().First();
                    Require((Vector2)spark.transform.position == new Vector2(5, 4) && spark.transform.localScale.x > spark.transform.localScale.y, "Melee hit initializes directional slivers at impact point");
                    int sparks = feedbackRoot.GetComponentsInChildren<SpriteRenderer>().Length;
                    feedback.Play(new CombatEvent(CombatEventKind.Kill, Vector2.zero, Faction.Companion));
                    Require(feedbackRoot.GetComponentsInChildren<SpriteRenderer>().Length == sparks, "No companion death particles");
                    feedback.Clear();
                    feedback.Play(new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, 12, subjectId: 901));
                    feedback.Play(new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, 8, subjectId: 901));
                    Require(feedbackRoot.GetComponentsInChildren<TextMesh>().Count(part => part.name == "DamageNumber" && part.gameObject.activeSelf) == 1 && feedbackRoot.GetComponentsInChildren<TextMesh>().Any(part => part.name == "DamageNumber" && part.text == "20"), "Same-target hits aggregate visually without altering combat damage");

                    var projectileRoot = new GameObject("K8MotionTestProjectile"); temporary.Add(projectileRoot);
                    SpriteRenderer bullet = projectileRoot.AddComponent<SpriteRenderer>(); bullet.sprite = combat.ImpactSprite;
                    ProjectileVisual projectile = projectileRoot.AddComponent<ProjectileVisual>(); projectile.Configure(combat.ImpactSprite);
                    var shot = new ProjectileState { Faction = Faction.Enemy, Color = Color.red, Velocity = Vector2.right, Weapon = weapon };
                    projectile.Display(shot, bullet, Vector2.zero, 1);
                    SpriteRenderer core = projectileRoot.GetComponentsInChildren<SpriteRenderer>().First(part => part.name == "Core");
                    Require(bullet.sortingOrder > 2304 && core.sortingOrder > bullet.sortingOrder, "Hostile bullets and cores remain foreground above squad and important death poses");
                    settings.ReducedMotion = true; PresentationSettings.Apply(settings);
                    projectile.Display(shot, bullet, Vector2.zero, 1);
                    SpriteRenderer halo = projectileRoot.GetComponentsInChildren<SpriteRenderer>().First(part => part.name == "Halo");
                    Vector3 haloScale = halo.transform.localScale;
                    projectile.Display(shot, bullet, Vector2.zero, 2);
                    Require(halo.transform.localScale == haloScale && core.enabled, "Reduced motion removes bullet pulse without hiding danger core");

                    var deathRoot = new GameObject("K8MotionTestDeath"); temporary.Add(deathRoot);
                    DeathFeedback death = deathRoot.AddComponent<DeathFeedback>(); death.Configure(combat.ImpactSprite);
                    var defeated = new UnitState { Id = 9000, Definition = definition, Faction = Faction.Enemy, Alive = false, Position = new Vector2(2, 3), HitDirection = Vector2.right };
                    view.Bind(defeated, combat); view.Display(1, 1); death.Play(view, defeated);
                    K8MotionDeath deathMotion = deathRoot.GetComponentInChildren<K8MotionDeath>();
                    Require(deathMotion.GetComponent<SpriteRenderer>().enabled, "Death copies full silhouette even when living body uses split puppet renderers");
                    Vector3 deathScale = deathMotion.transform.localScale;
                    death.Advance(0.1f);
                    typeof(K8MotionDeath).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(deathMotion, null);
                    Require((Vector2)deathMotion.transform.parent.position == defeated.Position && deathMotion.transform.localScale == deathScale, "Reduced-motion death retains stationary silhouette with existing timed fade");
                    settings.ReducedMotion = false; PresentationSettings.Apply(settings);

                    var expandedRoot = new GameObject("K8MotionTestExpanded"); temporary.Add(expandedRoot);
                    ExpandedVisual expanded = expandedRoot.AddComponent<ExpandedVisual>();
                    var fields = (List<ExpandedField>)typeof(CombatSimulation).GetField("fields", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(simulation);
                    fields.Add(new ExpandedField { Kind = ExpandedFieldKind.Link, Radius = 2, Until = 1 });
                    fields.Add(new ExpandedField { Kind = ExpandedFieldKind.OfficeHazard, Radius = 2, Until = 1 });
                    expanded.Display(simulation);
                    Require(expanded.VisibleCount == 1, "No buff target circles; real arena hazard retained");
                    LineRenderer hazard = expandedRoot.GetComponentInChildren<LineRenderer>();
                    Require(Mathf.Abs(hazard.GetPosition(0).magnitude - 2) < 0.001f, "Hazard boundary matches actual radius without decorative expansion");
                }
                Require(JsonUtility.ToJson(catalog) == original, "Catalog not mutated by presentation checks");
                Debug.Log("K8_MOTION_CHECKS_PASSED " + passed);
            }
            finally
            {
                PresentationSettings.Apply(originalSettings);
                for (int index = temporary.Count - 1; index >= 0; index--) UnityEngine.Object.DestroyImmediate(temporary[index]);
            }
        }
    }
}
