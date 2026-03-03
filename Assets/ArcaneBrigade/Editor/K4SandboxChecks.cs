using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K4SandboxChecks
    {
        [MenuItem("Arcane Brigade/K4/Check isolated weapon sandbox")]
        public static void Run()
        {
            var results = new List<string>();
            Action<bool, string> require = (condition, name) =>
            {
                if (!condition) throw new InvalidOperationException("K4 sandbox: " + name);
                results.Add("PASS " + name);
            };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            require(catalog != null && catalog.Weapons.Length > 0 && catalog.Companions.Length > 0, "Sandbox content exists");
            require(catalog.Arena.UnitPrefab.GetComponent<UnitVisual>() != null, "Uses the gameplay UnitVisual prefab");
            var profile = new PlayerProfile { BestWave = 7, Wins = 2, TotalKills = 42 };
            var saved = new RunSnapshot
            {
                ChampionId = catalog.Champions[0].Id, Gold = 217, Eggs = 9, Seed = 991,
                ShopRandom = 234, WeaponRandom = 876, TotalKills = 33,
                Roster = new List<RosterEntry> { new RosterEntry { UnitId = catalog.Companions[0].Id, WeaponId = catalog.Weapons[0].Id, Tier = 2 } }
            };
            string originalProfile = JsonUtility.ToJson(profile);
            string originalSaved = JsonUtility.ToJson(saved);
            string originalContent = Fingerprint(catalog);
            UnityEngine.Random.State originalRandom = UnityEngine.Random.state;
            var host = new GameObject("K4SandboxChecks") { hideFlags = HideFlags.HideAndDontSave };
            WeaponSandbox sandbox = host.AddComponent<WeaponSandbox>();
            try
            {
                profile.SeenUnits.Add(catalog.Companions[0].Id);
                sandbox.Open(catalog, profile, createPresentation: false);
                require(sandbox.IsOpen && sandbox.WeaponCount == 0 && sandbox.Simulation == null, "SeenUnits does not imply weapon discovery");
                sandbox.ResetPreview(); sandbox.SetTier(3); sandbox.SetMovingTarget(true); sandbox.Dispose(); sandbox.Dispose();
                require(!sandbox.IsOpen && sandbox.RuntimeAssetCount == 0 && sandbox.TemplateAssetCount == 0, "Empty list reset and repeated dispose are safe");
                profile.SeenUnits.Clear();
                sandbox.Open(catalog, profile, saved, createPresentation: false);
                require(sandbox.SelectWeapon(saved.Roster[0].WeaponId), "Saved roster grants read-only preview eligibility");
                require(sandbox.SelectWeapon(catalog.Champions[0].Weapon.Id), "Saved champion default grants read-only preview eligibility");
                require(profile.SeenWeapons.Count == 0, "Eligibility union never writes discovery");
                sandbox.Dispose();
                foreach (WeaponDefinition weapon in catalog.Weapons) profile.SeenWeapons.Add(weapon.Id);
                profile.SeenWeapons.Add("unknown-weapon-id");
                profile.SeenWeapons.Add(catalog.Weapons[0].Id);
                string discoveredProfile = JsonUtility.ToJson(profile);
                sandbox.Open(catalog, profile, weaponId: catalog.Weapons[0].Id, createPresentation: false);
                require(!sandbox.ReducedMotion, "Reduced motion defaults off with the profile");
                require(sandbox.WeaponCount == catalog.Weapons.Select(weapon => weapon.Id).Distinct().Count(), "Stable discovery IDs filter unknown and duplicate entries");
                require(!sandbox.SelectWeapon("locked-or-unknown"), "Unknown selection cannot unlock weapons");
                foreach (WeaponDefinition weapon in catalog.Weapons)
                {
                    sandbox.SelectWeapon(weapon.Id);
                    for (int tier = 1; tier <= 3; tier++)
                    {
                        sandbox.SetTier(tier);
                        require(sandbox.Attacker.Definition.CombatTier == tier && sandbox.Attacker.Definition.Weapon != weapon,
                            weapon.Id + " tier " + tier + " uses independent unit/weapon data");
                        require(Mathf.Approximately(sandbox.Attacker.Definition.Weapon.Damage, weapon.Damage * (weapon.Kind == WeaponKind.Melee ? MeleeBalance.Damage : 1) * Mathf.Pow(catalog.TierDamageMultiplier, tier - 1)), // the sandbox shows melee arms at their combat balance
                            weapon.Id + " tier damage follows catalog multiplier with no build bonuses");
                        Tick(sandbox, 300);
                        if (weapon.Kind != WeaponKind.Contact)
                            require(sandbox.FireEvents > 0 && sandbox.HitEvents > 0, weapon.Id + " tier " + tier + " auto attacks through actual simulation events");
                        if (weapon.Signature == SignatureWeapon.Bell || weapon.Signature == SignatureWeapon.Stamp)
                            require(sandbox.SignatureEvents > 0, weapon.Id + " real cadence payoff resolves");
                        require(sandbox.Simulation.Gold == 0 && sandbox.Simulation.Eggs == 0 && sandbox.Simulation.Kills == 0 && sandbox.Simulation.Pickups.Count == 0,
                            weapon.Id + " preview cannot farm rewards or kills");
                    }
                }
                sandbox.SelectWeapon(catalog.Weapons[0].Id);
                sandbox.SetTier(1); sandbox.SetMovingTarget(false);
                Tick(sandbox, 120);
                string first = StateFingerprint(sandbox);
                WeaponDefinition retiredWeapon = sandbox.Attacker.Definition.Weapon;
                sandbox.ResetPreview();
                require(retiredWeapon == null || Application.isPlaying, "Reset disposes old runtime weapon clone");
                require(sandbox.FireEvents == 0 && sandbox.HitEvents == 0 && sandbox.Simulation.Elapsed == 0 && sandbox.Attacker.AttackBeat == 0,
                    "Reset clears elapsed time, attacks and diagnostics");
                sandbox.SetReducedMotion(true);
                Tick(sandbox, 120);
                require(first == StateFingerprint(sandbox), "Local combat and roam RNG reproduce after reset despite reduced-motion change");
                require(!profile.ReducedMotion && sandbox.ReducedMotion, "Local reduced-motion override never writes the profile");
                sandbox.SetMovingTarget(true);
                Vector2 before = sandbox.Simulation.Boss.Position;
                Tick(sandbox, 30);
                require(sandbox.Simulation.Boss.Position != before, "Moving target preset actually moves");
                sandbox.SetMovingTarget(false); Tick(sandbox, 30);
                require(Mathf.Approximately(sandbox.Simulation.Boss.Position.y, 0), "Stationary preset returns to fixed lane");
                sandbox.AdvancePreview(float.NaN); sandbox.AdvancePreview(1);
                require(JsonUtility.ToJson(profile) == discoveredProfile, "Open, selection, tier, target, reset never mutate profile");
                require(JsonUtility.ToJson(saved) == originalSaved, "Saved roster, currency, score and RNG stay byte-identical");
                require(Fingerprint(catalog) == originalContent, "Shared catalog, unit and weapon data stay byte-identical");
                var reducedProfile = JsonUtility.FromJson<PlayerProfile>(discoveredProfile);
                reducedProfile.ReducedMotion = true;
                string originalReducedProfile = JsonUtility.ToJson(reducedProfile);
                sandbox.Open(catalog, reducedProfile, createPresentation: false);
                require(sandbox.ReducedMotion, "Opening snapshots the saved reduced-motion preference");
                CheckPose(catalog, sandbox, require);
                require(sandbox.ReducedMotion && JsonUtility.ToJson(reducedProfile) == originalReducedProfile, "Reduced motion survives tier/reset without profile mutation");
                require(UnityEngine.Random.state.Equals(originalRandom), "Global Unity RNG untouched");
                sandbox.Dispose();
                require(!sandbox.IsOpen && sandbox.Simulation == null && sandbox.Attacker == null && sandbox.PreviewTexture == null && sandbox.RuntimeAssetCount == 0 && sandbox.TemplateAssetCount == 0,
                    "Dispose releases simulation, templates, runtime assets and presentation references");
                sandbox.ResetPreview(); sandbox.AdvancePreview(0.05f); sandbox.Dispose();
                profile.SeenWeapons.Clear();
                require(JsonUtility.ToJson(profile) == originalProfile, "Fixture profile unchanged except explicit test discovery setup");
                CheckPipeline(require);
                require(Enum.IsDefined(typeof(RunMenu.Page), "WeaponSandbox"), "RunMenu exposes sandbox page");
            }
            finally
            {
                sandbox.Dispose();
                UnityEngine.Object.DestroyImmediate(host);
            }
            Directory.CreateDirectory("PreviewExports/K4");
            File.WriteAllLines("PreviewExports/K4/Sandbox_Checks.txt", results);
            Debug.Log("ARCANE_K4_SANDBOX_CHECKS_PASSED " + results.Count + " (model checks; native rendering/audio still require capture)");
        }

        private static void Tick(WeaponSandbox sandbox, int count)
        {
            for (int index = 0; index < count; index++) sandbox.AdvancePreview(0.05f);
        }

        private static string StateFingerprint(WeaponSandbox sandbox)
        {
            return sandbox.Simulation.Elapsed + ":" + sandbox.FireEvents + ":" + sandbox.HitEvents + ":" + sandbox.SignatureEvents + ":" + sandbox.Attacker.AttackBeat + ":" + sandbox.Attacker.NextAttack + ":" + sandbox.Simulation.Projectiles.Count;
        }

        private static string Fingerprint(RunCatalog catalog)
        {
            return JsonUtility.ToJson(catalog) + JsonUtility.ToJson(catalog.Arena)
                + string.Join("|", catalog.Weapons.Select(weapon => JsonUtility.ToJson(weapon)))
                + string.Join("|", catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).Select(unit => JsonUtility.ToJson(unit)))
                + JsonUtility.ToJson(catalog.Arena.MeleeEnemy);
        }

        private static void CheckPipeline(Action<bool, string> require)
        {
            RenderTextureDescriptor desktop = WeaponSandbox.GetPreviewDescriptor(false);
            RenderTextureDescriptor mobile = WeaponSandbox.GetPreviewDescriptor(true);
            require(desktop.width == 2560 && desktop.height == 1280 && desktop.msaaSamples == 4, "Desktop native preview uses 2560x1280 MSAA4");
            require(mobile.width == 880 && mobile.height == 440 && mobile.msaaSamples == 2, "Mobile preview uses 880x440 MSAA2 pending K7 device acceptance");
            require(desktop.width == desktop.height * 2 && mobile.width == mobile.height * 2
                && desktop.colorFormat == RenderTextureFormat.ARGB32 && mobile.colorFormat == RenderTextureFormat.ARGB32, "Both preview targets preserve 2:1 aspect and alpha format");
            string source = File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/WeaponSandbox.cs");
            foreach (string api in new[] { "new CombatSimulation(arena)", "Simulation.Step(delta, Vector2.zero)", "view.Bind(state, arena)", "view.Display(Simulation.VisualElapsed, 1)", "effects.Play(value)", "effects.Advance(delta)", "audioFeedback.ConfigurePresentation()", "audioFeedback.PlayBatch(Simulation.Events)", "AddComponent<ProjectileVisual>()", "previewCamera.scene = previewScene", "SceneManager.UnloadSceneAsync(previewScene)", "previewCamera.Render()", "PreviewTexture.Release()", "forceRenderingOff = true" })
                require(source.Contains(api), "Shared pipeline/lifecycle wiring: " + api);
            foreach (string forbidden in new[] { "Run" + "Session", "Run" + "Storage", "SaveProfile(", "RunLaunch.", "Random.InitState", "SeenWeapons.Add", "SeenUnits.Add" })
                require(!source.Contains(forbidden), "No persistent/global dependency: " + forbidden);
            foreach (string api in new[] { "ReducedMotion = readOnlyProfile.ReducedMotion", "view.ReducedMotion = ReducedMotion", "GetPreviewDescriptor(Application.isMobilePlatform)", "previewCamera.aspect = 2f", "RenderTextureFormat.ARGB32", "new Rect(65, 300, 760, 300)" })
                require(source.Contains(api), "Reduced-motion/capture wiring: " + api);
        }

        private static void CheckPose(RunCatalog catalog, WeaponSandbox sandbox, Action<bool, string> require)
        {
            WeaponDefinition weapon = catalog.Weapons.First(definition => definition.Kind != WeaponKind.Contact && definition.Motion != WeaponMotion.Bow);
            sandbox.SelectWeapon(weapon.Id);
            sandbox.SetTier(3);
            Tick(sandbox, 3);
            GameObject instance = UnityEngine.Object.Instantiate(catalog.Arena.UnitPrefab);
            instance.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                UnitVisual view = instance.GetComponent<UnitVisual>();
                view.ReducedMotion = sandbox.ReducedMotion;
                view.Bind(sandbox.Attacker, catalog.Arena);
                view.Display(sandbox.Simulation.VisualElapsed, 1);
                SpriteRenderer[] parts = instance.GetComponentsInChildren<SpriteRenderer>(true);
                require(view.State == sandbox.Attacker && (Vector2)view.transform.position == sandbox.Attacker.Position, "Actual gameplay UnitVisual binds and displays sandbox pose");
                require(view.ReducedMotion && UnitVisual.IdleBeat(sandbox.Attacker, sandbox.Simulation.VisualElapsed, false, view.ReducedMotion) == 0,
                    "Gameplay reduced-motion property suppresses decorative idle while retaining attack pose");
                require(parts.Any(part => part.name == "WeaponVisual" && part.sprite == weapon.Sprite), "Pose renders the selected weapon, not the companion default");
                require(parts.Any(part => part.name == "HandVisual" && part.sprite == catalog.Arena.Hand), "Gameplay hand/grip pipeline retained");
                require(parts.Any(part => part.name == "BodyVisual" && part.sprite == sandbox.Attacker.Definition.Body), "Tier preview retains gameplay body silhouette");
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
    }
}
