using System.Collections;
using System.Linq;
using UnityEngine;
namespace ArcaneBrigade
{
    public sealed partial class CombatRepairCapture
    {
        private IEnumerator ShopFlow()
        {
            yield return Load(4,true);yield return new WaitForSecondsRealtime(2.4f);
            float landing=ShopSceneryLanding.Active.StartedAt;
            Check(!ShopSceneryLanding.Active.Settling,"Initial shop landing finishes");
            for(int i=0;i<3;i++)
            {
                Check(owner.RerollShop(),"Real committed reroll succeeds "+i);yield return null;
                Check(ShopSceneryLanding.Active.StartedAt==landing && !ShopSceneryLanding.Active.Settling,"Reroll cannot replay scenery landing "+i);
            }
            var positive=new ShopOffer{Token=++owner.Session.State.NextOffer,Id="key-oil",Kind=OfferKind.Item,Price=2};
            owner.Session.State.Offers.Clear();owner.Session.State.Offers.Add(positive);owner.Shop.Display();
            Check(owner.BuyOffer(positive.Token),"Real positive-stat purchase succeeds");yield return new WaitForSecondsRealtime(.2f);
            Check(ShopSceneryLanding.Active.StartedAt==landing && !ShopSceneryLanding.Active.Settling,"Purchase cannot replay scenery landing");
            Check(owner.Session.LastShopStatChanges.Any(d=>d.ActorKey=="hero" && d.Label=="Damage" && d.Positive) && owner.Session.LastShopStatChanges.Any(d=>d.ActorKey!="hero" && d.Label=="Damage" && d.Positive),"Actual damage gains reported on hero and affected companions");
            yield return Shot("01-floating-stat-gains");
            yield return new WaitForSecondsRealtime(3.4f);
            var negative=catalog.Items.First(i=>i.EffectiveModifiers.Any(m=>m.Value<0));
            var reduction=new ShopOffer{Token=++owner.Session.State.NextOffer,Id=negative.Id,Kind=OfferKind.Item,Price=2};
            owner.Session.State.Offers.Clear();owner.Session.State.Offers.Add(reduction);owner.Shop.Display();
            Check(owner.BuyOffer(reduction.Token),"Real tradeoff-item purchase succeeds");yield return new WaitForSecondsRealtime(.18f);
            Check(owner.Session.LastShopStatChanges.Any(d=>d.Label!="Gold" && !d.Neutral && !d.Positive),"Negative stat changes are preserved");
            yield return Shot("02-floating-stat-tradeoff");
            owner.TogglePause();yield return new WaitForSecondsRealtime(.15f);owner.TogglePause();yield return null;
            Check(ShopSceneryLanding.Active.StartedAt==landing,"Pause and resume do not replay scenery landing");
            var interaction=owner.GetComponent<WaitingArchiveInteractions>();
            owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=new Vector2(0,-3);
            Check(!interaction.TryInspect(0) && !interaction.TryInspect(1),"Distant props cannot open story panels");
            for(int i=0;i<2;i++)
            {
                Vector2 anchor=interaction.Anchor(i);var view=interaction.Prop(i);
                owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=anchor+Vector2.down*.7f;
                yield return new WaitForSecondsRealtime(.2f);
                Check(interaction.Near(i),"Floating story object is approachable "+i);
                Vector2 foot=view.transform.TransformPoint(WorldSpriteGrounding.Foot(view.sprite));
                Check(foot.y-anchor.y>.1f,"Interactable prop floats above its fixed anchor "+i);
                var camera=FindAnyObjectByType<ArenaCamera>().GetComponent<Camera>();Rect click=interaction.ClickRect(i,camera);
                Vector3 centre=camera.WorldToViewportPoint(view.bounds.center);
                Check(click.Contains(new Vector2(centre.x*1920,(1-centre.y)*1080)),"Click region follows actual floating sprite "+i);
                yield return Shot(i==0?"03-archive-cabinet-near":"05-meeting-chair-near");
                Check(interaction.TryInspect(i) && owner.StoryOpen,"Nearby object opens its linked content "+i);yield return null;
                yield return Shot(i==0?"04-journal-from-cabinet":"06-paths-from-chair");
                if(i==0)owner.GetComponent<MienChoPresentation>().Close();else owner.GetComponent<WaitingJourney>().Close();
                yield return null;
                Check(!owner.StoryOpen && interaction.Anchor(i)==anchor,"Closing preserves world anchor "+i);
            }
            yield return Load(5,true);
            Check(ShopSceneryLanding.Active.Settling,"A genuinely new shop visit still plays its landing");
            yield return Load(1);yield return new WaitForSecondsRealtime(2);
            yield return Shot("07-early-wave-light");
            yield return Load(17);yield return new WaitForSecondsRealtime(4.5f);
            Check(NightmareLighting.Active.Ambient<.06f,"Late ambient becomes substantially darker");
            Check(SharedLightField.Active.Strength(owner.Simulation.Champion.Position)>.95f,"Hero still has a complete usable pool of light");
            Check(owner.Simulation.Units.Where(u=>u.Faction!=Faction.Enemy).All(u=>SharedLightField.Active.Visibility(u,owner.Simulation.Elapsed)==1),"Darkness keeps hero and companions fully visible");
            yield return Shot("08-late-wave-deep-night");
            yield return Load(13,true);yield return new WaitForSecondsRealtime(2.4f);
            var rare=catalog.Items.First(i=>i.Rarity==ItemRarity.Rare && i.UnlockWave<=13);
            owner.Session.State.Offers.Clear();owner.Session.State.Offers.Add(new ShopOffer{Token=++owner.Session.State.NextOffer,Id=rare.Id,Kind=OfferKind.Item,Price=1});owner.Shop.Display();
            var station=owner.Shop.Stations.First(s=>s.Offer!=null);
            owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=station.Position+Vector2.down*.3f;yield return new WaitForSecondsRealtime(.25f);
            yield return Shot("09-rarity-description-frame");
            owner.Session.State.OutsideTransfers=3;
            owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=OutsideStageChoice.ChallengePosition+Vector2.down*.7f;yield return new WaitForSecondsRealtime(.3f);
            var choice=owner.GetComponent<OutsideStageChoice>();
            Check(choice.Focused,"New Keeper landmark is reachable below the bond HUD");
            yield return Shot("10-clear-keeper-challenge-landmark");
            Check(choice.Interact(),"One ordinary interaction accepts the Keeper challenge without holding");
            float until=Time.realtimeSinceStartup+20;
            while((owner.TransitionPending || owner.Simulation.Boss?.Definition.Id!="OutsideStageKeeper") && Time.realtimeSinceStartup<until)yield return null;
            Check(owner.Simulation.Boss?.Definition.Id=="OutsideStageKeeper","Real challenge transition reaches Keeper arena");
            owner.Simulation.PrepareMienChoCapture(catalog);
            yield return new WaitForSecondsRealtime(7);
            Check(owner.Simulation.Units.Any(u=>u.Alive && u.Faction==Faction.Enemy && u!=owner.Simulation.Boss),"Actual Keeper calls guards in the native player");
            var keeperCamera=FindAnyObjectByType<ArenaCamera>().GetComponent<Camera>();
            var keeperParts=FindObjectsByType<SpriteRenderer>().Where(r=>r.enabled && r.name.StartsWith("BossPart-"));
            Check(keeperParts.Any() && keeperParts.All(r=>keeperCamera.WorldToViewportPoint(r.bounds.min).x>=0 && keeperCamera.WorldToViewportPoint(r.bounds.max).x<=1 && keeperCamera.WorldToViewportPoint(r.bounds.min).y>=0 && keeperCamera.WorldToViewportPoint(r.bounds.max).y<=1),"Larger Keeper anatomy fits the expanded encounter camera");
            yield return Shot("11-larger-keeper-with-summoned-guards");
            // Explicit reward-state fixture tests taking a rainbow ally into an already large roster.
            yield return Load(13,true,s=>{
                s.State.Roster.Clear();foreach(var u in catalog.Companions.Where(u=>u.Rarity!=ItemRarity.Rainbow).Take(20))s.State.Roster.Add(new RosterEntry{UnitId=u.Id,WeaponId=u.Weapon.Id});
                s.State.OutsideStage=OutsideStageStatus.Reward;s.State.OutsideRewards=new System.Collections.Generic.List<string>{"RainbowCrown","RainbowHeart","RainbowTailor"};
            });
            yield return new WaitForSecondsRealtime(.3f);
            owner.Simulation.Champion.Position=owner.Simulation.Champion.PreviousPosition=new Vector2(3,-.8f);yield return new WaitForSecondsRealtime(.2f);
            yield return Shot("12-rainbow-gift-with-twenty-companions");
            choice=owner.GetComponent<OutsideStageChoice>();
            Check(choice.Interact(),"One ordinary interaction takes a rainbow companion without a claim button");
            Check(owner.Session.State.Roster.Count==21 && owner.Session.State.Roster.Any(r=>r.UnitId=="RainbowTailor") && owner.Session.State.OutsideStage==OutsideStageStatus.Completed,"Rainbow ally joins beyond the old fifteen-body cap");
            Check(!choice.Interact(),"Single interaction cannot duplicate the gift");
            Check(!failed,"Shop flow native verification complete");
        }
    }
}
