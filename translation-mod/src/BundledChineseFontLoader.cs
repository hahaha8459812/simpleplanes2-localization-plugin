using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using TMPro;
using System.Runtime.InteropServices;

namespace SimplePlanes2TranslationMod
{
    internal sealed class BundledChineseFontLoader : IDisposable
    {
        private const uint PrivateFontFlag = 0x10;

        private readonly ManualLogSource _logger;
        private string _registeredFontPath = string.Empty;

        public BundledChineseFontLoader(ManualLogSource logger)
        {
            _logger = logger;
        }

        public TMP_FontAsset LoadFontAsset(string fontFilePath)
        {
            TMP_FontAsset fontAsset;

            if (string.IsNullOrWhiteSpace(fontFilePath) || !File.Exists(fontFilePath))
            {
                _logger.LogWarning(string.Format("Bundled Chinese font file not found: {0}", fontFilePath));
                return null;
            }

            fontAsset = CreateFontAssetFromFile(fontFilePath);
            if (fontAsset == null)
            {
                fontAsset = CreateFontAssetFromRegisteredFamily(fontFilePath);
            }

            if (fontAsset == null)
            {
                _logger.LogWarning(string.Format("Failed to create TMP font asset from bundled Chinese font file: {0}", fontFilePath));
                return null;
            }

            ForceDynamicAtlasPopulation(fontAsset);
            fontAsset.name = "Bundled Chinese Font";
            _logger.LogInfo(string.Format("Loaded bundled Chinese font asset from '{0}'.", Path.GetFileName(fontFilePath)));
            return fontAsset;
        }

        public void PrepareFontAssetForText(TMP_FontAsset fontAsset, string displayedText)
        {
            MethodInfo[] methods;
            int i;

            if (fontAsset == null || string.IsNullOrEmpty(displayedText))
            {
                return;
            }

            methods = fontAsset
                .GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(method => string.Equals(method.Name, "TryAddCharacters", StringComparison.Ordinal))
                .OrderBy(method => method.GetParameters().Length)
                .ToArray();

            for (i = 0; i < methods.Length; i++)
            {
                MethodInfo method;
                object[] arguments;

                method = methods[i];
                arguments = BuildTryAddCharactersArguments(method, displayedText);
                if (arguments == null)
                {
                    continue;
                }

                try
                {
                    method.Invoke(fontAsset, arguments);
                    return;
                }
                catch
                {
                }
            }
        }

        public void Dispose()
        {
            UnregisterPrivateFont();
        }

