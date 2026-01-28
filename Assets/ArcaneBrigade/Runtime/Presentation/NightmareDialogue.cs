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
                "That one would suit your peculiar little family.","No charge for looking. The chair is free, too.",
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
                "It remembers every owner. You may teach it kindness.","The eye in the glass is not a price tag.",
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
            new[] { "The kettle is still warm.","A little bell answers your footsteps.","Someone has left a light on.","Your friends know the way you walk." },
            new[] { "The chair has moved closer to the counter.","The window no longer reflects the street.","Your shadow stops. A friend waits for it.","The lamp is exactly where you remember it." },
            new[] { "A shelf inhales after you pass.","The receipt remembers a purchase you made.","Something behind the glass looks away first.","A familiar little gesture makes the room feel real." },
            new[] { "The same ribbon is tied around the wrist and the door.","The shop is trying to remember the shape of outside.","Someone has repaired a heart instead of replacing it.","The dark changes. Your friends still recognize you." },
            new[] { "The last chair is no longer empty.","There is no price written beneath the lamp.","A door handle waits under the final fold.","Hold on to the things that know you without a receipt." }
        };
        public static string Pick(RunSnapshot state, bool special, MerchantMoment moment, int cursor)
        {
            if (state == null) return MerchantDialogue.Pick(special,moment,cursor);
            int depth = NightmareRules.Depth(state.Wave);
            if (cursor % 4 == 3 && (moment == MerchantMoment.Enter || moment == MerchantMoment.Inspect || moment == MerchantMoment.Leave || moment == MerchantMoment.Buy))
            {
                string memory = Memory(state,Math.Abs(cursor / 4));
                if (!string.IsNullOrEmpty(memory)) return memory;
            }
            if (special) return Dream[depth][(int)moment];
            return Layers[depth][(int)moment * 2 + Math.Abs(cursor % 2)];
        }
        private static readonly string[][] Evidence =
        {
            new[] { "One chair has been reserved. Nobody remembers making the reservation.", "A blank receipt rests beneath the till. There is no price on it.", "The shopkeeper checks the latch, then leaves your side of it open.", "The little cup is warm. Not every comfort is a trick." },
            new[] { "The chair has one more leg than its shadow.", "The toys face the wall. One small friend still faces you.", "Someone has polished the empty place where a name should be.", "The shopkeeper counts the guests. Then counts the chairs again." },
            new[] { "The grain of the wood follows something like a pulse.", "The light beyond the glass closes like an eyelid. It was not a door.", "A glove waits beside a receipt that has never been paid.", "Your friend repeats a familiar gesture. For a moment, the room settles." },
            new[] { "The shelves bend inward. The shop is remembering a body.", "The keeper's hand belongs to this dreaming place. The visitor only wants to own it.", "A name has been rubbed away until the paper is almost transparent.", "The shopkeeper hides the blank receipt before the chair can see it." },
            new[] { "Everything has stopped pretending not to wait.", "He remembers every price. There is one name he cannot say.", "The shopkeeper's smile holds a little too long. The lamp stays warm.", "The door is not asking for another purchase. It is waiting for goodbye." }
        };
        public static string Whisper(RunSnapshot state, int cursor)
        {
            int depth = NightmareRules.Depth(state.Wave);
            int variation = Math.Abs(cursor / 3 % 4) + ((state.Seed >> (depth * 3)) & 3);
            string text = cursor % 3 == 2 ? Evidence[depth][variation % 4] : Whispers[depth][Math.Abs(cursor % 4)];
            if (state.Wave == 19) text = "The last bell has not rung yet. He remembers every price, but not the name.";
            if (NightmareRules.Remembered(state, SecretEventKind.Understudy) && (text.Contains("glass") || text.Contains("window"))) text = "The familiar window holds its shape. Your reflection stays beside you.";
            if (NightmareRules.Remembered(state, SecretEventKind.ImpossibleTailor) && (text.Contains("shelf") || text.Contains("shelves"))) text = "The repaired shelf holds its shape. Something else in the room is breathing.";
            return text;
        }
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
        public static string Ending(RunSnapshot state, int page)
        {
            return page switch
            {
                0 => "THE RECEIPT ENDS HERE\nThe contract breaks. Beneath the mask, a man holds a receipt with no name.\nYour friends were never his to buy. Your freedom is not another debt.",
                1 => "THE SHOPKEEPER\nI thought keeping the door open would keep you from forgetting me.\nBut goodbye is not the same as being forgotten.\n" + (state.Secrets?.LetterSent == true ? "Your letter showed me there was still an outside." : "You came back without being asked. That was never a sale."),
                2 => "MORNING, TOGETHER\n" + (state.Secrets?.RescuedToy == true ? "The friend from the drawer crosses first. No one is left inside it." : state.Roster.Count > 0 ? "Your little brigade pushes the door open together." : "You push the door open. The warmth you carried follows you.") + "\n" + (state.OutsideStage == OutsideStageStatus.Completed ? "The great hand holds the frame steady. This time it lets go." : "The great hand opens. It finally knows how to let go."),
                3 => "SOME THINGS REMAIN\nA lantern. A cup still warm. A folded wing that moves before the breeze.\nYou are awake. Your friends were not erased." + (state.Nightmare.Farewell == NightmareFarewell.LeaveLight ? "\nA small key rests beside them. A promise, not a chain." : "\nThe shop closes. You do not have to return to prove you remember."),
                _ => "ONE LIGHT LEFT ON\nYou are outside. Morning is real.\nSomewhere beneath the empty counter, three quiet knocks answer the lamp.\nThe key has no price. The name on the receipt is still missing.\nYou may return one day. Not because you owe him."
            };
        }
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
