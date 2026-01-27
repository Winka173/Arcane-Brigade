using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class OutsideStageChoice : MonoBehaviour
    {
        private ArenaController owner;
        private readonly List<GameObject> actors = new List<GameObject>();
        private readonly Vector2[] rewardPositions = { new Vector2(-3, -0.8f), new Vector2(0, -0.8f), new Vector2(3, -0.8f) };
        private readonly Vector2 challengePosition = new Vector2(4.3f, -3.3f);
        private int mode;
        private float hold;
        private int selection = -1;
        private GUIStyle title;
        private GUIStyle copy;
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
            if (mode == 1) { if (Vector2.Distance(player, challengePosition) < 1.05f) near = 0; }
            else for (int index = 0; index < 3; index++) if (Vector2.Distance(player, rewardPositions[index]) < 1.05f) near = index;
            if (selection != near) { selection = near; hold = 0; }
            bool pressed = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton0);
            hold = selection >= 0 && pressed ? hold + Time.deltaTime : 0;
            float time = owner.PresentationClock;
            for (int index = 0; index < actors.Count; index++)
            {
                Vector2 origin = mode == 1 ? challengePosition : rewardPositions[index];
                actors[index].transform.position = origin + Vector2.up * (PresentationSettings.ReducedMotion ? 0.5f : 0.5f + Mathf.Sin(time * 2 + index) * 0.12f);
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
                if (mode == 1) sprite.sprite = assets.BossFrames[4];
                else
                {
                    string id = owner.Session.State.OutsideRewards[index];
                    ItemDefinition item = owner.Content.Item(id); sprite.sprite = item == null ? owner.Content.Unit(id).Body : item.Icon;
                }
                sprite.sortingOrder = 320; actor.transform.localScale = Vector3.one * ((mode == 1 ? 2.5f : 1.1f) / Mathf.Max(0.01f, sprite.sprite.bounds.size.x));
                var glow = new GameObject("BoundPrismaticLight"); glow.transform.SetParent(actor.transform, false);
                glow.AddComponent<ShopRarityGlow>().Configure(ItemRarity.Rainbow, owner, index, sprite);
                actors.Add(actor);
            }
        }
        private void OnGUI()
        {
            if (mode == 0 || owner == null || owner.TransitionPending || owner.PresentationPaused) return;
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
                GUI.Label(new Rect(340, 870, 1240, 42), "THE HAND THAT CARRIED YOU IS WAITING", title);
                GUI.Label(new Rect(340, 915, 1240, 70), selection >= 0 ? "Hold Space / A to challenge • Defeat ends this run • Win: choose one prismatic reward" : "Approach the hand to challenge its owner. The normal exit remains available.", copy);
            }
            else
            {
                GUI.Label(new Rect(340, 180, 1240, 55), "TAKE ONE IMPOSSIBLE GIFT", title);
                if (selection >= 0)
                {
                    string id = owner.Session.State.OutsideRewards[selection]; ItemDefinition item = owner.Content.Item(id); UnitDefinition ally = owner.Content.Unit(id);
                    GUI.Label(new Rect(340, 870, 1240, 100), (item == null ? ally.DisplayName + " — " + ally.Description : item.Name + " — " + item.Description) + "\nHold Space / A to claim. The other gifts vanish.", copy);
                }
            }
            if (selection >= 0) GUI.Label(new Rect(760, 995, 400, 35), Mathf.RoundToInt(Mathf.Clamp01(hold / 0.85f) * 100) + "%", copy);
            GUI.matrix = matrix;
        }
        private void OnDestroy() => Clear();
    }
}
