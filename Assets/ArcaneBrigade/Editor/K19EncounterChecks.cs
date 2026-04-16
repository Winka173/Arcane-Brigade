using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K19EncounterChecks
    {
        private static readonly MethodInfo Damage = typeof(CombatSimulation).GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.NonPublic);
        private static void Hit(CombatSimulation simulation, UnitState target, float damage, Vector2 direction, Faction source = Faction.Companion)
            => Damage.Invoke(simulation, new object[] { target, damage, source, "K19 fixture", direction, null, false, DamageElement.Physical, default(CombatCause), false });
        public static void Verify()
        {
            Checks(); K18ExperienceBuilder.Checks();
            SpriteInkMetricsBuilder.Audit(); ShopLayerChecks.Verify();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K19/Windows_Build.txt", true);
        }
        public static void Campaign() => K17CampaignDiagnostics.RunInto("PreviewExports/K19");
        [MenuItem("Arcane Brigade/K19/Encounter checks")]
        public static void Checks()
        {
            Directory.CreateDirectory("PreviewExports/K19");
            var notes = new List<string>();
            void Require(bool condition, string caption)
            {
                if (!condition) throw new InvalidOperationException("K19 " + caption);
                notes.Add("PASS " + caption);
            }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            EvolutionArtwork art = Resources.Load<EvolutionArtwork>("EvolutionArtwork");
            Require(art != null && art.Parts.Length == 12 && art.Parts.All(sprite => sprite != null), "Twelve original transparent organ and evolution sprites are linked");
            Require(art.Parts.Select(sprite => sprite.rect).Distinct().Count() == 12, "Artwork regions are independently sliced, not a full-sheet placeholder");
            Require(catalog.Items.All(item => item.Icon != null), "Every relic has a real icon");
            Require(catalog.Items.Select(item => item.Icon).Distinct().Count() == catalog.Items.Length, "No two relics share an icon");
            var unitSprites = new HashSet<Sprite>(catalog.Companions.Concat(catalog.Champions).Concat(catalog.Enemies).Concat(catalog.Bosses).Select(unit => unit.Body));
            var weaponSprites = new HashSet<Sprite>(catalog.Weapons.Select(weapon => weapon.Sprite));
            Require(catalog.Items.All(item => !unitSprites.Contains(item.Icon) && !weaponSprites.Contains(item.Icon)), "Relics cannot reuse an ally, enemy, hero, boss or weapon sprite");
            Require(catalog.Items.All(item => !AssetDatabase.GetAssetPath(item.Icon).Contains("Characters") && !AssetDatabase.GetAssetPath(item.Icon).Contains("Weapons")), "Retired character/weapon atlas art cannot masquerade as relics");
            Require(catalog.Items.Count(item => AssetDatabase.GetAssetPath(item.Icon).Contains("Art/K19/Relics")) == 58, "Fifty-eight formerly reused relic illustrations replaced; ten dedicated relics retained");
            for (int wave = 1; wave <= 40; wave++)
            {
                float oldHealth = 1.2f * (1 + Mathf.Pow(Mathf.Max(0, wave - 1) / 12f, 1.45f)) * (1 + Mathf.Pow(Mathf.Max(0, wave - 4) / 16f, 1.4f) * 1.4f);
                float oldSpeed = 1.08f * (1 + 1.8f * (1 - Mathf.Exp(-Mathf.Max(0, wave - 1) * 0.05f)));
                float oldDamage = 1 + Mathf.Min(3, Mathf.Max(0, wave - 3) * 0.09f);
                Require(Mathf.Abs(CarnivalBalance.EnemyHealth(wave) / oldHealth - 1.2f) < 0.001f, "All enemy HP +20% over pre-22/10 tuning, wave " + wave);
                Require(CarnivalBalance.EnemySpeed(wave) <= oldSpeed && (wave < 6 || CarnivalBalance.EnemySpeed(wave) < oldSpeed * .8f), "Gentler enemy speed growth, wave " + wave);
                Require(Mathf.Abs(CarnivalBalance.EnemyDamage(wave) / oldDamage - 1.15f) < 0.001f, "All enemy attack scaling +15%, wave " + wave);
                Require(CarnivalBalance.BossBulk(wave) >= Mathf.Lerp(4, 6.5f, Mathf.Clamp01((wave - 4) / 16f)) * 1.149f, "Boss additional bulk at least +15%, wave " + wave);
                if (wave > 1) Require(CarnivalBalance.EnemyHealth(wave) / CarnivalBalance.EnemyHealth(wave - 1) < 1.12f, "No HP cliff, wave " + wave);
            }
            foreach (UnitDefinition source in catalog.Bosses)
            {
                using var session = new RunSession(catalog); session.Start("LanternKnight", 19077); session.State.Wave = 4;
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = source; arena.EnemyBudget = 0;
                var simulation = new CombatSimulation(arena); simulation.Start(19077);
                UnitState boss = simulation.Boss; boss.EntranceUntil = 0;
                float originalHealth = boss.Health;
                Hit(simulation, boss, BossPartRules.Durability(boss) * 0.5f, Vector2.right);
                Require(!boss.LeftPartBroken && !boss.RightPartBroken, "Part survives partial damage " + source.Id);
                Hit(simulation, boss, BossPartRules.Durability(boss) * 0.51f, Vector2.right);
                Require(boss.LeftPartBroken && !boss.RightPartBroken, "Left organ breaks independently " + source.Id);
                Require(BossPartRules.BulletCount(boss) > 0, "Boss keeps a readable core volley after side break " + source.Id);
                Require(BossPartRules.NextOrgan(boss) != 0, "Broken organ is excluded from attack selection " + source.Id);
                Require(!boss.RightPartBroken && boss.CenterPartBrokenAt < 0, "Other organs remain independently intact " + source.Id);
                Hit(simulation, boss, BossPartRules.Durability(boss) * 1.01f, Vector2.left);
                Require(boss.RightPartBroken && boss.CenterPartBrokenAt < 0, "Right side breaks without destroying the centre " + source.Id);
                Require(boss.Alive && boss.Health > originalHealth * 0.5f, "Breaking both is not instant boss kill " + source.Id);
                Require(simulation.Events.Count(value => value.Kind == CombatEventKind.BossPhase && value.Subject == boss) == 2, "Exactly two attributed break events " + source.Id);
                Hit(simulation, boss, 1, Vector2.left);
                Require(simulation.Events.Count(value => value.Kind == CombatEventKind.BossPhase && value.Subject == boss) == 2, "Broken part cannot retrigger rewards or effects " + source.Id);
                boss.NextBossPattern = 0; boss.TelegraphUntil = 0;
                simulation.Champion.NextAttack = float.PositiveInfinity;
                foreach (UnitState ally in simulation.Units.Where(unit => unit.Faction == Faction.Companion)) ally.NextAttack = float.PositiveInfinity;
                for (int tick = 0; tick < 22; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(simulation.ExpandedFields.Any(field => field.Source == boss && field.Kind == ExpandedFieldKind.Shockwave), "Exposed core gains telegraphed shockwave " + source.Id);
                simulation.EnterRest(); Require(simulation.ExpandedFields.Count == 0, "Shop clears boss hazards " + source.Id);
            }
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 19077); session.State.Wave = 5;
                CheckpointCatalog arena = session.PrepareCombat(); arena.Boss = null; arena.EnemyBudget = 0;
                arena.EnableHeroMechanics = arena.EnableArenaToys = arena.EnableBossArenaPhase = arena.EnableSpectacles = false;
                var simulation = new CombatSimulation(arena); simulation.Start(19077);
                foreach (UnitState unit in simulation.Units) unit.NextAttack = unit.NextContact = float.PositiveInfinity;
                simulation.Champion.Position = new Vector2(5, 3);
                UnitDefinition heavy = UnityEngine.Object.Instantiate(arena.MeleeEnemy);
                UnitDefinition puffer = UnityEngine.Object.Instantiate(arena.MeleeEnemy);
                heavy.BodyHeight = 1.6f; heavy.Speed = 0; heavy.Health = 100; heavy.Behavior = EnemyBehavior.Standard;
                puffer.Behavior = EnemyBehavior.Puffer; puffer.Speed = 0; puffer.Health = 10;
                UnitState wall = simulation.AddFixture(heavy, Faction.Enemy, Vector2.zero);
                UnitState bomb = simulation.AddFixture(puffer, Faction.Enemy, Vector2.right * 0.4f);
                wall.NextAttack = wall.NextContact = bomb.NextAttack = bomb.NextContact = float.PositiveInfinity;
                bomb.NextBehavior = wall.NextBehavior = float.PositiveInfinity;
                var projectile = new ProjectileState { Faction = Faction.Enemy, SourceUnitId = bomb.Id };
                Require(EnemyInteractionRules.CanBlock(projectile, wall), "Large enemy intercepts other enemy shots");
                projectile.SourceUnitId = wall.Id; Require(!EnemyInteractionRules.CanBlock(projectile, wall), "Enemy never intercepts its own shot");
                Require(!EnemyInteractionRules.CanBlock(projectile, simulation.Champion), "Player uses ordinary collision, not friendly blocker path");
                float health = wall.Health;
                simulation.DamageFixture(bomb, 100);
                for (int tick = 0; tick < 11; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(wall.Health == health, "Puffer death blast warns before collateral damage");
                for (int tick = 0; tick < 7; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(wall.Health < health && wall.Health >= health * 0.88f, "Puffer damages neighbours, capped at 12% max health");
                float after = wall.Health;
                for (int tick = 0; tick < 12; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(wall.Health == after, "One blast cannot repeatedly hit the same enemy");
                var liveShots = (List<ProjectileState>)typeof(CombatSimulation).GetField("projectiles", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(simulation);
                liveShots.Add(new ProjectileState { Id = 9001, Faction = Faction.Enemy, SourceUnitId = 9999, Position = Vector2.left * 1.5f, PreviousPosition = Vector2.left * 1.5f, Velocity = Vector2.right * 2, LaunchVelocity = Vector2.right * 2, Damage = 10, Radius = 0.1f, Remaining = 3 });
                for (int tick = 0; tick < 20; tick++) simulation.Step(0.05f, Vector2.zero);
                Require(wall.Health < after && wall.Health >= after - 1.51f, "Actual swept hostile shot damages the blocker with a 1.5% HP cap");
                Require(liveShots.All(shot => shot.Id != 9001), "Intercepted shot is consumed, not split or ricocheted");
                simulation.EnterRest(); Require(simulation.ExpandedFields.Count == 0, "Shop clears death reaction fields");
                UnityEngine.Object.DestroyImmediate(heavy); UnityEngine.Object.DestroyImmediate(puffer);
            }
            File.WriteAllLines("PreviewExports/K19/Focused_Checks.txt", notes);
            Debug.Log("K19_CHECKS_PASSED " + notes.Count);
        }
    }
}
