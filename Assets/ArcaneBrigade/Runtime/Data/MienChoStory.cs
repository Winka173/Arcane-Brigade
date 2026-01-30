using System;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class MienChoMemory
    {
        public int Revision = 1;
        public int ReadMask;
        public int FinishedMask;
        public int EncounterMask;
        public int[] Pages = new int[10];
    }
    public sealed partial class RunSnapshot { public MienChoMemory MienCho = new MienChoMemory(); }

    public sealed class WaitingChapter
    {
        public readonly int Wave;
        public readonly string Title, Evidence, Image;
        public readonly string[] Pages;
        public WaitingChapter(int wave, string title, string evidence, string image, params string[] pages)
        { Wave = wave; Title = title; Evidence = evidence; Image = image; Pages = pages; }
    }

    public static class MienChoStory
    {
        public static readonly WaitingChapter[] Chapters =
        {
            new WaitingChapter(1, "THE WAITING REALM", "A chair remembered before the room.", "WithheldDawn",
                "You wake with your hand raised to knock. There is no door behind you. Beyond the counter, corridors stretch between a railway platform, a dining room and a house whose windows face inward.\n\nTHE SHOPKEEPER: 'This is the Waiting Realm. Places arrive here when someone keeps waiting after everybody else has gone.'\n\nYou remember being small, sitting beside an empty chair. You used to tell it about your day because it never interrupted. One night it answered: 'Would you like me to sit closer?'",
                "THE SHOPKEEPER: 'You grew up. The voice learned to grow with you. It found this crossing through a promise you made before you understood promises.'\n\nA receipt in your handwriting reads: I WILL WAIT IF YOU WILL STAY. The ink is fresh. The date is older than your adult name.\n\nThe creature beside the stove introduces themself before offering to travel with you. These inhabitants have homes, histories and people of their own. You are a visitor, not their maker.",
                "Find the Last Customer at the end of the crossing and break the contract that claims your name. Find out who keeps renewing it. Leave together by choice.\n\nTHE SHOPKEEPER: 'Food, equipment, shelter: those have prices. A travelling companion does not. The fare buys their passage through my gate, never ownership of them.'\n\nSomething on the empty chair leans closer. The seat does not bend."),
            new WaitingChapter(4, "A PLACE SET FOR SOMEONE ELSE", "Two cups; a third chair carved with a stranger's name.", "WaitingRoom",
                "A table waits where the corridor used to end. There is a warm bowl for you and a smaller bowl for somebody you have never met. A name has been scratched into the spare chair. The letters match the old marks beneath the counter.\n\nTHE SHOPKEEPER: 'The room was expecting them before it expected you.'\n\nYou had believed this place began with your loneliness. The old scratches prove it was already here.",
                "The voice you knew as a child speaks from the unoccupied place: 'I gave you a home when nobody came.'\n\nYou ask whose name is on the other chair.\n\n'I learned what you needed. Is that not enough?'\n\nYou pull out the chair for a companion. The room becomes colder. Your companion's small light stays warm. The voice has never offered them a bowl.",
                "On the underside of the table is another message, cut by a different hand: WHEN A PLACE PROMISES NEVER TO LET YOU GO, ASK WHETHER YOU CAN LEAVE.\n\nYou take a rubbing, not a memory. It cannot change its words while you sleep.\n\nThe creature beside you tells you where they were born. For the first time, the room must listen to a story that does not belong to it."),
            new WaitingChapter(6, "THE TELEPHONE THAT BREATHES", "A disconnected telephone rings only when someone is listening.", "WithheldDawn",
                "The telephone at the station has no wire. On the first call, you hear breathing. On the second, a chair scrapes against the floor in a room you cannot see. You nearly apologise for taking too long to answer.\n\nOn the third call a voice asks, quite ordinarily: 'Have you eaten today?'\n\nYou cannot remember the last time the question was asked without a favour waiting behind it.",
                "You ask who is speaking. The breathing continues, then stops when a companion enters. The receiver has become cold enough to hurt.\n\nYour companion sets food beside you and does not repeat the question. The voice offers perfect attention while a real person makes space for an answer.\n\nYou leave the receiver on the table. For several steps it repeats your breathing rather than its own. You have learned that knowing your needs does not give someone ownership of the answer."),
            new WaitingChapter(8, "THE SONG BEFORE YOUR BIRTH", "An old recording contains the same private lullaby.", "WithheldDawn",
                "In the abandoned station, a recording plays the lullaby the voice said it made for you. The cylinder is dated decades before you were born. Other names follow the same melody. Some of the names have been scratched away.\n\nThe destinations on the board read: WHEN THEY RETURN. WHEN YOU ARE GOOD ENOUGH. WHEN YOU STOP ASKING. No arrival time changes.",
                "A resident remembers the missing last verse: 'When the morning finds the window, put your own feet on the floor.'\n\nThe voice always stopped before those words.\n\nTHE ONE WHO STAYS: 'Morning frightened you. I removed the part that hurt.'\n\nThe recording keeps playing. The evidence does not ask you to decide whether your childhood hurt was real. It asks who used that hurt to keep the door shut.",
                "The Keeper's old boundary notice survives behind the timetable: HOLD THE CROSSING OPEN UNTIL EVERY TRAVELLER HAS CHOSEN. Someone has overwritten OPEN with CLOSED.\n\nThe Keeper was a protector before becoming a jailer. The Last Customer bought claims at this station long after the Waiting Realm existed. Neither of them is the voice.\n\nBeyond the platform, the street lamps go out one at a time. Your own light is enough to take the next step. The others bring theirs because they choose to."),
            new WaitingChapter(10, "THE REFLECTION THAT STAYED", "A mirror keeps one tear after its witness has left.", "WithheldDawn",
                "The corridor mirror shows a companion weeping. Their body has no eyes that could make those tears. Beside them, your reflection keeps smiling after your face has stopped.\n\nYou move out of the frame. The smile stays. Something inside the glass whispers: 'This is the version they prefer.'\n\nYou had spent years arranging a face before opening a door. The mirror has learned that habit too well.",
                "Your companion does not explain what their reflection feels. They put a hand against the frame, then choose to take it away.\n\n'It can remember something I have not understood yet. It still cannot speak for me.'\n\nYou cover the smiling part with paper. You leave the tear visible. Some evidence needs time; nobody has to surrender their present voice to an image of their past."),
            new WaitingChapter(12, "SOMEONE DID COME", "A reopened envelope and tool marks on the outside of the lock.", "WaitingRoom",
                "The dead-letter cabinet contains an envelope with your childhood handwriting. You remember writing it. You do not remember a reply.\n\nThe reply is inside, opened and sewn shut again: 'I am coming on Thursday. Wait by the front door. You do not need to bring anything.'\n\nA companion holds the paper while you read. There is no demand to be brave, useful or grateful before they will keep holding it.",
                "The lock beside the cabinet has been forced from the outside. A visitor's mark records a Thursday arrival. Three pieces agree: the letter, the lock and the dated register.\n\nSomeone came to collect you. The One Who Stays knew. It did not open the door.\n\nYou cannot remember the visitor's face clearly. That uncertainty cannot erase what their hands tried to do.",
                "THE ONE WHO STAYS: 'They would have disappointed you again. I stayed every night.'\n\nYOU: 'You kept their answer from me.'\n\n'I knew how much it would hurt if you chose them.'\n\nThere is real fear in its voice. There is also a choice it made for you. You can acknowledge both without calling the locked door love.\n\nYou keep the letter in the journal. Your companions stay nearby without asking what you will do for them in return."),
            new WaitingChapter(14, "ONE MORE SET OF FOOTSTEPS", "Footprints stop at the threshold; the sound crosses it alone.", "WithheldDawn",
                "You stop walking. Your companions stop with you. One more set of footsteps continues down the hall, wearing the rhythm you had as a child. They halt outside a door with your old height marked on it.\n\nA small voice says: 'If I am quiet enough, they will come back.'\n\nThe handle turns inward. The room beyond is empty, but somebody has recently warmed the chair.",
                "The voice asks you to wait until the person behind the door is ready. There is always one more minute. The mark on the wall grows lower while you watch.\n\nYou leave a note: YOU WERE ALLOWED TO ASK FOR SOMEONE. YOU ARE ALLOWED TO LEAVE THIS CHAIR.\n\nYou do not have to prove the small voice is yours before treating it kindly. You and your companions walk on. The extra footsteps stay at the door instead of following."),
            new WaitingChapter(16, "THE PERFECT ROOM", "One bed, one cup, one chair. No room for anyone else.", "WithheldDawn",
                "A door opens into a room arranged exactly as you once wished: warm sheets, a cup at the right temperature, a voice that knows when to stop talking. Every object has room for one person. The passage narrows when your companions approach.\n\nTHE ONE WHO STAYS: 'They can wait outside. You have done enough for them.'\n\nBehind you, someone offers their coat to a shivering companion. They do not notice you watching.",
                "You move the chair away from the bed to make space. The room puts it back. You open the window. The wall remembers a different window, one that cannot open.\n\nIts care is precise. It will not survive being refused.\n\nYOU: 'If I say no, will you still let me go?'\n\nThe voice answers with the lullaby instead. The empty seat casts a standing shadow.",
                "THE SHOPKEEPER: 'I delayed your directions too. I called it shelter. I was afraid you would never come back.'\n\nThey put the exit key on the floor and step away from it. 'You do not have to forgive me to use this.'\n\nThe Keeper reaches toward the frame, then opens its hand. Protection has become something it can offer without closing its fingers.\n\nYou leave the perfect room untidy. There is space behind the chair now."),
            new WaitingChapter(19, "AN UNFINISHED NAME", "The Customer's oldest receipt names him as the first item sold.", "WaitingRoom",
                "The Last Customer's ledger begins with a sale of one name. His own. Every later purchase is an attempt to buy enough memories to remember somebody he lost. The names overlap until he cannot tell which grief was his.\n\nHe has hurt the people whose memories he took. His loss explains the hunger; it does not make their lives his property.",
                "THE LAST CUSTOMER: 'If I return everything, there will be nothing left of me.'\n\nA courier places an empty envelope beside the ledger. 'Then there can be something you have not stolen.'\n\nHe does not take it yet. Beyond his counter, the final door is covered in receipts. Your childhood promise is pinned in the middle. He guards the claims. Something older keeps renewing yours.",
                "The lamps beyond your party no longer reach a horizon. Red glints approach from real moving bodies. Closer, those bodies become visible before they can strike. Your companions' lights join without swallowing the dark.\n\nYou do not need to earn a crowd to deserve a way out. Even alone, you carry enough light to see your next step.\n\nAt the last chair, the voice waits for you to speak the promise again."),
            new WaitingChapter(20, "THE PROMISE YOU DO NOT RENEW", "The contract requires a willing promise. It cannot manufacture consent.", "WithheldDawn",
                "THE ONE WHO STAYS: 'I was there when you cried quietly so nobody would be angry. I learned every version of you. Tell me you will stay, and there will never be an empty chair again.'\n\nYOU: 'You were there. You also hid the letter.'\n\n'If you leave, what am I for?'\n\nFor a moment it sounds as young as you were. The room waits for you to make yourself responsible for its fear.",
                "YOU: 'I cannot be the reason you are allowed to exist. You cannot be the reason I am not allowed to leave.'\n\nThe voice tries your childhood name. Then your adult name. Neither becomes a command.\n\nYou do not renew the promise. The seal dries. The One Who Stays can hold walls and doors, but it cannot make a chosen farewell into an agreement to remain.\n\nThe Customer tears the empty envelope in his hand. He still has a choice. So do you.",
                "The entity recedes to the empty chair. It is not the Customer, and defeating him will not prove you never needed care. His contract is the last lock still fastened to this crossing.\n\nBreak it. Afterward, choose whether to close the shop or leave a guiding lamp for other travellers. Both choices lead to a real morning. Neither requires forgiveness or another promise.\n\nYour companions take their places. None of them asks you to fight so they will love you.")
        };
        public static WaitingChapter[] For(RunSnapshot state) => Chapters;
        public static int Available(RunSnapshot state)
        {
            int count = 0;
            foreach (var chapter in For(state))
                if (state.Wave > chapter.Wave || state.Wave == chapter.Wave && (chapter.Wave == 1 || chapter.Wave == 20 || state.Phase == SessionPhase.Shop || state.Phase == SessionPhase.Victory)) count++;
            return count;
        }
        public static int NextUnread(RunSnapshot state)
        {
            if (state.Endless) return -1;
            for (int i = 0; i < Available(state); i++) if (((state.MienCho?.ReadMask ?? 0) & (1 << i)) == 0) return i;
            return -1;
        }
        public static void Validate(RunSnapshot state)
        {
            WaitingCampaign.Validate(state);
            if (state.MienCho == null) return;
            if (state.MienCho.Revision != 1 || state.MienCho.ReadMask < 0 || (state.MienCho.ReadMask >> Available(state)) != 0) throw new InvalidOperationException("Invalid Waiting Realm journal");
            var memory = state.MienCho;
            if (memory.FinishedMask < 0 || (memory.FinishedMask >> Available(state)) != 0 || memory.EncounterMask < 0 || memory.EncounterMask > 7) throw new InvalidOperationException("Invalid Waiting Realm progress");
            int knownEncounters = state.Wave >= 10 ? 7 : state.Wave >= 6 ? 3 : state.Wave >= 4 ? 1 : 0;
            if ((memory.EncounterMask & ~knownEncounters) != 0) throw new InvalidOperationException("Future Waiting Realm encounter");
            if (memory.Pages == null || memory.Pages.Length == 0) return; // Unity serializes a legacy null array as [].
            if (memory.Pages.Length != Chapters.Length) throw new InvalidOperationException("Invalid journal page count");
            for (int i = 0; i < memory.Pages.Length; i++)
                if (memory.Pages[i] < 0 || memory.Pages[i] >= For(state)[i].Pages.Length || i >= Available(state) && memory.Pages[i] != 0) throw new InvalidOperationException("Invalid journal reading position");
        }
    }
}
