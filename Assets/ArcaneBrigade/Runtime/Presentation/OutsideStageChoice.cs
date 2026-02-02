using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class OutsideStageChoice : MonoBehaviour
    {
        private ArenaController owner;
        private readonly List<GameObject> actors = new List<GameObject>();
        private readonly Vector2[] rewardPositions = { new Vector2(-3, -0.8f), new Vector2(0, -0.8f), new Vector2(3, -0.8f) };
        public static readonly Vector2 ChallengePosition = new Vector2(5.2f, -3.3f);
        public bool Focused => owner != null && owner.Session.OutsideChallengeAvailable && Vector2.Distance(owner.Simulation.Champion.Position, ChallengePosition) < 1.6f;
        private int mode;
        private int selection = -1;
        private GUIStyle title;
        private GUIStyle copy;
        private Vector2 descriptionScroll;
        private int preview;
        public void Configure(ArenaController controller) { owner = controller; }
        private void Clear() { foreach (GameObject actor in actors) Destroy(actor); actors.Clear(); selection = -1; }
        private void Update()
        {
            if (owner == null || owner.Session == null) return;
            int desired = owner.Session.State.OutsideStage == OutsideStageStatus.Reward ? 2 : owner.Session.OutsideChallengeAvailable ? 1 : 0;
            if (desired != mode) { Clear(); mode = desired; Build(); }
            if (mode == 0 || owner.PresentationPaused || owner.TransitionPending || owner.StoryConsumesInput || owner.Details.Open) return;
            Vector2 player = owner.Simulation.Champion.Position;
            int near = -1;
            if (mode == 1) { if (Focused) near = 0; }
            else for (int index = 0; index < 3; index++) if (Vector2.Distance(player, rewardPositions[index]) < 1.05f) near = index;
            if (selection != near) { selection = near; if (near >= 0) { preview = near; descriptionScroll = Vector2.zero; } }
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
            if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.JoystickButton0))Interact();
        }
        public bool Interact()
        {
            if(selection<0 || owner.PresentationPaused || owner.TransitionPending || owner.StoryConsumesInput || owner.Details.Open)return false;
            bool success=mode==1?owner.AcceptOutsideChallenge():mode==2 && owner.ClaimOutsideReward(selection);
            if(success){Clear();mode=0;}
            return success;
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
                Vector3 point=Camera.main.WorldToViewportPoint(ChallengePosition);
                Rect badge=new Rect(Mathf.Clamp(point.x*1920-185,30,1520),Mathf.Clamp((1-point.y)*1080+16,190,898),370,100);
                RarityDescriptionFrame.Draw(badge,ItemRarity.Rainbow,owner.PresentationClock);
                GUI.Label(new Rect(badge.x+8,badge.y+5,354,32),"OPTIONAL BOSS / THE KEEPER",copy);
                GUI.Label(new Rect(badge.x+8,badge.y+37,354,55),Focused?"Space / A / Click to challenge\nDefeat ends the run":"Approach the glowing hand\nWin a rainbow companion or relic",copy);
                if(Focused && GUI.Button(badge,GUIContent.none,GUIStyle.none))Interact();
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
                RarityDescriptionFrame.Draw(panel,ItemRarity.Rainbow,owner.PresentationClock);
                GUI.Label(new Rect(395, 815, 1130, 36), item?.Name ?? ally.DisplayName, title);
                string detail = item == null ? CompanionDescriptions.Describe(ally) + "\nWeapon rolls when claimed." : RunModifierRules.Describe(item, 1);
                GUIStyle detailStyle = new GUIStyle(copy) { alignment = TextAnchor.UpperLeft, fontSize = 19 };
                float height = Mathf.Max(90, detailStyle.CalcHeight(new GUIContent(detail), 1080));
                descriptionScroll = GUI.BeginScrollView(new Rect(395, 860, 1125, 90), descriptionScroll, new Rect(0, 0, 1080, height));
                GUI.Label(new Rect(0, 0, 1080, height), detail, detailStyle); GUI.EndScrollView();
                GUI.Label(new Rect(395, 960, 1100, 40),"Walk to a gift and press Space / A once to take it. Choose one.",copy);
            }
            GUI.matrix = matrix;
            GUI.skin = previousSkin;
        }
        private void OnDestroy() => Clear();
    }
}
