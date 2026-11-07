using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Offline display translations. English source strings and gameplay identifiers stay unchanged.</summary>
    public static class GameLocalization
    {
        public const string English = "en", Vietnamese = "vi";
        public static string Language { get; private set; } = English;
        public static bool IsVietnamese => Language == Vietnamese;
        public static int Revision { get; private set; }
        [Serializable] public sealed class Entry { public string source; public string target; }
        [Serializable] public sealed class Table { public Entry[] entries; }
        private static bool loaded;
        // Four-character prefix buckets retain longest-fragment matching without a dictionary object for every character.
        private static readonly Dictionary<ulong, List<Entry>> fragments = new Dictionary<ulong, List<Entry>>();
        private static readonly Dictionary<string, string> exact = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> cache = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly HashSet<string> translated = new HashSet<string>(StringComparer.Ordinal);
        private static readonly char[] identifierBreaks = { ' ', '\n', '\r', '\t', '<' }, identifierMarks = { '-', '/', '\\' };
        private static readonly System.Text.RegularExpressions.Regex markup = new System.Text.RegularExpressions.Regex(@"\{(?:mark|gives|costs|stat:[^{}]*|bonus:[^{}]*|tag:[^{}]*)\}");
        public static string NormalizeLanguage(string language) => language == Vietnamese ? Vietnamese : English;

        public static void SetLanguage(string language)
        {
            language = NormalizeLanguage(language);
            if (language == Language) return;
            Language = language; Revision++; cache.Clear(); translated.Clear();
        }

        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            TextAsset asset = Resources.Load<TextAsset>("Localization/vi");
            if (asset == null) { Debug.LogError("Missing Vietnamese translation table"); return; }
            Table table = JsonUtility.FromJson<Table>(asset.text);
            foreach (Entry entry in table.entries)
            {
                string source = entry.source, target = entry.target;
                if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) continue;
                exact[source] = target;
                ulong key = Prefix(source, 0);
                if (!fragments.TryGetValue(key, out var bucket)) fragments[key] = bucket = new List<Entry>();
                bucket.Add(entry);
            }
            foreach (var bucket in fragments.Values) bucket.Sort((a, b) => b.source.Length.CompareTo(a.source.Length));
        }
        private static ulong Prefix(string text, int start)
        {
            ulong key = text[start];
            if (!char.IsLetterOrDigit(text[start])) return key;
            for (int i = 1; i < 4 && start + i < text.Length && char.IsLetterOrDigit(text[start + i]); i++) key |= (ulong)text[start + i] << (16 * i);
            return key;
        }
        public static void Warmup() { if (IsVietnamese) Load(); }

        public static string Text(string source)
        {
            if (!IsVietnamese || string.IsNullOrEmpty(source)) return source;
            if (cache.TryGetValue(source, out string result)) return result;
            // A measured GUIContent can subsequently pass through a label: never translate it twice.
            if (translated.Contains(source)) return source;
            Load();
            var tokens = markup.Matches(source);
            if (tokens.Count > 0)
            {
                // The glossary parses these tokens AFTER translation. Stat IDs must stay in English;
                // the glossary translates the visible bonus caption separately when drawing it.
                var output = new StringBuilder(source.Length);
                int position = 0;
                foreach (System.Text.RegularExpressions.Match token in tokens)
                {
                    output.Append(Text(source.Substring(position, token.Index - position)));
                    output.Append(token.Value); position = token.Index + token.Length;
                }
                output.Append(Text(source.Substring(position)));
                result = output.ToString();
            }
            else if (!exact.TryGetValue(source, out result))
            {
                // Unknown IDs and resource paths are indivisible, even if they contain an English word.
                bool identifier = source.IndexOfAny(identifierBreaks) < 0 && source.IndexOfAny(identifierMarks) >= 0;
                if (identifier) return source;
                var output = new StringBuilder(source.Length);
                for (int start = 0; start < source.Length;)
                {
                    // Rich-text markup belongs to Unity, including colours, sizes and numeric attributes.
                    if (source[start] == '<')
                    {
                        int end = source.IndexOf('>', start);
                        if (end >= 0) { output.Append(source, start, end - start + 1); start = end + 1; continue; }
                    }
                    string match = null; int next = start;
                    if ((start == 0 || !char.IsLetterOrDigit(source[start - 1])) && fragments.TryGetValue(Prefix(source, start), out var bucket))
                        foreach (Entry entry in bucket)
                        {
                            int end = start + entry.source.Length;
                            if (end <= source.Length && (end == source.Length || !char.IsLetterOrDigit(source[end])) && string.CompareOrdinal(source, start, entry.source, 0, entry.source.Length) == 0)
                            { match = entry.target; next = end; break; }
                        }
                    if (match != null) { output.Append(match); start = next; }
                    else output.Append(source[start++]);
                }
                result = output.ToString();
            }
            result = result.Normalize(NormalizationForm.FormC);
            // Dynamic counters must not grow memory throughout an endless run.
            if (cache.Count >= 4096) { cache.Clear(); translated.Clear(); }
            cache[source] = result; translated.Add(result);
            return result;
        }
    }
}
