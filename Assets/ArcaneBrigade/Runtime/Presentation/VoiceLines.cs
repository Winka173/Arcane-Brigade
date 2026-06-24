using System;
using System.Collections.Generic;

namespace ArcaneBrigade
{
    /// <summary>One voiced bark: its clip lives at Resources/Audio/Voice/&lt;Speaker&gt;/&lt;File&gt; (wav/ogg) and its subtitle is
    /// exactly <see cref="Text"/>. An empty text means a wordless sound (a hum, a grunt) whose subtitle comes from elsewhere.</summary>
    public sealed class VoiceLine
    {
        public readonly string Speaker, Trigger, File, Text, Note;
        public VoiceLine(string speaker, string trigger, string file, string text, string note) { Speaker = speaker; Trigger = trigger; File = file; Text = text; Note = note; }
        public string Path => "Audio/Voice/" + Speaker + "/" + File;
        public override string ToString() => Speaker + "/" + File;
    }

    /// <summary>The voice script (Docs/Audio/Voice_Prompts.md is generated from this table: keep each entry as
    /// "spoken text|delivery note"; file names are numbered in order, trigger_01, trigger_02...). Lines are short on
    /// purpose: two to six words for repeatable barks, longer only for one-off story beats.</summary>
    public static class VoiceLines
    {
        public const string Shopkeeper = "Shopkeeper", DreamMother = "DreamMother", Keeper = "Keeper", ChairPorter = "ChairPorter",
            Door = "Door", LastCustomer = "LastCustomer", Bedside = "Bedside", CoinWidow = "CoinWidow", Hob = "Hob", Latch = "Latch", Postscript = "Postscript",
            Tally = "Tally", Companion = "Companion";
        private static readonly Dictionary<string, VoiceLine[]> banks = new Dictionary<string, VoiceLine[]>(StringComparer.Ordinal);
        private static readonly List<VoiceLine> all = new List<VoiceLine>();
        public static IReadOnlyList<VoiceLine> All => all;
        public static VoiceLine[] Bank(string speaker, string trigger) => banks.TryGetValue(speaker + "/" + trigger, out VoiceLine[] lines) ? lines : Array.Empty<VoiceLine>();
        public static int Count(string speaker) { int total = 0; foreach (VoiceLine line in all) if (line.Speaker == speaker) total++; return total; }

        private static void B(string speaker, string trigger, params string[] entries)
        {
            var lines = new VoiceLine[entries.Length];
            for (int index = 0; index < entries.Length; index++)
            {
                string[] parts = entries[index].Split('|');
                lines[index] = new VoiceLine(speaker, trigger, trigger + "_" + (index + 1).ToString("00"), parts[0], parts.Length > 1 ? parts[1] : "");
                all.Add(lines[index]);
            }
            banks[speaker + "/" + trigger] = lines;
        }

