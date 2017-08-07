using System;
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
    [COMServerAssociation(AssociationType.ClassOfExtension, @".url")]
    internal class HeliosSteamUrlMenuExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return Shared.Helios.IsInstalled &&
                   (SelectedItemPaths.Count() == 1) &&
                   Profile.GetAllProfiles().Any() &&
                   (ParseSteamAppId() > 0);
        }

        protected override ContextMenuStrip CreateMenu()
        {
            var explorerMenu = new ContextMenuStrip();
            var extensionMenu = new ToolStripMenuItem(Language.Open_under_Display_Profile,
                Properties.Resources.Icon_x16);
            foreach (var profile in Profile.GetAllProfiles())
                extensionMenu.DropDownItems.Add(CreateProfileMenu(profile));
            extensionMenu.DropDownItems.Add(new ToolStripSeparator());
            extensionMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Manage_Profiles,
                Properties.Resources.Icon_x16,
                (sender, args) => { HeliosDisplayManagement.Open(); }));
            explorerMenu.Items.Add(extensionMenu);
            explorerMenu.Items.Add(new ToolStripSeparator());
            return explorerMenu;
        }

        private ToolStripMenuItem CreateProfileMenu(Profile profile)
        {
            var appId = ParseSteamAppId();
            var profileMenu = new ToolStripMenuItem(profile.Name, new ProfileIcon(profile).ToBitmap(16, 16));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Run, null,
                (sender, args) =>
                    HeliosDisplayManagement.OpenSteamGame(HeliosStartupAction.SwitchProfile, profile,
                        appId)));
            profileMenu.DropDownItems.Add(new ToolStripSeparator());
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Edit, null,
                (sender, args) => HeliosDisplayManagement.Open(HeliosStartupAction.EditProfile, profile)));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Create_Shortcut, null,
                (sender, args) =>
                    HeliosDisplayManagement.OpenSteamGame(HeliosStartupAction.CreateShortcut, profile,
                        appId)));
            return profileMenu;
        }

        private uint ParseSteamAppId()
        {
            try
            {
                var fileAddress = SelectedItemPaths.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(fileAddress) && File.Exists(fileAddress))
                {
                    var fileContent = File.ReadAllText(fileAddress);
                    if (!fileContent.Contains(@"[InternetShortcut]"))
                        return 0;
                    var steamUrlPattern = @"steam://rungameid/";
                    var urlIndex = fileContent.IndexOf(steamUrlPattern, StringComparison.InvariantCultureIgnoreCase);
                    if (urlIndex < 0)
                        return 0;
                    var nextLine = fileContent.IndexOf(@"\r", urlIndex + steamUrlPattern.Length,
                        StringComparison.InvariantCultureIgnoreCase);
                    if (nextLine < 0)
                        nextLine = fileContent.Length - 1;
                    var appIdString = fileContent.Substring(urlIndex + steamUrlPattern.Length,
                        nextLine - urlIndex - steamUrlPattern.Length);
                    uint appId;
                    if (uint.TryParse(appIdString, out appId))
                        return appId;
                }
            }
            catch
            {
                // ignored
            }
            return 0;
        }
    }
}