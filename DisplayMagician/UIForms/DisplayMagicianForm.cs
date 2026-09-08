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
    }
}
