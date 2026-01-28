using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class AnimationPolishBuilder
    {
        [MenuItem("Arcane Brigade/Animation/Import and verify polish")]
        public static void ImportAndCheck()
        {
            CharacterActingBuilder.Build();
            EnsembleBuilder.Build();
            Sprite[] frames = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/TactileImpacts_v2.png", "TactileImpact", 4, 4, Enumerable.Range(0, 16).ToArray(), new[] { 0, 256, 512, 768, 1024 }, true);
            const string path = "Assets/ArcaneBrigade/Resources/TactileAnimationLibrary.asset";
            TactileAnimationLibrary art = AssetDatabase.LoadAssetAtPath<TactileAnimationLibrary>(path);
            if (art == null) { art = ScriptableObject.CreateInstance<TactileAnimationLibrary>(); AssetDatabase.CreateAsset(art, path); }
            art.Paper = frames.Take(4).ToArray(); art.Metal = frames.Skip(4).Take(4).ToArray();
            art.Wax = frames.Skip(8).Take(4).ToArray(); art.Dust = frames.Skip(12).Take(4).ToArray();
            art.Chests = K25AtlasImporter.Import("Assets/ArcaneBrigade/Art/Nightmare/LivingChest_v1.png", "LivingChest", 4, 1, Enumerable.Range(0, 4).ToArray(), new[] { 0, 1024 }, true);
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            catalog.ChestSprite = art.Chests[0]; EditorUtility.SetDirty(catalog);
            EditorUtility.SetDirty(art); AssetDatabase.SaveAssets(); Run();
        }
        public static void Run()
        {
            var notes = new List<string>();
            void Check(bool valid, string caption) { if (!valid) throw new InvalidOperationException("Animation polish: " + caption); notes.Add("PASS " + caption); }
            TactileAnimationLibrary art = TactileAnimationLibrary.Current;
            Check(art != null && art.Dust.Length == 4, "Original painted four-frame dust imported");
            Check(art.Chests.Length == 4 && art.Chests.All(frame => frame != null), "Four original chest opening poses imported");
            foreach (Sprite frame in art.Paper.Concat(art.Metal).Concat(art.Wax).Concat(art.Dust))
            {
                int slot = int.Parse(frame.name.Substring(frame.name.Length - 2));
                Check(frame.rect.xMin > 1 && frame.rect.xMax < frame.texture.width - 1 && frame.rect.yMin > 1 && frame.rect.yMax < frame.texture.height - 1 && frame.rect.width > 8 && frame.rect.height > 8, "Connected painted cluster has complete source margins " + slot);
            }
            foreach (BodyMaterial material in Enum.GetValues(typeof(BodyMaterial))) Check(art.For(material).Length == 4 && art.For(material).All(frame => frame != null), "Material impact coverage " + material);
            Check(AnimationTiming.Pulse(-1, 1) == 0 && AnimationTiming.Pulse(1, 1) == 0 && AnimationTiming.Pulse(1, 0) == 0, "Pulse bounds and zero duration");
            Check(Mathf.Abs(AnimationTiming.Pulse(.5f, 1) - 1) < .0001f, "Pulse crest");
            Check(AnimationTiming.Settle(1, 1) == 0 && AnimationTiming.Settle(-1, 1) == 0, "Settling has no persistent offset");
            Check(AnimationTiming.Blend(12, 0) == 0 && AnimationTiming.Blend(12, 10) == AnimationTiming.Blend(12, .05f), "Frame gap bounded");
            Check(AnimationTiming.Phase("LanternKnight") == AnimationTiming.Phase("LanternKnight") && AnimationTiming.Phase("LanternKnight") != AnimationTiming.Phase("SoloMaw"), "Stable identity phase without RNG");
            Check(AnimationTiming.BossAct(100, 100) == 0 && AnimationTiming.BossAct(60, 100) == 1 && AnimationTiming.BossAct(30, 100) == 2 && AnimationTiming.BossAct(0, 0) == 0, "Boss act boundaries");
            Check(Resources.Load<Material>("UnitFlash").HasProperty("_WeaponMask"), "Weapon source supports articulated blade mask");
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            foreach (UnitDefinition hero in catalog.Champions)
            {
                var unit = new UnitState { Id = 29, Definition = hero, Health = hero.Health, Faction = Faction.Champion };
                var acting = new AnimationPerformance(); acting.Reset(unit);
                string before = JsonUtility.ToJson(unit);
                for (int index = 0; index < 20; index++) acting.Sample(unit, index * .05f, false, false);
                Check(JsonUtility.ToJson(unit) == before, "Presentation cannot mutate unit " + hero.Id);
                unit.Velocity = Vector2.right * 6;
                acting.Sample(unit, 1, false, false); acting.Sample(unit, 1.05f, false, false);
                Check(float.IsFinite(acting.BodyTurn) && acting.BodyOffset.magnitude <= .1f, "Bounded acceleration acting " + hero.Id);
                Vector2 offset = acting.BodyOffset; float turn = acting.GripTurn;
                acting.Sample(unit, 1.05f, false, false);
                Check(offset == acting.BodyOffset && turn == acting.GripTurn, "Repeated same-clock sample freezes " + hero.Id);
                acting.Sample(unit, 1.1f, true, false);
                Check(acting.BodyOffset == Vector2.zero && acting.GripTurn == 0 && acting.Gesture == 0, "Reduced motion clears acting " + hero.Id);
                acting.Reset(unit); acting.Sample(unit, 0, false, false);
                Check(acting.BodyOffset == Vector2.zero, "Pooled actor reset " + hero.Id);
            }
            foreach (WeaponMotion motion in Enum.GetValues(typeof(WeaponMotion)))
            {
                WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
                try
                {
                    weapon.Motion = motion;
                    Check(AnimationPerformance.WeaponFlex(weapon, .05f, .1f, true) == Vector4.zero, "Reduced articulation " + motion);
                    Check(AnimationPerformance.WeaponFlex(weapon, 10, .1f, false) == Vector4.zero, "No articulation after recovery " + motion);
                    K8Motion.Pose pose = K8Motion.Attack(weapon, .12f, .1f, false);
                    Check(float.IsFinite(pose.Angle) && float.IsFinite(pose.Recoil), "Finite weapon choreography " + motion);
                }
                finally { UnityEngine.Object.DestroyImmediate(weapon); }
            }
            Check(MerchantPortraitRig.GesturePose(-1, -1, false, false) == Vector3.zero, "No merchant reaction before event");
            Check(MerchantPortraitRig.GesturePose(1, 1, true, false) == Vector3.zero, "Merchant handoff fully settles");
            Check(MerchantPortraitRig.GesturePose(.2f, .1f, true, true) == Vector3.zero, "Reduced merchant handoff stays stable");
            Check(MerchantPortraitRig.GesturePose(.2f, -1, true, false) != MerchantPortraitRig.GesturePose(.2f, -1, false, false), "Dream and gold merchant have distinct event acting");
            foreach (string id in new[] { "SleepCamera", "MusicBox", "MailboxLauncher" })
                Check(WeaponMechanism.Supports(Resources.Load<RunCatalog>("RunCatalog").Weapon(id)), "Source-specific mechanical profile " + id);
            Directory.CreateDirectory("PreviewExports/AnimationPolish"); File.WriteAllLines("PreviewExports/AnimationPolish/Focused_Checks.txt", notes);
            Debug.Log("ANIMATION_POLISH_CHECKS " + notes.Count + " PASS");
        }
        public static void Gate() { NightmareVisualChecks.Gate(); }
    }
}
