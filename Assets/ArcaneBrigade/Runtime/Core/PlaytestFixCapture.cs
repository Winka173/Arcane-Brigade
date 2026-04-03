using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    /// <summary>One named screenshot per play-test fix (Docs/Playtest_Review_2026-10-20.md), taken in the Windows player:
    /// -arcane-playtest-capture -capturePath DIR. Each shot is named after its review item (Item01_HeroSelect, ...).
    /// A failed step is recorded and the capture goes on to the next shot.</summary>
    public sealed class PlaytestFixCapture : MonoBehaviour
    {
        public static bool Requested => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-playtest-capture") >= 0;
        private readonly List<string> checks = new List<string>();
        private string root; private RunCatalog catalog; private ArenaController owner;
        private RunSnapshot summary; private bool drawSummary;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            if (!Requested || FindAnyObjectByType<PlaytestFixCapture>() != null) return;
            var go = new GameObject("PlaytestFixCapture"); DontDestroyOnLoad(go); go.AddComponent<PlaytestFixCapture>();
        }
        private void Check(bool pass, string label) { checks.Add((pass ? "PASS " : "FAIL ") + label); File.WriteAllLines(Path.Combine(root, "Checks.txt"), checks); }
        private IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture();
            Check(image.width == Screen.width && image.height == Screen.height, "Shot " + name);
            File.WriteAllBytes(Path.Combine(root, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        private RunHud Hud => (RunHud)typeof(ArenaController).GetField("runHud", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(owner);

        private IEnumerator LoadShop(int wave, bool special, Action<RunSession> setup = null)
        {
            using (var run = new RunSession(catalog))
            {
                run.Start("LanternKnight", 20261020); run.State.Wave = wave; run.State.Phase = SessionPhase.Shop; run.State.Gold = 4000; run.State.Eggs = 30;
                run.State.MienCho.ReadMask = (1 << MienChoStory.Available(run.State)) - 1;
                run.State.ActOne ??= new ActOneProgress(); if (wave >= 8) run.State.ActOne.Offer = ActOneProgress.Refused;
                foreach (var u in catalog.Companions.Where(u => !u.GiftOnly && !u.SpecialRecruit && u.Weapon != null).Take(7))
                    run.State.Roster.Add(new RosterEntry { UnitId = u.Id, WeaponId = u.Weapon.Id, Tier = run.State.Roster.Count % 3 == 0 ? 2 : 1 });
                run.State.Offers.Clear(); run.State.SpecialOffers.Clear(); run.State.SpecialShopVisited = !special;
                var stock = special ? run.State.SpecialOffers : run.State.Offers;
                var tradeoff = catalog.Items.First(i => i.ShopWeight > 0 && i.EffectiveModifiers.Any(m => m.Value < 0) && i.EffectiveModifiers.Any(m => m.Value > 0));
                var plain = catalog.Items.First(i => i.ShopWeight > 0 && i.Mechanic != RelicMechanic.None && i.EffectiveModifiers.Any(m => m.Value > 0));
                // the friend on offer completes a duet with one already in the brigade (item 26: role badge, COMBO chip)
                var duet = Duets.All.First(d => catalog.Unit(d.A) is UnitDefinition a && catalog.Unit(d.B) is UnitDefinition b && !a.GiftOnly && !b.GiftOnly && a.Weapon != null && b.Weapon != null);
                var friend = catalog.Unit(duet.A); var partner = catalog.Unit(duet.B);
                if (!run.State.Roster.Exists(r => r.UnitId == partner.Id)) run.State.Roster.Add(new RosterEntry { UnitId = partner.Id, WeaponId = partner.Weapon.Id, Tier = 1 });
                run.State.Roster.RemoveAll(r => r.UnitId == friend.Id);
                stock.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Item, Id = tradeoff.Id, Price = special ? 6 : 40, BasePrice = special ? 6 : 40, Currency = special ? ShopCurrency.Health : ShopCurrency.Gold });
                stock.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Item, Id = plain.Id, Price = special ? 3 : 30, BasePrice = special ? 3 : 30, Currency = special ? ShopCurrency.Eggs : ShopCurrency.Gold });
                stock.Add(new ShopOffer { Token = ++run.State.NextOffer, Kind = OfferKind.Companion, Id = friend.Id, Tier = 1, Price = 24, BasePrice = 24, Currency = ShopCurrency.Gold });
                setup?.Invoke(run);
                new RunStorage(RunPaths.Root).SaveRun(run.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3);
            owner = FindAnyObjectByType<ArenaController>();
            Check(owner != null && owner.Session.State.Phase == SessionPhase.Shop, "Shop loads at wave " + wave);
        }
        private IEnumerator Stand(Vector2 at)
        {
            owner.GetComponent<ShopInspect>()?.Close();
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = at;
            FindAnyObjectByType<ArenaCamera>()?.Snap(); yield return new WaitForSecondsRealtime(.6f);
        }
        private IEnumerator Focus(int slot) { var stations = owner.Shop.Stations.Where(s => s.Offer != null).ToList(); if (slot >= stations.Count) { Check(false, "Offer slot " + slot); yield break; } var station = stations[slot]; yield return Stand(station.Position + new Vector2(0, -.3f)); }

        private IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-capturePath");
            if (at < 0 || at + 1 >= args.Length) { Application.Quit(1); yield break; }
            root = Path.GetFullPath(args[at + 1]); Directory.CreateDirectory(root); Application.runInBackground = true;
            catalog = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            storage.SaveProfile(new PlayerProfile { TutorialSeen = true, BestWave = 40 });

            // 01: hero select without the modes row
            yield return new WaitForSecondsRealtime(2);
            RunMenu menu = FindAnyObjectByType<RunMenu>();
            if (menu != null) { menu.ShowPage(RunMenu.Page.Roster); yield return new WaitForSecondsRealtime(.6f); yield return Shot("Item01_HeroSelect"); }
            else Check(false, "Menu found");

            // first shop: the what-now line and the first tip (04), the friend and relic cards (03/11), a stat explained (11)
            yield return LoadShop(1, false);
            yield return new WaitForSecondsRealtime(1.2f); yield return Shot("Item04_ShopHintAndTip");
            yield return Focus(0); yield return Shot("Item03_11_RelicCard_GivesCosts");
            yield return Focus(2); yield return Shot("Item03_11_FriendCard"); yield return Shot("Item26_FriendCard_RoleCombo");
            // 12/25: the refresh station: its price (about 8% of the shop's offers) and the new-friend pace
            var refresh = owner.Shop.Stations.FirstOrDefault(s => s.Action == "REROLL");
            if (refresh != null) { yield return Stand(refresh.Position + new Vector2(0, -.3f)); yield return Shot("Item12_25_RefreshAndFriendPace"); }
            yield return Focus(0); owner.SetShopDetails(true); GlossaryUI.Show(RunModifierRules.ModifierName(ItemEffect.Health)); yield return new WaitForSecondsRealtime(.3f);
            yield return Shot("Item11_TapStatExplains");
            typeof(RunHud).GetField("shopScroll", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, new Vector2(0, 160)); GlossaryUI.Close();
            yield return new WaitForSecondsRealtime(.2f); yield return Shot("Item19_ScrollPanel"); owner.SetShopDetails(false);
            // 04: the goal card and How to play
            Hud.Guide.ShowGoalForCapture(); yield return new WaitForSecondsRealtime(.6f); yield return Shot("Item04_GoalCard"); typeof(PlayerGuide).GetField("goalAt", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud.Guide, -100f);
            owner.TogglePause(); typeof(RunHud).GetField("pausePage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, 3);
            yield return new WaitForSecondsRealtime(.3f); yield return Shot("Item04_HowToPlay");
            typeof(RunHud).GetField("pausePage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, 4);
            yield return new WaitForSecondsRealtime(.3f); yield return Shot("Item26_CombosPage");
            typeof(RunHud).GetField("pausePage", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(Hud, 0); owner.TogglePause();
            // 06/14: the hero behind a stall (the stall fades, the halo and show-through)
            var stall = owner.Shop.Stations.Where(s => s.Offer != null).Skip(1).FirstOrDefault() ?? owner.Shop.Stations.First();
            yield return Stand(stall.Position + new Vector2(0, .55f)); yield return new WaitForSecondsRealtime(.4f);
            yield return Shot("Item06_14_PropFade_HeroHalo");
            // 17: a visitor with a price: accept button and the price on the world tag
            owner.Session.ForceVisitor("Sandman"); yield return Stand(new Vector2(4.2f, -.4f)); yield return Shot("Item17_VisitorTag");
            owner.GetComponent<VisitorCard>().Show(); yield return new WaitForSecondsRealtime(.4f); yield return Shot("Item17_VisitorCard");
            owner.VisitorDecline(); yield return new WaitForSecondsRealtime(.2f);
            // 07: the Keeper's gifts as plates
            if (owner.Session.OfferPrismGift(true)) { yield return new WaitForSecondsRealtime(.8f); yield return Shot("Item07_KeeperGifts"); }
            else Check(false, "Keeper gift opens");

            // 10: the blood-price card in the dream shop
            yield return LoadShop(12, true);
            yield return Focus(0); yield return new WaitForSecondsRealtime(.6f); yield return Shot("Item10_BloodPriceCard");
            owner.SetShopDetails(true); yield return new WaitForSecondsRealtime(.3f); yield return Shot("Item10_BloodPriceDetails"); owner.SetShopDetails(false);
            yield return Focus(1); yield return Shot("Item16_DreamShopEggs");

            // 07: a secret event room as plates (the Impossible Tailor, a relic in the middle)
            bool saved = false; SecretEventKind shown = SecretEventKind.ImpossibleTailor;
            var wanted = new[] { SecretEventKind.ImpossibleTailor, SecretEventKind.ThirteenthHour, SecretEventKind.ContrabandTrain, SecretEventKind.Understudy, SecretEventKind.DeadLetters };
            for (int seed = 0; seed < 600 && !saved; seed++)
                using (var s = new RunSession(catalog))
                {
                    s.Start("LanternKnight", seed); s.State.Wave = 8; s.State.Phase = SessionPhase.Shop; s.State.Gold = 400; // secret shops: 3, 8, 13, 18
                    s.State.MienCho.ReadMask = (1 << MienChoStory.Available(s.State)) - 1; s.State.ActOne ??= new ActOneProgress(); s.State.ActOne.Offer = ActOneProgress.Refused;
                    s.State.Roster.Add(new RosterEntry { UnitId = catalog.Companions.First(u => u.Id != s.State.Roster[0].UnitId && !u.Id.StartsWith("Secret") && u.Rarity < ItemRarity.Crimson).Id, WeaponId = catalog.Weapons.First(w => w.Kind == WeaponKind.Projectile).Id });
                    foreach (var relic in catalog.Items.Where(i => !i.Id.StartsWith("Secret") && i.Rarity < ItemRarity.Crimson && i.EffectiveModifiers.Any(m => m.Value > 0)).Take(3)) s.State.Items.Add(new ItemStack { Id = relic.Id, Count = 1 });
                    var portals = s.SecretPortals(); if (portals.Length == 0) continue;
                    shown = wanted.FirstOrDefault(k => Array.IndexOf(portals, k) >= 0); if (Array.IndexOf(portals, shown) < 0) { if (seed < 400) continue; shown = portals[0]; }
                    storage.SaveRun(s.State); saved = true;
                }
            if (saved)
            {
                RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(2.4f);
                owner = FindAnyObjectByType<ArenaController>();
                Check(owner.EnterSecret(shown), "Secret room opens: " + shown); yield return new WaitForSecondsRealtime(.5f);
                yield return Shot("Item07_SecretEventPlates");
            }
            else Check(false, "Secret portal seed");

            // the fight: 05 (no hit stop), 08 (ground coins), 09 (heart on the HP bar), 14 (hero in a crowd), 29 (chest popup)
            yield return LoadShop(5, false);
            owner.NextWave(); yield return new WaitForSecondsRealtime(1.5f);
            yield return Shot("Item21_LongerWaveTimer"); yield return new WaitForSecondsRealtime(4.5f);
            yield return Shot("Item05_Combat_NoHitStop");
            Vector2 hero = owner.Simulation.Champion.Position;
            for (int i = 0; i < 8; i++) owner.Simulation.AddLootPresentationFixture(LootKind.Coin, hero + new Vector2(1.2f + i * .35f, -.6f + (i % 2) * .3f), 3);
            yield return new WaitForSecondsRealtime(.5f); yield return Shot("Item08_09_GroundCoins_HPBarHeart");
            owner.GetComponent<PacingFeedback>()?.Observe(new[] { new CombatEvent(CombatEventKind.ChestOpened, hero + Vector2.right, Faction.Champion, 240, new Vector2(1, 12)) }, owner.PresentationClock);
            yield return new WaitForSecondsRealtime(1.2f); yield return Shot("Item29_ChestPopup");
            yield return new WaitForSecondsRealtime(2f); yield return Shot("Item14_HeroInCrowd");

            // 32: the run summary as the results screen draws it
            summary = JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(owner.Session.State));
            summary.Phase = SessionPhase.Victory; summary.Wave = 20; summary.TotalKills = 12523; summary.BossKills = 4; summary.CombatSeconds = 1415;
            summary.Contributions.Clear();
            summary.Contributions.Add(new ContributionEntry { UnitId = summary.ChampionId, WeaponId = "x", EffectId = "primary", Tier = 1, Damage = 1810087, Kills = 5200, Coins = 2400 });
            float share = 900000;
            foreach (RosterEntry r in summary.Roster) { summary.Contributions.Add(new ContributionEntry { UnitId = r.UnitId, WeaponId = r.WeaponId, EffectId = "primary", Tier = r.Tier, Damage = share, Kills = (int)(share / 400) }); share *= .7f; }
            if (summary.Items.Count == 0) foreach (var relic in catalog.Items.Where(i => i.ShopWeight > 0).Take(5)) summary.Items.Add(new ItemStack { Id = relic.Id, Count = 1 });
            drawSummary = true; yield return new WaitForSecondsRealtime(.5f); yield return Shot("Item32_RunSummary"); drawSummary = false;

            // 30: the dawn page, read line by line
            using (var s = new RunSession(catalog))
            {
                s.Start("LanternKnight", 7); s.State.Wave = 20; s.State.Phase = SessionPhase.Victory;
                s.State.Nightmare ??= new NightmareProgress();
                s.State.Nightmare.Ending = NightmareEndingStage.Dawn; s.State.Nightmare.Farewell = NightmareFarewell.CloseShop; s.State.Nightmare.EndingPage = 2; s.State.Nightmare.CustomerDefeated = true;
                try { storage.SaveRun(s.State); } catch (Exception e) { Check(false, "Ending save: " + e.Message); }
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return new WaitForSecondsRealtime(3.5f);
            owner = FindAnyObjectByType<ArenaController>();
            Check(owner != null && owner.NightmareEndingVisible, "Dawn page shows");
            yield return Shot("Item30_EndingLine1");
            for (int i = 0; i < 2; i++) { typeof(NightmarePresentation).GetMethod("RevealNextLine", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(owner.GetComponent<NightmarePresentation>(), null); yield return new WaitForSecondsRealtime(.5f); }
            yield return Shot("Item30_EndingLine3");

            Check(true, "Capture finished"); Application.Quit(checks.All(c => c.StartsWith("PASS ")) ? 0 : 1);
        }

        private readonly RunInvoice invoice = new RunInvoice();
        private void OnGUI()
        {
            if (!drawSummary || summary == null) return;
            GUI.depth = -200; var previous = GUI.matrix; GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1));
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(.03f, .03f, .06f, 1));
            invoice.Draw(new Rect(430, 120, 1060, 860), summary, catalog);
            GUI.matrix = previous;
        }
    }
}
