using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;

namespace SimplePlanes2TranslationMod
{
    internal sealed class TranslationCatalog
    {
        private sealed class ContextEntry
        {
            public string Key { get; set; }

            public string Value { get; set; }

            public string SceneName { get; set; }

            public string GameObjectPathContains { get; set; }

            public string ParentPathContains { get; set; }

            public string ObjectName { get; set; }

            public string ParentName { get; set; }

            public string ComponentType { get; set; }

            public int? SiblingIndex { get; set; }

            public string AnchoredPosition { get; set; }
        }

        private sealed class LookupCandidate
        {
            public string LookupText { get; set; }

            public string LeadingWhitespace { get; set; }

            public string TrailingWhitespace { get; set; }

            public bool ReplaceWithPlatformLineEndings { get; set; }
        }

        public static readonly TranslationCatalog Empty = new TranslationCatalog(
            new Dictionary<string, string>(StringComparer.Ordinal),
            new List<ContextEntry>());

        private readonly List<ContextEntry> _contextEntries;
        private readonly Dictionary<string, string> _entries;

        private TranslationCatalog(Dictionary<string, string> entries, List<ContextEntry> contextEntries)
        {
            _entries = entries;
            _contextEntries = contextEntries;
        }

        public int Count
        {
            get { return _entries.Count + _contextEntries.Count; }
        }

        public static TranslationCatalog Load(string path, ManualLogSource logger)
        {
            string json;
            JObject rootObject;
            JToken entriesToken;
            JObject entriesObject;
            Dictionary<string, string> entries;
            List<ContextEntry> contextEntries;

            if (!File.Exists(path))
            {
                logger.LogWarning(string.Format("Translation file not found: {0}", path));
                return Empty;
            }

            json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                logger.LogWarning(string.Format("Translation file is empty: {0}", path));
                return Empty;
            }

            rootObject = JObject.Parse(json);
            entriesToken = rootObject["entries"] ?? rootObject;
            entriesObject = entriesToken as JObject;
            if (entriesObject == null)
            {
                throw new InvalidDataException("Translation file must be a JSON object.");
            }

            entries = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (JProperty property in entriesObject.Properties())
            {
                string source;
                string translated;

                if (property.Value.Type != JTokenType.String)
                {
                    continue;
                }

                source = property.Name;
                translated = property.Value.Value<string>();
                if (string.IsNullOrEmpty(source) || translated == null)
                {
                    continue;
                }

                entries[source] = translated;
            }

            contextEntries = LoadContextEntries(rootObject["contextEntries"]);
            return new TranslationCatalog(entries, contextEntries);
        }

        public bool TryTranslate(string source, out string translated)
        {
            return TryTranslate(source, null, out translated);
        }

        public bool TryTranslate(string source, TextCaptureContext context, out string translated)
        {
            List<LookupCandidate> lookupCandidates;

            lookupCandidates = CreateLookupCandidates(source);
            if (TryTranslateFromContextEntries(lookupCandidates, context, out translated))
            {
                return true;
            }

            if (TryTranslateFromDictionary(lookupCandidates, out translated))
            {
                return true;
            }

            translated = source;
            return false;
        }

        private static List<ContextEntry> LoadContextEntries(JToken contextEntriesToken)
        {
            JArray contextEntriesArray;
            List<ContextEntry> contextEntries;
            int i;

            contextEntriesArray = contextEntriesToken as JArray;
            contextEntries = new List<ContextEntry>();
            if (contextEntriesArray == null)
            {
                return contextEntries;
            }

            for (i = 0; i < contextEntriesArray.Count; i++)
            {
                JObject contextEntryObject;
                string key;
                string value;

                contextEntryObject = contextEntriesArray[i] as JObject;
                if (contextEntryObject == null)
                {
                    continue;
                }

                key = ReadString(contextEntryObject, "key");
                value = ReadString(contextEntryObject, "value");
                if (string.IsNullOrEmpty(key) || value == null)
                {
                    continue;
                }

                contextEntries.Add(new ContextEntry
                {
                    Key = key,
                    Value = value,
                    SceneName = ReadString(contextEntryObject, "sceneName"),
                    GameObjectPathContains = ReadString(contextEntryObject, "gameObjectPathContains"),
                    ParentPathContains = ReadString(contextEntryObject, "parentPathContains"),
                    ObjectName = ReadString(contextEntryObject, "objectName"),
                    ParentName = ReadString(contextEntryObject, "parentName"),
                    ComponentType = ReadString(contextEntryObject, "componentType"),
                    SiblingIndex = ReadNullableInt(contextEntryObject["siblingIndex"]),
                    AnchoredPosition = ReadString(contextEntryObject, "anchoredPosition")
                });
            }

            return contextEntries;
        }

