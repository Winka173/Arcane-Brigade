using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace ArcaneBrigade
{
    /// <summary>The store behind Deluxe on phones (Google Play Billing through Unity IAP): it reads the player's price, buys the
    /// one-time product, confirms it, and asks the store again at each launch so a purchase made on another device or before a
    /// reinstall comes back, and a refund is taken away. Nothing here runs on PC, where Deluxe is included.</summary>
    public sealed class DeluxeStore : MonoBehaviour
    {
        private static DeluxeStore instance;
        private StoreController store;
        private Product product;

        /// <summary>The player's own price from the store (null until it answers).</summary>
        public static string PriceText { get; private set; }
        /// <summary>A purchase or restore is under way.</summary>
        public static bool Busy { get; private set; }
        /// <summary>The last thing the store said, in plain words, for the offer card (null when there is nothing to say).</summary>
        public static string Message { get; private set; }
        public static bool Available => instance != null && instance.product != null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (!Deluxe.Sold || instance != null) return;
            var holder = new GameObject("Deluxe store");
            DontDestroyOnLoad(holder);
            instance = holder.AddComponent<DeluxeStore>();
        }

        private async void Start()
        {
            try
            {
                store = UnityIAPServices.StoreController();
                store.OnStoreConnected += () => store.FetchProducts(new List<ProductDefinition> { new ProductDefinition(Deluxe.ProductId, ProductType.NonConsumable) });
                store.OnStoreDisconnected += failure => { Busy = false; };
                store.OnProductsFetched += products =>
                {
                    product = products.FirstOrDefault(p => p?.definition?.id == Deluxe.ProductId);
                    PriceText = product?.metadata?.localizedPriceString;
                    store.FetchPurchases(); // what this account already owns
                };
                store.OnProductsFetchFailed += failure => { };
                store.OnPurchasesFetched += orders =>
                {
                    bool owned = orders.ConfirmedOrders.Any(Holds) || orders.PendingOrders.Any(Holds);
                    foreach (PendingOrder pending in orders.PendingOrders.Where(Holds)) store.ConfirmPurchase(pending);
                    Deluxe.Grant(owned); // a refunded purchase no longer shows here
                };
                store.OnPurchasesFetchFailed += failure => { };
                store.OnPurchasePending += order =>
                {
                    if (Holds(order)) { Deluxe.Grant(true); Message = "Deluxe is yours. Thank you!"; }
                    store.ConfirmPurchase(order);
                    Busy = false;
                };
                store.OnPurchaseConfirmed += order => { Busy = false; };
                store.OnPurchaseFailed += failed => { Busy = false; Message = "The purchase did not go through."; };
                store.OnPurchaseDeferred += deferred => { Busy = false; Message = "Waiting for the payment to clear."; };
                await store.Connect();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Deluxe store unavailable: " + exception.Message);
            }
        }

        private static bool Holds(Order order) => order?.CartOrdered?.Items()?.Any(item => item?.Product?.definition?.id == Deluxe.ProductId) == true;

        public static void Buy()
        {
            if (instance?.store == null || instance.product == null) { Message = "The store is not available right now."; return; }
            if (Busy) return;
            Busy = true; Message = null;
            try { instance.store.PurchaseProduct(instance.product); }
            catch (Exception) { Busy = false; Message = "The purchase did not go through."; }
        }

        public static void Restore()
        {
            if (instance?.store == null) { Message = "The store is not available right now."; return; }
            if (Busy) return;
            Busy = true; Message = null;
            instance.store.RestoreTransactions((ok, error) =>
            {
                Busy = false;
                instance.store.FetchPurchases();
                Message = !ok ? "Could not reach the store." : Deluxe.Owned ? "Deluxe restored." : "No Deluxe purchase on this account.";
            });
        }
    }
}
