using System;
using System.Collections.Generic;

namespace ArcaneBrigade
{
    public static class CompanionBanter
    {
        private static readonly Dictionary<string, string[]> Lines = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { "TeapotPilgrim", new[] { "I brought a cup for whoever wakes up last.", "The tea is cold. Something below is still breathing.", "Keep the chipped cup. It knows your hands." } },
            { "CandleSnail", new[] { "A slow little flame still gets there.", "My wax is falling upward. Please keep walking.", "I saved one spark for the morning." } },
            { "MailboxImp", new[] { "No postage needed for a kind word.", "The empty letters have started answering.", "Address it to all of us. Same home." } },
            { "PillowOracle", new[] { "Rest your worry here. Not your whole head.", "Do not sleep yet. That dream is already occupied.", "We earned an ordinary pillow tonight." } },
            { "MushroomLibrarian", new[] { "This shelf smells like rain and good stories.", "The footnotes are growing teeth.", "Let us return the borrowed morning." } },
            { "HoodWisp", new[] { "My hood has room for one more secret.", "Something keeps tugging the hood from inside.", "You can keep the ribbon. I remember you." } },
            { "JellySwordsman", new[] { "A wobbly salute is still a salute.", "My reflection stopped wobbling. I did not.", "The brave part was coming back together." } },
            { "BrassAcorn", new[] { "A tiny crown fits a very big promise.", "I hear something counting the rings inside me.", "Plant the crown. Let it grow into a tree." } },
            { "BreadCrab", new[] { "Fresh crust. Firm claws. Excellent company.", "That smell is not bread anymore.", "Breakfast is better when everyone arrives." } },
            { "KettleBat", new[] { "I whistle when I am nervous. Just a little.", "The whistle came back before I made it.", "Open the window. I want real night air." } },
            { "BookmarkGolem", new[] { "I kept your place while you were fighting.", "This chapter has no last page.", "A folded corner is enough to find us." } },
            { "TinTick", new[] { "Tick. We still have time to be kind.", "There is another tick between my ticks.", "I would like a day without counting it." } },
            { "SpringDuck", new[] { "A small spring makes a very determined duck.", "Something wound me while we were looking away.", "One last hop. Then we go home." } },
            { "ButtonBeetle", new[] { "Hold still. Your courage needs a button.", "The coat on that chair is watching us.", "I kept the spare button. Just in case." } },
            { "StapleMole", new[] { "Loose pages? I can hold them together.", "The floor is softer than the paper above it.", "Do not staple the door shut. We need it." } },
            { "SnowSpoon", new[] { "A spoonful of snow keeps the stars quiet.", "The snow is warm underneath.", "I want to stir something that does not whisper." } },
            { "IceCreamOwl", new[] { "One scoop of wisdom. No biting the cone.", "My shadow is melting the wrong way.", "I saved the sweetest thought for daylight." } },
            { "FridgeWisp", new[] { "Cold hands. Warm intentions.", "Someone is knocking from behind my coldest shelf.", "Leave the door open. Nothing stays trapped." } },
            { "PearMoth", new[] { "A little light is worth circling.", "The lamps are drawing wings on the walls.", "Let the pear tree see the sun again." } },
            { "CitrusToad", new[] { "A quick hop and a sharper grin.", "The floor tastes like yesterday's thunder.", "One hop outside. Then we can slow down." } },
            { "RadishJester", new[] { "I promised the roots a very good joke.", "They laughed downstairs. I had not spoken yet.", "The last joke is that we made it out." } },
            { "SeedHedgehog", new[] { "Careful. These little seeds are for later.", "The seeds are turning toward the basement.", "We can plant every one. There is time." } },
            { "MoonJelly", new[] { "The moon fits nicely inside a quiet thought.", "Something underneath is pulling at my tide.", "I want to see the moon without glass." } },
            { "StarSock", new[] { "Left sock reporting. Right sock still exploring.", "There are too many footsteps for our feet.", "Keep the mismatched pair. They found each other." } },
            { "CometKite", new[] { "A little tailwind for a very small army.", "There is no ceiling. Why is my string taut?", "Cut the string only when we are outside." } },
            { "EclipseEgg", new[] { "No rushing. A sunrise takes its time.", "Something knocked on the shell from out there.", "Whatever hatches, we will meet it together." } },
            { "OrbitMoth", new[] { "You hold the light. I will mind the dark.", "The orbit is smaller than it was a moment ago.", "I can fly straight now. I choose to stay." } },
            { "DreamCompass", new[] { "The needle points toward good company.", "It keeps pointing at the hand holding it.", "Home was not a direction after all." } },
            { "ShellPostman", new[] { "Dry letters, even through the deepest tide.", "The stamp is wet. We have not seen water.", "A thank-you should always reach its door." } },
            { "CoralTeacup", new[] { "A little sea in a very polite cup.", "The broadcast is showing the room behind us.", "Switch it off. I want to hear the real sea." } },
            { "BubbleKoi", new[] { "I kept a small breath for you.", "The bubbles are falling. Do not follow them.", "A quiet pond. No glass. No price tag." } },
            { "AnchorSnail", new[] { "We can carry the anchor without dropping hope.", "Something is dragging the chain below the floor.", "Let us leave the anchor where the light reaches." } },
            { "SailSquid", new[] { "Small sail. Excellent weather for stubbornness.", "There is wind here, but no way it came in.", "I know a sea that does not need a ceiling." } },
            { "SockCyclops", new[] { "One eye is plenty to watch over friends.", "I blinked. The corridor did not.", "I would like a drawer that stays a drawer." } },
            { "DiceCrab", new[] { "A good roll leaves room for everyone.", "The seventh side has started showing up.", "Keep the crooked die. It brought us here." } },
            { "MaskPepper", new[] { "The mask is brave. I am learning from it.", "It smiled while my face was still.", "Outside, I might take it off." } },
            { "CrayonGremlin", new[] { "I drew a door. We will find the real one.", "The colours are moving after I lift the crayon.", "I will draw the morning from life this time." } },
            { "MirrorNewt", new[] { "Your reflection looks relieved to have company.", "Mine is a little late. Do not wait for it.", "Leave the mirror. We have the real faces." } },
            // content expansion: the new friends
            { "PaperPlanePigeon", new[] { "Every plane needs someone watching it land.", "One of my planes keeps circling. It will not come down.", "Let it land wherever it likes. I saw it fly." } },
            { "BobaTadpole", new[] { "Sip slowly. The pearls are the best part.", "The glass feels smaller every wave. Am I growing?", "I do not have to choose a shape today." } },
            { "BakerMouse", new[] { "Flour on your sleeve means you were welcome.", "The dough will not rise tonight. Something is cold.", "Let us share the crust. The middle can wait." } },
            { "WateringSnail", new[] { "Slow water still reaches the roots.", "The can feels lighter. The garden feels farther.", "Somebody looked down. That is enough for a bloom." } },
            { "RaincoatFrog", new[] { "Puddles are just small doors. Step lightly.", "My coat is soaked through on the inside.", "Come in out of the rain with me. Both of us." } },
            { "LampGoat", new[] { "Follow the light, not the goat.", "My horns feel heavy. The path keeps getting longer.", "Set the lanterns down. We can see the stars." } },
            { "YoyoSquirrel", new[] { "Watch. It always comes back.", "The string is fraying. What if it does not return?", "I let it go. It came back anyway." } },
            { "ShovelPenguin", new[] { "Clear path! Mind the slush.", "The snow keeps falling faster than I can lift it.", "Leave the shovel. Let us walk through together." } },
            { "TopBeetle", new[] { "Watch it wobble, then watch it stand.", "My spring is winding down. The window is empty.", "I can stand still and still be here." } },
            { "LensOwl", new[] { "Hold still. The light is almost on the page.", "The sun leaves faster. I keep losing my place.", "Read me the ending. I will listen." } },
            { "RainHeron", new[] { "Every drop gets a line in the book.", "My pages are wet. The words keep running.", "Let the rain write itself today." } },
            { "TeaDragon", new[] { "Mind the spout. It is hotter than it looks.", "The pot keeps whistling, but nobody comes to pour.", "Sit. The tea is for both of us." } },
            { "LighthouseWhale", new[] { "Stay in the light. I will keep it steady.", "The lamp is bright, but I am tired of the surface.", "Let the lamp sleep. I want to swim down." } },
            { "JackInTheBox", new[] { "Ladies and gentlemen! Eyes on me!", "The music plays. I jump. Who is turning the crank?", "I choose the show. Tonight, a quiet one." } },
            { "KiteDragon", new[] { "Every gust is a wish someone forgot to say.", "The string pulls tighter the higher I go.", "Let go of the string. I will still fly near." } },
            { "SundaeGolem", new[] { "Stand back. Cold hands, warm heart.", "I am melting faster than I can guard.", "Share the shade with me. Sticky hands are fine." } },
            { "LostCarousel", new[] { "Every horse remembers a rider.", "Round and round. The park is empty again.", "One last ride, then we take a real road." } },
            { "EndlessIndex", new[] { "Everything has a card. Even you.", "There is a drawer I cannot open. My own name is in it.", "File me under friend. That will do." } },
            { "FirstSeed", new[] { "Roots go down before anything goes up.", "The ground is dry. I am waiting to be planted.", "Plant me here. I do not need to be first." } },
            { "StaticFerret", new[] { "Do not touch my fur. Unless you want a spark.", "The crackle is louder. Nobody is on the line.", "Hello? Oh. It is you. That is good." } },
            { "StormKitten", new[] { "Do not worry, it is only a small storm.", "The cloud is getting darker. I cannot stop it.", "You stayed. The storm is smaller now." } },
            { "VoltEel", new[] { "Stay close. I glow for both of us.", "My stripes flicker. The tank is very dark.", "Let me rest my light. Yours is enough." } },
            { "ShantySeaLion", new[] { "Heave ho! Pull in time with me.", "The tide comes in, but no oars answer the song.", "Sing the next verse. I will listen." } },
            { "BottleGull", new[] { "Every bottle has someone waiting at the shore.", "The bottle addressed to me is getting heavier.", "Let us open it together. Whatever it says." } },
            { "PinnedBeetle", new[] { "Hold still. I know how to hold still.", "The pin hole aches when the wind blows.", "My label says friend now. In my handwriting." } },
            { "FossilArmadillo", new[] { "Curl up. Wait. Uncurl. Repeat.", "Something old is pressing on my shell.", "I uncurl because I want to, not because it is safe." } },
            { "SpecimenSlime", new[] { "Pour me anywhere. I will fit.", "I keep splitting. Which one is me?", "All of us are me. That is allowed." } },
            { "CourtHerald", new[] { "Hear ye! The brigade arrives!", "My trumpet echoes. The gate stays shut.", "No gate needed. Announce us to the road." } },
            { "RibbonSpider", new[] { "Eight little knots. Nobody gets left behind.", "The last thread is tied to something breathing.", "We can weave a hammock instead of a trap." } },
            { "CoilKettle", new[] { "A patient simmer makes a better bargain.", "The handle is turning without my help.", "No more boiling the same night twice." } },
            { "ThimbleMoth", new[] { "A thimble is a very small helmet for worry.", "I can hear a needle scratching inside the wall.", "We can mend things without being afraid." } },
            { "ReelOwl", new[] { "One more frame. Hold your best smile.", "The next frame is staring back at the projector.", "Keep this frame. We are all in it." } },
            { "RainbowTailor", new[] { "Nothing here is beyond a careful stitch.", "The cloth remembers a shape I never cut.", "I will sew a pocket for ordinary days." } },
            { "RainbowBell", new[] { "Little marbles, follow the gentlest ringing.", "The bell below answers without being struck.", "This last ring is an invitation, not an order." } },
            { "SecretPatchBear", new[] { "You came back for the thing under the shelf.", "I remember this darkness. I am not staying.", "I do not need a new patch to belong." } },
            { "SecretPaperBird", new[] { "The thank-you is still folded safely.", "The corridor keeps trying to read my wings.", "Please deliver me somewhere the sky is real." } },
            { "SecretDreamLamb", new[] { "A little nap can wait until everyone is safe.", "I dreamed of this room before it had a door.", "Wake me when we can all rest." } },
            { "SaffronMechanist", new[] { "Give me a screw and half a hopeful minute.", "The spare parts are arranging themselves.", "For once, let us build something without a lock." } },
            { "ClockworkPiper", new[] { "Two little helpers. One very cheerful tune.", "There is a second tune inside the first.", "The morning does not need winding." } },
            { "VelvetChampion", new[] { "Every friend counts. Especially the quiet one.", "The ledger has a blank line shaped like me.", "We can put the titles down at the door." } },
            { "PaperInventor", new[] { "I made a useful thing. Probably facing forward.", "The plans are drawing a hand I did not design.", "Next invention: a day with no emergency." } },
            { "ReturnCaptain", new[] { "A good entrance deserves a little patience.", "I know this place. It does not know I came back.", "This time, returning means going home." } },
            { "DoughForeman", new[] { "Warm hands. Steady work. Save everyone a piece.", "The dough is rising against the ceiling.", "No overtime. We finish together." } },
            { "IronLocomotive", new[] { "All aboard. Nobody rides alone.", "The track keeps bending back beneath the floor.", "Next stop has windows that open." } },
            { "VioletRemnant", new[] { "I stayed because somebody remembered.", "The dark keeps offering me my old name.", "You can remember me without keeping me here." } },
            { "VelvetEnchantress", new[] { "Come closer to the light. Not to the teeth.", "That curtain is trying to breathe with us.", "I would like a promise without a spell." } },
            { "GrandSprout", new[] { "Little leaves can shelter very big hearts.", "My roots will not touch what waits below.", "We can be tall without blocking the sun." } },
            { "BristleBall", new[] { "I am practising being a comforting shape.", "My spines are pointing at something underground.", "A soft bed can have a careful corner for me." } },
            { "PaperDevil", new[] { "Being kind is a wonderfully suspicious habit.", "The shadows want me back. I said no.", "Do not look so surprised. I kept my promise." } },
            { "BalloonMallet", new[] { "A gentle bonk for a terrible idea.", "Something breathed into me from the wrong side.", "Let a little air out. We can relax now." } },
            { "LittleSprout", new[] { "Do you think I will be tall tomorrow?", "The roots below are calling me by your name.", "Will there be a little window where we live?" } },
            { "MonsterMarble", new[] { "Round friends roll back when you need them.", "There is a corner inside me. I can feel it.", "I will try very hard not to roll away." } },
            { "CarouselPony", new[] { "Climb on. I know these stairs.", "The music stopped. Do not start the circle again.", "No more circles. Let us see a real road." } },
            { "TeacupCricket", new[] { "I tuned the cup. It sings when we agree.", "The quiet between notes is getting longer.", "We can play softly. Nothing is chasing us." } },
            { "RibbonOtter", new[] { "I tied the rain into a little bow.", "The ribbon is wet with a rain that falls inside.", "Untie it outside. Let the river have it back." } },
            { "SpindleWren", new[] { "A thread through the dark is still a path.", "Something at the other end has begun pulling.", "The spool can stop. The promise stays." } },
        };
        public static int SpeciesCount => Lines.Count;
        public static bool Has(string id) => id != null && Lines.ContainsKey(id);
        public static string Line(string id, int index, int wave = 1, RunSnapshot state = null)
        {
            if (!Has(id)) return null;
            string story = CreatureLives.Banter(id, state);
            if (story != null) return story;
            string[] lines = Lines[id];
            int variation = (index % 3 + 3) % 3;
            return lines[wave >= 20 ? 2 : wave >= 11 && variation != 2 ? 1 : variation == 1 ? 0 : variation];
        }
        public static IEnumerable<string> AllLines => Enumerate();
        private static IEnumerable<string> Enumerate() { foreach (string[] lines in Lines.Values) foreach (string line in lines) yield return line; }
    }
}
