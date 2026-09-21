using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Runtime.Serialization;
using WindowsWallpaperWrapper;

namespace DisplayMagician.UserAgent.Runtime
{
    public sealed class DisplayMagicianSerializationBinder : ISerializationBinder
    {
        private const string LegacyRuntimeNamespacePrefix = "DisplayMagicianShared.";
        private const string RuntimeNamespacePrefix = "DisplayMagician.UserAgent.Runtime.";
        public static readonly DisplayMagicianSerializationBinder Instance = new DisplayMagicianSerializationBinder();

        private DisplayMagicianSerializationBinder()
        {
        }

        public Type BindToType(string? assemblyName, string typeName)
        {
            if (typeName.StartsWith(RuntimeNamespacePrefix, StringComparison.Ordinal))
            {
                Type? runtimeType = typeof(DisplayMagicianSerializationBinder).Assembly.GetType(typeName, throwOnError: false);
                if (runtimeType != null)
                {
                    return runtimeType;
                }
            }

            return typeName switch
            {
                "WindowsWallpaperWrapper.PictureWallpaperConfig" => typeof(PictureWallpaperConfig),
                "WindowsWallpaperWrapper.SlideshowWallpaperConfig" => typeof(SlideshowWallpaperConfig),
                "WindowsWallpaperWrapper.SolidColorWallpaperConfig" => typeof(SolidColorWallpaperConfig),
                "WindowsWallpaperWrapper.SpotlightWallpaperConfig" => typeof(SpotlightWallpaperConfig),
                _ => throw new SerializationException($"DisplayMagician does not permit persisted type '{typeName}'.")
            };
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string typeName)
        {
            if (serializedType.Assembly == typeof(DisplayMagicianSerializationBinder).Assembly &&
                serializedType.Namespace?.StartsWith(RuntimeNamespacePrefix.TrimEnd('.'), StringComparison.Ordinal) == true)
            {
                assemblyName = serializedType.Assembly.GetName().Name;
                typeName = serializedType.FullName ?? throw new SerializationException($"DisplayMagician cannot persist unnamed type '{serializedType}'.");
                return;
            }

            if (serializedType != typeof(PictureWallpaperConfig) &&
                serializedType != typeof(SlideshowWallpaperConfig) &&
                serializedType != typeof(SolidColorWallpaperConfig) &&
                serializedType != typeof(SpotlightWallpaperConfig))
            {
                throw new SerializationException($"DisplayMagician does not permit persisting type '{serializedType.FullName}'.");
            }

            assemblyName = serializedType.Assembly.GetName().Name;
            typeName = serializedType.FullName ?? throw new SerializationException($"DisplayMagician cannot persist unnamed type '{serializedType}'.");
        }

        public static bool TryMigrateLegacyRuntimeTypeNames(string json, out string migratedJson)
        {
            migratedJson = json;
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            JToken root = JToken.Parse(json);
            if (!MigrateLegacyRuntimeTypeNames(root))
            {
                return false;
            }

            migratedJson = root.ToString(Formatting.Indented);
            return true;
        }

        private static bool MigrateLegacyRuntimeTypeNames(JToken token)
        {
            bool changed = false;
            if (token is JObject jsonObject)
            {
                foreach (JProperty property in jsonObject.Properties().ToList())
                {
                    if (string.Equals(property.Name, "$type", StringComparison.Ordinal) && property.Value.Type == JTokenType.String)
                    {
                        string legacyTypeName = property.Value.Value<string>()!;
                        int assemblySeparatorIndex = legacyTypeName.IndexOf(',');
                        string typeName = assemblySeparatorIndex < 0 ? legacyTypeName : legacyTypeName.Substring(0, assemblySeparatorIndex).Trim();
                        if (typeName.StartsWith(LegacyRuntimeNamespacePrefix, StringComparison.Ordinal))
                        {
                            string migratedTypeName = RuntimeNamespacePrefix + typeName.Substring(LegacyRuntimeNamespacePrefix.Length);
                            string assemblyName = typeof(DisplayMagicianSerializationBinder).Assembly.GetName().Name!;
                            property.Value = assemblySeparatorIndex < 0 ? migratedTypeName : $"{migratedTypeName}, {assemblyName}";
                            changed = true;
                        }
                    }

                    changed |= MigrateLegacyRuntimeTypeNames(property.Value);
                }
            }
            else if (token is JArray jsonArray)
            {
                foreach (JToken item in jsonArray)
                {
                    changed |= MigrateLegacyRuntimeTypeNames(item);
                }
            }

            return changed;
        }
    }
}
