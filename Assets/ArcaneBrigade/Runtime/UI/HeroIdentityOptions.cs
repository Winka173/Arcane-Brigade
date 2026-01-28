using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class HeroIdentityOptions
    {
        private GUIStyle choice;
        private Vector2 approachScroll;
        private string inspected;
        public void Draw(ArenaController owner, Rect area, GUIStyle text)
        {
            var s = owner.Session; var d = s.HeroIdentityDefinition; if (d == null) return;
            if (choice == null) choice = new GUIStyle(GUI.skin.button) { fontSize = 17, wordWrap = true };
            GUI.Label(new Rect(area.x,area.y,area.width,50),s.CanConfigureHero ? "APPROACH / 1-3 or X / F or L3: special" : "Approach choices unlock while resting",text);
            bool enabled = GUI.enabled; GUI.enabled = s.CanConfigureHero;
            for (int i=0;i<3;i++)
            {
                int option=i; string name=(s.State.HeroIdentity.Approach==i?"• ":"")+d.Approaches[i];
                if(GUI.Button(new Rect(area.x+i*200,area.y+51,190,52),name,choice))owner.ConfigureHeroIdentity(()=>s.ChooseHeroApproach(option));
            }
            string id=s.State.ChampionId;
            if(id=="PatchworkHost" && GUI.Button(new Rect(area.x,area.y+115,590,48),"NEXT FEATURED FRIEND",choice))owner.ConfigureHeroIdentity(s.CycleFeaturedFriend);
            if(id=="GlassDice" && GUI.Button(new Rect(area.x,area.y+115,590,48),"REROLL NEXT FACE / 6 GOLD / ONCE PER WAVE",choice))owner.ConfigureHeroIdentity(s.RerollHeroDie);
            if(id=="PennyBroker" && GUI.Button(new Rect(area.x,area.y+115,590,48),"PLACE ORDER / INVEST "+s.HeroOrderDeposit+" GOLD",choice))owner.ConfigureHeroIdentity(s.PlaceHeroOrder);
            if(id=="LoneJelly")for(int i=0;i<3;i++)
            {
                int slot=i;string organ=s.State.HeroIdentity.Organs[i];
                string label="ORGAN "+(i+1)+" / "+(string.IsNullOrEmpty(organ)?"Auto":owner.Content.Unit(organ).DisplayName);
                if(GUI.Button(new Rect(area.x+i*200,area.y+115,190,58),label,choice))owner.ConfigureHeroIdentity(()=>s.CycleMemoryOrgan(slot));
            }
            GUI.enabled=enabled;
            string current=id+"/"+s.State.HeroIdentity.Approach;
            if(inspected!=current){inspected=current;approachScroll=Vector2.zero;}
            string details=s.HeroIdentityShopSummary+"\n\n"+d.ApproachDescriptions[s.State.HeroIdentity.Approach];
            float height=Mathf.Max(110,text.CalcHeight(new GUIContent(details),555)+12);
            approachScroll=GUI.BeginScrollView(new Rect(area.x,area.y+182,590,110),approachScroll,new Rect(0,0,565,height));
            GUI.Label(new Rect(0,0,555,height),details,text);
            GUI.EndScrollView();
            if(Event.current.type==EventType.KeyDown && s.CanConfigureHero)
            {
                var key=Event.current.keyCode;
                int option=key==KeyCode.Alpha1?0:key==KeyCode.Alpha2?1:key==KeyCode.Alpha3?2:-1;
                if(option>=0){owner.ConfigureHeroIdentity(()=>s.ChooseHeroApproach(option));Event.current.Use();}
            }
        }
    }
}
