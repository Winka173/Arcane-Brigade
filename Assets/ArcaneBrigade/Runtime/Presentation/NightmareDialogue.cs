using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class NightmareDialogue
    {
        private static readonly string[][] Layers =
        {
            new[] {
                "Welcome! The little bell says you may stay.","Three fresh miracles. Please mind their teeth.",
                "That one would suit your peculiar little family.","No charge for looking. Sit anywhere but the corner.",
                "Sold! Take good care of that small impossibility.","It stopped trembling when you chose it.",
                "Just a drop. We do not sell your last heartbeat.","Keep enough blood to get everyone home.",
                "No hurry. A small purse still belongs to a person.","Come back with coins, not apologies.",
                "New stock! The old stock has gone for a nap.","Close your eyes. No, not all of them.",
                "Listen. Three little bells have found one voice.","A family resemblance! Bigger shoes this time.",
                "Follow the warm lights. I shall keep one for you.","See you after the noise. I will put the kettle on."
            },
            new[] {
                "You are back. Good. The street outside has gone quiet.","Do not mind the chair. It has always been empty.",
                "That ribbon was tied differently a moment ago.","If it looks at you, look at a friend instead.",
                "Sold. Something familiar to take into the dark.","Keep the receipt. It proves you were here.",
                "The night can wait. Your heart cannot.","A small red promise. Nothing more.",
                "You owe me nothing for standing in the light.","Not enough coins. There is still a place by the lamp.",
                "A different shelf. The same little shop. Surely.","The bell rang twice. I only touched it once.",
                "There. A clearer voice than anything outside.","They recognized each other before I did.",
                "If the corridor changes, remember the way your friend walks.","I will still be here. Please come back."
            },
            new[] {
                "The shelves remember your footsteps now.","I wrote your name. The receipt wrote the rest.",
                "They remember every crossing. Ask where they want to go.","The eye in the glass is not a price tag.",
                "Sold. It is a companion to your story, not its ending.","The drawer sighed. I think it wanted you to choose that.",
                "Blood is not a signature. Remember that.","Only the listed price. No hidden promises.",
                "No. An empty wallet does not make you merchandise.","Keep your breath. I do not take that currency.",
                "I moved the stock. Something moved the room.","Let us change the shelf before it changes us.",
                "For a moment I remembered what morning sounded like.","That light was yours. The shop did not make it.",
                "If you hear your name outside, let your friends answer first.","The warm cup is still real. I checked."
            },
            new[] {
                "You have seen the wrist. Please do not ask where it ends.","The shop keeps things because it is afraid of forgetting.",
                "That seam was a door once. It might be again.","Some things are held together by being remembered.",
                "Take it. Ownership is not the same as belonging.","Another line on the receipt. Not another chain.",
                "I can count your heartbeats. I will not claim them.","Let this be the last thing the dark asks from you.",
                "You are a guest. Whatever the other receipt says.","I would rather lose a sale than lose you.",
                "Different objects. The same fear underneath.","The cupboard is larger inside. Please do not count the ribs.",
                "They became stronger without forgetting who they were.","That is how a dream learns to let go.",
                "The hand is trying to help. It has forgotten how to ask.","There is a handle somewhere. It must still open something."
            },
            new[] {
                "The last chair is occupied. Do not sign anything.","I kept the lamp lit. I did not know what else to do.",
                "Choose what helps you leave. Not what helps me keep you.","That one is not afraid of morning anymore.",
                "Sold. No one can buy the friends who choose to follow.","Keep it. I am learning the difference between a gift and a sale.",
                "Your heart belongs on the other side of the door.","Only this price. Not your name. Never your name.",
                "I will not let him put a price on you.","No more bargains with the part of you that wants to live.",
                "One last shelf. Then we find the door.","I hear the kettle from the first room. Hold on to that.",
                "There you are. Still yourselves. All of you.","The dark cannot agree on your names. Your friends can.",
                "When the bell rings, walk toward the light together.","If I forget how to close, remind me that goodbye is not forgetting."
            }
        };
        private static readonly string[][] Dream =
        {
            new[] { "A small dream has hatched. Be gentle.","Look softly. That one startles easily.","The dream is yours. The friendship is its own.","Some dreams prefer an honest heartbeat.","Not enough. Your last heartbeat is not for sale.","Let the old dream sleep. Here are three more.","Three names, one bright little soul.","An egg is a promise with a shell. Carry yours gently." },
            new[] { "The windows have shut their eyes.","That one has heard the bell before.","Take a little certainty into the hall.","Leave enough warmth inside your chest.","Not enough. There is no shame in walking away.","I changed the dreams, not the price of being alive.","Listen. They remember the same song.","The empty chair dreamed of a hat. Do not wait for its owner." },
            new[] { "Do not confuse being watched with being known.","Its eyes open only when someone remembers it.","It chose to go with you. That matters.","A heartbeat is not proof of purchase.","Not enough. The dream can wait for your answer.","I put the sharp memories on a different shelf.","A small bright refusal to become forgotten.","The shelf is dreaming of a spine. Walk carefully." },
            new[] { "The hand remembers how small you are.","That ribbon once held the door open.","Let it belong beside you, not beneath you.","Keep the rest of your blood for the dawn.","Not enough. Do not pay with the part of you that wants to leave.","These dreams have not met the last customer yet.","He can count them. He cannot understand them.","The tongue was a carpet before the shop learned fear." },
            new[] { "He is here. He calls the whole shop inventory.","The eye is afraid to close. Help it remember morning.","Carry this beyond the edge of the receipt.","You are not the final item on his bill.","Not enough. Your last heartbeat is not for sale.","One more dream, then we wake the door.","He cannot buy what has become a family.","Choose your way out, little waking thing." }
        };
        private static readonly string[][] Whispers =
        {
            new[] { "The kettle is still warm.","The chair in the corner is facing you again.","A lantern on the road gutters, then holds.","Your friends know the way you walk." },
            new[] { "Somewhere down the platform, someone almost hums.","Dust settles on a shoulder that has not moved in years.","The rails are empty in both directions.","An unopened letter waits on every lap." },
            new[] { "The windows show the room you are standing in.","A pencil mark on the door frame is exactly your height, once.","The letterbox rattles, though there is no wind.","Footsteps in the hall stop a moment after yours." },
            new[] { "The sheets have been turned down for one.","The cup is exactly where your hand would reach.","A chair along the wall has your name on it.","Nobody in this room is asleep." },
            new[] { "The last lantern is still burning.","Somebody has left a light on.","The night goes on. So do you.","Hold on to the things that know you without a receipt." }
        };
        public static string Pick(RunSnapshot state, bool special, MerchantMoment moment, int cursor)
        {
            if (state == null) return MerchantDialogue.Pick(special,moment,cursor);
            int depth = state.Endless ? 4 : ActOneCanon.Zone(state.Wave);
            if (cursor % 4 == 3 && (moment == MerchantMoment.Enter || moment == MerchantMoment.Inspect || moment == MerchantMoment.Leave || moment == MerchantMoment.Buy))
            {
                string memory = Memory(state,Math.Abs(cursor / 4));
                if (!string.IsNullOrEmpty(memory)) return memory;
            }
            if (cursor % 3 == 0 && moment == MerchantMoment.Inspect)
                for (int i = MienChoStory.Available(state) - 1; i >= 0; i--)
                    if (((state.MienCho?.FinishedMask ?? 0) & (1 << i)) != 0) return MienChoStory.Chapters[i].Evidence;
            if (special) return Dream[depth][(int)moment];
            return Layers[depth][(int)moment * 2 + Math.Abs(cursor % 2)];
        }
        private static readonly string[][] Evidence =
        {
            new[] { "Three coins rest on the folded blanket. Nobody saw them arrive.", "The receipt under the till is in a child's handwriting.", "The shopkeeper never looks at the corner. Not once.", "The warm chair is one step closer than it was." },
            new[] { "Every traveller on the platform sits in a chair that waited for them.", "The lullaby on the speakers is older than you.", "Your name is on the departure board. DEPARTURE: DAWN.", "He tossed the coins through the window. They landed on the seat." },
            new[] { "The front door's lock was forced from the outside.", "A reply was sewn shut and nailed into the letterbox.", "Tally marks under the table. The last row is in your hand.", "He wore a kind face that was too small for him." },
            new[] { "One bed. One cup. A chair along the wall with your name.", "The brass key opens a door that was never in this wall.", "His ledger's first entry is his own name.", "The warm chair stands in front of the exit." },
            new[] { "The last lantern still holds a name.", "Somebody came eight times.", "The shop is still open for anyone who still has a choice.", "Dawn came once. The night keeps its own hours." }
        };
        public static string Whisper(RunSnapshot state, int cursor)
        {
            // Act I whispers follow the four zones; past dawn the endless night has its own.
            int depth = state.Endless || state.Wave > ActOneCanon.Waves ? 4 : ActOneCanon.Zone(state.Wave);
            if (depth > 0 && depth < 4 && !FinishedAt(state, depth * 5 + 1)) depth--;
            int variation = Math.Abs(cursor / 3 % 4) + ((state.Seed >> (depth * 3)) & 3);
            int evidence = variation % 4;
            string text = cursor % 3 == 2 && EvidenceKnown(state, depth, evidence) ? Evidence[depth][evidence] : Whispers[depth][Math.Abs(cursor % 4)];
            if (state.Wave == 19 && FinishedAt(state, 19)) text = "The last lantern holds a name he cannot buy.";
            if ((state.ActOne?.Offer ?? 0) == ActOneProgress.Sold && cursor % 4 == 1) text = "One of your companions introduces themself again, kindly, as if you had never met.";
            if (NightmareRules.Remembered(state, SecretEventKind.Understudy) && (text.Contains("glass") || text.Contains("window"))) text = "The familiar window holds its shape. Your reflection stays beside you.";
            if (NightmareRules.Remembered(state, SecretEventKind.ImpossibleTailor) && (text.Contains("shelf") || text.Contains("shelves"))) text = "The repaired shelf holds its shape. Something else in the room is breathing.";
            return text;
        }
        private static readonly int[][] EvidenceWaves = {
            new[] { 2, 1, 2, 3 }, new[] { 6, 7, 10, 7 },
            new[] { 12, 12, 11, 13 }, new[] { 16, 16, 19, 18 }, new[] { 20, 20, 20, 20 }
        };
        private static bool FinishedAt(RunSnapshot state, int wave)
        {
            if (wave == 20) return state.Nightmare?.CustomerDefeated == true;
            int finished = state.MienCho?.FinishedMask ?? 0;
            for (int i = MienChoStory.Available(state) - 1; i >= 0; i--)
                if (MienChoStory.Chapters[i].Wave == wave && MienChoStory.Chapters[i].Shop) return (finished & (1 << i)) != 0;
            return false;
        }
        private static bool EvidenceKnown(RunSnapshot state, int zone, int line)
            => FinishedAt(state, EvidenceWaves[zone][line]);
        public static string Memory(RunSnapshot state, int cursor)
        {
            NightmareProgress story = state.Nightmare;
            if (story == null || story.ResolvedMask == 0) return state.Secrets?.LetterSent == true ? "Your letter reached something beyond the stage. It is listening." : "";
            for (int offset = 0; offset < 8; offset++)
            {
                int kind = (cursor + offset) % 8;
                if ((story.ResolvedMask & (1 << kind)) == 0) continue;
                int choice = Mathf.Clamp(story.Choices[kind],0,2);
                string[][] lines = {
                    new[] { "The drawer is quiet now. Someone inside it went home with you.","The letter in the cabinet finally has someone to carry it.","You took its heart gently. The cabinet remembers." },
                    new[] { "That returning thread knows the way back to your hand.","The tailor left a second heartbeat inside your swing.","A little shelter, stitched into something you chose to keep." },
                    new[] { "Your reflection blinked first. You did not become it.","Your shadow is borrowed, not sold. We remember the difference.","You left the mirror without fighting yourself. That was allowed." },
                    new[] { "That ticket has a destination where the customer cannot follow.","The contraband thread was not on his inventory.","You faced the collector. This visitor is not your first." },
                    new[] { "The thirteenth hour was borrowed. Dawn is not.","A debt can be repaid. You are not collateral.","What you trusted to the bank still carries your name, not his." },
                    new[] { "The garden kept one small light awake for you.","You settled a promise beneath the sleeping lamp.","A dream friend is still a friend, even after visiting hours." },
                    new[] { "The paper audience learned to applaud instead of whisper.","You stood your ground when the leading actor stepped forward.","Two actors shared a stage. Neither could write your ending." },
                    new[] { "Your parcel has an address beyond these shelves.","Someone carries your letter close to their weapon.","Your invitation reached the other side of the curtain." }
                };
                return lines[kind][choice];
            }
            return "";
        }
        public static string Room(RunSnapshot state, SecretEventKind kind)
        {
            string[] early = { "Not every forgotten thing wants to be bought. Some want to be found.","A stitch can change a thing without erasing who kept it.","The reflection asks whether following you is the same as being you.","This train carries things that never belonged on a shelf.","The clock offers more time. It cannot promise a morning.","Here the shop remembers how to keep someone safe.","The audience is waiting to learn whether you are a person or a performance.","An address is a way of believing there is somewhere outside." };
            string[] late = { "The Last Customer counts missing stock. The drawer counts rescued friends.","The tailor can mend a seam. Perhaps even the one that sealed the door.","He wants obedient copies. The mirror offers you a chance to refuse.","Every unlisted parcel is a small hole in his perfect receipt.","He calls every promise a debt. The clock knows better.","The garden is not merchandise. Rest is not another purchase.","He bought the seats. He did not buy the right to choose your ending.","If a letter can leave the shop, perhaps all of you can." };
            return (NightmareRules.Depth(state.Wave) >= 3 ? late : early)[(int)kind];
        }
        /// <summary>Act I dawn (Docs/Mien_Cho_Act_One_Story.md, "Kết hồi"): four pages after closing the shop, five after
        /// leaving the last lantern lit. The waking page is always last.</summary>
        public static string Ending(RunSnapshot state, int page)
        {
            bool light = state.Nightmare?.Farewell == NightmareFarewell.LeaveLight;
            if (light && page == 3) return Lantern();
            if (page >= 3) return Waking();
            return page switch
            {
                0 => "THE CONTRACT BURNS\nThe Last Customer's face slips away one borrowed face at a time, until what is left is smooth and blank: his own, sold long ago.\nThe contract with your name on it catches fire on the counter. Under the last line there is a second signature. It is not his. It is the small shape of a chair.\nOne lantern is still burning on the road behind you. It holds a name, too bright to read here.",
                1 => (light ? "THE LAST LANTERN STAYS\nYou leave the last lantern burning at the crossing, so that whoever wakes here next can find the road." : "THE SHOP CLOSES\nYou carry the last lantern out with you, and the shopkeeper turns the sign on the door. Nobody new will wake up in front of it.")
                    + "\nThe shopkeeper does not ask to be forgiven. They hold the door until everyone is through.\n"
                    + (state.Secrets.LetterSent ? "The letter you sent from the stage is already in Postscript's bag, addressed in your own hand." : "Postscript still carries one unsent letter of yours. It will wait until you are ready to send it."),
                _ => "MORNING, TOGETHER\nIt is a real dawn. The light does not belong to anyone.\nYou ask, out of habit: 'Is there anything else you need me to do?'\nHob pulls out a chair. An ordinary chair, cold from the night. 'Sit here a while.'\nFor the first time, a chair is only a chair.\n"
                    + (state.Roster.Count > 0 ? "Your companions decide for themselves who goes on and who stays. Nobody asks you to earn the choice." : "Even alone, you carried enough light to reach the morning.") + "\n"
                    + (state.Secrets.RescuedToy ? "The creature you rescued takes its own first step into morning.\n" : "There are other lives still waiting along the platform.\n")
                    + (state.OutsideStage == OutsideStageStatus.Completed ? "The Keeper holds the frame of the gate and opens its hand." : "The Keeper stands at the open gate and lets every traveller through.")
                    + ((state.ActOne?.Offer ?? 0) == ActOneProgress.Sold ? "\nSomeone at the table pulls out a chair for you as well. You do not know who they are. They do it anyway." : "")
            };
        }
        private static string Lantern() => "FOR WHOEVER COMES NEXT\nThe shopkeeper keeps the lantern trimmed in the window.\nAlong the platform, a few of the travellers who sat down have lifted their heads toward the light. One of them has taken off their hat.\nIt is an invitation, not a debt. You do not have to come back to prove you remember.";
        private static string Waking() => "THURSDAY\nYou wake on the sofa in your own flat. The armchair beside you is just an armchair.\nYour phone still shows the message: 'See you tomorrow?' You answer it: 'Yes.'\nOn the doormat there is an envelope with no stamp and no postmark, as if someone carried it to the door by hand.\nToday is Thursday.";

        public static int AuthoredLineCount
        {
            get
            {
                int total = 24 + 16 + 4;
                foreach (string[] layer in Layers) total += layer.Length;
                foreach (string[] layer in Dream) total += layer.Length;
                foreach (string[] layer in Whispers) total += layer.Length;
                foreach (string[] layer in Evidence) total += layer.Length;
                return total;
            }
        }
    }
}
