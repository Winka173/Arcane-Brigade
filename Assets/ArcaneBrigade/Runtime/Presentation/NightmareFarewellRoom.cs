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
            if (age >= 1.2f && (focus >= 0 || picked >= 0) && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))) Select(focus >= 0 ? focus : picked);
        }
        private int selected;
        private void Select(int index)
        {
            if (index < 0) return;
            if (owner.PresentationPaused || owner.PresentationClock - appeared < 1.2f) return;
            owner.ChooseNightmareEnding(index == 0 ? NightmareFarewell.CloseShop : NightmareFarewell.LeaveLight);
        }
        private GUIStyle head, body, choiceTitle, choiceBody;
        private int picked = -1;
        private static readonly string[] Titles = { "CLOSE THE SHOP", "LEAVE ONE LIGHT ON" };
        private static readonly string[] Meanings =
        {
            "You all leave together and the shop closes for good. Goodbye is not forgetting.",
            "You all leave together, and one lamp stays lit to guide whoever comes next. One more page of the story."
        };
        /// <summary>The farewell (owner 22/10/2026): the words on paper so they read on any floor, the two choices as cards that
        /// say what each means, the one looked at or walked to lit, and CONFIRM to take it. No key hints.</summary>
        public void Draw(GUIStyle text, GUIStyle button)
        {
            if (head == null)
            {
                head = ArcadeTypography.Style(30, true); head.alignment = TextAnchor.MiddleCenter; head.wordWrap = true;
                body = ArcadeTypography.Style(24); body.font = ArcadeTypography.Hand; body.alignment = TextAnchor.MiddleCenter; body.wordWrap = true;
                choiceTitle = ArcadeTypography.Style(30, true); choiceTitle.alignment = TextAnchor.MiddleCenter; choiceTitle.wordWrap = false;
                choiceBody = ArcadeTypography.Style(22); choiceBody.alignment = TextAnchor.UpperCenter; choiceBody.wordWrap = true;
            }
            Color ink = new Color(.22f, .16f, .1f);
            Rect top = new Rect(510, 60, 900, 170);
            ArcadeSkin.Ticket(top, ArcadeSkin.Paper, ArcadeSkin.Wax);
            Rect topIn = ArcadeSkin.Content(top, KitFrame.Card);
            head.normal.textColor = ink; body.normal.textColor = new Color(.3f, .22f, .15f);
            GUI.Label(new Rect(topIn.x, topIn.y, topIn.width, 44), "THE CONTRACT HAS BURNED", head);
            GUI.Label(new Rect(topIn.x, topIn.y + 46, topIn.width, topIn.height - 46), "One lantern still holds a name that was never his to buy. The door is real: both choices lead to morning.", body);

            if (focus >= 0) picked = focus; // walking up to a witness picks its card
            bool ready = !owner.PresentationPaused && owner.PresentationClock - appeared >= 1.2f;
            for (int i = 0; i < 2; i++)
            {
                Rect card = new Rect(i == 0 ? 300 : 1020, 720, 600, 230);
                if (picked == i) { ArcadeSkin.Glow(new Rect(card.x - 26, card.y - 22, card.width + 52, card.height + 44), new Color(1f, .84f, .45f, .55f)); }
                ArcadeSkin.Ticket(card, ArcadeSkin.Paper, picked == i ? ArcadeSkin.Gold : ArcadeSkin.Accent);
                if (picked == i) ArcadeSkin.WorldFocus(card);
                Rect inner = ArcadeSkin.Content(card, KitFrame.Card);
                choiceTitle.normal.textColor = picked == i ? ArcadeSkin.Wax : ink;
                GUI.Label(new Rect(inner.x, inner.y, inner.width, 44), Titles[i], choiceTitle);
                choiceBody.normal.textColor = ink;
                GUI.Label(new Rect(inner.x, inner.y + 52, inner.width, inner.height - 52), Meanings[i], choiceBody);
                if (ready && GUI.Button(card, GUIContent.none, GUIStyle.none)) { picked = i; CheckpointAudio.PlayInterfaceId("ui.select"); }
            }
            bool can = ready && picked >= 0;
            if (ArcadeSkin.Button(new Rect(760, 968, 400, 88), can ? "CONFIRM" : "CHOOSE ONE", KitButton.Primary, can, button)) Select(picked);
            if (owner.PresentationPaused && ArcadeSkin.Button(new Rect(760, 650, 400, 84), "RESUME", KitButton.Primary, true, button)) owner.TogglePause();
        }
    }
}
