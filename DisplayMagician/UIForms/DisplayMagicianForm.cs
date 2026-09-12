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
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Large, resizable forms can opt in to remain fully accessible when Windows moves them
        /// to a monitor with a different DPI. Small dialogs retain the normal WinForms behaviour.
        /// </summary>
        protected virtual bool FitToWorkingAreaAfterDpiChange => false;

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

        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);

            if (!FitToWorkingAreaAfterDpiChange || WindowState != FormWindowState.Normal)
                return;

            Rectangle suggestedBounds = e.SuggestedRectangle;
            Rectangle workingArea = Screen.FromRectangle(suggestedBounds).WorkingArea;
            int width = Math.Min(suggestedBounds.Width, workingArea.Width);
            int height = Math.Min(suggestedBounds.Height, workingArea.Height);
            int x = Math.Max(workingArea.Left, Math.Min(suggestedBounds.Left, workingArea.Right - width));
            int y = Math.Max(workingArea.Top, Math.Min(suggestedBounds.Top, workingArea.Bottom - height));
            Rectangle fittedBounds = new Rectangle(x, y, width, height);

            if (Bounds != fittedBounds)
            {
                logger.Info($"DisplayMagicianForm/OnDpiChanged: Fitting '{Name}' within the destination monitor working area after DPI changed from {e.DeviceDpiOld} to {e.DeviceDpiNew}.");
                Bounds = fittedBounds;
            }
        }
    }
}
