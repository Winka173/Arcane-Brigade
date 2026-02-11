using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Who says what, and when, outside the purchase flow. It only observes state (the roster, the travelling four,
    /// the boss, the warm chair) and never changes it: a companion who joins or ranks up says a short line above its head,
    /// the story bosses bark on entry, attacks, wounds, the half-way turn and their fall, and the shopkeeper speaks when the
    /// shop is quiet or when someone wanders too close to the warm chair. Chatter keeps polite gaps (see Voice_System.md).</summary>
    public sealed class SpeechBubbleCast : MonoBehaviour
    {
        private sealed class Pending { public float At, Until; public Func<bool> Run; public bool Companion; }
        private const float CompanionGap = 2.6f;
        private ArenaController owner;
        private string runId;
        private bool baseline;
        private readonly Dictionary<string, int> roster = new Dictionary<string, int>();
        private readonly Dictionary<string, int> scratch = new Dictionary<string, int>();
        private readonly List<Pending> queue = new List<Pending>();
        private int shopWave = -1;
        private int hobRescued;
        private bool chairConfessed;
        private UnitState boss;
        private string bossSpeaker;
        private Vector2 bossLast;
        private float bossEnterAt, bossMark;
        private bool bossEntered, bossPhased, bossStory, bossDefeated;
        private int bossVolley;
        private float bossSkill;
        public int Joins { get; private set; }
        public int StarUps { get; private set; }
        public int BossLines { get; private set; }

        public void Configure(ArenaController controller) => owner = controller;
        private void OnDestroy() { SpeechBubbles.ClearAll(); }

        private void Update()
        {
            if (owner?.Session == null || owner.Simulation == null) return;
            RunSnapshot state = owner.Session.State;
            if (state.RunId != runId)
            {
                // a different run (restart): forget the old voices; the first look at a run keeps the welcome already said
                if (runId != null) SpeechBubbles.ClearAll();
                runId = state.RunId; baseline = false; queue.Clear(); boss = null; chairConfessed = false;
                shopWave = state.Phase == SessionPhase.Shop ? state.Wave : -1; hobRescued = state.ActOne?.HobRescued ?? 0;
            }
            bool blocked = owner.PresentationPaused || owner.StoryOpen || owner.TransitionPending || owner.Session.EndingPending || owner.Session.SecretRoomOpen;
            ObserveRoster(state);
            ObserveShop(state, blocked);
            ObserveBoss(state);
            Flush(blocked);
        }

        // ----- party -----

        private void ObserveRoster(RunSnapshot state)
        {
            if (state.Phase != SessionPhase.Shop && state.Phase != SessionPhase.Combat) return;
            scratch.Clear();
            foreach (RosterEntry entry in state.Roster) { string key = entry.UnitId + "#" + entry.Tier; scratch[key] = (scratch.TryGetValue(key, out int count) ? count : 0) + 1; }
            if (baseline)
            {
                foreach (KeyValuePair<string, int> pair in scratch)
                {
                    int before = roster.TryGetValue(pair.Key, out int count) ? count : 0;
                    if (pair.Value <= before) continue;
                    int split = pair.Key.LastIndexOf('#');
                    string id = pair.Key.Substring(0, split); int tier = int.Parse(pair.Key.Substring(split + 1));
                    if (tier >= 2) StarUp(id, tier); else Join(id);
                }
            }
            roster.Clear(); foreach (KeyValuePair<string, int> pair in scratch) roster[pair.Key] = pair.Value;
            baseline = true;
        }

        private static readonly string[] JoinFallback = { "I'll walk with you.", "Room for one more?", "Hello. I'm coming too.", "Same road. Better company.", "Lead on. I'm right behind you.", "I was hoping someone would ask." };
        private static readonly string[] SecondStar = { "All three of us. One stubborn heart.", "Bigger shoes. Same feet.", "I remember all of me.", "We held on. Now we hold more.", "Stronger. Still walking with you.", "Three little histories. One of me." };
        private static readonly string[] ThirdStar = { "Everything I was, all at once.", "Brightest I have ever been.", "Whole. Finally whole.", "Nothing in the dark is this bright.", "Lean on me. I can take it." };
        private static int Hash(string value) { int hash = 17; foreach (char c in value ?? "") hash = hash * 31 + c; return hash & 0x7fffffff; }

        public static string JoinLine(string unitId, RunSnapshot state)
        {
            // their own authored first line (or story-arc line) reads best as a hello; biographies are the fallback
            string line = RunSession.Stranger(state, unitId) || !CompanionBanter.Has(unitId) ? CompanionVoice.Line(unitId, state.Wave, state) : CompanionBanter.Line(unitId, 0, state.Wave);
            return !string.IsNullOrEmpty(line) && line.Length <= 90 ? line : JoinFallback[Hash(unitId + state.Wave) % JoinFallback.Length];
        }
        public static string StarLine(string unitId, int tier, int wave)
        {
            string[] lines = tier >= 3 ? ThirdStar : SecondStar;
            return lines[Hash(unitId + tier + wave) % lines.Length];
        }

        private void Join(string id)
        {
            UnitDefinition unit = owner.Content.Unit(id);
            if (unit == null) return;
            string name = RunSession.Stranger(owner.Session.State, id) ? "???" : unit.DisplayName;
            string text = JoinLine(id, owner.Session.State);
            Enqueue(1.4f, 7f, true, () => CompanionSays(id, 1, name, text, "join", VoiceDirector.Chatter, SpeechTone.Companion));
        }

        private void StarUp(string id, int tier)
        {
            UnitDefinition unit = owner.Content.Unit(id);
            if (unit == null) return;
            string name = (RunSession.Stranger(owner.Session.State, id) ? "???" : unit.DisplayName) + (tier >= 3 ? "  ★★★" : "  ★★");
            string text = StarLine(id, tier, owner.Session.State.Wave);
            // the merge reveal plays first; the new star speaks once it has landed
            Enqueue(1.1f, 7f, true, () => CompanionSays(id, tier, name, text, "starup", VoiceDirector.Normal, SpeechTone.Companion));
        }

        private bool CompanionSays(string id, int tier, string name, string text, string trigger, int priority, SpeechTone tone)
        {
            Func<Vector3?> anchor = () => { UnitState unit = FindCompanion(id, tier); return unit == null ? (Vector3?)null : SpeechBubbles.Head(unit); };
            if (anchor() == null) return false;
            VoiceDirector.Say(VoiceLines.Companion, trigger, priority, true);
            SpeechBubbles.Show("Companion:" + id, name, text, anchor, tone, 0, priority + 1);
            if (trigger == "join") Joins++; else StarUps++;
            return true;
        }

        private UnitState FindCompanion(string id, int tier)
        {
            if (owner?.Simulation == null) return null;
            UnitState best = null;
            foreach (UnitState unit in owner.Simulation.Units)
                if (unit.Faction == Faction.Companion && unit.Alive && unit.Definition.Id == id && unit.Definition.CombatTier >= tier && (best == null || unit.Id > best.Id)) best = unit;
            return best;
        }

        private UnitState FindStory(string name)
        {
            if (owner?.Simulation == null) return null;
            foreach (UnitState unit in owner.Simulation.Units)
                if (unit.Faction == Faction.Companion && unit.Alive && unit.Definition.DisplayName == name) return unit;
            return null;
        }

        /// <summary>One of the travelling four speaks a numbered line from its bank (join, starup, story).</summary>
        public bool StorySays(int index, string trigger, int line = -1, int priority = VoiceDirector.Important)
        {
            string name = ActOneCanon.StoryNames[index];
            string speaker = VoiceDirector.StorySpeaker(name);
            Func<Vector3?> anchor = () => { UnitState unit = FindStory(name); return unit == null ? (Vector3?)null : SpeechBubbles.Head(unit); };
            if (anchor() == null) return false;
            VoiceLine voiced = line >= 0 ? VoiceDirector.Exact(speaker, trigger, line) : VoiceDirector.Say(speaker, trigger, priority, true);
            if (voiced == null) return false;
            if (line >= 0) VoiceDirector.Active?.Play(voiced, priority);
            SpeechBubbles.Show("Story:" + name, name, voiced.Text, anchor, SpeechTone.Story, 0, priority + 1);
            if (trigger == "join") Joins++; else if (trigger == "starup") StarUps++;
            return true;
        }

        // ----- the shop -----

        private void ObserveShop(RunSnapshot state, bool blocked)
        {
            if (state.Phase != SessionPhase.Shop || owner.Shop == null) return;
            if (shopWave != state.Wave)
            {
                shopWave = state.Wave;
                if (!state.Endless && state.Wave <= ActOneCanon.Waves) EnterShop(state);
            }
            int rescued = state.ActOne?.HobRescued ?? 0;
            if (rescued != hobRescued)
            {
                hobRescued = rescued;
                if (rescued != 0 && state.Wave == ActOneCanon.HobChapter)
                {
                    Enqueue(1.4f, 12f, false, () => owner.Shop.Dialogue.Say(false, "story:3", owner.PresentationClock, VoiceDirector.Story));
                    Enqueue(4.2f, 14f, true, () => StorySays(0, "starup", 1));
                }
            }
            if (blocked || owner.HallExitPending || owner.Shop.Animating || owner.Shop.Special) return;
            MerchantDialogue dialogue = owner.Shop.Dialogue;
            float clock = owner.PresentationClock;
            WaitingRealmStage chair = owner.GetComponent<WaitingRealmStage>();
            if (chair != null && chair.ChairVisible && !chair.Asking && owner.Simulation.Champion != null
                && Vector2.Distance(owner.Simulation.Champion.Position, chair.ChairAnchor) < 1.9f && clock > dialogue.SpokeAt + 1.2f)
            {
                // the shopkeeper never looks at it; from shop 8 the warning almost becomes a confession, once
                if (!chairConfessed && state.Wave >= ActOneCanon.SitFrom && state.Wave <= ActOneCanon.SitUntil) chairConfessed = dialogue.Say(false, "story:1", clock, VoiceDirector.Story);
                else if (VoiceDirector.Ready(VoiceLines.Shopkeeper, "chair")) dialogue.Say(false, "chair", clock, VoiceDirector.Important);
                return;
            }
            if (owner.Shop.Nearby == null && clock > dialogue.ExpiresAt + 9f && clock - dialogue.SpokeAt > 15f && VoiceDirector.Ready(VoiceLines.Shopkeeper, "idle"))
                dialogue.Say(false, "idle", clock, VoiceDirector.Chatter);
        }

        private void EnterShop(RunSnapshot state)
        {
            int wave = state.Wave;
            // the four meet you in the shop after their wave, once the shopkeeper has said hello
            for (int index = 0; index < ActOneCanon.StoryIds.Length; index++)
                if (ActOneCanon.JoinShop[index] == wave) { int captured = index; Enqueue(3.4f, 16f, true, () => StorySays(captured, "join", captured == 0 ? 1 : 0)); } // first words are the canon ones
            // the travelling four grow with the zone (tier 2 at shop 11, tier 3 at shop 16): one of them says so
            if (wave == 11) Enqueue(6.5f, 18f, true, () => StorySays(1, "starup"));
            if (wave == 19 && ActOneCanon.StoryPresent(3, wave, true, state.Endless)) Enqueue(3.4f, 16f, true, () => StorySays(3, "story", 0));
            if (wave == 19 && (state.ActOne?.SitCloser ?? 0) == ActOneProgress.Won)
            {
                Enqueue(6f, 18f, false, () => Offscreen(VoiceLines.LastCustomer, "THE LAST CUSTOMER", VoiceDirector.Exact(VoiceLines.LastCustomer, "story", 3)));
                Enqueue(8.6f, 20f, true, () => StorySays(0, "story", 1, VoiceDirector.Story));
            }
        }

        /// <summary>Someone who is not in the room speaks (a box at the top of the screen, no tail).</summary>
        public static bool Offscreen(string speaker, string name, VoiceLine line, int priority = VoiceDirector.Story)
        {
            if (line == null) return false;
            VoiceDirector.Active?.Play(line, priority);
            SpeechBubbles.Show(speaker, name, line.Text, null, SpeechTone.Offscreen, 0, priority + 1);
            return true;
        }

        // ----- bosses -----

        private void ObserveBoss(RunSnapshot state)
        {
            CombatSimulation simulation = owner.Simulation;
            UnitState current = state.Phase == SessionPhase.Combat ? simulation.Boss : null;
            float now = Time.unscaledTime;
            if (current != boss)
            {
                boss = current; bossSpeaker = VoiceDirector.BossSpeaker(current?.Definition);
                bossEntered = bossPhased = bossStory = bossDefeated = false; bossMark = 1; bossEnterAt = now + 1.1f;
                bossVolley = current?.BossVolley ?? 0; bossSkill = current?.BossSkillStarted ?? -100;
            }
            if (boss == null || bossSpeaker == null || owner.PresentationPaused) return;
            if (boss.Alive) bossLast = (Vector2)SpeechBubbles.Head(boss);
            float fraction = Mathf.Clamp01(boss.Health / Mathf.Max(1, boss.Definition.Health));
            if (!boss.Alive || boss.Health <= 0)
            {
                if (!bossDefeated && bossEntered) { bossDefeated = true; BossSays("defeat", VoiceDirector.Story, 4.6f); }
                return;
            }
            if (!bossEntered) { if (now >= bossEnterAt) { bossEntered = true; BossSays("enter", VoiceDirector.Important); } return; }
            bool customer = bossSpeaker == VoiceLines.LastCustomer; // his fight has its own scripted beats (NightmarePresentation)
            if (!bossPhased && fraction <= 0.5f) { bossPhased = true; bossMark = fraction; BossSays("phase", VoiceDirector.Important); return; }
            if (!bossStory && fraction <= 0.22f && !customer) { bossStory = true; bossMark = fraction; BossSays("story", VoiceDirector.Important); return; }
            if (!customer && bossMark - fraction >= 0.12f) { bossMark = fraction; if (BossSays("hurt", VoiceDirector.Normal)) return; }
            if (!customer && (boss.BossVolley != bossVolley || boss.BossSkillStarted != bossSkill))
            {
                bossVolley = boss.BossVolley; bossSkill = boss.BossSkillStarted;
                if (!SpeechBubbles.Showing(bossSpeaker)) BossSays("attack", VoiceDirector.Chatter);
            }
        }

        private bool BossSays(string trigger, int priority, float seconds = 0)
        {
            VoiceLine line = VoiceDirector.Say(bossSpeaker, trigger, priority, trigger == "defeat" || trigger == "enter" || trigger == "phase");
            if (line == null || string.IsNullOrEmpty(line.Text)) return false;
            ShowBoss(bossSpeaker, boss, line.Text, priority, seconds);
            BossLines++;
            return true;
        }

        /// <summary>A boss's words pinned above it (also used by the Last Customer's scripted beats).</summary>
        public void ShowBoss(string speaker, UnitState unit, string text, int priority, float seconds = 0)
        {
            string name = unit?.Definition == null ? "" : unit.Definition.Id == NightmareRules.CustomerId ? "THE LAST CUSTOMER" : unit.Definition.DisplayName;
            Vector2 last = bossLast;
            UnitState held = unit;
            Func<Vector3?> anchor = () =>
            {
                if (owner?.Session == null || owner.Session.State.Phase != SessionPhase.Combat) return null;
                if (held != null && held.Alive) last = (Vector2)SpeechBubbles.Head(held);
                return (Vector3)last;
            };
            SpeechBubbles.Show(speaker, name, text, anchor, SpeechTone.Boss, seconds, priority + 1);
        }

        // ----- timing -----

        private void Enqueue(float delay, float patience, bool companion, Func<bool> run)
        {
            if (queue.Count >= 6) queue.RemoveAt(0);
            float now = Time.unscaledTime;
            queue.Add(new Pending { At = now + delay, Until = now + patience, Run = run, Companion = companion });
        }

        private void Flush(bool blocked)
        {
            float now = Time.unscaledTime;
            for (int index = 0; index < queue.Count; index++)
            {
                Pending pending = queue[index];
                if (blocked) { pending.At = Mathf.Max(pending.At, now + 0.6f); pending.Until += Time.unscaledDeltaTime; continue; }
                if (now > pending.Until) { queue.RemoveAt(index--); continue; }
                if (now < pending.At) continue;
                if (pending.Companion && now - SpeechBubbles.LastCompanionAt < CompanionGap) continue;
                // a merge or arrival reveal has the stage: the new friend speaks once it has landed
                if (pending.Companion && owner.Shop?.Reveal != null && owner.Shop.Reveal.PendingCount > 0) continue;
                if (pending.Run()) { queue.RemoveAt(index); return; } // one new voice per frame
            }
        }

        /// <summary>Diagnostics: say a story companion's join line now (capture fixtures that load straight into a shop).</summary>
        public bool DiagnosticStory(int index, string trigger) => StorySays(index, trigger);
        /// <summary>Diagnostics: make the current boss bark a trigger now.</summary>
        public bool DiagnosticBoss(string trigger) { if (boss == null || bossSpeaker == null) return false; bossEntered = true; return BossSays(trigger, VoiceDirector.Story); }
    }
}
