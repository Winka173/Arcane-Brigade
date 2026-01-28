using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum SecretEventKind { LostProperty, ImpossibleTailor, Understudy, ContrabandTrain, ThirteenthHour, SleepingGarden, EmptyTheatre, DeadLetters }
    public enum SecretEventStage { Idle, Choosing, Fighting, Reward }
    public enum SecretContractKind { BorrowedRelic, BorrowedShadow, IncomeDebt, InsuredParcel, DreamFriend, SkippedIncome }

    [Serializable] public sealed class SecretContract
    {
        public SecretContractKind Kind;
        public int Remaining;
        public string ItemId = "";
        public string UnitId = "";
        public string WeaponId = "";
    }
    [Serializable] public sealed class SecretStitch { public string ItemId; public int Pattern; }
    [Serializable] public sealed class SecretAllyGift { public string UnitId; public string WeaponId; }
    [Serializable] public sealed class SecretEventProgress
    {
        public SecretEventStage Stage;
        public SecretEventKind Kind;
        public int Visits;
        public int VisitedMask;
        public int VisitWave;
        public int Option = -1;
        public int LastContractWave;
        public int QueuedEvent = -1;
        public bool LetterSent;
        public bool RescuedToy;
        public bool ThankYouClaimed;
        public string TargetItem = "";
        public string TargetUnit = "";
        public string TargetWeapon = "";
        public string RewardId = "";
        public string Result = "";
        public List<SecretContract> Contracts = new List<SecretContract>();
        public List<SecretStitch> Stitches = new List<SecretStitch>();
        public List<string> ImprovedItems = new List<string>();
        public List<SecretAllyGift> AllyGifts = new List<SecretAllyGift>();
        public List<string> Journal = new List<string>();
    }
    public sealed class SecretChoice
    {
        public string Title, Copy, RewardId;
        public bool Enabled = true;
        public Sprite Icon;
    }

    [CreateAssetMenu(menuName = "Arcane Brigade/Secret Event Assets")]
    public sealed class SecretEventAssets : ScriptableObject
    {
        public Sprite[] Portals = new Sprite[16];
        public Sprite[] Relics = new Sprite[16];
        public UnitDefinition[] Rescues = new UnitDefinition[3];
        public UnitDefinition Guardian;
        public static SecretEventAssets Current => Resources.Load<SecretEventAssets>("SecretEventAssets");
        public static readonly string[] Titles = { "THE LOST-PROPERTY VAULT", "THE IMPOSSIBLE TAILOR", "THE UNDERSTUDY'S MIRROR", "THE CONTRABAND EXPRESS", "THE THIRTEENTH-HOUR BANK", "THE SLEEPING GARDEN", "THE EMPTY THEATRE", "THE DEAD-LETTER OFFICE" };
        public static readonly string[] Hints = {
            "Someone is knocking from inside a drawer.", "A red thread is sewing a door into the wall.", "Your reflection moved after you stopped.", "The little train has arrived without a timetable.",
            "The clock struck once too many.", "The lamp is asleep. Its shadow is still awake.", "There is applause behind the closed curtains.", "This letter has been waiting for your name." };
        public static readonly string[] EntryCues = { "shop.stand.appear", "wpn.needle.volley", "hero.mirror.guard", "menu.spot.printer", "hero.clock.runner", "hero.dream.breath", "ui.cutin.encore", "hero.parcel.express" };
        public static readonly string[] RewardItems = { "SecretWoodHeart", "SecretThread", "SecretMirror", "SecretTicket", "SecretHour", "SecretDream", "SecretCrown", "SecretLetter" };
    }

    public sealed partial class CheckpointCatalog
    {
        public int SecretEncounter; // 1 vault, 2 mirror, 3 train, 4 swarm, 5 solo actor, 6 duet
        public UnitDefinition SecretPartner;
        public bool SecretRecall, SecretMeleeEcho, SecretWard, SecretMirrorEcho;
    }
    public sealed partial class WeaponDefinition { public bool SecretRecall; }
}
