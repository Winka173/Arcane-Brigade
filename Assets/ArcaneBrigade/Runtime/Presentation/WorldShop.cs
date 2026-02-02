using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class WorldShop : MonoBehaviour
    {
        public const float InteractionRadius = 1.1f;
        public const float LayoutLift = 0.5f;
        public static readonly Vector2 ExitPosition = new Vector2(0, -4.5f);
        public const float ExitInteractionRadius = 1.85f;
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
            public float Reach = InteractionRadius;
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
        private MerchantVoice voice;
        private GUIStyle priceStyle;
        private GUIStyle denialStyle;
        private GUIStyle categoryStyle;
        private bool arrivingStock = true;
        private bool pedestalEntrance;
        private int displayedWave = -1;
        private bool displayedSpecial;
        private MarketLifecycle lifecycle;
        private int previewToken = -1;
        private int previewReceipt = -1;
        private string bondPreview = "";
        private Station audioFocus;
        public string BondPreview(ShopOffer offer)
        {
            if (offer == null) return "";
            if (previewToken != offer.Token || previewReceipt != owner.Session.State.NextReceipt)
            {
                previewToken = offer.Token; previewReceipt = owner.Session.State.NextReceipt;
                bondPreview = ShortOfferCopy.BondPreview(owner.Session, owner.Content, offer);
                if (offer.Kind == OfferKind.Item)
                {
                    string form = BuildMorphReadModel.ItemPreview(owner.Session, owner.Content, owner.Content.Item(offer.Id));
                    if (!string.IsNullOrEmpty(form)) bondPreview = string.IsNullOrEmpty(bondPreview) ? form : bondPreview + "\n" + form;
                }
            }
            return bondPreview;
        }
        private bool leavingSpecial;
        private float specialDepartureAt;
        public bool Animating => lifecycle != null && lifecycle.Busy;
        private readonly MerchantDialogue dialogue = new MerchantDialogue();
        public MerchantDialogue Dialogue => dialogue;
        public int PlayedSpeechSyllables => voice == null ? 0 : voice.PlayedSyllables;
        public MergeReveal Reveal { get; private set; }
        public ShopPickupEffect ItemPickup { get; private set; }
        public ShopMergePreview MergePreview { get; private set; }
        private readonly List<Station> stations = new List<Station>();
        public IReadOnlyList<Station> Stations => stations;
        public bool Special { get; private set; }
        public float MerchantHeight { get; private set; }
        public string MerchantIdentity { get; private set; }
        public Vector2 MerchantCenter { get; private set; }
        public Bounds MerchantBounds => merchant == null ? default : merchant.GetComponent<SpriteRenderer>().bounds;
        // Act I canon: it is always the same shop and the same shopkeeper, however deep the road goes.
        public static float MerchantSize(bool special, int wave) => special ? 5.65f : 5.25f;
        public Station Nearby { get; private set; }
        public int DeniedCount { get; private set; }
        public void Configure(ArenaController controller)
        {
            owner = controller; art = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            Reveal = gameObject.AddComponent<MergeReveal>(); Reveal.Configure(controller);
            ItemPickup = gameObject.AddComponent<ShopPickupEffect>(); ItemPickup.Configure(controller);
            MergePreview = gameObject.AddComponent<ShopMergePreview>(); MergePreview.Configure(controller);
            voice = gameObject.AddComponent<MerchantVoice>();
        }
        public void Display()
        {
            dialogue.Context = owner.Session.State;
            Reveal.ReducedMotion = owner.Profile != null && owner.Profile.ReducedMotion;
            if (owner.Session.State.Phase != SessionPhase.Shop || owner.Session.State.OutsideStage == OutsideStageStatus.Reward || owner.Session.SecretRoomOpen)
            {
                if (scenery != null) scenery.SetActive(false);
                if (voice != null) voice.Silence();
                Nearby = null; MergePreview.Observe(null, true); Special = false; signature = int.MinValue; arrivingStock = true; return;
            }
            RunSnapshot state = owner.Session.State;
            Special = state.SpecialOffers.Count > 0 && !state.SpecialShopVisited;
            if (leavingSpecial && owner.PresentationClock >= specialDepartureAt)
            {
                leavingSpecial = false;
                if (!owner.FinishSpecialShop()) { lifecycle.Reenter(); return; }
                Special = false; signature = int.MinValue; arrivingStock = true;
            }
            int current = Special ? 7919 : 17;
            foreach (ShopOffer offer in Special ? state.SpecialOffers : state.Offers) current = unchecked(current * 31 + offer.Token * 2 + (offer.Sold ? 1 : 0));
            current = unchecked(current * 31 + state.Wave);
            if (current != signature) { Build(); signature = current; }
            scenery.SetActive(true); Nearby = null;
            if (Animating || leavingSpecial || owner.HallExitPending) { MergePreview.Observe(null, true); return; }
            if (merchant != null) merchant.LookAt((owner.Simulation.Champion.Position.x - merchant.transform.position.x) * 0.25f);
            float closest = float.PositiveInfinity;
            foreach (Station station in stations)
            {
                if (station.Offer != null && station.Offer.Sold) continue;
                float distance = (station.Position - owner.Simulation.Champion.Position).sqrMagnitude;
                if (distance <= station.Reach * station.Reach && distance <= closest) { closest = distance; Nearby = station; }
            }
            if (owner.Simulation.Phase == RunPhase.Paused) { voice.Advance(dialogue, Special, owner.PresentationClock, true); return; }
            if (Nearby != audioFocus)
            {
                if (Nearby != null)
                {
                    CheckpointAudio.PlayInterfaceId(audioFocus == null ? "shop.approach" : "shop.focus");
                    if (Nearby.Offer != null) CheckpointAudio.PlayWorldId("shop.ticket.sway", Nearby.Position);
                    if (voice != null) voice.Gesture("hmm", Special);
                }
                audioFocus = Nearby;
            }
            dialogue.Observe(Special, Nearby == null ? null : Nearby.Offer != null ? "offer:" + Nearby.Offer.Token : Nearby.Action, owner.PresentationClock);
            if (merchant != null) merchant.Inspect(Nearby != null);
            MergePreview.Observe(Nearby, false);
            voice.Advance(dialogue, Special, owner.PresentationClock, NightmareHaunting.Quiet);
            if (merchant != null) merchant.Speak(!NightmareHaunting.Quiet && dialogue.IsSpeaking(owner.PresentationClock));
            foreach (Station station in stations)
            {
                float target = station == Nearby && !owner.HallExitPending ? 1 : 0;
                if (station.Actor != null) station.Actor.Inspect(target > 0);
                if (target > 0 && station.Focus < 0.02f) station.EnteredAt = owner.PresentationClock;
                station.Focus = Mathf.SmoothDamp(station.Focus, target, ref station.FocusVelocity, 0.09f);
                bool reduced = owner.Profile != null && owner.Profile.ReducedMotion;
                float denied = Mathf.Clamp01((station.DeniedUntil - owner.PresentationClock) / 0.65f);
                float shake = reduced ? 0 : Mathf.Sin(owner.PresentationClock * 55) * denied * 0.1f;
                // Focus animates merchandise, filigree and UI; the stand and doorway stay on the floor.
                station.Marker.localScale = Vector3.one;
                station.Marker.localPosition = station.Position + new Vector2(shake, 0);
                foreach (SpriteRenderer renderer in station.Renderers)
                    if (renderer.gameObject.name != "ShopFloorContact")
                    {
                        float opacity = renderer.color.a;
                        Color tint = Color.Lerp(station.Offer != null && station.Offer.Sold ? new Color(0.45f, 0.45f, 0.45f, 0.7f) : Color.white, new Color(1, 0.36f, 0.3f), denied);
                        tint.a = opacity; renderer.color = tint;
                    }
            }
        }
        public bool Interact()
        {
            if (owner.SecretRoom != null && owner.SecretRoom.BlocksShop) return false;
            OutsideStageChoice challenge = owner.GetComponent<OutsideStageChoice>();
            if (challenge != null && challenge.Focused) return false;
            Display();
            if (owner.Simulation.Phase != RunPhase.Rest || Nearby == null || owner.HallExitPending || owner.TransitionPending || Animating || leavingSpecial) return false;
            Station station = Nearby; Vector2 position = station.Position; Sprite icon = station.Icon;
            bool completed;
            RunSnapshot before = null;
            if (station.Offer != null)
            {
                before = MergeReveal.Capture(owner.Session.State);
                completed = owner.BuyOffer(station.Offer.Token);
                if (completed)
                {
                    CheckpointAudio.PlayInterfaceId(station.Currency == ShopCurrency.Gold ? "shop.buy.gold" : "shop.buy.dream");
                    CheckpointAudio.PlayInterfaceId("shop.ticket.sold", 0.07f);
                    PlayBondUnlocks(before);
                    if (station.Offer.Kind == OfferKind.Companion) owner.PlayCommittedArrival(before, position);
                    else ItemPickup.Play(icon, position + Vector2.up * 0.9f, owner.Content.Item(station.Offer.Id).Rarity);
                    Reveal.EnqueueCommitted(before, owner.Session.State, true, position, false);
                }
            }
            else if (station.Action == "NEXT HALL")
            {
                if (Special)
                {
                    leavingSpecial = true; specialDepartureAt = owner.PresentationClock + MarketMotion.HandoffDuration;
                    Depart(); dialogue.Speak(true, MerchantMoment.Leave, owner.PresentationClock); return true;
                }
                bool exit = owner.BeginHallExit(position);
                if (exit) dialogue.Speak(Special, MerchantMoment.Leave, owner.PresentationClock);
                return exit;
            }
            else if (station.Action == "REROLL")
            {
                completed = Special ? owner.RerollAltar() : owner.RerollShop();
                if (completed)
                {
                    foreach (Station old in stations)
                        if (old.Offer != null && !old.Offer.Sold && old.Actor != null)
                            ShopStockRetirement.Play(owner, old.Actor.View, old.Offer.Kind == OfferKind.Item ? owner.Content.Item(old.Offer.Id).Rarity : owner.Content.Unit(old.Offer.Id).Rarity);
                    arrivingStock = true;
                }
            }
            else if (station.Recipe != null)
            {
                before = MergeReveal.Capture(owner.Session.State);
                completed = owner.FuseAltar(station.Recipe);
                if (completed) { owner.PlayCommittedArrival(before, position); PlayBondUnlocks(before); }
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
                if (station.Offer != null && station.Offer.Kind == OfferKind.Item)
                {
                    ItemRarity rarity = owner.Content.Item(station.Offer.Id).Rarity;
                    if (rarity != ItemRarity.Common) CheckpointAudio.PlayInterfaceId(rarity == ItemRarity.Mythic ? "up.rarity.gold" : rarity == ItemRarity.Rare ? "up.rarity.purple" : "up.rarity.blue", 0.09f);
                }
                if (station.Offer != null || station.Recipe != null || station.Action == "REROLL")
                    dialogue.Speak(Special, station.Recipe != null ? MerchantMoment.Fuse : station.Action == "REROLL" ? MerchantMoment.Reroll : station.Currency == ShopCurrency.Health ? MerchantMoment.BloodBuy : MerchantMoment.Buy, owner.PresentationClock);
                if (merchant != null) merchant.Celebrate();
                if (voice != null) voice.Gesture(station.Recipe != null ? "gasp" : "laugh", Special);
                if (result != null) { result.ReactUntil = owner.PresentationClock + 0.8f; if (result.Actor != null) result.Actor.Celebrate(); }
            }
            else
            {
                dialogue.Speak(Special, MerchantMoment.Denied, owner.PresentationClock);
                CheckpointAudio.PlayInterfaceId(!string.IsNullOrEmpty(owner.SaveError) ? "ui.error" : station.Recipe != null ? "shop.invalid" : station.Currency == ShopCurrency.Eggs ? "shop.deny.egg" : station.Currency == ShopCurrency.Health ? "shop.deny.hp" : "shop.deny.gold");
                if (voice != null) voice.Gesture("hmm", Special);
                if (merchant != null) merchant.Reject();
                DeniedCount++;
                if (result != null)
                {
                    result.DeniedUntil = owner.PresentationClock + 0.65f;
                    result.Denial = !string.IsNullOrEmpty(owner.SaveError) ? "SAVE FAILED" : owner.Session.RecruitQuotaBlocks(station.Offer) ? owner.Session.RecruitQuotaDenialFor(station.Offer) : station.Recipe != null ? "MISSING MATERIALS" : station.Currency == ShopCurrency.Eggs ? "NOT ENOUGH EGGS" : station.Currency == ShopCurrency.Health ? "NOT ENOUGH HP" : "NOT ENOUGH GOLD";
                }
            }
            return completed;
        }
        private void PlayBondUnlocks(RunSnapshot before)
        {
            if (before == null) return;
            int highest = 0;
            foreach (BondDefinition bond in owner.Content.Bonds)
            {
                int previous = 0;
                foreach (RosterEntry entry in before.Roster)
                {
                    UnitDefinition unit = owner.Content.Unit(entry.UnitId);
                    if (unit != null && System.Array.IndexOf(unit.Tags, bond.Id) >= 0) previous++;
                }
                int current = owner.Session.BondCounts.TryGetValue(bond.Id, out int count) ? count : 0;
                if (bond.ActiveLevel(current) > bond.ActiveLevel(previous)) highest = Mathf.Max(highest, bond.ActiveLevel(current));
            }
            if (highest > 0) CheckpointAudio.PlayInterfaceId(highest >= 2 ? "up.bond.t2" : "up.bond.t1", 0.18f);
        }

        private void Build()
        {
            CheckpointAudio.CancelScheduledPrefix("shop.item.");
            CheckpointAudio.CancelScheduledPrefix("shop.stand.");
            CheckpointAudio.CancelScheduledPrefix("up.rarity.");
            audioFocus = null;
            if (scenery != null) { scenery.SetActive(false); Destroy(scenery); }
            scenery = new GameObject("NightMarketStations"); scenery.transform.SetParent(transform, false); stations.Clear();
            RunCatalog catalog = owner.Content; RunSnapshot state = owner.Session.State;
            bool arrival = arrivingStock || displayedWave != state.Wave;
            pedestalEntrance = arrival;
            bool merchantEntrance = displayedWave != state.Wave || displayedSpecial != Special || dialogue.CurrentLine == null;
            arrivingStock = false; displayedWave = state.Wave;
            displayedSpecial = Special;
            K8ArenaScenery.CreateMarketFloor(scenery.transform, Special);
            Sprite[] merchantFrames = Special ? art.EggMerchant : art.GoldMerchant;
            bool giant = true;
            Sprite[] giantFrames = Special ? art.GiantEggMerchant : art.GiantGoldMerchant;
            if (giant && giantFrames != null && giantFrames.Length == 4) merchantFrames = giantFrames;
            MerchantIdentity = giant ? Special ? "GiantEggMerchant" : "GiantGoldMerchant" : Special ? "EggOracle" : "ReceiptMaw";
            MerchantHeight = MerchantSize(Special, state.Wave);
            dialogue.MerchantName = Special ? "MOTHER OF A THOUSAND EGG-EYES" : "THE SHOPKEEPER";
            MerchantCenter = new Vector2(0, Mathf.Min(1.6f + MerchantHeight * 0.18f, owner.Content.RestViewHeight - MerchantHeight * 0.5f - 0.45f));
            merchant = AddActor(scenery.transform, merchantFrames[0], merchantFrames, MerchantCenter, MerchantHeight, 0.7f);
            merchant.SetMerchantMood(Special);
            merchant.SetMerchantIdentity(MerchantIdentity);
            if (merchantEntrance)
            {
                merchant.Arrive(MarketMotion.MerchantArrivalDelay); MerchantEntranceEffect.Play(owner, scenery.transform, merchant.transform.localPosition, Special);
                dialogue.ResetFocus(); dialogue.Speak(Special, MerchantMoment.Enter, owner.PresentationClock);
                CheckpointAudio.PlayInterfaceId(Special ? "shop.keeper.enter.dream" : "shop.keeper.enter.gold");
            }
            List<ShopOffer> offers = Special ? state.SpecialOffers : state.Offers;
            for (int index = 0; index < offers.Count; index++)
            {
                ShopOffer offer = offers[index];
                string name = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).DisplayName : catalog.Item(offer.Id).Name;
                Sprite icon = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).Body : catalog.Item(offer.Id).Icon;
                Sprite[] frames = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).AnimationFrames : null;
                float stockRow = Mathf.Min(-1.1f, MerchantCenter.y - MerchantHeight * 0.5f - 2.05f);
                Station station = AddStation(new Vector2((index - 1) * 3.2f, stockRow), name, offer.Sold ? null : icon, frames, true);
                station.Offer = offer; station.Price = offer.Price; station.Currency = offer.Currency;
                if (station.Actor != null) station.Actor.SetCompanionDisplay(offer.Kind == OfferKind.Companion);
                station.Description = offer.Kind == OfferKind.Companion ? CompanionDescriptions.Describe(catalog.Unit(offer.Id), offer.Tier, catalog.TierSupportMultiplier) : RelicDescriptions.Describe(catalog.Item(offer.Id), owner.Session.StackCount(offer.Id) + 1, catalog) + RelicDescriptions.PurchaseReactions(owner.Session, catalog, catalog.Item(offer.Id));
                if (!offer.Sold && arrival && station.Actor != null) station.Actor.Arrive(MarketMotion.StockArrivalDelay + index * MarketMotion.StockStagger, scenery.transform.TransformPoint(MerchantCenter));
                if (!offer.Sold && arrival)
                {
                    float landingAt = MarketMotion.StockArrivalDelay + index * MarketMotion.StockStagger + 0.55f;
                    CheckpointAudio.PlayInterfaceId(offer.Kind == OfferKind.Companion ? "shop.stand.appear" : "shop.item.pop", landingAt);
                    if (offer.Kind == OfferKind.Item)
                    {
                        ItemRarity rarity = catalog.Item(offer.Id).Rarity;
                        if (rarity != ItemRarity.Common) CheckpointAudio.PlayInterfaceId(rarity == ItemRarity.Mythic ? "up.rarity.gold" : rarity == ItemRarity.Rare ? "up.rarity.purple" : "up.rarity.blue", landingAt + 0.07f);
                    }
                }
                if (offer.Currency == ShopCurrency.Health) station.Description += " · Costs current HP, not maximum HP; must leave at least 1 HP.";
                ItemRarity offerRarity = offer.Kind == OfferKind.Item ? catalog.Item(offer.Id).Rarity : catalog.Unit(offer.Id).Rarity;
                if (!offer.Sold && offerRarity != ItemRarity.Common)
                {
                    var glow = new GameObject("ItemRarityAura"); glow.transform.SetParent(station.Marker, false);
                    glow.AddComponent<ShopRarityGlow>().Configure(offerRarity, owner, index * 1.1f, station.Actor.View);
                }
                if (offer.Sold)
                    foreach (SpriteRenderer renderer in station.Renderers) if (renderer.gameObject.name != "ShopFloorContact") renderer.color = new Color(0.45f, 0.45f, 0.45f, 0.7f);
            }
            Station reroll = AddStation(new Vector2(-5.3f, -1.5f), "REROLL", art.RerollMachine[0], art.RerollMachine, false);
            reroll.Price = owner.Session.RerollCost; reroll.Description = "Refresh stock. Starts at 2 gold each wave.\n" + owner.Session.RecruitmentSummary;
            Station exit = AddStation(ExitPosition, "NEXT HALL", art.HallDoor[0], art.HallDoor, false);
            exit.Reach = ExitInteractionRadius; exit.Description = "The way onward is at the bottom centre. Its broad entrance includes the floating threshold.";
            if (merchantEntrance)
                foreach (Station fixture in stations)
                    if (fixture.Offer == null && fixture.Actor != null) fixture.Actor.Arrive(0.45f);
            if (Special && state.Wave >= 4)
                for (int index = 0; index < catalog.Recipes.Length; index++)
                {
                    AltarRecipe recipe = catalog.Recipes[index];
                    if (!owner.Session.CanAltar(recipe.Id)) continue;
                    Station station = AddStation(new Vector2(-6.3f, -3.9f), "FUSE " + catalog.Unit(recipe.ResultUnit).DisplayName, catalog.Unit(recipe.ResultUnit).Body, catalog.Unit(recipe.ResultUnit).AnimationFrames, true);
                    station.Recipe = recipe.Id; station.Price = recipe.EggCost; station.Currency = ShopCurrency.Eggs;
                    station.Description = owner.Session.RecipePreview(recipe.Id);
                    break;
                }
            lifecycle = scenery.AddComponent<MarketLifecycle>(); lifecycle.Configure(owner, Special, merchantEntrance);
        }
        public void Depart()
        {
            if (voice != null) voice.Gesture("farewell", Special);
            if (lifecycle != null) lifecycle.Depart();
            int stock = 0;
            foreach (ShopDisplayActor actor in scenery.GetComponentsInChildren<ShopDisplayActor>())
                if (actor.enabled) actor.Depart(scenery.transform.TransformPoint(MerchantCenter), actor == merchant ? MarketMotion.MerchantDepartureDelay : stock++ * 0.07f);
            Nearby = null;
        }
        public void CancelDeparture()
        {
            if (lifecycle != null) lifecycle.Reenter();
            foreach (ShopDisplayActor actor in scenery.GetComponentsInChildren<ShopDisplayActor>())
                if (actor.enabled) actor.Arrive(0);
        }
        public string DescribeOffer(ShopOffer offer)
        {
            if (offer == null) return "";
            return ShortOfferCopy.PurchaseDelta(owner.Session, owner.Content, offer) + (offer.Currency == ShopCurrency.Health ? "\nPay current HP. Leave at least 1 HP" : "");
        }
        public string DescribeOfferDetails(ShopOffer offer)
        {
            if (offer == null) return "";
            RunCatalog catalog = owner.Content;
            string result;
            if (offer.Kind == OfferKind.Item)
            {
                ItemDefinition item = catalog.Item(offer.Id);
                int held = owner.Session.State.Items.Find(stack => stack.Id == item.Id)?.Count ?? 0;
                result = RelicDescriptions.Describe(item, held + 1, catalog) + RelicDescriptions.PurchaseReactions(owner.Session, catalog, item);
            }
            else
            {
                UnitDefinition unit = catalog.Unit(offer.Id);
                int copies = owner.Session.State.Roster.FindAll(entry => entry.UnitId == unit.Id && entry.Tier == 1).Count;
                result = CompanionDescriptions.Describe(unit, offer.Tier, catalog.TierSupportMultiplier) + "\nRandom weapon · " + (ShopMergePreview.Ready(owner.Session, offer) ? "Buy: tier " + ShopMergePreview.ProjectedTier(owner.Session, offer) : copies + "/3 copies");
                foreach (string tag in unit.Tags)
                    foreach (BondDefinition bond in catalog.Bonds)
                        if (bond.Id == tag) result += " · " + bond.Name;
            }
            if (offer.Currency == ShopCurrency.Health) result += "\nCosts HP · leaves at least 1";
            return result;
        }
        public void DrawMerchantDialogue()
        {
            if (scenery != null && scenery.activeSelf && !owner.HallExitPending && !NightmareHaunting.Quiet) dialogue.Draw(Special, owner.PresentationClock, owner.Profile != null && owner.Profile.ReducedMotion);
        }
        public string DisplayName(Station station)
        {
            if (station.Action == "REROLL") return "REROLL";
            if (station.Action == "NEXT HALL") return Special ? "HEAD DOWN · GOLD SHOP" : "HEAD DOWN · NEXT WAVE";
            if (station.Action == "GOLD SHOP") return "GOLD SHOP";
            if (station.Action == "DREAM ALTAR") return "DREAM SHOP";
            if (station.Recipe != null) return "FUSION · " + station.Action.Substring(5);
            return station.Action;
        }
        public void ResolveCompanionPositions()
        {
            if (scenery == null || !scenery.activeSelf || owner.Simulation.Phase != RunPhase.Rest) return;
            foreach (UnitState unit in owner.Simulation.Units)
            {
                if (unit.Faction != Faction.Companion) continue;
                if (unit.MountId > 0) continue;
                if (MergePreview.Destination(unit, out Vector2 mergeDestination))
                {
                    Vector2 destination = owner.Simulation.Champion.Position + Vector2.ClampMagnitude(mergeDestination - owner.Simulation.Champion.Position, owner.Session.CombatCatalog.CompanionLeash);
                    unit.Position = Vector2.MoveTowards(unit.PreviousPosition, destination, Mathf.Max(1.5f, unit.Definition.Speed * 0.6f) * Time.fixedDeltaTime);
                    unit.Velocity = (unit.Position - unit.PreviousPosition) / Time.fixedDeltaTime;
                    if (unit.Velocity.sqrMagnitude > 0.001f) unit.Facing = unit.Velocity.normalized;
                    continue;
                }
                Vector2 position = AvoidObstacle(unit.Position, MerchantCenter, Mathf.Min(2.2f, MerchantHeight * 0.45f), unit.Id);
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
            position += Vector2.up * LayoutLift;
            var marker = new GameObject(action); marker.transform.SetParent(scenery.transform, false); marker.transform.localPosition = position;
            ShopDisplayActor support = null;
            if (pedestal)
            {
                CarnivalStageAssets stage = Resources.Load<CarnivalStageAssets>("CarnivalStageAssets");
                if (stage != null && stage.Stands.Length == 12)
                {
                    int variant = Special ? 1 : stations.Count % 2 == 0 ? 0 : 2;
                    var poses = new Sprite[4]; System.Array.Copy(stage.Stands, variant * 4, poses, 0, 4);
                    ShopDisplayActor stand = AddActor(marker.transform, poses[0], poses, new Vector2(0, -0.1f), 0.85f, stations.Count * 0.9f);
                    stand.SetPedestalDisplay();
                    support = stand;
                    if (pedestalEntrance) stand.Arrive(0.7f + stations.Count * MarketMotion.StockStagger);
                }
                else { support = AddActor(marker.transform, art.ShopFixtures[Special ? 1 : 0], null, new Vector2(0, -0.1f), 0.85f, 0); support.enabled = false; }
            }
            ShopDisplayActor actor = sprite == null ? null : AddActor(marker.transform, sprite, frames, pedestal ? new Vector2(0, 0.38f) : Vector2.zero, pedestal ? 0.8f : action == "NEXT HALL" ? 1.65f : 1.25f, stations.Count * 0.93f);
            if (pedestal && actor != null) actor.MountMerchandise(support);
            if (!pedestal && actor != null) actor.SetFloorFixture();
            if (action == "NEXT HALL" && actor != null && OctoberArtwork.Current != null) actor.gameObject.AddComponent<OctoberDoor>().Configure(owner, actor.View);
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
        private readonly System.Collections.Generic.Dictionary<int, float> soldAt = new System.Collections.Generic.Dictionary<int, float>();
        private GUIStyle soldStyle;
        private void DrawSoldTicket(Camera camera, Station station, float age)
        {
            Vector3 screen = camera.WorldToScreenPoint(station.Marker.position + Vector3.down * 0.65f);
            Vector3 point = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y, 0));
            bool reduced = PresentationSettings.ReducedMotion;
            float fall = reduced ? 0 : Mathf.Max(0, age - 0.45f) * Mathf.Max(0, age - 0.45f) * 900f;
            float alpha = Mathf.Clamp01((1.1f - age) / 0.35f);
            Rect ticket = new Rect(point.x - 62, point.y - 20 + fall, 124, 40);
            Matrix4x4 unrotated = GUI.matrix;
            GUIUtility.RotateAroundPivot(reduced ? 0 : Mathf.Min(age, 0.6f) * 25f, new Vector2(ticket.x, ticket.y));
            Color previous = GUI.color; GUI.color = new Color(1, 1, 1, alpha);
            ArcadeSkin.Ticket(ticket, ArcadeSkin.Cream, ArcadeSkin.Gold);
            float slam = reduced ? 1 : Mathf.Lerp(1.8f, 1f, Mathf.Clamp01(age / 0.12f));
            GUIUtility.ScaleAroundPivot(Vector2.one * slam, ticket.center);
            GUIUtility.RotateAroundPivot(-12, ticket.center);
            if (soldStyle == null) { soldStyle = ArcadeTypography.Style(26, true); soldStyle.alignment = TextAnchor.MiddleCenter; soldStyle.normal.textColor = ArcadeSkin.Danger; if (ArcadeTypography.Display != null) soldStyle.font = ArcadeTypography.Display; }
            ArcadeSkin.Line(new Rect(ticket.x + 8, ticket.y + 6, ticket.width - 16, 3), ArcadeSkin.Danger);
            ArcadeSkin.Line(new Rect(ticket.x + 8, ticket.yMax - 9, ticket.width - 16, 3), ArcadeSkin.Danger);
            GUI.Label(ticket, "SOLD", soldStyle);
            GUI.color = previous;
            GUI.matrix = unrotated;
        }

        public void DrawPriceTags(Camera camera)
        {
            if (scenery == null || !scenery.activeSelf || owner.HallExitPending || Animating || leavingSpecial) return;
            if (priceStyle == null)
            {
                priceStyle = ArcadeTypography.Style(25, true); priceStyle.normal.textColor = ArcadeSkin.Ink; priceStyle.alignment = TextAnchor.MiddleLeft; priceStyle.wordWrap = false;
                categoryStyle = ArcadeTypography.Style(16, true); categoryStyle.normal.textColor = ArcadeSkin.Ink; categoryStyle.alignment = TextAnchor.MiddleCenter; categoryStyle.wordWrap = false;
                denialStyle = ArcadeTypography.Style(20, true); denialStyle.alignment = TextAnchor.MiddleCenter; denialStyle.normal.textColor = new Color(1, 0.45f, 0.35f);
            }
            int order = 0;
            foreach (Station station in stations)
            {
                order++;
                if (station.Action == "NEXT HALL" || station.Action == "GOLD SHOP" || station.Action == "DREAM ALTAR") continue;
                if (station.Offer != null && station.Offer.Sold)
                {
                    if (!soldAt.ContainsKey(station.Offer.Token)) soldAt[station.Offer.Token] = Time.unscaledTime;
                    float soldAge = Time.unscaledTime - soldAt[station.Offer.Token];
                    if (soldAge > 1.1f) continue;
                    DrawSoldTicket(camera, station, soldAge);
                    continue;
                }
                Vector3 screen = camera.WorldToScreenPoint(station.Marker.position + Vector3.down * (station.Action == "REROLL" ? 0.83f : 0.65f));
                Vector3 point = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y, 0));
                float scale = 1 + station.Focus * 0.09f + BeatPulse.Kick * 0.035f;
                string price = station.Price.ToString();
                float ticketWidth = Mathf.Clamp(118 + Mathf.Max(0, price.Length - 4) * 7, 118, 170) * scale;
                Rect ticket = new Rect(point.x - ticketWidth * 0.5f, point.y - 20 * scale, ticketWidth, 40 * scale);
                ticket.y = Mathf.Min(ticket.y, 778);
                bool denied = owner.PresentationClock < station.DeniedUntil;
                // Hanging ticket: a short string from a hook above, gentle sway, a hard shake when the purchase is denied.
                bool reduced = PresentationSettings.ReducedMotion;
                Vector2 hook = new Vector2(point.x, ticket.y - (station.Offer != null ? 44 : 18));
                float sway = reduced ? 0 : Mathf.Sin(Time.unscaledTime * 1.7f + order * 1.3f) * (2.5f + station.Focus * 3f);
                if (denied && !reduced) sway += Mathf.Sin(Time.unscaledTime * 48f) * 9f * Mathf.Clamp01((station.DeniedUntil - owner.PresentationClock) / 0.5f);
                Matrix4x4 unrotated = GUI.matrix;
                GUIUtility.RotateAroundPivot(sway, hook);
                ArcadeSkin.Line(new Rect(hook.x - 1, hook.y - 10, 2, ticket.y - hook.y + 12), new Color(ArcadeSkin.Ink.r, ArcadeSkin.Ink.g, ArcadeSkin.Ink.b, 0.85f));
                ArcadeSkin.Line(new Rect(hook.x - 4, hook.y - 14, 8, 8), ArcadeSkin.Gold);
                ArcadeSkin.Ticket(ticket, denied ? new Color(1, 0.48f, 0.4f) : ArcadeSkin.Cream, station.Currency == ShopCurrency.Eggs ? ArcadeSkin.Teal : new Color(1, 0.48f, 0.36f));
                Sprite currency = station.Currency == ShopCurrency.Eggs ? owner.Content.EggSprite : station.Currency == ShopCurrency.Health ? owner.Content.HeartSprite : owner.Content.Arena.Coin;
                RunHud.Sprite(new Rect(ticket.x + 10, ticket.y + 7, 26, 26), currency);
                priceStyle.fontSize = 25;
                while (priceStyle.fontSize > 12 && priceStyle.CalcSize(new GUIContent(price)).x > ticket.width - 50) priceStyle.fontSize--;
                priceStyle.normal.textColor = station.Offer == null || owner.Session.CanBuy(station.Offer) ? ArcadeSkin.Ink : new Color(0.75f, 0.15f, 0.2f);
                GUI.Label(new Rect(ticket.x + 44, ticket.y + 3, ticket.width - 50, 34), price, priceStyle);
                priceStyle.normal.textColor = ArcadeSkin.Ink;
                if (station.Offer != null)
                {
                    bool merge = ShopMergePreview.Ready(owner.Session, station.Offer);
                    bool item = station.Offer.Kind == OfferKind.Item;
                    Color tagColor = item ? ArcadeSkin.RarityColor(owner.Content.Item(station.Offer.Id).Rarity) : owner.Content.Unit(station.Offer.Id).Rarity != ItemRarity.Common ? ArcadeSkin.RarityColor(owner.Content.Unit(station.Offer.Id).Rarity) : ArcadeSkin.Teal;
                    Rect category = new Rect(point.x - (merge ? 60 : 45), ticket.y - 29, merge ? 120 : 90, 26);
                    ArcadeSkin.Slant(category, tagColor);
                    if (merge)
                    {
                        GUI.Label(new Rect(category.x + 3, category.y, 77, category.height), "MERGE", categoryStyle);
                        int tier = ShopMergePreview.ProjectedTier(owner.Session, station.Offer);
                        for (int star = 0; star < tier; star++) RunHud.Sprite(new Rect(category.x + 81 + star * 11, category.y + 7, 11, 11), K8Motion.TierStar());
                    }
                    else GUI.Label(category, item ? "RELIC" : "ALLY", categoryStyle);
                }
                GUI.matrix = unrotated;
                if (denied) GUI.Label(new Rect(point.x - 140, ticket.y - 65, 280, 35), station.Denial, denialStyle);
            }
        }
    }
}
