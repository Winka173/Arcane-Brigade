using UnityEngine;
namespace ArcaneBrigade
{
    public static class RarityDescriptionFrame
    {
        public static void Draw(Rect panel,ItemRarity rarity,float clock)
        {
            Color tint=RunModifierRules.RarityColor(rarity);
            bool motion=!PresentationSettings.ReducedMotion && !PresentationSettings.ReducedEffects && !PresentationSettings.ReducedFlashes;
            if(rarity==ItemRarity.Rainbow && motion)tint=Color.HSVToRGB(Mathf.Repeat(clock*.055f,1),.48f,1);
            Color paper=Color.Lerp(new Color(.035f,.045f,.075f,.98f),tint,rarity==ItemRarity.Common?.08f:.19f);paper.a=.98f;
            RunHud.Fill(panel,paper);
            // owner 23/10/2026 (the rarity frames read too quietly): a deeper band of the rarity's colour at the head of the
            // box fading down, a border that breathes (quicker the rarer), a carved wooden frame for Common, the rarity named
            // on a tab, one gem per rank on the crest, corner glints in turn, and on opening one shine across the box (Rare+)
            Header(panel,tint,rarity);
            int rank=Rank(rarity);
            float breathe=motion?.72f+.28f*Mathf.Sin(clock*(1.6f+.5f*rank)):.9f;
            if(rarity==ItemRarity.Common)Wood(panel);
            else
            {
                Color edge=tint;edge.a=.85f*breathe;
                Border(panel,3,edge);
                Color halo=tint;halo.a=.28f*breathe; Border(new Rect(panel.x-3,panel.y-3,panel.width+6,panel.height+6),2,halo);
                halo.a=.12f*breathe; Border(new Rect(panel.x-6,panel.y-6,panel.width+12,panel.height+12),2,halo);
            }
            if(Event.current!=null && Event.current.type==EventType.Repaint)
            {
                Ornaments(panel,rarity,clock);
                Extras(panel,rarity,tint,clock,motion);
            }
            if(!motion || rarity==ItemRarity.Common)return;
            // owner (22/10/2026): no sliding shimmer bar along the edges, it read as a glitch
        }

        /// <summary>The part of a framed box clear of its painted trim (side strips, corner caps, crest and foot), so text laid
        /// out inside never runs under the ornaments. Same sizes as Ornaments below.</summary>
        public static Rect Content(Rect panel,ItemRarity rarity,float margin=8)
        {
            Trim trim=rarity==ItemRarity.Common?null:For(rarity);
            if(trim==null || trim.Side==null)return new Rect(panel.x+margin+4,panel.y+margin+4,panel.width-2*(margin+4),panel.height-2*(margin+4));
            float size=Mathf.Clamp(panel.height/300f,.72f,1.1f), strip=trim.Width*size*(1-trim.Stem)-2;
            float c=Mathf.Clamp(Mathf.Min(panel.width,panel.height)*.2f,40,60), corner=trim.Corner!=null?c*.84f:0;
            float crestWidth=Mathf.Clamp(panel.width*.38f,210,350)*1.012f;
            float crest=trim.Crest!=null?trim.Crest.height*crestWidth/trim.Crest.width*(trim.CrestBottom-trim.CrestBase):0;
            float foot=trim.Foot!=null?trim.Foot.height*(crestWidth*.5f)/trim.Foot.width*.18f:0;
            float side=Mathf.Max(strip,corner*.6f)+margin, top=Mathf.Max(crest,corner*.6f)+margin, bottom=Mathf.Max(foot,corner*.6f)+margin;
            return new Rect(panel.x+side,panel.y+top,panel.width-2*side,panel.height-top-bottom);
        }

        // ---- each rarity's own trim, painted by Tools/Art/rarity_frame_art.py into Resources/RarityFrame ----
        // Uncommon  moon-vine: a silver-blue vine with leaves and dew growing slowly up both sides, a moon bud crest
        // Rare      rune rail: carved violet rails with glowing rune medallions, a rune-eye crest, a tassel below
        // Mythic    gilded wings: golden feathers down both sides with glinting beads, a sun-crown crest, a gold drop below
        // Crimson   blood briar: thorn stems with roses beating like a heart, a horned ruby crest, a hanging bud below
        // Rainbow   celestial prism: prism shards and stars drifting up, a crescent-moon crest, a star pendant below
        // The side strips run the whole height of the box, their stems just outside its edge; the crest sits on the top
        // edge, the foot hangs from the bottom one, and a cap marks each corner. Still under Reduced Motion; the glow
        // pulse is left out under Reduced Effects.
        private sealed class Trim
        {
            public Texture2D Side,Glow,Crest,Foot,Corner;
            public float Width,Stem,Scroll,Pulse;
            // measured from each painted crest (share of its height): the row where its rail spreads wide, which sits on the
            // box's top edge, and the lowest painted pixel, which may hang a little into the box
            public float CrestBase=.8f,CrestBottom=.92f;   // strip width on screen, where its stem lies (0..1 across), px/s, pulse speed
        }
        private static readonly Trim[] trims=new Trim[6];
        private static bool loaded;

