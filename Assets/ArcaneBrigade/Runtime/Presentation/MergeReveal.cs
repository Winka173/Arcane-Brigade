using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The star-up ceremony after a merge (18/10/2026), painted in the same storybook ink as the weapons' effects.
    /// Two different sequences, so a three-star is plainly the bigger moment (after a research pass on star-ups and golden
    /// reveals in auto-battlers and card games: a held beat before the payoff, a top-down shine wipe, gold leaf, a louder
    /// and longer three-star):
    /// <list type="bullet">
    /// <item>Two stars (about 1.75 s, silver and cream): the donors dim to midnight silhouettes and unspool into ribbons that
    /// wind up the new unit from its feet to its head; it stands as a cream silhouette, squashes, pops, and a wet brush wipes
    /// its colour back top-down behind a diagonal flick; two silver stars stamp on and silver petals drift down.</item>
    /// <item>Three stars (about 3.1 s, gold and amber): the stage dims; three ribbons braid up through the unit and knot over
    /// its head (a short held beat); two crossed gold swashes, gold paint drips down to reveal it over a gold pool; three gold
    /// stars stamp on with a small shake, a brush-drawn crown settles above them and a curtain of gold leaf, petals and stars
    /// falls from the top of the screen.</item>
    /// </list>
    /// No rings and nothing radiating from the unit's centre: ribbons, strokes, drips and falling tokens only. Painted
    /// StarUp_SilverStar / StarUp_GoldStar / StarUp_Crown art replaces the drawn stars and crown once it exists.</summary>
    public sealed class MergeReveal : MonoBehaviour
    {
        public sealed class Beat
        {
            public RosterEntry Result;
            public List<RosterEntry> Materials;
            public Vector2 Position;
            public bool Altar;
        }

        private ArenaController owner;
        private readonly Queue<Beat> queue = new Queue<Beat>();
        private readonly List<UnitVisual> ghosts = new List<UnitVisual>();
        private readonly List<UnitDefinition> definitions = new List<UnitDefinition>();
        private GameObject stage;
        private Beat active;
        private float age;
        private string runId;
        private int lastReceipt;
        private InkedText tierCaption;
        // two stars and fusions
        private const float RevealAt = 0.70f;
        private const float LandAt = 0.95f;
        private const float FinishAt = 1.75f;
        // three stars: the braid knots at GrandKnotAt and the frame holds for GrandHold before the reveal plays on
        private const float GrandKnotAt = 1.05f;
        private const float GrandHold = 0.12f;
        private const float GrandLandAt = 1.45f;
        private const float GrandFinishAt = 3.1f;
        private int cues;
        private SpriteRenderer purchaseShadow;
        private ArsenalInk back, front;
        private bool inked;
        private float dimmed;
        private MaterialPropertyBlock flashBlock;
        private readonly List<Vector2> path = new List<Vector2>(96);
        private readonly List<Vector2> run = new List<Vector2>(96);
        private readonly List<SpriteRenderer> parts = new List<SpriteRenderer>();
        // what the last frame showed (read by checks)
        private int starsShown;
        private bool crownShown;
        public bool ReadOnly { get; set; }
        public bool ReducedMotion { get; set; }
        /// <summary>Captures only: when 0 or more, the ceremony is held at this many seconds in (it never finishes).</summary>
        public float Scrub { get; set; } = -1;
        public int PendingCount => queue.Count + (active == null ? 0 : 1);
        public int CompletedCount { get; private set; }
        public void Configure(ArenaController controller) { owner = controller; }
        public static RunSnapshot Capture(RunSnapshot state) => JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(state));

        private static bool Same(RosterEntry first, RosterEntry second) => first.UnitId == second.UnitId && first.WeaponId == second.WeaponId && first.Tier == second.Tier;
        private static RosterEntry Copy(RosterEntry entry) => new RosterEntry { UnitId = entry.UnitId, WeaponId = entry.WeaponId, Tier = entry.Tier };

        public static List<Beat> CommittedBeats(RunSnapshot before, RunSnapshot after, bool committed, Vector2 position, bool altar)
        {
            var beats = new List<Beat>();
            if (!committed || before == null || after == null || before.Phase != SessionPhase.Shop || after.Phase != SessionPhase.Shop || before.RunId != after.RunId || after.NextReceipt != before.NextReceipt + 1 || !after.Receipts.Exists(receipt => receipt.Id == after.NextReceipt)) return beats;
            if (!altar && after.KitMerges <= before.KitMerges) return beats;
            // Match entries as a multiset: repeated weapons are legitimate saved ingredients.
            var added = new List<RosterEntry>(after.Roster);
            var removed = new List<RosterEntry>();
            foreach (RosterEntry entry in before.Roster)
            {
                int index = added.FindIndex(candidate => Same(entry, candidate));
                if (index >= 0) added.RemoveAt(index); else removed.Add(entry);
            }
            foreach (RosterEntry result in added)
            {
                List<RosterEntry> materials = altar ? new List<RosterEntry>(removed) : removed.FindAll(entry => entry.UnitId == result.UnitId && entry.Tier < result.Tier);
                if (!altar)
                {
                    materials.Sort((first, second) => second.Tier.CompareTo(first.Tier));
                    float capacity = CompanionDeployment.MaterialValue(result.Tier);
                    var allocated = new List<RosterEntry>();
                    foreach (RosterEntry material in materials)
                    {
                        float weight = CompanionDeployment.MaterialValue(material.Tier);
                        if (weight > capacity) continue;
                        allocated.Add(material); capacity -= weight;
                    }
                    materials = allocated;
                }
                if (materials.Count < (altar ? 2 : CompanionDeployment.MergeCopies - 1) || !altar && result.Tier <= 1) continue;
                var copies = new List<RosterEntry>();
                foreach (RosterEntry material in materials) copies.Add(Copy(material));
                beats.Add(new Beat { Result = Copy(result), Materials = copies, Position = position, Altar = altar });
                foreach (RosterEntry material in materials) removed.Remove(material);
                if (altar) break;
            }
            return beats;
        }

        public void EnqueueCommitted(RunSnapshot before, RunSnapshot after, bool committed, Vector2 position, bool altar)
        {
            if (after == null) return;
            if (runId != after.RunId) { Skip(); runId = after.RunId; lastReceipt = 0; CompletedCount = 0; }
            if (after.NextReceipt <= lastReceipt) return;
            List<Beat> beats = CommittedBeats(before, after, committed, position, altar);
            if (!committed) return;
            lastReceipt = after.NextReceipt;
            if (ReadOnly) { Skip(); return; }
            foreach (Beat beat in beats) queue.Enqueue(beat);
        }

        /// <summary>Plays a star-up for a unit already on the roster (captures and tools; no receipt is involved).</summary>
        public void Stage(RosterEntry result, List<RosterEntry> materials, Vector2 position)
        {
            if (owner == null || owner.Session == null) return;
            runId = owner.Session.State.RunId;
            queue.Enqueue(new Beat { Result = Copy(result), Materials = materials, Position = position, Altar = false });
        }

        public void Skip() { queue.Clear(); ClearStage(); }

        /// <summary>True while a ceremony (playing or waiting) is for this unit: the real unit stays hidden where the
        /// ceremony stands and steps out of it when it ends, instead of appearing somewhere nearby.</summary>
        public bool Holds(UnitState unit, out Vector2 at)
        {
            at = default;
            if (unit == null || unit.Faction != Faction.Companion || unit.Definition == null || unit.Definition.Weapon == null) return false;
            bool Match(Beat beat) => beat.Result.UnitId == unit.Definition.Id && beat.Result.WeaponId == unit.Definition.Weapon.Id && beat.Result.Tier == unit.Definition.CombatTier;
            if (active != null && Match(active)) { at = active.Position + Vector2.up * 1.3f; return true; }
            foreach (Beat beat in queue) if (Match(beat)) { at = beat.Position + Vector2.up * 1.3f; return true; }
            return false;
        }

        private bool Grand => active != null && !active.Altar && active.Result.Tier >= 3;
        private float Finish => Grand ? GrandFinishAt : FinishAt;
        /// <summary>The ceremony's clock: a three-star holds one frame for GrandHold when the braid knots.</summary>
        private float VisualTime => !Grand || age < GrandKnotAt ? age : age < GrandKnotAt + GrandHold ? GrandKnotAt : age - GrandHold;

        private void Update()
        {
            if (owner == null || owner.Session == null || owner.Simulation == null) { Idle(); return; }
            if (ReadOnly) { Skip(); return; }
            if (owner.Session.State.RunId != runId || owner.Session.State.Phase != SessionPhase.Shop) { Skip(); return; }
            if (owner.Simulation.Phase == RunPhase.Paused || owner.Details.Open || owner.MenuConfirmation) { Skip(); return; }
            if (owner.Simulation.Phase == RunPhase.Rest && Input.GetKeyDown(KeyCode.Backspace)) { Skip(); return; }
            if (active == null)
            {
                if (queue.Count == 0) { Idle(); return; }
                active = queue.Dequeue(); age = 0; cues = 0;
                CheckpointAudio.PlayInterfaceId("up.merge.gather");
                if (active.Altar) CheckpointAudio.PlayInterfaceId("shop.fusion", 0.09f);
                stage = new GameObject(active.Altar ? "AltarRevealGhosts" : "MergeRevealGhosts");
                stage.transform.SetParent(transform, false);
                stage.transform.position = active.Position + Vector2.up * 1.3f;
                foreach (RosterEntry material in active.Materials) ghosts.Add(CreateGhost(material));
                ghosts.Add(CreateGhost(active.Result));
                if (!active.Altar)
                {
                    var material = new GameObject("PurchasedBodyShadow_NoUnobservedWeapon");
                    material.transform.SetParent(stage.transform, false);
                    purchaseShadow = material.AddComponent<SpriteRenderer>();
                    purchaseShadow.sprite = owner.Content.Unit(active.Result.UnitId).Body;
                    material.transform.localScale = Vector3.one * (0.45f / Mathf.Max(0.01f, purchaseShadow.sprite.bounds.size.y));
                    purchaseShadow.sortingOrder = 2290;
                }
                CreateCaption();
            }
            if (Scrub >= 0) { age = Scrub; PresentStage(); return; }
            age += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            PresentStage();
            if (age >= Finish) { CompletedCount++; ClearStage(); }
        }

        private void PresentStage()
        {
            EnsureInk();
            back.Begin(); front.Begin(); inked = true;
            starsShown = 0; crownShown = false;
            bool grand = Grand, still = ReducedMotion;
            float t = still ? Finish - 0.35f : VisualTime, finish = Finish;
            float fade = still ? 1 : Mathf.Clamp01((finish - age) / 0.3f);
            InkPalette main = active.Altar ? ArsenalInk.Cyan : grand ? ArsenalInk.Gold : ArsenalInk.Silver;
            InkPalette second = active.Altar ? ArsenalInk.Water : grand ? ArsenalInk.Amber : ArsenalInk.Cream;
            float revealAt = grand ? GrandKnotAt : RevealAt, landAt = grand ? GrandLandAt : LandAt;
            float gatherEnd = grand ? 0.62f : 0.42f;
            int tier = Mathf.Clamp(active.Result.Tier, 1, 3);
            float starFirst = grand ? 1.42f : 0.78f, starStep = grand ? 0.12f : 0.09f, starSize = grand ? 0.6f : 0.5f;
            float crownAt = starFirst + 3 * starStep + 0.1f;
            Vector2 centre = stage.transform.position;

            // a small shake as each gold star lands (three stars only)
            Vector2 shake = Vector2.zero;
            if (grand && !still)
                for (int k = 0; k < tier; k++)
                {
                    float since = t - (starFirst + k * starStep);
                    if (since >= 0 && since < 0.14f) shake += new Vector2(Mathf.Sin(since * 190), Mathf.Cos(since * 230)) * 0.022f * (1 - since / 0.14f);
                }

            if (purchaseShadow != null)
            {
                float drop = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.46f));
                purchaseShadow.enabled = !still && age < 0.46f;
                purchaseShadow.transform.localPosition = Vector3.up * (1 - drop) * 0.8f;
                purchaseShadow.color = new Color(0.25f, 0.2f, 0.35f, 1 - drop * 0.7f);
            }

            // the dimmed stage (three stars): a midnight vignette, lightest round the unit, reaching past the screen's edge
            dimmed = grand ? Smooth(0, 0.3f, age) * Mathf.Clamp01((finish - age) / 0.55f) : 0;
            if (dimmed > 0.01f) back.Vignette(centre + Vector2.up * 0.6f, 16, new Color(0.05f, 0.04f, 0.12f), 0.34f * dimmed, 0.9f * dimmed);

            // the result: where it stands and how big it is this frame
            UnitVisual result = ghosts.Count > 0 ? ghosts[ghosts.Count - 1] : null;
            float cx = centre.x, feet = centre.y, height = 1.2f, rx = 0.42f;
            if (result != null)
            {
                bool show = still || t >= (grand ? 0.25f : 0.3f);
                result.gameObject.SetActive(show);
                if (show)
                {
                    result.Display(0, 1);
                    float grow = still ? 1 : Mathf.Lerp(0.88f, 1, Smooth(0.3f, revealAt, t));
                    float squash = still || t >= revealAt ? 0 : Smooth(revealAt - 0.28f, revealAt, t);
                    float since = Mathf.Max(0, t - revealAt) / 0.38f;
                    float pop = still || t < revealAt ? 0 : (grand ? 0.2f : 0.18f) * Mathf.Exp(-4.5f * since) * Mathf.Cos(since * 6.5f);
                    float size = 1.35f * grow;
                    result.transform.position = (Vector3)(centre + shake);
                    result.transform.localScale = new Vector3(size * (1 + pop + squash * 0.07f), size * (1 + pop - squash * 0.1f), 1);
                    result.transform.localRotation = Quaternion.identity;
                    // a flat silhouette until the wipe: cream for two stars, gold for three
                    float line = still ? -1 : t < revealAt ? 2 : Mathf.Lerp(1.1f, -0.2f, Smooth(revealAt + (grand ? 0.06f : 0.02f), revealAt + (grand ? 0.42f : 0.26f), t));
                    Color tone = grand ? new Color(1, 0.83f, 0.42f) : active.Altar ? new Color(0.8f, 0.97f, 1) : new Color(1, 0.97f, 0.88f);
                    Silhouette(result, tone, 1, line);
                    result.SetTerminalFade(still ? 1 : Smooth(grand ? 0.25f : 0.3f, grand ? 0.5f : 0.48f, t) * fade);
                    Measure(result, ref cx, ref feet, ref height, ref rx);
                }
            }
            float top = feet + height;

            // the donors: midnight silhouettes drawn in, each unspooling a ribbon to the new unit's feet
            int donors = Mathf.Max(0, ghosts.Count - 1);
            for (int i = 0; i < donors; i++)
            {
                UnitVisual donor = ghosts[i];
                bool show = !still && t < gatherEnd;
                donor.gameObject.SetActive(show);
                float spread = (i - (donors - 1) * 0.5f) * (grand ? 1.3f : 1.1f);
                float pull = Smooth(0.08f, gatherEnd, t);
                Vector2 from = centre + new Vector2(spread, 0.1f - Mathf.Abs(spread) * 0.08f);
                Vector2 at = Vector2.Lerp(from, centre, pull * 0.8f) + shake;
                if (show)
                {
                    donor.Display(0, 1);
                    donor.transform.position = at;
                    donor.transform.localScale = Vector3.one * Mathf.Lerp(0.95f, 0.5f, pull);
                    donor.transform.localRotation = Quaternion.identity;
                    Silhouette(donor, new Color(0.1f, 0.09f, 0.22f), Smooth(0, 0.22f, t), 2);
                    donor.SetTerminalFade(1 - Smooth(gatherEnd * 0.6f, gatherEnd, t));
                }
                if (still) continue;
                Vector2 chest = at + Vector2.up * 0.45f * Mathf.Lerp(0.95f, 0.5f, pull);
                Vector2 foot = new Vector2(cx, feet + 0.05f);
                Bezier(chest, chest + new Vector2(-spread * 0.2f, 0.9f), foot + new Vector2(spread * 0.35f, -0.1f), foot, 24);
                float head = OutCubic(Mathf.Clamp01((t - 0.06f) / (gatherEnd * 0.8f))), tail = InQuad(Mathf.Clamp01((t - gatherEnd * 0.6f) / (gatherEnd * 0.55f)));
                back.Ribbon(path, i % 2 == 0 ? main : second, 0.06f, head, tail, 1);
            }

            if (!still)
            {
                if (grand) Braid(t, cx, feet, height, rx, main, second);
                else Wind(t, Mathf.Clamp(donors, 2, 3), cx, feet, height, rx, main, second);
            }

            // a soft light behind the figure from the reveal on
            if (result != null && result.gameObject.activeSelf)
            {
                float light = still ? 0.5f : Smooth(revealAt - 0.25f, revealAt, t) * (1 - 0.5f * Smooth(landAt, finish - 0.3f, t)) * fade;
                back.Light(new Vector2(cx, feet + height * 0.55f), height * (grand ? 2.6f : 2f), main.Glow, (grand ? 0.55f : 0.4f) * light);
            }

            if (!still && t >= revealAt)
            {
                if (grand) Gilding(t, cx, feet, height, rx, fade);
                else
                {
                    // the wet brush painting the colour back in, top-down, and a diagonal flick behind
                    float wipe = Mathf.Clamp01((t - revealAt - 0.02f) / 0.24f);
                    if (wipe > 0 && wipe < 1)
                    {
                        float y = feet + Mathf.Lerp(1.1f, -0.2f, Mathf.SmoothStep(0, 1, wipe)) * height;
                        front.Stroke(new Vector2(cx - rx * 1.3f, y + 0.05f), new Vector2(cx + rx * 1.3f, y - 0.05f), second, 0.07f, 1, 0, 0.9f * (1 - wipe * 0.6f), 0.02f, 3);
                    }
                    float flick = t - revealAt;
                    back.Stroke(new Vector2(cx - rx * 2.1f, feet + height * 0.05f), new Vector2(cx + rx * 2.1f, feet + height * 1.1f), main, 0.1f,
                        OutCubic(flick / 0.1f), InQuad(Mathf.Clamp01((flick - 0.12f) / 0.36f)), fade, 0.05f, 7);
                }
            }

            // the stars stamp on above the head (an arch of gold for three stars)
            Sprite star = ArsenalArtwork.Frame(grand ? "StarUp_GoldStar" : "StarUp_SilverStar", 0) ?? MechanicInk.Drawn(InkToken.Star, main);
            float middleRise = grand && tier == 3 ? starSize * 0.2f : 0;
            for (int k = 0; k < tier; k++)
            {
                float since = still ? 1 : t - (starFirst + k * starStep);
                if (since < 0) continue;
                starsShown++;
                float q = Mathf.Clamp01(since / 0.13f);
                Vector2 at = new Vector2(cx + (k - (tier - 1) * 0.5f) * starSize * 0.62f, top + starSize * 0.45f + (k == 1 ? middleRise : 0)) + shake;
                float scale = still ? 1 : Mathf.LerpUnclamped(2.1f, 1, OutBack(q));
                float angle = still ? 0 : (1 - q) * -28 + Mathf.Sin(t * 2.2f + k) * 4;
                front.Place(star, at, starSize * scale, angle, new Color(1, 1, 1, (still ? 1 : Mathf.Clamp01(since / 0.05f)) * fade), true);
                if (still) continue;
                // flecks knocked off as it lands: silver glints, or gold leaf for three stars
                float fleck = (since - 0.1f) / 0.4f;
                if (fleck > 0 && fleck < 1)
                    for (int f = 0; f < 3; f++)
                    {
                        float side = f == 0 ? -1 : f == 1 ? 1 : (k % 2 == 0 ? -0.35f : 0.35f);
                        Vector2 kick = new Vector2(side * 0.32f, f == 2 ? 0.18f : 0.06f) * OutCubic(fleck) + Vector2.down * 0.45f * fleck * fleck;
                        front.TokenAt(grand ? InkToken.Coin : InkToken.Glint, grand ? ArsenalInk.Gold : main, at + kick, (grand ? 0.22f : 0.2f) * (1 - 0.4f * fleck), side * fleck * 300, (1 - fleck) * fade);
                    }
                // a glint passing over each star once it has settled
                float shine = (since - 0.3f) / 0.25f;
                if (shine > 0 && shine < 1) front.TokenAt(InkToken.Glint, ArsenalInk.Cotton, at + new Vector2(starSize * 0.12f, starSize * 0.12f), starSize * 0.5f * Mathf.Sin(shine * Mathf.PI), shine * 90, fade);
            }

            if (grand)
            {
                float since = still ? 1 : t - crownAt;
                if (since >= 0) { crownShown = true; Crown(new Vector2(cx, top + starSize * 0.95f + middleRise) + shake, since, still, fade); }
                if (!still) Curtain(t, cx, centre.y, fade);
            }
            else if (!still) Petals(t, cx, top, main, second, fade);

            if (tierCaption != null)
            {
                float shown = still ? 1 : Smooth(landAt, landAt + 0.15f, t);
                tierCaption.gameObject.SetActive(shown > 0);
                tierCaption.transform.localPosition = new Vector3(0, -0.8f + (1 - shown) * -0.12f, 0);
                Color tint = grand ? new Color(1, 0.8f, 0.34f) : active.Altar ? new Color(0.6f, 1, 0.92f) : new Color(0.86f, 0.93f, 1);
                tint.a = shown * fade; tierCaption.Color = tint;
            }

            if (!still) Cues(t, grand, revealAt, starFirst, crownAt);
            back.End(); front.End();
        }

        private void Cues(float t, bool grand, float revealAt, float starFirst, float crownAt)
        {
            void Once(int bit, float at, string id) { if (t >= at && (cues & bit) == 0) { cues |= bit; CheckpointAudio.PlayInterfaceId(id); } }
            Once(1, revealAt, "up.merge");
            Once(2, starFirst, grand ? "up.star3" : "up.star2");
            if (grand) Once(4, crownAt, "up.rarity.gold");
        }

        /// <summary>Two stars: the ribbons wind up round the silhouette from its feet to its head, passing behind and in front.</summary>
        private void Wind(float t, int strands, float cx, float feet, float height, float rx, InkPalette main, InkPalette second)
        {
            float head = OutCubic(Mathf.Clamp01((t - 0.3f) / 0.34f)), tail = InQuad(Mathf.Clamp01((t - 0.5f) / 0.32f));
            if (head <= tail) return;
            for (int k = 0; k < strands; k++)
            {
                float phase = k * Mathf.PI * 2 / strands + t * 1.4f;
                Helix(cx, feet, height, rx, 2.1f, phase, tail, head, 0.06f, k % 2 == 0 ? main : second, 1, 0);
            }
        }

        /// <summary>Three stars: three ribbons braid up through the unit and pull tight into a knot over its head.</summary>
        private void Braid(float t, float cx, float feet, float height, float rx, InkPalette main, InkPalette second)
        {
            float head = OutCubic(Mathf.Clamp01((t - 0.26f) / 0.72f)), tail = InQuad(Mathf.Clamp01((t - GrandKnotAt - 0.02f) / 0.32f));
            InkPalette[] three = { main, second, ArsenalInk.Cream };
            if (head > tail)
                for (int k = 0; k < 3; k++)
                    Helix(cx, feet - 0.05f, height + 0.55f, rx * 1.05f, 1.75f, k * Mathf.PI * 2 / 3 + t * 2.2f, tail, head, 0.07f, three[k], 1, 0.8f);
            // the knot: tied as the braid arrives, held through the beat, then it fades into the reveal
            float tie = (t - (GrandKnotAt - 0.14f)) / 0.14f;
            if (tie > 0)
            {
                float size = 0.72f * OutBack(Mathf.Clamp01(tie)), alpha = 1 - Smooth(GrandKnotAt + 0.15f, GrandKnotAt + 0.4f, t);
                if (alpha > 0.01f)
                {
                    Vector2 at = new Vector2(cx, feet + height + 0.5f);
                    back.Light(at, 1.4f, ArsenalInk.Gold.Glow, 0.6f * alpha);
                    front.TokenAt(InkToken.Knot, ArsenalInk.Gold, at, size * (1 + 0.15f * (1 - alpha)), Mathf.Sin(t * 3) * 6, alpha);
                }
            }
        }

        /// <summary>One strand wound round an upright figure, drawn for the part [from, to] of its length; the half that passes
        /// behind the figure is painted on the back layer, the half in front over it. narrow tightens it toward the top.</summary>
        private void Helix(float cx, float feet, float height, float rx, float turns, float phase, float from, float to, float width, InkPalette pal, float alpha, float narrow)
        {
            const int samples = 56;
            run.Clear(); bool runFront = false;
            for (int i = 0; i <= samples; i++)
            {
                float s = Mathf.Lerp(from, to, i / (float)samples);
                float angle = phase + s * turns * Mathf.PI * 2;
                float reach = rx * (1 - narrow * s) * (1 - 0.18f * s);
                var point = new Vector2(cx + Mathf.Cos(angle) * reach, feet + s * height + Mathf.Sin(angle) * 0.06f);
                bool inFront = Mathf.Sin(angle) < 0;
                if (run.Count > 0 && inFront != runFront)
                {
                    run.Add(point); FlushRun(runFront, width, pal, alpha);
                    run.Clear();
                }
                if (run.Count == 0) runFront = inFront;
                run.Add(point);
            }
            FlushRun(runFront, width, pal, alpha);
        }
        private void FlushRun(bool inFront, float width, InkPalette pal, float alpha)
        {
            if (run.Count < 2) return;
            path.Clear(); path.AddRange(run);
            (inFront ? front : back).Ribbon(path, pal, width, 1, 0, alpha * (inFront ? 1 : 0.85f), inFront);
        }

        /// <summary>Three stars: crossed calligraphic swashes over the gold silhouette, gold paint dripping down to reveal it,
        /// and the paint gathering in a pool at its feet.</summary>
        private void Gilding(float t, float cx, float feet, float height, float rx, float fade)
        {
            float since = t - GrandKnotAt;
            for (int k = 0; k < 2; k++)
            {
                float s = since - k * 0.07f; if (s < 0) continue;
                float side = k == 0 ? 1 : -1;
                Vector2 a = new Vector2(cx - side * rx * 1.9f, feet + height * 1.08f), b = new Vector2(cx + side * rx * 1.9f, feet + height * 0.12f);
                front.Stroke(a, b, k == 0 ? ArsenalInk.Gold : ArsenalInk.Amber, 0.085f, OutCubic(s / 0.08f), InQuad(Mathf.Clamp01((s - 0.2f) / 0.32f)), fade, 0.06f, 11 + k);
            }
            for (int j = 0; j < 6; j++)
            {
                float s = since - 0.05f - j * 0.028f; if (s <= 0) continue;
                float x = cx + (j - 2.5f) * rx * 0.36f, y = feet + height * (1.02f - 0.06f * (j % 2));
                float length = height * (0.34f + 0.12f * ((j * 37) % 5)) * OutCubic(s / 0.42f);
                float alpha = (1 - Smooth(0.55f, 0.85f, s)) * fade;
                if (alpha <= 0.01f || length < 0.02f) continue;
                front.Stroke(new Vector2(x, y), new Vector2(x + 0.015f * (j % 3 - 1), y - length), ArsenalInk.Gold, 0.075f - 0.008f * (j % 3), 1, 0, alpha, 0.012f, 20 + j);
                front.TokenAt(InkToken.Drop, ArsenalInk.Gold, new Vector2(x, y - length - 0.05f), 0.2f, 180, alpha);
            }
            float pool = Smooth(0.2f, 0.55f, since) * fade;
            if (pool > 0.01f) back.Pool(new Vector2(cx, feet + 0.02f), rx * 1.15f, pool, ArsenalInk.Gold, 23, 0.6f, 0.3f);
        }

        /// <summary>The three-star crown: painted art when it exists, otherwise a brush-drawn crown traced in gold with jewels
        /// on its points, settling down onto the arch of stars.</summary>
        private void Crown(Vector2 at, float since, bool still, float fade)
        {
            float q = still ? 1 : Mathf.Clamp01(since / 0.28f);
            at += Vector2.up * (still ? 0 : (1 - OutBack(q)) * 0.22f);
            float alpha = (still ? 1 : Mathf.Clamp01(since / 0.08f)) * fade;
            Sprite painted = ArsenalArtwork.Frame("StarUp_Crown", 0);
            if (painted != null) { front.Place(painted, at + Vector2.up * 0.2f, 0.62f, 0, new Color(1, 1, 1, alpha), true); return; }
            const float w = 0.5f, h = 0.32f;
            // painted body first: three tapered gold spikes over an amber band, then the inked outline traced over them
            float body = still ? 1 : Smooth(0.04f, 0.2f, since);
            if (body > 0)
            {
                front.Stroke(at + new Vector2(-w * 0.34f, 0.02f), at + new Vector2(-w * 0.52f, h * 0.78f), ArsenalInk.Gold, 0.15f, body, 0, alpha);
                front.Stroke(at + new Vector2(w * 0.34f, 0.02f), at + new Vector2(w * 0.52f, h * 0.78f), ArsenalInk.Gold, 0.15f, body, 0, alpha);
                front.Stroke(at + new Vector2(0, 0.02f), at + new Vector2(0, h), ArsenalInk.Gold, 0.18f, body, 0, alpha);
                front.Stroke(at + new Vector2(-w * 0.52f, 0.05f), at + new Vector2(w * 0.52f, 0.05f), ArsenalInk.Amber, 0.14f, body, 0, alpha);
            }
            path.Clear();
            path.Add(at + new Vector2(-w * 0.5f, 0)); path.Add(at + new Vector2(-w * 0.52f, h * 0.78f)); path.Add(at + new Vector2(-w * 0.24f, h * 0.36f));
            path.Add(at + new Vector2(0, h)); path.Add(at + new Vector2(w * 0.24f, h * 0.36f)); path.Add(at + new Vector2(w * 0.52f, h * 0.78f));
            path.Add(at + new Vector2(w * 0.5f, 0)); path.Add(at + new Vector2(-w * 0.5f, 0));
            front.Ribbon(path, ArsenalInk.Gold, 0.05f, still ? 1 : OutCubic(Mathf.Clamp01(since / 0.22f)), 0, alpha);
            float gems = still ? 1 : Smooth(0.18f, 0.3f, since);
            if (gems <= 0) return;
            front.TokenAt(InkToken.Glint, ArsenalInk.Wine, at + new Vector2(0, h + 0.05f), 0.3f * gems, 0, alpha);
            front.TokenAt(InkToken.Glint, ArsenalInk.Water, at + new Vector2(-w * 0.52f, h * 0.78f + 0.04f), 0.24f * gems, 0, alpha);
            front.TokenAt(InkToken.Glint, ArsenalInk.Water, at + new Vector2(w * 0.52f, h * 0.78f + 0.04f), 0.24f * gems, 0, alpha);
        }

        /// <summary>Two stars: silver petals drifting down past the new unit.</summary>
        private void Petals(float t, float cx, float top, InkPalette main, InkPalette second, float fade)
        {
            for (int i = 0; i < 7; i++)
            {
                float since = t - (RevealAt + 0.04f + i * 0.06f); if (since <= 0) continue;
                float x = cx + (Mathf.Repeat(i * 0.618f, 1) * 2 - 1) * 0.95f + Mathf.Sin(since * 3 + i) * 0.12f;
                float y = top + 0.75f - since * 0.6f;
                float alpha = Mathf.Clamp01(since / 0.1f) * (1 - Smooth(0.75f, 1.05f, since)) * fade;
                front.TokenAt(InkToken.Petal, i % 2 == 0 ? main : second, new Vector2(x, y), 0.36f, since * (120 + i * 20) * (i % 2 == 0 ? 1 : -1), alpha);
            }
        }

        /// <summary>Three stars: a curtain of gold leaf, petals and little stars falling from the top of the screen.</summary>
        private void Curtain(float t, float cx, float ground, float fade)
        {
            Camera view = Camera.main;
            float topEdge = view != null && view.orthographic ? view.transform.position.y + view.orthographicSize + 0.3f : ground + 4.5f;
            float half = view != null && view.orthographic ? view.orthographicSize * view.aspect * 0.9f : 3.5f;
            float middle = view != null ? Mathf.Lerp(view.transform.position.x, cx, 0.5f) : cx;
            for (int i = 0; i < 24; i++)
            {
                float since = t - (GrandLandAt + (i % 12) * 0.06f + (i / 12) * 0.35f); if (since <= 0) continue;
                float speed = 1.7f + ((i * 7) % 5) * 0.22f;
                float x = middle + (Mathf.Repeat(i * 0.618f + 0.1f, 1) * 2 - 1) * half + Mathf.Sin(since * 2.3f + i) * 0.18f;
                float y = topEdge - since * speed;
                if (y < ground - 3) continue;
                float alpha = Mathf.Clamp01(since / 0.1f) * fade;
                float spin = since * (110 + i * 13) * (i % 2 == 0 ? 1 : -1);
                switch (i % 3)
                {
                    case 0: front.TokenAt(InkToken.Coin, ArsenalInk.Gold, new Vector2(x, y), 0.42f, spin, alpha); break;
                    case 1: front.TokenAt(InkToken.Petal, i % 2 == 0 ? ArsenalInk.Amber : ArsenalInk.Wine, new Vector2(x, y), 0.46f, spin, alpha); break;
                    default: front.Place(MechanicInk.Drawn(InkToken.Star, ArsenalInk.Cream), new Vector2(x, y), 0.42f, spin, new Color(1, 1, 1, alpha), true); break;
                }
            }
        }

        /// <summary>A flat silhouette over a ghost's body and weapon in this colour, covering what lies below line (in sprite
        /// heights, 2 = all of it), so a reveal can wipe it away top-down.</summary>
        private void Silhouette(UnitVisual ghost, Color tone, float amount, float line)
        {
            if (flashBlock == null) flashBlock = new MaterialPropertyBlock();
            // every painted part (the body is often drawn as an upper and lower puppet half)
            ghost.GetComponentsInChildren(false, parts);
            foreach (SpriteRenderer part in parts)
            {
                if (part == null || part.sprite == null || !part.enabled || part.sharedMaterial == null || !part.sharedMaterial.HasProperty("_FlashLine")) continue;
                part.GetPropertyBlock(flashBlock);
                flashBlock.SetFloat("_Flash", Mathf.Clamp01(amount));
                flashBlock.SetColor("_FlashColor", tone);
                flashBlock.SetFloat("_FlashLine", line);
                part.SetPropertyBlock(flashBlock);
            }
        }

        /// <summary>The painted figure's extent in the world (its ink, not the sprite's empty margins).</summary>
        private static void Measure(UnitVisual ghost, ref float cx, ref float feet, ref float height, ref float rx)
        {
            SpriteRenderer body = ghost.BodyRenderer;
            if (body == null || body.sprite == null) return;
            Rect ink = SpriteInkMetrics.BoundsOf(body.sprite);
            Vector3 low = body.transform.TransformPoint(ink.min), high = body.transform.TransformPoint(ink.max);
            cx = (low.x + high.x) * 0.5f; feet = Mathf.Min(low.y, high.y); height = Mathf.Max(0.3f, Mathf.Abs(high.y - low.y));
            rx = Mathf.Clamp(Mathf.Abs(high.x - low.x) * 0.55f, 0.22f, 0.7f);
        }

        private void Bezier(Vector2 a, Vector2 b, Vector2 c, Vector2 d, int segments)
        {
            path.Clear();
            for (int i = 0; i <= segments; i++)
            {
                float s = i / (float)segments, u = 1 - s;
                path.Add(u * u * u * a + 3 * u * u * s * b + 3 * u * s * s * c + s * s * s * d);
            }
        }

        private static float Smooth(float a, float b, float x) => Mathf.SmoothStep(0, 1, Mathf.InverseLerp(a, b, x));
        private static float OutCubic(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static float InQuad(float x) { x = Mathf.Clamp01(x); return x * x; }
        private static float OutBack(float x) { x = Mathf.Clamp01(x); const float c = 1.7f; float y = x - 1; return 1 + (c + 1) * y * y * y + c * y * y; }

        private void EnsureInk()
        {
            if (back != null) return;
            // behind the ghosts (sorting group 2295) and in front of them
            back = new ArsenalInk(transform, 2270, 2280);
            front = new ArsenalInk(transform, 2300, 2304);
        }

        /// <summary>Nothing playing: clear the ink once.</summary>
        private void Idle()
        {
            if (!inked) return;
            back.Begin(); back.End(); front.Begin(); front.End(); inked = false; dimmed = 0;
        }

        private void CreateCaption()
        {
            // thick inked lettering (Lilita One with an ink outline), larger for three stars
            InkedText text = InkedText.Create(stage.transform, "CommittedTierLabel", ArcadeTypography.Punch, 64, Grand ? 0.05f : 0.042f, 2310, Grand ? 0.034f : 0.028f);
            text.transform.localPosition = Vector3.down * 0.8f;
            text.Text = active.Altar ? "FUSION COMPLETE" : Grand ? "MASTERED" : "STAR UP";
            text.Color = new Color(1, 0.79f, 0.32f, 0); tierCaption = text;
            text.gameObject.SetActive(false);
        }

        private UnitVisual CreateGhost(RosterEntry entry)
        {
            UnitDefinition definition = Instantiate(owner.Content.Unit(entry.UnitId));
            definitions.Add(definition);
            definition.Weapon = owner.Content.Weapon(entry.WeaponId);
            definition.CombatTier = entry.Tier;
            var child = new GameObject("PresentationGhost");
            child.transform.SetParent(stage.transform, false);
            child.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = 2295;
            UnitVisual visual = child.AddComponent<UnitVisual>();
            visual.CreateHierarchy(owner.Content.Arena.Hand);
            visual.ReducedMotion = true;
            visual.Bind(new UnitState { Id = ghosts.Count + 1, Definition = definition, Faction = Faction.Companion, Position = stage.transform.position, PreviousPosition = stage.transform.position }, owner.Content.Arena);
            child.SetActive(false);
            return visual;
        }

        private void ClearStage()
        {
            if (stage != null) { stage.SetActive(false); Destroy(stage); }
            foreach (UnitDefinition definition in definitions) Destroy(definition);
            definitions.Clear(); ghosts.Clear(); stage = null; active = null; purchaseShadow = null; tierCaption = null;
            Idle();
        }
        private void OnDisable() { Skip(); }
        private void OnDestroy()
        {
            back?.Destroy(); front?.Destroy();
        }
    }
}
