using UnityEngine;

namespace ArcaneBrigade
{
    // The archive cabinet and meeting chair already own their floating animation and floor shadows.
    [DefaultExecutionOrder(-65)]
    public sealed class WaitingArchiveInteractions : MonoBehaviour, ShopFocus.ISource
    {
        public const float Reach=2;
        private ArenaController owner;
        private readonly SpriteRenderer[] props=new SpriteRenderer[2];
        private float nearSince=-1;
        public int InputConsumedFrame{get;private set;}=-1;
        public void Configure(ArenaController value){owner=value;ShopFocus.Register(this);}
        private void OnDestroy()=>ShopFocus.Unregister(this);
        public int SpotCount=>1;
        public bool Spot(int index,out Vector2 position,out float reach){position=Anchor(0);reach=Reach;return owner?.Simulation!=null && Prop(0)!=null && Prop(0).enabled;}
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
        public bool Near(int index)=>index==0 && Spot(0,out _,out _) && ShopFocus.Has(this,0,owner.Simulation.Champion.Position);
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
            if(!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.JoystickButton0))return;
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
            // walking up shows the shop's one card at the bottom, like every other thing in the shop (no words on the floor)
            bool near=Near(0);
            if(!near){nearSince=-1;GUI.matrix=previous;return;}
            if(nearSince<0)nearSince=Time.unscaledTime;
            if(ShopPrompt.Draw("THE JOURNAL WINDOW","Every page the night has written about you is kept behind the glass.","READ",Time.unscaledTime-nearSince)
               || TouchTargets.Button(ClickRect(0,camera),GUIContent.none,GUIStyle.none))TryInspect(0);
            GUI.matrix=previous;
        }
    }
}
