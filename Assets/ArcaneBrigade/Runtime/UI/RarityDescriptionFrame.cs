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
            // requirement 23/10/2026 (the rarity frames read too quietly): a deeper band of the rarity's colour at the head of the
            // box fading down, a border that breathes (quicker the rarer), a carved wooden frame for Common, the rarity named
            // on a tab, one gem per rank on the crest, corner glints in turn; Rare+ light their edge on opening and motes rise up their rails
            Header(panel,tint,rarity);
            int rank=Rank(rarity);
            float breathe=motion?.72f+.28f*Mathf.Sin(clock*(1.6f+.5f*rank)):.9f;
            if(rarity==ItemRarity.Common && For(rarity)?.Side==null)Wood(panel); // the painted walnut trim when it is there
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
            // requirement (22/10/2026): no sliding shimmer bar along the edges, it read as a glitch
        }

        /// <summary>The part of a framed box clear of its painted trim (side strips, corner caps, crest and foot), so text laid
        /// out inside never runs under the ornaments. Same sizes as Ornaments below.</summary>
        public static Rect Content(Rect panel,ItemRarity rarity,float margin=8)
        {
            Trim trim=For(rarity);
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
                Load(ItemRarity.Common,"Common",48,.5f,0,.9f); // walnut and brass (review 31/10/2026: Common had only a plain wood border)
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
                case ItemRarity.Common: trim.CrestBase=.72f; trim.CrestBottom=.84f; break;
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
            Trim trim=For(rarity);
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
                // the hanging foot bobs a little (no turned matrix: a turn about a point lands elsewhere under the scaled HUD)
                float bob=still?0:Mathf.Round(Mathf.Sin(clock*1.6f)*1.5f);
                Image(trim.Foot,new Rect(panel.center.x-w*.5f,panel.yMax-h*.18f+bob,w,h),false,false);
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
        /// <summary>The gems, the rarity's tab, the corner glints, the edge lighting up as the box opens and the rising motes.</summary>
        private static void Extras(Rect panel,ItemRarity rarity,Color tint,float clock,bool motion)
        {
            int rank=Rank(rarity);
            float crestWidth=rarity==ItemRarity.Common?0:Mathf.Round(Mathf.Clamp(panel.width*.38f,210,350));
            // requirement 24/10/2026: no rarity name or gem tab on the frame any more; the crest and the frame's own motion say it
            Color bright=Color.Lerp(tint,Color.white,.35f);
            if(!motion)return;
            // the corners glint one after another
            int lit=Mathf.FloorToInt(clock*1.5f)%4; float k=Mathf.Repeat(clock*1.5f,1);
            if(rarity!=ItemRarity.Common)
            {
                const float inset=14; // the glints stay inside the box (requirement 24/10/2026)
                Vector2 cc=lit==0?new Vector2(panel.x+inset,panel.y+inset):lit==1?new Vector2(panel.xMax-inset,panel.y+inset):lit==2?new Vector2(panel.xMax-inset,panel.yMax-inset):new Vector2(panel.x+inset,panel.yMax-inset);
                float s=Mathf.Sin(k*Mathf.PI); Color g=Color.white; g.a=.9f*s; ArcadeSkin.Diamond(cc,6+8*s,g);
            }
            // requirement 24/10/2026: the old opening shine and falling glints were drawn through a turned GUI matrix and landed in one
            // fixed spot on the left of the screen; everything below is placed on the box itself, never turned
            long key=((long)rarity<<40)^((long)Mathf.RoundToInt(panel.x/8)<<20)^(long)Mathf.RoundToInt(panel.y/8);
            if(!seen.TryGetValue(key,out Seen sn) || Time.frameCount-sn.Frame>3)sn.Since=Time.unscaledTime;
            sn.Frame=Time.frameCount; seen[key]=sn;
            float age=Time.unscaledTime-sn.Since;
            // on opening (Rare and above): the frame lights up - two runs of light leave the crest along the top edge, race down
            // both sides and meet under the foot
            if(rank>=3 && age<.75f)
            {
                float p=Mathf.Clamp01(age/.55f), fade=1-Mathf.Clamp01((age-.55f)/.2f);
                for(int side=-1;side<=1;side+=2) EdgeRun(panel,side,1-(1-p)*(1-p),bright,fade,rarity==ItemRarity.Rainbow?clock:-1);
            }
            // while open, each rarity moves in its own way, strong enough to read at a glance (requirement 24/10/2026: "exaggerate it,
            // each rarity different"); everything stays on the frame or inside it
            float rail=Mathf.Max(10,Content(panel,rarity).x-panel.x-4);
            switch(rarity)
            {
                case ItemRarity.Uncommon: Motes(panel,rail,clock,bright,2,false,1.1f,-1); break;                  // dew running down the vine
                case ItemRarity.Rare:                                                                          // runes lighting round the rails in turn
                    Motes(panel,rail,clock,bright,3,true,1.4f,-1);
                    for(int i=0;i<6;i++){float q=Mathf.Repeat(clock*.9f-i/6f,1),g=Mathf.Clamp01(1-q*3); if(g<=0)continue; Color r=bright; r.a=.95f*g; float y=Mathf.Lerp(panel.y+24,panel.yMax-24,(i%3)/2f); ArcadeSkin.Diamond(new Vector2(i<3?panel.x+rail*.5f:panel.xMax-rail*.5f,y),7+6*g,r);}
                    break;
                case ItemRarity.Mythic:                                                                        // golden glints sweeping the frame
                    Motes(panel,rail,clock,bright,4,true,1.6f,-1);
                    {float q=Mathf.Repeat(age<.75f?0:clock*.42f,1); if(age>=.75f) for(int side=-1;side<=1;side+=2) EdgeRun(panel,side,q,bright,.85f*Mathf.Sin(q*Mathf.PI),-1);}
                    break;
                case ItemRarity.Crimson:                                                                       // a heartbeat of red light, petals falling
                {
                    float beat=Heartbeat(clock); Color h=new Color(.95f,.18f,.22f,.55f*beat);
                    Border(panel,4,h); h.a=.25f*beat; Border(new Rect(panel.x+4,panel.y+4,panel.width-8,panel.height-8),3,h);
                    Motes(panel,rail,clock,new Color(1f,.42f,.45f),4,false,1.5f,-1);
                    break;
                }
                case ItemRarity.Rainbow:                                                                       // prism light cycling round, star sparks
                    Motes(panel,rail,clock,bright,5,true,1.8f,clock);
                    {float q=Mathf.Repeat(age<.75f?0:clock*.55f,1); if(age>=.75f) for(int side=-1;side<=1;side+=2) EdgeRun(panel,side,q,bright,.9f,clock);}
                    break;
            }
        }
        /// <summary>Motes of the rarity's light on the inside of both side rails, rising (or falling like dew and petals).</summary>
        private static void Motes(Rect panel,float rail,float clock,Color colour,int count,bool rise,float size,float rainbowClock)
        {
            for(int side=0;side<2;side++)
                for(int i=0;i<count;i++)
                {
                    float life=3f+.6f*((i*7+side*3)%5)/4f, t=Mathf.Repeat(clock/life+i/(float)count+side*.37f,1);
                    float x=side==0?panel.x+rail*(.35f+.3f*Mathf.Sin(clock*1.3f+i*2.1f)):panel.xMax-rail*(.35f+.3f*Mathf.Sin(clock*1.2f+i*1.7f+1));
                    float y=rise?Mathf.Lerp(panel.yMax-14,panel.y+18,t):Mathf.Lerp(panel.y+18,panel.yMax-14,t);
                    Color m=rainbowClock>=0?Color.HSVToRGB(Mathf.Repeat(rainbowClock*.08f+i*.23f+side*.5f,1),.45f,1):colour;
                    m.a=.85f*Mathf.Sin(t*Mathf.PI);
                    ArcadeSkin.Diamond(new Vector2(x,y),(3+3f*Mathf.Sin(t*Mathf.PI))*size,m);
                }
        }
        /// <summary>One run of light along half the box's edge: from the top's middle along the top, down one side and along
        /// the bottom to its middle; p is how far the head has got (0..1), with a fading tail behind it.</summary>
        private static void EdgeRun(Rect panel,int side,float p,Color bright,float fade,float rainbowClock)
        {
            float half=panel.width*.5f, total=half+panel.height+half, head=p*total, tail=Mathf.Max(60,total*.35f);
            const int pieces=8;
            for(int i=0;i<pieces;i++)
            {
                float a=Mathf.Max(0,head-tail*(i+1)/pieces), b=Mathf.Max(0,head-tail*i/pieces); if(b<=a)continue;
                Color c=rainbowClock>=0?Color.HSVToRGB(Mathf.Repeat(rainbowClock*.08f+i*.06f,1),.4f,1):bright;
                c.a=fade*(1-i/(float)pieces)*.95f;
                EdgeSpan(panel,side,a,b,half,c);
            }
            Color h=Color.white; h.a=fade; ArcadeSkin.Diamond(EdgePoint(panel,side,head,half),7,h);
        }
        private static Vector2 EdgePoint(Rect panel,int side,float d,float half)
        {
            float cx=panel.center.x, ex=side<0?panel.x:panel.xMax;
            if(d<=half)return new Vector2(cx+side*d,panel.y+1.5f);
            if(d<=half+panel.height)return new Vector2(ex-side*1.5f,panel.y+(d-half));
            return new Vector2(ex-side*(d-half-panel.height),panel.yMax-1.5f);
        }
        private static void EdgeSpan(Rect panel,int side,float a,float b,float half,Color c)
        {
            float[] cuts={0,half,half+panel.height,half+panel.height+half};
            for(int k=0;k<3;k++)
            {
                float s0=Mathf.Max(a,cuts[k]), s1=Mathf.Min(b,cuts[k+1]); if(s1<=s0)continue;
                Vector2 p0=EdgePoint(panel,side,s0,half), p1=EdgePoint(panel,side,s1,half);
                Rect r=k==1?new Rect(p0.x-1.5f,Mathf.Min(p0.y,p1.y),3,Mathf.Abs(p1.y-p0.y)):new Rect(Mathf.Min(p0.x,p1.x),p0.y-1.5f,Mathf.Abs(p1.x-p0.x),3);
                ArcadeSkin.Line(r,c);
            }
        }
        /// <summary>A small framed tile's rarity (a choice slot, a list row): the breathing border and its gems, no trim.</summary>
        public static void Edge(Rect tile,ItemRarity rarity,float clock)
        {
            Color tint=RunModifierRules.RarityColor(rarity); int rank=Rank(rarity);
            bool motion=!PresentationSettings.ReducedMotion && !PresentationSettings.ReducedEffects && !PresentationSettings.ReducedFlashes;
            if(rarity==ItemRarity.Rainbow && motion)tint=Color.HSVToRGB(Mathf.Repeat(clock*.055f,1),.48f,1);
            float breathe=motion?.7f+.3f*Mathf.Sin(clock*(1.6f+.5f*rank)):.9f;
            if(Event.current==null || Event.current.type!=EventType.Repaint)return;
            // no light round the tile (requirement 4/11/2026, asked many times): the rarity shows as its diamonds on the top edge
            Color bright=Color.Lerp(tint,Color.white,.35f); float gap=11, x0=tile.center.x-(rank-1)*gap*.5f;
            for(int i=0;i<rank;i++){Vector2 c=new Vector2(x0+i*gap,tile.y);ArcadeSkin.Diamond(c,9,new Color(.08f,.05f,.1f,.95f));ArcadeSkin.Diamond(c,6,bright);}
        }
        /// <summary>Behind a picture (Rare and above): four sparkles twinkling in turn at its corners, more and quicker the rarer
        /// (requirement 24/10/2026: the turning rays were drawn through a turned GUI matrix and showed up in the wrong place).</summary>
        public static void Backdrop(Rect picture,ItemRarity rarity,float clock)
        {
            int rank=Rank(rarity); if(rank<3 || Event.current==null || Event.current.type!=EventType.Repaint)return;
            bool still=PresentationSettings.ReducedMotion;
            Color tint=RunModifierRules.RarityColor(rarity);
            // four sparkles on a circle round the picture, turning slowly (at the corners they drew a square)
            float radius=Mathf.Max(picture.width,picture.height)*.62f, turn=still?0:clock*.35f;
            Vector2[] at=new Vector2[4];
            for(int k=0;k<4;k++){float a=turn+k*Mathf.PI*.5f+Mathf.PI*.25f; at[k]=picture.center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;}
            float speed=.9f+.25f*rank;
            for(int i=0;i<4;i++)
            {
                float t=still?.6f:Mathf.Repeat(clock*speed*.5f-i*.25f,1), s=Mathf.Sin(Mathf.Clamp01(t*2)*Mathf.PI);
                if(rarity==ItemRarity.Rainbow && !still)tint=Color.HSVToRGB(Mathf.Repeat(clock*.08f+i*.25f,1),.45f,1);
                Color c=Color.Lerp(tint,Color.white,.4f); c.a=.85f*s; if(c.a<=.02f)continue;
                float size=4+6*s*(rank>=5?1.2f:1);
                ArcadeSkin.Diamond(at[i],size,c); Color core=Color.white; core.a=c.a; ArcadeSkin.Diamond(at[i],size*.4f,core);
            }
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
