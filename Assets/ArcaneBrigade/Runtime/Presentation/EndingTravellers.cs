using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One ending cast: living story companions retain their own identities outside the recruit roster.</summary>
    public static class EndingTravellers
    {
        public static IEnumerable<UnitDefinition> Story(ArenaController owner)
        {
            var party = owner.Session.CombatCatalog?.StoryCompanions;
            if (party == null) yield break;
            foreach (var unit in party) if (unit != null) yield return unit;
        }
        public static bool IsStory(UnitDefinition unit)
            => System.Array.IndexOf(ActOneCanon.StoryNames, unit?.DisplayName) >= 0;
        public static Sprite Frame(UnitDefinition unit, float time, int index, bool moving)
        {
            if (unit.AuthoredMotion != null)
            {
                var frames = moving && !PresentationSettings.ReducedMotion ? unit.AuthoredMotion.Walk : unit.AuthoredMotion.Idle;
                if (frames != null && frames.Length > 0) return frames[PresentationSettings.ReducedMotion ? 0 : Mathf.FloorToInt(time * (moving ? 6 : 2) + index) % frames.Length];
            }
            return IsStory(unit) ? ActOneArtwork.Picture(unit.DisplayName) ?? unit.Body : unit.Body;
        }
    }
}
