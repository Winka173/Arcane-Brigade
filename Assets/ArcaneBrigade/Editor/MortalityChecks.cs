using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class MortalityChecks
    {
        private const int EnemyCount = 100;
        private const float Health = 100f, Hit = 10f, BeforeRise = 14f, FinalSecond = 1f;
        private const float ShieldShare = .2f, Reduction = .5f, ProtectionDuration = 4f;
        private const float AreaRadius = 5f, ShortenRise = 5f;
        private static void Require(bool condition, string message)
        { if (!condition) throw new InvalidOperationException("Mortality: " + message); }
        private static void Near(float actual, float expected, string message)
        { Require(Mathf.Abs(actual - expected) < .001f, message); }

        public static void Run()
        {
            UnitDefinition definition = ScriptableObject.CreateInstance<UnitDefinition>();
            try
            {
                Near(FriendDefenseTuning.Armor("TowerRook",1),12,"Guard base armor");
                Near(FriendDefenseTuning.Armor("PumpkinReaper",1),6,"Striker base armor");
                Near(FriendDefenseTuning.Armor("BagpipeOctopus",1),5,"Controller base armor");
                Near(FriendDefenseTuning.Armor("ClockworkPiper",1),4,"Summoner base armor");
                Near(FriendDefenseTuning.Armor("NurseHen",1),3,"Support base armor");
                Near(FriendDefenseTuning.Armor("TeapotPilgrim",1),2,"Shooter base armor");
                Near(FriendDefenseTuning.Armor("BoneXylophone",1),2,"Bone Xylophone shoots: every house holds every class");
                Near(FriendDefenseTuning.Health("TowerRook",3),1.6f*1.8f,"class health times additive star health");
                Near(FriendDefenseTuning.Armor("TowerRook",3),16,"two armor per extra star");
                Near(FriendDefenseTuning.SharedItemPoints(1.5f),7,"half armor-item points round down");
                Near(RunModifierRules.AfterArmor(100,15*RunModifierRules.ArmorPoint),50,"15 points halve friend damage");
                Near(RunModifierRules.AfterArmor(100,1000*RunModifierRules.ArmorPoint),25,"friend armor retains quarter hit");
                definition.Health = Health;
                UnitState hero = Unit(1, Faction.Champion, definition);
                UnitState friend = Unit(2, Faction.Companion, definition);
                var units = new List<UnitState> { hero, friend };
                for (int i = 0; i < EnemyCount; i++) units.Add(Unit(i + 3, Faction.Enemy, definition));
                var runtime = new MortalityRuntime();
                int fell = 0, rose = 0, lost = 0;
                runtime.Fell += value => { fell++; Require(value.FriendId == hero.Id || value.FriendId == friend.Id, "only friends fall"); };
                runtime.Rose += value => { rose++; Near(value.Health, Health * MortalityTuning.RiseHealthFraction, "rise payload health"); };
                runtime.WaveLost += value => { lost++; Require(value.FallenCount == 2, "loss counts deployed friends, excludes 100 enemies"); };

                runtime.ResetWave(units);
                Require(runtime.FriendCount == 2 && runtime.LivingCount == 2, "100 enemies do not become friends");
                Near(friend.Health, Health, "companions have finite health");
                for (int i = 0; i < EnemyCount; i++) runtime.ApplyDamage(friend, Hit, units[i + 2].Id);
                Require(fell == 1 && !friend.Alive && hero.Alive && !runtime.Lost, "100 hits cause exactly one fall, hero keeps wave alive");
                Require(!friend.PendingAttack && runtime.IsFallen(friend), "fall cancels attacks");
                runtime.Tick(BeforeRise);
                Require(!friend.Alive && rose == 0, "no early rise");
                runtime.Tick(FinalSecond);
                Require(friend.Alive && rose == 1 && runtime.LivingCount == 2, "rise at exactly fifteen seconds");
                Near(friend.Health, Health * MortalityTuning.RiseHealthFraction, "rise at forty percent");
                runtime.ApplyDamage(hero, Health);
                Require(!runtime.Lost && !hero.Alive && friend.Alive, "hero fall alone does not lose wave");
                runtime.Tick(MortalityTuning.RiseSeconds);
                Require(hero.Alive && rose == 2, "hero rises under same friend rule");
                runtime.ApplyDamage(friend, Health);
                runtime.ApplyDamage(hero, Health);
                Require(runtime.Lost && lost == 1 && runtime.LivingCount == 0, "all friends down immediately loses wave");
                runtime.Tick(MortalityTuning.RiseSeconds);
                Require(!hero.Alive && !friend.Alive && lost == 1, "lost wave cannot revive itself");
                runtime.ResetWave(units);
                Require(hero.Alive && friend.Alive && !runtime.Lost, "next wave restores fallen team");
                Near(hero.Health, Health, "hero full next wave"); Near(friend.Health, Health, "friend full next wave");
                runtime.Shield(friend, ShieldShare, ProtectionDuration);
                runtime.ReduceDamage(friend, Reduction, ProtectionDuration);
                runtime.ApplyDamage(friend, Health);
                Near(friend.Health, Health - (Health * (1 - Reduction) - Health * ShieldShare), "shield and damage reduction protect health");
                runtime.Tick(ProtectionDuration);
                runtime.ApplyDamage(friend, Health);
                Require(!friend.Alive, "protection expires");
                runtime.ReviveSooner(friend, ShortenRise);
                runtime.Tick(MortalityTuning.RiseSeconds - ShortenRise);
                Require(friend.Alive, "protection skills accelerate revival");
                runtime.ApplyDamage(friend, Health);
                Require(runtime.ReviveNow(friend), "instant protection revival");
                Near(friend.Health, Health * MortalityTuning.RiseHealthFraction, "instant revival health");
                Require(runtime.WeakestFriend() == friend, "hunter picks weakest health share");
                runtime.ResetWave(units);
                runtime.DamageArea(Vector2.zero, AreaRadius, Health);
                Require(runtime.Lost && lost == 2, "enemy area damage strikes entire friend cluster");
                runtime.ResetWave(units);
                Near(runtime.ApplyDamage(friend, float.NaN), 0, "invalid damage ignored");
                Near(runtime.ApplyDamage(friend, float.PositiveInfinity), 0, "infinite damage ignored");
                runtime.ApplyDamage(friend, Hit);
                Near(runtime.Heal(friend, Health), Hit, "healing is capped by max health");
                Debug.Log("MortalityChecks: PASS (100-enemy fixture; fall/rise/loss/reset/protection/area damage)");
            }
            finally { UnityEngine.Object.DestroyImmediate(definition); }
        }

        private static UnitState Unit(int id, Faction faction, UnitDefinition definition)
        { return new UnitState { Id = id, Faction = faction, Definition = definition, Health = Health, Position = Vector2.zero }; }
    }
}
