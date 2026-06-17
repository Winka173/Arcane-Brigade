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
        // requirement 24/10/2026: after the deal the way back is a rift you walk into, not a button
        public static readonly Vector2 ReturnSpot = new Vector2(0, -3.4f);
        private const float ReturnReach = .9f;
        private bool walkingOut;
        private Transform returnRift, returnSwirl;
        private float returnTurn;
        private void BeginWalkOut()
        {
            if (walkingOut || gallery == null) return;
            walkingOut = true;
            MapTheme theme = MapIdentity.For(owner.Session.State.Wave);
            var art = OctoberDoor.PortalArt(theme);
            if (art.body == null) { walkingOut = false; owner.LeaveSecret(); return; }
            var root = new GameObject("ReturnRift"); root.transform.SetParent(gallery.transform, false); root.transform.localPosition = ReturnSpot;
            var body = new GameObject("Rim").AddComponent<SpriteRenderer>(); body.transform.SetParent(root.transform, false);
            body.sprite = art.body; body.sortingOrder = 312;
            body.transform.localScale = Vector3.one * (2f / Mathf.Max(.01f, art.body.bounds.size.y));
            Bounds b = art.body.bounds; Vector4 hole = OctoberDoor.PortalHoles[theme];
            var holder = new GameObject("Swirl holder").transform; holder.SetParent(body.transform, false);
            holder.localPosition = new Vector3(b.min.x + b.size.x * hole.x, b.min.y + b.size.y * hole.y, 0);
            holder.localScale = new Vector3(b.size.x * hole.z * 2.12f / art.swirl.bounds.size.x, b.size.y * hole.w * 2.12f / art.swirl.bounds.size.y, 1);
            var swirl = new GameObject("Swirl").AddComponent<SpriteRenderer>(); swirl.transform.SetParent(holder, false);
            swirl.sprite = art.swirl; swirl.sortingOrder = 311;
            returnRift = root.transform; returnSwirl = swirl.transform; returnTurn = 0;
            root.transform.localScale = new Vector3(.15f, .02f, 1); // it tears open (WalkOut grows it)
        }
        /// <summary>The return rift turns and opens wider as the hero nears; stepping into it leaves the room.</summary>
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
            if (Vector2.Distance(hero, ReturnSpot) <= ReturnReach && returnRift.localScale.y > .9f) { walkingOut = false; returnRift = null; owner.LeaveSecret(); }
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
            if (signature != key) { signature = key; walkingOut = false; picker = 0; returnRift = null; Rebuild(); enteredAt = owner.PresentationClock; selected = 0; hold = 0; armed = false; }
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
                if (near >= 0 && confirm && armed && owner.PresentationClock - enteredAt > .2f) { armed = false; owner.EnterSecret(kinds[near]); }
            }
            else if (MenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton4)) { selected = (selected + 2) % 3; hold = 0; }
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton5)) { selected = (selected + 1) % 3; hold = 0; }
                for (int i = 0; i < 3; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { selected = i; hold = 0; }
                if (confirm && armed && owner.PresentationClock - enteredAt > .2f)
                {
                    armed = false; hold = 0;
                    int pick = selected;
                    if (s.Stage == SecretEventStage.Choosing) Close(() => owner.ChooseSecret(pick));
                    else if (!string.IsNullOrEmpty(s.RewardId)) owner.ClaimSecret(); else Close(BeginWalkOut);
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
            body = new GUIStyle(heading) { font = ArcadeTypography.Body, fontSize = 19, alignment = TextAnchor.UpperLeft }; body.normal.textColor = new Color(.95f, .92f, .84f);
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
                    if (ShopPrompt.Draw(SecretEventAssets.Titles[(int)kinds[near]], SecretEventAssets.Plain[(int)kinds[near]], "DISCOVER", Time.unscaledTime - hintAt)) owner.EnterSecret(kinds[near]);
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
                    GUI.Label(new Rect(300, bannerY + 66, 1320, 32), SecretEventAssets.Plain[(int)s.Kind], label); // what the room is for
                });
                if (s.Stage == SecretEventStage.Choosing)
                {
                    bool target = owner.Session.SecretUsesTarget;
                    if (target) Popped(.08f, new Vector2(960, 350), () => TargetRelic(s));
                    SecretChoice[] choices = owner.Session.SecretChoices();
                    if (!choices[selected].Enabled) for (int i = 0; i < 3; i++) if (choices[i].Enabled) { selected = i; break; }
                    float top = target ? 470 : 300, height = 360;
                    Sprite relic = owner.Content.Item(s.TargetItem)?.Icon;
                    for (int i = 0; i < 3; i++)
                    {
                        float x = 260 + i * 470; SecretChoice c = choices[i];
                        Rect plate = new Rect(x, top, 460, height);
                        Matrix4x4 plateMatrix = GUI.matrix; Color plateColour = GUI.color;
                        LivingUI.Pop(Time.unscaledTime - roomAt - .12f - i * .06f, plate.center); // the three plates land one after another
                        bool lit = selected == i && c.Enabled;
                        bool creamPlate = ArcadeSkin.EventPlate(plate, lit, c.Enabled); // the painted cream plate, else the walnut code plate
                        if (!creamPlate) ArcadeSkin.CodePlate(plate, CodeTone.Walnut, !c.Enabled ? 3 : lit ? 1 : 0);
                        if (lit) ArcadeSkin.RunningLight(new Rect(plate.x - 5, plate.y - 5, plate.width + 10, plate.height + 10), Time.unscaledTime, new Color(1f, .82f, .45f));
                        Color keep = GUI.color; if (!c.Enabled) GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * .55f);
                        Rect pc = creamPlate ? ArcadeSkin.EventContent(plate) : new Rect(plate.x + 18, plate.y + 14, plate.width - 36, plate.height - 28);
                        if (plateTitle == null) Row(Rect.zero, null, null, null, CodeTone.Cream);
                        Color titleTone = plateTitle.normal.textColor; if (creamPlate) plateTitle.normal.textColor = ArcadeSkin.KitInk; // ink on the cream plate
                        GUI.Label(new Rect(pc.x, pc.y, pc.width, 38), c.Title, plateTitle);
                        plateTitle.normal.textColor = titleTone;
                        float bandH = creamPlate ? 84 : 96; // the painted plate's rim takes a little height
                        Row(new Rect(pc.x, pc.y + 40, pc.width, creamPlate ? 64 : 76), "DO", c.Do, null, CodeTone.Cream);
                        Row(new Rect(pc.x, pc.y + (creamPlate ? 108 : 124), pc.width, bandH), "LOSE", c.Lose, null, CodeTone.Oxblood);
                        Row(new Rect(pc.x, pc.y + (creamPlate ? 198 : 226), pc.width, bandH), "GET", c.Get, null, CodeTone.Green);
                        // what you get sits in the plate's round socket (it stood empty); a deal with no relic shows the room's own
                        if (creamPlate) DrawSprite(new Rect(plate.center.x - SocketSize * .5f, plate.y + SocketY - SocketSize * .5f, SocketSize, SocketSize), c.Icon != null ? c.Icon : SecretArt.Relic(s.Kind));
                        GUI.color = keep;
                        if (!c.Enabled) GUI.Label(new Rect(x, top + height + 4, 460, 30), "Not available now", label);
                        else if (!shut && GUI.Button(plate, GUIContent.none, GUIStyle.none)) { selected = i; hold = 0; }
                        GUI.matrix = plateMatrix; GUI.color = plateColour;
                    }
                    Popped(.3f, new Vector2(960, 922), () =>
                    {
                        GUI.enabled = choices[selected].Enabled;
                        int pick = selected;
                        if (ArcadeSkin.RibbonButton(new Rect(740, 880, 440, 84), owner.Session.SecretVerb, GUI.enabled, primary) && !shut) Close(() => owner.ChooseSecret(pick)); // the wax-sealed contract ribbon
                        GUI.enabled = true;
                        if (ArcadeSkin.DoorTagButton(new Rect(1250, 892, 300, 64), "Back to the shop", card) && !shut) Close(() => owner.LeaveSecret());
                    });
                    DrawPicker(s); // over everything in the room
                }
                else
                {
                    if (resultShown != s.Result + "/" + s.RewardId) { resultShown = s.Result + "/" + s.RewardId; resultAt = Time.unscaledTime; }
                    LivingUI.Pop(Time.unscaledTime - resultAt, new Vector2(960, 680));
                    bool claim = !string.IsNullOrEmpty(s.RewardId);
                    if (!claim && string.IsNullOrEmpty(s.ResultGot)) LegacyResult(s, shut);
                    else
                    {
                        // the deal as a card (play-test 24/10/2026: plain lines did not say what was bought): its picture, GET with
                        // what it does, LOSE with what it cost, one button
                        Rect frame = new Rect(440, 470, 1040, 430);
                        ArcadeSkin.Frame(frame, KitFrame.Dialogue, .6f);
                        string[] lines = s.Result.Split('\n');
                        GUI.Label(new Rect(frame.x + 40, frame.y + 24, frame.width - 80, 44), lines[0], heading);
                        string iconId = claim ? s.RewardId : s.ResultIcon;
                        Sprite icon = owner.Content.Item(iconId)?.Icon ?? owner.Content.Unit(iconId)?.Body;
                        float x0 = frame.x + 50;
                        if (icon != null)
                        {
                            ArcadeSkin.Glow(new Rect(x0 - 30, frame.y + 70, 220, 220), new Color(1f, .82f, .5f, .3f));
                            DrawSprite(new Rect(x0, frame.y + 100, 160, 160), icon); x0 += 196;
                        }
                        float w = frame.xMax - 50 - x0;
                        string got = claim ? (owner.Content.Item(s.RewardId) != null ? "New relic" : owner.Session.SecretRewardLabel) + "\n" + owner.Session.SecretRewardStats(s.RewardId) : s.ResultGot;
                        Row(new Rect(x0, frame.y + 84, w, 100), "GET", got, null, CodeTone.Green);
                        if (claim && lines.Length > 1) GUI.Label(new Rect(x0, frame.y + 196, w, 70), lines[1], label);
                        else if (!string.IsNullOrEmpty(s.ResultPaid) && s.ResultPaid != "Nothing") Row(new Rect(x0, frame.y + 196, w, 84), "LOSE", s.ResultPaid, null, CodeTone.Oxblood);
                        Rect go = new Rect(frame.center.x - 300, frame.yMax - 100, 600, 68);
                        if (claim) { if (ArcadeSkin.KitGuiButton(go, "CLAIM", primary) && !shut) owner.ClaimSecret(); }
                        else if (ArcadeSkin.KitGuiButton(go, "OK", primary) && !shut) Close(BeginWalkOut); // then walk into the rift
                    }
                }
            }
            else if (owner.Session.SecretFighting)
            {
                GUI.Label(new Rect(24, 1038, 1000, 30), SecretEventAssets.Titles[(int)owner.Session.State.Secrets.Kind], small);
            }
            GUI.matrix = previous; GUI.depth = depth; GUI.color = Color.white;
        }
        private GUIStyle plateTitle, tagStyle;
        /// <summary>Where the painted plate's round badge socket sits (its centre, measured from the plate's top) and its size.</summary>
        private const float SocketY = 8, SocketSize = 54;
        /// <summary>A result saved before deals kept their parts: its lines as they were written.</summary>
        private void LegacyResult(SecretEventProgress s, bool shut)
        {
            ArcadeSkin.Frame(new Rect(420, 540, 1080, 315), KitFrame.Dialogue, .6f);
            body.wordWrap = true; float resultHeight = body.CalcHeight(new GUIContent(s.Result), 990);
            resultScroll = ScrollPanel.Begin(new Rect(455, 562, 1010, 170), resultScroll, new Rect(0, 0, 990, resultHeight));
            GUI.Label(new Rect(0, 0, 990, resultHeight), s.Result, body);
            ScrollPanel.End();
            if (ArcadeSkin.KitGuiButton(new Rect(660, 745, 600, 68), "OK", primary) && !shut) Close(BeginWalkOut);
        }
        /// <summary>Your relic in the middle of the room: its picture and name; a tap shows the next one you own.</summary>
        private void TargetRelic(SecretEventProgress s)
        {
            Rect box = new Rect(810, 240, 300, 215);
            Rect stand = new Rect(850, 262, 220, 160);
            ArcadeSkin.Pedestal(stand, Time.unscaledTime); // velvet cushion on a brass stand, drawn in code
            ItemDefinition relic = owner.Content.Item(s.TargetItem);
            if (relic?.Icon != null) DrawSprite(ArcadeSkin.PedestalSeat(stand), relic.Icon);
            GUI.Label(new Rect(box.x - 40, 422, box.width + 80, 30), (relic?.Name ?? "No relic to offer") + (owner.Session.SecretTargets.Count > 1 ? "  ·  ⇄" : ""), label);
            if (!blocked && owner.Session.SecretTargets.Count > 1 && GUI.Button(box, GUIContent.none, GUIStyle.none)) OpenPicker(1);
            if (s.Kind == SecretEventKind.DeadLetters && owner.Session.State.Roster.Count > 0)
            {
                Rect friend = new Rect(1130, 300, 300, 100);
                ArcadeSkin.Plate(friend, ArcadeSkin.Accent);
                UnitDefinition unit = owner.Content.Unit(s.TargetUnit);
                if (unit?.Body != null) DrawSprite(new Rect(friend.x + 10, friend.y + 10, 80, 80), unit.Body);
                GUI.Label(new Rect(friend.x + 96, friend.y + 10, 196, 80), (unit != null ? RunUICopy.UnitName(unit) : "A friend") + "  ⇄", label);
                if (!blocked && owner.Session.State.Roster.Count > 1 && GUI.Button(friend, GUIContent.none, GUIStyle.none)) OpenPicker(2);
            }
        }
        /// <summary>One LOSE (red) or GET (green) row: a coloured tag, a picture when there is one, one short line.</summary>
        private void Row(Rect r, string tag, string text, Sprite icon, CodeTone tone)
        {
            if (plateTitle == null) { plateTitle = new GUIStyle(heading) { fontSize = 26 }; tagStyle = new GUIStyle(label) { fontSize = 16, fontStyle = FontStyle.Bold }; tagStyle.normal.textColor = Color.white; }
            if (tag == null) return;
            Rect area = ArcadeSkin.EventRow(r, tag, tone, tagStyle);
            if (icon != null) DrawSprite(new Rect(r.x + 18, r.y + 38, Mathf.Min(48, r.height - 44), Mathf.Min(48, r.height - 44)), icon);
            Color keep = body.normal.textColor;
            body.normal.textColor = tone == CodeTone.Cream ? ArcadeSkin.KitInk : tone == CodeTone.Green ? new Color(.86f, 1f, .86f) : new Color(1f, .86f, .82f);
            body.wordWrap = true; GUI.Label(area, text, body);
            body.normal.textColor = keep;
        }
        // requirement 24/10/2026 (no tap-to-cycle pickers): the relic or friend you offer is chosen from a popup of all of them
        private int picker; // 0 closed, 1 relics, 2 friends
        private float pickerAt;
        private bool blocked; // a popup or the closing room owns the input this frame
        private GUIStyle pickerWhat;
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
            float w = columns * tile + (columns - 1) * gap + 80, h = rows * (tile + 80) + (rows - 1) * gap + 130;
            Rect panel = new Rect(960 - w * .5f, 540 - h * .5f, w, h);
            Matrix4x4 m = GUI.matrix; Color c = GUI.color;
            LivingUI.Pop(Time.unscaledTime - pickerAt, panel.center);
            ArcadeSkin.Frame(panel, KitFrame.Dialogue, .6f);
            GUI.Label(new Rect(panel.x, panel.y + 22, panel.width, 40), relics ? "CHOOSE A RELIC" : "CHOOSE A FRIEND", heading);
            int picked = -1;
            for (int i = 0; i < choices.Count; i++)
            {
                Rect cell = new Rect(panel.x + 40 + (i % 5) * (tile + gap), panel.y + 80 + (i / 5) * (tile + 80 + gap), tile, tile + 80);
                ArcadeSkin.CodePlate(new Rect(cell.x, cell.y, tile, tile), CodeTone.Walnut, choices[i].chosen ? 1 : 0);
                if (choices[i].chosen) ArcadeSkin.RunningLight(new Rect(cell.x - 4, cell.y - 4, tile + 8, tile + 8), Time.unscaledTime, new Color(1f, .82f, .45f));
                if (choices[i].icon != null) DrawSprite(new Rect(cell.x + 22, cell.y + 22, tile - 44, tile - 44), choices[i].icon);
                GUI.Label(new Rect(cell.x - 6, cell.y + tile + 2, tile + 12, 30), choices[i].name, label);
                // what it does, in a line or two (requirement 25/10/2026: names alone told nothing)
                pickerWhat ??= new GUIStyle(label) { fontSize = 14, wordWrap = true, alignment = TextAnchor.UpperCenter };
                pickerWhat.normal.textColor = new Color(.62f, .92f, .66f);
                GUI.Label(new Rect(cell.x - 10, cell.y + tile + 30, tile + 20, 48), choices[i].what, pickerWhat);
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
