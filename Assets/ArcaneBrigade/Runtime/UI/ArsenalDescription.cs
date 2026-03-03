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
                ArsenalWeapon.CherryFuseBomb => "Lobs a bomb at the target. It fizzes 0.8 s, then bursts for 160% damage in a small area." + (tier >= 2 ? " Tier 2+: two bombs." : "") + (tier >= 3 ? " Tier 3: the burst throws 3 bomblets." : ""),
                ArsenalWeapon.PopperSticks => "Throws 3 firecracker sticks ahead. Each pops for 70% when an enemy touches it or after 2 s, setting off sticks nearby.",
                ArsenalWeapon.DreamRacket => "Serves a ball that bounces to up to 4 enemies, 85% damage, 10% less each bounce.",
                ArsenalWeapon.BambooVase => "Throws a vase. It shatters and 4 bamboo shoots spring up in a line beyond it, 55% each, briefly stopping enemies.",
                ArsenalWeapon.RoseBouquet => "Throws a bouquet (60%). Its petals stay on the path for 2 s, hurting and slowing enemies in them.",
                ArsenalWeapon.HomeRunBat => "Hits the nearest enemy for 120% and bats it away. The flying enemy hits others for 80%. Bosses only take the hit.",
                ArsenalWeapon.CandyDrill => "Pierces up to 4 enemies, then drills into the floor for 1 s, hurting enemies on it.",
                ArsenalWeapon.InkyOctopus => "Throws an octopus that clings to an enemy for 3 s (25% every 0.4 s) and inks the floor to slow others. Attacking while it clings squeezes for 40%.",
                ArsenalWeapon.GrabberClaw => "Grabs the first enemy (100%), reels it in and flings it sideways into others (60%). Bosses only take the hit.",
                ArsenalWeapon.StarforgeClaymore => "Very slow, huge sweep in front for 190%. While swinging, the blade destroys enemy shots it touches.",
                ArsenalWeapon.DragonLanternGlaive => "Sweep, sweep again, then a long thrust that pierces the whole line for 140%.",
                ArsenalWeapon.TwinMoonStaff => "Throws a two-bladed staff. It hovers at the end for 0.5 s grinding enemies (30% every 0.15 s), then flies back through them.",
                ArsenalWeapon.PlushShark => "Throws a shark at the farthest enemy. It bites up to 3 enemies on the way (110%) and swims back.",
                ArsenalWeapon.DrumstickClub => "Slams the floor ahead (130%); a shock runs 3 steps forward (50% each). Every third slam sprays crumbs.",
                ArsenalWeapon.PaperShuriken => "Throws 3 paper stars (55% each). Each bounces to 2 more enemies.",
                ArsenalWeapon.SquirtTrio => "Fires 3 water jets (50% each) that bounce off walls twice and slow what they hit.",
                ArsenalWeapon.ConfettiBlunderbuss => "Fires 7 candy pellets in a short cone, 26% each. Up close, most of them land.",
                ArsenalWeapon.PlungerPopper => "Sticks to an enemy, pulls it closer, then pops it off for 110% and pushes it away.",
                ArsenalWeapon.SignpostSlammer => "Swings a road sign. Every third swing plants it: enemies crossing its strip are slowed for 3 s.",
                ArsenalWeapon.DetonatorBox => "Each attack plants a toy bomb under the target. The third attack sets all of them off, 120% each.",
                ArsenalWeapon.FirecrackerPinwheel => "Plants a pinwheel ahead (up to 2). When an enemy comes close it sprays 6 sparks, then bursts for 90%.",
                ArsenalWeapon.GumballGatling => "Rapid gumballs. Each shot heats the barrel and fires sooner, up to 40% faster; it cools after 1.2 s without a target.",
                ArsenalWeapon.BalloonBuddy => "Releases a balloon that floats into the biggest crowd and pops: 60%, pushes and slows.",
                ArsenalWeapon.GlitchToybox => "Each attack borrows a random simple weapon and uses its attack at 90%. The next one is shown above the box.",
                ArsenalWeapon.SaucerBlaster => "Fires a toy saucer (70%) that pierces up to 4 enemies, trailing rings.",
                ArsenalWeapon.FanfareHorn => "Blows 3 golden sound fins forward in a fan (45% each); each enemy is struck once per blast.",
                ArsenalWeapon.WindupWrench => "Swings a wrench. Every third hit winds the enemy up: a second later it spins dizzy, hitting its neighbours for 50%.",
                ArsenalWeapon.CorkRifle => "Slow, long shot (170%) that passes through every enemy in the line.",
                ArsenalWeapon.CatcherMitt => "Catches an enemy shot in front every 0.6 s (holds 3). Each throw is a fastball (130%), then every caught shot thrown back (80%).",
                ArsenalWeapon.FizzBottle => "Throws a soda bottle; it bursts into a fizzing puddle for 2.5 s that hurts and slows.",
                ArsenalWeapon.LeekBaton => "A wide, quick swing (80%) that pushes enemies back and slows them.",
                ArsenalWeapon.FlySwatter => "Swats flat (110%) and dazes. Every third swat deals double damage to enemies below 30% health.",
                _ => weapon.GameplaySummary
            };
            return (string.IsNullOrEmpty(weapon.GameplaySummary) ? "" : weapon.GameplaySummary + "\n") + rules + "\nNothing it does comes from the wielder's own body: every hit is thrown, fired, planted or swung in front.";
        }
    }
}
