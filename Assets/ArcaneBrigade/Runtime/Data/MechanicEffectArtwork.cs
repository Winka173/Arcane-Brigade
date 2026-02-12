using System;
using UnityEngine;
namespace ArcaneBrigade
{
    [CreateAssetMenu(menuName="Arcane Brigade/Mechanic effect artwork")]
    public sealed class MechanicEffectArtwork : ScriptableObject
    {
        public Sprite[] Heroes=Array.Empty<Sprite>(),Forms=Array.Empty<Sprite>();
        // Optional hand-painted mechanic tokens indexed by InkToken (Docs/Hero_Mechanic_Art_Requests.md); empty slots use procedural ink.
        public Sprite[] Tokens=Array.Empty<Sprite>();
        public Sprite PaintedToken(int kind)=>kind>=0 && kind<Tokens.Length?Tokens[kind]:null;
        public static MechanicEffectArtwork Current=>Resources.Load<MechanicEffectArtwork>("MechanicEffectArtwork");
        public Sprite Hero(string id)
        {
            int index=Array.FindIndex(HeroIdentities.All,h=>h.Id==id);
            return index>=0 && index<Heroes.Length?Heroes[index]:Impact;
        }
        public Sprite Form(BuildMorphKind kind)
        {
            // The porcelain cell (blue-and-white crockery) read as broken dishes on every enemy death; the tea service
            // draws its spilt-tea art when painted (Resources/MienCho/ActOne/Form_TeaSpill), the plain impact until then.
            if(kind==BuildMorphKind.Porcelain) return TeaSpill??Impact;
            return Forms.Length>=12 && (int)kind>=1 && (int)kind<=12?Forms[(int)kind-1]:Impact;
        }
        [NonSerialized] private Sprite teaSpill; [NonSerialized] private bool teaLoaded;
        private Sprite TeaSpill { get { if(!teaLoaded){teaLoaded=true;teaSpill=Resources.Load<Sprite>("MienCho/ActOne/Form_TeaSpill");} return teaSpill; } }
        public Sprite Slash=>Forms.Length>12?Forms[12]:null;
        public Sprite Thrust=>Forms.Length>13?Forms[13]:null;
        public Sprite Impact=>Forms.Length>14?Forms[14]:null;
        public Sprite Healing=>Forms.Length>15?Forms[15]:null;
    }
}
