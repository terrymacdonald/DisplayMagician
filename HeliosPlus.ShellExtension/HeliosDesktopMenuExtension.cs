using System.Linq;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System;
using HeliosPlus.ShellExtension.Resources;
using HeliosPlus;
using SharpShell.Attributes;
using SharpShell.SharpContextMenu;
using SharpShell.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace HeliosPlus.ShellExtension
{
    [ComVisible(true)]
    [COMServerAssociation(AssociationType.DesktopBackground)]
    [Guid("346e3285-43ca-45bc-8b33-1d4cdfe32e00")]
    public class HeliosDesktopMenuExtension : SharpContextMenu
    {
        // Other constants that are useful
        internal static Version _version = new Version(1, 0, 0);
        internal static string AlternateAppHomePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "HeliosPlus");
        internal static string AppDataPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HeliosPlus");
        private static string AppProfileStoragePath = System.IO.Path.Combine(AppDataPath, $"Profiles");
        private static string _profileStorageJsonFileName = System.IO.Path.Combine(AppProfileStoragePath, $"DisplayProfiles_{_version.ToString(2)}.json");
        internal static string registryHeliosPlus = @"SOFTWARE\HeliosPlus";
        string heliosPlusFullname = "";
        string heliosPlusInstallDir = "";
        Process heliosPlusProcess = null;

        public HeliosDesktopMenuExtension()
        { }

        protected override bool CanShowMenu()
        {
            // Only show this menu if HeliosPlus is installed
            heliosPlusInstallDir = "";
            try
            {
                RegistryKey heliosPlusKey = Registry.LocalMachine.OpenSubKey(registryHeliosPlus, RegistryKeyPermissionCheck.ReadSubTree);
                heliosPlusInstallDir = heliosPlusKey.GetValue("InstallDir", AlternateAppHomePath).ToString();
            }
            catch (Exception)
            {
                heliosPlusInstallDir = AlternateAppHomePath;
            }

            heliosPlusFullname = Path.Combine(heliosPlusInstallDir, "HeliosPlus.exe");


            if (File.Exists(heliosPlusFullname))
                return true;
            else
                return false;
        }

        protected override ContextMenuStrip CreateMenu()
        {

            var explorerMenuStrip = new ContextMenuStrip();

            if (File.Exists(heliosPlusFullname))
            {

                var extensionMenu = new ToolStripMenuItem("HeliosPlus: Change display profiles...", Properties.Resources.HeliosPlusMenuImage);
                explorerMenuStrip.Items.Add(extensionMenu);

                Dictionary<string, string> profiles = new Dictionary<string, string>();

                if (File.Exists(_profileStorageJsonFileName))
                {
                    MatchCollection mc;
                    string uuid = "";
                    string profileName = "";

                    foreach (string aLine in File.ReadLines(_profileStorageJsonFileName, Encoding.Unicode))
                    {
                        string lineToProcess = aLine;
                        if (lineToProcess.StartsWith("    \"UUID\""))
                        {
                            mc = Regex.Matches(lineToProcess, "    \"UUID\": \"(.*)\"");
                            uuid = mc[0].Groups[1].ToString();
                        }
                        else if (lineToProcess.StartsWith("    \"Name\""))
                        {
                            mc = Regex.Matches(lineToProcess, "    \"Name\": \"(.*)\"");
                            profileName = mc[0].Groups[1].ToString();
                            if (!uuid.Equals(""))
                                profiles.Add(profileName, uuid);
                        }

                    }

                }

                if (profiles.Count > 0)
                {

                    foreach (KeyValuePair<string, string> pair in profiles.OrderBy(key => key.Key))
                    {
                        extensionMenu.DropDownItems.Add(new ToolStripMenuItem(pair.Key, null,
                            (sender, args) =>
                            {
                                Logging.Log(heliosPlusFullname + $" ChangeProfile \"{pair.Value}\"");
                                heliosPlusProcess = Process.Start(heliosPlusFullname,$"ChangeProfile \"{pair.Value}\"");
                                Logging.Log(heliosPlusProcess.ToString());
                            }
                        ));
                    }
                }
            }

            return explorerMenuStrip;
        }
    }
}