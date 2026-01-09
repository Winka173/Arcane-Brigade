using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class WeaponSandbox : MonoBehaviour, IDisposable
    {
        public const int PreviewSeed = 45016;
        private readonly List<UnityEngine.Object> templates = new List<UnityEngine.Object>();
        private readonly List<UnityEngine.Object> runtimeAssets = new List<UnityEngine.Object>();
        private readonly List<WeaponDefinition> weapons = new List<WeaponDefinition>();
        private readonly Dictionary<int, UnitVisual> unitViews = new Dictionary<int, UnitVisual>();
        private readonly Dictionary<int, SpriteRenderer> projectileViews = new Dictionary<int, SpriteRenderer>();
        private readonly List<int> retired = new List<int>();
        private readonly List<Renderer> renderers = new List<Renderer>();
        private CheckpointCatalog template;
        private CheckpointCatalog arena;
        private Sprite projectileDisc;
        private float tierDamageMultiplier;
        private int selected;
        private bool presentation;
        private bool focused = true;
        private bool applicationPaused;
        private GameObject world;
        private Scene previewScene;
        private Camera previewCamera;
        private ArcadeEffects effects;
        private ExpandedVisual expandedVisual;
        private CheckpointAudio audioFeedback;
        private UnitState target;
        private float targetDistance;
        private GUIStyle instructionText;
        private GUIStyle descriptionText;
        public bool IsOpen { get; private set; }
        public int Tier { get; private set; } = 1;
        public bool MovingTarget { get; private set; }
        public bool ReducedMotion { get; private set; }
        public int WeaponCount => weapons.Count;
        public string SelectedWeaponId => weapons.Count == 0 ? null : weapons[selected].Id;
        public CombatSimulation Simulation { get; private set; }
        public UnitState Attacker { get; private set; }
        public RenderTexture PreviewTexture { get; private set; }
        public Camera PreviewCamera => previewCamera;
        public UnitVisual GetUnitVisual(int unitId) => unitViews.TryGetValue(unitId, out UnitVisual view) ? view : null;
        public static RenderTextureDescriptor GetPreviewDescriptor(bool mobile)
        {
            return new RenderTextureDescriptor(mobile ? 880 : 2560, mobile ? 440 : 1280, RenderTextureFormat.ARGB32, 16)
            {
                msaaSamples = mobile ? 2 : 4
            };
        }
        public int FireEvents { get; private set; }
        public int HitEvents { get; private set; }
        public int SignatureEvents { get; private set; }
        public int RuntimeAssetCount => runtimeAssets.Count;
        public int TemplateAssetCount => templates.Count;

        public void Open(RunCatalog content, PlayerProfile readOnlyProfile, RunSnapshot readOnlySaved = null,
            string weaponId = null, int tier = 1, bool moving = false, bool createPresentation = true)
        {
            if (content == null || content.Arena == null || readOnlyProfile == null) throw new ArgumentException("Sandbox needs content and a read-only profile");
            Dispose();
            try
            {
                var discovered = new HashSet<string>(readOnlyProfile.SeenWeapons ?? new List<string>(), StringComparer.Ordinal);
                if (readOnlySaved != null)
                {
                    if (readOnlySaved.Roster != null)
                        foreach (RosterEntry entry in readOnlySaved.Roster) if (entry != null && !string.IsNullOrEmpty(entry.WeaponId)) discovered.Add(entry.WeaponId);
                    UnitDefinition champion = content.Unit(readOnlySaved.ChampionId);
                    if (champion != null && champion.Weapon != null) discovered.Add(champion.Weapon.Id);
                }
                var added = new HashSet<string>(StringComparer.Ordinal);
                foreach (WeaponDefinition weapon in content.Weapons)
                    if (weapon != null && discovered.Contains(weapon.Id) && added.Add(weapon.Id)) weapons.Add(Clone(weapon, templates));
                template = Clone(content.Arena, templates);
                template.Champion = CloneUnit(content.Champions[0], templates);
                template.Companion = CloneUnit(content.Companions[0], templates);
                template.MeleeEnemy = CloneUnit(content.Arena.MeleeEnemy, templates);
                template.RangedEnemy = template.MeleeEnemy;
                template.Boss = null;
                template.StartingCompanions = new UnitDefinition[0];
                template.Procs = new ActiveProc[0];
                projectileDisc = content.ProjectileDisc != null ? content.ProjectileDisc : template.Projectile;
                tierDamageMultiplier = content.TierDamageMultiplier;
                selected = Mathf.Max(0, weapons.FindIndex(weapon => weapon.Id == weaponId));
                Tier = Mathf.Clamp(tier, 1, 3);
                MovingTarget = moving;
                ReducedMotion = readOnlyProfile.ReducedMotion;
                presentation = createPresentation;
                IsOpen = true;
                ResetPreview();
            }
            catch { Dispose(); throw; }
        }

        public bool SelectWeapon(string id)
        {
            int index = weapons.FindIndex(weapon => weapon.Id == id);
            if (!IsOpen || index < 0) return false;
            selected = index;
            ResetPreview();
            return true;
        }

        public void CycleWeapon(int direction)
        {
            if (!IsOpen || weapons.Count == 0) return;
            selected = (selected + direction % weapons.Count + weapons.Count) % weapons.Count;
            ResetPreview();
        }

        public void SetTier(int tier) { Tier = Mathf.Clamp(tier, 1, 3); if (IsOpen) ResetPreview(); }
        public void SetMovingTarget(bool moving) { MovingTarget = moving; if (IsOpen) ResetPreview(); }
        public void SetReducedMotion(bool reducedMotion)
        {
            ReducedMotion = reducedMotion;
            foreach (UnitVisual view in unitViews.Values) view.ReducedMotion = reducedMotion;
        }

        public void ResetPreview()
        {
            ReleaseWorld();
            FireEvents = HitEvents = SignatureEvents = 0;
            if (!IsOpen || weapons.Count == 0) return;
            arena = Clone(template, runtimeAssets);
            arena.Champion = CloneUnit(template.Champion, runtimeAssets);
            arena.Companion = CloneUnit(template.Companion, runtimeAssets);
            arena.Companion.Weapon = Clone(weapons[selected], runtimeAssets);
            arena.Companion.CombatTier = Tier;
            arena.Companion.BodyHeight *= CompanionTierScaling.Body(Tier);
            arena.Companion.Passive = PassiveKind.None;
            arena.Companion.Speed = 0;
            WeaponDefinition weapon = arena.Companion.Weapon;
            MeleeBalance.Apply(weapon);
            CompanionTierScaling.Apply(weapon, Tier);
            weapon.Damage *= Mathf.Pow(tierDamageMultiplier, Tier - 1);
            weapon.Windup = Mathf.Min(weapon.Windup, weapon.Cooldown * 0.35f);
            weapon.Recovery = Mathf.Min(weapon.Recovery, weapon.Cooldown * 0.4f);
            arena.Champion.Passive = PassiveKind.None;
            arena.Champion.Speed = 0;
            arena.Boss = CloneUnit(template.MeleeEnemy, runtimeAssets);
            arena.Boss.IsBoss = false;
            arena.Boss.Health = float.PositiveInfinity;
            arena.Boss.Speed = arena.Boss.ContactDamage = 0;
            arena.ExplicitStartingRoster = true;
            arena.StartingCompanions = new[] { arena.Companion };
            arena.EnemyBudget = 0;
            arena.WaveDuration = arena.ChestInterval = float.PositiveInfinity;
            arena.ExtendedLoot = false;
            arena.GoldPerKill = 0;
            arena.PassiveHeal = arena.Armor = 0;
            arena.Procs = new ActiveProc[0];
            arena.CompanionInterceptor = false;
            arena.EnableArenaToys = arena.EnableBossArenaPhase = arena.CarbonRibbon = false;
            arena.HarvestStacks = arena.ReflectiveInkStacks = arena.OfficeLensStacks = 0;
            targetDistance = Mathf.Clamp(weapon.Range * 0.65f, 0.6f, 3f);
            arena.ArenaSize = new Vector2(14, 9);
            Simulation = new CombatSimulation(arena);
            Simulation.Start(PreviewSeed);
            Simulation.Champion.Position = Simulation.Champion.PreviousPosition = new Vector2(-1.5f, 0);
            Simulation.Champion.NextAttack = Simulation.Champion.NextPassive = float.PositiveInfinity;
            foreach (UnitState unit in Simulation.Units) if (unit.Faction == Faction.Companion) Attacker = unit;
            Attacker.Position = Attacker.PreviousPosition = Vector2.zero;
            target = Simulation.Boss;
            target.Position = target.PreviousPosition = new Vector2(targetDistance, 0);
            target.NextAttack = target.NextContact = target.NextBossPattern = float.PositiveInfinity;
            if (presentation) CreateWorld();
        }

        public void AdvancePreview(float delta)
        {
            if (!IsOpen || Simulation == null || !float.IsFinite(delta) || delta <= 0 || delta > 0.1f) return;
            target.Position = new Vector2(targetDistance, MovingTarget ? Mathf.Sin((Simulation.Elapsed + delta) * 1.6f) * Mathf.Min(0.65f, targetDistance * 0.35f) : 0);
            Simulation.Step(delta, Vector2.zero);
            foreach (CombatEvent value in Simulation.Events)
            {
                if (value.Kind == CombatEventKind.Fire) FireEvents++;
                if (value.Kind == CombatEventKind.Hit) HitEvents++;
                if (value.Kind == CombatEventKind.SignaturePulse || value.Kind == CombatEventKind.SignatureBurst) SignatureEvents++;
                if (effects != null) effects.Play(value);
                if (audioFeedback != null) audioFeedback.Play(value);
            }
            if (effects != null) effects.Advance(delta);
            if (world != null) HideRenderers();
        }

        private void FixedUpdate() { if (focused && !applicationPaused) AdvancePreview(Time.fixedDeltaTime); }
        private void OnApplicationFocus(bool focus) { focused = focus; }
        private void OnApplicationPause(bool paused) { applicationPaused = paused; }

        public void HandleInput()
        {
            if (!IsOpen) return;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton4)) CycleWeapon(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton5)) CycleWeapon(1);
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetTier(1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetTier(2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetTier(3);
            if (Input.GetKeyDown(KeyCode.JoystickButton0)) SetTier(Tier % 3 + 1);
            if (Input.GetKeyDown(KeyCode.T) || Input.GetKeyDown(KeyCode.JoystickButton2)) SetMovingTarget(!MovingTarget);
            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton3)) ResetPreview();
        }

        private void CreateWorld()
        {
            previewScene = SceneManager.CreateScene("WeaponSandbox-" + Guid.NewGuid().ToString("N"));
            world = new GameObject("WeaponSandboxWorld") { hideFlags = HideFlags.DontSave };
            SceneManager.MoveGameObjectToScene(world, previewScene);
            effects = world.AddComponent<ArcadeEffects>();
            expandedVisual = world.AddComponent<ExpandedVisual>();
            audioFeedback = world.AddComponent<CheckpointAudio>();
            audioFeedback.ConfigurePresentation();
            if (FindAnyObjectByType<AudioListener>() == null) world.AddComponent<AudioListener>();
            var cameraObject = new GameObject("WeaponSandboxCamera");
            cameraObject.transform.SetParent(world.transform, false);
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.scene = previewScene;
            previewCamera.enabled = false;
            previewCamera.orthographic = true;
            previewCamera.orthographicSize = 2.2f;
            previewCamera.aspect = 2f;
            previewCamera.allowMSAA = true;
            previewCamera.transform.position = new Vector3(0.8f, 0, -10);
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.10f, 0.08f, 0.19f, 1f);
            previewCamera.cullingMask = 1 << 31;
            PreviewTexture = new RenderTexture(GetPreviewDescriptor(Application.isMobilePlatform))
            {
                name = "WeaponSandboxPreview", hideFlags = HideFlags.DontSave
            };
            PreviewTexture.Create();
            previewCamera.targetTexture = PreviewTexture;
            foreach (UnitState state in Simulation.Units)
            {
                if (state.Faction == Faction.Champion) continue;
                UnitVisual view = Instantiate(arena.UnitPrefab, world.transform).GetComponent<UnitVisual>();
                view.ReducedMotion = ReducedMotion;
                view.Bind(state, arena);
                unitViews.Add(state.Id, view);
            }
            HideRenderers();
        }

        private void HideRenderers()
        {
            foreach (Renderer renderer in world.GetComponentsInChildren<Renderer>())
            {
                renderer.gameObject.layer = 31;
                renderer.forceRenderingOff = true;
            }
        }

        public void RenderPreview()
        {
            if (world == null || previewCamera == null) return;
            foreach (UnitVisual view in unitViews.Values) view.Display(Simulation.VisualElapsed, 1);
            retired.Clear();
            foreach (int identifier in projectileViews.Keys)
            {
                bool alive = false;
                foreach (ProjectileState state in Simulation.Projectiles) if (state.Id == identifier) { alive = true; break; }
                if (!alive) retired.Add(identifier);
            }
            foreach (int identifier in retired)
            {
                GameObject retiredView = projectileViews[identifier].gameObject;
                retiredView.SetActive(false);
                Release(retiredView);
                projectileViews.Remove(identifier);
            }
            foreach (ProjectileState state in Simulation.Projectiles)
            {
                if (!projectileViews.TryGetValue(state.Id, out SpriteRenderer view))
                {
                    var child = new GameObject("SandboxProjectile");
                    child.transform.SetParent(world.transform, false);
                    view = child.AddComponent<SpriteRenderer>();
                    view.sprite = state.Sprite != null ? state.Sprite : projectileDisc;
                    view.sortingOrder = 1802;
                    child.AddComponent<ProjectileVisual>().Configure(projectileDisc);
                    projectileViews.Add(state.Id, view);
                }
                view.transform.position = state.Position;
                float size = state.VisualWidth > 0 ? state.VisualWidth : 0.22f;
                view.transform.localScale = Vector3.one * (size / view.sprite.bounds.size.x);
                view.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(state.Velocity.y, state.Velocity.x) * Mathf.Rad2Deg);
                view.color = state.Color;
                view.GetComponent<ProjectileVisual>().Display(state, view, state.Position, Simulation.Elapsed);
            }
            expandedVisual.ReducedMotion = ReducedMotion; expandedVisual.Display(Simulation);
            renderers.Clear(); world.GetComponentsInChildren<Renderer>(true, renderers);
            RenderTexture previousActive = RenderTexture.active;
            try
            {
                foreach (Renderer renderer in renderers) { renderer.gameObject.layer = 31; renderer.forceRenderingOff = false; }
                previewCamera.Render();
            }
            finally
            {
                foreach (Renderer renderer in renderers) if (renderer != null) renderer.forceRenderingOff = true;
                RenderTexture.active = previousActive;
            }
        }

        private void LateUpdate() { if (IsOpen) RenderPreview(); }

        public void DrawPanel(GUIStyle text, GUIStyle button)
        {
            GUI.Label(new Rect(65, 170, 1150, 50), "TRY DISCOVERED WEAPONS · no rewards or saved-run changes", text);
            if (weapons.Count == 0)
            {
                GUI.Label(new Rect(100, 270, 1050, 150), "No discovered weapons yet. Acquire weapons in a real run first.\nDiscovering a companion does not unlock every weapon it might receive.", text);
                return;
            }
            if (GUI.Button(new Rect(65, 230, 140, 55), "← PREVIOUS", button)) CycleWeapon(-1);
            GUI.Label(new Rect(225, 235, 600, 50), RunUICopy.WeaponName(SelectedWeaponId) + "  (" + (selected + 1) + "/" + weapons.Count + ")", text);
            if (GUI.Button(new Rect(1045, 230, 170, 55), "SAU →", button)) CycleWeapon(1);
            if (PreviewTexture != null) GUI.DrawTexture(new Rect(65, 300, 760, 300), PreviewTexture, ScaleMode.ScaleToFit, false);
            if (descriptionText == null) descriptionText = new GUIStyle(text) { fontSize = 14, wordWrap = true, alignment = TextAnchor.UpperLeft };
            GUI.Label(new Rect(65, 605, 760, 95), SignatureDescription.Describe(Attacker.Definition.Weapon, Tier), descriptionText);
            GUI.Label(new Rect(850, 295, 365, 75), "Preview tier " + Tier + " · not owned yet\nNo relics, bonds or passives", text);
            for (int index = 1; index <= 3; index++)
                if (GUI.Button(new Rect(850 + (index - 1) * 122, 380, 115, 55), "TIER " + index, button)) SetTier(index);
            if (GUI.Button(new Rect(850, 450, 365, 55), MovingTarget ? "TARGET: MOVING" : "TARGET: STATIONARY", button)) SetMovingTarget(!MovingTarget);
            if (GUI.Button(new Rect(850, 520, 365, 55), "RETRY · R", button)) ResetPreview();
            if (instructionText == null) instructionText = new GUIStyle(text) { fontSize = 16, wordWrap = false, alignment = TextAnchor.UpperLeft };
            GUI.Label(new Rect(850, 585, 365, 110), RunUIHints.Device == RunUIDevice.Controller ? "Auto-attack · immortal\nLB/RB: weapon · A: tier\nX: target · Y: retry\nB: back" : RunUIHints.Device == RunUIDevice.Touch ? "Auto-attack · immortal\nTap buttons to change weapon, tier\nand target movement\nTap BACK to close" : "Auto-attack · immortal\nArrows: weapon · 1–3: tier\nT: target · R: retry\nEsc: back", instructionText);
        }

        private static T Clone<T>(T source, List<UnityEngine.Object> assets) where T : UnityEngine.Object
        {
            T copy = Instantiate(source);
            copy.hideFlags = HideFlags.DontSave;
            assets.Add(copy);
            return copy;
        }

        private static UnitDefinition CloneUnit(UnitDefinition source, List<UnityEngine.Object> assets)
        {
            UnitDefinition copy = Clone(source, assets);
            copy.Weapon = Clone(source.Weapon, assets);
            copy.Tags = source.Tags == null ? new string[0] : (string[])source.Tags.Clone();
            return copy;
        }

        private static void Release(UnityEngine.Object value)
        {
            if (value == null) return;
            if (Application.isPlaying) Destroy(value); else DestroyImmediate(value);
        }

        private void ReleaseWorld()
        {
            if (previewCamera != null) previewCamera.targetTexture = null;
            if (world != null) { world.SetActive(false); Release(world); }
            if (PreviewTexture != null) { PreviewTexture.Release(); Release(PreviewTexture); }
            if (previewScene.IsValid() && previewScene.isLoaded) SceneManager.UnloadSceneAsync(previewScene);
            previewScene = default;
            foreach (UnityEngine.Object asset in runtimeAssets) Release(asset);
            runtimeAssets.Clear();
            unitViews.Clear(); projectileViews.Clear(); retired.Clear();
            world = null; previewCamera = null; PreviewTexture = null; effects = null; audioFeedback = null; expandedVisual = null;
            arena = null; Simulation = null; Attacker = target = null;
        }

        public void Dispose()
        {
            IsOpen = false;
            ReleaseWorld();
            foreach (UnityEngine.Object asset in templates) Release(asset);
            templates.Clear(); weapons.Clear(); template = null; projectileDisc = null;
            ReducedMotion = false;
            FireEvents = HitEvents = SignatureEvents = 0;
        }

        private void OnDisable() { Dispose(); }
        private void OnDestroy() { Dispose(); }
    }
}
