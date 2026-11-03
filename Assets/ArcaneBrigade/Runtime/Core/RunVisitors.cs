using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class VisitorEffect
    {
        public string Id = ""; public int Wave; public float Value; public string Target = "";
    }

    public sealed partial class RunSnapshot
    {
        /// <summary>Shop visitors (Docs/Events_Ideas.md): who waits in this shop, whether they were answered, the choice in
        /// progress (a two-step offer), the effects waiting for a later wave, and the shelf before the last refresh.</summary>
        public string Visitor = ""; public int VisitorWave = -1, VisitorLastWave, VisitorStage, VisitorPick = -1; public bool VisitorDone; public uint VisitorRandom;
        public int VisitorRollWave = -1, RareVisitorLastWave = -100;
        public List<VisitorEffect> VisitorEffects = new List<VisitorEffect>();
        public List<ShopOffer> ShelfBeforeRefresh = new List<ShopOffer>(); public int RefreshPaid = -1, TwinShop = -1, PromiseDiscountWave = -1;
        public List<string> Portraits = new List<string>();
    }

    public sealed class VisitorOption
    {
        public string Label; public string Icon; public bool Enabled = true; public int Value;
        /// <summary>For the card: the gold this plate pays out (a relic sold or pawned) and a friend's stars.</summary>
        public int Gold, Tier;
    }

    /// <summary>The visitors of the dream shop, run side: they arrive every 2-3 shops from wave 3 (their own seeded roll), wait
    /// at their table until answered or until the shop is left. Rare gift visitors use a separate low-probability roll.</summary>
    public sealed partial class RunSession
    {
        // requirement 25/10/2026 (the three bets were the same deal, the big one worse): a bigger bet asks the hero to keep more of
        // its health through the next wave and pays a larger share back
        private static readonly int[] DiceWin = { 8, 20, 52 }, DiceHold = { 50, 70, 85 };
        private static float DiceHoldShare(string target) => target != null && target.StartsWith("paid:") && int.TryParse(target.Substring(5), out int hold) ? hold / 100f : .5f;

        /// <summary>Which visitors may appear (painted ones in the game; checks may allow all).</summary>
        public static Func<string, bool> VisitorPainted = id => ExpansionArtwork.Current != null && ExpansionArtwork.Current.Visitor(id) != null;

        public VisitorDefinition Visitor => State.Phase == SessionPhase.Shop && !State.VisitorDone && State.VisitorWave == State.Wave && !string.IsNullOrEmpty(State.Visitor) && !SecretRoomOpen ? ShopVisitors.For(State.Visitor) : null;
        private bool VisitorActive(string id, int wave) => State.VisitorEffects.Exists(e => e.Id == id && e.Wave == wave);
        private VisitorEffect VisitorEffectFor(string id, int wave) => State.VisitorEffects.Find(e => e.Id == id && e.Wave == wave);
        /// <summary>A visitor's gold reward as fixed when the deal was struck (on the gold curve); older saves: the authored amount.</summary>
        private int Reward(string id, int wave, int authored) => VisitorEffectFor(id, wave) is VisitorEffect e && e.Value > 0 ? Mathf.RoundToInt(e.Value) : authored;
        private void AddVisitorEffect(string id, float value = 0, string target = "", int ahead = 1) => State.VisitorEffects.Add(new VisitorEffect { Id = id, Wave = State.Wave + ahead, Value = value, Target = target ?? "" });

        /// <summary>A new shop: maybe a visitor (every 2-3 shops from wave 3; never with a story offer or a boss next).</summary>
        private void RollVisitor()
        {
            if (State.VisitorRollWave == State.Wave || State.VisitorWave == State.Wave) return;
            State.VisitorRollWave = State.Wave;
            State.VisitorStage = 0; State.VisitorPick = -1; State.VisitorDone = false; State.ShelfBeforeRefresh.Clear(); State.RefreshPaid = -1;
            if (State.VisitorRandom == 0) State.VisitorRandom = unchecked((uint)State.Seed ^ 0x51B7u) | 1;
            if (State.Wave < 3 || CustomerOfferAvailable || MemorySaleOpen || RelightChoiceOpen || State.OutsideStage == OutsideStageStatus.Reward) return;
            if (TryRollRareVisitor()) return;
            int since = State.Wave - State.VisitorLastWave;
            if (since < 2 || since == 2 && Next(ref State.VisitorRandom, 2) == 0) return;
            var pool = new List<VisitorDefinition>();
            foreach (VisitorDefinition v in ShopVisitors.All) if (!RareVisitorRules.IsRare(v.Id) && v.Id != State.Visitor && VisitorPainted(v.Id) && VisitorFits(v)) pool.Add(v);
            if (pool.Count == 0) return;
            State.Visitor = pool[Next(ref State.VisitorRandom, pool.Count)].Id; State.VisitorWave = State.Wave; State.VisitorLastWave = State.Wave;
        }
        private bool VisitorFits(VisitorDefinition v) => v.Id switch
        {
            "Pawnbroker" or "Barterer" or "JunkCollector" => State.Items.Count > 0,
            "Ferryman" => State.Roster.Exists(r => r.Tier == 1 && !StorySpecies(r.UnitId)),
            "ToyMender" or "Engraver" or "FriendSeeker" or "DreamSmith" => State.Roster.Exists(r => !StorySpecies(r.UnitId)),
            "EggTrader" => State.Eggs >= 2 || State.Items.Count > 0,
            "Twins" => State.Offers.Exists(o => o.Kind == OfferKind.Companion && !o.Sold),
            _ => true
        };
        /// <summary>For checks and captures: this visitor waits in the current shop.</summary>
        public void ForceVisitor(string id) { State.Visitor = id; State.VisitorWave = State.Wave; State.VisitorDone = false; State.VisitorStage = 0; State.VisitorPick = -1; }

        /// <summary>What the visitor offers right now (a two-step offer shows its second step after the first choice).</summary>
        public List<VisitorOption> VisitorOptions()
        {
            var list = new List<VisitorOption>(); VisitorDefinition v = Visitor; if (v == null) return list;
            int cost = VisitorCost(v); bool afford = State.Gold >= cost;
            if (v.Id == "DreamSmith" && State.VisitorStage == 1 && State.VisitorPick >= 0 && State.VisitorPick < State.Roster.Count)
            {
                foreach (WeaponDefinition w in ReforgeChoices()) list.Add(new VisitorOption { Label = RunUICopy.WeaponName(w.Id), Icon = "weapon:" + w.Id });
                return list;
            }
            if (v.Id == "EggTrader" && State.VisitorStage == 1) { AddItemOptions(list); return list; }
            switch (v.Choice)
            {
                case VisitorChoice.Accept:
                    bool ok = afford && (v.Id != "ClockRewinder" || State.ShelfBeforeRefresh.Count > 0)
                        && (!RareVisitorRules.IsRare(v.Id) || RareVisitorFits(v.Id));
                    list.Add(new VisitorOption { Label = cost > 0 ? "Accept (" + cost + " gold)" : "Accept", Enabled = ok }); break;
                case VisitorChoice.Options:
                    for (int i = 0; i < v.Options.Length; i++)
                    {
                        bool enabled = v.Id switch { "DiceElder" => State.Gold >= ScaledGold(new[] { 5, 10, 20 }[i]), "NightThief" => i != 0 || State.Gold >= ScaledGold(5), "EggTrader" => i == 0 ? State.Eggs >= 2 : State.Items.Count > 0, _ => true };
                        list.Add(new VisitorOption { Label = ShopVisitors.ScaleText(v.Options[i], GoldScale), Enabled = enabled, Value = i });
                    }
                    break;
                case VisitorChoice.PickItem: AddItemOptions(list); break;
                case VisitorChoice.PickCompanion:
                    for (int i = 0; i < State.Roster.Count; i++)
                    {
                        RosterEntry r = State.Roster[i];
                        if (!ActiveRoster.Contains(r)) continue; // visitor services target fighters, not stored ingredients
                        if (StorySpecies(r.UnitId) || v.Id == "Ferryman" && r.Tier != 1) continue;
                        list.Add(new VisitorOption { Label = RunUICopy.UnitName(catalog.Unit(r.UnitId)) + " " + r.Tier + "-star", Icon = "unit:" + r.UnitId, Enabled = afford, Value = i, Tier = r.Tier });
                    }
                    break;
            }
            return list;
        }
        private void AddItemOptions(List<VisitorOption> list)
        {
            VisitorDefinition v = Visitor;
            foreach (ItemStack s in State.Items)
            {
                ItemDefinition item = catalog.Item(s.Id); if (item == null) continue;
                int gold = v.Id == "JunkCollector" ? ScaledGold(Mathf.RoundToInt(item.Price * .6f)) : v.Id == "Pawnbroker" ? ScaledGold(item.Price / 2) : 0;
                string label = item.Name + (gold > 0 || v.Id == "JunkCollector" || v.Id == "Pawnbroker" ? " (+" + gold + ")" : "");
                list.Add(new VisitorOption { Label = label, Icon = "item:" + s.Id, Value = State.Items.IndexOf(s), Gold = gold });
            }
        }

        public bool VisitorDecline()
        {
            if (Visitor == null) return false;
            State.VisitorDone = true; Notice = ""; return true;
        }

        /// <summary>The traveller answers: option index into VisitorOptions().</summary>
        public bool VisitorChoose(int index)
        {
            VisitorDefinition v = Visitor; var options = VisitorOptions();
            if (v == null || index < 0 || index >= options.Count || !options[index].Enabled) return false;
            VisitorOption o = options[index];
            float previousMaximum = MaximumHealth();
            bool done = true;
            switch (v.Id)
            {
                case "StarMender": if (!GiveVisitorStars(2)) return false; break;
                case "ThirdBellKeeper": if (!GiveVisitorStars(3)) return false; break;
                case "UnclaimedGiftKeeper": if (!GiveVisitorRelic()) return false; break;
                case "GateKeeper": AddVisitorEffect("Dare"); break;
                case "LostPostman": AddVisitorEffect("Letter", ScaledGold(15)); break;
                // the bet is taken now; the effect keeps what the elder pays back (the card's GET), both on the gold curve
                case "DiceElder": { int bet = ScaledGold(new[] { 5, 10, 20 }[o.Value]); State.Gold -= bet; AddVisitorEffect("Bet", ScaledGold(DiceWin[o.Value]), "paid:" + DiceHold[o.Value]); break; }
                case "Pawnbroker":
                {
                    ItemStack s = State.Items[o.Value]; ItemDefinition item = catalog.Item(s.Id); int price = ScaledGold(item.Price / 2);
                    RemoveOneItem(s); State.Gold = BoundedCounter((long)State.Gold + price);
                    AddVisitorEffect("Pawn", price, item.Id, 2); Notice = item.Name + " pawned for " + price + " gold; redeem it within 2 waves.";
                    break;
                }
                case "ParcelCourier":
                {
                    State.Gold -= VisitorCost(v);
                    int roll = Next(ref State.VisitorRandom, 4);
                    if (roll == 0) { int found = ScaledGold(15); State.Gold = BoundedCounter((long)State.Gold + found); Notice = "The parcel held " + found + " gold."; }
                    else if (roll == 1) { State.Eggs = BoundedCounter((long)State.Eggs + 1); Notice = "The parcel held a pinch of dreamsand."; }
                    else if (roll == 2 && GiveRandomItem(ItemRarity.Common, false) is ItemDefinition got) Notice = "The parcel held " + got.Name + ".";
                    else Notice = "Empty, except a note: 'Thank you for opening me.'";
                    break;
                }
                case "TroupeLeader": AddVisitorEffect("Play", o.Value); break;
                case "Lamplighter": State.Gold -= VisitorCost(v); AddVisitorEffect("Lamps"); break;
                case "LostToy": AddVisitorEffect("Toy"); break;
                case "ToyMender": State.Gold -= VisitorCost(v); AddVisitorEffect("Mended", 0, State.Roster[o.Value].UnitId); break;
                case "MusicBoxKeeper": AddVisitorEffect("Tempo"); break;
                case "Barterer":
                {
                    ItemStack s = State.Items[o.Value]; ItemDefinition item = catalog.Item(s.Id);
                    var pool = new List<ItemDefinition>();
                    foreach (ItemDefinition c in catalog.Items) if (c.Rarity == item.Rarity && c.Id != item.Id && c.ShopWeight > 0 && Eligible(c, ShopPool.Gold) && HeroFits(c) && StackCount(c.Id) < c.StackLimit) pool.Add(c);
                    if (pool.Count == 0) return false;
                    ItemDefinition swap = pool[Next(ref State.VisitorRandom, pool.Count)];
                    RemoveOneItem(s); AddItem(swap); Notice = item.Name + " traded for " + swap.Name + ".";
                    break;
                }
                case "JunkCollector":
                {
                    ItemStack s = State.Items[o.Value]; ItemDefinition item = catalog.Item(s.Id); int price = ScaledGold(Mathf.RoundToInt(item.Price * .6f));
                    RemoveOneItem(s); State.Gold = BoundedCounter((long)State.Gold + price); Notice = item.Name + " sold for " + price + " gold.";
                    break;
                }
                case "Lender":
                {
                    if (GiveRandomItem(ItemRarity.Rare, true) is ItemDefinition lent) { AddVisitorEffect("Lent", 0, lent.Id); Notice = lent.Name + " lent to you for the next wave."; }
                    break;
                }
                case "Storyteller": AddVisitorEffect("Story"); Notice = StoryPage(); break;
                case "SwapClown": RestockShelf(+1, 1.3f); break;
                case "ClockRewinder":
                {
                    State.Gold -= VisitorCost(v);
                    State.Offers.Clear(); State.Offers.AddRange(State.ShelfBeforeRefresh); State.ShelfBeforeRefresh.Clear();
                    if (State.RefreshPaid > 0) State.Gold = BoundedCounter((long)State.Gold + State.RefreshPaid);
                    State.RefreshPaid = -1; Notice = "The shelf is as it was.";
                    RetireClosedNewSpecies(); // an old shelf may hold a new friend the quota has closed since
                    break;
                }
                case "SleeplessChild": AddVisitorEffect("Lullaby"); break;
                case "CandyWitch": State.Gold -= VisitorCost(v); AddVisitorEffect("Candy"); break;
                case "BalloonSeller": State.Gold -= VisitorCost(v); AddVisitorEffect("Balloon"); break;
                case "PromiseSeller": AddVisitorEffect("Promise"); break;
                case "Ferryman":
                {
                    RosterEntry r = State.Roster[o.Value]; UnitDefinition old = catalog.Unit(r.UnitId);
                    var pool = new List<UnitDefinition>();
                    foreach (UnitDefinition u in catalog.Companions)
                        if (u.Id != r.UnitId && !u.SpecialRecruit && !u.GiftOnly && !StorySpecies(u.Id) && u.Tags.Length > 0 && old.Tags.Length > 0 && u.Tags[0] == old.Tags[0]
                            && u.UnlockWave <= State.Wave && K11HeroRules.Eligible(catalog.Unit(State.ChampionId), u)) pool.Add(u);
                    if (pool.Count == 0) return false;
                    UnitDefinition other = pool[Next(ref State.VisitorRandom, pool.Count)];
                    var weapons = new List<WeaponDefinition>();
                    foreach (WeaponDefinition w in catalog.Weapons) if (RecruitWeaponEligible(other.Id, w) && VariantFree(other.Id, w)) weapons.Add(w);
                    if (weapons.Count == 0) return false;
                    State.Roster.Remove(r);
                    var arrival = new RosterEntry { UnitId = other.Id, Tier = JoinTier(other.Id, 1), WeaponId = weapons[Next(ref State.VisitorRandom, weapons.Count)].Id };
                    State.Roster.Add(arrival); RememberVariant(arrival); MergeCompanions(other.Id);
                    Notice = RunUICopy.UnitName(old) + " crossed the river; " + RunUICopy.UnitName(other) + " came back.";
                    break;
                }
                case "MatchGirl": State.Gold -= VisitorCost(v); AddVisitorEffect("Matches"); break;
                case "Collector": AddVisitorEffect("Collect", ScaledGold(12)); break;
                case "Engraver": State.Gold -= VisitorCost(v); AddVisitorEffect("Engraved", 0, State.Roster[o.Value].UnitId); break;
                case "Waker": State.Gold -= VisitorCost(v); AddVisitorEffect("Wake"); break;
                case "DreamSeller": AddVisitorEffect("Dream", (o.Value * 7 + Next(ref State.VisitorRandom, 7)) % 7); break;
                case "Twins": State.TwinShop = State.Wave; break;
                case "WaterCarrier": State.Gold -= VisitorCost(v); AddVisitorEffect("Puddles"); break;
                case "NightThief":
                    if (o.Value == 0) { State.Gold -= ScaledGold(5); Notice = "The thief slinks away."; }
                    else { AddVisitorEffect("Thief", 0, "", 0); Notice = "Keep an eye on your purse."; }
                    break;
                case "DollDoctor": State.Gold -= VisitorCost(v); AddVisitorEffect("Cloak"); break;
                case "KeyKeeper": State.Gold -= VisitorCost(v); AddVisitorEffect("Key"); break;
                case "CupMagician":
                {
                    var shelf = State.Offers.FindAll(x => !x.Sold && x.Currency == ShopCurrency.Gold && x.Price > 0);
                    if (shelf.Count == 0) return false;
                    State.Gold -= VisitorCost(v); ShopOffer free = shelf[Next(ref State.VisitorRandom, shelf.Count)]; free.Price = 0; free.BasePrice = 0;
                    Notice = "Ta-da! One offer is free.";
                    break;
                }
                case "FriendSeeker": AddVisitorEffect("Away", ScaledGold(10), State.Roster[o.Value].UnitId); break;
                case "DreamSmith":
                    if (State.VisitorStage == 0) { State.VisitorStage = 1; State.VisitorPick = o.Value; done = false; break; }
                    {
                        RosterEntry r = State.Roster[State.VisitorPick]; var choices = ReforgeChoices(); if (index >= choices.Count) return false;
                        State.Gold -= VisitorCost(v); r.WeaponId = choices[index].Id; RememberVariant(r);
                        Notice = RunUICopy.UnitName(catalog.Unit(r.UnitId)) + " now carries " + RunUICopy.WeaponName(r.WeaponId) + ".";
                    }
                    break;
                case "MapSeller": State.Gold -= VisitorCost(v); AddVisitorEffect("Map"); break;
                case "EggTrader":
                    if (State.VisitorStage == 0)
                    {
                        if (o.Value == 0) { State.Eggs -= 2; if (GiveRandomItem(ItemRarity.Uncommon, false) is ItemDefinition got) Notice = "Traded for " + got.Name + "."; }
                        else { State.VisitorStage = 1; done = false; }
                        break;
                    }
                    { ItemStack s = State.Items[o.Value]; Notice = catalog.Item(s.Id).Name + " traded for 2 dreamsand."; RemoveOneItem(s); State.Eggs = BoundedCounter((long)State.Eggs + 2); }
                    break;
                case "WeatherTeller": State.Gold -= VisitorCost(v); AddVisitorEffect("Umbrella"); Notice = "Next wave: " + NextWaveForecast() + "."; break;
                case "Knitter": State.Gold -= VisitorCost(v); AddVisitorEffect("Scarf"); break;
                case "Painter":
                {
                    var names = new List<string>(); foreach (RosterEntry r in State.Roster) names.Add(r.UnitId);
                    State.Portraits.Add("Wave " + State.Wave + ": " + State.ChampionId + " with " + string.Join(", ", names));
                    Notice = "Your brigade's portrait is kept with this run.";
                    break;
                }
                case "ShadowBorrower": State.Gold -= VisitorCost(v); AddVisitorEffect("Shadow"); break;
                case "Sandman": State.Gold -= VisitorCost(v); AddVisitorEffect("Sand"); break;
                case "ColourStranger": RestockShelf(-1, .7f); break;
                case "FuturePost": State.Gold -= VisitorCost(v); AddVisitorEffect("Post", ScaledGold(15), "", 3); break;
                case "HourglassFixer": AddVisitorEffect("Longer"); break;
                case "Busker":
                {
                    string best = null; int gap = int.MaxValue;
                    foreach (BondDefinition b in catalog.Bonds)
                    {
                        BondCounts.TryGetValue(b.Id, out int n); if (n == 0) continue;
                        int need = b.NextThreshold(n) - n; if (need < gap) { gap = need; best = b.Id; }
                    }
                    if (best != null) { AddVisitorEffect("Song", 0, best); Notice = "Next wave " + Array.Find(catalog.Bonds, b => b.Id == best).Name + " counts one more friend."; }
                    break;
                }
            }
            if (done) State.VisitorDone = true;
            RecountBonds(); ReconcileHealth(previousMaximum);
            return true;
        }

        private List<WeaponDefinition> ReforgeChoices()
        {
            var list = new List<WeaponDefinition>(); if (State.VisitorPick < 0 || State.VisitorPick >= State.Roster.Count) return list;
            RosterEntry r = State.Roster[State.VisitorPick];
            var pool = new List<WeaponDefinition>();
            foreach (WeaponDefinition w in catalog.Weapons) if (w.Id != r.WeaponId && RecruitWeaponEligible(r.UnitId, w) && VariantFree(r.UnitId, w)) pool.Add(w);
            uint seed = unchecked(State.VisitorRandom ^ (uint)(State.VisitorPick * 7919 + State.Wave));
            for (int i = 0; i < 3 && pool.Count > 0; i++) { int k = Next(ref seed, pool.Count); list.Add(pool[k]); pool.RemoveAt(k); }
            return list;
        }
        private void RemoveOneItem(ItemStack s) { if (--s.Count <= 0) State.Items.Remove(s); }
        private void AddItem(ItemDefinition item)
        {
            ItemStack stack = State.Items.Find(x => x.Id == item.Id);
            if (stack == null) State.Items.Add(new ItemStack { Id = item.Id, Count = 1 }); else stack.Count++;
            BuyRelic(item, 0);
            RetireClosedNewSpecies(); // a gift relic may have brought a new friend and closed the quota
        }
        private ItemDefinition GiveRandomItem(ItemRarity rarity, bool lent)
        {
            var pool = new List<ItemDefinition>();
            foreach (ItemDefinition c in catalog.Items) if (c.Rarity == rarity && c.ShopWeight > 0 && Eligible(c, ShopPool.Gold) && HeroFits(c) && StackCount(c.Id) < c.StackLimit && (!lent || c.Mechanic == RelicMechanic.None || c.Stacking != StackPolicy.Unique)) pool.Add(c);
            if (pool.Count == 0) { State.Gold = BoundedCounter((long)State.Gold + ScaledGold(10)); return null; }
            ItemDefinition item = pool[Next(ref State.VisitorRandom, pool.Count)];
            AddItem(item); return item;
        }
        /// <summary>The clown and the colour stranger: every unsold relic on the shelf moves one rarity up or down.</summary>
        private void RestockShelf(int step, float priceScale)
        {
            foreach (ShopOffer offer in State.Offers)
            {
                if (offer.Sold || offer.Kind != OfferKind.Item) continue;
                ItemDefinition item = catalog.Item(offer.Id);
                ItemRarity want = (ItemRarity)Mathf.Clamp((int)item.Rarity + step, 0, (int)ItemRarity.Mythic);
                if (want == ItemRarity.Crimson) want = ItemRarity.Mythic;
                var pool = new List<ItemDefinition>();
                foreach (ItemDefinition c in catalog.Items) if (c.Rarity == want && c.ShopWeight > 0 && Eligible(c, ShopPool.Gold) && HeroFits(c) && StackCount(c.Id) < c.StackLimit) pool.Add(c);
                if (pool.Count == 0) continue;
                ItemDefinition next = pool[Next(ref State.VisitorRandom, pool.Count)];
                // the shelf's own price (rarity markup and the wave's gold curve), then the visitor's +30% / -30%
                offer.Id = next.Id; offer.BasePrice = 0; offer.Price = Mathf.Max(1, Mathf.RoundToInt(next.Price * 0.85f * (1 + (int)next.Rarity * 0.4f) * StagePriceFactor * priceScale));
            }
            Notice = step > 0 ? "The shelf glitters: fancier and pricier." : "The colours drain: humbler and cheaper.";
        }
        private string NextWaveForecast()
        {
            int wave = State.Wave + 1;
            UnitDefinition a = catalog.Enemies[(wave - 1) % catalog.Enemies.Length], b = catalog.Enemies[wave % catalog.Enemies.Length];
            return RunUICopy.UnitName(a) + (a != b ? " and " + RunUICopy.UnitName(b) : "") + " lead the hall";
        }
        private static readonly string[] StoryPages =
        {
            "The lamp at the end of the platform was lit for someone who never came. It kept burning anyway, and that was its kindness.",
            "Lost things here are not lost. They are only waiting to be asked where they would like to go.",
            "Every letter in the Waiting Realm arrives, eventually. Some take a lifetime; some take a single breath.",
            "The Keeper does not keep doors. It keeps the moment before you decide to walk through one.",
        };
        private string StoryPage() => StoryPages[Next(ref State.VisitorRandom, StoryPages.Length)];

        /// <summary>A refresh: remember the shelf so the Clock Rewinder can put it back.</summary>
        private void RememberShelf(int paid)
        {
            State.ShelfBeforeRefresh.Clear();
            foreach (ShopOffer o in State.Offers) State.ShelfBeforeRefresh.Add(JsonUtility.FromJson<ShopOffer>(JsonUtility.ToJson(o)));
            State.RefreshPaid = paid;
        }
        /// <summary>A companion was bought: the twins offer its double, and a promise to recruit nobody is broken.</summary>
        private void VisitorBoughtCompanion(ShopOffer offer)
        {
            State.VisitorEffects.RemoveAll(e => e.Id == "Promise" && e.Wave == State.Wave);
            if (State.TwinShop != State.Wave) return;
            State.TwinShop = -1;
            // a dream-shop friend is only ever upgraded in the dream shop (requirement 22/10/2026): its twin waits there, at its own currency
            bool special = State.SpecialOffers.Contains(offer) || catalog.Unit(offer.Id).SpecialRecruit;
            (special ? State.SpecialOffers : State.Offers).Add(new ShopOffer { Token = ++State.NextOffer, Kind = OfferKind.Companion, Id = offer.Id, Tier = offer.Tier, Price = Mathf.Max(1, offer.Price / 2), BasePrice = Mathf.Max(1, offer.Price / 2), Currency = special ? offer.Currency : ShopCurrency.Gold });
            Notice = "The twin is offered at half price.";
        }
        /// <summary>Leaving the shop: the thief takes his cut; a pawned relic past its time is forfeit.</summary>
        private void VisitorLeaveShop()
        {
            if (VisitorActive("Thief", State.Wave) && State.Gold > ScaledGold(60)) { int lost = State.Gold / 10; State.Gold -= lost; Notice = "The night thief took " + lost + " gold."; }
            State.VisitorEffects.RemoveAll(e => e.Id == "Thief");
        }
        /// <summary>A pawned relic waiting to be redeemed (shown as a REDEEM table in the shop).</summary>
        public VisitorEffect Pawned => State.Phase == SessionPhase.Shop ? State.VisitorEffects.Find(e => e.Id == "Pawn" && e.Wave >= State.Wave) : null;
        public bool RedeemPawn()
        {
            VisitorEffect p = Pawned; if (p == null || State.Gold < p.Value) return false;
            ItemDefinition item = catalog.Item(p.Target); if (item == null) return false;
            State.Gold -= Mathf.RoundToInt(p.Value); State.VisitorEffects.Remove(p);
            float previousMaximum = MaximumHealth(); AddItem(item); RecountBonds(); ReconcileHealth(previousMaximum);
            Notice = item.Name + " redeemed."; return true;
        }
        private void VisitorNewShop()
        {
            // the promise: kept through the last shop, the next one is cheaper
            if (VisitorActive("Promise", State.Wave - 1)) State.PromiseDiscountWave = State.Wave;
            // what a wave used is gone; a pawn waits for its deadline, a promise for the shop it covers, a post for its wave
            State.VisitorEffects.RemoveAll(e => e.Wave < State.Wave || e.Wave == State.Wave && e.Id != "Pawn" && e.Id != "Promise");
            RollVisitor();
        }

        /// <summary>Into the next fight: the visitor effects for this wave.</summary>
        private void PrepareVisitors()
        {
            int w = State.Wave; CheckpointCatalog c = CombatCatalog; if (c == null) return;
            c.Visitor = new VisitorWave();
            if (VisitorActive("Dare", w)) { c.EnemyBudget = Mathf.Min(600, Mathf.CeilToInt(c.EnemyBudget * 1.2f)); c.EliteCount = 2; }
            if (VisitorEffectFor("Play", w) is VisitorEffect play) { if (play.Value == 0) c.Visitor.FriendTempo = 1.1f; else if (play.Value == 1) c.Visitor.EnemySpeed = .9f; else c.Visitor.CoinPlus = 1; }
            if (VisitorEffectFor("Dream", w) is VisitorEffect dream)
            {
                // a sealed dream: one of seven small surprises (the strange one: a drowsy hall, slower enemies and heavier coins)
                switch ((int)dream.Value) { case 0: c.Visitor.Lamps = true; break; case 1: c.Visitor.Puddles = true; break; case 2: c.Visitor.Candy = true; break; case 3: c.Visitor.Matches = 3; break;
                    case 4: c.Visitor.CoinPlus = 1; c.Visitor.CoinSeconds = 20; break; case 5: c.Visitor.HeroSpeed = 1.15f; break; default: c.Visitor.EnemySpeed = .9f; c.Visitor.CoinPlus = 1; break; }
            }
            if (VisitorActive("Lamps", w)) c.Visitor.Lamps = true;
            if (VisitorActive("Toy", w)) c.Visitor.Toy = true;
            if (VisitorEffectFor("Mended", w) is VisitorEffect mend) c.Visitor.SlowSpecies = mend.Target;
            if (VisitorActive("Tempo", w)) { c.Visitor.EnemySpeed *= 1.15f; c.Visitor.FriendTempo *= 1.15f; c.Visitor.HeroSpeed *= 1.15f; c.Visitor.CoinPlus = Mathf.Max(c.Visitor.CoinPlus, 1); }
            if (VisitorActive("Story", w)) { c.Visitor.CoinPlus = Mathf.Max(c.Visitor.CoinPlus, 1); c.Visitor.CoinSeconds = c.Visitor.CoinSeconds > 0 ? c.Visitor.CoinSeconds : 20; }
            if (VisitorActive("Candy", w)) c.Visitor.Candy = true;
            if (VisitorActive("Matches", w)) c.Visitor.Matches = 3;
            if (VisitorActive("Collect", w) && c.EnemyPool != null && c.EnemyPool.Length > 0) c.Visitor.Species = c.EnemyPool[Next(ref State.VisitorRandom, c.EnemyPool.Length)].Id;
            else if (VisitorActive("Collect", w) && c.MeleeEnemy != null) c.Visitor.Species = c.MeleeEnemy.Id;
            if (VisitorActive("Wake", w)) c.Visitor.GaugeStart = true;
            if (VisitorActive("Puddles", w)) c.Visitor.Puddles = true;
            if (VisitorActive("Cloak", w)) c.Visitor.Cloak = true;
            if (VisitorActive("Key", w)) c.Visitor.Key = true;
            if (VisitorActive("Map", w)) c.TreasureDetection += 1;
            if (VisitorActive("Umbrella", w)) c.Visitor.Umbrella = true;
            if (VisitorActive("Scarf", w)) c.Visitor.HeroSpeed *= 1.15f;
            if (VisitorActive("Shadow", w)) c.Visitor.ShadowSeconds = 5;
            if (VisitorActive("Sand", w)) c.Visitor.SandSeconds = 20;
            if (VisitorActive("Longer", w)) c.WaveDuration += 10;
            if (VisitorEffectFor("Engraved", w) is VisitorEffect carve)
                foreach (UnitDefinition u in c.StartingCompanions) if (u != null && u.Id == carve.Target) u.Weapon.Damage *= 1.15f;
            if (VisitorEffectFor("Away", w) is VisitorEffect away)
            {
                var kept = new List<UnitDefinition>(); bool gone = false;
                foreach (UnitDefinition u in c.StartingCompanions) { if (!gone && u != null && u.Id == away.Target) { gone = true; continue; } kept.Add(u); }
                c.StartingCompanions = kept.ToArray();
            }
        }
        /// <summary>After the fight: the visitors' rewards and the lent relic going home.</summary>
        private void SettleVisitors(CombatSimulation sim)
        {
            int w = State.Wave; bool won = sim.Phase == RunPhase.Victory; var notes = new List<string>();
            if (won && VisitorActive("Dare", w) && GiveRandomItem(w >= 12 ? ItemRarity.Uncommon : ItemRarity.Common, false) is ItemDefinition dare) notes.Add("The Gatekeeper pays: " + dare.Name);
            if (won && VisitorActive("Letter", w) && sim.OctoberInjuries <= 5) { int pay = Reward("Letter", w, 15); State.Gold = BoundedCounter((long)State.Gold + pay); notes.Add("Letter delivered: " + pay + " gold"); }
            if (VisitorEffectFor("Bet", w) is VisitorEffect bet && won && sim.LowestChampionShare >= DiceHoldShare(bet.Target)) { int pay = bet.Target != null && bet.Target.StartsWith("paid") ? Mathf.RoundToInt(bet.Value) : Mathf.RoundToInt(bet.Value * 1.5f); State.Gold = BoundedCounter((long)State.Gold + pay); notes.Add("The Dice Elder pays " + pay + " gold"); }
            if (VisitorActive("Toy", w) && sim.VisitorToySeconds >= 20) { State.Eggs = BoundedCounter((long)State.Eggs + 1); notes.Add("The lost toy found its way home (dreamsand)"); }
            if (won && VisitorActive("Lullaby", w) && sim.Kills >= 80) { State.Eggs = BoundedCounter((long)State.Eggs + 1); notes.Add("The child sleeps (dreamsand)"); }
            if (VisitorActive("Collect", w) && sim.VisitorSpeciesKills >= 10) { int pay = Reward("Collect", w, 12); State.Gold = BoundedCounter((long)State.Gold + pay); notes.Add("The collector pays " + pay + " gold"); }
            if (VisitorActive("Away", w)) { int pay = Reward("Away", w, 10); State.Eggs = BoundedCounter((long)State.Eggs + 1); State.Gold = BoundedCounter((long)State.Gold + pay); notes.Add("Your friend is back with dreamsand and " + pay + " gold"); }
            if (VisitorActive("Key", w) && sim.VisitorKeyUsed && GiveRandomItem(ItemRarity.Common, false) is ItemDefinition key) notes.Add("The key opened " + key.Name);
            if (VisitorActive("Balloon", w)) { if (Next(ref State.VisitorRandom, 10) < 7 && GiveRandomItem(ItemRarity.Common, false) is ItemDefinition balloon) notes.Add("The balloon brought back " + balloon.Name); else notes.Add("The balloon came back empty"); }
            if (VisitorEffectFor("Post", w) is VisitorEffect post) { State.Gold = BoundedCounter((long)State.Gold + Mathf.RoundToInt(post.Value)); notes.Add("Post from the past: " + post.Value + " gold"); }
            if (VisitorEffectFor("Lent", w) is VisitorEffect lent && State.Items.Find(s => s.Id == lent.Target) is ItemStack borrowed) { RemoveOneItem(borrowed); notes.Add("The lender took back " + catalog.Item(lent.Target).Name); }
            if (notes.Count > 0) Notice = string.Join(". ", notes) + ".";
        }
    }
}
