using System;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K4MergeChecks
    {
        [MenuItem("Arcane Brigade/K4/Check merge reveal and personality")]
        public static void Run()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            Require(catalog != null && catalog.Weapons.Length >= 3, "Catalog fixture available");
            using (var session = new RunSession(catalog))
            {
                session.Start(catalog.Champions[0].Id, 4313);
                session.State.Phase = SessionPhase.Shop; session.State.Gold = 10000;
                session.State.Roster.Clear();
                string unitId = catalog.Companions[0].Id;
                for (int index = 0; index < 2; index++) session.State.Roster.Add(new RosterEntry { UnitId = unitId, WeaponId = catalog.Weapons[index].Id });
                session.State.Offers.Add(new ShopOffer { Token = 1, Id = unitId, Kind = OfferKind.Companion, Price = 1 });
                session.State.NextOffer = 1;
                RunSnapshot before = MergeReveal.Capture(session.State);
                string original = JsonUtility.ToJson(session.State);
                bool saved = false;
                bool committed = RunTransaction.TryCommit(session, catalog, "K4 merge", () => session.Buy(1), () => { saved = true; return false; }, out string error);
                Require(saved && !committed && original == JsonUtility.ToJson(session.State), "Save failure restores currency roster RNG and receipts");
                Require(MergeReveal.CommittedBeats(before, session.State, committed, Vector2.zero, false).Count == 0, "Rollback has no reveal");
                committed = RunTransaction.TryCommit(session, catalog, "K4 merge", () => session.Buy(1), () => true, out error);
                Require(committed, "Actual recruit merge commits: " + error);
                string after = JsonUtility.ToJson(session.State);
                var beats = MergeReveal.CommittedBeats(before, session.State, committed, Vector2.zero, false);
                Require(beats.Count == 1 && beats[0].Result.Tier == 2 && beats[0].Result.WeaponId == session.State.Roster[0].WeaponId && beats[0].Materials.Count == 2, "Reveal uses actual rolled result and pre-purchase materials");
                var host = new GameObject("K4MergeCheck");
                try
                {
                    MergeReveal reveal = host.AddComponent<MergeReveal>();
                    reveal.EnqueueCommitted(before, session.State, true, Vector2.zero, false);
                    reveal.EnqueueCommitted(before, session.State, true, Vector2.zero, false);
                    Require(reveal.PendingCount == 1, "Duplicate receipt does not enqueue twice");
                    reveal.Skip(); reveal.EnqueueCommitted(before, session.State, true, Vector2.zero, false);
                    Require(reveal.PendingCount == 0 && reveal.CompletedCount == 0 && after == JsonUtility.ToJson(session.State), "Skip cannot replay consume roll or claim completed presentation");
                    RunSnapshot next = MergeReveal.Capture(session.State);
                    next.NextReceipt++; next.Receipts.Add(new RunTransactionReceipt { Id = next.NextReceipt, Title = "fixture", Hall = next.Wave });
                    next.Roster.Clear();
                    next.Roster.Add(new RosterEntry { UnitId = unitId, WeaponId = catalog.Weapons[0].Id, Tier = 3 });
                    RunSnapshot nextBefore = MergeReveal.Capture(session.State);
                    nextBefore.Roster.Clear();
                    for (int index = 0; index < 2; index++) nextBefore.Roster.Add(new RosterEntry { UnitId = unitId, WeaponId = catalog.Weapons[index].Id, Tier = 2 });
                    reveal.ReadOnly = true;
                    reveal.EnqueueCommitted(nextBefore, next, true, Vector2.zero, false);
                    reveal.ReadOnly = false;
                    reveal.EnqueueCommitted(nextBefore, next, true, Vector2.zero, false);
                    Require(reveal.PendingCount == 0, "Read-only skips permanently without later replay");
                    Require(MergeReveal.CommittedBeats(session.State, MergeReveal.Capture(session.State), true, Vector2.zero, false).Count == 0, "Reload snapshot does not reconstruct reveal");
                }
                finally { UnityEngine.Object.DestroyImmediate(host); }
                beats[0].Result.WeaponId = "visual-only";
                Require(after == JsonUtility.ToJson(session.State), "Beat payload is detached from gameplay");
                RunSnapshot altar = MergeReveal.Capture(before);
                altar.NextReceipt++; altar.Receipts.Add(new RunTransactionReceipt { Id = altar.NextReceipt });
                altar.Roster.Clear(); altar.Roster.Add(new RosterEntry { UnitId = catalog.Companions[1].Id, WeaponId = catalog.Weapons[0].Id, Tier = 1 });
                Require(MergeReveal.CommittedBeats(before, altar, true, Vector2.zero, true).Count == 1, "Altar reveal supports two materials and tier-one result");
            }
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                Require(UnitVisual.TierWeaponShape(weapon, 1) == Vector2.one && UnitVisual.TierWeaponShape(weapon, 2) != Vector2.one, "Tier silhouette without particle " + weapon.Id);
            }
            var unit = new UnitState { Id = 1, Faction = Faction.Companion };
            float beatTime = 6.4f - 0.73f + 0.325f;
            Require(UnitVisual.IdleBeat(unit, beatTime, false, false) > 0.9f, "Short deterministic idle beat");
            Require(UnitVisual.IdleBeat(unit, beatTime, true, false) == 0 && UnitVisual.IdleBeat(unit, beatTime, false, true) == 0, "Attack and reduced motion suppress idle");
            unit.PendingAttack = true;
            Require(UnitVisual.IdleBeat(unit, beatTime, false, false) == 0, "Pending attack suppresses personality");
            unit.Faction = Faction.Enemy; unit.ExposedUntil = 3;
            Require(UnitVisual.IsWeakspotVisible(unit, 2), "Live exposed enemy has mint weakspot cue");
            Require(!UnitVisual.IsWeakspotVisible(unit, 3), "Weakspot cue expires with gameplay exposure");
            unit.TelegraphUntil = 4;
            Require(!UnitVisual.IsWeakspotVisible(unit, 2), "Danger telegraph takes priority over weakspot cue");
            unit.TelegraphUntil = 0; unit.Alive = false;
            Require(!UnitVisual.IsWeakspotVisible(unit, 2), "Dead enemy never retains weakspot cue");
            unit.Alive = true; unit.Faction = Faction.Companion;
            Require(!UnitVisual.IsWeakspotVisible(unit, 2), "Weakspot cue never decorates allies");
            Debug.Log("K4 merge checks PASS (no build or asset writes)");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("K4 merge check failed: " + message);
        }
    }
}
