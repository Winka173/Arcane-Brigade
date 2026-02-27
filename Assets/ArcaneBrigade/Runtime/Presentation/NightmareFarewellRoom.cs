using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class NightmareFarewellRoom : MonoBehaviour
    {
        private ArenaController owner;
        private readonly SpriteRenderer[] views = new SpriteRenderer[4];
        private readonly GroundShadow[] shadows = new GroundShadow[4];
        private readonly DreamSuspension[] suspensions=new DreamSuspension[4];
        private readonly Vector2[] positions = new Vector2[4];
        private float appeared;
        private int focus = -1;
        private bool visible;
        public int Focus => focus;
        public void Configure(ArenaController controller)
        {
            owner = controller;
            for (int index = 0; index < views.Length; index++)
            {
                var child = new GameObject("FarewellWitness" + index); child.transform.SetParent(transform, false);
                views[index] = child.AddComponent<SpriteRenderer>(); views[index].sortingOrder = 650;
                views[index].sprite = index == 0 ? NightmareAssets.Current.Scenery[7] : index == 1 ? NightmareAssets.Current.Scenery[3] : ActOneArtwork.Picture(index == 2 ? "LastCustomer_Blank" : "Prop_BurntContract");
                shadows[index] = child.AddComponent<GroundShadow>();
                if(index!=2)suspensions[index]=child.AddComponent<DreamSuspension>();
            }
        }
        private void Update()
        {
            if (owner == null) return;
            bool show = owner.TerminalDelayComplete && owner.Session.EndingChoicePending;
            if (show && !visible)
            {
                appeared = owner.PresentationClock;
                Vector2 origin = owner.Simulation.ClampPosition(owner.Simulation.Champion.Position, 4);
                positions[0] = origin + new Vector2(-2.4f, 1); positions[1] = origin + new Vector2(2.4f, 1); positions[2] = origin + new Vector2(0, 1.9f);
                positions[3] = origin + new Vector2(.55f,.25f);
            }
            visible = show;
            if (!show) { foreach (SpriteRenderer view in views) view.enabled = false; foreach (GroundShadow shadow in shadows) shadow.Hide();foreach(var air in suspensions)air?.Hide();return; }
            if (owner.PresentationPaused) return;
            bool stationary = owner.Simulation.Champion.Definition.HeroMechanic == HeroMechanic.RootedOracle;
            float horizontal = Input.GetAxisRaw("Horizontal");
            focus = -1;
            if (stationary) focus = horizontal > .2f ? 1 : horizontal < -.2f ? 0 : selected;
            else
                for (int index = 0; index < 2; index++)
                    if (Vector2.Distance(owner.Simulation.Champion.Position, positions[index]) < 1.45f) focus = index;
            if (focus >= 0) selected = focus;
            float age = owner.PresentationClock - appeared;
            for (int index = 0; index < views.Length; index++)
            {
                SpriteRenderer view = views[index]; view.enabled = true;
                if (view.sprite == null) { view.enabled = false; continue; }
                float enter = PresentationSettings.ReducedMotion ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 1.1f));
                float lift = PresentationSettings.ReducedMotion ? 0 : (1 - enter) * .35f;
                float breathe = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(age * 1.2f + index) * .012f;
                float width = index == 2 ? 1.05f : index == 3 ? .75f : index == 0 ? 1.8f : 2;
                float scale = width * (1 + breathe + (focus == index ? .055f : 0)) / Mathf.Max(.01f, view.sprite.bounds.size.x);
                view.transform.localScale = Vector3.one * scale;
                Vector3 hovering = index == 2 ? Vector3.zero : DreamLevitation.Offset(owner.PresentationClock, 90 + index, 0, PresentationSettings.ReducedMotion);
                view.transform.rotation = Quaternion.Euler(0, 0, index == 2 ? 0 : DreamLevitation.Tilt(owner.PresentationClock, 90 + index, PresentationSettings.ReducedMotion));
                WorldSpriteGrounding.Place(view, (Vector3)positions[index] + Vector3.up * lift + hovering);
                view.color = new Color(1, 1, 1, enter);
                shadows[index].Display(view.sprite, positions[index], width, lift + hovering.y, .25f * enter);
                if(index!=2)suspensions[index].Display(positions[index],(Vector3)positions[index]+Vector3.up*lift+hovering,width,owner.PresentationClock,90+index,view.sortingOrder-1,enter,true);
            }
            if (age >= 1.2f && focus >= 0 && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))) Select(focus);
        }
        private int selected;
        private void Select(int index)
        {
            if (owner.PresentationPaused || owner.PresentationClock - appeared < 1.2f) return;
            owner.ChooseNightmareEnding(index == 0 ? NightmareFarewell.CloseShop : NightmareFarewell.LeaveLight);
        }
        public void Draw(GUIStyle text, GUIStyle button)
        {
            GUI.Label(new Rect(400, 85, 1120, 90), "THE CONTRACT HAS BURNED\nOne lantern still holds a name that was never his to buy.", text);
            GUI.Label(new Rect(470, 765, 980, 70), "The door is real. Both choices lead to morning.", text);
            bool previous = GUI.enabled;
            GUI.enabled = previous && !owner.PresentationPaused && owner.PresentationClock - appeared >= 1.2f;
            if (GUI.Button(new Rect(380, 855, 540, 74), "CLOSE THE SHOP", button)) Select(0);
            if (GUI.Button(new Rect(1000, 855, 540, 74), "LEAVE ONE LIGHT ON", button)) Select(1);
            GUI.enabled = previous;
            string leave = "Leave together.";
            GUI.Label(new Rect(380, 940, 540, 75), leave + " Goodbye is not forgetting.", text);
            GUI.Label(new Rect(1000, 940, 540, 75), leave + " A guiding lamp. No debt to return.", text);
            GUI.Label(new Rect(380, 1020, 1160, 45), "Move / choose left or right · Space / A · or click either choice", text);
            if (owner.PresentationPaused && GUI.Button(new Rect(760, 650, 400, 60), "RESUME", button)) owner.TogglePause();
        }
    }
}
