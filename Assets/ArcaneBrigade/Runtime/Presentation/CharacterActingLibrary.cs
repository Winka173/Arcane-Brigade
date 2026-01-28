using System;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable]
    public sealed class CharacterActingEntry
    {
        public string Id;
        public string Alias;
        public float ReferenceHeight;
        public Sprite[] Curious;
        public Sprite[] Cheer;
        public Sprite[] Hurt;
        public Sprite[] Defeat;
        public Sprite Preview(int cue, float progress) => Frame(cue == 0 ? Curious : cue == 1 ? Cheer : cue == 2 ? Hurt : cue == 3 ? Defeat : null, progress);
        public Sprite Select(bool alive, float hurtAge, bool attacking, float celebrationAge, float curiosity, bool moving, bool reduced)
        {
            if (reduced) return null;
            if (!alive) return Frame(Defeat, 0);
            if (hurtAge >= 0 && hurtAge < .18f) return Frame(Hurt, hurtAge / .18f);
            if (attacking || moving) return null;
            if (celebrationAge >= 0 && celebrationAge < .7f) return Frame(Cheer, celebrationAge / .7f);
            if (curiosity >= 0 && curiosity < 1) return Frame(Curious, curiosity);
            return null;
        }
        public static Sprite Frame(Sprite[] frames, float progress) => frames == null || frames.Length == 0 ? null : frames[Mathf.Clamp(Mathf.FloorToInt(Mathf.Clamp01(progress) * frames.Length), 0, frames.Length - 1)];
    }
    public sealed class CharacterActingLibrary : ScriptableObject
    {
        public CharacterActingEntry[] Entries = Array.Empty<CharacterActingEntry>();
        private static CharacterActingLibrary cached;
        public static CharacterActingLibrary Current => cached != null ? cached : cached = Resources.Load<CharacterActingLibrary>("CharacterActingLibrary");
        public CharacterActingEntry Find(string id)
        {
            foreach (CharacterActingEntry entry in Entries) if (entry.Id == id || !string.IsNullOrEmpty(entry.Alias) && entry.Alias == id) return entry;
            return null;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() { cached = null; }
    }
}