        private static TMP_FontAsset CreateFontAssetFromFile(string fontFilePath)
        {
            MethodInfo[] createMethods;
            int i;

            createMethods = typeof(TMP_FontAsset)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method =>
                {
                    ParameterInfo[] parameters;

                    if (!string.Equals(method.Name, "CreateFontAsset", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    parameters = method.GetParameters();
                    return parameters.Length == 7 && parameters[0].ParameterType == typeof(string);
                })
                .ToArray();

            for (i = 0; i < createMethods.Length; i++)
            {
                MethodInfo createMethod;
                object[] arguments;
                object result;

                createMethod = createMethods[i];
                arguments = BuildCreateFontAssetFromFileArguments(createMethod, fontFilePath);
                if (arguments == null)
                {
                    continue;
                }

                try
                {
                    result = createMethod.Invoke(null, arguments);
                    if (result is TMP_FontAsset)
                    {
                        return result as TMP_FontAsset;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private TMP_FontAsset CreateFontAssetFromRegisteredFamily(string fontFilePath)
        {
            string fontFamilyName;
            string fontStyleName;
            MethodInfo[] createMethods;
            int i;

            RegisterPrivateFont(fontFilePath);
            fontFamilyName = ReadFontFamilyName(fontFilePath);
            if (string.IsNullOrEmpty(fontFamilyName))
            {
                return null;
            }

            fontStyleName = ReadFontStyleName(fontFilePath);
            createMethods = typeof(TMP_FontAsset)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(method =>
                {
                    ParameterInfo[] parameters;

                    if (!string.Equals(method.Name, "CreateFontAsset", StringComparison.Ordinal))
                    {
                        return false;
                    }

                    parameters = method.GetParameters();
                    return parameters.Length == 3 &&
                           parameters[0].ParameterType == typeof(string) &&
                           parameters[1].ParameterType == typeof(string);
                })
                .ToArray();

            for (i = 0; i < createMethods.Length; i++)
            {
                MethodInfo createMethod;
                object result;

                createMethod = createMethods[i];
                try
                {
                    result = createMethod.Invoke(null, new object[] { fontFamilyName, fontStyleName, 90 });
                    if (result is TMP_FontAsset)
                    {
                        return result as TMP_FontAsset;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static object[] BuildCreateFontAssetFromFileArguments(MethodInfo createMethod, string fontFilePath)
        {
            ParameterInfo[] parameters;
            object[] arguments;
            int i;

            parameters = createMethod.GetParameters();
            arguments = new object[parameters.Length];
            arguments[0] = fontFilePath;

            for (i = 1; i < parameters.Length; i++)
            {
                ParameterInfo parameter;
                Type parameterType;
                string parameterName;

                parameter = parameters[i];
                parameterType = parameter.ParameterType;
                parameterName = parameter.Name ?? string.Empty;

                if (parameterType == typeof(int))
                {
                    arguments[i] = ResolveIntArgument(parameterName);
                    continue;
                }

                if (parameterType.IsEnum)
                {
                    arguments[i] = ResolveEnumArgument(parameterType, parameterName);
                    continue;
                }

                return null;
            }

            return arguments;
        }

        private static void ForceDynamicAtlasPopulation(TMP_FontAsset fontAsset)
        {
            PropertyInfo atlasPopulationModeProperty;
            FieldInfo atlasPopulationModeField;

            if (fontAsset == null)
            {
                return;
            }

            atlasPopulationModeProperty = fontAsset.GetType().GetProperty("atlasPopulationMode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (atlasPopulationModeProperty != null && atlasPopulationModeProperty.CanWrite)
            {
                atlasPopulationModeProperty.SetValue(fontAsset, ResolveEnumArgument(atlasPopulationModeProperty.PropertyType, "atlasPopulationMode"), null);
            }

            atlasPopulationModeField = fontAsset.GetType().GetField("m_AtlasPopulationMode", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (atlasPopulationModeField != null)
            {
                atlasPopulationModeField.SetValue(fontAsset, ResolveEnumArgument(atlasPopulationModeField.FieldType, "atlasPopulationMode"));
            }
        }

        private static object[] BuildTryAddCharactersArguments(MethodInfo method, string displayedText)
        {
            ParameterInfo[] parameters;
            object[] arguments;
            int i;

            parameters = method.GetParameters();
            arguments = new object[parameters.Length];
            for (i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter;
                Type parameterType;

                parameter = parameters[i];
                parameterType = parameter.ParameterType;

                if (parameterType == typeof(string))
                {
                    arguments[i] = displayedText;
                    continue;
                }

                if (parameterType == typeof(bool))
                {
                    arguments[i] = false;
                    continue;
                }

                if (parameterType.IsByRef && parameterType.GetElementType() == typeof(string))
                {
                    arguments[i] = null;
                    continue;
                }

                return null;
            }

            return arguments;
        }

        private static int ResolveIntArgument(string parameterName)
        {
            if (parameterName.IndexOf("face", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 0;
            }

            if (parameterName.IndexOf("point", StringComparison.OrdinalIgnoreCase) >= 0 ||
                parameterName.IndexOf("sampling", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 90;
            }

            if (parameterName.IndexOf("padding", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 9;
            }

            if (parameterName.IndexOf("width", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 2048;
            }

            if (parameterName.IndexOf("height", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return 2048;
            }

            return 0;
        }

        private static object ResolveEnumArgument(Type enumType, string parameterName)
        {
            string[] preferredNames;
            int i;

            preferredNames = new string[0];
            if (parameterName.IndexOf("atlasPopulationMode", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                preferredNames = new[] { "Dynamic" };
            }
            else if (parameterName.IndexOf("renderMode", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                preferredNames = new[] { "SDFAA_HINTED", "SDFAA", "SDF", "SMOOTH_HINTED" };
            }

            for (i = 0; i < preferredNames.Length; i++)
            {
                string preferredName;

                preferredName = preferredNames[i];
                if (!Enum.IsDefined(enumType, preferredName))
                {
                    continue;
                }

                return Enum.Parse(enumType, preferredName);
            }

            return Enum.GetValues(enumType).GetValue(0);
        }

        private void RegisterPrivateFont(string fontFilePath)
        {
            UnregisterPrivateFont();
            if (AddFontResourceEx(fontFilePath, PrivateFontFlag, IntPtr.Zero) != 0)
            {
                _registeredFontPath = fontFilePath;
            }
        }

        private static string ReadFontFamilyName(string fontFilePath)
        {
            using (PrivateFontCollection privateFontCollection = new PrivateFontCollection())
            {
                privateFontCollection.AddFontFile(fontFilePath);
                if (privateFontCollection.Families.Length == 0)
                {
                    return string.Empty;
                }

                return privateFontCollection.Families[0].Name;
            }
        }

        private static string ReadFontStyleName(string fontFilePath)
        {
            string fileName;
            int separatorIndex;

            fileName = Path.GetFileNameWithoutExtension(fontFilePath) ?? string.Empty;
            separatorIndex = fileName.LastIndexOf('-');
            if (separatorIndex < 0 || separatorIndex >= fileName.Length - 1)
            {
                return "Regular";
            }

            return fileName.Substring(separatorIndex + 1);
        }

        private void UnregisterPrivateFont()
        {
            if (string.IsNullOrEmpty(_registeredFontPath))
            {
                return;
            }

            RemoveFontResourceEx(_registeredFontPath, PrivateFontFlag, IntPtr.Zero);
            _registeredFontPath = string.Empty;
        }

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int AddFontResourceEx(string name, uint flags, IntPtr reserved);

        [DllImport("gdi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool RemoveFontResourceEx(string name, uint flags, IntPtr reserved);
    }
}
