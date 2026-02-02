using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class CombatRepairBuilder
    {
        public static void Import()
        {
            const string root="Assets/ArcaneBrigade/Art/MechanicEffects/";
            var a=K25AtlasImporter.Import(root+"HeroA_v1.png","HeroEffectA",4,4,Enumerable.Range(0,16).ToArray());
            var b=K25AtlasImporter.Import(root+"HeroB_v1.png","HeroEffectB",4,4,Enumerable.Range(0,16).ToArray());
            var forms=K25AtlasImporter.Import(root+"Forms_v1.png","BuildEffect",4,4,Enumerable.Range(0,16).ToArray());
            const string path="Assets/ArcaneBrigade/Resources/MechanicEffectArtwork.asset";
            var art=AssetDatabase.LoadAssetAtPath<MechanicEffectArtwork>(path);
            if(art==null){art=ScriptableObject.CreateInstance<MechanicEffectArtwork>();AssetDatabase.CreateAsset(art,path);}
            art.Heroes=a.Concat(b.Take(15)).ToArray();art.Forms=forms;EditorUtility.SetDirty(art);
            var frames=K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/WaitingScenery_Stages_v1.png","WaitingScenery",4,3,Enumerable.Range(0,12).ToArray());
            const string sceneryPath="Assets/ArcaneBrigade/Resources/WaitingSceneryArtwork.asset";
            var scenery=AssetDatabase.LoadAssetAtPath<WaitingSceneryArtwork>(sceneryPath);
            if(scenery==null){scenery=ScriptableObject.CreateInstance<WaitingSceneryArtwork>();AssetDatabase.CreateAsset(scenery,sceneryPath);}
            scenery.Frames=frames;EditorUtility.SetDirty(scenery);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
        }
        public static void Verify()
        {
            Import();CombatRepairChecks.Verify();RecruitmentChecks.Run();HeroIdentityChecks.Run();ShopPresentationBuilder.Check();MienChoChecks.Run();NightmareVisualChecks.Run();ShopFlowChecks.Run();K24OutsideStageChecks.Run();
        }
        public static void Build()
        {
            Verify();
            const string output="Builds/HeroIdentity";
            Directory.CreateDirectory(output);
            string[] scenes={"RunBoot","RunFrontend","RunArena","WaitingRoom"};
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=scenes.Select(s=>"Assets/ArcaneBrigade/Scenes/"+s+".unity").ToArray(),locationPathName=output+"/ArcaneBrigade.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            File.WriteAllText("PreviewExports/CombatRepair/Build.txt","Result: "+report.summary.result+"\nErrors: "+report.summary.totalErrors+"\nWarnings: "+report.summary.totalWarnings+"\nSize: "+report.summary.totalSize+"\nPlayer: "+output+"/ArcaneBrigade.exe\n");
            if(report.summary.result!=BuildResult.Succeeded || report.summary.totalErrors>0)throw new InvalidOperationException("Combat repair Windows build failed");
            Directory.CreateDirectory(output+"/ThirdPartyNotices");
            foreach(string font in Directory.GetFiles("Assets/ArcaneBrigade/Resources/Fonts","*OFL.txt"))File.Copy(font,output+"/ThirdPartyNotices/"+Path.GetFileName(font),true);
            const string audio="Assets/ArcaneBrigade/Resources/Audio/Licensed";
            if(Directory.Exists(audio))foreach(string notice in Directory.GetFiles(audio,"*.txt"))File.Copy(notice,output+"/ThirdPartyNotices/"+Path.GetFileName(notice),true);
        }
    }
}
