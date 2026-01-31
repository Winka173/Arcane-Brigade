using System;
using UnityEngine;
namespace ArcaneBrigade
{
    [CreateAssetMenu(menuName="Arcane Brigade/Waiting scenery damage stages")]
    public sealed class WaitingSceneryArtwork : ScriptableObject
    {
        public Sprite[] Frames=Array.Empty<Sprite>();
        public static WaitingSceneryArtwork Current=>Resources.Load<WaitingSceneryArtwork>("WaitingSceneryArtwork");
        public Sprite Frame(int kind,int damage)=>Frames.Length==12?Frames[Mathf.Clamp(damage,0,2)*4+kind%4]:null;
    }
}
