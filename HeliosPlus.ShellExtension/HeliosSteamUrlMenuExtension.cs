using System;
using System.IO;
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
    [COMServerAssociation(AssociationType.ClassOfExtension, @".url")]
    [Guid("E41ECFB2-3E7D-4A47-8A51-8627F1B21AE5")]
    internal class HeliosSteamUrlMenuExtension : SharpContextMenu
    {
        protected override bool CanShowMenu()
        {
            return Helios.IsInstalled &&
                   SelectedItemPaths.Count() == 1 &&
                   ProfileRepository.AllProfiles.Any() &&
                   ParseSteamAppId() > 0;
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
            var appId = ParseSteamAppId();
            var profileMenu = new ToolStripMenuItem(profile.Name, new ProfileIcon(profile).ToBitmap(16, 16));
            profileMenu.DropDownItems.Add(new ToolStripMenuItem(Language.Run, null,
                (sender, args) =>
                    HeliosPlus.OpenSteamGame(HeliosStartupAction.ChangeProfile, profile,
                        appId)));


            return profileMenu;
        }

        private uint ParseSteamAppId()
        {
            try
            {
                var fileAddress = SelectedItemPaths.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(fileAddress) &&
                    File.Exists(fileAddress) &&
                    new FileInfo(fileAddress).Length < 1024)
                {
                    var fileContent = File.ReadAllText(fileAddress);

                    if (!fileContent.Contains(@"[InternetShortcut]"))
                    {
                        return 0;
                    }

                    var steamUrlPattern = @"steam://rungameid/";
                    var urlIndex = fileContent.IndexOf(steamUrlPattern, StringComparison.InvariantCultureIgnoreCase);

                    if (urlIndex < 0)
                    {
                        return 0;
                    }

                    var nextLine = fileContent.IndexOf(@"\r", urlIndex + steamUrlPattern.Length,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (nextLine < 0)
                    {
                        nextLine = fileContent.Length - 1;
                    }

                    var appIdString = fileContent.Substring(urlIndex + steamUrlPattern.Length,
                        nextLine - urlIndex - steamUrlPattern.Length);
                    uint appId;

                    if (uint.TryParse(appIdString, out appId))
                    {
                        return appId;
                    }
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