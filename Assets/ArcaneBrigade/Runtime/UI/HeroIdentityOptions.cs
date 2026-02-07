using UnityEngine;
namespace ArcaneBrigade
{
    public sealed class HeroIdentityOptions
    {
        private GUIStyle choice, approachChoice;
        private Vector2 approachScroll;
        private string inspected;
        /// <summary>The hero's painted token with a small mark of the approach (one, a fan of three, or a circle of four),
        /// lit when chosen. Approaches are identified by their order, matching the 1-3 keys.</summary>
        private static void ApproachIcon(Rect r, HeroMechanicStyle style, int approach, bool chosen, bool enabled)
        {
            Sprite token = MechanicInk.Token(style.Token, style.Palette, style.Token == InkToken.Die ? 5 : 0); if (token == null) return;
            Color previous = GUI.color; GUI.color = enabled ? (chosen ? Color.white : new Color(.82f, .8f, .78f, .92f)) : new Color(.6f, .6f, .6f, .5f);
            void Draw(Rect at)
            {
                Rect uv = token.textureRect; Texture t = token.texture;
                GUI.DrawTextureWithTexCoords(at, t, new Rect(uv.x / t.width, uv.y / t.height, uv.width / t.width, uv.height / t.height));
            }
            Vector2 c = r.center; float big = r.width * 1.1f, small = r.width * .66f;
            if (approach == 0) Draw(new Rect(c.x - big * .5f, c.y - big * .5f, big, big));
            else if (approach == 1) for (int k = -1; k <= 1; k++) Draw(new Rect(c.x - small * .5f + k * small * .55f, c.y - small * .5f - (k == 0 ? small * .22f : -small * .08f), small, small));
            else for (int k = 0; k < 4; k++) { float a = k * Mathf.PI * .5f + Mathf.PI * .25f; Draw(new Rect(c.x - small * .5f + Mathf.Cos(a) * small * .48f, c.y - small * .5f + Mathf.Sin(a) * small * .48f, small, small)); }
            GUI.color = previous;
        }
        public void Draw(ArenaController owner, Rect area, GUIStyle text)
        {
            var s = owner.Session; var d = s.HeroIdentityDefinition; if (d == null) return;
            if (choice == null) choice = new GUIStyle(GUI.skin.button) { fontSize = 17, wordWrap = true }; if (approachChoice == null) approachChoice = new GUIStyle(choice) { padding = new RectOffset(46, 8, choice.padding.top, choice.padding.bottom) };
            GUI.Label(new Rect(area.x,area.y,area.width,50),s.CanConfigureHero ? "APPROACH / 1-3 or X / F or L3: special" : "Approach choices unlock while resting",text);
            bool enabled = GUI.enabled; GUI.enabled = s.CanConfigureHero;
            var style=HeroMechanicStyle.For(s.State.ChampionId);
            for (int i=0;i<3;i++)
            {
                int option=i; bool chosen=s.State.HeroIdentity.Approach==i; string name=(chosen?"• ":"")+d.Approaches[i];
                Rect button=new Rect(area.x+i*200,area.y+51,190,52);
                if(GUI.Button(button,name,approachChoice))owner.ConfigureHeroIdentity(()=>s.ChooseHeroApproach(option));
                if(Event.current.type==EventType.Repaint)ApproachIcon(new Rect(button.x+4,button.y+button.height*.5f-20,40,40),style,i,chosen,GUI.enabled);
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
