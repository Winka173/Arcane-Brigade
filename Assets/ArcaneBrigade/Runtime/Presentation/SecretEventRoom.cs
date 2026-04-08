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
        public void Configure(ArenaController controller) { owner = controller; ShopFocus.Register(this); }
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
        public bool MenuOpen => InRoom && owner.Session.State.Phase == SessionPhase.Shop;
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
            if (signature != key) { signature = key; Rebuild(); enteredAt = owner.PresentationClock; selected = 0; hold = 0; armed = false; }
            if (owner.PresentationPaused || owner.TransitionPending || owner.HallExitPending) { hold = 0; return; }
            bool pressed = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.JoystickButton0);
            bool confirm = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0);
            if (!pressed) armed = true;
            if (!InRoom)
            {
                int near = NearestPortal();
                for (int i = 0; i < portals.Count; i++)
                {
                    portals[i].sprite = SecretEventAssets.Current.Portals[(int)kinds[i] * 2 + (i == near ? 1 : 0)];
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
                    if (s.Stage == SecretEventStage.Choosing) owner.ChooseSecret(selected);
                    else if (!string.IsNullOrEmpty(s.RewardId)) owner.ClaimSecret(); else owner.LeaveSecret();
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
                    centerpiece = Prop(gallery.transform, "OpenedSecretDoor", SecretEventAssets.Current.Portals[(int)owner.Session.State.Secrets.Kind * 2 + 1], new Vector2(0, -.15f), 3.25f, 310);
                    Prop(gallery.transform, "LeftArtifact", SecretEventAssets.Current.Relics[(int)owner.Session.State.Secrets.Kind], new Vector2(-3, 1.2f), .85f, 310);
                    Prop(gallery.transform, "RightArtifact", SecretEventAssets.Current.Relics[(int)owner.Session.State.Secrets.Kind + 8], new Vector2(3, 1.2f), .85f, 310);
                }
                return;
            }
            SecretEventKind[] available = owner.Session.SecretPortals();
            for (int i = 0; i < available.Length; i++)
            {
                kinds.Add(available[i]);
                SpriteRenderer p = Prop(transform, "SecretInvitation_" + available[i], SecretEventAssets.Current.Portals[(int)available[i] * 2], Positions[i], 1.4f, 315); // 1.4 tall: the widest (the train) fits between the wallet and the merchant
                portals.Add(p);
                var glow = new GameObject("PortalGlow"); glow.transform.SetParent(p.transform, false); glow.AddComponent<ShopRarityGlow>().Configure(ItemRarity.Rare, owner, i * 2, p, false);
            }
        }
        private SpriteRenderer Prop(Transform parent, string name, Sprite sprite, Vector2 at, float height, int order)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false); obj.transform.localPosition = at;
            SpriteRenderer r = obj.AddComponent<SpriteRenderer>(); r.sprite = sprite; r.sortingOrder = order;
            obj.transform.localScale = Vector3.one * (height / Mathf.Max(.1f, sprite.bounds.size.y));
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
        private void OnGUI()
        {
            if (owner?.Session == null || owner.PresentationPaused || owner.TransitionPending || SecretEventAssets.Current == null) return;
            Styles(); Matrix4x4 previous = GUI.matrix; int depth = GUI.depth; GUI.depth = -30;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            if (!InRoom)
            {
                int near = NearestPortal();
                if (near >= 0)
                {
                    ArcadeSkin.Frame(new Rect(400, 850, 1120, 148), KitFrame.Dialogue, .6f);
                    GUI.Label(new Rect(420, 858, 1080, 44), SecretEventAssets.Titles[(int)kinds[near]], heading);
                    GUI.Label(new Rect(440, 906, 800, 64), SecretEventAssets.Hints[(int)kinds[near]] + "", label);
                    if (GUI.Button(new Rect(1260, 916, 230, 56), "DISCOVER", primary)) owner.EnterSecret(kinds[near]);
                }
            }
            else if (MenuOpen)
            {
                SecretEventProgress s = owner.Session.State.Secrets;
                // the room as plates (play-test 20/10/2026, item 7): the name and one line, your relic in the middle (tap to swap),
                // three DO / LOSE / GET plates, one confirm button and a small way back
                // drawn in code (21/10/2026): the room's painted vignette when it has one, a title banner, your relic on a
                // pedestal, three walnut choice plates with cream DO, oxblood LOSE and green-gilt GET rows
                Sprite vignette = SecretEventVignettes.For(s.Kind);
                float bannerY = 124;
                if (vignette != null)
                {
                    float vh = 150, vw = vh * vignette.rect.width / vignette.rect.height;
                    ArcadeSkin.Glow(new Rect(960 - vw * .75f, 0, vw * 1.5f, vh + 24), new Color(1f, .8f, .5f, .16f));
                    DrawSprite(new Rect(960 - vw * .5f, 4, vw, vh), vignette);
                    bannerY = 148;
                }
                ArcadeSkin.TitleBanner(new Rect(560, bannerY, 800, 64), SecretEventAssets.Titles[(int)s.Kind], heading);
                GUI.Label(new Rect(300, bannerY + 66, 1320, 32), NightmareDialogue.Room(owner.Session.State, s.Kind), label);
                if (s.Stage == SecretEventStage.Choosing)
                {
                    bool target = owner.Session.SecretUsesTarget;
                    if (target) TargetRelic(s);
                    SecretChoice[] choices = owner.Session.SecretChoices();
                    if (!choices[selected].Enabled) for (int i = 0; i < 3; i++) if (choices[i].Enabled) { selected = i; break; }
                    float top = target ? 470 : 300, height = 360;
                    Sprite relic = owner.Content.Item(s.TargetItem)?.Icon;
                    for (int i = 0; i < 3; i++)
                    {
                        float x = 260 + i * 470; SecretChoice c = choices[i];
                        Rect plate = new Rect(x, top, 460, height);
                        bool lit = selected == i && c.Enabled;
                        ArcadeSkin.CodePlate(plate, CodeTone.Walnut, !c.Enabled ? 3 : lit ? 1 : 0);
                        if (lit) ArcadeSkin.RunningLight(new Rect(plate.x - 5, plate.y - 5, plate.width + 10, plate.height + 10), Time.unscaledTime, new Color(1f, .82f, .45f));
                        Color keep = GUI.color; if (!c.Enabled) GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * .55f);
                        Rect pc = new Rect(plate.x + 18, plate.y + 14, plate.width - 36, plate.height - 28);
                        if (plateTitle == null) Row(Rect.zero, null, null, null, CodeTone.Cream);
                        GUI.Label(new Rect(pc.x, pc.y, pc.width, 38), c.Title, plateTitle);
                        Row(new Rect(pc.x, pc.y + 42, pc.width, 76), "DO", c.Do, null, CodeTone.Cream);
                        Row(new Rect(pc.x, pc.y + 124, pc.width, 96), "LOSE", c.Lose, c.UsesTarget ? relic : null, CodeTone.Oxblood);
                        Row(new Rect(pc.x, pc.y + 226, pc.width, 96), "GET", c.Get, c.Icon, CodeTone.Green);
                        GUI.color = keep;
                        if (!c.Enabled) GUI.Label(new Rect(x, top + height + 4, 460, 30), "Not available now", label);
                        else if (GUI.Button(plate, GUIContent.none, GUIStyle.none)) { selected = i; hold = 0; }
                    }
                    GUI.enabled = choices[selected].Enabled;
                    if (GUI.Button(new Rect(740, 880, 440, 84), owner.Session.SecretVerb, primary)) owner.ChooseSecret(selected);
                    GUI.enabled = true;
                    if (GUI.Button(new Rect(1250, 892, 300, 64), "Back to the shop", card)) owner.LeaveSecret();
                }
                else
                {
                    ArcadeSkin.Frame(new Rect(420, 540, 1080, 315), KitFrame.Dialogue, .6f);
                    body.wordWrap = true; float resultHeight = body.CalcHeight(new GUIContent(s.Result), 990);
                    resultScroll = ScrollPanel.Begin(new Rect(455, 562, 1010, 170), resultScroll, new Rect(0, 0, 990, resultHeight));
                    GUI.Label(new Rect(0, 0, 990, resultHeight), s.Result, body);
                    ScrollPanel.End();
                    if (!string.IsNullOrEmpty(s.RewardId))
                    {
                        string reward = owner.Session.SecretRewardLabel;
                        if (GUI.Button(new Rect(640, 745, 640, 68), "CLAIM " + reward, card)) owner.ClaimSecret();
                    }
                    else if (GUI.Button(new Rect(660, 745, 600, 68), "RETURN TO THE SHOP", card)) owner.LeaveSecret();
                }
            }
            else if (owner.Session.SecretFighting)
            {
                GUI.Label(new Rect(24, 1038, 1000, 30), SecretEventAssets.Titles[(int)owner.Session.State.Secrets.Kind], small);
            }
            GUI.matrix = previous; GUI.depth = depth;
        }
        private GUIStyle plateTitle, tagStyle;
        /// <summary>Your relic in the middle of the room: its picture and name; a tap shows the next one you own.</summary>
        private void TargetRelic(SecretEventProgress s)
        {
            Rect box = new Rect(810, 240, 300, 215);
            Rect stand = new Rect(850, 262, 220, 160);
            ArcadeSkin.Pedestal(stand, Time.unscaledTime); // velvet cushion on a brass stand, drawn in code
            ItemDefinition relic = owner.Content.Item(s.TargetItem);
            if (relic?.Icon != null) DrawSprite(ArcadeSkin.PedestalSeat(stand), relic.Icon);
            GUI.Label(new Rect(box.x - 40, 422, box.width + 80, 30), (relic?.Name ?? "No relic to offer") + (owner.Session.SecretTargets.Count > 1 ? "  ·  tap to swap" : ""), label);
            if (GUI.Button(box, GUIContent.none, GUIStyle.none)) CycleTarget(1);
            if (s.Kind == SecretEventKind.DeadLetters && owner.Session.State.Roster.Count > 0)
            {
                Rect friend = new Rect(1130, 300, 300, 100);
                ArcadeSkin.Plate(friend, ArcadeSkin.Accent);
                UnitDefinition unit = owner.Content.Unit(s.TargetUnit);
                if (unit?.Body != null) DrawSprite(new Rect(friend.x + 10, friend.y + 10, 80, 80), unit.Body);
                GUI.Label(new Rect(friend.x + 96, friend.y + 10, 196, 80), (unit != null ? RunUICopy.UnitName(unit) : "A friend") + "\ntap to swap", label);
                if (GUI.Button(friend, GUIContent.none, GUIStyle.none))
                {
                    var roster = owner.Session.State.Roster; int index = roster.FindIndex(r => r.UnitId == s.TargetUnit && r.WeaponId == s.TargetWeapon);
                    RosterEntry r = roster[(index + 1) % roster.Count]; owner.SecretTarget(r.UnitId, r.WeaponId);
                }
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
        private void TargetSelector(SecretEventProgress s)
        {
            if (s.Kind != SecretEventKind.ImpossibleTailor && s.Kind != SecretEventKind.DeadLetters && s.Kind != SecretEventKind.ThirteenthHour && s.Kind != SecretEventKind.Understudy && s.Kind != SecretEventKind.ContrabandTrain) return;
            string name = owner.Content.Item(s.TargetItem)?.Name ?? "No eligible relic owned";
            GUI.DrawTexture(new Rect(550, 310, 820, 72), panel);
            GUI.Label(new Rect(635, 325, 650, 40), "RELIC: " + name, label);
            if (GUI.Button(new Rect(555, 317, 65, 55), "<", card)) CycleTarget(-1);
            if (GUI.Button(new Rect(1300, 317, 65, 55), ">", card)) CycleTarget(1);
            if (s.Kind == SecretEventKind.DeadLetters && owner.Session.State.Roster.Count > 0)
            {
                if (GUI.Button(new Rect(550, 392, 820, 49), "FRIEND: " + owner.Content.Unit(s.TargetUnit)?.DisplayName + " / " + s.TargetWeapon + "  >", card))
                {
                    var roster = owner.Session.State.Roster; int index = roster.FindIndex(r => r.UnitId == s.TargetUnit && r.WeaponId == s.TargetWeapon);
                    RosterEntry r = roster[(index + 1) % roster.Count]; owner.SecretTarget(r.UnitId, r.WeaponId);
                }
            }
        }
        private void CycleTarget(int step)
        {
            string[] items = owner.Session.SecretTargets.ToArray(); if (items.Length == 0) return;
            int index = Array.IndexOf(items, owner.Session.State.Secrets.TargetItem);
            owner.SecretTarget(items[(index + step + items.Length) % items.Length]);
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
