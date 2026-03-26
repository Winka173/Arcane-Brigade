using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ArcaneBrigade
{
    public sealed partial class CombatRepairCapture : MonoBehaviour
    {
        public static bool Requested=>Array.IndexOf(Environment.GetCommandLineArgs(),"-arcane-combat-repair-capture")>=0;
        private readonly List<string> checks=new List<string>();
        private string root;private bool reduced,failed;private ArenaController owner;private RunCatalog catalog;
        private string[] labels;private Vector2[] labelPositions;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init(){if(!Requested || FindAnyObjectByType<CombatRepairCapture>()!=null)return;var go=new GameObject("CombatRepairVerification");DontDestroyOnLoad(go);go.AddComponent<CombatRepairCapture>();}
        private void Check(bool ok,string label){checks.Add((ok?"PASS ":"FAIL ")+label);failed|=!ok;File.WriteAllLines(Path.Combine(root,"Native_Checks.txt"),checks);}
        private void Log(string message,string stack,LogType kind){if(kind==LogType.Error || kind==LogType.Exception || kind==LogType.Assert){Check(false,message+"\n"+stack);Application.Quit(1);}}
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();
            int visible=0;for(int y=0;y<image.height;y+=24)for(int x=0;x<image.width;x+=24)if(image.GetPixel(x,y).maxColorComponent>.12f)visible++;
            Check(image.width==1920 && image.height==1080 && visible>60,"Actual nonblack Windows frame "+name);
            File.WriteAllBytes(Path.Combine(root,name+".png"),image.EncodeToPNG());Destroy(image);
        }
        private IEnumerator Load(int wave,bool shop=false,Action<RunSession> setup=null)
        {
            using(var run=new RunSession(catalog))
            {run.Start("LanternKnight",171026);run.State.Wave=wave;run.State.Phase=shop?SessionPhase.Shop:SessionPhase.Combat;run.State.Gold=1000;setup?.Invoke(run);new RunStorage(RunPaths.Root).SaveRun(run.State);}
            RunLaunch.Resume=true;SceneManager.LoadScene("RunArena");yield return new WaitForSecondsRealtime(.2f);owner=FindAnyObjectByType<ArenaController>();
            Check(owner?.Simulation!=null,"Actual saved run loaded "+wave+" / "+shop);
            if(!shop)owner.Simulation.PrepareMienChoCapture(catalog);
        }
        private IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-capturePath");if(at<0 || at+1>=args.Length){Application.Quit(1);yield break;}
            root=Path.GetFullPath(args[at+1]);Directory.CreateDirectory(root);Application.runInBackground=true;Application.logMessageReceived+=Log;
            reduced=Array.IndexOf(args,"-repair-reduced")>=0;catalog=Resources.Load<RunCatalog>("RunCatalog");
            new RunStorage(RunPaths.Root).SaveProfile(new PlayerProfile{BestWave=40,TutorialSeen=true,ReducedMotion=reduced,ReducedEffects=reduced,ReducedFlashes=reduced});
            Check(RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"),"Player saves stay isolated");
            if(Array.IndexOf(args,"-shop-flow")>=0){yield return ShopFlow();Application.Quit(failed?1:0);yield break;}
            yield return Load(4,true);
            var scenery=owner.transform.Find("DreamLandmark0").GetComponent<SpriteRenderer>();
            Vector3 first=scenery.transform.TransformPoint(WorldSpriteGrounding.Foot(scenery.sprite));
            Check(ShopSceneryLanding.Active.Settling,"Entering the shop starts the scenery landing");
            yield return Shot("01-shop-scenery-falling");
            yield return new WaitForSecondsRealtime(.4f);
            Vector3 landed=scenery.transform.TransformPoint(WorldSpriteGrounding.Foot(scenery.sprite));
            Check(reduced?Vector3.Distance(first,landed)<.02f:first.y-landed.y>.4f,"Scenery falls while reduced motion stays composed");
            for(int i=0;i<NightLanternField.Capacity;i++)
            {var field=NightLanternField.Active;float lift=field.FixtureFoot(i).y-field.FixtureAnchor(i).y;Check(lift>.1f && lift<.85f,"Lamp is excluded from the scenery drop "+i);}
            yield return Shot("02-shop-scenery-landed");
            yield return new WaitForSecondsRealtime(1);
            float deadline=Time.realtimeSinceStartup+10;while(owner.Shop.Animating && Time.realtimeSinceStartup<deadline)yield return null;
            Check(!owner.Shop.Animating,"Shop entrance is ready for real interaction");
            var exit=owner.Shop.Stations.First(s=>s.Action=="NEXT HALL");
            Check(Mathf.Abs(exit.Position.x)<.01f && exit.Position.y<-3.5f && exit.Reach>=1.8f,"Door is at bottom centre with a wide entrance");
            owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=exit.Position+Vector2.right*1.5f;owner.Shop.Display();
            Check(owner.Shop.Nearby==exit,"Door focuses beyond the old global 1.1-unit cap");
            yield return Shot("03-bottom-centre-door-wide-focus");
            Check(owner.Shop.Interact() && owner.HallExitPending,"Broad doorway actually accepts the continue action");
            yield return Load(1);yield return new WaitForSecondsRealtime(.5f);
            var stage=owner.GetComponent<NightmareGrandStage>();
            Check(stage.VisibleCount==NightmareGrandStage.Capacity,"All world scenery is present in the first wave");
            Vector2 viewpoint=stage.FixtureAnchor(10)+Vector2.down*2;
            owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=viewpoint;
            FindAnyObjectByType<ArenaCamera>().Snap();yield return new WaitForEndOfFrame();
            var anchors=Enumerable.Range(0,NightmareGrandStage.Capacity).Select(stage.FixtureAnchor).ToArray();
            var floors=Enumerable.Range(0,NightmareGrandStage.Capacity).Select(i=>owner.transform.Find("DreamArchitecture"+i).GetComponent<GroundShadow>().Contact.transform.position).ToArray();
            yield return Shot("04-world-scenery-intact");
            owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=viewpoint+Vector2.right*4;
            yield return new WaitForSecondsRealtime(.6f);
            Check(anchors.SequenceEqual(Enumerable.Range(0,NightmareGrandStage.Capacity).Select(stage.FixtureAnchor)),"Moving the hero/camera cannot move scenery anchors");
            Check(floors.SequenceEqual(Enumerable.Range(0,NightmareGrandStage.Capacity).Select(i=>owner.transform.Find("DreamArchitecture"+i).GetComponent<GroundShadow>().Contact.transform.position)),"Actual floor shadows stay fixed while the camera moves");
            yield return Shot("05-world-scenery-after-camera-move");
            foreach(int wave in new[]{11,17})
            {
                yield return Load(wave);yield return new WaitForSecondsRealtime(.4f);stage=owner.GetComponent<NightmareGrandStage>();
                owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=viewpoint;
                FindAnyObjectByType<ArenaCamera>().Snap();yield return new WaitForEndOfFrame();
                // the arena layout may step a piece aside for this zone's props (planned once per arena, never moved within a fight)
                var layout=ArenaLayout.Combat(owner);
                Check(layout!=null && Enumerable.Range(0,NightmareGrandStage.Capacity).All(i=>layout.TryGet("architecture"+i,out var spot) && spot.Visible && spot.Foot==stage.FixtureAnchor(i)),"Damaged scenery stands on its planned arena spots "+wave);
                for(int i=0;i<NightmareGrandStage.Capacity;i++)
                {var sprite=owner.transform.Find("DreamArchitecture"+i).GetComponent<SpriteRenderer>();Check(sprite.sprite==WaitingSceneryArtwork.Current.Frame(i%4,NightmareGrandStage.DamageFor(i,wave,171026)),"Actual scenery damage art matches wave "+wave+" / "+i);}
                yield return Shot(wave==11?"06-world-scenery-cracked":"07-world-scenery-broken");
            }
            yield return Load(3);
            // Freeze only this capture controller. A gameplay pause would cover the art with its menu.
            owner.enabled=false;
            Check(owner.Simulation.Phase==RunPhase.Combat,"Art gallery freezes without a pause-menu overlay");
            var art=MechanicEffectArtwork.Current;var gallery=new GameObject("Illustrated mechanic gallery");
            for(int page=0;page<4;page++)
            {
                var views=new List<PaintedMechanicView>();labels=new string[12];labelPositions=new Vector2[12];
                for(int slot=0;slot<12;slot++)
                {
                    int index=page<3?page*12+slot:slot;if(page<3 && index>=31)continue;
                    Sprite sprite=page<3?art.Hero(HeroIdentities.All[index].Id):art.Form((BuildMorphKind)(index+1));
                    labels[slot]=page<3?HeroIdentities.All[index].Id:BuildMorphs.Get((BuildMorphKind)(index+1)).Name;
                    Vector2 p=new Vector2((slot%4-1.5f)*3,1.7f-slot/4*1.8f);labelPositions[slot]=p+Vector2.down*.7f;
                    var view=new PaintedMechanicView(gallery.transform,"Gallery "+labels[slot],1200);
                    view.Display(sprite,MorphGeometry.Disc,p,p,Vector2.right,.65f,0,1,.28f,Color.white,reduced);views.Add(view);
                    Check(view.VisibleSprites>0,"Illustrated mechanic renders "+labels[slot]);
                }
                Check(gallery.GetComponentsInChildren<LineRenderer>().Length==0,"Mechanic gallery has no primitive outlines page="+page);
                yield return Shot("08-mechanic-art-page-"+page);
                foreach(var view in views)view.Hide();
            }
            labels=null;Destroy(gallery);owner.enabled=true;
            Check(!failed,"Combat repair native verification complete");Application.Quit(failed?1:0);
        }
        private void OnGUI()
        {
            if(labels==null || owner==null)return;var camera=FindAnyObjectByType<ArenaCamera>()?.GetComponent<Camera>();if(camera==null)return;
            var style=new GUIStyle(GUI.skin.label){alignment=TextAnchor.MiddleCenter,fontSize=14};style.normal.textColor=new Color(1,.94f,.78f);
            for(int i=0;i<labels.Length;i++)if(labels[i]!=null){Vector3 p=camera.WorldToScreenPoint(labelPositions[i]);GUI.Label(new Rect(p.x-130,Screen.height-p.y,260,24),labels[i],style);}
        }
        private void OnDestroy()=>Application.logMessageReceived-=Log;
    }
}
