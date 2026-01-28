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
            if (Secret.Visits >= 2 || State.Wave < 3 || State.Wave % 4 != 3 || Secret.VisitWave == State.Wave) return Array.Empty<SecretEventKind>();
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

        public bool EnterSecretEvent(SecretEventKind kind)
        {
            if (!Array.Exists(SecretPortals(), k => k == kind)) return false;
            bool queued = Secret.QueuedEvent == (int)kind;
            Secret.Kind = kind; Secret.Stage = SecretEventStage.Choosing; Secret.Option = -1;
            Secret.VisitWave = State.Wave; Secret.VisitedMask |= 1 << (int)kind;
            if (queued) Secret.QueuedEvent = -1; else Secret.Visits++;
            Secret.TargetItem = SecretTargets.FirstOrDefault() ?? "";
            RosterEntry first = State.Roster.FirstOrDefault();
            Secret.TargetUnit = first?.UnitId ?? ""; Secret.TargetWeapon = first?.WeaponId ?? "";
            Secret.RewardId = ""; Secret.Result = "";
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

        private SecretChoice Choice(string title, string copy, bool enabled = true, string reward = "") => new SecretChoice { Title = title, Copy = copy + (catalog.Item(reward) == null ? "" : "\n\n" + catalog.Item(reward).Description), Enabled = enabled, RewardId = reward, Icon = catalog.Item(reward)?.Icon ?? catalog.Unit(reward)?.Body };
        public SecretChoice[] SecretChoices()
        {
            string item = Secret.TargetItem; bool owned = SecretTargets.Contains(item);
            bool contract = Secret.Contracts.Count == 0;
            string name = catalog.Item(item)?.Name ?? "Select an owned relic";
            int price = SecretTrainPrice;
            switch (Secret.Kind)
            {
                case SecretEventKind.LostProperty:
                    return new[] {
                        Choice("THE HEART IN THE DRAWER", "Fight the cabinet warden. Rescue the Forgotten Patchwork; solo heroes receive its Wooden Heart. Defeat ends the run.", true, SecretSolo ? "SecretWoodHeart" : "SecretPatchBear"),
                        Choice("THE LETTER THAT KNOCKS", "Fight the cabinet warden. Rescue Folded Courier; solo heroes receive its Dead-Letter Key. A later letter remembers your choice. Defeat ends the run.", true, SecretSolo ? "SecretLetter" : "SecretPaperBird"),
                        Choice("TAKE THE ABANDONED HEART", "Leave the prisoners asleep and take a Wooden Heart relic. No battle. Team health support; no second claim.", CanSecretItem("SecretWoodHeart"), "SecretWoodHeart") };
                case SecretEventKind.ImpossibleTailor:
                    return new[] {
                        Choice("RECALL STITCH", name + ": replace this relic's stat modifiers with returning projectiles and +15% ranged damage; team projectiles lose pierce. One stitch per relic.", owned && HasSecretRanged),
                        Choice("AFTERSHOCK STITCH", name + ": replace its stat modifiers with a delayed melee echo at 30% damage; melee attacks take 20% longer. One stitch per relic.", owned && HasSecretMelee),
                        Choice("SHELTER STITCH", name + ": replace its stat modifiers with +18 maximum HP and 15% less incoming damage; team damage -8%. One stitch per relic.", owned) };
                case SecretEventKind.Understudy:
                    return new[] {
                        Choice("FIGHT YOUR REFLECTION", "Your current build becomes a hostile understudy. It repeats your champion's basic attacks after 0.65s at their locked direction. Win: Glass Understudy relic. Defeat ends the run.", CanSecretItem("SecretMirror"), "SecretMirror"),
                        Choice("LEND A RELIC", "Lend " + name + " for two normal victories. Its effects sleep while a temporary shadow ally fights with you. It returns automatically. Solo heroes borrow its champion echo instead.", owned && contract),
                        Choice("LEAVE A KIND REFLECTION", "Recover 20% maximum HP, then return. No battle or payment.") };
                case SecretEventKind.ContrabandTrain:
                    return new[] {
                        Choice("THE BRASS-LOCKED CRATE", "Pay " + price + " gold for Smuggler's Ticket. See the relic below before buying. One deal only.", State.Gold >= price && CanSecretItem("SecretTicket"), "SecretTicket"),
                        Choice("THE THREAD-BOUND CRATE", "Trade one copy of " + name + " for Living Contraband Thread. The offered copy is consumed. One deal only.", owned && CanSecretItem("SecretThread"), "SecretThread"),
                        Choice("THE OWNER'S CRATE", "Challenge the train's collector. Win: Theatre Crown. No gold fee or ordinary-wave payout. Defeat ends the run.", CanSecretItem("SecretCrown"), "SecretCrown") };
                case SecretEventKind.ThirteenthHour:
                    return new[] {
                        Choice("BORROW AN EXTRA HOUR", "Borrow an Hourglass Advance relic for three normal victories. It vanishes when the third wave is settled. Only one active contract.", contract && StackCount("SecretHour") == 0, "SecretHour"),
                        Choice("ADVANCE ON TOMORROW", "Receive " + SecretLoan + " gold now. The bank takes 25% of normal-wave base income and interest for the next two victories. Kill loot is untouched.", contract),
                        Choice("DEPOSIT A RELIC", "Send " + name + " to the vault for two normal victories. Its stat effects sleep until it returns with +25% positive stat modifiers for this run.", contract && owned && CanImproveSecretItem(item)) };
                case SecretEventKind.SleepingGarden:
                    return new[] {
                        Choice("WAKE THE PORCELAIN HEALER", "Restore all HP and gain a Sleeping-Lamb Brooch. No hidden payment or curse.", CanSecretItem("SecretDream"), "SecretDream"),
                        Choice("WAKE THE CONTRACT KEEPER", "Settle your current secret contract immediately: return lent/deposited relics, cancel income debt and release temporary gifts. No payment.", Secret.Contracts.Count > 0),
                        Choice(SecretSolo ? "WAKE A DREAM IN YOUR HEART" : "WAKE THE SLEEPING FRIEND", SecretSolo ? "Borrow a champion-damage dream for two normal victories. No companion is added." : "A porcelain Dream Lamb joins for two normal victories, then goes peacefully home.", contract) };
                case SecretEventKind.EmptyTheatre:
                    return new[] {
                        Choice("THE PAPER CHORUS", "Survive a 28-second swarm performance. Reward: Theatre Crown. No ordinary income or kill loot is paid. Defeat ends the run.", CanSecretItem("SecretCrown"), "SecretCrown"),
                        Choice("THE LEADING ACTOR", "Defeat one enlarged cabinet actor with deliberate, heavy attacks. Reward: Hourglass Advance to keep. Defeat ends the run.", CanSecretItem("SecretHour"), "SecretHour"),
                        Choice("THE HOSTILE DUET", "Defeat a cabinet actor and its ranged supporting performer. Their volleys alternate. Reward: Living Contraband Thread. Defeat ends the run.", CanSecretItem("SecretThread"), "SecretThread") };
                default:
                    return new[] {
                        Choice("ADDRESS IT TO YOURSELF", "Post one copy of " + name + ". After two normal victories, it returns with +25% positive stat modifiers. Its effects sleep while away.", owned && contract && CanImproveSecretItem(item)),
                        Choice("ADDRESS IT TO A FRIEND", "Consume one copy of " + name + " to give " + (catalog.Unit(Secret.TargetUnit)?.DisplayName ?? "a chosen companion") + " / " + Secret.TargetWeapon + " +20% weapon damage for this run. This gift stays with that unit/weapon pairing; it does not follow a different weapon after merging.", owned && !SecretSolo && State.Roster.Exists(r => r.UnitId == Secret.TargetUnit && r.WeaponId == Secret.TargetWeapon) && !Secret.AllyGifts.Exists(g => g.UnitId == Secret.TargetUnit && g.WeaponId == Secret.TargetWeapon)),
                        Choice("ADDRESS IT OUTSIDE THE STAGE", (Secret.RescuedToy && !Secret.ThankYouClaimed ? "The rescued toy enclosed a thank-you: 15 gold. " : "") + "Send an invitation and receive a Dead-Letter Key. A secret theatre encounter waits in a later shop, even after the two-visit limit. The Keeper remembers your letter.", !Secret.LetterSent && CanSecretItem("SecretLetter"), "SecretLetter") };
            }
        }

        public int SecretTrainPrice => 25 + Mathf.Min(State.Wave, 30) * 3;
        public int SecretLoan => 30 + Mathf.Min(State.Wave, 30) * 3;
        private bool HasSecretRanged => catalog.Unit(State.ChampionId).Weapon.Kind == WeaponKind.Projectile || State.Roster.Exists(r => catalog.Weapon(r.WeaponId).Kind == WeaponKind.Projectile);
        private bool HasSecretMelee => catalog.Unit(State.ChampionId).Weapon.Kind == WeaponKind.Melee || State.Roster.Exists(r => catalog.Weapon(r.WeaponId).Kind == WeaponKind.Melee);
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
            UnitDefinition unit = catalog.Unit(id); if (SecretSolo || unit == null || State.Roster.Count >= 20) return false;
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
                        if (Secret.RescuedToy && !Secret.ThankYouClaimed) { State.Gold = BoundedCounter((long)State.Gold + 15); Secret.ThankYouClaimed = true; }
                        if ((Secret.VisitedMask & (1 << (int)SecretEventKind.EmptyTheatre)) == 0) Secret.QueuedEvent = (int)SecretEventKind.EmptyTheatre;
                    }
                    break;
            }
            Secret.Result = choice.Title + "\n" + choice.Copy; Secret.Stage = SecretEventStage.Reward; Secret.RewardId = "";
            RecountBonds();
            State.Health = Mathf.Min(State.Health, MaximumHealth());
            RecordSecret(choice.Title);
            RememberNightmareChoice(Secret.Kind,option,true);
            return true;
        }

        public bool LeaveSecretEvent()
        {
            if (State.Phase != SessionPhase.Shop || Secret.Stage != SecretEventStage.Choosing && Secret.Stage != SecretEventStage.Reward || !string.IsNullOrEmpty(Secret.RewardId)) return false;
            RecordSecret(Secret.Stage == SecretEventStage.Reward ? "Returned to the unchanged shop." : "Left without a deal.");
            Secret.Stage = SecretEventStage.Idle; return true;
        }
        public bool ClaimSecretBattleReward()
        {
            if (State.Phase != SessionPhase.Shop || Secret.Stage != SecretEventStage.Reward || string.IsNullOrEmpty(Secret.RewardId)) return false;
            string id = Secret.RewardId;
            bool success = catalog.Item(id) != null ? GiveSecretItem(id) : GiveSecretAlly(id);
            if (!success) return false;
            if (Secret.Kind == SecretEventKind.LostProperty) Secret.RescuedToy = true;
            RememberNightmareChoice(Secret.Kind,Secret.Option,true);
            RecountBonds();
            Secret.RewardId = ""; RecordSecret("Received " + (catalog.Item(id)?.Name ?? catalog.Unit(id)?.DisplayName));
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
