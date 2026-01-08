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
        private bool arrivingStock = true;
        private int displayedWave = -1;
        private bool displayedSpecial;
        private readonly MerchantDialogue dialogue = new MerchantDialogue();
        public MerchantDialogue Dialogue => dialogue;
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
                Nearby = null; Special = false; signature = int.MinValue; arrivingStock = true; return;
            }
            RunSnapshot state = owner.Session.State;
            if (state.SpecialOffers.Count == 0) Special = false;
            int current = Special ? 7919 : 17;
            foreach (ShopOffer offer in Special ? state.SpecialOffers : state.Offers) current = unchecked(current * 31 + offer.Token * 2 + (offer.Sold ? 1 : 0));
            current = unchecked(current * 31 + state.Wave);
            if (current != signature) { Build(); signature = current; }
            scenery.SetActive(true); Nearby = null;
            if (merchant != null) merchant.LookAt((owner.Simulation.Champion.Position.x - merchant.transform.position.x) * 0.25f);
            float closest = InteractionRadius * InteractionRadius;
            foreach (Station station in stations)
            {
                if (station.Offer != null && station.Offer.Sold) continue;
                float distance = (station.Position - owner.Simulation.Champion.Position).sqrMagnitude;
                if (distance <= closest) { closest = distance; Nearby = station; }
            }
            if (owner.Simulation.Phase == RunPhase.Paused) return;
            dialogue.Observe(Special, Nearby == null ? null : Nearby.Offer != null ? "offer:" + Nearby.Offer.Token : Nearby.Action, owner.Simulation.VisualElapsed);
            if (merchant != null) merchant.Inspect(Nearby != null);
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
            else if (station.Action == "NEXT HALL")
            {
                bool exit = owner.BeginHallExit(position);
                if (exit) dialogue.Speak(Special, MerchantMoment.Leave, owner.Simulation.VisualElapsed);
                return exit;
            }
            else if (station.Action == "REROLL")
            {
                completed = Special ? owner.RerollAltar() : owner.RerollShop();
                if (completed)
                {
                    foreach (Station old in stations)
                        if (old.Offer != null && !old.Offer.Sold && old.Actor != null) ShopStockRetirement.Play(owner, old.Actor.GetComponent<SpriteRenderer>());
                    arrivingStock = true;
                }
            }
            else if (station.Recipe != null)
            {
                before = MergeReveal.Capture(owner.Session.State);
                completed = owner.FuseAltar(station.Recipe);
                if (completed) owner.PlayCommittedArrival(before, position);
                Reveal.EnqueueCommitted(before, owner.Session.State, completed, position, true);
            }
            else
            {
                Special = !Special; signature = int.MinValue; arrivingStock = true; completed = true; CheckpointAudio.PlayInterface(19);
            }
            Display();
            Station result = stations.Find(candidate => candidate.Position == position);
            if (completed)
            {
                if (station.Offer != null || station.Recipe != null || station.Action == "REROLL")
                    dialogue.Speak(Special, station.Recipe != null ? MerchantMoment.Fuse : station.Action == "REROLL" ? MerchantMoment.Reroll : station.Currency == ShopCurrency.Health ? MerchantMoment.BloodBuy : MerchantMoment.Buy, owner.Simulation.VisualElapsed);
                if (merchant != null) merchant.Celebrate();
                if (result != null) { result.ReactUntil = Time.time + 0.8f; if (result.Actor != null) result.Actor.Celebrate(); }
            }
            else
            {
                dialogue.Speak(Special, MerchantMoment.Denied, owner.Simulation.VisualElapsed);
                if (merchant != null) merchant.Reject();
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
            bool arrival = arrivingStock || displayedWave != state.Wave;
            bool merchantEntrance = displayedWave != state.Wave || displayedSpecial != Special || dialogue.CurrentLine == null;
            arrivingStock = false; displayedWave = state.Wave;
            displayedSpecial = Special;
            K8ArenaScenery.CreateMarketFloor(scenery.transform, Special);
            Sprite[] merchantFrames = Special ? art.EggMerchant : art.GoldMerchant;
            merchant = AddActor(scenery.transform, merchantFrames[0], merchantFrames, new Vector2(0, 2.7f), 3.15f, 0.7f);
            merchant.SetMerchantMood(Special);
            if (merchantEntrance)
            {
                merchant.Arrive(0); MerchantEntranceEffect.Play(owner, scenery.transform, merchant.transform.localPosition, Special);
                dialogue.ResetFocus(); dialogue.Speak(Special, MerchantMoment.Enter, owner.Simulation.VisualElapsed);
                CheckpointAudio.PlayInterface(20);
            }
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
                if (!offer.Sold && arrival && station.Actor != null) station.Actor.Arrive(0.16f + index * 0.09f);
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
                Sprite sign = art.EnvironmentProps != null && art.EnvironmentProps.Length > 3 ? art.EnvironmentProps[3] : Special ? art.GoldMerchant[0] : art.EggMerchant[0];
                Station visit = AddStation(new Vector2(4.8f, 2.8f), Special ? "GOLD SHOP" : "DREAM ALTAR", sign, null, false);
                visit.Description = Special ? "Ghé người bán vàng." : "Ghé người bán giấc mơ: dùng trứng hoặc HP theo từng món.";
            }
            if (!Special && state.Wave >= 4)
                for (int index = 0; index < catalog.Recipes.Length; index++)
                {
                    AltarRecipe recipe = catalog.Recipes[index];
                    Station station = AddStation(new Vector2(index % 2 == 0 ? -4.4f : 4.4f, -1.4f - index / 2 * 1.75f), "FUSE " + catalog.Unit(recipe.ResultUnit).DisplayName, catalog.Unit(recipe.ResultUnit).Body, catalog.Unit(recipe.ResultUnit).AnimationFrames, true);
                    station.Recipe = recipe.Id; station.Price = recipe.EggCost; station.Currency = ShopCurrency.Eggs;
                    station.Description = owner.Session.RecipePreview(recipe.Id);
                }
        }
        public string DescribeOffer(ShopOffer offer)
        {
            if (offer == null) return "";
            RunCatalog catalog = owner.Content;
            string result;
            if (offer.Kind == OfferKind.Item)
            {
                ItemDefinition item = catalog.Item(offer.Id);
                int held = owner.Session.State.Items.Find(stack => stack.Id == item.Id)?.Count ?? 0;
                string stats = RunModifierRules.Describe(item, 1).TrimEnd();
                result = item.Description.Contains(stats) ? item.Description : item.Description + "\n" + stats;
                result += "\nĐang có: " + held + " · Sau mua: " + item.EffectiveStacks(held + 1);
            }
            else
            {
                UnitDefinition unit = catalog.Unit(offer.Id);
                int copies = owner.Session.State.Roster.FindAll(entry => entry.UnitId == unit.Id && entry.Tier == 1).Count;
                result = RunUICopy.PassiveDescription(unit.Passive) + ".\nVũ khí ngẫu nhiên khi nhận · " + copies + "/3 bản cấp 1";
                if (copies >= 2) result += " · Có thể ghép cấp sau khi nhận";
                foreach (string tag in unit.Tags)
                    foreach (BondDefinition bond in catalog.Bonds)
                        if (bond.Id == tag) result += " · " + bond.Name;
            }
            if (offer.Currency == ShopCurrency.Health) result += "\nTrả HP hiện tại; không giảm HP tối đa; phải còn ít nhất 1 HP.";
            return result;
        }
        public void DrawMerchantDialogue()
        {
            if (scenery != null && scenery.activeSelf && !owner.HallExitPending) dialogue.Draw(Special, owner.Simulation.VisualElapsed, owner.Profile != null && owner.Profile.ReducedMotion);
        }
        public string DisplayName(Station station)
        {
            if (station.Action == "REROLL") return "ĐỔI HÀNG";
            if (station.Action == "NEXT HALL") return "ĐI XUỐNG · MÀN TIẾP";
            if (station.Action == "GOLD SHOP") return "QUẦY VÀNG";
            if (station.Action == "DREAM ALTAR") return "QUẦY GIẤC MƠ";
            if (station.Recipe != null) return "HỢP THỂ · " + station.Action.Substring(5);
            return station.Action;
        }
        public void ResolveCompanionPositions()
        {
            if (scenery == null || !scenery.activeSelf || owner.Simulation.Phase != RunPhase.Rest) return;
            foreach (UnitState unit in owner.Simulation.Units)
            {
                if (unit.Faction != Faction.Companion) continue;
                Vector2 position = AvoidObstacle(unit.Position, new Vector2(0, 2.4f), 1.65f, unit.Id);
                foreach (Station station in stations) position = AvoidObstacle(position, station.Position, station.Action == "NEXT HALL" ? 0.55f : 0.65f, unit.Id);
                position = owner.Simulation.Champion.Position + Vector2.ClampMagnitude(position - owner.Simulation.Champion.Position, owner.Session.CombatCatalog.CompanionLeash);
                unit.Position = position;
            }
        }
        public static Vector2 AvoidObstacle(Vector2 position, Vector2 center, float radius, int identity)
        {
            Vector2 offset = position - center;
            if (offset.sqrMagnitude >= radius * radius) return position;
            Vector2 direction = offset.sqrMagnitude > 0.0001f ? offset.normalized : new Vector2(Mathf.Cos(identity * 2.4f), Mathf.Sin(identity * 2.4f));
            return center + direction * radius;
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
