using System;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSession
    {
        private void ApplySecretUnit(UnitDefinition unit, bool companion)
        {
            if (State.Secrets == null) return;
            WeaponDefinition weapon = unit.Weapon;
            foreach (SecretStitch stitch in Secret.Stitches)
            {
                if (StackCount(stitch.ItemId) == 0) continue;
                if (stitch.Pattern == 0 && weapon.Kind == WeaponKind.Projectile)
                {
                    weapon.SecretRecall = true; weapon.ProjectilePierce = 0; weapon.Damage *= 1.15f;
                    weapon.ProjectileLifetime = Mathf.Max(2, weapon.ProjectileLifetime);
                }
                if (stitch.Pattern == 1 && weapon.Kind == WeaponKind.Melee) weapon.Cooldown *= 1.2f;
            }
            if (companion && Secret.AllyGifts.Exists(g => g.UnitId == unit.Id && g.WeaponId == weapon.Id)) weapon.Damage *= 1.2f;
        }
        private void PrepareSecretCombat()
        {
            if (State.Secrets == null || SecretEventAssets.Current == null) return;
            SecretEventAssets assets = SecretEventAssets.Current;
            CombatCatalog.SecretMeleeEcho = Secret.Stitches.Exists(t => t.Pattern == 1 && StackCount(t.ItemId) > 0);
            CombatCatalog.SecretWard = Secret.Stitches.Exists(t => t.Pattern == 2 && StackCount(t.ItemId) > 0);
            CombatCatalog.SecretMirrorEcho = StackCount("SecretMirror") > 0 && !SecretItemSuspended("SecretMirror");
            if (!SecretSolo)
            {
                var allies = CombatCatalog.StartingCompanions.ToList();
                if (Secret.Contracts.Exists(c => c.Kind == SecretContractKind.BorrowedShadow))
                {
                    UnitDefinition shadow = BuildUnit(catalog.Unit(State.ChampionId), catalog.Unit(State.ChampionId).Weapon, 1, true);
                    shadow.Id = "SecretTemporaryShadow"; shadow.DisplayName = "Borrowed Reflection"; shadow.Tint = new Color(.58f, .8f, 1, .8f);
                    shadow.HeroMechanic = HeroMechanic.None; shadow.Weapon.Damage *= .5f; shadow.Passive = PassiveKind.None; allies.Add(shadow);
                }
                if (Secret.Contracts.Exists(c => c.Kind == SecretContractKind.DreamFriend))
                {
                    UnitDefinition friend = assets.Rescues[2]; allies.Add(BuildUnit(friend, friend.Weapon, 1, true));
                }
                CombatCatalog.StartingCompanions = allies.ToArray();
            }
            if (!SecretFighting) return;
            CombatCatalog.EnableArenaToys = CombatCatalog.EnableSpectacles = CombatCatalog.EnableBossArenaPhase = false;
            CombatCatalog.ArenaSize = new Vector2(22, 12); CombatCatalog.ExtendedLoot = false; CombatCatalog.GoldPerKill = 0;
            CombatCatalog.ChestInterval = 100000; CombatCatalog.EnemyBudget = 0; CombatCatalog.BossSwarmBudget = 0; CombatCatalog.WaveDuration = 0; // no boss-night swarm in a secret fight
            CombatCatalog.EnemyPool = Array.Empty<UnitDefinition>(); CombatCatalog.AffixedEnemies = Array.Empty<UnitDefinition>();
            CombatCatalog.Boss = null;
            int encounter = Secret.Kind == SecretEventKind.LostProperty ? 1 : Secret.Kind == SecretEventKind.Understudy ? 2 : Secret.Kind == SecretEventKind.ContrabandTrain ? 3 : 4 + Secret.Option;
            CombatCatalog.SecretEncounter = encounter;
            if (encounter == 4)
            {
                UnitDefinition paper = catalog.Enemies.FirstOrDefault(e => e.Material == BodyMaterial.Paper && !e.IsBoss) ?? catalog.Enemies[0];
                UnitDefinition enemy = BuildEnemy(paper, 1 + State.Wave * .12f);
                enemy.Health *= .65f; enemy.SpecialEnemy = SpecialEnemyKind.None; enemy.Behavior = EnemyBehavior.Standard;
                CombatCatalog.MeleeEnemy = CombatCatalog.RangedEnemy = enemy; CombatCatalog.EnemyPool = new[] { enemy };
                CombatCatalog.EnemyBudget = 32; CombatCatalog.SpawnInterval = .7f; CombatCatalog.WaveDuration = 28; return;
            }
            UnitDefinition source = encounter == 2 ? CombatCatalog.Champion : assets.Guardian;
            UnitDefinition actor = Clone(source); actor.Weapon = Clone(source.Weapon);
            actor.Id = encounter == 2 ? "SecretUnderstudy" : "SecretCabinetActor";
            actor.DisplayName = encounter == 2 ? "The Understudy" : encounter == 3 ? "The Contraband Collector" : encounter == 5 ? "The Leading Actor" : encounter == 6 ? "The Hostile Duet" : "The Cabinet Warden";
            actor.IsBoss = true; actor.HeroMechanic = HeroMechanic.None; actor.SpecialEnemy = SpecialEnemyKind.None; actor.Mutation = EnemyMutation.None;
            actor.Health = (650 + State.Wave * 140) * .8f * (1 + State.Difficulty * .2f) * (encounter == 5 ? 1.3f : 1);
            actor.BodyHeight = encounter == 2 ? 1.5f : 2.3f; actor.Radius = .65f; actor.Speed = encounter == 2 ? 1.15f : .6f;
            actor.ContactDamage = (5 + State.Wave * .12f)*1.08f*1.6f; actor.ContactInterval = 1.17f; actor.KnockbackResistance = 1;
            actor.Weapon.Damage*=1.08f;
            actor.Tint = encounter == 2 ? new Color(.65f, .8f, 1) : Color.white;
            CombatCatalog.Boss = actor;
            if (encounter == 6)
            {
                UnitDefinition partner = BuildEnemy(catalog.Enemies.FirstOrDefault(e => e.Weapon.Kind == WeaponKind.Projectile) ?? catalog.Enemies[0], 1 + State.Wave * .1f);
                partner.Id = "SecretSupportingActor"; partner.DisplayName = "The Supporting Actor";
                partner.Health = actor.Health * .5f; partner.SpecialEnemy = SpecialEnemyKind.None; partner.Mutation = EnemyMutation.None;
                partner.Weapon.Cooldown = 2.8f; partner.Weapon.ProjectileCount = 2; partner.Weapon.Damage = 7;
                CombatCatalog.SecretPartner = partner;
            }
        }
        private bool SettleSecretCombat(CombatSimulation simulation)
        {
            if (!SecretFighting) return false;
            State.Health = Mathf.Min(MaximumHealth(), simulation.Champion.Health); State.CombatSeconds += simulation.Elapsed;
            State.LastDamageSource = simulation.LastDamageSource; State.LastDamageKind = simulation.LastDamageKind; State.LastDamageAmount = simulation.LastDamageAmount;
            ContributionLedger.Merge(State.Contributions, simulation.Contributions);
            if (simulation.Phase == RunPhase.Defeat) { State.Phase = SessionPhase.Defeat; return true; }
            Secret.Stage = SecretEventStage.Reward; State.Phase = SessionPhase.Shop;
            Secret.Result = "THE PERFORMANCE IS OVER\nClaim your chosen gift. Your normal shop, stock and wave are waiting unchanged. Health spent in this encounter stays spent.";
            RecordSecret("Won the optional encounter."); return true;
        }
    }

    public sealed partial class CombatSimulation
    {
        private float secretChampionAttack = -100, secretMirrorAt;
        private Vector2 secretMirrorDirection;
        private UnitState secretSecondActor;
        private float secretNextEcho;
        private void StartSecretCombat()
        {
            secretChampionAttack = -100; secretMirrorAt = 0; secretNextEcho = 0; secretSecondActor = null;
            if (catalog.SecretPartner != null)
            {
                secretSecondActor = Spawn(catalog.SecretPartner, Faction.Enemy, new Vector2(-3, 2));
                secretSecondActor.NextAttack = 3.5f;
            }
        }
        private bool SecretVictoryReady() => catalog.SecretEncounter == 6 ? BossDefeated && (secretSecondActor == null || !secretSecondActor.Alive) : catalog.Boss == null ? Elapsed >= catalog.WaveDuration : BossDefeated;

        private bool ProcessSecretBoss(UnitState boss)
        {
            if (catalog.SecretEncounter == 0) return false;
            boss.BossPoseTime = Elapsed;
            if (catalog.SecretEncounter == 2)
            {
                if (secretMirrorAt == 0 && Champion.AttackStarted > secretChampionAttack && Champion.AttackStarted > .05f)
                {
                    secretChampionAttack = Champion.AttackStarted; secretMirrorAt = Elapsed + .65f;
                    secretMirrorDirection = -Champion.AttackDirection;
                    boss.TelegraphDirection = secretMirrorDirection; boss.TelegraphUntil = secretMirrorAt; boss.AttackStarted = Elapsed; boss.AttackWindup = .65f;
                    events.Add(new CombatEvent(CombatEventKind.Windup, boss.Position, Faction.Enemy, subject: boss));
                }
                if (secretMirrorAt > 0 && Elapsed >= secretMirrorAt)
                {
                    secretMirrorAt = 0; boss.TelegraphUntil = 0; boss.AttackDirection = secretMirrorDirection;
                    WeaponDefinition w = boss.Definition.Weapon;
                    if (w.Kind == WeaponKind.Projectile)
                        EmitProjectile(boss, Mathf.Min(12, 5 + catalog.RunWave * .25f), false, NewCause(boss.Id));
                    else
                        Field(ExpandedFieldKind.Strike, boss, boss.Position, secretMirrorDirection, Mathf.Clamp(w.Range, 1.8f, 4), 8, 0, .2f, NewCause(boss.Id));
                    boss.AttackStarted = Elapsed; boss.BossOrganReleaseAt = Elapsed;
                    events.Add(new CombatEvent(CombatEventKind.Fire, boss.Position, Faction.Enemy, subject: boss));
                }
                return true;
            }
            if (boss.TelegraphUntil > 0 && Elapsed >= boss.TelegraphUntil)
            {
                boss.TelegraphUntil = 0; boss.BossOrganReleaseAt = Elapsed;
                if (boss.BossVolley++ % 2 == 0)
                {
                    Danger(ExpandedFieldKind.FallingProp, boss, boss.AbilityTarget, 1.2f, .45f, .3f);
                    if (catalog.SecretEncounter == 5) Danger(ExpandedFieldKind.Shockwave, boss, boss.AbilityTarget, 2.8f, .7f, .9f);
                }
                else for (int k = -1; k <= 1; k++) Danger(ExpandedFieldKind.Tentacle, boss, boss.AbilityTarget + Vector2.right * k * 1.7f, .35f, .35f + (k + 1) * .18f, .3f);
                boss.NextBossPattern = Elapsed + (catalog.SecretEncounter == 6 ? 4.2f : 3.5f); boss.BossRecoveryUntil = Elapsed + 1.1f;
                if (secretSecondActor != null) secretSecondActor.NextAttack = Elapsed + 1.5f;
                events.Add(new CombatEvent(CombatEventKind.Fire, boss.Position, Faction.Enemy, subject: boss));
            }
            if (Elapsed >= boss.NextBossPattern && boss.TelegraphUntil == 0)
            {
                boss.AttackWindup = 1.1f; boss.AttackStarted = Elapsed; boss.TelegraphUntil = Elapsed + 1.1f;
                boss.AbilityTarget = Champion.Position; boss.TelegraphDirection = (Champion.Position - boss.Position).normalized;
                events.Add(new CombatEvent(CombatEventKind.Windup, boss.Position, Faction.Enemy, subject: boss));
            }
            return true;
        }

        private void ProcessSecretEchoes()
        {
            if (!catalog.SecretMeleeEcho && !catalog.SecretMirrorEcho) return;
            // Snapshot events: secondary attacks never recursively create echoes.
            foreach (CombatEvent e in events.ToArray())
            {
                if (e.Kind != CombatEventKind.Fire || e.Faction == Faction.Enemy || e.Subject == null || e.Subject.Definition.Weapon == null) continue;
                UnitState source = e.Subject; WeaponDefinition w = source.Definition.Weapon;
                if (catalog.SecretMeleeEcho && w.Kind == WeaponKind.Melee)
                    Field(ExpandedFieldKind.Strike, source, source.Position, source.AttackDirection, w.Range, w.Damage * .3f, .26f, .18f, NewCause(source.Id).Child("secret-aftershock"));
                if (catalog.SecretMirrorEcho && source.Faction == Faction.Champion && Elapsed >= secretNextEcho)
                {
                    secretNextEcho = Elapsed + 4;
                    Field(ExpandedFieldKind.Strike, source, source.Position, source.AttackDirection, Mathf.Clamp(w.Range, 2, 5), w.Damage * .35f, .35f, .18f, NewCause(source.Id).Child("secret-mirror"));
                }
            }
        }
    }
}
