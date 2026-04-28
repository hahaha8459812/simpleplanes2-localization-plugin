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

        public bool EnableHotkeys { get; set; }

        public string ManualReloadHotkeyName { get; set; }

        public string ToggleTranslationHotkeyName { get; set; }

        public bool ShowFloatingPanel { get; set; }

        public bool FloatingPanelExpanded { get; set; }

        public bool FloatingPanelSettingsExpanded { get; set; }

        public float FloatingPanelX { get; set; }

        public float FloatingPanelY { get; set; }

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
                VerboseLogging = false,
                EnableHotkeys = true,
                ManualReloadHotkeyName = "F2",
                ToggleTranslationHotkeyName = "F1",
                ShowFloatingPanel = true,
                FloatingPanelExpanded = false,
                FloatingPanelSettingsExpanded = false,
                FloatingPanelX = 18.0f,
                FloatingPanelY = 120.0f
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
            settings = settings ?? CreateDefault();
            ApplyMissingBooleanDefaults(settings, json);
            ApplyMissingFloatDefaults(settings, json);
            settings.Normalize();
            return settings;
        }

        public void Save(string path)
        {
            string directoryPath;
            string json;

            Normalize();
            directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }

        public void Normalize()
        {
            TranslationSettings defaultSettings = CreateDefault();

            if (string.IsNullOrWhiteSpace(Mode))
            {
                Mode = defaultSettings.Mode;
            }

            if (string.IsNullOrWhiteSpace(Language))
            {
                Language = defaultSettings.Language;
            }

            if (string.IsNullOrWhiteSpace(CapturedTextsFileName))
            {
                CapturedTextsFileName = defaultSettings.CapturedTextsFileName;
            }

            if (CaptureFlushIntervalSeconds <= 0.0f)
            {
                CaptureFlushIntervalSeconds = defaultSettings.CaptureFlushIntervalSeconds;
            }

            if (SceneScanIntervalSeconds <= 0.0f)
            {
                SceneScanIntervalSeconds = defaultSettings.SceneScanIntervalSeconds;
            }

            if (IdleSceneScanIntervalSeconds <= 0.0f)
            {
                IdleSceneScanIntervalSeconds = defaultSettings.IdleSceneScanIntervalSeconds;
            }

            if (InteractiveSceneScanDurationSeconds <= 0.0f)
            {
                InteractiveSceneScanDurationSeconds = defaultSettings.InteractiveSceneScanDurationSeconds;
            }

            if (string.IsNullOrWhiteSpace(ManualReloadHotkeyName))
            {
                ManualReloadHotkeyName = defaultSettings.ManualReloadHotkeyName;
            }

            if (string.IsNullOrWhiteSpace(ToggleTranslationHotkeyName))
            {
                ToggleTranslationHotkeyName = defaultSettings.ToggleTranslationHotkeyName;
            }

            if (float.IsNaN(FloatingPanelX) || float.IsInfinity(FloatingPanelX) || FloatingPanelX < 0.0f)
            {
                FloatingPanelX = defaultSettings.FloatingPanelX;
            }

            if (float.IsNaN(FloatingPanelY) || float.IsInfinity(FloatingPanelY) || FloatingPanelY < 0.0f)
            {
                FloatingPanelY = defaultSettings.FloatingPanelY;
            }
        }

        private static void ApplyMissingBooleanDefaults(TranslationSettings settings, string json)
        {
            TranslationSettings defaultSettings = CreateDefault();

            if (!ContainsJsonProperty(json, "EnableHotkeys"))
            {
                settings.EnableHotkeys = defaultSettings.EnableHotkeys;
            }

            if (!ContainsJsonProperty(json, "ShowFloatingPanel"))
            {
                settings.ShowFloatingPanel = defaultSettings.ShowFloatingPanel;
            }

            if (!ContainsJsonProperty(json, "FloatingPanelExpanded"))
            {
                settings.FloatingPanelExpanded = defaultSettings.FloatingPanelExpanded;
            }

            if (!ContainsJsonProperty(json, "FloatingPanelSettingsExpanded"))
            {
                settings.FloatingPanelSettingsExpanded = defaultSettings.FloatingPanelSettingsExpanded;
            }
        }

        private static void ApplyMissingFloatDefaults(TranslationSettings settings, string json)
        {
            TranslationSettings defaultSettings = CreateDefault();

            if (!ContainsJsonProperty(json, "FloatingPanelX"))
            {
                settings.FloatingPanelX = defaultSettings.FloatingPanelX;
            }

            if (!ContainsJsonProperty(json, "FloatingPanelY"))
            {
                settings.FloatingPanelY = defaultSettings.FloatingPanelY;
            }
        }

        private static bool ContainsJsonProperty(string json, string propertyName)
        {
            return !string.IsNullOrEmpty(json) &&
                   json.IndexOf("\"" + propertyName + "\"", System.StringComparison.OrdinalIgnoreCase) >= 0;
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
