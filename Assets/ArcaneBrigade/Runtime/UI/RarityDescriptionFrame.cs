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
            Color edge=tint;edge.a=.85f;
            ArcadeSkin.Line(new Rect(panel.x,panel.y,panel.width,3),edge);ArcadeSkin.Line(new Rect(panel.x,panel.yMax-3,panel.width,3),edge);
            ArcadeSkin.Line(new Rect(panel.x,panel.y,3,panel.height),edge);ArcadeSkin.Line(new Rect(panel.xMax-3,panel.y,3,panel.height),edge);
            if(Event.current!=null && Event.current.type==EventType.Repaint)Ornaments(panel,rarity,clock);
            if(!motion || rarity==ItemRarity.Common)return;
            float travel=Mathf.Repeat(clock*.12f,1),width=Mathf.Min(62,panel.width*.1f);
            Color shimmer=Color.Lerp(tint,Color.white,.5f);shimmer.a=.65f;
            ArcadeSkin.Line(new Rect(panel.x+(panel.width-width)*travel,panel.y,width,3),shimmer);
            ArcadeSkin.Line(new Rect(panel.x+(panel.width-width)*(1-travel),panel.yMax-3,width,3),shimmer);
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
            float crest=trim.Crest!=null?trim.Crest.height*crestWidth/trim.Crest.width*.38f:0;
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
            public float Width,Stem,Scroll,Pulse;   // strip width on screen, where its stem lies (0..1 across), px/s, pulse speed
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
            float crestWidth=Mathf.Clamp(panel.width*.38f,210,350)*(still?1:1+.012f*Mathf.Sin(clock*1.7f));
            if(trim.Crest!=null)
            {
                float h=trim.Crest.height*crestWidth/trim.Crest.width;
                Image(trim.Crest,new Rect(panel.center.x-crestWidth*.5f,panel.y-h*.62f,crestWidth,h),false,false);
            }
            if(trim.Foot!=null)
            {
                float w=crestWidth*.5f,h=trim.Foot.height*w/trim.Foot.width;
                Matrix4x4 saved=GUI.matrix;
                if(!still)GUIUtility.RotateAroundPivot(Mathf.Sin(clock*1.3f)*2.5f,new Vector2(panel.center.x,panel.yMax));
                Image(trim.Foot,new Rect(panel.center.x-w*.5f,panel.yMax-h*.18f,w,h),false,false);
                GUI.matrix=saved;
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
