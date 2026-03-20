"""Image requests for the enemy roster review (approved 19/10/2026, Docs/Enemy_Roster_v2.md): eight new enemies of the
Waiting Realm and five re-themed species. Same atlas layout as K22/K25 (4 columns x 4 rows: rows 1-2 the NORMAL form in eight
poses, rows 3-4 the EVOLVED form in the same eight poses). Imported by the enemy builder from Art/Expansion/Enemies.
Used by Tools/Art/expansion_art.py (section 9)."""

ENEMY_ART = "Assets/ArcaneBrigade/Art/Expansion/Enemies/"
SHARED = ("Production original hand-painted Unity 2D ENEMY sprite atlas for Arcane Brigade's Waiting Realm (a storybook dream "
          "world of lost things, unsent letters, lanterns, a night station and a Black Fog), genuinely transparent background. EXACT "
          "4 columns x 4 rows = 16 full-body sprites of ONE species. TOP two rows: NORMAL form, eight poses in reading order "
          "idle / blink / walk left / walk right / crouched wind-up / charged wind-up / release (attacking) / hurt recoil. BOTTOM two "
          "rows: EVOLVED form, the same eight poses; recognisably the same creature with new visible anatomical changes, never a "
          "recolour. All 16 separate and fully contained, feet aligned, 12% transparent padding and generous transparent gutters, "
          "never overlapping. Seen slightly from above, front three-quarter, stout readable silhouette at about 90 px. Rich dark ink "
          "outline, midnight-indigo shadows, burgundy cloth, chipped ivory, tarnished brass, hostile ember or scarlet eyes. Sinister "
          "but storybook, never gory. Match the references' painted materials. No text, no ground, no cast shadow, no held weapons, "
          "no UI, no aura outside the body. Consistent proportions across the eight poses of each form. Subject: ")
SMALL = ("Production original hand-painted Unity 2D ENEMY sprite sheet for the same game, genuinely transparent background. EXACT 4 "
         "columns x 2 rows = 8 separate full-body sprites. ROW 1: {first} in four poses idle / flap / wind-up / hurt. ROW 2: {second} "
         "in four poses idle / hop / wind-up / hurt. Same ink and painted materials as the references, seen slightly from above, "
         "readable at 60 px, 12% transparent padding, generous gutters, no text, no ground, no shadow, no aura. Subject details: ")

