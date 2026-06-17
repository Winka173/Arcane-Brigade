using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Optional, world-anchored transactions. These painted coins never enter the loot or wallet simulation.</summary>
    [DefaultExecutionOrder(105)]
    public sealed class ActOneCustomerVisits : MonoBehaviour
    {
        private const string LayoutId = "story:LastCustomerVisit";
        private ArenaController owner;
        private SpriteRenderer customer, window;
        private readonly SpriteRenderer[] coins = new SpriteRenderer[3];
        private GroundShadow shadow;
        private FloatingStoryProp floatingWindow;
        private string visit;
        private int wave;
        private float elapsed, previousClock;
        private bool visible, registered;
        public void Configure(ArenaController value)
        {
            owner = value;
            customer = Make("Customer beside the seat"); window = Make("The window between us");
            window.sprite = ActOneArtwork.Picture("Prop_InwardWindow");
            floatingWindow = window.gameObject.AddComponent<FloatingStoryProp>(); floatingWindow.Configure(window,749);
            shadow = customer.gameObject.AddComponent<GroundShadow>();
            for (int i = 0; i < coins.Length; i++) { coins[i] = Make("Customer's payment " + i); coins[i].sprite = owner.Content.Arena.Coin; }
        }
        private SpriteRenderer Make(string label)
        {
            var go = new GameObject(label); go.transform.SetParent(transform, false);
            var view = go.AddComponent<SpriteRenderer>(); view.enabled = false; return view;
        }
        private static bool Payment(int number) => number is 2 or 4 or 7 or 9 or 12 or 13 or 16 or 19;
        private bool Available => owner?.Session != null && owner.Simulation != null && owner.Session.State.Phase == SessionPhase.Shop
            && !owner.Session.State.Endless && !owner.Session.SecretRoomOpen && !owner.TransitionPending && !owner.HallExitPending && !owner.NightmareEndingVisible;
        private void LateUpdate()
        {
            if (!Available) { Hide(); previousClock = owner?.PresentationClock ?? 0; return; }
            var state = owner.Session.State;
            wave = state.Wave;
            if (!Payment(wave) && wave != 8) { Hide(); previousClock = owner.PresentationClock; return; }
            string key = state.RunId + ":" + wave;
            if (visit != key) { Hide(); visit = key; elapsed = 0; previousClock = owner.PresentationClock; }
            float now = owner.PresentationClock;
            // Reading, pausing or choosing a reward never consumes the small scene off-screen.
            if (!owner.PresentationPaused && !owner.StoryConsumesInput && !owner.Details.Open && owner.Shop?.Animating != true)
                elapsed += Mathf.Max(0, now - previousClock);
            previousClock = now;
            var chair = owner.GetComponent<WaitingRealmStage>();
            if (chair == null || !chair.ChairVisible) { Hide(); return; }
            Vector2 target = chair.ChairAnchor + Vector2.up * (chair.ChairSize.y * .64f + .2f);
            // The intermediate payments are found already lying there: canon says nobody saw him arrive.
            bool actor = wave is 2 or 7 or 8 or 12 or 13 or 19;
            Sprite picture = ActOneArtwork.Picture(wave == 12 ? "LastCustomer_Kneel" : wave == 19 && elapsed > 3.6f && elapsed < 6 ? "LastCustomer_Blank" : wave >= 13 ? "LastCustomer_BorrowedFace" : "LastCustomer_Pleasant");
            customer.sprite = picture;
            Vector2 preferred = chair.ChairAnchor + new Vector2(-1.9f, .1f), anchor = preferred;
            float height = wave == 12 ? 2.15f : 2.6f, fit = 1;
            Vector2 size = picture == null ? new Vector2(1.5f,height) : SpriteInkMetrics.BoundsOf(picture).size * (height / Mathf.Max(.01f,SpriteInkMetrics.BoundsOf(picture).height));
            // wave 7: the window stands beside him, never over him (requirement 25/10/2026: it read as a prop on top of him)
            const float WindowRoom = 1.3f;
            float ownWidth = size.x;
            if (wave == 7) size.x += WindowRoom;
            bool room = true;
            if (actor && ShopLayout.Active != null)
            {
                registered = true;
                room = ShopLayout.Active.Place(LayoutId,preferred,size,-1,true,false,out anchor,out fit);
                fit = 1; // the Last Customer is the same size in every shop (the fitting may move him, never shrink him)
            }
            visible = actor && room && picture != null;
            customer.enabled = visible;
            window.enabled = visible && wave == 7 && window.sprite != null;
            float light = Mathf.Max(.4f, SharedLightField.Active?.SurfaceVisibility(anchor) ?? 1);
            if (visible)
            {
                float lean = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(Mathf.Clamp01((elapsed - .7f) / 2) * Mathf.PI) * -5;
                Vector2 figure = wave == 7 ? anchor - new Vector2(WindowRoom * .5f, 0) : anchor;
                Place(customer,figure,height * fit,WorldSpriteGrounding.SortAt(anchor.y));
                customer.transform.rotation = Quaternion.Euler(0,0,lean);
                customer.color = new Color(light,light * .94f,light * .85f);
                shadow.SortingBase = customer.sortingOrder - 12;
                shadow.Display(picture,figure,ownWidth * fit,0,.24f);
                if (window.enabled)
                {
                    Vector2 beside = figure + new Vector2(ownWidth * .5f + WindowRoom * .5f + .05f, .2f);
                    Place(window,beside,height * .64f * fit,customer.sortingOrder + 2);
                    floatingWindow.Display(beside,now,NightmareRules.Depth(wave));
                    window.sortingOrder = customer.sortingOrder + 2;
                    window.color = new Color(light * .65f,light * .7f,light * .78f);
                }
            }
            else shadow.Hide();
            if (!window.enabled) floatingWindow.Display(Vector3.zero,now,0);
            for (int i = 0; i < coins.Length; i++)
            {
                // After landing the existing painted chair offering carries the coins; do not draw a second pile.
                float t = Mathf.Clamp01((elapsed - 1.1f - i * .15f) / (wave == 7 ? 1.1f : .65f));
                var coin = coins[i];
                coin.enabled = visible && Payment(wave) && coin.sprite != null && elapsed > 1.1f + i * .15f && t < 1;
                if (!coin.enabled) continue;
                Vector2 source = (wave == 7 ? anchor - new Vector2(WindowRoom * .5f, 0) : anchor) + new Vector2(.55f,height * .49f * fit);
                Vector2 point = Vector2.Lerp(source,target + Vector2.right * (i - 1) * .13f,t);
                if (!PresentationSettings.ReducedMotion) point.y += Mathf.Sin(t * Mathf.PI) * (wave == 7 ? .85f : .3f);
                Place(coin,point,.18f,Mathf.Max(customer.sortingOrder,WorldSpriteGrounding.SortAt(chair.ChairAnchor.y)) + 16);
                coin.color = new Color(light,light * .94f,light * .75f);
            }
        }
        private static void Place(SpriteRenderer view,Vector2 point,float height,int order)
        {
            float scale = height / Mathf.Max(.01f,SpriteInkMetrics.BoundsOf(view.sprite).height);
            view.transform.localScale = Vector3.one * scale;
            view.transform.rotation = Quaternion.identity;
            WorldSpriteGrounding.Place(view,point); view.sortingOrder = order;
        }
        public bool DeferOffering => Available && visible && Payment(wave) && elapsed < (wave == 7 ? 2.5f : 2.05f);
        private void Hide()
        {
            visible = false;
            if (customer != null) customer.enabled = window.enabled = false;
            floatingWindow?.Display(Vector3.zero,owner?.PresentationClock ?? 0,0);
            foreach (var coin in coins) if (coin != null) coin.enabled = false;
            shadow?.Hide();
            if (registered) { ShopLayout.Active?.Release(LayoutId); registered = false; }
        }
        private void OnGUI()
        {
            if (!Available || !visible || elapsed < 2.6f || elapsed > 12 || owner.PresentationPaused || owner.StoryConsumesInput || owner.Details.Open || Camera.main == null) return;
            string line = wave switch
            {
                2 => "\"We'll be seeing a lot of each other.\"",
                7 => "Outside the window. The coins land on the seat. He has never used the counter.",
                12 => "He lowers himself beside the chair. \"The face, please.\"",
                19 => StoodUp() ? "\"You did. Who paid?\"\nYour brigade: \"Nobody. We pulled.\"" : "The coins wait. The chair gives him nothing.\n\"Nobody stands up without paying.\"",
                _ => null
            };
            if (line == null || spokenWave == wave) return;
            // the visit's line is said by the customer, in a bubble that stays with it (not a caption over the room)
            spokenWave = wave;
            SpeechBubbles.Show("Story:LastCustomer", "THE LAST CUSTOMER", line, () => customer != null && customer.enabled ? new Vector3(customer.bounds.center.x, customer.bounds.max.y, 0) : (Vector3?)null, SpeechTone.Story, Mathf.Max(3, 12 - elapsed), 3);
        }
        private int spokenWave = -1;
        private bool StoodUp() => owner.Session.State.ActOne?.SitCloser == ActOneProgress.Won;
        private void OnDestroy() => Hide();
    }
}
