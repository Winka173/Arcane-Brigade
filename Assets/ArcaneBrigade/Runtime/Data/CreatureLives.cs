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
                "The pilgrim pours for everyone and hides their own empty cup. Asked what they need, they list chores. You leave a place at the table without giving it a task. They stand beside it a long time before sitting.",
                "They miss a serving bell while resting. You return with two cups. 'I have not earned that one,' they say. Nobody takes it away. Outside, the room repeats the bell. They let somebody else answer.",
                "At the last rest, they ask for tea before offering yours. A companion brings it. 'I still like caring for people. I want to be here when I am tired too.' The reserved chair is no longer empty."}},
            {"HoodWisp", new[] {
                "The seal inside their hood belongs to an old gate officer. They always cover your left, even when the road is clearer on the right. They do not remember learning that habit.",
                "The register names them as the guard who stopped your earlier escape. Beside it, a correction in their hand: OPENED FROM INSIDE. They chose to help before the contract erased the night. Anger does not prevent you hearing the whole account.",
                "At the final gate, they step aside before offering cover. 'You do not need my permission.' They remain on your left by agreement. What survived the erased memory was a choice they can make again."}},
            {"ShellPostman", new[] {
                "The courier's envelope is worn soft by years. They will not read the addressee's letter for them. When you recognise the handwriting, they put it in your hands and wait.",
                "The reply names Thursday; the register confirms a visitor. The courier does not say this fixes everything. 'Would you like me nearby while you read it again?' The question leaves room for no.",
                "They return the receipt and keep an empty envelope for their own message. 'I have somebody to write to as well.' Their route no longer ends at your story. You ask about that person."}},
            {"PaperDevil", new[] {
                "They know which boards conceal alarm wires. They first claim to have learned while escaping. An old collection warrant suggests they came from the other direction. They stop making the convenient claim.",
                "They admit leading collectors to a shelter. A survivor refuses the apology. The tracker gives them the route ledger anyway and accepts the distance. Repair cannot depend on being welcomed back.",
                "At the crossing, they destroy marks they were paid to preserve. Nobody promises forgiveness. 'I can do this because it is right. The answer afterward belongs to them.' They keep the names, not the warrants."}},
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
        public static string Arc(string id, int wave)
        {
            if (!Arcs.TryGetValue(id, out var pages)) return "They chose this passage with the brigade. Their life continues beyond this journey; their origin is recorded above.";
            int count = wave >= 17 ? 3 : wave >= 9 ? 2 : 1;
            string result = "";
            for (int i = 0; i < count; i++) result += (i > 0 ? "\n\n" : "") + pages[i];
            return result;
        }
        public static string Banter(string id, int wave) => id switch
        {
            "TeapotPilgrim" => wave >= 17 ? "I would like my tea first today. Will you sit with me?" : wave >= 9 ? "You brought me a cup. I had not done anything for it." : "I poured for everyone. Is that chair really for me?",
            "HoodWisp" => wave >= 17 ? "Your left, if you want me there. The door is your choice." : wave >= 9 ? "I guarded this door. Then I opened it. Both were me." : "Why do I always know to cover your left?",
            "ShellPostman" => wave >= 17 ? "My next letter will be my own. Thank you for asking." : wave >= 9 ? "I can wait nearby while you read. Or leave you space." : "This envelope knows your handwriting. You should open it.",
            "PaperDevil" => wave >= 17 ? "I will break the tracking marks. Nobody owes me forgiveness." : wave >= 9 ? "They refused my apology. I gave them the routes anyway." : "I know the traps because I helped set them. You should know.",
            "CarouselPony" => wave >= 17 ? "Walk beside me a little. I want to choose the road." : wave >= 9 ? "The tune can play. I do not have to circle." : "Ask where I want to go before asking for a ride.",
            "SecretPatchBear" => wave >= 17 ? "Keep the claim ticket. I am keeping my name." : wave >= 9 ? "That patch can stay. Please ask before mending it." : "Do you need a receipt for me to have a seat?",
            _ => null
        };
    }
}
