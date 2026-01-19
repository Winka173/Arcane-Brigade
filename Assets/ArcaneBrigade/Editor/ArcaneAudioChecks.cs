using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>
    /// Structural checks for the Arcane audio library (manifest, files, import settings, event mapping, stem alignment).
    /// These prove the data is wired; they do NOT certify how anything sounds. See Docs/Audio/Audio_QA_Checklist.md.
    /// </summary>
    public static class ArcaneAudioChecks
    {
        private static readonly List<string> notes = new List<string>();

        private static void Require(bool condition, string caption)
        {
            if (!condition) throw new InvalidOperationException("Arcane audio: " + caption);
            notes.Add("PASS " + caption);
        }

        [MenuItem("Arcane Brigade/Audio/Arcane audio checks")]
        public static void Run()
        {
            notes.Clear();
            ArcaneCueLibrary.Enabled = true;
            Require(ArcaneCueLibrary.Available && ArcaneCueLibrary.CueCount >= 100, "cue manifest loads (" + ArcaneCueLibrary.CueCount + " cues)");
            var seen = new HashSet<string>();
            int clipCount = 0;
            foreach (ArcaneCueDef def in ArcaneCueLibrary.All)
            {
                Require(seen.Add(def.id), "unique cue id " + def.id);
                Require(def.clips != null && def.clips.Length > 0 && def.maxVoices >= 1 && def.cooldown >= 0f, "cue fields " + def.id);
                Require(Enum.TryParse(def.bus, out AudioBus _), "known bus for " + def.id);
                foreach (string clip in def.clips)
                {
                    AudioClip loaded = Resources.Load<AudioClip>("Audio/Arcane/" + clip);
                    Require(loaded != null, "clip imports: " + clip);
                    Require(loaded.channels == 1 || def.busId == AudioBus.Ambience, "mono SFX/voice: " + clip);
                    clipCount++;
                }
            }
            Require(clipCount >= 300, "library clip count " + clipCount);

            foreach (ArcanePieceDef piece in ArcaneCueLibrary.Pieces)
            {
                AudioClip[] stems = ArcaneCueLibrary.LoadStems(piece);
                Require(stems != null && stems.Length == 4, "stems import for " + piece.id);
                foreach (AudioClip stem in stems)
                {
                    Require(stem.channels == 2 && stem.frequency == 44100, piece.id + " stem stereo 44.1k");
                    Require(Mathf.Abs(stem.samples - piece.loopSamples) <= 1, piece.id + " stem " + stem.name + " has exact loop length");
                    var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(stem));
                    Require(importer.defaultSampleSettings.loadType == AudioClipLoadType.Streaming, piece.id + " stems stream (mobile RAM)");
                }
                Require(piece.scenes != null && piece.scenes.Length > 0 && piece.bpm > 0 && piece.bars > 0, piece.id + " manifest fields");
            }
            foreach (string scene in new[] { ArcaneCueLibrary.SceneMenu, ArcaneCueLibrary.SceneSelect, ArcaneCueLibrary.SceneCombatEarly, ArcaneCueLibrary.SceneCombatMid, ArcaneCueLibrary.SceneCombatLate, ArcaneCueLibrary.SceneBoss, ArcaneCueLibrary.SceneEndless, ArcaneCueLibrary.SceneShopGold, ArcaneCueLibrary.SceneShopDream })
                Require(ArcaneCueLibrary.PieceForScene(scene, 0) != null, "a composition exists for scene " + scene);

            for (int cue = 0; cue < 39; cue++)
            {
                string id = ArcaneCueLibrary.LegacyId(cue);
                Require(id == null || ArcaneCueLibrary.Get(id) != null, "legacy cue " + cue + " maps to a real cue (" + id + ")");
            }
            Require(ArcaneCueLibrary.SceneFor(SessionPhase.Combat, false, 1, 20, false, false) == ArcaneCueLibrary.SceneCombatEarly
                && ArcaneCueLibrary.SceneFor(SessionPhase.Combat, false, 10, 20, false, false) == ArcaneCueLibrary.SceneCombatMid
                && ArcaneCueLibrary.SceneFor(SessionPhase.Combat, false, 19, 20, false, false) == ArcaneCueLibrary.SceneCombatLate
                && ArcaneCueLibrary.SceneFor(SessionPhase.Combat, true, 5, 20, false, false) == ArcaneCueLibrary.SceneBoss
                && ArcaneCueLibrary.SceneFor(SessionPhase.Combat, false, 25, 20, true, false) == ArcaneCueLibrary.SceneEndless
                && ArcaneCueLibrary.SceneFor(SessionPhase.Shop, false, 3, 20, false, true) == ArcaneCueLibrary.SceneShopDream
                && ArcaneCueLibrary.SceneFor(SessionPhase.Shop, false, 3, 20, false, false) == ArcaneCueLibrary.SceneShopGold, "scene selection by phase/wave/boss/endless/dream shop");

            var levels = new float[4];
            ArcadeMusic.StemTargets(ArcaneCueLibrary.SceneCombatMid, 0f, levels);
            float calmIntensity = levels[3];
            ArcadeMusic.StemTargets(ArcaneCueLibrary.SceneCombatMid, 1f, levels);
            Require(calmIntensity < 0.01f && levels[3] > 0.99f && levels[0] == 1f, "intensity layer follows enemy density");

            // Event contract (Docs/Audio/Audio_Integration_Contract.md): facts about CombatEvent that EventId relies on.
            Require(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Windup, Vector2.zero, Faction.Enemy)) == "boss.telegraph", "bare enemy Windup (boss pattern) is the boss telegraph");
            Require(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Fire, Vector2.zero, Faction.Enemy)) == "boss.attack", "bare enemy Fire (boss pattern) is the boss attack");
            Require(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Proc, Vector2.zero, Faction.Champion)) == "up.chain", "item proc maps to up.chain");
            Require(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Champion, 4f)) == "plr.hurt", "hit on the champion is plr.hurt");
            foreach (string[] pair in new[] { new[] { "ember-status", "fx.burn" }, new[] { "element-reaction", "fx.reaction.steam" }, new[] { "arcane-chain", "fx.reaction.arcane" }, new[] { "expanded-fuse", "sig.alarm.boom" }, new[] { "zipper-link", "fx.field.zipper" }, new[] { "expanded-officehazard", "fx.hazard.hit" } })
                Require(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, 5f, cause: new CombatCause(1, 0, pair[0]))) == pair[1], "secondary hit cause " + pair[0] + " -> " + pair[1]);
            ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Hit, Vector2.zero, Faction.Enemy, 5f, cause: new CombatCause(1, 0, "projectile-split")), out float splitScale);
            Require(splitScale < 1f, "split fragments hit quieter than the primary impact");
            foreach (string id in new[] { "up.merge.gather", "sig.alarm.boom", "fx.field.zipper", "fx.hazard.warn", "fx.hazard.hit", "enemy.mimic.wake", "hero.pulse.ember", "hero.pulse.frost", "ui.locked", "ui.error", "shop.reroll" })
                Require(ArcaneCueLibrary.Get(id) != null, "contract cue exists: " + id);
            Require(ArcaneCueLibrary.LegacyId(21) == "shop.reroll", "legacy reroll cue 21 maps to the library");
            foreach (var pair in new[]
            {
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.ProjectileReturn, "wpn.boomerang.return"),
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.ProjectilePierce, "wpn.pierce"),
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.ProjectileSplit, "wpn.split"),
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.ProjectileBounce, "wpn.bounce"),
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.Knockback, "fx.knockback"),
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.BossPhase, "boss.phase"),
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.ContactWindup, "enemy.lunge"),
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.LootAttract, "loot.coin.vacuum"),
                new KeyValuePair<CombatEventKind, string>(CombatEventKind.MimicWake, "enemy.mimic.wake")
            }) Require(ArcaneCueLibrary.EventId(new CombatEvent(pair.Key, Vector2.zero, Faction.Champion)) == pair.Value, "new event mapping " + pair.Key);
            Require(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.LootDrop, Vector2.zero, Faction.Champion, loot: LootKind.Chest)) == "loot.chest.appear", "chest spawn mapping");
            Require(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.StatusApplied, Vector2.zero, Faction.Enemy, element: DamageElement.Frost)) == "fx.chill", "new chill application mapping");

            string text = string.Join("\n", notes);
            File.WriteAllText("Library/ArcaneAudioChecks.txt", text);
            Debug.Log("ARCANE_AUDIO_CHECKS_PASSED " + notes.Count + " assertions");
        }
    }
}