SPECIES = [
    # ---- eight new enemies
    ("SpareChair", "a folding wooden station chair that walks on its four spindly legs, a faded wine velvet seat with a brass number plate, the "
     "seat back shaped like a patient smiling face with two ember eyes; wind-up poses unfold it wide with a warm amber glow under the seat. "
     "EVOLVED: a double bench of two chairs joined by an armrest, a torn cushion with springs showing and a second number plate."),
    ("WakingSitter", "a dusty seated traveller of the Waiting Realm rising from a bench: a gaunt grey figure in a long coat and hat, dust "
     "settled on its shoulders, a luggage tag on its wrist, eyes closed and sad rather than angry, holding a coil of black ink thread that "
     "it throws (no weapon, the thread is part of its sleeve). EVOLVED: taller and more awake, eyes half open and glowing pale, the coat "
     "unravelling into several ink threads, the dust lifting off like moths. Melancholic, not monstrous."),
    ("InkNotary", "a hunched clerk made of folded receipt paper with a green visor, ink-stained sleeves, a big brass rubber stamp built into "
     "one forearm and a pen nib nose; wind-up raises the stamp high. EVOLVED: a second stamp arm, a ledger book opened on its back like a "
     "shell, CLOSED and PAID stamp marks (no readable letters, just stamp shapes) printed over its paper body."),
    ("SewnLetter", "a fat envelope creature sewn shut with crude black thread, a red wax-seal eye in the middle, flapping its top flap like a "
     "mouth, stamp-corner feet. EVOLVED: thicker, double-sealed with two wax eyes, the stitches straining and fraying, a torn address "
     "label flapping like a tongue."),
    ("PartyLine", "an old rotary telephone receiver standing on its curly cord like a snake, the earpiece a mouth with tiny teeth, the cord "
     "coiled under it, cream bakelite chipped and stained; wind-up poses it ringing and shaking. EVOLVED: a full rotary phone body as a "
     "hunched beetle with the dial as a single eye and two receivers as arms."),
    ("DoorKnocker", "a heavy brass door knocker in the shape of a scowling lion-like face holding its ring in its mouth, carried on two "
     "short iron feet; wind-up lifts the ring high to knock. EVOLVED: a whole little front door panel walking behind the knocker, a "
     "peephole eye glowing, three knockers stacked."),
    ("LampSnuffer", "a wisp of Black Fog wearing a brass candle-snuffer cone as a pointed hat, two pale ember eyes under the brim, smoky "
     "arms with long fingers that pinch; the fog body stays inside its silhouette. EVOLVED: a larger fog body wearing a tall stack of "
     "three snuffer cones like a crown, smoke fingers grown into long tongs."),
    ("LedgerHound", "a lean hunting hound made of ledger pages and ink, a fountain-pen nib snout, tally-mark stripes along its back, a "
     "torn ribbon bookmark tail, ember eyes; wind-up crouches low ready to slash. EVOLVED: a second head of folded paper, the nib snout "
     "split into two sharp nibs, five tally marks glowing along the spine."),
    # ---- re-themed species (same behaviour as before, new look)
    ("TollTick", "a round tick-like toll collector with a brass coin-slot mouth, a ticket-punch jaw, a striped toll-booth cap and receipt "
     "paper curling from its back, six little brass legs (replaces Coin Leech: it charges a toll, it does not steal). EVOLVED: a larger "
     "body with a cash-register drawer belly and a long receipt tail."),
    ("CoatRack", "a dusty wooden coat rack hat-stand walking on its three curved feet, an old grey coat and a hat hanging on it to form a "
     "faceless figure, dust drifting from the coat's shoulders, two pale eyes under the hat brim (replaces Spore Bishop: its delayed "
     "pools are settling dust). EVOLVED: more coats and scarves piled on, four arms of hooks, a tall top hat."),
    ("UnclaimedTrunk", "a big battered lost-luggage trunk with brass corners, peeling travel stickers, a paper claim tag, its lid lifting a "
     "crack to show two ember eyes and odd socks and moths spilling out (replaces Smoking Coffin: it releases little lost things). "
     "EVOLVED: the lid wide open like a jaw with leather straps as teeth, a second smaller suitcase strapped on top."),
    ("HatboxHopper", "a round striped station hatbox on a big coiled spring, its lid a mouth, a ribbon bow and a luggage tag, two eyes "
     "peeking from under the lid (replaces Hopper Keg). EVOLVED: two stacked hatboxes on a double spring, the top lid open with a "
     "crushed hat as a tongue."),
    ("SplinterImp", "a small angular imp built from house splinters, broken floorboards and bent nails, a cracked brass door-hinge horn on "
     "its forehead, scarlet eyes in a dark knot-hole face, two stubby feet (re-themes Bristle Imp with the same behaviour). EVOLVED: the "
     "hinge horn splits into forked antlers of door hinges, splinters bristle along the back, tiny ember cracks glow."),
]

def jobs(refs, single):
    for sid, subject in SPECIES:
        single(ENEMY_ART + sid + "_v1.png", "Enemy_" + sid, refs, SHARED + subject + " 1536x1536 canvas.")
    # the Sewn Letter splits: its two halves, then each half into stamps
    single(ENEMY_ART + "SewnLetterPieces_v1.png", "Enemy_SewnLetterPieces", [ENEMY_ART + "SewnLetter_v1.png"] + refs,
           SMALL.format(first="one torn HALF of the sewn envelope creature from the first reference, flapping on its own with half the wax eye",
                        second="a single postage stamp creature with a tiny scowling face and serrated edges")
           + "they must match the Sewn Letter in the first reference exactly. 1536x768 canvas.", depends="Enemy_SewnLetter")
    # the indebted elite's mark: a receipt-ink stamp pressed on the elite enemy
    single(ENEMY_ART + "IndebtedStamp_v1.png", "Enemy_IndebtedStamp", refs,
           "Production hand-painted 2D game emblem, genuinely transparent background: a round receipt-ink rubber-stamp impression in deep "
           "wine-black ink with a cracked coin in the middle and a rough double rim, slightly smudged like a real stamp, no readable letters. "
           "Readable at 32 px. ONE isolated emblem centred with 12% margin, no ground, no shadow, no text. 512x512 canvas.")