        static VoiceLines()
        {
            // THE SHOPKEEPER: kind, tired, deep and unhurried. Never looks at the warm chair. Never jokes at the traveller's expense.
            B(Shopkeeper, "enter",
                "Come in. Mind the step.|warm welcome, holding the door",
                "Ah. You made it.|quiet relief, a breath out first",
                "Lamp's lit. Come in.|low, practical, kind",
                "Back again. Good.|a small smile you can hear",
                "Shop's open. Take your time.|unhurried, settling in",
                "Welcome in, traveller.|old-fashioned courtesy",
                "Sit anywhere. Well. Almost.|the pause matters; do not glance anywhere",
                "Still you. Good.|checking you over, relieved",
                "Shake the dark off. Come.|gentle, like brushing snow off a coat",
                "Still open. Still here.|tired but steady");
            B(Shopkeeper, "purchase",
                "Good choice.|approving nod, deep",
                "Mind its teeth.|dry, fond warning",
                "That one suits you.|warm, sincere",
                "Sold. Carry it well.|ceremonial, soft",
                "Fair trade.|short, satisfied",
                "Take it. It wants out.|conspiratorial, low",
                "It'll keep you warm.|tender",
                "Wrapped. No ribbon, sorry.|small tired humour, still kind",
                "Hold it by the handle.|practical advice",
                "That one's been waiting.|knowing, a little sad",
                "A sensible thing.|calm approval",
                "Done. Off the shelf.|brisk, warm");
            B(Shopkeeper, "purchase_rare",
                "Oh. That one.|genuinely impressed, a hush",
                "Careful. That's a rare one.|reverent, slower",
                "I wondered who'd take that.|curious, pleased",
                "Now that's worth the walk.|a rare warm chuckle in the voice",
                "Hold it with both hands.|serious, protective",
                "Old stock. Very old.|remembering",
                "That one chose you back.|soft wonder",
                "Don't lose that one.|earnest, almost a plea");
            B(Shopkeeper, "purchase_companion",
                "Be kind to each other.|fatherly, quiet",
                "Not for keeping. Walk together.|firm on the first half, gentle on the second",
                "Another set of footsteps.|pleased, listening",
                "They chose to come.|respectful",
                "Good company. Look after them.|warm instruction",
                "Nobody walks alone in here.|steady promise");
            B(Shopkeeper, "reroll",
                "Let's see what else.|rummaging, easy",
                "Bottom drawer, then.|a grunt of effort",
                "New shelf. Same shop.|wry, patient",
                "Hm. Try these.|thoughtful",
                "Something else woke up.|curious, slightly wary",
                "Fresh from the back.|helpful",
                "Not those? Fair.|accepting, no offence taken",
                "Let me look again.|patient");
            B(Shopkeeper, "cant_afford",
                "Not today, I'm afraid.|apologetic, gentle",
                "Short a coin or two.|kind, matter-of-fact",
                "Purse is light, friend.|sympathetic",
                "Keep your coins. Come back.|reassuring",
                "Not enough. No shame in it.|warm, firm on 'no shame'",
                "That one costs more.|soft, not stern",
                "Next time, maybe.|hopeful",
                "I can't. I'm sorry.|truly sorry, low");
            B(Shopkeeper, "leave",
                "I'll leave the lamp on.|a promise",
                "Go carefully.|quiet blessing",
                "Come back. Please.|the 'please' slips out, he is afraid of being left",
                "Stay in the light.|urgent but soft",
                "Keep each other close.|parental",
                "Mind the road.|habitual farewell",
                "The kettle will be on.|warm, homely",
                "Don't sit down out there.|serious, low, not looking anywhere");
            B(Shopkeeper, "idle",
                "Take your time.|relaxed",
                "No rush. None at all.|easy, slow",
                "Kettle's still warm.|homely murmur",
                "Browse. I'll wait.|patient, a little too patient",
                "Hm. Quiet night.|to himself",
                "Look all you like.|generous");
            B(Shopkeeper, "chair",
                "Not that one.|flat, quick, eyes fixed elsewhere",
                "Not that chair. Please.|strained calm",
                "Sit anywhere else.|gentle but firm",
                "Don't. Not that one.|a sharp breath first",
                "Leave it be.|very quiet",
                "That one's not for you.|steady, kind, final");
            B(Shopkeeper, "blood",
                "Just a drop.|careful, gentle",
                "Keep enough to get home.|protective",
                "Careful. That's your heartbeat.|worried",
                "Only what it costs.|honest, reassuring");
            B(Shopkeeper, "story",
                "Sit anywhere you like. Not that one. Never the warm one.|shop 1: the only rule, said kindly, never looking at the chair",
                "If you ever sat in it... no. Never mind.|shop 8: starts to confess, stops himself",
                "Not that one!|shop 13: the first time he looks straight at the chair and shouts at it, not at you",
                "You do not have to forgive me to use this.|shop 16: he sets down the key; ashamed, steady",
                "They do not owe you a word. Put down the receipt.|final fight: standing up to the Last Customer");

            // MOTHER OF A THOUSAND EGG-EYES (the dream altar, optional side shop): soft, many-voiced, lullaby-slow.
            B(DreamMother, "enter",
                "Shh. A dream opened an eye.|whisper, delighted",
                "Come closer. Softly.|beckoning",
                "I kept your egg warm.|maternal",
                "So many eyes. All for you.|dreamy, layered");
            B(DreamMother, "purchase",
                "Hatch it gently.|tender",
                "The shell is yours.|ceremonial",
                "It knows your heartbeat now.|intimate",
                "Carry it like a secret.|hushed");
            B(DreamMother, "cant_afford",
                "Not enough. Not tonight.|sorry, soft",
                "Your last heartbeat stays yours.|protective",
                "The dream can wait.|patient");
            B(DreamMother, "reroll",
                "Blink. New dreams.|playful hush",
                "Let these ones sleep.|lullaby",
                "Three more, still warm.|pleased");
            B(DreamMother, "fuse",
                "Two names sink. One wakes.|ritual, slow",
                "Hold still. Threading.|concentrating",
                "Three little histories. One.|wonder",
                "Wake up, little one.|joyful whisper");
            B(DreamMother, "blood",
                "Only a little blood.|gentle",
                "Keep the rest for dreaming.|caring",
                "A red signature. Safe.|solemn");
            B(DreamMother, "leave",
                "Go. Don't look back.|warning, soft",
                "Dream well, wanderer.|blessing",
                "The eggs will remember you.|fond");

            // THE COIN WIDOW (boss, wave 5): collects for the Last Customer; counts the Keeper out of its own gate like a debt.
            B(CoinWidow, "enter",
                "Payment is due, traveller.|silken, counting coins",
                "The gate is closed. I closed it.|polite, proprietary",
                "Everything here is spoken for.|a ledger voice",
                "Mind the receipts.|amused warning");
            B(CoinWidow, "attack",
                "Pay.|clipped",
                "Every penny.|greedy relish",
                "Interest accrues.|cold, smiling",
                "You owe for the road.|matter-of-fact",
                "Count them. Count them!|shrill, eager",
                "Nothing is free.|the house rule");
            B(CoinWidow, "hurt",
                "That will cost you.|offended",
                "My ledger!|startled",
                "Careless spending.|tutting");
            B(CoinWidow, "phase",
                "He paid for this gate.|losing composure",
                "Stay where you are owed.|the Customer's order, recited",
                "The chains are paid in full.|clutching");
            B(CoinWidow, "defeat",
                "The account... is closed.|a last sigh",
                "He will collect from you.|fading threat",
                "Keep your change.|bitter, letting go");
            B(CoinWidow, "story",
                "Everyone here is collateral.|explaining, kindly cruel");

            // THE KEEPER, WHOLE (optional boss, shop 17): vast, old, gentle-stern. It carried the shop with its hand, its
            // carpet and its eye; before it gives the colours it tests whether the traveller can carry them.
            B(Keeper, "enter",
                "Let me see what you carry.|vast, slow, kind",
                "Stand. Show me.|a gate speaking",
                "You came back for the colours.|ancient recognition",
                "One more test. Then I give.|patient");
            B(Keeper, "attack",
                "Hold on to them.|command, protective",
                "Stand up straight.|pushing, heavy",
                "Again. Hold.|steady",
                "Do not let go.|urgent",
                "Can you carry this?|testing",
                "Feet under you.|the old order, true again");
            B(Keeper, "hurt",
                "You have grown strong.|surprised respect",
                "Hm. Again.|stoic",
                "Well struck.|approval",
                "Not like last time.|remembering the last loop");
            B(Keeper, "phase",
                "My hand remembers you.|the hand breaks",
                "The road was never mine to keep.|slower",
                "Look. Through the keyhole.|the eye opens");
            B(Keeper, "defeat",
                "Enough. You can carry it.|kneeling, proud",
                "Take them. All the colours.|giving",
                "Pull them up. All of them.|a last order, its true one");
            B(Keeper, "story",
                "I cannot walk the road with you.|quiet",
                "But I can carry you across it.|warm, enormous");

            // THE CHAIR PORTER (boss, wave 10): weary, creaking, carries a tower of chairs and sitters to the Customer as collateral.
            B(ChairPorter, "enter",
                "Another delivery.|weary sigh",
                "Heavy night. Heavy load.|creaking, slow",
                "Your chair's at the top.|matter-of-fact, unsettling",
                "Ah. The one with the name.|tired recognition");
            B(ChairPorter, "attack",
                "Sit. It's easier.|tired persuasion",
                "Up you go. On the pile.|grunting lift",
                "Mind the legs.|habit of a mover",
                "Everyone sits eventually.|resigned",
                "Collateral. Nothing personal.|bored apology",
                "Off your feet.|effort");
            B(ChairPorter, "hurt",
                "My back...|groan",
                "Careful! They're sleeping.|protective of its load",
                "Don't rock the tower.|alarmed",
                "Ow. Old joints.|creak");
            B(ChairPorter, "phase",
                "The tower's leaning.|panic rising",
                "Hold on up there!|shouted upward",
                "Too many chairs. Too many.|overwhelmed");
            B(ChairPorter, "defeat",
                "Put them down... gently.|collapsing, careful to the end",
                "Nobody woke. Nobody.|hollow",
                "Finally. Set it down.|relief");
            B(ChairPorter, "story",
                "Departure: dawn.|reading the board, flat",
                "When you stop asking.|reading the board, slower");

            // THE DOOR THAT STAYED SHUT (boss, wave 15): wood, hinges, chains; the letterbox is its mouth. The knocking never stops.
            B(Door, "enter",
                "Nobody's home.|wooden groan, low",
                "Stay inside.|chains settling",
                "Locked. From in here.|a bolt sliding",
                "Don't answer it.|hushed, urgent");
            B(Door, "attack",
                "Shut. Shut. Shut.|slamming rhythm",
                "Stay away from the door.|warning",
                "The chain holds.|rattling",
                "No one came.|cold, certain",
                "Quiet. They'll hear you.|hiss through the letterbox",
                "Back from the letterbox.|snap");
            B(Door, "hurt",
                "Splinters...|crack",
                "The hinge... the hinge.|straining",
                "Don't. Don't open me.|frightened",
                "The chain is slipping.|alarmed");
            B(Door, "phase",
                "Still knocking. Still knocking.|unravelling",
                "Somebody's on the step.|dread",
                "It's only the rain.|lying to itself");
            B(Door, "defeat",
                "...Open.|the first breath of outside air",
                "It's raining out there.|wonder",
                "They waited so long.|sorrow");
            B(Door, "story",
                "There was someone there.|a confession",
                "An umbrella. Still wet.|barely audible");

            // THE MID-MAP BOSSES (waves 3, 8, 11, 13, 17, 19): each carries off the memory of its night unless beaten.
            B("SleepCollector", "enter",
                "Shh. Their voice is mine now.|lullaby, hushed");
            B("SleepCollector", "phase",
                "Close your eyes. Stop listening.|sing-song, strained");
            B("SleepCollector", "defeat",
                "Keep it, then. Keep the voice.|yawning away");
            B("ChoirEngine", "enter",
                "Every letter sings for me.|booming chord");
            B("ChoirEngine", "phase",
                "Louder! Drown out the knocking!|discordant howl");
            B("ChoirEngine", "defeat",
                "The pipes... go quiet...|wheezing last chord");
            B("WaxAdmiral", "enter",
                "Melt it down. Every memory.|stately, dripping");
            B("WaxAdmiral", "phase",
                "Hold the line! Hold the wick!|captain's bark, melting");
            B("WaxAdmiral", "defeat",
                "Burnt down to the wick.|guttering");
            B("LastLantern", "enter",
                "One flame left. Mine.|cold flicker");
            B("LastLantern", "phase",
                "I'll snuff the face you keep.|hissing");
            B("LastLantern", "defeat",
                "Then burn for them.|fading glow");
            B("MarqueeMaw", "enter",
                "Tonight only! Your last act!|showman roar");
            B("MarqueeMaw", "phase",
                "Encore! I said ENCORE!|frantic applause");
            B("MarqueeMaw", "defeat",
                "Curtain... down...|bulbs dying one by one");
            B("ScarletIndex", "enter",
                "Every name, filed and forgotten.|dry rustle");
            B("ScarletIndex", "phase",
                "Strike it from the record!|scratching quill");
            B("ScarletIndex", "defeat",
                "Their name... still on the card.|whisper");

            // THE LAST CUSTOMER (boss, wave 20): polite, smooth, unsettling. Wears faces he bought. Never uses the counter.
            B(LastCustomer, "enter",
                "Ah. We meet properly.|courteous, pleased",
                "We'll be seeing a lot of each other.|canon line, silk",
                "Shall we settle the bill?|polite menace",
                "Good evening. Shall we?|a bow in the voice");
            B(LastCustomer, "attack",
                "Everything has a price.|smooth",
                "Just a small signature.|coaxing",
                "Let me take that off you.|helpful, wrong",
                "Allow me.|gentlemanly",
                "Do sit down.|an invitation that is an order",
                "Pay attention.|cold underneath");
            B(LastCustomer, "hurt",
                "Careful with the merchandise.|annoyed, still polite",
                "Rude.|clipped",
                "That will cost you.|promise",
                "Tsk. My face.|vain, rearranging");
            B(LastCustomer, "phase",
                "Let me try another face.|voice shifting",
                "Do I look familiar now?|sly",
                "This one suits me better.|admiring himself");
            B(LastCustomer, "defeat",
                "I sold my own name first.|confession, faces slipping",
                "No face left to wear.|blank, smooth",
                "The contract... burns.|disbelief");
            B(LastCustomer, "story",
                "Just say it once more. I will wait if you will stay.|beat 1: coaxing the promise back out",
                "They had kind eyes, didn't they? I can keep them for you.|beat 3: wearing the visitor's face",
                "Nobody stands up without paying.|beat 5: final, cold",
                "...You did. Who paid?|shop 19 after SIT CLOSER was won: genuinely shaken");

            // THE BEDSIDE (hidden boss, SIT CLOSER): soft, intimate, wrong. Always whispers from the left.
            B(Bedside, "enter",
                "Shh. It's only me.|close-mic whisper",
                "You came back to bed.|tender, wrong",
                "There you are.|too warm",
                "Just us now.|possessive softness");
            B(Bedside, "attack",
                "That's not them. Hide.|canon, urgent whisper",
                "Don't answer the door.|pleading",
                "Stay. Stay with me.|clinging",
                "Lie still.|lullaby cadence",
                "Shh, shh, shh.|rocking",
                "Nobody's coming.|sweet and cruel");
            B(Bedside, "hurt",
                "Why would you do that?|wounded",
                "I listened. Always.|guilt-trip",
                "After everything?|hurt",
                "You need me.|insistent");
            B(Bedside, "phase",
                "I'll sing it again.|humming under the words",
                "Only for you.|intimate",
                "The knocking will stop.|soothing, threatening");
            B(Bedside, "defeat",
                "...They came, didn't they?|small, defeated",
                "Don't open the...|cut off",
                "You'll be so alone.|last attempt");
            B(Bedside, "story",
                "I will wait if you will stay.|the child's promise, in its mouth",
                "Would you like me to sit closer?|after the credits; very soft");

            // The four who travel with you (canon voices from ActOneCanon / CreatureLives).
            B(Hob, "join",
                "Tea first. Then the road.|hearth spirit, cosy, brisk",
                "Need? I'm the one who asks that.|canon, amused",
                "I'll walk with you a while.|warm");
            B(Hob, "starup",
                "Fire's up. Still pouring.|bright",
                "Warmer now. Stay close.|protective",
                "Kettle's on. Let's go.|ready");
            B(Hob, "story",
                "Nobody here is for sale. Not even the ones who sat down.|final fight, quietly furious",
                "Nobody. We pulled.|shop 19 after SIT CLOSER, simple");
            B(Latch, "join",
                "I stand on your left.|guard, clipped",
                "Last time I stopped you. Not now.|blunt honesty",
                "Orders changed. I didn't.|dry");
            B(Latch, "starup",
                "Left side. Held.|soldierly",
                "Nothing gets past the left.|certain",
                "Stronger. Still watching it.|eyes on the chair");
            B(Postscript, "join",
                "A reply. Any reply.|canon, hopeful, papery",
                "Letters for everyone. Even you.|cheerful courier",
                "I'll carry what you can't.|kind");
            B(Postscript, "starup",
                "Sealed and stamped.|crisp",
                "Heavier bag. Lighter step.|pleased",
                "Every letter gets delivered.|vow");
            B(Tally, "join",
                "Each one is somebody I brought back.|canon, low, guilty",
                "Don't trust me. Fine.|defensive",
                "I let you go once.|quiet");
            B(Tally, "starup",
                "No new marks. Not on you.|a vow",
                "Sharper. On your side now.|grim",
                "I'll hunt for you now.|determined");
            B(Tally, "story",
                "I gave the list back.|shop 19: returning, tired, not asking forgiveness");

            // Every other companion species: wordless sounds only, so one bank fits all bodies. Their subtitles are their own words.
            B(Companion, "join",
                "|small pleased hum, rising",
                "|curious chirp",
                "|soft 'mm!' of greeting",
                "|tiny happy gasp",
                "|shy little laugh",
                "|eager 'hup!'");
            B(Companion, "starup",
                "|bright surprised gasp, then a laugh",
                "|proud 'ha!'",
                "|delighted trill",
                "|deep satisfied breath",
                "|a cheer, small but loud",
                "|an amazed 'ooh'");
        }
    }
}
