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
            if(!motion || rarity==ItemRarity.Common)return;
            float travel=Mathf.Repeat(clock*.12f,1),width=Mathf.Min(62,panel.width*.1f);
            Color shimmer=Color.Lerp(tint,Color.white,.5f);shimmer.a=.65f;
            ArcadeSkin.Line(new Rect(panel.x+(panel.width-width)*travel,panel.y,width,3),shimmer);
            ArcadeSkin.Line(new Rect(panel.x+(panel.width-width)*(1-travel),panel.yMax-3,width,3),shimmer);
        }
    }
}
