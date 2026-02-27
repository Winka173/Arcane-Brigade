using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class NightmareDeparture
    {
        private readonly List<UnitDefinition> guests = new List<UnitDefinition>();
        private readonly List<WeaponDefinition> weapons = new List<WeaponDefinition>();
        private readonly List<int> tiers = new List<int>();
        private string run;
        public int PartyCount => guests.Count;
        public static float Travel(float elapsed, int index, bool reduced) => reduced ? .55f : Mathf.SmoothStep(0, 1, Mathf.Clamp01((elapsed - index * .22f) / 4.5f));
        public static float Visibility(float travel) => 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.88f, 1, travel));
        public void Draw(Rect image, ArenaController owner, int page, float elapsed)
        {
            if (page != 2) return;
            if (run != owner.Session.State.RunId)
            {
                run = owner.Session.State.RunId; guests.Clear(); weapons.Clear(); tiers.Clear();
                foreach (var companion in EndingTravellers.Story(owner)) Add(companion);
                // whoever actually came out of the drawer (the paper bird is a rescue too); older runs only knew it was the bear
                if (owner.Session.State.Secrets.RescuedToy) Add(owner.Content.Unit(string.IsNullOrEmpty(owner.Session.State.Secrets.RescuedId) ? "SecretPatchBear" : owner.Session.State.Secrets.RescuedId));
                if (owner.Session.State.Secrets.LetterSent && !guests.Contains(owner.Content.Unit("SecretPaperBird"))) Add(owner.Content.Unit("SecretPaperBird"));
                foreach (RosterEntry entry in owner.Session.State.Roster)
                {
                    UnitDefinition unit = owner.Content.Unit(entry.UnitId);
                    if (unit != null && unit.Id.StartsWith("Secret", System.StringComparison.Ordinal) && guests.Contains(unit)) continue;
                    Add(unit, owner.Content.Weapon(entry.WeaponId), entry.Tier);
                }
                Add(owner.Content.Unit(owner.Session.State.ChampionId));
            }
            GUI.BeginGroup(image);
            Rect stage = new Rect(30, image.height - 190, image.width - 60, 170);
            bool reduced = PresentationSettings.ReducedMotion;
            float opening = reduced ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / 1.4f));
            Rect doorway = new Rect(stage.xMax - 150, stage.y + 2, 130, 163);
            ArcadeSkin.Glow(new Rect(doorway.x - 170, stage.yMax - 75, 330, 80), new Color(1, .75f, .36f, opening * .23f));
            Color doorTint = GUI.color;
            GUI.color = new Color(1, 1, 1, opening);
            RunHud.Sprite(doorway, NightmareAssets.Current.Scenery[7]);
            GUI.color = doorTint;
            for (int index = 0; index < guests.Count; index++)
            {
                UnitDefinition unit = guests[index];
                float stagger = guests.Count > 8 ? 1.6f / Mathf.Max(1, guests.Count - 1) : .22f;
                float localElapsed = elapsed + index * (.22f - stagger);
                float travel = Travel(localElapsed, index, reduced);
                float start = stage.x + 40 + index % 10 * 55;
                float horizontal = Mathf.Lerp(start, doorway.center.x - 12, travel);
                float rowStep = Mathf.Min(28,56f / Mathf.Max(1,Mathf.CeilToInt(guests.Count / 10f)-1));
                float bob = (reduced ? 0 : Mathf.Sin(elapsed * 9 + index) * 3 * (travel > 0 && travel < 1 ? 1 : 0)) - index / 10 * rowStep * (1 - travel);
                float size = (58 + Mathf.Clamp(unit.BodyHeight, .5f, 2) * 10) * (1 + (tiers[index] - 1) * .12f);
                Sprite sprite = EndingTravellers.Frame(unit, elapsed, index, travel > 0 && travel < 1);
                Vector3 acting = EnsembleMotion.Goodbye(unit, localElapsed, index, reduced);
                CharacterActingEntry art = EndingTravellers.IsStory(unit) ? null : CharacterActingLibrary.Current?.Find(unit.Id);
                if (!reduced && art != null && acting.x > 0) sprite = CharacterActingEntry.Frame(art.Curious, acting.x * .7f);
                if (!reduced && art != null && localElapsed - index * .22f >= 1.65f && localElapsed - index * .22f < 2.3f) sprite = CharacterActingEntry.Frame(art.Cheer, (localElapsed - index * .22f - 1.65f) / .65f);
                Color previous = GUI.color;
                float fade = Visibility(travel);
                GUI.color = new Color(0, 0, 0, fade * .3f);
                RunHud.Sprite(new Rect(horizontal - size * .5f, stage.yMax - 25, size, 13), sprite);
                GUI.color = new Color(1, 1, 1, fade);
                Rect actor = new Rect(horizontal - size * .5f, stage.yMax - size - 18 + bob - acting.z, size, size);
                Matrix4x4 matrix = GUI.matrix;
                GUIUtility.RotateAroundPivot(acting.y, new Vector2(actor.center.x, actor.yMax));
                if (acting.x > .3f) GUIUtility.ScaleAroundPivot(new Vector2(-1, 1), actor.center);
                RunHud.Sprite(actor, sprite);
                GUI.matrix = matrix;
                WeaponDefinition weapon = weapons[index];
                if (weapon != null && weapon.Sprite != null)
                    RunHud.Sprite(new Rect(horizontal + size * (acting.x > .3f ? -.7f : .15f), stage.yMax - size * .6f + bob - 15 - acting.z, size * .55f, size * .4f), weapon.Sprite);
                GUI.color = previous;
            }
            GUI.EndGroup();
        }
        private void Add(UnitDefinition unit, WeaponDefinition weapon = null, int tier = 1)
        {
            if (unit == null) return;
            guests.Add(unit); weapons.Add(weapon != null ? weapon : unit.Weapon); tiers.Add(Mathf.Clamp(tier, 1, 3));
        }
    }
}
