namespace ArcaneBrigade
{
    /// <summary>Plain tooltip rules for the Bizarre Arsenal, with the numbers ArsenalCombat actually uses.</summary>
    public static class ArsenalDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier)
        {
            tier = UnityEngine.Mathf.Clamp(tier, 1, 3);
            string rules = weapon.Arsenal switch
            {
                ArsenalWeapon.CherryFuseBomb => "Lobs a bomb at the target. It fizzes 0.45 s, then bursts for 160% damage in a small area." + (tier >= 2 ? " Tier 2+: two bombs." : "") + (tier >= 3 ? " Tier 3: the burst throws 3 bomblets." : ""),
                ArsenalWeapon.PopperSticks => "Lobs a bundle of firecrackers slowly on a high arc. It lands by the target and bursts for 170% in a wide area, pushing enemies back.",
                ArsenalWeapon.DreamRacket => "Serves a ball that bounces to up to 4 enemies, 85% damage, 10% less each bounce.",
                ArsenalWeapon.BambooVase => "Throws a vase. It shatters and 4 bamboo shoots spring up in a line beyond it, 70% each, briefly stopping enemies.",
                ArsenalWeapon.RoseBouquet => "Throws a bouquet (60%). Its petals stay on the path for 2 s, hurting and slowing enemies in them.",
                ArsenalWeapon.HomeRunBat => "A full swing clips everything in its arc (70%); the nearest enemy takes 120% and is batted away, hitting others for 80%. Bosses only take the hit.",
                ArsenalWeapon.CandyDrill => "Pierces up to 4 enemies, then drills into the floor for 1 s, hurting enemies on it.",
                ArsenalWeapon.InkyOctopus => "Throws an octopus that clings to an enemy for 3 s (25% every 0.4 s) and inks the floor to slow others. Attacking while it clings squeezes for 40%.",
                ArsenalWeapon.GrabberClaw => "Grabs the first enemy (100%), reels it in and flings it sideways into others (60%). Bosses only take the hit.",
                ArsenalWeapon.StarforgeClaymore => "Very slow, huge sweep in front for 190%. While swinging, the blade destroys enemy shots it touches.",
                ArsenalWeapon.DragonLanternGlaive => "Each attack sweeps in front (75%) and ends in a long thrust through the whole line (60%).",
                ArsenalWeapon.TwinMoonStaff => "Throws a two-bladed staff. It hovers at the end for 0.5 s grinding enemies (30% every 0.15 s), then flies back through them.",
                ArsenalWeapon.PlushShark => "Throws a shark at the farthest enemy. It bites up to 3 enemies on the way (110%) and swims back.",
                ArsenalWeapon.DrumstickClub => "Slams the floor ahead (130%); a shock runs 3 steps forward (50% each) and two crumbs fly off (25%).",
                ArsenalWeapon.PaperShuriken => "Throws 3 paper stars (55% each). Each bounces to 2 more enemies.",
                ArsenalWeapon.SquirtTrio => "Fires 3 water jets (50% each) that bounce off walls twice and slow what they hit.",
                ArsenalWeapon.ConfettiBlunderbuss => "Fires 7 candy pellets in a short cone, 26% each. Up close, most of them land.",
                ArsenalWeapon.PlungerPopper => "Sticks to an enemy, pulls it closer, then pops it off for 110%, knocking it into its neighbours (55%) and pushing it away.",
                ArsenalWeapon.SignpostSlammer => "Swings a road sign and plants it where it lands (one at a time): enemies crossing its strip are slowed for 2 s.",
                ArsenalWeapon.DetonatorBox => "Each attack sticks a toy bomb on the target; a second later it goes off for 120% in a small area (up to 4 at once).",
                ArsenalWeapon.FirecrackerPinwheel => "Plants a pinwheel by the target (up to 2). When an enemy comes close it sprays 6 sparks, then bursts for 90%.",
                ArsenalWeapon.GumballGatling => "Rapid gumballs. Each shot heats the barrel and fires sooner, up to 40% faster; it cools after 1.2 s without a target.",
                ArsenalWeapon.BalloonBuddy => "Releases a balloon that floats into the biggest crowd and pops: 60%, pushes and slows.",
                ArsenalWeapon.GlitchToybox => "Each attack borrows a random simple weapon and uses its attack at 90%. The next one is shown above the box.",
                ArsenalWeapon.SaucerBlaster => "Fires a toy saucer (70%) that pierces up to 4 enemies, trailing rings.",
                ArsenalWeapon.FanfareHorn => "Blows 3 golden sound fins forward in a fan (45% each); each enemy is struck once per blast.",
                ArsenalWeapon.WindupWrench => "Swings a wrench and winds up the enemy it hits (one key per enemy): a second later it spins dizzy, hitting its neighbours for 50%.",
                ArsenalWeapon.CorkRifle => "Slow, long shot (170%) that passes through every enemy in the line.",
                ArsenalWeapon.CatcherMitt => "Catches an enemy shot in front every 0.6 s (holds 3). Each throw is a fastball (130%), then every caught shot thrown back (80%).",
                ArsenalWeapon.FizzBottle => "Throws a soda bottle; it bursts into a fizzing puddle for 2.5 s that hurts and slows.",
                ArsenalWeapon.LeekBaton => "A wide, quick swing (80%) that pushes enemies back and slows them.",
                ArsenalWeapon.FlySwatter => "Swats flat (110%) and dazes; enemies below 30% health take 60% more.",
                ArsenalWeapon.DowsingRod => "Throws a twig at the weakest enemy in reach (120%); it bends toward it and exposes it for 2 s.",
                ArsenalWeapon.StarKnuckle => "Quick jabs (90%), each flicking a star shard forward (40%).",
                ArsenalWeapon.BuzzerBlock => "Places a prank buzzer by the target (up to 2). The enemy who steps on it and 3 neighbours in a chain are shocked (60%) and stopped briefly.",
                ArsenalWeapon.PawMitten => "Very quick, very short paw swipes (90%) that drag three claw lines a little further (12% each).",
                ArsenalWeapon.HearthBrick => "Lobs a brick at the target. It thuds down for 130% in a small area, dazing and shoving." + (tier >= 2 ? " Tier 2+: it skips on once more for 60% of that." : ""),
                ArsenalWeapon.VerdictGavel => "Bangs a verdict down on the enemy in front (small area). +25% per point of luck (up to 2) and up to +50% for the coins gathered this hall.",
                ArsenalWeapon.TalismanIngot => "Throws charm-sealed gold ingots; each hits, then pops in a small blast for the same again.",
                ArsenalWeapon.KunaiFan => "Throws its three kunai one after another down the same line, 60% each; each passes through one enemy.",
                ArsenalWeapon.BareKnuckles => "No weapon at all: very fast short punches. A critical punch also pops the air just ahead for 120% in a small area.",
                ArsenalWeapon.FortuneGlobe => "Three crystal motes wander out (45% each, each through two enemies). They sway as they fly and grow larger, then harder: +30% after 0.5 s and again after 1 s.",
                ArsenalWeapon.TopiaryShears => "Two quick snips: one down a short line ahead (60%), the next aimed at the toughest enemy in reach (70%).",
                ArsenalWeapon.PaperLanternBomb => "A paper lantern bomb floats over slowly (1 s), glows, then bursts for 190% in the widest area, pushing enemies back.",
                ArsenalWeapon.SnowGlobeGrenade => "A snow globe shatters where it lands: 130% in a small area, slowing what it hits for 1.5 s.",
                ArsenalWeapon.PinataBomb => "A pinata bursts by the target (100%) and scatters five candy bomblets that hop out and pop (35% each).",
                ArsenalWeapon.AcornCluster => "Three tied acorns split in the air and land in a triangle round the target, each popping for 70%.",
                _ => weapon.GameplaySummary
            };
            return (string.IsNullOrEmpty(weapon.GameplaySummary) ? "" : weapon.GameplaySummary + "\n") + rules + "\nNothing it does comes from the wielder's own body: every hit is thrown, fired, planted or swung in front.";
        }
    }
}
