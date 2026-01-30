using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    // This is deliberately a shop-only presentation queue. Combat never reads or displays it.
    public sealed class ShopStatFeedback
    {
        private sealed class ActorQueue { public string Key,Name;public readonly List<ShopStatDelta> Rows=new List<ShopStatDelta>(); }
        private readonly List<ActorQueue> pending=new List<ActorQueue>();
        private readonly List<Rect> placed=new List<Rect>();
        private GUIStyle text,caption;
        private string run;
        private int observed=-1;
        private float age;
        public int PendingActors=>pending.Count;
        public void Observe(RunSession session)
        {
            if(run!=session.State.RunId){run=session.State.RunId;observed=-1;pending.Clear();age=0;}
            if(session.State.Phase!=SessionPhase.Shop){pending.Clear();age=0;observed=session.ShopStatRevision;return;}
            if(observed==session.ShopStatRevision)return;
            observed=session.ShopStatRevision;
            foreach(var delta in session.LastShopStatChanges)
            {
                var actor=pending.Find(x=>x.Key==delta.ActorKey);
                if(actor==null){actor=new ActorQueue{Key=delta.ActorKey,Name=delta.ActorName};pending.Add(actor);}
                actor.Rows.Add(delta);
            }
        }
        public void Draw(ArenaController owner,Camera camera)
        {
            var s=owner.Session;
            Observe(s);
            if(s.State.Phase!=SessionPhase.Shop || owner.Simulation.Phase!=RunPhase.Rest || owner.TransitionPending || owner.HallExitPending || owner.Details.Open || pending.Count==0)return;
            if(text==null){text=ArcadeTypography.Style(19,true);caption=ArcadeTypography.Style(15);}
            if(Event.current.type==EventType.Repaint)age+=Mathf.Min(.05f,Time.unscaledDeltaTime);
            if(age>=1.5f)
            {
                int shown=Mathf.Min(8,pending.Count);
                var visible=pending.GetRange(0,shown);pending.RemoveRange(0,shown);
                foreach(var a in visible){a.Rows.RemoveRange(0,Mathf.Min(2,a.Rows.Count));if(a.Rows.Count>0)pending.Add(a);}
                age=0;if(pending.Count==0)return;
            }
            placed.Clear();
            for(int i=0;i<Mathf.Min(8,pending.Count);i++)
            {
                var a=pending[i];UnitState unit=a.Key=="hero"?owner.Simulation.Champion:null;
                if(unit==null)foreach(var u in owner.Simulation.Units)if(u.Faction==Faction.Companion && u.Definition.Id+"|"+u.Definition.Weapon.Id==a.Key){unit=u;break;}
                Vector2 world=unit==null?owner.Simulation.Champion.Position:unit.Position;
                Vector3 screen=camera.WorldToViewportPoint(world+Vector2.up*(unit==null?.7f:unit.Definition.BodyHeight));
                float x=Mathf.Clamp(screen.x*1920-155,35,1570),y=Mathf.Clamp((1-screen.y)*1080-86,205,835);
                Rect rect=new Rect(x,y,310,90);
                for(int tries=0;tries<20;tries++)
                {
                    bool overlap=false;foreach(var other in placed)if(rect.Overlaps(other)){overlap=true;break;}if(!overlap)break;
                    rect.y-=96;if(rect.y<205){rect.y=835;rect.x=Mathf.Clamp(rect.x+320,35,1570);}
                }
                placed.Add(rect);RunHud.Fill(rect,new Color(.055f,.07f,.12f,.94f));
                GUI.Label(new Rect(rect.x+10,rect.y+4,290,22),a.Name,caption);
                for(int n=0;n<Mathf.Min(2,a.Rows.Count);n++)
                {
                    var d=a.Rows[n];text.normal.textColor=d.Neutral?ArcadeSkin.Gold:d.Positive?ArcadeSkin.Better:ArcadeSkin.Worse;
                    string label=d.Message??d.Label.Replace("Run / ","").Replace("Companion bullet interception","Interception")+" "+d.SignedChange;
                    GUI.Label(new Rect(rect.x+10,rect.y+27+n*26,290,27),label,text);
                }
            }
            text.normal.textColor=ArcadeSkin.Cream;
        }
    }
}