        private bool TryTranslateFromContextEntries(List<LookupCandidate> lookupCandidates, TextCaptureContext context, out string translated)
        {
            int i;
            int j;

            if (context == null || _contextEntries.Count == 0)
            {
                translated = null;
                return false;
            }

            for (i = 0; i < _contextEntries.Count; i++)
            {
                ContextEntry contextEntry;

                contextEntry = _contextEntries[i];
                if (!IsContextMatch(contextEntry, context))
                {
                    continue;
                }

                for (j = 0; j < lookupCandidates.Count; j++)
                {
                    LookupCandidate lookupCandidate;

                    lookupCandidate = lookupCandidates[j];
                    if (!string.Equals(contextEntry.Key, lookupCandidate.LookupText, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    translated = ApplyLookupCandidate(lookupCandidate, contextEntry.Value);
                    return true;
                }
            }

            translated = null;
            return false;
        }

        private bool TryTranslateFromDictionary(List<LookupCandidate> lookupCandidates, out string translated)
        {
            int i;

            for (i = 0; i < lookupCandidates.Count; i++)
            {
                LookupCandidate lookupCandidate;

                lookupCandidate = lookupCandidates[i];
                if (!_entries.TryGetValue(lookupCandidate.LookupText, out translated))
                {
                    continue;
                }

                translated = ApplyLookupCandidate(lookupCandidate, translated);
                return true;
            }

            translated = null;
            return false;
        }

        private static List<LookupCandidate> CreateLookupCandidates(string source)
        {
            List<LookupCandidate> candidates;
            string normalized;
            int leadingWhitespaceCount;
            int trailingWhitespaceCount;
            string trimmed;
            string leadingWhitespace;
            string trailingWhitespace;
            bool normalizedLineEndings;

            candidates = new List<LookupCandidate>();
            AddLookupCandidate(candidates, source, string.Empty, string.Empty, false);

            normalized = NormalizeLineEndings(source);
            normalizedLineEndings = !ReferenceEquals(normalized, source);
            if (normalizedLineEndings)
            {
                AddLookupCandidate(candidates, normalized, string.Empty, string.Empty, true);
            }

            leadingWhitespaceCount = 0;
            while (leadingWhitespaceCount < normalized.Length && char.IsWhiteSpace(normalized[leadingWhitespaceCount]))
            {
                leadingWhitespaceCount++;
            }

            trailingWhitespaceCount = 0;
            while (trailingWhitespaceCount < normalized.Length - leadingWhitespaceCount &&
                   char.IsWhiteSpace(normalized[normalized.Length - trailingWhitespaceCount - 1]))
            {
                trailingWhitespaceCount++;
            }

            if (leadingWhitespaceCount == 0 && trailingWhitespaceCount == 0)
            {
                return candidates;
            }

            trimmed = normalized.Substring(leadingWhitespaceCount, normalized.Length - leadingWhitespaceCount - trailingWhitespaceCount);
            leadingWhitespace = normalized.Substring(0, leadingWhitespaceCount);
            trailingWhitespace = normalized.Substring(normalized.Length - trailingWhitespaceCount, trailingWhitespaceCount);
            AddLookupCandidate(candidates, trimmed, leadingWhitespace, trailingWhitespace, normalizedLineEndings);

            return candidates;
        }

        private static void AddLookupCandidate(
            List<LookupCandidate> candidates,
            string lookupText,
            string leadingWhitespace,
            string trailingWhitespace,
            bool replaceWithPlatformLineEndings)
        {
            int i;

            for (i = 0; i < candidates.Count; i++)
            {
                LookupCandidate existingCandidate;

                existingCandidate = candidates[i];
                if (!string.Equals(existingCandidate.LookupText, lookupText, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.Equals(existingCandidate.LeadingWhitespace, leadingWhitespace, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!string.Equals(existingCandidate.TrailingWhitespace, trailingWhitespace, StringComparison.Ordinal))
                {
                    continue;
                }

                if (existingCandidate.ReplaceWithPlatformLineEndings == replaceWithPlatformLineEndings)
                {
                    return;
                }
            }

            candidates.Add(new LookupCandidate
            {
                LookupText = lookupText,
                LeadingWhitespace = leadingWhitespace,
                TrailingWhitespace = trailingWhitespace,
                ReplaceWithPlatformLineEndings = replaceWithPlatformLineEndings
            });
        }

        private static string ApplyLookupCandidate(LookupCandidate lookupCandidate, string translated)
        {
            string value;

            value = translated ?? string.Empty;
            if (lookupCandidate.ReplaceWithPlatformLineEndings)
            {
                value = value.Replace("\n", Environment.NewLine);
            }

            if (string.IsNullOrEmpty(lookupCandidate.LeadingWhitespace) &&
                string.IsNullOrEmpty(lookupCandidate.TrailingWhitespace))
            {
                return value;
            }

            return lookupCandidate.LeadingWhitespace + value + lookupCandidate.TrailingWhitespace;
        }

        private static bool IsContextMatch(ContextEntry contextEntry, TextCaptureContext context)
        {
            if (!IsOptionalStringEqual(contextEntry.SceneName, context.SceneName))
            {
                return false;
            }

            if (!ContainsOrdinal(context.GameObjectPath, contextEntry.GameObjectPathContains))
            {
                return false;
            }

            if (!ContainsOrdinal(context.ParentPath, contextEntry.ParentPathContains))
            {
                return false;
            }

            if (!IsOptionalStringEqual(contextEntry.ObjectName, context.ObjectName))
            {
                return false;
            }

            if (!IsOptionalStringEqual(contextEntry.ParentName, context.ParentName))
            {
                return false;
            }

            if (!IsOptionalStringEqual(contextEntry.ComponentType, context.ComponentType))
            {
                return false;
            }

            if (contextEntry.SiblingIndex.HasValue && contextEntry.SiblingIndex != context.SiblingIndex)
            {
                return false;
            }

            if (!IsOptionalStringEqual(contextEntry.AnchoredPosition, context.AnchoredPosition))
            {
                return false;
            }

            return true;
        }

        private static bool ContainsOrdinal(string value, string expectedSubstring)
        {
            if (string.IsNullOrEmpty(expectedSubstring))
            {
                return true;
            }

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.IndexOf(expectedSubstring, StringComparison.Ordinal) >= 0;
        }

        private static bool IsOptionalStringEqual(string expected, string actual)
        {
            if (string.IsNullOrEmpty(expected))
            {
                return true;
            }

            return string.Equals(expected, actual, StringComparison.Ordinal);
        }

        private static string ReadString(JObject sourceObject, string propertyName)
        {
            JToken valueToken;

            valueToken = sourceObject[propertyName];
            if (valueToken == null || valueToken.Type == JTokenType.Null)
            {
                return string.Empty;
            }

            if (valueToken.Type == JTokenType.String)
            {
                return valueToken.Value<string>();
            }

            return valueToken.ToString();
        }

        private static int? ReadNullableInt(JToken valueToken)
        {
            int value;

            if (valueToken == null || valueToken.Type == JTokenType.Null)
            {
                return null;
            }

            if (valueToken.Type == JTokenType.Integer)
            {
                return valueToken.Value<int>();
            }

            if (int.TryParse(valueToken.ToString(), out value))
            {
                return value;
            }

            return null;
        }

        private static string NormalizeLineEndings(string source)
        {
            if (source.IndexOf("\r\n", StringComparison.Ordinal) < 0)
            {
                return source;
            }

            return source.Replace("\r\n", "\n");
        }
    }
}