        private static Trim For(ItemRarity rarity)
        {
            if(!loaded)
            {
                loaded=true;
                Load(ItemRarity.Uncommon,"Uncommon",54,.5f,6,1.4f);
                Load(ItemRarity.Rare,"Rare",50,.5f,0,1.7f);
                Load(ItemRarity.Mythic,"Mythic",64,.77f,0,1.1f);
                Load(ItemRarity.Crimson,"Crimson",58,.5f,-3,1);
                Load(ItemRarity.Rainbow,"Rainbow",56,.5f,11,2.4f);
            }
            return trims[(int)rarity];
        }
        private static void Load(ItemRarity rarity,string name,float width,float stem,float scroll,float pulse)
        {
            Texture2D Part(string part)=>Resources.Load<Texture2D>("RarityFrame/"+name+"_"+part);
            var trim=new Trim{Side=Part("Side"),Glow=Part("Glow"),Crest=Part("Crest"),Foot=Part("Foot"),Corner=Part("Corner"),Width=width,Stem=stem,Scroll=scroll,Pulse=pulse};
            foreach(Texture2D strip in new[]{trim.Side,trim.Glow})
                if(strip!=null){strip.wrapModeV=TextureWrapMode.Repeat;strip.wrapModeU=TextureWrapMode.Clamp;}
            switch(rarity) // crest rail row / lowest pixel, measured from Resources/RarityFrame/*_Crest.png
            {
                case ItemRarity.Uncommon: trim.CrestBase=.78f; trim.CrestBottom=.98f; break;
                case ItemRarity.Rare: trim.CrestBase=.81f; trim.CrestBottom=.91f; break;
                case ItemRarity.Mythic: trim.CrestBase=.70f; trim.CrestBottom=.95f; break;
                case ItemRarity.Crimson: trim.CrestBase=.87f; trim.CrestBottom=.95f; break;
                case ItemRarity.Rainbow: trim.CrestBase=.78f; trim.CrestBottom=.99f; break;
            }
            trims[(int)rarity]=trim;
        }

