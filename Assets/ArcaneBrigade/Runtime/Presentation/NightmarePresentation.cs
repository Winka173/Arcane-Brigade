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
        // the dawn pages are read line by line (play-test 20/10/2026): each tap shows the next line, nothing needs scrolling
        private int endingLine = 1; private float lineAt;
        private GUIStyle endingTitle, endingPrevious, endingCurrent;
        private static string[] EndingLines(string page) => page.Split(new[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        /// <summary>Show the next line of the page; true once every line was already shown (the page may turn).</summary>
        private bool RevealNextLine()
        {
            int count = EndingLines(NightmareDialogue.Ending(owner.Session.State, owner.Session.State.Nightmare.EndingPage)).Length;
            if (endingLine >= count - 1) return true;
            if (owner.PresentationClock - lineAt < .2f) return false;
            endingLine++; lineAt = owner.PresentationClock; CheckpointAudio.PlayInterfaceId("ui.page", .05f);
            return false;
        }
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
        /// <summary>Who gets a spot first when the shop is crowded: the journal cabinet, then the lamp, the reserved place,
        /// the keepsakes, and last the landmark that would only stand behind the merchant.</summary>
        private static int LandmarkPriority(int index) => index == 2 ? 0 : index == 1 ? 1 : index == 5 ? 4 : index >= 6 ? 5 : index == 4 ? 9 : 6;
        public void Configure(ArenaController controller)
        {
            owner = controller; assets = NightmareAssets.Current; voice = gameObject.AddComponent<MerchantVoice>();
            gameObject.AddComponent<ShopSceneryLanding>().Configure(controller);
            gameObject.AddComponent<ShopLayout>().Configure(controller);
            gameObject.AddComponent<NightmareLighting>().Configure(controller);
            gameObject.AddComponent<SharedLightField>().Configure(controller);
            gameObject.AddComponent<NightLanternField>().Configure(controller);
            gameObject.AddComponent<MienChoPresentation>().Configure(controller);
            gameObject.AddComponent<WaitingRealmStage>().Configure(controller);
            gameObject.AddComponent<ActOneWorldArt>().Configure(controller);
            gameObject.AddComponent<CustomerOffer>().Configure(controller);
            gameObject.AddComponent<FerryDock>().Configure(controller);
            gameObject.AddComponent<VisitorCard>().Configure(controller);
            gameObject.AddComponent<LucidAltar>().Configure(controller);
            gameObject.AddComponent<MapArrival>().Configure(controller);
            gameObject.AddComponent<MemoryReturn>().Configure(controller);
            gameObject.AddComponent<WaitingShopEncounter>().Configure(controller);
            gameObject.AddComponent<WaitingJourney>().Configure(controller);
            gameObject.AddComponent<WaitingArchiveInteractions>().Configure(controller);
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
                // a voice without a body: a quiet caption at the left edge (the one who stays always whispers from the left)
                SpeechBubbles.Show("Whisper", null, whisper, null, SpeechTone.Whisper, 5.5f, 0);
            }
            VisibleLandmarks = 0;
            Vector2 half = owner.WorldBounds * .5f;
            for (int index = 0; index < landmarks.Length; index++)
            {
                int art = NightmareRules.LandmarkArt(state,index);
                bool visible = !owner.Session.SecretRoomOpen && state.Phase != SessionPhase.Defeat && (!shop || index == 2) /* the shop keeps only the journal window */ && !(shop && index == 3) && !(shop && index == 0 && !state.Endless && state.Wave <= ActOneCanon.Waves) && (index < 5 || index == 5 && shop || index == 6 && state.Secrets?.RescuedToy == true || index == 7 && state.Secrets?.LetterSent == true);
                SpriteRenderer view = landmarks[index]; view.enabled = visible;
                if (!visible) { shadows[index].Hide(); puppets[index].Hide(); suspensions[index].Hide(); continue; }
                view.sprite = assets.Scenery[art];
                GroundedSceneryArtwork groundedArt = GroundedSceneryArtwork.Current;
                if ((art == 8 || art == 9) && groundedArt != null && groundedArt.Cabinets.Length == 2) view.sprite = groundedArt.Cabinets[art - 8];
                Sprite redrawn = FinalQualityArtwork.Current?.Frame("landmark-" + art, now, PresentationSettings.ReducedMotion);
                if (redrawn != null) view.sprite = redrawn;
                Vector2 position = NightmareLighting.LandmarkPosition(index, shop, half);
                float motion = 0;
                float width = index < 5 ? index == 0 ? 2.4f : 3.2f : .8f;
                // combat: the arena layout's spot, clear of the chair, the Act I props, lanterns and architecture
                if (!shop)
                {
                    if (!ArenaLayout.Place(owner, "landmark" + index, ArenaLayout.Drawn(view.sprite, width / Mathf.Max(.01f, view.sprite.bounds.size.x)), ref position, out float placed))
                    { view.enabled = false; shadows[index].Hide(); puppets[index].Hide(); suspensions[index].Hide(); continue; }
                    width *= placed; // a story landmark comes in smaller rather than not at all
                }
                if (shop && ShopLayout.Active != null)
                {
                    // the shop is a small room: the big landmarks come in a little smaller and every one asks the layout
                    // for a spot clear of the stalls, the chair's path, the HUD and each other (or sits this shop out)
                    if (index == 1) width = 2.0f; else if (index == 2) width = 2.2f; else if (index == 4) width = 2.6f;
                    Vector2 inked = SpriteInkMetrics.BoundsOf(view.sprite).size * (width / Mathf.Max(.01f, view.sprite.bounds.size.x));
                    if (!ShopLayout.Active.Place("landmark" + index, position, inked, LandmarkPriority(index), index == 1 || index == 2, false, out position, out float fit))
                    { view.enabled = false; shadows[index].Hide(); puppets[index].Hide(); suspensions[index].Hide(); continue; }
                    width *= fit; // the journal cabinet and the lamp come in smaller rather than not at all
                }
                if (shop) position = NightmareHaunting.UnsettledPosition(index, state.Seed, state.Wave, now, position, owner.Simulation.Champion.Position, owner.Simulation.Champion.Facing);
                view.transform.position = position;
                float scale = width / Mathf.Max(.01f,view.sprite.bounds.size.x);
                view.transform.localScale = new Vector3(scale * (1 + motion),scale * (1 - motion),1);
                Vector3 hovering = DreamLevitation.Offset(now, index + 1, depth, PresentationSettings.ReducedMotion);
                float tilt=DreamLevitation.Tilt(now,index+1,PresentationSettings.ReducedMotion);
                if(index!=1 && ShopSceneryLanding.Active!=null){hovering=ShopSceneryLanding.Active.Apply(hovering,index+1);tilt=ShopSceneryLanding.Active.Tilt(tilt,index+1);}
                view.transform.localRotation = Quaternion.Euler(0,0,tilt);
                WorldSpriteGrounding.Place(view, (Vector3)position + hovering);
                view.sortingOrder = index == 2 || index == 4 ? -35 : WorldSpriteGrounding.SortAt(position.y);
                bool anchor = index == 1 || index >= 6;
                float fusion = owner.Shop?.Dialogue.LastMoment == MerchantMoment.Fuse && now < owner.Shop.Dialogue.ExpiresAt ? 1 : 0;
                Color tint = anchor ? Color.Lerp(new Color(1,.9f,.7f),Color.white,fusion) : Color.Lerp(Color.white,new Color(.64f,.59f,.73f),depth * .1f);
                if (SharedLightField.Active != null) { float light = SharedLightField.Active.SurfaceVisibility(position); tint.r *= light; tint.g *= light; tint.b *= light; }
                tint.a = 1; view.color = tint;
                shadows[index].Configure(view.sprite); shadows[index].Display(view.sprite,position,width,hovering.y,.32f);
                suspensions[index].Display(position, (Vector3)position + hovering, width, now, index + 1, view.sortingOrder - 1, tint.a, anchor);
                if (shop && depth > 0 && !PresentationSettings.ReducedMotion && (index == 0 || index == 3))
                {
                    float beat = Mathf.Repeat(now + index * 3, 17);
                    float acting = beat < 1.5f ? Mathf.Sin(beat / 1.5f * Mathf.PI) : 0;
                    shadows[index].Cast.transform.position += Vector3.right * acting * depth * .035f;
                }
                if (redrawn == null) puppets[index].Display(view, index, NightmareHaunting.Quiet ? 0 : now, depth, shop, owner.Simulation.Champion.Position);
                else { puppets[index].Hide(); view.enabled = true; }
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
                    shownPage = page; pageAt = now; endingLine = 1; lineAt = now;
                    afterwordKnocks = 0;
                    int waking = state.Nightmare.Farewell == NightmareFarewell.LeaveLight ? 4 : 3;
                    narrator.MerchantName = page == waking ? "THURSDAY" : page == 1 ? "THE SHOPKEEPER" : "DAWN";
                    narrator.Say(NightmareDialogue.Ending(state,page),now,20);
                    CheckpointAudio.PlayInterfaceId(page == 2 ? "voice.gold.farewell" : "ui.page");
                }
                voice.Advance(narrator,false,now,false);
                // three knocks on the waking page: somebody is at the door, on a Thursday
                bool last = page == (state.Nightmare.Farewell == NightmareFarewell.LeaveLight ? 4 : 3);
                if (last && afterwordKnocks < 3 && now - pageAt >= .6f + afterwordKnocks * .55f)
                { CheckpointAudio.PlayInterfaceId("sig.stamp", .12f); afterwordKnocks++; }
                if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0)) && RevealNextLine() && now - lineAt >= (last ? 2.6f : .6f)) owner.AdvanceNightmareEnding();
            }
            else if (owner.Simulation.Boss?.Definition.Id == NightmareRules.CustomerId && owner.Simulation.Boss.Alive)
            {
                int beat = owner.Simulation.Boss.BossVolley / 2;
                if (beat != bossBeat && beat < 6)
                {
                    bossBeat = beat;
                    if (!CustomerBeat(beat)) { string[] lines = { "Just say it once more. I will wait if you will stay.","They do not owe you a word. Put down the receipt.","They had kind eyes, didn't they? I can keep them for you.","Nobody here is for sale. Not even the ones who sat down.","Nobody stands up without paying.","Then we pull." }; narrator.Say(lines[beat],now,5); }
                    else narrator.Say("", now, 0);
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
                Rect image = new Rect(495 - pan * 1.5f,56 - pan,930 + pan * 3,620 + pan * 2); // padded from the top edge, a clear gap above the title
                int illustration = Mathf.Min(page, 3);
                if (page == 2 && owner.GetComponent<MienChoPresentation>() != null) owner.GetComponent<MienChoPresentation>().DrawDawn(image);
                else if (page >= 3 && !(page == 3 && owner.Session.State.Nightmare.Farewell == NightmareFarewell.LeaveLight) && ActOneArtwork.Picture("Panel_Thursday") != null) GUI.DrawTexture(image, ActOneArtwork.Picture("Panel_Thursday").texture, ScaleMode.ScaleToFit);
                else if (page == 4 && assets.PromiseIllustration != null) GUI.DrawTexture(image, assets.PromiseIllustration, ScaleMode.ScaleToFit);
                else GUI.DrawTextureWithTexCoords(image,assets.EndingIllustration,new Rect(illustration % 2 * .5f,1 - (illustration / 2 + 1) * .5f,.5f,.5f));
                departure.Draw(image, owner, page, owner.PresentationClock - pageAt);
                // the page's title under the picture, then its lines one tap at a time: the last line read stays, dimmed
                if (endingTitle == null)
                {
                    endingTitle = ArcadeTypography.Style(30, true); endingTitle.alignment = TextAnchor.MiddleCenter; endingTitle.normal.textColor = ArcadeSkin.Gold;
                    endingPrevious = new GUIStyle(copy); endingPrevious.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, .55f);
                    endingCurrent = new GUIStyle(copy) { fontSize = 28 }; endingCurrent.normal.textColor = ArcadeSkin.Cream;
                }
                string[] lines = EndingLines(NightmareDialogue.Ending(owner.Session.State,page));
                int line = Mathf.Clamp(endingLine, 1, Mathf.Max(1, lines.Length - 1));
                bool more = line < lines.Length - 1;
                if (lines.Length > 0) GUI.Label(new Rect(320,700,1280,44),lines[0],endingTitle);
                if (line >= 2) GUI.Label(new Rect(320,752,1280,60),lines[line - 1],endingPrevious);
                float fresh = PresentationSettings.ReducedMotion ? 1 : Mathf.Clamp01((owner.PresentationClock - lineAt) / .35f);
                Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * fresh);
                if (lines.Length > 1) GUI.Label(new Rect(320,816,1280,140),lines[line],endingCurrent);
                GUI.color = keep;
                Rect next = new Rect(720,970,480,80);
                // a tap anywhere on the page shows the next line (the button too)
                if (!owner.PresentationPaused && more && Event.current.type == EventType.MouseDown && !next.Contains(Event.current.mousePosition)) { RevealNextLine(); Event.current.Use(); }
                GUI.enabled = owner.PresentationPaused || more || owner.PresentationClock - lineAt >= (page == 4 ? 2.6f : .6f);
                if (ArcadeSkin.Button(next,owner.PresentationPaused ? "RESUME" : more ? "NEXT" : page == 3 ? "KEEP THE MEMORY" : "CONTINUE",KitButton.Primary,GUI.enabled,button))
                { if (owner.PresentationPaused) owner.TogglePause(); else if (RevealNextLine()) owner.AdvanceNightmareEnding(); }
                GUI.enabled = true;
            }
            // speech boxes pinned to whoever speaks (the shop draws them first, under its tooltips; this covers the fight)
            // never over a card or menu (requirement 22/10/2026: a friend's chatter sat on the memory choice)
            else if (!owner.Session.SecretRoomOpen && !PlayerGuide.ModalOpen(owner)) SpeechBubbles.Draw(camera, owner.Session.State.Phase == SessionPhase.Combat);
            GUI.matrix = previous;
        }

        /// <summary>The Last Customer's fight is a conversation: each beat is said by its own speaker, in a box above them,
        /// and voiced when the clip exists. Returns true when a clip carries the line (no murmured syllables then).</summary>
        private bool CustomerBeat(int beat)
        {
            UnitState customer = owner.Simulation.Boss;
            SpeechBubbleCast cast = owner.GetComponent<SpeechBubbleCast>();
            VoiceLine line;
            switch (beat)
            {
                case 0: case 2: case 4:
                    line = VoiceDirector.Exact(VoiceLines.LastCustomer, "story", beat / 2);
                    if (line == null || cast == null) return false;
                    bool spoken = VoiceDirector.Active != null && VoiceDirector.Active.Play(line, VoiceDirector.Story);
                    cast.ShowBoss(VoiceLines.LastCustomer, customer, line.Text, VoiceDirector.Story, 5f);
                    return spoken;
                case 1:
                    line = VoiceDirector.Exact(VoiceLines.Shopkeeper, "story", 4);
                    SpeechBubbleCast.Offscreen(VoiceLines.Shopkeeper, "THE SHOPKEEPER", line);
                    return VoiceDirector.Speaking(VoiceLines.Shopkeeper);
                case 3:
                {
                    // the brigade answers him (Hob, who said it before, no longer travels with you)
                    UnitState hero = owner.Simulation.Champion;
                    SpeechBubbles.Show("Brigade", "YOUR BRIGADE", "Nobody here is for sale. Not even the ones who sat down.", () => hero == null || !hero.Alive ? (Vector3?)null : SpeechBubbles.Head(hero), SpeechTone.Hero, 4f, 4);
                    return false;
                }
                default:
                    UnitState champion = owner.Simulation.Champion;
                    SpeechBubbles.Show("Brigade", "YOUR BRIGADE", "Then we pull.", () => champion == null || !champion.Alive ? (Vector3?)null : SpeechBubbles.Head(champion), SpeechTone.Hero, 4f, 4);
                    return false;
            }
        }
    }
}
