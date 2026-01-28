using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed partial class AnimationPolishCapture
    {
        private readonly List<Renderer> shadowRenderers = new List<Renderer>();
        private IEnumerator ReviewEnsemble()
        {
            yield return ReviewSpecies(catalog.Companions, "companions", false);
            yield return ReviewSpecies(catalog.Enemies, "enemies-normal", false);
            yield return ReviewSpecies(catalog.Enemies, "enemies-evolved", true);
            for (int first = 0; first < catalog.Weapons.Length; first += 10)
            {
                ClearActors();
                for (int index = first; index < Mathf.Min(first + 10, catalog.Weapons.Length); index++) Add(catalog.Champions[index % catalog.Champions.Length], catalog.Weapons[index], index - first);
                pose = 2; started = fixtureClock; caption = "FULL WEAPON CATALOG / PAGE " + first / 10;
                yield return new WaitForSecondsRealtime(.1f); yield return Shot("all-weapons-" + first / 10 + "-load");
                yield return new WaitForSecondsRealtime(.15f); yield return Shot("all-weapons-" + first / 10 + "-release");
                foreach (UnitVisual actor in actors)
                {
                    WeaponPaintedAction action = actor.GetComponentInChildren<WeaponPaintedAction>();
                    if (EnsembleArtwork.Current.Weapon(actor.State.Definition.Weapon.Id) != null)
                    {
                        float age = fixtureClock - actor.State.AttackStarted;
                        WeaponDefinition definition = actor.State.Definition.Weapon;
                        bool active = !reduced && age >= 0 && age < definition.Windup + definition.Recovery;
                        Check(action != null && action.Visible == active, "Painted action follows actual phase " + definition.Id + " age " + age.ToString("0.000"));
                    }
                }
            }
            ClearActors(); pose = 0; caption = "FOUR MERCHANT IDENTITIES / PAINTED HANDOVER";
            var portraits = new List<SpriteRenderer>();
            foreach (EnsembleClip clip in EnsembleArtwork.Current.Merchants)
            {
                var child = new GameObject(clip.Id); child.transform.SetParent(stage.transform, false);
                SpriteRenderer view = child.AddComponent<SpriteRenderer>(); view.sharedMaterial = Resources.Load<Material>("UnitFlash"); view.gameObject.layer = 30; portraits.Add(view);
            }
            for (int beat = 0; beat < 3; beat++)
            {
                for (int index = 0; index < portraits.Count; index++)
                {
                    SpriteRenderer view = portraits[index]; view.sprite = EnsembleArtwork.Current.Merchants[index].At(.15f + beat * .3f);
                    float height = index < 2 ? 2.6f : 3.4f; view.transform.localScale = Vector3.one * (height / view.sprite.bounds.size.y);
                    view.transform.position = new Vector3(-6 + index * 4, -1.7f + height * .5f, 0);
                    var block = new MaterialPropertyBlock(); SpriteLighting.Apply(view, block, false, true); view.SetPropertyBlock(block);
                }
                yield return new WaitForSecondsRealtime(.1f); yield return Shot("merchant-catalog-" + beat);
            }
            foreach (SpriteRenderer portrait in portraits) Destroy(portrait.gameObject);
            Check(EnsembleArtwork.Current.Merchants.Length == 4, "All merchant body identities have authored delivery clips");
        }
        private IEnumerator ReviewSpecies(UnitDefinition[] sources, string group, bool evolved)
        {
            for (int first = 0; first < sources.Length; first += 10)
            {
                ClearActors();
                for (int index = first; index < Mathf.Min(first + 10, sources.Length); index++)
                {
                    UnitDefinition source = sources[index]; UnitDefinition variant = null;
                    if (evolved) { variant = Instantiate(source); EnemyEvolutionLibrary.Apply(variant, 100); source = variant; }
                    Add(source, source.Weapon, index - first, group == "companions" ? Faction.Companion : Faction.Enemy);
                    if (variant != null) Destroy(variant);
                }
                caption = group.ToUpperInvariant() + " / PAGE " + first / 10;
                pose = 0; started = fixtureClock; yield return new WaitForSecondsRealtime(.25f); yield return Shot(group + "-" + first / 10 + "-idle");
                pose = 1; started = fixtureClock; yield return new WaitForSecondsRealtime(.22f); yield return Shot(group + "-" + first / 10 + "-walk");
                pose = 2; started = fixtureClock; yield return new WaitForSecondsRealtime(.12f); yield return Shot(group + "-" + first / 10 + "-attack");
                foreach (UnitVisual actor in actors) Check(actor.DisplayedBody != null, "Catalog actor renders " + group + " " + actor.State.Definition.Id);
                if (group != "companions")
                {
                    foreach (UnitVisual actor in actors) { deaths.Play(actor, actor.State); actor.gameObject.SetActive(false); }
                    yield return new WaitForSecondsRealtime(.28f); yield return Shot(group + "-" + first / 10 + "-death");
                    Check(deaths.ActiveCount == actors.Count, "Species death pool coverage " + group + " " + first / 10); deaths.Clear();
                }
            }
        }
        private IEnumerator MeasureCrowd(RunStorage storage)
        {
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 761026); session.State.Wave = 17; session.State.Phase = SessionPhase.Combat; storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null; yield return new WaitForSecondsRealtime(1);
            ArenaController owner = FindAnyObjectByType<ArenaController>();
            Check(owner != null && owner.Simulation.Phase == RunPhase.Combat, "Actual combat scene opens for crowd measurement");
            UnitDefinition champion = Instantiate(owner.Simulation.Champion.Definition); definitions.Add(champion);
            owner.Simulation.Champion.Definition = champion; champion.Health = owner.Simulation.Champion.Health = 10000000;
            int previousRate = Application.targetFrameRate, previousSync = QualitySettings.vSyncCount;
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = 120;
            var result = new List<AnimationStressMeasurement>();
            for (int load = 0; load < 2; load++)
            {
                if (load == 1)
                {
                    for (int index = 0; index < 420; index++)
                    {
                        UnitDefinition enemy = Instantiate(catalog.Enemies[index % catalog.Enemies.Length]); enemy.Health = 500000; EnemyEvolutionLibrary.Apply(enemy, 100); definitions.Add(enemy);
                        float angle = index * 2.399963f; float radius = 4 + index % 9 * .65f;
                        owner.Simulation.AddAnimationStressUnit(enemy, owner.Simulation.Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
                    }
                }
                yield return new WaitForSecondsRealtime(2);
                var measurement = new AnimationStressMeasurement(); measurement.Begin(load == 0 ? "wave-17-combat" : "additional-420-enemies-hostile-stream");
                float until = Time.realtimeSinceStartup + (load == 0 ? 4 : 12); int sample = 0;
                while (Time.realtimeSinceStartup < until)
                {
                    if (load == 1 && sample % 6 == 0 && owner.Simulation.Projectiles.Count < 320)
                        for (int index = 0; index < 24; index++) { float angle = index * 2.399963f + sample; Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)); owner.Simulation.AddAnimationStressShot(owner.Simulation.Champion.Position + direction * 5, -direction * 2); }
                    owner.SetPresentationMoveTarget(new Vector2(Mathf.Cos(Time.unscaledTime), Mathf.Sin(Time.unscaledTime)) * 2);
                    yield return null; measurement.Sample(owner.Simulation); sample++;
                }
                measurement.Finish(); result.Add(measurement); yield return Shot("performance-" + load);
                Check(measurement.Samples > 0 && double.IsFinite(measurement.P95Milliseconds), "Measured real combat frame timing " + load);
            }
            string json = "[" + JsonUtility.ToJson(result[0], true) + "," + JsonUtility.ToJson(result[1], true) + "]";
            File.WriteAllText(Path.Combine(root, "Performance.json"), json);
            Check(result[1].PeakUnits >= 400, "Dense native fixture exercised at least four hundred real units");
            Application.targetFrameRate = previousRate; QualitySettings.vSyncCount = previousSync;
            owner.SetPresentationMoveTarget(null); owner.gameObject.SetActive(false); AnimationDetailBudget.SetLoad(0, 0);
            foreach (UnitDefinition definition in definitions) Destroy(definition); definitions.Clear();
        }
    }
}
