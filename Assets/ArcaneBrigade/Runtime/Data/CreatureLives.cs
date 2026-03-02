using System.Collections.Generic;

namespace ArcaneBrigade
{
    public static class CreatureLives
    {
        private static readonly Dictionary<string, string> Origins = new Dictionary<string, string>
        {
            {"TeapotPilgrim", "Born above the station kitchens, they fed travellers through a winter with no departures. Every cup was counted; nobody knew whether they had eaten. They want a table where the last person serving is also given a seat."},
            {"CandleSnail", "They carried their village's ember through a flood. Their shell holds the names of neighbours who took turns carrying them afterward. They seek dry ground for those neighbours, not another chance to endure everything alone."},
            {"MailboxImp", "A collector of letters with missing addresses, they once mistook silence for permission to read them. Delivering an apology unopened began their first honest route. They want a postal service that also lets a recipient refuse a letter."},
            {"PillowOracle", "They hear dreams through a boarding house's walls. They predicted visitors' needs until nobody remembered to ask what they wanted. They seek an ordinary night of sleep with someone else keeping watch."},
            {"MushroomLibrarian", "Their underground library grows books from witnessed events, keeping contradictory accounts on adjacent shelves. A borrower who never returned left a gap they refuse to fill with a convenient ending. They seek the borrower rather than a replacement story."},
            {"HoodWisp", "A guard bound to the promise that nobody would leave alone, they trapped solitary travellers. Someone returned voluntarily for them. The contract erased that night, but their body still moves to cover a companion's unguarded left side."},
            {"JellySwordsman", "A river-dweller taught fencing by a ferry captain, they were exiled for refusing a duel against a frightened passenger. They want to teach a form whose last lesson is lowering the sword. Their salute belongs to no recognised school."},
            {"BrassAcorn", "An elected keeper of a root settlement, they inherited a crown meant to be buried when their term ended. The frozen season never let that day arrive. They seek a spring that will let somebody else lead."},
            {"BreadCrab", "Their coastal family measures tides by rising dough. A storm took their street but left its breakfast bell. They carry the bell toward the coast, hoping a familiar sound will gather the scattered neighbours."},
            {"KettleBat", "Raised beneath the market, they whistle to navigate steam vents. An answering voice once brought their family into danger. They learned that an answer is not necessarily an invitation, and now check every route before guiding anyone."},
            {"BookmarkGolem", "They arose from a community's promise to keep a missing reader's place, then learned to read the unfinished book themselves. They seek that reader while building a life that does not end at the last reserved page."},
            {"TinTick", "A timekeeper whose heart stayed punctual after the trains stopped, they falsified bells to give stranded children hope. Admitting the lie cost their post. They want to ring for an arrival they have actually witnessed."},
            {"SpringDuck", "A wetland courier injured in the machinery district, they chose a spring prosthesis made by a friend. The spring helps their life continue; it did not create it. They want to return the friend's tools and report that the crossing worked."},
            {"ButtonBeetle", "Their family mends coats left on station hooks. They recognise travellers by repairs nobody else notices. They kept one uncollected coat because its missing owner once covered a sleeping stranger with it."},
            {"StapleMole", "A tunneller who bound scattered maps into a public atlas. Officials sealed dangerous routes by sealing their residents inside, so they stole the atlas back. They mark blocked exits in ink readable from either side."},
            {"SnowSpoon", "A frost-country pilgrim carrying the last spoon from a communal pot. They rationed too strictly and remember the neighbour who went hungry. They seek seeds for cold soil rather than another rule for enduring hunger."},
            {"IceCreamOwl", "An astronomer who tracked a star the realm's ceiling kept erasing. They left a student in charge and worry that leaving made them disloyal. They want to bring back a map drawn under a real sky."},
            {"FridgeWisp", "A keeper of cold stores who sheltered passengers during a heat wave. One store stayed cold after its key vanished. They seek every missing key, including those opening doors they were ordered to leave alone."},
            {"PearMoth", "Raised in an orchard lit by artificial suns, they followed moonlight through a broken roof. They return each season to describe the outside. Their family has begun cutting windows, and they want to see the first one open."},
            {"CitrusToad", "A marsh runner carrying news nobody wanted to hear. Jokes gave listeners somewhere to put their anger. They travel with people who let them finish a sentence without requiring it to be funny."},
            {"RadishJester", "A performer who hid criticism inside jokes until the court learned to applaud without listening. They left mid-punchline. They want to speak one plain truth and be heard without laughter covering it."},
            {"SeedHedgehog", "They preserve seeds from a garden erased from official maps. They will not replace a lost species with a prettier account of what grew there. They seek soil where the surviving seeds can belong without a ledger's permission."},
            {"MoonJelly", "A tide-born resident who remembers the moon before the sea acquired a ceiling. Their migration follows a pull no map records. They want to discover whether the older sea survives and whether their young can reach it."},
            {"StarSock", "Separated from a sibling at a crossing, they wear mismatched cloth recording the strangers who helped them continue. They seek their sibling without pretending every newer connection is temporary."},
            {"CometKite", "A sky fisher whose safety line was tied to the wrong shore. A keeper called the tightening tether protection. They carry their own spool now and search for the crew on the other side."},
            {"EclipseEgg", "An unhatched sky-dweller who speaks through their shell, postponing hatching until their flock is found. The shell is their body, not a container to open for a reward. They decide when the next part of their life begins."},
            {"OrbitMoth", "A lamplighter who circled a homeward beacon until discovering its keeper turned it to prevent arrivals. They now guide by several independent lights. They want routes no single keeper can own."},
            {"DreamCompass", "A navigator transformed by crossing incompatible dreams. Their needle answers freely made commitments, not an owner's commands. They are learning to choose a destination after a lifetime of being asked only for directions."},
            {"ShellPostman", "A coastal courier carrying unsent replies as carefully as delivered letters. One envelope bears the protagonist's childhood handwriting; its reply was intercepted. They want the addressee to read it before being told what it means."},
            {"CoralTeacup", "A reef resident whose ceramic-looking shell shelters a living reef. They broadcast weather to inland families. Their last warning brought an evacuation boat too late; they still check the harbour's arriving names."},
            {"BubbleKoi", "They kept air pockets along a drowned road and rescued people who later forgot it existed. They collect those accounts to stop another town building over it. They want witnesses rather than a grateful statue."},
            {"AnchorSnail", "A dockworker who carried a vessel's anchor inland when its harbour disappeared. The chain proves the harbour existed. They seek the crew to ask whether it is time to put the weight down."},
            {"SailSquid", "An estuary pilot whose captain demanded guarantees no uncertain tide could give. They left that ship. They offer passage with risks spoken aloud and seek a crew willing to hear an honest forecast."},
            {"SockCyclops", "A one-eyed watchkeeper dismissed for reporting footsteps behind the official patrol. Following them led to a missing resident. They want a watch that treats listening as part of seeing."},
            {"DiceCrab", "A gambling-hall accountant who found the house erasing winners from its records. They kept a crooked die as evidence rather than letting it decide every choice. They seek the families of the removed names."},
            {"MaskPepper", "A theatre refugee whose mask preserved a role after the troupe disbanded. The role taught courage but sometimes speaks over their wishes. They are learning to remove it without apologising for the quieter voice beneath."},
            {"CrayonGremlin", "A map maker who drew windows for a district with none. One drawing opened a real passage. They now check every imagined exit before sharing it, and want to draw somewhere from life rather than longing."},
            {"MirrorNewt", "A witness from marshes where reflections remember events bodies forget. They disagree with their reflection about a former friend. They travel to compare records, not force one witness to disappear."},
            {"RibbonSpider", "A bridge weaver who tightened a safety web until nobody could move. They cut it and helped rebuild the road. Each new knot has a release reachable by the person it supports."},
            {"CoilKettle", "A bathhouse engineer who kept pumps running after everyone left, afraid stopping meant nobody would return. They carry a replacement part and permission to build something elsewhere. They seek a place to use both."},
            {"ThimbleMoth", "An apprentice surgeon whose tools also repaired coats. Their teacher called every error a moral failure. They seek colleagues who correct mistakes without demanding a confession of worthlessness."},
            {"ReelOwl", "A documentary keeper who found the official film had removed the people opening an exit. They smuggled out the discarded frames. They want to show the complete sequence in the square where the edited version played."},
            {"RainbowTailor", "They repair bodies only at their wearers' request. Their former workshop sold alterations as obedience. They preserve its patterns as evidence while making clothes with pockets people choose themselves."},
            {"RainbowBell", "A caller whose voice gathers residents from the marble valleys. They once rang people back into a locked refuge. Every call now ends with silence long enough for someone to choose not to answer."},
            {"SecretPatchBear", "Hidden in the lost-property vault after refusing an ownership mark, their seams are injuries and chosen repairs. They were not manufactured for somebody. They want their name spoken at the table without a claim ticket."},
            {"SecretPaperBird", "A courier from the folded-paper estuary, detained for taking a letter past the gate. The message is not their entire identity. After delivery, they intend to keep travelling for reasons of their own."},
            {"SecretDreamLamb", "A garden resident visiting under a two-victory travel agreement, with a flock to return to. They tell you that before joining. Leaving when the visit ends is a kept promise, not abandonment or a debt you failed to pay."},
            {"SaffronMechanist", "A railway repairer who made locks until learning where officials installed them. They kept a duplicate master key. They want to teach repairs that give residents control of their own doors."},
            {"ClockworkPiper", "A musician from the gear district with two apprentices who argue about tempo. Their mechanical body is ordinary where they were born. They seek the missing final verse of a song officials insist always ended early."},
            {"VelvetChampion", "A court champion offered belonging only while winning. They abandoned a ceremonial match to escort a rival home. They want to discover who they can be when no title introduces them."},
            {"PaperInventor", "A drafter whose city builds from folded plans. Their first shelter trapped residents when the plan refused revisions. They now test every design with someone allowed to say it does not fit."},
            {"ReturnCaptain", "A ferry captain whose route was changed into a circle without telling passengers. They broke the tiller rather than announce another false arrival. They carry a replacement toward a port where returning is a choice."},
            {"DoughForeman", "A community baker who led a strike when ovens fed the station while workers went hungry. The recipes preserve every worker's name. They seek a kitchen whose first meal belongs to its cooks."},
            {"IronLocomotive", "A rail-born carrier remembering every passenger but denied a choice of destination. They left the assigned line after its last station ceased to exist. They want to visit somewhere simply because it interests them."},
            {"VioletRemnant", "A survivor carrying an afterimage of a district erased from the maps. They are more than evidence of a disaster. They seek other survivors to discover what each wants next."},
            {"VelvetEnchantress", "A negotiator who soothed people until they forgot their objections, then testified against the court that hired them. They now ask again after the enchantment fades. They seek agreement that survives its disappearance."},
            {"GrandSprout", "An elder from travelling gardens who shelters young plants without choosing where they must settle. They move their own roots when home becomes unsafe. They seek a garden with more than one open gate."},
            {"BristleBall", "A burrower whose spines protected a community through a siege. They are learning to let friends close without remaining on guard. They want one full night's sleep while somebody else takes watch."},
            {"PaperDevil", "A former tracker who guided collectors to people hiding from contracts. They deserted on finding the same name on rescue and debt lists. They open routes they once betrayed without demanding that survivors absolve them."},
            {"BalloonMallet", "A traveller whose buoyant body kept them working above flooded streets. A foreman charged rent for their air. They joined a rescue crew and carry a valve only they may open."},
            {"LittleSprout", "A young garden-dweller travelling with their root household's consent. They are curious about rain and what they might become. Dream growth does not make their life an object for an owner's use."},
            {"MonsterMarble", "A valley resident whose round body was mistaken for cargo. They memorised voices until another passenger answered them. They want a station sign that begins: ask the traveller their name."},
            {"CarouselPony", "A living mount bound to a circular route by a promise made on their behalf. They remember riders but no destinations. They choose passengers now and seek the first road that does not return by force."},
            {"TeacupCricket", "A reed-settlement musician who played the waiting melody until someone taught them its missing final notes. They want to compose a song that lets the audience leave before it ends."},
            {"RibbonOtter", "A river mender marking safe crossings with ribbons. A flood moved a marker; they returned to correct it rather than blame those who trusted it. They seek the source and a place to remember the missing by name."},
            {"SpindleWren", "A guide carrying thread between settlements. They tied it to their wrist until every departure pulled them apart. They now teach travellers to hold and release their own ends, and seek a route that survives an honest goodbye."}
        };
        private static readonly Dictionary<string, string[]> Arcs = new Dictionary<string, string[]>
        {
            {"TeapotPilgrim", new[] {
                "Hob is the shop's hearth spirit. It has poured tea for every traveller who ever woke at that counter and has never once sat down. Asked what it needs, Hob lists chores. 'Need? I'm the one who asks that.'",
                "When the first lantern broke, Hob dropped a cup. It had never dropped anything. Since then it watches the warm chair in the corner the way you watch a pan about to boil over, and refills your cup before it is empty.",
                "In the perfect room Hob sat down in a chair of its own, because the chair promised it could finally rest. Everyone pulled it out together. Now Hob chooses a stool by the stove, because it wants to, and tells you to stop asking for a minute."}},
            {"HoodWisp", new[] {
                "Latch was one of the Keeper's guards. Last time round, it stopped you at the gate; it did what the rewritten orders said. After the Keeper knelt, Latch came with you and took the place on your left without a word.",
                "The whisper that tells you to hide always comes from the left. Latch has always stood there. It remembers earlier journeys in pieces: which hand shook, which door it held, how far you got before it was told to close the way.",
                "Latch does not ask to be forgiven for the last time. It stays on your left by agreement now, not by orders, and lets you decide which doors to open. 'You do not need my permission. You never did.'"}},
            {"ShellPostman", new[] {
                "Postscript is a courier folded out of an envelope. It walks the last platform leaving letters on the laps of travellers who sat down, and none of them ever opens one. When it saw that you were still standing, it followed you.",
                "Couriers may open any letterbox. Postscript pulled the nails out of yours with its paper hands and found the reply that was sewn shut: Thursday, the front door, you do not need to bring anything. It held the letter while you read.",
                "'It's addressed to you. It always was.' Postscript has an empty envelope of its own now, for somebody it wants to write to. Its route no longer ends at your story. Ask it who, some time."}},
            {"PaperDevil", new[] {
                "Tally hunted runaways for the Last Customer. Every one it brought back is a neat carved mark on its body, in groups of five. Last time round, it found you and let you go. That is why the Customer is hunting it now.",
                "Tally reads receipts when it thinks nobody is watching. The Customer has offered to give it its own name back, if it brings you to sit down. For one night it was gone, and you did not know where.",
                "At the Customer's counter, Tally laid out every name it ever brought back and asked for them to be returned. Nobody promised to forgive it. It came back to the brigade without a word and took its place."}},
            {"CarouselPony", new[] {
                "They circle the resting group despite room to go straight. A collar counts completed rides. They ask your destination before offering their back. You wait for the offer.",
                "The carousel tune plays. They stop, choose a rider and cross the old circle diagonally. Nothing breaks except the assumption that whoever holds a ticket owns the route.",
                "At the exit they ask the rider to walk beside them. The rider climbs down. 'I want to choose this road.' Later they offer a ride again. This offer is allowed to end."}},
            {"SecretPatchBear", new[] {
                "They ask whether you need the claim ticket. You ask which name belongs on their cup instead. They keep the ticket for now: it is the only proof they were allowed to hold.",
                "A seam opens during rest. They choose who may mend it and which patch stays. The repair is neither a replacement identity nor a condition for remaining in the party.",
                "They leave the ticket on the counter. 'I would like to keep my name.' Nobody asks for proof. At the new table they offer a chair without becoming responsible for whether the next person stays."}}
        };
        public static int SpeciesCount => Origins.Count;
        public static int ArcCount => Arcs.Count;
        public static bool Has(string id) => id != null && Origins.ContainsKey(id);
        public static string Origin(string id) => Has(id) ? Origins[id] : "";
        // the travelling four reveal more of their story as the road reaches their beats (Docs/Mien_Cho_Act_One_Story.md)
        private static readonly Dictionary<string, int[]> ArcWaves = new Dictionary<string, int[]>
        {
            {"TeapotPilgrim", new[] { 1, 3, 16 }}, {"HoodWisp", new[] { 5, 14, 17 }}, {"ShellPostman", new[] { 6, 12, 15 }}, {"PaperDevil", new[] { 11, 17, 19 }},
        };
        public static string Arc(string id, int wave)
        {
            if (!Arcs.TryGetValue(id, out var pages)) return "They chose this passage with the brigade. Their life continues beyond this journey; their origin is recorded above.";
            int count = ArcWaves.TryGetValue(id, out int[] at) ? (wave >= at[2] ? 3 : wave >= at[1] ? 2 : 1) : wave >= 17 ? 3 : wave >= 9 ? 2 : 1;
            string result = "";
            for (int i = 0; i < count; i++) result += (i > 0 ? "\n\n" : "") + pages[i];
            return result;
        }
        public static string Arc(string id,RunSnapshot state)
        {
            if (!ArcWaves.TryGetValue(id,out var at)) return Arc(id,state?.Wave ?? 1);
            if (state==null || !ActOneKnowledge.Known(state,at[0])) return "They travel beside you. Their account unfolds in the journal as you discover the road together.";
            int count=1;
            if (ActOneKnowledge.Known(state,at[1])) count=2;
            bool last=ActOneKnowledge.Known(state,id=="TeapotPilgrim"?17:at[2]);
            if (id=="TeapotPilgrim") last &= ActOneKnowledge.HobRescued(state);
            if (last) count=3;
            string result="";
            for (int i=0;i<count;i++)
            {
                string page=Arcs[id][i];
                if (id=="PaperDevil" && i==1) page=ActOneKnowledge.Known(state,18)
                    ? "Tally's place is empty. A folded receipt remains where it had been reading. Nobody yet knows where it has gone."
                    : "Tally reads a receipt when it thinks nobody is watching. It folds the paper small and puts it away. You do not yet know what it says.";
                result+=(i>0?"\n\n":"")+page;
            }
            if (id=="TeapotPilgrim" && ActOneKnowledge.HobRescued(state) && !last) result+="\n\nHob is on its feet again. Everyone helped. Nobody asks for repayment.";
            return result;
        }
        public static string Banter(string id,RunSnapshot state) => id switch
        {
            "TeapotPilgrim" => ActOneKnowledge.HobRescued(state) && ActOneKnowledge.Known(state,17) ? "Yes. For you to stop asking for a minute." : ActOneKnowledge.Known(state,3) ? "I dropped a cup. I have never dropped a cup." : "Need? I'm the one who asks that.",
            "HoodWisp" => ActOneKnowledge.Known(state,17) ? "Your left, if you want me there. The doors are yours." : ActOneKnowledge.Known(state,14) ? "The whisper comes from your left. So do I." : "I'll walk on this side, if you don't mind.",
            "ShellPostman" => ActOneKnowledge.Known(state,15) ? "I have a letter of my own to write now." : ActOneKnowledge.Known(state,12) ? "It's addressed to you. It always was." : "A reply. Any reply.",
            "PaperDevil" => ActOneKnowledge.Known(state,19) ? "I asked for the names back. Nobody owes me anything for it." : ActOneKnowledge.Known(state,17) ? "Don't ask what's on the receipt." : ActOneKnowledge.Known(state,11) ? "Each mark is somebody I brought back." : "I'll keep watch from here.",
            _ => Banter(id,state?.Wave ?? 1)
        };
        public static string Banter(string id, int wave) => id switch
        {
            "TeapotPilgrim" => wave >= 17 ? "Yes. For you to stop asking for a minute." : wave >= 3 ? "I dropped a cup. I have never dropped a cup." : "Need? I'm the one who asks that.",
            "HoodWisp" => wave >= 17 ? "Your left, if you want me there. The doors are yours." : wave >= 10 ? "The whisper comes from your left. So do I." : "I stopped you, last time. I will not stop you again.",
            "ShellPostman" => wave >= 15 ? "I have a letter of my own to write now." : wave >= 12 ? "It's addressed to you. It always was." : "A reply. Any reply.",
            "PaperDevil" => wave >= 19 ? "I gave the names back. Nobody owes me anything for it." : wave >= 17 ? "Don't ask what's on the receipt." : wave >= 12 ? "Last time, I let you go. He has not forgiven me for it." : "Each mark is somebody I brought back.",
            "CarouselPony" => wave >= 17 ? "Walk beside me a little. I want to choose the road." : wave >= 9 ? "The tune can play. I do not have to circle." : "Ask where I want to go before asking for a ride.",
            "SecretPatchBear" => wave >= 17 ? "Keep the claim ticket. I am keeping my name." : wave >= 9 ? "That patch can stay. Please ask before mending it." : "Do you need a receipt for me to have a seat?",
            _ => null
        };
    }
}
