using UnityEngine;

namespace ArcaneBrigade
{
    // The archive cabinet and meeting chair already own their floating animation and floor shadows.
    [DefaultExecutionOrder(-65)]
    public sealed class WaitingArchiveInteractions : MonoBehaviour
    {
        public const float Reach=2;
        private ArenaController owner;
        private readonly SpriteRenderer[] props=new SpriteRenderer[2];
        private GUIStyle caption;
        public int InputConsumedFrame{get;private set;}=-1;
        public void Configure(ArenaController value)=>owner=value;
        private bool Allowed=>owner?.Session!=null && owner.Simulation?.Phase==RunPhase.Rest && owner.Session.State.Phase==SessionPhase.Shop && !owner.StoryConsumesInput && !owner.PresentationPaused && !owner.Details.Open && !owner.MenuConfirmation && !owner.TransitionPending && !owner.HallExitPending && !owner.Session.SecretRoomOpen && !owner.Shop.Animating && !owner.NightmareEndingVisible;
        public SpriteRenderer Prop(int index)
        {
            if(props[index]==null)props[index]=owner.transform.Find(index==0?"DreamLandmark2":"DreamLandmark0")?.GetComponent<SpriteRenderer>();
            return props[index];
        }
        public Vector2 Anchor(int index)
        {
            var view=Prop(index);var shadow=view==null?null:view.GetComponent<GroundShadow>();
            return shadow?.Contact!=null?(Vector2)shadow.Contact.transform.position:view==null?Vector2.zero:new Vector2(view.bounds.center.x,view.bounds.min.y);
        }
        // Act I canon: only the journal cabinet is a story object; the old "paths & people" chair is hidden in the Act I shop.
        public bool Near(int index)=>index==0 && Prop(index)!=null && Prop(index).enabled && Vector2.Distance(owner.Simulation.Champion.Position,Anchor(index))<=Reach;
        public bool TryInspect(int index)
        {
            if(!Allowed || !Near(index))return false;
            if(index==0)owner.GetComponent<MienChoPresentation>().OpenNext(); // the oldest unread page first, else the newest chapter
            else owner.GetComponent<WaitingJourney>().Show();
            bool opened=index==0?owner.GetComponent<MienChoPresentation>().Open:owner.GetComponent<WaitingJourney>().Open;
            if(opened)InputConsumedFrame=Time.frameCount;
            return opened;
        }
        private void Update()
        {
            if(!Allowed || !Application.isFocused && !RunPaths.Diagnostic)return;
            if(!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.JoystickButton0))return;
            int nearest=-1;float distance=float.PositiveInfinity;
            for(int i=0;i<2;i++)if(Near(i)){float d=Vector2.Distance(owner.Simulation.Champion.Position,Anchor(i));if(d<distance){distance=d;nearest=i;}}
            if(nearest>=0)TryInspect(nearest);
        }
        public Rect ClickRect(int index,Camera camera)
        {
            var bounds=Prop(index).bounds;Vector3 min=camera.WorldToViewportPoint(bounds.min),max=camera.WorldToViewportPoint(bounds.max);
            return Rect.MinMaxRect(min.x*1920-18,(1-max.y)*1080-18,max.x*1920+18,(1-min.y)*1080+18);
        }
        private void OnGUI()
        {
            if(!Allowed)return;var camera=Camera.main;if(camera==null)return;
            Matrix4x4 previous=ArcadeTypography.BeginViewport(camera.pixelRect,1920,1080);
            caption??=ArcadeTypography.Style(19,true);caption.alignment=TextAnchor.MiddleCenter;
            for(int i=0;i<2;i++)
            {
                if(!Near(i))continue;
                Rect bounds=ClickRect(i,camera);
                string label=i==0?"READ THE JOURNAL":"PATHS & PEOPLE";
                Rect text=new Rect(Mathf.Clamp(bounds.center.x-150,20,1600),Mathf.Clamp(bounds.yMax+8,185,940),300,48);
                caption.normal.textColor=new Color(.01f,.015f,.025f,.9f);GUI.Label(new Rect(text.x+1,text.y+2,text.width,text.height),label+"\nClick / Space / A",caption);
                caption.normal.textColor=ArcadeSkin.Cream;GUI.Label(text,label+"\nClick / Space / A",caption);
                if(GUI.Button(bounds,GUIContent.none,GUIStyle.none)){TryInspect(i);break;}
            }
            GUI.matrix=previous;
        }
    }
}
