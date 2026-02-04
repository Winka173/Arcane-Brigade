using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ArcaneBrigade
{
    /// <summary>A companion's own handwritten line (B3) and their word after a defeat (B9). Story arcs use their authored
    /// banter for the current stretch of the journey; every other species speaks the last sentence of its biography
    /// in the first person ("They want a table…" becomes "I want a table…").</summary>
    public static class CompanionVoice
    {
        private static readonly Dictionary<string, string> cache = new Dictionary<string, string>();
        private static readonly (string pattern, string replacement)[] person =
        {
            (@"\bThey are\b", "I am"), (@"\bthey are\b", "I am"), (@"\bThey were\b", "I was"), (@"\bthey were\b", "I was"),
            (@"\bthemselves\b", "myself"), (@"\bThemselves\b", "Myself"), (@"\btheirs\b", "mine"),
            (@"\bTheir\b", "My"), (@"\btheir\b", "my"), (@"\bThem\b", "Me"), (@"\bthem\b", "me"), (@"\bThey\b", "I"), (@"\bthey\b", "I"),
        };

        // biographies whose "their/them" points at other people, so the pronoun swap would misread them
        private static readonly Dictionary<string, string> Fixed = new Dictionary<string, string>
        {
            { "RainbowTailor", "I preserve its patterns as evidence while making clothes with pockets people choose themselves." },
            { "SaffronMechanist", "I want to teach repairs that give residents control of their own doors." },
            { "SpindleWren", "I now teach travellers to hold and release their own ends, and seek a route that survives an honest goodbye." },
            { "MonsterMarble", "I want a station sign that begins: ask the traveller their name." },
        };

        public static string Line(string unitId, int wave, RunSnapshot state)
            => RunSession.Stranger(state, unitId) ? "Hello. I don't think we've met. I'm glad to be walking with you anyway." : Line(unitId, wave);

        public static string Line(string unitId, int wave)
        {
            if (string.IsNullOrEmpty(unitId)) return null;
            string banter = CreatureLives.Banter(unitId, wave);
            if (!string.IsNullOrEmpty(banter)) return banter;
            if (Fixed.TryGetValue(unitId, out string fixedLine)) return fixedLine;
            if (cache.TryGetValue(unitId, out string line)) return line;
            string origin = CreatureLives.Origin(unitId);
            line = null;
            if (!string.IsNullOrEmpty(origin))
            {
                string[] sentences = Regex.Split(origin.Trim(), @"(?<=[.!?])\s+");
                string last = sentences[sentences.Length - 1];
                foreach (var (pattern, replacement) in person) last = Regex.Replace(last, pattern, replacement);
                line = last;
            }
            cache[unitId] = line;
            return line;
        }

        private static readonly string[] Comfort =
        {
            "We'll walk you back. Same road, slower this time.",
            "You lit the way for us. Let us carry the lamp for a while.",
            "Nobody here is counting your mistakes. Sit, then we go again.",
            "The door is still there. So are we.",
            "You don't have to earn the next try. It's yours.",
            "Rest your hands. I'll keep watch until you're ready.",
            "We were afraid too. We stayed anyway.",
            "Next time, lean on us sooner. That's what we're here for.",
        };

        /// <summary>A short word from someone in the brigade after the light goes out. Deterministic per run.</summary>
        public static string Comforting(string runId, string unitId)
        {
            int hash = 17;
            foreach (char c in (runId ?? "") + (unitId ?? "")) hash = hash * 31 + c;
            return Comfort[(hash & 0x7fffffff) % Comfort.Length];
        }
    }
}
