using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(-30)]
    public sealed class SecretEventRoom : MonoBehaviour
    {
        private ArenaController owner;
        private readonly List<SpriteRenderer> portals = new List<SpriteRenderer>();
        private readonly List<SecretEventKind> kinds = new List<SecretEventKind>();
        private GameObject gallery;
        private SpriteRenderer centerpiece;
        private string signature;
        private float hold, enteredAt;
        private int selected;
        private bool armed;
        private GUIStyle heading, body, small, card, label, primary;
        private Texture2D panel, paper, disabled;
        private Vector2 journalScroll;
        private static readonly Vector2[] Positions = { new Vector2(-6.3f, 1.3f), new Vector2(-6.3f, -3.5f) };
        public void Configure(ArenaController controller) { owner = controller; }
        public bool InRoom => owner?.Session?.SecretRoomOpen == true;
        public bool Focused => !InRoom && NearestPortal() >= 0;
        public bool BlocksShop => InRoom || Focused;
        public bool MenuOpen => InRoom && owner.Session.State.Phase == SessionPhase.Shop;
        private int NearestPortal()
        {
            if (owner?.Simulation == null || owner.Session == null || owner.Session.State.Phase != SessionPhase.Shop || owner.TransitionPending) return -1;
            for (int i = 0; i < portals.Count; i++) if (Vector2.Distance(owner.Simulation.Champion.Position, Positions[i]) < 1.5f) return i;
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
                hold = near >= 0 && pressed && armed ? hold + Time.deltaTime : 0;
                if (hold >= .85f) { armed = false; owner.EnterSecret(kinds[near]); hold = 0; }
            }
            else if (MenuOpen)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton4)) { selected = (selected + 2) % 3; hold = 0; }
                if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton5)) { selected = (selected + 1) % 3; hold = 0; }
                for (int i = 0; i < 3; i++) if (Input.GetKeyDown(KeyCode.Alpha1 + i)) { selected = i; hold = 0; }
                hold = pressed && armed ? hold + Time.deltaTime : 0;
                if (hold >= .85f)
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
                SpriteRenderer p = Prop(transform, "SecretInvitation_" + available[i], SecretEventAssets.Current.Portals[(int)available[i] * 2], Positions[i], 1.65f, 315);
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
                    ArcadeSkin.Panel(new Rect(400, 850, 1120, 148), ArcadeSkin.Surface);
                    GUI.Label(new Rect(420, 858, 1080, 44), SecretEventAssets.Titles[(int)kinds[near]], heading);
                    GUI.Label(new Rect(440, 906, 800, 64), SecretEventAssets.Hints[(int)kinds[near]] + "\nHold E / Space / A to discover  ·  " + Mathf.FloorToInt(hold / .85f * 100) + "%", label);
                    if (GUI.Button(new Rect(1260, 916, 230, 56), "DISCOVER", primary)) owner.EnterSecret(kinds[near]);
                }
            }
            else if (MenuOpen)
            {
                SecretEventProgress s = owner.Session.State.Secrets;
                ArcadeSkin.Panel(new Rect(250, 130, 1420, 150), ArcadeSkin.Surface);
                GUI.Label(new Rect(280, 142, 1360, 50), SecretEventAssets.Titles[(int)s.Kind], heading);
                GUI.Label(new Rect(300, 199, 1320, 66), NightmareDialogue.Room(owner.Session.State,s.Kind) + "\nRead each offer. The way back to your shop stays open until you accept a challenge.", label);
                if (s.Stage == SecretEventStage.Choosing)
                {
                    TargetSelector(s);
                    SecretChoice[] choices = owner.Session.SecretChoices();
                    for (int i = 0; i < 3; i++)
                    {
                        float x = 260 + i * 470; SecretChoice c = choices[i];
                        GUI.enabled = c.Enabled;
                        if (GUI.Button(new Rect(x, 540, 460, 70), (i + 1) + "  " + c.Title, card)) { selected = i; hold = 0; }
                        GUI.enabled = true;
                        ArcadeSkin.Panel(new Rect(x, 612, 460, 245), c.Enabled ? ArcadeSkin.Surface : new Color(.12f,.12f,.15f));
                        if (selected == i) ArcadeSkin.Focus(new Rect(x, 540, 460, 317));
                        GUI.Label(new Rect(x + 18, 626, 424, 210), c.Copy + (c.Enabled ? "" : "\n\nUnavailable with your current build or active contract."), body);
                        if (c.Icon != null) DrawSprite(new Rect(x + 175, 450, 90, 80), c.Icon);
                    }
                    ArcadeSkin.Panel(new Rect(490, 862, 940, 44), ArcadeSkin.Surface);
                    GUI.Label(new Rect(500, 866, 920, 36), "1–3 / ← → / LB RB: select  ·  Hold E / Enter / A to agree: " + Mathf.FloorToInt(hold / .85f * 100) + "%", label);
                    GUI.enabled = choices[selected].Enabled;
                    if (GUI.Button(new Rect(500, 920, 440, 58), "AGREE TO SELECTED OFFER", primary)) owner.ChooseSecret(selected);
                    GUI.enabled = true;
                    if (GUI.Button(new Rect(960, 920, 460, 58), "RETURN WITHOUT A DEAL", card)) owner.LeaveSecret();
                }
                else
                {
                    ArcadeSkin.Panel(new Rect(420, 540, 1080, 315), ArcadeSkin.Surface);
                    GUI.Label(new Rect(455, 562, 1010, 160), s.Result, body);
                    if (!string.IsNullOrEmpty(s.RewardId))
                    {
                        string reward = owner.Content.Item(s.RewardId)?.Name ?? owner.Content.Unit(s.RewardId)?.DisplayName;
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
            foreach (SpriteRenderer p in portals) if (p != null) Destroy(p.gameObject);
            if (gallery != null) Destroy(gallery);
            foreach (Texture2D t in new[] { panel, paper, disabled }) if (t != null) Destroy(t);
        }
    }
}
