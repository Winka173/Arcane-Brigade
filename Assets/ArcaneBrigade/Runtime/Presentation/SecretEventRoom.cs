using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(-30)]
    public sealed class SecretEventRoom : MonoBehaviour, ShopFocus.ISource
    {
        private ArenaController owner;
        private readonly List<SpriteRenderer> portals = new List<SpriteRenderer>();
        private readonly List<SecretEventKind> kinds = new List<SecretEventKind>();
        private GameObject gallery;
        private SpriteRenderer centerpiece;
        private string signature;
        private float hold, enteredAt;
        private int selected;
        private readonly Vector2[] copyScroll = new Vector2[3]; private Vector2 resultScroll; // long offers scroll (play-test 20/10/2026)
        private bool armed;
        private GUIStyle heading, body, small, card, label, primary;
        private Texture2D panel, paper, disabled;
        private Vector2 journalScroll;
        // shop spots at least 1.8 from every other thing you can use (stalls, reroll machine, fusion altar, way out, the
        // warm chair's whole path): the first sat 0.9 from the reroll machine and blocked it, the second 0.4 from the
        // altar. Left, above the reroll machine and clear of the wallet and the merchant; right, under the merchant's speech, clear of
        // the chair. ShopLayout keeps the decorations off them; ShopLayoutCapture measures the spacing in every shop.
        public static readonly Vector2[] Positions = { new Vector2(-3.95f, 0.85f), new Vector2(4.7f, -1.4f) };
        public void Configure(ArenaController controller) { owner = controller; ShopFocus.Register(this); SecretArt.UseItemPaintings(controller.Content); }
        public const float InvitationReach = 1.5f;
        public int SpotCount => portals.Count;
        public bool Spot(int index, out Vector2 position, out float reach)
        {
            position = Positions[Mathf.Min(index, Positions.Length - 1)]; reach = InvitationReach;
            return owner?.Simulation != null && owner.Session != null && !InRoom && owner.Session.State.Phase == SessionPhase.Shop && !owner.TransitionPending && index < Positions.Length;
        }
        public bool InRoom => owner?.Session?.SecretRoomOpen == true;
        public bool Focused => !InRoom && NearestPortal() >= 0;
        public bool BlocksShop => InRoom || Focused;
        public bool MenuOpen => InRoom && owner.Session.State.Phase == SessionPhase.Shop && !walkingOut;
        // after the deal the way back is a rift: stand by it and press (Space or its button) to go through, like every door
        public static readonly Vector2 ReturnSpot = new Vector2(0, -3.4f);
        private const float ReturnReach = .9f;
        private bool walkingOut;
        private Transform returnRift, returnSwirl;
        private float returnTurn;
        // the deal's card stays over the room while the way back is open; OK puts it away, walking into the rift leaves anyway
        private bool cardOpen; private float cardClosingAt = -1;
        public bool RiftOpen => walkingOut && returnRift != null;
        private void BeginWalkOut()
        {
            if (walkingOut || gallery == null) return;
            walkingOut = true; cardOpen = true; cardClosingAt = -1;
            MapTheme theme = MapIdentity.For(owner.Session.State.Wave);
            var art = OctoberDoor.PortalArt(theme);
            if (art.body == null) { walkingOut = false; owner.LeaveSecret(); return; }
            var root = new GameObject("ReturnRift"); root.transform.SetParent(gallery.transform, false); root.transform.localPosition = ReturnSpot;
            var body = new GameObject("Rim").AddComponent<SpriteRenderer>(); body.transform.SetParent(root.transform, false);
            body.sprite = art.body; body.sortingOrder = 312;
            body.transform.localScale = Vector3.one * (2f / Mathf.Max(.01f, art.body.bounds.size.y));
            Bounds b = art.body.bounds; Vector4 hole = OctoberDoor.PortalHole(theme);
            var holder = new GameObject("Swirl holder").transform; holder.SetParent(body.transform, false);
            holder.localPosition = new Vector3(b.min.x + b.size.x * hole.x, b.min.y + b.size.y * hole.y, 0);
            holder.localScale = new Vector3(b.size.x * hole.z * 2.12f / art.swirl.bounds.size.x, b.size.y * hole.w * 2.12f / art.swirl.bounds.size.y, 1);
            var swirl = new GameObject("Swirl").AddComponent<SpriteRenderer>(); swirl.transform.SetParent(holder, false);
            swirl.sprite = art.swirl; swirl.sortingOrder = 311;
            returnRift = root.transform; returnSwirl = swirl.transform; returnTurn = 0;
            root.transform.localScale = new Vector3(.15f, .02f, 1); // it tears open (WalkOut grows it)
        }
        /// <summary>The return rift turns and opens wider as the hero nears; stepping into it leaves the room.</summary>
        private bool nearReturn; private float returnPromptAt = -1;
        private void GoBack() { if (SceneDissolve.Busy) return; walkingOut = false; returnRift = null; nearReturn = false; SceneDissolve.Then(owner, () => owner.LeaveSecret()); } // the shop melts back in
        private void WalkOut()
        {
            if (returnRift == null) return;
            Vector2 hero = owner.Simulation.Champion.Position;
            float near = Mathf.Clamp01(1 - (Vector2.Distance(hero, ReturnSpot) - ReturnReach) / 3f);
            bool still = PresentationSettings.ReducedMotion;
            float target = 1 + .12f * near;
            Vector3 scale = returnRift.localScale;
            returnRift.localScale = still ? Vector3.one * target : new Vector3(Mathf.MoveTowards(scale.x, target, Time.deltaTime * 4), Mathf.MoveTowards(scale.y, target, Time.deltaTime * 4), 1);
            if (!still) returnTurn += Time.deltaTime * Mathf.Lerp(60, 240, near);
            returnSwirl.localRotation = Quaternion.Euler(0, 0, -returnTurn);
            float distance = Vector2.Distance(hero, ReturnSpot);
            nearReturn = distance <= ReturnReach + .6f && returnRift.localScale.y > .9f;
            if (!nearReturn) returnPromptAt = -1;
            // requirement 25/10/2026: touching the rift never takes you through by itself; standing by it, Space (or GO THROUGH) does
            bool confirm = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0);
            if (cardOpen && !nearReturn) { if (confirm && cardClosingAt < 0) cardClosingAt = Time.unscaledTime; return; } // the first press puts the card away
            if (nearReturn && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0))) GoBack();
        }
        private int NearestPortal()
        {
            if (owner?.Simulation == null || owner.Session == null || owner.Session.State.Phase != SessionPhase.Shop || owner.TransitionPending) return -1;
            for (int i = 0; i < portals.Count; i++) if (ShopFocus.Has(this, i, owner.Simulation.Champion.Position)) return i;
            return -1;
        }
        private void Update()
        {
            if (owner?.Session == null || SecretEventAssets.Current == null) return;
            SecretEventProgress s = owner.Session.State.Secrets;
            string key = owner.Session.State.RunId + "/" + owner.Session.State.Wave + "/" + owner.Session.State.Phase + "/" + owner.Session.State.OutsideStage + "/" + s?.Stage + "/" + s?.Kind + "/" + s?.Visits + "/" + s?.VisitedMask + "/" + s?.RewardId + "/" + s?.QueuedEvent;
            if (signature != key) { signature = key; walkingOut = false; picker = 0; returnRift = null; Rebuild(); enteredAt = Time.unscaledTime; selected = 0; hold = 0; armed = false; }
            if (picker != 0 && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))) picker = 0;
            if (owner.PresentationPaused || owner.TransitionPending || owner.HallExitPending || closing != null || picker != 0) { hold = 0; return; }
            bool pressed = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.JoystickButton0);
            bool confirm = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0);
            if (!pressed) armed = true;
            if (walkingOut && InRoom) { WalkOut(); return; }
            if (!InRoom)
            {
                int near = NearestPortal();
                for (int i = 0; i < portals.Count; i++)
                {
                    Sprite look = SecretArt.Portal(kinds[i], i == near);
                    if (portals[i].sprite != look) { portals[i].sprite = look; Fit(portals[i].transform, look, 1.4f, 1.9f); } // the opened painting is larger: same size on screen
                    float sway = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(owner.PresentationClock * 1.4f + i) * .022f;
                    portals[i].transform.localRotation = Quaternion.Euler(0, 0, sway * 20);
                }
                // requirement 25/10/2026 (Space did nothing here): real time, not the presentation clock (it can stand still after a
                // boss's slow motion), and the same way in as the DISCOVER button
                if (near >= 0 && confirm && armed && Time.unscaledTime - enteredAt > .2f && !SceneDissolve.Busy) { armed = false; SecretEventKind kind = kinds[near]; SceneDissolve.Then(owner, () => owner.EnterSecret(kind)); }
            }
            else if (MenuOpen)
            {
                // the deal is done: the way back opens at once, under the deal's card (requirement 25/10/2026)
                if (s.Stage != SecretEventStage.Choosing && string.IsNullOrEmpty(s.RewardId)) { BeginWalkOut(); return; }
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton4)) { selected = (selected + 2) % 3; hold = 0; }
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton5)) { selected = (selected + 1) % 3; hold = 0; }
                for (int i = 0; i < 3; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { selected = i; hold = 0; }
                if (confirm && armed && Time.unscaledTime - enteredAt > .2f)
                {
                    armed = false; hold = 0;
                    int pick = selected;
                    if (s.Stage == SecretEventStage.Choosing) Close(() => owner.ChooseSecret(pick));
                    else if (!string.IsNullOrEmpty(s.RewardId)) owner.ClaimSecret();
                }
            }
        }
        private void Rebuild()
        {
            foreach (SpriteRenderer p in portals) if (p != null) Destroy(p.gameObject);
            portals.Clear(); kinds.Clear();
            if (gallery != null) Destroy(gallery);
            if (owner.Session.SecretRoomOpen)
            {
                gallery = new GameObject("SecretRoom_" + owner.Session.State.Secrets.Kind); gallery.transform.SetParent(transform, false);
                if (owner.Session.State.Phase == SessionPhase.Shop)
                {
                    centerpiece = Prop(gallery.transform, "OpenedSecretDoor", SecretArt.Portal(owner.Session.State.Secrets.Kind, true), new Vector2(0, -.15f), 3.25f, 310, 4.4f); // wide doorways (the train) keep clear of the curios at either side
                    Prop(gallery.transform, "LeftArtifact", SecretArt.Relic(owner.Session.State.Secrets.Kind), new Vector2(-3.4f, 1.2f), .85f, 310, 1.1f);
                    Prop(gallery.transform, "RightArtifact", SecretArt.Artifact(owner.Session.State.Secrets.Kind), new Vector2(3.4f, 1.2f), .85f, 310, 1.1f);
                }
                return;
            }
            SecretEventKind[] available = owner.Session.SecretPortals();
            for (int i = 0; i < available.Length; i++)
            {
                kinds.Add(available[i]);
                SpriteRenderer p = Prop(transform, "SecretInvitation_" + available[i], SecretArt.Portal(available[i], false), Positions[i], 1.4f, 315, 1.9f); // 1.4 tall: the widest (the train) fits between the wallet and the merchant
                portals.Add(p);
                var glow = new GameObject("PortalGlow"); glow.transform.SetParent(p.transform, false); glow.AddComponent<ShopRarityGlow>().Configure(ItemRarity.Rare, owner, i * 2, p, false);
            }
        }
        private static void Fit(Transform t, Sprite sprite, float height, float maxWidth) => t.localScale = Vector3.one * Mathf.Min(height / Mathf.Max(.1f, sprite.bounds.size.y), maxWidth / Mathf.Max(.1f, sprite.bounds.size.x));
        private SpriteRenderer Prop(Transform parent, string name, Sprite sprite, Vector2 at, float height, int order, float maxWidth = float.PositiveInfinity)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false); obj.transform.localPosition = at;
            SpriteRenderer r = obj.AddComponent<SpriteRenderer>(); r.sprite = sprite; r.sortingOrder = order;
            Fit(obj.transform, sprite, height, maxWidth);
            obj.AddComponent<SuspendedScenery>().Configure(owner, r, at.x * 3 + at.y * 7);
            return r;
        }
        private Texture2D Solid(Color color)
        {
            Texture2D t = new Texture2D(1, 1); t.SetPixel(0, 0, color); t.Apply(); return t;
        }
        private void Styles()
        {
            if (heading != null) return;
            panel = Solid(new Color(.055f, .065f, .105f, .96f)); paper = Solid(new Color(.93f, .86f, .71f)); disabled = Solid(new Color(.15f, .17f, .22f));
            heading = new GUIStyle(GUI.skin.label) { font = ArcadeTypography.Body, fontSize = 30, alignment = TextAnchor.MiddleCenter, wordWrap = true }; heading.normal.textColor = new Color(1, .79f, .39f);
            if (ArcadeTypography.Display != null) heading.font = ArcadeTypography.Display;
            body = new GUIStyle(heading) { font = ArcadeTypography.Body, fontSize = 19, alignment = TextAnchor.UpperLeft }; body.normal.textColor = new Color(.95f, .92f, .84f); body.richText = true;
            small = new GUIStyle(body) { fontSize = 16 };
            card = new GUIStyle(ArcadeSkin.Skin.button) { font = ArcadeTypography.Body, fontSize = 20, wordWrap = true, alignment = TextAnchor.MiddleCenter };
            primary = ArcadeSkin.Primary(card);
            label = new GUIStyle(small) { alignment = TextAnchor.MiddleCenter };
        }
        private int hintShown = -1; private float hintAt, resultAt; private string resultShown;
        // the room opens with motion (plates one after another) and closes with it before the deal or the way back takes effect
        private const float CloseSeconds = .16f;
        private float roomAt = -1, closingAt;
        private System.Action closing;
        private void Close(System.Action then) { if (closing != null) return; closing = then; closingAt = Time.unscaledTime; }
        /// <summary>Draws one piece of the room popping in `delay` seconds after the room opened.</summary>
        private void Popped(float delay, Vector2 pivot, System.Action draw)
        {
            Matrix4x4 m = GUI.matrix; Color c = GUI.color;
            LivingUI.Pop(Time.unscaledTime - roomAt - delay, pivot);
            draw();
            GUI.matrix = m; GUI.color = c;
        }
        private void OnGUI()
        {
            if (owner?.Session == null || owner.PresentationPaused || owner.TransitionPending || SecretEventAssets.Current == null) return;
            Styles(); Matrix4x4 previous = GUI.matrix; int depth = GUI.depth; GUI.depth = -30;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            if (!InRoom)
            {
                roomAt = -1; closing = null;
                int near = NearestPortal();
                if (near != hintShown) { hintShown = near; hintAt = Time.unscaledTime; }
                if (near >= 0)
                {
                    if (ShopPrompt.Draw(SecretEventAssets.Titles[(int)kinds[near]], SecretEventAssets.Plain[(int)kinds[near]], "DISCOVER", Time.unscaledTime - hintAt) && !SceneDissolve.Busy) { SecretEventKind kind = kinds[near]; SceneDissolve.Then(owner, () => owner.EnterSecret(kind)); } // the room melts in
                }
            }
            else if (MenuOpen)
            {
                SecretEventProgress s = owner.Session.State.Secrets;
                if (roomAt < 0) roomAt = Time.unscaledTime;
                if (closing != null)
                {
                    float k = (Time.unscaledTime - closingAt) / CloseSeconds;
                    if (k >= 1) { System.Action then = closing; closing = null; roomAt = -1; resultShown = null; GUI.matrix = previous; GUI.depth = depth; GUI.color = Color.white; then(); return; }
                    if (!PresentationSettings.ReducedMotion) ArcadeSkin.ScaleAround(Vector2.one * Mathf.Lerp(1, .94f, k), new Vector2(960, 560));
                    GUI.color *= new Color(1, 1, 1, 1 - k);
                }
                bool shut = closing != null || picker != 0; blocked = shut;
                // the room as plates (play-test 20/10/2026, item 7): the name and one line, your relic in the middle (tap to swap),
                // three DO / LOSE / GET plates, one confirm button and a small way back
                // drawn in code (21/10/2026): the room's painted vignette when it has one, a title banner, your relic on a
                // pedestal, three walnut choice plates with cream DO, oxblood LOSE and green-gilt GET rows
                Sprite vignette = SecretEventVignettes.For(s.Kind);
                float bannerY = 124;
                if (vignette != null)
                {
                    float vh = 150, vw = vh * vignette.rect.width / vignette.rect.height;
                    Popped(0, new Vector2(960, vh * .5f), () =>
                    {
                        ArcadeSkin.Glow(new Rect(960 - vw * .75f, 0, vw * 1.5f, vh + 24), new Color(1f, .8f, .5f, .16f));
                        DrawSprite(new Rect(960 - vw * .5f, 4, vw, vh), vignette);
                    });
                    bannerY = 148;
                }
                Popped(.04f, new Vector2(960, bannerY + 50), () =>
                {
                    ArcadeSkin.TitleBanner(new Rect(560, bannerY, 800, 64), SecretEventAssets.Titles[(int)s.Kind], heading);
                    // what the room is for, on its own dark plate so it reads over the room's art
                    float plainW = Mathf.Min(1320, label.CalcSize(new GUIContent(SecretEventAssets.Plain[(int)s.Kind])).x + 60);
                    ArcadeSkin.CodePlate(new Rect(960 - plainW * .5f, bannerY + 66, plainW, 36), CodeTone.Walnut, 0);
                    GUI.Label(new Rect(300, bannerY + 68, 1320, 32), SecretEventAssets.Plain[(int)s.Kind], label);
                });
                if (s.Stage == SecretEventStage.Choosing)
                {
                    bool target = owner.Session.SecretUsesTarget;
                    if (target) Popped(.08f, new Vector2(960, 350), () => TargetRelic(s));
                    SecretChoice[] choices = owner.Session.SecretChoices();
                    if (!choices[selected].Enabled) for (int i = 0; i < 3; i++) if (choices[i].Enabled) { selected = i; break; }
                    float top = target ? 470 : 300; // the offered relic's name stays clear of the cards' sockets
                    Sprite relic = owner.Content.Item(s.TargetItem)?.Icon;
                    markRelic = owner.Content.Item(s.TargetItem)?.Name; UnitDefinition markUnit = owner.Content.Unit(s.TargetUnit); markFriend = markUnit != null ? RunUICopy.UnitName(markUnit) : null;
                    // requirement 25/10/2026 (cramped, text on the frame, the badge off its socket): one painted card each, its
                    // picture centred in the card's own socket, the title and three gem lines on open parchment with wide margins
                    Sprite cardArt = ArcadeSkin.KitSprite("SecretEvent/Card");
                    float cw = 540, ch = cardArt != null ? cw * cardArt.rect.height / cardArt.rect.width : 430, gap = 24;
                    if (top + ch > 868) top = 868 - ch; // the deal button stays clear below
                    for (int i = 0; i < 3; i++)
                    {
                        SecretChoice c = choices[i];
                        bool lit = selected == i && c.Enabled;
                        Rect plate = new Rect(960 - cw * 1.5f - gap + i * (cw + gap), top - (lit ? 10 : 0), cw, ch); // the chosen card lifts
                        Matrix4x4 plateMatrix = GUI.matrix; Color plateColour = GUI.color;
                        LivingUI.Pop(Time.unscaledTime - roomAt - .12f - i * .06f, plate.center); // the three cards land one after another
                        Color keep = GUI.color; if (!c.Enabled) GUI.color = new Color(keep.r * .7f, keep.g * .7f, keep.b * .75f, keep.a * .7f);
                        if (cardArt != null) DrawSprite(plate, cardArt); else ArcadeSkin.CodePlate(plate, CodeTone.Walnut, !c.Enabled ? 3 : lit ? 1 : 0);
                        if (lit) ArcadeSkin.RunningLight(plate, Time.unscaledTime, new Color(1f, .82f, .45f));
                        Vector2 socket = new Vector2(plate.center.x, plate.y + ch * CardSocketV);
                        float hole = cw * CardSocketD;
                        DrawFitted(new Rect(socket.x - hole * .4f, socket.y - hole * .4f, hole * .8f, hole * .8f), c.Icon != null ? c.Icon : SecretArt.Relic(s.Kind));
                        float padX = cw * CardPadX, y0 = socket.y + hole * .5f + 8, y1 = plate.yMax - ch * CardPadBottom;
                        if (plateTitle == null) Row(Rect.zero, null, null, null, CodeTone.Cream);
                        Color titleTone = plateTitle.normal.textColor; plateTitle.normal.textColor = cardArt != null ? ArcadeSkin.KitInk : titleTone;
                        int titleSize = plateTitle.fontSize; plateTitle.fontSize = 26;
                        GUI.Label(new Rect(plate.x + padX, y0, cw - padX * 2, 34), c.Title, plateTitle);
                        plateTitle.fontSize = titleSize; plateTitle.normal.textColor = titleTone;
                        float rowsTop = y0 + (c.Enabled ? 40 : 70), rowH = (y1 - rowsTop) / 3; // a closed deal says so under its title
                        GemRow(new Rect(plate.x + padX, rowsTop, cw - padX * 2, rowH), "DO", c.Do, CodeTone.Cream, true);
                        GemRow(new Rect(plate.x + padX, rowsTop + rowH, cw - padX * 2, rowH), "LOSE", c.Lose, CodeTone.Oxblood, true);
                        GemRow(new Rect(plate.x + padX, rowsTop + rowH * 2, cw - padX * 2, rowH), "GET", c.Get, CodeTone.Green, true);
                        GUI.color = keep;
                        if (!c.Enabled)
                        {
                            // a closed deal: a lock and the words on their own line under the title, no box drawn over the card
                            Vector2 mid = new Vector2(plate.center.x, y0 + 52);
                            ArcadeSkin.Lock(new Vector2(mid.x - 96, mid.y), 16, ArcadeSkin.KitInk);
                            Color was = label.normal.textColor; label.normal.textColor = ArcadeSkin.KitInk;
                            GUI.Label(new Rect(mid.x - 80, mid.y - 20, 200, 40), "Not available now", label);
                            label.normal.textColor = was;
                        }
                        else if (!shut && GUI.Button(plate, GUIContent.none, GUIStyle.none)) { selected = i; hold = 0; }
                        GUI.matrix = plateMatrix; GUI.color = plateColour;
                    }
                    Popped(.3f, new Vector2(960, 922), () =>
                    {
                        GUI.enabled = choices[selected].Enabled;
                        int pick = selected;
                        if (ArcadeSkin.RibbonButton(new Rect(740, 880, 440, 84), owner.Session.SecretVerb, GUI.enabled, primary) && !shut) Close(() => owner.ChooseSecret(pick)); // the wax-sealed contract ribbon
                        GUI.enabled = true;
                        if (ArcadeSkin.Button(new Rect(1250, 892, 300, 64), "BACK TO THE SHOP") && !shut) Close(() => owner.LeaveSecret()); // a full button, not a faint tag
                    });
                    DrawPicker(s); // over everything in the room
                }
                else DrawDeal(s, shut);
            }
            else if (walkingOut)
            {
                if (cardOpen) DrawDeal(owner.Session.State.Secrets, cardClosingAt >= 0);
                if (nearReturn)
                {
                    if (returnPromptAt < 0) returnPromptAt = Time.unscaledTime;
                    if (ShopPrompt.Draw("THE WAY BACK", "The rift leads back to the shop.", "GO THROUGH", Time.unscaledTime - returnPromptAt)) GoBack();
                }
            }
            else if (owner.Session.SecretFighting)
            {
                GUI.Label(new Rect(48, 1020, 1000, 30), SecretEventAssets.Titles[(int)owner.Session.State.Secrets.Kind], small); // padded in from the corner
            }
            GUI.matrix = previous; GUI.depth = depth; GUI.color = Color.white;
        }
        private GUIStyle plateTitle, tagStyle;
        /// <summary>A result saved before deals kept their parts: its lines as they were written.</summary>
        private void LegacyResult(SecretEventProgress s, bool shut)
        {
            ArcadeSkin.Frame(new Rect(420, 540, 1080, 315), KitFrame.Dialogue, .6f);
            body.wordWrap = true; float resultHeight = body.CalcHeight(new GUIContent(s.Result), 990);
            resultScroll = ScrollPanel.Begin(new Rect(455, 562, 1010, 170), resultScroll, new Rect(0, 0, 990, resultHeight));
            GUI.Label(new Rect(0, 0, 990, resultHeight), s.Result, body);
            ScrollPanel.End();
            if (ArcadeSkin.KitGuiButton(new Rect(660, 745, 600, 68), "OK", primary) && !shut) cardClosingAt = Time.unscaledTime;
        }
        /// <summary>Your relic in the middle of the room: its picture and name; a tap shows the next one you own.</summary>
        private void TargetRelic(SecretEventProgress s)
        {
            Rect box = new Rect(810, 240, 300, 215);
            Rect stand = new Rect(850, 262, 220, 160);
            ArcadeSkin.Pedestal(stand, Time.unscaledTime); // velvet cushion on a brass stand, drawn in code
            ItemDefinition relic = owner.Content.Item(s.TargetItem);
            if (relic?.Icon != null) DrawSprite(ArcadeSkin.PedestalSeat(stand), relic.Icon);
            GUI.Label(new Rect(box.x - 40, 422, box.width + 80, 30), (relic?.Name ?? "No relic to offer") + (owner.Session.SecretTargets.Count > 1 ? "  ·  tap to change" : ""), label);
            if (!blocked && owner.Session.SecretTargets.Count > 1 && GUI.Button(box, GUIContent.none, GUIStyle.none)) OpenPicker(1);
            if (s.Kind == SecretEventKind.DeadLetters && owner.Session.State.Roster.Count > 0)
            {
                Rect friend = new Rect(1130, 300, 300, 100);
                ArcadeSkin.Plate(friend, ArcadeSkin.Accent);
                UnitDefinition unit = owner.Content.Unit(s.TargetUnit);
                if (unit?.Body != null) DrawSprite(new Rect(friend.x + 10, friend.y + 10, 80, 80), unit.Body);
                GUI.Label(new Rect(friend.x + 96, friend.y + 10, 196, 80), (unit != null ? RunUICopy.UnitName(unit) : "A friend") + "\ntap to change", label);
                if (!blocked && owner.Session.State.Roster.Count > 1 && GUI.Button(friend, GUIContent.none, GUIStyle.none)) OpenPicker(2);
            }
        }
        /// <summary>One LOSE (red) or GET (green) row: a coloured tag, a picture when there is one, one short line.</summary>
        private string markRelic, markFriend;
        /// <summary>The offered relic's and friend's names in colour, so a plate says plainly who gains or loses what.</summary>
        private string Marked(string text, CodeTone tone)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string colour = tone == CodeTone.Cream ? "#9a5200" : "#ffd76a";
            if (!string.IsNullOrEmpty(markRelic)) text = text.Replace(markRelic, "<color=" + colour + "><b>" + markRelic + "</b></color>");
            if (!string.IsNullOrEmpty(markFriend) && markFriend != markRelic) text = text.Replace(markFriend, "<color=" + colour + "><b>" + markFriend + "</b></color>");
            return text;
        }
        /// <summary>The deal's card (requirement 25/10/2026: roomy, the picture in the panel, no light wheel round it): its
        /// picture, LOSE and GET as gem lines, one button. OK puts it away; a reward is claimed from it.</summary>
        private void DrawDeal(SecretEventProgress s, bool shut)
        {
            if (s == null) return;
            if (resultShown != s.Result + "/" + s.RewardId) { resultShown = s.Result + "/" + s.RewardId; resultAt = Time.unscaledTime; }
            bool claim = !string.IsNullOrEmpty(s.RewardId);
            float away = cardClosingAt < 0 ? 0 : (Time.unscaledTime - cardClosingAt) / CloseSeconds;
            if (away >= 1) { cardOpen = false; cardClosingAt = -1; return; }
            Matrix4x4 m = GUI.matrix; Color c = GUI.color;
            LivingUI.Pop(Time.unscaledTime - resultAt, new Vector2(960, 560));
            if (away > 0) { if (!PresentationSettings.ReducedMotion) ArcadeSkin.ScaleAround(Vector2.one * Mathf.Lerp(1, .94f, away), new Vector2(960, 560)); GUI.color *= new Color(1, 1, 1, 1 - away); }
            if (!claim && string.IsNullOrEmpty(s.ResultGot)) { LegacyResult(s, shut); GUI.matrix = m; GUI.color = c; return; }
            if (claim && owner.Content.Item(s.RewardId) != null) { RewardCard(s, shut); GUI.matrix = m; GUI.color = c; return; }
            Sprite panelArt = ArcadeSkin.KitSprite("SecretEvent/Panel");
            float pw = 1040, ph = panelArt != null ? pw * panelArt.rect.height / panelArt.rect.width : 430;
            Rect frame = new Rect(960 - pw * .5f, 330, pw, ph);
            if (panelArt != null) DrawSprite(frame, panelArt); else ArcadeSkin.Frame(frame, KitFrame.Dialogue, .6f);
            float pad = pw * PanelPad, x0 = frame.x + pad, top = frame.y + ph * PanelPadTop, bottom = frame.yMax - ph * PanelPadTop;
            string[] lines = s.Result.Split('\n');
            GUI.Label(new Rect(x0, top, frame.width - pad * 2, 44), lines[0], heading);
            string iconId = claim ? s.RewardId : s.ResultIcon;
            Sprite icon = owner.Content.Item(iconId)?.Icon ?? owner.Content.Unit(iconId)?.Body ?? SecretArt.Relic(s.Kind); // every deal shows a picture
            float rowsTop = top + 56, buttonH = 64, rowsBottom = bottom - buttonH - 14;
            if (icon != null)
            {
                float size = Mathf.Min(170, rowsBottom - rowsTop);
                Rect pic = new Rect(x0 + 10, (rowsTop + rowsBottom) * .5f - size * .5f, size, size);
                ArcadeSkin.Glow(new Rect(pic.x - size * .25f, pic.y - size * .25f, size * 1.5f, size * 1.5f), new Color(1f, .82f, .5f, .28f)); // held up to the light, inside the panel
                DrawFitted(pic, icon); x0 = pic.xMax + 36;
            }
            float w = frame.xMax - pad - x0;
            string got = claim ? (owner.Content.Item(s.RewardId) != null ? "New relic" : owner.Session.SecretRewardLabel) + " · " + owner.Session.SecretRewardStats(s.RewardId) : s.ResultGot;
            bool paid = !claim && !string.IsNullOrEmpty(s.ResultPaid) && s.ResultPaid != "Nothing";
            float rowH = (rowsBottom - rowsTop) / (paid ? 2 : 1);
            if (paid) GemRow(new Rect(x0, rowsTop, w, rowH), "LOSE", s.ResultPaid, CodeTone.Oxblood, false);
            GemRow(new Rect(x0, rowsTop + (paid ? rowH : 0), w, rowH), "GET", got.Replace("\n", " · "), CodeTone.Green, false);
            Rect go = new Rect(frame.center.x - 260, bottom - buttonH, 520, buttonH);
            if (claim) { if (ArcadeSkin.KitGuiButton(go, "CLAIM", primary) && !shut) owner.ClaimSecret(); }
            else if (ArcadeSkin.KitGuiButton(go, "OK", primary) && !shut) cardClosingAt = Time.unscaledTime; // the way back is already open
            GUI.matrix = m; GUI.color = c;
        }
        private ShopDescriptionCard rewardCard; private string rewardCardId;
        private GUIStyle rewardName, rewardKind, rewardLine;
        /// <summary>The relic won in a fight, on the shop's own description card (requirement 25/10/2026): its rarity frame, the
        /// picture rising on its rarity backdrop, its name and kind, its full description with stat icons, and CLAIM.</summary>
        private void RewardCard(SecretEventProgress s, bool shut)
        {
            if (rewardCardId != s.RewardId || rewardCard == null)
            {
                rewardCard = new ShopDescriptionCard(); rewardCardId = s.RewardId;
                rewardCard.Refresh(owner.Session, owner.Content, new ShopOffer { Token = -89, Kind = OfferKind.Item, Id = s.RewardId, Tier = 1, Price = 0 });
            }
            ItemDefinition item = owner.Content.Item(s.RewardId);
            rewardName ??= new GUIStyle(ArcadeTypography.Style(32, true)) { wordWrap = false };
            rewardKind ??= new GUIStyle(ArcadeTypography.Style(18)) { wordWrap = false };
            rewardLine ??= new GUIStyle(ArcadeTypography.Style(22)) { wordWrap = true };
            rewardName.normal.textColor = rewardLine.normal.textColor = ArcadeSkin.Cream;
            rewardKind.normal.textColor = RunModifierRules.RarityColor(rewardCard.Rarity);
            const float width = 1000, textW = width - 120 - 190;
            string summary = rewardCard.Summary ?? "";
            float textH = Mathf.Max(150, 64 + GlossaryUI.Height(summary, rewardLine, textW));
            float h = 96 + textH + 130;
            Rect card = new Rect(960 - width * .5f, Mathf.Max(260, 600 - h * .5f), width, h);
            RarityDescriptionFrame.Draw(card, rewardCard.Rarity, Time.unscaledTime);
            Rect inner = new Rect(card.x + 60, card.y + 34, card.width - 120, card.height - 68);
            GUI.Label(new Rect(inner.x, inner.y, inner.width, 44), s.Result.Split('\n')[0], heading);
            // the relic rises onto its rarity's backdrop with a little bounce and settles
            float held = Time.unscaledTime - resultAt, top = inner.y + 64;
            float rise = PresentationSettings.ReducedMotion ? 1 : Mathf.Clamp01(held / .45f), bounce = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(Mathf.Clamp01(held / .7f) * Mathf.PI) * 14;
            float side = 130 * (PresentationSettings.ReducedMotion ? 1 : Mathf.Lerp(.55f, 1, Mathf.SmoothStep(0, 1, rise)));
            RarityDescriptionFrame.Backdrop(new Rect(inner.x + 10, top, 150, 150), rewardCard.Rarity, Time.unscaledTime);
            if (item.Icon != null) ShopDescriptionCard.DrawSprite(new Rect(inner.x + 85 - side * .5f, top + 75 - side * .5f - bounce, side, side), item.Icon);
            float tx = inner.x + 190;
            GUI.Label(new Rect(tx, top - 4, textW, 42), item.Name, rewardName);
            GUI.Label(new Rect(tx, top + 36, textW, 24), "NEW RELIC · " + rewardCard.Identity, rewardKind);
            GlossaryUI.Text(new Rect(tx, top + 66, textW, textH - 64), summary, rewardLine);
            GlossaryUI.Draw(new Vector2(1920, 1080));
            Rect go = new Rect(card.center.x - 220, card.yMax - 104, 440, 68);
            if (ArcadeSkin.KitGuiButton(go, "CLAIM", primary) && !shut) owner.ClaimSecret();
        }
        // the painted card and panel: the card's socket (centre from its top, diameter, both shares of the card) and the margins
        private const float CardSocketV = .118f, CardSocketD = .114f, CardPadX = .12f, CardPadBottom = .12f, PanelPad = .08f, PanelPadTop = .1f;
        private GUIStyle gemTag, gemText;
        /// <summary>One line of a deal: its gem, the word (DO, LOSE, GET) and what it means, on parchment (ink) or navy (light).</summary>
        private void GemRow(Rect r, string tag, string text, CodeTone tone, bool parchment)
        {
            gemTag ??= new GUIStyle(label) { fontSize = 15, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperLeft };
            gemText ??= new GUIStyle(body) { wordWrap = true, richText = true, alignment = TextAnchor.UpperLeft };
            Sprite gem = ArcadeSkin.KitSprite(tone == CodeTone.Oxblood ? "SecretEvent/GemLose" : tone == CodeTone.Green ? "SecretEvent/GemGet" : "SecretEvent/GemDo");
            float g = Mathf.Min(34, r.height * .5f);
            if (gem != null) DrawFitted(new Rect(r.x, r.y + 4, g, g), gem);
            Color tagColour = parchment
                ? (tone == CodeTone.Oxblood ? new Color(.62f, .1f, .08f) : tone == CodeTone.Green ? new Color(.1f, .4f, .16f) : new Color(.42f, .28f, .14f))
                : (tone == CodeTone.Oxblood ? new Color(1f, .5f, .45f) : tone == CodeTone.Green ? new Color(.55f, .95f, .55f) : ArcadeSkin.KitCream);
            gemTag.normal.textColor = tagColour;
            float x = r.x + g + 12, tw = r.xMax - x;
            GUI.Label(new Rect(x, r.y, tw, 20), tag, gemTag);
            gemText.normal.textColor = parchment ? (tone == CodeTone.Cream ? ArcadeSkin.KitInk : Color.Lerp(ArcadeSkin.KitInk, tagColour, .55f)) : new Color(.96f, .93f, .86f);
            string marked = Marked(text, parchment ? CodeTone.Cream : tone);
            Rect area = new Rect(x, r.y + 21, tw, r.height - 25);
            gemText.fontSize = parchment ? 18 : 21;
            while (gemText.fontSize > 13 && gemText.CalcHeight(new GUIContent(marked), area.width) > area.height) gemText.fontSize--; // it fits its line
            GUI.Label(area, marked, gemText);
        }
        /// <summary>A picture kept to its own shape inside the box, centred.</summary>
        private void DrawFitted(Rect box, Sprite sprite)
        {
            if (sprite == null) return;
            float aspect = sprite.rect.width / Mathf.Max(1, sprite.rect.height), w = box.width, h = box.height;
            if (aspect > w / h) h = w / aspect; else w = h * aspect;
            DrawSprite(new Rect(box.center.x - w * .5f, box.center.y - h * .5f, w, h), sprite);
        }
        private void Row(Rect r, string tag, string text, Sprite icon, CodeTone tone)
        {
            text = Marked(text, tone);
            if (plateTitle == null) { plateTitle = new GUIStyle(heading) { fontSize = 26 }; tagStyle = new GUIStyle(label) { fontSize = 16, fontStyle = FontStyle.Bold }; tagStyle.normal.textColor = Color.white; }
            if (tag == null) return;
            Rect area = ArcadeSkin.EventRow(r, tag, tone, tagStyle);
            if (icon != null) DrawSprite(new Rect(r.x + 18, r.y + 38, Mathf.Min(48, r.height - 44), Mathf.Min(48, r.height - 44)), icon);
            Color keep = body.normal.textColor;
            body.normal.textColor = tone == CodeTone.Cream ? ArcadeSkin.KitInk : tone == CodeTone.Green ? new Color(.86f, 1f, .86f) : new Color(1f, .86f, .82f);
            body.wordWrap = true;
            area = new Rect(area.x, area.y, area.width - 14, area.height); // clear of the ribbon's right gem
            int size = body.fontSize; TextAnchor align = body.alignment;
            while (body.fontSize > 13 && body.CalcHeight(new GUIContent(text), area.width) > area.height) body.fontSize--; // two lines stay inside the ribbon
            body.alignment = TextAnchor.MiddleLeft; // centred in its ribbon's height
            GUI.Label(area, text, body);
            body.fontSize = size; body.alignment = align; body.normal.textColor = keep;
        }
        // requirement 24/10/2026 (no tap-to-cycle pickers): the relic or friend you offer is chosen from a popup of all of them
        private int picker; // 0 closed, 1 relics, 2 friends
        private float pickerAt;
        private bool blocked; // a popup or the closing room owns the input this frame
        private GUIStyle pickerWhat, pickerName;
        /// <summary>A relic's gifts in a short line: its stats, else its special effect.</summary>
        private string RelicWhat(ItemDefinition item)
        {
            if (item == null) return "";
            string stats = owner.Session.SecretRewardStats(item.Id);
            if (!string.IsNullOrEmpty(stats)) return stats;
            return item.Mechanic != RelicMechanic.None ? RelicDescriptions.ShortMechanic(item.Mechanic) : "";
        }
        private void OpenPicker(int kind) { picker = kind; pickerAt = Time.unscaledTime; CheckpointAudio.PlayInterfaceId("ui.open"); }
        private void DrawPicker(SecretEventProgress s)
        {
            if (picker == 0) return;
            bool relics = picker == 1;
            var choices = new List<(string id, string weapon, Sprite icon, string name, bool chosen, string what)>();
            if (relics)
                foreach (string id in owner.Session.SecretTargets)
                {
                    ItemDefinition item = owner.Content.Item(id);
                    choices.Add((id, "", item?.Icon, item?.Name ?? id, id == s.TargetItem, RelicWhat(item)));
                }
            else
                foreach (RosterEntry r in owner.Session.State.Roster)
                {
                    UnitDefinition unit = owner.Content.Unit(r.UnitId);
                    choices.Add((r.UnitId, r.WeaponId, unit?.Body, unit != null ? RunUICopy.UnitName(unit) : r.UnitId, r.UnitId == s.TargetUnit && r.WeaponId == s.TargetWeapon, RunUICopy.WeaponName(r.WeaponId) + " · " + r.Tier + "★"));
                }
            Color keep = GUI.color;
            GUI.color = new Color(0, 0, 0, .55f * Mathf.Clamp01((Time.unscaledTime - pickerAt) / .16f) * keep.a);
            GUI.DrawTexture(new Rect(0, 0, 1920, 1080), Texture2D.whiteTexture);
            GUI.color = keep;
            int columns = Mathf.Clamp(choices.Count, 1, 5), rows = Mathf.Max(1, Mathf.CeilToInt(choices.Count / 5f));
            const float tile = 160, gap = 16;
            float w = columns * tile + (columns - 1) * gap + 80, h = rows * (tile + 118) + (rows - 1) * gap + 130;
            Rect panel = new Rect(960 - w * .5f, 540 - h * .5f, w, h);
            Matrix4x4 m = GUI.matrix; Color c = GUI.color;
            LivingUI.Pop(Time.unscaledTime - pickerAt, panel.center);
            ArcadeSkin.Frame(panel, KitFrame.Dialogue, .6f);
            GUI.Label(new Rect(panel.x, panel.y + 22, panel.width, 40), relics ? "CHOOSE A RELIC" : "CHOOSE A FRIEND", heading);
            int picked = -1;
            for (int i = 0; i < choices.Count; i++)
            {
                Rect cell = new Rect(panel.x + 40 + (i % 5) * (tile + gap), panel.y + 80 + (i / 5) * (tile + 118 + gap), tile, tile + 118);
                ArcadeSkin.CodePlate(new Rect(cell.x, cell.y, tile, tile), CodeTone.Walnut, choices[i].chosen ? 1 : 0);
                if (choices[i].chosen) ArcadeSkin.RunningLight(new Rect(cell.x - 4, cell.y - 4, tile + 8, tile + 8), Time.unscaledTime, new Color(1f, .82f, .45f));
                if (choices[i].icon != null) DrawSprite(new Rect(cell.x + 22, cell.y + 22, tile - 44, tile - 44), choices[i].icon);
                // names and short descriptions wrap inside the tile's column and never run under each other (long names take two lines)
                pickerName ??= new GUIStyle(label) { fontSize = 20, wordWrap = true, alignment = TextAnchor.UpperCenter };
                GUI.Label(new Rect(cell.x - 10, cell.y + tile + 2, tile + 20, 48), choices[i].name, pickerName);
                // what it does, in a line or two (requirement 25/10/2026: names alone told nothing)
                pickerWhat ??= new GUIStyle(label) { fontSize = 14, wordWrap = true, alignment = TextAnchor.UpperCenter, clipping = TextClipping.Overflow };
                pickerWhat.normal.textColor = new Color(.62f, .92f, .66f);
                GUI.Label(new Rect(cell.x - 10, cell.y + tile + 52, tile + 20, 64), choices[i].what, pickerWhat);
                if (GUI.Button(cell, GUIContent.none, GUIStyle.none)) picked = i;
            }
            GUI.matrix = m; GUI.color = c;
            if (picked >= 0)
            {
                if (relics) owner.SecretTarget(choices[picked].id); else owner.SecretTarget(choices[picked].id, choices[picked].weapon);
                picker = 0;
            }
            else if (Event.current.type == EventType.MouseUp && !panel.Contains(Event.current.mousePosition)) { picker = 0; Event.current.Use(); } // a tap outside closes it
        }
        private void DrawSprite(Rect rect, Sprite sprite)
        {
            Rect t = sprite.textureRect;
            GUI.DrawTextureWithTexCoords(rect, sprite.texture, new Rect(t.x / sprite.texture.width, t.y / sprite.texture.height, t.width / sprite.texture.width, t.height / sprite.texture.height), true);
        }
        private void OnDestroy()
        {
            ShopFocus.Unregister(this);
            foreach (SpriteRenderer p in portals) if (p != null) Destroy(p.gameObject);
            if (gallery != null) Destroy(gallery);
            foreach (Texture2D t in new[] { panel, paper, disabled }) if (t != null) Destroy(t);
        }
    }
}
