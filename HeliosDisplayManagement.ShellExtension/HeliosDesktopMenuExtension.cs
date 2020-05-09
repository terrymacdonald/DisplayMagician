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
        private static ToolStripMenuItem CreateProfileMenu(Profile profile)
        {
            var profileMenu = new ToolStripMenuItem(profile.Name, new ProfileIcon(profile).ToBitmap(16, 16));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Apply, null,
                (sender, args) => HeliosPlus.Open(HeliosStartupAction.SwitchProfile, profile))
            {
                Enabled = profile.IsPossible && !profile.IsActive
            });
            profileMenu.DropDownItems.Add(new ToolStripSeparator());
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Edit, null,
                (sender, args) => HeliosPlus.Open(HeliosStartupAction.EditProfile, profile)));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Create_Shortcut, null,
                (sender, args) => HeliosPlus.Open(HeliosStartupAction.CreateShortcut, profile)));

            return profileMenu;
        }

        protected override bool CanShowMenu()
        {
            return Helios.IsInstalled;
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var explorerMenu = new ContextMenuStrip();

            if (Profile.LoadAllProfiles().Any())
            {
                Profile.RefreshActiveStatus();
                var extensionMenu = new ToolStripMenuItem(Language.Display_Profiles,
                    Properties.Resources.Icon_x16);

                foreach (var profile in Profile.LoadAllProfiles())
                {
                    extensionMenu.DropDownItems.Add(CreateProfileMenu(profile));
                }

                extensionMenu.DropDownItems.Add(new ToolStripSeparator());
                extensionMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Manage_Profiles,
                    Properties.Resources.Icon_x16,
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
                    Properties.Resources.Icon_x16,
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