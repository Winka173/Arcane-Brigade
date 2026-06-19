using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Comic words stamped over the moments that earn them (requirement 19/10/2026; STRIKE! is drawn by ImpactVisual):
    /// HOME RUN!, SPLAT!, PINNED!, OVERKILL!, ×N PIERCE, RICOCHET ×N, BONK!, CLEAVE ×N, DOMINO!, CRIT! and BREAK!. Each pops in
    /// big, settles, lifts a little and fades. Kept rare on purpose: at most three words a second and four on screen, a higher
    /// word pushing out a lower one; reduced effects keeps only the big ones. Painted punch type with an ink outline.</summary>
    public sealed class ImpactWords : MonoBehaviour
    {
        public const int MaxAlive = 4, PerSecond = 3;
        public const float Life = 0.95f;
        private sealed class Word { public string Text; public Vector2 At; public float Born, Scale, Angle; public Color Colour; public int Rank; }
        private readonly List<Word> words = new List<Word>(MaxAlive);
        private readonly List<InkedText> labels = new List<InkedText>(MaxAlive);
        private readonly List<float> born = new List<float>(8);
        private readonly Dictionary<int, int> cleave = new Dictionary<int, int>();
        private readonly Dictionary<int, Vector2> cleaveAt = new Dictionary<int, Vector2>();
        private readonly Dictionary<int, (int count, float at)> ricochets = new Dictionary<int, (int, float)>();
        private readonly HashSet<int> dominoes = new HashSet<int>();
        private int cleaveFrame = -1;
        public static ImpactWords Active { get; private set; }
        public int Shown { get; private set; }
        public static readonly Color Gold = new Color(1f, .86f, .32f), Cream = new Color(1f, .96f, .86f), Wine = new Color(.78f, .32f, .46f),
            Sky = new Color(.6f, .9f, 1f), Mint = new Color(.55f, 1f, .85f), Candy = new Color(1f, .74f, .86f), Ember = new Color(1f, .62f, .3f);
        private void Awake() { Active = this; }
        private void OnDestroy() { if (Active == this) Active = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { Active = null; }

        /// <summary>Stamps a word (rank 1 small, 2 notable, 3 big). False when the moment is crowded out.</summary>
        public static bool Pop(string text, Vector2 at, Color colour, int rank, float scale = 1, float angle = -6)
        {
            ImpactWords words = Active;
            return words != null && words.Stamp(text, at, colour, rank, scale, angle);
        }
        private bool Stamp(string text, Vector2 at, Color colour, int rank, float scale, float angle)
        {
            if (PresentationSettings.ReducedEffects && rank < 3) return false;
            float now = Time.time;
            born.RemoveAll(t => now - t > 1);
            Word weakest = null;
            foreach (Word w in words) if (weakest == null || w.Rank < weakest.Rank || w.Rank == weakest.Rank && w.Born < weakest.Born) weakest = w;
            bool full = born.Count >= PerSecond || words.Count >= MaxAlive;
            if (full)
            {
                if (weakest == null || weakest.Rank >= rank) return false;
                words.Remove(weakest);
            }
            // words never pile on one spot: a weaker word near a live one is dropped, a stronger one takes its place
            Word near = words.Find(w => (w.At - at).sqrMagnitude < 1.6f);
            if (near != null) { if (near.Rank >= rank) return false; words.Remove(near); }
            born.Add(now);
            words.Add(new Word { Text = text, At = at, Born = now, Scale = scale, Angle = angle, Colour = colour, Rank = rank });
            return true;
        }

        /// <summary>Every simulation step's events (RangedFeelVisual hands them over, so none is missed at low frame rates).</summary>
        public void Observe(CombatEvent value)
        {
            if (cleaveFrame != Time.frameCount) FlushCleaves();
            switch (value.Kind)
            {
                case CombatEventKind.WallSplat:
                {
                    bool pinned = value.Cause.ItemId != null && value.Cause.ItemId.EndsWith("pinned") || ImpactRules.Pointed(value.Weapon);
                    Pop(pinned ? "PINNED!" : "SPLAT!", value.Position + Vector2.up * .6f, pinned ? Cream : Wine, 1, .85f, pinned ? 0 : -10);
                    break;
                }
                case CombatEventKind.Kill when value.Faction == Faction.Enemy && value.Value >= 4 && value.Subject != null && !value.Subject.Definition.IsBoss:
                    Pop("OVERKILL!", value.Position + Vector2.up * .8f, Ember, 3, 1.05f, -8);
                    break;
                case CombatEventKind.ChainBlast:
                    Pop(value.Value >= 3 ? "CHAIN ×" + Mathf.RoundToInt(value.Value) : "CHAIN!", value.Position + Vector2.up * .8f, Ember, value.Value >= 3 ? 3 : 2, 1, -6);
                    break;
                case CombatEventKind.BodyImpact when value.Cause.ItemId == "pull-bump":
                    Pop("BONK!", value.Position + Vector2.up * .5f, Candy, 1, .8f, 8);
                    break;
                case CombatEventKind.BossPhase when value.Value >= 1 && value.Subject != null && value.Subject.Definition.IsBoss && value.Cause.ItemId != "boss-identity-encore":
                    Pop("BREAK!", value.Position + Vector2.up * .6f, Ember, 3, 1.1f, -4);
                    break;
                case CombatEventKind.ProjectileBounce when value.Faction != Faction.Enemy && value.Cause.RootId != 0:
                {
                    float now = Time.time;
                    var (count, at) = ricochets.TryGetValue(value.Cause.RootId, out var seen) && now - seen.at < 2 ? seen : (0, now);
                    count++;
                    if (ricochets.Count > 256) ricochets.Clear();
                    ricochets[value.Cause.RootId] = (count, now);
                    if (count == 4 || count == 6 || count == 8) Pop("RICOCHET ×" + count, value.Position + Vector2.up * .5f, Mint, 2, .8f, 4);
                    break;
                }
                case CombatEventKind.Hit when value.Faction == Faction.Enemy && value.Value > 0 && value.Weapon != null && value.Weapon.Kind == WeaponKind.Melee && value.Cause.RootId != 0:
                    // one swing's hits arrive in one step: counted here, stamped when the frame turns
                    cleave[value.Cause.RootId] = cleave.TryGetValue(value.Cause.RootId, out int hits) ? hits + 1 : 1;
                    cleaveAt[value.Cause.RootId] = value.Position;
                    break;
            }
        }
        /// <summary>×N PIERCE (RangedFeelVisual counts a shot's pierces): at 3, 5 and 7 enemies run through.</summary>
        public static void Pierced(int enemies, Vector2 at)
        {
            if (enemies == 3 || enemies == 5 || enemies == 7) Pop("×" + enemies + " PIERCE", at + Vector2.up * .5f, Sky, 2, .85f, -4);
        }
        private void FlushCleaves()
        {
            cleaveFrame = Time.frameCount;
            foreach (var pair in cleave) if (pair.Value >= 5) Pop("CLEAVE ×" + pair.Value, cleaveAt[pair.Key] + Vector2.up * .7f, Cream, 2, .9f, -10);
            cleave.Clear(); cleaveAt.Clear();
        }

        /// <summary>Per frame: DOMINO! for a body knocked flying by another, then draws the words.</summary>
        public void Display(CombatSimulation simulation)
        {
            bool live = simulation != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused);
            if (!live) { words.Clear(); dominoes.Clear(); }
            else
                foreach (FlungBody body in simulation.Flung)
                    if (body.Depth > 0 && dominoes.Add(body.UnitId)) Pop("DOMINO!", body.Position + Vector2.up * .7f, Gold, 2, .9f, 6);
            if (dominoes.Count > 512) dominoes.Clear();
        }

        private void LateUpdate()
        {
            if (cleaveFrame != Time.frameCount) FlushCleaves();
            float now = Time.time;
            words.RemoveAll(w => now - w.Born > Life);
            bool still = PresentationSettings.ReducedMotion;
            while (labels.Count < words.Count) labels.Add(InkedText.Create(transform, "ImpactWord", ArcadeTypography.Punch, 64, .045f, 2440, .035f));
            Shown = words.Count;
            for (int i = 0; i < labels.Count; i++)
            {
                InkedText label = labels[i];
                if (i >= words.Count) { if (label.gameObject.activeSelf) label.gameObject.SetActive(false); continue; }
                Word w = words[i];
                float t = (now - w.Born) / Life;
                label.gameObject.SetActive(true);
                label.Text = w.Text;
                float pop = still ? 1 : t < .1f ? Mathf.Lerp(1.55f, 1, t / .1f) : 1;
                Color c = w.Colour; c.a = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - .62f) / .38f)); label.Color = c;
                label.transform.position = new Vector3(w.At.x, w.At.y + (still ? 0 : t * .45f), -2f);
                label.transform.localScale = Vector3.one * pop * w.Scale * (w.Rank >= 3 ? 1.1f : w.Rank == 2 ? .95f : .8f);
                label.transform.rotation = Quaternion.Euler(0, 0, still ? 0 : w.Angle);
            }
        }
    }
}
