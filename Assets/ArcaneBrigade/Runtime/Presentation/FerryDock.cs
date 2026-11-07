using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The ferry at the edge of the shop (requirement 31/10/2026): walk up to it and pick a friend, then SEE THEM HOME
    /// (half its cost back, a letter worth Lamp Oil, its place for a new friend back before wave 20) or ROW ACROSS (eggs: it
    /// is traded for one of three new friends at the same stars). Each once a shop. Going home ends in a doorway of light.</summary>
    [DefaultExecutionOrder(-55)]
    public sealed class FerryDock : MonoBehaviour, ShopFocus.ISource
    {
        public static readonly Vector2 Anchor = new Vector2(7.35f, -4.05f); // the room's bottom right corner (requirement 1/11/2026), clear of the card and CONTINUE; an interactable, so it is placed before decoration
        public const float Reach = 1.65f, Width = 1.8f;
        private const string LayoutId = ShopLayout.FerryId, Key = "ferry-card";
        /// <summary>The ferry's card is up: the shop leaves the traveller's presses to it.</summary>
        public static bool Showing { get; private set; }
        public int InputConsumedFrame { get; private set; } = -1;
        private ArenaController owner;
        private SpriteRenderer prop, glow, leaving;
        private GroundShadow shadow;
        private Sprite homeIcon, ferryIcon, letterIcon;
        private GUIStyle hand, plain, label, names;
        private Vector2 placedAnchor;
        private bool registered, open;
        private readonly List<string> species = new List<string>();
        private int page;
        private string picked, crossing; // the friend chosen, and (rowing) the one chosen on the far bank
        private bool rowing;
        private string choicesFor; private List<string> choices; // the far bank, worked out once per friend picked
        private float farewellAt = -10; private Sprite farewellBody;
        private float promptAt; private bool promptShown;

        public bool Visible => prop != null && prop.enabled;
        public bool Near { get; private set; }
        public bool Open => open && Allowed;
        public int SpotCount => Visible && Allowed ? 1 : 0;
        public bool Spot(int index, out Vector2 position, out float reach) { position = placedAnchor; reach = Reach; return index == 0 && Visible && Allowed; }
        private bool Allowed => owner?.Session != null && owner.Simulation != null && owner.Session.State.Phase == SessionPhase.Shop && !owner.Session.SecretRoomOpen
            && owner.Session.State.OutsideStage != OutsideStageStatus.Reward && owner.Session.State.Wave >= 2 && !owner.TransitionPending && !owner.HallExitPending && !owner.NightmareEndingVisible;
        private bool Usable => owner.Session.SeeHomeOpen || owner.Session.FerryOpen;

        public void Configure(ArenaController controller)
        {
            owner = controller; ShopFocus.Register(this);
            prop = Make("Ferry"); glow = Make("Home light"); leaving = Make("Going home");
            prop.sprite = Resources.Load<Sprite>("Shop/FerryStation");
            glow.sprite = Resources.Load<Sprite>("Shop/HomeLight");
            homeIcon = Resources.Load<Sprite>("UI/Items/SeeHomeIcon"); ferryIcon = Resources.Load<Sprite>("UI/Items/FerryIcon"); letterIcon = Resources.Load<Sprite>("UI/Items/FarewellLetter");
            shadow = prop.gameObject.AddComponent<GroundShadow>(); shadow.PinCastToFoot = true;
        }
        private SpriteRenderer Make(string label)
        {
            var go = new GameObject(label); go.transform.SetParent(transform, false);
            var view = go.AddComponent<SpriteRenderer>(); view.enabled = false; return view;
        }
        private void Hide()
        {
            if (prop != null) prop.enabled = glow.enabled = leaving.enabled = false;
            shadow?.Hide(); Near = false; open = false; Showing = false;
            if (registered) { ShopLayout.Active?.Release(LayoutId); registered = false; }
        }

        private void LateUpdate()
        {
            if (!Allowed || prop.sprite == null || owner.Session.PartingSpecies().Count == 0 && Time.unscaledTime - farewellAt > 2) { Hide(); return; }
            float width = Width, fit = 1;
            placedAnchor = Anchor;
            if (ShopLayout.Active != null)
            {
                registered = true;
                // no room this frame: the prop waits unseen, keeping its claim and any open card (releasing made it blink)
                if (!ShopLayout.Active.Place(LayoutId, Anchor, ShopLayout.SizeAtWidth(prop.sprite, width), 6, true, false, out placedAnchor, out fit)) { prop.enabled = false; shadow.Hide(); Near = false; Showing = Open; return; }
                width *= fit;
            }
            prop.enabled = true;
            prop.transform.localScale = Vector3.one * (width / Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(prop.sprite).width));
            WorldSpriteGrounding.Place(prop, placedAnchor);
            prop.sortingOrder = WorldSpriteGrounding.SortAt(placedAnchor.y);
            float lit = Mathf.Max(.45f, SharedLightField.Active?.SurfaceVisibility(placedAnchor) ?? 1);
            prop.color = Usable ? new Color(lit, lit, lit) : new Color(lit * .6f, lit * .6f, lit * .65f); // used up for this shop: the lantern is out
            shadow.SortingBase = prop.sortingOrder - 10; // its contact (+7) stays under the prop's feet
            shadow.Display(prop.sprite, placedAnchor, width * .8f, 0, .25f);
            Farewell(width);
            Near = !open && ShopFocus.Has(this, 0, owner.Simulation.Champion.Position) && owner.Shop?.Nearby == null && owner.Shop?.Animating != true;
            Showing = Open;
            if (owner.PresentationPaused || owner.StoryConsumesInput) return;
            if (open && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))) { Close(); return; }
            if (Near && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton0))) Board();
        }

        /// <summary>The friend walks into a doorway of light above the jetty and is gone.</summary>
        private void Farewell(float width)
        {
            float t = (Time.unscaledTime - farewellAt) / 1.8f;
            glow.enabled = leaving.enabled = t >= 0 && t < 1 && glow.sprite != null;
            if (!glow.enabled) return;
            Vector2 door = placedAnchor + new Vector2(width * .18f, width * .55f);
            float open01 = Mathf.Clamp01(t * 3), fade = 1 - Mathf.Clamp01((t - .7f) / .3f);
            glow.transform.position = door; glow.transform.localScale = Vector3.one * (1.8f / Mathf.Max(.01f, glow.sprite.bounds.size.y)) * Mathf.Lerp(.3f, 1, open01);
            glow.color = new Color(1, .93f, .7f, fade); glow.sortingOrder = prop.sortingOrder + 20;
            leaving.sprite = farewellBody; leaving.enabled = farewellBody != null;
            if (leaving.enabled)
            {
                float walk = Mathf.Clamp01((t - .15f) / .6f);
                leaving.transform.position = Vector2.Lerp(placedAnchor + Vector2.right * width * .6f, door + Vector2.down * .5f, walk);
                leaving.transform.localScale = Vector3.one * (1.1f * (1 - walk * .6f) / Mathf.Max(.01f, farewellBody.bounds.size.y));
                leaving.color = new Color(1, 1, 1, (1 - walk) * fade); leaving.sortingOrder = glow.sortingOrder + 1;
            }
        }

        private void Board()
        {
            if (!Usable) return;
            open = true; rowing = false; crossing = null; picked = null; page = 0; Showing = true;
            InputConsumedFrame = Time.frameCount; CheckpointAudio.PlayInterfaceId("ui.select");
        }
        /// <summary>Opens the card with a friend picked (captures).</summary>
        public void ShowForCapture(string id) { Board(); picked = id; }
        private void Close() { open = false; Showing = false; InputConsumedFrame = Time.frameCount; CheckpointAudio.PlayInterfaceId("ui.back"); }

        private void SendHome()
        {
            string id = picked;
            Sprite body = owner.Content.Unit(id)?.Body;
            InputConsumedFrame = Time.frameCount;
            if (!owner.SeeHome(id)) return;
            farewellBody = body; farewellAt = Time.unscaledTime;
            open = false; Showing = false; picked = null;
        }
        private void Row()
        {
            InputConsumedFrame = Time.frameCount;
            if (!owner.Ferry(picked, crossing)) return;
            open = false; Showing = false; picked = crossing = null; rowing = false;
        }

        private void OnGUI()
        {
            if (!Visible && !Open || owner.PresentationPaused || owner.StoryConsumesInput || Camera.main == null) { promptShown = false; return; }
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            // no name sign over the ferry (requirement 1/11/2026): the boat itself and its prompt card say what it is
            if (!open)
            {
                if (Near)
                {
                    if (TouchTargets.Button(StoryPropInput.ClickRect(prop, Camera.main), GUIContent.none, GUIStyle.none)) Board();
                    if (!promptShown) { promptShown = true; promptAt = Time.unscaledTime; }
                    string plain = Usable ? "Send a friend home, or row one across for a new friend at the same stars." : "The ferry has gone out for tonight. It is back in the next shop.";
                    if (ShopPrompt.Draw("THE FERRY", plain, Usable ? "BOARD" : "TOMORROW", Time.unscaledTime - promptAt)) Board();
                }
                else promptShown = false;
                GUI.matrix = previous; return;
            }
            DrawCard();
            GUI.matrix = previous;
        }


        private void DrawCard()
        {
            GUI.depth = -60;
            if (hand == null)
            {
                hand = ArcadeTypography.Style(30); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleCenter; hand.wordWrap = true;
                plain = ArcadeTypography.Style(ArcadeTypography.SizeNote); plain.alignment = TextAnchor.UpperCenter; plain.wordWrap = true;
                label = ArcadeTypography.Style(ArcadeTypography.SizeNote, true); label.alignment = TextAnchor.MiddleCenter; label.wordWrap = true;
                names = new GUIStyle(label) { alignment = TextAnchor.UpperCenter, fontSize = 22 }; // a portrait's caption: up to three short lines
            }
            RunSession session = owner.Session;
            species.Clear(); species.AddRange(session.PartingSpecies());
            if (picked != null && !species.Contains(picked)) { picked = null; rowing = false; }
            RunHud.Fill(new Rect(-1920, -1080, 5760, 3240), new Color(0, 0, 0, .45f));
            if (rowing && choicesFor != picked + session.State.Wave) { choicesFor = picked + session.State.Wave; choices = session.FerryChoices(picked); }
            List<string> shownIds = rowing ? choices : species;
            int pages = Mathf.Max(1, (shownIds.Count + 8) / 9); page = rowing ? 0 : Mathf.Clamp(page, 0, pages - 1);
            int first = page * 9, shown = Mathf.Min(9, shownIds.Count - first);
            float width = Mathf.Max(980, 140 + shown * 166); // a clear gap between the round frames
            Rect card = new Rect((1920 - width) * .5f, 160, width, 860);
            Color keep = CardPop.Apply(Key, card);
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Accent);
            Sprite corner = rowing ? ferryIcon : homeIcon;
            if (corner != null) RunHud.Sprite(new Rect(card.xMax - 104, card.y + 16, 80, 80), corner);
            Color ink = new Color(.18f, .14f, .1f);
            hand.normal.textColor = new Color(.25f, .19f, .14f);
            LocalizedGUI.Label(new Rect(card.x + 60, card.y + 24, card.width - 180, 100), rowing
                ? "THE FERRYMAN: 'Three are waiting on the far bank. Who comes back with me?'"
                : "THE FERRYMAN: 'Someone going home tonight? Or someone crossing over?'", hand);
            plain.normal.textColor = ink;
            LocalizedGUI.Label(new Rect(card.x + 60, card.y + 128, card.width - 120, 76), rowing
                ? RunUICopy.UnitName(owner.Content.Unit(picked)) + " crosses, and the one you choose comes back at the same stars."
                : "Pick a friend first.", plain);
            for (int i = 0; i < shown; i++)
            {
                string id = shownIds[first + i];
                UnitDefinition unit = owner.Content.Unit(id);
                // the portrait in its round frame, the name (and stars) in ink on the paper below it
                Rect slot = new Rect(card.x + (card.width - shown * 166) * .5f + i * 166 + 14, card.y + 222, 138, 232), frame = new Rect(slot.x + 10, slot.y, 118, 118);
                bool chosen = rowing ? crossing == id : picked == id;
                Matrix4x4 unscaled = GUI.matrix;
                // the picked one keeps its size and place (the chosen seal marks it)
                ArcadeSkin.Plate(frame, ArcadeSkin.Card);
                if (unit != null) RunHud.SpriteInk(new Rect(frame.x + 14, frame.y + 12, 90, 92), unit.Body); // the friend fills its medallion (its painted part, not its empty cell)
                GUI.matrix = unscaled;
                if (chosen) ArcadeSkin.PickedSeal(frame); // the picked friend's medallion is stamped
                names.normal.textColor = chosen ? new Color(.62f, .27f, .05f) : ink; // and its name in warm brown
                // its stars as small gold stars on the medallion's rim, so the name keeps to its own lines (review 3/11/2026: "1★"
                // dropped onto a line of its own)
                if (!rowing)
                {
                    float progress = session.StarProgress(id); int whole = Mathf.Clamp(Mathf.FloorToInt(progress), 1, 5); bool half = progress - whole > .01f;
                    int count = whole + (half ? 1 : 0); float pip = 20, sx = frame.center.x - count * pip * .5f;
                    Color keepTint = GUI.color;
                    for (int k = 0; k < count; k++)
                    {
                        GUI.color = new Color(1, 1, 1, half && k == count - 1 ? .45f : 1) * UnitVisual.StarColour(whole);
                        RunHud.Sprite(new Rect(sx + k * pip, frame.yMax - pip * .7f, pip, pip), K8Motion.TierStar());
                    }
                    GUI.color = keepTint;
                }
                LocalizedGUI.Label(new Rect(slot.x - 8, frame.yMax + 8, slot.width + 16, 100), unit == null ? id : RunUICopy.UnitName(unit), names);
                if (TouchTargets.Button(slot, GUIContent.none, GUIStyle.none)) { if (rowing) crossing = id; else picked = id; CheckpointAudio.PlayInterfaceId("ui.select"); }
            }
            if (!rowing && pages > 1)
            {
                Rect back = new Rect(card.x + 24, card.y + 249, 64, 64), next = new Rect(card.xMax - 88, card.y + 249, 64, 64);
                if (ArcadeSkin.Button(back, "◀", KitButton.Secondary, true, label)) page = (page + pages - 1) % pages;
                if (ArcadeSkin.Button(next, "▶", KitButton.Secondary, true, label)) page = (page + 1) % pages;
            }
            if (rowing && shown == 0) LocalizedGUI.Label(new Rect(card.x + 60, card.y + 250, card.width - 120, 60), "Nobody is waiting on the far bank tonight.", plain);
            float rowY = card.y + 470; // a clear gap under the names
            if (rowing) DrawRowing(card, rowY, ink);
            else DrawChoices(card, rowY, ink);
            if (ArcadeSkin.Button(new Rect(card.center.x - 170, card.yMax - 112, 340, 80), rowing ? "BACK" : "LEAVE", KitButton.Secondary, true, label) && !CardPop.Closing(Key))
            {
                if (rowing) { rowing = false; crossing = null; } else CardPop.CloseThen(Key, Close);
            }
            GUI.color = keep;
        }

        /// <summary>The two answers for the picked friend, each with what it gives and costs.</summary>
        private void DrawChoices(Rect card, float y, Color ink)
        {
            RunSession session = owner.Session;
            bool ready = picked != null;
            float half = (card.width - 160) * .5f;
            Rect home = new Rect(card.x + 60, y, half, 190), row = new Rect(home.xMax + 40, y, half, 190);
            plain.normal.textColor = ink;
            string homeText = !session.SeeHomeOpen ? "Someone already went home tonight."
                : !ready ? "Half of what they cost comes back, and they leave a letter."
                : "+" + session.SeeHomeRefund(picked) + " gold · a letter (Lamp Oil at the end)" + (session.SeeHomeReturnsPlace(picked) ? " · a place for a new friend" : "");
            if (letterIcon != null) RunHud.Sprite(new Rect(home.x, home.y, 56, 56), letterIcon);
            LocalizedGUI.Label(new Rect(home.x + 64, home.y, home.width - 64, 92), homeText, plain);
            bool canHome = ready && session.SeeHomeOpen;
            if (ArcadeSkin.Button(new Rect(home.x, home.y + 100, home.width, 84), canHome ? "SEE THEM HOME" : "PICK ONE", KitButton.Danger, canHome, label) && canHome && !CardPop.Closing(Key))
                CardPop.CloseThen(Key, SendHome);
            bool eggs = session.State.Eggs >= session.FerryEggs;
            string rowText = !session.FerryOpen ? "The ferry already crossed tonight."
                : !eggs ? "Costs " + session.FerryEggs + " dreamsand. You have " + session.State.Eggs + "."
                : "Costs " + session.FerryEggs + " dreamsand. A new friend at the same stars.";
            if (ferryIcon != null) RunHud.Sprite(new Rect(row.x, row.y, 56, 56), ferryIcon);
            LocalizedGUI.Label(new Rect(row.x + 64, row.y, row.width - 64, 92), rowText, plain);
            bool canRow = ready && session.FerryOpen && eggs;
            if (ArcadeSkin.Button(new Rect(row.x, row.y + 100, row.width, 84), canRow ? "ROW ACROSS" : "PICK ONE", KitButton.Primary, canRow, label) && canRow)
            { rowing = true; crossing = null; CheckpointAudio.PlayInterfaceId("ui.select"); }
        }

        private void DrawRowing(Rect card, float y, Color ink)
        {
            RunSession session = owner.Session;
            plain.normal.textColor = ink;
            LocalizedGUI.Label(new Rect(card.x + 60, y, card.width - 120, 60), crossing == null ? "Pick who comes back."
                : RunUICopy.UnitName(owner.Content.Unit(crossing)) + " joins at " + RunSession.StarText(session.StarProgress(picked)) + " stars for " + session.FerryEggs + " dreamsand.", plain);
            bool ready = crossing != null;
            if (ArcadeSkin.Button(new Rect(card.center.x - 220, y + 90, 440, 88), ready ? "BRING THEM ACROSS" : "PICK ONE", KitButton.Primary, ready, label) && ready && !CardPop.Closing(Key))
                CardPop.CloseThen(Key, Row);
        }

        private void OnDestroy()
        {
            if (registered) ShopLayout.Active?.Release(LayoutId);
            ShopFocus.Unregister(this); Showing = false;
        }
    }
}
