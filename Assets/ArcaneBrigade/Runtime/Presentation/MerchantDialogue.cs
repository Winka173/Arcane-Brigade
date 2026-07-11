using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum MerchantMoment { Enter, Inspect, Buy, BloodBuy, Denied, Reroll, Fuse, Leave }
    public sealed class MerchantDialogue
    {
        private string line;
        private float began;
        private float expires;
        private int cursor;
        private string focus;
        private float nextInspect;
        private string lastTrigger;
        private GUIStyle copy;
        private GUIStyle name;
        public int SpokenCount { get; private set; }
        public MerchantMoment LastMoment { get; private set; }
        public string CurrentLine => line;
        public string MerchantName { get; set; }
        public float ExpiresAt => expires;
        public float SpokeAt => began;
        public RunSnapshot Context { get; set; }
        /// <summary>When set, every spoken line also appears as a speech box pinned to this world point (the merchant's head).</summary>
        public string BubbleKey { get; set; }
        public Func<Vector3?> Anchor { get; set; }
        /// <summary>The last voiced bark, if the line was one (its subtitle is the start of <see cref="CurrentLine"/>).</summary>
        public VoiceLine LastBark { get; private set; }
        public static string Bank(bool special) => special ? VoiceLines.DreamMother : VoiceLines.Shopkeeper;
        public int VisibleGlyphs(float now, bool reduced) => line == null ? 0 : reduced ? line.Length : Mathf.Clamp(Mathf.FloorToInt((now - began) * 58), 0, line.Length);
        public bool IsSpeaking(float now) => line != null && now >= began && now < expires && VisibleGlyphs(now, false) < line.Length;

        public static string Pick(bool special, MerchantMoment moment, int variant)
        {
            string[] choices;
            if (special)
                choices = moment switch
                {
                    MerchantMoment.Enter => new[] { "Shh... a dream has just opened one eye.", "Come in. I saw you inside an egg." },
                    MerchantMoment.Inspect => new[] { "Do not stare too long. That one stares back.", "Will you take it home... or will it take you?", "I accept eggs. Some wares prefer blood." },
                    MerchantMoment.BloodBuy => new[] { "Only a little blood. Keep the rest for dreaming.", "A beating heart makes a beautiful bargain." },
                    MerchantMoment.Buy => new[] { "Keep the shell. The dream inside is yours.", "A new owner. Do not let it remember the old name." },
                    MerchantMoment.Denied => new[] { "Not enough. I will not buy your final heartbeat.", "An empty purse echoes too. Sadly, I do not accept echoes." },
                    MerchantMoment.Reroll => new[] { "Blink. I shall change the dream underneath.", "Three things that have not happened. Choose one." },
                    MerchantMoment.Fuse => new[] { "Two names sink. One name wakes." },
                    _ => new[] { "Go. Do not look into the dream behind you." }
                };
            else
                choices = moment switch
                {
                    MerchantMoment.Enter => new[] { "Clang! Market open. All three eyes can see your purse!", "Welcome! Today's receipts are still fresh." },
                    MerchantMoment.Inspect => new[] { "That one has not bitten anyone... this shift.", "Look all you like. I am not charging for looking. Yet!", "One eye on the wares, two on your purse." },
                    MerchantMoment.Buy => new[] { "Sold! I chew the coins; you get the miracle.", "A receipt longer than a monster's life. Take it!", "Clink, clink! Another dream has an owner." },
                    MerchantMoment.Denied => new[] { "Is your purse asleep? Wake it and come back!", "I hear jingling... but those are my teeth." },
                    MerchantMoment.Reroll => new[] { "Old stock into my belly, new stock onto the stands!", "Three new wares! My third eye picked them." },
                    MerchantMoment.Fuse => new[] { "Two in, one out! Do not question my arithmetic." },
                    _ => new[] { "Head down! Nightmares are not covered by warranty." }
                };
            string[] extra = moment switch
            {
                MerchantMoment.Enter => special ? new[] { "I kept your dream warm beneath the counter.", "There are more chairs than there used to be." } : new[] { "Mind the threshold. It is older than the shop.", "A warm lamp, a little company. Browse slowly." },
                MerchantMoment.Inspect => special ? new[] { "That shell remembers a different moon.", "Some gifts prefer a quiet owner." } : new[] { "Try the handle. A good tool should know your hand.", "The small print is asleep. I shall read it for you." },
                MerchantMoment.Buy => special ? new[] { "I tied a ribbon around the part that remembers.", "Careful with the seams. The dream is still young." } : new[] { "A fair bargain. Keep the change for the stairs.", "One less lonely thing on my shelf." },
                MerchantMoment.Denied => new[] { "Take your time. The shelf is not going anywhere.", "Keep your last coin. You may need a way home." },
                MerchantMoment.Reroll => new[] { "Let me see what is hiding in the bottom drawer.", "New faces, familiar prices. Give or take a coin." },
                MerchantMoment.Fuse => new[] { "Three little histories. One weapon to remember them.", "Hold still. I am threading their names together." },
                MerchantMoment.BloodBuy => new[] { "Leave yourself a heartbeat for the road.", "A red signature. I will keep it safe." },
                _ => new[] { "I will leave the lamp on.", "Bring your friends back. The kettle can wait." }
            };
            int selected = Mathf.Abs(variant % (choices.Length + extra.Length));
            return selected < choices.Length ? choices[selected] : extra[selected - choices.Length];
        }

        /// <summary>The voiced bark each committed moment uses (Inspect stays a written aside with murmurs).</summary>
        public static string Trigger(MerchantMoment moment) => moment switch
        {
            MerchantMoment.Enter => "enter",
            MerchantMoment.Buy => "purchase",
            MerchantMoment.BloodBuy => "blood",
            MerchantMoment.Denied => "cant_afford",
            MerchantMoment.Reroll => "reroll",
            MerchantMoment.Fuse => "fuse",
            MerchantMoment.Leave => "leave",
            _ => null
        };

        /// <summary>Speak a committed moment. <paramref name="trigger"/> narrows the bark (purchase_rare, purchase_companion, or
        /// "story:N" for a numbered story line); the written line is the bark itself, and on entry the bark is followed by the
        /// shop's depth-aware line.</summary>
        public void Speak(bool special, MerchantMoment moment, float now, string trigger = null)
        {
            LastMoment = moment;
            string bank = Bank(special);
            string wanted = trigger ?? Trigger(moment);
            // the same bark is still being heard: let it finish rather than stacking a second line over it
            if (wanted != null && wanted == lastTrigger && line != null && now < expires && VoiceDirector.Speaking(bank))
            {
                SpokenCount++; expires = Mathf.Max(expires, now + 1.4f); nextInspect = now + 2.2f;
                return;
            }
            VoiceLine bark = Bark(bank, wanted, moment == MerchantMoment.Enter || moment == MerchantMoment.Leave || wanted != null && wanted.StartsWith("story") ? VoiceDirector.Important : VoiceDirector.Normal);
            string canon = NightmareDialogue.Pick(Context, special, moment, cursor++);
            LastBark = bark; lastTrigger = bark != null ? wanted : null;
            bool story = wanted != null && wanted.StartsWith("story");
            line = bark == null ? canon : bark.Text; // one greeting at a time, never two joined in one bubble
            began = now;
            float voiced = bark != null ? VoiceDirector.Remaining(bank) : 0;
            expires = now + (bark == null || moment == MerchantMoment.Enter && !story ? Mathf.Max(3.8f, line.Length / 32f + 1.5f) : Mathf.Max(2.6f, line.Length / 22f + 1.4f, voiced + 0.6f));
            SpokenCount++;
            nextInspect = now + (moment == MerchantMoment.Inspect ? 1.6f : 2.2f);
            Pin(special, moment == MerchantMoment.Inspect ? 1 : 2);
        }

        /// <summary>A bark outside the purchase flow (idle, the warm chair): voiced and pinned, without changing the moment.</summary>
        public bool Say(bool special, string trigger, float now, int priority = VoiceDirector.Normal)
        {
            if (MergeReveal.Busy) return false;
            string bank = Bank(special);
            VoiceLine bark = trigger.StartsWith("story:") ? Bark(bank, trigger, priority) : VoiceDirector.Say(bank, trigger, priority);
            if (bark == null || string.IsNullOrEmpty(bark.Text)) return false;
            LastBark = bark; lastTrigger = trigger;
            line = bark.Text; began = now; expires = now + Mathf.Max(2.6f, line.Length / 22f + 1.4f, VoiceDirector.Remaining(bank) + 0.6f);
            SpokenCount++; nextInspect = now + 2.2f;
            Pin(special, priority);
            return true;
        }

        private static VoiceLine Bark(string bank, string trigger, int priority)
        {
            if (trigger == null) return null;
            if (trigger.StartsWith("story:"))
            {
                VoiceLine exact = VoiceDirector.Exact(bank, "story", int.TryParse(trigger.Substring(6), out int index) ? index : -1);
                if (exact != null) VoiceDirector.Active?.Play(exact, VoiceDirector.Story);
                return exact;
            }
            // the dream altar has a smaller bank: a rare or companion purchase falls back to its plain purchase lines
            if (VoiceLines.Bank(bank, trigger).Length == 0 && trigger.StartsWith("purchase")) trigger = "purchase";
            if (VoiceLines.Bank(bank, trigger).Length == 0) return null;
            return VoiceDirector.Say(bank, trigger, trigger == "purchase_rare" ? VoiceDirector.Important : priority, true);
        }

        private void Pin(bool special, int priority)
        {
            if (BubbleKey == null || string.IsNullOrEmpty(line)) return;
            SpeechBubbles.Show(BubbleKey, MerchantName ?? (special ? "MOTHER OF A THOUSAND EGG-EYES" : "THE SHOPKEEPER"), line, Anchor, special ? SpeechTone.Dream : SpeechTone.Shopkeeper, expires - began, priority + 1, BubbleSide.Right);
        }

        public void Observe(bool special, string station, float now)
        {
            if (focus == station) return;
            if (station == null) { focus = null; return; }
            if (now < nextInspect) return;
            focus = station;
            Speak(special, MerchantMoment.Inspect, now);
        }
        public void ResetFocus() { focus = null; }
        public void Say(string text, float now, float seconds)
        {
            line = text; began = now; expires = now + seconds; SpokenCount++; LastBark = null;
        }
        /// <summary>Legacy fixed-rect card (kept for diagnostics that draw a detached dialogue); the game pins lines as speech boxes.</summary>
        public void Draw(bool special, float now, bool reduced)
        {
            if (line == null || now >= expires) return;
            if (copy == null) { copy = ArcadeTypography.Style(19); copy.normal.textColor = ArcadeSkin.Ink; name = ArcadeTypography.Style(18, true); name.normal.textColor = ArcadeSkin.Ink; }
            float reveal = reduced ? 1 : Mathf.Clamp01((now - began) / 0.18f);
            float width = 370 * Mathf.SmoothStep(0.9f, 1, reveal);
            Rect bubble = new Rect(1180, 260 + (1 - reveal) * 8, width, Mathf.Max(126, copy.CalcHeight(new GUIContent(line), width - 36) + 61));
            Color previous = GUI.color; GUI.color = new Color(1, 1, 1, reveal);
            ArcadeSkin.Ticket(bubble, ArcadeSkin.Cream, special ? new Color(0.7f, 0.45f, 0.85f) : ArcadeSkin.Accent);
            string title = MerchantName ?? (special ? "MADAM DREAMSHELL" : "THE RECEIPT GULPER");
            name.fontSize = 18; name.wordWrap = false;
            name.fontSize = Mathf.Clamp(Mathf.FloorToInt(18 * (bubble.width - 36) / Mathf.Max(1, name.CalcSize(new GUIContent(title)).x)), 12, 18);
            GUI.Label(new Rect(bubble.x + 18, bubble.y + 12, bubble.width - 36, 27), title, name);
            int length = VisibleGlyphs(now, reduced);
            GUI.Label(new Rect(bubble.x + 18, bubble.y + 45, bubble.width - 36, bubble.height - 55), line.Substring(0, length), copy);
            GUI.color = previous;
        }
    }
}
