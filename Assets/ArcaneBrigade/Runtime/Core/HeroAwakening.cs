using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>A hero's awakening (18/10/2026, pilot for Postmaster, CopperCauldron and DrowsySeer): a three-tier meter
    /// above the hero's own mechanic. Every time the mechanic casts it climbs a tier; tiers I and II strengthen the
    /// mechanic by 12% each; at III the hero transforms by itself for six seconds (no button), then cools for three.
    /// <list type="bullet">
    /// <item>Postmaster, SORTING GATLING: a six-barrel parcel sorter fires a homing parcel every 0.2 s, round-robin over
    /// the stamped addresses (or the nearest enemy), and every parcel stamps what it hits.</item>
    /// <item>CopperCauldron, STEAM ENGINE: the kettle runs as a locomotive (60% faster), ramming and shoving what is in
    /// front of it; pressure rises with time and with every blow taken; at the red line, or when the run ends, it blows
    /// its whistle as one long steam jet ahead (never a burst round itself).</item>
    /// <item>DrowsySeer, FULL MOON: three dreams of the seer walk the route it took one, two and three seconds ago,
    /// each casting at enemies near it.</item>
    /// </list></summary>
    public sealed partial class CombatSimulation
    {
        public const float AwakenSeconds = 6f, AwakenCooldown = 3f, AwakenTierBonus = .12f;
        private float awakenMeter, awakenUntil = -1, awakenStarted = -100, awakenCoolUntil = -1, awakenShotAt, awakenRamAt, awakenPressure, awakenEchoAt;
        private int awakenRound;
        private readonly List<Vector2> awakenPath = new List<Vector2>();
        private readonly Vector2[] awakenEchoes = new Vector2[3];

        /// <summary>The heroes with an awakening so far.</summary>
        public static bool HasAwakening(string heroId) => heroId == "Postmaster" || heroId == "CopperCauldron" || heroId == "DrowsySeer";
        public bool HeroAwakenEnabled => HeroIdentityEnabled && HasAwakening(IdentityId);
        /// <summary>0..1: one third per mechanic cast; full transforms.</summary>
        public float HeroAwakenMeter => awakenMeter;
        public int HeroAwakenTier => Mathf.Clamp(Mathf.FloorToInt(awakenMeter * 3 + .001f), 0, 3);
        public bool HeroAwakening => HeroAwakenEnabled && awakenUntil > Elapsed;
        /// <summary>1 at the start of a transformation, 0 at its end.</summary>
        public float HeroAwakenRemaining => HeroAwakening ? Mathf.Clamp01((awakenUntil - Elapsed) / AwakenSeconds) : 0;
        public float HeroAwakenStarted => awakenStarted;
        public bool HeroAwakenCooling => HeroAwakenEnabled && !HeroAwakening && awakenCoolUntil > Elapsed;
        /// <summary>The steam engine's boiler pressure, 0..1 (1 is the red line).</summary>
        public float HeroAwakenPressure => awakenPressure;
        /// <summary>Where the full-moon dreams walk (valid while awakening as DrowsySeer).</summary>
        public IReadOnlyList<Vector2> HeroAwakenEchoes => awakenEchoes;
        /// <summary>The Postmaster's sorting gatling replaces the hero's own attacks while it runs.</summary>
        private bool HeroAwakenReplacesAttacks => HeroAwakening && IdentityId == "Postmaster";
        private Vector2 awakenAim;
        /// <summary>Where the transformed weapon last aimed.</summary>
        public Vector2 HeroAwakenAim => HeroAwakening ? awakenAim : Vector2.zero;
        /// <summary>How fast the hero runs (the steam engine is quicker).</summary>
        private float ChampionSpeedScale => HeroAwakening && IdentityId == "CopperCauldron" ? 1.6f : 1f;
        /// <summary>The mechanic's strength: tiers I and II add 12% each while not transformed.</summary>
        private float AwakenScale => HeroAwakenEnabled && !HeroAwakening ? 1 + AwakenTierBonus * Mathf.Min(2, HeroAwakenTier) : 1;

        /// <summary>Captures only: fill the gauge so the next moment with an enemy near transforms.</summary>
        public void PrimeAwakeningForCapture() { if (HeroAwakenEnabled) { awakenMeter = 1; awakenCoolUntil = -1; } }
        private void ResetAwakening()
        {
            awakenMeter = awakenPressure = 0; awakenUntil = awakenCoolUntil = -1; awakenStarted = -100; awakenShotAt = awakenRamAt = awakenEchoAt = 0; awakenRound = 0;
            awakenPath.Clear(); for (int i = 0; i < awakenEchoes.Length; i++) awakenEchoes[i] = Champion != null ? Champion.Position : Vector2.zero;
            if (Champion != null) Champion.AwakenArt = null;
        }
        /// <summary>Called on every mechanic cast: climb a tier.</summary>
        private void AwakenOnCast()
        {
            if (!HeroAwakenEnabled || HeroAwakening || awakenCoolUntil > Elapsed) return;
            awakenMeter = Mathf.Min(1, awakenMeter + 1f / 3f);
        }
        /// <summary>Runs before the hero's own mechanic; true while transformed (the mechanic itself rests meanwhile).</summary>
        private bool ProcessAwakening(float delta, bool walking, UnitState target)
        {
            if (!HeroAwakenEnabled) return false;
            if (awakenPath.Count == 0 || (Champion.Position - awakenPath[awakenPath.Count - 1]).sqrMagnitude > .0004f || awakenPath.Count < 2)
            {
                awakenPath.Add(Champion.Position); if (awakenPath.Count > 120) awakenPath.RemoveAt(0);
            }
            if (!HeroAwakening)
            {
                if (awakenUntil > 0 && Elapsed >= awakenUntil) EndAwakening();
                if (awakenMeter >= 1 && target != null) BeginAwakening();
                if (!HeroAwakening) return false;
            }
            switch (IdentityId)
            {
                case "Postmaster": SortingGatling(); break;
                case "CopperCauldron": SteamEngine(delta); break;
                case "DrowsySeer": FullMoon(); break;
            }
            if (HeroAwakening && Elapsed >= awakenUntil - .0001f) EndAwakening();
            return true;
        }
        private void BeginAwakening()
        {
            awakenStarted = Elapsed; awakenUntil = Elapsed + AwakenSeconds; awakenMeter = 1; awakenPressure = 0;
            awakenShotAt = awakenRamAt = awakenEchoAt = Elapsed; awakenRound = 0;
            Champion.AwakenArt = IdentityId == "Postmaster" ? "Postmaster_Awake" : IdentityId == "DrowsySeer" ? "DrowsySeer_Awake" : null;
            identityCharge = 0;
            var cause = IdentityCause();
            events.Add(new CombatEvent(CombatEventKind.HeroIdentityCast, Champion.Position, Faction.Champion, 1, subject: Champion, cause: cause));
            IdentityGlyph(MorphGeometry.Seal, Champion.Position, Champion.Position, .5f, .9f, role: HeroGlyphRole.Emblem);
        }
        private void EndAwakening()
        {
            if (IdentityId == "CopperCauldron" && awakenUntil > 0) Whistle();
            awakenUntil = -1; awakenMeter = 0; awakenCoolUntil = Elapsed + AwakenCooldown; Champion.AwakenArt = null;
        }

        private void SortingGatling()
        {
            if (Elapsed < awakenShotAt) return;
            awakenShotAt = Elapsed + .2f;
            UnitState aim = null;
            int marks = identityMarks.Count;
            for (int k = 0; k < marks && aim == null; k++) { var m = identityMarks[(awakenRound + k) % marks]; if (m.Target.Alive) aim = m.Target; }
            awakenRound++;
            if (aim == null) aim = IdentityTarget(Champion.Position, 8);
            if (aim == null) return;
            IdentityMarkTarget(aim);
            // the hero turns the gatling on the parcel's address and it kicks back a little: one shot of a held burst
            awakenAim = (aim.Position - Champion.Position).sqrMagnitude > .0001f ? (aim.Position - Champion.Position).normalized : Champion.Facing;
            Champion.AttackDirection = awakenAim; Champion.AttackTargetId = aim.Id; Champion.AttackWindup = .04f; Champion.AttackStarted = Elapsed - .04f;
            // a shot like any gun's, so the barrel flashes and the gun sounds (an identity cause: it charges nothing)
            events.Add(new CombatEvent(CombatEventKind.Fire, Champion.Position, Champion.Faction, 0f, awakenAim, Champion.Definition.Weapon, subject: Champion, cause: IdentityCause()));
            IdentityShot(Champion.Position, aim.Position - Champion.Position, IdentityPower * .14f, element: IdentityApproach == 1 ? DamageElement.Ember : DamageElement.Physical, homingTarget: aim.Id);
        }

        private void SteamEngine(float delta)
        {
            awakenPressure = Mathf.Min(1, awakenPressure + delta / (AwakenSeconds * 1.15f));
            Vector2 heading = identityHeading.sqrMagnitude > .01f ? identityHeading.normalized : Champion.Facing;
            if (Elapsed >= awakenRamAt)
            {
                awakenRamAt = Elapsed + .3f;
                Vector2 front = Champion.Position + heading * (Champion.Definition.Radius + .45f);
                IdentityHit(MorphGeometry.Disc, front, front + heading * .4f, .75f, IdentityPower * .24f, .05f, 4, DamageElement.Ember, push: .45f);
            }
            if (awakenPressure >= 1) { awakenUntil = Elapsed; }
        }
        /// <summary>A blow taken while the engine runs builds pressure faster.</summary>
        private void AwakenOnInjury()
        {
            if (HeroAwakening && IdentityId == "CopperCauldron") awakenPressure = Mathf.Min(1, awakenPressure + .15f);
        }
        /// <summary>The engine's whistle: one long steam jet ahead, stronger the higher the pressure.</summary>
        private void Whistle()
        {
            Vector2 heading = identityHeading.sqrMagnitude > .01f ? identityHeading.normalized : Champion.Facing;
            Vector2 from = Champion.Position + heading * (Champion.Definition.Radius + .2f);
            IdentityHit(MorphGeometry.Line, from, from + heading * MaxLineLength, 0, IdentityPower * (1f + 1f * awakenPressure), .25f, 6, DamageElement.Ember, push: .4f);
            awakenPressure = 0;
        }

        private void FullMoon()
        {
            for (int k = 0; k < awakenEchoes.Length; k++) awakenEchoes[k] = PathAgo(k + 1);
            if (Elapsed < awakenEchoAt) return;
            awakenEchoAt = Elapsed + .3f;
            int k2 = awakenRound++ % awakenEchoes.Length;
            UnitState near = IdentityTarget(awakenEchoes[k2], 5);
            if (near != null) IdentityPattern(near, IdentityPower * .38f, origin: awakenEchoes[k2], element: IdentityApproach == 2 ? DamageElement.Frost : DamageElement.Arcane);
        }
        /// <summary>Where the hero stood about `seconds` ago (the path is sampled each step the hero moves).</summary>
        private Vector2 PathAgo(int seconds)
        {
            if (awakenPath.Count == 0) return Champion.Position;
            // sampled every simulation step the hero moved; walk back a distance worth of `seconds` at the hero's speed
            float want = seconds * Champion.Definition.Speed * .55f, walked = 0;
            for (int i = awakenPath.Count - 1; i > 0; i--)
            {
                walked += Vector2.Distance(awakenPath[i], awakenPath[i - 1]);
                if (walked >= want) return awakenPath[i - 1];
            }
            return awakenPath[0];
        }
    }
}
