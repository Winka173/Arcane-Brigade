using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed partial class RunSnapshot { public SecretEventProgress Secrets = new SecretEventProgress(); }

    public sealed partial class RunSession
    {
        public bool SecretRoomOpen => State.Secrets != null && State.Secrets.Stage != SecretEventStage.Idle;
        public bool SecretFighting => State.Secrets?.Stage == SecretEventStage.Fighting;
        private SecretEventProgress Secret => State.Secrets ?? (State.Secrets = new SecretEventProgress());
        private bool SecretSolo => catalog.Unit(State.ChampionId).HeroMechanic == HeroMechanic.SoloMaw;
        public bool SecretItemSuspended(string id) => State.Secrets != null && (State.Secrets.Stitches.Exists(t => t.ItemId == id) || State.Secrets.Contracts.Exists(c => c.ItemId == id && (c.Kind == SecretContractKind.BorrowedShadow || c.Kind == SecretContractKind.InsuredParcel)));
        public bool SecretItemImproved(string id) => State.Secrets != null && State.Secrets.ImprovedItems.Contains(id);
        public IReadOnlyList<string> SecretTargets => State.Items.Where(s => !SecretItemSuspended(s.Id) && !Secret.Stitches.Exists(t => t.ItemId == s.Id) && !Secret.ImprovedItems.Contains(s.Id)).Select(s => s.Id).ToArray();

        public SecretEventKind[] SecretPortals()
        {
            if (SecretEventAssets.Current == null || State.Phase != SessionPhase.Shop || SecretRoomOpen || State.OutsideStage == OutsideStageStatus.Reward) return Array.Empty<SecretEventKind>();
            if (Secret.QueuedEvent >= 0 && Secret.VisitWave != State.Wave) return new[] { (SecretEventKind)Secret.QueuedEvent };
            // requirement 25/10/2026: after every wave a 30% chance of a secret door, +30% for each shop without one (so one
            // comes within four shops), back to 30% once one appears; no cap on visits (each room is still met once)
            bool invited = SecretInvited(State.Wave);
            if (!invited || Secret.VisitWave == State.Wave) return Array.Empty<SecretEventKind>();
            var kinds = Enumerable.Range(0, 8).Where(k => (Secret.VisitedMask & (1 << k)) == 0).ToList();
            uint hash = unchecked((uint)State.Seed ^ (uint)State.Wave * 0x9e3779b9u);
            var result = new List<SecretEventKind>();
            for (int i = 0; i < 2 && kinds.Count > 0; i++)
            {
                hash = unchecked(hash * 1664525 + 1013904223); int pick = (int)(hash % (uint)kinds.Count);
                result.Add((SecretEventKind)kinds[pick]); kinds.RemoveAt(pick);
            }
            return result.ToArray();
        }

        /// <summary>The secret door opens on a fixed rhythm (requirement 27/10/2026: no luck of the draw): the shop after every third night,
        /// from night 3. The same every run, so a player can plan for it.</summary>
        public const int SecretEvery = 3;
        private bool SecretInvited(int wave) => wave >= SecretEvery && wave % SecretEvery == 0;
        private (int, int, int, int)? secretUndo;
        public bool EnterSecretEvent(SecretEventKind kind)
        {
            if (!Array.Exists(SecretPortals(), k => k == kind)) return false;
            bool queued = Secret.QueuedEvent == (int)kind;
            secretUndo = (Secret.VisitWave, Secret.VisitedMask, Secret.Visits, Secret.QueuedEvent); // walking out without a deal gives it all back
            Secret.Kind = kind; Secret.Stage = SecretEventStage.Choosing; Secret.Option = -1;
            Secret.VisitWave = State.Wave; Secret.VisitedMask |= 1 << (int)kind;
            Secret.RoomNights ??= new List<int>(); Secret.RoomNights.Add(State.Wave * 16 + (int)kind); // the next night's journal page remembers it
            if (queued) Secret.QueuedEvent = -1; else Secret.Visits++;
            Secret.TargetItem = SecretTargets.FirstOrDefault() ?? "";
            RosterEntry first = ActiveRoster.FirstOrDefault();
            Secret.TargetUnit = first?.UnitId ?? ""; Secret.TargetWeapon = first?.WeaponId ?? "";
            Secret.RewardId = ""; Secret.Result = Secret.ResultPaid = Secret.ResultGot = Secret.ResultIcon = "";
            return true;
        }

        public bool SetSecretTarget(string id, string weapon = "")
        {
            if (Secret.Stage != SecretEventStage.Choosing || State.Phase != SessionPhase.Shop) return false;
            if (weapon.Length > 0)
            {
                if (!State.Roster.Exists(r => r.UnitId == id && r.WeaponId == weapon)) return false;
                Secret.TargetUnit = id; Secret.TargetWeapon = weapon;
            }
            else
            {
                if (!SecretTargets.Contains(id)) return false;
                Secret.TargetItem = id;
            }
            return true;
        }

        /// <summary>A secret offer as a plate: a verb title, then DO / LOSE / GET in one short line each (play-test 20/10/2026).</summary>
        // a relic you get always says what it does (play-test 24/10/2026: the name alone told nothing)
        private SecretChoice Choice(string title, string act, string lose, string get, bool enabled = true, string reward = "", bool target = false)
            => Choice2(title, act, lose, catalog.Item(reward) != null ? (get == RewardName(reward) ? "New relic" : get.Replace(RewardName(reward), "a relic")) + "\n" + SecretRewardStats(reward) : get, enabled, reward, target);
        /// <summary>A secret relic's stats in one short line: "Shop discount +8% · Luck +8%" (luck is a percent like every chance).</summary>
        public string SecretRewardStats(string id)
        {
            ItemDefinition item = catalog.Item(id);
            if (item == null) return catalog.Unit(id) != null ? "Joins your brigade" : "";
            return string.Join(" · ", item.EffectiveModifiers.Where(m => m.Value != 0).Take(3).Select(m => ShortStat(m.Effect) + " " + RunModifierRules.ValueText(m.Effect, m.Value)));
        }
        /// <summary>A stat's name short enough for a plate row.</summary>
        private static string ShortStat(ItemEffect e) => e switch
        {
            ItemEffect.Crit => "Crit", ItemEffect.AttackSpeed => "Speed", ItemEffect.AttackRange => "Range", ItemEffect.Health => "Max HP",
            ItemEffect.Pickup => "Pickup", ItemEffect.TreasureDetection => "Chests", ItemEffect.HeartHealing => "Heart heal",
            _ => RunModifierRules.ModifierName(e)
        };
        private SecretChoice Choice2(string title, string act, string lose, string get, bool enabled, string reward, bool target)
            => new SecretChoice { Title = title, Do = act, Lose = lose, Get = get, UsesTarget = target, Copy = act + "\n" + lose + "\n" + get, Enabled = enabled, RewardId = reward, Icon = catalog.Item(reward)?.Icon ?? catalog.Unit(reward)?.Body };
        private string RewardName(string id) => catalog.Item(id)?.Name ?? catalog.Unit(id)?.DisplayName ?? "";
        /// <summary>The verb on the room's one confirm button.</summary>
        public string SecretVerb => Secret.Kind switch
        {
            SecretEventKind.ImpossibleTailor => "STITCH", SecretEventKind.ContrabandTrain => "DEAL", SecretEventKind.ThirteenthHour => "SIGN",
            SecretEventKind.SleepingGarden => "WAKE", SecretEventKind.EmptyTheatre => "PERFORM", SecretEventKind.LostProperty => "GO",
            SecretEventKind.Understudy => "AGREE", _ => "SEND"
        };
        /// <summary>Whether this room's offers work on one of your relics (shown in the middle, tap to swap).</summary>
        public bool SecretUsesTarget => Secret.Kind == SecretEventKind.ImpossibleTailor || Secret.Kind == SecretEventKind.DeadLetters || Secret.Kind == SecretEventKind.ThirteenthHour || Secret.Kind == SecretEventKind.Understudy || Secret.Kind == SecretEventKind.ContrabandTrain;
        public SecretChoice[] SecretChoices()
        {
            string item = Secret.TargetItem; bool owned = SecretTargets.Contains(item);
            bool contract = Secret.Contracts.Count == 0;
            string name = catalog.Item(item)?.Name ?? "your relic";
            int price = SecretTrainPrice;
            const string Fight = "Lose the fight and the run ends";
            switch (Secret.Kind)
            {
                case SecretEventKind.LostProperty:
                    return new[] {
                        Choice("FREE THE HEART", "Fight the cabinet warden", Fight, RewardName(SecretSolo ? "SecretWoodHeart" : "SecretPatchBear"), true, SecretSolo ? "SecretWoodHeart" : "SecretPatchBear"),
                        Choice("FREE THE COURIER", "Fight the cabinet warden", Fight, RewardName(SecretSolo ? "SecretLetter" : "SecretPaperBird"), true, SecretSolo ? "SecretLetter" : "SecretPaperBird"),
                        Choice("TAKE THE HEART", "No fight", "The prisoners stay asleep", RewardName("SecretWoodHeart"), CanSecretItem("SecretWoodHeart"), "SecretWoodHeart") };
                case SecretEventKind.ImpossibleTailor:
                    return new[] {
                        Choice("RECALL STITCH", "Restitch " + name, name + "'s stats · shots pierce 1 less", "Shots come back · +15% ranged damage", owned && HasSecretRanged, "", true),
                        Choice("AFTERSHOCK STITCH", "Restitch " + name, name + "'s stats · melee 20% slower", "Melee hits echo for 30% damage", owned && HasSecretMelee, "", true),
                        Choice("SHELTER STITCH", "Restitch " + name, name + "'s stats · −8% damage", "+18 max HP · 15% less damage taken", owned, "", true) };
                case SecretEventKind.Understudy:
                    return new[] {
                        Choice("FIGHT YOUR REFLECTION", "Fight a copy of your brigade", Fight, RewardName("SecretMirror"), CanSecretItem("SecretMirror"), "SecretMirror"),
                        Choice("LEND A RELIC", "Lend " + name + " for 2 waves", name + " is away for 2 waves", SecretSolo ? "+20% hero damage for 2 waves" : "A shadow friend for 2 waves", owned && contract, "", true),
                        Choice("A KIND REFLECTION", "Look, then leave", "Nothing", "Heal 20% of max HP") };
                case SecretEventKind.ContrabandTrain:
                    return new[] {
                        Choice("THE BRASS CRATE", "Buy the crate", price + " gold", RewardName("SecretTicket"), State.Gold >= price && CanSecretItem("SecretTicket"), "SecretTicket"),
                        Choice("THE THREAD CRATE", "Trade a relic", "1 × " + name, RewardName("SecretThread"), owned && CanSecretItem("SecretThread"), "SecretThread", true),
                        Choice("THE OWNER'S CRATE", "Fight the collector", Fight, RewardName("SecretCrown"), CanSecretItem("SecretCrown"), "SecretCrown") };
                case SecretEventKind.ThirteenthHour:
                    return new[] {
                        Choice("BORROW AN HOUR", "Borrow an hour for 3 waves", "Gone after 3 waves", RewardName("SecretHour"), contract && StackCount("SecretHour") == 0, "SecretHour"),
                        Choice("GOLD NOW", "Take a loan", "25% of wave gold for 2 waves", SecretLoan + " gold now", contract),
                        Choice("DEPOSIT A RELIC", "Leave " + name + " for 2 waves", name + " is away for 2 waves", name + " comes back with +25% stats", contract && owned && CanImproveSecretItem(item), "", true) };
                case SecretEventKind.SleepingGarden:
                    return new[] {
                        Choice("WAKE THE HEALER", "Wake her gently", "Nothing", "Full HP · " + RewardName("SecretDream"), CanSecretItem("SecretDream"), "SecretDream"),
                        Choice("WAKE THE KEEPER", "End your secret deals", "Your deal ends now", "Lent relics come back", Secret.Contracts.Count > 0),
                        Choice(SecretSolo ? "WAKE A DREAM" : "WAKE THE FRIEND", SecretSolo ? "Dream for 2 waves" : "A Dream Lamb joins you", "Gone after 2 waves", SecretSolo ? "+20% hero damage" : "A friend for 2 waves", contract) };
                case SecretEventKind.EmptyTheatre:
                    return new[] {
                        Choice("THE PAPER CHORUS", "Survive a 28 s swarm", Fight, RewardName("SecretCrown"), CanSecretItem("SecretCrown"), "SecretCrown"),
                        Choice("THE LEADING ACTOR", "Beat one giant actor", Fight, RewardName("SecretHour"), CanSecretItem("SecretHour"), "SecretHour"),
                        Choice("THE HOSTILE DUET", "Beat two actors at once", Fight, RewardName("SecretThread"), CanSecretItem("SecretThread"), "SecretThread") };
                default:
                    string friend = catalog.Unit(Secret.TargetUnit)?.DisplayName ?? "a friend";
                    return new[] {
                        Choice("TO YOURSELF", "Send " + name + " away for 2 waves", name + " is away for 2 waves", name + " comes back with +25% stats", owned && contract && CanImproveSecretItem(item), "", true),
                        Choice("TO A FRIEND", "Send " + name + " to " + friend, "1 × " + name, friend + ": +20% weapon damage", owned && !SecretSolo && State.Roster.Exists(r => r.UnitId == Secret.TargetUnit && r.WeaponId == Secret.TargetWeapon) && !Secret.AllyGifts.Exists(g => g.UnitId == Secret.TargetUnit && g.WeaponId == Secret.TargetWeapon), "", true),
                        Choice("OUTSIDE THE STAGE", "Send an invitation", "Nothing", RewardName("SecretLetter") + " · a secret show later" + (Secret.RescuedToy && !Secret.ThankYouClaimed ? " · " + ScaledGold(15) + " gold" : ""), !Secret.LetterSent && CanSecretItem("SecretLetter"), "SecretLetter") };
            }
        }

        // on the shop's gold curve (play-test 20/10/2026): the wave-1 amounts 28 and 33, scaled like every other gold price
        public int SecretTrainPrice => ScaledGold(28);
        public int SecretLoan => ScaledGold(33);
        private bool HasSecretRanged => catalog.Unit(State.ChampionId).Weapon.Kind == WeaponKind.Projectile || ActiveRoster.Exists(r => catalog.Weapon(r.WeaponId).Kind == WeaponKind.Projectile);
        private bool HasSecretMelee => catalog.Unit(State.ChampionId).Weapon.Kind == WeaponKind.Melee || ActiveRoster.Exists(r => catalog.Weapon(r.WeaponId).Kind == WeaponKind.Melee);
        private bool CanImproveSecretItem(string id) => catalog.Item(id) != null && catalog.Item(id).EffectiveModifiers.Any(m => m.Value > 0);
        private bool CanSecretItem(string id) => catalog.Item(id) != null && StackCount(id) < catalog.Item(id).StackLimit;
        private bool GiveSecretItem(string id)
        {
            if (!CanSecretItem(id)) return false;
            ItemStack stack = State.Items.Find(s => s.Id == id);
            if (stack == null) State.Items.Add(new ItemStack { Id = id, Count = 1 }); else stack.Count++;
            return true;
        }
        private void ConsumeSecretItem(string id)
        {
            ItemStack stack = State.Items.Find(s => s.Id == id); if (--stack.Count == 0) State.Items.Remove(stack);
        }
        private bool GiveSecretAlly(string id)
        {
            UnitDefinition unit = catalog.Unit(id); if (SecretSolo || unit == null) return false;
            RosterEntry existing = State.Roster.Find(r => r.UnitId == id && r.WeaponId == unit.Weapon.Id);
            if (existing != null) return false;
            var entry = new RosterEntry { UnitId = id, WeaponId = unit.Weapon.Id }; State.Roster.Add(entry); RememberVariant(entry); return true;
        }

        public bool ChooseSecretEvent(int option)
        {
            if (State.Phase != SessionPhase.Shop || Secret.Stage != SecretEventStage.Choosing || option < 0 || option > 2) return false;
            SecretChoice choice = SecretChoices()[option]; if (!choice.Enabled) return false;
            Secret.Option = option; Secret.RewardId = choice.RewardId ?? "";
            RememberNightmareChoice(Secret.Kind,option,false);
            bool fight = Secret.Kind == SecretEventKind.LostProperty && option < 2 || Secret.Kind == SecretEventKind.Understudy && option == 0 || Secret.Kind == SecretEventKind.ContrabandTrain && option == 2 || Secret.Kind == SecretEventKind.EmptyTheatre;
            if (fight) { Secret.Stage = SecretEventStage.Fighting; State.Phase = SessionPhase.Combat; return true; }
            switch (Secret.Kind)
            {
                case SecretEventKind.LostProperty: GiveSecretItem("SecretWoodHeart"); break;
                case SecretEventKind.ImpossibleTailor: Secret.Stitches.Add(new SecretStitch { ItemId = Secret.TargetItem, Pattern = option }); break;
                case SecretEventKind.Understudy:
                    if (option == 2) State.Health = Mathf.Min(MaximumHealth(), State.Health + MaximumHealth() * .2f);
                    else Secret.Contracts.Add(new SecretContract { Kind = SecretContractKind.BorrowedShadow, Remaining = 2, ItemId = Secret.TargetItem });
                    break;
                case SecretEventKind.ContrabandTrain:
                    if (option == 0) State.Gold -= SecretTrainPrice; else ConsumeSecretItem(Secret.TargetItem);
                    GiveSecretItem(choice.RewardId); break;
                case SecretEventKind.ThirteenthHour:
                    if (option == 0) { GiveSecretItem("SecretHour"); Secret.Contracts.Add(new SecretContract { Kind = SecretContractKind.BorrowedRelic, Remaining = 3, ItemId = "SecretHour" }); }
                    else if (option == 1) { State.Gold = BoundedCounter((long)State.Gold + SecretLoan); Secret.Contracts.Add(new SecretContract { Kind = SecretContractKind.IncomeDebt, Remaining = 2 }); }
                    else Secret.Contracts.Add(new SecretContract { Kind = SecretContractKind.InsuredParcel, Remaining = 2, ItemId = Secret.TargetItem });
                    break;
                case SecretEventKind.SleepingGarden:
                    if (option == 0) { GiveSecretItem("SecretDream"); State.Health = MaximumHealth(); }
                    else if (option == 1) { foreach (SecretContract c in Secret.Contracts) FinishSecretContract(c); Secret.Contracts.Clear(); }
                    else Secret.Contracts.Add(new SecretContract { Kind = SecretContractKind.DreamFriend, Remaining = 2 });
                    break;
                case SecretEventKind.DeadLetters:
                    if (option == 0) Secret.Contracts.Add(new SecretContract { Kind = SecretContractKind.InsuredParcel, Remaining = 2, ItemId = Secret.TargetItem });
                    else if (option == 1) { ConsumeSecretItem(Secret.TargetItem); Secret.AllyGifts.Add(new SecretAllyGift { UnitId = Secret.TargetUnit, WeaponId = Secret.TargetWeapon }); }
                    else
                    {
                        GiveSecretItem("SecretLetter"); Secret.LetterSent = true;
                        if (Secret.RescuedToy && !Secret.ThankYouClaimed) { State.Gold = BoundedCounter((long)State.Gold + ScaledGold(15)); Secret.ThankYouClaimed = true; }
                        if ((Secret.VisitedMask & (1 << (int)SecretEventKind.EmptyTheatre)) == 0) Secret.QueuedEvent = (int)SecretEventKind.EmptyTheatre;
                    }
                    break;
            }
            Secret.Result = choice.Title + "\n" + choice.Copy; Secret.Stage = SecretEventStage.Reward; Secret.RewardId = "";
            Secret.ResultPaid = choice.Lose; Secret.ResultGot = choice.Get; Secret.ResultIcon = choice.RewardId ?? "";
            RecountBonds();
            State.Health = Mathf.Min(State.Health, MaximumHealth());
            RecordSecret(choice.Title);
            RememberNightmareChoice(Secret.Kind,option,true);
            return true;
        }

        public bool LeaveSecretEvent()
        {
            if (State.Phase != SessionPhase.Shop || Secret.Stage != SecretEventStage.Choosing && Secret.Stage != SecretEventStage.Reward || !string.IsNullOrEmpty(Secret.RewardId)) return false;
            bool undealt = Secret.Stage == SecretEventStage.Choosing;
            RecordSecret(!undealt ? "Returned to the unchanged shop." : "Left without a deal.");
            Secret.Stage = SecretEventStage.Idle;
            // requirement 25/10/2026 (declined things stay reopenable): leaving without a deal keeps the invitation standing
            if (undealt && secretUndo.HasValue)
            {
                (int wave, int mask, int visits, int queued) = secretUndo.Value;
                Secret.VisitWave = wave; Secret.VisitedMask = mask; Secret.Visits = visits; Secret.QueuedEvent = queued;
                if (Secret.RoomNights != null && Secret.RoomNights.Count > 0) Secret.RoomNights.RemoveAt(Secret.RoomNights.Count - 1);
            }
            secretUndo = null;
            return true;
        }
        private bool SecretCompanionRewardUnavailable => catalog.Unit(Secret.RewardId) != null && (SecretSolo || State.Roster.Exists(r => r.UnitId == Secret.RewardId && r.WeaponId == catalog.Unit(Secret.RewardId).Weapon.Id));
        private string SecretCompanionFallback => Secret.RewardId == "SecretPaperBird" ? "SecretLetter" : "SecretWoodHeart";
        public string SecretRewardLabel => SecretCompanionRewardUnavailable ? CanSecretItem(SecretCompanionFallback) ? catalog.Item(SecretCompanionFallback).Name + " / RELIC INSTEAD" : "25 GOLD / ALTERNATE REWARD" : catalog.Item(Secret.RewardId)?.Name ?? catalog.Unit(Secret.RewardId)?.DisplayName;
        public bool ClaimSecretBattleReward()
        {
            if (State.Phase != SessionPhase.Shop || Secret.Stage != SecretEventStage.Reward || string.IsNullOrEmpty(Secret.RewardId)) return false;
            string id = Secret.RewardId;
            bool success;
            if (SecretCompanionRewardUnavailable)
            {
                if (CanSecretItem(SecretCompanionFallback)) { id = SecretCompanionFallback; success = GiveSecretItem(id); }
                else { State.Gold = BoundedCounter((long)State.Gold + ScaledGold(25)); id = ""; success = true; }
            }
            else success = catalog.Item(id) != null ? GiveSecretItem(id) : GiveSecretAlly(id);
            if (!success) return false;
            if (Secret.Kind == SecretEventKind.LostProperty) { Secret.RescuedToy = true; Secret.RescuedId = id ?? ""; }
            RememberNightmareChoice(Secret.Kind,Secret.Option,true);
            if (State.Difficulty >= 2) GiveRainbowThread();
            RecountBonds();
            // the won prize stays on the result card (its picture and what it gives), like every other deal's result
            string prize = string.IsNullOrEmpty(id) ? ScaledGold(25) + " gold" : catalog.Item(id)?.Name ?? catalog.Unit(id)?.DisplayName;
            string stats = string.IsNullOrEmpty(id) ? "" : SecretRewardStats(id);
            Secret.Result = "YOU WON THE FIGHT"; Secret.ResultGot = prize + (string.IsNullOrEmpty(stats) ? "" : "\n" + stats); Secret.ResultIcon = id ?? ""; Secret.ResultPaid = "";
            Secret.RewardId = ""; RecordSecret("Received " + prize);
            return true;
        }
        private void RecordSecret(string text)
        {
            Secret.Journal.Add("Hall " + State.Wave + " / " + SecretEventAssets.Titles[(int)Secret.Kind] + ": " + text);
            while (Secret.Journal.Count > 24) Secret.Journal.RemoveAt(0);
        }

        public float SecretBonus(ItemEffect effect, ModifierScope scope)
        {
            if (State.Secrets == null) return 0;
            float bonus = 0;
            foreach (SecretStitch stitch in Secret.Stitches)
            {
                if (StackCount(stitch.ItemId) == 0 || !RunModifierRules.Applies(ModifierScope.Team, scope)) continue;
                if (stitch.Pattern == 2 && effect == ItemEffect.Health && scope == ModifierScope.Champion) bonus += 18;
                if (stitch.Pattern == 2 && effect == ItemEffect.Damage) bonus -= .08f;
            }
            if (SecretSolo && Secret.Contracts.Exists(c => c.Kind == SecretContractKind.BorrowedShadow || c.Kind == SecretContractKind.DreamFriend) && effect == ItemEffect.ChampionDamage && scope == ModifierScope.Champion) bonus += .2f;
            return bonus;
        }

        private void FinishSecretContract(SecretContract c)
        {
            if (c.Kind == SecretContractKind.BorrowedRelic)
            {
                ItemStack stack = State.Items.Find(s => s.Id == c.ItemId); if (stack != null) ConsumeSecretItem(c.ItemId);
            }
            if (c.Kind == SecretContractKind.InsuredParcel && !Secret.ImprovedItems.Contains(c.ItemId)) Secret.ImprovedItems.Add(c.ItemId);
        }
        private void SettleSecretContracts()
        {
            if (State.Secrets == null || Secret.LastContractWave >= State.Wave) return;
            Secret.LastContractWave = State.Wave;
            foreach (SecretContract c in Secret.Contracts.ToArray())
            {
                if (c.Kind == SecretContractKind.IncomeDebt)
                {
                    int charge = Mathf.CeilToInt((State.LastIncome + State.LastInterest) * .25f); State.Gold = Mathf.Max(0, State.Gold - charge);
                    Secret.Journal.Add("Hall " + State.Wave + ": bank repayment " + charge + " gold.");
                }
                if (--c.Remaining > 0) continue;
                FinishSecretContract(c); Secret.Contracts.Remove(c); RecordSecret("Contract completed: " + c.Kind);
            }
            State.Health = Mathf.Min(State.Health, MaximumHealth());
            while (Secret.Journal.Count > 24) Secret.Journal.RemoveAt(0);
        }

        private void ValidateSecretEvents(RunSnapshot snapshot)
        {
            if (snapshot?.Secrets == null) return; // old saves migrate lazily
            SecretEventProgress s = snapshot.Secrets;
            bool invalid = !Enum.IsDefined(typeof(SecretEventStage), s.Stage) || !Enum.IsDefined(typeof(SecretEventKind), s.Kind) || s.Visits < 0 || s.Visits > 2 || s.VisitedMask < 0 || s.VisitedMask > 255 || s.VisitWave < 0 || s.VisitWave > snapshot.Wave || s.LastContractWave < 0 || s.LastContractWave > snapshot.Wave || s.QueuedEvent < -1 || s.QueuedEvent > 7 || s.Option < -1 || s.Option > 2;
            invalid |= s.Contracts == null || s.Stitches == null || s.AllyGifts == null || s.ImprovedItems == null || s.Journal == null;
            if (invalid) throw new InvalidOperationException("Invalid secret-event save");
            if (s.Stage == SecretEventStage.Fighting && snapshot.Phase != SessionPhase.Combat && snapshot.Phase != SessionPhase.Defeat || (s.Stage == SecretEventStage.Choosing || s.Stage == SecretEventStage.Reward) && snapshot.Phase != SessionPhase.Shop) throw new InvalidOperationException("Invalid secret-event phase");
            if (s.Stage != SecretEventStage.Idle && snapshot.OutsideStage == OutsideStageStatus.Fighting || s.Contracts.Count > 1 || s.Stitches.Count > 8 || s.AllyGifts.Count > 8 || s.ImprovedItems.Count > 8 || s.Journal.Count > 24) throw new InvalidOperationException("Conflicting secret progress");
            foreach (SecretContract c in s.Contracts)
                if (c == null || !Enum.IsDefined(typeof(SecretContractKind), c.Kind) || c.Remaining < 1 || c.Remaining > 3 || (c.Kind == SecretContractKind.BorrowedRelic || c.Kind == SecretContractKind.BorrowedShadow || c.Kind == SecretContractKind.InsuredParcel) && (catalog.Item(c.ItemId) == null || !snapshot.Items.Exists(i => i.Id == c.ItemId))) throw new InvalidOperationException("Invalid secret contract");
            var seen = new HashSet<string>();
            foreach (SecretStitch t in s.Stitches) if (t == null || catalog.Item(t.ItemId) == null || t.Pattern < 0 || t.Pattern > 2 || !seen.Add(t.ItemId)) throw new InvalidOperationException("Invalid relic stitch");
            seen.Clear(); foreach (string id in s.ImprovedItems) if (catalog.Item(id) == null || !seen.Add(id)) throw new InvalidOperationException("Invalid improved parcel");
            seen.Clear(); foreach (SecretAllyGift g in s.AllyGifts) if (g == null || catalog.Unit(g.UnitId) == null || catalog.Weapon(g.WeaponId) == null || !seen.Add(g.UnitId + "/" + g.WeaponId)) throw new InvalidOperationException("Invalid ally letter");
            if (!string.IsNullOrEmpty(s.RewardId) && catalog.Item(s.RewardId) == null && catalog.Unit(s.RewardId) == null) throw new InvalidOperationException("Invalid secret reward");
        }
    }
}