        private static void Ornaments(Rect panel,ItemRarity rarity,float clock)
        {
            Trim trim=rarity==ItemRarity.Common?null:For(rarity);
            if(trim==null || trim.Side==null)return;
            bool still=PresentationSettings.ReducedMotion, sparse=PresentationSettings.ReducedEffects;
            float size=Mathf.Clamp(panel.height/300f,.72f,1.1f);
            float width=trim.Width*size, tile=trim.Side.height*width/trim.Side.width;
            float scroll=still?0:clock*trim.Scroll/tile;
            float pulse=still?.6f:rarity==ItemRarity.Crimson?Heartbeat(clock):.5f+.5f*Mathf.Sin(clock*trim.Pulse*Mathf.PI);
            // the two sides, the whole height and a little past each corner, stems just outside the box's edge
            float top=panel.y-6, length=panel.height+12;
            Rect left=new Rect(panel.x-width*trim.Stem-2,top,width,length), right=new Rect(panel.xMax-width*(1-trim.Stem)+2,top,width,length);
            Strip(trim.Side,left,false,length/tile,scroll,Color.white);
            Strip(trim.Side,right,true,length/tile,scroll+.5f,Color.white);
            if(!sparse && trim.Glow!=null)
            {
                Color glow=new Color(1,1,1,.25f+.75f*pulse);
                Strip(trim.Glow,left,false,length/tile,scroll,glow);
                Strip(trim.Glow,right,true,length/tile,scroll+.5f,glow);
            }
            // the four corner caps, their bead on the box's corner
            if(trim.Corner!=null)
            {
                float c=Mathf.Clamp(Mathf.Min(panel.width,panel.height)*.2f,40,60), k=c*.16f;
                Image(trim.Corner,new Rect(panel.x-k,panel.y-k,c,c),false,false);
                Image(trim.Corner,new Rect(panel.xMax-c+k,panel.y-k,c,c),true,false);
                Image(trim.Corner,new Rect(panel.x-k,panel.yMax-c+k,c,c),false,true);
                Image(trim.Corner,new Rect(panel.xMax-c+k,panel.yMax-c+k,c,c),true,true);
            }
            // the crest seated on the top edge, breathing a little; the foot hanging from the bottom edge, swaying
            float crestWidth=Mathf.Round(Mathf.Clamp(panel.width*.38f,210,350)); // crest held still: its 'breathing' scale shimmered on the pixel grid
            if(trim.Crest!=null)
            {
                float h=trim.Crest.height*crestWidth/trim.Crest.width;
                Image(trim.Crest,new Rect(panel.center.x-crestWidth*.5f,panel.y-h*trim.CrestBase,crestWidth,h),false,false);
            }
            if(trim.Foot!=null)
            {
                float w=crestWidth*.5f,h=trim.Foot.height*w/trim.Foot.width;
                Matrix4x4 saved=GUI.matrix;
                if(!still)GUIUtility.RotateAroundPivot(Mathf.Sin(clock*1.1f)*1.5f,new Vector2(panel.center.x,panel.yMax)); // the hanging foot sways gently
                Image(trim.Foot,new Rect(panel.center.x-w*.5f,panel.yMax-h*.18f,w,h),false,false);
                GUI.matrix=saved;
            }
        }
        private static int Rank(ItemRarity rarity)=>rarity switch{ItemRarity.Common=>1,ItemRarity.Uncommon=>2,ItemRarity.Rare=>3,ItemRarity.Mythic=>4,ItemRarity.Crimson=>5,_=>6};
        private static void Border(Rect r,float w,Color c)
        {
            ArcadeSkin.Line(new Rect(r.x,r.y,r.width,w),c);ArcadeSkin.Line(new Rect(r.x,r.yMax-w,r.width,w),c);
            ArcadeSkin.Line(new Rect(r.x,r.y+w,w,r.height-2*w),c);ArcadeSkin.Line(new Rect(r.xMax-w,r.y+w,w,r.height-2*w),c);
        }
        /// <summary>The head of the box washed in the rarity's colour, fading down over its top third.</summary>
        private static void Header(Rect panel,Color tint,ItemRarity rarity)
        {
            float depth=Mathf.Min(panel.height*.34f,110), strength=rarity==ItemRarity.Common?.1f:.26f; const int steps=10;
            for(int i=0;i<steps;i++){Color c=tint;c.a=strength*(1-i/(float)steps)*(1-i/(float)steps);RunHud.Fill(new Rect(panel.x+3,panel.y+3+depth*i/steps,panel.width-6,depth/steps+1),c);}
        }
        /// <summary>Common's own frame: a carved wooden border with brass nail heads at the corners.</summary>
        private static void Wood(Rect panel)
        {
            Border(panel,5,new Color(.36f,.24f,.14f,.95f)); Border(new Rect(panel.x+5,panel.y+5,panel.width-10,panel.height-10),1,new Color(.62f,.46f,.28f,.8f));
            Border(new Rect(panel.x-1,panel.y-1,panel.width+2,panel.height+2),1,new Color(.12f,.07f,.04f,.9f));
            if(Event.current==null || Event.current.type!=EventType.Repaint)return;
            foreach(Vector2 c in new[]{new Vector2(panel.x+9,panel.y+9),new Vector2(panel.xMax-9,panel.y+9),new Vector2(panel.x+9,panel.yMax-9),new Vector2(panel.xMax-9,panel.yMax-9)})
            { ArcadeSkin.Diamond(c,9,new Color(.2f,.13f,.07f)); ArcadeSkin.Diamond(c,6,new Color(.86f,.68f,.36f)); }
        }
        private struct Seen{public int Frame;public float Since;}
        private static readonly System.Collections.Generic.Dictionary<long,Seen> seen=new System.Collections.Generic.Dictionary<long,Seen>();
        private static GUIStyle tabStyle;
        private static Texture2D shine;
        /// <summary>The gems, the rarity's tab, the corner glints and the one shine as the box opens.</summary>
        private static void Extras(Rect panel,ItemRarity rarity,Color tint,float clock,bool motion)
        {
            int rank=Rank(rarity);
            float crestWidth=rarity==ItemRarity.Common?0:Mathf.Round(Mathf.Clamp(panel.width*.38f,210,350));
            // the rarity named on a tab on the top edge, left of the crest, with one gem per rank beside the name (readable
            // without colour); a box too narrow for the tab shows the gems alone on its top edge
            if(tabStyle==null){tabStyle=ArcadeTypography.Style(15,true);tabStyle.alignment=TextAnchor.MiddleLeft;tabStyle.wordWrap=false;}
            string name=RunModifierRules.RarityName(rarity).ToUpperInvariant();
            Color bright=Color.Lerp(tint,Color.white,.35f); const float gap=13;
            float textW=tabStyle.CalcSize(new GUIContent(name)).x, tw2=12+textW+10+rank*gap+8, corner=rarity==ItemRarity.Common?14:Mathf.Clamp(Mathf.Min(panel.width,panel.height)*.2f,40,60);
            Rect tab=new Rect(panel.x+corner+6,panel.y-13,tw2,26);
            bool fits=tab.xMax<panel.center.x-crestWidth*.5f-6;
            if(fits){RunHud.Fill(tab,new Color(.06f,.04f,.08f,.96f)); Border(tab,2,bright); tabStyle.normal.textColor=bright; GUI.Label(new Rect(tab.x+12,tab.y,textW+4,tab.height),name,tabStyle);}
            float gx=fits?tab.x+12+textW+10+gap*.5f:panel.center.x-(rank-1)*gap*.5f, gy=fits?tab.center.y:panel.y+1;
            for(int i=0;i<rank;i++)
            {
                Vector2 c=new Vector2(gx+i*gap,gy); float tw=motion?.85f+.15f*Mathf.Sin(clock*3+i*1.3f):1;
                ArcadeSkin.Diamond(c,11,new Color(.08f,.05f,.1f,.95f)); ArcadeSkin.Diamond(c,8*tw,bright);
            }
            if(!motion)return;
            // the corners glint one after another
            int lit=Mathf.FloorToInt(clock*1.5f)%4; float k=Mathf.Repeat(clock*1.5f,1);
            if(rarity!=ItemRarity.Common)
            {
                const float inset=14; // the glints stay inside the box (owner 24/10/2026)
                Vector2 cc=lit==0?new Vector2(panel.x+inset,panel.y+inset):lit==1?new Vector2(panel.xMax-inset,panel.y+inset):lit==2?new Vector2(panel.xMax-inset,panel.yMax-inset):new Vector2(panel.x+inset,panel.yMax-inset);
                float s=Mathf.Sin(k*Mathf.PI); Color g=Color.white; g.a=.9f*s; ArcadeSkin.Diamond(cc,6+8*s,g);
            }
            // the shine: once, as the box first appears (Rare and above)
            long key=((long)rarity<<40)^((long)Mathf.RoundToInt(panel.x/8)<<20)^(long)Mathf.RoundToInt(panel.y/8);
            if(!seen.TryGetValue(key,out Seen sn) || Time.frameCount-sn.Frame>3)sn.Since=Time.unscaledTime;
            sn.Frame=Time.frameCount; seen[key]=sn;
            float age=Time.unscaledTime-sn.Since;
            if(rank>=3 && age<.7f)
            {
                if(shine==null)
                {
                    shine=new Texture2D(64,4,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
                    for(int x=0;x<64;x++){float u=x/63f*2-1;Color c=new Color(1,1,1,Mathf.Exp(-u*u*6)*.55f);for(int y=0;y<4;y++)shine.SetPixel(x,y,c);}
                    shine.Apply(false,true);
                }
                float p=age/.7f, band=Mathf.Max(90,panel.height*.5f), x0=Mathf.Lerp(-band*2,panel.width+band,p);
                GUI.BeginGroup(panel);
                Matrix4x4 saved=GUI.matrix; Color before=GUI.color; GUI.color=new Color(bright.r,bright.g,bright.b,Mathf.Sin(p*Mathf.PI));
                GUIUtility.RotateAroundPivot(20,new Vector2(x0+band*.5f,panel.height*.5f));
                GUI.DrawTexture(new Rect(x0,-panel.height,band,panel.height*3),shine);
                GUI.matrix=saved; GUI.color=before;
                GUI.EndGroup();
            }
            // Mythic and above: glints falling from the crest into the box as it opens (never outside the frame)
            if(rank>=4 && age<.6f)
                for(int i=0;i<7;i++)
                {
                    float a=(30+i*20)*Mathf.Deg2Rad, d=12+Mathf.Min(70,panel.height*.4f)*(age/.6f); Color g=bright; g.a=1-age/.6f;
                    Vector2 at=new Vector2(panel.center.x+Mathf.Cos(a)*d*1.6f,panel.y+16+Mathf.Sin(a)*d);
                    at.x=Mathf.Clamp(at.x,panel.x+14,panel.xMax-14); at.y=Mathf.Clamp(at.y,panel.y+14,panel.yMax-14);
                    ArcadeSkin.Diamond(at,7*(1-age/.6f)+2,g);
                }
        }
        /// <summary>A small framed tile's rarity (a choice slot, a list row): the breathing border and its gems, no trim.</summary>
        public static void Edge(Rect tile,ItemRarity rarity,float clock)
        {
            Color tint=RunModifierRules.RarityColor(rarity); int rank=Rank(rarity);
            bool motion=!PresentationSettings.ReducedMotion && !PresentationSettings.ReducedEffects && !PresentationSettings.ReducedFlashes;
            if(rarity==ItemRarity.Rainbow && motion)tint=Color.HSVToRGB(Mathf.Repeat(clock*.055f,1),.48f,1);
            float breathe=motion?.7f+.3f*Mathf.Sin(clock*(1.6f+.5f*rank)):.9f;
            Color edge=tint;edge.a=.9f*breathe; Border(tile,3,edge);
            Color halo=tint;halo.a=.3f*breathe; Border(new Rect(tile.x-3,tile.y-3,tile.width+6,tile.height+6),2,halo);
            if(Event.current==null || Event.current.type!=EventType.Repaint)return;
            Color bright=Color.Lerp(tint,Color.white,.35f); float gap=11, x0=tile.center.x-(rank-1)*gap*.5f;
            for(int i=0;i<rank;i++){Vector2 c=new Vector2(x0+i*gap,tile.y);ArcadeSkin.Diamond(c,9,new Color(.08f,.05f,.1f,.95f));ArcadeSkin.Diamond(c,6,bright);}
        }
        private static Texture2D ray;
        /// <summary>Rays of the rarity's light turning slowly behind a picture (Rare and above): never a disc.</summary>
        public static void Backdrop(Rect picture,ItemRarity rarity,float clock)
        {
            int rank=Rank(rarity); if(rank<3 || Event.current==null || Event.current.type!=EventType.Repaint)return;
            if(ray==null)
            {
                ray=new Texture2D(8,64,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
                for(int y=0;y<64;y++)for(int x=0;x<8;x++){float v=y/63f,u=Mathf.Abs(x/7f*2-1);ray.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(v*1.4f)*(1-u)*(1-u)));}
                ray.Apply(false,true);
            }
            Color tint=RunModifierRules.RarityColor(rarity); bool still=PresentationSettings.ReducedMotion;
            int n=rank==3?6:rank==4?8:rank==5?8:10; float len=Mathf.Max(picture.width,picture.height)*.78f, w=Mathf.Max(6,len*.16f);
            Vector2 c=picture.center; Matrix4x4 saved=GUI.matrix; Color before=GUI.color;
            for(int i=0;i<n;i++)
            {
                if(rarity==ItemRarity.Rainbow && !still)tint=Color.HSVToRGB(Mathf.Repeat(clock*.055f+i/(float)n,1),.5f,1);
                GUI.matrix=saved; GUIUtility.RotateAroundPivot((still?0:clock*12)+i*360f/n,c);
                GUI.color=new Color(tint.r,tint.g,tint.b,(.3f+.1f*rank/6f)*(i%2==0?1:.6f));
                GUI.DrawTexture(new Rect(c.x-w*.5f,c.y-len,w,len),ray);
            }
            GUI.matrix=saved; GUI.color=before;
        }
        // two quick beats then a rest, like a heart
        private static float Heartbeat(float clock){float t=Mathf.Repeat(clock,1.4f);return Mathf.Max(Mathf.Exp(-Mathf.Pow((t-.1f)/.06f,2)),.7f*Mathf.Exp(-Mathf.Pow((t-.32f)/.07f,2)));}

        private static void Strip(Texture2D texture,Rect place,bool mirror,float repeats,float offset,Color color)
        {
            Color before=GUI.color; GUI.color=color;
            GUI.DrawTextureWithTexCoords(place,texture,new Rect(mirror?1:0,-offset-repeats,mirror?-1:1,repeats),true);
            GUI.color=before;
        }
        private static void Image(Texture2D texture,Rect place,bool mirror,bool flip)
        {
            GUI.DrawTextureWithTexCoords(place,texture,new Rect(mirror?1:0,flip?1:0,mirror?-1:1,flip?-1:1),true);
        }
    }
}
