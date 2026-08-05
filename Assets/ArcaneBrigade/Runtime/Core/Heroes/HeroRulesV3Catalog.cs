using System;
using System.Collections.Generic;
namespace ArcaneBrigade
{
    public sealed class HeroRuleV3Definition
    {
        public readonly string Id, Name, WeaponId, Mechanic, StarterId;
        public readonly string[] MinorBonds, BuildDirections;
        public readonly HeroRelicV3Definition[] Relics;
        public HeroRuleV3Definition(string id,string name,string weapon,string mechanic,string starter,string[] minors,string[] builds,HeroRelicV3Definition[] relics)
        { Id=id; Name=name; WeaponId=weapon; Mechanic=mechanic; StarterId=starter; MinorBonds=minors; BuildDirections=builds; Relics=relics; }
    }
    public readonly struct HeroRelicV3Definition
    {
        public readonly string Id, Name, Description;
        public HeroRelicV3Definition(string id,string name,string description) { Id=id; Name=name; Description=description; }
    }
    public readonly struct PersonalTraitV3Definition
    {
        public readonly string CompanionId, Name, Description;
        public PersonalTraitV3Definition(string companion,string name,string description) { CompanionId=companion; Name=name; Description=description; }
    }
    public static class HeroRulesV3Catalog
    {
        public static readonly HeroRuleV3Definition[] Definitions = {
            new HeroRuleV3Definition("LanternKnight","Lantern Knight","KeyCleaver","LANTERN STRIDE: travel 12 units to charge a melee pulse within 2.4 units; deals 65% weapon damage, minimum once every 4s. Holds one pulse until an enemy is nearby. No charge in shops or while paused.","KettleBat",new[] {"Gears","Bastion"},new[] {"Lantern + Hearth: a lamplit camp, stand still and regenerate","Clockwork + Stone: tempo and a stone wall","Lantern + Archive: marked and lit targets"}, new[] {
                new HeroRelicV3Definition("LanternKnight-0","Wick Trimmer","the Stride pulse is 30% wider"),
                new HeroRelicV3Definition("LanternKnight-1","Oil Flask","a charged pulse leaves a lamp for 4 s"),
                new HeroRelicV3Definition("LanternKnight-2","Night Watch Coat","+3 armor while a pulse is held"),
                new HeroRelicV3Definition("LanternKnight-3","Beacon Ring","friends in the pulse gain +10% damage for 4 s"),
            }),
            new HeroRuleV3Definition("Postmaster","Postmaster","MailboxLauncher","PARCEL EXPRESS: collect 6 gold to charge an automatic parcel volley; deals 100% weapon damage, minimum once every 4s. Passive gold and income do not count.","BookmarkGolem",new[] {"Paper Wings","Sniper"},new[] {"Archive + Lantern: recorded species pay parcels","Storm + Windborne: a volley of parcels","Garden + Venom: poisoned deliveries"}, new[] {
                new HeroRelicV3Definition("Postmaster-0","Express Stamp","parcels need 5 gold instead of 6"),
                new HeroRelicV3Definition("Postmaster-1","Registered Mail","a parcel marks the species it hits"),
                new HeroRelicV3Definition("Postmaster-2","Return to Sender","parcels that miss come back"),
                new HeroRelicV3Definition("Postmaster-3","Dead Letter Box","parcels leave a poison cloud"),
            }),
            new HeroRuleV3Definition("DrowsySeer","Drowsy Seer","MoonseedOrb","DREAM BREATH: stand still for 2 seconds to chill enemies within 3 units and remove up to 3 nearby hostile bullets; once every 7s. Waits for nearby danger. Moving interrupts charge; grants no invulnerability.","CandleSnail",new[] {"Hats","Cloud"},new[] {"Starlight + Dusk: sleeping marked enemies","Frost + Tide: a chilled, soaked dream","Bulwark + Spirit: dream guardians"}, new[] {
                new HeroRelicV3Definition("DrowsySeer-0","Pillow Talk","Dream Breath lasts 2 s longer"),
                new HeroRelicV3Definition("DrowsySeer-1","Night Cap","removing bullets heals the hero 2%"),
                new HeroRelicV3Definition("DrowsySeer-2","Lullaby Loop","breath sleeps enemies 1 s"),
                new HeroRelicV3Definition("DrowsySeer-3","Dream Catcher","removed bullets return as friendly shots"),
            }),
            new HeroRuleV3Definition("RootOracle","The Root Oracle","BambooVase","Stationary in combat. Whole-arena pickup and a slowing ward. Walks in shops.","MushroomLibrarian",new[] {"Root","Monolith"},new[] {"Garden + Venom: a poison grove","Frost + Tide: a winter ward","Stone + Clockwork: an unmovable fortress"}, new[] {
                new HeroRelicV3Definition("RootOracle-0","Deep Roots","the slowing ward is 25% wider"),
                new HeroRelicV3Definition("RootOracle-1","Root Cellar","pickups heal the hero 1%"),
                new HeroRelicV3Definition("RootOracle-2","Heartwood Charm","+15% max health while stationary"),
                new HeroRelicV3Definition("RootOracle-3","Sap Flask","the ward leaves sticky patches"),
            }),
            new HeroRuleV3Definition("LoneJelly","The Lone Jelly","InkyOctopus","Travels without roster friends; absorbs purchased friends' traits. Strong attacks and natural recovery support relic-focused builds.","JellySwordsman",new[] {"Clover","Multicaster"},new[] {"Carnival + Court: borrowed traits and a crown","Tide + Frost: soaked and chilled","Spirit + Bulwark: absorbed spirits"}, new[] {
                new HeroRelicV3Definition("LoneJelly-0","Sponge Tendril","absorbed traits are 20% stronger"),
                new HeroRelicV3Definition("LoneJelly-1","Jelly Crown","+1 trait absorbed from the first friend"),
                new HeroRelicV3Definition("LoneJelly-2","Slow Drift","natural recovery +50%"),
                new HeroRelicV3Definition("LoneJelly-3","Glow Pearl","kills drop a pearl of a random bond"),
            }),
            new HeroRuleV3Definition("GlassDice","Lady Loaded Dice","GlitchToybox","Collect 6 gold to burst nearby enemies. High luck and critical chance; low health.","DiceCrab",new[] {"Clover","Desperado"},new[] {"Carnival + Court: a rigged table","Starlight + Dusk: lucky critical hits","Garden + Venom: loaded dice with poison"}, new[] {
                new HeroRelicV3Definition("GlassDice-0","Loaded Die","the burst triggers at 5 gold"),
                new HeroRelicV3Definition("GlassDice-1","Snake Eyes","a double 1 refunds the burst"),
                new HeroRelicV3Definition("GlassDice-2","Pocket Pips","+2% crit per gold collected this wave"),
                new HeroRelicV3Definition("GlassDice-3","House Edge","bursts deal +20% to groups of 10 or more"),
            }),
            new HeroRuleV3Definition("TideSinger","The Tide Singer","RibbonHarp","Collect 8 gold to heal 3 health and push nearby enemies back.","AnchorSnail",new[] {"Sea Shanty","Mend"},new[] {"Tide + Frost: a healing, slowing sea","Starlight + Dusk: tidal wishes","Melody + Gravity: a song that pulls"}, new[] {
                new HeroRelicV3Definition("TideSinger-0","Conch Horn","the push is 30% stronger"),
                new HeroRelicV3Definition("TideSinger-1","Salt Cup","the heal also reaches friends"),
                new HeroRelicV3Definition("TideSinger-2","Pearl Necklace","every 3rd gold gives a bonus pearl"),
                new HeroRelicV3Definition("TideSinger-3","Shanty Book","Sea Shanty needs one member fewer"),
            }),
            new HeroRuleV3Definition("CometRunner","The Comet Courier","ClockCrescent","Travel 10 units to release a slowing pulse. Moving charges it; shops do not.","CometKite",new[] {"Quicksoles","Rapidfire"},new[] {"Starlight + Dusk: a trail of stars","Storm + Windborne: a lightning tail","Clockwork + Stone: runs that wind the clocks"}, new[] {
                new HeroRelicV3Definition("CometRunner-0","Tail Sparks","the slowing pulse leaves a star"),
                new HeroRelicV3Definition("CometRunner-1","Spare Wheels","moving 10% faster in the first 10 s"),
                new HeroRelicV3Definition("CometRunner-2","Cinder Trail","the pulse burns for 3 s"),
                new HeroRelicV3Definition("CometRunner-3","Horizon Map","the pulse charges 20% faster"),
            }),
            new HeroRuleV3Definition("MirrorKeeper","The Mirror Keeper","SnowGlobeOrb","Stand still for 2 seconds to reflect up to 3 nearby bullets. Moving resets charge.","VioletRemnant",new[] {"Gauntlet","Spiky Shell"},new[] {"Frost + Tide: reflecting cold","Carnival + Court: reflections with applause","Bulwark + Spirit: a mirror shield"}, new[] {
                new HeroRelicV3Definition("MirrorKeeper-0","Silver Backing","reflects 4 bullets instead of 3"),
                new HeroRelicV3Definition("MirrorKeeper-1","Hand Mirror","reflected bullets crit"),
                new HeroRelicV3Definition("MirrorKeeper-2","Prism Shard","reflected bullets split in two"),
                new HeroRelicV3Definition("MirrorKeeper-3","Cracked Frame","reflect heals the hero 1%"),
            }),
            new HeroRuleV3Definition("TreasureAcorn","The Treasure Clerk","AcornSling","Every 10 collected gold grants 1 extra gold and a small healing pulse.","BrassAcorn",new[] {"Clover","Bludgeoner"},new[] {"Garden + Venom: a fruitful tree","Archive + Lantern: ledger of finds","Starlight + Dusk: a lucky clerk"}, new[] {
                new HeroRelicV3Definition("TreasureAcorn-0","Gold Stamp","every 8th gold gives an extra gold"),
                new HeroRelicV3Definition("TreasureAcorn-1","Pouch Lining","the pulse heals 2% more"),
                new HeroRelicV3Definition("TreasureAcorn-2","Stock Ticker","+1% damage per 10 gold held (max 15%)"),
                new HeroRelicV3Definition("TreasureAcorn-3","Acorn Cap","chests open one tier higher"),
            }),
            new HeroRuleV3Definition("VerdantMatron","Verdant Matron","LeafCrossbow","SEED BASKET: every 8 coins collected plants a pod that grows into a Sprout ally for the wave (up to 3 alive)..","LittleSprout",new[] {"Root","Bastion"},new[] {"Garden + Venom: a bitter grove","Hearth + Ember: a feast garden","Bulwark + Spirit: protective vines"}, new[] {
                new HeroRelicV3Definition("VerdantMatron-0","Sun Hat","Sprouts last the whole run"),
                new HeroRelicV3Definition("VerdantMatron-1","Watering Can","Sprouts heal friends 3% a second"),
                new HeroRelicV3Definition("VerdantMatron-2","Bramble Gloves","Sprouts have thorns"),
                new HeroRelicV3Definition("VerdantMatron-3","Pollen Pouch","a 4th Sprout may live"),
            }),
            new HeroRuleV3Definition("CopperCauldron","Copper Cauldron","FireExtinguisher","SLOW BOIL: standing near 10 or more enemies for 3 s starts a boil: for 5 s the hero's shots are 30% faster and burn..","TeacupCricket",new[] {"Sweet Tooth","Spiky Shell"},new[] {"Hearth + Ember: a roaring stew","Carnival + Court: a bubbling show","Stone + Clockwork: a pressure cooker"}, new[] {
                new HeroRelicV3Definition("CopperCauldron-0","Lid Rattle","the boil starts at 8 enemies"),
                new HeroRelicV3Definition("CopperCauldron-1","Wooden Ladle","the boil also heals the hero 1% a second"),
                new HeroRelicV3Definition("CopperCauldron-2","Chilli Pot","burn stacks one more"),
                new HeroRelicV3Definition("CopperCauldron-3","Soup Ticket","kills during the boil drop a coin"),
            }),
            new HeroRuleV3Definition("ChinaMarionette","China Marionette","NeedleSinger","STRINGS: friends within 4 units of the hero are tied to the hero and take 20% less damage; the hero takes a part of it (up to 10% max health)..","JellySwordsman",new[] {"Masks","Shield"},new[] {"Frost + Tide: porcelain ice","Carnival + Court: a puppet show","Bulwark + Spirit: a guarded stage"}, new[] {
                new HeroRelicV3Definition("ChinaMarionette-0","Silk Thread","the radius is 5 units"),
                new HeroRelicV3Definition("ChinaMarionette-1","Porcelain Powder","the hero takes 5% less of the shared damage"),
                new HeroRelicV3Definition("ChinaMarionette-2","Stage Lights","tied friends gain +10% crit"),
                new HeroRelicV3Definition("ChinaMarionette-3","Spare Doll","a fallen friend rises 3 s sooner"),
            }),
            new HeroRuleV3Definition("SoapSkater","Soap Skater","BubbleWand","SLIDE: moving builds Lather; at full Lather the next shot leaves a slippery trail that slows enemies 40% for 3 s..","SailSquid",new[] {"Quicksoles","Rapidfire"},new[] {"Frost + Tide: a rink","Garden + Venom: a slippery grove","Stone + Clockwork: clockwork skates"}, new[] {
                new HeroRelicV3Definition("SoapSkater-0","Bubble Rink","the trail is 50% longer"),
                new HeroRelicV3Definition("SoapSkater-1","Roller Wheels","Lather fills 25% faster"),
                new HeroRelicV3Definition("SoapSkater-2","Foam Cannon","the trail blinds enemies 1 s"),
                new HeroRelicV3Definition("SoapSkater-3","Rinse Cycle","the trail cleans friends of debuffs"),
            }),
            new HeroRuleV3Definition("VelvetRegent","Velvet Regent","StarforgeClaymore","ROYAL GUARD: when a friend falls the hero gains +25% damage for 8 s and the friend rises 5 s sooner..","ButtonBeetle",new[] {"Hats","Bastion"},new[] {"Archive + Lantern: a recorded reign","Clockwork + Stone: a standing court","Carnival + Court: a crowned show"}, new[] {
                new HeroRelicV3Definition("VelvetRegent-0","Ermine Collar","+30% damage during Royal Guard"),
                new HeroRelicV3Definition("VelvetRegent-1","Scepter Tip","Royal Guard lasts 4 s longer"),
                new HeroRelicV3Definition("VelvetRegent-2","Page Boy","the friend rises 3 s sooner still"),
                new HeroRelicV3Definition("VelvetRegent-3","Regent's Seal","a revive also gives a 20% shield"),
            }),
            new HeroRuleV3Definition("ButtonCavalier","Button Cavalier","CorkscrewRapier","CHARGE: every 6 s the hero dashes toward the largest cluster, dealing 100% weapon damage along the path..","TinTick",new[] {"Gears","Unstoppable"},new[] {"Carnival + Court: a jousting show","Clockwork + Stone: an armoured charge","Bulwark + Spirit: a guardian knight"}, new[] {
                new HeroRelicV3Definition("ButtonCavalier-0","Lance Tip","Charge hits twice"),
                new HeroRelicV3Definition("ButtonCavalier-1","Spare Button","Charge every 5 s"),
                new HeroRelicV3Definition("ButtonCavalier-2","Plume","Charge gives +20% speed for 3 s"),
                new HeroRelicV3Definition("ButtonCavalier-3","Horse Blanket","Charge shields the hero 10%"),
            }),
            new HeroRuleV3Definition("NightMaw","Night Maw","DrumstickClub","DEVOUR: kills heal 1% max health; every 20 kills grows the Maw +5% size and damage until the end of the wave..","MaskPepper",new[] {"Gauntlet","Masks"},new[] {"Carnival + Court: a hungry mascot","Starlight + Dusk: devouring shadows","Storm + Windborne: a howling maw"}, new[] {
                new HeroRelicV3Definition("NightMaw-0","Sharp Stitch","Devour heals 1.5%"),
                new HeroRelicV3Definition("NightMaw-1","Second Jaw","the Maw grows every 15 kills"),
                new HeroRelicV3Definition("NightMaw-2","Red Scarf","+10% damage when above 70% health"),
                new HeroRelicV3Definition("NightMaw-3","Bone Bell","elites count as 5 kills"),
            }),
            new HeroRuleV3Definition("ThornTreasurer","Thorn Treasurer","GumballGatling","THORN PURSE: every 12 coins collected fires a ring of thorns that poisons what it hits..","SeedHedgehog",new[] {"Horns","Bludgeoner"},new[] {"Garden + Venom: a thorny fortune","Archive + Lantern: an inventory of thorns","Stone + Clockwork: a bank of stones"}, new[] {
                new HeroRelicV3Definition("ThornTreasurer-0","Rose Clasp","thorns every 10 coins"),
                new HeroRelicV3Definition("ThornTreasurer-1","Thorn Ledger","thorns crit"),
                new HeroRelicV3Definition("ThornTreasurer-2","Gilt Prick","thorns also stun 0.3 s"),
                new HeroRelicV3Definition("ThornTreasurer-3","Coin Spike","every 5th thorn drops a coin"),
            }),
            new HeroRuleV3Definition("CitrusMarshal","Citrus Marshal","SignpostSlammer","SIGNPOST: plants a signpost every 10 s that enemies must walk around; the hero gains +3 armor beside it..","CitrusToad",new[] {"Quicksoles","Monolith"},new[] {"Garden + Venom: a sour grove","Hearth + Ember: a roadside kitchen","Stone + Clockwork: a fortified road"}, new[] {
                new HeroRelicV3Definition("CitrusMarshal-0","Hard Hat","signposts last 6 s longer"),
                new HeroRelicV3Definition("CitrusMarshal-1","Reflective Paint","signposts reflect bullets"),
                new HeroRelicV3Definition("CitrusMarshal-2","Directions","signposts push enemies away"),
                new HeroRelicV3Definition("CitrusMarshal-3","Wayfinder Map","two signposts at once"),
            }),
            new HeroRuleV3Definition("IvorySovereign","Ivory Sovereign","DreamStamp","DECREE: at the wave start choose 1 of 3 decrees that last the wave (more shop slots, +damage, or fewer enemies)..","IceCreamOwl",new[] {"Hats","Cloud"},new[] {"Archive + Lantern: written law","Frost + Tide: a cold court","Starlight + Dusk: fate and judgment"}, new[] {
                new HeroRelicV3Definition("IvorySovereign-0","Gavel","a 4th decree option"),
                new HeroRelicV3Definition("IvorySovereign-1","Royal Seal","a decree may last two waves"),
                new HeroRelicV3Definition("IvorySovereign-2","Scribe","the decree text also marks a species"),
                new HeroRelicV3Definition("IvorySovereign-3","Clemency","once a run a decree may be re-chosen"),
            }),
            new HeroRuleV3Definition("PetalCitadel","Petal Citadel","RoseBouquet","BLOOM WALL: standing still grows petal walls that block enemy bullets; moving fades them..","PearMoth",new[] {"Root","Bastion"},new[] {"Garden + Venom: a poisoned bloom","Starlight + Dusk: night bloom","Storm + Windborne: petals on the wind"}, new[] {
                new HeroRelicV3Definition("PetalCitadel-0","Rose Window","walls reflect bullets"),
                new HeroRelicV3Definition("PetalCitadel-1","Thorn Trellis","walls hurt melee enemies"),
                new HeroRelicV3Definition("PetalCitadel-2","Stained Glass","walls grow 25% faster"),
                new HeroRelicV3Definition("PetalCitadel-3","Gate Key","a wall also shields friends"),
            }),
            new HeroRuleV3Definition("PatchworkHost","Patchwork Host","ConfettiBlunderbuss","PATCHWORK: the hero copies the last skill any friend cast, at half strength..","SockCyclops",new[] {"Big Top","Multicaster"},new[] {"Carnival + Court: a mixed show","Hearth + Ember: leftovers and fire","Archive + Lantern: copied pages"}, new[] {
                new HeroRelicV3Definition("PatchworkHost-0","Spare Patch","copies are at 60%"),
                new HeroRelicV3Definition("PatchworkHost-1","Needle Case","copies cost no bar"),
                new HeroRelicV3Definition("PatchworkHost-2","Quilting Frame","copies of heals reach all friends"),
                new HeroRelicV3Definition("PatchworkHost-3","Mending Thread","a copy of a shield stays 3 s longer"),
            }),
            new HeroRuleV3Definition("ArchiveCurator","Archive Curator","SleepCamera","CATALOGUE: each new enemy species seen gives +1% damage for the run and a coin..","FridgeWisp",new[] {"Runes","Sniper"},new[] {"Archive + Lantern: a complete collection","Stone + Clockwork: preserved specimens","Frost + Tide: pickled specimens"}, new[] {
                new HeroRelicV3Definition("ArchiveCurator-0","Magnifier","marked species take +5%"),
                new HeroRelicV3Definition("ArchiveCurator-1","Index Cards","species coins are doubled"),
                new HeroRelicV3Definition("ArchiveCurator-2","Display Case","bosses count as 3 species"),
                new HeroRelicV3Definition("ArchiveCurator-3","Camera Flash","a new species is stunned 1 s"),
            }),
            new HeroRuleV3Definition("PennyBroker","Penny Broker","DetonatorBox","HAGGLE: recruits cost 10% less; every purchase gives +3% damage for the next wave..","MailboxImp",new[] {"Gears","Bludgeoner"},new[] {"Archive + Lantern: appraised goods","Hearth + Ember: a burning bargain","Stone + Clockwork: coin presses"}, new[] {
                new HeroRelicV3Definition("PennyBroker-0","Fine Print","recruits cost 15% less"),
                new HeroRelicV3Definition("PennyBroker-1","Receipt Roll","purchases stack to +9%"),
                new HeroRelicV3Definition("PennyBroker-2","Pawn Ticket","a sold friend refunds 100%"),
                new HeroRelicV3Definition("PennyBroker-3","Loose Change","kills drop a coin 10% of the time"),
            }),
            new HeroRuleV3Definition("StormFencer","Storm Fencer","PlasmaTrident","RIPOSTE: 3 s after the hero is hit the next shot always crits and chains lightning..","SpringDuck",new[] {"Paper Wings","Desperado"},new[] {"Storm + Windborne: a lightning duel","Clockwork + Stone: a metronome of steel","Starlight + Dusk: a duellist's fate"}, new[] {
                new HeroRelicV3Definition("StormFencer-0","Rapier Wire","Riposte chains to 4"),
                new HeroRelicV3Definition("StormFencer-1","Foil Guard","Riposte also blocks a bullet"),
                new HeroRelicV3Definition("StormFencer-2","Duelling Glove","Riposte after 2 s"),
                new HeroRelicV3Definition("StormFencer-3","Storm Cloak","+10% speed after a Riposte"),
            }),
            new HeroRuleV3Definition("TinVirtuoso","Tin Virtuoso","MusicBox","METRONOME: every 3 s a beat sounds; shots fired on the beat deal +40%..","StapleMole",new[] {"Gears","Rapidfire"},new[] {"Melody + Gravity: a march","Clockwork + Stone: a music box","Carnival + Court: a one-man orchestra"}, new[] {
                new HeroRelicV3Definition("TinVirtuoso-0","Wind-up Key","beat every 2.5 s"),
                new HeroRelicV3Definition("TinVirtuoso-1","Brass Valve","on-beat shots pierce"),
                new HeroRelicV3Definition("TinVirtuoso-2","Sheet Music","friends also gain 15% on the beat"),
                new HeroRelicV3Definition("TinVirtuoso-3","Tin Whistle","three on-beat shots in a row chain"),
            }),
            new HeroRuleV3Definition("MoonProwler","Moon Prowler","MoonScythe","PROWL: standing still for 2 s makes the hero unseen; the next hit from stealth deals triple damage..","StarSock",new[] {"Masks","Bludgeoner"},new[] {"Starlight + Dusk: a moonlit ambush","Frost + Tide: a silent hunt","Spirit + Bulwark: a shadow guardian"}, new[] {
                new HeroRelicV3Definition("MoonProwler-0","Velvet Paws","stealth after 1.5 s"),
                new HeroRelicV3Definition("MoonProwler-1","Moon Dust","stealth shots mark"),
                new HeroRelicV3Definition("MoonProwler-2","Night Collar","+20% crit from stealth"),
                new HeroRelicV3Definition("MoonProwler-3","Silver Claw","stealth hits stun 0.5 s"),
            }),
            new HeroRuleV3Definition("SupperMortar","Supper Mortar","TeapotPopgun","SUPPER: each food pickup is lobbed as a shell that heals friends in an area and burns enemies..","BreadCrab",new[] {"Sweet Tooth","Mend"},new[] {"Hearth + Ember: a banquet","Tide + Frost: soups and ice","Garden + Venom: a poisoned table"}, new[] {
                new HeroRelicV3Definition("SupperMortar-0","Gravy Boat","shells are 30% larger"),
                new HeroRelicV3Definition("SupperMortar-1","Dinner Bell","shells land twice"),
                new HeroRelicV3Definition("SupperMortar-2","Spice Rack","shells apply burn"),
                new HeroRelicV3Definition("SupperMortar-3","Napkin","shells cleanse friends"),
            }),
            new HeroRuleV3Definition("LanternWeaver","Lantern Weaver","LanternStaff","WEAVE: lines of light join friends; enemies crossing a line take damage and are lit..","ThimbleMoth",new[] {"Runes","Monolith"},new[] {"Archive + Lantern: a loom of light","Starlight + Dusk: a net of stars","Melody + Gravity: strings that pull"}, new[] {
                new HeroRelicV3Definition("LanternWeaver-0","Golden Thread","lines last 50% longer"),
                new HeroRelicV3Definition("LanternWeaver-1","Shuttle","the hero joins the web"),
                new HeroRelicV3Definition("LanternWeaver-2","Spool of Dusk","lines slow enemies"),
                new HeroRelicV3Definition("LanternWeaver-3","Tassel","line damage crits"),
            }),
            new HeroRuleV3Definition("HourglassStag","Hourglass Stag","AlarmMortar","HOURGLASS: every 15 s time slows 40% around the hero for 3 s..","DreamCompass",new[] {"Gears","Cloud"},new[] {"Clockwork + Stone: slow minutes","Frost + Tide: a frozen moment","Starlight + Dusk: a stretched night"}, new[] {
                new HeroRelicV3Definition("HourglassStag-0","Fine Sand","every 12 s"),
                new HeroRelicV3Definition("HourglassStag-1","Brass Rim","slow 55%"),
                new HeroRelicV3Definition("HourglassStag-2","Glass Bulb","friends cast 20% faster inside"),
                new HeroRelicV3Definition("HourglassStag-3","Antler Bell","the end of the slow stuns 0.5 s"),
            }),
            new HeroRuleV3Definition("RainArchivist","Rain Archivist","SquirtTrio","DOWNPOUR: rain falls over the arena each wave: it soaks enemies, and friends in the rain gain +10% speed..","RibbonOtter",new[] {"Rain Script","Mend"},new[] {"Tide + Frost: a flood and ice","Storm + Windborne: a thunder shower","Garden + Venom: a nourished grove"}, new[] {
                new HeroRelicV3Definition("RainArchivist-0","Rain Gauge","rain starts 5 s sooner"),
                new HeroRelicV3Definition("RainArchivist-1","Scroll Case","rain also clears debuffs"),
                new HeroRelicV3Definition("RainArchivist-2","Wet Ink","soaked enemies take +5%"),
                new HeroRelicV3Definition("RainArchivist-3","Umbrella Clasp","rain heals friends 1% a second"),
            }),
            new HeroRuleV3Definition("KiteAdmiral","Kite Admiral","KiteSpool","FLEET: every 8 s a kite fleet sails along a flight line hitting everything in it..","PaperPlanePigeon",new[] {"Paper Wings","Sniper"},new[] {"Storm + Windborne: a squall line","Starlight + Dusk: a comet fleet","Melody + Gravity: a sky orchestra"}, new[] {
                new HeroRelicV3Definition("KiteAdmiral-0","Spyglass","the fleet is 30% longer"),
                new HeroRelicV3Definition("KiteAdmiral-1","Flagship","the fleet carries a bomb"),
                new HeroRelicV3Definition("KiteAdmiral-2","Rigging","the fleet every 6 s"),
                new HeroRelicV3Definition("KiteAdmiral-3","Weather Chart","the fleet follows wind lanes"),
            }),
            new HeroRuleV3Definition("SundaeDuchess","Sundae Duchess","ScoopLauncher","SUNDAE: every 10 s she throws a scoop that freezes enemies and heals nearby friends 10%..","BakerMouse",new[] {"Sweet Tooth","Mend"},new[] {"Hearth + Ember: hot fudge","Frost + Tide: a double scoop","Bulwark + Spirit: a sweet shield"}, new[] {
                new HeroRelicV3Definition("SundaeDuchess-0","Waffle Cone","the scoop is 30% larger"),
                new HeroRelicV3Definition("SundaeDuchess-1","Cherry Bomb","the scoop explodes at the end"),
                new HeroRelicV3Definition("SundaeDuchess-2","Sprinkles","the scoop crits"),
                new HeroRelicV3Definition("SundaeDuchess-3","Extra Whip","the heal is 15%"),
            }),
            new HeroRuleV3Definition("BonsaiClocksmith","Bonsai Clocksmith","CuckooCannon","PRUNING: every wave survived adds +2% skill speed for the run; once a wave trim a friend's bar to refill it at once..","TopBeetle",new[] {"Gears","Multicaster"},new[] {"Clockwork + Stone: a slow-growing machine","Garden + Venom: a fruiting clock","Archive + Lantern: a recorded tree"}, new[] {
                new HeroRelicV3Definition("BonsaiClocksmith-0","Fine Shears","+3% a wave"),
                new HeroRelicV3Definition("BonsaiClocksmith-1","Cuckoo Chime","trim refills two bars"),
                new HeroRelicV3Definition("BonsaiClocksmith-2","Grafting Knife","trimmed friends gain +10% damage 5 s"),
                new HeroRelicV3Definition("BonsaiClocksmith-3","Moss Gear","skill speed also grows between waves"),
            }),
            new HeroRuleV3Definition("JugglingJester","Juggling Jester","JugglingPins","JUGGLE: keeps up to 3 projectiles in the air; they bounce between enemies and return to the hero..","LensOwl",new[] {"Big Top","Rapidfire"},new[] {"Carnival + Court: the main act","Archive + Lantern: a lit stage","Melody + Gravity: a juggling rhythm"}, new[] {
                new HeroRelicV3Definition("JugglingJester-0","Fourth Pin","4 projectiles"),
                new HeroRelicV3Definition("JugglingJester-1","Chalk Hands","pins crit"),
                new HeroRelicV3Definition("JugglingJester-2","Bell Cap","pins stun on the 5th bounce"),
                new HeroRelicV3Definition("JugglingJester-3","Acrobat Mat","pins heal the hero 1%"),
            }),
            new HeroRuleV3Definition("BellDiver","Bell Diver","SonarHarpoon","DIVE: every 10 s the hero dives (untargetable for 2 s) and surfaces with a sonar pulse that marks enemies..","RaincoatFrog",new[] {"Rain Script","Bastion"},new[] {"Tide + Frost: the deep","Spirit + Bulwark: a sunken guardian","Melody + Gravity: sonar and pressure"}, new[] {
                new HeroRelicV3Definition("BellDiver-0","Air Tank","untargetable 3 s"),
                new HeroRelicV3Definition("BellDiver-1","Depth Gauge","marked enemies take +8%"),
                new HeroRelicV3Definition("BellDiver-2","Brass Helmet","surfacing shields friends"),
                new HeroRelicV3Definition("BellDiver-3","Sonar Pearl","the pulse pushes enemies"),
            }),
        };
        public static readonly PersonalTraitV3Definition[] Traits = {
            new PersonalTraitV3Definition("TeaDragon","Undying Flame","the coals it leaves stay for the whole run (up to 12 patches) and burn at +50%"),
            new PersonalTraitV3Definition("CoilKettle","Coil Shot","the third bounce of Spring Boil becomes a coil shot that chains to two more clusters"),
            new PersonalTraitV3Definition("SundaeGolem","Cherry on Top","every Cherry it drops also adds Warmth and a coin"),
            new PersonalTraitV3Definition("KiteDragon","Gale Ribbon","each Ribbon Sweep leaves a wind lane for 6 s"),
            new PersonalTraitV3Definition("EclipseEgg","Dusk or Dawn","its form is bright after a wave with more crits and dark after a wave with more kills: bright gives +Wishes, dark gives +Dusk"),
            new PersonalTraitV3Definition("FirstSeed","Thorn Garden","its Sprout stays as a seed pod that grows each wave and finally fruits"),
            new PersonalTraitV3Definition("ClockworkPiper","Tuning Fork","Tune the Clocks stacks up to 3 times (each stack +10% fill speed for 6 s)"),
            new PersonalTraitV3Definition("EndlessIndex","Marginalia","Last Page also records the skill, so a copied skill can be used once more at the next wave"),
            new PersonalTraitV3Definition("JackInTheBox","Centre Stage","while its Spotlight is up, every skill cast adds Applause twice"),
            new PersonalTraitV3Definition("LostCarousel","The Arena Spins","its horses become a lasting ring: enemies cannot stand inside the carousel while it spins"),
            new PersonalTraitV3Definition("LighthouseWhale","Sweeping Beam","its Fog Horn also sends the lighthouse beam around the whole arena once"),
            new PersonalTraitV3Definition("RainbowBell","Third Toll","every third toll also rings every Clockwork member once (adds Ticks)"),
            new PersonalTraitV3Definition("PhoenixChick","Second Wind","a friend revived by its feather returns with 60% health and +20% damage for 8 s"),
            new PersonalTraitV3Definition("PlanetHamster","Gravity Well","counts as two members for Gravity and pulls enemy bullets toward itself"),
            new PersonalTraitV3Definition("Metronome","Perfect Pitch","every Beat of the Melody bond also refills 10% of every friend's skill bar"),
            new PersonalTraitV3Definition("TowerRook","Stronghold","counts as two members for Bulwark and Spirit and doubles the Bastion wall time"),
        };
        public static HeroRuleV3Definition Find(string id)
        { for(int i=0;i<Definitions.Length;i++) if(string.Equals(Definitions[i].Id,id,StringComparison.OrdinalIgnoreCase)||string.Equals(Definitions[i].Name,id,StringComparison.OrdinalIgnoreCase)) return Definitions[i]; return null; }
        public static string[] MinorIds(string heroId)
        { var h=Find(heroId); return h==null ? Array.Empty<string>() : new[] { BondV3Catalog.BondId(h.MinorBonds[0]),BondV3Catalog.BondId(h.MinorBonds[1]) }; }
        public static int ExtraMembership(string companionId,string bondId)
        { return companionId=="PlanetHamster" && bondId=="gravity" || companionId=="TowerRook" && (bondId=="bulwark"||bondId=="spirit") ? HeroRulesV3Tuning.ExtraMembership : 0; }
    }
}
