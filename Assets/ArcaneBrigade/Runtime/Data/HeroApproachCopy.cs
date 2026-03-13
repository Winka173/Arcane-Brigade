using System;

namespace ArcaneBrigade
{
    // Percentages describe a total damage budget, never damage per enemy in an area.
    public static class HeroApproachCopy
    {
        public const string DamageRules = "DAMAGE GUIDE: W means your prepared weapon damage, with a minimum of 1 for this mechanic. Extra area attacks share their stated total among at most 3 targets per shape; they do not deal that total to every target. Areas are capped (circles 1.6, rings 1.8, sweeps 2.4, lines 4.5 units) and the painted area is exactly the area that hits. Basic attacks and normal damage modifiers remain separate. Extra hits cannot recursively trigger this mechanic. Bosses ignore push/pull and this mechanic's explicit slow lasts at most 0.3s. Healing follows your hero's normal restrictions.";
        /// <summary>The three approaches of a hero's transformation (18/10/2026): focused, spread wide, or lasting.</summary>
        public static string[] For(string id)
        {
            HeroKit kit = HeroKits.For(id);
            if (kit == null) return Legacy(id);
            string name = kit.Transformation;
            return new[] {
                name + " focuses: every strike of the transformation deals 15% more, shared among at most 3 targets. Best against bosses and large enemies.",
                name + " spreads: the same strikes reach up to 5 targets each at their usual strength. Best against crowds and swarms.",
                name + " lasts 1.5s longer and heals you 1 HP every 2s while it runs; its strikes deal 15% less. Best when you need to hold on." };
        }
        /// <summary>The approaches as they read before hero kits (kept for heroes without a kit).</summary>
        private static string[] Legacy(string id) => id switch
        {
            "LanternKnight" => new[] {
                "At 8 sparks, up to 3 friends share 180% W in weapon-shaped responses, followed by a 60% W line toward the target. 4s cooldown.",
                "At 8 sparks, up to 4 friends share 180% W in weapon-shaped responses, followed by a 60% W forward crescent. 4s cooldown.",
                "At 8 sparks, up to 3 friends share 180% W in responses. Throw 3 lantern embers at the target sharing 35% W and remove one hostile shot within 2 units. 4s cooldown." },
            "Postmaster" => new[] {
                "Every third personal attack sends one homing parcel per address. Parcels share 160% W total physical damage. Up to 3 addresses; 2.5s cooldown.",
                "The same delivery uses ember parcels instead of physical parcels, sharing 160% W across all addresses. 2.5s cooldown.",
                "Replace parcels with delayed line segments from you through the marked targets. Segments share 160% W total. 2.5s cooldown." },
            "DrowsySeer" => new[] {
                "When 4 dream sparks are full, the dream stays where you pause for 0.5s (or by itself after 3s) and aims an arcane line at a target within 6 units every 0.7s for 4s. Each strike has an 80% W budget.",
                "Your waiting dream uses a forward arcane crescent instead of a line. It lasts 4s, striking every 0.7s for an 80% W budget.",
                "Your waiting dream uses two crossing frost lines at its target. They share 80% W per strike, every 0.7s for 4s. Frost interactions remain separate." },
            "RootOracle" => new[] {
                "Every 3s, the next root catches one hostile shot within 2 units and makes a radius-1.3 disc with an 80% W budget. Ordinary enemies hit are slowed for 0.7s.",
                "Every 3s, the next root draws a line toward a threat within 4 units, with a 150% W budget and 0.7s slow on ordinary enemies. No shot catch.",
                "Every 3s, the next root attempts to heal you for 2 HP and makes a radius-1.3 disc with a 130% W budget and 0.7s ordinary-enemy slow." },
            "LoneJelly" => new[] {
                "Every third attack uses the next memory's element. A projectile memory fires 3 shots; other memories make a forward crescent. Total budget 150% W; 2.4s cooldown.",
                "Every third attack expresses the next memory as 3 shots, whatever its original weapon kind. They share 150% W and keep the memory's element. 2.4s cooldown.",
                "Every third attack uses a line for a non-projectile memory, or 3 shots for a projectile memory. Total budget 150% W; memory element retained; 2.4s cooldown." },
            "GlassDice" => new[] {
                "Face 1: 3 darts share 200% W. Face 2: crossed lines share 240% W. Face 3: radius-1.5 frost disc, 200% W and 1s ordinary slow. Face 4: +1 gold and a 120% W line. Face 5: heal 2 HP and a 120% W line. Face 6: 320% W line. Six collected gold per cast; 4s cooldown.",
                "Faces 1-3 keep their dart/cross/frost effects. Face 4 grants 1 gold and a 120% W crescent; face 5 heals 2 HP and a 120% W crescent; face 6 makes a 320% W crescent. Six collected gold; 4s cooldown.",
                "Faces 1-3 keep their dart/cross/frost effects. Face 4 grants 1 gold plus crossing lines sharing 120% W; face 5 heals 2 HP plus the same lines; face 6 crosses for 320% W total. Six collected gold; 4s cooldown." },
            "TideSinger" => new[] {
                "Three coin pickups release 2 frost notes, plus one per 2 drinks bought, up to 5 notes. Notes share 220% W. 4s cooldown; buying drinks does not count as collecting coins.",
                "Three coin pickups attempt to heal 3 HP and roll a 3-unit tide line toward the target with a 160% W budget. Ordinary enemies are slowed 0.6s and gently pushed. 4s cooldown.",
                "Three coin pickups create a radius-2.2 forward crescent with a 220% W budget, 0.5s ordinary slow and capped push. 4s cooldown." },
            "CometRunner" => new[] {
                "Travel 10 units to ignite recent path segments as ember lines. All segments share 230% W, with staggered short warnings. 3s cooldown.",
                "Travel 10 units to draw two parallel ember tracks, offset 0.4 units on each side. Both tracks together share 230% W. 3s cooldown.",
                "Travel 10 units to leave radius-1.1 physical rings at recent path points. All rings share 160% W; unlike Trail, these rings are not ember attacks. 3s cooldown." },
            "MirrorKeeper" => new[] {
                "The mirror catches a hostile shot that comes within 1 unit (2 units after you stand still for 0.5s), one every 2s, up to 3. Your next basic attack, or 2s of waiting, returns them toward the target in a 15-degree fan, 60% W each, keeping the caught element.",
                "Caught shots are returned opposite their original travel direction, 60% W each, keeping the caught element. Same 1-unit (2 when still) catch and 2s catch cooldown.",
                "Caught shots are returned toward the target in a wide fan (up to 24 degrees apart), 60% W each, keeping the caught element. Same catch rules; 1s release cooldown." },
            "TreasureAcorn" => new[] {
                "Prefer the nearest bounty. Settling 3 stamps or its kill pays 1 gold, at most 8 payouts per wave, and makes an 85% W line. 3s cooldown.",
                "Prefer a ranged enemy as the next bounty. Settlement pays the same capped gold and makes an 85% W forward crescent. 3s cooldown.",
                "Favor a high-health bounty in the target score. Settlement pays the same capped gold and makes crossing lines sharing 85% W. 3s cooldown." },
            "VerdantMatron" => new[] {
                "Nursery evolution favors healers or HP/armor support. Every 6s, bloom at a sprout, or at you if none: catch one hostile shot within 2 units and strike a radius-1.6 disc for a 140% W budget with 0.6s ordinary slow.",
                "Nursery evolution favors melee-weapon friends. Every 6s, bloom at a sprout, or you if none, with a 300% W radius-1.6 disc and 0.6s ordinary slow.",
                "Nursery evolution favors passive/support friends. Every 6s, bloom with a 220% W radius-1.6 disc and 0.6s ordinary slow; attempt to heal 3 HP with a sprout, otherwise 2 HP." },
            "CopperCauldron" => new[] {
                "At 6 steam, catch one hostile shot within 3 units and vent an ember line for a 180% W budget. 3s cooldown.",
                "At 6 steam, vent a forward ember crescent for a 250% W budget. Wider coverage instead of the shot catch. 3s cooldown.",
                "At 6 steam, vent an ember line for a 250% W budget. It uses the same line reach as Lid and does not catch a shot. 3s cooldown." },
            "ChinaMarionette" => new[] {
                "A personal attack spends one porcelain shard on a delayed line for a 110% W budget. 1s cooldown; each ordinary injury or each 6 actual HP recovered creates one shard, up to 3.",
                "Spend a shard on a delayed forward crescent for a 110% W budget. 1s cooldown; shards grow from injuries and from actual recovery, not attempted overhealing.",
                "Spend a shard on two crossing lines sharing 110% W and attempt to heal 1 HP. This healing cannot grow another shard. 1s cooldown." },
            "SoapSkater" => new[] {
                "A qualifying turn leaves a line bubble that pops after 0.65s. Budget 70% W, or 180% W on every third turn; 0.7s ordinary slow and gentle push. At most one turn counted per 1.2s.",
                "A qualifying turn leaves a forward crescent bubble, radius 0.9 or 1.4 on every third turn. Budget 70%/180% W after 0.65s, with 0.7s ordinary slow and gentle push.",
                "A qualifying turn splashes a disc bubble ahead of you, radius 0.9 or 1.4 on every third turn. Budget 70%/180% W after 0.65s, with 0.7s ordinary slow and gentle push." },
            "VelvetRegent" => new[] {
                "After 6 allied basic attacks, up to 3 friends answer with their own weapon shapes. Favor nearby targets. 4s cooldown; each performer adds 50% of its weapon damage, capped at 75% W.",
                "The same 6-attack command favors ranged enemies and shifts companion targeting toward ranged threats. Up to 3 performers, 50% own weapon each capped at 75% W; 4s cooldown.",
                "The same command favors high-health enemies and shifts companion target priority toward them. Up to 3 performers; 4s cooldown. Each performer's budget contribution is 50% of its weapon damage, capped at 75% W." },
            "ButtonCavalier" => new[] {
                "Poise: +1 for a near miss, +2 for a dodge, +1 when an ordinary blow lands on you. At 3 poise, your next attack triggers a delayed line for a 260% W budget against a threat within 5 units. 2s cooldown.",
                "At 3 poise, your next attack triggers a forward crescent for a 260% W budget. Guard sweep does not grant an additional projectile block. 2s cooldown.",
                "At 3 poise, your next attack triggers crossed lines sharing a 260% W budget. 2s cooldown; each near-miss bullet grants poise only once." },
            "NightMaw" => new[] {
                "Three non-summoned kills release a line bite for a 120% W budget if the nearest threat is within 3.5 units. 2.5s cooldown.",
                "The same meal count releases a forward crescent instead of a line, sharing 120% W. Requires a threat within 3.5 units; 2.5s cooldown.",
                "The crescent originates one unit behind your facing position and aims at the threat. Budget 120% W; 2.5s cooldown. No instant boss execution." },
            "ThornTreasurer" => new[] {
                "An ordinary injury spends one collateral thorn to draw a line with a 130% W budget toward a threat within 6 units. Every 4 basic attacks regrows one thorn, up to 4; a full crown spends one by itself on an enemy within 2.5 units (3s).",
                "Spend a thorn on a forward crescent with a 130% W budget. Crown is a forward attack, not a persistent shield. Every 4 basic attacks regrows one thorn.",
                "Spend a thorn on two crossing lines sharing a 130% W budget. Gold is not spent on retaliation; gold only sets starting thorns." },
            "CitrusMarshal" => new[] {
                "Friends follow two close lanes behind your heading. Travel 8 units to leave physical path lines sharing 270% W with 0.5s ordinary slow. 3s cooldown.",
                "Friends follow wider lanes, 1.5 units to either side. Travel 8 units to leave parallel physical paths sharing 270% W with 0.5s ordinary slow. 3s cooldown.",
                "Friends arrange in a close circle. Travel 8 units to leave radius-1.1 rings at path points, sharing 220% W. These rings do not apply the other approaches' slow. 3s cooldown." },
            "IvorySovereign" => new[] {
                "An ordinary injury or every third personal basic attack prepares a delayed verdict line with a 100% W budget. Shared 1s cooldown.",
                "The decree is a radius-1.5 filled seal at the target, sharing a 100% W budget. Shared 1s cooldown; deliberate health costs do not write decrees.",
                "The decree is two crossing lines at the target sharing 100% W. The second line is delayed longer. Shared 1s cooldown." },
            "PetalCitadel" => new[] {
                "Every fourth attack places 4 staggered petal discs around the target. Total budget is W times (1.5 + min(0.9, 0.08 times total petal levels)). Each petal's radius grows from 0.65 by 0.1 per level (3 levels at most). 3s cooldown.",
                "The same bloom becomes 4 lines from you toward the petal points. Total damage budget is unchanged; each petal level still contributes to growth. 3s cooldown.",
                "Keep the petal discs; if a guard petal exists, also catch one hostile shot within 2.5 units. Every approach heals 1-3 HP when a healing petal exists and adds a 50% W outer ring of radius 1.5 when all 4 petal groups exist." },
            "PatchworkHost" => new[] {
                "After 4 lead attacks/contact hits, imitate its weapon shape from your position. Budget is 110% of the lead's weapon damage, capped at 220% W. A healing lead also attempts 2 HP recovery. 3s cooldown.",
                "Split the same imitation budget equally: one response from the lead and one from you. Weapon element and shape are inherited. Healing-lead recovery remains 2 HP total. 3s cooldown.",
                "Replace the imitation with a radius-2.2 physical crescent from you, with the same lead-based capped budget. Healing-lead recovery remains 2 HP. 3s cooldown." },
            "ArchiveCurator" => new[] {
                "Keep up to 3 species. A newly hit species replaces the next old specimen when full. Every 6s replay the shelf, sharing 300% W across its specimens: ranged volleys, charge lines or close crescents.",
                "Keep the first 3 distinct species encountered; later species do not replace them. Replay every 6s with the same 300% W total budget.",
                "Allow replacements and clear the shelf after each replay, so you must hit species again. Replay budget remains 300% W, with a 6s cooldown." },
            "PennyBroker" => new[] {
                "Invest in an eligible CURIO for the next shop. The deposit is a separate cost, not deducted from its quoted purchase price. The order survives rerolls. Combat receipt: every 10 collected gold makes a 145% W radius-0.8 seal, 3s cooldown.",
                "Reserve an eligible DRINK with the same deposit and quoted-price rules. No drink is counted until you actually buy the delivered item. Combat receipt is unchanged.",
                "Reserve an eligible FOOD with the same deposit and quoted-price rules. No food is counted until you buy it. If stack limits make delivery unavailable, refund the deposit; normal purchase gives no deposit refund." },
            "StormFencer" => new[] {
                "At 3 static, make a delayed arcane line with a 280% W budget. 3s cooldown. A threat within 6 units builds 0.4 static/s; near misses add 1 and dodges add 2.",
                "At 3 static, make a forward arcane crescent with a 280% W budget. This is a close sweep, not an extra normal basic attack. 3s cooldown.",
                "At 3 static, make crossing arcane lines sharing 280% W. 3s cooldown; bosses are not stunned." },
            "TinVirtuoso" => new[] {
                "A 2-beat phrase guarantees one friend's response; crits count as 2 beats. Response uses the friend's weapon shape, with a 70% own-weapon budget capped at twice W. 1.5s cooldown.",
                "A 4-beat phrase calls up to 2 friends. Each contributes 70% own-weapon damage, capped at twice W; total is shared by weapon-damage weights. 1.5s cooldown.",
                "A 6-beat phrase calls up to 4 friends using the same weighted, capped damage-budget rule. Fewer friends means fewer performers, not duplicate phantom bodies. 1.5s cooldown." },
            "MoonProwler" => new[] {
                "Travel 5 units: your shadow waits behind the nearest enemy. Your next basic attack sends it out of that target's far side as a delayed 80% W line back through it. If you are truly behind the target (against its facing), the basic also gains +45%. Flank-kill gold is capped at 6 per wave.",
                "The same shadow step; the 80% W budget becomes a crescent swung from the far side. A real rear attack still adds +45% to the basic.",
                "The same shadow step; the 80% W budget splits between two crossing lines at the target. A real rear attack still adds +45% to the basic." },
            "SupperMortar" => new[] {
                "Each basic is served as 2 physical dumpling bursts landing 0.4s and 0.5s later where the target is heading (up to 1.2 units ahead), each carrying 85% of its damage. Radius 1.05 plus 0.025 per food bought, capped at +0.35. Each burst shares damage across at most 3 enemies.",
                "Replace both bursts with vertical ember pepper lines, each carrying 110% of the basic damage (lines are narrow) at the same delays. This line recipe does not use the food-based radius bonus.",
                "Use the food-scaled bursts with frost sorbet damage and 1s ordinary slow, at the same delays and 85% shares. No projectile piercing." },
            "LanternWeaver" => new[] {
                "Every fourth attack closes lines between up to 3 live knots. All links share 380% W and apply 0.3s slow. 2.4s cooldown; knots are consumed.",
                "Start the stitch at your position and connect through the knots. Links share 340% W, slow for 0.3s and gently pull ordinary targets toward each segment's origin. 2.4s cooldown.",
                "Replace links with radius-1.4 crescents from each knot toward you. Crescents share 360% W and slow for 0.3s; no pull. 2.4s cooldown." },
            "HourglassStag" => new[] {
                "35% of a personal basic that does not finish its target is deferred, up to 3 W. 4s after the first entry the debt is collected from that enemy (or the nearest) as a radius-0.6 strike worth 320% of the bank.",
                "Settle after 2.5s through a radius-1.4 disc at the debtor worth 260% of the bank, shared across at most 3 targets.",
                "Settle after 5s through 3 homing frost shots at the debtor sharing 320% of the bank. Frost interactions remain separate; the payout cannot refill its own bank." },
            "RainArchivist" => new[] {
                "Three pages release 3 frost shots plus one per chest page, up to 6, spaced 14 degrees apart. Total budget 450% W; recovery pages allow healing min(4, recovery pages + 1) HP. Every third personal attack writes a page. 3s cooldown.",
                "Add one frost shot to the same page formula, still at most 6. Keep the 450% W total and 14-degree spacing. Healing requires a recovery page. 3s cooldown.",
                "Spread frost shots 24 degrees apart. Always attempt at least 1 HP recovery, or more with recovery pages up to 4. Total budget 450% W; its own recovery cannot write pages. 3s cooldown." },
            _ => Array.Empty<string>()
        };
    }
}
