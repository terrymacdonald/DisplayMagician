using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    /// <summary>
    /// Provides the executable's application icon to every DisplayMagician form.
    /// The icon is loaded once so individual Designer resource files do not embed copies of it.
    /// </summary>
    public class DisplayMagicianForm : Form
    {
        private static readonly Icon _applicationIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application;

        public DisplayMagicianForm()
        {
            Icon = _applicationIcon;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (!DesignMode && LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                ShortcutManager.ConfigureWindowTaskbar(Handle);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // Window properties own native resources and must be cleared before destruction.
            // OnHandleCreated reapplies them if WinForms recreates the handle (for example, for DPI).
            if (IsHandleCreated && !DesignMode && LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                ShortcutManager.ConfigureWindowTaskbar(Handle, clear: true);
            base.OnHandleDestroyed(e);
        }
    }
}
