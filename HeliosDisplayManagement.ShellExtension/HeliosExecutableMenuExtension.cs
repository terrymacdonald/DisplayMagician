using System.IO;
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
    [COMServerAssociation(AssociationType.ClassOfExtension, @".exe")]
    internal class HeliosExecutableMenuExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return Helios.IsInstalled &&
                   (SelectedItemPaths.Count() == 1) &&
                   Profile.GetAllProfiles().Any() &&
                   (Path.GetExtension(SelectedItemPaths.First())?.ToLower() == @".exe");
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var explorerMenu = new ContextMenuStrip();
            var extensionMenu = new ToolStripMenuItem(Language.Open_under_Display_Profile,
                Properties.Resources.Icon_x16);
            if (Profile.GetAllProfiles().Any())
            {
                Profile.RefreshActiveStatus();
                foreach (var profile in Profile.GetAllProfiles())
                    extensionMenu.DropDownItems.Add(CreateProfileMenu(profile));
                extensionMenu.DropDownItems.Add(new ToolStripSeparator());
            }
            extensionMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Manage_Profiles,
                Properties.Resources.Icon_x16,
                (sender, args) => { HeliosDisplayManagement.Open(); }));
            explorerMenu.Items.Add(extensionMenu);
            explorerMenu.Items.Add(new ToolStripSeparator());
            return explorerMenu;
        }

        private ToolStripMenuItem CreateProfileMenu(Profile profile)
        {
            var profileMenu = new ToolStripMenuItem(profile.Name, new ProfileIcon(profile).ToBitmap(16, 16));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Run, Properties.Resources.Run_x16,
                (sender, args) =>
                    HeliosDisplayManagement.Open(HeliosStartupAction.SwitchProfile, profile,
                        SelectedItemPaths.FirstOrDefault())));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Run_as_administrator, Shield.SmallIcon,
                (sender, args) =>
                    HeliosDisplayManagement.Open(HeliosStartupAction.SwitchProfile, profile,
                        SelectedItemPaths.FirstOrDefault(), true)));
            profileMenu.DropDownItems.Add(new ToolStripSeparator());
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Create_Shortcut,
                Properties.Resources.Shortcut_x16,
                (sender, args) =>
                    HeliosDisplayManagement.Open(HeliosStartupAction.CreateShortcut, profile,
                        SelectedItemPaths.FirstOrDefault())));
            return profileMenu;
        }
    }
}