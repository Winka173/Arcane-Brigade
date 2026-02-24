using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcaneBrigade
{
    public enum CompanionReactionKind { Purchase, Bond, Merge }
    public sealed class CompanionReaction
    {
        public string UnitId, Text, BondId, StoryName;
        public int Tier;
        public CompanionReactionKind Kind;
    }

    public static class CompanionReactions
    {
        // Purchase / bond / merge. Personal voices describe their own choices, never ownership of another person.
        private static readonly Dictionary<string, string[]> lines = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { "TeapotPilgrim", new[] { "A cup for me too? I nearly forgot to ask.", "Leave the last seat open. I'll sit with you.", "A bigger kettle. Someone else can pour the first cup." } },
            { "CandleSnail", new[] { "A little help carrying the flame. Thank you.", "Your spark, my shell. We take turns.", "More room for the names I carried through the flood." } },
            { "MailboxImp", new[] { "Useful delivery. Nobody's name hidden in the bill.", "One route, several addresses. We'll visit them all.", "More room in the bag. The unopened letters stay unopened." } },
            { "PillowOracle", new[] { "Something for the road, and an ordinary rest afterward.", "I can close one eye if you keep the other watch.", "A little stronger. Still allowed to need sleep." } },
            { "MushroomLibrarian", new[] { "That deserves a footnote in our favour.", "Different accounts, kept on the same shelf.", "A wider shelf. No missing reader gets written over." } },
            { "HoodWisp", new[] { "I'll keep your left side covered. You choose the road.", "You came back by choice. I remember that much.", "A wider hood. An open way out for everyone." } },
            { "JellySwordsman", new[] { "That should steady the wobble in my salute.", "Same ferry. Nobody has to prove they deserve a crossing.", "A firmer stance. The last lesson is still lowering the sword." } },
            { "BrassAcorn", new[] { "A useful purchase. Put no crown on the receipt.", "Every root gets a say in where we grow.", "Bigger branches. Plenty of room for the next leader." } },
            { "BreadCrab", new[] { "Good. We can save a little strength for breakfast.", "Ring once for each neighbour. I hear them coming.", "A stronger shell for carrying the breakfast bell." } },
            { "KettleBat", new[] { "A quieter whistle already. Let me check the passage.", "I hear your answer. I'll ask before I follow.", "A deeper whistle. I still check where the echo comes from." } },
            { "BookmarkGolem", new[] { "I'll mark this page. We did something kind here.", "Your place is kept. You can go and come back.", "More pages, and a life beyond the reserved chapter." } },
            { "TinTick", new[] { "Time well spent. I checked the hands myself.", "That bell is for a real arrival. Ours.", "A louder bell. Only for things I've really seen." } },
            { "SpringDuck", new[] { "A little help for the crossing. My friend would approve.", "Hold the rail. I'll test the next step.", "A longer hop. Still my choice when to take it." } },
            { "ButtonBeetle", new[] { "A better fit. Keep the old stitch where I can see it.", "I know those repairs. You've looked after somebody too.", "Larger buttons. I kept the little coat's old repair." } },
            { "StapleMole", new[] { "That belongs in the column marked help, not debt.", "Several witnesses. One honest record.", "A heavier ledger. Nobody's name becomes a charge." } },
            { "SnowSpoon", new[] { "A little less cold to carry with us.", "Take the warm side. I'll mind the frost.", "A broader bowl. We can share the thaw." } },
            { "IceCreamOwl", new[] { "Something useful, with room to change our minds.", "Our lessons can disagree and still belong together.", "Wider wings. No rule says we must fly the same way." } },
            { "FridgeWisp", new[] { "Good. Save your strength for getting home.", "You can lean here without winning anything first.", "A little sturdier. I can put the old title down." } },
            { "PearMoth", new[] { "That might help us reach the next open window.", "Tell them about the outside. I'll keep the light in sight.", "Stronger wings. I'd like to show my family a real sunrise." } },
            { "CitrusToad", new[] { "A sensible purchase. I'll save the joke for later.", "You listened all the way through. I'll do the same.", "A bigger voice. It needn't be funny to be heard." } },
            { "RadishJester", new[] { "For once, a good surprise without a punchline.", "Hear that? Nobody had to laugh to let us stay.", "I can stand a little taller without hiding in the joke." } },
            { "SeedHedgehog", new[] { "That leaves a little more strength for planting.", "Different seeds can share the same ground.", "More room for the seeds. The lost ones still have their names." } },
            { "MoonJelly", new[] { "A little help holding the shape I chose.", "Your tide meets mine. Neither has to swallow the other.", "A wider tide. I'll leave the shore where you can reach it." } },
            { "StarSock", new[] { "A useful thing. I'll try not to lose its other half.", "Mismatched is fine. We can still walk together.", "Bigger steps. Same mismatched pair." } },
            { "CometKite", new[] { "Good. A little more room before the string goes tight.", "Hold loosely. I'm staying because I want to.", "A longer tail. The string still comes back to my hands." } },
            { "EclipseEgg", new[] { "Something for the journey, whoever I become.", "You don't need to know what hatches to make room for me.", "A stronger shell. I'll choose when it opens." } },
            { "OrbitMoth", new[] { "I can mind the dark a little longer with that.", "We can share a light without circling it forever.", "Wider wings. I can fly straight and still choose your company." } },
            { "DreamCompass", new[] { "The needle likes a plan with help in it.", "Several roads, and permission to choose one.", "A steadier needle. Home still gets more than one direction." } },
            { "ShellPostman", new[] { "Good. The letters should reach their doors dry.", "Same rain. I'll shelter your letter beside mine.", "A roomier shell. The thank-you travels safely." } },
            { "CoralTeacup", new[] { "A small improvement. Let's keep the real sea in mind.", "I can hear you over the broadcast now.", "A clearer voice. One day I'll listen without the static." } },
            { "BubbleKoi", new[] { "A little help breathing easily. I'll save you a breath.", "Stay beside me. Nobody has to hold their breath alone.", "A deeper breath. There will still be room in the pond." } },
            { "AnchorSnail", new[] { "That should make the weight a little kinder.", "Take the other end. We can set it down together.", "A stronger shell. Carrying it needn't be my whole life." } },
            { "SailSquid", new[] { "Good supplies. Now let's find a wind we can trust.", "Several hands on the sail. Every hand can let go.", "A fuller sail. This time we choose the harbour." } },
            { "SockCyclops", new[] { "One eye on the purchase, one hope for the road.", "I can watch your back if you watch the doorway.", "A bigger stride. I'll still look back for you." } },
            { "DiceCrab", new[] { "I'd call that a fair roll. No seventh side involved.", "Whatever comes up, we share the table.", "A sturdier pair of claws. The crooked die stays with me." } },
            { "MaskPepper", new[] { "Good. Perhaps I can loosen the mask a little.", "You made room before I put on the brave face.", "Stronger legs. The mask can come off when I'm ready." } },
            { "CrayonGremlin", new[] { "I'll draw that beside the door we're finding.", "We can colour outside the same old lines.", "A broader stroke. Nobody else's picture gets rubbed out." } },
            { "MirrorNewt", new[] { "That looks useful from this side of the glass too.", "I see real company. The reflection can catch up.", "A clearer outline. The face looking back is still mine." } },
            { "RibbonSpider", new[] { "A helpful thread. No knot around anybody's wrist.", "Hold this end if you want to. I'll hold mine.", "A wider weave. Every thread still has a loose end." } },
            { "CoilKettle", new[] { "A careful bargain. Let it simmer, then we go.", "Room by the kettle for every tired traveller.", "A bigger kettle. We don't have to boil the same night again." } },
            { "ThimbleMoth", new[] { "A little help for careful hands. Thank you.", "Pass me the loose edge. We'll mend it without pulling.", "A stronger seam. I kept the old stitches too." } },
            { "ReelOwl", new[] { "Hold that thought. This frame is worth keeping.", "Everyone fits. Nobody has to stand behind the camera.", "A wider frame. I can keep us all in the picture." } },
            { "RainbowTailor", new[] { "A useful pattern. Leave room for the wearer to choose.", "Different colours. No one is sewn into their place.", "More cloth for pockets we can fill with ordinary days." } },
            { "RainbowBell", new[] { "A gentle note for the road. No marching orders.", "Follow if you wish. The ringing isn't a command.", "A fuller bell. There is room for a quieter answer." } },
            { "SecretPatchBear", new[] { "You thought of me while looking at that shelf.", "You made space without asking me to be repaired first.", "A bigger patchwork. I was already worth coming back for." } },
            { "SecretPaperBird", new[] { "That should keep the thank-you safe a little longer.", "No address missing now. We know where to find each other.", "A wider fold. The letter inside is still unopened." } },
            { "SecretDreamLamb", new[] { "A little help before I go back to sleep.", "I'll rest easier knowing you have one another.", "A softer place to land when the visit is over." } },
            { "SaffronMechanist", new[] { "Useful parts. I'll leave the release where you can reach it.", "A working hinge needs room to open both ways.", "Steadier hands. Every lock still gets its own spare key." } },
            { "ClockworkPiper", new[] { "Good. A little more breath for the unfinished tune.", "Keep your own tempo. I'll listen for where we meet.", "A fuller sound. I kept the missing verse's place." } },
            { "VelvetChampion", new[] { "Something useful, without another title to defend.", "The quiet ones count too. I wrote them in myself.", "Stronger. You can still call me without the title." } },
            { "PaperInventor", new[] { "A useful addition. Tell me if it doesn't fit.", "Several hands on the plan. Anyone may ask for a revision.", "A sturdier fold. Every shelter still gets an opening." } },
            { "ReturnCaptain", new[] { "Good provisions. We won't call another circle an arrival.", "Same deck. Every passenger keeps a say in the destination.", "A stronger tiller. This return will be our choice." } },
            { "DoughForeman", new[] { "That's help we can use. Save a portion for the cooks.", "Everyone at the table. Nobody eats after their shift forever.", "Stronger hands. The first loaf still belongs to the people who made it." } },
            { "IronLocomotive", new[] { "A little help for the journey. I'll choose the next stop.", "All aboard by choice. The doors open at every stop.", "A stronger engine. I'd like to visit somewhere new." } },
            { "VioletRemnant", new[] { "That helps the person here, not just the memory.", "You remembered my name and asked what I wanted next.", "A clearer outline. I have more to become than evidence." } },
            { "VelvetEnchantress", new[] { "A useful thing. I'll ask again when the spell has faded.", "You can change your mind. I'll still hear you.", "A steadier voice. An answer given freely carries further." } },
            { "GrandSprout", new[] { "That leaves a little more shelter for young leaves.", "Several roots, and more than one open gate.", "Wider leaves. I won't choose where the little ones settle." } },
            { "BristleBall", new[] { "Something helpful. Perhaps my spines can relax a little.", "Close is all right. Someone else is keeping watch.", "A bigger shape. Still practising how to be gentle." } },
            { "PaperDevil", new[] { "I can use that to open a route I once closed.", "You can walk beside me without saying I've been forgiven.", "I don't merge. I'll keep making my own next choice." } },
            { "BalloonMallet", new[] { "A little help. The valve stays in my hands.", "Rescue crew together. Nobody charges rent for air.", "More room to breathe. I still choose how much air I carry." } },
            { "LittleSprout", new[] { "Will that help us find a window with real rain?", "I can grow this way? You can grow another way too.", "We grew. Please let us choose where the new roots go." } },
            { "MonsterMarble", new[] { "A useful thing. Please put my name on the traveller list.", "Company, not cargo. I like this arrangement.", "Rounder, perhaps. Still a traveller with my own destination." } },
            { "CarouselPony", new[] { "Good supplies. Ask before you climb aboard.", "I'll carry whoever asks. We choose the road together.", "A stronger back. No more circles unless we want them." } },
            { "TeacupCricket", new[] { "I'll try a note with that. Leave room for an answer.", "Different notes. We can still make a tune together.", "A fuller song. The quiet between notes belongs there too." } },
            { "RibbonOtter", new[] { "A little help for the current. I'll mind the loose ribbon.", "Hold on lightly. We can swim beside one another.", "A stronger stroke. I'll leave the river room to turn." } },
            { "SpindleWren", new[] { "A helpful thread. I'll keep both ends within reach.", "A path that can survive an honest goodbye. That's our work.", "A longer thread. I still teach people how to let it go." } },
        };
        public static int SpeciesCount => lines.Count;
        public static bool Has(string id) => id != null && lines.ContainsKey(id);
        public static string Line(string id, CompanionReactionKind kind) => Has(id) ? lines[id][(int)kind] : null;

        public static int Affinity(UnitDefinition unit, WeaponDefinition weapon, ItemDefinition item)
        {
            if (unit == null || weapon == null || item == null) return 0;
            if (item.Category == RelicCategory.Drink && unit.ReferenceCompanion == "Alien") return 100;
            if (item.Category == RelicCategory.Food && unit.ReferenceCompanion == "\"Art of Food\" Roshi") return 100;
            int score = 0;
            foreach (var m in item.EffectiveModifiers)
            {
                if (m.Value <= 0 || !RunModifierRules.Applies(m.Scope, ModifierScope.Companions)) continue;
                bool projectile = weapon.Kind == WeaponKind.Projectile;
                bool applies = m.Effect switch
                {
                    ItemEffect.MeleeDamage => weapon.Kind == WeaponKind.Melee,
                    ItemEffect.RangedDamage or ItemEffect.Projectiles or ItemEffect.BulletPierce or ItemEffect.BulletSplit or ItemEffect.BulletBounce or ItemEffect.BulletSize or ItemEffect.BulletSpeed or ItemEffect.BulletDistance => projectile,
                    ItemEffect.Damage or ItemEffect.CompanionDamage or ItemEffect.AttackSpeed or ItemEffect.CompanionAttackSpeed or ItemEffect.MoveSpeed or ItemEffect.AttackRange or ItemEffect.Crit or ItemEffect.CollisionDamage or ItemEffect.Knockback or ItemEffect.CompanionSize or ItemEffect.DamageCorrection or ItemEffect.SpeedCorrection or ItemEffect.CollisionFrequency => true,
                    _ => false
                };
                if (applies) score = Math.Max(score, 10 + (Array.Exists(unit.CompanionModifiers ?? Array.Empty<StatModifier>(), own => own.Effect == m.Effect) ? 5 : 0));
            }
            return score;
        }

        /// <summary>Only receives a saved successful purchase. Reading/rerolling/reloading and failed saves produce no events.</summary>
        public static List<CompanionReaction> AfterPurchase(RunCatalog catalog, RunSnapshot before, RunSession after, ShopOffer purchase)
        {
            var result = new List<CompanionReaction>(); var state = after.State;
            if (purchase == null || !purchase.Sold || state.NextReceipt <= before.NextReceipt || before.RunId != state.RunId || state.Phase != SessionPhase.Shop || before.Wave != state.Wave) return result;
            using var prior = new RunSession(catalog); prior.Restore(before);
            var crossed = catalog.Bonds.Where(b =>
            {
                prior.BondCounts.TryGetValue(b.Id, out int old); after.BondCounts.TryGetValue(b.Id, out int count);
                return count > old && Math.Abs(b.Reward(count) - b.Reward(old)) > .00001f;
            }).ToArray();
            foreach (var bond in crossed)
            {
                var speakers = state.Roster.Where(r => Has(r.UnitId) && !RunSession.Stranger(state, r.UnitId) && Array.IndexOf(catalog.Unit(r.UnitId).Tags, bond.Id) >= 0).OrderBy(r => r.UnitId).ToArray();
                if (speakers.Length == 0) continue;
                var speaker = speakers[state.NextReceipt % speakers.Length];
                result.Add(new CompanionReaction { UnitId = speaker.UnitId, Tier = speaker.Tier, Kind = CompanionReactionKind.Bond, Text = Line(speaker.UnitId, CompanionReactionKind.Bond), BondId = bond.Id });
                return result; // one response, even if several bonds cross together
            }
            if (purchase.Kind != OfferKind.Item) return result;
            var item = catalog.Item(purchase.Id);
            var participants = state.Roster.Select(r => (entry: r, story: (string)null)).ToList();
            // The four canon travellers are independent of the purchased roster. They may answer a gift, never add bond counts.
            for (int i = 0; i < ActOneCanon.StoryIds.Length; i++)
            {
                string id = ActOneCanon.StoryIds[i];
                if (!ActOneCanon.StoryPresent(i, state.Wave, true, state.Endless) || i == 0 && after.HobRescueNeeded || participants.Any(p => p.entry.UnitId == id)) continue;
                participants.Add((new RosterEntry { UnitId = id, Tier = ActOneCanon.StoryTier(state.Wave), WeaponId = catalog.Unit(id).Weapon.Id }, ActOneCanon.StoryNames[i]));
            }
            var candidates = participants.Where(p => Has(p.entry.UnitId) && !RunSession.Stranger(state, p.entry.UnitId)).Select(p => (entry: p.entry, story: p.story, score: Affinity(catalog.Unit(p.entry.UnitId), catalog.Weapon(p.entry.WeaponId), item))).Where(p => p.score > 0).ToArray();
            if (candidates.Length == 0) return result;
            int best = candidates.Max(p => p.score); var matches = candidates.Where(p => p.score == best).OrderBy(p => p.entry.UnitId).ToArray();
            var chosen = matches[state.NextReceipt % matches.Length];
            result.Add(new CompanionReaction { UnitId = chosen.entry.UnitId, Tier = chosen.entry.Tier, StoryName = chosen.story, Kind = CompanionReactionKind.Purchase, Text = Line(chosen.entry.UnitId, CompanionReactionKind.Purchase) });
            return result;
        }
    }
}
