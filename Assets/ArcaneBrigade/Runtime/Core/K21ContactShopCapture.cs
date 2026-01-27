using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ArcaneBrigade
{
    public static class K21ContactShopCapture
    {
        public static IEnumerator Run(Action<bool, string> check, Func<string, IEnumerator> capture)
        {
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            var storage = new RunStorage(RunPaths.Root);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 21077); session.State.Roster.Clear(); storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
            ArenaController owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            owner.Session.CombatCatalog.EnemyBudget = 0; owner.Simulation.Champion.NextAttack = float.PositiveInfinity;
            owner.SetPresentationMoveTarget(Vector2.zero);
            UnitDefinition contact = UnityEngine.Object.Instantiate(owner.Session.CombatCatalog.MeleeEnemy);
            contact.Speed = 0; contact.ContactDamage = 6; contact.Health = 100;
            UnitState enemy = owner.Simulation.AddExpandedPresentationFixture(contact, new Vector2(0.1f, 0), 100);
            enemy.NextAttack = float.PositiveInfinity;
            float health = owner.Simulation.Champion.Health;
            yield return new WaitForSecondsRealtime(0.4f);
            check(enemy.Health < 100 && owner.Simulation.Champion.Health < health, "Native collision hurts champion and enemy");
            check(owner.Simulation.Contributions.Any(row => row.UnitId == owner.Simulation.Champion.Definition.Id && row.Damage > 0), "Native collision damage credited to champion");
            yield return capture("Collision-both-sides");
            SceneManager.LoadScene("RunFrontend"); yield return null; UnityEngine.Object.Destroy(contact);
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 21077); session.State.Wave = 4; session.State.Phase = SessionPhase.Shop; session.State.Gold = 200;
                session.Reroll(); session.State.RerollsThisShop = 0; session.State.SpecialOffers.Clear(); storage.SaveRun(session.State);
            }
            RunLaunch.Resume = true; SceneManager.LoadScene("RunArena"); yield return null;
            owner = UnityEngine.Object.FindAnyObjectByType<ArenaController>();
            yield return new WaitForSecondsRealtime(3.5f);
            WorldShop.Station offer = owner.Shop.Stations.First(station => station.Offer != null);
            Vector2 focus = offer.Position + Vector2.down * 0.5f;
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = focus; owner.SetPresentationMoveTarget(focus);
            yield return new WaitForSecondsRealtime(0.3f);
            check(owner.Shop.Nearby != null && owner.Shop.Nearby.Offer != null, "Native stock proximity opens description");
            yield return capture("Shop-description-below-price");
            WorldShop.Station reroll = owner.Shop.Stations.First(station => station.Action == "REROLL");
            check(reroll.Price == 2 && Vector2.Distance(reroll.Position, offer.Position) < 3, "Reroll station nearby with two-gold base price");
            focus = reroll.Position + Vector2.down * 0.4f;
            owner.Simulation.Champion.Position = owner.Simulation.Champion.PreviousPosition = focus; owner.SetPresentationMoveTarget(focus);
            yield return new WaitForSecondsRealtime(0.3f);
            check(owner.Shop.Nearby != null && owner.Shop.Nearby.Action == "REROLL", "Native reroll station focuses without overlapping stock");
            yield return capture("Shop-reroll-two-gold");
            int gold = owner.Session.State.Gold; owner.Shop.Interact();
            yield return new WaitForSecondsRealtime(0.5f);
            check(owner.Session.State.Gold == gold - 2 && owner.Session.State.RerollsThisShop == 1, "Native reroll pays exactly two gold");
            yield return capture("Shop-reroll-after-purchase");
        }
    }
}
