using System.Collections;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class AnimationPolishCapture
    {
        private IEnumerator ReviewExpandedPerformance()
        {
            ExpandedPerformanceLibrary library = ExpandedPerformanceLibrary.Current;
            Check(library != null, "Actual player contains expanded performance library");
            yield return ReviewFinalQualityProps();
            if (FinalQualityArtwork.Current != null) yield return ReviewIdentityBosses();
            foreach (string kind in new[] { "companions", "locomotion", "enemies" }) yield return ReviewExpandedActors(library, kind);
            ExpandedPerformanceEntry[] bosses = library.Entries.Where(entry => entry.Kind == "bosses").ToArray();
            Check(bosses.Length == 12, "All twelve boss identities have painted reactions");
            foreach (ExpandedPerformanceEntry entry in bosses)
            {
                ClearActors(); deaths.Clear();
                UnitDefinition boss = catalog.Bosses.Concat(new[] { NightmareAssets.Current.Customer, OutsideStageAssets.Current.Boss }).First(definition => definition.Id == entry.Id);
                Add(boss, boss.Weapon, 0, Faction.Enemy);
                UnitVisual actor = actors[0]; origins[0] = Vector2.zero;
                actor.transform.Find("ReviewCaption").localPosition = new Vector3(0, -2.5f, 0);
                actor.State.Definition.BodyHeight = 3.6f;
                string[] phases = { "anticipation", "release", "recoil" };
                for (int index = 0; index < phases.Length; index++)
                {
                    freeze = true;
                    UnitState unit = actor.State; float clock = 10 + index;
                    unit.Position = unit.PreviousPosition = Vector2.zero; unit.Facing = unit.AttackDirection = Vector2.right;
                    unit.EntranceUntil = 0; unit.PendingAttack = index == 0; unit.TelegraphUntil = index == 0 ? clock + .5f : 0;
                    unit.BossRecoveryUntil = index == 1 ? clock + .5f : 0;
                    unit.AttackStarted = index == 0 ? clock : -100; unit.LastHit = index == 2 ? clock - .05f : -100;
                    actor.Display(clock, 1);
                    foreach (Renderer renderer in stage.GetComponentsInChildren<Renderer>(true)) renderer.gameObject.layer = 30;
                    Check(reduced || actor.BodyRenderer.sprite == entry.Frames[index], "Boss actual " + phases[index] + " selects its own painted frame " + entry.Id);
                    Check(actor.BodyRenderer.bounds.size.y > 1 && actor.BodyRenderer.bounds.size.y < 7, "Boss pose has bounded world scale " + entry.Id + "/" + index);
                    caption = entry.Id + " / " + phases[index].ToUpperInvariant();
                    yield return Shot("boss-" + entry.Id + "-" + phases[index]);
                }
                actor.State.LastHit = -100;
                if (boss.BossPattern % 2 == 0)
                {
                    actor.State.PendingAttack = false; actor.State.TelegraphUntil = actor.State.BossRecoveryUntil = 0;
                    actor.State.AttackStarted = -100; actor.Display(13, 1);
                }
                deaths.Play(actor, actor.State); actor.gameObject.SetActive(false);
                Check(deaths.ActiveCount == 1, "Boss death uses real remnant pool " + entry.Id);
                deaths.Advance(.12f); LayerExpandedRemnants();
                caption = entry.Id + " / DEATH RECOIL"; yield return Shot("boss-" + entry.Id + "-death-recoil");
                deaths.Advance(.45f); LayerExpandedRemnants();
                SpriteRenderer corpse = deaths.GetComponentsInChildren<SpriteRenderer>().First(view => view.name == "Body");
                Check(reduced || corpse.sprite == entry.Frames[3], "Boss finale selects painted collapsed pose " + entry.Id);
                Check(corpse.bounds.size.y > .3f && corpse.bounds.size.y < 7, "Boss painted collapse remains readable " + entry.Id);
                caption = entry.Id + " / PAINTED COLLAPSE"; yield return Shot("boss-" + entry.Id + "-death-collapse");
                deaths.Clear(); freeze = false;
            }
            ExpandedPerformanceEntry[] weaponEntries = library.Entries.Where(entry => entry.Kind == "weapons").ToArray();
            foreach (ExpandedPerformanceEntry entry in weaponEntries)
            {
                ClearActors();
                WeaponDefinition weapon = catalog.Weapons.First(definition => definition.Id == entry.Id);
                Add(catalog.Champions[0], weapon, 0, Faction.Champion); origins[0] = Vector2.zero;
                UnitVisual actor = actors[0]; actor.State.Definition.BodyHeight = 2;
                actor.transform.Find("ReviewCaption").localPosition = new Vector3(0, -2.5f, 0);
                actor.State.Definition.Weapon.VisualWidth = 3;
                freeze = true;
                for (int phase = 0; phase < 2; phase++)
                {
                    float age = phase == 0 ? weapon.Windup * .5f : weapon.Windup + weapon.Recovery * .4f;
                    actor.State.Position = actor.State.PreviousPosition = Vector2.zero;
                    actor.State.Facing = actor.State.AttackDirection = Vector2.right;
                    actor.State.AttackStarted = 20 - age; actor.State.AttackWindup = weapon.Windup;
                    actor.State.PendingAttack = phase == 0; actor.State.LastHit = -100;
                    actor.Display(20, 1);
                    foreach (Renderer renderer in stage.GetComponentsInChildren<Renderer>(true)) renderer.gameObject.layer = 30;
                    WeaponPaintedAction action = actor.GetComponentInChildren<WeaponPaintedAction>();
                    Check(action != null && action.Visible == !reduced, "Actual weapon action uses painted clip and Reduced fallback " + entry.Id + "/" + phase);
                    WeaponMechanism mechanism = actor.GetComponentInChildren<WeaponMechanism>();
                    Check(action == null || !action.Visible || mechanism == null || mechanism.VisibleParts == 0, "Painted action does not duplicate legacy moving pieces " + entry.Id);
                    if (!reduced)
                    {
                        SpriteRenderer view = action.GetComponentsInChildren<SpriteRenderer>().First(renderer => renderer.name == "PaintedMechanicalAction");
                        Check(entry.Frames.Contains(view.sprite), "Weapon mechanism never borrows another identity " + entry.Id);
                        Check(view.bounds.size.magnitude > .1f && view.bounds.size.magnitude < 8, "Weapon pose has bounded visible scale " + entry.Id);
                    }
                    caption = entry.Id + (phase == 0 ? " / MECHANICAL WINDUP" : " / MECHANICAL RELEASE");
                    yield return Shot("weapon-" + entry.Id + "-" + phase);
                }
                freeze = false;
            }
            ClearActors(); deaths.Clear(); reviewing = false; stage.SetActive(false);
        }
        private void LayerExpandedRemnants()
        {
            foreach (Renderer renderer in stage.GetComponentsInChildren<Renderer>(true)) renderer.gameObject.layer = 30;
            shadowRenderers.Clear();
            foreach (GroundShadow shadow in stage.GetComponentsInChildren<GroundShadow>(true)) shadow.AddRenderers(shadowRenderers);
            foreach (Renderer renderer in shadowRenderers) renderer.gameObject.layer = 30;
        }
        private IEnumerator ReviewFinalQualityProps()
        {
            FinalQualityArtwork artwork = FinalQualityArtwork.Current;
            if (artwork == null) yield break;
            Check(artwork.Props.Length == 20, "All redrawn story and architecture props included");
            ClearActors(); freeze = true;
            var gallery = new GameObject("FinalQualityPropGallery"); gallery.transform.SetParent(stage.transform, false);
            var renderers = new SpriteRenderer[4];
            for (int index = 0; index < renderers.Length; index++)
            {
                var child = new GameObject("PropReview" + index); child.transform.SetParent(gallery.transform, false); child.layer = 30;
                renderers[index] = child.AddComponent<SpriteRenderer>(); renderers[index].sortingOrder = 100;
            }
            for (int first = 0; first < artwork.Props.Length; first += 4)
                for (int phase = 0; phase < 4; phase++)
                {
                    for (int index = 0; index < 4; index++)
                    {
                        SpriteRenderer view = renderers[index]; var entry = artwork.Props[first + index];
                        view.sprite = entry.Frames[reduced ? 0 : phase];
                        float scale = Mathf.Min(5.2f / view.sprite.bounds.size.x, 3.5f / view.sprite.bounds.size.y);
                        view.transform.localScale = Vector3.one * scale;
                        view.transform.position = new Vector3(index % 2 == 0 ? -4 : 4, index < 2 ? 2.3f : -2.3f, 0) - Vector3.Scale(view.sprite.bounds.center, view.transform.localScale);
                        Check(view.sprite != null && view.bounds.size.y > .1f && view.bounds.size.x < 5.3f, "Native complete prop pose " + entry.Id + "/" + phase);
                    }
                    caption = "REDRAWN DREAM FURNITURE / PAGE " + first / 4 + " / POSE " + phase;
                    yield return Shot("final-props-" + first / 4 + "-" + phase);
                }
            gallery.SetActive(false); Destroy(gallery); freeze = false;
        }
        private IEnumerator ReviewIdentityBosses()
        {
            foreach (UnitDefinition source in catalog.Bosses.Concat(new[] { OutsideStageAssets.Current.Boss, NightmareAssets.Current.Customer }))
            {
                using (var session = new RunSession(catalog))
                {
                    ClearActors(); freeze = true;
                    session.Start("LanternKnight", 161026); session.State.Wave = 12;
                    CheckpointCatalog combat = session.PrepareCombat(); combat.Boss = Instantiate(source); definitions.Add(combat.Boss); combat.EnemyBudget = 0;
                    var simulation = new CombatSimulation(combat); simulation.Start(161026);
                    UnitState boss = simulation.Boss; boss.EntranceUntil = 0; boss.Position = boss.PreviousPosition = new Vector2(0, 2.8f);
                    boss.NextBossPattern = float.PositiveInfinity; boss.Definition.Speed = 0;
                    simulation.Champion.Health = 1000000; simulation.Champion.NextAttack = float.PositiveInfinity;
                    simulation.ReviewIdentityEncore(0);
                    Add(boss.Definition, boss.Definition.Weapon, 0, Faction.Enemy);
                    actors[0].Bind(boss, catalog.Arena, true); actors[0].ReducedMotion = reduced;
                    var displayObject = new GameObject("IndividualBossChoreography"); displayObject.transform.SetParent(stage.transform, false);
                    ExpandedVisual display = displayObject.AddComponent<ExpandedVisual>(); display.ReducedMotion = reduced;
                    for (int phase = 0; phase < 3; phase++)
                    {
                        while (simulation.Elapsed < new[] { .35f, 1.2f, 1.7f }[phase]) simulation.Step(.05f, Vector2.zero);
                        actors[0].Display(simulation.Elapsed, 1); display.Display(simulation);
                        LayerExpandedRemnants();
                        Check(display.VisibleCount > 0, "Actual individual boss hazards rendered " + source.Id + "/" + phase);
                        caption = source.Id + " / INDIVIDUAL ENCORE / " + phase;
                        yield return Shot("identity-boss-" + source.Id + "-" + phase);
                    }
                    displayObject.SetActive(false); Destroy(displayObject); freeze = false;
                }
            }
            ClearActors();
        }
        private IEnumerator ReviewExpandedActors(ExpandedPerformanceLibrary library, string kind)
        {
            ExpandedPerformanceEntry[] entries = library.Entries.Where(entry => entry.Kind == kind).ToArray();
            for (int first = 0; first < entries.Length; first += 8)
            {
                ClearActors(); deaths.Clear(); freeze = true;
                int count = Mathf.Min(8, entries.Length - first);
                for (int index = 0; index < count; index++)
                {
                    ExpandedPerformanceEntry entry = entries[first + index];
                    UnitDefinition source = catalog.Champions.Concat(catalog.Companions).Concat(catalog.Enemies).First(definition => definition.Id == entry.Id);
                    UnitDefinition appearance = Instantiate(source);
                    definitions.Add(appearance);
                    if (kind == "enemies" && entry.Evolution > 0) { EnemyEvolutionLibrary.Apply(appearance, 100); EnemyAffixLibrary.ApplyArt(appearance, 100); }
                    Faction faction = kind == "enemies" ? Faction.Enemy : catalog.Champions.Contains(source) ? Faction.Champion : Faction.Companion;
                    Add(appearance, appearance.Weapon, index, faction);
                    actors[index].State.Definition.EvolutionStage = entry.Evolution;
                    actors[index].State.Definition.BodyHeight = 1.55f;
                    actors[index].Bind(actors[index].State, catalog.Arena, true);
                    actors[index].ReducedMotion = reduced;
                    origins[index] = new Vector2(-6.3f + index % 4 * 4.2f, index < 4 ? 2.1f : -1.8f);
                    actors[index].transform.Find("ReviewCaption").localPosition = new Vector3(0, -1.15f, 0);
                    Check(library.Find(kind, entry.Id, entry.Evolution) == entry, "Actual actor binds its exact identity and evolution " + kind + "/" + entry.Id + "/" + entry.Evolution);
                }
                int phases = kind == "locomotion" ? 8 : 4;
                for (int phase = 0; phase < phases; phase++)
                {
                    for (int index = 0; index < count; index++)
                    {
                        ExpandedPerformanceEntry entry = entries[first + index];
                        UnitVisual actor = actors[index]; UnitState unit = actor.State;
                        float clock = 20 + phase * 2;
                        unit.Position = unit.PreviousPosition = origins[index]; unit.Velocity = Vector2.zero;
                        unit.PendingAttack = false; unit.AttackStarted = unit.LastHit = -100;
                        unit.Facing = unit.AttackDirection = Vector2.right;
                        actor.PreviewActing(-1, 0, false);
                        Sprite expected;
                        if (kind == "companions")
                        {
                            actor.PreviewActing(phase, .5f); expected = entry.Frames[phase];
                        }
                        else if (kind == "locomotion")
                        {
                            if (phase == 0 || phase == 1) clock = 4.7f * 100 - unit.Id * .73f + (phase == 1 ? .02f : .3f);
                            if (phase >= 2 && phase <= 4)
                            {
                                clock = (81 + phase - 2 - unit.Id * .37f) / 8f + .01f;
                                unit.PreviousPosition = unit.Position - Vector2.right * .01f; unit.Velocity = Vector2.right;
                            }
                            if (phase == 5) { unit.PendingAttack = true; unit.AttackStarted = clock; }
                            if (phase >= 6) unit.AttackStarted = clock - unit.Definition.Weapon.Windup - unit.Definition.Weapon.Recovery * (phase == 6 ? .25f : .8f);
                            expected = entry.Frames[phase];
                        }
                        else
                        {
                            if (phase == 0) { unit.PendingAttack = true; unit.AttackStarted = clock; }
                            if (phase == 1) unit.AttackStarted = clock - unit.Definition.Weapon.Windup - unit.Definition.Weapon.Recovery * .2f;
                            if (phase == 2) unit.LastHit = clock - .05f;
                            expected = phase == 3 ? null : entry.Frames[phase];
                        }
                        actor.Display(clock, 1);
                        if (kind == "enemies" && phase == 3)
                        {
                            deaths.Play(actor, unit); actor.gameObject.SetActive(false);
                        }
                        else
                        {
                            Check(reduced || actor.BodyRenderer.sprite == expected, "Actual painted pose selected " + kind + "/" + entry.Id + "/" + entry.Evolution + "/" + phase);
                            Check(reduced || actor.BodyRenderer.enabled, "Painted pose is actually visible, not hidden by body puppet " + entry.Id + "/" + phase);
                            Check(actor.BodyRenderer.bounds.size.y > .35f && actor.BodyRenderer.bounds.size.y < 3.8f, "Expanded pose keeps readable world scale " + entry.Id + "/" + phase);
                        }
                    }
                    if (kind == "enemies" && phase == 3)
                    {
                        deaths.Advance(.45f);
                        foreach (SpriteRenderer corpse in deaths.GetComponentsInChildren<SpriteRenderer>().Where(view => view.name == "Body" && view.enabled))
                            Check(reduced || entries.Skip(first).Take(count).Any(entry => entry.Frames[3] == corpse.sprite), "Enemy pooled death displays its painted collapse");
                        Check(deaths.ActiveCount == count, "Every normal/evolved enemy has an actual pooled death");
                    }
                    LayerExpandedRemnants(); caption = kind.ToUpperInvariant() + " / PAGE " + first / 8 + " / POSE " + phase;
                    yield return Shot(kind + "-" + first / 8 + "-" + phase);
                }
                freeze = false;
            }
            ClearActors(); deaths.Clear();
        }
    }
}
