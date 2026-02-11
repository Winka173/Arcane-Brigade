using System;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class OctoberDialogue : MonoBehaviour
    {
        private ArenaController owner;
        private float next, until;
        private string speaker, line;
        private int cursor, lastWave;
        public void Configure(ArenaController controller) => owner = controller;
        public static string HeroLine(string reference, int index)
        {
            string[] lines = reference switch
            {
                "Mr. Work" => new[] { "One more lamp. One more friend home.", "My shift ends when everyone is safe." },
                "Derpy Wolf" => new[] { "I delivered tomorrow's letter yesterday.", "Slow down? The clock started it!" },
                "Jade Cabbage" => new[] { "Little leaves, stay where I can see you.", "Even the smallest root can break a nightmare." },
                "The Big Pot" => new[] { "Come close. There is still warmth in me.", "A dent is just another place to keep a story." },
                "Porcelain Doll" => new[] { "Careful. My cracks remember everything.", "We can mend this night, one seam at a time." },
                "Slippy" => new[] { "No running on the wet floor. That is my job.", "If it bites, give it a little nudge." },
                "Card King" => new[] { "A good king leaves room for his friends.", "The crown is heavy. Your company helps." },
                "Sassy Senior" => new[] { "I was old when this shop was young.", "Luck is experience wearing a ridiculous hat." },
                "Baggy" => new[] { "Leave the walking to the impatient.", "The roots downstairs are listening." },
                "Sir Purrcival" => new[] { "For the little ones behind the counter!", "My helmet is brave. I am catching up." },
                "Red Scale" => new[] { "Another cup, another tide.", "I can hear the ocean inside these coins." },
                "Omnom" => new[] { "The dark is hungry. So am I.", "Leave a little nightmare for me." },
                "Periwinky" => new[] { "I counted my quills. The shop counted twice.", "A full purse makes a surprisingly good pillow." },
                "Clementie" => new[] { "Stay close. We move as one.", "Little feet can make a thunderstorm." },
                "Gulpy" => new[] { "Every locked chest dreams of opening.", "I promise not to eat the receipt this time." },
                "Frosty" => new[] { "Hush. I am keeping a snowflake alive.", "Give me a moment. Even winter needs to breathe." },
                "Capped Gallant" => new[] { "Many voices. One very small hat.", "I carry my friends where the dark cannot reach." },
                "Card Queen" => new[] { "Twelve heartbeats. Spend them kindly.", "My court will remember who stood with us." },
                "Steel Shell" => new[] { "Let the dark strike first.", "A quiet shield hears the smallest cry." },
                "Box Shell" => new[] { "Special delivery. Signature optional.", "There is a whole sunrise in this box." },
                "Bloom Shell" => new[] { "A flower opened while we were fighting.", "There is room for a garden on every shell." },
                "Belle" => new[] { "Everyone gets a place in this story.", "The tickets are patched. The show goes on." },
                "Wise" => new[] { "A borrowed voice is still worth hearing.", "I keep the names the shop forgot." },
                "Nicole" => new[] { "That miracle had better come with change.", "One eye for bargains, one for the way home." },
                "Anby" => new[] { "I can hear the storm inside the wires.", "Wait for the quiet between two sparks." },
                "Billy" => new[] { "Wind me up. I know the next step.", "A little applause for the whole brigade!" },
                "Nekomata" => new[] { "The moon left pawprints on the stairs.", "If you hear a bell, try to keep up." },
                "Burgerboo" => new[] { "Dinner is ready. Stand well back.", "No nightmare leaves my kitchen hungry." },
                "Original:LanternWeaver" => new[] { "The light holds where the thread is tied.", "I stitched a path through this darkness." },
                "Original:HourglassStag" => new[] { "A grain of time for every friend.", "We cannot stop the night. We can soften its steps." },
                _ => new[] { "I wrote the rain down before it forgot us.", "Every raindrop knows a way home." }
            };
            return lines[Math.Abs(index % lines.Length)];
        }
        public static string AllyLine(UnitDefinition u, int index, int wave = 1)
        {
            string personal = CompanionBanter.Line(u.Id, index, wave);
            if (personal != null) return personal;
            if (u.ReferenceCompanion == "Pony") return index % 2 == 0 ? "Climb on. I know these stairs." : "Two friends travel better than one.";
            if (u.ReferenceCompanion == "Little Cabbage") return index % 2 == 0 ? "Do you think I will be tall tomorrow?" : "I found a root shaped like your hand.";
            if (u.ReferenceCompanion == "Domestic Spiked Ball") return "I am trying very hard to be a comforting shape.";
            if (u.ReferenceCompanion == "\"Triumphant Return\" Daniel") return "I was not late. I was waiting for the right entrance.";
            if (u.ReferenceCompanion == "\"The Creator\" Roger") return "I made something useful. Please do not shake it.";
            if (u.ReferenceCompanion == "Evil") return "I could be worse. I am choosing not to be.";
            string[] lines = u.Material switch
            {
                BodyMaterial.Fabric => new[] { "That was close. Check your stitches.", "Your sleeve is warm. May I stay here?", "I saved a soft corner for you." },
                BodyMaterial.Metal => new[] { "Still ticking. Still together.", "I polished a little star for the journey.", "A loose screw is no reason to leave a friend." },
                BodyMaterial.Paper => new[] { "Please keep the rain off my name.", "Fold the map. Remember the way home.", "I wrote our names on the same ticket." },
                BodyMaterial.Glass => new[] { "The light looks different through a crack.", "Hold still. There is a sunrise in your reflection.", "I remember when this window showed the sky." },
                _ => new[] { "I heard the floor whisper. I pretended not to.", "One more door. We go together.", "If you get lost, listen for my footsteps." }
            };
            return lines[Math.Abs(index % lines.Length)];
        }
        private void Update()
        {
            if (owner?.Session == null || owner.PresentationPaused) return;
            if (lastWave != owner.Session.State.Wave) { lastWave = owner.Session.State.Wave; next = owner.PresentationClock + 8; }
            if (owner.Session.State.Phase != SessionPhase.Shop || owner.HallExitPending || owner.Session.SecretRoomOpen || owner.TransitionPending || owner.Session.State.OutsideStage == OutsideStageStatus.Reward) { until = 0; return; }
            if (owner.PresentationClock < next || owner.Shop?.Dialogue.ExpiresAt > owner.PresentationClock) return;
            // banter waits politely for anyone who has just joined or ranked up, and for the journal
            if (owner.StoryOpen || Time.unscaledTime - SpeechBubbles.LastCompanionAt < 4f) { next = owner.PresentationClock + 2; return; }
            UnitDefinition u = owner.Content.Unit(owner.Session.State.ChampionId);
            UnitState body = owner.Simulation?.Champion;
            bool hero = true;
            if (cursor % 2 != 0 && owner.Session.State.Roster.Count > 0)
            {
                RosterEntry entry = owner.Session.State.Roster[(cursor / 2) % owner.Session.State.Roster.Count];
                u = owner.Content.Unit(entry.UnitId); body = FindCompanion(entry.UnitId); hero = false;
            }
            speaker = u.DisplayName; line = string.IsNullOrEmpty(u.ReferenceHero) ? AllyLine(u, cursor / 2, owner.Session.State.Wave) : HeroLine(u.ReferenceHero, cursor / 2);
            cursor++; until = owner.PresentationClock + 5; next = owner.PresentationClock + 16;
            if (body == null || string.IsNullOrEmpty(line)) return;
            UnitState held = body;
            SpeechBubbles.Show(hero ? "Hero" : "Companion:" + u.Id, hero ? speaker : RunSession.Stranger(owner.Session.State, u.Id) ? "???" : speaker, line,
                () => held.Alive && owner.Session.State.Phase == SessionPhase.Shop ? SpeechBubbles.Head(held) : (Vector3?)null,
                hero ? SpeechTone.Hero : SpeechTone.Companion, 5f, 0);
        }
        private UnitState FindCompanion(string id)
        {
            if (owner.Simulation == null) return null;
            foreach (UnitState unit in owner.Simulation.Units) if (unit.Faction == Faction.Companion && unit.Alive && unit.Definition.Id == id) return unit;
            return null;
        }
        /// <summary>The line most recently said in the shop (the box itself is pinned above the speaker by SpeechBubbles).</summary>
        public string CurrentLine => owner != null && owner.PresentationClock < until ? line : null;
    }
}
