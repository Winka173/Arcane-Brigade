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
        }

        private ArenaController owner;
        private GameObject scenery;
        private int signature = int.MinValue;
        private readonly List<Station> stations = new List<Station>();
        public IReadOnlyList<Station> Stations => stations;
        public bool Special { get; private set; }
        public Station Nearby { get; private set; }
        public void Configure(ArenaController controller) { owner = controller; }

        public void Display()
        {
            if (owner.Session.State.Phase != SessionPhase.Shop)
            {
                if (scenery != null) scenery.SetActive(false);
                Nearby = null;
                Special = false;
                signature = int.MinValue;
                return;
            }
            RunSnapshot state = owner.Session.State;
            if (state.SpecialOffers.Count == 0) Special = false;
            int current = Special ? 7919 : 17;
            current = unchecked(current * 31 + state.SpecialOffers.Count);
            foreach (ShopOffer offer in Special ? state.SpecialOffers : state.Offers) current = unchecked(current * 31 + offer.Token * 2 + (offer.Sold ? 1 : 0));
            current = unchecked(current * 31 + state.Wave);
            if (current != signature) { Build(); signature = current; }
            scenery.SetActive(true);
            Nearby = null;
            float closest = InteractionRadius * InteractionRadius;
            foreach (Station station in stations)
            {
                float distance = (station.Position - owner.Simulation.Champion.Position).sqrMagnitude;
                if (distance <= closest) { closest = distance; Nearby = station; }
            }
            foreach (Station station in stations) station.Marker.localScale = Vector3.one * (station == Nearby ? 1.12f : 1f);
        }

        public bool Interact()
        {
            Display();
            if (owner.Simulation.Phase != RunPhase.Rest || Nearby == null) return false;
            Station station = Nearby;
            if (station.Offer != null)
            {
                if (station.Offer.Sold) return false;
                bool purchased = owner.BuyOffer(station.Offer.Token);
                Display();
                return purchased;
            }
            bool completed = true;
            if (station.Action == "NEXT HALL") { owner.NextWave(); completed = owner.Session.State.Phase == SessionPhase.Combat; }
            else if (station.Action == "REROLL") completed = owner.RerollShop();
            else if (station.Recipe != null) completed = owner.FuseAltar(station.Recipe);
            else { Special = !Special; signature = int.MinValue; }
            Display();
            return completed;
        }

        private void Build()
        {
            if (scenery != null) { scenery.SetActive(false); Destroy(scenery); }
            scenery = new GameObject("WorldShopStations");
            scenery.transform.SetParent(transform, false);
            stations.Clear();
            RunCatalog catalog = owner.Content;
            RunSnapshot state = owner.Session.State;
            Color tint = Special ? new Color(0.45f, 0.95f, 0.88f) : new Color(1f, 0.78f, 0.4f);
            AddSprite(scenery.transform, catalog.Companions[Special ? 1 : 0].Body, new Vector2(0, 3.9f), 1.9f, Color.white);
            List<ShopOffer> offers = Special ? state.SpecialOffers : state.Offers;
            for (int index = 0; index < offers.Count; index++)
            {
                ShopOffer offer = offers[index];
                string name = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).DisplayName : catalog.Item(offer.Id).Name;
                string description = offer.Kind == OfferKind.Companion ? "Random weapon on purchase. " + catalog.Unit(offer.Id).Passive : catalog.Item(offer.Id).Description + "\n" + RunModifierRules.Describe(catalog.Item(offer.Id), 1);
                if (offer.Currency == ShopCurrency.Health) description += "Trả HP HIỆN TẠI, không giảm max HP. Còn ≥1 HP trước khi nhận item.\n";
                Sprite icon = offer.Kind == OfferKind.Companion ? catalog.Unit(offer.Id).Body : catalog.Item(offer.Id).Icon;
                Color offerTint = offer.Kind == OfferKind.Item ? RunModifierRules.RarityColor(catalog.Item(offer.Id).Rarity) : tint;
                string rarity = offer.Kind == OfferKind.Item ? RunModifierRules.RarityName(catalog.Item(offer.Id).Rarity) + "\n" : "";
                Station station = AddStation(new Vector2((index - 1) * 3f, 0.5f), offer.Sold ? "SOLD" : name, rarity + offer.Price + " " + (offer.Currency == ShopCurrency.Health ? "HP HIỆN TẠI" : offer.Currency.ToString().ToUpperInvariant()), icon, offerTint);
                station.Offer = offer;
                station.Description = description;
                if (offer.Sold) foreach (SpriteRenderer renderer in station.Marker.GetComponentsInChildren<SpriteRenderer>()) renderer.color *= new Color(0.45f, 0.45f, 0.45f, 0.7f);
            }
            if (!Special) AddStation(new Vector2(0, 2.3f), "REROLL", owner.Session.RerollCost + " GOLD", catalog.Arena.Coin, tint).Description = "Đổi ba offer vàng. Đã đổi " + state.RerollsThisShop + " lần; mỗi lần tăng " + catalog.RerollPriceStep + " vàng. Không đổi offer trứng.";
            AddStation(new Vector2(7, 1), "NEXT HALL", "SPACE TO CONTINUE", catalog.Arena.Coin, tint).Description = "Finish shopping and enter the next hall.";
            if (Special || state.SpecialOffers.Count > 0)
                AddStation(new Vector2(-7, 1), Special ? "GOLD SHOP" : "DREAM ALTAR", "SPACE TO VISIT", Special ? catalog.Arena.Coin : catalog.Weapon("RuneStaff")?.ProjectileSprite, new Color(0.45f, 0.95f, 0.88f)).Description = "Altar dùng trứng hoặc HP hiện tại theo từng quầy. Đi qua không mất tài nguyên.";
            if (!Special && state.Wave >= 4)
                for (int index = 0; index < catalog.Recipes.Length; index++)
                {
                    AltarRecipe recipe = catalog.Recipes[index];
                    Station station = AddStation(new Vector2(-4 + index * 8, -3), "FUSE " + catalog.Unit(recipe.ResultUnit).DisplayName, recipe.EggCost + " EGGS + ALLIES", catalog.Unit(recipe.ResultUnit).Body, tint);
                    station.Recipe = recipe.Id;
                    station.Description = "Consume " + recipe.FirstUnit + " + " + recipe.SecondUnit + " for a two-star companion.";
                }
        }

        private Station AddStation(Vector2 position, string action, string price, Sprite sprite, Color tint)
        {
            var marker = new GameObject(action);
            marker.transform.SetParent(scenery.transform, false);
            marker.transform.localPosition = position;
            SpriteRenderer pedestal = AddSprite(marker.transform, owner.Content.Arena.Hand, new Vector2(0, -0.2f), 1.25f, new Color(0.25f, 0.17f, 0.36f));
            if (pedestal != null) { Vector3 scale = pedestal.transform.localScale; scale.y *= 0.35f; pedestal.transform.localScale = scale; }
            AddSprite(marker.transform, sprite, new Vector2(0, 0.35f), 0.72f, Color.white);
            AddText(marker.transform, action, new Vector2(0, -0.85f), 0.045f, Color.white);
            AddText(marker.transform, price, new Vector2(0, -1.17f), 0.04f, tint, TextAnchor.UpperCenter);
            var station = new Station { Position = position, Action = action, Marker = marker.transform };
            stations.Add(station);
            return station;
        }

        private static SpriteRenderer AddSprite(Transform parent, Sprite sprite, Vector2 position, float width, Color tint)
        {
            if (sprite == null) return null;
            var child = new GameObject("ShopIcon");
            child.transform.SetParent(parent, false);
            child.transform.localPosition = position;
            child.transform.localScale = Vector3.one * (width / sprite.bounds.size.x);
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint;
            renderer.sortingOrder = 300;
            return renderer;
        }

        private static void AddText(Transform parent, string text, Vector2 position, float size, Color tint, TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var child = new GameObject("ShopLabel");
            child.transform.SetParent(parent, false);
            child.transform.localPosition = position;
            TextMesh mesh = child.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.font = ArcadeTypography.Body;
            mesh.GetComponent<MeshRenderer>().sharedMaterial = mesh.font.material;
            mesh.fontSize = 64;
            mesh.characterSize = size;
            mesh.anchor = anchor;
            mesh.color = tint;
            mesh.GetComponent<MeshRenderer>().sortingOrder = 2200;
        }
    }
}
