using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>Pictures and clips for the game's website, taken in the Windows player from the real game. The save has really been
    /// played: fourteen finished runs go through the game's own progress code (history, records, Lamp Oil, finds, mastery,
    /// achievements), then the Lamp Tree is lit with the oil they earned. Runs in play hold friends, relics and gold a player of that
    /// night would hold. Diagnostic save folder only.
    /// -arcane-website -capturePath DIR [-arcane-phone] [-website-vi] [-website-no-music] [-website-clips -ffmpegPath ffmpeg.exe]
    /// Writes DIR/shots/*.png, DIR/clips/*.mp4 with a .wav of the game's sound beside each, DIR/facts.json and DIR/Checks.txt.</summary>
    public sealed class WebsiteCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-website") >= 0;
        private readonly List<string> checks = new List<string>();
        private string root, shots, clips, ffmpeg;
        private RunCatalog catalog; private ArenaController owner; private RunStorage storage;
        private bool keepAlive, vietnamese;
        private RunSnapshot summary; private bool drawSummary;
        private readonly RunInvoice invoice = new RunInvoice();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<WebsiteCapture>() != null) return;
            var go = new GameObject("WebsiteCapture"); DontDestroyOnLoad(go); go.AddComponent<WebsiteCapture>();
        }

        private float lastStep;
        private void Check(bool pass, string label) { lastStep = Time.realtimeSinceStartup; checks.Add((pass ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks); }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Texture2D image = ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(shots, name + ".png"), image.EncodeToPNG());
            Check(image.width == Screen.width && image.height == Screen.height, "Shot " + name + " " + image.width + "x" + image.height);
            Destroy(image);
        }
        private void Update()
        {
            if (root != null && Time.realtimeSinceStartup - lastStep > 150) { checks.Add("FAIL stuck after the last step"); File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks); Application.Quit(2); }
            if (keepAlive && owner != null && owner.Simulation?.Champion != null)
            {
                // the hero's health rises and falls as in play; only a hidden floor keeps a recorded night from ending in a fall
                UnitState hero = owner.Simulation.Champion; float floor = hero.Definition.Health * .6f; // high enough that one heavy blow cannot end the night
                if (hero.Health < floor) hero.Health = floor;
            }
        }
        private IEnumerator Wait(float seconds) { yield return new WaitForSecondsRealtime(seconds); }

        // ---- the save: really played ----

        private UnitDefinition[] Friends => catalog.Companions.Where(u => !u.GiftOnly && !u.SpecialRecruit && u.Weapon != null && u.ReferenceCompanion != "Evil" && !u.Id.StartsWith("Secret")).ToArray();
        private ItemDefinition[] Relics => catalog.Items.Where(i => i.ShopWeight > 0 && !i.Id.StartsWith("Secret") && i.Rarity < ItemRarity.Crimson).ToArray();

        /// <summary>A brigade a good player would hold at that night: one bond family filled towards its 2/4/6 marks, a named combo
        /// group, a duet pair, stars that climb as the nights go on, and the stronger relics a player keeps (the website's fights must
        /// look like real play: the crowd falls in droves).</summary>
        private void Brigade(RunSnapshot state, int wave, int seed)
        {
            var random = new System.Random(seed);
            UnitDefinition[] pool = Friends.OrderBy(_ => random.Next()).ToArray();
            int count = Mathf.Clamp(3 + wave / 2, 4, 11);
            state.Roster.RemoveAll(r => true);
            var chosen = new List<UnitDefinition>();
            void Take(UnitDefinition u) { if (u != null && chosen.Count < count && !chosen.Contains(u)) chosen.Add(u); }
            if (wave >= 5)
            {
                ComboDefinition combo = Combos.All.Where(c => c.Members.Length <= 3 && c.Members.All(m => pool.Any(u => u.Id == m))).OrderBy(_ => random.Next()).FirstOrDefault();
                if (combo != null) foreach (string m in combo.Members) Take(pool.First(u => u.Id == m));
            }
            string bond = pool.SelectMany(u => u.Tags ?? new string[0]).GroupBy(t => t).Where(g => g.Count() >= 6).OrderBy(_ => random.Next()).Select(g => g.Key).FirstOrDefault();
            if (bond != null) foreach (UnitDefinition u in pool.Where(u => u.Tags != null && u.Tags.Contains(bond)).Take(Mathf.Clamp(2 + wave / 4, 2, 6))) Take(u);
            var duet = Duets.All.FirstOrDefault(d => pool.Any(u => u.Id == d.A) && pool.Any(u => u.Id == d.B));
            if (duet != null && wave >= 3) { Take(catalog.Unit(duet.A)); Take(catalog.Unit(duet.B)); }
            foreach (UnitDefinition u in pool) Take(u);
            for (int i = 0; i < chosen.Count; i++)
            {
                int tier = wave >= 4 && i % 2 == 0 ? 2 : 1;
                if (wave >= 8) tier = 2;
                if (wave >= 11 && i % 3 == 0) tier = 3;
                if (wave >= 15 && i % 2 == 0) tier = 3;
                state.Roster.Add(new RosterEntry { UnitId = chosen[i].Id, WeaponId = chosen[i].Weapon.Id, Tier = tier });
            }
            state.Items.Clear();
            int relics = Mathf.Clamp(wave / 2 + 2, 2, 12);
            foreach (ItemDefinition relic in Relics.OrderByDescending(r => (int)r.Rarity * (wave / 6 + 1) + random.Next(4)).Take(relics))
                state.Items.Add(new ItemStack { Id = relic.Id, Count = random.Next(4) == 0 ? 2 : 1 });
        }

        private PlayerProfile PlayedProfile()
        {
            var p = new PlayerProfile { TutorialSeen = true, StoryPagesAutoOpen = false };
            if (vietnamese) p.Language = GameLocalization.Vietnamese;
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-website-no-music") >= 0) p.MusicVolume = 0; // effects only: a trailer lays its own music over them
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-website-no-numbers") >= 0) p.ShowDamageNumbers = false; // cinematic shots: the art, not the arithmetic
            MetaProgression.Ensure(p);
            // hero, difficulty, the night reached, won
            var runs = new (int hero, int difficulty, int wave, bool won)[]
            {
                (0, 0, 6, false), (0, 0, 11, false), (0, 0, 14, false), (0, 0, 20, true), (1, 0, 9, false), (1, 0, 17, false),
                (2, 0, 20, true), (0, 1, 13, false), (3, 1, 20, true), (2, 1, 15, false), (4, 1, 18, false), (1, 1, 20, true),
                (5, 2, 10, false), (3, 2, 16, false),
            };
            for (int i = 0; i < runs.Length; i++)
            {
                var plan = runs[i];
                UnitDefinition hero = catalog.Champions[plan.hero % catalog.Champions.Length];
                if (!HeroUnlocks.Open(p, hero)) hero = catalog.Champions.First(c => HeroUnlocks.Open(p, c));
                var random = new System.Random(4100 + i * 37);
                using (var session = new RunSession(catalog))
                {
                    session.Start(hero.Id, 4100 + i * 37);
                    RunSnapshot s = session.State;
                    s.Difficulty = plan.difficulty; s.Wave = plan.wave; s.Phase = plan.won ? SessionPhase.Victory : SessionPhase.Defeat;
                    Brigade(s, plan.wave, 4100 + i * 37);
                    s.TotalKills = plan.wave * plan.wave * 26 + plan.wave * 120 + random.Next(380);
                    s.BossKills = plan.wave / 4 - (!plan.won && plan.wave % 4 == 0 ? 1 : 0);
                    s.CombatSeconds = plan.wave * 61 + random.Next(90);
                    s.DefeatedBosses.Clear(); foreach (string boss in new[] { "CoinWidow", "SuitcaseLeviathan" }.Take(s.BossKills)) if (catalog.Bosses.Any(b => b.Id == boss)) s.DefeatedBosses.Add(boss); // later bosses stay a surprise
                    s.Contributions.Clear();
                    float damage = 9000f * plan.wave * plan.wave * (1 + plan.difficulty * .35f);
                    s.Contributions.Add(new ContributionEntry { UnitId = s.ChampionId, WeaponId = hero.Weapon != null ? hero.Weapon.Id : "", EffectId = "primary", Tier = 1, Damage = damage, Kills = s.TotalKills / 3, Coins = plan.wave * 95 });
                    float share = damage * .62f;
                    foreach (RosterEntry r in s.Roster) { s.Contributions.Add(new ContributionEntry { UnitId = r.UnitId, WeaponId = r.WeaponId, EffectId = "primary", Tier = r.Tier, Damage = share * (.8f + (float)random.NextDouble() * .4f), Kills = (int)(share / 420) }); share *= .78f; }
                    storage.RecordProgress(p, s, catalog);
                }
            }
            // the Lamp Tree lit with the oil those runs earned, cheapest first, as a player tends to
            foreach (string id in new[] { "hero.wick", "gold.purse", "friends.stork", "luck.chest", "dream.oil", "hero.stride", "friends.cradle", "gold.pockets", "luck.clover", "dream.quick", "hero.reach", "gold.purse2", "friends.stork2", "luck.chest2", "dream.oil2", "hero.wick2" })
                MetaProgression.Light(p, id);
            foreach (string id in new[] { "letter.boss", "letter.ten" }) MetaProgression.ReadLetter(p, id);
            MetaProgression.TogglePact(p, MetaProgression.PactSwift);
            p.LastDifficulty = 1; // the menus open on Wandering, the difficulty most players pick
            storage.SaveProfile(p);
            Check(p.History.Count >= 10 && p.Wins >= 3, "Played save: " + p.History.Count + " runs, " + p.Wins + " wins, best wave " + p.BestWave + ", oil " + p.Meta.LampOil + ", keepsakes " + p.Meta.Keepsakes + ", seen friends " + p.SeenUnits.Count + ", relics " + p.SeenItems.Count + ", achievements " + p.Achievements.Count);
            return p;
        }

        /// <summary>A run in play at that night, saved and opened in the arena (the shop, unless the fight is started).</summary>
        private IEnumerator Open(string heroId, int wave, int gold, int sand, int seed, Action<RunSnapshot> setup = null, bool special = false)
        {
            StopRoaming();
            using (var session = new RunSession(catalog))
            {
                session.Start(heroId, seed);
                RunSnapshot s = session.State;
                s.Wave = wave; s.Phase = SessionPhase.Shop; s.Gold = gold; s.Eggs = sand; s.Difficulty = 1;
                s.TotalKills = wave * wave * 24 + wave * 90; s.CombatSeconds = wave * 60;
                s.MienCho.ReadMask = (1 << MienChoStory.Available(s)) - 1;
                s.ActOne ??= new ActOneProgress(); if (wave >= 8) s.ActOne.Offer = ActOneProgress.Refused;
                Brigade(s, wave, seed);
                if (!special) s.SpecialShopVisited = true;
                else
                {
                    // the dream market's wares: a relic paid in blood, one in Dreamsand, a friend in gold
                    s.SpecialShopVisited = false; s.SpecialOffers.Clear();
                    ItemDefinition blood = Relics.First(i => i.EffectiveModifiers.Any(m => m.Value < 0) && i.EffectiveModifiers.Any(m => m.Value > 0));
                    ItemDefinition dream = Relics.Where(i => i.Mechanic != RelicMechanic.None).Skip(seed % 7).First();
                    s.SpecialOffers.Add(new ShopOffer { Token = ++s.NextOffer, Kind = OfferKind.Item, Id = blood.Id, Price = 6, BasePrice = 6, Currency = ShopCurrency.Health });
                    s.SpecialOffers.Add(new ShopOffer { Token = ++s.NextOffer, Kind = OfferKind.Item, Id = dream.Id, Price = 3, BasePrice = 3, Currency = ShopCurrency.Eggs });
                }
                setup?.Invoke(s);
                storage.SaveRun(s);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena");
            owner = null;
            for (float t = 0; t < 8 && (owner == null || owner.Session == null || owner.Shop == null); t += .2f) { yield return Wait(.2f); owner = FindAnyObjectByType<ArenaController>(); }
            yield return Wait(1.2f);
            Check(owner != null && owner.Session != null, "Run opens at wave " + wave + " (" + heroId + ")");
            if (owner != null && owner.Session.State.Phase == SessionPhase.Shop && owner.Session.State.Offers.Count(o => !o.Sold) == 0)
            {
                // a shop opened from a save shows the wares it rolled: roll them as the night's end would, at no cost
                int keep = owner.Session.State.Gold; owner.Session.Reroll(); owner.Session.State.Gold = keep; owner.Shop.Display(); yield return Wait(.6f);
            }
        }
        private IEnumerator Fight(float seconds) { keepAlive = true; owner.NextWave(); for (float t = 0; t < 8 && owner.Simulation.Phase != RunPhase.Combat; t += .1f) yield return Wait(.1f); yield return Roaming(); yield return Wait(seconds); }
        private IEnumerator Stand(Vector2 at)
        {
            owner.GetComponent<ShopInspect>()?.Close();
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = at;
            FindAnyObjectByType<ArenaCamera>()?.Snap(); yield return Wait(.6f);
        }
        /// <summary>The hero walks to a point the way a player steers (the game's own movement input, so its walk and its friends follow).</summary>
        private IEnumerator Walk(Vector2 to, float unused = 0)
        {
            UnitState hero = owner.Simulation.Champion;
            for (float t = 0; t < 6 && Vector2.Distance(hero.Position, to) > .2f; t += Time.unscaledDeltaTime)
            { ArenaController.ScriptedMove = Vector2.ClampMagnitude((to - hero.Position) * 2.5f, 1f); yield return null; }
            ArenaController.ScriptedMove = null;
        }
        private bool roaming; private int roamId;
        /// <summary>Through a fight the hero keeps moving as a player does: weaving loops across the field, pulling the crowd after
        /// it while the friends fight on their own.</summary>
        private IEnumerator Roaming(float unusedSeconds = 0, float unusedRadius = 0) { if (!roaming) StartCoroutine(Roam(++roamId)); yield break; }
        private IEnumerator Roam(int id)
        {
            roaming = true; float start = Time.unscaledTime;
            while (roaming && id == roamId)
            {
                float t = Time.unscaledTime - start;
                bool fighting = owner != null && owner.Simulation != null && owner.Simulation.Phase == RunPhase.Combat && owner.Session != null && owner.Session.State.Phase == SessionPhase.Combat;
                ArenaController.ScriptedMove = fighting ? Steer(owner.Simulation, t) : (Vector2?)null;
                yield return null;
            }
            if (id == roamId) ArenaController.ScriptedMove = null;
        }
        private void StopRoaming() { roaming = false; roamId++; ArenaController.ScriptedMove = null; }

        /// <summary>Where a player would steer now: a weaving loop that drags the crowd, bent towards coins and chests close by, away
        /// from a boss that is too close, and sideways out of a boss's warned strike.</summary>
        private static Vector2 Steer(CombatSimulation sim, float t)
        {
            UnitState hero = sim.Champion;
            Vector2 move = new Vector2(Mathf.Cos(t * .85f), Mathf.Sin(t * 1.3f) * .75f);
            PickupState nearest = null; float best = 3.5f;
            foreach (PickupState pick in sim.Pickups) { float d = Vector2.Distance(pick.Position, hero.Position); if (d < best) { best = d; nearest = pick; } }
            if (nearest != null) move += (nearest.Position - hero.Position).normalized * 1.4f;
            UnitState boss = sim.Boss;
            if (boss != null && boss.Alive)
            {
                Vector2 away = hero.Position - boss.Position; float d = away.magnitude;
                if (d < 3f) move += away.normalized * (3f - d) * 1.2f;
                if (boss.TelegraphUntil > sim.Elapsed && boss.TelegraphDirection.sqrMagnitude > .01f)
                {
                    // step sideways out of the warned line, on the side the hero already stands
                    Vector2 dir = boss.TelegraphDirection.normalized, side = new Vector2(-dir.y, dir.x);
                    if (Vector2.Dot(side, away) < 0) side = -side;
                    move = side * 2.2f + move * .3f;
                }
            }
            return Vector2.ClampMagnitude(move, 1f);
        }

        /// <summary>Plays as a player does for a while: weaves through the night; in the shop walks to the stalls it can afford and
        /// buys (friends first), then starts the next night.</summary>
        private IEnumerator Play(float seconds)
        {
            float from = Time.realtimeSinceStartup;
            while (Time.realtimeSinceStartup - from < seconds)
            {
                if (owner == null || owner.Session == null) { yield return null; continue; }
                bool shop = owner.Session.State.Phase == SessionPhase.Shop; // the session's phase: the fight's own phase lags behind it
                if (!shop && owner.Simulation != null && owner.Simulation.Phase == RunPhase.Combat) { yield return Roaming(); yield return Wait(.25f); continue; }
                StopRoaming();
                if (!shop || owner.Shop == null) { yield return Wait(.25f); continue; }
                // the night's end plays out, the keeper rolls the rug out and the stalls fill: wait for all of it, then a moment to take the shop in
                for (float t = 0; t < 10 && (owner.TransitionPending || owner.Simulation.Phase != RunPhase.Rest); t += .2f) yield return Wait(.2f);
                yield return Wait(2.2f);
                if (owner.Shop.Special)
                {
                    // the dream market opens first: a look, a buy in Dreamsand when there is enough, then on to the keeper's own shop
                    yield return Buy(from, seconds, 1);
                    owner.FinishSpecialShop();
                    for (float t = 0; t < 10 && (owner.TransitionPending || owner.Shop.Special); t += .2f) yield return Wait(.2f);
                    yield return Wait(2f);
                }
                yield return Buy(from, seconds, 3);
                yield return Wait(1f);
                owner.NextWave();
                for (float t = 0; t < 6 && owner.Simulation.Phase != RunPhase.Combat; t += .1f) yield return Wait(.1f);
            }
        }

        /// <summary>How much a player wants an offer (lower first): a copy of a friend already owned (a star-up), a friend that shares
        /// the brigade's bonds, another friend, a relic.</summary>
        private int Want(RunSnapshot s, ShopOffer offer)
        {
            if (offer.Kind != OfferKind.Companion) return 3;
            if (s.Roster.Any(r => r.UnitId == offer.Id && r.Tier == offer.Tier)) return 0;
            UnitDefinition unit = catalog.Unit(offer.Id);
            var bonds = new HashSet<string>(s.Roster.Select(r => catalog.Unit(r.UnitId)).Where(u => u?.Tags != null).SelectMany(u => u.Tags));
            return unit?.Tags != null && unit.Tags.Any(bonds.Contains) ? 1 : 2;
        }

        /// <summary>Walks to the stalls the brigade can pay for (friends first, nearest next) and buys up to <paramref name="most"/>.</summary>
        private IEnumerator Buy(float from, float seconds, int most)
        {
            for (int bought = 0; bought < most && Time.realtimeSinceStartup - from < seconds - 6; bought++)
            {
                RunSnapshot s = owner.Session.State;
                WorldShop.Station stall = owner.Shop.Stations.Where(st => st.Offer != null && !st.Offer.Sold && (st.Offer.Currency == ShopCurrency.Gold && st.Offer.Price <= s.Gold || st.Offer.Currency == ShopCurrency.Eggs && st.Offer.Price <= s.Eggs))
                    .OrderBy(st => Want(s, st.Offer)).ThenBy(st => Vector2.Distance(st.Position, owner.Simulation.Champion.Position)).FirstOrDefault();
                if (stall == null) break;
                yield return Walk(stall.Position + new Vector2(0, -.8f));
                yield return Wait(1.1f); // a look at the card, as a player reads it
                ShopOffer offer = stall.Offer;
                for (int tries = 0; tries < 30 && offer != null && !offer.Sold; tries++) { owner.Shop.Interact(); yield return Wait(.1f); }
                Check(offer != null && offer.Sold, "Gameplay buys " + offer?.Kind + " " + offer?.Id + " on night " + owner.Session.State.Wave);
                yield return Wait(offer != null && offer.Kind == OfferKind.Companion ? 2.2f : 1.2f);
            }
        }

        private RunHud Hud => (RunHud)typeof(ArenaController).GetField("runHud", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);
        private IEnumerator Focus(Func<ShopOffer, bool> which)
        {
            WorldShop.Station station = owner.Shop.Stations.FirstOrDefault(s => s.Offer != null && which(s.Offer));
            if (station == null) { Check(false, "Stall found"); yield break; }
            yield return Stand(station.Position + new Vector2(0, -.3f));
        }

        /// <summary>One part of the capture: if it fails or stalls, it is recorded and the next part goes on.</summary>
        private IEnumerator Section(string name, IEnumerator body)
        {
            bool done = false; keepAlive = false; StopRoaming();
            IEnumerator Run() { yield return body; done = true; }
            Coroutine running = StartCoroutine(Run());
            lastStep = Time.realtimeSinceStartup;
            while (!done && Time.realtimeSinceStartup - lastStep < 45) yield return null;
            if (!done) { StopCoroutine(running); Check(false, "Section stopped: " + name); }
            Time.timeScale = 1; keepAlive = false; StopRoaming();
            ArenaController.CleanFrame = false; ArenaCamera.CaptureFocus = null; ArenaCamera.CaptureHeight = 0; // a stopped trailer part leaves no framing behind
        }

        /// <summary>The death-style review's staging: every 0.9 s a ring of bodies appears beside the hero (three small, one large, one
        /// of air, every other round an elite) and a friend's blow fells them one after another, the same rounds in every variant.</summary>
        private IEnumerator StagedDeaths()
        {
            UnitDefinition[] small = catalog.Enemies.Where(e => e != null && e.Radius < 0.4f && e.BodyHeight < 1.4f && !ExplosionFeedback.Drifts(e)).Take(3).ToArray();
            UnitDefinition large = catalog.Enemies.FirstOrDefault(e => e != null && e.AuthoredMotion != null && e.AuthoredMotion.name.EndsWith("BigRedoMotion", StringComparison.Ordinal)) ?? catalog.Enemies.First(e => e != null && e.BodyHeight >= 1.4f);
            UnitDefinition air = catalog.Enemies.First(e => e != null && ExplosionFeedback.Drifts(e));
            for (int round = 0; round < 10; round++)
            {
                CombatSimulation sim = owner.Simulation; Vector2 hero = sim.Champion.Position;
                var bodies = new List<(UnitState unit, DamageElement element, bool crit, float overkill)>();
                for (int i = 0; i < small.Length; i++) bodies.Add((sim.AddForFeelCapture(small[i], Faction.Enemy, hero + new Vector2(1.6f + i * 0.9f, -1.1f + (i % 2) * 0.9f)), (DamageElement)(i % 4), i == 1, 0.2f));
                UnitState big = sim.AddForFeelCapture(large, Faction.Enemy, hero + new Vector2(-2.2f, 0.4f));
                if (big != null && round % 2 == 1) big.Elite = true;
                bodies.Add((big, DamageElement.Ember, false, 0.4f));
                bodies.Add((sim.AddForFeelCapture(air, Faction.Enemy, hero + new Vector2(0.4f, 1.8f)), DamageElement.Arcane, false, 0.2f));
                yield return Wait(0.35f);
                foreach (var body in bodies) { sim.FellForCapture(body.unit, body.element, body.crit, body.overkill); yield return Wait(0.08f); }
                yield return Wait(0.15f);
            }
        }

        // ---- trailer framing ----

        private Vector2 crowdAt; private float crowdTime = -10;
        /// <summary>The middle of the thickest knot of enemies (looked for twice a second, so the view drifts rather than jumps).</summary>
        private Vector2 Crowd()
        {
            CombatSimulation sim = owner != null ? owner.Simulation : null;
            if (sim == null || sim.Champion == null) return crowdAt;
            if (Time.unscaledTime - crowdTime < .5f) return crowdAt;
            crowdTime = Time.unscaledTime;
            List<Vector2> foes = sim.Units.Where(u => u.Alive && u.Faction == Faction.Enemy).Select(u => u.Position).ToList();
            if (foes.Count == 0) return crowdAt = sim.Champion.Position;
            Vector2 best = foes[0]; int most = -1;
            for (int i = 0; i < foes.Count; i += Mathf.Max(1, foes.Count / 60))
            {
                int near = 0; foreach (Vector2 q in foes) if ((q - foes[i]).sqrMagnitude < 6.25f) near++;
                if (near > most) { most = near; best = foes[i]; }
            }
            Vector2 sum = Vector2.zero; int count = 0;
            foreach (Vector2 q in foes) if ((q - best).sqrMagnitude < 9f) { sum += q; count++; }
            return crowdAt = sum / Mathf.Max(1, count);
        }

        /// <summary>Measures how the friends stand while a clip records: their mean distance to the nearest other friend, and how
        /// often two of them overlap (friends heaped on one spot read as one blob).</summary>
        private IEnumerator Spacing(string label)
        {
            float nearest = 0; int samples = 0, pairs = 0, heaped = 0;
            for (int n = 0; n < 16 && owner != null && owner.Simulation != null; n++)
            {
                yield return Wait(.5f);
                List<Vector2> friends = owner.Simulation.Units.Where(u => u.Alive && u.Faction == Faction.Companion && u.SummonedBy == 0).Select(u => u.Position).ToList(); // the brigade, not summoned slimes and saplings
                for (int i = 0; i < friends.Count; i++)
                {
                    float best = float.MaxValue;
                    for (int j = 0; j < friends.Count; j++) if (j != i) best = Mathf.Min(best, (friends[j] - friends[i]).magnitude);
                    if (best == float.MaxValue) continue;
                    nearest += best; samples++; pairs++; if (best < .35f) heaped++;
                }
            }
            float share = pairs == 0 ? 0 : heaped / (float)pairs;
            if (share >= .2f && owner != null && owner.Simulation != null)
            {
                // which friends heap, and how each one is moving, so a heap can be traced to its cause
                UnitState hero = owner.Simulation.Champion;
                var lines = owner.Simulation.Units.Where(u => u.Alive && u.Faction == Faction.Companion).Select(u => u.Id + " " + u.Definition.Id + " summon " + u.SummonedBy + " mount " + u.MountId + " lift " + u.BodyLift.ToString("0.0") + " mode " + u.TravelMode + " fromHero " + (u.Position - hero.Position).magnitude.ToString("0.00") + " at " + u.Position.ToString("0.00") + " v " + u.Velocity.magnitude.ToString("0.0"));
                File.WriteAllLines(Path.Combine(root, "spacing-" + label + ".txt"), new[] { "hero " + hero.Position.ToString("0.00") + " step " + ((hero.Position - hero.PreviousPosition).magnitude / Mathf.Max(.001f, Time.deltaTime)).ToString("0.0") }.Concat(lines));
            }
            Check(samples > 0 && share < .2f, "Friends spread (" + label + "): mean nearest " + (samples == 0 ? 0 : nearest / samples).ToString("0.00") + ", heaped " + (share * 100).ToString("0") + "%");
        }

        // ---- clips ----

        private IEnumerator Clip(string name, float seconds, IEnumerator action)
        {
            if (ffmpeg == null) { if (action != null) yield return action; yield break; }
            var recorder = gameObject.AddComponent<WebsiteClipRecorder>();
            var tap = gameObject.AddComponent<StarVideoAudioTap>();
            string path = Path.Combine(clips, name + ".mp4");
            recorder.Begin(ffmpeg, path, seconds); tap.Begin();
            if (action != null) StartCoroutine(action);
            while (!recorder.Complete) { lastStep = Time.realtimeSinceStartup; yield return null; } // a long clip is progress, not a stall
            tap.Save(Path.Combine(clips, name + ".wav"));
            Check(recorder.Passed, "Clip " + name);
            Destroy(recorder); Destroy(tap);
        }

        // ---- the capture ----

        private IEnumerator Start()
        {
            string[] args = Environment.GetCommandLineArgs();
            int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length || !RunPaths.Diagnostic) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); shots = Path.Combine(root, "shots"); clips = Path.Combine(root, "clips");
            Directory.CreateDirectory(shots);
            int f = Array.IndexOf(args, "-ffmpegPath");
            if (Array.IndexOf(args, "-website-clips") >= 0 && f >= 0 && f + 1 < args.Length) { ffmpeg = args[f + 1]; Directory.CreateDirectory(clips); }
            vietnamese = Array.IndexOf(args, "-website-vi") >= 0;
            Application.runInBackground = true;
            catalog = Resources.Load<RunCatalog>("RunCatalog");
            storage = new RunStorage(RunPaths.Root);
            foreach (string stale in new[] { "run.json", "run.json.bak" }) { string file = Path.Combine(RunPaths.Root, stale); if (File.Exists(file)) File.Delete(file); }
            if (!vietnamese && !RunUIHints.Phone) Facts();

            PlayerProfile profile = PlayedProfile();
            UnitDefinition[] heroes = catalog.Champions.Where(c => HeroUnlocks.Open(profile, c)).ToArray();
            // the website shows two heroes in play: the Lantern Knight (the game's face) and the Comet Runner, whose wake ends in a
            // starfall; the fights, bosses and places take turns between them
            string mainHero = "LanternKnight", cometHero = "CometRunner";
            string[] stars = { mainHero, cometHero };
            // more heroes whose skills look grand, for the trailer's gallery and some of the gameplay
            string[] guests = { "TideSinger", "VelvetRegent", "TinVirtuoso", "LoneJelly" };

            // a run left at night 13, so the menu offers to continue it
            using (var session = new RunSession(catalog))
            {
                session.Start(cometHero, 913);
                session.State.Wave = 13; session.State.Phase = SessionPhase.Shop; session.State.Gold = 214; session.State.Difficulty = 1;
                Brigade(session.State, 13, 913); storage.SaveRun(session.State);
            }
            SceneManager.LoadScene("RunFrontend"); yield return Wait(3f);

            IEnumerator S0() {
            // ---- the menus ----
            RunMenu menu = FindAnyObjectByType<RunMenu>();
            Check(menu != null, "Menu opens");
            if (menu != null)
            {
                foreach (var (page, name) in new[] { (RunMenu.Page.Landing, "menu-01-main"), (RunMenu.Page.Roster, "menu-02-hero-select"), (RunMenu.Page.Resume, "menu-03-continue"), (RunMenu.Page.Museum, "menu-04-collection"), (RunMenu.Page.Achievements, "menu-05-achievements"), (RunMenu.Page.Records, "menu-06-records"), (RunMenu.Page.Settings, "menu-07-settings"), (RunMenu.Page.Help, "menu-08-how-to-play") })
                {
                    menu.ShowPage(page); yield return Wait(.9f); yield return Shot(name);
                }
                // the Waiting Room between runs and each of its stations
                menu.ShowPage(RunMenu.Page.Hub); menu.Hub.Approach(MetaStation.Lantern); yield return Wait(1f);
                if (ffmpeg != null) yield return Clip("clip-hub-waiting-room", 6f, null);
                yield return Shot("hub-00-waiting-room");
                int n = 1;
                foreach (MetaStation station in (MetaStation[])Enum.GetValues(typeof(MetaStation)))
                {
                    if (!MetaProgression.Works(station)) continue; // stations still to come have no place in the room yet
                    menu.Hub.Close(); menu.Hub.Approach(station); menu.Hub.OpenStation(station); yield return Wait(.7f);
                    if (menu.Hub.PanelOpen) yield return Shot("hub-" + (n++).ToString("00") + "-" + station.ToString().ToLowerInvariant());
                    else Check(false, "Station opens: " + station);
                }
                menu.Hub.Close();
            }

            yield break; }
            IEnumerator S1() {
            // ---- the shop between nights ----
            yield return Open(mainHero, 12, 236, 4, 1212);
            yield return Wait(.8f); yield return Shot("shop-01-overview");
            yield return Focus(o => o.Kind == OfferKind.Item); yield return Shot("shop-02-relic-card");
            yield return Focus(o => o.Kind == OfferKind.Companion); yield return Shot("shop-03-friend-card");
            string bond = owner.Session.BondCounts.Keys.FirstOrDefault(k => owner.Session.BondCounts[k] > 0);
            if (bond != null)
            {
                typeof(RunHud).GetField("selectedBond", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, bond);
                typeof(RunHud).GetField("bondTooltipUntil", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, Time.unscaledTime + 30);
                yield return Wait(.4f); yield return Shot("shop-04-bond");
                typeof(RunHud).GetField("bondTooltipUntil", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, -1f);
            }
            owner.ToggleDetails(); owner.Details.SelectTab(0); yield return Wait(.6f); yield return Shot("shop-05-brigade-details"); owner.ToggleDetails();
            if (!vietnamese)
            {
                owner.Session.ForceVisitor("Sandman"); yield return Stand(new Vector2(4.2f, -.4f));
                IEnumerator Knock() { yield return Wait(1.2f); owner.GetComponent<VisitorCard>().Show(); }
                if (ffmpeg != null) yield return Clip("clip-visitor", 6f, Knock()); else { owner.GetComponent<VisitorCard>().Show(); yield return Wait(.5f); }
                yield return Shot("shop-06-visitor"); owner.VisitorDecline(); yield return Wait(.3f);
                if (owner.Session.OfferPrismGift(true)) { yield return Wait(.9f); yield return Shot("shop-07-keeper-gifts"); }
            }
            owner.TogglePause();
            for (int page = 0; page < 5; page++)
            {
                typeof(RunHud).GetField("pausePage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, page);
                yield return Wait(.35f); yield return Shot("pause-0" + page);
            }
            typeof(RunHud).GetField("pausePage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, 0); owner.TogglePause();

            yield break; }
            IEnumerator S2() {
            // buying: the hero walks to a friend's stall and buys it; a third copy merges into a higher star
            yield return Open(mainHero, 9, 168, 3, 909, s =>
            {
                UnitDefinition friend = Friends.FirstOrDefault(u => u.Id == "TeapotPilgrim") ?? Friends[0];
                s.Roster.RemoveAll(r => r.UnitId == friend.Id);
                s.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = friend.Weapon.Id, Tier = 2 }); s.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = friend.Weapon.Id, Tier = 2 });
                if (s.Offers.Count == 0) s.Offers.Add(new ShopOffer { Token = ++s.NextOffer });
                ShopOffer offer = s.Offers[0]; offer.Kind = OfferKind.Companion; offer.Id = friend.Id; offer.Tier = 2; offer.Price = 48; offer.BasePrice = 48; offer.Currency = ShopCurrency.Gold; offer.Sold = false;
                s.NewRecruitsThisWave = 0;
            });
            {
                ShopOffer offer = owner.Session.State.Offers.FirstOrDefault(o => o.Kind == OfferKind.Companion && o.Tier == 2 && !o.Sold);
                WorldShop.Station stall = offer == null ? null : owner.Shop.Stations.FirstOrDefault(st => st.Offer == offer);
                if (stall != null)
                {
                    yield return Stand(stall.Position + new Vector2(-4.4f, -2.1f));
                    IEnumerator Buy()
                    {
                        yield return Walk(stall.Position + new Vector2(0, -.8f), 3.4f);
                        yield return Wait(.5f);
                        for (int tries = 0; tries < 60 && !offer.Sold; tries++) { owner.Shop.Interact(); yield return Wait(.1f); }
                    }
                    if (ffmpeg != null) yield return Clip("clip-01-buy-and-star-up", 11f, Buy());
                    else { StartCoroutine(Buy()); yield return Wait(3.2f); yield return Shot("shop-08-star-up"); yield return Wait(3f); }
                    if (ffmpeg != null) { yield return Shot("shop-09-after-star-up"); }
                    Check(offer.Sold, "The third copy is bought and merges");
                }
                else Check(false, "Star-up stall");
            }

            yield break; }
            IEnumerator S3() {
            // the dream market: blood prices, Dreamsand, the ferry (the Last Customer's trade is not shown: it is the last boss)
            if (!vietnamese)
            {
                yield return Open(mainHero, 12, 236, 6, 1213, null, true);
                yield return Wait(.6f);
                if (owner.Shop.Special)
                {
                    if (ffmpeg != null) yield return Clip("clip-dream-market", 6f, null);
                    yield return Shot("market-01-dream-market");
                    yield return Focus(o => o.Currency == ShopCurrency.Health || o.Currency == ShopCurrency.Eggs); yield return Shot("market-02-dream-price");
                    var ferry = owner.GetComponent<FerryDock>();
                    if (ferry != null && ferry.Visible && ferry.Spot(0, out Vector2 jetty, out _))
                    {
                        yield return Stand(jetty + new Vector2(.6f, .2f));
                        ferry.ShowForCapture(owner.Session.PartingSpecies().FirstOrDefault()); yield return Wait(.6f); yield return Shot("market-03-ferry");
                        typeof(FerryDock).GetMethod("Close", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(ferry, null); yield return Wait(.2f);
                    }
                }
                else Check(false, "Dream market at wave 12");

                // a secret room: the doors open every third night
                bool saved = false; SecretEventKind shown = SecretEventKind.ImpossibleTailor;
                for (int seed = 0; seed < 400 && !saved; seed++)
                    using (var s = new RunSession(catalog))
                    {
                        s.Start(mainHero, seed); s.State.Wave = 9; s.State.Phase = SessionPhase.Shop; s.State.Gold = 190;
                        s.State.MienCho.ReadMask = (1 << MienChoStory.Available(s.State)) - 1; s.State.ActOne ??= new ActOneProgress(); s.State.ActOne.Offer = ActOneProgress.Refused;
                        Brigade(s.State, 9, seed);
                        SecretEventKind[] portals = s.SecretPortals(); if (portals.Length == 0) continue;
                        shown = portals[0]; storage.SaveRun(s.State); saved = true;
                    }
                if (saved)
                {
                    RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return Wait(2.6f);
                    owner = FindAnyObjectByType<ArenaController>();
                    if (owner.EnterSecret(shown))
                    {
                        if (ffmpeg != null) yield return Clip("clip-secret-room", 6f, null); else yield return Wait(.8f);
                        yield return Shot("secret-01-" + shown.ToString().ToLowerInvariant());
                    }
                    else Check(false, "Secret room opens");
                }
            }

            yield break; }
            IEnumerator S4() {
            // ---- the nights ----
            foreach (var (wave, name) in new[] { (5, "fight-01-early-night"), (14, "fight-02-mid-night"), (18, "fight-03-late-night") })
            {
                yield return Open(stars[wave % 2], wave - 1, 120, 2, 3000 + wave);
                yield return Fight(5f); yield return Shot(name + "-a");
                yield return Wait(3.5f); yield return Shot(name + "-b");
                if (ffmpeg != null && wave >= 14 && !vietnamese) yield return Clip("clip-0" + (wave == 14 ? "2" : "3") + "-fight-wave-" + wave, 12f, Roaming());
                keepAlive = false;
            }
            yield break; }
            IEnumerator S5() {
            // chest and coins
            {
                yield return Open(mainHero, 6, 140, 2, 3006);
                yield return Fight(4f);
                Vector2 hero = owner.Simulation.Champion.Position;
                for (int i = 0; i < 9; i++) owner.Simulation.AddLootPresentationFixture(LootKind.Coin, hero + new Vector2(1.2f + i * .35f, -.6f + (i % 2) * .3f), 3);
                owner.GetComponent<PacingFeedback>()?.Observe(new[] { new CombatEvent(CombatEventKind.ChestOpened, hero + Vector2.right, Faction.Champion, 186, new Vector2(1, 12)) }, owner.PresentationClock);
                yield return Wait(1f); yield return Shot("fight-04-chest-and-coins");
                keepAlive = false;
            }

            yield break; }
            IEnumerator S6() {
            // ---- the bosses: one at the end of every fourth night ----
            if (!vietnamese)
                foreach (int wave in new[] { 4, 8 }) // only the first two bosses: the later ones stay a surprise
                {
                    yield return Open(stars[(wave / 4 + 1) % 2], wave - 1, 150, 3, 4000 + wave);
                    keepAlive = true; owner.NextWave(); yield return Roaming();
                    bool filmed = ffmpeg != null;
                    if (filmed) yield return Clip("clip-" + (wave == 4 ? "04" : "05") + "-boss-night-" + wave, 12f, Roaming());
                    else yield return Wait(2.2f);
                    yield return Shot("boss-night-" + wave.ToString("00") + "-a");
                    yield return Wait(3f); yield return Shot("boss-night-" + wave.ToString("00") + "-b");
                    UnitState boss = owner.Simulation.Boss;
                    if (wave == 4 && boss != null && boss.Alive) { owner.Simulation.BossTelegraphCaptureFixture(); yield return Wait(.9f); yield return Shot("boss-night-04-attack-warning"); }
                    Check(boss != null, "Boss on night " + wave + ": " + boss?.Definition?.DisplayName);
                    keepAlive = false;
                }

            yield break; }
            IEnumerator S7() {
            // ---- the six places ----
            if (!vietnamese)
                foreach (var (wave, name) in new[] { (3, "waiting-room"), (7, "post-office"), (9, "glasshouse"), (13, "lamp-hall"), (17, "theatre"), (19, "archive-vault") })
                {
                    yield return Open(stars[wave % 2], wave - 1, 120, 2, 5000 + wave);
                    yield return Fight(2.5f);
                    owner.Simulation.MapEnvironmentCaptureFixture(); yield return Wait(.8f);
                    yield return Shot("place-" + name);
                    if (ffmpeg != null) yield return Clip("clip-place-" + name, 6f, Roaming());
                    keepAlive = false;
                }

            yield break; }
            IEnumerator S8() {
            // ---- every hero's own skill, and the transformation ----
            if (!vietnamese)
            {
                int index = 0, shown = 0;
                foreach (HeroIdentityDefinition identity in HeroIdentities.All)
                {
                    index++;
                    UnitDefinition hero = catalog.Unit(identity.Id);
                    if (hero == null || !HeroUnlocks.Listed(hero)) continue; // heroes held for a later update or act stay a surprise
                    shown++;
                    yield return Open(identity.Id, 6, 120, 2, 6000 + index);
                    yield return Fight(3.8f); // past the night's opening badge
                    CombatSimulation sim = owner.Simulation;
                    sim.PrepareMechanicCapture(catalog, 0);
                    int casts = sim.HeroIdentityActivations; float started = Time.realtimeSinceStartup;
                    while (sim.HeroIdentityActivations == casts && !sim.HeroAwakening && Time.realtimeSinceStartup - started < 10) { sim.NudgeMechanicCapture(); if (Time.realtimeSinceStartup - started > 4) sim.FillGaugeForCapture(); yield return null; }
                    yield return Wait(.25f);
                    string stem = "skill-" + shown.ToString("00") + "-" + identity.Id;
                    yield return Shot(stem);
                    Check(sim.HeroIdentityActivations > casts || sim.HeroAwakening, "Hero skill fires: " + identity.Id + " (" + identity.Name + ")");
                    if (Array.IndexOf(stars, identity.Id) >= 0)
                    {
                        // the transformation: a full gauge
                        float wait = Time.realtimeSinceStartup;
                        while (!sim.HeroAwakening && Time.realtimeSinceStartup - wait < 8) { sim.FillGaugeForCapture(); yield return null; }
                        if (sim.HeroAwakening)
                        {
                            yield return Wait(.5f);
                            yield return Shot("transform-" + identity.Id);
                        }
                    }
                    keepAlive = false;
                }
            }

            yield break; }
            IEnumerator S10() {
            // ---- the two heroes in a late crowd: their skill again and again, then the transformation ----
            if (!vietnamese)
                foreach (var (hero, wave, name) in new[] { (mainHero, 17, "lantern-knight"), (cometHero, 18, "comet-runner"), (guests[0], 13, "tide-singer"), (guests[1], 14, "velvet-regent"), (guests[2], 15, "tin-virtuoso"), (guests[3], 10, "lone-jelly") })
                {
                    bool lead = hero == mainHero || hero == cometHero; float length = lead ? 15f : 10f;
                    yield return Open(hero, wave - 1, 160, 3, 8100 + wave);
                    yield return Fight(2f);
                    CombatSimulation sim = owner.Simulation;
                    IEnumerator Show()
                    {
                        // the skill comes on its own in a real fight; only if it has not come for a while is it called, and only the two
                        // lead heroes transform (late in the clip, when a full gauge would be near anyway)
                        float from = Time.realtimeSinceStartup; int seen = sim.HeroIdentityActivations; float lastCast = from;
                        while (Time.realtimeSinceStartup - from < length)
                        {
                            float t = Time.realtimeSinceStartup - from;
                            if (sim.HeroIdentityActivations != seen) { seen = sim.HeroIdentityActivations; lastCast = Time.realtimeSinceStartup; }
                            if (lead && t > 8.5f && !sim.HeroAwakening) sim.FillGaugeForCapture();
                            else if (Time.realtimeSinceStartup - lastCast > 4.5f) { sim.NudgeMechanicCapture(); lastCast = Time.realtimeSinceStartup; }
                            yield return null;
                        }
                    }
                    int casts = sim.HeroIdentityActivations;
                    if (ffmpeg != null) yield return Clip("clip-hero-" + name, length, Show());
                    else yield return Show();
                    Check(sim.HeroIdentityActivations > casts, "Hero skill in a crowd: " + hero + " x" + (sim.HeroIdentityActivations - casts));
                    // stills: the skill landing in the crowd, and the transformation (shop 17's Keeper offer is never reached: no gameplay starts there)
                    casts = sim.HeroIdentityActivations; float wait = Time.realtimeSinceStartup;
                    while (sim.HeroIdentityActivations == casts && Time.realtimeSinceStartup - wait < 8) { sim.NudgeMechanicCapture(); yield return null; }
                    yield return Wait(.3f); yield return Shot("hero-" + name + "-skill-a");
                    yield return Wait(.6f); yield return Shot("hero-" + name + "-skill-b");
                    wait = Time.realtimeSinceStartup;
                    while (lead && !sim.HeroAwakening && Time.realtimeSinceStartup - wait < 8) { sim.FillGaugeForCapture(); yield return null; }
                    if (lead && sim.HeroAwakening) { yield return Wait(.8f); yield return Shot("hero-" + name + "-transformed"); }
                    keepAlive = false;
                }
            yield break; }
            IEnumerator S11() {
            // ---- long gameplay: a night to its end, the shop (walk to stalls, buy), the next night; no cuts ----
            if (!vietnamese && ffmpeg != null)
                foreach (var (hero, wave, seconds, name) in new[] {
                    (mainHero, 3, 100f, "gameplay-01-lantern-knight-night-3"), (cometHero, 7, 100f, "gameplay-02-comet-runner-night-7"),
                    (guests[0], 9, 100f, "gameplay-03-tide-singer-night-9"), (guests[1], 13, 100f, "gameplay-04-velvet-regent-night-13"),
                    (mainHero, 14, 100f, "gameplay-05-lantern-knight-night-14"), (cometHero, 18, 100f, "gameplay-06-comet-runner-night-18") }) // a fall is fine: it is real play
                {
                    int pick = Array.IndexOf(args, "-website-gameplay"); // re-take some clips only: -website-gameplay 06
                    if (pick >= 0 && pick + 1 < args.Length && !args[pick + 1].Split(',').Any(n => name.StartsWith("gameplay-" + n, StringComparison.Ordinal))) continue;
                    yield return Open(hero, wave - 1, 260, 9, 9100 + wave);
                    keepAlive = true; owner.NextWave();
                    for (float t = 0; t < 8 && owner.Simulation.Phase != RunPhase.Combat; t += .1f) yield return Wait(.1f);
                    yield return Clip(name, seconds, Play(seconds));
                    Check(true, "Gameplay " + name + " reached night " + owner.Session.State.Wave + " (" + owner.Session.State.Phase + ")");
                    StopRoaming(); keepAlive = false;
                }
            yield break; }
            IEnumerator S9() {
            // ---- the end of a run ----
            foreach (SessionPhase end in new[] { SessionPhase.Victory, SessionPhase.Defeat })
            {
                using (var s = new RunSession(catalog))
                {
                    s.Start(mainHero, end == SessionPhase.Victory ? 7720 : 7709);
                    s.State.Wave = end == SessionPhase.Defeat ? 11 : 20; s.State.Phase = end; s.State.Difficulty = 1;
                    Brigade(s.State, s.State.Wave, 77);
                    s.State.TotalKills = end == SessionPhase.Defeat ? 3842 : 11906; s.State.BossKills = end == SessionPhase.Defeat ? 2 : 5; s.State.CombatSeconds = end == SessionPhase.Defeat ? 702 : 1288;
                    // what each one did, so the result's bars are filled as after a real run
                    s.State.Contributions.Clear(); float share = end == SessionPhase.Defeat ? 410000 : 1462310;
                    s.State.Contributions.Add(new ContributionEntry { UnitId = s.State.ChampionId, WeaponId = "", EffectId = "primary", Tier = 1, Damage = share, Kills = (int)(share / 360), Coins = 1800 });
                    foreach (RosterEntry r in s.State.Roster) { share *= .8f; s.State.Contributions.Add(new ContributionEntry { UnitId = r.UnitId, WeaponId = r.WeaponId, EffectId = "primary", Tier = r.Tier, Damage = share, Kills = (int)(share / 410) }); }
                    storage.SaveRun(s.State);
                }
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return Wait(3.6f);
                owner = FindAnyObjectByType<ArenaController>();
                yield return Shot("result-" + (end == SessionPhase.Victory ? "01-dawn-reached" : "02-brigade-fallen"));
            }
            // the night's invoice: what each friend did and what the Waiting Room receives
            summary = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(owner.Session.State));
            summary.Phase = SessionPhase.Victory; summary.Wave = 20; summary.TotalKills = 11906; summary.BossKills = 5; summary.CombatSeconds = 1288;
            Brigade(summary, 20, 77);
            summary.Contributions.Clear();
            summary.Contributions.Add(new ContributionEntry { UnitId = summary.ChampionId, WeaponId = "", EffectId = "primary", Tier = 1, Damage = 1462310, Kills = 4120, Coins = 2210 });
            float shareLeft = 820000;
            foreach (RosterEntry r in summary.Roster) { summary.Contributions.Add(new ContributionEntry { UnitId = r.UnitId, WeaponId = r.WeaponId, EffectId = "primary", Tier = r.Tier, Damage = shareLeft, Kills = (int)(shareLeft / 410) }); shareLeft *= .76f; }
            summary.Meta ??= new MetaRunState(); summary.Meta.FoundUnits = 2; summary.Meta.FoundItems = 3; summary.Difficulty = 1;
            var earner = new PlayerProfile(); MetaProgression.Ensure(earner); MetaProgression.Credit(earner, summary, catalog);
            drawSummary = true; yield return Wait(.5f); yield return Shot("result-03-night-summary"); drawSummary = false;

            yield break; }
            IEnumerator S12() {
            // ---- trailer footage: the world alone (no HUD), framed close: the dark, the horde, the bosses, a lost night, the Lamp Tree,
            // a star-up, the two lead heroes' skills ----
            if (vietnamese || ffmpeg == null) yield break;
            int pickAt = Array.IndexOf(args, "-trailer-takes"); // re-take some shots only: -trailer-takes alone,fall
            string[] takes = pickAt >= 0 && pickAt + 1 < args.Length ? args[pickAt + 1].Split(',') : null;
            bool Take(string name) => takes == null || Array.IndexOf(takes, name) >= 0;
            RunMenu menu = FindAnyObjectByType<RunMenu>();
            if (menu != null && Take("lamp-tree"))
            {
                // the Lamp Tree: three lanterns lit one after another
                var played = (PlayerProfile)typeof(RunMenu).GetField("profile", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(menu);
                played.Meta.LampOil += 600;
                menu.ShowPage(RunMenu.Page.Hub); menu.Hub.OpenStation(MetaStation.Lantern); menu.Hub.FocusLampBranch(-1); yield return Wait(1f);
                IEnumerator LightUp()
                {
                    int lit = 0;
                    foreach (string id in new[] { "hero.spark", "hero.thick", "hero.candle", "friends.sharp", "gold.shelf", "luck.nose", "dream.glow" })
                    {
                        if (lit == 3) break;
                        if (!menu.Hub.LightForCapture(played, id)) continue;
                        lit++; yield return Wait(2.2f);
                    }
                    Check(lit > 0, "Lamp Tree lanterns lit for the trailer: " + lit);
                }
                yield return Clip("trailer-lamp-tree", 8f, LightUp());
                menu.Hub.Close(); yield return Wait(.5f);
            }
            ArenaController.CleanFrame = true;
            try
            {
                // alone in the dark: the start of a late night in the Archive Vault, close on the hero as the first shapes come
                if (Take("alone"))
                {
                    yield return Open(mainHero, 18, 40, 0, 12001);
                    keepAlive = true; owner.NextWave(); ArenaCamera.CaptureHeight = 2.8f; FindAnyObjectByType<ArenaCamera>()?.Snap();
                    IEnumerator Creep() { yield return Walk(owner.Simulation.Champion.Position + new Vector2(1.6f, .4f)); }
                    yield return Clip("trailer-alone", 9f, Creep());
                    StopRoaming(); keepAlive = false;
                }

                // the horde: late nights framed from the crowd's side, the brigade small at the far edge
                foreach (int wave in new[] { 14, 18 })
                {
                    if (!Take("horde-" + wave)) continue;
                    yield return Open(wave == 14 ? mainHero : cometHero, wave - 1, 160, 3, 12100 + wave);
                    keepAlive = true; owner.NextWave(); yield return Wait(4f);
                    ArenaCamera.CaptureHeight = 3.8f; ArenaCamera.CaptureFocus = () => Vector2.Lerp(Crowd(), owner.Simulation.Champion.Position, .3f);
                    yield return Clip("trailer-horde-" + wave, 10f, null);
                    ArenaCamera.CaptureFocus = null; keepAlive = false;
                }

                // the two first bosses, close, then wide with the brigade around them
                foreach (int wave in new[] { 4, 8 })
                {
                    if (!Take("boss-" + wave)) continue;
                    yield return Open(stars[(wave / 4 + 1) % 2], wave - 1, 150, 3, 12200 + wave);
                    keepAlive = true; owner.NextWave(); yield return Roaming();
                    ArenaCamera.CaptureHeight = 3.4f; ArenaCamera.CaptureFocus = () => owner.Simulation.Boss != null && owner.Simulation.Boss.Alive ? owner.Simulation.Boss.Position : owner.Simulation.Champion.Position;
                    IEnumerator Widen() { yield return Wait(5.5f); if (owner.Simulation.Boss != null && owner.Simulation.Boss.Alive) owner.Simulation.BossTelegraphCaptureFixture(); yield return Wait(1.5f); ArenaCamera.CaptureHeight = 5.2f; ArenaCamera.CaptureFocus = () => Vector2.Lerp(owner.Simulation.Boss != null && owner.Simulation.Boss.Alive ? owner.Simulation.Boss.Position : owner.Simulation.Champion.Position, owner.Simulation.Champion.Position, .5f); }
                    StartCoroutine(Widen());
                    yield return Clip("trailer-boss-" + wave, 13f, null);
                    Check(owner.Simulation.Boss != null, "Trailer boss on night " + wave + ": " + owner.Simulation.Boss?.Definition?.DisplayName);
                    ArenaCamera.CaptureFocus = null; StopRoaming(); keepAlive = false;
                }

                // a lost night: a thin brigade in a late crowd; the hero holds a while as the crowd closes in, then falls
                if (Take("fall"))
                {
                    yield return Open(mainHero, 18, 40, 0, 12319, s => { s.Roster.RemoveRange(2, Mathf.Max(0, s.Roster.Count - 2)); foreach (RosterEntry r in s.Roster) r.Tier = 1; s.Items.Clear(); });
                    keepAlive = true; owner.NextWave(); yield return Wait(3f);
                    ArenaCamera.CaptureHeight = 4.8f; FindAnyObjectByType<ArenaCamera>()?.Snap(); ArenaCamera.CaptureHeight = 3.2f; // a slow push in as the crowd closes
                    IEnumerator Overrun() { yield return Wait(8f); keepAlive = false; owner.Simulation.Champion.Health = owner.Simulation.Champion.Definition.Health * .12f; }
                    yield return Clip("trailer-fall", 16f, Overrun());
                    Check(owner.Session.State.Phase == SessionPhase.Defeat, "Trailer lost night ends in a fall (" + owner.Session.State.Phase + ")");
                }

                // a star-up: the third copy bought and merged, close on the hero
                if (Take("star-up")) {
                yield return Open(mainHero, 9, 168, 3, 12409, s =>
                {
                    UnitDefinition friend = Friends.FirstOrDefault(u => u.Id == "TeapotPilgrim") ?? Friends[0];
                    s.Roster.RemoveAll(r => r.UnitId == friend.Id);
                    s.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = friend.Weapon.Id, Tier = 2 }); s.Roster.Add(new RosterEntry { UnitId = friend.Id, WeaponId = friend.Weapon.Id, Tier = 2 });
                    if (s.Offers.Count == 0) s.Offers.Add(new ShopOffer { Token = ++s.NextOffer });
                    ShopOffer o = s.Offers[0]; o.Kind = OfferKind.Companion; o.Id = friend.Id; o.Tier = 2; o.Price = 48; o.BasePrice = 48; o.Currency = ShopCurrency.Gold; o.Sold = false;
                    s.NewRecruitsThisWave = 0;
                });
                {
                    ShopOffer offer = owner.Session.State.Offers.FirstOrDefault(o => o.Kind == OfferKind.Companion && o.Tier == 2 && !o.Sold);
                    WorldShop.Station stall = offer == null ? null : owner.Shop.Stations.FirstOrDefault(st => st.Offer == offer);
                    if (stall != null)
                    {
                        yield return Stand(stall.Position + new Vector2(-2.6f, -1.4f));
                        ArenaCamera.CaptureHeight = 3.2f; ArenaCamera.CaptureFocus = () => Vector2.Lerp(owner.Simulation.Champion.Position, stall.Position, .35f); FindAnyObjectByType<ArenaCamera>()?.Snap();
                        IEnumerator Buy() { yield return Walk(stall.Position + new Vector2(0, -.8f)); yield return Wait(.4f); for (int tries = 0; tries < 60 && !offer.Sold; tries++) { owner.Shop.Interact(); yield return Wait(.1f); } }
                        yield return Clip("trailer-star-up", 9f, Buy());
                        Check(offer.Sold, "Trailer star-up bought");
                        ArenaCamera.CaptureFocus = null;
                    }
                    else Check(false, "Trailer star-up stall");
                }
                }

                // the brigade at work: friends spread around the hero (their spacing is measured)
                if (Take("brigade"))
                {
                    yield return Open(mainHero, 12, 160, 3, 12513);
                    keepAlive = true; owner.NextWave(); yield return Roaming(); yield return Wait(2f);
                    ArenaCamera.CaptureHeight = 4.4f;
                    yield return Clip("trailer-brigade", 10f, Spacing("brigade"));
                    StopRoaming(); keepAlive = false;
                }

                // the two lead heroes: close on each skill as it lands, then the transformation in a late crowd
                foreach (var (hero, wave, name) in new[] { (mainHero, 13, "lantern-knight"), (cometHero, 18, "comet-courier"), (mainHero, 19, "lantern-knight-late") })
                {
                    if (!Take("skill-" + name)) continue;
                    yield return Open(hero, wave - 1, 160, 3, 12600 + wave);
                    keepAlive = true; owner.NextWave(); yield return Roaming(); yield return Wait(2f);
                    CombatSimulation sim = owner.Simulation; float length = 16f;
                    ArenaCamera.CaptureHeight = name.EndsWith("late") ? 5f : 3.9f;
                    IEnumerator Show()
                    {
                        StartCoroutine(Spacing(name));
                        float from = Time.realtimeSinceStartup; int seen = sim.HeroIdentityActivations; float lastCast = from;
                        while (Time.realtimeSinceStartup - from < length)
                        {
                            float t = Time.realtimeSinceStartup - from;
                            if (sim.HeroIdentityActivations != seen) { seen = sim.HeroIdentityActivations; lastCast = Time.realtimeSinceStartup; }
                            if (t > 9f && !sim.HeroAwakening) sim.FillGaugeForCapture();
                            else if (Time.realtimeSinceStartup - lastCast > 3.5f) { sim.NudgeMechanicCapture(); lastCast = Time.realtimeSinceStartup; }
                            yield return null;
                        }
                    }
                    int casts = sim.HeroIdentityActivations;
                    yield return Clip("trailer-skill-" + name, length, Show());
                    Check(sim.HeroIdentityActivations > casts, "Trailer skill: " + hero + " x" + (sim.HeroIdentityActivations - casts) + (sim.HeroAwakening ? ", transformed" : ""));
                    StopRoaming(); keepAlive = false;
                }
            }
            finally { ArenaController.CleanFrame = false; ArenaCamera.CaptureFocus = null; ArenaCamera.CaptureHeight = 0; }
            yield break; }
            IEnumerator S13() {
            // ---- the death review: a staged fight where small, large, elite and airy bodies fall beside the hero ----
            if (vietnamese || ffmpeg == null) yield break;
            yield return Open(mainHero, 4, 160, 3, 13016);
            keepAlive = true; owner.NextWave(); yield return Wait(2f);
            ArenaCamera.CaptureHeight = 3.4f;
            try { yield return Clip("deaths", 10f, StagedDeaths()); }
            finally { ArenaCamera.CaptureHeight = 0; keepAlive = false; }
            yield break; }
            var sections = new (string, Func<IEnumerator>)[] { ("menus", S0), ("shop", S1), ("star-up", S2), ("dream-market", S3), ("nights", S4), ("chest", S5), ("bosses", S6), ("places", S7), ("hero-skills", S8), ("results", S9), ("hero-clips", S10), ("gameplay", S11), ("trailer", S12), ("deaths", S13) };
            int only = Array.IndexOf(args, "-website-sections"); string[] wanted = only >= 0 && only + 1 < args.Length ? args[only + 1].Split(',') : null; // re-take some parts only
            foreach (var (name, body) in sections) if (wanted == null || Array.IndexOf(wanted, name) >= 0) yield return Section(name, body());
            Check(true, "Capture finished");
            File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks);
            Application.Quit(0);
        }

        private void OnGUI()
        {
            if (!drawSummary || summary == null) return;
            GUI.depth = -200; Matrix4x4 previous = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(.03f, .03f, .06f, 1));
            invoice.Draw(new Rect(430, 120, 1060, 860), summary, catalog);
            GUI.matrix = previous;
        }

        // ---- facts for the website copy, read from the game's own catalogue ----

        [Serializable] private sealed class Hero { public string Id, Name, Description, Skill, SkillResource, SkillDescription; public string[] Approaches; }
        [Serializable] private sealed class Friend { public string Id, Name, Description, Rarity; public string[] Tags; }
        [Serializable] private sealed class Thing { public string Id, Name, Description; }
        [Serializable] private sealed class FactSheet
        {
            public int Heroes, Friends, Relics, Weapons, Enemies, Bosses, Duets;
            public List<Hero> HeroList = new List<Hero>(); public List<Friend> FriendList = new List<Friend>();
            public List<Thing> BossList = new List<Thing>(); public List<Thing> RelicList = new List<Thing>();
            public List<string> CompanionSkills = new List<string>();
        }
        private void Facts()
        {
            var facts = new FactSheet
            {
                Heroes = catalog.Champions.Count(HeroUnlocks.Listed), Friends = Friends.Length, Relics = Relics.Length,
                Weapons = catalog.Weapons.Length - catalog.Champions.Where(c => !HeroUnlocks.Listed(c) && c.Weapon != null).Select(c => c.Weapon).Distinct().Count(),
                Enemies = catalog.Enemies.Length, Bosses = 2, Duets = Duets.All.Count(), // the bosses the website shows
            };
            foreach (UnitDefinition c in catalog.Champions.Where(HeroUnlocks.Listed)) // heroes held for a later update or act stay a surprise
            {
                HeroIdentityDefinition id = HeroIdentities.All.FirstOrDefault(h => h.Id == c.Id);
                facts.HeroList.Add(new Hero { Id = c.Id, Name = c.DisplayName, Description = c.Description, Skill = id?.Name, SkillResource = id?.Resource, SkillDescription = id?.Description, Approaches = id?.Approaches });
            }
            foreach (UnitDefinition u in Friends) facts.FriendList.Add(new Friend { Id = u.Id, Name = u.DisplayName, Description = u.Description, Rarity = u.Rarity.ToString(), Tags = u.Tags });
            // only the first two bosses (the later ones stay a surprise): the Coin Widow from the catalogue, the Chair Porter from Act I's own bosses
            foreach (UnitDefinition b in catalog.Bosses.Where(b => b.Id == "CoinWidow")) facts.BossList.Add(new Thing { Id = b.Id, Name = b.DisplayName, Description = b.Description });
            facts.BossList.Add(new Thing { Id = "ChairPorter", Name = "The Chair Porter", Description = "Night 8. Carries a tower of chairs on his back, the travellers still sitting in them." });
            foreach (ItemDefinition i in Relics) facts.RelicList.Add(new Thing { Id = i.Id, Name = i.Name, Description = i.Description });
            facts.CompanionSkills.AddRange(CombatSimulation.CompanionSkillIds);
            File.WriteAllText(Path.Combine(root, "facts.json"), JsonUtility.ToJson(facts, true));
        }
    }

    /// <summary>Records the game's own picture (any window size) into an MP4 at 30 frames a second through ffmpeg, in real time.</summary>
    public sealed class WebsiteClipRecorder : MonoBehaviour
    {
        private readonly object sync = new object();
        private readonly Queue<byte[]> free = new Queue<byte[]>();
        private readonly Queue<(byte[] pixels, int frame)> packets = new Queue<(byte[], int)>();
        private System.Diagnostics.Process encoder;
        private Thread writer;
        private RenderTexture surface;
        private bool finishing; private volatile bool failed, writerDone;
        private int pending, encoded, goal, width, height;
        private double started;
        public bool Complete { get; private set; }
        public bool Passed => Complete && !failed && encoded == goal;

        public void Begin(string ffmpeg, string destination, float seconds)
        {
            width = Screen.width & ~1; height = Screen.height & ~1; goal = Mathf.RoundToInt(seconds * 30);
            var settings = new System.Diagnostics.ProcessStartInfo(ffmpeg, "-y -hide_banner -loglevel error -f rawvideo -pixel_format rgba -video_size " + width + "x" + height + " -framerate 30 -i pipe:0 -an -c:v libx264 -preset veryfast -crf 20 -pix_fmt yuv420p -movflags +faststart \"" + destination + "\"")
            { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardError = true };
            encoder = System.Diagnostics.Process.Start(settings); encoder.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) failed = true; }; encoder.BeginErrorReadLine();
            for (int i = 0; i < 6; i++) free.Enqueue(new byte[width * height * 4]);
            surface = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32); surface.Create();
            writer = new Thread(Write) { IsBackground = true }; writer.Start(); started = Time.realtimeSinceStartupAsDouble;
            StartCoroutine(Capture(seconds));
        }
        private IEnumerator Capture(float seconds)
        {
            var end = new WaitForEndOfFrame();
            while (Time.realtimeSinceStartupAsDouble - started < seconds && !failed)
            {
                yield return end;
                byte[] buffer = null; lock (sync) { if (free.Count > 0) buffer = free.Dequeue(); }
                if (buffer == null) continue;
                int frame = Mathf.Min(goal - 1, (int)Math.Floor((Time.realtimeSinceStartupAsDouble - started) * 30));
                ScreenCapture.CaptureScreenshotIntoRenderTexture(surface); pending++;
                AsyncGPUReadback.Request(surface, 0, TextureFormat.RGBA32, request =>
                {
                    pending--;
                    if (request.hasError) { failed = true; lock (sync) free.Enqueue(buffer); return; }
                    request.GetData<byte>().CopyTo(buffer);
                    lock (sync) { packets.Enqueue((buffer, frame)); Monitor.Pulse(sync); }
                });
            }
            while (pending > 0) yield return null;
            lock (sync) { finishing = true; Monitor.Pulse(sync); }
            while (!writerDone) yield return null;
            encoder.WaitForExit(); failed |= encoder.ExitCode != 0;
            surface.Release(); Destroy(surface); surface = null; Complete = true;
        }
        private void Write()
        {
            byte[] previous = null;
            try
            {
                Stream stream = encoder.StandardInput.BaseStream;
                while (true)
                {
                    (byte[] pixels, int frame) packet;
                    lock (sync)
                    {
                        while (packets.Count == 0 && !finishing) Monitor.Wait(sync);
                        if (packets.Count == 0 && finishing) break;
                        packet = packets.Dequeue();
                    }
                    while (encoded <= packet.frame && encoded < goal) { stream.Write(packet.pixels, 0, packet.pixels.Length); encoded++; }
                    lock (sync) { if (previous != null) free.Enqueue(previous); previous = packet.pixels; }
                }
                while (previous != null && encoded < goal) { stream.Write(previous, 0, previous.Length); encoded++; }
                encoder.StandardInput.Close();
            }
            catch { failed = true; }
            finally { writerDone = true; }
        }
        private void OnDestroy()
        {
            lock (sync) { finishing = true; Monitor.PulseAll(sync); }
            if (encoder != null && !encoder.HasExited && !Complete) encoder.Kill();
            if (surface != null) { surface.Release(); Destroy(surface); }
        }
    }
}
