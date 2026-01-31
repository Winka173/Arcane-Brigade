using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public sealed class MienChoCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-miencho-capture") >= 0;
        public static bool AllowAutomaticScenes { get; private set; }
        private string root;
        private bool failed, reduced;
        private RunCatalog content;
        private ArenaController owner;
        private readonly List<string> checks = new List<string>();
        private void Check(bool value, string text)
        {
            checks.Add((value ? "PASS " : "FAIL ") + text); failed |= !value;
            File.WriteAllLines(Path.Combine(root, "Native_Checks.txt"), checks);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<MienChoCapture>() != null) return;
            var go = new GameObject("MienChoNativeVerification"); DontDestroyOnLoad(go); go.AddComponent<MienChoCapture>();
        }
        private void Log(string message, string trace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            Check(false, message + "\n" + trace); Application.Quit(1);
        }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var frame = ScreenCapture.CaptureScreenshotAsTexture();
            int visible = 0; for (int y = 0; y < frame.height; y += 24) for (int x = 0; x < frame.width; x += 24) if (frame.GetPixel(x, y).maxColorComponent > .12f) visible++;
            Check(visible > 60 && frame.width >= 1280, "Actual nonblack native frame " + name);
            string path=Path.Combine(root,name+".png"),temporary=path+".writing";
            File.WriteAllBytes(temporary,frame.EncodeToPNG());
            if(File.Exists(path))File.Replace(temporary,path,null);else File.Move(temporary,path);
            Destroy(frame);
        }
        private IEnumerator Load(int wave, int companions, bool shop = false, int seed = 161026)
        {
            using (var s = new RunSession(content))
            {
                s.Start("LanternKnight", seed); s.State.Wave = wave; s.State.Phase = shop ? SessionPhase.Shop : SessionPhase.Combat;
                s.State.Gold = 1200; s.State.Eggs = 20; s.State.Roster.Clear();
                foreach (var u in content.Companions.Take(companions)) s.State.Roster.Add(new RosterEntry { UnitId = u.Id, WeaponId = "SpoonSabre" });
                new RunStorage(RunPaths.Root).SaveRun(s.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            yield return new WaitForSecondsRealtime(.7f);
            owner = FindAnyObjectByType<ArenaController>();
            if (!shop) owner.Simulation.PrepareMienChoCapture(content);
            yield return new WaitForSecondsRealtime(reduced ? .8f : 4);
            if (shop)
            {
                float deadline = Time.realtimeSinceStartup + 6;
                while (owner.Shop.Animating && Time.realtimeSinceStartup < deadline) yield return null;
                Check(!owner.Shop.Animating, "Shop entrance has finished before interaction fixture " + wave);
            }
            Check(SharedLightField.Active != null && SharedLightField.Active.SourceCount == companions + 1, "Hero and exact companion light budget wave=" + wave + " party=" + companions);
            Check(owner.GetComponent<WaitingRealmStage>().ChairVisible, "Drawn chair appears at world boundary " + wave);
        }
        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true;
            Application.logMessageReceived += Log; reduced = Array.IndexOf(args, "-miencho-reduced") >= 0;
            content = Resources.Load<RunCatalog>("RunCatalog");
            new RunStorage(RunPaths.Root).SaveProfile(new PlayerProfile { BestWave = 40, TutorialSeen = true, ReducedMotion = reduced, ReducedEffects = reduced, ReducedFlashes = reduced, HighReadability = reduced });
            Check(RunPaths.Root.Contains("ArcaneBrigadeDiagnostics"), "Real player profile is isolated");
            if (Array.IndexOf(args, "-miencho-lanterns") >= 0)
            {
                // Keep the launch resolution: changing a hidden DX11 window can stall DXGI.
                yield return LanternReview();
                Check(!failed,"Smaller seeded lantern verification finished");Application.Quit(failed?1:0);yield break;
            }
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.4f);
            Screen.SetResolution(1920, 1080, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.6f);
            if (Array.IndexOf(args, "-miencho-actone") >= 0)
            {
                yield return ActOneReview();
                Check(!failed,"Act One lantern, fog and walkable-room verification finished");Application.Quit(failed?1:0);yield break;
            }
            if (Array.IndexOf(args, "-miencho-encounters") >= 0)
            {
                yield return EncounterReview();
                Check(!failed, "Waiting shop encounters and reading review finished"); Application.Quit(failed ? 1 : 0); yield break;
            }
            yield return Load(1, 0); yield return Shot("01-first-crossing-solo");
            yield return Load(9, 6); yield return Shot("02-middle-six-lights");
            yield return Load(19, 0); yield return Shot("03-endless-night-solo");
            var enemy = owner.Simulation.Units.First(u => u.Faction == Faction.Enemy && !u.Definition.IsBoss);
            float far = SharedLightField.Active.Visibility(enemy, owner.Simulation.Elapsed);
            Check(far < (reduced ? .9f : .6f), "Far real enemy keeps a silhouette in darkness " + far);
            enemy.PendingAttack = true; Check(SharedLightField.Active.Visibility(enemy, owner.Simulation.Elapsed) == 1, "Attack windup fully reveals a distant attacker"); enemy.PendingAttack = false;
            Vector2 oldPosition = enemy.Position;
            enemy.Position = owner.Simulation.Champion.Position + Vector2.right * 3;
            Check(SharedLightField.Active.Visibility(enemy, owner.Simulation.Elapsed) == 1, "Contact approach fully revealed before reaching hero"); enemy.Position = oldPosition;
            owner.TogglePause(); float clock = owner.PresentationClock;
            Vector4 source = Shader.GetGlobalVector("_SharedLightRect"); yield return new WaitForSecondsRealtime(.25f);
            Check(clock == owner.PresentationClock && source == Shader.GetGlobalVector("_SharedLightRect"), "Manual pause freezes presentation and light mapping"); owner.TogglePause();
            yield return Load(19, 15); yield return Shot("04-endless-night-fifteen-lights");
            Check(SharedLightField.Active.Strength(owner.Simulation.Champion.Position) <= 1, "Fifteen companions cannot multiply illumination beyond one");
            Check(FindObjectsByType<NightmarePresence>().Sum(p => p.VisibleCues) > 0, "Eye or ember cues belong to real live enemy actors");
            double total = 0; int frames = 120;
            for (int i = 0; i < frames; i++) { yield return null; total += Time.unscaledDeltaTime * 1000; }
            File.WriteAllText(Path.Combine(root, "FrameTiming.txt"), "120 native frames / average ms=" + (total / frames).ToString("F2") + "\nVSync/cap and capture overhead included; not a hardware-independent benchmark.");
            yield return Load(20, 5); yield return Shot("05-customer-boss-readable");
            Check(SharedLightField.Active.Visibility(owner.Simulation.Boss, owner.Simulation.Elapsed) >= .8f, "Boss silhouette never hidden by darkness");
            yield return Load(16, 6, true); yield return Shot("06-warm-shop-prices-readable");
            var journal = owner.GetComponent<MienChoPresentation>(); journal.Show(5);
            Check(owner.PresentationPaused && owner.Simulation.Phase == RunPhase.Paused, "Opening story journal actually pauses simulation");
            yield return Shot("07-intercepted-letter-1080");
            journal.Advance(); yield return Shot("08-physical-evidence-1080");
            Screen.SetResolution(1280, 720, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.4f);
            yield return Shot("09-journal-scroll-720"); journal.Close();
            Check(!owner.PresentationPaused && owner.Session.State.MienCho.ReadMask == 1 << 5, "Close persists exact evidence acknowledgement and resumes world");
            var saved = new RunStorage(RunPaths.Root).LoadRun(owner.Session);
            Check(saved.MienCho.ReadMask == 1 << 5, "Actual disk save retains new journal");
            journal.Show(5); journal.ShowLives(); yield return Shot("09b-companion-life-and-arc-720"); journal.Close();
            owner.Session.State.Nightmare.CustomerDefeated = true;
            yield return new WaitForSecondsRealtime(reduced ? .5f : 4); yield return Shot("10-real-dawn-restores-world");
            Check(NightmareLighting.Active.Dawn > .95f && NightmareLighting.Active.Ambient > 1, "Actual runtime dawn restores environmental illumination");
            yield return Load(20, 6);
            owner.Simulation.DefeatCustomerCapture(); yield return new WaitForSecondsRealtime(4.8f);
            Check(owner.Session.State.Phase == SessionPhase.Victory && owner.Session.EndingChoicePending, "Real fixed-step boss defeat reaches the two true dawn choices");
            string fork = JsonUtility.ToJson(owner.Session.State);
            foreach (var choice in new[] { NightmareFarewell.CloseShop, NightmareFarewell.LeaveLight })
            {
                Check(owner.ChooseNightmareEnding(choice), "Chosen farewell persists " + choice);
                int pages = choice == NightmareFarewell.LeaveLight ? 5 : 4;
                for (int p = 0; p < pages; p++)
                {
                    yield return new WaitForSecondsRealtime(p == 4 ? 2.9f : 1.4f);
                    yield return Shot("ending-" + choice + "-" + p);
                    owner.AdvanceNightmareEnding();
                }
                Check(owner.ResultsReady && !owner.Session.EndingPending, "Narrative closes and results unlock " + choice);
                if (choice == NightmareFarewell.CloseShop)
                {
                    new RunStorage(RunPaths.Root).SaveRun(JsonUtility.FromJson<RunSnapshot>(fork));
                    RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2);
                    owner = FindAnyObjectByType<ArenaController>();
                }
            }
            AllowAutomaticScenes = true;
            yield return Load(1, 0);
            Check(owner.StoryOpen && owner.PresentationPaused, "Actual automatic arrival opens before first combat input");
            yield return Shot("00-automatic-arrival");
            owner.GetComponent<MienChoPresentation>().Close(); AllowAutomaticScenes = false;
            Check(owner.Session.State.MienCho.ReadMask == 1 && !owner.PresentationPaused, "Read-later acknowledgement resumes first combat without repeated intro");
            Check(!failed, "Waiting Realm native verification finished"); Application.Quit(failed ? 1 : 0);
        }
        private static int FogSignature(RenderTexture target)
        {
            var previous=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();RenderTexture.active=previous;
            int hash=17;unchecked{for(int y=7;y<target.height;y+=19)for(int x=7;x<target.width;x+=19)hash=hash*31+((Color32)texture.GetPixel(x,y)).a;}
            UnityEngine.Object.Destroy(texture);return hash;
        }
        private IEnumerator LanternReview()
        {
            Vector2[] first=null;
            foreach(int wave in new[]{1,9,19})
            {
                yield return Load(wave,4);
                var field=NightLanternField.Active;
                CheckFloatingLanterns(field,false,wave);
                var anchors=Enumerable.Range(0,8).Select(field.FixtureAnchor).ToArray();
                if(first==null)first=anchors;
                else Check(first.SequenceEqual(anchors),"Damaged lamps stay at the original run locations wave="+wave);
                Check(field.BrokenCount==NightLanternField.BrokenFor(wave),"Damage progression survives smaller scattered lamps wave="+wave);
                yield return Shot("scattered-lamps-wave-"+wave);
                var before=LanternPoses(field);yield return new WaitForSecondsRealtime(1.1f);
                Check(reduced?before.SequenceEqual(LanternPoses(field)):before.Where((p,i)=>Vector3.Distance(p,LanternPoses(field)[i])>.001f).Count()==8,"All eight lamps respect the motion setting wave="+wave);
                owner.TogglePause();before=LanternPoses(field);float clock=field.Clock;
                yield return new WaitForSecondsRealtime(.3f);
                Check(before.SequenceEqual(LanternPoses(field)) && clock==field.Clock,"Pause freezes seeded lamps wave="+wave);owner.TogglePause();
            }
            yield return Load(1,4,false,170027);
            Check(Enumerable.Range(0,8).All(i=>Vector2.Distance(first[i],NightLanternField.Active.FixtureAnchor(i))>.01f),"A different saved run seed changes every lamp location");
            CheckFloatingLanterns(NightLanternField.Active,false,1);
            yield return Shot("scattered-lamps-another-run");
            yield return Load(1,4);
            Check(first.SequenceEqual(Enumerable.Range(0,8).Select(NightLanternField.Active.FixtureAnchor)),"Reloading the original run restores all lamp locations");
            yield return Load(8,4,true);
            CheckFloatingLanterns(NightLanternField.Active,true,8);
            Check(NightLanternField.Active.LitCount==2,"Both smaller shop lamps stay lit");
            for(int i=0;i<2;i++)
            {
                Check(owner.Shop.Stations.All(s=>Vector2.Distance(s.Position,NightLanternField.Active.FixtureAnchor(i))>1.2f),"Shop lamp stays clear of actual stations "+i);
                var camera=FindAnyObjectByType<ArenaCamera>().GetComponent<Camera>();
                var sprite=NightLanternField.Active.transform.Find("AntiqueLanternSprite"+i).GetComponent<SpriteRenderer>();
                Vector3 low=camera.WorldToViewportPoint(sprite.bounds.min),high=camera.WorldToViewportPoint(sprite.bounds.max);
                Check(low.x>=0 && low.y>=0 && high.x<=1 && high.y<=1,"Smaller shop lamp fits inside the camera frame "+i);
            }
            yield return Shot("smaller-asymmetric-shop-lamps");
        }
        private IEnumerator ActOneReview()
        {
            foreach(int wave in new[]{1,9,19})
            {
                yield return Load(wave,4);yield return null;
                var lanterns=owner.GetComponent<NightLanternField>();
                Check(RenderTexture.active!=lanterns.FogTexture && RenderTexture.active!=lanterns.LightTexture,"Offscreen light/fog passes restore the render target wave="+wave);
                Check(lanterns!=null && lanterns.FogTexture!=null && lanterns.LightTexture!=null,"Actual fog and lantern GPU targets exist wave="+wave);
                Check(lanterns.BrokenCount==NightLanternField.BrokenFor(wave) && lanterns.LitCount==8-lanterns.BrokenCount,"Live fixture damage and lit count wave="+wave);
                Check(lanterns.PracticalCount==0,"Sprite lamps use Unity light field without Blender mesh lights wave="+wave);
                var poses=CheckFloatingLanterns(lanterns,false,wave);
                var chair=owner.GetComponent<WaitingRealmStage>().GetComponentInChildren<FloatingStoryProp>();
                Vector3 chairPosition=chair.transform.position;
                var old=RenderTexture.active;RenderTexture.active=lanterns.FogTexture;
                var sample=new Texture2D(320,240,TextureFormat.RGBA32,false);sample.ReadPixels(new Rect(0,0,320,240),0,0);sample.Apply();RenderTexture.active=old;
                float maximum=0,minimum=1;for(int y=8;y<232;y+=13)for(int x=8;x<312;x+=13){float a=sample.GetPixel(x,y).a;maximum=Mathf.Max(maximum,a);minimum=Mathf.Min(minimum,a);}
                Check(maximum>0 && maximum>minimum,"Fog pass contains spatially varying optical density wave="+wave);Destroy(sample);
                yield return Shot("lamps-and-fog-wave-"+wave);
                int fogBefore=FogSignature(lanterns.FogTexture);yield return new WaitForSecondsRealtime(1.1f);
                Check(reduced || FogSignature(lanterns.FogTexture)!=fogBefore,"GPU fog advects with live presentation clock wave="+wave);
                var after=CheckFloatingLanterns(lanterns,false,wave);
                Check(reduced?poses.SequenceEqual(after):poses.Where((p,i)=>Vector3.Distance(p,after[i])>.001f).Count()==8,"Every lamp pose obeys motion setting wave="+wave);
                Check(reduced?chairPosition==chair.transform.position:Vector3.Distance(chairPosition,chair.transform.position)>.001f,"Narrative chair floats with live clock wave="+wave);
                if(wave==1)yield return Shot("lamps-second-animation-pose");
                owner.TogglePause();var pausedPoses=LanternPoses(lanterns);float clock=lanterns.Clock;int pausedFog=FogSignature(lanterns.FogTexture);yield return new WaitForSecondsRealtime(.3f);
                Check(FogSignature(lanterns.FogTexture)==pausedFog,"Paused GPU fog texture remains identical wave="+wave);
                Check(lanterns.Clock==clock,"Pause freezes lantern variation and fog advection wave="+wave);owner.TogglePause();
                Check(pausedPoses.SequenceEqual(LanternPoses(lanterns)),"Pause freezes every physical lamp pose wave="+wave);
            }
            yield return Load(19,0);yield return Shot("deep-night-solo");
            Check(SharedLightField.Active.SourceCount==1 && NightLanternField.Active.LitCount==0,"Solo keeps own light after every environmental lamp breaks");
            yield return Load(19,15);yield return Shot("deep-night-fifteen");
            Check(SharedLightField.Active.SourceCount==16 && SharedLightField.Active.Strength(owner.Simulation.Champion.Position)<=1,"Fifteen companions retain bounded independent light");
            double total=0;for(int i=0;i<120;i++){yield return null;total+=Time.unscaledDeltaTime*1000;}
            File.WriteAllText(Path.Combine(root,"FrameTiming.txt"),"120 native frames with 15 companions and fog / average ms="+(total/120).ToString("F2")+"\nIncludes frame cap and capture workload; diagnostic fixture, not an unrestricted battle benchmark.");
            foreach(int wave in new[]{8,14,16})
            {
                yield return Load(wave,4,true);
                CheckFloatingLanterns(NightLanternField.Active,true,wave);
                int gold=owner.Session.State.Gold,bodies=owner.Session.State.Roster.Count;
                var journey=owner.GetComponent<WaitingJourney>();journey.Show();
                Check(journey.Open && owner.PresentationPaused,"Paths and people pauses ordinary shop wave="+wave);
                yield return Shot("paths-and-people-"+wave);
                Check(journey.EnterRoom(),"Actual room entry saves shop boundary wave="+wave);
                yield return new WaitForSecondsRealtime(1);
                var room=FindAnyObjectByType<WaitingRoomController>();
                Check(room!=null && room.Room==WaitingCampaign.RoomFor(0,wave),"Actual independent walkable room scene loaded wave="+wave);
                Check(!room.TryInspect(0),"Room evidence cannot be examined from across the floor wave="+wave);
                var records=room.GetComponentsInChildren<FloatingStoryProp>().Where(p=>p.name=="A physical record").ToArray();
                var recordPoses=records.Select(p=>p.transform.position).ToArray();
                Check(records.Length==3 && records.All(p=>p.GetComponent<GroundShadow>().Contact.enabled && p.GetComponentsInChildren<LineRenderer>().Length==0),"All physical room records have floor shadows without under-object wind wave="+wave);
                yield return new WaitForSecondsRealtime(.9f);
                Check(reduced?records.Select(p=>p.transform.position).SequenceEqual(recordPoses):records.Where((p,i)=>Vector3.Distance(p.transform.position,recordPoses[i])>.001f).Count()==3,"Room evidence animation obeys motion setting wave="+wave);
                yield return Shot("room-"+wave+"-world");
                for(int point=0;point<3;point++)
                {
                    room.DiagnosticApproach(point);yield return null;
                    Check(room.TryInspect(point),"Walk to physical evidence point "+wave+" / "+point);
                    var frozen=records.Select(p=>p.transform.position).ToArray();yield return new WaitForSecondsRealtime(.2f);
                    Check(records.Select(p=>p.transform.position).SequenceEqual(frozen),"Reading physical evidence freezes prop motion "+wave+" / "+point);
                    Check(room.Choose(1+(point&1)),"Real room response committed "+wave+" / "+point);
                    Check(!room.Choose(1),"Recorded response cannot be rewritten "+wave+" / "+point);
                    if(point==0)yield return Shot("room-"+wave+"-evidence");room.CloseRecord();
                }
                int mask=room.Session.State.WaitingCampaign.EvidenceMask;
                SceneManager.LoadScene("WaitingRoom");yield return new WaitForSecondsRealtime(.7f);room=FindAnyObjectByType<WaitingRoomController>();
                Check(room.Session.State.WaitingCampaign.EvidenceMask==mask,"Room choices survive real scene reload from disk wave="+wave);
                Check(room.ReturnToShop(),"Room exit saves before returning to shop wave="+wave);
                yield return new WaitForSecondsRealtime(2);owner=FindAnyObjectByType<ArenaController>();
                Check(owner.Session.State.Gold==gold && owner.Session.State.Roster.Count==bodies && owner.Session.State.WaitingCampaign.Room==0,"Exploration preserves wallet and roster and clears room mode wave="+wave);
            }
            yield return Load(18,6,true);
            owner.Session.State.Roster.Clear();foreach(var r in WaitingCompanionQuests.Routes)owner.Session.State.Roster.Add(new RosterEntry{UnitId=r.Id,WeaponId="SpoonSabre"});
            for(int route=0;route<6;route++)for(int part=0;part<3;part++)Check(owner.Session.ChooseWaitingCompanion(route,1+((route+part)&1)),"Actual session companion branch "+route+" / "+part);
            Check(owner.SaveNightmareBoundary(),"Companion branch state saves to real disk");
            var saved=new RunStorage(RunPaths.Root).LoadRun(owner.Session);
            Check(WaitingCompanionQuests.Next(saved,5)==-1 && WaitingCompanionQuests.Futures(saved).Length>200,"Recorded companion futures reach authored epilogue text");
            owner.GetComponent<WaitingJourney>().Show();yield return Shot("companion-decisions");owner.GetComponent<WaitingJourney>().Close();
            yield return Load(20,5);yield return Shot("customer-in-deep-fog");
            Check(SharedLightField.Active.Visibility(owner.Simulation.Boss,owner.Simulation.Elapsed)>=.8f,"Final boss remains readable through deep fog");
        }
        private Vector3[] LanternPoses(NightLanternField field)
        {
            return Enumerable.Range(0,8).Select(i=>field.transform.Find("AntiqueLanternSprite"+i).position).ToArray();
        }
        private Vector3[] CheckFloatingLanterns(NightLanternField field,bool shop,int wave)
        {
            for(int i=0;i<8;i++)
            {
                var sprite=field.transform.Find("AntiqueLanternSprite"+i).GetComponent<SpriteRenderer>();
                var air=field.transform.Find("LanternSuspension"+i).GetComponentsInChildren<LineRenderer>();
                var shadow=sprite.GetComponent<GroundShadow>();bool visible=!shop || i<2;
                Vector2 floor=field.FixtureAnchor(i);
                Vector3 foot=field.FixtureFoot(i);
                Check(visible?foot.y-floor.y>.2f && Vector2.Distance(shadow.Contact.transform.position,floor)<.001f:!sprite.enabled && (shadow.Contact==null || !shadow.Contact.enabled),"Lamp air gap and fixed floor shadow "+wave+" / "+i);
                Check(air.Length==0 && !field.transform.Find("LanternSuspension"+i).GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name=="PaintedSuspensionMist"),"No under-object wind ribbons or smoke "+wave+" / "+i);
                Check(!reduced || Quaternion.Angle(sprite.transform.rotation,Quaternion.Euler(0,0,NightLanternField.RestTiltFor(i,owner.Session.State.Seed)))<.01f,"Reduced motion keeps the static authored lamp lean "+wave+" / "+i);
                float height=sprite.sprite.bounds.size.y*sprite.transform.localScale.y;
                Check(height>=NightLanternField.MinimumHeight-.001f && height<=NightLanternField.MaximumHeight+.001f,"Actual drawn lamp is smaller than the previous 2.7 unit fixture "+wave+" / "+i);
                var model=field.GetComponentsInChildren<Transform>().FirstOrDefault(t=>t.name.StartsWith("Antique Lantern "+i+" /"));
                Check(model==null && sprite.enabled==visible,"Only drawn lamp sprites are displayed "+wave+" / "+i);
            }
            foreach(var halo in field.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.enabled && r.name.StartsWith("Flame Bloom")))
            {
                int i=int.Parse(halo.name.Substring("Flame Bloom ".Length));
                Check(Vector2.Distance(halo.transform.position,field.SourcePosition(i)+Vector2.up*field.FlameFloorDrop(i))<.001f,"Unity light field remains attached to drawn moving flame "+wave);
            }
            return LanternPoses(field);
        }
        private void OnDestroy() => Application.logMessageReceived -= Log;
        private IEnumerator EncounterReview()
        {
            foreach (int wave in new[] { 4, 6, 10 })
            {
                yield return Load(wave, 4, true);
                var encounter = owner.GetComponent<WaitingShopEncounter>();
                Check(encounter.Visible && encounter.Active == WaitingShopEncounter.ForWave(wave), "Correct physical encounter appears in ordinary shop " + wave);
                Check(!encounter.TryInspect(), "Cannot examine encounter from across shop " + wave);
                owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = WaitingShopEncounter.Anchor + Vector2.down * .4f;
                yield return new WaitForSecondsRealtime(.3f);
                Check(encounter.Near, "Actual champion approaches new corner prop " + wave);
                yield return Shot("encounter-" + wave + "-world");
                if (wave == 6)
                {
                    yield return new WaitForSecondsRealtime(1.7f);
                    Check(encounter.RingCount == 1, "Phone answers actual approach with a bounded ring cue");
                }
                if (wave == 10)
                {
                    Check(owner.Simulation.Units.Any(u => u.Faction == Faction.Companion && u.Id == encounter.ReflectionUnitId), "Mirror shows an actual companion instead of an unrelated creature");
                    Screen.SetResolution(1280, 720, FullScreenMode.Windowed); yield return new WaitForSecondsRealtime(.4f);
                    yield return Shot("encounter-mirror-720");
                }
                owner.TogglePause(); float clock = encounter.AnimationClock; int rings = encounter.RingCount;
                yield return new WaitForSecondsRealtime(.3f);
                Check(encounter.AnimationClock == clock && encounter.RingCount == rings && !encounter.TryInspect(), "Pause freezes physical prop / audio schedule and blocks examine " + wave);
                owner.TogglePause(); yield return null;
                int gold = owner.Session.State.Gold, eggs = owner.Session.State.Eggs, party = owner.Session.State.Roster.Count;
                Check(encounter.TryInspect(), "Examine opens existing authored evidence in real journal " + wave);
                var journal = owner.GetComponent<MienChoPresentation>();
                Check(journal.Open && journal.Chapter == WaitingShopEncounter.ChapterFor(encounter.Active) && owner.StoryConsumesInput, "Correct chapter and shared-input guard " + wave);
                journal.Advance(); Check(journal.Page == 1, "Reading advances to saved page two " + wave);
                journal.Close();
                Check(!journal.Open && owner.StoryConsumesInput, "Closing still consumes the input frame " + wave);
                Check(owner.Session.State.Gold == gold && owner.Session.State.Eggs == eggs && owner.Session.State.Roster.Count == party, "Story does not buy items or change build " + wave);
                var saved = new RunStorage(RunPaths.Root).LoadRun(owner.Session);
                int chapter = WaitingShopEncounter.ChapterFor(encounter.Active);
                Check(saved.MienCho.Pages[chapter] == 1 && saved.MienCho.EncounterMask == (1 << (int)encounter.Active), "Actual disk save retains page and examined scene " + wave);
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2);
                owner = FindAnyObjectByType<ArenaController>(); journal = owner.GetComponent<MienChoPresentation>(); journal.Show(chapter);
                Check(journal.Page == 1, "Scene reload and reopen resumes exact page " + wave);
                journal.ShowLives(); journal.Advance();
                Check(journal.Open && owner.StoryOpen, "Controller confirm returns from companion lives without closing journal " + wave);
                journal.Close();
                Check((owner.Session.State.MienCho.FinishedMask & 1 << chapter) == 0, "Reading later is distinct from chapter completion " + wave);
                journal.Show(chapter);
                for (int i = 0; i < 5 && journal.Open; i++) journal.Advance();
                Check((owner.Session.State.MienCho.FinishedMask & 1 << chapter) != 0, "Last-page confirmation records actual chapter completion " + wave);
            }
            yield return Load(11, 0, true);
            var solo = owner.GetComponent<WaitingShopEncounter>();
            Check(solo.ReflectionUnitId == owner.Simulation.Champion.Id && solo.Visible, "Solo mirror uses actual hero; no companion requirement");
            yield return Load(12, 4, true);
            Check(!owner.GetComponent<WaitingShopEncounter>().Visible, "Encounter presentation clears outside its authored shop window");
            yield return Load(10, 4);
            Check(!owner.GetComponent<WaitingShopEncounter>().Visible && !owner.GetComponent<WaitingShopEncounter>().TryInspect(), "No shop encounters or interactions during live combat");
            owner.TogglePause(); owner.GetComponent<MienChoPresentation>().Show(0);
            Check(!owner.StoryOpen && owner.PresentationPaused, "Journal cannot replace a manual pause and resume by accident"); owner.TogglePause();
        }
    }

    public sealed partial class CombatSimulation
    {
        public void PrepareMienChoCapture(RunCatalog content)
        {
            if (!MienChoCapture.Requested && !CombatRepairCapture.Requested) throw new InvalidOperationException("Isolated Waiting Realm capture required");
            catalog.EnemyBudget = 0;
            Champion.Position = Champion.PreviousPosition = Vector2.zero;
            Champion.Health = Champion.Definition.Health;
            catalog.Armor = 1000;
            int companion = 0;
            foreach (var unit in units)
            {
                if (unit.Faction == Faction.Companion)
                {
                    float angle = companion++ * 2.4f;
                    unit.Position = unit.PreviousPosition = new Vector2(Mathf.Cos(angle) * 3.8f, Mathf.Sin(angle) * 2.4f);
                }
                if (unit.Faction == Faction.Enemy && !unit.Definition.IsBoss) unit.Alive = false;
            }
            if (Boss == null)
                for (int i = 0; i < 12; i++)
                {
                    var definition = UnityEngine.Object.Instantiate(content.Enemies[i % 10]);
                    definition.Health = 100000; definition.Speed = .1f; definition.ContactDamage = 0;
                    var position = new Vector2((i % 2 == 0 ? -1 : 1) * (7.2f + i % 3 * .7f), -4 + i / 2 * 1.5f);
                    var unit = Spawn(definition, Faction.Enemy, position); unit.NextAttack = unit.NextContact = float.PositiveInfinity;
                }
            if (Boss != null)
            {
                Boss.EntranceUntil = 0; Boss.Position = Boss.PreviousPosition = new Vector2(0, 2.8f);
                Boss.NextBossPattern = float.PositiveInfinity; Boss.Definition.ContactDamage = 0;
                Boss.NextAttack = Boss.NextContact = float.PositiveInfinity;
            }
        }
    }
}
