using Newtonsoft.Json.Serialization;
using System;
using System.Runtime.Serialization;
using WindowsWallpaperWrapper;

namespace DisplayMagicianShared
{
    public sealed class DisplayMagicianSerializationBinder : ISerializationBinder
    {
        public static readonly DisplayMagicianSerializationBinder Instance = new DisplayMagicianSerializationBinder();

        private DisplayMagicianSerializationBinder()
        {
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return typeName switch
            {
                "WindowsWallpaperWrapper.PictureWallpaperConfig" => typeof(PictureWallpaperConfig),
                "WindowsWallpaperWrapper.SlideshowWallpaperConfig" => typeof(SlideshowWallpaperConfig),
                "WindowsWallpaperWrapper.SolidColorWallpaperConfig" => typeof(SolidColorWallpaperConfig),
                "WindowsWallpaperWrapper.SpotlightWallpaperConfig" => typeof(SpotlightWallpaperConfig),
                _ => throw new SerializationException($"DisplayMagician does not permit persisted type '{typeName}'.")
            };
        }

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (serializedType != typeof(PictureWallpaperConfig) &&
                serializedType != typeof(SlideshowWallpaperConfig) &&
                serializedType != typeof(SolidColorWallpaperConfig) &&
                serializedType != typeof(SpotlightWallpaperConfig))
            {
                throw new SerializationException($"DisplayMagician does not permit persisting type '{serializedType.FullName}'.");
            }

            assemblyName = serializedType.Assembly.GetName().Name;
            typeName = serializedType.FullName;
        }
    }
}
