using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Jundroo.Juicy.Widgets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Globalization;
using System.Reflection;

namespace SimplePlanes2TranslationMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class SimplePlanes2TranslationPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.codex.simpleplanes2.translation";
        public const string PluginName = "SimplePlanes 2 Translation Mod";
        public const string PluginVersion = "0.1.2";

        private const string ManualReloadHotkeyName = "F2";
        private const string ToggleTranslationHotkeyName = "F1";
        private const float MinimumSceneScanIntervalSeconds = 0.1f;
        private const float DefaultIdleSceneScanIntervalSeconds = 1.0f;
        private const float DefaultInteractiveSceneScanDurationSeconds = 1.0f;
        private const float MouseReleaseSceneScanDelaySeconds = 0.1f;

        private static readonly HashSet<string> MissingTexts = new HashSet<string>(StringComparer.Ordinal);
        private static readonly object MissingTextsLock = new object();

        private sealed class TrackedTextState
        {
            public string OriginalText { get; set; }

            public FontStyles OriginalFontStyle { get; set; }

            public TMP_FontAsset OriginalFontAsset { get; set; }
        }

        private Harmony _harmony;
        private BundledChineseFontLoader _bundledChineseFontLoader;
        private TMP_FontAsset _bundledChineseFontAsset;
        private TranslationCatalog _catalog = TranslationCatalog.Empty;
        private TextCaptureStore _captureStore;
        private readonly Dictionary<int, TrackedTextState> _trackedTexts = new Dictionary<int, TrackedTextState>();
        private readonly Dictionary<int, string> _lastSceneScanTexts = new Dictionary<int, string>();
        private TranslationSettings _settings = TranslationSettings.CreateDefault();
        private bool _isTranslationTemporarilyDisabled;
        private bool _hasLoggedBundledFontFallbackInjection;
        private TMP_FontAsset _preferredChineseFontAsset;
        private float _nextCaptureFlushTime;
        private float _nextSceneScanTime;
        private float _interactiveSceneScanUntilTime;
        private float _delayedMouseReleaseSceneScanTime = -1.0f;
        private string _capturedTextsPath = string.Empty;
        private string _missingTextsPath = string.Empty;
        private string _pluginRootPath = string.Empty;
        private string _settingsPath = string.Empty;
        private string _translationsPath = string.Empty;

        internal static SimplePlanes2TranslationPlugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            _pluginRootPath = Path.GetDirectoryName(Info.Location) ?? Paths.PluginPath;
            _settingsPath = Path.Combine(_pluginRootPath, "settings.json");

            LoadSettings();
            _translationsPath = Path.Combine(_pluginRootPath, "translations", _settings.Language + ".json");
            _capturedTextsPath = Path.Combine(_pluginRootPath, _settings.CapturedTextsFileName ?? "captured-texts.json");
            _missingTextsPath = Path.Combine(_pluginRootPath, "missing-texts.txt");
            _captureStore = new TextCaptureStore(_capturedTextsPath);
            _bundledChineseFontLoader = new BundledChineseFontLoader(Logger);
            LoadCatalog();
            LoadBundledChineseFont();

            _harmony = new Harmony(PluginGuid);
            _harmony.PatchAll(typeof(SimplePlanes2TranslationPlugin));

            SceneManager.sceneLoaded += OnSceneLoaded;
            ApplySceneTranslations("Awake");

            Logger.LogInfo(string.Format("Loaded {0} v{1} in {2} mode with {3} translation entries.", PluginName, PluginVersion, _settings.Mode, _catalog.Count));
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            FlushCapturedTexts();

            if (_harmony != null)
            {
                _harmony.UnpatchSelf();
                _harmony = null;
            }

            if (_bundledChineseFontLoader != null)
            {
                _bundledChineseFontLoader.Dispose();
                _bundledChineseFontLoader = null;
            }

            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                ReloadTranslations();
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleTranslation();
            }

            RefreshInteractiveSceneScanWindow();
            ScheduleMouseReleaseSceneScan();

            if (ShouldCaptureTexts() && Time.unscaledTime >= _nextCaptureFlushTime)
            {
                _nextCaptureFlushTime = Time.unscaledTime + Math.Max(2.0f, _settings.CaptureFlushIntervalSeconds);
                FlushCapturedTexts();
            }

            if (!_settings.EnableSceneScan)
            {
                return;
            }

            if (TryRunDelayedMouseReleaseSceneScan())
            {
                return;
            }

            if (Time.unscaledTime < _nextSceneScanTime)
            {
                return;
            }

            _nextSceneScanTime = Time.unscaledTime + GetCurrentSceneScanIntervalSeconds();
            ApplySceneTranslations(IsInteractiveSceneScanActive() ? "Interactive Scan" : "Idle Scan");
        }

        internal string Translate(string source, TextCaptureContext context)
        {
            string translated;

            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            if (_catalog.TryTranslate(source, context, out translated))
            {
                return translated;
            }

            RecordMissingText(source);
            return source;
        }

        private void ApplySceneTranslations(string reason)
        {
            int changedCount = 0;
            TextWidget[] textWidgets = Resources.FindObjectsOfTypeAll<TextWidget>();
            int i;

            for (i = 0; i < textWidgets.Length; i++)
            {
                TextWidget textWidget = textWidgets[i];
                string originalText;
                string translatedText;
                TextCaptureContext captureContext;

                if (textWidget == null || textWidget.TextMeshPro == null || !IsSceneObject(textWidget.gameObject))
                {
                    continue;
                }

                originalText = textWidget.TextMeshPro.text;
                if (string.IsNullOrEmpty(originalText))
                {
                    continue;
                }

                if (!ShouldProcessSceneText(textWidget.TextMeshPro, originalText))
                {
                    continue;
                }

                captureContext = CreateCaptureContext(textWidget.TextMeshPro, "SceneScan.TextWidget");
                ObserveText(originalText, captureContext);

                if (!ShouldTranslateTexts())
                {
                    continue;
                }

                translatedText = Translate(originalText, captureContext);
                if (translatedText == originalText)
                {
                    continue;
                }

                RememberOriginalText(textWidget.TextMeshPro, originalText);
                textWidget.SetText(translatedText, true);
                ApplyTranslatedTextStyle(textWidget.TextMeshPro, translatedText);
                RememberSceneScanText(textWidget.TextMeshPro, translatedText);
                changedCount++;
            }

            if (!_settings.CaptureStandaloneTmpTexts)
            {
                if (_settings.VerboseLogging && changedCount > 0)
                {
                    Logger.LogInfo(string.Format("Applied {0} translations during {1}.", changedCount, reason));
                }

                return;
            }

            TextMeshProUGUI[] tmpTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            for (i = 0; i < tmpTexts.Length; i++)
            {
                TextMeshProUGUI tmpText = tmpTexts[i];
                string originalText;
                string translatedText;
                TextCaptureContext captureContext;

                if (tmpText == null || !IsSceneObject(tmpText.gameObject))
                {
                    continue;
                }

                if (tmpText.GetComponentInParent<TextWidget>() != null)
                {
                    continue;
                }

                originalText = tmpText.text;
                if (string.IsNullOrEmpty(originalText))
                {
                    continue;
                }

                if (!ShouldProcessSceneText(tmpText, originalText))
                {
                    continue;
                }

                captureContext = CreateCaptureContext(tmpText, "SceneScan.TMP");
                ObserveText(originalText, captureContext);

                if (!ShouldTranslateTexts())
                {
                    continue;
                }

                translatedText = Translate(originalText, captureContext);
                if (translatedText == originalText)
                {
                    continue;
                }

                RememberOriginalText(tmpText, originalText);
                tmpText.text = translatedText;
                ApplyTranslatedTextStyle(tmpText, translatedText);
                RememberSceneScanText(tmpText, translatedText);
                changedCount++;
            }

            if (_settings.VerboseLogging && changedCount > 0)
            {
                Logger.LogInfo(string.Format("Applied {0} translations during {1}.", changedCount, reason));
            }
        }

        private void ScheduleMouseReleaseSceneScan()
        {
            if (!Input.GetMouseButtonUp(0) && !Input.GetMouseButtonUp(1))
            {
                return;
            }

            _delayedMouseReleaseSceneScanTime = Time.unscaledTime + MouseReleaseSceneScanDelaySeconds;
            RequestInteractiveSceneScan();
        }

        private bool TryRunDelayedMouseReleaseSceneScan()
        {
            if (_delayedMouseReleaseSceneScanTime < 0.0f)
            {
                return false;
            }

            if (Time.unscaledTime < _delayedMouseReleaseSceneScanTime)
            {
                return false;
            }

            _delayedMouseReleaseSceneScanTime = -1.0f;
            _nextSceneScanTime = Time.unscaledTime + GetCurrentSceneScanIntervalSeconds();
            ApplySceneTranslations("Mouse Release Scan");
            return true;
        }

        private void RefreshInteractiveSceneScanWindow()
        {
            if (!HasUserInterfaceActivity())
            {
                return;
            }

            RequestInteractiveSceneScan();
        }

        private static bool HasUserInterfaceActivity()
        {
            return Input.anyKeyDown ||
                   Input.GetMouseButtonDown(0) ||
                   Input.GetMouseButtonDown(1) ||
                   Input.GetMouseButtonDown(2) ||
                   Input.mouseScrollDelta.sqrMagnitude > 0.0f;
        }

        private void RequestInteractiveSceneScan()
        {
            float activeUntilTime;

            activeUntilTime = Time.unscaledTime + GetInteractiveSceneScanDurationSeconds();
            if (activeUntilTime > _interactiveSceneScanUntilTime)
            {
                _interactiveSceneScanUntilTime = activeUntilTime;
            }

            if (_nextSceneScanTime > Time.unscaledTime)
            {
                _nextSceneScanTime = Time.unscaledTime;
            }
        }

        private bool IsInteractiveSceneScanActive()
        {
            return Time.unscaledTime < _interactiveSceneScanUntilTime;
        }

        private float GetCurrentSceneScanIntervalSeconds()
        {
            if (IsInteractiveSceneScanActive())
            {
                return Mathf.Max(MinimumSceneScanIntervalSeconds, _settings.SceneScanIntervalSeconds);
            }

            return Mathf.Max(DefaultIdleSceneScanIntervalSeconds, _settings.IdleSceneScanIntervalSeconds);
        }

        private float GetInteractiveSceneScanDurationSeconds()
        {
            return Mathf.Max(DefaultInteractiveSceneScanDurationSeconds, _settings.InteractiveSceneScanDurationSeconds);
        }

        private bool ShouldProcessSceneText(TMP_Text textComponent, string displayedText)
        {
            int instanceId;
            string lastScannedText;

            if (textComponent == null)
            {
                return false;
            }

            instanceId = textComponent.GetInstanceID();
            if (_lastSceneScanTexts.TryGetValue(instanceId, out lastScannedText) &&
                string.Equals(lastScannedText, displayedText, StringComparison.Ordinal))
            {
                return false;
            }

            _lastSceneScanTexts[instanceId] = displayedText;
            return true;
        }

        private void RememberSceneScanText(TMP_Text textComponent, string displayedText)
        {
            if (textComponent == null)
            {
                return;
            }

            _lastSceneScanTexts[textComponent.GetInstanceID()] = displayedText ?? string.Empty;
        }

        private void LoadCatalog()
        {
            _catalog = TranslationCatalog.Load(_translationsPath, Logger);
        }

        private void LoadBundledChineseFont()
        {
            string fontFilePath;

            if (_bundledChineseFontLoader == null)
            {
                _bundledChineseFontAsset = null;
                return;
            }

            fontFilePath = Path.Combine(_pluginRootPath, "fonts", "SourceHanSansSC-Regular.otf");
            _bundledChineseFontAsset = _bundledChineseFontLoader.LoadFontAsset(fontFilePath);
            InjectBundledChineseFontFallbacks();
        }

        private void LoadSettings()
        {
            _settings = TranslationSettings.LoadOrCreate(_settingsPath, Logger);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            InjectBundledChineseFontFallbacks();
            _lastSceneScanTexts.Clear();
            RequestInteractiveSceneScan();
            ApplySceneTranslations("Scene Loaded: " + scene.name);
        }

        private void RecordMissingText(string source)
        {
            string directoryPath;

            if (!_settings.LogMissingTexts || !ShouldRecordMissingText(source))
            {
                return;
            }

            lock (MissingTextsLock)
            {
                if (!MissingTexts.Add(source))
                {
                    return;
                }

                directoryPath = Path.GetDirectoryName(_missingTextsPath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                File.AppendAllText(_missingTextsPath, source + Environment.NewLine, System.Text.Encoding.UTF8);
            }
        }

        private void ReloadTranslations()
        {
            LoadSettings();
            _translationsPath = Path.Combine(_pluginRootPath, "translations", _settings.Language + ".json");
            _capturedTextsPath = Path.Combine(_pluginRootPath, _settings.CapturedTextsFileName ?? "captured-texts.json");
            _captureStore = new TextCaptureStore(_capturedTextsPath);
            LoadCatalog();
            LoadBundledChineseFont();
            MissingTexts.Clear();
            _trackedTexts.Clear();
            _lastSceneScanTexts.Clear();
            _isTranslationTemporarilyDisabled = false;
            RequestInteractiveSceneScan();
            ApplySceneTranslations("Manual Reload (" + ManualReloadHotkeyName + ")");
            Logger.LogInfo(string.Format("Reloaded translations from '{0}'.", _translationsPath));
        }

        private void FlushCapturedTexts()
        {
            if (_captureStore == null || !ShouldCaptureTexts())
            {
                return;
            }

            _captureStore.Flush(Logger);
        }

        private void ObserveText(string text, TextCaptureContext captureContext)
        {
            if (!ShouldCaptureTexts())
            {
                return;
            }

            if (_captureStore == null)
            {
                return;
            }

            _captureStore.Record(
                text,
                captureContext != null ? captureContext.Source : string.Empty,
                SceneManager.GetActiveScene().name,
                captureContext);
        }

        private TextCaptureContext CreateCaptureContext(TMP_Text textComponent, string source)
        {
            GameObject gameObject;
            Transform transform;
            RectTransform rectTransform;
            Transform parentTransform;
            string anchoredPosition;

            if (textComponent == null)
            {
                return new TextCaptureContext
                {
                    SceneName = SceneManager.GetActiveScene().name,
                    Source = source,
                    ComponentType = "Unknown"
                };
            }

            gameObject = textComponent.gameObject;
            transform = gameObject != null ? gameObject.transform : null;
            rectTransform = transform as RectTransform;
            parentTransform = transform != null ? transform.parent : null;
            anchoredPosition = GetAnchoredPositionText(rectTransform);

            return new TextCaptureContext
            {
                SceneName = SceneManager.GetActiveScene().name,
                Source = source,
                ObjectName = gameObject != null ? gameObject.name : string.Empty,
                ParentName = parentTransform != null ? parentTransform.name : string.Empty,
                GameObjectPath = GetTransformPath(transform),
                ParentPath = GetTransformPath(parentTransform),
                ComponentType = textComponent.GetType().Name,
                SiblingIndex = transform != null ? (int?)transform.GetSiblingIndex() : null,
                AnchoredPosition = anchoredPosition
            };
        }

        private static string GetTransformPath(Transform transform)
        {
            Stack<string> pathSegments;

            if (transform == null)
            {
                return string.Empty;
            }

            pathSegments = new Stack<string>();
            while (transform != null)
            {
                pathSegments.Push(transform.name);
                transform = transform.parent;
            }

            return string.Join("/", pathSegments.ToArray());
        }

        private static string GetAnchoredPositionText(RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                return string.Empty;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                "{0:0.##},{1:0.##}",
                rectTransform.anchoredPosition.x,
                rectTransform.anchoredPosition.y);
        }

        private void RememberOriginalText(TMP_Text textComponent, string originalText)
        {
            int instanceId;
            TrackedTextState trackedTextState;

            if (textComponent == null || string.IsNullOrEmpty(originalText))
            {
                return;
            }

            instanceId = textComponent.GetInstanceID();
            if (!_trackedTexts.TryGetValue(instanceId, out trackedTextState))
            {
                trackedTextState = new TrackedTextState
                {
                    OriginalText = originalText,
                    OriginalFontStyle = textComponent.fontStyle,
                    OriginalFontAsset = textComponent.font
                };

                _trackedTexts.Add(instanceId, trackedTextState);
                return;
            }

            trackedTextState.OriginalText = originalText;
        }

        private void ToggleTranslation()
        {
            _isTranslationTemporarilyDisabled = !_isTranslationTemporarilyDisabled;

            if (_isTranslationTemporarilyDisabled)
            {
                RestoreTrackedTexts();
                Logger.LogInfo(string.Format("Translation disabled ({0}).", ToggleTranslationHotkeyName));
                return;
            }

            ApplySceneTranslations("Toggle Translation (" + ToggleTranslationHotkeyName + ")");
            Logger.LogInfo(string.Format("Translation enabled ({0}).", ToggleTranslationHotkeyName));
        }

        private void RestoreTrackedTexts()
        {
            RestoreTrackedTextWidgets();
            RestoreTrackedTmpTexts();
        }

        private void RestoreTrackedTextWidgets()
        {
            TextWidget[] textWidgets;
            int i;

            textWidgets = Resources.FindObjectsOfTypeAll<TextWidget>();
            for (i = 0; i < textWidgets.Length; i++)
            {
                TextWidget textWidget;
                TMP_Text textComponent;
                TrackedTextState trackedTextState;

                textWidget = textWidgets[i];
                if (textWidget == null || textWidget.TextMeshPro == null || !IsSceneObject(textWidget.gameObject))
                {
                    continue;
                }

                textComponent = textWidget.TextMeshPro;
                if (!_trackedTexts.TryGetValue(textComponent.GetInstanceID(), out trackedTextState))
                {
                    continue;
                }

                textWidget.SetText(trackedTextState.OriginalText, true);
                textComponent.font = trackedTextState.OriginalFontAsset;
                textComponent.fontStyle = trackedTextState.OriginalFontStyle;
                textComponent.ForceMeshUpdate();
            }
        }

        private void RestoreTrackedTmpTexts()
        {
            TextMeshProUGUI[] tmpTexts;
            int i;

            tmpTexts = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
            for (i = 0; i < tmpTexts.Length; i++)
            {
                TextMeshProUGUI tmpText;
                TrackedTextState trackedTextState;

                tmpText = tmpTexts[i];
                if (tmpText == null || !IsSceneObject(tmpText.gameObject))
                {
                    continue;
                }

                if (!_trackedTexts.TryGetValue(tmpText.GetInstanceID(), out trackedTextState))
                {
                    continue;
                }

                tmpText.text = trackedTextState.OriginalText;
                tmpText.font = trackedTextState.OriginalFontAsset;
                tmpText.fontStyle = trackedTextState.OriginalFontStyle;
                tmpText.ForceMeshUpdate();
            }
        }

        private void ApplyTranslatedTextStyle(TMP_Text textComponent, string displayedText)
        {
            if (textComponent == null || string.IsNullOrEmpty(displayedText))
            {
                return;
            }

            if (!ContainsCjkCharacter(displayedText))
            {
                return;
            }

            EnsureChineseFontSupport(textComponent, displayedText);

            if (_settings.ApplyBoldStyleToTranslatedText)
            {
                textComponent.fontStyle = textComponent.fontStyle | FontStyles.Bold;
            }

            textComponent.ForceMeshUpdate();
        }

        private void EnsureChineseFontSupport(TMP_Text textComponent, string displayedText)
        {
            TMP_FontAsset currentFontAsset;
            TMP_FontAsset fallbackFontAsset;

            if (TryInjectBundledChineseFontFallback(textComponent, displayedText))
            {
                return;
            }

            currentFontAsset = textComponent.font;
            if (CanFontAssetDisplayText(currentFontAsset, displayedText))
            {
                return;
            }

            fallbackFontAsset = FindChineseCapableFontAsset(displayedText);
            if (fallbackFontAsset == null || ReferenceEquals(fallbackFontAsset, currentFontAsset))
            {
                return;
            }

            textComponent.font = fallbackFontAsset;
        }

        private bool TryInjectBundledChineseFontFallback(TMP_Text textComponent, string displayedText)
        {
            if (_bundledChineseFontAsset == null || _bundledChineseFontLoader == null)
            {
                return false;
            }

            _bundledChineseFontLoader.PrepareFontAssetForText(_bundledChineseFontAsset, displayedText);
            InjectBundledChineseFontFallbacks();
            if (!ReferenceEquals(textComponent.font, _bundledChineseFontAsset))
            {
                textComponent.font = _bundledChineseFontAsset;
            }

            return true;
        }

        private void InjectBundledChineseFontFallbacks()
        {
            TMP_FontAsset[] fontAssets;
            int i;

            if (_bundledChineseFontAsset == null)
            {
                return;
            }

            TryAddFallbackToTmpSettings(_bundledChineseFontAsset);

            fontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            for (i = 0; i < fontAssets.Length; i++)
            {
                TMP_FontAsset fontAsset;

                fontAsset = fontAssets[i];
                if (fontAsset == null || ReferenceEquals(fontAsset, _bundledChineseFontAsset))
                {
                    continue;
                }

                TryAddFallbackToFontAsset(fontAsset, _bundledChineseFontAsset);
            }

            if (!_hasLoggedBundledFontFallbackInjection)
            {
                Logger.LogInfo("Injected bundled Chinese font into TMP fallback chains.");
                _hasLoggedBundledFontFallbackInjection = true;
            }
        }

        private static void TryAddFallbackToTmpSettings(TMP_FontAsset fallbackFontAsset)
        {
            Type settingsType;
            PropertyInfo instanceProperty;
            object settingsInstance;

            settingsType = typeof(TMP_Settings);
            instanceProperty = settingsType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (instanceProperty == null)
            {
                return;
            }

            settingsInstance = instanceProperty.GetValue(null, null);
            if (settingsInstance == null)
            {
                return;
            }

            TryAddFontAssetToCollection(settingsInstance, "fallbackFontAssets", fallbackFontAsset);
        }

        private static void TryAddFallbackToFontAsset(TMP_FontAsset fontAsset, TMP_FontAsset fallbackFontAsset)
        {
            TryAddFontAssetToCollection(fontAsset, "fallbackFontAssetTable", fallbackFontAsset);
        }

        private static bool HasFallbackReference(TMP_FontAsset fontAsset, TMP_FontAsset fallbackFontAsset)
        {
            IList collection;
            int i;

            collection = GetFontAssetCollection(fontAsset, "fallbackFontAssetTable");
            if (collection == null)
            {
                return false;
            }

            for (i = 0; i < collection.Count; i++)
            {
                if (ReferenceEquals(collection[i], fallbackFontAsset))
                {
                    return true;
                }
            }

            return false;
        }

        private static void TryAddFontAssetToCollection(object target, string memberName, TMP_FontAsset fontAssetToAdd)
        {
            IList collection;

            collection = GetFontAssetCollection(target, memberName);
            if (collection == null)
            {
                return;
            }

            if (!collection.Contains(fontAssetToAdd))
            {
                collection.Add(fontAssetToAdd);
            }
        }

        private static IList GetFontAssetCollection(object target, string memberName)
        {
            PropertyInfo propertyInfo;
            FieldInfo fieldInfo;
            object value;
            IList collection;

            if (target == null)
            {
                return null;
            }

            propertyInfo = target.GetType().GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propertyInfo != null)
            {
                value = propertyInfo.GetValue(target, null);
                collection = value as IList;
                if (collection != null)
                {
                    return collection;
                }
            }

            fieldInfo = target.GetType().GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (fieldInfo == null)
            {
                return null;
            }

            value = fieldInfo.GetValue(target);
            return value as IList;
        }

        private TMP_FontAsset FindChineseCapableFontAsset(string displayedText)
        {
            TMP_FontAsset[] fontAssets;
            int i;

            if (CanFontAssetDisplayText(_preferredChineseFontAsset, displayedText))
            {
                return _preferredChineseFontAsset;
            }

            fontAssets = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
            for (i = 0; i < fontAssets.Length; i++)
            {
                TMP_FontAsset fontAsset;

                fontAsset = fontAssets[i];
                if (!CanFontAssetDisplayText(fontAsset, displayedText))
                {
                    continue;
                }

                _preferredChineseFontAsset = fontAsset;
                return fontAsset;
            }

            return null;
        }

        private static bool CanFontAssetDisplayText(TMP_FontAsset fontAsset, string displayedText)
        {
            MethodInfo hasCharactersMethod;
            MethodInfo hasCharacterMethod;
            int i;

            if (fontAsset == null || string.IsNullOrEmpty(displayedText))
            {
                return false;
            }

            hasCharactersMethod = fontAsset.GetType().GetMethod(
                "HasCharacters",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { typeof(string) },
                null);
            if (hasCharactersMethod != null)
            {
                return (bool)hasCharactersMethod.Invoke(fontAsset, new object[] { displayedText });
            }

            hasCharacterMethod = fontAsset.GetType().GetMethod(
                "HasCharacter",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[] { typeof(char) },
                null);
            if (hasCharacterMethod == null)
            {
                return true;
            }

            for (i = 0; i < displayedText.Length; i++)
            {
                char currentCharacter;

                currentCharacter = displayedText[i];
                if (currentCharacter == '\r' || currentCharacter == '\n' || currentCharacter == '\t')
                {
                    continue;
                }

                if ((bool)hasCharacterMethod.Invoke(fontAsset, new object[] { currentCharacter }))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool ShouldCaptureTexts()
        {
            return _settings.IsCollectMode() || _settings.IsHybridMode();
        }

        private bool ShouldTranslateTexts()
        {
            return !_isTranslationTemporarilyDisabled &&
                   (_settings.IsTranslateMode() || _settings.IsHybridMode());
        }

        private static bool IsSceneObject(GameObject gameObject)
        {
            return gameObject != null && gameObject.scene.IsValid();
        }

        private static bool ContainsCjkCharacter(string text)
        {
            int i;

            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            for (i = 0; i < text.Length; i++)
            {
                char currentChar = text[i];
                if ((currentChar >= 0x3400 && currentChar <= 0x4DBF) ||
                    (currentChar >= 0x4E00 && currentChar <= 0x9FFF) ||
                    (currentChar >= 0xF900 && currentChar <= 0xFAFF))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ShouldRecordMissingText(string source)
        {
            bool hasLetter = false;
            int i;

            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            if (source.Length > 120)
            {
                return false;
            }

            if (source.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            for (i = 0; i < source.Length; i++)
            {
                char currentChar = source[i];
                if (char.IsLetter(currentChar))
                {
                    hasLetter = true;
                    continue;
                }

                if (currentChar == '\r' || currentChar == '\n' || currentChar == '\t')
                {
                    return false;
                }
            }

            return hasLetter;
        }

        [HarmonyPatch(typeof(TextWidget), "SetText")]
        private static class TextWidgetSetTextPatch
        {
            private static void Prefix(TextWidget __instance, ref string text)
            {
                TextCaptureContext captureContext;

                if (Instance == null)
                {
                    return;
                }

                captureContext = Instance.CreateCaptureContext(__instance != null ? __instance.TextMeshPro : null, "Harmony.TextWidget.SetText");
                Instance.ObserveText(text, captureContext);

                if (!Instance.ShouldTranslateTexts())
                {
                    return;
                }

                if (__instance != null && __instance.TextMeshPro != null)
                {
                    Instance.RememberOriginalText(__instance.TextMeshPro, text);
                }

                text = Instance.Translate(text, captureContext);
            }

            private static void Postfix(TextWidget __instance)
            {
                if (Instance == null || __instance == null)
                {
                    return;
                }

                Instance.ApplyTranslatedTextStyle(__instance.TextMeshPro, __instance.TextMeshPro != null ? __instance.TextMeshPro.text : string.Empty);
                Instance.RememberSceneScanText(__instance.TextMeshPro, __instance.TextMeshPro != null ? __instance.TextMeshPro.text : string.Empty);
            }
        }
    }
}
