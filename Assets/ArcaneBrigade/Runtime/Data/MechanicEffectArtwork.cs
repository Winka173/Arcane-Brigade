using System;
using UnityEngine;
namespace ArcaneBrigade
{
    [CreateAssetMenu(menuName="Arcane Brigade/Mechanic effect artwork")]
    public sealed class MechanicEffectArtwork : ScriptableObject
    {
        public Sprite[] Heroes=Array.Empty<Sprite>(),Forms=Array.Empty<Sprite>();
        public static MechanicEffectArtwork Current=>Resources.Load<MechanicEffectArtwork>("MechanicEffectArtwork");
        public Sprite Hero(string id)
        {
            int index=Array.FindIndex(HeroIdentities.All,h=>h.Id==id);
            return index>=0 && index<Heroes.Length?Heroes[index]:Impact;
        }
        public Sprite Form(BuildMorphKind kind)=>Forms.Length>=12 && (int)kind>=1 && (int)kind<=12?Forms[(int)kind-1]:Impact;
        public Sprite Slash=>Forms.Length>12?Forms[12]:null;
        public Sprite Thrust=>Forms.Length>13?Forms[13]:null;
        public Sprite Impact=>Forms.Length>14?Forms[14]:null;
        public Sprite Healing=>Forms.Length>15?Forms[15]:null;
    }
}
