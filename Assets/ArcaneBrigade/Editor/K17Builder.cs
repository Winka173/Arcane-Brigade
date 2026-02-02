using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K17Builder
    {
        [MenuItem("Arcane Brigade/K17/Build review balance")]
        public static void Build()
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            BuildBosses(catalog);
            BuildEnemies(catalog);
            BuildHeroes(catalog);
            BuildStage();
            string[] rare = { "CoilKettle", "ThimbleMoth", "MoonJelly", "MirrorNewt", "SpringDuck", "BookmarkGolem" };
            string[] uncommon = { "CandleSnail", "CitrusToad", "CloudScissors", "CoralTeacup", "PearMoth", "SailSquid", "StarSock", "AnchorSnail", "IceCreamOwl" };
            foreach (UnitDefinition unit in catalog.Companions)
            {
                unit.Rarity = unit.SpecialRecruit ? ItemRarity.Mythic : rare.Contains(unit.Id) ? ItemRarity.Rare : uncommon.Contains(unit.Id) ? ItemRarity.Uncommon : ItemRarity.Common;
                unit.Description = ShortOfferCopy.Companion(unit);
                EditorUtility.SetDirty(unit);
            }
            int index = 0;
            foreach (UnitDefinition unit in catalog.Enemies)
            {
                if (unit.Behavior >= EnemyBehavior.Hopper) continue;
                unit.Shot = unit.Weapon.Kind == WeaponKind.Projectile ? (HostileShot)(1 + index++ % 4) : HostileShot.Straight;
                EditorUtility.SetDirty(unit);
            }
            foreach (UnitDefinition boss in catalog.Bosses)
            {
                boss.Shot = (HostileShot)(boss.BossPattern % 5);
                if (boss.BossPattern < 5) boss.Description = boss.BossPattern switch
                {
                    0 => "Rotating volleys, stage bites and thrown tickets.",
                    1 => "Fan volleys and shadow lashes.",
                    2 => "Cross volleys and expanding shockwaves.",
                    3 => "Spread shots and paired shockwaves.",
                    _ => "Paired volleys and falling luggage."
                };
                EditorUtility.SetDirty(boss);
            }
            foreach (ItemDefinition item in catalog.Items) item.Description = ShortOfferCopy.Item(item);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets(); Verify();
        }

        private static T Asset<T>(string id, T source) where T : ScriptableObject
        {
            string path = "Assets/ArcaneBrigade/Data/Run/" + id + ".asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) { asset = UnityEngine.Object.Instantiate(source); asset.name = id; AssetDatabase.CreateAsset(asset, path); }
            return asset;
        }

        private static void BuildBosses(RunCatalog catalog)
        {
            string[] ids = { "MarqueeMaw", "UmbraCradle", "CoinWidow", "ChoirEngine", "SuitcaseLeviathan" };
            string[] names = { "The Marquee Maw", "Mother Umbra", "The Coin Widow", "The Howling Organ", "Suitcase Leviathan" };
            string[] copy = { "Bites the stage and throws broken tickets.", "Lashes with shadow limbs. Avoid the dark pools.", "Pulls loose coins and releases shockwaves.", "Two expanding shockwaves. Dodge between them.", "Throws luggage and lashes its tongue." };
            var bosses = catalog.Bosses.Where(unit => !ids.Contains(unit.Id)).ToList();
            for (int index = 0; index < ids.Length; index++)
            {
                string id = ids[index];
                UnitDefinition unit = Asset(id, catalog.Bosses[0]);
                unit.Id = id; unit.DisplayName = names[index]; unit.Description = copy[index];
                unit.IsBoss = true; unit.BossPattern = 5 + index; unit.Health = 600 + index * 125;
                unit.BodyHeight = 2.7f + index * 0.12f; unit.Radius = 0.85f; unit.Speed = 0.62f;
                unit.Shot = index == 2 ? HostileShot.Hook : (HostileShot)(index % 5);
                unit.Behavior = EnemyBehavior.Standard; unit.Passive = PassiveKind.None;
                unit.Motif = (BossMotif)(index % 3); unit.Material = index == 2 ? BodyMaterial.Metal : index == 3 ? BodyMaterial.Wood : BodyMaterial.Fabric;
                WeaponDefinition attack = Asset(id + "Attack", catalog.Bosses[0].Weapon);
                attack.Id = id + "Attack"; attack.Kind = WeaponKind.Contact; attack.Damage = 10 + index;
                attack.Cooldown = 2.4f; attack.ProjectileCount = 5; attack.ProjectileVisualWidth = 0.36f; attack.ProjectileRadius = 0.1f;
                attack.Windup = 0.85f; attack.Recovery = 0.9f;
                unit.Weapon = attack;
                string sourcePath = "Assets/ArcaneBrigade/Art/K17/" + id + (File.Exists("Assets/ArcaneBrigade/Art/K17/" + id + "_v2.png") ? "_v2.png" : "_v1.png");
                Sprite[] frames = K16Builder.ImportSheet(id, sourcePath);
                string motionPath = "Assets/ArcaneBrigade/Resources/" + id + "Motion.asset";
                AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(motionPath);
                if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, motionPath); }
                motion.Idle = frames.Take(2).ToArray(); motion.Walk = frames.Skip(2).Take(4).ToArray();
                motion.Windup = frames.Skip(6).Take(2).ToArray(); motion.Release = frames.Skip(8).Take(2).ToArray(); motion.Recovery = frames.Skip(10).Take(2).ToArray();
                unit.AuthoredMotion = motion; unit.Body = frames[0]; unit.AnimationFrames = new[] { frames[0], frames[1], frames[2], frames[4] };
                EditorUtility.SetDirty(unit); EditorUtility.SetDirty(attack); EditorUtility.SetDirty(motion); bosses.Add(unit);
            }
            catalog.Bosses = bosses.ToArray(); EditorUtility.SetDirty(catalog);
        }

        private static void BuildHeroes(RunCatalog catalog)
        {
            string[] ids = { "RootOracle", "LoneJelly", "GlassDice", "TideSinger", "CometRunner", "MirrorKeeper", "TreasureAcorn" };
            string[] sources = { "PillowOracle", "JellySwordsman", "DiceCrab", "BubbleKoi", "CometKite", "MirrorNewt", "BrassAcorn" };
            string[] names = { "The Root Oracle", "The Lone Jelly", "Lady Loaded Dice", "The Tide Singer", "The Comet Courier", "The Mirror Keeper", "The Treasure Clerk" };
            string[] weapons = { "RuneStaff", "TwinSabres", "PlasmaTrident", "BubbleWand", "ClockCrescent", "SnowGlobeOrb", "AcornSling" };
            float[] speeds = { 3.1f, 3.4f, 3.0f, 3.1f, 3.8f, 2.9f, 3.2f };
            var heroes = catalog.Champions.Where(unit => !ids.Contains(unit.Id)).ToList();
            for (int index = 0; index < ids.Length; index++)
            {
                UnitDefinition source = catalog.Unit(sources[index]);
                UnitDefinition unit = Asset(ids[index], source);
                unit.Id = ids[index]; unit.DisplayName = names[index]; unit.HeroMechanic = (HeroMechanic)((int)HeroMechanic.RootedOracle + index);
                unit.Description = K11HeroRules.Describe(unit.HeroMechanic); unit.Weapon = catalog.Weapon(weapons[index]);
                unit.Health = index == 2 ? 60 : index == 0 ? 180 : index == 1 ? 145 : 120; unit.Speed = speeds[index];
                unit.BodyHeight = 1.08f; unit.Radius = 0.25f; unit.IsBoss = false; unit.SpecialRecruit = false;
                unit.Rarity = ItemRarity.Common; unit.Passive = PassiveKind.None; unit.UnlockWave = 0;
                unit.RecruitThemes = index == 2 ? catalog.Bonds.Select(bond => bond.Id).ToArray() : source.Tags;
                unit.CompanionModifiers = new StatModifier[0];
                ItemEffect[] traits = { ItemEffect.HealthCorrection, ItemEffect.MeleeDamage, ItemEffect.ShopRarity, ItemEffect.HeartHealing, ItemEffect.SpeedCorrection, ItemEffect.Dodge, ItemEffect.Income };
                float[] strengths = { 0.15f, 4, 0.15f, 4, -0.05f, 0.1f, 5 };
                unit.ChampionModifiers = index == 2 ? new[] { new StatModifier { Effect = ItemEffect.Luck, Scope = ModifierScope.Run, Value = 0.6f }, new StatModifier { Effect = ItemEffect.Crit, Scope = ModifierScope.Champion, Value = 0.2f }, new StatModifier { Effect = ItemEffect.ShopRarity, Scope = ModifierScope.Run, Value = 0.15f } } : new[] { new StatModifier { Effect = traits[index], Scope = RunModifierRules.DefaultScope(traits[index]), Value = strengths[index] } };
                if (index == 0 || index == 1)
                    unit.ChampionModifiers = unit.ChampionModifiers.Concat(new[] { new StatModifier { Effect = ItemEffect.Armor, Scope = ModifierScope.Champion, Value = index == 0 ? 1.5f : 0.3f } }).ToArray();
                EditorUtility.SetDirty(unit); heroes.Add(unit);
            }
            catalog.Champions = heroes.ToArray(); EditorUtility.SetDirty(catalog);
        }

        private static void BuildEnemies(RunCatalog catalog)
        {
            string[] ids = { "HopperKeg", "ChargerRam", "CoinLeech", "BlastPuffer", "SporeBishop", "BroomSkater", "RibbonHydra", "BurrowBell" };
            string[] names = { "Hopper Keg", "Charging Ram", "Coin Leech", "Blast Puffer", "Spore Bishop", "Broom Skater", "Ribbon Hydra", "Burrow Bell" };
            string[] copy = { "Jumps onto a marked landing spot.", "Locks a direction, then charges.", "Steals loose coins. Defeat it to recover them.", "Inflates before releasing a shockwave.", "Plants delayed spore pools.", "Sweeps three spots in succession.", "Spits three curving streams.", "Burrows, then erupts at a marked spot." };
            var enemies = catalog.Enemies.Where(unit => !ids.Contains(unit.Id)).ToList();
            for (int index = 0; index < ids.Length; index++)
            {
                UnitDefinition unit = Asset(ids[index], catalog.Enemies[0]);
                unit.Id = ids[index]; unit.DisplayName = names[index]; unit.Description = copy[index];
                unit.Health = 32 + index * 7; unit.Speed = index == 1 ? 1.5f : 0.85f + index % 3 * 0.12f;
                unit.BodyHeight = index == 0 || index == 3 ? 1.3f : 1.05f; unit.Radius = index == 3 ? 0.38f : 0.3f;
                BodyMaterial[] materials = { BodyMaterial.Metal, BodyMaterial.Wood, BodyMaterial.Fabric, BodyMaterial.Fabric, BodyMaterial.Wood, BodyMaterial.Wood, BodyMaterial.Fabric, BodyMaterial.Metal };
                unit.Material = materials[index]; unit.BodyScale = index == 0 || index == 3 ? 1.8f : 1;
                unit.Behavior = (EnemyBehavior)((int)EnemyBehavior.Hopper + index); unit.IsBoss = false;
                unit.UnlockWave = 2 + index; unit.Passive = PassiveKind.None; unit.Shot = (HostileShot)(1 + index % 4);
                WeaponDefinition attack = Asset(ids[index] + "Attack", catalog.Enemies[0].Weapon);
                attack.Id = ids[index] + "Attack"; attack.Kind = WeaponKind.Contact; attack.Damage = 5 + index * 0.4f;
                attack.ProjectileCount = index == 6 ? 3 : 1; attack.ProjectileSpread = 0.24f; attack.ProjectileSpeed = 2.1f;
                attack.ProjectileLifetime = 5; attack.ProjectileVisualWidth = index == 6 ? 0.32f : 0.26f; attack.ProjectileRadius = 0.08f;
                attack.Windup = 0.7f; attack.Recovery = 0.75f;
                if (index == 2) attack.ProjectileSprite = catalog.Arena.Coin;
                unit.Weapon = attack;
                string path = "Assets/ArcaneBrigade/Art/K17/" + ids[index] + (File.Exists("Assets/ArcaneBrigade/Art/K17/" + ids[index] + "_v2.png") ? "_v2.png" : "_v1.png");
                Sprite[] frames = K16Builder.ImportSheet(ids[index], path);
                string motionPath = "Assets/ArcaneBrigade/Resources/" + ids[index] + "Motion.asset";
                AuthoredMotionSet motion = AssetDatabase.LoadAssetAtPath<AuthoredMotionSet>(motionPath);
                if (motion == null) { motion = ScriptableObject.CreateInstance<AuthoredMotionSet>(); AssetDatabase.CreateAsset(motion, motionPath); }
                motion.Idle = frames.Take(2).ToArray(); motion.Walk = frames.Skip(2).Take(4).ToArray();
                motion.Windup = frames.Skip(6).Take(2).ToArray(); motion.Release = frames.Skip(8).Take(2).ToArray(); motion.Recovery = frames.Skip(10).Take(2).ToArray();
                unit.AuthoredMotion = motion; unit.Body = frames[0]; unit.AnimationFrames = new[] { frames[0], frames[1], frames[2], frames[4] };
                EditorUtility.SetDirty(unit); EditorUtility.SetDirty(attack); EditorUtility.SetDirty(motion); enemies.Add(unit);
            }
            catalog.Enemies = enemies.ToArray(); EditorUtility.SetDirty(catalog);
        }

        private static void BuildStage()
        {
            string path = "Assets/ArcaneBrigade/Resources/CarnivalStageAssets.asset";
            CarnivalStageAssets assets = AssetDatabase.LoadAssetAtPath<CarnivalStageAssets>(path);
            if (assets == null) { assets = ScriptableObject.CreateInstance<CarnivalStageAssets>(); AssetDatabase.CreateAsset(assets, path); }
            assets.Stands = K16Builder.ImportSheet("MarketStands", "Assets/ArcaneBrigade/Art/K17/MarketStands_v1.png");
            string courier = "Assets/ArcaneBrigade/Art/K17/StageCourier" + (File.Exists("Assets/ArcaneBrigade/Art/K17/StageCourier_v2.png") ? "_v2.png" : "_v1.png");
            assets.Courier = K16Builder.ImportSheet("StageCourier", courier);
            assets.Floors = new Texture2D[3];
            assets.Floors[0] = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ArcaneBrigade/Art/K13/CarnivalStone_v1.png");
            string[] names = { "ClockworkFloor", "DreamGardenFloor" };
            for (int index = 0; index < names.Length; index++)
            {
                string source = "Assets/ArcaneBrigade/Art/K17/" + names[index] + "_v1.png";
                var importer = (TextureImporter)AssetImporter.GetAtPath(source);
                importer.textureType = TextureImporterType.Default; importer.wrapMode = TextureWrapMode.Repeat;
                importer.maxTextureSize = 4096; importer.mipmapEnabled = true; importer.filterMode = FilterMode.Trilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.SaveAndReimport();
                assets.Floors[index + 1] = AssetDatabase.LoadAssetAtPath<Texture2D>(source);
            }
            EditorUtility.SetDirty(assets);
        }

        public static void Verify()
        {
            Directory.CreateDirectory("PreviewExports/K17");
            AuditArt();
            Checks();
            K16Checks.Run(); K15Checks.Run(); K14Verification.RunChecks();
            FullRunBuilder.BuildWindows();
            File.Copy("PreviewExports/FullRun_Build.txt", "PreviewExports/K17/Windows_Build.txt", true);
        }

        public static void Campaign() => K14CampaignDiagnostics.RunInto("PreviewExports/K17");
        public static void AuditArt()
        {
            Directory.CreateDirectory("PreviewExports/K17");
            var notes = new List<string>();
            foreach (string file in Directory.GetFiles("Assets/ArcaneBrigade/Art/K17", "*_v1.png"))
            {
                if (file.Contains("Floor")) continue;
                string id = Path.GetFileNameWithoutExtension(file).Replace("_v1", "");
                string revised = file.Replace("_v1", "_v2");
                try { K16Builder.ImportSheet(id, File.Exists(revised) ? revised : file); notes.Add("PASS " + id); }
                catch (Exception error) { notes.Add("FAIL " + id + " " + error.Message); }
            }
            File.WriteAllLines("PreviewExports/K17/Source_Art_Audit.txt", notes);
            if (notes.Any(note => note.StartsWith("FAIL", StringComparison.Ordinal))) throw new InvalidOperationException("K17 authored atlas audit failed");
            Debug.Log(string.Join("\n", notes));
        }

        public static void Checks()
        {
            var notes = new List<string>();
            void Require(bool condition, string caption)
            {
                if (!condition) throw new InvalidOperationException("K17 " + caption);
                notes.Add("PASS " + caption);
            }
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (ItemRarity rarity in Enum.GetValues(typeof(ItemRarity)))
                Require(catalog.Companions.Any(unit => unit.Rarity == rarity), "Companion rarity " + rarity);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 171077); session.State.Phase = SessionPhase.Shop;
                session.State.Gold = session.State.Eggs = 100000;
                string id = session.State.Roster[0].UnitId;
                for (int purchase = 0; purchase < 3; purchase++)
                {
                    var offer = new ShopOffer { Id = id, Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Price = 1 };
                    session.State.Offers.Add(offer);
                    Require(session.Buy(offer.Token), "Existing species purchase " + purchase);
                    Require(session.State.NewRecruitsThisWave == purchase + 1 && session.State.NewSpeciesRecruitsThisWave == 0, "Owned species use purchase quota only " + purchase);
                }
                var blocked = new ShopOffer { Id = id, Token = ++session.State.NextOffer, Kind = OfferKind.Companion, Price = 1 };
                session.State.SpecialOffers.Add(blocked);
                Require(session.Buy(blocked.Token), "Special shop remains exempt from ordinary new-species quota");
                session.State.NewRecruitsThisWave = RunSession.CompanionPurchaseLimit;
                for (int roll = 0; roll < 20; roll++)
                {
                    Require(session.Reroll(), "Quota refresh " + roll);
                    Require(session.State.Offers.All(offer => offer.Kind == OfferKind.Item), "Three-purchase limit retains items only " + roll);
                }
                int cost = session.RerollCost;
                session.State.Gold = cost - 1;
                Require(!session.Reroll(), "Insufficient gold does not refresh");
                session.State.Gold = 100000;
                session.State.NewRecruitsThisWave = 0;
                session.State.Roster.Clear();
                UnitDefinition rare = catalog.Companions.First(unit => unit.Rarity == ItemRarity.Rare);
                session.State.Roster.Add(new RosterEntry { UnitId = rare.Id, WeaponId = "SpoonSabre" });
                float sourceDamage = catalog.Weapon("SpoonSabre").Damage;
                CheckpointCatalog combat = session.PrepareCombat();
                Require(combat.StartingCompanions[0].Weapon.Damage > sourceDamage * 1.5f, "Rare bonus survives stat compilation");
                float damage = combat.StartingCompanions[0].Weapon.Damage;
                Require(Mathf.Approximately(session.PrepareCombat().StartingCompanions[0].Weapon.Damage, damage), "No repeated rarity stacking");
                session.State.Wave = 20;
                combat = session.PrepareCombat();
                Require(combat.Boss != null && combat.Boss.BodyHeight > catalog.Bosses.First(boss => boss.Id == combat.Boss.Id).BodyHeight, "Boss visibly enlarged");
                float lateHealth = combat.MeleeEnemy.Health / catalog.Enemies[19 % catalog.Enemies.Length].Health;
                Require(lateHealth >= 12 && lateHealth <= 18, "K19 restores 20% late health: x12-x18, without a wave cliff");
                Require(combat.MeleeEnemy.Speed < catalog.Enemies[19 % catalog.Enemies.Length].Speed * 2.8f, "Speed remains readable");
                session.State.Wave = 1; combat = session.PrepareCombat();
                Require(combat.EnemyPool.All(unit => unit.Weapon.Kind == WeaponKind.Contact), "No first-wave projectile enemies");
            }
            foreach (HostileShot motion in Enum.GetValues(typeof(HostileShot)))
                for (int sample = 0; sample < 180; sample++)
                {
                    Vector2 velocity = HostileBallistics.Velocity(motion, Vector2.right * 7, sample / 30f, sample % 2);
                    Require(float.IsFinite(velocity.x) && velocity.magnitude <= 2.401f, "Safe hostile velocity " + motion + "/" + sample);
                }
            Require(HostileBallistics.Velocity(HostileShot.Weave, Vector2.right, 0.3f, 1).y > 0.01f, "Weave changes direction");
            Require(HostileBallistics.Velocity(HostileShot.Hook, Vector2.right, 1, 1).y < 0, "Hook has alternate handedness");
            Require(CarnivalBalance.ShopPrice(20) > 10 && CarnivalBalance.ShopPrice(int.MaxValue) <= 10000, "Inflation grows and remains bounded");
            Require(ArcadeMusic.SceneLevel(ArcaneCueLibrary.SceneBoss) <= 0.36f, "Boss music leaves SFX room");
            Require(catalog.Champions.Length == 10 && catalog.Bosses.Length == 10, "Ten playable heroes and ten bosses");
            Require(catalog.Enemies.Count(unit => unit.Behavior >= EnemyBehavior.Hopper) == 8, "Eight new body-attack enemy species");
            foreach (UnitDefinition hero in catalog.Champions)
            {
                using var session = new RunSession(catalog); session.Start(hero.Id, 171077); session.Validate(session.State);
                CheckpointCatalog combat = session.PrepareCombat(); combat.EnemyBudget = 0; combat.Boss = null;
                var simulation = new CombatSimulation(combat); simulation.Start(171077);
                simulation.Step(1f / 60, Vector2.right);
                Require(hero.HeroMechanic == HeroMechanic.RootedOracle ? simulation.Champion.Position == Vector2.zero : simulation.Champion.Position.x > 0, "Combat movement policy " + hero.Id);
                simulation.EnterRest(); simulation.StepRest(1f / 60, Vector2.right);
                Require(simulation.Champion.Position.x > 0, "Shop movement remains available " + hero.Id);
                if (hero.HeroMechanic == HeroMechanic.SoloMaw)
                {
                    Require(session.State.Roster.Count == 0 && combat.StartingCompanions.Length == 0, "Solo starts without allies");
                    session.State.Phase = SessionPhase.Shop; session.State.Gold = 100000;
                    for (int refresh = 0; refresh < 10; refresh++) { session.Reroll(); Require(session.State.Offers.All(offer => offer.Kind == OfferKind.Item), "Solo item-only shop " + refresh); }
                }
                if (hero.HeroMechanic == HeroMechanic.RootedOracle)
                    Require(combat.PickupRadius >= catalog.WorldSize.magnitude && combat.ChestOpenRadius >= catalog.WorldSize.magnitude, "Stationary hero collects across whole arena");
            }
            foreach (UnitDefinition definition in catalog.Enemies.Where(unit => unit.Behavior >= EnemyBehavior.Hopper))
            {
                using var session = new RunSession(catalog); session.Start("LanternKnight", 171077); session.State.Wave = 12; session.State.Roster.Clear();
                CheckpointCatalog combat = session.PrepareCombat(); combat.EnemyBudget = 0; combat.Boss = null; combat.WaveDuration = 100;
                var simulation = new CombatSimulation(combat); simulation.Start(171077);
                simulation.Champion.NextAttack = float.PositiveInfinity; simulation.Champion.Health = 10000;
                UnitState enemy = simulation.AddFixture(definition, Faction.Enemy, new Vector2(3, 0));
                enemy.Health = 10000;
                bool moved = false, lifted = false, telegraphed = false;
                for (int tick = 0; tick < 180; tick++)
                {
                    simulation.Step(1f / 60, Vector2.zero);
                    moved |= enemy.Position != enemy.AbilityOrigin; lifted |= enemy.BodyLift > 0;
                    telegraphed |= simulation.ExpandedFields.Count > 0 || simulation.Projectiles.Count > 0;
                    Require(float.IsFinite(enemy.Position.x) && float.IsFinite(enemy.Position.y), "Finite ability movement " + definition.Id + "/" + tick);
                }
                Require(enemy.NextBehavior > 0 && moved, "Enemy body behavior executes " + definition.Id);
                if (definition.Behavior == EnemyBehavior.Hopper) Require(lifted, "Jump has real airborne body motion");
                if (definition.Behavior != EnemyBehavior.CoinThief) Require(telegraphed, "Dangerous ability visibly telegraphed " + definition.Id);
                Require(definition.AuthoredMotion != null && definition.AuthoredMotion.Walk.Length == 4, "New enemy authored poses " + definition.Id);
            }
            var tentacle = new ExpandedField { Kind = ExpandedFieldKind.Tentacle, Start = Vector2.zero, Position = new Vector2(4, 0), Radius = 0.3f };
            foreach (UnitDefinition definition in catalog.Bosses.Where(unit => unit.BossPattern >= 5))
            {
                using var session = new RunSession(catalog); session.Start("LanternKnight", 171077); session.State.Wave = 2; session.State.Roster.Clear();
                CheckpointCatalog combat = session.PrepareCombat(); combat.EnemyBudget = 0; combat.Boss = definition; combat.WaveDuration = 100;
                var simulation = new CombatSimulation(combat); simulation.Start(171077);
                simulation.Champion.NextAttack = float.PositiveInfinity;
                simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(-7, -4);
                bool bodyAttack = false;
                for (int tick = 0; tick < 360; tick++)
                {
                    simulation.Step(1f / 60, Vector2.zero);
                    bodyAttack |= simulation.ExpandedFields.Any(field => field.Kind >= ExpandedFieldKind.Tentacle);
                }
                Require(simulation.Boss.BossVolley >= 1 && bodyAttack, "Boss body attack actually executes " + definition.Id);
                Require(definition.AuthoredMotion.Release.Length == 2 && definition.AuthoredMotion.Windup.Length == 2, "Boss anticipation and release drawings " + definition.Id);
            }
            Require(CarnivalHazards.Contains(tentacle, new Vector2(2, 0), 0.1f, 0) && !CarnivalHazards.Contains(tentacle, new Vector2(2, 1), 0.1f, 0), "Tentacle uses a lane, not a disk");
            var wave = new ExpandedField { Kind = ExpandedFieldKind.Shockwave, Position = Vector2.zero, Radius = 4, At = 1, Until = 3 };
            Require(CarnivalHazards.Contains(wave, new Vector2(2, 0), 0.1f, 2) && !CarnivalHazards.Contains(wave, Vector2.zero, 0.1f, 2), "Shockwave ring leaves a safe interior");
            Require(StageTransfer.ChangesMap(6) && StageTransfer.ChangesMap(11) && StageTransfer.ChangesMap(16) && !StageTransfer.ChangesMap(5), "Map changes when Act I enters a new zone (waves 6, 11, 16)");
            Require(Vector2.Distance(StageTransfer.PassengerPosition(new Vector2(0, -5), 2.6f), new Vector2(0, -0.2f)) < 0.001f, "Transport lands exactly, without scene loading");
            File.WriteAllLines("PreviewExports/K17/Focused_Checks.txt", notes);
            Debug.Log("K17_CHECKS_PASSED " + notes.Count);
        }
    }
}
