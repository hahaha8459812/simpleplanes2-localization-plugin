using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json;

namespace SimplePlanes2TranslationMod
{
    internal sealed class TranslationSettings
    {
        public string Mode { get; set; }

        public string Language { get; set; }

        public bool EnableSceneScan { get; set; }

        public bool LogMissingTexts { get; set; }

        public bool CaptureStandaloneTmpTexts { get; set; }

        public string CapturedTextsFileName { get; set; }

        public float CaptureFlushIntervalSeconds { get; set; }

        public float SceneScanIntervalSeconds { get; set; }

        public float IdleSceneScanIntervalSeconds { get; set; }

        public float InteractiveSceneScanDurationSeconds { get; set; }

        public bool ApplyBoldStyleToTranslatedText { get; set; }

        public bool VerboseLogging { get; set; }

        public static TranslationSettings CreateDefault()
        {
            return new TranslationSettings
            {
                Mode = "collect",
                Language = "zh-CN",
                EnableSceneScan = true,
                LogMissingTexts = true,
                CaptureStandaloneTmpTexts = true,
                CapturedTextsFileName = "captured-texts.json",
                CaptureFlushIntervalSeconds = 10.0f,
                SceneScanIntervalSeconds = 0.1f,
                IdleSceneScanIntervalSeconds = 1.0f,
                InteractiveSceneScanDurationSeconds = 1.0f,
                ApplyBoldStyleToTranslatedText = false,
                VerboseLogging = false
            };
        }

        public static TranslationSettings LoadOrCreate(string path, ManualLogSource logger)
        {
            string json;
            TranslationSettings defaultSettings;
            TranslationSettings settings;
            string directoryPath;

            if (!File.Exists(path))
            {
                defaultSettings = CreateDefault();
                json = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
                directoryPath = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.WriteAllText(path, json, System.Text.Encoding.UTF8);
                logger.LogInfo(string.Format("Created default settings file: {0}", path));
                return defaultSettings;
            }

            json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            settings = JsonConvert.DeserializeObject<TranslationSettings>(json);
            return settings ?? CreateDefault();
        }

        public bool IsCollectMode()
        {
            return string.Equals(Mode, "collect", System.StringComparison.OrdinalIgnoreCase);
        }

        public bool IsTranslateMode()
        {
            return string.Equals(Mode, "translate", System.StringComparison.OrdinalIgnoreCase);
        }

        public bool IsHybridMode()
        {
            return string.Equals(Mode, "hybrid", System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
