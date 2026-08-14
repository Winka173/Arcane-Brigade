using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    /// <summary>Missing v3 roster entries are rules data; presentation may provide their art independently.</summary>
    public static class RulesV3CatalogAdapter
    {
        private readonly struct FriendRow
        {
            public readonly string Id, Name; public readonly ItemRarity Rarity; public readonly bool Melee;
            public FriendRow(string id,string name,ItemRarity rarity,bool melee) { Id=id;Name=name;Rarity=rarity;Melee=melee; }
        }
        private static readonly FriendRow[] Friends = {
            new FriendRow("TeapotPilgrim","Teapot Pilgrim",ItemRarity.Common,false),
            new FriendRow("CandleSnail","Candle Snail",ItemRarity.Uncommon,false),
            new FriendRow("MailboxImp","Mailbox Imp",ItemRarity.Common,false),
            new FriendRow("HoodWisp","Hood Wisp",ItemRarity.Common,false),
            new FriendRow("BreadCrab","Bread Crab",ItemRarity.Common,true),
            new FriendRow("KettleBat","Kettle Bat",ItemRarity.Common,false),
            new FriendRow("BookmarkGolem","Bookmark Golem",ItemRarity.Rare,false),
            new FriendRow("SnowSpoon","Snow Spoon",ItemRarity.Common,false),
            new FriendRow("FridgeWisp","Fridge Wisp",ItemRarity.Common,true),
            new FriendRow("PearMoth","Pear Moth",ItemRarity.Uncommon,true),
            new FriendRow("CometKite","Comet Kite",ItemRarity.Common,false),
            new FriendRow("ShellPostman","Shell Postman",ItemRarity.Common,false),
            new FriendRow("CoralTeacup","Coral Teacup",ItemRarity.Uncommon,false),
            new FriendRow("AnchorSnail","Anchor Snail",ItemRarity.Uncommon,false),
            new FriendRow("SailSquid","Sail Squid",ItemRarity.Uncommon,true),
            new FriendRow("DiceCrab","Dice Crab",ItemRarity.Common,false),
            new FriendRow("MaskPepper","Mask Pepper",ItemRarity.Common,false),
            new FriendRow("CoilKettle","Coil Kettle",ItemRarity.Mythic,false),
            new FriendRow("ThimbleMoth","Thimble Moth",ItemRarity.Rare,false),
            new FriendRow("ReelOwl","Reel Owl",ItemRarity.Mythic,false),
            new FriendRow("RainbowTailor","The Impossible Tailor",ItemRarity.Rainbow,true),
            new FriendRow("SecretPaperBird","The Folded Courier",ItemRarity.Rare,false),
            new FriendRow("SaffronMechanist","Saffron Mechanist",ItemRarity.Rare,true),
            new FriendRow("VelvetChampion","Velvet Champion",ItemRarity.Rare,true),
            new FriendRow("IronLocomotive","Iron Locomotive",ItemRarity.Mythic,false),
            new FriendRow("BalloonMallet","Balloon Mallet",ItemRarity.Uncommon,true),
            new FriendRow("TeacupCricket","Teacup Cricket",ItemRarity.Uncommon,true),
            new FriendRow("RibbonOtter","Ribbon Otter",ItemRarity.Uncommon,true),
            new FriendRow("SpindleWren","Spindle Wren",ItemRarity.Uncommon,false),
            new FriendRow("BakerMouse","Baker Mouse",ItemRarity.Uncommon,false),
            new FriendRow("TeaDragon","Tea Dragon",ItemRarity.Mythic,false),
            new FriendRow("SundaeGolem","Sundae Golem",ItemRarity.Rainbow,false),
            new FriendRow("LensOwl","Lens Owl",ItemRarity.Rare,false),
            new FriendRow("EndlessIndex","The Endless Index",ItemRarity.Rainbow,false),
            new FriendRow("ShovelPenguin","Shovel Penguin",ItemRarity.Rare,false),
            new FriendRow("BobaTadpole","Boba Tadpole",ItemRarity.Common,false),
            new FriendRow("RainHeron","Rain Heron",ItemRarity.Rare,false),
            new FriendRow("PaperPlanePigeon","Paper Plane Pigeon",ItemRarity.Common,false),
            new FriendRow("RaincoatFrog","Raincoat Frog",ItemRarity.Uncommon,false),
            new FriendRow("LighthouseWhale","Harbor Whale",ItemRarity.Mythic,false),
            new FriendRow("KiteDragon","Kite Dragon",ItemRarity.Rainbow,false),
            new FriendRow("StaticFerret","Static Ferret",ItemRarity.Common,true),
            new FriendRow("StormKitten","Storm Kitten",ItemRarity.Uncommon,false),
            new FriendRow("VoltEel","Volt Eel",ItemRarity.Rare,false),
            new FriendRow("ShantySeaLion","Shanty Sea Lion",ItemRarity.Common,false),
            new FriendRow("BottleGull","Bottle Gull",ItemRarity.Uncommon,false),
            new FriendRow("PinnedBeetle","Pinned Beetle",ItemRarity.Common,false),
            new FriendRow("FossilArmadillo","Fossil Armadillo",ItemRarity.Uncommon,false),
            new FriendRow("PillowOracle","Pillow Oracle",ItemRarity.Mythic,false),
            new FriendRow("MushroomLibrarian","Mushroom Librarian",ItemRarity.Common,false),
            new FriendRow("JellySwordsman","Jelly Swordsman",ItemRarity.Common,true),
            new FriendRow("BrassAcorn","Brass Acorn",ItemRarity.Common,true),
            new FriendRow("TinTick","Tin Tick",ItemRarity.Common,false),
            new FriendRow("SpringDuck","Spring Duck",ItemRarity.Rare,true),
            new FriendRow("ButtonBeetle","Button Beetle",ItemRarity.Common,false),
            new FriendRow("StapleMole","Staple Mole",ItemRarity.Common,false),
            new FriendRow("IceCreamOwl","Ice Cream Owl",ItemRarity.Uncommon,false),
            new FriendRow("CitrusToad","Citrus Toad",ItemRarity.Uncommon,false),
            new FriendRow("RadishJester","Radish Jester",ItemRarity.Common,false),
            new FriendRow("SeedHedgehog","Seed Hedgehog",ItemRarity.Common,false),
            new FriendRow("MoonJelly","Moon Jelly",ItemRarity.Rare,false),
            new FriendRow("StarSock","Star Sock",ItemRarity.Uncommon,false),
            new FriendRow("EclipseEgg","Eclipse Egg",ItemRarity.Rainbow,false),
            new FriendRow("OrbitMoth","Orbit Moth",ItemRarity.Common,false),
            new FriendRow("DreamCompass","Dream Compass",ItemRarity.Common,false),
            new FriendRow("BubbleKoi","Bubble Koi",ItemRarity.Common,false),
            new FriendRow("SockCyclops","Sock Cyclops",ItemRarity.Common,true),
            new FriendRow("CrayonGremlin","Crayon Gremlin",ItemRarity.Common,false),
            new FriendRow("MirrorNewt","Mirror Newt",ItemRarity.Mythic,false),
            new FriendRow("RibbonSpider","Ribbon Spider",ItemRarity.Common,true),
            new FriendRow("RainbowBell","The Last Bell",ItemRarity.Rainbow,false),
            new FriendRow("SecretPatchBear","The Forgotten Patchwork",ItemRarity.Rare,false),
            new FriendRow("SecretDreamLamb","The Sleeping Friend",ItemRarity.Rare,false),
            new FriendRow("ClockworkPiper","Clockwork Piper",ItemRarity.Mythic,false),
            new FriendRow("PaperInventor","Paper Inventor",ItemRarity.Mythic,false),
            new FriendRow("ReturnCaptain","Return Captain",ItemRarity.Mythic,false),
            new FriendRow("DoughForeman","Dough Foreman",ItemRarity.Rare,true),
            new FriendRow("VioletRemnant","Violet Remnant",ItemRarity.Uncommon,true),
            new FriendRow("VelvetEnchantress","Velvet Enchantress",ItemRarity.Uncommon,false),
            new FriendRow("GrandSprout","Grand Sprout",ItemRarity.Uncommon,true),
            new FriendRow("BristleBall","Bristle Ball",ItemRarity.Uncommon,true),
            new FriendRow("PaperDevil","Paper Devil",ItemRarity.Uncommon,true),
            new FriendRow("LittleSprout","Little Sprout",ItemRarity.Uncommon,true),
            new FriendRow("MonsterMarble","Monster Marble",ItemRarity.Uncommon,true),
            new FriendRow("CarouselPony","Carousel Pony",ItemRarity.Uncommon,false),
            new FriendRow("LampGoat","Lamp Goat",ItemRarity.Uncommon,true),
            new FriendRow("TopBeetle","Top Beetle",ItemRarity.Rare,false),
            new FriendRow("WateringSnail","Watering Snail",ItemRarity.Uncommon,false),
            new FriendRow("YoyoSquirrel","Yo-yo Squirrel",ItemRarity.Rare,false),
            new FriendRow("FirstSeed","The First Seed",ItemRarity.Rainbow,false),
            new FriendRow("JackInTheBox","Ringmaster Jack",ItemRarity.Mythic,false),
            new FriendRow("LostCarousel","The Lost Carousel",ItemRarity.Rainbow,false),
            new FriendRow("SpecimenSlime","Specimen Slime",ItemRarity.Rare,false),
            new FriendRow("CourtHerald","Court Herald",ItemRarity.Common,false),
            new FriendRow("StitchedPangolin","Stitched Pangolin",ItemRarity.Uncommon,true),
            new FriendRow("SaddleMoth","Saddle Moth",ItemRarity.Rare,false),
            new FriendRow("TowerRook","Tower Rook",ItemRarity.Rare,true),
            new FriendRow("LifebuoyHippo","Lifebuoy Hippo",ItemRarity.Uncommon,true),
            new FriendRow("NurseHen","Nurse Hen",ItemRarity.Uncommon,false),
            new FriendRow("PhoenixChick","Phoenix Chick",ItemRarity.Mythic,false),
            new FriendRow("DandelionPuff","Dandelion Puff",ItemRarity.Rare,false),
            new FriendRow("TotemPole","Totem Pole",ItemRarity.Rare,false),
            new FriendRow("BrassLion","Brass Lion",ItemRarity.Uncommon,true),
            new FriendRow("QuiltWalrus","Quilt Walrus",ItemRarity.Common,true),
            new FriendRow("PlasterPanda","Plaster Panda",ItemRarity.Common,false),
            new FriendRow("PumpkinReaper","Pumpkin Reaper",ItemRarity.Uncommon,true),
            new FriendRow("BoneXylophone","Bone Xylophone",ItemRarity.Common,false),
            new FriendRow("ScarecrowCrow","Scarecrow Crow",ItemRarity.Uncommon,true),
            new FriendRow("Metronome","Metronome",ItemRarity.Rare,false),
            new FriendRow("HarpSwan","Harp Swan",ItemRarity.Uncommon,false),
            new FriendRow("BagpipeOctopus","Bagpipe Octopus",ItemRarity.Uncommon,false),
            new FriendRow("AnvilOx","Anvil Ox",ItemRarity.Uncommon,true),
            new FriendRow("SinkholeWorm","Sinkhole Worm",ItemRarity.Common,false),
            new FriendRow("PlanetHamster","Planet Hamster",ItemRarity.Mythic,false),
            new FriendRow("CymbalMonkey","Cymbal Monkey",ItemRarity.Common,true),
            new FriendRow("ChoirCanary","Choir Canary",ItemRarity.Common,false),
            new FriendRow("PianoCrocodile","Piano Crocodile",ItemRarity.Rare,false),
            new FriendRow("NewtonApple","Newton Apple",ItemRarity.Common,false),
            new FriendRow("BowlingSloth","Bowling Sloth",ItemRarity.Uncommon,true),
            new FriendRow("SatelliteBee","Satellite Bee",ItemRarity.Rare,false),
        };
        public static void Configure(RunCatalog catalog)
        {
            var companions=new List<UnitDefinition>(catalog.Companions??Array.Empty<UnitDefinition>());
            WeaponDefinition melee=catalog.Weapon("SpoonSabre"), ranged=catalog.Weapon("InkPistol");
            if(melee==null || ranged==null)
                foreach(WeaponDefinition weapon in catalog.Weapons??Array.Empty<WeaponDefinition>())
                { if(melee==null && weapon.Kind==WeaponKind.Melee) melee=weapon; if(ranged==null && weapon.Kind==WeaponKind.Projectile) ranged=weapon; }
            foreach(FriendRow row in Friends)
            {
                bool exists=false;foreach(UnitDefinition unit in companions) if(unit.Id==row.Id)
                {
                    // Section 6 makes these roster species available independently of retired hero gifts and exclusives.
                    unit.GiftOnly=false;unit.ExclusiveHero=string.Empty;
                    exists=true;break;
                }
                if(exists)continue;
                UnitDefinition friend=ScriptableObject.CreateInstance<UnitDefinition>();
                friend.hideFlags=HideFlags.DontSave; friend.Id=row.Id;friend.DisplayName=row.Name;friend.Rarity=row.Rarity;
                friend.Health=MortalityTuning.DefaultMaxHealth;friend.Weapon=row.Melee?melee:ranged;friend.Tags=BondV3Catalog.Tags(row.Id);
                friend.Price=RulesV3Tuning.RecruitPrice;companions.Add(friend);
            }
            catalog.Companions=companions.ToArray();
            BondV3Catalog.Configure(catalog);
            var heroes=new List<UnitDefinition>(catalog.Champions??Array.Empty<UnitDefinition>());
            foreach(HeroRuleV3Definition definition in HeroRulesV3Catalog.Definitions)
            {
                UnitDefinition hero=null;foreach(UnitDefinition candidate in heroes)if(candidate.Id==definition.Id){hero=candidate;break;}
                if(hero==null)
                {
                    hero=ScriptableObject.CreateInstance<UnitDefinition>();hero.hideFlags=HideFlags.DontSave;
                    hero.Id=definition.Id;hero.DisplayName=definition.Name;hero.Weapon=catalog.Weapon(definition.WeaponId)??ranged;
                    hero.Health=MortalityTuning.DefaultMaxHealth;heroes.Add(hero);
                }
                hero.Tags=HeroRulesV3Catalog.MinorIds(hero.Id);
                hero.ReferenceHero=string.Empty;
                // Build directions describe all houses; recruitment is not restricted by obsolete rival exclusions.
                hero.RecruitThemes=AllMajorIds();
            }
            catalog.Champions=heroes.ToArray();
            var items=new List<ItemDefinition>(catalog.Items??Array.Empty<ItemDefinition>());
            foreach(HeroRuleV3Definition hero in HeroRulesV3Catalog.Definitions)
                foreach(HeroRelicV3Definition relic in hero.Relics)
                {
                    if(catalog.Item(relic.Id)!=null)continue;
                    items.Add(new ItemDefinition { Id=relic.Id,Name=relic.Name,Description=relic.Description,
                        Price=RulesV3Tuning.RelicPrice,ShopWeight=RulesV3Tuning.RelicWeight,Value=0,
                        Rarity=ItemRarity.Rare,Stacking=StackPolicy.Unique,Pools=ShopPool.Gold });
                }
            catalog.Items=items.ToArray();
            EnemyRulesV3Catalog.Configure(catalog);
        }
        private static string[] AllMajorIds()
        { var result=new string[BondTuning.MajorCount];for(int i=0;i<result.Length;i++)result[i]=BondTuning.V3Definitions[i].Id;return result; }
    }
}
