using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared
{
    public sealed class Wallpaper
    {
        Wallpaper() { }

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public enum Style : int
        {
            Fill = 0,
            Fit = 1,
            Stretch = 2,
            Tile = 3,
            Center = 4,            
            Span = 5
        }

        public enum Mode : int
        {
            DoNothing = 0,
            Clear = 1,
            Apply = 2
        }

        public static bool Set(String filename, Style style)
        {
            //System.IO.Stream s = new System.Net.WebClient().OpenRead(uri.ToString());

            Bitmap img = new Bitmap(filename);            

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Fill)
            {
                key.SetValue(@"WallpaperStyle", 10.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Fit)
            {
                key.SetValue(@"WallpaperStyle", 6.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Span) // Windows 8 or newer only!
            {
                key.SetValue(@"WallpaperStyle", 22.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Stretch)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }
            if (style == Style.Tile)
            {
                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }
            if (style == Style.Center)
            {
                key.SetValue(@"WallpaperStyle", 0.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                filename,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE) > 0)
            {
                // applying desktop wallpaper worked!
                return true;
            }
            else
            {
                // applying desktop wallpaper failed!
                return false;
            }
        }

        public static bool Clear()
        {
            RegistryKey desktopKey = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            desktopKey.SetValue(@"WallPaper", "");

            RegistryKey explorerKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Wallpapers", true);
            explorerKey.SetValue(@"BackgroundType", 1);

            if (SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                "",
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE) > 0)
            {
                // applying desktop wallpaper worked!
                return true;
            }
            else
            {
                // applying desktop wallpaper failed!
                return false;
            }

        }
    }
}
