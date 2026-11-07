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
        private sealed class Node
        {
            public readonly Dictionary<char, Node> Children = new Dictionary<char, Node>();
            public string Translation;
        }
        private static Node root;
        private static readonly Dictionary<string, string> exact = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> cache = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly HashSet<string> translated = new HashSet<string>(StringComparer.Ordinal);
        private static readonly System.Text.RegularExpressions.Regex markup = new System.Text.RegularExpressions.Regex(@"\{(?:mark|stat:[^{}]*|bonus:[^{}]*)\}");
        public static string NormalizeLanguage(string language) => language == Vietnamese ? Vietnamese : English;

        public static void SetLanguage(string language)
        {
            language = NormalizeLanguage(language);
            if (language == Language) return;
            Language = language; Revision++; cache.Clear(); translated.Clear();
        }

        private static void Load()
        {
            if (root != null) return;
            root = new Node();
            TextAsset asset = Resources.Load<TextAsset>("Localization/vi");
            if (asset == null) { Debug.LogError("Missing Vietnamese translation table"); return; }
            Table table = JsonUtility.FromJson<Table>(asset.text);
            foreach (Entry entry in table.entries)
            {
                string source = entry.source, target = entry.target;
                if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target)) continue;
                exact[source] = target;
                // Longest authored fragment wins for sentences assembled from numbers, names and UI copy.
                Node node = root;
                foreach (char c in source)
                {
                    if (!node.Children.TryGetValue(c, out Node child)) node.Children[c] = child = new Node();
                    node = child;
                }
                node.Translation = target;
            }
        }

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
                bool identifier = source.IndexOfAny(new[] { ' ', '\n', '\r', '\t', '<' }) < 0 && source.IndexOfAny(new[] { '-', '/', '\\' }) >= 0;
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
                    Node node = root; string match = null; int next = start;
                    if (start == 0 || !char.IsLetterOrDigit(source[start - 1]))
                        for (int i = start; i < source.Length && node.Children.TryGetValue(source[i], out node); i++)
                            if (node.Translation != null && (i + 1 == source.Length || !char.IsLetterOrDigit(source[i + 1])))
                            { match = node.Translation; next = i + 1; }
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
