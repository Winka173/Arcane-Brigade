ROOT = 'C:/Users/Winka/OneDrive/Documents/Arcane-Brigade/'
# id, english design (all living storybook creatures of the Waiting Realm; each carries the prop of its offer)
VISITORS = [
 ("GateKeeper", "the Gatekeeper's herald: a tall lanky door-key creature in a night-blue coat holding a gilded challenge scroll"),
 ("LostPostman", "a lost postman: a small grey owl in a too-big postman cap, clutching a crumpled letter and looking at a map upside down"),
 ("DiceElder", "an old dice gambler: a bearded tortoise in a waistcoat rattling two ivory dice in a cup"),
 ("Pawnbroker", "a roadside pawnbroker: a raccoon behind a tiny folding counter with a brass scale and numbered tags"),
 ("ParcelCourier", "the unmarked parcel: a shy paper-box creature with stubby legs, wrapped in string, no address label"),
 ("TroupeLeader", "a travelling troupe leader: a fox in a patched harlequin cape holding three little theatre masks"),
 ("Lamplighter", "a lamplighter: a thin heron in a long coat carrying a tall brass lamp-lighting pole with a tiny flame"),
 ("LostToy", "a lost toy: a worn little rag rabbit with one button eye, clutching a luggage tag, looking hopeful"),
 ("ToyMender", "a toy mender: a mole in a leather apron with a pincushion on its wrist, holding up a needle and a patched little coat"),
 ("MusicBoxKeeper", "a music-box winder: a mouse in a velvet jacket carrying a music box with a big brass key"),
 ("Barterer", "a barterer: a magpie with a cloak sewn full of little pockets, holding out a shiny trinket in each wing"),
 ("JunkCollector", "a junk collector: a hedgehog pulling a small handcart heaped with old lamps, pans and clocks"),
 ("Lender", "a kind lender: a plump goose in spectacles holding out a glowing wrapped object on a velvet cushion"),
 ("Storyteller", "an old storyteller: a grandmother sheep in a shawl sitting on a small stool with an open storybook"),
 ("SwapClown", "a stall-swapping clown: a jester-doll clown with a striped cone hat juggling two price tags"),
 ("ClockRewinder", "a clock rewinder: a cricket in a waistcoat turning back the hands of a pocket watch bigger than its head"),
 ("SleeplessChild", "a sleepless child: a little teddy-bear child in pyjamas holding a lantern and a pillow, yawning"),
 ("CandyWitch", "a candy witch: a small witch made of spun sugar holding out a drawstring bag of wrapped sweets"),
 ("BalloonSeller", "a balloon seller: a lanky stork holding a bunch of string balloons, each with a little parcel tied below"),
 ("PromiseSeller", "a promise seller: a quiet moth in a long coat holding a ribbon-tied paper promise and a fountain pen"),
 ("Ferryman", "a ferryman: a calm frog in a straw hat leaning on a long punting pole, a little lantern at its tip"),
 ("MatchGirl", "a match girl: a small mouse girl in a hooded cloak holding out one lit match, a matchbox basket on her arm"),
 ("Collector", "an odd collector: a tall crane with a monocle holding a butterfly net and an order notebook"),
 ("Engraver", "a name engraver: a beaver with a tiny chisel and hammer, a brass name plate hanging from its belt"),
 ("Waker", "the waker: a rooster in a nightcap holding a big brass alarm bell"),
 ("DreamSeller", "a dream seller: a cloud-sheep merchant holding three sealed glass jars of swirling dreams"),
 ("Twins", "the twins: two identical small cat sisters in matching dresses holding hands, one holding a coupon"),
 ("WaterCarrier", "a well-water carrier: a sturdy badger carrying two sloshing buckets on a shoulder yoke"),
 ("NightThief", "a night thief: a sly masked weasel in a dark scarf with a sack, tiptoeing"),
 ("DollDoctor", "a doll doctor: a porcelain doll tailor-doctor in a white cap holding a thread spool and a small mended cloak"),
 ("KeyKeeper", "a key keeper: a squirrel with a huge ring of keys on its belt, holding one key up"),
 ("CupMagician", "a stage magician: a rabbit magician in a tall hat waving a wand over a price tag that turns into a ribbon"),
 ("FriendSeeker", "a friend seeker: a shy little ghost-lantern creature holding a drawing of a friend"),
 ("DreamSmith", "a dream smith: a small bear blacksmith with a glowing star-hammer and a tiny anvil"),
 ("MapSeller", "a map seller: a tortoise with rolled maps sticking out of its shell, holding one map open"),
 ("EggTrader", "an egg trader: a hen merchant with a padded basket of speckled eggs"),
 ("WeatherTeller", "a weather teller: a frog in a raincoat holding a tiny umbrella and a weathervane"),
 ("Knitter", "an old knitter: a grandmother cat in a rocking chair knitting a long striped scarf"),
 ("Painter", "a portrait painter: a lanky stoat in a beret with a palette and a small easel"),
 ("ShadowBorrower", "a shadow borrower: a thin figure made of folded paper holding a dark shadow on a string like a kite"),
 ("Sandman", "the sandman: a sleepy mole with a pouch of glittering sand and a nightcap"),
 ("ColourStranger", "a colour stranger: a chameleon in a patchwork coat whose skin shifts between pastel colours, holding a paint pot"),
 ("FuturePost", "the post office to the future: a tiny walking postbox creature with a clock face on its chest and a slot mouth"),
 ("HourglassFixer", "an hourglass turner: a lizard tinkerer with goggles flipping a big brass hourglass"),
 ("Busker", "a wandering busker: a song thrush with a small accordion and an open hat for coins"),
]
SHEET = ("A production hand-painted 2D CHARACTER SPRITE SHEET for the same game as the reference sheets (match their characters' "
         "rendering, proportions and ink line exactly). Exactly 4 columns by {rows} rows of completely isolated full-body sprites on a "
         "truly transparent background. Each ROW holds TWO visiting characters of the dream shop: columns 1-2 are the first visitor, "
         "columns 3-4 the second. For each visitor: the first column is the visitor waiting, relaxed; the second is the visitor "
         "offering its deal toward the viewer with a friendly expressive gesture, holding out its prop. Same scale and identity in both "
         "poses. Living storybook creatures with hand-made curio details; our side never glows ember-red. Front three-quarter view "
         "facing LEFT (toward the player in the shop), full body and feet visible. Sprites use at most 70% of the cell width and 75% of "
         "the cell height, large transparent margins, never overlapping. No ground, no shadows, no glow outside the silhouette, no text, "
         "no grid. 1536x{h} canvas. ")
def jobs(refs, style_ok=True):
    out = []
    for n in range(0, len(VISITORS), 4):
        group = VISITORS[n:n + 4]
        rows = (len(group) + 1) // 2
        cells = []
        for i, (vid, desc) in enumerate(group):
            cells.append(f"ROW {i // 2 + 1} {'LEFT' if i % 2 == 0 else 'RIGHT'} PAIR ({vid}): {desc}")
        sheet = n // 4 + 1
        out.append(dict(name=f"Visitors{sheet}", path=f"Assets/ArcaneBrigade/Art/Expansion/Visitors{sheet}_v1.png", transparent=True, refs=refs,
                        depends=None, mode="single", prompt=SHEET.format(rows=rows, h=512 * rows) + "; ".join(cells) + "."))
    return out
