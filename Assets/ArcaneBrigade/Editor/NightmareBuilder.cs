using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class NightmareBuilder
    {
        private static T Resource<T>(string id) where T : ScriptableObject
        {
            string path = "Assets/ArcaneBrigade/Resources/" + id + ".asset";
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) { asset = ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset,path); }
            return asset;
        }
        [MenuItem("Arcane Brigade/Nightmare/Build story")]
        public static void Build()
        {
            NightmareAssets assets = Resource<NightmareAssets>("NightmareAssets");
            assets.Scenery = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/NightmareProps_v2.png","NightmareProp",4,4,Enumerable.Range(0,16).ToArray(), new[] { 0, 300, 566, 793, 1024 }, true);
            assets.CustomerFrames = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/LastCustomerIdle_v4.png","LastCustomerIdle",2,2,Enumerable.Range(0,4).ToArray(), new[] { 0, 512, 1024 }, true, true)
                .Concat(K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/LastCustomerActions_v4.png","LastCustomerAction",2,2,Enumerable.Range(0,4).ToArray(), new[] { 0, 512, 1024 }, true, true))
                .Concat(K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/LastCustomerOrgans_v2.png","LastCustomerOrgan",4,2,Enumerable.Range(0,8).ToArray(), new[] { 0, 512, 1024 }, true)).ToArray();
            string endingPath = "Assets/ArcaneBrigade/Art/Nightmare/NightmareEnding_v1.png";
            var importer = (TextureImporter)AssetImporter.GetAtPath(endingPath);
            importer.textureType = TextureImporterType.Default; importer.mipmapEnabled = false; importer.maxTextureSize = 4096; importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.CompressedHQ; importer.SaveAndReimport();
            assets.EndingIllustration = AssetDatabase.LoadAssetAtPath<Texture2D>(endingPath);
            string promisePath = "Assets/ArcaneBrigade/Art/Nightmare/OneLightLeftOn_v1.png";
            AssetDatabase.ImportAsset(promisePath, ImportAssetOptions.ForceSynchronousImport);
            var promiseImporter = (TextureImporter)AssetImporter.GetAtPath(promisePath);
            promiseImporter.textureType = TextureImporterType.Default; promiseImporter.mipmapEnabled = false; promiseImporter.maxTextureSize = 4096;
            promiseImporter.wrapMode = TextureWrapMode.Clamp; promiseImporter.textureCompression = TextureImporterCompression.CompressedHQ;
            promiseImporter.SaveAndReimport(); assets.PromiseIllustration = AssetDatabase.LoadAssetAtPath<Texture2D>(promisePath);
            assets.ManFrames = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/ManAfterTheReceipt_v1.png", "ManAfterReceipt", 4, 1, Enumerable.Range(0, 4).ToArray(), new[] { 0, 1024 }, true);
            assets.GrandStage = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/NightmareGrandStage_v1.png", "GrandStage", 4, 1, Enumerable.Range(0, 4).ToArray(), regions: new[] {
                new Rect(0, .56f, .65f, .44f), new Rect(.69f, .37f, .31f, .63f), new Rect(0, 0, .31f, .55f), new Rect(.32f, 0, .68f, .28f) });
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            UnitDefinition boss = Resource<UnitDefinition>(NightmareRules.CustomerId);
            EditorUtility.CopySerialized(catalog.Bosses[0],boss);
            boss.Id = NightmareRules.CustomerId; boss.DisplayName = "The Last Customer";
            boss.Health = 180000; boss.BodyHeight = 3.1f; boss.Radius = .7f; boss.Speed = .5f;
            boss.ContactDamage = 14; boss.IsBoss = true; boss.BossPattern = 10; boss.Material = BodyMaterial.Fabric;
            boss.Behavior = EnemyBehavior.Standard; boss.Body = assets.CustomerFrames[0]; boss.AnimationFrames = assets.CustomerFrames.Take(4).ToArray();
            boss.AuthoredMotion = Resource<AuthoredMotionSet>("LastCustomerMotion");
            boss.AuthoredMotion.Idle = assets.CustomerFrames.Take(2).ToArray(); boss.AuthoredMotion.Walk = assets.CustomerFrames.Skip(2).Take(2).ToArray();
            boss.AuthoredMotion.Windup = new[] { assets.CustomerFrames[4],assets.CustomerFrames[6] };
            boss.AuthoredMotion.Release = new[] { assets.CustomerFrames[5],assets.CustomerFrames[6] };
            boss.AuthoredMotion.Recovery = new[] { assets.CustomerFrames[7],assets.CustomerFrames[1] };
            WeaponDefinition weapon = Resource<WeaponDefinition>("LastCustomerAttack");
            EditorUtility.CopySerialized(catalog.Bosses[0].Weapon,weapon);
            weapon.Id = "LastCustomerAttack"; weapon.Sprite = null; weapon.Damage = 18; weapon.Kind = WeaponKind.Contact; weapon.Range = 1;
            boss.Weapon = weapon; assets.Customer = boss;
            assets.Anatomy = new BossAnatomyEntry { BossId = boss.Id,Organs = new[] {
                new BossOrganDefinition { Id = "customer-stamp",Name = "The Final Stamp",BreakSummary = "Break to weaken stamping attacks.",Role = BossOrganRole.Props,Offset = new Vector2(-.4f,.05f),Height = .3f,Durability = .2f,Rest = assets.CustomerFrames[8],Charged = assets.CustomerFrames[9],Release = assets.CustomerFrames[9],Scar = assets.CustomerFrames[15] },
                new BossOrganDefinition { Id = "customer-ribbon",Name = "Binding Promise",BreakSummary = "Break to interrupt binding ribbons.",Role = BossOrganRole.Lash,Offset = new Vector2(.4f,.05f),Height = .26f,Durability = .2f,Rest = assets.CustomerFrames[10],Charged = assets.CustomerFrames[11],Release = assets.CustomerFrames[11],Scar = assets.CustomerFrames[15] },
                new BossOrganDefinition { Id = "customer-drawer",Name = "The Inventory",BreakSummary = "Break to stop the parcel collection.",Role = BossOrganRole.Props,Offset = new Vector2(0,-.16f),Height = .2f,Durability = .2f,Rest = assets.CustomerFrames[13],Charged = assets.CustomerFrames[12],Release = assets.CustomerFrames[14],Scar = assets.CustomerFrames[15] }
            } };
            foreach (Object asset in new Object[] { assets,boss,weapon,boss.AuthoredMotion }) EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets(); Directory.CreateDirectory("PreviewExports/Nightmare");
            Verify();
        }
        public static void Verify() => NightmareChecks.Run();
        public static void Regressions()
        {
            Verify(); SecretEventChecks.Run(); K26RelicChecks.Run(); K24OutsideStageChecks.Run();
        }
        public static void Gate() { Regressions(); BuildWindows(); }
        public static void BuildWindows()
        {
            Verify();
            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = "Builds/Nightmare/ArcaneBrigade.exe",target = BuildTarget.StandaloneWindows64,options = BuildOptions.None });
            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new System.InvalidOperationException("Nightmare player build failed");
            Debug.Log("NIGHTMARE_PLAYER_BUILT errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings);
        }
    }
}
