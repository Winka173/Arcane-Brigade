using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace ArcaneBrigade
{
    public sealed class HeroIdentityCapture : MonoBehaviour
    {
        public static bool Requested=>Array.IndexOf(Environment.GetCommandLineArgs(),"-arcane-identity-capture")>=0;
        private string root;
        private bool failed;
        private readonly List<string> checks=new List<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init(){if(!Requested || FindAnyObjectByType<HeroIdentityCapture>()!=null)return;var go=new GameObject("HeroIdentityCapture");DontDestroyOnLoad(go);go.AddComponent<HeroIdentityCapture>();}
        private void Check(bool ok,string label){checks.Add((ok?"PASS ":"FAIL ")+label);failed|=!ok;File.WriteAllLines(Path.Combine(root,"Native_Checks.txt"),checks);}
        private void Log(string message,string trace,LogType type){if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert){Check(false,message+"\n"+trace);Application.Quit(1);}}
        private IEnumerator Shot(string name){yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();Check(image.width>=1280&&image.height>=720,"Native frame "+name+" / "+image.width+"x"+image.height);int bright=0;for(int y=0;y<image.height;y+=32)for(int x=0;x<image.width;x+=32){Color p=image.GetPixel(x,y);if(p.r+p.g+p.b>.15f)bright++;}Check(bright>40,"Rendered content is not black "+name);File.WriteAllBytes(Path.Combine(root,name+".png"),image.EncodeToPNG());Destroy(image);}
        private IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-capturePath");if(at<0||at+1>=args.Length){Application.Quit(1);yield break;}
            root=Path.GetFullPath(args[at+1]);Directory.CreateDirectory(root);Application.runInBackground=true;Application.logMessageReceived+=Log;
            bool reduced=Array.IndexOf(args,"-identity-reduced")>=0;
            var c=Resources.Load<RunCatalog>("RunCatalog");var storage=new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile{BestWave=40,TutorialSeen=true,ReducedMotion=reduced,ReducedEffects=reduced,ReducedFlashes=reduced});
            Check(RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"),"Capture uses isolated save directory");
            Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);
            Screen.SetResolution(1920,1080,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);
            using(var s=new RunSession(c))
            {
                s.Start("PetalCitadel",641031);s.State.Wave=9;s.State.Phase=SessionPhase.Shop;s.State.Gold=2000;s.State.Eggs=20;
                s.State.Roster.Clear();string[] ids={"TeapotPilgrim","HoodWisp","BrassAcorn","PillowOracle"};
                for(int i=0;i<ids.Length;i++)s.State.Roster.Add(new RosterEntry{UnitId=ids[i],WeaponId=i%2==0?"InkPistol":"SpoonSabre"});
                s.State.Items.Add(new ItemStack{Id="key-oil",Count=1});s.State.Items.Add(new ItemStack{Id="coin-salve",Count=1});
                s.State.Offers.Add(new ShopOffer{Token=++s.State.NextOffer,Id="key-oil",Kind=OfferKind.Item,Price=2});
                storage.SaveRun(s.State);
            }
            RunLaunch.Resume=true;SceneManager.LoadScene("RunArena");yield return new WaitForSecondsRealtime(2.6f);
            var owner=FindAnyObjectByType<ArenaController>();owner.ToggleDetails();owner.Details.SelectTab(0);
            float opened=Time.unscaledTime;for(int i=0;i<8 || Time.unscaledTime-opened<.4f;i++)yield return null;
            yield return Shot("01-hero-approach-1080");
            Check(owner.ConfigureHeroIdentity(()=>owner.Session.ChooseHeroApproach(1)),"Hero approach uses real committed shop transaction");
            Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.4f);yield return Shot("02-hero-approach-720");
            owner.Details.SelectTab(1);yield return new WaitForSecondsRealtime(.3f);yield return Shot("03-complete-companion-description");
            owner.ToggleDetails();yield return new WaitForSecondsRealtime(.15f);
            int token=owner.Session.State.Offers.First(o=>o.Id=="key-oil").Token;
            Check(owner.BuyOffer(token),"Native purchase succeeds");
            for(int i=0;i<200&&owner.TransitionPending;i++)yield return new WaitForSecondsRealtime(.02f);
            yield return new WaitForSecondsRealtime(.1f);
            Check(owner.Session.LastShopStatChanges.Any(d=>d.ActorKey=="hero"&&d.Label=="Damage")&&owner.Session.LastShopStatChanges.Any(d=>d.ActorKey!="hero"&&d.Label=="Damage"),"Native purchase reports hero and companion damage changes");
            yield return Shot("04-shop-stat-gains-720");
            owner.ToggleDetails();owner.Details.SelectTab(2);yield return new WaitForSecondsRealtime(.3f);yield return Shot("05-complete-relic-description");owner.ToggleDetails();
            using(var s=new RunSession(c)){s.Start("LanternWeaver",641032);s.State.Wave=3;storage.SaveRun(s.State);}
            RunLaunch.Resume=true;SceneManager.LoadScene("RunArena");yield return new WaitForSecondsRealtime(.3f);owner=FindAnyObjectByType<ArenaController>();
            owner.Simulation.PrepareIdentityCapture(c);
            for(int i=0;i<180;i++){var view=FindAnyObjectByType<HeroIdentityVisual>();if(view!=null&&view.VisibleCount>0&&owner.Simulation.HeroIdentityActivations>0)break;yield return new WaitForSecondsRealtime(.05f);}
            var identity=FindAnyObjectByType<HeroIdentityVisual>();Check(identity!=null&&identity.VisibleCount>0,"Real identity geometry visible in native player");
            Check(owner.Simulation.HeroIdentityActivations>0,"Live hero signature activated");yield return Shot("06-live-network-combat-720");
            Check(!failed,"Native identity and shop UI checks complete");Application.Quit(failed?1:0);
        }
        private void OnDestroy()=>Application.logMessageReceived-=Log;
    }
    public sealed partial class CombatSimulation
    {
        public void PrepareIdentityCapture(RunCatalog c)
        {
            if(!HeroIdentityCapture.Requested)throw new InvalidOperationException("Isolated identity capture required");
            catalog.EnemyBudget=0;catalog.Armor=100;Champion.Health=Champion.Definition.Health;
            var dummy=UnityEngine.Object.Instantiate(c.Enemies[0]);dummy.Speed=0;dummy.Health=100000;dummy.ContactDamage=0;dummy.Behavior=EnemyBehavior.Standard;
            for(int i=0;i<3;i++){var u=Spawn(dummy,Faction.Enemy,Champion.Position+new Vector2(.9f+i*.55f,(i-1)*.4f));u.NextAttack=u.NextContact=float.PositiveInfinity;}
        }
    }
}
