using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class OutsideStageChoice : MonoBehaviour, ShopFocus.ISource
    {
        private ArenaController owner;
        private readonly List<GameObject> actors = new List<GameObject>();
        private readonly Vector2[] rewardPositions = { new Vector2(-3, -0.8f), new Vector2(0, -0.8f), new Vector2(3, -0.8f) };
        public static readonly Vector2 ChallengePosition = new Vector2(5.2f, -3.3f);
        /// <summary>In Act I (shop 17) the warm chair stands by the bottom-right corner, so the hand waits on the right under
        /// the merchant's speech instead (the secret invitations' spot; shop 17 has none).</summary>
        public static readonly Vector2 ActOneChallengePosition = new Vector2(5.05f, -1.3f);
        private Vector2 HandAt => owner?.Session != null && !owner.Session.State.Endless && owner.Session.State.Wave <= ActOneCanon.Waves ? ActOneChallengePosition : ChallengePosition;
        public bool Focused => owner != null && owner.Session.OutsideChallengeAvailable && ShopFocus.Has(this, 0, owner.Simulation.Champion.Position);
        public int SpotCount => mode == 1 ? 1 : mode == 2 ? 3 : 0;
        public bool Spot(int index, out Vector2 position, out float reach)
        {
            position = mode == 1 ? HandAt : rewardPositions[Mathf.Clamp(index, 0, 2)]; reach = mode == 1 ? 1.6f : 1.05f;
            return owner?.Simulation != null && (mode == 1 ? owner.Session.OutsideChallengeAvailable : mode == 2);
        }
        private int mode;
        private int selection = -1;
        private GUIStyle title;
        private GUIStyle copy;
        private Vector2 descriptionScroll;
        private int preview;
        private readonly ShopDescriptionCard giftCard = new ShopDescriptionCard();
        private readonly ShopOffer giftOffer = new ShopOffer();
        private GUIStyle cardName, cardLine, identity;
        public void Configure(ArenaController controller) { owner = controller; ShopFocus.Register(this); }
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
            else for (int index = 0; index < 3; index++) if (ShopFocus.Has(this, index, player)) near = index;
            if (selection != near) { selection = near; if (near >= 0) { preview = near; descriptionScroll = Vector2.zero; } }
            float time = owner.PresentationClock;
            for (int index = 0; index < actors.Count; index++)
            {
                Vector2 origin = mode == 1 ? HandAt : rewardPositions[index];
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
                if (mode == 1) sprite.sprite = ActOneArtwork.Picture("Keeper_Hand_Intact") ?? (CarrierArtwork.Current != null ? CarrierArtwork.Current.Hand[0] : assets.BossFrames[4]);
                else
                {
                    string id = owner.Session.State.OutsideRewards[index];
                    ItemDefinition item = owner.Content.Item(id); sprite.sprite = item == null ? owner.Content.Unit(id).Body : item.Icon;
                }
                sprite.sortingOrder = 950; actor.transform.localScale = Vector3.one * ((mode == 1 ? 2.5f : 1.1f) / Mathf.Max(0.01f, sprite.sprite.bounds.size.x));
                GroundShadow shadow = actor.AddComponent<GroundShadow>(); shadow.Display(sprite.sprite, mode == 1 ? HandAt : rewardPositions[index], mode == 1 ? 2.2f : 0.8f, 0.5f, 0.22f);
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
                Vector3 point=Camera.main.WorldToViewportPoint(HandAt);
                Rect badge=new Rect(Mathf.Clamp(point.x*1920-(HandAt.x>0?110:185),30,1520),Mathf.Clamp((1-point.y)*1080+16,190,898),370,100);
                RarityDescriptionFrame.Draw(badge,ItemRarity.Rainbow,owner.PresentationClock);
                GUI.Label(new Rect(badge.x+8,badge.y+5,354,32),HandAt==ActOneChallengePosition?"THE KEEPER, WHOLE · TEST":"OPTIONAL BOSS / THE KEEPER",copy);
                GUI.Label(new Rect(badge.x+8,badge.y+37,354,55),Focused?"Defeat ends the run":owner.Session.State.Secrets!=null && owner.Session.State.Secrets.LetterSent?"It remembers your letter.\nWin a rainbow companion or relic":(HandAt==ActOneChallengePosition?"Break its hand, carpet and eye\nWin a rainbow companion or relic":"Approach the glowing hand\nWin a rainbow companion or relic"),copy);
                if(Focused && GUI.Button(badge,GUIContent.none,GUIStyle.none))Interact();
            }
            else
            {
                GUI.Label(new Rect(340, 180, 1240, 55), "TAKE ONE IMPOSSIBLE GIFT", title);
                for (int i = 0; i < 3; i++)
                {
                    string rewardId = owner.Session.State.OutsideRewards[i];
                    string rewardName = owner.Content.Item(rewardId)?.Name ?? owner.Content.Unit(rewardId).DisplayName;
                    if (GUI.Button(new Rect(340 + i * 420, 640, 400, 56), rewardName, choiceButton)) { preview = i; descriptionScroll = Vector2.zero; }
                }
                // the same card as the shop's (owner, 20/10/2026): icon, name, identity and the shop's own description with its
                // stat icons, laid out inside the frame's clear area so the rainbow trim never covers the text; TAKE claims it
                int shown = preview;
                string id = owner.Session.State.OutsideRewards[shown]; ItemDefinition item = owner.Content.Item(id); UnitDefinition ally = owner.Content.Unit(id);
                giftOffer.Kind = item != null ? OfferKind.Item : OfferKind.Companion; giftOffer.Id = id; giftOffer.Tier = 1; giftOffer.Token = -1; giftOffer.Price = 0;
                giftCard.Refresh(owner.Session, owner.Content, giftOffer);
                Rect panel = new Rect(330, 778, 1260, 292); // below the crest's reach of the name row
                RarityDescriptionFrame.Draw(panel, ItemRarity.Rainbow, owner.PresentationClock);
                Rect inner = RarityDescriptionFrame.Content(panel, ItemRarity.Rainbow);
                if (cardName == null) { cardName = ArcadeTypography.Style(30, true); cardName.wordWrap = false; cardName.clipping = TextClipping.Clip; cardLine = ArcadeTypography.Style(21); identity = ArcadeTypography.Style(19); }
                cardName.normal.textColor = RunModifierRules.RarityColor(ItemRarity.Rainbow); cardLine.normal.textColor = ArcadeSkin.Cream; identity.normal.textColor = ArcadeSkin.Teal;
                ShopDescriptionCard.DrawSprite(new Rect(inner.x, inner.y, 60, 60), giftCard.Portrait);
                Rect take = new Rect(inner.xMax - 180, inner.y, 180, 60);
                GUI.Label(new Rect(inner.x + 74, inner.y - 2, take.x - inner.x - 90, 38), item?.Name ?? ally.DisplayName, cardName);
                GUI.Label(new Rect(inner.x + 74, inner.y + 36, take.x - inner.x - 90, 26), giftCard.Identity, identity);
                if (GUI.Button(take, "TAKE", choiceButton) && owner.ClaimOutsideReward(shown)) { Clear(); mode = 0; GUI.matrix = matrix; GUI.skin = previousSkin; return; }
                string detail = item != null ? giftCard.Summary + "\n\n" + giftCard.Details : giftCard.Details;
                float top = inner.y + 72, width = inner.width - 26, height = GlossaryUI.Height(detail, cardLine, width) + 8;
                descriptionScroll = GUI.BeginScrollView(new Rect(inner.x, top, inner.width, inner.yMax - top), descriptionScroll, new Rect(0, 0, width, height));
                GlossaryUI.Text(new Rect(0, 0, width, height), detail, cardLine);
                GUI.EndScrollView();
                GlossaryUI.Draw(new Vector2(1920, 1080));
            }
            GUI.matrix = matrix;
            GUI.skin = previousSkin;
        }
        private void OnDestroy() { ShopFocus.Unregister(this); Clear(); }
    }
}
