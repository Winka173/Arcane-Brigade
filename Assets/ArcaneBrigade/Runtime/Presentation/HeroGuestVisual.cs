using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Our friends' skills, their traits and their chained blows are drawn by the hero renderer itself, so every
    /// effect on our side has the hero skills' look and pace (requirement 28/10/2026: Storm Fencer's lightning is the
    /// standard): the gathering line, the strike running out and drawing back as thick soft light, the creature's own
    /// tokens riding it and the element's signature where it lands. Each element borrows one hero's brush, palette and
    /// signature; the tokens on the stroke stay the creature's own piece.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private sealed class Guest { public HeroIdentityGlyph Glyph; public HeroMechanicStyle Style; }
        private static readonly List<Guest> guests = new List<Guest>();
        private const int MaxGuests = 48;

        /// <summary>The hero whose brush, palette and signature an element's effects borrow.</summary>
        public static string GuestHero(ElementLook look) => look switch
        {
            ElementLook.Storm => "StormFencer",
            ElementLook.Fire => "CometRunner",
            ElementLook.Water => "BellDiver",
            ElementLook.Ice or ElementLook.Crystal => "MirrorKeeper",
            ElementLook.Bloom => "PetalCitadel",
            ElementLook.Void or ElementLook.Dream => "MoonProwler",
            ElementLook.Light => "LanternKnight",
            ElementLook.Gold => "PennyBroker",
            ElementLook.Sound => "TinVirtuoso",
            ElementLook.Time => "HourglassStag",
            ElementLook.Wind => "KiteAdmiral",
            ElementLook.Poison => "SoapSkater",
            ElementLook.Paper => "ArchiveCurator",
            ElementLook.Earth => "RootOracle",
            _ => "JugglingJester",
        };

        /// <summary>An element's borrowed hero style, its tokens swapped for the creature's own piece when it has one.</summary>
        public static HeroMechanicStyle GuestStyle(ElementLook look, InkToken? token = null)
        {
            HeroMechanicStyle hero = HeroMechanicStyle.For(GuestHero(look));
            return token.HasValue ? new HeroMechanicStyle(hero.Id, token.Value, hero.Brush, hero.Gauge, hero.Palette) : hero;
        }

        /// <summary>Queue one of our side's effects for the hero renderer (times are simulation seconds).</summary>
        public static void AddGuest(HeroIdentityGlyph glyph, HeroMechanicStyle style)
        {
            if (glyph == null || style == null) return;
            if (guests.Count >= MaxGuests) guests.RemoveAt(0);
            guests.Add(new Guest { Glyph = glyph, Style = style });
        }

        public static int GuestCount => guests.Count;

        private void DrawGuests(CombatSimulation simulation, bool live)
        {
            bool fighting = simulation != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (!fighting) { if (simulation == null || simulation.Phase != RunPhase.Paused) guests.Clear(); return; }
            if (!live)
            {
                // the hero's own mechanic is off (or the hero is down): set up the frame the way Display does for it
                sim = simulation; t = simulation.Elapsed;
                still = PresentationSettings.ReducedMotion; sparse = PresentationSettings.ReducedEffects || AnimationDetailBudget.Level >= 2;
                calm = PresentationSettings.ReducedFlashes; plain = PresentationSettings.HighReadability;
                strength = Mathf.Lerp(1, .72f, EffectDensity.CrowdPressure) * (sparse ? .85f : 1) * 1.25f;
                sparse |= EffectDensity.CrowdPressure > .6f;
                var cam = Camera.main; screenCap = cam != null && cam.orthographic ? Mathf.Max(1.2f, cam.orthographicSize * .45f) : 3;
            }
            HeroMechanicStyle ownStyle = style; InkPalette ownInk = ink;
            for (int i = guests.Count - 1; i >= 0; i--)
            {
                Guest g = guests[i];
                if (t > g.Glyph.Until + .05f || t < g.Glyph.Created - 30) { guests.RemoveAt(i); continue; }
                if (t < g.Glyph.Created) continue;
                style = g.Style; ink = g.Style.Palette;
                if (DrawGlyph(g.Glyph)) VisibleCount++;
            }
            style = ownStyle; ink = ownInk;
        }
    }
}
