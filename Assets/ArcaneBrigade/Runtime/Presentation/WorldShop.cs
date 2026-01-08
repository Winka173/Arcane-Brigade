using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class WorldShop : MonoBehaviour
    {
        public const float InteractionRadius = 1.1f;
        public sealed class Station
        {
            public Vector2 Position;
            public ShopOffer Offer;
            public string Action;
            public string Description;
            public string Recipe;
            public Transform Marker;
            public Sprite Icon;
            public int Price;
            public ShopCurrency Currency;
            public float Focus;
            public float FocusVelocity;
            public float EnteredAt = -10;
            public float DeniedUntil;
            public float ReactUntil;
            public string Denial = "";
            public ShopDisplayActor Actor;
            public SpriteRenderer[] Renderers;
        }
        private ArenaController owner;
        private GameObject scenery;
        private int signature = int.MinValue;
        private PaintedAnimationLibrary art;
        private ShopDisplayActor merchant;
        private GUIStyle priceStyle;
        private GUIStyle denialStyle;
        public MergeReveal Reveal { get; private set; }
        public ShopPickupEffect ItemPickup { get; private set; }
        private readonly List<Station> stations = new List<Station>();
        public IReadOnlyList<Station> Stations => stations;
        public bool Special { get; private set; }
        public Station Nearby { get; private set; }
        public int DeniedCount { get; private set; }
        public void Configure(ArenaController controller)
        {
            owner = controller; art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            Reveal = gameObject.AddComponent<MergeReveal>(); Reveal.Configure(controller);
            ItemPickup = gameObject.AddComponent<ShopPickupEffect>(); ItemPickup.Configure(controller);
        }
        public void Display()
        {
            Reveal.ReducedMotion = owner.Profile != null && owner.Profile.ReducedMotion;
            if (owner.Session.State.Phase != SessionPhase.Shop)
            {
                if (scenery != null) scenery.SetActive(false);
                Nearby = null; Special = false; signature = int.MinValue; return;
            }
            RunSnapshot state = owner.Session.State;
            if (state.SpecialOffers.Count == 0) Special = false;
            int current = Special ? 7919 : 17;
            foreach (ShopOffer offer in Special ? state.SpecialOffers : state.Offers) current = unchecked(current * 31 + offer.Token * 2 + (offer.Sold ? 1 : 0));
            current = unchecked(current * 31 + state.Wave);
            if (current != signature) { Build(); signature = current; }
            scenery.SetActive(true); Nearby = null;
            float closest = InteractionRadius * InteractionRadius;
            foreach (Station station in stations)
            {
                if (station.Offer != null && station.Offer.Sold) continue;
                float distance = (station.Position - owner.Simulation.Champion.Position).sqrMagnitude;
                if (distance <= closest) { closest = distance; Nearby = station; }
            }
            if (owner.Simulation.Phase == RunPhase.Paused) return;
            foreach (Station station in stations)
            {
                float target = station == Nearby && !owner.HallExitPending ? 1 : 0;
                if (target > 0 && station.Focus < 0.02f) station.EnteredAt = Time.time;
                station.Focus = Mathf.SmoothDamp(station.Focus, target, ref station.FocusVelocity, 0.09f);
                bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
                float age = Time.time - station.EnteredAt;
                float bounce = reduced || age > 0.55f ? 0 : Mathf.Sin(age * 18f) * Mathf.Exp(-age * 7f) * 0.13f;
                float denied = Mathf.Clamp01((station.DeniedUntil - Time.time) / 0.65f);
                float shake = reduced ? 0 : Mathf.Sin(Time.time * 55) * denied * 0.1f;
                station.Marker.localScale = Vector3.one * (1 + station.Focus * (reduced ? 0.07f : 0.18f) + bounce);
                station.Marker.localPosition = station.Position + new Vector2(shake, reduced ? 0 : station.Focus * 0.08f + Mathf.Max(0, bounce));
                foreach (SpriteRenderer renderer in station.Renderers)
                    if (renderer.gameObject.name != "ShopFloorContact") renderer.color = Color.Lerp(station.Offer != null && station.Offer.Sold ? new Color(0.45f, 0.45f, 0.45f, 0.7f) : Color.white, new Color(1, 0.36f, 0.3f), denied);
            }
        }
        public bool Interact()
        {
            Display();
            if (owner.Simulation.Phase != RunPhase.Rest || Nearby == null || owner.HallExitPending || owner.TransitionPending) return false;
            Station station = Nearby; Vector2 position = station.Position; Sprite icon = station.Icon;
            bool completed;
            RunSnapshot before = null;
            if (station.Offer != null)
            {
                before = MergeReveal.Capture(owner.Session.State);
                completed = owner.BuyOffer(station.Offer.Token);
                if (completed)
                {
                    if (station.Offer.Kind == OfferKind.Companion) owner.PlayCommittedArrival(before, position);
                    else ItemPickup.Play(icon, position + Vector2.up * 0.35f);
                    Reveal.EnqueueCommitted(before, owner.Session.State, true, position, false);
                }
            }
            else if (station.Action == "NEXT HALL") return owner.BeginHallExit(position);
            else if (station.Action == "REROLL") completed = Special ? owner.RerollAltar() : owner.RerollShop();
            else if (station.Recipe != null)
            {
                before = MergeReveal.Capture(owner.Session.State);
                completed = owner.FuseAltar(station.Recipe);
                if (completed) owner.PlayCommittedArrival(before, position);
                Reveal.EnqueueCommitted(before, owner.Session.State, completed, position, true);
            }
            else
            {
                Special = !Special; signature = int.MinValue; completed = true; CheckpointAudio.PlayInterface(19);
            }
            Display();
            Station result = stations.Find(candidate => candidate.Position == position);
            if (completed)
            {
                if (merchant != null) merchant.Celebrate();
                if (result != null) { result.ReactUntil = Time.time + 0.8f; if (result.Actor != null) result.Actor.Celebrate(); }
            }
            else
            {
                DeniedCount++;
                if (result != null)
                {
                    result.DeniedUntil = Time.time + 0.65f;
                    result.Denial = !string.IsNullOrEmpty(owner.SaveError) ? "CHƯA LƯU ĐƯỢC" : station.Recipe != null ? "THIẾU NGUYÊN LIỆU" : station.Currency == ShopCurrency.Eggs ? "THIẾU TRỨNG" : station.Currency == ShopCurrency.Health ? "KHÔNG ĐỦ HP" : "THIẾU VÀNG";
                }
            }
            return completed;
        }
        private void Build()
        {
            if (scenery != null) { scenery.SetActive(false); Destroy(scenery); }
            scenery = new GameObject("NightMarketStations"); scenery.transform.SetParent(transform, false); stations.Clear();
            RunCatalog catalog = owner.Content; RunSnapshot state = owner.Session.State;
            Sprite[] merchantFrames = Special ? art.EggMerchant : art.GoldMerchant;
            merchant = AddActor(scenery.transform, merchantFrames[0], merchantFrames, new Vector2(0, 2.7f), 3.15f, 0.7f);
            ShopFloorShadow.Create(scenery.transform, new Vector2(0, 1.26f), 3.8f, 0.65f);
            ShopFloorShadow.Create(scenery.transform, new Vector2(0, 1.28f), 2.2f, 0.6f);
            List<ShopOffer> offers = Special ? state.SpecialOffers : state.Offers;
            for (int index = 0; index < offers.Count; index++)
            {
                ShopOffer offer = offers[index];
                string name = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).DisplayName : catalog.Item(offer.Id).Name;
                Sprite icon = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).Body : catalog.Item(offer.Id).Icon;
                Sprite[] frames = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).AnimationFrames : null;
                Station station = AddStation(new Vector2((index - 1) * 3f, 0.5f), name, offer.Sold ? null : icon, frames, true);
                station.Offer = offer; station.Price = offer.Price; station.Currency = offer.Currency;
                station.Description = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).Description : catalog.Item(offer.Id).Description;
                if (offer.Currency == ShopCurrency.Health) station.Description += " · Trả HP hiện tại, không giảm HP tối đa; phải còn ít nhất 1 HP.";
                if (!offer.Sold && offer.Kind == OfferKind.Item && catalog.Item(offer.Id).Rarity != ItemRarity.Common)
                {
                    var glow = new GameObject("ItemRarityAura"); glow.transform.SetParent(station.Marker, false); glow.transform.localPosition = new Vector2(0, 0.4f);
                    glow.AddComponent<ShopRarityGlow>().Configure(catalog.Item(offer.Id).Rarity, owner, index * 1.1f);
                }
                if (offer.Sold)
                    foreach (SpriteRenderer renderer in station.Renderers) if (renderer.gameObject.name != "ShopFloorContact") renderer.color = new Color(0.45f, 0.45f, 0.45f, 0.7f);
            }
            Station reroll = AddStation(new Vector2(-4.8f, 2.8f), "REROLL", art.RerollMachine[0], art.RerollMachine, false);
            reroll.Price = owner.Session.RerollCost; reroll.Description = "Đổi ba món đang bày. Giá tăng sau mỗi lần.";
            AddStation(new Vector2(0, -4.3f), "NEXT HALL", art.HallDoor[0], art.HallDoor, false).Description = "Đi xuống để tới màn tiếp theo.";
            if (Special || state.SpecialOffers.Count > 0)
            {
                Station visit = AddStation(new Vector2(4.8f, 2.8f), Special ? "GOLD SHOP" : "DREAM ALTAR", Special ? art.GoldMerchant[0] : art.EggMerchant[0], Special ? art.GoldMerchant : art.EggMerchant, false);
                visit.Description = Special ? "Ghé người bán vàng." : "Ghé người bán giấc mơ: dùng trứng hoặc HP theo từng món.";
            }
            if (!Special && state.Wave >= 4)
                for (int index = 0; index < catalog.Recipes.Length; index++)
                {
                    AltarRecipe recipe = catalog.Recipes[index];
                    Station station = AddStation(new Vector2(-4 + index * 8, -2.6f), "FUSE " + catalog.Unit(recipe.ResultUnit).DisplayName, catalog.Unit(recipe.ResultUnit).Body, catalog.Unit(recipe.ResultUnit).AnimationFrames, true);
                    station.Recipe = recipe.Id; station.Price = recipe.EggCost; station.Currency = ShopCurrency.Eggs;
                    station.Description = owner.Session.RecipePreview(recipe.Id);
                }
        }
        private Station AddStation(Vector2 position, string action, Sprite sprite, Sprite[] frames, bool pedestal)
        {
            var marker = new GameObject(action); marker.transform.SetParent(scenery.transform, false); marker.transform.localPosition = position;
            if (pedestal) AddActor(marker.transform, art.ShopFixtures[Special ? 1 : 0], null, new Vector2(0, -0.1f), 0.85f, 0).enabled = false;
            ShopDisplayActor actor = sprite == null ? null : AddActor(marker.transform, sprite, frames, pedestal ? new Vector2(0, 0.38f) : Vector2.zero, pedestal ? 0.8f : action == "NEXT HALL" ? 1.65f : 1.25f, stations.Count * 0.93f);
            ShopFloorShadow.Create(marker.transform, new Vector2(0, pedestal ? -0.48f : action == "NEXT HALL" ? -0.72f : -0.56f), pedestal ? 1.55f : 1.65f, 0.7f);
            var station = new Station { Position = position, Action = action, Marker = marker.transform, Icon = sprite, Actor = actor, Renderers = marker.GetComponentsInChildren<SpriteRenderer>() };
            stations.Add(station); return station;
        }
        private ShopDisplayActor AddActor(Transform parent, Sprite sprite, Sprite[] frames, Vector2 position, float height, float phase)
        {
            var child = new GameObject("AnimatedShopActor"); child.transform.SetParent(parent, false); child.transform.localPosition = position;
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = 1000 - Mathf.RoundToInt(child.transform.position.y * 12);
            child.transform.localScale = Vector3.one * (height / sprite.bounds.size.y);
            ShopDisplayActor actor = child.AddComponent<ShopDisplayActor>(); actor.Configure(renderer, frames, height, owner, phase); return actor;
        }
        public void DrawPriceTags(Camera camera)
        {
            if (scenery == null || !scenery.activeSelf || owner.HallExitPending) return;
            if (priceStyle == null) { priceStyle = ArcadeTypography.Style(25, true); priceStyle.normal.textColor = ArcadeSkin.Ink; priceStyle.alignment = TextAnchor.MiddleLeft; denialStyle = ArcadeTypography.Style(20, true); denialStyle.alignment = TextAnchor.MiddleCenter; denialStyle.normal.textColor = new Color(1, 0.45f, 0.35f); }
            foreach (Station station in stations)
            {
                if (station.Offer != null && station.Offer.Sold || station.Action == "NEXT HALL" || station.Action == "GOLD SHOP" || station.Action == "DREAM ALTAR") continue;
                Vector3 screen = camera.WorldToScreenPoint(station.Marker.position + Vector3.down * (station.Action == "REROLL" ? 0.83f : 0.65f));
                Vector3 point = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y, 0));
                point.x -= station.Focus * 84;
                float scale = 1 + station.Focus * 0.09f;
                Rect ticket = new Rect(point.x - 59 * scale, point.y - 20 * scale, 118 * scale, 40 * scale);
                bool denied = Time.time < station.DeniedUntil;
                ArcadeSkin.Ticket(ticket, denied ? new Color(1, 0.48f, 0.4f) : ArcadeSkin.Cream, station.Currency == ShopCurrency.Eggs ? ArcadeSkin.Teal : new Color(1, 0.48f, 0.36f));
                Sprite currency = station.Currency == ShopCurrency.Eggs ? owner.Content.EggSprite : station.Currency == ShopCurrency.Health ? owner.Content.HeartSprite : owner.Content.Arena.Coin;
                RunHud.Sprite(new Rect(ticket.x + 10, ticket.y + 7, 26, 26), currency);
                GUI.Label(new Rect(ticket.x + 44, ticket.y + 3, ticket.width - 50, 34), station.Price.ToString(), priceStyle);
                if (denied) GUI.Label(new Rect(point.x - 140, point.y + 31, 280, 35), station.Denial, denialStyle);
            }
        }
    }
}
