using System.Drawing;
using System.Windows.Forms;

namespace HeliosDisplayManagement.ShellExtension
{
    public static class Shield
    {
        private static Bitmap _smallIcon;

        public static Bitmap SmallIcon
        {
            get
            {
                if (_smallIcon != null)
                    return _smallIcon;
                var iconSize = SystemInformation.SmallIconSize;
                _smallIcon = new Bitmap(iconSize.Width, iconSize.Height);
                using (var g = Graphics.FromImage(_smallIcon))
                {
                    g.DrawIcon(SystemIcons.Shield, new Rectangle(Point.Empty, iconSize));
                }
                return _smallIcon;
            }
        }
    }
}