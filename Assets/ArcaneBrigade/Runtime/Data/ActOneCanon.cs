using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Act I "Someone Is Waiting" (Docs/Mien_Cho_Act_One_Story.md): four descending zones, the eight lamps
    /// of the Thursday visitor and the fixed story beats. Pure data; presentation and saves read it.</summary>
    public static class ActOneCanon
    {
        public const int Waves = 20;
        public static readonly string[] ZoneNames = { "THE CROSSING", "THE LAST PLATFORM", "THE HOUSE WITH INWARD WINDOWS", "THE PERFECT ROOM" };
        public static int Zone(int wave) => Mathf.Clamp((Mathf.Max(1, wave) - 1) / 5, 0, 3);
        public static bool ZoneStarts(int wave) => wave > 1 && wave <= Waves && (wave - 1) % 5 == 0;

        // Story bosses close each zone. Wave 20 stays the Last Customer's final hall.
        public static bool BossWave(int wave) => wave >= 5 && wave <= Waves && wave % 5 == 0;
        public const string KeeperWave = "Keeper";
        public static string BossName(int wave) => wave switch { 5 => "THE KEEPER", 10 => "THE CHAIR PORTER", 15 => "THE DOOR THAT STAYED SHUT", _ => null };

        // Each lamp is one of the eight times the visitor came looking. The eighth never goes out in Act I.
        public static readonly int[] BreakWave = { 3, 5, 8, 10, 13, 14, 17, int.MaxValue };
        public static readonly string[] LampHeld = { "their voice", "the colour of their raincoat", "how tall they were", "the smell of rain on their coat", "their face", "the way they knocked", "their handwriting", "their name" };
        public static string LampLoss(int lamp) => lamp >= 0 && lamp < 7 ? "You can no longer remember " + LampHeld[lamp] + "." : "";
        public static int Broken(int wave)
        {
            int count = 0;
            for (int lamp = 0; lamp < BreakWave.Length; lamp++) if (wave >= BreakWave[lamp]) count++;
            return count;
        }
        /// <summary>0 lit, 1 flickering (breaks this wave or next, or the last lamp near dawn), 2 broken.</summary>
        public static int LampState(int lamp, int wave)
        {
            if (lamp < 0 || lamp >= BreakWave.Length) return 0;
            if (wave >= BreakWave[lamp]) return 2;
            if (lamp == 7) return wave >= 19 ? 1 : 0;
            return BreakWave[lamp] - wave <= 1 ? 1 : 0;
        }

        // A relit lamp (won from the warm chair, or bought back from the Customer) burns again whatever its wave.
        public static int Broken(int wave, int relitMask)
        {
            int count = 0;
            for (int lamp = 0; lamp < BreakWave.Length; lamp++) if (LampState(lamp, wave, relitMask) == 2) count++;
            return count;
        }
        public static int LampState(int lamp, int wave, int relitMask) => (relitMask & (1 << lamp)) != 0 ? 0 : LampState(lamp, wave);
        /// <summary>The lamp a relighting restores: the most recently broken one that is still dark.</summary>
        public static int RelightTarget(int wave, int relitMask)
        {
            for (int lamp = BreakWave.Length - 1; lamp >= 0; lamp--) if (LampState(lamp, wave, relitMask) == 2) return lamp;
            return -1;
        }

        // The four travelling companions of the canon have their own places in the party (never bought, merged or sold).
        // They reuse existing species bodies: Hob the hearth spirit, Latch the Keeper's guard, Postscript the courier, Tally the hunter.
        public static readonly string[] StoryIds = { "TeapotPilgrim", "HoodWisp", "ShellPostman", "PaperDevil" };
        public static readonly string[] StoryNames = { "Hob", "Latch", "Postscript", "Tally" };
        public static readonly int[] JoinShop = { 1, 5, 6, 11 }; // they meet you in the shop after this wave
        public static bool StoryPresent(int index, int wave, bool shop, bool endless)
        {
            if (endless || wave > Waves) return true;
            if (shop ? wave < JoinShop[index] : wave <= JoinShop[index]) return false;
            if (index == 3 && (shop ? wave == 18 : wave == 19)) return false; // Tally is gone from shop 18 until it returns in shop 19
            return true;
        }
        public static int StoryTier(int wave) => wave >= 16 ? 3 : wave >= 11 ? 2 : 1;
        public static int StoryCount(int wave, bool shop, bool endless)
        {
            int count = 0;
            for (int index = 0; index < StoryIds.Length; index++) if (StoryPresent(index, wave, shop, endless)) count++;
            return count;
        }

        // The warm chair creeps from the far corner of the shop toward the exit, one position per shop visit (1..19).
        public static Vector2 ChairPosition(int wave)
        {
            // Down the right-hand edge of the shop: from the far corner under the bonds card (right of the merchant's speech
            // bubble), past the lantern-less strip, round the bottom-right corner below the stock, and on the last visit
            // beside the exit. Every position is reserved in ShopLayout, so no stall, offer or decoration is ever under it.
            if (wave >= 19) return new Vector2(1.9f, -4.75f);
            Vector2[] path = { new Vector2(6.75f, 0.55f), new Vector2(6.85f, -1.7f), new Vector2(6.0f, -4.5f), new Vector2(4.75f, -4.6f) };
            float t = Mathf.Clamp01((Mathf.Clamp(wave, 1, 18) - 1) / 17f) * (path.Length - 1);
            int segment = Mathf.Min(path.Length - 2, Mathf.FloorToInt(t));
            return Vector2.Lerp(path[segment], path[segment + 1], t - segment);
        }
        /// <summary>What lies on the blanket this visit: 0 nothing, 1 coins, 2 the murmuring jar, 3 the breathing receiver.</summary>
        public static int ChairOffering(int wave, int telephoneChoice) => wave == 2 ? 2 : wave == 8 && telephoneChoice == 1 ? 3 : wave is 4 or 7 or 9 or 12 or 13 or 16 or 19 ? 1 : 0;
        public const int SitFrom = 8, SitUntil = 19, SitWaveBonus = 8;
        public const int HobChapter = 16; // the journal beat of the perfect room

        // The relic for standing up from the warm chair. It lives here rather than in the shop catalogue: it is never sold.
        public const string ThursdayShoesId = "act1-thursday-shoes";
        private static ItemDefinition thursdayShoes;
        public static ItemDefinition ThursdayShoes
        {
            get
            {
                if (thursdayShoes != null) return thursdayShoes;
                thursdayShoes = new ItemDefinition
                {
                    Id = ThursdayShoesId, Name = "Thursday Shoes", Rarity = ItemRarity.Mythic, Stacking = StackPolicy.Unique, MaximumStacks = 1,
                    Description = "The small shoes you laced every evening, in case someone came. This time you answered the door. +12% movement speed, +6% dodge.",
                    Effect = ItemEffect.MoveSpeed, Value = 0.12f, ShopWeight = 0, Price = 0,
                    Modifiers = new[]
                    {
                        new StatModifier { Effect = ItemEffect.MoveSpeed, Value = 0.12f, Scope = RunModifierRules.DefaultScope(ItemEffect.MoveSpeed) },
                        new StatModifier { Effect = ItemEffect.Dodge, Value = 0.06f, Scope = RunModifierRules.DefaultScope(ItemEffect.Dodge) },
                    },
                    Icon = Resources.Load<Sprite>("MienCho/ActOne/Item_ThursdayShoes") ?? Resources.Load<Sprite>("MienCho/Lanterns/Intact"),
                };
                return thursdayShoes;
            }
        }
        public static ItemDefinition StoryItem(string id) => id == ThursdayShoesId ? ThursdayShoes : null;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetItems() => thursdayShoes = null;
        public const float KnockSeconds = 80f;

        /// <summary>Lines that only exist if the traveller did something: appended to a beat's page.</summary>
        public static string PageExtra(RunSnapshot state, int chapter, int page)
        {
            if (chapter == 19 && page == 1 && state?.ActOne?.SitCloser == ActOneProgress.Won)
                return "\n\nHe stops. For the first time the pleasant face forgets to smile.\n\nTHE LAST CUSTOMER: '…You did. Who paid?'\n\nHOB: 'Nobody. We pulled.'";
            return "";
        }

        // Journal art keys per zone; MienChoPresentation falls back to the older panels while new art is pending.
        public static readonly string[] ZonePanel = { "Panel_Crossing", "Panel_Platform", "Panel_House", "Panel_PerfectRoom" };

        public static WaitingChapter[] Beats() => new[]
        {
            Beat(1, false, "ARRIVAL", "Your hand was raised to knock on nothing.",
                "Before.\n\nA late message lights the dark flat: 'See you tomorrow?'\n\nYou do not answer yet. It has been a long time since anyone asked. You lie down on the sofa beside the empty armchair and tell yourself you will reply in the morning.\n\nThe armchair is the last thing you see. You do not remember it being turned toward you.",
                "You wake standing up, your hand raised to knock.\n\nThere is no door in front of you. There is a shop: a lit window, a bell above an open threshold, a counter the colour of old honey. Beyond it the dark goes on in every direction, and eight lanterns hang along the road, burning.\n\nSomething comes out of the dark toward the light. You are already fighting before you understand that you are afraid."),

            Beat(1, true, "THE WARM ONE", "A child's chair that is never cold.",
                "Inside, the shopkeeper sets down a cup of tea without asking whether you want one.\n\nIn the far corner stands a small wooden chair, the kind that belongs beside a child's bed. A folded blanket lies on the seat. It is turned toward you.\n\nTHE SHOPKEEPER: 'Sit anywhere you like.' They do not look at the corner. 'Not that one. Never the warm one.'\n\nYou had not touched it. You already knew it was warm.",
                "A receipt lies on the counter in a child's careful handwriting: I WILL WAIT IF YOU WILL STAY. Beneath it, in another hand: HELD BY THE LAST CUSTOMER.\n\nTHE SHOPKEEPER: 'Those lanterns outside. While they burn, your name is still yours. At dawn, the last one goes out.'\n\nA small hearth spirit pours your tea and drinks its own standing up. Its name is Hob. You ask whether it needs anything.\n\nHOB: 'Need? I'm the one who asks that.'"),

            Beat(2, true, "HE NEVER USES THE COUNTER", "Coins on the seat of the warm chair.",
                "A gentleman is already in the shop when you return: a good coat, a good hat, and a face that will not hold still. When you look straight at him it is pleasant. At the edge of your eye it is somebody else's.\n\nHe does not go to the counter. He lays three coins on the folded blanket of the warm chair, and when he lifts his hand there is a small stoppered jar where the coins were. Something inside it is murmuring.\n\nHe tips his hat to you as if you were an old acquaintance.\n\nTHE LAST CUSTOMER: 'We'll be seeing a lot of each other.'\n\nThe shopkeeper keeps their eyes on the till until the bell stops ringing."),

            Beat(3, true, "THE FIRST LAMP", "One lantern on the road has gone dark.",
                "During the fight, one of the lanterns on the road went out. You heard it crack.\n\nYou try to remember something, and there is only the shape where it used to be.\n\nYou can no longer remember their voice.\n\nYou do not know whose. That is the worst part.",
                "Hob drops a cup. It is the first thing Hob has ever dropped; the shopkeeper says so, quietly, as they sweep it up.\n\nYou ask about the chair.\n\nTHE SHOPKEEPER: 'Some furniture comes with the building.'\n\nWhen you look again, the chair is one step closer than it was."),

            Beat(4, true, "IT USED TO PULL PEOPLE UP", "A notice board on a distant gate.",
                "Far down the road, past the last lit lantern, there is a gate in a stone arch. Something tall stands in front of it.\n\nTHE SHOPKEEPER: 'The Keeper. It used to pull people up. Travellers who sat down where they shouldn't. It would take them by the shoulders and stand them on their feet and point them home.' They wipe the same glass twice. 'Now it only keeps them in.'\n\nYou ask what changed.\n\n'Someone rewrote its orders.'\n\nThere are coins on the warm chair again. Nobody saw anyone come in."),

            Beat(5, true, "THE KEEPER KNEELS", "OPEN, written over as CLOSED.",
                "The notice on the gate reads HOLD THE CROSSING OPEN UNTIL EVERY TRAVELLER HAS CHOSEN. Someone has written CLOSED over OPEN in brown receipt ink.\n\nThe Keeper fought you the way a parent catches a child who runs into the road: hard, and never quite to hurt. When it falls, it kneels instead of breaking.\n\nTHE KEEPER: 'Last time, you did not get this far.'\n\nAnother lantern goes dark behind you. You can no longer remember the colour of their raincoat.",
                "One of the Keeper's guards follows you back to the shop. It is a small hooded thing called Latch, and it does not explain itself. It simply takes the place on your left and stays there, facing the warm chair.\n\nWhen you step outside, the shop is no longer where it was. The bell rings above a different threshold, at the edge of a railway platform."),

            Beat(6, true, "THE LAST PLATFORM", "Everyone on the platform sits in a chair of their own.",
                "The platform has no end. Along it, in their own mismatched chairs, sit the travellers who came before you: a dining chair, a rocking chair, a stool, a bench seat with a suitcase beside it. Their coats are grey with dust. They face the empty rails.\n\nNobody moves. Nobody is asleep.\n\nA courier made of a folded envelope walks among them, leaving a letter on each lap. None of the letters is ever opened. When it sees that you are still standing, it follows you.",
                "Its name is Postscript. In the shop it tries to deliver a letter to the warm chair. The envelope curls, blackens and burns away on the blanket without a flame.\n\nYou ask Postscript whether it needs anything.\n\nPOSTSCRIPT: 'A reply. Any reply.'\n\nThe shopkeeper says nothing at all about the travellers on the platform. Later you understand that every one of them is sitting in a chair that was waiting for them."),

            Beat(7, true, "A SONG OLDER THAN YOU", "A lullaby recorded before you were born.",
                "During the fight, a recording began to play over the station speakers: the lullaby that was sung to you every night when you were small, the one you were told was made only for you.\n\nThe label on the cylinder is dated decades before your birth.\n\nAll along the platform, the dusty travellers hummed it with you. Every one of them stopped at the same place, where the last verse should be. Nobody knows how it ends.",
                "The Last Customer does not come in this time. Latch stands in the doorway and he stays outside the window, smiling, and tosses his coins through the open pane. They land on the warm chair without a sound.\n\nTHE SHOPKEEPER: 'He's never once used the counter.' It is the first thing they have told you that you did not ask."),

            Beat(8, true, "THE TELEPHONE THAT BREATHES", "A shop telephone with no wire.",
                "Another lantern went dark on the platform. You can no longer remember how tall they were.\n\nOn the counter there is a telephone that you are certain was not there before. Nothing connects it to any wall. It is ringing.",
                "You leave it ringing. It rings until the bell over the door rings with it, and then it stops.\n\nWhen you look back, the receiver is lying on the folded blanket of the warm chair, its cord trailing into nothing. You can hear breathing through it from across the room.\n\nThe chair is one step closer.",
                "You lift the receiver. A child is speaking, quietly, so as not to wake anyone. It is your voice, from before your voice broke.\n\n'Is anyone coming?'\n\nBefore you can answer, another voice answers for you, very gently.\n\n'No.'\n\nThe line clicks. The receiver is cold enough to burn.",
                "The Last Customer is waiting at the end of the platform when you leave.\n\nTHE LAST CUSTOMER: 'Sell me your memory of one of them, and I'll light a lamp for you.' He nods at your companions as though they were stock on a shelf. 'You'd hardly miss it.'\n\nThe shopkeeper says, too quickly, 'If you ever sat in it—' and then, 'No. Never mind.'"),

            Beat(9, true, "STILL ON THE SEAT", "The coins are always placed on the blanket.",
                "There are coins on the warm chair again. You count the visits now. He has never once laid them on the counter.\n\nYou ask the shopkeeper who he is paying.\n\nTHE SHOPKEEPER: 'Not me.'\n\nThey will not say anything else. Hob refills your cup, standing."),

            Beat(10, true, "DEPARTURE: DAWN", "Your name on the departure board.",
                "The Chair Porter came down the platform carrying a tower of chairs strapped to its back, and on every chair sat a traveller who did not wake. The chair at the very top was empty. Your name was carved into the back of it.\n\nWhen it fell, the tower fell with it. Nobody woke.\n\nThe departure board turned over. Under WHEN YOU STOP ASKING there is your name, and beside it a departure time: DAWN.\n\nAnother lantern is out. You can no longer remember the smell of rain on their coat.",
                "Some of the travellers from the tower have come with the shop. They sit along its walls now, facing the door.\n\nTHE SHOPKEEPER: 'Ten more.' You do not know whether they mean waves or chairs.\n\nThere is not much food left. Everyone shares what there is. You try to give your portion to Hob. Hob will not take it, and for once does not explain why."),

            Beat(11, true, "HOME, WRONG", "A house whose windows face inward.",
                "The road leads into a house. It is the house you grew up in, exact to the last scuff on the skirting board, except that every window looks inward, at the room you are standing in.\n\nThe dining table is set for two. One place is yours. The other is set before an empty chair.\n\nUnder the table, in the dark, there are tally marks scratched into the floor. Under the table, in the dark, something is hiding.",
                "Its name is Tally. It used to hunt runaways for the Last Customer, and its body is covered in neat carved marks, in groups of five.\n\nTALLY: 'Each one is somebody I brought back.'\n\nIt asks to come with you. Nobody trusts it.\n\nThe shop now stands where the kitchen used to be. Latch watches Tally across the counter.\n\nLATCH: 'You let them go last time.'\n\nTally does not deny it."),

            Beat(12, true, "THE LETTER SEWN SHUT", "An envelope opened, then sewn closed again.",
                "The letterbox by the front door has been nailed shut. Postscript opens it anyway; couriers may open any letterbox, it says, and the nails come out in its paper hands.\n\nInside is a letter you wrote when you were small, asking somebody to come. Folded into it is the reply, opened once and sewn shut again with coarse thread.\n\n'I am coming on Thursday. Wait by the front door. You do not need to bring anything.'\n\nThe lock on the front door has been forced from the outside.",
                "Back in the shop, the Last Customer comes through the door for the first time. He crosses to the warm chair and kneels beside it, the way you would kneel beside a child, and says something very softly.\n\nTHE LAST CUSTOMER: 'The face, please.'\n\nYou see the whole thing. Nobody stops him. You are not sure anyone could."),

            Beat(13, true, "KIND EYES", "He wore it as if it had always been his.",
                "In the hallway, during the fight, the Last Customer stood watching. The face he wore was gentle and damp with rain, a little too small for his head, and you knew it before you knew why.\n\nOutside, a lantern burst.\n\nYou can no longer remember their face. You can only see it on him.\n\nTHE LAST CUSTOMER: 'They had kind eyes, didn't they?'",
                "For the first time, the shopkeeper looks directly at the warm chair.\n\nTHE SHOPKEEPER: 'Not that one.' They say it to the chair, not to you, and their hands are shaking.\n\nThere are coins on the blanket."),

            Beat(14, true, "THE KNOCK", "Footsteps that do not stop when you do.",
                "When you stop walking, one set of footsteps keeps going. It is an adult's step, slow and careful, and it goes to the front door, the one with your childhood height marked in pencil on the frame.\n\nSomebody knocks.\n\nFrom your left, very close to your ear, someone whispers: 'That's not them. Hide.'",
                "You hide. You are very good at it; you have done it before.\n\nThe knocking goes on. Someone says your childhood name through the door, kindly, as if they have been looking for a long time. Then the footsteps go away.\n\nYou remember now. Thursday. The bed. The dust under it. A voice telling you that the thing at the door takes children who complain. Someone calling your name. You, holding your breath until they left.",
                "You open the door. The chain stops it a hand's width from the frame.\n\nThrough the gap you see a pair of wet boots turning away down the path, unhurried, as if they have done this before and will do it again.\n\nYou remember now. Thursday. The bed. The dust under it. A voice telling you that the thing at the door takes children who complain. Someone calling your name. You did not open it then.",
                "Another lantern is gone. You can no longer remember the way they knocked.\n\nIn the shop, nobody speaks. Hob brings a blanket, an ordinary one, and puts it round your shoulders. Tally sits under the counter where it cannot be seen.\n\nPostscript gives you the letter from Thursday.\n\nPOSTSCRIPT: 'It's addressed to you. It always was.'"),

            Beat(15, true, "THE DOOR OPENS", "A closed umbrella, still wet, on the step.",
                "The front door fought you with its chains and its letterbox mouth, and the knocking never stopped on the other side the whole time.\n\nWhen it fell, it opened for the first time.\n\nIt is raining outside. There is nobody there. There is only an umbrella, closed and still wet, leaning against the step, as if someone stood there a long time and then had to go.\n\nYou bring it with you.",
                "The shop has opened beside the front door. Rain falls past the window.\n\nThe warm chair is beside the exit now. To leave, you will have to walk past it."),

            Beat(16, true, "THE PERFECT ROOM", "A bed for one. A chair with your name.",
                "The last room is your bedroom, made perfect: warm sheets, a cup of water exactly where your hand would reach, a voice that knows when to stop talking. There is one bed.\n\nAlong the walls, the travellers who sat down sit facing the bed. Among them is an empty chair with your name carved into it.\n\nAnd among them, in a chair of its own, sits Hob.\n\nThe chair has promised Hob the one thing Hob has never had: 'You have done enough. You may rest now. Forever.' Hob does not answer. Dust has begun to settle on its shoulders.",
                "One of you cannot lift it. You try anyway, and then Latch takes the other side, and Postscript, and Tally, and everyone who came with you, and Hob comes up out of the chair like something pulled out of deep water.\n\nAs you turn to leave, one of the seated travellers turns its head to watch you go.\n\nThe shopkeeper is waiting at the door.\n\nTHE SHOPKEEPER: 'I sat down once. The Keeper pulled me up. I have been slow with your directions on purpose. I was afraid that if you left, nobody would ever come back.' They set a brass key on the floor and step away from it. 'You do not have to forgive me to use this.'"),

            Beat(17, true, "A STOOL", "Hob chooses where to sit.",
                "Another lantern is out. In the journal, the letter's handwriting fades into plain print while you watch. You can no longer remember their handwriting.\n\nIn the shop, Hob sits down for the first time: on an ordinary stool by the stove, because it chose to.\n\nYou ask whether it needs anything.\n\nHOB: 'Yes. For you to stop asking for a minute.'",
                "Across the room, Tally is reading a receipt it thinks nobody can see. When it notices you, it folds the paper very small and puts it away."),

            Beat(18, true, "TALLY IS GONE", "An empty place under the counter.",
                "Tally fought beside you this time as if it were somewhere else. In the shop, it is gone.\n\nThere are more travellers sitting along the walls than there were. The warm chair stands directly in front of the exit. To leave, you have to brush past the blanket.\n\nIt is warm through your sleeve."),

            Beat(19, true, "AN UNFINISHED NAME", "The first thing he ever sold.",
                "The brass key opens a door in your bedroom wall that was never there. Behind it is the Last Customer's counter and his ledger, a book so large it rests on a lectern.\n\nThe first entry is a sale. One name: his own. The price of standing up.\n\nThe ink is the same brown ink that wrote CLOSED over OPEN on the Keeper's gate.\n\nTally is standing at the counter. It has laid out every name it ever brought back and is asking for them to be returned. Nobody has promised it anything.",
                "In the last shop, the Last Customer comes himself. He lays his coins on the warm chair and waits for the last lamp, the one that holds the visitor's name.\n\nNothing appears on the blanket.\n\nTHE SHOPKEEPER: 'It can't sell you that. Their name was never its to sell.'\n\nHe straightens his hat. The pleasant face slips, and slips back.\n\nTHE LAST CUSTOMER: 'Then you'll say it yourself. Nobody stands up without paying.'\n\nTally comes back through the door and takes its place without a word."),

            Beat(20, false, "THE PROMISE YOU DO NOT RENEW", "The contract needs you to say it again.",
                "Behind the counter the Last Customer rises from a chair made of receipts. The face he wears is the kind one, the one you lost.\n\nTHE LAST CUSTOMER: 'Just say it once more. I will wait if you will stay. Then you can rest, and I can keep my face, and nobody has to be afraid of mornings.'\n\nOne lantern is still burning on the road behind you. Its light is very small.\n\nYou do not say it."),
        };

        private static WaitingChapter Beat(int wave, bool shop, string title, string evidence, params string[] pages)
            => new WaitingChapter(wave, title, evidence, ZonePanel[Zone(wave)], pages) { Shop = shop };

        // Wave 8's telephone and wave 14's door are choices: page 1 or 2 is shown depending on what the traveller did.
        public static int ChoicePage(int chapter, int choice) => chapter == 8 || chapter == 14 ? 1 + Mathf.Clamp(choice, 0, 1) : -1;
        public static readonly string[][] ChoiceOptions = new string[21][];
        static ActOneCanon()
        {
            ChoiceOptions[8] = new[] { "LET IT RING", "ANSWER IT" };
            ChoiceOptions[14] = new[] { "HIDE", "OPEN THE DOOR" };
        }
    }
}
