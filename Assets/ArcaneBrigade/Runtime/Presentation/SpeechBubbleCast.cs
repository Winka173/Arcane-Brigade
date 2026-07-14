using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Who says what, and when, after committed purchases and observed state changes (the roster, the travelling four,
    /// the boss, the warm chair) and never changes it: a companion who joins or ranks up says a short line above its head,
    /// the story bosses bark on entry, attacks, wounds, the half-way turn and their fall, and the shopkeeper speaks when the
    /// shop is quiet or when someone wanders too close to the warm chair. Chatter keeps polite gaps (see Voice_System.md).</summary>
    public sealed class SpeechBubbleCast : MonoBehaviour
    {
        private sealed class Pending { public float At, Until; public Func<bool> Run; public bool Companion, Context, Chorus; }
        private const float CompanionGap = 2.6f;
        private ArenaController owner;
        private string runId;
        private bool baseline;
        private int observedMerges;
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
        private int reactedReceipt = -1;
        private string reactedRun;
        private readonly HashSet<string> contextSaid = new HashSet<string>();
        public int Joins { get; private set; }
        public int StarUps { get; private set; }
        public int ContextLines { get; private set; }
        public CompanionReactionKind LastContext { get; private set; }
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
                runId = state.RunId; baseline = false; queue.Clear(); contextSaid.Clear(); boss = null; chairConfessed = false;
                shopWave = state.Phase == SessionPhase.Shop ? state.Wave : -1; hobRescued = state.ActOne?.HobRescued ?? 0;
            }
            bool blocked = owner.PresentationPaused || owner.StoryOpen || owner.TransitionPending || owner.Session.EndingPending || owner.Session.SecretRoomOpen || MergeReveal.Busy;
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
            foreach (RosterEntry entry in CompanionDeployment.Select(state.Roster)) { string key = entry.UnitId + "#" + entry.Tier; scratch[key] = (scratch.TryGetValue(key, out int count) ? count : 0) + 1; }
            if (baseline)
            {
                foreach (KeyValuePair<string, int> pair in scratch)
                {
                    int before = roster.TryGetValue(pair.Key, out int count) ? count : 0;
                    if (pair.Value <= before) continue;
                    int split = pair.Key.LastIndexOf('#');
                    string id = pair.Key.Substring(0, split); int tier = int.Parse(pair.Key.Substring(split + 1));
                    // A directly bought high-star body joins; a real merge consumed lower-star ingredients.
                    string lower = id + "#" + (tier - 1);
                    int remaining = scratch.TryGetValue(lower, out int bodies) ? bodies : 0;
                    bool merged = tier >= 2 && state.KitMerges > observedMerges && roster.TryGetValue(lower, out int ingredients)
                        && ingredients - remaining >= CompanionDeployment.MergeCopies - 1;
                    if (merged || tier >= 5) StarUp(id, tier); else Join(id); // a Starlight Thread weaves a friend without any merge
                }
            }
            roster.Clear(); foreach (KeyValuePair<string, int> pair in scratch) roster[pair.Key] = pair.Value;
            baseline = true;
            observedMerges = state.KitMerges;
        }

        private static readonly string[] JoinFallback = { "I'll walk with you.", "Room for one more?", "Hello. I'm coming too.", "Same road. Better company.", "Lead on. I'm right behind you.", "I was hoping someone would ask." };
        private static readonly string[] SecondStar = { "Both of us. One stubborn heart.", "Bigger shoes. Same feet.", "I remember all of me.", "We held on. Now we hold more.", "Stronger. Still walking with you.", "Two little histories. One of me." };
        private static readonly string[] ThirdStar = { "Everything I was, all at once.", "Brightest I have ever been.", "Whole. Finally whole.", "Nothing in the dark is this bright.", "Lean on me. I can take it." };
        private static readonly string[] FourthStar = { "Past three, there is more.", "Violet, and so bright.", "I can see all of the road now.", "Look how far we came.", "Stars on stars. I am humming." };
        private static readonly string[] FifthStar = { "Every colour. All of me.", "One of a kind, they said.", "I was never small.", "The whole night is in me.", "Remember this. Remember me." };
        private static readonly string[] Cheer3 = { "Look at them go!", "That is our friend.", "Well earned.", "Shining, that one." };
        private static readonly string[] Cheer4 = { "Four stars! Did you see that light?", "They glow like a lantern now.", "So proud of that one.", "Purple, like a night sky." };
        private static readonly string[] Cheer5 = { "ONE OF A KIND!", "I will remember this forever.", "Every colour at once!", "We are so lucky.", "Never seen anything like it.", "A legend walks with us!" };
        private static int Hash(string value) { int hash = 17; foreach (char c in value ?? "") hash = hash * 31 + c; return hash & 0x7fffffff; }

        public static string JoinLine(string unitId, RunSnapshot state)
        {
            // their own authored first line (or story-arc line) reads best as a hello; biographies are the fallback
            string line = RunSession.Stranger(state, unitId) || !CompanionBanter.Has(unitId) ? CompanionVoice.Line(unitId, state.Wave, state) : CompanionBanter.Line(unitId, 0, state.Wave, state);
            return !string.IsNullOrEmpty(line) && line.Length <= 90 ? line : JoinFallback[Hash(unitId + state.Wave) % JoinFallback.Length];
        }
        public static string StarLine(string unitId, int tier, int wave)
        {
            string personal = CompanionReactions.Line(unitId, CompanionReactionKind.Merge);
            if (personal != null) return personal;
            string[] lines = tier >= 5 ? FifthStar : tier == 4 ? FourthStar : tier >= 3 ? ThirdStar : SecondStar;
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
            string name = (RunSession.Stranger(owner.Session.State, id) ? "???" : unit.DisplayName) + "  " + new string('★', Mathf.Clamp(tier, 2, 5));
            string text = StarLine(id, tier, owner.Session.State.Wave);
            // the merge reveal plays first; the new star speaks once it has landed
            Enqueue(1.1f, 16f, true, () => CompanionSays(id, tier, name, text, "starup", VoiceDirector.Normal, SpeechTone.Companion), false, tier >= 3);
            // once the animation ends the brigade reacts: how many friends cheer, and what they say, grows with the star
            // (three stars one friend, four two, five all of them at once, up to the bubble limit)
            int others = tier >= 5 ? SpeechBubbles.MaxBubbles - 1 : tier == 4 ? 2 : tier == 3 ? 1 : 0;
            string[] pool = tier >= 5 ? Cheer5 : tier == 4 ? Cheer4 : Cheer3;
            int spoken = 0;
            foreach (RosterEntry e in owner.Session.ActiveRoster)
            {
                if (spoken >= others) break;
                if (e.UnitId == id) continue;
                UnitDefinition friend = owner.Content.Unit(e.UnitId);
                if (friend == null) continue;
                string friendId = e.UnitId, friendName = (RunSession.Stranger(owner.Session.State, friendId) ? "???" : friend.DisplayName) + "  " + new string('★', Mathf.Clamp(e.Tier, 1, 5));
                string line = pool[(Hash(friendId + tier) + spoken) % pool.Length]; int friendTier = e.Tier;
                Enqueue(1.1f + spoken * (tier >= 5 ? .04f : .3f), 16f, true, () => CompanionSays(friendId, friendTier, friendName, line, "cheer", VoiceDirector.Normal, SpeechTone.Companion), false, true);
                spoken++;
            }
        }

        private bool CompanionSays(string id, int tier, string name, string text, string trigger, int priority, SpeechTone tone)
        {
            Func<Vector3?> anchor = () => { UnitState unit = FindCompanion(id, tier); return unit == null ? (Vector3?)null : SpeechBubbles.Head(unit); };
            if (anchor() == null) return false;
            // Personal merge/context copy has no new recording: do not play an unrelated generic spoken sentence over it.
            if (trigger == "join" || !CompanionReactions.Has(id)) VoiceDirector.Say(VoiceLines.Companion, trigger, priority, true);
            SpeechBubbles.Show("Companion:" + id, name, text, anchor, tone, 0, priority + 1);
            if (trigger == "join") Joins++; else if (trigger == "starup") StarUps++; else ContextLines++;
            return true;
        }

        public void CommittedPurchase(RunSnapshot before, ShopOffer offer)
        {
            if (owner?.Session == null) return;
            var state = owner.Session.State;
            if (reactedRun == state.RunId && state.NextReceipt <= reactedReceipt) return;
            if (state.NextReceipt <= before.NextReceipt) return;
            reactedRun = state.RunId; reactedReceipt = state.NextReceipt;
            foreach (var reaction in CompanionReactions.AfterPurchase(owner.Content, before, owner.Session, offer))
            {
                var held = reaction; int wave = state.Wave; string run = state.RunId;
                string once = wave + ":" + held.UnitId + ":" + held.Kind;
                if (!contextSaid.Add(once)) continue;
                // The merchant and the actual recruit/merge reveal speak first. A sale has one additional response at most.
                Enqueue(3.5f, 14f, true, () =>
                {
                    if (owner.Session.State.Phase != SessionPhase.Shop || owner.Session.State.Wave != wave || owner.Session.State.RunId != run) return true;
                    bool said;
                    if (!string.IsNullOrEmpty(held.StoryName))
                    {
                        var actor = FindStory(held.StoryName);
                        if (actor == null) return false;
                        SpeechBubbles.Show("Story:" + held.StoryName, held.StoryName, held.Text, () =>
                        {
                            var current = FindStory(held.StoryName); return current == null ? (Vector3?)null : SpeechBubbles.Head(current);
                        }, SpeechTone.Story, 0, VoiceDirector.Chatter + 1);
                        ContextLines++; said = true;
                    }
                    else said = CompanionSays(held.UnitId, held.Tier, owner.Content.Unit(held.UnitId).DisplayName, held.Text, held.Kind == CompanionReactionKind.Bond ? "bond" : "purchase", VoiceDirector.Chatter, SpeechTone.Companion);
                    if (said) LastContext = held.Kind;
                    return said;
                }, context: true);
            }
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

        private void Enqueue(float delay, float patience, bool companion, Func<bool> run, bool context = false, bool chorus = false)
        {
            if (queue.Count >= 8) queue.RemoveAt(0);
            float now = Time.unscaledTime;
            queue.Add(new Pending { At = now + delay, Until = now + patience, Run = run, Companion = companion, Context = context, Chorus = chorus });
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
                // a star-up chorus speaks together, once the animation is over: no gaps, no waiting for the shopkeeper, all in one frame
                if (pending.Chorus) { if (!(owner.Shop?.Reveal != null && owner.Shop.Reveal.PendingCount > 0)) { queue.RemoveAt(index--); pending.Run(); } continue; }
                if (pending.Companion && now - SpeechBubbles.LastCompanionAt < CompanionGap) continue;
                if (pending.Companion && owner.Shop?.Dialogue.ExpiresAt > owner.PresentationClock) continue;
                if (pending.Context && (SpeechBubbles.CountTone(SpeechTone.Companion) > 0 || SpeechBubbles.CountTone(SpeechTone.Story) > 0)) continue;
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
