using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HeliosPlus.Shared;
using HeliosPlus.ShellExtension.Resources;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;

namespace HeliosPlus.ShellExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.Class, @"DesktopBackground")]
    [Guid("2EC0C798-715B-458E-8C86-5D846F67FBA1")]
    internal class HeliosDesktopMenuExtension : SharpContextMenu
    {
        private static ToolStripMenuItem CreateProfileMenu(ProfileItem profile)
        {
            var profileMenu = new ToolStripMenuItem(profile.Name, new ProfileIcon(profile).ToBitmap(16, 16));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem("Change Profile", null,
                (sender, args) => HeliosPlus.Open(HeliosStartupAction.ChangeProfile, profile))
            {
                Enabled = profile.IsPossible && !profile.IsActive
            }) ;
            profileMenu.DropDownItems.Add(new ToolStripSeparator());
            profileMenu.DropDownItems.Add(new ToolStripMenuItem("Run Shortcut", null,
                (sender, args) => HeliosPlus.Open(HeliosStartupAction.RunShortcut, profile)));

            return profileMenu;
        }

        protected override bool CanShowMenu()
        {
            return Helios.IsInstalled;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var explorerMenu = new ContextMenuStrip();

            if (ProfileRepository.AllProfiles.Any())
            {
                var extensionMenu = new ToolStripMenuItem(Language.Display_Profiles,
                    Properties.Resources.HeliosPlus.ToBitmap());

                foreach (var profile in ProfileRepository.AllProfiles)
                {
                    extensionMenu.DropDownItems.Add(CreateProfileMenu(profile));
                }

                extensionMenu.DropDownItems.Add(new ToolStripSeparator());
                extensionMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Manage_Profiles,
                    Properties.Resources.HeliosPlus.ToBitmap(),
                    (sender, args) =>
                    {
                        HeliosPlus.Open();
                    }));
                explorerMenu.Items.Add(extensionMenu);
                explorerMenu.Items.Add(new ToolStripSeparator());
            }
            else
            {
                var extensionMenu = new ToolStripMenuItem(Language.Manage_Profiles,
                    Properties.Resources.HeliosPlus.ToBitmap(),
                    (sender, args) =>
                    {
                        HeliosPlus.Open();
                    });
                explorerMenu.Items.Add(extensionMenu);
                explorerMenu.Items.Add(new ToolStripSeparator());
            }

            return explorerMenu;
        }
    }
}