using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Comic sound words over struck enemies (owner 24/10/2026, "ZAP" for the lightning hero and the like): the
    /// hero's own words on its critical blows, on its skill's hits and on a skill's kill, plus a few shared words when a foe
    /// is first shocked, burned or slowed, flung hard or splatted on a wall. Lettered like the skill titles (inked, in the
    /// hero's colours, tilted, popping up then drifting away) but smaller. Kept sparse for small screens: one hero word per
    /// 0.6s (a kill word per 2s), one shared word per second, at most three on screen; the damage-numbers setting hides them.
    /// No words for ordinary hits.</summary>
    public sealed class HitWords : MonoBehaviour
    {
        private sealed class Word { public InkedText View; public Vector2 At; public float Age, Life, Tilt, Size; public Color Colour; }
        private readonly struct Set
        {
            public readonly string[] Crit, Skill; public readonly string Finish;
            public Set(string crit, string skill, string finish) { Crit = crit.Split(' '); Skill = skill.Split(' '); Finish = finish; }
        }
        // crit words | skill words | the skill's kill word (spaces split the choices; a dash joins a word's syllables)
        private static readonly Dictionary<string, Set> Words = new Dictionary<string, Set>
        {
            ["LanternKnight"] = new Set("CLANG! CHOP!", "FWOOSH! GLARE!", "BLAZE!"),
            ["Postmaster"] = new Set("THUNK! STAMP!", "RAT-TAT! THWUMP!", "DELIVERED!"),
            ["DrowsySeer"] = new Set("WHUMP! HUSH!", "ZZZ! SHOOM!", "SWEET_DREAMS"),
            ["RootOracle"] = new Set("THWACK! SNAP!", "KRAK! CREAK!", "TIMBER!"),
            ["LoneJelly"] = new Set("BLOOP! SQUISH!", "SPLORCH! GLORP!", "SLURP!"),
            ["GlassDice"] = new Set("CLACK! TOK!", "DING! ROLL!", "JACKPOT!"),
            ["TideSinger"] = new Set("TWANG! SPLASH!", "SWOOSH! SPLOOSH!", "SURF'S_UP!"),
            ["CometRunner"] = new Set("WHIRR! TICK!", "WHOOSH! KA-BOOM!", "STARFALL!"),
            ["MirrorKeeper"] = new Set("TING! KRSSH!", "BLING! SHATTER!", "MIRRORED!"),
            ["TreasureAcorn"] = new Set("PLINK! BONK!", "RAT-A-TAT! CLINK!", "CHA-CHING!"),
            ["VerdantMatron"] = new Set("FWIP! POP!", "BLOOM! SPROING!", "IN_BLOOM!"),
            ["CopperCauldron"] = new Set("PSSHH! FSSST!", "CHUGGA! WHAM!", "TOOT_TOOT!"),
            ["ChinaMarionette"] = new Set("SHING! SNIP!", "KRAK! TINK!", "GOLDEN!"),
            ["SoapSkater"] = new Set("BLUB! POP!", "SKRRT! CRICK!", "WIPEOUT!"),
            ["VelvetRegent"] = new Set("SHING! CLASH!", "HUZZAH! FANFARE!", "KNIGHTED!"),
            ["ButtonCavalier"] = new Set("TCHK! SHNK!", "EN_GARDE! FLICK!", "TOUCHE!"),
            ["NightMaw"] = new Set("BONK! THUD!", "CHOMP! MUNCH!", "GULP!"),
            ["ThornTreasurer"] = new Set("PLONK! PRICK!", "SKRITCH! STING!", "THORNED!"),
            ["CitrusMarshal"] = new Set("WHAM! SQUIRT!", "CHUGGA! CHOO!", "ALL_ABOARD!"),
            ["IvorySovereign"] = new Set("STAMP! THUD!", "DECREE! GAVEL!", "DENIED!"),
            ["PetalCitadel"] = new Set("FWIP! SWISH!", "FLUTTER! PETAL!", "RISE!"),
            ["PatchworkHost"] = new Set("BOOM! PAFF!", "TA-DA! ENCORE!", "BRAVO!"),
            ["ArchiveCurator"] = new Set("CLICK! FLASH!", "RUSTLE! SNAP!", "ARCHIVED!"),
            ["PennyBroker"] = new Set("BOOM! KLIK!", "KA-CHING! RIIIP!", "SOLD!"),
            ["StormFencer"] = new Set("ZAP! ZZT!", "KRZZT! CRACKLE!", "THUNDER!"),
            ["TinVirtuoso"] = new Set("DING! PLINK!", "BWAMP! TA-DA!", "ENCORE!"),
            ["MoonProwler"] = new Set("SHNK! SLASH!", "SHH! SWISH!", "ECLIPSED"),
            ["SupperMortar"] = new Set("POP! SPLAT!", "SIZZLE! BLORP!", "SERVED!"),
            ["LanternWeaver"] = new Set("ZING! FWIP!", "TWANG! KNOT!", "CAUGHT!"),
            ["HourglassStag"] = new Set("BRRING! DING!", "TICK-TOCK! CLUNK!", "PAID_UP!"),
            ["RainArchivist"] = new Set("SPLISH! DRIP!", "KRAKA! PITTER!", "DOWNPOUR!"),
            ["KiteAdmiral"] = new Set("FWOOP! ZIP!", "SWOOP! WHOOSH!", "FULL_SAIL!"),
            ["SundaeDuchess"] = new Set("PLOP! SPLOOT!", "BRRR! SCOOP!", "BRAIN_FREEZE!"),
            ["BonsaiClocksmith"] = new Set("TICK! BOOM!", "CUCKOO! WHIRR!", "TIME'S_UP!"),
            ["JugglingJester"] = new Set("BOING! HONK!", "WHAP! TOSS!", "TA-DA!"),
            ["BellDiver"] = new Set("PING! CLONG!", "BLOOP! BWOOM!", "DEEP_DIVE!"),
        };
        private static readonly Color Shock = new Color(.6f, .95f, 1f), Burn = new Color(1f, .62f, .3f), Chill = new Color(.66f, .88f, 1f), Plain = new Color(1f, .95f, .82f);

        private readonly List<Word> words = new List<Word>();
        private readonly Dictionary<int, float> shockSeen = new Dictionary<int, float>();
        private float heroWordAt = -10, finishAt = -10, sharedAt = -10;
        private int pick;

        public void Clear()
        {
            foreach (Word w in words) if (w.View != null) w.View.gameObject.SetActive(false);
            shockSeen.Clear(); heroWordAt = finishAt = sharedAt = -10;
        }

        public void Play(CombatSimulation simulation, CombatEvent e)
        {
            if (simulation == null || simulation.Champion == null || !PresentationSettings.DamageNumbers) return;
            float now = simulation.Elapsed;
            if (now < heroWordAt - 5) Clear(); // a new run restarted the clock
            UnitState hero = simulation.Champion;
            bool fromHero = e.Cause.SourceUnitId == hero.Id, skill = e.Cause.ItemId != null && e.Cause.ItemId.StartsWith("hero-identity-", System.StringComparison.Ordinal);
            bool friendly = e.Faction == Faction.Enemy; // events name the struck side
            Words.TryGetValue(hero.Definition.Id ?? "", out Set set);
            HeroMechanicStyle style = HeroMechanicStyle.For(hero.Definition.Id);
            Color heroColour = Color.Lerp(style.PaletteFor(simulation.HeroIdentityApproach).Light, Color.white, .3f);
            Font font = Regal(hero.Definition.Id) ? ArcadeTypography.Display : ArcadeTypography.Punch;

            if (e.Kind == CombatEventKind.Kill && friendly && fromHero && skill && set.Finish != null && now - finishAt >= 2f)
            {
                finishAt = heroWordAt = now;
                Show(set.Finish, e.Position, heroColour, font, 1.35f, 1.05f);
                return;
            }
            if (e.Kind == CombatEventKind.Hit && friendly && fromHero && set.Crit != null && now - heroWordAt >= .6f && (skill || e.Critical))
            {
                heroWordAt = now;
                string[] pool = skill ? set.Skill : set.Crit;
                Show(pool[pick++ % pool.Length], e.Position, heroColour, font, skill ? 1.1f : 1f, .75f);
                return;
            }
            // shared words, for any of our side's blows: a foe first shocked, burned or slowed, flung hard, splatted on a wall
            if (now - sharedAt < 1f) return;
            string shared = null; Color tint = Plain;
            if (e.Kind == CombatEventKind.Hit && friendly && e.Subject != null && e.Subject.KitShockedUntil > now)
            {
                if (!shockSeen.TryGetValue(e.Subject.Id, out float seen) || seen < e.Subject.KitShockedUntil - 2.5f) { shockSeen[e.Subject.Id] = e.Subject.KitShockedUntil; shared = "ZAP!"; tint = Shock; }
            }
            else if (e.Kind == CombatEventKind.StatusApplied && friendly)
            {
                if (e.Element == DamageElement.Ember) { shared = "SIZZLE!"; tint = Burn; }
                else if (e.Element == DamageElement.Frost) { shared = "SLOW"; tint = Chill; }
            }
            else if (e.Kind == CombatEventKind.Knockback && e.Faction != Faction.Enemy && e.Value >= .8f) shared = "WHAM!";
            else if (e.Kind == CombatEventKind.WallSplat && e.Faction != Faction.Enemy) shared = "SPLAT!";
            if (shared == null) return;
            sharedAt = now;
            Show(shared, e.Position, tint, ArcadeTypography.Punch, .85f, .7f);
        }

        private static bool Regal(string id) => id == "VelvetRegent" || id == "IvorySovereign" || id == "ArchiveCurator" || id == "PennyBroker" || id == "ThornTreasurer" || id == "PetalCitadel" || id == "HourglassStag" || id == "MirrorKeeper";

        private void Show(string text, Vector2 at, Color colour, Font font, float size, float life)
        {
            Word word = words.Find(w => w.Age >= w.Life);
            int live = 0; foreach (Word w in words) if (w.Age < w.Life) live++;
            if (live >= 3) return; // never more than three at once
            if (word == null)
            {
                word = new Word { View = InkedText.Create(transform, "Hit word", font, 48, .04f, 2380, .026f) };
                words.Add(word);
            }
            foreach (TextMesh mesh in word.View.GetComponentsInChildren<TextMesh>(true))
                if (mesh.font != font) { mesh.font = font; mesh.GetComponent<MeshRenderer>().sharedMaterial = font.material; }
            word.View.Text = text.Replace('_', ' ');
            int seed = text.Length * 31 + Mathf.RoundToInt(at.x * 17 + at.y * 7);
            word.At = at + new Vector2(((seed % 7) - 3) * .06f, .55f);
            word.Tilt = ((seed % 5) - 2) * 6f; word.Size = size; word.Life = life; word.Age = 0; word.Colour = colour;
            word.View.gameObject.SetActive(true);
            Place(word);
        }

        private void Update()
        {
            float dt = Mathf.Min(Time.deltaTime, .05f);
            foreach (Word w in words)
            {
                if (w.Age >= w.Life) continue;
                w.Age += dt;
                if (w.Age >= w.Life) { w.View.gameObject.SetActive(false); continue; }
                Place(w);
            }
        }

        private static void Place(Word w)
        {
            bool still = PresentationSettings.ReducedMotion;
            float k = w.Age / w.Life, pop = still ? 1 : Mathf.Clamp01(w.Age / .14f);
            float c = 1.7f, y = pop - 1, back = 1 + (c + 1) * y * y * y + c * y * y; // a quick overshoot as it lands
            Vector2 at = w.At + (still ? Vector2.zero : Vector2.up * .38f * k);
            w.View.transform.position = new Vector3(at.x, at.y, 0);
            w.View.transform.localRotation = Quaternion.Euler(0, 0, still ? 0 : w.Tilt * (1 - .5f * k));
            w.View.transform.localScale = Vector3.one * Mathf.Max(.01f, w.Size * (still ? 1 : back) * (1 - .15f * k));
            Color colour = Color.Lerp(w.Colour, Color.white, still ? 0 : .6f * Mathf.Clamp01(1 - w.Age / .12f)); // a white flash as it lands
            colour.a = 1 - Mathf.Clamp01((k - .7f) / .3f);
            w.View.Color = colour;
        }
    }
}
