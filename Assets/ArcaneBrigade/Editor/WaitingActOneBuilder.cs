using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade.Editor
{
    public static class WaitingActOneBuilder
    {
        private const string Root="Assets/ArcaneBrigade/Resources/MienCho/";
        public static void Import()
        {
            AssetDatabase.Refresh();
            foreach(string path in Directory.GetFiles(Root,"*.png",SearchOption.AllDirectories))
            {
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)continue;
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=256;
                importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;
                importer.textureCompression=TextureImporterCompression.CompressedHQ;importer.SaveAndReimport();
            }
            foreach(var pair in new[]{("LanternField","ArcaneBrigade/NightLanternField"),("NightFogVolume","ArcaneBrigade/NightFogVolume"),("NightFogDisplay","Unlit/Transparent"),("JourneyRoomLight","ArcaneBrigade/JourneyRoomLight")})
            {
                var path=Root+pair.Item1+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);
                var shader=Shader.Find(pair.Item2);if(shader==null)throw new InvalidOperationException("Missing shader "+pair.Item2);
                if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}else m.shader=shader;
                EditorUtility.SetDirty(m);
            }
            string scenePath="Assets/ArcaneBrigade/Scenes/WaitingRoom.unity";
            var previous=SceneManager.GetActiveScene();if(string.IsNullOrEmpty(previous.path)) previous=EditorSceneManager.OpenScene("Assets/ArcaneBrigade/Scenes/RunBoot.unity",OpenSceneMode.Single);var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);new GameObject("Waiting Room Exploration").AddComponent<WaitingRoomController>();
            EditorSceneManager.SaveScene(scene,scenePath);EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        public static void VerifyAndBuild()
        {
            Import();DreamLevitationChecks.Run();WaitingActOneChecks.Run();MienChoChecks.Run();RosterCapAndBondChecks.Run();RecruitmentChecks.Run();HeroIdentityChecks.RefreshDescriptions();
            Directory.CreateDirectory("PreviewExports/ActOne");
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ArcaneBrigade/Scenes/RunBoot.unity","Assets/ArcaneBrigade/Scenes/RunFrontend.unity","Assets/ArcaneBrigade/Scenes/RunArena.unity","Assets/ArcaneBrigade/Scenes/WaitingRoom.unity"},locationPathName="Builds/HeroIdentity/ArcaneBrigade.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.CleanBuildCache});
            File.WriteAllText("PreviewExports/ActOne/Build.txt","Result: "+report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nBytes: "+report.summary.totalSize+"\nPath: Builds/HeroIdentity/ArcaneBrigade.exe\n");
            File.WriteAllLines("PreviewExports/ActOne/Build_Messages.txt",report.steps.SelectMany(x=>x.messages).Where(x=>x.type==LogType.Warning||x.type==LogType.Error).Select(x=>x.type+": "+x.content));
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Act One build failed");
            GameReviewSnapshot.Export();
        }
    }
}
