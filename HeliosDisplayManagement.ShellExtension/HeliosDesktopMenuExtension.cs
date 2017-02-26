using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HeliosDisplayManagement.Shared;
using HeliosDisplayManagement.ShellExtension.Resources;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace HeliosDisplayManagement.ShellExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Class, @"DesktopBackground")]
    internal class HeliosDesktopMenuExtension : SharpContextMenu
    {
        private static ToolStripMenuItem CreateProfileMenu(Profile profile)
        {
            var profileMenu = new ToolStripMenuItem(profile.Name, new ProfileIcon(profile).ToBitmap(16, 16));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Apply, null,
                (sender, args) => HeliosDisplayManagement.Open(HeliosStartupAction.SwitchProfile, profile))
            {
                Enabled = profile.IsPossible && !profile.IsActive
            });
            profileMenu.DropDownItems.Add(new ToolStripSeparator());
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Edit, null,
                (sender, args) => HeliosDisplayManagement.Open(HeliosStartupAction.EditProfile, profile)));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Create_Shortcut, null,
                (sender, args) => HeliosDisplayManagement.Open(HeliosStartupAction.CreateShortcut, profile)));
            return profileMenu;
        }

        protected override bool CanShowMenu()
        {
            return Shared.Helios.IsInstalled && Profile.GetAllProfiles().Any();
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var explorerMenu = new ContextMenuStrip();
            var extensionMenu = new ToolStripMenuItem(Language.Display_Profiles,
                Properties.Resources.DisplaySwitcher_x16);
            foreach (var profile in Profile.GetAllProfiles())
                extensionMenu.DropDownItems.Add(CreateProfileMenu(profile));
            extensionMenu.DropDownItems.Add(new ToolStripSeparator());
            extensionMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Manage_Profiles,
                Properties.Resources.DisplaySwitcher_x16,
                (sender, args) => { HeliosDisplayManagement.Open(); }));
            explorerMenu.Items.Add(extensionMenu);
            explorerMenu.Items.Add(new ToolStripSeparator());
            return explorerMenu;
        }
    }
}