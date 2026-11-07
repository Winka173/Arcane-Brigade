using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    // This is deliberately a shop-only presentation queue. Combat never reads or displays it.
    public sealed class ShopStatFeedback
    {
        private sealed class ActorQueue { public string Key;public float Started=-1;public readonly List<ShopStatDelta> Rows=new List<ShopStatDelta>(); }
        private readonly List<ActorQueue> pending=new List<ActorQueue>();
        private readonly List<Rect> placed=new List<Rect>();
        private GUIStyle text;
        private string run;
        private int observed=-1;
        public const float Duration=1.65f;
        public int PendingActors=>pending.Count;
        public void Observe(RunSession session)
        {
            if(run!=session.State.RunId){run=session.State.RunId;observed=-1;pending.Clear();}
            if(session.State.Phase!=SessionPhase.Shop){pending.Clear();observed=session.ShopStatRevision;return;}
            if(observed==session.ShopStatRevision)return;
            observed=session.ShopStatRevision;
            if(MergeReveal.Busy)return; // a star-up shows its own stat lines
            foreach(var delta in session.LastShopStatChanges)
            {
                if(delta.Label=="Gold" || delta.Label=="Eggs" || delta.Label=="Next refresh cost" || delta.Label.StartsWith("Relic / "))continue;
                var actor=pending.Find(x=>x.Key==delta.ActorKey);
                if(actor==null){actor=new ActorQueue{Key=delta.ActorKey};pending.Add(actor);}
                actor.Rows.Add(delta);
            }
        }
        public void Draw(ArenaController owner,Camera camera)
        {
            var s=owner.Session;
            Observe(s);
            if(s.State.Phase!=SessionPhase.Shop || owner.Simulation.Phase!=RunPhase.Rest || owner.TransitionPending || owner.HallExitPending || owner.Details.Open || pending.Count==0 || MergeReveal.Busy)return; // a star-up plays first; its stat lines follow
            if(text==null){text=ArcadeTypography.Style(21,true);text.fontStyle=FontStyle.Bold;text.alignment=TextAnchor.MiddleLeft;}
            float clock=owner.PresentationClock;
            for(int i=Mathf.Min(8,pending.Count)-1;i>=0;i--)
            {
                var a=pending[i];if(a.Started<0)a.Started=clock;
                if(clock-a.Started<Duration)continue;
                pending.RemoveAt(i);a.Rows.RemoveRange(0,Mathf.Min(2,a.Rows.Count));
                if(a.Rows.Count>0){a.Started=-1;pending.Add(a);}
            }
            placed.Clear();
            for(int i=0;i<Mathf.Min(8,pending.Count);i++)
            {
                var a=pending[i];UnitState unit=a.Key=="hero"?owner.Simulation.Champion:null;
                if(unit==null)foreach(var u in owner.Simulation.Units)if(u.Faction==Faction.Companion && u.Alive && u.Definition.Id+"|"+u.Definition.Weapon.Id==a.Key){unit=u;break;}
                // requirement 26/10/2026: a friend's gain rises over that friend only; never over the hero when the friend is not found
                if(unit==null)continue;
                Vector2 world=unit.Position;
                Vector3 screen=camera.WorldToViewportPoint(world+Vector2.up*(unit.Definition.BodyHeight*.65f+.2f));
                if(a.Started<0)a.Started=clock;
                float age=clock-a.Started;
                float rise=PresentationSettings.ReducedMotion?0:Mathf.SmoothStep(0,1,Mathf.Clamp01(age/Duration))*28;
                float x=Mathf.Clamp(screen.x*1920-140,20,1620),y=Mathf.Clamp((1-screen.y)*1080-54-rise,175,940);
                Rect rect=new Rect(x,y,280,54);
                for(int tries=0;tries<20;tries++)
                {
                    bool overlap=SpeechBubbles.OverlapsDialogue(rect);foreach(var other in placed)if(rect.Overlaps(other)){overlap=true;break;}if(!overlap)break;
                    rect.y-=58;if(rect.y<175){rect.y=175;rect.x=Mathf.Clamp(rect.x+285,20,1620);}
                }
                placed.Add(rect);
                for(int n=0;n<Mathf.Min(2,a.Rows.Count);n++)
                {
                    var d=a.Rows[n];Color color=d.Neutral?ArcadeSkin.Gold:d.Positive?ArcadeSkin.Better:ArcadeSkin.Worse;
                    float t=Mathf.Max(0,age-n*.07f);float alpha=Mathf.Clamp01(t/.1f)*Mathf.Clamp01((Duration-age)/.35f);
                    text.fontSize=PresentationSettings.ReducedMotion?20:20+Mathf.RoundToInt(Mathf.Sin(Mathf.Clamp01(t/.2f)*Mathf.PI)*3);
                    // this purchase's own change in the stat's unit ("+12%", "+4 flat", "+0.15"), not the moved compiled total
                    string number=d.Display??Number(d.After-d.Before);
                    string label=d.Message??d.Label.Replace("Run / ","").Replace("Companion bullet interception","Interception")+" "+number;
                    while(text.fontSize>16 && text.CalcSize(LocalizedGUI.Content(label)).x>rect.width-30)text.fontSize--;
                    float start=rect.x+(rect.width-text.CalcSize(LocalizedGUI.Content(label)).x-29)*.5f;
                    Color old=GUI.color;GUI.color=new Color(old.r,old.g,old.b,old.a*alpha);
                    StatGlyph.Draw(new Rect(start,rect.y+n*25+3,23,23),Effect(d.Label));GUI.color=old;
                    Rect line=new Rect(start+29,rect.y+n*25,rect.width-29,29);
                    text.normal.textColor=new Color(.015f,.02f,.035f,alpha*.9f);
                    LocalizedGUI.Label(new Rect(line.x+1.5f,line.y+1.5f,line.width,line.height),label,text);
                    color.a=alpha;text.normal.textColor=color;LocalizedGUI.Label(line,label,text);LocalizedGUI.Label(new Rect(line.x+.4f,line.y,line.width,line.height),label,text);
                }
            }
            text.normal.textColor=ArcadeSkin.Cream;
        }
        public static string Number(double value)=>System.Math.Abs(value)<1 && value!=0?(value>0?"+<1":"-<1"):System.Math.Round(value,0,System.MidpointRounding.AwayFromZero).ToString("+0;-0;0",System.Globalization.CultureInfo.InvariantCulture);
        private static ItemEffect Effect(string label)=>label switch{
            "Current HP" or "Max HP"=>ItemEffect.Health,"Regen HP/s"=>ItemEffect.Heal,"Armor"=>ItemEffect.Armor,"Dodge %"=>ItemEffect.Dodge,
            "Crit %"=>ItemEffect.Crit,"Speed"=>ItemEffect.MoveSpeed,"Reach"=>ItemEffect.AttackRange,"Attacks/s"=>ItemEffect.AttackSpeed,
            "Shots"=>ItemEffect.Projectiles,"Pierce"=>ItemEffect.BulletPierce,"Split"=>ItemEffect.BulletSplit,"Bullet speed"=>ItemEffect.BulletSpeed,
            "Body size"=>ItemEffect.BodySize,"Contact damage"=>ItemEffect.CollisionDamage,"Contacts/s"=>ItemEffect.CollisionFrequency,"Push"=>ItemEffect.Knockback,
            "Pickup"=>ItemEffect.Pickup,"Chest reach"=>ItemEffect.ChestReach,"Bullet size"=>ItemEffect.BulletSize,"Bounce %"=>ItemEffect.BulletBounce,
            "Travel distance"=>ItemEffect.BulletDistance,"Heart healing"=>ItemEffect.HeartHealing,"Stars" or "Roster"=>ItemEffect.Luck,_=>ItemEffect.Damage};
    }
}
