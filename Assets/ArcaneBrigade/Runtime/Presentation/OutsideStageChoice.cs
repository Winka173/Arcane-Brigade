using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class OutsideStageChoice : MonoBehaviour
    {
        private ArenaController owner;
        private readonly List<GameObject> actors = new List<GameObject>();
        private readonly Vector2[] rewardPositions = { new Vector2(-3, -0.8f), new Vector2(0, -0.8f), new Vector2(3, -0.8f) };
        public static readonly Vector2 ChallengePosition = new Vector2(6.1f, 0.3f);
        public bool Focused => owner != null && owner.Session.OutsideChallengeAvailable && Vector2.Distance(owner.Simulation.Champion.Position, ChallengePosition) < 1.6f;
        private int mode;
        private float hold;
        private int selection = -1;
        private GUIStyle title;
        private GUIStyle copy;
        private Vector2 descriptionScroll;
        private int preview;
        public void Configure(ArenaController controller) { owner = controller; }
        private void Clear() { foreach (GameObject actor in actors) Destroy(actor); actors.Clear(); hold = 0; selection = -1; }
        private void Update()
        {
            if (owner == null || owner.Session == null) return;
            int desired = owner.Session.State.OutsideStage == OutsideStageStatus.Reward ? 2 : owner.Session.OutsideChallengeAvailable ? 1 : 0;
            if (desired != mode) { Clear(); mode = desired; Build(); }
            if (mode == 0 || owner.PresentationPaused || owner.TransitionPending) { hold = 0; return; }
            Vector2 player = owner.Simulation.Champion.Position;
            int near = -1;
            if (mode == 1) { if (Focused) near = 0; }
            else for (int index = 0; index < 3; index++) if (Vector2.Distance(player, rewardPositions[index]) < 1.05f) near = index;
            if (selection != near) { selection = near; hold = 0; if (near >= 0) { preview = near; descriptionScroll = Vector2.zero; } }
            bool pressed = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.JoystickButton0);
            hold = selection >= 0 && pressed ? hold + Time.deltaTime : 0;
            float time = owner.PresentationClock;
            for (int index = 0; index < actors.Count; index++)
            {
                Vector2 origin = mode == 1 ? ChallengePosition : rewardPositions[index];
                Vector3 hovering = DreamLevitation.Offset(time, 70 + index, NightmareRules.Depth(owner.Session.State.Wave), PresentationSettings.ReducedMotion);
                SpriteRenderer view = actors[index].GetComponent<SpriteRenderer>();
                actors[index].transform.rotation = Quaternion.Euler(0, 0, DreamLevitation.Tilt(time, 70 + index, PresentationSettings.ReducedMotion));
                WorldSpriteGrounding.Place(view, (Vector3)origin + hovering);
                actors[index].GetComponent<GroundShadow>().Display(view.sprite, origin, mode == 1 ? 2.2f : .8f, hovering.y, .28f);
                actors[index].GetComponent<DreamSuspension>().Display(origin, (Vector3)origin + hovering, mode == 1 ? 2.2f : .8f, time, 70 + index, view.sortingOrder - 1, 1);
            }
            if (hold < 0.85f) return;
            hold = 0;
            if (mode == 1) owner.AcceptOutsideChallenge(); else owner.ClaimOutsideReward(selection);
        }
        private void Build()
        {
            OutsideStageAssets assets = OutsideStageAssets.Current;
            if (mode == 0 || assets == null) return;
            for (int index = 0; index < (mode == 1 ? 1 : 3); index++)
            {
                var actor = new GameObject(mode == 1 ? "TheHandWaits" : "PrismaticReward" + index); actor.transform.SetParent(transform, false);
                SpriteRenderer sprite = actor.AddComponent<SpriteRenderer>();
                if (mode == 1) sprite.sprite = CarrierArtwork.Current != null ? CarrierArtwork.Current.Hand[0] : assets.BossFrames[4];
                else
                {
                    string id = owner.Session.State.OutsideRewards[index];
                    ItemDefinition item = owner.Content.Item(id); sprite.sprite = item == null ? owner.Content.Unit(id).Body : item.Icon;
                }
                sprite.sortingOrder = 950; actor.transform.localScale = Vector3.one * ((mode == 1 ? 2.5f : 1.1f) / Mathf.Max(0.01f, sprite.sprite.bounds.size.x));
                GroundShadow shadow = actor.AddComponent<GroundShadow>(); shadow.Display(sprite.sprite, mode == 1 ? ChallengePosition : rewardPositions[index], mode == 1 ? 2.2f : 0.8f, 0.5f, 0.22f);
                actor.AddComponent<DreamSuspension>();
                var glow = new GameObject("BoundPrismaticLight"); glow.transform.SetParent(actor.transform, false);
                glow.AddComponent<ShopRarityGlow>().Configure(ItemRarity.Rainbow, owner, index, sprite);
                actors.Add(actor);
            }
        }
        private void OnGUI()
        {
            if (mode == 0 || owner == null || owner.TransitionPending || owner.PresentationPaused) return;
            GUISkin previousSkin = GUI.skin; GUI.skin = ArcadeSkin.Skin;
            GUIStyle choiceButton = new GUIStyle(GUI.skin.button) { font = ArcadeTypography.Heading, fontSize = 22, wordWrap = true, padding = new RectOffset(16, 16, 8, 8) };
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { font = ArcadeTypography.Body, fontSize = 28, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                title.normal.textColor = new Color(0.68f, 1, 0.92f);
                copy = new GUIStyle(title) { fontSize = 19 }; copy.normal.textColor = Color.white;
            }
            Matrix4x4 matrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            if (mode == 1)
            {
                if (Focused && GUI.Button(new Rect(1430, 710, 340, 70), "CHALLENGE KEEPER")) owner.AcceptOutsideChallenge();
                GUI.Label(new Rect(340, 870, 1240, 42), owner.Session.State.Secrets?.LetterSent == true ? "YOUR LETTER REACHED THE HAND THAT CARRIED YOU" : "THE HAND THAT CARRIED YOU IS WAITING", title);
                GUI.Label(new Rect(340, 915, 1240, 70), selection >= 0 ? "Hold Space / A to challenge • Defeat ends this run • Win: choose one prismatic reward" : "Approach the hand to challenge its owner. The normal exit remains available.", copy);
            }
            else
            {
                GUI.Label(new Rect(340, 180, 1240, 55), "TAKE ONE IMPOSSIBLE GIFT", title);
                for (int i = 0; i < 3; i++)
                {
                    string rewardId = owner.Session.State.OutsideRewards[i];
                    string rewardName = owner.Content.Item(rewardId)?.Name ?? owner.Content.Unit(rewardId).DisplayName;
                    if (GUI.Button(new Rect(340 + i * 420, 725, 400, 62), rewardName, choiceButton)) { preview = i; descriptionScroll = Vector2.zero; }
                }
                int shown = preview;
                string id = owner.Session.State.OutsideRewards[shown]; ItemDefinition item = owner.Content.Item(id); UnitDefinition ally = owner.Content.Unit(id);
                Rect panel = new Rect(370, 805, 1180, 205);
                ArcadeSkin.Ticket(panel, new Color(.11f, .09f, .19f, .98f), RunModifierRules.RarityColor(ItemRarity.Rainbow));
                GUI.Label(new Rect(395, 815, 1130, 36), item?.Name ?? ally.DisplayName, title);
                string detail = item == null ? CompanionDescriptions.Describe(ally) + "\nWeapon rolls when claimed." : RunModifierRules.Describe(item, 1);
                bool full = item == null && owner.Session.State.Roster.Count >= owner.Session.CompanionLimit;
                if (full) detail = "BRIGADE FULL / 15 COMPANIONS. Choose a relic gift instead.\n\n" + detail;
                GUIStyle detailStyle = new GUIStyle(copy) { alignment = TextAnchor.UpperLeft, fontSize = 19 };
                float height = Mathf.Max(90, detailStyle.CalcHeight(new GUIContent(detail), 1080));
                descriptionScroll = GUI.BeginScrollView(new Rect(395, 860, 1125, 90), descriptionScroll, new Rect(0, 0, 1080, height));
                GUI.Label(new Rect(0, 0, 1080, height), detail, detailStyle); GUI.EndScrollView();
                bool previousEnabled = GUI.enabled; GUI.enabled = previousEnabled && !full;
                if (GUI.Button(new Rect(1040, 958, 470, 42), full ? "BRIGADE FULL / CHOOSE A RELIC" : "CLAIM THIS GIFT", choiceButton)) owner.ClaimOutsideReward(shown);
                GUI.enabled = previousEnabled;
                GUI.Label(new Rect(395, 960, 620, 40), "Choose one. The other two gifts vanish.", copy);
            }
            if (selection >= 0 && hold > 0) GUI.Label(new Rect(760, 1020, 400, 35), Mathf.RoundToInt(Mathf.Clamp01(hold / 0.85f) * 100) + "%", copy);
            GUI.matrix = matrix;
            GUI.skin = previousSkin;
        }
        private void OnDestroy() => Clear();
    }
}
