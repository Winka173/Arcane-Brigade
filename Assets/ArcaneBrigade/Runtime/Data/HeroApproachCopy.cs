using System;

namespace ArcaneBrigade
{
    // Percentages describe a total damage budget, never damage per enemy in an area.
    public static class HeroApproachCopy
    {
        public const string DamageRules = "DAMAGE GUIDE: W means your prepared weapon damage, with a minimum of 1 for this mechanic. Extra area attacks share their stated total among at most 3 targets per shape; they do not deal that total to every target. Basic attacks and normal damage modifiers remain separate. Extra hits cannot recursively trigger this mechanic. Bosses ignore push/pull and this mechanic's explicit slow lasts at most 0.3s. Healing follows your hero's normal restrictions.";
        public static string[] For(string id) => id switch
        {
            "LanternKnight" => new[] {
                "At 8 sparks, up to 3 friends share 110% W in weapon-shaped responses, followed by a 35% W line toward the target. 4s cooldown.",
                "At 8 sparks, up to 4 friends share 110% W in weapon-shaped responses, followed by a 35% W forward crescent. 4s cooldown.",
                "At 8 sparks, up to 3 friends share 110% W in responses. Add a 35% W ring of radius 2.2 around you and remove one hostile shot within 2 units. 4s cooldown." },
            "Postmaster" => new[] {
                "Every third personal attack sends one homing parcel per address. Parcels share 90% W total physical damage. Up to 3 addresses; 2.5s cooldown.",
                "The same delivery uses ember parcels instead of physical parcels, sharing 90% W across all addresses. 2.5s cooldown.",
                "Replace parcels with delayed line segments from you through the marked targets. Segments share 90% W total. 2.5s cooldown." },
            "DrowsySeer" => new[] {
                "A dream stays at the place where you rested for 4s and aims an arcane line at a target within 6 units every 0.85s. Each strike has a 30% W budget.",
                "Your stationary dream uses a forward arcane crescent instead of a line. It lasts 4s, striking every 0.85s for a 30% W budget.",
                "Your stationary dream uses two crossing frost lines at its target. They share 30% W per strike, every 0.85s for 4s. Frost interactions remain separate." },
            "RootOracle" => new[] {
                "Every 3s, the next root catches one hostile shot within 2 units and makes a radius-1.8 disc with a 50% W budget. Ordinary enemies hit are slowed for 0.7s.",
                "Every 3s, the next root draws a line toward a threat within 4 units, with an 85% W budget and 0.7s slow on ordinary enemies. No shot catch.",
                "Every 3s, the next root attempts to heal you for 1 HP and makes a radius-1.8 disc with a 50% W budget and 0.7s ordinary-enemy slow." },
            "LoneJelly" => new[] {
                "Every third attack uses the next memory's element. A projectile memory fires 3 shots; other memories make a forward crescent. Total budget 70% W; 2.4s cooldown.",
                "Every third attack expresses the next memory as 3 shots, whatever its original weapon kind. They share 70% W and keep the memory's element. 2.4s cooldown.",
                "Every third attack uses a line for a non-projectile memory, or 3 shots for a projectile memory. Total budget 70% W; memory element retained; 2.4s cooldown." },
            "GlassDice" => new[] {
                "Face 1: 3 darts share 100% W. Face 2: crossed lines share 120% W. Face 3: radius-1.8 frost disc, 100% W and 1s ordinary slow. Face 4: +1 gold and a 60% W line. Face 5: heal 2 HP and a 60% W line. Face 6: 160% W line. Six collected gold per cast; 4s cooldown.",
                "Faces 1-3 keep their dart/cross/frost effects. Face 4 grants 1 gold and a 60% W crescent; face 5 heals 2 HP and a 60% W crescent; face 6 makes a 160% W crescent. Six collected gold; 4s cooldown.",
                "Faces 1-3 keep their dart/cross/frost effects. Face 4 grants 1 gold plus crossing lines sharing 60% W; face 5 heals 2 HP plus the same lines; face 6 crosses for 160% W total. Six collected gold; 4s cooldown." },
            "TideSinger" => new[] {
                "Three coin pickups release 2 frost notes, plus one per 2 drinks bought, up to 5 notes. Notes share 90% W. 4s cooldown; buying drinks does not count as collecting coins.",
                "Three coin pickups attempt to heal 3 HP and make a radius-2 ring with a 45% W budget. Ordinary enemies are slowed 0.6s and gently pushed. 4s cooldown.",
                "Three coin pickups create a radius-3 forward crescent with a 90% W budget, 0.5s ordinary slow and capped push. 4s cooldown." },
            "CometRunner" => new[] {
                "Travel 10 units to ignite recent path segments as ember lines. All segments share 90% W, with staggered short warnings. 3s cooldown.",
                "Travel 10 units to draw two parallel ember tracks, offset 0.4 units on each side. Both tracks together share 90% W. 3s cooldown.",
                "Travel 10 units to leave radius-1.1 physical rings at recent path points. All rings share 90% W; unlike Trail, these rings are not ember attacks. 3s cooldown." },
            "MirrorKeeper" => new[] {
                "Release stored shots toward the current target in a 15-degree fan. All reflections together share 95% W and retain the caught elements. 1s release cooldown.",
                "Release each stored shot opposite its original travel direction. All reflections together share 95% W; caught elements retained. 1s release cooldown.",
                "Release stored shots at evenly spaced radial angles. All reflections together share 95% W; caught elements retained. 1s release cooldown." },
            "TreasureAcorn" => new[] {
                "Prefer the nearest bounty. Settling 3 stamps or its kill pays 1 gold, at most 8 payouts per wave, and makes a 65% W line. 3s cooldown.",
                "Prefer a ranged enemy as the next bounty. Settlement pays the same capped gold and makes a 65% W forward crescent. 3s cooldown.",
                "Favor a high-health bounty in the target score. Settlement pays the same capped gold and makes crossing lines sharing 65% W. 3s cooldown." },
            "VerdantMatron" => new[] {
                "Nursery evolution favors healers or HP/armor support. Every 6s, bloom at a sprout, or at you if none: catch one hostile shot within 2 units and strike a radius-2.5 disc for a 50% W budget with 0.6s ordinary slow.",
                "Nursery evolution favors melee-weapon friends. Every 6s, bloom at a sprout, or you if none, with a 100% W radius-2.5 disc and 0.6s ordinary slow.",
                "Nursery evolution favors passive/support friends. Every 6s, bloom with a 50% W radius-2.5 disc and 0.6s ordinary slow; attempt to heal 2 HP with a sprout, otherwise 1 HP." },
            "CopperCauldron" => new[] {
                "At 6 steam, catch one hostile shot within 3 units and vent an ember line for a 100% W budget. 3s cooldown.",
                "At 6 steam, vent a forward ember crescent for a 100% W budget. Wider coverage instead of the shot catch. 3s cooldown.",
                "At 6 steam, vent an ember line for a 100% W budget. It uses the same line reach as Lid and does not catch a shot. 3s cooldown." },
            "ChinaMarionette" => new[] {
                "A personal attack spends one porcelain shard on a delayed line for a 55% W budget. 1s cooldown; each 6 actual HP recovered creates one shard, up to 3.",
                "Spend a shard on a delayed forward crescent for a 55% W budget. 1s cooldown; shard growth uses actual recovery, not attempted overhealing.",
                "Spend a shard on two crossing lines sharing 55% W and attempt to heal 1 HP. This healing cannot grow another shard. 1s cooldown." },
            "SoapSkater" => new[] {
                "A qualifying turn leaves a line bubble that pops after 0.65s. Budget 30% W, or 80% W on every third turn; 0.7s ordinary slow and gentle push. At most one turn counted per 1.2s.",
                "A qualifying turn leaves a forward crescent bubble, radius 0.9 or 1.7 on every third turn. Budget 30%/80% W after 0.65s, with 0.7s ordinary slow and gentle push.",
                "A qualifying turn leaves a full disc bubble, radius 0.9 or 1.7 on every third turn. Budget 30%/80% W after 0.65s, with 0.7s ordinary slow and gentle push." },
            "VelvetRegent" => new[] {
                "After 6 allied basic attacks, up to 3 friends answer with their own weapon shapes. Favor nearby targets. 4s cooldown; response budget uses their weapon damage, capped per performer at twice W.",
                "The same 6-attack command favors ranged enemies and shifts companion targeting toward ranged threats. Up to 3 performers; 4s cooldown.",
                "The same command favors high-health enemies and shifts companion target priority toward them. Up to 3 performers; 4s cooldown. Each performer's budget contribution is 70% of its weapon damage, capped at twice W." },
            "ButtonCavalier" => new[] {
                "At 3 poise, your next attack triggers a delayed line for a 90% W budget against a threat within 5 units. 2s cooldown.",
                "At 3 poise, your next attack triggers a forward crescent for a 90% W budget. Guard sweep does not grant an additional projectile block. 2s cooldown.",
                "At 3 poise, your next attack triggers crossed lines sharing a 90% W budget. 2s cooldown; each near-miss bullet grants poise only once." },
            "NightMaw" => new[] {
                "Three non-summoned kills release a line bite for a 120% W budget if the nearest threat is within 3.5 units. 2.5s cooldown.",
                "The same meal count releases a forward crescent instead of a line, sharing 120% W. Requires a threat within 3.5 units; 2.5s cooldown.",
                "The crescent originates one unit behind your facing position and aims at the threat. Budget 120% W; 2.5s cooldown. No instant boss execution." },
            "ThornTreasurer" => new[] {
                "An ordinary injury spends one collateral thorn to draw a line with an 80% W budget toward a threat within 6 units. Every 6 basic attacks regrows one thorn, up to 4.",
                "Spend a thorn on a forward crescent with an 80% W budget. Crown is a forward attack, not a persistent shield. Every 6 basic attacks regrows one thorn.",
                "Spend a thorn on two crossing lines sharing an 80% W budget. Gold is not spent on retaliation; gold only sets starting thorns." },
            "CitrusMarshal" => new[] {
                "Friends follow two close lanes behind your heading. Travel 8 units to leave physical path lines sharing 90% W with 0.5s ordinary slow. 3s cooldown.",
                "Friends follow wider lanes, 1.5 units to either side. Travel 8 units to leave parallel physical paths sharing 90% W with 0.5s ordinary slow. 3s cooldown.",
                "Friends arrange in a close circle. Travel 8 units to leave radius-1.1 rings at path points, sharing 90% W. These rings do not apply the other approaches' slow. 3s cooldown." },
            "IvorySovereign" => new[] {
                "An ordinary injury or every third personal basic attack prepares a delayed verdict line with a 100% W budget. Shared 1s cooldown.",
                "The decree is a radius-1.5 filled seal at the target, sharing a 100% W budget. Shared 1s cooldown; deliberate health costs do not write decrees.",
                "The decree is two crossing lines at the target sharing 100% W. The second line is delayed longer. Shared 1s cooldown." },
            "PetalCitadel" => new[] {
                "Every fourth attack places 4 staggered petal discs around the target. Total budget is W times (0.65 + min(0.4, 0.035 times total petal levels)). Each petal's radius grows from 0.65 by 0.1 per level. 3s cooldown.",
                "The same bloom becomes 4 lines from you toward the petal points. Total damage budget is unchanged; each petal level still contributes to growth. 3s cooldown.",
                "Keep the petal discs; if a guard petal exists, also catch one hostile shot within 2.5 units. Every approach heals 1-3 HP when a healing petal exists and adds a 25% W outer ring when all 4 petal groups exist." },
            "PatchworkHost" => new[] {
                "After 4 lead attacks/contact hits, imitate its weapon shape from your position. Budget is 80% of the lead's weapon damage, capped at 160% W. A healing lead also attempts 2 HP recovery. 3s cooldown.",
                "Split the same imitation budget equally: one response from the lead and one from you. Weapon element and shape are inherited. Healing-lead recovery remains 2 HP total. 3s cooldown.",
                "Replace the imitation with a radius-3.4 physical crescent from you, with the same lead-based capped budget. Healing-lead recovery remains 2 HP. 3s cooldown." },
            "ArchiveCurator" => new[] {
                "Keep up to 3 species. A newly hit species replaces the next old specimen when full. Every 6s replay the shelf, sharing 110% W across its specimens: ranged volleys, charge lines or close crescents.",
                "Keep the first 3 distinct species encountered; later species do not replace them. Replay every 6s with the same 110% W total budget.",
                "Allow replacements and clear the shelf after each replay, so you must hit species again. Replay budget remains 110% W, with a 6s cooldown." },
            "PennyBroker" => new[] {
                "Invest in an eligible CURIO for the next shop. The deposit is a separate cost, not deducted from its quoted purchase price. The order survives rerolls. Combat receipt: every 10 collected gold makes an 80% W radius-0.8 seal, 3s cooldown.",
                "Reserve an eligible DRINK with the same deposit and quoted-price rules. No drink is counted until you actually buy the delivered item. Combat receipt is unchanged.",
                "Reserve an eligible FOOD with the same deposit and quoted-price rules. No food is counted until you buy it. If stack limits make delivery unavailable, refund the deposit; normal purchase gives no deposit refund." },
            "StormFencer" => new[] {
                "At 3 static, make a delayed arcane line with a 160% W budget. 3s cooldown. A threat within 6 units builds 0.4 static/s; near misses add 1 and dodges add 2.",
                "At 3 static, make a forward arcane crescent with a 160% W budget. This is a close sweep, not an extra normal basic attack. 3s cooldown.",
                "At 3 static, make crossing arcane lines sharing 160% W. 3s cooldown; bosses are not stunned." },
            "TinVirtuoso" => new[] {
                "A 2-beat phrase guarantees one friend's response; crits count as 2 beats. Response uses the friend's weapon shape, with a 70% own-weapon budget capped at twice W. 1.5s cooldown.",
                "A 4-beat phrase calls up to 2 friends. Each contributes 70% own-weapon damage, capped at twice W; total is shared by weapon-damage weights. 1.5s cooldown.",
                "A 6-beat phrase calls up to 4 friends using the same weighted, capped damage-budget rule. Fewer friends means fewer performers, not duplicate phantom bodies. 1.5s cooldown." },
            "MoonProwler" => new[] {
                "Travel 5 units and attack behind the target's visible fixed shadow direction. The triggering basic gains 45% damage plus a delayed line budget of 20% of its incoming basic damage. Flank-kill gold is capped at 6 per wave.",
                "The same rear-hit requirement and +45% basic damage; the extra 20% damage budget becomes a forward crescent. Front attacks retain the charge.",
                "The same rear-hit requirement and +45% basic damage; the extra 20% budget splits between two crossing lines. The visible shadow direction stays fixed for the prepared flank." },
            "SupperMortar" => new[] {
                "Each basic becomes 2 physical disc explosions after 0.85s and 1s, each carrying half its damage. Radius 1.05 plus 0.025 per food bought, capped at +0.35. Each explosion shares damage across at most 3 enemies.",
                "Replace both discs with vertical ember line explosions, each carrying half the basic damage at the same delays. This line recipe does not use the food-based disc radius bonus.",
                "Use the food-scaled discs with frost damage and 1s ordinary slow, at the same delays and shared half-damage budgets. No projectile piercing." },
            "LanternWeaver" => new[] {
                "Every fourth attack closes lines between up to 3 live knots. All links share 90% W and apply 0.3s slow. 2.4s cooldown; knots are consumed.",
                "Start the stitch at your position and connect through the knots. Links share 90% W, slow for 0.3s and gently pull ordinary targets toward each segment's origin. 2.4s cooldown.",
                "Replace links with radius-1.8 crescents from each knot toward you. Crescents share 90% W and slow for 0.3s; no pull. 2.4s cooldown." },
            "HourglassStag" => new[] {
                "Wait 4s from the first stored damage, then pay 110% of the bank through a line. Only the deferred part leaves the original basic hit; stored damage is capped at twice W.",
                "Pay after 3s through a radius-2 filled disc at the target. Budget remains 110% of the bank, shared across targets rather than applied in full to each.",
                "Pay after 5s through 3 frost shots sharing 110% of the bank. Frost interactions remain separate; the payout cannot refill its own bank." },
            "RainArchivist" => new[] {
                "Three pages release 3 frost shots plus one per chest page, up to 6, spaced 14 degrees apart. Total budget 90% W; recovery pages allow healing min(4, recovery pages + 1) HP. 3s cooldown.",
                "Add one frost shot to the same page formula, still at most 6. Keep the 90% W total and 14-degree spacing. Healing requires a recovery page. 3s cooldown.",
                "Spread frost shots 24 degrees apart. Always attempt at least 1 HP recovery, or more with recovery pages up to 4. Total budget 90% W; its own recovery cannot write pages. 3s cooldown." },
            _ => Array.Empty<string>()
        };
    }
}
