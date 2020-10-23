﻿using System.IO;
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
    [COMServerAssociation(AssociationType.ClassOfExtension, @".exe")]
    [Guid("48B49131-2258-4694-879F-A3F96310A220")]
    internal class HeliosExecutableMenuExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return Helios.IsInstalled &&
                   SelectedItemPaths.Count() == 1 &&
                   ProfileRepository.AllProfiles.Any() &&
                   Path.GetExtension(SelectedItemPaths.First())?.ToLower() == @".exe";
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var explorerMenu = new ContextMenuStrip();
            var extensionMenu = new ToolStripMenuItem(Language.Open_under_Display_Profile,
                Properties.Resources.HeliosPlus.ToBitmap());

            if (ProfileRepository.AllProfiles.Any())
            {
                foreach (var profile in ProfileRepository.AllProfiles)
                {
                    extensionMenu.DropDownItems.Add(CreateProfileMenu(profile));
                }

                extensionMenu.DropDownItems.Add(new ToolStripSeparator());
            }

            extensionMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Manage_Profiles,
                Properties.Resources.HeliosPlus.ToBitmap(),
                (sender, args) =>
                {
                    HeliosPlus.Open();
                }));
            explorerMenu.Items.Add(extensionMenu);
            explorerMenu.Items.Add(new ToolStripSeparator());

            return explorerMenu;
        }

        private ToolStripMenuItem CreateProfileMenu(ProfileItem profile)
        {
            var profileMenu = new ToolStripMenuItem(profile.Name, new ProfileIcon(profile).ToBitmap(16, 16));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Run, null,
                (sender, args) =>
                    HeliosPlus.Open(HeliosStartupAction.ChangeProfile, profile,
                        SelectedItemPaths.FirstOrDefault())));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Run_as_administrator, Shield.SmallIcon,
                (sender, args) =>
                    HeliosPlus.Open(HeliosStartupAction.ChangeProfile, profile,
                        SelectedItemPaths.FirstOrDefault(), true)));
            profileMenu.DropDownItems.Add(new ToolStripSeparator());
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Create_Shortcut, null,
                (sender, args) =>
                    HeliosPlus.Open(HeliosStartupAction.ChangeProfile, profile,
                        SelectedItemPaths.FirstOrDefault())));

            return profileMenu;
        }
    }
}