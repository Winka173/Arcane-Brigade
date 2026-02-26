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
            bool repaint=Event.current!=null && Event.current.type==EventType.Repaint;
            if(repaint)Wings(panel,rarity,clock,tint);
            Color paper=Color.Lerp(new Color(.035f,.045f,.075f,.98f),tint,rarity==ItemRarity.Common?.08f:.19f);paper.a=.98f;
            RunHud.Fill(panel,paper);
            Color edge=tint;edge.a=.85f;
            ArcadeSkin.Line(new Rect(panel.x,panel.y,panel.width,3),edge);ArcadeSkin.Line(new Rect(panel.x,panel.yMax-3,panel.width,3),edge);
            ArcadeSkin.Line(new Rect(panel.x,panel.y,3,panel.height),edge);ArcadeSkin.Line(new Rect(panel.xMax-3,panel.y,3,panel.height),edge);
            if(repaint)Trim(panel,rarity,clock,tint);
            if(!motion || rarity==ItemRarity.Common)return;
            float travel=Mathf.Repeat(clock*.12f,1),width=Mathf.Min(62,panel.width*.1f);
            Color shimmer=Color.Lerp(tint,Color.white,.5f);shimmer.a=.65f;
            ArcadeSkin.Line(new Rect(panel.x+(panel.width-width)*travel,panel.y,width,3),shimmer);
            ArcadeSkin.Line(new Rect(panel.x+(panel.width-width)*(1-travel),panel.yMax-3,width,3),shimmer);
        }

        // ---- the frame's accessories: small drawn ornaments, tinted by rarity, never the shop's bloom ----
        // Rare: scrolled filigree just outside the four corners and a cut gem on the top edge. Mythic and Crimson add a pair of small
        // feathered wings swept up from the two sides, that lift a little now and then. Rainbow adds a second gem under the
        // bottom edge and its wings shift hue. Each piece is a texture drawn once in code (antialiased shapes with a dark
        // outline and a lighter inner tone), so it reads as an ornament at any panel size. Still under Reduced Motion.
        private static Texture2D wing,corner,gem;

        // behind the card: the wings, so their roots tuck under the frame's edge
        private static void Wings(Rect panel,ItemRarity rarity,float clock,Color tint)
        {
            if(rarity<ItemRarity.Mythic)return;
            wing??=MakeWing();
            bool still=PresentationSettings.ReducedMotion;
            float size=Mathf.Clamp(panel.height*.78f,96,200);
            // a slow breath with a small lift every few seconds
            float lift=still?0:Mathf.Sin(clock*1.6f)*2.5f+Mathf.Pow(Mathf.Max(0,Mathf.Sin(clock*.7f)),12)*9;
            Color fill=Shade(tint,rarity);
            float rootY=panel.y+Mathf.Min(panel.height*.36f,size*.5f), top=rootY-size*(1-WingRoot.y/256f);
            // the art's root sits at its right (238, 105 of 256): the left wing as drawn, the right one mirrored
            Draw(wing,new Rect(panel.x-size*.88f,top,size,size),false,new Vector2(panel.x+size*.05f,rootY),-lift,fill);
            Draw(wing,new Rect(panel.xMax-size*.12f,top,size,size),true,new Vector2(panel.xMax-size*.05f,rootY),lift,fill);
        }

        // over the card's edge: the corner filigree and the gems
        private static void Trim(Rect panel,ItemRarity rarity,float clock,Color tint)
        {
            if(rarity<ItemRarity.Rare)return;
            corner??=MakeCorner(); gem??=MakeGem();
            bool still=PresentationSettings.ReducedMotion;
            Color gold=Color.Lerp(new Color(1,.84f,.5f),tint,.35f);
            float c=Mathf.Clamp(Mathf.Min(panel.width,panel.height)*.26f,44,72);
            // the four corners, each the same scroll turned to face its own corner, lying outside the frame (its corner bead on
            // the frame's corner, one arm along the edge, the scrolls curling away) so it never covers the card's own text
            float k=c*14/128f;
            Draw(corner,new Rect(panel.x-k,panel.y-c+k,c,c),false,Vector2.zero,0,gold,true);
            Draw(corner,new Rect(panel.xMax-c+k,panel.y-c+k,c,c),true,Vector2.zero,0,gold,true);
            Draw(corner,new Rect(panel.x-k,panel.yMax-k,c,c),false,Vector2.zero,0,gold,false);
            Draw(corner,new Rect(panel.xMax-c+k,panel.yMax-k,c,c),true,Vector2.zero,0,gold,false);
            // a cut gem set into the top edge, its glint sweeping across now and then
            float g=Mathf.Clamp(panel.height*.15f,30,44)*(rarity>=ItemRarity.Mythic?1.2f:1);
            Color stone=Color.Lerp(tint,Color.white,.15f);
            float glint=still?0:Mathf.Pow(Mathf.Max(0,Mathf.Sin(clock*1.3f)),20);
            Rect top=new Rect(panel.center.x-g*.5f,panel.y-g*.55f,g,g);
            Draw(gem,top,false,Vector2.zero,0,Color.Lerp(stone,Color.white,glint*.6f),false);
            ArcadeSkin.Line(new Rect(top.x-g*1.6f,panel.y-1,g*1.3f,2),gold);ArcadeSkin.Line(new Rect(top.xMax+g*.3f,panel.y-1,g*1.3f,2),gold);
            if(rarity==ItemRarity.Rainbow)Draw(gem,new Rect(panel.center.x-g*.4f,panel.yMax-g*.45f,g*.8f,g*.8f),false,Vector2.zero,0,Color.HSVToRGB(Mathf.Repeat(clock*.055f+.5f,1),.45f,1),false);
        }

        private static Color Shade(Color tint,ItemRarity rarity)
        {
            Color fill=rarity==ItemRarity.Crimson?new Color(1,.42f,.42f):Color.Lerp(tint,Color.white,.25f);
            fill.a=.97f; return fill;
        }
        private static void Draw(Texture2D texture,Rect place,bool mirror,Vector2 pivot,float angle,Color color,bool flipY=false)
        {
            Matrix4x4 saved=GUI.matrix; Color before=GUI.color;
            if(angle!=0)GUIUtility.RotateAroundPivot(angle,pivot);
            GUI.color=color;
            GUI.DrawTextureWithTexCoords(place,texture,new Rect(mirror?1:0,flipY?1:0,mirror?-1:1,flipY?-1:1),true);
            GUI.matrix=saved; GUI.color=before;
        }

        // ---- the ornaments, drawn once: white-to-light inner tone, dark outline, so a tint gives the rarity's own colours ----
        private static Texture2D Bake(string name,int width,int height,System.Func<float,float,Color> paint)
        {
            var texture=new Texture2D(width,height,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,hideFlags=HideFlags.DontSave};
            var pixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++)pixels[y*width+x]=paint(x+.5f,y+.5f);
            texture.SetPixels(pixels);texture.Apply(false,true);return texture;
        }
        private static Color Over(Color under,Color top){float a=top.a+under.a*(1-top.a);if(a<=0)return Color.clear;Color c=(top*top.a+under*under.a*(1-top.a))/a;c.a=a;return c;}
        private static readonly Color Outline=new Color(.22f,.17f,.14f);

        // a swept wing rising up and away from its root at the right: a back row of six long pointed feathers and a front
        // row of four shorter, lighter coverts, each with a darker vein and tip curling back; a round shoulder over the roots
        private static readonly (float[] angles,float[] lengths,float[] halves,float tone)[] Feathers=
        {
            (new[]{138f,152f,166f,180f,193f,206f},new[]{200f,194f,180f,160f,138f,114f},new[]{15f,15f,15f,14f,13f,12f},.80f),
            (new[]{140f,158f,177f,198f},new[]{118f,106f,92f,76f},new[]{19f,18f,17f,15f},.93f),
        };
        private static readonly Vector2 WingRoot=new Vector2(238,105);
        private static Texture2D MakeWing()=>Bake("RarityFrameWing",256,256,(x,y)=>
        {
            Vector2 p=new Vector2(x,y)-WingRoot; Color colour=Color.clear; float nearest=float.PositiveInfinity;
            foreach(var row in Feathers)
                for(int i=0;i<row.angles.Length;i++)
                {
                    float angle=row.angles[i]*Mathf.Deg2Rad,length=row.lengths[i];
                    Vector2 along=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)),side=new Vector2(-along.y,along.x);
                    float a=Vector2.Dot(p,along),s=Vector2.Dot(p,side);
                    float t=Mathf.Clamp01(a/length),w=row.halves[i]*Mathf.Pow(Mathf.Sin(Mathf.PI*Mathf.Clamp(t,.02f,1)),.5f)*(1-.3f*t);
                    s-=Mathf.Pow(t,2.2f)*10;
                    float edge=a<0||a>length?float.PositiveInfinity:Mathf.Abs(s)-w;
                    nearest=Mathf.Min(nearest,edge);
                    if(edge>=1.8f)continue;
                    float fill=Mathf.Clamp01(.5f-edge),shade=row.tone+(1-row.tone)*Mathf.Clamp01(t*.8f+(s<0?.15f:0));
                    float line=Mathf.Clamp01(2.2f-Mathf.Abs(edge+1.6f));
                    float vein=a>length*.1f&&a<length*.85f?Mathf.Clamp01(1.2f-Mathf.Abs(s+w*.2f))*.5f:0;
                    Color c=Color.Lerp(new Color(shade,shade,shade),Outline,Mathf.Max(line,vein)); c.a=Mathf.Max(fill,line*Mathf.Clamp01(1.8f-edge));
                    colour=Over(colour,c);
                }
            float shoulder=Vector2.Distance(new Vector2(x,y),WingRoot+new Vector2(-16,-4))-22;
            if(shoulder<1.8f)
            {
                float line=Mathf.Clamp01(2.2f-Mathf.Abs(shoulder+1.6f));
                Color c=Color.Lerp(Color.white,Outline,line); c.a=Mathf.Clamp01(Mathf.Clamp01(.5f-shoulder)+line*Mathf.Clamp01(1.8f-shoulder));
                colour=Over(colour,c);
            }
            if(colour.a<.02f && nearest<12)colour=Over(new Color(1,1,1,.18f*(1-nearest/12)),colour);
            return colour;
        });

        // a corner of filigree for the top-left corner: two arms along the edges ending in scrolls, a bead at the corner
        private static Texture2D MakeCorner()=>Bake("RarityFrameCorner",128,128,(x,y)=>
        {
            Vector2 p=new Vector2(x,128-y); // from the top-left, down
            float d=float.PositiveInfinity;
            d=Mathf.Min(d,Segment(p,new Vector2(14,14),new Vector2(96,14)));
            d=Mathf.Min(d,Segment(p,new Vector2(14,14),new Vector2(14,58)));
            d=Mathf.Min(d,Scroll(p,new Vector2(96,26),12,1));
            d=Mathf.Min(d,Scroll(p,new Vector2(26,58),12,-1));
            d=Mathf.Min(d,Segment(p,new Vector2(14,14),new Vector2(42,42)));
            float stroke=d-6.5f, bead=Vector2.Distance(p,new Vector2(14,14))-12, leaf=Vector2.Distance(p,new Vector2(46,46))-8;
            float shape=Mathf.Min(stroke,Mathf.Min(bead,leaf));
            float fill=Mathf.Clamp01(.5f-shape), line=Mathf.Clamp01(2.2f-Mathf.Abs(shape+1.6f));
            Color c=Color.Lerp(Color.Lerp(new Color(.85f,.8f,.7f),Color.white,Mathf.Clamp01((140-p.x-p.y)/140)),Outline,line);
            c.a=Mathf.Max(fill,line*Mathf.Clamp01(1.5f-shape));
            return c;
        });

        // a cut gem: an eight-sided stone with a table facet, light upper facets, darker lower ones and a dark rim
        private static Texture2D MakeGem()=>Bake("RarityFrameGem",96,96,(x,y)=>
        {
            Vector2 p=new Vector2(x-48,y-48);
            float r=Mathf.Max(Mathf.Abs(p.x)+Mathf.Abs(p.y)*.72f,Mathf.Max(Mathf.Abs(p.x),Mathf.Abs(p.y))*.92f)-38;
            if(r>1.5f)return Color.clear;
            float table=Mathf.Max(Mathf.Abs(p.x)+Mathf.Abs(p.y)*.72f,Mathf.Max(Mathf.Abs(p.x),Mathf.Abs(p.y))*.92f)-20;
            float tone=table<0?1:p.y>0?.86f-.1f*Mathf.Sign(p.x)*.5f:.62f+.08f*Mathf.Sign(p.x);
            float line=Mathf.Max(Mathf.Clamp01(1.6f-Mathf.Abs(r+1.4f)),Mathf.Clamp01(1.1f-Mathf.Abs(table))*.6f);
            float spark=Mathf.Clamp01(1-Vector2.Distance(p,new Vector2(-10,14))/7);
            Color c=Color.Lerp(new Color(tone,tone,tone),Outline,line); c=Color.Lerp(c,Color.white,spark);
            c.a=Mathf.Max(Mathf.Clamp01(.5f-r),line);
            return c;
        });
        private static float Segment(Vector2 p,Vector2 a,Vector2 b){Vector2 ab=b-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,ab)/ab.sqrMagnitude);return Vector2.Distance(p,a+ab*t);}
        // a spiral turning in on itself from the end of an arm
        private static float Scroll(Vector2 p,Vector2 centre,float radius,float turn)
        {
            float best=float.PositiveInfinity;
            for(int i=0;i<=48;i++){float t=i/48f,a=(-90+turn*t*330)*Mathf.Deg2Rad,r=radius*(1-.62f*t);Vector2 q=centre+new Vector2(Mathf.Cos(a)*r*turn,Mathf.Sin(a)*r);best=Mathf.Min(best,Vector2.Distance(p,q));}
            return best;
        }
    }
}
