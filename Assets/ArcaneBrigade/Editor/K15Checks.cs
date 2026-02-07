using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K15Checks
    {
        public static void Run()
        {
            var notes = new List<string>();
            Action<bool, string> require = (condition, caption) => { if (!condition) throw new InvalidOperationException("K15 " + caption); notes.Add("PASS " + caption); };
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            ImpactAnimationLibrary art = Resources.Load<ImpactAnimationLibrary>("ImpactAnimationLibrary");
            require(art != null, "Original four-family atlas imported");
            foreach (Sprite[] frames in new[] { art.Cut, art.Slam, art.Pierce, art.Orb })
            {
                require(frames.Length == 4 && frames.Distinct().Count() == 4, "Four different authored frames");
                require(frames.All(frame => frame.rect.size == frames[0].rect.size && frame.pivot == frames[0].pivot), "Fixed cell/pivot prevents frame size popping");
            }
            require(art.For(catalog.Weapon("SpringKnocker")) == art.Slam, "Spring hammer uses heavy impact");
            require(art.For(catalog.Weapon("NeedleSinger")) == art.Pierce, "Needles use pinpoint impact");
            require(art.For(catalog.Weapon("RibbonWheel")) == art.Cut, "Ribbon uses cut impact");
            require(art.For(catalog.Weapon("DreamProjector")) == art.Orb, "Projector uses arcane impact");
            foreach (WeaponDefinition weapon in catalog.Weapons)
            {
                var value = new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, 10, Vector2.right, weapon);
                ImpactAccent accent = ImpactAccent.For(value);
                require(accent.Hold > 0 && accent.Hold <= 0.045f && accent.Camera <= 0.09f, "Bounded weapon feel " + weapon.Id);
            }
            float previousTimeScale = Time.timeScale;
            var feel = new CombatFeel();
            var hit = new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Champion, 10, Vector2.right);
            feel.Play(new[] { hit }, new Dictionary<int, UnitVisual>(), 1); feel.Advance(0.025f);
            require(feel.CameraOffset.magnitude > 0 && feel.CameraOffset.magnitude <= CombatFeel.MaxShake, "Camera accent has a bounded displacement");
            var heavy = new CombatFeel();
            var detonation = new CombatEvent(CombatEventKind.Detonation, Vector2.zero, Faction.Champion, 2, Vector2.right);
            heavy.Play(new[] { detonation }, new Dictionary<int, UnitVisual>(), 1);
            require(heavy.Frozen && !heavy.SlowMotion && heavy.Trauma > 0.1f, "An explosion holds the frame and shakes the camera");
            heavy.Advance(0.2f); require(!heavy.Frozen, "The hit hold is brief");
            var routine = new CombatFeel(); var kills = new List<CombatEvent>();
            for (int i = 0; i < 200; i++) kills.Add(new CombatEvent(CombatEventKind.Kill, Vector2.zero, Faction.Enemy, 0, Vector2.right, subjectId: i + 2));
            routine.Play(kills, new Dictionary<int, UnitVisual>(), 1);
            require(routine.Trauma <= 0.43f, "Swarm kills alone cannot shake the screen into noise");
            PresentationSettings.Apply(new PlayerProfile { ReducedShake = true });
            require(feel.CameraOffset == Vector2.zero, "Independent reduced shake removes accents");
            PresentationSettings.Apply(new PlayerProfile { ReducedMotion = true });
            require(feel.CameraOffset == Vector2.zero, "Reduced motion also removes accents");
            PresentationSettings.Apply(new PlayerProfile { ReducedEffects = true });
            require(EffectDensity.Limit(96) == 32 && !EffectDensity.CosmeticProjectile(1), "Reduced effects caps cosmetic work");
            var effectRoot = new GameObject("K15BudgetFixture");
            try
            {
                PaintedEffects effects = effectRoot.AddComponent<PaintedEffects>(); effects.SendMessage("Awake");
                for (int identity = 1; identity < 100; identity++) effects.Play(new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, 1, weapon: catalog.Weapon("NeedleSinger"), subjectId: identity));
                require(effects.ActiveCount == 21, "Real painted pool obeys reduced density, including reuse");
                effects.Clear(); PresentationSettings.Apply(new PlayerProfile());
                for (int identity = 1; identity < 100; identity++) effects.Play(new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, 1, weapon: catalog.Weapon("NeedleSinger"), subjectId: identity));
                require(effects.ActiveCount == 64, "Full painted pool stays bounded");
                effects.Advance(1); require(effects.ActiveCount == 0, "Expired impact sprites return to pool");
            }
            finally { UnityEngine.Object.DestroyImmediate(effectRoot); PresentationSettings.Apply(new PlayerProfile()); }
            PresentationSettings.Apply(new PlayerProfile()); feel.Advance(1);
            require(feel.CameraOffset == Vector2.zero && Time.timeScale == previousTimeScale, "No lasting camera offset or global time-scale change");
            PlayerProfile old = JsonUtility.FromJson<PlayerProfile>("{\"Version\":3}");
            require(!old.ReducedShake && !old.ReducedEffects && !old.ReducedFlashes, "Existing version3 profiles keep new defaults");
            PlayerProfile saved = JsonUtility.FromJson<PlayerProfile>(JsonUtility.ToJson(new PlayerProfile { ReducedShake = true, ReducedEffects = true, ReducedFlashes = true }));
            require(saved.ReducedShake && saved.ReducedEffects && saved.ReducedFlashes, "New accessibility fields survive serialization");
            for (int wave = 1; wave <= 20; wave++)
            {
                bool bounded = true;
                for (int sample = 0; sample < 240; sample++) { float factor = WaveCadence.IntervalMultiplier(wave, sample * 0.1f); bounded &= float.IsFinite(factor) && factor >= 0.88f && factor <= 1.45f; }
                require(bounded, "Bounded pressure cycle wave " + wave);
            }
            require(WaveCadence.IntervalMultiplier(1, 8) == 1 && WaveCadence.IntervalMultiplier(2, 8) == 1, "First two waves retain introduction cadence");
            require(WaveCadence.IntervalMultiplier(5, 9) > WaveCadence.IntervalMultiplier(5, 2), "Higher waves have a recovery beat");
            for (int identity = 0; identity < 32; identity++)
            {
                Vector2 position = new Vector2(3, 2);
                bool approaching = true;
                for (int step = 0; step < 80; step++) { Vector2 next = WaveCadence.Attract(position, Vector2.zero, 0.08f, identity); approaching &= next.sqrMagnitude <= position.sqrMagnitude; position = next; }
                require(approaching && position == Vector2.zero, "Curved loot converges without overshooting " + identity);
            }
            foreach (BodyMaterial material in Enum.GetValues(typeof(BodyMaterial)))
                require(MaterialDeathPose.Shape(material, 1).x > 0 && MaterialDeathPose.Shape(material, 1).y > 0, "Finite material death silhouette " + material);
            require(MaterialDeathPose.Shape(BodyMaterial.Wax, 1).x > MaterialDeathPose.Shape(BodyMaterial.Fabric, 1).x, "Wax puddle differs from folded fabric");
            using (var session = new RunSession(catalog))
            {
                session.Start("LanternKnight", 151077); CheckpointCatalog combat = session.PrepareCombat();
                var simulation = new CombatSimulation(combat); simulation.Start(151077);
                for (int step = 0; step < 180; step++) simulation.Step(1f / 60, Vector2.right);
                require(!simulation.Units.Any(unit => unit.Faction == Faction.Enemy && unit.Definition.Weapon.Kind == WeaponKind.Projectile), "Wave1 remains free of ranged attackers");
                Vector2 before = simulation.Champion.Position; simulation.Pause(true); simulation.Step(0.1f, Vector2.one);
                require(simulation.Champion.Position == before, "Simulation pause still blocks movement");
            }
            Directory.CreateDirectory("PreviewExports/K15"); File.WriteAllLines("PreviewExports/K15/Checks.txt", notes);
            Debug.Log("K15_CHECKS_PASSED " + notes.Count);
        }
    }
}
