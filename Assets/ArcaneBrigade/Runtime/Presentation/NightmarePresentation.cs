using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class NightmarePresentation : MonoBehaviour
    {
        private ArenaController owner;
        private NightmareAssets assets;
        private readonly SpriteRenderer[] landmarks = new SpriteRenderer[8];
        private readonly GroundShadow[] shadows = new GroundShadow[8];
        private readonly NightmarePropPuppet[] puppets = new NightmarePropPuppet[8];
        private readonly DreamSuspension[] suspensions = new DreamSuspension[8];
        private readonly MerchantDialogue narrator = new MerchantDialogue();
        private MerchantVoice voice;
        private int shownWave = -1;
        private int shownPage = -1;
        private int bossBeat = -1;
        private float pageAt;
        private float whisperAt;
        private string whisper;
        private GUIStyle copy;
        private GUIStyle button;
        private readonly NightmareDeparture departure = new NightmareDeparture();
        private NightmareFarewellRoom farewell;
        private int afterwordKnocks;
        public bool EndingVisible => owner != null && owner.Session.EndingPending && owner.TerminalDelayComplete;
        public int VisibleLandmarks { get; private set; }
        public int DeparturePartyCount => departure.PartyCount;
        public void Configure(ArenaController controller)
        {
            owner = controller; assets = NightmareAssets.Current; voice = gameObject.AddComponent<MerchantVoice>();
            gameObject.AddComponent<NightmareLighting>().Configure(controller);
            gameObject.AddComponent<NightmareAtmosphere>().Configure(controller);
            gameObject.AddComponent<NightmareHaunting>().Configure(controller);
            gameObject.AddComponent<NightmareGrandStage>().Configure(controller);
            farewell = gameObject.AddComponent<NightmareFarewellRoom>(); farewell.Configure(controller);
            for (int index = 0; index < landmarks.Length; index++)
            {
                var prop = new GameObject("DreamLandmark" + index); prop.transform.SetParent(transform,false);
                landmarks[index] = prop.AddComponent<SpriteRenderer>(); landmarks[index].sortingOrder = -35;
                shadows[index] = prop.AddComponent<GroundShadow>();
                shadows[index].PinCastToFoot = true;
                shadows[index].SortingBase = -45;
                puppets[index] = prop.AddComponent<NightmarePropPuppet>();
                suspensions[index] = prop.AddComponent<DreamSuspension>();
            }
        }
        private void Update()
        {
            if (owner == null || owner.Session == null || assets == null || owner.Simulation == null) return;
            if (owner.PresentationPaused) { voice.Silence(); return; }
            RunSnapshot state = owner.Session.State;
            float now = owner.PresentationClock;
            bool shop = state.Phase == SessionPhase.Shop && !owner.Session.SecretRoomOpen;
            int depth = NightmareRules.Depth(state.Wave);
            if (shownWave != state.Wave)
            {
                shownWave = state.Wave; whisperAt = now + 2.5f; whisper = null; bossBeat = -1;
            }
            if (shop && !NightmareHaunting.Quiet && !owner.TransitionPending && now >= whisperAt && now > owner.Shop.Dialogue.ExpiresAt + .5f && state.Nightmare.LastWhisperWave != state.Wave)
            {
                whisper = NightmareDialogue.Whisper(state,state.Nightmare.WhisperCursor++);
                state.Nightmare.LastWhisperWave = state.Wave; whisperAt = now;
                owner.SaveNightmareBoundary();
            }
            VisibleLandmarks = 0;
            Vector2 half = owner.WorldBounds * .5f;
            for (int index = 0; index < landmarks.Length; index++)
            {
                int art = NightmareRules.LandmarkArt(state,index);
                bool visible = !owner.Session.SecretRoomOpen && state.Phase != SessionPhase.Defeat && !(shop && index == 3) && (index < 5 || index == 5 && shop || index == 6 && state.Secrets?.RescuedToy == true || index == 7 && state.Secrets?.LetterSent == true);
                SpriteRenderer view = landmarks[index]; view.enabled = visible;
                if (!visible) { shadows[index].Hide(); puppets[index].Hide(); suspensions[index].Hide(); continue; }
                view.sprite = assets.Scenery[art];
                GroundedSceneryArtwork groundedArt = GroundedSceneryArtwork.Current;
                if ((art == 8 || art == 9) && groundedArt != null && groundedArt.Cabinets.Length == 2) view.sprite = groundedArt.Cabinets[art - 8];
                Vector2 position = NightmareLighting.LandmarkPosition(index, shop, half);
                if (shop) position = NightmareHaunting.UnsettledPosition(index, state.Seed, state.Wave, now, position, owner.Simulation.Champion.Position, owner.Simulation.Champion.Facing);
                float motion = 0;
                float width = index < 5 ? index == 0 ? 2.4f : 3.2f : .8f;
                view.transform.position = position;
                float scale = width / Mathf.Max(.01f,view.sprite.bounds.size.x);
                view.transform.localScale = new Vector3(scale * (1 + motion),scale * (1 - motion),1);
                Vector3 hovering = DreamLevitation.Offset(now, index + 1, depth, PresentationSettings.ReducedMotion);
                view.transform.localRotation = Quaternion.Euler(0,0,DreamLevitation.Tilt(now, index + 1, PresentationSettings.ReducedMotion));
                WorldSpriteGrounding.Place(view, (Vector3)position + hovering);
                view.sortingOrder = index == 2 || index == 4 ? -35 : WorldSpriteGrounding.SortAt(position.y);
                bool anchor = index == 1 || index >= 6;
                float fusion = owner.Shop?.Dialogue.LastMoment == MerchantMoment.Fuse && now < owner.Shop.Dialogue.ExpiresAt ? 1 : 0;
                Color tint = anchor ? Color.Lerp(new Color(1,.9f,.7f),Color.white,fusion) : Color.Lerp(Color.white,new Color(.64f,.59f,.73f),depth * .1f);
                tint.a = 1; view.color = tint;
                shadows[index].Configure(view.sprite); shadows[index].Display(view.sprite,position,width,hovering.y,.32f);
                suspensions[index].Display(position, (Vector3)position + hovering, width, now, index + 1, view.sortingOrder - 1, tint.a, anchor);
                if (shop && depth > 0 && !PresentationSettings.ReducedMotion && (index == 0 || index == 3))
                {
                    float beat = Mathf.Repeat(now + index * 3, 17);
                    float acting = beat < 1.5f ? Mathf.Sin(beat / 1.5f * Mathf.PI) : 0;
                    shadows[index].Cast.transform.position += Vector3.right * acting * depth * .035f;
                }
                puppets[index].Display(view, index, NightmareHaunting.Quiet ? 0 : now, depth, shop, owner.Simulation.Champion.Position);
                VisibleLandmarks++;
            }
            if (EndingVisible)
            {
                int page = state.Nightmare.EndingPage;
                if (owner.Session.EndingChoicePending)
                {
                    if (shownPage != -2) { shownPage = -2; narrator.MerchantName = "THE MAN"; narrator.Say("I remembered the price. I forgot the name.", now, 30); }
                    voice.Advance(narrator, true, now, false); return;
                }
                if (page != shownPage)
                {
                    shownPage = page; pageAt = now;
                    afterwordKnocks = 0;
                    narrator.MerchantName = page == 1 ? "THE SHOPKEEPER" : "THE WAKING SHOP";
                    narrator.Say(NightmareDialogue.Ending(state,page),now,20);
                    CheckpointAudio.PlayInterfaceId(page == 2 ? "voice.gold.farewell" : "ui.page");
                }
                voice.Advance(narrator,false,now,false);
                if (page == 4 && afterwordKnocks < 3 && now - pageAt >= .6f + afterwordKnocks * .55f)
                { CheckpointAudio.PlayInterfaceId("sig.stamp", .12f); afterwordKnocks++; }
                if (now - pageAt >= (page == 4 ? 2.6f : 1.2f) && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))) owner.AdvanceNightmareEnding();
            }
            else if (owner.Simulation.Boss?.Definition.Id == NightmareRules.CustomerId && owner.Simulation.Boss.Alive)
            {
                int beat = owner.Simulation.Boss.BossVolley / 2;
                if (beat != bossBeat && beat < 6)
                {
                    bossBeat = beat;
                    string[] lines = { "THE LAST CUSTOMER\nEverything on these shelves has a price. Even you.","THE SHOPKEEPER\nThey are guests. Put down the receipt.","THE LAST CUSTOMER\nAn unlisted friend is still missing inventory.","THE SHOPKEEPER\nNo. A promise is not a chain.","THE LAST CUSTOMER\nThen who will pay for the morning?", "YOUR BRIGADE\nNo one gets left on the shelf." };
                    whisper = lines[beat]; whisperAt = now; narrator.Say(whisper,now,5);
                }
                voice.Advance(narrator,true,now,false);
            }
        }
        public void Draw(Camera camera)
        {
            if (assets == null || owner.PresentationPaused && !EndingVisible) return;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(camera.pixelRect,1920,1080);
            if (copy == null) { copy = ArcadeTypography.Style(24); copy.alignment = TextAnchor.MiddleCenter; copy.wordWrap = true; button = ArcadeTypography.Style(25,true); button.alignment = TextAnchor.MiddleCenter; }
            if (EndingVisible)
            {
                if (owner.Session.EndingChoicePending)
                {
                    farewell.Draw(copy, button); GUI.matrix = previous; return;
                }
                RunHud.Fill(new Rect(0,0,1920,1080),new Color(.045f,.035f,.06f,1));
                int page = owner.Session.State.Nightmare.EndingPage;
                float pan = PresentationSettings.ReducedMotion ? 0 : Mathf.Clamp01((owner.PresentationClock - pageAt) / 12) * 8;
                Rect image = new Rect(450 - pan * 1.5f,30 - pan,1020 + pan * 3,680 + pan * 2);
                int illustration = Mathf.Min(page, 3);
                if (page == 4 && assets.PromiseIllustration != null) GUI.DrawTexture(image, assets.PromiseIllustration, ScaleMode.ScaleToFit);
                else GUI.DrawTextureWithTexCoords(image,assets.EndingIllustration,new Rect(illustration % 2 * .5f,1 - (illustration / 2 + 1) * .5f,.5f,.5f));
                departure.Draw(image, owner, page, owner.PresentationClock - pageAt);
                GUI.Label(new Rect(170,715,1580,220),NightmareDialogue.Ending(owner.Session.State,page),copy);
                GUI.enabled = owner.PresentationPaused || owner.PresentationClock - pageAt >= (page == 4 ? 2.6f : 1.2f);
                if (GUI.Button(new Rect(720,970,480,65),owner.PresentationPaused ? "RESUME" : page == 3 ? "KEEP THE MEMORY" : "CONTINUE",button))
                { if (owner.PresentationPaused) owner.TogglePause(); else owner.AdvanceNightmareEnding(); }
                GUI.enabled = true;
            }
            else if (!string.IsNullOrEmpty(whisper) && owner.PresentationClock - whisperAt < 5.5f && !owner.Session.SecretRoomOpen && (owner.Session.State.Phase == SessionPhase.Shop || owner.Simulation.Boss?.Definition.Id == NightmareRules.CustomerId && owner.Simulation.Boss.Alive))
            {
                float fade = Mathf.Clamp01((5.5f - owner.PresentationClock + whisperAt) * 2);
                Color color = GUI.color; GUI.color = new Color(1,1,1,fade);
                GUI.Label(new Rect(430,owner.Session.State.Phase == SessionPhase.Shop ? 115 : 790,1060,85),whisper,copy); GUI.color = color;
            }
            GUI.matrix = previous;
        }
    }
}
