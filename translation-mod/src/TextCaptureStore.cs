using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace SimplePlanes2TranslationMod
{
    internal sealed class TextCaptureContext
    {
        public string SceneName { get; set; }

        public string Source { get; set; }

        public string ObjectName { get; set; }

        public string ParentName { get; set; }

        public string GameObjectPath { get; set; }

        public string ParentPath { get; set; }

        public string ComponentType { get; set; }

        public int? SiblingIndex { get; set; }

        public string AnchoredPosition { get; set; }
    }

    internal sealed class TextCaptureStore
    {
        private sealed class TextCaptureEntry
        {
            public string Text { get; set; }

            public int Count { get; set; }

            public string FirstSeenUtc { get; set; }

            public string LastSeenUtc { get; set; }

            public List<string> Scenes { get; set; }

            public List<string> Sources { get; set; }

            public List<TextCaptureContext> Contexts { get; set; }
        }

        private readonly object _syncLock = new object();
        private readonly Dictionary<string, TextCaptureEntry> _entries = new Dictionary<string, TextCaptureEntry>(StringComparer.Ordinal);
        private readonly string _outputPath;

        public TextCaptureStore(string outputPath)
        {
            _outputPath = outputPath;
        }

        public void Record(string text, string source, string sceneName, TextCaptureContext context)
        {
            TextCaptureEntry entry;
            string utcNow;

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            utcNow = DateTime.UtcNow.ToString("o");

            lock (_syncLock)
            {
                if (!_entries.TryGetValue(text, out entry))
                {
                    entry = new TextCaptureEntry
                    {
                        Text = text,
                        Count = 0,
                        FirstSeenUtc = utcNow,
                        LastSeenUtc = utcNow,
                        Scenes = new List<string>(),
                        Sources = new List<string>(),
                        Contexts = new List<TextCaptureContext>()
                    };

                    _entries.Add(text, entry);
                }

                entry.Count++;
                entry.LastSeenUtc = utcNow;

                if (!string.IsNullOrEmpty(sceneName) && !entry.Scenes.Contains(sceneName))
                {
                    entry.Scenes.Add(sceneName);
                }

                if (!string.IsNullOrEmpty(source) && !entry.Sources.Contains(source))
                {
                    entry.Sources.Add(source);
                }

                AddContextIfMissing(entry.Contexts, context);
            }
        }

        public void Flush(ManualLogSource logger)
        {
            List<TextCaptureEntry> snapshot;
            string directoryPath;

            lock (_syncLock)
            {
                snapshot = new List<TextCaptureEntry>(_entries.Values);
            }

            snapshot.Sort(delegate(TextCaptureEntry left, TextCaptureEntry right)
            {
                return string.Compare(left.Text, right.Text, StringComparison.Ordinal);
            });

            directoryPath = Path.GetDirectoryName(_outputPath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(_outputPath, JsonConvert.SerializeObject(snapshot, Formatting.Indented), System.Text.Encoding.UTF8);

            if (logger != null)
            {
                logger.LogInfo(string.Format("Captured {0} unique text entries to '{1}'.", snapshot.Count, _outputPath));
            }
        }

        private static void AddContextIfMissing(List<TextCaptureContext> contexts, TextCaptureContext context)
        {
            int i;

            if (contexts == null || context == null)
            {
                return;
            }

            for (i = 0; i < contexts.Count; i++)
            {
                if (AreSameContext(contexts[i], context))
                {
                    return;
                }
            }

            contexts.Add(CloneContext(context));
        }

        private static TextCaptureContext CloneContext(TextCaptureContext context)
        {
            return new TextCaptureContext
            {
                SceneName = context.SceneName,
                Source = context.Source,
                ObjectName = context.ObjectName,
                ParentName = context.ParentName,
                GameObjectPath = context.GameObjectPath,
                ParentPath = context.ParentPath,
                ComponentType = context.ComponentType,
                SiblingIndex = context.SiblingIndex,
                AnchoredPosition = context.AnchoredPosition
            };
        }

        private static bool AreSameContext(TextCaptureContext left, TextCaptureContext right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            return string.Equals(left.SceneName, right.SceneName, StringComparison.Ordinal) &&
                   string.Equals(left.Source, right.Source, StringComparison.Ordinal) &&
                   string.Equals(left.ObjectName, right.ObjectName, StringComparison.Ordinal) &&
                   string.Equals(left.ParentName, right.ParentName, StringComparison.Ordinal) &&
                   string.Equals(left.GameObjectPath, right.GameObjectPath, StringComparison.Ordinal) &&
                   string.Equals(left.ParentPath, right.ParentPath, StringComparison.Ordinal) &&
                   string.Equals(left.ComponentType, right.ComponentType, StringComparison.Ordinal) &&
                   left.SiblingIndex == right.SiblingIndex &&
                   string.Equals(left.AnchoredPosition, right.AnchoredPosition, StringComparison.Ordinal);
        }
    }
}
