using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    public static class ShopFlowChecks
    {
        public static void Run()
        {
            var notes=new List<string>();
            void Check(bool ok,string label){notes.Add((ok?"PASS ":"FAIL ")+label);Directory.CreateDirectory("PreviewExports/ShopFlow");File.WriteAllLines("PreviewExports/ShopFlow/Checks.txt",notes);if(!ok)throw new InvalidOperationException(label);}
            var c=Resources.Load<RunCatalog>("RunCatalog");
            Check(ShopStatFeedback.Number(.25)=="+<1" && ShopStatFeedback.Number(-.25)=="-<1" && ShopStatFeedback.Number(4.66)=="+5","Stat figures use integers and preserve sub-unit direction");
            foreach(int count in new[]{15,20,40})
            using(var s=new RunSession(c))
            {
                s.Start("PatchworkHost",161026);s.State.Wave=8;s.State.Phase=SessionPhase.Shop;s.State.Gold=100000;s.State.Roster.Clear();s.State.Offers.Clear();s.State.SpecialOffers.Clear();
                var candidate=c.Companions.First(u=>!u.GiftOnly && !u.SpecialRecruit && u.UnlockWave<=8 && Array.Exists(c.Weapons,w=>s.RecruitWeaponEligible(u.Id,w)));
                foreach(var unit in c.Companions.Where(u=>u.Id!=candidate.Id && u.Rarity!=ItemRarity.Rainbow).Take(count))s.State.Roster.Add(new RosterEntry{UnitId=unit.Id,WeaponId=unit.Weapon.Id});
                var offer=new ShopOffer{Token=++s.State.NextOffer,Id=candidate.Id,Kind=OfferKind.Companion,Price=1};s.State.Offers.Add(offer);
                Check(s.Buy(offer.Token) && s.State.Roster.Count==count+1,"Normal companion can join above the former body cap "+count);
                s.State.Offers.Clear();s.State.SpecialOffers.Clear();s.Restore(JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(s.State)));
                Check(s.State.Roster.Count==count+1,"Larger roster survives save/load "+count);
                s.State.OutsideStage=OutsideStageStatus.Reward;s.State.OutsideRewards=new List<string>{"RainbowTailor","RainbowCrown","RainbowHeart"};
                Check(s.ClaimOutsideStageReward(0) && s.State.Roster.Count==count+2,"Rainbow companion joins larger roster "+count);
                Check(!s.ClaimOutsideStageReward(0),"Rainbow reward cannot be claimed twice "+count);
            }
            using(var s=new RunSession(c))
            {
                s.Start("LanternKnight",161026);s.State.Wave=13;s.State.Phase=SessionPhase.Shop;s.State.OutsideTransfers=3;s.AcceptOutsideStage();
                var arena=s.PrepareCombat();var source=OutsideStageAssets.Current.Boss;
                Check(Mathf.Abs(arena.Boss.BodyHeight/source.BodyHeight-2.45f)<.001f,"Keeper body is 40 percent larger than previous 1.75 scale");
                Check(Mathf.Abs(arena.Boss.Health/(source.Health*20*.8f)-1.8f)<.001f,"Keeper has 80 percent more HP than previous build at equal settings");
                var sim=new CombatSimulation(arena);sim.Start(161026);sim.Champion.NextAttack=float.PositiveInfinity;
                bool summoned=false;int most=0;
                for(int tick=0;tick<800;tick++)
                {sim.Champion.Health=sim.Champion.Definition.Health;sim.Step(.05f,Vector2.zero);int guards=sim.Units.Count(u=>u.Alive && u.Faction==Faction.Enemy && u!=sim.Boss);most=Mathf.Max(most,guards);summoned|=guards>0;}
                Check(summoned && most<=12,"Live Keeper summons bounded guards without ordinary arena spawns");
                Check(OutsideStageChoice.ChallengePosition.y<-3 && OutsideStageChoice.ChallengePosition.x>3,"Challenge landmark moved below the bond HUD");
            }
            Debug.Log("SHOP_FLOW_CHECKS "+notes.Count+" PASS");
        }
    }
}
